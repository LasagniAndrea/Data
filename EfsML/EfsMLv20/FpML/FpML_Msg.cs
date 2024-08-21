#region Using Directives
using System;
using System.Reflection;

using EFS.GUI.Interface;
using EFS.GUI.Attributes;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Fx;
using FpML.v42.Ird;
using FpML.v42.Eqd;
using FpML.v42.Eqs;
using FpML.v42.Cd;
using FpML.v42.Shared;
#endregion Using Directives


namespace FpML.v42.Msg
{
    #region AcceptQuote
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>trade (one or more occurrences; of the type Trade)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AcceptQuote : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public Trade[] trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion AcceptQuote
    #region AdditionalData 
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AdditionalData
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string additionalDataScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion AdditionalData

    #region BestFitTrade
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier)</newpara>
    /// <newpara>differences (zero or more occurrences; of the type TradeDifference)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: TradeMismatched</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class BestFitTrade
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("differences")]
        public TradeDifference[] differences;
    }
    #endregion BestFitTrade

    #region CancelTradeConfirmation
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message requesting that a previously requested
    ///  TradeConfirmation process be cancelled.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>partyTradeIdentifier (exactly one occurrence; of the type PartyTradeIdentifier) The trade reference
    /// identifier(s) allocated to the trade by the parties involved.</newpara>
    /// <newpara>party (exactly one occurrence; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CancelTradeConfirmation : RequestMessage
    {
        public PartyTradeIdentifier partyTradeIdentifier;
        public Party party;
    }
    #endregion CancelTradeConfirmation
    #region CancelTradeMatch
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message requesting that a previously requested 
    /// TradeMatch process be cancelled.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>partyTradeIdentifier (exactly one occurrence; of the type PartyTradeIdentifier) The trade reference
    /// identifier(s) allocated to the trade by the parties involved.</newpara>
    /// <newpara>party (exactly one occurrence; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CancelTradeMatch : RequestMessage
    {
        public PartyTradeIdentifier partyTradeIdentifier;
        public Party party;
    }
    #endregion CancelTradeMatch
    #region ConfirmationCancelled
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for the message generated in reponse to a CancelConfirmation 
    /// request under normal circumstances.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>party (exactly one occurrence; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ConfirmationCancelled : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion ConfirmationCancelled
    #region ConfirmTrade
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message that indicates acceptance 
    /// of a previously matched trade.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>partyTradeIdentifier (exactly one occurrence; of the type PartyTradeIdentifier) The trade reference
    /// identifier(s) allocated to the trade by the parties involved.</newpara>
    /// <newpara>party (exactly one occurrence; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ConfirmTrade : RequestMessage
    {
        public PartyTradeIdentifier partyTradeIdentifier;
        public Party party;
    }
    #endregion
    #region ConversationId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ConversationId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string conversationIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ConversationId

    #region Location
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Location
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "normalizedString")]
        public string locationType;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion Location

    #region Message
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResponseMessage))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationResponse))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseResponse))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentResponse))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRefused))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentGranted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadySubmitted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuoteResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteUpdated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAlreadyExpired))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAcceptanceConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AcceptQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNotFound))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestMessage))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestValuationReport))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTerminationConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestNovationConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestIncreaseConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAmendmentConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAllocation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovateTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NotificationMessage))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationReport))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmended))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TerminationConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationStatusNotification))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationMatched))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationAlleged))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(IncreaseConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventNotification))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AmendmentConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCancelled))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationAmended))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeUnmatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMismatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRejected))]
    public abstract class Message : Document
    {
        public MessageHeader header;
        [System.Xml.Serialization.XmlElementAttribute("validation")]
        public Validation[] validation;
    }
    #endregion Message
    #region MessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestMessageHeader))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResponseMessageHeader))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NotificationMessageHeader))]
    public class MessageHeader
    {
        public ConversationId conversationId;
        public MessageId messageId;
        public MessageId inReplyTo;
        public PartyId sentBy;
        [System.Xml.Serialization.XmlElementAttribute("sendTo")]
        public PartyId[] sendTo;
        [System.Xml.Serialization.XmlElementAttribute("copyTo")]
        public PartyId[] copyTo;
        public System.DateTime creationTimestamp;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expiryTimestampSpecified;
        public System.DateTime expiryTimestamp;
    }
    #endregion MessageHeader
    #region MessageId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MessageId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string messageIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion MessageId
    #region MessageRejected
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MessageRejected : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("reason")]
        public Reason[] reason;
        public AdditionalData additionalData;
    }
    #endregion MessageRejected
    #region ModifyTradeConfirmation
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message requesting that the details of a 
    /// trade previously sent for confirmation be changed.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade) The root element in an FpML trade document.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ModifyTradeConfirmation : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ModifyTradeConfirmation
    #region ModifyTradeMatch
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content of a message requesting that the details of a trade 
    /// previously sent for matching be modified.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade) The root element in an FpML trade document.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ModifyTradeMatch : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ModifyTradeMatch

    #region NotificationMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(ValuationReport))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNovated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmended))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TerminationConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationStatusNotification))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationMatched))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationAlleged))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(IncreaseConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditEventNotification))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AmendmentConfirmed))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCancelled))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationAmended))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(AllocationCreated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeUnmatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMismatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MessageRejected))]
    public class NotificationMessage : Message { }
    #endregion NotificationMessage
    #region NotificationMessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class NotificationMessageHeader : MessageHeader { }
    #endregion NotificationMessageHeader

    #region QuotableFXLeg
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type QuotableProduct)</newpara>
    /// <newpara>exchangedCurrency (zero or one occurrence; of the type QuotablePayment)</newpara>
    /// <newpara>exchangeRate (exactly one occurrence; of the type QuotableFXRate)</newpara>
    /// <newpara>nonDeliverableForward (zero or one occurrence; of the type FXCashSettlement) Used to describe a
    /// particular type of FX forward transaction that is settled in a single currency.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Element: quotableFxSingleLeg</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("quotableFxSingleLeg", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class QuotableFXLeg : QuotableProduct
    {
        public QuotablePayment exchangedCurrency;
        public QuotableFXRate exchangeRate;
        public FxCashSettlement nonDeliverableForward;
    }
    #endregion QuotableFXLeg
    #region QuotableFXRate
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: QuotableFXLeg</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QuotableFXRate
    {
        public QuotedCurrencyPair quotedCurrencyPair;
    }
    #endregion QuotableFXRate
    #region QuotablePayment
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining payments</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>payerPartyReference (zero or one occurrence; with locally defined content) A pointer style reference to a
    /// party identifier defined elsewhere in the document.</newpara>
    /// <newpara>receiverPartyReference (zero or one occurrence; with locally defined content) A pointer style reference to a
    /// party identifier defined elsewhere in the document.</newpara>
    /// <newpara>paymentAmount (exactly one occurrence; of the type Money) The currency amount of the payment.</newpara>
    /// <newpara>paymentDate (zero or one occurrence; of the type AdjustableDate) The payment date. This date is subject to
    /// adjustment in accordance with any applicable business day convention.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: QuotableFXLeg</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QuotablePayment
    {
        public PartyReference payerPartyReference;
        public PartyReference receiverPartyReference;
        public Money paymentAmount;
        public AdjustableDate paymentDate;
    }
    #endregion QuotablePayment
    #region QuotableProduct
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>productType (zero or one occurrence; of the type ProductType)</newpara>
    /// <newpara>productId (zero or more occurrences; of the type ProductId)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Element: quotableProduct</newpara>
    ///<newpara>• Complex type: QuotableFXLeg</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///<newpara>• Complex type: QuotableFXLeg</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuotableFXLeg))]
    [System.Xml.Serialization.XmlRootAttribute("quotableProduct", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class QuotableProduct
    {
        public ProductType productType;
        [System.Xml.Serialization.XmlElementAttribute("productId")]
        public ProductId[] productId;
    }
    #endregion QuotableProduct
    #region QuoteAcceptanceConfirmed
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>trade (one or more occurrences; of the type Trade)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QuoteAcceptanceConfirmed : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("trade")]
        public Trade[] trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion QuoteAcceptanceConfirmed
    #region QuoteAlreadyExpired
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QuoteAlreadyExpired : ResponseMessage { }
    #endregion QuoteAlreadyExpired
    #region QuoteUpdated
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>quotableProduct (one or more occurrences; of the type QuotableProduct)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QuoteUpdated : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("quotableProduct")]
        public QuotableProduct[] quotableProduct;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion QuoteUpdated

    #region Reason
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Reason
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "token")]
        public string reasonCode;
        public Location location;
        public string description;
        public ValidationRuleId validationRuleId;
        [System.Xml.Serialization.XmlElementAttribute("additionalData")]
        public AdditionalData[] additionalData;
    }
    #endregion Reason
    #region RequestMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestValuationReport))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTerminationConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestNovationConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestIncreaseConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAmendmentConfirmation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestAllocation))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRequest))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovateTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ModifyTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeMatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CancelTradeConfirmation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestTradeStatus))]
    public abstract class RequestMessage : Message { }
    #endregion RequestMessage
    #region RequestMessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RequestMessageHeader : MessageHeader { }
    #endregion RequestMessageHeader
    #region RequestQuote
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>quotableProduct (one or more occurrences; of the type QuotableProduct)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RequestQuote : RequestMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("quotableProduct")]
        public QuotableProduct[] quotableProduct;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestQuote
    #region RequestQuoteResponse
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>product (one or more occurrences; of the type Product) An abstract element used as a place holder for the
    /// substituting product elements.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RequestQuoteResponse : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(BulletPayment))]
        [System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(CapFloor))]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwap", typeof(CreditDefaultSwap))]
        [System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EquityOption))]
        [System.Xml.Serialization.XmlElementAttribute("equitySwap", typeof(EquitySwap))]
        [System.Xml.Serialization.XmlElementAttribute("fra", typeof(Fra))]
        [System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(FxAverageRateOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(FxBarrierOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(FxDigitalOption))]
        [System.Xml.Serialization.XmlElementAttribute("fxSimpleOption", typeof(FxOptionLeg))]
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(FxLeg))]
        [System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(FxSwap))]
        [System.Xml.Serialization.XmlElementAttribute("swap", typeof(Swap))]
        [System.Xml.Serialization.XmlElementAttribute("swaption", typeof(Swaption))]
        [System.Xml.Serialization.XmlElementAttribute("strategy", typeof(Strategy))]
        [System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(TermDeposit))]
        public Product Item;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }

    #endregion RequestQuoteResponse
    #region RequestTradeConfirmation
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message requesting that the contained trade 
    /// be put forward for matching and confirmation.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade) The root element in an FpML trade document.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RequestTradeConfirmation : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTradeConfirmation
    #region RequestTradeMatch
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message requesting that the contained trade 
    /// be put forward for matching.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type RequestMessage)</newpara>
    /// <newpara>• A type defining the basic content of a message that requests the receiver to perform some
    /// business operation determined by the message type and its content.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade) The root element in an FpML trade document.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RequestTradeMatch : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTradeMatch
    #region RequestTradeStatus
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RequestTradeStatus : RequestMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public TradeIdentifier[] tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTradeStatus
    #region ResponseMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeTerminationResponse))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeIncreaseResponse))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAmendmentResponse))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRefused))]
    //[System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentGranted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadySubmitted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAlreadyMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAffirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConfirmationCancelled))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestQuoteResponse))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteUpdated))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAlreadyExpired))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(QuoteAcceptanceConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AcceptQuote))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeStatus))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNotFound))]
    public class ResponseMessage : Message { }
    #endregion ResponseMessage
    #region ResponseMessageHeader
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ResponseMessageHeader : MessageHeader { }
    #endregion ResponseMessageHeader

    #region TradeAffirmation
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message that indicates that a trade 
    /// is considered affirmed by the sender.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade) The root element in an FpML trade document.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeAffirmation : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        [ControlGUI(Name = "party", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public Party[] party;
    }
    #endregion TradeAffirmation
    #region TradeAffirmed
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message generated when a party confirms 
    /// that a trade is affirmed.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>tradeIdentifier (one or more occurrences; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeAffirmed : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public TradeIdentifier[] tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAffirmed
    #region TradeAlleged
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message sent by a confirmation provider 
    /// when it believes that one party has been tardy in providing its side of a transaction.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>bestFitTradeId (zero or more occurrences; of the type TradeIdentifier) A trade identifier for a transaction that
    /// closely resembles the characteristics of the trade under consideration.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeAlleged : NotificationMessage
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("bestFitTradeId")]
        public TradeIdentifier[] bestFitTradeId;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAlleged
    #region TradeAlreadyMatched
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message sent by a confirmation provider when 
    /// it believes that one party has repeated a request to confirm a trade.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>party (exactly one occurrence; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeAlreadyMatched : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion TradeAlreadyMatched
    #region TradeAlreadySubmitted
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message sent by a confirmation provider 
    /// when it believes that one party has repeated a request to confirm a trade.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type ResponseMessage)</newpara>
    /// <newpara>• A type refining the generic message content model to make it specific to response messages.</newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>party (exactly one occurrence; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeAlreadySubmitted : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion TradeAlreadySubmitted
    #region TradeAmended
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeAmended : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAmended
    #region TradeCancelled
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>tradeIdentifier (one or more occurrences; of the type xsd:string)</newpara>
    /// <newpara>party (one or more occurrences; of the type xsd:string)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeCancelled : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public string[] tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public string[] party;
    }
    #endregion TradeCancelled
    #region TradeConfirmed
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model of a message generated when a trade 
    /// is determined to be confirmed.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade) The root element in an FpML trade document.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeConfirmed : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeConfirmed
    #region TradeCreated
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>trade (exactly one occurrence; of the type Trade)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeCreated : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeCreated
    #region TradeDifference
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>differenceType (exactly one occurrence; of the type DifferenceTypeEnum)</newpara>
    /// <newpara>differenceSeverity (exactly one occurrence; of the type DifferenceSeverityEnum)</newpara>
    /// <newpara>element (exactly one occurrence; of the type xsd:string)</newpara>
    /// <newpara>basePath (zero or one occurrence; of the type xsd:string)</newpara>
    /// <newpara>baseValue (zero or one occurrence; of the type xsd:string)</newpara>
    /// <newpara>otherPath (zero or one occurrence; of the type xsd:string)</newpara>
    /// <newpara>otherValue (zero or one occurrence; of the type xsd:string)</newpara>
    /// <newpara>missingElement (zero or more occurrences; of the type xsd:string)</newpara>
    /// <newpara>extraElement (zero or more occurrences; of the type xsd:string)</newpara>
    /// <newpara>message (exactly one occurrence; of the type xsd:string)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: BestFitTrade</newpara>
    ///<newpara>• Complex type: TradeMatched</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeDifference
    {
        public DifferenceTypeEnum differenceType;
        public DifferenceSeverityEnum differenceSeverity;
        public string element;
        public string basePath;
        public string baseValue;
        public string otherPath;
        public string otherValue;
        [System.Xml.Serialization.XmlElementAttribute("missingElement")]
        public string[] missingElement;
        [System.Xml.Serialization.XmlElementAttribute("extraElement")]
        public string[] extraElement;
        public string message;
    }
    #endregion TradeDifference
    #region TradeMatched
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model for a message indicating that a correlation 
    /// has been made between two transactions.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>tradeIdentifier (one or more occurrences; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>differences (zero or more occurrences; of the type TradeDifference)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeMatched : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public TradeIdentifier[] tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("differences")]
        public TradeDifference[] differences;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeMatched
    #region TradeMismatched
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model of a message generated when a trade 
    /// is determined to be mismatched.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>bestFitTrade (zero or more occurrences; of the type BestFitTrade)</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeMismatched : NotificationMessage
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("bestFitTrade")]
        public BestFitTrade[] bestFitTrade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeMismatched
    #region TradeNotFound
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeNotFound : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion TradeNotFound
    #region TradeStatus
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeStatus : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeStatusItem")]
        public TradeStatusItem[] tradeStatusItem;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeStatus
    #region TradeStatusItem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeStatusItem
    {
        public TradeIdentifier tradeIdentifier;
        public TradeStatusValue tradeStatusValue;
    }
    #endregion TradeStatusItem
    #region TradeStatusValue
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeStatusValue
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string tradeStatusScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion TradeStatusValue
    #region TradeUnmatched
    /// <summary>
    /// <newpara><b>Description :</b> A type defining the content model of a message generated when a trade 
    /// is determined to be unmatched.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type NotificationMessage)</newpara>
    /// <newpara>• A type defining the basic content for a message sent to inform another system that some
    /// 'business event' has occured. Notifications are not expected to be replied to.</newpara>
    /// <newpara>tradeIdentifier (exactly one occurrence; of the type TradeIdentifier) An instance of a unique trade identifier.</newpara>
    /// <newpara>bestFitTradeId (zero or more occurrences; of the type TradeIdentifier) A trade identifier for a transaction that
    /// closely resembles the characteristics of the trade under consideration.</newpara>
    /// <newpara>party (one or more occurrences; of the type Party) The parties obligated to make payments from time to time
    /// during the term of the trade. This will include, at a minimum, the principal parties involved in the swap or
    /// forward rate agreement. Other parties paying or receiving fees, commissions etc. must also be specified if
    /// referenced in other party payments.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeUnmatched : NotificationMessage
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("bestFitTradeId")]
        public TradeIdentifier[] bestFitTradeId;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeUnmatched

    #region ValidationRuleId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ValidationRuleId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string validationScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion Validation
}

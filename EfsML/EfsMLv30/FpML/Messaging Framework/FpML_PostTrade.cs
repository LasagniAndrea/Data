#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Doc;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Matching.Status.ToDefine;
using FpML.v44.Msg.ToDefine;
using FpML.v44.PostTrade.Confirmation.ToDefine;
using FpML.v44.PostTrade.Execution.ToDefine;
using FpML.v44.PostTrade.Negotiation.ToDefine;
using FpML.v44.Shared;

#endregion using directives


namespace FpML.v44.PostTrade.ToDefine
{
    #region AffectedTransactions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AffectedTransactions
    {
        [System.Xml.Serialization.XmlElementAttribute("trade", typeof(Trade),Order=1)]
        [System.Xml.Serialization.XmlElementAttribute("tradeReference", typeof(PartyTradeIdentifier[]), Order = 1)]
        public object Item;
    }
    #endregion AffectedTransactions

    #region Novation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Novation : Event
    {
		/*
        [System.Xml.Serialization.XmlElementAttribute("oldTransactionReference", typeof(PartyTradeIdentifier[]))]
        [System.Xml.Serialization.XmlElementAttribute("newTransaction", typeof(Trade))]
        [System.Xml.Serialization.XmlElementAttribute("oldTransaction", typeof(Trade))]
        [System.Xml.Serialization.XmlElementAttribute("newTransactionReference", typeof(PartyTradeIdentifier[]))]
        public object Item;
        public PartyReference transferor;
        public PartyReference transferee;
        public PartyReference remainingParty;
        public PartyReference otherRemainingParty;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime novationDate;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime novationTradeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool novationTradeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("novatedNumberOfOptions", typeof(System.Decimal))]
        [System.Xml.Serialization.XmlElementAttribute("novatedAmount", typeof(Money))]
        public object Item1;
        public Trade remainingTrade;
        public bool fullFirstCalculationPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fullFirstCalculationPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("firstPeriodStartDate")]
        public FirstPeriodStartDate[] firstPeriodStartDate;
        public Empty nonReliance;
        public CreditDerivativesNotices creditDerivativesNotices;
        [System.Xml.Serialization.XmlElementAttribute("contractualDefinitions")]
        public ContractualDefinitions[] contractualDefinitions;
        [System.Xml.Serialization.XmlElementAttribute("contractualSupplement", typeof(ContractualSupplement))]
        [System.Xml.Serialization.XmlElementAttribute("contractualTermsSupplement", typeof(ContractualTermsSupplement))]
        public object[] Items;
        public Payment payment;
		*/
    }
    #endregion Novation
    #region NovationNotificationMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConfirmed))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationAlleged))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationMatched))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeNovated))]
    public abstract class NovationNotificationMessage : NotificationMessage
    {
        public Novation novation;
        [System.Xml.Serialization.XmlElementAttribute("party",Order=1)]
        public Party[] party;
    }
    #endregion NovationNotificationMessage
    #region NovationRequestMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRequest))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovateTrade))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RequestNovationConfirmation))]
    public abstract class NovationRequestMessage : RequestMessage
    {
        public Novation novation;
        [System.Xml.Serialization.XmlElementAttribute("party",Order=1)]
        public Party[] party;
    }
    #endregion NovationRequestMessage
    #region NovationResponseMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentGranted))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NovationConsentRefused))]
    public abstract class NovationResponseMessage : ResponseMessage
    {
        public Novation novation;
        [System.Xml.Serialization.XmlElementAttribute("party",Order=1)]
        public Party[] party;
    }
    #endregion NovationResponseMessage

    #region PartialTerminationAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PartialTerminationAmount
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Partial termination informations")]
		public EFS_RadioChoice detail;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailDecreaseInNotionalAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("decreaseInNotionalAmount", typeof(Money),Order=1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money detailDecreaseInNotionalAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailOutstandingNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("outstandingNotionalAmount", typeof(Money), Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Money detailOutstandingNotionalAmount;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailDecreaseInNumberOfOptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("decreaseInNumberOfOptions", typeof(EFS_Decimal), Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal detailDecreaseInNumberOfOptions;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool detailOutstandingNumberOfOptionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("outstandingNumberOfOptions", typeof(EFS_Decimal), Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public EFS_Decimal detailOutstandingNumberOfOptions;
		#endregion Members
	}
    #endregion PartialTerminationAmount

    #region Termination
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Termination : Event
    {
        [System.Xml.Serialization.XmlElementAttribute("trade", Order = 1)]
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier", Order = 2)]
        public PartyTradeIdentifier[] tradeReference;
        [System.Xml.Serialization.XmlElementAttribute("terminationTradeDate", Order = 3)]
        public System.DateTime terminationTradeDate;
        [System.Xml.Serialization.XmlElementAttribute("terminationEffectiveDate", Order = 4)]
        public System.DateTime terminationEffectiveDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Termination type")]
		public EFS_RadioChoice type;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typePartialSpecified;
		[System.Xml.Serialization.XmlElementAttribute("partial", typeof(PartialTerminationAmount),Order=5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public PartialTerminationAmount typePartial;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool typeFullSpecified;
		[System.Xml.Serialization.XmlElementAttribute("full", typeof(Empty),Order=6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public Empty typeFull;
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 7)]
        public Payment payment;
    }
    #endregion Termination
    #region TradeAmendment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAmendment
    {
        [System.Xml.Serialization.XmlElementAttribute("originalTrade", typeof(Trade), Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("originalTradeIdentifier", typeof(PartyTradeIdentifier), Order = 1)]
        public object[] Items;
        [System.Xml.Serialization.XmlElementAttribute("amendedTrade", Order = 2)]
        public Trade amendedTrade;
    }
    #endregion TradeAmendment

}

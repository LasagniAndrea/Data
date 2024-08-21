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
using FpML.v44.Msg.ToDefine;
using FpML.v44.Shared;
#endregion using directives


namespace FpML.v44.PreTrade.ToDefine
{
	#region AcceptQuote
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class AcceptQuote : ResponseMessage
	{
		[System.Xml.Serialization.XmlElementAttribute("trade")]
		public Trade[] trade;
		[System.Xml.Serialization.XmlElementAttribute("party")]
		public Party[] party;
	}
	#endregion AcceptQuote

	#region QuotableFxLeg
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	[System.Xml.Serialization.XmlRootAttribute("quotableFxSingleLeg", Namespace="http://www.fpml.org/2007/FpML-4-4", IsNullable=false)]
	public class QuotableFxLeg : QuotableProduct 
	{
		public QuotablePayment exchangedCurrency;
		public QuotableFxRate exchangeRate;
		public FxCashSettlement nonDeliverableForward;
	}
	#endregion QuotableFxLeg
	#region QuotableFxRate
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class QuotableFxRate
	{
		public QuotedCurrencyPair quotedCurrencyPair;
	}
	#endregion QuotableFxRate    
	#region QuotablePayment
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class QuotablePayment
	{
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
		public Money paymentAmount;
		public AdjustableDate paymentDate;
	}
	#endregion QuotablePayment
	#region QuotableProduct
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(QuotableFxLeg))]
	public abstract class QuotableProduct
	{
		[System.Xml.Serialization.XmlElementAttribute("productType")]
		public ProductType[] productType;
		[System.Xml.Serialization.XmlElementAttribute("productId")]
		public ProductId[] productId;
	}
	#endregion QuotableProduct
	#region QuoteAcceptanceConfirmed
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class QuoteAcceptanceConfirmed : ResponseMessage
	{
		[System.Xml.Serialization.XmlElementAttribute("trade")]
		public Trade[] trade;
		[System.Xml.Serialization.XmlElementAttribute("party")]
		public Party[] party;
	}
	#endregion QuoteAcceptanceConfirmed
	#region QuoteAlreadyExpired
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class QuoteAlreadyExpired : ResponseMessage {}
	#endregion QuoteAlreadyExpired
	#region QuoteUpdated
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class QuoteUpdated : ResponseMessage 
	{
		public object Item;
		[System.Xml.Serialization.XmlElementAttribute("party")]
		public Party[] party;
	}
	#endregion QuoteUpdated

	#region RequestQuote
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class RequestQuote : RequestMessage
	{
		public object Item;
		[System.Xml.Serialization.XmlElementAttribute("party")]
		public Party[] party;
	}
	#endregion RequestQuote
	#region RequestQuoteResponse
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2007/FpML-4-4")]
	public class RequestQuoteResponse : ResponseMessage
	{
		public object Item;
		[System.Xml.Serialization.XmlElementAttribute("party")]
		public Party[] party;
	}
	#endregion RequestQuoteResponse
}

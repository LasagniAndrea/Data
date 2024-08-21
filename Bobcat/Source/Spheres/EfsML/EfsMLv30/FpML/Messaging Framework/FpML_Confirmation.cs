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


namespace FpML.v44.Confirmation.ToDefine
{
    #region CancelTradeConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CancelTradeConfirmation : RequestMessage
    {
        public PartyTradeIdentifier partyTradeIdentifier;
        public Party party;
    }
    #endregion CancelTradeConfirmation
    #region ConfirmTrade
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ConfirmTrade : RequestMessage
    {
        public PartyTradeIdentifier partyTradeIdentifier;
        public Party party;
    }
    #endregion ConfirmTrade
    #region ConfirmationCancelled
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ConfirmationCancelled : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion ConfirmationCancelled

    #region ModifyTradeConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ModifyTradeConfirmation : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ModifyTradeConfirmation

    #region RequestTradeConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestTradeConfirmation : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTradeConfirmation

    #region TradeAffirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAffirmation : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAffirmation
    #region TradeAffirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAffirmed : ResponseMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("tradeIdentifier")]
        public TradeIdentifier[] tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAffirmed
    #region TradeAlreadyAffirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlreadyAffirmed : TradeErrorResponse { }
    #endregion TradeAlreadyAffirmed
    #region TradeAlreadyConfirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlreadyConfirmed : TradeErrorResponse { }
    #endregion TradeAlreadyConfirmed
    #region TradeConfirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeConfirmed : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeConfirmed
}

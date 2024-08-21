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

// EG 20140702 New File
namespace FpML.v44.TradeRequest.ToDefine
{
    #region CancelTradeMatch
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CancelTradeMatch : RequestMessage
    {
        public PartyTradeIdentifier partyTradeIdentifier;
        public Party party;
    }
    #endregion CancelTradeMatch

    #region ModifyTradeMatch
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ModifyTradeMatch : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ModifyTradeMatch

    #region RequestTradeMatch
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestTradeMatch : RequestMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTradeMatch

    #region TradeAlreadyMatched
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlreadyMatched : ResponseMessage
    {
        public TradeIdentifier tradeIdentifier;
        public Party party;
    }
    #endregion TradeAlreadyMatched

}

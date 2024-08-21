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
using FpML.v44.Shared;
using FpML.v44.Msg.ToDefine;
using FpML.v44.PostTrade.ToDefine;
#endregion using directives


namespace FpML.v44.Matching.Status.ToDefine
{
    #region NovationMatched
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovationMatched : NovationNotificationMessage { }
    #endregion NovationMatched

    #region TradeAlleged
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAlleged : NotificationMessage
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("bestFitTradeId")]
        public TradeIdentifier[] bestFitTradeId;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAlleged
    #region TradeMatched
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeMismatched : NotificationMessage
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("bestFitTrade")]
        public BestFitTrade[] bestFitTrade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeMismatched
    #region TradeUnmatched
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeUnmatched : NotificationMessage
    {
        public TradeIdentifier tradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("bestFitTradeId")]
        public TradeIdentifier[] bestFitTradeId;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeUnmatched
}

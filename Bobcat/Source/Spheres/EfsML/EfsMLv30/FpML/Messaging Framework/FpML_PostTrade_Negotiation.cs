#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Doc.ToDefine;
using FpML.v44.Msg.ToDefine;
using FpML.v44.PostTrade.ToDefine;
using FpML.v44.Shared;
#endregion using directives


namespace FpML.v44.PostTrade.Negotiation.ToDefine
{
    #region NovationConsentGranted
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovationConsentGranted : NovationResponseMessage { }
    #endregion NovationConsentGranted
    #region NovationConsentRefused
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovationConsentRefused : NovationResponseMessage
    {
        public Reason reason;
    }
    #endregion NovationConsentRefused

    #region NovationConsentRequest
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovationConsentRequest : NovationRequestMessage { }
    #endregion NovationConsentRequest

    #region TradeAmendmentRequest
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAmendmentRequest : RequestMessage
    {
        public Amendment amendment;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAmendmentRequest
    #region TradeAmendmentResponse
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeAmendmentResponse : ResponseMessage
    {
        public Amendment amendment;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeAmendmentResponse
    #region TradeIncreaseRequest
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeIncreaseRequest : RequestMessage
    {
        public Increase increase;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeIncreaseRequest
    #region TradeIncreaseResponse
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeIncreaseResponse : ResponseMessage
    {
        public Increase increase;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeIncreaseResponse
    #region TradeTerminationRequest
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeTerminationRequest : RequestMessage
    {
        public Termination termination;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeTerminationRequest
    #region TradeTerminationResponse
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeTerminationResponse : ResponseMessage
    {
        public Termination termination;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeTerminationResponse
}

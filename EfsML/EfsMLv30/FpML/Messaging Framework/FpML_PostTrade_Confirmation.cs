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


namespace FpML.v44.PostTrade.Confirmation.ToDefine
{
    #region AmendmentConfirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AmendmentConfirmed : NotificationMessage
    {
        public Amendment amendment;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion AmendmentConfirmed

    #region IncreaseConfirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class IncreaseConfirmed : NotificationMessage
    {
        public Increase increase;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion IncreaseConfirmed

    #region NovationAlleged
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovationAlleged : NovationNotificationMessage { }
    #endregion NovationAlleged
    #region NovationConfirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovationConfirmed : NovationNotificationMessage { }
    #endregion NovationConfirmed

    #region RequestAmendmentConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestAmendmentConfirmation : RequestMessage
    {
        public Amendment amendment;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestAmendmentConfirmation
    #region RequestIncreaseConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestIncreaseConfirmation : RequestMessage
    {
        public Increase increase;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestIncreaseConfirmation
    #region RequestNovationConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestNovationConfirmation : NovationRequestMessage { }
    #endregion RequestNovationConfirmation
    #region RequestTerminationConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestTerminationConfirmation : RequestMessage
    {
        public Termination termination;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestTerminationConfirmation

    #region TerminationConfirmed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TerminationConfirmed : NotificationMessage
    {
        public Termination termination;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TerminationConfirmed
}

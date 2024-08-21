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
using FpML.v44.Enum;
using FpML.v44.Msg.ToDefine;
using FpML.v44.PostTrade.ToDefine;
using FpML.v44.Shared;
#endregion using directives


namespace FpML.v44.CreditEvent.Notification.ToDefine
{
    #region BankruptcyEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("bankruptcy", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class BankruptcyEvent : CreditEvent { }
    #endregion BankruptcyEvent

    #region CreditEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ObligationDefaultEvent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FailureToPayEvent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BankruptcyEvent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ObligationAccelerationEvent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RepudiationMoratoriumEvent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RestructuringEvent))]
    public class CreditEvent { }
    #endregion CreditEvent
    #region CreditEventNoticeDocument
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("creditEventNotice", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class CreditEventNoticeDocument : Event
    {
		/*
        public AffectedTransactions affectedTransactions;
        public LegalEntity referenceEntity;
        public object Item;
        [System.Xml.Serialization.XmlElementAttribute("publiclyAvailableInformation")]
        public Resource[] publiclyAvailableInformation;
        public PartyReference notifyingPartyReference;
        public PartyReference notifiedPartyReference;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime creditEventNoticeDate;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime creditEventDate;
		*/
    }
    #endregion CreditEventNoticeDocument
    #region CreditEventNotification
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CreditEventNotification : NotificationMessage
    {
        public CreditEventNoticeDocument creditEventNotice;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion CreditEventNotification

    #region FailureToPayEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("failureToPay", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class FailureToPayEvent : CreditEvent { }
    #endregion FailureToPayEvent

    #region Language
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Language
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string languageScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion Language

    #region ObligationAccelerationEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("obligationAcceleration", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class ObligationAccelerationEvent : CreditEvent { }
    #endregion ObligationAccelerationEvent
    #region ObligationDefaultEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("obligationDefault", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class ObligationDefaultEvent : CreditEvent { }
    #endregion ObligationDefaultEvent

    #region RepudiationMoratoriumEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("repudiationMoratorium", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class RepudiationMoratoriumEvent : CreditEvent { }
    #endregion RepudiationMoratoriumEvent
    #region Resource
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Resource
    {
        public ResourceId resourceId;
        public Language language;
        public System.Decimal sizeInBytes;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sizeInBytesSpecified;
        public ResourceLength length;
        public MimeType mimeType;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString")]
        public string name;
        public string comments;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "anyURI")]
        public string url;
    }
    #endregion Resource
    #region ResourceId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ResourceId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string resourceIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ResourceId
    #region ResourceLength
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ResourceLength
    {
        public LengthUnitEnum lengthUnit;
        public System.Decimal lengthValue;
    }
    #endregion ResourceLength
    #region RestructuringEvent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("restructuring", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class RestructuringEvent : CreditEvent
    {
        public Money partialExerciseAmount;
    }
    #endregion RestructuringEvent
}

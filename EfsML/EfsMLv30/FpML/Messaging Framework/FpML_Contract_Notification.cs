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
using FpML.v44.Msg.ToDefine;
using FpML.v44.Shared;
#endregion using directives


namespace FpML.v44.ContractNotification.ToDefine
{
    #region ContractCancelled
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractCancelled : ContractReferenceMessage { }
    #endregion ContractCancelled
    #region ContractCreated
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractCreated : NotificationMessage
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("partyTradeIdentifier", IsNullable = false)]
        public PartyTradeIdentifier[] tradeReference;
        public Contract contract;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ContractCreated
    #region ContractFullTermination
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractFullTermination : NotificationMessage
    {
        public ContractTermination termination;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ContractFullTermination
    #region ContractIncreased
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractIncreased : NotificationMessage
    {
        public ChangeContractSize increase;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ContractIncreased
    #region ContractNovated
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractNovated : NotificationMessage
    {
        public ContractNovation novation;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ContractNovated
    #region ContractPartialTermination
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ContractPartialTermination : NotificationMessage
    {
        public ChangeContractSize termination;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ContractPartialTermination
    #region ContractReferenceMessage
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ContractCancelled))]
    public abstract class ContractReferenceMessage : NotificationMessage
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("identifier", IsNullable = false)]
        public ContractIdentifier[] contractReference;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion ContractReferenceMessage
}

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
using FpML.v44.PostTrade.ToDefine;
using FpML.v44.Shared;
#endregion using directives


namespace FpML.v44.Allocation.ToDefine
{
    #region AllocationAmended
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AllocationAmended : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("amendment", Order = 1)]
        public TradeAmendment[] amendment;
        [System.Xml.Serialization.XmlElementAttribute("party", Order = 2)]
        public Party[] party;
    }
    #endregion AllocationAmended
    #region AllocationCancelled
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AllocationCancelled : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("trade", typeof(Trade), Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier", typeof(PartyTradeIdentifier), Order = 1)]
        public object[] Items;
        [System.Xml.Serialization.XmlElementAttribute("party", Order = 2 )]
        public Party[] party;
    }
    #endregion AllocationCancelled
    #region AllocationCreated
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AllocationCreated : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("trade",Order = 1)]
        public Trade[] trade;
        [System.Xml.Serialization.XmlElementAttribute("party", Order = 2)]
        public Party[] party;
    }
    #endregion AllocationCreated

    #region RequestAllocation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestAllocation : RequestMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("blockTradeIdentifier", Order = 1)]
        public BlockTradeIdentifier blockTradeIdentifier;
        [System.Xml.Serialization.XmlElementAttribute("allocations", Order = 1)]
        public Allocations allocations;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestAllocation
}

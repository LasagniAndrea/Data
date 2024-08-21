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

// EG 20140702 New build FpML4.4 TradeAmended DEPRECATED
// EG 20140702 New build FpML4.4 TradeCancelled DEPRECATED
// EG 20140702 New build FpML4.4 TradeCreated DEPRECATED
namespace FpML.v44.TradeNotification.ToDefine
{
    #region TradeExecution
    // EG 20140702 New build FpML4.4 New
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeExecution : NotificationMessage
    {
        public Trade trade;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion TradeExecution

}

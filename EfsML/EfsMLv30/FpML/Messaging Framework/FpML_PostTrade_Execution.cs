#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.PostTrade.ToDefine;
#endregion using directives


namespace FpML.v44.PostTrade.Execution.ToDefine
{
    #region NovateTrade
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class NovateTrade : NovationRequestMessage { }
    #endregion NovateTrade

    #region TradeNovated
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeNovated : NovationNotificationMessage { }
    #endregion TradeNovated
}

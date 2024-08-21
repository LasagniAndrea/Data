#region using directives
using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Riskdef;
using FpML.v44.Doc;
using FpML.v44.Doc.ToDefine;
using FpML.v44.Msg.ToDefine;
using FpML.v44.Shared;
using FpML.v44.ValuationResults.ToDefine;

#endregion using directives

namespace FpML.v44.Reporting.ToDefine
{
    #region PortfolioValuationItem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PortfolioValuationItem
    {
        public Portfolio portfolio;
        [System.Xml.Serialization.XmlElementAttribute("tradeValuationItem")]
        public TradeValuationItem[] tradeValuationItem;
        public ValuationSet valuationSet;
    }
    #endregion PortfolioValuationItem
    #region PositionReport
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionReport : NotificationMessage
    {
        public IdentifiedDate asOfDate;
        public string dataSetName;
        public QuotationCharacteristics quotationCharacteristics;
        [System.Xml.Serialization.XmlElementAttribute("position")]
        public Position[] position;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion PositionReport

    #region RequestPositionReport
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestPositionReport : RequestMessage
    {
        public object asOfDate;
        [System.Xml.Serialization.XmlElementAttribute("dataSetName", typeof(string), DataType = "normalizedString")]
        [System.Xml.Serialization.XmlElementAttribute("requestedPositions", typeof(RequestedPositions))]
        public object Item;
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
    }
    #endregion RequestPositionReport
    #region RequestValuationReport
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestValuationReport : RequestMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
        public Market market;
        [System.Xml.Serialization.XmlElementAttribute("portfolioValuationItem")]
        public PortfolioValuationItem[] portfolioValuationItem;
        [System.Xml.Serialization.XmlElementAttribute("tradeValuationItem")]
        public TradeValuationItem[] tradeValuationItem;
    }
    #endregion RequestValuationReport
    #region RequestedPositions
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class RequestedPositions
    {
        [System.Xml.Serialization.XmlElementAttribute("queryPortfolio")]
        public QueryPortfolio Item;
    }
    #endregion RequestedPositions

    #region TradeValuationItem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TradeValuationItem
    {
        [System.Xml.Serialization.XmlElementAttribute("trade", typeof(Trade))]
        [System.Xml.Serialization.XmlElementAttribute("partyTradeIdentifier", typeof(PartyTradeIdentifier))]
        public object[] Items;
        public ValuationSet valuationSet;
    }
    #endregion TradeValuationItem

    #region ValuationReport
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationReport : NotificationMessage
    {
        [System.Xml.Serialization.XmlElementAttribute("party")]
        public Party[] party;
        public Market market;
        [System.Xml.Serialization.XmlElementAttribute("portfolioValuationItem")]
        public PortfolioValuationItem[] portfolioValuationItem;
        [System.Xml.Serialization.XmlElementAttribute("tradeValuationItem")]
        public TradeValuationItem[] tradeValuationItem;
    }
    #endregion ValuationReport

}

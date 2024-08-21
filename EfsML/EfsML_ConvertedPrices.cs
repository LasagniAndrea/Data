using System.Linq;
using EfsML.Interface;
using FixML.Enum;

// 20120820 MF - Ticket 18073
/// <summary>
/// Converted values collection description. The values conversion is performed according with a specific base and format style specified
/// on the derivative contract referential related to the current asset.
/// </summary>
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
public class ConvertedPrices : IConvertedPrices
{
    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedStrikePriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted strike price. 
    /// The Strike has been converted from the default numerical base to the display numerical
    /// base of the derivative contract related to the current asset element. 
    /// Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedStrikePrice")]
    public string ConvertedStrikePrice { get; set; }

    /// <summary>
    /// Weighted average price structure
    /// </summary>
    public class WeightedAverageTradePrice
    {
        /// <summary>
        /// Get the weighted average of the current asset, average of a trade prices subset inside of the ConvertedTradePrices collection. 
        /// This value is always given in Base 100. 
        /// </summary>
        /// <remarks>
        /// weighted average means that each trade price enters the evaluation after multiplication with the current trade quantity.
        /// WeightedPrice(Tk) = LastPx(Tk) * LastQty(Tk), with Tk current trade.
        /// </remarks>
        public decimal PriceBase100 { get; set; }

        /// <summary>
        /// Get the number current quantity used to evaluate the current average PriceBase100.
        /// This value is incremented by the EvaluateWeightedAverage method.
        /// </summary>
        public decimal Quantity { get; set; }
    }

    /// <summary>
    /// Average price of all the trade inside of the ConvertedTradePrices collection with side 1.
    /// This value is evaluated by the EvaluateWeightedAverage method.
    /// </summary>
    [System.Xml.Serialization.XmlIgnore()]
    public WeightedAverageTradePrice LongAveragePrice = new WeightedAverageTradePrice();

    /// <summary>
    /// Average price of all the trade inside of the ConvertedTradePrices collection with side 2.
    /// This value is evaluated by the EvaluateWeightedAverage method.
    /// </summary>
    [System.Xml.Serialization.XmlIgnore()]
    public WeightedAverageTradePrice ShortAveragePrice = new WeightedAverageTradePrice();

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedLongAveragePriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted weighted average long price of a trade subset (netted by asset). 
    /// The long average price has been converted from the default numerical base to the display numerical
    /// base of the derivative contract related to the current asset element. 
    /// Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedLongAveragePrice")]
    public string ConvertedLongAveragePrice { get; set; }

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedShortAveragePriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted weighted average short price of a trade subset (netted by asset). 
    /// The long average price has been converted from the default numerical base to the display numerical
    /// base of the derivative contract related to the current asset element. 
    /// Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedShortAveragePrice")]
    public string ConvertedShortAveragePrice { get; set; }

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedSynthPositionPriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted weighted average price of a synthetic position (netting by asset).
    /// The average has been converted from the numerical base 100 to the display numerical
    /// base of the derivative contract related to the current asset element. Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedSynthPositionPrice")]
    public string ConvertedSynthPositionPrice { get; set; }

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedClearingPriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted closing price.
    /// The value has been converted from the numerical base 100 to the display numerical
    /// base of the derivative contract related to the current asset element. Moreover a derivative contract specific style has been applied.
    /// </summary>
    // RD 20140725 [20212] The serialization attribut is renamed in order to use this element for all reports: PosAction, Position, ...
    //[System.Xml.Serialization.XmlElement("convertedSynthPositionClearingPrice")]
    [System.Xml.Serialization.XmlElement("convertedClearingPrice")]
    public string ConvertedClearingPrice { get; set; }

    // RD 20140725 [20212] Add new element ConvertedSettltPrice
    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedSettltPriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted closing price.
    /// The value has been converted from the numerical base 100 to the display numerical
    /// base of the derivative contract related to the current asset element. Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedSettltPrice")]
    public string ConvertedSettltPrice { get; set; }

    // RD 20140725 [20212] Add new element ConvertedClosingPrice
    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedClosingPriceSpecified { set; get; }

    /// <summary>
    /// Get/Set the value of the converted setlment price.
    /// The value has been converted from the numerical base 100 to the display numerical
    /// base of the derivative contract related to the current asset element. Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedClosingPrice")]
    public string ConvertedClosingPrice { get; set; }

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedPosActionUnderlyerPriceSpecified { set; get; }
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlElement("unlPx")]
    public string ConvertedPosActionUnderlyerPrice { get; set; }

    //[System.Xml.Serialization.XmlIgnore()]
    //public bool ConvertedClearingPriceSpecified { set; get; }
    ///// <summary>
    ///// 
    ///// </summary>
    //[System.Xml.Serialization.XmlElement("clrPx")]
    //public string ConvertedClearingPrice { get; set; }

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedTradePriceSpecified { set; get; }
    /// <summary>
    /// 
    /// </summary>
    [System.Xml.Serialization.XmlElement("LastPx")]
    public string ConvertedTradePrice { get; set; }

    ConvertedTradePrice[] _ConvertedTradePrices = new ConvertedTradePrice[0];

    [System.Xml.Serialization.XmlIgnore()]
    public bool ConvertedTradePricesSpecified { set; get; }
    /// <summary>
    /// Get the vector of prices of the trades, having the current asset (instance of ETDRepository) as underlying product. 
    /// All the prices are converted from the default numerical base into the display numerical base of the derivative contract 
    /// related to the current asset element. Moreover a derivative contract specific style has been applied.
    /// </summary>
    [System.Xml.Serialization.XmlArray("convertedTradePrices", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlArrayItem("convertedTradePrice")]
    public ConvertedTradePrice[] ConvertedTradePrices
    {
        get
        {
            return _ConvertedTradePrices;
        }
        set
        {
            _ConvertedTradePrices = new ConvertedTradePrice[value.Length];
            value.CopyTo(_ConvertedTradePrices, 0);

            if (value.Length > 0)
                ConvertedTradePricesSpecified = true;
        }
    }

    /// <summary>
    /// Add or update a new IConvertedTradePrice reference to the ConvertedTradePrices collection
    /// </summary>
    /// <param name="pHRef">trade identifier reference</param>
    /// <param name="pConvertedPrice">converted value</param>
    /// <returns>true if the reference is added, false if updated</returns>
    public bool AddUpdateConvertedTradePrice(string pHRef, string pConvertedPrice)
    {
        bool added = true;

        IConvertedTradePrice tradePrice = ConvertedTradePrices.Where(elem => elem.HRef == pHRef).FirstOrDefault();

        if (tradePrice == null)
        {
            ConvertedTradePrice[] newTradePrice = new ConvertedTradePrice[] { new ConvertedTradePrice(pHRef, pConvertedPrice) };
            ConvertedTradePrices = ConvertedTradePrices.Union(newTradePrice).ToArray();
        }
        else
        {
            added = false;
            tradePrice.ConvertedPrice = pConvertedPrice;
        }

        return added;
    }

    /// <summary>
    /// Evaluate the weighted average price of the current trade prices collection, adding the new given price.
    /// The average is evaluated in base100.
    /// </summary>
    /// <param name="pLastPx">trade price to add to the current average value</param>
    /// <param name="pLastQty">trade quantity used to multiply the input trade price</param>
    /// <param name="pCurrentBase">current numerical base of the value.</param>
    /// <param name="pSide">trade side, Buy or Sell</param>
    /// <returns>the average value in base100, 0 when the side is different from Buy or Sell</returns>
    public decimal EvaluateWeightedAverage(decimal pLastPx, decimal pLastQty, int pCurrentBase, SideEnum pSide)
    {
        if (pCurrentBase <= 0)
        {
            pCurrentBase = 100;
        }

        WeightedAverageTradePrice averageTradePrice;
        switch (pSide)
        {
            case SideEnum.Buy:
                averageTradePrice = this.LongAveragePrice;
                break;

            case SideEnum.Sell:
                averageTradePrice = this.ShortAveragePrice;
                break;

            default:
                return 0;
        }

        if (averageTradePrice.Quantity < 0)
        {
            averageTradePrice.Quantity = 0;
        }

        // 1. Finding the decimal part of the value to be converted
        int integerPart = (int)decimal.Truncate(pLastPx);
        decimal fractionPart = pLastPx - integerPart;
        // 2. Value conversion to the default base (base 100)
        decimal convertedFractionPart = fractionPart * 100 / pCurrentBase;

        // 3. Rebuild the converted last price in base 100
        decimal lastPxBase100 = integerPart + convertedFractionPart;

        averageTradePrice.Quantity += pLastQty;

        // 4. Evaluate the weighted average : (("current average" * "previous quantity") + "new price") / "current quantity"

        // RD 20150909 [21298] Test sur Quantité à zéro
        if (averageTradePrice.Quantity != 0)
        {
            decimal previousQuantity = averageTradePrice.Quantity - pLastQty;
            decimal newPrice = lastPxBase100 * pLastQty;

            averageTradePrice.PriceBase100 =
                ((averageTradePrice.PriceBase100 * previousQuantity) + newPrice)
                /
                averageTradePrice.Quantity;
        }

        return averageTradePrice.PriceBase100;
    }

    IConvertedTradePrice[] IConvertedPrices.ConvertedTradePrices
    {
        get { return this.ConvertedTradePrices.ToArray<IConvertedTradePrice>(); }
    }

}

// 20120820 MF - Ticket 18073
/// <summary>
/// Description of a converted trade price. the converted value has been evalated according with the conversion base and style expressed
/// on the derivative cotnract referential.
/// </summary>
/// <remarks>
/// A collection of these elements is saved into any Asset_ETDRepository object 
/// </remarks>
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
public class ConvertedTradePrice : IConvertedTradePrice
{
    /// <summary>
    /// Returns a new empty object
    /// </summary>
    public ConvertedTradePrice() { }

    /// <summary>
    /// Returns a valid ConvertedTradePrice object
    /// </summary>
    /// <param name="pHRef">trade OTCmlId reference</param>
    /// <param name="pConvertedPrice">converted price value</param>
    public ConvertedTradePrice(string pHRef, string pConvertedPrice)
    {
        HRef = pHRef;

        ConvertedPrice = pConvertedPrice;
    }

    /// <summary>
    /// OTCmlId reference of the trade
    /// </summary>
    [System.Xml.Serialization.XmlAttribute("href", DataType = "IDREF")]
    public string HRef { get; set; }

    /// <summary>
    /// Converted price value
    /// </summary>
    [System.Xml.Serialization.XmlElement("convertedPrice")]
    public string ConvertedPrice { get; set; }
}
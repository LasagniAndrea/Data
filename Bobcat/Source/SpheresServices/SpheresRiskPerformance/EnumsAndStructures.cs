using System;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Filtering and grouping modes for net positions
    /// </summary>
    internal enum GroupingNetPositions
    {
        /// <summary>
        /// Considering a Clearing HOuse aggregation , for each book involved 
        ///     in the risk evaluation, we extract its net positions on trades owned by the Clearing House passed as argument.
        /// </summary>
        ClearingHouse,
        /// <summary>
        /// when we consider the Market aggregation , for each book involved 
        ///     in the risk evaluation, we extract its net positions on trades on derivative contracts owned by the market passed as argument.
        /// </summary>
        [Obsolete("Eurosys legacy, le tri par marché n'est peut-être pas utile pour Spheres", true)]
        Market,
    }

    /// <summary>
    /// Type de la stratégie calculée dans la méthode CBOE Margin
    /// </summary>
    // PM 20191025 [24983] ajout LongCallSpread, ShortCallSpread, LongPutSpread, ShortPutSpread, LongCallTimeSpread, ShortCallTimeSpread, LongPutTimeSpread, ShortPutTimeSpread
    public enum CboeStrategyTypeEnum
    {
        /// <summary>
        /// Spread
        /// </summary>
        Spread,
        /// <summary>
        /// Straddle
        /// </summary>
        Straddle,
        /// <summary>
        /// Combination
        /// </summary>
        Combination,
        /// <summary>
        /// Long Call Spread
        /// </summary>
        LongCallSpread,
        /// <summary>
        /// Short Call Spread
        /// </summary>
        ShortCallSpread,
        /// <summary>
        /// Long Put Spread
        /// </summary>
        LongPutSpread,
        /// <summary>
        /// Short Put Spread
        /// </summary>
        ShortPutSpread,
        /// <summary>
        /// Long Call Time Spread
        /// </summary>
        LongCallTimeSpread,
        /// <summary>
        /// Short Call Time Spread
        /// </summary>
        ShortCallTimeSpread,
        /// <summary>
        /// Long Put Time Spread
        /// </summary>
        LongPutTimeSpread,
        /// <summary>
        /// Short Put Time Spread
        /// </summary>
        ShortPutTimeSpread,
    }
}
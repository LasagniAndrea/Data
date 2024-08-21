using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;
using EfsML.v30.PosRequest;

using FixML.Enum;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Helper class returning positions grouped by a specific data set
    /// </summary>
    public static class PositionsGrouping
    {
        /// <summary>
        /// Collection of parameters used to net positions 
        /// </summary>
        /// <typeparam name="T1">type of the first grouping parameter</typeparam>
        /// <typeparam name="T2">type of the second grouping parameter</typeparam>
        public struct NettingParameters<T1, T2>
        {
            /// <summary>
            /// Internal id of the ETD asset (identifying a net position)
            /// </summary>
            public int AssetId;

            /// <summary>
            /// Type of the quantity in position for the current asset
            /// </summary>
            public RiskMethodQtyType Type;

            /// <summary>
            /// Parameters de grouping/netting, all the asset having the same pair values will be grouped/netted. 
            /// Two netting parameters max available, the first parameter identifies the first level of the netting process.
            /// If you want to exclude the second grouping/netting parameter, then assign null to all the occurences.
            /// </summary>
            /// <example>Scénario TIMS IDEM : 
            /// grouping positions by underlying asset and maturity rule of a contrat.
            /// Pair.First: type int, value UnlAssetId.
            /// Pair.Second: type decimal, value MaturityYearMonth.
            /// </example>
            public Pair<T1, T2> GroupingParameters;

            /// <summary>
            /// Contract category for the current position (F for Futures, O for Options)
            /// </summary>
            public string ContractCategory;

            /// <summary>
            /// Contract multiplier of the current ETD asset, used to compute the LCM among the multipliers of the same group.
            /// </summary>
            public decimal Multiplier;

            /// <summary>
            /// Asset maturity
            /// </summary>
            public decimal Maturity;
        }

        /// <summary>
        /// group the input positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)
        /// </summary>
        /// <param name="pPositions">positions set to be groupedt</param>
        /// <returns>a positions set grouped by asset</returns>
        public static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>>
            GroupPositionsByAsset(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {

            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionsByIdAsset =

                from positionsGroup

                    in (pPositions

                    // group the given input net position by asset
                    .GroupBy(position => position.First.idAsset))

                    // 20120718 MF Ticket 18004 - introducing let on LINQ for positions to do not repeat quantity netting calculations
                let quantityLong = positionsGroup.Where(elem => elem.First.Side == "1").Select(elem => elem.Second.Quantity).Sum()
                // 20120718 MF Ticket 18004 - introducing let on LINQ for positions to do not repeat quantity netting calculations
                let quantityShort = positionsGroup.Where(elem => elem.First.Side == "2").Select(elem => elem.Second.Quantity).Sum()
                // 20120718 MF Ticket 18004 - introducing let on LINQ for positions to do not repeat quantity netting calculations
                let exeAssQuantityLong = positionsGroup.Where(elem => elem.First.Side == "1").Select(elem => elem.Second.ExeAssQuantity).Sum()
                // 20120718 MF Ticket 18004 - introducing let on LINQ for positions to do not repeat quantity netting calculations
                let exeAssQuantityShort = positionsGroup.Where(elem => elem.First.Side == "2").Select(elem => elem.Second.ExeAssQuantity).Sum()

                select

                    new Pair<PosRiskMarginKey, RiskMarginPosition>
                    {
                        First =
                        new PosRiskMarginKey
                        {
                            idI = 0,
                            idAsset = positionsGroup.Key,

                            Side = (quantityLong - quantityShort > 0) ? "1" : "2",

                            idA_Dealer = 0,
                            idB_Dealer = 0,
                            idA_Clearer = 0,
                            idB_Clearer = 0,
                            idA_EntityDealer = 0,
                            idA_EntityClearer = 0
                        },

                        Second =
                        new RiskMarginPosition
                        {
                            TradeIds = (
                                from position
                                    in positionsGroup
                                from tradeId in position.Second.TradeIds
                                select tradeId).ToArray(),

                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            Quantity = System.Math.Abs(quantityLong - quantityShort),

                            // The net settled quantity per asset is evaluated by this formule : "net short" - "net long". If the long
                            //  quantity is greater than the short one the resulting quantity will be negative
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            ExeAssQuantity = exeAssQuantityShort - exeAssQuantityLong,

                            // UNDONE MF Attention, on fait le netting des dénouements , et on prend la date la majeure de livraison, 
                            //  c'est faux, à corriger        
                            DeliveryDate = (from position in positionsGroup select position.Second.DeliveryDate).Max(),

                            SettlementDate = (from position in positionsGroup select position.Second.SettlementDate).Max(),

                            // PM 20130904 [17949] Livraison
                            DeliveryQuantity = (from position in positionsGroup where position.Second.DeliveryQuantity != 0 select position.Second.DeliveryQuantity).Sum(),
                            DeliveryStep = (from position in positionsGroup where position.Second.DeliveryQuantity != 0 select position.Second.DeliveryStep).FirstOrDefault(),
                            DeliveryStepDate = (from position in positionsGroup where position.Second.DeliveryQuantity != 0 select position.Second.DeliveryStepDate).FirstOrDefault(),
                            // PM 20190401 [24625][24387] Ajout DeliveryExpressionType
                            DeliveryExpressionType = (from position in positionsGroup where position.Second.DeliveryQuantity != 0 select position.Second.DeliveryExpressionType).FirstOrDefault(),
                        }

                    };

            return groupedPositionsByIdAsset;
        }

        /// <summary>
        /// Netting/grouping Futures positions 
        /// </summary>
        /// <typeparam name="TNettingParam1">type of the first grouping key</typeparam>
        /// <typeparam name="TNettingParam2">type of the second grouping key</typeparam>
        /// <param name="pGroupedPositionsByIdAsset">positions (already netted by asset) to be grouped/netted</param>
        /// <param name="pGroupingParameters">netting/grouping asset keys, 
        /// any key identifies a group of positions (inside of pGroupedPositionsByIdAsset)</param>
        /// <returns>resulting set of netted/grouped Futures positions</returns>
        public static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> GroupFuturePositions<TNettingParam1, TNettingParam2>
            (IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset,
            IEnumerable<NettingParameters<TNettingParam1, TNettingParam2>> pGroupingParameters)
        {
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositions = null;

            var groupFuturesFirst = from position in pGroupedPositionsByIdAsset
                                    join groupingParameter in pGroupingParameters on position.First.idAsset equals groupingParameter.AssetId
                                    where (groupingParameter.Type == RiskMethodQtyType.FutureMoff && position.Second.ExeAssQuantity != 0)
                                            ||
                                          (groupingParameter.Type == RiskMethodQtyType.Future && position.Second.Quantity > 0)
                                            &&
                                          (groupingParameter.ContractCategory == "F")
                                    // 2.1 definition of the element of the group
                                    group new
                                    {
                                        // Asset Id
                                        groupingParameter.AssetId,
                                        // Net position for the current asset
                                        Position = position,
                                        // Multiplier of the element
                                        groupingParameter.Multiplier,
                                        // Maturity
                                        groupingParameter.Maturity,

                                        // 2.2 grouping criteria (Ex: First =underlying asset)
                                    } by new { groupingParameter.GroupingParameters.First }
                                        into groupFutures
                                    select new
                                    {
                                        // Asset with min multiplier
                                        AssetWithMinMultiplier = groupFutures.Aggregate((current, next) =>
                                                        next.Multiplier < current.Multiplier ?
                                                        next : current),

                                        // Union of all the positions of the group assets
                                        Positions = from elem in groupFutures
                                                    select new
                                                    {
                                                        elem.AssetId,
                                                        elem.Maturity,
                                                        elem.Position.First.Side,
                                                        elem.Multiplier,
                                                        Quantity = (elem.Position.First.Side == "2") ?
                                                            elem.Position.Second.Quantity
                                                            :
                                                            (-1) * elem.Position.Second.Quantity,
                                                        elem.Position.Second.ExeAssQuantity,
                                                        elem.Position.Second.TradeIds
                                                    },

                                    } into groupFuturesExt
                                    select groupFuturesExt;

            var groupFuturesSecond = from groupFuture in groupFuturesFirst
                                     from elem in groupFuture.Positions
                                     group new { groupFuture.AssetWithMinMultiplier, Positions = elem }
                                     by new { elem.Maturity, groupFuture.AssetWithMinMultiplier }
                                         into groupElem
                                     select new
                                     {
                                         IdAsset = groupElem.Any(e => e.Positions.Multiplier == e.AssetWithMinMultiplier.Multiplier) ?
                                          groupElem.First(e => e.Positions.Multiplier == e.AssetWithMinMultiplier.Multiplier).Positions.AssetId : groupElem.First().Positions.AssetId,
                                         groupElem.Key.Maturity,
                                         TradeIds = from elem in groupElem
                                                    select elem.Positions.TradeIds,
                                         Quantity = groupElem.Sum(e => e.Positions.Quantity * e.Positions.Multiplier / e.AssetWithMinMultiplier.Multiplier),
                                         ExeAssQuantity = groupElem.Sum(e => e.Positions.ExeAssQuantity * e.Positions.Multiplier / e.AssetWithMinMultiplier.Multiplier),
                                         groupElem.Key.AssetWithMinMultiplier.Multiplier,
                                     };

            groupedPositions = from groupSecond in groupFuturesSecond
                               select new Pair<PosRiskMarginKey, RiskMarginPosition>
                               {
                                   First = new PosRiskMarginKey()
                                   {
                                       idAsset = groupSecond.IdAsset,

                                       Side = (groupSecond.Quantity > 0) ? "2" : "1",
                                   },
                                   Second = new RiskMarginPosition
                                   {
                                       TradeIds = groupSecond.TradeIds.Aggregate((current, next) =>
                                       {
                                           int[] copy = new int[current.Length + next.Length];
                                           current.CopyTo(copy, 0);
                                           next.CopyTo(copy, current.Length);
                                           return copy;
                                       }),

                                       // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                       // EG 20170127 Qty Long To Decimal
                                       Quantity = System.Math.Abs(groupSecond.Quantity),
                                       ExeAssQuantity = groupSecond.ExeAssQuantity,
                                       Multiplier = groupSecond.Multiplier,
                                   }
                               };

            return groupedPositions;
        }
    }

    /// <summary>
    /// Struct representing a position built on a collection of grouped trades type "allocation" or/and physical settlements,  (<seealso cref="PosRiskMarginKey"/>)
    /// </summary>
    public struct RiskMarginPosition
    {
        /// <summary>
        /// Trades collection constituting the netted position element
        /// </summary>
        /// <remarks>the trades are identified by their Spheres internal id</remarks>
        [XmlArray(ElementName = "Trades")]
        public int[] TradeIds;

        /// <summary>
        /// Quantity in position.
        /// The value must be strictly > 0 for valid position, any netting phase must be return a strictly positive value. 
        /// </summary>
        /// <remarks>
        /// The side of the relative position key (<seealso cref="PosRiskMarginKey"/>) 
        /// could be useful to evaluate the sign of the quantity in position 
        /// (usally negative for long position, and positive for short positions)
        /// </remarks>
        [XmlAttribute(AttributeName = "Qty")]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity;

        /// <summary>
        /// Delivering quantity.
        /// Any value different by 0 is valid. 
        /// A positive value is relative to an assigned quantity (side must be "2" on the position key), 
        /// a negative value to an exercised quantity (side msut be "1" on the position key). 
        /// </summary>
        [XmlAttribute(AttributeName = "ExeAssQty")]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal ExeAssQuantity;

        /// <summary>
        /// Quantité en livraison
        /// </summary>
        // PM 20130904 [17949] Livraison
        [XmlAttribute(AttributeName = "DeliveryQty")]
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal DeliveryQuantity;

        /// <summary>
        /// Date de l'étape courante de livraison
        /// </summary>
        // PM 20130904 [17949] Livraison
        [XmlIgnore]
        public DateTime? DeliveryStepDate;

        /// <summary>
        /// Etape courante de livraison
        /// </summary>
        // PM 20130905 [17949] Livraison
        [XmlIgnore]
        public InitialMarginDeliveryStepEnum DeliveryStep;

        /// <summary>
        /// Delivery date (not null for positions representing physical settlement only)
        /// </summary>
        [XmlIgnore]
        public DateTime? DeliveryDate;

        /// <summary>
        /// Settlement date (not null for positions representing physical settlement only)
        /// </summary>
        [XmlIgnore]
        public DateTime? SettlementDate;

        /// <summary>
        /// Multiplier
        /// </summary>
        [XmlIgnore]
        public decimal? Multiplier;

        /// <summary>
        /// Type d'expression pour déposit de livraison
        /// </summary>
        // PM 20190401 [24625][24387] Ajout DeliveryExpressionType
        [XmlIgnore]
        public ExpressionType DeliveryExpressionType;

        /// <summary>
        /// Get a well initialized position
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public RiskMarginPosition(int[] pTradeIds, decimal pQuantity, decimal pExeAssQuantity, DateTime? pDeliveryDate, DateTime? pSettlementDate)
        {

            this.TradeIds = pTradeIds;

            this.Quantity = pQuantity;

            this.ExeAssQuantity = pExeAssQuantity;

            this.DeliveryDate = pDeliveryDate;

            this.SettlementDate = pSettlementDate;

            this.Multiplier = null;

            // PM 20130905 [17949] Livraison
            this.DeliveryQuantity = 0;
            this.DeliveryStepDate = null;
            this.DeliveryStep = InitialMarginDeliveryStepEnum.NA;
            this.DeliveryExpressionType = ExpressionType.NA;
        }
    }

    /// <summary>
    /// key that identifies one single net position
    /// </summary>
    public class PosRiskMarginKey : PosKeepingKey
    {
        #region Membres
        // PM 20220111 [25617] Ajout membres m_Side et m_FixSide
        private string m_Side;

        private SideEnum m_FixSide;
        #endregion Membres

        // UNDONE MF 20111006 - remplacer le type string du side par une énumération
        /// <summary>
        /// Dealer side of the trade/position ("1" = Buy, "2" = Sell)
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag54.html</remarks>
        // PM 20220111 [25617] Convertion membre side en accesseur
        [XmlAttribute("side")]
        public string Side
        {
            get { return m_Side; }
            set
            {
                m_Side = value;
                m_FixSide = ReflectionTools.ConvertStringToEnumOrDefault<SideEnum>(value, SideEnum.Buy);
            }
        }

        /// <summary>
        /// Side Fix
        /// </summary>
        // PM 20220111 [25617] Ajout FixSide
        public SideEnum FixSide
        {
            get { return m_FixSide; }
            set
            {
                m_FixSide = value;
                m_Side = ReflectionTools.ConvertEnumToString<SideEnum>(value);
            }
        }
        
    }

    /// <summary>
    /// IEqualityComparer implementation for the PosRiskMarginKey class
    /// </summary>
    public class PosRiskMarginKeyComparer : IEqualityComparer<PosRiskMarginKey>
    {
        #region IEqualityComparer<PosRiskMarginKey> Members
        /// <summary>
        /// Check the equality of two keys
        /// </summary>
        /// <param name="x">first key to be compared</param>
        /// <param name="y">second key to be compared</param>
        /// <returns>true when the provided keys are equal</returns>
        public bool Equals(PosRiskMarginKey x, PosRiskMarginKey y)
        {
            bool res = new PosKeepingKeyComparer().Equals(x, y);

            return res && x.Side == y.Side;
        }

        /// <summary>
        /// Get the hashing code of the input key
        /// </summary>
        /// <param name="obj">input key we want ot compute the hashing code</param>
        /// <returns></returns>
        public int GetHashCode(PosRiskMarginKey obj)
        {
            int hashPosKeepingKey = new PosKeepingKeyComparer().GetHashCode(obj);

            int hashSide = obj.Side.GetHashCode();

            return hashPosKeepingKey ^ hashSide;
        }
        #endregion
    }
}

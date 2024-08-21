namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Element definition for a stock coverage 
    /// </summary>
    public sealed class StockCoverageCommunicationObject
    {
        /// <summary>
        /// Derivative contract id to be covered
        /// </summary>
        public int ContractId
        {
            get;

            set;
        }

        /// <summary>
        /// Asset id to be covered
        /// </summary>
        public int AssetId
        {
            get;

            set;
        }

        /// <summary>
        /// Coverage order
        /// </summary>
        public int Order
        {
            get;

            set;
        }

        /// <summary>
        /// Coverage quantity
        /// <para>Qté de couverte par des assets Equities (s'applique aux positions short CALL OPTION ou FUTURE)</para>
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        {
            get;

            set;
        }
    }

    /// <summary>
    ///  Position Equity utilisée pour couvrir des positions ETD short Call ou short Future
    ///  <para>TABLE : POSEQUSECURITY</para>
    /// </summary>
    /// FI 20160613 [22256] Add
    public sealed class StockCoverageDetailCommunicationObject
    {

        /// <summary>
        /// Id non significatif d'une position Equity 
        /// <para>POSEQUSECURITY.IDPOSEQUSECURITY</para>
        /// </summary>
        public int PosId
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier 
        /// <para>POSEQUSECURITY.IDENTIFIER</para>
        /// </summary>
        public string PosIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// <para>Id non significatif de l'acteur</para>
        /// </summary>
        public int ActorId
        {
            get;

            set;
        }

        /// <summary>
        /// <para>Identifier de l'acteur</para>
        /// </summary>
        public string ActorIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// <para>Id non significatif du book</para>
        /// </summary>
        public int BookId
        {
            get;

            set;
        }

        /// <summary>
        /// <para>Identifier non significatif du book</para>
        /// </summary>
        public string BookIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Id non significatif de l'asset Equity
        /// </summary>
        public int AssetId
        {
            get;

            set;
        }

        /// <summary>
        /// Identifiant de l'asset Equity 
        /// </summary>
        public string AssetIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Qté Disponible
        /// <para>POSEQUSECURITY.QTY</para>
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal QtyAvailable
        {
            get;
            set;
        }

        /// <summary>
        ///  Qté utilisée pour réduire des positions Futures
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal QtyUsedFut
        {
            get;
            set;
        }

        /// <summary>
        ///  Qté utilisée pour réduire des positions Options
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal QtyUsedOpt
        {
            get;
            set;
        }
    }
}

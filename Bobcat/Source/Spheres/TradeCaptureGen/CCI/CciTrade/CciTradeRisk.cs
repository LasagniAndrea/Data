namespace EFS.TradeInformation
{
    /// <summary>
    /// Concerne les trades Déposit
    /// </summary>
    public class CciTradeRisk : CciTradeBase
    {

        /// <summary>
        /// Obtient TradeInput
        /// </summary>
        public TradeRiskInput TradeInput
        {
            get { return (TradeRiskInput)TradeCommonInput; }
        }
        
        #region constructor
        public CciTradeRisk(TradeCustomCaptureInfos pCcis)
            : base(pCcis)
        {
            //Initialize(PrefixHeader);
        }
        #endregion constructor
    }

    
}

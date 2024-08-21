namespace EFS.TradeInformation
{

    /// <summary>
    /// Concerne les titres  (référentiel)
    /// </summary>
    public class CciTradeDebtSecurity : CciTradeBase
    {

        /// <summary>
        /// Obtient TradeInput
        /// </summary>
        public DebtSecInput TradeInput
        {
            get { return (DebtSecInput)TradeCommonInput; }
        }

        
        #region constructor
        public CciTradeDebtSecurity(TradeCustomCaptureInfos pCcis)
            : base(pCcis)
        {
            //Initialize(PrefixHeader);
        }
        #endregion constructor
    }
}

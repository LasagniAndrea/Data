#region Debug Directives
using EFS.ACommon;
using EfsML.Business;
#endregion Using Directives

namespace EfsML
{
    #region IRD_TradeAction
    /// <summary>Used by the trade action process </summary>
    public class IRD_TradeAction : TradeAction
    {
        #region Constructors
        public IRD_TradeAction() : base() { }
        public IRD_TradeAction(int pCurrentIdE, TradeActionType.TradeActionTypeEnum pTradeActionType, TradeActionMode.TradeActionModeEnum pTradeActionMode,
            DataDocumentContainer pDataDocumentContainer, TradeActionEvent pEvents)
            : base(pCurrentIdE, pTradeActionType, pTradeActionMode, pDataDocumentContainer, pEvents) { }
        #endregion Constructors
    }
    #endregion IRD_TradeAction
}

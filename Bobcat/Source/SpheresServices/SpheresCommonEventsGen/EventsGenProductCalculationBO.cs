#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationBO : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationBO(string pCS, TradeInfo pTradeInfo) : base(pCS, pTradeInfo) { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pProduct.ProductBase.IsBondOption)
            {
                #region BondOption
                IDebtSecurityOption debtSecurityOption = (IDebtSecurityOption)pProduct;
                debtSecurityOption.Efs_BondOption = new EFS_BondOption(m_ConnectionString, debtSecurityOption, m_tradeInfo.tradeLibrary.DataDocument);
                #region Premium
                debtSecurityOption.Efs_BondOptionPremium = new EFS_BondOptionPremium(m_ConnectionString, debtSecurityOption, m_tradeInfo.tradeLibrary.DataDocument);
                #endregion Premium
                #endregion BondOption
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 20), 0, new LogParam(pProduct.ProductBase.ProductName)));

                throw new NotImplementedException();
            }
            return ret;
        }
        #endregion Methods
    }
    
}

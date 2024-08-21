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
using FixML.Interface;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationESE : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationESE(string pCS, TradeInfo pTradeInfo): base(pCS, pTradeInfo) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        /// EG 20150306 [POC-BERKELEY] : Passage datadocument + statusBusiness au constructeur de EFS_EquitySecurityTransaction
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use slaveDbTransaction
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret;
            if (pProduct.ProductBase.IsEquitySecurityTransaction)
            {
                #region EquitySecurityTransaction
                IEquitySecurityTransaction equitySecurityTransaction = (IEquitySecurityTransaction)pProduct;
                equitySecurityTransaction.Efs_EquitySecurityTransaction = new EFS_EquitySecurityTransaction(CS, pProcess.SlaveDbTransaction, equitySecurityTransaction, 
                    m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);
                ret = equitySecurityTransaction.Efs_EquitySecurityTransaction.ErrLevel;
                #endregion EquitySecurityTransaction
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

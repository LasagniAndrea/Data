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
    public class EventsGenProductCalculationCOMD : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationCOMD(string pCS, TradeInfo pTradeInfo) : base(pCS, pTradeInfo) { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pProduct.ProductBase.IsCommoditySpot)
            {
                #region CommoditySpot
                ICommoditySpot commoditySpot = (ICommoditySpot)pProduct;
                commoditySpot.Efs_CommoditySpot = new EFS_CommoditySpot(m_ConnectionString, m_tradeInfo.tradeLibrary.DataDocument, commoditySpot);
                #endregion CommoditySpot

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

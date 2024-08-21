#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.Accruals;
//
using EfsML;
using EfsML.Business;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.LinearDepreciation
{
    #region LinearDepGenProcess
    public class LinearDepGenProcess : AccrualsBase
    {
        #region Constructor
        public LinearDepGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance) { }
        #endregion Constructor
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret;
            
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 720), 0, new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
            }

            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 1,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                new LogParam(MQueue.GetStringValueIdInfoByKey("GPRODUCT"))));

            m_DsTrade = new DataSetTrade(Cs, CurrentId);
            m_TradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);
            SetAccrualsBookList();
            ret = ProcessExecuteSimpleProduct(m_TradeLibrary.Product);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180502 Analyse du code Correction [CA2214]
        protected override Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            Cst.ErrLevel ret;

            LinearDepGenTreatment linearDepGenTreatment = new LinearDepGenTreatment(this, m_DsTrade, m_TradeLibrary, pProduct);
            linearDepGenTreatment.InitializeDataSetEvent();
            ret = linearDepGenTreatment.Valorize();
            return ret;
        }
        #endregion
    }
    #endregion LinearDepGenProcess
}

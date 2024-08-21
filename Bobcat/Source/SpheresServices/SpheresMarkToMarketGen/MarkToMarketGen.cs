#region Using Directives
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.MarkToMarket
{
	#region MarkToMarketGenProcess (Mark to market)
    // EG 20150317 [POC] : Add mtmGenProcessBase
	public class MarkToMarketGenProcess : ProcessTradeBase 
	{
		#region Members
        private MarkToMarketGenProcessBase mtmGenProcessBase;
		private DataSetTrade     m_DsTrade;
		private EFS_TradeLibrary m_TradeLibrary;
		#endregion Members
        #region Accessors
        // EG 20150317 [POC] : Add MtmGenProcessBase
        public MarkToMarketGenProcessBase MtmGenProcessBase
        {
            get { return mtmGenProcessBase; }
        }
        #endregion Accessors
        #region Constructor
        public MarkToMarketGenProcess(MQueueBase pMQueue,AppInstanceService pAppInstance) : base(pMQueue,pAppInstance) {}
		#endregion Constructor
		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 610), 0, new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
            }
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 1,
                new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                new LogParam(MQueue.GetStringValueIdInfoByKey("GPRODUCT"))));

            #region Select Trade and deserialization
            m_DsTrade = new DataSetTrade(Cs, CurrentId);
            m_TradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);
            #endregion Select Trade and deserialization
            Cst.ErrLevel codeReturn = ProcessExecuteSimpleProduct(m_TradeLibrary.Product);
            return codeReturn;

        }
		
        /// <summary>
		/// 
		/// </summary>
		/// <param name="pProduct"></param>
		/// <returns></returns>
        // EG 20150317 [POC] : mtmGenProcessBase (member of class)
        // EG 20180502 Analyse du code Correction [CA2214]
        protected override Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            mtmGenProcessBase = null;
            if (pProduct.ProductBase.IsIRD)
                mtmGenProcessBase = new MarkToMarketGenProcessIRD(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsFxOption)
                mtmGenProcessBase = new MarkToMarketGenProcessFxOption(this, m_DsTrade, m_TradeLibrary, pProduct);
            else if (pProduct.ProductBase.IsFx)
                mtmGenProcessBase = new MarkToMarketGenProcessFX(this, m_DsTrade, m_TradeLibrary, pProduct);
            // PM 20160811 [RATP] Ajout ExchangeTradedDerivative
            else if (pProduct.ProductBase.IsExchangeTradedDerivative)
                mtmGenProcessBase = new MarkToMarketGenProcessETD(this, m_DsTrade, m_TradeLibrary, pProduct);

            if (null != mtmGenProcessBase)
            {
                mtmGenProcessBase.EndOfInitialize();
                codeReturn = mtmGenProcessBase.Valorize();
            }
            return codeReturn;
        }
		#endregion Methods
	}
	#endregion MarkToMarketGenProcess


}

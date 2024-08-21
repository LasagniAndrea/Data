#region Using
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Linq;
//
using EFS;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;


using EFS.Status;
using EFS.Restriction;  
using EFS.SpheresService;
using EFS.Tuning;

using FpML.Enum;
using EfsML.Enum;
using FpML.Interface;
using EfsML.Business;
using EfsML.Interface;
#endregion Using

namespace EFS.Process
{
    /// <summary>
    /// class de base pour traitement sur un trade
    /// </summary>
    public abstract class ProcessTradeBase : EfsMLProcessBase
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAdmin
        {
            get { return mQueue.IsTradeAdmin; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAsset
        {
            get { return mQueue.IsTradeAsset; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeRisk
        {
            get { return mQueue.IsTradeRisk; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsMonoDataProcess
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected DataSet dsTrades
        {
            get { return _dsDatas; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string dataIdent
        {
            get
            {
                string ret = "N/A";
                if (IsTradeAdmin)
                    ret = Cst.OTCml_TBL.TRADEADMIN.ToString();
                else if (IsTradeAsset)
                    ret = Cst.OTCml_TBL.TRADEDEBTSEC.ToString();
                else if (IsTradeRisk)
                    ret = Cst.OTCml_TBL.TRADERISK.ToString();
                else if (mQueue.GetType().Equals(typeof(PosKeepingRequestMQueue)))
                    ret = Cst.OTCml_TBL.POSREQUEST.ToString();
                else
                    ret = Cst.OTCml_TBL.TRADE.ToString();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum dataTypeLock
        {
            get
            {
                return TypeLockEnum.TRADE;
            }
        }



        #endregion Accessors
        #region constructor
        public ProcessTradeBase(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        { }
        #endregion
        #region Methods
        /// <summary>
        /// Charge les ids qui doivent être traités
        /// <para>Ici ce sont nécessairement des trades</para>
        /// </summary>
        protected override void SelectDatas()
        {
            SelectTrades();
        }

        /// <summary>
        /// Charge les trades qui doivent être traités 
        /// <para>Cette méthode est exécutée lorsque le message ne contient pas d'Id</para>
        /// </summary>
        protected virtual void SelectTrades()
        {
            DataParameters parameters = new DataParameters();
            if (mQueue.idSpecified)
                parameters.Add(new DataParameter(cs, "IDT", DbType.Int32), mQueue.id);
            else if (mQueue.identifierSpecified)
                parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDENTIFIER), mQueue.identifier);
            //
            if (mQueue.parametersSpecified)
            {
                if (null != mQueue.GetObjectValueParameterById(MQueueBase.PARAM_ENTITY))
                {
                    if (-1 != mQueue.GetIntValueParameterById(MQueueBase.PARAM_ENTITY))
                    {
                        DataParameter paramEntity = new DataParameter(cs, "ENTITY", DbType.Int32);
                        paramEntity.Value = mQueue.GetIntValueParameterById(MQueueBase.PARAM_ENTITY);
                        parameters.Add(paramEntity);
                    }
                }
            }

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"t.IDT As IDDATA, t.GPRODUCT As GPRODUCT" + Cst.CrLf);
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE + " t" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            if (mQueue.idSpecified)
                sqlWhere.Append(@"(t.IDT=@IDT)");
            else if (mQueue.identifierSpecified)
                sqlWhere.Append(@"(t.IDENTIFIER=@IDENTIFIER)");
            if (parameters.Contains("ENTITY"))
                sqlWhere.Append(@"((t.IDA_ENTITYBUYER=@ENTITY) or (t.IDA_ENTITYSELLER=@ENTITY))");

            sqlSelect += sqlWhere.ToString();
            string sqlSelectTrade = sqlSelect.ToString();
            _dsDatas = DataHelper.ExecuteDataset(cs, CommandType.Text, sqlSelectTrade, parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();

            if (false == IsProcessObserver)
            {
                #region ProcessTuning => Initialisation from Trade
                // RD 20120809 [18070] Optimisation de la compta
                if (IsMonoDataProcess /*&&
                    (mQueue.ProcessType != Cst.ProcessTypeEnum.EARGEN && mQueue.ProcessType != Cst.ProcessTypeEnum.ACCOUNTGEN)*/)
                {
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CSTools.SetCacheOn(cs), currentId);
                    if (sqlTrade.LoadTable(new string[] { "IDT", "IDI" }))
                    {
                        processTuning = new ProcessTuning(CSTools.SetCacheOn(cs), sqlTrade.IdI, mQueue.ProcessType, appInstance.serviceName, appInstance.HostName);
                        if (processTuningSpecified && (processCall != ProcessCallEnum.Slave))
                            logDetailEnum = processTuning.logDetailEnum;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();
            if (false == IsProcessObserver)
            {
                if (false == TradeRDBMSTools.IsTradeExist(this.cs, currentId))
                {
                    ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);
                    throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat("Trade n°{0} not found", currentId.ToString()), processState);
                }
                if (ProcessStateTools.IsCodeReturnSuccess(CodeReturn) && (false == NoLockCurrentId))
                    CodeReturn = LockCurrentObjectId();
                if (ProcessStateTools.IsCodeReturnSuccess(CodeReturn))
                    CodeReturn = ScanCompatibility_Trade(currentId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            SetTradeStatus(currentId, ProcessState.Status);
        }

        /// <summary>
        /// Execute le traitement pour le produit main du trade (ce produit peut être une strategie)
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        protected Cst.ErrLevel ProcessExecuteProduct(IProduct pProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevel masterCodeReturn = Cst.ErrLevel.SUCCESS;

            if (pProduct.productBase.IsStrategy)
            {
                foreach (IProduct product in ((IStrategy)pProduct).subProduct)
                {
                    codeReturn = ProcessExecuteProduct(product);
                    if (Cst.ErrLevel.SUCCESS != codeReturn)
                        masterCodeReturn = codeReturn;
                }
            }
            else if (pProduct.productBase.IsSwaption)
            {
                codeReturn = ProcessExecuteSimpleProduct(pProduct);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = ProcessExecuteSimpleProduct((IProduct)((ISwaption)pProduct).swap);
                if (Cst.ErrLevel.SUCCESS != codeReturn)
                    masterCodeReturn = codeReturn;
            }
            else
            {
                masterCodeReturn = ProcessExecuteSimpleProduct(pProduct);
            }

            return masterCodeReturn;
        }

        /// <summary>
        /// Execute le traitement pour un produit simple (différent de strategie)
        /// <para>Le product peut être un subProduct d'une strategie</para>
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        protected virtual Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Methods
    }
}

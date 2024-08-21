#region Using
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.Tuning;
using FpML.Interface;
using System.Data;
using System.Reflection;
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
            get { return MQueue.IsTradeAdmin; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAsset
        {
            get { return MQueue.IsTradeAsset; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeRisk
        {
            get { return MQueue.IsTradeRisk; }
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
        protected override string DataIdent
        {
            get
            {
                string ret;
                if (IsTradeAdmin)
                    ret = Cst.OTCml_TBL.TRADEADMIN.ToString();
                else if (IsTradeAsset)
                    ret = Cst.OTCml_TBL.TRADEDEBTSEC.ToString();
                else if (IsTradeRisk)
                    ret = Cst.OTCml_TBL.TRADERISK.ToString();
                else if (MQueue.GetType().Equals(typeof(PosKeepingRequestMQueue)))
                    ret = Cst.OTCml_TBL.POSREQUEST.ToString();
                else
                    ret = Cst.OTCml_TBL.TRADE.ToString();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum DataTypeLock
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
            if (MQueue.idSpecified)
                parameters.Add(new DataParameter(Cs, "IDT", DbType.Int32), MQueue.id);
            else if (MQueue.identifierSpecified)
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), MQueue.identifier);
            //
            if (MQueue.parametersSpecified)
            {
                if (null != MQueue.GetObjectValueParameterById(MQueueBase.PARAM_ENTITY))
                {
                    if (-1 != MQueue.GetIntValueParameterById(MQueueBase.PARAM_ENTITY))
                    {
                        DataParameter paramEntity = new DataParameter(Cs, "ENTITY", DbType.Int32)
                        {
                            Value = MQueue.GetIntValueParameterById(MQueueBase.PARAM_ENTITY)
                        };
                        parameters.Add(paramEntity);
                    }
                }
            }

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"t.IDT As IDDATA, t.GPRODUCT As GPRODUCT" + Cst.CrLf);
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE + " t" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            if (MQueue.idSpecified)
                sqlWhere.Append(@"(t.IDT=@IDT)");
            else if (MQueue.identifierSpecified)
                sqlWhere.Append(@"(t.IDENTIFIER=@IDENTIFIER)");
            if (parameters.Contains("ENTITY"))
                sqlWhere.Append(@"((t.IDA_ENTITYBUYER=@ENTITY) or (t.IDA_ENTITYSELLER=@ENTITY))");

            sqlSelect += sqlWhere.ToString();
            string sqlSelectTrade = sqlSelect.ToString();
            DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelectTrade, parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Chargement de processTuning
        /// </summary>
        // EG 20190613 [24683] Use slaveDbTransaction
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
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
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CSTools.SetCacheOn(Cs), CurrentId)
                    {
                        DbTransaction = SlaveDbTransaction
                    };
                    if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDI" }))
                    {
                        ProcessTuning = new ProcessTuning(CSTools.SetCacheOn(Cs), sqlTrade.IdI, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                        if (ProcessTuningSpecified && (ProcessCall == ProcessCallEnum.Master))
                        {
                            LogDetailEnum = ProcessTuning.LogDetailEnum;

                            
                            Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Verification ultime avant execution du traitement 
        /// <para>- Mise en place d'un lock sur le trade </para>
        /// <para>- Verification du respect par rapport à processTuning</para>
        /// </summary>
        // EG 20190613 [24683] Use slaveDbTransaction
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();

            if (false == IsProcessObserver)
            {
                if (false == TradeRDBMSTools.IsTradeExist(this.Cs, SlaveDbTransaction, CurrentId))
                {
                    ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat("Trade n°{0} not found", CurrentId.ToString()), processState);
                }
                
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn) && (false == NoLockCurrentId))
                    ProcessState.CodeReturn = LockCurrentObjectId();
                
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                    ProcessState.CodeReturn = ScanCompatibility_Trade(CurrentId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            SetTradeStatus(CurrentId, ProcessState.Status);
        }

        /// <summary>
        /// Execute le traitement pour le produit main du trade (ce produit peut être une strategie)
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        //PL 20200107 Set PUBLIC intead of PROTECTED
        //protected Cst.ErrLevel ProcessExecuteProduct(IProduct pProduct)
        public Cst.ErrLevel ProcessExecuteProduct(IProduct pProduct)
        {
            Cst.ErrLevel masterCodeReturn = Cst.ErrLevel.SUCCESS;

            Cst.ErrLevel codeReturn;
            if (pProduct.ProductBase.IsStrategy)
            {
                foreach (IProduct product in ((IStrategy)pProduct).SubProduct)
                {
                    codeReturn = ProcessExecuteProduct(product);
                    if (Cst.ErrLevel.SUCCESS != codeReturn)
                        masterCodeReturn = codeReturn;
                }
            }
            else if (pProduct.ProductBase.IsSwaption)
            {
                codeReturn = ProcessExecuteSimpleProduct(pProduct);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = ProcessExecuteSimpleProduct((IProduct)((ISwaption)pProduct).Swap);
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

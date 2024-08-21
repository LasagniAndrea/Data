#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using System.Data;
#endregion Using Directives

namespace EFS.Process.SettlementInstrGen
{
    /// <summary>
    /// 
    /// </summary>
    public class SettlementInstrGenProcess : ProcessTradeBase
    {
        #region Members
        public SettlementInstrGenMQueue settlementInstrGenMQueue;
        #endregion Members
        #region Constructor
        public SettlementInstrGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance)
        {
            settlementInstrGenMQueue = (SettlementInstrGenMQueue)pMQueue;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void SelectTrades()
        {
            DataParameters parameters = new DataParameters();
            bool isToEnd = false;
            if (settlementInstrGenMQueue.idSpecified)
                parameters.Add(new DataParameter(Cs, "IDT", DbType.Int32), settlementInstrGenMQueue.id);
            else if (MQueue.identifierSpecified)
                parameters.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDENTIFIER), MQueue.identifier);

            if (settlementInstrGenMQueue.parametersSpecified)
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
                // DATE1 paramètre équivalent à DTSTL paramètre
                if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_DATE1)
                    ||
                    null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTL)
                )
                {
                    DataParameter paramDtSTL = new DataParameter(Cs, "DTSTL", DbType.Date)
                    {
                        Value = settlementInstrGenMQueue.GetDateTimeValueParameterById(SettlementInstrGenMQueue.PARAM_DATE1)
                    };
                    parameters.Add(paramDtSTL);
                }
                if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTM))
                {
                    DataParameter paramDtSTM = new DataParameter(Cs, "DTSTM", DbType.Date)
                    {
                        Value = settlementInstrGenMQueue.GetDateTimeValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTM)
                    };
                    parameters.Add(paramDtSTM);
                }
                if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_ISTOEND))
                    isToEnd = settlementInstrGenMQueue.GetBoolValueParameterById(SettlementInstrGenMQueue.PARAM_ISTOEND);
                //
                if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_PAY))
                {
                    DataParameter paramPayer = new DataParameter(Cs, "IDA_PAY", DbType.Int32)
                    {
                        Value = settlementInstrGenMQueue.GetIntValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_PAY)
                    };
                    parameters.Add(paramPayer);
                }
                if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_REC))
                {
                    DataParameter paramReceiver = new DataParameter(Cs, "IDA_REC", DbType.Int32)
                    {
                        Value = settlementInstrGenMQueue.GetIntValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_REC)
                    };
                    parameters.Add(paramReceiver);
                }
            }

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT);
            sqlSelect += @"e.IDT As IDDATA,t.GPRODUCT As GPRODUCT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_TRADE + " t";
            sqlSelect += SQLCst.ON + "(t.IDT = e.IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ecs";
            sqlSelect += SQLCst.ON + "(ecs.IDE = e.IDE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ecs.EVENTCLASS =" + DataHelper.SQLString(EventClassFunc.Settlement) + ")" + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ecm ";
            sqlSelect += SQLCst.ON + "(ecm.IDE = e.IDE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "(ecm.EVENTCLASS " + SQLCst.IN + "(";
            sqlSelect += DataHelper.SQLString(EventClassFunc.DeliveryMessage) + "," + DataHelper.SQLString(EventClassFunc.SettlementMessage);
            sqlSelect += ")" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            if (settlementInstrGenMQueue.idSpecified)
                sqlWhere.Append(@"(e.IDT=@IDT)");
            else if (MQueue.identifierSpecified)
                sqlWhere.Append(@"(t.IDENTIFIER=@IDENTIFIER)");

            if (parameters.Contains("ENTITY"))
                sqlWhere.Append(@"((t.IDA_ENTITYBUYER=@ENTITY) or (t.IDA_ENTITYSELLER=@ENTITY))");
            if (null != parameters["IDA_PAY"])
                sqlWhere.Append(@"(e.IDA_PAY=@IDA_PAY)");
            if (null != parameters["IDA_REC"])
                sqlWhere.Append(@"(e.IDA_REC=@IDA_REC)");
            if (null != parameters["DTSTL"])
            {
                if (isToEnd)
                    sqlWhere.Append(@"(ecs.DTEVENTFORCED>=@DTSTL)");
                else
                    sqlWhere.Append(@"(ecs.DTEVENTFORCED=@DTSTL)");
            }
            if (null != parameters["DTSTM"])
            {
                if (isToEnd)
                    sqlWhere.Append(@"(ecm.DTEVENTFORCED>=@DTSTM)");
                else
                    sqlWhere.Append(@"(ecm.DTEVENTFORCED=@DTSTM)");
            }
            sqlSelect += sqlWhere.ToString();

            DsDatas = DataHelper.ExecuteDataset(Cs, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            // Chargement du trade
            if (ProcessCall == ProcessCallEnum.Master)
            {

                // PM 20210121 [XXXXX] Passage du message au niveau de log None
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 3400), 0, new LogParam(LogTools.IdentifierAndId(settlementInstrGenMQueue.Identifier, CurrentId))));
            }

            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 501), 1,
                new LogParam(LogTools.IdentifierAndId(settlementInstrGenMQueue.Identifier, CurrentId)),
                new LogParam(settlementInstrGenMQueue.GetStringValueIdInfoByKey("GPRODUCT"))));

            #region Select Trade and deserialization
            DataSetTrade dsTrade = new DataSetTrade(Cs, CurrentId);
            EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);
            #endregion Select Trade and deserialization

            #region Le traitement doit-il être ignoré 
            //FI 20120217 Ce code est à suppimer dans l'avenir Les évènement STL seront correctement alimentés vis à vis de ISPAYMENT 
            _ = dsTrade.DtTrade.Rows[0]["IDSTBUSINESS"].ToString();
            if (Cst.StatusBusiness.ALLOC.ToString() == dsTrade.DtTrade.Rows[0]["IDSTBUSINESS"].ToString())
                codeReturn = Cst.ErrLevel.TUNING_IGNOREFORCED;
            else if (tradeLibrary.Product.ProductBase.IsCashBalance)
                codeReturn = Cst.ErrLevel.TUNING_IGNOREFORCED;
            // PM 20120831 [18058] Ignore également pour les Cash Interest
            else if (tradeLibrary.Product.ProductBase.IsCashBalanceInterest)
                codeReturn = Cst.ErrLevel.TUNING_IGNOREFORCED;
            else
            {
                SQL_Instrument instr = new SQL_Instrument(CSTools.SetCacheOn(Cs), dsTrade.IdI);
                if (instr.Product_Identifier == Cst.ProductCashPayment)
                    codeReturn = Cst.ErrLevel.TUNING_IGNOREFORCED;
            }
            #endregion

            if (codeReturn == Cst.ErrLevel.SUCCESS)
            {
                SettlementInstrTradeGen stl = new SettlementInstrTradeGen(this, dsTrade, tradeLibrary);
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3401), 1,
                    new LogParam(LogTools.IdentifierAndId(settlementInstrGenMQueue.Identifier, CurrentId))));

                codeReturn = stl.Execute();
            }

            return codeReturn;
        }
        #endregion Methods
    }
}

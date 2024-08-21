#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;

using EfsML;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.MarkToMarket
{
	#region class MarkToMarketGenProcessIRD
	public class MarkToMarketGenProcessIRD : MarkToMarketGenProcessBase
	{
		#region Accessors
		#region Parameters
        public override CommonValParameters ParametersIRD
        {
            get { return m_Parameters; }
        }
		#endregion Parameters
		#endregion Accessors
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public MarkToMarketGenProcessIRD(MarkToMarketGenProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_Parameters = new CommonValParametersIRD();
            ////Colonnes fictives pour stocker des informations servant à alimenter EVENTPRICING
            //DsEvents.DtEventAsset.Columns.Add("IDYIELDCURVEVAL_H", Type.GetType("System.Int32"));
            //DsEvents.DtEventAsset.Columns.Add("ZEROCOUPON1", Type.GetType("System.Decimal"));
            //DsEvents.DtEventAsset.Columns.Add("ZEROCOUPON2", Type.GetType("System.Decimal"));
            //DsEvents.DtEventAsset.Columns.Add("FORWARDRATE", Type.GetType("System.Decimal"));
            //DsEvents.DtEventAsset.Columns.Add("RATE", Type.GetType("System.Decimal"));
            ////ISEXISTING = true sur les evènements déjà valorisés
            //DsEvents.DtEvent.Columns.Add("ISEXISTING", Type.GetType("System.Boolean"));
            //DataRow[] rows =  DsEvents.DtEvent.Select() ;
            //foreach (DataRow dr in rows)
            //{
            //    if (dr["VALORISATION"] != Convert.DBNull)
            //        dr["ISEXISTING"] = true;
            //}
        }
		#endregion Constructors
		#region Methods

        #region EndOfInitialize
        // EG 20180502 Analyse du code Correction [CA2214]
        public override void EndOfInitialize()
        {
            base.EndOfInitialize();
            //Colonnes fictives pour stocker des informations servant à alimenter EVENTPRICING
            DsEvents.DtEventAsset.Columns.Add("IDYIELDCURVEVAL_H", Type.GetType("System.Int32"));
            DsEvents.DtEventAsset.Columns.Add("ZEROCOUPON1", Type.GetType("System.Decimal"));
            DsEvents.DtEventAsset.Columns.Add("ZEROCOUPON2", Type.GetType("System.Decimal"));
            DsEvents.DtEventAsset.Columns.Add("FORWARDRATE", Type.GetType("System.Decimal"));
            DsEvents.DtEventAsset.Columns.Add("RATE", Type.GetType("System.Decimal"));
            //ISEXISTING = true sur les evènements déjà valorisés
            DsEvents.DtEvent.Columns.Add("ISEXISTING", Type.GetType("System.Boolean"));
            DataRow[] rows = DsEvents.DtEvent.Select();
            foreach (DataRow dr in rows)
            {
                if (dr["VALORISATION"] != Convert.DBNull)
                    dr["ISEXISTING"] = true;
            }
        }
        #endregion EndOfInitialize

        #region IsRowMustBeCalculate
        /// <summary>
        /// Obtient true si l'évènement est en prendre en considération pour obtenir le MarkToMarket
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
		public override bool IsRowMustBeCalculate(DataRow pRow)
		{
            bool ret = false;
            string eventCode = pRow["EVENTCODE"].ToString();
            string eventType = pRow["EVENTTYPE"].ToString();
            if (EventTypeFunc.IsInterest(eventType) || EventTypeFunc.IsNominal(eventType) || EventCodeFunc.IsAdditionalPayment(eventCode))
            {
                DataRow rowEventClassSTL = GetRowEventClass(Convert.ToInt32(pRow["IDE"]), EventClassEnum.STL.ToString());
                ret = (null != rowEventClassSTL) && BoolFunc.IsTrue(rowEventClassSTL["ISPAYMENT"]);
                if (ret)
                    ret = Convert.ToDateTime(rowEventClassSTL["DTEVENT"]) > CommonValDate;
            }
            else if (EventCodeFunc.IsCalculationPeriod(eventCode) ||
                EventCodeFunc.IsReset(eventCode) || EventCodeFunc.IsSelfAverage(eventCode) || EventCodeFunc.IsSelfReset(eventCode))
            {
                //Ce cas est utile à EFS_PaymentEvent
                //On considère tous les évènemnets enfants d'un flux d'intérêt
                ret = true ;
            }
            return ret;
		}
		#endregion IsRowMustBeCalculate
        #region IsRowsEventCalculated
        public override bool IsRowsEventCalculated(DataRow[] pRows)
        {
            foreach (DataRow row in pRows)
            {
                if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
                {
                    DataRow[] rowChilds = row.GetChildRows(DsEvents.ChildEvent);
                    if (0 != rowChilds.Length)
                        return IsRowsEventCalculated(rowChilds);
                    return false;
                }
            }
            return true;
        }
		#endregion IsRowsEventCalculated
        #region Valorize
        /// <summary>
		///  Calcul du markToMarket pour le {Trade,Product} en cours
        /// <para> Il y aura au final plusieurs MTM</para>
        /// <para> un pour chaque stream d'intérêt</para>
        /// <para> un pour chaque additional paiement</para>
        /// </summary>
		/// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            DataRow[] rowsStreams = GetStreams() ;
            if (ArrFunc.IsEmpty(rowsStreams))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 
                    StrFunc.AppendFormat("No Stream Events found for trade:{0}", DsTrade.Identifier));

            foreach (DataRow rowStream in rowsStreams)
            {
                int idE = Convert.ToInt32(rowStream["IDE"]);
                DataRowEvent rowInfo = new DataRowEvent(rowStream);
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 611), 1,
                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                    new LogParam(LogTools.IdentifierAndId(rowInfo.EventCode + " / " + rowInfo.EventType, rowInfo.Id))));

                #region ScanCompatibility_Event
                bool isRowOk = (Cst.ErrLevel.SUCCESS == m_MarkToMarketGenProcess.ScanCompatibility_Event(idE));
                if (false == isRowOk)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.SYS, 25), 1,
                        new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                        new LogParam(rowInfo.Id)));
                }
                #endregion ScanCompatibility_Event

                if (isRowOk)
                {
                    bool isError = false;
                    #region Calcul du MarkToMarket
                    try
                    {
                        MarkToMarketStream(rowStream);
                    }
                    catch (Exception ex)
                    {
                        //Erreur non fatale pour le traitement, le traitement poursuit sur le prochain stream
                        isError = true;

                        // FI 20200623 [XXXXX] AddCriticalException
                        m_MarkToMarketGenProcess.ProcessState.AddCriticalException(ex);

                        Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                    }
                    #endregion Calcul du MarkToMarket
                    // update Database
                    Update(idE, isError); 
                    if (isError)
                        ret = Cst.ErrLevel.ABORTED;
                }
            }
            return ret;
        }	
		#endregion Valorize
        #region MarkToMarketStream
        /// <summary>
        /// Calcul du MTM du stream, Ajout d'un évènement enfant représentatif du MTM
        /// </summary>
        /// <param name="pRowStream"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void MarkToMarketStream(DataRow pRowStream)
        {
            DeleteRowsEventPricing2(pRowStream);
            Decimal amountMarkToMarket = Decimal.Zero;
            bool isAddRowMarkToMark = false;

            int instrNo = IntFunc.IntValue(pRowStream["INSTRUMENTNO"].ToString());
            int streamNo = IntFunc.IntValue(pRowStream["STREAMNO"].ToString());
            string eventCode = pRowStream["EVENTCODE"].ToString();
            string eventType = pRowStream["EVENTTYPE"].ToString();

            PayerReceiverInfo payRecInfoStream = new PayerReceiverInfo(this, pRowStream);

            m_ParamInstrumentNo.Value = instrNo;
            m_ParamStreamNo.Value = streamNo;
            ParametersIRD.Add(m_CS, TradeLibrary, pRowStream);

            //Recherche de la devise du MTM
            string currencyMarkToMarket = GetMartToMarketCurrency(pRowStream);
            //Calcul du MTM
            try
            {
                if (IsStreamInterest(eventCode))
                {
                    #region Estimation des flux futurs
                    DataRow[] rowsInterest = GetRowInterest(streamNo);
                    if (ArrFunc.IsFilled(rowsInterest))
                    {
                        //Estimations des flux futurs d'intérêts
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 612), 2,
                            new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                            new LogParam(LogTools.IdentifierAndId(eventCode + " / " + eventType, Convert.ToInt32(pRowStream["IDE"])))));

                        foreach (DataRow rowInterest in rowsInterest)
                        {
                            bool isRowMustBeCalculate = IsRowMustBeCalculate(rowInterest);
                            if (isRowMustBeCalculate)
                            {
                                isAddRowMarkToMark = true;
                                new EFS_PaymentEvent(CommonValDate, this, rowInterest);

                                //Ajout des informations détails dans EVENTPRICING2
                                AddEventPricingForInterest(rowInterest);

                                //Ajout des informations détails dans le Log
                                AddLogDetailPricingForInterest(rowInterest);  

                                decimal interest = DecFunc.DecValue(rowInterest["VALORISATION"].ToString() );
                                
                                //string sysNumber = "LOG-00613";
                                SysMsgCode sysNumber = new SysMsgCode(SysCodeEnum.LOG, 613);
                                if (BoolFunc.IsTrue(rowInterest["ISEXISTING"]))
                                {
                                    //sysNumber = "LOG-00614";
                                    sysNumber = new SysMsgCode(SysCodeEnum.LOG, 614);
                                }
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, sysNumber, 3,
                                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                    new LogParam(LogTools.AmountAndCurrency(interest, currencyMarkToMarket))));
                            }
                        }
                    }
                    #endregion Estimation des flux futurs
                }
                #region Somme algébrique des flux futurs actualisés
                //Somme algébrique des flux futures actualisés
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 615), 2,
                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id))));

                if (IsStreamInterest(eventCode))
                {
                    #region Somme algébrique des flux d'intérêrts actualisés
                    DataRow[] rowsInterest = GetRowInterest(streamNo);
                    if (ArrFunc.IsFilled(rowsInterest))
                    {
                        foreach (DataRow rowInterest in rowsInterest)
                        {
                            bool isRowMustBeCalculate = IsRowMustBeCalculate(rowInterest);
                            if (isRowMustBeCalculate)
                            {
                                isAddRowMarkToMark = true;
                                PayerReceiverInfo payRecInfoItem = new PayerReceiverInfo(this, rowInterest);
                                decimal actualizedAmount = CalcActualizedAmount(rowInterest);

                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 616), 3,
                                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                    new LogParam(EventTypeFunc.Interest),
                                    new LogParam(LogTools.AmountAndCurrency(actualizedAmount, currencyMarkToMarket))));

                                amountMarkToMarket = ConstructMarkToMarket(amountMarkToMarket, actualizedAmount, payRecInfoItem, payRecInfoStream);
                            }
                        }
                    }
                    #endregion Somme algébrique des flux d'intérêrts actualisés

                    #region Somme algébrique des flux de nominal actualisés
                    DataRow[] rowsNominal = GetRowNominal(streamNo);
                    if (ArrFunc.IsFilled(rowsNominal))
                    {
                        foreach (DataRow rowNominal in rowsNominal)
                        {
                            bool isRowMustBeCalculate = IsRowMustBeCalculate(rowNominal);
                            if (isRowMustBeCalculate)
                            {
                                isAddRowMarkToMark = true;
                                PayerReceiverInfo payRecInfoItem = new PayerReceiverInfo(this, rowNominal);
                                decimal actualizedAmount = CalcActualizedAmount(rowNominal);

                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 616), 3,
                                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                    new LogParam(EventTypeFunc.Nominal),
                                    new LogParam(LogTools.AmountAndCurrency(actualizedAmount, currencyMarkToMarket))));

                                amountMarkToMarket = ConstructMarkToMarket(amountMarkToMarket, actualizedAmount, payRecInfoItem, payRecInfoStream);
                            }
                        }
                    }
                    #endregion Somme algébrique des flux de nominal actualisés
                }
                else if (EventCodeFunc.IsAdditionalPayment(pRowStream["EVENTCODE"].ToString()))
                {
                    #region Actualisation de l'additionnal payment
                    bool isRowMustBeCalculate = IsRowMustBeCalculate(pRowStream);
                    if (isRowMustBeCalculate)
                    {
                        isAddRowMarkToMark = true;
                        PayerReceiverInfo payRecInfoItem = new PayerReceiverInfo(this, pRowStream);
                        decimal actualizedAmount = CalcActualizedAmount(pRowStream);

                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 616), 3,
                            new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                            new LogParam(EventCodeFunc.AdditionalPayment),
                            new LogParam(LogTools.AmountAndCurrency(actualizedAmount, currencyMarkToMarket))));

                        amountMarkToMarket = ConstructMarkToMarket(amountMarkToMarket, actualizedAmount, payRecInfoItem, payRecInfoStream);
                    }
                    #endregion Actualisation de l'additionnal payment
                }
                else
                    throw new Exception(StrFunc.AppendFormat("EVENTCODE:{0} not implemented", eventCode));
                #endregion
            }
            catch {throw;}
            finally
            {
                if (IsStreamInterest(eventCode))
                {
                    //Suppression des modifications effectuées par EFS_PaymentEvent
                    DataRow[] rowsInterest = GetRowInterest(streamNo);
                    foreach (DataRow rowInterest in rowsInterest)
                        RejectChangesInRowInterest(rowInterest);
                }
            }
            //Ajout de la ligne MarkToMarket
            if (isAddRowMarkToMark)
            {
                amountMarkToMarket = RoundingCurrencyAmount(currencyMarkToMarket, amountMarkToMarket);
                IMoney money = ProductBase.CreateMoney(amountMarkToMarket, currencyMarkToMarket);

                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 617), 2,
                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                    new LogParam(EventCodeFunc.DailyClosing + " / " + EventTypeFunc.MarkToMarket + " / " + EventClassFunc.ForwardRateProjection),
                    new LogParam(LogTools.AmountAndCurrency(money.Amount.DecValue, money.Currency))));

                //AddRowMarkToMarket(pRowStream, amountMarkToMarket, currencyMarkToMarket, EventClassEnum.FRP.ToString(), payRecInfoStream);
                // EG 20150605 [21011] New
                CommonValParameterIRD parameter = (CommonValParameterIRD)ParametersIRD[instrNo, streamNo];
                PayerReceiverInfoDet streamPayer = parameter.GetPayerReceiverInfoDet(TradeLibrary.DataDocument, PayerReceiverEnum.Payer);
                PayerReceiverInfoDet streamReceiver = parameter.GetPayerReceiverInfoDet(TradeLibrary.DataDocument, PayerReceiverEnum.Receiver);
                AddRowMarkToMarket(pRowStream, amountMarkToMarket, currencyMarkToMarket, EventClassEnum.FRP.ToString(), streamPayer, streamReceiver);
            }
        }
        #endregion
        #region GetMartToMarketCurrency
        /// <summary>
        /// Retourne la devise du MarkToMarket
        /// </summary>
        /// <exception cref="Spheres
        /// Exception si devise non trouvée [BUG]"></exception>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        private string GetMartToMarketCurrency(DataRow pRowStream)
        {
            string ret = string.Empty;
            int instrNo = Convert.ToInt32(pRowStream["INSTRUMENTNO"]);
            int streamNo = Convert.ToInt32(pRowStream["STREAMNO"]);
            string eventCode = pRowStream["EVENTCODE"].ToString();
            //
            if (IsStreamInterest(eventCode))
            {
                CommonValParameterIRD paramIRD = (CommonValParameterIRD) Parameters[instrNo, streamNo];
                if (m_CurrentProduct.ProductBase.IsFra)
                    ret = paramIRD.FraNotional.Currency;
                else if (m_CurrentProduct.ProductBase.IsSwap || m_CurrentProduct.ProductBase.IsLoanDeposit || m_CurrentProduct.ProductBase.IsFxTermDeposit)
                    ret = paramIRD.Stream.StreamCurrency;
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Product {0} is not is not managed, please contact EFS", m_CurrentProduct.GetType().FullName));
            }
            else if (EventCodeFunc.IsAdditionalPayment(eventCode))
            {
                ret = pRowStream["UNIT"].ToString();
            }
            if (StrFunc.IsEmpty(ret))
                throw new Exception(StrFunc.AppendFormat("Currency is empty"));
            return ret;
        }
        #endregion
        #region CalcActualizedAmount
        private decimal CalcActualizedAmount(DataRow pDataRow)
        {
            int idEvent = Convert.ToInt32(pDataRow["IDE"]);
            PayerReceiverInfo payRecInfo = new PayerReceiverInfo(this, pDataRow);

            DataRow eventClass = GetRowEventClass(idEvent, EventClassEnum.STL.ToString());
            DateTime dateSTL = Convert.ToDateTime(eventClass["DTEVENT"]);

            decimal amount = Convert.ToDecimal(pDataRow["VALORISATION"]);
            string currency = Convert.ToString(pDataRow["UNIT"]);
            IMoney money = ProductBase.CreateMoney(amount, currency);

            decimal ret = CalcActualizedAmount(money, dateSTL, payRecInfo, out YieldCurveVal yieldCurveVal, out decimal discountFactor);

            string eventType = pDataRow["EVENTTYPE"].ToString();
            if (EventTypeFunc.IsNominal(eventType))
                eventType = "nominal";
            else if  (EventTypeFunc.IsInterest(eventType))  
                eventType = "interest";
            else 
                eventType =  pDataRow["EVENTTYPE"].ToString();

            AddRowEventPricing2(idEvent);
            DataRow[] row = GetRowEventPricing2(idEvent);
            if (ArrFunc.IsFilled(row))
            {
                row[0]["FLOWTYPE"] = eventType;
                row[0]["CASHFLOW"] = amount;
                row[0]["METHOD"] = EventClassEnum.FRP.ToString();
                row[0]["DISCOUNTFACTOR"] = discountFactor;
                row[0]["NPV"] = ret;
                row[0]["IDYIELDCURVEVAL_H"] = yieldCurveVal.idYieldCurveVal_H;
                row[0]["TOTALOFDAY"] = new TimeSpan(dateSTL.Ticks - CommonValDate.Ticks).Days;
            }
            return ret;
        }
        #endregion
        #region GetStreams
        /// <summary>
        /// Retourne les lignes évènement pour lesquelles Spheres va évaluer le MTM
        /// <para>Le martToMarket sera stocké dans un évènement enfant </para>
        /// </summary>
        /// <returns></returns>
        private DataRow[] GetStreams()
        {
            DataRow[] ret = null;
            ArrayList al = new ArrayList();
            // Ajout des lignes d'intérêts
            try
            {
                DataRow[] rowsStreams = null;
                //
                if (m_CurrentProduct.ProductBase.IsSwap)
                    rowsStreams = GetRowEventByEventCode(EventCodeFunc.InterestRateSwap);
                else if (m_CurrentProduct.ProductBase.IsLoanDeposit)
                    rowsStreams = GetRowEventByEventCode(EventCodeFunc.LoanDeposit);
                else if (m_CurrentProduct.ProductBase.IsFra)
                    rowsStreams = GetRowEventByEventCode(EventCodeFunc.ForwardRateAgreement);
                else if (m_CurrentProduct.ProductBase.IsFxTermDeposit)
                    rowsStreams = GetRowEventByEventCode(EventCodeFunc.TermDeposit);
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("Product {0} is not is not managed, please contact EFS", m_CurrentProduct.GetType().FullName));
                //
                if (ArrFunc.IsEmpty(rowsStreams))
                    throw new Exception(StrFunc.AppendFormat("No Interest Stream Events found for trade:{0}", DsTrade.Identifier));
                al.AddRange(rowsStreams);
            }
            catch (Exception) { throw; }

            // Ajout des lignes ADP
            try
            {
                DataRow[] rowsStreams = GetRowAdditionalPayment();
                if (ArrFunc.IsFilled(rowsStreams))
                    al.AddRange(rowsStreams);
            }
            catch (Exception) { throw; }
            //
            if (ArrFunc.IsFilled(al))
                ret = (DataRow[])al.ToArray(typeof(DataRow));
            return ret;
        }
        #endregion
        #region AddEventPricingForInterest
        /// <summary>
        /// Ajout des informations détails dans EVENTPRICING2 (taux zero coupon, forwardRate, etc...) utilisé pour estimer le flux d'intérêt {pRowInterest}
        /// </summary>
        /// <param name="pRowInterest"></param>
        private void AddEventPricingForInterest(DataRow pRowInterest)
        {
            DataRow[] rowChilds = pRowInterest.GetChildRows(DsEvents.ChildEvent);
            foreach (DataRow rowChild in rowChilds)
            {
                if (EventCodeFunc.IsCalculationPeriod(rowChild["EVENTCODE"].ToString()))
                {
                    DataRow[] rowResets = rowChild.GetChildRows(DsEvents.ChildEvent);
                    foreach (DataRow rowReset in rowResets)
                    {
                        if (EventCodeFunc.IsReset(rowReset["EVENTCODE"].ToString()))
                        {
                            DataRow[] rowSelfAverages = rowReset.GetChildRows(DsEvents.ChildEvent);
                            foreach (DataRow rowSelfAverage in rowSelfAverages)
                            {
                                if (EventCodeFunc.IsSelfAverage(rowSelfAverage["EVENTCODE"].ToString()))
                                {
                                    DataRow[] rowSelfResets = rowSelfAverage.GetChildRows(DsEvents.ChildEvent);
                                    foreach (DataRow rowSelfReset in rowSelfResets)
                                    {
                                        if (EventCodeFunc.IsSelfReset(rowSelfReset["EVENTCODE"].ToString()))
                                        {
                                            int idESelfReset = Convert.ToInt32(rowSelfReset["IDE"]);
                                            DataRow[] rowsAssetSelfReset = GetRowAsset(idESelfReset);
                                            if (ArrFunc.IsFilled(rowsAssetSelfReset) && rowsAssetSelfReset[0]["IDYIELDCURVEVAL_H"] != Convert.DBNull)
                                            {
                                                /* Pas d'insertion au delà du RESET
                                                /*
                                                AddRowEventPricing2(idESelfReset);
                                                DataRow[] rows = GetRowEventPricing2(idESelfReset);
                                                //
                                                if (ArrFunc.IsFilled(rows))
                                                {
                                                    rows[0]["IDYIELDCURVEVAL_H"] = rowsAssetSelfReset[0]["IDYIELDCURVEVAL_H"];
                                                    rows[0]["ZEROCOUPON1"] = rowsAssetSelfReset[0]["ZEROCOUPON1"];
                                                    rows[0]["ZEROCOUPON2"] = rowsAssetSelfReset[0]["ZEROCOUPON2"];
                                                    rows[0]["FORWARDRATE"] = rowsAssetSelfReset[0]["FORWARDRATE"];
                                                    rows[0]["RATE"] = rowsAssetSelfReset[0]["RATE"];
                                                }
                                                */
                                            }
                                        }
                                    }
                                }
                            }
                            //
                            int idEReset = Convert.ToInt32(rowReset["IDE"]);
                            DataRow[] rowsAssetReset = GetRowAsset(idEReset);
                            if (ArrFunc.IsFilled(rowsAssetReset) && rowsAssetReset[0]["IDYIELDCURVEVAL_H"] != Convert.DBNull)
                            {
                                AddRowEventPricing2(idEReset);
                                DataRow[] rows = GetRowEventPricing2(idEReset);
                                if (ArrFunc.IsFilled(rows))
                                {
                                    rows[0]["FLOWTYPE"] = "reset";
                                    rows[0]["IDYIELDCURVEVAL_H"] = rowsAssetReset[0]["IDYIELDCURVEVAL_H"];
                                    rows[0]["ZEROCOUPON1"] = rowsAssetReset[0]["ZEROCOUPON1"];
                                    rows[0]["ZEROCOUPON2"] = rowsAssetReset[0]["ZEROCOUPON2"];
                                    rows[0]["FORWARDRATE"] = rowsAssetReset[0]["FORWARDRATE"];
                                    rows[0]["RATE"] = rowsAssetReset[0]["RATE"];
                                }
                            }
                        }
                    }
                    if (EventTypeFunc.IsFloatingRate(rowChild["EVENTCODE"].ToString()))
                    {
                        int idE = Convert.ToInt32(rowChild["IDE"]);
                        AddRowEventPricing2(idE);
                        DataRow[] rows = GetRowEventPricing2(idE);
                        if (ArrFunc.IsFilled(rows))
                        {
                            rows[0]["FLOWTYPE"] = "interest";
                            rows[0]["METHOD"] = EventClassEnum.FRP.ToString();
                            rows[0]["CASHFLOW"] = rowChild["VALORISATION"];
                        }
                    }
                }
            }
        }
        #endregion AddEventPricingForInterest
        #region AddEventPricingForInterest
        /// <summary>
        /// Ajout des informations détail pour le log
        /// </summary>
        /// <param name="pRowInterest"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void AddLogDetailPricingForInterest(DataRow pRowInterest)
        {
            
            DataRow[] rowChilds = pRowInterest.GetChildRows(DsEvents.ChildEvent);
            foreach (DataRow rowChild in rowChilds)
            {
                if (EventCodeFunc.IsCalculationPeriod(rowChild["EVENTCODE"].ToString()))
                {
                    DataRow[] rowResets = rowChild.GetChildRows(DsEvents.ChildEvent);
                    foreach (DataRow rowReset in rowResets)
                    {
                        if (EventCodeFunc.IsReset(rowReset["EVENTCODE"].ToString()))
                        {
                            DataRow[] rowSelfAverages = rowReset.GetChildRows(DsEvents.ChildEvent);
                            bool isExistSeftAverages = ArrFunc.IsFilled(rowSelfAverages);
                            SysMsgCode sysNumber;
                            foreach (DataRow rowSelfAverage in rowSelfAverages)
                            {
                                if (EventCodeFunc.IsSelfAverage(rowSelfAverage["EVENTCODE"].ToString()))
                                {
                                    DataRow[] rowSelfResets = rowSelfAverage.GetChildRows(DsEvents.ChildEvent);
                                    foreach (DataRow rowSelfReset in rowSelfResets)
                                    {
                                        if (EventCodeFunc.IsSelfReset(rowSelfReset["EVENTCODE"].ToString()))
                                        {
                                            bool isExistingSelfReset = BoolFunc.IsTrue(rowSelfReset["ISEXISTING"]);
                                            int idESelfReset = Convert.ToInt32(rowSelfReset["IDE"]);
                                            //
                                            if (isExistingSelfReset)
                                            {
                                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 620), 2,
                                                    new LogParam(LogTools.IdentifierAndId(Convert.ToString(rowSelfReset["EVENTCODE"]) + " / " +
                                                        Convert.ToString(rowSelfReset["EVENTTYPE"]), Convert.ToInt32(rowSelfReset["IDE"]))),
                                                    new LogParam(DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowSelfReset["DTSTARTUNADJ"])) + " / " +
                                                        DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowSelfReset["DTENDUNADJ"]))),
                                                    new LogParam(LogTools.AmountAndCurrency(Convert.ToDecimal(rowSelfReset["VALORISATION"]), string.Empty))));
                                            }
                                            else
                                            {
                                                DataRow[] rowsAssetSelfReset = GetRowAsset(idESelfReset);
                                                if (ArrFunc.IsFilled(rowsAssetSelfReset))
                                                {
                                                    int idAsset = Convert.ToInt32(rowsAssetSelfReset[0]["IDASSET"]);
                                                    SQL_AssetRateIndex sqlRateIndex = new SQL_AssetRateIndex(m_CS, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                                                    sqlRateIndex.LoadTable();
                                                    
                                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 618), 2,
                                                        new LogParam(LogTools.IdentifierAndId(sqlRateIndex.Identifier, sqlRateIndex.Id)),
                                                        new LogParam(LogTools.IdentifierAndId(Convert.ToString(rowSelfReset["EVENTCODE"]) + " / " +
                                                            Convert.ToString(rowSelfReset["EVENTTYPE"]), Convert.ToInt32(rowSelfReset["IDE"]))),
                                                        new LogParam(DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowSelfReset["DTSTARTUNADJ"])) + " / " +
                                                            DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowSelfReset["DTENDUNADJ"])))));
                                                    
                                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 619), 2,
                                                        new LogParam(LogTools.IdentifierAndId(sqlRateIndex.Identifier, sqlRateIndex.Id)),
                                                        new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowsAssetSelfReset[0]["RATE"]))),
                                                        new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowsAssetSelfReset[0]["ZEROCOUPON1"])) + " / " +
                                                            StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowsAssetSelfReset[0]["ZEROCOUPON2"]))),
                                                        new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowsAssetSelfReset[0]["FORWARDRATE"]))),
                                                        new LogParam(StrFunc.FmtIntegerToInvariantCulture(Convert.ToInt32(rowsAssetSelfReset[0]["IDYIELDCURVEVAL_H"])))));
                                                }
                                            }
                                        }
                                    }
                                }

                                bool isExistingSelfAverage = BoolFunc.IsTrue(rowSelfAverage["ISEXISTING"]);
                                int idESelfAverage = Convert.ToInt32(rowSelfAverage["IDE"]);

                                //sysNumber = "LOG-00621";
                                sysNumber = new SysMsgCode(SysCodeEnum.LOG, 621);
                                if (isExistingSelfAverage)
                                {
                                    //sysNumber = "LOG-00620";
                                    sysNumber = new SysMsgCode(SysCodeEnum.LOG, 620);
                                }

                                Logger.Log(new LoggerData(LogLevelEnum.Debug, sysNumber, 2,
                                    new LogParam(LogTools.IdentifierAndId(Convert.ToString(rowSelfAverage["EVENTCODE"]) + " / " +
                                        Convert.ToString(rowSelfAverage["EVENTTYPE"]), Convert.ToInt32(rowSelfAverage["IDE"]))),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowSelfAverage["DTSTARTUNADJ"])) + " / " +
                                        DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowSelfAverage["DTENDUNADJ"]))),
                                    new LogParam(LogTools.AmountAndCurrency(Convert.ToDecimal(rowSelfAverage["VALORISATION"]), string.Empty))));
                            }

                            int idEReset = Convert.ToInt32(rowReset["IDE"]);
                            bool isExistingReset = BoolFunc.IsTrue(rowReset["ISEXISTING"]);
                            if (isExistingReset || isExistSeftAverages)
                            {
                                //sysNumber = "LOG-00621";
                                sysNumber = new SysMsgCode(SysCodeEnum.LOG, 621);
                                if (isExistingReset)
                                {
                                    //sysNumber = "LOG-00620";
                                    sysNumber = new SysMsgCode(SysCodeEnum.LOG, 620);
                                }
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, sysNumber, 2,
                                    new LogParam(LogTools.IdentifierAndId(Convert.ToString(rowReset["EVENTCODE"]) + " / " +
                                        Convert.ToString(rowReset["EVENTTYPE"]), Convert.ToInt32(rowReset["IDE"]))),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowReset["DTSTARTUNADJ"])) + " / " +
                                        DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowReset["DTENDUNADJ"]))),
                                    new LogParam(LogTools.AmountAndCurrency(Convert.ToDecimal(rowReset["VALORISATION"]), string.Empty))));
                            }
                            else
                            {
                                DataRow[] rowsAssetReset = GetRowAsset(idEReset);
                                if (ArrFunc.IsFilled(rowsAssetReset))
                                {
                                    foreach (DataRow rowItem in rowsAssetReset) 
                                    {
                                        int idAsset = Convert.ToInt32(rowItem["IDASSET"]);
                                        SQL_AssetRateIndex sqlRateIndex = new SQL_AssetRateIndex(m_CS, SQL_AssetRateIndex.IDType.IDASSET, idAsset);
                                        sqlRateIndex.LoadTable();
                                        //
                                        //Si le taux est le réultat d'ine interpolation on a 2 lignes
                                        //la valeur d'un des 2 assets pour être connu et l'autre peut être estimé
                                        if (Convert.DBNull != rowItem["IDYIELDCURVEVAL_H"])
                                        {
                                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 618), 2,
                                                new LogParam(LogTools.IdentifierAndId(sqlRateIndex.Identifier, sqlRateIndex.Id)),
                                                new LogParam(LogTools.IdentifierAndId(Convert.ToString(rowReset["EVENTCODE"]) + " / " +
                                                    Convert.ToString(rowReset["EVENTTYPE"]), Convert.ToInt32(rowReset["IDE"]))),
                                                new LogParam(DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowReset["DTSTARTUNADJ"])) + " / " +
                                                    DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(rowReset["DTENDUNADJ"])))));

                                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 619), 2,
                                                new LogParam(LogTools.IdentifierAndId(sqlRateIndex.Identifier, sqlRateIndex.Id)),
                                                new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowItem["RATE"]))),
                                                new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowItem["ZEROCOUPON1"])) + " / " +
                                                    StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowItem["ZEROCOUPON2"]))),
                                                new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowItem["FORWARDRATE"]))),
                                                new LogParam(StrFunc.FmtIntegerToInvariantCulture(Convert.ToInt32(rowItem["IDYIELDCURVEVAL_H"])))));
                                        }
                                        else
                                        {
                                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 622), 2,
                                                new LogParam(LogTools.IdentifierAndId(sqlRateIndex.Identifier, sqlRateIndex.Id)),
                                                new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowItem["RATE"])))));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion AddEventPricingForInterest
        #region CalcActualizedAmount
        // EG 20150608 [21011] CompareTo
        private static Decimal ConstructMarkToMarket(decimal pMarkToMarket, decimal pActualizedAmount, PayerReceiverInfo pPayRecInfoItem, PayerReceiverInfo pPayRecInfoStream)
        {
            decimal ret;
            if (pPayRecInfoItem.CompareTo(pPayRecInfoStream) == 0)
                ret = pMarkToMarket + pActualizedAmount;
            else
                ret = pMarkToMarket - pActualizedAmount;
            return ret;
        }
        #endregion
        #region isStreamInterest
        /// <summary>
        /// Obtient true si l'eventCode représente un stream avec intérêts
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        private static bool IsStreamInterest(string pEventCode)
        {
            bool ret =
            EventCodeFunc.IsForwardRateAgreement(pEventCode) ||
            EventCodeFunc.IsInterestRateSwap(pEventCode) ||
            EventCodeFunc.IsLoanDeposit(pEventCode) ||
            EventCodeFunc.IsTermDeposit(pEventCode);
            return ret;
        }
        #endregion IsStreamInterest
        #endregion Methods
    }
	#endregion MarkToMarketGenProcessIRD
}

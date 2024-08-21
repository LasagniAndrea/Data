#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresService;
//
using EfsML;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.MarkToMarket
{
    #region MarkToMarketGenProcessFxOption
    public class MarkToMarketGenProcessFxOption : MarkToMarketGenProcessFxBase
    {
        #region Members
        private DateTime m_ExpiryDate;
        private IMoney m_MarkToMarketAmount;
        #endregion Members
        #region Constructors
        public MarkToMarketGenProcessFxOption(MarkToMarketGenProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct) { }
        #endregion Constructors
        #region Methods
        #region Valorize
        /// <summary>
        /// Master process to valorize a MarkToMarket amount for an FxSimpleOption
        /// </summary>
        /// <returns></returns>
        // EG 20150317 [POC] use MtMEventCodes
        // EG 20150317 [POC] Test m_MarkToMarketGenMQueue.isEOD (EVT = MGR enfant de LPC/AMT) sinon inchangé (EVT = MTM)
        // EG 20150407 [POC] Seule la méthode MarkToMarket gérée
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use slaveDbTransaction
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                DataRow[] rowEventProducts = GetRowEventProducts();
                foreach (DataRow rowEventProduct in rowEventProducts)
                {
                    dbTransaction = DataHelper.BeginTran(m_MarkToMarketGenProcess.Cs);

                    DataRow[] rowChilds = rowEventProduct.GetChildRows(DsEvents.ChildEvent);
                    foreach (DataRow rowChild in rowChilds)
                    {
                        DataRow rowCandidate = null;

                        string eventCode = rowChild["EVENTCODE"].ToString();
                        string eventType = rowChild["EVENTTYPE"].ToString();
                        m_ExpiryDate = Convert.ToDateTime(rowChild["DTENDUNADJ"]);
                        //
                        bool isRowMustBeCalculate = IsRowMustBeCalculate(rowChild);
                        if (isRowMustBeCalculate)
                            isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_MarkToMarketGenProcess.ScanCompatibility_Event(Convert.ToInt32(rowChild["IDE"])));

                        if (isRowMustBeCalculate)
                        {

                            m_ParamInstrumentNo.Value = Convert.ToInt32(rowChild["INSTRUMENTNO"]);
                            m_ParamStreamNo.Value = Convert.ToInt32(rowChild["STREAMNO"]);

                            MarkToMarketFxInputData fxInputData = GetFxInputData(eventCode);
                            rowCandidate = fxInputData.RowCandidate;

                            // EG 20150325 [POC] Pas de MGR si Achat d'option
                            if (false == IsDealerBuyer)
                            {

                                DataRowEvent rowInfo = new DataRowEvent(rowChild);
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 611), 1,
                                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                    new LogParam(LogTools.IdentifierAndId(rowInfo.EventCode + " / " + rowInfo.EventType, rowInfo.Id))));

                                m_MarkToMarketAmount = MarkToMarketFxOption(fxInputData);

                                // EG 20150325 [POC] 
                                PayerReceiverInfo payerReceiverInfoCur = new PayerReceiverInfo(this,
                                    (fxInputData.PutCurrency == fxInputData.PremiumCurrency) ? RowStartPutCurrency : RowStartCallCurrency);

                                PayerReceiverInfoDet payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer,
                                    payerReceiverInfoCur.Payer, null, payerReceiverInfoCur.BookPayer, null);
                                PayerReceiverInfoDet receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Payer,
                                    payerReceiverInfoCur.Receiver, null, payerReceiverInfoCur.BookReceiver, null);

                                if (null != rowCandidate)
                                {
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 617), 2,
                                        new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                                        new LogParam(MtMEventCodes.First + " / " + MtMEventCodes.Second + " / " + fxInputData.PricingMethod),
                                        new LogParam((null != m_MarkToMarketAmount) ? LogTools.AmountAndCurrency(m_MarkToMarketAmount.Amount.DecValue, m_MarkToMarketAmount.Currency) : "NaN")));

                                    if (m_MarkToMarketGenMQueue.isEOD)
                                    {
                                        // EG 20150402 MGR
                                        #region MarginRequirement
                                        bool isDealerBuyer = m_RptSideProductContainer.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER);
                                        Pair<Nullable<decimal>,Nullable<decimal>> marginRatio = GetMarginRatio(dbTransaction, m_RptSideProductContainer, CommonValDate);
                                        if (marginRatio.First.HasValue)
                                        {
                                            rowCandidate = GetRowAmountGroup();
                                            if (null == rowCandidate)
                                            {
                                                DateTime dtStart = Convert.ToDateTime(rowChild["DTSTARTADJ"]);
                                                rowCandidate = NewRowEvent(SlaveDbTransaction, rowChild, EventCodeFunc.LinkedProductClosing, EventTypeFunc.Amounts, dtStart,
                                                    Convert.ToDateTime(rowChild["DTENDADJ"]), m_MarkToMarketGenProcess.AppInstance);
                                                m_DsEvents.DtEvent.Rows.Add(rowCandidate);
                                                DataRow rowEventClass = NewRowEventClass(Convert.ToInt32(rowCandidate["IDE"]), EventClassFunc.GroupLevel, dtStart, false);
                                                m_DsEvents.DtEventClass.Rows.Add(rowEventClass);
                                            }

                                            // UMG
                                            AddRowUnrealizedMargin(dbTransaction, rowCandidate, m_MarkToMarketAmount.Amount.DecValue);

                                            payer.actor = new Pair<Nullable<int>, string>(m_Seller.OTCmlId, m_Seller.PartyName);
                                            // EG 20150706 [21021]
                                            payer.book = new Pair<Nullable<int>,string>(m_BookSeller, string.Empty);
                                            receiver.actor = new Pair<Nullable<int>, string>(m_Buyer.OTCmlId, m_Buyer.PartyName);
                                            // EG 20150706 [21021]
                                            receiver.book = new Pair<Nullable<int>, string>(m_BookBuyer, string.Empty);

                                            // LOV = MTM
                                            // EG 20150608 [21011] payer|receiver remplace  payerReceiverInfoCur 
                                            AddRowLiquidationOptionValue(rowCandidate, m_MarkToMarketAmount.Amount.DecValue, m_MarkToMarketAmount.Currency,
                                                EventClassFunc.Recognition, payer, receiver);
                                            // MGR
                                            m_MarkToMarketAmount.Amount.DecValue = Math.Abs(m_MarkToMarketAmount.Amount.DecValue * marginRatio.First.Value);
                                            Nullable<decimal> initialMargin = InitialMarginAmount;
                                            m_MarkToMarketAmount.Amount.DecValue = Math.Max(m_MarkToMarketAmount.Amount.DecValue, initialMargin ?? 0);
                                        }
                                        else
                                        {
                                            // EG 20150402 [POC] Pas de margin Factor
                                            #region Log error message
                                            ProcessState _processState = new ProcessState(ProcessStateTools.StatusErrorEnum);

                                            // FI 20200623 [XXXXX] SetErrorWarning
                                            m_MarkToMarketGenProcess.ProcessState.SetErrorWarning(_processState.Status);

                                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 680), 2,
                                                new LogParam(isDealerBuyer ? LogTools.IdentifierAndId(m_Buyer.PartyName, m_Buyer.OTCmlId) : LogTools.IdentifierAndId(m_Seller.PartyName, m_Seller.OTCmlId)),
                                                new LogParam(isDealerBuyer ? LogTools.IdentifierAndId(m_Seller.PartyName, m_Seller.OTCmlId) : LogTools.IdentifierAndId(m_Buyer.PartyName, m_Buyer.OTCmlId)),
                                                new LogParam(LogTools.IdentifierAndId(m_MarkToMarketGenMQueue.GetStringValueIdInfoByKey("identifier"), m_DsTrade.IdT))));

                                            throw new SpheresException2(_processState);
                                            #endregion Log error message
                                        }
                                        #endregion MarginRequirement
                                    }
                                    else
                                    {
                                        // EG 20150402 MTM
                                    }

                                    // EG 20150608 [21011]
                                    //AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount, fxInputData.PricingMethod, payerReceiverInfoCur);
                                    if (null != m_MarkToMarketAmount)
                                        AddRowMarkToMarket(rowCandidate, m_MarkToMarketAmount.Amount.DecValue, m_MarkToMarketAmount.Currency, fxInputData.PricingMethod, payer, receiver);
                                    else
                                        AddRowMarkToMarket(rowCandidate, null, string.Empty, fxInputData.PricingMethod, payer, receiver);
                                }
                            }

                            Update(dbTransaction, Convert.ToInt32(rowCandidate["IDE"]), false);
                        }
                    }
                    DataHelper.CommitTran(dbTransaction);
                }
                dbTransaction = null;

            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (isException))
                {
                    try
                    {
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    catch { }
                }
            }
            return ret;
        }
        #endregion Valorize
        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            string eventCode = pRow["EVENTCODE"].ToString();
            return (EventCodeFunc.IsSimpleOption(eventCode) && (0 < m_ExpiryDate.CompareTo(CommonValDate)));
        }
        #endregion
        #region MarkToMarketFxOption
        /// <summary>
        /// <para>Processus de calcul du MTM</para>
        /// 
        /// <para>Exemple : Put USD – Call INR, Prime en USD</para>
        /// <para>1. Identification de la devise de prime = USD</para>
        /// <para>2. Recherche de l’option Call ou Put, dont la devise est identique à la devise de prime = Put USD</para>
        /// <para>3. Calcul des taux des 2 devises</para>
        /// <para>        DomesticInterest = Prix de la devise à valoriser (PricingCurrency = INR)</para>
        /// <para>        ForeignInterest  = Prix de la devise de prime (PremiumCurrency = USD)</para>
        /// <para>4. Calcul de la volatilité (via Courbe mutli-dimensionnelle)</para>
        /// <para>5. Lecture du spot jour (exchangeRate)</para>
        /// <para>6. Calcul du prix théorique de l’autre option = Call INR</para>
        /// <para>	    Ce prix est côté en devise de l’autre option mais en tant que pourcentage du montant de l’option valorisé.</para>
        /// <para>	    Prime en Put Currency = Call price * Call Amount (Prime en USD = CallPrice x INT Amount)</para>
        /// <para>	    Spot et Strike côté pour 1 unité de la devise de l’option que l’on souhaite valoriser (INR per 1 USD donc ex 1/62.50 = 0.016)</para>
        /// </summary>
        /// <param name="pFxInputData">Données en entrée pour valorisation</param>
        /// <returns></returns>
        protected IMoney MarkToMarketFxOption(MarkToMarketFxInputData pFxInputData)
        {
            IMoney mtmAmount = null;
            const int binomialSteps = 100; //0; // Nombre de pas pour la méthode binomial : arbitrairement fixé

            PayerReceiverInfo payerReceiverInfo = new PayerReceiverInfo((CommonValProcessBase)this, pFxInputData.RowCandidate);

            //InterestRate calculation (with yieldCurve)

            Nullable<decimal> domesticInterest = InterestRate(pFxInputData.PricingCurrency, m_ExpiryDate, payerReceiverInfo);
            Nullable<decimal> foreignInterest = InterestRate(pFxInputData.PremiumCurrency, m_ExpiryDate, payerReceiverInfo);
            
            //Volatility Calculation (with multi-dimensional Matrix)
            int idAsset = pFxInputData.GetDefaultAsset(CSTools.SetCacheOn(m_CS));
            if (idAsset == 0)
                throw new Exception(StrFunc.AppendFormat("No defaut asset found for cur1:{0}/cur2:{1}", pFxInputData.PremiumCurrency, pFxInputData.PricingCurrency));
            
            decimal volatility = Volatility(idAsset, m_ExpiryDate, pFxInputData.StrikePrice.Rate.DecValue, payerReceiverInfo);

            // EG 20150319 [POC] pFxInputData.QuoteBasis
            Nullable<decimal> exchangeRate = GetFxRate(payerReceiverInfo, CommonValDate, pFxInputData.PremiumCurrency, pFxInputData.PricingCurrency, pFxInputData.QuoteBasis);

            IInterval interval = ProductBase.CreateInterval(PeriodEnum.D, 0);
            EFS_DayCountFraction dcf = new EFS_DayCountFraction(CommonValDate, m_ExpiryDate.AddDays(-1), DayCountFractionEnum.ACTACTISDA, interval);
            if (EventClassFunc.IsGarman_Kolhagen(pFxInputData.PricingMethod))
            {
                m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(exchangeRate, pFxInputData.StrikePrice.Rate.DecValue,
                    domesticInterest, foreignInterest, dcf, volatility, pFxInputData.IsInverse);
            }
            else if (EventClassFunc.IsCoxRossRubinstein(pFxInputData.PricingMethod))
            {
                m_PricingValues = MarkToMarketTools.BinomialTrees(pFxInputData.ExerciseStyle, exchangeRate.Value, pFxInputData.StrikePrice.Rate.DecValue,
                    domesticInterest.Value, foreignInterest.Value, dcf, volatility, binomialSteps);
            }
            else
                throw new NotImplementedException("Mark To Market Pricing Method Error (" + pFxInputData.PricingMethod + ")");

            #region MarkToMarket Amount calculation
            if (exchangeRate.HasValue && domesticInterest.HasValue && foreignInterest.HasValue)
            {
                m_PricingValues.Currency1 = pFxInputData.PremiumCurrency;
                m_PricingValues.Currency2 = pFxInputData.PricingCurrency;
                // Prime en Domestic currency = Prix théorique * Foreign Amount
                decimal amount = pFxInputData.NotionalReference * ((pFxInputData.PricingCurrency == pFxInputData.CallCurrency) ? m_PricingValues.CallPrice : m_PricingValues.PutPrice);
                EFS_Cash cashAmount = new EFS_Cash(m_CS, amount, pFxInputData.PremiumCurrency);
                mtmAmount = ProductBase.CreateMoney(cashAmount.AmountRounded, pFxInputData.PremiumCurrency);

            }
            return mtmAmount;
            #endregion MarkToMarket Amount calculation
        }
        #endregion MarkToMarketFxOption

        #region TestMTM
        /*
        private void TestMTM(decimal pExchangeRate, decimal pStrike, decimal pInterestRate1, decimal pInterestRate2, EFS_DayCountFraction pDCF, decimal pVolatility)
        {
            System.Diagnostics.Debug.WriteLine("START");
            // PUT
            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(pExchangeRate, pStrike, pInterestRate1, pInterestRate2, pDCF, pVolatility, true);
            System.Diagnostics.Debug.WriteLine("V1 : Cours Inverse");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            EFS_Cash cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.CallPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.PutPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("EUR CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.CallPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            System.Diagnostics.Debug.WriteLine("EUR PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.PutPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            // CALL
            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(pExchangeRate, pStrike, pInterestRate1, pInterestRate2, pDCF, pVolatility, false);
            System.Diagnostics.Debug.WriteLine("V2 : Cours certain");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.CallPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.PutPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.CallPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.PutPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            // CALL
            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(pExchangeRate, pStrike, pInterestRate2, pInterestRate1, pDCF, pVolatility, true);
            System.Diagnostics.Debug.WriteLine("V3 : cours inverse et Taux domestic/foreign inversé ");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.CallPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.PutPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.CallPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.PutPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");
            
            // PUT
            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(pExchangeRate, pStrike, pInterestRate2, pInterestRate1, pDCF, pVolatility, false);
            System.Diagnostics.Debug.WriteLine("V4 : cours certain et Taux domestic/foreign inversé ");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.CallPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString()); 
            cashAmount = new EFS_Cash(m_CS, 1000000m * m_PricingValues.PutPrice, "USD");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " USD");

            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.CallPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.Write(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
            cashAmount = new EFS_Cash(m_CS, 925925.93m * m_PricingValues.PutPrice, "EUR");
            System.Diagnostics.Debug.WriteLine("MTM : " + cashAmount.AmountRounded.ToString() + " EUR");
            cashAmount = new EFS_Cash(m_CS, cashAmount.AmountRounded * pExchangeRate, "EUR");
            System.Diagnostics.Debug.WriteLine(" (" + cashAmount.AmountRounded.ToString() + " USD)");

            // Source : http://www.derivativepricing.com/blogpage.asp?id=22
            IInterval interval = ProductBase.CreateInterval(PeriodEnum.D, 0);
            DateTime valDate = new DtFunc().StringToDateTime("24/12/2009");
            DateTime expDate = new DtFunc().StringToDateTime("07/01/2010");
            EFS_DayCountFraction dcf = new EFS_DayCountFraction(valDate, expDate, DayCountFractionEnum.ACTACTISDA, interval);

            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(1.599m, 1.58m, 0.0042m, 0.0025m, dcf, 0.1m, false);
            System.Diagnostics.Debug.WriteLine("Put : GBP");
            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());

            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(1.599m, 1.58m, 0.0025m, 0.0042m, dcf, 0.1m, true);
            System.Diagnostics.Debug.WriteLine("Call : USD");
            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString()); 
        }
        */
        /*
        private void Test2()
        {
            System.Diagnostics.Debug.WriteLine("START");

            // Source : http://www.derivativepricing.com/blogpage.asp?id=22
            IInterval interval = ProductBase.CreateInterval(PeriodEnum.D, 0);
            DateTime valDate = new DtFunc().StringToDateTime("03/03/2015");
            DateTime expDate = new DtFunc().StringToDateTime("27/03/2015");
            EFS_DayCountFraction dcf = new EFS_DayCountFraction(valDate, expDate, DayCountFractionEnum.ACTACTISDA, interval);

            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(61.82107m, 62.50m, 0.0584809636666667M, 0.0015818338M, dcf, 0.1m, false);
            System.Diagnostics.Debug.WriteLine("Put : USD");
            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());

            m_PricingValues = MarkToMarketTools.GarmanAndKolhagen(61.82107m, 62.50m, 0.0015818338M, 0.0584809636666667M, dcf, 0.1m, true);
            System.Diagnostics.Debug.WriteLine("Call : USD");
            System.Diagnostics.Debug.WriteLine("USD CallPrice : " + m_PricingValues.CallPrice.ToString());
            System.Diagnostics.Debug.WriteLine("USD PutPrice : " + m_PricingValues.PutPrice.ToString());
        }
        */
        #endregion TestMTM

        #region Volatility
        /// <summary>
        /// Read the volatility in the matrix created for the date CommonValDate with respect to the given Book, Currency, Expiry and Strike.
        /// </summary>
        /// <param name="pIdC">Currency Id</param>
        /// <param name="pDtExpiry">Expiry date (date of the required volatility)</param>
        /// <param name="pStrike"> Strike value</param>
        /// <returns></returns>
        // EG 20150706 [21021] Pair<int,Nullable<int>> for payer|receiver
        protected decimal Volatility(string pIdC, DateTime pDtExpiry, decimal pStrike, PayerReceiverInfo pPayerReceiverInfo)
        {
            decimal volatility = 0m;

            Pair<int, Nullable<int>> payer = new Pair<int, Nullable<int>>(pPayerReceiverInfo.Payer, pPayerReceiverInfo.BookPayer);
            Pair<int, Nullable<int>> receiver = new Pair<int, Nullable<int>>(pPayerReceiverInfo.Receiver, pPayerReceiverInfo.BookReceiver);

            MultiDimMatrix multiDimMatrix = new MultiDimMatrix(m_CS, ProductBase,
                payer, receiver, CommonValDate, pIdC, null);

            if (multiDimMatrix.IsLoaded(m_CS, Product.ProductBase))
                volatility = Convert.ToDecimal(multiDimMatrix.GetPointValue(m_CS, pDtExpiry, Convert.ToDouble(pStrike)));

            return volatility;
        }
        /// <summary>
        /// Read the volatility in the matrix created for the date CommonValDate with respect to the given Book, asset, Expiry and Strike.
        /// </summary>
        /// <param name="idAsset">idAsset</param>
        /// <param name="pDtExpiry">Expiry date (date of the required volatility)</param>
        /// <param name="pStrike"> Strike value</param>
        /// <returns></returns>
        // EG 20150706 [21021] Pair<int,Nullable<int>> for payer|receiver
        protected decimal Volatility(int idAsset, DateTime pDtExpiry, decimal pStrike, PayerReceiverInfo pPayerReceiverInfo)
        {
            decimal volatility = 0m;


            Pair<Cst.UnderlyingAsset, int> asset = new Pair<Cst.UnderlyingAsset, int>(Cst.UnderlyingAsset.FxRateAsset, idAsset);
            // EG 20150706 [21021]
            Pair<int, Nullable<int>> payer = new Pair<int, Nullable<int>>(pPayerReceiverInfo.Payer, pPayerReceiverInfo.BookPayer);
            Pair<int, Nullable<int>> receiver = new Pair<int, Nullable<int>>(pPayerReceiverInfo.Receiver, pPayerReceiverInfo.BookReceiver);

            MultiDimMatrix multiDimMatrix = new MultiDimMatrix(m_CS, ProductBase, payer, receiver, CommonValDate, null, asset);

            if (multiDimMatrix.IsLoaded(m_CS, Product.ProductBase))
                volatility = Convert.ToDecimal(multiDimMatrix.GetPointValue(m_CS, pDtExpiry, Convert.ToDouble(pStrike)));

            return volatility;
        }
        #endregion Volatility

        #region GetFxInputData
        // EG 20150324 [POC]
        private MarkToMarketFxInputData GetFxInputData(string pEventCode)
        {
            StrikeQuoteBasisEnum strikeQuoteBasis = StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency;
            MarkToMarketFxInputData fxInputData = null;
            DataRow rowEventDetail = GetRowEventDetail(Convert.ToInt32(RowStartCallCurrency["IDE"]));
            
            if (null != rowEventDetail)
            {
                decimal strikePrice = Convert.ToDecimal(rowEventDetail["STRIKEPRICE"]);
                
                string basis = (Convert.IsDBNull(rowEventDetail["BASIS"])) ? string.Empty : rowEventDetail["BASIS"].ToString();
                if (System.Enum.IsDefined(typeof(StrikeQuoteBasisEnum), basis))
                    strikeQuoteBasis = (StrikeQuoteBasisEnum)System.Enum.Parse(typeof(StrikeQuoteBasisEnum), basis, true);
                
                IFxStrikePrice strike = m_tradeLibrary.Product.ProductBase.CreateStrikePrice();
                strike.Rate = new EFS.GUI.Interface.EFS_Decimal(strikePrice);
                strike.StrikeQuoteBasis  = strikeQuoteBasis;

                #region ExerciseStyle & PricingMethod
                ExerciseStyleEnum exerciseStyle = ExerciseStyleEnum.European;
                string pricingMethod;
                if (EventCodeFunc.IsEuropeanSimpleOption(pEventCode))
                {
                    exerciseStyle = ExerciseStyleEnum.European;
                    pricingMethod = EventClassFunc.Garman_Kolhagen;
                }
                else
                {
                    if (EventCodeFunc.IsAmericanSimpleOption(pEventCode))
                        exerciseStyle = ExerciseStyleEnum.American;
                    else if (EventCodeFunc.IsBermudaSimpleOption(pEventCode))
                        exerciseStyle = ExerciseStyleEnum.Bermuda;
                    pricingMethod = EventClassFunc.CoxRossRubinstein;
                }
                #endregion ExerciseStyle & PricingMethod

                #region InputData setting
                fxInputData = new MarkToMarketFxInputData(pricingMethod, exerciseStyle, RowStartCallCurrency, RowStartPutCurrency, RowPremium, strike);
                #endregion InputData setting
            }
            return fxInputData;
        }
        #endregion GetFxInputData

        #region AddRowUnrealizedMargin
        // EG 20150706 [21021] Nullable<int> for book (payer|receiver)
        private void AddRowUnrealizedMargin(IDbTransaction pDbTransaction, DataRow pRowAMT, decimal pMarkToMarketAmount)
        {
            // EG 20150408 
            IFxOptionPremium fxOptionPremium = ((FxOptionLegContainer)m_RptSideProductContainer).FxOptionLeg.FxOptionPremium[0];
            IPremiumQuote fxPremiumQuote = fxOptionPremium.PremiumQuote;
            decimal premiumQuote = fxOptionPremium.PremiumQuote.PremiumValue.DecValue;
            IMoney premiumAmount = fxOptionPremium.PremiumAmount;
            decimal theoreticalPrice = 0;
            // EG 20160404 Migration vs2013
            //DataRow rowCurrency = null;
            Pair<Nullable<decimal>, string> unrealizedMarginAmount = new Pair<decimal?,string>(pMarkToMarketAmount - premiumAmount.Amount.DecValue, premiumAmount.Currency);
            // EG 20150706 [21021]
            Pair<int, Nullable<int>> payer = new Pair<int, Nullable<int>>();
            Pair<int, Nullable<int>> receiver = new Pair<int, Nullable<int>>();
            payer.First = (0 < unrealizedMarginAmount.First.Value) ? m_Seller.OTCmlId : m_Buyer.OTCmlId;
            payer.Second = (0 < unrealizedMarginAmount.First.Value) ? m_BookSeller : m_BookBuyer;
            receiver.First = (0 < unrealizedMarginAmount.First.Value) ? m_Buyer.OTCmlId : m_Seller.OTCmlId;
            receiver.Second = (0 < unrealizedMarginAmount.First.Value) ? m_BookBuyer : m_BookSeller;

            switch (fxPremiumQuote.PremiumQuoteBasis)
            {
                case PremiumQuoteBasisEnum.PercentageOfCallCurrencyAmount:
                    theoreticalPrice = m_PricingValues.CallPrice;
                    break;
                case PremiumQuoteBasisEnum.PercentageOfPutCurrencyAmount:
                    theoreticalPrice = m_PricingValues.PutPrice;
                    break;
                default:
                    break;
            }
            if (0 < theoreticalPrice)
                AddRowUnrealizedMargin(pDbTransaction, pRowAMT, unrealizedMarginAmount.First, unrealizedMarginAmount.Second, premiumQuote, theoreticalPrice, payer, receiver);
        }
        #endregion AddRowUnrealizedMargin

        #endregion Methods
    }
    #endregion MarkToMarketGenProcessFxOption

    #region MarkToMarketFxInputData
    public class MarkToMarketFxInputData
    {
        
        #region Accessors
        public bool IsInverse { private set; get; }
        public string CallCurrency { private set; get; }
        public string PutCurrency { private set; get; }
        /// <summary>
        /// <para>Est l'autre devise (différente de la devise de la prime) C'est elle qui sera valorisée (CallPrice|PutPrice)</para>
        /// </summary>
        public string PricingCurrency { private set; get; }
        /// <summary>
        /// <para>Devise de Prime</para>
        /// </summary>
        public string PremiumCurrency { private set; get; }
        /// <summary>
        /// <para>Option relative à la PricingCurrency utilisé pour le calcul du prix théorique</para>
        /// </summary>
        public DataRow RowCandidate { private set; get; }

        /// <summary>
        /// Type d'option 
        /// </summary>
        public ExerciseStyleEnum ExerciseStyle { private set; get; }
        
        /// <summary>
        /// <para>Si Call : Montant en devise CALL</para>
        /// <para>Si Put : Montant en devise PUT</para>
        /// </summary>
        public decimal NotionalReference { private set; get; }
        
        /// <summary>
        /// 
        /// </summary>
        public string PricingMethod { private set; get; }
                
        
        /// <summary>
        /// Strike de l'option
        /// </summary>
        public IFxStrikePrice StrikePrice { private set; get; }
        
        public bool IsCall { private set; get; }

        /// <summary>
        /// <para>Conversion StrikeQuoteBasisEnum en QuoteBasisEnum</para>
        /// </summary>
        // EG 20150319 (POC] New
        public QuoteBasisEnum QuoteBasis
        {
            get
            {
                QuoteBasisEnum quoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                switch (StrikePrice.StrikeQuoteBasis)
                {
                    case StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency:
                        quoteBasis = ((PremiumCurrency == CallCurrency) ? QuoteBasisEnum.Currency1PerCurrency2 : QuoteBasisEnum.Currency2PerCurrency1);
                        break;
                    case StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency:
                        quoteBasis = ((PremiumCurrency == CallCurrency) ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2);
                        break;
                }
                return quoteBasis;
            }
        }
        #endregion Accessors
        
        #region Constructors
        /// <summary>
        /// Ininitalisation des données de base pour le calcul du MTM
        /// Si Call : 
        /// . NotionalReference = START CallCurrency amount
        /// . Currency1 = CallCurrency
        /// . Currency2 = PutCurrency
        /// Si Put  : 
        /// . NotionalReference = START PutCurrency amount
        /// . Currency1 = PutCurrency
        /// . Currency2 = CallCurrency
        /// </summary>
        /// <param name="pProductBase"></param>
        /// <param name="pPricingMethod"></param>
        /// <param name="pExerciseStyle"></param>
        /// <param name="pOptionType"></param>
        /// <param name="pRowCallCurrency"></param>
        /// <param name="pRowPutCurrency"></param>
        /// <param name="pStrikePrice"></param>
        public MarkToMarketFxInputData(string pPricingMethod, ExerciseStyleEnum pExerciseStyle, 
            DataRow pRowCallCurrency, DataRow pRowPutCurrency, DataRow pRowPremium, IFxStrikePrice pStrikePrice)
        {
            PricingMethod = pPricingMethod;
            ExerciseStyle = pExerciseStyle;
            StrikePrice = pStrikePrice;

            // Identification de la devise de prime
            PremiumCurrency = pRowPremium["UNIT"].ToString();
            CallCurrency = pRowCallCurrency["UNIT"].ToString();
            PutCurrency = pRowPutCurrency["UNIT"].ToString();

            // Recherche de l’option Call ou Put, dont la devise est identique à la devise de prime. 
            // Cette devise  sera la DomesticCurrency. 
            // L’autre devise sera la ForeignCurrency.
            // Le Prix théorique sera calculé sur l'option relative à la ForeignCurrency
            if (pRowPremium["UNIT"].ToString() == CallCurrency)
            {
                PricingCurrency = PutCurrency;
                IsInverse = (StrikePrice.StrikeQuoteBasis == StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency);
                // Prix théorique sur la base de l'option Put (ForeignCurrency)
                RowCandidate = pRowPutCurrency;
            }
            else
            {
                PricingCurrency = CallCurrency;
                IsInverse = (StrikePrice.StrikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency);
                // Prix théorique sur la base de l'option Call (ForeignCurrency)
                RowCandidate = pRowCallCurrency;
            }
            NotionalReference = Convert.ToDecimal(RowCandidate["VALORISATION"]);
        }
        #endregion Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public int GetDefaultAsset(string pCS)
        {
            KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
            {
                IdC1 = this.PremiumCurrency,
                IdC2 = this.PricingCurrency
            };
            return keyAssetFxRate.GetIdAsset(pCS);
        }
    }
    #endregion MarkToMarketFxInputData
}

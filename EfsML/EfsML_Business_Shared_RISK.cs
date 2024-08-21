#region Using Directives
using EFS.ACommon;
using EfsML.v30.CashBalance;
using EfsML.v30.Shared;
#endregion Using Directives

namespace EfsML.Business
{
    #region EFS_CashBalance
    /// <summary>
    /// 
    /// </summary>
    ///PM 20140923 [20066][20185] New
    public class EFS_CashBalance
    {
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pCashBalance"></param>
        /// <param name="pDataDocument"></param>
        public EFS_CashBalance(CashBalance pCashBalance, DataDocumentContainer pDataDocument)
        {
            Calc(pCashBalance, pDataDocument);
        }
        #endregion
        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashBalance"></param>
        /// <param name="pDataDocument"></param>
        private void Calc(CashBalance pCashBalance, DataDocumentContainer pDataDocument)
        {
            #region ExchangeCashBalanceStream
            if (pCashBalance.exchangeCashBalanceStreamSpecified)
            {
                ExchangeCashBalanceStream exCashBalenceStream = pCashBalance.exchangeCashBalanceStream;
                exCashBalenceStream.efs_ExchangeCashBalanceStream = new EFS_CashBalanceStream(pCashBalance.exchangeCashBalanceStream, pDataDocument);
            }
            #endregion
            #region CashBalanceStream
            for (int i = 0; i < ArrFunc.Count(pCashBalance.cashBalanceStream); i++)
            {
                CashBalanceStream cbs = pCashBalance.cashBalanceStream[i];
                cbs.efs_CashBalanceStream = new EFS_CashBalanceStream(pCashBalance.cashBalanceStream[i], pDataDocument);
            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region EFS_CashBalanceStream
    /// <summary>
    /// 
    /// </summary>
    ///PM 20140923 [20066][20185] New
    public class EFS_CashBalanceStream
    {
        #region members
        /// <summary>
        /// 
        /// </summary>
        public CashPosition globalRealizedMargin;
        /// <summary>
        /// 
        /// </summary>
        public bool globalRealizedMarginSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public CashPosition globalUnrealizedMargin;
        /// <summary>
        /// 
        /// </summary>
        public bool globalUnrealizedMarginSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public EFS_MarginConstituent futureMarginConstituent;
        /// <summary>
        /// 
        /// </summary>
        public bool futureMarginConstituentSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public EFS_MarginConstituent futuresStyleOptionMarginConstituent;
        /// <summary>
        /// 
        /// </summary>
        public bool futuresStyleOptionMarginConstituentSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public EFS_MarginConstituent premiumStyleOptionMarginConstituent;
        /// <summary>
        /// 
        /// </summary>
        public bool premiumStyleOptionMarginConstituentSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public CashPosition cashDeposit;
        /// <summary>
        /// 
        /// </summary>
        public bool cashDepositSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public CashPosition cashWithdrawal;
        /// <summary>
        /// 
        /// </summary>
        public bool cashWithdrawalSpecified = false;
        #endregion
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pCashBalanceStream"></param>
        /// <param name="pDataDocument"></param>
        public EFS_CashBalanceStream(CashBalanceStream pCashBalanceStream, DataDocumentContainer pDataDocument)
        {
            Calc(pCashBalanceStream, pDataDocument);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pCashBalanceStream"></param>
        /// <param name="pDataDocument"></param>
        public EFS_CashBalanceStream(ExchangeCashBalanceStream pCashBalanceStream, DataDocumentContainer pDataDocument)
        {
            Calc(pCashBalanceStream, pDataDocument);
        }
        #endregion
        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashBalance"></param>
        /// <param name="pDataDocument"></param>
        private void Calc(CashBalanceStream pCashBalanceStream, DataDocumentContainer pDataDocument)
        {
            futureMarginConstituent = new EFS_MarginConstituent(pDataDocument);
            futuresStyleOptionMarginConstituent = new EFS_MarginConstituent(pDataDocument);
            premiumStyleOptionMarginConstituent = new EFS_MarginConstituent(pDataDocument);
            //
            if (pCashBalanceStream.realizedMarginSpecified)
            {
                globalRealizedMarginSpecified = pCashBalanceStream.realizedMargin.globalAmountSpecified;
                if (globalRealizedMarginSpecified)
                {
                    globalRealizedMargin = pCashBalanceStream.realizedMargin.globalAmount;
                    globalRealizedMargin.SetEfsCashPosition(pDataDocument);
                }
                if (pCashBalanceStream.realizedMargin.futureSpecified)
                {
                    futureMarginConstituentSpecified = true;
                    futureMarginConstituent.SetRealizedMargin(pCashBalanceStream.realizedMargin.future);
                }
                if ((pCashBalanceStream.realizedMargin.optionSpecified) && ArrFunc.IsFilled(pCashBalanceStream.realizedMargin.option))
                {
                    foreach (OptionMarginConstituent option in pCashBalanceStream.realizedMargin.option)
                    {
                        switch (option.valuationMethod)
                        {
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket:
                                futuresStyleOptionMarginConstituentSpecified = true;
                                futuresStyleOptionMarginConstituent.SetRealizedMargin((CashPosition)option);
                                break;
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle:
                                premiumStyleOptionMarginConstituentSpecified = true;
                                premiumStyleOptionMarginConstituent.SetRealizedMargin((CashPosition)option);
                                break;
                        }
                    }
                }
            }
            if (pCashBalanceStream.unrealizedMarginSpecified)
            {
                globalUnrealizedMarginSpecified = pCashBalanceStream.unrealizedMargin.globalAmountSpecified;
                if (globalUnrealizedMarginSpecified)
                {
                    globalUnrealizedMargin = pCashBalanceStream.unrealizedMargin.globalAmount;
                    globalUnrealizedMargin.SetEfsCashPosition(pDataDocument);
                }
                if (pCashBalanceStream.unrealizedMargin.futureSpecified)
                {
                    futureMarginConstituentSpecified = true;
                    futureMarginConstituent.SetUnrealizedMargin(pCashBalanceStream.unrealizedMargin.future);
                }
                if ((pCashBalanceStream.unrealizedMargin.optionSpecified) && ArrFunc.IsFilled(pCashBalanceStream.unrealizedMargin.option))
                {
                    foreach (OptionMarginConstituent option in pCashBalanceStream.unrealizedMargin.option)
                    {
                        switch (option.valuationMethod)
                        {
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket:
                                futuresStyleOptionMarginConstituentSpecified = true;
                                futuresStyleOptionMarginConstituent.SetUnrealizedMargin((CashPosition)option);
                                break;
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle:
                                premiumStyleOptionMarginConstituentSpecified = true;
                                premiumStyleOptionMarginConstituent.SetUnrealizedMargin((CashPosition)option);
                                break;
                        }
                    }
                }
            }
            if (pCashBalanceStream.cashAvailable.constituentSpecified)
            {
                cashDepositSpecified = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashDepositSpecified;
                if (cashDepositSpecified)
                {
                    cashDeposit = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashDeposit;
                    cashDeposit.SetEfsCashPosition(pDataDocument);
                }
                cashWithdrawalSpecified = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashWithdrawalSpecified;
                if (cashWithdrawalSpecified)
                {
                    cashWithdrawal = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashWithdrawal;
                    cashWithdrawal.SetEfsCashPosition(pDataDocument);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashBalanceStream"></param>
        /// <param name="pDataDocument"></param>
        private void Calc(ExchangeCashBalanceStream pCashBalanceStream, DataDocumentContainer pDataDocument)
        {
            futureMarginConstituent = new EFS_MarginConstituent(pDataDocument);
            futuresStyleOptionMarginConstituent = new EFS_MarginConstituent(pDataDocument);
            premiumStyleOptionMarginConstituent = new EFS_MarginConstituent(pDataDocument);
            //
            if (pCashBalanceStream.realizedMarginSpecified)
            {
                globalRealizedMarginSpecified = pCashBalanceStream.realizedMargin.globalAmountSpecified;
                if (globalRealizedMarginSpecified)
                {
                    globalRealizedMargin = pCashBalanceStream.realizedMargin.globalAmount;
                    globalRealizedMargin.SetEfsCashPosition(pDataDocument);
                }
                if (pCashBalanceStream.realizedMargin.futureSpecified)
                {
                    futureMarginConstituentSpecified = true;
                    futureMarginConstituent.SetRealizedMargin(pCashBalanceStream.realizedMargin.future);
                }
                if ((pCashBalanceStream.realizedMargin.optionSpecified) && ArrFunc.IsFilled(pCashBalanceStream.realizedMargin.option))
                {
                    foreach (OptionMarginConstituent option in pCashBalanceStream.realizedMargin.option)
                    {
                        switch (option.valuationMethod)
                        {
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket:
                                futuresStyleOptionMarginConstituentSpecified = true;
                                futuresStyleOptionMarginConstituent.SetRealizedMargin((CashPosition)option);
                                break;
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle:
                                premiumStyleOptionMarginConstituentSpecified = true;
                                premiumStyleOptionMarginConstituent.SetRealizedMargin((CashPosition)option);
                                break;
                        }
                    }
                }
            }
            if (pCashBalanceStream.unrealizedMarginSpecified)
            {
                globalUnrealizedMarginSpecified = pCashBalanceStream.unrealizedMargin.globalAmountSpecified;
                if (globalUnrealizedMarginSpecified)
                {
                    globalUnrealizedMargin = pCashBalanceStream.unrealizedMargin.globalAmount;
                    globalUnrealizedMargin.SetEfsCashPosition(pDataDocument);
                }
                if (pCashBalanceStream.unrealizedMargin.futureSpecified)
                {
                    futureMarginConstituentSpecified = true;
                    futureMarginConstituent.SetUnrealizedMargin(pCashBalanceStream.unrealizedMargin.future);
                }
                if ((pCashBalanceStream.unrealizedMargin.optionSpecified) && ArrFunc.IsFilled(pCashBalanceStream.unrealizedMargin.option))
                {
                    foreach (OptionMarginConstituent option in pCashBalanceStream.unrealizedMargin.option)
                    {
                        switch (option.valuationMethod)
                        {
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket:
                                futuresStyleOptionMarginConstituentSpecified = true;
                                futuresStyleOptionMarginConstituent.SetUnrealizedMargin((CashPosition)option);
                                break;
                            case FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle:
                                premiumStyleOptionMarginConstituentSpecified = true;
                                premiumStyleOptionMarginConstituent.SetUnrealizedMargin((CashPosition)option);
                                break;
                        }
                    }
                }
            }
            if (pCashBalanceStream.cashAvailable.constituentSpecified)
            {
                cashDepositSpecified = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashDepositSpecified;
                if (cashDepositSpecified)
                {
                    cashDeposit = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashDeposit;
                    cashDeposit.SetEfsCashPosition(pDataDocument);
                }
                cashWithdrawalSpecified = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashWithdrawalSpecified;
                if (cashWithdrawalSpecified)
                {
                    cashWithdrawal = pCashBalanceStream.cashAvailable.constituent.cashBalancePayment.cashWithdrawal;
                    cashWithdrawal.SetEfsCashPosition(pDataDocument);
                }
            }
        }
        #endregion
    }
    #endregion

    #region EFS_MarginConstituent
    /// <summary>
    /// 
    /// </summary>
    ///PM 20140926 [20066][20185] New
    public class EFS_MarginConstituent
    {
        #region members
        private readonly DataDocumentContainer m_DataDocument;
        /// <summary>
        /// 
        /// </summary>
        public CashPosition realizedMargin;
        /// <summary>
        /// 
        /// </summary>
        public bool realizedMarginSpecified = false;
        /// <summary>
        /// 
        /// </summary>
        public CashPosition unrealizedMargin;
        /// <summary>
        /// 
        /// </summary>
        public bool unrealizedMarginSpecified = false;
        #endregion
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMarginConstituent"></param>
        /// <param name="pDataDocument"></param>
        public EFS_MarginConstituent(DataDocumentContainer pDataDocument)
        {
            m_DataDocument = pDataDocument;
        }
        #endregion
        #region methods
        public void SetRealizedMargin(CashPosition pRealizedMargin)
        {
            if (pRealizedMargin != null)
            {
                realizedMargin = pRealizedMargin;
                realizedMargin.SetEfsCashPosition(m_DataDocument);
                realizedMarginSpecified = true;
            }
            else
            {
                realizedMarginSpecified = false;
            }
        }
        public void SetUnrealizedMargin(CashPosition pUnrealizedMargin)
        {
            if (pUnrealizedMargin != null)
            {
                unrealizedMargin = pUnrealizedMargin;
                unrealizedMargin.SetEfsCashPosition(m_DataDocument);
                unrealizedMarginSpecified = true;
            }
            else
            {
                unrealizedMarginSpecified = false;
            }
        }
        #endregion
    }
    #endregion
}

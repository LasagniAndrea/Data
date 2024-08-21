using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;

using EfsML.Interface;

using EfsML.v30.AssetDef;
using EfsML.v30.CashBalance;
using EfsML.v30.Doc;
using EfsML.v30.Shared;

using FpML.v44.Shared;

//
// PM 20190701 [24761] New
//
namespace EFS.SpheresRiskPerformance.CashBalance
{
    /// <summary>
    /// Ensemble des "Comparer" pour le Cash Balance
    /// </summary>
    public static class CashBalanceComparer
    {
        #region Static Members
        #region Cash Balance Information Class Comparer
        public static CashBalanceSettingsComparer CashBalanceSettingsComparer = new CashBalanceSettingsComparer();
        public static CssCustodianStatusComparer CssCustodianStatusComparer = new CssCustodianStatusComparer();
        public static IdentifiedFxRateComparer IdentifiedFxRateComparer = new IdentifiedFxRateComparer();
        #endregion Cash Balance Information  Class Comparer
        #region Stream Class Comparer
        public static ExchangeCashBalanceStreamComparer ExchangeCashBalanceStreamComparer = new ExchangeCashBalanceStreamComparer();
        public static CashBalanceStreamComparer CashBalanceStreamComparer = new CashBalanceStreamComparer();
        #endregion Stream Class Comparer
        #region Object of Stream Class Comparer
        public static AssetMarginConstituentComparer AssetMarginConstituentComparer = new AssetMarginConstituentComparer();
        public static CashAvailableComparer CashAvailableComparer = new CashAvailableComparer();
        public static CashBalancePaymentComparer CashBalancePaymentComparer = new CashBalancePaymentComparer();
        public static CashFlowsComparer CashFlowsComparer = new CashFlowsComparer();
        public static CashPositionComparer CashPositionComparer = new CashPositionComparer();
        public static CollateralAvailableComparer CollateralAvailableComparer = new CollateralAvailableComparer();
        public static ContractAmountComparer ContractAmountComparer = new ContractAmountComparer();
        public static ContractAmountAndTaxComparer ContractAmountAndTaxComparer = new ContractAmountAndTaxComparer();
        public static ContractSimplePaymentComparer ContractSimplePaymentComparer = new ContractSimplePaymentComparer();
        public static ContractSimplePaymentConstituentComparer ContractSimplePaymentConstituentComparer = new ContractSimplePaymentConstituentComparer();
        public static CssExchangeCashPositionComparer CssExchangeCashPositionComparer = new CssExchangeCashPositionComparer();
        public static DetailedCashPositionComparer DetailedCashPositionComparer = new DetailedCashPositionComparer();
        public static DetailedContractPaymentComparer DetailedContractPaymentComparer = new DetailedContractPaymentComparer();
        public static DetailedDateAmountComparer DetailedDateAmountComparer = new DetailedDateAmountComparer();
        public static ExchangeCashPositionComparer ExchangeCashPositionComparer = new ExchangeCashPositionComparer();
        public static MarginConstituentComparer MarginConstituentComparer = new MarginConstituentComparer();
        public static OptionLiquidatingValueComparer OptionLiquidatingValueComparer = new OptionLiquidatingValueComparer();
        public static OptionMarginConstituentComparer OptionMarginConstituentComparer = new OptionMarginConstituentComparer();
        public static PreviousMarginConstituentComparer PreviousMarginConstituentComparer = new PreviousMarginConstituentComparer();
        public static PosCollateralComparer PosCollateralComparer = new PosCollateralComparer();
        public static SimplePaymentComparer SimplePaymentComparer = new SimplePaymentComparer();
        #endregion Object of Stream Class Comparer
        #endregion Static Members
        #region Static Methods
        /// <summary>
        /// Indique si deux BusinessCenters sont équivalents
        /// </summary>
        /// <param name="pBCDefA"></param>
        /// <param name="pBCDefB"></param>
        /// <returns></returns>
        public static bool BusinessCentersDefineEquals(BusinessCenters pBCDefA, BusinessCenters pBCDefB)
        {
            bool equal = (pBCDefA == pBCDefB);
            if (!equal && (pBCDefA != default(BusinessCenters)) && (pBCDefB != default(BusinessCenters)))
            {
                equal = ((pBCDefA.businessCenter == pBCDefB.businessCenter) || ((pBCDefA.businessCenter != default(BusinessCenter[])) && (pBCDefB.businessCenter != default(BusinessCenter[]))));
                if (equal && (pBCDefA.businessCenter != default(BusinessCenter[])) && (pBCDefB.businessCenter != default(BusinessCenter[])))
                {
                    equal = (pBCDefA.businessCenter.Count() == pBCDefB.businessCenter.Count());
                    var except = pBCDefA.businessCenter.Select(bc => bc.Value).Except(pBCDefB.businessCenter.Select(bc => bc.Value));
                    equal = equal && (except.Count() == 0);
                }
            }
            return equal;
        }
        #endregion Static Methods
    }

    #region Cash Balance Information Class Comparer
    /// <summary>
    /// EqualityComparer pour CashBalanceSettings
    /// </summary>
    public class CashBalanceSettingsComparer : EqualityComparer<CashBalanceSettings>
    {
        /// <summary>
        /// Indique si deux CashBalanceSettings sont équivalents
        /// </summary>
        /// <param name="pSettingA"></param>
        /// <param name="pSettingB"></param>
        /// <returns></returns>
        public override bool Equals(CashBalanceSettings pSettingA, CashBalanceSettings pSettingB)
        {
            bool equal = (pSettingA == pSettingB);
            if (!equal && (pSettingA != default(CashBalanceSettings)) && (pSettingB != default(CashBalanceSettings)))
            {
                equal = (pSettingA.cashBalanceOfficePartyReference.href == pSettingB.cashBalanceOfficePartyReference.href);
                equal = equal && (pSettingA.exchangeCurrencySpecified == pSettingB.exchangeCurrencySpecified);
                if (equal && pSettingA.exchangeCurrencySpecified && pSettingB.exchangeCurrencySpecified)
                {
                    equal = (pSettingA.exchangeCurrency.Value == pSettingB.exchangeCurrency.Value);
                }
                equal = equal && (pSettingA.scope == pSettingB.scope);
                equal = equal && (pSettingA.exchangeCurrencySpecified == pSettingB.exchangeCurrencySpecified);
                if (equal && pSettingA.exchangeCurrencySpecified && pSettingB.exchangeCurrencySpecified)
                {
                    equal = (pSettingA.exchangeCurrency.Value == pSettingB.exchangeCurrency.Value);
                }
                equal = equal && (pSettingA.useAvailableCash.BoolValue == pSettingB.useAvailableCash.BoolValue);
                equal = equal && (pSettingA.cashAndCollateral == pSettingB.cashAndCollateral);
                equal = equal && (pSettingA.managementBalance.BoolValue == pSettingB.managementBalance.BoolValue);
                equal = equal && (pSettingA.marginCallCalculationMethod == pSettingB.marginCallCalculationMethod);
                equal = equal && (pSettingA.cashBalanceMethodSpecified == pSettingB.cashBalanceMethodSpecified);
                if (equal && pSettingA.cashBalanceMethodSpecified && pSettingB.cashBalanceMethodSpecified)
                {
                    equal = (pSettingA.cashBalanceMethod == pSettingB.cashBalanceMethod);
                }
                equal = equal && (pSettingA.cashBalanceCurrencySpecified == pSettingB.cashBalanceCurrencySpecified);
                if (equal && pSettingA.cashBalanceCurrencySpecified && pSettingB.cashBalanceCurrencySpecified)
                {
                    equal = (pSettingA.cashBalanceCurrency.Value == pSettingB.cashBalanceCurrency.Value);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CashBalanceSettings
        /// </summary>
        /// <param name="pSetting"></param>
        /// <returns></returns>
        public override int GetHashCode(CashBalanceSettings pSetting)
        {
            int hCode = 0;
            if (pSetting != default(CashBalanceSettings))
            {
                hCode = pSetting.cashBalanceOfficePartyReference.GetHashCode();
            }
            return hCode;
        }
    }

    /// <summary>
    /// EqualityComparer pour IdentifiedFxRate
    /// </summary>
    public class IdentifiedFxRateComparer : EqualityComparer<IdentifiedFxRate>
    {
        /// <summary>
        /// Indique si deux IdentifiedFxRate sont équivalents
        /// </summary>
        /// <param name="pFxRateA"></param>
        /// <param name="pFxRateB"></param>
        /// <returns></returns>
        public override bool Equals(IdentifiedFxRate pFxRateA, IdentifiedFxRate pFxRateB)
        {
            bool equal = (pFxRateA == pFxRateB);
            if (!equal && (pFxRateA != default(IdentifiedFxRate)) && (pFxRateB != default(IdentifiedFxRate)))
            {
                equal = (pFxRateA.otcmlId == pFxRateB.otcmlId);
                equal = equal && (pFxRateA.Id == pFxRateB.Id);
                equal = equal && ((pFxRateA.rate == pFxRateB.rate) || ((pFxRateA.rate != null) && (pFxRateB.rate != null)));
                if (equal && (pFxRateA.rate != null) && (pFxRateB.rate != null))
                {
                    equal = (pFxRateA.rate.DecValue == pFxRateB.rate.DecValue);
                }
                equal = equal && ((pFxRateA.quotedCurrencyPair == pFxRateB.quotedCurrencyPair) || ((pFxRateA.quotedCurrencyPair != default(QuotedCurrencyPair)) && (pFxRateB.quotedCurrencyPair != default(QuotedCurrencyPair))));
                if (equal && (pFxRateA.quotedCurrencyPair != default(QuotedCurrencyPair)) && (pFxRateB.quotedCurrencyPair != default(QuotedCurrencyPair)))
                {
                    equal = (CBDataDocument.GetFxRateId(pFxRateA.quotedCurrencyPair) == CBDataDocument.GetFxRateId(pFxRateB.quotedCurrencyPair));
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un IdentifiedFxRate
        /// </summary>
        /// <param name="pFxRate"></param>
        /// <returns></returns>
        public override int GetHashCode(IdentifiedFxRate pFxRate)
        {
            decimal hCode = 0;
            if (pFxRate != default(IdentifiedFxRate))
            {
                hCode = pFxRate.rate.DecValue;
            }
            return hCode.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de IdentifiedFxRate sont équivalents
        /// </summary>
        /// <param name="pFxRateA"></param>
        /// <param name="pFxRateB"></param>
        /// <returns></returns>
        public bool ArrayEquals(IdentifiedFxRate[] pFxRateA, IdentifiedFxRate[] pFxRateB)
        {
            bool equal = (pFxRateA == pFxRateB);
            if (!equal && (pFxRateA != default(IdentifiedFxRate[])) && (pFxRateB != default(IdentifiedFxRate[])))
            {
                equal = (pFxRateA.Count() == pFxRateB.Count());
                if (equal)
                {
                    foreach (IdentifiedFxRate detailA in pFxRateA)
                    {
                        IdentifiedFxRate detailB = pFxRateB.FirstOrDefault(b => b.Id == detailA.Id);
                        equal = equal && Equals(detailA, detailB);
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }

    /// <summary>
    /// EqualityComparer pour CssCustodianStatus
    /// </summary>
    public class CssCustodianStatusComparer : EqualityComparer<CssCustodianStatus>
    {
        /// <summary>
        /// Indique si deux CssCustodianStatus sont équivalents
        /// </summary>
        /// <param name="pStatusA"></param>
        /// <param name="pStatusB"></param>
        /// <returns></returns>
        public override bool Equals(CssCustodianStatus pStatusA, CssCustodianStatus pStatusB)
        {
            bool equal = (pStatusA == pStatusB);
            if (!equal && (pStatusA != default(CssCustodianStatus)) && (pStatusB != default(CssCustodianStatus)))
            {
                equal = (pStatusA.cssCustodianHref == pStatusB.cssCustodianHref);
                // Ne pas de comparer idACssCustodian
                //equal = equal && (pStatusA.idACssCustodian == pStatusB.idACssCustodian);
                equal = equal && (pStatusA.status == pStatusB.status);
                equal = equal && ((pStatusA.exchStatus == pStatusB.exchStatus) || ((pStatusA.exchStatus != default(ExchangeStatus[])) && (pStatusB.exchStatus != default(ExchangeStatus[]))));
                if (equal && (pStatusA.exchStatus != default(ExchangeStatus[])) && (pStatusB.exchStatus != default(ExchangeStatus[])))
                {
                    equal = (pStatusA.exchStatus.Count() == pStatusB.exchStatus.Count());
                    if (equal)
                    {
                        foreach (ExchangeStatus detailA in pStatusA.exchStatus)
                        {
                            ExchangeStatus detailB = pStatusB.exchStatus.FirstOrDefault(b => b.OTCmlId == detailA.OTCmlId);
                            equal = equal && (detailB != default(ExchangeStatus));
                            if (equal)
                            {
                                equal = equal && (detailA.Exch == detailB.Exch);
                                equal = equal && (detailA.status == detailB.status);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un EndOfDayStatus
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public override int GetHashCode(CssCustodianStatus pStatus)
        {
            int hCode = 0;
            if (pStatus != default(CssCustodianStatus))
            {
                hCode = pStatus.cssCustodianHref.GetHashCode();
            }
            return hCode;
        }

        /// <summary>
        /// Indique si deux tableaux de CssCustodianStatus sont équivalents
        /// </summary>
        /// <param name="pStatusA"></param>
        /// <param name="pStatusB"></param>
        /// <returns></returns>
        public bool ArrayEquals(CssCustodianStatus[] pStatusA, CssCustodianStatus[] pStatusB)
        {
            bool equal = ((pStatusA == pStatusB) || ((pStatusA != default(CssCustodianStatus[])) && (pStatusB != default(CssCustodianStatus[]))));
            if (equal && (pStatusA != default(CssCustodianStatus[])) && (pStatusB != default(CssCustodianStatus[])))
            {
                equal = (pStatusA.Count() == pStatusB.Count());
                if (equal)
                {
                    foreach (CssCustodianStatus detailA in pStatusA)
                    {
                        CssCustodianStatus detailB = pStatusB.FirstOrDefault(b => b.cssCustodianHref == detailA.cssCustodianHref);
                        equal = equal && Equals(detailA, detailB);
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }
    #endregion Cash Balance Information Class Comparer

    #region Stream Class Comparer
    /// <summary>
    /// EqualityComparer pour ExchangeCashBalanceStream
    /// </summary>
    public class ExchangeCashBalanceStreamComparer : EqualityComparer<ExchangeCashBalanceStream>
    {
        /// <summary>
        /// Indique si deux ExchangeCashBalanceStream sont équivalents
        /// </summary>
        /// <param name="pExchStreamA"></param>
        /// <param name="pExchStreamB"></param>
        /// <returns></returns>
        public override bool Equals(ExchangeCashBalanceStream pExchStreamA, ExchangeCashBalanceStream pExchStreamB)
        {
            bool equal = (pExchStreamA == pExchStreamB);
            if (!equal && (pExchStreamA != default(ExchangeCashBalanceStream)) && (pExchStreamB != default(ExchangeCashBalanceStream)))
            {
                #region Currency
                //
                // Currency
                equal = ((pExchStreamA.currency == pExchStreamB.currency) || ((pExchStreamA.currency != default(Currency)) && (pExchStreamB.currency != default(Currency))));
                if (equal && (pExchStreamA.currency != default(Currency)) && (pExchStreamB.currency != default(Currency)))
                {
                    equal = equal && (pExchStreamA.currency.Value == pExchStreamB.currency.Value);
                }
                #endregion Currency
                #region Excess Deficit With Forward Cash
                //
                // Cash Balance - Exchange cash balance stream - Excess Deficit With Forward Cash
                equal = equal && (pExchStreamA.excessDeficitWithForwardCashSpecified == pExchStreamB.excessDeficitWithForwardCashSpecified);
                if (equal && pExchStreamA.excessDeficitWithForwardCashSpecified && pExchStreamB.excessDeficitWithForwardCashSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.excessDeficitWithForwardCash, pExchStreamB.excessDeficitWithForwardCash));
                }
                #endregion Excess Deficit With Forward Cash
                #region Excess Deficit
                //
                // Cash Balance - Exchange cash balance stream - Excess Deficit
                equal = equal && (pExchStreamA.excessDeficitSpecified == pExchStreamB.excessDeficitSpecified);
                if (equal && pExchStreamA.excessDeficitSpecified && pExchStreamB.excessDeficitSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.excessDeficit, pExchStreamB.excessDeficit));
                }
                #endregion Excess Deficit
                #region Total Account Value
                //
                // Cash Balance - Exchange cash balance stream - Total Account Value
                equal = equal && (pExchStreamA.totalAccountValueSpecified == pExchStreamB.totalAccountValueSpecified);
                if (equal && pExchStreamA.totalAccountValueSpecified && pExchStreamB.totalAccountValueSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.totalAccountValue, pExchStreamB.totalAccountValue));
                }
                #endregion Total Account Value
                #region Equity Balance With Forward Cash
                //
                // Cash Balance - Exchange cash balance stream - Equity Balance With Forward Cash
                equal = equal && (pExchStreamA.equityBalanceWithForwardCashSpecified == pExchStreamB.equityBalanceWithForwardCashSpecified);
                if (equal && pExchStreamA.equityBalanceWithForwardCashSpecified && pExchStreamB.equityBalanceWithForwardCashSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.equityBalanceWithForwardCash, pExchStreamB.equityBalanceWithForwardCash));
                }
                #endregion Equity Balance With Forward Cash
                #region Equity Balance
                //
                // Cash Balance - Exchange cash balance stream - Equity Balance
                equal = equal && (pExchStreamA.equityBalanceSpecified == pExchStreamB.equityBalanceSpecified);
                if (equal && pExchStreamA.equityBalanceSpecified && pExchStreamB.equityBalanceSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.equityBalance, pExchStreamB.equityBalance));
                }
                #endregion Equity Balance
                #region Forward Cash Payment
                //
                // Cash Balance - Exchange cash balance stream - Forward Cash Payment
                equal = equal && (pExchStreamA.forwardCashPaymentSpecified == pExchStreamB.forwardCashPaymentSpecified);
                if (equal && pExchStreamA.forwardCashPaymentSpecified && pExchStreamB.forwardCashPaymentSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashBalancePaymentComparer.Equals(pExchStreamA.forwardCashPayment, pExchStreamB.forwardCashPayment));
                }
                #endregion Forward Cash Payment
                #region Unsettled Cash
                //
                // Cash Balance - Exchange cash balance stream - Unsettled Cash
                equal = equal && (pExchStreamA.unsettledCashSpecified == pExchStreamB.unsettledCashSpecified);
                if (equal && pExchStreamA.unsettledCashSpecified && pExchStreamB.unsettledCashSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pExchStreamA.unsettledCash, pExchStreamB.unsettledCash));
                }
                #endregion Unsettled Cash
                #region Borrowing
                //
                // Cash Balance - Exchange cash balance stream - Borrowing
                equal = equal && (pExchStreamA.borrowingSpecified == pExchStreamB.borrowingSpecified);
                if (equal && pExchStreamA.borrowingSpecified && pExchStreamB.borrowingSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pExchStreamA.borrowing, pExchStreamB.borrowing));
                }
                #endregion Borrowing
                #region Funding
                //
                // Cash Balance - Exchange cash balance stream - Funding
                equal = equal && (pExchStreamA.fundingSpecified == pExchStreamB.fundingSpecified);
                if (equal && pExchStreamA.fundingSpecified && pExchStreamB.fundingSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pExchStreamA.funding, pExchStreamB.funding));
                }
                #endregion Funding
                #region Market Value
                //
                // Cash Balance - Exchange cash balance stream - Market Value
                equal = equal && (pExchStreamA.marketValueSpecified == pExchStreamB.marketValueSpecified);
                if (equal && pExchStreamA.marketValueSpecified && pExchStreamB.marketValueSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pExchStreamA.marketValue, pExchStreamB.marketValue));
                }
                #endregion Market Value
                #region Liquidating Value
                //
                // Cash Balance - Exchange cash balance stream - Liquidating Value
                equal = equal && (pExchStreamA.liquidatingValueSpecified == pExchStreamB.liquidatingValueSpecified);
                if (equal && pExchStreamA.liquidatingValueSpecified && pExchStreamB.liquidatingValueSpecified)
                {
                    equal = equal && (CashBalanceComparer.OptionLiquidatingValueComparer.Equals(pExchStreamA.liquidatingValue, pExchStreamB.liquidatingValue));
                }
                #endregion Liquidating Value
                #region Unrealized Margin
                //
                // Cash Balance - Exchange cash balance stream - Unrealized Margin
                equal = equal && (pExchStreamA.unrealizedMarginSpecified == pExchStreamB.unrealizedMarginSpecified);
                if (equal && pExchStreamA.unrealizedMarginSpecified && pExchStreamB.unrealizedMarginSpecified)
                {
                    equal = equal && (CashBalanceComparer.MarginConstituentComparer.Equals(pExchStreamA.unrealizedMargin, pExchStreamB.unrealizedMargin));
                }
                #endregion Unrealized Margin
                #region Realized Margin
                //
                // Cash Balance - Exchange cash balance stream - Realized Margin
                equal = equal && (pExchStreamA.realizedMarginSpecified == pExchStreamB.realizedMarginSpecified);
                if (equal && pExchStreamA.realizedMarginSpecified && pExchStreamB.realizedMarginSpecified)
                {
                    equal = equal && (CashBalanceComparer.MarginConstituentComparer.Equals(pExchStreamA.realizedMargin, pExchStreamB.realizedMargin));
                }
                #endregion Realized Margin
                #region Cash Balance
                //
                // Cash Balance - Exchange cash balance stream - Cash Balance
                equal = equal && (pExchStreamA.cashBalanceSpecified == pExchStreamB.cashBalanceSpecified);
                if (equal && pExchStreamA.cashBalanceSpecified && pExchStreamB.cashBalanceSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.cashBalance, pExchStreamB.cashBalance));
                }
                #endregion Cash Balance
                #region Collateral Available
                //
                // Cash Balance - Exchange cash balance stream - Collateral Available
                equal = equal && (pExchStreamA.collateralAvailableSpecified == pExchStreamB.collateralAvailableSpecified);
                if (equal && pExchStreamA.collateralAvailableSpecified && pExchStreamB.collateralAvailableSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.collateralAvailable, pExchStreamB.collateralAvailable));
                }
                #endregion Collateral Available
                #region Cash Available / Cash Payment / Previous Cash Balance / Cash Flows
                //
                // Cash Balance - Exchange cash balance stream - Cash Available
                equal = equal && (pExchStreamA.cashAvailableSpecified == pExchStreamB.cashAvailableSpecified);
                if (equal && pExchStreamA.cashAvailableSpecified && pExchStreamB.cashAvailableSpecified)
                {
                    equal &= CashBalanceComparer.CashAvailableComparer.Equals(pExchStreamA.cashAvailable, pExchStreamB.cashAvailable);
                }
                #endregion Cash Available / Cash Payment / Previous Cash Balance / Cash Flows
                #region Margin call
                //
                // Cash Balance - Exchange cash balance stream - Margin call
                equal = equal && (pExchStreamA.marginCallSpecified == pExchStreamB.marginCallSpecified);
                if (equal && pExchStreamA.marginCallSpecified && pExchStreamB.marginCallSpecified)
                {
                    equal = equal && (CashBalanceComparer.SimplePaymentComparer.Equals(pExchStreamA.marginCall, pExchStreamB.marginCall));
                }
                #endregion Margin call
                #region Collateral used
                //
                // Cash Balance - Exchange cash balance stream - Collateral used
                equal = equal && (pExchStreamA.collateralUsedSpecified == pExchStreamB.collateralUsedSpecified);
                if (equal && pExchStreamA.collateralUsedSpecified && pExchStreamB.collateralUsedSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.collateralUsed, pExchStreamB.collateralUsed));
                }
                #endregion Collateral used
                #region Cash used
                //
                // Cash Balance - Exchange cash balance stream - Cash used
                equal = equal && (pExchStreamA.cashUsedSpecified == pExchStreamB.cashUsedSpecified);
                if (equal && pExchStreamA.cashUsedSpecified && pExchStreamB.cashUsedSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.cashUsed, pExchStreamB.cashUsed));
                }
                #endregion Cash used
                #region Uncovered Margin requirement
                //
                // Cash Balance - Exchange cash balance stream - Uncovered Margin requirement
                equal = equal && (pExchStreamA.uncoveredMarginRequirementSpecified == pExchStreamB.uncoveredMarginRequirementSpecified);
                if (equal && pExchStreamA.uncoveredMarginRequirementSpecified && pExchStreamB.uncoveredMarginRequirementSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.uncoveredMarginRequirement, pExchStreamB.uncoveredMarginRequirement));
                }
                #endregion PUncovered Margin requirement
                #region Margin Requirement
                //
                // Cash Balance - Exchange cash balance stream - Margin requirement
                equal = equal && (pExchStreamA.marginRequirementSpecified == pExchStreamB.marginRequirementSpecified);
                if (equal && pExchStreamA.marginRequirementSpecified && pExchStreamB.marginRequirementSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pExchStreamA.marginRequirement, pExchStreamB.marginRequirement));
                }
                #endregion Margin Requirement
                #region Previous Margin requirement constituent
                //
                // Cash Balance - Exchange cash balance stream - Previous Margin requirement constituent
                equal = equal && (pExchStreamA.previousMarginConstituentSpecified == pExchStreamB.previousMarginConstituentSpecified);
                if (equal && pExchStreamA.previousMarginConstituentSpecified && pExchStreamB.previousMarginConstituentSpecified)
                {
                    equal = equal && (CashBalanceComparer.PreviousMarginConstituentComparer.Equals(pExchStreamA.previousMarginConstituent, pExchStreamB.previousMarginConstituent));
                }
                #endregion Previous Margin requirement constituent
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un ExchangeCashBalanceStream
        /// </summary>
        /// <param name="pExchStream"></param>
        /// <returns></returns>
        public override int GetHashCode(ExchangeCashBalanceStream pExchStream)
        {
            decimal hCode = 0;
            if ((pExchStream != default(ExchangeCashBalanceStream)) && (pExchStream.cashBalanceSpecified) && (pExchStream.cashBalance != default(CashPosition)))
            {
                hCode = pExchStream.cashBalance.amount.amount.DecValue;
            }
            return hCode.GetHashCode();
        }
    }

    /// <summary>
    /// EqualityComparer pour CashBalanceStream
    /// </summary>
    public class CashBalanceStreamComparer : EqualityComparer<CashBalanceStream>
    {
        /// <summary>
        /// Indique si deux CashBalanceStream sont équivalents
        /// </summary>
        /// <param name="pStreamA"></param>
        /// <param name="pStreamB"></param>
        /// <returns></returns>
        public override bool Equals(CashBalanceStream pStreamA, CashBalanceStream pStreamB)
        {
            bool equal = (pStreamA == pStreamB);
            if (!equal && (pStreamA != default(CashBalanceStream)) && (pStreamB != default(CashBalanceStream)))
            {
                equal = true;
                #region Currency
                //
                // Currency
                equal = equal && ((pStreamA.currency == pStreamB.currency) || ((pStreamA.currency != default(Currency)) && (pStreamB.currency != default(Currency))));
                if (equal && (pStreamA.currency != default(Currency)) && (pStreamB.currency != default(Currency)))
                {
                    equal = equal && (pStreamA.currency.Value == pStreamB.currency.Value);
                }
                #endregion Currency
                #region Excess Deficit With Forward Cash
                //
                // Cash Balance - Cash balance stream - Excess Deficit With Forward Cash
                equal = equal && (pStreamA.excessDeficitWithForwardCashSpecified == pStreamB.excessDeficitWithForwardCashSpecified);
                if (equal && pStreamA.excessDeficitWithForwardCashSpecified && pStreamB.excessDeficitWithForwardCashSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.excessDeficitWithForwardCash, pStreamB.excessDeficitWithForwardCash));
                }
                #endregion Excess Deficit With Forward Cash
                #region Excess Deficit
                //
                // Cash Balance - Cash balance stream - Excess Deficit
                equal = equal && (pStreamA.excessDeficitSpecified == pStreamB.excessDeficitSpecified);
                if (equal && pStreamA.excessDeficitSpecified && pStreamB.excessDeficitSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.excessDeficit, pStreamB.excessDeficit));
                }
                #endregion Excess Deficit
                #region Total Account Value
                //
                // Cash Balance - Cash balance stream - Total Account Value
                equal = equal && (pStreamA.totalAccountValueSpecified == pStreamB.totalAccountValueSpecified);
                if (equal && pStreamA.totalAccountValueSpecified && pStreamB.totalAccountValueSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.totalAccountValue, pStreamB.totalAccountValue));
                }
                #endregion Total Account Value
                #region Equity Balance With Forward Cash
                //
                // Cash Balance - Cash balance stream - Equity Balance With Forward Cash
                equal = equal && (pStreamA.equityBalanceWithForwardCashSpecified == pStreamB.equityBalanceWithForwardCashSpecified);
                if (equal && pStreamA.equityBalanceWithForwardCashSpecified && pStreamB.equityBalanceWithForwardCashSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.equityBalanceWithForwardCash, pStreamB.equityBalanceWithForwardCash));
                }
                #endregion Equity Balance With Forward Cash
                #region Equity Balance
                //
                // Cash Balance - Cash balance stream - Equity Balance
                equal = equal && (pStreamA.equityBalanceSpecified == pStreamB.equityBalanceSpecified);
                if (equal && pStreamA.equityBalanceSpecified && pStreamB.equityBalanceSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.equityBalance, pStreamB.equityBalance));
                }
                #endregion Equity Balance
                #region Forward Cash Payment
                //
                // Cash Balance - Cash balance stream - Forward Cash Payment
                equal = equal && (pStreamA.forwardCashPaymentSpecified == pStreamB.forwardCashPaymentSpecified);
                if (equal && pStreamA.forwardCashPaymentSpecified && pStreamB.forwardCashPaymentSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashBalancePaymentComparer.Equals(pStreamA.forwardCashPayment, pStreamB.forwardCashPayment));
                }
                #endregion Forward Cash Payment
                #region Unsettled Cash
                //
                // Cash Balance - Cash balance stream - Unsettled Cash
                equal = equal && (pStreamA.unsettledCashSpecified == pStreamB.unsettledCashSpecified);
                if (equal && pStreamA.unsettledCashSpecified && pStreamB.unsettledCashSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pStreamA.unsettledCash, pStreamB.unsettledCash));
                }
                #endregion Unsettled Cash
                #region Borrowing
                //
                // Cash Balance - Cash balance stream - Borrowing
                equal = equal && (pStreamA.borrowingSpecified == pStreamB.borrowingSpecified);
                if (equal && pStreamA.borrowingSpecified && pStreamB.borrowingSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pStreamA.borrowing, pStreamB.borrowing));
                }
                #endregion Borrowing
                #region Funding
                //
                // Cash Balance - Cash balance stream - Funding
                equal = equal && (pStreamA.fundingSpecified == pStreamB.fundingSpecified);
                if (equal && pStreamA.fundingSpecified && pStreamB.fundingSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pStreamA.funding, pStreamB.funding));
                }
                #endregion Funding
                #region Market Value
                //
                // Cash Balance - Cash balance stream - Market Value
                equal = equal && (pStreamA.marketValueSpecified == pStreamB.marketValueSpecified);
                if (equal && pStreamA.marketValueSpecified && pStreamB.marketValueSpecified)
                {
                    equal = equal && (CashBalanceComparer.DetailedCashPositionComparer.Equals(pStreamA.marketValue, pStreamB.marketValue));
                }
                #endregion Market Value
                #region Liquidating Value
                //
                // Cash Balance - Cash balance stream - Liquidating Value
                equal = equal && (pStreamA.liquidatingValueSpecified == pStreamB.liquidatingValueSpecified);
                if (equal && pStreamA.liquidatingValueSpecified && pStreamB.liquidatingValueSpecified)
                {
                    equal = equal && (CashBalanceComparer.OptionLiquidatingValueComparer.Equals(pStreamA.liquidatingValue, pStreamB.liquidatingValue));
                }
                #endregion Liquidating Value
                #region Unrealized Margin
                //
                // Cash Balance - Cash balance stream - Unrealized Margin
                equal = equal && (pStreamA.unrealizedMarginSpecified == pStreamB.unrealizedMarginSpecified);
                if (equal && pStreamA.unrealizedMarginSpecified && pStreamB.unrealizedMarginSpecified)
                {
                    equal = equal && (CashBalanceComparer.MarginConstituentComparer.Equals(pStreamA.unrealizedMargin, pStreamB.unrealizedMargin));
                }
                #endregion Unrealized Margin
                #region Realized Margin
                //
                // Cash Balance - Cash balance stream - Realized Margin
                equal = equal && (pStreamA.realizedMarginSpecified == pStreamB.realizedMarginSpecified);
                if (equal && pStreamA.realizedMarginSpecified && pStreamB.realizedMarginSpecified)
                {
                    equal = equal && (CashBalanceComparer.MarginConstituentComparer.Equals(pStreamA.realizedMargin, pStreamB.realizedMargin));
                }
                #endregion Realized Margin
                #region Cash Balance
                //
                // Cash Balance - Cash balance stream - Cash Balance
                equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.cashBalance, pStreamB.cashBalance));
                #endregion Cash Balance
                #region Margin call
                //
                // Cash Balance - Cash balance stream - Margin call
                equal = equal && (pStreamA.marginCallSpecified == pStreamB.marginCallSpecified);
                if (equal && pStreamA.marginCallSpecified && pStreamB.marginCallSpecified)
                {
                    equal = equal && (CashBalanceComparer.SimplePaymentComparer.Equals(pStreamA.marginCall, pStreamB.marginCall));
                }
                #endregion Margin call
                #region Uncovered Margin requirement
                //
                // Cash Balance - Cash balance stream - Uncovered Margin requirement
                equal = equal && (pStreamA.uncoveredMarginRequirementSpecified == pStreamB.uncoveredMarginRequirementSpecified);
                if (equal && pStreamA.uncoveredMarginRequirementSpecified && pStreamB.uncoveredMarginRequirementSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.uncoveredMarginRequirement, pStreamB.uncoveredMarginRequirement));
                }
                #endregion Uncovered Margin requirement
                #region Collateral used
                //
                // Cash Balance - Cash balance stream - Collateral used
                equal = equal && (pStreamA.collateralUsedSpecified == pStreamB.collateralUsedSpecified);
                if (equal && pStreamA.collateralUsedSpecified && pStreamB.collateralUsedSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.collateralUsed, pStreamB.collateralUsed));
                }
                #endregion Collateral used
                #region Cash used
                //
                // Cash Balance - Cash balance stream - Cash used
                equal = equal && (pStreamA.cashUsedSpecified == pStreamB.cashUsedSpecified);
                if (equal && pStreamA.cashUsedSpecified && pStreamB.cashUsedSpecified)
                {
                    equal = equal && (CashBalanceComparer.CashPositionComparer.Equals(pStreamA.cashUsed, pStreamB.cashUsed));
                }
                #endregion Cash used
                #region Collateral
                //
                // Cash Balance - Cash balance stream - Collateral
                equal = equal && (pStreamA.collateralSpecified == pStreamB.collateralSpecified);
                if (equal && pStreamA.collateralSpecified && pStreamB.collateralSpecified)
                {
                    equal = equal && (CashBalanceComparer.PosCollateralComparer.ArrayEquals(pStreamA.collateral, pStreamB.collateral));
                }
                #endregion Collateral
                #region Collateral Available
                //
                // Cash Balance - Cash balance stream - Collateral Available
                equal = equal && (CashBalanceComparer.CollateralAvailableComparer.Equals(pStreamA.collateralAvailable, pStreamB.collateralAvailable));
                #endregion Collateral Available
                #region Cash Available
                //
                // Cash Balance - Cash balance stream - Cash Available
                equal = equal && CashBalanceComparer.CashAvailableComparer.Equals(pStreamA.cashAvailable, pStreamB.cashAvailable);
                #endregion Cash Available
                #region Margin Requirement
                //
                // Cash Balance - Cash balance stream - Margin requirement
                equal = equal && (CashBalanceComparer.CssExchangeCashPositionComparer.Equals(pStreamA.marginRequirement, pStreamB.marginRequirement));
                #endregion Margin Requirement
                #region Previous Margin requirement constituent
                //
                // Cash Balance - Cash balance stream - Previous Margin requirement constituent
                equal = equal && (CashBalanceComparer.PreviousMarginConstituentComparer.Equals(pStreamA.previousMarginConstituent, pStreamB.previousMarginConstituent));
                #endregion Previous Margin requirement constituent
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CashBalanceStream
        /// </summary>
        /// <param name="pStream"></param>
        /// <returns></returns>
        public override int GetHashCode(CashBalanceStream pStream)
        {
            decimal hCode = 0;
            if ((pStream != default(CashBalanceStream)) && (pStream.cashBalance != default(CashPosition)))
            {
                hCode = pStream.cashBalance.amount.amount.DecValue;
            }
            return hCode.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de CashBalanceStream sont équivalents
        /// </summary>
        /// <param name="pStreamA"></param>
        /// <param name="pStreamB"></param>
        /// <returns></returns>
        public bool ArrayEquals(CashBalanceStream[] pStreamA, CashBalanceStream[] pStreamB)
        {
            bool equal = (pStreamA == pStreamB);
            if (!equal && (pStreamA != default(CashBalanceStream[])) && (pStreamB != default(CashBalanceStream[])))
            {
                equal = (pStreamA.Count() == pStreamB.Count());
                if (equal)
                {
                    foreach (CashBalanceStream detailA in pStreamA)
                    {
                        if (detailA.currency != default(Currency))
                        {
                            CashBalanceStream detailB = pStreamB.FirstOrDefault(b => (b.currency != default(Currency)) && (b.currency.Value == detailA.currency.Value));
                            equal = equal && Equals(detailA, detailB);
                        }
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }
    #endregion Stream Class Comparer

    #region Object of Stream Class Comparer
    /// <summary>
    /// EqualityComparer pour AssetMarginConstituent
    /// </summary>
    public class AssetMarginConstituentComparer : EqualityComparer<AssetMarginConstituent>
    {
        /// <summary>
        /// Indique si deux AssetMarginConstituent sont équivalents
        /// </summary>
        /// <param name="pAssetMrgA"></param>
        /// <param name="pAssetMrgB"></param>
        /// <returns></returns>
        public override bool Equals(AssetMarginConstituent pAssetMrgA, AssetMarginConstituent pAssetMrgB)
        {
            bool equal = (pAssetMrgA == pAssetMrgB);
            if (!equal && (pAssetMrgA != default(AssetMarginConstituent)) && (pAssetMrgB != default(AssetMarginConstituent)))
            {
                equal = (pAssetMrgA.assetCategorySpecified == pAssetMrgB.assetCategorySpecified);
                if (equal && pAssetMrgA.assetCategorySpecified && pAssetMrgB.assetCategorySpecified)
                {
                    equal = (pAssetMrgA.assetCategory == pAssetMrgB.assetCategory);
                }
                equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pAssetMrgA, pAssetMrgB);
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un AssetMarginConstituent
        /// </summary>
        /// <param name="pAssetMrg"></param>
        /// <returns></returns>
        public override int GetHashCode(AssetMarginConstituent pAssetMrg)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pAssetMrg);
        }
    }

    /// <summary>
    /// EqualityComparer pour CashAvailable
    /// </summary>
    public class CashAvailableComparer : EqualityComparer<CashAvailable>
    {
        /// <summary>
        /// Indique si deux CashAvailable sont équivalents
        /// </summary>
        /// <param name="pCashAvailableA"></param>
        /// <param name="pCashAvailableB"></param>
        /// <returns></returns>
        public override bool Equals(CashAvailable pCashAvailableA, CashAvailable pCashAvailableB)
        {
            bool equal = (pCashAvailableA == pCashAvailableB);
            if (!equal && (pCashAvailableA != default(CashAvailable)) && (pCashAvailableB != default(CashAvailable)))
            {
                equal = CashBalanceComparer.CashPositionComparer.Equals(pCashAvailableA, pCashAvailableB);
                //
                // Cash Available - Constituent
                equal = equal && (pCashAvailableA.constituentSpecified == pCashAvailableB.constituentSpecified);
                if (equal && pCashAvailableA.constituentSpecified && pCashAvailableB.constituentSpecified)
                {
                    equal = ((pCashAvailableA.constituent == pCashAvailableB.constituent) || ((pCashAvailableA.constituent != default(CashAvailableConstituent)) && (pCashAvailableB.constituent != default(CashAvailableConstituent))));
                    if (equal && (pCashAvailableA.constituent != default(CashAvailableConstituent)) && (pCashAvailableB.constituent != default(CashAvailableConstituent)))
                    {
                        #region Cash Balance Payment
                        //
                        // Cash Available - Constituent - Cash Balance Payment
                        equal = equal && CashBalanceComparer.CashBalancePaymentComparer.Equals(pCashAvailableA.constituent.cashBalancePayment, pCashAvailableB.constituent.cashBalancePayment);
                        #endregion Cash Balance Payment
                        #region Previous Cash Balance
                        //
                        // Cash Available - Constituent - Previous Cash Balance
                        equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pCashAvailableA.constituent.previousCashBalance, pCashAvailableB.constituent.previousCashBalance);
                        #endregion Previous Cash Balance
                        #region Cash Flows
                        //
                        // Cash Available - Constituent - Cash Flows
                        equal = equal && CashBalanceComparer.CashFlowsComparer.Equals(pCashAvailableA.constituent.cashFlows, pCashAvailableB.constituent.cashFlows);
                        #endregion Cash Flows
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CashAvailable
        /// </summary>
        /// <param name="pCashAvailable"></param>
        /// <returns></returns>
        public override int GetHashCode(CashAvailable pCashAvailable)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pCashAvailable);
        }
    }

    /// <summary>
    /// EqualityComparer pour CashBalancePayment
    /// </summary>
    public class CashBalancePaymentComparer : EqualityComparer<CashBalancePayment>
    {
        /// <summary>
        /// Indique si deux CashBalancePayment sont équivalents
        /// </summary>
        /// <param name="pCBPaymentA"></param>
        /// <param name="pCBPaymentB"></param>
        /// <returns></returns>
        public override bool Equals(CashBalancePayment pCBPaymentA, CashBalancePayment pCBPaymentB)
        {
            bool equal = (pCBPaymentA == pCBPaymentB);
            if (!equal && (pCBPaymentA != default(CashBalancePayment)) && (pCBPaymentB != default(CashBalancePayment)))
            {
                // Cash Payment Global
                equal = CashBalanceComparer.CashPositionComparer.Equals(pCBPaymentA, pCBPaymentB);
                //
                // Cash Payment Deposit
                equal = equal && (pCBPaymentA.cashDepositSpecified == pCBPaymentB.cashDepositSpecified);
                if (equal && pCBPaymentA.cashDepositSpecified && pCBPaymentB.cashDepositSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pCBPaymentA.cashDeposit, pCBPaymentB.cashDeposit);
                }
                //
                // Cash Payment Withdrawal
                equal = equal && (pCBPaymentA.cashWithdrawalSpecified == pCBPaymentB.cashWithdrawalSpecified);
                if (equal && pCBPaymentA.cashWithdrawalSpecified && pCBPaymentB.cashWithdrawalSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pCBPaymentA.cashWithdrawal, pCBPaymentB.cashWithdrawal);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CashBalancePayment
        /// </summary>
        /// <param name="pCBPayment"></param>
        /// <returns></returns>
        public override int GetHashCode(CashBalancePayment pCBPayment)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pCBPayment);
        }
    }

    /// <summary>
    /// EqualityComparer pour CashFlows
    /// </summary>
    public class CashFlowsComparer : EqualityComparer<CashFlows>
    {
        /// <summary>
        /// Indique si deux CashFlows sont équivalents
        /// </summary>
        /// <param name="pCashFlowsA"></param>
        /// <param name="pCashFlowsB"></param>
        /// <returns></returns>
        public override bool Equals(CashFlows pCashFlowsA, CashFlows pCashFlowsB)
        {
            bool equal = (pCashFlowsA == pCashFlowsB);
            if (!equal && (pCashFlowsA != default(CashFlows)) && (pCashFlowsB != default(CashFlows)))
            {
                // Cash Flows
                equal = CashBalanceComparer.CashPositionComparer.Equals(pCashFlowsA, pCashFlowsB);
                //
                // Cash Flows - Constituent
                equal = equal && ((pCashFlowsA.constituent == pCashFlowsB.constituent) || ((pCashFlowsA.constituent != default(CashFlowsConstituent)) && (pCashFlowsB.constituent != default(CashFlowsConstituent))));
                if (equal && (pCashFlowsA.constituent != default(CashFlowsConstituent)) && (pCashFlowsB.constituent != default(CashFlowsConstituent)))
                {
                    CashFlowsConstituent cashFlowsConstituentA = pCashFlowsA.constituent;
                    CashFlowsConstituent cashFlowsConstituentB = pCashFlowsB.constituent;
                    #region Variation Margin
                    //
                    // Cash Flows - Constituent - Variation Margin
                    equal = equal && CashBalanceComparer.ContractSimplePaymentComparer.Equals(cashFlowsConstituentA.variationMargin, cashFlowsConstituentB.variationMargin);
                    #endregion Variation Margin
                    #region Premium
                    //
                    // Cash Flows - Constituent - Premium
                    equal = equal && CashBalanceComparer.ContractSimplePaymentComparer.Equals(cashFlowsConstituentA.premium, cashFlowsConstituentB.premium);
                    #endregion Premium
                    #region Cash Settlement
                    //
                    // Cash Flows - Constituent - Cash Settlement
                    equal = equal && CashBalanceComparer.ContractSimplePaymentConstituentComparer.Equals(cashFlowsConstituentA.cashSettlement, cashFlowsConstituentB.cashSettlement);
                    #endregion Cash Settlement
                    #region Fee
                    //
                    //  Cash Flows - Constituent - Fee
                    equal = equal && CashBalanceComparer.DetailedContractPaymentComparer.ArrayEquals(cashFlowsConstituentA.fee, cashFlowsConstituentB.fee);
                    #endregion Fee
                    #region Safekeeping
                    //
                    // Cash flows - Constituent - Safekeeping
                    equal = equal && (cashFlowsConstituentA.safekeepingSpecified == cashFlowsConstituentB.safekeepingSpecified);
                    if (equal && cashFlowsConstituentA.safekeepingSpecified && cashFlowsConstituentB.safekeepingSpecified)
                    {
                        equal = CashBalanceComparer.DetailedContractPaymentComparer.ArrayEquals(cashFlowsConstituentA.safekeeping, cashFlowsConstituentB.safekeeping);
                    }
                    #endregion Safekeeping
                    #region Equalisation Payment
                    //
                    // Cash Flows - Equalisation Payment
                    equal = equal && (cashFlowsConstituentA.equalisationPaymentSpecified == cashFlowsConstituentB.equalisationPaymentSpecified);
                    if (equal && cashFlowsConstituentA.equalisationPaymentSpecified && cashFlowsConstituentB.equalisationPaymentSpecified)
                    {
                        equal = CashBalanceComparer.ContractSimplePaymentComparer.Equals(cashFlowsConstituentA.equalisationPayment, cashFlowsConstituentB.equalisationPayment);
                    }
                    #endregion Equalisation Payment
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CashFlows
        /// </summary>
        /// <param name="pCashFlows"></param>
        /// <returns></returns>
        public override int GetHashCode(CashFlows pCashFlows)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pCashFlows);
        }
    }

    /// <summary>
    /// EqualityComparer pour CashPosition
    /// </summary>
    public class CashPositionComparer : EqualityComparer<CashPosition>
    {
        /// <summary>
        /// Indique si deux CashPosition sont équivalents
        /// </summary>
        /// <param name="pCashPosA"></param>
        /// <param name="pCashPosB"></param>
        /// <returns></returns>
        public override bool Equals(CashPosition pCashPosA, CashPosition pCashPosB)
        {
            bool equal = (pCashPosA == pCashPosB);
            if (!equal && (pCashPosA != default(CashPosition)) && (pCashPosB != default(CashPosition)))
            {
                equal = (pCashPosA.payerPartyReference.href == pCashPosB.payerPartyReference.href);
                equal = equal && (pCashPosA.receiverPartyReference.href == pCashPosB.receiverPartyReference.href);
                if ((pCashPosA.amount != default) && (pCashPosB.amount != default))
                {
                    equal = equal && (pCashPosA.amount.amount.DecValue == pCashPosB.amount.amount.DecValue);
                }
                else
                {
                    equal = equal && (pCashPosA.amount == pCashPosB.amount);
                }
                equal = equal && (pCashPosA.dateReferenceSpecified == pCashPosB.dateReferenceSpecified);
                if (equal && pCashPosA.dateReferenceSpecified && pCashPosB.dateReferenceSpecified)
                {
                    equal = (pCashPosA.dateReference.href == pCashPosB.dateReference.href);
                }
                equal = equal && (pCashPosA.dateDefineSpecified == pCashPosB.dateDefineSpecified);
                if (equal && pCashPosA.dateDefineSpecified && pCashPosB.dateDefineSpecified)
                {
                    equal = (pCashPosA.dateDefine.DateValue == pCashPosB.dateDefine.DateValue);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CashPosition
        /// </summary>
        /// <param name="pCashPos"></param>
        /// <returns></returns>
        public override int GetHashCode(CashPosition pCashPos)
        {
            decimal hCode = 0;
            if ((pCashPos != default(CashPosition)) && (pCashPos.amount != default))
            {
                hCode = pCashPos.amount.amount.DecValue;
            }
            return hCode.GetHashCode() ^ pCashPos.payerPartyReference.href.GetHashCode() ^ pCashPos.receiverPartyReference.href.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de CashPosition sont équivalents
        /// </summary>
        /// <param name="pCashPosA"></param>
        /// <param name="pCashPosB"></param>
        /// <returns></returns>
        public bool ArrayEquals(CashPosition[] pCashPosA, CashPosition[] pCashPosB)
        {
            bool equal = (pCashPosA == pCashPosB);
            if (!equal && (pCashPosA != default(CashPosition[])) && (pCashPosB != default(CashPosition[])))
            {
                equal = (pCashPosA.Count() == pCashPosB.Count());
                if (equal)
                {
                    foreach (CashPosition detailA in pCashPosA)
                    {
                        CashPosition detailB = pCashPosB.FirstOrDefault(b => (b.payerPartyReference.href == detailA.payerPartyReference.href) && (b.receiverPartyReference.href == detailA.receiverPartyReference.href));
                        equal = equal && Equals(detailA, detailB);
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }

    /// <summary>
    /// EqualityComparer pour CollateralAvailable
    /// </summary>
    public class CollateralAvailableComparer : EqualityComparer<CollateralAvailable>
    {
        /// <summary>
        /// Indique si deux CollateralAvailable sont équivalents
        /// </summary>
        /// <param name="pColatA"></param>
        /// <param name="pColatB"></param>
        /// <returns></returns>
        public override bool Equals(CollateralAvailable pColatA, CollateralAvailable pColatB)
        {
            bool equal = (pColatA == pColatB);
            if (!equal && (pColatA != default(CollateralAvailable)) && (pColatB != default(CollateralAvailable)))
            {
                equal = CashBalanceComparer.ExchangeCashPositionComparer.Equals(pColatA, pColatB);
                equal = equal && (pColatA.constituentSpecified == pColatB.constituentSpecified);
                if (equal && pColatA.constituentSpecified && pColatB.constituentSpecified)
                {
                    equal = (pColatA.constituent == pColatB.constituent);
                    if (!equal && (pColatA.constituent != default(CollateralAvailableConstituent)) && (pColatB.constituent != default(CollateralAvailableConstituent)))
                    {
                        equal = (pColatA.constituent.collateral == pColatB.constituent.collateral);
                        if (!equal && (pColatA.constituent.collateral != default) && (pColatB.constituent.collateral != default))
                        {
                            equal = (pColatA.constituent.collateral.Count() == pColatB.constituent.collateral.Count());
                            if (equal)
                            {
                                foreach (Money detailA in pColatA.constituent.collateral)
                                {
                                    Money detailB = pColatB.constituent.collateral.FirstOrDefault(b => (b.amount.DecValue == detailA.amount.DecValue) && (b.Currency == detailA.Currency));
                                    equal = equal && (detailB != default);
                                    if (!equal)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        equal = equal && (pColatA.constituent.collateralAlreadyUsedSpecified == pColatB.constituent.collateralAlreadyUsedSpecified);
                        if (equal && pColatA.constituent.collateralAlreadyUsedSpecified && pColatB.constituent.collateralAlreadyUsedSpecified)
                        {
                            equal = (pColatA.constituent.collateralAlreadyUsed == pColatB.constituent.collateralAlreadyUsed);
                            if (!equal && (pColatA.constituent.collateralAlreadyUsed != default) && (pColatB.constituent.collateralAlreadyUsed != default))
                            {
                                equal = (pColatA.constituent.collateralAlreadyUsed.Count() == pColatB.constituent.collateralAlreadyUsed.Count());
                                if (equal)
                                {
                                    foreach (Money detailA in pColatA.constituent.collateralAlreadyUsed)
                                    {
                                        Money detailB = pColatB.constituent.collateralAlreadyUsed.FirstOrDefault(b => (b.amount.DecValue == detailA.amount.DecValue) && (b.Currency == detailA.Currency));
                                        equal = equal && (detailB != default);
                                        if (!equal)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CollateralAvailable
        /// </summary>
        /// <param name="pCashPos"></param>
        /// <returns></returns>
        public override int GetHashCode(CollateralAvailable pColat)
        {
            return CashBalanceComparer.ExchangeCashPositionComparer.GetHashCode(pColat);
        }
    }

    /// <summary>
    /// EqualityComparer pour ContractAmount
    /// </summary>
    public class ContractAmountComparer : EqualityComparer<ContractAmount>
    {
        /// <summary>
        /// Indique si deux ContractAmount sont équivalents
        /// </summary>
        /// <param name="pCtrAmtA"></param>
        /// <param name="pCtrAmtB"></param>
        /// <returns></returns>
        public override bool Equals(ContractAmount pCtrAmtA, ContractAmount pCtrAmtB)
        {
            bool equal = (pCtrAmtA == pCtrAmtB);
            if (!equal && (pCtrAmtA != default(ContractAmount)) && (pCtrAmtB != default(ContractAmount)))
            {
                equal = (pCtrAmtA.AmtSideSpecified == pCtrAmtB.AmtSideSpecified);
                if (equal && pCtrAmtA.AmtSideSpecified && pCtrAmtB.AmtSideSpecified)
                {
                    equal = (pCtrAmtA.AmtSide == pCtrAmtB.AmtSide);
                }
                //
                equal = equal && (pCtrAmtA.Amt == pCtrAmtB.Amt);
                //
                equal = equal && (pCtrAmtA.assetCategorySpecified == pCtrAmtB.assetCategorySpecified);
                if (equal && pCtrAmtA.assetCategorySpecified && pCtrAmtB.assetCategorySpecified)
                {
                    equal = (pCtrAmtA.assetCategory == pCtrAmtB.assetCategory);
                }
                //
                equal = equal && (pCtrAmtA.otcmlIdSpecified == pCtrAmtB.otcmlIdSpecified);
                if (equal && pCtrAmtA.otcmlIdSpecified && pCtrAmtB.otcmlIdSpecified)
                {
                    equal = (pCtrAmtA.otcmlId == pCtrAmtB.otcmlId);
                }
                //
                equal = equal && (pCtrAmtA.SymSpecified == pCtrAmtB.SymSpecified);
                if (equal && pCtrAmtA.SymSpecified && pCtrAmtB.SymSpecified)
                {
                    equal = (pCtrAmtA.Sym == pCtrAmtB.Sym);
                }
                //
                equal = equal && (pCtrAmtA.ExchSpecified == pCtrAmtB.ExchSpecified);
                if (equal && pCtrAmtA.ExchSpecified && pCtrAmtB.ExchSpecified)
                {
                    equal = (pCtrAmtA.Exch == pCtrAmtB.Exch);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un ContractAmount
        /// </summary>
        /// <param name="pCtrAmt"></param>
        /// <returns></returns>
        public override int GetHashCode(ContractAmount pCtrAmt)
        {
            decimal hCode = 0;
            if (pCtrAmt != default(ContractAmount))
            {
                hCode = pCtrAmt.Amt;
            }
            return hCode.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de ContractAmount sont équivalents
        /// </summary>
        /// <param name="pCtrAmtA"></param>
        /// <param name="pCtrAmtB"></param>
        /// <returns></returns>
        public bool ArrayEquals(ContractAmount[] pCtrAmtA, ContractAmount[] pCtrAmtB)
        {
            bool equal = (pCtrAmtA == pCtrAmtB);
            if (!equal && (pCtrAmtA != default(ContractAmount[])) && (pCtrAmtB != default(ContractAmount[])))
            {
                equal = (pCtrAmtA.Count() == pCtrAmtB.Count());
                if (equal)
                {
                    foreach (ContractAmount detailA in pCtrAmtA)
                    {
                        if (detailA.assetCategorySpecified && detailA.otcmlIdSpecified)
                        {
                            ContractAmount detailB = pCtrAmtB.FirstOrDefault(b => b.assetCategorySpecified && b.otcmlIdSpecified && (b.otcmlId == detailA.otcmlId) && (b.assetCategory == detailA.assetCategory));
                            equal = equal && Equals(detailA, detailB);
                            if (!equal)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return equal;
        }
    }         

    /// <summary>
    /// EqualityComparer pour ContractAmountAndTax
    /// </summary>
    public class ContractAmountAndTaxComparer : EqualityComparer<ContractAmountAndTax>
    {
        /// <summary>
        /// Indique si deux ContractAmountAndTax sont équivalents
        /// </summary>
        /// <param name="pCtrAmtA"></param>
        /// <param name="pCtrAmtB"></param>
        /// <returns></returns>
        public override bool Equals(ContractAmountAndTax pCtrAmtA, ContractAmountAndTax pCtrAmtB)
        {
            bool equal = (pCtrAmtA == pCtrAmtB);
            if (!equal && (pCtrAmtA != default(ContractAmountAndTax)) && (pCtrAmtB != default(ContractAmountAndTax)))
            {
                equal = CashBalanceComparer.ContractAmountComparer.Equals(pCtrAmtA, pCtrAmtB);
                //
                equal = equal && (pCtrAmtA.taxSpecified == pCtrAmtB.taxSpecified);
                if (equal && pCtrAmtA.taxSpecified && pCtrAmtB.taxSpecified)
                {
                    equal = (pCtrAmtA.tax == pCtrAmtB.tax) || ((pCtrAmtA.tax != default(AmountSide)) && (pCtrAmtB.tax != default(AmountSide)));
                    if (equal && (pCtrAmtA.tax != default(AmountSide)) && (pCtrAmtB.tax != default(AmountSide)))
                    {
                        equal = (pCtrAmtA.tax.AmtSideSpecified == pCtrAmtB.tax.AmtSideSpecified);
                        if (equal && pCtrAmtA.tax.AmtSideSpecified && pCtrAmtB.tax.AmtSideSpecified)
                        {
                            equal = (pCtrAmtA.tax.AmtSide == pCtrAmtB.tax.AmtSide);
                        }
                        equal = equal && (pCtrAmtA.tax.Amt == pCtrAmtB.tax.Amt);
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un ContractAmountAndTax
        /// </summary>
        /// <param name="pCtrAmt"></param>
        /// <returns></returns>
        public override int GetHashCode(ContractAmountAndTax pCtrAmt)
        {
            return CashBalanceComparer.ContractAmountComparer.GetHashCode(pCtrAmt);
        }

        /// <summary>
        /// Indique si deux tableaux de ContractAmountAndTax sont équivalents
        /// </summary>
        /// <param name="pCtrAmtA"></param>
        /// <param name="pCtrAmtB"></param>
        /// <returns></returns>
        public bool ArrayEquals(ContractAmountAndTax[] pCtrAmtA, ContractAmountAndTax[] pCtrAmtB)
        {
            bool equal = (pCtrAmtA == pCtrAmtB);
            if (!equal && (pCtrAmtA != default(ContractAmountAndTax[])) && (pCtrAmtB != default(ContractAmountAndTax[])))
            {
                equal = (pCtrAmtA.Count() == pCtrAmtB.Count());
                if (equal)
                {
                    foreach (ContractAmountAndTax detailA in pCtrAmtA)
                    {
                        if (detailA.assetCategorySpecified && detailA.otcmlIdSpecified)
                        {
                            ContractAmountAndTax detailB = pCtrAmtB.FirstOrDefault(b => b.assetCategorySpecified && b.otcmlIdSpecified && (b.otcmlId == detailA.otcmlId) && (b.assetCategory == detailA.assetCategory));
                            equal = equal && Equals(detailA, detailB);
                            if (!equal)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return equal;
        }
    }         
    
    /// <summary>
    /// EqualityComparer pour ContractSimplePayment
    /// </summary>
    public class ContractSimplePaymentComparer : EqualityComparer<ContractSimplePayment>
    {
        /// <summary>
        /// Indique si deux ContractSimplePayment sont équivalents
        /// </summary>
        /// <param name="pPayA"></param>
        /// <param name="pPayB"></param>
        /// <returns></returns>
        public override bool Equals(ContractSimplePayment pPayA, ContractSimplePayment pPayB)
        {
            bool equal = (pPayA == pPayB);
            if (!equal && (pPayA != default(ContractSimplePayment)) && (pPayB != default(ContractSimplePayment)))
            {
                equal = (pPayA.payerPartyReference.href == pPayB.payerPartyReference.href);
                equal = equal && (pPayA.receiverPartyReference.href == pPayB.receiverPartyReference.href);
                if ((pPayA.paymentAmount != default) && (pPayB.paymentAmount != default))
                {
                    equal = equal && (pPayA.paymentAmount.amount.DecValue == pPayB.paymentAmount.amount.DecValue);
                }
                else
                {
                    equal = equal && (pPayA.paymentAmount == pPayB.paymentAmount);
                }
                if (equal && (pPayA.paymentDate != default(AdjustableOrRelativeAndAdjustedDate)) && (pPayB.paymentDate != default(AdjustableOrRelativeAndAdjustedDate)))
                {
                    equal = (pPayA.paymentDate.adjustedDateSpecified == pPayB.paymentDate.adjustedDateSpecified);
                    if (equal && pPayA.paymentDate.adjustedDateSpecified && pPayB.paymentDate.adjustedDateSpecified)
                    {
                        equal = (pPayA.paymentDate.adjustedDate.Value == pPayB.paymentDate.adjustedDate.Value);
                    }
                    equal = equal && (pPayA.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified == pPayB.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified);
                    if (equal && pPayA.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified && pPayB.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified)
                    {
                        AdjustableDate adjustableDateA = pPayA.paymentDate.adjustableOrRelativeDateAdjustableDate;
                        AdjustableDate adjustableDateB = pPayB.paymentDate.adjustableOrRelativeDateAdjustableDate;
                        equal = (adjustableDateA.Id == adjustableDateB.Id);
                        equal = equal && (adjustableDateA.unadjustedDate.Value == adjustableDateB.unadjustedDate.Value);
                        equal = equal && (adjustableDateA.dateAdjustments.Id == adjustableDateB.dateAdjustments.Id);
                        equal = equal && (adjustableDateA.dateAdjustments.businessDayConvention == adjustableDateB.dateAdjustments.businessDayConvention);
                        //
                        equal = equal && (adjustableDateA.dateAdjustments.businessCentersDefineSpecified == adjustableDateB.dateAdjustments.businessCentersDefineSpecified);
                        if (equal && adjustableDateA.dateAdjustments.businessCentersDefineSpecified && adjustableDateB.dateAdjustments.businessCentersDefineSpecified)
                        {
                            equal = CashBalanceComparer.BusinessCentersDefineEquals(adjustableDateA.dateAdjustments.businessCentersDefine, adjustableDateB.dateAdjustments.businessCentersDefine);
                        }
                    }
                    equal = equal && (pPayA.paymentDate.adjustableOrRelativeDateRelativeDateSpecified == pPayB.paymentDate.adjustableOrRelativeDateRelativeDateSpecified);
                    if (equal && pPayA.paymentDate.adjustableOrRelativeDateRelativeDateSpecified && pPayB.paymentDate.adjustableOrRelativeDateRelativeDateSpecified)
                    {
                        equal = (pPayA.paymentDate.adjustableOrRelativeDateRelativeDate.Id == pPayB.paymentDate.adjustableOrRelativeDateRelativeDate.Id);
                        equal = equal && (pPayA.paymentDate.adjustableOrRelativeDateRelativeDate.DateRelativeToValue == pPayB.paymentDate.adjustableOrRelativeDateRelativeDate.DateRelativeToValue);
                    }
                }
                else
                {
                    equal = equal && (pPayA.paymentDate == pPayB.paymentDate);
                }
                //
                equal = equal && (pPayA.detailSpecified == pPayB.detailSpecified);
                if (equal && pPayA.detailSpecified && pPayB.detailSpecified)
                {
                    equal = CashBalanceComparer.ContractAmountComparer.ArrayEquals(pPayA.detail, pPayB.detail);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un ContractSimplePayment
        /// </summary>
        /// <param name="pPay"></param>
        /// <returns></returns>
        public override int GetHashCode(ContractSimplePayment pPay)
        {
            decimal hCode = 0;
            if ((pPay != default(ContractSimplePayment)) && (pPay.paymentAmount != default))
            {
                hCode = pPay.paymentAmount.amount.DecValue;
            }
            return hCode.GetHashCode();
        }
    }

    /// <summary>
    /// EqualityComparer pour ContractSimplePaymentConstituent
    /// </summary>
    public class ContractSimplePaymentConstituentComparer : EqualityComparer<ContractSimplePaymentConstituent>
    {
        /// <summary>
        /// Indique si deux ContractSimplePaymentConstituent sont équivalents
        /// </summary>
        /// <param name="pPayA"></param>
        /// <param name="pPayB"></param>
        /// <returns></returns>
        public override bool Equals(ContractSimplePaymentConstituent pPayA, ContractSimplePaymentConstituent pPayB)
        {
            bool equal = (pPayA == pPayB);
            if (!equal && (pPayA != default(ContractSimplePaymentConstituent)) && (pPayB != default(ContractSimplePaymentConstituent)))
            {
                equal = CashBalanceComparer.ContractSimplePaymentComparer.Equals(pPayA, pPayB);
                //
                equal = equal && (pPayA.optionSpecified == pPayB.optionSpecified);
                if (equal && pPayA.optionSpecified && pPayB.optionSpecified)
                {
                    equal = CashBalanceComparer.OptionMarginConstituentComparer.ArrayEquals(pPayA.option, pPayB.option);
                    
                }
                //
                equal = equal && (pPayA.otherSpecified == pPayB.otherSpecified);
                if (equal && pPayA.otherSpecified && pPayB.otherSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pPayA.other, pPayB.other);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un ContractSimplePaymentConstituent
        /// </summary>
        /// <param name="pPay"></param>
        /// <returns></returns>
        public override int GetHashCode(ContractSimplePaymentConstituent pPay)
        {
            return CashBalanceComparer.ContractSimplePaymentComparer.GetHashCode(pPay);
        }
    }         

    /// <summary>
    /// EqualityComparer pour CssExchangeCashPosition
    /// </summary>
    public class CssExchangeCashPositionComparer : EqualityComparer<CssExchangeCashPosition>
    {
        /// <summary>
        /// Indique si deux  CssExchangeCashPosition sont équivalents
        /// </summary>
        /// <param name="pCashPosA"></param>
        /// <param name="pCashPosB"></param>
        /// <returns></returns>
        public override bool Equals(CssExchangeCashPosition pCashPosA, CssExchangeCashPosition pCashPosB)
        {
            bool equal = (pCashPosA == pCashPosB);
            if (!equal && (pCashPosA != default(CssExchangeCashPosition)) && (pCashPosB != default(CssExchangeCashPosition)))
            {
                equal = CashBalanceComparer.ExchangeCashPositionComparer.Equals(pCashPosA, pCashPosB);
                // Pas de detail si montant à 0
                if ((pCashPosA.amount != default) && (pCashPosB.amount != default)
                    && (pCashPosA.amount.amount.DecValue != 0) && (pCashPosB.amount.amount.DecValue != 0))
                {
                    equal = equal && (pCashPosA.detailSpecified == pCashPosB.detailSpecified);
                    if (equal && pCashPosA.detailSpecified == pCashPosB.detailSpecified)
                    {
                        equal = (pCashPosA.detail == pCashPosB.detail);
                        if (!equal && (pCashPosA.detail != default(CssAmount[])) && (pCashPosB.detail != default(CssAmount[])))
                        {
                            equal = (pCashPosA.detail.Count() == pCashPosB.detail.Count());
                            if (equal)
                            {
                                foreach (CssAmount detailA in pCashPosA.detail)
                                {
                                    CssAmount detailB;
                                    if (detailA.cssHrefSpecified)
                                    {
                                        detailB = pCashPosA.detail.FirstOrDefault(b => (b.cssHrefSpecified == detailA.cssHrefSpecified) && (b.cssHref == detailA.cssHref) && (b.Amt == detailA.Amt));
                                    }
                                    else
                                    {
                                        detailB = pCashPosA.detail.FirstOrDefault(b => (b.cssHrefSpecified == detailA.cssHrefSpecified) && (b.Amt == detailA.Amt));
                                    }
                                    equal = equal && (detailB != default(CssAmount));
                                    if (equal)
                                    {
                                        equal = (detailA.AmtSideSpecified == detailB.AmtSideSpecified);
                                        if (equal && detailA.AmtSideSpecified && detailB.AmtSideSpecified)
                                        {
                                            equal = (detailA.AmtSide == detailB.AmtSide);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un CssExchangeCashPosition
        /// </summary>
        /// <param name="pCashPos"></param>
        /// <returns></returns>
        public override int GetHashCode(CssExchangeCashPosition pCashPos)
        {
            return CashBalanceComparer.ExchangeCashPositionComparer.GetHashCode(pCashPos);
        }
    }

    /// <summary>
    /// EqualityComparer pour DetailedCashPosition
    /// </summary>
    public class DetailedCashPositionComparer : EqualityComparer<DetailedCashPosition>
    {
        /// <summary>
        /// Indique si deux DetailedCashPosition sont équivalents
        /// </summary>
        /// <param name="pCashPosA"></param>
        /// <param name="pCashPosB"></param>
        /// <returns></returns>
        public override bool Equals(DetailedCashPosition pCashPosA, DetailedCashPosition pCashPosB)
        {
            bool equal = (pCashPosA == pCashPosB);
            if (!equal && (pCashPosA != default(DetailedCashPosition)) && (pCashPosB != default(DetailedCashPosition)))
            {
                equal = CashBalanceComparer.CashPositionComparer.Equals(pCashPosA, pCashPosB);
                equal = equal && (pCashPosA.dateDetailSpecified == pCashPosB.dateDetailSpecified);
                if (equal && pCashPosA.dateDetailSpecified && pCashPosB.dateDetailSpecified)
                {
                    equal = CashBalanceComparer.DetailedDateAmountComparer.ArrayEquals(pCashPosA.dateDetail, pCashPosB.dateDetail);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un DetailedCashPosition
        /// </summary>
        /// <param name="pCashPos"></param>
        /// <returns></returns>
        public override int GetHashCode(DetailedCashPosition pCashPos)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pCashPos);
        }
    }

    /// <summary>
    /// EqualityComparer pour DetailedContractPayment
    /// </summary>
    public class DetailedContractPaymentComparer : EqualityComparer<DetailedContractPayment>
    {
        /// <summary>
        /// Indique si deux DetailedContractPayment sont équivalents
        /// </summary>
        /// <param name="pPayA"></param>
        /// <param name="pPayB"></param>
        /// <returns></returns>
        public override bool Equals(DetailedContractPayment pPayA, DetailedContractPayment pPayB)
        {
            bool equal = (pPayA == pPayB);
            if (!equal && (pPayA != default(DetailedContractPayment)) && (pPayB != default(DetailedContractPayment)))
            {
                equal = (pPayA.type == pPayB.type);
                equal = equal && (pPayA.payerPartyReference.href == pPayB.payerPartyReference.href);
                equal = equal && (pPayA.receiverPartyReference.href == pPayB.receiverPartyReference.href);
                if ((pPayA.paymentAmount != default) && (pPayB.paymentAmount != default))
                {
                    equal = equal && (pPayA.paymentAmount.amount.DecValue == pPayB.paymentAmount.amount.DecValue);
                }
                else
                {
                    equal = equal && (pPayA.paymentAmount == pPayB.paymentAmount);
                }
                //
                // PaymentDate
                equal = equal && (pPayA.paymentDateSpecified == pPayB.paymentDateSpecified);
                if (equal && pPayA.paymentDateSpecified && pPayB.paymentDateSpecified)
                {
                    if ((pPayA.paymentDate != default(AdjustableDate)) && (pPayB.paymentDate != default(AdjustableDate)))
                    {
                        equal = (pPayA.paymentDate.unadjustedDate.DateValue == pPayB.paymentDate.unadjustedDate.DateValue);
                        equal = equal && (pPayA.paymentDate.dateAdjustments.businessCentersDefineSpecified == pPayB.paymentDate.dateAdjustments.businessCentersDefineSpecified);
                        if (equal && pPayA.paymentDate.dateAdjustments.businessCentersDefineSpecified && pPayB.paymentDate.dateAdjustments.businessCentersDefineSpecified)
                        {
                            equal = CashBalanceComparer.BusinessCentersDefineEquals(pPayA.paymentDate.dateAdjustments.businessCentersDefine, pPayB.paymentDate.dateAdjustments.businessCentersDefine);
                            equal = equal && (pPayA.paymentDate.dateAdjustments.businessDayConvention == pPayB.paymentDate.dateAdjustments.businessDayConvention);
                        }
                    }
                    else
                    {
                        equal = (pPayA.paymentDate == pPayB.paymentDate);
                    }
                }
                //
                // PaymentType
                equal = equal && (pPayA.paymentTypeSpecified == pPayB.paymentTypeSpecified);
                if (equal && pPayA.paymentTypeSpecified && pPayB.paymentTypeSpecified)
                {
                    equal = ((pPayA.paymentType == pPayB.paymentType) || ((pPayA.paymentType != default(PaymentType)) && (pPayB.paymentType != default(PaymentType))));
                    if (equal && (pPayA.paymentType != default(PaymentType)) && (pPayB.paymentType != default(PaymentType)))
                    {
                        equal = (pPayA.paymentType.Value == pPayB.paymentType.Value);
                    }
                }
                //
                // PaymentSource
                equal = equal && (pPayA.paymentSourceSpecified == pPayB.paymentSourceSpecified);
                if ( equal && pPayA.paymentSourceSpecified && pPayB.paymentSourceSpecified)
                {
                    equal = ((pPayA.paymentSource == pPayB.paymentSource) || ((pPayA.paymentSource != default(SpheresSource)) && (pPayB.paymentSource != default(SpheresSource))));
                    if (equal && (pPayA.paymentSource != default(SpheresSource)) && (pPayB.paymentSource != default(SpheresSource)))
                    {
                        SpheresId[] spheresIdA = pPayA.paymentSource.spheresId;
                        SpheresId[] spheresIdB = pPayB.paymentSource.spheresId;
                        equal = ((spheresIdA == spheresIdB) || ((spheresIdA != default(SpheresId[])) && (spheresIdB != default(SpheresId[]))));
                        if (equal && ((spheresIdA != default(SpheresId[])) && (spheresIdB != default(SpheresId[]))))
                        {
                            equal = (spheresIdA.Count() == spheresIdB.Count());
                            if (equal)
                            {
                                foreach (SpheresId detailA in spheresIdA)
                                {
                                    SpheresId detailB = spheresIdB.FirstOrDefault(b => b.scheme == detailA.scheme);
                                    equal = equal && (detailB != default(SpheresId));
                                    if (equal)
                                    {
                                        equal = equal && (detailA.Value == detailB.Value);
                                        equal = equal && (detailA.otcmlId == detailB.otcmlId);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        equal = equal && (pPayA.paymentSource.statusSpecified == pPayB.paymentSource.statusSpecified);
                        if (equal && pPayA.paymentSource.statusSpecified && pPayB.paymentSource.statusSpecified)
                        {
                            equal = (pPayA.paymentSource.status == pPayB.paymentSource.status);
                        }
                    }
                }
                //
                // Tax
                equal = equal && (pPayA.taxSpecified == pPayB.taxSpecified);
                if (equal && pPayA.taxSpecified && pPayB.taxSpecified)
                {
                    equal = (pPayA.tax == pPayB.tax) || ((pPayA.tax != default(Tax[])) && (pPayB.tax != default(Tax[])));
                    if (equal && (pPayA.tax != default(Tax[])) && (pPayB.tax != default(Tax[])))
                    {
                        equal = (pPayA.tax.Count() == pPayB.tax.Count());
                        if (equal)
                        {
                            foreach (Tax detailA in pPayA.tax)
                            {
                                // Pour chaque Tax A recherche de la Tax B correspondante par rapport à l'OTCmlId présent dans les SpheresId de taxsource
                                if ((detailA.taxSource != default(SpheresSource)) && (detailA.taxSource.spheresId != default(SpheresId[])))
                                {
                                    SpheresId spheresIdA = detailA.taxSource.spheresId.FirstOrDefault(a => a.scheme == Cst.OTCml_RepositoryTaxScheme);
                                    if (spheresIdA != default(SpheresId))
                                    {
                                        Tax detailB = pPayB.tax.FirstOrDefault(b => (b.taxSource != default(SpheresSource))
                                            && (b.taxSource.spheresId != default(SpheresId[]))
                                            && b.taxSource.spheresId.FirstOrDefault(id => (id.scheme == Cst.OTCml_RepositoryTaxScheme) && (id.otcmlId == spheresIdA.otcmlId)) != default(SpheresId));
                                        //
                                        equal = equal && (detailB != default(Tax));
                                        if (equal)
                                        {
                                            equal = equal && TaxScheduleArraysEquals(detailA.taxDetail, detailB.taxDetail);
                                        }
                                    }
                                }
                                if (!equal)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                //
                // Detail
                equal = equal && (pPayA.detailSpecified == pPayB.detailSpecified);
                if (equal && pPayA.detailSpecified && pPayB.detailSpecified)
                {
                    equal = CashBalanceComparer.ContractAmountAndTaxComparer.ArrayEquals(pPayA.detail, pPayB.detail);
                }
            }
            return equal;
        }

        /// <summary>
        /// Indique si deux tableaux de TaxSchedule sont équivalents
        /// </summary>
        /// <param name="pTaxScheduleA"></param>
        /// <param name="pTaxScheduleB"></param>
        /// <returns></returns>
        private bool TaxScheduleArraysEquals(TaxSchedule[] pTaxScheduleA, TaxSchedule[] pTaxScheduleB)
        {
            bool equal = (pTaxScheduleA == pTaxScheduleB);
            if (!equal && (pTaxScheduleA != default(TaxSchedule[])) && (pTaxScheduleB != default(TaxSchedule[])))
            {
                equal = (pTaxScheduleA.Count() == pTaxScheduleB.Count());
                if (equal)
                {
                    foreach (TaxSchedule detailA in pTaxScheduleA)
                    {
                        // Recherche du TaxSchedule B correspondant au TaxSchedule A
                        if (detailA.taxSource != default(SpheresSource))
                        {
                            if (detailA.taxSource.spheresId != default(SpheresId[]))
                            {
                                SpheresId spheresIdA = detailA.taxSource.spheresId.FirstOrDefault(a => (a.scheme == Cst.OTCml_RepositoryTaxDetailScheme));
                                if (spheresIdA != default(SpheresId))
                                {
                                    TaxSchedule detailB = pTaxScheduleB.FirstOrDefault(b => (b.taxSource != default(SpheresSource))
                                        && (b.taxSource.spheresId != default(SpheresId[]))
                                        && (b.taxSource.spheresId.FirstOrDefault(bs => (bs.scheme == Cst.OTCml_RepositoryTaxDetailScheme) && (bs.otcmlId == spheresIdA.otcmlId))) != default(SpheresId));
                                    //
                                    equal = equal && (detailB != default(TaxSchedule));
                                    if (equal)
                                    {
                                        // Amount
                                        equal = equal && (detailA.taxAmountSpecified == detailB.taxAmountSpecified);
                                        if (equal && detailA.taxAmountSpecified && detailB.taxAmountSpecified)
                                        {
                                            equal = equal && ((detailA.taxAmount == detailB.taxAmount) || ((detailA.taxAmount != default(TripleInvoiceAmounts)) && (detailB.taxAmount != default(TripleInvoiceAmounts))));
                                            if (equal && (detailA.taxAmount != default(TripleInvoiceAmounts)) && (detailB.taxAmount != default(TripleInvoiceAmounts)))
                                            {
                                                equal = equal && (detailA.taxAmount.amount.amount.DecValue == detailB.taxAmount.amount.amount.DecValue);
                                            }
                                        }
                                        if (equal)
                                        {
                                            // Source SpheresId
                                            foreach (SpheresId detailSSA in detailA.taxSource.spheresId)
                                            {
                                                SpheresId detailSSB = detailB.taxSource.spheresId.FirstOrDefault(b => (b.scheme == detailSSA.scheme) && (b.Value == detailSSA.Value));
                                                equal = equal && (detailSSB != default(SpheresId));
                                                if (!equal)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un DetailedContractPayment
        /// </summary>
        /// <param name="pPay"></param>
        /// <returns></returns>
        public override int GetHashCode(DetailedContractPayment pPay)
        {
            decimal hCode = 0;
            if ((pPay != default(DetailedContractPayment)) && (pPay.paymentAmount != default))
            {
                hCode = pPay.paymentAmount.amount.DecValue;
            }
            return hCode.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de DetailedContractPayment sont équivalents
        /// </summary>
        /// <param name="pCashPosA"></param>
        /// <param name="pCashPosB"></param>
        /// <returns></returns>
        public bool ArrayEquals(DetailedContractPayment[] pPayA, DetailedContractPayment[] pPayB)
        {
            bool equal = (pPayA == pPayB);
            if (!equal && (pPayA != default(DetailedContractPayment[])) && (pPayB != default(DetailedContractPayment[])))
            {
                equal = (pPayA.Count() == pPayB.Count());
                if (equal)
                {
                    foreach (DetailedContractPayment detailA in pPayA)
                    {
                        if (detailA.paymentTypeSpecified)
                        {
                            DetailedContractPayment detailB = pPayB.FirstOrDefault(b => (b.payerPartyReference.href == detailA.payerPartyReference.href)
                                && (b.receiverPartyReference.href == detailA.receiverPartyReference.href)
                                && (b.paymentAmount.amount.DecValue == detailA.paymentAmount.amount.DecValue)
                                && (b.paymentTypeSpecified == detailA.paymentTypeSpecified)
                                && (b.paymentType != default(PaymentType)) && (detailA.paymentType != default(PaymentType))
                                && (b.paymentType.Value == detailA.paymentType.Value)
                                );
                            equal = equal && Equals(detailA, detailB);
                        }
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }

    /// <summary>
    /// EqualityComparer pour DetailedDateAmount
    /// </summary>
    public class DetailedDateAmountComparer : EqualityComparer<DetailedDateAmount>
    {
        /// <summary>
        /// Indique si deux DetailedDateAmount sont équivalents
        /// </summary>
        /// <param name="pDetDtAmtA"></param>
        /// <param name="pDetDtAmtB"></param>
        /// <returns></returns>
        public override bool Equals(DetailedDateAmount pDetDtAmtA, DetailedDateAmount pDetDtAmtB)
        {
            bool equal = (pDetDtAmtA == pDetDtAmtB);
            if (!equal && (pDetDtAmtA != default(DetailedDateAmount)) && (pDetDtAmtB != default(DetailedDateAmount)))
            {
                equal = (pDetDtAmtA.Amt == pDetDtAmtB.Amt);
                equal = equal && (pDetDtAmtA.AmtSideSpecified == pDetDtAmtB.AmtSideSpecified);
                if (equal && pDetDtAmtA.AmtSideSpecified && pDetDtAmtB.AmtSideSpecified)
                {
                    equal = (pDetDtAmtA.AmtSide == pDetDtAmtB.AmtSide);
                }
                equal = equal && (pDetDtAmtA.detailSpecified == pDetDtAmtB.detailSpecified);
                if (equal && pDetDtAmtA.detailSpecified && pDetDtAmtB.detailSpecified)
                {
                    equal = CashBalanceComparer.ContractAmountComparer.ArrayEquals(pDetDtAmtA.detail, pDetDtAmtB.detail);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un DetailedDateAmount
        /// </summary>
        /// <param name="pDetDtAmt"></param>
        /// <returns></returns>
        public override int GetHashCode(DetailedDateAmount pDetDtAmt)
        {
            decimal hCode = 0;
            if (pDetDtAmt != default(DetailedDateAmount))
            {
                hCode = pDetDtAmt.Amt;
            }
            return hCode.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de DetailedDateAmount sont équivalents
        /// </summary>
        /// <param name="pDetDtAmtA"></param>
        /// <param name="pDetDtAmtB"></param>
        /// <returns></returns>
        public bool ArrayEquals(DetailedDateAmount[] pDetDtAmtA, DetailedDateAmount[] pDetDtAmtB)
        {
            bool equal = ((pDetDtAmtA == pDetDtAmtB) || ((pDetDtAmtA != default(DetailedDateAmount[])) && (pDetDtAmtB != default(DetailedDateAmount[]))));
            if (equal && (pDetDtAmtA != default(DetailedDateAmount[])) && (pDetDtAmtB != default(DetailedDateAmount[])))
            {
                equal = (pDetDtAmtA.Count() == pDetDtAmtB.Count());
                if (equal)
                {
                    foreach (DetailedDateAmount detailA in pDetDtAmtA)
                    {
                        DetailedDateAmount detailB = pDetDtAmtB.FirstOrDefault(b => (b.ValueDate == detailA.ValueDate));
                        equal = equal && Equals(detailA, detailB);
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }

    /// <summary>
    /// EqualityComparer pour ExchangeCashPosition
    /// </summary>
    public class ExchangeCashPositionComparer : EqualityComparer<ExchangeCashPosition>
    {
        /// <summary>
        /// Indique si deux ExchangeCashPosition sont équivalents
        /// </summary>
        /// <param name="pCashPosA"></param>
        /// <param name="pCashPosB"></param>
        /// <returns></returns>
        public override bool Equals(ExchangeCashPosition pCashPosA, ExchangeCashPosition pCashPosB)
        {
            bool equal = (pCashPosA == pCashPosB);
            if (!equal && (pCashPosA != default(ExchangeCashPosition)) && (pCashPosB != default(ExchangeCashPosition)))
            {
                equal = CashBalanceComparer.CashPositionComparer.Equals(pCashPosA, pCashPosB);
                equal = equal && (pCashPosA.exchangeAmountSpecified == pCashPosB.exchangeAmountSpecified);
                if (equal && pCashPosA.exchangeAmountSpecified && pCashPosB.exchangeAmountSpecified)
                {
                    equal = ((pCashPosA.exchangeAmount == pCashPosB.exchangeAmount) || ((pCashPosA.exchangeAmount != default) && (pCashPosB.exchangeAmount != default)));
                    if (equal && (pCashPosA.exchangeAmount != default) && (pCashPosB.exchangeAmount != default))
                    {
                        equal = (pCashPosA.exchangeAmount.amount.DecValue == pCashPosB.exchangeAmount.amount.DecValue);
                        equal = equal && (pCashPosA.exchangeAmount.Currency == pCashPosB.exchangeAmount.Currency);
                    }
                }
                equal = equal && (pCashPosA.exchangeFxRateReferenceSpecified == pCashPosB.exchangeFxRateReferenceSpecified);
                if (equal && pCashPosA.exchangeFxRateReferenceSpecified && pCashPosB.exchangeFxRateReferenceSpecified)
                {
                    equal = (pCashPosA.exchangeFxRateReference == pCashPosB.exchangeFxRateReference);
                    if (!equal && (pCashPosA.exchangeFxRateReference != default(FxRateReference[] )) && (pCashPosB.exchangeFxRateReference != default(FxRateReference[])))
                    {
                        equal = (pCashPosA.exchangeFxRateReference.Count() == pCashPosB.exchangeFxRateReference.Count());
                        if (equal)
                        {
                            equal = (pCashPosA.exchangeFxRateReference.Select(a=>a.href).Intersect(pCashPosB.exchangeFxRateReference.Select(b=>b.href)).Count() == 0);
                        }
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un ExchangeCashPosition
        /// </summary>
        /// <param name="pCashPos"></param>
        /// <returns></returns>
        public override int GetHashCode(ExchangeCashPosition pCashPos)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pCashPos);
        }
    }

    /// <summary>
    /// EqualityComparer pour MarginConstituent
    /// </summary>
    public class MarginConstituentComparer : EqualityComparer<MarginConstituent>
    {
        /// <summary>
        /// Indique si deux MarginConstituent sont équivalents
        /// </summary>
        /// <param name="pMrgA"></param>
        /// <param name="pMrgB"></param>
        /// <returns></returns>
        public override bool Equals(MarginConstituent pMrgA, MarginConstituent pMrgB)
        {
            bool equal = (pMrgA == pMrgB);
            if (!equal && (pMrgA != default(MarginConstituent)) && (pMrgB != default(MarginConstituent)))
            {
                // Montant global
                equal = (pMrgA.globalAmountSpecified == pMrgB.globalAmountSpecified);
                if (equal && pMrgA.globalAmountSpecified && pMrgB.globalAmountSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pMrgA.globalAmount, pMrgB.globalAmount);
                }
                //
                // Montant future
                equal = equal && (pMrgA.futureSpecified == pMrgB.futureSpecified);
                if (equal && pMrgA.futureSpecified && pMrgB.futureSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pMrgA.future, pMrgB.future);
                }
                //
                // Montant option
                equal = equal && (pMrgA.optionSpecified == pMrgB.optionSpecified);
                if (equal && pMrgA.optionSpecified && pMrgB.optionSpecified)
                {
                    equal = (pMrgA.option.Count() == pMrgB.option.Count());
                    if (equal)
                    {
                        foreach (OptionMarginConstituent optMrgA in pMrgA.option)
                        {
                            OptionMarginConstituent optMrgB = pMrgB.option.FirstOrDefault(b => b.valuationMethod == optMrgA.valuationMethod);
                            equal = equal && CashBalanceComparer.OptionMarginConstituentComparer.Equals(optMrgA, optMrgB);
                            if (!equal)
                            {
                                break;
                            }
                        }
                    }
                }
                //
                // Montant autres (non ETD)
                equal = equal && (pMrgA.otherSpecified == pMrgB.otherSpecified);
                if (equal && pMrgA.otherSpecified && pMrgB.otherSpecified)
                {
                    equal = (pMrgA.other.Count() == pMrgB.other.Count());
                    if (equal)
                    {
                        foreach (AssetMarginConstituent assetMrgA in pMrgA.other)
                        {
                            if (assetMrgA.assetCategorySpecified)
                            {
                                AssetMarginConstituent assetMrgB = pMrgB.other.FirstOrDefault(b => b.assetCategorySpecified && (b.assetCategory == assetMrgA.assetCategory));
                                equal = equal && CashBalanceComparer.AssetMarginConstituentComparer.Equals(assetMrgA, assetMrgB);
                            }
                            if (!equal)
                            {
                                break;
                            }
                        }
                    }
                }
                //
                // Détail des montants
                equal = equal && (pMrgA.detailSpecified == pMrgB.detailSpecified);
                if (equal && pMrgA.detailSpecified && pMrgB.detailSpecified)
                {
                    equal = CashBalanceComparer.ContractAmountComparer.ArrayEquals(pMrgA.detail, pMrgB.detail);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un MarginConstituent
        /// </summary>
        /// <param name="pMrg"></param>
        /// <returns></returns>
        public override int GetHashCode(MarginConstituent pMrg)
        {
            decimal hCode = 0;
            if ((pMrg != default(MarginConstituent)) && (pMrg.globalAmountSpecified))
            {
                hCode = CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.globalAmount);
            }
            return hCode.GetHashCode();
        }
    }

    /// <summary>
    /// EqualityComparer pour OptionLiquidatingValue
    /// </summary>
    public class OptionLiquidatingValueComparer : EqualityComparer<OptionLiquidatingValue>
    {
        /// <summary>
        /// Indique si deux OptionLiquidatingValue sont équivalents
        /// </summary>
        /// <param name="pLOVA"></param>
        /// <param name="pLOVB"></param>
        /// <returns></returns>
        public override bool Equals(OptionLiquidatingValue pLOVA, OptionLiquidatingValue pLOVB)
        {
            bool equal = (pLOVA == pLOVB);
            if (!equal && (pLOVA != default(OptionLiquidatingValue)) && (pLOVB != default(OptionLiquidatingValue)))
            {
                equal = CashBalanceComparer.CashPositionComparer.Equals(pLOVA, pLOVB);
                //
                // Long Option Value
                equal = equal && (pLOVA.longOptionValueSpecified == pLOVB.longOptionValueSpecified);
                if (equal && pLOVA.longOptionValueSpecified && pLOVB.longOptionValueSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pLOVA.longOptionValue, pLOVB.longOptionValue);
                }
                //
                // Short Option Value
                equal = equal && (pLOVA.shortOptionValueSpecified == pLOVB.shortOptionValueSpecified);
                if (equal && pLOVA.shortOptionValueSpecified && pLOVB.shortOptionValueSpecified)
                {
                    equal = CashBalanceComparer.CashPositionComparer.Equals(pLOVA.shortOptionValue, pLOVB.shortOptionValue);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un OptionLiquidatingValue
        /// </summary>
        /// <param name="pLOV"></param>
        /// <returns></returns>
        public override int GetHashCode(OptionLiquidatingValue pLOV)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pLOV);
        }
    }

    /// <summary>
    /// EqualityComparer pour OptionMarginConstituent
    /// </summary>
    public class OptionMarginConstituentComparer : EqualityComparer<OptionMarginConstituent>
    {
        /// <summary>
        /// Indique si deux OptionMarginConstituent sont équivalents
        /// </summary>
        /// <param name="pOptMrgA"></param>
        /// <param name="pOptMrgB"></param>
        /// <returns></returns>
        public override bool Equals(OptionMarginConstituent pOptMrgA, OptionMarginConstituent pOptMrgB)
        {
            bool equal = (pOptMrgA == pOptMrgB);
            if (!equal && (pOptMrgA != default(OptionMarginConstituent)) && (pOptMrgB != default(OptionMarginConstituent)))
            {
                equal = CashBalanceComparer.CashPositionComparer.Equals(pOptMrgA, pOptMrgB);
                equal = equal && (pOptMrgA.valuationMethod == pOptMrgB.valuationMethod);
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un OptionMarginConstituent
        /// </summary>
        /// <param name="pOptMrg"></param>
        /// <returns></returns>
        public override int GetHashCode(OptionMarginConstituent pOptMrg)
        {
            return CashBalanceComparer.CashPositionComparer.GetHashCode(pOptMrg);
        }

        /// <summary>
        /// Indique si deux tableaux de OptionMarginConstituent sont équivalents
        /// </summary>
        /// <param name="pOptMrgA"></param>
        /// <param name="pOptMrgB"></param>
        /// <returns></returns>
        public bool ArrayEquals(OptionMarginConstituent[] pOptMrgA, OptionMarginConstituent[] pOptMrgB)
        {
            bool equal = (pOptMrgA == pOptMrgB);
            if (!equal && (pOptMrgA != default(OptionMarginConstituent[])) && (pOptMrgB != default(OptionMarginConstituent[])))
            {
                equal = (pOptMrgA.Count() == pOptMrgB.Count());
                if (equal)
                {
                    foreach (OptionMarginConstituent detailA in pOptMrgA)
                    {
                        OptionMarginConstituent detailB = pOptMrgB.FirstOrDefault(b => (b.valuationMethod == detailA.valuationMethod));
                        equal = equal && Equals(detailA, detailB);
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }

    /// <summary>
    /// EqualityComparer pour PosCollateral
    /// </summary>
    public class PosCollateralComparer : EqualityComparer<PosCollateral>
    {
        /// <summary>
        /// Indique si deux PosCollateral sont équivalents
        /// </summary>
        /// <param name="pColatA"></param>
        /// <param name="pColatB"></param>
        /// <returns></returns>
        public override bool Equals(PosCollateral pColatA, PosCollateral pColatB)
        {
            bool equal = (pColatA == pColatB);
            if (!equal && (pColatA != default(PosCollateral)) && (pColatB != default(PosCollateral)))
            {
                equal = (pColatA.otcmlId == pColatB.otcmlId);
                // Asset
                equal = equal && ((pColatA.asset == pColatB.asset) || ((pColatA.asset != default(ContractAsset)) && (pColatB.asset != default(ContractAsset))));
                if (equal && (pColatA.asset != default(ContractAsset)) && (pColatB.asset != default(ContractAsset)))
                {
                    equal = (pColatA.asset.assetCategorySpecified == pColatB.asset.assetCategorySpecified);
                    if (equal && pColatA.asset.assetCategorySpecified && pColatB.asset.assetCategorySpecified)
                    {
                        equal = (pColatA.asset.assetCategory == pColatB.asset.assetCategory);
                    }
                    equal = equal && (pColatA.asset.ExchSpecified == pColatB.asset.ExchSpecified);
                    if (equal && pColatA.asset.ExchSpecified && pColatB.asset.ExchSpecified)
                    {
                        equal = (pColatA.asset.Exch == pColatB.asset.Exch);
                    }
                    equal = equal && (pColatA.asset.otcmlIdSpecified == pColatB.asset.otcmlIdSpecified);
                    if (equal && pColatA.asset.otcmlIdSpecified && pColatB.asset.otcmlIdSpecified)
                    {
                        equal = (pColatA.asset.otcmlId == pColatB.asset.otcmlId);
                    }
                    equal = equal && (pColatA.asset.SymSpecified == pColatB.asset.SymSpecified);
                    if (equal && pColatA.asset.SymSpecified && pColatB.asset.SymSpecified)
                    {
                        equal = (pColatA.asset.Sym == pColatB.asset.Sym);
                    }
                }
                // Book
                equal = equal && ((pColatA.bookId == pColatB.bookId) || ((pColatA.bookId != default(BookId)) && (pColatB.bookId != default(BookId))));
                if (equal && (pColatA.bookId != default(BookId)) && (pColatB.bookId != default(BookId)))
                {
                    // PM 20201202 [25592] ne pas comparer bookIdScheme
                    //equal = (pColatA.bookId.bookIdScheme == pColatB.bookId.bookIdScheme);
                    equal = equal && (pColatA.bookId.bookNameSpecified == pColatB.bookId.bookNameSpecified);
                    if (equal && pColatA.bookId.bookNameSpecified && pColatB.bookId.bookNameSpecified)
                    {
                        equal = (pColatA.bookId.bookName == pColatB.bookId.bookName);
                    }
                    equal = equal && (pColatA.bookId.otcmlId == pColatB.bookId.otcmlId);
                    equal = equal && (pColatA.bookId.Value == pColatB.bookId.Value);
                }
                // MarketValue
                equal = equal && ((pColatA.valuation == pColatB.valuation) || ((pColatA.valuation != default(PosCollateralValuation)) && (pColatB.valuation != default(PosCollateralValuation))));
                if (equal && (pColatA.valuation != default(PosCollateralValuation)) && (pColatB.valuation != default(PosCollateralValuation)))
                {
                    equal = (pColatA.valuation.Amt == pColatB.valuation.Amt);
                    equal = equal && (pColatA.valuation.AmtSideSpecified == pColatB.valuation.AmtSideSpecified);
                    if (equal && pColatA.valuation.AmtSideSpecified && pColatB.valuation.AmtSideSpecified)
                    {
                        equal = (pColatA.valuation.AmtSide == pColatB.valuation.AmtSide);
                    }
                    equal = equal && (pColatA.valuation.otcmlId == pColatB.valuation.otcmlId);
                    equal = equal && (pColatA.valuation.QtySpecified == pColatB.valuation.QtySpecified);
                    if (equal && pColatA.valuation.QtySpecified && pColatB.valuation.QtySpecified)
                    {
                        equal = (pColatA.valuation.Qty == pColatB.valuation.Qty);
                    }
                }
                // Haircut
                equal = equal && ((pColatA.haircut == pColatB.haircut) || ((pColatA.haircut != default(CssValue[])) && (pColatB.haircut != default(CssValue[]))));
                if (equal && (pColatA.haircut != default(CssValue[])) && (pColatB.haircut != default(CssValue[])))
                {
                    equal = (pColatA.haircut.Count() == pColatB.haircut.Count());
                    if (equal)
                    {
                        foreach (CssValue detailA in pColatA.haircut)
                        {
                            // PM 20201202 [25592] Correction teste existance haircut suite recette v10
                            //CssValue detailB = pColatB.haircut.FirstOrDefault(b => (b.cssHrefSpecified == detailA.cssHrefSpecified)
                            //    && b.cssHrefSpecified
                            //    && (b.cssHref == detailA.cssHref)
                            //    && (b.value == detailA.value));
                            CssValue detailB = pColatB.haircut.FirstOrDefault(b => (b.cssHrefSpecified == detailA.cssHrefSpecified)
                                && (
                                    (detailA.cssHrefSpecified && (b.cssHref == detailA.cssHref)) || (false == detailA.cssHrefSpecified)
                                   )
                                && (b.Value == detailA.Value));
                            equal = equal && (detailB != default(CssValue));
                            if (!equal)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un PosCollateral
        /// </summary>
        /// <param name="pColat"></param>
        /// <returns></returns>
        public override int GetHashCode(PosCollateral pColat)
        {
            decimal hCode = 0;
            if ((pColat != default(PosCollateral)) && (pColat.valuation != default(PosCollateralValuation)))
            {
                hCode = pColat.valuation.Amt;
            }
            return hCode.GetHashCode();
        }

        /// <summary>
        /// Indique si deux tableaux de PosCollateral sont équivalents
        /// </summary>
        /// <param name="pColatA"></param>
        /// <param name="pColatB"></param>
        /// <returns></returns>
        public bool ArrayEquals(PosCollateral[] pColatA, PosCollateral[] pColatB)
        {
            bool equal = (pColatA == pColatB);
            if (!equal && (pColatA != default(PosCollateral[])) && (pColatB != default(PosCollateral[])))
            {
                equal = (pColatA.Count() == pColatB.Count());
                if (equal)
                {
                    foreach (PosCollateral detailA in pColatA)
                    {
                        PosCollateral detailB = pColatB.FirstOrDefault(b => (b.otcmlId == detailA.otcmlId));
                        equal = equal && Equals(detailA, detailB);
                        if (!equal)
                        {
                            break;
                        }
                    }
                }
            }
            return equal;
        }
    }

    /// <summary>
    /// EqualityComparer pour PreviousMarginConstituent
    /// </summary>
    public class PreviousMarginConstituentComparer : EqualityComparer<PreviousMarginConstituent>
    {
        /// <summary>
        /// Indique si deux PreviousMarginConstituent sont équivalents
        /// </summary>
        /// <param name="pMrgA"></param>
        /// <param name="pMrgB"></param>
        /// <returns></returns>
        public override bool Equals(PreviousMarginConstituent pMrgA, PreviousMarginConstituent pMrgB)
        {
            bool equal = (pMrgA == pMrgB);
            if (!equal && (pMrgA != default(PreviousMarginConstituent)) && (pMrgB != default(PreviousMarginConstituent)))
            {
                // Margin Requirement
                equal = CashBalanceComparer.CashPositionComparer.Equals(pMrgA.marginRequirement, pMrgB.marginRequirement);
                //
                // Cash Available
                equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pMrgA.cashAvailable, pMrgB.cashAvailable);
                //
                // Cash Used
                equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pMrgA.cashUsed, pMrgB.cashUsed);
                //
                // Collateral Available
                equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pMrgA.collateralAvailable, pMrgB.collateralAvailable);
                //
                // Collateral Used
                equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pMrgA.collateralUsed, pMrgB.collateralUsed);
                //
                // Uncovered Margin Requirement
                equal = equal && CashBalanceComparer.CashPositionComparer.Equals(pMrgA.uncoveredMarginRequirement, pMrgB.uncoveredMarginRequirement);
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un PreviousMarginConstituent
        /// </summary>
        /// <param name="pMrg"></param>
        /// <returns></returns>
        public override int GetHashCode(PreviousMarginConstituent pMrg)
        {
            int hCode = 0;
            if (pMrg != default(PreviousMarginConstituent))
            {
                hCode = CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.marginRequirement);
                hCode ^= CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.cashAvailable);
                hCode ^= CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.cashUsed);
                hCode ^= CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.collateralAvailable);
                hCode ^= CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.collateralUsed);
                hCode ^= CashBalanceComparer.CashPositionComparer.GetHashCode(pMrg.uncoveredMarginRequirement);
            }
            return hCode.GetHashCode();
        }
    }

    /// <summary>
    /// EqualityComparer pour SimplePayment
    /// </summary>
    public class SimplePaymentComparer : EqualityComparer<SimplePayment>
    {
        /// <summary>
        /// Indique si deux SimplePayment sont équivalents
        /// </summary>
        /// <param name="pPayA"></param>
        /// <param name="pPayB"></param>
        /// <returns></returns>
        public override bool Equals(SimplePayment pPayA, SimplePayment pPayB)
        {
            bool equal = (pPayA == pPayB);
            if (!equal && (pPayA != default(SimplePayment)) && (pPayB != default(SimplePayment)))
            {
                equal = (pPayA.payerPartyReference.href == pPayB.payerPartyReference.href);
                equal = equal && (pPayA.receiverPartyReference.href == pPayB.receiverPartyReference.href);
                if ((pPayA.paymentAmount != default) && (pPayB.paymentAmount != default))
                {
                    equal = equal && (pPayA.paymentAmount.amount.DecValue == pPayB.paymentAmount.amount.DecValue);
                }
                else
                {
                    equal = equal && (pPayA.paymentAmount == pPayB.paymentAmount);
                }
                if (equal && (pPayA.paymentDate != default(AdjustableOrRelativeAndAdjustedDate)) && (pPayB.paymentDate != default(AdjustableOrRelativeAndAdjustedDate)))
                {
                    equal = (pPayA.paymentDate.adjustedDateSpecified == pPayB.paymentDate.adjustedDateSpecified);
                    if (equal && pPayA.paymentDate.adjustedDateSpecified && pPayB.paymentDate.adjustedDateSpecified)
                    {
                        equal = (pPayA.paymentDate.adjustedDate.Value == pPayB.paymentDate.adjustedDate.Value);
                    }
                    equal = equal && (pPayA.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified == pPayB.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified);
                    if (equal && pPayA.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified && pPayB.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified)
                    {
                        AdjustableDate adjustableDateA = pPayA.paymentDate.adjustableOrRelativeDateAdjustableDate;
                        AdjustableDate adjustableDateB = pPayB.paymentDate.adjustableOrRelativeDateAdjustableDate;
                        equal = (adjustableDateA.Id == adjustableDateB.Id);
                        equal = equal && (adjustableDateA.unadjustedDate.Value == adjustableDateB.unadjustedDate.Value);
                        equal = equal && (adjustableDateA.dateAdjustments.Id == adjustableDateB.dateAdjustments.Id);
                        equal = equal && (adjustableDateA.dateAdjustments.businessDayConvention == adjustableDateB.dateAdjustments.businessDayConvention);
                        //
                        equal = equal && (adjustableDateA.dateAdjustments.businessCentersDefineSpecified == adjustableDateB.dateAdjustments.businessCentersDefineSpecified);
                        if (equal && adjustableDateA.dateAdjustments.businessCentersDefineSpecified && adjustableDateB.dateAdjustments.businessCentersDefineSpecified)
                        {
                            equal = CashBalanceComparer.BusinessCentersDefineEquals(adjustableDateA.dateAdjustments.businessCentersDefine, adjustableDateB.dateAdjustments.businessCentersDefine);
                        }
                    }
                    equal = equal && (pPayA.paymentDate.adjustableOrRelativeDateRelativeDateSpecified == pPayB.paymentDate.adjustableOrRelativeDateRelativeDateSpecified);
                    if (equal && pPayA.paymentDate.adjustableOrRelativeDateRelativeDateSpecified && pPayB.paymentDate.adjustableOrRelativeDateRelativeDateSpecified)
                    {
                        equal = (pPayA.paymentDate.adjustableOrRelativeDateRelativeDate.Id == pPayB.paymentDate.adjustableOrRelativeDateRelativeDate.Id);
                        equal = equal && (pPayA.paymentDate.adjustableOrRelativeDateRelativeDate.DateRelativeToValue == pPayB.paymentDate.adjustableOrRelativeDateRelativeDate.DateRelativeToValue);
                    }
                }
                else
                {
                    equal = equal && (pPayA.paymentDate == pPayB.paymentDate);
                }
            }
            return equal;
        }

        /// <summary>
        /// HashCode d'un SimplePayment
        /// </summary>
        /// <param name="pPay"></param>
        /// <returns></returns>
        public override int GetHashCode(SimplePayment pPay)
        {
            decimal hCode = 0;
            if ((pPay != default(SimplePayment)) && (pPay.paymentAmount != default))
            {
                hCode = pPay.paymentAmount.amount.DecValue;
            }
            return hCode.GetHashCode();
        }
    }
    #endregion Object of Stream Class Comparer
}

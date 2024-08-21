using System;
using System.Collections.Generic;
using System.Linq;
//
using EFS.ACommon;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.Properties;
//
using EfsML.Business;
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
//
using FpML.Enum;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheet repository, containing the results and the calculation details of a deposit
    /// </summary>
    /// <remarks>this part of the class includes the members building the method calculation details for the "TIMS EUREX" method</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// generic signature for a method returning a calculation sheet component
        /// </summary>
        /// <param name="pComObj">communication object related to the returned calculation sheet component </param>
        /// <returns>a calculation sheet component</returns>
        private delegate TimsDecomposableParameter ParameterBuilder(
            TimsDecomposableParameterCommunicationObject pComObj);

        /// <summary>
        /// Build the calculation details for a deposit evaluated with the TIMS EUREX method
        /// </summary>
        /// <param name="pMethodComObj">commuincation object root returned from the method</param>
        /// <returns>the calculation sheet object ready to be serialized</returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildTimsEurexMarginCalculationMethod(
            TimsEurexMarginCalculationMethodCommunicationObject pMethodComObj)
        {
            TimsEurexMarginCalculationMethod method = new TimsEurexMarginCalculationMethod
            {
                Name = "TIMS EUREX"
            };

            // PM 20131212 [19332] Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(TimsEurexMarginCalculationMethodCommunicationObject))
            {
                //PM 20150511 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (pMethodComObj.Parameters != default(TimsEurexGroupParameterCommunicationObject[]))
                {
                    //
                    method.ExchRates = BuildExchRateReport(
                        pMethodComObj.ExchRates, pMethodComObj.CssCurrency);

                    method.NotCrossedMarginAmounts = pMethodComObj.NotCrossedMarginAmounts?.Cast<Money>().ToArray();

                    method.Cross = BuildCrossMarginReport(pMethodComObj.Cross);

                    method.Parameters = new TimsEurexGroupParameter[pMethodComObj.Parameters.Length];

                    int idxGroup = 0;

                    foreach (TimsEurexGroupParameterCommunicationObject groupComObj in pMethodComObj.Parameters)
                    {
                        TimsEurexGroupParameter group = new TimsEurexGroupParameter();
                        method.Parameters[idxGroup++] = group;

                        group.Name = groupComObj.Group;

                        if (groupComObj.Missing || Settings.Default.TimsPositionExtendedReport)
                        {
                            group.positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, groupComObj.Positions, null);
                        }

                        if (!groupComObj.Missing)
                        {
                            bool isMissingContractParameters = false;
                            
                            //string firstMissingContractErrorCode = null;
                            SysMsgCode firstMissingContractErrorCode = null;

                            group.OutOfTheMoneyMinValue = groupComObj.OutOfTheMoneyMinValue;
                            group.Offset = groupComObj.Offset;

                            group.Premiums = BuildMultipleMarginsReport(groupComObj.Premiums, this.BuildPremiumMarginReport);

                            group.Spreads = BuildMultipleMarginsReport(groupComObj.Spreads, this.BuildSpreadMarginReport);

                            group.Additionals = BuildMultipleMarginsReport(groupComObj.Additionals, this.BuildAdditionalMarginReport);

                            // groupComObj.MarginAmounts may not be null or empty... in case of exceptions verify the risk method implementation 
                            group.marginAmounts = (from amount in groupComObj.MarginAmounts select MarginMoney.FromMoney(amount)).ToArray();

                            group.Parameters = new TimsEurexClassParameter[groupComObj.Parameters.Length];
                            int idxClass = 0;

                            foreach (TimsEurexClassParameterCommunicationObject classComObj in groupComObj.Parameters)
                            {
                                TimsEurexClassParameter classParam = new TimsEurexClassParameter();
                                group.Parameters[idxClass++] = classParam;

                                classParam.Name = classComObj.Class;
                                classParam.Offset = classComObj.Offset;
                                classParam.MaturityFactor = classComObj.MaturityFactor;
                                classParam.SpotMonthSpreadRate = classComObj.SpotMonthSpreadRate;
                                classParam.BackMonthSpreadRate = classComObj.BackMonthSpreadRate;

                                classParam.Symbols =
                                    (from symbol in classComObj.ContractSymbols select symbol)
                                    .Aggregate((total, next) => String.Format("{0}.{1}", total, next));

                                classParam.Premium = BuildPremiumMarginReport(classComObj.Premium);

                                classParam.Spread = BuildSpreadMarginReport(classComObj.Spread);

                                classParam.Additional = BuildAdditionalMarginReport(classComObj.Additional);

                                classParam.Liquidating = BuildLiquidatingReport(classComObj.Liquidating);

                                classParam.marginAmount = MarginMoney.FromMoney(classComObj.MarginAmount);

                                if (Settings.Default.TimsPositionExtendedReport)
                                {
                                    classParam.positions =
                                        BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, classComObj.Positions, null);
                                }

                                classParam.Parameters = new TimsEurexContractParameter[classComObj.Parameters.Length];
                                int idxContract = 0;

                                foreach (TimsEurexContractParameterCommunicationObject contractComObj in classComObj.Parameters)
                                {
                                    TimsEurexContractParameter contractParam = new TimsEurexContractParameter();
                                    classParam.Parameters[idxContract++] = contractParam;

                                    contractParam.Name = contractComObj.Contract;

                                    contractParam.Premium = BuildPremiumMarginReport(contractComObj.Premium,
                                        Settings.Default.EurexPremiumFactorsReport);

                                    contractParam.Additional = BuildAdditionalMarginReport(contractComObj.Additional,
                                        Settings.Default.EurexAdditionalContractFactorsReport);

                                    contractParam.positions =
                                        BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false,
                                        contractComObj.Positions, contractComObj.StocksCoverage, contractComObj.CompensatedShortOptionQuantities);

                                    // PM 20130628 Ajout information s'il manque des données de calcul pour ce contrat
                                    contractParam.InformationSpecified = contractComObj.Missing;
                                    if (contractComObj.Missing)
                                    {
                                        isMissingContractParameters = true;
                                        if (firstMissingContractErrorCode == null)
                                        {
                                            firstMissingContractErrorCode = contractComObj.ErrorCode;
                                        }
                                        contractParam.Information = "Parameters not found";
                                    }

                                }
                            }
                            // PM 20130628 Ajout de l'indication dans le log comme quoi il y a des erreurs sur les paramètres des contrats
                            if ((isMissingContractParameters) && (firstMissingContractErrorCode != default(SysMsgCode)))
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, firstMissingContractErrorCode, 0,
                                    new LogParam(method.Name),
                                    new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                            }
                        }
                        else
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Warning, groupComObj.ErrorCode, 0,
                                new LogParam(method.Name),
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                        }

                    }
                }
            }
            return method;
        }

        private TimsDecomposableParameter BuildLiquidatingReport(
            TimsDecomposableParameterCommunicationObject pLiquidatingMarginParameterCommunicationObject)
        {
            if (pLiquidatingMarginParameterCommunicationObject == null)
            {
                return null;
            }

            TimsDecomposableParameter liquidating = new TimsDecomposableParameter
            {
                MarginAmount = MarginMoney.FromMoney(pLiquidatingMarginParameterCommunicationObject.MarginAmount)
            };

            return liquidating;
        }

        private TimsDecomposableParameter[] BuildCrossMarginReport(
            IEnumerable<TimsDecomposableParameterCommunicationObject> pCrossMarginParameterCommunicationObject)
        {
            TimsDecomposableParameter[] cross = null;

            if (pCrossMarginParameterCommunicationObject != null)
            {
                cross = (
                    from crossAmount in pCrossMarginParameterCommunicationObject
                    select new TimsDecomposableParameter
                    {
                        MarginAmount = MarginMoney.FromMoney(crossAmount.MarginAmount),

                        Factors = crossAmount.Factors == null ? null : (
                        from factorComObj in crossAmount.Factors
                        select new TimsFactor
                        {
                            Id = factorComObj.Identifier,
                            MarginAmount = MarginMoney.FromMoney(factorComObj.MarginAmount),

                        }).ToArray(),
                    }).ToArray();
            }
            return cross;
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginFxRatePair[] BuildExchRateReport(IEnumerable<TimsEurexExchRateCommunicationObject> pExchRates, string pCssCurrency)
        {
            if (String.IsNullOrEmpty(pCssCurrency))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1011), 0, new LogParam(this.GetSQLActorFromCache(m_Cs, this.ProcessInfo.CssId).Identifier)));
            }

            if (pExchRates == null)
            {
                return null;
            }

            var rates = (
                from exchRate in pExchRates
                select new
                {
                    Rate = new MarginFxRatePair
                    {
                        Missing = exchRate.Missing,
                        MissingSpecified = exchRate.Missing,

                        Credit = new FxRate
                        {
                            rate = !exchRate.Missing ? new EFS_Decimal(exchRate.RateCredit) : null,

                            quotedCurrencyPair = new QuotedCurrencyPair(
                                exchRate.CurrencyFrom, exchRate.CurrencyTo, QuoteBasisEnum.Currency1PerCurrency2),

                        },

                        Debit = new FxRate
                        {
                            rate = !exchRate.Missing ? new EFS_Decimal(exchRate.RateDebit) : null,

                            quotedCurrencyPair = new QuotedCurrencyPair(
                                exchRate.CurrencyFrom, exchRate.CurrencyTo, QuoteBasisEnum.Currency1PerCurrency2),
                        },
                    },

                    exchRate.ErrorCode,

                    exchRate.SystemMsgInfo,
                }
            );


            foreach (var rate in (from rate in rates where rate.Rate.Missing select rate))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);


                Logger.Log(new LoggerData(LogLevelEnum.Warning, rate.ErrorCode, 0,
                    new LogParam(rate.Rate.Credit.quotedCurrencyPair.currency1.Value),
                    new LogParam(rate.Rate.Credit.quotedCurrencyPair.currency2.Value),
                    new LogParam(String.Format("{0}/{1}", QuotationSideEnum.Ask, QuotationSideEnum.Bid)),
                    new LogParam(this.ProcessInfo.DtBusiness.ToShortDateString())));

                if (null != rate.SystemMsgInfo)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Warning, rate.SystemMsgInfo.SysMsgCode, 0, rate.SystemMsgInfo.LogParamDatas));
                }

            }

            return (from elem in rates select elem.Rate).ToArray();
        }


        private TimsDecomposableParameter[] BuildMultipleMarginsReport(
            IEnumerable<TimsDecomposableParameterCommunicationObject> pComObjs, ParameterBuilder methodBuilder)
        {
            TimsDecomposableParameter[] elementsToSerialize = null;

            if (pComObjs != null)
            {

                elementsToSerialize = (
                    from elem in pComObjs
                    where elem != null
                    select methodBuilder.Invoke(elem)
                ).ToArray();
            }

            return elementsToSerialize;
        }

    }
}
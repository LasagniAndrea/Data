using System;
using System.Linq;
//
using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.CommunicationObjects;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheet repository, containing the results and the calculation details of a deposit
    /// </summary>
    /// <remarks>This part of the class includes the members building the method calculation details for the SPAN method</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildSpanMarginCalculationMethod(
            SpanMarginCalcMethCom pMethodComObj)
        {
            SpanMarginCalculationMethod method = new SpanMarginCalculationMethod();

            if (pMethodComObj != default(SpanMarginCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;
                //PM 20150511 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                if (pMethodComObj.MarginMethodType != InitialMarginMethodEnum.London_SPAN)
                {
                    method.AccountType = pMethodComObj.AccountType.ToString();
                    method.MaintenanceInitial = pMethodComObj.IsMaintenanceAmount ? "MNT" : "INIT";
                    method.AccountTypeSpecified = true;
                    method.MaintenanceInitialSpecified = true;
                }
                else
                {
                    method.AccountTypeSpecified = false;
                    method.MaintenanceInitialSpecified = false;
                }

                // PM 20130814 [18883] Ajout log explicite en cas de paramètres manquants sur la clearing house
                if (pMethodComObj.Missing)
                {
                    IScheme tradeScheme = Tools.GetScheme(m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme);
                    string tradeInfo = tradeScheme != default(IScheme) ? tradeScheme.Value : "";

                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, pMethodComObj.ErrorCode, 0,
                        new LogParam(this.GetSQLActorFromCache(m_Cs, this.ProcessInfo.CssId).Identifier),
                        new LogParam(Convert.ToString(this.m_ProcessInfo.CssId)),
                        new LogParam(method.Name),
                        new LogParam(tradeInfo)));
                }

                // PM 20130814 Vérification qu'il y a bien eu des données de calcul sur les marchés affectés à la clearing house
                if (null != pMethodComObj.Parameters)
                {
                    SpanExchangeComplexCom[] exchangeComplexCom = (SpanExchangeComplexCom[])pMethodComObj.Parameters;

                    // PM 20150902 [21385] Add SettlementSession, DtBusinessTime, DtFile, FileIdentifier & FileFormat
                    // PM 20150930 [21134] Add OneFactorCredit
                    // PM 20190401 [24625][24387] Utilisation d'une méthode
                    //SpanExchangeComplexParameter[] exchangeComplexParameters = (
                    //    from exchangeComplex in exchangeComplexCom
                    //    select new SpanExchangeComplexParameter
                    //    {
                    //        Name = exchangeComplex.ExchangeComplex,
                    //        IsOptionValueLimit = exchangeComplex.IsOptionValueLimit,
                    //        SettlementSession = exchangeComplex.SettlementSession,
                    //        DtBusinessTime = exchangeComplex.DtBusinessTime,
                    //        DtFile = exchangeComplex.DtFile,
                    //        FileIdentifier = exchangeComplex.FileIdentifier,
                    //        FileFormat = exchangeComplex.FileFormat,
                    //        SuperInterCommoditySpread = BuildSpanInterCommoditySpreadParameter((SpanInterCommoditySpreadCom[])exchangeComplex.SuperInterCommoditySpread),
                    //        InterCommoditySpread = BuildSpanInterCommoditySpreadParameter((SpanInterCommoditySpreadCom[])exchangeComplex.InterCommoditySpread),
                    //        InterExchangeSpread = BuildSpanInterCommoditySpreadParameter((SpanInterCommoditySpreadCom[])exchangeComplex.InterExchangeSpread),
                    //        OneFactorCredit = BuildSpanOneFactorCreditParameter((SpanOneFactorCreditCom)exchangeComplex.OneFactorCredit),
                    //        OneFactorCreditSpecified = ((SpanOneFactorCreditCom)exchangeComplex.OneFactorCredit != default),
                    //        parameters = BuildSpanCombinedCommodityGroupParameter(pMethodComObj.MarginMethodType, (SpanCombinedGroupCom[])exchangeComplex.Parameters),
                    //    }).ToArray();

                    //method.parameters = exchangeComplexParameters;
                    method.Parameters = BuildSpanExchangeComplexParameter(pMethodComObj.MarginMethodType, exchangeComplexCom);

                    foreach (SpanExchangeComplexCom exchangeComplex in exchangeComplexCom)
                    {
                        if (exchangeComplex.Missing)
                        {
                            IScheme tradeScheme = Tools.GetScheme(m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme);
                            string tradeInfo = tradeScheme != default(IScheme) ? tradeScheme.Value : "";

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, exchangeComplex.ErrorCode, 0,
                                new LogParam(method.Name),
                                new LogParam(tradeInfo)));
                        }
                    }

                    // PM 20190401 [24625][24387] Ajout AdditionalMarginBoM
                    if ((pMethodComObj.AdditionalMarginBoM != default(SpanAdditionalMarginBoMCom))
                        && (pMethodComObj.AdditionalMarginBoM.Parameters != null))
                    {
                        exchangeComplexCom = (SpanExchangeComplexCom[])pMethodComObj.AdditionalMarginBoM.Parameters;
                        //
                        method.AdditionalMarginBoM = BuildSpanExchangeComplexParameter(pMethodComObj.MarginMethodType, exchangeComplexCom);
                    }

                    // PM 20190801 [24717] Ajout du Concentration Risk Margin de l'ECC
                    method.IsCalcECCConcentrationRiskMargin = pMethodComObj.IsCalcECCConcentrationRiskMargin;
                    if (pMethodComObj.IsCalcECCConcentrationRiskMargin)
                    {
                        method.ConcentrationRiskMargin = BuildECCConcentrationRiskMarginParameter(pMethodComObj);
                        method.ConcentrationRiskMarginSpecified = (method.ConcentrationRiskMargin != default(ECCConcentrationRiskMarginParameter));
                    }
                }
            }
            return method;
        }

        /// <summary>
        /// Construction des données du détails pour les exchanges complex
        /// </summary>
        /// <param name="pMarginMethodType"></param>
        /// <param name="pExchangeComplexCom"></param>
        /// <returns></returns>
        // PM 20190401 [24625][24387] New
        /// <summary>
        private SpanExchangeComplexParameter[] BuildSpanExchangeComplexParameter(InitialMarginMethodEnum pMarginMethodType, SpanExchangeComplexCom[] pExchangeComplexCom)
        {
            SpanExchangeComplexParameter[] exchangeComplexParameter = null;
            if (pExchangeComplexCom != default(SpanExchangeComplexCom[]))
            {
                exchangeComplexParameter = (
                    from exchangeComplex in pExchangeComplexCom
                    select new SpanExchangeComplexParameter
                    {
                        Name = exchangeComplex.ExchangeComplex,
                        IsOptionValueLimit = exchangeComplex.IsOptionValueLimit,
                        SettlementSession = exchangeComplex.SettlementSession,
                        DtBusinessTime = exchangeComplex.DtBusinessTime,
                        DtFile = exchangeComplex.DtFile,
                        FileIdentifier = exchangeComplex.FileIdentifier,
                        FileFormat = exchangeComplex.FileFormat,
                        SuperInterCommoditySpread = BuildSpanInterCommoditySpreadParameter((SpanInterCommoditySpreadCom[])exchangeComplex.SuperInterCommoditySpread),
                        InterCommoditySpread = BuildSpanInterCommoditySpreadParameter((SpanInterCommoditySpreadCom[])exchangeComplex.InterCommoditySpread),
                        InterExchangeSpread = BuildSpanInterCommoditySpreadParameter((SpanInterCommoditySpreadCom[])exchangeComplex.InterExchangeSpread),
                        OneFactorCredit = BuildSpanOneFactorCreditParameter((SpanOneFactorCreditCom)exchangeComplex.OneFactorCredit),
                        OneFactorCreditSpecified = ((SpanOneFactorCreditCom)exchangeComplex.OneFactorCredit != default),
                        Parameters = BuildSpanCombinedCommodityGroupParameter(pMarginMethodType, (SpanCombinedGroupCom[])exchangeComplex.Parameters),
                    }).ToArray();
            }
            return exchangeComplexParameter;
        }

        /// <summary>
        /// Construction des données du détails du calcul des spread inter commodity
        /// </summary>
        /// <param name="pInterSpreadCom"></param>
        /// <returns></returns>
        private SpanInterCommoditySpreadParameter[] BuildSpanInterCommoditySpreadParameter(SpanInterCommoditySpreadCom[] pInterSpreadCom)
        {
            SpanInterCommoditySpreadParameter[] interSpreadParameters = null;
            if (pInterSpreadCom != default(SpanInterCommoditySpreadCom[]))
            {
                interSpreadParameters =
                    ((from interSpread in pInterSpreadCom
                      where interSpread.IsOffsetChargeMethod == false
                      select new SpanInterCommoditySpreadParameter
                      {
                          SpreadPriority = interSpread.SpreadPriority,
                          InterSpreadMethod = interSpread.InterSpreadMethod,
                          IsSeparatedSpreadRate = interSpread.IsSeparatedSpreadRate,
                          SpreadRate = interSpread.SpreadRate,
                          SpreadRateSpecified = !interSpread.IsSeparatedSpreadRate,
                          NumberOfSpreadLimit = interSpread.NumberOfSpreadLimit,
                          NumberOfSpreadLimitSpecified = (interSpread.InterSpreadMethod != "04") && (interSpread.InterSpreadMethod != "S"),
                          NumberOfSpread = interSpread.NumberOfSpread,
                          NumberOfSpreadSpecified = (interSpread.InterSpreadMethod != "04") && (interSpread.InterSpreadMethod != "S"),
                          OffsetChargeSpecified = false,
                          PortfolioScanRiskSpecified = false,
                          PortfolioRiskSpecified = false,
                          SpreadScanRisk = interSpread.SpreadScanRisk,
                          SpreadScanRiskSpecified = (interSpread.InterSpreadMethod == "04") || (interSpread.InterSpreadMethod == "S"),
                          DeltaAvailable = interSpread.DeltaAvailable,
                          DeltaAvailableSpecified = (interSpread.InterSpreadMethod == "04") || (interSpread.InterSpreadMethod == "S"),
                          LegParameters = BuildSpanInterCommoditySpreadLegParameter(interSpread.LegParameters),
                      }).Concat(
                        from interSpread in pInterSpreadCom
                        where interSpread.IsOffsetChargeMethod == true
                        select new SpanInterCommoditySpreadParameter
                        {
                            SpreadPriority = interSpread.SpreadPriority,
                            InterSpreadMethod = interSpread.InterSpreadMethod,
                            IsSeparatedSpreadRate = interSpread.IsSeparatedSpreadRate,
                            SpreadRate = interSpread.SpreadRate,
                            SpreadRateSpecified = !interSpread.IsSeparatedSpreadRate,
                            NumberOfSpreadLimit = interSpread.NumberOfSpreadLimit,
                            NumberOfSpread = interSpread.NumberOfSpread,
                            OffsetCharge = interSpread.OffsetCharge,
                            OffsetChargeSpecified = true,
                            PortfolioScanRisk = interSpread.PortfolioScanRisk,
                            PortfolioScanRiskSpecified = true,
                            PortfolioRisk = interSpread.PortfolioRisk,
                            PortfolioRiskSpecified = true,
                            SpreadScanRisk = interSpread.SpreadScanRisk,
                            SpreadScanRiskSpecified = true,
                            LegParameters = BuildSpanInterCommoditySpreadLegParameter(interSpread.LegParameters),
                        })
                     ).ToArray();
            }
            return interSpreadParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des jambes des spread inter commodity
        /// </summary>
        /// <param name="pInterSpreadLegCom"></param>
        /// <returns></returns>
        private SpanInterCommoditySpreadLegParameter[] BuildSpanInterCommoditySpreadLegParameter(SpanInterCommoditySpreadLegCom[] pInterSpreadLegCom)
        {
            SpanInterCommoditySpreadLegParameter[] interSpreadLegParameters = null;
            if (pInterSpreadLegCom != default(SpanInterCommoditySpreadLegCom[]))
            {
                interSpreadLegParameters =
                    (from interSpreadLeg in pInterSpreadLegCom
                     select new SpanInterCommoditySpreadLegParameter
                     {
                         ExchangeAcronym = interSpreadLeg.ExchangeAcronym,
                         CombinedCommodityCode = interSpreadLeg.CombinedCommodityCode,
                         SpreadRate = interSpreadLeg.SpreadRate,
                         TierNumber = interSpreadLeg.TierNumber,
                         Maturity = interSpreadLeg.Maturity,
                         DeltaPerSpread = interSpreadLeg.DeltaPerSpread,
                         DeltaAvailable = interSpreadLeg.DeltaAvailable,
                         DeltaRemaining = interSpreadLeg.DeltaRemaining,
                         ComputedDeltaConsumed = interSpreadLeg.ComputedDeltaConsumed,
                         RealyDeltaConsumed = interSpreadLeg.RealyDeltaConsumed,
                         WeightedRisk = interSpreadLeg.WeightedRisk,
                         SpreadCredit = interSpreadLeg.SpreadCredit,
                         IsRequired = interSpreadLeg.IsRequired,
                         IsTarget = interSpreadLeg.IsTarget,
                     }).ToArray();
            }
            return interSpreadLegParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des groupes combinés
        /// </summary>
        /// <param name="pMarginMethod"></param>
        /// <param name="pCombinedGroupCom"></param>
        /// <returns></returns>
        private SpanCombinedCommodityGroupParameter[] BuildSpanCombinedCommodityGroupParameter(InitialMarginMethodEnum pMarginMethod, SpanCombinedGroupCom[] pCombinedGroupCom)
        {
            SpanCombinedCommodityGroupParameter[] compinedGroupParameters = null;
            if (pCombinedGroupCom != default(SpanCombinedGroupCom[]))
            {
                compinedGroupParameters =
                    (from combinedGroup in pCombinedGroupCom
                     select new SpanCombinedCommodityGroupParameter
                     {
                         Name = combinedGroup.CombinedGroup,
                         LongOptionValue = MarginMoney.FromMoney(combinedGroup.LongOptionValue),
                         ShortOptionValue = MarginMoney.FromMoney(combinedGroup.ShortOptionValue),
                         NetOptionValue = MarginMoney.FromMoney(combinedGroup.NetOptionValue),
                         RiskInitial = MarginMoney.FromMoney(combinedGroup.RiskInitialAmount),
                         RiskMaintenance = MarginMoney.FromMoney(combinedGroup.RiskMaintenanceAmount),
                         RiskMaintenanceSpecified = (pMarginMethod != InitialMarginMethodEnum.London_SPAN),
                         Parameters = BuildSpanCombinedCommodityParameter(pMarginMethod, (SpanContractGroupCom[])combinedGroup.Parameters),
                     }).ToArray();
            }
            return compinedGroupParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des groupes de contrats
        /// </summary>
        /// <param name="pMarginMethod"></param>
        /// <param name="pContractGroupCom"></param>
        /// <returns></returns>
        private SpanCombinedCommodityParameter[] BuildSpanCombinedCommodityParameter(InitialMarginMethodEnum pMarginMethod, SpanContractGroupCom[] pContractGroupCom)
        {
            SpanCombinedCommodityParameter[] contractGroupParameters = null;
            if (pContractGroupCom != default(SpanContractGroupCom[]))
            {
                bool isLondonSPAN = (pMarginMethod == InitialMarginMethodEnum.London_SPAN);

                contractGroupParameters =
                    (from contractGroup in pContractGroupCom
                     select new SpanCombinedCommodityParameter
                     {
                         Name = contractGroup.ContractGroup,
                         StrategySpreadMethod = contractGroup.StrategySpreadChargeMethod,
                         StrategySpreadMethodSpecified = isLondonSPAN,
                         IntraSpreadMethod = contractGroup.InterMonthSpreadChargeMethod,
                         DeliveryMonthMethod = contractGroup.DeliveryMonthChargeMethod,
                         WeightedRiskMethod = contractGroup.WeightedRiskMethod.ToString(),
                         // PM 20150930 [21134] Ajout IsUseLambda, LambdaMin, LambdaMax
                         IsUseLambda = contractGroup.IsUseLambda,
                         LambdaMin = contractGroup.LambdaMin,
                         LambdaMax = contractGroup.LambdaMax,
                         //
                         LongOptionValue = MarginMoney.FromMoney(contractGroup.LongOptionValue),
                         ShortOptionValue = MarginMoney.FromMoney(contractGroup.ShortOptionValue),
                         NetOptionValue = MarginMoney.FromMoney(contractGroup.NetOptionValue),
                         ShortOptionMinimum = MarginMoney.FromMoney(contractGroup.ShortOptionMinimum),
                         DeltaNet = contractGroup.DeltaNet,
                         DeltaNetRemaining = contractGroup.DeltaNetRemaining,
                         //
                         ActiveScenario = contractGroup.ActiveScenario,
                         LongScanRisk = MarginMoney.FromMoney(contractGroup.LongScanRiskAmount),
                         ShortScanRisk = MarginMoney.FromMoney(contractGroup.ShortScanRiskAmount),
                         ScanRisk = MarginMoney.FromMoney(contractGroup.ScanRiskAmount),
                         PriceRisk = MarginMoney.FromMoney(contractGroup.PriceRiskAmount),
                         TimeRisk = MarginMoney.FromMoney(contractGroup.TimeRiskAmount),
                         VolatilityRisk = MarginMoney.FromMoney(contractGroup.VolatilityRiskAmount),
                         NormalWeightedRisk = MarginMoney.FromMoney(contractGroup.NormalWeightedRiskAmount),
                         CappedWeightedRisk = MarginMoney.FromMoney(contractGroup.CappedWeightedRiskAmount),
                         WeightedRisk = MarginMoney.FromMoney(contractGroup.WeightedRiskAmount),
                         StrategySpreadCharge = MarginMoney.FromMoney(contractGroup.StrategySpreadChargeAmount),
                         StrategySpreadChargeSpecified = isLondonSPAN,
                         IntraCommoditySpreadCharge = MarginMoney.FromMoney(contractGroup.IntraSpreadChargeAmount),
                         DeliveryMonthCharge = MarginMoney.FromMoney(contractGroup.DeliveryMonthChargeAmount),
                         InterCommodityCredit = MarginMoney.FromMoney(contractGroup.InterCommodityCreditAmount),
                         IntexCommodityCredit = MarginMoney.FromMoney(contractGroup.InterExchangeCreditAmount),
                         RiskInitial = MarginMoney.FromMoney(contractGroup.RiskInitialAmount),
                         RiskMaintenance = MarginMoney.FromMoney(contractGroup.RiskMaintenanceAmount),
                         RiskMaintenanceSpecified = !isLondonSPAN,

                         ScanRiskValueLong = contractGroup.RiskValueLong == null ? null :
                                        (from value in contractGroup.RiskValueLong
                                         select new ScenarioRiskValue
                                         {
                                             Scenario = value.Key,
                                             RiskValue = value.Value,
                                         }).ToArray(),
                         ScanRiskValueShort = contractGroup.RiskValueShort == null ? null :
                                        (from value in contractGroup.RiskValueShort
                                         select new ScenarioRiskValue
                                         {
                                             Scenario = value.Key,
                                             RiskValue = value.Value,
                                         }).ToArray(),
                         ScanRiskValue = contractGroup.RiskValue == null ? null :
                                        (from value in contractGroup.RiskValue
                                         select new ScenarioRiskValue
                                         {
                                             Scenario = value.Key,
                                             RiskValue = value.Value,
                                         }).ToArray(),

                         MaturityDelta = contractGroup.MaturityDelta == null ? null : (
                                         from delta in contractGroup.MaturityDelta
                                         select new SpanPeriodDelta
                                         {
                                             Period = delta.Period,
                                             DeltaNet = delta.DeltaNet,
                                             DeltaNetRemaining = delta.DeltaNetRemaining,
                                         }).ToArray(),
                         StrategySpreadParameter = BuildSpanIntraCommoditySpreadParameter(contractGroup.StrategyParameters),
                         IntraCommoditySpreadParameter = BuildSpanIntraCommoditySpreadParameter(contractGroup.IntraCommodityParameters),
                         DeliveryMonthChargeParameters = BuildSpanDeliveryMonthChargeParameter(pMarginMethod, contractGroup.DeliveryMonthParameters),

                         Parameters = BuildSpanContractParameter((SpanContractCom[])contractGroup.Parameters),
                     }).ToArray();
            }
            return contractGroupParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des contrats
        /// </summary>
        /// <param name="pContractCom"></param>
        /// <returns></returns>
        private SpanContractParameter[] BuildSpanContractParameter(SpanContractCom[] pContractCom)
        {
            SpanContractParameter[] contractParameters = null;
            if (pContractCom != default(SpanContractCom[]))
            {
                contractParameters =
                    (from contract in pContractCom
                     select new SpanContractParameter
                     {
                         Name = contract.Contract,
                         positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, contract.Positions, null),
                     }).ToArray();
            }
            return contractParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des spreads intra commodity
        /// </summary>
        /// <param name="pIntraSpreadCom"></param>
        /// <returns></returns>
        private SpanIntraCommoditySpreadParameter[] BuildSpanIntraCommoditySpreadParameter(SpanIntraCommoditySpreadCom[] pIntraSpreadCom)
        {
            SpanIntraCommoditySpreadParameter[] intraSpreadParameters = null;
            if (pIntraSpreadCom != default(SpanIntraCommoditySpreadCom[]))
            {
                intraSpreadParameters =
                    (from intraSpread in pIntraSpreadCom
                     select new SpanIntraCommoditySpreadParameter
                     {
                         SpreadPriority = intraSpread.SpreadPriority,
                         NumberOfLeg = intraSpread.NumberOfLeg,
                         ChargeRate = intraSpread.ChargeRate,
                         NumberOfSpread = intraSpread.NumberOfSpread,
                         SpreadCharge = intraSpread.SpreadCharge,
                         LegParameters = BuildSpanIntraCommoditySpreadLegParameter(intraSpread.SpreadLeg),
                     }).ToArray();
            }
            return intraSpreadParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des jambes des spreads intra commodity
        /// </summary>
        /// <param name="pIntraSpreadLegCom"></param>
        /// <returns></returns>
        private SpanIntraCommoditySpreadLegParameter[] BuildSpanIntraCommoditySpreadLegParameter(SpanIntraCommoditySpreadLegCom[] pIntraSpreadLegCom)
        {
            SpanIntraCommoditySpreadLegParameter[] intraSpreadLegParameters = null;
            if (pIntraSpreadLegCom != default(SpanIntraCommoditySpreadLegCom[]))
            {
                intraSpreadLegParameters =
                    (from intraSpreadLeg in pIntraSpreadLegCom
                     select new SpanIntraCommoditySpreadLegParameter
                     {
                         LegNumber = intraSpreadLeg.LegNumber,
                         LegSide = intraSpreadLeg.LegSide,
                         TierNumber = intraSpreadLeg.TierNumber,
                         TierNumberSpecified = (intraSpreadLeg.TierNumber != 0),
                         Maturity = intraSpreadLeg.Maturity,
                         MaturitySpecified = StrFunc.IsFilled(intraSpreadLeg.Maturity),
                         DeltaPerSpread = intraSpreadLeg.DeltaPerSpread,
                         AssumedLongSide = intraSpreadLeg.AssumedLongSide,
                         DeltaLongAvailable = intraSpreadLeg.DeltaLong,
                         DeltaShortAvailable = intraSpreadLeg.DeltaShort,
                         DeltaConsumed = intraSpreadLeg.DeltaConsumed,
                     }).ToArray();
            }
            return intraSpreadLegParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des charge de livraison
        /// </summary>
        /// <param name="pMarginMethod"></param>
        /// <param name="pDeliveryCom"></param>
        /// <returns></returns>
        private SpanDeliveryMonthChargeParameter[] BuildSpanDeliveryMonthChargeParameter(InitialMarginMethodEnum pMarginMethod, SpanDeliveryMonthChargeCom[] pDeliveryCom)
        {
            SpanDeliveryMonthChargeParameter[] deliveryParameters = null;
            if (pDeliveryCom != default(SpanDeliveryMonthChargeCom[]))
            {
                deliveryParameters =
                    (from delivery in pDeliveryCom
                     select new SpanDeliveryMonthChargeParameter
                     {
                         Maturity = delivery.Maturity,
                         DeltaSign = delivery.DeltaSign,
                         DeltaSignSpecified = (pMarginMethod == InitialMarginMethodEnum.London_SPAN),
                         ConsumedChargeRate = delivery.ConsumedChargeRate,
                         RemainingChargeRate = delivery.RemainingChargeRate,
                         DeltaNetUsed = delivery.DeltaNetUsed,
                         DeltaNetRemaining = delivery.DeltaNetRemaining,
                         DeliveryCharge = delivery.DeliveryCharge,
                     }).ToArray();
            }
            return deliveryParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul du One Factor Credit
        /// </summary>
        /// <param name="pOneFactorCreditCom"></param>
        /// <returns></returns>
        /// PM 20150930 [21134] New
        private SpanOneFactorCreditParameter BuildSpanOneFactorCreditParameter(SpanOneFactorCreditCom pOneFactorCreditCom)
        {
            SpanOneFactorCreditParameter oneFactorCreditParameter = default;
            if (pOneFactorCreditCom != default)
            {
                oneFactorCreditParameter = new SpanOneFactorCreditParameter
                {
                    FinalGeneralRiskLMax = pOneFactorCreditCom.FinalGeneralRiskLMax,
                    FinalGeneralRiskLMin = pOneFactorCreditCom.FinalGeneralRiskLMin,
                    IdiosyncraticRiskLMax = pOneFactorCreditCom.IdiosyncraticRiskLMax,
                    IdiosyncraticRiskLMin = pOneFactorCreditCom.IdiosyncraticRiskLMin,
                    ScanRiskOffsetLMax = pOneFactorCreditCom.ScanRiskOffsetLMax,
                    ScanRiskOffsetLMin = pOneFactorCreditCom.ScanRiskOffsetLMin,
                    ScanRiskOffset = pOneFactorCreditCom.ScanRiskOffset,
                    GlobalScanRisk = pOneFactorCreditCom.GlobalScanRisk,
                    OffsetPercentage = pOneFactorCreditCom.OffsetPercentage,
                    OffsetMax = pOneFactorCreditCom.OffsetMax
                };
            }
            return oneFactorCreditParameter;
        }

        /// <summary>
        /// Construction des données du détail du Concentration Risk Margin de l'ECC
        /// </summary>
        /// <param name="pConcentrationRiskMargin"></param>
        /// <returns></returns>
        // PM 20190801 [24717] New
        private ECCConcentrationRiskMarginParameter BuildECCConcentrationRiskMarginParameter(SpanMarginCalcMethCom pMethodComObj)
        {
            ECCConcentrationRiskMarginParameter concentrationRiskMarginParameter = default;
            if ((pMethodComObj != default(SpanMarginCalcMethCom)) && (pMethodComObj.ConcentrationRiskMargin != default(ECCConcentrationRiskMarginCom)))
            {
                concentrationRiskMarginParameter = new ECCConcentrationRiskMarginParameter
                {

                    AdditionalAddOn = pMethodComObj.ConcentrationRiskMargin.AdditionalAddOn,

                    ConcentrationRiskMarginAmounts = (
                    from conRisk in pMethodComObj.ConcentrationRiskMargin.ConcentrationRiskMarginAmounts
                    select new ECCConRiskMarginAmountParameter
                    {
                        AbsoluteCumulativePosition = conRisk.AbsoluteCumulativePosition,
                        ConcentrationRiskMargin = MarginMoney.FromMoney(conRisk.ConcentrationRiskMargin),
                        LiquidationPeriod = conRisk.LiquidationPeriod,
                        WeightedAbsCumulPosition = conRisk.WeightedAbsCumulPosition,
                        ConcentrationRiskMarginUnits = (
                            from unit in conRisk.ConcentrationRiskMarginUnits
                            select new ECCConRiskMarginUnitParameter
                            {
                                AbsoluteCumulativePosition = unit.AbsoluteCumulativePosition,
                                CombinedCommodityStress = unit.CombinedCommodityStress,
                                DailyMarketVolume = unit.DailyMarketVolume,
                                LiquidationPeriod = unit.LiquidationPeriod,
                                WeightedAbsCumulPosition = unit.WeightedAbsCumulPosition,
                            }).ToArray(),
                        ConcentrationRiskMarginUnitsSpecified = ((conRisk.ConcentrationRiskMarginUnits != default(ECCConRiskMarginUnitCom[])) && (conRisk.ConcentrationRiskMarginUnits.Count() > 0)),
                    }).ToArray(),

                    MarketVolume = (
                    from vol in pMethodComObj.ConcentrationRiskMargin.MarketVolume
                    select new ECCMarketVolumeParameter
                    {
                        CombinedCommodityStress = vol.CombinedCommodityStress,
                        MarketVolume = vol.MarketVolume,
                    }).ToArray()
                };
            }
            return concentrationRiskMarginParameter;
        }
    }
}
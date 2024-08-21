using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheetRepository, contient le resultats et le détail du calcul de déposit
    /// </summary>
    /// <remarks>Cette partie de la class inclue les membres permettant de contruire le détail du calcul par la méthode MEFFCOM2</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildMEFFCOM2MarginCalculationMethod(
            MeffCalcMethCom pMethodComObj)
        {
            MeffMarginCalculationMethod method = new MeffMarginCalculationMethod();

            // PM 20131212 [19332] Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(MeffCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;
                //PM 20150511 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (pMethodComObj.Parameters != default(MeffMarginClassCom[]))
                {
                    MeffMarginClassCom[] marginClassCom = (MeffMarginClassCom[])pMethodComObj.Parameters;
                    //
                    MeffMarginClassParameter[] marginClassParameters = (
                        from mrgClass in marginClassCom
                        select new MeffMarginClassParameter
                        {
                            MarginClassCode = mrgClass.MarginClassCode,
                            WorstCaseScenario = mrgClass.WorstCaseScenario,
                            ClassDelta = mrgClass.ClassDelta,
                            DeltaToOffset = mrgClass.DeltaToOffset,
                            PriceFluctuationType = mrgClass.PriceFluctuationType,
                            UnderlyingPrice = mrgClass.UnderlyingPrice,
                            UnderlyingPriceSpecified = (mrgClass.PriceFluctuationType == "P"),
                            AccumulatedLossAtClose = mrgClass.AccumulatedLossAtClose,
                            DeltaPotentialFutureLoss = mrgClass.DeltaPotentialFutureLoss,
                            MarginClassPotentialFutureLoss = mrgClass.MarginClassPotentialFutureLoss,
                            MaximumDeltaOffset = mrgClass.MaximumDeltaOffset,
                            NetPositionMarginAmount = MarginMoney.FromMoney(mrgClass.NetPositionMarginAmount),
                            TimeSpreadMarginAmount = MarginMoney.FromMoney(mrgClass.TimeSpreadMarginAmount),
                            CommodityMarginAmount = MarginMoney.FromMoney(mrgClass.CommodityMarginAmount),
                            InterCommodityCredit = MarginMoney.FromMoney(mrgClass.InterCommodityCreditAmount),
                            marginAmount = MarginMoney.FromMoney(mrgClass.MarginAmount),
                            NetPositionMarginValues = mrgClass.NetPositionMarginArray == null ? null :
                                                        (from value in mrgClass.NetPositionMarginArray
                                                         select new ScenarioRiskValue
                                                         {
                                                             Scenario = value.Key,
                                                             RiskValue = value.Value,
                                                         }).ToArray(),
                            TimeSpreadMarginValues = mrgClass.TimeSpreadMarginArray == null ? null :
                                                        (from value in mrgClass.TimeSpreadMarginArray
                                                         select new ScenarioRiskValue
                                                         {
                                                             Scenario = value.Key,
                                                             RiskValue = value.Value,
                                                         }).ToArray(),
                            CommodityMarginValues = mrgClass.CommodityMarginArray == null ? null :
                                                        (from value in mrgClass.CommodityMarginArray
                                                         select new ScenarioRiskValue
                                                         {
                                                             Scenario = value.Key,
                                                             RiskValue = value.Value,
                                                         }).ToArray(),
                            // Les positions sont dans les Margin Maturity
                            //positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, mrgClass.Positions, mrgClass.StocksCoverage),
                            Parameters = BuildMeffMarginMaturityParameter((MeffMarginMaturityCom[])mrgClass.Parameters),
                        }).ToArray();

                    method.Parameters = marginClassParameters;
                    method.InterCommoditySpread = BuildMeffInterCommoditySpreadParameter(pMethodComObj.InterCommoditySpread);

                    foreach (MeffMarginClassCom marginClass in marginClassCom)
                    {
                        if (marginClass.Missing)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, marginClass.ErrorCode, 0,
                                new LogParam(method.Name),
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                        }
                        else
                        {
                            foreach (MeffMarginMaturityCom marginMaturity in marginClass.Parameters)
                            {
                                if (marginMaturity.Missing)
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Warning, marginMaturity.ErrorCode, 0,
                                        new LogParam(method.Name),
                                        new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                                }
                            }
                        }
                    }
                }
            }
            return method;
        }

        /// <summary>
        /// Construction du détails du calcul pour chaque échéances
        /// </summary>
        /// <param name="pMarginMaturityCom"></param>
        /// <returns></returns>
        private MeffMarginMaturityParameter[] BuildMeffMarginMaturityParameter(MeffMarginMaturityCom[] pMarginMaturityCom)
        {
            MeffMarginMaturityParameter[] marginMaturityParameters = null;
            if (pMarginMaturityCom != default(MeffMarginMaturityCom[]))
            {
                marginMaturityParameters =
                    (from marginMaturity in pMarginMaturityCom
                     select new MeffMarginMaturityParameter
                     {
                         Name = marginMaturity.MarginAssetCode,
                         FuturePrice = marginMaturity.Price,
                         DeltaValues = marginMaturity.DeltaArray == null ? null :
                                        (from value in marginMaturity.DeltaArray
                                         select new ScenarioRiskValue
                                         {
                                             Scenario = value.Key,
                                             RiskValue = value.Value,
                                         }).ToArray(),
                         positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, marginMaturity.Positions, marginMaturity.StocksCoverage),
                     }).ToArray();
            }
            return marginMaturityParameters;
        }

        /// <summary>
        /// Construction du détail du calcul des spreads inter-commodity
        /// </summary>
        /// <param name="pInterSpreadCom"></param>
        /// <returns></returns>
        private MeffInterCommoditySpreadParameter[] BuildMeffInterCommoditySpreadParameter(MeffInterCommoditySpreadCom[] pInterSpreadCom)
        {
            MeffInterCommoditySpreadParameter[] interSpreadParameters = null;
            if (pInterSpreadCom != default(MeffInterCommoditySpreadCom[]))
            {
                interSpreadParameters =
                    (from interSpread in pInterSpreadCom
                     select new MeffInterCommoditySpreadParameter
                     {
                         Priority = interSpread.Priority,
                         DiscountType = interSpread.DiscountType,
                         NumberOfSpread = interSpread.NumberOfSpread,
                         LegParameters = BuildMeffInterCommoditySpreadLegParameter(interSpread.LegParameters),
                     }).ToArray();
            }
            return interSpreadParameters;
        }

        /// <summary>
        /// Construction du détail du calcul des jambes des spreads inter-commodity
        /// </summary>
        /// <param name="pInterSpreadLegCom"></param>
        /// <returns></returns>
        private MeffInterCommoditySpreadLegParameter[] BuildMeffInterCommoditySpreadLegParameter(MeffInterCommoditySpreadLegCom[] pInterSpreadLegCom)
        {
            MeffInterCommoditySpreadLegParameter[] interSpreadLegParameters = null;
            if (pInterSpreadLegCom != default(MeffInterCommoditySpreadLegCom[]))
            {
                interSpreadLegParameters =
                    (from interSpreadLeg in pInterSpreadLegCom
                     select new MeffInterCommoditySpreadLegParameter
                     {
                         MarginClassCode = interSpreadLeg.MarginClass.MarginClassCode,
                         MarginCredit = interSpreadLeg.MarginCredit,
                         DeltaPerSpread = interSpreadLeg.DeltaPerSpread,
                         DeltaAvailable = interSpreadLeg.DeltaAvailable,
                         DeltaRemaining = interSpreadLeg.DeltaRemaining,
                         DeltaConsumed = interSpreadLeg.DeltaConsumed,
                         SpreadCredit = interSpreadLeg.SpreadCredit,
                     }).ToArray();
            }
            return interSpreadLegParameters;
        }

    }
}
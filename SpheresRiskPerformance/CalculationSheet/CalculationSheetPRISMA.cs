using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//
using EFS.ACommon;
using EFS.Common;
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
    /// CalculationSheet repository, containing the results and the calculation details of a deposit
    /// </summary>
    /// <remarks>This part of the class includes the members building the method calculation details for the PRISMA method</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildPrismaMarginCalculationMethod(PrismaCalcMethCom pMethodComObj)
        {
            PrismaMarginCalculationMethod method = new PrismaMarginCalculationMethod();

            if (pMethodComObj != default(PrismaCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;

                //PM 20150417 [20957] Add PrismaRelease
                method.PrismaRelease = pMethodComObj.PrismaRelease;
                //PM 20150511 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (pMethodComObj.Parameters != default(PrismaLiquidGroupCom[]))
                {
                    PrismaLiquidGroupCom[] liquidGroupCom = (PrismaLiquidGroupCom[])pMethodComObj.Parameters;

                    // PM 20150907 [21236] Ajout PremiumMargin et MarginRequirement
                    PrismaLiquidGroupParameter[] liquidGroupParameters = (
                        from liquidGroup in liquidGroupCom
                        select new PrismaLiquidGroupParameter
                        {
                            Name = liquidGroup.Identifier,
                            IdLg = liquidGroup.IdLg,
                            CurrencyTypeFlag = liquidGroup.CurrencyTypeFlag,
                            ClearingCurrency = liquidGroup.ClearingCurrency,
                            InitialMargin = MarginMoney.FromMoney(liquidGroup.InitialMargin),
                            PremiumMargin = MarginMoney.FromMoney(liquidGroup.PremiumMargin),
                            MarginRequirement = MarginMoney.FromMoney(liquidGroup.MarginRequirement),
                            Parameters = BuildPrismaLiquidGroupSplitParameter((PrismaLiquidGroupSplitCom[])liquidGroup.Parameters),
                        }).ToArray();

                    method.Parameters = liquidGroupParameters;

                    // Missing parameters
                    foreach (PrismaLiquidGroupCom liquidGroup in liquidGroupCom)
                    {
                        if (liquidGroup.Missing)
                        {
                            //PM 20141216 [9700] Eurex Prisma for Eurosys Futures : Bypass (pas de tradeId)
                        //    this.m_LogWithData.Invoke(ProcessStateTools.StatusWarningEnum, LogLevelDetail.NONE, 0,
                        //        liquidGroup.ErrorCode, false, null,
                        //        new string[] 
                        //{
                        //    method.name,
                        //    // RD 20140414 [19815] Utiliser le Cst.OTCml_TradeIdScheme pour récupérer l'id du trade, et non pas le premier Id trouvé qui peut être un UTI, un id du trader, ....
                        //    //this.m_DataDocumentContainer.currentTrade.tradeHeader.partyTradeIdentifier[0].tradeId[0].Value
                        //    Tools.GetScheme(this.m_DataDocumentContainer.currentTrade.tradeHeader.partyTradeIdentifier[0].tradeId, Cst.OTCml_TradeIdScheme).Value
                        //});
                            string id;
                            if (m_ProcessInfo.SoftwareRequester == Software.SOFTWARE_EurosysFutures)
                            {
                                //PM 20150416 [20957] Utiliser l'id de l'acteur
                                //id = "N/A";
                                id = pMethodComObj.IdA.ToString();
                            }
                            else
                            {
                                id = Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value;
                            }

                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, liquidGroup.ErrorCode, 0, new LogParam(method.Name), new LogParam(id)));
                        }
                        else
                        {
                            foreach (PrismaLiquidGroupSplitCom liquidGroupSplit in liquidGroup.Parameters)
                            {
                                if (liquidGroupSplit.Missing)
                                {
                                    //PM 20141216 [9700] Eurex Prisma for Eurosys Futures : Bypass (pas de tradeId)
                                //    this.m_LogWithData.Invoke(ProcessStateTools.StatusWarningEnum, LogLevelDetail.NONE, 0,
                                //        liquidGroupSplit.ErrorCode, false, null,
                                //        new string[] 
                                //{
                                //    method.name,
                                //    // RD 20140414 [19815] Utiliser le Cst.OTCml_TradeIdScheme pour récupérer l'id du trade, et non pas le premier Id trouvé qui peut être un UTI, un id du trader, ....
                                //    //this.m_DataDocumentContainer.currentTrade.tradeHeader.partyTradeIdentifier[0].tradeId[0].Value
                                //    Tools.GetScheme(this.m_DataDocumentContainer.currentTrade.tradeHeader.partyTradeIdentifier[0].tradeId, Cst.OTCml_TradeIdScheme).Value
                                //});
                                    string id;
                                    if (m_ProcessInfo.SoftwareRequester == Software.SOFTWARE_EurosysFutures)
                                    {
                                        //PM 20150416 [20957] Utiliser l'id de l'acteur
                                        //id = "N/A";
                                        id = pMethodComObj.IdA.ToString();
                                    }
                                    else
                                    {
                                        id = Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value;
                                    }
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                    
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Warning, liquidGroupSplit.ErrorCode, 0, new LogParam(method.Name), new LogParam(id)));
                                }
                            }
                        }
                    }
                }
            }
            return method;
        }

        /// <summary>
        /// Construction des données du détails du calcul pour un Group Liquidation Split
        /// </summary>
        /// <param name="pLiquidGroupSplitCom"></param>
        /// <returns></returns>
        private PrismaLiquidGroupSplitParameter[] BuildPrismaLiquidGroupSplitParameter(PrismaLiquidGroupSplitCom[] pLiquidGroupSplitCom)
        {
            PrismaLiquidGroupSplitParameter[] liquidgroupSplitParameters = null;
            if (pLiquidGroupSplitCom != null)
            {
                // PM 20200826 [25467] Ajout détail du calcul Long Option Credit: Present Value et Maximal Lost
                liquidgroupSplitParameters = (
                    from liquidGroupSplit in pLiquidGroupSplitCom
                    select new PrismaLiquidGroupSplitParameter
                    {
                        IdLgs = liquidGroupSplit.IdLgs,
                        Name = liquidGroupSplit.Identifier,
                        AggregationMethod = liquidGroupSplit.AggregationMethod,
                        RiskMethod = liquidGroupSplit.RiskMethod,
                        AssetLiquidityRisk = BuildPrismaAssetLiquidityRisk(liquidGroupSplit.Positions, liquidGroupSplit.AssetLiquidityRisk),
                        MarketRisk = MarginMoney.FromMoney(liquidGroupSplit.MarketRisk),
                        // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustment
                        TimeToExpiryAdjustment = MarginMoney.FromMoney(liquidGroupSplit.TimeToExpiryAdjustment),
                        LiquidityRisk = MarginMoney.FromMoney(liquidGroupSplit.LiquidityRisk),
                        InitialMargin = MarginMoney.FromMoney(liquidGroupSplit.InitialMargin),
                        PremiumMargin = MarginMoney.FromMoney(liquidGroupSplit.PremiumMargin),
                        PresentValue = ((liquidGroupSplit.PresentValue != null) && (liquidGroupSplit.PresentValue.Count() > 0)) ? MarginMoney.FromMoney(liquidGroupSplit.PresentValue) : null,
                        MaximalLost = ((liquidGroupSplit.MaximalLost != null) && (liquidGroupSplit.MaximalLost.Count() > 0)) ? MarginMoney.FromMoney(liquidGroupSplit.MaximalLost) : null,
                        LongOptionCredit = ((liquidGroupSplit.LongOptionCredit != null) && (liquidGroupSplit.LongOptionCredit.Count() > 0)) ? MarginMoney.FromMoney(liquidGroupSplit.LongOptionCredit) : null,
                        TotalInitialMargin = MarginMoney.FromMoney(liquidGroupSplit.TotalInitialMargin),
                        positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, liquidGroupSplit.Positions, null), //liquidGroupSplit.StocksCoverage),
                        Parameters = BuildPrismaRiskMeasureSetParameter((PrismaRiskMeasureSetCom[])liquidGroupSplit.Parameters),
                    }
                    ).ToArray();
            }
            return liquidgroupSplitParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul pour un Risk Measure Set
        /// </summary>
        /// <param name="pRiskMeasureSetCom"></param>
        /// <returns></returns>
        private PrismaRiskMeasureSetParameter[] BuildPrismaRiskMeasureSetParameter(PrismaRiskMeasureSetCom[] pRiskMeasureSetCom)
        {
            PrismaRiskMeasureSetParameter[] riskMeasureSetParameters = null;
            if (pRiskMeasureSetCom != null)
            {
                riskMeasureSetParameters = (
                    from riskMeasureSet in pRiskMeasureSetCom
                    select new PrismaRiskMeasureSetParameter
                    {
                        IdRms = riskMeasureSet.IdRms,
                        Name = riskMeasureSet.Identifier,
                        HistoricalStressed = riskMeasureSet.HistoricalStressed,
                        AggregationMethod = riskMeasureSet.AggregationMethod,
                        ConfidenceLevel = riskMeasureSet.ConfidenceLevel,
                        IsUseRobustness = riskMeasureSet.IsUseRobustness,
                        ScalingFactor = riskMeasureSet.ScalingFactor,
                        CorrelationBreakConfidenceLevel = riskMeasureSet.CorrelationBreakConfidenceLevel,
                        CorrelationBreakSubWindow = riskMeasureSet.CorrelationBreakSubWindow,
                        CorrelationBreakMultiplier = riskMeasureSet.CorrelationBreakMultiplier,
                        CorrelationBreakMin = riskMeasureSet.CorrelationBreakMin,
                        CorrelationBreakMax = riskMeasureSet.CorrelationBreakMax,
                        
                        IsLiquidityComponent = riskMeasureSet.IsLiquidityComponent,
                        AlphaConfidenceLevelSpecified = riskMeasureSet.IsLiquidityComponent, 
                        AlphaConfidenceLevel = riskMeasureSet.AlphaConfidenceLevel,
                        AlphaFloorSpecified = riskMeasureSet.IsLiquidityComponent,
                        AlphaFloor = riskMeasureSet.AlphaFloor,
                        AlphaFactorSpecified = riskMeasureSet.IsLiquidityComponent,
                        AlphaFactor = riskMeasureSet.AlphaFactor,
                        MarketRiskComponent = riskMeasureSet.MarketRiskComponent,
                        ScaledMarketRiskComponent = riskMeasureSet.ScaledMarketRiskComponent,
                        ValueAtRiskLiquidityComponentSpecified = riskMeasureSet.IsLiquidityComponent,
                        ValueAtRiskLiquidityComponent= riskMeasureSet.ValueAtRiskLiquidityComponent, 
 
                        Parameters = BuildPrismaSubSampleParameter((PrismaSubSampleCom[])riskMeasureSet.Parameters),
                    }
                    ).ToArray();
            }
            return riskMeasureSetParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul pour un Sub-Sample
        /// </summary>
        /// <param name="pSubSampleCom"></param>
        /// <returns></returns>
        private PrismaSubSampleParameter[] BuildPrismaSubSampleParameter(PrismaSubSampleCom[] pSubSampleCom)
        {
            PrismaSubSampleParameter[] subSampleParameters = null;
            if (pSubSampleCom != null)
            {
                // PM 20161019 [22174] Prisma 5.0 : Ajout PureMarketRisk
                subSampleParameters = (
                    from subSample in pSubSampleCom
                    select new PrismaSubSampleParameter
                    {
                        CompressionError = subSample.CompressionError,
                        ValueAtRisk = subSample.ValueAtRisk,
                        ValueAtRiskScaled = subSample.ValueAtRiskScaled,
                        ValueAtRiskLiquidityComponentSpecified = subSample.ValueAtRiskLiquidityComponentSpecified,
                        ValueAtRiskLiquidityComponent = subSample.ValueAtRiskLiquidityComponent,
                        PureMarketRisk = subSample.PureMarketRisk,
                        //
                        MeanExcessRisk = subSample.MeanExcessRisk,
                        CorrelationBreakAdjustment = subSample.CorrelationBreakAdjustment,
                        CbLowerBound = subSample.CbLowerBound,
                        CbUpperBound = subSample.CbUpperBound,
                        CompressionAdjustment = subSample.CompressionAdjustment,
                        MarketRiskComponent = subSample.MarketRiskComponent,
                    }
                    ).ToArray();
            }
            return subSampleParameters;
        }

        /// <summary>
        /// Retourne un array de PrismaAssetLiquidityRisk
        /// </summary>
        /// <param name="pPosition">Liste des positions</param>
        /// <param name="pAssetLiquidityRisk">Liste des éléments de calcul de la liquidity Risk par asset</param>
        /// <returns></returns>
        private PrismaAssetLiquidityRisk[] BuildPrismaAssetLiquidityRisk(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPosition, Dictionary<int, PrismaAssetLiquidityRiskCom> pAssetLiquidityRisk)
        {
            PrismaAssetLiquidityRisk[] ret = new PrismaAssetLiquidityRisk[pPosition.Count()];
            int index = 0;
            foreach (Pair<PosRiskMarginKey, RiskMarginPosition> pos in pPosition)
            {
                //PM 20141216 [9700] Eurex Prisma for Eurosys Futures : AssetETDCache non alimenté
                //SQL_AssetETD asset = this.AssetETDCache[pos.First.idAsset];
                //if (null == asset)
                //    throw new Exception(StrFunc.AppendFormat("asset (id:{0}) doesn't in cache", pos.First.idAsset));

                //ret[index] = new PrismaAssetLiquidityRisk();
                //ret[index].ID = asset.Identifier;
                ret[index] = new PrismaAssetLiquidityRisk();
                if ((null != AssetETDCache) && (AssetETDCache.TryGetValue(pos.First.idAsset, out SQL_AssetETD asset)))
                {
                    // PM 20180918 [XXXXX] Test Prisma Eurosys : ajout vérification de la validité du SQL_AssetETD
                    if (asset != default(SQL_AssetETD))
                    {
                        ret[index].ID = asset.Identifier;
                    }
                    else
                    {
                        ret[index].ID = string.Format( "IdAsset: {0}", pos.First.idAsset);
                    }
                }
                else
                {
                    ret[index].ID = "N/A";
                }

                if (pAssetLiquidityRisk != null)
                {
                    if (pAssetLiquidityRisk.ContainsKey(pos.First.idAsset))
                    {
                        ret[index].tradeUnit = pAssetLiquidityRisk[pos.First.idAsset].tradeUnit;
                        ret[index].netGrossRatio = pAssetLiquidityRisk[pos.First.idAsset].netGrossRatio;
                        ret[index].liquidityPremium = pAssetLiquidityRisk[pos.First.idAsset].liquidityPremium;
                        ret[index].liquidityFactor = pAssetLiquidityRisk[pos.First.idAsset].liquidityFactor;
                        ret[index].riskMeasure = pAssetLiquidityRisk[pos.First.idAsset].riskMeasure;
                        ret[index].liquidityAdjustment = pAssetLiquidityRisk[pos.First.idAsset].liquidityAdjustment;
                        ret[index].additionalRiskMeasure = pAssetLiquidityRisk[pos.First.idAsset].additionalRiskMeasure;
                    }
                }

                index++;
            }
            return ret;
        }
    }
}

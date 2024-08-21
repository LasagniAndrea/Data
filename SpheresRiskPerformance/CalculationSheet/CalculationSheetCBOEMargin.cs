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
using EFS.SpheresRiskPerformance.RiskMethods;
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
    /// <remarks>Cette partie de la class inclue les membres permettant de contruire le détail du calcul par la méthode CBOE Margin</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildCBOEMarginCalculationMethod(
            CboeMarginCalcMethCom pMethodComObj)
        {
            CboeMarginCalculationMethod method = new CboeMarginCalculationMethod();

            // PM 20131212 [19332] Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(CboeMarginCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;

                // PM 20191025 [24983] Ajout MaintenanceInitial
                method.MaintenanceInitial = pMethodComObj.IsMaintenanceAmount ? "MNT" : "INIT";
                method.MaintenanceInitialSpecified = true;

                //PM 20150512 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (pMethodComObj.Parameters != default(CboeContractMarginCom[]))
                {
                    CboeContractMarginCom[] contractCom = (CboeContractMarginCom[])pMethodComObj.Parameters;
                    //
                    // PM 20191025 [24983] Ajout NormalMarginMaint et StrategyMarginMaint (rename de NormalMarginInit et StrategyMarginInit)
                    CboeMarginContractParameter[] contractParameters = (
                        from contract in contractCom
                        select new CboeMarginContractParameter
                        {
                            OTCmlId = contract.Contract.ContractId,
                            Name = contract.Contract.ContractDisplayname,
                            Symbol = contract.Contract.ContractSymbol,
                            PctOptionValue = contract.Contract.PctOptionValue,
                            PctUnderlyingValue = contract.Contract.PctUnderlyingValue,
                            PctMinimumUnderlyingValue = contract.Contract.PctMinimumUnderlyingValue,
                            UnderlyingQuote = contract.UnderlyingQuote,
                            NormalMarginInit = MarginMoney.FromMoney(contract.NormalMarginAmountInit),
                            NormalMarginMaint = MarginMoney.FromMoney(contract.NormalMarginAmountMaint),
                            StrategyMarginInit = MarginMoney.FromMoney(contract.StrategyMarginAmountInit),
                            StrategyMarginMaint = MarginMoney.FromMoney(contract.StrategyMarginAmountMaint),
                            marginAmount = MarginMoney.FromMoney(contract.MarginAmount),
                            Parameters = BuildCboeNormalMarginParameter(method.Name, (CboeNormalMarginCom[])contract.Parameters),
                            Strategy = (contract.StrategyMarginList.Count > 0) ? BuildCboeStrategyMarginParameter(contract.StrategyMarginList.ToArray()) : null,
                            // PM 20130322 Ajout position couverte par des actions
                            positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, contract.Positions, contract.StocksCoverage),
                        }).ToArray();

                    method.Parameters = contractParameters;

                    foreach (CboeContractMarginCom contract in contractCom)
                    {
                        if (contract.Missing)
                        {
                            if (null != contract.SystemMsgInfo)
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, contract.SystemMsgInfo.SysMsgCode, 0, contract.SystemMsgInfo.LogParamDatas));
                            }
                            else
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, contract.ErrorCode, 0,
                                    new LogParam(method.Name),
                                    new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                            }
                        }
                    }
                }
            }

            return method;
        }

        /// <summary>
        /// Construction des données du détails du calcul pour position séche
        /// </summary>
        /// <param name="pMethodName"></param>
        /// <param name="pAssetCom"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private CboeMarginNormalMarginParameter[] BuildCboeNormalMarginParameter(string pMethodName, CboeNormalMarginCom[] pAssetCom)
        {
            CboeMarginNormalMarginParameter[] assetParameters = null;
            if (pAssetCom != default(CboeNormalMarginCom[]))
            {
                // PM 20191025 [24983] Ajout UnitMarginMaint, MarginAmountInit, MarginAmountMaint
                assetParameters =
                    (from asset in pAssetCom
                     select new CboeMarginNormalMarginParameter
                     {
                         Asset = BuildCboeAssetParameter(asset.Asset),
                         Quote = asset.Quote,
                         UnitMarginInit = asset.UnitMarginInit,
                         UnitMarginMaint = asset.UnitMarginMaint,
                         UnitMinimumMargin = asset.UnitMinimumMargin,
                         InitialQuantity = asset.InitialQuantity,
                         Quantity = asset.Quantity,
                         ContractValue = MarginMoney.FromMoney(asset.ContractValue),
                         MinimumMargin = MarginMoney.FromMoney(asset.MinMarginAmount),
                         MarginAmountInit = MarginMoney.FromMoney(asset.MarginAmountInit),
                         MarginAmountMaint = MarginMoney.FromMoney(asset.MarginAmountMaint),
                         marginAmount = MarginMoney.FromMoney(asset.MarginAmount),
                         //positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, asset.Positions, null),
                     }).ToArray();

                foreach (CboeNormalMarginCom asset in pAssetCom)
                {
                    if (asset.Missing)
                    {
                        if (null != asset.SystemMsgInfo)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, asset.SystemMsgInfo.SysMsgCode, 0, asset.SystemMsgInfo.LogParamDatas));
                        }
                        else
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, asset.ErrorCode, 0, 
                                new LogParam(pMethodName),
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                        }
                    }
                }
            }
            return assetParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des stratégies
        /// </summary>
        /// <param name="pStrategyCom"></param>
        /// <returns></returns>
        private CboeMarginStrategyMarginParameter[] BuildCboeStrategyMarginParameter(CboeStrategyMarginCom[] pStrategyCom)
        {
            CboeMarginStrategyMarginParameter[] strategyParameters = null;
            if (pStrategyCom != default(CboeStrategyMarginCom[]))
            {
                // PM 20191025 [24983] Ajout UnitMarginMaint & MarginAmountMaint
                strategyParameters =
                    (from strategy in pStrategyCom
                     select new CboeMarginStrategyMarginParameter
                     {
                         StrategyType = strategy.StrategyTypeEnum.ToString(),
                         AssetFirstLeg = BuildCboeAssetParameter(strategy.Asset),
                         AssetSecondLeg = BuildCboeAssetParameter(strategy.AssetCombined),
                         QuantityFirstLeg = strategy.Quantity,
                         QuantitySecondLeg = strategy.QuantityCombined,
                         UnitMarginInit = strategy.UnitMarginInit,
                         UnitMarginMaint = strategy.UnitMarginMaint,
                         ContractValueFirstLeg = MarginMoney.FromMoney(strategy.ContractValue),
                         ContractValueSecondLeg = MarginMoney.FromMoney(strategy.ContractValueCombined),
                         MarginAmount = MarginMoney.FromMoney(strategy.MarginAmount),
                         MarginAmountInit = MarginMoney.FromMoney(strategy.MarginAmountInit),
                         MarginAmountMaint = MarginMoney.FromMoney(strategy.MarginAmountMaint),
                     }).ToArray();
            }
            return strategyParameters;
        }

        /// <summary>
        /// Construction du détails des données des assets
        /// </summary>
        /// <param name="pAsset"></param>
        /// <returns></returns>
        private CboeMarginAssetInformationParameter BuildCboeAssetParameter(CboeAssetExpandedParameter pAsset)
        {
            CboeMarginAssetInformationParameter assetParameters = null;
            if (pAsset != default(CboeAssetExpandedParameter))
            {
                assetParameters = new CboeMarginAssetInformationParameter
                {
                    OTCmlId = pAsset.AssetId,
                    PutCall = pAsset.PutCall,
                    Maturity = pAsset.MaturityYearMonth,
                    StrikePrice = pAsset.StrikePrice,
                    ContractMultiplier = pAsset.ContractMultiplier,
                };
            }
            return assetParameters;
        }
    }
}
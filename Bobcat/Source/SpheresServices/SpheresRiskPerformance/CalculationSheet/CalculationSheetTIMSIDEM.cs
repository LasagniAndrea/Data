using System;
using System.Linq;
//
using EFS.ACommon;
using EFS.Common.Log;
using EFS.Process;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.Properties;
//
using EfsML.Business;
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{

    /// <summary>
    /// CalculationSheet repository, containing the results and the calculation details of a deposit
    /// </summary>
    /// <remarks>this part of the class includes the members building the calculation sheet for the "TIMS IDEM" method</remarks>
    public sealed partial class CalculationSheetRepository
    {

        // EG 20190114 Add detail to ProcessLog Refactoring
        private TimsIdemMarginCalculationMethod BuildTimsIdemMarginCalculationMethod(TimsIdemMarginCalculationMethodCommunicationObject pMethodComObj)
        {
            TimsIdemMarginCalculationMethod method = new TimsIdemMarginCalculationMethod
            {
                Name = "TIMS IDEM"
            };

            // PM 20131212 [19332] Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(TimsIdemMarginCalculationMethodCommunicationObject))
            {
                method.CrossMarginActivated = pMethodComObj.CrossMarginActivated;
                //PM 20150511 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (pMethodComObj.Parameters != default(TimsIdemProductParameterCommunicationObject[]))
                {
                    method.Parameters = new TimsIdemProductParameter[pMethodComObj.Parameters.Length];
                    int idxProduct = 0;

                    foreach (TimsIdemProductParameterCommunicationObject productComObj in pMethodComObj.Parameters)
                    {
                        TimsIdemProductParameter product = new TimsIdemProductParameter();
                        method.Parameters[idxProduct++] = product;

                        product.Name = productComObj.Product;

                        if (productComObj.Missing || Settings.Default.TimsPositionExtendedReport)
                        {
                            product.positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, productComObj.Positions, null);
                        }

                        if (!productComObj.Missing)
                        {
                            product.marginAmount = MarginMoney.FromMoney(productComObj.MarginAmount);

                            product.Spread = BuildSpreadMarginReport(productComObj.Spread);

                            product.Mtm = BuildMtmMarginReport(productComObj.Mtm);

                            product.Premium = BuildPremiumMarginReport(productComObj.Premium);

                            product.Additional = BuildAdditionalMarginReport(productComObj.Additional);

                            product.Minimum = BuildMinimumMarginReport(productComObj.Minimum);

                            product.Parameters = new TimsIdemClassParameter[productComObj.Parameters.Length];
                            int idxClass = 0;

                            foreach (TimsIdemClassParameterCommunicationObject classComObj in productComObj.Parameters)
                            {
                                TimsIdemClassParameter classParam = new TimsIdemClassParameter();
                                product.Parameters[idxClass++] = classParam;

                                classParam.Name = classComObj.Class;

                                classParam.Symbols =
                                    (from symbol in classComObj.ContractSymbols select symbol)
                                    .Aggregate((total, next) => String.Format("{0}.{1}", total, next));

                                if (Settings.Default.TimsPositionExtendedReport)
                                {
                                    classParam.positions =
                                        BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, classComObj.Positions, null);
                                }

                                classParam.Spread = BuildSpreadMarginReport(classComObj.Spread);

                                classParam.Mtm = BuildMtmMarginReport(classComObj.Mtm);

                                classParam.Premium = BuildPremiumMarginReport(classComObj.Premium);

                                classParam.Additional = BuildAdditionalMarginReport(classComObj.Additional);

                                classParam.Minimum = BuildMinimumMarginReport(classComObj.Minimum);

                                classParam.Parameters = new TimsIdemContractParameter[classComObj.Parameters.Length];
                                int idxContract = 0;

                                foreach (TimsIdemContractParameterCommunicationObject contractComObj in classComObj.Parameters)
                                {
                                    TimsIdemContractParameter contractParam = new TimsIdemContractParameter();
                                    classParam.Parameters[idxContract++] = contractParam;

                                    contractParam.Name = contractComObj.Contract;
                                    contractParam.Description = contractComObj.Description;

                                    contractParam.Offset = contractComObj.Offset;

                                    contractParam.positions =
                                        BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false,
                                        contractComObj.Positions, contractComObj.StocksCoverage);

                                    if (contractComObj.SpreadPositions != null)
                                    {
                                        if (contractComObj.SpreadPositions.Count() > 0)
                                        {
                                            contractParam.ConvertedPositions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false,
                                                contractComObj.SpreadPositions, contractComObj.StocksCoverage);
                                        }
                                    }
                                    //PM 20141113 [20491] Ajout du calcul des spreads au niveau Contract
                                    contractParam.Spread = BuildSpreadMarginReport(contractComObj.Spread);

                                    contractParam.Mtm = BuildMtmMarginReport(contractComObj.Mtm);

                                    contractParam.Premium = BuildPremiumMarginReport(contractComObj.Premium);

                                    contractParam.Additional = BuildAdditionalMarginReport(contractComObj.Additional);

                                }
                            }
                        }
                        else
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Warning, productComObj.ErrorCode, 0,
                                new LogParam(method.Name),
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                        }
                    }
                }
            }
            return method;
        }
    }
}
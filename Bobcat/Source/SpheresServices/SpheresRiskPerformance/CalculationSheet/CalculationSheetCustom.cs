using System;
//
using EFS.ACommon;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Enum;
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
    /// <remarks>this part of the class includes the members building the method calculation details for the "custom" method</remarks>
    public sealed partial class CalculationSheetRepository
    {

        // EG 20190114 Add detail to ProcessLog Refactoring
        private CustomMarginCalculationMethod BuildCustomMarginCalculationMethod(CustomMarginCalculationMethodCommunicationObject pMethodCommObj)
        {
            CustomMarginCalculationMethod method = new CustomMarginCalculationMethod
            {
                Name = "Custom"
            };

            // PM 20131212 [19332] Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodCommObj != default(CustomMarginCalculationMethodCommunicationObject))
            {
                //PM 20150512 [20575] Ajout date des paramètres de risque
                method.ParametersDate = pMethodCommObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (pMethodCommObj.Parameters != default(IRiskParameterCommunicationObject[]))
                {
                    method.Parameters = new CustomContractParameter[pMethodCommObj.Parameters.Length];

                    int idxParameter = 0;

                    foreach (IRiskParameterCommunicationObject parameterCommObj in pMethodCommObj.Parameters)
                    {
                        CustomContractParameterCommunicationObject contractParameterCommObj =
                            (CustomContractParameterCommunicationObject)parameterCommObj;

                        CustomContractParameter contractParameter = new CustomContractParameter();

                        method.Parameters[idxParameter] = contractParameter;

                        contractParameter.OTCmlId = contractParameterCommObj.ContractId;
                        contractParameter.Identifier = contractParameterCommObj.Identifier;

                        contractParameter.marginExpressionType =
                            System.Enum.GetName(typeof(ExpressionType), contractParameterCommObj.ExpressionType);

                        if (contractParameterCommObj.Missing)
                        {
                            contractParameter.missing = true;
                            contractParameter.missingSpecified = true;
                        }

                        if (contractParameterCommObj.ExpressionType == ExpressionType.Percentage)
                        {
                            contractParameter.quote = contractParameterCommObj.Quote;
                            contractParameter.quoteSpecified = true;

                            contractParameter.multiplier = contractParameterCommObj.Multiplier;
                            contractParameter.multiplierSpecified = true;
                        }

                        contractParameter.marginAmount = MarginMoney.FromMoney(contractParameterCommObj.MarginAmount);

                        contractParameter.positions = BuildPositionReport
                            (DateTime.MinValue, SettlSessIDEnum.EndOfDay, false, contractParameterCommObj.Positions, null);

                        contractParameter.Parameters = new CustomAmountParameter[contractParameterCommObj.Parameters.Length];

                        int idxAmount = 0;

                        foreach (IRiskParameterCommunicationObject subParameterCommObj in contractParameterCommObj.Parameters)
                        {
                            CustomAmountParameterCommunicationObject amountParameterCommObj =
                                (CustomAmountParameterCommunicationObject)subParameterCommObj;

                            CustomAmountParameter amountParameter = new CustomAmountParameter();

                            contractParameter.Parameters[idxAmount] = amountParameter;

                            amountParameter.amount = amountParameterCommObj.RiskValue;
                            amountParameter.type = amountParameterCommObj.Type;

                            amountParameter.positions =
                                BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.EndOfDay, true, subParameterCommObj.Positions, null);

                            amountParameter.marginAmount = MarginMoney.FromMoney(subParameterCommObj.MarginAmount);

                            idxAmount++;
                        }


                        if (contractParameterCommObj.Missing)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, contractParameterCommObj.ErrorCode, 2,
                                new LogParam(contractParameterCommObj.Identifier),
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                        }

                        idxParameter++;
                    }
                }
            }
            return method;
        }
    }
}
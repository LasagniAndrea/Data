using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using EfsML.v30.MarginRequirement;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheetRepository, contient le resultats et le détail du calcul de déposit
    /// </summary>
    /// <remarks>Cette partie de la class inclue les membres permettant de contruire le détail du calcul par la méthode IMSM</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildIMSMCalculationMethod(IMSMCalcMethCom pMethodComObj)
        {
            IMSMCalculationMethod method = new IMSMCalculationMethod();

            // Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(IMSMCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;
                // PM 20180316 [23840] Ajout Version
                method.Version = pMethodComObj.MethodVersion;
                method.VersionSpecified = true;

                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;

                method.BusinessCenter = pMethodComObj.BusinessCenter;
                method.IMSMDate = pMethodComObj.DtIMSM;

                // PM 20170808 [23371] Ajout log
                if ((pMethodComObj.ExchangeRate != default(IMExchangeRateParameterCom[]))
                    && pMethodComObj.ExchangeRate.Any(e => e.Missing))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1001), 0,
                        new LogParam(method.Name),
                        new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                }
                
                if (pMethodComObj.IMSMParameter != default(IMSMGlobalParameterCom))
                {
                    IMSMGlobalParameterCom parameterCom = pMethodComObj.IMSMParameter;
                    IMSMGlobalParameterDetail globalParameter = new IMSMGlobalParameterDetail
                    {
                        // PM 20200910 [25482] Ajout IsCalcCESMOnly
                        IsCalcCESMOnly = parameterCom.IsCalcCESMOnly,
                        IsWithHolidayAdjustment = parameterCom.IsWithHolidayAdjustment,
                        WindowSizeStatistic = parameterCom.WindowSizeStatistic,
                        WindowSizeMaximum = parameterCom.WindowSizeMaximum,
                        EWMAFactor = parameterCom.EWMAFactor,
                        Alpha = parameterCom.Alpha,
                        Beta = parameterCom.Beta,
                        MinIMSMInitial = parameterCom.MinIMSMInitial,
                        MinIMSMInitialWindowSize = parameterCom.MinIMSMInitialWindowSize,
                        MinIMSM = parameterCom.MinIMSM
                    };

                    method.IMSMParameter = globalParameter;
                    method.IMSMParameterSpecified = true;

                    // PM 20170808 [23371] Ajout
                    if (pMethodComObj.IMSMParameter.CESMParameters != default(IMSMCESMParameterCom[]))
                    {
                        IMSMCESMParameterCom[] cesmParameters = pMethodComObj.IMSMParameter.CESMParameters;
                        globalParameter.CESMParameters = (
                            from param in cesmParameters
                            select new IMSMCESMParameter
                            {
                                ContractIdentifier = param.ContractIdentifier,
                                IdCC = param.IdCC,
                                MarginParameterBuy = param.MarginParameterBuy,
                                MarginParameterSell = param.MarginParameterSell,
                            }).ToArray();
                    }

                    if (pMethodComObj.ExchangeRate != default(IMExchangeRateParameterCom[]))
                    {
                        IMExchangeRateParameter[] exchangeRateParameter = (
                            from rate in pMethodComObj.ExchangeRate
                            select new IMExchangeRateParameter
                            {
                                Currency1 = rate.Currency1,
                                Currency2 = rate.Currency2,
                                Rate = rate.Rate,
                                RateSpecified = (false == rate.Missing),
                                QuoteBasis = rate.QuoteBasisString,
                            }).ToArray();
                        
                        globalParameter.ExchangeRateParameter = exchangeRateParameter;

                        foreach (IMExchangeRateParameterCom exchangeRate in pMethodComObj.ExchangeRate)
                        {
                            if (exchangeRate.Missing)
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                                Logger.Log(new LoggerData(LogLevelEnum.Warning, exchangeRate.ErrorCode, 0,
                                    new LogParam(exchangeRate.Currency1),
                                    new LogParam(exchangeRate.Currency2),
                                    new LogParam(exchangeRate.QuoteBasisString),
                                    new LogParam(method.ParametersDate.ToShortDateString())));
                            }
                        }
                    }
                }

                // PM 20170808 [23371] exposure est remontée pour pouvoir contenir les différentes exposures
                IMSMExposureDetail exposure = new IMSMExposureDetail();
                if (pMethodComObj.Exposure != default)
                {
                    IMSMExposureCom exposureCom = pMethodComObj.Exposure;
                    //IMSMExposureDetail exposure = new IMSMExposureDetail();
                    exposure.WindowDateMin = exposureCom.WindowDateMin;
                    exposure.T0Exposure = exposureCom.T0Exposure;
                    if (exposureCom.Exposure != default(Dictionary<DateTime, decimal>))
                    {
                        exposure.Exposure = exposureCom.Exposure.Select(e => new IMSMExposureItem { Date = e.Key, Value = e.Value }).ToArray();
                        exposure.ExposureSpecified = (exposure.Exposure.Count() > 0);
                    }
                    //else
                    //{
                    //    exposure.Exposure = new IMSMExposureItem[0];
                    //}
                    //method.Exposure = exposure;
                    //method.ExposureSpecified = true;
                }
                // PM 20170808 [23371] Ajout
                if (pMethodComObj.CurrentExposure != default(IMSMCurrentExposureCom[]))
                {
                    IMSMCurrentExposureCom[] currentExposureCom = pMethodComObj.CurrentExposure;
                    // PM 20200910 [25482] Ajout IdAsset
                    //exposure.CurrentExposure = currentExposureCom.Select(e => new IMSMCurrentExposureItem { IdCC = e.IdCC, ExposureBuy = e.ExposureBuy, ExposureSell = e.ExposureSell }).ToArray();
                    exposure.CurrentExposure = currentExposureCom.Select(e => new IMSMCurrentExposureItem
                    {
                        IdCC = e.IdCC,
                        IdAsset = e.IdAsset,
                        AssetIdentifier = e.AssetIdentifier,
                        ExposureBuy = e.ExposureBuy,
                        ExposureSell = e.ExposureSell
                    }).ToArray();
                    exposure.CurrentExposureSpecified = (exposure.CurrentExposure.Count() > 0);
                } 
                // PM 20170808 [23371] Déplacé aprés construction des différentes exposures
                if ((exposure.Exposure != default(IMSMExposureItem[])) || (exposure.CurrentExposure != default(IMSMCurrentExposureItem[])))
                {
                    method.Exposure = exposure;
                    method.ExposureSpecified = true;
                }
                //
                if (pMethodComObj.IMSMCalculationData != default(IMSMCalculationCom))
                {
                    IMSMCalculationCom calculationCom = pMethodComObj.IMSMCalculationData;
                    IMSMCalculationDetail calculation = new IMSMCalculationDetail
                    {
                        EffectiveImsmDate = calculationCom.EffectiveImsmDate,
                        AgreementDate = calculationCom.AgreementDate,
                        HolydayAdjDays = calculationCom.HolydayAdjDays,
                        HolydayAdjAmount = calculationCom.HolydayAdjAmount,
                        T0Exposure = calculationCom.T0Exposure,
                        Mean = calculationCom.Mean,
                        NoDataPoint = calculationCom.NoDataPoint,
                        StandardDeviation = calculationCom.StandardDeviation,
                        SDS = calculationCom.SDS,
                        MaxShortWindow = calculationCom.MaxShortWindow,
                        BetaMax = calculationCom.BetaMax,
                        MainImsm = calculationCom.MainImsm,
                        RoundedImsm = calculationCom.RoundedImsm
                    };

                    method.IMSMCalculation = calculation;
                    method.IMSMCalculationSpecified = true;
                }
                else
                {
                    method.IMSMCalculationSpecified = false;
                }
                //
                // PM 20200910 [25482] Ajout gestion CESMCalculationData
                if (pMethodComObj.CESMCalculationData != default(CESMCalculationCom))
                {
                    CESMCalculationCom calculationCom = pMethodComObj.CESMCalculationData;
                    CESMCalculationDetail calculation = new CESMCalculationDetail
                    {
                        CESMAMount = calculationCom.CESMAMount
                    };
                    method.CESMCalculation = calculation;
                    method.CESMCalculationSpecified = true;
                }
                else
                {
                    method.CESMCalculationSpecified = false;
                }
            }
            return method;
        }
    }
}


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
//
using FixML.Enum;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheetRepository, contient le resultats et le détail du calcul de déposit
    /// </summary>
    /// <remarks>Cette partie de la class inclue les membres permettant de construire le détail du calcul par la méthode NOMX_CFM</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détails du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        private MarginCalculationMethod BuildNOMXCFMCalculationMethod(NOMXCFMCalcMethCom pMethodComObj)
        {
            NOMXCFMCalculationMethod method = new NOMXCFMCalculationMethod();

            // Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(NOMXCFMCalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;
                //
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                //
                if (null != pMethodComObj.Parameters)
                {
                    NOMXCFMResultCurveAmountCom[] curveAmounts = (NOMXCFMResultCurveAmountCom[])pMethodComObj.Parameters;
                    method.Parameters = BuildResultCurveAmount(curveAmounts);
                }
                if (pMethodComObj.CalculationDetail != default(NOMXCFMResultDetailCom))
                {
                    method.CalculationDetail = BuildResultDetail(pMethodComObj.CalculationDetail);
                }
                // Log en cas de paramètres manquants sur la clearing house
                if (pMethodComObj.Missing)
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, pMethodComObj.ErrorCode, 0,
                            new LogParam(this.GetSQLActorFromCache(m_Cs, this.ProcessInfo.CssId).Identifier),
                            new LogParam(Convert.ToString(this.m_ProcessInfo.CssId)),
                            new LogParam(method.Name),
                            new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                }
            }
            return method;
        }

        /// <summary>
        /// Construction des données du détail des montants
        /// </summary>
        /// <param name="pResultCom"></param>
        /// <returns></returns>
        private NOMXCFMResultCurveAmount[] BuildResultCurveAmount(NOMXCFMResultCurveAmountCom[] pResultCom)
        {
            NOMXCFMResultCurveAmount[] resultDetail = null;
            if (pResultCom != default(NOMXCFMResultCurveAmountCom[]))
            {
                resultDetail = (
                    from amount in pResultCom
                    select new NOMXCFMResultCurveAmount
                    {
                        CurveName = amount.CurveName,
                        MarginClass = amount.MarginClass,
                        marginAmount = MarginMoney.FromMoney(amount.MarginAmount),
                        positions = (amount.Positions != null) ? BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, amount.Positions, null) : default,
                    }).ToArray();
            }
            return resultDetail;
        }

        /// <summary>
        /// Construction des données du détail du calcul
        /// </summary>
        /// <param name="pResultCom"></param>
        /// <returns></returns>
        private NOMXCFMResultDetail BuildResultDetail(NOMXCFMResultDetailCom pResultCom)
        {
            NOMXCFMResultDetail detail = null;
            if (pResultCom != default(NOMXCFMResultDetailCom))
            {
                detail = new NOMXCFMResultDetail
                {
                    SingleCurves = BuildResultCurve(pResultCom.SingleCurveDetail),
                    CorrelationCurves = BuildResultCorrelationCurve(pResultCom.CorrelationCurveDetail)
                };
            }
            return detail;
        }

        /// <summary>
        /// Construction de la hiérarchie des correlation curves
        /// </summary>
        /// <param name="pCurveCom"></param>
        /// <returns></returns>
        private NOMXCFMResultCorrelationCurve[] BuildResultCorrelationCurve(IEnumerable<NOMXCFMResultCurveCom> pCurveCom)
        {
            NOMXCFMResultCorrelationCurve[] resultCurve = default;
            if (pCurveCom != default(IEnumerable<NOMXCFMResultCurveCom>))
            {
                resultCurve = (
                    from curve in pCurveCom
                    select new NOMXCFMResultCorrelationCurve
                    {
                        CurveName = curve.CurveName,
                        MarginClass = curve.MarginClass,
                        Curve = BuildResultCorrelationCurve(curve.ChildCurves),
                        OverlapPC1 = curve.OverlapPC1,
                        OverlapPC1Specified = curve.IsChildCurve,
                        OverlapPC2 = curve.OverlapPC2,
                        OverlapPC2Specified = curve.IsChildCurve,
                        OverlapPC3 = curve.OverlapPC3,
                        OverlapPC3Specified = curve.IsChildCurve,
                        marginAmount = MarginMoney.FromMoney(curve.MarginAmount),
                        positions = (curve.Positions != null) ? BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, curve.Positions, null) : default,
                    }).ToArray();
            }
            return resultCurve;
        }

        /// <summary>
        /// Construction des données d'une curve
        /// </summary>
        /// <param name="pCurveCom"></param>
        /// <returns></returns>
        private NOMXCFMCurveParameter[] BuildResultCurve(IEnumerable<NOMXCFMResultCurveCom> pCurveCom)
        {
            NOMXCFMCurveParameter[] resultCurve = default;
            if (pCurveCom != default(IEnumerable<NOMXCFMResultCurveCom>))
            {
                resultCurve = (
                    from curve in pCurveCom
                    select new NOMXCFMCurveParameter
                    {
                        CurveName = curve.CurveName,
                        MarginClass = curve.MarginClass,
                        Scenarios = BuildResultScenarios(curve.Scenarios),
                        marginAmount = MarginMoney.FromMoney(curve.MarginAmount),
                        positions = (curve.Positions != null) ? BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, curve.Positions, null) : default,
                    }).ToArray();
            }
            return resultCurve;
        }
        
        /// <summary>
        /// Construction des scénarios d'une curve
        /// </summary>
        /// <param name="pScenariosCom"></param>
        /// <returns></returns>
        private NOMXCFMScenarioParameter[] BuildResultScenarios(IEnumerable<NOMXCFMScenarioCom> pScenariosCom)
        {
            NOMXCFMScenarioParameter[] resultScenarios = default;
            if (pScenariosCom != default(IEnumerable<NOMXCFMScenarioCom>))
            {
                resultScenarios = (
                    from sc in pScenariosCom
                    select new NOMXCFMScenarioParameter
                    {
                        PC1PointNo = sc.PC1PointNo,
                        PC2PointNo = sc.PC2PointNo,
                        PC3PointNo = sc.PC3PointNo,
                        LowValue = sc.LowValue,
                        MiddleValue = sc.MiddleValue,
                        HighValue = sc.HighValue,
                    }).ToArray();
            }
            return resultScenarios;
        }
        
    }
}

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
        /// Alimentation du log
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private MarginCalculationMethod BuildOMXMarginCalculationMethod(OMXCalcMethCom pMethodComObj)
        {
            OMXMarginCalculationMethod ret = new OMXMarginCalculationMethod();

            // PM 20131212 [19332] Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(OMXCalcMethCom))
            {
                ret.Name = pMethodComObj.MarginMethodName;
                //PM 20150511 [20575] Ajout date des paramètres de risque
                ret.ParametersDate = pMethodComObj.DtParameters;
                ret.ParametersDateSpecified = true;
                //
                if (pMethodComObj.Parameters != default(OMXUnderlyingSymbolCom[]))
                {
                    OMXUnderlyingSymbolCom[] unlSymbolCom = (OMXUnderlyingSymbolCom[])pMethodComObj.Parameters;

                    OMXUnderlyingSymbolParameter[] unlSymbolParameters = (
                        from unlSymbol in unlSymbolCom
                        select new OMXUnderlyingSymbolParameter
                        {
                            Symbol = unlSymbol.Symbol,
                            marginAmount = MarginMoney.FromMoney(unlSymbol.MarginAmount),
                            positions = BuildPositionReport(DateTime.MinValue, SettlSessIDEnum.None, false, unlSymbol.Positions, null),
                        }).ToArray();


                    ret.Parameters = unlSymbolParameters;

                    foreach (OMXUnderlyingSymbolCom unlSymbol in unlSymbolCom)
                    {
                        if (unlSymbol.Missing)
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, unlSymbol.ErrorCode, 0,
                                new LogParam(ret.Name),
                                new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                        }
                    }
                }
            }
            return ret;
        }
    }
}
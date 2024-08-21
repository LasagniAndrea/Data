using System;
using System.Collections.Generic;
using System.Linq;
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
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheet contenant les résultats et le détail du calcul d'un déposit
    /// </summary>
    /// <remarks>Cette partie de la classe comprend les membres construisant le détail du calcul de la méthode SPAN2</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détail du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        private MarginCalculationMethod BuildSpan2MarginCalculationMethod(SPAN2CalcMethCom pMethodComObj)
        {
            Span2MarginCalculationMethod method = new Span2MarginCalculationMethod();

            if (pMethodComObj != default(SPAN2CalcMethCom))
            {
                method.Name = pMethodComObj.MarginMethodName;
                method.ParametersDate = pMethodComObj.DtParameters;
                method.ParametersDateSpecified = true;
                method.CycleCode = pMethodComObj.CycleCode;
                method.AccountType = pMethodComObj.SpanAccountType.ToString();
                method.MaintenanceInitial = pMethodComObj.IsMaintenanceAmount ? "MNT" : "INIT";
                method.AccountTypeSpecified = true;
                method.MaintenanceInitialSpecified = true;
                method.IsTryExcludeWrongPosition = pMethodComObj.IsTryExcludeWrongPosition;
                // PM 20231030 [26547][WI735] Ajout MarginCurrencyType
                method.MarginCurrencyType = pMethodComObj.MarginCurrencyTypeEnum.ToString();
                method.MarginCurrencyTypeSpecified = true;

                method.BaseUrlSpecified = StrFunc.IsFilled(pMethodComObj.BaseUrl);
                if (method.BaseUrlSpecified)
                {
                    method.BaseUrl = pMethodComObj.BaseUrl;
                }

                method.UserIdSpecified = StrFunc.IsFilled(pMethodComObj.UserId);
                if (method.UserIdSpecified)
                {
                    method.UserId = pMethodComObj.UserId;
                }

                // Log de la position écartée car problème de référentiel
                // PM 20230830 [26470] Ajout des positions écartées car échues
                //if (pMethodComObj.IsTryExcludeWrongPosition)
                if ((pMethodComObj.DiscartedPositions != default) && (pMethodComObj.DiscartedPositions.Count() > 0))
                {
                    method.DiscartedPositions = BuildPositionReport(default, pMethodComObj.Timing, false, pMethodComObj.DiscartedPositions, null);
                }

                method.ConsideredPositions = BuildPositionReport(default, pMethodComObj.Timing, false, pMethodComObj.ConsideredPositions, null);

                // PM 20230929 [XXXXX] Changement de type de XmlRequestMessage: string => List<string>
                if ((pMethodComObj.XmlRequestMessage != default(List<string>)) && (pMethodComObj.XmlRequestMessage.Count() > 0))
                {
                    method.XmlRequestMessage = pMethodComObj.XmlRequestMessage.ToArray();
                    method.XmlRequestMessageSpecified = true;
                }
                else
                {
                    method.XmlRequestMessageSpecified = false;
                }

                // PM 20230929 [XXXXX] Changement de type de XmlResponseMessage: string => List<string>
                if ((pMethodComObj.XmlResponseMessage != default(List<string>)) && (pMethodComObj.XmlResponseMessage.Count() > 0))
                {
                    method.XmlResponseMessage = pMethodComObj.XmlResponseMessage.ToArray();
                    method.XmlResponseMessageSpecified = true;
                }
                else
                {
                    method.XmlResponseMessageSpecified = false;
                }

                method.JsonRequestMessage = pMethodComObj.JsonRequestMessage;
                method.JsonResponseMessage = pMethodComObj.JsonResponseMessage;

                // Compteurs
                if (pMethodComObj.CounterInfo != default(MarginCounterCom))
                {
                    method.MarginCounter = new Span2MarginCounter
                    {
                        NbAssetParameters = pMethodComObj.CounterInfo.NbAssetParameters,
                        NbInitialPosition = pMethodComObj.CounterInfo.NbInitialPosition,
                        NbNettedPosition = pMethodComObj.CounterInfo.NbNettedPosition,
                        NbReducedPosition = pMethodComObj.CounterInfo.NbReducedPosition,
                        NbActivePosition = pMethodComObj.CounterInfo.NbActivePosition,
                        NbDiscartedPosition = pMethodComObj.CounterInfo.NbDiscartedPosition,
                        NbConsideredPosition = pMethodComObj.CounterInfo.NbConsideredPosition,
                        NbSpanRiskPosition = pMethodComObj.CounterInfo.NbSpanRiskPosition,
                        NbProcessedPosition = pMethodComObj.CounterInfo.NbProcessedPosition
                    };
                }

                // Erreur principale
                if (pMethodComObj.Missing)
                {
                    IScheme tradeScheme = Tools.GetScheme(m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme);
                    string tradeInfo = tradeScheme != default(IScheme) ? tradeScheme.Value : "";
                    string acteur = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].PartyReference.HRef;
                    string book = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].BookId.BookName;

                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    // PM 20230830 [26470] Gestion liste d'erreurs
                    //Logger.Log(new LoggerData(LogLevelEnum.Warning, pMethodComObj.ErrorCode, 0,
                    //    new LogParam(method.name),
                    //    new LogParam(LogTools.IdentifierAndId(acteur, pMethodComObj.IdA)),
                    //    new LogParam(LogTools.IdentifierAndId(book, pMethodComObj.IdB)),
                    //    new LogParam(tradeInfo)));

                    foreach ((SysMsgCode, List<LogParam>) erreur in pMethodComObj.ErrorList)
                    {
                        List<LogParam> erreurParam = new List<LogParam>
                        {
                            new LogParam(method.Name),
                            new LogParam(LogTools.IdentifierAndId(acteur, pMethodComObj.IdA)),
                            new LogParam(LogTools.IdentifierAndId(book, pMethodComObj.IdB)),
                            new LogParam(tradeInfo)
                        };

                        if ((erreur.Item2 != default(List<LogParam>)) && (erreur.Item2.Count() > 0))
                        {
                            erreurParam.AddRange(erreur.Item2);
                        }

                        Logger.Log(new LoggerData(LogLevelEnum.Warning, erreur.Item1, 0, erreurParam));
                    }
                }

                // Message d'erreur complémentaire
                if (pMethodComObj.ErrorMessage != default)
                {
                    method.ErrorMessage = pMethodComObj.ErrorMessage;

                    // PM 20230830 [26470] Suppression car gestion liste d'erreurs
                    //m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    //Logger.Log(new LoggerData(LogLevelEnum.Warning, pMethodComObj.ErrorMessage, 0));
                }

                // Alimentation des montants globaux du calcul
                if (pMethodComObj.RequierementAmounts != default(Span2TotalAmountCom))
                {
                    method.SpanTotalAmounts = BuildSpan2TotalAmount(pMethodComObj.RequierementAmounts);
                }

                // Alimentation des données du détail du calcul
                if (null != pMethodComObj.Parameters)
                {
                    Span2CCPCom[] ccpCom = (Span2CCPCom[])pMethodComObj.Parameters;

                    method.Parameters = BuildSpan2CCPParameter(ccpCom);
                }
            }
            return method;
        }

        /// <summary>
        /// Construction des données des montants totaux
        /// </summary>
        /// <param name="pSpan2TotalAmountCom"></param>
        /// <returns></returns>
        private Span2TotalAmount BuildSpan2TotalAmount(Span2TotalAmountCom pSpan2TotalAmountCom)
        {
            Span2TotalAmount totalAmount = default;
            if (pSpan2TotalAmountCom != default(Span2TotalAmountCom))
            {
                totalAmount = new Span2TotalAmount();
                if (pSpan2TotalAmountCom.NetOptionValue != default(IMoney))
                {
                    totalAmount.NetOptionValue = new MarginMoney
                    {
                        Amount = pSpan2TotalAmountCom.NetOptionValue.Amount.DecValue,
                        Currency = pSpan2TotalAmountCom.NetOptionValue.Currency
                    };
                    totalAmount.NetOptionValueSpecified = true;
                }
                else
                {
                    totalAmount.NetOptionValueSpecified = false;
                }
                if (pSpan2TotalAmountCom.RiskInitialAmount != default(IMoney))
                {
                    totalAmount.RiskInitialAmount = new MarginMoney
                    {
                        Amount = pSpan2TotalAmountCom.RiskInitialAmount.Amount.DecValue,
                        Currency = pSpan2TotalAmountCom.RiskInitialAmount.Currency
                    };
                    totalAmount.RiskInitialAmountSpecified = true;
                }
                else
                {
                    totalAmount.RiskInitialAmountSpecified = false;
                }
                if (pSpan2TotalAmountCom.RiskMaintenanceAmount != default(IMoney))
                {
                    totalAmount.RiskMaintenanceAmount = new MarginMoney
                    {
                        Amount = pSpan2TotalAmountCom.RiskMaintenanceAmount.Amount.DecValue,
                        Currency = pSpan2TotalAmountCom.RiskMaintenanceAmount.Currency
                    };
                    totalAmount.RiskMaintenanceAmountSpecified = true;
                }
                else
                {
                    totalAmount.RiskMaintenanceAmountSpecified = false;
                }
                if (pSpan2TotalAmountCom.TotalInitialMarginAmount != default(IMoney))
                {
                    totalAmount.TotalInitialMarginAmount = new MarginMoney
                    {
                        Amount = pSpan2TotalAmountCom.TotalInitialMarginAmount.Amount.DecValue,
                        Currency = pSpan2TotalAmountCom.TotalInitialMarginAmount.Currency
                    };
                    totalAmount.TotalInitialMarginAmountSpecified = true;
                }
                else
                {
                    totalAmount.TotalInitialMarginAmountSpecified = false;
                }
                if (pSpan2TotalAmountCom.TotalMaintenanceMarginAmount != default(IMoney))
                {
                    totalAmount.TotalMaintenanceMarginAmount = new MarginMoney
                    {
                        Amount = pSpan2TotalAmountCom.TotalMaintenanceMarginAmount.Amount.DecValue,
                        Currency = pSpan2TotalAmountCom.TotalMaintenanceMarginAmount.Currency
                    };
                    totalAmount.TotalMaintenanceMarginAmountSpecified = true;
                }
                else
                {
                    totalAmount.TotalMaintenanceMarginAmountSpecified = false;
                }
            }
            return totalAmount;
        }

        /// <summary>
        /// Construction des données du détails pour le CCP (exchange complex)
        /// </summary>
        /// <param name="pCcpCom"></param>
        /// <returns></returns>
        /// <summary>
        private Span2CCPParameter[] BuildSpan2CCPParameter(Span2CCPCom[] pCcpCom)
        {
            Span2CCPParameter[] ccpParameter = null;
            if (pCcpCom != default(Span2CCPCom[]))
            {
                ccpParameter = (
                    from ccp in pCcpCom
                    select new Span2CCPParameter
                    {
                        Name = ccp.ClearingOrganization,
                        Parameters = BuildSpan2GroupParameter(pCcpCom),
                        NetOptionValue = (from money in ccp.NetOptionValue
                                          select new MarginMoney
                                          {
                                              Currency = money.Currency,
                                              Amount = money.Amount.DecValue
                                          }).ToArray(),
                        RiskInitial = (from money in ccp.RiskInitialAmount
                                       select new MarginMoney
                                       {
                                           Currency = money.Currency,
                                           Amount = money.Amount.DecValue
                                       }).ToArray(),
                        RiskMaintenance = (from money in ccp.RiskMaintenanceAmount
                                           select new MarginMoney
                                           {
                                               Currency = money.Currency,
                                               Amount = money.Amount.DecValue
                                           }).ToArray(),
                        TotalInitialMarginAmount = (from money in ccp.TotalInitialMarginAmount
                                                     select new MarginMoney
                                                     {
                                                         Currency = money.Currency,
                                                         Amount = money.Amount.DecValue
                                                     }).ToArray(),
                        TotalMaintenanceMarginAmount = (from money in ccp.TotalMaintenanceMarginAmount
                                                         select new MarginMoney
                                                         {
                                                             Currency = money.Currency,
                                                             Amount = money.Amount.DecValue
                                                         }).ToArray(),
                    }).ToArray();

            }
            return ccpParameter;
        }

        /// <summary>
        /// Construction des données du détails du calcul des groupes pour compatibilité SPAN
        /// </summary>
        /// <param name="pCcpCom"></param>
        /// <returns></returns>
        private Span2GroupParameter[] BuildSpan2GroupParameter(Span2CCPCom[] pCcpCom)
        {
            Span2GroupParameter[] groupParameters = null;
            if (pCcpCom != default(Span2CCPCom[]))
            {
                List<Span2GroupParameter> Span2GroupList = new List<Span2GroupParameter>();

                foreach (Span2CCPCom ccp in pCcpCom)
                {
                    if ((ccp.Parameters != default(Span2PodCom[])) && (ccp.Parameters.Length != 0))
                    {
                        var Span2GroupFromPop = 
                        from pod in (Span2PodCom[])ccp.Parameters
                        group pod by pod.MarginMethod into podGroup
                        select new Span2GroupParameter
                        {
                            Name = podGroup.Key,
                            LongOptionValue = (from money in podGroup.Select(p => p.LongOptionValue)
                                               group money by money.Currency into moneyCur
                                               select new MarginMoney
                                               {
                                                   Currency = moneyCur.Key,
                                                   Amount = moneyCur.Sum(m => m.Amount.DecValue)
                                               }).ToArray(),
                            ShortOptionValue = (from money in podGroup.Select(p => p.ShortOptionValue)
                                                group money by money.Currency into moneyCur
                                                select new MarginMoney
                                                {
                                                    Currency = moneyCur.Key,
                                                    Amount = moneyCur.Sum(m => m.Amount.DecValue)
                                                }).ToArray(),
                            NetOptionValue = (from money in podGroup.Select(p => p.NetOptionValue)
                                              group money by money.Currency into moneyCur
                                              select new MarginMoney
                                              {
                                                  Currency = moneyCur.Key,
                                                  Amount = moneyCur.Sum(m => m.Amount.DecValue)
                                              }).ToArray(),
                            RiskInitial = (from money in podGroup.Select(p => p.RiskInitialAmount)
                                           group money by money.Currency into moneyCur
                                           select new MarginMoney
                                           {
                                               Currency = moneyCur.Key,
                                               Amount = moneyCur.Sum(m => m.Amount.DecValue)
                                           }).ToArray(),
                            RiskMaintenance = (from money in podGroup.Select(p => p.RiskMaintenanceAmount)
                                               group money by money.Currency into moneyCur
                                               select new MarginMoney
                                               {
                                                   Currency = moneyCur.Key,
                                                   Amount = moneyCur.Sum(m => m.Amount.DecValue)
                                               }).ToArray(),
                            Parameters = BuildSpan2PodParameter(podGroup.ToArray()),
                        };

                        Span2GroupList.AddRange(Span2GroupFromPop);
                    }
                }

                groupParameters = Span2GroupList.ToArray();
            }
            return groupParameters;
        }

         /// <summary>
        /// Construction des données du détails du calcul des groupes de contrats
        /// </summary>
        /// <param name="pContractGroupCom"></param>
        /// <returns></returns>
        private Span2PodParameter[] BuildSpan2PodParameter(Span2PodCom[] pPodCom)
        {
            Span2PodParameter[] podParameters = null;
            if (pPodCom != default(Span2PodCom[]))
            {
                podParameters =
                    (from pod in pPodCom
                     select new Span2PodParameter
                     {
                         Name = pod.ContractGroup,
                         MarginMethod = pod.MarginMethod,
                         //
                         LongNonOptionValue = MarginMoney.FromMoney(pod.LongNonOptionValue),
                         ShortNonOptionValue = MarginMoney.FromMoney(pod.ShortNonOptionValue),
                         LongOptionValue = MarginMoney.FromMoney(pod.LongOptionValue),
                         ShortOptionValue = MarginMoney.FromMoney(pod.ShortOptionValue),
                         LongOptionFuturesStyleValue = MarginMoney.FromMoney(pod.LongOptionFuturesStyleValue),
                         ShortOptionFuturesStyleValue = MarginMoney.FromMoney(pod.ShortOptionFuturesStyleValue),
                         NetOptionValue = MarginMoney.FromMoney(pod.NetOptionValue),
                         ShortOptionMinimum = MarginMoney.FromMoney(pod.ShortOptionMinimum),
                         //
                         ScanRisk = MarginMoney.FromMoney(pod.ScanRiskAmount),
                         InterCommodityVolatilityCredit = MarginMoney.FromMoney(pod.InterCommodityVolatilityCredit),
                         IntraCommoditySpreadCharge = MarginMoney.FromMoney(pod.IntraSpreadChargeAmount),
                         DeliveryMonthCharge = MarginMoney.FromMoney(pod.DeliveryMonthChargeAmount),
                         InterCommodityCredit = MarginMoney.FromMoney(pod.InterCommodityCreditAmount),
                         IntexCommodityCredit = MarginMoney.FromMoney(pod.InterExchangeCreditAmount),
                         //
                         FullValueComponent = MarginMoney.FromMoney(pod.FullValueComponent),
                         ConcentrationComponent = MarginMoney.FromMoney(pod.ConcentrationComponent),
                         HvarComponent = MarginMoney.FromMoney(pod.HvarComponent),
                         LiquidityComponent = MarginMoney.FromMoney(pod.LiquidityComponent),
                         StressComponent = MarginMoney.FromMoney(pod.StressComponent),
                         ImpliedOffset = MarginMoney.FromMoney(pod.ImpliedOffset),
                         //
                         RiskInitial = MarginMoney.FromMoney(pod.RiskInitialAmount),
                         RiskMaintenance = MarginMoney.FromMoney(pod.RiskMaintenanceAmount),
                         //
                         Parameters = BuildSpan2ProductGroupParameter((Span2ProductGroupCom[])pod.Parameters),
                     }).ToArray();
            }
            return podParameters;
        }

        /// <summary>
        /// Construction des données du détails du calcul des ProductGroup
        /// </summary>
        /// <param name="pContractCom"></param>
        /// <returns></returns>
        private Span2ProductGroupParameter[] BuildSpan2ProductGroupParameter(Span2ProductGroupCom[] pProductGroupCom)
        {
            Span2ProductGroupParameter[] productGroupParameters = null;
            if (pProductGroupCom != default(Span2ProductGroupCom[]))
            {
                productGroupParameters =
                    (from prdGrp in pProductGroupCom
                     select new Span2ProductGroupParameter
                     {
                         Name = prdGrp.ProductGroup,
                         //
                         ConcentrationComponent = MarginMoney.FromMoney(prdGrp.ConcentrationComponent),
                         HvarComponent = MarginMoney.FromMoney(prdGrp.HvarComponent),
                         LiquidityComponent = MarginMoney.FromMoney(prdGrp.LiquidityComponent),
                         StressComponent = MarginMoney.FromMoney(prdGrp.StressComponent),
                         //
                         RiskInitialAmount = MarginMoney.FromMoney(prdGrp.RiskInitialAmount),
                         RiskMaintenanceAmount = MarginMoney.FromMoney(prdGrp.RiskMaintenanceAmount),
                         //

                     }).ToArray();
            }
            return productGroupParameters;
        }
    }
}
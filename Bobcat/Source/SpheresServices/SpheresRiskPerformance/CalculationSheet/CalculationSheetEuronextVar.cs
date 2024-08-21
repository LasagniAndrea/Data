using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EfsML.Business;
//
using EfsML.v30.MarginRequirement;
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheetRepository, contient le resultats et le détail du calcul de déposit
    /// </summary>
    /// <remarks>Cette partie de la class inclue les membres permettant de construire le détail du calcul par la méthode Euronext Var Based</remarks>
    public sealed partial class CalculationSheetRepository
    {
        /// <summary>
        /// Construction des données du détail du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <returns></returns>
        private MarginCalculationMethod BuildEuronextVarCalculationMethod(EuronextVarCalcMethCom pMethodComObj)
        {
            EuronextVarCalculationMethod methodCalc = new EuronextVarCalculationMethod();

            // Vérification qu'il y a bien des données communiquées pour le log
            if (pMethodComObj != default(EuronextVarCalcMethCom))
            {
                methodCalc.Name = pMethodComObj.MarginMethodName;
                methodCalc.ParametersDateSpecified = true;
                methodCalc.ParametersDate = pMethodComObj.DtParameters;

                BuildEuronextVarDiscartedPositions(methodCalc, pMethodComObj);

                BuildEuronextVarAssetIncomplete(methodCalc, pMethodComObj);

                if (pMethodComObj.EuronextVarSectorDetail != default(EuronextVarCalcSectorCom[]))
                {
                    methodCalc.SectorMargin = pMethodComObj.EuronextVarSectorDetail.Select(s => new EuronextVarCalcSector { Sector = s.Sector.ToString() }).ToArray();

                    IEnumerable<(EuronextVarCalcSector SectorCalc, EuronextVarCalcSectorCom SectorComObj)> logSector =
                        from sectorCom in pMethodComObj.EuronextVarSectorDetail
                        join sectorCalc in methodCalc.SectorMargin on sectorCom.Sector.ToString() equals sectorCalc.Sector
                        select (sectorCalc, sectorCom);

                    foreach ((EuronextVarCalcSector SectorCalc, EuronextVarCalcSectorCom SectorComObj) in logSector)
                    {
                        BuildEuronextVarCalcSector(SectorCalc, pMethodComObj, SectorComObj);
                    }
                }

                if (pMethodComObj.Missing)
                {
                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    Logger.Log(new LoggerData(LogLevelEnum.Warning, pMethodComObj.ErrorCode, 0,
                            new LogParam(this.GetSQLActorFromCache(m_Cs, this.ProcessInfo.CssId).Identifier),
                            new LogParam(Convert.ToString(this.m_ProcessInfo.CssId)),
                            new LogParam(methodCalc.Name),
                            new LogParam(Tools.GetScheme(this.m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value)));
                }
                else if (ArrFunc.Count(pMethodComObj.AssetIncomplet) > 0)
                {
                    if (BoolFunc.IsTrue(ConfigurationManager.AppSettings["EuronextVar_LogDisplayAssetIncomplet"]))
                    {
                        m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                        IScheme tradeScheme = Tools.GetScheme(m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme);
                        string tradeInfo = tradeScheme != default(IScheme) ? tradeScheme.Value : "";
                        string acteur = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].PartyReference.HRef;
                        string book = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].BookId.BookName;

                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1039), 0,
                            new LogParam(methodCalc.Name),
                            new LogParam(LogTools.IdentifierAndId(acteur, pMethodComObj.IdA)),
                            new LogParam(LogTools.IdentifierAndId(book, pMethodComObj.IdB)),
                            new LogParam(tradeInfo)));
                    }
                }
            }
            return methodCalc;
        }

        /// <summary>
        /// Construction du log de chaque sector
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        /// <param name="pMethodSectorComObj"></param>
        private void BuildEuronextVarCalcSector(EuronextVarCalcSector pMethodCalc, EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pMethodSectorComObj)
        {
            BuildEuronextVarParameters(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarDeliveryParameters(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarLookbackPeriod(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarObservationNumber(pMethodCalc, pMethodSectorComObj);


            if (pMethodSectorComObj.EuronextVarGroupDetail != default(EuronextVarCalcGroupCom[]))
            {
                pMethodCalc.GroupMarginDetail = pMethodSectorComObj.EuronextVarGroupDetail.Select(g => new EuronextVarCalcGroup { Group = g.Group }).ToArray();

                IEnumerable<(EuronextVarCalcGroup GroupCalc, EuronextVarCalcGroupCom GroupComObj)> logGroup =
                    from groupCom in pMethodSectorComObj.EuronextVarGroupDetail
                    join groupCalc in pMethodCalc.GroupMarginDetail on groupCom.Group equals groupCalc.Group
                    select (groupCalc, groupCom);

                foreach ((EuronextVarCalcGroup GroupCalc, EuronextVarCalcGroupCom GroupComObj) in logGroup)
                {
                    BuildEuronextVarCalcGroup(GroupCalc, pMethodComObj, GroupComObj);
                }
            }

            if (pMethodSectorComObj.EuronextVarNearExpiryDetail != default(EuronextVarCalcPhyDlyNearExpiryCom[]))
            {
                pMethodCalc.NearExpiryMarginDetail = pMethodSectorComObj.EuronextVarNearExpiryDetail.Select(g => new EuronextVarCalcAssetNearExpiry { IsinCode = g.IsinCode }).ToArray();

                IEnumerable<(EuronextVarCalcAssetNearExpiry NearExpiryCalc, EuronextVarCalcPhyDlyNearExpiryCom NearExpiryComObj)> logGroup =
                    from nearExpiryCom in pMethodSectorComObj.EuronextVarNearExpiryDetail
                    join nearExpiryCalc in pMethodCalc.NearExpiryMarginDetail on nearExpiryCom.IsinCode equals nearExpiryCalc.IsinCode
                    select (nearExpiryCalc, nearExpiryCom);

                foreach ((EuronextVarCalcAssetNearExpiry NearExpiryCalc, EuronextVarCalcPhyDlyNearExpiryCom NearExpiryComObj) in logGroup)
                {
                    BuildEuronextVarAssetNearExpiry(NearExpiryCalc, NearExpiryComObj);
                }
            }

            if (pMethodSectorComObj.EuronextVarPhysicalDeliveryDetail != default(EuronextVarCalcPhysicalDeliveryCom[]))
            {
                pMethodCalc.PhysicalDeliveryMarginDetail = pMethodSectorComObj.EuronextVarPhysicalDeliveryDetail.Select(g => new EuronextVarCalcAssetPhysicalDelivery { IsinCode = g.IsinCode }).ToArray();

                IEnumerable<(EuronextVarCalcAssetPhysicalDelivery DeliveryCalc, EuronextVarCalcPhysicalDeliveryCom DeliveryComObj)> logGroup =
                    from deliveryryCom in pMethodSectorComObj.EuronextVarPhysicalDeliveryDetail
                    join deliveryCalc in pMethodCalc.PhysicalDeliveryMarginDetail on deliveryryCom.IsinCode equals deliveryCalc.IsinCode
                    select (deliveryCalc, deliveryryCom);

                foreach ((EuronextVarCalcAssetPhysicalDelivery DeliveryCalc, EuronextVarCalcPhysicalDeliveryCom DeliveryComObj) in logGroup)
                {
                    BuildEuronextVarAssetPhysicalDelivery(DeliveryCalc, DeliveryComObj);
                }
            }

            BuildEuronextVarMarkToMarket(pMethodCalc, pMethodComObj, pMethodSectorComObj);

            BuildEuronextVarSectorAdditionalMargin(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarSectorTotalMarginGroup(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarSectorTotalMarginNearExpiry(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarSectorTotalMarginPhysicalDelivery(pMethodCalc, pMethodSectorComObj);

            BuildEuronextVarSectorTotalMargin(pMethodCalc, pMethodSectorComObj);
        }

        /// <summary>
        /// Construction du log de chaque groupe
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        /// <param name="pMethodGroupComObj"></param>
        private void BuildEuronextVarCalcGroup(EuronextVarCalcGroup pMethodCalc, EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcGroupCom pMethodGroupComObj)
        {
            BuildEuronextVardInitialMargin(pMethodCalc, pMethodGroupComObj);

            BuildEuronextVarDecorrelationAddOn(pMethodCalc, pMethodGroupComObj);

            BuildEuronextVarGroupAdditionalMargin(pMethodCalc, pMethodGroupComObj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalcSector"></param>
        /// <param name="pMethodSectorComObj"></param>
        private static void BuildEuronextVarParameters(EuronextVarCalcSector pMethodCalcSector, EuronextVarCalcSectorCom pMethodSectorComObj)
        {
            if (pMethodSectorComObj.EuronextVarParameters != default(EuronextVarParametersCom))
            {
                pMethodCalcSector.EuronextVarParameters = new EuronextVarParameters()
                {
                    OrdinaryConfidenceLevel = pMethodSectorComObj.EuronextVarParameters.OrdinaryConfidenceLevel,
                    StressedConfidenceLevel = pMethodSectorComObj.EuronextVarParameters.StressedConfidenceLevel,
                    DecorrelationParameter = pMethodSectorComObj.EuronextVarParameters.DecorrelationParameter,
                    OrdinaryWeight = pMethodSectorComObj.EuronextVarParameters.OrdinaryWeight,
                    StressedWeight = pMethodSectorComObj.EuronextVarParameters.StressedWeight,
                    HoldingPeriod = pMethodSectorComObj.EuronextVarParameters.HoldingPeriod,
                    SubPortfolioSeparatorDaysNumber = pMethodSectorComObj.EuronextVarParameters.SubPortfolioSeparatorDaysNumber,
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalcSector"></param>
        /// <param name="pMethodSectorComObj"></param>
        private static void BuildEuronextVarDeliveryParameters(EuronextVarCalcSector pMethodCalcSector, EuronextVarCalcSectorCom pMethodSectorComObj)
        {
            if (pMethodSectorComObj.DeliveryParameters != default(EuronextVarDeliveryParametersCom[]))
            {
                pMethodCalcSector.DeliveryParameters =
                    (from param in pMethodSectorComObj.DeliveryParameters
                     select new EuronextVarDeliveryParameters()
                     {
                         ContractCode = param.ContractCode,
                         Currency = param.Currency,
                         Sens = param.Sens,
                         ExtraPercentage = param.ExtraPercentage,
                         MarginPercentage = param.MarginPercentage,
                         FeePercentage = param.FeePercentage,
                     }).ToArray();

                pMethodCalcSector.DeliveryParametersSpecified = true;
            }
            else
            {
                pMethodCalcSector.DeliveryParametersSpecified = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalcSector"></param>
        /// <param name="pMethodSectorComObj"></param>
        private static void BuildEuronextVarLookbackPeriod(EuronextVarCalcSector pMethodCalcSector, EuronextVarCalcSectorCom pMethodSectorComObj)
        {
            if (pMethodSectorComObj.LookbackPeriod != default)
            {
                pMethodCalcSector.LookBackPeriod = new EuronextVarLookBackPeriod
                {
                    Ordinary = pMethodSectorComObj.LookbackPeriod.TypeS,
                    Stressed = pMethodSectorComObj.LookbackPeriod.TypeU
                };
            }
            if (pMethodSectorComObj.LookbackPeriodDelivery != default)
            {
                pMethodCalcSector.LookBackPeriodDelivery = new EuronextVarLookBackPeriod
                {
                    Ordinary = pMethodSectorComObj.LookbackPeriodDelivery.TypeS,
                    Stressed = pMethodSectorComObj.LookbackPeriodDelivery.TypeU
                };

                pMethodCalcSector.LookBackPeriodDeliverySpecified = true;
            }
            else
            {
                pMethodCalcSector.LookBackPeriodDeliverySpecified = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalcSector"></param>
        /// <param name="pMethodSectorComObj"></param>
        private static void BuildEuronextVarObservationNumber(EuronextVarCalcSector pMethodCalcSector, EuronextVarCalcSectorCom pMethodSectorComObj)
        {
            if (pMethodSectorComObj.ObservationNumber != default)
            {
                pMethodCalcSector.ObservationNumber = new EuronextVarObservationNumber()
                {
                    Ordinary = new EuronextVarObservationNumberDetail
                    {
                        Decvalue = pMethodSectorComObj.ObservationNumber.TypeS.DecValue,
                        Value = pMethodSectorComObj.ObservationNumber.TypeS.IntValue
                    },
                    Stressed = new EuronextVarObservationNumberDetail
                    {
                        Decvalue = pMethodSectorComObj.ObservationNumber.TypeU.DecValue,
                        Value = pMethodSectorComObj.ObservationNumber.TypeU.IntValue
                    }
                };
            }
            if (pMethodSectorComObj.ObservationNumberDelivery != default)
            {
                pMethodCalcSector.ObservationNumberDelivery = new EuronextVarObservationNumber()
                {
                    Ordinary = new EuronextVarObservationNumberDetail
                    {
                        Decvalue = pMethodSectorComObj.ObservationNumberDelivery.TypeS.DecValue,
                        Value = pMethodSectorComObj.ObservationNumberDelivery.TypeS.IntValue
                    },
                    Stressed = new EuronextVarObservationNumberDetail
                    {
                        Decvalue = pMethodSectorComObj.ObservationNumberDelivery.TypeU.DecValue,
                        Value = pMethodSectorComObj.ObservationNumberDelivery.TypeU.IntValue
                    }
                };

                pMethodCalcSector.ObservationNumberDeliverySpecified = true;
            }
            else
            {
                pMethodCalcSector.ObservationNumberDeliverySpecified = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVardInitialMargin(EuronextVarCalcGroup pMethodCalc, EuronextVarCalcGroupCom pMethodComObj)
        {
            if (pMethodComObj.ExpectedShortfallBook != default)
            {
                pMethodCalc.InitialMargin = new EuronextVarInitialMargin
                {
                    Ordinary = new EuronextVarExpectedShortfallsContainer
                    {
                        ExpectedShortfalls = BuildEuronextVarExpectedShortfalls(pMethodComObj.ExpectedShortfallBook.TypeS)
                    },
                    Stressed = new EuronextVarExpectedShortfallsContainer
                    {
                        ExpectedShortfalls = BuildEuronextVarExpectedShortfalls(pMethodComObj.ExpectedShortfallBook.TypeU)
                    }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarDecorrelationAddOn(EuronextVarCalcGroup pMethodCalc, EuronextVarCalcGroupCom pMethodComObj)
        {
            if ((pMethodComObj.ExpectedShortfallDecorrelation != default) && pMethodComObj.ExpectedShortfallDecorrelation.Count > 0)
            {
                pMethodCalc.DecorrelationAddOn = new EuronextVarDecorrelationAddOn
                {
                    Ordinary = new EuronextVarExpectedShortfallsDecoGroupContainer()
                    {
                        ExpectedShortfalls = new EuronextVarExpectedShortfallsDecorrelationGroup[pMethodComObj.ExpectedShortfallDecorrelation.Count]
                    },

                    Stressed = new EuronextVarExpectedShortfallsDecoGroupContainer()
                    {
                        ExpectedShortfalls = new EuronextVarExpectedShortfallsDecorrelationGroup[pMethodComObj.ExpectedShortfallDecorrelation.Count]
                    }
                };

                EuronextVarDecorrelationAddOn decorrelationAddOn = pMethodCalc.DecorrelationAddOn;

                int i = 0;
                foreach (string key in pMethodComObj.ExpectedShortfallDecorrelation.Keys)
                {
                    decorrelationAddOn.Ordinary.ExpectedShortfalls[i] = BuildEuronextVarExpectedShortfallsDecorrelation(key, pMethodComObj.ExpectedShortfallDecorrelation[key].TypeS);
                    decorrelationAddOn.Stressed.ExpectedShortfalls[i] = BuildEuronextVarExpectedShortfallsDecorrelation(key, pMethodComObj.ExpectedShortfallDecorrelation[key].TypeU);
                    i++;
                }

                decorrelationAddOn.Ordinary.DecorrelationExpectedShortfalls = new EuronextVarResult
                {
                    Function = pMethodComObj.DecorellationExpectedShortfall.TypeS.Function,
                    Value = pMethodComObj.DecorellationExpectedShortfall.TypeS.Value,
                };

                decorrelationAddOn.Stressed.DecorrelationExpectedShortfalls = new EuronextVarResult
                {
                    Function = pMethodComObj.DecorellationExpectedShortfall.TypeU.Function,
                    Value = pMethodComObj.DecorellationExpectedShortfall.TypeU.Value,
                };

                decorrelationAddOn.Ordinary.Result = new EuronextVarResult
                {
                    Function = pMethodComObj.DecorrelationAddOnResult.TypeS.Function,
                    Value = pMethodComObj.DecorrelationAddOnResult.TypeS.Value,
                };

                decorrelationAddOn.Stressed.Result = new EuronextVarResult
                {
                    Function = pMethodComObj.DecorrelationAddOnResult.TypeU.Function,
                    Value = pMethodComObj.DecorrelationAddOnResult.TypeU.Value,
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarAssetNearExpiry(EuronextVarCalcAssetNearExpiry pMethodCalc, EuronextVarCalcPhyDlyNearExpiryCom pMethodComObj)
        {
            if (pMethodComObj != default(EuronextVarCalcPhyDlyNearExpiryCom))
            {
                pMethodCalc.IsinCode = pMethodComObj.IsinCode;

                pMethodCalc.IncreasePercentage = pMethodComObj.IncreasePercentage;

                pMethodCalc.Ordinary = new EuronextVarExpectedShortfallsAssetContainer
                {
                    ExpectedShortfalls = BuildEuronextVarExpectedShortfallsAsset(pMethodComObj.IsinCode, pMethodComObj.ExpectedShortfallAssetNearExpiry.TypeS),
                };

                pMethodCalc.Stressed = new EuronextVarExpectedShortfallsAssetContainer
                {
                    ExpectedShortfalls = BuildEuronextVarExpectedShortfallsAsset(pMethodComObj.IsinCode, pMethodComObj.ExpectedShortfallAssetNearExpiry.TypeU),
                };

                pMethodCalc.FloorMargin = new EuronextVarResult
                {
                    Function = pMethodComObj.FloorMargin.Function,
                    Value = pMethodComObj.FloorMargin.Value
                };

                pMethodCalc.RiskMeasureMargin = new EuronextVarCalc
                {
                    Value1 = pMethodComObj.RiskMeasureMargin.Value1,
                    Value2 = pMethodComObj.RiskMeasureMargin.Value2,
                    Result = new EuronextVarResult
                    {
                        Function = pMethodComObj.RiskMeasureMargin.Result.Function,
                        Value = pMethodComObj.RiskMeasureMargin.Result.Value
                    }
                };

                pMethodCalc.NearExpiryMargin = new EuronextVarResult
                {
                    Function = pMethodComObj.NearExpiryMargin.Function,
                    Value = pMethodComObj.NearExpiryMargin.Value
                };
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarAssetPhysicalDelivery(EuronextVarCalcAssetPhysicalDelivery pMethodCalc, EuronextVarCalcPhysicalDeliveryCom pMethodComObj)
        {
            if (pMethodComObj != default(EuronextVarCalcPhysicalDeliveryCom))
            {
                pMethodCalc.IsinCode = pMethodComObj.IsinCode;

                pMethodCalc.ExtraPercentage = pMethodComObj.ExtraPercentage;

                pMethodCalc.Ordinary = new EuronextVarExpectedShortfallsAssetContainer
                {
                    ExpectedShortfalls = BuildEuronextVarExpectedShortfallsAsset(pMethodComObj.IsinCode, pMethodComObj.ExpectedShortfallAssetDelivery.TypeS),
                };

                pMethodCalc.Stressed = new EuronextVarExpectedShortfallsAssetContainer
                {
                    ExpectedShortfalls = BuildEuronextVarExpectedShortfallsAsset(pMethodComObj.IsinCode, pMethodComObj.ExpectedShortfallAssetDelivery.TypeU),
                };

                pMethodCalc.FloorMargin = new EuronextVarResult
                {
                    Function = pMethodComObj.FloorMargin.Function,
                    Value = pMethodComObj.FloorMargin.Value
                };

                pMethodCalc.RiskMeasureMargin = new EuronextVarCalc
                {
                    Value1 = pMethodComObj.RiskMeasureMargin.Value1,
                    Value2 = pMethodComObj.RiskMeasureMargin.Value2,
                    Result = new EuronextVarResult
                    {
                        Function = pMethodComObj.RiskMeasureMargin.Result.Function,
                        Value = pMethodComObj.RiskMeasureMargin.Result.Value
                    }
                };

                pMethodCalc.DeliveryMargin = new EuronextVarResult
                {
                    Function = pMethodComObj.DeliveryMargin.Function,
                    Value = pMethodComObj.DeliveryMargin.Value
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalcSector"></param>
        /// <param name="pMethodComObj"></param>
        /// <param name="pMethodSectorComObj"></param>
        private void BuildEuronextVarMarkToMarket(EuronextVarCalcSector pMethodCalcSector, EuronextVarCalcMethCom pMethodComObj, EuronextVarCalcSectorCom pMethodSectorComObj)
        {
            if ((pMethodSectorComObj.MarkToMarket != default) && ArrFunc.Count(pMethodSectorComObj.MarkToMarket.MarkToMarketPos) > 0)
            {
                EuronextVarMarkToMarketCom MarkToMarketCom = pMethodSectorComObj.MarkToMarket;

                pMethodCalcSector.MarkToMarket = new EuronextVarMarkToMarket()
                {
                    MarkToMarketPos = (from item in MarkToMarketCom.MarkToMarketPos
                                       let pos = new Pair<PosRiskMarginKey, RiskMarginPosition>[1] { item.Position }
                                       let fixPos = BuildPositionReport(DateTime.MinValue, FixML.Enum.SettlSessIDEnum.None, pHideInstrument: false, pos, null)
                                       select new EuronextVarMarkToMarketPos
                                       {
                                           Position = ArrFunc.IsFilled(fixPos) ? fixPos[0] : null,
                                           // Price
                                           PriceSpecified = item.Price.HasValue,
                                           Price = item.Price.HasValue ? new EuronextVarPrice()
                                           {
                                               Missing = item.PriceMissing.Item1,
                                               MissingSpecified = item.PriceMissing.Item1,
                                               Value = item.Price.Value
                                           } : null,
                                           // UnderlyingPrice
                                           UnderlyingPriceSpecified = item.UnderlyingPrice.HasValue,
                                           UnderlyingPrice = item.UnderlyingPrice.HasValue ? new EuronextVarPrice()
                                           {
                                               Missing = item.UnderlyingPriceMissing.Item1,
                                               MissingSpecified = item.UnderlyingPriceMissing.Item1,
                                               Value = item.UnderlyingPrice.Value
                                           } : null,

                                           Multiplier = item.Multiplier,
                                           MarkToMarket = item.MarkToMarket
                                       }).ToArray(),
                    Result = new EuronextVarResultMoney
                    {
                        Function = MarkToMarketCom.MarkToMarketAmount.Function,
                        Amount = MarkToMarketCom.MarkToMarketAmount.Value
                    }

                };

                if (MarkToMarketCom.Missing)
                {
                    m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                    IScheme tradeScheme = Tools.GetScheme(m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme);
                    string tradeInfo = tradeScheme != default(IScheme) ? tradeScheme.Value : "";
                    string acteur = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].PartyReference.HRef;
                    string book = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].BookId.BookName;

                    Logger.Log(new LoggerData(LogLevelEnum.Warning, MarkToMarketCom.ErrorCode, 0,
                        new LogParam(pMethodComObj.MarginMethodName),
                        new LogParam(LogTools.IdentifierAndId(acteur, pMethodComObj.IdA)),
                        new LogParam(LogTools.IdentifierAndId(book, pMethodComObj.IdB)),
                        new LogParam(tradeInfo)));

                    foreach (var item in MarkToMarketCom.MarkToMarketPos.Where(x => x.MissingPrice))
                    {
                        if (item.PriceMissing.Item1)
                            LogMissingPrice(item.PriceMissing.Item2);
                        if (item.UnderlyingPriceMissing.Item1)
                            LogMissingPrice(item.UnderlyingPriceMissing.Item2);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemMSGInfo"></param>
        private void LogMissingPrice(SystemMSGInfo systemMSGInfo)
        {
            Logger.Log(new LoggerData(LogLevelEnum.Warning, systemMSGInfo.SysMsgCode, 0, systemMSGInfo.LogParamDatas));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarSectorAdditionalMargin(EuronextVarCalcSector pMethodCalc, EuronextVarCalcSectorCom pMethodComObj)
        {
            if (pMethodComObj.SectorAdditionalMargin != default)
            {
                pMethodCalc.SectorAdditionalMargin = new EuronextVarResultMoney
                {
                    Function = pMethodComObj.SectorAdditionalMargin.Function,
                    Amount = pMethodComObj.SectorAdditionalMargin.Amount
                };
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarGroupAdditionalMargin(EuronextVarCalcGroup pMethodCalc, EuronextVarCalcGroupCom pMethodComObj)
        {
            if (pMethodComObj.AdditionalMargin != default)
            {
                pMethodCalc.AdditionalMargin = new EuronextVarCalc
                {
                    Value1 = pMethodComObj.AdditionalMargin.Value1,
                    Value2 = pMethodComObj.AdditionalMargin.Value2,
                    Result = new EuronextVarResult
                    {
                        Function = pMethodComObj.AdditionalMargin.Result.Function,
                        Value = pMethodComObj.AdditionalMargin.Result.Value
                    }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarSectorTotalMargin(EuronextVarCalcSector pMethodCalc, EuronextVarCalcSectorCom pMethodComObj)
        {
            if (pMethodComObj.SectorTotalMargin != default)
            {
                pMethodCalc.SectorTotalMargin = new EuronextVarResultMoney
                {
                    Function = pMethodComObj.SectorTotalMargin.Function,
                    Amount = pMethodComObj.SectorTotalMargin.Amount
                };
            }

            if (pMethodComObj.TotalMargin != default)
            {
                pMethodCalc.TotalMargin = new EuronextVarCalcMoney
                {
                    Value1 = pMethodComObj.TotalMargin.Value1,
                    Value2 = pMethodComObj.TotalMargin.Value2,
                    Result = new EuronextVarResultMoney()
                    {
                        Function = pMethodComObj.TotalMargin.Result.Function,
                        Amount = pMethodComObj.TotalMargin.Result.Amount
                    }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarSectorTotalMarginGroup(EuronextVarCalcSector pMethodCalc, EuronextVarCalcSectorCom pMethodComObj)
        {
            if (pMethodComObj.GroupTotalMargin != default)
            {
                pMethodCalc.GroupTotalMargin = new EuronextVarCalcMoney
                {
                    Value1 = pMethodComObj.GroupTotalMargin.Value1,
                    Value2 = pMethodComObj.GroupTotalMargin.Value2,
                    Result = new EuronextVarResultMoney
                    {
                        Function = pMethodComObj.GroupTotalMargin.Result.Function,
                        Amount = pMethodComObj.GroupTotalMargin.Result.Amount
                    }
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarSectorTotalMarginNearExpiry(EuronextVarCalcSector pMethodCalc, EuronextVarCalcSectorCom pMethodComObj)
        {
            if (pMethodComObj.NearExpiryTotalMargin != default)
            {
                pMethodCalc.NearExpiryTotalMargin = new EuronextVarResultMoney
                {
                    Function = pMethodComObj.NearExpiryTotalMargin.Function,
                    Amount = pMethodComObj.NearExpiryTotalMargin.Amount
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarSectorTotalMarginPhysicalDelivery(EuronextVarCalcSector pMethodCalc, EuronextVarCalcSectorCom pMethodComObj)
        {
            if (pMethodComObj.PhysicalDeliveryTotalMargin != default)
            {
                pMethodCalc.PhysicalDeliveryTotalMargin = new EuronextVarResultMoney
                {
                    Function = pMethodComObj.PhysicalDeliveryTotalMargin.Function,
                    Amount = pMethodComObj.PhysicalDeliveryTotalMargin.Amount
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private void BuildEuronextVarDiscartedPositions(EuronextVarCalculationMethod pMethodCalc, EuronextVarCalcMethCom pMethodComObj)
        {
            if (ArrFunc.IsFilled(pMethodComObj.Positions.DiscartedPositions))
            {
                pMethodCalc.DiscartedPositions = new EuronextVarDiscartedPositions
                {
                    Information = "Parameters not found. Probable cause: ETD Asset or underlying asset unknown",
                    InformationSpecified = true,
                    positions = BuildPositionReport(DateTime.MinValue, FixML.Enum.SettlSessIDEnum.None, pHideInstrument: false, pMethodComObj.Positions.DiscartedPositions, null)
                };
            }

            if (pMethodComObj.Positions.Missing)
            {

                m_SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                IScheme tradeScheme = Tools.GetScheme(m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme);
                string tradeInfo = tradeScheme != default(IScheme) ? tradeScheme.Value : "";
                string acteur = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].PartyReference.HRef;
                string book = m_DataDocumentContainer.CurrentTrade.TradeHeader.PartyTradeIdentifier[0].BookId.BookName;
                Logger.Log(new LoggerData(LogLevelEnum.Warning, pMethodComObj.Positions.ErrorCode, 0,
                    new LogParam(pMethodCalc.Name),
                    new LogParam(LogTools.IdentifierAndId(acteur, pMethodComObj.IdA)),
                    new LogParam(LogTools.IdentifierAndId(book, pMethodComObj.IdB)),
                    new LogParam(tradeInfo)));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEuronextVarExpectedShortfallCom"></param>
        /// <returns></returns>
        private static EuronextVarExpectedShortfalls BuildEuronextVarExpectedShortfalls(EuronextVarExpectedShortfallCom pEuronextVarExpectedShortfallCom)
        {
            EuronextVarExpectedShortfalls ret = new EuronextVarExpectedShortfalls();

            if (pEuronextVarExpectedShortfallCom != default(EuronextVarExpectedShortfallCom) && ArrFunc.Count(pEuronextVarExpectedShortfallCom.ExtremeEvents) > 0)
            {
                ret = new EuronextVarExpectedShortfalls()
                {
                    ExpectedShortfall = (from item in pEuronextVarExpectedShortfallCom.ExtremeEvents.Keys
                                         select new EuronextVarExpectedShortfall()
                                         {
                                             number = item + 1,
                                             Value = pEuronextVarExpectedShortfallCom.ExtremeEvents[item]
                                         }).ToArray(),

                    Result = new EuronextVarResult()
                    {
                        Function = pEuronextVarExpectedShortfallCom.Result.Funcion,
                        Value = pEuronextVarExpectedShortfallCom.Result.Value
                    }
                };
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDecorrelationGroup"></param>
        /// <param name="pEuronextVarExpectedShortfallCom"></param>
        /// <returns></returns>
        private static EuronextVarExpectedShortfallsDecorrelationGroup BuildEuronextVarExpectedShortfallsDecorrelation(string pDecorrelationGroup, EuronextVarExpectedShortfallCom pEuronextVarExpectedShortfallCom)
        {
            EuronextVarExpectedShortfallsDecorrelationGroup ret = new EuronextVarExpectedShortfallsDecorrelationGroup();

            if (pEuronextVarExpectedShortfallCom != default)
            {
                EuronextVarExpectedShortfalls euronextVarExpectedShortfalls = BuildEuronextVarExpectedShortfalls(pEuronextVarExpectedShortfallCom);

                ret = new EuronextVarExpectedShortfallsDecorrelationGroup
                {
                    DecorrelationGroup = pDecorrelationGroup,
                    ExpectedShortfall = euronextVarExpectedShortfalls.ExpectedShortfall,
                    Result = euronextVarExpectedShortfalls.Result
                };
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pISINCode"></param>
        /// <param name="pEuronextVarExpectedShortfallCom"></param>
        /// <returns></returns>
        private static EuronextVarExpectedShortfallsAssetNearExpiry BuildEuronextVarExpectedShortfallsAsset(string pISINCode, EuronextVarExpectedShortfallCom pEuronextVarExpectedShortfallCom)
        {
            EuronextVarExpectedShortfallsAssetNearExpiry ret = new EuronextVarExpectedShortfallsAssetNearExpiry();

            if (pEuronextVarExpectedShortfallCom != default)
            {
                EuronextVarExpectedShortfalls euronextVarExpectedShortfalls = BuildEuronextVarExpectedShortfalls(pEuronextVarExpectedShortfallCom);

                ret = new EuronextVarExpectedShortfallsAssetNearExpiry
                {
                    ISINCode = pISINCode,
                    ExpectedShortfall = euronextVarExpectedShortfalls.ExpectedShortfall,
                    Result = euronextVarExpectedShortfalls.Result,
                };
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMethodCalc"></param>
        /// <param name="pMethodComObj"></param>
        private static void BuildEuronextVarAssetIncomplete(EuronextVarCalculationMethod pMethodCalc, EuronextVarCalcMethCom pMethodComObj)
        {
            if (pMethodComObj.AssetIncomplet != default && ArrFunc.Count(pMethodComObj.AssetIncomplet) > 0)
            {
                pMethodCalc.AssetIncomplete = new EuronextVarAssetIncomplet[pMethodComObj.AssetIncomplet.Length];
                for (int i = 0; i < pMethodComObj.AssetIncomplet.Length; i++)
                {
                    pMethodCalc.AssetIncomplete[i] = new EuronextVarAssetIncomplet
                    {
                        IdAsset = pMethodComObj.AssetIncomplet[i].Asset.Item1,
                        ISINCode = pMethodComObj.AssetIncomplet[i].Asset.Item2,
                        OrdinaryScenarioNumber = pMethodComObj.AssetIncomplet[i].NbScenarioTypeS,
                        ScaledScenarioNumber = pMethodComObj.AssetIncomplet[i].NbScenarioTypeU
                    };
                }
            }
        }
    }
}

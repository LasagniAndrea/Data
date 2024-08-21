using System;
using System.Linq;

using EFS.GUI.Interface;

using EFS.SpheresRiskPerformance.CommunicationObjects;

using EfsML.v30.MarginRequirement;

namespace EFS.SpheresRiskPerformance.CalculationSheet
{
    /// <summary>
    /// CalculationSheet repository, containing the results and the calculation details of a deposit
    /// </summary>
    /// <remarks>this part of the class includes the members building the calculation sheet for the "TIMS IDEM/EUREX" method</remarks>
    public sealed partial class CalculationSheetRepository
    {

        private TimsDecomposableParameter BuildSpreadMarginReport(TimsDecomposableParameterCommunicationObject pSpreadComObj)
        {
            if (pSpreadComObj == null)
            {
                return null;
            }

            TimsDecomposableParameter spread = new TimsDecomposableParameter
            {
                MarginAmount = MarginMoney.FromMoney(pSpreadComObj.MarginAmount)
            };

            if (pSpreadComObj.Factors != null)
            {

                spread.Factors = (
                    from factorComObj in pSpreadComObj.Factors
                    select new TimsFactor
                    {
                        Id = factorComObj.AssetId.HasValue ? this.AssetETDCache[factorComObj.AssetId.Value].Identifier : factorComObj.Identifier,
                        Quote = factorComObj.Quote.HasValue ? new EFS_Decimal(factorComObj.Quote.Value) : null,
                        Qty = factorComObj.Quantity > 0 ?
                            new FixQuantity { Long = System.Math.Abs(factorComObj.Quantity.Value) } :
                            new FixQuantity { Short = System.Math.Abs(factorComObj.Quantity.Value) },
                        SpotMonth = factorComObj.SpotMonth.HasValue && factorComObj.SpotMonth.Value,
                        SpotMonthSpecified = factorComObj.SpotMonth.HasValue && factorComObj.SpotMonth.Value,

                        //MarginAmount = (Money)factorComObj.MarginAmount
                        MarginAmount = MarginMoney.FromMoney(factorComObj.MarginAmount),

                    }).ToArray();
            }

            return spread;
        }

        private TimsDecomposableParameter BuildMtmMarginReport(TimsDecomposableParameterCommunicationObject pMtmComObj)
        {
            if (pMtmComObj == null)
            {
                return null;
            }

            TimsDecomposableParameter mtm = new TimsDecomposableParameter
            {
                MarginAmount = MarginMoney.FromMoney(pMtmComObj.MarginAmount)
            };

            if (pMtmComObj.Factors != null)
            {

                mtm.Factors = (
                    from factorComObj in pMtmComObj.Factors
                    select new TimsFactor
                    {
                        Id = factorComObj.AssetId.HasValue ? this.AssetETDCache[factorComObj.AssetId.Value].Identifier : factorComObj.Identifier,
                        Quote = new EFS_Decimal(factorComObj.Quote.Value),
                        Qty = factorComObj.Quantity > 0 ?
                            new FixQuantity { Short = System.Math.Abs(factorComObj.Quantity.Value), Typ = factorComObj.PosType.Value} :
                            new FixQuantity { Long = System.Math.Abs(factorComObj.Quantity.Value), Typ = factorComObj.PosType.Value },

                        //MarginAmount = (Money)factorComObj.MarginAmount
                        MarginAmount = MarginMoney.FromMoney(factorComObj.MarginAmount),

                    }).ToArray();
            }

            return mtm;
        }

        private TimsDecomposableParameter BuildPremiumMarginReport(
            TimsDecomposableParameterCommunicationObject pPremiumComObj,
            bool pShowFactors)
        {
            if (pPremiumComObj == null)
            {
                return null;
            }

            TimsDecomposableParameter premium = new TimsDecomposableParameter
            {
                MarginAmount = MarginMoney.FromMoney(pPremiumComObj.MarginAmount)
            };

            if (pPremiumComObj.Factors != null && pShowFactors)
            {

                premium.Factors = (
                    from factorComObj in pPremiumComObj.Factors
                    select new TimsFactor
                    {
                        Id = this.AssetETDCache[factorComObj.AssetId.Value].Identifier,
                        Multiplier = factorComObj.Multiplier ?? 0,
                        MultiplierSpecified = factorComObj.Multiplier.HasValue,
                        Quote = new EFS_Decimal(factorComObj.Quote.Value),
                        StrkPx = factorComObj.StrikePrice.HasValue ? new EFS_Decimal(factorComObj.StrikePrice.Value) : null,
                        Qty = factorComObj.Quantity > 0 ?
                            new FixQuantity { Short = System.Math.Abs(factorComObj.Quantity.Value), Typ = factorComObj.PosType.Value } :
                            new FixQuantity { Long = System.Math.Abs(factorComObj.Quantity.Value), Typ = factorComObj.PosType.Value },

                        MarginAmount = MarginMoney.FromMoney(factorComObj.MarginAmount),

                    }).ToArray();
            }

            return premium;
        }

        private TimsDecomposableParameter BuildPremiumMarginReport(TimsDecomposableParameterCommunicationObject pPremiumComObj)
        {
            return BuildPremiumMarginReport(pPremiumComObj, true);
        }

        /// <summary>
        /// Build the additional factors objects for the calculation sheet
        /// </summary>
        /// <param name="pAdditionalComObj">the communication object built during the method execution</param>
        /// <param name="pShowFactors">if true all the factors will be shown, otherwiste just the LASt factor will be shown</param>
        /// <returns>the calculation sheet object with all the additional calculation details</returns>
        private TimsDecomposableParameter BuildAdditionalMarginReport(TimsDecomposableParameterCommunicationObject pAdditionalComObj,
            bool pShowFactors)
        {
            if (pAdditionalComObj == null)
            {
                return null;
            }

            TimsDecomposableParameter additional = new TimsDecomposableParameter
            {
                MarginAmount = MarginMoney.FromMoney(pAdditionalComObj.MarginAmount),
                CurrencyFrom = pAdditionalComObj.CurrencyFrom
            };

            if (pAdditionalComObj.Factors != null && pAdditionalComObj.Factors.Count() > 0)
            {
                if (!pShowFactors)
                    // reduce the factors list to the last one
                {
                    pAdditionalComObj.Factors = new TimsFactorCommunicationObject[] { pAdditionalComObj.Factors.Last() };
                }
               
                additional.Factors = (
                    from factorComObj in pAdditionalComObj.Factors
                    select new TimsFactor
                    {
                        Id = factorComObj.AssetId.HasValue ? this.AssetETDCache[factorComObj.AssetId.Value].Identifier : factorComObj.Identifier,
                        Multiplier = factorComObj.Multiplier ?? 0,
                        MultiplierSpecified = factorComObj.Multiplier.HasValue,
                        Qty =
                            factorComObj.Quantity.HasValue ?
                                factorComObj.Quantity > 0 ?
                                    new FixQuantity { Short = System.Math.Abs(factorComObj.Quantity.Value), Typ = factorComObj.PosType.Value }
                                    :
                                    new FixQuantity { Long = System.Math.Abs(factorComObj.Quantity.Value), Typ = factorComObj.PosType.Value }
                                :
                                null,

                        RiskArray = factorComObj.RiskArray != null ? (
                                from sidePoint in factorComObj.RiskArray
                                select new TimsFactor
                                {
                                    Id = sidePoint.Identifier,
                                    ShortAdj = sidePoint.ShortAdj.HasValue && sidePoint.ShortAdj.Value,
                                    ShortAdjSpecified = sidePoint.ShortAdj.HasValue && sidePoint.ShortAdj.Value,
                                    Index = sidePoint.RiskArrayIndex ?? -1,
                                    IndexSpecified = sidePoint.RiskArrayIndex.HasValue,
                                    MaturityYearMonth = sidePoint.MaturityYearMonth.HasValue ?
                                        String.Format("{0:0000/00}", sidePoint.MaturityYearMonth.Value) : null,
                                    MaturityFactor = sidePoint.MaturityFactor.HasValue ?
                                        new EFS_Decimal(sidePoint.MaturityFactor.Value).Value : null,
                                    Quote = sidePoint.Quote.HasValue ? new EFS_Decimal(sidePoint.Quote.Value) : null,
                                    CompensatedQty =
                                        sidePoint.CompensatedQuantity.HasValue ?
                                        // compensated quantity is not null just for short position
                                            new FixQuantity
                                            {
                                                Short = System.Math.Abs(sidePoint.CompensatedQuantity.Value),
                                                Typ = FixML.Enum.PosType.XM
                                            }
                                            :
                                            null,

                                    //MarginAmount = (Money)sidePoint.MarginAmount,
                                    MarginAmount = MarginMoney.FromMoney(sidePoint.MarginAmount),
                                }
                                ).ToArray() :
                                null,

                        //MarginAmount = (Money)factorComObj.MarginAmount,
                        MarginAmount = MarginMoney.FromMoney(factorComObj.MarginAmount),

                    }).ToArray();
                
            }

            return additional;
        }

        private TimsDecomposableParameter BuildAdditionalMarginReport(TimsDecomposableParameterCommunicationObject pAdditionalComObj)
        {
            return BuildAdditionalMarginReport(pAdditionalComObj, true);
        }

        private TimsDecomposableParameter BuildMinimumMarginReport(TimsDecomposableParameterCommunicationObject pMinimumComObj)
        {
            TimsDecomposableParameter minimum = new TimsDecomposableParameter
            {
                MarginAmount = MarginMoney.FromMoney(pMinimumComObj.MarginAmount)
            };

            if (pMinimumComObj.Factors != null)
            {

                minimum.Factors = (
                    from factorComObj in pMinimumComObj.Factors
                    select new TimsFactor
                    {
                        Id = factorComObj.Identifier,
                        MinimumRate = new  EFS_Decimal(factorComObj.MinimumRate.Value),
                        Qty = factorComObj.Quantity > 0 ? 
                            new FixQuantity { Short = System.Math.Abs(factorComObj.Quantity.Value) } :
                            new FixQuantity { Long = System.Math.Abs(factorComObj.Quantity.Value) },

                        //MarginAmount = (Money)factorComObj.MarginAmount
                        MarginAmount = MarginMoney.FromMoney(factorComObj.MarginAmount),

                    }).ToArray();
            }

            return minimum;
        }
    }

}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Enum;

using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.Fix;

using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the "Custom" (ex Standard) Risk method
    /// </summary>
    public sealed class CustomMethod : BaseMethod
    {
        /// <summary>
        /// Parameters 
        /// </summary>
        List<ParameterCustom> m_Parameters;

        /// <summary>
        /// Assets/contracts infos
        /// </summary>
        IEnumerable<AssetCustom> m_Assets;

        /// <summary>
        /// No public builder, use the factory method inside the base class
        /// </summary>
        internal CustomMethod()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.Position;
        }

        #region override base accessors
        /// <summary>
        /// Returns the CUSTOM type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.Custom; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise les paramètres DTBUSINESS & SESSIONID</remarks>
        /// </summary>
        /// PM 20150512 [20575] Add QueryExistRiskParameter
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMASSET_ETD_{BuildTableId}_W
        protected override string QueryExistRiskParameter
        {
            get
            {
                string query;
                query = @"
                    select distinct 1
                      from dbo.QUOTE_ETD_H q
                     inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = q.IDASSET)
                     where (q.TIME = @DTBUSINESS) and (q.QUOTESIDE = 'OfficialClose') and (q.QUOTETIMING = 'Close')";
                return query;
            }
        }
        #endregion

        /// <summary>
        /// Initialize the Custom method, load the parameters for all the contracts in position
        /// </summary>
        /// <param name="pAssetETDCache">collection containing all the assets in position</param>
        /// <param name="pCS">connection string</param>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            // PM 20150512 [20575] Gestion DtRiskParameters
            DateTime dtBusiness = GetRiskParametersDate(pCS);
            
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // 1. get all the assets in position

                int[] assetsId = (from key in pAssetETDCache.Keys select key).ToArray();

                // 20120718 MF - Ticket 18004 - asset and contract vectors not used anymore

                if (assetsId.Length > 0)
                {
                    // 20120718 MF - Ticket 18004 - 
                    Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
                    //dbParametersValue.Add("SESSIONID", this.SessionId);

                    // 1.1 get the parameters for contracts and assets

                    GetAssets(connection, dbParametersValue);

                    // 2. get the custom parameters 

                    GetParameters(connection, dbParametersValue);

                    // 3. Get the quote prices for the contract parameters with expression type "Percentage"

                    GetQuotes(pCS);
                }
                else
                {
                    m_Parameters = new List<ParameterCustom>();

                    m_Assets = new List<AssetCustom>();
                }

            }

        }

        /// <summary>
        /// Reset the Assets and Parameters collections that contributed to evaluate the "Custom" risk amount
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            this.m_Parameters = null;

            this.m_Assets = null;
        }

        /// <summary>
        /// Evaluate a deposit item, according with the parameters of the CUSTOM method
        /// </summary>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">the book where the positions set has been registered</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">the positions to evaluate the partial amount for the current deposit item</param>
        /// <param name="opMethodComObj">output value containing all the datas to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository.BuildCustomMarginCalculationMethod"/>) 
        /// in order to build a margin calculation node (type of <see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>the partial amount for the current deposit item</returns>
        /// PM 20160404 [22116] Devient public
        /// FI 20160613 [22256] Add parameter pDepositHierarchyClass
        /// PM 20170313 [22833] Changement de type pour le paramètre pPositionsToEvaluate (=>  RiskData pRiskDataToEvaluate)
        //public override List<Money> EvaluateRiskElementSpecific(
        //    int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass, 
        //    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
        //    out IMarginCalculationMethodCommunicationObject opMethodComObj)
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> amounts = new List<Money>();
            CustomMarginCalculationMethodCommunicationObject calculationSheetMethodObj = new CustomMarginCalculationMethodCommunicationObject();

            if (pRiskDataToEvaluate != default(RiskData))
            {
                // PM 20170313 [22833] Prendre uniquement la position (à l'ancien format)
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                // 1. Group the positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)

                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionsByIdAsset =
                    PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);


                // 2. Build the calculation sheet business object (used to write the calculation sheet report)

                List<CustomContractParameterCommunicationObject> contractParameters = new List<CustomContractParameterCommunicationObject>();

                // 3. cycle on the loaded parameters collection

                // 1.1 Add the missing parameters and remove the unuseful ones

                IEnumerable<ParameterCustom> normalizedParameters = NormalizeParameters(groupedPositionsByIdAsset);

                foreach (ParameterCustom parameter in normalizedParameters)
                {
                    // 4. get the positions for the current contract by side (parameter.ContractId)


                    GetContractPositionsBySide(groupedPositionsByIdAsset, parameter,
                        out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> contractPositionsLong,
                        out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> contractPositionsShort);

                    // 5. Build the contract object (used to write the calculation sheet report)

                    CustomContractParameterCommunicationObject contractParameter = new CustomContractParameterCommunicationObject();
                    contractParameters.Add(contractParameter);

                    // 5.1 the contract information (identifier, ...) for the current parameter

                    AssetCustom contractInformations =
                        (from contract in m_Assets where contract.ContractId == parameter.ContractId select contract).First();

                    contractParameter.ContractId = parameter.ContractId;
                    contractParameter.Identifier = contractInformations.Contract;
                    contractParameter.ExpressionType = parameter.ExpressionType;
                    contractParameter.Missing = parameter.Missing;
                    contractParameter.Quote = 1;
                    contractParameter.Multiplier = 1;

                    if (contractParameter.ExpressionType == ExpressionType.Percentage)
                    {
                        contractParameter.Quote = parameter.Quote;
                        contractParameter.Multiplier = contractInformations.ContractMultiplier;
                    }

                    decimal percentageFactor = contractParameter.Quote * contractParameter.Multiplier;

                    contractParameter.Positions = contractPositionsLong.Union(contractPositionsShort);

                    contractParameter.MarginAmount = new Money(0, parameter.Currency);

                    // 6. get the parameters risk values compliant with the current contract category


                    GetContractParameterValues(
                        parameter, contractInformations.Category, parameter.ExpressionType,
                        out string[] parameterValueTypes, out decimal[] parameterValues);

                    List<CustomAmountParameterCommunicationObject> amountParameters = new List<CustomAmountParameterCommunicationObject>();

                    // 7. compute the amount value for each parameter risk value

                    for (int idxParameterValue = 0; idxParameterValue < parameterValueTypes.Length; idxParameterValue++)
                    {
                        string parameterValueType = parameterValueTypes[idxParameterValue];
                        decimal parameterValue = parameterValues[idxParameterValue];

                        if (parameterValue > 0)
                        {
                            // 8. Build the amount object (used to write the calculation sheet report)

                            CustomAmountParameterCommunicationObject amountParameter = new CustomAmountParameterCommunicationObject
                            {
                                RiskValue = parameterValue,
                                Type = parameterValueType
                            };
                            // EG 20150920 [21374] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            decimal quantityLong = 0;
                            decimal quantityShort = 0;
                            decimal quantityTot = 0;

                            Pair<PosRiskMarginKey, RiskMarginPosition> outputPositionLong = null;
                            Pair<PosRiskMarginKey, RiskMarginPosition> outputPositionShort = null;

                            // 9. Compute the amounts...

                            switch (parameterValueType)
                            {
                                // 9.1 ...compute the amount for net future positions (delta(long quantity - short quantity))
                                case "Normal":

                                    quantityLong = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Future, null, out outputPositionLong);
                                    quantityShort = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Future, null, out outputPositionShort);

                                    quantityTot = System.Math.Abs(quantityLong - quantityShort);

                                    amountParameter.MarginAmount =
                                        new Money(quantityTot * parameterValue * percentageFactor, parameter.Currency);

                                    if (quantityLong > quantityShort)
                                    {
                                        outputPositionLong.Second = new RiskMarginPosition() { Quantity = quantityTot };

                                        amountParameter.Positions = new[] { outputPositionLong };
                                    }
                                    else if (quantityLong < quantityShort)
                                    {
                                        outputPositionShort.Second = new RiskMarginPosition() { Quantity = quantityTot };

                                        amountParameter.Positions = new[] { outputPositionShort };
                                    }

                                    break;

                                // 9.2 ...compute the amount for straddle future positions (min(long quantity,short quantity))
                                case "Straddle":

                                    quantityLong = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Future, null, out outputPositionLong);
                                    quantityShort = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Future, null, out outputPositionShort);

                                    quantityTot = System.Math.Min(quantityLong, quantityShort);

                                    amountParameter.MarginAmount =
                                        new Money(quantityTot * parameterValue * percentageFactor, parameter.Currency);

                                    if (quantityTot > 0)
                                    {
                                        outputPositionLong.Second = new RiskMarginPosition() { Quantity = quantityTot };
                                        outputPositionShort.Second = new RiskMarginPosition() { Quantity = quantityTot };

                                        amountParameter.Positions = new[] { outputPositionLong, outputPositionShort };
                                    }

                                    break;

                                // 9.3 ...compute the amount for long positions over call option
                                case "LongCall":

                                    quantityTot = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Option, "1", out outputPositionLong);

                                    amountParameter.MarginAmount =
                                        new Money(quantityTot * parameterValue * percentageFactor, parameter.Currency);

                                    amountParameter.Positions = new[] { outputPositionLong };

                                    break;

                                // 9.4 ...compute the amount for long positions over put option
                                case "LongPut":

                                    quantityTot = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Option, "0", out outputPositionLong);

                                    amountParameter.MarginAmount =
                                        new Money(quantityTot * parameterValue * percentageFactor, parameter.Currency);

                                    amountParameter.Positions = new[] { outputPositionLong };

                                    break;

                                // 9.5 ...compute the amount for short positions over call option
                                case "ShortCall":

                                    quantityTot = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Option, "1", out outputPositionShort);

                                    amountParameter.MarginAmount =
                                        new Money(quantityTot * parameterValue * percentageFactor, parameter.Currency);

                                    amountParameter.Positions = new[] { outputPositionShort };

                                    break;

                                // 9.6 ...compute the amount for short positions over put option
                                case "ShortPut":

                                    quantityTot = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Option, "0", out outputPositionShort);

                                    amountParameter.MarginAmount =
                                        new Money(quantityTot * parameterValue * percentageFactor, parameter.Currency);

                                    amountParameter.Positions = new[] { outputPositionShort };

                                    break;
                            }

                            if (quantityTot > 0)
                            {
                                amountParameters.Add(amountParameter);

                                if (amountParameter.MarginAmount != null)
                                {
                                    contractParameter.MarginAmount.Amount.DecValue += amountParameter.MarginAmount.Amount.DecValue;
                                }
                            }


                        } // parameter risk value <= 0

                    } // loop parameter risk values

                    contractParameter.Parameters = amountParameters.ToArray();

                    SumAmounts(new Money[] { (Money)contractParameter.MarginAmount }, ref amounts);

                } // loop parameter

                calculationSheetMethodObj.Parameters = contractParameters.ToArray();
            }
            opMethodComObj = calculationSheetMethodObj;
            //PM 20150512 [20575] Ajout date des paramètres de risque
            opMethodComObj.DtParameters = DtRiskParameters;

            if (amounts.Count <= 0)
            {
                // no amount for the current deposit, inserting amount 0 using default currency EUR 
                //  (trade RISK requires at least an amount to be built)
                amounts.Add(new Money(0, "EUR"));
            }

            return amounts;
        }

        /// <summary>
        /// Not used, no coverage parameters can be computed for the custom method
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset"></param>
        /// <returns>a null collection</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return null;
        }
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private decimal GetQuantity(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions,
            CfiCodeCategoryEnum pContractCategory, string pPutOrCall, out Pair<PosRiskMarginKey, RiskMarginPosition> opPosition)
        {
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal quantity = 0;

            opPosition = null;

            switch (pContractCategory)
            {
                case CfiCodeCategoryEnum.Future:

                    quantity = (from position in pPositions select position.Second.Quantity).Sum();

                    break;

                case CfiCodeCategoryEnum.Option:

                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions =
                            from position in pPositions
                            join asset in m_Assets on position.First.idAsset equals asset.AssetId
                            where asset.PutOrCall == pPutOrCall
                            select position;

                    quantity = (from position in positions select position.Second.Quantity).Sum();

                    break;
            }

            if (quantity > 0)
            {
                opPosition =
                    new Pair<PosRiskMarginKey, RiskMarginPosition>
                    {
                       First = new PosRiskMarginKey() { Side = pPositions.First().First.Side },
                       Second = new RiskMarginPosition() { Quantity = quantity }
                    };
            }

            return quantity;
        }

        private void GetParameters(IDbConnection pConnection, Dictionary<string, object> pDbParametersValue)
        {
            string parametersRequest = DataContractHelper.GetQuery(DataContractResultSets.PARAMS_CUSTOMMETHOD);

            // 1. Loading parameters from the database for all the ETD contracts in position

            m_Parameters =
                DataHelper<ParameterCustom>.ExecuteDataSet(pConnection,
                DataContractHelper.GetType(DataContractResultSets.PARAMS_CUSTOMMETHOD),
                parametersRequest, 
                DataContractHelper.GetDbDataParameters(DataContractResultSets.PARAMS_CUSTOMMETHOD, pDbParametersValue));

            // 2. Completing parameters mandatory informations when missing

            foreach (ParameterCustom parameter in m_Parameters)
            {
                if (String.IsNullOrEmpty(parameter.Currency))
                {
                    parameter.Currency = GetCurrencyFromAssetsCollection(parameter.ContractId);
                }
            }
        }

        private void GetAssets(IDbConnection connection, Dictionary<string, object> pDbParametersValue)
        {
            string derivativeContractRequest = DataContractHelper.GetQuery(DataContractResultSets.CONTRACTASSET_CUSTOMMETHOD);

            m_Assets =
                DataHelper<AssetCustom>.ExecuteDataSet(connection,
                DataContractHelper.GetType(DataContractResultSets.CONTRACTASSET_CUSTOMMETHOD),
                derivativeContractRequest,
                DataContractHelper.GetDbDataParameters(DataContractResultSets.CONTRACTASSET_CUSTOMMETHOD, pDbParametersValue));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        private void GetQuotes(string pCS)
        {
            IEnumerable<ParameterCustom> parametersExpressionPercentage =
                from parameter
                    in m_Parameters
                where parameter.ExpressionType == ExpressionType.Percentage
                select parameter;

            ExchangeTradedDerivative unkProduct = new ExchangeTradedDerivative();

            foreach (ParameterCustom parameter in parametersExpressionPercentage)
            {
                AssetCustom contractInformations =
                    (from contract in m_Assets where contract.ContractId == parameter.ContractId select contract).First();

                Cst.UnderlyingAsset categoryUnderlyer;

                int idAsset;

                switch (contractInformations.Category)
                {
                    case "F":

                        categoryUnderlyer = Cst.UnderlyingAsset.Future;
                        idAsset = PosKeepingTools.GetIdAssetNextMaturity(pCS, contractInformations.ContractId, this.DtBusiness);
                        break;

                    case "O":
                    default:

                        categoryUnderlyer = Cst.ConvertToUnderlyingAsset(contractInformations.CategoryUnderlyer);
                        switch (categoryUnderlyer)
                        {
                            case Cst.UnderlyingAsset.Future:

                                idAsset = PosKeepingTools.GetIdAssetNextMaturity(pCS, contractInformations.ContractIdUnderlyer, this.DtBusiness);
                                break;

                            default:

                                idAsset = contractInformations.IdUnderlyer;
                                break;
                        }

                        break;
                }


                // PM 20150512 [20575] Utilisation de DtRiskParameters à la place de DtBusiness
                parameter.Quote =
                    GetQuote(unkProduct, pCS, idAsset, categoryUnderlyer, this.DtRiskParameters,
                    out string idMarketEnv, out string idValScenario, out DateTime adjustedTime, out string quoteSide, out string quoteTiming, out SystemMSGInfo systemMsgInfo);
            }
        }

        private IEnumerable<ParameterCustom> NormalizeParameters(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            IEnumerable<int> idContractsInPosition = (
                from position in pGroupedPositionsByIdAsset
                join asset in m_Assets on position.First.idAsset equals asset.AssetId
                select asset.ContractId
                ).Distinct();

            IEnumerable<ParameterCustom> normalizedParameters = (
                from parameter in m_Parameters
                join idContract in idContractsInPosition on parameter.ContractId equals idContract
                select parameter
                ).ToList();

            IEnumerable<int> idContractsParameters =
                from parameter in normalizedParameters
                select parameter.ContractId;

            IEnumerable<ParameterCustom> addedParameters =
                from idContract in idContractsInPosition
                where !idContractsParameters.Contains(idContract)
                select 
                    new ParameterCustom
                    {
                        ContractId = idContract,
                        Currency = GetCurrencyFromAssetsCollection(idContract),
                        Missing = true
                    };

            normalizedParameters = normalizedParameters.Union(addedParameters); 

            return normalizedParameters;
        }

        private string GetCurrencyFromAssetsCollection(int pIdContract)
        {
            return
                (from assetParameter in this.m_Assets
                 where assetParameter.ContractId == pIdContract
                 select assetParameter)
                             .First().Currency;
        }

        private void GetContractPositionsBySide(
           IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset, ParameterCustom pParameter,
           out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositionsLong,
           out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositionsShort)
        {
            // 1. Get the positions of the contract parameter

            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> contractPositions =
                from position in pGroupedPositionsByIdAsset
                join asset in m_Assets on position.First.idAsset equals asset.AssetId
                where asset.ContractId == pParameter.ContractId
                select position;

            // 2. get the long positions for the current contract 

            pContractPositionsLong =
                from position in contractPositions
                join asset in m_Assets on position.First.idAsset equals asset.AssetId
                where position.First.Side == "1"
                select position;

            // 3. get the long positions for the current contract 

            pContractPositionsShort =
                from position in contractPositions
                join asset in m_Assets on position.First.idAsset equals asset.AssetId
                where position.First.Side == "2"
                select position;

        }

        private static void GetContractParameterValues(
            ParameterCustom pParameter, string pContractCategory, ExpressionType pExpressionType,
            out string[] opStdValueTypes, out decimal[] opStdValues)
        {

            decimal percentageFactor = 1;

            if (pExpressionType == ExpressionType.Percentage)
            {
                percentageFactor = 100;
            }

            switch (pContractCategory)
            {
                case "F":

                    opStdValueTypes = new string[] 
                        { 
                            "Normal", 
                            "Straddle"
                        };

                    opStdValues = new decimal[] 
                        { 
                            pParameter.FutureNormalMultiplier / percentageFactor, 
                            pParameter.FutureStraddleMultiplier / percentageFactor,
                        };

                    break;

                case "O":
                default:

                    opStdValueTypes = new string[] 
                        { 
                            "LongCall",
                            "ShortCall",
                            "LongPut",
                            "ShortPut"
                        };

                    opStdValues = new decimal[] 
                        { 
                            pParameter.LongCallMultiplier / percentageFactor,
                            pParameter.ShortCallMultiplier / percentageFactor,
                            pParameter.LongPutMultiplier / percentageFactor,
                            pParameter.ShortPutMultiplier / percentageFactor
                        };

                    break;
            }
        }

    }
}

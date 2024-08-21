using System.Collections.Generic;
using System.Linq;
using System.Data;

using EFS.EFSTools;
using EFS.Common;
using EFS.ApplicationBlocks.Data;

using FpML.v44.Shared;

using FixML.Enum;
using EfsML.v30.MarginRequirement;
using System;
using EfsML.Interface;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EfsML.Enum;
using EfsML.Business;
using EFS.ACommon;
using FpML.Enum;
using EfsML.v30.Fix;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the "Standard" Risk method
    /// </summary>
    public sealed class StandardMethod : BaseMethod
    {
        /// <summary>
        /// Returns the STANDARD type
        /// </summary>
        public override RiskMethodType Type
        {
            get { return RiskMethodType.STANDARD; }
        }

        /// <summary>
        /// Parameters by contract, the dictionary key is the contract id
        /// </summary>
        List<ParameterStandardMethod> m_Parameters;

        /// <summary>
        /// Asset contracts infos, the dictionary key is the asset id
        /// </summary>
        IEnumerable<DerivativeContractStandardMethod> m_Contracts;

        /// <summary>
        /// No public builder, use the factory method inside the base class
        /// </summary>
        internal StandardMethod()
        { }

        /// <summary>
        /// Initialize the standard method, load the parameters of the contract in position
        /// </summary>
        /// <returns></returns>
        public override void LoadParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // 1. get all the contracts for all the loaded series

                int[] assetsId = (from key in pAssetETDCache.Keys select key).ToArray();

                object[] boxedId = new object[assetsId.Length];
                assetsId.CopyTo(boxedId, 0);

                object[][] valuesMatrix = { boxedId };

                GetContracts(connection, valuesMatrix);

                // 2. get the parameters for the contracts

                int[] contractsId = (from value in m_Contracts select value.ContractId).Distinct().ToArray();

                boxedId = new object[contractsId.Length];
                contractsId.CopyTo(boxedId, 0);

                valuesMatrix[0] = boxedId;

                GetParametersByContract(connection, valuesMatrix);

                // 3. Get the quote prices for the contract parameters with expression type "Percentage"

                GetQuotes(pCS);

                //pAssetETDCache[0].ContractMultiplier;

            }

        }

        /// <summary>
        /// Evaluate a risk elelement constituting one deposit item, according with the parameters of the standard method
        /// </summary>
        /// <returns></returns>
        public override List<Money> EvaluateRiskElement(
            int pActorId, int pBookId, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
            out ICalculationSheetMethod opCalculationMarginMethod)
        {

            List<Money> amounts = new List<Money>();

            // 1. Group the positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)

            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionsByIdAsset = 
                GroupPositionsByAsset(pPositionsToEvaluate);

            // 2. Build the calculation sheet business object (used to write the calculation sheet report)

            StdCalculationSheetMethod calculationSheetMethodObj = new StdCalculationSheetMethod();
            calculationSheetMethodObj.Parameters = new StdCalculationSheetRiskContractParameter[m_Parameters.Count];

            int idxParameter = 0;

            // 3. cycle on the loaded parameters collection

            foreach (ParameterStandardMethod parameter in m_Parameters)
            {
                // 4. get the positions for the current contract by side (parameter.ContractId)

                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> contractPositionsLong;
                IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> contractPositionsShort;

                GetContractPositionsBySide(groupedPositionsByIdAsset, parameter, out contractPositionsLong, out contractPositionsShort);

                // 5. Build the contract object (used to write the calculation sheet report)

                StdCalculationSheetRiskContractParameter contractParameter = new StdCalculationSheetRiskContractParameter();
                calculationSheetMethodObj.Parameters[idxParameter] = contractParameter;

                // 5.1 ge the contract information (identifier, ...) for the current parameter

                DerivativeContractStandardMethod contractInformations =
                    (from contract in m_Contracts where contract.ContractId == parameter.ContractId select contract).First();

                contractParameter.ContractId = parameter.ContractId;
                contractParameter.Identifier = contractInformations.Contract;
                contractParameter.ExpressionType = parameter.ExpressionType;
                contractParameter.Quote = 1;
                contractParameter.Multiplier = 1;

                if (contractParameter.ExpressionType == ExpressionTypeStandardMethod.Percentage)
                {
                    contractParameter.Quote = parameter.Quote;
                    // UNDONE MF
                    contractParameter.Multiplier = contractInformations.ContractMultiplier;
                }

                decimal percentageFactor = contractParameter.Quote * contractParameter.Multiplier;

                contractParameter.Positions = contractPositionsLong.Union(contractPositionsShort);

                contractParameter.MarginAmount = new Money(0, "EUR");

                // 6. get the parameters risk values compliant with the current contract category

                string[] parameterValueTypes;
                decimal[] parameterValues;

                GetContractParameterValues(
                    parameter, contractInformations.Category, parameter.ExpressionType, 
                    out parameterValueTypes, out parameterValues);

                List<StdCalculationSheetRiskAmountParameter> amountParameters = new List<StdCalculationSheetRiskAmountParameter>();

                // 7. compute the amount value for each parameter risk value

                for (int idxParameterValue = 0; idxParameterValue < parameterValueTypes.Length; idxParameterValue++)
                {
                    string parameterValueType = parameterValueTypes[idxParameterValue];
                    decimal parameterValue = parameterValues[idxParameterValue];

                    if (parameterValue > 0)
                    {
                        // 8. Build the amount object (used to write the calculation sheet report)

                        StdCalculationSheetRiskAmountParameter amountParameter = new StdCalculationSheetRiskAmountParameter();
                        amountParameters.Add(amountParameter);

                        amountParameter.RiskValue = parameterValue;
                        amountParameter.Type = parameterValueType;

                        int quantityLong = 0;
                        int quantityShort = 0;

                        IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> outputPositions;

                        // 9. Compute the amounts...

                        switch (parameterValueType)
                        {
                            // 9.1 ...compute the amount for net future positions (delta(long quantity - short quantity))
                            case "Normal":

                                quantityLong = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Future, null, out outputPositions);
                                quantityShort = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Future, null, out outputPositions);

                                amountParameter.MarginAmount =
                                    new Money(System.Math.Abs(quantityLong - quantityShort) * parameterValue * percentageFactor, "EUR");

                                amountParameter.Positions = contractPositionsLong.Union(contractPositionsShort);

                                break;

                            // 9.2 ...compute the amount for straddle future positions (min(long quantity,short quantity))
                            case "Straddle":

                                quantityLong = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Future, null, out outputPositions);
                                quantityShort = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Future, null, out outputPositions);

                                amountParameter.MarginAmount =
                                    new Money(System.Math.Min(quantityLong, quantityShort) * parameterValue * percentageFactor, "EUR");

                                amountParameter.Positions = contractPositionsLong.Union(contractPositionsShort);

                                break;

                            // 9.3 ...compute the amount for long positions over call option
                            case "LongCall":

                                int quantityLongCall = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Option, "1", out outputPositions);

                                amountParameter.MarginAmount =
                                    new Money(quantityLongCall * parameterValue * percentageFactor, "EUR");

                                amountParameter.Positions = outputPositions;

                                break;

                            // 9.4 ...compute the amount for long positions over put option
                            case "LongPut":

                                int quantityLongPut = GetQuantity(contractPositionsLong, CfiCodeCategoryEnum.Option, "0", out outputPositions);

                                amountParameter.MarginAmount =
                                    new Money(quantityLongPut * parameterValue * percentageFactor, "EUR");

                                amountParameter.Positions = outputPositions;

                                break;

                            // 9.5 ...compute the amount for short positions over call option
                            case "ShortCall":

                                int quantityShortCall = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Option, "1", out outputPositions);

                                amountParameter.MarginAmount =
                                    new Money(quantityShortCall * parameterValue * percentageFactor, "EUR");

                                amountParameter.Positions = outputPositions;

                                break;

                            // 9.6 ...compute the amount for short positions over put option
                            case "ShortPut":

                                int quantityShortPut = GetQuantity(contractPositionsShort, CfiCodeCategoryEnum.Option, "0", out outputPositions);

                                amountParameter.MarginAmount =
                                    new Money(quantityShortPut * parameterValue * percentageFactor, "EUR");

                                amountParameter.Positions = outputPositions;

                                break;
                        }

                        contractParameter.MarginAmount.amount.DecValue += amountParameter.MarginAmount.amount.DecValue;

                    } // parameter risk value <= 0

                } // loop parameter risk values

                contractParameter.Parameters = amountParameters.ToArray();

                SumAmounts(new Money[] { (Money)contractParameter.MarginAmount }, ref amounts);

                idxParameter++;

            } // loop parameter

            opCalculationMarginMethod = calculationSheetMethodObj;

            return amounts;
        }

        private int GetQuantity(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions,
            CfiCodeCategoryEnum pContractCategory, string pPutOrCall, out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> opPositions)
        {
            int quantity = 0;
            opPositions = null;

            switch (pContractCategory)
            {
                case CfiCodeCategoryEnum.Future:

                    quantity = (from position in pPositions select position.Second.Quantity).Sum();

                    break;

                case CfiCodeCategoryEnum.Option:

                    opPositions =
                            from position in pPositions
                            join asset in m_Contracts on position.First.idAsset equals asset.AssetId
                            where asset.PutOrCall == pPutOrCall
                            select position;

                    quantity = (from position in opPositions select position.Second.Quantity).Sum();

                    break;
            }

            return quantity;
        }

        private void GetParametersByContract(IDbConnection pConnection, object[][] pValuesMatrix)
        {
            string parametersRequest = DataContractHelper.GetQuery(DataContractResultSets.PARAMSSTANDARDMETHOD, pValuesMatrix);

            m_Parameters =
                DataHelper<ParameterStandardMethod>.ExecuteDataSet(pConnection,
                DataContractHelper.GetType(DataContractResultSets.PARAMSSTANDARDMETHOD),
                parametersRequest);
        }

        private void GetContracts(IDbConnection connection, object[][] valuesMatrix)
        {
            string derivativeContractRequest = DataContractHelper.GetQuery(DataContractResultSets.DERIVATIVECONTRACTBYASSET, valuesMatrix);

            m_Contracts =
                DataHelper<DerivativeContractStandardMethod>.ExecuteDataSet(connection,
                DataContractHelper.GetType(DataContractResultSets.DERIVATIVECONTRACTBYASSET),
                derivativeContractRequest);
        }

        private void GetQuotes(string pCS)
        {
            ExchangeTradedDerivative unkProduct = new ExchangeTradedDerivative();

            IEnumerable<ParameterStandardMethod> parametersExpressionPercentage =
                from parameter
                    in m_Parameters
                where parameter.ExpressionType == ExpressionTypeStandardMethod.Percentage
                select parameter;

            foreach (ParameterStandardMethod parameter in parametersExpressionPercentage)
            {
                DerivativeContractStandardMethod contractInformations =
                    (from contract in m_Contracts where contract.ContractId == parameter.ContractId select contract).First();

                Cst.UnderlyingAsset categoryUnderlyer;

                switch (contractInformations.Category)
                {
                    case "F":

                        categoryUnderlyer = Cst.UnderlyingAsset.Future;
                        break;

                    case "O":
                    default:

                        categoryUnderlyer = PosKeepingTools.ConvertToAssetCategoryEnum(contractInformations.CategoryUnderlyer);
                        break;
                }

                int idUnderlyer;

                switch (categoryUnderlyer)
                {
                    case Cst.UnderlyingAsset.Future:

                        idUnderlyer = PosKeepingTools_Ext.GetIdAssetNextMaturity(pCS, parameter.ContractId, this.DtBusiness);
                        break;

                    default:

                        idUnderlyer = contractInformations.IdUnderlyer;
                        break;
                }

                SQL_Quote quote = new SQL_Quote(
                    pCS,
                    PosKeepingTools.ConvertToQuoteEnum(categoryUnderlyer),
                    AvailabilityEnum.NA,
                    unkProduct,
                    new KeyQuote(
                        pCS, this.DtBusiness, null, null, QuotationSideEnum.Mid),
                    idUnderlyer);

                if (quote.IsLoaded)
                {
                    parameter.Quote = quote.QuoteValue;
                }
            }
        }

        private static IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> 
            GroupPositionsByAsset(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            // group the positions by asset (the side of the new merged assets will be set with regards to the long and short quantities)

            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> groupedPositionsByIdAsset =

                from positionsGroup

                    in (pPositions

                    .GroupBy(position => position.First.idAsset))

                select

                    new Pair<PosRiskMarginKey, RiskMarginPosition>
                    {
                        First =
                        new PosRiskMarginKey
                        {
                            idI = 0,
                            idAsset = positionsGroup.Key,



                            side = (
                                (from position in positionsGroup where position.First.side == "1" select position.Second.Quantity).Sum()
                                 -
                                (from position in positionsGroup where position.First.side == "2" select position.Second.Quantity).Sum() 

                                > 0) ? "1" : "2",

                            idA_Dealer = 0,
                            idB_Dealer = 0,
                            idA_Clearer = 0,
                            idB_Clearer = 0,
                            idA_EntityDealer = 0,
                            idA_EntityClearer = 0
                        },

                        Second =
                        new RiskMarginPosition
                        {
                            TradeIds = (
                                from position
                                    in positionsGroup
                                from tradeId in position.Second.TradeIds
                                select tradeId).ToArray(),

                            Quantity = 
                            (int)System.Math.Abs(
                                (from position in positionsGroup where position.First.side == "1" select position.Second.Quantity).Sum()
                                -
                                (from position in positionsGroup where position.First.side == "2" select position.Second.Quantity).Sum())
                        }

                    };

            return groupedPositionsByIdAsset;
        }

        private void GetContractPositionsBySide(
           IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset, ParameterStandardMethod pParameter,
           out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositionsLong,
           out IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pContractPositionsShort)
        {
            // 1. Get the positions of the contract parameter
            
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> contractPositions =
                from position in pGroupedPositionsByIdAsset
                join asset in m_Contracts on position.First.idAsset equals asset.AssetId
                where asset.ContractId == pParameter.ContractId
                select position;

            // 2. get the long positions for the current contract 

            pContractPositionsLong =
                from position in contractPositions
                join asset in m_Contracts on position.First.idAsset equals asset.AssetId
                where position.First.side == "1"
                select position;

            // 3. get the long positions for the current contract 

            pContractPositionsShort =
                from position in contractPositions
                join asset in m_Contracts on position.First.idAsset equals asset.AssetId
                where position.First.side == "2"
                select position;
        }

        private static void GetContractParameterValues(
            ParameterStandardMethod pParameter, string pContractCategory, ExpressionTypeStandardMethod pExpressionType,
            out string[] opStdValueTypes, out decimal[] opStdValues)
        {

            decimal percentageFactor = 1;

            if (pExpressionType == ExpressionTypeStandardMethod.Percentage)
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

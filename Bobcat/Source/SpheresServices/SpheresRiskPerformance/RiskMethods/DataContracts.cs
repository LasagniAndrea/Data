using System;
using System.Runtime.Serialization;

using EFS.ApplicationBlocks.Data;

using FixML.Enum;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the point of view of a derivative contract with regards to the standard method
    /// </summary>
    [DataContract( Name = DataHelper<DerivativeContractStandardMethod>.DATASETROWNAME, 
        Namespace = DataHelper<DerivativeContractStandardMethod>.DATASETNAMESPACE)]
    internal sealed class DerivativeContractStandardMethod
    {
        /// <summary>
        /// Derivative Contract identifier
        /// </summary>
        [DataMember(Name = "CONTRACT", Order = 1)]
        public string Contract
        { get; set; }

        /// <summary>
        /// Derivative contract internal id
        /// </summary>
        [DataMember(Name = "CONTRACTID", Order = 2)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Derivative contract category
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 3)]
        public string Category
        { get; set; }

        /// <summary>
        /// Currency identifier of the derivative contract price
        /// </summary>
        [DataMember(Name = "CURRENCY", Order = 4)]
        public string Currency
        { get; set; }

        /// <summary>
        /// PutCall info (null for dc futures)
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag201.html</remarks>
        [DataMember(Name = "PUTCALL", Order = 5)]
        public string PutOrCall
        { get; set; }

        /// <summary>
        /// Id of the asset
        /// </summary>
        [DataMember(Name = "ASSETID", Order = 6)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// Category of the underlying asset (for contracts option only)
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag167.html</remarks>
        [DataMember(Name = "UNDERLYERCATEGORY", Order = 7)]
        public string CategoryUnderlyer
        { get; set; }

        /// <summary>
        /// id of the underlying asset (for contracts option only)
        /// </summary>
        [DataMember(Name = "UNDERLYERID", Order = 8)]
        public int IdUnderlyer
        { get; set; }

        /// <summary>
        /// Contract mulitplier of the asset quote, parameters using value expression type "percentage"
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 9)]
        public decimal ContractMultiplier
        { get; set; }
    }
    
    /// <summary>
    /// Parameters containing a parameters set entry for the standard method
    /// </summary>
    [DataContract(Name = DataHelper<ParameterStandardMethod>.DATASETROWNAME,
        Namespace = DataHelper<ParameterStandardMethod>.DATASETNAMESPACE)]
    internal sealed class ParameterStandardMethod
    {
        /// <summary>
        /// Derivative contract internal id
        /// </summary>
        [DataMember(Name = "CONTRACTID", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Starting parameter validity date
        /// </summary>
        [DataMember(Name = "PARAM_DTEN", Order = 2)]
        public DateTime ParameterEnabledFrom
        { get; set; }

        /// <summary>
        /// Ending parameter validity date
        /// </summary>
        [DataMember(Name = "PARAM_DTDIS", Order = 3)]
        public DateTime ParameteDisabledFrom
        { get; set; }

        /// <summary>
        /// Expression type for the multipliers 
        /// </summary>
        [DataMember(Name = "EXPRESSIONTYPE", Order = 4)]
        public ExpressionTypeStandardMethod ExpressionType
        { get; set; }

        /// <summary>
        /// Multiplier for future straddles
        /// </summary>
        [DataMember(Name = "IMSTRADDLE", Order = 5)]
        public decimal FutureStraddleMultiplier
        { get; set; }

        /// <summary>
        /// Multiplier for futures or in long or in short position 
        /// </summary>
        [DataMember(Name = "IMNORMAL", Order = 6)]
        public decimal FutureNormalMultiplier
        { get; set; }

        /// <summary>
        /// Multiplier for call options in long position
        /// </summary>
        [DataMember(Name = "IMLONGCALL", Order = 7)]
        public decimal LongCallMultiplier
        { get; set; }

        /// <summary>
        /// Multiplier for put options in long position
        /// </summary>
        [DataMember(Name = "IMLONGPUT", Order = 8)]
        public decimal LongPutMultiplier
        { get; set; }

        /// <summary>
        /// Multiplier for call options in short position
        /// </summary>
        [DataMember(Name = "IMSHORTCALL", Order = 9)]
        public decimal ShortCallMultiplier
        { get; set; }

        /// <summary>
        /// Multiplier for put options in short position
        /// </summary>
        [DataMember(Name = "IMSHORTPUT", Order = 10)]
        public decimal ShortPutMultiplier
        { get; set; }

        /// <summary>
        /// Underlyer quote, parameters using value expression type "percentage"
        /// </summary>
        public decimal Quote
        { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.EFSTools;

using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.Fix;

using FpML.Enum;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class representing the OMX Nordic Risk method
    /// </summary>
    public sealed class RiskMethodRCaR : BaseMethod
    {
        /// <summary>
        /// Returns the OMX_NORDIC type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.OMX_RCAR; }
        }


        /// <summary>
        /// No public builder, use the factory method inside the base class
        /// </summary>
        internal RiskMethodRCaR()
        { }

        /// <summary>
        /// Initialize the Custom method, load the parameters for all the contracts in position
        /// </summary>
        /// <param name="pAssetETDCache">collection containing all the assets in position</param>
        /// <param name="pCS">connection string</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            throw new Exception("not implemented");
        }

        /// <summary>
        /// Reset the Assets and Parameters collections that contributed to evaluate the "OMX Nordic Risk method" risk amount
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            throw new Exception("not implemented");
        }

        /// <summary>
        /// Evaluate a deposit item, according with the parameters of the OMX Nordic method
        /// </summary>
        /// <param name="opMethodComObj">output value containing all the datas to pass to the calculation sheet repository object
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository.BuildCustomMarginCalculationMethod"/>) 
        /// in order to build a margin calculation node (type of <see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <param name="pActorId">the actor owning the positions set</param>
        /// <param name="pBookId">the book where the positions set has been registered</param>
        /// <param name="pPositionsToEvaluate">the positions to evaluate the partial amount for the current deposit item</param>
        /// <returns>the partial amount for the current deposit item</returns>
        protected override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositionsToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            throw new Exception("not implemented");
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset"></param>
        /// <returns>a null collection</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEntityMarkets"></param>
        public override void BuildMarketParameters(IEnumerable<EFS.Spheres.DataContracts.EntityMarketWithCSS> pEntityMarkets)
        {
            throw new Exception("not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <returns></returns>
        protected override decimal RoundAmount(decimal pAmount)
        {
            return base.RoundAmount(pAmount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAmount"></param>
        /// <param name="pPrecision"></param>
        /// <returns></returns>
        protected override decimal RoundAmount(decimal pAmount, int pPrecision)
        {
            return base.RoundAmount(pAmount, pPrecision);
        }
    }
}

using System.Collections.Generic;

using EFS.Common;

using FpML.Interface;
using EFS.ACommon;


// Containing all the interfaces in order to communicate the calculation details 
// between the calculation method class (RiskMethodBase and extended classes)
// and the calculation sheet builder (CalculationSheetRepository and partial extentions)
namespace EFS.SpheresRiskPerformance.CalculationSheet.Interfaces
{
    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.BaseMethod"/>)
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build a margin calculation node (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
    /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)
    /// </summary>
    public interface IMarginCalculationMethodCommunicationObject
    {
        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the main chapters (<see cref="EfsML.Interface.IRiskParameter"/> 
        /// and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) of the margin calculation node
        /// (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>).
        /// </summary>
        IRiskParameterCommunicationObject[] Parameters { get; set; }
    }

    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.BaseMethod"/>)
    /// to the calculation sheet repository object(<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) 
    /// in order to
    /// build a risk parameter node (<see cref="EfsML.Interface.IRiskParameter"/> and 
    /// <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>)
    /// </summary>
    public interface IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter
        /// </summary>
        IMoney MarginAmount { get; set; }
    }

    /// <summary>
    /// Interface any communication parameter must implement when missable
    /// </summary>
    public interface IMissingCommunicationObject
    {
        /// <summary>
        /// Missing status
        /// </summary>
        bool Missing { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        string ErrorCode { get; }
    }

    
}
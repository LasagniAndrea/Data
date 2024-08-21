using System;
using System.Collections.Generic;
//
using EFS.Common;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Enum;
//
using FpML.Interface;

// Containing all the interfaces in order to communicate the calculation details 
// between the calculation method class (RiskMethodBase and extended classes)
// and the calculation sheet builder (CalculationSheetRepository and partial extentions)
namespace EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces
{
    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.BaseMethod"/>)
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build a margin calculation node (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
    /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>)
    /// </summary>
    /// FI 20160613 [22256] Modify
    public interface IMarginCalculationMethodCommunicationObject
    {
        /// <summary>
        /// Date des parametres de calcul utilisée
        /// </summary>
        /// PM 20150507 [20575] Ajout DtParameters
        DateTime DtParameters { set; get; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the main chapters (<see cref="EfsML.Interface.IRiskParameter"/> 
        /// and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) of the margin calculation node
        /// (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// and <see cref="EfsML.Interface.IMarginCalculationMethod"/>).
        /// </summary>
        IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// amounts relative to the whole set of the parameters
        /// </summary>
        IMoney[] MarginAmounts { get; set; }

        /// <summary>
        ///  Liste des positions Equities utilisées pour couvrir des positions ETD short Call ou short Future
        /// </summary>
        /// FI 20160613 [22256] Add
        IEnumerable<StockCoverageDetailCommunicationObject> UnderlyingStock { get; set; }

        /// <summary>
        /// Type de Methode
        /// </summary>
        // PM 20200910 [25481] Ajout type de méthode
        InitialMarginMethodEnum MarginMethodType { get; set; }

        /// <summary>
        /// Nom de la méthode
        /// </summary>
        // PM 20230817 [XXXXX] Ajout MarginMethodName
        string MarginMethodName { get; }

        /// <summary>
        /// Version de la méthode
        /// </summary>
        // PM 20230817 [XXXXX] Ajout MethodVersion
        decimal MethodVersion { set; get; }

        /// <summary>
        /// Indique si un deposit a été calculé de façon incomplete (ou même pas calculé)
        /// </summary>
        // PM 20220202 Ajout IsIncomplete
        bool IsIncomplete { get; set; }
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
        
        //string ErrorCode { get; }
        SysMsgCode ErrorCode { get; }
    }
}
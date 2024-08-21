using System;
using System.Collections.Generic;
//
using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode None 
    /// à l'objet référentiel de la feuille de calcul de sorte à construire le noeud du calcul par la méthode None
    /// </summary>
    public abstract class CalcMethComBase : IMarginCalculationMethodCommunicationObject
    {
        #region Members
        #region IMarginCalculationMethodCommunicationObject Members
        /// <summary>
        /// Date des parametres de calcul utilisés
        /// </summary>
        public DateTime DtParameters { set; get; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble de données pour construire les principaux éléments
        /// du noeud de calcul du déposit par la méthode None
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montants global relatifs à l'ensemble des paramètres
        /// </summary>
        public IMoney[] MarginAmounts { get; set; }

        /// <summary>
        /// Position Equity utilisée pour couvrir des positions ETD short Call ou short Future
        /// </summary>
        public IEnumerable<StockCoverageDetailCommunicationObject> UnderlyingStock { get; set; }

        /// <summary>
        /// Type de Methode
        /// </summary>
        public InitialMarginMethodEnum MarginMethodType { get; set; }

        /// <summary>
        /// Nom de la méthode de calcul du déposit
        /// </summary>
        public string MarginMethodName
        {
            get { return MarginMethodType.ToString(); }
        }

        /// <summary>
        /// Version de la méthode
        /// </summary>
        public decimal MethodVersion { set; get; }

        /// <summary>
        /// Indique si un deposit a été calculé de façon incomplete (ou même pas calculé)
        /// </summary>
        public bool IsIncomplete { get; set; }
        #endregion IMarginCalculationMethodCommunicationObject Members
        #endregion Members
    }
}

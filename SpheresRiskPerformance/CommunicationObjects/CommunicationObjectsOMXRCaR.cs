using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using FpML.Interface;
using System.Collections.Generic;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble de données que doit passer l'objet de calcul de la méthode OMXRCaR
    /// à l'objet référentiel de la feuille de calcul de sorte à construire le noeud du calcul de la méthode OMXRCaR
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public class OMXCalcMethCom : CalcMethComBase
    {
        #region Members
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public OMXCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Communication object identifying a OMX Marging Class
    /// <para>
    /// used to build the sub chapters of the OMX margin calculation node (typeof <see cref="EfsML.v30.MarginRequirement.OMXUnderlyingSymbolParameter"/>)
    /// </para>
    /// <para>
    /// Contient les postions par symbol (exemple de symbole OMXS30) et le montants de déposit associé
    /// <para>
    /// Les postions pour lesquelles il n'existe pas de vector File sont regrouper dans item où Symbol est renseigné avec Not Found
    /// </para>
    /// </para>
    /// </summary>
    public sealed class OMXUnderlyingSymbolCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Symbole identifier
        /// </summary>
        public string Symbol { get; set; }
        #endregion Members

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres

        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Set to true when the current parameter has not been found in the parameters set, 
        /// but it has been built to stock one set of asset elements in position and no parameters have been found for them.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Error code to log the missing parameter event
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
//
using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
using EfsML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode NOMX_CFM 
    /// à l'objet référentiel de la feuille de calcul de sorte à construire le noeud du calcul par la méthode NOMX_CFM
    /// </summary>
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class NOMXCFMCalcMethCom : CalcMethComBase, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1031);
        #endregion static Members

        /// <summary>
        /// Devise pour la chambre de compensation
        /// </summary>
        public string CssCurrency;

        /// <summary>
        /// Id de l'acteur
        /// </summary>
        public int IdA;

        /// <summary>
        /// Id du book
        /// </summary>
        public int IdB;

        /// <summary>
        /// Calculation Detail
        /// </summary>
        public NOMXCFMResultDetailCom CalculationDetail;

        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Set to true when the current parameter has not been found in the parameters set, 
        /// but it has been built to stock one set of asset elements in position and no parameters have been found for them.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Error code to log the missing parameter event
        /// </summary>
        
        //public string ErrorCode
        public SysMsgCode ErrorCode
        {
            // Log en cas de paramètres manquants sur la clearing house
            get { return m_SysMsgCode; }
        }
        #endregion
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public NOMXCFMCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Objet de communication decrivant un montant associé à une courbe
    /// </summary>
    public sealed class NOMXCFMResultCurveAmountCom : NOMXCFMResultCurveCom
    {
    }

    /// <summary>
    /// Objet de communication decrivant une courbe
    /// </summary>
    public class NOMXCFMResultCurveCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Curve Name
        /// </summary>
        public string CurveName;

        /// <summary>
        /// Margin Class
        /// </summary>
        public string MarginClass;

        /// <summary>
        /// Child Curves
        /// </summary>
        public List<NOMXCFMResultCurveCom> ChildCurves;

        /// <summary>
        /// Indique s'il s'agit d'une curve de correlation
        /// </summary>
        public bool IsChildCurve;

        /// <summary>
        /// Overlap PC1
        /// </summary>
        public int OverlapPC1;
        /// <summary>
        /// Overlap PC2
        /// </summary>
        public int OverlapPC2;
        /// <summary>
        /// Overlap PC3
        /// </summary>
        public int OverlapPC3;

        /// <summary>
        /// Ensemble des scénarios de la courbe
        /// </summary>
        public List<NOMXCFMScenarioCom> Scenarios;

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Positions partielles relative aux paramètres de risque courant
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données pour construire
        /// les sous-éléments (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètres de risque
        /// (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant partiel de risque en rapport au paramètre de risque courant
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Définir à vrai si le paramètre actuel n'a pas été trouvé dans l'ensemble des paramètres,
        /// mais il a été bati pour stocker un ensemble d'éléments d'actifs en position pour lesquels aucun paramètre n'a été trouvé.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Code d'erreur pour identifier l'événement paramètre manquant
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
        #endregion Members
    }

    /// <summary>
    /// Objet de communication decrivant un scénario d'une courbe
    /// </summary>
    public class NOMXCFMScenarioCom
    {
        #region Members
        /// <summary>
        /// Point Number for PC1
        /// </summary>
        public int PC1PointNo;
        /// <summary>
        /// Point Number for PC2
        /// </summary>
        public int PC2PointNo;
        /// <summary>
        /// Point Number for PC3
        /// </summary>
        public int PC3PointNo;
        /// <summar>
        /// Low
        /// </summary>
        public decimal LowValue;
        /// <summary>
        /// Middle
        /// </summary>
        public decimal MiddleValue;
        /// <summary>
        /// High
        /// </summary>
        public decimal HighValue;
        #endregion Members
    }

    /// <summary>
    /// Objet de communication decrivant l'ensemble des courbes utilisées lors du calcul
    /// </summary>
    public class NOMXCFMResultDetailCom
    {
        #region Members
        /// <summary>
        /// Détail de la hiérarchie de correlation
        /// </summary>
        public List<NOMXCFMResultCurveCom> CorrelationCurveDetail;

        /// <summary>
        /// Détail des courbes
        /// </summary>
        public List<NOMXCFMResultCurveCom> SingleCurveDetail;
        #endregion Members
    }
}

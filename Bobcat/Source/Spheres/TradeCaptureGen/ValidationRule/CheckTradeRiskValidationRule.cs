#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Validation de la saisie des trades tels que GPRODUCT = 'RISK'
    /// </summary>
    ///<remarks>
    /// Pour l'instant aucune validation rule n'est contrôlée
    ///</remarks> 
    public class CheckTradeRiskValidationRule : CheckTradeInputValidationRuleBase
    {
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRiskInput"></param>
        /// <param name="pCaptureModeEnum"></param>
        /// <param name="pUser"></param>
        public CheckTradeRiskValidationRule(TradeRiskInput pRiskInput, Cst.Capture.ModeEnum pCaptureModeEnum, User pUser)
            : base(pRiskInput, pCaptureModeEnum, pUser)
        {
        }
        #endregion constructor

        /// <summary>
        /// Retourne true si toutes les validations rules sont respectées
        /// </summary>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        /// FI 20150730 [21156] Modify
        /// FI 20160517 [22148] Modify
        // EG 20180205 [23769] Use dbTransaction  
        public override bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();

            // FI 20160517 [22148] Add if puisque désormais un cas d'annulation il y a appel aux validations rules
            if (Cst.Capture.IsModeNewCapture(CaptureMode) ||
                Cst.Capture.IsModeUpdate(CaptureMode))
            {
                CheckValidationRule_Party();
                // FI 20150730 [21156] call CheckValidationRule_BookManaged
                CheckValidationRule_BookManaged(pCS, pDbTransaction);
            }

            return ArrFunc.IsEmpty(m_CheckConformity);
        }
    }
}
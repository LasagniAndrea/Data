#region Using Directives
using EFS.ACommon;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Validation d'une saisie de correctionOfQuantity
    /// </summary>
    public class CheckCorrectionOfQuantityValidationRule : CheckValidationRuleBase
    {
        #region Constructors
        /// <summary>
        /// Aucun contrôle n'est opéré pour l'instant
        /// </summary>
        /// <param name="pCorrectionOfQty"></param>
        public CheckCorrectionOfQuantityValidationRule(TradePositionCancelation _) : base()
        {
        }
        #endregion constructor

        #region  Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        public override bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();

            CheckValidationRule_CorrectionOfQuantity();

            return ArrFunc.IsEmpty(m_CheckConformity);
        }
        
        /// <summary>
        /// Contrôle la correction de quantité
        /// </summary>
        private void CheckValidationRule_CorrectionOfQuantity()
        {
            if (IsCheckError)
            {
            }
        }
        #endregion
    }

}

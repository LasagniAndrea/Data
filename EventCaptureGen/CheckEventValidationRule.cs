#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;

using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;




using EFS.Status;
using EFS.Tuning;
using EFS.Permission;



using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CheckEventValidationRule
    /// <summary>
    /// Validation de la saisie des évènements
    /// </summary>
    ///<remarks>
    ///</remarks> 
    public class CheckEventValidationRule : CheckTradeValidationRuleBase
    {
        #region Members
        //private EventInput m_Input;
        #endregion Members
        

        #region Constructors
        public CheckEventValidationRule(EventInput pEventInput  , Cst.Capture.ModeEnum pCaptureModeEnum)
            : base(pEventInput.SQLInstrument, pCaptureModeEnum)
        {
            //m_Input = pEventInput;
        }
        #endregion constructor
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCheckMode"></param>
        /// <returns></returns>
        public override bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable();
            //
            return ArrFunc.IsEmpty(m_CheckConformity);
        }
        
        #endregion Methods
    }

    #endregion CheckEventValidationRule
}

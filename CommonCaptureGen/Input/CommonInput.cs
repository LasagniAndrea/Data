#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

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
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    // EG 20180205 [23769] Add dbTransaction  
    public class CommonInput
    {
        #region Enums
        #region DefaultEnum
        // EG 20171114 [23509] New facility
        public enum DefaultEnum
        {
            party,
            currency,
            businessCenter,
            market,
            facility,
        }
        #endregion DefaultEnum
        #endregion Enums
        
        #region Members
        
        /// <summary>
        /// Détient les valeurs par défaut de la saisie
        /// </summary>
        protected Hashtable m_Default;
        #endregion Members

        
        #region Constructors
        public CommonInput()
        {
            m_Default = new Hashtable();
        }
   
         
        #endregion Constructors
        //
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public virtual void Clear()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnum"></param>
        /// <returns></returns>
        public bool IsDefaultSpecified(DefaultEnum pEnum)
        {
            bool isSpecified = false;
            object obj = GetDefault(pEnum);
            //
            if (null != obj)
            {
                switch (pEnum)
                {
                    case DefaultEnum.party:
                        isSpecified = (0 < ((EFS_DefaultParty)obj).OTCmlId);
                        break;
                    default:
                        isSpecified = StrFunc.IsFilled((string)obj);
                        break;
                }
            }
            //
            return isSpecified;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnum"></param>
        /// <returns></returns>
        public object GetDefault(DefaultEnum pEnum)
        {
            object obj = null;
            //
            if (ArrFunc.IsFilled(m_Default))
                obj = m_Default[pEnum];
            //
            return obj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnum"></param>
        /// <param name="pObj"></param>
        public void SetDefault(DefaultEnum pEnum, object pObj)
        {
            if (null != m_Default)
            {
                if (m_Default.Contains(pEnum))
                    m_Default[pEnum] = pObj;
                else
                    m_Default.Add(pEnum, pObj);
            }
        }
        // EG 20171114 [23509] Upd
        public void InitDefault(DefaultEnum pType, string pDefaultValue)
        {
            if (m_Default.Contains(pType))
                m_Default[pType] = pDefaultValue;
            else
                m_Default.Add(pType, pDefaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDefaultParty"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pBusinessDayConvention"></param>
        public void InitDefault(EFS_DefaultParty pDefaultParty, string pCurrency, string pBusinessDayConvention)
        {
            m_Default.Clear();
            m_Default.Add(DefaultEnum.party, pDefaultParty);
            m_Default.Add(DefaultEnum.currency, pCurrency);
            m_Default.Add(DefaultEnum.businessCenter, pBusinessDayConvention);
        }
        #endregion Methods
    }
}

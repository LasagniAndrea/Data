#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
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
    /// Exception réservée pour la procédure de sauvegarde d'un trade (CheckAndRecord)
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA2237]
    [Serializable]
    public class TradeCommonCaptureGenException : SpheresException2
    {
        #region Members
        /// <summary>
        /// Représente l'erreur 
        /// </summary>
        private readonly TradeCommonCaptureGen.ErrorLevel m_ErrLevel;
        #endregion Members

        #region Accessors
        #region ErrLevel
        /// <summary>
        /// Représente l'erreur 
        /// </summary>
        public TradeCommonCaptureGen.ErrorLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #endregion Accessors
       
        #region Constructors
        /// <summary>
        ///  Nouvelle instance avec InnerException
        ///  <para>Ce constructeur doit être appellé si une exception est à l'origine de cette instance</para>
        /// </summary>
        /// <param name="pMethod">Method C# à l'origine de l'exception</param>
        /// <param name="pException">Exception d'origine</param>
        /// <param name="pErrLevel">Error level</param>
        public TradeCommonCaptureGenException(string pMethod, Exception pException, TradeCommonCaptureGen.ErrorLevel pErrLevel)
            : base(pMethod, pException)
        {
            m_ErrLevel = pErrLevel;
        }
        /// <summary>
        ///  Nouvelle instance sans InnerException
        /// </summary>
        /// <param name="pMethod">Method C# à l'origine de l'exception</param>
        /// <param name="pMessage">Messsage descriptif de l'exception</param>
        /// <param name="pErrLevel">Error level</param>
        public TradeCommonCaptureGenException(string pMethod, string pMessage, TradeCommonCaptureGen.ErrorLevel pErrLevel)
            : base(pMethod, pMessage)
        {
            m_ErrLevel = pErrLevel;
        }
        #endregion Constructors
        
        #region Methods
        /// <summary>
        /// Retourne "[Code Return:{ErrLevel}]"
        /// </summary>
        /// <returns></returns>
        protected override string GetMsgHeader()
        {
            return StrFunc.AppendFormat("[Code Return:{0}]", m_ErrLevel.ToString());
        }

        // EG 20180425 Analyse du code Correction [CA2240]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("ErrLevel", m_ErrLevel);
            base.GetObjectData(info, context);
        }

        #endregion Methods
    }
}

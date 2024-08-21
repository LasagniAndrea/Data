#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class TradeAdminCaptureGen : TradeCommonCaptureGen
    {
        #region Members
        private TradeAdminInput m_Input;
        #endregion Members
        
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeAdminInput Input
        {
            set { m_Input = value; }
            get { return m_Input; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInput TradeCommonInput
        {
            get
            {
                return (TradeCommonInput)Input;
            }
        }
        
        
        /// <summary>
        /// Obtient TRADEADMIN
        /// </summary>
        public override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.TRADEADMIN.ToString();
            }
        }
        
        #endregion Accessors
        
        #region Constructor
        public TradeAdminCaptureGen() : base() {
            m_Input = new TradeAdminInput();
        }
        #endregion Constructors
        
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUSer"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSetNewCustomcapturesInfos"></param>
        /// <returns></returns>
        ///FI 20091130 [16769] appel de la méthode de la classe de base,add  pIsSetNewCustomcapturesInfos
        ///EG 20100401 Add new [SQL_TableWithID.IDType] parameter
        public override bool Load(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, Cst.Capture.ModeEnum pCaptureMode, User pUSer, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {
            bool isOk = base.Load(pCS, pDbTransaction, pId, pIdType, pCaptureMode, pUSer, pSessionId, pIsSetNewCustomcapturesInfos);
            Input.IsAllocatedInvoiceDates = false;
            return isOk;
        }
        #endregion Methods
    }
    
    
    
}

#region Using Directives
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    public class TradeAdminCaptureGenException : TradeCommonCaptureGenException
    {
        #region Constructors
        public TradeAdminCaptureGenException(string pMethod, Exception pException, TradeAdminCaptureGen.ErrorLevel pErrLevel)
            : base(pMethod, pException, pErrLevel) { }
        public TradeAdminCaptureGenException(string pMethod, string pMessage, TradeAdminCaptureGen.ErrorLevel pErrLevel)
            : base(pMethod, pMessage, pErrLevel) { }
        #endregion Constructors
    }
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using System;
using System.Collections;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{

    #region CheckValidationLockBase
    public abstract class CheckValidationLockBase
    {
        #region Members
        protected Hashtable m_CheckLock;
        protected string m_Cs;
        
        protected AppSession m_Session;
        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public string CS
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public IDbTransaction DbTransaction
        {
            get;
            private set;
        }

        
        #endregion Accessors
        #region Constructors
        public CheckValidationLockBase(string pCs, IDbTransaction pDbTransaction, AppSession pSession)
        {
            CS = pCs;
            DbTransaction = pDbTransaction;
            m_Session = pSession;
            m_CheckLock = new Hashtable();
        }
        #endregion Constructors
        #region Methods
        #region GetLockMsg
        public string GetLockMsg()
        {
                StrBuilder ret = new StrBuilder();
                if (ArrFunc.IsFilled(m_CheckLock))
                {
                    Array targetArray = Array.CreateInstance(typeof(String), m_CheckLock.Count);
                    m_CheckLock.Values.CopyTo(targetArray, 0);
                    string[] arrMsg = (string[])targetArray;
                    for (int i = 0; i < arrMsg.Length; i++)
                        ret += arrMsg[i] + Cst.CrLf + Cst.CrLf;
                }
                return ret.ToString();
            
        }
        #endregion GetLockMsg
        #region GetIdAdditionalInfo
        protected static string GetIdAdditionalInfo(string pId)
        {
            string ret = String.Empty;
            if (StrFunc.IsFilled(pId))
                ret = "(" + Cst.Space + "id:" + Cst.Space + pId + Cst.Space + ")";
            return ret;
        }
        #endregion GetIdAdditionalInfo
        #region SetValidationLockError
        /// <summary>
        /// Ajoute un item dans la collection des messages
        /// </summary>
        /// <param name="pRes">ressoure</param>
        protected void SetValidationLockError(string pRes)
        {
            SetValidationLockError(pRes, null, null);
        }

        /// <summary>
        /// Ajoute un item dans la collection des messages 
        /// </summary>
        /// <param name="pRes">ressoure</param>
        /// <param name="pAdditionalInfo">Info supplementaire</param>
        protected void AddCheckLockError(string pMessage)
        {
            if (false == m_CheckLock.Contains(pMessage))
                m_CheckLock.Add(pMessage, "-" + Cst.Space + pMessage);
        }
        /// <summary>
        /// Ajoute un item dans la collection des messages 
        /// </summary>
        /// <param name="pRes">ressoure</param>
        /// <param name="pItems">liste des paramètres de la ressource</param>
        protected void SetValidationLockError(string pRes, string[] pItems)
        {
            SetValidationLockError(pRes, null, pItems);
        }
        /// <summary>
        /// Ajoute un item dans la collection des messages 
        /// </summary>
        /// <param name="pRes">ressoure</param>
        /// <param name="pAdditionalInfo">info supplémentaire</param>
        /// <param name="pItems">liste des paramètres de la ressource</param>
        protected void SetValidationLockError(string pRes, string pAdditionalInfo, string[] pItems)
        {
            string res;
            if (ArrFunc.IsEmpty(pItems))
                res = Ressource.GetString(pRes, true);
            else
                res = Ressource.GetString2(pRes, true, pItems);
            res = res + Cst.Space + pAdditionalInfo;
            AddCheckLockError(res);
        }
        #endregion SetValidationLockError
        #region ValidationLocks
        public virtual bool ValidationLocks()
        {
            return ArrFunc.IsEmpty(m_CheckLock);
        }
        #endregion ValidationLocks
        #endregion Methods
    }
    #endregion CheckValidationLockBase

    #region CheckTradeValidationLockBase
    public abstract class CheckTradeValidationLockBase : CheckValidationLockBase
    {
        #region Members
        protected SQL_Instrument m_SQLInstrument;
        protected Cst.Capture.ModeEnum m_CaptureMode;
        #endregion Members
        #region Accessors
        #region SQLInstrument
        public SQL_Instrument SQLInstrument
        {
            get { return m_SQLInstrument; }
        }
        #endregion SQLInstrument
        #endregion Accessors
        #region Constructors
        public CheckTradeValidationLockBase(string pCs, IDbTransaction pDbTransaction, SQL_Instrument pSQLInstrument, Cst.Capture.ModeEnum pCaptureModeEnum, AppSession pSession)
            : base(pCs, pDbTransaction, pSession)
        {
            m_CaptureMode = pCaptureModeEnum;
            m_SQLInstrument = pSQLInstrument;
        }
        #endregion Constructors
    }
    #endregion CheckTradeValidationLockBase
}


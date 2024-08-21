#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using System;
using System.Data;
using System.Reflection;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    ///  Saisie/Modifications des évènements
    /// </summary>
    public sealed class EventCaptureGen : CommonCaptureGen
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        public enum ErrorLevel
        {
            SUCCESS = -1,
            UNDEFINED = 0,
            LICENSEE_ERROR = 5,
            STATUS_ERROR = 17,
            LOCK_ERROR = 30,
            LOCKPROCESS_ERROR = 32,
            ROWVERSION_ERROR = 35,
            BEGINTRANSACTION_ERROR = 60,
            EDIT_EVENT_ERROR = 70,
            EDIT_EVENTDET_ERROR = 71,
            EDIT_EVENTCLASS_ERROR = 72,
            EDIT_EVENTASSET_ERROR = 73,
            EDIT_EVENTPRICING_ERROR = 74,
            EDIT_EVENTPROCESS_ERROR = 75,
            COMMIT_ERROR = 90,
            BUG = 100,
            VALIDATIONRULE_ERROR = 300,
        }
        #endregion Enums

        #region Members
        #endregion Members

        #region Accessors
        /// <summary>
        /// Classe qui représente les évènements d'un trade (contient le dataDocument...)  
        /// <para>Contient la collection cci pour consulter, modifier les évènemnets</para>
        /// </summary>
        public EventInput Input
        {
            set;
            get;
        }
        
        /// <summary>
        ///  Représente le trade
        /// </summary>
        public TradeCommonInput TradeCommonInput
        {
            set;
            get;
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTradeCommonInput"></param>
        public EventCaptureGen(TradeCommonInput pTradeCommonInput)
            : base()
        {
            TradeCommonInput = pTradeCommonInput;
            Input = new EventInput();
        }
        #endregion Constructors
        
        #region Methods
        /// <summary>
        ///  Validation de la saisie d'un évènement
        /// </summary>
        /// <param name="pCaptureMode"></param>
        // EG 20171115 Add CaptureSessionInfo pSessionInfo
        public void CheckValidationRule(string pCS, IDbTransaction pDbTransaction,  Cst.Capture.ModeEnum pCaptureMode)
        {
            CheckEventValidationRule chk = new CheckEventValidationRule(Input, pCaptureMode);
            if (false == chk.ValidationRules(CSTools.SetCacheOn(pCS), pDbTransaction, CheckEventValidationRule.CheckModeEnum.Error))
                throw new EventCaptureGenException("EventCaptureGen.CheckValidationRule", chk.GetConformityMsg(),
                    EventCaptureGen.ErrorLevel.VALIDATIONRULE_ERROR);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pCaptureSessionInfo"></param>
        public void CheckValidationLock(string pCS, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pCaptureSessionInfo)
        {
            CheckEventValidationLock chk = new CheckEventValidationLock(pCS, null, TradeCommonInput, pCaptureMode, pCaptureSessionInfo.session);
            if (false == chk.ValidationLocks())
                throw new EventCaptureGenException(MethodInfo.GetCurrentMethod().Name, chk.GetLockMsg(), EventCaptureGen.ErrorLevel.LOCKPROCESS_ERROR);
        }
        
       
        
        
        /// <summary>
        ///  Charge tous les évènements du trade dans le dataset et se positonne sur le 1er évènment
        /// </summary>
        /// <param name="pTradeIdentifier"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        public bool LoadEvents(string pCS, string pTradeIdentifier, User pUser, string pSessionId)
        {
            return LoadEvents(pCS, TradeRDBMSTools.GetTradeIdT(pCS, pTradeIdentifier), pUser, pSessionId);
        }
        /// <summary>
        ///  Charge tous les évènements du trade dans le dataset et se positonne sur le 1er évènment
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2235]
        public bool LoadEvents(string pCS, int pIdT, User pUser, string pSessionId)
        {
            bool isOk = false;
            Input.SearchAndDeserializeEvents(pCS, pIdT);
            Input.CustomCaptureInfos = new EventCustomCaptureInfos(pCS, this.Input, pUser, pSessionId, true);
            Input.CustomCaptureInfos.InitializeCciContainer();
            return (isOk);
        }
        
        /// <summary>
        ///  Charge tous les évènements du trade dans le dataset et se positonne sur un évènement en  particulier
        /// </summary>
        /// <param name="pTradeIdentifier"></param>
        /// <param name="EventIdentifier"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modification de la signature
        public void LoadEvent(string pCS, string pTradeIdentifier, string EventIdentifier,  User pUser, string pSessionId)
        {
            LoadEvent(pCS, TradeRDBMSTools.GetTradeIdT(pCS, pTradeIdentifier), Convert.ToInt32(EventIdentifier), pUser, pSessionId);
        }
        /// <summary>
        ///  Charge tous les évènements du trade dans le dataset et se positonne sur un évènement en  particulier
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modification de la signature
        // EG 20180425 Analyse du code Correction [CA2235]
        public void LoadEvent(string pCS, int pIdT, int pIdE, User pUser, string pSessionId)
        {
            Input.SearchAndDeserializeEvent(pCS, pIdT, pIdE);
            Input.CustomCaptureInfos = new EventCustomCaptureInfos(pCS, this.Input, pUser, pSessionId, true);
            Input.CustomCaptureInfos.InitializeCciContainer();
        }
        
        /// <summary>
        /// Sauvegarde de l'évènement (Modification ou Création)
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pIdT"></param>
        /// <param name="pTradeidentifier"></param>
        /// <param name="pIdE"></param>
        /// 20161123 [22629] Modify 
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SaveCurrentEvent(string pCS, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, ref int pIdE)
        {
            ErrorLevel errLevel = ErrorLevel.SUCCESS;
            EventCaptureGenException errExc = null;

            int idE = pIdE;
            
            IDbTransaction dbTransaction = null;

            //20161123 [22629] gestion isCaptureNew
            bool isCaptureUpd = (Cst.Capture.ModeEnum.Update == pCaptureMode);
            bool isCaptureNew = Cst.Capture.IsModeNew(pCaptureMode) || Cst.Capture.IsModeDuplicate(pCaptureMode);
                       
            LockObject lockTrade = null;
            bool isLockSuccessful = false;
            
            try
            {
                //StringBuilder sb = null;   
                //string SQLQuery = string.Empty;
                #region Check Licensee
                try
                {
                    if (false == pSessionInfo.licence.IsLicProductAuthorised_Add(Input.SQLProduct.Identifier))
                        throw new EventCaptureGenException("EventCaptureGen.Save", string.Empty, ErrorLevel.LICENSEE_ERROR);
                }
                catch (EventCaptureGenException) { throw; }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.LICENSEE_ERROR); }
                #endregion Check Licensee
                #region Lock Trade (if Modify only)
                try
                {
                    if (isCaptureUpd)
                    {
                        lockTrade = new LockObject(TypeLockEnum.TRADE, Input.TradeIdentification.OTCmlId,Input.TradeIdentification.Identifier, LockTools.Exclusive);
                        Lock lck = new Lock(pCS, lockTrade, pSessionInfo.session, Cst.Capture.GetLabel(pCaptureMode));
                        isLockSuccessful = LockTools.LockMode2(lck, out Lock lckExisting);
                        if (!isLockSuccessful)
                        {
                            if (null != lckExisting)
                                throw new EventCaptureGenException("EventCaptureGen.Save", lckExisting.ToString(), ErrorLevel.LOCK_ERROR);
                        }
                    }
                }
                catch (EventCaptureGenException) { throw; }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.LOCK_ERROR); }
                #endregion Lock Trade (if Modify only)
                #region Check RowVersion
                try
                {
                }
                catch (EventCaptureGenException) { throw; }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.ROWVERSION_ERROR); }
                #endregion Check RowVersion
                #region Check Status
                try
                {
                }
                catch (EventCaptureGenException) { throw; }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.STATUS_ERROR); }
                #endregion
                //
                #region START Transaction (Begin Tran)
                //Begin Tran  doit être la 1er instruction Car si Error un  roolback est fait de manière systematique
                try { dbTransaction = DataHelper.BeginTran(pCS); }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.BEGINTRANSACTION_ERROR); }
                #endregion START Transaction (Begin Tran)

                #region Check Validation Rule
                try { CheckValidationRule(pCS, null, pCaptureMode); }
                catch (EventCaptureGenException) { throw; }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.VALIDATIONRULE_ERROR); }
                #endregion Check Validation Rule

                #region Check Validation Lock
                try
                {
                    CheckValidationLock(pCS,pCaptureMode, pSessionInfo);
                }
                catch (EventCaptureGenException) { throw; }
                catch (Exception ex) { throw new EventCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.LOCKPROCESS_ERROR); }
                #endregion Check Validation Lock

                #region Update Event
                if (isCaptureUpd || isCaptureNew)
                {
                 
                    #region EVENT
                    try { Input.DataSetEvent.UpdateEvent(dbTransaction,  Input.CurrentEventIndex, Input.CurrentEvent, Input.CurrentEventStatus, pSessionInfo.session); }
                    catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.EDIT_EVENT_ERROR); }
                    #endregion EVENT
                    #region EVENTASSET
                    try { Input.DataSetEvent.UpdateEventAsset(dbTransaction, Input.CurrentEvent); }
                    catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.EDIT_EVENTASSET_ERROR); }
                    finally { }
                    #endregion EVENTASSET
                    #region EVENTCLASS
                    try { Input.DataSetEvent.UpdateEventClass(dbTransaction, Input.CurrentEvent); }
                    catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.EDIT_EVENTCLASS_ERROR); }
                    finally { }
                    #endregion EVENTCLASS
                    #region EVENTDET
                    try { Input.DataSetEvent.UpdateEventDetail(dbTransaction, Input.CurrentEvent); }
                    catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.EDIT_EVENTDET_ERROR); }
                    finally { }
                    #endregion EVENTDET
                    #region EVENTPROCESS
                    try { Input.DataSetEvent.UpdateEventProcess(dbTransaction, Input.CurrentEvent); }
                    catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.EDIT_EVENTPROCESS_ERROR); }
                    finally { }
                    #endregion EVENTPROCESS


                    if (isCaptureNew)
                        idE = Input.CurrentEvent.idE;
                }
                #endregion Update Event
                #region END Transaction (Commit/Rollback)
                try { DataHelper.CommitTran(dbTransaction); }
                catch (Exception ex) { throw new EventCaptureGenException("EventCaptureGen.Save", ex, ErrorLevel.COMMIT_ERROR); }
                #endregion
            }
            catch (EventCaptureGenException ex)
            {
                errExc = ex;
                errLevel = ex.ErrLevel;
            }
            catch (Exception ex)
            {
                errLevel = ErrorLevel.BUG;
                errExc = new EventCaptureGenException("EventCaptureGen.Save", ex, errLevel);
            }
            finally
            {
                if (ErrorLevel.SUCCESS != errLevel)
                {
                    try { DataHelper.RollbackTran(dbTransaction); }
                    catch { }
                }
                
                try
                {
                    if (isLockSuccessful && (null != lockTrade))
                        LockTools.UnLock(pCS, lockTrade, pSessionInfo.session.SessionId);
                }
                catch
                {
                    //Si ça plante tant pis le lock sera supprimer en fin de session
                }
               
                if (null != errExc)
                    throw errExc;
            }
            pIdE = idE;
        }
        
        #endregion Methods
        
    }
    

    /// <summary>
    /// 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA2237]
    [Serializable]
    public class EventCaptureGenException : SpheresException2
    {
        #region Members
        private readonly EventCaptureGen.ErrorLevel m_ErrLevel;
        #endregion Members
        #region Accessors
        #region ErrLevel
        public EventCaptureGen.ErrorLevel ErrLevel
        {
            get { return m_ErrLevel; }
        }
        #endregion ErrLevel
        #endregion Accessors
        #region Constructors
        public EventCaptureGenException(string pMethod, Exception pException, EventCaptureGen.ErrorLevel pErrLevel)
            : base(pMethod, pException)
        {
            m_ErrLevel = pErrLevel;
        }
        public EventCaptureGenException(string pMethod, string pMessage, EventCaptureGen.ErrorLevel pErrLevel)
            : base(pMethod, pMessage,new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnAbortedEnum))
        {
            m_ErrLevel = pErrLevel;
        }
        #endregion Constructors
        #region Methods
        #region GetMsgHeader
        protected override string GetMsgHeader()
        {
            return "[Code Return:" + Cst.Space + m_ErrLevel.ToString() + Cst.Space + "]";
        }
        #endregion GetMsgHeader
        #endregion Methods
    }
    
}

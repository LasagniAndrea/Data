#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.CashBalance;
using FixML.Enum;
using EfsML.Enum.Tools;
using System.Data;
using System;
using System.Reflection;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Classe chargé de sauvegarder un trade RISK
    /// </summary>
    public sealed partial class TradeRiskCaptureGen : TradeCommonCaptureGen
    {
        #region members
        /// <summary>
        /// 
        /// </summary>
        private TradeRiskInput m_Input;
        #endregion members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeRiskInput Input
        {
            set { m_Input = value; }
            get { return m_Input; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20130204 [] propertie set utilisée pour l'importation 
        public override TradeCommonInput TradeCommonInput
        {
            get
            {
                return (TradeCommonInput)m_Input;
            }
            set
            {
                this.Input = (TradeRiskInput)value;
            }
        }

        /// <summary>
        /// Obtient TRADERISK
        /// </summary>
        public override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.TRADERISK.ToString();
            }
        }

        #endregion Accessors

        #region Constructor
        public TradeRiskCaptureGen()
        {
            m_Input = new TradeRiskInput();
        }
        #endregion Constructors

        #region Methods
        ///<summary>
        ///Contrôle des validationRules
        ///</summary>
        ///<param name="pDbTransaction"></param>
        ///<param name="pCaptureMode"></param>
        ///<param name="pCheckMode"></param>
        ///<exception cref="TradeCommonCaptureGenException[VALIDATIONRULE_ERROR] lorsque les règles ne sont pas respectées"></exception>
        // EG 20171115 Upd Add CaptureSessionInfo
        public override void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CheckTradeValidationRule.CheckModeEnum pCheckMode, User pUser)
        {
            CheckTradeRiskValidationRule chk = new CheckTradeRiskValidationRule(m_Input, pCaptureMode, pUser);
            //
            if (false == chk.ValidationRules(CSTools.SetCacheOn(pCS), pDbTransaction, pCheckMode))
            {
                throw new TradeCommonCaptureGenException("CheckValidationRule", chk.GetConformityMsg(),
                    TradeCaptureGen.ErrorLevel.VALIDATIONRULE_ERROR);
            }
        }

        
        /// <summary>
        /// Charge le dataDocument et les ccis qui se trouvent dans TradeCommonInput
        /// </summary>
        public override bool Load(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, Cst.Capture.ModeEnum pCaptureMode, User pUser, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {
            bool ret = base.Load(pCS, pDbTransaction, pId, pIdType, pCaptureMode, pUser, pSessionId, pIsSetNewCustomcapturesInfos);

            if ((TradeCommonInput.TradeStatus.IsStEnvironment_Regular) || (TradeCommonInput.TradeStatus.IsStEnvironment_Simul))
            {
                if ((ret) && (Input.Product.IsMarginRequirement))
                    Input.LoadEventMarginRequirement(pCS, pDbTransaction);
            }

            return ret;
        }

        /// <summary>
        /// Enregistre la correction => Impacte le trade et les évènements
        /// </summary>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pIdScreen"></param>
        /// FI 20140127 [19533] Gestion correcte des Margin Requirement multi-devise
        // EG 20180423 Analyse du code Correction [CA2200]
        public void RecordMarginRequirementCorrection(string pCS, CaptureSessionInfo pCaptureSessionInfo, string pIdScreen)
        {
            IDbTransaction dbTransaction = null;
            //
            ErrorLevel errLevel = ErrorLevel.SUCCESS;
            TradeCommonCaptureGenException errExc = null;
            //
            //Date système pour alimentation des colonnes DTUPD et DTINS ou equivalentes
            // FI 20200820 [XXXXXX] dates systèmes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);
            //
            LockObject lockTrade = null;
            try
            {
                MsgDet = string.Empty;
                //
                dbTransaction = DataHelper.BeginTran(pCS);
                Cst.Capture.ModeEnum captureMode = Cst.Capture.ModeEnum.Correction;

                #region LockTrade
                try
                {
                    lockTrade = LockTrade(pCS, dbTransaction, captureMode, pCaptureSessionInfo, Input.Identification);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.LOCK_ERROR); }
                #endregion

                #region CheckValidationLock
                try
                {
                    CheckValidationLock(pCS, dbTransaction, captureMode, pCaptureSessionInfo.session);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.LOCKPROCESS_ERROR); }
                #endregion

                #region CheckRowVersion
                try
                {
                    CheckRowVersion(pCS, dbTransaction);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.ROWVERSION_ERROR); }
                #endregion

                #region Serialize FpMLDocReader
                string data = null;
                try
                {
                    data = SerializeDataDocument();
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.SERIALIZE_ERROR); }
                #endregion

                #region Update TRADE
                try
                {
                    //Remarque( l'utilisateur ne peut pas modifier l'identification du trade)
                    SaveTrade(pCS, dbTransaction, Cst.Capture.ModeEnum.Correction, Input.Identification, pCaptureSessionInfo, dtSys);
                    SaveTradeXML(pCS, dbTransaction, Input.Identification.OTCmlId, data, Cst.Capture.ModeEnum.Correction);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.EDIT_TRADE_ERROR); }
                #endregion

                #region Update TRADETRAIL
                try
                {
                    int idT = this.TradeCommonInput.Identification.OTCmlId;
                    SaveTradeTrail(pCS, dbTransaction, idT, pCaptureSessionInfo, pIdScreen, captureMode, dtSys);
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.ADD_TRADETRAIL); }
                #endregion

                #region Update EVENT
                try
                {
                    MarginRequirementContainer marginRequirement = new MarginRequirementContainer((IMarginRequirement)TradeCommonInput.Product.Product);
                    string eventCode = EventCodeFunc.LinkedProductClosing.ToString();
                    if (marginRequirement.Timing == SettlSessIDEnum.Intraday)
                        eventCode = EventCodeFunc.LinkedProductIntraday.ToString();
                    string eventType = EventTypeFunc.MarginRequirement.ToString();
                    //
                    int idT = TradeCommonInput.Identification.OTCmlId;
                    int[] idE = EventRDBMSTools.GetEvents(pCS, idT, eventCode, eventType, 1, 1);
                    //
                    DataSetEvent dataSetEvent = new DataSetEvent(pCS);
                    DataSetEventLoadSettings loadSetting = new DataSetEventLoadSettings(DataSetEventLoadEnum.Event);
                    dataSetEvent.Load(dbTransaction, idT, idE, loadSetting);
                    Events events = dataSetEvent.GetEvents();
                    //
                    for (int i = 0; i < ArrFunc.Count(idE); i++)
                    {
                        Event @event = events[idE[i]];
                        //FI 20140127 [19533] recherche du payment qui correspond à la devise de l'évènement 
                        ISimplePayment payment = marginRequirement.GetPayment(@event.unit);
                        if (null != payment)
                        {
                            @event.valorisation = new EFS_Decimal(payment.PaymentAmount.Amount.DecValue);
                            @event.unit = payment.PaymentAmount.Currency;
                            dataSetEvent.UpdateEvent(dbTransaction, i, @event, null, pCaptureSessionInfo.session);
                        }
                    }
                }
                catch (TradeCommonCaptureGenException) { throw; }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.EDIT_EVENT_ERROR); }
                #endregion

                #region commit
                try
                {
                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.COMMIT_ERROR); }
                #endregion
            }
            catch (TradeCommonCaptureGenException ex)
            {
                errExc = ex;
                errLevel = ex.ErrLevel;
            }
            catch (Exception ex)
            {
                errLevel = ErrorLevel.FAILURE;
                errExc = new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, errLevel);
            }
            finally
            {
                #region finally
                if (ErrorLevel.SUCCESS != errLevel)
                {
                    try
                    {
                        //
                        if (null != dbTransaction)
                            DataHelper.RollbackTran(dbTransaction);

                    }
                    catch { }
                }
                //
                try
                {
                    if (null != lockTrade)
                        LockTools.UnLock(pCS, lockTrade, pCaptureSessionInfo.session.SessionId);
                }
                catch
                {
                    //Si plantage, le lock sera alors supprimer lors de la fin de la session.
                }
                //
                #endregion finally
            }
            //
            if (null != errExc)
                throw errExc;
        }

        /// <summary>
        /// Alimente la table TRADEACTOR
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        protected override void SaveTradeActor(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            base.SaveTradeActor(pCS, pDbTransaction, pIdT);

            if (TradeCommonInput.Product.IsMarginRequirement)
            {
                foreach (IParty party in TradeCommonInput.DataDocument.Party)
                {
                    if (TradeCommonInput.DataDocument.IsPartyClearingOrganization(party))
                        InsertTradeActorClearingOrganization(pCS, pDbTransaction, pIdT, party);
                    if (TradeCommonInput.DataDocument.IsPartyMarginRequirementOffice(party))
                        InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, party, RoleActor.MARGINREQOFFICE, null, false);
                    if (TradeCommonInput.DataDocument.IsPartyEntity(party))
                        InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, party, RoleActor.ENTITY, null, false);
                }
            }
            else if (TradeCommonInput.Product.IsCashBalance)
            {
                foreach (IParty party in TradeCommonInput.DataDocument.Party)
                {
                    if (TradeCommonInput.DataDocument.IsPartyCashBalanceOffice(party))
                        InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, party, RoleActor.CSHBALANCEOFFICE, null, false);
                    //
                    if (TradeCommonInput.DataDocument.IsPartyEntity(party))
                        InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, party, RoleActor.ENTITY, null, false);
                }
            }
            else if (TradeCommonInput.Product.IsBulletPayment)
            {
                //Ici il y a les cashPayments (versements) et les collateral titre
                //FI 20110914 => Ajout de l'acteur Entité => cela est nécessaire pour la consultation des cashBalance,des collateral,des versements
                int idAEntity = TradeCommonInput.DataDocument.GetFirstEntity(pCS);
                if (idAEntity > 0)
                {
                    IParty entity = TradeCommonInput.DataDocument.GetParty(idAEntity.ToString(), PartyInfoEnum.OTCmlId);
                    InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, entity, RoleActor.ENTITY, null, false);
                }
            }
            // PM 20120926 [18058] Add CashBalanceInterest
            else if (TradeCommonInput.Product.ProductBase.IsCashBalanceInterest)
            {
                foreach (IParty party in TradeCommonInput.DataDocument.Party)
                {
                    if (TradeCommonInput.DataDocument.IsPartyEntity(party))
                        InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, party, RoleActor.ENTITY, null, false);
                }
            }
        }

        /// <summary>
        /// Retourne systèmatiquement False pour les trades Risk afin que les trades Incomplete (Missing) soit traités à l'identique des trades Regular
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        // PM 20220202 [XXXXX] Ajout override pour que les trades Risk Incomplet soient traités à l'identique des trades Regular
        // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
        public override bool IsInputIncompleteAllow(Cst.Capture.ModeEnum pCaptureMode)
        {
            return base.IsInputIncompleteAllow(pCaptureMode);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class TradeRiskCaptureGen
    {
        #region Methods
        /// <summary>
        /// Alimente seulement la table TRADESTREAM pour un trade RISK
        /// </summary>
        /// <param name="pCS">Chaine de connection</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pTradeProduct">product RISK</param>
        /// <param name="pIsUpdateOnly_TradeInstrument">Mise à jour de TRADEINSTRUMENT uniquement</param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct, bool pIsUpdateOnly_TradeInstrument)
        {
            if (false == pIsUpdateOnly_TradeInstrument)
                InsertTradeStream(pCS, pDbTransaction, pIdT, pProduct);
        }
        protected override void InsertTradeStream(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct)
        {
            int streamNo = 1;
            int instrumentNo = Convert.ToInt32(pProduct.ProductBase.Id.Replace(Cst.FpML_InstrumentNo, string.Empty));

            if (pProduct.ProductBase.IsMarginRequirement)
            {
                IMarginRequirement marginRequirement = ((IMarginRequirement)pProduct.Product);
                TradeStreamTools.InsertStreamSimplePayment(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo, marginRequirement.Payment[instrumentNo - 1]);
            }
            else if (pProduct.ProductBase.IsCashBalance)
            {
                streamNo = 0;
                CashBalance cashBalance = (CashBalance)pProduct.Product;
                CashBalanceStream[] stream = cashBalance.cashBalanceStream;
                for (int i = 0; i < ArrFunc.Count(stream); i++)
                {
                    streamNo++;
                    TradeStreamInfoShortForm streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo)
                    {
                        IdC1 = stream[i].currency.Value
                    };
                    streamInfo.Insert(pCS, pDbTransaction);
                }
                if (cashBalance.exchangeCashBalanceStreamSpecified)
                {
                    streamNo++;
                    TradeStreamInfoShortForm streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo)
                    {
                        IdC1 = cashBalance.exchangeCashBalanceStream.currency.Value
                    };
                    streamInfo.Insert(pCS, pDbTransaction);
                }
            }
            else if (pProduct.ProductBase.IsBulletPayment) //CashPayment
            {
                IPayment payment = ((IBulletPayment)pProduct.Product).Payment;
                TradeStreamTools.InsertStreamPayment(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo, payment);
            }
            else if (pProduct.ProductBase.IsCashBalanceInterest)
            {
                ICashBalanceInterest cbi = (ICashBalanceInterest)pProduct.Product;
                IInterestRateStream[] streams = cbi.Stream;
                TradeStreamTools.InsertInterestRateStreams(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, streams);
            }
            else
            {
                throw new Exception("Error, Current product is not managed, please contact EFS");
            }
        }
        #endregion
    }
}

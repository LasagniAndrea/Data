#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML.Business;
using EfsML.CorporateActions;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    // EG [33415/33420]
    public partial class PosKeepingGen_ETD
    {
        #region Members
        // EG 20150708 [21103] Move to PosKeepingGenProcessBase
        //protected IPosKeepingMarket m_EntityMarketInfo;
        protected CorporateEventMktRules m_CorporateEventMktRules;
        protected CorporateAction m_CorporateAction;
        protected string m_CATemplatePath;
        /// EG 20140518 [19913] New 
        protected string m_CATemplatePath_AI; // Path template Script SQL
        protected DateTime m_DtSys;
        protected Pair<DataTable, DataTable> m_DtContract = new Pair<DataTable, DataTable>();
        protected Pair<DataTable, DataTable> m_DtDAttrib = new Pair<DataTable, DataTable>();
        protected Pair<DataTable, DataTable> m_DtAsset = new Pair<DataTable, DataTable>();
        // EG [33415/33420]
        protected Pair<DataTable, DataTable> m_DtAsset_Equity = new Pair<DataTable, DataTable>();

        protected Dictionary<int, Pair<DataAdapter, DataAdapter>> m_DicDataAdapter;

        #endregion Members
        #region Methods
        #region Override Methods
        //────────────────────────────────────────────────────────────────────────────────────────────────
        // CORPORATE ACTIONS (EOD)
        //────────────────────────────────────────────────────────────────────────────────────────────────
        #region EOD_CorporateActionsGen
        /// <summary>
        /// CORPORATE ACTIONS
        /// </summary>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        ///<para>► Gestion des demandes de CORPORATE ACTION du jour</para>
        ///<para>──────────────────────────────────────────────────────────────────────────</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        /// EG 20140225 [19575][19666]
        /// EG 20170315 [22967] m_PosRequest.groupProductValue remplace m_MarketPosRequest.groupProductValue
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180221 [23769] Upd Signature && Used collection List<IposRequest> m_LstMarketSubPosRequest|m_LstSubPosRequest
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel EOD_CorporateActionsGen(int pIdM)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            Cst.ErrLevel codeReturnAdj = Cst.ErrLevel.NOTHINGTODO;
            bool isClosingDay = (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay);

            m_PosRequest = RestoreMarketRequest;

            // INSERTION LOG
            
            Logger.Log(new LoggerData((isClosingDay ? LogLevelEnum.Info : LogLevelEnum.None), new SysMsgCode(SysCodeEnum.LOG, 5031), (isClosingDay ? 2 : 0),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_PosRequest.IdM)),
                new LogParam(m_PosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            Logger.Write();

            m_CATemplatePath = ProcessBase.AppInstance.MapPath(@"CorporateActions\Templates\");
            /// EG 20140518 [19913] New 
            m_CATemplatePath_AI = ProcessBase.AppInstance.MapPath(@"CorporateActions\Templates\Additionals\");
            // EG 20140106 [19441] Set null to DbTransaction;
            m_PKGenProcess.ProcessCacheContainer.DbTransaction = null;
            m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarket(m_PosRequest.IdA_Entity, pIdM, null);
            m_CorporateEventMktRules = m_PKGenProcess.ProcessCacheContainer.GetMktRules(pIdM);


            CAQueryEMBEDDED caQry = new CAQueryEMBEDDED(CS);
            QueryParameters qryParameters = caQry.GetQuerySelectCandidate();

            // EG 20140104 Recalcul DTMARKETNEXT
            IOffset offset = m_Product.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness);
            IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, m_EntityMarketInfo.IdBC);
            DateTime newDtBusinessNext = Tools.ApplyOffset(CS, m_PosRequest.DtBusiness, offset, bda, null);

            DataParameters parameters = qryParameters.Parameters;
            parameters["IDM"].Value = m_PosRequest.IdM;
            parameters["EFFECTIVEDATE"].Value = newDtBusinessNext;
            parameters["READYSTATE"].Value = CorporateActionReadyStateEnum.EMBEDDED.ToString();

            // PL 20180312 WARNING: Use Read Commited !
            //DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadUncommitted, qryParameters, null);
            DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);
            if (null != ds)
            {
                // Insert POSREQUEST REGROUPEMENT
                int nbRow = ds.Tables[0].Rows.Count;
                SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, nbRow + 1);
                ProcessStateTools.StatusEnum status = ((0 < nbRow) ? ProcessStateTools.StatusProgressEnum : ProcessStateTools.StatusNoneEnum);
                IPosRequest posRequestGroupLevel = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.EOD_CorporateActionGroupLevel, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                    status, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);
                posRequestGroupLevel.IdMSpecified = true;
                posRequestGroupLevel.IdM = pIdM;

                if (0 < nbRow)
                {
                    #region Traitement des CA
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        try
                        {
                            newIdPR++;

                            // EG 20140106 [19910] Set null to DbTransaction;
                            m_PKGenProcess.ProcessCacheContainer.DbTransaction = null;

                            // Insertion POSREQUEST (CORPORATEACTION)    
                            m_PosRequest = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.CorporateAction, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                                ProcessStateTools.StatusProgressEnum, posRequestGroupLevel.IdPR, m_LstSubPosRequest, null);


                            m_PosRequest.IdMSpecified = true;
                            m_PosRequest.IdM = pIdM;
                            m_PosRequest.IdCESpecified = true;
                            m_PosRequest.IdCE = Convert.ToInt32(dr["IDCE"]);

                            
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9001), 1,
                                new LogParam(LogTools.IdentifierAndId(dr["IDENTIFIER"].ToString(), Convert.ToInt32(dr["ID"]))),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                new LogParam(dr["REFNOTICE"])));

                            // Alimentation des classes Corporate (Action/Event/Procedure)
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 9002), 2,
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                new LogParam(LogTools.IdentifierAndId(dr["IDENTIFIER"].ToString(), Convert.ToInt32(dr["ID"]))),
                                new LogParam(dr["REFNOTICE"])));

                            m_CorporateAction = new CorporateAction(CS);
                            CAInfo _caInfo = new CAInfo(m_PKGenProcess.Session.IdA, "IDCA", dr["ID"].ToString());
                            codeReturn = m_CorporateAction.Load(_caInfo, CATools.CAWhereMode.ID, m_CATemplatePath);

                            if (Cst.ErrLevel.SUCCESS == codeReturn)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 9003), 2,
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                                    new LogParam(m_MarketPosRequest.GroupProductValue),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                    new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                                    new LogParam(m_CorporateAction.refNotice.value)));

                                // Alimentation des classes Corporate (DerivativeContract/Asset)
                                codeReturn = LoadDerivativeContractCandidates();
                                if (Cst.ErrLevel.SUCCESS == codeReturn)
                                {
                                    // Calcul des ajustement (Ratio, Contract size, StrikePrice, Equalisation payment ...)
                                    codeReturnAdj = AdjustmentCalculation();
                                    if (Cst.ErrLevel.FAILURE != codeReturnAdj)
                                    {
                                        // Ecritures des résultats des ajustements par DCs, DAs et Assets candidats
                                        
                                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 9005), 2,
                                            new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                                            new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                                            new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                                            new LogParam(m_MarketPosRequest.GroupProductValue),
                                            new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                            new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                                            new LogParam(m_CorporateAction.refNotice.value),
                                            new LogParam(m_CorporateAction.corporateEvent[0].adjMethod)));

                                        codeReturn = AdjustmentWriting();

                                    }

                                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                                    {
                                        codeReturn = codeReturnAdj;
                                        if (isClosingDay)
                                        {
                                            // EG 20140422 Add test sur Cst.ErrLevel.FAILUREWARNING (EXCLOSINGPRICE_UNEVALUATED)
                                            if ((Cst.ErrLevel.SUCCESS == codeReturn) || (Cst.ErrLevel.FAILUREWARNING == codeReturn))
                                            {
                                                codeReturn = AdjustmentTrades();
                                                if ((Cst.ErrLevel.SUCCESS == codeReturn) && (Cst.ErrLevel.FAILUREWARNING == codeReturnAdj))
                                                    codeReturn = codeReturnAdj;
                                            }
                                            // EG 20131119 Add test sur Cst.ErrLevel.NOTHINGTODO
                                            else if (codeReturn != Cst.ErrLevel.NOTHINGTODO)
                                                codeReturn = Cst.ErrLevel.FAILURE;
                                        }
                                    }
                                }
                                else if (Cst.ErrLevel.NOTHINGTODO != codeReturn)
                                {

                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 9006), 2,
                                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                                        new LogParam(m_MarketPosRequest.GroupProductValue),
                                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                        new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                                        new LogParam(m_CorporateAction.refNotice.value),
                                        new LogParam(m_CorporateAction.corporateEvent[0].adjMethod)));
                                }
                            }
                            else
                            {
                                // FI 20200623 [XXXXX] SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 9051), 2,
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                                    new LogParam(m_MarketPosRequest.GroupProductValue),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                    new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                                    new LogParam(m_CorporateAction.refNotice.value),
                                    new LogParam(m_CorporateAction.corporateEvent[0].adjMethod)));
                            }
                        }
                        catch (Exception ex)
                        {
                            codeReturn = Cst.ErrLevel.FAILURE;
                            // FI 20200623 [XXXXX] call SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            // FI 20200623 [XXXXX] AddCriticalException
                            m_PKGenProcess.ProcessState.AddCriticalException(ex);

                            
                            Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                                new LogParam(LogTools.IdentifierAndId(GetPosRequestLogValue(((PosKeepingRequestMQueue)Queue).requestType), Queue.id)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                        }
                        finally
                        {
                            // Update POSREQUEST (CORPOACTION)
                            switch (codeReturn)
                            {
                                case Cst.ErrLevel.SUCCESS:
                                    m_PosRequest.Status = ProcessStateTools.StatusSuccessEnum;
                                    break;
                                case Cst.ErrLevel.DATAIGNORE:
                                case Cst.ErrLevel.NOTHINGTODO:
                                    m_PosRequest.Status = ProcessStateTools.StatusNoneEnum;
                                    break;
                                case Cst.ErrLevel.DATANOTFOUND:
                                case Cst.ErrLevel.FAILUREWARNING:
                                    m_PosRequest.Status = ProcessStateTools.StatusWarningEnum;
                                    break;
                                default:
                                    m_PosRequest.Status = ProcessStateTools.StatusErrorEnum;
                                    break;
                            }
                            
                            //PosKeepingTools.UpdatePosRequest(CS, null, m_PosRequest.idPR, m_PosRequest, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, posRequestGroupLevel.idPR);
                            PosKeepingTools.UpdatePosRequest(CS, null, m_PosRequest.IdPR, m_PosRequest, m_PKGenProcess.Session.IdA, IdProcess, posRequestGroupLevel.IdPR);
                        }

                    }
                    #endregion Traitement des CA
                    // EG 20130719 Refactoring gestion du status final CA
                    // Update POSREQUEST GROUP
                    // Le traitement des CAs en EOD N'EST PAS BLOQUANT  si WARNING
                    // Le traitement des CAs en CLOSINGDAY est TOUJOURS BLOQUANT
                    ProcessStateTools.StatusEnum _status = GetStatusGroupLevel(posRequestGroupLevel.IdPR, ProcessStateTools.StatusUnknownEnum);
                    if ((m_MarketPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay) && ProcessStateTools.IsStatusWarning(_status))
                        _status = ProcessStateTools.StatusErrorEnum;
                    posRequestGroupLevel.StatusSpecified = true;
                    posRequestGroupLevel.Status = _status;
                    
                    //PosKeepingTools.UpdatePosRequest(CS, null, posRequestGroupLevel.idPR, posRequestGroupLevel, m_PKGenProcess.appInstance.IdA, LogHeader.IdProcess, posRequestGroupLevel.idPR_PosRequest);
                    PosKeepingTools.UpdatePosRequest(CS, null, posRequestGroupLevel.IdPR, posRequestGroupLevel, m_PKGenProcess.Session.IdA, IdProcess, posRequestGroupLevel.IdPR_PosRequest);
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion EOD_CorporateActionsGen
        #endregion Override Methods

        #region AdjustmentCalculation
        /// <summary>
        /// Ajustement des élements de CA (pour CONTRACT / DATTRIB et ASSET)
        /// Par ENTITE/MARCHE
        /// ● Ratio
        /// ● Future
        ///   ContractSize (ContractMultiplier + Factor), DailySettlementPrice
        /// ● Option
        ///   ContractSize (ContractMultiplier + Factor), StrikePrice, EqualisationPayment
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel AdjustmentCalculation()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.NOTHINGTODO;

            CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
            CorporateEventProcedure procedure = _corporateEvent.procedure;
            m_PKGenProcess.ProcessCacheContainer.SetCACumDate(_corporateEvent.effectiveDate, m_EntityMarketInfo.IdBC);

            m_PKGenProcess.ProcessCacheContainer.InitDelegate(m_PKGenProcess.ProcessState.SetErrorWarning);

            if (procedure.adjustment is AdjustmentRatio)
            {

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 9004), 2,
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                    new LogParam(m_CorporateAction.refNotice.value),
                    new LogParam(_corporateEvent.adjMethod)));

                // Evaluation du ratio
                Rounding rounding = m_CorporateEventMktRules.rounding.Find(item => item.First == AdjustmentElementEnum.RFactor).Second;
                AdjustmentRatio ratio = procedure.adjustment as AdjustmentRatio;
                Cst.ErrLevel retRatio = ratio.rFactor.Evaluate(ratio, procedure.underlyers, m_PosRequest.IdA_Entity, m_PosRequest.IdM, m_PKGenProcess.ProcessCacheContainer, false);
                if (m_CorporateAction.corporateEvent[0].contractsSpecified)
                {
                    // Evaluation des ContractSize, Price, StrikePrice sur Contrats et Assets
                    foreach (CorporateEventContract _contract in m_CorporateAction.corporateEvent[0].contracts)
                    {
                        if (_contract.rowState != DataRowState.Deleted)
                        {
                            _contract.exDataSpecified = true;
                            _contract.exData = new CalculationExData();
                            ratio.rFactor.FillRatio(_contract, retRatio);

                            if (CATools.IsAdjStatusOK(_contract.adjStatus))
                            {
                                switch (_contract.category)
                                {
                                    case CfiCodeCategoryEnum.Future:
                                        if (ratio.contract.futureSpecified)
                                            ratio.contract.future.Evaluate(ratio, _contract, m_PosRequest.IdA_Entity, m_PosRequest.IdM, m_PKGenProcess.ProcessCacheContainer, false);
                                        break;
                                    case CfiCodeCategoryEnum.Option:
                                        if (ratio.contract.optionSpecified)
                                            ratio.contract.option.Evaluate(ratio, _contract, m_PosRequest.IdA_Entity, m_PosRequest.IdM, m_PKGenProcess.ProcessCacheContainer, false);
                                        break;
                                }
                            }
                            CATools.SetCodeReturnAfterAdjustment(_contract, ref ret);
                        }
                    }
                    retRatio = ret;
                }
                else if (retRatio != Cst.ErrLevel.SUCCESS)
                    ret = retRatio;
            }
            else if (procedure.adjustment is AdjustmentPackage)
            {
                ret = Cst.ErrLevel.DATAIGNORE;
            }
            else if (procedure.adjustment is AdjustmentFairValue)
            {
                //if (m_CorporateAction.corporateEvent[0].contractsSpecified)
                //{
                //    // Evaluation des ContractSize, Price, StrikePrice sur Contrats et Assets
                //    foreach (CorporateEventContract _contract in m_CorporateAction.corporateEvent[0].contracts)
                //    {
                //        if (_contract.rowState != DataRowState.Deleted)
                //        {
                //            AdjustmentFairValue fairValue = procedure.adjustment as AdjustmentFairValue;
                //            fairValue.SetCumDataToExData(_contract, m_PosRequest.idA_Entity, m_PosRequest.idM, m_PKGenProcess.ProcessCacheContainer);
                //        }
                //    }
                //}
                //ret = Cst.ErrLevel.SUCCESS;
                ret = Cst.ErrLevel.DATAIGNORE;
            }
            else if (procedure.adjustment is AdjustmentNone)
            {
                // EG [33415/33420] New
                if (m_CorporateAction.corporateEvent[0].contractsSpecified)
                {
                    // Evaluation des ContractSize, Price, StrikePrice sur Contrats et Assets
                    foreach (CorporateEventContract _contract in m_CorporateAction.corporateEvent[0].contracts)
                    {
                        if (_contract.rowState != DataRowState.Deleted)
                        {
                            AdjustmentNone none = procedure.adjustment as AdjustmentNone;
                            none.SetCumDataToExData(_contract, m_PosRequest.IdA_Entity, m_PosRequest.IdM, m_PKGenProcess.ProcessCacheContainer);
                        }
                    }
                }
                ret = Cst.ErrLevel.SUCCESS;
            }
            return ret;

        }
        #endregion AdjustmentCalculation

        #region UpdateDataRowCum
        /// <summary>
        /// Mise à jour DTDISABLED (Date de Corporate action = DTDISABLED) des datarow (CONTRACT / DATTRIB et ASSET)
        /// </summary>
        /// <param name="pContract">Contract (DC) courant</param>
        /// <param name="pRowCum">Row du DC (Cum)</param>
        /// <param name="pNewIdDC">Id du DC Ex</param>
        /// <param name="pNewIdDA">Id du DA Ex</param>
        /// <param name="pNewIdASSET">Id de l'ASSET Ex</param>
        /// <returns></returns>
        /// EG 20130716 Les DCex,DAEx et ASSETEx ne sont créés que sur le DC de base (ContractAttribute = 0 ou ContractSymbol sans Suffixe)
        // EG [33415/33420] New Gestion Sous-jacent ASSET_EQUITY
        private void UpdateDataRowCum(DataRow pRowCum)
        {
            // DC inactif (DTDISABLED = EFFECTIVEDATE du CE)
            pRowCum.BeginEdit();
            pRowCum["DTDISABLED"] = m_CorporateAction.corporateEvent[0].effectiveDate;
            pRowCum["DTUPD"] = m_DtSys;
            pRowCum["IDAUPD"] = 1;
            pRowCum.EndEdit();
        }
        #endregion UpdateDataRowCum
        #region InsertDataRowEx
        /// <summary>
        /// Mise à jour des Datarow (CONTRACT / DATTRIB et ASSET)
        /// ● Mise à jour DTDISABLED (Date de Corporate action = DTDISABLED) 
        /// ● Insertion nouveaux DC, DA et ASSET non ajustés (DTENABLED = DTCA)
        /// </summary>
        /// <param name="pContract">Contract (DC) courant</param>
        /// <param name="pRowCum">Row du DC (Cum)</param>
        /// <param name="pNewIdDC">Id du DC Ex</param>
        /// <param name="pNewIdDA">Id du DA Ex</param>
        /// <param name="pNewIdASSET">Id de l'ASSET Ex</param>
        /// <returns></returns>
        /// EG 20130716 Les DCex,DAEx et ASSETEx ne sont créés que sur le DC de base (ContractAttribute = 0 ou ContractSymbol sans Suffixe)
        // EG [33415/33420] New Gestion Sous-jacent ASSET_EQUITY
        private bool InsertDataRowEx(CorporateEventContract pContract, DataRow pRowCum, int? pNewIdDC, int? pNewIdAsset_UNL)
        {
            CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];
            bool isNewRow = true;
            Adjustment adjustment = corporateEvent.procedure.adjustment;
            Nullable<bool> resetDerivativeIsinCode = adjustment.GetResetDerivativeIsinCode();
            Nullable<decimal> exContractSize = null;
            Nullable<decimal> exContractMultiplier = null;
            string exContractDisplayName = string.Empty;
            string exContractIsinCode = string.Empty;
            string exContractSymbol = string.Empty;

            AIContract _aiContract = adjustment.GetAIContract(CATools.CAElementTypeEnum.Ex, pContract);
            if (null != _aiContract)
            {
                exContractSize = _aiContract.ContractSize;
                exContractMultiplier = _aiContract.ContractMultiplier;
                exContractDisplayName = _aiContract.DisplayName;
                exContractIsinCode = _aiContract.IsinCode;
                exContractSymbol = _aiContract.Symbol;
            }
                

            // DC actif (DTENABLED = EFFECTIVEDATE du CE)
            DataRow newRow = pRowCum.Table.NewRow();
            newRow.ItemArray = (object[])pRowCum.ItemArray.Clone();
            newRow.BeginEdit();
            if (pNewIdDC.HasValue && pRowCum.Table.Columns.Contains("IDDC"))
                newRow["IDDC"] = pNewIdDC.Value;

            if (pNewIdAsset_UNL.HasValue && pRowCum.Table.Columns.Contains("IDASSET_UNL"))
                newRow["IDASSET_UNL"] = pNewIdAsset_UNL.Value;

            if (pRowCum.Table.Columns.Contains("IDENTIFIER"))
                newRow["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
            if (pRowCum.Table.Columns.Contains("DISPLAYNAME"))
                newRow["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE;
            if (pRowCum.Table.Columns.Contains("DESCRIPTION"))
                newRow["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE;

            // EG 20140107 [19454]
            if (pRowCum.Table.Columns.Contains("AII"))
                newRow["AII"] = Cst.AUTOMATIC_COMPUTE;

            // EG 20140320 Additional infos
            if (StrFunc.IsFilled(exContractSymbol))
            {
                if (pRowCum.Table.Columns.Contains("CONTRACTSYMBOL"))
                {
                    newRow["CONTRACTSYMBOL"] = exContractSymbol;
                    // Pas de création de DC Ex si instruction = ##UNDO##
                    if ((false == Convert.IsDBNull(newRow["CONTRACTSYMBOL"])) && (newRow["CONTRACTSYMBOL"].ToString() == CATools.AI_Undo))
                        isNewRow = false;
                }
            }

            if (isNewRow)
            {
                if (StrFunc.IsFilled(exContractDisplayName))
                    newRow["DESCRIPTION"] += exContractDisplayName;

                if (exContractSize.HasValue && (false == Convert.IsDBNull(newRow["FACTOR"])))
                        newRow["FACTOR"] = exContractSize.Value;

                if (exContractMultiplier.HasValue && (false == Convert.IsDBNull(newRow["CONTRACTMULTIPLIER"])))
                    newRow["CONTRACTMULTIPLIER"] = exContractMultiplier.Value;

                if (pRowCum.Table.Columns.Contains("ISINCODE"))
                {
                    bool isResetIsinCode = resetDerivativeIsinCode.HasValue && resetDerivativeIsinCode.Value;
                    if (StrFunc.IsFilled(exContractIsinCode))
                        newRow["ISINCODE"] = exContractIsinCode;
                    else if (isResetIsinCode)
                        newRow["ISINCODE"] = Convert.DBNull;
                }
                newRow["DTENABLED"] = corporateEvent.effectiveDate;
                newRow["DTDISABLED"] = Convert.DBNull;
                newRow["DTINS"] = m_DtSys;
                newRow["IDAINS"] = m_PKGenProcess.Session.IdA;
                newRow["DTUPD"] = Convert.DBNull;
                newRow["IDAUPD"] = Convert.DBNull;
                newRow.EndEdit();
                pRowCum.Table.Rows.Add(newRow);
            }
            else
            {
                newRow.CancelEdit();
            }
            return isNewRow;
        }
        #endregion InsertDataRowEx
        #region InsertDataRowExForTradePostCa
        private void InsertDataRowExForTradePostCa<T>(T pCorpoEventSource, DataRow pRowCum, int? pNewIdDC, int? pNewIdDA, int? pNewIdASSET)
        {
            CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];
            Adjustment adjustment = corporateEvent.procedure.adjustment;
            Nullable<bool> resetDerivativeIsinCode = adjustment.GetResetDerivativeIsinCode();

            DataRow newRowEx = pRowCum.Table.NewRow();
            newRowEx.ItemArray = (object[])pRowCum.ItemArray.Clone();
            newRowEx.BeginEdit();

            if (pRowCum.Table.Columns.Contains("ISINCODE") && resetDerivativeIsinCode.HasValue && resetDerivativeIsinCode.Value)
                newRowEx["ISINCODE"] = Convert.DBNull;

            newRowEx["DTENABLED"] = corporateEvent.effectiveDate;
            newRowEx["DTDISABLED"] = Convert.DBNull;
            newRowEx["DTINS"] = m_DtSys;
            newRowEx["IDAINS"] = m_PKGenProcess.Session.IdA;
            newRowEx["DTUPD"] = Convert.DBNull;
            newRowEx["IDAUPD"] = Convert.DBNull;

            if (pCorpoEventSource is CorporateEventDAttrib)
            {
                if (pNewIdDA.HasValue)
                    newRowEx["IDDERIVATIVEATTRIB"] = pNewIdDA.Value;
                if (pNewIdDC.HasValue)
                    newRowEx["IDDC"] = pNewIdDC.Value;
            }
            else if (pCorpoEventSource is CorporateEventAsset)
            {
                if (pNewIdASSET.HasValue)
                    newRowEx["IDASSET"] = pNewIdASSET.Value;
                if (pNewIdDA.HasValue)
                    newRowEx["IDDERIVATIVEATTRIB"] = pNewIdDA.Value;

                newRowEx["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
                newRowEx["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE;
                newRowEx["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE;

                if (pRowCum.Table.Columns.Contains("AII"))
                    newRowEx["AII"] = Cst.AUTOMATIC_COMPUTE;
            }
            newRowEx.EndEdit();
            pRowCum.Table.Rows.Add(newRowEx);
        }
        #endregion InsertDataRowExForTradePostCa
        #region InsertDataRowExRecycled
        /// <summary>
        /// ● Insertion nouveaux DC recyclés
        /// </summary>
        /// <param name="pContract">Contract (DC) courant à recycler</param>
        /// <param name="pRowCum">Row du DC (Cum)</param>
        /// <param name="pNewIdDC">Id du DC Recyclé</param>
        /// <returns></returns>
        private void InsertDataRowExRecycled(CorporateEventContract pContract, DataRow pRowCum, int? pNewIdDC, int? pNewIdAsset_UNL)
        {
            CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];
            Adjustment adjustment = corporateEvent.procedure.adjustment;
            Nullable<bool> resetDerivativeIsinCode = adjustment.GetResetDerivativeIsinCode();

            Nullable<decimal> exContractSize = null;
            Nullable<decimal> exContractMultiplier = null;
            string exContractDisplayName = string.Empty;
            string exContractIsinCode = string.Empty;
            string exContractSymbol = string.Empty;

            AIContract _aiContract = adjustment.GetAIContract(CATools.CAElementTypeEnum.ExRecycled, pContract);
           
            if (null != _aiContract)
            {
                exContractSize = _aiContract.ContractSize;
                exContractMultiplier = _aiContract.ContractMultiplier;
                exContractDisplayName = _aiContract.DisplayName;
                exContractIsinCode = _aiContract.IsinCode;
                exContractSymbol = _aiContract.Symbol;
            }

            // DC actif (DTENABLED = EFFECTIVEDATE du CE)
            DataRow newRow = pRowCum.Table.NewRow();
            newRow.ItemArray = (object[])pRowCum.ItemArray.Clone();
            newRow.BeginEdit();
            if (pNewIdDC.HasValue && pRowCum.Table.Columns.Contains("IDDC"))
                newRow["IDDC"] = pNewIdDC.Value;

            if (pNewIdAsset_UNL.HasValue && pRowCum.Table.Columns.Contains("IDASSET_UNL"))
                newRow["IDASSET_UNL"] = pNewIdAsset_UNL.Value;

            if (pRowCum.Table.Columns.Contains("IDENTIFIER"))
                newRow["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
            if (pRowCum.Table.Columns.Contains("DISPLAYNAME"))
                newRow["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE;
            if (pRowCum.Table.Columns.Contains("DESCRIPTION"))
                newRow["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE;

            if (StrFunc.IsFilled(exContractDisplayName))
                newRow["DESCRIPTION"] += exContractDisplayName;

            // EG 20140107 [19454]
            if (pRowCum.Table.Columns.Contains("AII"))
                newRow["AII"] = Cst.AUTOMATIC_COMPUTE;

            if (StrFunc.IsFilled(exContractSymbol))
                newRow["CONTRACTSYMBOL"] = exContractSymbol;

            if (exContractSize.HasValue && (false == Convert.IsDBNull(newRow["FACTOR"])))
                    newRow["FACTOR"] = exContractSize.Value;

            if (exContractMultiplier.HasValue && (false == Convert.IsDBNull(newRow["CONTRACTMULTIPLIER"])))
                    newRow["CONTRACTMULTIPLIER"] = exContractMultiplier.Value;

            if (pRowCum.Table.Columns.Contains("ISINCODE"))
            {
                bool isResetIsinCode = resetDerivativeIsinCode.HasValue && resetDerivativeIsinCode.Value;
                if (StrFunc.IsFilled(exContractIsinCode))
                    newRow["ISINCODE"] = exContractIsinCode;
                else if (isResetIsinCode)
                    newRow["ISINCODE"] = Convert.DBNull;
            }

            newRow["DTENABLED"] = corporateEvent.effectiveDate;
            newRow["DTDISABLED"] = Convert.DBNull;
            newRow["DTINS"] = m_DtSys;
            newRow["IDAINS"] = m_PKGenProcess.Session.IdA;
            newRow["DTUPD"] = Convert.DBNull;
            newRow["IDAUPD"] = Convert.DBNull;
            newRow.EndEdit();
            pRowCum.Table.Rows.Add(newRow);
        }
        #endregion InsertDataRowExRecycled
        #region InsertDataRowExAdj
        /// <summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCorpoEventSource"></param>
        /// <param name="pRowCum"></param>
        /// <param name="pNewIdDC"></param>
        /// <param name="pNewIdDA"></param>
        /// <param name="pNewIdASSET"></param>
        /// <summary>
        /// Mise à jour des Datarow (CONTRACT / DATTRIB et ASSET)
        /// ● Insertion nouveaux DC (selon marché) , DA et ASSET ajustés (DTENABLED = DTCA)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCorpoEventSource"></param>
        /// <param name="pRowCum"></param>
        /// <param name="pNewIdDC"></param>
        /// <param name="pNewIdDA"></param>
        /// <param name="pNewIdASSET"></param>
        /// <param name="pNewIdAsset_UNL"></param>
        /// <returns></returns>
        /// EG 20130716 No return (void)
        /// EG [33415/33420] New Gestion Sous-jacent ASSET_EQUITY
        /// EG 20141014 Suite CA Merger FIAT/CHRYSLER
        private bool InsertDataRowExAdj<T>(T pCorpoEventSource, DataRow pRowCum, int? pNewIdDC, int? pNewIdDA, int? pNewIdASSET, int? pNewIdAsset_UNL)
        {
            bool isNewRowEx = true;
            CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];
            Adjustment adjustment = corporateEvent.procedure.adjustment;
            Nullable<bool> resetDerivativeIsinCode = adjustment.GetResetDerivativeIsinCode();

            DataRow newRowEx = pRowCum.Table.NewRow();
            newRowEx.ItemArray = (object[])pRowCum.ItemArray.Clone();
            newRowEx.BeginEdit();

            if (pRowCum.Table.Columns.Contains("ISINCODE") && resetDerivativeIsinCode.HasValue && resetDerivativeIsinCode.Value)
                newRowEx["ISINCODE"] = Convert.DBNull;

            newRowEx["DTENABLED"] = corporateEvent.effectiveDate;
            newRowEx["DTDISABLED"] = Convert.DBNull;
            newRowEx["DTINS"] = m_DtSys;
            newRowEx["IDAINS"] = m_PKGenProcess.Session.IdA;
            newRowEx["DTUPD"] = Convert.DBNull;
            newRowEx["IDAUPD"] = Convert.DBNull;

            CalculationExData exData = null;
            Nullable<decimal> exAdjContractSize = null;
            Nullable<decimal> exAdjContractMultiplier = null;
            string exAdjContractDisplayName = string.Empty;
            string exAdjContractIsinCode = string.Empty;
            string exAdjContractSymbol = string.Empty;

            AIContract aiContract;
            if (pCorpoEventSource is CorporateEventContract)
            {
                CorporateEventContract contract = pCorpoEventSource as CorporateEventContract;

                string cumContractSymbol = pRowCum["CONTRACTSYMBOL"].ToString();

                if (pNewIdDC.HasValue)
                    newRowEx["IDDC"] = pNewIdDC.Value;

                if (pNewIdAsset_UNL.HasValue)
                    newRowEx["IDASSET_UNL"] = pNewIdAsset_UNL.Value;

                bool isResetIsinCode = resetDerivativeIsinCode.HasValue && resetDerivativeIsinCode.Value;


                aiContract = adjustment.GetAIContract(CATools.CAElementTypeEnum.ExAdj, contract);

                if (null != aiContract)
                {
                    exAdjContractSize = aiContract.ContractSize;
                    exAdjContractMultiplier = aiContract.ContractMultiplier;
                    exAdjContractDisplayName = aiContract.DisplayName;
                    exAdjContractIsinCode = aiContract.IsinCode;
                    exAdjContractSymbol = aiContract.Symbol;
                }


                if (StrFunc.IsFilled(exAdjContractIsinCode))
                    newRowEx["ISINCODE"] = exAdjContractIsinCode;
                else if (isResetIsinCode)
                    newRowEx["ISINCODE"] = Convert.DBNull;

                exData = contract.exData;
                newRowEx["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
                newRowEx["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE;
                newRowEx["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE;

                if (StrFunc.IsFilled(exAdjContractDisplayName))
                {
                    newRowEx["DESCRIPTION"] += exAdjContractDisplayName;
                }

                // EG 20140429 Si le symbole du contrat EXADJ est différent du CUM alors pas de mise à jour du CONTRACTATTRIBUTE

                // EG 20140320 Additional infos
                if (StrFunc.IsFilled(exAdjContractSymbol) && (cumContractSymbol != exAdjContractSymbol))
                {
                    newRowEx["CONTRACTSYMBOL"] = exAdjContractSymbol;
                    /// EG 20141014 Suite CA Merger FIAT/CHRYSLER
                    // Pas de création de DC Ex si instruction = ##UNDO##
                    if ((false == Convert.IsDBNull(newRowEx["CONTRACTSYMBOL"])) && (newRowEx["CONTRACTSYMBOL"].ToString() == CATools.AI_Undo))
                        isNewRowEx = false;
                }
                else
                {
                    if (null != exData)
                    {
                        switch (m_CorporateEventMktRules.renamingContractMethod)
                        {
                            case CorpoEventRenamingContractMethodEnum.ContractAttribute:
                                newRowEx["CONTRACTATTRIBUTE"] = exData.renamingValue;
                                break;
                            case CorpoEventRenamingContractMethodEnum.SymbolSuffix:
                                newRowEx["CONTRACTSYMBOL"] = exData.renamingValue;
                                break;
                        }
                    }
                }
            }
            else if (pCorpoEventSource is CorporateEventDAttrib)
            {
                CorporateEventDAttrib corpoEventDAttrib = pCorpoEventSource as CorporateEventDAttrib;
                CorporateEventContract contract = corporateEvent[corpoEventDAttrib.idDC];
                aiContract = adjustment.GetAIContract(CATools.CAElementTypeEnum.ExAdj, contract);
                if (null != aiContract)
                {
                    exAdjContractSize = aiContract.ContractSize;
                    exAdjContractMultiplier = aiContract.ContractMultiplier;
                }

                if (pNewIdDA.HasValue)
                    newRowEx["IDDERIVATIVEATTRIB"] = pNewIdDA.Value;
                if (pNewIdDC.HasValue)
                    newRowEx["IDDC"] = pNewIdDC.Value;

                exData = corpoEventDAttrib.exData;

            }
            else if (pCorpoEventSource is CorporateEventAsset)
            {
                CorporateEventAsset corporateEventAsset = pCorpoEventSource as CorporateEventAsset;
                CorporateEventContract contract = corporateEvent[corporateEventAsset.idDC];
                aiContract = adjustment.GetAIContract(CATools.CAElementTypeEnum.ExAdj, contract);
                if (null != aiContract)
                {
                    exAdjContractSize = aiContract.ContractSize;
                    exAdjContractMultiplier = aiContract.ContractMultiplier;
                }

                if (pNewIdASSET.HasValue)
                    newRowEx["IDASSET"] = pNewIdASSET.Value;
                if (pNewIdDA.HasValue)
                    newRowEx["IDDERIVATIVEATTRIB"] = pNewIdDA.Value;
                exData = corporateEventAsset.exData;
                newRowEx["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
                newRowEx["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE;
                newRowEx["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE;

                // EG 20140107 [19454]
                if (pRowCum.Table.Columns.Contains("AII"))
                    newRowEx["AII"] = Cst.AUTOMATIC_COMPUTE;

                if (((pCorpoEventSource as CorporateEventAsset).category == CfiCodeCategoryEnum.Option) && (null != exData))
                    newRowEx["STRIKEPRICE"] = exData.strikePrice.valueRounded.DecValue;
            }

            if ((null == exData) && (adjustment is AdjustmentFairValue))
            {
                isNewRowEx = false;
            }
            else
            {
                // EG 20140518 Test IsDBNull sur CONTRACTMULTIPLIER et FACTOR
                if (m_CorporateEventMktRules.renamingContractMethod == CorpoEventRenamingContractMethodEnum.None)
                {
                    if (exAdjContractMultiplier.HasValue && (false == Convert.IsDBNull(newRowEx["CONTRACTMULTIPLIER"])))
                        newRowEx["CONTRACTMULTIPLIER"] = exAdjContractMultiplier.Value;
                    else if (exData.contractMultiplierSpecified)
                        newRowEx["CONTRACTMULTIPLIER"] = exData.contractMultiplier.valueRounded.DecValue;

                    if (exAdjContractSize.HasValue && (false == Convert.IsDBNull(newRowEx["FACTOR"])))
                        newRowEx["FACTOR"] = exAdjContractSize.Value;
                    else if (exData.contractSizeSpecified)
                        newRowEx["FACTOR"] = exData.contractSize.valueRounded.DecValue;
                }
                else
                {
                    if ((false == Convert.IsDBNull(newRowEx["CONTRACTMULTIPLIER"])))
                        newRowEx["CONTRACTMULTIPLIER"] = (exAdjContractMultiplier ?? exData.contractMultiplier.valueRounded.DecValue);

                    if ((false == Convert.IsDBNull(newRowEx["FACTOR"])))
                        newRowEx["FACTOR"] = (exAdjContractSize ?? exData.contractSize.valueRounded.DecValue);
                }
            }

            if (isNewRowEx)
            {
                newRowEx.EndEdit();
                pRowCum.Table.Rows.Add(newRowEx);
            }
            else
                newRowEx.CancelEdit();
            return isNewRowEx;

        }
        #endregion InsertDataRowExAdj

        #region AdjustmentWriting
        /// <summary>
        /// Ecriture des résultats dans les tables : CORPOEVENTCONTRACT,CORPOEVENTDATTRIB et CORPOEVENTASSET 
        /// ● R-Factor
        /// ● Eléments(CUM et EX) : ContractSize (Factor), ContractMultiplier, StrikePrice, DailySettlementPrice
        /// ● EqualisationPayment : Soulte (unitaire)
        /// </summary>
        /// <returns></returns>
        /// EG 20140317 [19722]
        private Cst.ErrLevel AdjustmentWriting()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            if (m_CorporateAction.corporateEvent[0].contractsSpecified)
            {
                CorporateEventContract[] contracts = m_CorporateAction.corporateEvent[0].contracts;
                DCQueryEMBEDDED dcQry = new DCQueryEMBEDDED(CS);
                IDbTransaction dbTransaction = null;
                try
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    foreach (CorporateEventContract contract in contracts)
                    {
                        QueryParameters dcQryParameters;
                        switch (contract.rowState)
                        {
                            case DataRowState.Added:
                                int newIdCEC = 0;
                                SQLUP.GetId(out newIdCEC, dbTransaction, SQLUP.IdGetId.CORPOEVENTCONTRACT, SQLUP.PosRetGetId.First, 1);
                                contract.IdCEC = newIdCEC;
                                CATools.ExRenaming(m_CorporateEventMktRules, contract);
                                dcQryParameters = dcQry.GetQueryInsert();
                                dcQry.SetParametersInsert(contract, dcQryParameters.Parameters);
                                break;
                            case DataRowState.Modified:
                                CATools.ExRenaming(m_CorporateEventMktRules, contract);
                                dcQryParameters = dcQry.GetQueryUpdate();
                                dcQry.SetParametersUpdate(contract, dcQryParameters.Parameters);
                                break;
                            case DataRowState.Deleted:
                                dcQryParameters = dcQry.GetQueryDelete();
                                dcQry.SetParametersDelete(contract, dcQryParameters.Parameters);
                                break;
                            case DataRowState.Detached:
                            default:
                                continue;
                        }
                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, dcQryParameters.Query, dcQryParameters.Parameters.GetArrayDbParameter());

                        if (contract.dAttribsSpecified)
                        {
                            DAQueryEMBEDDED dadQry = new DAQueryEMBEDDED(CS);
                            QueryParameters daQryParameters = null;

                            foreach (CorporateEventDAttrib dAttrib in contract.dAttribs)
                            {
                                switch (dAttrib.rowState)
                                {
                                    case DataRowState.Added:
                                        if (0 == contract.IdCEC)
                                            contract.SetID(dbTransaction);
                                        dAttrib.idCEC = contract.IdCEC;
                                        daQryParameters = dadQry.GetQueryInsert();
                                        dadQry.SetParametersInsert(dAttrib, daQryParameters.Parameters);
                                        break;
                                    case DataRowState.Modified:
                                        daQryParameters = dadQry.GetQueryUpdate();
                                        dadQry.SetParametersUpdate(dAttrib, daQryParameters.Parameters);
                                        break;
                                    case DataRowState.Deleted:
                                        daQryParameters = dadQry.GetQueryDelete();
                                        dadQry.SetParametersDelete(dAttrib, daQryParameters.Parameters);
                                        break;
                                }
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, daQryParameters.Query, daQryParameters.Parameters.GetArrayDbParameter());

                                if (dAttrib.assetsSpecified)
                                {
                                    ETDQueryEMBEDDED etdQry = new ETDQueryEMBEDDED(CS);
                                    QueryParameters etdQryParameters = null;

                                    foreach (CorporateEventAsset asset in dAttrib.assets)
                                    {
                                        switch (asset.rowState)
                                        {
                                            case DataRowState.Added:
                                                if (0 == contract.IdCEC)
                                                    contract.SetID(dbTransaction);
                                                asset.idCEC = contract.IdCEC;
                                                etdQryParameters = etdQry.GetQueryInsert();
                                                etdQry.SetParametersInsert(asset, etdQryParameters.Parameters);
                                                break;
                                            case DataRowState.Modified:
                                                etdQryParameters = etdQry.GetQueryUpdate();
                                                etdQry.SetParametersUpdate(asset, etdQryParameters.Parameters);
                                                break;
                                            case DataRowState.Deleted:
                                                etdQryParameters = etdQry.GetQueryDelete();
                                                etdQry.SetParametersDelete(asset, etdQryParameters.Parameters);
                                                break;
                                        }
                                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, etdQryParameters.Query, etdQryParameters.Parameters.GetArrayDbParameter());
                                    }
                                }
                            }
                        }
                    }
                    DataHelper.CommitTran(dbTransaction);
                    dbTransaction = null;
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
                catch (Exception)
                {
                    codeReturn = Cst.ErrLevel.FAILURE;
                    throw;
                }
                finally
                {
                    if ((null != dbTransaction) && (Cst.ErrLevel.FAILURE == codeReturn))
                    {
                        try { DataHelper.RollbackTran(dbTransaction); }
                        catch { }
                    }
                }
            }
            return codeReturn;
        }
        #endregion AdjustmentWriting

        #region LoadDerivativeContractCandidates
        /// <summary>
        /// Récupératiohn des DCs candidats potentiels à CA (Step EOD)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20150114 [20676] Add FutureValuationMethodEnum
        // EG 20220621 [34623] Usage du using sur DataReader
        protected Cst.ErrLevel LoadDerivativeContractCandidates()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            try
            {
                // Recherche des DCs concernés par la CA
                CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
                CorporateEventUnderlyer _underlyer = _corporateEvent.procedure.underlyers[0];
                bool isFutureSpecified = true;
                bool isOptionSpecified = true;
                if (_corporateEvent.procedure.adjustment is AdjustmentRatio)
                {
                    AdjustmentRatio ratio = _corporateEvent.procedure.adjustment as AdjustmentRatio;
                    isFutureSpecified = ratio.contract.futureSpecified;
                    isOptionSpecified = ratio.contract.optionSpecified;
                }

                DCQueryEMBEDDED dcQry = new DCQueryEMBEDDED(CS);
                QueryParameters qryParameters = dcQry.GetQuerySelectCandidate();
                DataParameters parameters = qryParameters.Parameters;
                parameters["IDCE"].Value = _corporateEvent.IdCE;
                parameters["IDA_ENTITY"].Value = m_EntityMarketInfo.IdA_Entity;
                parameters["IDM"].Value = m_PosRequest.IdM;
                parameters["EFFECTIVEDATE"].Value = _corporateEvent.effectiveDate;
                parameters["IDASSET_UNL"].Value = _underlyer.SpheresID;
                parameters["ASSETCATEGORY_UNL"].Value = _underlyer.category.ToString();
                parameters["CATEGORY"].Value = m_CorporateAction.cfiCodeSpecified ? ReflectionTools.ConvertEnumToString<CfiCodeCategoryEnum>(m_CorporateAction.cfiCode) : Convert.DBNull;
                using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (null != dr)
                    {
                        ArrayList aContract = new ArrayList();
                        // Insertion - Mise à jour des DCs concernés par la CA
                        CorporateEventContract _dc = null;
                        while (dr.Read())
                        {
                            #region Alimentation de la classe CorporateEventContract
                            _dc = new CorporateEventContract();
                            _dc.rowState = (DataRowState)ReflectionTools.EnumParse(_dc.rowState, dr["ROWSTATE"].ToString());
                            _dc.idCE = _corporateEvent.IdCE;
                            _dc.category = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(_dc.category, dr["CATEGORY"].ToString());
                            _dc.idA_Entity = m_EntityMarketInfo.IdA_Entity;
                            _dc.assetCategory_UNL = _underlyer.category;
                            _dc.idAsset_UNL = _underlyer.SpheresID;
                            _dc.adjMethod = _corporateEvent.adjMethod;
                            _dc.readyState = CorporateEventReadyStateEnum.REQUESTED;
                            _dc.adjStatus = CATools.AdjStatusEnum.REQUESTED;
                            // EG 20150114 [20676] 
                            _dc.futValuationMethod = (FuturesValuationMethodEnum)ReflectionTools.EnumParse(_dc.futValuationMethod, dr["FUTVALUATIONMETHOD"].ToString());

                            // EG 20190121 [23249] New pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
                            _dc.cashFlowCalcMethod = CashFlowCalculationMethodEnum.OVERALL;
                            if (false == Convert.IsDBNull(dr["CASHFLOWCALCMETHOD"]))
                                _dc.cashFlowCalcMethod = (CashFlowCalculationMethodEnum)ReflectionTools.EnumParse(_dc.cashFlowCalcMethod, dr["CASHFLOWCALCMETHOD"].ToString());

                            if (false == Convert.IsDBNull(dr["CONTRACTSYMBOL"]))
                            {
                                _dc.contractSymbol = dr["CONTRACTSYMBOL"].ToString();
                                _dc.contractSymbolSpecified = true;
                            }

                            if (false == Convert.IsDBNull(dr["CONTRACTATTRIBUTE"]))
                            {
                                _dc.contractAttribute = dr["CONTRACTATTRIBUTE"].ToString();
                                _dc.contractAttributeSpecified = true;
                            }
                            _dc.cumData = new CalculationCumData();

                            _dc.idDC = Convert.ToInt32(dr["IDDC"]);
                            if (false == Convert.IsDBNull(dr["IDENTIFIER"]))
                            {
                                _dc.identifierSpecified = true;
                                _dc.identifier = dr["IDENTIFIER"].ToString();
                            }
                            if (false == Convert.IsDBNull(dr["CONTRACTSIZE"]))
                            {
                                _dc.cumData.contractSize = new EFS_Decimal(dr["CONTRACTSIZE"].ToString());
                                _dc.cumData.contractSizeSpecified = true;
                            }
                            if (false == Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]))
                            {
                                _dc.cumData.contractMultiplier = new EFS_Decimal(dr["CONTRACTMULTIPLIER"].ToString());
                                _dc.cumData.contractMultiplierSpecified = true;
                            }
                            if (false == Convert.IsDBNull(dr["PRICEDECLOCATOR"]))
                            {
                                _dc.priceDecLocator = new EFS_Integer(dr["PRICEDECLOCATOR"].ToString());
                                _dc.priceDecLocatorSpecified = true;
                            }
                            if (false == Convert.IsDBNull(dr["STRIKEDECLOCATOR"]))
                            {
                                _dc.strikeDecLocator = new EFS_Integer(dr["STRIKEDECLOCATOR"].ToString());
                                _dc.strikeDecLocatorSpecified = true;
                            }
                            #endregion Alimentation de la classe CorporateEventContract

                            // EG 20130719 Exclusion des DC non candidats en fonction des checks dans la procédure.
                            #region Exclure les catégories de contrats non candidates
                            if (_dc.rowState != DataRowState.Deleted)
                            {
                                if (((_dc.category == CfiCodeCategoryEnum.Future) && (false == isFutureSpecified)) ||
                                    ((_dc.category == CfiCodeCategoryEnum.Option) && (false == isOptionSpecified)))
                                {
                                    if (_dc.rowState == DataRowState.Added)
                                        _dc.rowState = DataRowState.Detached;
                                    else if (_dc.rowState == DataRowState.Modified)
                                        _dc.rowState = DataRowState.Deleted;
                                }
                            }
                            #endregion Exclure les catégories de contrats non candidates

                            if ((_dc.rowState == DataRowState.Added) || (_dc.rowState == DataRowState.Modified))
                            {
                                if (Cst.ErrLevel.SUCCESS != LoadDerivativeAttribCandidates(_dc))
                                    codeReturn = Cst.ErrLevel.FAILURE;
                            }
                            aContract.Add(_dc);
                        }
                        _corporateEvent.contractsSpecified = (0 < aContract.Count);
                        _corporateEvent.contracts = (CorporateEventContract[])aContract.ToArray(typeof(CorporateEventContract));
                        if (_corporateEvent.contractsSpecified)
                            codeReturn = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return codeReturn;
        }
        #endregion LoadDerivativeContractCandidates
        #region LoadDerivativeAttribCandidates
        /// <summary>
        /// Récupération des DA associés à un DC candidat à CA (Step EOD)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20150114 [20676] Add FutureValuationMethodEnum
        // EG 20220621 [34623] Usage du using sur DataReader
        protected Cst.ErrLevel LoadDerivativeAttribCandidates(CorporateEventContract pCorporateEventContract)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            try
            {
                // Recherche des DCs concernés par la CA
                CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
                DAQueryEMBEDDED daQry = new DAQueryEMBEDDED(CS);
                QueryParameters qryParameters = daQry.GetQuerySelectCandidate();
                DataParameters parameters = qryParameters.Parameters;
                parameters["IDCE"].Value = pCorporateEventContract.idCE;
                parameters["IDDC"].Value = pCorporateEventContract.idDC;
                parameters["IDA_ENTITY"].Value = pCorporateEventContract.idA_Entity;
                parameters["EFFECTIVEDATE"].Value = _corporateEvent.effectiveDate;

                using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter()))
                {
                    if (null != dr)
                    {
                        ArrayList aDerivativeAttrib = new ArrayList();
                        // Insertion - Mise à jour des DCs concernés par la CA
                        CorporateEventDAttrib _da = null;
                        while (dr.Read())
                        {
                            _da = new CorporateEventDAttrib();

                            _da.rowState = (DataRowState)ReflectionTools.EnumParse(_da.rowState, dr["ROWSTATE"].ToString());

                            _da.idDA = Convert.ToInt32(dr["IDDA"]);
                            _da.idCE = pCorporateEventContract.idCE;
                            _da.idDC = pCorporateEventContract.idDC;
                            _da.idA_Entity = pCorporateEventContract.idA_Entity;

                            _da.category = pCorporateEventContract.category;
                            _da.adjMethod = _corporateEvent.adjMethod;
                            _da.readyState = CorporateEventReadyStateEnum.REQUESTED;
                            _da.adjStatus = CATools.AdjStatusEnum.REQUESTED;

                            // EG 20150114 [20676] 
                            _da.futValuationMethod = pCorporateEventContract.futValuationMethod;

                            // EG 20190121 [23249] New pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
                            _da.cashFlowCalcMethod = pCorporateEventContract.cashFlowCalcMethod;


                            #region Alimentation de la classe CorporateEventAsset (CUM)
                            _da.cumData = new CalculationCumData();

                            if (false == Convert.IsDBNull(dr["CONTRACTSIZE"]))
                            {
                                _da.cumData.contractSize = new EFS_Decimal(dr["CONTRACTSIZE"].ToString());
                                _da.cumData.contractSizeSpecified = true;
                            }
                            if (false == Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]))
                            {
                                _da.cumData.contractMultiplier = new EFS_Decimal(dr["CONTRACTMULTIPLIER"].ToString());
                                _da.cumData.contractMultiplierSpecified = true;
                            }
                            #endregion Alimentation de la classe CorporateEventAsset (CUM)


                            _da.priceDecLocator = pCorporateEventContract.priceDecLocator;
                            _da.priceDecLocatorSpecified = pCorporateEventContract.priceDecLocatorSpecified;
                            _da.strikeDecLocator = pCorporateEventContract.strikeDecLocator;
                            _da.strikeDecLocatorSpecified = pCorporateEventContract.strikeDecLocatorSpecified;

                            if ((_da.rowState == DataRowState.Added) || (_da.rowState == DataRowState.Modified))
                            {
                                if (Cst.ErrLevel.SUCCESS != LoadAssetCandidates(_da))
                                    codeReturn = Cst.ErrLevel.FAILURE;

                            }
                            aDerivativeAttrib.Add(_da);
                        }
                        pCorporateEventContract.dAttribsSpecified = (0 < aDerivativeAttrib.Count);
                        pCorporateEventContract.dAttribs = (CorporateEventDAttrib[])aDerivativeAttrib.ToArray(typeof(CorporateEventDAttrib));
                        codeReturn = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return codeReturn;
        }
        #endregion LoadDerivativeAttribCandidates
        #region LoadAssetCandidates
        /// <summary>
        /// Récupératiohn des Assets associés à un DC candidat à CA (Step EOD)
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20150114 [20676] Add FutureValuationMethodEnum
        // EG 20220621 [34623] Usage du using sur DataReader
        protected Cst.ErrLevel LoadAssetCandidates(CorporateEventDAttrib pCorporateEventDAttrib)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.UNDEFINED;
            try
            {
                // Recherche des DCs concernés par la CA
                CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
                ETDQueryEMBEDDED etdQry = new ETDQueryEMBEDDED(CS);
                QueryParameters qryParameters = etdQry.GetQuerySelectCandidate();
                DataParameters parameters = qryParameters.Parameters;
                parameters["IDCE"].Value = pCorporateEventDAttrib.idCE;
                parameters["IDDA"].Value = pCorporateEventDAttrib.idDA;
                parameters["IDA_ENTITY"].Value = pCorporateEventDAttrib.idA_Entity;
                parameters["EFFECTIVEDATE"].Value = _corporateEvent.effectiveDate;

                using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter()))
                {
                    if (null != dr)
                    {
                        ArrayList aAsset = new ArrayList();
                        // Insertion - Mise à jour des DCs concernés par la CA
                        CorporateEventAsset _asset = null;
                        while (dr.Read())
                        {
                            _asset = new CorporateEventAsset();

                            _asset.rowState = (DataRowState)ReflectionTools.EnumParse(_asset.rowState, dr["ROWSTATE"].ToString());

                            _asset.idASSET = Convert.ToInt32(dr["IDASSET"]);
                            _asset.idCE = pCorporateEventDAttrib.idCE;
                            _asset.idDA = pCorporateEventDAttrib.idDA;
                            _asset.idDC = pCorporateEventDAttrib.idDC;
                            _asset.idA_Entity = pCorporateEventDAttrib.idA_Entity;

                            _asset.category = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(_asset.category, dr["CATEGORY"].ToString());
                            _asset.adjMethod = _corporateEvent.adjMethod;
                            _asset.readyState = CorporateEventReadyStateEnum.REQUESTED;
                            _asset.adjStatus = CATools.AdjStatusEnum.REQUESTED;

                            _asset.priceDecLocator = pCorporateEventDAttrib.priceDecLocator;
                            _asset.priceDecLocatorSpecified = pCorporateEventDAttrib.priceDecLocatorSpecified;
                            _asset.strikeDecLocator = pCorporateEventDAttrib.strikeDecLocator;
                            _asset.strikeDecLocatorSpecified = pCorporateEventDAttrib.strikeDecLocatorSpecified;

                            // EG 20150114 [20676] 
                            _asset.futValuationMethod = pCorporateEventDAttrib.futValuationMethod;

                            // EG 20190121 [23249] New pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
                            _asset.cashFlowCalcMethod = pCorporateEventDAttrib.cashFlowCalcMethod;

                            #region Alimentation de la classe CorporateEventAsset (CUM)
                            _asset.cumData = new CalculationCumData();

                            if (false == Convert.IsDBNull(dr["IDENTIFIER"]))
                            {
                                _asset.identifierSpecified = true;
                                _asset.identifier = dr["IDENTIFIER"].ToString();
                            }
                            if (false == Convert.IsDBNull(dr["CONTRACTSIZE"]))
                            {
                                _asset.cumData.contractSize = new EFS_Decimal(dr["CONTRACTSIZE"].ToString());
                                _asset.cumData.contractSizeSpecified = true;
                            }
                            if (false == Convert.IsDBNull(dr["CONTRACTMULTIPLIER"]))
                            {
                                _asset.cumData.contractMultiplier = new EFS_Decimal(dr["CONTRACTMULTIPLIER"].ToString());
                                _asset.cumData.contractMultiplierSpecified = true;
                            }
                            if (false == Convert.IsDBNull(dr["STRIKEPRICE"]))
                            {
                                _asset.cumData.strikePrice = new EFS_Decimal(dr["STRIKEPRICE"].ToString());
                                _asset.cumData.strikePriceSpecified = true;
                            }
                            #endregion Alimentation de la classe CorporateEventAsset (CUM)
                            aAsset.Add(_asset);
                        }
                        pCorporateEventDAttrib.assetsSpecified = (0 < aAsset.Count);
                        pCorporateEventDAttrib.assets = (CorporateEventAsset[])aAsset.ToArray(typeof(CorporateEventAsset));
                        codeReturn = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return codeReturn;
        }
        #endregion LoadAssetCandidates


        #region LoadCATables
        /// <summary>
        /// Chargement des candidats (CONTRACT / DATTRIB ou ASSET)
        /// </summary>
        /// <param name="pTransaction"></param>
        /// <param name="pElement">Candidat</param>
        /// <param name="pTableName">Nom de mapping de la datatable</param>
        /// <param name="pIdCE">Identifiant CE de la CA</param>
        /// <param name="pIdA_Entity">Entité</param>
        /// <param name="pDs">Dataset qui réceptionne le Fill</param>
        /// <returns></returns>
        private DataAdapter LoadCATable(IDbTransaction pDbTransaction, DataContractResultSets pElement, Cst.OTCml_TBL pTableName,
            int pIdCE, int pIdA_Entity, int pIdDC, ref DataSet pDs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            // EG 20160105 [34091] Add Switch 
            switch (pElement)
            {
                case DataContractResultSets.CA_DERIVATIVECONTRACT:
                case DataContractResultSets.CA_DERIVATIVEATTRIB:
                case DataContractResultSets.CA_ASSET:
                    parameters.Add("IDCE", pIdCE);
                    parameters.Add("IDA_ENTITY", pIdA_Entity);
                    parameters.Add("IDDC", pIdDC);
                    // RD 20150813 [21241] Valoriser le paramètre EFFECTIVEDATE
                    parameters.Add("EFFECTIVEDATE", (m_CorporateAction.corporateEvent[0].effectiveDateSpecified ? m_CorporateAction.corporateEvent[0].effectiveDate : Convert.DBNull));
                    break;
                case DataContractResultSets.CA_CORPOEVENTCONTRACT:
                case DataContractResultSets.CA_CORPOEVENTDATTRIB:
                case DataContractResultSets.CA_CORPOEVENTASSET:
                    parameters.Add("IDCE", pIdCE);
                    parameters.Add("IDA_ENTITY", pIdA_Entity);
                    break;
            }
            //parameters.Add("IDCE", pIdCE);
            //parameters.Add("IDA_ENTITY", pIdA_Entity);
            //parameters.Add("IDDC", pIdDC);
            //// RD 20150813 [21241] Valoriser le paramètre EFFECTIVEDATE
            //parameters.Add("EFFECTIVEDATE", (m_CorporateAction.corporateEvent[0].effectiveDateSpecified ? m_CorporateAction.corporateEvent[0].effectiveDate : Convert.DBNull));

            DataAdapter _da = DataHelper.GetDataAdapter(pDbTransaction, DataContractHelper.GetQuery(pElement),
                DataContractHelper.GetDbDataParameters(pElement, parameters), pTableName.ToString(), ref pDs, out _);
            return _da;
        }
        #endregion LoadCATables
        #region LoadCAUNLTables
        /// <summary>
        /// Chargement des sous-jacents de CA
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pElement">Table asset sous-jacent</param>
        /// <param name="pTableName">Nom de mapping de la datatable</param>
        /// <param name="pIdCE">Identifiant du sous-jacent (CUM)</param>
        /// <param name="pIdM">Marché du sous-jacent</param>
        /// <param name="pIdAsset_UNL">Id du Sous-jacent</param>
        /// <param name="pExIsinCode">Nouveau code isin du sous-jacent</param>
        /// <param name="pExLstIsinCode">Nouveau code isin du sous-jacent (Basket - Merger - DeMerge)</param>
        /// <param name="pNewIsinCode">Nouveau Code ISIN (EX)</param>
        /// <param name="pDs">Dataset qui réceptionne le Fill</param>
        /// <returns></returns>
        private DataAdapter LoadCAUNLTables(IDbTransaction pDbTransaction, DataContractResultSets pElement, Cst.OTCml_TBL pTableName,
            int pIdM, int pIdAsset_UNL, List<string> pExLstIsinCode, ref DataSet pDs)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "IDM", pIdM },
                { "IDASSET", pIdAsset_UNL }
            };

            string sqlSelect = DataContractHelper.GetQuery(pElement);
            // Listes des sous-jacents (ISIN codes) acteurs de la CA
            string lstIsinCode = string.Empty;
            if ((null != pExLstIsinCode) && (0 < pExLstIsinCode.Count))
            {
                pExLstIsinCode.ForEach(item => lstIsinCode += DataHelper.SQLString(item) + ",");
                lstIsinCode = lstIsinCode.Substring(0, lstIsinCode.Length - 1);
            }
            // EG 20160105 [34091] Replace @LSTISINCODE by ##LSTISINCODE##
            sqlSelect = sqlSelect.Replace("##LSTISINCODE##", lstIsinCode);

            DataAdapter _da = DataHelper.GetDataAdapter(pDbTransaction, sqlSelect, DataContractHelper.GetDbDataParameters(pElement, parameters),
                pTableName.ToString(), ref pDs, out IDbCommand _command);
            return _da;
        }
        #endregion LoadCAUNLTables


        #region GetQueryTradeOpenInterest
        /// <summary>
        /// Construit le requête retournant les trades en positions impacté par exécution d'une CA.
        /// </summary>
        /// <param name="pCS">Connexion string</param>
        /// <returns>La requête construite</returns>
        /// EG 20140411 Ajout DTBUSINESS parameter (pour ne PLUS prendre les TRADES > DTBUSINESS
        //  EG 20170412 [23081] Refactoring : Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        // EG 20220206 [26212][WI523] Ajout jointure pour retrancher la clôture effectuée par la CA en cours (en cas de retraitement de la journée)
        private DataSet GetQueryTradeOpenInterest(IDbTransaction pDbTransaction, int pIdASSET)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_EntityMarketInfo.IdEM);
            parameters.Add(DataParameter.GetParameter (CS, DataParameter.ParameterEnum.DTPOS), m_CorporateAction.corporateEvent[0].effectiveDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pIdASSET);
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness); // FI 20201006 [XXXXX] DbType.Date

            // PM 20150601 [20575] Ajout de DTENTITY
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) - isnull(pos_ca.QTY_BUY,0) - isnull(pos_ca.QTY_SELL,0)) as QTY, 
            tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDEM, tr.IDI, tr.IDASSET, tr.DTMARKET, tr.DTENTITY, 
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.SIDE, 
            ev.IDE as IDE_EVENT, ast.IDC, tr.ASSETCATEGORY, tr.IDA_CSSCUSTODIAN as IDA_CSS, 0 as ISCUSTODIAN
            from dbo.VW_TRADE_POSETD tr
            inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE in ('FUT','CAL','PUT'))" + Cst.CrLf;

            // Achats Clôturés
            sqlSelect += SQLCst.X_LEFT + "(" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER) + ") pab on (pab.IDT = tr.IDT)" + Cst.CrLf;
            // Ventes Clôturées
            sqlSelect += SQLCst.X_LEFT + "(" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER) + ") pas on (pas.IDT = tr.IDT)" + Cst.CrLf;

            // Clôturés par la CA courante (en cas de plantage sur la clôture de journée et retraitement
            sqlSelect += @"left outer join (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
                where (pa.DTBUSINESS = @DTPOS) and (pad.DTCAN is null or (pad.DTCAN > @DTPOS)) and (pr.REQUESTTYPE = 'CORPOACTION')
                group by pad.IDT_BUY

                union all

                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
                where (pa.DTBUSINESS = @DTPOS) and (pad.DTCAN is null or (pad.DTCAN > @DTPOS)) and (pr.REQUESTTYPE = 'CORPOACTION')
                group by pad.IDT_SELL
            ) pos_ca on (pos_ca.IDT = tr.IDT)" + Cst.CrLf;

            sqlSelect += @"where ((tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) - isnull(pos_ca.QTY_BUY,0) - isnull(pos_ca.QTY_SELL,0)) <> 0) and 
            (tr.POSKEEPBOOK_DEALER = 1) and (tr.IDASSET = @IDASSET) and (tr.IDEM = @IDEM) and (tr.DTBUSINESS <= @DTBUSINESS)";

            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            return DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter());
        }
        #endregion GetQueryTradeOpenInterest

        #region AdjustmentTradeByTrade
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCorporateEventAsset"></param>
        /// <param name="pLstTradeEx"></param>
        /// <returns></returns>
        // EG 20150114 [20676] Test si FutureStyleMarkToMarket
        // PM 20170911 [23408] Modification du type de pLstTradeEx la liste : List<Pair<int, string>> vers List<CATradeEx>
        // EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private Cst.ErrLevel AdjustmentTradeByTrade(IDbTransaction pDbTransaction, CorporateEventAsset pCorporateEventAsset, ref List<CATradeEx> pLstTradeEx)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DataSet ds = GetQueryTradeOpenInterest(pDbTransaction, pCorporateEventAsset.idASSET);
            IPosRequest _posRequestCA = null;
            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                if (0 < nbRow)
                {
                    m_PKGenProcess.ProcessCacheContainer.DbTransaction = pDbTransaction;

                    // EG 20140103 Test sur pCorporateEventAsset.idASSETEXADJ
                    // 0. Creation du Daily Settlement price (ajusté) pour l'asset ajusté
                    // EG 20150114 [20676] 
                    if (pCorporateEventAsset.cumData.dailyClosingPriceSpecified && (0 < pCorporateEventAsset.idASSETEXADJ) &&
                        (pCorporateEventAsset.futValuationMethod == FuturesValuationMethodEnum.FuturesStyleMarkToMarket))
                        codeReturn = CreateDailySettlementPriceAdjusted(pDbTransaction, pCorporateEventAsset);

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        try
                        {
                            // Insertion dans POSREQUEST d'une ligne pour chaque trade  (POSREQUESTTYPE = CORPOACTION)
                            _posRequestCA = AddPosRequestCorporateActionTrade(pDbTransaction, Cst.PosRequestTypeEnum.CorporateAction, dr, m_PosRequest);


                            // Pour chaque trade en position :
                            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, _posRequestCA.IdT)
                            {
                                IsWithTradeXML = true,
                                DbTransaction = pDbTransaction,
                                IsAddRowVersion = true
                            };
                            // RD 20180108 [23705] Ajouter les colonnes "date" 
                            // RD 20210304 Add "trx."
                            if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDENTIFIER", "trx.TRADEXML", "DTTRADE", "DTBUSINESS", "DTEXECUTION", "DTORDERENTERED", "DTTIMESTAMP", "TZFACILITY" }))
                            {
                                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                                IDataDocument _dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                                DataDocumentContainer _dataDocContainer = (DataDocumentContainer)new DataDocumentContainer(_dataDoc).Clone();

                                // RD 20180108 [23705] Add
                                // EG 20240531 [WI926] Add Parameter pIsTemplate
                                _dataDocContainer.UpdateMissingTimestampAndFacility(CS, sqlTrade, false);

                                // 1. Clôture du trade sur l'asset en cours (pas de dégagement de résult mais soulte possible)
                                string idC = dr["IDC"].ToString();
                                int idE_Event = Convert.ToInt32(dr["IDE_EVENT"]);
                                bool isDealerBuyer = IsTradeBuyer(dr["SIDE"].ToString());

                                // PM 20170911 [23408] Ajout paramètre out equalisationPayment
                                codeReturn = OffSettingTradeCum(pDbTransaction, (IPosRequestCorporateAction)_posRequestCA, _dataDocContainer, pCorporateEventAsset, 
                                    isDealerBuyer, idE_Event, idC, out CAEqualisationPaymentEvent equalisationPayment);

                                // 2. Création d'un trade identique sur l'ASSET ajusté ou non en fonction de RenaminMethod du contrat (DCex et/ou DAex et ASSETex) 
                                // PM 20170911 [23408] Ajout paramètre equalisationPayment
                                codeReturn = CreateExTrade(pDbTransaction, (IPosRequestCorporateAction)_posRequestCA, _dataDocContainer, pCorporateEventAsset, ref pLstTradeEx, equalisationPayment);
                            }
                            else
                            {
                                codeReturn = Cst.ErrLevel.FAILURE;
                                
                                // FI 20200623 [XXXXX] Call SetErrorWarning
                                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 9052), 2,
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                                    new LogParam(m_MarketPosRequest.GroupProductValue),
                                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                                    new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                                    new LogParam(m_CorporateAction.refNotice.value),
                                    new LogParam(m_CorporateAction.corporateEvent[0].adjMethod),
                                    new LogParam(LogTools.IdentifierAndId(_posRequestCA.Identifiers.Trade, _posRequestCA.IdT))));
                            }
                        }
                        catch (Exception)
                        {
                            codeReturn = Cst.ErrLevel.FAILURE;
                            throw;
                        }
                        finally
                        {
                            // Update POSREQUEST GROUP
                            UpdatePosRequest(codeReturn, _posRequestCA, m_PosRequest.IdPR);
                        }
                    }
                }
                else
                {

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.SYS, 9007), 2,
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                        new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                        new LogParam(m_CorporateAction.refNotice.value),
                        new LogParam(m_CorporateAction.corporateEvent[0].adjMethod.ToString())));

                    codeReturn = Cst.ErrLevel.NOTHINGTODO;
                }
            }
            return codeReturn;
        }
        #endregion AdjustmentTradeByTrade
        #region AdjustmentTrades
        /// <summary>
        /// Ajustement des trades.
        /// ● Clôture des trades en position
        /// ● Mise à jour DTCA (Date de Corporate action = DTDISABLED) pour DC, DA et ASSET
        /// ● Insertion nouveaux DC, DA et ASSET non ajustés (DTENABLED = DTCA)
        /// ● Insertion nouveaux DC (selon marché) , DA et ASSET ajustés (DTENABLED = DTCA)
        /// ● Insertion nouveaux trade (Insertion de position) sur ASSET ajustés (DTTRANSAC = DTBUSINESS = DTCA)
        /// </summary>
        /// <returns></returns>
        /// EG [33415/33420]
        /// EG 20140518 [19913] New Execution Script PRECEDING/FOLLOWING
        /// EG 20141014 Suite CA Merger FIAT/CHRYSLER
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20220621 [34623] Réutilisation des Ids des contrats, dAttrib et Asset ayant déjà été affecté par la CA (sur multi-entité) (Maj Restriction DTDISABLED)
        public Cst.ErrLevel AdjustmentTrades()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;

            DataSet ds = null;
            CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
            Adjustment adjustment = _corporateEvent.procedure.adjustment;
            CorporateEventUnderlyer underlyer = null;
            IEnumerable<CorporateEventUnderlyer> underlyers = _corporateEvent.procedure.underlyers.Where(item => item.category == Cst.UnderlyingAsset_ETD.EquityAsset);
            if (0 < underlyers.Count())
                underlyer = underlyers.First();

            try
            {
                DateTime dtCA = _corporateEvent.effectiveDate;
                // EG 20130716 on trie les contrats par n° de version descendants (ceci afin de désactiver un contrat rectifié avant une nouvelle rectification
                List<CorporateEventContract> lstContracts = _corporateEvent.contracts.Where(
                    contract => contract.rowState == DataRowState.Added || contract.rowState == DataRowState.Modified).ToList();
                switch (m_CorporateEventMktRules.renamingContractMethod)
                {
                    case CorpoEventRenamingContractMethodEnum.ContractAttribute:
                        lstContracts = lstContracts.OrderByDescending(item => item.contractSymbol).OrderByDescending(item => item.contractAttribute).ToList();
                        break;
                    case CorpoEventRenamingContractMethodEnum.SymbolSuffix:
                        lstContracts = lstContracts.OrderByDescending(item => item.contractSymbol).ToList();
                        break;
                }
                DataRow _row = null;

                // Initiliasation de la liste des contrats recyclés + Sous-jacents
                adjustment.LstUnderlyerIsinCode = new List<string>();
                adjustment.InitAIUnderlyer();
                adjustment.InitAIContract(lstContracts);

                string exContractSymbol = adjustment.GetLstContractSymbol(CATools.CAElementTypeEnum.Ex);
                string exAdjContractSymbol = adjustment.GetLstContractSymbol(CATools.CAElementTypeEnum.ExAdj);

                

                
                Logger.Write();

                // EG 20140518 [19913] New Execution Script PRECEDING

                
                // EG 20211109 [XXXXX] Changement de signature (usage de ProcessBase)
                CATools.ExecCorporateActionSQLScript(ProcessBase, ProcessBase.ProcessType.ToString(), m_CorporateAction, CATools.SQLRunTimeEnum.PRECEDING, m_CATemplatePath_AI);

                foreach (CorporateEventContract contract in lstContracts)
                {
                    // Le contrat n'est pas ajusté (Pas d'open interest)
                    if (adjustment.IsContractAdjustable(contract))
                    {
                        // PM 20170911 [23408] Remplacement de la liste de Pair par une liste de CATradeEx
                        //List<Pair<int, string>> lstTradeEx = new List<Pair<int, string>>();
                        List<CATradeEx> lstTradeEx = new List<CATradeEx>();

                        // EG 20140317 Add ReadUncommitted
                        // PL 20180312 WARNING: Use Read Commited !
                        //using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted))
                        using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                ds = new DataSet();
                                DataContractHelper.InitCA(CS);

                                #region DataAdapters
                                m_DicDataAdapter = new Dictionary<int, Pair<DataAdapter, DataAdapter>>();
                                Pair<DataAdapter, DataAdapter> _da = new Pair<DataAdapter, DataAdapter>
                                {

                                    // 1. Chargement des DCs Candidats
                                    First = LoadCATable(dbTransaction, DataContractResultSets.CA_DERIVATIVECONTRACT, Cst.OTCml_TBL.DERIVATIVECONTRACT,
                                    _corporateEvent.IdCE, m_EntityMarketInfo.IdA_Entity, contract.idDC, ref ds),
                                    Second = LoadCATable(dbTransaction, DataContractResultSets.CA_CORPOEVENTCONTRACT, Cst.OTCml_TBL.CORPOEVENTCONTRACT,
                                    _corporateEvent.IdCE, m_EntityMarketInfo.IdA_Entity, contract.idDC, ref ds)
                                };
                                m_DicDataAdapter.Add(0, _da);

                                // 2. Chargement des DAs Candidats
                                _da = new Pair<DataAdapter, DataAdapter>
                                {
                                    First = LoadCATable(dbTransaction, DataContractResultSets.CA_DERIVATIVEATTRIB, Cst.OTCml_TBL.DERIVATIVEATTRIB,
                                    _corporateEvent.IdCE, m_EntityMarketInfo.IdA_Entity, contract.idDC, ref ds),
                                    Second = LoadCATable(dbTransaction, DataContractResultSets.CA_CORPOEVENTDATTRIB, Cst.OTCml_TBL.CORPOEVENTDATTRIB,
                                    _corporateEvent.IdCE, m_EntityMarketInfo.IdA_Entity, contract.idDC, ref ds)
                                };
                                m_DicDataAdapter.Add(1, _da);
                                // 3. Chargement des ASSETs Candidats
                                _da = new Pair<DataAdapter, DataAdapter>
                                {
                                    First = LoadCATable(dbTransaction, DataContractResultSets.CA_ASSET, Cst.OTCml_TBL.ASSET_ETD,
                                    _corporateEvent.IdCE, m_EntityMarketInfo.IdA_Entity, contract.idDC, ref ds),
                                    Second = LoadCATable(dbTransaction, DataContractResultSets.CA_CORPOEVENTASSET, Cst.OTCml_TBL.CORPOEVENTASSET,
                                    _corporateEvent.IdCE, m_EntityMarketInfo.IdA_Entity, contract.idDC, ref ds)
                                };
                                m_DicDataAdapter.Add(2, _da);

                                #endregion DataAdapters
                                #region Initialisation ADO
                                // DERIVATIVECONTRACT
                                m_DtContract.First = ds.Tables[Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()];
                                DataColumn pkDCColumn = m_DtContract.First.Columns["IDDC"];
                                m_DtContract.First.PrimaryKey = new DataColumn[] { pkDCColumn };
                                m_DtContract.Second = ds.Tables[Cst.OTCml_TBL.CORPOEVENTCONTRACT.ToString()];
                                m_DtContract.Second.PrimaryKey = new DataColumn[] { m_DtContract.Second.Columns["IDCEC"] };

                                // DERIVATIVEATTRIB
                                m_DtDAttrib.First = ds.Tables[Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString()];
                                DataColumn pkDAColumn = m_DtDAttrib.First.Columns["IDDERIVATIVEATTRIB"];
                                m_DtDAttrib.First.PrimaryKey = new DataColumn[] { pkDAColumn };
                                m_DtDAttrib.Second = ds.Tables[Cst.OTCml_TBL.CORPOEVENTDATTRIB.ToString()];
                                m_DtDAttrib.Second.PrimaryKey = new DataColumn[] { m_DtDAttrib.Second.Columns["IDCEDA"] };
                                ds.Relations.Add(new DataRelation("FK_DA_DC", pkDCColumn, m_DtDAttrib.First.Columns["IDDC"], false));

                                // ASSET_ETD
                                m_DtAsset.First = ds.Tables[Cst.OTCml_TBL.ASSET_ETD.ToString()];
                                DataColumn pkASSETColumn = m_DtAsset.First.Columns["IDASSET"];
                                m_DtAsset.First.PrimaryKey = new DataColumn[] { pkASSETColumn };
                                m_DtAsset.Second = ds.Tables[Cst.OTCml_TBL.CORPOEVENTASSET.ToString()];
                                m_DtAsset.Second.PrimaryKey = new DataColumn[] { m_DtAsset.Second.Columns["IDCEA"] };
                                ds.Relations.Add(new DataRelation("FK_ASSET_DA", pkDAColumn, m_DtAsset.First.Columns["IDDERIVATIVEATTRIB"], false));
                                #endregion Initialisation ADO

                                // EG [33415/33420] ASSET_EQUITY
                                #region Sous-jacents à créer
                                if ((null != underlyer) || ((null != adjustment.LstUnderlyerIsinCode) && (0 < adjustment.LstUnderlyerIsinCode.Count)))
                                {
                                    if (StrFunc.IsFilled(underlyer.isinCode) && false == adjustment.LstUnderlyerIsinCode.Contains(underlyer.isinCode))
                                        adjustment.LstUnderlyerIsinCode.Add(underlyer.isinCode);

                                    _da = new Pair<DataAdapter, DataAdapter>
                                    {
                                        First = LoadCAUNLTables(dbTransaction, DataContractResultSets.CA_ASSET_EQUITY, Cst.OTCml_TBL.ASSET_EQUITY,
                                        underlyer.market.IdM, underlyer.SpheresId, adjustment.LstUnderlyerIsinCode, ref ds),
                                        Second = LoadCAUNLTables(dbTransaction, DataContractResultSets.CA_ASSET_EQUITY_RDCMK, Cst.OTCml_TBL.ASSET_EQUITY_RDCMK,
                                        underlyer.market.IdM, underlyer.SpheresId, adjustment.LstUnderlyerIsinCode, ref ds)
                                    };
                                    m_DicDataAdapter.Add(3, _da);

                                    m_DtAsset_Equity.First = ds.Tables[Cst.OTCml_TBL.ASSET_EQUITY.ToString()];
                                    pkASSETColumn = m_DtAsset_Equity.First.Columns["IDASSET"];
                                    m_DtAsset_Equity.First.PrimaryKey = new DataColumn[] { pkASSETColumn };
                                    m_DtAsset_Equity.Second = ds.Tables[Cst.OTCml_TBL.ASSET_EQUITY_RDCMK.ToString()];
                                    ds.Relations.Add(new DataRelation("FK_RDCMK_EQUITY", pkASSETColumn, m_DtAsset_Equity.Second.Columns["IDASSET"], false));
                                }
                                #endregion Sous-jacents à créer

                                #region On traite un DC
                                // FI 20200820 [25468] date systemes en UTC
                                m_DtSys = OTCmlHelper.GetDateSysUTC(CS);

                                Pair<int, int> _idDCIdentity = new Pair<int, int>(-1, -1);

                                DataRow rowCumDC = m_DtContract.First.Select().First();
                                // EG [33415/33420]
                                _row = m_DtContract.Second.Select(String.Format("IDCEC={0}", rowCumDC["CEC_IDCE"])).First();
                                contract.IdCEC = Convert.ToInt32(_row["IDCEC"]);
                                #region Gestion des contrats (Mise à jour et Création)

                                // EG 20220621 [34623] 
                                SetContractWithAlreadyCAExecuted(_row);

                                if (Convert.IsDBNull(_row["IDDCEX"]))
                                {
                                    Nullable<int> newExIdAsset_UNL = null;
                                    string exUnderlyerIsinCode = adjustment.GetStringAIContractValue(CATools.CAElementTypeEnum.Ex, CATools.CAElementEnum.unlisin, contract);

                                    if (StrFunc.IsFilled(exUnderlyerIsinCode))
                                        newExIdAsset_UNL = GetUnderlyerAsset(dbTransaction, underlyer.SpheresID, exUnderlyerIsinCode, CATools.CAElementTypeEnum.Ex);


                                    // Mise à jour du DC CUM
                                    UpdateDataRowCum(rowCumDC);
                                    // Insertion du DC EX
                                    if (1 == CATools.RenamingNextVersion(m_CorporateEventMktRules, contract))
                                    {
                                        if (InsertDataRowEx(contract, rowCumDC, _idDCIdentity.First, newExIdAsset_UNL))
                                            _row["IDDCEX"] = _idDCIdentity.First;
                                    }

                                    string exRecycledContract = adjustment.GetStringAIContractValue(CATools.CAElementTypeEnum.ExRecycled, CATools.CAElementEnum.sym, contract);
                                    if (StrFunc.IsFilled(exRecycledContract) && Convert.IsDBNull(_row["IDDCRECYCLED"]))
                                    {
                                        Nullable<int> newRecycledIdAsset_UNL = null;
                                        string exRecUnderlyerIsinCode = adjustment.GetStringAIContractValue(CATools.CAElementTypeEnum.ExRecycled, CATools.CAElementEnum.unlisin, contract);
                                        if (StrFunc.IsFilled(exRecUnderlyerIsinCode))
                                            newRecycledIdAsset_UNL = GetUnderlyerAsset(dbTransaction, underlyer.SpheresID, exRecUnderlyerIsinCode, CATools.CAElementTypeEnum.ExRecycled);
                                        _idDCIdentity.Second--;
                                        InsertDataRowExRecycled(contract, rowCumDC, _idDCIdentity.Second, newRecycledIdAsset_UNL);
                                        _row["IDDCRECYCLED"] = _idDCIdentity.Second;
                                    }

                                }

                                bool isNewRowAdj = CATools.IsNewDCForExAdj(contract, adjustment, m_CorporateEventMktRules.renamingContractMethod);


                                if (isNewRowAdj && Convert.IsDBNull(_row["IDDCEXADJ"]))
                                {
                                    Nullable<int> newExAdjIdAsset_UNL = null;
                                    string exAdjUnderlyerIsinCode = adjustment.GetStringAIContractValue(CATools.CAElementTypeEnum.ExAdj, CATools.CAElementEnum.unlisin, contract);
                                    if (StrFunc.IsFilled(exAdjUnderlyerIsinCode))
                                        newExAdjIdAsset_UNL = GetUnderlyerAsset(dbTransaction, underlyer.SpheresID, exAdjUnderlyerIsinCode, CATools.CAElementTypeEnum.ExAdj);

                                    _idDCIdentity.Second--;
                                    /// EG 20141014 Suite CA Merger FIAT/CHRYSLER
                                    if (InsertDataRowExAdj(contract, rowCumDC, _idDCIdentity.Second, null, null, newExAdjIdAsset_UNL))
                                    {
                                        _row["IDDCEXADJ"] = _idDCIdentity.Second;
                                    }
                                }


                                #endregion Gestion des contrats (Mise à jour et Création)

                                if (contract.dAttribsSpecified)
                                {
                                    #region Gestion des échéances ouvertes (Mise à jour et Création)
                                    foreach (CorporateEventDAttrib dAttrib in contract.dAttribs)
                                    {
                                        Pair<int, int> _idDAIdentity = new Pair<int, int>(-1, -2);

                                        DataRow rowCumDA = m_DtDAttrib.First.Select(String.Format("IDDERIVATIVEATTRIB={0}", dAttrib.idDA)).First();
                                        _row = m_DtDAttrib.Second.Select(String.Format("IDCEDA={0}", rowCumDA["IDCEDA"])).First();
                                        dAttrib.IdCEDA = Convert.ToInt32(_row["IDCEDA"]);

                                        // EG 20220621 [34623] 
                                        SetDAttribWithAlreadyCAExecuted(_row);

                                        if (Convert.IsDBNull(_row["IDDAEX"]))
                                            UpdateDataRowCum(rowCumDA);

                                        if (Convert.IsDBNull(_row["IDDAEXADJ"]))
                                        {
                                            InsertDataRowExAdj(dAttrib, rowCumDA, _idDCIdentity.Second, _idDAIdentity.Second, null, null);
                                            _row["IDDAEXADJ"] = _idDAIdentity.Second;

                                        }

                                        if (dAttrib.assetsSpecified)
                                        {
                                            #region Gestion des ASSETs (Mise à jour et Création)
                                            foreach (CorporateEventAsset asset in dAttrib.assets)
                                            {
                                                Pair<int, int> _idASSETIdentity = new Pair<int, int>(-1, -2);

                                                DataRow rowCumASSET = m_DtAsset.First.Select(String.Format("IDASSET={0}", asset.idASSET)).First();
                                                _row = m_DtAsset.Second.Select(String.Format("IDCEA={0}", rowCumASSET["IDCEA"])).First();
                                                asset.IdCEA = Convert.ToInt32(_row["IDCEA"]);

                                                // EG 20220621 [34623] 
                                                SetAssetWithAlreadyCAExecuted(_row);

                                                // Mise à jour du DC CUM
                                                if (Convert.IsDBNull(_row["IDASSETEX"]))
                                                    UpdateDataRowCum(rowCumASSET);

                                                if (Convert.IsDBNull(_row["IDASSETEXADJ"]))
                                                {
                                                    InsertDataRowExAdj(asset, rowCumASSET, _idDCIdentity.Second, _idDAIdentity.Second, _idASSETIdentity.Second, null);
                                                    _row["IDASSETEXADJ"] = _idASSETIdentity.Second;
                                                }

                                                Update(dbTransaction, contract);
                                                _idDAIdentity = new Pair<int, int>(dAttrib.idDAEX, dAttrib.idDAEXADJ);
                                                _idDCIdentity = new Pair<int, int>(contract.idDCEX, contract.idDCEXADJ ?? contract.idDCEX);

                                                #region TRADES en position
                                                codeReturn = AdjustmentTradeByTrade(dbTransaction, asset, ref lstTradeEx);
                                                #endregion TRADES en position

                                                #region TRADES entrés en portefeuille POSTCA à réaffecter sur l'IDASSET Ex
                                                codeReturn = RecoveryTradePostCorporateActions(CS, dbTransaction, contract, dAttrib, asset, ref lstTradeEx);
                                                #endregion TRADES entrés en portefeuille POSTCA à réaffecter sur l'IDASSET Ex

                                            }
                                            #endregion Gestion des ASSETs (Mise à jour et Création)
                                        }
                                        else
                                        {
                                            // EG 20130716 Pas d'ASSET on gère tout de même les DC/DA
                                            Update(dbTransaction, contract);
                                            _idDAIdentity = new Pair<int, int>(dAttrib.idDAEX, dAttrib.idDAEXADJ);
                                            _idDCIdentity = new Pair<int, int>(contract.idDCEX, contract.idDCEXADJ ?? contract.idDCEX);
                                            codeReturn = Cst.ErrLevel.SUCCESS;
                                        }
                                    }
                                    #endregion Gestion des échéances ouvertes (Mise à jour et Création)
                                }
                                else
                                {
                                    // EG 20130716 Pas de DERIVATIVATTRIB on gère tout de même le DC
                                    Update(dbTransaction, contract);
                                    _idDCIdentity = new Pair<int, int>(contract.idDCEX, contract.idDCEXADJ ?? contract.idDCEX);
                                    codeReturn = Cst.ErrLevel.SUCCESS;
                                }
                                #endregion On traite un DC

                                UpdateCorpoEventResult(dbTransaction);
                                InsertLinkedTable(dbTransaction, contract);

                                DataHelper.CommitTran(dbTransaction);

                                if (0 < lstTradeEx.Count)
                                {
                                    lstTradeEx.ForEach(trade =>
                                    {
                                        // PM 20170911 [23408] Remplacement de la liste de Pair par une liste de CATradeEx et ajout insertion Equalisation Payment
                                        codeReturn = RecordTradeEvent(trade.IdT, trade.Identifier);
                                        if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                        {
                                            codeReturn = InsertEqualisationPaymentEvent(trade);
                                        }
                                        if (false == ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                                        {
                                            
                                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 5179), 0,
                                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                                new LogParam(LogTools.IdentifierAndId(trade.Identifier, trade.IdT))));
                                        }
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                DataHelper.RollbackTran(dbTransaction);
                                throw;
                            }
                        }
                    }
                }

                // EG 20140518 [19913] New Execution Script FOLLOWING
                
                // EG 20211109 [XXXXX] Changement de signature (usage de ProcessBase)
                CATools.ExecCorporateActionSQLScript(ProcessBase, ProcessBase.ProcessType.ToString(), m_CorporateAction, CATools.SQLRunTimeEnum.FOLLOWING, m_CATemplatePath_AI);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (null != m_DicDataAdapter)
                {
                    foreach (Pair<DataAdapter, DataAdapter> _da in m_DicDataAdapter.Values)
                    {
                        if (null != _da.First)
                            _da.First.Dispose();
                        if (null != _da.Second)
                            _da.Second.Dispose();
                    }

                }
                if (null != ds)
                    ds.Dispose();
            }
            return codeReturn;
        }
        #endregion AdjustmentTrades
        /// <summary>
        /// Affectation des IDs (EX et ADJ des DC ayant déjà été traité par la même CA
        /// mais sur une autre entité.
        /// </summary>
        /// <param name="pRow"></param>
        /// EG 20220621 [34623] New 
        /// EG 20230106 [34623][26212][WI523] Upd
        private void SetContractWithAlreadyCAExecuted(DataRow pRow)
        {
            pRow.BeginEdit();
            pRow["READYSTATE"] = CorporateEventReadyStateEnum.EXECUTED.ToString();

            if (Convert.IsDBNull(pRow["IDDCEX"]) && (false == Convert.IsDBNull(pRow["IDDCEX_EXECUTED"])))
                pRow["IDDCEX"] = pRow["IDDCEX_EXECUTED"];

            if (Convert.IsDBNull(pRow["IDDCEXADJ"]) && (false == Convert.IsDBNull(pRow["IDDCEXADJ_EXECUTED"])))
                pRow["IDDCEXADJ"] = pRow["IDDCEXADJ_EXECUTED"];

            if (Convert.IsDBNull(pRow["IDDCRECYCLED"]) && (false == Convert.IsDBNull(pRow["IDDCRECYCLED_EXECUTED"])))
                pRow["IDDCRECYCLED"] = pRow["IDDCRECYCLED_EXECUTED"];
            pRow.EndEdit();
        }
        /// <summary>
        /// Affectation des IDs (EX et ADJ) des DATTRIB ayant déjà été traité par la même CA
        /// mais sur une autre entité.
        /// </summary>
        /// <param name="pRow"></param>
        /// EG 20220621 [34623] New 
        /// EG 20230106 [34623][26212][WI523] Upd
        private void SetDAttribWithAlreadyCAExecuted(DataRow pRow)
        {
            pRow.BeginEdit();
            pRow["READYSTATE"] = CorporateEventReadyStateEnum.EXECUTED.ToString();

            if (Convert.IsDBNull(pRow["IDDAEX"]) && (false == Convert.IsDBNull(pRow["IDDAEX_EXECUTED"])))
                pRow["IDDAEX"] = pRow["IDDAEX_EXECUTED"];

            if (Convert.IsDBNull(pRow["IDDAEXADJ"]) && (false == Convert.IsDBNull(pRow["IDDAEXADJ_EXECUTED"])))
                pRow["IDDAEXADJ"] = pRow["IDDAEXADJ_EXECUTED"];
            pRow.EndEdit();
        }
        /// <summary>
        /// Affectation des IDs (EX et ADJ) des ASSET ayant déjà été traité par la même CA
        /// mais sur une autre entité.
        /// </summary>
        /// <param name="pRow"></param>
        /// EG 20220621 [34623] New 
        /// EG 20230106 [34623][26212][WI523] Upd
        private void SetAssetWithAlreadyCAExecuted(DataRow pRow)
        {
            pRow.BeginEdit();
            pRow["READYSTATE"] = CorporateEventReadyStateEnum.EXECUTED.ToString();

            if (Convert.IsDBNull(pRow["IDASSETEX"]) && (false == Convert.IsDBNull(pRow["IDASSETEX_EXECUTED"])))
                pRow["IDASSETEX"] = pRow["IDASSETEX_EXECUTED"];

            if (Convert.IsDBNull(pRow["IDASSETEXADJ"]) && (false == Convert.IsDBNull(pRow["IDASSETEXADJ_EXECUTED"])))
                pRow["IDASSETEXADJ"] = pRow["IDASSETEXADJ_EXECUTED"];
            pRow.EndEdit();
        }

        #region GetUnderlyerAsset
        /// <summary>
        /// Création des DataRow pour nouveau Sous-jacents de type ASSET_EQUITY 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pUnderlyer"></param>
        /// <returns></returns>
        // EG [33415/33420]
        private Nullable<int> GetUnderlyerAsset(IDbTransaction pDbTransaction, int pIdAsset_UNL, string pUnderlyerIsinCode, CATools.CAElementTypeEnum pEltType)
        {
            Nullable<int> idAsset_UNL = null;
            if (null != m_DtAsset_Equity)
            {
                DateTime dtSys = OTCmlHelper.GetDateSysUTC(CS);
                CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];

                // ASSET_EQUITY
                DataRow[] rowCum = m_DtAsset_Equity.First.Select(String.Format("IDASSET={0}", pIdAsset_UNL));
                // ASSET_EQUITY_RDCMK
                _ = m_DtAsset_Equity.Second.Select(String.Format("IDASSET={0} and IDM_RELATED={1}", pIdAsset_UNL, m_CorporateAction.market.IdM));

                DataRow[] rowEx = m_DtAsset_Equity.First.Select(String.Format("ISINCODE='{0}'", pUnderlyerIsinCode));

                string _relatedUnderlyerSymbol = string.Empty;
                string _underlyerDisplayName = string.Empty;
                string _underlyerSymbol = string.Empty;
                string _underlyerDescription = string.Empty;

                Adjustment adjustment = corporateEvent.procedure.adjustment;
                AIUnderlyer _ai = adjustment.GetAIUnderlyer(pEltType, pUnderlyerIsinCode);
                if (null != _ai)
                {
                    _relatedUnderlyerSymbol = _ai.RelatedSymbol;
                    _underlyerDisplayName = _ai.DisplayName;
                    _underlyerSymbol = _ai.Symbol;
                    _underlyerDescription = _ai.Description;
                }

                DataRow newRow;
                if (ArrFunc.IsFilled(rowEx))
                {
                    #region EQUITY (EX/EXADJ/RECYCLED) EXISTE DEJA
                    idAsset_UNL = Convert.ToInt32(rowEx[0]["IDASSET"]);

                    if (StrFunc.IsFilled(_underlyerDescription) || StrFunc.IsFilled(_underlyerDisplayName) || StrFunc.IsFilled(_underlyerSymbol))
                    {
                        rowEx[0].BeginEdit();
                        if (StrFunc.IsFilled(_underlyerDisplayName))
                        {
                            if (m_DtAsset_Equity.First.Columns.Contains("DISPLAYNAME"))
                                rowEx[0]["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE + _underlyerDisplayName;
                        }
                        if (StrFunc.IsFilled(_underlyerDescription))
                        {
                            if (m_DtAsset_Equity.First.Columns.Contains("DESCRIPTION"))
                                rowEx[0]["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE + _underlyerDescription;
                        }
                        if (StrFunc.IsFilled(_underlyerSymbol))
                        {
                            rowEx[0]["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
                            rowEx[0]["SYMBOL"] = _underlyerSymbol;
                        }
                        rowEx[0].EndEdit();
                        UpdateAssetEquity(pDbTransaction);
                    }
                    #endregion EQUITY (EX/EXADJ/RECYCLED) EXISTE DEJA

                    // Type EX : Recherche existence dans ASSET_EQUITY_RDCMK pour le marché spécifié
                    DataRow[] rowExRelated = m_DtAsset_Equity.Second.Select(String.Format("IDASSET={0} and IDM_RELATED={1}", idAsset_UNL, m_CorporateAction.market.IdM));
                    if (ArrFunc.IsFilled(rowExRelated))
                    {
                        #region EQUITY_RDCMK (EX/EXADJ/RECYCLED) EXISTE DEJA
                        if (StrFunc.IsFilled(_relatedUnderlyerSymbol))
                        {
                            rowExRelated[0].BeginEdit();
                            rowExRelated[0]["SYMBOL"] = _relatedUnderlyerSymbol;
                            rowExRelated[0].EndEdit();
                            UpdateAssetEquity(pDbTransaction);
                        }
                        #endregion EQUITY_RDCMK (EX/EXADJ/RECYCLED) EXISTE DEJA
                    }
                    else
                    {
                        #region EQUITY_RDCMK (EX/EXADJ/RECYCLED) N'EXISTE PAS
                        newRow = m_DtAsset_Equity.Second.NewRow();
                        newRow.BeginEdit();
                        newRow["IDASSET"] = idAsset_UNL;
                        newRow["IDM_RELATED"] = m_CorporateAction.market.IdM;
                        newRow["DTENABLED"] = corporateEvent.effectiveDate;
                        newRow["DTENABLED"] = Convert.DBNull;
                        newRow["IDAINS"] = ProcessBase.Session.IdA;
                        newRow["DTINS"] = dtSys;
                        if (StrFunc.IsFilled(_relatedUnderlyerSymbol))
                        {
                            newRow["SYMBOL"] = _relatedUnderlyerSymbol;
                        }
                        newRow.EndEdit();
                        m_DtAsset_Equity.Second.Rows.Add(newRow);
                        UpdateAssetEquity(pDbTransaction);
                        #endregion EQUITY_RDCMK (EX/EXADJ/RECYCLED) N'EXISTE PAS
                    }
                }
                else if (ArrFunc.IsFilled(rowCum))
                {
                    #region EQUITY (EX/EXADJ/RECYCLED) N'EXISTE PAS
                    newRow = m_DtAsset_Equity.First.NewRow();
                    // RD 20170518 [23163] Add Clone
                    newRow.ItemArray = (object[])rowCum[0].ItemArray.Clone();
                    newRow.BeginEdit();
                    newRow["IDASSET"] = -1;
                    //newRow["IDM"] = rowCum[0]["IDM"];
                    //newRow["IDC"] = rowCum[0]["IDC"];

                    if (m_DtAsset_Equity.First.Columns.Contains("IDENTIFIER"))
                        newRow["IDENTIFIER"] = Cst.AUTOMATIC_COMPUTE;
                    if (m_DtAsset_Equity.First.Columns.Contains("DISPLAYNAME"))
                        newRow["DISPLAYNAME"] = Cst.AUTOMATIC_COMPUTE;
                    if (m_DtAsset_Equity.First.Columns.Contains("DESCRIPTION"))
                        newRow["DESCRIPTION"] = Cst.AUTOMATIC_COMPUTE;


                    if (StrFunc.IsFilled(_underlyerDisplayName))
                    {
                        newRow["DISPLAYNAME"] += _underlyerDisplayName;
                    }
                    if (StrFunc.IsFilled(_underlyerDescription))
                    {
                        newRow["DESCRIPTION"] += _underlyerDescription;
                    }
                    if (StrFunc.IsFilled(_underlyerSymbol))
                    {
                        newRow["SYMBOL"] = _underlyerSymbol;
                    }

                    newRow["ISINCODE"] = pUnderlyerIsinCode;
                    newRow["DTENABLED"] = corporateEvent.effectiveDate;
                    newRow["IDAINS"] = ProcessBase.Session.IdA;
                    newRow["DTINS"] = dtSys;
                    newRow.EndEdit();
                    m_DtAsset_Equity.First.Rows.Add(newRow);
                    #endregion EQUITY (EX/EXADJ/RECYCLED) N'EXISTE PAS

                    #region EQUITY_RDCMK (EX/EXADJ/RECYCLED) N'EXISTE PAS
                    newRow = m_DtAsset_Equity.Second.NewRow();
                    newRow.BeginEdit();
                    newRow["IDASSET"] = -1;
                    newRow["IDM_RELATED"] = m_CorporateAction.market.IdM;
                    newRow["DTENABLED"] = corporateEvent.effectiveDate;
                    newRow["IDAINS"] = ProcessBase.Session.IdA;
                    newRow["DTINS"] = dtSys;

                    if (StrFunc.IsFilled(_relatedUnderlyerSymbol))
                    {
                        newRow["SYMBOL"] = _relatedUnderlyerSymbol;
                    }
                    newRow.EndEdit();
                    m_DtAsset_Equity.Second.Rows.Add(newRow);
                    UpdateAssetEquity(pDbTransaction);

                    idAsset_UNL = Convert.ToInt32(newRow["IDASSET"]);

                    #endregion EQUITY_RDCMK (EX/EXADJ/RECYCLED) N'EXISTE PAS
                }
            }
            return idAsset_UNL;
        }
        #endregion GetUnderlyerAsset

        #region CreateDailySettlementPriceAdjusted
        // EG 20220206 [26212][WI523] Contrôle Duplicate Key si on relance la clôture de journée après un plantage au changement de date (la CA a été exécutée)
        private Cst.ErrLevel CreateDailySettlementPriceAdjusted(IDbTransaction pDbTransaction, CorporateEventAsset pCorporateEventAsset)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pCorporateEventAsset.idASSET);
            parameters.Add(new DataParameter(CS, "IDASSETEXADJ", DbType.Int32), pCorporateEventAsset.idASSETEXADJ);
            parameters.Add(new DataParameter(CS, "TIME", DbType.DateTime), m_PKGenProcess.ProcessCacheContainer.CACumDate);
            parameters.Add(new DataParameter(CS, "VALUE", DbType.Decimal), pCorporateEventAsset.exData.dailyClosingPrice.valueRounded.DecValue);
            parameters.Add(new DataParameter(CS, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), QuotationSideEnum.OfficialClose.ToString());
            parameters.Add(new DataParameter(CS, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), QuoteTimingEnum.Close.ToString());
            parameters.Add(new DataParameter(CS, "QUOTEUNIT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Cst.PriceQuoteUnits.Price.ToString());
            parameters.Add(new DataParameter(CS, "IDMARKETENV", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN),"DEFAULT_MARKET_ENV");
            parameters.Add(new DataParameter(CS, "IDVALSCENARIO", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN),"EOD_VALUATION");
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(CS));
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDAINS), m_PKGenProcess.Session.IdA);
            parameters.Add(new DataParameter(CS, "SPREADVALUE", DbType.Int32), 0);
            parameters.Add(new DataParameter(CS, "ISENABLED", DbType.Boolean), true);
            parameters.Add(new DataParameter(CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), 
                String.Format("Adjusted Daily Settlement price ({0}) after  {1} :", pCorporateEventAsset.cumData.dailyClosingPrice.CultureValue, m_CorporateAction.identifier));

            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.QUOTE_ETD_H + Cst.CrLf;
            sqlQuery += "(IDMARKETENV, IDVALSCENARIO, IDASSET, IDC, IDBC, IDM, TIME, VALUE, SPREADVALUE, QUOTEUNIT, QUOTESIDE, QUOTETIMING, " + Cst.CrLf;
            sqlQuery += "ISENABLED, SOURCE, DTINS, IDAINS, EXTLLINK)" + Cst.CrLf;
            sqlQuery += SQLCst.SELECT + "IDMARKETENV, IDVALSCENARIO, @IDASSETEXADJ, IDC, IDBC, IDM, TIME, @VALUE, @SPREADVALUE, @QUOTEUNIT, @QUOTESIDE, @QUOTETIMING, " + Cst.CrLf;
            sqlQuery += "@ISENABLED, SOURCE, @DTINS, @IDAINS, @EXTLLINK" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.QUOTE_ETD_H + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDASSET = @IDASSET) and (TIME = @TIME) and (QUOTESIDE = @QUOTESIDE)  and (QUOTETIMING = @QUOTETIMING) and";
            sqlQuery += "(IDMARKETENV = @IDMARKETENV) and (IDVALSCENARIO = @IDVALSCENARIO)";
            try
            {
                DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
            }
            catch (Exception ex)
            {
                if (!DataHelper.IsDuplicateKeyError(CS, ex))
                    throw;
            }
            return codeReturn;
        }
        #endregion CreateDailySettlementPriceAdjusted
        #region CreateExTrade
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pDataDocumentContainer"></param>
        /// <param name="pCorporateEventAsset"></param>
        /// <param name="pLstTradeEx"></param>
        /// <param name="pEqualisationPayment"></param>
        /// <returns></returns>
        // EG [33415/33420]
        // PM 20170911 [23408] Modification du type de pLstTradeEx la liste : List<Pair<int, string>> vers List<CATradeEx>
        // PM 20170911 [23408] Ajout paramètre CAEqualisationPaymentEvent pEqualisationPayment
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CreateExTrade(IDbTransaction pDbTransaction, IPosRequestCorporateAction pPosRequest, DataDocumentContainer pDataDocumentContainer,
            CorporateEventAsset pCorporateEventAsset, ref List<CATradeEx> pLstTradeEx, CAEqualisationPaymentEvent pEqualisationPayment)
        {
            CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
            // EG [33415/33420]
            int idAsset = (pCorporateEventAsset.idASSETEXADJ == 0 ? pCorporateEventAsset.idASSETEX : pCorporateEventAsset.idASSETEXADJ);
            SQL_AssetETD sql_AssetETD = new SQL_AssetETD(CS, idAsset)
            {
                DbTransaction = pDbTransaction
            };
            Cst.ErrLevel codeReturn;
            if (sql_AssetETD.LoadTable(new string[] { "IDASSET", "IDDC", "CONTRACTIDENTIFIER", "CONTRACTSYMBOL" }))
            {
                // Création d'un nouveau PosRequest fictif avec les informations détaillées
                IPosRequestCorporateAction _posRequestTrade =
                    (IPosRequestCorporateAction)PosKeepingTools.SetPosRequestCorporateAction(pPosRequest,
                    sql_AssetETD.IdDerivativeContract, sql_AssetETD.DrvContract_Identifier, sql_AssetETD.DrvContract_Symbol, _corporateEvent.effectiveDate);

                // Lire le template du trade à créer
                codeReturn = SetAdditionalInfoFromTrade(pDbTransaction, _posRequestTrade);


                // Création du DataDocument du nouveau trade
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    // Copie du datadocument du trade initiale comme base pour du trade généré
                    _posRequestTrade.Detail.DataDocument = (DataDocumentContainer)pDataDocumentContainer.Clone();
                    codeReturn = SetDataDocumentCorporateAction(_posRequestTrade, pCorporateEventAsset);
                }
                int idTEx = 0;
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    codeReturn = RecordExTrade(pDbTransaction, _posRequestTrade, out idTEx);
                }
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    // Enregistrement des trades pour génération des événements post Commit
                    string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(CS, pDbTransaction, idTEx);

                    // PM 20170911 [23408] Modification du type de pLstTradeEx la liste : List<Pair<int, string>> vers List<CATradeEx>
                    //pLstTradeEx.Add(new Pair<int, string>(idTEx, tradeIdentifier));
                    pLstTradeEx.Add(new CATradeEx(idTEx, tradeIdentifier, pEqualisationPayment));
                }
            }
            else
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 9007), 2,
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness)),
                    new LogParam(LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA)),
                    new LogParam(m_CorporateAction.refNotice.value),
                    new LogParam(_corporateEvent.adjMethod),
                    new LogParam(idAsset)));

                codeReturn = Cst.ErrLevel.FAILURE;
            }
            return codeReturn;
        }
        #endregion CreateExTrade

        #region OffSettingTradeCum
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pCorporateEventAsset"></param>
        /// <param name="pIsDealerBuyer"></param>
        /// <param name="pIdE_Event"></param>
        /// <param name="pIdC"></param>
        /// <param name="opEqualisationPayment"></param>
        /// <returns></returns>
        /// EG 20141106 [20253] Equalisation payment
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150706 [21021] Nullable<int> for idB_Buyer|idB_Seller
        // PM 20170911 [23408] Ajout paramètre out opEqualisationPayment
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // PL 20180309 UP_GETID use Shared Sequence on POSACTION/POSACTIONDET
        private Cst.ErrLevel OffSettingTradeCum(IDbTransaction pDbTransaction, IPosRequestCorporateAction pPosRequest, DataDocumentContainer pDataDocument,
            CorporateEventAsset pCorporateEventAsset, bool pIsDealerBuyer, int pIdE_Event, string pIdC, out CAEqualisationPaymentEvent opEqualisationPayment)
        {
            opEqualisationPayment = default;

            // 1. Clôture du trade sur l'asset en cours (pas de dégagement de résult mais soulte possible)
            #region GetId of POSACTION/POSACTIONDET
            SQLUP.GetId(out int newIdPA, pDbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
            //int newIdPADET = 0;
            //SQLUP.GetId(out newIdPADET, pDbTransaction, SQLUP.IdGetId.POSACTIONDET, SQLUP.PosRetGetId.First, 1);
            int newIdPADET = newIdPA;
            #endregion GetId of POSACTION/POSACTIONDET

            DateTime dtEffectiveDate = m_CorporateAction.corporateEvent[0].effectiveDate;

            #region Insertion dans POSACTION
            InsertPosAction(pDbTransaction, newIdPA, dtEffectiveDate, pPosRequest.IdPR);
            #endregion Insertion dans POSACTION

            #region Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS
            int idT_Closing = pPosRequest.IdT;
            // EG 20150920 [21374] Int (int32) to Long (Int64)
            // EG 20170127 Qty Long To Decimal
            decimal qty = pPosRequest.Qty;
            int idE_Event = pIdE_Event; // Id EVT Group (FUT/CALL/PUT) qui sera parent de la clôture

            Nullable<int> idT_Buy = null;
            Nullable<int> idT_Sell = null;
            if (pIsDealerBuyer)
                idT_Buy = idT_Closing;
            else
                idT_Sell = idT_Closing;

            #region EVENT et POSACTIONDET
            int nbEvent = pPosRequest.NbTokenIdE;
            SQLUP.GetId(out int newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);

            #region POSACTIONDET
            // EG 20141205 null for PositionEffect
            InsertPosActionDet(pDbTransaction, newIdPA, newIdPADET, idT_Buy, idT_Sell, idT_Closing, qty, null);
            #endregion POSACTIONDET


            #region EVENT OCA (OffSetting Corporate Action )
            m_EventQuery.InsertEvent(pDbTransaction, idT_Closing, newIdE, idE_Event, null, 1 , 1, null, null, null, null,
                    EventCodeFunc.OffSettingCorporateAction, EventTypeFunc.Total,
                    dtEffectiveDate, dtEffectiveDate, dtEffectiveDate, dtEffectiveDate, qty, null, UnitTypeEnum.Qty.ToString(), null, null);
            m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.GroupLevel, dtEffectiveDate, false);

            //PosActionDet
            EventQuery.InsertEventPosActionDet(pDbTransaction, newIdPADET, newIdE);
            #endregion EVENT OCA (OffSetting Corporate Action)

            //idE_Event Contient L'evènement OCA
            idE_Event = newIdE;

            #region EVENT NOM/QTY (Nominal/Quantité)
            newIdE++;
            Cst.ErrLevel codeReturn = InsertNominalQuantityEvent(pDbTransaction, idT_Closing, ref newIdE, idE_Event, dtEffectiveDate, pIsDealerBuyer, qty, 0, newIdPADET);
            #endregion EVENT NOM/QTY (Nominal/Quantité)
            #endregion EVENT et POSACTIONDET
            #endregion Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS

            // 2. Equalisation payment éventuel
            if ((null != pCorporateEventAsset.exData) && pCorporateEventAsset.exData.equalizationPaymentSpecified)
            {
                if (0 != pCorporateEventAsset.exData.equalizationPayment.value.DecValue)
                {
                    newIdE++;

                    #region EqualisationPayment
                    // Si EP < 0 le détenteur de la position acheteuse perçoit la soulte
                    // Si EP > 0 le détenteur de la position vendeuse perçoit la soulte
                    decimal amount = pCorporateEventAsset.exData.equalizationPayment.value.DecValue;

                    // EG 20190121 [23249] Upd pour Application arrondi sur EQP avant|après application de la quantité en position sur le trade
                    if (pCorporateEventAsset.cashFlowCalcMethod == CashFlowCalculationMethodEnum.OVERALL)
                    {
                        // Equalisation payment multiplié par le nombre de lot en position + Application de la règle d'arrondi
                        amount *= qty;
                        if (pCorporateEventAsset.exData.equalizationPayment.roundingSpecified)
                        {
                            Rounding _rounding = pCorporateEventAsset.exData.equalizationPayment.rounding;
                            EFS_Round _round = new EFS_Round(_rounding.direction, _rounding.precision, amount);
                            amount = _round.AmountRounded;
                        }
                    }
                    else
                    {
                        // La règle d'arrondi existe et a été appliquée unitairement
                    if (pCorporateEventAsset.exData.equalizationPayment.valueRoundedSpecified)
                    {
                        amount = pCorporateEventAsset.exData.equalizationPayment.valueRounded.DecValue;
                    }
                    // Equalisation payment multiplié par le nombre de lot en position 
                    amount *= qty;
                    }

                    IParty buyer = pDataDocument.GetPartyBuyer();
                    IParty seller = pDataDocument.GetPartySeller();

                    Nullable<int> idA_Buyer = pDataDocument.GetOTCmlId_Party(buyer.Id);
                    Nullable<int> idB_Buyer = pDataDocument.GetOTCmlId_Book(buyer.Id);
                    Nullable<int> idA_Seller = pDataDocument.GetOTCmlId_Party(seller.Id);
                    Nullable<int> idB_Seller = pDataDocument.GetOTCmlId_Book(seller.Id);

                    // EG 20190121 [23249] Correction calcul Payer|Receiver de l'EQP
                    // Si EP > 0 le détenteur de la position vendeuse perçoit la soulte
                    // Si EP < 0 le détenteur de la position acheteuse perçoit la soulte
                    Nullable<int> idA_Payer = (0 < amount)?idA_Buyer:idA_Seller;
                    Nullable<int> idB_Payer = (0 < amount)?idB_Buyer:idB_Seller;
                    Nullable<int> idA_Receiver = (0 < amount)?idA_Seller:idA_Buyer;
                    Nullable<int> idB_Receiver = (0 < amount) ? idB_Seller : idB_Buyer;

                    // PM 20170911 [23408] Changement de code/type pour les Equalisation Payment
                    //string eventCode = EventCodeFunc.Termination;
                    //string eventType = EventTypeFunc.SettlementCurrency;
                    string eventCode = EventCodeFunc.LinkedProductClosing;
                    string eventType = EventTypeFunc.EqualisationPayment;

                    // PM 20170911 [23408] Le paramétrage des codes événements n'est plus possible
                    //if (pCorporateEventAsset.category == CfiCodeCategoryEnum.Future)
                    //{
                    //    if (m_CorporateEventMktRules.equalPaymentFutureEventCodeSpecified)
                    //        eventCode = m_CorporateEventMktRules.equalPaymentFutureEventCode;
                    //    if (m_CorporateEventMktRules.equalPaymentFutureEventTypeSpecified)
                    //        eventType = m_CorporateEventMktRules.equalPaymentFutureEventType;

                    //}
                    //else if (pCorporateEventAsset.category == CfiCodeCategoryEnum.Option)
                    //{
                    //    if (m_CorporateEventMktRules.equalPaymentOptionEventCodeSpecified)
                    //        eventCode = m_CorporateEventMktRules.equalPaymentOptionEventCode;
                    //    if (m_CorporateEventMktRules.equalPaymentOptionEventTypeSpecified)
                    //        eventType = m_CorporateEventMktRules.equalPaymentOptionEventType;
                    //}

                    codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pPosRequest.IdT, newIdE, idE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                    eventCode, eventType, dtEffectiveDate, dtEffectiveDate, dtEffectiveDate, dtEffectiveDate,
                    Math.Abs(amount), pIdC, UnitTypeEnum.Currency.ToString(), null, null);
                    if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtEffectiveDate))
                    {
                        DateTime dtSettlement = dtEffectiveDate;
                        if (m_CorporateAction.market.idBCSpecified)
                        {
                            dtSettlement = Tools.ApplyOffset(CS, m_Product, dtEffectiveDate, 1, DayTypeEnum.ExchangeBusiness, m_CorporateAction.market.idBC, pDataDocument);
                        }
                        _ = m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.Recognition, dtEffectiveDate, false);

                        // EG 20150616 [21124]
                        // PM 20170911 [23408] L'Equalisation Payment n'est plus IsPayment sur le trade Cum
                        _ = m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.ValueDate, dtEffectiveDate, false);

                        codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.Settlement, dtSettlement, false);

                        // PM 20170911 [23408] Sauvegarde des éléments de l'événement Equalisation Payment
                        opEqualisationPayment = new CAEqualisationPaymentEvent(idA_Payer, idB_Payer, idA_Receiver, idB_Receiver, dtEffectiveDate, dtSettlement, Math.Abs(amount), pIdC);
                    }
                    #endregion EqualisationPayment
                }
            }
            else if (m_CorporateAction.IsEqualisationPaymentSpecified(pCorporateEventAsset.category) && pCorporateEventAsset.IsEqualPaymentMustBeCalculated)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-09053",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                    LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_PosRequest.IdA_Entity),
                    LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_PosRequest.IdA_CssCustodian),
                    LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_PosRequest.IdM),
                    m_MarketPosRequest.GroupProductValue,
                    DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness),
                    LogTools.IdentifierAndId(m_CorporateAction.identifier, m_CorporateAction.IdCA),
                    m_CorporateAction.refNotice.value,
                    m_CorporateAction.corporateEvent[0].adjMethod.ToString(),
                    LogTools.IdentifierAndId(pPosRequest.Identifiers.Trade, pPosRequest.IdT),
                    pCorporateEventAsset.adjStatus.ToString());
            }
            return codeReturn;
        }
        #endregion OffSettingTradeCum

        #region RecordExTrade
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pIdTEx"></param>
        /// <returns></returns>
        /// FI 20161206 [22092] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Add identifierEx
        private Cst.ErrLevel RecordExTrade(IDbTransaction pDbTransaction, IPosRequestCorporateAction pPosRequest, out int pIdTEx)
        {
            try
            {
                IPosRequestTradeAdditionalInfo additionalInfo = (IPosRequestTradeAdditionalInfo)m_TemplateDataDocumentTrade[pPosRequest.PosKeepingKey.IdI];

                // FI 20161206 [22092] add tradeLinkInfo 
                List<Pair<int, string>> tradeLinkInfo = new List<Pair<int, string>>
                {
                    new Pair<int, string>(pPosRequest.IdT, pPosRequest.Identifiers.Trade)
                };

                Cst.ErrLevel codeReturn = RecordTrade(pDbTransaction, pPosRequest.Detail.DataDocument, additionalInfo, new int[] { pPosRequest.IdPR },
                pPosRequest.RequestType, tradeLinkInfo, out int idTEx, out string identifierEx);

                pIdTEx = idTEx;

                return codeReturn;
            }
            catch (Exception)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5180), 0,
                    new LogParam(GetPosRequestLogValue(pPosRequest.RequestType)),
                    new LogParam(LogTools.IdentifierAndId(pPosRequest.Identifiers.Trade, pPosRequest.IdT)),
                    new LogParam(LogTools.IdentifierAndId(pPosRequest.Detail.IdentifierDCEx, pPosRequest.Detail.IdDCEx))));

                throw;
            }
        }
        #endregion RecordExTrade


        #region RowUpdating
        /// <summary>
        /// RowUpdating
        /// </summary>
        /// EG [33415/33420] Gestion SCOPE_IDENTITY ou NON en fonction présence TRIGGER InsteadOF
        /// EG 20160105 [34091]
        private void RowUpdating(object pSender, RowUpdatingEventArgs pEventArgs)
        {
            if (pEventArgs.StatementType == StatementType.Insert)
            {
                Cst.OTCml_TBL _parentTable = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pEventArgs.Row.Table.TableName, true);

                DbSvrType svrType = DataHelper.GetDbSvrType(pEventArgs.Command.Connection.ConnectionString);
                // EG 20160404 Migration vs2013
                //Nullable<int> _valIdentity = null;
                switch (svrType)
                {
                    case DbSvrType.dbSQL:
                        //Pas de lecture de l'ID généré sur les tables DERIVATIVECONTRACT et ASSET_ETD via l'instruction SCOPE_IDENTITY()
                        //en raison d'existence de trigger InsteadOF sur chaque table où il existe l'instruction OUTPUT inserted.IDXXXX as COLIDENTITY
                        if (_parentTable == Cst.OTCml_TBL.DERIVATIVEATTRIB)
                            pEventArgs.Command.CommandText += ";" + Cst.CrLf + "select SCOPE_IDENTITY() as COLIDENTITY";
                        break;
                    case DbSvrType.dbORA:
                        if (false == pEventArgs.Command.Parameters.Contains(":COLIDENTITY"))
                        {
                            if (_parentTable == Cst.OTCml_TBL.DERIVATIVECONTRACT)
                            {
                                pEventArgs.Command.CommandText += " returning IDDC into :COLIDENTITY";
                                DataParameter dp = new DataParameter(pEventArgs.Command.Connection.ConnectionString, ":COLIDENTITY", DbType.Int32)
                                {
                                    Direction = ParameterDirection.Output
                                };
                                pEventArgs.Command.Parameters.Add(dp.DbDataParameter);
                            }
                            else if (_parentTable == Cst.OTCml_TBL.DERIVATIVEATTRIB)
                            {
                                pEventArgs.Command.CommandText += " returning IDDERIVATIVEATTRIB into :COLIDENTITY";
                                DataParameter dp = new DataParameter(pEventArgs.Command.Connection.ConnectionString, ":COLIDENTITY", DbType.Int32)
                                {
                                    Direction = ParameterDirection.Output
                                };
                                pEventArgs.Command.Parameters.Add(dp.DbDataParameter);
                            }
                            // RD 20170518 [23163] Add table ASSET_EQUITY
                            else if ((_parentTable == Cst.OTCml_TBL.ASSET_ETD) || ((_parentTable == Cst.OTCml_TBL.ASSET_EQUITY)))
                            {
                                pEventArgs.Command.CommandText += " returning IDASSET into :COLIDENTITY";
                                DataParameter dp = new DataParameter(pEventArgs.Command.Connection.ConnectionString, ":COLIDENTITY", DbType.Int32)
                                {
                                    Direction = ParameterDirection.Output
                                };
                                pEventArgs.Command.Parameters.Add(dp.DbDataParameter);
                            }
                        }
                        break;
                }
                //if (_parentTable == Cst.OTCml_TBL.DERIVATIVEATTRIB)
                //    pEventArgs.Command.CommandText += ";" + Cst.CrLf + "select SCOPE_IDENTITY() as COLIDENTITY;";
                pEventArgs.Command.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
            }
        }
        #endregion RowUpdating
        #region RowUpdated
        /// <summary>
        /// RowUpdated
        /// </summary>
        /// EG 20160105 [34091]
        private void RowUpdated(object pSender, RowUpdatedEventArgs pEventArgs)
        {
            if (pEventArgs.Status == UpdateStatus.Continue)
            {
                switch (pEventArgs.StatementType)
                {
                    case StatementType.Insert:
                        DbSvrType svrType = DataHelper.GetDbSvrType(pEventArgs.Command.Connection.ConnectionString);
                        Nullable<int> _valIdentity = null;
                        switch (svrType)
                        {
                            case DbSvrType.dbSQL:
                                if (pEventArgs.Row.Table.Columns.Contains("COLIDENTITY"))
                                    _valIdentity = Convert.ToInt32(pEventArgs.Row["COLIDENTITY"]);
                                break;
                            case DbSvrType.dbORA:
                                if (pEventArgs.Command.Parameters.Contains(":COLIDENTITY"))
                                {
                                    IDataParameter dp = pEventArgs.Command.Parameters[":COLIDENTITY"] as IDataParameter;
                                    _valIdentity = Convert.ToInt32(dp.Value);
                                }
                                break;
                        }
                        if (_valIdentity.HasValue)
                        {

                            Cst.OTCml_TBL _parentTable = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pEventArgs.Row.Table.TableName, true);
                            SetIdentityToContractElements(pEventArgs.Row, _parentTable, _valIdentity.Value);
                            SetIdentityToChildRows(pEventArgs.Row, _parentTable, _valIdentity.Value);
                        }
                        //if (pEventArgs.Row.Table.Columns.Contains("COLIDENTITY"))
                        //{
                        //    int _valIdentity = Convert.ToInt32(pEventArgs.Row["COLIDENTITY"]);
                        //    Cst.OTCml_TBL _parentTable = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pEventArgs.Row.Table.TableName, true);
                        //    SetIdentityToContractElements(pEventArgs.Row, _parentTable, _valIdentity);
                        //    SetIdentityToChildRows(pEventArgs.Row, _parentTable, _valIdentity);
                        //}
                        break;
                }
            }
        }
        #endregion RowUpdated

        #region SetDataDocumentCorporateAction
        // EG [33415/33420]
        // EG 20141027 QtyMultiplier;
        private Cst.ErrLevel SetDataDocumentCorporateAction(IPosRequestCorporateAction pPosRequest, CorporateEventAsset pCorporateEventAsset)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool _isContractSizeAdjusted = m_CorporateAction.IsContractSizeAdjusted(pCorporateEventAsset.category);

            CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
            Adjustment adjustment = _corporateEvent.procedure.adjustment;
            CorporateEventContract _contract = _corporateEvent[pCorporateEventAsset.idDC];
            Nullable<decimal> _qtyMultiplier = adjustment.GetDecimalAIContractValue(CATools.CAElementTypeEnum.ExAdj,CATools.CAElementEnum.qtymul,_contract);

            DataDocumentContainer _dataDocContainer = pPosRequest.Detail.DataDocument;
            _dataDocContainer.OtherPartyPaymentSpecified = false;

            // RD 20180109 [23705]
            // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness
            _dataDocContainer.SetClearedDate(pPosRequest.Detail.BusinessDate.Date);

            IExchangeTradedDerivative _exTrade = (IExchangeTradedDerivative)_dataDocContainer.CurrentProduct.Product;
            IFixTrdCapRptSideGrp _exTradeRptSide = (IFixTrdCapRptSideGrp)_exTrade.TradeCaptureReport.TrdCapRptSideGrp[0];
            _exTrade.TradeCaptureReport.ClearingBusinessDate = new EFS_Date
            {
                DateValue = pPosRequest.Detail.BusinessDate.Date
            };
            _exTrade.TradeCaptureReport.ClearingBusinessDateSpecified = true;
            _exTrade.TradeCaptureReport.TrdTypeSpecified = true;
            //EG 20130607 New TrdType
            _exTrade.TradeCaptureReport.TrdType = TrdTypeEnum.CorporateAction;
            _exTrade.TradeCaptureReport.TrdSubTypeSpecified = true;
            _exTrade.TradeCaptureReport.TrdSubType = TrdSubTypeEnum.InternalTransferOrAdjustment;
            _exTrade.TradeCaptureReport.ExecType = ExecTypeEnum.TriggeredOrActivatedBySystem;
            _exTrade.TradeCaptureReport.LastQtySpecified = true;
            _exTrade.TradeCaptureReport.LastQty = new EFS_Decimal();
            // EG 20140516 Mise à jour du symbole par l'identifier DCEX
            _exTrade.TradeCaptureReport.Instrument.Symbol = pPosRequest.Detail.IdentifierDCEx;
            // Il n'y a pas d'ajustement du ContractSize l'ajustement se fait sur la quantité du trade 
            if (_qtyMultiplier.HasValue)
                _exTrade.TradeCaptureReport.LastQty.DecValue = pPosRequest.Qty * _qtyMultiplier.Value;
            else if (_isContractSizeAdjusted)
                _exTrade.TradeCaptureReport.LastQty.DecValue = pPosRequest.Qty;
            else if (pCorporateEventAsset.rFactorSpecified)
            {
                decimal _reverseRatio = 1 / pCorporateEventAsset.rFactor.valueRounded.DecValue;
                EFS_Round _round = new EFS_Round(Cst.RoundingDirectionSQL.D, 0, _reverseRatio);
                _exTrade.TradeCaptureReport.LastQty.DecValue = pPosRequest.Qty * _round.AmountRounded;
            }
            else
                _exTrade.TradeCaptureReport.LastQty.DecValue = pPosRequest.Qty;

            _exTradeRptSide.Text = m_CorporateAction.identifier;
            _exTradeRptSide.TextSpecified = true;

            // Nouvelles caractéristiques de l'Asset
            IFixInstrument _instrument = _exTrade.TradeCaptureReport.Instrument;
            _instrument.Symbol = pPosRequest.Detail.IdentifierDCEx;
            // EG [33415/33420]
            _instrument.SecurityId = (pCorporateEventAsset.idASSETEXADJ == 0 ? pCorporateEventAsset.idASSETEX.ToString() : pCorporateEventAsset.idASSETEXADJ.ToString());
            if (_instrument.StrikePriceSpecified)
                _instrument.StrikePrice.DecValue = pCorporateEventAsset.exData.strikePrice.valueRounded.DecValue;

            _exTrade.TradeCaptureReport.LastPxSpecified = true;
            switch (pCorporateEventAsset.adjMethod)
            {
                case AdjustmentMethodOfDerivContractEnum.Ratio:
                    decimal _lastPxAdj = _exTrade.TradeCaptureReport.LastPx.DecValue * pCorporateEventAsset.rFactor.valueRounded.DecValue;
                    Rounding _rounding = m_CorporateEventMktRules.rounding.Find(item => item.First == AdjustmentElementEnum.Price).Second;
                    EFS_Round _round = new EFS_Round(_rounding.direction, _rounding.precision, _lastPxAdj);
                    _exTrade.TradeCaptureReport.LastPx.DecValue = _round.AmountRounded;
                    break;
                case AdjustmentMethodOfDerivContractEnum.FairValue:
                    break;
                case AdjustmentMethodOfDerivContractEnum.Package:
                    break;
                default:
                    break;
            }
            return codeReturn;
        }
        #endregion SetDataDocumentCorporateAction
        #region SetIdentityToChildRows
        /// <summary>
        /// Mise à jour des valeurs IDENTITY sur les table ENFANTS
        /// </summary>
        // EG [33415/33420]
        public void SetIdentityToChildRows(DataRow pParentRow, Cst.OTCml_TBL pParentTableName, int pValueIdentity)
        {
            string _columnNameIdentity = string.Empty;
            string _relationName = null;
            switch (pParentTableName)
            {
                case Cst.OTCml_TBL.DERIVATIVECONTRACT:
                    _columnNameIdentity = "IDDC";
                    _relationName = "FK_DA_DC";
                    break;
                case Cst.OTCml_TBL.DERIVATIVEATTRIB:
                    _columnNameIdentity = "IDDERIVATIVEATTRIB";
                    _relationName = "FK_ASSET_DA";
                    break;
                case Cst.OTCml_TBL.ASSET_ETD:
                    _columnNameIdentity = "IDASSET";
                    break;
                case Cst.OTCml_TBL.ASSET_EQUITY:
                    _columnNameIdentity = "IDASSET";
                    _relationName = "FK_RDCMK_EQUITY";
                    break;
            }
            if (StrFunc.IsFilled(_relationName))
            {
                DataRelation _relation = pParentRow.Table.ChildRelations[_relationName];
                if (null != _relation)
                {
                    DataRow[] _childRows = pParentRow.GetChildRows(_relation, DataRowVersion.Default);
                    if (ArrFunc.IsFilled(_childRows))
                    {
                        IEnumerable<DataRow> _rows = _childRows.Where(row => row.RowState == DataRowState.Added);
                        foreach (DataRow _row in _rows)
                            _row[_columnNameIdentity] = pValueIdentity;
                    }
                }
            }
            pParentRow[_columnNameIdentity] = pValueIdentity;
        }
        #endregion SetIdentityToChildRows
        #region SetIdentityToContractElements
        /// <summary>
        /// Mise à jour des valeurs IDEx (DC / DA et ASSET sur les éléments de la liste ContractElements
        /// </summary>
        // EG [33415/33420]
        public void SetIdentityToContractElements(DataRow pParentRow, Cst.OTCml_TBL pParentTableName, int pSourceColumnValue)
        {
            string _sourceColumn = string.Empty;
            Pair<DataTable, string> _target = new Pair<DataTable, string>();
            switch (pParentTableName)
            {
                case Cst.OTCml_TBL.DERIVATIVECONTRACT:
                    _sourceColumn = "IDDC";
                    _target.First = m_DtContract.Second;
                    _target.Second = "IDDCEX";
                    break;
                case Cst.OTCml_TBL.DERIVATIVEATTRIB:
                    _sourceColumn = "IDDERIVATIVEATTRIB";
                    _target.First = m_DtDAttrib.Second;
                    _target.Second = "IDDAEX";
                    break;
                case Cst.OTCml_TBL.ASSET_ETD:
                    _sourceColumn = "IDASSET";
                    _target.First = m_DtAsset.Second;
                    _target.Second = "IDASSETEX";
                    break;
                case Cst.OTCml_TBL.ASSET_EQUITY:
                    _sourceColumn = "IDASSET";
                    _target.First = m_DtAsset_Equity.Second;
                    _target.Second = "IDASSET";
                    break;

            }
            if (null != _target.First)
            {
                DataRow[] _row = _target.First.Select(String.Format(_target.Second + "={0}", pParentRow[_sourceColumn]));
                if (ArrFunc.IsEmpty(_row))
                {
                    _target.Second += "ADJ";
                    _row = _target.First.Select(String.Format(_target.Second + "={0}", pParentRow[_sourceColumn]));
                    if (ArrFunc.IsEmpty(_row))
                    {
                        _target.Second = _target.Second.Replace("EXADJ", "RECYCLED");
                        _row = _target.First.Select(String.Format(_target.Second + "={0}", pParentRow[_sourceColumn]));
                    }
                }
                if (ArrFunc.IsFilled(_row))
                {
                    _row.First().BeginEdit();
                    _row.First()[_target.Second] = pSourceColumnValue;
                    _row.First().EndEdit();
                }
            }
        }
        #endregion SetIdentityToContractElements
        #region SetIdToCorporateContract
        // EG [33415/33420]
        private void SetIdToCorporateContract(CorporateEventContract pContract)
        {
            DataRow _row = m_DtContract.Second.Rows.Find(pContract.IdCEC);
            if (null != _row)
            {
                if (false == Convert.IsDBNull(_row["IDDCEX"]))
                    pContract.idDCEX = Convert.ToInt32(_row["IDDCEX"]);
                if (false == Convert.IsDBNull(_row["IDDCEXADJ"]))
                    pContract.idDCEXADJ = Convert.ToInt32(_row["IDDCEXADJ"]);
                if (false == Convert.IsDBNull(_row["IDDCRECYCLED"]))
                    pContract.idDDCRecycled = Convert.ToInt32(_row["IDDCRECYCLED"]);

                if (pContract.dAttribsSpecified)
                {
                    foreach(CorporateEventDAttrib attrib in pContract.dAttribs)
                    {
                        _row = m_DtDAttrib.Second.Rows.Find(attrib.IdCEDA);
                        if (null != _row)
                        {
                            // EG [33415/33420]
                            if (false == Convert.IsDBNull(_row["IDDAEX"]))
                                attrib.idDAEX = Convert.ToInt32(_row["IDDAEX"]);
                            if (false == Convert.IsDBNull(_row["IDDAEXADJ"]))
                                attrib.idDAEXADJ = Convert.ToInt32(_row["IDDAEXADJ"]);
                            if (attrib.assetsSpecified)
                            {
                                foreach (CorporateEventAsset asset in attrib.assets)
                                {
                                    _row = m_DtAsset.Second.Rows.Find(asset.IdCEA);
                                    if (null != _row)
                                    {
                                        // EG [33415/33420]
                                        if (false == Convert.IsDBNull(_row["IDASSETEX"]))
                                            asset.idASSETEX = Convert.ToInt32(_row["IDASSETEX"]);
                                        if (false == Convert.IsDBNull(_row["IDASSETEXADJ"]))
                                            asset.idASSETEXADJ = Convert.ToInt32(_row["IDASSETEXADJ"]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
        #endregion SetIdToCorporateContract

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        // EG 20160105 [34091] Delete ';' end of query, add dbo.
        private void Update(IDbTransaction pDbTransaction, CorporateEventContract pContract)
        {
            #region Update : Ecritures des CONTRACT, DATTRIB et ASSET nouveaux ou modifiés
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[0].First, @"select dc.* from dbo.DERIVATIVECONTRACT dc");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[0].First, m_DtContract.First, RowUpdating, RowUpdated);
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[1].First, @"select da.* from dbo.DERIVATIVEATTRIB da");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[1].First, m_DtDAttrib.First, RowUpdating, RowUpdated);
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[2].First, @"select asset.* from dbo.ASSET_ETD asset");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[2].First, m_DtAsset.First, RowUpdating, RowUpdated);
            #endregion Update : Ecritures des CONTRACT, DATTRIB et ASSET nouveaux ou modifiés
            SetIdToCorporateContract(pContract);
        }
        #endregion Update

        #region UpdateAssetEquity
        /// <summary>
        /// Update AssetEquity
        /// </summary>
        // EG [33415/33420]
        // EG 20160105 [34091] Delete ';' end of query, add dbo.
        private void UpdateAssetEquity(IDbTransaction pDbTransaction)
        {
            #region Update : Mise à jour de la table ASSET_EQUITY et ASSET_EQUITY_RDCMK
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[3].First, @"select eqty.* from dbo.ASSET_EQUITY eqty");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[3].First, m_DtAsset_Equity.First, RowUpdating, RowUpdated);
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[3].Second, @"select eqty_rdcmk.* from dbo.ASSET_EQUITY_RDCMK eqty_rdcmk");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[3].Second, m_DtAsset_Equity.Second, RowUpdating, RowUpdated);
            #endregion Update : Mise à jour de la table ASSET_EQUITY et ASSET_EQUITY_RDCMK

        }
        #endregion UpdateAssetEquity

        #region UpdateCorpoEventResult
        /// <summary>
        /// Update
        /// </summary>
        // EG 20160105 [34091] Delete ';' end of query, add dbo.
        private void UpdateCorpoEventResult(IDbTransaction pDbTransaction)
        {
            #region Update : Mise à jour des IDs Ex (DC, DA et ASSET) dans les tables CORPOREVENTCONTRACT, CORPOREVENTDATTRIB, CORPOREVENTASSET
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[0].Second, @"select cec.READYSTATE, cec.IDCEC, cec.IDDC, cec.IDDCEX, cec.IDDCEXADJ, cec.IDDCRECYCLED from dbo.CORPOEVENTCONTRACT cec");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[0].Second, m_DtContract.Second, RowUpdating, RowUpdated);
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[1].Second, @"select ceda.READYSTATE, ceda.IDCEDA, ceda.IDDA, ceda.IDDAEX, ceda.IDDAEXADJ from dbo.CORPOEVENTDATTRIB ceda");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[1].Second, m_DtDAttrib.Second, RowUpdating, RowUpdated);
            DataHelper.SetSelectCommandText(pDbTransaction, m_DicDataAdapter[2].Second, @"select cea.READYSTATE, cea.IDCEA, cea.IDASSET, cea.IDASSETEX, cea.IDASSETEXADJ from dbo.CORPOEVENTASSET cea");
            DataHelper.Update(pDbTransaction, m_DicDataAdapter[2].Second, m_DtAsset.Second, RowUpdating, RowUpdated);
            #endregion Update : Mise à jour des IDs Ex (DC, DA et ASSET) dans les tables CORPOREVENTCONTRACT, CORPOREVENTDATTRIB, CORPOREVENTASSET

        }
        #endregion UpdateCorpoEventResult

        #region InsertLinkedTable
        /// EG 20140326 [19783][19791] New
        /// RD 20161129 [22651] Modify
        /// FI 20170919 [23409] Modify
        /// FI 20170925 [23445] Modify 
        /// EG 20220206 [26212][WI565] Contrôle de NON EXISTENCE des nouveaux DC (Ex/ExAdj et Recycled) avant insertion d'un le groupe de contrat du DC Cum)
        /// EG 20220208 [26212][WI565] Concaténation des queries
        /// EG 20230307 [XXXXX] Création des (éventuelles) lignes dans DRVCONTRACTMATRULE pour les DCs résultant de la CA (DCex, DCExAdj et DCRecycled)
        private void InsertLinkedTable(IDbTransaction pDbTransaction, CorporateEventContract pContract)
        {
            if ((0 < pContract.idDCEX) || pContract.idDCEXADJ.HasValue || pContract.idDDCRecycled.HasValue)
            {
                string _CS = pDbTransaction.Connection.ConnectionString;
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(_CS, "IDDC", DbType.Int32), pContract.idDC);
                parameters.Add(DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.DTENABLED), m_CorporateAction.corporateEvent[0].effectiveDate);// FI 20201006 [XXXXX] DbType.Date
                parameters.Add(DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.DTINS), m_DtSys);
                parameters.Add(DataParameter.GetParameter(_CS, DataParameter.ParameterEnum.IDAINS), 1);
                // RD 20161129 [22651] Add parameter TABLENAME
                parameters.Add(new DataParameter(_CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString());

                string sqlInsert = string.Empty;

                if (0 < pContract.idDCEX)
                {
                    parameters.Add(new DataParameter(_CS, "IDDCEX", DbType.Int32), pContract.idDCEX);
                    // EG 20220206 [26212][WI565]
                    sqlInsert = @"insert into dbo.CONTRACTG
                    (IDGCONTRACT, IDXC, CONTRACTCATEGORY, DTENABLED, DTINS, IDAINS, EXTLLINK)
                    select IDGCONTRACT, @IDDCEX, 'DerivativeContract', @DTENABLED, @DTINS, @IDAINS, EXTLLINK
                    from dbo.CONTRACTG cg
                    where (cg.IDXC = @IDDC) and (cg.CONTRACTCATEGORY='DerivativeContract')
                    and not exists (select IDXC from dbo.CONTRACTG where IDXC = @IDDCEX);
                    insert into dbo.EXTLID
                    (TABLENAME, ID, IDENTIFIER, VALUE, DTINS, IDAINS)
                    select TABLENAME, @IDDCEX, IDENTIFIER, VALUE, @DTINS, @IDAINS
                    from dbo.EXTLID ei
                    where (ei.TABLENAME = @TABLENAME) and (ei.ID = @IDDC)
                    and not exists (select ID from dbo.EXTLID where TABLENAME = @TABLENAME and ID = @IDDCEX);
                    insert into dbo.DRVCONTRACTMATRULE
                    (IDDC, IDMATURITYRULE, SEQUENCENO, DTENABLED, DTINS, IDAINS, EXTLLINK)
                    select @IDDCEX, IDMATURITYRULE, SEQUENCENO, @DTENABLED, @DTINS, @IDAINS, EXTLLINK
                    from dbo.DRVCONTRACTMATRULE cmr
                    where (cmr.IDDC = @IDDC)
                    and not exists (select IDDC from dbo.DRVCONTRACTMATRULE where IDDC = @IDDCEX);" + Cst.CrLf;
                }
                if (pContract.idDCEXADJ.HasValue)
                {
                    parameters.Add(new DataParameter(_CS, "IDDCEXADJ", DbType.Int32), pContract.idDCEXADJ.Value);
                    // EG 20220206 [26212][WI565]
                    sqlInsert += @"insert into dbo.CONTRACTG
                    (IDGCONTRACT, IDXC, CONTRACTCATEGORY, DTENABLED, DTINS, IDAINS, EXTLLINK)
                    select IDGCONTRACT, @IDDCEXADJ, 'DerivativeContract', @DTENABLED, @DTINS, @IDAINS, EXTLLINK
                    from dbo.CONTRACTG cg
                    where (cg.IDXC = @IDDC) and (cg.CONTRACTCATEGORY='DerivativeContract')
                    and not exists (select IDXC from CONTRACTG where IDXC = @IDDCEXADJ);
                    insert into dbo.EXTLID
                    (TABLENAME, ID, IDENTIFIER, VALUE, DTINS, IDAINS)
                    select TABLENAME, @IDDCEXADJ, IDENTIFIER, VALUE, @DTINS, @IDAINS
                    from dbo.EXTLID ei
                    where (ei.TABLENAME = @TABLENAME) and (ei.ID = @IDDC)
                    and not exists (select ID from dbo.EXTLID where TABLENAME = @TABLENAME and ID = @IDDCEXADJ);
                    insert into dbo.DRVCONTRACTMATRULE
                    (IDDC, IDMATURITYRULE, SEQUENCENO, DTENABLED, DTINS, IDAINS, EXTLLINK)
                    select @IDDCEXADJ, IDMATURITYRULE, SEQUENCENO, @DTENABLED, @DTINS, @IDAINS, EXTLLINK
                    from dbo.DRVCONTRACTMATRULE cmr
                    where (cmr.IDDC = @IDDC)
                    and not exists (select IDDC from dbo.DRVCONTRACTMATRULE where IDDC = @IDDCEXADJ);" + Cst.CrLf;
                }
                if (pContract.idDDCRecycled.HasValue)
                {
                    parameters.Add(new DataParameter(_CS, "IDDCRECYCLED", DbType.Int32), pContract.idDDCRecycled.Value);
                    // EG 20220206 [26212][WI565]
                    sqlInsert += @"insert into dbo.CONTRACTG
                    (IDGCONTRACT, IDXC, CONTRACTCATEGORY, DTENABLED, DTINS, IDAINS, EXTLLINK)
                    select IDGCONTRACT, @IDDCRECYCLED, 'DerivativeContract', @DTENABLED, @DTINS, @IDAINS, EXTLLINK
                    from dbo.CONTRACTG cg
                    where (cg.IDXC = @IDDC) and (cg.CONTRACTCATEGORY='DerivativeContract')
                    and not exists (select IDXC from CONTRACTG where IDXC = @IDDCRECYCLED);
                    insert into dbo.EXTLID
                    (TABLENAME, ID, IDENTIFIER, VALUE, DTINS, IDAINS)
                    select TABLENAME, @IDDCRECYCLED, IDENTIFIER, VALUE, @DTINS, @IDAINS
                    from dbo.EXTLID ei
                    where (ei.TABLENAME = @TABLENAME) and (ei.ID = @IDDC)
                    and not exists (select ID from dbo.EXTLID where TABLENAME = @TABLENAME and ID = @IDDCRECYCLED);
                    insert into dbo.DRVCONTRACTMATRULE
                    (IDDC, IDMATURITYRULE, SEQUENCENO, DTENABLED, DTINS, IDAINS, EXTLLINK)
                    select @IDDCRECYCLED, IDMATURITYRULE, SEQUENCENO, @DTENABLED, @DTINS, @IDAINS, EXTLLINK
                    from dbo.DRVCONTRACTMATRULE cmr
                    where (cmr.IDDC = @IDDC)
                    and not exists (select IDDC from dbo.DRVCONTRACTMATRULE where IDDC = @IDDCRECYCLED);" + Cst.CrLf;
                }
                QueryParameters qryParameters = new QueryParameters(_CS, sqlInsert, parameters);
                DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
        }
        #endregion InsertLinkedTable

        #region RecoveryTradePostCorporateActions
        /// <summary>
        /// TRADES entrés en portefeuille POSTCA à réaffecter sur l'IDASSET Ex
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pContract"></param>
        /// <param name="pDAttrib"></param>
        /// <param name="pAsset"></param>
        /// <param name="pLstTradeEx"></param>
        /// <returns></returns>
        /// EG 20140326 [19783][19791] New
        /// EG 20150115 [20683]
        /// FI 20150302 [XXXXX] Modify
        // PM 20170911 [23408] Modification du type de pLstTradeEx la liste : List<Pair<int, string>> vers List<CATradeEx>
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private Cst.ErrLevel RecoveryTradePostCorporateActions(string pCS, IDbTransaction pDbTransaction,
            CorporateEventContract pContract, CorporateEventDAttrib pDAttrib, CorporateEventAsset pAsset,
            ref List<CATradeEx> pLstTradeEx)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), m_EntityMarketInfo.IdEM);
            parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), pAsset.idASSET);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness); // FI 20201006 [XXXXX] DbType.Date
            // FI 20150302 [XXXXX] use ReflectionTools.EnumValueName(
            parameters.Add(new DataParameter(pCS, "TRDTYPE", DbType.AnsiString), ReflectionTools.ConvertEnumToString<TrdTypeEnum>(TrdTypeEnum.CorporateAction));

            // FI 20150302 [XXXXX] use dbo.
            string sqlQuery = SQLCst.SELECT + @"tr.IDT, tr.IDENTIFIER, tr.IDASSET, tr.DTBUSINESS
            from dbo.VW_TRADE_POSETD tr
            inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)
            where (tr.DTBUSINESS > @DTBUSINESS) and (tr.IDASSET = @IDASSET) and (tr.IDEM = @IDEM) and (tr.TRDTYPE <> @TRDTYPE)";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);
            DataSet ds = DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                if (0 < nbRow)
                {
                    _ = m_DtContract.First.Select(String.Format("IDDC={0}", pContract.idDCEX)).First();
                    if ((0 == pDAttrib.idDAEX) || (0 < pAsset.idASSETEX))
                    {
                        DataRow _row;
                        if (0 == pDAttrib.idDAEX)
                        {
                            DataRow rowCumDA = m_DtDAttrib.First.Select(String.Format("IDDERIVATIVEATTRIB={0}", pDAttrib.idDA)).First();
                            _row = m_DtDAttrib.Second.Select(String.Format("IDCEDA={0}", rowCumDA["IDCEDA"])).First();
                            InsertDataRowExForTradePostCa(pDAttrib, rowCumDA, pContract.idDCEX, -1000, null);
                            _row["IDDAEX"] = -1000;
                        }
                        if (0 == pAsset.idASSETEX)
                        {
                            DataRow rowCumAsset = m_DtAsset.First.Select(String.Format("IDASSET={0}", pAsset.idASSET)).First();
                            _row = m_DtAsset.Second.Select(String.Format("IDCEA={0}", rowCumAsset["IDCEA"])).First();
                            InsertDataRowExForTradePostCa(pAsset, rowCumAsset, pContract.idDCEX, -1000, -1000);
                            _row["IDASSETEX"] = -1000;
                        }

                        Update(pDbTransaction, pContract);
                    }

                    SQL_AssetETD sql_AssetETD = new SQL_AssetETD(CS, pAsset.idASSETEX)
                    {
                        DbTransaction = pDbTransaction
                    };
                    if (sql_AssetETD.LoadTable(new string[] { "IDASSET", "IDDC", "CONTRACTIDENTIFIER" }))
                    {
                    }

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int idT = Convert.ToInt32(dr["IDT"]);
                        string identifier = dr["IDENTIFIER"].ToString();
                        // Désérialisation du trade
                        DeserializeTrade(pDbTransaction, idT);
                        // Nouvelles caractéristiques de l'Asset
                        IFixInstrument _instrument = ExchangeTradedDerivativeContainer.TradeCaptureReport.Instrument;
                        _instrument.Symbol = sql_AssetETD.DrvContract_Identifier;
                        _instrument.SecurityId = pAsset.idASSETEX.ToString();
                        // Ecriture 
                        EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_TradeLibrary.DataDocument.DataDocument);
                        StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
                        parameters.Clear();
                        parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), idT);
                        parameters.Add(new DataParameter(pCS, "TRADEXML", DbType.Xml), sb.ToString());
                        parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), pAsset.idASSETEX);
                        sqlQuery = SQLCst.UPDATE_DBO + "TRADE set TRADEXML = @TRADEXML, IDASSET = @IDASSET where (IDT = @IDT)";
                        DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());

                        // Suppression des événements et positions
                        // EG 20150115 [20683]
                        // FI 20160524 [XXXXX] Use Cst.ProductGProduct_FUT
                        // FI 20160816 [22146] passage des paramètres idA, pDateSys
                        TradeRDBMSTools.DeleteEvent(pCS, pDbTransaction, idT, Cst.ProductGProduct_FUT, m_PKGenProcess.Session.IdA, OTCmlHelper.GetDateSysUTC(CS));
                        TradeRDBMSTools.DeletePosRequest(pCS, pDbTransaction, idT);

                        // Recalcul des événements
                        // PM 20170911 [23408] Modification du type de pLstTradeEx la liste : List<Pair<int, string>> vers List<CATradeEx>
                        //pLstTradeEx.Add(new Pair<int, string>(idT, identifier));
                        pLstTradeEx.Add(new CATradeEx(idT, identifier));
                    }
                }
            }
            else
                codeReturn = Cst.ErrLevel.NOTHINGTODO;

            return codeReturn;
        }
        #endregion RecoveryTradePostCorporateActions

        #region InsertEqualisationPaymentEvent
        /// <summary>
        /// Insertion de l'Equalisation Payment dans les Events du trade Ex
        /// </summary>
        /// <returns></returns>
        /// PM 20170911 [23408] New
        // EG 20190121 [23249] Remplacement IDataReader par ExecuteScalar
        private Cst.ErrLevel InsertEqualisationPaymentEvent(CATradeEx pTradeEx)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if ((pTradeEx != default(CATradeEx)) && (pTradeEx.EqualisationPayment != default(CAEqualisationPaymentEvent)))
            {
                // PL 20180312 WARNING: Use Read Commited !
                //using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadUncommitted))
                using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS, IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        string sqlSelect = sqlSelect = @"select ev.IDE from dbo.EVENT ev where (ev.IDT = @IDT) and (ev.EVENTCODE='LPC') and (ev.EVENTTYPE = 'AMT')";
                        DataParameters parameters = new DataParameters();
                        parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDT), pTradeEx.IdT);
                        QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);

                        object obj = DataHelper.ExecuteScalar(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        if (null != obj)
                        {
                            int idE_parent = Convert.ToInt32(obj);
                            CAEqualisationPaymentEvent equalisationPayment = pTradeEx.EqualisationPayment;
                            SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                            codeReturn = m_EventQuery.InsertEvent(dbTransaction, pTradeEx.IdT, newIdE, idE_parent, null, 1, 1,
                                equalisationPayment.IdA_Payer, equalisationPayment.IdB_Payer, equalisationPayment.IdA_Receiver, equalisationPayment.IdB_Receiver,
                                EventCodeFunc.LinkedProductClosing, EventTypeFunc.EqualisationPayment,
                                equalisationPayment.DtEffectiveDate, equalisationPayment.DtEffectiveDate, equalisationPayment.DtEffectiveDate, equalisationPayment.DtEffectiveDate,
                                equalisationPayment.Amount, equalisationPayment.IdC, UnitTypeEnum.Currency.ToString(), null, null);

                            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != equalisationPayment.DtEffectiveDate))
                            {
                                codeReturn = m_EventQuery.InsertEventClass(dbTransaction, newIdE, EventClassFunc.Recognition, equalisationPayment.DtEffectiveDate, false);
                                codeReturn = m_EventQuery.InsertEventClass(dbTransaction, newIdE, EventClassFunc.ValueDate, equalisationPayment.DtEffectiveDate, true);
                                codeReturn = m_EventQuery.InsertEventClass(dbTransaction, newIdE, EventClassFunc.Settlement, equalisationPayment.DtSettlement, true);
                            }
                        }
                        else
                        {
                            codeReturn = Cst.ErrLevel.DATANOTFOUND;
                        }
                        DataHelper.CommitTran(dbTransaction);
                    }
                    catch (Exception)
                    {
                        DataHelper.RollbackTran(dbTransaction);
                        throw;
                    }
                }
            }
            return codeReturn;
        }
        #endregion InsertEqualisationPaymentEvent
        #endregion Methods
    }
}

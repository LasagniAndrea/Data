#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process.EventsGen;
using EFS.Status;
using EFS.TradeInformation;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.ClosingReopeningPositions;
using EfsML.CorporateActions;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
using EfsML.v30.PosRequest;
//
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;
using static EfsML.ClosingReopeningPositions.ARQTools;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    public partial class PosKeepingGenProcessBase
    {
        #region Members
        /// <summary>
        /// Liste des instructions de closing/Reopening action
        /// </summary>
        public List<ClosingReopeningAction> m_LstClosingReopeningAction;
        // EG 20240520 [WI930] Copie des EntityMarket traitées avec succès avec un changement de CSS sur le marché (utile pour la validation du CLOSINGDAY sur nouvelle CSS
        public List<IPosKeepingMarket> m_LstEntityMarketInfoChangedAfterCRP;
        /// <summary>
        /// Dictionnaire des trades candidats à Fermeture/Réouverture
        /// </summary>
        public Dictionary<int, ARQTools.TradeCandidate> m_DicARQTradeCandidate;
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20240520 [WI930] Upd
        public (DateTime dtClosing,DateTime dtReopening) m_ClosingReopeningDtBusiness;
        /// <summary>
        /// Locks multi-theading
        /// </summary>
        public static object m_StUserLock = new object();
        #endregion Members
        #region Methods
        #region AddPosRequestClosingReopening
        /// <summary>
        /// Ajout d'une ligne dans POSREQUEST (ClosingPosition|ClosingReopeningPosition) par clé de position
        /// </summary>
        /// <param name="pRequestType">Type d'action (ClosingPosition|ClosingReopeningPosition)</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pPosRequestParent">Id du POSREQUEST parent</param>
        /// <returns></returns>
        // EG 20190613 [24683] New
        public IPosRequest AddPosRequestClosingReopening(Cst.PosRequestTypeEnum pRequestType, ARQTools.TradeKey pTradeKey, IPosRequest pPosRequestParent)
        {
            int newIdPR = 0;
            int idPR_Parent = pPosRequestParent.IdPR;
            IPosRequest _posRequest = (IPosRequest)pPosRequestParent.CloneMain();
            _posRequest.RequestType = pRequestType;
            _posRequest.SetPosKey(pTradeKey.IdI, pTradeKey.AssetCategory, pTradeKey.IdAsset, pTradeKey.IdA_Dealer, pTradeKey.IdB_Dealer, pTradeKey.IdA_Clearer, pTradeKey.IdB_Clearer);
            InitStatusPosRequest(_posRequest, newIdPR, idPR_Parent, ProcessStateTools.StatusProgressEnum);
            
            //PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, _posRequest, m_PKGenProcess.appInstance, LogHeader.IdProcess, idPR_Parent);
            PosKeepingTools.AddNewPosRequest(CS, null, out newIdPR, _posRequest, m_PKGenProcess.Session, IdProcess, idPR_Parent);
            _posRequest.IdPR = newIdPR;
            return _posRequest;
        }
        #endregion AddPosRequestClosingReopening

        #region GetHeapSize
        private int GetHeapSize(ARQTools.TradeKey pTradeKey)
        {
            int heapSize = ProcessBase.GetHeapSize(ParallelProcess.ClosingReopeningWritingTrade);
            ClosingReopeningAction action = pTradeKey.actionReference;
            if ((action.Closing.Mode != TransferModeEnum.ReverseTrade) || (action.ReopeningSpecified && (action.Reopening.Mode != TransferModeEnum.Trade)))
                heapSize = 1;
            return heapSize;
        }
        #endregion GetHeapSize

        #region LoadClosingReopeningAction
        /// <summary>
        /// Chargement des instructions de ClosingReopening
        /// - pour une ENTITE donnée
        /// - à une date donnée (DTBUSINESS)
        /// - à un mode donné (SOD|EOD)
        /// (Alimentation de m_LstClosingReopeningAction) 
        /// <para>Retourne Cst.ErrLevel.SUCCESS</para>
        /// </summary>
        /// <returns>Cst.ErrLevel</returns>
        // EG 20190308 New
        // EG 20190613 [24683] Upd
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20230906 [WI701] Closing/reopening module : End Of Day and Closing Day Process(Gestion Marchés sur IsinUnderlyingContract)
        // EG 20231030 [WI725] Upd Closing/Reopening : ARQFilters & EFFECTIVEENDDATE
        // EG 20240520 [WI930] Add CSSCustodian columns
        // EG 20240520 [WI930] Upd Gestion des dates pour le CLOSING et REOPENING en fonction du Timing spécifié
        protected virtual Cst.ErrLevel LoadClosingReopeningAction()
        {
            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 8849), (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay ? 0 : 1),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("ENTITY"), m_MasterPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(Queue.GetStringValueIdInfoByKey("CSSCUSTODIAN"), m_MasterPosRequest.IdA_CssCustodian)),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MasterPosRequest.DtBusiness))));
            Logger.Write();

            m_ClosingReopeningDtBusiness = (m_PosRequest.DtBusiness, m_PosRequest.DtBusiness);
            m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarketLock(m_MarketPosRequest.IdEM);

            SettlSessIDEnum currentTiming;
            SettlSessIDEnum currentTiming2;
            switch (m_MasterPosRequest.RequestType)
            {
                case Cst.PosRequestTypeEnum.ClosingDay:
                    currentTiming = SettlSessIDEnum.StartOfDay;
                    currentTiming2 = SettlSessIDEnum.EndOfDayPlusStartOfDay;
                    IOffset offset = m_Product.CreateOffset(PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness);
                    IBusinessDayAdjustments bda = m_Product.CreateBusinessDayAdjustments(BusinessDayConventionEnum.FOLLOWING, m_EntityMarketInfo.IdBC);
                    DateTime _dtNext  = Tools.ApplyOffset(CS, m_PosRequest.DtBusiness, offset, bda, null);
                    m_ClosingReopeningDtBusiness = (m_PosRequest.DtBusiness, _dtNext);
                    break;
                case Cst.PosRequestTypeEnum.EndOfDay:
                    currentTiming = SettlSessIDEnum.EndOfDay;
                    currentTiming2 = currentTiming;
                    break;
                default:
                    currentTiming = SettlSessIDEnum.Intraday;
                    currentTiming2 = currentTiming;
                    break;
            }

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "DTEFFECTIVE", DbType.Date), m_ClosingReopeningDtBusiness.dtReopening);
            parameters.Add(new DataParameter(CS, "IDA", DbType.Int32), m_PosRequest.IdA_Entity);
            parameters.Add(new DataParameter(CS, "TIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(currentTiming));
            parameters.Add(new DataParameter(CS, "TIMING2", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(currentTiming2));

            string sqlSelect = @"select arq.IDARQ, arq.IDENTIFIER, arq.DISPLAYNAME, arq.DESCRIPTION, 
            arq.REQUESTTYPE, arq.TIMING, arq.READYSTATE, arq.EFFECTIVEDATE, arq.EFFECTIVEENDDATE, arq.IDA_ENTITY,
            arq.TYPEINSTR, arq.IDINSTR, arq.TYPECONTRACT , arq.IDCONTRACT,
            arq.TYPEDEALER_C, arq.IDDEALER_C, arq.TYPECLEARER_C, arq.IDCLEARER_C, arq.TYPECSSCUSTODIAN_C, arq.IDCSSCUSTODIAN_C, 
            arq.MODE_C, arq.ISSUMCLOSINGAMT_C, arq.ISDELISTING_C,arq.EQTYPRICE_C, arq.FUTPRICE_C, arq.OTHERPRICE_C, arq.FEEACTION_C, mnu_c.IDMENU as FEEIDMENU_C,
            arq.TYPEDEALER_O, arq.IDDEALER_O, arq.TYPECLEARER_O, arq.IDCLEARER_O, arq.TYPECSSCUSTODIAN_O, arq.IDCSSCUSTODIAN_O, 
            arq.MODE_O, arq.EQTYPRICE_O, arq.FUTPRICE_O, arq.OTHERPRICE_O, arq.FEEACTION_O, mnu_o.IDMENU as FEEIDMENU_O,
            arq.BUILDINFO,
            arq.ARQFILTER, arqf.DISPLAYNAME as DN_FILTER, arqf.DESCRIPTION as DESC_FILTER, arqf.DTENABLED as DTENABLED_FILTER, arqf.DTDISABLED as DTDISABLED_FILTER, arqf.FILTER as QRY_FILTER
            from dbo.ACTIONREQUEST arq
            left outer join dbo.ACTIONREQUESTFILTER arqf on (arqf.IDENTIFIER = arq.ARQFILTER)
            left outer join dbo.VW_ALL_VW_PERMIS_MENU mnu_c on (mnu_c.IDPERMISSION = arq.FEEACTION_C)
            left outer join dbo.VW_ALL_VW_PERMIS_MENU mnu_o on (mnu_o.IDPERMISSION = arq.FEEACTION_O)
            -- where (arq.EFFECTIVEDATE = @DTEFFECTIVE) and (isnull(arq.IDA_ENTITY, @IDA) = @IDA) and 
            where ((arq.EFFECTIVEDATE = @DTEFFECTIVE) or ((arq.EFFECTIVEDATE < @DTEFFECTIVE) and (isnull(arq.EFFECTIVEENDDATE, @DTEFFECTIVE) >= @DTEFFECTIVE))) and 
            (isnull(arq.IDA_ENTITY, @IDA) = @IDA) and (arq.TIMING in (@TIMING,@TIMING2)) and (arq.READYSTATE = 'REGULAR')" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, parameters);
            DataSet ds = OTCmlHelper.GetDataSetWithIsolationLevel(CS, IsolationLevel.ReadCommitted, qryParameters, null);
            if (null != ds)
            {
                int nbRow = ds.Tables[0].Rows.Count;
                if (0 < nbRow)
                {
                    m_LstClosingReopeningAction = new List<ClosingReopeningAction>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ClosingReopeningAction item = new ClosingReopeningAction(CS);
                        item.SetActionRequest(row);
                        // EG 20231214 [WI725] Closing/Reopening : On écrase la date effective par la date business en cours pour gérer la perpétualité des instructions 
                        item.effectiveDate = m_ClosingReopeningDtBusiness.dtReopening;
                        // EG 20230906 [WI701] Closing/reopening module : End Of Day and Closing Day Process(Gestion Marchés sur IsinUnderlyingContract)
                        if (IsARQCandidate(item, row))
                            m_LstClosingReopeningAction.Add(item);
                    }
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion LoadClosingReopeningAction
        /// <summary>
        /// UNIQUEMENT SI TYPECONTRACT = TypeContractEnum.IsinUnderlyerContract
        /// --------------------------------------------------------------------
        /// Contrôle pour définir si la ligne d'instruction de Closing/Reopening est candidate
        /// lors du traitement EOD|SOD du marché en cours
        /// = Le marché en cours doit être le même que celui du DerivativeContract qui est référencé pour l'IsinUnderlyingContract
        /// </summary>
        /// <param name="pItem"></param>
        /// <param name="pRow"></param>
        /// <returns></returns>
        // EG 20230906 [WI701] Closing/reopening module : End Of Day and Closing Day Process(Gestion Marchés sur IsinUnderlyingContract)
        private bool IsARQCandidate(ClosingReopeningAction pItem, DataRow pRow)
        {
            bool isCandidate = true;
            // Cas Spécial (on récupère l'IDM du marché)
            if (pItem.EnvironmentSpecified && pItem.Environment.contractSpecified && (pItem.Environment.contract.type == TypeContractEnum.IsinUnderlyerContract))
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(NormMsgFactoryMQueue), pRow["BUILDINFO"].ToString());
                pItem.normMsgFactoryMQueue = (NormMsgFactoryMQueue)CacheSerializer.Deserialize(serializeInfo);
                if (pItem.normMsgFactoryMQueue.buildingInfo.parametersSpecified)
                {
                    MQueueparameter _parameter = pItem.normMsgFactoryMQueue.buildingInfo.parameters["CONTRACTTYPE"];
                    Pair<string, int> _parameterSplit = pItem.Environment.SplitParameter(CS, "-", _parameter.name);
                    pItem.Environment.contract.identifier = _parameter.name;
                    pItem.Environment.contract.idM = _parameterSplit.Second;
                    isCandidate = (m_MarketPosRequest.IdMSpecified && (m_MarketPosRequest.IdM == pItem.Environment.contract.idM));
                }
            }
            return isCandidate;
        }

        #region EOD_ClosingReopeningActionsGen
        /// <summary>
        /// Traitement de ClosingReopeningPosition
        /// - Chargement des trades en position et alimentation des contextes par matchage
        /// - Assemblage des trades candidats présents dans chaque contexte par regroupement 
        /// - Traitement final de closing/Reopening (avec Offsetting et Calcul des Cash-Flows)
        /// </summary>
        /// <returns></returns>
        // EG 20190308 New
        // EG 20190613 [24683] Upd
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        protected virtual Cst.ErrLevel EOD_ClosingReopeningActionsGen()
        {
            m_PosRequest = RestoreMarketRequest;

            bool isParallelCalculation = ProcessBase.IsParallelProcess(ParallelProcess.ClosingReopeningCalculation);
            bool isParallelWriting = ProcessBase.IsParallelProcess(ParallelProcess.ClosingReopeningWriting);

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5032), (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay ? 0 : 2),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_PosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness)),
                new LogParam(isParallelCalculation ? "YES" : "NO"),
                new LogParam(isParallelCalculation ? Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.ClosingReopeningCalculation)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.ClosingReopeningCalculation)) : "-"),
                new LogParam(isParallelWriting ? "YES" : "NO"),
                new LogParam(isParallelWriting ? 
                    Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.ClosingReopeningWriting)) + "/" +
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.ClosingReopeningWriting)) +
                    "-" + 
                    Convert.ToString(m_PKGenProcess.GetHeapSize(ParallelProcess.ClosingReopeningWritingTrade)) + "/" + 
                    Convert.ToString(m_PKGenProcess.GetMaxThreshold(ParallelProcess.ClosingReopeningWritingTrade))
                    : "-")));
            Logger.Write();

            m_PKGenProcess.ProcessCacheContainer.DbTransaction = null;
            m_EntityMarketInfo = m_PKGenProcess.ProcessCacheContainer.GetEntityMarketLock(m_MarketPosRequest.IdEM);

            Cst.ErrLevel codeReturn = LoadClosingReopeningAction();

            SQLUP.GetId(out int newIdPR, CS, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, 1);

            // Insertion POSREQUEST (CLOSINGREOPENING)    
            IPosRequest _posRequestARQ = InsertPosRequestGroupLevel(Cst.PosRequestTypeEnum.ClosingReopeningPosition, newIdPR, m_PosRequest, m_PosRequest.IdEM,
                ProcessStateTools.StatusProgressEnum, m_PosRequest.IdPR, m_LstSubPosRequest, m_MarketPosRequest.GroupProductEnum);

            try
            {
                // Reset des contextes (POIDS, IDT candidates)
                if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                    codeReturn = ResetTradeCandidatesBeforeProcessing();

                // Chargement des trades en position et alimentation des contextes par matchage
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = LoadTradeForClosingReopeningAction();
                }

                // Assemblage des trades candidats présents dans chaque contexte par regroupement 
                // (en fonction de leur clé - Données identiques)
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = GroupTradeCandidates();
                }

                // Traitement final de closing/Reopening
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    if (false == IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref codeReturn))
                        codeReturn = TradeClosingReopeningGen(_posRequestARQ);
                }
            }
            catch (Exception ex)
            {
                /// EG 20140326 [19775] 
                codeReturn = Cst.ErrLevel.FAILURE;

                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                m_PKGenProcess.ProcessState.AddCriticalException(ex);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5101), 0,
                    new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
            }
            finally
            {
                // Update POSREQUEST (CORPOACTION)
                switch (codeReturn)
                {
                    case Cst.ErrLevel.SUCCESS:
                        _posRequestARQ.Status = ProcessStateTools.StatusSuccessEnum;
                        break;
                    case Cst.ErrLevel.NOTHINGTODO:
                        _posRequestARQ.Status = ProcessStateTools.StatusNoneEnum;
                        codeReturn = Cst.ErrLevel.SUCCESS;
                        break;
                    case Cst.ErrLevel.IRQ_EXECUTED:
                        _posRequestARQ.Status = ProcessStateTools.StatusInterruptEnum;
                        break;
                    case Cst.ErrLevel.QUOTENOTFOUND:
                    case Cst.ErrLevel.QUOTEDISABLED:
                    case Cst.ErrLevel.CLOSINGREOPENINGREJECTED:
                        _posRequestARQ.Status = ProcessStateTools.StatusErrorEnum;
                        //codeReturn = Cst.ErrLevel.SUCCESS;
                        break;
                    default:
                        _posRequestARQ.Status = ProcessStateTools.StatusErrorEnum;
                        break;
                }
                
                PosKeepingTools.UpdatePosRequest(CS, null, newIdPR, _posRequestARQ, m_PKGenProcess.Session.IdA, IdProcess, m_MarketPosRequest.IdPR);
            }
            return codeReturn;
        }
        #endregion EOD_ClosingReopeningActionsGen
        #region GroupTradeCandidates
        /// <summary>
        /// Assemblage des trades candidats présents dans chaque contexte par regroupement 
        /// </summary>
        /// <returns></returns>
        // EG 20190308 New
        // EG 20190318 Upd ClosingReopening position Step3
        private Cst.ErrLevel GroupTradeCandidates()
        {
            
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8851), (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay ? 1 : 3),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            List<ClosingReopeningAction> _lst = m_LstClosingReopeningAction.FindAll(action => (0 < action.Results.LstIdTCandidate.Count));
            if (null != _lst)
            {
                // Regroupement des trades
                _lst.ForEach(action =>
                {
                    // Filtrage des trades sur la base de ceux candidats dans le contexte courant
                    List<ARQTools.TradeKey> _lstTradeKey = (
                    from idTCandidate in action.Results.LstIdTCandidate
                    join tradeCandidate in m_DicARQTradeCandidate on idTCandidate equals tradeCandidate.Key
                    select new ARQTools.TradeKey(tradeCandidate.Value.posKeys)).ToList();

                    //// Agrégation sur les valeurs identiques
                    List<IGrouping<ARQTools.TradeKey, ARQTools.TradeKey>> _lstTradeKeyGroup =
                        _lstTradeKey.GroupBy(group => group, new ARQTools.TradeKeyComparer()).ToList();

                    action.AssembleTrade(m_DicARQTradeCandidate, _lstTradeKeyGroup);

                });
                codeReturn = Cst.ErrLevel.SUCCESS;
            }
            return codeReturn;
        }
        #endregion GroupTradeCandidates

        #region TradeClosingReopeningGen
        /// <summary>
        /// Traitement des trades
        /// - Calcul du poids des contextes pour traiter les n-uplets dans le bon ordre
        /// - Tri par POIDS de contexte
        /// - Construction d'un LOG
        /// - Traitement
        /// </summary>
        // EG 20190308 New
        // EG 20190318 Upd ClosingReopening position Step3
        // EG 20190613 [24683] Upd
        private Cst.ErrLevel TradeClosingReopeningGen(IPosRequest pPosRequestARQ)
        {
            ProcessState processStateMain = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            m_PosRequest = pPosRequestARQ;

            
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8852), (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay ? 1 : 3),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

            // Calcul du poids des contextes pour traiter les n-uplets dans le bon ordre
            if (Cst.ErrLevel.SUCCESS == FinalWeighting())
            {
                m_LstClosingReopeningAction.ForEach(action =>
                {
                    // Suppression des clés sans trades
                    if (null != action.Results.Positions)
                        action.Results.Positions.ToList().Where(item => (null == item.Value)).ToList().ForEach(item => action.Results.Positions.Remove(item.Key));

                });

                // Tri par POIDS de contexte
                m_LstClosingReopeningAction = m_LstClosingReopeningAction.OrderByDescending(action => action.Results.ResultMatching).ToList();

                // Si LOG >= Mode DEFAULT construction du fichier LOG START
                if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL3))
                    SaveResultToXMLFile("CANDIDATES");

                // Si aucun candidat après construction alors on sort...
                bool isContinue = m_LstClosingReopeningAction.Exists(action => action.HasDataToProcess);

                // EG 20190318 Upd ClosingReopening position Step3
                if (isContinue)
                {
                    User user = new User(m_PKGenProcess.Session.IdA, null, RoleActor.SYSADMIN);

                    m_LstClosingReopeningAction.ForEach(action =>
                    {
                        // Traitement
                        if (action.HasDataToProcess)
                        {
                        
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8853), (action.timing == SettlSessIDEnum.EndOfDay ? 2 : 4),
                                new LogParam(LogTools.IdentifierAndId(action.identifier, action.IdARQ)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                                new LogParam(m_MarketPosRequest.GroupProductValue),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness)),
                                new LogParam(action.DisplayMode),
                                new LogParam(action.DisplayNbPositionAndTrades)));

                            #region Calculation
                            if (false == IRQTools.IsIRQRequested(ProcessBase, ProcessBase.IRQNamedSystemSemaphore, ref processStateMain))
                            {
                                processState = CommonGenThreading(ParallelProcess.ClosingReopeningCalculation, action.Results.Positions.ToList(), user);
                                processStateMain.SetErrorWarning(processState.Status, processState.CodeReturn);
                                // Si LOG = Mode DEFAULT construction du fichier LOG FINAL APRES CALCUL
                                if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL3))
                                    SaveResultToXMLFile(action, "CALCULATION");
                            }
                            #endregion Calculation

                            #region Writing
                            if (false == IRQTools.IsIRQRequested(ProcessBase, ProcessBase.IRQNamedSystemSemaphore, ref processStateMain))
                            {
                                // Seuls les positions avec le status SUCCESS sont conservées
                                processState = CommonGenThreading(ParallelProcess.ClosingReopeningWriting, action.Results.Positions.ToList(), user);
                                processStateMain.SetErrorWarning(processState.Status, processState.CodeReturn);
                                // Si LOG = Mode DEFAULT construction du fichier LOG FINAL APRES ECRITURE
                                if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL3))
                                    SaveResultToXMLFile(action, "WRITING");
                            }
                            #endregion Writing
                        };
                    });

                    #region Mise à jour ENTITYMARKET
                    // EG 20240520 [WI930] New
                    if (Cst.ErrLevel.SUCCESS == processStateMain.CodeReturn)
                    {
                        processState = FinalClosingReopening();
                        processStateMain.SetErrorWarning(processState.Status, processState.CodeReturn);
                    }
                    #endregion Mise à jour ENTITYMARKET

                }
                else
                {
                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8855), (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay ? 2 : 4),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                        new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                        new LogParam(m_MarketPosRequest.GroupProductValue),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

                    processStateMain = new ProcessState(ProcessStateTools.StatusNoneEnum, Cst.ErrLevel.NOTHINGTODO);
                }
            }
            return processStateMain.CodeReturn;
        }
        #endregion TradeClosingReopeningGen


        #region Mise à jour ENTITYMARKET
        /// <summary>
        /// 1. Mise à jour du ROWATTRIBUT sur ENTITYMARKET (L = LOCKED)
        /// 2. Lecture d'ENTITYMARKET pour voir si toutes ont été traitées pour le marché
        /// 3. Si toutes les ENTITYMARKET sur le marché ont été traitées, on peut mettre à jour le marché avec sa nouvelle CSS
        /// 4. Mise à jour du ROWATTRIBUT sur ENTITYMARKET (null)
        /// </summary>
        /// <returns></returns>
        // EG 20240520 [WI930] New
        private ProcessState FinalClosingReopening()
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            if (false == m_EntityMarketInfo.IdA_CustodianSpecified)
            {
                if (m_LstClosingReopeningAction.Exists(action => action.HasDataToProcess && action.ReopeningSpecified && action.Reopening.cssCustodianSpecified))
                {
                    // Mise à jour du ROWATTRIBUT (L = LOCKED)
                    DataParameters parameters = new DataParameters();
                    parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_EntityMarketInfo.IdEM);
                    string sqlUpdate = $"update dbo.ENTITYMARKET set ROWATTRIBUT = 'L' where (IDEM = @IDEM)";
                    QueryParameters qry = new QueryParameters(CS, sqlUpdate, parameters);
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    // Lecture d'ENTITYMARKET pour voir si toutes ont été traitées pour le marché
                    parameters.Clear();
                    parameters.Add(new DataParameter(CS, "IDM", DbType.Int32), m_EntityMarketInfo.IdM);
                    parameters.Add(new DataParameter(CS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN), m_EntityMarketInfo.IdM);
                    string sqlSelect = $"select 1 from dbo.ENTITYMARKET where (IDM = @IDM) and (ROWATTRIBUT != 'L')";
                    qry = new QueryParameters(CS, sqlSelect, parameters);
                    object obj = DataHelper.ExecuteScalar(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    if (null == obj)
                    {
                        // Toutes les ENTITYMARKET sur le marché ont été traitées, on peut mettre à jour le marché
                        m_LstClosingReopeningAction.ForEach(action =>
                        {
                            if (action.HasDataToProcess && action.ReopeningSpecified && action.Reopening.cssCustodianSpecified && (false == m_EntityMarketInfo.IdA_CustodianSpecified))
                            {
                                ExActor exCssCustodian = new ExActor();
                                exCssCustodian.SetActorForReopening(this.PKGenProcess, action.Reopening.cssCustodian, m_EntityMarketInfo.IdA_CSS, null);
                                // EG 20240628 [OTRS1000275] New Contrôle if multi CRP instructions for a market
                                if (0 < exCssCustodian.IdA)
                                {
                                    parameters.Clear();
                                    parameters.Add(new DataParameter(CS, "IDM", DbType.Int32), m_EntityMarketInfo.IdM);
                                    parameters.Add(new DataParameter(CS, "IDA", DbType.Int32), exCssCustodian.IdA);
                                    sqlUpdate = $"update dbo.MARKET set IDA = @IDA where (IDM = @IDM)";
                                    qry = new QueryParameters(CS, sqlUpdate, parameters);
                                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                                    m_EntityMarketInfo.IdA_CSS = exCssCustodian.IdA;
                                    if (exCssCustodian.IdA_IdentifierSpecified)
                                        m_EntityMarketInfo.IdA_CSS_Identifier = exCssCustodian.IdA_Identifier;
                                    m_LstEntityMarketInfoChangedAfterCRP.Add(m_EntityMarketInfo);
                                }
                            }
                        });

                        // Mise à jour du ROWATTRIBUT (null)
                        parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_EntityMarketInfo.IdM);
                        sqlUpdate = $"update dbo.ENTITYMARKET set ROWATTRIBUT = null where (IDM = @IDM)";
                        qry = new QueryParameters(CS, sqlUpdate, parameters);
                        DataHelper.ExecuteNonQuery(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                    }
                }
            }
            return processState;
        }
        #endregion Mise à jour ENTITYMARKET
        #region ClosingReopeningCalculation
        /// <summary>
        /// Traitement des trades (par clé de position) pour un contexte donné 
        /// Phase de calcul et d'initialisation des trades à générer
        /// </summary>
        // EG 20190308 New
        // EG 20190318 Upd ClosingReopening position Step3
        // EG 20190613 [24683] Upd
        private ProcessState ClosingReopeningCalculation(ARQTools.TradeKey pTradeKey, List<ARQTools.TradeCandidate> pLstTradeCandidates)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            ClosingReopeningAction action = pTradeKey.actionReference;

            #region LOG MODE FULL
            if (m_PKGenProcess.IsLevelToLog(LogLevelDetail.LEVEL4))
            {
                string lstTradeToLog = string.Empty;
                pLstTradeCandidates.ForEach(trade => lstTradeToLog += LogTools.IdentifierAndId(trade.identifier, trade.idT) + " ");

                lstTradeToLog = lstTradeToLog.Substring(0, Math.Min(125, lstTradeToLog.Length));
                if (125 == lstTradeToLog.Length)
                    lstTradeToLog += "...";

                
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 8854), (pTradeKey.IsTimingEndOfDay ? 3 : 5),
                    new LogParam(LogTools.IdentifierAndId(action.identifier, action.IdARQ)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                    new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                    new LogParam(m_MarketPosRequest.GroupProductValue),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness)),
                    new LogParam(lstTradeToLog)));
            }
            #endregion LOG MODE FULL

            // Lecture des Prix utilisés pour Fermeture/Réouverture
            pTradeKey.statusCalculation = pTradeKey.SetPriceUsedForClosingReopening(m_PKGenProcess);
            pTradeKey.statusCalculationSpecified = true;

            // Préparation des trades de fermeture/réouverture
            // Fermeture
            SetTradesAccordingToClosingReopeningMode(pTradeKey, pLstTradeCandidates, action.Closing);
            // Réouverture
            if ((action.requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition) && action.ReopeningSpecified)
                SetTradesAccordingToClosingReopeningMode(pTradeKey, pLstTradeCandidates, action.Reopening);

            // Recherche des acteurs de réouverture
            processState = pTradeKey.SetActorForReopening(m_PKGenProcess);
            pTradeKey.statusCalculation.SetErrorWarning(processState.Status, processState.CodeReturn);

            return pTradeKey.statusCalculation;
        }
        #endregion ClosingReopeningCalculation
        #region ClosingReopeningWriting
        /// <summary>
        /// Traitement des trades (par clé de position) pour un contexte donné
        /// Phase d'écriture des trades
        /// </summary>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pSource">Liste des trades candidats pour une clé de position</param>
        /// <param name="pUser">Utilisateur</param>
        /// <returns></returns>
        // EG 20190613 [24683] New
        private ProcessState ClosingReopeningWriting(ARQTools.TradeKey pTradeKey, List<ARQTools.TradeCandidate> pSource, User pUser)
        {
            ProcessState processStateMain = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            ClosingReopeningAction action = pTradeKey.actionReference;
            List<ARQTools.OverrideTradeCandidate> lstTrades = null;
            ARQTools.OverrideTradeCandidate tradeClosing = null;
            ARQTools.OverrideTradeCandidate tradeReopening = null;

            IPosRequest _posRequestARQ = null;
            try
            {
                _posRequestARQ = AddPosRequestClosingReopening(action.requestType, pTradeKey, m_PosRequest);
                if (false == IRQTools.IsIRQRequested(ProcessBase, ProcessBase.IRQNamedSystemSemaphore, ref processStateMain))
                {
                    if (pTradeKey.SetProductInstrument(CS))
                    {
                        pTradeKey.posKeepingData = CreatePosKeepingData(pTradeKey);

                        switch (action.requestType)
                        {
                            case Cst.PosRequestTypeEnum.ClosingPosition:
                                #region Fermeture de position
                                // IDbTransaction en bout d'appel local
                                lstTrades = (from source in pSource
                                             where source.overrideClosingSpecified
                                             select source.overrideClosing).ToList();
                                processState = ClosingWriting(_posRequestARQ, pTradeKey, lstTrades, pUser);
                                #endregion Fermeture de position
                                break;
                            case Cst.PosRequestTypeEnum.ClosingReopeningPosition:
                                #region Fermeture et réouverture de position
                                if ((action.Closing.Mode == TransferModeEnum.ReverseTrade) && (action.Reopening.Mode == TransferModeEnum.Trade))
                                {
                                    // IDbTransaction en bout d'appel local sur la paire overrideClosing,overrideReopening
                                    List<Pair<ARQTools.OverrideTradeCandidate, ARQTools.OverrideTradeCandidate>> trades =
                                        (from source in pSource
                                         where source.overrideClosingSpecified && source.overrideReopeningSpecified
                                         select new Pair<ARQTools.OverrideTradeCandidate, ARQTools.OverrideTradeCandidate>(source.overrideClosing, source.overrideReopening)).ToList();
                                    if (0 < trades.Count)
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, trades);
                                }
                                else if ((action.Closing.Mode == TransferModeEnum.ReverseTrade) && (action.Reopening.Mode == TransferModeEnum.LongShortPosition))
                                {
                                    // List<T>,T>
                                    lstTrades = (from source in pSource
                                                 where source.side == "1" && source.overrideClosingSpecified
                                                 select source.overrideClosing).ToList();
                                    tradeReopening = (from source in pSource
                                                      where source.side == "1" && source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((0 < lstTrades.Count) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, lstTrades, tradeReopening);

                                    lstTrades = (from source in pSource
                                                 where source.side == "2" && source.overrideClosingSpecified
                                                 select source.overrideClosing).ToList();
                                    tradeReopening = (from source in pSource
                                                      where source.side == "2" && source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((0 < lstTrades.Count) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, lstTrades, tradeReopening);
                                }
                                else if (((action.Closing.Mode == TransferModeEnum.ReverseTrade) || (action.Closing.Mode == TransferModeEnum.ReverseLongShortPosition)) &&
                                        (action.Reopening.Mode == TransferModeEnum.SyntheticPosition))
                                {
                                    // List<T>, T                         
                                    lstTrades = (from source in pSource
                                                 where source.overrideClosingSpecified
                                                 select source.overrideClosing).ToList();
                                    tradeReopening = (from source in pSource
                                                      where source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((0 < lstTrades.Count) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, lstTrades, tradeReopening);
                                }
                                else if ((action.Closing.Mode == TransferModeEnum.ReverseLongShortPosition) && (action.Reopening.Mode == TransferModeEnum.Trade))
                                {
                                    // T , List<T>
                                    tradeClosing = (from source in pSource
                                                    where source.side == "1" && source.overrideClosingSpecified
                                                    select source.overrideClosing).FirstOrDefault();
                                    lstTrades = (from source in pSource
                                                 where source.side == "1" && source.overrideReopeningSpecified
                                                 select source.overrideReopening).ToList();
                                    if ((0 < lstTrades.Count) && (null != tradeClosing))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, tradeClosing, lstTrades);

                                    tradeClosing = (from source in pSource
                                                    where source.side == "2" && source.overrideClosingSpecified
                                                    select source.overrideClosing).FirstOrDefault();
                                    lstTrades = (from source in pSource
                                                 where source.side == "2" && source.overrideReopeningSpecified
                                                 select source.overrideReopening).ToList();
                                    if ((0 < lstTrades.Count) && (null != tradeClosing))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, tradeClosing, lstTrades);
                                }
                                else if ((action.Closing.Mode == TransferModeEnum.ReverseLongShortPosition) && (action.Reopening.Mode == TransferModeEnum.LongShortPosition))
                                {
                                    // T, T
                                    tradeClosing = (from source in pSource
                                                    where source.side == "1" && source.overrideClosingSpecified
                                                    select source.overrideClosing).FirstOrDefault();
                                    tradeReopening = (from source in pSource
                                                      where source.side == "1" && source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((null != tradeClosing) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, tradeClosing, tradeReopening);

                                    tradeClosing = (from source in pSource
                                                    where source.side == "2" && source.overrideClosingSpecified
                                                    select source.overrideClosing).FirstOrDefault();
                                    tradeReopening = (from source in pSource
                                                      where source.side == "2" && source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((null != tradeClosing) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, tradeClosing, tradeReopening);
                                }
                                else if ((action.Closing.Mode == TransferModeEnum.ReverseLongShortPosition) && (action.Reopening.Mode == TransferModeEnum.SyntheticPosition))
                                {
                                    // List<T> , T
                                    lstTrades = (from source in pSource
                                                 where source.overrideClosingSpecified
                                                 select source.overrideClosing).ToList();
                                    tradeReopening = (from source in pSource
                                                      where source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((0 < lstTrades.Count) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, lstTrades, tradeReopening);
                                }
                                else if ((action.Closing.Mode == TransferModeEnum.ReverseSyntheticPosition) &&
                                        ((action.Reopening.Mode == TransferModeEnum.Trade) || (action.Reopening.Mode == TransferModeEnum.LongShortPosition)))
                                {
                                    // T, List<T>
                                    tradeClosing = (from source in pSource
                                                    where source.overrideClosingSpecified
                                                    select source.overrideClosing).FirstOrDefault();
                                    lstTrades = (from source in pSource
                                                 where source.overrideReopeningSpecified
                                                 select source.overrideReopening).ToList();
                                    if ((0 < lstTrades.Count) && (null != tradeClosing))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, tradeClosing, lstTrades);
                                }
                                else if ((action.Closing.Mode == TransferModeEnum.ReverseSyntheticPosition) && (action.Reopening.Mode == TransferModeEnum.SyntheticPosition))
                                {
                                    tradeClosing = (from source in pSource
                                                    where source.overrideClosingSpecified
                                                    select source.overrideClosing).FirstOrDefault();
                                    tradeReopening = (from source in pSource
                                                      where source.overrideReopeningSpecified
                                                      select source.overrideReopening).FirstOrDefault();
                                    if ((null != tradeClosing) && (null != tradeReopening))
                                        processState = ClosingReopeningWriting(_posRequestARQ, pTradeKey, tradeClosing, tradeReopening);
                                }
                                #endregion Fermeture et réouverture de position
                                break;
                        }
                    }
                    else
                    {
                        // Pb Chargement PRODUCT/INSTRUMENT
                        processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.FAILUREWARNING);
                        

                        
                        Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 8860), (pTradeKey.IsTimingEndOfDay ? 2 : 4),
                            new LogParam(pTradeKey.IdP),
                            new LogParam(pTradeKey.IdI)));
                    }
                }
            }
            catch (Exception)
            {
                processStateMain = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                throw; 
            }
            finally
            {
                pTradeKey.statusWritingSpecified = true;
                pTradeKey.statusWriting = processStateMain;
                UpdatePosRequestGroupLevel(_posRequestARQ, processStateMain.Status);
            }
            return processState;
        }
        // EG 20190613 [24683] New
        // EG 20210415 [24683] Ajout Paramètre pPosRequest à RecordClosingReopeningTrade2 (Alimentation TRLINKPOSREQUEST)
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20240520 [WI930] Upd gestion du Timing pour recalcul des CashFLows
        private ProcessState ClosingWriting<T>(IPosRequest pPosRequest, ARQTools.TradeKey pTradeKey, List<T> pLstTradeClosing, User pUser)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS); 

            List<ARQTools.OverrideTradeCandidate> lstTradeClosing = pLstTradeClosing as List<ARQTools.OverrideTradeCandidate>;

            string nbTrades = (from source in lstTradeClosing select source.linkedTrades.Count).Sum().ToString();

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel trades writing {0} - {1} - Nb trades {2}", pPosRequest.RequestType.ToString(), pTradeKey.DisplayInfo, nbTrades),
                (pTradeKey.IsTimingEndOfDay ? 3 : 4)));
            Logger.Write();

            IDbTransaction dbTransaction = null;
            bool isException = false;
            lstTradeClosing.ForEach(tradeClosing =>
            {
                try
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    // Génération du trade Clôturant
                    RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, pPosRequest, pTradeKey, tradeClosing);
                    DataHelper.CommitTran(dbTransaction);

                    dbTransaction = DataHelper.BeginTran(CS);
                    // Génération des événements sur Clôturant
                    EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, tradeClosing,
                        tradeClosing.idTClosing, tradeClosing.identifierClosing, pTradeKey.sqlProduct.GroupProduct);
                    DataHelper.CommitTran(dbTransaction);

                    // Offsetting des trades clôturés (OCP)
                    dbTransaction = DataHelper.BeginTran(CS);
                    processState = OffSettingClosing(dbTransaction, pPosRequest, pTradeKey, tradeClosing);
                    DataHelper.CommitTran(dbTransaction);

                    if (pTradeKey.IsTimingEndOfDay || pTradeKey.IsTimingEndOfDayPlusStartOfDay)
                    {
                        tradeClosing.linkedTrades.ForEach(linkedTrade =>
                        {
                            // Recalcul des cash-flows sur les lignes clôturées
                            dbTransaction = DataHelper.BeginTran(CS);
                            // EG 20240115 [WI808] Usage de TradeCandidate.TradeInfo
                            ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, linkedTrade.TradeInfo);
                            DataHelper.CommitTran(dbTransaction);
                        });
                        // EG 20231214 [WI725] Closing/Reopening : Add Calcul du VMG (restitution) On écrase la date effective par la date business en cours pour gérer la perpétualité des instructions 
                        dbTransaction = DataHelper.BeginTran(CS);
                        ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, tradeClosing.ClosingTradeInfo);
                        DataHelper.CommitTran(dbTransaction);
                    }
                }
                catch (Exception)
                {
                    isException = true;
                    processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                    throw;
                }
                finally
                {
                    if ((null != dbTransaction) && (null != dbTransaction.Connection))
                    {
                        if (isException)
                            DataHelper.RollbackTran(dbTransaction);
                        dbTransaction.Dispose();
                    }
                }

            });

            // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
            // Désactivation des contrats et assets clôturés (Delisting)
            if (pTradeKey.actionReference.Closing.isDelistingSpecified && pTradeKey.actionReference.Closing.isDelisting)  
            {
                try
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    // Génération du trade Clôturant
                    DelistingContract(dbTransaction, pTradeKey, pUser);
                    DataHelper.CommitTran(dbTransaction);
                }

                catch (Exception)
                {
                    isException = true;
                    processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                    throw;
                }
                finally
                {
                    if ((null != dbTransaction) && (null != dbTransaction.Connection))
                    {
                        if (isException)
                            DataHelper.RollbackTran(dbTransaction);
                        dbTransaction.Dispose();
                    }
                }
            }
            return processState;
        }
        // EG 20190613 [24683] New
        // EG 20210415 [24683] Ajout Paramètre pPosRequest à RecordClosingReopeningTrade2 (Alimentation TRLINKPOSREQUEST)
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20240520 [WI930] Upd gestion du Timing pour recalcul des CashFLows
        private ProcessState ClosingReopeningWriting<T>(IPosRequest pPosRequest, ARQTools.TradeKey pTradeKey, List<T> pLstTradeClosing, T pTradeReopening)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                List<ARQTools.OverrideTradeCandidate> lstTradeClosing = pLstTradeClosing as List<ARQTools.OverrideTradeCandidate>;
                ARQTools.OverrideTradeCandidate tradeReopening = pTradeReopening as ARQTools.OverrideTradeCandidate;

                string nbTrades = (from source in lstTradeClosing select source.linkedTrades.Count).Sum().ToString();

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel trades writing {0} - {1} - Nb trades {2}", pPosRequest.RequestType.ToString(), pTradeKey.DisplayInfo, nbTrades),
                    (pTradeKey.IsTimingEndOfDay ? 3 : 5)));
                Logger.Write();

                dbTransaction = DataHelper.BeginTran(CS);
                // Fermeture des trades (CLOSING)
                lstTradeClosing.ForEach(item => RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, pPosRequest, pTradeKey, item));
                // Ouverture du trade (REOPENING)
                RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingReopeningPosition, pPosRequest, pTradeKey, tradeReopening);
                DataHelper.CommitTran(dbTransaction);

                // Génération des événements sur Clôturants et Réouvrante
                lstTradeClosing.ForEach(item =>
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, item, item.idTClosing, item.identifierClosing, pTradeKey.sqlProduct.GroupProduct);
                    DataHelper.CommitTran(dbTransaction);
                });

                dbTransaction = DataHelper.BeginTran(CS);
                EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingReopeningPosition, tradeReopening,
                    tradeReopening.idTReopening, tradeReopening.identifierReopening, pTradeKey.sqlProduct.GroupProduct);
                DataHelper.CommitTran(dbTransaction);

                // Offsetting des trades clôturés (OCP)
                lstTradeClosing.ForEach(item =>
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    OffSettingClosing(dbTransaction, pPosRequest, pTradeKey, item);
                    DataHelper.CommitTran(dbTransaction);

                    if (pTradeKey.IsTimingEndOfDay || pTradeKey.IsTimingEndOfDayPlusStartOfDay)
                    {
                        item.linkedTrades.ForEach(linkedTrade =>
                        {
                            // Recalcul des cash-flows sur les lignes clôturées
                            dbTransaction = DataHelper.BeginTran(CS);
                            // EG 20240115 [WI808] Usage de TradeCandidate.TradeInfo
                            ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, linkedTrade.TradeInfo);
                            DataHelper.CommitTran(dbTransaction);
                        });
                    }
                });

                if (pTradeKey.IsTimingEndOfDay)
                {
                    // Calcul des Cash-Flows sur la Réouvrante si traitement EOD
                    dbTransaction = DataHelper.BeginTran(CS);
                    // EG 20240115 [WI808] Usage de OverrideTradeCandidate.TradeInfo
                    ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, tradeReopening.ReopeningTradeInfo);
                    DataHelper.CommitTran(dbTransaction);
                }
            }
            catch (Exception)
            {
                isException = true;
                processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null != dbTransaction.Connection))
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }
            return processState;
        }
        // EG 20190613 [24683] New
        // EG 20210415 [24683] Ajout Paramètre pPosRequest à RecordClosingReopeningTrade2 (Alimentation TRLINKPOSREQUEST)
        // EG 20240520 [WI930] Upd gestion du Timing pour recalcul des CashFLows
        private ProcessState ClosingReopeningWriting<T>(IPosRequest pPosRequest, ARQTools.TradeKey pTradeKey, T pTradeClosing, List<T> pLstTradeReopening)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                ARQTools.OverrideTradeCandidate tradeClosing = pTradeClosing as ARQTools.OverrideTradeCandidate;
                List<ARQTools.OverrideTradeCandidate> lstTradeReopening = pLstTradeReopening as List<ARQTools.OverrideTradeCandidate>;

                string nbTrades = tradeClosing.linkedTrades.Count.ToString();

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel trades writing {0} - {1} - Nb trades {2}", pPosRequest.RequestType.ToString(), pTradeKey.DisplayInfo, nbTrades),
                    (pTradeKey.IsTimingEndOfDay ? 3 : 5)));
                Logger.Write();

                dbTransaction = DataHelper.BeginTran(CS);
                // Fermeture des trades (CLOSING)
                RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, pPosRequest, pTradeKey, tradeClosing);
                // Ouverture des trades (REOPENING)
                lstTradeReopening.ForEach(item =>
                {
                    RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingReopeningPosition, pPosRequest, pTradeKey, item);
                });
                DataHelper.CommitTran(dbTransaction);

                dbTransaction = DataHelper.BeginTran(CS);
                // Génération des événements sur Clôturant
                EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, tradeClosing, tradeClosing.idTClosing, tradeClosing.identifierClosing, pTradeKey.sqlProduct.GroupProduct);
                DataHelper.CommitTran(dbTransaction);
                // Génération des événements sur Réouvrantes
                lstTradeReopening.ForEach(item =>
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingReopeningPosition, item, item.idTReopening, item.identifierReopening, pTradeKey.sqlProduct.GroupProduct);
                    DataHelper.CommitTran(dbTransaction);
                });

                // Offsetting des trades clôturés (OCP) si traitement EOD
                dbTransaction = DataHelper.BeginTran(CS);
                OffSettingClosing(dbTransaction, pPosRequest, pTradeKey, tradeClosing);
                DataHelper.CommitTran(dbTransaction);

                if (pTradeKey.IsTimingEndOfDay || pTradeKey.IsTimingEndOfDayPlusStartOfDay)
                {
                    tradeClosing.linkedTrades.ForEach(linkedTrade =>
                    {
                        // Recalcul des cash-flows sur les lignes clôturées
                        dbTransaction = DataHelper.BeginTran(CS);
                        // EG 20240115 [WI808] Usage de TradeCandidate.TradeInfo
                        ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, linkedTrade.TradeInfo);
                        DataHelper.CommitTran(dbTransaction);
                    });
                }
                if (pTradeKey.IsTimingEndOfDay)
                {
                    // Calcul des Cash-Flows sur la Réouvrante si traitement EOD
                    dbTransaction = DataHelper.BeginTran(CS);
                    // EG 20240115 [WI808] Usage de OverrideTradeCandidate.TradeInfo
                    lstTradeReopening.ForEach(item => ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, item.ReopeningTradeInfo));
                    DataHelper.CommitTran(dbTransaction);
                }

            }
            catch (Exception)
            {
                isException = true;
                processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null != dbTransaction.Connection))
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }
            return processState;
        }
        // EG 20190613 [24683] New
        // EG 20210415 [24683] Ajout Paramètre pPosRequest à RecordClosingReopeningTrade2 (Alimentation TRLINKPOSREQUEST)
        // EG 20240520 [WI930] Upd gestion du Timing pour recalcul des CashFLows
        private ProcessState ClosingReopeningWriting<T>(IPosRequest pPosRequest, ARQTools.TradeKey pTradeKey, T pTradeClosing, T pTradeReopening)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            IDbTransaction dbTransaction = null;
            bool isException = false;
            try
            {
                ARQTools.OverrideTradeCandidate tradeClosing = pTradeClosing as ARQTools.OverrideTradeCandidate;
                ARQTools.OverrideTradeCandidate tradeReopening = pTradeReopening as ARQTools.OverrideTradeCandidate;

                dbTransaction = DataHelper.BeginTran(CS);
                // Génération du trade Clôturant
                RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, pPosRequest, pTradeKey, tradeClosing);
                // Génération du trade réouvrant
                RecordClosingReopeningTrade2(dbTransaction, Cst.PosRequestTypeEnum.ClosingReopeningPosition, pPosRequest, pTradeKey, tradeReopening);
                DataHelper.CommitTran(dbTransaction);

                dbTransaction = DataHelper.BeginTran(CS);
                // Génération des événements sur Clôturante
                processState = EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingPosition, tradeClosing,
                    tradeClosing.idTClosing, tradeClosing.identifierClosing, pTradeKey.sqlProduct.GroupProduct);
                DataHelper.CommitTran(dbTransaction);

                dbTransaction = DataHelper.BeginTran(CS);
                // Génération des événements sur Réouvrante
                processState = EventsGenCalculation(dbTransaction, Cst.PosRequestTypeEnum.ClosingReopeningPosition, tradeReopening,
                    tradeReopening.idTReopening, tradeReopening.identifierReopening, pTradeKey.sqlProduct.GroupProduct);
                DataHelper.CommitTran(dbTransaction);

                // Offsetting des trades clôturés (OCP)
                dbTransaction = DataHelper.BeginTran(CS);
                processState = OffSettingClosing(dbTransaction, pPosRequest, pTradeKey, tradeClosing);
                DataHelper.CommitTran(dbTransaction);

                // Cash-Flows 
                if (pTradeKey.IsTimingEndOfDay || pTradeKey.IsTimingEndOfDayPlusStartOfDay)
                {
                    tradeClosing.linkedTrades.ForEach(linkedTrade =>
                    {
                        // Recalcul des cash-flows sur les lignes clôturées
                        dbTransaction = DataHelper.BeginTran(CS);
                        // EG 20240115 [WI808] Usage de TradeCandidate.TradeInfo
                        ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, linkedTrade.TradeInfo);
                        DataHelper.CommitTran(dbTransaction);
                    });
                }
                if (pTradeKey.IsTimingEndOfDay)
                {
                    // Calcul des Cash-Flows sur la Réouvrante si traitement EOD
                    dbTransaction = DataHelper.BeginTran(CS);
                    // EG 20240115 [WI808] Usage de OverrideTradeCandidate.TradeInfo
                    processState = ClosingReopeningValuationAmountGen(dbTransaction, pTradeKey, tradeReopening.ReopeningTradeInfo);
                    DataHelper.CommitTran(dbTransaction);
                }
            }
            catch (Exception)
            {
                isException = true;
                processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                throw;
            }
            finally
            {
                if ((null != dbTransaction) && (null != dbTransaction.Connection))
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);
                    dbTransaction.Dispose();
                }
            }

            return processState;
        }
        // EG 20190613 [24683] New
        private ProcessState ClosingReopeningWriting<T>(IPosRequest pPosRequest, ARQTools.TradeKey pTradeKey, List<Pair<T, T>> pLstSource)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            try
            {
                string nbTrades = (from source in pLstSource as List<Pair<ARQTools.OverrideTradeCandidate,ARQTools.OverrideTradeCandidate>> 
                                   select source.First.linkedTrades.Count).Sum().ToString();
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel trades writing {0} - {1} - Nb trades {2}", pPosRequest.RequestType.ToString(), pTradeKey.DisplayInfo, nbTrades),
                    (pTradeKey.IsTimingEndOfDay ? 3 : 5)));
                Logger.Write();

                pLstSource.ForEach(item=> ClosingReopeningWriting(pPosRequest, pTradeKey, item.First, item.Second));
                pTradeKey.statusWritingSpecified = true;
                pTradeKey.statusWriting = processState;
            }
            catch (Exception)
            {
                processState = new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                throw;
            }
            finally
            {
                pTradeKey.statusWritingSpecified = true;
                pTradeKey.statusWriting = processState;
            }
            return processState;
        }
        #endregion ClosingReopeningWriting

        /// <summary>
        /// Delisting des contrats 
        /// => Closing/Reopening qui équivaut à une corporate action de type Delisting
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPosRequest"></param>
        /// <param name="pTradeKey"></param>
        /// <param name="pUser"></param>
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        private void DelistingContract(IDbTransaction pDbTransaction, ARQTools.TradeKey pTradeKey, User pUser)
        {
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(CS);
            int idContract = pTradeKey.IdDC;
            string tblContract = "DERIVATIVECONTRACT";
            string tblAsset = "ASSET_ETD";
            string columnContract = "IDDC";
            if (pTradeKey.IdCCSpecified)
            {
                idContract = pTradeKey.IdCC;
                tblContract = "COMMODITYCONTRACT";
                tblAsset = "ASSET_COMMODITY";
                columnContract = "IDCC";
            }

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "DTEFFECTIVE", DbType.Date), pTradeKey.actionReference.effectiveDate);
            parameters.Add(new DataParameter(CS, "DTUPD", DbType.Date), dtSys);
            parameters.Add(new DataParameter(CS, "IDA", DbType.Int32), pUser.IdA);
            parameters.Add(new DataParameter(CS, "IDCONTRACT", DbType.Int32), idContract);

            string sqlUpdate = $"UPDATE dbo.{tblContract} set DTDISABLED=@DTEFFECTIVE, DTUPD=@DTUPD, IDAUPD=@IDA where ({columnContract} = @IDCONTRACT) and (DTDISABLED is null)";
            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());


            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pTradeKey.IdAsset);
            if (pTradeKey.IdDCSpecified)
            {
                sqlUpdate = $"UPDATE dbo.DERIVATIVEATTRIB set DTDISABLED=@DTEFFECTIVE, DTUPD=@DTUPD, IDAUPD=@IDA where ({columnContract} = @IDCONTRACT) and (IDASSET = @IDASSET) and (DTDISABLED is null)";
                qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
                DataHelper.ExecuteNonQuery(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }

            parameters.Remove("IDCONTRACT");
            sqlUpdate = $"UPDATE dbo.{tblAsset} set DTDISABLED=@DTEFFECTIVE, DTUPD=@DTUPD, IDAUPD=@IDA where (IDASSET = @IDASSET) and (DTDISABLED is null)";
            qryParameters = new QueryParameters(CS, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(CS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #region RecordClosingReopeningTrade
        /// <summary>
        /// Construction du trade matérialisant la fermeture|réouverture de position
        /// Via pOverrideTrade (IdTSource) -> Construction du trade de Fermeture|Réouverture (SetDataDocumentClosing|SetDataDocumentReopening)
        /// - Mise à jour prix, quantité, acteurs, 
        /// - Calcul des frais
        /// - Ecriture du trade
        /// - Génération des événements
        /// - Calcul des cash-flows (sur la réouverture)
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pPosRequestType">Type d'action (ClosingPosition|ClosingReopeningPosition</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pOverrideTrade">Caractéristiques pour construction du trade matérialisant la fermeture ou la réouverture de position</param>
        /// <param name="pUser">Utilisateur</param>
        // EG 20190613 [24683] New
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20210415 [24683] Ajout Paramètre pPosRequest à RecordClosingReopeningTrade2 (Alimentation TRLINKPOSREQUEST dans RecordTrade)
        // EG 20211029 [25696] Transfert de position : Ajout préfixe sur EXTLLINK
        private ProcessState RecordClosingReopeningTrade2(IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pPosRequestType, IPosRequest pPosRequest,
            ARQTools.TradeKey pTradeKey, ARQTools.OverrideTradeCandidate pOverrideTrade)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);

            int idT = 0;
            string identifier = string.Empty;
            ClosingReopeningAction action = pTradeKey.actionReference;

            bool isOk = false;

            PosRequestTradeAdditionalInfo additionalInfo = new PosRequestTradeAdditionalInfo();
            List<Pair<int, string>> tradeLinkInfo = null;
            DataDocumentContainer dataDocContainer = null;

            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CS, pOverrideTrade.idTSource)
            {
                IsWithTradeXML = true,
                DbTransaction = pDbTransaction,
                IsAddRowVersion = true
            };
            // RD 20210304 Add "trx."            
            // EG 20211029 [25696] Transfert de position : Ajout préfixe sur EXTLLINK
            isOk = (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "IDI", "IDENTIFIER", "trx.TRADEXML", "DTTRADE", "DTBUSINESS", "DTEXECUTION", "DTORDERENTERED", "DTTIMESTAMP", "TZFACILITY", "EXTLLINK"}));

            if (isOk)
            {
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(sqlTrade.TradeXml);
                IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);
                dataDocContainer = (DataDocumentContainer)new DataDocumentContainer(dataDoc).Clone();

                SQL_LastTrade_L sqlTrail = new SQL_LastTrade_L(CS, sqlTrade.IdT, null)
                {
                    DbTransaction = pDbTransaction
                };
                sqlTrail.LoadTable(new string[] { "SCREENNAME" });

                additionalInfo.templateDataDocument = dataDocContainer;
                additionalInfo.sqlTemplateTrade = sqlTrade;
                additionalInfo.stActivation = Cst.StatusActivation.REGULAR;
                additionalInfo.screenName = sqlTrail.ScreenName;

                additionalInfo.sqlInstrument = pTradeKey.sqlInstrument;
                additionalInfo.sqlProduct = pTradeKey.sqlProduct;

                if (StrFunc.IsFilled(sqlTrade.ExtlLink))
                {
                    // EG 20211029 [25696] Transfert de position : Ajout préfixe sur EXTLLINK
                    string prefix = "[" + ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequestType) + "]";
                    additionalInfo.extlLink = prefix + sqlTrade.ExtlLink.Replace(prefix, string.Empty);
                }

                Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

                if (Cst.PosRequestTypeEnum.ClosingPosition == pPosRequestType)
                    ret = SetDataDocumentClosing(pDbTransaction, pTradeKey, pOverrideTrade, dataDocContainer, additionalInfo);
                else
                    ret = SetDataDocumentReopening(pDbTransaction, pTradeKey, pOverrideTrade, dataDocContainer, additionalInfo);

                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    // EG 20240531 [WI926] Add Parameter pIsTemplate
                    dataDocContainer.UpdateMissingTimestampAndFacility(CS, pDbTransaction, sqlTrade, false);

                    if (null != pOverrideTrade.linkedTrades)
                    {
                        tradeLinkInfo = (from item in pOverrideTrade.linkedTrades
                                         select new Pair<int, string>(item.idT, item.identifier)).ToList();
                    }
                    else
                    {
                        tradeLinkInfo = new List<Pair<int, string>>
                        {
                            new Pair<int, string>(sqlTrade.IdT, sqlTrade.Identifier)
                        };
                    }
                    // EG 20210415 [24683] ajout paramètre pPosRequest.IDPR (Alimentation TRLINKPOSREQUEST)
                    ret = RecordTrade(pDbTransaction, dataDocContainer, additionalInfo, new int[] { pPosRequest.IdPR }, pPosRequestType, tradeLinkInfo, out idT, out identifier);
                    if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        if (Cst.PosRequestTypeEnum.ClosingPosition == pPosRequestType)
                        {
                            pOverrideTrade.idTClosingSpecified = true;
                            pOverrideTrade.idTClosing = idT;
                            pOverrideTrade.identifierClosingSpecified = true;
                            pOverrideTrade.identifierClosing = identifier;
                        }
                        else
                        {
                            pOverrideTrade.idTReopeningSpecified = true;
                            pOverrideTrade.idTReopening = idT;
                            pOverrideTrade.identifierReopeningSpecified = true;
                            pOverrideTrade.identifierReopening = identifier;
                        }
                    }
                    else
                    {
                        processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, ret);
                    }
                }
                else
                {
                    processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, ret);
                }
            }
            else
            {
                processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.FAILUREWARNING);

                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 8865), (pTradeKey.IsTimingEndOfDay ? 2 : 4),
                    new LogParam(LogTools.IdentifierAndId(pOverrideTrade.identifierSource, pOverrideTrade.idTSource))));
            }
            return processState;
        }
        #endregion RecordClosingReopeningTrade

        #region EventsGenCalculation
        /// <summary>
        /// Génération des événement sur le trade de fermeture et réouverture
        /// - par SlaveCall
        /// - en mode transactionnel
        /// Sur la fermeture de position (IdEParent_ForOffsettingPosition retourne l'IDE parent sur lequel seront raccrochés les événement d'offsetting (OCP)
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pPosRequestType">Type d'action (ClosingPosition|ClosingReopeningPosition</param>
        /// <param name="pOverrideTrade">Caractéristiques pour construction du trade matérialisant la fermeture ou la réouverture de position</param>
        /// <param name="pIdT">IdT de la fermeture|réouverture</param>
        /// <param name="pIdentifier">Identifiant de la fermeture|réouverture</param>
        /// <param name="pGProduct">Groupe de produit</param>
        /// <returns></returns>
        // EG 20190613 [24683] New
        private ProcessState EventsGenCalculation(IDbTransaction pDbTransaction,
            Cst.PosRequestTypeEnum pPosRequestType, ARQTools.OverrideTradeCandidate pOverrideTrade, int pIdT, string pIdentifier, ProductTools.GroupProductEnum pGProduct)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusUnknownEnum, Cst.ErrLevel.SUCCESS);
            try
            {
                if (0 < pIdT)
                {
                    AppInstance.TraceManager.TraceVerbose(ProcessBase, String.Format("START EventsGenCalculation {0}({1})", pIdentifier, pIdT));

                    EventsGenMQueue _queue = New_EventsGenAPI.CreateEventsGenMQueue(ProcessBase,  pIdT, pIdentifier, pGProduct);
                    processState = New_EventsGenAPI.ExecuteSlaveCall(_queue, pDbTransaction, ProcessBase, false);
                    if (pPosRequestType == Cst.PosRequestTypeEnum.ClosingPosition)
                    {
                        pOverrideTrade.idEParent_ForOffsettingPositionSpecified = true;
                        pOverrideTrade.idEParent_ForOffsettingPosition = _queue.IdEParent_ForOffsettingPosition;
                    }
                    AppInstance.TraceManager.TraceVerbose(ProcessBase, String.Format("FINISHED EventsGenCalculation {0}({1})", pIdentifier, pIdT));
                }
            }
            catch (Exception ex)
            {
                SpheresException2 otcmlEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);
                
                // FI 20200623 [XXXXX] AddCriticalException
                ProcessBase.ProcessState.AddCriticalException(otcmlEx);
                
                
                
                Logger.Log(new LoggerData(otcmlEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 2));
                Logger.Write();

                ProcessBase.ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
            }
            return processState;
        }
        #endregion EventsGenCalculation


        #region OffSettingClosing
        /// <summary>
        /// Génération des événements de fermeture de position (EVENTCODE = OCP)
        /// pour une trade fermeture (à ce trade est associé une liste de 1 à n trade(s) source(s))
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pPosRequest">PosRequest attachée</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pTradeClosing">Trade de fermeture</param>
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20231214 [WI725] Closing/Reopening : Ajout Commentaires
        // EG 20240520 [WI930] Upd m_ClosingReopeningDtBusiness (avec tuple)
        private ProcessState OffSettingClosing(IDbTransaction pDbTransaction, IPosRequest pPosRequest, ARQTools.TradeKey pTradeKey, ARQTools.OverrideTradeCandidate pTradeClosing)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            // EG 20231211 [WI701] Correction Date pour clôture
            //DateTime _dtClosing =  pTradeKey.actionReference.effectiveDate;
            DateTime _dtClosing = m_ClosingReopeningDtBusiness.dtClosing;
            bool multiTrades = (1 < pTradeClosing.linkedTrades.Count);
            // isSumRealizedMarginForClosingTrade Lecture du paramètre dans le contexte attaché à la fermeture
            // Si true  => 1 RMG GLOBAL pour l'ensemble des couples Trade closed,Trade closing)
            // Sinon    => 1 RMG pour chaque couple Trade closed,Trade closing)
            bool isSumRealizedMarginForClosingTrade = pTradeKey.actionReference.Closing.isSumClosingAmountSpecified &&
                                                      pTradeKey.actionReference.Closing.isSumClosingAmount &&
                                                      multiTrades;

            #region GetId of POSACTION/POSACTIONDET
            SQLUP.GetId(out int newIdPA, pDbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
            int newIdPADET = newIdPA;
            #endregion GetId of POSACTION/POSACTIONDET

            #region Insertion dans POSACTION
            InsertPosAction(pDbTransaction, newIdPA, _dtClosing, pPosRequest.IdPR, pTradeKey.DtOut);
            #endregion Insertion dans POSACTION

            #region Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS
            // [(OCP + NOM + QTY + RMG) * Nb Trades clôturés]  + [(OCP + NOM + QTY) * Trade clôturant] + [(RMG) * Nb Trades clôturés * Trade clôturant]
            int nbEvent = (4 * pTradeClosing.linkedTrades.Count) + 3 + (isSumRealizedMarginForClosingTrade ? 1 : pTradeClosing.linkedTrades.Count);
            SQLUP.GetId(out int newIdE, pDbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbEvent);

            #region EVENT OCP (OffSetting Closing Position)
            m_EventQuery.InsertEvent(pDbTransaction, pTradeClosing.idTClosing, newIdE, pTradeClosing.idEParent_ForOffsettingPosition, null, 1, 1, null, null, null, null,
                    EventCodeFunc.OffSettingClosingPosition, EventTypeFunc.Total,
                    _dtClosing, _dtClosing, _dtClosing, _dtClosing, pTradeClosing.qty, null, UnitTypeEnum.Qty.ToString(), null, null);
            m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.GroupLevel, _dtClosing, false);

            //PosActionDet
            EventQuery.InsertEventPosActionDet(pDbTransaction, newIdPADET, newIdE);

            //idE_Event Contient L'evènement OCP
            int idE_EventForClosing = newIdE;
            newIdE++;
            InsertNominalQuantityEvent(pDbTransaction, pTradeKey.posKeepingData, pTradeClosing.idTClosing, ref newIdE, idE_EventForClosing, _dtClosing, IsTradeBuyer(pTradeClosing.side), pTradeClosing.qty, 0, newIdPADET);
            #endregion EVENT OCP (OffSetting Closing Position)

            if (isSumRealizedMarginForClosingTrade)
            {
                newIdE++;
                AgreggatePayerReceiverAmountInfo realizedMarginInfo = AggregateRealizedMargin(pTradeKey.posKeepingData, pTradeClosing);
                InsertClosingPositionsRealizedMarginEvent(pDbTransaction, pTradeKey.posKeepingData, pTradeKey.actionReference.effectiveDate,
                realizedMarginInfo.IdA_Payer, realizedMarginInfo.IdB_Payer, realizedMarginInfo.IdA_Receiver, realizedMarginInfo.IdB_Receiver,
                realizedMarginInfo.Amount,
                pTradeClosing.qty, null, pTradeClosing.price, pTradeClosing.idTClosing, newIdE, newIdPADET, idE_EventForClosing);
            }


            // Listes des trades clôturés
            newIdE++;
            pTradeClosing.linkedTrades.ForEach(item =>
            {
                bool isDealerBuyer = IsTradeBuyer(item.side);
                #region POSACTIONDET
                InsertPosActionDet(pDbTransaction, newIdPA, newIdPADET,
                    isDealerBuyer ? item.idT : pTradeClosing.idTClosing,
                    isDealerBuyer ? pTradeClosing.idTClosing : item.idT,
                    pTradeClosing.idTClosing, item.qty, null);
                #endregion POSACTIONDET

                #region EVENT OCP (OffSetting Closed Position)
                m_EventQuery.InsertEvent(pDbTransaction, item.idT, newIdE, item.idE_Event, null, 1, 1, null, null, null, null,
                        EventCodeFunc.OffSettingClosingPosition, EventTypeFunc.Total,
                        DtBusiness, DtBusiness, DtBusiness, DtBusiness, item.qty, null, UnitTypeEnum.Qty.ToString(), null, null);
                m_EventQuery.InsertEventClass(pDbTransaction, newIdE, EventClassFunc.GroupLevel, DtBusiness, false);

                //PosActionDet
                EventQuery.InsertEventPosActionDet(pDbTransaction, newIdPADET, newIdE);
                #endregion EVENT OCP (OffSetting Closed Position)

                //idE_Event Contient L'evènement OCP
                int idE_EventForClosed = newIdE;
                #region EVENT NOM/QTY (Nominal/Quantité)
                newIdE++;
                InsertNominalQuantityEvent(pDbTransaction, pTradeKey.posKeepingData, item.idT, ref newIdE, idE_EventForClosed, DtBusiness, isDealerBuyer, item.qty, 0, newIdPADET);
                #endregion EVENT NOM/QTY (Nominal/Quantité)

                #region EVENT RMG
                // Pour chaque trade clôturé
                InsertClosingPositionsRealizedMargin(pDbTransaction, pTradeKey.posKeepingData, _dtClosing, newIdPADET, item, idE_EventForClosed, pTradeClosing, idE_EventForClosing,
                    (false == isSumRealizedMarginForClosingTrade), ref newIdE);
                #endregion EVENT RMG

                if (multiTrades && (pTradeClosing.linkedTrades.Last().idT != item.idT))
                {
                    newIdE++;
                    SQLUP.GetId(out newIdPADET, pDbTransaction, SQLUP.IdGetId.POSACTION, SQLUP.PosRetGetId.First, 1);
                }
            });

            #endregion Insertion dans POSACTIONDET/EVENTPOSACTIONDET/EVENTS

            return processState;
        }
        #endregion OffSettingClosing
        #region ClosingReopeningValuationAmountGen
        /// <summary>
        /// Calcul des cash-flows pour le trade de réouverture
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pIdT">Idt du trade de réouverture</param>
        /// <param name="pIdentifier">Indentifiant de réouverture</param>
        // EG 20190613 [24683] New
        private ProcessState ClosingReopeningValuationAmountGen(IDbTransaction pDbTransaction, ARQTools.TradeKey pTradeKey, (int id, string identifier) pTrade)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            // EG 20240115 [WI808] Usage de TradeKey.AssetInfo.TradeInfo
            Cst.ErrLevel ret = ValuationAmountGen(pDbTransaction, pTrade, pTradeKey.AssetInfo, true);
            if (Cst.ErrLevel.SUCCESS != ret)
                processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, ret);

            return processState;
        }
        #endregion ClosingReopeningValuationAmountGen
        #region CreatePosKeepingData
        /// <summary>
        /// Alimentation de PosKeepingData (utilisée dans des méthodes communes du traitement EOD)
        /// - Clé de position
        /// - Asset lecture ou alimentation du cache
        /// </summary>
        /// <param name="pTradeKey">Clé de poistion</param>
        // EG 20190613 [24683] New
        private IPosKeepingData CreatePosKeepingData(ARQTools.TradeKey pTradeKey)
        {
            IPosKeepingData _posKeepingData = m_Product.CreatePosKeepingData();
            _posKeepingData.IdI = pTradeKey.IdI;
            _posKeepingData.UnderlyingAsset = pTradeKey.AssetCategory;
            _posKeepingData.IdAsset = pTradeKey.IdAsset;
            _posKeepingData.IdA_Dealer = pTradeKey.IdA_Dealer;
            _posKeepingData.IdB_Dealer = pTradeKey.IdB_Dealer;
            _posKeepingData.IdA_Clearer = pTradeKey.IdA_Clearer;
            _posKeepingData.IdB_Clearer = pTradeKey.IdB_Clearer;
            _posKeepingData.Asset = GetAsset(null, pTradeKey.AssetCategory, pTradeKey.IdAsset, Cst.PosRequestAssetQuoteEnum.Asset);
            _posKeepingData.Product = pTradeKey.sqlProduct;
            return _posKeepingData;
        }
        #endregion CreatePosKeepingData

        #region SetDataDocumentClosingReopening
        /// <summary>
        /// Alimentation et Modification du DataDocument d'un trade de fermeture ou réouverture
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pPositionEffect">Open|Close</param>
        /// <param name="pRptSide">RptSide</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pOverrideTrade">Caractéristiques pour modification du trade matérialisant la fermeture ou la réouverture de position</param>
        /// <param name="pDataDocument">DataDocument associé</param>
        /// <param name="pUser">Utilisateur</param>
        // EG 20190613 [24683] New
        // EG 202208016 [XXXXX] Modification TRDTYPE de réouverture sur ClosingReopeningPosition (Réouverture = PositionOpening)
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20240520 [WI930] Upd m_ClosingReopeningDtBusiness (avec tuple)
        private Cst.ErrLevel SetDataDocumentClosingReopening(IDbTransaction pDbTransaction, PositionEffectEnum pPositionEffect, RptSideProductContainer pRptSide,
            ARQTools.OverrideTradeCandidate pOverrideTrade, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            //DateTime dtBusiness = pTradeKey.IsTimingEndOfDay ? m_EntityMarketInfo.dtMarket : m_EntityMarketInfo.dtMarketNext;
            // EG 20231211 [WI701] Correction Date pour clôture
            //DateTime dtBusiness = pTradeKey.actionReference.effectiveDate;
            DateTime dtBusiness = (pPositionEffect == PositionEffectEnum.Close ? m_ClosingReopeningDtBusiness.dtClosing : m_ClosingReopeningDtBusiness.dtReopening);

            if (null != pRptSide)
            {
                SideEnum side = (pRptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Buy : SideEnum.Sell);
                IFixTrdCapRptSideGrp rptSideGrp = pRptSide.GetTrdCapRptSideGrp(side);
                if (null != rptSideGrp)
                {
                    rptSideGrp.Text = (pPositionEffect == PositionEffectEnum.Close?"Closing":"Reopening") + " position";
                    rptSideGrp.TextSpecified = true;
                }
            }
            ExchangeTradedContainer _exchangeTradedContainer;
            if (pDataDocument.CurrentProduct.IsExchangeTradedDerivative)
            {
                #region ExchangeTradedDerivative
                _exchangeTradedContainer = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)pDataDocument.CurrentProduct.Product);
                // EG 202208016 [XXXXX] Modification TRDTYPE de réouverture sur ClosingReopeningPosition (Réouverture = PositionOpening)
                _exchangeTradedContainer.InitClosingReopening(pOverrideTrade.qty, pOverrideTrade.price, dtBusiness,
                    pPositionEffect== PositionEffectEnum.Close?TrdTypeEnum.TechnicalTrade:TrdTypeEnum.PositionOpening);
                // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du CRP
                pDataDocument.SetClearedDate(dtBusiness);
                #endregion ExchangeTradedDerivative
            }
            else if (pDataDocument.CurrentProduct.IsEquitySecurityTransaction)
            {
                #region EquitySecurityTransaction
                IEquitySecurityTransaction _equitySecurityTransaction = (IEquitySecurityTransaction)pDataDocument.CurrentProduct.Product;
                _exchangeTradedContainer = new EquitySecurityTransactionContainer(_equitySecurityTransaction);
                // EG 202208016 [XXXXX] Modification TRDTYPE de réouverture sur ClosingReopeningPosition (Réouverture = PositionOpening)
                _exchangeTradedContainer.InitClosingReopening(pOverrideTrade.qty, pOverrideTrade.price, dtBusiness,
                    pPositionEffect == PositionEffectEnum.Close ? TrdTypeEnum.TechnicalTrade : TrdTypeEnum.PositionOpening);
                // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du CRP
                pDataDocument.SetClearedDate(dtBusiness);
                #endregion EquitySecurityTransaction
            }
            else if (pDataDocument.CurrentProduct.IsDebtSecurityTransaction)
            {
                #region DebtSecurityTransaction
                IDebtSecurityTransaction _debtSecurityTransaction = (IDebtSecurityTransaction)pDataDocument.CurrentProduct.Product;
                DebtSecurityTransactionContainer _debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(_debtSecurityTransaction, pDataDocument);
                _debtSecurityTransactionContainer.SetOrderQuantityForAction(pOverrideTrade.qty);
                // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du CRP
                pDataDocument.SetClearedDate(dtBusiness);
                // GrossAmount prorata ?
                // AccruedInterest prorata ?
                #endregion DebtSecurityTransaction
            }
            else if (pDataDocument.CurrentProduct.IsReturnSwap)
            {
                #region ReturnSwap
                ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(CS, pDbTransaction, (IReturnSwap)pDataDocument.CurrentProduct.Product, pDataDocument);
                _returnSwapContainer.SetMainOpenUnits(pOverrideTrade.qty);
                // EG 20211029 [25696] Mise à jour de ClearedDate avec la DtBusiness du CRP
                pDataDocument.SetClearedDate(dtBusiness);
                #endregion ReturnSwap

            }

            if (pOverrideTrade.reverseSide)
            {
                TradeInput tradeInput = new TradeInput
                {
                    DataDocument = pDataDocument
                };
                tradeInput.ReversePartyReference();
            }
            return codeReturn;
        }
        #endregion SetDataDocumentClosingReopening
        #region SetDataDocumentReopening
        /// <summary>
        /// Alimentation et Modification du DataDocument d'un trade de réouverture
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pOverrideTrade">Caractéristiques pour modification du trade matérialisant la réouverture de position</param>
        /// <param name="pDataDocument">DataDocument associé</param>
        /// <param name="pAdditionalInfo">Informations additionnelles (SQLProduct, SQLInstrument, etc.</param>
        // EG 20190613 [24683] New
        // EG 20200311 [24683] Upd 
        // EG 20210315 [24683] Gestion Rôle SUBOFFICE sur Dealer
        // EG 20230905 [WI701][XXXXX] Mise à jour OtherPartyPayment sur la réouverture après celle des Dealer/Clearer
        private Cst.ErrLevel SetDataDocumentReopening(IDbTransaction pDbTransaction, ARQTools.TradeKey pTradeKey, ARQTools.OverrideTradeCandidate pOverrideTrade,
            DataDocumentContainer pDataDocument, PosRequestTradeAdditionalInfo pAdditionalInfo)
        {
            RptSideProductContainer rptSide = pDataDocument.CurrentProduct.RptSide(CS, pDbTransaction, true);
            Cst.ErrLevel codeReturn = SetDataDocumentClosingReopening(pDbTransaction, PositionEffectEnum.Open, rptSide, pOverrideTrade, pDataDocument);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                ReopeningInstructions reopening = pTradeKey.actionReference.Reopening;

                if (null != rptSide)
                {
                    #region Dealer|Clearer Update
                    if (pTradeKey.exDealerSpecified)
                    {
                        IParty _dealerParty = pDataDocument.GetParty(rptSide.GetDealer().PartyId.href);

                        bool isDealerOk = ActorTools.IsActorWithRole(CS, pTradeKey.exDealer.IdA, new RoleActor[] { RoleActor.CLIENT, RoleActor.COUNTERPARTY });
                        if (false == isDealerOk)
                        {
                            KeyValuePair<int, bool> ret = ActorTools.IsActorWithRoleRegardTo(CS, pDbTransaction, pTradeKey.exDealer.IdA, new RoleActor[] { RoleActor.SUBOFFICE });
                            if (ret.Value && (ret.Key == pTradeKey.IdA_Dealer))
                                isDealerOk = ActorTools.IsActorWithRole(CS, ret.Key, new RoleActor[] { RoleActor.CLIENT, RoleActor.COUNTERPARTY });
                        }

                        if (isDealerOk)
                        {
                            SetActorTransfer(pDbTransaction, pDataDocument, _dealerParty, pTradeKey.exDealer.IdA, pTradeKey.exDealer.IdB);
                            SideEnum side = (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Buy : SideEnum.Sell);
                            rptSide.SetBuyerSeller(_dealerParty.Id, side, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                        }
                        else
                        {
                            Exception ex = new SpheresException2(MethodInfo.GetCurrentMethod().Name, (pTradeKey.IsTimingEndOfDay ? 2 : 4), "SYS-08866",
                                new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATAREJECTED),
                                    LogTools.IdentifierAndId(_dealerParty.PartyName, _dealerParty.OTCmlId));

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 8866), 0,
                                new LogParam(GetPosRequestLogValue(m_PosRequest.RequestType)),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_PosRequest.DtBusiness))));
                            throw ex;
                        }
                    }

                    // Clearer|Custodian --> Update (Traders/Sales --> Remove)
                    if (pTradeKey.exClearerSpecified)
                    {
                        IParty _clearerParty = pDataDocument.GetParty(rptSide.GetClearerCustodian().PartyId.href);
                        PartyRoleEnum role = PartyRoleEnum.ClearingOrganization;
                        if (null != _clearerParty)
                        {
                            int idA_Clearer = pTradeKey.exClearer.IdA;
                            if (rptSide.ProductBase.IsExchangeTradedDerivative || rptSide.ProductBase.IsCommoditySpot)
                            {
                                bool isCompart = true;
                                while (isCompart)
                                {
                                    if (ActorTools.IsActorWithRole(CS, idA_Clearer, RoleActor.CSS))
                                    {
                                        role = PartyRoleEnum.ClearingOrganization;
                                        isCompart = false;
                                    }
                                    else if (ActorTools.IsActorWithRole(CS, idA_Clearer, RoleActor.CLEARER))
                                    {
                                        role = PartyRoleEnum.ClearingFirm;
                                        isCompart = false;
                                    }
                                    else
                                    {
                                        KeyValuePair<int, bool> ret = ActorTools.IsActorWithRoleRegardTo(CS, pDbTransaction, idA_Clearer,
                                            new RoleActor[] { RoleActor.CCLEARINGCOMPART, RoleActor.HCLEARINGCOMPART, RoleActor.MCLEARINGCOMPART });
                                        if ((0 < ret.Key) && ret.Value)
                                        {
                                            idA_Clearer = ret.Key;
                                        }
                                        else
                                        {
                                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, (pTradeKey.IsTimingEndOfDay ? 2 : 4), "SYS-08867",
                                                new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATAREJECTED),
                                                    LogTools.IdentifierAndId(_clearerParty.PartyName, _clearerParty.OTCmlId));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                role = PartyRoleEnum.Custodian;
                            }
                            SetActorTransfer(pDbTransaction, pDataDocument, _clearerParty, idA_Clearer, pTradeKey.exClearer.IdB);
                            SideEnum side = (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Sell : SideEnum.Buy);
                            rptSide.SetBuyerSeller(_clearerParty.Id, side, role);
                        }
                    }
                    #endregion Dealer|Clearer Update

                    #region ExCssCustodian
                    // EG 20240520 [WI930] New
                    if (pTradeKey.exCssCustodianSpecified)
                    {
                        pDataDocument.ExCssCustodian = pTradeKey.exCssCustodian.IdA;
                        // EG 20240628 [OTRS1000275] Euronext Clearing: CCP non changed in the XML of the trade
                        // Suppression de la CSS (cum) dans les parties du DataDocument
                        IParty _cumCustodianParty = pDataDocument.GetParty(pTradeKey.CssCustodian_Identifier);
                        if (null != _cumCustodianParty)
                            pDataDocument.RemoveParty(_cumCustodianParty.Id);

                        // EG 20240628 [OTRS1000275] Euronext Clearing: CCP non changed in the XML of the trade
                        // Ajout de la nouvelle CSS (ex) dans les parties du DataDocument
                        IParty _exCustodianParty = pDataDocument.GetParty(pTradeKey.exCssCustodian.IdA_Identifier);
                        if (null == _exCustodianParty)
                        {
                            SQL_Actor sqlClearingHouse = new SQL_Actor(CS, pTradeKey.exCssCustodian.IdA);
                            sqlClearingHouse.DbTransaction = pDbTransaction;
                            if (sqlClearingHouse.IsLoaded)
                                pDataDocument.AddParty(sqlClearingHouse);
                        }
                    }
                    #endregion ExCssCustodian
                    #region Others Buyer|seller|Payer|receiver Update
                    // GAM ?
                    if (pDataDocument.CurrentProduct.IsDebtSecurityTransaction)
                    {
                        IDebtSecurityTransaction _debtSecurityTransaction = (IDebtSecurityTransaction)pDataDocument.CurrentProduct.Product;
                        if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                        {
                            _debtSecurityTransaction.BuyerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            _debtSecurityTransaction.SellerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                        }
                        else
                        {
                            _debtSecurityTransaction.BuyerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                            _debtSecurityTransaction.SellerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                        }
                    }
                    if (pDataDocument.CurrentProduct.IsReturnSwap)
                    {
                        ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(CS, pDbTransaction, (IReturnSwap)pDataDocument.CurrentProduct.Product, pDataDocument);
                        if (rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                        {
                            _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                            _returnSwapContainer.ReturnLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                            _returnSwapContainer.ReturnLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                        }
                        else
                        {
                            _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef = rptSide.GetDealer().PartyId.href;
                            _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef = rptSide.GetClearerCustodian().PartyId.href;
                            _returnSwapContainer.ReturnLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                            _returnSwapContainer.ReturnLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.PayerPartyReference.HRef = _returnSwapContainer.ReturnSwap.SellerPartyReference.HRef;
                            _returnSwapContainer.InterestLeg.ReceiverPartyReference.HRef = _returnSwapContainer.ReturnSwap.BuyerPartyReference.HRef;
                        }
                    }

                    #endregion Others Buyer|seller|Payer|receiver Update

                    rptSide.SynchronizeFromDataDocument();
                }

                // CALCUL DES FRAIS
                pDataDocument.SetOtherPartyPayment(null);
                if (reopening.FeeActionSpecified)
                    pDataDocument.CurrentTrade.OtherPartyPayment = CalFeesClosingReopening(pDbTransaction, pTradeKey, reopening, pOverrideTrade, pDataDocument, pAdditionalInfo);

            }
            return codeReturn;
        }
        #endregion SetDataDocumentReopening
        #region SetDataDocumentClosing
        /// <summary>
        /// Alimentation et Modification du DataDocument d'un trade de fermeture
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pOverrideTrade">Caractéristiques pour modification du trade matérialisant la fermeture de position</param>
        /// <param name="pDataDocument">DataDocument associé</param>
        /// <param name="pAdditionalInfo">Informations additionnelles (SQLProduct, SQLInstrument, etc.</param>
        // EG 20190613 [24683] New
        private Cst.ErrLevel SetDataDocumentClosing(IDbTransaction pDbTransaction, ARQTools.TradeKey pTradeKey, ARQTools.OverrideTradeCandidate pOverrideTrade, 
            DataDocumentContainer pDataDocument, PosRequestTradeAdditionalInfo pAdditionalInfo)
        {
            RptSideProductContainer rptSide = pDataDocument.CurrentProduct.RptSide(CS, pDbTransaction, true);

            Cst.ErrLevel codeReturn = SetDataDocumentClosingReopening(pDbTransaction, PositionEffectEnum.Close, rptSide, pOverrideTrade, pDataDocument);

            if(Cst.ErrLevel.SUCCESS == codeReturn)
            {
                // CALCUL DES FRAIS
                ClosingInstructions closing = pTradeKey.actionReference.Closing;
                pDataDocument.SetOtherPartyPayment(null);
                if (closing.FeeActionSpecified)
                    pDataDocument.CurrentTrade.OtherPartyPayment = CalFeesClosingReopening(pDbTransaction, pTradeKey, closing, pOverrideTrade, pDataDocument, pAdditionalInfo);
            }
            rptSide.SynchronizeFromDataDocument();
            return codeReturn;
        }
        #endregion SetDataDocumentClosing

        #region CalFeesClosingReopening
        /// <summary>
        /// Calcul éventuel des frais associés à la fermeture ou la réouverture
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="positionInstructions">Instructions de fermeture|réouverture</param>
        /// <param name="pOverrideTrade">Caractéristiques pour modification du trade matérialisant la fermeture ou la réouverture de position</param>
        /// <param name="pDataDocument">DataDocument associé</param>
        /// <param name="pAdditionalInfo">Informations additionnelles (SQLProduct, SQLInstrument, etc.</param>
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20231030 [WI725] Upd Closing/Reopening : Test Action
        private IPayment[] CalFeesClosingReopening(IDbTransaction pDbTransaction,  ARQTools.TradeKey pTradeKey,  
            PositionInstructions positionInstructions, ARQTools.OverrideTradeCandidate pOverrideTrade, DataDocumentContainer pDataDocument, PosRequestTradeAdditionalInfo pAdditionalInfo)
        {
            IPayment[] payment = null;
            //positionInstructions.feeAction
            if (positionInstructions.FeeActionSpecified && StrFunc.IsFilled(positionInstructions.FeeAction.identifier))
            {
                TradeInput tradeInput = new TradeInput
                {
                    SQLProduct = pAdditionalInfo.sqlProduct,
                    SQLTrade = pAdditionalInfo.sqlTemplateTrade,
                    DataDocument = pDataDocument
                };
                tradeInput.TradeStatus.Initialize(ProcessBase.Cs, pDbTransaction, pAdditionalInfo.sqlTemplateTrade.IdT);
                tradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;
                tradeInput.CurrentTrade.OtherPartyPayment = null;

                IExchangeTradedDerivative etd = (IExchangeTradedDerivative)tradeInput.CurrentTrade.Product;
                etd.TradeCaptureReport.ClearingBusinessDate = new EFS_Date
                {
                    DateValue = pTradeKey.IsTimingEndOfDay ? m_EntityMarketInfo.DtMarket : m_EntityMarketInfo.DtMarketNext
                };

                FeeRequest feeRequest = new FeeRequest(CSTools.SetCacheOn(CS), pDbTransaction, tradeInput, positionInstructions.FeeAction.identifier, Cst.Capture.ModeEnum.Update,
                    new string[] { Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity),
                               Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.QuantityContractMultiplier) },
                        pOverrideTrade.qty, "");
                FeeProcessing fees = new FeeProcessing(feeRequest);
                fees.Calc(CSTools.SetCacheOn(CS), pDbTransaction);

                bool isExistFeeCalculated = ArrFunc.IsFilled(fees.FeeResponse);
                if (isExistFeeCalculated)
                {
                    tradeInput.SetFee(TradeInput.FeeTarget.trade, fees.FeeResponse);
                    tradeInput.ProcessFeeTax(CS, pDbTransaction, TradeInput.FeeTarget.trade, feeRequest.DtReference);
                    payment = tradeInput.CurrentTrade.OtherPartyPayment;
                }
            }
            return payment;
        }
        #endregion CalFeesClosingReopening

        #region SetTradesAccordingToClosingReopeningMode
        /// <summary>
        /// Détermination de la quantité totale et du sens associé pour Fermeture|Réouverture de position
        /// FERMETURE DE POSITION
        /// =====================
        /// TransferModeEnum.ReverseTrade             : Un pour un et sens inverse
        /// TransferModeEnum.ReverseLongShortPosition : Calcul de la quantité totale par sens
        /// TransferModeEnum.ReverseSyntheticPosition : Calcul de la quantité totale et détermination du sens
        /// 
        /// => alimentation overrideClosing sur le 1er trade
        /// 
        /// REOUVERTURE DE POSITION
        /// =======================
        /// 
        /// TransferModeEnum.Trade                    : Un pour un même sens
        /// TransferModeEnum.LongShortPosition        : Calcul de la quantité totale par sens
        /// TransferModeEnum.SyntheticPosition        : Calcul de la quantité totale et détermination du sens
        /// 
        /// => alimentation overrideReopening sur le 1er trade
        /// 
        /// et en complément Listes des trades liés
        /// </summary>
        /// <param name="pTradeKey">Clé de position</param>
        /// <param name="pLstTradeCandidates">List des trades candidats</param>
        /// <param name="pPositionInstructions">Instruction de fermeture|réouverture</param>
        // EG 20190318 New ClosingReopening position Step3
        // EG 20190613 [24683] Upd
        // EG 20210408 [24683] CRP - Implémentation Moyenne Pondérée sur Prix de négo
        private void SetTradesAccordingToClosingReopeningMode(ARQTools.TradeKey pTradeKey, List<ARQTools.TradeCandidate> pLstTradeCandidates, PositionInstructions pPositionInstructions)
        {
            List<ARQTools.OverrideTradeCandidate> lstOverride = null;
            ARQTools.PriceUsed priceUsed = (pPositionInstructions is ClosingInstructions) ? pTradeKey.priceUsedForClosing : pTradeKey.priceUsedForReopening;
            switch (pPositionInstructions.Mode)
            {
                case TransferModeEnum.ReverseLongShortPosition:
                case TransferModeEnum.LongShortPosition:
                    // Les trades sont agrégés par Side (pour une même clé) avec somme de la quantité.
                    lstOverride =
                        (
                            from tradeBySide in
                                (from trade in pLstTradeCandidates select trade).GroupBy(item => item.side)
                            select new ARQTools.OverrideTradeCandidate
                            {
                                side = tradeBySide.Key == "1" ? "2" : "1",
                                identifierSource = tradeBySide.First().identifier,
                                idTSource = tradeBySide.First().idT,
                                qty = (from trade in tradeBySide select trade.qty).Sum(),
                                price = ((null != priceUsed) && priceUsed.priceSpecified) ? priceUsed.price.DecValue : (from trade in tradeBySide select trade.price * trade.qty).Sum() / (from trade in tradeBySide select trade.qty).Sum(),

                                linkedTrades = (from linkTrade in tradeBySide
                                                select linkTrade
                                               ).ToList()
                            }
                        ).ToList();
                    break;
                case TransferModeEnum.ReverseSyntheticPosition:
                    // Les trades sont agrégés (pour une même clé) avec somme de la quantité et attribution du sens adéquat.
                    lstOverride =
                        (
                            from tradeBySide in
                                (from trade in pLstTradeCandidates select trade).GroupBy(item => item.posKeys.IdEM)
                            select new ARQTools.OverrideTradeCandidate
                            {
                                side = (from trade in tradeBySide select (trade.side == "1" ? 1 : -1) * trade.qty).Sum() > 0 ? "2" : "1",
                                identifierSource = tradeBySide.First().identifier,
                                idTSource = tradeBySide.First().idT,
                                qty = Math.Abs((from trade in tradeBySide select (trade.side == "1" ? 1 : -1) * trade.qty).Sum()),
                                price = ((null != priceUsed) && priceUsed.priceSpecified) ? priceUsed.price.DecValue : (from trade in tradeBySide select trade.price * trade.qty).Sum() / (from trade in tradeBySide select trade.qty).Sum(),

                                linkedTrades = (from linkTrade in tradeBySide
                                                select linkTrade
                                               ).ToList()
                            }
                        ).ToList();
                    break;
                case TransferModeEnum.SyntheticPosition:
                    // Les trades sont agrégés (pour une même clé) avec somme de la quantité et attribution du sens adéquat.
                    lstOverride =
                        (
                            from tradeBySide in
                                (from trade in pLstTradeCandidates select trade).GroupBy(item => item.posKeys.IdEM)
                            select new ARQTools.OverrideTradeCandidate
                            {
                                side = (from trade in tradeBySide select (trade.side == "1"? 1:-1) * trade.qty).Sum()>0 ? "1" : "2",
                                identifierSource = tradeBySide.First().identifier,
                                idTSource = tradeBySide.First().idT,
                                qty = Math.Abs((from trade in tradeBySide select (trade.side == "1"? 1:-1) * trade.qty).Sum()),
                                price = ((null != priceUsed) && priceUsed.priceSpecified) ? priceUsed.price.DecValue : (from trade in tradeBySide select trade.price * trade.qty).Sum() / (from trade in tradeBySide select trade.qty).Sum(),

                                linkedTrades = (from linkTrade in tradeBySide
                                                select linkTrade
                                               ).ToList()
                            }
                        ).ToList();
                    break;
                case TransferModeEnum.ReverseTrade:
                    lstOverride = (from trade in pLstTradeCandidates
                                      select new ARQTools.OverrideTradeCandidate()
                                      {
                                          identifierSource = trade.identifier,
                                          idTSource = trade.idT,
                                          price = ((null != priceUsed) && priceUsed.priceSpecified) ? priceUsed.price.DecValue : trade.price,
                                          qty = trade.qty,
                                          side = trade.side == "1" ? "2" : "1",
                                          linkedTrades = (from linkTrade in pLstTradeCandidates
                                                          where linkTrade.idT == trade.idT
                                                          select linkTrade).ToList()

                                      }).ToList();
                    break;
                case TransferModeEnum.Trade:
                    lstOverride = (from trade in pLstTradeCandidates
                                      select new ARQTools.OverrideTradeCandidate()
                                      {
                                          identifierSource = trade.identifier,
                                          idTSource = trade.idT,
                                          price = ((null != priceUsed) && priceUsed.priceSpecified) ? priceUsed.price.DecValue : trade.price,
                                          qty = trade.qty,
                                          side = trade.side,
                                          linkedTrades = (from linkTrade in pLstTradeCandidates
                                                          where linkTrade.idT == trade.idT
                                                          select linkTrade).ToList()
                                      }).ToList();
                    break;
            }

            if (null != lstOverride)
            {
                lstOverride.ForEach(overrideTrade =>
                {
                    ARQTools.TradeCandidate tradeFound = pLstTradeCandidates.Find(trade => trade.idT == overrideTrade.idTSource);
                    overrideTrade.reverseSide = (tradeFound.side != overrideTrade.side);
                    if (null != tradeFound)
                    {
                        if (pPositionInstructions is ClosingInstructions)
                        {
                            tradeFound.overrideClosingSpecified = true;
                            tradeFound.overrideClosing = overrideTrade;
                        }
                        else if (pPositionInstructions is ReopeningInstructions)
                        {
                            tradeFound.overrideReopeningSpecified = true;
                            tradeFound.overrideReopening = overrideTrade;
                        }
                    }
                });
            }
        }
        #endregion SetTradesAccordingToClosingReopeningMode
        #region LoadTradeForClosingReopeningAction
        /// <summary>
        /// Chargement des contextes (Demandes de traitement)
        /// </summary>
        // EG 20190308 New
        // EG 20190318 New ClosingReopening position Step3
        // EG 20190613 [24683] uPD
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20231030 [WI725] Upd Closing/Reopening : WeightingPlus
        // EG 20240520 [WI930] Upd m_ClosingReopeningDtBusiness (avec tuple)
        private Cst.ErrLevel LoadTradeForClosingReopeningAction()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            int totalTradesInPosition = 0;

            // INSERTION LOG
            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 8850), (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.EndOfDay ? 1 : 3),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Entity, m_MarketPosRequest.IdA_Entity)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.CssCustodian, m_MarketPosRequest.IdA_CssCustodian)),
                new LogParam(LogTools.IdentifierAndId(m_MarketPosRequest.Identifiers.Market, m_MarketPosRequest.IdM)),
                new LogParam(m_MarketPosRequest.GroupProductValue),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_MarketPosRequest.DtBusiness))));

            m_DicARQTradeCandidate = new Dictionary<int, ARQTools.TradeCandidate>();

            // 1. Chargement des trades en position
            DataSet ds = GetDataRequestWithIsolationLevel(CS, IsolationLevel.ReadCommitted, Cst.PosRequestTypeEnum.ClosingPosition, m_ClosingReopeningDtBusiness.dtReopening, m_PosRequest.IdEM);
            if (null != ds)
            {
                totalTradesInPosition = ds.Tables[0].Rows.Count;
                if (0 < totalTradesInPosition)
                {
                    DataView dv = ds.Tables[0].DefaultView;
                    dv.Sort = "DTEXECUTION desc";
                    DataTable sortedDt = dv.ToTable();
                    foreach (DataRow row in sortedDt.Rows)
                    {
                        ARQTools.TradeContext _context = new ARQTools.TradeContext(m_MarketPosRequest.IdA_Entity, row);

                        // 2. Test de matchage de chaque trade avec toutes les lignes du référentiel de contexte de CLOSING/REOPENING
                        m_LstClosingReopeningAction.ForEach(action => action.WeightingPlus(CS, m_ClosingReopeningDtBusiness.dtReopening, Convert.ToInt32(row["IDT"]), _context));

                        // 3. Construction de la liste des trades CANDIDATS
                        // Un trade est CANDIDAT POTENTIEL A CLOSING/REOPENING pour un contexte SI Context.ResultMatching > 0
                        List<ClosingReopeningAction> _lst = m_LstClosingReopeningAction.FindAll(action => action.Results.ResultMatching > 0);
                        if (0 < _lst.Count)
                        {
                            // Stockage des infos complètes du trade candidat
                            ARQTools.TradeCandidate _tradeCandidate = new ARQTools.TradeCandidate(row, _context);
                            m_DicARQTradeCandidate.Add(_tradeCandidate.idT, _tradeCandidate);

                            // Sélection du + fort context qui matche via tri sur ResultMatching par ordre décroissant (on prend le 1er context)
                            ClosingReopeningAction actionSelected = _lst.OrderByDescending(action => action.Results.ResultMatching).First();

                            // Ajout de l'IDT du trade candidat sur le plus fort contexte qui matche
                            if (false == actionSelected.Results.LstIdTCandidate.Exists(idT => idT == _tradeCandidate.idT))
                                actionSelected.Results.LstIdTCandidate.Add(_tradeCandidate.idT);
                        }
                    }
                    if (0 < m_DicARQTradeCandidate.Keys.Count)
                        codeReturn = Cst.ErrLevel.SUCCESS;
                }
            }

            m_LstClosingReopeningAction.FindAll(action => (0 < action.Results.LstIdTCandidate.Count)).ForEach(action =>
            {
                action.Results.TotalTradesInPosition = totalTradesInPosition;
                action.Results.TotalTradesCandidates = action.Results.LstIdTCandidate.Count;
            });
            return codeReturn;
        }
        #endregion LoadTradeForClosingReopeningAction
        #region ResetTradeCandidatesBeforeProcessing
        /// <summary>
        /// Reset des trades candidats sur chaque contexte avant TRAITEMENT
        /// . IdT_Candidate et IdT_ByPosition
        /// </summary>
        // EG 20190308 New
        private Cst.ErrLevel ResetTradeCandidatesBeforeProcessing()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            if ((null != m_LstClosingReopeningAction) && (0 < m_LstClosingReopeningAction.Count))
            {
                m_LstClosingReopeningAction.ForEach(action =>
                {
                    action.ResultsSpecified = true;
                    action.Results = new ResultActionRequest();
                });
                codeReturn = Cst.ErrLevel.SUCCESS;
            }
            return codeReturn;
        }
        #endregion ResetTradeCandidatesBeforeProcessing
        #region FinalWeighting
        /// <summary>
        /// Tri final des contextes
        /// Principe :
        /// Un contexte doit posséder des trades pour calculer son poids.
        /// Le poids est calculé sur le 1er trade de la 1ere clé de trade.
        /// Un tri par poids des contextes est appliqué à la fin de la boucle.
        /// </summary>
        // EG 20190308 New
        // EG 20190318 New ClosingReopening position Step3
        private Cst.ErrLevel FinalWeighting()
        {
            m_LstClosingReopeningAction.ForEach(action =>
            {
                action.Results.ResultMatching = 1;
                if ((null != action.Results.Positions) && (0 < action.Results.Positions.Keys.Count))
                {
                    ARQTools.TradeKey key = action.Results.Positions.Keys.First();
                    if (null != key)
                    {
                        action.Results.Positions.Keys.ToList().ForEach(trakeKey => trakeKey.actionReference = action);
                        int _idT = action.Results.Positions[key].First().idT;
                        action.Weighting(m_DicARQTradeCandidate[_idT].context);
                    }
                }
                else
                {
                    action.Results.ResultMatching = 0;
                }
            });
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion FinalWeighting
        #region SaveResultToXMLFile
        /// <summary>
        /// Sauvegard dans fichier LOG
        /// Suffixe
        ///  - CANDIDATE    : après matchage des trades en position par contexte
        ///  - CALCULATION  : après processus de calcul
        ///  - WRITING      : après processus d'écriture
        ///  
        ///  Nom du fichier : {MATCH|UNMATCH}_{IDACTION}_{IDEM}_{DTBUSINESS}_{Suffixe}.xml
        /// </summary>
        // EG 20190308 New
        // EG 20190318 New ClosingReopening position Step3
        // EG 20190613 [24683] New
        private void SaveResultToXMLFile(string pSuffixe)
        {
            m_LstClosingReopeningAction.ForEach(action => SaveResultToXMLFile(action, pSuffixe));
        }
        // EG 20190613 [24683] Upd
        // EG 20201104 [24683] Gestion AttachedDoc
        private void SaveResultToXMLFile(ClosingReopeningAction pAction, string pSuffixe)
        {
            string _fileName = "{0}_{1}_{2}_{3}_" + DtFunc.DateTimeToStringyyyyMMdd(DtBusiness) + "." + pSuffixe + ".xml";
            XmlSerializer serializer = new XmlSerializer(typeof(ClosingReopeningAction));
            _fileName = String.Format(_fileName, pAction.ResultMatchingName, pAction.RequestTypeName, pAction.spheresid, m_MarketPosRequest.IdEM);
            string endfileName = m_PKGenProcess.Session.MapTemporaryPath(_fileName, AppSession.AddFolderSessionId.True);

            using (StreamWriter streamWriter = new StreamWriter(endfileName, false, Encoding.Default))
            {
                serializer.Serialize(streamWriter, pAction);
            }
            byte[] data = FileTools.ReadFileToBytes(endfileName);
            LogTools.AddAttachedDoc(CS, m_PKGenProcess.IdProcess, m_PKGenProcess.Session.IdA, data, _fileName, Cst.TypeMIME.Text.Xml.ToString());
        }
        #endregion SaveResultToXMLFile

        #region InsertClosingPositionsRealizedMargin
        /// <summary>
        /// Calcul des RMG et insertion des événements sur offseting de fermeture de position (clôturé|clôturant)
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pPosKeepingData">Clé de position</param>
        /// <param name="pIdPADET">Id POSACTIONDET</param>
        /// <param name="pClosedTrade">Trade source cloturé</param>
        /// <param name="pIdE_EventForClosed">Id EVENT pour trade source cloturé</param>
        /// <param name="pClosingTrade">Trade de fermeture = Trade clôturant</param>
        /// <param name="pIdE_EventForClosing">Id EVENT pour trade clôturant</param>
        /// <param name="pIsRealizedMarginForClosingTrade">Indicateur de somme des RMG (Cas fermeture synthétique)</param>
        /// <param name="pIdE">Jeton IDE pour insertion</param>
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        public virtual Cst.ErrLevel InsertClosingPositionsRealizedMargin(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, DateTime pDtBusiness, int pIdPADET, 
            ARQTools.TradeCandidate pClosedTrade, int pIdE_EventForClosed,
            ARQTools.OverrideTradeCandidate pClosingTrade, int pIdE_EventForClosing, bool pIsRealizedMarginForClosingTrade, ref int pIdE)
        {
            int idE = pIdE;

            bool isDealerBuyer = IsTradeBuyer(pClosedTrade.side);
            Nullable<decimal> realizedMargin = pPosKeepingData.RealizedMargin(pClosingTrade.price, pClosedTrade.price, pClosedTrade.qty);
            PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(pPosKeepingData, EventTypeFunc.RealizedMargin, isDealerBuyer);
            _payrec.SetPayerReceiver(realizedMargin);

            idE++;
            Cst.ErrLevel codeReturn = InsertClosingPositionsRealizedMarginEvent(pDbTransaction, pPosKeepingData, pDtBusiness,
            _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver, realizedMargin,
            pClosedTrade.qty, pClosedTrade.price, pClosingTrade.price, pClosedTrade.idT, idE, pIdPADET, pIdE_EventForClosed);
            if (pIsRealizedMarginForClosingTrade)
            {
                idE++;
                codeReturn = InsertClosingPositionsRealizedMarginEvent(pDbTransaction, pPosKeepingData, pDtBusiness, 
                    _payrec.IdA_Payer, _payrec.IdB_Payer, _payrec.IdA_Receiver, _payrec.IdB_Receiver, realizedMargin,
                    pClosedTrade.qty, pClosedTrade.price, pClosingTrade.price, pClosingTrade.idTClosing, idE, pIdPADET, pIdE_EventForClosing);
            }

            pIdE = idE;
            return codeReturn;
        }
        #endregion InsertClosingPositionsRealizedMargin
        #region InsertClosingPositionsRealizedMarginEvent
        /// <summary>
        /// Calcul des RMG et insertion des événements sur offseting de fermeture de position (clôturé|clôturant)
        /// </summary>
        /// <param name="pDbTransaction">Transaction attachée</param>
        /// <param name="pPosKeepingData">Clé de position</param>
        /// <param name="pIdA_Payer">Acteur payeur</param>
        /// <param name="pIdB_Payer">Book payeur</param>
        /// <param name="pIdA_Receiver">Acteur receveur</param>
        /// <param name="pIdB_Receiver">Book receveur</param>
        /// <param name="pRealizedMarginAmount">Montant RMG</param>
        /// <param name="pQty">Quantité</param>
        /// <param name="pPrice">Price^du clôturé</param>
        /// <param name="pClosingPrice">Prix du clturant</param>
        /// <param name="pIdT">IdT du trade</param>
        /// <param name="pIdE">IdE pour insertion</param>
        /// <param name="pIdPADET">Id POSACTIONDET</param>
        /// <param name="pIdE_Event">IdE parent</param>
        /// <returns></returns>
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        private Cst.ErrLevel InsertClosingPositionsRealizedMarginEvent(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, DateTime pDtBusiness,
            Nullable<int> pIdA_Payer, Nullable<int> pIdB_Payer, Nullable<int> pIdA_Receiver, Nullable<int> pIdB_Receiver,
            Nullable<decimal> pRealizedMarginAmount, decimal pQty, Nullable<decimal> pPrice, Nullable<decimal> pClosingPrice,
            int pIdT, int pIdE, int pIdPADET, int pIdE_Event)
        {
            Cst.ErrLevel codeReturn = m_EventQuery.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1,
                pIdA_Payer, pIdB_Payer, pIdA_Receiver, pIdB_Receiver,
                EventCodeFunc.LinkedProductClosing, EventTypeFunc.RealizedMargin, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness,
                pRealizedMarginAmount, pPosKeepingData.Asset.currency, UnitTypeEnum.Currency.ToString(), null, null);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = m_EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.ValueDate, pDtBusiness, false);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = InsertEventDet(pDbTransaction, pPosKeepingData, pIdE, pQty, pPosKeepingData.Asset.contractMultiplier, null, pPrice, pClosingPrice);

            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);
            return codeReturn;
        }
        #endregion InsertClosingPositionsRealizedMarginEvent


        #region AggregateRealizedMargin
        /// <summary>
        /// Calcul de la somme des RMG un trade de Fermeture pour n trades sources (Mode synthétique)
        /// </summary>
        /// <param name="pPosKeepingData">Clé de position</param>
        /// <param name="pTradeClosing">Trade clôturant</param>
        // EG 20190613 [24683] New
        public virtual AgreggatePayerReceiverAmountInfo AggregateRealizedMargin(IPosKeepingData pPosKeepingData, ARQTools.OverrideTradeCandidate pTradeClosing)
        {
            List<PayerReceiverAmountInfo> lstRealizedMargin = new List<PayerReceiverAmountInfo>();
            pTradeClosing.linkedTrades.ForEach(tradeClosed =>
            {
                bool isDealerBuyer = IsTradeBuyer(tradeClosed.side);
                PayerReceiverAmountInfo _payrec = new PayerReceiverAmountInfo(pPosKeepingData, EventTypeFunc.RealizedMargin, isDealerBuyer);
                _payrec.SetPayerReceiver(pPosKeepingData.RealizedMargin(pTradeClosing.price, tradeClosed.price, tradeClosed.qty));
                lstRealizedMargin.Add(_payrec);
            });


            List<AgreggatePayerReceiverAmountInfo> sumRealizedMarginByPayRec = (from realizedMarginByPayRec in lstRealizedMargin
                                                                                group realizedMarginByPayRec by new
                                                                                {
                                                                                    realizedMarginByPayRec.IdA_Payer,
                                                                                    realizedMarginByPayRec.IdB_Payer,
                                                                                    realizedMarginByPayRec.IdA_Receiver,
                                                                                    realizedMarginByPayRec.IdB_Receiver
                                                                                } into cumulatedRealizedMargin
                                                                                select new AgreggatePayerReceiverAmountInfo(
                                                                                    cumulatedRealizedMargin.Sum(rmg => rmg.Amount.Value),
                                                                                    cumulatedRealizedMargin.Key.IdA_Payer,
                                                                                    cumulatedRealizedMargin.Key.IdB_Payer,
                                                                                    cumulatedRealizedMargin.Key.IdA_Receiver,
                                                                                    cumulatedRealizedMargin.Key.IdB_Receiver)
                                                                                    ).ToList();

            Nullable<decimal> realizedMargin = sumRealizedMarginByPayRec.Sum(item => item.Amount);
            AgreggatePayerReceiverAmountInfo result = 
                (Math.Sign(realizedMargin.Value) == Math.Sign(sumRealizedMarginByPayRec.First().Amount.Value)) ?
                sumRealizedMarginByPayRec.First(): sumRealizedMarginByPayRec.Last();
            result.Amount = realizedMargin;
            return result;
        }
        #endregion AggregateRealizedMargin
#endregion Methods
    }


    public partial class PosKeepingGen_OTC
    {
        #region InsertClosingPositionsRealizedMargin
        /// <summary>
        /// Calcul des RMG (sur RTS) et insertion des événements sur offseting de fermeture de position (clôturé|clôturant)
        /// </summary>
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        public override Cst.ErrLevel InsertClosingPositionsRealizedMargin(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, DateTime pDtBusiness, int pIdPADET,
            ARQTools.TradeCandidate pClosedTrade, int pIdE_EventForClosed,
            ARQTools.OverrideTradeCandidate pClosingTrade, int pIdE_EventForClosing,
            bool pIsRealizedMarginForClosingTrade, ref int pIdE)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (pPosKeepingData.Product.IsRTS)
            {
                codeReturn = base.InsertClosingPositionsRealizedMargin(pDbTransaction, pPosKeepingData, pDtBusiness, pIdPADET,
                    pClosedTrade, pIdE_EventForClosed, pClosingTrade, pIdE_EventForClosing, pIsRealizedMarginForClosingTrade, ref pIdE);
            }
            return codeReturn;
        }
        #endregion InsertClosingPositionsRealizedMargin
    }
    public partial class PosKeepingGen_COM
    {
        #region InsertClosingPositionsRealizedMargin
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        public override Cst.ErrLevel InsertClosingPositionsRealizedMargin(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, DateTime pDtBusiness,  int pIdPADET,
            ARQTools.TradeCandidate pClosedTrade, int pIdE_EventForClosed,
            ARQTools.OverrideTradeCandidate pClosingTrade, int pIdE_EventForClosing,
            bool pIsRealizedMarginForClosingTrade,  ref int pIdE)
        {
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertClosingPositionsRealizedMargin

    }
    public partial class PosKeepingGen_SEC
    {
        #region InsertClosingPositionsRealizedMargin
        /// <summary>
        /// Calcul des RMG (sur EST) et insertion des événements sur offseting de fermeture de position (clôturé|clôturant)
        /// </summary>
        // EG 20190613 [24683] New
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        public override Cst.ErrLevel InsertClosingPositionsRealizedMargin(IDbTransaction pDbTransaction, IPosKeepingData pPosKeepingData, DateTime pDtBusiness, int pIdPADET,
            ARQTools.TradeCandidate pClosedTrade, int pIdE_EventForClosed,
            ARQTools.OverrideTradeCandidate pClosingTrade, int pIdE_EventForClosing,
            bool pIsRealizedMarginForClosingTrade, ref int pIdE)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            if (pPosKeepingData.Product.IsEquitySecurityTransaction)
            {
                codeReturn = base.InsertClosingPositionsRealizedMargin(pDbTransaction, pPosKeepingData, pDtBusiness, pIdPADET,
                    pClosedTrade, pIdE_EventForClosed, pClosingTrade, pIdE_EventForClosing, pIsRealizedMarginForClosingTrade, ref pIdE);
            }
            return codeReturn;
        }
        #endregion InsertClosingPositionsRealizedMargin
    }
}

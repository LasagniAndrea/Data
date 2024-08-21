#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.Settlement;
using EfsML.Settlement.Message;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
#endregion Using Directives

namespace EFS.Process.SettlementInstrGen
{
    /// <summary>
    /// 
    /// </summary>
    public class SettlementInstrTradeGen
    {
        #region  Members
        private readonly SettlementInstrGenProcess siProcess;
        private readonly SettlementInstrGenMQueue settlementInstrGenMQueue;
        private readonly string connectionString;
        private readonly EFS_TradeLibrary tradeLibrary;
        private readonly DataSetTrade dsTrade;
        private readonly DataSetEventTrade dsEvents;
        //private DataSet dsEventSi;
        private readonly EventProcess eventProcess;
        #endregion
        #region Accessor
        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEvent
        {
            get
            {
                if (null != dsEvents)
                    return dsEvents.DtEvent;
                else
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventSi
        {
            get
            {
                DataTable dt = null;
                if (null != dsEvents)
                    dt = siProcess.IsModeSimul ? dsEvents.DtEventSi_Simul : dsEvents.DtEventSi;
                return dt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTable DtEventClass
        {
            get
            {
                DataTable dt = null;
                if (null != dsEvents)
                    dt = dsEvents.DtEventClass;
                return dt;
            }
        }

        #endregion Accessor
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSiProcess"></param>
        /// <param name="pDsTrade"></param>
        /// <param name="pTradeLibrary"></param>
        public SettlementInstrTradeGen(SettlementInstrGenProcess pSiProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary)
        {
            siProcess = pSiProcess;
            settlementInstrGenMQueue = pSiProcess.settlementInstrGenMQueue;
            connectionString = settlementInstrGenMQueue.ConnectionString;
            dsTrade = pDsTrade;
            tradeLibrary = pTradeLibrary;
            dsEvents = new DataSetEventTrade(connectionString, dsTrade.IdT);
            eventProcess = new EventProcess(connectionString);
            if (siProcess.IsModeSimul)
                AddDtEventSiSimul();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel Execute()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            DataRow[] rowsEvent = dsEvents.GetRowsEvent();

            foreach (DataRow rowEvent in rowsEvent)
            {
                bool isError = false;
                DateTime stlDtEventForced;

                // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
                //bool isRowMustBeCalculate = IsRowHasSettlementClass(rowEvent, out stlDtEventForced);
                //20071107 FI Ticket 15916 => Pas d'alimentation de EVENTPROCESS si not IsRowEventcompatibleWithMQueueParameter
                // EG 20100617 Ticket 17051 SSI for TRADEADMIN
                bool isRowMustBeCalculate = false;
                if (siProcess.IsTradeAdmin)
                    isRowMustBeCalculate = IsRowHasNetTurnOverIssueRecognition(rowEvent, out stlDtEventForced);
                else
                    isRowMustBeCalculate = IsRowHasSettlementClass(rowEvent, out stlDtEventForced);

                //0101122 FI [17229] Spheres® ne traite les règlements ou le payer et le receiver sont connus
                if (isRowMustBeCalculate)
                    isRowMustBeCalculate = (rowEvent["IDA_PAY"] != Convert.DBNull) && (rowEvent["IDA_REC"] != Convert.DBNull);

                if (isRowMustBeCalculate)
                    isRowMustBeCalculate = IsRowEventcompatibleWithMQueueParameter(rowEvent);

                if (isRowMustBeCalculate)
                {
                    int idEvent = Convert.ToInt32(rowEvent["IDE"]);
                    try
                    {
                        SQL_Event sqlEvent = new SQL_Event(siProcess.Cs, idEvent);
                        sqlEvent.LoadTable();


                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 3402), 2,
                            new LogParam(LogTools.IdentifierAndId(sqlEvent.EventCode + "-" + sqlEvent.EventType, sqlEvent.Id)),
                            new LogParam(sqlEvent.Unit),
                            new LogParam(DtFunc.DateTimeToStringDateISO(sqlEvent.DtStartAdjusted)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(sqlEvent.DtEndAdjusted))));
                        Logger.Write();

                        if (Cst.ErrLevel.SUCCESS != siProcess.ScanCompatibility_Event(idEvent))
                            isRowMustBeCalculate = false;

                        if (isRowMustBeCalculate)
                        {
                            // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
                            SearchEventSi searchSi = new SearchEventSi(siProcess.Cs, idEvent, stlDtEventForced, tradeLibrary);

                            
                            //searchSi.LoadSettlementChain(siProcess.processLog.header.IdProcess);
                            searchSi.LoadSettlementChain(siProcess.IdProcess);

                            UpdDataEventSi(idEvent, searchSi.SettlementChain, PayerReceiverEnum.Receiver);
                            UpdDataEventSi(idEvent, searchSi.SettlementChain, PayerReceiverEnum.Payer);
                            UpdDataEventClass(rowEvent, searchSi.SettlementChain);
                            UpdateDataEventProcess(idEvent, Tuning.TuningOutputTypeEnum.OES);
                        }
                    }
                    catch (Exception ex)
                    {
                        isError = true;
                        if (StrFunc.ContainsIn(ex.Message, "ORA-2"))
                        {
                            siProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, "Exception occured while calling Oracle Procedure"));
                        }
                        else
                        {
                            SpheresException2 ex2 = SpheresExceptionParser.GetSpheresException(string.Empty, ex);

                            // Les messages personnalisés Issus de PROC ORACLE ne sont ajoutés dans le Log 
                            //=> C'est la procedure qui s'en charge (Exit User)
                            siProcess.ProcessState.AddCriticalException(ex2);
                            
                            Logger.Log(new LoggerData(ex2));
                        }
                    }
                    finally
                    {
                        if (isError)
                        {
                            ret = Cst.ErrLevel.FAILURE;
                            UpdateDataEventProcess(idEvent, Tuning.TuningOutputTypeEnum.OEE);
                        }
                        Update(idEvent, isError);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private DataRow GetRowSettlementOrDeliveryMessage(DataRow pRow)
        {
            bool isDeliveryMessage = EventTypeFunc.IsQuantity(pRow["EVENTTYPE"].ToString());
            DataRow row = GetRowEventClass(pRow, (isDeliveryMessage ? EventClassFunc.DeliveryMessage : EventClassFunc.SettlementMessage));
            return row;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pEventClass"></param>
        /// <returns></returns>
        private DataRow GetRowEventClass(DataRow pRow, string pEventClass)
        {
            DataRow row = null;
            DataRow[] rowChilds = pRow.GetChildRows(dsEvents.ChildEventClass);
            foreach (DataRow rowChild in rowChilds)
            {
                if (pEventClass == rowChild["EVENTCLASS"].ToString())
                    row = rowChild;
            }
            return row;
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddDtEventSiSimul()
        {
            string sqlSelectEventSi = QueryLibraryTools.GetQuerySelect(connectionString, Cst.OTCml_TBL.EVENTSI_T);
            sqlSelectEventSi += @"where (ev.IDT = @IDT)" + Cst.CrLf;
            sqlSelectEventSi += @"order by ev.IDE" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(connectionString, "IDT", DbType.Int32), dsEvents.IdT);
            QueryParameters qryParameters = new QueryParameters(connectionString, sqlSelectEventSi, parameters);
            DataTable dtEventSiSimul = DataHelper.ExecuteDataTable(connectionString, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            dtEventSiSimul.TableName = "EventSi_T";
            dtEventSiSimul.PrimaryKey = new DataColumn[2] { dtEventSiSimul.Columns["IDE"], dtEventSiSimul.Columns["PAYER_RECEIVER"] };
            dsEvents.DsEvent.Tables.Add(dtEventSiSimul);

            if ((null != dsEvents.DtEvent) && (null != dsEvents.DtEventSi))
            {
                DataRelation relEventSiSimul = new DataRelation("Event_EventSi_T", dsEvents.DtEvent.Columns["IDE"], dsEvents.DtEventSi_Simul.Columns["IDE"], false);
                dsEvents.DsEvent.Relations.Add(relEventSiSimul);
            }
        }

        /// <summary>
        ///  Retourne true s'il existe un EVENT CLASS STL avec ISPAYMENT = true
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pStlDtEventForced"></param>
        /// <returns></returns>
        private bool IsRowHasSettlementClass(DataRow pRow, out DateTime pStlDtEventForced)
        {
            // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
            bool ret = false;
            pStlDtEventForced = DateTime.MinValue;
            //			
            DataRow row = GetRowEventClass(pRow, EventClassFunc.Settlement);
            if (null != row)
            {
                ret = Convert.ToBoolean(row["ISPAYMENT"]);
                pStlDtEventForced = Convert.ToDateTime(row["DTEVENTFORCED"]);
            }
            //
            return ret;
        }

        /// <summary>
        ///  Retourne true si LPP, NTI , REC 
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pStlDtEventForced"></param>
        /// <returns></returns>
        /// EG 20100617 Ticket 17051 SSI for TRADEADMIN
        private bool IsRowHasNetTurnOverIssueRecognition(DataRow pRow, out DateTime pStlDtEventForced)
        {
            bool ret = false;
            pStlDtEventForced = DateTime.MinValue;
            if (EventCodeFunc.IsLinkedProductPayment(pRow["EVENTCODE"].ToString()) &&
                EventTypeFunc.IsNetTurnOverIssueAmount(pRow["EVENTTYPE"].ToString()))
            {
                DataRow row = GetRowEventClass(pRow, EventClassFunc.Recognition);
                if (null != row)
                {
                    ret = true;
                    pStlDtEventForced = Convert.ToDateTime(row["DTEVENTFORCED"]);
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private bool IsRowEventcompatibleWithMQueueParameter(DataRow pRow)
        {
            bool isOk = true;
            //
            if (settlementInstrGenMQueue.parametersSpecified)
            {
                //PARAM_DTSTL ou PARAM_DATE1
                if (isOk)
                {
                    if ((null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTL))
                        ||
                        (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_DATE1)))
                    {
                        bool isToEnd = settlementInstrGenMQueue.GetBoolValueParameterById(SettlementInstrGenMQueue.PARAM_ISTOEND);
                        //                                    
                        DateTime paramDtSTL = settlementInstrGenMQueue.GetDateTimeValueParameterById(SettlementInstrGenMQueue.PARAM_DATE1);
                        if (DateTime.MinValue == paramDtSTL)
                            paramDtSTL = settlementInstrGenMQueue.GetDateTimeValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTL);
                        //
                        DataRow row = GetRowEventClass(pRow, (siProcess.IsTradeAdmin ? EventClassFunc.Recognition : EventClassFunc.Settlement));
                        if (isToEnd)
                            isOk = DateTime.Compare(Convert.ToDateTime(row["DTEVENTFORCED"]), paramDtSTL) >= 0;
                        else
                            isOk = (0 == DateTime.Compare(Convert.ToDateTime(row["DTEVENTFORCED"]), paramDtSTL));
                    }
                }
                //PARAM_DTSTM
                if (isOk)
                {
                    if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTM))
                    {
                        bool isToEnd = settlementInstrGenMQueue.GetBoolValueParameterById(SettlementInstrGenMQueue.PARAM_ISTOEND);
                        DateTime paramDtSTM = settlementInstrGenMQueue.GetDateTimeValueParameterById(SettlementInstrGenMQueue.PARAM_DTSTM);
                        string eventType = pRow["EVENTTYPE"].ToString();
                        _ = EventTypeFunc.IsQuantity(eventType);
                        DataRow row = GetRowSettlementOrDeliveryMessage(pRow);
                        isOk = (null != row);
                        if (isOk)
                        {
                            if (isToEnd)
                                isOk = DateTime.Compare(Convert.ToDateTime(row["DTEVENTFORCED"]), paramDtSTM) >= 0;
                            else
                                isOk = (0 == DateTime.Compare(Convert.ToDateTime(row["DTEVENTFORCED"]), paramDtSTM));
                        }
                    }
                }
                //PARAM_IDA_PAY
                if (isOk)
                {
                    if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_PAY))
                    {
                        int idA_Pay = settlementInstrGenMQueue.GetIntValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_PAY);
                        isOk = (Convert.ToInt32(pRow["IDA_PAY"]) == idA_Pay);
                    }
                }
                //PARAM_IDA_REC
                if (isOk)
                {
                    if (null != settlementInstrGenMQueue.GetObjectValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_REC))
                    {
                        int idA_Rec = settlementInstrGenMQueue.GetIntValueParameterById(SettlementInstrGenMQueue.PARAM_IDA_REC);
                        isOk = (Convert.ToInt32(pRow["IDA_REC"]) == idA_Rec);
                    }
                }
            }
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEvent"></param>
        /// <param name="pSettlementChain"></param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void UpdDataEventClass(DataRow pEvent, ISettlementChain pSettlementChain)
        {

            #region Calcul DateSTM
            IProductBase productBase = tradeLibrary.Product.ProductBase;
            DateTime dateSTM = Convert.ToDateTime(null);
            bool isGenEventClassSTM = false;
            IBusinessCenters lastBcs = null;
            //
            foreach (string s in Enum.GetNames(typeof(PayerReceiverEnum)))
            {
                PayerReceiverEnum payRec = (PayerReceiverEnum)Enum.Parse(typeof(PayerReceiverEnum), s);
                IEfsSettlementInstruction si = pSettlementChain[payRec].SettlementInstruction;
                //	
                if (null != si)
                {
                    int idCss = SettlementTools.GetIdCss(connectionString, (IEfsSettlementInstruction)si);
                    if (0 < idCss)
                    {
                        //Pour déterminer la date de message => Considération des businessCenters  suivant
                        // Si Payeur => BC du correspondant , siReceiver => BC du dernier intermédiaire (si l'un ou l'autre existe)
                        // BC le l'acteur CSS
                        // BC le l'acteur beneficiairy Bank (s'il existe)
                        // DTEVENT = Date Théorique
                        // Ensuite la date de message est égale à 
                        // DTEVENTFORCED = MAX (Date Théorique, Dat Jour)
                        //
                        if (false == isGenEventClassSTM)
                            isGenEventClassSTM = true;
                        //
                        #region Load offset
                        SQL_Css sqlCss = new SQL_Css(connectionString, idCss);
                        sqlCss.LoadTable(new string[] { "a.IDA", "ISMSGISSUE", "PERIODMLTPMSGISSUE", "PERIODMSGISSUE" });
                        IOffset offset;
                        //20070917 Prise en consideration de décalage pour calcul da la Date STM
                        //sqlCss.IsMsgIssue n'est utilisé que pur la mesagerie
                        if (sqlCss.IsLoaded && (StrFunc.IsFilled(sqlCss.PeriodMsgIssue)))
                            offset = productBase.CreateOffset(StringToEnum.Period(sqlCss.PeriodMsgIssue), sqlCss.PeriodMultiplierMsgIssue, DayTypeEnum.Business);
                        else
                            offset = productBase.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.Business);
                        #endregion Load offset
                        //
                        #region Load BusinessCenters
                        ArrayList aIDBCs = new ArrayList();
                        int idA = 0;
                        if (payRec == PayerReceiverEnum.Payer && (si.CorrespondentInformationSpecified))
                        {
                            idA = new RoutingContainer(si.CorrespondentInformation).GetRoutingIdA(connectionString);
                        }
                        else if (payRec == PayerReceiverEnum.Receiver && (si.IntermediaryInformationSpecified))
                        {
                            if (0 < si.IntermediaryInformation.Length)
                            {
                                int lastIndex = si.IntermediaryInformation.Length - 1;
                                idA = new RoutingContainer(si.IntermediaryInformation[lastIndex]).GetRoutingIdA(connectionString);
                            }
                        }
                        if (0 < idA)
                        {
                            SQL_Actor actor = new SQL_Actor(connectionString, idA);
                            if (actor.LoadTable(new string[] { "IDA", "IDBC" }))
                            {
                                if (StrFunc.IsFilled(actor.IdBC))
                                    aIDBCs.Add(actor.IdBC);
                            }
                        }
                        //
                        if ((0 == aIDBCs.Count) && sqlCss.IsLoaded)
                        {
                            sqlCss.LoadSQlActor();
                            if (StrFunc.IsFilled(sqlCss.SqlActor.IdBC))
                                aIDBCs.Add(sqlCss.SqlActor.IdBC);
                        }
                        //
                        if (si.BeneficiaryBankSpecified)
                        {
                            int IdA = new RoutingContainer(si.BeneficiaryBank).GetRoutingIdA(connectionString);
                            SQL_Actor actor = new SQL_Actor(connectionString, IdA);
                            if (actor.LoadTable(new string[] { "IDA", "IDBC" }))
                            {
                                if (StrFunc.IsFilled(actor.IdBC))
                                    aIDBCs.Add(actor.IdBC);
                            }
                        }
                        #endregion Load BusinessCenters

                        IBusinessDayAdjustments bda =
                            productBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, (string[])aIDBCs.ToArray(typeof(string)));
                        //CalCul Date 
                        DateTime dtStart = Convert.ToDateTime(GetRowEventClass(pEvent, (siProcess.IsTradeAdmin ? EventClassFunc.Recognition : EventClassFunc.Settlement))["DTEVENT"]);
                        EFS_Offset efs_Offset = new EFS_Offset(connectionString, offset, dtStart, bda, tradeLibrary.DataDocument);
                        DateTime dtTheo = efs_Offset.offsetDate[0];
                        //1er tour de boucle PayerReceiverEnum => dateSTM est vide
                        if (DtFunc.IsDateTimeEmpty(dateSTM))
                        {
                            dateSTM = dtTheo;
                            lastBcs = (IBusinessCenters)bda.BusinessCentersDefine.Clone();
                        }
                        else
                        {
                            if (0 < dateSTM.CompareTo(dtTheo))
                                dateSTM = dtTheo;
                            // Decalage au jour suivant
                            offset = productBase.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.Business);
                            bda.BusinessCentersDefine = lastBcs;
                            bda.BusinessCentersDefineSpecified = (null != lastBcs.BusinessCenter);
                            efs_Offset = new EFS_Offset(connectionString, offset, dateSTM, bda, tradeLibrary.DataDocument);
                            dateSTM = efs_Offset.offsetDate[0];
                        }
                    }
                }
            }
            #endregion
            //				
            #region Calcul Info Netting
            NettingMethodEnum netMethod = NettingMethodEnum.Standard;
            DataDocumentContainer dataDocument = tradeLibrary.DataDocument;

            int netId = 0;
            // Sauf Commission et coutage => Tentative de netting
            if ((false == dataDocument.IsPartyBroker(Convert.ToInt32(pEvent["IDA_PAY"]))) &&
                (false == dataDocument.IsPartyBroker(Convert.ToInt32(pEvent["IDA_REC"]))) &&
                (false == EventCodeFunc.IsOtherPartyPayment(Convert.ToString(pEvent["EVENTCODE"])))
                )
            {
                //Default
                netMethod = NettingMethodEnum.Convention;
                //
                ITrade trade = tradeLibrary.CurrentTrade;
                if (trade.NettingInformationInputSpecified)
                    netMethod = trade.NettingInformationInput.NettingMethod;
                //
                switch (netMethod)
                {
                    case NettingMethodEnum.None:
                        netMethod = trade.NettingInformationInput.NettingMethod;
                        netId = 0;
                        break;

                    case NettingMethodEnum.Designation:
                        if (trade.NettingInformationInput.NettingDesignation.OTCmlId > 0)
                        {
                            netMethod = trade.NettingInformationInput.NettingMethod;
                            netId = trade.NettingInformationInput.NettingDesignation.OTCmlId;
                        }
                        break;
                    case NettingMethodEnum.Convention:
                        string currency = pEvent["UNIT"].ToString();
                        int instrNo = Convert.ToInt32(pEvent["INSTRUMENTNO"]);
                        int idAPayer = Convert.ToInt32(pEvent["IDA_PAY"]);
                        int idAReceiver = Convert.ToInt32(pEvent["IDA_REC"]);
                        IProduct product = (IProduct)ReflectionTools.GetObjectById(tradeLibrary.CurrentTrade, Cst.FpML_InstrumentNo + instrNo.ToString());
                        //
                        if (null != product)
                        {
                            NetConventions netConventions =
                                new NetConventions(product.ProductBase.ProductType.OTCmlId, currency, idAPayer, idAReceiver);
                            SettlementTools.LoadNetConvention(connectionString, netConventions, SQL_Table.ScanDataDtEnabledEnum.Yes);
                            netConventions.Sort();
                            if (0 < netConventions.Count)
                                netId = netConventions[0].idNetConvention;
                        }
                        //
                        if (0 == netId)
                            netMethod = NettingMethodEnum.Standard;
                        break;
                    default:
                        break;
                }
            }
            #endregion
            //	
            DataRow row = GetRowSettlementOrDeliveryMessage(pEvent);
            //	
            if (isGenEventClassSTM)
            {
                bool isInsert = (null == row);
                if (isInsert)
                    row = DtEventClass.NewRow();
                //
                row["IDE"] = pEvent["IDE"];
                //
                string eventType = pEvent["EVENTTYPE"].ToString();
                if (EventTypeFunc.IsQuantity(eventType))
                    row["EVENTCLASS"] = EventClassFunc.DeliveryMessage;
                else
                    row["EVENTCLASS"] = EventClassFunc.SettlementMessage;
                row["DTEVENT"] = dateSTM;
                row["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(connectionString, dateSTM);
                row["ISPAYMENT"] = false;
                //
                row["NETMETHOD"] = netMethod.ToString();
                row["IDNETCONVENTION"] = (netId > 0 && (NettingMethodEnum.Convention == netMethod)) ? netId : Convert.DBNull;
                row["IDNETDESIGNATION"] = (netId > 0 && (NettingMethodEnum.Designation == netMethod)) ? netId : Convert.DBNull;
                //
                if (isInsert)
                    DtEventClass.Rows.Add(row);
            }
            else
            {
                if (null != row)
                    DtEventClass.Rows.Remove(row);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdEvt"></param>
        /// <param name="pSettlementChain"></param>
        /// <param name="pPayRec"></param>
        private void UpdDataEventSi(int pIdE, ISettlementChain pSettlementChain, FpML.Enum.PayerReceiverEnum pPayRec)
        {
            IEfsSettlementInstruction settlementInstructions = pSettlementChain[pPayRec].SettlementInstruction;
            Type tSettlementInstructions = Tools.GetTypeSettlementInstruction(tradeLibrary.DataDocument.DataDocument);
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(tSettlementInstructions, settlementInstructions);
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
            object[] findTheseVals = new object[2];
            findTheseVals[0] = pIdE;
            findTheseVals[1] = pPayRec.ToString();
            DataRow row = DtEventSi.Rows.Find(findTheseVals);
            bool isNewRow = (null == row);
            //
            if (isNewRow)
                row = DtEventSi.NewRow();
            //
            row["IDE"] = pIdE;
            row["PAYER_RECEIVER"] = pPayRec.ToString();
            row["SIXML"] = sb.ToString();
            row["SIMODE"] = pSettlementChain[pPayRec].SiMode.ToString();
            //
            row["IDSSIDB"] = Convert.DBNull;
            int idSsiDb = pSettlementChain[pPayRec].SettlementInstruction.IdssiDb;
            if (idSsiDb > 0)
                row["IDSSIDB"] = idSsiDb;
            //
            int idIssi = pSettlementChain[pPayRec].SettlementInstruction.IdIssi;
            if (idIssi > 0)
                row["IDISSI"] = idIssi;
            //
            row["IDA_CSS"] = Convert.DBNull;
            int idCss = SettlementTools.GetIdCss(connectionString, (IEfsSettlementInstruction)pSettlementChain[pPayRec].SettlementInstruction);
            if (idCss > 0)
                row["IDA_CSS"] = idCss;
            //
            row["IDCSSLINK"] = Convert.DBNull;
            if (pSettlementChain.CssLinkSpecified)
                row["IDCSSLINK"] = pSettlementChain.CssLink;
            //
            row["IDA_STLOFFICE"] = Convert.DBNull;
            int idASltOffice = pSettlementChain[pPayRec].IdASettlementOffice;
            if (idASltOffice > 0)
                row["IDA_STLOFFICE"] = idASltOffice.ToString();
            //
            row["SIREF"] = SettlementTools.GetSiRef(connectionString, pPayRec, pSettlementChain);
            row["IDA_MSGRECEIVER"] = SettlementTools.GetIdAMsgReceiver(connectionString, pPayRec, pSettlementChain);
            //
            if (isNewRow)
            {
                row["IDAINS"] = siProcess.Session.IdA;
                // FI 20200820 [25468] dates systèmes en UTC
                row["DTINS"] = OTCmlHelper.GetDateSysUTC(connectionString);
            }
            else
            {
                row["IDAUPD"] = siProcess.Session.IdA;
                // FI 20200820 [25468] dates systèmes en UTC
                row["DTUPD"] = OTCmlHelper.GetDateSysUTC(connectionString);
            }
            //
            if (isNewRow)
                DtEventSi.Rows.Add(row);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdEvent"></param>
        /// <param name="pTuningOutputEnum"></param>
        private void UpdateDataEventProcess(int pIdEvent, Tuning.TuningOutputTypeEnum pTuningOutputEnum)
        {
            if (siProcess.ProcessTuningSpecified)
                dsEvents.SetEventStatus(pIdEvent, siProcess.ProcessTuning.GetProcessTuningOutput(pTuningOutputEnum), siProcess.Session.IdA, OTCmlHelper.GetDateSysUTC(siProcess.Cs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pIsError"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        private void Update(int pIdE, bool pIsError)
        {
            bool existErr = false;
            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(connectionString);

                dsEvents.Update(dbTransaction, dsEvents.DtEventSi, Cst.OTCml_TBL.EVENTSI);
                dsEvents.Update(dbTransaction, Cst.OTCml_TBL.EVENTCLASS);
                dsEvents.Update(dbTransaction,Cst.OTCml_TBL.EVENTSTCHECK);
                dsEvents.Update(dbTransaction, Cst.OTCml_TBL.EVENTSTMATCH);
                ProcessStateTools.StatusEnum statusEnum = pIsError ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusSuccessEnum;
                // FI 20200820 [25468] dates systèmes en UTC
                eventProcess.Write(dbTransaction, pIdE, Cst.ProcessTypeEnum.SIGEN, statusEnum, OTCmlHelper.GetDateSysUTC(this.connectionString), siProcess.Tracker.IdTRK_L);
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception) { existErr = true; throw; }
            finally
            {
                if ((existErr) && (null != dbTransaction))
                    DataHelper.RollbackTran(dbTransaction);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Recherche les Settlement Instruction qui s'applique sur un évement
    /// La chaîne de règlement résultat de la recherche est disponible dans _settlementChain
    /// </summary>
    public class SearchEventSi
    {
        #region Members
        private readonly EFS_TradeLibrary _tradeLibrary;
        /// <summary>
        /// Obtient le produit rattaché à l'évènement
        /// Si l'évènenement est un OPP, Obtient le produit principale du trade
        /// </summary>
        private readonly ProductContainer _productContainer;
        /// <summary>
        /// Obtient l'évènement sur lequel la recherche de SI s'applique
        /// </summary>
        private readonly SQL_Event _sqlEvent;
        // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
        private readonly DateTime _stlDtEventForced;
        /// <summary>
        /// 
        /// </summary>
        private ISettlementChain _settlementChain;
        /// <summary>
        /// 
        /// </summary>
        private readonly string cs;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Obtient la chaine de règlement Obtenu
        /// </summary>
        public ISettlementChain SettlementChain
        {
            get { return _settlementChain; }
        }
        #endregion Accessors
        #region Constructors
        public SearchEventSi(string pCS, int pIdE, DateTime pStlDtStlEventForced, EFS_TradeLibrary pTradeLibrary)
            : this(pCS, new SQL_Event(pCS, pIdE), pStlDtStlEventForced, pTradeLibrary)
        {
        }
        public SearchEventSi(string pCS, SQL_Event pEvent, DateTime pStlDtStlEventForced, EFS_TradeLibrary pTradeLibrary)
        {
            cs = pCS;
            _tradeLibrary = pTradeLibrary;
            // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
            _stlDtEventForced = pStlDtStlEventForced;
            _sqlEvent = pEvent;
            if (false == _sqlEvent.IsLoaded)
                _sqlEvent.LoadTable();
            //
            _productContainer = _tradeLibrary.DataDocument.CurrentProduct.GetProduct(_sqlEvent.InstrumentNo_ID);
            if (null == _productContainer) //cas des OPP car non rattachés à un produit
                _productContainer = _tradeLibrary.DataDocument.CurrentProduct;
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdLog"></param>
        /// <returns></returns>
        /// 
        // EG 20180425 Analyse du code Correction [CA2202]
        public bool LoadSettlementChain(int pIdLog)
        {
            _settlementChain = null;
            IEfsSettlementInstruction[] siReceiver = null;
            IEfsSettlementInstruction[] siPayer = null;
            if (null == _tradeLibrary)
                throw new NullReferenceException(" LoadSettlementChain : _tradeLibrary is null");

            if ((null == _sqlEvent) || (false == _sqlEvent.IsLoaded))
                throw new NullReferenceException(" LoadSettlementChain : _sqlEvent is null or not Loaded ");

            _settlementChain = _tradeLibrary.CurrentTrade.CreateSettlementChain();

            // Load Settlement office
            // EG 20220519 [WI637] Ajout paramètre pIsAdministrativeProduct
            _settlementChain[PayerReceiverEnum.Receiver].IdASettlementOffice = SettlementTools.GetSettlementOfficeIdA(cs, null, _sqlEvent.GetIdAPayRec(PayerReceiverEnum.Receiver), _sqlEvent.GetIdBook(PayerReceiverEnum.Receiver));
            _settlementChain[PayerReceiverEnum.Payer].IdASettlementOffice = SettlementTools.GetSettlementOfficeIdA(cs, null, _sqlEvent.GetIdAPayRec(PayerReceiverEnum.Payer), _sqlEvent.GetIdBook(PayerReceiverEnum.Payer));


            //Receiver
            #region Is Receiver Osi
            ISettlementInputInfo osiRec = null;
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Receiver].SiMode)
            {
                osiRec = GetOsi(PayerReceiverEnum.Receiver);
                if (null != osiRec)
                {
                    if (osiRec.SettlementInformation.StandardSpecified)
                        _settlementChain[PayerReceiverEnum.Receiver].SiMode = SiModeEnum.Oessi;
                    else if (osiRec.SettlementInformation.InstructionSpecified)
                        _settlementChain[PayerReceiverEnum.Receiver].SiMode = SiModeEnum.Osi;
                }
            }
            #endregion

            #region Is Receiver/Payer Fsi
            ISettlementInformation fsi = null;
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Receiver].SiMode)
            {
                fsi = GetFsi();
                if (null != fsi)
                {
                    //=> fsi Settlement
                    if (fsi.StandardSpecified)
                    {
                        _settlementChain[PayerReceiverEnum.Receiver].SiMode = SiModeEnum.Fessi;
                        _settlementChain[PayerReceiverEnum.Payer].SiMode = SiModeEnum.Fessi;
                    }
                    else if (fsi.InstructionSpecified)
                    {
                        _settlementChain[PayerReceiverEnum.Receiver].SiMode = SiModeEnum.Fsi;
                        _settlementChain[PayerReceiverEnum.Payer].SiMode = SiModeEnum.Fsi;
                    }
                }
            }
            #endregion

            #region Is DefaultBookEntry
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Receiver].SiMode)
            {
                if (IsBookEntry())
                {
                    _settlementChain[PayerReceiverEnum.Receiver].SiMode = SiModeEnum.Dbsi;
                }
            }
            #endregion

            #region Default : Receiver is Dsi
            // Il n'existe pas de OSI instruction ni de FSI instruction, ni Book-Entry de detecté => Search Default Standing Settlement					
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Receiver].SiMode)
                _settlementChain[PayerReceiverEnum.Receiver].SiMode = SiModeEnum.Dessi;
            #endregion

            #region Find siReceiver candidate
            if (_settlementChain[PayerReceiverEnum.Receiver].SiMode == SiModeEnum.Osi)
            {
                //Set Receiver OSI Settlement Instruction
                siReceiver = osiRec.CreateEfsSettlementInstructions(osiRec.SettlementInformation.Instruction);
            }
            else if (_settlementChain[PayerReceiverEnum.Receiver].SiMode == SiModeEnum.Fsi)
            {
                //Set Receiver FSI Settlement Instruction
                ISettlementChain sc = _tradeLibrary.CurrentTrade.CreateSettlementChain();

                SettlementTools.SetSettlementInstruction(cs, sc, fsi.Instruction, ((IProductBase)_tradeLibrary.Product).CreateRoutingCreateElement());
                siReceiver = fsi.Instruction.CreateEfsSettlementInstructions(sc[PayerReceiverEnum.Receiver].SettlementInstruction);
            }
            else if (_settlementChain[PayerReceiverEnum.Receiver].SiMode == SiModeEnum.Dbsi)
            {
                //
                if (null == siReceiver)
                {
                    siReceiver = _settlementChain.CreateEfsSettlementInstructions();
                    SettlementTools.SetCssBookEntry(cs, siReceiver[0]);
                    SettlementTools.SetSiDefaultBeneficiaryAndBeneficiaryBank(cs, siReceiver[0], _sqlEvent, _tradeLibrary.DataDocument, PayerReceiverEnum.Receiver, pIdLog);
                }
            }
            else if (SettlementTools.IsStanding(_settlementChain[PayerReceiverEnum.Receiver].SiMode))
            {
                //Set Receiver DSI Settlement Instruction
                ICssCriteria cssCriteria = GetCssCriteriaFromSettlementInfo(osiRec);
                ISsiCriteria ssiCriteria = GetSsiCriteriaFromSettlementInfo(osiRec);
                //
                _settlementChain[PayerReceiverEnum.Receiver].SiMode = LoadStandingInstructionCandidate(
                    PayerReceiverEnum.Receiver,
                    _settlementChain[PayerReceiverEnum.Receiver].IdASettlementOffice,
                    GetDefaultCashSecuritiesEnum(),
                    cssCriteria, ssiCriteria, ref siReceiver, _settlementChain[PayerReceiverEnum.Receiver].SiMode);
            }
            #endregion Set siReceiver

            //Payer
            #region Is Payer Osi
            ISettlementInputInfo osiPay = null;
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Payer].SiMode)
            {
                osiPay = GetOsi(PayerReceiverEnum.Payer);
                if (null != osiPay)
                {
                    if (osiPay.SettlementInformation.StandardSpecified)
                        _settlementChain[PayerReceiverEnum.Payer].SiMode = SiModeEnum.Oessi;
                    else if (osiPay.SettlementInformation.InstructionSpecified)
                        _settlementChain[PayerReceiverEnum.Payer].SiMode = SiModeEnum.Osi;
                }
            }
            #endregion Is Payer Osi
            //Il n'existe pas de OSI instruction pour le payer et Book Entry a été détecté 
            //=> Book-Entry aussi côté Payer 
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Payer].SiMode)
            {
                if (SiModeEnum.Dbsi == _settlementChain[PayerReceiverEnum.Receiver].SiMode)
                    _settlementChain[PayerReceiverEnum.Payer].SiMode = SiModeEnum.Dbsi;
            }

            #region Default : Payer is Dsi
            // Il n'existe pas de OSI instruction => Search Default Standind Settlement					
            if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Payer].SiMode)
                _settlementChain[PayerReceiverEnum.Payer].SiMode = SiModeEnum.Dessi;
            #endregion Default : Payer is Dsi

            #region Find siPayer candidate
            if (_settlementChain[PayerReceiverEnum.Payer].SiMode == SiModeEnum.Osi)
            {
                //Set Payer OSI Settlement Instruction
                siPayer = osiPay.CreateEfsSettlementInstructions(osiPay.SettlementInformation.Instruction);
            }
            else if (_settlementChain[PayerReceiverEnum.Payer].SiMode == SiModeEnum.Fsi)
            {
                ISettlementChain si = _tradeLibrary.CurrentTrade.CreateSettlementChain();
                SettlementTools.SetSettlementInstruction(cs, si, fsi.Instruction, ((IProductBase)_tradeLibrary.Product).CreateRoutingCreateElement());
                siPayer = fsi.Instruction.CreateEfsSettlementInstructions(si[PayerReceiverEnum.Payer].SettlementInstruction);
            }
            else if (_settlementChain[PayerReceiverEnum.Payer].SiMode == SiModeEnum.Dbsi)
            {
                siPayer = _settlementChain.CreateEfsSettlementInstructions();
                SettlementTools.SetCssBookEntry(cs, siPayer[0]);
                SettlementTools.SetSiDefaultBeneficiaryAndBeneficiaryBank(cs, siPayer[0], _sqlEvent, _tradeLibrary.DataDocument, PayerReceiverEnum.Payer, pIdLog);
            }
            else if (SettlementTools.IsStanding(_settlementChain[PayerReceiverEnum.Payer].SiMode))
            {
                ICssCriteria cssCriteria = GetCssCriteriaFromSettlementInfo(osiPay);
                ISsiCriteria ssiCriteria = GetSsiCriteriaFromSettlementInfo(osiPay);
                //
                _settlementChain[PayerReceiverEnum.Payer].SiMode =
                    LoadStandingInstructionCandidate(
                    PayerReceiverEnum.Payer,
                    _settlementChain[PayerReceiverEnum.Payer].IdASettlementOffice,
                    GetDefaultCashSecuritiesEnum(),
                    cssCriteria, ssiCriteria, ref siPayer, _settlementChain[PayerReceiverEnum.Payer].SiMode);
            }
            #endregion Set siPayer

            #region Load siReceiver and siPayer
            bool isOk = SetSettlementInstructionCompatible(siReceiver, siPayer, out int firstRecIdCssNotCompatible, out int firstPayIdCssNotCompatible);

            if (isOk)
            {
                //Region cas Particulier 
                // Si le Css est de type FrancoOfPayment et que l'évènement est un évènment Cash, alors on recherche des Standing SI de type Cash  
                if (_productContainer.IsSecurityTransaction && (false == EventTypeFunc.IsQuantity(_sqlEvent.EventType)))
                {
                    IEfsSettlementInstruction si = _settlementChain[PayerReceiverEnum.Receiver].SettlementInstruction;
                    SQL_Css sqlCss = SettlementTools.GetSqlCss(cs, si);
                    if (null == sqlCss)
                    {
                        si = _settlementChain[PayerReceiverEnum.Payer].SettlementInstruction;
                        sqlCss = SettlementTools.GetSqlCss(cs, si);
                    }
                    if ((null != sqlCss) && ("FOP" == sqlCss.SettlementType))
                    {
                        //reciever
                        ICssCriteria cssCriteria = GetCssCriteriaFromSettlementInfo(osiRec);
                        ISsiCriteria ssiCriteria = GetSsiCriteriaFromSettlementInfo(osiRec);
                        //
                        _settlementChain[PayerReceiverEnum.Receiver].SiMode = LoadStandingInstructionCandidate(
                        PayerReceiverEnum.Receiver,
                        _settlementChain[PayerReceiverEnum.Receiver].IdASettlementOffice,
                        CashSecuritiesEnum.CASH,
                        cssCriteria, ssiCriteria, ref siReceiver, _settlementChain[PayerReceiverEnum.Receiver].SiMode);
                        //
                        //Payer
                        cssCriteria = GetCssCriteriaFromSettlementInfo(osiPay);
                        ssiCriteria = GetSsiCriteriaFromSettlementInfo(osiPay);
                        //
                        _settlementChain[PayerReceiverEnum.Payer].SiMode =
                        LoadStandingInstructionCandidate(
                        PayerReceiverEnum.Payer,
                        _settlementChain[PayerReceiverEnum.Payer].IdASettlementOffice,
                        CashSecuritiesEnum.CASH,
                        cssCriteria, ssiCriteria, ref siPayer, _settlementChain[PayerReceiverEnum.Payer].SiMode);
                        //    
                        isOk = SetSettlementInstructionCompatible(siReceiver, siPayer, out firstRecIdCssNotCompatible, out firstPayIdCssNotCompatible);
                    }
                }
            }
            //
            if (isOk)
            {
                SettlementTools.SetSiDefaultBeneficiaryAndBeneficiaryBank(cs, _settlementChain[PayerReceiverEnum.Receiver].SettlementInstruction, _sqlEvent, _tradeLibrary.DataDocument, PayerReceiverEnum.Receiver, pIdLog);
                SettlementTools.SetSiDefaultBeneficiaryAndBeneficiaryBank(cs, _settlementChain[PayerReceiverEnum.Payer].SettlementInstruction, _sqlEvent, _tradeLibrary.DataDocument, PayerReceiverEnum.Payer, pIdLog);
            }
            #endregion Load siReceiver and siPayer


            if (!isOk)
            {
                string payer = string.Empty;
                string receiver = string.Empty;

                DataParameter paramIdE = new DataParameter(cs, "IDE", DbType.Int32)
                {
                    Value = _sqlEvent.Id
                };

                StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT + @"e.IDE," + Cst.CrLf);
                sqlQuery += "pay.IDENTIFIER as PAYER, rec.IDENTIFIER as RECEIVER" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR + " pay on pay.IDA=e.IDA_PAY" + Cst.CrLf;
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR + " rec on rec.IDA=e.IDA_REC" + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + @"(e.IDE=@IDE)" + Cst.CrLf;

                using (IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, sqlQuery.ToString(), paramIdE.DbDataParameter))
                {
                    if (dr.Read())
                    {
                        payer = dr["PAYER"].ToString();
                        receiver = dr["RECEIVER"].ToString();
                    }
                }

                ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);

                if (ArrFunc.IsEmpty(siReceiver) && ArrFunc.IsEmpty(siPayer))
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03402", processState,
                            LogTools.IdentifierAndId(_sqlEvent.EventCode + "-" + _sqlEvent.EventType, _sqlEvent.Id),
                            _sqlEvent.Unit,
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtStartAdjusted),
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtEndAdjusted),
                            LogTools.IdentifierAndId(payer, _sqlEvent.IdAPayer),
                            _settlementChain[PayerReceiverEnum.Payer].SiMode.ToString(),
                            LogTools.IdentifierAndId(receiver, _sqlEvent.IdAReceiver),
                            _settlementChain[PayerReceiverEnum.Receiver].SiMode.ToString());
                }
                else if (ArrFunc.IsEmpty(siPayer))
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03403", processState,
                            LogTools.IdentifierAndId(_sqlEvent.EventCode + "-" + _sqlEvent.EventType, _sqlEvent.Id),
                            _sqlEvent.Unit,
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtStartAdjusted),
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtEndAdjusted),
                            LogTools.IdentifierAndId(payer, _sqlEvent.IdAPayer),
                            _settlementChain[PayerReceiverEnum.Payer].SiMode.ToString());
                }
                else if (ArrFunc.IsEmpty(siReceiver))
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 1, "SYS-03404", processState,
                            LogTools.IdentifierAndId(_sqlEvent.EventCode + "-" + _sqlEvent.EventType, _sqlEvent.Id),
                            _sqlEvent.Unit,
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtStartAdjusted),
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtEndAdjusted),
                            LogTools.IdentifierAndId(receiver, _sqlEvent.IdAReceiver),
                            _settlementChain[PayerReceiverEnum.Receiver].SiMode.ToString());
                }
                else
                {
                    processState.CodeReturn = ProcessStateTools.CodeReturnDataUnMatchEnum;
                    SQL_Css sql_CssPay = new SQL_Css(cs, firstPayIdCssNotCompatible, SQL_Table.ScanDataDtEnabledEnum.No);
                    sql_CssPay.LoadTable(new string[] { "IDENTIFIER" });
                    SQL_Css sql_CssRec = new SQL_Css(cs, firstRecIdCssNotCompatible, SQL_Table.ScanDataDtEnabledEnum.No);
                    sql_CssRec.LoadTable(new string[] { "IDENTIFIER" });

                    string cssPay = string.Empty;
                    if (sql_CssPay.IsLoaded)
                        cssPay = sql_CssPay.Identifier;
                    string cssRec = string.Empty;
                    if (sql_CssRec.IsLoaded)
                        cssRec = sql_CssRec.Identifier;

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-03405", processState,
                            LogTools.IdentifierAndId(_sqlEvent.EventCode + "-" + _sqlEvent.EventType, _sqlEvent.Id),
                            _sqlEvent.Unit,
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtStartAdjusted),
                            DtFunc.DateTimeToStringDateISO(_sqlEvent.DtEndAdjusted),
                            LogTools.IdentifierAndId(payer, _sqlEvent.IdAPayer), cssPay,
                            _settlementChain[PayerReceiverEnum.Payer].SiMode.ToString(),
                            LogTools.IdentifierAndId(receiver, _sqlEvent.IdAReceiver), cssRec);
                }
            }

            return isOk;
        }

        /// <summary>
        /// Recherche des instructions CASH ou SECURITIES
        /// <para>
        /// 
        /// </para>
        /// </summary>
        /// <returns></returns>
        private CashSecuritiesEnum GetDefaultCashSecuritiesEnum()
        {

            CashSecuritiesEnum ret = CashSecuritiesEnum.CASH;
            if (false == EventCodeFunc.IsOtherPartyPayment(_sqlEvent.EventCode))
            {
                if (_productContainer.IsSecurityTransaction)
                    ret = CashSecuritiesEnum.SECURITIES;
            }
            return ret;

        }

        /// <summary>
        /// Retourne une l'instruction de RL FpML (type ISettlementInformation) défini spécifiquement sur un fpML:payment
        /// </summary>
        /// <returns></returns>
        /// FI 20161114 [RATP] Modify
        private ISettlementInformation GetFsi()
        {
            ISettlementInformation settlementInformation = null;

            IProduct product = (IProduct)ReflectionTools.GetObjectById(_tradeLibrary.CurrentTrade, Cst.FpML_InstrumentNo + _sqlEvent.InstrumentNo.ToString());
            if (null != product)
            {
                if (product.ProductBase.IsBulletPayment)
                {
                    IBulletPayment bulletPayment = (IBulletPayment)product;
                    if (EventCodeFunc.IsStart(_sqlEvent.EventCode) &&
                        (bulletPayment.Payment.GetPaymentType(_sqlEvent.EventCode) == _sqlEvent.EventType))
                        settlementInformation = bulletPayment.Payment.SettlementInformation;
                }
                else if (product.ProductBase.IsCapFloor)
                {
                    ICapFloor capFloor = (ICapFloor)product;
                    if (EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode) ||
                        (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) && EventTypeFunc.IsPremium(_sqlEvent.EventType)))
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                        {
                            if (EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode) && capFloor.AdditionalPaymentSpecified)
                                settlementInformation = GetSettlementInformationPayment(capFloor.AdditionalPayment, item);
                            else if (EventTypeFunc.IsPremium(_sqlEvent.EventType) && capFloor.PremiumSpecified)
                                settlementInformation = GetSettlementInformationPayment(capFloor.Premium, item);
                        }
                    }
                }
                else if (product.ProductBase.IsFxAverageRateOption)
                {
                    IFxAverageRateOption fxAverageRateOption = (IFxAverageRateOption)product;
                    if (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsPremium(_sqlEvent.EventType) &&
                        fxAverageRateOption.FxOptionPremiumSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = fxAverageRateOption.FxOptionPremium[item].SettlementInformation;
                    }
                }
                else if (product.ProductBase.IsFxBarrierOption)
                {
                    IFxBarrierOption fxBarrierOption = (IFxBarrierOption)product;
                    if (EventCodeFunc.IsTermination(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsPayout(_sqlEvent.EventType) &&
                        fxBarrierOption.TriggerPayout.SettlementInformationSpecified)
                        settlementInformation = fxBarrierOption.TriggerPayout.SettlementInformation;
                    else if (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsPremium(_sqlEvent.EventType) &&
                        fxBarrierOption.FxOptionPremiumSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = fxBarrierOption.FxOptionPremium[item].SettlementInformation;
                    }
                }
                else if (product.ProductBase.IsFxDigitalOption)
                {
                    IFxDigitalOption fxDigitalOption = (IFxDigitalOption)product;

                    if (EventCodeFunc.IsTermination(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsPayout(_sqlEvent.EventType))
                        settlementInformation = fxDigitalOption.TriggerPayout.SettlementInformation;
                    else if (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) &&
                             EventTypeFunc.IsPremium(_sqlEvent.EventType) &&
                             fxDigitalOption.FxOptionPremiumSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = fxDigitalOption.FxOptionPremium[item].SettlementInformation;
                    }
                }
                else if (product.ProductBase.IsFxLeg)
                {
                    IFxLeg fxLeg = (IFxLeg)product;
                    if (EventCodeFunc.IsTermination(_sqlEvent.EventCode))
                    {
                        if (EventTypeFunc.IsCurrency1(_sqlEvent.EventType))
                            settlementInformation = fxLeg.ExchangedCurrency1.SettlementInformation;
                        else if (EventTypeFunc.IsCurrency2(_sqlEvent.EventType))
                            settlementInformation = fxLeg.ExchangedCurrency2.SettlementInformation;
                    }
                }
                else if (product.ProductBase.IsFxOptionLeg)
                {
                    IFxOptionLeg fxOptionLeg = (IFxOptionLeg)product;
                    if (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsPremium(_sqlEvent.EventType) &&
                        fxOptionLeg.FxOptionPremiumSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = fxOptionLeg.FxOptionPremium[item].SettlementInformation;
                    }
                }
                else if (product.ProductBase.IsFxSwap)
                {
                    IFxSwap fxSwap = (IFxSwap)product;
                    if (EventCodeFunc.IsTermination(_sqlEvent.EventCode))
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                        {
                            if (EventTypeFunc.IsCurrency1(_sqlEvent.EventType))
                                settlementInformation = fxSwap.FxSingleLeg[item].ExchangedCurrency1.SettlementInformation;
                            else if (EventTypeFunc.IsCurrency2(_sqlEvent.EventType))
                                settlementInformation = fxSwap.FxSingleLeg[item].ExchangedCurrency2.SettlementInformation;
                        }
                    }
                }
                else if (product.ProductBase.IsSwap)
                {
                    ISwap swap = (ISwap)product;
                    if (EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode) && swap.AdditionalPaymentSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = GetSettlementInformationPayment(swap.AdditionalPayment, item);
                    }
                }
                else if (product.ProductBase.IsSwaption)
                {
                    ISwaption swaption = (ISwaption)product;
                    if (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsPremium(_sqlEvent.EventType) &&
                        swaption.PremiumSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = GetSettlementInformationPayment(swaption.Premium, item);
                    }
                }
                else if (product.ProductBase.IsFxTermDeposit)
                {
                    ITermDeposit termDeposit = (ITermDeposit)product;
                    if (EventCodeFunc.IsTermination(_sqlEvent.EventCode) && termDeposit.PaymentSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = GetSettlementInformationPayment(termDeposit.Payment, item);
                    }
                }
                else if (product.ProductBase.IsLoanDeposit)
                {
                    ILoanDeposit loadDeposit = (ILoanDeposit)product;
                    if (EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode) && loadDeposit.AdditionalPaymentSpecified)
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0)
                            settlementInformation = GetSettlementInformationPayment(loadDeposit.AdditionalPayment, item);
                    }
                }
                else if (product.ProductBase.IsFra)
                {
                    //pas d'instruction sur les fras
                }
                else if (product.ProductBase.IsSecurityTransaction)
                {
                    //on néglige settlementInformation, ils doivent être invisible sur les écrans
                }
                else if (product.ProductBase.IsEquitySecurityTransaction) // FI 20161114 [RATP] Modify
                {
                    IEquitySecurityTransaction equitySecurityTransaction = (IEquitySecurityTransaction)product;
                    if (EventCodeFunc.IsLinkedProductPayment(_sqlEvent.EventCode) &&
                        EventTypeFunc.IsGrossAmount(_sqlEvent.EventType))
                    {
                        int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                        if (item >= 0 && equitySecurityTransaction.GrossAmount.SettlementInformationSpecified)
                            settlementInformation = equitySecurityTransaction.GrossAmount.SettlementInformation;
                    }
                }
                else if (product.ProductBase.IsInvoice || product.ProductBase.IsAdditionalInvoice || product.ProductBase.IsCreditNote)
                {
                    //20090826 PL Test Add for EPL
                    //on néglige settlementInformation, ils doivent être invisible sur les écrans
                }
                else if (product.ProductBase.IsInvoiceSettlement)
                {
                    //20090826 PL Test 
                    //on néglige settlementInformation, ils doivent être invisible sur les écrans
                }
                else if (product.ProductBase.IsLSD)
                {
                    //RD 20100512 Pour les LSD donc les product ExchangeTradeDerivative et STGexchangeTradeDerivative
                    //pas d'instructions sur les LSD
                }
                else if (product.ProductBase.IsRISK)
                {
                    //RD 20111227 Pour les Trades Risk donc les product MarginRequirement et CashBalance
                    //pas d'instructions sur les Trades Risk
                }
                else
                {
                    throw new Exception("Error, Current product is not managed, please contact EFS");
                }
            }
            // cas OtherPartyPayment			
            if ((null == settlementInformation) && EventCodeFunc.IsOtherPartyPayment(_sqlEvent.EventCode) &&
                _tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified)
            {
                int item = EventRDBMSTools.IndexOf(cs, _sqlEvent);
                settlementInformation = GetSettlementInformationPayment((IPayment[])_tradeLibrary.CurrentTrade.OtherPartyPayment, item);
            }

            return settlementInformation;
        }

        /// <summary>
        /// Generation des instructions de RL compatibles à partir des Internal Standing SI(ISSI) [=> SSI stockée dans la base Spheres OTCML]
        /// <para> Considération des directives spécifiées via ICssCriteria et ISsiCriteria</para>
        /// <para> ICssCriteria donne des indications pour obtenir le bon css</para>
        /// <para> ISsiCriteria donne des indications pour retenir la bonne instructions (ISSI)</para>
        /// </summary>
        private IEfsSettlementInstruction[] GetIssis(int pIdAStlOffice, PayerReceiverEnum pPayerReceiver, CashSecuritiesEnum pCashSecuritiesEnum, ICssCriteria pCssCriteria, ISsiCriteria pSsiCriteria)
        {

            IEfsSettlementInstruction[] ret = null;
            //Recherche des ISSIS du settlement office 
            IssisContext issiContext = new IssisContext(_sqlEvent, pPayerReceiver, pSsiCriteria, _tradeLibrary.DataDocument);
            // RD 20100106 [16766] Disabled SSI not ignored / Use DtEventForced like reference to ignore it.
            issiContext.LoadIssiCollection(cs, pCssCriteria, pIdAStlOffice, pCashSecuritiesEnum, _stlDtEventForced);
            issiContext.Sort();

            Issis issis = new Issis
            {
                issi = issiContext.Issi
            };

            ArrayList siList = new ArrayList();
            IEfsSettlementInstruction si = null;
            for (int i = 0; i < issis.Count; i++)
            {
                int idIssi = issis[i].idIssi;
                IssiItem[] issiItem = SettlementTools.GetIssiItems(cs, issis[i].idIssi);
                if (ArrFunc.IsFilled(issiItem))
                {
                    IIssiItemsRoutingActorsInfo issiItems = (IIssiItemsRoutingActorsInfo)_tradeLibrary.CurrentTrade.CreateIssiItemsRoutingActorsInfo(cs, issis[i].idIssi, issiItem);
                    si = issiItems.GetInstruction(pPayerReceiver, EventTypeFunc.IsQuantity(_sqlEvent.EventType));
                    if (null != si)
                    {
                        si.IdIssi = idIssi;
                        siList.Add(si);
                    }
                }
            }
            //
            if (null != si)
                ret = (IEfsSettlementInstruction[])siList.ToArray(si.GetType());
            //
            return ret;
        }

        /// <summary>
        ///  Recherche le contexte parmi currentTrade.settlementInput qui matche le mieux avec l'évènement en cours parmi. 
        ///  Retourne la directive associée (ISettlementInputInfo)
        /// </summary>
        /// <param name="pPayerReceiver"></param>
        /// <returns></returns>
        private ISettlementInputInfo GetOsi(PayerReceiverEnum pPayerReceiver )
        {
            ISettlementInputInfo ret = null;
            IFlowContext retContext = null;
            //
            ISettlementInput[] settlementInput = _tradeLibrary.CurrentTrade.SettlementInput;
            //
            if (ArrFunc.IsFilled(settlementInput))
            {
                #region eventCodesSchedule Match
                for (int i = 0; i < settlementInput.Length; i++)
                {
                    FlowContextContainer settlementContext = new FlowContextContainer(settlementInput[i].SettlementContext);
                    //
                    if (settlementContext.IsMatchWith(_sqlEvent, pPayerReceiver, _tradeLibrary.DataDocument))
                    {
                        //On clone ce qui existe dans le trade pour ne pas modifier le trade de origine 
                        ISettlementInput settlementInputItem = (ISettlementInput)settlementInput[i].Clone();
                        //
                        FlowContextContainer settlementContextItem = new FlowContextContainer(settlementInputItem.SettlementContext);
                        settlementContextItem.CleanUpNoMatchWith(_sqlEvent, pPayerReceiver, _tradeLibrary.DataDocument);
                        //
                        //settlementInput.settlementContext.CleanUpNoMatchWith(_sqlEvent, _tradeLibrary.dataDocument, pPayerReceiver);
                        //_productContainer.CleanUpNoMatchWith(settlementInput.settlementContext, pPayerReceiver, _sqlEvent);
                        if (null == ret)
                        {
                            ret = settlementInputItem.SettlementInputInfo;
                            retContext = settlementInputItem.SettlementContext;
                        }
                        else
                        {
                            if (((IComparable)settlementInputItem.SettlementContext).CompareTo(retContext) > 0)
                            {
                                ret = settlementInputItem.SettlementInputInfo;
                                retContext = settlementInputItem.SettlementContext;
                            }
                        }
                    }
                }
                #endregion
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payments"></param>
        /// <param name="pItem"></param>
        /// <returns></returns>
        private ISettlementInformation GetSettlementInformationPayment(IPayment[] payments, int pItem)
        {
            ISettlementInformation ret = null;

            IPayment payment = null;
            int j = -1;
            for (int i = 0; i < payments.Length; i++)
            {
                if (payments[i].GetPaymentType(_sqlEvent.EventCode) == _sqlEvent.EventType)
                    j++;
                if (j == pItem)
                {
                    payment = payments[i];
                    break;
                }
            }

            if ((null != payment) && (payment.SettlementInformationSpecified))
                ret = payment.SettlementInformation;

            return ret;
        }

        /// <summary>
        ///  book where IDA_ENTITY >0 And there is no specific SSI
        /// </summary>
        /// <remarks>
        /// 20090610 FI [16157] Gestion des dérogations
        /// Le settlement office considéré n'est pas celui spécifié sur le book
        /// mais celui détecté classiquement à partir du book ou de l'acteur
        /// Il est déjà alimenté dans les properties 
        /// _settlementChain[PayerReceiverEnum.Receiver].idASettlementOffice et
        /// _settlementChain[PayerReceiverEnum.Payer].idASettlementOffice 
        /// 
        /// </remarks>
        /// <returns></returns>
        private bool IsBookEntry()
        {

            bool ret = false;
            if ((null != _sqlEvent) && (_sqlEvent.GetIdBook(PayerReceiverEnum.Receiver) > 0) && (_sqlEvent.GetIdBook(PayerReceiverEnum.Payer) > 0))
            {
                int idaReceiverStlOffice = _settlementChain[PayerReceiverEnum.Receiver].IdASettlementOffice;
                int idaPayerStlOffice = _settlementChain[PayerReceiverEnum.Payer].IdASettlementOffice;

                SQL_Book sqlbookReceiver = new SQL_Book(cs, _sqlEvent.GetIdBook(PayerReceiverEnum.Receiver));
                SQL_Book sqlbookPayer = new SQL_Book(cs, _sqlEvent.GetIdBook(PayerReceiverEnum.Payer));
                //
                if (sqlbookReceiver.LoadTable(new string[] { "IDB,IDA_ENTITY" }) && 
                        sqlbookPayer.LoadTable(new string[] { "IDB,IDA_ENTITY" }))
                {
                    int idAEntity = 0;
                    IEfsSettlementInstruction[] si = null;
                    //
                    ret = (sqlbookReceiver.IdA_Entity == sqlbookPayer.IdA_Entity && (sqlbookReceiver.IdA_Entity > 0));
                    if (ret)
                        idAEntity = sqlbookReceiver.IdA_Entity;
                    //
                    //BOOK-ENTRY uniquement si l'entité est teneur de compte
                    //Cas de Spheres installé chez un courtier
                    //HPC n'est pas teneur de compte, ses comptes sont chez SG
                    //Il n'y aura jamais de BOOK-ENTRY 
                    ret = ret && ActorTools.IsActorWithRole(cs, idAEntity, RoleActor.ACCOUNTSERVICER);
                    SiModeEnum siMode;
                    //
                    if (ret)
                    {
                        si = null;
                        siMode = SiModeEnum.Undefined;
                        //Receiver  
                        if (Book.BookTools.IsCounterPartyClient(cs, _sqlEvent.IdAReceiver, _sqlEvent.IdBReceiver)) //suis-je côté client ?? 
                        {
                            /// 20090612 FI/PL [16157] Derogations
                            // Cas des dérogations
                            // Lorsque le receiver est client, qu'il possède un "settlement office" différent du settlement office de la contrepartie du flux
                            // S'il existe des SSIs spécifiées qui matchent avec le contexte alors alors le règlement n'est plus en Book-Entry
                            //if (idaReceiverStlOffice > 0 && idaReceiverStlOffice != idAEntity)
                            if (idaReceiverStlOffice > 0 && idaReceiverStlOffice != idaPayerStlOffice)
                            {
                                //Exists specifics SSI Client ? BookEntry only if not Exist specifics SSI Client
                                LoadStandingInstructionCandidate(PayerReceiverEnum.Receiver, idaReceiverStlOffice, GetDefaultCashSecuritiesEnum(), null, null, ref si, siMode);
                                ret = (null == si);
                            }
                        }
                    }
                    //
                    if (ret)
                    {
                        si = null;
                        siMode = SiModeEnum.Undefined;
                        //Payer
                        if (Book.BookTools.IsCounterPartyClient(cs, _sqlEvent.IdAPayer, _sqlEvent.IdBPayer)) //suis-je côté client ?? 
                        {
                            /// 20090612 FI/PL [16157] Derogations
                            // Cas des dérogations
                            // Lorsque le payer est client, qu'il possède un "settlement office" différent du settlement office de la contrepartie du flux
                            // S'il existe des SSIs spécifiées qui matchent avec le contexte alors le règlement n'est plus en Book-Entry
                            // 
                            //if (idaPayerStlOffice > 0 && idaPayerStlOffice != idAEntity)
                            if (idaPayerStlOffice > 0 && idaPayerStlOffice != idaReceiverStlOffice)
                            {
                                //Exists specifics SSI Client ? BookEntry only if not Exist specifics SSI Client
                                LoadStandingInstructionCandidate(PayerReceiverEnum.Payer, idaPayerStlOffice, GetDefaultCashSecuritiesEnum(), null, null, ref si, siMode);
                                ret = (null == si);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Recherche des standing instructions 
        /// <para>Elles peuvent être définies sur une base externe ou dans la base Spheres (table ISSI)</para>
        /// </summary>
        /// <param name="pPayRec"></param>
        /// <param name="pIdAStlOffice"></param>
        /// <param name="pCashSecuritiesEnum"></param>
        /// <param name="pCssCriteria"></param>
        /// <param name="pSsiCriteria"></param>
        /// <param name="pSsis"></param>
        /// <param name="pSiMode"></param>
        /// <returns></returns>
        private SiModeEnum LoadStandingInstructionCandidate(
            PayerReceiverEnum pPayRec, int pIdAStlOffice, CashSecuritiesEnum pCashSecuritiesEnum,
            ICssCriteria pCssCriteria, ISsiCriteria pSsiCriteria,
            ref  IEfsSettlementInstruction[] pSsis, SiModeEnum pSiMode)
        {
            SiModeEnum siMode = pSiMode;  // Default

            //Search potentially differents ssi (internal Or external) for settlementChain[pPayRec]
            IEfsSettlementInstruction[] ssis = null;
            if (0 < pIdAStlOffice)
            {
                //Load ssiDatabase for each database
                SsiDbs ssidbs = new SsiDbs();
                ssidbs.InitializeActorSsiDb(cs, pIdAStlOffice);
                //
                if (0 < ssidbs.Count)
                {
                    ssidbs.Sort();
                    for (int j = 0; j < ssidbs.Count; j++)
                    {
                        if (ssidbs[j].IsLocalDatabase)
                        {
                            ssis = GetIssis(pIdAStlOffice, pPayRec, pCashSecuritiesEnum, pCssCriteria, pSsiCriteria);
                            if (null != ssis)
                            {
                                siMode = SettlementTools.GetInternalModeSI(siMode);
                                for (int k = 0; k < ssis.Length; k++)
                                    ssis[k].IdssiDb = ssidbs[j].idssiDb;
                            }
                        }
                        else
                            ssis = GetESsi();
                        // if SSI is Find then break search; 
                        if (null != ssis)
                            break;
                    }
                }
                else
                {
                    ssis = GetIssis(pIdAStlOffice, pPayRec, pCashSecuritiesEnum, pCssCriteria, pSsiCriteria);
                    if (null != ssis)
                        siMode = SettlementTools.GetInternalModeSI(siMode);
                }
            }
            pSsis = ssis;
            return siMode;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSiReceiver"></param>
        /// <param name="pSiPayer"></param>
        /// <param name="pFirstRecIdCssNotCompatible"></param>
        /// <param name="pFirstPayIdCssNotCompatible"></param>
        /// <returns></returns>
        private bool SetSettlementInstructionCompatible(IEfsSettlementInstruction[] pSiReceiver, IEfsSettlementInstruction[] pSiPayer, out  int pFirstRecIdCssNotCompatible, out int pFirstPayIdCssNotCompatible)
        {

            pFirstRecIdCssNotCompatible = 0;
            pFirstPayIdCssNotCompatible = 0;
            bool isOk = false;
            for (int i = 0; i < ArrFunc.Count(pSiReceiver); i++)
            {
                for (int j = 0; j < ArrFunc.Count(pSiPayer); j++)
                {
                    int idCssRec = SettlementTools.GetIdCss(cs, (IEfsSettlementInstruction)pSiReceiver[i]);
                    int idCssPay = SettlementTools.GetIdCss(cs, (IEfsSettlementInstruction)pSiPayer[j]);
                    int linkId = 0;
                    //
                    isOk = ((idCssPay == idCssRec) || ((0 == idCssRec) && (idCssPay > 0)) || ((0 == idCssPay) && (idCssRec > 0)));
                    if (!isOk && (idCssPay > 0) && (idCssRec > 0))
                    {
                        linkId = SettlementTools.IsCssLinked(cs, idCssRec, idCssPay);
                        isOk = (linkId > 0);
                    }
                    if (isOk)
                    {
                        _settlementChain[PayerReceiverEnum.Receiver].SettlementInstruction = pSiReceiver[i];
                        _settlementChain[PayerReceiverEnum.Payer].SettlementInstruction = pSiPayer[j];
                        if (linkId > 0)
                            _settlementChain.CssLink = linkId;
                        break;
                    }
                    else
                    {
                        if (0 == (pFirstRecIdCssNotCompatible + pFirstPayIdCssNotCompatible))
                        {
                            pFirstRecIdCssNotCompatible = idCssRec;
                            pFirstPayIdCssNotCompatible = idCssPay;
                        }
                    }
                }
                if (isOk)
                    break;
            }
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSettlementInfo"></param>
        /// <returns></returns>
        private static ICssCriteria GetCssCriteriaFromSettlementInfo(ISettlementInputInfo pSettlementInfo)
        {
            ICssCriteria cssCriteria = null;
            if (null != pSettlementInfo)
            {
                if (pSettlementInfo.CssCriteriaSpecified)
                    cssCriteria = pSettlementInfo.CssCriteria;
            }
            return cssCriteria;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSettlementInfo"></param>
        /// <returns></returns>
        private static ISsiCriteria GetSsiCriteriaFromSettlementInfo(ISettlementInputInfo pSettlementInfo)
        {
            ISsiCriteria ssiCriteria = null;
            if (null != pSettlementInfo)
            {
                if (pSettlementInfo.SsiCriteriaSpecified)
                    ssiCriteria = pSettlementInfo.SsiCriteria;
            }
            return ssiCriteria;
        }

        /// <summary>
        /// Recherche des External Standing SI(ESSI) [=> SSI non stockée dans la base Spheres OTCML, OMGEO par exemple]
        /// </summary>
        /// <remarks> aucune connexion vers l'extérieur n'est aujourd'hui gérée </remarks> 
        /// <returns></returns>
        private static IEfsSettlementInstruction[] GetESsi()
        {
            //TODO After
            IEfsSettlementInstruction[] ret = null;
            return ret;
        }

        #endregion Methods
    }
}

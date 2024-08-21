#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
#endregion Using Directives

namespace EFS.Process
{

    #region EventProcess
    /// <summary>
    /// Classe chargée d'alimenter la table EVENTPROCESS
    /// </summary>
    /// IDTRK_L remplace IDREQUEST_L
    public class EventProcess
    {
        #region Members
        private readonly string m_Cs;
        private readonly string m_SQLQueryEventProcess;



        #endregion Members
        #region Constructors
        public EventProcess(string pCs)
        {
            m_Cs = pCs;

            m_SQLQueryEventProcess = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.EVENTPROCESS.ToString();
            m_SQLQueryEventProcess += "(IDE, PROCESS, IDTRK_L, IDSTPROCESS, DTSTPROCESS, EVENTCLASS, IDDATA, EXTLLINK)" + Cst.CrLf;
            m_SQLQueryEventProcess += "values (@IDE, @PROCESS, @IDTRK_L, @IDSTPROCESS, @DTSTPROCESS, @EVENTCLASS, @IDDATA, @EXTLLINK)";
        }
        #endregion Constructors
        #region Methods
        #region Write
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType,
                        ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSys, int pIdTRK, int pIdData)
        {
            return Write(pDbTransaction, pIdE, pProcessType, pStatusProcess, pDtSys, string.Empty, pIdTRK, pIdData);
        }
        //
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType,
                        ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSys, int pIdTRK)
        {
            return Write(pDbTransaction, pIdE, pProcessType, pStatusProcess, pDtSys, string.Empty, pIdTRK, 0);
        }
        
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public int Write(IDbTransaction pDbTransaction, int pIdE, Cst.ProcessTypeEnum pProcessType,
                        ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSys, string pEventClass, int pIdTRK, int pIdData)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(m_Cs, "IDE", DbType.Int32), pIdE);
            dp.Add(new DataParameter(m_Cs, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pProcessType.ToString());
            dp.Add(new DataParameter(m_Cs, "IDSTPROCESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN), pStatusProcess.ToString());
            dp.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.DTSTPROCESS), pDtSys); // FI 20201006 [XXXXX] Appel à GetParameter
            dp.Add(new DataParameter(m_Cs, "IDTRK_L", DbType.Int32), (pIdTRK > 0) ? pIdTRK : Convert.DBNull);
            dp.Add(new DataParameter(m_Cs, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), StrFunc.IsFilled(pEventClass) ? pEventClass : Convert.DBNull);
            dp.Add(new DataParameter(m_Cs, "IDDATA", DbType.Int32), (pIdData > 0) ? pIdData : Convert.DBNull);
            dp.Add(new DataParameter(m_Cs, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), Convert.DBNull);

            return DataHelper.ExecuteNonQuery(m_Cs, pDbTransaction, CommandType.Text, m_SQLQueryEventProcess, dp.GetArrayDbParameter());
        }

        // EG 20180205 [23769] Upd DataHelper.ExecuteDataSet
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataAdapter
        public static void Write(string pCs, IDbTransaction pDbTransaction, List<int> pEvents,
            Cst.ProcessTypeEnum pProcessType, ProcessStateTools.StatusEnum pStatusProcess, DateTime pDtSys,
            string pEventClass, int pIdTRK, int pIdData)
        {
            if (pEvents.Count > 0)
            {
                string sql_Select_EventProcess = SQLCst.SELECT + "IDEP, IDE, PROCESS, IDTRK_L, IDSTPROCESS, DTSTPROCESS, EVENTCLASS, IDDATA" + Cst.CrLf;
                sql_Select_EventProcess += SQLCst.FROM_DBO + "EVENTPROCESS";
                string sql_Where_EventProcess = SQLCst.WHERE + "(" + DataHelper.SQLColumnIn(pCs, "IDE", pEvents, TypeData.TypeDataEnum.integer, false, true) + ")";
                sql_Where_EventProcess += SQLCst.AND + "(PROCESS = @PROCESS)" + Cst.CrLf;
                sql_Where_EventProcess += SQLCst.ORDERBY + "DTSTPROCESS";

                DataParameters dp = new DataParameters();
                dp.Add(new DataParameter(pCs, "PROCESS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pProcessType.ToString());
                
                DataSet dsEventProcess = DataHelper.ExecuteDataset(pCs, pDbTransaction, CommandType.Text,
                        sql_Select_EventProcess + sql_Where_EventProcess, dp.GetArrayDbParameter());

                DataTable dtEventProcess = dsEventProcess.Tables[0];

                foreach (int ide in pEvents)
                {
                    DataRow[] drEventProcess = dtEventProcess.Select("IDE = " + ide);
                    int count = drEventProcess.Length;

                    if (count > 0)
                    {
                        // Mettre à jour la dérnière ligne
                        drEventProcess[count - 1].BeginEdit();
                        drEventProcess[count - 1]["IDTRK_L"] = (pIdTRK > 0) ? pIdTRK : Convert.DBNull;
                        drEventProcess[count - 1]["IDSTPROCESS"] = pStatusProcess.ToString();
                        drEventProcess[count - 1]["DTSTPROCESS"] = pDtSys;
                        drEventProcess[count - 1]["EVENTCLASS"] = StrFunc.IsFilled(pEventClass) ? pEventClass : Convert.DBNull;
                        drEventProcess[count - 1]["IDDATA"] = (pIdData > 0) ? pIdData : Convert.DBNull;
                        drEventProcess[count - 1].EndEdit();
                    }
                    else
                    {
                        DataRow newRow = dtEventProcess.NewRow();
                        newRow.BeginEdit();
                        newRow["IDE"] = ide;
                        newRow["PROCESS"] = pProcessType.ToString();
                        newRow["IDTRK_L"] = (pIdTRK > 0) ? pIdTRK : Convert.DBNull;
                        newRow["IDSTPROCESS"] = pStatusProcess.ToString();
                        newRow["DTSTPROCESS"] = pDtSys;
                        newRow["EVENTCLASS"] = StrFunc.IsFilled(pEventClass) ? pEventClass : Convert.DBNull;
                        newRow["IDDATA"] = (pIdData > 0) ? pIdData : Convert.DBNull;
                        newRow.EndEdit();
                        dtEventProcess.Rows.Add(newRow);
                    }
                }

                _ = DataHelper.ExecuteDataAdapter(pCs, pDbTransaction, sql_Select_EventProcess, dtEventProcess);
            }
        }
        #endregion Write
        #endregion Methods
    }
    #endregion EventProcess

    #region DataRowEvent
    public class DataRowEvent
    {
        #region Members
        private readonly DataRow _rowEvent;
        #endregion Members
        #region Accessors
        #region rowEvent
        public DataRow RowEvent
        {
            get { return _rowEvent; }
        }
        #endregion
        #region id
        public int Id
        {
            get
            {
                return Convert.ToInt32(_rowEvent["IDE"]);
            }
        }
        #endregion
        #region idParentEvent
        public int IdParentEvent
        {
            get
            {
                return Convert.ToInt32(_rowEvent["IDE_EVENT"]);
            }
        }
        #endregion
        #region idParent
        public int IdParent
        {
            get
            {
                return Convert.ToInt32(_rowEvent["IDT"]);
            }
        }
        #endregion

        #region instrumentNo
        public int InstrumentNo
        {
            get
            {
                return Convert.ToInt32(_rowEvent["INSTRUMENTNO"]);
            }
        }
        #endregion
        #region streamNo
        public int StreamNo
        {
            get
            {
                return Convert.ToInt32(_rowEvent["STREAMNO"]);
            }
        }
        #endregion

        #region idA_Pay
        public Nullable<Int32> IdA_Pay
        {
            get
            {
                Nullable<Int32> ret = 0;
                if (RowEvent["IDA_PAY"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDA_PAY"]);
                return ret;
            }
        }
        #endregion
        #region idB_Pay
        public Nullable<Int32> IdB_Pay
        {
            get
            {
                Nullable<Int32> ret = 0;
                if (RowEvent["IDB_PAY"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDB_PAY"]);
                return ret;
            }
        }
        #endregion
        #region idA_Rec
        public Nullable<Int32> IdA_Rec
        {
            get
            {
                Nullable<Int32> ret = 0;
                if (RowEvent["IDA_REC"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDA_REC"]);
                return ret;
            }
        }
        #endregion
        #region idB_Rec
        public Nullable<Int32> IdB_Rec
        {
            get
            {
                Nullable<Int32> ret = null;
                if (RowEvent["IDB_REC"] != Convert.DBNull)
                    ret = Convert.ToInt32(_rowEvent["IDB_REC"]);
                return ret;
            }
        }
        #endregion

        #region eventCode
        public string EventCode
        {
            get { return _rowEvent["EVENTCODE"].ToString(); }
        }
        #endregion
        #region eventType
        public string EventType
        {
            get { return _rowEvent["EVENTTYPE"].ToString(); }
        }
        #endregion


        #region dtStartUnadj
        public DateTime DtStartUnadj
        {
            get { return Convert.ToDateTime(_rowEvent["DTSTARTUNADJ"]); }
        }
        #endregion
        #region dtStartAdj
        public DateTime DtStartAdj
        {
            get { return Convert.ToDateTime(_rowEvent["DTSTARTADJ"]); }
        }
        #endregion

        #region dtEndUnadj
        public DateTime DtEndUnadj
        {
            get { return Convert.ToDateTime(_rowEvent["DTENDUNADJ"]); }
        }
        #endregion
        #region dtEndAdj
        public DateTime DtEndAdj
        {
            get { return Convert.ToDateTime(_rowEvent["DTENDADJ"]); }
        }
        #endregion

        #region valorisation
        public Nullable<Decimal> Valorisation
        {
            get
            {
                Nullable<Decimal> ret = null;
                if (Convert.DBNull != _rowEvent["VALORISATION"])
                    ret = Convert.ToDecimal(_rowEvent["VALORISATION"]);
                return ret;
            }
        }
        #endregion
        #region valorisationSys
        public Nullable<Decimal> ValorisationSys
        {
            get
            {
                Nullable<Decimal> ret = null;
                if (Convert.DBNull != _rowEvent["VALORISATIONSYS"])
                    ret = Convert.ToDecimal(_rowEvent["VALORISATIONSYS"]);
                return ret;
            }
        }
        #endregion
        #region unit
        public string Unit
        {
            get
            {
                string ret = null;
                if (Convert.DBNull != _rowEvent["UNIT"])
                    ret = Convert.ToString(_rowEvent["UNIT"]);
                return ret;
            }
        }
        #endregion
        #region unitSys
        public string UnitSys
        {
            get
            {
                string ret = null;
                if (Convert.DBNull != _rowEvent["UNITSYS"])
                    ret = Convert.ToString(_rowEvent["UNITSYS"]);
                return ret;
            }
        }
        #endregion
        #endregion Accessors
        #region Constructor
        public DataRowEvent(DataRow pRowEvent)
        {
            _rowEvent = pRowEvent;
        }
        #endregion Constructor
    }
    #endregion DataRowEvent

    #region EventQuery
    // EG 20120328 Add SetFeeLogInformation, InsertPaymentEvents et InitPaymentForEvent (Ticket 17706 Recalcul des frais )
    // EG 20141006 Fusion EVENDET_ETD/EVENTDET -> EVENTDET
    public class EventQuery
    {
        #region Members
        private readonly AppSession _session;
        private readonly Cst.ProcessTypeEnum _processType;
        private readonly int _idTrk; 
        #endregion Members

        #region Constructors
        /// <summary>
        ///  Contructeur 
        /// </summary>
        /// FI 20160928 [XXXXX] Modify
        public EventQuery(AppSession pSession, Cst.ProcessTypeEnum processTypeEnum, int idTrk)
        {
            _session = pSession;
            _processType = processTypeEnum;
            _idTrk = idTrk;
            // FI 20160928 [XXXXX] Modify
            //m_DtSys = OTCmlHelper.GetDateSys(pProcess.cs).Date;
            
        }
        #endregion Constructors

        #region Methods
        #region InsertEvent
        /// <summary>
        /// Insert into EVENT and EVENTPROCESS tables
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pIdE_Event"></param>
        /// <param name="pIdE_Source"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pIdA_Payer"></param>
        /// <param name="pIdB_Payer"></param>
        /// <param name="pIdA_Receiver"></param>
        /// <param name="pIdB_Receiver"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pDtStartUnadj"></param>
        /// <param name="pDtStartAdj"></param>
        /// <param name="pDtEndUnadj"></param>
        /// <param name="pDtEndAdj"></param>
        /// <param name="pValorisation"></param>
        /// <param name="pUnit"></param>
        /// <param name="pUnitType"></param>
        /// <param name="pStatusCalcul"></param>
        /// <param name="pIdStTrigger"></param>
        /// <returns></returns>
        /// EG 20150128 [20726] Math.Abs on VALORISATION|VALORISATIONSYS
        public Cst.ErrLevel InsertEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, int pIdE_Event, Nullable<int> pIdE_Source, int pInstrumentNo, int pStreamNo,
            Nullable<int> pIdA_Payer, Nullable<int> pIdB_Payer, Nullable<int> pIdA_Receiver, Nullable<int> pIdB_Receiver,
            string pEventCode, string pEventType, DateTime pDtStartUnadj, DateTime pDtStartAdj,
            DateTime pDtEndUnadj, DateTime pDtEndAdj, Nullable<decimal> pValorisation,
            string pUnit, string pUnitType, Nullable<StatusCalculEnum> pStatusCalcul, Nullable<Cst.StatusTrigger.StatusTriggerEnum> pIdStTrigger )
        {

            string cs = pDbTransaction.Connection.ConnectionString;
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(cs);

            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENT);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDT"].Value = pIdT;
            parameters["IDE"].Value = pIdE;
            parameters["IDE_EVENT"].Value = pIdE_Event;
            parameters["INSTRUMENTNO"].Value = pInstrumentNo;
            parameters["STREAMNO"].Value = pStreamNo;

            if (pIdE_Source.HasValue)
                parameters["IDE_SOURCE"].Value = pIdE_Source.Value;

            parameters["EVENTCODE"].Value = pEventCode;
            parameters["EVENTTYPE"].Value = pEventType;

            parameters["DTSTARTADJ"].Value = DataHelper.GetDBData(pDtStartAdj);
            parameters["DTSTARTUNADJ"].Value = DataHelper.GetDBData(pDtStartUnadj);
            // EG 20150723 [21215]
            parameters["DTENDADJ"].Value = DataHelper.GetDBData(pDtEndAdj);
            parameters["DTENDUNADJ"].Value = DataHelper.GetDBData(pDtEndUnadj);

            if (pIdA_Payer.HasValue)
                parameters["IDA_PAY"].Value = pIdA_Payer.Value;
            if (pIdB_Payer.HasValue)
                parameters["IDB_PAY"].Value = pIdB_Payer.Value;
            if (pIdA_Receiver.HasValue)
                parameters["IDA_REC"].Value = pIdA_Receiver.Value;
            if (pIdB_Receiver.HasValue)
                parameters["IDB_REC"].Value = pIdB_Receiver.Value;

            // EG 20150129 Add Math.Abs
            if (pValorisation.HasValue)
            {
                parameters["VALORISATION"].Value = System.Math.Abs(pValorisation.Value);
                parameters["VALORISATIONSYS"].Value = System.Math.Abs(pValorisation.Value);
            }
            if (StrFunc.IsFilled(pUnit))
            {
                parameters["UNIT"].Value = pUnit;
                parameters["UNITSYS"].Value = pUnit;
            }
            if (StrFunc.IsFilled(pUnitType))
            {
                parameters["UNITTYPE"].Value = pUnitType;
                parameters["UNITTYPESYS"].Value = pUnitType;
            }

            parameters["IDSTACTIVATION"].Value = Cst.STATUSREGULAR.ToString();
            parameters["IDASTACTIVATION"].Value = _session.IdA;
            parameters["DTSTACTIVATION"].Value = dtSys;

            parameters["IDSTCALCUL"].Value = pStatusCalcul.HasValue ? pStatusCalcul.Value.ToString() : StatusCalculEnum.CALC.ToString();
            parameters["IDSTTRIGGER"].Value = pIdStTrigger.HasValue ? pIdStTrigger.Value.ToString() : Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            parameters["SOURCE"].Value = (_session.AppInstance as AppInstanceService).ServiceName;

            //INSERT EVENT
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            //INSERT EVENTPROCESS
            Cst.ProcessTypeEnum processType = _processType;
            EventProcess eventProcess = new EventProcess(cs);
            eventProcess.Write(pDbTransaction, pIdE, processType, ProcessStateTools.StatusSuccessEnum, dtSys, _idTrk);

            return Cst.ErrLevel.SUCCESS;
        }
        
        /// <summary>
        /// Insert EVENT and EVENTPROCESS
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        /// <param name="pEvent">Données de l'événement à générer</param>
        /// <param name="pEventParent">Données de l'événement parent</param>
        /// <param name="pDataDocument">DataDocumentContainer</param>
        /// <returns></returns>
        /// EG 20150706 [21021] Nullable integer (idA_Pay|idB_Pay|idA_Rec|idB_Rec)
        public int InsertEvent(IDbTransaction pDbTransaction, int pIdT, int pIdE, EFS_Event pEvent, EFS_EventParent pEventParent, DataDocumentContainer pDataDocument)
        {
            Nullable<int> idE_Source = null;
            if (pEvent.eventKey.idE_SourceSpecified)
                idE_Source = pEvent.eventKey.idE_Source;

            int idE_Event = 0;
            // Case of OPP and ADP events
            if (null != pEventParent)
            {
                idE_Event = pEventParent.idE;
                // PM 20150324 [POC] Ne pas mettre à 0 le N° de stream des OPP pour les CashBalances
                if (false == pDataDocument.CurrentProduct.IsCashBalance)
                {
                    pEventParent.ChangeSequence(pEvent);
                }
            }

            int? idB_Pay = null;
            int? idA_Pay;
            // Payer/Receiver (If... Else pour gérer les payeurs des frais élémentaires sur facture)
            if ((pEvent.eventKey.idE_SourceSpecified || ((null != pEventParent) && pEventParent.idE_SourceSpecified)) &&
                ((null != pEvent.payer) && (-1 < pEvent.payer.IndexOf(';'))))
            {
                string[] invoice_Payer = pEvent.payer.Split(';');
                idA_Pay = Convert.ToInt32(invoice_Payer[0]);
                if (1 < invoice_Payer.Length)
                    idB_Pay = Convert.ToInt32(invoice_Payer[1]);
            }
            else
            {
                idA_Pay = pDataDocument.GetOTCmlId_Party(pEvent.payer);
                idB_Pay = pDataDocument.GetOTCmlId_Book(pEvent.payer);
            }
            int? idA_Rec = pDataDocument.GetOTCmlId_Party(pEvent.receiver);
            int? idB_Rec = pDataDocument.GetOTCmlId_Book(pEvent.receiver);

            string eventCode = EventCodeFunc.IsStrategy(pEvent.eventKey.eventCode) ? EventCodeFunc.Product : pEvent.eventKey.eventCode;

            Nullable<decimal> valorisation = null;
            if (pEvent.valorisationSpecified)
                valorisation = pEvent.valorisation.DecValue;

            string unit = string.Empty;
            if (pEvent.unitSpecified)
                unit = pEvent.unit;

            string unitType = string.Empty;
            if (pEvent.unitTypeSpecified)
                unitType = pEvent.unitType.ToString();

            Nullable<Cst.StatusTrigger.StatusTriggerEnum> idStTrigger = null;
            if (EventCodeFunc.IsTrigger(pEvent.eventKey.eventCode))
                idStTrigger = Cst.StatusTrigger.StatusTriggerEnum.NONE;

            Cst.ErrLevel ret = InsertEvent(pDbTransaction, pIdT, pIdE, idE_Event, idE_Source, pEvent.instrumentNo, pEvent.streamNo,
                idA_Pay, idB_Pay, idA_Rec, idB_Rec, eventCode, pEvent.eventKey.eventType,
                pEvent.startDate.unadjustedDate.DateValue, pEvent.startDate.adjustedDate.DateValue,
                pEvent.endDate.unadjustedDate.DateValue, pEvent.endDate.adjustedDate.DateValue,
                valorisation, unit, unitType, pEvent.idStCalcul, idStTrigger);

            return (ret == Cst.ErrLevel.SUCCESS ? 1 : -1);

        }
        #endregion InsertEvent

        #region InsertEventAsset
        /// <summary>
        /// Insertion dans la table EVENTASSET (NEW VERSION)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pAsset"></param>
        /// <returns></returns>
        // EG 20150203 IDM Setting to all Asset and IDEC to FXRateAsset
        public static Cst.ErrLevel InsertEventAsset(IDbTransaction pDbTransaction, int pIdE, EFS_Asset pAsset)
        {
            if ((null != pAsset) && (pAsset.assetCategory.HasValue))
            {
                if (pAsset.assetCategory.HasValue)
                {
                    string cs = pDbTransaction.Connection.ConnectionString;
                    QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTASSET);
                    
                    DataParameters parameters = queryParameters.Parameters;
                    parameters["IDE"].Value = pIdE;
                    parameters["IDASSET"].Value = pAsset.idAsset;
                    parameters["ASSETCATEGORY"].Value = DataHelper.GetDBData(pAsset.assetCategory.Value);
                    parameters["ASSETTYPE"].Value = DataHelper.GetDBData(pAsset.AssetType.Value);
                    parameters["IDM"].Value = DataHelper.GetDBData(pAsset.IdMarket);

                    switch (pAsset.assetCategory.Value)
                    {
                        case Cst.UnderlyingAsset.RateIndex:
                        case Cst.UnderlyingAsset.FxRateAsset:
                            parameters["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            parameters["TIME"].Value = DataHelper.GetDBData(pAsset.time);
                            parameters["IDBC"].Value = DataHelper.GetDBData(pAsset.idBC);
                            parameters["PRIMARYRATESRC"].Value = DataHelper.GetDBData(pAsset.primaryRateSrc);
                            parameters["PRIMARYRATESRCPAGE"].Value = DataHelper.GetDBData(pAsset.primaryRateSrcPage);
                            parameters["PRIMARYRATESRCHEAD"].Value = DataHelper.GetDBData(pAsset.primaryRateSrcHead);
                            break;
                        case Cst.UnderlyingAsset.EquityAsset:
                            parameters["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            parameters["ISINCODE"].Value = DataHelper.GetDBData(pAsset.isinCode);
                            parameters["ASSETSYMBOL"].Value = DataHelper.GetDBData(pAsset.assetSymbol);
                            break;
                        case Cst.UnderlyingAsset.Index:
                        case Cst.UnderlyingAsset.Bond:
                            parameters["CLEARANCESYSTEM"].Value = DataHelper.GetDBData(pAsset.clearanceSystem);
                            parameters["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            parameters["ISINCODE"].Value = DataHelper.GetDBData(pAsset.isinCode);
                            break;
                        case Cst.UnderlyingAsset.ExchangeTradedContract:
                        case Cst.UnderlyingAsset.Future:
                            parameters["CLEARANCESYSTEM"].Value = DataHelper.GetDBData(pAsset.clearanceSystem);
                            parameters["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            parameters["ISINCODE"].Value = DataHelper.GetDBData(pAsset.isinCode);
                            parameters["ASSETSYMBOL"].Value = DataHelper.GetDBData(pAsset.assetSymbol);
                            parameters["IDENTIFIER"].Value = DataHelper.GetDBData(pAsset.contractIdentifier);
                            parameters["DISPLAYNAME"].Value = DataHelper.GetDBData(pAsset.contractDisplayName);
                            parameters["CONTRACTSYMBOL"].Value = DataHelper.GetDBData(pAsset.contractSymbol);
                            parameters["CATEGORY"].Value = DataHelper.GetDBData(pAsset.category);
                            if (CfiCodeCategoryEnum.Option == pAsset.category)
                            {
                                parameters["PUTORCALL"].Value = DataHelper.GetDBData(pAsset.putOrCall);
                                if (null != pAsset.strikePrice)
                                    parameters["STRIKEPRICE"].Value = DataHelper.GetDBData(pAsset.strikePrice.DecValue);
                            }
                            if (null != pAsset.contractMultiplier)
                                parameters["CONTRACTMULTIPLIER"].Value = DataHelper.GetDBData(pAsset.contractMultiplier.DecValue);
                            parameters["MATURITYDATE"].Value = DataHelper.GetDBData(pAsset.maturityDate);
                            parameters["MATURITYDATESYS"].Value = DataHelper.GetDBData(pAsset.maturityDateSys);
                            parameters["DELIVERYDATE"].Value = DataHelper.GetDBData(pAsset.deliveryDate);
                            parameters["NOMINALVALUE"].Value = DataHelper.GetDBData(pAsset.nominalValue);
                            break;

                        case Cst.UnderlyingAsset.Commodity:
                            parameters["IDENTIFIER"].Value = DataHelper.GetDBData(pAsset.contractIdentifier);
                            parameters["DISPLAYNAME"].Value = DataHelper.GetDBData(pAsset.contractDisplayName);
                            parameters["CONTRACTSYMBOL"].Value = DataHelper.GetDBData(pAsset.contractSymbol);
                            parameters["IDC"].Value = DataHelper.GetDBData(pAsset.idC);
                            break;

                    }

                    DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventAsset
        #region InsertEventClass
        /// <summary>
        /// Insert into EVENTCLASS table (NEW VERSION)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pEventClass"></param>
        /// <param name="pDtEvent"></param>
        /// <param name="pIsPayment"></param>
        /// <returns></returns>
        /// FI 20160928 [XXXXX] Modify
        public Cst.ErrLevel InsertEventClass(IDbTransaction pDbTransaction, int pIdE, string pEventClass, DateTime pDtEvent, bool pIsPayment)
        {

            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTCLASS);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDE"].Value = pIdE;
            parameters["EVENTCLASS"].Value = pEventClass;
            parameters["DTEVENT"].Value = pDtEvent;
            // FI 20160928 [XXXXX] use m_DtSys.Date            
            //parameters["DTEVENTFORCED"].Value = m_DtSys;
            parameters["DTEVENTFORCED"].Value = new DateTime(Math.Max(OTCmlHelper.GetDateSys(cs).Date.Ticks, pDtEvent.Ticks));
            parameters["ISPAYMENT"].Value = pIsPayment;

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventClass
        #region InsertEventFee
        /// <summary>
        /// Insert into EVENTFEE table (NEW VERSION)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pSource">EFS_PaymentSource or EFS_TaxSource</param>
        /// <returns></returns>
        public static Cst.ErrLevel InsertEventFee<T>(IDbTransaction pDbTransaction, int pIdE, T pSource)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTFEE);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDE"].Value = pIdE;

            bool isEventFee = false;
            #region PAYMENT
            if (pSource is EFS_PaymentSource _source)
            {
                isEventFee = true;
                if (_source.statusSpecified)
                    parameters["STATUS"].Value = _source.status.ToString();
                // FI 20180328 [23871] Add PAYMENTID column
                if (_source.paymentIdSpecified)
                    parameters["PAYMENTID"].Value = _source.paymentId;
                // PL 20200107 [25099] Add FEESCOPE
                if (_source.feeScopeSpecified)
                    parameters["FEESCOPE"].Value = _source.feeScope;
                if (_source.idFeeMatrixSpecified)
                    parameters["IDFEEMATRIX"].Value = _source.idFeeMatrix;
                if (_source.idFeeSpecified)
                    parameters["IDFEE"].Value = _source.idFee;
                if (_source.idFeeScheduleSpecified)
                    parameters["IDFEESCHEDULE"].Value = _source.idFeeSchedule;
                if (_source.bracket1Specified)
                    parameters["BRACKET1"].Value = _source.bracket1;
                if (_source.bracket2Specified)
                    parameters["BRACKET2"].Value = _source.bracket2;
                if (_source.formulaSpecified)
                    parameters["FORMULA"].Value = _source.formula;
                if (_source.formulaValue1Specified)
                    parameters["FORMULAVALUE1"].Value = _source.formulaValue1;
                if (_source.formulaValue2Specified)
                    parameters["FORMULAVALUE2"].Value = _source.formulaValue2;
                if (_source.formulaValueBracketSpecified)
                    parameters["FORMULAVALUEBRACKET"].Value = _source.formulaValueBracket;
                if (_source.formulaDCFSpecified)
                    parameters["FORMULADCF"].Value = _source.formulaDCF;
                if (_source.formulaMinSpecified)
                    parameters["FORMULAMIN"].Value = _source.formulaMin;
                if (_source.formulaMaxSpecified)
                    parameters["FORMULAMAX"].Value = _source.formulaMax;
                if (_source.feePaymentFrequencySpecified)
                    parameters["FEEPAYMENTFREQUENCY"].Value = _source.feePaymentFrequency;
                //PL 20141023
                //if (_source.assessmentBasisValueSpecified)
                //    m_ParamEventFee["ASSESSMENTBASISVALUE"].Value = _source.assessmentBasisValue;
                if (_source.assessmentBasisValue1Specified)
                    parameters["ASSESSMENTBASISVALUE1"].Value = _source.assessmentBasisValue1;
                if (_source.assessmentBasisValue2Specified)
                    parameters["ASSESSMENTBASISVALUE2"].Value = _source.assessmentBasisValue2;
                if (_source.paymentTypeSpecified)
                    parameters["PAYMENTTYPE"].Value = _source.paymentType;
                if (_source.assessmentBasisDetSpecified)
                    parameters["ASSESSMENTBASISDET"].Value = _source.assessmentBasisDet;

                parameters["ISFEEINVOICING"].Value = _source.isFeeInvoicing;
            }
            #endregion PAYMENT
            #region TAX
            else if (pSource is EFS_TaxSource _taxsource)
            {
                isEventFee = true;
                if (_taxsource.idTaxSpecified)
                    parameters["IDTAX"].Value = _taxsource.idTax;
                if (_taxsource.idTaxDetSpecified)
                    parameters["IDTAXDET"].Value = _taxsource.idTaxDet;
                if (_taxsource.taxTypeSpecified)
                    parameters["TAXTYPE"].Value = _taxsource.taxType;
                if (_taxsource.taxRateSpecified)
                    parameters["TAXRATE"].Value = _taxsource.taxRate;
                if (_taxsource.taxCountrySpecified)
                    parameters["TAXCOUNTRY"].Value = _taxsource.taxCountry;
            }
            #endregion TAX

            if (isEventFee)
                DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventFee
        #region InsertEventPosActionDet
        /// <summary>
        /// Insertion de la ligne matérialisant les liens entre POSACTIONDET et EVENT
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdPADET">Identifiant de la ligne matérialisant la clôture</param>
        /// <param name="pIdE">Identifiant de l'événément de départ (boucle de 2x4 EVENT (Clôturée et clôturante avec OFS,NOM,QTY et RMG)</param>
        /// <returns></returns>
        public static Cst.ErrLevel InsertEventPosActionDet(IDbTransaction pDbTransaction, int pIdPADET, int pIdE)
        {

            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTPOSACTIONDET);
            
            DataParameters parameters = queryParameters.Parameters;
            parameters["IDPADET"].Value = pIdPADET;
            parameters["IDE"].Value = pIdE;

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventPosActionDet
        #region InsertEventDet
        // EG 20161122 New Commodity Derivative
        // EG 20171025 [23509] Add TZDLVY
        public static Cst.ErrLevel InsertEventDet(IDbTransaction pDbTransaction, int pIdE, EFS_EventDetail pEventDetail, string pUnitBase)
        {
            if (pEventDetail.IsEventDetSpecified)
            {

                string cs = pDbTransaction.Connection.ConnectionString;
                QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTDET);

                DataParameters parameters = queryParameters.Parameters;
                parameters["IDE"].Value = pIdE;
                
                if (pEventDetail.fixedRateSpecified)
                {
                    #region fixedRate
                    parameters["RATE"].Value = pEventDetail.fixedRate.DecValue;
                    #endregion
                }
                
                if (pEventDetail.dayCountFractionSpecified)
                {
                    #region dayCountFraction
                    parameters["DCF"].Value = pEventDetail.dayCountFraction.DayCountFractionFpML;
                    parameters["DCFNUM"].Value = pEventDetail.dayCountFraction.Numerator;
                    parameters["DCFDEN"].Value = pEventDetail.dayCountFraction.Denominator;
                    parameters["TOTALOFYEAR"].Value = pEventDetail.dayCountFraction.NumberOfCalendarYears;
                    parameters["TOTALOFDAY"].Value = pEventDetail.dayCountFraction.TotalNumberOfCalculatedDays;
                    #endregion
                }

                if (pEventDetail.multiplierSpecified)
                {
                    #region multiplier 
                    parameters["MULTIPLIER"].Value = pEventDetail.multiplier.DecValue;
                    #endregion multiplier
                }

                if (pEventDetail.spreadSpecified)
                {
                    #region spread
                    parameters["SPREAD"].Value = pEventDetail.spread.DecValue;
                    #endregion spread
                }

                if (pEventDetail.paymentQuoteSpecified)
                {
                    #region paymentQuote
                    if (null != pEventDetail.paymentQuote.notionalAmount)
                        parameters["NOTIONALREFERENCE"].Value = pEventDetail.paymentQuote.notionalAmount.DecValue;
                    if (null != pEventDetail.paymentQuote.percentageRate)
                        parameters["PCTRATE"].Value = pEventDetail.paymentQuote.percentageRate.DecValue;
                    #endregion paymentQuote
                }
                else if (pEventDetail.premiumQuoteSpecified)
                {
                    #region premiumQuote
                    EFS_FxPremiumQuote fxPremiumQuote = pEventDetail.premiumQuote;
                    parameters["IDC1"].Value = fxPremiumQuote.callCurrency;
                    parameters["IDC2"].Value = fxPremiumQuote.putCurrency;
                    parameters["BASIS"].Value = fxPremiumQuote.premiumQuote.PremiumQuoteBasis.ToString();
                    if (fxPremiumQuote.amountReferenceSpecified)
                    {
                        parameters["NOTIONALREFERENCE"].Value = fxPremiumQuote.amountReference.Amount.DecValue;
                        parameters["IDC_REF"].Value = fxPremiumQuote.amountReference.Currency;
                        parameters["PCTRATE"].Value = fxPremiumQuote.premiumQuote.PremiumValue.DecValue;
                    }
                    #endregion
                }
                else if (pEventDetail.etdPremiumCalculationSpecified)
                {
                    #region etdPremiumCalculation
                    EFS_ETDPremiumCalculation premiumCalculation = pEventDetail.etdPremiumCalculation;
                    parameters["DAILYQUANTITY"].Value = premiumCalculation.qty;
                    parameters["CONTRACTMULTIPLIER"].Value = premiumCalculation.multiplier;
                    parameters["PRICE"].Value = premiumCalculation.price;
                    parameters["PRICE100"].Value = premiumCalculation.price100;
                    #endregion
                }

                if (pEventDetail.exchangeRateSpecified)
                {
                    #region exchangeRate
                    IExchangeRate iExchangeRate = pEventDetail.exchangeRate.exchangeRate;
                    parameters["IDC1"].Value = iExchangeRate.QuotedCurrencyPair.Currency1;
                    parameters["IDC2"].Value = iExchangeRate.QuotedCurrencyPair.Currency2;
                    parameters["BASIS"].Value = iExchangeRate.QuotedCurrencyPair.QuoteBasis.ToString();
                    parameters["RATE"].Value = iExchangeRate.Rate.DecValue;
                    if (iExchangeRate.SpotRateSpecified)
                        parameters["SPOTRATE"].Value = iExchangeRate.SpotRate.DecValue;
                    if (iExchangeRate.ForwardPointsSpecified)
                        parameters["FWDPOINTS"].Value = iExchangeRate.ForwardPoints.DecValue;
                    
                    if (pEventDetail.exchangeRate.referenceCurrencySpecified)
                        parameters["IDC_REF"].Value = pEventDetail.exchangeRate.referenceCurrency;
                    
                    if (pEventDetail.exchangeRate.notionalAmountSpecified)
                        parameters["NOTIONALAMOUNT"].Value = pEventDetail.exchangeRate.notionalAmount.DecValue;
                    #endregion
                }
                else if (pEventDetail.sideRateSpecified)
                {
                    #region sideRate
                    ISideRate iSideRate = pEventDetail.sideRate;
                    parameters["IDC_BASE"].Value = StrFunc.IsFilled(pUnitBase) ? pUnitBase : Convert.DBNull;
                    if ((SideRateBasisEnum.BaseCurrencyPerCurrency1 == iSideRate.SideRateBasis) ||
                        (SideRateBasisEnum.Currency1PerBaseCurrency == iSideRate.SideRateBasis))
                        parameters["IDC1"].Value = iSideRate.Currency;
                    else if ((SideRateBasisEnum.BaseCurrencyPerCurrency2 == iSideRate.SideRateBasis) ||
                        (SideRateBasisEnum.Currency2PerBaseCurrency == iSideRate.SideRateBasis))
                        parameters["IDC2"].Value = iSideRate.Currency;
                    
                    parameters["BASIS"].Value = iSideRate.SideRateBasis.ToString();
                    parameters["RATE"].Value = iSideRate.Rate.DecValue;
                    
                    if (iSideRate.SpotRateSpecified)
                        parameters["SPOTRATE"].Value = iSideRate.SpotRate.DecValue;
                    
                    if (iSideRate.ForwardPointsSpecified)
                        parameters["FWDPOINTS"].Value = iSideRate.ForwardPoints.DecValue;
                    #endregion
                }
                else if (pEventDetail.fixingRateSpecified)
                {
                    #region FixingRate
                    EFS_FxFixing fixingRate = pEventDetail.fixingRate;
                    IFxFixing iFxFixing = fixingRate.fixing;
                    DateTime dtFixingTime = iFxFixing.FixingTime.HourMinuteTime.TimeValue;
                    DateTime dtFixing = DtFunc.AddTimeToDate(iFxFixing.FixingDate.DateValue, dtFixingTime);

                    parameters["DTFIXING"].Value = DataHelper.GetDBData(dtFixing);
                    parameters["IDBC"].Value = iFxFixing.FixingTime.BusinessCenter.Value;
                    parameters["IDC1"].Value = iFxFixing.QuotedCurrencyPair.Currency1;
                    parameters["IDC2"].Value = iFxFixing.QuotedCurrencyPair.Currency2;
                    parameters["BASIS"].Value = iFxFixing.QuotedCurrencyPair.QuoteBasis.ToString();
                    if (fixingRate.referenceCurrencySpecified)
                        parameters["IDC_REF"].Value = fixingRate.referenceCurrency;
                    if (fixingRate.notionalAmountSpecified)
                        parameters["NOTIONALAMOUNT"].Value = fixingRate.notionalAmount.DecValue;
                    #endregion FixingRate
                }
                else if (pEventDetail.invoicingAmountBaseSpecified)
                {
                    #region InvoicingAmountBase
                    EFS_InvoicingAmountBase invoicingAmountBase = pEventDetail.invoicingAmountBase;
                    parameters["NOTIONALAMOUNT"].Value = invoicingAmountBase.notionalAmount.DecValue;
                    parameters["IDC_REF"].Value = invoicingAmountBase.referenceCurrency;
                    #endregion InvoicingAmountBase
                }
                else if (pEventDetail.settlementRateSpecified)
                {
                    #region SettlementRate
                    EFS_SettlementRate settlementRate = pEventDetail.settlementRate;
                    parameters["FXTYPE"].Value = settlementRate.fxType.ToString();
                    parameters["NOTIONALAMOUNT"].Value = settlementRate.notionalAmount.DecValue;
                    parameters["RATE"].Value = settlementRate.forwardRate.DecValue;
                    parameters["IDC_REF"].Value = settlementRate.referenceCurrency;
                    if (settlementRate.spotRateSpecified)
                        parameters["SPOTRATE"].Value = settlementRate.spotRate.DecValue;
                    if (settlementRate.settlementRateSpecified)
                        parameters["SETTLEMENTRATE"].Value = settlementRate.settlementRate.DecValue;
                    if (settlementRate.conversionRateSpecified)
                        parameters["CONVERSIONRATE"].Value = settlementRate.conversionRate.DecValue;
                    #endregion SettlementRate
                }
                else if (pEventDetail.currencyPairSpecified)
                {
                    #region CurrencyPair
                    IQuotedCurrencyPair iCurrencyPair = pEventDetail.currencyPair;
                    parameters["IDC1"].Value = iCurrencyPair.Currency1;
                    parameters["IDC2"].Value = iCurrencyPair.Currency2;
                    parameters["BASIS"].Value = iCurrencyPair.QuoteBasis.ToString();
                    #endregion CurrencyPair
                }
                else if (pEventDetail.triggerRateSpecified)
                {
                    #region TriggerRate
                    EFS_TriggerRate triggerRate = pEventDetail.triggerRate;
                    parameters["SPOTRATE"].Value = triggerRate.spotRate.DecValue;
                    parameters["RATE"].Value = triggerRate.triggerRate.DecValue;
                    IQuotedCurrencyPair iCurrencyPair = triggerRate.currencyPair;
                    parameters["IDC1"].Value = iCurrencyPair.Currency1;
                    parameters["IDC2"].Value = iCurrencyPair.Currency2;
                    parameters["BASIS"].Value = iCurrencyPair.QuoteBasis.ToString();
                    #endregion TriggerRate
                }
                else if (pEventDetail.strikePriceSpecified)
                {
                    #region StrikePrice
                    IFxStrikePrice iStrikePrice = pEventDetail.strikePrice;
                    parameters["STRIKEPRICE"].Value = iStrikePrice.Rate.DecValue;
                    parameters["BASIS"].Value = iStrikePrice.StrikeQuoteBasis.ToString();
                    #endregion StrikePrice
                }
                else if (pEventDetail.strikeSpecified)
                {
                    #region Strike
                    parameters["STRIKEPRICE"].Value = pEventDetail.strike.DecValue;
                    #endregion Strike
                }
                else if (pEventDetail.triggerEventSpecified)
                {
                    #region TriggerEvent
                    EFS_TriggerEvent triggerEvent = pEventDetail.triggerEvent;
                    parameters["RATE"].Value = triggerEvent.triggerPrice.DecValue;
                    if (triggerEvent.strike.spotPriceSpecified)
                        parameters["SPOTRATE"].Value = triggerEvent.strike.spotPrice.DecValue;
                    #endregion TriggerRate
                }

                /// EG 20161122 New Commodity Derivative
                if (pEventDetail.fixedPriceSpecified)
                {
                    #region fixedPrice
                    parameters["PRICE"].Value = pEventDetail.fixedPrice.price.DecValue;
                    parameters["PRICE100"].Value = pEventDetail.fixedPrice.price.DecValue;
                    parameters["SETTLTPRICE"].Value = pEventDetail.fixedPrice.settlementPrice.DecValue;
                    parameters["SETTLTPRICE100"].Value = pEventDetail.fixedPrice.settlementPrice.DecValue;
                    parameters["DAILYQUANTITY"].Value = pEventDetail.fixedPrice.totalQuantity.DecValue;
                    parameters["UNITDAILYQUANTITY"].Value = pEventDetail.fixedPrice.unitQuantity;
                    #endregion fixedPrice
                }
                else if (pEventDetail.deliveryDateSpecified)
                {
                    #region fixedPrice
                    parameters["DTDLVYSTART"].Value = pEventDetail.deliveryDate.startDate;
                    parameters["DTDLVYEND"].Value = pEventDetail.deliveryDate.endDate;
                    parameters["TZDLVY"].Value = pEventDetail.deliveryDate.timezone;
                    #endregion fixedPrice
                }
 
                DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet
        #region InsertEventDet_Notes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pNotes"></param>
        /// <returns></returns>
        /// EG 20141114 Correction alimentation paramètres IDE, DTACTION, NOTE
        /// FI 20160928 [XXXXX] Modify
        public Cst.ErrLevel InsertEventDet_Notes(IDbTransaction pDbTransaction, int pIdE, string pNotes)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTDET);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDE"].Value = pIdE;
            // FI 20160928 [XXXXX] m_DtSys.Date
            parameters["DTACTION"].Value = OTCmlHelper.GetDateSys(cs).Date;
            parameters["NOTE"].Value = (StrFunc.IsFilled(pNotes) ? pNotes : Convert.DBNull);

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet_Notes
        #region InsertEventDet_Denouement
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pQuantity"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pStrikePrice"></param>
        /// <param name="pSettltQuoteSide"></param>
        /// <param name="pSettltQuoteTiming"></param>
        /// <param name="pDtSettltPrice"></param>
        /// <param name="pSettltPrice"></param>
        /// <param name="pSettltPrice100"></param>
        /// <param name="pNotes"></param>
        /// <returns></returns>
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// FI 20160928 [XXXXX] Modify
        /// // EG 20170127 Qty Long To Decimal
        public Cst.ErrLevel InsertEventDet_Denouement(IDbTransaction pDbTransaction, int pIdE, decimal pQuantity,
            Nullable<decimal> pContractMultiplier, Nullable<decimal> pStrikePrice,
            Nullable<QuotationSideEnum> pSettltQuoteSide, Nullable<QuoteTimingEnum> pSettltQuoteTiming,
            Nullable<DateTime> pDtSettltPrice, Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100, string pNotes)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTDET);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDE"].Value = pIdE;
            // FI 20160928 [XXXXX] use m_DtSys.Date
            //parameters["DTACTION"].Value = m_DtSys;
            parameters["DTACTION"].Value = OTCmlHelper.GetDateSys(cs).Date;
            parameters["NOTE"].Value = (StrFunc.IsFilled(pNotes) ? pNotes : Convert.DBNull);

            if (pContractMultiplier.HasValue)
                parameters["CONTRACTMULTIPLIER"].Value = pContractMultiplier.Value;

            parameters["DAILYQUANTITY"].Value = pQuantity;

            if (pDtSettltPrice.HasValue)
                parameters["DTSETTLTPRICE"].Value = pDtSettltPrice.Value;

            if (pSettltQuoteSide.HasValue)
                parameters["SETTLTQUOTESIDE"].Value = pSettltQuoteSide.Value;
            if (pSettltQuoteTiming.HasValue)
                parameters["SETTLTQUOTETIMING"].Value = pSettltQuoteTiming.Value;
            if (pSettltPrice.HasValue)
                parameters["SETTLTPRICE"].Value = pSettltPrice.Value;
            if (pSettltPrice100.HasValue)
                parameters["SETTLTPRICE100"].Value = pSettltPrice100.Value;
            if (pStrikePrice.HasValue)
                parameters["STRIKEPRICE"].Value = pStrikePrice.Value;

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet_Denouement
        #region InsertEventDet_Closing
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        // EG 20190730 Add TypePrice parameter
        public Cst.ErrLevel InsertEventDet_Closing(IDbTransaction pDbTransaction, int pIdE, decimal pQuantity,
            decimal pContractMultiplier, Nullable<AssetMeasureEnum> pTypePrice,
            Nullable<decimal> pPrice, Nullable<decimal> pPrice100, Nullable<decimal> pClosingPrice, Nullable<decimal> pClosingPrice100)
        {
            return InsertEventDet_Closing(pDbTransaction, pIdE, pQuantity, pContractMultiplier, null, null, pTypePrice, pPrice, pPrice100,
                null, null, null, null, null, null, pClosingPrice, pClosingPrice100);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pQuantity"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pFactor"></param>
        /// <param name="pStrikePrice"></param>
        /// <param name="pPrice"></param>
        /// <param name="pPrice100"></param>
        /// <param name="pQuoteTiming"></param>
        /// <param name="pQuotePrice"></param>
        /// <param name="pQuotePrice100"></param>
        /// <param name="pQuoteDelta"></param>
        /// <param name="pQuotePriceYest"></param>
        /// <param name="pQuotePriceYest100"></param>
        /// <param name="pClosingPrice"></param>
        /// <param name="pClosingPrice100"></param>
        /// <returns></returns>
        /// EG 20150920 [21374] Int (int32) to Long (Int64) 
        /// FI 20160928 [XXXXX] Modify
        /// EG 20170127 Qty Long To Decimal
        // EG 20190730 Add TypePrice parameter
        public Cst.ErrLevel InsertEventDet_Closing(IDbTransaction pDbTransaction, int pIdE, decimal pQuantity,
            Nullable<decimal> pContractMultiplier, Nullable<decimal> pFactor, Nullable<decimal> pStrikePrice,
            Nullable<AssetMeasureEnum> pTypePrice, 
            Nullable<decimal> pPrice, Nullable<decimal> pPrice100,
            string pQuoteTiming, Nullable<decimal> pQuotePrice, Nullable<decimal> pQuotePrice100, Nullable<decimal> pQuoteDelta,
            Nullable<decimal> pQuotePriceYest, Nullable<decimal> pQuotePriceYest100,
            Nullable<decimal> pClosingPrice, Nullable<decimal> pClosingPrice100)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTDET);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDE"].Value = pIdE;
            
            if (pTypePrice.HasValue)
                parameters["ASSETMEASURE"].Value = pTypePrice.Value;

            if (pClosingPrice.HasValue)
                parameters["CLOSINGPRICE"].Value = pClosingPrice.Value;
            if (pClosingPrice100.HasValue)
                parameters["CLOSINGPRICE100"].Value = pClosingPrice100.Value;

            if (pContractMultiplier.HasValue)
                parameters["CONTRACTMULTIPLIER"].Value = pContractMultiplier.Value;

            parameters["DAILYQUANTITY"].Value = pQuantity;
            //parameters["DTACTION"].Value = m_DtSys;
            // FI 20160928 [XXXXX] use m_DtSys.Date
            parameters["DTACTION"].Value = OTCmlHelper.GetDateSys(cs).Date;

            if (pFactor.HasValue)
                parameters["FACTOR"].Value = pFactor.Value;

            if (pPrice.HasValue)
                parameters["PRICE"].Value = pPrice.Value;
            if (pPrice100.HasValue)
                parameters["PRICE100"].Value = pPrice100.Value;

            if (pQuoteDelta.HasValue)
                parameters["QUOTEDELTA"].Value = pQuoteDelta;
            if (StrFunc.IsFilled(pQuoteTiming))
                parameters["QUOTETIMING"].Value = pQuoteTiming;
            if (pQuotePrice.HasValue)
                parameters["QUOTEPRICE"].Value = pQuotePrice.Value;
            if (pQuotePrice100.HasValue)
                parameters["QUOTEPRICE100"].Value = pQuotePrice100.Value;
            if (pQuotePriceYest.HasValue)
                parameters["QUOTEPRICEYEST"].Value = pQuotePriceYest.Value;
            if (pQuotePriceYest100.HasValue)
                parameters["QUOTEPRICEYEST100"].Value = pQuotePriceYest100.Value;

            if (pStrikePrice.HasValue)
                parameters["STRIKEPRICE"].Value = pStrikePrice.Value;

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet_Closing

        #region InsertEventDet_Delivery
        /// <summary>
        /// Insertion dans la table EVENTDET des information liées
        /// aux paiement et à la livraison (Cas des DC avec livraison périodique)
        /// - Quantité (Quantité en position * UnitMeasureQty * Duréée de la période)
        /// - Unité de la quantité
        /// - Settlement price informations
        /// - Date de début et de fin de livraison (DatetimeOffset)
        /// </summary>
        // EG 20170206 [22787]
        // EG 20171025 [23509] Upd DateTime on DtDeliveryDates
        public Cst.ErrLevel InsertEventDet_Delivery(IDbTransaction pDbTransaction, int pIdE, 
            Nullable<decimal> pQuantity, string pUnitQuantity,
            Nullable<QuotationSideEnum> pSettltQuoteSide, Nullable<QuoteTimingEnum> pSettltQuoteTiming,
            Nullable<DateTime> pDtSettltPrice, Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100,
            Nullable<DateTime> pDtDeliveryStart, Nullable<DateTime> pDtDeliveryEnd, string pDeliveryTimeZone, string pNotes)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            QueryParameters queryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTDET);

            DataParameters parameters = queryParameters.Parameters;
            parameters["IDE"].Value = pIdE;
            parameters["NOTE"].Value = (StrFunc.IsFilled(pNotes) ? pNotes : Convert.DBNull);

            if (pQuantity.HasValue)
            {
                parameters["DAILYQUANTITY"].Value = pQuantity.Value;
                parameters["UNITDAILYQUANTITY"].Value = (StrFunc.IsFilled(pUnitQuantity) ? pUnitQuantity : Convert.DBNull); ;
            }

            if (pDtSettltPrice.HasValue)
                parameters["DTSETTLTPRICE"].Value = pDtSettltPrice.Value;

            if (pSettltQuoteSide.HasValue)
                parameters["SETTLTQUOTESIDE"].Value = pSettltQuoteSide.Value;
            if (pSettltQuoteTiming.HasValue)
                parameters["SETTLTQUOTETIMING"].Value = pSettltQuoteTiming.Value;
            if (pSettltPrice.HasValue)
                parameters["SETTLTPRICE"].Value = pSettltPrice.Value;
            if (pSettltPrice100.HasValue)
                parameters["SETTLTPRICE100"].Value = pSettltPrice100.Value;

            if (pDtDeliveryStart.HasValue)
                parameters["DTDLVYSTART"].Value = pDtDeliveryStart.Value;
            if (pDtDeliveryEnd.HasValue)
                parameters["DTDLVYEND"].Value = pDtDeliveryEnd.Value;
            if (StrFunc.IsFilled(pDeliveryTimeZone))
                parameters["TZDLVY"].Value = pDeliveryTimeZone;

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion InsertEventDet_Delivery

        /// <summary>
        /// Désactivation d'un événement s'il existe (via IDT)
        /// Ajout d'un EVENTCLASS (RMV) sur  l'événement
        /// pour une EVENTCODE,EVENTYPE à une date donnée
        /// </summary>
        /// EG 20210415 [25584][25702] Gestion de la validité des LPP/LPC VMG sur options marginées (suite à modification de Date de maturité)
        public Cst.ErrLevel DeactivEvents(string pCS, IDbTransaction pDbTransaction, int pIdT, string pEventCode, string pEventType, string pEventClass, DateTime pDtBusiness, int pIdA)
        {

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventCode);
            parameters.Add(new DataParameter(pCS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventType);
            parameters.Add(new DataParameter(pCS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventClass);
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pDtBusiness);

            string sqlQuery = @"select ev.IDE from dbo.EVENT ev
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ev.EVENTTYPE = @EVENTTYPE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = @EVENTCLASS) and (IDSTACTIVATION = 'REGULAR')";

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);

            Object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, parameters.GetArrayDbParameter());
            if (null != obj)
            {
                DateTime dtEventForced = OTCmlHelper.GetAnticipatedDate(pCS, pDbTransaction, pDtBusiness);

                parameters.Clear();
                parameters.Add(new DataParameter(pCS, "IDE", DbType.Int32), Convert.ToInt32(obj));
                parameters.Add(new DataParameter(pCS, "IDASTACTIVATION", DbType.Int32), pIdA);
                parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pDtBusiness);
                parameters.Add(new DataParameter(pCS, "DTEVENTFORCED", DbType.Date), dtEventForced);

                sqlQuery = @"Update dbo.EVENT set IDASTACTIVATION = @IDASTACTIVATION, DTSTACTIVATION = @DTBUSINESS, IDSTACTIVATION = 'DEACTIV' where (IDE = @IDE);
                insert dbo.EVENTCLASS (IDE,EVENTCLASS, DTEVENT, DTEVENTFORCED) values (@IDE, 'RMV', @DTBUSINESS, @DTEVENTFORCED);" + Cst.CrLf;

                qryParameters = new QueryParameters(pCS, sqlQuery, parameters);
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// [UNUSED]
        /// Désactivation d'un événement s'il existe (via IDT)
        /// Ajout d'un EVENTCLASS (RMV) sur  l'événement
        /// pour une EVENTCODE,EVENTYPE à une date donnée
        /// </summary>
        /// EG 20210415 [25584][25702] Gestion de la validité des LPP/LPC VMG sur options marginées (suite à modification de Date de maturité)
        public Cst.ErrLevel DeleteEvents(string pCS, IDbTransaction pDbTransaction, int pIdT, string pEventCode, string pEventType, string pEventClass, DateTime pDtBusiness)
        {
            string sqlDelete = @"delete from dbo.EVENT ev
            inner join EVENTCLASS ec on (ec.IDE = ev.IDE)
            where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ev.EVENTTYPE = @EVENTTYPE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = @EVENTCLASS)";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventCode);
            parameters.Add(new DataParameter(pCS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventType);
            parameters.Add(new DataParameter(pCS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventClass);
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pDtBusiness);

            QueryParameters qryParameters = new QueryParameters(pCS, sqlDelete, parameters);
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        /* ---------------------------------------------------------------------- */
        /* Utilisées pour le recalcul des SCU|VMG sur Dénouements d'options ETD   */
        /* ---------------------------------------------------------------------- */
        #region UpdateEvent_Amount
        // EG 20140116 [19456]
        // EG 20150129 [20726]
        public static Cst.ErrLevel UpdateEvent_Amount(string pCS, IDbTransaction pDbTransaction, int pIdE,
            Nullable<decimal> pAmount, Nullable<decimal> pIdA_Payer, Nullable<decimal> pIdB_Payer,
            Nullable<decimal> pIdA_Receiver, Nullable<decimal> pIdB_Receiver, Nullable<StatusCalculEnum> pStatusCalculEnum)
        {
            //FI 20181220 [24414] Add VALORISATIONSYS = @VALORISATIONSYS 

            string sqlUpdate = @"update dbo.EVENT set
            VALORISATION = @VALORISATION, 
            VALORISATIONSYS = @VALORISATIONSYS, 
            IDA_PAY= @IDA_PAY, IDB_PAY = @IDB_PAY, 
            IDA_REC= @IDA_REC, IDB_REC = @IDB_REC, 
            IDSTCALCUL = @IDSTCALCUL
            where IDE = @IDE";

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDE", DbType.Int32), pIdE);
            parameters.Add(new DataParameter(pCS, "IDA_PAY", DbType.Int32));
            parameters.Add(new DataParameter(pCS, "IDB_PAY", DbType.Int32));
            parameters.Add(new DataParameter(pCS, "IDA_REC", DbType.Int32));
            parameters.Add(new DataParameter(pCS, "IDB_REC", DbType.Int32));
            parameters.Add(new DataParameter(pCS, "VALORISATION", DbType.Decimal));
            parameters.Add(new DataParameter(pCS, "VALORISATIONSYS", DbType.Decimal));
            parameters.Add(new DataParameter(pCS, "IDSTCALCUL", DbType.AnsiString, SQLCst.UT_STATUS_LEN));

            parameters.SetAllDBNull();

            parameters["IDE"].Value = pIdE;
            if (pIdA_Payer.HasValue) parameters["IDA_PAY"].Value = pIdA_Payer;
            if (pIdB_Payer.HasValue) parameters["IDB_PAY"].Value = pIdB_Payer;
            if (pIdA_Receiver.HasValue) parameters["IDA_REC"].Value = pIdA_Receiver;
            if (pIdB_Receiver.HasValue) parameters["IDB_REC"].Value = pIdB_Receiver;
            if (pAmount.HasValue)
            {
                parameters["VALORISATION"].Value = System.Math.Abs(pAmount.Value);
                parameters["VALORISATIONSYS"].Value = System.Math.Abs(pAmount.Value);
            }
            if (pStatusCalculEnum.HasValue) 
                parameters["IDSTCALCUL"].Value = pStatusCalculEnum;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateEvent_Amount
        #region UpdateEventDet_Closing
        // EG 20150129 [20726] New
        public static Cst.ErrLevel UpdateEventDet_Closing(string pCS, IDbTransaction pDbTransaction, int pIdE, Nullable<decimal> pQuotePrice, Nullable<decimal> pQuotePrice100, Nullable<decimal> pQuoteDelta)
        {
            string sqlUpdate = @"update dbo.EVENTDET set 
            QUOTEPRICE = @QUOTEPRICE, QUOTEPRICE100 = @QUOTEPRICE100, QUOTEDELTA = @QUOTEDELTA
            where IDE = @IDE";

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDE", DbType.Int32));
            parameters.Add(new DataParameter(pCS, "QUOTEPRICE", DbType.Decimal));
            parameters.Add(new DataParameter(pCS, "QUOTEPRICE100", DbType.Decimal));
            parameters.Add(new DataParameter(pCS, "QUOTEDELTA", DbType.Decimal));
            parameters.SetAllDBNull();

            parameters["IDE"].Value = pIdE;
            if (pQuotePrice.HasValue) parameters["QUOTEPRICE"].Value = pQuotePrice;
            if (pQuotePrice100.HasValue) parameters["QUOTEPRICE100"].Value = pQuotePrice100;
            if (pQuoteDelta.HasValue) parameters["QUOTEDELTA"].Value = pQuoteDelta;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateEventDet_Closing
        #region UpdateEventDet_Denouement
        // EG 20140116 [19456]
        public static Cst.ErrLevel UpdateEventDet_Denouement(string pCS, IDbTransaction pDbTransaction, int pIdE,
            Nullable<decimal> pStrikePrice, Nullable<QuotationSideEnum> pSettltQuoteSide, Nullable<QuoteTimingEnum> pSettltQuoteTiming,
            Nullable<DateTime> pDtSettltPrice, Nullable<decimal> pSettltPrice, Nullable<decimal> pSettltPrice100)
        {

            string sqlUpdate = @"update dbo.EVENTDET set 
            STRIKEPRICE = @STRIKEPRICE, SETTLTQUOTESIDE = @SETTLTQUOTESIDE, SETTLTQUOTETIMING = @SETTLTQUOTETIMING, 
            DTSETTLTPRICE = @DTSETTLTPRICE, SETTLTPRICE = @SETTLTPRICE, SETTLTPRICE100 = @SETTLTPRICE100
            where IDE = @IDE";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDE", DbType.Int32));
            parameters.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal));
            parameters.Add(new DataParameter(pCS, "SETTLTQUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            parameters.Add(new DataParameter(pCS, "SETTLTQUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
            parameters.Add(new DataParameter(pCS, "DTSETTLTPRICE", DbType.DateTime));
            parameters.Add(new DataParameter(pCS, "SETTLTPRICE", DbType.Decimal));
            parameters.Add(new DataParameter(pCS, "SETTLTPRICE100", DbType.Decimal));
            parameters.SetAllDBNull();

            parameters["IDE"].Value = pIdE;
            if (pStrikePrice.HasValue) parameters["STRIKEPRICE"].Value = pStrikePrice;
            if (pSettltQuoteSide.HasValue) parameters["SETTLTQUOTESIDE"].Value = pSettltQuoteSide;
            if (pSettltQuoteTiming.HasValue) parameters["SETTLTQUOTETIMING"].Value = pSettltQuoteTiming;
            if (pDtSettltPrice.HasValue) parameters["DTSETTLTPRICE"].Value = pDtSettltPrice;
            if (pSettltPrice.HasValue) parameters["SETTLTPRICE"].Value = pSettltPrice;
            if (pSettltPrice100.HasValue) parameters["SETTLTPRICE100"].Value = pSettltPrice100;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlUpdate, parameters);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion UpdateEventDet_Denouement

        /* ---------------------------------------------------------------------- */
        /* Utilisées pour le recalcul des frais et calcul des frais manquants     */
        /* ---------------------------------------------------------------------- */

        #region DeleteFeeEvents
        /// <summary>
        /// Suppression des lignes de frais
        /// Utilisé dans :
        /// le recalcul des frais (ActionGen.FeeCalculation)
        /// le calcul des frais manquants (PosKeepingGenProcessBase.FeesCalculationGen)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// EG 20130703 New Mise en commun
        /// FI 20160907 [21831] Modify
        public static void DeleteFeeEvents(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            // FI 20160907 [21831] Appel à TradeRDBMSTools.DeleteFeeEvent
            TradeRDBMSTools.DeleteFeeEvent(pCS, pDbTransaction, pIdT);
        }
        #endregion DeleteFeeEvents
        #region DeleteSafekeepingEvent
        /// <summary>
        /// Suppression de la ligne de frais de garde de la journée de bourse en cours (pDtEvent = DTBUSINESS)
        /// Utilisé dans : Safekeeping process
        /// EG 20150708 [21103] New
        // EG 20190308 Upd
        public static void DeleteSafekeepingEvent(string pCS, IDbTransaction pDbTransaction, int pIdT, DateTime pDtEvent)
        {
            // FI 20190221 [XXXXX] reecriture du delete de manière à utiliser l'index IX_EVENT1
            string sqlDelete = @"delete from dbo.EVENT
            where (IDT = @IDT) and (EVENTCODE = @EVENTCODE) and 
            (IDE in (
            select ev.IDE 
            from dbo.EVENT ev
                inner join dbo.EVENT evp on (evp.IDE = ev.IDE_EVENT) and (evp.IDT = @IDT) and (evp.EVENTCODE = 'LPC') and (evp.EVENTTYPE = 'AMT')
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTEVENT)
                where ev.IDT = @IDT)
            )" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.SafeKeepingPayment);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENT), pDtEvent); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters qryParameters = new QueryParameters(pCS, sqlDelete, parameters);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion DeleteSafekeepingEvent

        #region InsertFeeEvents
        /// <summary>
        /// Insertion dans les tables SQL, EVENT,EVENTCLASS,EVENTFEE, ..., des lignes de frais relatives aux paiements {pPayments} 
        /// </summary>
        // EG 20160116 [POC-MUREX] New
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        public void InsertFeeEvents(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument,
            int pIdT, DateTime pDtMarket, int pIdE_Event, IPayment[] pPayments, int newIdE)
        {
            foreach (IPayment payment in pPayments)
            {
                if (null == payment.Efs_Payment)
                    payment.Efs_Payment = new EFS_Payment(pCS, pDbTransaction, payment, pDataDocument.CurrentProduct.Product, pDataDocument);

                // Ecriture des événements de frais
                InsertPaymentEvents(pDbTransaction, pDataDocument, pIdT, payment, pDtMarket, 0, 0, ref newIdE, pIdE_Event);

                newIdE++;
            }
        }
        #endregion InsertFeeEvents

        #region PrepareFeeEvents
        /// <summary>
        /// Préparation des lignes de frais avant insertion dans les tables SQL, EVENT,EVENTCLASS,EVENTFEE, ...
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pIdT"></param>
        /// <param name="paPayments"></param>
        /// <param name="opNbEvent"></param>
        /// <returns></returns>
        // EG 20160115 [POC-MUREX] New
        // EG 20180221 New 
        public static IPayment[] PrepareFeeEvents(string pCS, IProduct pProduct, DataDocumentContainer pDataDocument, int pIdT, ArrayList palPayments, 
            ref int opNbEvent)
        {
            IPayment[] payments = (IPayment[])palPayments.ToArray(typeof(IPayment));
            return PrepareFeeEvents(pCS, pProduct, pDataDocument, pIdT, payments, ref opNbEvent);
        }
        /// <summary>
        /// Préparation des lignes de frais avant insertion dans les tables SQL, EVENT,EVENTCLASS,EVENTFEE, ...
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPayments"></param>
        /// <param name="opNbEvent"></param>
        /// <returns></returns>
        // EG 20180221 Upd Signature pPayments
        public static IPayment[] PrepareFeeEvents(string pCS, IProduct pProduct, DataDocumentContainer pDataDocument, int pIdT, IPayment[] pPayments,
            ref int opNbEvent)
        {
            RptSideProductContainer container;
            if (pProduct is IExchangeTradedDerivative)
            {
                IExchangeTradedDerivative etd = pProduct as IExchangeTradedDerivative;
                container = new ExchangeTradedDerivativeContainer(pCS, etd, pDataDocument);
                ((ExchangeTradedDerivativeContainer)container).Efs_ExchangeTradedDerivative = new EFS_ExchangeTradedDerivative(pCS, etd, pDataDocument, Cst.StatusBusiness.ALLOC, pIdT);
            }
            else if (pProduct is IEquitySecurityTransaction)
            {
                IEquitySecurityTransaction eqs = pProduct as IEquitySecurityTransaction;
                container = new EquitySecurityTransactionContainer(pCS, eqs, pDataDocument);
                ((EquitySecurityTransactionContainer)container).Efs_EquitySecurityTransaction = new EFS_EquitySecurityTransaction(pCS, eqs, pDataDocument, Cst.StatusBusiness.ALLOC);
            }
            else if (pProduct is IDebtSecurityTransaction)
            {
                IDebtSecurityTransaction dst = pProduct as IDebtSecurityTransaction;
                container = new DebtSecurityTransactionContainer(pCS, null, dst, pDataDocument);
                ((DebtSecurityTransactionContainer)container).DebtSecurityTransaction.SetStreams(pCS, pDataDocument, Cst.StatusBusiness.ALLOC);
            }

            // Détermination du nombre d'IDE en fonction des événements potentiels de taxes...
            InitPaymentForEvent(pCS, pPayments, pDataDocument, out opNbEvent);
            return pPayments;
        }
        #endregion PrepareFeeEvents

        #region InitPaymentForEvent
        /// <summary>
        ///  Enrichissement de l'array pPaymentFees pour la génération des évènements (ex. alimentation des EFS_Payment)
        /// <para>Retourne également le nombre d'évènements qui auront vocation à être insérés</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPaymentFees"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="opNbEvent">Nombre d'évènements à insérer</param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public static void InitPaymentForEvent(string pCS, IPayment[] pPaymentFees, DataDocumentContainer pDataDocument, out int opNbEvent)
        {
            InitPaymentForEvent(pCS, null, pPaymentFees, pDataDocument, out opNbEvent);
        }
        /// <summary>
        ///  Enrichissement de l'array pPaymentFees pour la génération des évènements (ex. alimentation des EFS_Payment)
        /// <para>Retourne également le nombre d'évènements qui auront vocation à être insérés</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pPaymentFees"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="opNbEvent"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public static void InitPaymentForEvent(string pCS, IDbTransaction pDbTransaction, IPayment[] pPaymentFees, DataDocumentContainer pDataDocument, out int opNbEvent)
        {
            int nbEvent = 0;
            if (ArrFunc.IsFilled(pPaymentFees))
            {
                foreach (IPayment payment in pPaymentFees)
                {
                    payment.Efs_Payment = new EFS_Payment(pCS, pDbTransaction, payment, Convert.ToDateTime(null), pDataDocument.CurrentTrade.Product, pDataDocument);
                    nbEvent++;
                    if (payment.Efs_Payment.originalPaymentSpecified)
                        nbEvent += 1;
                    if (payment.Efs_Payment.taxSpecified)
                        nbEvent += payment.Efs_Payment.tax.Length;
                }
            }
            opNbEvent = nbEvent;
        }
        #endregion InitPaymentForEvent

        #region InsertPaymentEvents
        /// <summary>
        /// Insère dans les tables SQL, EVENT,EVENTPROCESS,EVENTCLASS,[EVENTFEE],..., les enregistrements relatifs au paiement {pPayment}
        /// <para>NB: pDataDocument doit être correctement alimenté</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPayment"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="opIdE">Nouvelle valeur du jeton EVENT</param>
        /// <param name="pIdEParent">Evènement parent</param>
        /// <returns></returns>
        // EG 20150616 [21124] New EventClass VAL : ValueDate
        // EG 20150706 [21021] Nullable Payer|Receiver (Actor|Book)
        // EG 20150708 [21103] New
        public Cst.ErrLevel InsertPaymentEvents(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument,
            int pIdT, IPayment pPayment, DateTime pDtBusiness, int pInstrumentNo, int pStreamNo, ref int opIdE, int pIdEParent)
        {
            return InsertPaymentEvents(pDbTransaction, pDataDocument, 
                pIdT, pPayment, EventCodeFunc.OtherPartyPayment, pDtBusiness, pInstrumentNo, pStreamNo, ref opIdE, pIdEParent);
        }
        /// <summary>
        /// Ajout d'une ligne de paiement matérialisant les frais classiques (OPP|ADP) ou les frais de garde (SKP). 
        /// <para>Insère dans les tables SQL, EVENT,EVENTPROCESS,EVENTCLASS,[EVENTFEE],..., les enregistrements relatifs au paiement {pPayment}</para>
        /// <para>NB: pDataDocument doit être correctement alimenté</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPayment"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="opIdE">Nouvelle valeur du jeton EVENT</param>
        /// <param name="pIdEParent"></param>
        /// <returns></returns>
        //EG 20150708 [21103] New Add pEventCode
        public Cst.ErrLevel InsertPaymentEvents(IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument,
            int pIdT, IPayment pPayment, string pEventCode, DateTime pDtBusiness, int pInstrumentNo, int pStreamNo, ref int opIdE, int pIdEParent)
        {
            Cst.ErrLevel codeReturn;

            #region Variables
            EFS_Payment efs_Payment = pPayment.Efs_Payment;
            int idE = opIdE;
            int idE_Event = pIdEParent;

            // EG 20150706 [21021]
            Nullable<int> idA_Payer = pDataDocument.GetOTCmlId_Party(pPayment.PayerPartyReference.HRef);
            Nullable<int> idB_Payer = pDataDocument.GetOTCmlId_Book(pPayment.PayerPartyReference.HRef);
            Nullable<int> idA_Receiver = pDataDocument.GetOTCmlId_Party(pPayment.ReceiverPartyReference.HRef);
            Nullable<int> idB_Receiver = pDataDocument.GetOTCmlId_Book(pPayment.ReceiverPartyReference.HRef);

            // EG 20150708 [21103] pEventCode = [OPP|ADP|SKP]
            string eventType = pPayment.GetPaymentType(pEventCode);
            _ = efs_Payment.ExpirationDate;
            EFS_EventDate dtPaymentDate = efs_Payment.PaymentDate;
            EFS_Date dtAdjustedPaymentDate = efs_Payment.AdjustedPaymentDate;
            EFS_Date dtPreSettlementDate = null;
            if (efs_Payment.preSettlementSpecified)
                dtPreSettlementDate = efs_Payment.preSettlement.AdjustedPreSettlementDate;
            EFS_Date dtInvoicingDate = efs_Payment.Invoicing_AdjustedPaymentDate;
            decimal paymentAmount = efs_Payment.paymentAmount.Amount.DecValue;
            string currency = efs_Payment.paymentAmount.Currency;
            #endregion Variables

            #region EVENT PAYMENT
            // EG 20120217 DTPAYMENDATE = DTSTART = DTEND
            // EG 20150708 [21103] Upd efs_Payment.dtEventStartPeriod|efs_Payment.dtEventEndPeriod
            codeReturn = InsertEvent(pDbTransaction, pIdT, idE, idE_Event, null, pInstrumentNo, pStreamNo, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                pEventCode, eventType,
                efs_Payment.dtEventStartPeriod.First, efs_Payment.dtEventStartPeriod.Second,
                efs_Payment.dtEventEndPeriod.First, efs_Payment.dtEventEndPeriod.Second,
                paymentAmount, currency, UnitTypeEnum.Currency.ToString(), null, null);

            #region EVENTCLASS
            // EG 20120220 RECOGNITION = DTBUSINESS
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtPaymentDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Recognition, pDtBusiness, false);

            // EG 20240125 [WI816] UPD : Test sur dtPaymentDate|dtAdjustedPaymentDate
            //if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtAdjustedPaymentDate))
            //    codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.ValueDate,
            //        pDataDocument.CurrentProduct.IsExchangeTradedDerivative ? pDtBusiness : dtAdjustedPaymentDate.DateValue, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
            {
                if (pDataDocument.CurrentProduct.IsExchangeTradedDerivative && (null != dtPaymentDate))
                    codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.ValueDate, pDtBusiness, false);
                else if(null != dtAdjustedPaymentDate)
                   codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.ValueDate, dtAdjustedPaymentDate.DateValue, false);
            }

            // EG 20170324 Add Test (false == isExchangeTradedDerivative)
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && efs_Payment.preSettlementSpecified && (false == pDataDocument.CurrentProduct.IsExchangeTradedDerivative))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.PreSettlement, dtPreSettlementDate.DateValue, false);
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtAdjustedPaymentDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Settlement, dtAdjustedPaymentDate.DateValue, true);
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && (null != dtInvoicingDate))
                codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Invoiced, dtInvoicingDate.DateValue, true);
            #endregion EVENTCLASS

            #region EVENTFEE
            if ((Cst.ErrLevel.SUCCESS == codeReturn) && efs_Payment.paymentSourceSpecified)
                codeReturn = InsertEventFee(pDbTransaction, idE, efs_Payment.paymentSource);
            #endregion EVENTFEE

            #endregion EVENT PAYMENT

            idE_Event = idE;
            EFS_EventDate dtExpirationDate;
            if (efs_Payment.originalPaymentSpecified)
            {
                #region ORIGINAL PAYMENT
                EFS_OriginalPayment originalPayment = efs_Payment.originalPayment;
                _ = originalPayment.ExpirationDate;
                dtPaymentDate = originalPayment.PaymentDate;
                paymentAmount = originalPayment.PaymentAmount.DecValue;
                currency = originalPayment.PaymentCurrency;
                idE++;
                // EG 20150708 [SKP]
                // EG 20150708 [SKP] Upd originalPayment.dtEventStartPeriod|originalPayment.dtEventEndPeriod
                codeReturn = InsertEvent(pDbTransaction, pIdT, idE, idE_Event, null, pInstrumentNo, pStreamNo, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                    pEventCode, eventType,
                    originalPayment.dtEventStartPeriod.First, originalPayment.dtEventStartPeriod.Second,
                    originalPayment.dtEventEndPeriod.First, originalPayment.dtEventEndPeriod.Second,
                    paymentAmount, currency, UnitTypeEnum.Currency.ToString(), null, null);
                #region EVENTCLASS
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.GroupLevel, dtPaymentDate.adjustedDate.DateValue, false);
                #endregion EVENTCLASS
                #endregion ORIGINAL PAYMENT
            }
            if (efs_Payment.taxSpecified)
            {
                #region TAX
                foreach (EFS_Tax tax in efs_Payment.tax)
                {
                    idE++;
                    dtExpirationDate = tax.ExpirationDate;
                    dtPaymentDate = tax.PaymentDate;

                    paymentAmount = tax.TaxAmount.DecValue;
                    currency = tax.TaxCurrency;
                    // EG 20150708 [SKP]
                    // EG 20150708 [SKP] Upd tax.dtEventStartPeriod|tax.dtEventEndPeriod
                    codeReturn = InsertEvent(pDbTransaction, pIdT, idE, idE_Event, null, pInstrumentNo, pStreamNo, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                        pEventCode, tax.EventType,
                        tax.dtEventStartPeriod.First, tax.dtEventStartPeriod.Second,
                        tax.dtEventEndPeriod.First, tax.dtEventEndPeriod.Second,
                        paymentAmount, currency, UnitTypeEnum.Currency.ToString(), null, null);
                    #region EVENTCLASS
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Recognition, pDtBusiness, false);

                    // EG 20150616 [21124]
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.ValueDate,
                            pDataDocument.CurrentProduct.IsExchangeTradedDerivative ? pDtBusiness : dtAdjustedPaymentDate.DateValue, false);

                    if ((Cst.ErrLevel.SUCCESS == codeReturn) && efs_Payment.preSettlementSpecified)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.PreSettlement, dtPreSettlementDate.DateValue, false);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventClass(pDbTransaction, idE, EventClassFunc.Settlement, dtPaymentDate.adjustedDate.DateValue, true);
                    #endregion EVENTCLASS

                    #region EVENTFEE
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeReturn = InsertEventFee(pDbTransaction, idE, tax.taxSource);
                    #endregion EVENTFEE

                }
                #endregion TAX
            }
            opIdE = idE;
            return codeReturn;
        }
        #endregion InsertPaymentEvents

        #region UpdateTradeXMLForFees / UpdateTradeXML
        /// <summary>
        /// Mise à jour du trade XML avec les frais (Les frais manuels sont placés avant les frais auto)
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pDataDocument">DatadocumentContainer</param>
        /// <param name="paManualPayments">Paiements manuels</param>
        /// <param name="paAutoPayments">Paiements automatiques</param>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        /// FI 20170323 [XXXXX] Modification de signature (ajout paramètres nécessaires à SaveTradeTrail)
        public static void UpdateTradeXMLForFees(IDbTransaction pDbTransaction, int pIdT, DataDocumentContainer pDataDocument, ArrayList paManualPayments, ArrayList paAutoPayments,
            DateTime pDtSys, int pIdA, AppSession pSession, int pIdTRK_L, int pIdProcess_L)
        {
            ArrayList aPayments = new ArrayList();
            aPayments.AddRange(paManualPayments);
            aPayments.AddRange(paAutoPayments);

            IPayment[] payment = (IPayment[])aPayments.ToArray(typeof(IPayment));

            // FI 20170306 [22225] call UpdateTradeXMLForFees
            UpdateTradeXMLForFees(pDbTransaction, pIdT, pDataDocument, payment, pDtSys, pIdA, pSession, pIdTRK_L, pIdProcess_L);
        }

        /// <summary>
        /// Mise à jour du trade XML avec les frais
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT">Id Système du trade</param>
        /// <param name="pDataDocument">DatadocumentContainer</param>
        /// <param name="pPayment"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pIdA"></param>
        /// <param name="pSession"></param>
        /// <param name="pIdTRK_L"></param>
        /// <param name="pIdProcess_L"></param>
        // FI 20170306 [22225] Add
        // FI 20170323 [XXXXX] Modification de signature (ajout paramètres nécessaires à SaveTradeTrail)
        public static void UpdateTradeXMLForFees(IDbTransaction pDbTransaction, int pIdT, DataDocumentContainer pDataDocument, IPayment[] pPayment,
                                                 DateTime pDtSys, int pIdA, AppSession pSession, int pIdTRK_L, int pIdProcess_L)
        {
            pDataDocument.SetOtherPartyPayment(pPayment);

            UpdateTradeXML(pDbTransaction, pIdT, pDataDocument, pDtSys, pIdA, pSession, pIdTRK_L, pIdProcess_L);
        }

        /// <summary>
        /// Mise à jour du trade XML
        /// <remarks>NB: l'appel à cette méthode est opéré lorsque le DataDocument a déjà été mis à jour.</remarks>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT">Id Système du trade</param>
        /// <param name="pDataDocument">DatadocumentContainer</param>
        /// <param name="pDtSys"></param>
        /// <param name="pIdA"></param>
        /// <param name="pSession"></param>
        /// <param name="pIdTRK_L"></param>
        /// <param name="pIdProcess_L"></param>
        // PL 20191218 [25099] New Method (Method created from UpdateTradeXMLForFees() source code)
        public static void UpdateTradeXML(IDbTransaction pDbTransaction, int pIdT, DataDocumentContainer pDataDocument, 
                                          DateTime pDtSys, int pIdA, AppSession pSession, int pIdTRK_L, int pIdProcess_L)
        {
            EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(pDataDocument.DataDocument);
            StringBuilder sb = CacheSerializer.Serialize(serializerInfo);

            TradeRDBMSTools.UpdateTradeXML(pDbTransaction, pIdT, sb, pDtSys, pIdA, pSession, pIdTRK_L, pIdProcess_L); 
        }
        #endregion UpdateTradeXMLForFees / UpdateTradeXML

        /* ---------------------------------------------------------------------- */
        /* Utilisées pour le MTM                                                  */
        /* ---------------------------------------------------------------------- */

        #region UpdateEventPricing
        /// <summary>
        /// INSERT|UPDATE EVENTPRICING
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdE"></param>
        /// <param name="pPricingValues"></param>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        public static Cst.ErrLevel UpdateEventPricing(IDbTransaction pDbTransaction, int pIdE, PricingValues pPricingValues, IProduct pProduct)
        {
            string cs = pDbTransaction.Connection.ConnectionString;
            
            string sqlSelect = @"select  count(*)  from dbo.EVENTPRICING where (IDE = @IDE)";
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDE", DbType.Int32), pIdE);
            QueryParameters qryParameters = new QueryParameters(cs, sqlSelect, parameters);
            int count = 0;
            object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (null != obj)
                count = Convert.ToInt32(obj);

            if (0 == count)
                qryParameters = QueryLibraryTools.GetQueryInsert(cs, Cst.OTCml_TBL.EVENTPRICING);
            else
                qryParameters = QueryLibraryTools.GetQueryUpdate(cs,Cst.OTCml_TBL.EVENTPRICING);
            
            parameters["IDE"].Value = pIdE;

            if (StrFunc.IsFilled(pPricingValues.Currency1))
                parameters["IDC1"].Value = pPricingValues.Currency1;
            if (StrFunc.IsFilled(pPricingValues.Currency2))
                parameters["IDC2"].Value = pPricingValues.Currency2;

            if (null != pPricingValues.DayCountFraction)
            {
                parameters["DCF"].Value = pPricingValues.DayCountFraction.DayCountFraction_FpML;
                parameters["DCFNUM"].Value = pPricingValues.DayCountFraction.Numerator;
                parameters["DCFDEN"].Value = pPricingValues.DayCountFraction.Denominator;
                parameters["TOTALOFYEAR"].Value = pPricingValues.DayCountFraction.NumberOfCalendarYears;
                parameters["TOTALOFDAY"].Value = pPricingValues.DayCountFraction.TotalNumberOfCalculatedDays;
                parameters["TIMETOEXPIRATION"].Value = pPricingValues.DayCountFraction.Factor;
            }

            if (pProduct.ProductBase.IsFxOption)
            {
                #region FxOption
                parameters["STRIKE"].Value = pPricingValues.StrikeCertain;
                parameters["VOLATILITY"].Value = pPricingValues.Volatility;
                if (pProduct.ProductBase.IsFxOptionLeg)
                {
                    parameters["EXCHANGERATE"].Value = pPricingValues.ForwardPrice ?? (pPricingValues.UnderlyingPriceCertain.HasValue?pPricingValues.UnderlyingPriceCertain:Convert.DBNull);
                    parameters["INTERESTRATE1"].Value = pPricingValues.DomesticInterest;
                    parameters["INTERESTRATE2"].Value = pPricingValues.ForeignInterest;
                }
                else
                {
                    parameters["UNDERLYINGPRICE"].Value = pPricingValues.UnderlyingPriceCertain;
                    parameters["DIVIDENDYIELD"].Value = pPricingValues.DividendYield;
                    parameters["RISKFREEINTEREST"].Value = pPricingValues.RiskFreeInterest;
                }

                #region Call values
                if (0 != pPricingValues.CallPrice)
                    parameters["CALLPRICE"].Value = pPricingValues.CallPrice;
                if (0 != pPricingValues.CallDelta)
                    parameters["CALLDELTA"].Value = pPricingValues.CallDelta;
                if (0 != pPricingValues.CallRho1)
                    parameters["CALLRHO1"].Value = pPricingValues.CallRho1;
                if (0 != pPricingValues.CallRho2)
                    parameters["CALLRHO2"].Value = pPricingValues.CallRho2;
                if (0 != pPricingValues.CallTheta)
                    parameters["CALLTHETA"].Value = pPricingValues.CallTheta;
                if (0 != pPricingValues.CallCharm)
                    parameters["CALLCHARM"].Value = pPricingValues.CallCharm;
                #endregion Call values
                #region Put values
                if (0 != pPricingValues.PutPrice)
                    parameters["PUTPRICE"].Value = pPricingValues.PutPrice;
                if (0 != pPricingValues.PutDelta)
                    parameters["PUTDELTA"].Value = pPricingValues.PutDelta;
                if (0 != pPricingValues.PutRho1)
                    parameters["PUTRHO1"].Value = pPricingValues.PutRho1;
                if (0 != pPricingValues.PutRho2)
                    parameters["PUTRHO2"].Value = pPricingValues.PutRho2;
                if (0 != pPricingValues.PutTheta)
                    parameters["PUTTHETA"].Value = pPricingValues.PutTheta;
                if (0 != pPricingValues.PutCharm)
                    parameters["PUTCHARM"].Value = pPricingValues.PutCharm;
                #endregion Put values
                #region Others values
                if (0 != pPricingValues.Gamma)
                    parameters["GAMMA"].Value = pPricingValues.Gamma;
                if (0 != pPricingValues.Vega)
                    parameters["VEGA"].Value = pPricingValues.Vega;
                if (0 != pPricingValues.Color)
                    parameters["COLOR"].Value = pPricingValues.Color;
                if (0 != pPricingValues.Speed)
                    parameters["SPEED"].Value = pPricingValues.Speed;
                if (0 != pPricingValues.Vanna)
                    parameters["VANNA"].Value = pPricingValues.Vanna;
                if (0 != pPricingValues.Volga)
                    parameters["VOLGA"].Value = pPricingValues.Volga;
                #endregion Others values

                #endregion FxOption
            }
            else if (pProduct.ProductBase.IsFxLeg)
            {
                #region FxLeg
                if (null != pPricingValues.DayCountFraction2)
                {
                    parameters["DCF2"].Value = pPricingValues.DayCountFraction2.DayCountFraction_FpML;
                    parameters["DCFNUM2"].Value = pPricingValues.DayCountFraction2.Numerator;
                    parameters["DCFDEN2"].Value = pPricingValues.DayCountFraction2.Denominator;
                    parameters["TOTALOFYEAR2"].Value = pPricingValues.DayCountFraction2.NumberOfCalendarYears;
                    parameters["TOTALOFDAY2"].Value = pPricingValues.DayCountFraction2.TotalNumberOfCalculatedDays;
                    parameters["TIMETOEXPIRATION2"].Value = pPricingValues.DayCountFraction2.Factor;
                }

                parameters["EXCHANGERATE"].Value = pPricingValues.ForwardPrice ?? Convert.DBNull;
                parameters["INTERESTRATE1"].Value = pPricingValues.InterestRate1 ?? Convert.DBNull;
                parameters["INTERESTRATE2"].Value = pPricingValues.InterestRate2 ?? Convert.DBNull;
                parameters["SPOTRATE"].Value = pPricingValues.SpotRate ?? Convert.DBNull;

                #endregion FxLeg
            }

            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion WriteEventPricing
        #endregion Methods
    }
    #endregion EventQuery
}

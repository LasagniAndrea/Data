#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{

    #region TradeInfo
    /// <summary>
    ///  Représente les informations issues d'un trade nécessaires à la génération des évènements
    /// </summary>
    public class TradeInfo
    {
        #region Members
        public int idT;
        public string identifier;
        public int idI;
        public Cst.StatusBusiness statusBusiness;
        public EFS_TradeLibrary tradeLibrary;
        public string GProduct;
        #endregion Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pGProduct"></param>
        /// <returns></returns>
        public static TradeInfo LoadTradeInfo(string pCS, IDbTransaction pDbTransaction, int pIdT, string pGProduct)
        {
            string sqlSelect = @"select t.IDT, t.IDI, t.IDENTIFIER,  t.IDSTBUSINESS
            from dbo.TRADE t
            where t.IDT = @IDT";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dp);

            TradeInfo ret = new TradeInfo();

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    ret.idT = Convert.ToInt32(dr["IDT"]);
                    ret.identifier = Convert.ToString(dr["IDENTIFIER"]);
                    ret.idI = Convert.ToInt32(dr["IDI"]);
                    ret.statusBusiness = (Cst.StatusBusiness)Enum.Parse(typeof(Cst.StatusBusiness), dr["IDSTBUSINESS"].ToString());
                }
            }

            ret.tradeLibrary = new EFS_TradeLibrary(pCS, pDbTransaction, pIdT);
            ret.GProduct = pGProduct;
            return ret;
        }
    }
    #endregion TradeInfo

    /// <summary>
    /// Class chargée de générer les évènements d'un trade
    /// </summary>
    public class EventsGenTrade
    {
        #region Members
        private int m_MultiplierIdE;
        private int m_NbTaxDet;
        /// <summary>
        /// EventMatrix by product
        /// </summary>
        /// FI 20180919 [23976] add _cashEventMatrix
        private readonly Dictionary<String, EFS_EventMatrix> _cacheEventMatrix = new Dictionary<string, EFS_EventMatrix>();
        private readonly static object _cacheEventMatrixLock = new object();
        
        #endregion Members
        #region Accessors
        /// <summary>
        /// Représente le trade
        /// </summary>
        /// FI 20180919 [23976] add
        public TradeInfo TradeInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180919 [23976] add
        private EventQuery EventQuery
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit un indicateur qui permet de supprimer les évènements existants avant la génération
        /// <para>La suppression des évènements ne doit être effectué qu'en modification de trade</para>
        /// </summary>
        public bool IsDelEvents
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou définit un indicateur qui permet de mettre à jour le Trade XML
        /// </summary>
        // PL 20191218 [25099] New
        public bool IsUpdTradeXMLForFees
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient le process qui génère les évènements
        /// </summary>
        public ProcessBase Process
        {
            get;
            private set;
        }

        

        /// <summary>
        /// 
        /// </summary>
        private string CS
        {
            get { return Process.Cs; }
        }
        #endregion Accessors
        #region Constructors
        public EventsGenTrade(ProcessBase pProcess)
        {
            Process = pProcess;
        }
        #endregion Constructors
        #region Methods

        #region Generation
        /// <summary>
        /// Génère les évènements
        /// 1. Construction de la matrice (XSLT) des événements en fonction de la nature du trade (PRODUCT) (si nécessaire)
        /// 2. Génération des événements
        /// 3. Ecriture des événements
        /// </summary>
        /// <param name="pGProduct"></param>
        /// <returns>retourne SUCCESS ou  NOTHINGTODO si le trade a déjà des événements (uniquement si isDelEvents)</returns>
        /// Refactoring Alimentation de TradeInfo.tradeLibrary.EventMatrix uniquement losque non renseigné 
        public Cst.ErrLevel Generation()
        {
            if (null == TradeInfo)
                throw new NullReferenceException("TradeInfo is null");
            
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            string logTradeIdentifier = LogTools.IdentifierAndId(TradeInfo.identifier, TradeInfo.idT);

            if ((false == IsDelEvents) && TradeRDBMSTools.IsEventExist(CS, Process.SlaveDbTransaction, TradeInfo.idT, string.Empty))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.LOG, 506), 0, new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.CurrentId))));
                codeReturn = Cst.ErrLevel.NOTHINGTODO;

                Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusEnum.WARNING);
            }

            if (!(codeReturn == Cst.ErrLevel.NOTHINGTODO))
            {
                ITrade trade = TradeInfo.tradeLibrary.CurrentTrade;

                if ((null == TradeInfo.tradeLibrary.EventMatrix))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 503), 4, new LogParam(logTradeIdentifier)));

                    string key = TradeInfo.tradeLibrary.Product.ProductBase.ProductName;
                    lock (_cacheEventMatrixLock)
                    {
                        if (false == _cacheEventMatrix.ContainsKey(key))
                        {
                            string path = Process.AppInstance.MapPath(@"EventsGen");

                            string xslFileName = "Trade";
                            if (TradeInfo.GProduct.ToUpper() == ProductTools.GroupProductEnum.Administrative.ToString().ToUpper())
                                xslFileName = "InvoiceTrade";
                            else if (TradeInfo.GProduct.ToUpper() == ProductTools.GroupProductEnum.Risk.ToString().ToUpper())
                                xslFileName = "RiskTrade";

                            TradeInfo.tradeLibrary.EventsMatrixConstruction(Process.Cs, path, xslFileName);
                            _cacheEventMatrix.Add(key, TradeInfo.tradeLibrary.EventMatrix);
                        }
                        else
                            TradeInfo.tradeLibrary.EventMatrix = _cacheEventMatrix[key];
                    }
                }

                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 504), 4, new LogParam(logTradeIdentifier)));
                    
                    trade.ProductEvents = new EFS_Events(TradeInfo.tradeLibrary);
                    trade.ProductEvents.Calc();
                }

                if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 505), 4, new LogParam(logTradeIdentifier)));
                    codeReturn = Write();
                }

                if (ProcessStateTools.IsCodeReturnUnsuccessful(codeReturn) || (null == trade.ProductEvents.events))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 542), 4, new LogParam(logTradeIdentifier)));
                }
            }
            return codeReturn;
        }
        #endregion Generation
        #region GetAllocatedNetTurnOverAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        ///EG 20120215 Exclusion des allocations de montants NET désactivés
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20200914 [XXXXX] Suppression Inner join EVENT
        private decimal GetAllocatedNetTurnOverAmount(IDbTransaction pDbTransaction, int pIdT, string pEventType)
        {
            decimal totalAllocatedNetTurnOverAmount = 0;
            string sqlQuery = @"select ev.IDE, ev.VALORISATION from dbo.EVENT ev " + DataHelper.SQLNOLOCK(CS) + Cst.CrLf;
            sqlQuery += @"inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = @EVENTCLASS)
                where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ev.EVENTTYPE = @EVENTTYPE) and (ev.IDSTACTIVATION = @IDSTACTIVATION)" + Cst.CrLf;

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(CS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventClassFunc.Settlement);
            parameters.Add(new DataParameter(CS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.LinkedProductPayment);
            parameters.Add(new DataParameter(CS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventType);
            parameters.Add(new DataParameter(CS, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusActivation.REGULAR.ToString());

            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
            // RD 20151029 [21495] Use pDbTransaction instead CS
            using (IDataReader dr = DataHelper.ExecuteReader(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["VALORISATION"]))
                        totalAllocatedNetTurnOverAmount += Convert.ToDecimal(dr["VALORISATION"]);
                }
            }
            return totalAllocatedNetTurnOverAmount;
        }
        #endregion GetAllocatedNetTurnOverAmount
        #region GetCreditNoteNetTurnOverAmount
        private decimal GetCreditNoteNetTurnOverAmount(IDbTransaction pDbTransaction, IInvoice pInvoice, int pIdT, string pEventType)
        {
            decimal netCreditNoteTurnOverAmount = GetCreditNoteNetTurnOverIssueAmount(pDbTransaction, pIdT);
            if (EventTypeFunc.IsNetTurnOverAmount(pEventType))
            {
                if (pInvoice.NetTurnOverIssueRateSpecified)
                    netCreditNoteTurnOverAmount /= pInvoice.NetTurnOverIssueRate.DecValue;
            }
            else if (EventTypeFunc.IsNetTurnOverAccountingAmount(pEventType))
            {
                if (pInvoice.NetTurnOverAccountingRateSpecified)
                    netCreditNoteTurnOverAmount *= pInvoice.NetTurnOverAccountingRate.DecValue;
            }
            return netCreditNoteTurnOverAmount;
        }
        #endregion GetCreditNoteNetTurnOverAmount
        #region GetCreditNoteNetTurnOverIssueAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        private decimal GetCreditNoteNetTurnOverIssueAmount(IDbTransaction pDbTransaction, int pIdT)
        {
            decimal netTurnOverIssueAmount = 0;
                string sqlQuery = @"select ev.IDE, ev.VALORISATION
                from dbo.EVENT ev 
                where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ev.EVENTTYPE = @EVENTTYPE)" + Cst.CrLf;

                DataParameters parameters = new DataParameters(new DataParameter[] { });
                parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
                parameters.Add(new DataParameter(CS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.CreditNoteDates);
                parameters.Add(new DataParameter(CS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventTypeFunc.Period);

                QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["VALORISATION"]))
                        netTurnOverIssueAmount += Convert.ToDecimal(dr["VALORISATION"]);
                }
            }
            return netTurnOverIssueAmount;
        }
        #endregion GetCreditNoteNetTurnOverIssueAmount
        #region GetNbTaxDetailByAllocatedInvoice
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        private int GetNbTaxDetailByAllocatedInvoice(int pIdT)
        {
            int nbTaxDet = 0;
                string sqlQuery = @"select count(ev2.IDE) as NBTAXDET from dbo.EVENT ev " + DataHelper.SQLNOLOCK(CS) + Cst.CrLf;
                sqlQuery += @"inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = @EVENTCLASS)
                inner join dbo.EVENT ev2 ON (ev2.IDE_EVENT = ev.IDE)
                where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ev.EVENTTYPE = @EVENTTYPE)" + Cst.CrLf;

                DataParameters parameters = new DataParameters(new DataParameter[] { });
                parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), pIdT);
                parameters.Add(new DataParameter(CS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventClassFunc.Recognition);
                parameters.Add(new DataParameter(CS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.LinkedProductPayment);
                parameters.Add(new DataParameter(CS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventTypeFunc.TaxIssueAmount);

                QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    nbTaxDet += Convert.ToInt32(dr["NBTAXDET"]);
            }
            return nbTaxDet;
        }
        #endregion GetNbTaxDetailByAllocatedInvoice
        #region GetNumberOfEvents
        /// <summary>
        /// Retourne le nombre de jeton IDE à aller récupérer dans la table GETID
        /// Par défaut 1, 
        /// Une formule est utilisée pour gérer l'insertion des règlements dans les factures :
        /// 1 facture = 5 evts (ALD + NTO + NTI + NTA + (FXP ou FXL))
        ///               + (3 evts (TXO / TXI / TXA) si facture soldée)
        ///               + (n evts (TXO / TXI / TXA) * nbTaxes détail) si facture soldée)
        /// </summary>
        /// <returns></returns>
        private int GetNumberOfEvents()
        {
            EFS_TradeLibrary tradeLibrary = TradeInfo.tradeLibrary;
            ITrade trade = tradeLibrary.CurrentTrade;
            ProductContainer productContainer = tradeLibrary.DataDocument.CurrentProduct;

            int nbOfEvents = trade.ProductEvents.events.Length;
            int multiplier = 1;
            int nbTaxDet = 0;
            if (productContainer.IsInvoiceSettlement)
            {
                IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)productContainer.Product;
                if (invoiceSettlement.Efs_InvoiceSettlement.allocatedInvoiceSpecified)
                {
                    multiplier = 5;
                    if (invoiceSettlement.Efs_InvoiceSettlement.allocatedInvoiceSpecified)
                    {
                        EFS_AllocatedInvoice[] allocatedInvoices = invoiceSettlement.Efs_InvoiceSettlement.allocatedInvoice;
                        foreach (EFS_AllocatedInvoice allocatedInvoice in allocatedInvoices)
                        {
                            int idT_Invoice = allocatedInvoice.invoiceIdT;
                            nbTaxDet = GetNbTaxDetailByAllocatedInvoice(idT_Invoice);
                            if (0 < nbTaxDet)
                            {
                                multiplier = 5 + (3 + (3 * nbTaxDet));
                                break;
                            }
                        }
                        multiplier = 5 + (3 + (3 * nbTaxDet));
                    }
                    nbOfEvents += (multiplier * invoiceSettlement.Efs_InvoiceSettlement.allocatedInvoice.Length);
                }
            }
            m_MultiplierIdE = multiplier;
            m_NbTaxDet = nbTaxDet;
            return nbOfEvents;
        }
        #endregion GetNumberOfEvents
        #region GetTotalAllocatedNetTurnOverAmount
        /// <summary>
        /// Retourne le montant total alloué sur la facture
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pInvoice"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        private decimal GetTotalAllocatedNetTurnOverAmount(IDbTransaction pDbTransaction, IInvoice pInvoice, int pIdT, string pEventType)
        {
            decimal netCreditNoteTurnOverAmount = GetCreditNoteNetTurnOverAmount(pDbTransaction, pInvoice, pIdT, pEventType);
            decimal totalAllocatedNetTurnOverAmount = GetAllocatedNetTurnOverAmount(pDbTransaction, pIdT, pEventType);
            return (totalAllocatedNetTurnOverAmount - netCreditNoteTurnOverAmount);
        }
        #endregion GetTotalAllocatedNetTurnOverAmount

        #region InitParameters
        /// <summary>
        /// Initialisation de la collections des paramètres pour insertion/mise à jour des tables
        /// </summary>
        /// <param name="pTable">Enumérateur déterminant la table mise à jour (EVENT|EVENTCLASS|EVENTASSET|EVENTDET|EVENTFEE|TRADE|TRADEINSTRUMENT|TRADESTREAM)</param>
        /// EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private DataParameters InitParameters(Cst.OTCml_TBL pTable)
        {
            DataParameters ret = new DataParameters();

            switch (pTable)
            {
                case Cst.OTCml_TBL.TRADE:
                    ret.Add(new DataParameter(CS, "IDT", DbType.Int32)); 
                    ret.Add(new DataParameter(CS, "DTINADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTINUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTOUTADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTOUTUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.SetAllDBNull();
                    break;
                case Cst.OTCml_TBL.TRADEINSTRUMENT:
                    ret.Add(new DataParameter(CS, "IDT", DbType.Int32));
                    ret.Add(new DataParameter(CS, "INSTRUMENTNO", DbType.Int32));
                    ret.Add(new DataParameter(CS, "DTINADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTINUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTOUTADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTOUTUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.SetAllDBNull();
                    break;
                case Cst.OTCml_TBL.TRADESTREAM:
                    ret.Add(new DataParameter(CS, "IDT", DbType.Int32));
                    ret.Add(new DataParameter(CS, "INSTRUMENTNO", DbType.Int32));
                    ret.Add(new DataParameter(CS, "STREAMNO", DbType.Int32));
                    ret.Add(new DataParameter(CS, "DTEFFECTIVEUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTEFFECTIVEADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTTERMINATIONUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTTERMINATIONADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTINUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTINADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTOUTUNADJ", DbType.Date));  // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(CS, "DTOUTADJ", DbType.Date));  // FI 20201006 [XXXXX] DbType.Date
                    ret.SetAllDBNull();
                    break;
            }
            return ret;
        }
        #endregion InitParameters
        #region IsInvoiceFullyAllocated
        private bool IsInvoiceFullyAllocated(IDbTransaction pDbTransaction, IInvoice pInvoice, EFS_AllocatedInvoice pAllocatedInvoice)
        {
            decimal currentAllocatedIssueAmount = pAllocatedInvoice.issueAmount.Amount.DecValue;
            decimal totalAllocatedIssueAmount = GetAllocatedNetTurnOverAmount(pDbTransaction, pAllocatedInvoice.InvoiceIdT, EventTypeFunc.NetTurnOverIssueAmount);
            decimal netCreditNoteTurnOverAmount = GetCreditNoteNetTurnOverAmount(pDbTransaction, pInvoice, pAllocatedInvoice.InvoiceIdT, EventTypeFunc.NetTurnOverIssueAmount);
            return (pInvoice.NetTurnOverIssueAmount.Amount.DecValue == (totalAllocatedIssueAmount + currentAllocatedIssueAmount + netCreditNoteTurnOverAmount));
        }
        #endregion IsInvoiceFullyAllocated

        #region SetPeriod
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEvent"></param>
        /// <returns></returns>
        private Cst.ErrLevel SetPeriod(EFS_Event pEvent)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            ITrade trade = TradeInfo.tradeLibrary.CurrentTrade;
            DateTime startDate = Convert.ToDateTime(null);
            DateTime adjustedStartDate = Convert.ToDateTime(null);
            DateTime endDate = Convert.ToDateTime(null);
            DateTime adjustedEndDate = Convert.ToDateTime(null);

            foreach (EFS_Event itemEvent in trade.ProductEvents.events)
            {
                if (itemEvent.eventReferenceSpecified && (pEvent.id == itemEvent.eventReference))
                {
                    if (null == itemEvent.startDate || null == itemEvent.endDate)
                        codeReturn = SetPeriod(itemEvent);

                    if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
                    {
                        #region StartDate
                        DateTime _startDate = itemEvent.startDate.unadjustedDate.DateValue;
                        if ((Convert.ToDateTime(null) == startDate) || (startDate > _startDate))
                        {
                            startDate = _startDate;
                            adjustedStartDate = itemEvent.startDate.adjustedDate.DateValue;
                        }
                        #endregion StartDate
                        #region EndDate
                        DateTime _endDate = itemEvent.endDate.unadjustedDate.DateValue;
                        if ((Convert.ToDateTime(null) == endDate) || (endDate < _endDate))
                        {
                            endDate = _endDate;
                            adjustedEndDate = itemEvent.endDate.adjustedDate.DateValue;
                        }
                        #endregion EndDate
                    }
                }
            }
            if (ProcessStateTools.IsCodeReturnSuccess(codeReturn))
            {
                pEvent.startDate = new EFS_EventDate(startDate, adjustedStartDate);
                pEvent.endDate = new EFS_EventDate(endDate, adjustedEndDate);
            }
            return codeReturn;
        }
        #endregion SetPeriod

        #region UpdateTradeStSysUsedBy
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200914 [XXXXX] Usage de pDbTransaction
        private Cst.ErrLevel UpdateTradeStSysUsedBy(IDbTransaction pDbTransaction)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            ProductContainer productContainer = TradeInfo.tradeLibrary.DataDocument.CurrentProduct;
            if (productContainer.IsInvoiceSettlement)
            {
                IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)productContainer.Product;
                if (invoiceSettlement.AllocatedInvoiceSpecified && (0 < invoiceSettlement.AllocatedInvoice.Length))
                {
                    foreach (IAllocatedInvoice allocated in invoiceSettlement.AllocatedInvoice)
                    {
                        #region TradeStSys
                        SQL_TradeCommon sqlTradeStSys = new SQL_TradeCommon(CS, allocated.OTCmlId)
                        {
                            DbTransaction = pDbTransaction
                        };
                        string sqlQuery = sqlTradeStSys.GetQueryParameters(
                                new string[] { "IDT", "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY", "ROWATTRIBUT" }).QueryReplaceParameters;

                        DataSet ds = DataHelper.ExecuteDataset(CS, pDbTransaction, CommandType.Text, sqlQuery);
                        DataTable dt = ds.Tables[0];
                        #region Edit DataRow
                        DataRow dr = dt.Rows[0];
                        dr.BeginEdit();
                        dr["IDSTUSEDBY"] = Cst.StatusUsedBy.REGULAR.ToString();
                        dr["LIBSTUSEDBY"] = Convert.DBNull;
                        dr["DTSTUSEDBY"] = OTCmlHelper.GetDateSysUTC(CS);
                        dr["IDASTUSEDBY"] = Process.Session.IdA;
                        dr.EndEdit();
                        #endregion Edit DataRow
                        DataHelper.ExecuteDataAdapter(pDbTransaction, sqlQuery, dt);
                        #endregion TradeStSys
                    }
                }
            }
            return codeReturn;
        }
        #endregion UpdateTradeStSysUsedBy

        #region Write
        /// <summary>
        /// Ecriture des événements
        /// </summary>
        /// <returns></returns>
        /// EG 20150115 [20683] Add pIsTradeAdmin parameter
        /// FI 20160524 [XXXXX] Modify 
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20160326 [23769] Upd Surcharges pour appel via RiskPerformance (pas de queue)
        // EG 20190613 [24683] Use slaveDbTransaction
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private Cst.ErrLevel Write()
        {
            IDbTransaction dbTransaction = Process.SlaveDbTransaction;
            bool isUseLocalDbTransaction = (null == dbTransaction);

            ITrade trade = TradeInfo.tradeLibrary.CurrentTrade;
            int idT = TradeInfo.idT;

            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (IsDelEvents)
            {
                // EG 20150115 [20683]
                // FI 20160524 [XXXXX] use  mQueue.GProduct()
                // FI 20160816 [22146] passage des paramètres idA, pDateSys
                // FI 20200820 [25468] dates systemes en UTC
                TradeRDBMSTools.DeleteEvent(CS, dbTransaction, idT, TradeInfo.GProduct, Process.Session.IdA, OTCmlHelper.GetDateSysUTC(CS));
                TradeRDBMSTools.DeletePosRequest(CS, dbTransaction, idT);
            }
            // FI 20201023 [XXXXX] Mise en commentaire (Fais en amont)
            //else if (TradeRDBMSTools.IsEventExist(CS, dbTransaction, idT, string.Empty))
            //{
            //    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-00543",
            //            new ProcessState(ProcessStateTools.StatusErrorEnum),
            //            LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.CurrentId));
            //}

            //IDbTransaction dbTransaction = null;
            try
            {
                EventQuery = new EventQuery(Process.Session, Process.ProcessType, Process.Tracker.IdTRK_L);

                if (isUseLocalDbTransaction)
                    dbTransaction = DataHelper.BeginTran(CS);

                // On saute l'étape de purge des INT
                SQL_Instrument sqlInstrument = new SQL_Instrument(CS, TradeInfo.idI)
                {
                    DbTransaction = dbTransaction
                };
                if (sqlInstrument.IsNoINTEvents)
                {
                    #region INT Events purge
                    ArrayList aNoINTEvent = new ArrayList();
                    ArrayList aINTParent = new ArrayList();

                    EFS_Event itemParent = null;
                    bool isNoINTEvent;
                    // ---------------------------------------------------------------------------------------
                    //   Algorithme de suppression des Events INT et de leurs descendants                
                    // ---------------------------------------------------------------------------------------
                    // Chercher dans la liste de tous les Events
                    // Si l'Event 'E' est un Event INT alors 
                    //     Rajouter l'Event 'E' dans la liste des Events INT (aINTParent)
                    // Sinon
                    //     Chercher le Parent de l'Event 'E' dans la liste des Events non INT (aNoINTEvent)
                    //     Si le Parent de l'Event est un non INT alors 
                    //          Rajouter l'Event 'E' dans la liste des Events non INT (aNoINTEvent)
                    //     Sinon
                    //          Chercher le Parent de l'Event 'E' dans la liste des Events INT (aINTParent)
                    //          Si le Parent de l'Event 'E' est un Event INT alors
                    //              Rajouter l'Event 'E' dans la liste des Events INT (aINTParent)
                    //          Sinon
                    //              Rajouter l'Event 'E' dans la liste des Events non INT (aNoINTEvent)
                    // ---------------------------------------------------------------------------------------
                    foreach (EFS_Event itemEvent in trade.ProductEvents.events)
                    {
                        isNoINTEvent = false;
                        if (false == EventCodeFunc.IsIntermediary(itemEvent.eventKey.eventCode))
                            isNoINTEvent = true;
                        if (isNoINTEvent)
                        {
                            if (itemEvent.eventReferenceSpecified)
                            {
                                bool isParentNoINTEvent = false;

                                #region Chercher un eventuel Parent non INT
                                for (int i = aNoINTEvent.Count - 1; -1 < i; i--)
                                {
                                    itemParent = (EFS_Event)aNoINTEvent[i];
                                    if (itemEvent.eventReference == itemParent.id)
                                    {
                                        isParentNoINTEvent = true;
                                        break;
                                    }
                                }
                                #endregion

                                if (false == isParentNoINTEvent)
                                {
                                    #region Chercher un eventuel Parent INT
                                    isParentNoINTEvent = true;

                                    for (int i = aINTParent.Count - 1; -1 < i; i--)
                                    {
                                        itemParent = (EFS_Event)aINTParent[i];
                                        if (itemEvent.eventReference == itemParent.id)
                                        {
                                            isParentNoINTEvent = false;
                                            break;
                                        }
                                    }
                                    #endregion
                                }
                                isNoINTEvent = isParentNoINTEvent;
                            }
                        }
                        if (isNoINTEvent)
                            aNoINTEvent.Add(itemEvent);
                        else
                            aINTParent.Add(itemEvent);
                    }
                    trade.ProductEvents.events = (EFS_Event[])aNoINTEvent.ToArray(typeof(EFS_Event));
                    #endregion INT Events purge
                }

                //Récupération des jetons 
                int nbOfEvent = GetNumberOfEvents();
                int totalRowsAffected = 0;
                Cst.ErrLevel errLevelGetId = SQLUP.GetId(out int newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbOfEvent);
                if (isUseLocalDbTransaction)
                    DataHelper.CommitTran(dbTransaction);

                if (ProcessStateTools.IsCodeReturnSuccess(errLevelGetId))
                {
                    if (isUseLocalDbTransaction)
                        dbTransaction = DataHelper.BeginTran(CS);

                    int rowsAffected = 0;
                    int idE = newIdE;
                    int instrumentNo = 0;
                    int lastInstrumentNo = 0;
                    int streamNo = 0;
                    int lastStreamNo = 0;
                    int offsetStrategyNo = 0;
                    int multiplierStreamNo = 1;
                    int spreadStreamNo = 0;
                    ArrayList aIdEParent = new ArrayList();
                    EFS_EventParent eventParent = null;

                    #region Insert Events Loop
                    foreach (EFS_Event itemEvent in trade.ProductEvents.events)
                    {
                        #region IdEParent Setting
                        if (itemEvent.eventReferenceSpecified)
                        {
                            if (0 < aIdEParent.Count)
                            {
                                for (int i = aIdEParent.Count - 1; -1 < i; i--)
                                {
                                    eventParent = (EFS_EventParent)aIdEParent[i];
                                    if (itemEvent.eventReferenceSpecified && (itemEvent.eventReference == eventParent.key))
                                        break;
                                }
                            }
                        }
                        #endregion IdEParent Setting

                        #region Trade Code
                        if (EventCodeFunc.IsTrade(itemEvent.eventKey.eventCode))
                            SetPeriod(itemEvent);
                        #endregion Trade Code

                        #region Strategy Code
                        if (EventCodeFunc.IsStrategy(itemEvent.eventKey.eventCode))
                        {
                            streamNo = 0;
                            lastInstrumentNo = instrumentNo;
                            instrumentNo = Convert.ToInt32(itemEvent.id.Replace(itemEvent.eventKey.eventCode, string.Empty));
                            instrumentNo += offsetStrategyNo;
                            offsetStrategyNo = instrumentNo;
                        }
                        #endregion Strategy Code
                        #region Product Code
                        else if (EventCodeFunc.IsProduct(itemEvent.eventKey.eventCode))
                        {
                            streamNo = 0;
                            lastInstrumentNo = instrumentNo;
                            instrumentNo = Convert.ToInt32(itemEvent.id.Replace(itemEvent.eventKey.eventCode, string.Empty));
                            instrumentNo += offsetStrategyNo;
                        }
                        else if (EventCodeFunc.IsProductUnderlyer(itemEvent.eventKey.eventCode))
                        {
                            streamNo = 0;
                            lastInstrumentNo = instrumentNo;
                            instrumentNo += 1;
                        }
                        #endregion Product Code
                        #region Stream Code (Stream / Leg)
                        //PM 20140924 [20066][20185] Sauf si parent = CashBalanceStream
                        else if (EventCodeFunc.IsLegGroup(itemEvent.eventKey.eventCode)
                                && (false == EventCodeFunc.IsCashBalanceStream(eventParent.eventCode)))
                        {
                            // Cas des sous Stream (Application d'un multiplicateur)
                            // Ex : DebtSecurityTransactionStream d'un SpotRepoLeg/ForwardRepoLeg
                            // Stream SpotRepoLeg    : 2   Stream DebtSecurityTransactionStream : 2001..2xxx
                            // Stream ForwardRepoLeg : 3   Stream DebtSecurityTransactionStream : 3001..3xxx
                            if (EventCodeFunc.IsLegGroup(eventParent.eventCode))
                            {
                                multiplierStreamNo = 1000;
                                spreadStreamNo++;
                            }
                            else
                            {
                                multiplierStreamNo = 1;
                                spreadStreamNo = 0;
                                lastStreamNo = streamNo;
                                streamNo++;
                            }
                        }
                        #endregion Stream Code (Stream / Leg)
                        #region TradeInstrument
                        if ((lastInstrumentNo != instrumentNo) && (0 < instrumentNo))
                        {
                            lastInstrumentNo = instrumentNo;
                            if (1 == instrumentNo)
                                WriteTrade(dbTransaction, itemEvent);
                            else
                                WriteTradeInstrument(dbTransaction, itemEvent, instrumentNo);
                        }
                        #endregion TradeInstrument
                        #region TradeStream
                        if ((lastStreamNo != streamNo) && (0 < streamNo))
                        {
                            lastStreamNo = streamNo;
                            WriteTradeStream(dbTransaction, itemEvent, instrumentNo, streamNo);
                        }
                        #endregion TradeStream

                        #region WriteEvent
                        itemEvent.SetSequence(instrumentNo, streamNo, multiplierStreamNo, spreadStreamNo);
                        rowsAffected = WriteEvent(dbTransaction, itemEvent, idE, eventParent, false);
                        #endregion WriteEvent

                        #region UpdEvent and EVENTSTCHECK EVENTSTMATCH from  ProcessTuning
                        if (null != Process.ProcessTuning)
                            Process.ProcessTuning.WriteStatus(CS, dbTransaction, Tuning.TuningOutputTypeEnum.OES, itemEvent.eventKey.idE, 0);
                        #endregion Use ProcessTuning

                        #region Store Event (use to find IdEParent)
                        aIdEParent.Add(new EFS_EventParent(itemEvent, itemEvent.eventKey.idE));
                        #endregion Store Event (use to find IdEParent)

                        idE = itemEvent.eventKey.idE + 1;
                        totalRowsAffected += rowsAffected;
                    }
                    #endregion Insert Events Loop

                    #region Update TradeStSysUsedBy (Actually for InvoiceSettlement)
                    UpdateTradeStSysUsedBy(dbTransaction);
                    #endregion Update TradeStSysUsedBy (Actually for InvoiceSettlement)

                    #region Update Trade (Trade XML)
                    // PL 20191218 [25099] New
                    if (IsUpdTradeXMLForFees)
                    {
                        // FI 20200820 [25468] dates systemes en UTC
                        EventQuery.UpdateTradeXML(dbTransaction, idT, TradeInfo.tradeLibrary.DataDocument, OTCmlHelper.GetDateSysUTC(CS), Process.UserId,
                                                  Process.Session, Process.Tracker.IdTRK_L, Process.IdProcess);
                    }
                    #endregion Update Trade (Trade XML)

                    if (isUseLocalDbTransaction)
                        DataHelper.CommitTran(dbTransaction);
                }
            }
            catch (Exception)
            {
                if ((null != dbTransaction) && isUseLocalDbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
                throw;
            }

            return codeReturn;
        }
        #endregion Write

        #region Write [Trade]
        /// <summary>
        /// Mise à jour des dates IN|OUT pour un TRADE avec INSTRUMENTNO = 1
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pEvent">Evénement</param>
        /// /// <param name="pStreamNo">Séquence Instrument</param>
        /// <returns></returns>
        /// EG 20200226 New suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private int WriteTrade(IDbTransaction pDbTransaction, EFS_Event pEvent)
        {
            string sqlUpdate = @"update dbo.TRADE set 
            DTINUNADJ = @DTINUNADJ, DTINADJ = @DTINADJ, DTOUTUNADJ = @DTOUTUNADJ, DTOUTADJ = @DTOUTADJ
            where (IDT = @IDT)";

            DataParameters paramTradeInstrument = InitParameters(Cst.OTCml_TBL.TRADE);
            paramTradeInstrument["IDT"].Value = TradeInfo.idT;
            paramTradeInstrument["DTINADJ"].Value = DataHelper.GetDBData(pEvent.startDate.adjustedDate.DateValue);
            paramTradeInstrument["DTINUNADJ"].Value = DataHelper.GetDBData(pEvent.startDate.unadjustedDate.DateValue);
            paramTradeInstrument["DTOUTADJ"].Value = DataHelper.GetDBData(pEvent.endDate.adjustedDate.DateValue);
            paramTradeInstrument["DTOUTUNADJ"].Value = DataHelper.GetDBData(pEvent.endDate.unadjustedDate.DateValue);

            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, paramTradeInstrument);
            return DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion Write [Trade]

        #region Write [TradeInstrument]
        /// <summary>
        /// Mise à jour des dates IN|OUT pour un TRADE avec INSTRUMENTNO donné
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pEvent">Evénement</param>
        /// /// <param name="pStreamNo">Séquence Instrument</param>
        /// <returns></returns>
        private int WriteTradeInstrument(IDbTransaction pDbTransaction, EFS_Event pEvent, int pInstrumentNo)
        {
            string sqlUpdate = @"update dbo.TRADEINSTRUMENT set 
            DTINUNADJ = @DTINUNADJ, DTINADJ = @DTINADJ, DTOUTUNADJ = @DTOUTUNADJ, DTOUTADJ = @DTOUTADJ
            where (IDT = @IDT) and (INSTRUMENTNO = @INSTRUMENTNO)";
            DataParameters paramTradeInstrument = InitParameters(Cst.OTCml_TBL.TRADEINSTRUMENT);
            paramTradeInstrument["IDT"].Value = TradeInfo.idT;
            paramTradeInstrument["INSTRUMENTNO"].Value = pInstrumentNo;
            paramTradeInstrument["DTINADJ"].Value = DataHelper.GetDBData(pEvent.startDate.adjustedDate.DateValue);
            paramTradeInstrument["DTINUNADJ"].Value = DataHelper.GetDBData(pEvent.startDate.unadjustedDate.DateValue);
            paramTradeInstrument["DTOUTADJ"].Value = DataHelper.GetDBData(pEvent.endDate.adjustedDate.DateValue);
            paramTradeInstrument["DTOUTUNADJ"].Value = DataHelper.GetDBData(pEvent.endDate.unadjustedDate.DateValue);

            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, paramTradeInstrument);
            return DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion Write [TradeInstrument]
        #region Write [TradeStream]
        /// <summary>
        /// Mise à jour des dates EFFECTIVE|TERMINATION pour un TRADE avec INSTRUMENTNO et STREAMNO donnés
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pEvent">Evénement</param>
        /// <param name="pStreamNo">Séquence Instrument</param>
        /// <param name="pStreamNo">Séquence Stream</param>
        /// <returns></returns>
        private int WriteTradeStream(IDbTransaction pDbTransaction, EFS_Event pEvent, int pInstrumentNo, int pStreamNo)
        {
            string sqlUpdate = @"update dbo.TRADESTREAM set 
            DTEFFECTIVEADJ = @DTEFFECTIVEADJ, DTEFFECTIVEUNADJ = @DTEFFECTIVEUNADJ, 
            DTTERMINATIONADJ = @DTTERMINATIONADJ,DTTERMINATIONUNADJ = @DTTERMINATIONUNADJ, 
            DTINADJ = @DTINADJ,DTINUNADJ = @DTINUNADJ, 
            DTOUTADJ = @DTOUTADJ, DTOUTUNADJ = @DTOUTUNADJ
            where (IDT = @IDT) and (INSTRUMENTNO = @INSTRUMENTNO) and (STREAMNO = @STREAMNO)";

            DataParameters paramTradeStream = InitParameters(Cst.OTCml_TBL.TRADESTREAM);

            paramTradeStream["IDT"].Value = TradeInfo.idT;
            paramTradeStream["INSTRUMENTNO"].Value = pInstrumentNo;
            paramTradeStream["STREAMNO"].Value = pStreamNo;

            paramTradeStream["DTEFFECTIVEADJ"].Value = DataHelper.GetDBData(pEvent.startDate.adjustedDate.DateValue);
            paramTradeStream["DTEFFECTIVEUNADJ"].Value = DataHelper.GetDBData(pEvent.startDate.unadjustedDate.DateValue);
            paramTradeStream["DTTERMINATIONADJ"].Value = DataHelper.GetDBData(pEvent.endDate.adjustedDate.DateValue);
            paramTradeStream["DTTERMINATIONUNADJ"].Value = DataHelper.GetDBData(pEvent.endDate.unadjustedDate.DateValue);

            paramTradeStream["DTINADJ"].Value = DataHelper.GetDBData(pEvent.startDate.adjustedDate.DateValue);
            paramTradeStream["DTINUNADJ"].Value = DataHelper.GetDBData(pEvent.startDate.unadjustedDate.DateValue);
            paramTradeStream["DTOUTADJ"].Value = DataHelper.GetDBData(pEvent.endDate.adjustedDate.DateValue);
            paramTradeStream["DTOUTUNADJ"].Value = DataHelper.GetDBData(pEvent.endDate.unadjustedDate.DateValue);

            QueryParameters qryParameters = new QueryParameters(CS, sqlUpdate, paramTradeStream);
            return DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion Write [TradeStream]
        #region WriteEvent [Event / EventClass]
        /// <summary>
        /// Ecriture d'un événement et tables enfants (EVENTCLASS|EVENTASSET|EVENTDET|EVENTFEE)
        /// </summary>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pEvent">Evénement</param>
        /// <param name="pIdE">Id Evénement</param>
        /// <param name="pEventParent">Evénement parent</param>
        /// <param name="pIsEventOnly">Si true seule la table EVENT est alimentée</param>
        /// <returns></returns>
        /// EG 20150302 Test StartDate and endDate null on BaseCurrency Event (si null => nous ne somme pas sur ReturnSwap FOREX
        /// FI 20160921 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        private int WriteEvent(IDbTransaction pDbTransaction, EFS_Event pEvent, int pIdE, EFS_EventParent pEventParent, bool pIsEventOnly)
        {
            if ((null == pEvent.startDate) && (null == pEvent.endDate) && EventTypeFunc.IsBaseCurrency(pEvent.eventKey.eventType))
            {
                pEvent.eventKey.idE = pIdE;
                return 0;
            }

            if ((pEvent.startDate == null) || (pEvent.endDate == null))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 545), 0,
                    new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.CurrentId)),
                    new LogParam((pEvent.startDate == null ? Cst.NotAvailable : DtFunc.DateTimeToStringDateISO(pEvent.startDate.adjustedDate.DateValue))),
                    new LogParam((pEvent.endDate == null ? Cst.NotAvailable : DtFunc.DateTimeToStringDateISO(pEvent.endDate.adjustedDate.DateValue)))));

                throw new Exception("Start date or end date empty");
            }

            pEvent.eventKey.idE = pIdE;
            WriteEventSource(pDbTransaction, pEvent, pIsEventOnly);

            // EVENT,EVENTPROCESS
            int rowAffected = EventQuery.InsertEvent(pDbTransaction, TradeInfo.idT, pEvent.eventKey.idE, pEvent, pEventParent, TradeInfo.tradeLibrary.DataDocument);

            // EVENTASSET, EVENTFEE, EVENTDET Insert
            if ((null != pEvent.eventDetail) && (false == pIsEventOnly) && (0 < rowAffected))
            {
                // EG 20150129 Replace by pIdE pEvent.eventKey.idE
                if (pEvent.eventDetail.assetSpecified)
                    EventQuery.InsertEventAsset(pDbTransaction, pEvent.eventKey.idE, pEvent.eventDetail.asset);
                // EG 20150129 Replace by pIdE pEvent.eventKey.idE
                if (pEvent.eventDetail.asset2Specified && (pEvent.eventDetail.asset2.idAsset != pEvent.eventDetail.asset.idAsset))
                    EventQuery.InsertEventAsset(pDbTransaction, pEvent.eventKey.idE, pEvent.eventDetail.asset2);
                // EG 20150129 Replace by pIdE pEvent.eventKey.idE
                if (pEvent.eventDetail.paymentSourceSpecified)
                    EventQuery.InsertEventFee(pDbTransaction, pEvent.eventKey.idE, pEvent.eventDetail.paymentSource);
                // EG 20150129 Replace by pIdE pEvent.eventKey.idE
                if (pEvent.eventDetail.taxSourceSpecified)
                    EventQuery.InsertEventFee(pDbTransaction, pEvent.eventKey.idE, pEvent.eventDetail.taxSource);
                // EG 20150129 Replace by pIdE pEvent.eventKey.idE
                EventQuery.InsertEventDet(pDbTransaction, pEvent.eventKey.idE, pEvent.eventDetail, pEvent.unit);
            }
            

            // EVENTCLASS Insert
            if ((false == pIsEventOnly) && (0 <= rowAffected))
            {
                foreach (EFS_EventClass item in pEvent.eventClass)
                {
                    // EG 20150129 Replace by pIdE pEvent.eventKey.idE
                    if (item.eventDateSpecified)
                        EventQuery.InsertEventClass(pDbTransaction, pEvent.eventKey.idE, item.eventClass, item.eventDate.DateValue, item.isPayment);
                }
            }
            

            //FI 20160921 [XXXXX] Mise en commentraire 
            // L'insert dans EVENTPROCESS est déjà effectuée par la méthode m_EventQuery.InsertEvent
            /*
            #region EVENTPROCESS Insert
            EventProcess eventProcess = new EventProcess(CS);
            // EG 20150129 Replace by pIdE pEvent.eventKey.idE
            eventProcess.Write(pDbTransaction, pEvent.eventKey.idE, Cst.ProcessTypeEnum.EVENTSGEN, ProcessStateTools.StatusSuccessEnum, m_EventQuery.DtSys, Process.tracker.idTRK_L);
            #endregion EVENTPROCESS Insert
             */

            return rowAffected;
        }
        #endregion WriteEvent [Event / EventClass]
        #region WriteEventSource [Invoice for InvoiceSettlement]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pEvent"></param>
        /// <param name="pIsEventOnly"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> for idA_Pay|idB_Pay|idA_Rec|idB_Rec
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Use DbTransaction
        // EG 20200914 [XXXXX] Update TRADE sur InvoiceSettlement
        private void WriteEventSource(IDbTransaction pDbTransaction, EFS_Event pEvent, bool pIsEventOnly)
        {
                EFS_EventKey eventKey = pEvent.eventKey;
                ProductContainer productContainer = TradeInfo.tradeLibrary.DataDocument.CurrentProduct;
                if (productContainer.IsInvoiceSettlement && eventKey.idE_SourceSpecified)
                {
                    int idE = eventKey.idE;

                    // ATTENTION !!!
                    // STEP1 : idE_Source contient temporairement le n° de facture associé au règlement 
                    // STEP2 : il sera remplace par la suite par le n° d'événement généré sur la facture

                    // STEP1
                    int idESource = eventKey.idE_Source;
                    // EG 20160404 Migration vs2013
                    //int tmpInt = 0;
                    EFS_AllocatedInvoice allocatedInvoice = ((IInvoiceSettlement)productContainer.Product).Efs_InvoiceSettlement[idESource];
                    if (null != allocatedInvoice)
                    {
                        DateTime receptionDate = allocatedInvoice.receptionDate.DateValue;

                        idESource = idE;

                        #region Recherche de l'événement facture (INV/PER)
                        string sqlQuery = @"select IDE, INSTRUMENTNO, STREAMNO
                        from dbo.EVENT
                        where (IDT = @IDT) and (EVENTCODE = @EVENTCODE) and (EVENTTYPE = @EVENTTYPE)" + Cst.CrLf;

                        DataParameters parameters = new DataParameters(new DataParameter[] { });
                        parameters.Add(new DataParameter(CS, "IDT", DbType.Int32), allocatedInvoice.invoiceIdT);
                        parameters.Add(new DataParameter(CS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.InvoicingDates);
                        parameters.Add(new DataParameter(CS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventTypeFunc.Period);
                        parameters.Add(new DataParameter(CS, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusActivation.REGULAR.ToString());

                        QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
                    using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            int idEParent = Convert.ToInt32(dr["IDE"]);
                            int instrumentNo = Convert.ToInt32(dr["INSTRUMENTNO"]);
                            int streamNo = Convert.ToInt32(dr["STREAMNO"]);
                            DataSetTrade ds_AllocatedInvoice = new DataSetTrade(CS, pDbTransaction, allocatedInvoice.invoiceIdT);
                            EFS_TradeLibrary tradeAllocatedInvoiceLibrary = new EFS_TradeLibrary(CS, null, ds_AllocatedInvoice.IdT);
                            IInvoice invoice = (IInvoice)tradeAllocatedInvoiceLibrary.Product;
                            bool isInvoiceFullyAllocated = IsInvoiceFullyAllocated(pDbTransaction, invoice, allocatedInvoice);
                            bool isTaxGenerated = invoice.TaxSpecified && isInvoiceFullyAllocated;

                            #region Insertion Evénement ALD sur la facture
                            // Payer/Receiver
                            // EG 20150706 [21021]
                            Nullable<int> idA_Pay = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Party(allocatedInvoice.payerPartyReference.HRef);
                            Nullable<int> idB_Pay = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Book(allocatedInvoice.payerPartyReference.HRef);
                            Nullable<int> idA_Rec = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Party(allocatedInvoice.receiverPartyReference.HRef);
                            Nullable<int> idB_Rec = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Book(allocatedInvoice.receiverPartyReference.HRef); 

                            Nullable<int> idE_Source = idE + (isTaxGenerated ? m_MultiplierIdE : 5);

                            EventQuery.InsertEvent(pDbTransaction, allocatedInvoice.invoiceIdT, idE, idEParent, idE_Source,
                                instrumentNo, streamNo, idA_Pay, idB_Pay, idA_Rec, idB_Rec,
                                EventCodeFunc.AllocatedInvoiceDates, EventTypeFunc.CashSettlement, receptionDate, receptionDate, receptionDate, receptionDate,
                                null, null, null, StatusCalculEnum.CALC, null);

                            if (false == pIsEventOnly)
                                EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.GroupLevel, receptionDate, false);

                            idEParent = idE;
                            idE++;
                            #endregion Insertion Evénement ALD sur la facture

                            #region Insertion Règlement en devise de courtage
                            WriteEvent_AllocatedInvoice(pDbTransaction, idEParent, idE, instrumentNo, streamNo, idA_Pay, idB_Pay, idA_Rec, idB_Rec,
                                EventTypeFunc.NetTurnOverAmount, invoice, allocatedInvoice, pIsEventOnly, isTaxGenerated);
                            idE++;
                            if (isTaxGenerated)
                                idE += 1 + m_NbTaxDet;
                            #endregion Insertion Règlement en devise de courtage

                            #region Insertion Règlement en devise de facturation
                            WriteEvent_AllocatedInvoice(pDbTransaction, idEParent, idE, instrumentNo, streamNo, idA_Pay, idB_Pay, idA_Rec, idB_Rec,
                                EventTypeFunc.NetTurnOverIssueAmount, invoice, allocatedInvoice, pIsEventOnly, isTaxGenerated);
                            idE++;
                            if (isTaxGenerated)
                                idE += 1 + m_NbTaxDet;
                            #endregion Insertion Règlement en devise de facturation

                            #region Insertion Règlement en devise de comptabilisation
                            WriteEvent_AllocatedInvoice(pDbTransaction, idEParent, idE, instrumentNo, streamNo, idA_Pay, idB_Pay, idA_Rec, idB_Rec,
                                EventTypeFunc.NetTurnOverAccountingAmount, invoice, allocatedInvoice, pIsEventOnly, isTaxGenerated);
                            idE++;
                            if (isTaxGenerated)
                                idE += 1 + m_NbTaxDet;
                            #endregion Insertion Règlement en devise de comptabilisation

                            #region Insertion Ecart de change

                            if (allocatedInvoice.fxGainOrLossAmountSpecified)
                            {
                                // Payer/Receiver
                                string hRefPayer = allocatedInvoice.payerPartyReference.HRef;
                                string hRefReceiver = allocatedInvoice.receiverPartyReference.HRef;
                                if (EventTypeFunc.IsForeignExchangeLoss(allocatedInvoice.FxGainOrLossType))
                                {
                                    hRefPayer = allocatedInvoice.receiverPartyReference.HRef;
                                    hRefReceiver = allocatedInvoice.payerPartyReference.HRef;
                                }
                                // EG 20150706 [21021]
                                idA_Pay = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Party(hRefPayer);
                                idB_Pay = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Book(hRefPayer);
                                idA_Rec = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Party(hRefReceiver);
                                idB_Rec = TradeInfo.tradeLibrary.DataDocument.GetOTCmlId_Book(hRefReceiver);

                                EventQuery.InsertEvent(pDbTransaction, allocatedInvoice.invoiceIdT, idE, idEParent, null,
                                    Convert.ToInt32(dr["INSTRUMENTNO"]), Convert.ToInt32(dr["STREAMNO"]), idA_Pay, idB_Pay, idA_Rec, idB_Rec,
                                    EventCodeFunc.LinkedProductPayment, allocatedInvoice.FxGainOrLossType, receptionDate, receptionDate, receptionDate, receptionDate,
                                    allocatedInvoice.fxGainOrLossAmount.Amount.DecValue, allocatedInvoice.fxGainOrLossAmount.Currency,
                                    UnitTypeEnum.Currency.ToString(), StatusCalculEnum.CALC, null);

                                if (false == pIsEventOnly)
                                    EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.Recognition, receptionDate, false);
                            }
                            idE++;
                            #endregion Insertion Ecart de change

                            #region Mise jour ROWATTRIBUT CLOSED
                            if (isInvoiceFullyAllocated)
                            {
                                ds_AllocatedInvoice.DtTrade.Rows[0]["ROWATTRIBUT"] = Cst.RowAttribut_InvoiceClosed;
                                ds_AllocatedInvoice.UpdateTrade(pDbTransaction);
                                ds_AllocatedInvoice.UpdateTradeXML(pDbTransaction);
                            }
                            #endregion Mise jour ROWATTRIBUT CLOSED

                        }
                    }
                        #endregion Recherche de l'événement facture (INV/PER)

                        // STEP2
                        eventKey.idE = idE;
                        eventKey.idE_Source = idESource;
                    }
                }
            }
        #endregion WriteEventSource [Invoice for InvoiceSettlement]
        #region WriteEvent_AllocatedInvoice [Allocated amount Invoice for InvoiceSettlement]
        /// <summary>
        /// Génération des événements sur facture après allocations sur règlement (NTO|NTI|NTA et TXO|TXI|TXA)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdEParent"></param>
        /// <param name="pIdE"></param>
        /// <param name="pEventType"></param>
        /// <param name="pInvoice"></param>
        /// <param name="pAllocatedInvoice"></param>
        /// <param name="pIsEventOnly"></param>
        /// <param name="pIsTaxGenerated"></param>
        /// <returns></returns>
        /// EG 20150129 Add IsNetTurnOverAccountingAmount test
        // EG 20200914 [XXXXX] Correction IdEParent sur taxes des factures allouées 
        private void WriteEvent_AllocatedInvoice(IDbTransaction pDbTransaction, int pIdEParent, int pIdE,
            int pInstrumentNo, int pStreamNo, Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec, string pEventType, 
            IInvoice pInvoice, EFS_AllocatedInvoice pAllocatedInvoice, bool pIsEventOnly, bool pIsTaxGenerated)
        {
            IMoney allocatedAmount = null;
            if (EventTypeFunc.IsNetTurnOverAmount(pEventType))
                allocatedAmount = pAllocatedInvoice.amount;
            else if (EventTypeFunc.IsNetTurnOverIssueAmount(pEventType))
                allocatedAmount = pAllocatedInvoice.issueAmount;
            else if (EventTypeFunc.IsNetTurnOverAccountingAmount(pEventType))
                allocatedAmount = pAllocatedInvoice.accountingAmount;

            DateTime receptionDate = pAllocatedInvoice.receptionDate.DateValue;
            EventQuery.InsertEvent(pDbTransaction, pAllocatedInvoice.invoiceIdT, pIdE, pIdEParent, null,
                pInstrumentNo, pStreamNo, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec,
                EventCodeFunc.LinkedProductPayment, pEventType, receptionDate, receptionDate, receptionDate, receptionDate,
                allocatedAmount.Amount.DecValue, allocatedAmount.Currency, UnitTypeEnum.Currency.ToString(), StatusCalculEnum.CALC, null);

            if (false == pIsEventOnly)
                EventQuery.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Settlement, receptionDate, true);

            if (pIsTaxGenerated)
                WriteTaxEvent_AllocatedInvoice(pDbTransaction, pIdE, pInstrumentNo, pStreamNo, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec,
                    pEventType, allocatedAmount.Currency, pInvoice, pAllocatedInvoice, pIsEventOnly);

        }
        #endregion WriteEvent_AllocatedInvoice [Allocated amount Invoice for InvoiceSettlement]
        #region WriteTaxEvent_AllocatedInvoice [Tax Invoice for InvoiceSettlement]
        /// <summary>
        /// Evénements de taxes sur Allocation de facture (TXO|TXI|TXA)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pInvoice"></param>
        /// <param name="pAllocatedInvoice"></param>
        /// <param name="pEventType"></param>
        /// <param name="pIsEventOnly"></param>
        /// <returns></returns>
        private void WriteTaxEvent_AllocatedInvoice(IDbTransaction pDbTransaction, int pIdE,
            int pInstrumentNo, int pStreamNo, Nullable<int> pIdA_Pay, Nullable<int> pIdB_Pay, Nullable<int> pIdA_Rec, Nullable<int> pIdB_Rec,
            string pEventType, string pCurrency, IInvoice pInvoice, EFS_AllocatedInvoice pAllocatedInvoice, bool pIsEventOnly)
        {
            DateTime receptionDate = pAllocatedInvoice.ReceptionDate.adjustedDate.DateValue;

            string eventTypeTax = EventTypeFunc.TaxAmount;
            if (EventTypeFunc.IsNetTurnOverIssueAmount(pEventType))
                eventTypeTax = EventTypeFunc.TaxIssueAmount;
            else if (EventTypeFunc.IsNetTurnOverIssueAmount(pEventType))
                eventTypeTax = EventTypeFunc.TaxAccountingAmount;

            IInvoiceTax tax = (IInvoiceTax)pInvoice.Tax;
            decimal totalAllocatedNetTurnOverAmount = GetTotalAllocatedNetTurnOverAmount(pDbTransaction, pInvoice, pAllocatedInvoice.InvoiceIdT, pEventType);
            decimal baseAmountForTax = tax.GetBaseAmountForTax(CS, totalAllocatedNetTurnOverAmount, pCurrency);
            decimal totalTaxAmount = tax.GetTotalTaxAmount(Process.Cs, baseAmountForTax, pCurrency);


            int idE = pIdE;
            int idEParent = pIdE;
            idE++;

            EventQuery.InsertEvent(pDbTransaction, pAllocatedInvoice.invoiceIdT, idE, idEParent, null,
                pInstrumentNo, pStreamNo, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec,
                EventCodeFunc.LinkedProductPayment, eventTypeTax, receptionDate, receptionDate, receptionDate, receptionDate,
                totalTaxAmount, pCurrency, UnitTypeEnum.Currency.ToString(), StatusCalculEnum.CALC, null);

            if (false == pIsEventOnly)
                EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.Settlement, receptionDate, true);

            #region TAX Events details
            int nbTaxDet = pInvoice.Tax.Details.Length;

            if (0 < nbTaxDet)
                idEParent = idE;

            for (int i = 0; i < nbTaxDet; i++)
            {
                idE++;

                ITaxSchedule taxSchedule = (ITaxSchedule)pInvoice.Tax.Details[i];
                decimal taxAmount = taxSchedule.GetTaxAmount(Process.Cs, baseAmountForTax, pCurrency);

                EventQuery.InsertEvent(pDbTransaction, pAllocatedInvoice.invoiceIdT, idE, idEParent, null,
                    pInstrumentNo, pStreamNo, pIdA_Pay, pIdB_Pay, pIdA_Rec, pIdB_Rec,
                    EventCodeFunc.LinkedProductPayment, taxSchedule.GetEventType(), receptionDate, receptionDate, receptionDate, receptionDate,
                    taxAmount, pCurrency, UnitTypeEnum.Currency.ToString(), StatusCalculEnum.CALC, null);

                if (false == pIsEventOnly)
                    EventQuery.InsertEventClass(pDbTransaction, idE, EventClassFunc.Settlement, receptionDate, true);

                EFS_TaxSource taxSource = new EFS_TaxSource(pInvoice.Tax.Details[i].TaxSource);
                EventQuery.InsertEventFee(pDbTransaction, idE, taxSource);
            }
            #endregion TAX Events details
        }
        #endregion WriteTaxEvent_AllocatedInvoice [TaxInvoice for InvoiceSettlement]
        #endregion Methods
    }
}

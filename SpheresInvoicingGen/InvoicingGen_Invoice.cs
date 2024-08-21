#region Using Directives
//
using EFS.ACommon;
using EFS.Administrative.Invoicing;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
//
#endregion Using Directives

namespace EFS.Process
{
    public class InvoicingGen_Invoice : InvoicingGenProcessBase
    {
        #region Members
        private DataSet m_DsEvent;
        #endregion Members
        #region Accessors
        #region BracketByInvoicingRules
        public DataRelation BracketByInvoicingRules
        {
            get
            {
                DataRelation ret = null;
                if (m_DsEvent.Relations.Contains("BracketByInvoicingRules"))
                    ret = m_DsEvent.Relations["BracketByInvoicingRules"];
                return ret;
            }
        }
        #endregion BracketByInvoicingRules
        #region Criteria
        public override SQL_Criteria Criteria
        {
            get { return ((InvoicingGenMQueue)Queue).criteria; }
        }
        #endregion Criteria
        #region DtEvent
        public DataTable DtEvent
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("Events"))
                    ret = m_DsEvent.Tables["Events"];
                return ret;
            }
        }
        #endregion DtEvent
        #region DtInvoicingRules
        public DataTable DtInvoicingRules
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("InvoicingRules"))
                    ret = m_DsEvent.Tables["InvoicingRules"];
                return ret;
            }
        }
        #endregion DtInvoicingRules
        #region DtInvRulesBracket
        public DataTable DtInvRulesBracket
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("InvRulesBracket"))
                    ret = m_DsEvent.Tables["InvRulesBracket"];
                return ret;
            }
        }
        #endregion DtInvRulesBracket
        #region DtTraderActor
        public DataTable DtTraderActor
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("TraderActor"))
                    ret = m_DsEvent.Tables["TraderActor"];
                return ret;
            }
        }
        #endregion DtTradeActor
        #region DtSalesActor
        public DataTable DtSalesActor
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("SalesActor"))
                    ret = m_DsEvent.Tables["SalesActor"];
                return ret;
            }
        }
        #endregion DtSalesActor

        #region DtTradeInstrument
        public DataTable DtTradeInstrument
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("TradeInstrument"))
                    ret = m_DsEvent.Tables["TradeInstrument"];
                return ret;
            }
        }
        #endregion DtTradeInstrument
        #region DtTradeStream
        public DataTable DtTradeStream
        {
            get
            {
                DataTable ret = null;
                if ((null != m_DsEvent) && m_DsEvent.Tables.Contains("TradeStream"))
                    ret = m_DsEvent.Tables["TradeStream"];
                return ret;
            }
        }
        #endregion DtTradeStream

        #region Entity
        // EG 20150706 [21021] Nullable<int>
        public override Nullable<int> Entity
        {
            get 
            {
                Nullable<int> ret = null;
                if (Queue.parametersSpecified)
                    ret = Queue.GetIntValueParameterById(InvoicingGenMQueue.PARAM_ENTITY);
                return ret;
            }
        }
        #endregion Entity
        #region InvoicingRulesByPayer
        public DataRelation InvoicingRulesByPayer
        {
            get
            {
                DataRelation ret = null;
                if (m_DsEvent.Relations.Contains("InvoicingRulesByPayer"))
                    ret = m_DsEvent.Relations["InvoicingRulesByPayer"];
                return ret;
            }
        }
        #endregion InvoicingRulesByPayer
        #region IsSimulation
        public override bool IsSimulation
        {
            get { return Queue.GetBoolValueParameterById(InvoicingGenMQueue.PARAM_ISSIMUL); }
        }
        #endregion IsSimulation
        #region MasterDate
        public override DateTime MasterDate
        {
            get { return Queue.GetMasterDate(); }
        }
        #endregion MasterDate
        #region TraderActorByTrade
        public DataRelation TraderActorByTrade
        {
            get
            {
                DataRelation ret = null;
                if (m_DsEvent.Relations.Contains("TraderActorByTrade"))
                    ret = m_DsEvent.Relations["TraderActorByTrade"];
                return ret;
            }
        }
        #endregion TraderActorByTrade
        #region SalesActorByTrade
        //20091021 FI add SalesActorByTrade
        public DataRelation SalesActorByTrade
        {
            get
            {
                DataRelation ret = null;
                if (m_DsEvent.Relations.Contains("SalesActorByTrade"))
                    ret = m_DsEvent.Relations["SalesActorByTrade"];
                return ret;
            }
        }
        #endregion SalesActorByTrade
        #region TradeStreamByTrade
        public DataRelation TradeStreamByTrade
        {
            get
            {
                DataRelation ret = null;
                if (m_DsEvent.Relations.Contains("TradeStreamByTrade"))
                    ret = m_DsEvent.Relations["TradeStreamByTrade"];
                return ret;
            }
        }
        #endregion TradeStreamByTrade
        #endregion Accessors
        #region Constructors
        public InvoicingGen_Invoice(InvoicingGenProcess pInvoicingGenProcess)
            : base(pInvoicingGenProcess)
        {
        }
        #endregion Constructors
        #region Methods
        #region Generate
        /// <summary>
        /// Méthode principale de génération d'un facture
        /// - Alimentation des données liées à l'entité 
        /// - Chargement des événements de frais des trade de marché candidats à facturation
        /// - Chargement des périmètres de facturation candidats
        /// - Appairage des événements avec un périmètre de facturation candidat (poids de contexte)
        /// - Mise à jour du DataDocument et Sauvegarde
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Generate()
        {
            try
            {
                Cst.ErrLevel codeReturn = SetInvoicing();

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    SetCurrentParallelSettings();

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SelectEventsAndInvoicingScopes();
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SetEventsIntoInvoicingScope();
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = SetDataDocumentAndSave();
                return codeReturn;
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessBase.ProcessState.AddCriticalException(ex);
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5200), 0,
                    new LogParam(LogTools.IdentifierAndId(m_Invoicing.EntityIdentifier, EntityValue)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(MasterDate)),
                    new LogParam(IsSimulation ? " (SIMUL)" : string.Empty)));

                throw;
            }
        }
        #endregion Generate
        /// <summary>
        /// Exécution de la query de sélection des événements candidats à facturation
        /// avec gestion des deadlocks
        /// </summary>
        /// <param name="pQryParameters"></param>
        /// <returns></returns>
        // EG 20220523 [WI639] New
        protected Cst.ErrLevel ExecuteSelectEvents(QueryParameters pQryParameters)
        {
            m_DsEvent = DataHelper.ExecuteDataset(ProcessBase.Cs, CommandType.Text, pQryParameters.Query, pQryParameters.Parameters.GetArrayDbParameter());
            if ((null == m_DsEvent) || (0 == m_DsEvent.Tables[0].Rows.Count))
            {
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 5201), 0,
                    new LogParam(LogTools.IdentifierAndId(m_Invoicing.EntityIdentifier, EntityValue)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

                return Cst.ErrLevel.DATANOTFOUND;
            }
            m_DsEvent.DataSetName = "EventFees";
            m_DsEvent.Tables[0].TableName = "Events";
            m_DsEvent.Tables[1].TableName = "TraderActor";
            m_DsEvent.Tables[2].TableName = "SalesActor";
            m_DsEvent.Tables[3].TableName = "InvoicingRules";
            m_DsEvent.Tables[4].TableName = "InvRulesBracket";
            //
            if ((null != DtEvent) && (null != DtTraderActor))
            {
                DataRelation relTraderActorByTrade = new DataRelation("TraderActorByTrade", DtEvent.Columns["IDT"], DtTraderActor.Columns["IDT"], false);
                m_DsEvent.Relations.Add(relTraderActorByTrade);
            }
            //
            if ((null != DtEvent) && (null != DtSalesActor))
            {
                DataRelation relSalesActorByTrade = new DataRelation("SalesActorByTrade", DtEvent.Columns["IDT"], DtSalesActor.Columns["IDT"], false);
                m_DsEvent.Relations.Add(relSalesActorByTrade);
            }
            //
            if ((null != DtEvent) && (null != DtInvoicingRules))
            {
                DataRelation relInvoicingRulesByPayer = new DataRelation("InvoicingRulesByPayer", DtEvent.Columns["IDA_PAY"], DtInvoicingRules.Columns["IDA"], false);
                m_DsEvent.Relations.Add(relInvoicingRulesByPayer);
            }

            if ((null != DtInvoicingRules) && (null != DtInvRulesBracket))
            {
                DataRelation relBracketByInvoicingRules = new DataRelation("BracketByInvoicingRules",
                    DtInvoicingRules.Columns["IDINVOICINGRULES"], DtInvRulesBracket.Columns["IDINVOICINGRULES"], false);
                m_DsEvent.Relations.Add(relBracketByInvoicingRules);
            }

            return Cst.ErrLevel.SUCCESS;
        }
        #region SelectEventsAndInvoicingScopes
        /// <summary>
        /// Requête de sélection des frais candidats à facturation
        /// - en fonction de la date de facturation et de l'entité
        /// - éventuels filtres opérés et envoyer dans le message (PAYEUR et DEVISE)
        /// Requête de sélection des Traders et Sales des potentiels trades candidats
        /// Requête de sélection des périmètres de facturation
        /// </summary>
        /// <returns></returns>
        // EG 20091105 DTEVENT -> DTEVENTFORCED -> DTEVENT
        // EG 20110205 Add @DATE2 (Plage de sélection des factures = 3MOIS)
        // EG 20110504 Modification Query Externe de lecture dans TRADELINK (moins de colonnes dans le SELECT DISTINCT)
        //PL 20120830 Refactoring for use Execept/Minus
        // EG 20141020 [20442] 
        // RD 20151117 [21556] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20220406 [XXXXX][WI614] Left outer join on MARKET (before inner join)
        // EG 20220523 [WI639] Gestion des Deadlocks
        protected Cst.ErrLevel SelectEventsAndInvoicingScopes()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            // Insertion LOG Détail 
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5234), 0,
                new LogParam(LogTools.IdentifierAndId(m_Invoicing.EntityIdentifier, EntityValue)),
                new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

            StrBuilder sqlSelect = new StrBuilder();

            #region Parameters
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(ProcessBase.Cs, InvoicingGenMQueue.PARAM_ENTITY, DbType.Int32), IdA_Entity);
            parameters.Add(new DataParameter(ProcessBase.Cs, InvoicingGenMQueue.PARAM_DATE1, DbType.Date), InvoiceDate);
            parameters.Add(new DataParameter(ProcessBase.Cs, InvoicingGenMQueue.PARAM_DATE2, DbType.Date), InvoiceDate.AddMonths(-3));   
            parameters.Add(new DataParameter(ProcessBase.Cs, "DATE2BIS", DbType.Date), InvoiceDate.AddMonths(-4));
            #endregion Parameters

            #region Events Query
            //EG 20141020 [20442] NEW
            //PL 20141023
            //RD 20151117 [21556] Inversion du sens, correction du script pour "SIDE"
            StrBuilder sqlEvent = new StrBuilder();
            sqlEvent += @"select e.IDT, e.IDE, e.EVENTCODE, e.EVENTTYPE, e.IDA_PAY, e.IDB_PAY, e.IDA_REC, e.IDB_REC, e.INSTRUMENTNO, e.VALORISATION, e.UNIT as IDC_FEE, ec.DTEVENT, 
            ef.IDFEESCHEDULE, ef.FORMULADCF, ef.ASSESSMENTBASISVALUE1, ef.ASSESSMENTBASISVALUE2, 
            ar_payer.IDA as IDA_INVOICED, 
            fee.IDFEE, fee.PAYMENTTYPE,
            fsch.IDENTIFIER as FEESCHEDULE_IDENTIFIER, 
            t.IDENTIFIER as TRADE_IDENTIFIER, t.IDI, t.DTTRADE, t.DTTIMESTAMP, t.DTINADJ, t.DTOUTADJ, t.IDM, 
            p.GPRODUCT, p.FAMILY, p.IDP,
            case e.IDA_PAY when t.IDA_BUYER  then t.IDA_SELLER
                           when t.IDA_SELLER then t.IDA_BUYER
            else
            case e.IDA_REC when t.IDA_BUYER  then t.IDA_SELLER
                           when t.IDA_SELLER then t.IDA_BUYER else 0 end end as COUNTERPARTY,

            case e.IDA_PAY when t.IDA_BUYER  then 'Buyer'
                           when t.IDA_SELLER then 'Seller'
            else
            case e.IDA_REC when t.IDA_BUYER  then 'Buyer'
                           when t.IDA_SELLER then 'Seller' else 'NA' end end as SIDE,

            a_payer.IDENTIFIER as PAYER_IDENTIFIER, i.IDENTIFIER as INSTR_IDENTIFIER, mk.IDENTIFIER as MARKET_IDENTIFIER, 
            ci.IDXC as IDXC, ci.CONTRACTCATEGORY, ci.CONTRACT_IDENTIFIER,
            ig.IDGINSTR, cg.IDGCONTRACT, mg.IDGMARKET, bg_payer.IDGBOOK, 
            t_inv.IDSTENVIRONMENT as IDSTENVIRONMENT_INVOICE,t_inv.IDT as IDT_INVOICE" + Cst.CrLf;

            #region Code de cette région quasi-identique à celui du fichier XML pour le Web (à l'usage de @DATE2 et @DATE2BIS près, et du ctrl de la date saisie)
            sqlEvent += @"from (
	        select e.IDE, ec.IDEC
	        from dbo.EVENT e
	        inner join dbo.EVENTCLASS ec on (ec.IDE=e.IDE) and (ec.EVENTCLASS='INV') and (ec.DTEVENT>=@DATE2) and (ec.DTEVENT<=@DATE1) 
	        /* Search and check Invoiced Office of Payer */
	        inner join dbo.BOOK b_payer on (b_payer.IDB=e.IDB_PAY)
	        inner join dbo.ACTORROLE ar_payer on (ar_payer.IDA=b_payer.IDA_INVOICEDOFFICE) and (ar_payer.IDROLEACTOR='INVOICINGOFFICE')
	        /* Check Entity of Receiver (see also where) */
	        left outer join dbo.BOOK b_receiver on (b_receiver.IDB=e.IDB_REC) and (b_receiver.IDA_ENTITY=@ENTITY)
	        /* Check if date is EOM */
            where (e.IDSTACTIVATION='REGULAR') and (isnull(b_receiver.IDA_ENTITY,e.IDA_REC)=@ENTITY)" + Cst.CrLf;

            sqlEvent += DataHelper.SQLGetExcept(ProcessBase.Cs);

            sqlEvent += @"/* ---------------------------------------------------------------- */
            /* EventSource: Fees already invoiced on a valid invoice  */
            select e_inv.IDE_SOURCE, ec.IDEC
            from dbo.TRADE t_inv
            inner join dbo.INSTRUMENT i_inv on (i_inv.IDI=t_inv.IDI)
            inner join dbo.PRODUCT p_inv on (p_inv.IDP=i_inv.IDP) and (p_inv.GPRODUCT='ADM')
            inner join dbo.EVENT e_inv on (e_inv.IDT=t_inv.IDT) 
            inner join dbo.EVENTCLASS ec on (ec.IDE=e_inv.IDE_SOURCE) and (ec.EVENTCLASS='INV') and (ec.DTEVENT>=@DATE2) and (ec.DTEVENT<=@DATE1) 
            where t_inv.DTTRADE>=@DATE2BIS and (t_inv.IDSTENVIRONMENT = 'REGULAR') and (e_inv.IDE_SOURCE is not null)
            /* ---------------------------------------------------------------- */";
            sqlEvent += ") e_candidate" + Cst.CrLf;

            #endregion

            #region Code de cette région, quasi-identique à celui du fichier XML pour le Web (jointures en plus et keywords %%SR%% en moins)

            sqlEvent += @"inner join dbo.EVENT e on (e.IDE=e_candidate.IDE)
            inner join dbo.EVENTCLASS ec on (ec.IDEC=e_candidate.IDEC)
            inner join dbo.BOOK b_payer on (b_payer.IDB=e.IDB_PAY)
            inner join dbo.ACTORROLE ar_payer on (ar_payer.IDA=b_payer.IDA_INVOICEDOFFICE) and (ar_payer.IDROLEACTOR='INVOICINGOFFICE')
            inner join dbo.TRADE t on (t.IDT=e.IDT)
            inner join dbo.INSTRUMENT i on (i.IDI=t.IDI)
            inner join dbo.PRODUCT p on (p.IDP=i.IDP)
            left outer join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = t.IDM)        
            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI=t.IDI) and (ig.IDA=ar_payer.IDA) and (ig.IDROLEGINSTR='INVOICING')
		    left outer join dbo.VW_CONTRACT_INVOICE ci on (ci.IDASSET = t.IDASSET) and (ci.ASSETCATEGORY = t.ASSETCATEGORY)
		    left outer join dbo.VW_CONTRACTG_INVOICE cg on (cg.IDASSET = t.IDASSET) and (cg.ASSETCATEGORY = t.ASSETCATEGORY) and (cg.IDA = ar_payer.IDA)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM=t.IDM) and (mg.IDA=ar_payer.IDA) and (mg.IDROLEGMARKET='INVOICING')
            left outer join dbo.BOOK b_receiver on (b_receiver.IDB=e.IDB_REC) and (b_receiver.IDA_ENTITY=@ENTITY)
            left outer join dbo.VW_GBOOKROLE bg_payer on (bg_payer.IDB = e.IDB_PAY) and (bg_payer.IDROLEGBOOK = 'INVOICING') and (bg_payer.IDA = ar_payer.IDA)
            inner join dbo.EVENTFEE ef on (ef.IDE=e.IDE)
            inner join dbo.FEE fee on (fee.IDFEE=ef.IDFEE)
            inner join dbo.ACTOR a_payer on (a_payer.IDA=e.IDA_PAY)
            left outer join dbo.FEESCHEDULE fsch on (fsch.IDFEESCHEDULE=ef.IDFEESCHEDULE)
            left outer join dbo.EVENT e_inv on (e_inv.IDE_SOURCE=e.IDE)
            left outer join dbo.TRADE t_inv on (t_inv.IDT=e_inv.IDT)
            left outer join dbo.INSTRUMENT i_inv on (i_inv.IDI=t_inv.IDI)
            left outer join dbo.PRODUCT  p_inv on (p_inv.IDP=i_inv.IDP) and (p_inv.GPRODUCT='ADM')" + Cst.CrLf;
            #endregion

            //Where additional
            string sqlWhere = m_Invoicing.parameter.GetSQLWhere(ProcessBase.Cs, "e");

            if (StrFunc.IsFilled(sqlWhere))
                sqlEvent += SQLCst.WHERE + sqlWhere;
            //Order by
            // EG 20230526 [WI640] Ajout e.IDA_REC (Cas des REBATE)
            sqlEvent += SQLCst.ORDERBY + "IDA_INVOICED, e.IDA_PAY, e.UNIT, e.IDA_REC, t.IDENTIFIER, e.IDE";
            #endregion Events Query

            #region TraderActor Query
            StrBuilder sqlTraderActor = new StrBuilder();
            sqlTraderActor += @"select distinct ta.IDT, ta.BUYER_SELLER, ta.IDA, ta.IDROLEACTOR, ta.IDB, ta.IDA_ACTOR, ta.FACTOR, a.IDENTIFIER, a.DISPLAYNAME
            from dbo.TRADEACTOR ta 
            inner join dbo.EVENT e on (e.IDT = ta.IDT)
            inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'INV') and (ec.DTEVENT >= @DATE2) and (ec.DTEVENT <= @DATE1)
            inner join dbo.ACTOR a on (a.IDA = ta.IDA)
            inner join dbo.ACTOR a_payer on (a_payer.IDA = e.IDA_PAY)
            where (ta.IDROLEACTOR = 'TRADER') and (ta.IDA_ACTOR = e.IDA_PAY) and (@ENTITY = @ENTITY) and (@DATE2BIS = @DATE2BIS)" + Cst.CrLf;
            if (StrFunc.IsFilled(sqlWhere))
                sqlTraderActor += SQLCst.AND + sqlWhere;
            #endregion TraderActor Query

            #region SalesActor Query
            //PL 20120830 Query identique à celle ci-dessus à la valeur près de IDROLACTOR --> FI, refactorer et utiliser un parameter
            StrBuilder sqlSalesActor = new StrBuilder();
            sqlSalesActor += @"select distinct ta.IDT, ta.BUYER_SELLER, ta.IDA, ta.IDROLEACTOR, ta.IDB, ta.IDA_ACTOR, ta.FACTOR, a.IDENTIFIER, a.DISPLAYNAME
            from dbo.TRADEACTOR ta 
            inner join dbo.EVENT e on (e.IDT = ta.IDT)
            inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'INV') and (ec.DTEVENT >= @DATE2) and (ec.DTEVENT <= @DATE1)
            inner join dbo.ACTOR a on (a.IDA = ta.IDA)
            inner join dbo.ACTOR a_payer on (a_payer.IDA = e.IDA_PAY)
            where (ta.IDROLEACTOR = 'SALES') and (ta.IDA_ACTOR = e.IDA_PAY) and (@ENTITY = @ENTITY) and (@DATE2BIS = @DATE2BIS)" + Cst.CrLf;
            if (StrFunc.IsFilled(sqlWhere))
                sqlSalesActor += SQLCst.AND + sqlWhere;
            #endregion SalesActor Query

            #region InvoicingRules
            // EG 20141020 [20442] 
            StrBuilder sqlInvoiceRules = new StrBuilder();
            sqlInvoiceRules += @"select ir.IDINVOICINGRULES, ir.IDENTIFIER, ir.DISPLAYNAME, ir.IDA, ir.IDA_INVOICED, ir.EVENTTYPE, 
            ir.IDC_FEE, ir.GPRODUCT, ir.TYPEINSTR, ir.IDINSTR as INSTR, ir.TYPEINSTR_UNL, ir.IDINSTR_UNL as INSTR_UNL, 
            ir.TYPEMARKETCONTRACT, ir.IDMARKETCONTRACT as MARKETCONTRACT, ir.TYPEBOOK, ir.IDBOOK as BOOK, ir.PAYMENTTYPE, ir.IDA_TRADER, ir.IDC_TRADE, 
            ir.ADDRESSIDENT, ir.IDC_INVOICING, ir.IDASSET_FXRATE_INV, ir.PERIODMLTPOFFSET, ir.PERIODOFFSET, ir.DAYTYPEOFFSET, 
            ir.PERIODMLTP, ir.PERIOD, ir.ROLLCONVENTION, ir.TAXAPPLICATION, ir.TAXCONDITION, 
            ir.RELATIVESTLDELAY, ir.PERIODMLTPSTLDELAY, ir.PERIODSTLDELAY, ir.DAYTYPESTLDELAY, ir.BDC_STLDELAY, 
            ir.MAXVALUE, ir.MAXPERIODMLTP, ir.MAXPERIOD, ir.DISCOUNTPERIODMLTP, ir.DISCOUNTPERIOD, ir.BRACKETAPPLICATION 
            from dbo.INVOICINGRULES ir" + Cst.CrLf;
            #endregion InvoicingRules

            #region InvoicingRules Brackets
            StrBuilder sqlInvoiceRulesBracket = new StrBuilder();
            sqlInvoiceRulesBracket += @"select irb.IDINVRULESBRACKET,irb.IDINVOICINGRULES, irb.IDENTIFIER, irb.DISPLAYNAME, irb.LOWVALUE, irb.HIGHVALUE, irb.DISCOUNTRATE
            from dbo.INVRULESBRACKET irb 
            inner join dbo.INVOICINGRULES ir on (ir.IDINVOICINGRULES = irb.IDINVOICINGRULES)" + Cst.CrLf;
            #endregion InvoicingRules Brackets

            #region Filter InvoicingRules and InvoicingRulesBrackets
            //PL 20120830 NB: Utilisation de critères fictifs (ex. @DATE1=@DATE1) afin d'exposer des variables indispensables aux paramètres sous ODP.NET
            StrBuilder sqlInvoiceRulesFilter = new StrBuilder();
            if (StrFunc.IsFilled(sqlWhere))
            {
                sqlInvoiceRulesFilter += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a_payer on (a_payer.IDA=ir.IDA)" + Cst.CrLf;
                sqlInvoiceRulesFilter += SQLCst.WHERE + sqlWhere.Replace("e.UNIT", "ir.IDC_FEE");
                sqlInvoiceRulesFilter += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(ProcessBase.Cs, "ir", InvoiceDate);
                sqlInvoiceRulesFilter += SQLCst.AND + "(@ENTITY=@ENTITY)" + Cst.CrLf;
            }
            else
            {
                sqlInvoiceRulesFilter += SQLCst.WHERE + "(@ENTITY=@ENTITY)" + Cst.CrLf;
            }
            sqlInvoiceRulesFilter += SQLCst.AND + "(@DATE1=@DATE1) and (@DATE2=@DATE2) and (@DATE2BIS=@DATE2BIS)" + Cst.CrLf;
            #endregion Filter InvoicingRules and InvoicingRulesBrackets
            
            #region Dataset
            sqlSelect = new StrBuilder();
            sqlSelect += sqlEvent + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += sqlTraderActor + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += sqlSalesActor + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += sqlInvoiceRules + sqlInvoiceRulesFilter + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += sqlInvoiceRulesBracket + sqlInvoiceRulesFilter + SQLCst.SEPARATOR_MULTISELECT;

            QueryParameters qryParameters = new QueryParameters(ProcessBase.Cs, sqlSelect.ToString(), parameters);

            TryMultiple tryMultiple = new TryMultiple(this.ProcessBase.Cs, "ExecuteSelectEvents", "ExecuteSelectEvents")
            {
                SetErrorWarning = ProcessBase.ProcessState.SetErrorWarning,
                IsModeTransactional = false,
                ThreadSleep = 10 //blocage de 5 secondes entre chaque tentative
            };
            codeReturn = tryMultiple.Exec<QueryParameters, Cst.ErrLevel>(ExecuteSelectEvents, qryParameters);
            #endregion Dataset
            
            return codeReturn;
        }
        #endregion SelectEventsAndInvoicingScopes
        #region SetEventsIntoInvoicingScope
        /// <summary>
        /// Ajout des événements de frais candidats à facturation dans les périmètres de facturation (scope)
        /// 
        /// Lecture de chaque événement candidat
        /// - Chargement des périmètres de facturation en fonction de l'acteur Payeur des frais
        /// - Evaluation du poids des élements du contexte en fonction de l'événement de frais 
        /// - Ajout de l'événement sur le contexte sélectionné (celui de poids le plus fort)
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel SetEventsIntoInvoicingScope()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevel codeMasterReturn = Cst.ErrLevel.UNDEFINED;
            #region Lecture des événements candidats : Chargement des périmètres et ajout événements

            // Insertion LOG Détail 
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 5235), 0,
                new LogParam(LogTools.IdentifierAndId(m_Invoicing.EntityIdentifier, EntityValue)),
                new LogParam(DtFunc.DateTimeToStringDateISO(InvoiceDate))));

            int idPayer = 0;
            int idReceiver = 0;
            string idC_Fee = string.Empty;
            foreach (DataRow rowEvent in DtEvent.Rows)
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED == codeReturn) || IRQTools.IsIRQRequested(ProcessBase, ProcessBase.IRQNamedSystemSemaphore, ref codeReturn))
                    break;

                // EG 20230526 [WI640] Ajout IDA_REC (Cas des REBATE)
                if ((idPayer != Convert.ToInt32(rowEvent["IDA_PAY"])) || (idReceiver != Convert.ToInt32(rowEvent["IDA_REC"])))
                {
                    codeReturn = Cst.ErrLevel.SUCCESS;
                    #region Invoicing Setting : Context - Rules - Brackets
                    DataRow[] rowInvoicingRules = rowEvent.GetChildRows(InvoicingRulesByPayer);
                    if ((null != rowInvoicingRules) && (0 < rowInvoicingRules.Length))
                    {
                        idPayer = Convert.ToInt32(rowEvent["IDA_PAY"]);
                        idReceiver = Convert.ToInt32(rowEvent["IDA_REC"]);
                        ArrayList aScopes = new ArrayList();
                        if (ArrFunc.IsFilled(m_Invoicing.scopes))
                            aScopes.AddRange(m_Invoicing.scopes);
                        for (int i = 0; i < rowInvoicingRules.Length; i++)
                        {
                            aScopes.Add(new InvoicingScope(m_Invoicing.entity, rowInvoicingRules[i], BracketByInvoicingRules));
                        }
                        m_Invoicing.scopes = (InvoicingScope[])aScopes.ToArray(typeof(InvoicingScope));
                    }
                    else
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                        //Aucun périmètre trouvé pour le payer
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5202), 0,
                            new LogParam(LogTools.IdentifierAndId(rowEvent["TRADE_IDENTIFIER"].ToString(), Convert.ToInt32(rowEvent["IDT"]))),
                            new LogParam(LogTools.IdentifierAndId(rowEvent["INSTR_IDENTIFIER"].ToString(), Convert.ToInt32(rowEvent["IDI"]))),
                            new LogParam(LogTools.IdentifierAndId(rowEvent["PAYER_IDENTIFIER"].ToString(), Convert.ToInt32(rowEvent["IDA_PAY"]))),
                            new LogParam(LogTools.IdentifierAndId(rowEvent["EVENTCODE"].ToString() + "-" + rowEvent["EVENTTYPE"].ToString(), Convert.ToInt32(rowEvent["IDE"]))),
                            new LogParam(StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(rowEvent["VALORISATION"])) + " " + rowEvent["IDC_FEE"].ToString())));

                        codeReturn = Cst.ErrLevel.DATANOTFOUND;
                        continue;
                    }
                    #endregion Invoicing Setting : Context - Rules - Brackets
                }
                // EG 20091110
                else if (idC_Fee != rowEvent["IDC_FEE"].ToString())
                {
                    idC_Fee = rowEvent["IDC_FEE"].ToString();
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }

                if (Cst.ErrLevel.SUCCESS == codeReturn)
                {
                    #region Evaluation du poids des élements du contexte en fonction de l'événement en cours
                    InvoicingContextEvent invoicingContextEvent = new InvoicingContextEvent(ProcessBase.Cs, rowEvent, TraderActorByTrade);
                    m_Invoicing.WeightingScope(invoicingContextEvent);
                    #endregion Evaluation du poids des élements du contexte en fonction de l'événement en cours

                    #region Ajout de l'événement dans le périmètre trouvé
                    codeReturn = m_Invoicing.AddEvent(rowEvent, invoicingContextEvent, TraderActorByTrade, SalesActorByTrade);
                    if (Cst.ErrLevel.SUCCESS == codeReturn)
                        codeMasterReturn = Cst.ErrLevel.SUCCESS;
                    #endregion Ajout de l'événement dans le périmètre trouvé
                }
            }
            // EG 20220519 [WI637] IRQ
            if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
            {
                codeMasterReturn = Cst.ErrLevel.IRQ_EXECUTED;
            }
            else if ((0 == idPayer) && (Cst.ErrLevel.SUCCESS == codeReturn))
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "SYS-05201",
                    new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND),
                    LogTools.IdentifierAndId(m_Invoicing.EntityIdentifier, EntityValue),
                    DtFunc.DateTimeToStringDateISO(InvoiceDate));
            }
            #endregion Lecture des événements candidats : Chargement des périmètres et ajout événements
            return codeMasterReturn;
        }
        #endregion SetEventsIntoInvoicingScope
        #region SetInvoicing
        /// <summary>
        /// Alimentation de la classe Invoicing avec les données du message
        /// </summary>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel SetInvoicing()
        {
            if (Queue.IsMasterDateSpecified)
            {
                m_Invoicing = new Invoicing(this);
                return Cst.ErrLevel.SUCCESS;
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessBase.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5215), 0));

                return Cst.ErrLevel.DATANOTFOUND;
            }
        }
        #endregion SetInvoicing
        #region SetCurrentParallelSettings
        /// <summary>
        /// Chargement des paramètres de multi§threading relatifs 
        /// à la génération des factures
        /// </summary>
        public void SetCurrentParallelSettings()
        {
            m_InvoicingGenProcess.CurrentParallelSettings = null;
            if (ParallelTools.GetParallelSection("parallelInvoicing") is ParallelInvoicingSection parallelSection)
                m_InvoicingGenProcess.CurrentParallelSettings = parallelSection.GetParallelSettings(m_Invoicing.EntityIdentifier);

            if (null != m_InvoicingGenProcess.CurrentParallelSettings)
            {
                bool isParallelInvoicingWriting = m_InvoicingGenProcess.IsParallelProcess(ParallelProcess.InvoicingWriting);

                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 5250), 0,
                    new LogParam(isParallelInvoicingWriting ? "YES" : "NO"),
                    new LogParam(isParallelInvoicingWriting ? Convert.ToString(ProcessBase.GetHeapSize(ParallelProcess.InvoicingWriting)) : "-"),
                    new LogParam(isParallelInvoicingWriting ? Convert.ToString(ProcessBase.GetMaxThreshold(ParallelProcess.InvoicingWriting)) : "-")));
            }
        }
        #endregion SetCurrentParallelSettings
        #endregion Methods
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.SpheresRiskPerformance.Properties;
using EFS.TradeLink;

using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Enum;


namespace EFS.SpheresRiskPerformance.CashBalanceInterest
{
    /// <summary>
    /// Data container for interest rules parameters.
    /// </summary>
    [DataContract(Name = DataHelper<InterestRule>.DATASETROWNAME,
        Namespace = DataHelper<InterestRule>.DATASETNAMESPACE)]
    internal partial class InterestRule
    {
        /// <summary>
        /// Cash Balance Actor Id
        /// </summary>
        [DataMember(Name = "IDA_CBO", Order = 1)]
        public int IdA_CBO
        { get; set; }

        /// <summary>
        /// Amount Type
        /// </summary>
        [DataMember(Name = "AMOUNTTYPE", Order = 2)]
        public string AmountType
        { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 3)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Fixed Rate
        /// </summary>
        [DataMember(Name = "FIXEDRATE", Order = 4)]
        public decimal? Fixedrate
        { get; set; }

        /// <summary>
        /// Rate Index Asset ID
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 5)]
        public int? IdAsset
        { get; set; }

        /// <summary>
        /// Rate Multiplier
        /// </summary>
        [DataMember(Name = "MULTIPLIER", Order = 6)]
        public decimal? Multiplier
        { get; set; }

        /// <summary>
        /// Spread Schedule
        /// </summary>
        [DataMember(Name = "SPREAD", Order = 7)]
        public decimal? Spread
        { get; set; }

        /// <summary>
        /// Business Center
        /// </summary>
        [DataMember(Name = "IDBC", Order = 8)]
        public string BusinessCenter
        { get; set; }

        /// <summary>
        /// Day Count Fraction
        /// </summary>
        [DataMember(Name = "DCF", Order = 9)]
        public string DayCountFraction
        { get; set; }

        /// <summary>
        /// Period
        /// </summary>
        [DataMember(Name = "PERIOD", Order = 10)]
        public string Period
        { get; set; }

        /// <summary>
        /// Period Multiplier
        /// </summary>
        [DataMember(Name = "PERIODMLTP", Order = 11)]
        public int PeriodMultiplier
        { get; set; }

        /// <summary>
        /// Minimum Threshold (Seuil Minimum)
        /// </summary>
        // PM 20130813 [MINTHRESHOLD] Ajout MINTHRESHOLD
        [DataMember(Name = "MINTHRESHOLD", Order = 12)]
        public decimal? MinimumThreshold
        { get; set; }

        /// <summary>
        /// Apply negative interest rate
        /// </summary>
        // CC 20161012 [22532] Ajout ISAPPLYNEGATIVEINT - Gestion des taux négatifs
        [DataMember(Name = "ISAPPLYNEGATIVEINT", Order = 13)]
        public bool IsApplyNegativeInt
        { get; set; }

        /// <summary>
        /// Asset Identifier
        /// </summary>
        [DataMember(Name = "ASSET_IDENTIFIER", Order = 14)]
        public string AssetIdentifier
        { get; set; }

        /// <summary>
        /// Rate Index ID
        /// </summary>
        [DataMember(Name = "IDRX", Order = 15)]
        public int? IdRateIndex
        { get; set; }

        /// <summary>
        /// Is Using Available Cash to cover deposit
        /// </summary>
        [DataMember(Name = "ISUSEAVAILABLECASH", Order = 16)]
        public bool IsUseAvailableCash
        { get; set; }
    }

    /// <summary>
    /// Data container for interest flows.
    /// </summary>
    [DataContract(Name = DataHelper<InterestFlow>.DATASETROWNAME,
        Namespace = DataHelper<InterestFlow>.DATASETNAMESPACE)]
    internal partial class InterestFlow
    {
        /// <summary>
        /// Actor Id
        /// </summary>
        [DataMember(Name = "IDA", Order = 1)]
        public int IdA
        { get; set; }

        /// <summary>
        /// Book Id
        /// </summary>
        [DataMember(Name = "IDB", Order = 2)]
        public int IdB
        { get; set; }

        /// <summary>
        /// Entity Actor Id
        /// </summary>
        [DataMember(Name = "IDA_ENTITY", Order = 3)]
        public int IdA_Entity
        { get; set; }

        /// <summary>
        /// Entity Book Id
        /// </summary>
        [DataMember(Name = "IDB_ENTITY", Order = 4)]
        public int IdB_Entity
        { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [DataMember(Name = "AMOUNT", Order = 5)]
        public decimal FlowAmount
        { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 6)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Date Business
        /// </summary>
        [DataMember(Name = "DTEVENT", Order = 7)]
        public DateTime DateBusiness
        { get; set; }

        /// <summary>
        /// Date Value
        /// </summary>
        [DataMember(Name = "DTEVENTSTL", Order = 8)]
        public DateTime? DateSTL
        { get; set; }

        /// <summary>
        /// Flux d'un Clearer ou non ?
        /// </summary>
        [DataMember(Name = "ISCLEARER", Order = 9)]
        public bool IsClearer
        { get; set; }

        /// <summary>
        /// Méthode de calcul des Cash Balances
        /// </summary>
        [DataMember(Name = "CBCALCMETHOD", Order = 10)]
        public string CBCalcMethod
        { get; set; }
        /// <summary>
        /// Méthode de calcul des Cash Balances
        /// </summary>
        public CashBalanceCalculationMethodEnum CBCalcMethodEnum
        {
            get
            {
                CashBalanceCalculationMethodEnum retCBCalcMethod = CashBalanceCalculationMethodEnum.CSBDEFAULT;
                if (StrFunc.IsFilled(CBCalcMethod))
                {
                    retCBCalcMethod = (CashBalanceCalculationMethodEnum)ReflectionTools.EnumParse(retCBCalcMethod, CBCalcMethod);
                }
                return retCBCalcMethod;
            }
        }
    }

    /// <summary>
    /// Data container for cash interest "trade" information.
    /// </summary>
    [DataContract(Name = DataHelper<InterestTrade>.DATASETROWNAME,
        Namespace = DataHelper<InterestTrade>.DATASETNAMESPACE)]
    internal partial class InterestTrade
    {
        /// <summary>
        /// Trade Identifier
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 1)]
        public String Identifier
        { get; set; }

        /// <summary>
        /// Trade Id
        /// </summary>
        [DataMember(Name = "IDT", Order = 2)]
        public int IdT
        { get; set; }

        /// <summary>
        /// Displayname
        /// </summary>
        [DataMember(Name = "DISPLAYNAME", Order = 3)]
        public string Displayname
        { get; set; }
    }

    #region static class
    /// <summary>
    /// DataContract data loading
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class DataContractLoad<T>
    {
        /// <summary>
        /// Charge les données.
        /// </summary>
        /// <param name="pConnection">Connection courante</param>
        /// <param name="pDbParametersValue">Collection de valeurs pour les paramètres de la requetes de chargement</param>
        /// <param name="pResultSets">Nom du jeux de resultats à charger</param>
        public static List<T> LoadData(IDbConnection pConnection, Dictionary<string, object> pDbParametersValue, DataContractResultSets pResultSets)
        {
            CommandType cmdTyp = DataContractHelper.GetType(pResultSets);
            string request = DataContractHelper.GetQuery(pResultSets);

            IDbDataParameter[] dbParameters = DataContractHelper.GetDbDataParameters(pResultSets, pDbParametersValue);

            return DataHelper<T>.ExecuteDataSet(pConnection, cmdTyp, request, dbParameters);
        }
    }

    /// <summary>
    /// Cash Balance Interest DataContract Helper
    /// </summary>
    internal static class CBInterestDataContractHelper
    {
        #region members
        static bool m_IsInitialized = false;
        #endregion
        #region methods
        #region Init
        /// <summary>
        /// Initialise tous les DataContractHelper
        /// </summary>
        /// <param name="pCS"></param>
        public static void InitCashBalanceInterest(string pCS)
        {
            if (false == m_IsInitialized)
            {
                InitInterestRule(pCS);
                InitCashBalance(pCS);
                InitCashPayment(pCS);
                InitCashUsed(pCS);
                InitExistTrade(pCS);
                m_IsInitialized = true;
            }
        }
        /// <summary>
        /// Initialise le DataContractHelper des règles de calcul d'intérêt
        /// </summary>
        /// <param name="pCS"></param>
        private static void InitInterestRule(string pCS)
        {
            DataContractHelper.DataContractParameter parameters;
            DataParameter paramIDA = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
            DataParameter paramIDC = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC);
            DataParameter paramAMOUNTTYPE = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.AMOUNTTYPE);
            DataParameter paramPERIOD = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PERIOD);
            DataParameter paramPERIODMLTP = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PERIODMLTP);

            parameters.Query = GetInterestRuleQuery(pCS);
            parameters.Type = CommandType.Text;
            parameters.Parameters = new DataParameter[]
                {
                    paramIDA,
                    paramIDC,
                    paramAMOUNTTYPE,
                    paramPERIOD,
                    paramPERIODMLTP
                };

            DataContractHelper.ParameterSets.Add(DataContractResultSets.INTERESTRULE_CBI, parameters);
        }
        /// <summary>
        /// Initialise le DataContractHelper des flux cash balance
        /// </summary>
        /// <param name="pCS"></param>
        private static void InitCashBalance(string pCS)
        {
            DataContractHelper.DataContractParameter parameters;
            DataParameter paramPRODUCT = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PRODUCT);
            DataParameter paramIDA = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
            DataParameter paramIDC = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC);
            DataParameter paramEVENTTYPE = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTTYPE);
            DataParameter paramDTSTART = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSTART);
            DataParameter paramDTEND = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEND);

            parameters.Query = GetCashBalanceQuery(pCS);
            parameters.Type = CommandType.Text;
            parameters.Parameters = new DataParameter[]
                {
                    paramPRODUCT,
                    paramIDA,
                    paramIDC,
                    paramEVENTTYPE,
                    paramDTSTART,
                    paramDTEND
                };

            DataContractHelper.ParameterSets.Add(DataContractResultSets.FLOWCASHBALANCE_CBI, parameters);
        }
        /// <summary>
        /// Initialise le DataContractHelper des flux cash payment
        /// </summary>
        /// <param name="pCS"></param>
        private static void InitCashPayment(string pCS)
        {
            DataContractHelper.DataContractParameter parameters;
            DataParameter paramPRODUCT = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PRODUCT);
            DataParameter paramIDA = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
            DataParameter paramIDC = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC);
            //CC/PM 20150318 Paramètre retiré de la query
            //DataParameter paramEVENTTYPE = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTTYPE);
            DataParameter paramDTSTART = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSTART);
            DataParameter paramDTEND = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEND);
            DataParameter paramISXXX = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ISXXX);

            // PM 20161115 [RATP] Ajout utilisation de GetCashPaymentQuery2
            if (Settings.Default.IsCashPaymentFromCashBalanceForInterest)
            {
                parameters.Query = GetCashPaymentQuery2(pCS);
            }
            else
            {
                parameters.Query = GetCashPaymentQuery(pCS);
            }
            parameters.Type = CommandType.Text;
            parameters.Parameters = new DataParameter[]
                {
                    paramPRODUCT,
                    paramIDA,
                    paramIDC,
                    //paramEVENTTYPE,
                    paramDTSTART,
                    paramDTEND,
                    paramISXXX
                };

            DataContractHelper.ParameterSets.Add(DataContractResultSets.FLOWCASHPAYMENT_CBI, parameters);
        }
        /// <summary>
        /// Initialise le DataContractHelper des flux cash used
        /// </summary>
        /// <param name="pCS"></param>
        private static void InitCashUsed(string pCS)
        {
            DataContractHelper.DataContractParameter parameters;
            DataParameter paramPRODUCT = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PRODUCT);
            DataParameter paramIDA = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
            DataParameter paramIDC = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC);
            DataParameter paramEVENTTYPE = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTTYPE);
            DataParameter paramDTSTART = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSTART);
            DataParameter paramDTEND = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEND);

            parameters.Query = GetCashCoveredQuery(pCS);
            parameters.Type = CommandType.Text;
            parameters.Parameters = new DataParameter[]
                {
                    paramPRODUCT,
                    paramIDA,
                    paramIDC,
                    paramEVENTTYPE,
                    paramDTSTART,
                    paramDTEND
                };

            DataContractHelper.ParameterSets.Add(DataContractResultSets.FLOWCASHCOVERED_CBI, parameters);
        }
        /// <summary>
        /// Initialise le DataContractHelper des "trades" cash interest déjà existante
        /// </summary>
        /// <param name="pCS"></param>
        private static void InitExistTrade(string pCS)
        {
            DataContractHelper.DataContractParameter parameters;
            DataParameter paramPRODUCT = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.PRODUCT);
            DataParameter paramIDA = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
            DataParameter paramIDB = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDB);
            DataParameter paramIDA_ENTITY = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY);
            DataParameter paramIDB_ENTITY = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDB_ENTITY);
            DataParameter paramIDC = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDC);
            DataParameter paramDT = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT);
            DataParameter paramAMOUNTTYPE = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.AMOUNTTYPE);

            parameters.Query = GetExistTradeQuery(pCS);
            parameters.Type = CommandType.Text;
            parameters.Parameters = new DataParameter[]
                {
                    paramPRODUCT,
                    paramIDA,
                    paramIDB,
                    paramIDA_ENTITY,
                    paramIDB_ENTITY,
                    paramIDC,
                    paramDT,
                    paramAMOUNTTYPE
                };

            DataContractHelper.ParameterSets.Add(DataContractResultSets.PREVIOUSTRADE_CBI, parameters);
        }
        #endregion
        #region queries
        /// <summary>
        /// Construit la requête des règles de calcul d'intérêts
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private static string GetInterestRuleQuery(string pCS)
        {
            StrBuilder sqlQuery = new StrBuilder();
            // CC 20161012 [22532] Ajout ISAPPLYNEGATIVEINT - Gestion des taux négatifs
            // PM 20130813 [MINTHRESHOLD] Ajout MINTHRESHOLD
            sqlQuery += SQLCst.SELECT + "ir.IDA_CBO, ir.AMOUNTTYPE, ir.IDC, ir.FIXEDRATE, ir.IDASSET, ir.MULTIPLIER, ir.SPREAD, ir.IDBC, ir.DCF, ir.PERIOD, ir.PERIODMLTP, ir.MINTHRESHOLD, ir.ISAPPLYNEGATIVEINT" + Cst.CrLf;
            sqlQuery += ", asset.IDENTIFIER as ASSET_IDENTIFIER, asset.IDRX as IDRX, cbo.ISUSEAVAILABLECASH as ISUSEAVAILABLECASH" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBINTERESTRULE.ToString() + " ir" + Cst.CrLf;
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "ir.IDA_CBO", "a", DataEnum.EnabledOnly);
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.CASHBALANCE, SQLJoinTypeEnum.Inner, "ir.IDA_CBO", "cbo", DataEnum.EnabledOnly);
            sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ASSET_RATEINDEX, SQLJoinTypeEnum.Left, "ir.IDASSET", "asset", DataEnum.EnabledOnly);
            sqlQuery += SQLCst.WHERE + "(ir.IDA_CBO = @IDA)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(('" + InterestAmountTypeEnum.CashBalance.ToString() + "' = @AMOUNTTYPE)";
            sqlQuery += SQLCst.AND + "(ir.AMOUNTTYPE in ('" + InterestAmountTypeEnum.CreditCashBalance.ToString() + "','" + InterestAmountTypeEnum.DebitCashBalance.ToString() + "'))" + Cst.CrLf;
            sqlQuery += SQLCst.OR + "(ir.AMOUNTTYPE = @AMOUNTTYPE))" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(ir.IDC = @IDC )" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(ir.PERIOD = @PERIOD )" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(ir.PERIODMLTP = @PERIODMLTP )" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(" + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ir") + ")" + Cst.CrLf;

            return sqlQuery.ToString();
        }
        /// <summary>
        /// Construit la requête des flux cash balance
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetCashBalanceQuery(string pCS)
        {
            string tmpQueryIDA = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDA_PAY else ev.IDA_REC end";
            string tmpQueryIDB = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_PAY else ev.IDB_REC end";
            string tmpQueryIDB_Entity = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_REC else ev.IDB_PAY end";
            string tmpQuerySign = @"case when ev.IDA_PAY = e_mgr.IDA_PAY then (-1) else (1) end";
            string tmpQueryIsClearer = @"case when ta_entity.IDA = e_mgr.IDA_PAY then 1 else 0 end";
            string tmpQueryCBMethod = @"case when e_eqt.EVENTTYPE is not null then 'CSBUK' else 'CSBDEFAULT' end";
            string tmpQueryInstrumentEnabled = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ns");

            string sqlSelect = @"select {0} as IDA, {1} as IDB, ta_entity.IDA as IDA_ENTITY, {2} as IDB_ENTITY,
            SUM(ev.VALORISATION * ({3})) as AMOUNT, ev.UNIT as IDC, ec.DTEVENT, null as DTEVENTSTL,
            {4} as ISCLEARER, {5} as CBCALCMETHOD
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT on (ns.IDI = tr.IDI) and ({6})
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)
            inner join dbo.TRADEACTOR ta_cbo on (ta_cbo.IDT = tr.IDT) and (ta_cbo.IDROLEACTOR = 'CSHBALANCEOFFICE') and (ta_cbo.IDA = @IDA)
            inner join dbo.TRADEACTOR ta_entity on (ta_entity.IDT = tr.IDT) and (ta_entity.IDROLEACTOR = 'ENTITY')
            inner join dbo.EVENT e_cbs on (e_cbs.IDT = tr.IDT) and (e_cbs.UNIT = @IDC) and (ev.EVENTCODE = 'CBS') and (ev.EVENTTYPE = 'AMT')
            inner join dbo.EVENT ev on (ev.IDT = e_cbs.IDT) and (ev.IDE_EVENT = e_cbs.IDE) and (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = @EVENTTYPE) and 
            (ev.STREAMNO = e_cbs.STREAMNO) and (ev.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (ev.UNIT = e_cbs.UNIT)
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = 'REC') and (ec.DTEVENT >= @DTSTART) and (ec.DTEVENT <= @DTEND)
            inner join dbo.EVENT e_mgr on (e_mgr.IDT = e_cbs.IDT) and (e_mgr.IDE_EVENT = e_cbs.IDE) and (e_mgr.EVENTCODE = 'LPC') and (e_mgr.EVENTTYPE = 'MGR') and 
            (e_mgr.STREAMNO = e_cbs.STREAMNO) and (e_mgr.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (e_mgr.UNIT = e_cbs.UNIT)
            inner join dbo.BOOK bk on (bk.IDB = {1})
            inner join dbo.EVENT e_eqt on (e_eqt.IDT = e_cbs.IDT) and (e_eqt.IDE_EVENT = e_cbs.IDE) and (e_eqt.EVENTCODE = 'LPC') and (e_eqt.EVENTTYPE = 'E_B') and 
            (e_eqt.STREAMNO = e_cbs.STREAMNO) and (e_eqt.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (e_eqt.UNIT = e_cbs.UNIT)

            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR')
            group by {0},{1}, ta_entity.IDA, {2}, ev.UNIT, ec.DTEVENT, {4}, {5}";

            return String.Format(sqlSelect, tmpQueryIDA, tmpQueryIDB, tmpQueryIDB_Entity, tmpQuerySign, tmpQueryIsClearer, tmpQueryCBMethod, tmpQueryInstrumentEnabled);
        }
        /// <summary>
        /// Construit la requête des flux cash payment
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetCashPaymentQuery(string pCS)
        {
            // ===========================================================================================
            // Rappel :
            //              Dealer	    Entité
            //              --------    --------
            // Versement	Payeur	    Receveur
            // Retrait	    Receveur	Payeur
            //
            //              Clearer	    Entité
            //              --------    --------
            // Versement	Receveur	Payeur
            // Retrait	    Payeur	    Receveur
            // ===========================================================================================

            /* On suppose qu'il s'agit d'un acteur Dealer*/
            string tmpQueryIDA = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDA_PAY else ev.IDA_REC end";
            string tmpQueryIDB = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_PAY else ev.IDB_REC end";
            string tmpQueryIDB_Entity = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_REC else ev.IDB_PAY end";
            string tmpQuerySign = @"case when (((ev.IDA_PAY = ta_entity.IDA) and (@ISXXX = 0)) or ((ev.IDA_REC = ta_entity.IDA) and (@ISXXX != 0)) then (-1) else (1) end";
            string tmpQueryInstrumentEnabled = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ns");

            string sqlSelect = @"select {0} as IDA, {1} as IDB, ta_entity.IDA as IDA_ENTITY, {2} as IDB_ENTITY,
            SUM(ev.VALORISATION * ({3})) as AMOUNT, ev.UNIT as IDC, ec.DTEVENT, ec_stl.DTEVENT as DTEVENTSTL, @ISXXX as ISCLEARER
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT on (ns.IDI = tr.IDI) and ({4})
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)
            inner join dbo.TRADEACTOR ta_entity on (ta_entity.IDT = tr.IDT) and (ta_entity.IDROLEACTOR = 'ENTITY')
            inner join dbo.TRADEACTOR ta_cbo on (ta_cbo.IDT = tr.IDT) and (ta_cbo.BUYER_SELLER is not null) and (ta_cbo.IDA != ta_entity.IDA) and (ta_cbo.IDA = @IDA)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.UNIT = @IDC) and (ev.EVENTCODE = 'STA')
            inner join dbo.EVENTCLASS ec_stl on (ec_stl.IDE = ev.IDE) and (ec_stl.EVENTCLASS = 'STL')
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = 'REC')
            inner join dbo.EVENT e_mgr on (e_mgr.IDT = e_cbs.IDT) and (e_mgr.IDE_EVENT = e_cbs.IDE) and (e_mgr.EVENTCODE = 'LPC') and (e_mgr.EVENTTYPE = 'MGR') and 
            (e_mgr.STREAMNO = e_cbs.STREAMNO) and (e_mgr.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (e_mgr.UNIT = e_cbs.UNIT)
            inner join dbo.BOOK bk on (bk.IDB = {1})
            inner join dbo.EVENT e_eqt on (e_eqt.IDT = e_cbs.IDT) and (e_eqt.IDE_EVENT = e_cbs.IDE) and (e_eqt.EVENTCODE = 'LPC') and (e_eqt.EVENTTYPE = 'E_B') and 
            (e_eqt.STREAMNO = e_cbs.STREAMNO) and (e_eqt.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (e_eqt.UNIT = e_cbs.UNIT)

            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR') and
            (((ec_stl.DTEVENT >= @DTSTART) and (ec_stl.DTEVENT <= @DTEND)) or ((ec.DTEVENT >= @DTSTART) and (ec.DTEVENT <= @DTEND)))
            group by {0},{1}, ta_entity.IDA, {2}, ev.UNIT, ec.DTEVENT, ec_stl.DTEVENT";

            return String.Format(sqlSelect, tmpQueryIDA, tmpQueryIDB, tmpQueryIDB_Entity, tmpQuerySign, tmpQueryInstrumentEnabled);
        }

        /// <summary>
        /// Construit la requête des flux cash payment
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// PM 20161115 [RATP] new pour RATP : prendre les CashPayments liés au CashBalance et pas uniquement ceux du CBO
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetCashPaymentQuery2(string pCS)
        {
            // ===========================================================================================
            // Rappel :
            //              Dealer	    Entité
            //              --------    --------
            // Versement	Payeur	    Receveur
            // Retrait	    Receveur	Payeur
            //
            //              Clearer	    Entité
            //              --------    --------
            // Versement	Receveur	Payeur
            // Retrait	    Payeur	    Receveur
            // ===========================================================================================

            /* On suppose qu'il s'agit d'un acteur Dealer*/
            string tmpQueryIDB_Entity = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_REC else ev.IDB_PAY end";
            string tmpQuerySign = @"case when (((ev.IDA_PAY = ta_entity.IDA) and (@ISXXX = 0)) or ((ev.IDA_REC = ta_entity.IDA) and (@ISXXX != 0)) then (-1) else (1) end";
            string tmpQueryInstrumentEnabled = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ns");

            string sqlSelect = @"select ta_cbo.IDA as IDA, ta_cbo.IDB as IDB, ta_entity.IDA as IDA_ENTITY, {0} as IDB_ENTITY,
            SUM(ev.VALORISATION * ({1})) as AMOUNT, ev.UNIT as IDC, ec.DTEVENT, ec_stl.DTEVENT as DTEVENTSTL, @ISXXX as ISCLEARER
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT on (ns.IDI = tr.IDI) and ({2})
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)
            inner join dbo.TRADEACTOR ta_entity on (ta_entity.IDT = tr.IDT) and (ta_entity.IDROLEACTOR = 'ENTITY')
            inner join dbo.TRADELINK tl on (tl.IDT_B = tr.IDT) and (tl.LINK = 'CashPaymentInCashBalance')
            inner join dbo.TRADE t_cb on (t_cb.IDT = tl.IDT_A)
            inner join dbo.TRADEACTOR ta_cbo on (ta_cbo.IDT = t_cb.IDT) and (ta_cbo.BUYER_SELLER is not null) and (ta_cbo.IDA != ta_entity.IDA) and (ta_cbo.IDA = @IDA)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.UNIT = @IDC) and (ev.EVENTCODE = 'STA')
            inner join dbo.EVENTCLASS ec_stl on (ec_stl.IDE = ev.IDE) and (ec_stl.EVENTCLASS = 'STL')
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = 'REC')
            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR') and
            (((ec_stl.DTEVENT >= @DTSTART) and (ec_stl.DTEVENT <= @DTEND)) or ((ec.DTEVENT >= @DTSTART) and (ec.DTEVENT <= @DTEND)))
            group by ta_cbo.IDA, ta_cbo.IDB, ta_entity.IDA, {0}, ev.UNIT, ec.DTEVENT, ec_stl.DTEVENT";

            return String.Format(sqlSelect, tmpQueryIDB_Entity, tmpQuerySign, tmpQueryInstrumentEnabled);
        }

        /// <summary>
        /// Construit la requête des flux cash covered initial margin
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetCashCoveredQuery(string pCS)
        {
            /* Le montant espèce en couverture est toujours un montant "positif" : peut importe si l'acteur est payeur ou receveur */
            string tmpQueryIDA = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDA_PAY else ev.IDA_REC end";
            string tmpQueryIDB = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_PAY else ev.IDB_REC end";
            string tmpQueryIDB_Entity = @"case when ev.IDA_REC = ta_entity.IDA then ev.IDB_REC else ev.IDB_PAY end";
            string tmpQueryIsClearer = @"case when ev.IDA_REC = ta_entity.IDA then 0 else 1 end";
            // Ajout recherche de la méthode de calcul du CB en fonction de la présence ou non de l'événement Equity Balance
            string tmpQueryCBMethod = @"case when e_eqt.EVENTTYPE is not null then 'CSBUK' else 'CSBDEFAULT' end";
            string tmpQueryInstrumentEnabled = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ns");

            string sqlSelect = @"select {0} as IDA, {1} as IDB, ta_entity.IDA as IDA_ENTITY, {2} as IDB_ENTITY,
            SUM(ev.VALORISATION) as AMOUNT, ev.UNIT as IDC, ec.DTEVENT, null as DTEVENTSTL,
            {3} as ISCLEARER, {4} as CBCALCMETHOD
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT on (ns.IDI = tr.IDI) and ({5})
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)
            inner join dbo.TRADEACTOR ta_cbo on (ta_cbo.IDT = tr.IDT) and (ta_cbo.IDROLEACTOR = 'CSHBALANCEOFFICE') and (ta_cbo.IDA = @IDA)
            inner join dbo.TRADEACTOR ta_entity on (ta_entity.IDT = tr.IDT) and (ta_entity.IDROLEACTOR = 'ENTITY')
            inner join dbo.EVENT e_cbs on (e_cbs.IDT = tr.IDT) and (e_cbs.UNIT = @IDC) and (ev.EVENTCODE = 'CBS') and (ev.EVENTTYPE = 'AMT')
            inner join dbo.EVENT ev on (ev.IDT = e_cbs.IDT) and (ev.IDE_EVENT = e_cbs.IDE) and (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = @EVENTTYPE) and 
            (ev.STREAMNO = e_cbs.STREAMNO) and (ev.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (ev.UNIT = e_cbs.UNIT)
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = 'REC') and (ec.DTEVENT >= @DTSTART) and (ec.DTEVENT <= @DTEND)
            inner join dbo.BOOK bk on (bk.IDB = {1})
            inner join dbo.EVENT e_eqt on (e_eqt.IDT = e_cbs.IDT) and (e_eqt.IDE_EVENT = e_cbs.IDE) and (e_eqt.EVENTCODE = 'LPC') and (e_eqt.EVENTTYPE = 'E_B') and 
            (e_eqt.STREAMNO = e_cbs.STREAMNO) and (e_eqt.INSTRUMENTNO = e_cbs.INSTRUMENTNO) and (e_eqt.UNIT = e_cbs.UNIT)
            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR')
            group by {0},{1}, ta_entity.IDA, {2}, ev.UNIT, ec.DTEVENT, {3}, {4}";

            return String.Format(sqlSelect, tmpQueryIDA, tmpQueryIDB, tmpQueryIDB_Entity, tmpQueryIsClearer, tmpQueryCBMethod, tmpQueryInstrumentEnabled);
        }
        /// <summary>
        /// Construit la requête de recherche des "trades" Cash Interest déjà présent
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string GetExistTradeQuery(string pCS)
        {
            string tmpQueryInstrumentEnabled = OTCmlHelper.GetSQLDataDtEnabled(pCS, "ns");
            string tmpXQuery = DataHelper.GetSQLXQuery_ExistsNode(pCS, "TRADEXML", "t", 
            @"//efs:EfsML/trade/efs:cashBalanceInterest/efs:interestAmountType[text()=sql:variable(""@AMOUNTTYPE"")]", OTCmlHelper.GetXMLNamespace_3_0(pCS));

            string sqlSelect = @"select tr.IDENTIFIER, tr.IDT, tr.DISPLAYNAME
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT on (ns.IDI = tr.IDI) and ({0})
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)
            inner join dbo.TRADEACTOR ta on (ta.IDT = tr.IDT) and (ta.IDA = @IDA) and 
            ((ta.IDB = @IDB) or ((ta.IDB is null) and (@IDB = 0))) and (ta.BUYER_SELLER is not null)
            inner join dbo.TRADEACTOR ta_entity on (ta_entity.IDT = tr.IDT) and (ta_entity.IDA = @IDA_ENTITY) and 
            ((ta_entity.IDB = @IDB_ENTITY) or ((ta_entity.IDB is null ) and (@IDB_ENTITY = 0))) and (ta_entity.BUYER_SELLER is not null)


            inner join dbo.TRADESTREAM ts (ts.IDT = tr.IDT) and (ts.IDC = @IDC) and (ts.STREAMNO = 1)
            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.DTTRADE = @DT) and {1}";

            return String.Format(sqlSelect, tmpQueryInstrumentEnabled, tmpXQuery);
        }
        #endregion
        #endregion
    }
    #endregion
}

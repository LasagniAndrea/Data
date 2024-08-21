using System;
using System.Data;  

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Status;




namespace EFS.Common
{
    #region QueryLibraryTools
    public sealed class QueryLibraryTools
    {
        #region Constructors
        public QueryLibraryTools(){}
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Initialisation de la collections des paramètres pour insertion/mise à jour des tables
        /// </summary>
        /// <param name="pTable">Enumérateur déterminant la table mise à jour (EVENT|EVENTASSET|EVENTCLASS|EVENTDET|EVENTFEE|EVENTPOSACTIONDET|EVENTPRICING|EVENTPRICING2)</param>
        // EG 20161122 New Commodity Derivative
        // FI 20170908 [23409] Modify
        // EG 20171025 [23509] Add TZDLVY, DTDLVY (DateTime2)
        // EG 20190613 [24683] Correction Type EXTLLINK sur EVENTCLASS
        // EG 20190716 [VCL : New FixedIncome] Add ASSETMEASURE (EVENTDET)
        private static DataParameters InitParameters(string pCS,Cst.OTCml_TBL pTable)
        {
            DataParameters ret;
            switch (pTable)
            {
                case Cst.OTCml_TBL.EVENT:
                    #region EVENT
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDT", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDE_EVENT", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDE_SOURCE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDA_PAY", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDB_PAY", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDA_REC", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDB_REC", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
                    ret.Add(new DataParameter(pCS, "EVENTTYPE", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
                    ret.Add(new DataParameter(pCS, "DTSTARTADJ", DbType.Date));   // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTSTARTUNADJ", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTENDADJ", DbType.Date));     // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTENDUNADJ", DbType.Date));   // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "VALORISATION", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "UNIT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "UNITTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "VALORISATIONSYS", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "UNITSYS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "UNITTYPESYS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "TAXLEVYOPT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                    ret.Add(new DataParameter(pCS, "IDASTACTIVATION", DbType.Int32));
                    ret.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSTACTIVATION)); 
                    ret.Add(new DataParameter(pCS, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                    ret.Add(new DataParameter(pCS, "IDSTCALCUL", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                    ret.Add(new DataParameter(pCS, "IDSTTRIGGER", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENT
                    break;
                case Cst.OTCml_TBL.EVENTASSET:
                    #region EVENTASSET
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDASSET", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "ASSETSYMBOL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "ASSETTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "CATEGORY", DbType.AnsiString, SQLCst.UT_CFICODE_LEN));
                    ret.Add(new DataParameter(pCS, "CLEARANCESYSTEM", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "CONTRACTMULTIPLIER", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CONTRACTSYMBOL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "DELIVERYDATE", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DISPLAYNAME", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN));
                    ret.Add(new DataParameter(pCS, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "IDM", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN));
                    ret.Add(new DataParameter(pCS, "ISINCODE", DbType.AnsiString, SQLCst.UT_ISINCODE_LEN));
                    ret.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYDATE)); // FI 20200610 [XXXXX] DbType.Date (Call DataParameter.GetParameter)
                    ret.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYDATESYS)); // FI 20200610 [XXXXX] DbType.Date (Call DataParameter.GetParameter)
                    ret.Add(new DataParameter(pCS, "NOMINALVALUE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PRIMARYRATESRC", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                    ret.Add(new DataParameter(pCS, "PRIMARYRATESRCHEAD", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                    ret.Add(new DataParameter(pCS, "PRIMARYRATESRCPAGE", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                    ret.Add(new DataParameter(pCS, "PUTORCALL", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "QUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_UNC_LEN));
                    // EG/CC/PL 20141128 Decimal
                    ret.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "TIME", DbType.DateTime));
                    ret.Add(new DataParameter(pCS, "WEIGHT", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "UNITWEIGHT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "UNITTYPEWEIGHT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENTASSET
                    break;
                case Cst.OTCml_TBL.EVENTCLASS:
                    #region EVENTCLASS
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN));
                    ret.Add(new DataParameter(pCS, "DTEVENT", DbType.Date));       // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTEVENTFORCED", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "IDNETCONVENTION", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDNETDESIGNATION", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "ISPAYMENT", DbType.Boolean));
                    ret.Add(new DataParameter(pCS, "NETMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "EXTLLINK",  DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENTCLASS
                    break;
                case Cst.OTCml_TBL.EVENTDET:
                    #region EVENTDET
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "BASIS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "CLOSINGPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CLOSINGPRICE100", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CONTRACTMULTIPLIER", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CONVERSIONRATE", DbType.Decimal));
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    ret.Add(new DataParameter(pCS, "DAILYQUANTITY", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "UNITDAILYQUANTITY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "DCF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "DCFDEN", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "DCFNUM", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "DTACTION", DbType.Date)); // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTFIXING", DbType.DateTime));
                    ret.Add(new DataParameter(pCS, "DTSETTLTPRICE", DbType.DateTime));
                    ret.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                    ret.Add(new DataParameter(pCS, "FACTOR", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "FWDPOINTS", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "FXTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "GAPRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "IDC_BASE", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "IDC_REF", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "INTEREST", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "MULTIPLIER", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "NOTE", DbType.AnsiString, SQLCst.UT_NOTE_LEN));
                    ret.Add(new DataParameter(pCS, "NOTIONALAMOUNT", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "NOTIONALREFERENCE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PCTPAYOUT", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PCTRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PERIODPAYOUT", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "PRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PRICE100", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "QUOTEDELTA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "QUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "QUOTEPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "QUOTEPRICE100", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "QUOTEPRICEYEST", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "QUOTEPRICEYEST100", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "RATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN));
                    ret.Add(new DataParameter(pCS, "SETTLTPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "SETTLTPRICE100", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "SETTLTQUOTESIDE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "SETTLTQUOTETIMING", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "SETTLEMENTRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "SPOTRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "SPREAD", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "TOTALOFDAY", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TOTALOFYEAR", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TOTALPAYOUTAMOUNT", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PIP", DbType.Decimal));
                    /// EG 20161122 New Commodity Derivative
                    ret.Add(new DataParameter(pCS, "DTDLVYSTART", DbType.DateTime2));
                    ret.Add(new DataParameter(pCS, "DTDLVYEND", DbType.DateTime2));
                    ret.Add(new DataParameter(pCS, "TZDLVY", DbType.AnsiString, SQLCst.UT_TIMEZONE_LEN));
                    ret.Add(new DataParameter(pCS, "ASSETMEASURE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENTDET
                    break;
                case Cst.OTCml_TBL.EVENTFEE:
                    #region EVENTFEE
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "STATUS", DbType.AnsiString, SQLCst.UT_STATUS_LEN));
                    ret.Add(new DataParameter(pCS, "PAYMENTID", DbType.AnsiString, SQLCst.UT_LABEL_LEN));        // FI 20180328 [23871] Add
                    ret.Add(new DataParameter(pCS, "FEESCOPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN)); // PL 20200107 [25099] Add FEESCOPE
                    ret.Add(new DataParameter(pCS, "IDFEEMATRIX", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDFEE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDFEESCHEDULE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "BRACKET1", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "BRACKET2", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULA", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULAVALUE1", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULAVALUE2", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULAVALUEBRACKET", DbType.AnsiString, SQLCst.UT_NOTE_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULADCF", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULAMIN", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "FORMULAMAX", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    ret.Add(new DataParameter(pCS, "FEEPAYMENTFREQUENCY", DbType.AnsiString, SQLCst.UT_LABEL_LEN));
                    //PL 20141023
                    //m_ParamEventFee.Add(new DataParameter(CS, "ASSESSMENTBASISVALUE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "ASSESSMENTBASISVALUE1", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "ASSESSMENTBASISVALUE2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "ISFEEINVOICING", DbType.Boolean));
                    ret.Add(new DataParameter(pCS, "PAYMENTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "ASSESSMENTBASISDET", DbType.AnsiString, 1000));
                    ret.Add(new DataParameter(pCS, "IDTAX", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDTAXDET", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TAXTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "TAXRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "TAXCOUNTRY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENTFEE
                    break;
                case Cst.OTCml_TBL.EVENTPOSACTIONDET:
                    #region EVENTPOSACTIONDET
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDPADET", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.SetAllDBNull();
                    #endregion EVENTPOSACTIONDET
                    break;
                case Cst.OTCml_TBL.EVENTPRICING:
                    #region EVENTPRICING
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDC1", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "DCF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "DCFNUM", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "DCFDEN", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "TOTALOFYEAR", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TOTALOFDAY", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TIMETOEXPIRATION", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "DCF2", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "DCFNUM2", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "DCFDEN2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "TOTALOFYEAR2", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TOTALOFDAY2", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "TIMETOEXPIRATION2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "STRIKE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "INTERESTRATE1", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "INTERESTRATE2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "SPOTRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "EXCHANGERATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "VOLATILITY", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "UNDERLYINGPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "DIVIDENDYIELD", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "RISKFREEINTEREST", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CALLPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CALLDELTA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CALLRHO1", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CALLRHO2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CALLTHETA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CALLCHARM", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PUTPRICE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PUTDELTA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PUTRHO1", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PUTRHO2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PUTTHETA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "PUTCHARM", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "DELTA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "GAMMA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "VEGA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "THETA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "RHO", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "BPV", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "COLOR", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "SPEED", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "VANNA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "VOLGA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CONVEXITY", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENTPRICING
                    break;
                case Cst.OTCml_TBL.EVENTPRICING2:
                    #region EVENTPRICING2
                    ret = new DataParameters();
                    ret.Add(new DataParameter(pCS, "IDE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDE_SOURCE", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "FLOWTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "DTFIXING", DbType.DateTime));
                    ret.Add(new DataParameter(pCS, "CASHFLOW", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "DTSTART", DbType.Date));      // FI 20201006 [XXXXX] DbType.Date  
                    ret.Add(new DataParameter(pCS, "DTCLOSING", DbType.Date));    // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTEND", DbType.Date));        // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "DTPAYMENT", DbType.Date));    // FI 20201006 [XXXXX] DbType.Date
                    ret.Add(new DataParameter(pCS, "TOTALOFDAY", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN));
                    ret.Add(new DataParameter(pCS, "VOLATILITY", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "DISCOUNTFACTOR", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "RATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "STRIKE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "BARRIER", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "FWDDELTA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "FWDGAMMA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "VEGA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "FXVEGA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "THETA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "BPV", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "CONVEXITY", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "DELTA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "GAMMA", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "NPV", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "METHOD", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN));
                    ret.Add(new DataParameter(pCS, "IDYIELDCURVEVAL_H", DbType.Int32));
                    ret.Add(new DataParameter(pCS, "ZEROCOUPON1", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "ZEROCOUPON2", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "FORWARDRATE", DbType.Decimal));
                    ret.Add(new DataParameter(pCS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN));
                    ret.SetAllDBNull();
                    #endregion EVENTPRICING
                    break;
                default:
                    // FI 20170908 [23409] ajout du pTable sinon plantage violent
                    throw new NotImplementedException(StrFunc.AppendFormat("EventQuure (table: {0}) is not implemented", pTable));
            }
            return ret;
        }

        #region GetQuerySelect
        public static string GetQuerySelect(string pCS, Cst.OTCml_TBL pTable, Nullable<Cst.ProcessTypeEnum> pProcessRequester)
        {
            return GetQuerySelect(pCS, pTable, false, pProcessRequester);
        }
        public static string GetQuerySelect(string pCS, Cst.OTCml_TBL pTable)
        {
            return GetQuerySelect(pCS, pTable, false, null);
        }
        public static string GetQuerySelect(string pCS, Cst.OTCml_TBL pTable, bool pWithOnlyTblMain)
        {
            return GetQuerySelect(pCS, pTable, pWithOnlyTblMain, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable"></param>
        /// <param name="pWithOnlyTblMain"></param>
        /// <param name="pProcessRequester"></param>
        /// <returns></returns>
        /// FI 20170607 [23221] Modify
        /// FI 20170609 [22189] Modify and private method
        // EG 20190716 [VCL : New FixedIncome] Add ASSETMEASURE (EVENTDET)
        private static string GetQuerySelect(string pCS, Cst.OTCml_TBL pTable, bool pWithOnlyTblMain, Nullable<Cst.ProcessTypeEnum> pProcessRequester)
        {
            string sqlSelect;
            switch (pTable)
            {
                case Cst.OTCml_TBL.EVENT:
                    #region EVENT
                    sqlSelect = @"select ev.IDE, ev.IDT, ev.INSTRUMENTNO, ev.STREAMNO, ev.IDE_EVENT, ev.IDE_SOURCE, 
                    ev.IDA_PAY, ev.IDB_PAY, ev.IDA_REC, ev.IDB_REC, ev.EVENTCODE, ev.EVENTTYPE, ev.DTSTARTADJ, ev.DTSTARTUNADJ, ev.DTENDADJ, ev.DTENDUNADJ, 
                    ev.VALORISATION, ev.UNIT, ev.UNITTYPE, ev.VALORISATIONSYS, ev.UNITSYS, ev.UNITTYPESYS, ev.TAXLEVYOPT, ev.IDSTCALCUL, ev.SOURCE, 
                    ev.IDSTTRIGGER, ev.IDSTACTIVATION, ev.IDASTACTIVATION, ev.DTSTACTIVATION, ev.EXTLLINK" + Cst.CrLf;

                    if (pProcessRequester.HasValue)
                    {
                        switch (pProcessRequester.Value)
                        {
                            case Cst.ProcessTypeEnum.ACCOUNTGEN:
                                // FI 20170609 [22189] la colonne EVENTENUM.IDEAR est obsolete 
                                // Usage d'un case when et utilisation de la vue VW_EAREVENTENUM
                                //sqlSelect += @", enumcode.ISEARDAY
                                sqlSelect += @", case @GPRODUCT 
                                when 'ADM'   then en.EAR_DAY_ADM
                                when 'ASSET' then en.EAR_DAY_ASSET
                                when 'FUT'   then en.EAR_DAY_FUT
                                when 'COM'   then en.EAR_DAY_COM                                
                                when 'FX'    then en.EAR_DAY_FX
                                when 'OTC'   then en.EAR_DAY_OTC
                                when 'RISK'  then en.EAR_DAY_RISK
                                when 'SEC'   then en.EAR_DAY_SEC end as ISEARDAY
                                from dbo.EVENT ev
                                inner join dbo.VW_EAREVENTENUM en on (en.CODE = 'EventCode') and (en.VALUE = ev.EVENTCODE)" + Cst.CrLf;
                                break;

                            case Cst.ProcessTypeEnum.EARGEN:
                                // FI 20170607 [23221] Prise en considération de GPRODUCT= 'COM'
                                string bitand = DataHelper.SQLBitand(pCS, "encode.X", "entype.X");
                                sqlSelect += @", case @GPRODUCT " + Cst.CrLf;
                                sqlSelect += @"when 'ADM'   then " + bitand.Replace("X", "EAR_DAY_ADM") + Cst.CrLf;
                                sqlSelect += @"when 'ASSET' then " + bitand.Replace("X", "EAR_DAY_ADM") + Cst.CrLf;
                                sqlSelect += @"when 'FUT'   then " + bitand.Replace("X", "EAR_DAY_FUT") + Cst.CrLf;
                                sqlSelect += @"when 'COM'   then " + bitand.Replace("X", "EAR_DAY_COM") + Cst.CrLf;
                                sqlSelect += @"when 'FX'    then " + bitand.Replace("X", "EAR_DAY_FX") + Cst.CrLf;
                                sqlSelect += @"when 'OTC'   then " + bitand.Replace("X", "EAR_DAY_OTC") + Cst.CrLf;
                                sqlSelect += @"when 'RISK'  then " + bitand.Replace("X", "EAR_DAY_RISK") + Cst.CrLf;
                                sqlSelect += @"when 'SEC'   then " + bitand.Replace("X", "EAR_DAY_SEC") + " end as ISEARDAY" + Cst.CrLf;
                                sqlSelect += @"from dbo.EVENT ev
                                inner join dbo.VW_EAREVENTENUM encode on (encode.CODE = 'EventCode') and (encode.VALUE = ev.EVENTCODE)
                                inner join dbo.VW_EAREVENTENUM entype on (entype.CODE = 'EventType') and (entype.VALUE = ev.EVENTTYPE)" + Cst.CrLf;
                                break;

                            default:
                                throw new Exception("GetQuerySelect for " + pTable.ToString() + "(" + pProcessRequester.Value.ToString() + " is not managed, please contact EFS");
                                // EG 20160404 Migration vs2013
                                //break;
                        }
                    }
                    else
                    {
                        sqlSelect += @"from dbo.EVENT ev" + Cst.CrLf;
                    }
                    #endregion EVENT
                    break;

                case Cst.OTCml_TBL.EVENTASSET:
                    #region EVENTASSET
                    sqlSelect = @"select ea.IDE, ea.IDASSET, ea.ASSETCATEGORY, ea.ASSETSYMBOL, ea.ASSETTYPE, ea.CATEGORY, 
                    ea.CLEARANCESYSTEM, ea.CONTRACTMULTIPLIER, ea.CONTRACTSYMBOL, ea.DELIVERYDATE, ea.DISPLAYNAME, ea.IDBC, ea.IDC, ea.IDM, 
                    ea.IDENTIFIER, ea.IDMARKETENV, ea.IDVALSCENARIO, ea.ISINCODE, ea.MATURITYDATE, ea.MATURITYDATESYS, ea.NOMINALVALUE, 
                    ea.PRIMARYRATESRC, ea.PRIMARYRATESRCHEAD, ea.PRIMARYRATESRCPAGE, ea.PUTORCALL, ea.QUOTESIDE, ea.QUOTETIMING, 
                    ea.STRIKEPRICE, ea.TIME, ea.WEIGHT, ea.UNITWEIGHT, ea.UNITTYPEWEIGHT
                    from dbo.EVENTASSET ea" + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ea.IDE)" + Cst.CrLf;
                    #endregion EVENTASSET
                    break;

                case Cst.OTCml_TBL.EVENTCLASS:
                    #region EVENTCLASS
                    sqlSelect = @"select ec.IDEC, ec.IDE, ec.EVENTCLASS, ec.DTEVENT, ec.DTEVENTFORCED, ec.ISPAYMENT,
                    ec.NETMETHOD, ec.IDNETCONVENTION, ec.IDNETDESIGNATION, ec.EXTLLINK" + Cst.CrLf;

                    if (pProcessRequester.HasValue)
                    {
                        switch (pProcessRequester.Value)
                        {
                            case Cst.ProcessTypeEnum.ACCOUNTGEN:
                                // FI 20170609 [22189] la colonne EVENTENUM.IDEAR est obsolete
                                // Usage d'un case when et utilisation de la vue VW_EAREVENTENUM
                                //sqlSelect += @", enumclass.ISEARDAY
                                sqlSelect += @", case @GPRODUCT 
                                when 'ADM'   then en.EAR_DAY_ADM
                                when 'ASSET' then en.EAR_DAY_ASSET
                                when 'FUT'   then en.EAR_DAY_FUT
                                when 'COM'   then en.EAR_DAY_COM                                
                                when 'FX'    then en.EAR_DAY_FX
                                when 'OTC'   then en.EAR_DAY_OTC
                                when 'RISK'  then en.EAR_DAY_RISK
                                when 'SEC'   then en.EAR_DAY_SEC end as ISEARDAY
                                from dbo.EVENTCLASS ec
                                inner join dbo.EVENT ev on (ev.IDE = ec.IDE)
                                inner join dbo.VW_EAREVENTENUM en on (en.CODE = 'EventClass') and (en.VALUE = ec.EVENTCLASS)" + Cst.CrLf;
                                break;
                            case Cst.ProcessTypeEnum.EARGEN:
                                // FI 20170607 [23221] Prise en considération de GPRODUCT= 'COM'
                                sqlSelect += @", case @GPRODUCT 
                                when 'ADM'   then en.EAR_DAY_ADM
                                when 'ASSET' then en.EAR_DAY_ASSET
                                when 'FUT'   then en.EAR_DAY_FUT
                                when 'COM'   then en.EAR_DAY_COM                                
                                when 'FX'    then en.EAR_DAY_FX
                                when 'OTC'   then en.EAR_DAY_OTC
                                when 'RISK'  then en.EAR_DAY_RISK
                                when 'SEC'   then en.EAR_DAY_SEC end as ISEARDAY
                                from dbo.EVENTCLASS ec
                                inner join dbo.EVENT ev on (ev.IDE = ec.IDE)
                                inner join dbo.VW_EAREVENTENUM en on (en.CODE = 'EventClass') and (en.VALUE = ec.EVENTCLASS)" + Cst.CrLf;
                                break;
                            default:
                                throw new Exception("GetQuerySelect for " + pTable.ToString() + "(" + pProcessRequester.Value.ToString() + " is not managed, please contact EFS");
                                // EG 20160404 Migration vs2013
                                //break;
                        }
                    }
                    else
                    {
                        sqlSelect += @"from dbo.EVENTCLASS ec" + Cst.CrLf;
                        if (false == pWithOnlyTblMain)
                            sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ec.IDE)" + Cst.CrLf;
                    }
                    #endregion EVENTCLASS
                    break;

                case Cst.OTCml_TBL.EVENTFEE:
                    #region EVENTFEE
                    // PL 20141023
                    // PL 20200107 [25099] Add FEESCOPE and PAYMENTID 
                    sqlSelect = @"select efee.IDE, efee.STATUS, efee.PAYMENTID, efee.FEESCOPE, efee.IDFEEMATRIX, efee.IDFEE, efee.IDFEESCHEDULE, efee.BRACKET1, efee.BRACKET2, 
                    efee.FORMULA, efee.FORMULAVALUE1, efee.FORMULAVALUE2, efee.FORMULAVALUEBRACKET, efee.FORMULADCF, efee.FORMULAMIN, efee.FORMULAMAX, 
                    efee.FEEPAYMENTFREQUENCY, efee.ASSESSMENTBASISVALUE1, efee.ASSESSMENTBASISVALUE2, efee.ISFEEINVOICING, efee.PAYMENTTYPE, efee.ASSESSMENTBASISDET,
                    efee.IDTAX, efee.IDTAXDET, efee.TAXTYPE, efee.TAXRATE, efee.TAXCOUNTRY 
                    from dbo.EVENTFEE efee" + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = efee.IDE)" + Cst.CrLf;
                    #endregion EVENTFEE
                    break;

                case Cst.OTCml_TBL.EVENTSI:
                case Cst.OTCml_TBL.EVENTSI_T:
                    #region EVENTSI / EVENTSI_T
                    sqlSelect = @"select esi.IDE, esi.PAYER_RECEIVER, esi.IDA_STLOFFICE, esi.IDA_MSGRECEIVER, esi.SIMODE, esi.IDCSSLINK, esi.IDSSIDB, esi.IDISSI,
                    esi.SIXML, esi.IDA_CSS, esi.SIREF, esi.IDAINS, esi.DTINS, esi.IDAUPD, esi.DTUPD
                    from dbo.EVENTSI esi" + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = esi.IDE)" + Cst.CrLf;
                    #endregion EVENTSI / EVENTSI_T
                    break;

                case Cst.OTCml_TBL.EVENTDET:
                    #region EVENTDET
                    sqlSelect = @"select ed.IDE, ed.BASIS, ed.CLOSINGPRICE, ed.CLOSINGPRICE100, ed.CONTRACTMULTIPLIER, ed.CONVERSIONRATE, 
                    ed.DAILYQUANTITY, ed.UNITDAILYQUANTITY, ed.DCF, ed.DCFDEN, ed.DCFNUM, ed.DTACTION, ed.DTFIXING, ed.DTSETTLTPRICE, ed.EXTLLINK, 
                    ed.FACTOR, ed.FWDPOINTS, ed.FXTYPE, ed.GAPRATE, ed.IDBC, ed.IDC_BASE, ed.IDC_REF, ed.IDC1, ed.IDC2, ed.INTEREST, 
                    ed.MULTIPLIER, ed.NOTE, ed.NOTIONALAMOUNT, ed.NOTIONALREFERENCE, ed.PCTPAYOUT, ed.PCTRATE, ed.PERIODPAYOUT, ed.PRICE, ed.PRICE100, 
                    ed.QUOTEDELTA, ed.QUOTEPRICE, ed.QUOTEPRICE100, ed.QUOTEPRICEYEST, ed.QUOTEPRICEYEST100, ed.QUOTETIMING, 
                    ed.RATE, ed.ROWATTRIBUT, ed.SETTLEMENTRATE, ed.SETTLTPRICE, ed.SETTLTPRICE100, ed.SETTLTQUOTESIDE, ed.SETTLTQUOTETIMING, 
                    ed.SPOTRATE, ed.SPREAD, ed.STRIKEPRICE, ed.TOTALOFDAY, ed.TOTALOFYEAR, ed.TOTALPAYOUTAMOUNT, ed.PIP, ed.DTDLVYSTART, ed.DTDLVYEND,
                    ed.ASSETMEASURE  
                    from dbo.EVENTDET ed " + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ed.IDE)" + Cst.CrLf;
                    #endregion EVENTDET
                    break;

                case Cst.OTCml_TBL.EVENTPRICING:
                    #region EVENTPRICING
                    sqlSelect = @"select ep.IDE, ep.IDC1, ep.IDC2, ep.STRIKE, ep.VOLATILITY, 
                    ep.DCF, ep.DCFNUM, ep.DCFDEN, ep.TOTALOFYEAR, ep.TOTALOFDAY, ep.TIMETOEXPIRATION, 
                    ep.DCF2, ep.DCFNUM2, ep.DCFDEN2, ep.TOTALOFYEAR2, ep.TOTALOFDAY2, ep.TIMETOEXPIRATION2, 
                    ep.UNDERLYINGPRICE, ep.DIVIDENDYIELD, ep.RISKFREEINTEREST,
                    ep.EXCHANGERATE, ep.INTERESTRATE1, ep.INTERESTRATE2, ep.SPOTRATE, 
                    ep.CALLPRICE, ep.CALLDELTA, ep.CALLRHO1, ep.CALLRHO2, ep.CALLTHETA, ep.CALLCHARM, 
                    ep.PUTPRICE, ep.PUTDELTA, ep.PUTRHO1, ep.PUTRHO2, ep.PUTTHETA, ep.PUTCHARM, 
                    ep.GAMMA, ep.VEGA, ep.COLOR, ep.SPEED, ep.VANNA, ep.VOLGA, ep.DELTA, 
                    ep.THETA, ep.RHO, ep.BPV, ep.CONVEXITY, ep.EXTLLINK
                    from dbo.EVENTPRICING ep " + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ep.IDE)" + Cst.CrLf;
                    #endregion EVENTPRICING
                    break;

                case Cst.OTCml_TBL.EVENTPRICING2:
                    #region EVENTPRICING2
                    sqlSelect = @"select ep.IDEVENTPRICING2, ep.IDE, ep.IDE_SOURCE, 
                    ep.FLOWTYPE, ep.DTFIXING, ep.CASHFLOW, ep.DTSTART, ep.DTCLOSING, ep.DTEND, ep.DTPAYMENT, ep.TOTALOFDAY, 
                    ep.IDC, ep.VOLATILITY, ep.DISCOUNTFACTOR, ep.RATE, ep.STRIKE, ep.BARRIER, ep.FWDDELTA, ep.FWDGAMMA, 
                    ep.VEGA, ep.FXVEGA, ep.THETA, ep.BPV, ep.CONVEXITY, ep.DELTA, ep.GAMMA, ep.NPV, ep.METHOD, 
                    ep.IDYIELDCURVEVAL_H, ep.ZEROCOUPON1, ep.ZEROCOUPON2, ep.FORWARDRATE, ep.EXTLLINK " + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                    {
                        sqlSelect += @", yc.IDYIELDCURVEDEF, en.EXTVALUE as METHODDISPLAYNAME
                        from dbo.EVENTPRICING2 ep 
                        inner join dbo.EVENT ev on (ev.IDE = ep.IDE)
                        inner join dbo.YIELDCURVEVAL_H yc on (yc.IDYIELDCURVEVAL_H = ep.IDYIELDCURVEVAL_H)
                        left join dbo.EVENTENUM en on (en.CODE = 'EventClass') and (en.VALUE = ep.METHOD)" + Cst.CrLf;
                    }
                    else
                    {
                        sqlSelect += @"from dbo.EVENTPRICING2 ep " + Cst.CrLf;
                    }
                    #endregion EVENTPRICING2
                    break;

                case Cst.OTCml_TBL.EVENTPROCESS:
                    #region EVENTPROCESS
                    sqlSelect = @"select ep.IDE, ep.IDEP, ep.PROCESS, ep.IDSTPROCESS, ep.DTSTPROCESS, ep.EVENTCLASS, ep.IDDATA, ep.IDTRK_L, ep.EXTLLINK 
                    from dbo.EVENTPROCESS ep" + Cst.CrLf;
                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = ep.IDE)" + Cst.CrLf;

                    #endregion EVENTPROCESS
                    break;

                case Cst.OTCml_TBL.EVENTSTCHECK:
                    #region EVENTSTCHECK
                    SQL_EventStUser stCheck = new SQL_EventStUser(pCS, 0, StatusEnum.StatusCheck);
                    sqlSelect = stCheck.GetQueryParameters(new string[]{
																"EVENTSTCHECK.IDE", "EVENTSTCHECK.IDSTCHECK", 
																"EVENTSTCHECK.DTINS", "EVENTSTCHECK.IDAINS",
																"EVENTSTCHECK.EXTLLINK"}).QueryReplaceParameters.ToString();

                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = EVENTSTCHECK.IDE)" + Cst.CrLf;

                    #endregion EVENTSTCHECK
                    break;

                case Cst.OTCml_TBL.EVENTSTMATCH:
                    #region EVENTSTMATCH
                    SQL_EventStUser stMatch = new SQL_EventStUser(pCS, 0, StatusEnum.StatusMatch);
                    sqlSelect = stMatch.GetQueryParameters(new string[]{
																"EVENTSTMATCH.IDE", "EVENTSTMATCH.IDSTMATCH", 
																"EVENTSTMATCH.DTINS", "EVENTSTMATCH.IDAINS",
																"EVENTSTMATCH.EXTLLINK"}).QueryReplaceParameters;

                    if (false == pWithOnlyTblMain)
                        sqlSelect += @"inner join dbo.EVENT ev on (ev.IDE = EVENTSTMATCH.IDE)" + Cst.CrLf;

                    #endregion EVENTSTMATCH
                    break;

                default:
                    throw new Exception("GetQuerySelect for " + pTable.ToString() + " is not managed, please contact EFS");
            }
            return sqlSelect;
        }
        #endregion GetQuerySelect

        #region GetQueryInsert
        /// <summary>
        /// Retourne la requête d'insert
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable"></param>
        /// <returns></returns>
        /// EG 20161122 New Commodity Derivative
        /// EG 20171025 [23509] Add TZDLVY
        public static QueryParameters GetQueryInsert(string pCS, Cst.OTCml_TBL pTable)
        {

            string sqlInsert;
            switch (pTable)
            {
                case Cst.OTCml_TBL.EVENT:
                    sqlInsert = @"insert into dbo.EVENT
                    (IDE, IDT, INSTRUMENTNO, STREAMNO, IDE_EVENT,IDE_SOURCE, IDA_PAY, IDB_PAY, IDA_REC, IDB_REC, EVENTCODE, EVENTTYPE, 
                     DTSTARTADJ, DTSTARTUNADJ, DTENDADJ, DTENDUNADJ, VALORISATION, UNIT, UNITTYPE, VALORISATIONSYS, UNITSYS, UNITTYPESYS, TAXLEVYOPT, IDSTCALCUL, 
                     IDSTTRIGGER, SOURCE, EXTLLINK,  IDSTACTIVATION, IDASTACTIVATION, DTSTACTIVATION) 
                     values 
                    (@IDE, @IDT, @INSTRUMENTNO, @STREAMNO, @IDE_EVENT, @IDE_SOURCE, @IDA_PAY, @IDB_PAY, @IDA_REC, @IDB_REC, @EVENTCODE, @EVENTTYPE, 
                     @DTSTARTADJ, @DTSTARTUNADJ, @DTENDADJ, @DTENDUNADJ, @VALORISATION, @UNIT, @UNITTYPE, @VALORISATIONSYS, @UNITSYS, @UNITTYPESYS, @TAXLEVYOPT, @IDSTCALCUL, 
                     @IDSTTRIGGER, @SOURCE, @EXTLLINK, @IDSTACTIVATION, @IDASTACTIVATION, @DTSTACTIVATION)";
                    break;

                case Cst.OTCml_TBL.EVENTASSET:
                    sqlInsert = @"insert into dbo.EVENTASSET 
                    (IDE, IDASSET, ASSETCATEGORY, ASSETSYMBOL, ASSETTYPE, CATEGORY, CLEARANCESYSTEM, CONTRACTMULTIPLIER, CONTRACTSYMBOL, DELIVERYDATE, 
                     DISPLAYNAME, IDBC, IDC, IDM, IDENTIFIER, ISINCODE, MATURITYDATE, MATURITYDATESYS, NOMINALVALUE, PRIMARYRATESRC, PRIMARYRATESRCHEAD, PRIMARYRATESRCPAGE, 
                     PUTORCALL, QUOTESIDE, QUOTETIMING, STRIKEPRICE, TIME, WEIGHT, UNITWEIGHT, UNITTYPEWEIGHT) 
                     values 
                    (@IDE, @IDASSET, @ASSETCATEGORY, @ASSETSYMBOL, @ASSETTYPE, @CATEGORY, @CLEARANCESYSTEM, @CONTRACTMULTIPLIER, @CONTRACTSYMBOL, @DELIVERYDATE, 
                     @DISPLAYNAME, @IDBC, @IDC, @IDM, @IDENTIFIER, @ISINCODE, @MATURITYDATE, @MATURITYDATESYS, @NOMINALVALUE, @PRIMARYRATESRC, @PRIMARYRATESRCHEAD, @PRIMARYRATESRCPAGE, 
                     @PUTORCALL, @QUOTESIDE, @QUOTETIMING, @STRIKEPRICE, @TIME, @WEIGHT, @UNITWEIGHT, @UNITTYPEWEIGHT)";
                    break;

                case Cst.OTCml_TBL.EVENTCLASS:
                    sqlInsert = @"insert into dbo.EVENTCLASS 
                    (IDE, EVENTCLASS, DTEVENT, DTEVENTFORCED, ISPAYMENT, IDNETCONVENTION, IDNETDESIGNATION, NETMETHOD, EXTLLINK ) 
                    values 
                    (@IDE, @EVENTCLASS, @DTEVENT, @DTEVENTFORCED, @ISPAYMENT, @IDNETCONVENTION, @IDNETDESIGNATION, @NETMETHOD, @EXTLLINK)";
                    break;

                case Cst.OTCml_TBL.EVENTDET:
                    sqlInsert = @"insert into dbo.EVENTDET 
                    (IDE, BASIS, CLOSINGPRICE, CLOSINGPRICE100, CONTRACTMULTIPLIER, CONVERSIONRATE, DAILYQUANTITY, UNITDAILYQUANTITY, DCF, DCFDEN, DCFNUM, 
                     DTACTION, DTFIXING, DTSETTLTPRICE, EXTLLINK, FACTOR, FWDPOINTS, FXTYPE, GAPRATE, IDBC, IDC1, IDC2, IDC_BASE, IDC_REF, INTEREST, MULTIPLIER, 
                     NOTE, NOTIONALAMOUNT, NOTIONALREFERENCE, PCTRATE, PCTPAYOUT, PERIODPAYOUT, PRICE, PRICE100, QUOTEDELTA, QUOTETIMING, QUOTEPRICE, QUOTEPRICE100, 
                     QUOTEPRICEYEST, QUOTEPRICEYEST100, RATE, ROWATTRIBUT, SETTLTPRICE, SETTLTPRICE100, SETTLTQUOTESIDE, SETTLTQUOTETIMING, SETTLEMENTRATE,
                     SPOTRATE, SPREAD, STRIKEPRICE, TOTALOFDAY, TOTALOFYEAR, TOTALPAYOUTAMOUNT, PIP, DTDLVYSTART, DTDLVYEND, TZDLVY, ASSETMEASURE) 
		             values 
                    (@IDE, @BASIS, @CLOSINGPRICE, @CLOSINGPRICE100, @CONTRACTMULTIPLIER, @CONVERSIONRATE, @DAILYQUANTITY, @UNITDAILYQUANTITY, @DCF, @DCFDEN, @DCFNUM, 
                     @DTACTION, @DTFIXING, @DTSETTLTPRICE, @EXTLLINK, @FACTOR, @FWDPOINTS, @FXTYPE, @GAPRATE, @IDBC, @IDC1, @IDC2, @IDC_BASE, @IDC_REF, @INTEREST, @MULTIPLIER, 
                     @NOTE, @NOTIONALAMOUNT, @NOTIONALREFERENCE, @PCTRATE, @PCTPAYOUT, @PERIODPAYOUT, @PRICE, @PRICE100, @QUOTEDELTA, @QUOTETIMING, @QUOTEPRICE, @QUOTEPRICE100, 
                     @QUOTEPRICEYEST, @QUOTEPRICEYEST100, @RATE, @ROWATTRIBUT, @SETTLTPRICE, @SETTLTPRICE100, @SETTLTQUOTESIDE, @SETTLTQUOTETIMING, @SETTLEMENTRATE,
                     @SPOTRATE, @SPREAD, @STRIKEPRICE, @TOTALOFDAY, @TOTALOFYEAR, @TOTALPAYOUTAMOUNT, @PIP, @DTDLVYSTART, @DTDLVYEND, @TZDLVY, @ASSETMEASURE) ";
                    break;

                case Cst.OTCml_TBL.EVENTFEE:
                    // PL 20141023
                    // FI 20180328 [23871] Add PAYMENTID
                    // PL 20200107 [25099] Add FEESCOPE
                    sqlInsert = @"insert into dbo.EVENTFEE
                    (IDE, STATUS, PAYMENTID, FEESCOPE, 
                     IDFEEMATRIX, IDFEE, IDFEESCHEDULE, BRACKET1, BRACKET2, 
                     FORMULA, FORMULAVALUE1, FORMULAVALUE2, FORMULAVALUEBRACKET, FORMULADCF, FORMULAMIN, FORMULAMAX, 
                     FEEPAYMENTFREQUENCY, ASSESSMENTBASISVALUE1, ASSESSMENTBASISVALUE2, ISFEEINVOICING, PAYMENTTYPE, ASSESSMENTBASISDET,
                     IDTAX, IDTAXDET, TAXTYPE, TAXRATE, TAXCOUNTRY) 
                     values 
                    (@IDE, @STATUS, @PAYMENTID, @FEESCOPE, 
                     @IDFEEMATRIX, @IDFEE, @IDFEESCHEDULE, @BRACKET1, @BRACKET2, 
                     @FORMULA, @FORMULAVALUE1, @FORMULAVALUE2, @FORMULAVALUEBRACKET, @FORMULADCF, @FORMULAMIN, @FORMULAMAX, 
                     @FEEPAYMENTFREQUENCY, @ASSESSMENTBASISVALUE1, @ASSESSMENTBASISVALUE2, @ISFEEINVOICING, @PAYMENTTYPE, @ASSESSMENTBASISDET, 
                     @IDTAX, @IDTAXDET, @TAXTYPE, @TAXRATE, @TAXCOUNTRY)";
                    break;

                case Cst.OTCml_TBL.EVENTPOSACTIONDET:
                    // EG 20170425 [23064] Suppression ";" fin de query
                    sqlInsert = @"insert into dbo.EVENTPOSACTIONDET (IDPADET, IDE) values (@IDPADET, @IDE)";
                    break;

                case Cst.OTCml_TBL.EVENTPRICING:

                    sqlInsert = @"insert into dbo.EVENTPRICING
                    (IDE, IDC1, IDC2, STRIKE, VOLATILITY, 
                     DCF, DCFNUM, DCFDEN, TOTALOFYEAR, TOTALOFDAY, TIMETOEXPIRATION, 
                     DCF2, DCFNUM2, DCFDEN2, TOTALOFYEAR2, TOTALOFDAY2, TIMETOEXPIRATION2, 
                     UNDERLYINGPRICE, DIVIDENDYIELD, RISKFREEINTEREST, EXCHANGERATE, SPOTRATE, INTERESTRATE1, INTERESTRATE2,
                     CALLPRICE, CALLDELTA, CALLRHO1, CALLRHO2, CALLTHETA, CALLCHARM, 
                     PUTPRICE, PUTDELTA, PUTRHO1, PUTRHO2, PUTTHETA, PUTCHARM, 
                     GAMMA, VEGA, COLOR, SPEED, VANNA, VOLGA, DELTA, THETA, RHO, BPV, CONVEXITY, EXTLLINK)
                     values
                    (@IDE, @IDC1, @IDC2, @STRIKE, @VOLATILITY, 
                     @DCF, @DCFNUM, @DCFDEN, @TOTALOFYEAR, @TOTALOFDAY, @TIMETOEXPIRATION, 
                     @DCF2, @DCFNUM2, @DCFDEN2, @TOTALOFYEAR2, @TOTALOFDAY2, @TIMETOEXPIRATION2, 
                     @UNDERLYINGPRICE, @DIVIDENDYIELD, @RISKFREEINTEREST, @EXCHANGERATE, @SPOTRATE, @INTERESTRATE1, @INTERESTRATE2,
                     @CALLPRICE, @CALLDELTA, @CALLRHO1, @CALLRHO2, @CALLTHETA, @CALLCHARM, 
                     @PUTPRICE, @PUTDELTA, @PUTRHO1, @PUTRHO2, @PUTTHETA, @PUTCHARM, 
                     @GAMMA, @VEGA, @COLOR, @SPEED, @VANNA, @VOLGA, @DELTA, @THETA, @RHO, @BPV, @CONVEXITY, @EXTLLINK)";
                    break;

                default:
                    throw new Exception("GetQueryInsert for " + pTable.ToString() + " is not managed, please contact EFS");
            }

            DataParameters parameters = InitParameters(pCS, pTable);
            QueryParameters ret = new QueryParameters(pCS, sqlInsert, parameters);

            return ret;
        }
        #endregion GetQueryInsert

        #region GetQueryUpdate
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <returns></returns>
        public static QueryParameters GetQueryUpdate(string pCS, Cst.OTCml_TBL pTable)
        {
            string sqlUpdate;
            switch (pTable)
            {
                case Cst.OTCml_TBL.EVENTPRICING:

                    sqlUpdate = @"update dbo.EVENTPRICING
                    set IDC1 = @IDC1, IDC2 = @IDC2, STRIKE = @STRIKE, VOLATILITY = @VOLATILITY, 
                    DCF = @DCF, DCFNUM = @DCFNUM, DCFDEN = @DCFDEN, TOTALOFYEAR = @TOTALOFYEAR, TOTALOFDAY = @TOTALOFDAY, TIMETOEXPIRATION = @TIMETOEXPIRATION, 
                    DCF2 = @DCF2, DCFNUM2 = @DCFNUM2, DCFDEN2 = @DCFDEN2, TOTALOFYEAR2 = @TOTALOFYEAR2, TOTALOFDAY2 = @TOTALOFDAY2, TIMETOEXPIRATION2 = @TIMETOEXPIRATION2, 
                    UNDERLYINGPRICE = @UNDERLYINGPRICE, DIVIDENDYIELD = @DIVIDENDYIELD, RISKFREEINTEREST = @RISKFREEINTEREST,
                    EXCHANGERATE = @EXCHANGERATE, SPOTRATE = @SPOTRATE, INTERESTRATE1 = @INTERESTRATE1, INTERESTRATE2 = @INTERESTRATE2,
                    CALLPRICE = @CALLPRICE, CALLDELTA = @CALLDELTA, CALLRHO1 = @CALLRHO1, CALLRHO2 = @CALLRHO2, CALLTHETA = @CALLTHETA, CALLCHARM = @CALLCHARM, 
                    PUTPRICE = @PUTPRICE, PUTDELTA = @PUTDELTA, PUTRHO1 = @PUTRHO1, PUTRHO2 = @PUTRHO2, PUTTHETA = @PUTTHETA, PUTCHARM = @PUTCHARM, 
                    GAMMA = @GAMMA, VEGA = @VEGA, COLOR = @COLOR, SPEED = @SPEED, VANNA = @VANNA, VOLGA = @VOLGA, 
                    DELTA = @DELTA, THETA = @THETA, RHO = @RHO, BPV = @BPV, CONVEXITY = @CONVEXITY, EXTLLINK = @EXTLLINK 
                    where (IDE = @IDE)";
                    break;

                default:
                    throw new Exception("GetQueryUpdate for " + pTable.ToString() + " is not managed, please contact EFS");
            }

            DataParameters parameters = InitParameters(pCS, pTable);
            QueryParameters ret = new QueryParameters(pCS, sqlUpdate, parameters);

            return ret;
        }
        #endregion GetQueryUpdate
        #endregion Methods
    }
    #endregion QueryLibraryTools
}

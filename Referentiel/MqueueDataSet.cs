using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Process;

namespace EFS.Referential
{
    
    /// <summary>
    /// Classe chargée de générer des messages Queue vers les services suite à modification d'un référentiel 
    /// </summary>
    public class MQueueDataset
    {
        #region Members
        /// <summary>
        ///  Représente le nom du référentiel
        /// </summary>
        private readonly string m_TableName;
        private ArrayList m_objDatas;
        private readonly string m_source;

        #endregion
        #region Accessors
        /// <summary>
        /// Obtient true si le référentiel doit émettre un message vers les services
        /// </summary>
        /// PM 20151027 [20964] Ajout IsDERIVATIVEATTRIB
        public bool IsAvailable
        {
            get
            {
                //return IsQUOTE_H || IsMATURITY || IsDERIVATIVECONTRACT || IsASSET_ETD || IsPOSCOLLATERAL;
                return IsQUOTE_H || IsMATURITY || IsDERIVATIVECONTRACT || IsDERIVATIVEATTRIB || IsASSET_ETD || IsPOSCOLLATERAL;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsQUOTE_DEBTSEC_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_DEBTSEC_H.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsQUOTE_EQUITY_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_EQUITY_H.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsQUOTE_ETD_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_ETD_H.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsQUOTE_FXRATE_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_FXRATE_H.ToString()); }
        }

        /// <summary>
        /// Obtient true si m_TableName.StartsWith("QUOTE_") et  m_TableName.EndsWith("_H")
        /// </summary>
        private bool IsQUOTE_H
        {
            get { return (m_TableName.StartsWith("QUOTE_") && m_TableName.EndsWith("_H")); }
        }
        /// EG 20140120 Report v3.7
        private bool IsQUOTE_INDEX_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_INDEX_H.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsQUOTE_RATEINDEX_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_RATEINDEX_H.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsQUOTE_SIMPLEIRS_H
        {
            get { return (m_TableName == Cst.OTCml_TBL.QUOTE_SIMPLEIRS_H.ToString()); }
        }

        /// <summary>
        /// Obtient true si m_TableName == Cst.OTCml_TBL.MATURITY
        /// </summary>
        private bool IsMATURITY
        {
            get { return (m_TableName == Cst.OTCml_TBL.MATURITY.ToString()); }
        }

        /// <summary>
        /// Obtient true si m_TableName == Cst.OTCml_TBL.DERIVATIVECONTRACT
        /// </summary>
        private bool IsDERIVATIVECONTRACT
        {
            get { return (m_TableName == Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString()); }
        }

        /// <summary>
        /// Obtient true si m_TableName == Cst.OTCml_TBL.DERIVATIVEATTRIB
        /// </summary>
        /// PM 20151027 [20964] New
        private bool IsDERIVATIVEATTRIB
        {
            get { return (m_TableName == Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString()); }
        }

        /// <summary>
        /// Obtient true si m_TableName == Cst.OTCml_TBL.ASSET_ETD
        /// </summary>
        private bool IsASSET_ETD
        {
            get { return (m_TableName == Cst.OTCml_TBL.ASSET_ETD.ToString()); }
        }

        /// <summary>
        /// Obtient true si m_TableName == IsPOSCOLLATERAL
        /// </summary>
        private bool IsPOSCOLLATERAL
        {
            get { return (m_TableName == Cst.OTCml_TBL.POSCOLLATERAL.ToString()); }
        }

        /// <summary>
        /// 
        /// </summary>
        public ArrayList ObjDatas
        {
            get
            {
                return m_objDatas;
            }
            set
            {
                m_objDatas = value;
            }
        }

        #endregion Accessors
        #region Constructors
        public MQueueDataset(string pCS, string pTableName)
        {
            m_objDatas = new ArrayList();
            m_source = pCS;
            m_TableName = pTableName.ToUpper();
        }
        #endregion
        #region Methods

        // PM 20140211 [19601] Passage de la méthode de private à public
        public void AddMaturity(DataRow pRow, DataRowVersion pDataRowVersion, DataRowState pDataRowState)
        {
            Maturity maturity = new Maturity
            {
                action = pDataRowState.ToString()
            };

            // IDMATURITY
            string colName = "IDMATURITY";
            maturity.idMaturitySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.idMaturitySpecified)
                maturity.idMaturity = Convert.ToInt32(pRow[colName, pDataRowVersion]);

            // IDMATURITYRULE
            maturity.idMaturityRule = Convert.ToInt32(pRow["IDMATURITYRULE", pDataRowVersion]);
            //SQL_MaturityRule maturityRule = new SQL_MaturityRule(m_source, maturity.idMaturityRule);
            //PL 20131112 [TRIM 19164]
            SQL_MaturityRuleRead maturityRule = new SQL_MaturityRuleRead(m_source, maturity.idMaturityRule);
            maturity.idMaturityRule_identifier = maturityRule.Identifier;

            // MATURITYMONTHYEAR
            maturity.maturityMonthYear = pRow["MATURITYMONTHYEAR", pDataRowVersion].ToString();

            // Dates
            colName = "MATURITYDATE";
            maturity.maturityDateSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.maturityDateSpecified)
                maturity.maturityDate = Convert.ToDateTime(pRow[colName, pDataRowVersion]);
            colName = "MATURITYTIME";
            maturity.maturityTimeSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.maturityTimeSpecified)
                maturity.maturityTime = pRow[colName, pDataRowVersion].ToString();

            colName = "LASTTRADINGDAY";
            maturity.lastTradingDaySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.lastTradingDaySpecified)
                maturity.lastTradingDay = Convert.ToDateTime(pRow[colName, pDataRowVersion]);
            colName = "LASTTRADINGTIME";
            maturity.lastTradingTimeSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.lastTradingTimeSpecified)
                maturity.lastTradingTime = pRow[colName, pDataRowVersion].ToString();

            colName = "DELIVERYDATE";
            maturity.deliveryDateSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.deliveryDateSpecified)
                maturity.deliveryDate = Convert.ToDateTime(pRow[colName, pDataRowVersion]);

            colName = "FIRSTDELIVERYDATE";
            maturity.firstDeliveryDateSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.firstDeliveryDateSpecified)
                maturity.firstDeliveryDate = Convert.ToDateTime(pRow[colName, pDataRowVersion]);
            colName = "LASTDELIVERYDATE";
            maturity.lastDeliveryDateSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.lastDeliveryDateSpecified)
                maturity.lastDeliveryDate = Convert.ToDateTime(pRow[colName, pDataRowVersion]);
            colName = "FIRSTDLVSETTLTDATE";
            maturity.firstDlvSettltDateSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.firstDlvSettltDateSpecified)
                maturity.firstDlvSettltDate = Convert.ToDateTime(pRow[colName, pDataRowVersion]);
            colName = "LASTDLVSETTLTDATE";
            maturity.lastDlvSettltDateSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.lastDlvSettltDateSpecified)
                maturity.lastDlvSettltDate = Convert.ToDateTime(pRow[colName, pDataRowVersion]);

            colName = "PERIODMLTPDELIVERY";
            maturity.periodMltpDeliverySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.periodMltpDeliverySpecified)
                maturity.periodMltpDelivery = Convert.ToInt32(pRow[colName, pDataRowVersion]);
            colName = "PERIODDELIVERY";
            maturity.periodDeliverySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.periodDeliverySpecified)
                maturity.periodDelivery = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "DAYTYPEDELIVERY";
            maturity.dayTypeDeliverySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.dayTypeDeliverySpecified)
                maturity.dayTypeDelivery = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "ROLLCONVDELIVERY";
            maturity.rollConvDeliverySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.rollConvDeliverySpecified)
                maturity.rollConvDelivery = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "DELIVERYTIMESTART";
            maturity.deliveryTimeStartSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.deliveryTimeStartSpecified)
                maturity.deliveryTimeStart = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "DELIVERYTIMEEND";
            maturity.deliveryTimeEndSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.deliveryTimeEndSpecified)
                maturity.deliveryTimeEnd = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "DELIVERYTIMEZONE";
            maturity.deliveryTimeZoneSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.deliveryTimeZoneSpecified)
                maturity.deliveryTimeZone = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "ISAPPLYSUMMERTIME";
            maturity.isApplySummerTimeSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.isApplySummerTimeSpecified)
                maturity.isApplySummerTime = Convert.ToBoolean(pRow[colName, pDataRowVersion]);
            colName = "PERIODMLTPDLVSETTLTOFFSET";
            maturity.periodMltpDlvSettltSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.periodMltpDeliverySpecified)
                maturity.periodMltpDelivery = Convert.ToInt32(pRow[colName, pDataRowVersion]);
            colName = "PERIODDLVSETTLTOFFSET";
            maturity.periodDlvSettltSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.periodDlvSettltSpecified)
                maturity.periodDlvSettlt = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "DAYTYPEDLVSETTLTOFFSET";
            maturity.dayTypeDlvSettltSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.dayTypeDlvSettltSpecified)
                maturity.dayTypeDlvSettlt = Convert.ToString(pRow[colName, pDataRowVersion]);
            colName = "SETTLTOFHOLIDAYDLVCONVENTION";
            maturity.settltOfHolidayDlvConventionSpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.settltOfHolidayDlvConventionSpecified)
                maturity.settltOfHolidayDlvConvention = Convert.ToString(pRow[colName, pDataRowVersion]);

            colName = "FIRSTNOTICEDAY";
            maturity.firstNoticeDaySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.firstNoticeDaySpecified)
                maturity.firstNoticeDay = Convert.ToDateTime(pRow[colName, pDataRowVersion]);
            colName = "LASTNOTICEDAY";
            maturity.lastNoticeDaySpecified = (false == Convert.IsDBNull(pRow[colName, pDataRowVersion]));
            if (maturity.lastNoticeDaySpecified)
                maturity.lastNoticeDay = Convert.ToDateTime(pRow[colName, pDataRowVersion]);

            m_objDatas.Add(maturity);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pDataRowVersion"></param>
        /// <param name="pDataRowState"></param>
        /// FI 20170306 [22225] Modify (Modifications diverses)
        private void AddQuote(DataRow pRow, DataRowVersion pDataRowVersion, DataRowState pDataRowState)
        {
            Quote quote = null;
            string columnName = null;
            if (IsQUOTE_H)
            {
                if (IsQUOTE_FXRATE_H)
                {
                    quote = new Quote_FxRate();
                    columnName = "VW_FX_IDENTIFIER_ISDEFAULT";
                }
                else if (IsQUOTE_ETD_H)
                {
                    #region QUOTE_ETD
                    quote = new Quote_ETDAsset();
                    Quote_ETDAsset quoteETD = (Quote_ETDAsset)quote;
                    columnName = "ASSET_IDENTIFIER";

                    quoteETD.contractMultiplierSpecified = (false == Convert.IsDBNull(pRow["CONTRACTMULTIPLIER", pDataRowVersion]));

                    if ((pDataRowState == DataRowState.Deleted) && (quoteETD.contractMultiplierSpecified))
                    {
                        // RD 20110629
                        // Dans le cas de la suppression d'une Quotation
                        // Et le contractMultiplier était valorisé
                        // alors recalculer les events de cashflow avec le contractMultiplier en viguer sur l'Asset/DC
                        quoteETD.contractMultiplierSpecified = false;
                        quoteETD.isCashFlowsValSpecified = true;
                        quoteETD.isCashFlowsVal = true;
                    }
                    else
                    {
                        if (quoteETD.contractMultiplierSpecified)
                            quoteETD.contractMultiplier = Convert.ToDecimal(pRow["CONTRACTMULTIPLIER", pDataRowVersion]);
                        //
                        // RD 20110629
                        // Pour revaloriser les Events de CashFlow du jour, à la lumière d'un éventuel nouveau contractMultiplier
                        // A faire uniquement si le contractMultiplier a changé dans le cas d'une modification d'une Quotation existente
                        if ((pDataRowState == DataRowState.Modified) && ContractMultiplierHasChanged(pRow))
                        {
                            quoteETD.isCashFlowsValSpecified = true;
                            quoteETD.isCashFlowsVal = true;
                        }
                        // Ou bien contractMultiplier est spécifié dans le cas d'une insertion d'une nouvelle Quotation
                        else if ((pDataRowState == DataRowState.Added) && quoteETD.contractMultiplierSpecified)
                        {
                            quoteETD.isCashFlowsValSpecified = true;
                            quoteETD.isCashFlowsVal = true;
                        }
                        else
                            quoteETD.isCashFlowsValSpecified = false;
                    }
                    #endregion QUOTE_ETD
                }
                else if (IsQUOTE_EQUITY_H)
                {
                    quote = new Quote_Equity();
                    columnName = "ASSET_IDENTIFIER";
                }
                else if (IsQUOTE_INDEX_H)
                {
                    quote = new Quote_Index();
                    columnName = "ASSET_IDENTIFIER";
                }
                else if (IsQUOTE_SIMPLEIRS_H)
                {
                    quote = new Quote_SimpleIRSwap();
                    columnName = "ASSET_IDENTIFIER";
                }
                else if (IsQUOTE_DEBTSEC_H)
                {
                    quote = new Quote_DebtSecurityAsset();
                    columnName = "ASSET_IDENTIFIER";
                }
                else if (IsQUOTE_RATEINDEX_H)
                {
                    quote = new Quote_RateIndex();
                    //FI 20110707 Now is ASSET_IDENTIFIER
                    columnName = "ASSET_IDENTIFIER";
                }
                else
                {
                    quote = new Quote_ToDefine();
                    //FI 20110707 Now is ASSET_IDENTIFIER
                    columnName = "ASSET_IDENTIFIER";
                }
            }
            else if (IsDERIVATIVECONTRACT || IsDERIVATIVEATTRIB || IsASSET_ETD)  // PM 20151027 [20964] Ajout IsDERIVATIVEATTRIB
            {
                quote = new Quote_ETDAsset();
                columnName = "IDENTIFIER";

                Quote_ETDAsset quoteETD = (Quote_ETDAsset)quote;
                // FI 20170306 [22225] Mise en commentaire puisque par défaut quoteETD.contractMultiplierSpecified vaut false
                //quoteETD.contractMultiplierSpecified = false;
                quoteETD.isCashFlowsValSpecified = true;
                quoteETD.isCashFlowsVal = true;
            }

            //PL 20100608 Add if()
            if (StrFunc.IsFilled(columnName))
            {
                // RD 20110629
                // Gérer la modification du ContractMultiplier  sur DERIVATIVECONTRACT et ASSET_ETD
                quote.action = pDataRowState.ToString();

                if (IsQUOTE_H || IsASSET_ETD)
                {
                    // PM 20151027 [20964] IdAsset disponible uniquement pour IsQUOTE_H et IsASSET_ETD
                    quote.idAsset = Convert.ToInt32(pRow["IDASSET", pDataRowVersion]);
                    quote.idAsset_Identifier = pRow[columnName, pDataRowVersion].ToString();

                    // RD 20200911 [25475] A la création d'une nouvelle cotation, "ASSET_IDENTIFIER" n'est pas disponible.
                    if (StrFunc.IsEmpty(quote.idAsset_Identifier) && quote.idAsset > 0)
                    {
                        SQL_AssetBase sqlAsset = null;

                        if (IsQUOTE_ETD_H || IsASSET_ETD)
                            sqlAsset = new SQL_AssetETD(CSTools.SetCacheOn(m_source), quote.idAsset);
                        else if (IsQUOTE_FXRATE_H)
                            sqlAsset = new SQL_AssetFxRate(CSTools.SetCacheOn(m_source), quote.idAsset);
                        else if (IsQUOTE_EQUITY_H)
                            sqlAsset = new SQL_AssetEquity(CSTools.SetCacheOn(m_source), quote.idAsset);
                        else if (IsQUOTE_INDEX_H)
                            sqlAsset = new SQL_AssetIndex(CSTools.SetCacheOn(m_source), quote.idAsset);
                        else if (IsQUOTE_SIMPLEIRS_H)
                            sqlAsset = new SQL_AssetSimpleIRSwap(CSTools.SetCacheOn(m_source), quote.idAsset);
                        else if (IsQUOTE_DEBTSEC_H)
                            sqlAsset = new SQL_AssetDebtSecurity(CSTools.SetCacheOn(m_source), quote.idAsset);
                        else if (IsQUOTE_RATEINDEX_H)
                            sqlAsset = new SQL_AssetRateIndex(CSTools.SetCacheOn(m_source), SQL_AssetRateIndex.IDType.IDASSET, quote.idAsset);

                        if ((null != sqlAsset) && sqlAsset.IsLoaded)
                            quote.idAsset_Identifier = sqlAsset.Identifier;
                    }
                }

                if (IsDERIVATIVECONTRACT || IsDERIVATIVEATTRIB || IsASSET_ETD) // PM 20151027 [20964] Ajout IsDERIVATIVEATTRIB
                {
                    Quote_ETDAsset Quote_ETDAsset = (Quote_ETDAsset)quote;

                    Quote_ETDAsset.QuoteTable = m_TableName;
                    // FI 20170306 [22225] mise en commentaire car le chargement s'effectue uniquement s'il s'agit d'une cotation
                    //quote.idQuoteSpecified = true; // Pour éviter d'aller le charger
                    Quote_ETDAsset.time = Convert.ToDateTime(DateTime.MinValue);
                    Quote_ETDAsset.timeSpecified = false;
                    quote.valueSpecified = false;
                    quote.eventClass = null;

                    if (IsDERIVATIVECONTRACT)
                    {
                        Quote_ETDAsset.idDC = Convert.ToInt32(pRow["IDDC", pDataRowVersion]);
                        Quote_ETDAsset.idDCSpecified = (((Quote_ETDAsset)quote).idDC > 0);
                        Quote_ETDAsset.idDC_Identifier = pRow[columnName, pDataRowVersion].ToString();
                        Quote_ETDAsset.idDC_IdentifierSpecified = (((Quote_ETDAsset)quote).idDC > 0);
                    }

                    else if (IsDERIVATIVEATTRIB) // PM 20151027 [20964] Ajout pour DERIVATIVEATTRIB
                    {
                        Quote_ETDAsset.idDerivativeAttrib = Convert.ToInt32(pRow["IDDERIVATIVEATTRIB", pDataRowVersion]);
                        Quote_ETDAsset.idDerivativeAttribSpecified = (Quote_ETDAsset.idDerivativeAttrib > 0);
                    }
                }
                else
                {
                    quote.idQuoteSpecified = (false == Convert.IsDBNull(pRow["IDQUOTE_H", pDataRowVersion]));
                    if (quote.idQuoteSpecified)
                        quote.idQuote = Convert.ToInt32(pRow["IDQUOTE_H", pDataRowVersion]);
                    quote.idMarketEnv = pRow["IDMARKETENV", pDataRowVersion].ToString();
                    quote.idValScenario = pRow["IDVALSCENARIO", pDataRowVersion].ToString();
                    quote.time = Convert.ToDateTime(pRow["TIME", pDataRowVersion]);
                    quote.idBC = pRow["IDBC", pDataRowVersion].ToString();
                    quote.quoteSide = pRow["QUOTESIDE", pDataRowVersion].ToString();
                    quote.cashFlowType = pRow["CASHFLOWTYPE", pDataRowVersion].ToString();
                    quote.quoteTiming = pRow["QUOTETIMING", pDataRowVersion].ToString();
                    quote.valueSpecified = true;
                    quote.value = Convert.ToDecimal(pRow["VALUE", pDataRowVersion]);
                }

                m_objDatas.Add(quote);
            }
        }
        /// <summary>
        /// Create a business element for each data changed: Quote, Maturity...
        /// </summary>
        /// <param name="pDt"></param>
        public void Prepare(DataTable pDt)
        {
            if (IsAvailable && pDt.DataSet.HasChanges())
            {
                DataTable dtRowsChange = pDt.GetChanges();
                if (null != dtRowsChange)
                {
                    if (IsQUOTE_H)
                    {
                        #region Quote_FXRate - Quote_RateIndex - Quote_ETD
                        foreach (DataRow row in dtRowsChange.Rows)
                        {
                            DataRowState dataRowState = row.RowState;
                            if (DataRowState.Deleted == row.RowState)
                            {
                                AddQuote(row, DataRowVersion.Original, dataRowState);
                            }
                            else
                            {
                                // EG 20100412 Test sur ISENABLED
                                bool isEnabled = Convert.ToBoolean(row["ISENABLED", DataRowVersion.Current]);
                                if (DataRowState.Modified == row.RowState)
                                {
                                    if (QuoteKeyHasChanged(row))
                                    {
                                        AddQuote(row, DataRowVersion.Original, DataRowState.Deleted);
                                        // EG 20100412 Test sur ISENABLED
                                        if (isEnabled)
                                            AddQuote(row, DataRowVersion.Current, DataRowState.Added);
                                    }
                                    else
                                    {
                                        // EG 20100412 Test sur ISENABLED
                                        if (isEnabled)
                                            AddQuote(row, DataRowVersion.Current, dataRowState);
                                        else
                                            AddQuote(row, DataRowVersion.Current, DataRowState.Deleted);
                                    }
                                }
                                else if (isEnabled && DataRowState.Added == row.RowState)
                                {
                                    AddQuote(row, DataRowVersion.Current, dataRowState);
                                }
                            }
                        }
                        #endregion
                    }
                    // PM 20151027 [20964] Ajout IsDERIVATIVEATTRIB
                    //else if (IsDERIVATIVECONTRACT || IsASSET_ETD)
                    else if (IsDERIVATIVECONTRACT || IsASSET_ETD || IsDERIVATIVEATTRIB)
                    {
                        #region DERIVATIVECONTRACT - ASSET_ETD - IsDERIVATIVEATTRIB
                        foreach (DataRow row in dtRowsChange.Rows)
                        {
                            DataRowState dataRowState = row.RowState;
                            if ((DataRowState.Modified == dataRowState) && (ContractMultiplierHasChanged(row)))
                                AddQuote(row, DataRowVersion.Current, dataRowState);
                        }
                        #endregion
                    }
                    else if (IsMATURITY)
                    {
                        #region Maturity
                        foreach (DataRow row in dtRowsChange.Rows)
                        {
                            DataRowState dataRowState = row.RowState;

                            switch (dataRowState)
                            {
                                case DataRowState.Added:
                                    AddMaturity(row, DataRowVersion.Current, dataRowState);
                                    break;
                                case DataRowState.Deleted:
                                    AddMaturity(row, DataRowVersion.Original, dataRowState);
                                    break;
                                case DataRowState.Modified:
                                    if (MaturityDateHasChanged(row))
                                    {
                                        AddMaturity(row, DataRowVersion.Current, dataRowState);
                                    }
                                    break;
                            }
                        }
                        #endregion
                    }
                    else if (IsPOSCOLLATERAL)
                    {
                        foreach (DataRow row in dtRowsChange.Rows)
                        {
                            if ((DataRowState.Added == row.RowState))
                            {
                                m_objDatas.Add(row);
                            }
                            else if (DataRowState.Modified == row.RowState)
                            {
                                //Aucune action (sauf si cash) (vu avec PL)
                                //La valorisation est effectuée uniquement lors de la mise à jour des prix, ou en cas d'insertion 
                                //
                                //FI 20120208 Les assets Cash étant non côtés, le calcul est effectué à chaque modification de ce montant
                                //
                                if (null == row["ASSETCATEGORY"])
                                    throw new NullReferenceException("ASSETCATEGORY is null");
                                //
                                string assetCategory = row["ASSETCATEGORY"].ToString();
                                if (false == Enum.IsDefined(typeof(Cst.UnderlyingAsset), assetCategory))
                                    throw new Exception(StrFunc.AppendFormat("ASSETCATEGORY [{0}] is not defined", assetCategory));
                                //
                                if (assetCategory == Cst.UnderlyingAsset.Cash.ToString())
                                {
                                    if (PosCashCollateralHasChanged(row))
                                        m_objDatas.Add(row);
                                }
                            }
                            else if (DataRowState.Deleted == row.RowState)
                            {
                                //Aucune action
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Retourne si MATURITYDATE a changé 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] method static
        private static bool MaturityDateHasChanged(DataRow pRow)
        {
            DateTime maturityDateOriginal = DateTime.MinValue;
            DateTime maturityDateCurrent = DateTime.MinValue;

            if (false == Convert.IsDBNull(pRow["MATURITYDATE", DataRowVersion.Original]))
                maturityDateOriginal = Convert.ToDateTime(pRow["MATURITYDATE", DataRowVersion.Original]);

            if (false == Convert.IsDBNull(pRow["MATURITYDATE", DataRowVersion.Current]))
                maturityDateCurrent = Convert.ToDateTime(pRow["MATURITYDATE", DataRowVersion.Current]);

            return (maturityDateOriginal != maturityDateCurrent);
        }

        /// <summary>
        ///  Retourne si CONTRACTMULTIPLIER a changé 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] method static
        private static bool ContractMultiplierHasChanged(DataRow pRow)
        {
            decimal contractMultiplierOriginal = 0;
            decimal contractMultiplierCurrent = 0;

            if (false == Convert.IsDBNull(pRow["CONTRACTMULTIPLIER", DataRowVersion.Original]))
                contractMultiplierOriginal = Convert.ToDecimal(pRow["CONTRACTMULTIPLIER", DataRowVersion.Original]);

            if (false == Convert.IsDBNull(pRow["CONTRACTMULTIPLIER", DataRowVersion.Current]))
                contractMultiplierCurrent = Convert.ToDecimal(pRow["CONTRACTMULTIPLIER", DataRowVersion.Current]);

            return contractMultiplierOriginal != contractMultiplierCurrent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private bool QuoteKeyHasChanged(DataRow pRow)
        {
            string idMarketEnvOriginal = pRow["IDMARKETENV", DataRowVersion.Original].ToString();
            string idMarketEnvCurrent = pRow["IDMARKETENV", DataRowVersion.Current].ToString();
            bool isKeyChanged = idMarketEnvOriginal != idMarketEnvCurrent;
            if (false == isKeyChanged)
            {
                string idValScenarioOriginal = pRow["IDVALSCENARIO", DataRowVersion.Original].ToString();
                string idValScenarioCurrent = pRow["IDVALSCENARIO", DataRowVersion.Current].ToString();
                isKeyChanged = (idValScenarioOriginal != idValScenarioCurrent);
            }

            if (false == isKeyChanged)
            {
                int idAssetOriginal = Convert.ToInt32(pRow["IDASSET", DataRowVersion.Original]);
                int idAssetCurrent = Convert.ToInt32(pRow["IDASSET", DataRowVersion.Current]);
                isKeyChanged = (idAssetOriginal != idAssetCurrent);
            }

            if (false == isKeyChanged)
            {
                DateTime timeOriginal = Convert.ToDateTime(pRow["TIME", DataRowVersion.Original]);
                DateTime timeCurrent = Convert.ToDateTime(pRow["TIME", DataRowVersion.Current]);
                isKeyChanged = (timeOriginal != timeCurrent);

            }

            if (false == isKeyChanged)
            {
                string idBCOriginal = pRow["IDBC", DataRowVersion.Original].ToString();
                string idBCCurrent = pRow["IDBC", DataRowVersion.Current].ToString();
                isKeyChanged = (idBCOriginal != idBCCurrent);
            }

            if (false == isKeyChanged)
            {
                string idQuoteSideOriginal = pRow["QUOTESIDE", DataRowVersion.Original].ToString();
                string idQuoteSideCurrent = pRow["QUOTESIDE", DataRowVersion.Current].ToString();
                isKeyChanged = (idQuoteSideOriginal != idQuoteSideCurrent);
            }

            if (false == isKeyChanged)
            {
                string idCashFlowTypeOriginal = pRow["CASHFLOWTYPE", DataRowVersion.Original].ToString();
                string idCashFlowTypeCurrent = pRow["CASHFLOWTYPE", DataRowVersion.Current].ToString();
                isKeyChanged = (idCashFlowTypeOriginal != idCashFlowTypeCurrent);
            }

            // RD 20110629
            // Gérer la modification du ContractMultiplier  sur QUOTE_ETD_H
            if (IsQUOTE_ETD_H)
                isKeyChanged = ContractMultiplierHasChanged(pRow);

            return isKeyChanged;

        }
        /// <summary>
        /// Sending a message to QuotationHandling service
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSession"></param>
        /// <param name="pIdMenu"></param>
        /// FI 20130201 [XXXXX] correction dans la mise à jour du COLLATERAL
        /// FI 20170306 [22225] Modify
        /// FI 20170313 [22225] Modify
        /// EG 20190215 Correction Tooltip Tracker sur MAJ Données de marché via Saisie des prix EDSP
        public void Send(IDbTransaction pDbTransaction, AppSession pSession, string pIdMenu)
        {
            if (IsAvailable && (ArrFunc.IsFilled(m_objDatas)))
            {
                Cst.ProcessTypeEnum processType = Cst.ProcessTypeEnum.QUOTHANDLING;

                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    connectionString = m_source,
                    Session = pSession,
                    process = processType,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = processType,
                        gProduct = null,
                        caller = pIdMenu
                    }
                };

                MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = m_source };


                for (int i = 0; i < m_objDatas.Count; i++)
                {
                    MQueueBase[] mQueue = null;
                    List<DictionaryEntry> infoTracker = new List<DictionaryEntry>();

                    if (m_objDatas[i].GetType().BaseType.Equals(typeof(Quote)))
                    {
                        Quote quote = (Quote)m_objDatas[i];

                        if (quote is Quote_ETDAsset && quote.IsQuoteTable_ETD)
                        {
                            // FI 20170313 [22225] Gestion particulière lorsque modification d'un asset, d'un DC ou d'une échéance 
                            Quote_ETDAsset quote_ETDAsset = quote as Quote_ETDAsset;
                            switch (quote_ETDAsset.QuoteTable)
                            {
                                case "DERIVATIVECONTRACT":
                                    infoTracker.Add(new DictionaryEntry("IDDATA", quote_ETDAsset.idDC));
                                    infoTracker.Add(new DictionaryEntry("IDDATAIDENTIFIER", quote_ETDAsset.idDC_Identifier));
                                    break;
                                case "DERIVATIVEATTRIB":
                                    // FI 20170313 [22225] GLOP (Il faudrait un identifiant)
                                    infoTracker.Add(new DictionaryEntry("IDDATA", quote_ETDAsset.idDerivativeAttrib));
                                    break;
                                case "ASSET_ETD":
                                    infoTracker.Add(new DictionaryEntry("IDDATA", quote_ETDAsset.idAsset));
                                    infoTracker.Add(new DictionaryEntry("IDDATAIDENTIFIER", quote_ETDAsset.idAsset_Identifier));
                                    break;
                                default:
                                    throw new NotImplementedException(StrFunc.AppendFormat("Table (id:{0}) is not implemented", quote_ETDAsset.QuoteTable));
                            }
                            infoTracker.Add(new DictionaryEntry("IDDATAIDENT", quote_ETDAsset.QuoteTable));
                        }
                        else
                        {
                            if (quote.QuoteTable.StartsWith("QUOTE_") && quote.QuoteTable.EndsWith("_H")) // FI 20170306 [22225] ad if
                            {
                                if ((false == quote.idQuoteSpecified) ||
                                    (quote.action == DataRowState.Added.ToString()))
                                    quote.SetIdQuote(m_source, null);
                            }

                            /// EG 20190215 Correction Tooltip
                            if (taskInfo.trackerAttrib.caller == "OTC_INP_QUOTE_EOD")
                            {
                                taskInfo.trackerAttrib.caller = GetMenuQuoteByTable(quote.QuoteTable, taskInfo.trackerAttrib.caller);
                            }

                            infoTracker.Add(new DictionaryEntry("IDDATA", quote.idQuote));
                            infoTracker.Add(new DictionaryEntry("IDDATAIDENT", quote.QuoteTable));
                            infoTracker.Add(new DictionaryEntry("IDDATAIDENTIFIER", quote.idAsset_Identifier));
                            infoTracker.Add(new DictionaryEntry("DATA1", quote.idAsset_Identifier));
                            infoTracker.Add(new DictionaryEntry("DATA2", quote.action));
                            infoTracker.Add(new DictionaryEntry("DATA3", DtFunc.DateTimeToStringISO(quote.time)));
                            infoTracker.Add(new DictionaryEntry("DATA4", StrFunc.FmtDecimalToInvariantCulture(quote.value)));
                        }
                        mQueue = new QuotationHandlingMQueue[] { new QuotationHandlingMQueue(quote, mQueueAttributes) };
                    }
                    else if (m_objDatas[i].GetType().Equals(typeof(Maturity)))
                    {
                        Maturity maturity = (Maturity)m_objDatas[i];
                        if ((false == maturity.idMaturitySpecified) ||
                            (maturity.action == DataRowState.Added.ToString()))
                            maturity.SetIdMaturity(m_source, null);

                        infoTracker.Add(new DictionaryEntry("IDDATA", maturity.idMaturityRule));
                        infoTracker.Add(new DictionaryEntry("IDDATAIDENT", "MATURITYRULE")); //TODO
                        infoTracker.Add(new DictionaryEntry("IDDATAIDENTIFIER", maturity.idMaturityRule_identifier));
                        infoTracker.Add(new DictionaryEntry("DATA1", maturity.idMaturityRule_identifier));
                        infoTracker.Add(new DictionaryEntry("DATA2", maturity.action));
                        if (maturity.maturityDateSpecified)
                            infoTracker.Add(new DictionaryEntry("DATA3", DtFunc.DateTimeToStringISO(maturity.maturityDate)));
                        infoTracker.Add(new DictionaryEntry("DATA4", maturity.maturityMonthYear));

                        mQueue = new QuotationHandlingMQueue[] { new QuotationHandlingMQueue(maturity, mQueueAttributes) };
                    }
                    else if (IsPOSCOLLATERAL)
                    {
                        DataRow row = (DataRow)m_objDatas[i];

                        mQueue = SendPosCollateral(pDbTransaction, mQueueAttributes, row, out int[] idPosCollateral);
                        //FI 20130425 [18598]  alimentation de IDDATA uniquement s'il existe un collateral à valoriser
                        if (ArrFunc.Count(mQueue) == 1)
                        {
                            // FI 20130201 add alimentation de infoTracker
                            infoTracker = new List<DictionaryEntry>
                            {
                                new DictionaryEntry("IDDATA", idPosCollateral[0]),
                                new DictionaryEntry("IDDATAIDENT", "POSCOLLATERAL")
                            };
                        }
                    }
                    if (null != mQueue)
                    {
                        taskInfo.mQueue = mQueue;
                        if (0 < infoTracker.Count)
                            taskInfo.trackerAttrib.info = infoTracker;

                        taskInfo.SetTrackerAckWebSessionSchedule(ArrFunc.Count(taskInfo.mQueue) == 1 ? taskInfo.mQueue[0].idInfo : null);


                        (bool IsOk, string ErrMsg) = MQueueTaskInfo.SendMultiple(taskInfo);

                        if (!IsOk)
                            throw new SpheresException2("MQueueTaskInfo.SendMultiple", ErrMsg);
                    }
                }
            }
        }

        /// EG 20190215 New Correction Tooltip Tracker sur MAJ Données de marché via Saisie des prix EDSP
        private string GetMenuQuoteByTable(string pTable, string pDefaultMenu)
        {
            string ret = pDefaultMenu;

            Nullable<Cst.OTCml_TBL> table = ReflectionTools.ConvertStringToEnumOrNullable<Cst.OTCml_TBL>(pTable);
            if (table.HasValue)
            {
                switch (table.Value)
                {
                    case Cst.OTCml_TBL.QUOTE_BOND_H:
                        ret = "OTC_REF_DATA_QUOTE_BOND";
                        break;
                    case Cst.OTCml_TBL.QUOTE_DEBTSEC_H:
                        ret = "OTC_REF_DATA_QUOTE_DEBTSECURITY";
                        break;
                    case Cst.OTCml_TBL.QUOTE_DEPOSIT_H:
                        ret = "OTC_REF_DATA_QUOTE_DEPOSIT";
                        break;
                    case Cst.OTCml_TBL.QUOTE_EQUITY_H:
                        ret = "OTC_REF_DATA_QUOTE_EQUITY";
                        break;
                    case Cst.OTCml_TBL.QUOTE_EXTRDFUND_H:
                        ret = "OTC_REF_DATA_QUOTE_EXCHANGETRADEDFUND";
                        break;
                    case Cst.OTCml_TBL.QUOTE_ETD_H:
                        ret = "OTC_REF_DATA_QUOTE_ETD";
                        break;
                    case Cst.OTCml_TBL.QUOTE_FXRATE_H:
                        ret = "OTC_REF_DATA_QUOTE_FXRATE";
                        break;
                    case Cst.OTCml_TBL.QUOTE_INDEX_H:
                        ret = "OTC_REF_DATA_QUOTE_INDEX";
                        break;
                    case Cst.OTCml_TBL.QUOTE_RATEINDEX_H:
                        ret = "OTC_REF_DATA_QUOTE_RATEINDEX";
                        break;
                    case Cst.OTCml_TBL.QUOTE_SIMPLEFRA_H:
                        ret = "OTC_REF_DATA_QUOTE_SIMPLEFRA";
                        break;
                    case Cst.OTCml_TBL.QUOTE_SIMPLEIRS_H:
                        ret = "OTC_REF_DATA_QUOTE_SIMPLEIRS";
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les message Queue nécessaires pour générer la valorisation de la ligne créée/modifiée
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pMQueueAttributes"></param>
        /// <param name="pRow"></param>
        /// <param name="pIdCollateral"></param>
        /// FI 20130201 [] Modification 
        /// FI 20130425 [18598] modification de la signature de la fonction => pIdCollateral est de type array
        /// Spheres® valorise toutes les enregristrements qui colle avec le contexte
        private MQueueBase[] SendPosCollateral(IDbTransaction pDbTransaction, MQueueAttributes pMQueueAttributes, DataRow pRow, out int[] pIdCollateral)
        {
            MQueueBase[] ret = null;

            DataRow row = pRow;
            //payer
            Pair<int, int> payer = new Pair<int, int>
            {
                First = Convert.ToInt32(row["IDA_PAY"]),
                Second = Convert.ToInt32(row["IDB_PAY"])
            };
            //Receiver
            Pair<int, int> receiver = new Pair<int, int>
            {
                First = Convert.ToInt32(row["IDA_REC"]),
                Second = Convert.ToInt32(row["IDB_REC"])
            };
            //DtBusiness
            DateTime dtBusiness = Convert.ToDateTime(row["DTBUSINESS"]);
            //asset
            Pair<Cst.UnderlyingAsset, int> asset = new Pair<Cst.UnderlyingAsset, int>
            {
                First = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), row["ASSETCATEGORY"].ToString()),
                Second = Convert.ToInt32(row["IDASSET"])
            };

            int[] idPosCollateral = PosCollateralRDBMSTools.GetId(m_source, pDbTransaction, payer, receiver, dtBusiness, asset);
            pIdCollateral = idPosCollateral;

            if (ArrFunc.IsEmpty(idPosCollateral))
                throw new Exception("Collateral not found");

            List<CollateralValMQueue> lst = new List<CollateralValMQueue>();
            for (int i = 0; i < ArrFunc.Count(idPosCollateral); i++)
            {
                //FI 20130201 add alimentation de pMQueueAttributes.id et pMQueueAttributes.idInfo
                pMQueueAttributes.id = idPosCollateral[i];
                pMQueueAttributes.idInfo = new IdInfo
                {
                    id = idPosCollateral[i],
                    idInfos = new DictionaryEntry[] { new DictionaryEntry("ident", "COLLATERAL") }
                };

                CollateralValMQueue mqueue = new CollateralValMQueue(pMQueueAttributes)
                {
                    actionSpecified = true,
                    //FI 20130425 [18598] Il y a une petite faille puisque il y a potentiellement plusieurs lignes
                    //Spheres® pourrait position l'action Added alors les enregistements existent déjà (=> C'est pas grave) 
                    action = row.RowState.ToString()
                };

                lst.Add(mqueue);
            }

            if ((lst.Count) > 0)
                ret = lst.ToArray();

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private bool PosCashCollateralHasChanged(DataRow pRow)
        {
            decimal qtyOriginal = 0;
            decimal qtyCurrent = 0;

            if (false == Convert.IsDBNull(pRow["QTY", DataRowVersion.Original]))
                qtyOriginal = Convert.ToDecimal(pRow["QTY", DataRowVersion.Original]);

            if (false == Convert.IsDBNull(pRow["QTY", DataRowVersion.Current]))
                qtyCurrent = Convert.ToDecimal(pRow["QTY", DataRowVersion.Current]);

            return qtyOriginal != qtyCurrent;
        }
        #endregion Methods
    }
    
}

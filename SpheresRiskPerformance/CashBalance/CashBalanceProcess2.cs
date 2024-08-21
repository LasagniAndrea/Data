using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;

using EFS.GUI.Interface;
using EFS.Process;
using EFS.Process.EventsGen;
using EFS.TradeInformation;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.v30.CashBalance;
using EfsML.v30.Fix;

using FixML.Enum;
using FixML.v50SP1.Enum;

using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

using EFS.SpheresRiskPerformance;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.Tuning;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    /// <summary>
    /// Partialisation de la classe CashBalanceProcess
    /// Construction des montants/trades/book par type de FLUX
    /// Se substitue à la fonction "GetFlowSqlQuery" dans CashBalanceProcess
    /// Découpage de la construction et de l'exécution unique des requêtes (CashFlows, OthersFlows et Payment) en n Appels
    /// - CashFlows
    ///   ---------
    ///     1. OPP sans taxes et Taxes sur OPP
    ///         a. Flux
    ///         b. Si existence de flux alors Trades
    ///     2. PRM / VMG / SCU
    ///         a. Flux
    ///         b. Si existence de flux alors Trades
    /// - OtherFlows
    ///   ----------
    ///     1. Flux
    ///     2. Si existence de flux alors Trades
    /// - Payment
    ///   -------
    ///     1. Flux
    ///     2. Si existence de flux alors Trades
    /// - Autres 
    ///   ------
    ///     RAS    
    /// </summary>
    /// EG New
    public partial class CashBalanceProcess : RiskPerformanceProcessBase
    {
        #region Enum
        public enum LoadStep
        {
            /// <summary>
            /// N/A
            /// </summary>
            NA,
            /// <summary>
            /// Amount value
            /// </summary>
            Amount,
            /// <summary>
            /// Trade identifier
            /// </summary>
            Trade
        }
        public enum CashFlowsStep
        {
            /// <summary>
            /// N/A
            /// </summary>
            NA,
            /// <summary>
            /// Fee, Tax
            /// </summary>
            OPP,
            /// <summary>
            /// Premium, Variation Marging, Cash-settlement and others Flows
            /// </summary>
            /// PRM_VMG_SCU => NOT_OPP
            NOT_OPP
        }
        #endregion Enum
        /// <summary>
        /// Charger les flux de type {pFlowType} et insertion des books correspondants dans la hiérarchie
        /// <para>Les montants peuvent être:</para>
        /// <para>- All Daily Cash-Flows (Variation Margin, Daily Premium, Daily Cash Settlement, Daily Other party Payment (Fees))</para>
        /// <para>- Other Daily flows (Liquidative Option Value, Realized Margin, Unrealized Margin)</para>
        /// <para>- Daily Deposit</para>
        /// <para>- Daily Payments</para>
        /// <para>- Daily Collaterals</para>
        /// <para>- Last Cash Balance Amount</para>
        /// <para>- Last Uncovered Margin Requierement</para>
        /// Le flux sera chargé
        /// <para>- Pour une date spécifique si {pDtBusinessSpecific} est spécifié, sinon pour la date de compensation sélectionnée</para>
        /// <para>- Dans le cas du déposit, il sera chargé pour une Chambre spécifique si {pIda_Css_Specific} est spécifié, sinon pour toutes les chambres</para>
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        private void LoadFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType)
        {
            LoadFlows(pCS, pIsClearerActorExist, pFlowType, DateTime.MinValue, 0);
        }
        /// <summary>
        /// Voir surcharge
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pIdACss">Chambre</param>
        /// PM 20150323 [POC] Add FlowTypeEnum.TradeFlows
        /// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
        // RD 20170502 [22515] Modify
        private void LoadFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType, DateTime pDtBusiness, int pIdACss)
        {
            // PM 20150616 [21124] Sorti de l'alimentation de dtBusiness hors de la méthode GetFlowDataParameters()
            // puis utilisation de dtBusiness au lieu de pDtBusiness dans toute la méthode LoadFlows()
            DateTime dtBusiness = DtFunc.IsDateTimeFilled(pDtBusiness) ? pDtBusiness : m_CBHierarchy.DtBusiness;
            DataParameters dataParameters = GetFlowDataParameters(pCS, pFlowType, m_CBHierarchy.Ida_Entity, dtBusiness, pIdACss);
            
            // RD 20170502 [22515] Use global dicMaterializedViews
            Dictionary<string, string> dicMaterializedViews = null;
            if (m_IsCreateMaterializedView && (DbSvrType.dbORA == DataHelper.GetDbSvrType(m_CBHierarchy.dbConnection)))
                dicMaterializedViews = new Dictionary<string, string>();

            try
            {
                switch (pFlowType)
                {
                    case FlowTypeEnum.AllocNotFungibleFlows:
                    case FlowTypeEnum.CashFlows:
                    case FlowTypeEnum.TradeFlows:
                        if (Cst.ErrLevel.SUCCESS == LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.OPP, dicMaterializedViews))
                        {
                            LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.OPP, dicMaterializedViews);
                        }
                        if (Cst.ErrLevel.SUCCESS == LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.NOT_OPP, dicMaterializedViews))
                        {
                            LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.NOT_OPP, dicMaterializedViews);
                        }
                        break;
                    case FlowTypeEnum.OtherFlows:
                        if (Cst.ErrLevel.SUCCESS == LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, dicMaterializedViews))
                        {
                            LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, dicMaterializedViews);
                        }
                        break;
                    case FlowTypeEnum.Payment:
                    case FlowTypeEnum.SettlementPayment:
                        if (Cst.ErrLevel.SUCCESS == LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, dicMaterializedViews))
                        {
                            LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, dicMaterializedViews);
                        }
                        break;
                    //case FlowTypeEnum.TradeFlows:
                    //    if (Cst.ErrLevel.SUCCESS == LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.OPP))
                    //    {
                    //        LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.OPP);
                    //    }
                    //    if (Cst.ErrLevel.SUCCESS == LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.NOT_OPP))
                    //    {
                    //        LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, CashFlowsStep.NOT_OPP);
                    //    }
                    //    break;
                    default:
                        LoadFlows(pCS, pIsClearerActorExist, pFlowType, dtBusiness, pIdACss, dataParameters, dicMaterializedViews);
                        break;
                }
            }
            finally
            {
                // RD 20170502 [22515] Drop all materialized view
                if (dicMaterializedViews != null && dicMaterializedViews.Count > 0)
                {
                    foreach (string key in dicMaterializedViews.Keys)
                    {
                        string sqlVM_Drop = "drop materialized view " + dicMaterializedViews[key];
                        int retExecuteNonQuery = DataHelper.ExecuteNonQuery(m_CBHierarchy.dbConnection, CommandType.Text, sqlVM_Drop);
                    }
                }
            }
        }
        /// <summary>
        /// Voir surcharge
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pIdACss">Chambre</param>
        /// <param name="pDataParameters">Paramètres passés à  la requête</param>
        /// <param name="pMaterializedViews">Liste des vues materialisées</param>
        /// <returns>Cst.ErrLevel.SUCCESS = il existe des flux, Cst.ErrLevel.NOTHINGTODO = pas de flux</returns>
        // RD 20170502 [22515] Add pMaterializedViews
        private Cst.ErrLevel LoadFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType,
            DateTime pDtBusiness, int pIdACss, DataParameters pDataParameters, Dictionary<string, string> pMaterializedViews)
        {
            return LoadFlows(pCS, pIsClearerActorExist, pFlowType, pDtBusiness, pIdACss, pDataParameters, CashFlowsStep.NA, pMaterializedViews);
        }
        /// <summary>
        /// Voir surcharge
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pIdACss">Chambre</param>
        /// <param name="pDataParameters">Paramètres passés à  la requête</param>
        /// <param name="pStep">Step de construction (voir fonction GetQueryFlows)</param>
        /// <param name="pMaterializedViews">Liste des vues materialisées</param>
        /// <returns>Cst.ErrLevel.SUCCESS = il existe des flux, Cst.ErrLevel.NOTHINGTODO = pas de flux</returns>
        /// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        /// FI 20160530 [21885] Modify
        /// FI 20170208 [22151][22152] Modify
        /// PM 20170213 [21916] Ajout AllocNotFungibleFlows
        /// FI 20170316 [22950] Modify
        /// RD 20170502 [22515] Add pMaterializedViews
        // EG 20181119 PERF Correction post RC (Step 2)
        private Cst.ErrLevel LoadFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType,
            DateTime pDtBusiness, int pIdACss, DataParameters pDataParameters, CashFlowsStep pStep, Dictionary<string, string> pMaterializedViews)
        //PL 20160524 REM DateTime pDtBusiness, int pIdACss, DataParameters pDataParameters, Nullable<int> pStep)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.NOTHINGTODO;
            //PL 20160524 REM AppInstance.TraceManager.TraceDataInformation(this, string.Format("LoadFlows:{0} Step:{1}", pFlowType.ToString(), (pStep.HasValue ? pStep.ToString() : "n/a")));
            AppInstance.TraceManager.TraceDataInformation(this, string.Format("LoadFlows:{0} Step:{1}", pFlowType.ToString(), pStep.ToString()));

            StrBuilder sqlQuery = GetQueryFlows(pCS, pIsClearerActorExist, pFlowType, pDataParameters, pStep);
            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), pDataParameters);

            string message = TranslateFlowTypeForProcessLog(LoadStep.Amount, pFlowType, pStep);
            ProcessLogAddDetail_CB(pCS, message, 2, pDtBusiness, pIdACss);

            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("SQL:{0}", queryParameters.GetQueryReplaceParameters()));

            if (m_IsDataReaderMode)
            {
                #region DataReader Mode
                //PL 20160524 TEST IN PROGRESS 
                string sql = queryParameters.GetQueryReplaceParameters();

                #region Materialized View on ORACLE
                if (m_IsCreateMaterializedView && (DbSvrType.dbORA == DataHelper.GetDbSvrType(m_CBHierarchy.dbConnection)))
                {
                    // RD 20170502 [22515] Use pDicMaterializedViews
                    //Dictionary<string, string> dicMaterializedViews = new Dictionary<string, string>();
                    string materializedViewName;
                    int guard = 0;
                    int posEnd = 0;
                    int posBegin = sql.IndexOf("/* VM-Begin");
                    while ((posBegin > 0) && (guard++ < 99))
                    {
                        posEnd = sql.IndexOf("/* VM-End */", posBegin);
                        int posBeginEnd = sql.IndexOf("*/", posBegin);
                        string prefixMV = "MV" + sql.Substring(posBegin + "/* VM-Begin".Length, posBeginEnd - (posBegin + "/* VM-Begin".Length)).Trim();

                        if (pMaterializedViews.ContainsKey(prefixMV))
                        {
                            materializedViewName = pMaterializedViews[prefixMV];
                        }
                        else
                        {
                            materializedViewName ="dbo." +  m_CBHierarchy.tblCBACTOR_Work.First.Replace("CBACTOR_", prefixMV + "_");
                            pMaterializedViews.Add(prefixMV, materializedViewName);
                            string sqlVM_Create = "create materialized view " + materializedViewName + " as " + Cst.CrLf;
                            sqlVM_Create += sql.Substring(posBegin, posEnd - posBegin);

                            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("SQL:{0}", sqlVM_Create));
                            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("VM:{0}", "In progress..."));
                            int retExecuteNonQuery = DataHelper.ExecuteNonQuery(m_CBHierarchy.dbConnection, CommandType.Text, sqlVM_Create);
                            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("VM:{0}", "Created."));
                        }

                        sql = sql.Substring(0, posBegin) + " " + materializedViewName + " " + sql.Substring(posEnd + "/* VM-End */".Length);

                        posBegin = sql.IndexOf("/* VM-Begin");
                    }
                    if (pMaterializedViews.Count > 0)
                        AppInstance.TraceManager.TraceDataVerbose(this, string.Format("SQL:{0}", sql));
                }
                #endregion Materialized View on ORACLE

                AppInstance.TraceManager.TraceDataVerbose(this, string.Format("Execute:{0}", "In progress..."));
                IDataReader dr = DataHelper.ExecuteReader(m_CBHierarchy.dbConnection, CommandType.Text, sql);
                FlowTypeEnum flowType = pFlowType;
                CBBookLeaf book = null;
                CBDetFlow flow = null;
                CBBookTrade trade = null;
                int count = 0;
                while (dr.Read())
                {
                    count++;

                    //PM 20150408 [POC] Ajout d'un indicateur afin de ne pas tenir compte de certain flux
                    bool isFlowToAdd = true;
                    int ida = Convert.ToInt32(dr["IDA"]);
                    int idb = (Convert.IsDBNull(dr["IDB"]) ? 0 : Convert.ToInt32(dr["IDB"]));
                    string identifier_b = (Convert.IsDBNull(dr["B_IDENTIFIER"]) ? string.Empty : dr["B_IDENTIFIER"].ToString());
                    decimal amount = (Convert.IsDBNull(dr["AMOUNT"]) ? 0 : Convert.ToDecimal(dr["AMOUNT"]));
                    string idc = (Convert.IsDBNull(dr["IDC"]) ? string.Empty : dr["IDC"].ToString());
                    // FI 20170208 [22151][22152] Alimentation de status
                    StatusEnum status = (StatusEnum)Enum.Parse(typeof(StatusEnum), dr["STATUS"].ToString());
                    
                    

                    FlowSubTypeEnum flowSubType = FlowSubTypeEnum.None;
                    if (pFlowType == FlowTypeEnum.AllocNotFungibleFlows ||
                        pFlowType == FlowTypeEnum.CashFlows ||
                        pFlowType == FlowTypeEnum.OtherFlows ||
                        pFlowType == FlowTypeEnum.TradeFlows ||
                        pFlowType == FlowTypeEnum.LastCashBalance)
                    {
                        string flowSubTypeStr = (Convert.IsDBNull(dr["AMOUNTSUBTYPE"]) ? string.Empty : dr["AMOUNTSUBTYPE"].ToString());
                        if (Enum.IsDefined(typeof(FlowSubTypeEnum), flowSubTypeStr.Trim()))
                            flowSubType = (FlowSubTypeEnum)Enum.Parse(typeof(FlowSubTypeEnum), flowSubTypeStr, true);
                    }
                    switch (pFlowType)
                    {
                        case FlowTypeEnum.AllocNotFungibleFlows:
                            #region AllocNotFungibleFlows
                            {
                                // CC/PM 20180307 [23827] Replace DTEVENT by DTVAL
                                //DateTime dtVal = (Convert.IsDBNull(dr["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTEVENT"]));
                                DateTime dtVal = (Convert.IsDBNull(dr["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTVAL"]));
                                string productIdentifier = (Convert.IsDBNull(dr["PRODUCTIDENTIFIER"]) ? string.Empty : dr["PRODUCTIDENTIFIER"].ToString());
                                string assetCategory = (Convert.IsDBNull(dr["ASSETCATEGORY"]) ? string.Empty : dr["ASSETCATEGORY"].ToString());
                                Cst.UnderlyingAsset? assetCategoryEnum = StrFunc.IsEmpty(assetCategory) ? default(Cst.UnderlyingAsset?) : Cst.ConvertToUnderlyingAsset(assetCategory);
                                int idAsset = (Convert.IsDBNull(dr["IDASSET"]) ? 0 : Convert.ToInt32(dr["IDASSET"]));
                                SideEnum? sideEnum = null;
                                //
                                string paymentType = (Convert.IsDBNull(dr["PAYMENTTYPE"]) ? string.Empty : dr["PAYMENTTYPE"].ToString());
                                int idTax = (Convert.IsDBNull(dr["IDTAX"]) ? 0 : Convert.ToInt32(dr["IDTAX"]));
                                int idTaxDet = (Convert.IsDBNull(dr["IDTAXDET"]) ? 0 : Convert.ToInt32(dr["IDTAXDET"]));
                                string taxCountry = (Convert.IsDBNull(dr["TAXCOUNTRY"]) ? string.Empty : dr["TAXCOUNTRY"].ToString());
                                string taxType = (Convert.IsDBNull(dr["TAXTYPE"]) ? string.Empty : dr["TAXTYPE"].ToString());
                                decimal taxRate = (Convert.IsDBNull(dr["TAXRATE"]) ? 0 : Convert.ToDecimal(dr["TAXRATE"]));

                                if (dtVal != pDtBusiness)
                                {
                                    // Unsettled
                                    flowSubType = FlowSubTypeEnum.UST;
                                }
                                else if (flowSubType == FlowSubTypeEnum.GAM)
                                {
                                    // Amount => Cash Settlement
                                    flowSubType = FlowSubTypeEnum.SCU;
                                }
                                flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, default(CfiCodeCategoryEnum?), default(FuturesValuationMethodEnum?), 0, idAsset, sideEnum, status);
                            }
                            #endregion AllocNotFungibleFlows
                            break;
                        case FlowTypeEnum.CashFlows:
                            #region CashFlows
                            {
                                string paymentType = (Convert.IsDBNull(dr["PAYMENTTYPE"]) ? string.Empty : dr["PAYMENTTYPE"].ToString());
                                //
                                int idTax = (Convert.IsDBNull(dr["IDTAX"]) ? 0 : Convert.ToInt32(dr["IDTAX"]));
                                int idTaxDet = (Convert.IsDBNull(dr["IDTAXDET"]) ? 0 : Convert.ToInt32(dr["IDTAXDET"]));
                                string taxCountry = (Convert.IsDBNull(dr["TAXCOUNTRY"]) ? string.Empty : dr["TAXCOUNTRY"].ToString());
                                string taxType = (Convert.IsDBNull(dr["TAXTYPE"]) ? string.Empty : dr["TAXTYPE"].ToString());
                                decimal taxRate = (Convert.IsDBNull(dr["TAXRATE"]) ? 0 : Convert.ToDecimal(dr["TAXRATE"]));
                                int idDC = (Convert.IsDBNull(dr["IDDC"]) ? 0 : Convert.ToInt32(dr["IDDC"]));
                                //
                                string assetCategory = (Convert.IsDBNull(dr["ASSETCATEGORY"]) ? string.Empty : dr["ASSETCATEGORY"].ToString());
                                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                                int idAsset = (Convert.IsDBNull(dr["IDASSET"]) ? 0 : Convert.ToInt32(dr["IDASSET"]));
                                //
                                // PM 20150616 [21124] Lecture de DTVAL utilisée pour savoir si le flux est Unsettled ou non
                                DateTime dtVal = (Convert.IsDBNull(dr["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTVAL"]));
                                if (dtVal != pDtBusiness)
                                {
                                    // Unsettled
                                    flowSubType = FlowSubTypeEnum.UST;
                                }
                                // FI 20170217 [22862] DVA identique SCU (pas de distinction entre cashSettlement et delivery Amount)
                                if (flowSubType == FlowSubTypeEnum.DVA)
                                {
                                    flowSubType = FlowSubTypeEnum.SCU;
                                }
                                if (idTax > 0)
                                {
                                    // Arrondir le Montant des taxes
                                    EFS_Cash cash = new EFS_Cash(pCS, amount, idc);
                                    amount = cash.AmountRounded;
                                }
                                
                                flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, idDC, idAsset, status);
                            }
                            #endregion CashFlows
                            break;
                        case FlowTypeEnum.LastCashBalance:
                            #region LastCashBalance
                            {
                                int idt = Convert.ToInt32(dr["IDT"]);
                                string identifier_t = dr["T_IDENTIFIER"].ToString();
                                // PM 20150616 [21124] Add DTEVENT
                                DateTime dtEvent = (Convert.IsDBNull(dr["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTEVENT"]));

                                flow = new CBDetLastFlow(idb, ida, amount, idc, flowSubType, dtEvent, idt, identifier_t, status);
                            }
                            #endregion LastCashBalance
                            break;
                        case FlowTypeEnum.Deposit:
                            #region Deposit
                            {
                                int idt = Convert.ToInt32(dr["IDT"]);
                                string identifier_t = dr["T_IDENTIFIER"].ToString();
                                DateTime dtSysDeposit = Convert.ToDateTime(dr["DTSYS"]);
                                int ida_css = Convert.ToInt32(dr["IDA_CSS"]);
                                // FI 20170316 [22950] Alimentation de DTBUSINESS
                                DateTime dtBusiness =  Convert.ToDateTime(dr["DTBUSINESS"]);

                                flow = new CBDetDeposit(idb, ida, amount, idc, dtSysDeposit, ida_css, status);
                                trade = new CBBookTrade(idb, ida, idt, identifier_t, pFlowType, status,dtBusiness);
                            }
                            #endregion Deposit
                            break;
                        case FlowTypeEnum.Collateral:
                            #region Collateral
                            {
                                // FI 20160530 [21885] Add IDPOSCOLLATERAL and IDPOSCOLLATERALVAL
                                DateTime dtBusinessCollateral = Convert.ToDateTime(dr["DTBUSINESS"]);
                                int ida_css = (Convert.IsDBNull(dr["IDA_CSS"]) ? 0 : Convert.ToInt32(dr["IDA_CSS"]));
                                bool isValorised = Convert.ToBoolean(dr["ISVALORISED"]);
                                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                // EG 20170127 Qty Long To Decimal
                                decimal? qty = (Convert.IsDBNull(dr["QTYVAL"]) ? (decimal?)null : Convert.ToDecimal(dr["QTYVAL"]));
                                string assetCategory = (Convert.IsDBNull(dr["ASSETCATEGORY"]) ? string.Empty : dr["ASSETCATEGORY"].ToString());
                                int idAsset = (Convert.IsDBNull(dr["IDASSET"]) ? 0 : Convert.ToInt32(dr["IDASSET"]));
                                // PM 20150901 pHaircutForced devient nullable
                                //decimal haircutForced = (Convert.IsDBNull(dr["HAIRCUTFORCED"]) ? 0 : Convert.ToDecimal(dr["HAIRCUTFORCED"]));
                                decimal? haircutForced = (Convert.IsDBNull(dr["HAIRCUTFORCED"]) ? (decimal?)null : Convert.ToDecimal(dr["HAIRCUTFORCED"]));
                                int idPoscollateral = Convert.ToInt32(dr["IDPOSCOLLATERAL"]);
                                // RD 20210906 [25803] Test
                                //int idPoscollateralVal = Convert.ToInt32(dr["IDPOSCOLLATERALVAL"]);
                                int idPoscollateralVal = (Convert.IsDBNull(dr["IDPOSCOLLATERALVAL"]) ? 0 : Convert.ToInt32(dr["IDPOSCOLLATERALVAL"]));

                                flow = new CBDetCollateral(idb, ida, amount, idc, dtBusinessCollateral, ida_css, isValorised, qty,
                                    assetCategory, idAsset, haircutForced, status);
                                ((CBDetCollateral)flow).idPoscollateral = idPoscollateral;
                                ((CBDetCollateral)flow).idPoscollateralVal = idPoscollateralVal;
                            }
                            #endregion Collateral
                            break;
                        case FlowTypeEnum.OtherFlows:
                            #region OtherFlows
                            {
                                string assetCategory = (Convert.IsDBNull(dr["ASSETCATEGORY"]) ? string.Empty : dr["ASSETCATEGORY"].ToString());
                                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                                int idDC = (Convert.IsDBNull(dr["IDDC"]) ? 0 : Convert.ToInt32(dr["IDDC"]));
                                int idAsset = (Convert.IsDBNull(dr["IDASSET"]) ? 0 : Convert.ToInt32(dr["IDASSET"]));
                                string category = (Convert.IsDBNull(dr["CATEGORY"]) ? string.Empty : dr["CATEGORY"].ToString());
                                Nullable<CfiCodeCategoryEnum> categoryEnum = ReflectionTools.ConvertStringToEnumOrNullable<CfiCodeCategoryEnum>(category);
                                string futValuationMethod = (Convert.IsDBNull(dr["FUTVALUATIONMETHOD"]) ? string.Empty : dr["FUTVALUATIONMETHOD"].ToString());
                                Nullable<FuturesValuationMethodEnum> futValuationMethodEnum = ReflectionTools.ConvertStringToEnumOrNullable<FuturesValuationMethodEnum>(futValuationMethod);
                                string side = (Convert.IsDBNull(dr["SIDE"]) ? string.Empty : dr["SIDE"].ToString());
                                Nullable<SideEnum> sideEnum = ReflectionTools.ConvertStringToEnumOrNullable<SideEnum>(side);
                                // PM 20150616 [21124] Lecture de DTVAL utilisée pour savoir si le flux est Unsettled ou non
                                DateTime dtVal = (Convert.IsDBNull(dr["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTVAL"]));
                                //
                                if (flowSubType == FlowSubTypeEnum.MGR)
                                {
                                    // Margin Requierement sur le Trade (CFD)
                                    flow = new CBDetDeposit(idb, ida, amount, idc, pDtBusiness, 0, status);
                                }
                                else
                                {
                                    // Lorsque la date valeur du GrossAMount égale la date business alors transformer le GAM en CashSettlement sinon en Unsettled
                                    if (dtVal != pDtBusiness)
                                    {
                                        // Unsettled
                                        flowSubType = FlowSubTypeEnum.UST;
                                    }
                                    // PM 20151208 [21317] Ajout INT/INT sur la famille de product DSE en tant que SCU.
                                    //else if (flowSubType == FlowSubTypeEnum.GAM)
                                    else if ((flowSubType == FlowSubTypeEnum.GAM) || (flowSubType == FlowSubTypeEnum.INT))
                                    {
                                        flowSubType = FlowSubTypeEnum.SCU;
                                    }
                                    flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, assetCategoryEnum, categoryEnum, futValuationMethodEnum, idDC, idAsset, sideEnum, status);
                                }
                            }
                            #endregion OtherFlows
                            break;
                        case FlowTypeEnum.SettlementPayment:
                            #region SettlementPayment
                            {
                                bool isForward = Convert.ToBoolean(dr["ISFORWARD"]);
                                // PM 20150616 [21124] Add DTEVENT
                                DateTime dtEvent = (Convert.IsDBNull(dr["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTEVENT"]));

                                flow = new CBDetStlPayment(idb, ida, amount, idc, flowSubType, dtEvent, isForward, status);
                            }
                            #endregion SettlementPayment
                            break;
                        case FlowTypeEnum.TradeFlows:
                            #region TradeFlows
                            {
                                if (flowSubType == FlowSubTypeEnum.MGR)
                                {
                                    flow = new CBDetDeposit(idb, ida, amount, idc, pDtBusiness, 0, status);
                                }
                                else
                                {
                                    // CC/PM 20180307 [23827] Replace DTEVENT by DTVAL
                                    //DateTime dtVal = (Convert.IsDBNull(dr["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTEVENT"]));
                                    DateTime dtVal = (Convert.IsDBNull(dr["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTVAL"]));
                                    string productIdentifier = (Convert.IsDBNull(dr["PRODUCTIDENTIFIER"]) ? string.Empty : dr["PRODUCTIDENTIFIER"].ToString());
                                    string assetCategory = (Convert.IsDBNull(dr["ASSETCATEGORY"]) ? string.Empty : dr["ASSETCATEGORY"].ToString());
                                    Cst.UnderlyingAsset? assetCategoryEnum = StrFunc.IsEmpty(assetCategory) ? default(Cst.UnderlyingAsset?) : Cst.ConvertToUnderlyingAsset(assetCategory);
                                    int idAsset = (Convert.IsDBNull(dr["IDASSET"]) ? 0 : Convert.ToInt32(dr["IDASSET"]));
                                    SideEnum? sideEnum = null;
                                    //
                                    string paymentType = (Convert.IsDBNull(dr["PAYMENTTYPE"]) ? string.Empty : dr["PAYMENTTYPE"].ToString());
                                    int idTax = (Convert.IsDBNull(dr["IDTAX"]) ? 0 : Convert.ToInt32(dr["IDTAX"]));
                                    int idTaxDet = (Convert.IsDBNull(dr["IDTAXDET"]) ? 0 : Convert.ToInt32(dr["IDTAXDET"]));
                                    string taxCountry = (Convert.IsDBNull(dr["TAXCOUNTRY"]) ? string.Empty : dr["TAXCOUNTRY"].ToString());
                                    string taxType = (Convert.IsDBNull(dr["TAXTYPE"]) ? string.Empty : dr["TAXTYPE"].ToString());
                                    decimal taxRate = (Convert.IsDBNull(dr["TAXRATE"]) ? 0 : Convert.ToDecimal(dr["TAXRATE"]));
                                    //
                                    if (flowSubType == FlowSubTypeEnum.LOV)
                                    {
                                        sideEnum = (amount < 0) ? SideEnum.Sell : SideEnum.Buy;
                                    }
                                    else if (flowSubType == FlowSubTypeEnum.UMG)
                                    {
                                        // Ne pas prendre les UMG sur les Options
                                        switch (productIdentifier.Trim())
                                        {
                                            case Cst.ProductFxSimpleOption:
                                            case Cst.ProductFxDigitalOption:
                                            case Cst.ProductFxBarrierOption:
                                            case Cst.ProductFxAverageRateOption:
                                                isFlowToAdd = false;
                                                break;
                                        }
                                    }
                                    else if (dtVal != pDtBusiness)
                                    {
                                        // Unsettled
                                        flowSubType = FlowSubTypeEnum.UST;
                                    }
                                    else if ((flowSubType == FlowSubTypeEnum.CU1) || (flowSubType == FlowSubTypeEnum.CU2))
                                    {
                                        // Cash Settlement
                                        flowSubType = FlowSubTypeEnum.SCU;
                                    }
                                    else if (flowSubType == FlowSubTypeEnum.GAM)
                                    {
                                        // Amount => Cash Settlement
                                        flowSubType = FlowSubTypeEnum.SCU;
                                    }
                                    //flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtEvent, assetCategoryEnum, null, null, 0, idAsset, sideEnum);
                                    flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, default(CfiCodeCategoryEnum?), default(FuturesValuationMethodEnum?), 0, idAsset, sideEnum, status);
                                }
                            }
                            #endregion TradeFlows
                            break;
                        default:
                            flow = new CBDetFlow(idb, ida, amount, idc, flowType, flowSubType, pDtBusiness, status);
                            break;
                    }

                    //PM 20150408 [POC] Tenir compte de l'indicateur afin de savoir s'il faut ajouter le flux
                    if (isFlowToAdd)
                    {
                        book = new CBBookLeaf(ida, idb, identifier_b, flow);
                        m_CBHierarchy.Add(book);
                        if (null != trade)
                        {
                            m_CBHierarchy.Add(trade);
                            trade = null;
                        }
                    }
                }
                if (null != dr)
                {
                    dr.Close();
                }
                if (count > 0)
                {
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
                AppInstance.TraceManager.TraceDataInformation(this, string.Format("RowCount:{0}", count.ToString()));
                #endregion DataReader Mode
            }
            else
            {
                #region DataSet Mode
                AppInstance.TraceManager.TraceDataVerbose(this, string.Format("Execute:{0}", "In progress..."));
                DataSet dsFlows;
                if (m_IsDataValueMode)
                    dsFlows = DataHelper.ExecuteDataset(m_CBHierarchy.dbConnection, CommandType.Text, queryParameters.GetQueryReplaceParameters());
                else
                    dsFlows = DataHelper.ExecuteDataset(m_CBHierarchy.dbConnection, CommandType.Text, queryParameters.query, queryParameters.parameters.GetArrayDbParameter());
                DataRow[] drFlows = dsFlows.Tables[0].Select();
                AppInstance.TraceManager.TraceDataInformation(this, string.Format("RowCount:{0}", drFlows.Length.ToString()));

                if (drFlows.Length > 0)
                {
                    FlowTypeEnum flowType = pFlowType;
                    CBBookLeaf book = null;
                    CBDetFlow flow = null;
                    CBBookTrade trade = null;

                    foreach (DataRow rowFlow in drFlows)
                    {
                        //PM 20150408 [POC] Ajout d'un indicateur afin de ne pas tenir compte de certain flux
                        bool isFlowToAdd = true;
                        int ida = Convert.ToInt32(rowFlow["IDA"]);
                        int idb = (Convert.IsDBNull(rowFlow["IDB"]) ? 0 : Convert.ToInt32(rowFlow["IDB"]));
                        string identifier_b = (Convert.IsDBNull(rowFlow["B_IDENTIFIER"]) ? string.Empty : rowFlow["B_IDENTIFIER"].ToString());
                        decimal amount = (Convert.IsDBNull(rowFlow["AMOUNT"]) ? 0 : Convert.ToDecimal(rowFlow["AMOUNT"]));
                        string idc = (Convert.IsDBNull(rowFlow["IDC"]) ? string.Empty : rowFlow["IDC"].ToString());
                        // FI 20170208 [22151][22152] Alimentation de status
                        StatusEnum status = (StatusEnum)Enum.Parse(typeof(StatusEnum), rowFlow["STATUS"].ToString());

                        FlowSubTypeEnum flowSubType = FlowSubTypeEnum.None;
                        if (pFlowType == FlowTypeEnum.AllocNotFungibleFlows ||
                            pFlowType == FlowTypeEnum.CashFlows ||
                            pFlowType == FlowTypeEnum.OtherFlows ||
                            pFlowType == FlowTypeEnum.TradeFlows ||
                            pFlowType == FlowTypeEnum.LastCashBalance)
                        {
                            string flowSubTypeStr = (Convert.IsDBNull(rowFlow["AMOUNTSUBTYPE"]) ? string.Empty : rowFlow["AMOUNTSUBTYPE"].ToString());
                            if (Enum.IsDefined(typeof(FlowSubTypeEnum), flowSubTypeStr.Trim()))
                                flowSubType = (FlowSubTypeEnum)Enum.Parse(typeof(FlowSubTypeEnum), flowSubTypeStr, true);
                        }
                        //
                        switch (pFlowType)
                        {
                            case FlowTypeEnum.AllocNotFungibleFlows:
                                #region AllocNotFungibleFlows
                                {
                                    DateTime dtVal = (Convert.IsDBNull(rowFlow["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTVAL"]));
                                    string productIdentifier = (Convert.IsDBNull(rowFlow["PRODUCTIDENTIFIER"]) ? string.Empty : rowFlow["PRODUCTIDENTIFIER"].ToString());
                                    string assetCategory = (Convert.IsDBNull(rowFlow["ASSETCATEGORY"]) ? string.Empty : rowFlow["ASSETCATEGORY"].ToString());
                                    Cst.UnderlyingAsset? assetCategoryEnum = StrFunc.IsEmpty(assetCategory) ? default(Cst.UnderlyingAsset?) : Cst.ConvertToUnderlyingAsset(assetCategory);
                                    int idAsset = (Convert.IsDBNull(rowFlow["IDASSET"]) ? 0 : Convert.ToInt32(rowFlow["IDASSET"]));
                                    SideEnum? sideEnum = null;
                                    //
                                    string paymentType = (Convert.IsDBNull(rowFlow["PAYMENTTYPE"]) ? string.Empty : rowFlow["PAYMENTTYPE"].ToString());
                                    int idTax = (Convert.IsDBNull(rowFlow["IDTAX"]) ? 0 : Convert.ToInt32(rowFlow["IDTAX"]));
                                    int idTaxDet = (Convert.IsDBNull(rowFlow["IDTAXDET"]) ? 0 : Convert.ToInt32(rowFlow["IDTAXDET"]));
                                    string taxCountry = (Convert.IsDBNull(rowFlow["TAXCOUNTRY"]) ? string.Empty : rowFlow["TAXCOUNTRY"].ToString());
                                    string taxType = (Convert.IsDBNull(rowFlow["TAXTYPE"]) ? string.Empty : rowFlow["TAXTYPE"].ToString());
                                    decimal taxRate = (Convert.IsDBNull(rowFlow["TAXRATE"]) ? 0 : Convert.ToDecimal(rowFlow["TAXRATE"]));

                                    if (dtVal != pDtBusiness)
                                    {
                                        // Unsettled
                                        flowSubType = FlowSubTypeEnum.UST;
                                    }
                                    else if (flowSubType == FlowSubTypeEnum.GAM)
                                    {
                                        // Amount => Cash Settlement
                                        flowSubType = FlowSubTypeEnum.SCU;
                                    }
                                    flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, default(CfiCodeCategoryEnum?), default(FuturesValuationMethodEnum?), 0, idAsset, sideEnum, status);
                                }
                                #endregion AllocNotFungibleFlows
                                break;
                            case FlowTypeEnum.CashFlows:
                                #region CashFlows
                                {
                                    string paymentType = (Convert.IsDBNull(rowFlow["PAYMENTTYPE"]) ? string.Empty : rowFlow["PAYMENTTYPE"].ToString());
                                    //
                                    int idTax = (Convert.IsDBNull(rowFlow["IDTAX"]) ? 0 : Convert.ToInt32(rowFlow["IDTAX"]));
                                    int idTaxDet = (Convert.IsDBNull(rowFlow["IDTAXDET"]) ? 0 : Convert.ToInt32(rowFlow["IDTAXDET"]));
                                    string taxCountry = (Convert.IsDBNull(rowFlow["TAXCOUNTRY"]) ? string.Empty : rowFlow["TAXCOUNTRY"].ToString());
                                    string taxType = (Convert.IsDBNull(rowFlow["TAXTYPE"]) ? string.Empty : rowFlow["TAXTYPE"].ToString());
                                    decimal taxRate = (Convert.IsDBNull(rowFlow["TAXRATE"]) ? 0 : Convert.ToDecimal(rowFlow["TAXRATE"]));
                                    int idDC = (Convert.IsDBNull(rowFlow["IDDC"]) ? 0 : Convert.ToInt32(rowFlow["IDDC"]));
                                    //
                                    string assetCategory = (Convert.IsDBNull(rowFlow["ASSETCATEGORY"]) ? string.Empty : rowFlow["ASSETCATEGORY"].ToString());
                                    Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                                    int idAsset = (Convert.IsDBNull(rowFlow["IDASSET"]) ? 0 : Convert.ToInt32(rowFlow["IDASSET"]));
                                    //
                                    // PM 20150616 [21124] Lecture de DTVAL utilisée pour savoir si le flux est Unsettled ou non
                                    DateTime dtVal = (Convert.IsDBNull(rowFlow["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTVAL"]));
                                    if (dtVal != pDtBusiness)
                                    {
                                        // Unsettled
                                        flowSubType = FlowSubTypeEnum.UST;
                                    }
                                    // FI 20170217 [22862] DVA identique SCU (pas de distinction entre cashSettlement et delivery Amount)
                                    if (flowSubType == FlowSubTypeEnum.DVA)
                                    {
                                        flowSubType = FlowSubTypeEnum.SCU;
                                    }
                                    if (idTax > 0)
                                    {
                                        // Arrondir le Montant des taxes
                                        EFS_Cash cash = new EFS_Cash(pCS, amount, idc);
                                        amount = cash.AmountRounded;
                                    }
                                    
                                    flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, idDC, idAsset, status);
                                }
                                #endregion CashFlows
                                break;
                            case FlowTypeEnum.LastCashBalance:
                                #region LastCashBalance
                                {
                                    int idt = Convert.ToInt32(rowFlow["IDT"]);
                                    string identifier_t = rowFlow["T_IDENTIFIER"].ToString();
                                    // PM 20150616 [21124] Add DTEVENT
                                    DateTime dtEvent = (Convert.IsDBNull(rowFlow["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTEVENT"]));

                                    flow = new CBDetLastFlow(idb, ida, amount, idc, flowSubType, dtEvent, idt, identifier_t, status);
                                }
                                #endregion LastCashBalance
                                break;
                            case FlowTypeEnum.Deposit:
                                #region Deposit
                                {
                                    int idt = Convert.ToInt32(rowFlow["IDT"]);
                                    string identifier_t = rowFlow["T_IDENTIFIER"].ToString();
                                    DateTime dtSysDeposit = Convert.ToDateTime(rowFlow["DTSYS"]);
                                    int ida_css = Convert.ToInt32(rowFlow["IDA_CSS"]);
                                    // FI 20170316 [22950] Alimentation de DTBUSINESS
                                    DateTime dtBusiness = Convert.ToDateTime(rowFlow["DTBUSINESS"]);

                                    flow = new CBDetDeposit(idb, ida, amount, idc, dtSysDeposit, ida_css, status);
                                    trade = new CBBookTrade(idb, ida, idt, identifier_t, pFlowType, status, dtBusiness);
                                }
                                #endregion Deposit
                                break;
                            case FlowTypeEnum.Collateral:
                                #region Collateral
                                {
                                    // FI 20160530 [21885] Add IDPOSCOLLATERAL and IDPOSCOLLATERALVAL
                                    DateTime dtBusinessCollateral = Convert.ToDateTime(rowFlow["DTBUSINESS"]);
                                    int ida_css = (Convert.IsDBNull(rowFlow["IDA_CSS"]) ? 0 : Convert.ToInt32(rowFlow["IDA_CSS"]));
                                    bool isValorised = Convert.ToBoolean(rowFlow["ISVALORISED"]);
                                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                    // EG 20170127 Qty Long To Decimal
                                    decimal? qty = (Convert.IsDBNull(rowFlow["QTYVAL"]) ? (decimal?)null : Convert.ToDecimal(rowFlow["QTYVAL"]));
                                    string assetCategory = (Convert.IsDBNull(rowFlow["ASSETCATEGORY"]) ? string.Empty : rowFlow["ASSETCATEGORY"].ToString());
                                    int idAsset = (Convert.IsDBNull(rowFlow["IDASSET"]) ? 0 : Convert.ToInt32(rowFlow["IDASSET"]));
                                    // PM 20150901 pHaircutForced devient nullable
                                    //decimal haircutForced = (Convert.IsDBNull(rowFlow["HAIRCUTFORCED"]) ? 0 : Convert.ToDecimal(rowFlow["HAIRCUTFORCED"]));
                                    decimal? haircutForced = (Convert.IsDBNull(rowFlow["HAIRCUTFORCED"]) ? (decimal?)null : Convert.ToDecimal(rowFlow["HAIRCUTFORCED"]));
                                    int idPoscollateral = Convert.ToInt32(rowFlow["IDPOSCOLLATERAL"]);
                                    // RD 20171212 [23638] IDPOSCOLLATERALVAL peut être null
                                    //int idPoscollateralVal = Convert.ToInt32(rowFlow["IDPOSCOLLATERALVAL"]);
                                    int idPoscollateralVal = (Convert.IsDBNull(rowFlow["IDPOSCOLLATERALVAL"]) ? 0 : Convert.ToInt32(rowFlow["IDPOSCOLLATERALVAL"]));

                                    flow = new CBDetCollateral(idb, ida, amount, idc, dtBusinessCollateral, ida_css, isValorised, qty,
                                        assetCategory, idAsset, haircutForced, status);

                                    ((CBDetCollateral)flow).idPoscollateral = idPoscollateral;
                                    ((CBDetCollateral)flow).idPoscollateralVal = idPoscollateralVal;
                                }
                                #endregion Collateral
                                break;
                            case FlowTypeEnum.OtherFlows:
                                #region OtherFlows
                                {
                                    string assetCategory = (Convert.IsDBNull(rowFlow["ASSETCATEGORY"]) ? string.Empty : rowFlow["ASSETCATEGORY"].ToString());
                                    Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                                    int idDC = (Convert.IsDBNull(rowFlow["IDDC"]) ? 0 : Convert.ToInt32(rowFlow["IDDC"]));
                                    int idAsset = (Convert.IsDBNull(rowFlow["IDASSET"]) ? 0 : Convert.ToInt32(rowFlow["IDASSET"]));
                                    string category = (Convert.IsDBNull(rowFlow["CATEGORY"]) ? string.Empty : rowFlow["CATEGORY"].ToString());
                                    Nullable<CfiCodeCategoryEnum> categoryEnum = ReflectionTools.ConvertStringToEnumOrNullable<CfiCodeCategoryEnum>(category);
                                    string futValuationMethod = (Convert.IsDBNull(rowFlow["FUTVALUATIONMETHOD"]) ? string.Empty : rowFlow["FUTVALUATIONMETHOD"].ToString());
                                    FuturesValuationMethodEnum? futValuationMethodEnum = ReflectionTools.ConvertStringToEnumOrNullable<FuturesValuationMethodEnum>(futValuationMethod);
                                    string side = (Convert.IsDBNull(rowFlow["SIDE"]) ? string.Empty : rowFlow["SIDE"].ToString());
                                    Nullable<SideEnum> sideEnum = ReflectionTools.ConvertStringToEnumOrNullable<SideEnum>(side);
                                    // PM 20150616 [21124] Lecture de DTVAL utilisée pour savoir si le flux est Unsettled ou non
                                    DateTime dtVal = (Convert.IsDBNull(rowFlow["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTVAL"]));
                                    //
                                    if (flowSubType == FlowSubTypeEnum.MGR)
                                    {
                                        // Margin Requierement sur le Trade (CFD)
                                        flow = new CBDetDeposit(idb, ida, amount, idc, pDtBusiness, 0, status);
                                    }
                                    else
                                    {
                                        // Lorsque la date valeur du GrossAMount égale la date business alors transformer le GAM en CashSettlement sinon en Unsettled
                                        if (dtVal != pDtBusiness)
                                        {
                                            // Unsettled
                                            flowSubType = FlowSubTypeEnum.UST;
                                        }
                                        // PM 20151208 [21317] Ajout INT/INT sur la famille de product DSE en tant que SCU.
                                        //else if (flowSubType == FlowSubTypeEnum.GAM)
                                        else if ((flowSubType == FlowSubTypeEnum.GAM) || (flowSubType == FlowSubTypeEnum.INT))
                                        {
                                            flowSubType = FlowSubTypeEnum.SCU;
                                        }
                                        flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, assetCategoryEnum, categoryEnum, futValuationMethodEnum, idDC, idAsset, sideEnum, status);
                                    }
                                }
                                #endregion OtherFlows
                                break;
                            case FlowTypeEnum.SettlementPayment:
                                #region SettlementPayment
                                {
                                    bool isForward = Convert.ToBoolean(rowFlow["ISFORWARD"]);
                                    // PM 20150616 [21124] Add DTEVENT
                                    DateTime dtEvent = (Convert.IsDBNull(rowFlow["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTEVENT"]));

                                    flow = new CBDetStlPayment(idb, ida, amount, idc, flowSubType, dtEvent, isForward, status);
                                }
                                #endregion SettlementPayment
                                break;
                            case FlowTypeEnum.TradeFlows: // PM 20150324 [POC] Add TradeFlows
                                #region TradeFlows
                                {
                                    if (flowSubType == FlowSubTypeEnum.MGR)
                                    {
                                        flow = new CBDetDeposit(idb, ida, amount, idc, pDtBusiness, 0, status);
                                    }
                                    else
                                    {
                                        //DateTime dtEvent = (Convert.IsDBNull(rowFlow["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTEVENT"]));
                                        DateTime dtVal = (Convert.IsDBNull(rowFlow["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(rowFlow["DTVAL"]));
                                        string productIdentifier = (Convert.IsDBNull(rowFlow["PRODUCTIDENTIFIER"]) ? string.Empty : rowFlow["PRODUCTIDENTIFIER"].ToString());
                                        string assetCategory = (Convert.IsDBNull(rowFlow["ASSETCATEGORY"]) ? string.Empty : rowFlow["ASSETCATEGORY"].ToString());
                                        Cst.UnderlyingAsset? assetCategoryEnum = StrFunc.IsEmpty(assetCategory) ? default(Cst.UnderlyingAsset?) : Cst.ConvertToUnderlyingAsset(assetCategory);
                                        int idAsset = (Convert.IsDBNull(rowFlow["IDASSET"]) ? 0 : Convert.ToInt32(rowFlow["IDASSET"]));
                                        SideEnum? sideEnum = null;
                                        //
                                        string paymentType = (Convert.IsDBNull(rowFlow["PAYMENTTYPE"]) ? string.Empty : rowFlow["PAYMENTTYPE"].ToString());
                                        int idTax = (Convert.IsDBNull(rowFlow["IDTAX"]) ? 0 : Convert.ToInt32(rowFlow["IDTAX"]));
                                        int idTaxDet = (Convert.IsDBNull(rowFlow["IDTAXDET"]) ? 0 : Convert.ToInt32(rowFlow["IDTAXDET"]));
                                        string taxCountry = (Convert.IsDBNull(rowFlow["TAXCOUNTRY"]) ? string.Empty : rowFlow["TAXCOUNTRY"].ToString());
                                        string taxType = (Convert.IsDBNull(rowFlow["TAXTYPE"]) ? string.Empty : rowFlow["TAXTYPE"].ToString());
                                        decimal taxRate = (Convert.IsDBNull(rowFlow["TAXRATE"]) ? 0 : Convert.ToDecimal(rowFlow["TAXRATE"]));
                                        //
                                        if (flowSubType == FlowSubTypeEnum.LOV)
                                        {
                                            sideEnum = (amount < 0) ? SideEnum.Sell : SideEnum.Buy;
                                        }
                                        else if (flowSubType == FlowSubTypeEnum.UMG)
                                        {
                                            // Ne pas prendre les UMG sur les Options
                                            switch (productIdentifier.Trim())
                                            {
                                                case Cst.ProductFxSimpleOption:
                                                case Cst.ProductFxDigitalOption:
                                                case Cst.ProductFxBarrierOption:
                                                case Cst.ProductFxAverageRateOption:
                                                    isFlowToAdd = false;
                                                    break;
                                            }
                                        }
                                        else if (dtVal != pDtBusiness)
                                        {
                                            // Unsettled
                                            flowSubType = FlowSubTypeEnum.UST;
                                        }
                                        else if ((flowSubType == FlowSubTypeEnum.CU1) || (flowSubType == FlowSubTypeEnum.CU2))
                                        {
                                            // Cash Settlement
                                            flowSubType = FlowSubTypeEnum.SCU;
                                        }
                                        else if (flowSubType == FlowSubTypeEnum.GAM)
                                        {
                                            // Amount => Cash Settlement
                                            flowSubType = FlowSubTypeEnum.SCU;
                                        }
                                        //flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtEvent, assetCategoryEnum, null, null, 0, idAsset, sideEnum);
                                        flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, default(CfiCodeCategoryEnum?), default(FuturesValuationMethodEnum?), 0, idAsset, sideEnum, status);
                                    }
                                }
                                #endregion TradeFlows
                                break;
                            default:
                                flow = new CBDetFlow(idb, ida, amount, idc, flowType, flowSubType, pDtBusiness, status);
                                break;
                        }

                        //PM 20150408 [POC] Tenir compte de l'indicateur afin de savoir s'il faut ajouter le flux
                        if (isFlowToAdd)
                        {
                            book = new CBBookLeaf(ida, idb, identifier_b, flow);
                            m_CBHierarchy.Add(book);
                            if (null != trade)
                            {
                                m_CBHierarchy.Add(trade);
                                trade = null;
                            }
                        }
                    }
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
                #endregion DataSet Mode
            }

            if (codeReturn != Cst.ErrLevel.SUCCESS)
            {
                #region No Amount
                switch (pFlowType)
                {
                    case FlowTypeEnum.AllocNotFungibleFlows:
                        // CB_LoadAmount_NoFeesFound / CB_LoadAmount_NoCASHFLOWS-ETDFound
                        if (pStep == CashFlowsStep.OPP)
                            message = "LOG-04010";
                        else
                            message = "LOG-04021";
                        break;
                    case FlowTypeEnum.OtherFlows:
                        message = string.Empty;
                        break;
                    case FlowTypeEnum.CashFlows:
                        // CB_LoadAmount_NoFeesFound / CB_LoadAmount_NoCASHFLOWS-ETDFound
                        //PL 20160524 REM if (1 == pStep)
                        if (pStep == CashFlowsStep.OPP)
                            message = "LOG-04010";
                        else
                            message = "LOG-04021";
                        break;
                    case FlowTypeEnum.Deposit:
                        // CB_LoadAmount_NoDEPOSITFound
                        if (IntFunc.IsFilledAndNoZero(pIdACss))
                            message = "LOG-04015";
                        else
                            message = "LOG-04011";
                        break;
                    case FlowTypeEnum.Payment:
                    case FlowTypeEnum.SettlementPayment:
                        // CB_LoadAmount_NoPAYMENTFound
                        message = "LOG-04012";
                        break;
                    case FlowTypeEnum.Collateral:
                        // CB_LoadAmount_NoCOLLATERALFound
                        message = "LOG-04013";
                        break;
                    case FlowTypeEnum.LastCashBalance:
                        // CB_LoadAmount_NoLASTCASHBALANCEFound
                        message = "LOG-04014";
                        break;
                    case FlowTypeEnum.TradeFlows:
                        if (pStep == CashFlowsStep.OPP)
                        {
                            // CB_LoadAmount_NoFeesFound
                            message = "LOG-04010";
                        }
                        else
                        {
                            // CB_LoadAmount_NoCASHFLOWS-OTCFound
                            message = "LOG-04022";
                        }
                        break;
                    default:
                        message = Ressource.GetString(pFlowType.ToString());
                        break;
                }
                #endregion
                ProcessLogAddDetail_CB(pCS, message, 2, pDtBusiness, pIdACss);
            }

            return codeReturn;
        }

        /// <summary>
        /// Chargement des trades candidats, types de flux concernés : CashFlows, OtherFlows, Payment
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pIdACss">Chambre</param>
        /// <param name="pDataParameters">Paramètres passés à  la requête</param>
        /// <param name="pMaterializedViews">Liste des vues materialisées</param>        
        // RD 20170502 [22515] Add pMaterializedViews
        private void LoadTradeFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType,
            DateTime pDtBusiness, int pIdACss, DataParameters pDataParameters, Dictionary<string, string> pMaterializedViews)
        {
            LoadTradeFlows(pCS, pIsClearerActorExist, pFlowType, pDtBusiness, pIdACss, pDataParameters, CashFlowsStep.NA, pMaterializedViews);
        }

        /// <summary>
        /// Chargement des trades candidats, types de flux concernés : CashFlows, OtherFlows, Payment
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDtBusiness">Date de compensation</param>
        /// <param name="pIdACss">Chambre</param>
        /// <param name="pDataParameters">Paramètres passés à  la requête</param>
        /// <param name="pStep">Step de construction (voir fonction GetQueryFlows)</param>
        /// <param name="pMaterializedViews">Liste des vues materialisées</param>
        /// FI 20170208 [22151][22152] Modify
        /// FI 20170316 [22950] Modify
        /// RD 20170502 [22515] Add pMaterializedViews
        // EG 20181119 PERF Correction post RC (Step 2)
        private void LoadTradeFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType,
            DateTime pDtBusiness, int pIdACss, DataParameters pDataParameters, CashFlowsStep pStep, Dictionary<string, string> pMaterializedViews)
        {
            //PL 20160524 REM AppInstance.TraceManager.TraceDataInformation(this, string.Format("LoadTradeFlows:{0} Step:{1}", pFlowType.ToString(), (pStep.HasValue ? pStep.ToString() : "n/a")));
            AppInstance.TraceManager.TraceDataInformation(this, string.Format("LoadTradeFlows:{0} Step:{1}", pFlowType.ToString(), pStep.ToString()));

            StrBuilder sqlQuery = GetQueryTrades(pCS, pIsClearerActorExist, pFlowType, pDataParameters, pStep);
            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), pDataParameters);

            string message = TranslateFlowTypeForProcessLog(LoadStep.Trade, pFlowType, pStep);
            ProcessLogAddDetail_CB(pCS, message, 2, pDtBusiness, pIdACss);

            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("SQL:{0}", queryParameters.GetQueryReplaceParameters()));

            if (m_IsDataReaderMode)
            {
                #region DataReader Mode
                //PL 20160524 TEST IN PROGRESS 
                string sql = queryParameters.GetQueryReplaceParameters();

                #region Materialized View on ORACLE
                // RD 20170502 [22515] Use pMaterializedViews
                //Dictionary<string, string> dicMaterializedViews = new Dictionary<string, string>();
                if (m_IsCreateMaterializedView && (DbSvrType.dbORA == DataHelper.GetDbSvrType(m_CBHierarchy.dbConnection)))
                {
                    string materializedViewName;
                    int guard = 0;
                    int posBegin = sql.IndexOf("/* VM-Begin");
                    while ((posBegin > 0) && (guard++ < 99))
                    {
                        int posEnd = sql.IndexOf("/* VM-End */", posBegin);
                        int posBeginEnd = sql.IndexOf("*/", posBegin);
                        string prefixMV = "MV" + sql.Substring(posBegin + "/* VM-Begin".Length, posBeginEnd - (posBegin + "/* VM-Begin".Length)).Trim();

                        if (pMaterializedViews.ContainsKey(prefixMV))
                        {
                            materializedViewName = pMaterializedViews[prefixMV];
                        }
                        else
                        {
                            materializedViewName = "dbo." + m_CBHierarchy.tblCBACTOR_Work.First.Replace("CBACTOR_", prefixMV + "_");
                            pMaterializedViews.Add(prefixMV, materializedViewName);
                            // RD 20170502 [22515] Create materialized view
                            string sqlVM_Create = "create materialized view " + materializedViewName + " as " + Cst.CrLf;
                            sqlVM_Create += sql.Substring(posBegin, posEnd - posBegin);

                            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("SQL:{0}", sqlVM_Create));
                            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("VM:{0}", "In progress..."));
                            int retExecuteNonQuery = DataHelper.ExecuteNonQuery(m_CBHierarchy.dbConnection, CommandType.Text, sqlVM_Create);
                            AppInstance.TraceManager.TraceDataVerbose(this, string.Format("VM:{0}", "Created."));
                        }

                        sql = sql.Substring(0, posBegin) + " " + materializedViewName + " " + sql.Substring(posEnd + "/* VM-End */".Length);

                        posBegin = sql.IndexOf("/* VM-Begin");
                    }
                    if (pMaterializedViews.Count > 0)
                        AppInstance.TraceManager.TraceDataVerbose(this, string.Format("SQL:{0}", sql));
                }
                #endregion Materialized View on ORACLE

                AppInstance.TraceManager.TraceDataVerbose(this, string.Format("Execute:{0}", "In progress..."));
                IDataReader dr = DataHelper.ExecuteReader(m_CBHierarchy.dbConnection, CommandType.Text, sql);
                CBBookTrade trade = null;
                int count = 0;
                while (dr.Read())
                {
                    count++;

                    int ida = Convert.ToInt32(dr["IDA"]);

                    // RD 20131014 [19018] En principe le Book  est obligatoire, 
                    // mais par mesure de sécurité on laisse le code suivante en place.
                    //int idb = Convert.ToInt32(rowTrade["IDB"]);
                    int idb = (Convert.IsDBNull(dr["IDB"]) ? 0 : Convert.ToInt32(dr["IDB"]));

                    int idt = Convert.ToInt32(dr["IDT"]);
                    string identifier_t = dr["T_IDENTIFIER"].ToString();
                    // FI 20170208 [22151][22152] alimentation de status
                    StatusEnum status = (StatusEnum)Enum.Parse(typeof(StatusEnum), dr["STATUS"].ToString());

                    // FI 20170316 [22950] Alimentation de DTBUSINESS
                    DateTime dtBusiness = (Convert.IsDBNull(dr["DTBUSINESS"]) ? DateTime.MinValue : Convert.ToDateTime(dr["DTBUSINESS"]));


                    // PM 20150401 [POC] Considérer OtherFlows et TradeFlows comme des CashFlows
                    // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                    if ((pFlowType == FlowTypeEnum.OtherFlows) || (pFlowType == FlowTypeEnum.TradeFlows) || (pFlowType == FlowTypeEnum.AllocNotFungibleFlows))
                    {
                        pFlowType = FlowTypeEnum.CashFlows;
                    }
                    trade = new CBBookTrade(idb, ida, idt, identifier_t, pFlowType, status, dtBusiness); 
                    m_CBHierarchy.Add(trade);
                }
                
                if (null != dr)
                    dr.Close();
                
                AppInstance.TraceManager.TraceDataInformation(this, string.Format("RowCount:{0}", count.ToString()));

                // RD 20170502 [22515] Drop all materialized views globaly
                //if (dicMaterializedViews.Count > 0)
                //{
                //    foreach (string key in dicMaterializedViews.Keys)
                //    {
                //        string sqlVM_Drop = "drop materialized view " + dicMaterializedViews[key];
                //        int retExecuteNonQuery = DataHelper.ExecuteNonQuery(m_CBHierarchy.dbConnection, CommandType.Text, sqlVM_Drop);
                //    }
                //}
                #endregion DataReader Mode
            }
            else
            {
                #region DataSet Mode
                AppInstance.TraceManager.TraceDataVerbose(this, string.Format("Execute:{0}", "In progress..."));
                DataSet dsTrades;
                if (m_IsDataValueMode)
                    dsTrades = DataHelper.ExecuteDataset(pCS, CommandType.Text, queryParameters.GetQueryReplaceParameters());
                else
                    dsTrades = DataHelper.ExecuteDataset(pCS, CommandType.Text, queryParameters.query, queryParameters.parameters.GetArrayDbParameter());
                DataRow[] drTrades = dsTrades.Tables[0].Select();
                AppInstance.TraceManager.TraceDataInformation(this, string.Format("RowCount:{0}", drTrades.Length.ToString()));

                if (0 < drTrades.Length)
                {
                    CBBookTrade trade = null;

                    foreach (DataRow rowTrade in drTrades)
                    {
                        int ida = Convert.ToInt32(rowTrade["IDA"]);

                        // RD 20131014 [19018] En principe le Book  est obligatoire, 
                        // mais par mesure de sécurité on laisse le code suivante en place.
                        //int idb = Convert.ToInt32(rowTrade["IDB"]);
                        int idb = (Convert.IsDBNull(rowTrade["IDB"]) ? 0 : Convert.ToInt32(rowTrade["IDB"]));

                        int idt = Convert.ToInt32(rowTrade["IDT"]);
                        string identifier_t = rowTrade["T_IDENTIFIER"].ToString();

                        // FI 20170208 [22151][22152] alimentation de status
                        StatusEnum status = (StatusEnum)Enum.Parse(typeof(StatusEnum), rowTrade["STATUS"].ToString());

                        // FI 20170316 [22950] Alimentation de DTBUSINESS
                        DateTime dtBusiness = (Convert.IsDBNull(rowTrade["DTBUSINESS"]) ? DateTime.MinValue : Convert.ToDateTime(rowTrade["DTBUSINESS"]));

                        // PM 20150401 [POC] Considérer OtherFlows et TradeFlows comme des CashFlows
                        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                        if ((pFlowType == FlowTypeEnum.OtherFlows) || (pFlowType == FlowTypeEnum.TradeFlows) || (pFlowType == FlowTypeEnum.AllocNotFungibleFlows))
                        {
                            pFlowType = FlowTypeEnum.CashFlows;
                        }
                        trade = new CBBookTrade(idb, ida, idt, identifier_t, pFlowType, status, dtBusiness);   
                        m_CBHierarchy.Add(trade);
                    }
                }
                #endregion DataSet Mode
            }
        }

        private string TranslateFlowTypeForProcessLog(LoadStep pLoadStep, FlowTypeEnum pFlowType, CashFlowsStep pStep)
        {
            string message = pFlowType.ToString();
            switch (pFlowType)
            {
                // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                case FlowTypeEnum.AllocNotFungibleFlows:
                    if (pStep == CashFlowsStep.OPP)
                    {
                        message = "Fee, Commission, Brokerage";
                    }
                    else
                    {
                        message = "Flows: " + FlowTypeEnum.AllocNotFungibleFlows.ToString();
                    }
                    break;
                case FlowTypeEnum.CashFlows:
                    if (pStep == CashFlowsStep.OPP)
                        message = "Fee, Commission, Brokerage";
                    else if (pStep == CashFlowsStep.NOT_OPP)
                        message = "Premium, Var. Margin, Cash-Settlt.";
                    break;
                case FlowTypeEnum.TradeFlows:
                    if (pStep == CashFlowsStep.OPP)
                    {
                        message = "Fee, Commission, Brokerage";
                    }
                    else
                    {
                        message = "OTC " + FlowTypeEnum.CashFlows.ToString();
                    }
                    break;
                case FlowTypeEnum.LastCashBalance:
                    message = "Prev. CashBalance";
                    break;
                case FlowTypeEnum.Deposit:
                    message = "Init. Margin";
                    break;
                case FlowTypeEnum.OtherFlows:
                    message = "Unrealised, P&L, Coupon,  etc.";
                    break;
                case FlowTypeEnum.Payment:
                case FlowTypeEnum.SettlementPayment:
                    message = "Deposit/Withdrawal";
                    break;
            }
            switch (pLoadStep)
            {
                case LoadStep.Amount:
                    message += ": Loading Amounts in progress...";
                    break;
                case LoadStep.Trade:
                    message += ": Loading References in progress...";
                    break;
                default:
                    message += ": Loading Data in progress...";
                    break;
            }
            return message;
        }

        /// <summary>
        /// Add a record into Process Log
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pMsg"></param>
        /// <param name="pLevelOrder"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdACss"></param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void ProcessLogAddDetail_CB(string pCS, string pMsg, int pLevelOrder, DateTime pDtBusiness, int pIdACss)
        {
            if (!String.IsNullOrEmpty(pMsg))
            {
                DateTime dtBusiness = (DtFunc.IsDateTimeFilled(pDtBusiness) ? pDtBusiness : m_CBHierarchy.DtBusiness);
                string identifier_Css = string.Empty;
                if (IntFunc.IsFilledAndNoZero(pIdACss))
                {
                    SQL_Actor sql_Css = new SQL_Actor(CSTools.SetCacheOn(pCS), pIdACss);
                    if (sql_Css.LoadTable(new string[] { "IDENTIFIER" }))
                        identifier_Css = sql_Css.Identifier;
                }
                if (String.IsNullOrEmpty(identifier_Css))
                    ProcessLogAddDetail(new ProcessLogParameters(ProcessStateTools.StatusNoneEnum, ErrorManager.DetailEnum.NONE, pLevelOrder), pMsg, DtFunc.DateTimeToStringDateISO(dtBusiness));
                else
                    ProcessLogAddDetail(new ProcessLogParameters(ProcessStateTools.StatusNoneEnum, ErrorManager.DetailEnum.NONE, pLevelOrder), pMsg,
                        LogTools.IdentifierAndId(identifier_Css, pIdACss),
                        DtFunc.DateTimeToStringDateISO(dtBusiness));

                processLog.SQLWriteDetail(); //PL 20160524 Add
            }
        }

        #region GetQueryFlows
        /// <summary>
        /// Point d'entrée principale pour la construction des requêtes (Montants / Books) par type de flux :
        /// - Daily CashFlows - VMG, PRM, SCU, OPP (Fees)
        /// - Last  Cash Balance 
        /// - Daily Collateral   
        /// - Daily Deposit
        /// - Daily OtherFlows - LOV, UMG, RMG (côté clôturante), FDA, MGR(OTC)
        /// - Daily Payments
        /// - Payments in settlement date
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDataParameters">Paramètres des requêtes</param>
        /// <returns>Requête</returns>
        /// PM 20150323 [POC] Add FlowTypeEnum.TradeFlows
        //private StrBuilder GetQueryFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType, DataParameters pDataParameters)
        //{
        //    //PL 20160524 REM return GetQueryFlows(pCS, pIsClearerActorExist, pFlowType, pDataParameters, null);
        //    return GetQueryFlows(pCS, pIsClearerActorExist, pFlowType, pDataParameters, CashFlowsStep.NA);
        //}
        //PL 20160524 REM private StrBuilder GetQueryFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType, DataParameters pDataParameters, Nullable<int> pStep)
        private StrBuilder GetQueryFlows(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType, DataParameters pDataParameters, CashFlowsStep pStep)
        {
            StrBuilder sqlQuery = new StrBuilder();
            switch (pFlowType)
            {
                // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                case FlowTypeEnum.AllocNotFungibleFlows:
                    sqlQuery = GetQueryFlows_AllocNotFungibleFlows(pCS, pIsClearerActorExist, pStep);
                    break;
                case FlowTypeEnum.CashFlows:
                    sqlQuery = GetQueryFlows_CashFlows(pCS, pIsClearerActorExist, pStep);
                    break;
                case FlowTypeEnum.Collateral:
                    sqlQuery = GetQueryFlows_Collateral(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.Deposit:
                    sqlQuery = GetQueryFlows_Deposit(pCS, pIsClearerActorExist, pDataParameters);
                    break;
                case FlowTypeEnum.LastCashBalance:
                    sqlQuery = GetQueryFlows_LastCashBalance(pCS);
                    break;
                case FlowTypeEnum.OtherFlows:
                    sqlQuery = GetQueryFlows_OtherFlows(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.Payment:
                    sqlQuery = GetQueryFlows_Payment(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.SettlementPayment:
                    sqlQuery = GetQueryFlows_SettlementPayment(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.TradeFlows:
                    sqlQuery = GetQueryFlows_TradeFlows(pCS, pStep);
                    break;
            }
            return sqlQuery;
        }
        #endregion GetQueryFlows
        #region GetQueryTrades
        /// <summary>
        /// Point d'entrée principale pour la construction des requêtes (Books / Trades) par type de flux :
        /// - Daily CashFlows - VMG, PRM, SCU, OPP (Fees)
        /// - Daily OtherFlows - LOV, UMG, RMG (côté clôturante)
        /// - Daily Payments
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pDataParameters">Paramètres des requêtes</param>
        /// <param name="pStep">1 = OPP et TAXES,  2 = PRM, VMG et SCU</param> 
        /// <returns>Requête</returns>
        //PM 20150327 [POC] Add FlowTypeEnum.TradeFlows
        private StrBuilder GetQueryTrades(string pCS, bool pIsClearerActorExist, FlowTypeEnum pFlowType, DataParameters pDataParameters, CashFlowsStep pStep)
        {
            StrBuilder sqlQuery = new StrBuilder();
            switch (pFlowType)
            {
                // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
                case FlowTypeEnum.AllocNotFungibleFlows:
                    sqlQuery = GetQueryTrades_AllocNotFungibleFlows(pCS, pIsClearerActorExist, pStep);
                    break;
                case FlowTypeEnum.CashFlows:
                    sqlQuery = GetQueryTrades_CashFlows(pCS, pIsClearerActorExist, pStep);
                    break;
                case FlowTypeEnum.OtherFlows:
                    sqlQuery = GetQueryTrades_OtherFlows(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.Payment:
                    sqlQuery = GetQueryTrades_Payment(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.SettlementPayment:
                    sqlQuery = GetQueryTrades_SettlemenPayment(pCS, pIsClearerActorExist);
                    break;
                case FlowTypeEnum.TradeFlows:
                    sqlQuery = GetQueryTrades_TradeFlows(pCS, pStep);
                    break;
            }
            return sqlQuery;
        }
        #endregion GetQueryTrades

        #region depreacated_GetSubQueryTrade
        ///// <summary>
        ///// Construction de la requête trades candidats, types de flux concernés : CashFlows, OtherFlows, Payment
        ///// </summary>
        ///// <typeparam name="T">voir les fonctions appelantes</typeparam>
        ///// <param name="pCS">Chaine de connexion</param>
        ///// <param name="pFlowType">Type de flux</param>
        ///// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        ///// <returns>Requête Trade</returns>
        ///// EG 20140218 [19575][19666] REFACTORING QUERY
        ///// FI 20161021 [22152] 
        //private string depreacated_GetSubQueryTrade<T>(string pCS, FlowTypeEnum pFlowType, T pConditionSwitch)
        //{
        //    //PL 20160607
        //    string actorSuffix = string.Empty;
        //    string sqlQuery = string.Empty;

        //    switch (pFlowType)
        //    {
        //        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
        //        case FlowTypeEnum.AllocNotFungibleFlows:
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), IDA_OWNER" + actorSuffix + " as IDA, IDB_" + actorSuffix + " as IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER";
        //            sqlQuery += "," + SQLColumnStatus(cs, "ti", "STATUS") + Cst.CrLf;
        //            sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch, false);
        //            sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
        //            sqlQuery += SQLCst.GROUPBY.Trim() + " IDA_OWNER" + actorSuffix + ", IDB_" + actorSuffix + ", tr.IDT, tr.IDENTIFIER";
        //            sqlQuery += "," + SQLColumnStatus(cs, "ti", string.Empty);
        //            break;
        //        case FlowTypeEnum.CashFlows:
        //        case FlowTypeEnum.OtherFlows:
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER";
        //            sqlQuery += "," + SQLColumnStatus(cs, "tr", "STATUS") + Cst.CrLf;
        //            sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch, false);
        //            sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
        //            sqlQuery += SQLCst.GROUPBY.Trim() + " tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", tr.IDT, tr.IDENTIFIER";
        //            sqlQuery += "," + SQLColumnStatus(cs, "tr", string.Empty);
        //            break;
        //        case FlowTypeEnum.TradeFlows:
        //            sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER";
        //            sqlQuery += "," + SQLColumnStatus(cs, "ti", "STATUS") + Cst.CrLf;
        //            sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch, false);
        //            sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
        //            sqlQuery += SQLCst.GROUPBY.Trim() + " b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER";
        //            sqlQuery += "," + SQLColumnStatus(cs, "ti", string.Empty);
        //            break;
        //        default:
        //            sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER";
        //            sqlQuery += "," + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid) + Cst.CrLf;
        //            sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch, false);
        //            sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
        //            sqlQuery += SQLCst.GROUPBY.Trim() + " b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER";
        //            break;
        //    }
        //    return sqlQuery;
        //}
        #endregion depreacated_GetSubQueryTrade
        
        #region GetSubQueryTrade
        /// <summary>
        /// Construction de la requête trades candidats, types de flux concernés : CashFlows, OtherFlows, Payment
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pConditionSwitch">Informations des conditions de construction de la requête</param>
        /// <returns>Requête Trade</returns>
        /// EG 20140218 [19575][19666] REFACTORING QUERY
        /// FI 20161021 [22152] 
        /// PM 20170213 [21916] Refactoring pConditionSwitch et ajout AllocNotFungibleFlows pour Commodity Spot
        /// FI 20170316 [22950] Modify
        private string GetSubQueryTrade(string pCS, FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string actorSuffix = pConditionSwitch.DealerClearerSuffixe;
            string sqlQuery = string.Empty;
            
            // FI 20170316 [22950] ajout de tr.DTBUSINESS dans tous les cas
            switch (pFlowType)
            {
                case FlowTypeEnum.AllocNotFungibleFlows:
                    sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), b" + actorSuffix.ToLower() + ".IDA as IDA, IDB_" + actorSuffix + " as IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS";
                    sqlQuery += "," + SQLColumnStatus(cs, "ti", "STATUS") + Cst.CrLf;
                    sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += SQLCst.GROUPBY.Trim() + " b" + actorSuffix.ToLower() + ".IDA, IDB_" + actorSuffix + ", tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS";
                    if (m_CBHierarchy.cssCustodianEODValid.Count > 0) // FI 20170316 [22950] add test pour SQL Sqlserver (l'usage de d'une constante (ici 'unvalid') n'est pas toléré)
                        sqlQuery += "," + SQLColumnStatus(cs, "ti", string.Empty);
                    break;
                case FlowTypeEnum.CashFlows:
                case FlowTypeEnum.OtherFlows:
                    sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS";
                    sqlQuery += "," + SQLColumnStatus(cs, "tr", "STATUS") + Cst.CrLf;
                    sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += SQLCst.GROUPBY.Trim() + " tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS";
                    if (m_CBHierarchy.cssCustodianEODValid.Count > 0) // FI 20170316 [22950] add test pour SQL Sqlserver (l'usage de d'une constante (ici 'unvalid') n'est pas toléré)
                        sqlQuery += "," + SQLColumnStatus(cs, "tr", string.Empty);
                    break;
                case FlowTypeEnum.TradeFlows:
                    sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS";
                    sqlQuery += "," + SQLColumnStatus(cs, "ti", "STATUS") + Cst.CrLf;
                    sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += SQLCst.GROUPBY.Trim() + " b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER";
                    if (m_CBHierarchy.cssCustodianEODValid.Count > 0) // FI 20170316 [22950] add test pour SQL Sqlserver (l'usage de d'une constante (ici 'unvalid') n'est pas toléré)
                        sqlQuery += "," + SQLColumnStatus(cs, "ti", string.Empty);
                    break;
                default:
                    sqlQuery = SQLCst.SELECT + "/*+ ordered */ count(tr.IDT), b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS";
                    sqlQuery += "," + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid) + Cst.CrLf;
                    sqlQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
                    sqlQuery += SQLCst.GROUPBY.Trim() + " b.IDA, b.IDB, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS";
                    break;
            }
            return sqlQuery;
        }
        #endregion GetSubQueryTrade

        #region deprecated_GetSubQueryFlows
        //#region GetSubQueryFlows
        ///// <summary>
        ///// Construction d'une partie de requête : Select + From + Where + [GroupBy]
        ///// </summary>
        ///// <typeparam name="T">Switch de construction de requête</typeparam>
        ///// <param name="pCS">Chaine de connexion</param>
        ///// <param name="pFlowType">Type de flux</param>
        ///// <param name="pConditionSwitch">
        ///// Condition complémentaire pour construction des requête : 
        ///// --------------------------------------------------------
        ///// CashFlows         : Si frais : Pair(DEALER/CLEARER (string), Taxes (bool)) sinon : DEALER/CLEARER (string)
        ///// Collateral        : Pair(Payer/Receiver (PayerReceiverEnum), isCash (bool))
        ///// Deposit           : Pair(Payer/Receiver (PayerReceiverEnum), isFilterCSS (bool))
        ///// LastCashBalance   : null 
        ///// OtherFlows        : DEALER/CLEARER (string) 
        ///// Payment           : Payer/Receiver (PayerReceiverEnum)
        ///// SettlementPayment : Payer/Receiver (PayerReceiverEnum)
        ///// </param>
        ///// <returns>Partie de requête</returns>
        ///// PM 20170213 [21916] Refactoring pConditionSwitch
        //private string deprecated_GetSubQueryFlows<T>(string pCS, FlowTypeEnum pFlowType, T pConditionSwitch)
        //{
        //    string sqlSubQuery = GetSubQueryFlows_Column(pFlowType, pConditionSwitch);
        //    sqlSubQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch);
        //    sqlSubQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
        //    sqlSubQuery += GetSubQueryFlows_GroupBy(pFlowType, pConditionSwitch);
        //    return sqlSubQuery + Cst.CrLf;
        //}
        //#endregion GetSubQueryFlows
        //#region GetSubQueryFlows_Column
        ///// <summary>
        ///// Construction d'une partie de requête : Select
        ///// </summary>
        ///// <typeparam name="T">voir les fonctions appelantes</typeparam>
        ///// <param name="pFlowType">Type de flux</param>
        ///// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        ///// <returns>Partie de requête : Select</returns>
        ///// EG 20140218 [19575][19666] REFACTORING QUERY
        ///// PM 20140911 [20066][20185] Refactoring
        ///// PM 20150323 [POC] Add TradeFlows
        ///// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        ///// FI 20160530 [21885] Modify 
        ///// FI 20170208 [22151][22152] Modify
        ///// PM 20170213 [21916] Refactoring pConditionSwitch
        //private string deprecated_GetSubQueryFlows_Column<T>(FlowTypeEnum pFlowType, T pConditionSwitch)
        //{
        //    string actorSuffix = string.Empty;
        //    string sqlSelect = string.Empty;
        //    bool isFees = pConditionSwitch is Pair<string, bool>;
        //    switch (pFlowType)
        //    {
        //        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
        //        case FlowTypeEnum.AllocNotFungibleFlows:
        //            #region AllocNotFungibleFlows
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlSelect += SQLCst.SELECT + "/*+ ordered */ IDA_OWNER" + actorSuffix + " as IDA, IDB_" + actorSuffix + " as IDB, ev.DTVAL, ev.UNIT as IDC, p.IDENTIFIER as PRODUCTIDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET" + Cst.CrLf;
        //            if (isFees)
        //            {
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                sqlSelect += ", ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE";
        //                if (_conditionSwitch.Second)
        //                {
        //                    // Taxes
        //                    sqlSelect += ", ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE," + Cst.CrLf;
        //                }
        //                else
        //                {
        //                    // Frais sans les taxes
        //                    sqlSelect += ", null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE," + Cst.CrLf;
        //                }
        //                // Ajout SUM() de VALORISATION en fonction de Payer/Receiver car _conditionSwitch.First de type string
        //                sqlSelect += GetFlows_CommonAmount(_conditionSwitch.First) + Cst.CrLf;
        //            }
        //            else
        //            {
        //                sqlSelect += ", ev.EVENTTYPE as AMOUNTSUBTYPE, null as PAYMENTTYPE ";
        //                sqlSelect += ", null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE," + Cst.CrLf;
        //                sqlSelect += GetFlows_CommonAmount(pConditionSwitch) + Cst.CrLf;
        //            }
        //            sqlSelect += "," + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end as STATUS", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid) + Cst.CrLf;
        //            #endregion AllocNotFungibleFlows
        //            break;
        //        case FlowTypeEnum.CashFlows:
        //            #region CashFlows
        //            // PM 20150616 [21124] Add column DTVAL
        //            //PL 20160524 
        //            //sqlSelect += SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end as IDASSET, asset.ASSETCATEGORY, ev.DTVAL, ";
        //            //PL 20160607
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlSelect += SQLCst.SELECT + "/*+ ordered */ tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, ev.UNIT as IDC, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end as IDASSET, asset.ASSETCATEGORY, ev.DTVAL, ";

        //            if (isFees)
        //            {
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                // PM 20150709 [21103] Gestion SafeKeepingPayment
        //                //sqlSelect += "@EVENTCODE_OPP as AMOUNTSUBTYPE, ev.PAYMENTTYPE, " + Cst.CrLf;
        //                sqlSelect += "ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE, "; //PL 20160524
        //                if (_conditionSwitch.Second)
        //                {
        //                    // Taxes
        //                    sqlSelect += "ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE," + Cst.CrLf;

        //                }
        //                else
        //                {
        //                    // Frais sans les taxes
        //                    sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE," + Cst.CrLf;
        //                }
        //                // Ajout SUM() de VALORISATION en fonction de Payer/Receiver car _conditionSwitch.First de type string
        //                sqlSelect += GetFlows_CommonAmount(_conditionSwitch.First) + Cst.CrLf;
        //            }
        //            else
        //            {
        //                // Autres
        //                sqlSelect += "ev.EVENTTYPE as AMOUNTSUBTYPE, null as PAYMENTTYPE, ";
        //                sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE," + Cst.CrLf;
        //                sqlSelect += GetFlows_CommonAmount(pConditionSwitch) + Cst.CrLf;
        //            }
        //            // FI 20170208 [22151][22152] add column STATUS
        //            sqlSelect += SQLColumnStatus(cs, "tr", "STATUS");
        //            #endregion CashFlows
        //            break;
        //        case FlowTypeEnum.Collateral:
        //            // FI 20160530 [21885] Add columns IDPOSCOLLATERAL and IDPOSCOLLATERALVAL
        //            // FI 20170208 [22151][22152] add column STATUS
        //            #region Collateral
        //            {
        //                Pair<PayerReceiverEnum, bool> _conditionSwitch = pConditionSwitch as Pair<PayerReceiverEnum, bool>;
        //                sqlSelect = SQLCst.SELECT + "pc.IDPOSCOLLATERAL, pc.IDA_CSS, pc.ASSETCATEGORY, pc.IDASSET, pc.HAIRCUTFORCED, pc.DTBUSINESS, ";
        //                if (PayerReceiverEnum.Payer == _conditionSwitch.First)
        //                    sqlSelect += "pc.IDA_PAY as IDA, pc.IDB_PAY as IDB, (-1 * pcval_last.VALORISATION) as AMOUNT, ";
        //                else if (PayerReceiverEnum.Receiver == _conditionSwitch.First)
        //                    sqlSelect += "pc.IDA_REC as IDA, pc.IDB_REC as IDB, (pcval_last.VALORISATION) as AMOUNT, ";
        //                sqlSelect += "pcval_last.IDPOSCOLLATERALVAL, pcval_last.IDC, case when pcval_last.IDPOSCOLLATERALVAL is null then 0 else 1 end as ISVALORISED, ";
        //                sqlSelect += "pcval_last.QTY as QTYVAL";
        //                sqlSelect += "," + Cst.CrLf + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid);
        //            }
        //            #endregion Collateral
        //            break;
        //        case FlowTypeEnum.Deposit:
        //            #region Deposit
        //            {
        //                // FI 20170208 [22151][22152] condition en paramètre à la méthode GetFlows_CommonAmount
        //                Pair<PayerReceiverEnum, bool> condition = pConditionSwitch as Pair<PayerReceiverEnum, bool>;
        //                if (null == condition)
        //                    throw new InvalidOperationException("pConditionSwitch is not valid");
        //                // FI 20170208 [22151][22152] ti.I=
        //                sqlSelect = SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTSYS, ti.IDA_CSSCUSTODIAN as IDA_CSS, " + Cst.CrLf;
        //                sqlSelect += GetFlows_CommonAmount(condition) + Cst.CrLf;
        //            }
        //            #endregion Deposit
        //            break;
        //        case FlowTypeEnum.LastCashBalance:
        //            #region LastCashBalance
        //            // PM 20150616 [21124] Add column DTEVENT
        //            // FI 20170208 [22151][22152] add column STATUS
        //            sqlSelect = SQLCst.SELECT_DISTINCT + "tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTSYS, ec.DTEVENT, " + Cst.CrLf;
        //            sqlSelect += "                case when ev.IDA_PAY = @IDA_ENTITY then ev.IDA_REC else ev.IDA_PAY end as IDA, " + Cst.CrLf;
        //            sqlSelect += "                case when ev.IDA_PAY = @IDA_ENTITY then ev.IDB_REC else ev.IDB_PAY end as IDB, " + Cst.CrLf;
        //            sqlSelect += "                ev.UNIT as IDC, case when ev.IDA_PAY = @IDA_ENTITY then 1 else -1 end * ev.VALORISATION as AMOUNT, ev.EVENTTYPE as AMOUNTSUBTYPE";/* + Cst.CrLf;*/
        //            sqlSelect += "," + Cst.CrLf + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid);
        //            #endregion LastCashBalance
        //            break;
        //        case FlowTypeEnum.OtherFlows:
        //            #region OtherFlows
        //            //PM 20150616 [21124] Add column DTVAL
        //            //sqlSelect += SQLCst.SELECT + "/*+ ordered */ b.IDA, b.IDB, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, asset.ASSETCATEGORY, asset.CATEGORY, asset.FUTVALUATIONMETHOD, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end as IDASSET, tr.SIDE, ev.DTVAL, " + Cst.CrLf;
        //            //PL 20160524 
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlSelect += SQLCst.SELECT + "/*+ ordered */ tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, asset.ASSETCATEGORY, asset.CATEGORY, asset.FUTVALUATIONMETHOD, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end as IDASSET, tr.SIDE, ev.DTVAL, " + Cst.CrLf;
        //            sqlSelect += GetFlows_CommonAmount(pConditionSwitch) + Cst.CrLf;
        //            #endregion OtherFlows
        //            break;
        //        case FlowTypeEnum.Payment:
        //            #region Payment
        //            sqlSelect = SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTSYS, " + Cst.CrLf;
        //            sqlSelect += GetFlows_CommonAmount(pConditionSwitch) + Cst.CrLf;
        //            #endregion Payment
        //            break;
        //        case FlowTypeEnum.SettlementPayment:
        //            #region SettlementPayment
        //            // PM 20150616 [21124] Add column DTEVENT
        //            sqlSelect = SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTSYS, ec.DTEVENT, ";
        //            sqlSelect += "case when (ec.DTEVENT = @BUSINESSDATE) then 0 else 1 end as ISFORWARD, " + Cst.CrLf;
        //            sqlSelect += GetFlows_CommonAmount(pConditionSwitch) + Cst.CrLf;
        //            #endregion SettlementPayment
        //            break;
        //        case FlowTypeEnum.TradeFlows:
        //            #region TradeFlows
        //            //sqlSelect += SQLCst.SELECT + "/*+ ordered */ b.IDA, b.IDB, ev.DTEVENT, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, p.IDENTIFIER as PRODUCTIDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET," + Cst.CrLf;
        //            sqlSelect += SQLCst.SELECT + "/*+ ordered */ b.IDA, b.IDB, ev.DTVAL, ev.UNIT as IDC, p.IDENTIFIER as PRODUCTIDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET," + Cst.CrLf;
        //            sqlSelect += "       sum(case b.IDA when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end" + Cst.CrLf;
        //            sqlSelect += "                      when ev.IDA_REC then case when (ISREMOVED=0) then  1 else -1 end" + Cst.CrLf;
        //            sqlSelect += "                      end * ev.VALORISATION) as AMOUNT" + Cst.CrLf;

        //            if (isFees)
        //            {
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                sqlSelect += ", ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE";
        //                if (_conditionSwitch.Second)
        //                {
        //                    // Taxes
        //                    sqlSelect += ", ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE" + Cst.CrLf;
        //                }
        //                else
        //                {
        //                    // Frais sans les taxes
        //                    sqlSelect += ", null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE";/* +Cst.CrLf;*/
        //                }
        //            }
        //            else
        //            {
        //                sqlSelect += ", ev.EVENTTYPE as AMOUNTSUBTYPE, null as PAYMENTTYPE ";
        //                sqlSelect += ", null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE"; /*+ Cst.CrLf;*/
        //            }
        //            // FI 20170208 [22151][22152] Add column STATUS
        //            sqlSelect += "," + Cst.CrLf + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end as STATUS", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid) + Cst.CrLf;
        //            #endregion TradeFlows
        //            break;
        //    }
        //    return sqlSelect;
        //}
        //#endregion GetSubQueryFlows_Column
        //#region GetSubQueryFlows_From
        ///// <summary>
        ///// Construction d'une partie de requête : From / Join
        ///// </summary>
        ///// <typeparam name="T">voir les fonctions appelantes</typeparam>
        ///// <param name="pCS">Chaine de connexion</param>
        ///// <param name="pFlowType">Type de flux</param>
        ///// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        ///// <returns>Partie de requête : From / Join</returns>
        ///// EG 20140218 [19575][19666] REFACTORING QUERY
        ///// PM 20140903 [20066][20185] Gestion méthode UK (ETD & CFD)
        ///// PM 20140911 [20066][20185] Refactoring
        ///// PM 20150320 [POC BFL] Add TradeFlows
        ///// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        //private string deprecated_GetSubQueryFlows_From<T>(string pCS, FlowTypeEnum pFlowType, T pConditionSwitch)
        //{
        //    return GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch, true);
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="pCS"></param>
        ///// <param name="pFlowType"></param>
        ///// <param name="pConditionSwitch"></param>
        ///// <param name="pIsJoinAsset"></param>
        ///// <returns></returns>
        ///// FI 20161021 [22152] Modify
        ///// FI 20170208 [21916] Modify
        ///// FI 20170208 [22151][22152] Modify
        //private string deprecated_GetSubQueryFlows_From<T>(string pCS, FlowTypeEnum pFlowType, T pConditionSwitch, bool pIsJoinAsset)
        //{
        //    string sqlFrom = string.Empty;
        //    string padLeft = "             ";
        //    string actorSuffix = string.Empty;
        //    bool isFees = pConditionSwitch is Pair<string, bool>;

        //    switch (pFlowType)
        //    {
        //        // PM 20170213 [21916] Ajout AllocNotFungibleFlows pour Commodity Spot
        //        case FlowTypeEnum.AllocNotFungibleFlows:
        //            #region AllocNotFungibleFlows
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            if (isFees)
        //            {
        //                #region Frais - OPP
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                string _vmFee = "F";
        //                string _lblFee = "Fee";
        //                string _colFee = "/*+ ordered */ evfee.PAYMENTTYPE";
        //                string _joinFee = "inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)";
        //                if (_conditionSwitch.Second)
        //                {
        //                    _vmFee = "T";
        //                    _lblFee = "Tax";
        //                    _colFee = "/*+ ordered */ evfeep.PAYMENTTYPE";
        //                    _joinFee += Cst.CrLf + padLeft + "inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
        //                }
        //                sqlFrom += "from       (/* VM-Begin" + _vmFee + " */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT, ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  REMOVED Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "select " + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT, @BUSINESSDATE as DTVAL, 1 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  Unsettled " + _lblFee + "  |-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + "select " + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE) and (ecrec.EVENTCLASS = 'REC') and (ecrec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
        //                #endregion  Frais - OPP
        //            }
        //            else
        //            {
        //                #region  Not OPP flows
        //                sqlFrom += " from      (/* Begin not OPP */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  Flows |-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
        //                sqlFrom += "            where (ec.DTEVENT >= @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "              and (ev.EVENTCODE = 'LPP') and (ev.EVENTTYPE = 'GAM') and (ec.EVENTCLASS = 'VAL')" + Cst.CrLf;
        //                sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows  |-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
        //                sqlFrom += "            where (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "              and (ev.EVENTCODE = 'LPP') and (ev.EVENTTYPE = 'GAM') and ec.EVENTCLASS = 'VAL')" + Cst.CrLf;
        //                sqlFrom += " /* End  */) ev" + Cst.CrLf;
        //                #endregion  Not OPP flows
        //            }
        //            sqlFrom += "inner join dbo.TRADE tr on (tr.IDT = ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.TRADEINSTRUMENT ti on (ti.IDT = tr.IDT)" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.INSTRUMENT i on (i.IDI = ti.IDI)" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.GPRODUCT = 'COM') and (p.FUNGIBILITYMODE = 'NONE')" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.TRADESTSYS tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = 'REGULAR') and (tsys.IDSTBUSINESS  = 'ALLOC')" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.BOOK bd on (bd.IDB = ti.IDB_DEALER)" + Cst.CrLf;
        //            sqlFrom += "left outer join dbo.BOOK bc on (bc.IDB = ti.IDB_CLEARER)" + Cst.CrLf;
        //            sqlFrom += "inner join " + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = b.IDA)" + Cst.CrLf;
        //            #endregion AllocNotFungibleFlows
        //            break;
        //        case FlowTypeEnum.CashFlows:
        //            #region CashFlows
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            //PL 20160524 
        //            //sqlFrom = SQLCst.FROM_DBO.Trim() + Cst.OTCml_TBL.VW_TRADE_FUNGIBLE + " tr" + Cst.CrLf;
        //            //sqlFrom += SQLCst.INNERJOIN_DBO.Trim() + Cst.OTCml_TBL.BOOK + " b on (b.IDB = tr.IDB_" + actorSuffix + ")" + Cst.CrLf;//PL 20160524 GLOP USE NEW VW_TRADE_FUNGIBLE with IDA_OWNERDEALER/CLEARER

        //            //if (pIsJoinAsset)
        //            //{
        //            //    sqlFrom += "inner join ( select da.IDDC, asset.IDASSET, 'ExchangeTradedContract' as ASSETCATEGORY" + Cst.CrLf;
        //            //    sqlFrom += "             from dbo.ASSET_ETD asset" + Cst.CrLf;
        //            //    sqlFrom += "             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
        //            //    sqlFrom += "             union all -- --------------------------------------------------------------------------------" + Cst.CrLf;
        //            //    sqlFrom += "             select 0 as IDDC, asset.IDASSET, asset.ASSETCATEGORY" + Cst.CrLf;
        //            //    //PL 20160524 GLOP USE VW_ASSET_WOETD
        //            //    //sqlFrom += "             from dbo.VW_ASSET asset where (asset.ASSETCATEGORY!='ExchangeTradedContract')" + Cst.CrLf; 
        //            //    sqlFrom += "             from dbo.VW_ASSET_WOETD asset" + Cst.CrLf; 
        //            //    sqlFrom += "           ) asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
        //            //}
        //            if (isFees)
        //            {
        //                #region Frais - OPP
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                string _vmFee = "F";
        //                string _lblFee = "Fee";
        //                string _colFee = "/*+ ordered */ evfee.PAYMENTTYPE";
        //                string _joinFee = "inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)";
        //                if (_conditionSwitch.Second)
        //                {
        //                    _vmFee = "T";
        //                    _lblFee = "Tax";
        //                    _colFee = "/*+ ordered */ evfeep.PAYMENTTYPE";
        //                    _joinFee += Cst.CrLf + padLeft + "inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
        //                }

        //                //sqlFrom += "inner join (/* VM-Begin */" + Cst.CrLf;
        //                sqlFrom += "from       (/* VM-Begin" + _vmFee + " */" + Cst.CrLf;
        //                // PM 20150311 [POC] Ajout condition sur ISPAYMENT = true
        //                // PM 20150616 [21124] Ajout DTVAL en DTEVENT = BUSINESSDATE car pas Unsettled
        //                // PM 20150709 [21103] Ajout ev.EVENTCODE
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('" + EventCodeEnum.OPP.ToString() + "', '" + EventCodeEnum.SKP.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                // PM 20150709 [21103] Add SafeKeepingPayment : EVENTCODE_SKP
        //                //sqlFrom += "where (ev.EVENTCODE = '" + EventCodeEnum.OPP.ToString() + "') and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

        //                sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

        //                // PM 20150311 [POC] Ajout condition sur ISPAYMENT = true
        //                // PM 20150616 [21124] Ajout DTVAL en BUSINESSDATE car Removed != Unsettled
        //                // PM 20150709 [21103] Ajout ev.EVENTCODE
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  REMOVED Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTCODE in ('" + EventCodeEnum.OPP.ToString() + "', '" + EventCodeEnum.SKP.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                // PM 20150709 [21103] Add SafeKeepingPayment : EVENTCODE_SKP
        //                //sqlFrom += "where (ev.EVENTCODE = '" + EventCodeEnum.OPP.ToString() + "') and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ecrmv.EVENTCLASS = '" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                // PM 20150616 [21124] Ajout query pour Unsettled
        //                sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                // PM 20150709 [21103] Ajout ev.EVENTCODE
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  Unsettled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       /* @BUSINESSDATE as DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE) and (ecrec.EVENTCLASS = '" + EventClassEnum.REC.ToString() + "') and (ecrec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('" + EventCodeEnum.OPP.ToString() + "', '" + EventCodeEnum.SKP.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                // PM 20150709 [21103] Add SafeKeepingPayment : EVENTCODE_SKP
        //                //sqlFrom += "where (ev.EVENTCODE = '" + EventCodeEnum.OPP.ToString() + "') and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;

        //                //sqlFrom += "/* VM-End */) ev on (ev.IDT = tr.IDT)" + Cst.CrLf;
        //                sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
        //                #endregion Frais - OPP
        //            }
        //            else
        //            {
        //                #region Cash-flows - PRM/VMG/SCU
        //                // PM 20150616 [21124] Ajout colonne DTVAL en BUSINESSDATE
        //                //sqlFrom += "inner join (/* VM-Begin */" + Cst.CrLf;
        //                sqlFrom += "from       (/* VM-Begin */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  Variation Margin/Cash Settlement  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //                sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTTYPE in ('" + EfsML.Enum.EventTypeEnum.VMG.ToString() + "','" + EfsML.Enum.EventTypeEnum.SCU.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += "            where (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  Option Premium  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //                sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS='" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTTYPE = '" + EfsML.Enum.EventTypeEnum.PRM.ToString() + "') and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += "            where (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  REMOVED Variation Margin/Cash Settlement  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //                sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTTYPE in ('" + EfsML.Enum.EventTypeEnum.VMG.ToString() + "', '" + EfsML.Enum.EventTypeEnum.SCU.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += "            where (ecrmv.EVENTCLASS='" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  REMOVED Option Premium  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //                sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS='" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTTYPE = '" + EfsML.Enum.EventTypeEnum.PRM.ToString() + "') and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += "            where (ecrmv.EVENTCLASS='" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                //sqlFrom += "/* VM-End */) ev on (ev.IDT = tr.IDT)" + Cst.CrLf;
        //                sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
        //                #endregion Cash-flows - PRM/VMG/SCU
        //            }

        //            sqlFrom += SQLCst.INNERJOIN_DBO.Trim() + Cst.OTCml_TBL.VW_TRADE_FUNGIBLE + " tr on (tr.IDT=ev.IDT)";
                    
        //            // FI 20161021 [22152] Add Restrict 
        //            // FI 20170208 [22151][22152] suppression de la restriction sur les chambre traitée avec succès
        //            // sqlFrom += m_CBHierarchy.SQLRestrictCssCustodianValid(pCS, "tr") + Cst.CrLf;

        //            if (pIsJoinAsset)
        //            {
        //                sqlFrom += "inner join ( select da.IDDC, asset.IDASSET, 'ExchangeTradedContract' as ASSETCATEGORY" + Cst.CrLf;
        //                sqlFrom += "             from dbo.ASSET_ETD asset" + Cst.CrLf;
        //                sqlFrom += "             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
        //                sqlFrom += "             union all -- --------------------------------------------------------------------------------" + Cst.CrLf;
        //                sqlFrom += "             select 0 as IDDC, asset.IDASSET, asset.ASSETCATEGORY" + Cst.CrLf;
        //                //PL 20160524 GLOP USE VW_ASSET_WOETD
        //                //sqlFrom += "             from dbo.VW_ASSET asset where (asset.ASSETCATEGORY!='ExchangeTradedContract')" + Cst.CrLf; 
        //                sqlFrom += "             from dbo.VW_ASSET_WOETD asset" + Cst.CrLf;
        //                sqlFrom += "           ) asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
        //            }

        //            //sqlFrom += "inner join (" + GetQuery_CBACTORv1_DEPRECATED(pCS, actorSuffix) + Cst.CrLf; 
        //            //sqlFrom += "           ) cb on (cb.IDA = b.IDA)" + Cst.CrLf;
        //            //sqlFrom += "inner join " + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = b.IDA)" + Cst.CrLf;
        //            sqlFrom += "inner join " + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = tr.IDA_OWNER" + actorSuffix + ")" + Cst.CrLf;
        //            #endregion CashFlows
        //            break;
        //        case FlowTypeEnum.Collateral:
        //            #region Collateral
        //            {
        //                Pair<PayerReceiverEnum, bool> _conditionSwitch = pConditionSwitch as Pair<PayerReceiverEnum, bool>;
        //                sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_COLLATERALPOS + " pc" + Cst.CrLf;
        //                if (_conditionSwitch.Second)
        //                {
        //                    DbSvrType serverType = DataHelper.GetDbSvrType(pCS);

        //                    // Chargement de la dernière valorisation connue pour les Collateral de type Cash
        //                    if (DbSvrType.dbSQL == serverType)
        //                    {
        //                        sqlFrom += SQLCst.X_LEFT + "(" + Cst.CrLf;
        //                        sqlFrom += SQLCst.SELECT + "pc.IDPOSCOLLATERAL," + Cst.CrLf;
        //                        sqlFrom += "(" + Cst.CrLf;
        //                        sqlFrom += SQLCst.SELECT_TOP + "1 pcval.IDPOSCOLLATERALVAL" + Cst.CrLf;
        //                        sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval" + Cst.CrLf;
        //                        sqlFrom += SQLCst.WHERE + "(pcval.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "') and (pcval.DTBUSINESS <= @BUSINESSDATE)" + Cst.CrLf;
        //                        sqlFrom += SQLCst.ORDERBY + "pcval.DTBUSINESS" + SQLCst.DESC + Cst.CrLf;
        //                        sqlFrom += ") as IDPOSCOLLATERALVAL" + Cst.CrLf;
        //                        sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_COLLATERALPOS + " pc" + Cst.CrLf;
        //                        sqlFrom += ") pcval" + SQLCst.ON + "(pcval.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL)" + Cst.CrLf;
        //                        sqlFrom += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval_last on (pcval_last.IDPOSCOLLATERALVAL = pcval.IDPOSCOLLATERALVAL)" + Cst.CrLf;
        //                    }
        //                    else if (DbSvrType.dbORA == serverType)
        //                    {
        //                        sqlFrom += SQLCst.X_LEFT + "(" + Cst.CrLf;
        //                        sqlFrom += SQLCst.SELECT + "max(pcval.DTBUSINESS) as LAST_DTBUSINESS, pcval.IDPOSCOLLATERAL" + Cst.CrLf;
        //                        sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval" + Cst.CrLf;
        //                        sqlFrom += SQLCst.WHERE + "(pcval.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "') and (pcval.DTBUSINESS <= @BUSINESSDATE)" + Cst.CrLf;
        //                        sqlFrom += SQLCst.GROUPBY + "pcval.IDPOSCOLLATERAL" + Cst.CrLf;
        //                        sqlFrom += ") pcval_last1" + SQLCst.ON + "(pcval_last1.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL)" + Cst.CrLf;
        //                        sqlFrom += SQLCst.X_LEFT + "(" + Cst.CrLf;
        //                        sqlFrom += SQLCst.SELECT + "pcval.DTBUSINESS, pcval.IDPOSCOLLATERAL, pcval.IDPOSCOLLATERALVAL, pcval.IDC, pcval.QTY, pcval.VALORISATION" + Cst.CrLf;
        //                        sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval" + Cst.CrLf;
        //                        sqlFrom += SQLCst.WHERE + "(pcval.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "') and (pcval.DTBUSINESS <= @BUSINESSDATE)" + Cst.CrLf;
        //                        sqlFrom += ") pcval_last" + SQLCst.ON + "(pcval_last.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval_last.DTBUSINESS = pcval_last1.LAST_DTBUSINESS)" + Cst.CrLf;
        //                    }
        //                }
        //                else
        //                {
        //                    sqlFrom += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval_last on (pcval_last.IDPOSCOLLATERAL=pc.IDPOSCOLLATERAL) and";
        //                    sqlFrom += "(pcval_last.DTBUSINESS=@BUSINESSDATE)" + Cst.CrLf;
        //                }
        //            }
        //            #endregion Collateral
        //            break;
        //        case FlowTypeEnum.Deposit:
        //            #region Deposit
        //            //PM 20140411 [19846][19841] Ajout restriction sur IDSTENVIRONMENT = REGULAR et EVENTCODE = LPC
        //            sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
        //            sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly) + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "') and (tsys.IDSTENVIRONMENT = '" + Cst.StatusEnvironment.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = @EVENTTYPE_MGR) and (ev.EVENTCODE = @EVENTCODE_LPC)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK + " b on (b.IDB = ev.IDB_" + GetPayerReceiverTag(pConditionSwitch) + ")" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @BUSINESSDATE) and (ec.EVENTCLASS = '" + EventClassEnum.REC.ToString() + "')" + Cst.CrLf;
        //            // FI 20170208 [22151][22152] mise en commenatire et utilisation de TRADEINSTRUMENT
        //            //sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADEACTOR + " ta on (ta.IDT = tr.IDT) and (ta.IDROLEACTOR = @IDROLEACTOR_CSS)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADEINSTRUMENT + " ti on ti.IDT=tr.IDT";

        //            // FI 20161021 [22152] Add condition SQL
        //            Pair<PayerReceiverEnum, bool> condition = pConditionSwitch as Pair<PayerReceiverEnum, bool>;
        //            if (null == condition)
        //                throw new InvalidOperationException("pConditionSwitch is not valid");
        //            Boolean isSpecificCss = condition.Second;
        //            if (isSpecificCss)
        //            {
        //                // FI 20170208 [22151][22152] colonne ti.IDA_CSSCUSTODIAN 
        //                // Il s'agit de lire un deposit à une date antérieure
        //                //sqlFrom += " and (ta.IDA=@IDA_CSS)" + Cst.CrLf;
        //                sqlFrom += " and (ti.IDA_CSSCUSTODIAN=@IDA_CSS)" + Cst.CrLf;
        //            }

        //            // FI 20170208 [22151][22152] suppression de la restriction sur les chambre traitée avec succès
        //            /*
        //            else
        //            {
        //                // FI 20161021 [22152] Restriction sur les css/chambre valides (c-a-d dont les traitements EOD sont en succès ou warning)  
        //                sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADEINSTRUMENT + " ti on ti.IDT=tr.IDT";
        //                sqlFrom += m_CBHierarchy.SQLRestrictCssCustodianValid(pCS, "ti") + Cst.CrLf;
        //            }*/
        //            #endregion Deposit
        //            break;
        //        case FlowTypeEnum.LastCashBalance:
        //            #region LastCashBalance
        //            // PM 20140930 [20066][20185] Ajout Inner Join avec événement parent pour ne pas prendre le stream en contrevaleur
        //            sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
        //            sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly);
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT)";
        //            sqlFrom += " and (ev.EVENTTYPE in (@EVENTTYPE_CSB, @EVENTTYPE_CSA, @EVENTTYPE_CSU, @EVENTTYPE_CLA, @EVENTTYPE_CLU, @EVENTTYPE_UMR, @EVENTTYPE_MGR))" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_parent on (ev_parent.IDE = ev.IDE_EVENT) and (ev_parent.EVENTCODE = @EVENTCODE_CBS)";
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT=@BUSINESSDATE_PREV) and (ec.EVENTCLASS = '" + EventClassEnum.REC.ToString() + "')" + Cst.CrLf;
        //            #endregion LastCashBalance
        //            break;
        //        case FlowTypeEnum.OtherFlows:
        //            #region OtherFlows
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            //sqlFrom = " from dbo.VW_TRADE_FUNGIBLE tr" + Cst.CrLf;
        //            //sqlFrom += "inner join dbo.BOOK b on (b.IDB = tr.IDB_" + actorSuffix + ")" + Cst.CrLf; //PL 20160524 GLOP USE NEW VW_TRADE_FUNGIBLE with IDA_OWNERDEALER/CLEARER
        //            //if (pIsJoinAsset)
        //            //{
        //            //    sqlFrom += "inner join ( select dc.CATEGORY, dc.FUTVALUATIONMETHOD, dc.IDDC, asset.IDASSET, 'ExchangeTradedContract' as ASSETCATEGORY" + Cst.CrLf;
        //            //    sqlFrom += "             from dbo.ASSET_ETD asset" + Cst.CrLf;
        //            //    sqlFrom += "             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
        //            //    sqlFrom += "             inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)" + Cst.CrLf;
        //            //    sqlFrom += "             union all -- --------------------------------------------------------------------------------" + Cst.CrLf;
        //            //    sqlFrom += "             select null as CATEGORY, null as FUTVALUATIONMETHOD, 0 as IDDC, asset.IDASSET, asset.ASSETCATEGORY" + Cst.CrLf;
        //            //    //PL 20160524 GLOP USE VW_ASSET_WOETD
        //            //    //sqlFrom += "             from dbo.VW_ASSET asset where asset.ASSETCATEGORY != 'ExchangeTradedContract'" + Cst.CrLf;
        //            //    sqlFrom += "             from dbo.VW_ASSET_WOETD asset" + Cst.CrLf; 
        //            //    sqlFrom += "           ) asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
        //            //}
        //            //sqlFrom += "inner join (/* VM-Begin */" + Cst.CrLf;
        //            sqlFrom += "from       (/* VM-Begin */" + Cst.CrLf;
        //            // PM 20150616 [21124] Ajout colonne DTVAL
        //            // PM 20150616 [21124] Ajout MKV et nouvelle gestion des UMG et RMG
        //            // PM 20151208 [21317] Ajout INT/INT sur la famille de product DSE.
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  Flows  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','MGR')) and (ev.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            where (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','MGR')) and (ev.IDSTACTIVATION='" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            where (ecrmv.EVENTCLASS='" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  Flows RMG (except for equitySecurityTransaction)  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.EVENTTYPE='RMG') and (ev.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and ((p.GPRODUCT != 'SEC') or (p.FAMILY != 'ESE'))" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTPOSACTIONDET ev_pad on (ev_pad.IDE=ev.IDE)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.POSACTIONDET pad on (pad.IDPADET=ev_pad.IDPADET) and (pad.IDT_CLOSING=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            where (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows RMG (except for equitySecurityTransaction)  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.EVENTTYPE='RMG') and (ev.IDSTACTIVATION='" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and ((p.GPRODUCT != 'SEC') or (p.FAMILY != 'ESE'))" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTPOSACTIONDET ev_pad on (ev_pad.IDE=ev.IDE)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.POSACTIONDET pad on (pad.IDPADET=ev_pad.IDPADET) and (pad.IDT_CLOSING=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            where (ecrmv.EVENTCLASS='" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  Unsettled GAM  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* @BUSINESSDATE as DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENT ev" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTCLASS ec_rec on (ec_rec.IDE = ev.IDE) and (ec_rec.EVENTCLASS = '" + EventClassEnum.REC.ToString() + "') and (ec_rec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            where (ev.EVENTTYPE = '" + EfsML.Enum.EventTypeEnum.GAM.ToString() + "') and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  Flows UMG et MKV  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* @BUSINESSDATE as DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            where (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "              and (" + Cst.CrLf;
        //            sqlFrom += "                    ((ti.DTSETTLT is null) and (ev.EVENTTYPE='" + EfsML.Enum.EventTypeEnum.UMG.ToString() + "'))" + Cst.CrLf;
        //            sqlFrom += "                    or" + Cst.CrLf;
        //            sqlFrom += "                    (" + Cst.CrLf;
        //            sqlFrom += "                      (ti.DTSETTLT is not null)" + Cst.CrLf;
        //            sqlFrom += "                      and" + Cst.CrLf;
        //            sqlFrom += "                      (" + Cst.CrLf;
        //            sqlFrom += "                        ((ev.EVENTTYPE='" + EfsML.Enum.EventTypeEnum.MKV.ToString() + "') and (ec.DTEVENT >= ti.DTSETTLT))" + Cst.CrLf;
        //            sqlFrom += "                        or" + Cst.CrLf;
        //            sqlFrom += "                        ((ev.EVENTTYPE='" + EfsML.Enum.EventTypeEnum.UMG.ToString() + "') and (ec.DTEVENT < ti.DTSETTLT))" + Cst.CrLf;
        //            sqlFrom += "                      )" + Cst.CrLf;
        //            sqlFrom += "                    )" + Cst.CrLf;
        //            sqlFrom += "                  )" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  Flows INT/INT (for DSE)  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.EVENTCODE='INT')and (ev.EVENTTYPE='INT') and (ev.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.GPRODUCT='SEC') and (p.FAMILY='DSE')" + Cst.CrLf;
        //            sqlFrom += "            where (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            union all" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows INT/INT (for DSE)  |+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
        //            sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
        //            sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //            sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.EVENTCLASS='" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.EVENTCODE='INT')and (ev.EVENTTYPE='INT') and (ev.IDSTACTIVATION='" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
        //            sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.GPRODUCT='SEC') and (p.FAMILY='DSE')" + Cst.CrLf;
        //            sqlFrom += "            where (ecrmv.EVENTCLASS='" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //            //sqlFrom += "/* VM-End */) ev on (ev.IDT = tr.IDT)" + Cst.CrLf;
        //            sqlFrom += "/* VM-End */) ev" + Cst.CrLf;

        //            sqlFrom += " inner join dbo.VW_TRADE_FUNGIBLE tr on (tr.IDT=ev.IDT)" + Cst.CrLf;
        //            // FI 20161021 [22152] Add Restrict 
        //            // FI 20170208 [22151][22152] suppression de la restriction sur les chambre traitée avec succès
        //            //sqlFrom += m_CBHierarchy.SQLRestrictCssCustodianValid(pCS, "tr") + Cst.CrLf;

        //            //sqlFrom += "inner join dbo.BOOK b on (b.IDB = tr.IDB_" + actorSuffix + ")" + Cst.CrLf; //PL 20160524 GLOP USE NEW VW_TRADE_FUNGIBLE with IDA_OWNERDEALER/CLEARER
        //            if (pIsJoinAsset)
        //            {
        //                sqlFrom += "inner join ( select dc.CATEGORY, dc.FUTVALUATIONMETHOD, dc.IDDC, asset.IDASSET, 'ExchangeTradedContract' as ASSETCATEGORY" + Cst.CrLf;
        //                sqlFrom += "             from dbo.ASSET_ETD asset" + Cst.CrLf;
        //                sqlFrom += "             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
        //                sqlFrom += "             inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)" + Cst.CrLf;
        //                sqlFrom += "             union all -- --------------------------------------------------------------------------------" + Cst.CrLf;
        //                sqlFrom += "             select null as CATEGORY, null as FUTVALUATIONMETHOD, 0 as IDDC, asset.IDASSET, asset.ASSETCATEGORY" + Cst.CrLf;
        //                //PL 20160524 GLOP USE VW_ASSET_WOETD
        //                //sqlFrom += "             from dbo.VW_ASSET asset where asset.ASSETCATEGORY != 'ExchangeTradedContract'" + Cst.CrLf;
        //                sqlFrom += "             from dbo.VW_ASSET_WOETD asset" + Cst.CrLf;
        //                sqlFrom += "           ) asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
        //            }

        //            //sqlFrom += "inner join (" + Cst.CrLf + GetQuery_CBACTORv1_DEPRECATED(pCS, actorSuffix) + Cst.CrLf + ") cb on (cb.IDA = b.IDA)" + Cst.CrLf;
        //            //sqlFrom += "inner join " + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = b.IDA)" + Cst.CrLf;
        //            sqlFrom += "inner join " + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = tr.IDA_OWNER" + actorSuffix + ")" + Cst.CrLf;

        //            //PL 20160524 sqlFrom += "left join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ev.IDE)" + Cst.CrLf;
        //            //PL 20160524 sqlFrom += "left join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and (pad.IDT_CLOSING = ev.IDT)" + Cst.CrLf;
        //            #endregion OtherFlows
        //            break;
        //        case FlowTypeEnum.Payment:
        //            #region Payment
        //            sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
        //            sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly);
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            // PM 20150205 [] Prendre tous les CashPayment et pas uniquement ceux ayant pour PaymentType = Cash (EventType = CSH)
        //            //sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = @EVENTTYPE_CSH)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = @EVENTCODE_STA)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK + " b on (b.IDB = ev.IDB_" + GetPayerReceiverTag(pConditionSwitch) + ")" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT=@BUSINESSDATE) and (ec.EVENTCLASS = '" + EventClassEnum.REC.ToString() + "')" + Cst.CrLf;
        //            #endregion Payment
        //            break;
        //        case FlowTypeEnum.SettlementPayment:
        //            #region SettlementPayment
        //            sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
        //            sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly);
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //            // PM 20150205 [] Prendre tous les CashPayment et pas uniquement ceux ayant pour PaymentType = Cash (EventType = CSH)
        //            //sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = @EVENTTYPE_CSH)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = @EVENTCODE_STA)" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK + " b on (b.IDB = ev.IDB_" + GetPayerReceiverTag(pConditionSwitch) + ")" + Cst.CrLf;
        //            sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT>=@BUSINESSDATE) and (ec.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "')" + Cst.CrLf;
        //            #endregion SettlementPayment
        //            break;
        //        case FlowTypeEnum.TradeFlows:
        //            #region TradeFlows
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            if (isFees)
        //            {
        //                #region Frais - OPP
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                string _vmFee = "F";
        //                string _lblFee = "Fee";
        //                string _colFee = "/*+ ordered */ evfee.PAYMENTTYPE";
        //                string _joinFee = "inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)";
        //                if (_conditionSwitch.Second)
        //                {
        //                    _vmFee = "T";
        //                    _lblFee = "Tax";
        //                    _colFee = "/*+ ordered */ evfeep.PAYMENTTYPE";
        //                    _joinFee += Cst.CrLf + padLeft + "inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
        //                }
        //                sqlFrom += "from       (/* VM-Begin" + _vmFee + " */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('" + EventCodeEnum.OPP.ToString() + "', '" + EventCodeEnum.SKP.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  REMOVED Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       @BUSINESSDATE as DTVAL, 1 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTCODE in ('" + EventCodeEnum.OPP.ToString() + "', '" + EventCodeEnum.SKP.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ecrmv.EVENTCLASS = '" + EventClassEnum.RMV.ToString() + "') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-|  Unsettled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
        //                sqlFrom += padLeft + "       ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
        //                sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
        //                sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = '" + EventClassEnum.STL.ToString() + "') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE) and (ecrec.EVENTCLASS = '" + EventClassEnum.REC.ToString() + "') and (ecrec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('" + EventCodeEnum.OPP.ToString() + "', '" + EventCodeEnum.SKP.ToString() + "')) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += padLeft + _joinFee + Cst.CrLf;
        //                sqlFrom += padLeft + "where (ec.EVENTCLASS = '" + EventClassEnum.VAL.ToString() + "') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
        //                #endregion  Frais - OPP
        //            }
        //            else
        //            {
        //                #region  Other flows
        //                sqlFrom += "from       (/* VM-Begin */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+--+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  Flows  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+--+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, /*ec.DTEVENT*/ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.REGULAR.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += "            where (ec.DTEVENT >= @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "              and (" + Cst.CrLf;
        //                sqlFrom += "                    (" + Cst.CrLf;
        //                sqlFrom += "                      (ev.EVENTTYPE = 'MGR' and ec.EVENTCLASS in ('FWR','G_K','VAL'))" + Cst.CrLf;
        //                sqlFrom += "                       or " + Cst.CrLf;
        //                sqlFrom += "                      (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','RMG','VMG','UMG') and ec.EVENTCLASS='VAL')" + Cst.CrLf;
        //                sqlFrom += "                     )" + Cst.CrLf;
        //                sqlFrom += "                     or" + Cst.CrLf;
        //                sqlFrom += "                     (" + Cst.CrLf;
        //                sqlFrom += "                       (ec.ISPAYMENT = 1) and (ec.EVENTCLASS='VAL')" + Cst.CrLf;
        //                sqlFrom += "                       and" + Cst.CrLf;
        //                sqlFrom += "                       ( (ev.EVENTTYPE = 'PRM') or (ev.EVENTCODE = 'TER' and ev.EVENTTYPE in ('CU1','CU2','SCU')) )" + Cst.CrLf;
        //                sqlFrom += "                     )" + Cst.CrLf;
        //                sqlFrom += "                   )" + Cst.CrLf;
        //                sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;
        //                sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows  |+-+-+-+-+- */" + Cst.CrLf;
        //                sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
        //                sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, /*ecrmv.DTEVENT*/ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
        //                sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.IDSTACTIVATION = '" + Cst.StatusActivation.DEACTIV.ToString() + "')" + Cst.CrLf;
        //                sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
        //                sqlFrom += "              and (" + Cst.CrLf;
        //                sqlFrom += "                    (" + Cst.CrLf;
        //                sqlFrom += "                      (ev.EVENTTYPE = 'MGR' and ec.EVENTCLASS in ('FWR','G_K','VAL'))" + Cst.CrLf;
        //                sqlFrom += "                       or " + Cst.CrLf;
        //                sqlFrom += "                      (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','RMG','VMG','UMG') and ec.EVENTCLASS='VAL')" + Cst.CrLf;
        //                sqlFrom += "                     )" + Cst.CrLf;
        //                sqlFrom += "                     or" + Cst.CrLf;
        //                sqlFrom += "                     (" + Cst.CrLf;
        //                sqlFrom += "                       (ec.ISPAYMENT = 1) and (ec.EVENTCLASS='VAL')" + Cst.CrLf;
        //                sqlFrom += "                       and" + Cst.CrLf;
        //                sqlFrom += "                       ( (ev.EVENTTYPE = 'PRM') or (ev.EVENTCODE = 'TER' and ev.EVENTTYPE in ('CU1','CU2','SCU')) )" + Cst.CrLf;
        //                sqlFrom += "                     )" + Cst.CrLf;
        //                sqlFrom += "                   )" + Cst.CrLf;
        //                sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
        //                #endregion  Other flows
        //            }
        //            sqlFrom += "inner join dbo.TRADE tr on (tr.IDT=ev.IDT)" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=tr.IDT)" + Cst.CrLf;
                    
        //            // FI 20170208 [21916]  add restriction sur chambre valide 
        //            // ti.IDA_CSSCUSTODIAN is null est nécessaire pour gérer les trades OTC non ALLOC  
        //            // FI 20170208 [22151][22152] suppression de la restriction sur les chambre traitée avec succès
        //            /* sqlFrom += StrFunc.AppendFormat(" and (({0}) or (ti.IDA_CSSCUSTODIAN is null))", m_CBHierarchy.SQLRestrictCssCustodianValidOrUnValid(pCS, "ti", true, String.Empty)) + Cst.CrLf;*/
        //            sqlFrom += "inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.GPRODUCT in ('COM','FX','OTC','SEC')) and (p.FUNGIBILITYMODE='NONE')" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.TRADESTSYS tsys on (tsys.IDT=tr.IDT) and (tsys.IDSTACTIVATION='" + Cst.StatusActivation.REGULAR.ToString() + "') and (tsys.IDSTBUSINESS in ('ALLOC','EXECUTED','INTERMED'))" + Cst.CrLf;
        //            sqlFrom += "inner join dbo.BOOK b on (b.IDB=ti.IDB_BUYER) or (b.IDB=ti.IDB_SELLER)" + Cst.CrLf;
        //            sqlFrom += "inner join " + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = b.IDA)" + Cst.CrLf;
        //            #endregion TradeFlows
        //            break;
        //    }
        //    return sqlFrom;
        //}
        //#endregion GetSubQueryFlows_From
        //#region GetSubQueryFlows_Where
        ///// <summary>
        ///// Construction d'une partie de requête : Where
        ///// </summary>
        ///// <typeparam name="T">voir les fonctions appelantes</typeparam>
        ///// <param name="pCS">Chaine de connexion</param>
        ///// <param name="pFlowType">Type de flux</param>
        ///// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        ///// <returns>Partie de requête : Where</returns>
        ///// EG 20140218 [19575][19666] REFACTORING QUERY
        ///// PM 20140911 [20066][20185] Refactoring
        ///// PM 20150323 [POC] Add TradeFlows 
        //private string deprecated_GetSubQueryFlows_Where<T>(string pCS, FlowTypeEnum pFlowType, T pConditionSwitch)
        //{
        //    string sqlWhere = string.Empty;
        //    string actorSuffix = string.Empty;
        //    bool isFees = pConditionSwitch is Pair<string, bool>;

        //    switch (pFlowType)
        //    {
        //        case FlowTypeEnum.CashFlows:
        //            #region CashFlows
        //            // Les événements du jour :
        //            // - Non désactivés (REGULAR)
        //            // ou bien
        //            // Les événements passés, annulés le jour même, pour extourner les flux
        //            // EG 20140226 [19575][19666]
        //            sqlWhere = SQLCst.WHERE.Trim() + " (tr.DTBUSINESS <= @BUSINESSDATE)";

        //            if (isFees)
        //            {
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                sqlWhere += " and (ev.IDTAX is " + (_conditionSwitch.Second ? "not null" : "null") + ")";
        //            }
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlWhere += " and (tr.IDA_ENTITY" + actorSuffix + " = @IDA_ENTITY) and ((tr.IDA_" + actorSuffix + " = ev.IDA_PAY) or (tr.IDA_" + actorSuffix + " = ev.IDA_REC))" + Cst.CrLf;
        //            #endregion CashFlows
        //            break;
        //        case FlowTypeEnum.Collateral:
        //            #region Collateral
        //            sqlWhere = SQLCst.WHERE + "(pc.DTBUSINESS <= @BUSINESSDATE) and ((pc.DTTERMINATION is null) or (pc.DTTERMINATION >=@BUSINESSDATE))" + Cst.CrLf;
        //            sqlWhere += SQLCst.AND + "(pc.IDA_" + GetPayerReceiverTag(pConditionSwitch, true) + "= @IDA_ENTITY) and " + Cst.CrLf;
        //            sqlWhere += "(pc.ASSETCATEGORY " + ((pConditionSwitch as Pair<PayerReceiverEnum, bool>).Second ? "=" : "!=") + " @ASSETCATEGORY_CASH) and " + Cst.CrLf;
        //            sqlWhere += "(pc.IDA_" + GetPayerReceiverTag(pConditionSwitch) + " in (" + Cst.CrLf +
        //                GetQuery_CBACTOR(pCS, (pConditionSwitch as Pair<PayerReceiverEnum, bool>).First, CBHierarchy.RolesCboMro) + "))" + Cst.CrLf;
        //            #endregion Collateral
        //            break;
        //        case FlowTypeEnum.Deposit:
        //            #region Deposit
        //            sqlWhere = SQLCst.WHERE + "(tr.DTBUSINESS = @BUSINESSDATE) and (ev.IDA_" + GetPayerReceiverTag(pConditionSwitch, true) + " = @IDA_ENTITY) and " + Cst.CrLf;
        //            sqlWhere += "(b.IDA in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, (pConditionSwitch as Pair<PayerReceiverEnum, bool>).First, new RoleActor[1] { RoleActor.MARGINREQOFFICE }) + "))" + Cst.CrLf;
        //            #endregion Deposit
        //            break;
        //        case FlowTypeEnum.LastCashBalance:
        //            #region LastCashBalance
        //            sqlWhere = SQLCst.WHERE + "(tr.DTBUSINESS = @BUSINESSDATE_PREV) and " + Cst.CrLf;
        //            sqlWhere += "(" + Cst.CrLf;
        //            sqlWhere += "((ev.IDA_REC = @IDA_ENTITY) and (ev.IDA_PAY in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + ")))" + Cst.CrLf;
        //            sqlWhere += " or " + Cst.CrLf;
        //            sqlWhere += "((ev.IDA_PAY = @IDA_ENTITY) and (ev.IDA_REC in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + ")))" + Cst.CrLf;
        //            sqlWhere += ")" + Cst.CrLf;
        //            #endregion LastCashBalance
        //            break;
        //        case FlowTypeEnum.OtherFlows:
        //            #region OtherFlows
        //            // Les événements du jour :
        //            // - Non désactivés (REGULAR)
        //            // ou bien
        //            // Les événements passés, annulés le jour même, pour extourner les flux
        //            // EG 20140226 [19575][19666]
        //            sqlWhere = SQLCst.WHERE + "(tr.DTBUSINESS <= @BUSINESSDATE)";
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlWhere += " and (tr.IDA_ENTITY" + actorSuffix + "=@IDA_ENTITY) and ((tr.IDA_" + actorSuffix + "=ev.IDA_PAY) or (tr.IDA_" + actorSuffix + "=ev.IDA_REC))" + Cst.CrLf;
        //            //--------------------------------------------------------------------------------
        //            // NB : Les montants RMG 
        //            // - Il faut prendre le montant sur le trade clôturant uniquement
        //            //   Car le résultat réalisé est disponible à la fois dans les événements du trade clôturé et du trade clôturant
        //            //--------------------------------------------------------------------------------                            
        //            //PL 20160524 sqlWhere += "and (ev.EVENTTYPE != @EVENTTYPE_RMG or pad.IDPADET is not null)" + Cst.CrLf;
        //            #endregion OtherFlows
        //            break;
        //        case FlowTypeEnum.Payment:
        //            #region Payment
        //            // EG 20131118 [19195] DTBUSINESS => DTTRADE
        //            sqlWhere = SQLCst.WHERE + "(tr.DTTRADE = @BUSINESSDATE) and (ev.IDA_" + GetPayerReceiverTag(pConditionSwitch, true) + " = @IDA_ENTITY) and " + Cst.CrLf;
        //            sqlWhere += "(b.IDA in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + "))" + Cst.CrLf;
        //            #endregion Payment
        //            break;
        //        case FlowTypeEnum.SettlementPayment:
        //            #region SettlementPayment
        //            sqlWhere = SQLCst.WHERE + "(ev.IDA_" + GetPayerReceiverTag(pConditionSwitch, true) + " = @IDA_ENTITY) and " + Cst.CrLf;
        //            sqlWhere += "(b.IDA in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + "))" + Cst.CrLf;
        //            #endregion SettlementPayment
        //            break;
        //        case FlowTypeEnum.TradeFlows:
        //            #region TradeFlows
        //            sqlWhere = SQLCst.WHERE + "(tr.DTTRADE <= @BUSINESSDATE)" + Cst.CrLf;
        //            if (isFees)
        //            {
        //                Pair<string, bool> _conditionSwitch = pConditionSwitch as Pair<string, bool>;
        //                sqlWhere += " and (ev.IDTAX is " + (_conditionSwitch.Second ? "not null" : "null") + ")" + Cst.CrLf;
        //            }
        //            #endregion TradeFlows
        //            break;
        //    }
        //    return sqlWhere;
        //}
        //#endregion GetSubQueryFlows_Where
        //#region GetSubQueryFlows_GroupBy
        ///// <summary>
        ///// Construction d'une partie de requête : GroupBy
        ///// </summary>
        ///// <typeparam name="T">voir les fonctions appelantes</typeparam>
        ///// <param name="pFlowType">Type de flux</param>
        ///// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        ///// <returns>Partie de requête : GroupBy</returns>
        ///// EG 20140218 [19575][19666] REFACTORING QUERY
        ///// PM 20150323 [POC] Add TradeFlows 
        ///// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        ///// FI 20170208 [22151][22152] Modify
        //private string deprecated_GetSubQueryFlows_GroupBy<T>(FlowTypeEnum pFlowType, T pConditionSwitch)
        //{
        //    string actorSuffix = string.Empty;
        //    string sqlGroupBy = string.Empty;
        //    bool isFees = pConditionSwitch is Pair<string, bool>;

        //    switch (pFlowType)
        //    {
        //        case FlowTypeEnum.AllocNotFungibleFlows:
        //            #region AllocNotFungibleFlows
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlGroupBy = SQLCst.GROUPBY + "IDA_OWNER" + actorSuffix + ", IDB_" + actorSuffix + ", ev.DTVAL, ev.EVENTTYPE, ev.UNIT, p.IDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET ";
        //            if (isFees)
        //            {
        //                sqlGroupBy += ", ev.EVENTCODE, ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE ";
        //            }
        //            sqlGroupBy += "," + Cst.CrLf + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid);
        //            #endregion AllocNotFungibleFlows
        //            break;
        //        case FlowTypeEnum.CashFlows:
        //            #region CashFlows
        //            // PM 20150616 [21124] Add column DTVAL
        //            //PL 20160607
        //            //sqlGroupBy = SQLCst.GROUPBY.Trim() + " b.IDA, b.IDB, ev.UNIT, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end, asset.ASSETCATEGORY, ev.DTVAL, ";
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlGroupBy = SQLCst.GROUPBY.Trim() + " tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", ev.UNIT, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end, asset.ASSETCATEGORY, ev.DTVAL, ";

        //            if (isFees)
        //            {
        //                // PM 20150709 [21103] Ajout ev.EVENTCODE
        //                sqlGroupBy += "ev.EVENTCODE, ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE ";
        //            }
        //            else
        //            {
        //                sqlGroupBy += "ev.EVENTTYPE ";
        //            }
        //            // FI 20170208 [22151][22152] add colum STATUS
        //            sqlGroupBy += "," + SQLColumnStatus(cs, "tr", string.Empty);
        //            #endregion CashFlows
        //            break;
        //        case FlowTypeEnum.OtherFlows:
        //            #region OtherFlows
        //            // PM 20150616 [21124] Add column DTVAL
        //            //sqlGroupBy = SQLCst.GROUPBY + "b.IDA, b.IDB, ev.EVENTTYPE, asset.ASSETCATEGORY, asset.CATEGORY, asset.FUTVALUATIONMETHOD, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end, tr.SIDE, ev.UNIT, ev.DTVAL ";
        //            //PL 20160607
        //            actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //            sqlGroupBy = SQLCst.GROUPBY + "tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", ev.EVENTTYPE, asset.ASSETCATEGORY, asset.CATEGORY, asset.FUTVALUATIONMETHOD, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end, tr.SIDE, ev.UNIT, ev.DTVAL ";
        //            // FI 20170208 [22151][22152] add column STATUS
        //            sqlGroupBy += "," + SQLColumnStatus(cs, "tr", string.Empty);
        //            #endregion OtherFlows
        //            break;
        //        case FlowTypeEnum.TradeFlows:
        //            #region TradeFlows
        //            /*sqlGroupBy = SQLCst.GROUPBY + "b.IDA, b.IDB, ev.DTEVENT, ev.EVENTTYPE, ev.UNIT, p.IDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET ";*/
        //            sqlGroupBy = SQLCst.GROUPBY + "b.IDA, b.IDB, ev.DTVAL, ev.EVENTTYPE, ev.UNIT, p.IDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET ";
        //            if (isFees)
        //            {
        //                sqlGroupBy += ", ev.EVENTCODE, ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE ";
        //            }
        //            // FI 20170208 [22151][22152] add column STATUS
        //            sqlGroupBy += "," + Cst.CrLf + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid);
        //            #endregion TradeFlows
        //            break;
        //    }
        //    return sqlGroupBy;
        //}
        //#endregion GetSubQueryFlows_GroupBy
        #endregion deprecated_GetSubQueryFlows

        #region GetSubQueryFlows (Column / From / Where / GroupBy)
        #region GetSubQueryFlows
        /// <summary>
        /// Construction d'une partie de requête : Select + From + Where + [GroupBy]
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pConditionSwitch">Informations des conditions de construction de la requête</param>
        /// <returns>Partie de requête</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private string GetSubQueryFlows(string pCS, FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string sqlSubQuery = GetSubQueryFlows_Column(pFlowType, pConditionSwitch);
            sqlSubQuery += GetSubQueryFlows_From(pCS, pFlowType, pConditionSwitch);
            sqlSubQuery += GetSubQueryFlows_Where(pCS, pFlowType, pConditionSwitch);
            sqlSubQuery += GetSubQueryFlows_GroupBy(pFlowType, pConditionSwitch);
            return sqlSubQuery + Cst.CrLf;
        }
        #endregion GetSubQueryFlows
        #region GetSubQueryFlows_Column
        /// <summary>
        /// Construction d'une partie de requête : Select
        /// </summary>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pConditionSwitch">Informations des conditions de construction de la requête</param>
        /// <returns>Partie de requête : Select</returns>
        /// EG 20140218 [19575][19666] REFACTORING QUERY
        /// PM 20140911 [20066][20185] Refactoring
        /// PM 20150323 [POC] Add TradeFlows
        /// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        /// FI 20160530 [21885] Modify 
        /// FI 20170208 [22151][22152] Modify
        /// PM 20170213 [21916] Refactoring pConditionSwitch et ajout AllocNotFungibleFlows pour Commodity Spot
        /// FI 20170316 [22950] Modify
        private string GetSubQueryFlows_Column(FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string sqlSelect = string.Empty;
            string padLeft = new string(' ', 7);
            string actorSuffix = pConditionSwitch.DealerClearerSuffixe;
            switch (pFlowType)
            {
                case FlowTypeEnum.AllocNotFungibleFlows:
                    #region AllocNotFungibleFlows
                    sqlSelect += SQLCst.SELECT + "/*+ ordered */ b" + actorSuffix.ToLower() + ".IDA as IDA, ti.IDB_" + actorSuffix + " as IDB, ev.DTVAL, ev.UNIT as IDC, p.IDENTIFIER as PRODUCTIDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET, " + Cst.CrLf;
                    if (pConditionSwitch.IsFees)
                    {
                        sqlSelect += padLeft + "ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE, ";
                        if (pConditionSwitch.IsTax)
                        {
                            // Taxes
                            sqlSelect += "ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE, " + Cst.CrLf;
                        }
                        else
                        {
                            // Frais sans les taxes
                            sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE, " + Cst.CrLf;
                        }
                    }
                    else
                    {
                        // Autres flux
                        sqlSelect += padLeft + "ev.EVENTTYPE as AMOUNTSUBTYPE, null as PAYMENTTYPE, ";
                        sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE, " + Cst.CrLf;
                    }
                    sqlSelect += padLeft + GetFlows_CommonAmount(pFlowType, pConditionSwitch) + ", " + Cst.CrLf;
                    sqlSelect += padLeft + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end as STATUS", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid) + Cst.CrLf;
                    #endregion AllocNotFungibleFlows
                    break;
                case FlowTypeEnum.CashFlows:
                    #region CashFlows
                    // RD 20170809 [23370][23432] Utiliser VW_ASSET à la place de VW_ASSET_WOETD et réfactoring de la requête
                    //sqlSelect += SQLCst.SELECT + "/*+ ordered */ tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, ev.UNIT as IDC, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end as IDASSET, asset.ASSETCATEGORY, ev.DTVAL, ";
                    sqlSelect += SQLCst.SELECT + "/*+ ordered */ tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, ev.UNIT as IDC," + Cst.CrLf;
                    sqlSelect += "case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC," + Cst.CrLf;
                    sqlSelect += "case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET," + Cst.CrLf;
                    sqlSelect += "asset.ASSETCATEGORY, ev.DTVAL, ";
                    if (pConditionSwitch.IsFees)
                    {
                        sqlSelect += padLeft + "ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE, ";
                        if (pConditionSwitch.IsTax)
                        {
                            // Taxes
                            sqlSelect += "ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE, " + Cst.CrLf;

                        }
                        else
                        {
                            // Frais sans les taxes
                            sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE, " + Cst.CrLf;
                        }
                    }
                    else
                    {
                        // Autres flux
                        sqlSelect += padLeft + "ev.EVENTTYPE as AMOUNTSUBTYPE, null as PAYMENTTYPE, ";
                        sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE, " + Cst.CrLf;
                    }
                    sqlSelect += padLeft + GetFlows_CommonAmount(pFlowType, pConditionSwitch) + ", " + Cst.CrLf;
                    sqlSelect += padLeft + SQLColumnStatus(cs, "tr", "STATUS") + Cst.CrLf;
                    #endregion CashFlows
                    break;
                case FlowTypeEnum.Collateral:
                    #region Collateral
                    sqlSelect = SQLCst.SELECT + "pc.IDPOSCOLLATERAL, pc.IDA_CSS, pc.ASSETCATEGORY, pc.IDASSET, pc.HAIRCUTFORCED, pc.DTBUSINESS, " + Cst.CrLf;
                    if (pConditionSwitch.IsPayer)
                    {
                        sqlSelect += padLeft + "pc.IDA_PAY as IDA, pc.IDB_PAY as IDB, (-1 * pcval_last.VALORISATION) as AMOUNT, " + Cst.CrLf;
                    }
                    else if (pConditionSwitch.IsReceiver)
                    {
                        sqlSelect += padLeft + "pc.IDA_REC as IDA, pc.IDB_REC as IDB, (pcval_last.VALORISATION) as AMOUNT, " + Cst.CrLf;
                    }
                    sqlSelect += padLeft + "pcval_last.IDPOSCOLLATERALVAL, pcval_last.IDC, case when pcval_last.IDPOSCOLLATERALVAL is null then 0 else 1 end as ISVALORISED, " + Cst.CrLf;
                    sqlSelect += padLeft + "pcval_last.QTY as QTYVAL, " + Cst.CrLf;
                    sqlSelect += padLeft + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid) + Cst.CrLf;
                    #endregion Collateral
                    break;
                case FlowTypeEnum.Deposit:
                    #region Deposit
                    {
                        // FI 20170208 [22151][22152] 
                        // FI 20170316 [22950] add column DTBUSINESS
                        sqlSelect = SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, tr.DTSYS, ti.IDA_CSSCUSTODIAN as IDA_CSS, " + Cst.CrLf;
                        sqlSelect += GetFlows_CommonAmount(pFlowType, pConditionSwitch) + "," + Cst.CrLf;
                        // FI 20170208 [22151][22152] condition en paramètre à la méthode GetFlows_CommonAmount
                        if (pConditionSwitch.IsFilterCss)
                        {
                            // on passe ici lors de la lecture d'un deposit à une date antérieure (nécessairement valid)
                            sqlSelect += StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid);
                        }
                        else
                        {
                            sqlSelect += SQLColumnStatus(cs, "ti", "STATUS");
                        }
                    }
                    #endregion Deposit
                    break;
                case FlowTypeEnum.LastCashBalance:
                    #region LastCashBalance
                    // FI 20170316 [22950] add column DTBUSINESS
                    padLeft = new string(' ', 16);
                    sqlSelect = SQLCst.SELECT_DISTINCT + "tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, tr.DTSYS, ec.DTEVENT, " + Cst.CrLf;
                    sqlSelect += padLeft + "case when ev.IDA_PAY = @IDA_ENTITY then ev.IDA_REC else ev.IDA_PAY end as IDA, " + Cst.CrLf;
                    sqlSelect += padLeft + "case when ev.IDA_PAY = @IDA_ENTITY then ev.IDB_REC else ev.IDB_PAY end as IDB, " + Cst.CrLf;
                    sqlSelect += padLeft + "ev.UNIT as IDC, case when ev.IDA_PAY = @IDA_ENTITY then 1 else -1 end * ev.VALORISATION as AMOUNT, ev.EVENTTYPE as AMOUNTSUBTYPE, " + Cst.CrLf;
                    sqlSelect += padLeft + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid) + Cst.CrLf;
                    #endregion LastCashBalance
                    break;
                case FlowTypeEnum.OtherFlows:
                    #region OtherFlows
                    // RD 20170809 [23370][23432] Utiliser VW_ASSET à la place de VW_ASSET_WOETD et réfactoring de la requête
                    //sqlSelect += SQLCst.SELECT + "/*+ ordered */ tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, asset.ASSETCATEGORY, asset.CATEGORY, asset.FUTVALUATIONMETHOD, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end as IDASSET, tr.SIDE, ev.DTVAL, " + Cst.CrLf;
                    sqlSelect += SQLCst.SELECT + "/*+ ordered */ tr.IDA_OWNER" + actorSuffix + " as IDA, tr.IDB_" + actorSuffix + " as IDB, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, asset.ASSETCATEGORY, asset_ETD.CATEGORY, asset_ETD.FUTVALUATIONMETHOD," + Cst.CrLf;
                    sqlSelect += "case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC," + Cst.CrLf;
                    sqlSelect += "case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET," + Cst.CrLf;
                    sqlSelect += "tr.SIDE, ev.DTVAL," + Cst.CrLf;
                    sqlSelect += padLeft + GetFlows_CommonAmount(pFlowType, pConditionSwitch) + ", " + Cst.CrLf;
                    sqlSelect += padLeft + SQLColumnStatus(cs, "tr", "STATUS") + Cst.CrLf;
                    #endregion OtherFlows
                    break;
                case FlowTypeEnum.Payment:
                    #region Payment
                    // FI 20170316 [22950] add column DTBUSINESS
                    sqlSelect = SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, tr.DTSYS, " + Cst.CrLf;
                    sqlSelect += padLeft + GetFlows_CommonAmount(pFlowType, pConditionSwitch) + ", " + Cst.CrLf;
                    sqlSelect += padLeft + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid) + Cst.CrLf;
                    #endregion Payment
                    break;
                case FlowTypeEnum.SettlementPayment:
                    #region SettlementPayment
                    // FI 20170316 [22950] add column DTBUSINESS
                    sqlSelect = SQLCst.SELECT + "b.IDA, b.IDB, ev.UNIT as IDC, tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, tr.DTSYS, ec.DTEVENT, ";
                    sqlSelect += padLeft + "case when (ec.DTEVENT = @BUSINESSDATE) then 0 else 1 end as ISFORWARD, " + Cst.CrLf;
                    sqlSelect += padLeft + GetFlows_CommonAmount(pFlowType, pConditionSwitch) + ", " + Cst.CrLf;
                    sqlSelect += padLeft + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid) + Cst.CrLf;
                    #endregion SettlementPayment
                    break;
                case FlowTypeEnum.TradeFlows:
                    #region TradeFlows
                    sqlSelect += SQLCst.SELECT + "/*+ ordered */ b.IDA, b.IDB, ev.DTVAL, ev.UNIT as IDC, p.IDENTIFIER as PRODUCTIDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET," + Cst.CrLf;
                    sqlSelect += "       sum(case b.IDA when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end" + Cst.CrLf;
                    sqlSelect += "                      when ev.IDA_REC then case when (ISREMOVED=0) then  1 else -1 end" + Cst.CrLf;
                    sqlSelect += "                      end * ev.VALORISATION) as AMOUNT, " + Cst.CrLf;
                    if (pConditionSwitch.IsFees)
                    {
                        sqlSelect += padLeft + "ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE, ";
                        if (pConditionSwitch.IsTax)
                        {
                            // Taxes
                            sqlSelect += "ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE, " + Cst.CrLf;
                        }
                        else
                        {
                            // Frais sans les taxes
                            sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE, " + Cst.CrLf;
                        }
                    }
                    else
                    {
                        // Autres flux
                        sqlSelect += padLeft + "ev.EVENTTYPE as AMOUNTSUBTYPE, null as PAYMENTTYPE, ";
                        sqlSelect += "null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE, " + Cst.CrLf;
                    }
                    sqlSelect += padLeft + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end as STATUS", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid) + Cst.CrLf;
                    #endregion TradeFlows
                    break;
            }
            return sqlSelect;
        }
        #endregion GetSubQueryFlows_Column
        #region GetSubQueryFlows_From
        /// <summary>
        /// Construction d'une partie de requête : From / Join
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pConditionSwitch">Informations des conditions de construction de la requête</param>
        /// <returns></returns>
        /// FI 20161021 [22152] Modify
        /// FI 20170208 [21916] Modify
        /// FI 20170208 [22151][22152] Modify
        /// PM 20170213 [21916] Refactoring pConditionSwitch et ajout AllocNotFungibleFlows pour Commodity Spot
        /// FI 20170217 [22862] Modify
        // EG 20181119 PERF Correction post RC (Step 2)
        // PL 20190628 Add Flows TER/INT (for DSE) and Add Unsettled INT|TER/INT
        // PL 20190718 Remove Unsettled INT|TER/INT (VALBURY ne souhaitant pas voir les couposn avant leur règlement). 
        private string GetSubQueryFlows_From(string pCS, FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string sqlFrom = string.Empty;
            string padLeft = new string(' ', 13);
            string actorSuffix = pConditionSwitch.DealerClearerSuffixe;
            switch (pFlowType)
            {
                case FlowTypeEnum.AllocNotFungibleFlows:
                    #region AllocNotFungibleFlows
                    if (pConditionSwitch.IsFees)
                    {
                        #region Frais - OPP
                        string _vmFee = "F";
                        string _lblFee = "Fee";
                        string _colFee = "/*+ ordered */ evfee.PAYMENTTYPE";
                        string _joinFee = "inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)";
                        if (pConditionSwitch.IsTax)
                        {
                            _vmFee = "T";
                            _lblFee = "Tax";
                            _colFee = "/*+ ordered */ evfeep.PAYMENTTYPE";
                            _joinFee += Cst.CrLf + padLeft + "inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
                        }
                        sqlFrom += "from       (/* VM-Begin" + _vmFee + " */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT, ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + " from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  REMOVED Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "select " + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT, @BUSINESSDATE as DTVAL, 1 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + " from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  Unsettled " + _lblFee + "  |-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + "select " + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + " from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE) and (ecrec.EVENTCLASS = 'REC') and (ecrec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
                        #endregion  Frais - OPP
                    }
                    else
                    {
                        #region  Not OPP flows
                        sqlFrom += " from      (/* Begin not OPP */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  Flows |-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "             from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += "            where (ec.DTEVENT >= @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "              and (ev.EVENTCODE = 'LPP') and (ev.EVENTTYPE = 'GAM') and (ec.EVENTCLASS = 'VAL')" + Cst.CrLf;

                        sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows  |-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "             from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += "            where (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "              and (ev.EVENTCODE = 'LPP') and (ev.EVENTTYPE = 'GAM') and (ec.EVENTCLASS = 'VAL')" + Cst.CrLf;
                        sqlFrom += " /* End  */) ev" + Cst.CrLf;
                        #endregion  Not OPP flows
                    }
                    sqlFrom += "inner join dbo.TRADE tr on (tr.IDT = ev.IDT)" + Cst.CrLf;
                    sqlFrom += "inner join dbo.TRADEINSTRUMENT ti on (ti.IDT = tr.IDT)" + Cst.CrLf;
                    sqlFrom += "inner join dbo.INSTRUMENT i on (i.IDI = ti.IDI)" + Cst.CrLf;
                    sqlFrom += "inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.GPRODUCT = 'COM') and (p.FUNGIBILITYMODE = 'NONE')" + Cst.CrLf;
                    sqlFrom += "inner join dbo.TRADESTSYS tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = 'REGULAR') and (tsys.IDSTBUSINESS  = 'ALLOC')" + Cst.CrLf;
                    sqlFrom += "inner join dbo.BOOK bdealer on (bdealer.IDB = ti.IDB_DEALER)" + Cst.CrLf;
                    sqlFrom += "left outer join dbo.BOOK bclearer on (bclearer.IDB = ti.IDB_CLEARER)" + Cst.CrLf;
                    sqlFrom += "inner join dbo." + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = b" + actorSuffix.ToLower() + ".IDA)" + Cst.CrLf;
                    #endregion AllocNotFungibleFlows
                    break;
                case FlowTypeEnum.CashFlows:
                    #region CashFlows
                    if (pConditionSwitch.IsFees)
                    {
                        #region Frais - OPP
                        string _vmFee = "F";
                        string _lblFee = "Fee";
                        string _colFee = "/*+ ordered */ evfee.PAYMENTTYPE";
                        string _joinFee = "inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)";
                        if (pConditionSwitch.IsTax)
                        {
                            _vmFee = "T";
                            _lblFee = "Tax";
                            _colFee = "/*+ ordered */ evfeep.PAYMENTTYPE";
                            _joinFee += Cst.CrLf + padLeft + "inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
                        }

                        sqlFrom += "from       (/* VM-Begin" + _vmFee + " */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  REMOVED Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + " from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  Unsettled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       /* @BUSINESSDATE as DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + " from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE) and (ecrec.EVENTCLASS = 'REC') and (ecrec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
                        #endregion Frais - OPP
                    }
                    else
                    {
                        // FI 20170217 [22862] add DVA
                        // PM 20170911 [23408] Ajout EQP
                        #region Cash-flows - PRM/VMG/SCU
                        sqlFrom += "from       (/* VM-Begin */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  Variation Margin/Cash Settlement/  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                        sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "             from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTTYPE in ('VMG','SCU')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += "            where (ec.EVENTCLASS='VAL') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  Option Premium/Delivery Amount  |+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                        sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "             from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS='STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTTYPE in ('PRM','DVA','EQP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += "            where (ec.EVENTCLASS='VAL') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  REMOVED Variation Margin/Cash Settlement  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                        sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "             from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS='VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTTYPE in ('VMG', 'SCU')) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  REMOVED Option Premium/Delivery Amount  |+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                        sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS='STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS='VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTTYPE in ('PRM','DVA','EQP')) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
                        #endregion Cash-flows - PRM/VMG/SCU
                    }

                    sqlFrom += SQLCst.INNERJOIN_DBO.Trim() + Cst.OTCml_TBL.VW_TRADE_FUNGIBLE + " tr on (tr.IDT=ev.IDT)";

                    if (pConditionSwitch.IsJoinAsset)
                    {
                        // RD 20170726 [23370][23432] Utiliser VW_ASSET à la place de VW_ASSET_WOETD et réfactoring de la requête
                        //sqlFrom += "inner join ( select da.IDDC, asset.IDASSET, 'ExchangeTradedContract' as ASSETCATEGORY" + Cst.CrLf;
                        //sqlFrom += "               from dbo.ASSET_ETD asset" + Cst.CrLf;
                        //sqlFrom += "              inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
                        //sqlFrom += "             union all -- --------------------------------------------------------------------------------" + Cst.CrLf;
                        //sqlFrom += "             select 0 as IDDC, asset.IDASSET, asset.ASSETCATEGORY" + Cst.CrLf;
                        //sqlFrom += "               from dbo.VW_ASSET_WOETD asset" + Cst.CrLf;
                        //sqlFrom += "           ) asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
                        sqlFrom += "inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
                        sqlFrom += "left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)" + Cst.CrLf;
                    }
                    sqlFrom += "inner join dbo." + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = tr.IDA_OWNER" + actorSuffix + ")" + Cst.CrLf;
                    #endregion CashFlows
                    break;
                case FlowTypeEnum.Collateral:
                    #region Collateral
                    {
                        sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_COLLATERALPOS + " pc" + Cst.CrLf;
                        if (pConditionSwitch.IsAssetCash)
                        {
                            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
                            // Chargement de la dernière valorisation connue pour les Collateral de type Cash
                            if (DbSvrType.dbSQL == serverType)
                            {
                                sqlFrom += SQLCst.X_LEFT + "(" + Cst.CrLf;
                                sqlFrom += SQLCst.SELECT + "pc.IDPOSCOLLATERAL," + Cst.CrLf;
                                sqlFrom += "(" + Cst.CrLf;
                                sqlFrom += SQLCst.SELECT_TOP + "1 pcval.IDPOSCOLLATERALVAL" + Cst.CrLf;
                                sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval" + Cst.CrLf;
                                sqlFrom += SQLCst.WHERE + "(pcval.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval.IDSTACTIVATION = 'REGULAR') and (pcval.DTBUSINESS <= @BUSINESSDATE)" + Cst.CrLf;
                                sqlFrom += SQLCst.ORDERBY + "pcval.DTBUSINESS" + SQLCst.DESC + Cst.CrLf;
                                sqlFrom += ") as IDPOSCOLLATERALVAL" + Cst.CrLf;
                                sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_COLLATERALPOS + " pc" + Cst.CrLf;
                                sqlFrom += ") pcval" + SQLCst.ON + "(pcval.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL)" + Cst.CrLf;
                                sqlFrom += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval_last on (pcval_last.IDPOSCOLLATERALVAL = pcval.IDPOSCOLLATERALVAL)" + Cst.CrLf;
                            }
                            else if (DbSvrType.dbORA == serverType)
                            {
                                sqlFrom += SQLCst.X_LEFT + "(" + Cst.CrLf;
                                sqlFrom += SQLCst.SELECT + "max(pcval.DTBUSINESS) as LAST_DTBUSINESS, pcval.IDPOSCOLLATERAL" + Cst.CrLf;
                                sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval" + Cst.CrLf;
                                sqlFrom += SQLCst.WHERE + "(pcval.IDSTACTIVATION='REGULAR') and (pcval.DTBUSINESS <= @BUSINESSDATE)" + Cst.CrLf;
                                sqlFrom += SQLCst.GROUPBY + "pcval.IDPOSCOLLATERAL" + Cst.CrLf;
                                sqlFrom += ") pcval_last1" + SQLCst.ON + "(pcval_last1.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL)" + Cst.CrLf;
                                sqlFrom += SQLCst.X_LEFT + "(" + Cst.CrLf;
                                sqlFrom += SQLCst.SELECT + "pcval.DTBUSINESS, pcval.IDPOSCOLLATERAL, pcval.IDPOSCOLLATERALVAL, pcval.IDC, pcval.QTY, pcval.VALORISATION" + Cst.CrLf;
                                sqlFrom += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval" + Cst.CrLf;
                                sqlFrom += SQLCst.WHERE + "(pcval.IDSTACTIVATION='REGULAR') and (pcval.DTBUSINESS <= @BUSINESSDATE)" + Cst.CrLf;
                                sqlFrom += ") pcval_last" + SQLCst.ON + "(pcval_last.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval_last.DTBUSINESS = pcval_last1.LAST_DTBUSINESS)" + Cst.CrLf;
                            }
                        }
                        else
                        {
                            sqlFrom += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.POSCOLLATERALVAL + " pcval_last on (pcval_last.IDPOSCOLLATERAL=pc.IDPOSCOLLATERAL) and";
                            sqlFrom += "(pcval_last.DTBUSINESS=@BUSINESSDATE)" + Cst.CrLf;
                        }
                    }
                    #endregion Collateral
                    break;
                case FlowTypeEnum.Deposit:
                    #region Deposit
                    sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
                    sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly) + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = 'REGULAR') and (tsys.IDSTENVIRONMENT = 'REGULAR')" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = @EVENTTYPE_MGR) and (ev.EVENTCODE = @EVENTCODE_LPC)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK + " b on (b.IDB = ev.IDB_" + pConditionSwitch.PayerReceiverCode + ")" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @BUSINESSDATE) and (ec.EVENTCLASS = 'REC')" + Cst.CrLf;
                    // FI 20170208 [22151][22152] mise en commenatire et utilisation de TRADEINSTRUMENT
                    //sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADEACTOR + " ta on (ta.IDT = tr.IDT) and (ta.IDROLEACTOR = @IDROLEACTOR_CSS)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADEINSTRUMENT + " ti on ti.IDT=tr.IDT";

                    // FI 20161021 [22152] Add condition SQL
                    if (pConditionSwitch.IsFilterCss)
                    {
                        // FI 20170208 [22151][22152] colonne ti.IDA_CSSCUSTODIAN 
                        // Il s'agit de lire un deposit à une date antérieure
                        //sqlFrom += " and (ta.IDA=@IDA_CSS)" + Cst.CrLf;
                        sqlFrom += " and (ti.IDA_CSSCUSTODIAN=@IDA_CSS)" + Cst.CrLf;
                    }
                    #endregion Deposit
                    break;
                case FlowTypeEnum.LastCashBalance:
                    #region LastCashBalance
                    // PM 20140930 [20066][20185] Ajout Inner Join avec événement parent pour ne pas prendre le stream en contrevaleur
                    sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
                    sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly);
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT)";
                    sqlFrom += " and (ev.EVENTTYPE in (@EVENTTYPE_CSB, @EVENTTYPE_CSA, @EVENTTYPE_CSU, @EVENTTYPE_CLA, @EVENTTYPE_CLU, @EVENTTYPE_UMR, @EVENTTYPE_MGR))" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev_parent on (ev_parent.IDE = ev.IDE_EVENT) and (ev_parent.EVENTCODE = @EVENTCODE_CBS)";
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT=@BUSINESSDATE_PREV) and (ec.EVENTCLASS = 'REC')" + Cst.CrLf;
                    #endregion LastCashBalance
                    break;
                case FlowTypeEnum.OtherFlows:
                    #region OtherFlows
                    sqlFrom += "from       (/* VM-Begin */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  Flows  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','MGR')) and (ev.IDSTACTIVATION='REGULAR')" + Cst.CrLf;
                    sqlFrom += "            where (ec.EVENTCLASS='VAL') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.EVENTCLASS='VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','MGR')) and (ev.IDSTACTIVATION='DEACTIV')" + Cst.CrLf;
                    sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  Flows RMG (except for equitySecurityTransaction)  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.EVENTTYPE='RMG') and (ev.IDSTACTIVATION='REGULAR')" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and ((p.GPRODUCT != 'SEC') or (p.FAMILY != 'ESE'))" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTPOSACTIONDET ev_pad on (ev_pad.IDE=ev.IDE)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.POSACTIONDET pad on (pad.IDPADET=ev_pad.IDPADET) and (pad.IDT_CLOSING=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            where (ec.EVENTCLASS='VAL') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows RMG (except for equitySecurityTransaction)  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.EVENTCLASS='VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.EVENTTYPE='RMG') and (ev.IDSTACTIVATION='DEACTIV')" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and ((p.GPRODUCT != 'SEC') or (p.FAMILY != 'ESE'))" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTPOSACTIONDET ev_pad on (ev_pad.IDE=ev.IDE)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.POSACTIONDET pad on (pad.IDPADET=ev_pad.IDPADET) and (pad.IDT_CLOSING=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  Unsettled GAM et INT|TER/INT |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* @BUSINESSDATE as DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENT ev" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTCLASS ec_rec on (ec_rec.IDE = ev.IDE) and (ec_rec.EVENTCLASS = 'REC') and (ec_rec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
                    sqlFrom += "            where (ev.EVENTTYPE in ('GAM' /* ,'INT' */)) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  Flows UMG et MKV  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* @BUSINESSDATE as DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.IDSTACTIVATION='REGULAR')" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            where (ec.EVENTCLASS='VAL') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
                    sqlFrom += "              and (" + Cst.CrLf;
                    sqlFrom += "                    ((ti.DTSETTLT is null) and (ev.EVENTTYPE='UMG'))" + Cst.CrLf;
                    sqlFrom += "                    or" + Cst.CrLf;
                    sqlFrom += "                    (" + Cst.CrLf;
                    sqlFrom += "                      (ti.DTSETTLT is not null)" + Cst.CrLf;
                    sqlFrom += "                      and" + Cst.CrLf;
                    sqlFrom += "                      (" + Cst.CrLf;
                    sqlFrom += "                        ((ev.EVENTTYPE='MKV') and (ec.DTEVENT >= ti.DTSETTLT))" + Cst.CrLf;
                    sqlFrom += "                        or" + Cst.CrLf;
                    sqlFrom += "                        ((ev.EVENTTYPE='UMG') and (ec.DTEVENT < ti.DTSETTLT))" + Cst.CrLf;
                    sqlFrom += "                      )" + Cst.CrLf;
                    sqlFrom += "                    )" + Cst.CrLf;
                    sqlFrom += "                  )" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  Flows INT|TER/INT (for DSE)  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* ec.DTEVENT, */ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.EVENTCODE in ('INT','TER')) and (ev.EVENTTYPE='INT') and (ev.IDSTACTIVATION='REGULAR')" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.GPRODUCT='SEC') and (p.FAMILY='DSE')" + Cst.CrLf;
                    sqlFrom += "            where (ec.EVENTCLASS='VAL') and (ec.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;

                    sqlFrom += "            union all" + Cst.CrLf;

                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows INT|TER/INT (for DSE)  |+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */" + Cst.CrLf;
                    sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, " + Cst.CrLf;
                    sqlFrom += "                   /* ec.DTEVENT, */ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                    sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.EVENTCLASS='VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.EVENTCODE in ('INT','TER'))and (ev.EVENTTYPE='INT') and (ev.IDSTACTIVATION='DEACTIV')" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
                    sqlFrom += "            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.GPRODUCT='SEC') and (p.FAMILY='DSE')" + Cst.CrLf;
                    sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;

                    sqlFrom += "/* VM-End */) ev" + Cst.CrLf;

                    sqlFrom += " inner join dbo.VW_TRADE_FUNGIBLE tr on (tr.IDT=ev.IDT)" + Cst.CrLf;

                    if (pConditionSwitch.IsJoinAsset)
                    {
                        // RD 20170726 [23370][23432] Utiliser VW_ASSET à la place de VW_ASSET_WOETD et réfactoring de la requête
                        //sqlFrom += "inner join ( select dc.CATEGORY, dc.FUTVALUATIONMETHOD, dc.IDDC, asset.IDASSET, 'ExchangeTradedContract' as ASSETCATEGORY" + Cst.CrLf;
                        //sqlFrom += "             from dbo.ASSET_ETD asset" + Cst.CrLf;
                        //sqlFrom += "             inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
                        //sqlFrom += "             inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)" + Cst.CrLf;
                        //sqlFrom += "             union all -- --------------------------------------------------------------------------------" + Cst.CrLf;
                        //sqlFrom += "             select null as CATEGORY, null as FUTVALUATIONMETHOD, 0 as IDDC, asset.IDASSET, asset.ASSETCATEGORY" + Cst.CrLf;
                        //sqlFrom += "             from dbo.VW_ASSET_WOETD asset" + Cst.CrLf;
                        //sqlFrom += "           ) asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
                        sqlFrom += "inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)" + Cst.CrLf;
                        sqlFrom += "left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)" + Cst.CrLf;
                    }

                    sqlFrom += "inner join dbo." + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = tr.IDA_OWNER" + actorSuffix + ")" + Cst.CrLf;
                    #endregion OtherFlows
                    break;
                case FlowTypeEnum.Payment:
                    #region Payment
                    sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
                    sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly);
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = @EVENTCODE_STA)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK + " b on (b.IDB = ev.IDB_" + pConditionSwitch.PayerReceiverCode + ")" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT=@BUSINESSDATE) and (ec.EVENTCLASS = 'REC')" + Cst.CrLf;
                    #endregion Payment
                    break;
                case FlowTypeEnum.SettlementPayment:
                    #region SettlementPayment
                    sqlFrom = SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " tr" + Cst.CrLf;
                    sqlFrom += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.INSTRUMENT, SQLJoinTypeEnum.Inner, "tr.IDI", "ns", DataEnum.EnabledOnly);
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADESTSYS + " tsys on (tsys.IDT = tr.IDT) and (tsys.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = @EVENTCODE_STA)" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK + " b on (b.IDB = ev.IDB_" + pConditionSwitch.PayerReceiverCode + ")" + Cst.CrLf;
                    sqlFrom += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec on (ec.IDE = ev.IDE) and (ec.DTEVENT>=@BUSINESSDATE) and (ec.EVENTCLASS = 'STL')" + Cst.CrLf;
                    #endregion SettlementPayment
                    break;
                case FlowTypeEnum.TradeFlows:
                    #region TradeFlows
                    if (pConditionSwitch.IsFees)
                    {
                        #region Frais - OPP
                        string _vmFee = "F";
                        string _lblFee = "Fee";
                        string _colFee = "/*+ ordered */ evfee.PAYMENTTYPE";
                        string _joinFee = "inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)";
                        if (pConditionSwitch.IsTax)
                        {
                            _vmFee = "T";
                            _lblFee = "Tax";
                            _colFee = "/*+ ordered */ evfeep.PAYMENTTYPE";
                            _joinFee += Cst.CrLf + padLeft + "inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
                        }
                        sqlFrom += "from       (/* VM-Begin" + _vmFee + " */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  REMOVED Settled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       @BUSINESSDATE as DTVAL, 1 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + "from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ecrmv.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @BUSINESSDATE)" + Cst.CrLf;

                        sqlFrom += padLeft + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-|  Unsettled " + _lblFee + "  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += padLeft + "/* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += padLeft + SQLCst.SELECT + _colFee + ", ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.UNIT," + Cst.CrLf;
                        sqlFrom += padLeft + "       ec.DTEVENT as DTVAL, 0 as ISREMOVED," + Cst.CrLf;
                        sqlFrom += padLeft + "       evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, ev.VALORISATION as VALORISATION" + Cst.CrLf;
                        sqlFrom += padLeft + "from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE) and (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE) and (ecrec.EVENTCLASS = 'REC') and (ecrec.DTEVENT <= @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += padLeft + "inner join dbo.EVENT ev on (ev.IDE = ec.IDE) and (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += padLeft + _joinFee + Cst.CrLf;
                        sqlFrom += padLeft + "where (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
                        #endregion  Frais - OPP
                    }
                    else
                    {
                        #region  Other flows
                        sqlFrom += "from       (/* VM-Begin */" + Cst.CrLf;
                        sqlFrom += "            /* +-+--+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  Flows  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+--+-+-+-+--+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, /*ec.DTEVENT*/ ec.DTEVENT as DTVAL, 0 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "            from dbo.EVENTCLASS ec" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ec.IDE) and (ev.IDSTACTIVATION = 'REGULAR')" + Cst.CrLf;
                        sqlFrom += "            where (ec.DTEVENT >= @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "              and (" + Cst.CrLf;
                        sqlFrom += "                    (" + Cst.CrLf;
                        sqlFrom += "                      (ev.EVENTTYPE = 'MGR' and ec.EVENTCLASS in ('FWR','G_K','VAL'))" + Cst.CrLf;
                        sqlFrom += "                       or " + Cst.CrLf;
                        sqlFrom += "                      (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','RMG','VMG','UMG') and ec.EVENTCLASS='VAL')" + Cst.CrLf;
                        sqlFrom += "                     )" + Cst.CrLf;
                        sqlFrom += "                     or" + Cst.CrLf;
                        sqlFrom += "                     (" + Cst.CrLf;
                        sqlFrom += "                       (ec.ISPAYMENT = 1) and (ec.EVENTCLASS='VAL')" + Cst.CrLf;
                        sqlFrom += "                       and" + Cst.CrLf;
                        sqlFrom += "                       ( (ev.EVENTTYPE = 'PRM') or (ev.EVENTCODE = 'TER' and ev.EVENTTYPE in ('CU1','CU2','SCU')) )" + Cst.CrLf;
                        sqlFrom += "                     )" + Cst.CrLf;
                        sqlFrom += "                   )" + Cst.CrLf;

                        sqlFrom += "            " + SQLCst.UNIONALL.Trim() + Cst.CrLf;

                        sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            /* +-+-+-+-+-|  REMOVED Flows  |+-+-+-+-+- */" + Cst.CrLf;
                        sqlFrom += "            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */" + Cst.CrLf;
                        sqlFrom += "            select /*+ ordered */ ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.VALORISATION, ev.UNIT, /*ecrmv.DTEVENT*/ @BUSINESSDATE as DTVAL, 1 as ISREMOVED" + Cst.CrLf;
                        sqlFrom += "            from dbo.EVENTCLASS ecrmv" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENTCLASS ec on (ec.IDE=ecrmv.IDE) and (ec.DTEVENT < @BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "            inner join dbo.EVENT ev on (ev.IDE=ecrmv.IDE) and (ev.IDSTACTIVATION = 'DEACTIV')" + Cst.CrLf;
                        sqlFrom += "            where (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT=@BUSINESSDATE)" + Cst.CrLf;
                        sqlFrom += "              and (" + Cst.CrLf;
                        sqlFrom += "                    (" + Cst.CrLf;
                        sqlFrom += "                      (ev.EVENTTYPE = 'MGR' and ec.EVENTCLASS in ('FWR','G_K','VAL'))" + Cst.CrLf;
                        sqlFrom += "                       or " + Cst.CrLf;
                        sqlFrom += "                      (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','RMG','VMG','UMG') and ec.EVENTCLASS='VAL')" + Cst.CrLf;
                        sqlFrom += "                     )" + Cst.CrLf;
                        sqlFrom += "                     or" + Cst.CrLf;
                        sqlFrom += "                     (" + Cst.CrLf;
                        sqlFrom += "                       (ec.ISPAYMENT = 1) and (ec.EVENTCLASS='VAL')" + Cst.CrLf;
                        sqlFrom += "                       and" + Cst.CrLf;
                        sqlFrom += "                       ( (ev.EVENTTYPE = 'PRM') or (ev.EVENTCODE = 'TER' and ev.EVENTTYPE in ('CU1','CU2','SCU')) )" + Cst.CrLf;
                        sqlFrom += "                     )" + Cst.CrLf;
                        sqlFrom += "                   )" + Cst.CrLf;
                        sqlFrom += "/* VM-End */) ev" + Cst.CrLf;
                        #endregion  Other flows
                    }
                    sqlFrom += "inner join dbo.TRADE tr on (tr.IDT=ev.IDT)" + Cst.CrLf;
                    sqlFrom += "inner join dbo.TRADEINSTRUMENT ti on (ti.IDT=tr.IDT)" + Cst.CrLf;
                    sqlFrom += "inner join dbo.INSTRUMENT i on (i.IDI=ti.IDI)" + Cst.CrLf;
                    sqlFrom += "inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.GPRODUCT in ('FX','OTC','SEC')) and (p.FUNGIBILITYMODE='NONE')" + Cst.CrLf;
                    sqlFrom += "inner join dbo.TRADESTSYS tsys on (tsys.IDT=tr.IDT) and (tsys.IDSTACTIVATION='REGULAR') and (tsys.IDSTBUSINESS in ('EXECUTED','INTERMED'))" + Cst.CrLf;
                    sqlFrom += "inner join dbo.BOOK b on (b.IDB=ti.IDB_BUYER) or (b.IDB=ti.IDB_SELLER)" + Cst.CrLf;
                    sqlFrom += "inner join dbo." + GetQuery_CBACTORv2(actorSuffix) + " cb on (cb.IDA = b.IDA)" + Cst.CrLf;
                    #endregion TradeFlows
                    break;
            }
            return sqlFrom;
        }
        #endregion GetSubQueryFlows_From
        #region GetSubQueryFlows_Where
        /// <summary>
        /// Construction d'une partie de requête : Where
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pConditionSwitch">Informations des conditions de construction de la requête</param>
        /// <returns>Partie de requête : Where</returns>
        /// EG 20140218 [19575][19666] REFACTORING QUERY
        /// PM 20140911 [20066][20185] Refactoring
        /// PM 20150323 [POC] Add TradeFlows 
        /// PM 20170213 [21916] Refactoring pConditionSwitch et ajout AllocNotFungibleFlows pour Commodity Spot
        private string GetSubQueryFlows_Where(string pCS, FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string sqlWhere = string.Empty;
            string actorSuffix = pConditionSwitch.DealerClearerSuffixe;

            switch (pFlowType)
            {
                case FlowTypeEnum.AllocNotFungibleFlows:
                    #region AllocNotFungibleFlows
                    sqlWhere = SQLCst.WHERE.Trim() + " (tr.DTBUSINESS <= @BUSINESSDATE)";

                    if (pConditionSwitch.IsFees)
                    {
                        sqlWhere += " and (ev.IDTAX is " + (pConditionSwitch.IsTax ? "not null" : "null") + ")";
                    }
                    sqlWhere += " and (b" + actorSuffix.ToLower() + ".IDA_ENTITY = @IDA_ENTITY) and ((ti.IDA_" + actorSuffix + " = ev.IDA_PAY) or (ti.IDA_" + actorSuffix + " = ev.IDA_REC))" + Cst.CrLf;
                    #endregion AllocNotFungibleFlows
                    break;
                case FlowTypeEnum.CashFlows:
                    #region CashFlows
                    // Les événements du jour :
                    // - Non désactivés (REGULAR)
                    // ou bien
                    // Les événements passés, annulés le jour même, pour extourner les flux
                    sqlWhere = SQLCst.WHERE.Trim() + " (tr.DTBUSINESS <= @BUSINESSDATE)";

                    if (pConditionSwitch.IsFees)
                    {
                        sqlWhere += " and (ev.IDTAX is " + (pConditionSwitch.IsTax ? "not null" : "null") + ")";
                    }
                    sqlWhere += " and (tr.IDA_ENTITY" + actorSuffix + " = @IDA_ENTITY) and ((tr.IDA_" + actorSuffix + " = ev.IDA_PAY) or (tr.IDA_" + actorSuffix + " = ev.IDA_REC))" + Cst.CrLf;
                    #endregion CashFlows
                    break;
                case FlowTypeEnum.Collateral:
                    #region Collateral
                    sqlWhere = SQLCst.WHERE + "(pc.DTBUSINESS <= @BUSINESSDATE) and ((pc.DTTERMINATION is null) or (pc.DTTERMINATION >=@BUSINESSDATE))" + Cst.CrLf;
                    sqlWhere += SQLCst.AND + "(pc.IDA_" + pConditionSwitch.PayerReceiverCodeInverted + "= @IDA_ENTITY) and " + Cst.CrLf;
                    sqlWhere += "(pc.ASSETCATEGORY " + (pConditionSwitch.IsAssetCash ? "=" : "!=") + " @ASSETCATEGORY_CASH) and " + Cst.CrLf;
                    sqlWhere += "(pc.IDA_" + pConditionSwitch.PayerReceiverCode + " in (" + Cst.CrLf +
                        GetQuery_CBACTOR(pCS, pConditionSwitch.PayerReceiver.Value, CBHierarchy.RolesCboMro) + "))" + Cst.CrLf;
                    #endregion Collateral
                    break;
                case FlowTypeEnum.Deposit:
                    #region Deposit
                    sqlWhere = SQLCst.WHERE + "(tr.DTBUSINESS = @BUSINESSDATE) and (ev.IDA_" + pConditionSwitch.PayerReceiverCodeInverted + " = @IDA_ENTITY) and " + Cst.CrLf;
                    sqlWhere += "(b.IDA in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, pConditionSwitch.PayerReceiver.Value, new RoleActor[1] { RoleActor.MARGINREQOFFICE }) + "))" + Cst.CrLf;
                    #endregion Deposit
                    break;
                case FlowTypeEnum.LastCashBalance:
                    #region LastCashBalance
                    sqlWhere = SQLCst.WHERE + "(tr.DTBUSINESS = @BUSINESSDATE_PREV) and " + Cst.CrLf;
                    sqlWhere += "(" + Cst.CrLf;
                    sqlWhere += "((ev.IDA_REC = @IDA_ENTITY) and (ev.IDA_PAY in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + ")))" + Cst.CrLf;
                    sqlWhere += " or " + Cst.CrLf;
                    sqlWhere += "((ev.IDA_PAY = @IDA_ENTITY) and (ev.IDA_REC in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + ")))" + Cst.CrLf;
                    sqlWhere += ")" + Cst.CrLf;
                    #endregion LastCashBalance
                    break;
                case FlowTypeEnum.OtherFlows:
                    #region OtherFlows
                    // Les événements du jour :
                    // - Non désactivés (REGULAR)
                    // ou bien
                    // Les événements passés, annulés le jour même, pour extourner les flux
                    sqlWhere = SQLCst.WHERE + "(tr.DTBUSINESS <= @BUSINESSDATE)";
                    sqlWhere += " and (tr.IDA_ENTITY" + actorSuffix + "=@IDA_ENTITY) and ((tr.IDA_" + actorSuffix + "=ev.IDA_PAY) or (tr.IDA_" + actorSuffix + "=ev.IDA_REC))" + Cst.CrLf;
                    //--------------------------------------------------------------------------------
                    // NB : Les montants RMG 
                    // - Il faut prendre le montant sur le trade clôturant uniquement
                    //   Car le résultat réalisé est disponible à la fois dans les événements du trade clôturé et du trade clôturant
                    //--------------------------------------------------------------------------------                            
                    //PL 20160524 sqlWhere += "and (ev.EVENTTYPE != @EVENTTYPE_RMG or pad.IDPADET is not null)" + Cst.CrLf;
                    #endregion OtherFlows
                    break;
                case FlowTypeEnum.Payment:
                    #region Payment
                    sqlWhere = SQLCst.WHERE + "(tr.DTTRADE = @BUSINESSDATE) and (ev.IDA_" + pConditionSwitch.PayerReceiverCodeInverted + " = @IDA_ENTITY) and " + Cst.CrLf;
                    sqlWhere += "(b.IDA in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + "))" + Cst.CrLf;
                    #endregion Payment
                    break;
                case FlowTypeEnum.SettlementPayment:
                    #region SettlementPayment
                    sqlWhere = SQLCst.WHERE + "(ev.IDA_" + pConditionSwitch.PayerReceiverCodeInverted + " = @IDA_ENTITY) and " + Cst.CrLf;
                    sqlWhere += "(b.IDA in (" + Cst.CrLf + GetQuery_CBACTOR(pCS, CBHierarchy.RolesCboMro) + "))" + Cst.CrLf;
                    #endregion SettlementPayment
                    break;
                case FlowTypeEnum.TradeFlows:
                    #region TradeFlows
                    sqlWhere = SQLCst.WHERE + "(tr.DTTRADE <= @BUSINESSDATE)" + Cst.CrLf;
                    if (pConditionSwitch.IsFees)
                    {
                        sqlWhere += " and (ev.IDTAX is " + (pConditionSwitch.IsTax ? "not null" : "null") + ")" + Cst.CrLf;
                    }
                    #endregion TradeFlows
                    break;
            }
            return sqlWhere;
        }
        #endregion GetSubQueryFlows_Where
        #region GetSubQueryFlows_GroupBy
        /// <summary>
        /// Construction d'une partie de requête : GroupBy
        /// </summary>
        /// <param name="pFlowType">Type de flux</param>
        /// <param name="pConditionSwitch">Informations des conditions de construction de la requête</param>
        /// <returns>Partie de requête : GroupBy</returns>
        /// EG 20140218 [19575][19666] REFACTORING QUERY
        /// PM 20150323 [POC] Add TradeFlows 
        /// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        /// FI 20170208 [22151][22152] Modify
        /// PM 20170213 [21916] Refactoring pConditionSwitch et ajout AllocNotFungibleFlows pour Commodity Spot
        /// FI 20170316 [22950] Modify
        private string GetSubQueryFlows_GroupBy(FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string sqlGroupBy = string.Empty;
            string actorSuffix = pConditionSwitch.DealerClearerSuffixe;

            switch (pFlowType)
            {
                case FlowTypeEnum.AllocNotFungibleFlows:
                    #region AllocNotFungibleFlows
                    sqlGroupBy = SQLCst.GROUPBY + "b" + actorSuffix.ToLower() + ".IDA, ti.IDB_" + actorSuffix + ", ev.DTVAL, ev.EVENTTYPE, ev.UNIT, p.IDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET ";
                    if (pConditionSwitch.IsFees)
                    {
                        sqlGroupBy += ", ev.EVENTCODE, ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE ";
                    }
                    sqlGroupBy += "," + Cst.CrLf + StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid);
                    #endregion AllocNotFungibleFlows
                    break;
                case FlowTypeEnum.CashFlows:
                    #region CashFlows
                    // RD 20170726 [23370][23432] Utiliser VW_ASSET à la place de VW_ASSET_WOETD et réfactoring de la requête  
                    //sqlGroupBy = SQLCst.GROUPBY.Trim() + " tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", ev.UNIT, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end, asset.ASSETCATEGORY, ev.DTVAL, ";  
                    sqlGroupBy = SQLCst.GROUPBY.Trim() + " tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", ev.UNIT," + Cst.CrLf;
                    sqlGroupBy += "case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end," + Cst.CrLf;
                    sqlGroupBy += "case when asset_ETD.IDDC is null then asset.IDASSET else 0 end," + Cst.CrLf;
                    sqlGroupBy += "asset.ASSETCATEGORY, ev.DTVAL, ";

                    if (pConditionSwitch.IsFees)
                    {
                        sqlGroupBy += "ev.EVENTCODE, ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE ";
                    }
                    else
                    {
                        sqlGroupBy += "ev.EVENTTYPE ";
                    }
                    if (m_CBHierarchy.cssCustodianEODValid.Count > 0) // FI 20170316 [22950] add test pour SQL Sqlserver (l'usage de d'une constante (ici 'unvalid') n'est pas toléré)
                        sqlGroupBy += "," + SQLColumnStatus(cs, "tr", string.Empty);
                    #endregion CashFlows
                    break;
                case FlowTypeEnum.OtherFlows:
                    #region OtherFlows
                    // RD 20170726 [23370][23432] Utiliser VW_ASSET à la place de VW_ASSET_WOETD et réfactoring de la requête
                    //sqlGroupBy = SQLCst.GROUPBY + "tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + ", ev.EVENTTYPE, asset.ASSETCATEGORY, asset.CATEGORY, asset.FUTVALUATIONMETHOD, asset.IDDC, case when asset.IDDC != 0 then 0 else asset.IDASSET end, tr.SIDE, ev.UNIT, ev.DTVAL ";
                    sqlGroupBy = SQLCst.GROUPBY + "tr.IDA_OWNER" + actorSuffix + ", tr.IDB_" + actorSuffix + @", ev.EVENTTYPE, asset.ASSETCATEGORY, asset_ETD.CATEGORY, asset_ETD.FUTVALUATIONMETHOD," + Cst.CrLf;
                    sqlGroupBy += "case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end," + Cst.CrLf;
                    sqlGroupBy += "case when asset_ETD.IDDC is null then asset.IDASSET else 0 end," + Cst.CrLf;
                    sqlGroupBy += "tr.SIDE, ev.UNIT, ev.DTVAL ";
                    if (m_CBHierarchy.cssCustodianEODValid.Count > 0) // FI 20170316 [22950] add test pour SQL Sqlserver (l'usage de d'une constante (ici 'unvalid') n'est pas toléré)
                        sqlGroupBy += "," + SQLColumnStatus(cs, "tr", string.Empty);
                    #endregion OtherFlows
                    break;
                case FlowTypeEnum.TradeFlows:
                    #region TradeFlows
                    sqlGroupBy = SQLCst.GROUPBY + "b.IDA, b.IDB, ev.DTVAL, ev.EVENTTYPE, ev.UNIT, p.IDENTIFIER, ti.ASSETCATEGORY, ti.IDASSET, " + Cst.CrLf;
                    if (pConditionSwitch.IsFees)
                    {
                        sqlGroupBy += "ev.EVENTCODE, ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE, " + Cst.CrLf;
                    }
                    sqlGroupBy +=  StrFunc.AppendFormat("case when ti.IDA_CSSCUSTODIAN is not null then {0} else '{1}' end", SQLColumnStatus(cs, "ti", string.Empty), StatusEnum.Valid);
                    #endregion TradeFlows
                    break;
            }
            return sqlGroupBy;
        }
        #endregion GetSubQueryFlows_GroupBy
        #endregion GetSubQueryFlows (Column / From / Where / GroupBy)

        #region GetSubQueryFlowsFrom_AllocCommon
        /// <summary>
        /// Construction de la partie "From" des requêtes flux issus de trade de type ALLOCATION
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Indique si le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pStep">OPP ou NOT_OPP</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <returns>Sous requête de la partie From de la requête finale</returns>
        /// PM 20170213 [21916] Ajout pour partage de code et refactoring pConditionSwitch
        private string GetSubQueryFlowsFrom_AllocCommon(string pCS, bool pIsClearerActorExist, CashFlowsStep pStep, FlowTypeEnum pFlowType)
        {
            string sqlSubQuery = string.Empty;
            //
            if (pStep == CashFlowsStep.OPP)
            {
                sqlSubQuery = GetSubQueryFlows(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Dealer, true, false, true)); //new Pair<string, bool>("DEALER", false));
                sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
                sqlSubQuery += GetSubQueryFlows(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Dealer, true, true, true)); //new Pair<string, bool>("DEALER", true));
                if (pIsClearerActorExist)
                {
                    sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
                    sqlSubQuery += GetSubQueryFlows(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Clearer, true, false, true)); //new Pair<string, bool>("CLEARER", false));
                    sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
                    sqlSubQuery += GetSubQueryFlows(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Clearer, true, true, true)); //new Pair<string, bool>("CLEARER", true));
                }
            }
            else if (pStep == CashFlowsStep.NOT_OPP)
            {
                sqlSubQuery = GetSubQueryFlows(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Dealer, false, false, true)); //"DEALER");
                if (pIsClearerActorExist)
                {
                    sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
                    sqlSubQuery += GetSubQueryFlows(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Clearer, false, false, true)); //"CLEARER");
                }
            }
            return sqlSubQuery;
        }
        #endregion GetSubQueryFlowsFrom_AllocCommon
        #region GetSubQueryTradesFrom_AllocCommon
        /// <summary>
        /// Construction des requêtes (Trades ALLOCATION) pour le type de flux pFlowType
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pStep">OPP ou NOT_OPP</param>
        /// <param name="pFlowType">Type de flux</param>
        /// <returns>Requête finale</returns>
        // PM 20170213 [21916] Ajout pour partage de code et refactoring pConditionSwitch
        private StrBuilder GetSubQueryTradesFrom_AllocCommon(string pCS, bool pIsClearerActorExist, CashFlowsStep pStep, FlowTypeEnum pFlowType)
        {
            StrBuilder sqlQuery = new StrBuilder();
            //
            if (pStep == CashFlowsStep.OPP)
            {
                #region Trades (OPP)
                sqlQuery += GetSubQueryTrade(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Dealer, true, false, true)); //new Pair<string, bool>("DEALER", false));
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetSubQueryTrade(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Dealer, true, true, true)); //new Pair<string, bool>("DEALER", true));
                if (pIsClearerActorExist)
                {
                    sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                    sqlQuery += GetSubQueryTrade(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Clearer, true, false, true)); //new Pair<string, bool>("CLEARER", false));
                    sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                    sqlQuery += GetSubQueryTrade(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Clearer, true, true, true)); //new Pair<string, bool>("CLEARER", true));
                }
                #endregion Trades (OPP)
            }
            else if (pStep == CashFlowsStep.NOT_OPP)
            {
                #region Trades (NOT_OPP)
                sqlQuery += GetSubQueryTrade(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Dealer, false, false, true)); //"DEALER");
                if (pIsClearerActorExist)
                {
                    sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                    sqlQuery += GetSubQueryTrade(pCS, pFlowType, new CBQueryCondition(TypeSideAllocation.Clearer, false, false, true)); //"CLEARER");
                }
                #endregion Trades (NOT_OPP)
            }
            return sqlQuery;
        }
        #endregion GetSubQueryTradesFrom_AllocCommon
        #region GetQueryFlows_AllocNotFungibleFlows
        /// <summary>
        /// Construction des requêtes (Flux) pour le type de flux : AllocNotFungibleFlows
        /// - Somme des OPP (en excluant les taxes) par couple Actor/Book, type (PAYMENTTYPE), devise et Derivative Contract
        /// - Somme des TAXES sur OPP               par couple Actor/Book, type (PAYMENTTYPE, caractériqtiques TAXE), devise et Derivative Contract
        /// - Somme des GAM                         par couple Actor/Book, type (EVENTTYPE), devise et Derivative Contract
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Indique si le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pStep">OPP ou NOT_OPP</param>
        /// <returns>Requête finale</returns>
        // PM 20170213 [21916] Ajout pour Commodity Spot
        private StrBuilder GetQueryFlows_AllocNotFungibleFlows(string pCS, bool pIsClearerActorExist, CashFlowsStep pStep)
        {
            StrBuilder sqlQuery = new StrBuilder();
            string sqlSubQuery = GetSubQueryFlowsFrom_AllocCommon(pCS, pIsClearerActorExist, pStep, FlowTypeEnum.AllocNotFungibleFlows);
            //
            sqlQuery += GetFlows_FinalCommonColumnSelect();
            sqlQuery += ", amt.DTVAL, amt.AMOUNTSUBTYPE, amt.PRODUCTIDENTIFIER, amt.ASSETCATEGORY, amt.IDASSET";
            sqlQuery += ", amt.PAYMENTTYPE, amt.IDTAX, amt.IDTAXDET, amt.TAXCOUNTRY, amt.TAXTYPE, amt.TAXRATE" + Cst.CrLf;
            //
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += "    ) amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);
            return sqlQuery;
        }
        #endregion GetQueryFlows_AllocNotFungibleFlows
        #region GetQueryTrades_AllocNotFungibleFlows
        /// <summary>
        /// Construction des requêtes (Trades) pour le type de flux : AllocNotFungibleFlows
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pStep">OPP ou NOT_OPP</param>
        /// <returns>Requêtes finales</returns>
        // PM 20170213 [21916] Ajout pour Commodity Spot
        private StrBuilder GetQueryTrades_AllocNotFungibleFlows(string pCS, bool pIsClearerActorExist, CashFlowsStep pStep)
        {
            StrBuilder sqlQuery = GetSubQueryTradesFrom_AllocCommon(pCS, pIsClearerActorExist, pStep, FlowTypeEnum.AllocNotFungibleFlows);
            //
            return sqlQuery;
        }
        #endregion GetQueryTrades_AllocNotFungibleFlows
        #region GetQueryFlows_CashFlows
        /// <summary>
        /// Construction des requêtes (Flux) pour le type de flux : CashFlows
        /// - Somme des OPP (en excluant les taxes) par couple Actor/Book, type (PAYMENTTYPE), devise et Derivative Contract
        /// - Somme des TAXES sur OPP               par couple Actor/Book, type (PAYMENTTYPE, caractériqtiques TAXE), devise et Derivative Contract
        /// - Somme des PRM, VMG et SCU             par couple Actor/Book, type (EVENTTYPE), devise et Derivative Contract
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pStep">1 = OPP et TAXES,  2 = PRM, VMG et SCU</param>
        /// <returns>Requête finale</returns>
        /// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        //PL 20160524 REM private StrBuilder GetQueryFlows_CashFlows(string pCS, bool pIsClearerActorExist, Nullable<int> pStep)
        private StrBuilder GetQueryFlows_CashFlows(string pCS, bool pIsClearerActorExist, CashFlowsStep pStep)
        {
            StrBuilder sqlQuery = new StrBuilder();
            // PM 20170213 [21916] Remplacement du bloc de construction de sqlSubQuery par l'appel à une méthode
            string sqlSubQuery = GetSubQueryFlowsFrom_AllocCommon(pCS, pIsClearerActorExist, pStep, FlowTypeEnum.CashFlows);
            //// Flows 
            ////PL 20160524 REM if (1 == pStep)
            //if (pStep == CashFlowsStep.OPP)
            //{
            //    sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("DEALER", false));
            //    sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
            //    sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("DEALER", true));
            //    if (pIsClearerActorExist)
            //    {
            //        sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
            //        sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("CLEARER", false));
            //        sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
            //        sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("CLEARER", true));
            //    }
            //}
            ////PL 20160524 REM else if (2 == pStep)
            //if (pStep == CashFlowsStep.NOT_OPP)
            //{
            //    sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.CashFlows, "DEALER");
            //    if (pIsClearerActorExist)
            //    {
            //        sqlSubQuery += SQLCst.UNIONALL_STAR100.Trim() + Cst.CrLf;
            //        sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.CashFlows, "CLEARER");
            //    }
            //}

            sqlQuery += GetFlows_FinalCommonColumnSelect();
            sqlQuery += ", amt.AMOUNTSUBTYPE, amt.PAYMENTTYPE, amt.IDTAX, amt.IDTAXDET, amt.TAXCOUNTRY, amt.TAXTYPE, amt.TAXRATE, amt.ASSETCATEGORY, amt.IDDC, amt.IDASSET, amt.DTVAL" + Cst.CrLf;
            sqlQuery += "from (" + Cst.CrLf;
            sqlQuery += "    " + sqlSubQuery.Replace(Cst.CrLf, Cst.CrLf + "    ");
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS) + ", amt.IDDC";
            return sqlQuery;
        }
        #endregion GetQueryFlows_CashFlows
        #region GetQueryTrades_CashFlows
        /// <summary>
        /// Construction des requêtes (Trades) pour le type de flux : CashFlows
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pStep">1 = OPP et TAXES,  2 = PRM, VMG et SCU</param>
        /// <returns>Requêtes finales</returns>
        private StrBuilder GetQueryTrades_CashFlows(string pCS, bool pIsClearerActorExist, CashFlowsStep pStep)
        {
            // PM 20170213 [21916] Remplacement du bloc de construction de la requête par l'appel à une méthode
            //StrBuilder sqlQuery = new StrBuilder();
            ////if (1 == pStep)
            //if (pStep == CashFlowsStep.OPP)
            //{
            //    #region Trades (OPP)
            //    sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("DEALER", false));
            //    sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            //    sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("DEALER", true));
            //    if (pIsClearerActorExist)
            //    {
            //        sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            //        sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("CLEARER", false));
            //        sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            //        sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.CashFlows, new Pair<string, bool>("CLEARER", true));
            //    }
            //    #endregion Trades (OPP)
            //}
            ////else if (2 == pStep)
            //else if (pStep == CashFlowsStep.NOT_OPP)
            //{
            //    #region Trades (PRM / VMG / SCU)
            //    sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.CashFlows, "DEALER");
            //    if (pIsClearerActorExist)
            //    {
            //        sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            //        sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.CashFlows, "CLEARER");
            //    }
            //    #endregion Trades (PRM / VMG / SCU)
            //}
            StrBuilder sqlQuery = GetSubQueryTradesFrom_AllocCommon(pCS, pIsClearerActorExist, pStep, FlowTypeEnum.CashFlows);
            return sqlQuery;
        }
        #endregion GetQueryFlows_CashFlows
        #region GetQueryFlows_Collateral
        /// <summary>
        /// Construction de la requête (Flux) pour le type de flux : Collateral
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requête finale</returns>
        /// FI 20160530 [21885] Modify 
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryFlows_Collateral(string pCS, bool pIsClearerActorExist)
        {
            // Flows
            string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.Collateral, new CBQueryCondition(FlowTypeEnum.Collateral, PayerReceiverEnum.Payer, false)); //new Pair<PayerReceiverEnum, bool>(PayerReceiverEnum.Payer, false));
            sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.Collateral, new CBQueryCondition(FlowTypeEnum.Collateral, PayerReceiverEnum.Payer, true)); //new Pair<PayerReceiverEnum, bool>(PayerReceiverEnum.Payer, true));
            if (pIsClearerActorExist)
            {
                sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.Collateral, new CBQueryCondition(FlowTypeEnum.Collateral, PayerReceiverEnum.Receiver, false)); //new Pair<PayerReceiverEnum, bool>(PayerReceiverEnum.Receiver, false));
                sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.Collateral, new CBQueryCondition(FlowTypeEnum.Collateral, PayerReceiverEnum.Receiver, true)); //new Pair<PayerReceiverEnum, bool>(PayerReceiverEnum.Receiver, true));
            }
            // FI 20160530 [21885] Add IDPOSCOLLATERAL et IDPOSCOLLATERALVAL
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect();
            sqlQuery += ", amt.IDPOSCOLLATERAL, amt.IDPOSCOLLATERALVAL, amt.IDA_CSS, amt.DTBUSINESS, amt.ISVALORISED, amt.QTYVAL, amt.ASSETCATEGORY, amt.IDASSET, amt.HAIRCUTFORCED" + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);

            return sqlQuery;
        }
        #endregion GetQueryFlows_Collateral
        #region GetQueryFlows_Deposit
        /// <summary>
        /// Construction de la requête (Flux) pour le type de flux : Déposit
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <param name="pDataParameters">Paramètres de la requête</param>
        /// <returns>Requête finale</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        /// FI 20170316 [22950] Modify
        private StrBuilder GetQueryFlows_Deposit(string pCS, bool pIsClearerActorExist, DataParameters pDataParameters)
        {
            bool isFilterCSS = pDataParameters.Contains("IDA_CSS");
            //--------------------------------------------------------------------------------
            // Actor Dealer est Payeur du Déposit à l'Entity
            //--------------------------------------------------------------------------------
            string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.Deposit, new CBQueryCondition(FlowTypeEnum.Deposit, PayerReceiverEnum.Payer, isFilterCSS)); //new Pair<PayerReceiverEnum, bool>(PayerReceiverEnum.Payer, isFilterCSS));
            if (pIsClearerActorExist)
            {
                //--------------------------------------------------------------------------------
                // Actor Clearer est Receveur du Déposit de l'Entity
                //--------------------------------------------------------------------------------
                sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.Deposit, new CBQueryCondition(FlowTypeEnum.Deposit, PayerReceiverEnum.Receiver, isFilterCSS)); //new Pair<PayerReceiverEnum, bool>(PayerReceiverEnum.Receiver, isFilterCSS));
            }

            // FI 20170316 [22950] add column DTBUSINESS
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect() + ", amt.IDT, amt.T_IDENTIFIER, amt.DTBUSINESS, amt.IDA_CSS, amt.DTSYS" + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);

            return sqlQuery;
        }
        #endregion GetQueryFlows_Deposit
        #region GetQueryFlows_LastCashBalance
        /// <summary>
        /// Construction de la requête (Flux) pour le type de flux : LastCashBalance
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <returns>Requête finale</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        /// FI 20170316 [22950] Modify
        private StrBuilder GetQueryFlows_LastCashBalance(string pCS)
        {
            string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.LastCashBalance, new CBQueryCondition()); //string.Empty);

            // FI 20170316 [22950] Add DTBUSINESS
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect() + ", amt.DTEVENT, amt.AMOUNTSUBTYPE, amt.IDT, amt.T_IDENTIFIER, amt.DTBUSINESS" + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);
            return sqlQuery;
        }
        #endregion GetQueryFlows_LastCashBalance
        #region GetQueryFlows_OtherFlows
        /// <summary>
        /// Construction des requêtes (Flux) pour le type de flux : OtherFlows
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requêtes finales</returns>
        /// PM 20150616 [21124] Gestion EventClass VAL pour les flux des trades
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryFlows_OtherFlows(string pCS, bool pIsClearerActorExist)
        {
            // Flows
            string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.OtherFlows, new CBQueryCondition(TypeSideAllocation.Dealer, false, false, true)); //"DEALER");
            if (pIsClearerActorExist)
            {
                sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.OtherFlows, new CBQueryCondition(TypeSideAllocation.Clearer, false, false, true)); //"CLEARER");
            }

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect();
            sqlQuery += ", amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.CATEGORY, amt.FUTVALUATIONMETHOD, amt.IDDC, amt.IDASSET, amt.SIDE, amt.DTVAL" + Cst.CrLf;
            //
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);

            return sqlQuery;
        }
        #endregion GetQueryFlows_OtherFlows
        #region GetQueryTrades_OtherFlows
        /// <summary>
        /// Construction des requêtes (Trades) pour le type de flux : OtherFlows
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requête finale</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryTrades_OtherFlows(string pCS, bool pIsClearerActorExist)
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.OtherFlows, new CBQueryCondition(TypeSideAllocation.Dealer, false, false, false)); //"DEALER");
            if (pIsClearerActorExist)
            {
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.OtherFlows, new CBQueryCondition(TypeSideAllocation.Clearer, false, false, false)); //"CLEARER");
            }
            return sqlQuery;
        }
        #endregion GetQueryTrades_OtherFlows
        #region GetQueryFlows_Payment
        /// <summary>
        /// Construction des requêtes (Flux) pour le type de flux : Payment
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requêtes finales</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryFlows_Payment(string pCS, bool pIsClearerActorExist)
        {
            string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.Payment, new CBQueryCondition(PayerReceiverEnum.Payer)); //PayerReceiverEnum.Payer);
            sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.Payment, new CBQueryCondition(PayerReceiverEnum.Receiver)); //PayerReceiverEnum.Receiver);
            //}

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect() + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);

            return sqlQuery;
        }
        #endregion GetQueryFlows_Payment
        #region GetQueryTrades_Payment
        /// <summary>
        /// Construction des requêtes (Trades) pour le type de flux : Payment
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requête finale</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryTrades_Payment(string pCS, bool pIsClearerActorExist)
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.Payment, new CBQueryCondition(PayerReceiverEnum.Payer)); //PayerReceiverEnum.Payer);
            // PM/CC 20141031 Suppression de la condition "pIsClearerActorExist" : 
            // Les versements doivent être considérés que l'on soit payer ou receiver
            // qu'il y ait ou non des acteurs "Clearer" dans la hiérarchie des acteurs
            //if (pIsClearerActorExist)
            //{
            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.Payment, new CBQueryCondition(PayerReceiverEnum.Receiver)); //PayerReceiverEnum.Receiver);
            //}
            return sqlQuery;
        }
        #endregion GetQueryTrades_Payment
        #region GetQueryFlows_SettlementPayment
        /// <summary>
        /// Construction des requêtes (Flux) pour le type de flux : Payment en Settlement date
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requêtes finales</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryFlows_SettlementPayment(string pCS, bool pIsClearerActorExist)
        {
            string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.SettlementPayment, new CBQueryCondition(PayerReceiverEnum.Payer)); //PayerReceiverEnum.Payer);
            sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.SettlementPayment, new CBQueryCondition(PayerReceiverEnum.Receiver)); //PayerReceiverEnum.Receiver);

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect() + ", amt.DTEVENT, amt.ISFORWARD" + Cst.CrLf;
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);

            return sqlQuery;
        }
        #endregion GetQueryFlows_SettlemenPayment
        #region GetQueryTrades_SettlemenPayment
        /// <summary>
        /// Construction des requêtes (Trades) pour le type de flux : Payment en Settlemen date
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pIsClearerActorExist">Le rôle Clearer est présent dans la hiérarchie</param>
        /// <returns>Requête finale</returns>
        /// PM 20140911 [20066][20185] New
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryTrades_SettlemenPayment(string pCS, bool pIsClearerActorExist)
        {
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.SettlementPayment, new CBQueryCondition(PayerReceiverEnum.Payer)); //PayerReceiverEnum.Payer);
            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.SettlementPayment, new CBQueryCondition(PayerReceiverEnum.Receiver)); //PayerReceiverEnum.Receiver);
            return sqlQuery;
        }
        #endregion GetQueryTrades_SettlemenPayment
        #region GetQueryFlows_TradeFlows
        /// <summary>
        /// Construction des requêtes (Flux) pour le type de flux : TradeFlows
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pStep">OPP ou NOT_OPP</param>
        /// <returns>Requêtes finales</returns>
        /// PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryFlows_TradeFlows(string pCS, CashFlowsStep pStep)
        {
            // Flows
            //string sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.TradeFlows, "DEALER");
            string sqlSubQuery = string.Empty;
            if (pStep == CashFlowsStep.OPP)
            {
                sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.TradeFlows, new CBQueryCondition(TypeSideAllocation.Dealer, true, false, false)); //new Pair<string, bool>("DEALER", false));
                sqlSubQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlSubQuery += GetSubQueryFlows(pCS, FlowTypeEnum.TradeFlows, new CBQueryCondition(TypeSideAllocation.Dealer, true, true, false)); //new Pair<string, bool>("DEALER", true));
            }
            else
            {
                sqlSubQuery = GetSubQueryFlows(pCS, FlowTypeEnum.TradeFlows, new CBQueryCondition(TypeSideAllocation.Dealer, false, false, false)); //"DEALER");
            }

            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += GetFlows_FinalCommonColumnSelect();
            sqlQuery += ", amt.AMOUNTSUBTYPE, amt.PRODUCTIDENTIFIER, amt.ASSETCATEGORY, amt.IDASSET, amt.PAYMENTTYPE, amt.IDTAX, amt.IDTAXDET, amt.TAXCOUNTRY, amt.TAXTYPE, amt.TAXRATE, amt.DTVAL" + Cst.CrLf;
            //
            sqlQuery += SQLCst.X_FROM + "(" + Cst.CrLf;
            sqlQuery += sqlSubQuery + Cst.CrLf;
            sqlQuery += ") amt" + Cst.CrLf;
            sqlQuery += GetFlows_FinalJoinBook(pCS);

            return sqlQuery;
        }
        #endregion GetQueryFlows_TradeFlows
        #region GetQueryTrades_TradeFlows
        /// <summary>
        /// Construction des requêtes (Trades) pour le type de flux : TradeFlows
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pStep">OPP ou NOT_OPP</param>
        /// <returns>Requête finale</returns>
        ///  PM 20170213 [21916] Refactoring pConditionSwitch
        private StrBuilder GetQueryTrades_TradeFlows(string pCS, CashFlowsStep pStep)
        {
            StrBuilder sqlQuery = new StrBuilder();
            if (pStep == CashFlowsStep.OPP)
            {
                sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.TradeFlows, new CBQueryCondition(TypeSideAllocation.Dealer, true, false, false)); //new Pair<string, bool>("DEALER", false));
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.TradeFlows, new CBQueryCondition(TypeSideAllocation.Dealer, true, true, false)); //new Pair<string, bool>("DEALER", true));
            }
            else
            {
                sqlQuery += GetSubQueryTrade(pCS, FlowTypeEnum.TradeFlows, new CBQueryCondition(TypeSideAllocation.Dealer, false, false, false)); //"DEALER");
            }
            return sqlQuery;
        }
        #endregion GetQueryTrades_TradeFlows

        #region GetFlows_FinalCommonColumnSelect
        /// <summary>
        /// Colonnes communes à toutes les requêtes finales
        /// </summary>
        /// <returns></returns>
        /// FI 20170208 [22151][22152] Modify
        private string GetFlows_FinalCommonColumnSelect()
        {
            // FI 20170208 [22151][22152] add STATUS column
            string sqlSelect = SQLCst.SELECT + "amt.IDA, amt.IDB, b.IDENTIFIER as B_IDENTIFIER, amt.AMOUNT, amt.IDC, amt.STATUS";
            return sqlSelect;
        }
        #endregion GetFlows_FinalCommonColumnSelect
        #region GetFlows_FinalJoinBook
        /// <summary>
        /// Jointure sur BOOK et tri communs à toutes les requêtes finales
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <returns></returns>
        private string GetFlows_FinalJoinBook(string pCS)
        {
            // FI 20180827 [24141] remplacement de DataEnum.EnabledOnly par DataEnum.All
            string join = OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.BOOK, SQLJoinTypeEnum.Inner, "amt.IDB", "b", DataEnum.All);
            join += SQLCst.ORDERBY + "amt.IDA, amt.IDB, amt.IDC";
            return join;
        }
        #endregion GetFlows_FinalJoinBook

        #region deprecated_GetFlows_CommonAmount
        ///// <summary>
        ///// Partie de requête pour évaluation du montant de flux, utilisée par : CashFlows, Deposit, Payment et OtherFlows
        ///// CashFlows et OtherFlows : 
        ///// -------------------------
        ///// Les événements du jour (non annulés, ou bien annulés à une date ultérieure)
        ///// - les flux sont pris dans le même sens d'origine dans la table Event
        ///// Les événements passés (annulés le jour même)
        ///// - les flux sont extournés, donc pris dans le sens inverse au sens d'origine dans la table Event
        ///// 
        ///// Deposit : 
        ///// ---------
        /////          Dealer	    Entité
        /////          --------   --------
        ///// Deposit  Payeur	    Receveur
        /////
        /////          Clearer	Entité
        /////          --------   --------
        ///// Deposit	 Receveur	Payeur
        ///// 
        ///// Payment : 
        ///// ---------
        /////             Dealer	    Entité
        /////             --------    --------
        ///// Versement	Payeur	    Receveur
        ///// Retrait	    Receveur	Payeur
        /////
        /////             Clearer	    Entité
        /////             --------    --------
        /////Versement	Receveur	Payeur
        /////Retrait	    Payeur	    Receveur
        ///// </summary>
        ///// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        ///// <returns></returns>
        ///// EG 20140218 [19575][19666] REFACTORING QUERY
        ///// PL 20160524 Padding
        ///// FI 20170208 [22151][22152] Modify
        //private string deprecated_GetFlows_CommonAmount<T>(T pConditionSwitch)
        //{
        //    int spaceLeft = 6;
        //    string colAmount = string.Empty;
        //    if (pConditionSwitch is PayerReceiverEnum)
        //    {
        //        // Deposit et Payment 
        //        Nullable<PayerReceiverEnum> payerReceiver = pConditionSwitch as Nullable<PayerReceiverEnum>;
                
        //        int multiplier = (PayerReceiverEnum.Payer == payerReceiver.Value ? -1 : 1);
        //        colAmount += "       (" + multiplier + " * ev.VALORISATION) as AMOUNT";
        //        // FI 20170208 [22151][22152] add column STATUS
        //        colAmount += "," + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid);
        //    }
        //    else if (pConditionSwitch is Pair<PayerReceiverEnum, Boolean>) // FI 20170208 [22151][22152] add if
        //    {
        //        // FI 20170208 [22151][22152]  code spécifique aux requêtes de chargement de deposit
        //        Pair<PayerReceiverEnum, Boolean> condition = pConditionSwitch as Pair<PayerReceiverEnum, Boolean>;
                
        //        int multiplier = multiplier = (PayerReceiverEnum.Payer == condition.First ? -1 : 1);
        //        colAmount += "       (" + multiplier + " * ev.VALORISATION) as AMOUNT";
        //        if ((condition.Second))
        //        {
        //            // on passe ici lors de la lecture d'un deposit à une date antérieure (nécessairement valid)
        //            colAmount += "," + StrFunc.AppendFormat("'{0}' as STATUS", StatusEnum.Valid);
        //        }
        //        else
        //        {
        //            colAmount += "," + SQLColumnStatus(cs, "ti", "STATUS");
        //        }
        //    }
        //    else if (pConditionSwitch is string)
        //    {
        //        // CashFlows et OtherFlows
        //        string actorSuffix = GetDealerClearerTag(pConditionSwitch);
        //        colAmount = " ".PadLeft(spaceLeft) + " sum(case tr.IDA_" + actorSuffix;
        //        spaceLeft = colAmount.Length;
        //        //PL 20160524
        //        //colAmount += " when ev.IDA_PAY then case when (ev.DTEVENT = @BUSINESSDATE) then -1 else  1 end" + Cst.CrLf;
        //        //colAmount += " ".PadLeft(spaceLeft) + " when ev.IDA_REC then case when (ev.DTEVENT = @BUSINESSDATE) then 1  else -1 end" + Cst.CrLf;
        //        colAmount += " when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end" + Cst.CrLf;
        //        colAmount += " ".PadLeft(spaceLeft) + " when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end" + Cst.CrLf;
        //        colAmount += " ".PadLeft(spaceLeft) + " end * ev.VALORISATION) as AMOUNT,";
        //        // FI 20170208 [22151][22152] add column STATUS
        //        colAmount += SQLColumnStatus(cs, "tr", "STATUS");
        //    }
        //    return colAmount;
        //}
        #endregion deprecated_GetFlows_CommonAmount
        
        #region GetFlows_CommonAmount
        /// <summary>
        /// Partie de requête pour évaluation du montant de flux, utilisée par : CashFlows, Deposit, Payment et OtherFlows
        /// CashFlows et OtherFlows : 
        /// -------------------------
        /// Les événements du jour (non annulés, ou bien annulés à une date ultérieure)
        /// - les flux sont pris dans le même sens d'origine dans la table Event
        /// Les événements passés (annulés le jour même)
        /// - les flux sont extournés, donc pris dans le sens inverse au sens d'origine dans la table Event
        /// 
        /// Deposit : 
        /// ---------
        ///          Dealer	    Entité
        ///          --------   --------
        /// Deposit  Payeur	    Receveur
        ///
        ///          Clearer	Entité
        ///          --------   --------
        /// Deposit	 Receveur	Payeur
        /// 
        /// Payment : 
        /// ---------
        ///             Dealer	    Entité
        ///             --------    --------
        /// Versement	Payeur	    Receveur
        /// Retrait	    Receveur	Payeur
        ///
        ///             Clearer	    Entité
        ///             --------    --------
        ///Versement	Receveur	Payeur
        ///Retrait	    Payeur	    Receveur
        /// </summary>
        /// <param name="pConditionSwitch">voir les fonctions appelantes</param>
        /// <returns></returns>
        /// EG 20140218 [19575][19666] REFACTORING QUERY
        /// PL 20160524 Padding
        /// FI 20170208 [22151][22152] Modify
        /// PM 20170213 [21916] Refactoring pConditionSwitch et ajout AllocNotFungibleFlows
        private string GetFlows_CommonAmount(FlowTypeEnum pFlowType, CBQueryCondition pConditionSwitch)
        {
            string padLeft = string.Empty;
            string colAmount = string.Empty;
            switch (pFlowType)
            {
                case FlowTypeEnum.AllocNotFungibleFlows:
                    colAmount = " sum(case ti.IDA_" + pConditionSwitch.DealerClearerSuffixe;
                    padLeft = new string(' ', 7 + colAmount.Length);
                    colAmount += " when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end" + Cst.CrLf;
                    colAmount += padLeft + " when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end" + Cst.CrLf;
                    colAmount += padLeft + " end * ev.VALORISATION) as AMOUNT";
                    break;
                case FlowTypeEnum.CashFlows:
                case FlowTypeEnum.OtherFlows:
                    colAmount = " sum(case tr.IDA_" + pConditionSwitch.DealerClearerSuffixe;
                    padLeft = new string(' ', 7 + colAmount.Length);
                    colAmount += " when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end" + Cst.CrLf;
                    colAmount += padLeft + " when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end" + Cst.CrLf;
                    colAmount += padLeft + " end * ev.VALORISATION) as AMOUNT";
                    break;
                case FlowTypeEnum.Deposit:
                case FlowTypeEnum.Payment:
                case FlowTypeEnum.SettlementPayment:
                    colAmount += "       (" + pConditionSwitch.SignPositifIfReceiver + " * ev.VALORISATION) as AMOUNT";
                    break;
                case FlowTypeEnum.TradeFlows:
                case FlowTypeEnum.Collateral:
                case FlowTypeEnum.LastCashBalance:
                    // Noop
                    break;
            }
            return colAmount;
        }
        #endregion GetFlows_CommonAmount

        #region GetQuery_CBACTOR // RoleActor[]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRoleActor"></param>
        /// <returns></returns>
        // EG 20131114 [19186] NEW
        private string GetQuery_CBACTOR(string pCS, RoleActor[] pRoleActor)
        {
            return GetQuery_CBACTOR(pCS, pRoleActor, false);
        }
        // EG 20140226 [19575][19666]
        // EG 20181119 PERF Correction post RC (Step 2)
        private string GetQuery_CBACTOR(string pCS, RoleActor[] pRoleActor, bool pIsDistinct)
        {
            string sqlQuery = (pIsDistinct ? SQLCst.SELECT_DISTINCT : SQLCst.SELECT) + "cba.IDA" + Cst.CrLf;
            //          "inner join ( "        NB: espaces pour alignement de la query avec "inner join ( "
            sqlQuery += "             from dbo." + m_CBHierarchy.tblCBACTOR_Work.First + " cba" + Cst.CrLf;
            if (ArrFunc.IsFilled(pRoleActor))
            {
                sqlQuery += "             " + SQLCst.INNERJOIN_DBO.Trim() + Cst.OTCml_TBL.ACTORROLE + " ar on (ar.IDA = cba.IDA) and (";
                sqlQuery += DataHelper.SQLColumnIn(pCS, "ar.IDROLEACTOR", pRoleActor, TypeData.TypeDataEnum.@string, false, true) + ")";
            }
            return sqlQuery;
        }
        // EG 20131114 [19186] NEW
        // EG 20140219 [19575][19666] Add Parameter Distinct
        private string GetQuery_CBACTORv1_DEPRECATED(string pCS, string pActorSuffix)
        {
            string sqlQuery = " ";
            if ("DEALER" == pActorSuffix)
            {
                sqlQuery += GetQuery_CBACTOR(pCS, new RoleActor[] { });
                //          "inner join ( "        NB: espaces pour alignement de la query avec "inner join ( "
                sqlQuery += "             " + DataHelper.SQLGetExcept(pCS) + Cst.CrLf;
                sqlQuery += "             ";
            }
            //PM 20140403 [Recette v4.0] Toujours faire un distinct côté Clearer/CSS dans le cas où il leur serait donnée à la fois le rôle Clearer et le rôle ClearingCompartiment
            //sqlQuery += GetQuery_CBACTOR(pCS, CBHierarchy.RolesClearer, "DEALER" == pActorSuffix);
            sqlQuery += GetQuery_CBACTOR(pCS, CBHierarchy.RolesClearer, true);
            return sqlQuery;
        }
        // EG 20181119 PERF Correction post RC (Step 2)
        private string GetQuery_CBACTORv2(string pActorSuffix)
        {
            string tblWork = string.Empty;
            if ("DEALER" == pActorSuffix)
                tblWork = m_CBHierarchy.tblCBACTOR_NOTCLEARER_Work.First;
            else
                tblWork = m_CBHierarchy.tblCBACTOR_CLEARER_Work.First;
            return tblWork;
        }
        // EG 20131114 [19186] NEW
        private string GetQuery_CBACTOR(string pCS, PayerReceiverEnum pPayerReceiver, RoleActor[] pRoleActor)
        {
            string sqlQuery = GetQuery_CBACTOR(pCS, pRoleActor);
            sqlQuery += ((PayerReceiverEnum.Payer == pPayerReceiver) ? DataHelper.SQLGetExcept(pCS) : "intersect") + Cst.CrLf;
            sqlQuery += GetQuery_CBACTOR(pCS, CBHierarchy.RolesClearer);
            return sqlQuery;
        }
        #endregion GetQuery_CBACTOR

        #region deprecated_GetDealerClearerTag
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="pConditionSwitch"></param>
        ///// <returns></returns>
        ///// EG 20131114 [19186] NEW
        ///// FI 20161021 [22152] Modify (Method static)
        ///// PM 20170213 [21916] La méthode devient inutile
        //private static string deprecated_GetDealerClearerTag<T>(T pConditionSwitch)
        //{
        //    string tag = string.Empty;
        //    if (pConditionSwitch is Pair<string, bool>)
        //    {
        //        tag = (pConditionSwitch as Pair<string, bool>).First;
        //    }
        //    else if (pConditionSwitch is string)
        //    {
        //        tag = pConditionSwitch as string;
        //    }
        //    return tag.ToUpper();
        //}
        #endregion deprecated_GetDealerClearerTag
        #region deprecated_GetPayerReceiverTag
        //// EG 20131114 [19186] NEW
        //// PM 20170213 [21916] La méthode devient inutile
        //private string deprecated_GetPayerReceiverTag<T>(T pConditionSwitch)
        //{
        //    return GetPayerReceiverTag(pConditionSwitch, false);
        //}
        //// PM 20170213 [21916] La méthode devient inutile
        //private string deprecated_GetPayerReceiverTag<T>(T pConditionSwitch, bool pIsReverse)
        //{
        //    string tag = string.Empty;
        //    Nullable<PayerReceiverEnum> payerReceiver = null;
        //    if (pConditionSwitch is Pair<PayerReceiverEnum, bool>)
        //        payerReceiver = (pConditionSwitch as Pair<PayerReceiverEnum, bool>).First;
        //    else if (pConditionSwitch is PayerReceiverEnum)
        //        payerReceiver = pConditionSwitch as Nullable<PayerReceiverEnum>;

        //    if (payerReceiver.HasValue)
        //    {
        //        if (pIsReverse)
        //            tag = (PayerReceiverEnum.Payer == payerReceiver ? "REC" : "PAY");
        //        else
        //            tag = (PayerReceiverEnum.Payer == payerReceiver ? "PAY" : "REC");
        //    }
        //    return tag.ToUpper();
        //}
        #endregion deprecated_GetPayerReceiverTag

        /// <summary>
        ///  Retourne Expression SQL qui donne le statut d'un flux ou d'un trade
        ///  <para> Le statut est Unvalid si le traitement EOD sur le css/Custodian est Unvalid</para>
        ///  <para> Le statut est Unvalid si le traitement EOD sur le css/Custodian est Valid</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAliasTradeInstrument"></param>
        /// <param name="pAliasColumn"></param>
        /// <returns></returns>
        /// FI 20170208 [22151][22152] Add
        private string SQLColumnStatus(string pCS, string pAliasTradeInstrument, string pAliasColumn)
        {
            string ret = string.Empty;

            int[] idCssValid = m_CBHierarchy.cssCustodianEODValid.Select(x => x.OTCmlId).ToArray();
            if (ArrFunc.IsFilled(idCssValid))
            {
                ret = StrFunc.AppendFormat("case when {0}.IDA_CSSCUSTODIAN in({1}) then '{2}' else '{3}' end",
                    pAliasTradeInstrument,
                    DataHelper.SQLCollectionToSqlList(pCS, idCssValid, TypeData.TypeDataEnum.@int),
                    StatusEnum.Valid, StatusEnum.Unvalid
                    );
            }
            else
            {
                ret = StrFunc.AppendFormat("'{0}'", StatusEnum.Unvalid);
            }

            if (StrFunc.IsFilled(pAliasColumn))
                ret += SQLCst.AS + pAliasColumn;

            return ret;
        }
    }

    /// <summary>
    /// Condition de construction des requêtes de recherches des flux et trades
    /// </summary>
    /// PM 20170213 [21916] New
    internal class CBQueryCondition
    {
        #region Members
        private TypeSideAllocation? m_DealerClearer = null;
        private bool m_IsFees = false;
        private bool m_IsTax = false;
        private bool m_IsJoinAsset = false;
        private PayerReceiverEnum? m_PayerReceiver = null;
        private bool m_IsAssetCash = false;
        private bool m_IsFilterCss = false;
        #endregion Members

        #region Accessors
        public TypeSideAllocation? DealerClearer
        {
            get { return m_DealerClearer; }
        }
        public string DealerClearerSuffixe
        {
            get { return m_DealerClearer.HasValue ? m_DealerClearer.ToString().ToUpper() : string.Empty; }
        }
        public bool IsFees
        {
            get { return m_IsFees; }
        }
        public bool IsTax
        {
            get { return m_IsTax; }
        }
        public bool IsJoinAsset
        {
            get { return m_IsJoinAsset; }
        }

        public bool IsPayer
        {
            get { return m_PayerReceiver.HasValue ? (PayerReceiverEnum.Payer == m_PayerReceiver ? true : false) : false; }
        }

        public bool IsReceiver
        {
            get { return m_PayerReceiver.HasValue ? (PayerReceiverEnum.Receiver == m_PayerReceiver ? true : false) : false; }
        }

        public PayerReceiverEnum? PayerReceiver
        {
            get { return m_PayerReceiver; }
        }

        public string PayerReceiverCode
        {
            get { return m_PayerReceiver.HasValue ? (PayerReceiverEnum.Payer == m_PayerReceiver ? "PAY" : "REC") : string.Empty; }
        }

        public string PayerReceiverCodeInverted
        {
            get { return m_PayerReceiver.HasValue ? (PayerReceiverEnum.Payer == m_PayerReceiver ? "REC" : "PAY") : string.Empty; }
        }

        public int SignPositifIfPayer
        {
            get { return m_PayerReceiver.HasValue ? (PayerReceiverEnum.Payer == m_PayerReceiver ? 1 : -1) : 0; }
        }

        public int SignPositifIfReceiver
        {
            get { return m_PayerReceiver.HasValue ? (PayerReceiverEnum.Payer == m_PayerReceiver ? -1 : 1) : 0; }
        }

        /// <summary>
        /// Indicateur pour les flux Collateral lorsqu'ils ne concernent que les Asset de type CASH
        /// </summary>
        public bool IsAssetCash
        {
            get { return m_IsAssetCash; }
        }

        /// <summary>
        /// Indicateur pour les flux Deposit lorsqu'ils ne concernent qu'un CSS
        /// </summary>
        public bool IsFilterCss
        {
            get { return m_IsFilterCss; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur vide pour les requêtes portant sur les flux des cashs balance précédants
        /// </summary>
        public CBQueryCondition() {}

        /// <summary>
        /// Constructeur pour les requêtes portant sur les flux des Trades
        /// </summary>
        /// <param name="pDealerClearer"></param>
        /// <param name="pIsFees"></param>
        /// <param name="pIsTax"></param>
        /// <param name="pIsJoinAsset"></param>
        public CBQueryCondition(TypeSideAllocation pDealerClearer, bool pIsFees, bool pIsTax, bool pIsJoinAsset)
        {
            m_DealerClearer = pDealerClearer;
            m_IsFees = pIsFees;
            m_IsTax = pIsTax;
            m_IsJoinAsset = pIsJoinAsset;
        }

        /// <summary>
        /// Constructeur pour les requêtes portant sur les flux des Payments
        /// </summary>
        /// <param name="pPayerReceiver"></param>
        public CBQueryCondition(PayerReceiverEnum pPayerReceiver)
        {
            m_PayerReceiver = pPayerReceiver;
        }

        /// <summary>
        /// Constructeur pour les requêtes portant sur les flux des Collaterals ou des Deposits
        /// </summary>
        /// <param name="pFlowType"></param>
        /// <param name="pPayerReceiver"></param>
        /// <param name="pIsAssetCashOrIsFilterCss"></param>
        public CBQueryCondition(FlowTypeEnum pFlowType, PayerReceiverEnum pPayerReceiver, bool pIsAssetCashOrIsFilterCss)
        {
            m_PayerReceiver = pPayerReceiver;
            if (FlowTypeEnum.Deposit == pFlowType)
            {
                m_IsFilterCss = pIsAssetCashOrIsFilterCss;
            }
            else if (FlowTypeEnum.Collateral == pFlowType)
            {
                m_IsAssetCash = pIsAssetCashOrIsFilterCss;
            }
        }
        #endregion Constructors
    }
}

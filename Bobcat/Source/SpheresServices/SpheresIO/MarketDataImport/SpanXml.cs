namespace EFS.SpheresIO.MarketData
{
    using EFS.ACommon;
    using EFS.ApplicationBlocks.Data;
    using EFS.Common;
    using EFS.Common.IO;
    using EFS.Common.MQueue;
    using EFS.Process;
    using EFS.SpheresIO.MarketData.Span;

    using EfsML.Enum;
    using EfsML.Enum.Tools;

    using FpML.Enum;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// 
    /// </summary>
    internal class MarketDataImportSPANXml : MarketDataImportBase
    {
        #region Class
        /// <summary>
        /// Class de gestion des informations sur un sous-jacent
        /// </summary>
        private class UnderlyingInfo
        {
            #region Members
            private readonly ClearingOrg m_ClearingOrg;
            private readonly string m_ContractType;
            private readonly undC m_UndC = default;
            private readonly undPf m_UndPf = default;
            //
            private Exchange m_OptUndExch;
            private NonOptionPF m_OptUndNonOptPf;
            private NonOptionContract m_OptUndNonOptCtr;
            private string m_OptUndMaturity;
            #endregion Members

            #region Accessors
            /// <summary>
            /// 
            /// </summary>
            private bool IsWithUndPf { get { return (m_UndPf != default(undPf)); } }
            //
            /// <summary>
            /// Exchange du sous-jacent d'une option
            /// </summary>
            public Exchange OptUndExch { get { return m_OptUndExch; } }
            /// <summary>
            /// Non Option Product Family du sous-jacent d'une option
            /// </summary>
            public NonOptionPF OptUndNonOptPf { get { return m_OptUndNonOptPf; } }
            /// <summary>
            /// Non Option Contract du sous-jacent d'une option
            /// </summary>
            public NonOptionContract OptUndNonOptCtr { get { return m_OptUndNonOptCtr; } }
            /// <summary>
            /// Maturity du sous-jacent d'une option
            /// </summary>
            public string OptUndMaturity { get { return m_OptUndMaturity; } }
            /// <summary>
            /// Underlying Product Family Code
            /// </summary>
            public string PfCode
            {
                get
                {
                    string pfCode;
                    if (IsWithUndPf)
                    {
                        pfCode = m_UndPf.pfCode;
                    }
                    else
                    {
                        pfCode = (OptUndNonOptPf != default(NonOptionPF) ? OptUndNonOptPf.pfCode : null);
                    }
                    return pfCode;
                }
            }
            #endregion Accessors

            #region Constructors
            /// <summary>
            /// Constructeur pour une Option
            /// </summary>
            /// <param name="pClearingOrg"></param>
            /// <param name="pUnderlying"></param>
            /// <param name="pContractType"></param>
            public UnderlyingInfo(ClearingOrg pClearingOrg, undC pUnderlying, string pContractType)
            {
                m_ClearingOrg = pClearingOrg;
                m_UndC = pUnderlying;
                m_ContractType = pContractType;
                //
                GetUnderlyingInfo();
            }
            /// <summary>
            /// Constructeur pour un Future
            /// </summary>
            /// <param name="pClearingOrg"></param>
            /// <param name="pUnderlying"></param>
            public UnderlyingInfo(ClearingOrg pClearingOrg, undPf pUnderlying)
            {
                m_ClearingOrg = pClearingOrg;
                m_UndPf = pUnderlying;
                if (m_UndPf != default(undPf))
                {
                    m_ContractType = m_UndPf.pfType;
                }
            }
            #endregion Constructors

            #region Methods
            /// <summary>
            /// Recherche les informations sur un sous-jacent
            /// </summary>
            private void GetUnderlyingInfo()
            {
                if ((m_ClearingOrg != default(ClearingOrg)) && (m_UndC != default(undC)))
                {
                    Exchange exch = m_ClearingOrg.exchange.FirstOrDefault(e => e.exch == m_UndC.exch);
                    if (exch != default(Exchange))
                    {
                        m_OptUndExch = exch;
                        switch (m_ContractType)
                        {
                            case "OOC":
                                CmbPf cmbPf = exch.CmbPf.FirstOrDefault(f => f.pfId == m_UndC.pfId);
                                if (cmbPf != default(CmbPf))
                                {
                                    m_OptUndNonOptPf = (NonOptionPF)cmbPf;
                                    Cmb cmb = cmbPf.cmb.FirstOrDefault(f => f.cId == m_UndC.cId);
                                    if (cmb != default(Cmb))
                                    {
                                        m_OptUndNonOptCtr = cmb;
                                        m_OptUndMaturity = cmb.pe;
                                    }
                                }
                                break;
                            case "OOE":
                                EquityPf eqyPf = exch.equityPf.FirstOrDefault(f => f.pfId == m_UndC.pfId);
                                if (eqyPf != default(EquityPf))
                                {
                                    m_OptUndNonOptPf = (NonOptionPF)eqyPf;
                                    Equity eqy = eqyPf.equity.FirstOrDefault(f => f.cId == m_UndC.cId);
                                    if (eqy != default(Equity))
                                    {
                                        m_OptUndMaturity = eqy.pe;
                                    }
                                }
                                break;
                            case "OOF":
                                FutPf futPf = exch.futPf.FirstOrDefault(f => f.pfId == m_UndC.pfId);
                                if (futPf != default(FutPf))
                                {
                                    m_OptUndNonOptPf = (NonOptionPF)futPf;
                                    Fut fut = futPf.fut.FirstOrDefault(f => f.cId == m_UndC.cId);
                                    if (fut != default(Fut))
                                    {
                                        m_OptUndNonOptCtr = fut;
                                        m_OptUndMaturity = fut.pe;
                                    }
                                }
                                break;
                            case "OOP":
                                PhyPf phyPf = exch.phyPf.FirstOrDefault(f => f.pfId == m_UndC.pfId);
                                if (phyPf != default(PhyPf))
                                {
                                    m_OptUndNonOptPf = (NonOptionPF)phyPf;
                                    Phy phy = phyPf.phy.FirstOrDefault(f => f.cId == m_UndC.cId);
                                    if (phy != default(Phy))
                                    {
                                        m_OptUndMaturity = phy.pe;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            #endregion Methods
        }
        #endregion

        #region Constants
        private const string SelectIMSPAN_HQuery =
            @"select * from dbo.IMSPAN_H
               where (DTBUSINESS = @DTBUSINESS)
                 and (DTBUSINESSTIME = @DTBUSINESSTIME)
                 and (SETTLEMENTSESSION = @SETTLEMENTSESSION)
                 and (EXCHANGECOMPLEX = @EXCHANGECOMPLEX)";
        private const string SelectIMSPANCURRENCY_HQuery =
            @"select * from dbo.IMSPANCURRENCY_H
               where (IDIMSPAN_H = @IDIMSPAN_H)
                 and (IDC = @IDC)";
        private const string SelectIMSPANCURCONV_HQuery =
            @"select * from dbo.IMSPANCURCONV_H
               where (IDIMSPAN_H = @IDIMSPAN_H)
                 and (IDC_CONTRACT = @IDC_CONTRACT)
                 and (IDC_MARGIN = @IDC_MARGIN)";
        private const string SelectIMSPANEXCHANGE_HQuery =
            @"select * from dbo.IMSPANEXCHANGE_H
               where (IDIMSPAN_H = @IDIMSPAN_H)
                 and (EXCHANGEACRONYM = @EXCHANGEACRONYM)";
        private const string SelectIMSPANCONTRACT_HQuery =
            @"select * from dbo.IMSPANCONTRACT_H
               where (IDIMSPANEXCHANGE_H = @IDIMSPANEXCHANGE_H)
                 and (CONTRACTSYMBOL = @CONTRACTSYMBOL)
                 and (CONTRACTTYPE = @CONTRACTTYPE)";
        private const string SelectIMSPANMATURITY_HQuery =
            @"select * from dbo.IMSPANMATURITY_H
               where (IDIMSPANCONTRACT_H = @IDIMSPANCONTRACT_H)
                 and (FUTMMY = @FUTMMY)
                 and ((OPTMMY = @OPTMMY) or ((OPTMMY is null) and (@OPTMMY is null)))";
        private const string SelectIMSPANARRAY_HQuery =
            @"select * from dbo.IMSPANARRAY_H
               where (IDIMSPANCONTRACT_H = @IDIMSPANCONTRACT_H)
                 and (FUTMMY = @FUTMMY)
                 and ((OPTMMY = @OPTMMY) or ((OPTMMY is null) and (@OPTMMY is null)))
                 and ((PUTCALL = @PUTCALL) or ((PUTCALL is null) and (@PUTCALL is null)))
                 and ((STRIKEPRICE = @STRIKEPRICE) or ((STRIKEPRICE is null) and (@STRIKEPRICE is null)))";
        private const string SelectIMSPANGRPCTR_HQuery =
            @"select * from dbo.IMSPANGRPCTR_H
               where (IDIMSPAN_H = @IDIMSPAN_H)
                 and (COMBCOMCODE = @COMBCOMCODE)";
        private const string SelectIMSPANGRPCOMB_HQuery =
            @"select * from dbo.IMSPANGRPCOMB_H
               where (IDIMSPAN_H = @IDIMSPAN_H)
                 and (COMBINEDGROUPCODE = @COMBINEDGROUPCODE)";
        private const string SelectIMSPANDLVMONTH_HQuery =
            @"select * from dbo.IMSPANDLVMONTH_H
               where (IDIMSPANGRPCTR_H = @IDIMSPANGRPCTR_H)
                 and (MATURITYMONTHYEAR = @MATURITYMONTHYEAR)";
        private const string SelectIMSPANTIER_HQuery =
            @"select * from dbo.IMSPANTIER_H
               where (IDIMSPANGRPCTR_H = @IDIMSPANGRPCTR_H)
                 and (SPREADTYPE = @SPREADTYPE)
                 and (TIERNUMBER = @TIERNUMBER)";
        private const string SelectIMSPANINTRASPR_HQuery =
            @"select * from dbo.IMSPANINTRASPR_H
               where (IDIMSPANGRPCTR_H = @IDIMSPANGRPCTR_H)
                 and (SPREADTYPE = @SPREADTYPE)
                 and (SPREADPRIORITY = @SPREADPRIORITY)";
        private const string SelectIMSPANINTRALEG_HQuery =
            @"select * from dbo.IMSPANINTRALEG_H
               where (IDIMSPANINTRASPR_H = @IDIMSPANINTRASPR_H)
                 and (LEGNUMBER = @LEGNUMBER)";
        private const string SelectIMSPANINTERSPR_HQuery =
            @"select * from dbo.IMSPANINTERSPR_H
               where (IDIMSPAN_H = @IDIMSPAN_H)
                 and (COMBINEDGROUPCODE = @COMBINEDGROUPCODE)
                 and (SPREADPRIORITY = @SPREADPRIORITY)
                 and (SPREADGROUPTYPE = @SPREADGROUPTYPE)";
        private const string SelectIMSPANINTERLEG_HQuery =
            @"select * from dbo.IMSPANINTERLEG_H
               where (IDIMSPANINTERSPR_H = @IDIMSPANINTERSPR_H)
                 and (LEGNUMBER = @LEGNUMBER)";
        #endregion Constants

        #region Members
        private spanFile m_SpanRiskFile;
        private ClearingOrg m_currentClearingOrg;
        private DateTime m_DtBusinessTime;
        private readonly bool m_IsPriceOnly = false;
        //
        /// <summary>
        /// Dictionary&lt;Combined Group Code,IDIMSPANGRPCOMB_H&gt;
        /// </summary>
        private Dictionary<string, int> m_CcGroup; 
        /// <summary>
        /// Liste des pfLink des Contracts importés
        /// </summary>
        private List<PfLink> m_ContractImported;
        /// <summary>
        /// Dictionary&lt;Commodity Code,IDIMSPANGRPCTR_H&gt; des groupes de contrats importés
        /// </summary>
        private Dictionary<string,int> m_CcImported;
        //
        private readonly static PfLink m_PfLinkComparer = new PfLink();
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pTask">Tâche IO</param>
        /// <param name="pDataName">Représente le source de donnée</param>
        /// <param name="pDataStyle">Représente le type de donnée en entrée</param>
        /// <param name="pIsPriceOnly">True si importation uniquement des prix</param>
        public MarketDataImportSPANXml(Task pTask, string pDataName, string pDataStyle, bool pIsPriceOnly)
            : base(pTask, pDataName, pDataStyle, true, false, AssetFindMaturityEnum.MATURITYMONTHYEAR)
        {
            m_IsPriceOnly = pIsPriceOnly;
            m_CcGroup = new Dictionary<string,int>();
            m_ContractImported = new List<PfLink>();
            m_CcImported = new Dictionary<string, int>();
        }
        #endregion Constructor

        #region Destructor
        ~MarketDataImportSPANXml()
        {
            m_CcGroup = null;
            m_ContractImported = null;
            m_CcImported = null;
        }
        #endregion Destructor

        #region Methods
        /// <summary>
        /// Méthode principale d'importation d'un fichier Span Xml
        /// </summary>
        /// <returns></returns>
        // EG 20220221 [XXXXX] Gestion IRQ
        public int ImportRiskParameterFile()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            int lineNumber = 0;
            XmlSerializer serializer = new XmlSerializer(typeof(spanFile));

            dtStart = OTCmlHelper.GetDateSysUTC(Cs);

            OpenInputFileName();

            // PM 20180219 [23824] IOTools => IOCommonTools
            //m_SpanRiskFile = (spanFile)serializer.Deserialize(IOTools.StreamReader);
            m_SpanRiskFile = (spanFile)serializer.Deserialize(IOCommonTools.StreamReader);
            if (m_SpanRiskFile != default(spanFile))
            {
                DateTime dtCreated = new DtFunc().StringToDateTime(m_SpanRiskFile.created, "yyyyMMddHHmm");
                // PM 20180808 [XXXXX] Ajout test pour vérifier qu'il y a des pointInTime
                if ((m_SpanRiskFile.pointInTime != default(pointInTime[])) && (m_SpanRiskFile.pointInTime.Count() > 0))
                {
                    foreach (pointInTime pit in m_SpanRiskFile.pointInTime)
                    {
                        if (IRQTools.IsIRQRequested(task.Process, task.Process.IRQNamedSystemSemaphore, ref ret))
                            break;
                        ProcessClearingOrg(dtCreated, m_SpanRiskFile.fileFormat, pit);
                    }
                }
            }

            CloseAllFiles();
            m_SpanRiskFile = default;

            return lineNumber;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPAN_H avec les paramètres DTBUSINESS, SETTLEMENTSESSION et EXCHANGECOMPLEX de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQuery"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtBusinessTime"></param>
        /// <param name="pSettlSession"></param>
        /// <param name="pExchangeComplex"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPAN_H(string pCS, string pQuery, DateTime pDtBusiness, DateTime pDtBusinessTime, string pSettlSession, string pExchangeComplex)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
            dataParameters.Add(new DataParameter(pCS, "DTBUSINESSTIME", DbType.DateTime), pDtBusinessTime);
            dataParameters.Add(new DataParameter(pCS, "SETTLEMENTSESSION", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pSettlSession);
            dataParameters.Add(new DataParameter(pCS, "EXCHANGECOMPLEX", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pExchangeComplex);

            QueryParameters qryParameters = new QueryParameters(pCS, pQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANCURRENCY_H avec les paramètres IDIMSPAN_H et IDC de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pIdC"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANCURRENCY_H(string pCS, int pIdIMSPAN_H, string pIdC)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPAN_H", DbType.Int32), pIdIMSPAN_H);
            dataParameters.Add(new DataParameter(pCS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), pIdC);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANCURRENCY_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANCURCONV_H avec les paramètres IDIMSPAN_H, IDC_CONTRACT et IDC_MARGIN de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pIdCContract"></param>
        /// <param name="pIdCMargin"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANCURCONV_H(string pCS, int pIdIMSPAN_H, string pIdCContract, string pIdCMargin)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPAN_H", DbType.Int32), pIdIMSPAN_H);
            dataParameters.Add(new DataParameter(pCS, "IDC_CONTRACT", DbType.AnsiString, SQLCst.UT_CURR_LEN), pIdCContract);
            dataParameters.Add(new DataParameter(pCS, "IDC_MARGIN", DbType.AnsiString, SQLCst.UT_CURR_LEN), pIdCMargin);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANCURCONV_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANEXCHANGE_H avec les paramètres IDIMSPAN_H et EXCHANGEACRONYM de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pExchangeAcronym"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANEXCHANGE_H(string pCS, int pIdIMSPAN_H, string pExchangeAcronym)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPAN_H", DbType.Int32), pIdIMSPAN_H);
            dataParameters.Add(new DataParameter(pCS, "EXCHANGEACRONYM", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pExchangeAcronym);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANEXCHANGE_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANCONTRACT_H avec les paramètres IDIMSPANEXCHANGE_H, CONTRACTSYMBOL et CONTRACTTYPE de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pContractType"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANCONTRACT_H(string pCS, int pIdIMSPANEXCHANGE_H, string pContractSymbol, string pContractType)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANEXCHANGE_H", DbType.Int32), pIdIMSPANEXCHANGE_H);
            dataParameters.Add(new DataParameter(pCS, "CONTRACTSYMBOL", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), pContractSymbol);
            dataParameters.Add(new DataParameter(pCS, "CONTRACTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pContractType);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANCONTRACT_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANMATURITY_H avec les paramètres IDIMSPANCONTRACT_H, FUTMMY et OPTMMY de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pFutMmy"></param>
        /// <param name="pOptMmy"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANMATURITY_H(string pCS, int pIdIMSPANCONTRACT_H, string pFutMmy, string pOptMmy)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANCONTRACT_H", DbType.Int32), pIdIMSPANCONTRACT_H);
            dataParameters.Add(new DataParameter(pCS, "FUTMMY", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), pFutMmy);
            dataParameters.Add(new DataParameter(pCS, "OPTMMY", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), pOptMmy);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANMATURITY_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANARRAY_H avec les paramètres IDIMSPANCONTRACT_H, FUTMMY, OPTMMY, PUTCALL et STRIKEPRICE de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pFutMmy"></param>
        /// <param name="pOptMmy"></param>
        /// <param name="pPutCall"></param>
        /// <param name="pStrikePrice"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANARRAY_H(string pCS, int pIdIMSPANCONTRACT_H, string pFutMmy, string pOptMmy, string pPutCall, decimal? pStrikePrice)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANCONTRACT_H", DbType.Int32), pIdIMSPANCONTRACT_H);
            dataParameters.Add(new DataParameter(pCS, "FUTMMY", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), pFutMmy);
            dataParameters.Add(new DataParameter(pCS, "OPTMMY", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), pOptMmy);
            dataParameters.Add(new DataParameter(pCS, "PUTCALL", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pPutCall);
            dataParameters.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal), pStrikePrice);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANARRAY_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANGRPCTR_H avec les paramètres IDIMSPAN_H et COMBCOMCODE de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pCombComCode"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANGRPCTR_H(string pCS, int pIdIMSPAN_H, string pCombComCode)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPAN_H", DbType.Int32), pIdIMSPAN_H);
            dataParameters.Add(new DataParameter(pCS, "COMBCOMCODE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), pCombComCode);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANGRPCTR_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANGRPCOMB_H avec les paramètres IDIMSPAN_H et COMBINEDGROUPCODE de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pCombGroupCode"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANGRPCOMB_H(string pCS, int pIdIMSPAN_H, string pCombGroupCode)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPAN_H", DbType.Int32), pIdIMSPAN_H);
            dataParameters.Add(new DataParameter(pCS, "COMBINEDGROUPCODE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), pCombGroupCode);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANGRPCOMB_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANDLVMONTH_H avec les paramètres IDIMSPANGRPCTR_H, MATURITYMONTHYEAR de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pMaturityMonthYear"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANDLVMONTH_H(string pCS, int pIdIMSPANGRPCTR_H, string pMaturityMonthYear)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANGRPCTR_H", DbType.Int32), pIdIMSPANGRPCTR_H);
            dataParameters.Add(new DataParameter(pCS, "MATURITYMONTHYEAR", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), pMaturityMonthYear);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANDLVMONTH_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANTIER_H avec les paramètres IDIMSPANGRPCTR_H, SPREADTYPE et TIERNUMBER de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pSpreadType"></param>
        /// <param name="pTierNumber"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANTIER_H(string pCS, int pIdIMSPANGRPCTR_H, string pSpreadType, int pTierNumber)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANGRPCTR_H", DbType.Int32), pIdIMSPANGRPCTR_H);
            dataParameters.Add(new DataParameter(pCS, "SPREADTYPE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_MANDATORY_LEN), pSpreadType);
            dataParameters.Add(new DataParameter(pCS, "TIERNUMBER", DbType.Int32), pTierNumber);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANTIER_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANINTRASPR_H avec les paramètres IDIMSPANGRPCTR_H, SPREADTYPE et SPREADPRIORITY de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pSpreadType"></param>
        /// <param name="pSpreadPriority"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANINTRASPR_H(string pCS, int pIdIMSPANGRPCTR_H, string pSpreadType, int pSpreadPriority)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANGRPCTR_H", DbType.Int32), pIdIMSPANGRPCTR_H);
            dataParameters.Add(new DataParameter(pCS, "SPREADTYPE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_MANDATORY_LEN), pSpreadType);
            dataParameters.Add(new DataParameter(pCS, "SPREADPRIORITY", DbType.Int32), pSpreadPriority);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANINTRASPR_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANINTRALEG_H avec les paramètres IDIMSPANINTRASPR_H et LEGNUMBER de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANINTRASPR_H"></param>
        /// <param name="pLegNumber"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANINTRALEG_H(string pCS, int pIdIMSPANINTRASPR_H, int pLegNumber)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANINTRASPR_H", DbType.Int32), pIdIMSPANINTRASPR_H);
            dataParameters.Add(new DataParameter(pCS, "LEGNUMBER", DbType.Int32), pLegNumber);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANINTRALEG_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANINTERSPR_H avec les paramètres IDIMSPAN_H, COMBINEDGROUPCODE et SPREADPRIORITY de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pCombinedGroupCode"></param>
        /// <param name="pSpreadPriority"></param>
        /// <param name="pSpreadGroupType"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANINTERSPR_H(string pCS, int pIdIMSPAN_H, string pCombinedGroupCode, int pSpreadPriority, string pSpreadGroupType)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPAN_H", DbType.Int32), pIdIMSPAN_H);
            dataParameters.Add(new DataParameter(pCS, "COMBINEDGROUPCODE", DbType.AnsiString, SQLCst.UT_LINKCODE_LEN), pCombinedGroupCode);
            dataParameters.Add(new DataParameter(pCS, "SPREADPRIORITY", DbType.Int32), pSpreadPriority);
            dataParameters.Add(new DataParameter(pCS, "SPREADGROUPTYPE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), pSpreadGroupType);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANINTERSPR_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table IMSPANINTERLEG_H avec les paramètres IDIMSPANINTERSPR_H et LEGNUMBER de renseignés.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pLegNumber"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParameters_IMSPANINTERLEG_H(string pCS, int pIdIMSPANINTERSPR_H, int pLegNumber)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCS, "IDIMSPANINTERSPR_H", DbType.Int32), pIdIMSPANINTERSPR_H);
            dataParameters.Add(new DataParameter(pCS, "LEGNUMBER", DbType.Int32), pLegNumber);

            QueryParameters qryParameters = new QueryParameters(pCS, SelectIMSPANINTERLEG_HQuery, dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// Construit un QueryParameters pour la table QUOTE_ETD_H.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMarketEnv"></param>
        /// <param name="pIdValScenario"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20181126 [24308] Add parameter pQuoteSide
        private static QueryParameters GetQueryParameters_QUOTE_ETD_H(string pCS, string pIdMarketEnv, string pIdValScenario, int pIdAsset, DateTime pDate, QuotationSideEnum pQuotationSideEnum)
        {
            QueryParameters qryParameters = GetQueryParameters_QUOTE_XXX_H(pCS, Cst.OTCml_TBL.QUOTE_ETD_H, pIdMarketEnv, pIdValScenario, pDate, pQuotationSideEnum);
            qryParameters.Parameters["IDASSET"].Value = pIdAsset;
            //
            return qryParameters;
        }

        /// <summary>
        ///  Construit un QueryParameters pour rechercher l'Asset Future sous-jacent d'un Asset Option sur Future
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdDC"></param>
        /// <param name="pMaturity"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryParametersUnderlyingFuture(string pCS, int pIdDC, string pMaturity)
        {
            // Parameters
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDDC), pIdDC);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.MATURITYMONTHYEAR), pMaturity);
            // Query
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + " da.IDASSET, dc.FINALSETTLTTIME" + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB + " da" + SQLCst.ON + "(da.IDDC = dc.IDDC)" + Cst.CrLf;
            query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MATURITY + " mat" + SQLCst.ON + "(mat.IDMATURITY = da.IDMATURITY)" + Cst.CrLf;
            query += SQLCst.WHERE + "(dc.CATEGORY = 'O')" + Cst.CrLf;
            query += SQLCst.AND + "(dc.ASSETCATEGORY = 'Future')" + Cst.CrLf;
            query += SQLCst.AND + "(dc.IDDC = @IDDC)" + Cst.CrLf;
            query += SQLCst.AND + "(mat.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)" + Cst.CrLf;
            //
            QueryParameters qryParameters = new QueryParameters(pCS, query.ToString(), dataParameters);
            return qryParameters;
        }

        /// <summary>
        /// Construction du QueryParameters pour la mise à jour des CONTRACTMULTIPLIER de la table DERIVATIVECONTRACT
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pIdAUpd"></param>
        /// <param name="pDtUpd"></param>
        /// <returns></returns>
        private static QueryParameters GetQueryUpdCtrMultDERIVATIVECONTRACT(string pCS, int pIdDC, decimal pContractMultiplier, int pIdAUpd, DateTime pDtUpd)
        {
            string sql = @"update dbo.DERIVATIVECONTRACT 
                              set CONTRACTMULTIPLIER = @CONTRACTMULTIPLIER,
                                  IDAUPD = @IDAUPD,
                                  DTUPD = @DTUPD
                              where (ISAUTOSETTING = 1)
                                and (((CONTRACTMULTIPLIER is null) or (CONTRACTMULTIPLIER != @CONTRACTMULTIPLIER))
                                and (IDDC=@IDDC)";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.CONTRACTMULTIPLIER), pContractMultiplier);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), pIdAUpd);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), pDtUpd);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDDC), pIdDC);
            QueryParameters qry = new QueryParameters(pCS, sql, dp);

            return qry;
        }

        /// <summary>
        /// Supprime les paramètres de risque précédemment intégrés
        /// pour une date business, settlement session et exchange complex
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pDtBusinessTime"></param>
        /// <param name="pSettlSession"></param>
        /// <param name="pExchangeComplex"></param>
        /// <returns></returns>
        private static int DeleteAllRiskParameters(string pCS, DateTime pDtBusiness, DateTime pDtBusinessTime, string pSettlSession, string pExchangeComplex)
        {
            string sqlQuery = @"
                delete from dbo.IMSPAN_H
                 where (DTBUSINESS        = @DTBUSINESS)
                   and (DTBUSINESSTIME    = @DTBUSINESSTIME)
                   and (SETTLEMENTSESSION = @SETTLEMENTSESSION)
                   and (EXCHANGECOMPLEX   = @EXCHANGECOMPLEX)";

            QueryParameters qryParameters = GetQueryParameters_IMSPAN_H(pCS, sqlQuery, pDtBusiness, pDtBusinessTime, pSettlSession, pExchangeComplex);

            int nRowDeleted;
            try
            {
                nRowDeleted = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            }
            catch (Exception e)
            {
                throw new Exception(StrFunc.AppendFormat("Delete from IMSPAN_H for date: {0}, clearing organization: {1} and settlement session: {2}", pDtBusiness, pExchangeComplex, pSettlSession), e);
            }
            return nRowDeleted;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPAN_H et retourne son Id
        /// </summary>
        /// <param name="pDtBusiness"></param>
        /// <param name="pSettlSession"></param>
        /// <param name="pExchangeComplex"></param>
        /// <param name="pDtBusinessTime"></param>
        /// <param name="pDtFile"></param>
        /// <param name="pFileIdentifier"></param>
        /// <param name="pFileFormat"></param>
        /// <param name="pIsOptValueLimit"></param>
        /// <param name="pBusinessFunction"></param>
        /// <returns></returns>
        private int Insert_IMSPAN_H(DateTime pDtBusiness, string pSettlSession, string pExchangeComplex,
            DateTime pDtBusinessTime, DateTime pDtFile, string pFileIdentifier, string pFileFormat, bool pIsOptValueLimit, string pBusinessFunction)
        {
            int idIMSPAN_H;
            try
            {
                QueryParameters qryParameters = GetQueryParameters_IMSPAN_H(Cs, SelectIMSPAN_HQuery, pDtBusiness, pDtBusinessTime, pSettlSession, pExchangeComplex);
                DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                DataRow dr = SetDataRow(dt);
                //
                dr["DTBUSINESS"] = pDtBusiness;
                dr["SETTLEMENTSESSION"] = pSettlSession;
                dr["EXCHANGECOMPLEX"] = pExchangeComplex;
                //
                dr["DTBUSINESSTIME"] = pDtBusinessTime;
                dr["DTFILE"] = pDtFile;
                dr["FILEIDENTIFIER"] = pFileIdentifier;
                dr["FILEFORMAT"] = pFileFormat;
                dr["ISOPTIONVALUELIMIT"] = pIsOptValueLimit;
                dr["BUSINESSFUNCTION"] = pBusinessFunction;
                //
                DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                //
                idIMSPAN_H = Convert.ToInt32(dt.Rows[0]["IDIMSPAN_H"]);
            }
            catch (Exception e)
            {
                throw new Exception(StrFunc.AppendFormat("Insert IMSPAN_H for date:{0}, clearing organization: {1} and settlement session: {2}", DtFunc.DateTimeToStringISO(m_dtFile), pExchangeComplex, pSettlSession), e);
            }
            return idIMSPAN_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANCURRENCY_H
        /// </summary>
        /// <param name="pIdSPAN_H"></param>
        /// <param name="pExchange"></param>
        private void Insert_IMSPANCURRENCY_H(int pIdSPAN_H, currencyDef pCurrencyDef)
        {
            if (pCurrencyDef != default(currencyDef))
            {
                string idC = pCurrencyDef.currency;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANCURRENCY_H(Cs, pIdSPAN_H, idC);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPAN_H"] = pIdSPAN_H;
                    dr["IDC"] = idC;
                    //
                    dr["DESCRIPTION"] = pCurrencyDef.name;
                    dr["EXPONENT"] = 0;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANCURRENCY_H for Currency: {0}", idC), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANCURCONV_H
        /// </summary>
        /// <param name="pIdSPAN_H"></param>
        /// <param name="pExchange"></param>
        /// <returns></returns>
        private void Insert_IMSPANCURCONV_H(int pIdSPAN_H, curConv pCurConv)
        {
            if (pCurConv != default(curConv))
            {
                string idCFrom = pCurConv.fromCur;
                string idCTo = pCurConv.toCur;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANCURCONV_H(Cs, pIdSPAN_H, idCFrom, idCTo);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPAN_H"] = pIdSPAN_H;
                    dr["IDC_CONTRACT"] = idCFrom;
                    dr["IDC_MARGIN"] = idCTo;
                    //
                    //Non présent: dr["SHIFTDOWN"] =;
                    //Non présent: dr["SHIFTUP"] =;
                    if (StrFunc.IsFilled(pCurConv.factor) && decimal.TryParse(pCurConv.factor, out decimal value))
                    {
                        dr["VALUE"] = value;
                    }
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANCURCONV_H for Currency From: {0} and Currency To: {1}", idCFrom, idCTo), e);
                }
            }
        }
        
        /// <summary>
        /// Insère un enregistrement dans IMSPANEXCHANGE_H et retourne son Id
        /// </summary>
        /// <param name="pIdSPAN_H"></param>
        /// <param name="pExchange"></param>
        /// <returns></returns>
        private int Insert_IMSPANEXCHANGE_H(int pIdSPAN_H, Exchange pExchange)
        {
            int idIMSPANEXCHANGE_H = 0;
            if (pExchange != default(Exchange))
            {
                string exchangeAcronym = pExchange.exch;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANEXCHANGE_H(Cs, pIdSPAN_H, exchangeAcronym);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPAN_H"] = pIdSPAN_H;
                    dr["EXCHANGEACRONYM"] = exchangeAcronym;
                    //
                    dr["EXCHANGESYMBOL"] = exchangeAcronym;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANEXCHANGE_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANEXCHANGE_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANEXCHANGE_H for Exchange: {0}", exchangeAcronym), e);
                }
            }
            return idIMSPANEXCHANGE_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANCONTRACT_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pFutPf"></param>
        /// <returns></returns>
        private int Insert_IMSPANCONTRACT_H(int pIdIMSPANEXCHANGE_H, NonOptionPF pFutPf)
        {
            int idIMSPANCONTRACT_H = 0;
            if (pFutPf != default(NonOptionPF))
            {
                string contractSymbol = pFutPf.pfCode;
                string contractType = "FUT";
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANCONTRACT_H(Cs, pIdIMSPANEXCHANGE_H, contractSymbol, contractType);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    string productName = (StrFunc.IsFilled(pFutPf.name) ? pFutPf.name.Substring(0, System.Math.Min(pFutPf.name.Length, SQLCst.UT_DISPLAYNAME_LEN)) : null);
                    //
                    dr["IDIMSPANEXCHANGE_H"] = pIdIMSPANEXCHANGE_H;
                    dr["CONTRACTSYMBOL"] = contractSymbol;
                    dr["CONTRACTTYPE"] = contractType;
                    //
                    //Non présent: dr["CABINETOPTVALUE"] =;
                    dr["CATEGORY"] = "F";
                    if (pFutPf.cvfSpecified && (pFutPf.cvf != 0))
                    {
                        dr["CONTRACTMULTIPLIER"] = pFutPf.cvf;
                    }
                    //Non présent: dr["DELTADEN"] =;
                    dr["DELTASCALINGFACTOR"] = 1;
                    //Non présent: dr["EXERCISESTYLE"] =;
                    dr["FUTVALUATIONMETHOD"] = (StrFunc.IsFilled(pFutPf.valueMeth) ? pFutPf.valueMeth : null);
                    dr["IDC_PRICE"] = (StrFunc.IsFilled(pFutPf.currency) ? pFutPf.currency : null);
                    dr["ISOPTVARIABLETICK"] = false;
                    //Non présent: dr["MINPRICEINCR"] =;
                    //Non présent: dr["MINPRICEINCRAMOUNT"] =;
                    dr["PFID"] = pFutPf.pfId;
                    dr["PRICEALIGNCODE"] = (StrFunc.IsFilled(pFutPf.priceFmt) ? pFutPf.priceFmt : null);
                    if (pFutPf.priceDlSpecified)
                    {
                        dr["PRICEDECLOCATOR"] = pFutPf.priceDl;
                    }
                    dr["PRICEQUOTEMETHOD"] = (StrFunc.IsFilled(pFutPf.priceMeth) ? pFutPf.priceMeth : null);
                    //Non présent: dr["PRICESCANQUOTEMETH"] =;
                    //Non présent: dr["PRICESCANVALTYPE"] =;
                    dr["PRODUCTLONGNAME"] = productName;
                    dr["PRODUCTNAME"] = productName;
                    //Non présent: dr["PRODUCTPERCONTRACT"] =;
                    //Non présent: dr["PUTCALL"] =;
                    //Non présent: dr["RISKVALDECLOCATOR"] =;
                    //Non présent: dr["SCANNINGRANGE"] =;
                    dr["SETTLTMETHOD"] = (pFutPf.setlMeth == "DELIV" ? 'P' : 'C');
                    //Non présent: dr["STRIKEALIGNCODE"] =;
                    //Non présent: dr["STRIKEDECLOCATOR"] =;
                    //Non présent: dr["STRIKEDEN"] =;
                    //Non présent: dr["TICKVALUE"] =;
                    //Non présent: dr["VOLATSCANQUOTEMETH"] =;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANCONTRACT_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANCONTRACT_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANCONTRACT_H for Contract: Sym({0}), Type(FUT)", contractSymbol), e);
                }
            }
            return idIMSPANCONTRACT_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANCONTRACT_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pFutPf"></param>
        /// <returns></returns>
        private int Insert_IMSPANCONTRACT_H(int pIdIMSPANEXCHANGE_H, OptionPF pOptPf, string pContractType)
        {
            int idIMSPANCONTRACT_H = 0;
            if (pOptPf != default(OptionPF))
            {
                string contractSymbol = pOptPf.pfCode;
                string contractType = pContractType;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANCONTRACT_H(Cs, pIdIMSPANEXCHANGE_H, contractSymbol, contractType);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    string productName = (StrFunc.IsFilled(pOptPf.name) ? pOptPf.name.Substring(0, System.Math.Min(pOptPf.name.Length, SQLCst.UT_DISPLAYNAME_LEN)) : null);
                    //
                    dr["IDIMSPANEXCHANGE_H"] = pIdIMSPANEXCHANGE_H;
                    dr["CONTRACTSYMBOL"] = contractSymbol;
                    dr["CONTRACTTYPE"] = contractType;
                    //
                    // PM 20180822 [XXXXX] Ajout test Cabinet Option Value != 0
                    if (pOptPf.cab != 0)
                    {
                        dr["CABINETOPTVALUE"] = pOptPf.cab;
                    }
                    dr["CATEGORY"] = "O";
                    if (pOptPf.cvfSpecified && (pOptPf.cvf != 0))
                    {
                        dr["CONTRACTMULTIPLIER"] = pOptPf.cvf;
                    }
                    //Non présent: dr["DELTADEN"] =;
                    dr["DELTASCALINGFACTOR"] = 1;
                    dr["EXERCISESTYLE"] = (pOptPf.exercise == "EURO" ? 0 : 1);
                    dr["FUTVALUATIONMETHOD"] = (StrFunc.IsFilled(pOptPf.valueMeth) ? pOptPf.valueMeth : null);
                    dr["IDC_PRICE"] = (StrFunc.IsFilled(pOptPf.currency) ? pOptPf.currency : null);
                    dr["ISOPTVARIABLETICK"] = (pOptPf.isVariableTickSpecified && pOptPf.isVariableTick);
                    //Non présent: dr["MINPRICEINCR"] =;
                    //Non présent: dr["MINPRICEINCRAMOUNT"] =;
                    dr["PFID"] = pOptPf.pfId;
                    dr["PRICEALIGNCODE"] = (StrFunc.IsFilled(pOptPf.priceFmt) ? pOptPf.priceFmt : null);
                    if (pOptPf.priceDlSpecified)
                    {
                        dr["PRICEDECLOCATOR"] = pOptPf.priceDl;
                    }
                    dr["PRICEQUOTEMETHOD"] = (StrFunc.IsFilled(pOptPf.priceMeth) ? pOptPf.priceMeth : null);
                    //Non présent: dr["PRICESCANQUOTEMETH"] =;
                    //Non présent: dr["PRICESCANVALTYPE"] =;
                    dr["PRODUCTLONGNAME"] = productName;
                    dr["PRODUCTNAME"] = productName;
                    if (StrFunc.IsFilled(pOptPf.strikeDl) && decimal.TryParse(pOptPf.strikeDl, out decimal contractSize))
                    {
                        dr["PRODUCTPERCONTRACT"] = contractSize;
                    }
                    //Non présent: dr["PUTCALL"] =;
                    //Non présent: dr["RISKVALDECLOCATOR"] =;
                    //Non présent: dr["SCANNINGRANGE"] =;
                    dr["SETTLTMETHOD"] = (pOptPf.setlMeth == "DELIV" ? 'P' : 'C');
                    dr["STRIKEALIGNCODE"] = (StrFunc.IsFilled(pOptPf.strikeFmt) ? pOptPf.strikeFmt : null);
                    if (StrFunc.IsFilled(pOptPf.strikeDl) && int.TryParse(pOptPf.strikeDl, out int strikeDl))
                    {
                        dr["STRIKEDECLOCATOR"] = strikeDl;
                    }
                    //Non présent: dr["STRIKEDEN"] =;
                    //Non présent: dr["TICKVALUE"] =;
                    //Non présent: dr["VOLATSCANQUOTEMETH"] =;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANCONTRACT_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANCONTRACT_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANCONTRACT_H for Contract: Sym({0}), Type({1})", contractSymbol, contractType), e);
                }
            }
            return idIMSPANCONTRACT_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANMATURITY_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pCmb"></param>
        /// <returns></returns>
        private int Insert_IMSPANMATURITY_H(int pIdIMSPANCONTRACT_H, UnderlyingInfo pUnderlying, Cmb pCmb)
        {
            int idIMSPANMATURITY_H = 0;
            if ((pUnderlying != default(UnderlyingInfo)) && (pCmb != default(Cmb)))
            {
                string futMaturity = pCmb.pe;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANMATURITY_H(Cs, pIdIMSPANCONTRACT_H, futMaturity, null);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANCONTRACT_H"] = pIdIMSPANCONTRACT_H;
                    dr["FUTMMY"] = futMaturity;
                    dr["OPTMMY"] = null;
                    //
                    dr["BASEVOLATILITY"] = pCmb.v;
                    if (StrFunc.IsFilled(pCmb.sc) && decimal.TryParse(pCmb.sc, out decimal scaling))
                    {
                        dr["DELTASCALINGFACTOR"] = scaling;
                    }
                    else
                    {
                        dr["DELTASCALINGFACTOR"] = 1;
                    }
                    //Non présent: dr["DISCOUNTFACTOR"] =;
                    //Non présent: dr["DIVIDENDYIELD"] =;
                    //Non présent: dr["EXTRMMOVECOVFRACT"] =;
                    //Non présent: dr["EXTRMMOVEMULT"] =;
                    //Non présent: dr["INTERESTRATE"] =;
                    //Non présent: dr["LOOKAHEADTIME"] =;
                    dr["MATURITYDATE"] = new DtFunc().StringyyyyMMddToDateTime(pCmb.setlDate);
                    //Non présent: dr["OPTREFERENCEPRICE"] =;
                    //Non présent: dr["PRICINGMODEL"] =;
                    if (StrFunc.IsFilled(pCmb.t) && decimal.TryParse(pCmb.t, out decimal timeToExpiration))
                    {
                        dr["TIMETOEXPIRATION"] = timeToExpiration;
                    }
                    dr["UNLCONTRACTSYMBOL"] = pUnderlying.PfCode;
                    if (pCmb.cvfSpecified)
                    {
                        dr["VALUEFACTOR"] = pCmb.cvf;
                    }
                    //Non présent: dr["VOLATILITYDOWN"] =;
                    //Non présent: dr["VOLATILITYUP"] =;
                    if ((pCmb.scanRate != default(scanRate[])) && pCmb.scanRate.Length > 0)
                    {
                        scanRate scan = pCmb.scanRate[0];
                        dr["FUTPRICESCANRANGE"] = scan.priceScan;
                        dr["VOLATSCANRANGE"] = scan.volScan;
                    }
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANMATURITY_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANMATURITY_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANMATURITY_H for Maturity: Ctr Id({0}), FMat({1})", pIdIMSPANCONTRACT_H, futMaturity), e);
                }
            }
            return idIMSPANMATURITY_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANMATURITY_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pFut"></param>
        /// <returns></returns>
        private int Insert_IMSPANMATURITY_H(int pIdIMSPANCONTRACT_H, UnderlyingInfo pUnderlying, Fut pFut)
        {
            int idIMSPANMATURITY_H = 0;
            if ((pUnderlying != default(UnderlyingInfo)) && (pFut != default(Fut)))
            {
                string futMaturity = pFut.pe;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANMATURITY_H(Cs, pIdIMSPANCONTRACT_H, futMaturity, null);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANCONTRACT_H"] = pIdIMSPANCONTRACT_H;
                    dr["FUTMMY"] = futMaturity;
                    dr["OPTMMY"] = null;
                    //
                    dr["BASEVOLATILITY"] = pFut.v;
                    if (StrFunc.IsFilled(pFut.sc) && decimal.TryParse(pFut.sc, out decimal scaling))
                    {
                        dr["DELTASCALINGFACTOR"] = scaling;
                    }
                    else
                    {
                        dr["DELTASCALINGFACTOR"] = 1;
                    }
                    //Non présent: dr["DISCOUNTFACTOR"] =;
                    //Non présent: dr["DIVIDENDYIELD"] =;
                    //Non présent: dr["EXTRMMOVECOVFRACT"] =;
                    //Non présent: dr["EXTRMMOVEMULT"] =;
                    //Non présent: dr["INTERESTRATE"] =;
                    //Non présent: dr["LOOKAHEADTIME"] =;
                    dr["MATURITYDATE"] = new DtFunc().StringyyyyMMddToDateTime(pFut.setlDate);
                    //Non présent: dr["OPTREFERENCEPRICE"] =;
                    //Non présent: dr["PRICINGMODEL"] =;
                    if (StrFunc.IsFilled(pFut.t) && decimal.TryParse(pFut.t, out decimal timeToExpiration))
                    {
                        dr["TIMETOEXPIRATION"] = timeToExpiration;
                    }
                    dr["UNLCONTRACTSYMBOL"] = pUnderlying.PfCode;
                    if (pFut.cvfSpecified)
                    {
                        dr["VALUEFACTOR"] = pFut.cvf;
                    }
                    //Non présent: dr["VOLATILITYDOWN"] =;
                    //Non présent: dr["VOLATILITYUP"] =;
                    if ((pFut.scanRate != default(scanRate[])) && pFut.scanRate.Length > 0)
                    {
                        scanRate scan = pFut.scanRate[0];
                        dr["FUTPRICESCANRANGE"] = scan.priceScan;
                        dr["VOLATSCANRANGE"] = scan.volScan;
                    }
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANMATURITY_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANMATURITY_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANMATURITY_H for Maturity: Ctr Id({0}), FMat({1})", pIdIMSPANCONTRACT_H, futMaturity), e);
                }
            }
            return idIMSPANMATURITY_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANMATURITY_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pOptPf"></param>
        /// <param name="pSerie"></param>
        /// <returns></returns>
        private int Insert_IMSPANMATURITY_H(int pIdIMSPANCONTRACT_H, UnderlyingInfo pUnderlying, OptionPF pOptPf, Series pSerie)
        {
            int idIMSPANMATURITY_H = 0;
            if ((pOptPf != default(OptionPF)) && (pSerie != default(Series)) && (pUnderlying != default(UnderlyingInfo)))
            {
                string underlyingMaturity = pUnderlying.OptUndMaturity;
                string optMaturity = pSerie.pe;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANMATURITY_H(Cs, pIdIMSPANCONTRACT_H, underlyingMaturity, optMaturity);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANCONTRACT_H"] = pIdIMSPANCONTRACT_H;
                    dr["FUTMMY"] = underlyingMaturity;
                    dr["OPTMMY"] = optMaturity;
                    //
                    dr["BASEVOLATILITY"] = pSerie.v;
                    if (StrFunc.IsFilled(pSerie.sc) && decimal.TryParse(pSerie.sc, out decimal scaling))
                    {
                        dr["DELTASCALINGFACTOR"] = scaling;
                    }
                    else
                    {
                        dr["DELTASCALINGFACTOR"] = 1;
                    }
                    //Non présent: dr["DISCOUNTFACTOR"] =;
                    //Non présent: dr["DIVIDENDYIELD"] =;
                    //Non présent: dr["EXTRMMOVECOVFRACT"] =;
                    //Non présent: dr["EXTRMMOVEMULT"] =;
                    //Non présent: dr["INTERESTRATE"] =;
                    //Non présent: dr["LOOKAHEADTIME"] =;
                    dr["MATURITYDATE"] = new DtFunc().StringyyyyMMddToDateTime(pSerie.setlDate);
                    if (pSerie.refPriceSpecified)
                    {
                        dr["OPTREFERENCEPRICE"] = pSerie.refPrice;
                    }
                    dr["PRICINGMODEL"] = pOptPf.priceModel;
                    if (StrFunc.IsFilled(pSerie.t) && decimal.TryParse(pSerie.t, out decimal timeToExpiration))
                    {
                        dr["TIMETOEXPIRATION"] = timeToExpiration;
                    }
                    dr["UNLCONTRACTSYMBOL"] = pUnderlying.PfCode;
                    if (pSerie.cvfSpecified)
                    {
                        dr["VALUEFACTOR"] = pSerie.cvf;
                    }
                    //Non présent: dr["VOLATILITYDOWN"] =;
                    //Non présent: dr["VOLATILITYUP"] =;
                    if ((pSerie.scanRate != default(scanRate[])) && pSerie.scanRate.Length > 0)
                    {
                        scanRate scan = pSerie.scanRate[0];
                        dr["FUTPRICESCANRANGE"] = scan.priceScan;
                        dr["VOLATSCANRANGE"] = scan.volScan;
                    }
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANMATURITY_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANMATURITY_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANMATURITY_H for Maturity: Ctr Id({0}), FMat({1}), OMat({2})", pIdIMSPANCONTRACT_H, underlyingMaturity, optMaturity), e);
                }
            }
            return idIMSPANMATURITY_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANARRAY_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pIdIMSPANMATURITY_H"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCmb"></param>
        /// <param name="pCompositeDelta"></param>
        /// <returns></returns>
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private int Insert_IMSPANARRAY_H(int pIdIMSPANCONTRACT_H, int pIdAsset, UnderlyingInfo pUnderlying, decimal? pPrice, Cmb pCmb, decimal pCompositeDelta)
        {
            int idIMSPANARRAY_H = 0;
            if (pCmb != default(Cmb))
            {
                string futMaturity = pCmb.pe;
                string optMaturity = null;
                string putCall = null;
                decimal? strikePrice = null;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANARRAY_H(Cs, pIdIMSPANCONTRACT_H, futMaturity, optMaturity, putCall, strikePrice);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANCONTRACT_H"] = pIdIMSPANCONTRACT_H;
                    dr["FUTMMY"] = futMaturity;
                    //dr["OPTMMY"] = optMaturity;
                    //dr["PUTCALL"] = putCall;
                    //dr["STRIKEPRICE"] = strikePrice;
                    //
                    dr["ASSETCATEGORY"] = "Future";
                    if (pCmb.cvfSpecified)
                    {
                        dr["CONTRACTMULTIPLIER"] = pCmb.cvf;
                        dr["CONTRACTVALUEFACTOR"] = pCmb.cvf;
                    }
                    dr["CURRENTDELTA"] = (decimal)(pCmb.d);
                    //Non présent: dr["CYCLEINDICATOR"] =;
                    dr["IDASSET"] = pIdAsset;
                    //Non présent: dr["LOTSIZE"] =;
                    //Non présent: dr["MATURITYDAY"] =;
                    if (pPrice.HasValue)
                    {
                        dr["PRICE"] = pPrice.Value;
                    }
                    //Non présent: dr["STRIKEVALUEFACTOR"] =;
                    dr["UNLCONTRACTSYMBOL"] = pUnderlying.PfCode;
                    dr["VOLATILITY"] = (decimal)(pCmb.v);
                    //
                    /* RISKVALUE 1 à 16 et Delta*/
                    // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
                    FillRiskArrayDataRow(dr, pCmb.ra, pCompositeDelta);
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANARRAY_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANARRAY_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANARRAY_H for Asset: Ctr Id({0}), FMat({1}))", pIdIMSPANCONTRACT_H, futMaturity), e);
                }
            }
            return idIMSPANARRAY_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANARRAY_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pIdIMSPANMATURITY_H"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pPrice"></param>
        /// <param name="pFut"></param>
        /// <param name="pCompositeDelta"></param>
        /// <returns></returns>
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private int Insert_IMSPANARRAY_H(int pIdIMSPANCONTRACT_H, int pIdIMSPANMATURITY_H, int pIdAsset, UnderlyingInfo pUnderlying, decimal? pPrice, Fut pFut, decimal pCompositeDelta)
        {
            int idIMSPANARRAY_H = 0;
            if (pFut != default(Fut))
            {
                string futMaturity = pFut.pe;
                string optMaturity = null;
                string putCall = null;
                decimal? strikePrice = null;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANARRAY_H(Cs, pIdIMSPANCONTRACT_H, futMaturity, optMaturity, putCall, strikePrice);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANCONTRACT_H"] = pIdIMSPANCONTRACT_H;
                    dr["FUTMMY"] = futMaturity;
                    //dr["OPTMMY"] = optMaturity;
                    //dr["PUTCALL"] = putCall;
                    //dr["STRIKEPRICE"] = strikePrice;
                    //
                    dr["ASSETCATEGORY"] = "Future";
                    if (pFut.cvfSpecified)
                    {
                        dr["CONTRACTMULTIPLIER"] = pFut.cvf;
                        dr["CONTRACTVALUEFACTOR"] = pFut.cvf;
                    }
                    dr["CURRENTDELTA"] = (decimal)(pFut.d);
                    //Non présent: dr["CYCLEINDICATOR"] =;
                    dr["IDASSET"] = pIdAsset;
                    //Non présent: dr["LOTSIZE"] =;
                    //Non présent: dr["MATURITYDAY"] =;
                    if (pPrice.HasValue)
                    {
                        dr["PRICE"] = pPrice.Value;
                    }
                    //Non présent: dr["STRIKEVALUEFACTOR"] =;
                    dr["UNLCONTRACTSYMBOL"] = pUnderlying.PfCode;
                    dr["VOLATILITY"] = (decimal)(pFut.v);
                    //
                    /* RISKVALUE 1 à 16 et Delta*/
                    // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
                    FillRiskArrayDataRow(dr, pFut.ra, pCompositeDelta);
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANARRAY_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANARRAY_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANARRAY_H for Asset: Ctr Id({0}), FMat({1}))", pIdIMSPANCONTRACT_H, futMaturity), e);
                }
            }
            return idIMSPANARRAY_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANARRAY_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pIdIMSPANMATURITY_H"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pSerie"></param>
        /// <param name="pOpt"></param>
        /// <param name="pStrikePrice"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCompositeDelta"></param>
        /// <returns></returns>
        // PM 20180824 [XXXXX] Gestion de la Base >= 10000 : ajout pStrikePrice comme paramètre
        //private int Insert_IMSPANARRAY_H(int pIdIMSPANCONTRACT_H, int pIdIMSPANMATURITY_H, int pIdAsset, UnderlyingInfo pUnderlying, series pSerie, decimal? pPrice, opt pOpt)
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private int Insert_IMSPANARRAY_H(int pIdIMSPANCONTRACT_H, int pIdIMSPANMATURITY_H, int pIdAsset, UnderlyingInfo pUnderlying, Series pSerie, Opt pOpt, decimal? pStrikePrice, decimal? pPrice, decimal pCompositeDelta)
        {
            int idIMSPANARRAY_H = 0;
            if ((pSerie != default(Series)) && (pUnderlying != default(UnderlyingInfo)))
            {
                string futMaturity = pUnderlying.OptUndMaturity;
                string optMaturity = pSerie.pe;
                string putCall = ((pOpt.o.ToUpper() == "P") ? "0" : ((pOpt.o.ToUpper() == "C") ? "1" : null));
                // PM 20180824 [XXXXX] Gestion de la Base >= 10000 : ajout pStrikePrice comme paramètre
                //decimal? strikePrice = null;
                //if (DecFunc.IsDecimal(pOpt.k))
                //{
                //    strikePrice = DecFunc.DecValueFromInvariantCulture(pOpt.k);
                //}
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANARRAY_H(Cs, pIdIMSPANCONTRACT_H, futMaturity, optMaturity, putCall, pStrikePrice);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANCONTRACT_H"] = pIdIMSPANCONTRACT_H;
                    dr["FUTMMY"] = futMaturity;
                    dr["OPTMMY"] = optMaturity;
                    dr["PUTCALL"] = putCall;
                    dr["STRIKEPRICE"] = pStrikePrice;
                    //
                    dr["ASSETCATEGORY"] = "Future";
                    if (pSerie.cvfSpecified)
                    {
                        dr["CONTRACTMULTIPLIER"] = pSerie.cvf;
                        dr["CONTRACTVALUEFACTOR"] = pSerie.cvf;
                    }
                    dr["CURRENTDELTA"] = (decimal)(pOpt.d);
                    //Non présent: dr["CYCLEINDICATOR"] =;
                    dr["IDASSET"] = pIdAsset;
                    //Non présent: dr["LOTSIZE"] =;
                    //Non présent: dr["MATURITYDAY"] =;
                    if (pPrice.HasValue)
                    {
                        dr["PRICE"] = pPrice.Value;
                    }
                    //Non présent: dr["STRIKEVALUEFACTOR"] =;
                    dr["UNLCONTRACTSYMBOL"] = pUnderlying.PfCode;
                    dr["VOLATILITY"] = (decimal)(pOpt.v);
                    //
                    /* RISKVALUE 1 à 16 et Delta*/
                    // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
                    FillRiskArrayDataRow(dr, pOpt.ra, pCompositeDelta);
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANARRAY_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANARRAY_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANARRAY_H for Asset: Ctr Id({0}), FMat({1}, OMat({2}), P/C({3}), Strk({4})", pIdIMSPANCONTRACT_H, futMaturity, optMaturity, putCall, pStrikePrice), e);
                }
            }
            return idIMSPANARRAY_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANGRPCOMB_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pCcDef"></param>
        /// <returns></returns>
        private int Insert_IMSPANGRPCOMB_H(int pIdIMSPAN_H, CcDef pCcDef)
        {
            int idIMSPANGRPCOMB_H = 0;
            if (pCcDef != default(CcDef))
            {
                string combGroupCode = "ALL";
                if ((pCcDef.group != null) && (pCcDef.group.Count() > 0))
                {
                    // Prendre le premier groupe
                    combGroupCode = pCcDef.group.Where(g => g.id == pCcDef.group.Min(mg => mg.id)).First().aVal;
                }
                if (m_CcGroup.TryGetValue(combGroupCode, out idIMSPANGRPCOMB_H) == false)
                {
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANGRPCOMB_H(Cs, pIdIMSPAN_H, combGroupCode);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPAN_H"] = pIdIMSPAN_H;
                        dr["COMBINEDGROUPCODE"] = combGroupCode;
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                        dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        //
                        idIMSPANGRPCOMB_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANGRPCOMB_H"]);
                        m_CcGroup.Add(combGroupCode, idIMSPANGRPCOMB_H);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANGRPCOMB_H for Combined Commodity Group: {0}", combGroupCode), e);
                    }
                }
            }
            return idIMSPANGRPCOMB_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANGRPCTR_H et retourne son Id
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pIdIMSPANGRPCOMB_H"></param>
        /// <param name="pCcDef"></param>
        /// <returns></returns>
        private int Insert_IMSPANGRPCTR_H(int pIdIMSPAN_H, int pIdIMSPANGRPCOMB_H, CcDef pCcDef)
        {
            int idIMSPANGRPCTR_H = 0;
            if (pCcDef != default(CcDef))
            {
                string combComCode = pCcDef.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANGRPCTR_H(Cs, pIdIMSPAN_H, combComCode);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPAN_H"] = pIdIMSPAN_H;
                    dr["COMBCOMCODE"] = combComCode;
                    //
                    //Non présent: dr["ALGORITHMCODE"] =;
                    if (pCcDef.cmbMethSpecified)
                    {
                        dr["COMBMARGININGMTH"] = pCcDef.cmbMeth.ToString();
                    }
                    /* Delivery (Spot) Charge Method Code */
                    dr["DELIVERYCHARGEMETH"] = (((pCcDef.spotRate != default(spotRate[])) && (pCcDef.spotRate.Count() > 0)) ? "10" : "01");
                    dr["DESCRIPTION"] = pCcDef.name;
                    //Non présent: dr["FUTVALUATIONMETHOD"] =;
                    dr["HEDGERADJFACTOR"] = 1;
                    dr["HEDGERITOMRATIO"] = FindPBRate("H", pCcDef.adjRate);
                    dr["IDC"] = pCcDef.currency;
                    dr["IDC_PB"] = pCcDef.currency;
                    //Non présent: dr["IDIMSPANEXCHANGE_H"] =;
                    dr["IDIMSPANGRPCOMB_H"] = pIdIMSPANGRPCOMB_H;
                    dr["INTERSPREADMETHOD"] = ((pCcDef.InterTiers.Count() > 0) ? "10" : "01");
                    dr["INTRASPREADMETHOD"] = ((pCcDef.IntraTiers.Count() > 0) ? "10" : "01");
                    dr["ISOPTIONVALUELIMIT"] = (pCcDef.capAnovSpecified && pCcDef.capAnov);
                    dr["MEMBERADJFACTOR"] = 1;
                    dr["MEMBERITOMRATIO"] = FindPBRate("M", pCcDef.adjRate);
                    dr["NBOFDELIVERYMONTH"] = (pCcDef.spotRate != null) ? pCcDef.spotRate.Count() : 0;
                    // PM 20180828 [XXXXX] RiskExponent forcé à 0 car les valeurs du fichier en tiennent déjà compte
                    //dr["RISKEXPONENT"] = (pCcDef.riskExponentSpecified ? pCcDef.riskExponent : 0);
                    dr["RISKEXPONENT"] = 0;
                    /* Short Option Minimum Charge Rate */
                    if (pCcDef.SomTiers.Count() > 0)
                    {
                        IEnumerable<rate> somRate = pCcDef.SomTiers.First().Rate;
                        if (somRate.Count() > 0)
                        {
                            dr["SOMCHARGERATE"] = somRate.First().val;
                        }
                    }
                    /* Short Option Minimum Calculation Method */
                    if (StrFunc.IsFilled(pCcDef.somMeth))
                    {
                        switch (pCcDef.somMeth.ToUpper())
                        {
                            case "MAX":
                                dr["SOMMETHOD"] = "1";
                                break;
                            case "GROSS":
                                dr["SOMMETHOD"] = "2";
                                break;
                        }
                    }
                    dr["SPECULATADJFACTOR"] = 1;
                    dr["SPECULATITOMRATIO"] = FindPBRate("S", pCcDef.adjRate);
                    //Non présent: dr["STRATEGYSPREADMETH"] =;
                    /* Weighted Futures Price Risk Calculation Method */
                    if (StrFunc.IsFilled(pCcDef.wfprMeth))
                    {
                        switch (pCcDef.wfprMeth.ToUpper())
                        {
                            case "NORMAL":
                                dr["WEIGHTEDRISKMETHOD"] = "1";
                                break;
                            case "SCANRANGE_CAP":
                                dr["WEIGHTEDRISKMETHOD"] = "2";
                                break;
                            case "SCANRANGE":
                                dr["WEIGHTEDRISKMETHOD"] = "3";
                                break;
                        }
                    }
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    //
                    idIMSPANGRPCTR_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANGRPCTR_H"]);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANGRPCTR_H for Combined Commodity: {0}", combComCode), e);
                }
            }
            return idIMSPANGRPCTR_H;
        }

        /// <summary>
        /// Mise à jour de l'id du groupe de contrat sur les contrats
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pCcDef"></param>
        /// <returns></returns>
        private int Update_IMSPANCONTRACT_H(int pIdIMSPANGRPCTR_H, CcDef pCcDef)
        {
            int rowUpdated = 0;
            if (pCcDef != default(CcDef))
            {
                IEnumerable<PfLink> pfLinkImported = pCcDef.pfLink.Intersect(m_ContractImported, m_PfLinkComparer);
                if (pfLinkImported.Count() > 0)
                {
                    string combComCode = pCcDef.cc;
                    try
                    {
                        string sqlQuery = String.Format(@"
                        update IMSPANCONTRACT_H
                           set IDIMSPANGRPCTR_H = {0}
                         where ", pIdIMSPANGRPCTR_H);

                        bool isFirstLoop = true;
                        foreach (PfLink pfl in pfLinkImported)
                        {
                            Exchange exch = m_currentClearingOrg.exchange.FirstOrDefault(e => e.exch == pfl.exch);
                            if ((exch != default(Exchange)) && (exch.IdIMSPANEXCHANGE_H != 0))
                            {
                                if (isFirstLoop)
                                {
                                    isFirstLoop = false;
                                }
                                else
                                {
                                    sqlQuery += " or ";
                                }
                                sqlQuery += String.Format("((IMSPANCONTRACT_H.IDIMSPANEXCHANGE_H = {0}) and (IMSPANCONTRACT_H.PFID = {1}) and (IMSPANCONTRACT_H.CONTRACTSYMBOL = '{2}') and (IMSPANCONTRACT_H.CONTRACTTYPE = '{3}'))",
                                    exch.IdIMSPANEXCHANGE_H, pfl.pfId, pfl.pfCode, pfl.pfType);

                            }
                        }
                        if (false == isFirstLoop)
                        {
                            rowUpdated = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlQuery);
                            //
                            sqlQuery = @"
                            update IMSPANCONTRACT_H
                               set DELTASCALINGFACTOR = {4}
                             where (IMSPANCONTRACT_H.IDIMSPANEXCHANGE_H = '{0}')
                               and (IMSPANCONTRACT_H.PFID = {1})
                               and (IMSPANCONTRACT_H.CONTRACTSYMBOL = '{2}')
                               and (IMSPANCONTRACT_H.CONTRACTTYPE = '{3}')";
                            foreach (PfLink pfl in pfLinkImported)
                            {
                                if (StrFunc.IsFilled(pfl.sc) && decimal.TryParse(pfl.sc, out decimal deltaScalingFactor))
                                {
                                    if (deltaScalingFactor != 1)
                                    {
                                        Exchange exch = m_currentClearingOrg.exchange.FirstOrDefault(e => e.exch == pfl.exch);
                                        if ((exch != default(Exchange)) && (exch.IdIMSPANEXCHANGE_H != 0))
                                        {
                                            string formatedSqlQuery = String.Format(sqlQuery, exch.IdIMSPANEXCHANGE_H, pfl.pfId, pfl.pfCode, pfl.pfType, pfl.sc);
                                            DataHelper.ExecuteNonQuery(Cs, CommandType.Text, formatedSqlQuery);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Update IMSPANCONTRACT_H for Combined Commodity: {0}", combComCode), e);
                    }
                }
            }
            return rowUpdated;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANDLVMONTH_H
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pSpot"></param>
        private void Insert_IMSPANDLVMONTH_H(int pIdIMSPANGRPCTR_H, spotRate pSpot)
        {
            if (pSpot != default(spotRate))
            {
                string mmy = pSpot.pe;
                if (StrFunc.IsFilled(mmy))
                {
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANDLVMONTH_H(Cs, pIdIMSPANGRPCTR_H, mmy);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                        dr["MATURITYMONTHYEAR"] = mmy;
                        //
                        if (StrFunc.IsFilled(pSpot.sprd) && decimal.TryParse(pSpot.sprd, out decimal sprd))
                        {
                            // RD 20210609 [25780] Pour éviter l'erreur de dépassement de capacité "out of range", ignorer les zéros après le point décimal
                            // La colonne CONSUMEDCHARGERATE est de type numeric(10,0)
                            string sSprd = pSpot.sprd.TrimEnd('0').TrimEnd('.');
                            if (decimal.TryParse(sSprd, out sprd))
                                dr["CONSUMEDCHARGERATE"] = sprd;
                        }
                        //Non présent: dr["DELTASIGN"] =;
                        dr["MONTHNUMBER"] = pSpot.r;
                        if (StrFunc.IsFilled(pSpot.outr) && decimal.TryParse(pSpot.outr, out decimal outr))
                        {
                            // RD 20210609 [25780] Pour éviter l'erreur de dépassement de capacité "out of range", ignorer les zéros après le point décimal
                            // La colonne REMAINCHARGERATE est de type numeric(10,0)
                            string sOutr = pSpot.outr.TrimEnd('0').TrimEnd('.');
                            if (decimal.TryParse(sOutr, out outr))
                                dr["REMAINCHARGERATE"] = outr;
                        }
                        //
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANDLVMONTH_H for Combined Commodity Id: {0} and Maturity: {1}", pIdIMSPANGRPCTR_H, mmy), e);
                    }
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANTIER_H
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pTier"></param>
        /// <param name="pTierType"></param>
        private void Insert_IMSPANTIER_H(int pIdIMSPANGRPCTR_H, Tier pTier, string pTierType)
        {
            if (pTier != default(Tier))
            {
                if (int.TryParse(pTier.tn, out int tierNumber))
                {
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANTIER_H(Cs, pIdIMSPANGRPCTR_H, pTierType, tierNumber);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                        dr["SPREADTYPE"] = pTierType;
                        dr["TIERNUMBER"] = tierNumber;
                        //
                        if (pTier.ePe != null)
                        {
                            dr["ENDINGMONTHYEAR"] = pTier.ePe;
                        }
                        //Non présent: dr["ENDTIERNUMBER"] =;
                        if (pTier.Rate.Count() > 0)
                        {
                            dr["SOMCHARGERATE"] = pTier.Rate.First().val;
                        }
                        if (pTier.sPe != null)
                        {
                            dr["STARTINGMONTHYEAR"] = pTier.sPe;
                        }
                        //Non présent: dr["STARTTIERNUMBER"] =;
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANTIER_H for Combined Commodity Id: {0}, Spread Type: {1} and Tier Number: {2}", pIdIMSPANGRPCTR_H, pTierType, tierNumber), e);
                    }
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTRASPR_H
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pSpread"></param>
        private int Insert_IMSPANINTRASPR_H(int pIdIMSPANGRPCTR_H, DSpread pSpread)
        {
            int idIMSPANINTRASPR_H = 0;
            if (pSpread != default(DSpread))
            {
                if (int.TryParse(pSpread.spread, out int priority))
                {
                    string spreadType = ((pSpread.TLeg.Count() > 0) ? "T" : "S");
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANINTRASPR_H(Cs, pIdIMSPANGRPCTR_H, spreadType, priority);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                        dr["SPREADPRIORITY"] = priority;
                        dr["SPREADTYPE"] = spreadType;
                        //
                        dr["CHARGERATE"] = (pSpread.RateValue / 100);
                        switch (spreadType)
                        {
                            case "T":   /* Tier */
                                dr["NUMBEROFLEG"] = pSpread.TLeg.Count();
                                break;
                            case "S":   /* Serie */
                                dr["NUMBEROFLEG"] = pSpread.PLeg.Count();
                                break;
                            default:
                                dr["NUMBEROFLEG"] = 0;
                                break;
                        }
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                        dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        //
                        idIMSPANINTRASPR_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANINTRASPR_H"]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTRASPR_H for Spread: CC Id({0}), Priority({1}, Type({2})", pIdIMSPANGRPCTR_H, priority, spreadType), e);
                    }
                }
            }
            return idIMSPANINTRASPR_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTRALEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTRASPR_H"></param>
        /// <param name="pPLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTRALEG_H(int pIdIMSPANINTRASPR_H, pLeg pPLeg, int pLegNumber)
        {
            if (pPLeg != default(pLeg))
            {
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTRALEG_H(Cs, pIdIMSPANINTRASPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTRASPR_H"] = pIdIMSPANINTRASPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["DELTAPERSPREAD"] = pPLeg.i;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["LEGSIDE"] = pPLeg.rs;
                    dr["MATURITYMONTHYEAR"] = pPLeg.pe;
                    //Non présent: dr["TIERNUMBER"] =;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTRALEG_H for Leg: Spread Id({0}), Leg Number({1})", pIdIMSPANINTRASPR_H, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTRALEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTRASPR_H"></param>
        /// <param name="pTLeg"></param>
        /// <param name="pLegNumber"></param>
        /// PM 20160920 [22436] Ajout alimentation de la colonne TIERNUMBER
        private void Insert_IMSPANINTRALEG_H(int pIdIMSPANINTRASPR_H, tLeg pTLeg, int pLegNumber)
        {
            if (pTLeg != default(tLeg))
            {
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTRALEG_H(Cs, pIdIMSPANINTRASPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTRASPR_H"] = pIdIMSPANINTRASPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["DELTAPERSPREAD"] = pTLeg.i;
                    //dr["IDIMSPANTIER_H"] = ;
                    dr["LEGSIDE"] = pTLeg.rs;
                    //Non présent: dr["MATURITYMONTHYEAR"] =;
                    dr["TIERNUMBER"] = pTLeg.tn;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTRALEG_H for Leg: Spread Id({0}), Leg Number({1})", pIdIMSPANINTRASPR_H, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERSPR_H
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pSpread"></param>
        /// <param name="pSpreadGroupType"></param>
        private int Insert_IMSPANINTERSPR_H(int pIdIMSPAN_H, DSpread pSpread, string pSpreadGroupType)
        {
            int idIMSPANINTERSPR_H = 0;
            if (pSpread != default(DSpread))
            {
                if (int.TryParse(pSpread.spread, out int priority))
                {
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANINTERSPR_H(Cs, pIdIMSPAN_H, "ALL", priority, pSpreadGroupType);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPAN_H"] = pIdIMSPAN_H;
                        dr["SPREADPRIORITY"] = priority;
                        dr["COMBINEDGROUPCODE"] = "ALL";
                        //
                        dr["CREDITCALCMETHOD"] = pSpread.chargeMeth;
                        dr["CREDITRATE"] = pSpread.RateValue;
                        //Non présent: dr["ELIGIBILITYCODE"] =;
                        dr["INTERSPREADMETHOD"] = "D";
                        dr["ISCDTRATESEPARATED"] = (pSpread.RpLeg.Count() > 0);
                        //Non présent: dr["MINNUMBEROFLEG"] =;
                        // PM 20161003 [22436] Ajout alimentation de NUMBEROFLEG
                        if (pSpread.Items1 != null)
                        {
                            dr["NUMBEROFLEG"] = pSpread.Items1.Count();
                        }
                        //Non présent: dr["OFFSETRATE"] =;
                        dr["SPREADGROUPTYPE"] = pSpreadGroupType;
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                        dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        //
                        idIMSPANINTERSPR_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANINTERSPR_H"]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERSPR_H for Spread: Span Id({0}), Priority({1})", pIdIMSPAN_H, priority), e);
                    }
                }
            }
            return idIMSPANINTERSPR_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERSPR_H
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pSpread"></param>
        /// <param name="pSpreadGroupType"></param>
        private int Insert_IMSPANINTERSPR_H(int pIdIMSPAN_H, SSpread pSpread, string pSpreadGroupType)
        {
            int idIMSPANINTERSPR_H = 0;
            if (pSpread != default(SSpread))
            {
                if (int.TryParse(pSpread.spread, out int priority))
                {
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANINTERSPR_H(Cs, pIdIMSPAN_H, "ALL", priority, pSpreadGroupType);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPAN_H"] = pIdIMSPAN_H;
                        dr["SPREADPRIORITY"] = priority;
                        dr["COMBINEDGROUPCODE"] = "ALL";
                        //
                        //Non présent: dr["CREDITCALCMETHOD"] =;
                        //Non présent: dr["CREDITRATE"] =;
                        // PM 20180830 [XXXXX] Ajout lecture Credit Rate
                        if ((pSpread.rate != default(rate[])) && (pSpread.rate.Count() > 0))
                        {
                            dr["CREDITRATE"] = (decimal)(pSpread.rate[0].val) * 100;
                        }
                        //Non présent: dr["ELIGIBILITYCODE"] =;
                        dr["INTERSPREADMETHOD"] = "S";
                        //Non présent: dr["ISCDTRATESEPARATED"] =;
                        if (StrFunc.IsFilled(pSpread.numLegsReq))
                        {
                            dr["MINNUMBEROFLEG"] = pSpread.numLegsReq;
                        }
                        else
                        {
                            // PM 20180830 [XXXXX] Ajout valeur 2 par defaut pour Min Number of Leg
                            dr["MINNUMBEROFLEG"] = 2;
                        }
                        //Non présent: dr["NUMBEROFLEG"] =;
                        //Non présent: dr["OFFSETRATE"] =;
                        dr["SPREADGROUPTYPE"] = pSpreadGroupType;
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                        dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        //
                        idIMSPANINTERSPR_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANINTERSPR_H"]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERSPR_H for Spread: Span Id({0}), Priority({1})", pIdIMSPAN_H, priority), e);
                    }
                }
            }
            return idIMSPANINTERSPR_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERSPR_H
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <param name="pSpread"></param>
        /// <param name="pSpreadGroupType"></param>
        private int Insert_IMSPANINTERSPR_H(int pIdIMSPAN_H, clearSpread pSpread, string pSpreadGroupType)
        {
            int idIMSPANINTERSPR_H = 0;
            if (pSpread != default(clearSpread))
            {
                if (int.TryParse(pSpread.spread, out int priority))
                {
                    try
                    {
                        QueryParameters qryParameters = GetQueryParameters_IMSPANINTERSPR_H(Cs, pIdIMSPAN_H, "ALL", priority, pSpreadGroupType);
                        DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        DataRow dr = SetDataRow(dt);
                        //
                        dr["IDIMSPAN_H"] = pIdIMSPAN_H;
                        dr["SPREADPRIORITY"] = priority;
                        dr["COMBINEDGROUPCODE"] = "ALL";
                        //
                        //Non présent: dr["CREDITCALCMETHOD"] =;
                        //Non présent: dr["CREDITRATE"] =;
                        //Non présent: dr["ELIGIBILITYCODE"] =;
                        dr["INTERSPREADMETHOD"] = "01";
                        dr["ISCDTRATESEPARATED"] = true;
                        //Non présent: dr["MINNUMBEROFLEG"] =;
                        //Non présent: dr["NUMBEROFLEG"] =;
                        //Non présent: dr["OFFSETRATE"] =;
                        dr["SPREADGROUPTYPE"] = pSpreadGroupType;
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                        dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        //
                        idIMSPANINTERSPR_H = Convert.ToInt32(dt.Rows[0]["IDIMSPANINTERSPR_H"]);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERSPR_H for Spread: Span Id({0}), Priority({1})", pIdIMSPAN_H, priority), e);
                    }
                }
            }
            return idIMSPANINTERSPR_H;
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERLEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTERLEG_H(int pIdIMSPANINTERSPR_H, int pIdIMSPANGRPCTR_H, pLeg pLeg, int pLegNumber)
        {
            if (pLeg != default(pLeg))
            {
                string combinedCommodity = pLeg.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTERLEG_H(Cs, pIdIMSPANINTERSPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTERSPR_H"] = pIdIMSPANINTERSPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["COMBCOMCODE"] = combinedCommodity;
                    //Non présent: dr["CREDITRATE"] =;
                    decimal deltaPerSpread = 1;
                    if (StrFunc.IsFilled(pLeg.i) && decimal.TryParse(pLeg.i, out deltaPerSpread))
                    {
                        decimal.TryParse(pLeg.i, out deltaPerSpread);
                    }
                    dr["DELTAPERSPREAD"] = deltaPerSpread;
                    //Non présent: dr["EXCHANGEACRONYM"] =;
                    dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["ISREQUIRED"] = true;
                    dr["ISTARGET"] = false;
                    dr["LEGSIDE"] = pLeg.rs;
                    dr["MATURITYMONTHYEAR"] = pLeg.pe;
                    //Non présent: dr["TIERNUMBER"] =;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERLEG_H for Leg: Spread Id({0}), CC({1}), Leg Number({2})", pIdIMSPANINTERSPR_H, combinedCommodity, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERLEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTERLEG_H(int pIdIMSPANINTERSPR_H, int pIdIMSPANGRPCTR_H, RpLeg pLeg, int pLegNumber)
        {
            if (pLeg != default(RpLeg))
            {
                string combinedCommodity = pLeg.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTERLEG_H(Cs, pIdIMSPANINTERSPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTERSPR_H"] = pIdIMSPANINTERSPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["COMBCOMCODE"] = combinedCommodity;
                    dr["CREDITRATE"] = pLeg.RateValue;
                    decimal deltaPerSpread = 1;
                    if (StrFunc.IsFilled(pLeg.i) && decimal.TryParse(pLeg.i, out deltaPerSpread))
                    {
                        decimal.TryParse(pLeg.i, out deltaPerSpread);
                    }
                    dr["DELTAPERSPREAD"] = deltaPerSpread;
                    //Non présent: dr["EXCHANGEACRONYM"] =;
                    dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["ISREQUIRED"] = true;
                    dr["ISTARGET"] = false;
                    dr["LEGSIDE"] = pLeg.rs;
                    //Non présent: dr["MATURITYMONTHYEAR"];
                    if (pLeg.btNumSpecified)
                    {
                        dr["TIERNUMBER"] = pLeg.btNum;
                    }
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERLEG_H for Leg: Spread Id({0}), CC({1}), Leg Number({2})", pIdIMSPANINTERSPR_H, combinedCommodity, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERLEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTERLEG_H(int pIdIMSPANINTERSPR_H, int pIdIMSPANGRPCTR_H, sLeg pLeg, int pLegNumber)
        {
            if (pLeg != default(sLeg))
            {
                string combinedCommodity = pLeg.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTERLEG_H(Cs, pIdIMSPANINTERSPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTERSPR_H"] = pIdIMSPANINTERSPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["COMBCOMCODE"] = combinedCommodity;
                    //Non présent: dr["CREDITRATE"] =;
                    decimal deltaPerSpread = 1;
                    if (StrFunc.IsFilled(pLeg.i) && decimal.TryParse(pLeg.i, out deltaPerSpread))
                    {
                        decimal.TryParse(pLeg.i, out deltaPerSpread);
                    }
                    dr["DELTAPERSPREAD"] = deltaPerSpread;
                    //Non présent: dr["EXCHANGEACRONYM"] =;
                    dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["ISREQUIRED"] = (!pLeg.isRequiredSpecified || pLeg.isRequired);
                    dr["ISTARGET"] = pLeg.isTarget;
                    //Non présent: dr["LEGSIDE"] =;
                    //Non présent: dr["TIERNUMBER"] =;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERLEG_H for Leg: Spread Id({0}), CC({1}), Leg Number({2})", pIdIMSPANINTERSPR_H, combinedCommodity, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERLEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTERLEG_H(int pIdIMSPANINTERSPR_H, int pIdIMSPANGRPCTR_H, tLeg pLeg, int pLegNumber)
        {
            if (pLeg != default(tLeg))
            {
                string combinedCommodity = pLeg.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTERLEG_H(Cs, pIdIMSPANINTERSPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTERSPR_H"] = pIdIMSPANINTERSPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["COMBCOMCODE"] = combinedCommodity;
                    //Non présent: dr["CREDITRATE"] =;
                    decimal deltaPerSpread = 1;
                    if (StrFunc.IsFilled(pLeg.i) && decimal.TryParse(pLeg.i, out deltaPerSpread))
                    {
                        decimal.TryParse(pLeg.i, out deltaPerSpread);
                    }
                    dr["DELTAPERSPREAD"] = deltaPerSpread;
                    //Non présent: dr["EXCHANGEACRONYM"] =;
                    dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["ISREQUIRED"] = true;
                    dr["ISTARGET"] = false;
                    dr["LEGSIDE"] = pLeg.rs;
                    dr["TIERNUMBER"] = pLeg.tn;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERLEG_H for Leg: Spread Id({0}), CC({1}), Leg Number({2})", pIdIMSPANINTERSPR_H, combinedCommodity, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERLEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTERLEG_H(int pIdIMSPANINTERSPR_H, int pIdIMSPANGRPCTR_H, HomeLeg pLeg, int pLegNumber)
        {
            if (pLeg != default(HomeLeg))
            {
                string combinedCommodity = pLeg.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTERLEG_H(Cs, pIdIMSPANINTERSPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTERSPR_H"] = pIdIMSPANINTERSPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["COMBCOMCODE"] = combinedCommodity;
                    dr["CREDITRATE"] = pLeg.RateValue;
                    decimal deltaPerSpread = 1;
                    if (StrFunc.IsFilled(pLeg.i) && decimal.TryParse(pLeg.i, out deltaPerSpread))
                    {
                        decimal.TryParse(pLeg.i, out deltaPerSpread);
                    }
                    dr["DELTAPERSPREAD"] = deltaPerSpread;
                    //Non présent: dr["EXCHANGEACRONYM"] =;
                    dr["IDIMSPANGRPCTR_H"] = pIdIMSPANGRPCTR_H;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["ISREQUIRED"] = true;
                    dr["ISTARGET"] = false;
                    dr["LEGSIDE"] = pLeg.rs;
                    dr["TIERNUMBER"] = pLeg.tn;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERLEG_H for Leg: Spread Id({0}), CC({1}), Leg Number({2})", pIdIMSPANINTERSPR_H, combinedCommodity, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans IMSPANINTERLEG_H
        /// </summary>
        /// <param name="pIdIMSPANINTERSPR_H"></param>
        /// <param name="pLeg"></param>
        /// <param name="pLegNumber"></param>
        private void Insert_IMSPANINTERLEG_H(int pIdIMSPANINTERSPR_H, awayLeg pLeg, int pLegNumber)
        {
            if (pLeg != default(awayLeg))
            {
                string combinedCommodity = pLeg.cc;
                try
                {
                    QueryParameters qryParameters = GetQueryParameters_IMSPANINTERLEG_H(Cs, pIdIMSPANINTERSPR_H, pLegNumber);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr = SetDataRow(dt);
                    //
                    dr["IDIMSPANINTERSPR_H"] = pIdIMSPANINTERSPR_H;
                    dr["LEGNUMBER"] = pLegNumber;
                    //
                    dr["COMBCOMCODE"] = combinedCommodity;
                    //Non présent: dr["CREDITRATE"] =;
                    decimal deltaPerSpread = 1;
                    if (StrFunc.IsFilled(pLeg.i) && decimal.TryParse(pLeg.i, out deltaPerSpread))
                    {
                        decimal.TryParse(pLeg.i, out deltaPerSpread);
                    }
                    dr["DELTAPERSPREAD"] = deltaPerSpread;
                    dr["EXCHANGEACRONYM"] = pLeg.ec;
                    //Non présent: dr["IDIMSPANGRPCTR_H"] =;
                    //Non présent: dr["IDIMSPANTIER_H"] =;
                    dr["ISREQUIRED"] = true;
                    dr["ISTARGET"] = false;
                    dr["LEGSIDE"] = pLeg.rs;
                    dr["TIERNUMBER"] = pLeg.tn;
                    //
                    DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert IMSPANINTERLEG_H for Leg: Spread Id({0}), CC({1}), Leg Number({2})", pIdIMSPANINTERSPR_H, combinedCommodity, pLegNumber), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans QUOTE_ETD_H
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdm"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pFut"></param>
        /// <param name="pCompositeDelta"></param>
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private void Insert_QUOTE_ETD_H(int pIdAsset, int pIdM, decimal? pPrice, string pCurrency, NonOptionContract pFut, decimal pCompositeDelta)
        {
            Insert_QUOTE_ETD_H(pIdAsset, pIdM, m_dtFile, QuotationSideEnum.OfficialClose, pPrice, pCurrency, pFut, pCompositeDelta);
        }
        /// <summary>
        /// Insère un enregistrement dans QUOTE_ETD_H
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdM"></param>
        /// <param name="pQuoteTime"></param>
        /// <param name="pQuoteSide"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pFut"></param>
        /// <param name="pCompositeDelta"></param>
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private void Insert_QUOTE_ETD_H(int pIdAsset, int pIdM, DateTime pQuoteTime, QuotationSideEnum pQuoteSide, decimal? pPrice, string pCurrency, NonOptionContract pFut, decimal pCompositeDelta)
        {
            if (pFut != default(NonOptionContract))
            {
                try
                {
                    //FI 20181126 [24308] Add Alimentation du paramètre pQuoteSide
                    QueryParameters qryParameters = GetQueryParameters_QUOTE_ETD_H(Cs, m_IdMarketEnv, m_IdValScenario, pIdAsset, m_dtFile, pQuoteSide);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    DataRow dr;
                    bool setValues = false;
                    if (dt.Rows.Count == 0)
                    {
                        dr = dt.NewRow();
                        SetDataRowIns(dr);
                        dt.Rows.Add(dr);
                        setValues = true;
                    }
                    else
                    {
                        dr = dt.Rows[0];
                        // Vérification de ENUM.CUSTOMVALUE
                        string quoteSource = dr["SOURCE"].ToString();
                        if (!IsQuoteOverridable(CSTools.SetCacheOn(Cs), quoteSource))
                        {
                            ArrayList message = new ArrayList();
                            string msgDet = " (source:<b>" + quoteSource + "</b>, rule:NOTOVERRIDABLE)";
                            message.Insert(0, msgDet);
                            message.Insert(0, "LOG-06073");
                            ProcessMapping.LogLogInfo(task.SetErrorWarning, null, message);
                        }
                        else if (((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                                || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                                || ((Convert.IsDBNull(dr["DELTA"]) == false) && (Convert.ToDecimal(dr["DELTA"]) != pCompositeDelta))
                                || (Convert.IsDBNull(dr["IDBC"]) == false)
                                || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                                || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                                || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != m_IdMarketEnv))
                                || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != m_IdValScenario))
                                || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                                || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != pQuoteSide.ToString()))
                                || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                                || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                                || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                                || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                                || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != pQuoteTime))
                                || ((Convert.IsDBNull(dr["TIMETOEXPIRATION"]) == false) && (Convert.ToString(dr["TIMETOEXPIRATION"]) != pFut.t))
                                || (pPrice.HasValue && (Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice.Value))
                                || (pFut.vSpecified && (Convert.IsDBNull(dr["VOLATILITY"]) == false) && (Convert.ToDouble(dr["VOLATILITY"]) != pFut.v))
                                )
                        {
                            SetDataRowUpd(dr);
                            setValues = true;
                        }
                    }
                    //
                    if (setValues)
                    {
                        dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                        dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                        //Non présent: dr["CONTRACTMULTIPLIER"] =;
                        // PM 20180828 [XXXXX] Utilisation du Delta en paramètre
                        //dr["DELTA"] = (decimal)(pFut.d);
                        dr["DELTA"] = pCompositeDelta;
                        //Non présent: dr["EXPIRITYTIME"] =;
                        dr["IDASSET"] = pIdAsset;
                        dr["IDBC"] = DBNull.Value; //Non présent
                        dr["IDC"] = pCurrency;
                        if (pIdM != 0)
                        {
                            dr["IDM"] = pIdM;
                        }
                        dr["IDMARKETENV"] = m_IdMarketEnv;
                        dr["IDVALSCENARIO"] = m_IdValScenario;
                        dr["ISENABLED"] = true;
                        dr["QUOTESIDE"] = pQuoteSide.ToString();
                        dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString(); ;
                        dr["QUOTEUNIT"] = "Price";
                        dr["SOURCE"] = "ClearingOrganization";
                        //Non présent: dr["SPREADVALUE"] =;
                        dr["TIME"] = pQuoteTime;
                        dr["TIMETOEXPIRATION"] = pFut.t;
                        if (pPrice.HasValue)
                        {
                            dr["VALUE"] = pPrice.Value;
                        }
                        if (pFut.vSpecified)
                        {
                            dr["VOLATILITY"] = pFut.v;
                        }
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert QUOTE_ETD_H for Asset Id: {0}", pIdAsset), e);
                }
            }
        }

        /// <summary>
        /// Insère un enregistrement dans QUOTE_ETD_H
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdm"></param>
        /// <param name="pSqlDC"></param>
        /// <param name="pPrice"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pSerie"></param>
        /// <param name="pOpt"></param>
        /// <param name="pCompositeDelta"></param>
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private void Insert_QUOTE_ETD_H(int pIdAsset, int pIdM, decimal? pPrice, string pCurrency, Series pSerie, Opt pOpt, decimal pCompositeDelta)
        {
            if ((pSerie != default(Series)) && (pOpt != default(Opt)))
            {
                try
                {
                    //FI 20181126 [24308] Alimentation du paramètre pQuoteSide
                    QueryParameters qryParameters = GetQueryParameters_QUOTE_ETD_H(Cs, m_IdMarketEnv, m_IdValScenario, pIdAsset, m_dtFile , QuotationSideEnum.OfficialClose);
                    DataTable dt = DataHelper.ExecuteDataTable(Cs, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    // PM 20180822 [XXXXX] Ne pas appeler SetDataRow car sinon setValues sera toujours false puisque dt.Rows.Count toujours différent de 0
                    //DataRow dr = SetDataRow(dt);
                    DataRow dr;
                    bool setValues = false;
                    if (dt.Rows.Count == 0)
                    {
                        dr = dt.NewRow();
                        SetDataRowIns(dr);
                        dt.Rows.Add(dr);
                        setValues = true;
                    }
                    else
                    {
                        dr = dt.Rows[0];
                        // Vérification de ENUM.CUSTOMVALUE
                        string quoteSource = dr["SOURCE"].ToString();
                        if (!IsQuoteOverridable(CSTools.SetCacheOn(Cs), quoteSource))
                        {
                            ArrayList message = new ArrayList();
                            string msgDet = " (source:<b>" + quoteSource + "</b>, rule:NOTOVERRIDABLE)";
                            message.Insert(0, msgDet);
                            message.Insert(0, "LOG-06073");
                            ProcessMapping.LogLogInfo(task.SetErrorWarning, null, message);
                        } 
                        else if (((Convert.IsDBNull(dr["ASSETMEASURE"]) == false) && (Convert.ToString(dr["ASSETMEASURE"]) != AssetMeasureEnum.MarketQuote.ToString()))
                            || (Convert.IsDBNull(dr["CASHFLOWTYPE"]) == false)
                            || ((Convert.IsDBNull(dr["DELTA"]) == false) && (Convert.ToDecimal(dr["DELTA"]) != pCompositeDelta))
                            || (Convert.IsDBNull(dr["IDBC"]) == false)
                            || ((Convert.IsDBNull(dr["IDC"]) == false) && (Convert.ToString(dr["IDC"]) != pCurrency))
                            || ((pIdM != 0) && (Convert.IsDBNull(dr["IDM"]) == false) && (Convert.ToInt32(dr["IDM"]) != pIdM))
                            || ((Convert.IsDBNull(dr["IDMARKETENV"]) == false) && (Convert.ToString(dr["IDMARKETENV"]) != m_IdMarketEnv))
                            || ((Convert.IsDBNull(dr["IDVALSCENARIO"]) == false) && (Convert.ToString(dr["IDVALSCENARIO"]) != m_IdValScenario))
                            || ((Convert.IsDBNull(dr["ISENABLED"]) == false) && (Convert.ToBoolean(dr["ISENABLED"]) != true))
                            || ((Convert.IsDBNull(dr["QUOTESIDE"]) == false) && (Convert.ToString(dr["QUOTESIDE"]) != QuotationSideEnum.OfficialClose.ToString()))
                            || ((Convert.IsDBNull(dr["QUOTETIMING"]) == false) && (Convert.ToString(dr["QUOTETIMING"]) != QuoteTimingEnum.Close.ToString()))
                            || ((Convert.IsDBNull(dr["QUOTEUNIT"]) == false) && (Convert.ToString(dr["QUOTEUNIT"]) != "Price"))
                            || ((Convert.IsDBNull(dr["SOURCE"]) == false) && (Convert.ToString(dr["SOURCE"]) != "ClearingOrganization"))
                            || ((Convert.IsDBNull(dr["SPREADVALUE"]) == false) && (Convert.ToDecimal(dr["SPREADVALUE"]) != 0))
                            || ((Convert.IsDBNull(dr["TIME"]) == false) && (Convert.ToDateTime(dr["TIME"]) != m_dtFile))
                            || ((Convert.IsDBNull(dr["TIMETOEXPIRATION"]) == false) && (Convert.ToString(dr["TIMETOEXPIRATION"]) != pSerie.t))
                            || (pPrice.HasValue && (Convert.IsDBNull(dr["VALUE"]) == false) && (Convert.ToDecimal(dr["VALUE"]) != pPrice.Value))
                            || (pOpt.vSpecified && (Convert.IsDBNull(dr["VOLATILITY"]) == false) && (Convert.ToDouble(dr["VOLATILITY"]) != pOpt.v))
                            )
                        {
                            SetDataRowUpd(dr);
                            setValues = true;
                        }
                    }
                    //
                    if (setValues)
                    {
                        //
                        dr["ASSETMEASURE"] = AssetMeasureEnum.MarketQuote.ToString();
                        dr["CASHFLOWTYPE"] = DBNull.Value; //Non présent
                        //Non présent: dr["CONTRACTMULTIPLIER"] =;
                        // PM 20180828 [XXXXX] Utilisation du Composite Delta
                        //dr["DELTA"] = (decimal)(pOpt.d);
                        dr["DELTA"] = pCompositeDelta;
                        //Non présent: dr["EXPIRITYTIME"] =;
                        dr["IDASSET"] = pIdAsset;
                        dr["IDBC"] = DBNull.Value; //Non présent
                        dr["IDC"] = pCurrency;
                        if (pIdM != 0)
                        {
                            dr["IDM"] = pIdM;
                        }
                        dr["IDMARKETENV"] = m_IdMarketEnv;
                        dr["IDVALSCENARIO"] = m_IdValScenario;
                        dr["ISENABLED"] = true;
                        dr["QUOTESIDE"] = QuotationSideEnum.OfficialClose.ToString();
                        dr["QUOTETIMING"] = QuoteTimingEnum.Close.ToString(); ;
                        dr["QUOTEUNIT"] = "Price";
                        dr["SOURCE"] = "ClearingOrganization";
                        dr["SPREADVALUE"] = 0;
                        dr["TIME"] = m_dtFile;
                        dr["TIMETOEXPIRATION"] = pSerie.t;
                        if (pPrice.HasValue)
                        {
                            dr["VALUE"] = pPrice.Value;
                        }
                        if (pOpt.vSpecified)
                        {
                            dr["VOLATILITY"] = pOpt.v;
                        }
                        //
                        DataHelper.ExecuteDataAdapter(Cs, qryParameters.GetQueryReplaceParameters(), dt);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(StrFunc.AppendFormat("Insert QUOTE_ETD_H for Asset Id: {0}", pIdAsset), e);
                }
            }
        }

        /// <summary>
        /// Insère du prix de référence d'une échéance d'une option sur future
        /// </summary>
        /// <param name="pIdM"></param>
        /// <param name="pSqlDC"></param>
        /// <param name="pSerie"></param>
        /// <param name="pUnderlying"></param>
        /// // PM 20180827 [XXXXX] Gestion de la Base >= 10000 : int pIdDC => SQL_DerivativeContract pSqlDC
        //private void Insert_OptionReferencePrice(int pIdM, int pIdDC, series pSerie, UnderlyingInfo pUnderlying)
        private void Insert_OptionReferencePrice(int pIdM, SQL_DerivativeContract pSqlDC, Series pSerie, UnderlyingInfo pUnderlying)
        {
            if ((pSerie != default(Series))
                && (pUnderlying != default(UnderlyingInfo))
                && (pUnderlying.OptUndNonOptPf != default(NonOptionPF))
                && ((pSerie.refPriceFlag == "Y") || (pSerie.refPriceFlag == "S"))
                && pSerie.refPriceSpecified)
            {
                IDataReader dr = null;
                try
                {
                    QueryParameters qryParameters = GetQueryParametersUnderlyingFuture(Cs, pSqlDC.Id, pSerie.pe);
                    dr = DataHelper.ExecuteReader(Cs, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    if ((null != dr) && (dr.Read()))
                    {
                        int idAsset = Convert.ToInt32(dr["IDASSET"]);
                        string finalSettlementTime = Convert.ToString(dr["FINALSETTLTTIME"]);
                        DateTime quoteTime = m_DtBusinessTime;
                        // FI 20190228 [24559] Pour le contrat 21 O à l'échéance, on recoit la valeur Y et le prix associé n'est pas le prix de clôture de l'asset Future
                        //=> Il est donc décider de mettre systématiquement le prix en OfficialSettlement
                        //QuotationSideEnum quoteSideEnum = (pSerie.refPriceFlag == "S") ? QuotationSideEnum.OfficialSettlement : QuotationSideEnum.OfficialClose;
                        QuotationSideEnum quoteSideEnum = QuotationSideEnum.OfficialSettlement;
                        finalSettlementTime = finalSettlementTime.Trim();
                        if (StrFunc.IsFilled(finalSettlementTime) && (finalSettlementTime.Length == 5))
                        {
                            // FI 20190123 [24468] correction pour charger les minutes   
                            if (int.TryParse(finalSettlementTime.Substring(0, 2), out int hour)
                                && int.TryParse(finalSettlementTime.Substring(3, 2), out int minute))
                            {
                                TimeSpan time = new TimeSpan(hour, minute, 0);
                                quoteTime = m_DtBusinessTime.Date.Add(time);
                            }
                        }
                        // PM 20180827 [XXXXX] Gestion de la Base >= 10000
                        decimal price = (decimal)pSerie.refPrice;
                        if (pSqlDC.InstrumentDen >= 10000)
                        {
                            price *= (pSqlDC.InstrumentDen / 100);
                        }
                        Insert_QUOTE_ETD_H(idAsset, pIdM, quoteTime, quoteSideEnum, price, pUnderlying.OptUndNonOptPf.currency, pUnderlying.OptUndNonOptCtr, (decimal)(pUnderlying.OptUndNonOptCtr.d));
                    }
                }
                catch (Exception) { }
                finally
                {
                    if (null != dr)
                    {
                        dr.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Mise à jour du Contract Multiplier de l'Asset ETD et déclanchement de QuoteHandling
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pContractMultiplier"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        private int UpdateAssetContractMultiplier(int pIdAsset, decimal pContractMultiplier)
        {
            int nbRows = 0;
            if ((pContractMultiplier != 0) && (pIdAsset != 0))
            {
                // PM 20240122[WI822] Rename GetQueryUpdASSET_ETD to GetQueryUpdASSET_ETDMultiplier
                QueryParameters qryUpdateAsset = GetQueryUpdASSET_ETDMultiplier(Cs, pIdAsset, pContractMultiplier, task.Process.UserId, dtStart);
                nbRows = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateAsset.Query, qryUpdateAsset.Parameters.GetArrayDbParameter());
                if (nbRows > 0)
                {
                    // Quote Handling pour l'asset
                    Quote_ETDAsset quoteETD = new Quote_ETDAsset
                    {
                        QuoteTable = Cst.OTCml_TBL.ASSET_ETD.ToString(),
                        idAsset = pIdAsset,
                        action = DataRowState.Modified.ToString(),
                        contractMultiplier = pContractMultiplier,
                        contractMultiplierSpecified = true,
                        timeSpecified = true,
                        time = m_dtFile,
                        isCashFlowsVal = true,
                        isCashFlowsValSpecified = true
                    };

                    MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = Cs };
                    QuotationHandlingMQueue qHMQueue = new QuotationHandlingMQueue(quoteETD, mQueueAttributes);

                    IOTools.SendMQueue(task, qHMQueue, Cst.ProcessTypeEnum.QUOTHANDLING);
                }
            }
            return nbRows;
        }
        
        /// <summary>
        ///  Mise à jour du Contract Multiplier du DC et déclanchement de QuoteHandling
        /// </summary>
        /// <param name="pSqlDC"></param>
        /// <param name="pContractMultiplier"></param>
        /// <returns></returns>
        private int UpdateDCContractMultiplier(SQL_DerivativeContract pSqlDC, decimal pContractMultiplier)
        {
            int nbRows = 0;
            if ((pContractMultiplier != 0) && (pSqlDC != default(SQL_DerivativeContract)) && pSqlDC.IsAutoSetting)
            {
                // Mise à jour du Contract Multiplier du DC
                if ((false == pSqlDC.ContractMultiplier.HasValue) || (pSqlDC.ContractMultiplier.HasValue && (pSqlDC.ContractMultiplier.Value != pContractMultiplier)))
                {
                    QueryParameters qryUpdateDC = GetQueryUpdCtrMultDERIVATIVECONTRACT(Cs, pSqlDC.Id, pContractMultiplier, task.Process.UserId, dtStart);
                    nbRows = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, qryUpdateDC.Query, qryUpdateDC.Parameters.GetArrayDbParameter());
                    if (nbRows > 0)
                    {
                        // Quote Handling pour le contract
                        Quote_ETDAsset quoteETD = new Quote_ETDAsset
                        {
                            QuoteTable = Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString(),
                            idDC = pSqlDC.Id,
                            idDCSpecified = true,
                            action = DataRowState.Modified.ToString(),
                            contractMultiplier = pContractMultiplier,
                            contractMultiplierSpecified = true,
                            timeSpecified = true,
                            time = m_dtFile,
                            isCashFlowsVal = true,
                            isCashFlowsValSpecified = true
                        };

                        MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = Cs };
                        QuotationHandlingMQueue qHMQueue = new QuotationHandlingMQueue(quoteETD, mQueueAttributes);

                        IOTools.SendMQueue(task, qHMQueue, Cst.ProcessTypeEnum.QUOTHANDLING);
                    }
                }
            }
            return nbRows;
        }

        /// <summary>
        /// Mise à jour de l'id des tiers pour les interspreads
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <returns></returns>
        private int Update_IMSPANINTERLEG_H(int pIdIMSPAN_H)
        {
            int rowUpdated;
            try
            {
                // Mise à jour de l'id des tiers pour les interspreads avec une seule échéance
                string sqlQuery = String.Format(@"
                update IMSPANINTERLEG_H
                   set IDIMSPANTIER_H = (select t.IDIMSPANTIER_H
                                           from IMSPANTIER_H t
                                          where (t.IDIMSPANGRPCTR_H = IMSPANINTERLEG_H.IDIMSPANGRPCTR_H)
			                                and (t.SPREADTYPE = 'R')
			  	                            and (t.STARTINGMONTHYEAR = IMSPANINTERLEG_H.MATURITYMONTHYEAR)
				                            and (t.ENDINGMONTHYEAR = t.STARTINGMONTHYEAR))
                 where (MATURITYMONTHYEAR is not null )
                   and exists ( select 1 from IMSPANTIER_H t
                                 inner join IMSPANGRPCTR_H g on (g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H)
		                         inner join IMSPAN_H s on (s.IDIMSPAN_H = g.IDIMSPAN_H)
                                 where (t.IDIMSPANGRPCTR_H = IMSPANINTERLEG_H.IDIMSPANGRPCTR_H)
			                       and (t.SPREADTYPE = 'R')
			                       and (t.STARTINGMONTHYEAR = IMSPANINTERLEG_H.MATURITYMONTHYEAR)
			                       and (t.ENDINGMONTHYEAR = t.STARTINGMONTHYEAR)
			                       and (s.IDIMSPAN_H = {0})
                              )", pIdIMSPAN_H);

                rowUpdated = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlQuery);

                // Mise à jour de l'id des tiers pour les interspreads avec un numéro de tier renseigné
                sqlQuery = String.Format(@"
                update IMSPANINTERLEG_H
                   set IDIMSPANTIER_H = (select t.IDIMSPANTIER_H
                                           from IMSPANTIER_H t
                                          where (t.IDIMSPANGRPCTR_H = IMSPANINTERLEG_H.IDIMSPANGRPCTR_H)
			                                and (t.SPREADTYPE = 'R')
			  	                            and (t.TIERNUMBER = IMSPANINTERLEG_H.TIERNUMBER))
                 where (TIERNUMBER is not null )
                   and exists ( select 1 from IMSPANTIER_H t
                                 inner join IMSPANGRPCTR_H g on (g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H)
		                         inner join IMSPAN_H s on (s.IDIMSPAN_H = g.IDIMSPAN_H)
                                 where (t.IDIMSPANGRPCTR_H = IMSPANINTERLEG_H.IDIMSPANGRPCTR_H)
			                       and (t.SPREADTYPE = 'R')
			                       and (t.TIERNUMBER = IMSPANINTERLEG_H.TIERNUMBER)
			                       and (s.IDIMSPAN_H = {0})
                              )", pIdIMSPAN_H);

                rowUpdated += DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlQuery);
            }
            catch (Exception e)
            {
                throw new Exception(StrFunc.AppendFormat("Update IMSPANINTERLEG_H for Span Id: {0}", pIdIMSPAN_H), e);
            }
            return rowUpdated;
        }

        /// <summary>
        /// Mise à jour de l'id des tiers pour les intraspreads
        /// </summary>
        /// <param name="pIdIMSPAN_H"></param>
        /// <returns></returns>
        /// PM 20160920 [22436] New
        private int Update_IMSPANINTRALEG_H(int pIdIMSPAN_H)
        {
            int rowUpdated;
            try
            {
                // Mise à jour de l'id des tiers pour tous les intraspreads avec un numéro de tier renseigné
                string sqlQuery = String.Format(@"
                update IMSPANINTRALEG_H
                   set IDIMSPANTIER_H = (select t.IDIMSPANTIER_H
                                           from IMSPANTIER_H t
                                          inner join IMSPANINTRASPR_H i on (i.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H)
                                          where (i.IDIMSPANINTRASPR_H = IMSPANINTRALEG_H.IDIMSPANINTRASPR_H)
			                                and (t.SPREADTYPE = 'A')
			  	                            and (t.TIERNUMBER = IMSPANINTRALEG_H.TIERNUMBER))
                 where (TIERNUMBER is not null )
                   and exists ( select 1 from IMSPANTIER_H t
                                 inner join IMSPANINTRASPR_H i on (i.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H)
                                 inner join IMSPANGRPCTR_H g on (g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H)
		                         inner join IMSPAN_H s on (s.IDIMSPAN_H = g.IDIMSPAN_H)
                                 where (i.IDIMSPANINTRASPR_H = IMSPANINTRALEG_H.IDIMSPANINTRASPR_H)
			                       and (t.SPREADTYPE = 'A')
			                       and (t.TIERNUMBER = IMSPANINTRALEG_H.TIERNUMBER)
			                       and (s.IDIMSPAN_H = {0})
                              )", pIdIMSPAN_H);

                rowUpdated = DataHelper.ExecuteNonQuery(Cs, CommandType.Text, sqlQuery);
            }
            catch (Exception e)
            {
                throw new Exception(StrFunc.AppendFormat("Update IMSPANINTRALEG_H for Span Id: {0}", pIdIMSPAN_H), e);
            }
            return rowUpdated;
        }

        /// <summary>
        /// Alimentation d'un DataRow de IMSPANARRAY_H avec les valeurs de risque d'un ra[] (RiskArray)
        /// </summary>
        /// <param name="pDr"></param>
        /// <param name="pRa"></param>
        /// <param name="pCompositeDelta"></param>
        // PM 20180828 [XXXXX] Ajout du Composite Delta en paramètre
        private static void FillRiskArrayDataRow(DataRow pDr, ra[] pRa, decimal pCompositeDelta)
        {
            if ((pDr != default(DataRow)) && (pRa != default(ra[])) && (pRa.Count() > 0))
            {
                ra riskArray = pRa[0];
                if (riskArray != default(ra))
                {
                    // PM 20180828 [XXXXX] Utilistaion du Composite Delta en paramètre
                    //pDr["COMPOSITEDELTA"] = (decimal)(riskArray.d);
                    pDr["COMPOSITEDELTA"] = pCompositeDelta;
                    //
                    if (riskArray.a != default(double[]))
                    {
                        int riskArraySize = riskArray.a.Count();
                        string column = "RISKVALUE";
                        for (int i = 0; i < 16; i += 1)
                        {
                            string columnName = column + (i + 1).ToString();
                            if (i < riskArraySize)
                            {
                                pDr[columnName] = (decimal)(riskArray.a[i]);
                            }
                            else
                            {
                                pDr[columnName] = 0;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Clearing Organisations
        /// </summary>
        /// <param name="pDtCreated"></param>
        /// <param name="pFileFormat"></param>
        /// <param name="pPit"></param>
        // EG 20220221 [XXXXX] Gestion IRQ
        private void ProcessClearingOrg(DateTime pDtCreated, string pFileFormat, pointInTime pPit)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if ((pPit != default(pointInTime)) && (pPit.clearingOrg != default(ClearingOrg[])))
            {
                try
                {
                    if (StrFunc.IsEmpty(pPit.date))
                    {
                        throw new Exception("Current business day is empty.");
                    }
                    else
                    {
                        m_dtFile = new DtFunc().StringyyyyMMddToDateTime(pPit.date);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error while initializing the current business day from 'pointInTime' in file.", e);
                }
                string settlSession = (pPit.isSetl ? "EOD" : "ITD");
                m_DtBusinessTime = (StrFunc.IsFilled(pPit.time) ? new DtFunc().StringToDateTime(pPit.date + pPit.time, "yyyyMMddHHmm") : m_dtFile);
                string businessFunction = (((pPit.businessFunction != default(businessFunction[])) && (pPit.businessFunction.Length > 0)) ? pPit.businessFunction[0].name : "CLR");
                //
                InitDefaultMembers();
                //
                foreach (ClearingOrg clearOrg in pPit.clearingOrg)
                {
                    if (IRQTools.IsIRQRequested(task.Process, task.Process.IRQNamedSystemSemaphore, ref ret))
                        break;

                    m_currentClearingOrg = clearOrg;
                    // Gestion m_IsPriceOnly
                    if (m_IsPriceOnly)
                    {
                        ProcessExchange(clearOrg);
                    }
                    else
                    {
                        DeleteAllRiskParameters(Cs, m_dtFile, m_DtBusinessTime, settlSession, clearOrg.ec);
                        clearOrg.IdIMSPAN_H = Insert_IMSPAN_H(m_dtFile, settlSession, clearOrg.ec, m_DtBusinessTime, pDtCreated, pPit.setlQualifier, pFileFormat, clearOrg.capAnov, businessFunction);
                        ProcessCurrency(clearOrg);
                        ProcessCurConv(clearOrg);
                        ProcessExchange(clearOrg);
                        // S'il existe des marché ayant des trades actifs
                        if ((clearOrg.exchange != null) && (clearOrg.exchange.Any(e => e.IsInTrade)))
                        {
                            ProcessCcDef(clearOrg);
                            ProcessInterSpreads(clearOrg);
                            //
                            // PM 20160920 [22436] Ajout Update_IMSPANINTRALEG_H et déplacement de Update_IMSPANINTERLEG_H qui était dans ProcessInterSpreads
                            Update_IMSPANINTRALEG_H(clearOrg.IdIMSPAN_H);
                            Update_IMSPANINTERLEG_H(clearOrg.IdIMSPAN_H);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres des devises
        /// </summary>
        /// <param name="pClearOrg"></param>
        private void ProcessCurrency(ClearingOrg pClearOrg)
        {
            if ((pClearOrg != default(ClearingOrg)) && (m_SpanRiskFile.definitions != default(definitions)))
            {
                // PM 20180808 [XXXXX] Ajout test pour vérifier qu'il y a des currencyDef
                if ((m_SpanRiskFile.definitions.currencyDef != default(currencyDef[])) && (m_SpanRiskFile.definitions.currencyDef.Count() > 0))
                {
                    foreach (currencyDef curDef in m_SpanRiskFile.definitions.currencyDef)
                    {
                        Insert_IMSPANCURRENCY_H(pClearOrg.IdIMSPAN_H, curDef);
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de conversion des devises
        /// </summary>
        /// <param name="pClearOrg"></param>
        private void ProcessCurConv(ClearingOrg pClearOrg)
        {
            if ((pClearOrg != default(ClearingOrg)) && (pClearOrg.curConv != default(curConv[])))
            {
                foreach (curConv curCnv in pClearOrg.curConv)
                {
                    Insert_IMSPANCURCONV_H(pClearOrg.IdIMSPAN_H, curCnv);
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Exchanges
        /// </summary>
        /// <param name="pClearOrg"></param>
        private void ProcessExchange(ClearingOrg pClearOrg)
        {
            if ((pClearOrg != default(ClearingOrg)) && (pClearOrg.exchange != default(Exchange[])))
            {
                foreach (Exchange exch in pClearOrg.exchange)
                {
                    // Vérification qu'il existe des trades actifs sur le marché
                    exch.IsInTrade = IsExistExchangeInTrade(base.Cs, m_dtFile, exch.exch);
                    if (exch.IsInTrade)
                    {
                        // Gestion m_IsPriceOnly
                        if (false == m_IsPriceOnly)
                        {
                            exch.IdIMSPANEXCHANGE_H = Insert_IMSPANEXCHANGE_H(pClearOrg.IdIMSPAN_H, exch);
                        }
                        //
                        // Lecture de l'IDM du marché
                        exch.Idm = GetIdmFromExchangeAcronym(exch.exch);
                        //
                        ProcessProductFamily(exch);
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Product Family
        /// </summary>
        /// <param name="pExchange"></param>
        private void ProcessProductFamily(Exchange pExchange)
        {
            if (pExchange != default(Exchange))
            {
                if (pExchange.CmbPf != default(CmbPf[]))
                {
                    foreach (CmbPf pf in pExchange.CmbPf)
                    {
                        ProcessComboPf(pExchange.IdIMSPANEXCHANGE_H, pExchange.Idm, pExchange.exch, pf);
                    }
                }
                if (pExchange.futPf != default(FutPf[]))
                {
                    foreach (FutPf pf in pExchange.futPf)
                    {
                        ProcessFuturePf(pExchange.IdIMSPANEXCHANGE_H, pExchange.Idm, pExchange.exch, pf);
                    }
                }
                if (pExchange.OocPf != default(OptionPF[]))
                {
                    foreach (OptionPF pf in pExchange.OocPf)
                    {
                        ProcessOptionPf(pExchange.IdIMSPANEXCHANGE_H, pExchange.Idm, pExchange.exch, "OOC", pf);
                    }
                }
                if (pExchange.ooePf != default(OptionPF[]))
                {
                    foreach (OptionPF pf in pExchange.ooePf)
                    {
                        ProcessOptionPf(pExchange.IdIMSPANEXCHANGE_H, pExchange.Idm, pExchange.exch, "OOE", pf);
                    }
                }
                if (pExchange.oofPf != default(OptionPF[]))
                {
                    foreach (OptionPF pf in pExchange.oofPf)
                    {
                        ProcessOptionPf(pExchange.IdIMSPANEXCHANGE_H, pExchange.Idm, pExchange.exch, "OOF", pf);
                    }
                }
                if (pExchange.oopPf != default(OptionPF[]))
                {
                    foreach (OptionPF pf in pExchange.oopPf)
                    {
                        ProcessOptionPf(pExchange.IdIMSPANEXCHANGE_H, pExchange.Idm, pExchange.exch, "OOP", pf);
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Combo
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pIdm"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pCmbPf"></param>
        private void ProcessComboPf(int pIdIMSPANEXCHANGE_H, int pIdm, string pExchAcr, CmbPf pCmbPf)
        {
            if (pCmbPf != default(CmbPf))
            {
                // Vérification qu'il existe des trades actifs sur le contrat
                // avec vérification par rapport au Cascading
                int? idDCInTrade = GetIdDCInTrade(Cs, m_dtFile, pExchAcr, pCmbPf.pfCode, "F", true);
                pCmbPf.IsInTrade = idDCInTrade.HasValue;
                if (pCmbPf.IsInTrade)
                {
                    undPf und = ((pCmbPf.undPf != default(undPf[])) ? pCmbPf.undPf.FirstOrDefault() : default);
                    UnderlyingInfo underlying = new UnderlyingInfo(m_currentClearingOrg, und);
                    int idIMSPANCONTRACT_H = 0;
                    // Gestion m_IsPriceOnly
                    if (false == m_IsPriceOnly)
                    {
                        idIMSPANCONTRACT_H = Insert_IMSPANCONTRACT_H(pIdIMSPANEXCHANGE_H, pCmbPf);
                    }
                    //
                    PfLink link = new PfLink(pExchAcr, "COMBO", pCmbPf);
                    m_ContractImported.Add(link);
                    //
                    if (pCmbPf.cmb != default(Cmb[]))
                    {
                        SQL_DerivativeContract sqlDC = new SQL_DerivativeContract(Cs, idDCInTrade.Value);
                        // Mise à jour du Contract Multiplier du DC
                        if (sqlDC.IsAutoSetting && pCmbPf.cvfSpecified && (pCmbPf.cvf != 0))
                        {
                            decimal contractMultiplier = (decimal)pCmbPf.cvf;
                            int nbRows = UpdateDCContractMultiplier(sqlDC, contractMultiplier);
                            if (nbRows > 0)
                            {
                                // Recharger le DC
                                sqlDC = new SQL_DerivativeContract(Cs, sqlDC.Id);
                            }
                        }
                        //
                        foreach (Cmb cmb in pCmbPf.cmb)
                        {
                            ProcessCmb(pIdm, sqlDC, idIMSPANCONTRACT_H, pExchAcr, pCmbPf.pfCode, pCmbPf.currency, underlying, cmb);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Futures
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pIdm"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pFutPf"></param>
        private void ProcessFuturePf(int pIdIMSPANEXCHANGE_H, int pIdm, string pExchAcr, FutPf pFutPf)
        {
            if (pFutPf != default(FutPf))
            {
                // Vérification qu'il existe des trades actifs sur le contrat
                // avec vérification par rapport au Cascading
                int? idDCInTrade = GetIdDCInTrade(Cs, m_dtFile, pExchAcr, pFutPf.pfCode, "F", true);
                pFutPf.IsInTrade = idDCInTrade.HasValue;
                if (pFutPf.IsInTrade)
                {
                    UnderlyingInfo underlying = new UnderlyingInfo(m_currentClearingOrg, pFutPf.undPf);
                    int idIMSPANCONTRACT_H = 0;
                    // Gestion m_IsPriceOnly
                    if (false == m_IsPriceOnly)
                    {
                        idIMSPANCONTRACT_H = Insert_IMSPANCONTRACT_H(pIdIMSPANEXCHANGE_H, pFutPf);
                    }
                    //
                    PfLink link = new PfLink(pExchAcr, "FUT", pFutPf);
                    m_ContractImported.Add(link);
                    //
                    if (pFutPf.fut != default(Fut[]))
                    {
                        SQL_DerivativeContract sqlDC = new SQL_DerivativeContract(Cs, idDCInTrade.Value);
                        // Mise à jour du Contract Multiplier du DC
                        if (sqlDC.IsAutoSetting && pFutPf.cvfSpecified && (pFutPf.cvf != 0))
                        {
                            decimal contractMultiplier = (decimal)pFutPf.cvf;
                            int nbRows = UpdateDCContractMultiplier(sqlDC, contractMultiplier);
                            if (nbRows > 0)
                            {
                                // Recharger le DC
                                sqlDC = new SQL_DerivativeContract(Cs, sqlDC.Id);
                            }
                        }
                        //
                        foreach (Fut fut in pFutPf.fut)
                        {
                            ProcessFut(pIdm, sqlDC, idIMSPANCONTRACT_H, pExchAcr, pFutPf.pfCode, pFutPf.currency, underlying, fut);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Cmb (Asset Future)
        /// </summary>
        /// <param name="pIdm"></param>
        /// <param name="pSqlDC"></param>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pCmb"></param>
        private void ProcessCmb( int pIdm, SQL_DerivativeContract pSqlDC, int pIdIMSPANCONTRACT_H, string pExchAcr, string pContractSymbol, string pCurrency, UnderlyingInfo pUnderlying, Cmb pCmb)
        {
            if (pCmb != default(Cmb))
            {
                // Vérification qu'il existe des trades actifs sur l'asset Future
                int? idAsset = GetIdAssetFutureInTrade(Cs, m_dtFile, pExchAcr, pContractSymbol, pCmb.pe, m_MaturityType, true);
                pCmb.IsInTrade = idAsset.HasValue;
                if (pCmb.IsInTrade)
                {
                    // Ajustement du Settlement Price
                    decimal? price = (pCmb.pSpecified) ? AdjustPrice(pSqlDC, (decimal)(pCmb.p)) : (decimal?)null;
                    // PM 20180827 [XXXXX] Gestion de la Base >= 10000
                    if (price.HasValue && pSqlDC.InstrumentDen >= 10000)
                    {
                        price *= (pSqlDC.InstrumentDen / 100);
                    }
                    // PM 20180828 [XXXXX] Ajout recherche du Composite Delta
                    decimal compositeDelta = 1;
                    if ((pCmb.ra != default(ra[])) && (pCmb.ra.Count() > 0))
                    {
                        ra riskArray = pCmb.ra[0];
                        if (riskArray != default(ra))
                        {
                            compositeDelta = (decimal)(riskArray.d);
                        }
                    }
                    // Gestion m_IsPriceOnly
                    if (false == m_IsPriceOnly)
                    {
                        _ = Insert_IMSPANMATURITY_H(pIdIMSPANCONTRACT_H, pUnderlying, pCmb);
                        //
                        // PM 20180828 [XXXXX] ajout compositeDelta comme paramètre
                        Insert_IMSPANARRAY_H(pIdIMSPANCONTRACT_H, idAsset.Value, pUnderlying, price, pCmb, compositeDelta);
                    }
                    // PM 20180828 [XXXXX] ajout compositeDelta comme paramètre
                    Insert_QUOTE_ETD_H(idAsset.Value, pIdm, price, pCurrency, pCmb, compositeDelta);

                    // Mise à jour ContractMultiplier et envoie à Quote Handling si différent de celui du DC
                    if ((pCmb.cvfSpecified) && (pSqlDC.IsAutoSetting))
                    {
                        decimal contractMultiplier = (decimal)pCmb.cvf;
                        if ((false == pSqlDC.ContractMultiplier.HasValue) || (pSqlDC.ContractMultiplier.HasValue && (pSqlDC.ContractMultiplier.Value != contractMultiplier)))
                        {
                            UpdateAssetContractMultiplier(idAsset.Value, (decimal)pCmb.cvf);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Fut (Asset Future)
        /// </summary>
        /// <param name="pIdm"></param>
        /// <param name="pSqlDC"></param>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pFut"></param>
        private void ProcessFut(int pIdm, SQL_DerivativeContract pSqlDC, int pIdIMSPANCONTRACT_H, string pExchAcr, string pContractSymbol, string pCurrency, UnderlyingInfo pUnderlying, Fut pFut)
        {
            if (pFut != default(Fut))
            {
                // Vérification qu'il existe des trades actifs sur l'asset Future
                int? idAsset = GetIdAssetFutureInTrade(Cs, m_dtFile, pExchAcr, pContractSymbol, pFut.pe, m_MaturityType, true);
                pFut.IsInTrade = idAsset.HasValue;
                if (pFut.IsInTrade)
                {
                    // Ajustement du Settlement Price
                    decimal? price = (pFut.pSpecified) ? AdjustPrice(pSqlDC, (decimal)(pFut.p)) : (decimal?)null;
                    // PM 20180827 [XXXXX] Gestion de la Base >= 10000
                    if (price.HasValue && pSqlDC.InstrumentDen >= 10000)
                    {
                        price *= (pSqlDC.InstrumentDen / 100);
                    }
                    // PM 20180828 [XXXXX] Ajout recherche du Composite Delta
                    decimal compositeDelta = 1;
                    if ((pFut.ra != default(ra[])) && (pFut.ra.Count() > 0))
                    {
                        ra riskArray = pFut.ra[0];
                        if (riskArray != default(ra))
                        {
                            compositeDelta = (decimal)(riskArray.d);
                        }
                    }
                    // Gestion m_IsPriceOnly
                    if (false == m_IsPriceOnly)
                    {
                        int idIMSPANMATURITY_H = Insert_IMSPANMATURITY_H(pIdIMSPANCONTRACT_H, pUnderlying, pFut);
                        //
                        // PM 20180828 [XXXXX] ajout compositeDelta comme paramètre
                        Insert_IMSPANARRAY_H(pIdIMSPANCONTRACT_H, idIMSPANMATURITY_H, idAsset.Value, pUnderlying, price, pFut, compositeDelta);
                    }
                    // PM 20180828 [XXXXX] ajout compositeDelta comme paramètre
                    Insert_QUOTE_ETD_H(idAsset.Value, pIdm, price, pCurrency, pFut, compositeDelta);

                    // Mise à jour ContractMultiplier et envoie à Quote Handling si différent de celui du DC
                    if ((pFut.cvfSpecified) && (pSqlDC.IsAutoSetting))
                    {
                        decimal contractMultiplier = (decimal)pFut.cvf;
                        if ((false == pSqlDC.ContractMultiplier.HasValue) || (pSqlDC.ContractMultiplier.HasValue && (pSqlDC.ContractMultiplier.Value != contractMultiplier)))
                        {
                            UpdateAssetContractMultiplier(idAsset.Value, (decimal)pFut.cvf);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Options
        /// </summary>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pIdM"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pOptPf"></param>
        /// <param name="pContractType"></param>
        private void ProcessOptionPf(int pIdIMSPANEXCHANGE_H, int pIdM, string pExchAcr, string pContractType, OptionPF pOptPf)
        {
            if (pOptPf != default(OptionPF))
            {
                // Vérification qu'il existe des trades actifs sur le contrat
                // avec vérification par rapport au Cascading
                int? idDCInTrade = GetIdDCInTrade(Cs, m_dtFile, pExchAcr, pOptPf.pfCode, "O", true);
                pOptPf.IsInTrade = idDCInTrade.HasValue;
                if (pOptPf.IsInTrade)
                {
                    int idIMSPANCONTRACT_H = 0;
                    // Gestion m_IsPriceOnly
                    if (false == m_IsPriceOnly)
                    {
                        idIMSPANCONTRACT_H = Insert_IMSPANCONTRACT_H(pIdIMSPANEXCHANGE_H, pOptPf, pContractType);
                    }
                    //
                    PfLink link = new PfLink(pExchAcr, pContractType, pOptPf);
                    m_ContractImported.Add(link);
                    //
                    if (pOptPf.series != default(Series[]))
                    {
                        SQL_DerivativeContract sqlDC = new SQL_DerivativeContract(Cs, idDCInTrade.Value);
                        // Mise à jour du Contract Multiplier du DC
                        if (sqlDC.IsAutoSetting && pOptPf.cvfSpecified && (pOptPf.cvf != 0))
                        {
                            decimal contractMultiplier = (decimal)pOptPf.cvf;
                            int nbRows = UpdateDCContractMultiplier(sqlDC, contractMultiplier);
                            if (nbRows > 0)
                            {
                                // Recharger le DC
                                sqlDC = new SQL_DerivativeContract(Cs, sqlDC.Id);
                            }
                        }
                        //
                        foreach (Series serie in pOptPf.series)
                        {
                            if (serie != default(Series))
                            {
                                // Vérification qu'il existe des trades actifs sur l'échéance
                                m_QueryExistDCMaturityInTrade.Parameters["DTFILE"].Value = m_dtFile;
                                m_QueryExistDCMaturityInTrade.Parameters["EXCHANGEACRONYM"].Value = pExchAcr;
                                m_QueryExistDCMaturityInTrade.Parameters["CONTRACTSYMBOL"].Value = pOptPf.pfCode;
                                m_QueryExistDCMaturityInTrade.Parameters["CATEGORY"].Value = "O";
                                m_QueryExistDCMaturityInTrade.Parameters["MATURITYMONTHYEAR"].Value = serie.pe;

                                object obj_Dc_Maturity_Trade = DataHelper.ExecuteScalar(Cs, CommandType.Text, m_QueryExistDCMaturityInTrade.Query, m_QueryExistDCMaturityInTrade.Parameters.GetArrayDbParameter());

                                serie.IsInTrade = (obj_Dc_Maturity_Trade != null);
                                if (serie.IsInTrade)
                                {
                                    ProcessSeries(pIdM, sqlDC, idIMSPANCONTRACT_H, pExchAcr, pContractType, pOptPf, serie);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Series (Maturity)
        /// </summary>
        /// <param name="pIdm"></param>
        /// <param name="pSqlDC"></param>
        /// <param name="pIdIMSPANEXCHANGE_H"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pContractType"></param>
        /// <param name="pOptPf"></param>
        /// <param name="pSerie"></param>
        private void ProcessSeries(int pIdm, SQL_DerivativeContract pSqlDC, int pIdIMSPANCONTRACT_H, string pExchAcr, string pContractType, OptionPF pOptPf, Series pSerie)
        {
            if (pSerie != default(Series))
            {
                UnderlyingInfo underlying = new UnderlyingInfo(m_currentClearingOrg, pSerie.undC, pContractType);
                int idIMSPANMATURITY_H = 0;
                // Gestion m_IsPriceOnly
                if (false == m_IsPriceOnly)
                {
                    idIMSPANMATURITY_H = Insert_IMSPANMATURITY_H(pIdIMSPANCONTRACT_H, underlying, pOptPf, pSerie);
                }
                // Gestion prix de référence
                if (pContractType == "OOF")
                {
                    // PM 20180827 [XXXXX] Gestion de la Base >= 10000 : pSqlDC.Id => pSqlDC
                    //Insert_OptionReferencePrice(pIdm, pSqlDC.Id, pSerie, underlying);
                    Insert_OptionReferencePrice(pIdm, pSqlDC, pSerie, underlying);
                }
                // Gestion des assets
                // PM 20180808 [XXXXX] Ajout teste pour vérifier qu'il y a bien des assets de définis dans le fichier
                if ((pSerie.opt != default(Opt[])) && (pSerie.opt.Count() > 0))
                {
                    foreach (Opt serieOpt in pSerie.opt)
                    {
                        ProcessOpt(pIdm, pSqlDC, pIdIMSPANCONTRACT_H, idIMSPANMATURITY_H, pExchAcr, pContractType, underlying, pOptPf, pSerie, serieOpt);
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Opt (Asset)
        /// </summary>
        /// <param name="pIdm"></param>
        /// <param name="pSqlDC"></param>
        /// <param name="pIdIMSPANCONTRACT_H"></param>
        /// <param name="pIdIMSPANMATURITY_H"></param>
        /// <param name="pExchAcr"></param>
        /// <param name="pContractType"></param>
        /// <param name="pUnderlying"></param>
        /// <param name="pOptPf"></param>
        /// <param name="pSerie"></param>
        /// <param name="pOpt"></param>
        private void ProcessOpt(int pIdm, SQL_DerivativeContract pSqlDC, int pIdIMSPANCONTRACT_H, int pIdIMSPANMATURITY_H, string pExchAcr, string pContractType, UnderlyingInfo pUnderlying, OptionPF pOptPf, Series pSerie, Opt pOpt)
        {
            if ((pSerie != default(Series)) && (pOpt != default(Opt)))
            {
                string putCall = ((pOpt.o.ToUpper() == "P") ? "0" : ((pOpt.o.ToUpper() == "C") ? "1" : null));
                decimal? strikePrice = null;
                if (DecFunc.IsDecimal(pOpt.k))
                {
                    // Utilisation de Abs à cause des OOC
                    strikePrice = System.Math.Abs(DecFunc.DecValueFromInvariantCulture(pOpt.k));
                }
                // PM 20180824 [XXXXX] Gestion de la Base >= 10000
                if (strikePrice.HasValue && pSqlDC.InstrumentDen >= 10000)
                {
                    strikePrice *= (pSqlDC.InstrumentDen / 100);
                }
                // Vérification qu'il existe des trades actifs sur l'asset Option
                QueryParameters queryParameters = QueryExistAssetOptionInTrades(Cs, m_MaturityType);
                DataParameters dp = queryParameters.Parameters;
                dp["DTFILE"].Value = m_dtFile;
                dp["EXCHANGEACRONYM"].Value = pExchAcr;
                dp["CONTRACTSYMBOL"].Value = pOptPf.pfCode;
                dp["CATEGORY"].Value = "O";
                dp["MATURITYMONTHYEAR"].Value = pSerie.pe;
                dp["PUTCALL"].Value = putCall;
                dp["STRIKEPRICE"].Value = strikePrice;
                object asset_IDASSET_Option = DataHelper.ExecuteScalar(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

                pOpt.IsInTrade = (asset_IDASSET_Option != null);
                if (pOpt.IsInTrade)
                {
                    int idAsset = Convert.ToInt32(asset_IDASSET_Option);
                    //
                    decimal? price;
                    // PM 20180822 [XXXXX] Gestion cas du price égal au Cabinet Option Value
                    if (pOpt.p == pOpt.val)
                    {
                        decimal contractMultiplier = 0;
                        if (pOpt.cvfSpecified)
                        {
                            contractMultiplier = (decimal)pOpt.cvf;
                        }
                        else if (pSerie.cvfSpecified)
                        {
                            contractMultiplier = (decimal)pSerie.cvf;
                        }
                        else if (pSqlDC.ContractMultiplier.HasValue)
                        {
                            contractMultiplier = pSqlDC.ContractMultiplier.Value;
                        }
                        if (contractMultiplier != 0)
                        {
                            price = ((decimal)pOpt.p) / contractMultiplier;
                        }
                        else
                        {
                            // Ajustement du Settlement Price
                            price = AdjustPrice(pSqlDC, (decimal)(pOpt.p));
                        }
                    }
                    else
                    {
                        // Ajustement du Settlement Price
                        price = AdjustPrice(pSqlDC, (decimal)(pOpt.p));
                    }
                    // PM 20180827 [XXXXX] Gestion de la Base >= 10000
                    if (price.HasValue && pSqlDC.InstrumentDen >= 10000)
                    {
                        price *= (pSqlDC.InstrumentDen / 100);
                    }
                    //
                    // PM 20180828 [XXXXX] Ajout recherche du Composite Delta
                    decimal compositeDelta = 1;
                    if ((pOpt.ra != default(ra[])) && (pOpt.ra.Count() > 0))
                    {
                        ra riskArray = pOpt.ra[0];
                        if (riskArray != default(ra))
                        {
                            compositeDelta = (decimal)(riskArray.d);
                        }
                    }
                    // Gestion m_IsPriceOnly
                    if (false == m_IsPriceOnly)
                    {
                        // PM 20180824 [XXXXX] Gestion de la Base >= 10000 : ajout StrikePrice comme paramètre
                        // PM 20180828 [XXXXX] ajout compositeDelta comme paramètre
                        Insert_IMSPANARRAY_H(pIdIMSPANCONTRACT_H, pIdIMSPANMATURITY_H, idAsset, pUnderlying, pSerie, pOpt, strikePrice, price, compositeDelta);
                    }
                    // PM 20180828 [XXXXX] ajout compositeDelta comme paramètre
                    Insert_QUOTE_ETD_H(idAsset, pIdm, price, pOptPf.currency, pSerie, pOpt, compositeDelta);

                    // Mise à jour ContractMultiplier et envoie à Quote Handling si différent de celui du DC
                    if (((pOpt.cvfSpecified) || (pSerie.cvfSpecified)) && (pSqlDC.IsAutoSetting))
                    {
                        decimal contractMultiplier = (decimal)(pOpt.cvfSpecified ? pOpt.cvf : pSerie.cvf);
                        if ((false == pSqlDC.ContractMultiplier.HasValue) || (pSqlDC.ContractMultiplier.HasValue && (pSqlDC.ContractMultiplier.Value != contractMultiplier)))
                        {
                            UpdateAssetContractMultiplier(idAsset, (decimal)pSerie.cvf);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import les paramètres de risque des Combined Commodity
        /// </summary>
        /// <param name="pClearOrg"></param>
        private void ProcessCcDef(ClearingOrg pClearOrg)
        {
            if ((pClearOrg != default(ClearingOrg)) && (pClearOrg.ccDef != default(CcDef[])))
            {
                foreach (CcDef cc in pClearOrg.ccDef)
                {
                    // Vérifier si le Combined Commodity doit être importé
                    cc.IsToImport = IsCcDefToImport(pClearOrg, cc);
                    if (cc.IsToImport)
                    {
                        int idIMSPANGRPCOMB_H = Insert_IMSPANGRPCOMB_H(pClearOrg.IdIMSPAN_H, cc);
                        int idIMSPANGRPCTR_H = Insert_IMSPANGRPCTR_H(pClearOrg.IdIMSPAN_H, idIMSPANGRPCOMB_H, cc);
                        // Rattachement des contrats du groupe
                        int nbContrat = Update_IMSPANCONTRACT_H(idIMSPANGRPCTR_H, cc);
                        if (nbContrat > 0)
                        {
                            // Import des Spot Month Rate
                            ProcessSpotRate(idIMSPANGRPCTR_H, cc.spotRate);
                            //
                            // Import des Intra Tiers
                            ProcessTier(idIMSPANGRPCTR_H, cc.IntraTiers, SPANSpreadType.IntraTierType);
                            // Import des Inter Tiers
                            ProcessTier(idIMSPANGRPCTR_H, cc.InterTiers, SPANSpreadType.InterTierType);
                            // Import des Scan Tiers
                            ProcessTier(idIMSPANGRPCTR_H, cc.ScanTiers, SPANSpreadType.ScanTierType);
                            // Import des Som Tiers
                            ProcessTier(idIMSPANGRPCTR_H, cc.SomTiers, SPANSpreadType.SomTierType);
                            //
                            // Import des Intra Spread
                            ProcessIntraSpread(idIMSPANGRPCTR_H, cc.dSpread);
                        }
                        // Construction du disctionnaire des Cc importés
                        m_CcImported.Add(cc.cc, idIMSPANGRPCTR_H);
                    }
                }
            }
        }

        /// <summary>
        /// Import les définitions des SpotRate
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pTiers"></param>
        /// <param name="pTierType"></param>
        private void ProcessSpotRate(int pIdIMSPANGRPCTR_H, spotRate[] pSpotMonth)
        {
            if ((pSpotMonth != default(spotRate[])) && (pSpotMonth.Count() > 0))
            {
                foreach (spotRate spot in pSpotMonth)
                {
                    Insert_IMSPANDLVMONTH_H(pIdIMSPANGRPCTR_H, spot);
                }
            }
        }
        
        /// <summary>
        /// Import les définitions des Tiers
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pTiers"></param>
        /// <param name="pTierType"></param>
        private void ProcessTier(int pIdIMSPANGRPCTR_H, IEnumerable<Tier> pTiers, string pTierType)
        {
            if ((pTiers != default(IEnumerable<Tier>)) && (pTiers.Count() > 0))
            {
                foreach (Tier spreadTier in pTiers)
                {
                    Insert_IMSPANTIER_H(pIdIMSPANGRPCTR_H, spreadTier, pTierType);
                }
            }
        }
        
        /// <summary>
        /// Import les définitions des Spread
        /// </summary>
        /// <param name="pIdIMSPANGRPCTR_H"></param>
        /// <param name="pTiers"></param>
        /// <param name="pTierType"></param>
        private void ProcessIntraSpread(int pIdIMSPANGRPCTR_H, DSpread[] pSpread)
        {
            if ((pSpread != default(DSpread[])) && (pSpread.Count() > 0))
            {
                foreach (DSpread spread in pSpread)
                {
                    int idIMSPANINTRASPR_H = Insert_IMSPANINTRASPR_H(pIdIMSPANGRPCTR_H, spread);
                    int legNumber = 1;
                    // PM 20180808 [XXXXX] Ajout test pour vérifier qu'il y a des PLeg
                    if ((spread.PLeg != default(IEnumerable<pLeg>)) && (spread.PLeg.Count() > 0))
                    {
                        foreach (pLeg leg in spread.PLeg)
                        {
                            Insert_IMSPANINTRALEG_H(idIMSPANINTRASPR_H, leg, legNumber);
                            legNumber += 1;
                        }
                    }
                    legNumber = 1;
                    // PM 20180808 [XXXXX] Ajout test pour vérifier qu'il y a des TLeg
                    if ((spread.TLeg != default(IEnumerable<tLeg>)) && (spread.TLeg.Count() > 0))
                    {
                        foreach (tLeg leg in spread.TLeg)
                        {
                            Insert_IMSPANINTRALEG_H(idIMSPANINTRASPR_H, leg, legNumber);
                            legNumber += 1;
                        }
                    }
                }
            }
        }
    
        /// <summary>
        /// Import les paramètres de risque des Combined Commodity
        /// </summary>
        /// <param name="pClearOrg"></param>
        private void ProcessInterSpreads(ClearingOrg pClearOrg)
        {
            if (pClearOrg != default(ClearingOrg))
            {
                //Super Inter-Spreads "S"
                // PM 20180808 [XXXXX] Modif test pour vérifier qu'il y a des SuperSSpreads
                if ((pClearOrg.SuperSSpreads != default(IEnumerable<SSpread>)) && (pClearOrg.SuperSSpreads.Count() > 0))
                {
                    foreach (SSpread spread in pClearOrg.SuperSSpreads)
                    {
                        bool isToImportSleg = false;
                        if ((spread.sLeg != null) && (spread.sLeg.Count() > 0))
                        {
                            // Le Combined Contract de chaque jambe doit être présent
                            IEnumerable<string> ccInSpread = spread.sLeg.Select(leg => leg.cc).Distinct();
                            isToImportSleg = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                        }
                        // Importer le spread si les jambes peuvent être importées
                        if (isToImportSleg)
                        {
                            int idIMSPANINTERSPR_H = Insert_IMSPANINTERSPR_H(pClearOrg.IdIMSPAN_H, spread, SPANSpreadType.SuperInterSpreadType);
                            int legNumber = 1;
                            //Super Inter-Spreads Leg "S"
                            foreach (sLeg leg in spread.sLeg)
                            {
                                m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                legNumber += 1;
                            }
                        }
                    }
                }
                //Super Inter-Spreads "D"
                // PM 20180808 [XXXXX] Modif test pour vérifier qu'il y a des SuperDSpreads
                if ((pClearOrg.SuperDSpreads != default(IEnumerable<DSpread>)) && (pClearOrg.SuperDSpreads.Count() > 0))
                {
                    foreach (DSpread spread in pClearOrg.SuperDSpreads)
                    {
                        bool isToImportTleg = false;
                        if ((spread.TLeg != null) && (spread.TLeg.Count() > 0))
                        {
                            // Le Combined Contract de chaque jambe doit être présent
                            IEnumerable<string> ccInSpread = spread.TLeg.Select(leg => leg.cc).Distinct();
                            isToImportTleg = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                        }
                        // Importer le spread si les jambes peuvent être importées
                        if (isToImportTleg)
                        {
                            int idIMSPANINTERSPR_H = Insert_IMSPANINTERSPR_H(pClearOrg.IdIMSPAN_H, spread, SPANSpreadType.SuperInterSpreadType);
                            int legNumber = 1;
                            //Super Inter-Spreads Leg "T"
                            foreach (tLeg leg in spread.TLeg)
                            {
                                m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                legNumber += 1;
                            }
                        }
                    }
                }
                //Inter-Spreads "S"
                // PM 20180808 [XXXXX] Modif test pour vérifier qu'il y a des InterSSpreads
                if ((pClearOrg.InterSSpreads != default(IEnumerable<SSpread>)) && (pClearOrg.InterSSpreads.Count() > 0))
                {
                    foreach (SSpread spread in pClearOrg.InterSSpreads)
                    {
                        bool isToImportSleg = false;
                        if ((spread.sLeg != null) && (spread.sLeg.Count() > 0))
                        {
                            // Le Combined Contract de chaque jambe doit être présent
                            IEnumerable<string> ccInSpread = spread.sLeg.Select(leg => leg.cc).Distinct();
                            isToImportSleg = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                        }
                        // Importer le spread si les jambes peuvent être importées
                        if (isToImportSleg)
                        {
                            int idIMSPANINTERSPR_H = Insert_IMSPANINTERSPR_H(pClearOrg.IdIMSPAN_H, spread, SPANSpreadType.InterSpreadType);
                            int legNumber = 1;
                            //Inter-Spreads Leg "S"
                            foreach (sLeg leg in spread.sLeg)
                            {
                                m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                legNumber += 1;
                            }
                        }
                    }
                }
                //Inter-Spreads "D"
                if (pClearOrg.InterDSpreads != null)
                {
                    foreach (DSpread spread in pClearOrg.InterDSpreads)
                    {
                        // PM 20161003 [22436] Ajout isToImportSpread pour vérifier que toutes les jambes sont présentes
                        bool isToImportSpread = false;
                        // PM 20180828 [XXXXX] Modification de la vérification des Spreads à importer
                        bool isToImportPleg = false;
                        bool isToImportRPleg = false;
                        bool isToImportTleg = false;
                        bool isPleg = ((spread.PLeg != null) && (spread.PLeg.Count() > 0));     // Présence de Pleg
                        bool isRPleg = ((spread.RpLeg != null) && (spread.RpLeg.Count() > 0));  // Présence de RPleg
                        bool isTleg = ((spread.TLeg != null) && (spread.TLeg.Count() > 0));     // Présence de Tleg
                        if (isPleg)
                        {
                            // Le Combined Contract de chaque jambe doit être présent
                            IEnumerable<string> ccInSpread = spread.PLeg.Select(leg => leg.cc).Distinct();
                            isToImportPleg = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                            // PM 20161003 [22436] ajout isToImportSpread (true si toutes les jambes présentes)
                            //isToImportSpread = isToImportPleg;
                        }
                        if (isRPleg)
                        {
                            // Le Combined Contract de chaque jambe doit être présent
                            IEnumerable<string> ccInSpread = spread.RpLeg.Select(leg => leg.cc).Distinct();
                            isToImportRPleg = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                            // PM 20161003 [22436] ajout isToImportSpread (true si toutes les jambes présentes)
                            //isToImportSpread |= isToImportRPleg;
                        }
                        if (isTleg)
                        {
                            // Le Combined Contract de chaque jambe doit être présent
                            IEnumerable<string> ccInSpread = spread.TLeg.Select(leg => leg.cc).Distinct();
                            isToImportTleg = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                            // PM 20161003 [22436] ajout isToImportSpread (true si toutes les jambes présentes)
                            //isToImportSpread |= isToImportTleg;
                        }
                        isToImportSpread = (isToImportPleg || !isPleg) && (isToImportRPleg || !isRPleg) && (isToImportTleg || !isTleg);
                        // Importer le spread si les jambes peuvent être importées
                        // PM 20161003 [22436] Tester uniquement isToImportSpread, plus besoin de (isToImportPleg || isToImportRPleg || isToImportTleg) 
                        //if (isToImportPleg || isToImportRPleg || isToImportTleg)
                        if (isToImportSpread)
                            {
                            int idIMSPANINTERSPR_H = Insert_IMSPANINTERSPR_H(pClearOrg.IdIMSPAN_H, spread, SPANSpreadType.InterSpreadType);
                            int legNumber = 1;
                            //Inter-Spreads Leg "P"
                            if (isToImportPleg)
                            {
                                foreach (pLeg leg in spread.PLeg)
                                {
                                    m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                    Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                    legNumber += 1;
                                }
                            }
                            //Inter-Spreads Leg "RP"
                            if (isToImportRPleg)
                            {
                                // PM 20161003 [22436] Ne pas réinitialiser legNumber : plusieurs types de leg possible
                                //legNumber = 1;
                                foreach (RpLeg leg in spread.RpLeg)
                                {
                                    m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                    Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                    legNumber += 1;
                                }
                            }
                            //Inter-Spreads Leg "T"
                            if (isToImportTleg)
                            {
                                // PM 20161003 [22436] Ne pas réinitialiser legNumber : plusieurs types de leg possible
                                //legNumber = 1;
                                foreach (tLeg leg in spread.TLeg)
                                {
                                    m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                    Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                    legNumber += 1;
                                }
                            }
                        }
                    }
                }
                //Spreads Inter-Clearing
                if (pClearOrg.interClearSpreads != null)
                {
                    foreach (clearSpread spread in pClearOrg.interClearSpreads)
                    {
                        if ((spread.homeLeg != null) && (spread.awayLeg != null) && (spread.homeLeg.Count() > 0) && (spread.awayLeg.Count() > 0))
                        {
                            // Le Combined Contract de chaque jambe de la chambre courante doit être présent
                            IEnumerable<string> ccInSpread = spread.homeLeg.Select(leg => leg.cc).Distinct();
                            bool isToImport = (m_CcImported.Keys.Intersect(ccInSpread).Count() == ccInSpread.Count());
                            //
                            if (isToImport)
                            {
                                int idIMSPANINTERSPR_H = Insert_IMSPANINTERSPR_H(pClearOrg.IdIMSPAN_H, spread, SPANSpreadType.InterClearSpreadType);
                                int legNumber = 1;
                                foreach (HomeLeg leg in spread.homeLeg)
                                {
                                    m_CcImported.TryGetValue(leg.cc, out int idIMSPANGRPCTR_H);
                                    Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, idIMSPANGRPCTR_H, leg, legNumber);
                                    legNumber += 1;
                                }
                                foreach (awayLeg leg in spread.awayLeg)
                                {
                                    Insert_IMSPANINTERLEG_H(idIMSPANINTERSPR_H, leg, legNumber);
                                    legNumber += 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClearOrg"></param>
        /// <param name="pCcDef"></param>
        /// <returns></returns>
        private bool IsCcDefToImport(ClearingOrg pClearOrg, CcDef pCcDef)
        {
            bool isToImport = false;
            if ((pCcDef != default(CcDef)) && (pCcDef.pfLink != default(PfLink[])) && (pCcDef.pfLink.Count() > 0))
            {
                // Vérification que le Combined Commodity contient des contrats qui ont été importés
                var pfLinkImported = m_ContractImported.Intersect(pCcDef.pfLink, m_PfLinkComparer);
                isToImport = (pfLinkImported.Count() > 0);
                // Vérification que le Combined Commodity n'est pas une des jambes d'un SLeg d'un Spread Inter-Contract qui soit la jambe cible ou une jambe non requise
                if ((isToImport == false) && (pClearOrg != default(ClearingOrg)))
                {
                    //Super Inter-Spreads "S"
                    if (pClearOrg.SuperSSpreads != null)
                    {
                        isToImport = pClearOrg.SuperSSpreads.Any(s => ((s.sLeg != null) && (s.sLeg.Any(l => (l.cc == pCcDef.cc) && (l.isTarget || (l.isRequiredSpecified && !l.isRequired))))));
                    }
                    //Inter-Spreads "S"
                    if ((isToImport == false) && (pClearOrg.InterSSpreads != null))
                    {
                        isToImport = pClearOrg.InterSSpreads.Any(s => ((s.sLeg != null) && (s.sLeg.Any(l => (l.cc == pCcDef.cc) && (l.isTarget || (l.isRequiredSpecified && !l.isRequired))))));
                    }
                }
            }
            return isToImport;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSqlDC"></param>
        /// <param name="pPrice"></param>
        /// <returns></returns>
        // PL 20181205 [24368] Add case "C" for TNOTES 5 YR, 2 YR, ... on CBOT 
        protected decimal AdjustPrice(SQL_DerivativeContract pSqlDC, decimal pPrice)
        {
            string alignCode = pSqlDC.PriceAlignCode;
            if (alignCode != null)
            {
                decimal priceDecPart = pPrice - System.Math.Truncate(pPrice);
                string priceDec3Digit = ((int)System.Math.Truncate(priceDecPart * 1000)).ToString().PadLeft(3,'0');
                int lastDigit = Convert.ToInt32(priceDec3Digit.Substring(2,1));
                int last2Digit = Convert.ToInt32(priceDec3Digit.Substring(0,2));
                switch (alignCode)
                {
                    case "0":
                        switch (lastDigit)
                        {
                            case 1:
                            case 6:
                                pPrice += 0.00025m;
                                break;
                            case 2:
                            case 7:
                                pPrice += 0.00050m;
                                break;
                            case 3:
                            case 8:
                                pPrice += 0.00075m;
                                break;
                        }
                        break;
                    case "4":
                    case "7":
                        pPrice = System.Math.Truncate(pPrice) + (last2Digit * 0.0015630m) - (System.Math.Floor(last2Digit / 2.0m) / 100000.0m);
                        break;
                    case "9":
                        pPrice = System.Math.Truncate(pPrice) + (last2Digit * 0.003125m);
                        break;
                    case "B":
                        {
                            int last3Digit = Convert.ToInt32(priceDec3Digit);
                            pPrice = System.Math.Truncate(pPrice) + (last2Digit * 0.003126m) - (System.Math.Floor(last3Digit * 0.1m) / 100000.0m);
                        }
                        break;
                    case "C":
                        switch (lastDigit)
                        {
                            //PL 20190311 Add Case 1,3,6,8 for CME - ADVISORY #: 17-461 - Price Precision Extension [24560] 
                            #region CME - ADVISORY #: 17-461 - Price Precision Extension
                            /*
                             * With the first phase of this project, where the tick is being halved, here are the revised examples: 
                             * 
                             * Price description                    TCC format  FIXML format 
                             * 109 and 13 32nd's                    0109130     109.40625000 
                             * 109 and 13 and one-eighth 32nd's     0109131     109.41015625 
                             * 109 and 13 and two-eighth 32nd's     0109132     109.41406250 
                             * 109 and 13 and three-eighth 32nd's   0109133     109.41796875 
                             * 109 and 13 and four-eighth 32nd's    0109135     109.42187500 
                             * 109 and 13 and five-eighth 32nd's    0109136     109.42578125 
                             * 109 and 13 and six-eighth 32nd's     0109137     109.42968750 
                             * 109 and 13 and seven-eighth 32nd's   0109138     109.43359375 
                             * 
                             * In other words, in that final digit of the TCC format for a Treasury future, we currently support 0, 2, 5 or 7, meaning 0.00, 0.25, 0.50, or 0.75 of a 32nd.  
                             * With the change, we will also support 1, 3, 6 or 8, meaning 0.125, 0.375, 0.625 or 0.875 of a 32nd. 
                            */
                            #endregion
                            case 1:
                            case 6:
                                pPrice += 0.00025m;
                                break;
                            case 2:
                            case 7:
                                pPrice += 0.00050m;
                                break;
                            case 3:
                            case 8:
                                pPrice += 0.00075m;
                                break;
                        }
                        break;
                    case "K":
                        switch (lastDigit)
                        {
                            //PL 20190311 Newness for CME - ADVISORY #: 17-461 - Price Precision Extension 
                            #region CME - ADVISORY #: 17-461 - Price Precision Extension
                            /*
                             * In the first phase of this project, where the tick is being halved, here are examples of possible values for such option prices: 
                             * 
                             * Price description                TCC format  FIXML format 
                             * 5 and 57 64th's                  0005570     5.890625000 
                             * 5 and 57 and one-quarter 64th    0005572     5.894531250 
                             * 5 and 57 and one-half 64th       0005575     5.898437500 
                             * 5 and 57 and three-quarter 64th  0005577     5.902343750 
                            */
                            #endregion
                            case 2:
                            case 7:
                                pPrice += 0.00050m;
                                break;
                        }
                        break;
                }
            }
            return pPrice;
        }
        
        /// <summary>
        /// Recherche to taux pour passer de Initial à Maintenance en fonction du type de compte
        /// </summary>
        /// <param name="pAcctType"></param>
        /// <param name="pAdjRate"></param>
        /// <returns></returns>
        private decimal FindPBRate(string pAcctType, adjRate[] pAdjRate)
        {
            decimal acctRate = 1;
            if (StrFunc.IsFilled(pAcctType)
                && (pAdjRate != default(adjRate[])) && (pAdjRate.Count() > 0)
                && (m_currentClearingOrg.pbRateDef != default(pbRateDef[])) && (m_currentClearingOrg.pbRateDef.Count() > 0))
            {
                acctRate = (decimal)
                    (from rateDef in m_currentClearingOrg.pbRateDef
                     where (rateDef.acctType == pAcctType)
                     join rateVal in pAdjRate on rateDef.r equals rateVal.r
                     select rateVal.val).FirstOrDefault();
                if (acctRate == 0)
                {
                    acctRate = 1;
                }
            }
            return acctRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDt"></param>
        /// <returns></returns>
        private DataRow SetDataRow(DataTable pDt)
        {
            DataRow dr;
            if (pDt.Rows.Count == 0)
            {
                dr = pDt.NewRow();
                SetDataRowIns(dr);
                pDt.Rows.Add(dr);
            }
            else
            {
                dr = pDt.Rows[0];
                SetDataRowUpd(dr);
            }
            return dr;
        }

        #endregion Methods
    }
}
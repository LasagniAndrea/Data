using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

using FixML.Enum;

using System;
using System.Data;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Class inspirée de MarketDataImportBase et prenant en charge la recherche de données dans Eurosys
    /// </summary>
    // PM 20141216 [9700] Eurex Prisma for Eurosys Futures : New class
    public class EurosysMarketData
    {
        #region members
        private readonly string m_Cs;
        private readonly string m_EurosysSchemaPrefix;
        // IsUse...
        private readonly bool m_IsUseDcCategory = false;
        private readonly bool m_IsUseDcSettltMethod = false;
        private readonly bool m_IsUseDcExerciseStyle = false;
        // Queries
        private readonly QueryParameters m_QueryExistDCInTrade;
        // Previous values
        private string m_lastDC_Exchange = string.Empty;
        private string m_lastDC_ContractSymbol = string.Empty;
        private string m_lastDC_Category = string.Empty;
        private bool m_isExistPreviousDC_ContractSymbol = false;
        #endregion
        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pEurosysSchemaPrefix"></param>
        public EurosysMarketData(string pCs, string pEurosysSchemaPrefix)
            : this (pCs, pEurosysSchemaPrefix, false, false, false) { }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pEurosysSchemaPrefix"></param>
        /// <param name="pIsUseDcCategory"></param>
        /// <param name="pIsUseDcSettltMethod"></param>
        /// <param name="pIsUseDcExerciseStyle"></param>
        /// <param name="pIsUseDcExerciseStyle"></param>
        /// <param name="pIsUseDcContractAttrib"></param>
        public EurosysMarketData(string pCs, string pEurosysSchemaPrefix, bool pIsUseDcCategory, bool pIsUseDcSettltMethod, bool pIsUseDcExerciseStyle)
        {
            m_Cs = pCs;
            // Eurosys Schema Prefix
            if (string.IsNullOrEmpty(pEurosysSchemaPrefix))
            {
                m_EurosysSchemaPrefix = "";
            }
            else
            {
                m_EurosysSchemaPrefix = pEurosysSchemaPrefix;
            }
            // Set IsUse...
            m_IsUseDcSettltMethod = pIsUseDcSettltMethod;
            m_IsUseDcExerciseStyle = pIsUseDcExerciseStyle;
            m_IsUseDcCategory = (pIsUseDcExerciseStyle || pIsUseDcCategory);
            // Queries
            m_QueryExistDCInTrade = QueryExistDCInTrades();
        }
        #endregion
        #region methods
        /// <summary>
        /// Retourne la requête qui permet vérifier que le DC a déjà été négocié
        /// <para>Les paramètres obligatoires de la requête sont DTFILE, ISO10383_ALPHA4, CONTRACTSYMBOL</para>
        /// <para>Les paramètres optionels de la requête sont CATEGORY, SETTLTMETHOD, EXERCISESTYLE</para>
        /// </summary>
        /// <returns></returns>
        private QueryParameters QueryExistDCInTrades()
        {
            StrBuilder query = new StrBuilder();
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.ISO10383_ALPHA4));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));

            query += SQLCst.SELECT_DISTINCT + "1" + Cst.CrLf;
            query += " from " + m_EurosysSchemaPrefix + "EVW_EUROSYS_ASSET_ETD a " + Cst.CrLf;
            query += SQLCst.WHERE + "(a.ISO10383_ALPHA4 = @ISO10383_ALPHA4)" + Cst.CrLf;
            query += SQLCst.AND + "(a.CONTRACTSYMBOL = @CONTRACTSYMBOL)" + Cst.CrLf;
            query += SQLCst.AND + "((a.MATURITYDATE is null) or (a.MATURITYDATE >= @DTFILE))" + Cst.CrLf;
            if (m_IsUseDcCategory)
            {
                query += SQLCst.AND + "(a.CATEGORY = @CATEGORY)" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.CATEGORY));
                if (m_IsUseDcExerciseStyle)
                {
                    query += SQLCst.AND + "((a.EXERCISESTYLE = @EXERCISESTYLE)";
                    query += SQLCst.OR + "(@CATEGORY = 'F'))" + Cst.CrLf;
                    dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.EXERCISESTYLE));
                }
            }
            if (m_IsUseDcSettltMethod)
            {
                query += SQLCst.AND + "((a.SETTLTMETHOD = @SETTLTMETHOD)";
                query += SQLCst.OR + "(@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }

            QueryParameters qryParameters = new QueryParameters(m_Cs, query.ToString(), dataParameters);

            return qryParameters;
        }
        /// <summary>
        /// Retourne la requête qui permet de vérifier qu'un asset Option est déjà négocié 
        /// <para>Les paramètres de la requête sont DTFILE, ISO10383_ALPHA4, CONTRACTSYMBOL, CATEGORY, MATURITYMONTHYEAR, PUTCALL, STRIKEPRICE</para>
        /// <para>et optionnellement SETTLTMETHOD, EXERCISESTYLE, CONTRACTATTRIBUTE</para>
        /// </summary>
        /// <param name="pIsUseDcExerciseStyle"></param>
        /// <param name="pIsUseDcSettltMethod"></param>
        /// <param name="pIsUseDcContractAttrib"></param>
        /// <returns></returns>
        public QueryParameters QueryExistAssetInTrades(bool pIsUseDcExerciseStyle, bool pIsUseDcSettltMethod, bool pIsUseDcContractAttrib)
        {
            // Parameters
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.DTFILE));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.ISO10383_ALPHA4));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.CONTRACTSYMBOL));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.CATEGORY));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.MATURITYMONTHYEAR));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.PUTCALL));
            dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.STRIKEPRICE));
            // Query
            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + "a.IDASSET" + Cst.CrLf;
            query += " from " + m_EurosysSchemaPrefix + "EVW_EUROSYS_ASSET_ETD a " + Cst.CrLf;
            query += SQLCst.WHERE + "(a.ISO10383_ALPHA4 = @ISO10383_ALPHA4)" + Cst.CrLf;
            query += SQLCst.AND + "(a.CONTRACTSYMBOL = @CONTRACTSYMBOL)" + Cst.CrLf;
            query += SQLCst.AND + "(a.MATURITYMONTHYEAR = @MATURITYMONTHYEAR)" + Cst.CrLf;
            query += SQLCst.AND + "((a.MATURITYDATE is null) or (a.MATURITYDATE >= @DTFILE))" + Cst.CrLf;
            query += SQLCst.AND + "(a.CATEGORY = @CATEGORY)" + Cst.CrLf;
            query += SQLCst.AND + "((@CATEGORY = 'F')" + Cst.CrLf;
            query += SQLCst.OR + "((a.PUTCALL = @PUTCALL)" + Cst.CrLf;
            query += SQLCst.AND + "(a.STRIKEPRICE = @STRIKEPRICE)))" + Cst.CrLf;

            if (pIsUseDcExerciseStyle)
            {
                query += SQLCst.AND + "((a.EXERCISESTYLE = @EXERCISESTYLE)";
                query += SQLCst.OR + "(@CATEGORY = 'F'))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.EXERCISESTYLE));
            }
            if (pIsUseDcSettltMethod)
            {
                query += SQLCst.AND + "((a.SETTLTMETHOD = @SETTLTMETHOD)";
                query += SQLCst.OR + "(@SETTLTMETHOD is null))" + Cst.CrLf;
                dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.SETTLTMETHOD));
            }
            if (pIsUseDcContractAttrib)
            {
                query += SQLCst.AND + "(a.CONTRACTATTRIBUTE = @CONTRACTATTRIBUTE)";
                dataParameters.Add(DataParameter.GetParameter(m_Cs, DataParameter.ParameterEnum.CONTRACTATTRIBUTE));
            }

            QueryParameters qryParameters = new QueryParameters(m_Cs, query.ToString(), dataParameters);

            return qryParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDtFile"></param>
        /// <param name="pIso10383_Alpha4"></param>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        public bool IsExistDcInTrade(DateTime pDtFile, string pIso10383_Alpha4, string pContractSymbol)
        {
            return IsExistDcInTrade(pDtFile, pIso10383_Alpha4, pContractSymbol, null, null, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDtFile"></param>
        /// <param name="pIso10383_Alpha4"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pCategory"></param>
        /// <param name="pSettltMethod"></param>
        /// <param name="pExerciseStyle"></param>
        /// <returns></returns>
        public bool IsExistDcInTrade(DateTime pDtFile, string pIso10383_Alpha4, string pContractSymbol, string pCategory, string pSettltMethod, string pExerciseStyle)
        {
            bool exist;
            if (StrFunc.IsEmpty(pIso10383_Alpha4) || StrFunc.IsEmpty(pContractSymbol))
            {
                exist = false;
            }
            else
            {
                if ((pIso10383_Alpha4 == m_lastDC_Exchange)
                    && (pContractSymbol == m_lastDC_ContractSymbol)
                    && (!m_IsUseDcCategory || (pCategory == m_lastDC_Category))
                    && (!m_IsUseDcExerciseStyle)
                    && (!m_IsUseDcSettltMethod))
                {
                    exist = m_isExistPreviousDC_ContractSymbol;
                }
                else
                {
                    m_QueryExistDCInTrade.Parameters["DTFILE"].Value = pDtFile;
                    m_QueryExistDCInTrade.Parameters["ISO10383_ALPHA4"].Value = pIso10383_Alpha4;
                    m_QueryExistDCInTrade.Parameters["CONTRACTSYMBOL"].Value = pContractSymbol;

                    if (m_IsUseDcCategory)
                    {
                        m_QueryExistDCInTrade.Parameters["CATEGORY"].Value = pCategory;
                        m_lastDC_Category = pCategory;
                        if (m_IsUseDcExerciseStyle)
                        {
                            if (pCategory == "O")
                            {
                                m_QueryExistDCInTrade.Parameters["EXERCISESTYLE"].Value = pExerciseStyle;
                            }
                            else
                            {
                                m_QueryExistDCInTrade.Parameters["EXERCISESTYLE"].Value = null;
                            }
                        }
                    }
                    if (m_IsUseDcSettltMethod)
                    {
                        m_QueryExistDCInTrade.Parameters["SETTLTMETHOD"].Value = pSettltMethod;
                    }

                    object obj_Dc_Trade = DataHelper.ExecuteScalar(m_Cs, CommandType.Text, m_QueryExistDCInTrade.Query, m_QueryExistDCInTrade.Parameters.GetArrayDbParameter());

                    exist = (obj_Dc_Trade != null);

                    m_lastDC_ContractSymbol = pContractSymbol;
                    m_isExistPreviousDC_ContractSymbol = exist;
                    m_lastDC_Exchange = pIso10383_Alpha4;
                }
            }
            return exist;
        }
        #endregion
    }
}
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;

using FixML.Enum;

using System;
using System.Data;
using EFS.SpheresIO.Properties;
using FixML.v50SP1.Enum;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// 
    /// </summary>
    /// PM 20141022 [9700] Eurex Prisma for Eurosys Futures : New class
    [Obsolete("L'importation dans les tables PRISMA_H n'est plus nécessaire")  ]
    internal class EurosysMarketDataImportEurexPrisma : MarketDataImportEurexPrisma
    {
        #region members
        private readonly EurosysMarketData m_EurosysTools;
        #endregion
        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pTask">Tâche IO</param>
        /// <param name="pDataName">Représente le source de donnée</param>
        /// <param name="pDataStyle">Représente le type de donnée en entrée</param>
        public EurosysMarketDataImportEurexPrisma(Task pTask, string pDataName, string pDataStyle)
            : base(pTask, pDataName, pDataStyle)
        {
            string eurosysSchemaPrefix = Settings.Default.EurosysSchemaPrefix;
            m_EurosysTools = new EurosysMarketData(CSTools.SetCacheOn(pTask.Cs), eurosysSchemaPrefix);
        }
        #endregion
        #region methods
        /// <summary>
        /// Retourne true si le symbol est négocié dans Eurosys
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <returns></returns>
        protected override Boolean IsExistDcInTrade(string pContractSymbol)
        {
            return m_EurosysTools.IsExistDcInTrade(m_dtFile, EUREX_MIC, pContractSymbol);
        }

        /// <summary>
        /// Retourne l'Id non significatif de la serie dans Eurosys
        /// <para>Retourne 0 si la serie n'existe pas dans Eurosys ou si la série n'a jamais été négociée</para>
        /// </summary>
        /// <param name="request">Element de recherche d'un asset</param>
        /// <param name="oAssetFound">Données parsées de l'asset</param>
        /// <returns></returns>
        protected override int GetIdAssetETD(MarketAssetETDRequest request, out MarketAssetETDToImport oAssetFound)
        {
            
            QueryParameters queryParameters = m_EurosysTools.QueryExistAssetInTrades(_assetETDRequestSettings.IsWithExerciseStyle, _assetETDRequestSettings.IsWithSettlementMethod, _assetETDRequestSettings.IsWithContractAttrib);
            DataParameters dp = queryParameters.Parameters;
            dp["DTFILE"].Value = m_dtFile;
            dp["ISO10383_ALPHA4"].Value = EUREX_MIC;
            dp["CONTRACTSYMBOL"].Value = request.ContractSymbol;
            dp["CATEGORY"].Value = request.ContractCategory;
            dp["MATURITYMONTHYEAR"].Value = request.MaturityMonthYear;

            if (_assetETDRequestSettings.IsWithSettlementMethod)
                dp["SETTLTMETHOD"].Value = ReflectionTools.ConvertEnumToString<SettlMethodEnum>(request.SettlementMethod);

            if (_assetETDRequestSettings.IsWithContractAttrib)
                dp["CONTRACTATTRIBUTE"].Value = request.ContractAttribute;

            if (request.ContractCategory == "O")
            {
                if (_assetETDRequestSettings.IsWithExerciseStyle)
                    dp["EXERCISESTYLE"].Value = ReflectionTools.ConvertEnumToString<DerivativeExerciseStyleEnum>(request.ExerciseStyle.Value);

                dp["PUTCALL"].Value = ReflectionTools.ConvertEnumToString<PutOrCallEnum>(request.PutCall.Value);
                dp["STRIKEPRICE"].Value = request.StrikePrice;
            }
            else
            {
                dp["EXERCISESTYLE"].Value = null;
                dp["PUTCALL"].Value = null;
                dp["STRIKEPRICE"].Value = null;
            }

            int idAsset = 0;
            using (IDataReader dr = DataHelper.ExecuteDataTable(CSTools.SetCacheOn(Cs), queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                if (dr.Read())
                    idAsset = IntFunc.IntValue(dr["IDASSET"].ToString());
            }

            oAssetFound = new MarketAssetETDToImport();
            if (idAsset > 0)
                oAssetFound = GetEurexAssetETDToImport(idAsset);

            return oAssetFound.IdAsset;
        }
        #endregion
    }
}

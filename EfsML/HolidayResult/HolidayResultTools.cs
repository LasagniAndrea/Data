using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;



namespace EfsML.Business
{

    /// <summary>
    /// 
    /// </summary>
    public static class HolidayResultTools
    {

        /// <summary>
        ///  Ajoute dans la table HOLIDAYRESULT les jours fériés qui potentiellement existe entre ENTITYMARKET.DTMARKET et ENTITYMARKET.DTMARKETNEXT  
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdEM"></param>
        /// <param name="pIdAIns"></param>
        /// <param name="pDtIns"></param>
        /// EG 20180205 [23769] Use dbTransaction  
        /// EG 20180307 [23769] Gestion dbTransaction
        /// EG 20180423 Analyse du code Correction [CA2200]
        public static void AddHoliday(string pCS, IDbTransaction pDbTransaction, int pIdEM, int pIdAIns, Nullable<DateTime> pDtIns)
        {

            SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(pCS, pIdEM);

            // RD 20130516 [18662] Utilisation du mode transactionnel
            if (pDbTransaction != null)
                sqlEntityMarket.DbTransaction = pDbTransaction;

            if (false == sqlEntityMarket.LoadTable(new string[] { "ENTITYMARKET.DTMARKET", "ENTITYMARKET.DTMARKETNEXT", "ENTITYMARKET.IDM" }))
                throw new ArgumentException(StrFunc.AppendFormat("ENTITYMARTKET (id:{0}) doesn't exist", pIdEM.ToString()));

            DateTime dtMarketNext = sqlEntityMarket.DtMarketNext;
            DateTime dtMarket = sqlEntityMarket.DtMarket;
            int idM = sqlEntityMarket.IdM;

#if DEBUG
            bool isTest = false;
            if (isTest)
            {
                dtMarket = new DateTime(2013, 04, 30);
                dtMarketNext = new DateTime(2013, 05, 30);
            }
#endif

            if (dtMarket.AddDays(1) != dtMarketNext)
            {
                int[] arrayIdM = new int[] { idM };

                string[] stringArrayIdM = Array.ConvertAll(arrayIdM, i => i.ToString());
                IProductBase product = Tools.GetNewProductBase();
                IBusinessCenters bcs = product.LoadBusinessCenters(pCS, pDbTransaction, null, null, stringArrayIdM);

                EFS_BusinessCenters efs_bc = new EFS_BusinessCenters(pCS, null as DataDocumentContainer)
                {
                    BusinessCenters = bcs,
                    IsLoadDescription = true
                };
                efs_bc.LoadHoliday();

                DateTime dtIns = DateTime.MinValue;
                if (pDtIns.HasValue)
                    dtIns = pDtIns.Value;
                else
                {
                    // FI 20200820 [25468] Dates systemes en UTC
                    dtIns = OTCmlHelper.GetDateSysUTC(pCS);
                }

                DateTime dt = dtMarket.AddDays(1);
                while (dtMarketNext.CompareTo(dt) == 1)
                {
                    if (efs_bc.IsHoliday(dt, DayTypeEnum.ExchangeBusiness))
                    {
                        if (false == IsHoliday(CSTools.SetCacheOn(pCS, 1, -1), dt, idM))
                        {
                            Nullable<Cst.HolidayType> holidayType = efs_bc.GetHolidayType(dt, DayTypeEnum.ExchangeBusiness, out EFS_HolidayBase holiday);
                            if (null == holidayType)
                                throw new InvalidOperationException(StrFunc.AppendFormat("unabled fo find type of holiday for date", DtFunc.DateTimeToStringDateISO(dt)));

                            string description = holiday.Description;

                            //PL 20131121
                            bool isHolidayBusiness = efs_bc.IsHoliday(dt, DayTypeEnum.Business);
                            bool isHolidayCommodityBusiness = efs_bc.IsHoliday(dt, DayTypeEnum.CommodityBusiness);
                            bool isHolidayCurrencyBusiness = efs_bc.IsHoliday(dt, DayTypeEnum.CurrencyBusiness);
                            bool isHolidayExchangeBusiness = efs_bc.IsHoliday(dt, DayTypeEnum.ExchangeBusiness);
                            bool isHolidayScheduledTradingDay = efs_bc.IsHoliday(dt, DayTypeEnum.ScheduledTradingDay);

                            //PL 20131121
                            // EG 20150220 Test Non existence DTHOLIDAY (IDM) dans HOLIDAYRESULT (en multi instance = DUPLICATE KEY)
                            DataParameters parameters = new DataParameters();
                            parameters.Add(new DataParameter(pCS, "IDM", DbType.Int32), idM);
                            parameters.Add(new DataParameter(pCS, "DTHOLIDAY", DbType.Date), dt);
                            string sqlQuery = @"select 1 from dbo.HOLIDAYRESULT where (IDM = @IDM)  and (DTHOLIDAY = @DTHOLIDAY)";
                            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);

                            object objHoliday = DataHelper.ExecuteScalar(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if (null == objHoliday)
                            {
                                try
                                {
                                    sqlQuery = @"insert into dbo.HOLIDAYRESULT
                                (IDM, DTHOLIDAY, IDBC, HOLIDAYTYPE, DESCRIPTION, IDAINS, DTINS, 
                                 ISBUSINESS, ISCOMMODITYBUSINESS, ISCURRENCYBUSINESS, ISEXCHANGEBUSINESS, ISSCHEDULEDTRADINGDAY)
                                values
                                (@IDM, @DTHOLIDAY, @IDBC, @HOLIDAYTYPE, @DESCRIPTION, @IDAINS, @DTINS, 
                                 @ISBUSINESS, @ISCOMMODITYBUSINESS, @ISCURRENCYBUSINESS, @ISEXCHANGEBUSINESS, @ISSCHEDULEDTRADINGDAY)";

                                    parameters.Add(new DataParameter(pCS, "HOLIDAYTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), holidayType.ToString());
                                    parameters.Add(new DataParameter(pCS, "IDBC", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), bcs.BusinessCenter[0].Value);
                                    parameters.Add(new DataParameter(pCS, "DESCRIPTION", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), description);
                                    parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pIdAIns);
                                    parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), dtIns);
                                    parameters.Add(new DataParameter(pCS, "ISBUSINESS", DbType.Boolean), isHolidayBusiness);
                                    parameters.Add(new DataParameter(pCS, "ISCOMMODITYBUSINESS", DbType.Boolean), isHolidayCommodityBusiness);
                                    parameters.Add(new DataParameter(pCS, "ISCURRENCYBUSINESS", DbType.Boolean), isHolidayCurrencyBusiness);
                                    parameters.Add(new DataParameter(pCS, "ISEXCHANGEBUSINESS", DbType.Boolean), isHolidayExchangeBusiness);
                                    parameters.Add(new DataParameter(pCS, "ISSCHEDULEDTRADINGDAY", DbType.Boolean), isHolidayScheduledTradingDay);
                                    qryParameters = new QueryParameters(pCS, sqlQuery, parameters);
                                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                                }
                                catch (Exception ex)
                                {
                                    if (false == DataHelper.IsDuplicateKeyError(pCS, ex))
                                        throw;
                                }
                            }
                        }
                    }
                    dt = dt.AddDays(1);
                }
                try
                {
                    DataHelper.queryCache.Remove(Cst.OTCml_TBL.HOLIDAYRESULT.ToString(), pCS, false);
                    DataEnabledHelper.ClearCache(pCS, Cst.OTCml_TBL.HOLIDAYRESULT);
                }
                catch
                {
                    //pas la peine de planter s'il existe un pb lors de la purge du cache
                }
            }
        }

        /// <summary>
        /// Retourne true si la date {pDate} déclarée jour férié  sur le marché {pIdM}
        /// <para>Lecture de la table HOLIDAYRESULT</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdM">Représente un marché</param>
        /// <returns></returns>
        public static bool IsHoliday(string pCS, DateTime pDate, int pIdM)
        {
            string sql = @"select 1 from dbo.HOLIDAYRESULT where IDM=@IDM and DTHOLIDAY=@DT";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDM), pIdM);

            QueryParameters qry = new QueryParameters(pCS, sql.ToString(), dp);

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            return (null != obj);
        }

    }
}

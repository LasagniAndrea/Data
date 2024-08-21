using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using System.IO;

using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{
    /// <summary>
    /// 
    /// </summary>
    public enum SQLJoinTypeEnum
    {
        Inner, Left, Right, Full
    }


    /// <summary>
    /// 
    /// </summary>
    public enum DataEnum
    {
        EnabledOnly, All
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OTCmlHelper
    {
        #region constructor
        public OTCmlHelper() { }
        #endregion constructor
        //

        /// <summary>
        ///  Retourne la date max entre {pDate} et la Date business
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static DateTime GetAnticipatedDate(string pCS, DateTime pDate)
        {
            return GetAnticipatedDate(pCS, null, pDate);
        }
        /// <summary>
        ///  Retourne la date max entre {pDate} et la date système
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public static DateTime GetAnticipatedDate(string pCS, IDbTransaction pDbTransaction, DateTime pDate)
        {
            DateTime dtSys = GetDateSys(pCS);
            return (0 < DateTime.Compare(pDate.Date, dtSys.Date)) ? pDate.Date : dtSys.Date;
        }

        /// <summary>
        ///  Retourne la date max entre {pDate} et la date système, cette date est formatée
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pDataFormat"></param>
        /// <returns></returns>
        public static string GetAnticipatedDateToString(string pCS, DateTime pDate, string pDataFormat)
        {
            return GetAnticipatedDateToString(pCS, null, pDate, pDataFormat);
        }
        /// <summary>
        ///  Retourne la date max entre {pDate} et la date système, cette date est formatée
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDate"></param>
        /// <param name="pDataFormat"></param>
        /// <returns></returns>
        public static string GetAnticipatedDateToString(string pCS, IDbTransaction pDbTransaction, DateTime pDate, string pDataFormat)
        {
            string ret;
            if (StrFunc.IsFilled(pDataFormat))
                ret = DtFunc.DateTimeToString(OTCmlHelper.GetAnticipatedDate(pCS, pDbTransaction, pDate).Date, pDataFormat);
            else
                ret = DtFunc.DateTimeToStringDateISO(OTCmlHelper.GetAnticipatedDate(pCS, pDbTransaction, pDate).Date);
            //
            return ret;
        }

        #region GetDataSetWithIsolationLevel
        /// <summary>
        /// Exécute une requete avec un Niveau d'Isolation spécifique
        /// <para>Avec également facultativement un TimeOut spécifique</para>
        /// </summary>
        /// <returns></returns>
        // EG 20140130 [19586] New
        // PL 20180202 Read Uncommited/Read Commited
        /// EG 20140130 [19586] New
        /// FI 20180316 [23769] Modify
        public static DataSet GetDataSetWithIsolationLevel(string pCS, IsolationLevel pIsolationLevel, QueryParameters pQryParameters, Nullable<int> pMaxTimeOut)
        {
            IDbTransaction dbTransaction = null;
            bool isException = false;
            string cs = pCS;
            try
            {
                if (pMaxTimeOut.HasValue)
                    cs = CSTools.SetMaxTimeOut(cs, pMaxTimeOut.Value);

                dbTransaction = DataHelper.BeginTran(cs, pIsolationLevel);
                // FI 20180316 [23769] passage de pCS pour permttre de mettre le résultat dans un le cache SQL
                DataSet ds = DataHelper.ExecuteDataset(pCS, dbTransaction, CommandType.Text, pQryParameters.Query, pQryParameters.Parameters.GetArrayDbParameter());
                //PL 20151229 Use DataHelper.CommitTran()
                //dbTransaction. Commit();
                DataHelper.CommitTran(dbTransaction);

                return ds;
            }
            catch (Exception)
            {
                isException = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isException) { DataHelper.RollbackTran(dbTransaction); }

                    //PL 20180202 WARNING +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    if (DataHelper.IsDbSqlServer(cs) && (pIsolationLevel != IsolationLevel.ReadCommitted))
                    {
                        //Restauration du niveau d'isolation à Read Commited, car maintenu sur la Connexion !
                        dbTransaction = DataHelper.BeginTran(cs, IsolationLevel.ReadCommitted);
                        DataHelper.CommitTran(dbTransaction);
                    }
                    //PL 20180202 WARNING +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                    dbTransaction.Dispose();
                }
            }
        }
        #endregion GetDataSetWithIsolationLevel

        /// <summary>
        /// Retourne la date Business (Lecture de la colonne EFSSOFTWARE.DTSYSSPHERES lorsque renseignée)
        /// <remarks>
        /// <para>
        /// Cette méthode s'applique sur tous les produits autres que ETD ou statégie avec ETD 
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static DateTime GetDateBusiness(string pCS)
        {
            return GetDateBusiness(pCS, null);
        }
        /// <summary>
        /// Retourne la date Business (Lecture de la colonne EFSSOFTWARE.DTSYSSPHERES lorsque renseignée)
        /// <remarks>
        /// <para>
        /// Cette méthode s'applique sur tous les produits autres que ETD ou statégie avec ETD 
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        /// FI 20200811 [XXXXX] Refactoring
        public static DateTime GetDateBusiness(string pCS, IDbTransaction pDbTransaction)
        {
            // FI 20200811 [XXXXX] Mise en commentaire, la méthode GetRDBMSDateSys est obsolete
            //DateTime dtTmp;
            //return GetRDBMSDateSys(pCS, pDbTransaction, true, false, out dtTmp);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += DataHelper.SQLIsNull(pCS, "DTSYSSPHERES", DataHelper.SQLGetDate(pCS)) + " as DTSYS" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString();

            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlSelect.ToString());
            return Convert.ToDateTime(obj).Date;
        }
        /// <summary>
        /// Retourne la date Business pour le couple {Entité, Marché / Custodian}. Retourne DateTime.MinValue si l'entité ou le marché / custodian est inconnu
        /// <para>
        /// Cette méthode s'applique uniquement sur les ETD (ALLOC, EXEC, INTERMED) et sur les stratégies qui contiennent au minimum un ETD 
        /// Cette méthode s'applique également sur les ESE, DSE et RTS
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdEntity"></param>
        /// <param name="pIdMarket"></param>
        /// <param name="pIdCustodian"></param>
        /// <returns></returns>
        public static DateTime GetDateBusiness(string pCS, int pIdEntity, int pIdMarket, Nullable<int> pIdCustodian)
        {
            return GetDateBusiness(pCS, null, pIdEntity, pIdMarket, pIdCustodian);
        }
        /// <summary>
        /// Retourne la date Business pour le couple {Entité, Marché / Custodian}. Retourne DateTime.MinValue si l'entité ou le marché / custodian est inconnu
        /// <para>
        /// Cette méthode s'applique uniquement sur les ETD (ALLOC, EXEC, INTERMED) et sur les stratégies qui contiennent au minimum un ETD 
        /// Cette méthode s'applique également sur les ESE, DSE et RTS
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdEntity"></param>
        /// <param name="pIdMarket"></param>
        /// <param name="pIdCustodian"></param>
        /// <returns></returns>
        /// EG 20150331 [POC] pIdMarket <> 0 en lieu et place de > 0
        /// EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        public static DateTime GetDateBusiness(string pCS, IDbTransaction pDbTransaction, int pIdEntity, int pIdMarket, Nullable<int> pIdCustodian)
        {

            DateTime ret = DateTime.MinValue;
            //
            if ((pIdEntity > 0) && (pIdMarket != 0))
            {
                DataParameters dp = new DataParameters();
                dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdEntity);
                dp.Add(new DataParameter(pCS, "IDM", DbType.Int32), pIdMarket);
                if (pIdCustodian.HasValue)
                    dp.Add(new DataParameter(pCS, "IDA_CUSTODIAN", DbType.Int32), pIdCustodian.Value);

                // PM 20150512 [20575] Utilisation de DTENTITY à la place de DTMARKET
                // et correction du cas ou au recherche la date d'un marché non encore présent dans entitymarket mais dont un marché de la même chambre est présent.
                //string sqlSelect = @"select em.DTMARKET
                //from dbo.ENTITYMARKET em
                //where (em.IDM=@IDM) and (em.IDA=@IDA) and " + (pIdCustodian.HasValue? "(em.IDA_CUSTODIAN=@IDA_CUSTODIAN)":"(em.IDA_CUSTODIAN is null)");
                string sqlSelect = @"select em.DTENTITY
                from dbo.ENTITYMARKET em
                inner join dbo.MARKET m on m.IDM = em.IDM
                inner join dbo.MARKET om on (om.IDA = m.IDA) or ((om.IDA is null) and (m.IDA is null) and (om.IDM = m.IDM))
                where (om.IDM=@IDM) and (em.IDA=@IDA)";
                sqlSelect += "and " + (pIdCustodian.HasValue ? "(em.IDA_CUSTODIAN=@IDA_CUSTODIAN)" : "(em.IDA_CUSTODIAN is null)") + Cst.CrLf;
                sqlSelect += "order by case m.IDM when om.IDM then 1 else 2 end";

                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dp);

                object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter());
                if (null != obj)
                    ret = Convert.ToDateTime(obj);
            }
            return ret;
        }

        public static DateTime GetDateBusinessCustodian(string pCS, int pIdEntity, int pIdMarket, int pIdCustodian)
        {
            return GetDateBusinessCustodian(pCS, null, pIdEntity, pIdMarket, pIdCustodian);
        }
        // EG 20150331 (POC] pIdMarket <> 0 en lieu et place de > 0
        // EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        public static DateTime GetDateBusinessCustodian(string pCS, IDbTransaction pDbTransaction, int pIdEntity, int pIdMarket, int pIdCustodian)
        {

            DateTime ret = DateTime.MinValue;
            // EG 20150331 (POC] pIdMarket
            if ((pIdEntity > 0) && (0 != pIdMarket))
            {
                DataParameters dp = new DataParameters();
                dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIdEntity);
                dp.Add(new DataParameter(pCS, "IDM", DbType.Int32), pIdMarket);
                dp.Add(new DataParameter(pCS, "IDA_CUSTODIAN", DbType.Int32), pIdCustodian);

                // PM 20150512 [20575] Utilisation de DTENTITY à la place de DTMARKET
                //                string sqlSelect = @"select em.DTMARKET
                //                from dbo.ENTITYMARKET em
                //                where (em.IDM=@IDM) and (em.IDA=@IDA) and (em.IDA_CUSTODIAN=@IDA_CUSTODIAN)
                //                union
                //                select MAX(em.DTMARKET)
                //                from dbo.ENTITYMARKET em
                //                where (em.IDA=@IDA) and (em.IDA_CUSTODIAN=@IDA_CUSTODIAN)" + Cst.CrLf;
                string sqlSelect = @"select em.DTENTITY
                from dbo.ENTITYMARKET em
                where (em.IDM=@IDM) and (em.IDA=@IDA) and (em.IDA_CUSTODIAN=@IDA_CUSTODIAN)
                union
                select MAX(em.DTENTITY)
                from dbo.ENTITYMARKET em
                where (em.IDA=@IDA) and (em.IDA_CUSTODIAN=@IDA_CUSTODIAN)" + Cst.CrLf;
                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dp);

                object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter());
                if (null != obj)
                    ret = Convert.ToDateTime(obj);
            }
            return ret;
        }

        /// <summary>
        /// Retoune l'horodatage système (SGBD)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public static DateTime GetDateSys(string pCS)
        {
            return GetDateSys(pCS, out _);
        }

        /// <summary>
        /// Retoune l'horodatage système (SGBD)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsUTC"></param>
        /// <returns></returns>
        /// FI 20200901 [25468] Add
        public static DateTime GetDateSys(string pCS, Boolean pIsUTC)
        {
            if (pIsUTC)
                return GetDateSysUTC(pCS);
            else
                return GetDateSys(pCS);
        }


        /// <summary>
        /// Retoune l'horodatage système (SGBD) dans le fuseau horaire UTC    
        /// </summary>
        /// <param name="pCS"></param>
        public static DateTime GetDateSysUTC(string pCS)
        {
            GetDateSys(pCS, out DateTime dt);
            return dt;
        }

        /// <summary>
        /// Retoune l'horodatage système (SGBD) et éventuellement son equivalent dans le fuseau horaire UTC       
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="opUTCDateSys"></param>
        /// <returns></returns>
        /// FI 20200811 [XXXXX] Add 
        public static DateTime GetDateSys(string pCS, out DateTime opUTCDateSys)
        {
            opUTCDateSys = new DateTime();
            DateTime ret = new DateTime();

            lock (DatesysCol.SyncRoot)
            {
                string csCol = new CSManager(pCS).GetCSWithoutPwd();

                if (false == DatesysCol.ContainsKey(csCol))
                    SynchroDatesysCol(pCS);

                ret = (DatesysCol[csCol] as Tuple<DatetimeProfiler, DatetimeProfiler>).Item1.GetDate();
                opUTCDateSys = (DatesysCol[csCol] as Tuple<DatetimeProfiler, DatetimeProfiler>).Item2.GetDate();
            }

            if (null == ret)
                throw new NullReferenceException("Date systeme is null");

            return ret;
        }


        /// <summary>
        /// Retourne une ressource (culture)
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pSqlTable"></param>
        /// <param name="pString"></param>
        /// <param name="opString"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        public static bool GetRessource(string pCs, string pSqlTable, string pString, ref string opString)
        {
            opString = "~";
            string cultureTwoLetterISOLanguageName = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            bool isEnglishCulture = (cultureTwoLetterISOLanguageName == "en");
            System.Text.RegularExpressions.Match matchCode = Cst.Regex_CodeNumber.Match(pString);

            if (matchCode.Success)
            {
                #region Msg dans une table SQL (ex. SYSTEMMSG)
                string code = matchCode.Groups[1].Value;
                int number = Convert.ToInt32(matchCode.Groups[2].Value);

                StrBuilder sqlSelect = new StrBuilder();
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCs, "SYSCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), code);
                parameters.Add(new DataParameter(pCs, "SYSNUMBER", DbType.Int32), number);
                parameters.Add(new DataParameter(pCs, "CULTURE", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), cultureTwoLetterISOLanguageName);

                if (isEnglishCulture)
                {
                    sqlSelect += SQLCst.SELECT + "smd.MESSAGE" + Cst.CrLf;
                }
                else
                {
                    sqlSelect += SQLCst.SELECT + DataHelper.SQLIsNull(pCs, "smd.MESSAGE", "smd_en.MESSAGE", "MESSAGE") + Cst.CrLf;
                    parameters.Add(new DataParameter(pCs, "CULTUREEN", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), "en");
                }

                sqlSelect += SQLCst.FROM_DBO + pSqlTable + " sm" + Cst.CrLf;
                sqlSelect += SQLCst.LEFTJOIN_DBO + pSqlTable + "DET" + " smd" + Cst.CrLf;
                sqlSelect += SQLCst.ON + "smd.SYSCODE=sm.SYSCODE" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "smd.SYSNUMBER=sm.SYSNUMBER" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "smd.CULTURE=@CULTURE" + Cst.CrLf;

                if (!isEnglishCulture)
                {
                    sqlSelect += SQLCst.LEFTJOIN_DBO + pSqlTable + "DET" + " smd_en" + Cst.CrLf;
                    sqlSelect += SQLCst.ON + "smd_en.SYSCODE=sm.SYSCODE" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "smd_en.SYSNUMBER=sm.SYSNUMBER" + Cst.CrLf;
                    sqlSelect += SQLCst.AND + "smd_en.CULTURE=@CULTUREEN" + Cst.CrLf;
                }

                sqlSelect += SQLCst.WHERE + "sm.SYSCODE=@SYSCODE" + SQLCst.AND + "sm.SYSNUMBER=@SYSNUMBER" + Cst.CrLf;

                using (IDataReader dr = DataHelper.ExecuteReader(pCs, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter()))
                {
                    if (dr.Read() && (!(dr["MESSAGE"] is DBNull)))
                        opString = Convert.ToString(dr["MESSAGE"]);
                }
                #endregion
            }
            return (opString != "~");
        }

       

        /// <summary>
        ///  Collection des dates systèmes. La clé d'accès à la collection est une connectionString 
        ///  <para>Cette collection permet d'obtenir la date système SGBD sans avoir à systématiquement faire une requête SQL</para>
        /// </summary>
        /// FI 20191211 [XXXXX] add
        private static readonly Hashtable DatesysCol = new Hashtable();

      

        /// <summary>
        /// Suppression la dernière date système connue pour la CS
        /// </summary>
        /// <param name="pCS">si null Reset de la collection des dates sytèmes dans sa totalité</param>
        public static void ResetDatesysCol(string pCS)
        {
            // FI 20200810 [XXXXX] Usage de lock
            lock (DatesysCol.SyncRoot)
            {
                if (StrFunc.IsFilled(pCS))
                {
                    string csCol = new CSManager(pCS).GetCSWithoutPwd();
                    if (DatesysCol.ContainsKey(csCol))
                        DatesysCol.Remove(csCol);
                }
                else
                {
                    // FI 20200810 [XXXXX] Corection Bug
                    // Hashtable DatesysCol = new System.Collections.Hashtable();
                    DatesysCol.Clear();
                }
            }
        }



        /// <summary>
        /// Synchronization de la date de référence avec la date système du SGBD (la référence)
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20200811 [XXXXX] Add Method
        public static void SynchroDatesysCol(string pCS)
        {
            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += DataHelper.SQLGetDate(pCS) + " as DTSYS" + Cst.CrLf;
            sqlSelect += ",";
            sqlSelect += DataHelper.SQLGetDate(pCS, true) + " as DTSYSUTC" + Cst.CrLf;
            sqlSelect += DataHelper.SQLFromDual(pCS);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sqlSelect.ToString()))
            {
                dr.Read();
                DateTime dtsys = Convert.ToDateTime(dr["DTSYS"]);
                DateTime dtsysUTC = DateTime.SpecifyKind(Convert.ToDateTime(dr["DTSYSUTC"]), DateTimeKind.Utc);
                ResetDatesysCol(pCS);
                lock (DatesysCol.SyncRoot)
                {
                    string csCol = new CSManager(pCS).GetCSWithoutPwd();
                    DatesysCol.Add(csCol, new Tuple<DatetimeProfiler, DatetimeProfiler>(new DatetimeProfiler(dtsys), new DatetimeProfiler(dtsysUTC)));
                }
            }
        }

        /// <summary>
        /// Get a boolean value for ADO.Net, from a string
        /// Oracle: 0 or 1
        /// Others: false or true
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public static string GetADONetBoolValue(string pSource, string pValue)
        {
            string ret;
            bool isOracle = ConnectionStringCache.GetConnectionStringState(pSource) == ConnectionStringCacheState.isOracle;
            if (BoolFunc.IsTrue(pValue))
                ret = (isOracle ? "1" : "true");
            else
                ret = (isOracle ? "0" : "false");
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <param name="pSQLVariable"></param>
        /// <returns></returns>
        //PL 20120917 Newness
        public static string GetSQLDataDtEnabled(string pSource, string pTableOrAliasTable, string pSQLVariable)
        {
            const string Table = "[Table]";
            const string DtCheck = "[DtCheck]";
            string SQLServerTemplate = "([Table].DTENABLED<=[DtCheck] and isnull([Table].DTDISABLED,dateadd(day,1,[DtCheck]))>[DtCheck])";
            string NoSQLServerTemplate = "([Table].DTENABLED<=[DtCheck] and nvl([Table].DTDISABLED,[DtCheck]+1)>[DtCheck])";

            bool isSqlServer = (ConnectionStringCache.GetConnectionStringState(pSource) == ConnectionStringCacheState.isSqlServer);
            string ret = isSqlServer ? SQLServerTemplate : NoSQLServerTemplate;

            ret = ret.Replace(Table, pTableOrAliasTable);
            if (!pSQLVariable.StartsWith("@"))
                pSQLVariable = "@" + pSQLVariable;
            ret = ret.Replace(DtCheck, pSQLVariable);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <param name="pDtCheck"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource, string pTableOrAliasTable, DateTime pDtCheck)
        {
            return GetSQLDataDtEnabled(pSource, pTableOrAliasTable, pDtCheck, false);
        }

        public static string GetSQLDataDtEnabled(string pSource, string pTableOrAliasTable, DateTime pDtCheck, bool pIsNoTime)
        {
            return GetSQLDataDtEnabled(pSource, pTableOrAliasTable, string.Empty, pDtCheck, pIsNoTime);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <param name="pColumnPrefix"></param>
        /// <param name="pDtCheck"></param>
        /// <param name="pIsNoTime"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource, string pTableOrAliasTable, string pColumnPrefix, DateTime pDtCheck, bool pIsNoTime)
        {
            //PL 20211022 Allow empty pTableOrAliasTable, add pColumnPrefix and use new C# syntax
            //PL 20120321 Change for optimization on Oracle (see FEE_ACTOR.xml)
            //PL 20120329 Add double syntax and isSqlServer
            //const string Template = "([Table].DTENABLED<=[DtCheck] and ([Table].DTDISABLED is null or [Table].DTDISABLED>[DtCheck]))";

            string alias = string.IsNullOrEmpty(pTableOrAliasTable) ? string.Empty : pTableOrAliasTable + ".";
            string dtCheck = (DtFunc.IsDateTimeEmpty(pDtCheck) ? 
                             (pIsNoTime ? DataHelper.SQLGetDateNoTime(pSource): DataHelper.SQLGetDate(pSource)) 
                             : 
                             DataHelper.SQLToDate(pSource, pDtCheck.ToString("yyyyMMdd")));

            if (ConnectionStringCache.GetConnectionStringState(pSource) == ConnectionStringCacheState.isSqlServer)
            {
                //MS SQLServer
                return $"({alias}{pColumnPrefix}DTENABLED<={dtCheck} and isnull({alias}{pColumnPrefix}DTDISABLED,dateadd(day,1,{dtCheck}))>{dtCheck})";
            }
            else
            {
                //Oracle and other
                return $"({alias}{pColumnPrefix}DTENABLED<={dtCheck} and nvl({alias}{pColumnPrefix}DTDISABLED,{dtCheck}+1)>{dtCheck})";
            }
        }

        /// <summary>
        /// Retourne une restriction SQL qui s'appuie sur de la date système du SGBD
        /// </summary>
        /// <param name="pSource"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource)
        {
            return GetSQLDataDtEnabled(pSource, null, new DateTime());
        }

        /// <summary>
        /// Retourne une restriction SQL qui s'appuie sur de la date système du SGBD
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource, string pTableOrAliasTable)
        {
            return GetSQLDataDtEnabled(pSource, pTableOrAliasTable, new DateTime());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <param name="pIsNoTime"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource, string pTableOrAliasTable, bool pIsNoTime)
        {
            return GetSQLDataDtEnabled(pSource, pTableOrAliasTable, new DateTime(), pIsNoTime);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <param name="pDtCheck"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource, Cst.OTCml_TBL pTableOrAliasTable, DateTime pDtCheck)
        {
            return GetSQLDataDtEnabled(pSource, pTableOrAliasTable.ToString(), pDtCheck);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLDataDtEnabled(string pSource, Cst.OTCml_TBL pTableOrAliasTable)
        {
            return GetSQLDataDtEnabled(pSource, pTableOrAliasTable.ToString(), new DateTime());
        }


        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static string GetSQLDataDtDisabled(string pSource, string pTableOrAliasTable, bool pIsNoTime)
        {
            return GetSQLDataDtDisabled(pSource, pTableOrAliasTable, new DateTime(), pIsNoTime);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static string GetSQLDataDtDisabled(string pSource, string pTableOrAliasTable, DateTime pDtCheck, bool pIsNoTime)
        {
            //PL 20211022 Allow empty pTableOrAliasTable and use new C# syntax
            string alias = string.IsNullOrEmpty(pTableOrAliasTable) ? string.Empty : pTableOrAliasTable + ".";
            string dtCheck = (DtFunc.IsDateTimeEmpty(pDtCheck) ?
                             (pIsNoTime ? DataHelper.SQLGetDateNoTime(pSource) : DataHelper.SQLGetDate(pSource))
                             :
                             DataHelper.SQLToDate(pSource, pDtCheck.ToString("yyyyMMdd")));

            return $"(({alias}DTENABLED>{dtCheck}) or ({alias}DTDISABLED is not null and ({alias}DTDISABLED<={dtCheck})))";
        }
        /// <summary>
        /// Retourne le script SQL qui permet de filtrer les données mises à jour aujourd’hui (créées ou modifiées) 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTableOrAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLDataDtUpd(string pSource, string pTableOrAliasTable)
        {
            return $"(({GetSQLDataDtNewOnly(pSource, pTableOrAliasTable)}) or ({GetSQLDataDtUpdOnly(pSource, pTableOrAliasTable)}))";
        }

        // EG [XXXXX][WI437] Nouvelles options de filtrage des données sur les référentiels
        public static string GetSQLDataUserDtUpd(string pSource, string pTableOrAliasTable, int pIdA)
        {
            return $"(({GetSQLDataUserDtNewOnly(pSource, pTableOrAliasTable, pIdA)}) or ({GetSQLDataUserDtUpdOnly(pSource, pTableOrAliasTable, pIdA)}))";
        }

        /// <summary>
        /// Retourne le script SQL qui permet de filtrer les données mises à jour aujourd’hui (modification uniquement) 
        /// </summary>
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// FI 20200820 [25468] refactoring
        public static string GetSQLDataDtUpdOnly(string pSource, string pTableOrAliasTable)
        {
            string ret;
            DbSvrType serverType = DataHelper.GetDbSvrType(pSource);
            switch (serverType)
            {
                case DbSvrType.dbORA:
                    // FI 20200820 [25468] DTUPD étant en UTC convertion en date local pour comparaison avec la date local 
                    ret = $"(trunc(FROM_TZ(cast ({pTableOrAliasTable}.DTUPD as timestamp), 'UTC' ) AT LOCAL)  = TRUNC(SYSDATE))";
                    break;
                case DbSvrType.dbSQL:
                    // FI 20200820 [25468] DTUPD étant en UTC, Spheres applique le décalage horaire du jour entre le fuseau horaire UTC et le fuseau horaire du SGBDR avant comparaison avec la date local
                    // Remarque Je n'ai pas trouvé de méthode plus simple pour convertir un horodatage UTC dans le fuseau horaire local
                    ret = $@"((convert(date,(dateadd(""HOUR"", datediff(""HOUR"", getutcdate(), getdate()),{pTableOrAliasTable}.DTUPD)))) = convert(date,getdate()))";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }
        // EG [XXXXX][WI437] Nouvelles options de filtrage des données sur les référentiels
        public static string GetSQLDataUserDtUpdOnly(string pSource, string pTableOrAliasTable, int pIdA)
        {
            string ret;
            DbSvrType serverType = DataHelper.GetDbSvrType(pSource);
            switch (serverType)
            {
                case DbSvrType.dbORA:
                    // FI 20200820 [25468] DTUPD étant en UTC convertion en date local pour comparaison avec la date local 
                    ret = $"(trunc(FROM_TZ(cast ({pTableOrAliasTable}.DTUPD as timestamp), 'UTC' ) AT LOCAL)  = TRUNC(SYSDATE)) and ({pTableOrAliasTable}.IDAUPD = {pIdA})";
                    break;
                case DbSvrType.dbSQL:
                    // FI 20200820 [25468] DTUPD étant en UTC, Spheres applique le décalage horaire du jour entre le fuseau horaire UTC et le fuseau horaire du SGBDR avant comparaison avec la date local
                    // Remarque Je n'ai pas trouvé de méthode plus simple pour convertir un horodatage UTC dans le fuseau horaire local
                    ret = $@"((convert(date,(dateadd(""HOUR"", datediff(""HOUR"", getutcdate(), getdate()),{pTableOrAliasTable}.DTUPD)))) = convert(date,getdate()))  and ({pTableOrAliasTable}.IDAUPD = {pIdA})";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }
        /// <summary>
        /// Retourne le script SQL qui permet de filtrer les données mises à jour aujourd’hui (création uniquement) 
        /// </summary>
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// FI 20200820 [25468] refactoring
        public static string GetSQLDataDtNewOnly(string pSource, string pTableOrAliasTable)
        {
            string ret;
            DbSvrType serverType = DataHelper.GetDbSvrType(pSource);
            switch (serverType)
            {
                case DbSvrType.dbORA:
                    // FI 20200820 [25468] DTINS étant en UTC convertion en date local pour comparaison avec la date local 
                    ret = $"(trunc(FROM_TZ(cast ({pTableOrAliasTable}.DTINS as timestamp), 'UTC' ) AT LOCAL)  = TRUNC(SYSDATE))";
                    break;
                case DbSvrType.dbSQL:
                    // FI 20200820 [25468] DTINS étant en UTC, Spheres applique le décalage horaire du jour entre le fuseau horaire UTC et le fuseau horaire du SGBDR avant comparaison avec la date local
                    // Je n'ai pas trouvé de méthode simple pour convertir un horodatage dans le fuseau horaire local
                    ret = $@"((convert(date,(dateadd(""HOUR"", datediff(""HOUR"", getutcdate(), getdate()),{pTableOrAliasTable}.DTINS)))) = convert(date,getdate()))";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }
        // EG [XXXXX][WI437] Nouvelles options de filtrage des données sur les référentiels
        public static string GetSQLDataUserDtNewOnly(string pSource, string pTableOrAliasTable, int pIdA)
        {
            string ret;
            DbSvrType serverType = DataHelper.GetDbSvrType(pSource);
            switch (serverType)
            {
                case DbSvrType.dbORA:
                    // FI 20200820 [25468] DTINS étant en UTC convertion en date local pour comparaison avec la date local 
                    ret = $"(trunc(FROM_TZ(cast ({pTableOrAliasTable}.DTINS as timestamp), 'UTC' ) AT LOCAL)  = TRUNC(SYSDATE)) and ({pTableOrAliasTable}.IDAINS = {pIdA})";
                    break;
                case DbSvrType.dbSQL:
                    // FI 20200820 [25468] DTINS étant en UTC, Spheres applique le décalage horaire du jour entre le fuseau horaire UTC et le fuseau horaire du SGBDR avant comparaison avec la date local
                    // Je n'ai pas trouvé de méthode simple pour convertir un horodatage dans le fuseau horaire local
                    ret = $@"((convert(date,(dateadd(""HOUR"", datediff(""HOUR"", getutcdate(), getdate()),{pTableOrAliasTable}.DTINS)))) = convert(date,getdate())) and ({pTableOrAliasTable}.IDAINS = {pIdA})";
                    break;
                default:
                    throw (new ArgumentException("No match for connection found", "connectionString"));
            }
            return ret;
        }

        /// <summary>
        /// Retourne la colonne ID associée à la table
        /// <para>Exemple ACTOR: retrourne IDA</para>
        /// </summary>
        /// <param name="pTable"></param>
        /// <returns></returns>
        /// FI 20170223 [22883] Add
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public static string GetColunmID(string pTable)
        {

            string columnTable = string.Empty;
            bool isCastOk = true;
            Cst.OTCml_TBL table = Cst.OTCml_TBL.TRADEACTOR;
            //
            try
            {
                table = (Cst.OTCml_TBL)System.Enum.Parse(typeof(Cst.OTCml_TBL), pTable, true);
            }
            catch { isCastOk = false; }
            //
            if (isCastOk)
            {
                switch (table)
                {
                    case Cst.OTCml_TBL.ACTOR:
                    case Cst.OTCml_TBL.ACTORROLE:
                    case Cst.OTCml_TBL.BOOK:
                    case Cst.OTCml_TBL.CURRENCY:
                    case Cst.OTCml_TBL.INSTRUMENT:
                    case Cst.OTCml_TBL.MARKET:
                    case Cst.OTCml_TBL.PRODUCT:
                    case Cst.OTCml_TBL.TRADE:
                        columnTable = "ID" + pTable.ToString().Substring(0, 1);
                        break;
                    // FI 20170223 [22883] add
                    case Cst.OTCml_TBL.COMMODITYCONTRACT:
                        columnTable = "IDCC";
                        break;
                    // FI 20170223 [22883] add
                    case Cst.OTCml_TBL.VW_COMMODITYCONTRACT:
                        columnTable = GetColunmID(Cst.OTCml_TBL.COMMODITYCONTRACT.ToString());
                        break;
                    case Cst.OTCml_TBL.DERIVATIVECONTRACT:
                        columnTable = "IDDC";
                        break;
                    case Cst.OTCml_TBL.CASHBALANCE:
                    case Cst.OTCml_TBL.CSS:
                    case Cst.OTCml_TBL.ENTITY:
                    case Cst.OTCml_TBL.NCS:
                    case Cst.OTCml_TBL.RISKMARGIN:
                        columnTable = GetColunmID(Cst.OTCml_TBL.ACTOR.ToString());
                        break;
                    case Cst.OTCml_TBL.ENTITYMARKET:
                        columnTable = "IDEM";
                        break;
                    case Cst.OTCml_TBL.BUSINESSCENTER:
                    case Cst.OTCml_TBL.HOLIDAYCALCULATED:
                    case Cst.OTCml_TBL.HOLIDAYMISC:
                    case Cst.OTCml_TBL.HOLIDAYMONTHLY:
                    case Cst.OTCml_TBL.HOLIDAYWEEKLY:
                    case Cst.OTCml_TBL.HOLIDAYYEARLY:
                        columnTable = "IDBC";
                        break;
                    case Cst.OTCml_TBL.RATEINDEX:
                    case Cst.OTCml_TBL.SELFCOMPOUNDING_CF:
                    case Cst.OTCml_TBL.SELFCOMPOUNDING_AI:
                    case Cst.OTCml_TBL.SELFCOMPOUNDING_V:
                        columnTable = "IDRX";
                        break;
                    //case Cst.OTCml_TBL.FPML_ENUM:
                    case Cst.OTCml_TBL.VW_ALL_ENUM:
                    case Cst.OTCml_TBL.ENUM:
                        columnTable = "VALUE";
                        break;
                    case Cst.OTCml_TBL.TRACKER_L:
                        columnTable = "IDTRK_L";
                        break;
                    case Cst.OTCml_TBL.TRADETRAIL:
                    case Cst.OTCml_TBL.TRADESTCHECK:
                    case Cst.OTCml_TBL.TRADESTMATCH:
                    case Cst.OTCml_TBL.TRADEINSTRUMENT:
                    case Cst.OTCml_TBL.VW_ASSET_DEBTSECURITY:
                    case Cst.OTCml_TBL.TRADEXML:
                        columnTable = "IDT";
                        break;
                    case Cst.OTCml_TBL.EVENT:
                    case Cst.OTCml_TBL.EVENTASSET:
                    case Cst.OTCml_TBL.EVENTDET:
                    case Cst.OTCml_TBL.EVENTPRICING:
                    case Cst.OTCml_TBL.VW_EVENT:
                        columnTable = "IDE";
                        break;
                    case Cst.OTCml_TBL.EVENTCLASS:
                        columnTable = "IDEC";
                        break;
                    case Cst.OTCml_TBL.INSTRUMENTOF:
                        columnTable = "IDI_INSTR";
                        break;
                    case Cst.OTCml_TBL.SSIDB:
                        columnTable = "IDSSIDB";
                        break;
                    case Cst.OTCml_TBL.POSREQUEST:
                        columnTable = "IDPR";
                        break;
                    case Cst.OTCml_TBL.VW_TRADE:
                    case Cst.OTCml_TBL.VW_TRADEADMIN:
                    case Cst.OTCml_TBL.VW_TRADEDEBTSEC:
                    case Cst.OTCml_TBL.VW_INVOICEFEESDETAIL:
                        columnTable = GetColunmID(Cst.OTCml_TBL.TRADE.ToString());
                        break;
                    case Cst.OTCml_TBL.VW_BOOK_VIEWER:
                        columnTable = GetColunmID(Cst.OTCml_TBL.BOOK.ToString());
                        break;
                    case Cst.OTCml_TBL.VW_INSTR_PRODUCT:
                        columnTable = GetColunmID(Cst.OTCml_TBL.INSTRUMENT.ToString());
                        break;
                    case Cst.OTCml_TBL.VW_ENTITYCSS:
                    case Cst.OTCml_TBL.VW_ENTITY_CSSCUSTODIAN:
                        columnTable = "EMKEY";
                        break;
                    case Cst.OTCml_TBL.VW_MARKET_IDENTIFIER:
                        columnTable = GetColunmID(Cst.OTCml_TBL.MARKET.ToString());
                        break;
                    case Cst.OTCml_TBL.VW_MATURITYRULE:
                        columnTable = GetColunmID(Cst.OTCml_TBL.MATURITYRULE.ToString());
                        break;
                    default:
                        if (pTable.ToString().StartsWith("ASSET_") || pTable.ToString().StartsWith("VW_ASSET_"))
                            columnTable = "IDASSET";
                        else if (pTable.ToString().StartsWith("QUOTE_") && pTable.ToString().EndsWith("H"))
                            columnTable = "IDQUOTE_H";
                        else if (pTable.ToString().StartsWith("QUOTE_"))
                            columnTable = "IDQUOTE";
                        else
                            columnTable = "ID" + pTable.ToString();
                        break;
                }
            }
            //20090729 FI => Comme cela c'est mieux
            if (StrFunc.IsEmpty(columnTable))
                throw new NotImplementedException(StrFunc.AppendFormat("Table {0} is not Implemented", pTable));
            //
            return columnTable;
        }

        /// <summary>
        /// Add a signature 
        /// </summary>
        /// <param name="opQuery"></param>
        /// <param name="pSignature"></param>
        public static void SQLAddSignature(ref string opQuery, string pSignature)
        {
            SQLAddSignature(ref opQuery, pSignature, 2, 80);
        }
        public static void SQLAddSignature(ref string opQuery, string pSignature, int pTabSpace, int pLen)
        {
            string s = "".PadRight(pTabSpace, ' ');

            opQuery = Cst.CrLf
                    + "/* -- auto-generated by " + pSignature + " ".PadRight(pLen - 27 - pSignature.Length, '-') + " */" + Cst.CrLf
                    + s + "(" + Cst.CrLf + opQuery + s + ")"
                    + Cst.CrLf
                    + "/* ---------------------" + "-".PadRight(pLen - 27, '-') + " */" + Cst.CrLf;
        }

        /// <summary>
        /// Add a SQL comment
        /// </summary>
        /// <param name="pComment"></param>
        /// <returns></returns>
        public static string GetSQLComment(string pComment)
        {
            return GetSQLComment(pComment, 80);
        }
        public static string GetSQLComment(string pComment, int pLen)
        {
            return "/* ***** " + pComment + " ".PadRight(pLen - 12 - pComment.Length, '*') + " */" + Cst.CrLf;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTable"></param>
        /// <param name="pSQLJoinTypeEnum"></param>
        /// <param name="pColumnMainTable"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pDataEnum"></param>
        /// <returns></returns>
        public static string GetSQLJoin(string pSource, Cst.OTCml_TBL pTable, SQLJoinTypeEnum pSQLJoinTypeEnum, string pColumnMainTable, string pAliasTable, DataEnum pDataEnum)
        {
            string extendCondition = string.Empty;
            string join = string.Empty;
            string columnTable = GetColunmID(pTable.ToString());

            if (StrFunc.IsFilled(columnTable))
            {
                switch (pSQLJoinTypeEnum)
                {
                    case SQLJoinTypeEnum.Inner:
                        join = SQLCst.INNERJOIN_DBO;
                        break;
                    case SQLJoinTypeEnum.Left:
                        join = SQLCst.LEFTJOIN_DBO;
                        break;
                    case SQLJoinTypeEnum.Right:
                        join = SQLCst.RIGHTJOIN_DBO;
                        break;
                    case SQLJoinTypeEnum.Full:
                    default:
                        join = SQLCst.FULLJOIN_DBO;
                        break;
                }
                join += pTable.ToString() + " " + pAliasTable + SQLCst.ON;
                join += " (" + extendCondition + pAliasTable + "." + columnTable + "=" + pColumnMainTable + ")" + Cst.CrLf;
                if (pDataEnum == DataEnum.EnabledOnly)
                    join += SQLCst.AND + "(" + GetSQLDataDtEnabled(pSource, pAliasTable) + ")" + Cst.CrLf;
            }
            return join;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTable"></param>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pColumnMainTable"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pIsDataEnabledOnly"></param>
        /// <returns></returns>
        public static string GetSQLJoin(string pSource, Cst.OTCml_TBL pTable, bool pIsInnerJoin, string pColumnMainTable, string pAliasTable, bool pIsDataEnabledOnly)
        {
            SQLJoinTypeEnum sqlJoinTypeEnum = (pIsInnerJoin ? SQLJoinTypeEnum.Inner : SQLJoinTypeEnum.Left);
            DataEnum dataEnum = (pIsDataEnabledOnly ? DataEnum.EnabledOnly : DataEnum.All);
            return GetSQLJoin(pSource, pTable, sqlJoinTypeEnum, pColumnMainTable, pAliasTable, dataEnum);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTable"></param>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pColumnMainTable"></param>
        /// <returns></returns>
        public static string GetSQLJoin(string pSource, Cst.OTCml_TBL pTable, bool pIsInnerJoin, string pColumnMainTable)
        {
            string aliasTable = pTable.ToString().Substring(0, 1).ToLower();
            return GetSQLJoin(pSource, pTable, pIsInnerJoin, pColumnMainTable, aliasTable, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTable"></param>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pColumnMainTable"></param>
        /// <param name="pAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLJoin(string pSource, Cst.OTCml_TBL pTable, bool pIsInnerJoin, string pColumnMainTable, string pAliasTable)
        {
            return GetSQLJoin(pSource, pTable, pIsInnerJoin, pColumnMainTable, pAliasTable, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pTable"></param>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pColumnMainTable"></param>
        /// <param name="pIsDataEnabledOnly"></param>
        /// <returns></returns>
        public static string GetSQLJoin(string pSource, Cst.OTCml_TBL pTable, bool pIsInnerJoin, string pColumnMainTable, bool pIsDataEnabledOnly)
        {
            string aliasTable = pTable.ToString().Substring(0, 1).ToLower();
            return GetSQLJoin(pSource, pTable, pIsInnerJoin, pColumnMainTable, aliasTable, pIsDataEnabledOnly);
        }
        /// <summary>
        /// Retourne la jointure externe qui permet de jointurer avec la table STATISITIC
        /// <para>Exemple Retourne left outer join ACTOR_S on ACTOR_S.IDA = {Alias}.IDA </para>
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pAliasTable">Alias de la table sur laquellla s'applique la jointure</param>
        /// <returns></returns>
        public static string GetSQLJoin_Statistic(string pTable, string pAliasTable)
        {
            string columnTable = GetColunmID(pTable);
            string ret = string.Empty;
            //
            if (StrFunc.IsFilled(columnTable))
            {
                ret = SQLCst.LEFTJOIN_DBO + pTable + "_S" + SQLCst.ON;
                ret += " (" + pTable + "_S." + columnTable + "=" + pAliasTable + "." + columnTable + ")" + Cst.CrLf;
            }
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable"></param>
        /// <returns></returns>
        public static string GetSQLOrderBy_Statistic(string pCS, string pTable)
        {
            return GetSQLOrderBy_Statistic(pCS, pTable, string.Empty);
        }
        /// <summary>
        /// Retourne le order by qui permet tri les données selon la fréquence de leur utilisation 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable"></param>
        /// <param name="pSqlOrderBy"></param>
        /// <returns></returns>
        public static string GetSQLOrderBy_Statistic(string pCS, string pTable, string pSqlOrderBy)
        {
            if (Cst.OTCml_TBL.VW_ASSET_RATEINDEX.ToString() == pTable)
                pTable = Cst.OTCml_TBL.ASSET_RATEINDEX.ToString();
            //
            if (StrFunc.IsFilled(pSqlOrderBy))
            {
                pSqlOrderBy = pSqlOrderBy.Trim();
                if (!pSqlOrderBy.EndsWith(","))
                    pSqlOrderBy += ", ";
            }
            return SQLCst.ORDERBY + pSqlOrderBy + DataHelper.SQLIsNullChar(pCS, pTable + "_S.USEFREQUENCY", "1");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pColumnMainTable"></param>
        /// <param name="pAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLJoinNotePad(bool pIsInnerJoin, string pMainTable, string pColumnMainTable, string pAliasTable)
        {
            string join = (pIsInnerJoin ? SQLCst.INNERJOIN_DBO : SQLCst.LEFTJOIN_DBO) + Cst.OTCml_TBL.NOTEPAD + " " + pAliasTable + SQLCst.ON;
            join += " (" + pAliasTable + ".TABLENAME='" + pMainTable + "' and " + pAliasTable + ".ID=" + pColumnMainTable + ")" + Cst.CrLf;
            return join;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pMainTable"></param>
        /// <param name="pColumnMainTable"></param>
        /// <param name="pAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLJoinNotePad(bool pIsInnerJoin, Cst.OTCml_TBL pMainTable, string pColumnMainTable, string pAliasTable)
        {
            return GetSQLJoinNotePad(pIsInnerJoin, pMainTable.ToString(), pColumnMainTable, pAliasTable); ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pMainTable"></param>
        /// <param name="pColumnMainTable"></param>
        /// <returns></returns>
        public static string GetSQLJoinNotePad(bool pIsInnerJoin, string pMainTable, string pColumnMainTable)
        {
            return GetSQLJoinNotePad(pIsInnerJoin, pMainTable, pColumnMainTable, "n");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsInnerJoin"></param>
        /// <param name="pMainTable"></param>
        /// <param name="pColumnMainTable"></param>
        /// <returns></returns>
        public static string GetSQLJoinNotePad(bool pIsInnerJoin, Cst.OTCml_TBL pMainTable, string pColumnMainTable)
        {
            return GetSQLJoinNotePad(pIsInnerJoin, pMainTable.ToString(), pColumnMainTable, "n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pIsWithLastComma"></param>
        /// <returns></returns>
        public static string GetSQLColumns(Cst.OTCml_TBL pTable, string pAliasTable, bool pIsWithLastComma)
        {
            string c = string.Empty;

            switch (pTable)
            {
                case Cst.OTCml_TBL.STMATCH:
                case Cst.OTCml_TBL.STCHECK:
                    c += pAliasTable + ".DISPLAYNAME as " + pAliasTable + "DISPLAYNAME,";
                    c += pAliasTable + ".DESCRIPTION as " + pAliasTable + "DESCRIPTION,";
                    c += pAliasTable + ".NOTE as " + pAliasTable + "NOTE,";
                    c += pAliasTable + ".FORECOLOR as " + pAliasTable + "FORECOLOR,";
                    c += pAliasTable + ".BACKCOLOR as " + pAliasTable + "BACKCOLOR,";
                    break;
            }
            if ((!pIsWithLastComma) && (c.EndsWith(",")))
            {
                char[] cTrim = (",").ToCharArray();
                c = c.TrimEnd(cTrim);
            }
            return c + Cst.CrLf;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pAliasTable"></param>
        /// <returns></returns>
        public static string GetSQLColumns(Cst.OTCml_TBL pTable, string pAliasTable)
        {
            return GetSQLColumns(pTable, pAliasTable, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pWhere"></param>
        /// <param name="pConnectionString"></param>
        /// <param name="pCommandType"></param>
        /// <returns></returns>
        public static byte[] GetColumnBytesArrayFromDatabase(string pTableName, string pColumnName, string pWhere, string pConnectionString, System.Data.CommandType pCommandType)
        {
            byte[] retValue;
            retValue = null;
            string SQLSelect = string.Empty;
            SQLSelect += SQLCst.SELECT + pColumnName + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + pTableName + Cst.CrLf;
            SQLSelect += pWhere;
            IDataReader dr = DataHelper.ExecuteReader(pConnectionString, pCommandType, SQLSelect);
            if (dr.Read())
            {
                if (dr[pColumnName] != null && dr[pColumnName].ToString().Length > 0)
                    retValue = (byte[])dr[pColumnName];
            }
            dr.Close();
            return retValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <returns></returns>
        public static string GetXMLNamespace_3_0(string pCs)
        {
            string ret = string.Empty;
            //
            if (DataHelper.IsDbSqlServer(pCs))
            {
                ret += @"declare default element namespace """ + Cst.FpML_Namespace_4_4 + @""";" + Cst.CrLf;
                ret += @"declare namespace efs=""" + Cst.EFSmL_Namespace_3_0 + @""";" + Cst.CrLf;
                ret += @"declare namespace fixml=""" + Cst.FixML_Namespace_5_0_SP1 + @""";" + Cst.CrLf;
            }
            else
            {
                // RD 20100517 / Bug Oracle: 
                //  1 - mettre le symbol "=" juste après le namespace par défaut
                //  2 - ne pas mettre de saut de lignes entre les différents namespaces
                ret += @"xmlns=""" + Cst.FpML_Namespace_4_4 + @"""";
                ret += @" xmlns:efs=""" + Cst.EFSmL_Namespace_3_0 + @"""";
                ret += @" xmlns:fixml=""" + Cst.FixML_Namespace_5_0_SP1 + @"""";
            }
            return ret;
        }

        /// <summary>
        ///  Retourne l'expression SQL qui permet d'obtenir un XML:ID 
        /// </summary>
        /// <returns></returns>
        /// FI 20161027 [22151] Add
        /// RD 20171013 [23507] Use "coalesce" instead of "isnull" and "nvl"
        public static string GetACTORXMLId(string pCS, string pAliasTableActor, string pAliasColumnResult)
        {
            string ret;
            if (DataHelper.IsDbSqlServer(pCS))
            {
                ret = StrFunc.AppendFormat(@"case when isnumeric(substring(coalesce({0}.BIC, {0}.IDENTIFIER), 1, 1)) = 1 
                then 'id' +  coalesce({0}.BIC, {0}.IDENTIFIER)
                else coalesce({0}.BIC, {0}.IDENTIFIER)
                end as {1}", pAliasTableActor, pAliasColumnResult);
            }
            else if (DataHelper.IsDbOracle(pCS))
            {
                ret = StrFunc.AppendFormat(@"case when substr(coalesce({0}.BIC, {0}.IDENTIFIER), 1, 1) in ('1','2','3','4','5','6','7','8','9')
                then 'id' || coalesce({0}.BIC, {0}.IDENTIFIER)
                else coalesce({0}.BIC, {0}.IDENTIFIER)
                end as {1}", pAliasTableActor, pAliasColumnResult);
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", DataHelper.GetDbSvrType(pCS)));
            }
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OTCmlHelper
    {
        /// <summary>
        /// Remplace dans un string les mots clefs %%DH:DTENABLED_WHERE_PREDICATE%% 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120803 add
        private static string ReplaceDtEnabledKeyword(string pCS, string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.DH_START) >= 0))
            {
                //
                //%%DH:DTENABLED_WHERE%%(aliasTable)
                int guard = 0;
                while (ret.Contains(Cst.DH_DTENABLED_WHERE_PREDICATE) & (guard < 100))
                {
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.DH_DTENABLED_WHERE_PREDICATE);
                    string alias = arg[0];
                    //
                    string where = OTCmlHelper.GetSQLDataDtEnabled(pCS, alias);
                    ret = ret.Replace(Cst.DH_DTENABLED_WHERE_PREDICATE + "(" + alias + ")", where);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%DH:DH_FIRSTDAY_OF_MONTH%% 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120803 add
        private static string ReplaceFirstDayOfMonth(string pCS, string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.DH_START) >= 0))
            {
                //
                //%%DH:DH_FIRSTDAY_OF_MONTH%%(col)
                int guard = 0;
                while (ret.Contains(Cst.DH_FIRSTDAY_OF_MONTH) & (guard < 100))
                {
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.DH_FIRSTDAY_OF_MONTH);
                    string col = arg[0];
                    //
                    string sql = DataHelper.SQLFirstDayOfMonth(pCS, col);
                    ret = ret.Replace(Cst.DH_FIRSTDAY_OF_MONTH + "(" + col + ")", sql);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%DH:DH_LASTDAY_OF_MONTH%% 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120803 add
        private static string ReplaceLastDayOfMonth(string pCS, string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.DH_START) >= 0))
            {
                //
                //%%DH:DH_LASTDAY_OF_MONTH%%(col)
                int guard = 0;
                while (ret.Contains(Cst.DH_LASTDAY_OF_MONTH) & (guard < 100))
                {
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.DH_LASTDAY_OF_MONTH);
                    string col = arg[0];
                    //
                    string sql = DataHelper.SQLLastDayOfMonth(pCS, col);
                    ret = ret.Replace(Cst.DH_LASTDAY_OF_MONTH + "(" + col + ")", sql);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string le mot clef %%DH:SQLIF%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        private static string ReplaceSQLIF(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) & (ret.IndexOf(Cst.DH_START) >= 0))
            {
                //%%DH:SQLIF%%[param;paramValue;sql]
                int guard = 0;
                while (ret.Contains(Cst.DH_SQLIF) & (guard < 100))
                {
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.DH_SQLIF, "[", "]", ";");
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false);
                    //
                    string sql = string.Empty;
                    if (ArrFunc.Count(arg) == 3)
                    {
                        // RD 20130114 [18349] 
                        // Si le premier argument est toujours "%%PARAMn",
                        // cela veut dire qu'il n'existe pas dans l'URL un paramètre "Pn" correspondant
                        // Dans ce cas, considérer par défaut la valeur "COMMON"
                        if ((arg[0] == arg[1]) || (arg[0].StartsWith(Cst.PARAM_START) && arg[1] == "COMMON"))
                            sql = arg[2];
                    }
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Arguments error for (0)", Cst.DH_SQLIF));
                    //
                    ret = ret.Replace(Cst.DH_SQLIF + "[" + arg2 + "]", sql);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%DH:XXXXX%% 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120803 add
        /// RD 20121009 add ReplaceSQLIF
        public static string ReplaceKeyword(string pCS, string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.DH_START) >= 0))
            {
                ret = ReplaceDtEnabledKeyword(pCS, pData);
                ret = ReplaceFirstDayOfMonth(pCS, ret);
                ret = ReplaceLastDayOfMonth(pCS, ret);
                ret = ReplaceSQLIF(ret);
            }
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SQLWhere
    {
        public enum StartEnum
        {
            /// <summary>
            /// La chaîne constitué commence par Where
            /// </summary>
            StartWithWhere,
            /// <summary>
            /// La chaîne constitué ne commence pas par Where
            /// </summary>
            StartWithoutWhere
        }

        #region Members
        private string _sqlWhere;
        private readonly StartEnum _startEnum;
        #endregion Members

        #region propertie
        private bool IsStartWithWhere
        {
            get { return (_startEnum == StartEnum.StartWithWhere); }
        }
        #endregion

        //
        #region constructor
        public SQLWhere()
        {
            _sqlWhere = string.Empty;
            _startEnum = StartEnum.StartWithWhere;
        }
        public SQLWhere(string pString)
        {
            _sqlWhere = string.Empty;
            _startEnum = StartEnum.StartWithWhere;
            //
            if (StrFunc.IsFilled(pString))
            {
                if (pString.IndexOf(SQLCst.WHERE) > -1)
                    _sqlWhere = pString;
                else
                    _sqlWhere = SQLCst.WHERE + pString;
            }
        }
        public SQLWhere(StartEnum pStart)
        {
            _sqlWhere = string.Empty;
            _startEnum = pStart;
        }
        public SQLWhere(bool pIsNoWhere)
        {
            _sqlWhere = string.Empty;
            if (pIsNoWhere)
                _startEnum = StartEnum.StartWithoutWhere;
            else
                _startEnum = StartEnum.StartWithWhere;
        }
        #endregion constructor
        //
        #region public Append
        /// <summary>
        /// Ajoute la string "{And}+{pString}"
        /// </summary>
        /// <param name="pString"></param>
        public void Append(string pString)
        {
            Append(pString, false);
        }
        /// <summary>
        /// Ajoute la string "{pSeparator}+{pString}"
        /// <para>Au premier ajout {pSeparator} est remplacé par Where</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pSeparator"></param>
        public void Append(string pString, string pSeparator)
        {
            Append(pString, pSeparator, false);
        }
        /// <summary>
        /// Ajoute la string "{And}+{pString}"
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pCrLf">si true ajoute un retour à la ligne avant</param>
		public void Append(string pString, bool pCrLf)
        {
            Append(pString, SQLCst.AND, pCrLf);
        }
        /// <summary>
        /// Ajoute la string "{pSeparator}+{pString}"
        /// <para>Au premier ajout {pSeparator} est remplacé par Where (si StartEnum.StartWithWhere)</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pSeparator"></param>
        /// <param name="pCrLf">si true ajoute un retour à la ligne avant</param>
        public void Append(string pString, string pSeparator, bool pCrLf)
        {
            if ((StrFunc.IsFilled(pString)) && (pString.TrimEnd().Length > 0))
            {
                if (pCrLf)
                    _sqlWhere += Cst.CrLf;
                //
                if (IsStartWithWhere && StrFunc.IsEmpty(_sqlWhere))
                    _sqlWhere += SQLCst.WHERE;
                else
                    _sqlWhere += pSeparator;
                //
                _sqlWhere += pString;
            }
        }

        #endregion public Append
        //
        #region public ToString
        public override string ToString()
        {
            return _sqlWhere;
        }
        public string ToString(bool pCrLf)
        {
            return (pCrLf? Cst.CrLf:string.Empty) + _sqlWhere;
        }
        #endregion public ToString
        //
        #region public Length
        public int Length()
        {
            return _sqlWhere.Length;
        }
        #endregion public Length
    }


    /// <summary>
    ///  Représente une query SQL avec ses paramètres
    /// </summary>
    public class QueryParameters
    {
        #region Membres
        /// <summary>
        /// La connectionString où la query va être executée
        /// </summary>
        private string _cs;
        /// <summary>
        /// la requêtes (la query est écrite au format Spheres)
        /// <para>Format Spheres : les paramètres sont systématiquement écrits avec des @, "||" est un opérateur utilisé pour effectuer de la concaténation, etc...</para>
        /// </summary>
        private string _query;
        /// <summary>
        /// les paramètres de la requêtes
        /// </summary>
        private DataParameters _parameters;

        #endregion Membres

        #region Accessors

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("CS", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public String Cs
        {
            get { return _cs; }
            set { _cs = value; }
        }

        /// <summary>
        /// Obtient ou définit la query SQL au format Spheres (@ est le prefix des parametres)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("Query", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public String Query
        {
            get { return _query; }
            set { _query = value; }
        }

        /// <summary>
        /// Obtient ou définit les paramètres de la query
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DataParameters", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataParameters Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }

        /// <summary>
        /// Obtient la query au format SQLServer
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public String QuerySqlServer
        {
            get { return GetQueryExecSqlServer(); }
        }

        /// <summary>
        /// Obtient la query au format associé à la ConnectionString, le nom des paramètres est remplacé par leurs valeurs respectives.  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public String QueryReplaceParameters
        {
            get { return GetQueryReplaceParameters(true, false); }
        }
        /// <summary>
        /// Obtient la query au format associé à la ConnectionString, le nom des paramètres est mis en commentaire et complété par leurs valeurs respectives .  
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public String QueryReplaceAndCommentParameters
        {
            get { return GetQueryReplaceParameters(true, true); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Hint
        {
            get
            {
                string hint = null;
                // EG 20140204 [19586]
                if (Query.Contains("/* Spheres:Hint"))
                {
                    int hintStart = Query.IndexOf("/* Spheres:Hint");
                    int hintEnd = Query.IndexOf("*/", hintStart);
                    hint = Query.Substring(hintStart, hintEnd - hintStart);
                }

                return hint;
            }
        }

        /// <summary>
        /// Obtient la query SQL au format Spheres (@ est le prefix des parametres)
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string QueryHint
        {
            get
            {
                string hintQuery = Query;
                string hintCmd = Hint;

                if (!String.IsNullOrEmpty(hintCmd))
                {
                    if (hintCmd.Contains("NOPARAMS"))
                    {
                        hintQuery = QueryReplaceParameters;
                    }

                    if (hintCmd.Contains("ARITHABORT_ON"))
                    {
                        if (DataHelper.IsDbSqlServer(Cs))
                        {
                            //PL 20190226 Add semi-colon
                            hintQuery = "SET ARITHABORT ON;" + Cst.CrLf + hintQuery;
                        }
                    }
                }

                return hintQuery;
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pQuery"></param>
        /// <param name="pParameters">null est autorisé</param>
        public QueryParameters(string pCs, string pQuery, DataParameters pParameters)
        {
            _cs = pCs;
            _query = pQuery;
            _parameters = pParameters;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Retourne la Query au Format SqlServer 
        /// </summary>
        /// <returns></returns>
        /// FI 20220805 [XXXXX] Gestion de datetime2
        public String GetQueryExecSqlServer()
        {
            StrBuilder ret = new StrBuilder(string.Empty);
            //
            StrBuilder strDeclare = new StrBuilder(string.Empty);
            strDeclare += @"declare @sql nvarchar(max)" + Cst.CrLf;
            StrBuilder strSet = new StrBuilder(string.Empty);
            strSet += @"set @sql = N'" + _query.Replace("'", "''") + "'" + Cst.CrLf;
            //
            if (null != _parameters)
            {
                DataParameter[] dataParameter = _parameters.GetArrayParameter();
                //Declaration des paramètres
                if (_parameters.Count > 0)
                {
                    for (int i = 0; i < dataParameter.Length; i++)
                        strDeclare += "declare " + dataParameter[i].GetSqlServerDeclaration() + Cst.CrLf;
                    strDeclare += @"declare @paramList nvarchar(4000)" + Cst.CrLf;
                }
                //Affectation des paramètres
                if (_parameters.Count > 0)
                {
                    for (int i = 0; i < dataParameter.Length; i++)
                    {
                        strSet += @"set @" + dataParameter[i].ParameterKey + "=";
                        SqlDbType sqldbType = DataHelper.DbTypeToSqlDbType(dataParameter[i].DbType);
                        switch (sqldbType)
                        {
                            case SqlDbType.Char:
                            case SqlDbType.NChar:
                            case SqlDbType.VarChar:
                            case SqlDbType.NVarChar:
                                strSet += "'" + dataParameter[i].Value.ToString() + "'";
                                break;
                            case SqlDbType.Int:
                                strSet += Convert.ToInt32(dataParameter[i].Value).ToString();
                                break;
                            case SqlDbType.Decimal:
                            case SqlDbType.Float:
                                strSet += StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(dataParameter[i].Value));
                                break;
                            case SqlDbType.SmallDateTime:
                            case SqlDbType.DateTime:
                                strSet += "'" + DtFunc.DateTimeToStringISO((DateTime)dataParameter[i].Value) + "'";
                                break;
                            case SqlDbType.DateTime2:
                                strSet += "'" + DtFunc.DateTimeToString((DateTime)dataParameter[i].Value, "yyyy-MM-ddTHH:mm:ss.fffffff") + "'";
                                break;
                            case SqlDbType.Bit:
                                strSet += Convert.ToInt32(dataParameter[i].Value).ToString();
                                break;

                            default:
                                throw new NotImplementedException($"Type:{sqldbType} is not implemented");
                        }
                        strSet += Cst.CrLf;
                    }
                    //
                    string paramList = string.Empty;
                    for (int i = 0; i < dataParameter.Length; i++)
                        paramList += dataParameter[i].GetSqlServerDeclaration() + ",";
                    if (StrFunc.IsFilled(paramList))
                        paramList = paramList.Substring(0, paramList.Length - 1);
                    strSet += "set @paramList = '" + paramList + "'";
                }
                //
            }
            ret += strDeclare.ToString() + Cst.CrLf;
            ret += strSet.ToString() + Cst.CrLf;
            ret += "exec sp_executesql @sql";

            if ((null != _parameters) && _parameters.Count > 0)
                ret += ",@paramList," + _parameters.GetSqlServerParamList(); ;

            return ret.ToString();
        }

        /// <summary>
        /// Retourne la requête au format spécifié via la connectionString avec les éventuels paramètres remplacés par leur valeurs respectives.
        /// <para>La requête est au format du SGBD</para>
        /// </summary>
        /// <returns></returns>
        public String GetQueryReplaceParameters()
        {
            return GetQueryReplaceParameters(true, false);
        }

        /// <summary>
        /// Retourne la requête avec les éventuels paramètres remplacés par leur valeurs respectives.
        /// </summary>
        /// <param name="pTransformQuery">Si true, on applique au préalable une transformation de la query au format spécifiée via la connectionString</param>
        /// <returns></returns>
        public String GetQueryReplaceParameters(bool pTransformQuery)
        {
            return GetQueryReplaceParameters(pTransformQuery, false);
        }

        /// <summary>
        /// Retourne la requête avec les éventuels paramètres remplacés par leur valeurs respectives.
        /// </summary>
        /// <param name="pTransformQuery">Si true, on applique au préalable une transformation de la query au format spécifiée via la connectionString</param>
        /// <param name="pCommentParamName">Si true, conserve le nom du paramètre en commentaire </param>
        /// <returns></returns>
        public String GetQueryReplaceParameters(bool pTransformQuery, bool pCommentParamName)
        {
            string retQuery = (pTransformQuery ? TransformQuery() : Query);

            if (null != Parameters)
            {
                DataParameter[] dataParameter = Parameters.GetArrayParameter();
                if (ArrFunc.IsFilled(dataParameter))
                {
                    CSManager csManager = new CSManager(Cs);
                    DbSvrType svrType = csManager.GetDbSvrType();

                    if (false == pTransformQuery)
                    {
                        // S'il n'y a pas eu transformation de la query, celle-çi est au format EFS.
                        // Dans le format EFS les paramètres sont écrits avec des @, il convient de les remplacer par les paramètres au format du SGBD)
                        foreach (DataParameter item in dataParameter)
                            retQuery = retQuery.Replace("@" + item.ParameterKey, item.ParameterName);
                    }
                    retQuery = DataHelper.ReplaceParametersInQuery(retQuery, svrType, pCommentParamName, dataParameter);
                }
            }

            return retQuery;
        }

        /// <summary>
        /// Retourne la query formattée selon le moteur SQL précisé via la chaîne de connexion 
        /// </summary>
        /// <returns></returns>
        public String TransformQuery()
        {
            CSManager csManager = new CSManager(Cs);

            string ret;
            if (null != Parameters)
                ret = DataHelper.TransformQuery2(csManager.Cs, CommandType.Text, Query, Parameters.GetArrayDbParameter());
            else
                ret = DataHelper.TransformQuery2(csManager.Cs, CommandType.Text, Query, null);

            return ret;
        }

        public IDbDataParameter[] GetArrayDbParameterHint()
        {
            string hintCmd = Hint;

            if ((!String.IsNullOrEmpty(hintCmd)) && (hintCmd.Contains("NOPARAMS")))
            {
                return null;
            }
            else
            {
                return Parameters.GetArrayDbParameter();
            }
        }
        #endregion Methods
    }


    

    /// <summary>
    /// Fichier stocké dans un champ SQL
    /// </summary>
    public class LOFile
    {
        /// <summary>
        /// 
        /// </summary>
        private byte[] _fileContent;

        /// <summary>
        /// 
        /// </summary>
        public Boolean FileContentSpecified
        {
            get { return (null != FileContent); }
        }
        /// <summary>
        /// Contenu binaire
        /// </summary>
        public byte[] FileContent
        {
            get { return _fileContent; }
        }

        /// <summary>
        ///  
        /// </summary>
        public Boolean FileNameSpecified
        {
            get { return StrFunc.IsFilled(FileName); }
        }
        /// <summary>
        /// Nom du fichier
        /// </summary>
        public string FileName
        {
            get; set;
        }

        public Boolean FileTypeSpecified
        {
            get { return StrFunc.IsFilled(FileType); }
        }
        /// <summary>
        /// Type de fichier (exemple application/pdf, text/xml,etc) 
        /// </summary>
        public string FileType
        {
            get; set;
        }
        /// <summary>
        /// 
        /// </summary>
        public static Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public LOFile()
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileType"></param>
        /// <param name="filecontent"></param>
        public LOFile(string fileName, string fileType, object filecontent)
        {
            FileName = fileName;
            FileType = fileType;
            SetFileContent(filecontent);
            SetDefaultFileName();
        }

        /// <summary>
        /// Alimente FileContent
        /// </summary>
        /// <param name="fileContent">donnée binaire ou donnée texte</param>
        public void SetFileContent(object fileContent)
        {
            _fileContent = null;

            if (fileContent.GetType().Equals(typeof(System.String)))
            {
                //20090918 FI 
                //Suppression de la declaration xml lorsque l'encoding vaut utf-16 car IE ne sait pas ouvrir un fichier xml qui commence avec cette déclaration
                string content = fileContent.ToString();
                if (FileTypeSpecified && Cst.TypeMIME.Text.Xml == FileType.ToLower())
                {
                    //20101115 PL Add test for ms-office 
                    if (content.IndexOf(@"<?mso-application") < 0)
                    {
                        content = content.Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
                        content = content.Replace(@"<?xml version=""1.0"" encoding=""UTF-16""?>", string.Empty);
                    }
                }
                //20080804 FI Mise en place de Encoding UTF8 à la place de ASCII
                //On suppose que la collation en base de données est équivalente à SQL_Latin1_General_CP1_CI_AS 
                //SQL_Latin1 estl'équivalent de Windows-1252 soit un sous ensemble de UTF8
                //Ceci afin de palier le pb de consultation de Confirmation ou les "é" sont remplacés par "?"
                _fileContent = Encoding.GetBytes(content);
            }
            else if (fileContent.GetType().Equals(typeof(byte[])))
            {
                _fileContent = (byte[])fileContent;
            }
            else
            {
                throw new NotSupportedException($"type : {fileContent.GetType()} is not supported");
            }
        }

        /// <summary>
        /// Alimente FileName lorsqu'il est non renseigné (valeur par défaut : tempFile)
        /// <para>Ajoute une extension à FileName si FileType est spécifié</para>
        /// </summary>
        public void SetDefaultFileName()
        {
            if (StrFunc.IsEmpty(FileName))
                FileName = "tempFile";

            if ((FileName.IndexOf(".") < 0) && (FileTypeSpecified))
            {
                string defaultExtension = "dat";

                //TODO...
                if (FileType.ToLower().StartsWith("text"))
                {
                    defaultExtension = "txt";
                    switch (FileType.ToLower())
                    {
                        case Cst.TypeMIME.Text.Html:
                            defaultExtension = "htm";
                            break;
                        case Cst.TypeMIME.Text.Xml:
                            defaultExtension = "xml";
                            break;
                        case Cst.TypeMIME.Text.RichText:
                            defaultExtension = "rtf";
                            break;
                    }
                }
                else if (FileType.ToLower().StartsWith("application"))
                {
                    defaultExtension = "dat";
                    switch (FileType.ToLower())
                    {
                        case Cst.TypeMIME.Application.Pdf:
                            defaultExtension = "pdf";
                            break;
                        case Cst.TypeMIME.Application.Postscript:
                            defaultExtension = "ps";
                            break;
                    }
                }
                FileName += "." + defaultExtension;
            }
        }
    }

    /// <summary>
    /// Classe pour manipuler un fichier stocké dans une colonne SQL
    /// </summary>
    public class LOFileColumn
    {
        private string[] _keyColumns;
        private string[] _keyValues;
        private string[] _keyDatatypes;
        private readonly string _cs;

        /// <summary>
        /// Table qui contient le contenu binaire du fichier
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Colonne qui contient le contenu binaire du fichier
        /// </summary>
        public string ColumnName { get; set; }


        public Boolean ColumnFileNameSpecified { get { return StrFunc.IsFilled(ColumnFileName); } }
        /// <summary>
        ///  Colonne qui contient le nom de fichier 
        /// </summary>
        public string ColumnFileName { get; set; }


        public Boolean ColumnFileTypeSpecified { get { return StrFunc.IsFilled(ColumnFileType); } }

        /// <summary>
        ///  Colonne qui contient le type MIME du fichier 
        /// </summary>
        public string ColumnFileType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ColumnIDAUPD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ColumnDTUPD { get; set; }

        // EG 20240207 [WI825] Logs: Harmonization data of consultation (VW_ATTACHEDDOC_TRACKER_L)
        private string TableForSelect
        {
            get
            {
                return _keyValues.Contains(Cst.OTCml_TBL.TRACKER_L.ToString()) ?
                    Cst.OTCml_TBL.VW_ATTACHEDDOC_TRACKER_L.ToString() : TableName;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableName"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDataTypes"></param>
        public LOFileColumn(string pCS, string pTableName,
            string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes)
        {
            _cs = pCS;
            if (pTableName.StartsWith(Cst.OTCml_TBL.ATTACHEDDOC.ToString()))
            {
                Initialize(pTableName, "LODOC", "DOCNAME", "DOCTYPE",
                    pKeyColumns, pKeyValues, pKeyDataTypes,
                     "IDAUPD", "DTUPD");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDataTypes"></param>
        public LOFileColumn(string pCS, string pTableName, string pColumnName,
            string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes)
            : this(pCS, pTableName, pColumnName, string.Empty, string.Empty,
            pKeyColumns, pKeyValues, pKeyDataTypes,
            string.Empty, string.Empty)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pColumnFileName"></param>
        /// <param name="pColumnFileType"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDataTypes"></param>
        public LOFileColumn(string pCS, string pTableName, string pColumnName, string pColumnFileName, string pColumnFileType,
            string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes)
            : this(pCS, pTableName, pColumnName, pColumnFileName, pColumnFileType,
            pKeyColumns, pKeyValues, pKeyDataTypes,
            string.Empty, string.Empty)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pColumnFileName"></param>
        /// <param name="pColumnFileType"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDataTypes"></param>
        /// <param name="pColumnIDAUPD"></param>
        /// <param name="pColumnDTUPD"></param>
        public LOFileColumn(string pCS, string pTableName, string pColumnName, string pColumnFileName, string pColumnFileType,
            string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes,
            string pColumnIDAUPD, string pColumnDTUPD)
        {
            _cs = pCS;
            Initialize(pTableName, pColumnName, pColumnFileName, pColumnFileType,
                pKeyColumns, pKeyValues, pKeyDataTypes,
                pColumnIDAUPD, pColumnDTUPD);
        }


        /// <summary>
        /// Charge le LOFile présente dans la table SQL
        /// </summary>
        public LOFile LoadFile()
        {
            LOFile loFile = new LOFile();

            for (int i = 0; i < _keyColumns.GetLength(0); i++)
            {
                if (_keyColumns[i] == Cst.OTCml_COL.ROWVERSION.ToString())
                    _keyDatatypes[i] = TypeData.TypeDataEnum.integer.ToString();
            }

            string SQLSelect = GetSqlSelect();

            using (IDataReader dr = DataHelper.ExecuteReader(_cs, CommandType.Text, SQLSelect))
            {
                if (dr.Read())
                {
                    if (ColumnFileTypeSpecified)
                        loFile.FileType = dr["FILETYPE"].ToString();

                    if (ColumnFileNameSpecified)
                        loFile.FileName = dr["FILENAME"].ToString();

                    //PL 20120531 Add test on DBNull
                    if (!(dr["FILECONTENT"] is DBNull))
                        loFile.SetFileContent(dr["FILECONTENT"]);

                    loFile.SetDefaultFileName();
                }
            }

            return loFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLOFile"></param>
        /// <param name="pIDA"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        public Cst.ErrLevel SaveFile(LOFile pLOFile, int pIDA, out Exception opException)
        {
            Cst.ErrLevel ret;

            if (TableName == Cst.OTCml_TBL.ATTACHEDDOC.ToString() || TableName == Cst.OTCml_TBL.ATTACHEDDOCS.ToString())
                ret = InsertATTACHEDDOC(pLOFile, pIDA, out opException);
            else
                ret = Update(pLOFile, pIDA, out opException); //on rentre ici avec ACTOR.LOGO par exemple

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel DeleteFile()
        {
            Cst.ErrLevel ret;

            if (TableName == Cst.OTCml_TBL.ATTACHEDDOC.ToString() || TableName == Cst.OTCml_TBL.ATTACHEDDOCS.ToString())
                ret = Delete();
            else
            {
                ret = Update(new LOFile(), 0, out _); //on rentre ici avec ACTOR.LOGO par exemple
            }
            return ret;
        }

        /// <summary>
        /// Retourne la requete SQL qui permet de charger le fichier 
        /// </summary>
        /// <returns></returns>
        // EG 20240207 [WI825] Logs: Harmonization data of consultation (VW_ATTACHEDDOC_TRACKER_L)
        public string GetSqlSelect()
        {
            string ret = SQLCst.SELECT + ColumnName + " as FILECONTENT";
            if (ColumnFileTypeSpecified)
                ret += ", " + ColumnFileType + " as FILETYPE";
            if (ColumnFileNameSpecified)
                ret += ", " + ColumnFileName + " as FILENAME";
            ret += Cst.CrLf;
            ret += SQLCst.FROM_DBO + TableForSelect + Cst.CrLf;
            if ((_keyColumns != null) && (_keyColumns.Length > 0) && (_keyValues != null) && (_keyValues.Length > 0))
                ret += GetWhere();

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLOFile"></param>
        /// <param name="pIDA"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        /// FI 20200820 [25468] Dates systemes en UTC (Usage de DataParameter) 
        private Cst.ErrLevel InsertATTACHEDDOC(LOFile pLOFile, int pIDA, out Exception opException)
        {
            if (null == pLOFile)
                throw new ArgumentNullException("loFile is null");

            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            opException = null;
            string _SQLQuery = SQLCst.INSERT_INTO_DBO + TableName + @"(TABLENAME, ID, DOCNAME, DOCTYPE, LODOC, DTUPD, IDAUPD) values (@TABLENAME, @ID, @DOCNAME, @DOCTYPE, @LODOC, @DTUPD , @IDAUPD)";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(_cs, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), _keyValues[0]);
            if (TableName == Cst.OTCml_TBL.ATTACHEDDOCS.ToString())
                dp.Add(new DataParameter(_cs, "ID", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), _keyValues[1]);
            else
                dp.Add(new DataParameter(_cs, "ID", DbType.Int32), Convert.ToInt32(_keyValues[1]));

            dp.Add(new DataParameter(_cs, "DOCNAME", DbType.AnsiString, SQLCst.UT_UNC_LEN), pLOFile.FileName);
            dp.Add(new DataParameter(_cs, "DOCTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pLOFile.FileType);
            dp.Add(new DataParameter(_cs, "LODOC", DbType.Binary), pLOFile.FileContent);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.IDAUPD), pIDA);
            dp.Add(DataParameter.GetParameter(_cs, DataParameter.ParameterEnum.DTUPD), OTCmlHelper.GetDateSysUTC(_cs));

            try
            {
                int retValue = DataHelper.ExecuteNonQuery(_cs, CommandType.Text, _SQLQuery, dp.GetArrayDbParameter());
                if (retValue == 1)
                    ret = Cst.ErrLevel.SUCCESS;
            }
            catch (Exception e)
            {
                opException = e;
                ret = Cst.ErrLevel.FAILURE;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Cst.ErrLevel Delete()
        {
            Cst.ErrLevel ret;
            if (_keyColumns != null && _keyColumns.Length > 0 && _keyValues != null && _keyValues.Length > 0)
            {
                string sqlDelete = SQLCst.DELETE_DBO + TableName + Cst.CrLf + GetWhere();

                int nbRow = DataHelper.ExecuteNonQuery(_cs, CommandType.Text, sqlDelete);
                if (nbRow == 1)
                    ret = Cst.ErrLevel.SUCCESS;
                else
                    ret = Cst.ErrLevel.ABORTED;
            }
            else
                ret = Cst.ErrLevel.FAILURE;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetWhere()
        {
            SQLWhere ret = new SQLWhere();
            for (int i = 0; i < _keyColumns.Length; i++)
            {
                if (StrFunc.IsFilled(_keyColumns[i]))
                {
                    string tmpWhere = @"(" + _keyColumns[i] + @"=";
                    if (_keyDatatypes[i] == TypeData.TypeDataEnum.@string.ToString())
                        tmpWhere += DataHelper.SQLString(_keyValues[i]);
                    //FI 20120608 [17867] mise en commentaire, les données dans l'URL sont transmises au format invariant, 
                    //Spheres® prend la donnée présente dans l'URL.
                    //else if (TypeData.IsTypeInt(_keyDatatypes[i]))
                    //    tmpWhere += StrFunc.DeleteGroupSeparator(_keyValues[i]);
                    else
                        tmpWhere += _keyValues[i];
                    tmpWhere += ")";
                    ret.Append(tmpWhere);
                }
            }
            return ret.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLOFile"></param>
        /// <param name="pIDA"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        private Cst.ErrLevel Update(LOFile pLOFile, int pIDA, out Exception opException)
        {
            Cst.ErrLevel ret;
            opException = null;

            string SQLUpdate = SQLCst.UPDATE_DBO + TableName + Cst.CrLf;
            SQLUpdate += SQLCst.SET + ColumnName + @" = @" + ColumnName + Cst.CrLf;
            if (StrFunc.IsFilled(ColumnFileName))
                SQLUpdate += "," + ColumnFileName + @" = " + DataHelper.SQLString(pLOFile.FileName) + Cst.CrLf;
            if (StrFunc.IsFilled(ColumnFileType))
                SQLUpdate += "," + ColumnFileType + @" = " + DataHelper.SQLString(pLOFile.FileType) + Cst.CrLf;

            if (StrFunc.IsFilled(ColumnIDAUPD))
                SQLUpdate += "," + ColumnIDAUPD + @" = " + pIDA.ToString() + Cst.CrLf;

            if (StrFunc.IsFilled(ColumnDTUPD)) // FI 20200820 [25468] Dates systemes en UTC
                SQLUpdate += "," + ColumnDTUPD + @" = " + DataHelper.SQLGetDate(_cs, true) + Cst.CrLf;

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(_cs, ColumnName, DbType.Binary));

            if (null != pLOFile.FileContent)
                dp[ColumnName].Value = pLOFile.FileContent;
            else
                dp[ColumnName].Value = DBNull.Value;

            if (ArrFunc.IsFilled(_keyColumns))
                SQLUpdate += GetWhere();

            int nbRow = DataHelper.ExecuteNonQuery(_cs, CommandType.Text, SQLUpdate, dp.GetArrayDbParameter());
            if (nbRow == 1)
                ret = Cst.ErrLevel.SUCCESS;

            else
                ret = Cst.ErrLevel.ABORTED;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableName"></param>
        /// <param name="pColumnName"></param>
        /// <param name="pColumnFileName"></param>
        /// <param name="pColumnFileType"></param>
        /// <param name="pKeyColumns"></param>
        /// <param name="pKeyValues"></param>
        /// <param name="pKeyDataTypes"></param>
        /// <param name="pColumnIDAUPD"></param>
        /// <param name="pColumnDTUPD"></param>
        private void Initialize(string pTableName, string pColumnName, string pColumnFileName, string pColumnFileType, string[] pKeyColumns, string[] pKeyValues, string[] pKeyDataTypes, string pColumnIDAUPD, string pColumnDTUPD)
        {
            TableName = pTableName;
            ColumnName = pColumnName;
            ColumnFileName = pColumnFileName;
            ColumnFileType = pColumnFileType;
            ColumnIDAUPD = pColumnIDAUPD;
            ColumnDTUPD = pColumnDTUPD;

            _keyColumns = pKeyColumns;
            _keyValues = pKeyValues;
            _keyDatatypes = pKeyDataTypes;
        }


    }


}
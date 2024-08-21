#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Status;
using EFS.Tuning;
using EfsML.Business;
using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Tz = EFS.TimeZone;

#endregion Using Directives

namespace EfsML.DynamicData
{
    /// <summary>
    /// Représente une librairie Spheres®
    /// <para>Une librairie Spheres® est généralement appelée depuis les intercaes IO</para>
    /// </summary>
    [XmlRoot(ElementName = "SpheresLib", IsNullable = true)]
    public class DataSpheresLib
    {

        #region Members
        /// <summary>
        /// Nom de la fonction
        /// </summary>
        [XmlAttribute()]
        public string function;

        /// <summary>
        /// Paramètres de la function
        /// </summary>
        [XmlElementAttribute("Param", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ParamData[] param;
        #endregion Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="opParam"></param>
        /// <returns></returns>
        public bool GetParam(string pName, out ParamData opParam)
        {
            return GetParam(pName, out opParam, out _);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="opParam"></param>
        /// <param name="opErrMsg"></param>
        /// <returns></returns>
        public bool GetParam(string pName, out ParamData opParam, out string opErrMsg)
        {
            bool ret = false;
            opParam = null;
            opErrMsg = null;
            string name = pName.ToLower();

            //
            if (ArrFunc.IsEmpty(param))
            {
                opErrMsg = "parameters are missing !";
            }
            else
            {
                for (int i = 0; i < ArrFunc.Count(param); i++)
                {
                    // RD 20101014 / Bug si le param n'a pas de nom
                    if (StrFunc.IsFilled(param[i].name) && param[i].name.ToLower() == name)
                        opParam = param[i];
                }
                //
                if (opParam == null)
                {
                    name = "p" + name;
                    for (int i = 0; i < ArrFunc.Count(param); i++)
                    {
                        // RD 20101014 / Bug si le param n'a pas de nom
                        if (StrFunc.IsFilled(param[i].name) && param[i].name.ToLower() == name)
                            opParam = param[i];
                    }
                }
                //
                ret = (opParam != null);
                if (!ret)
                    opErrMsg = "parameter " + pName + " is missing !";
            }

            return ret;
        }

        /// <summary>
        ///  Retourne l'évaluation de la fonction
        ///  <para>La fonction retourne nécessairement un résultat en String</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        /// FI 20130115 [XXXXX] Ajout de la fonction GETENUMVALUE 
        /// FI 20171103 [23039] Modify
        public string Exec(string pCS, IDbTransaction pDbTransaction)
        {
            string format = string.Empty;

            string ret = null;
            string functionName = function.ToUpper();
            ParamData paramItem;
            string errMsg;
            switch (functionName)
            {
                #region Spheres Functions for SQLUP
                case "GETNEWIDEVENT()":
                    ret = GetNewIDEvent(pCS, pDbTransaction);
                    break;
                case "GETNEWIDTRADE()":
                    //PL 20130405 New feature
                    if (GetParam("SOURCE", out paramItem, out _))
                    {
                        string source = paramItem.GetDataValue(pCS, pDbTransaction);
                        ret = GetNewIDTrade(pCS, pDbTransaction, source);
                    }
                    else
                    {
                        ret = GetNewIDTrade(pCS, pDbTransaction);
                    }
                    break;
                case "GETNEWID()":
                    if (!GetParam("IDGETID", out paramItem, out errMsg))
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    }

                    string idGetId = paramItem.GetDataValue(pCS, pDbTransaction);
                    ret = GetNewID(pCS, pDbTransaction, idGetId);
                    break;
                #endregion

                case "ISTRADECOMPATIBLE()":
                case "ISEVENTCOMPATIBLE()":
                    ret = IstradeOrEventCompatible(pCS, pDbTransaction, functionName);
                    break;

                #region Spheres Functions for Date
                case "CONVERTTOUTC()": //FI 20171103 [23039] Ajout de la méthode
                    // Convertie un horodatedate spécifique à une timeZone en horodatedate utc
                    // Formate l'horodatage en yyyy-MM-ddTHH:mm:ss.ffffffZ
                    if (!GetParam("TIMESTAMP", out paramItem, out errMsg))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    string dtTimestamp = paramItem.GetDataValue(pCS, pDbTransaction);
                    string dtFormat = paramItem.dataformat;

                    if (!GetParam("TIMEZONE", out paramItem, out errMsg))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    string timeZone = paramItem.GetDataValue(pCS, pDbTransaction);

                    TimeZoneInfo tzInfo = Tz.Tools.GetTimeZoneInfoFromTzdbId(timeZone);
                    DateTime dtUtc = TimeZoneInfo.ConvertTimeToUtc(new DtFunc().StringToDateTime(dtTimestamp, dtFormat, CultureInfo.InvariantCulture), tzInfo);

                    ret = DtFuncML.DateTimeToString(dtUtc, DtFunc.FmtTZISOLongDateTime, CultureInfo.InvariantCulture);
                    break;


                case "GETDATESYS()": // Retourne la date système dans le format spécifié. Si le format n'est pas spécifié => Retourne la date au format ISO 
                case "GETUTCDATESYS()": // FI 20200901 [25468] add

                    if (GetParam("FORMAT", out paramItem))
                        format = paramItem.GetDataValue(pCS, pDbTransaction);
                    ret = FormatDate(SystemTools.GetOSDateSys((functionName == "GETUTCDATESYS()")), format);
                    break;

                case "GETDATETIMESYS()":
                case "GETUTCDATETIMESYS()": // FI 20200901 [25468] add
                    if (GetParam("FORMAT", out paramItem))
                        format = paramItem.GetDataValue(pCS, pDbTransaction);
                    ret = FormatDatetime(SystemTools.GetOSDateSys((functionName == "GETUTCDATETIMESYS()")), format); // FI 20211029 [XXXXX] GETUTCDATETIMESYS() à la place GETUTCDATESYS()
                    break;

                case "GETDATERDBMS()":
                case "GETUTCDATERDBMS()": // FI 20200901 [25468] add
                    if (GetParam("FORMAT", out paramItem))
                        format = paramItem.GetDataValue(pCS, pDbTransaction);
                    ret = FormatDate(OTCmlHelper.GetDateSys(pCS, (functionName == "GETUTCDATERDBMS()")), format);
                    break;

                case "GETDATETIMERDBMS()":
                case "GETUTCDATETIMERDBMS()": // FI 20200901 [25468] add
                    if (GetParam("FORMAT", out paramItem))
                        format = paramItem.GetDataValue(pCS, pDbTransaction);
                    ret = FormatDatetime(OTCmlHelper.GetDateSys(pCS, (functionName == "GETUTCDATETIMERDBMS()")), format);
                    break;

                case "GETANTICIPATEDDATE()":
                    if (GetParam("FORMAT", out paramItem))
                        format = paramItem.GetDataValue(pCS, pDbTransaction);
                    //
                    if (!GetParam("DATE", out paramItem, out errMsg))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    string dt_tmp = paramItem.GetDataValue(pCS, pDbTransaction);
                    DateTime date = new DtFunc().StringDateISOToDateTime(dt_tmp);
                    if (DtFunc.IsDateTimeEmpty(date))
                    {
                        //Cas d'un datetime
                        date = new DtFunc().StringDateTimeISOToDateTime(dt_tmp);
                    }
                    ret = OTCmlHelper.GetAnticipatedDateToString(pCS, pDbTransaction, date, format);
                    break;

                case "ISDATE()":
                case "ISNOTDATE()":
                case "ISDATETIME()":
                case "ISNOTDATETIME()":
                case "ISDATESYS()":
                case "ISNOTDATESYS()":
                case "ISHOLIDAY()":
                case "ISLESSTHANDATERDBMS()":
                case "ISLESSOREQUALDATERDBMS()":
                case "ISGREATEROREQUALDATERDBMS()":
                case "ISGREATERTHANDATERDBMS()":
                    // RD 20100421 [16955]
                    if (GetParam("FORMAT", out paramItem))
                        format = paramItem.GetDataValue(pCS, pDbTransaction);
                    //
                    if (!GetParam("VALUE", out paramItem, out errMsg))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    //		
                    string checkValue = paramItem.GetDataValue(pCS, pDbTransaction);
                    //
                    DateTime dateToCheck = DateTime.MinValue;
                    DateTime dateSys = DateTime.MinValue;
                    //
                    if (("ISHOLIDAY()" == functionName) ||
                        ("ISLESSTHANDATERDBMS()" == functionName) ||
                        ("ISLESSOREQUALDATERDBMS()" == functionName) ||
                        ("ISGREATEROREQUALDATERDBMS()" == functionName) ||
                        ("ISGREATERTHANDATERDBMS()" == functionName))
                    {
                        dateToCheck = new DtFunc().StringToDateTime(checkValue, format, CultureInfo.InvariantCulture);
                        dateSys = OTCmlHelper.GetDateSys(pCS).Date;
                    }
                    //
                    if ("ISDATE()" == functionName)
                        ret = ObjFunc.FmtToISo(StrFunc.IsDate(checkValue, format, CultureInfo.InvariantCulture), TypeData.TypeDataEnum.@bool);
                    if ("ISNOTDATE()" == functionName)
                        ret = ObjFunc.FmtToISo(false == StrFunc.IsDate(checkValue, format, CultureInfo.InvariantCulture), TypeData.TypeDataEnum.@bool);
                    //					
                    else if ("ISDATETIME()" == functionName)
                        ret = ObjFunc.FmtToISo(StrFunc.IsDateTime(checkValue, format, CultureInfo.InvariantCulture), TypeData.TypeDataEnum.@bool);
                    else if ("ISNOTDATETIME()" == functionName)
                        ret = ObjFunc.FmtToISo(false == StrFunc.IsDateTime(checkValue, format, CultureInfo.InvariantCulture), TypeData.TypeDataEnum.@bool);
                    //
                    else if ("ISDATESYS()" == functionName)
                        ret = ObjFunc.FmtToISo(StrFunc.IsDateSys(checkValue, format, CultureInfo.InvariantCulture), TypeData.TypeDataEnum.@bool);
                    else if ("ISNOTDATESYS()" == functionName)
                        ret = ObjFunc.FmtToISo(false == StrFunc.IsDateSys(checkValue, format, CultureInfo.InvariantCulture), TypeData.TypeDataEnum.@bool);
                    //
                    else if ("ISHOLIDAY()" == functionName)
                    {
                        if (!GetParam("IDBC", out paramItem, out errMsg))
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                        //
                        string idBC = paramItem.GetDataValue(pCS, pDbTransaction);
                        //
                        ret = ObjFunc.FmtToISo(Tools.IsHoliday(pCS, dateToCheck, new string[] { idBC }), TypeData.TypeDataEnum.@bool);
                    }
                    //
                    else if ("ISLESSTHANDATERDBMS()" == functionName)
                        ret = ObjFunc.FmtToISo((0 > DateTime.Compare(dateToCheck, dateSys)), TypeData.TypeDataEnum.@bool);
                    else if ("ISLESSOREQUALDATERDBMS()" == functionName)
                        ret = ObjFunc.FmtToISo((0 >= DateTime.Compare(dateToCheck, dateSys)), TypeData.TypeDataEnum.@bool);
                    else if ("ISGREATEROREQUALDATERDBMS()" == functionName)
                        ret = ObjFunc.FmtToISo((0 <= DateTime.Compare(dateToCheck, dateSys)), TypeData.TypeDataEnum.@bool);
                    else if ("ISGREATERTHANDATERDBMS()" == functionName)
                        ret = ObjFunc.FmtToISo((0 < DateTime.Compare(dateToCheck, dateSys)), TypeData.TypeDataEnum.@bool);
                    break;
                //
                #endregion

                #region Spheres Functions for Filled
                case "ISNULL()":
                case "ISNOTNULL()":
                case "ISEMPTY()":
                case "ISNOTEMPTY()":
                    if (!GetParam("VALUE", out paramItem, out errMsg))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    checkValue = paramItem.GetDataValue(pCS, pDbTransaction);
                    //
                    if ("ISNULL()" == functionName)
                        ret = ObjFunc.FmtToISo(ObjFunc.IsNull(checkValue), TypeData.TypeDataEnum.@bool);
                    else if ("ISNOTNULL()" == functionName)
                        ret = ObjFunc.FmtToISo(false == ObjFunc.IsNull(checkValue), TypeData.TypeDataEnum.@bool);
                    else if ("ISEMPTY()" == functionName)
                        ret = ObjFunc.FmtToISo(StrFunc.IsEmpty(checkValue), TypeData.TypeDataEnum.@bool);
                    else if ("ISNOTEMPTY()" == functionName)
                        ret = ObjFunc.FmtToISo(false == StrFunc.IsEmpty(checkValue), TypeData.TypeDataEnum.@bool);
                    break;
                #endregion

                case "DELETEEVENTCLASS()":
                    DeleteEventClass(pCS, pDbTransaction, functionName);
                    break;

                case "ADDROWACCOUNTINGEVENTCLASS()":
                    AddRowAccountingEventClass(pCS, pDbTransaction, functionName);
                    break;

                case "GETNEWGUID()":
                    ret = SystemTools.GetNewGUID();
                    break;

                case "GETPARTYXMLID()":
                    ret = GetPartyXmlId(pCS, pDbTransaction, functionName);
                    break;

                case "GETENUMVALUE()":
                    ret = GetEnumValue(pCS, pDbTransaction, functionName);
                    break;

                case "CONVERTTOLONGFROMBASE64STRING()": // FI 20240229 [WI860] Add case 
                    if (!GetParam("BASE64STRING", out paramItem, out errMsg))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                    string base64String = paramItem.GetDataValue(pCS, pDbTransaction);
                    ret = IntFunc.ConvertFromBase64String(base64String).ToString();
                    break;
            }

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private string IstradeOrEventCompatible(string pCS, IDbTransaction pDbTransaction, string functionName)
        {
            bool isTrade = ("ISTRADECOMPATIBLE()" == functionName);

            if (ArrFunc.IsEmpty(param))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "parameters are not defined");

            if (!GetParam("PROCESSTYPE", out ParamData paramItem, out string errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            Cst.ProcessTypeEnum processType = (Cst.ProcessTypeEnum)System.Enum.Parse(typeof(Cst.ProcessTypeEnum), paramItem.GetDataValue(pCS, pDbTransaction), true);

            if (!GetParam("IDI", out paramItem, out errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            int idI = Convert.ToInt32(paramItem.GetDataValue(pCS, pDbTransaction));
            //
            int idT = 0;
            int idE = 0;
            if (isTrade)
            {
                if (!GetParam("IDT", out paramItem, out errMsg))
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                idT = Convert.ToInt32(paramItem.GetDataValue(pCS, pDbTransaction));
            }
            else
            {
                if (!GetParam("IDE", out paramItem, out errMsg))
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
                idE = Convert.ToInt32(paramItem.GetDataValue(pCS, pDbTransaction));
            }
            //
            string appName = string.Empty;
            if (GetParam("APPNAME", out paramItem))
                appName = paramItem.GetDataValue(pCS, pDbTransaction);
            //
            string hostName = string.Empty;
            if (GetParam("HOSTNAME", out paramItem))
                hostName = paramItem.GetDataValue(pCS, pDbTransaction);

            Cst.ErrLevel errlevel = Cst.ErrLevel.SUCCESS;

            ProcessTuning processTuning = new ProcessTuning(pCS, idI, processType, appName, hostName);
            if (processTuning.DrSpecified)
            {
                if (isTrade)
                {
                    TradeStatus tradeStatus = new TradeStatus();
                    tradeStatus.Initialize(pCS, idT);
                    errlevel = processTuning.ScanTradeCompatibility(pCS, idT, tradeStatus, out _);
                }
                else
                {
                    EventStatus eventStatus = new EventStatus();
                    eventStatus.Initialize(pCS, pDbTransaction, idE);
                    errlevel = processTuning.ScanEventCompatibility(pCS, idE, eventStatus, out _);
                }
            }

            string ret = ObjFunc.FmtToISo(Cst.ErrLevel.SUCCESS == errlevel, TypeData.TypeDataEnum.@bool);

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="functionName"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        private void DeleteEventClass(string pCS, IDbTransaction pDbTransaction, string functionName)
        {
            if (!GetParam("IDE", out ParamData paramItem, out string errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);

            int idE = Convert.ToInt32(paramItem.GetDataValue(pCS, pDbTransaction));

            DataParameters sqlParam = new DataParameters();
            sqlParam.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDE), idE);

            StrBuilder sqlDelete = new StrBuilder();
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + Cst.CrLf;
            sqlDelete += SQLCst.WHERE + "IDE=@IDE" + Cst.CrLf;
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlDelete.ToString(), sqlParam.GetArrayDbParameter());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="functionName"></param>
        // EG 20180205 [23769] Add dbTransaction  
        private void AddRowAccountingEventClass(string pCS, IDbTransaction pDbTransaction, string functionName)
        {
            if (!GetParam("IDE", out ParamData paramItem, out string errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            int idE = Convert.ToInt32(paramItem.GetDataValue(pCS, pDbTransaction));

            if (!GetParam("IDI", out paramItem, out errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            int idI = Convert.ToInt32(paramItem.GetDataValue(pCS, pDbTransaction));

            if (!GetParam("DATE", out  paramItem, out errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            DateTime dt = new DtFunc().StringToDateTime(paramItem.GetDataValue(pCS, pDbTransaction), paramItem.dataformat);
            EFS_AccountingEventClass.AddRowAccountingEventClass(pCS, pDbTransaction, idE, idI, dt, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private string GetPartyXmlId(string pCS, IDbTransaction pDbTransaction, string functionName)
        {
            string ret = null;

            if (!GetParam("ACTOR_IDENTIFIER", out ParamData paramItem, out string errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);

            string actorIdentifier = paramItem.GetDataValue(pCS, pDbTransaction);
            
            if (StrFunc.IsFilled(actorIdentifier))
            {
                SQL_Actor sql_actor = new SQL_Actor(pCS, actorIdentifier);
                if (sql_actor.LoadTable(new string[] { "IDA", "IDENTIFIER", "BIC" }))
                    ret = sql_actor.XmlId;
            }
            return ret;
        }

        /// <summary>
        /// Retourne la valeur d'un enum à partir d'une donnée en entrée 
        /// <para>Cette méthode n'effectue pas nécessairement un select dans la base de donnée</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetEnumValue(string pCS, IDbTransaction pDbTransaction, string functionName)
        {
            string ret = null;

            if (!GetParam("CODE", out ParamData paramItem, out string errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            string code = paramItem.GetDataValue(pCS, pDbTransaction);

            if (!GetParam("DATA", out paramItem, out errMsg))
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, functionName + ": " + errMsg);
            string data = paramItem.GetDataValue(pCS, pDbTransaction);

            //Recherche en priorité dans la liste des enums présents en mémoire
            //Spheres recherche l'enum dont EXTVALUE vaut la donnée en entrée
            //Si non trouvé 
            //Spheres recherche l'enum dont VALUE vaut la donnée en entrée
            //Si non trouvé 
            //Spheres recherche l'enum dont EXTLLINK vaut la donnée en entrée
            //Si non trouvé 
            //Spheres execute en requête plus complexes pour chercher à identifier l'enum qui correspond à la donnée en entrée
            //FI 20120211[]  pour améliorer les performances =>  ajout des tests sur EXTLLINK
            
            // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
            // ExtendEnum extendEnum = ExtendEnumsTools.ListEnumsSchemes[code];
            ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, code);
            if (null != extendEnum)
            {
                ExtendEnumValue extendEnumValue = extendEnum[data];
                if (null == extendEnumValue)
                    extendEnumValue = extendEnum.GetExtendEnumValueByValue(data);

                if (null == extendEnumValue)
                    extendEnumValue = extendEnum.GetExtendEnumValueByExtlLink(data);

                if (null != extendEnumValue)
                    ret = extendEnumValue.Value;
            }
            

            if (StrFunc.IsEmpty(ret))
            {
                //FI Ce select était présent dans LSD_TradeImport_Map.xsl
                //Il est retranscrit ici
                string sql = @"
                select
                case count(e.VALUE) when 0 then @DATA else min(e.VALUE) end
                ||
                case count(e.VALUE) when 0 then '!NotFound' when 1 then '' else '!NotUnique' end as RETDATA
                from dbo.ENUM e
                where e.CODE = @CODE
                and ( e.EXTVALUE = @DATA or e.VALUE = @DATA or e.EXTLLINK = @DATA or 
	                    e.EXTLATTRB = @DATA or e.EXTLLINK2 = @DATA)
                order by
                case min(e.EXTVALUE) when @DATA then 1
                                     else case min(e.VALUE) when @DATA then 2
                                          else case min(e.EXTLATTRB) when @DATA then 3
                                               else case min(e.EXTLLINK) when @DATA then 4
                                                    else 5
                                                    end
                                               end
                                          end
                                     end
                ";

                DataParameters dp = new DataParameters();
                dp.Add(new DataParameter(pCS, "CODE", DbType.String), code);
                dp.Add(new DataParameter(pCS, "DATA", DbType.String), data);

                QueryParameters queryParameters = new QueryParameters(pCS, sql, dp);

                using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                {
                    if (dr.Read())
                        ret = Convert.ToString(dr["RETDATA"]);
                }
            }
            return ret;
        }

        #region private GetNewID
        private static string GetNewIDEvent(string pCs, IDbTransaction pDbTransaction)
        {
            return GetNewID(pCs, pDbTransaction, SQLUP.IdGetId.EVENT);
        }
        private static string GetNewIDTrade(string pCs, IDbTransaction pDbTransaction)
        {
            return GetNewID(pCs, pDbTransaction, SQLUP.IdGetId.TRADE);
        }
        private static string GetNewIDTrade(string pCs, IDbTransaction pDbTransaction, string pSource)
        {
            //NB: La source est de type Instr=<InstrumentIdentifier>;Book=<IDB>;TrdDt=<TradeDateISO>;[BizDt=<BusinessDateISO>;]
            //    ex. Src=TRADEID;Instr=ExchangeTradedFuture;Book=IDB_PAY;TrdDt=DTBUSINESS;

            SQL_Instrument sqlInstrument = null;
            int idAEntity = 0;
            DateTime tradeDate = DateTime.MinValue;
            DateTime businessDate = DateTime.MinValue;

            string[] srcs = pSource.Split(";".ToCharArray());
            foreach (string src in srcs)
            {
                if (src.ToUpper().StartsWith("INSTR"))
                {
                    string instrument = src.Remove(0, "INSTR".Length + 1);
                    sqlInstrument = new SQL_Instrument(pCs, instrument);
                }
                else if (src.ToUpper().StartsWith("BOOK"))
                {
                    #region Entity
                    string idb = src.Remove(0, "BOOK".Length + 1);
                    if (IntFunc.IsPositiveInteger(idb))
                    {
                        SQL_Book sqlBook = new SQL_Book(pCs, Convert.ToInt32(idb));
                        if (sqlBook.IsLoaded)
                        {
                            idAEntity = sqlBook.IdA_Entity;
                        }
                    }
                    #endregion
                }
                else if (src.ToUpper().StartsWith("TRDDT"))
                {
                    #region Trade date
                    string date_ISO = src.Remove(0, "TRDDT".Length + 1);
                    if (DtFunc.IsParsableValue(date_ISO))
                    {
                        tradeDate = DtFunc.ParseDate(date_ISO, DtFunc.FmtISODate, null);
                    }
                    #endregion
                }
                else if (src.ToUpper().StartsWith("BIZDT"))
                {
                    #region Business date
                    string date_ISO = src.Remove(0, "BIZDT".Length + 1);
                    if (DtFunc.IsParsableValue(date_ISO))
                    {
                        businessDate = DtFunc.ParseDate(date_ISO, DtFunc.FmtISODate, null);
                    }
                    #endregion
                }
            }
            if (DtFunc.IsDateTimeEmpty(businessDate))
            {
                businessDate = tradeDate;
            }
            else if (DtFunc.IsDateTimeEmpty(tradeDate))
            {
                tradeDate = businessDate;
            }

            TradeRDBMSTools.BuildTradeIdentifier(pCs, pDbTransaction,
                                sqlInstrument, idAEntity, null, tradeDate, businessDate,
                                out string newIdentifier, out string prefix, out string suffix);
            return prefix + newIdentifier + suffix;
        }
        private static string GetNewID(string pCs, IDbTransaction pDbTransaction, string pClass)
        {
            SQLUP.IdGetId id = (SQLUP.IdGetId)System.Enum.Parse(typeof(SQLUP.IdGetId), pClass, true);
            return GetNewID(pCs, pDbTransaction, id);
        }
        private static string GetNewID(string pCs, IDbTransaction pDbTransaction, SQLUP.IdGetId pClass)
        {
            int newId;
            if (null != pDbTransaction)
            {
                SQLUP.GetId(out newId, pDbTransaction, pClass);
            }
            else
            {
                SQLUP.GetId(out newId, pCs, pClass);
            }			
            return newId.ToString();
        }
        #endregion


        /// <summary>
        /// Formate une date 
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <param name="pFormat">si null, yyyy-MM-dd(Z)</param>
        /// <returns></returns>
        // FI 20200901 [25468] add Method
        private static string FormatDate(DateTime pDateTime, string pFormat)
        {
            string ret;
            if (StrFunc.IsFilled(pFormat))
                ret = DtFunc.DateTimeToString(pDateTime, pFormat);
            else
                ret = DtFunc.DateTimeToString(pDateTime, (pDateTime.Kind == DateTimeKind.Utc) ? DtFunc.FmtISODate + "Z" : DtFunc.FmtISODate, CultureInfo.InvariantCulture);
            return ret;
        }

        /// <summary>
        /// Formate un horodatage  
        /// </summary>
        /// <param name="pDateTime"></param>
        /// <param name="pFormat">si null, yyyy-MM-ddTHH:mm:ss(Z)</param>
        /// <returns></returns>
        // FI 20200901 [25468] add Method
        private static string FormatDatetime(DateTime pDateTime, string pFormat)
        {
            string ret;
            if (StrFunc.IsFilled(pFormat))
                ret = DtFunc.DateTimeToString(pDateTime, pFormat);
            else
                ret = DtFunc.DateTimeToString(pDateTime, (pDateTime.Kind == DateTimeKind.Utc) ? DtFunc.FmtISODateTime2 + "Z" : DtFunc.FmtISODateTime2, CultureInfo.InvariantCulture);
            return ret;
        }
    }
}

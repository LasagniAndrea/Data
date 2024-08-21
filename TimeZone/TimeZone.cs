using EFS.ACommon;
using NodaTime;
using NodaTime.TimeZones;
using NodaTime.TimeZones.Cldr;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NodaTimeForText = NodaTime.Text;

namespace EFS.TimeZone
{
    /// EG 20170922 [22374] Upd
    public sealed class CustomPatterns
    {
        public string microsecondPatternSuffix = "$1.ffffff";
        public string instantPatternSuffix = "+00:00";
        public string offsetPatternSuffix = "o<m>";
        public string zonedPatternSuffix = "'('z')'";
        public string patternISOBase = "uuuu'-'MM'-'dd'T'HH':'mm':'ss.ffffff";
        public string patternISODate = "uuuu'-'MM'-'dd'";
        public string patternISOTime = "HH':'mm':'ss.ffffff";

        public NodaTimeForText.InstantPattern iso8601UTCPattern;
        public NodaTimeForText.InstantPattern instantPattern;
        public NodaTimeForText.OffsetDateTimePattern offsetDatetimePattern;
        public NodaTimeForText.ZonedDateTimePattern zonedDateTimePattern;

        public CustomPatterns()
        {
            SetCulturePatterns(CultureInfo.CurrentCulture);
        }
        /// EG 20170922 [22374] Upd
        public void SetCulturePatterns(CultureInfo pCultureInfo)
        {
            iso8601UTCPattern = NodaTimeForText.InstantPattern.CreateWithInvariantCulture(patternISOBase + "'Z'");

            if (pCultureInfo.LCID.Equals(CultureInfo.InvariantCulture.LCID))
            {
                instantPattern = NodaTimeForText.InstantPattern.CreateWithInvariantCulture(patternISOBase + "'Z'");
                offsetDatetimePattern = NodaTimeForText.OffsetDateTimePattern.CreateWithInvariantCulture(patternISOBase + offsetPatternSuffix);
                zonedDateTimePattern = NodaTimeForText.ZonedDateTimePattern.CreateWithInvariantCulture(patternISOBase + offsetPatternSuffix + " " + zonedPatternSuffix, null);
            }
            else
            {
                DateTimeFormatInfo dfi = pCultureInfo.DateTimeFormat as DateTimeFormatInfo;
                string patternTextBase = dfi.ShortDatePattern + " " + Regex.Replace(dfi.LongTimePattern, "(:ss|:s)", microsecondPatternSuffix);
                instantPattern = NodaTimeForText.InstantPattern.CreateWithCurrentCulture(patternTextBase + instantPatternSuffix);
                offsetDatetimePattern = NodaTimeForText.OffsetDateTimePattern.CreateWithCurrentCulture(patternTextBase + offsetPatternSuffix);
                zonedDateTimePattern = NodaTimeForText.ZonedDateTimePattern.CreateWithCurrentCulture(patternTextBase + offsetPatternSuffix + " " + zonedPatternSuffix, null);
            }
        }
    }

    /// EG 20170922 [22374] Upd
    public sealed class ZonedDateTimeExtended
    {
        #region Members
        private string tzdbId;
        private DateTimeZone dateTimeZone;
        private readonly DateTimeFormatInfo dfi;
        private readonly NodaTimeForText.OffsetDateTimePattern offsetDateTimePattern;
        private readonly CustomPatterns patterns;
        // EG 20240531 [WI926] The date is RESET (= empty)
        private bool dateIsReset;
        private ZonedDateTime dtZoned;
        #endregion Members
        #region accessors
        public bool IsDateFilled
        {
            get {return Tools.IsDateFilled(dtZoned.LocalDateTime);}
        }
        public bool IsDateEmpty
        {
            get {return Tools.IsDateEmpty(dtZoned.LocalDateTime);}
        }
        public string TzDbId
        {
            get { return tzdbId; }
            set { tzdbId = value; }
        }
        public ZonedDateTime DtZoned
        {
            get { return dtZoned; }
            set { dtZoned = value; }
        }
        public string LocaleDateTime
        {
            get
            {
                DateTimeZone _zone = DateTimeZoneProviders.Tzdb[Tools.WindowsIdToTzdbId(TimeZoneInfo.Local.Id)];
                ZonedDateTime dt = new ZonedDateTime(dtZoned.ToInstant(), _zone);
                return dt.ToString(patterns.zonedDateTimePattern.PatternText, null);
            }
        }
        // EG 20171004 [22374][23452] New
        public string OffsetDateTime
        {
            get 
            {
                return dtZoned.ToOffsetDateTime().ToString(patterns.offsetDatetimePattern.PatternText, null);
            }
        }
        public string UniversalDateTime
        {
            get 
            {
                return dtZoned.ToInstant().ToString(patterns.instantPattern.PatternText, null);
            }
        }
        // EG 20171004 [22374][23452] New
        public string ZoneId
        {
            get
            {
                return dateTimeZone.Id;
            }
        }
        public string ISO8601DateTime
        {
            get
            {
                return dtZoned.ToInstant().ToString(patterns.iso8601UTCPattern.PatternText, null);
            }
        }
        // EG 20171031 [23509] Upd
        public string Parse
        {
            set 
            {
                Nullable<OffsetDateTime> odt = Tools.ToOffsetDateTime(value);
                if (odt.HasValue)
                    dtZoned = Tools.ToZonedDateTime(odt.Value, dateTimeZone); 
            }
        }
        // EG 20240531 [WI926] New : the date is RESET (= empty)
        public bool DateIsReset
        {
            get { return dateIsReset; }
            set { dateIsReset = value; }
        }
        #endregion accessors
        #region Constructors
        public ZonedDateTimeExtended()
            : this(Tools.WindowsIdToTzdbId(TimeZoneInfo.Local.Id))
        {
        }
        /// EG 20170922 [22374] Upd
        public ZonedDateTimeExtended(string pTzdbId)
        {
            // Extension du longTimePattern à la microseconde
            dfi = CultureInfo.CurrentCulture.DateTimeFormat.Clone() as DateTimeFormatInfo;
            dfi.LongTimePattern = Regex.Replace(dfi.LongTimePattern, "(:ss|:s)",  Tools.cPatterns.microsecondPatternSuffix);

            // Pattern par défaut d'une date OffsetDateTime
            string patternTextBase =  dfi.ShortDatePattern + " " + dfi.LongTimePattern;
            _ = NodaTimeForText.InstantPattern.CreateWithCurrentCulture(patternTextBase + Tools.cPatterns.instantPatternSuffix);
            string patternText = patternTextBase + Tools.cPatterns.offsetPatternSuffix;
            offsetDateTimePattern = NodaTimeForText.OffsetDateTimePattern.CreateWithCurrentCulture(patternText);
            _ = NodaTimeForText.ZonedDateTimePattern.CreateWithCurrentCulture(patternText + " " + Tools.cPatterns.zonedPatternSuffix, null);

            patterns = new CustomPatterns();
            patterns.SetCulturePatterns(CultureInfo.CurrentCulture);

            SetZone(pTzdbId);

        }
        #endregion Constructors
        #region Methods
        #region ChangeZone
        public void ChangeZone(string pTzdbId)
        {
            SetZone(pTzdbId);
            Offset offset = dateTimeZone.GetUtcOffset(dtZoned.ToInstant());
            OffsetDateTime odt = dtZoned.LocalDateTime.WithOffset(offset);
            dtZoned = Tools.ToZonedDateTime(odt, dateTimeZone);
        }
        #endregion ChangeZone
        #region FromDateTime
        public void FromDateTime(DateTime pValue)
        {
            DateTimeOffset dto = new DateTimeOffset(pValue);
            DtZoned = ZonedDateTime.FromDateTimeOffset(dto);
        }
        #endregion FromDateTime
        #region SetZone
        public void SetZone(string pTzdbId)
        {
            // Time zone Id (IANA)
            tzdbId = pTzdbId;
            // Time zone Id (Windows)
            _ = Tools.TzdbIdToWindowsId(tzdbId);

            dateTimeZone = DateTimeZoneProviders.Tzdb[pTzdbId];
            _ = Tools.GetTimeZoneInfoFromTzdbId(tzdbId);
        }
        #endregion SetZone
        #region ToString
        /// <summary>
        /// Retourne l'offsetDateTime de la ZonedDateTime en String 
        /// Dans la culture courante
        /// Par défaut : ShortDatePattern + " " + LongTimePattern + "m"
        /// </summary>
        public override string ToString()
        {
            return ToString(offsetDateTimePattern.PatternText, CultureInfo.CurrentCulture);
        }
        public string ToString(string pPattern, IFormatProvider pFormatProvider)
        {
            return dtZoned.ToString(pPattern, pFormatProvider);
        }
        #endregion ToString
        #region DateToString
        /// <summary>
        /// Retourne la date de la ZonedDateTime en String 
        /// Dans la culture courante
        /// Par défaut : ShortDatePattern (dd/MM/yyyy)
        /// </summary>
        public string DateToString()
        {
            return DateToString(dfi.ShortDatePattern, CultureInfo.CurrentCulture);
        }
        public string DateToString(string pPattern, IFormatProvider pFormatProvider)
        {
            return dtZoned.Date.ToString(pPattern, pFormatProvider);
        }
        #endregion DateToString
        #region TimeToString
        /// <summary>
        /// Retourne l'heure de la ZonedDateTime en String 
        /// Dans la culture courante
        /// Par défaut : HH:mm:ss.ffffff
        /// </summary>
        public string TimeToString()
        {
            return TimeToString(dfi.LongTimePattern, CultureInfo.CurrentCulture);
        }
        public string TimeToString(string pPattern, IFormatProvider pFormatProvider)
        {
            return dtZoned.TimeOfDay.ToString(pPattern, pFormatProvider);
        }
        #endregion TimeToString
        #region TimeOffsetToString
        /// <summary>
        /// Retourne l'heure et l'offset de la ZonedDateTime en String 
        /// Dans la culture courante
        /// Par défaut : HH:mm:ss.ffffffm
        /// </summary>
        public string TimeOffsetToString()
        {
            return TimeOffsetToString(dfi.LongTimePattern, "m", CultureInfo.CurrentCulture);
        }
        public string TimeOffsetToString(string pTimePattern, string pOffsetPattern, IFormatProvider pFormatProvider)
        {
            return TimeToString(pTimePattern, pFormatProvider) + OffsetToString(pOffsetPattern, pFormatProvider);
        }
        #endregion TimeOffsetToString
        #region OffsetToString
        /// <summary>
        /// Retourne l'offset de la ZonedDateTime en String 
        /// Dans la culture courante
        /// Par défaut : m (+02:00)
        /// </summary>
        public string OffsetToString()
        {
            return OffsetToString("m", CultureInfo.CurrentCulture);
        }
        public string OffsetToString(string pPattern, IFormatProvider pFormatProvider)
        {
            return dtZoned.Offset.ToString(pPattern, pFormatProvider);
        }
        #endregion OffsetToString
        #endregion Methods
    }
    // EG 20170922 [22374] Upd
    // EG 20170929 [22374][23450] Add ALLTERRITORY, Del ActiveMapZones
    // EG 20171025 [23509] Add EmptyTime
    public sealed class Tools
    {
        #region Constants
        public static string AllWindowsID = "ALLWZID";
        public static string AllTerritories = "ALLTERRITORY";
        public static string EmptyTime = "00:00:00.000000";
        #endregion Constants

        #region Members
        private static string m_PickerTimeZoneList;
        public static CustomPatterns cPatterns = new CustomPatterns();
        private static string m_LocaleTimeZone;
        private static string m_UTCTimeZone;
        #endregion Members

        #region Accessors
        #region LocaleTimeZone
        public static string LocaleTimeZone
        {
            get
            {
                if (StrFunc.IsEmpty(m_LocaleTimeZone))
                    m_LocaleTimeZone = WindowsIdToTzdbId(TimeZoneInfo.Local.Id);
                return m_LocaleTimeZone;
            }
        }
        #endregion LocaleTimeZone
        #region UniversalTimeZone
        public static string UniversalTimeZone
        {
            get
            {
                if (StrFunc.IsEmpty(m_UTCTimeZone))
                    m_UTCTimeZone = WindowsIdToTzdbId(TimeZoneInfo.Utc.Id);
                return m_UTCTimeZone;
            }
        }
        #endregion UniversalTimeZone

        #region TimezonePickerList
        /// <summary>
        /// Construit la liste des Timezones pour TimePicker
        /// </summary>
        /// EG 20170929 [22374][23450] Upd
        public static string TimezonePickerList
        {
            get
            {
                if (StrFunc.IsEmpty(m_PickerTimeZoneList))
                {
                    List<MapZone> mapZones = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.ToList();
                    List<String> tzdbIds = ListTzdbIdByMapZone(mapZones);
                    tzdbIds.Sort();
                    DateTime dtNow = DateTime.Now;
                    tzdbIds.ForEach(tzdbId =>
                    {
                        TimeZoneInfo tzInfo = GetTimeZoneInfoFromTzdbId(tzdbId);
                        m_PickerTimeZoneList += "{ label:'" + tzdbId + "'," + "value:" + tzInfo.GetUtcOffset(dtNow).TotalMinutes.ToString() + "},";
                    });
                }
                return m_PickerTimeZoneList;
            }
        }
        #endregion TimezonePickerList
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Convertie {pDateValue} en DateTimeOffset et retourne le résulat au format HH:mm:ss.ffffff zzz
        /// </summary>
        /// EG 20170918 [22374] New
        public static string GetTimeOffsetString(string pDateValue)
        {
            return GetDateTimeOffsetString(pDateValue, DtFunc.FmtLongTime);
        }
        /// <summary>
        /// Convertie {pDateValue} en DateTimeOffset et retourne le résulat au format {pFormat}
        /// </summary>
        /// EG 20170918 [22374] New
        public static string GetDateTimeOffsetString(string pDateValue)
        {
            return GetDateTimeOffsetString(pDateValue, DtFunc.FmtDateLongTime);
        }
        public static string GetDateTimeOffsetString(string pDateValue, string pPattern)
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(pDateValue))
            {
                if (false == DateTimeOffset.TryParse(pDateValue, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dt))
                    dt = StringDateTimeToDateTimeOffset(pDateValue, pPattern);
                ret = dt.ToString(pPattern, DtFunc.DateTimeOffsetPattern);
            }
            return ret;
        }

        public static DateTimeOffset StringDateTimeToDateTimeOffset(string pValue, string pPattern)
        {
            DateTime dt;
            if (StrFunc.IsFilled(pPattern))
                dt = new DtFunc().StringToDateTime(pValue, pPattern);
            else
                dt = new DtFunc().StringToDateTime(pValue, null, null, true);
            return new DateTimeOffset(dt);
        }
        public static Instant StringDateTimeToInstant(string pValue, string pPattern)
        {
            DateTime dt;
            if (StrFunc.IsFilled(pPattern))
                dt = new DtFunc().StringToDateTime(pValue, pPattern);
            else
                dt = new DtFunc().StringToDateTime(pValue, null, null, true);
            ;
            return Instant.FromDateTimeUtc(DateTime.SpecifyKind(dt, DateTimeKind.Utc)); 
        }

        /// <summary>
        /// Ajoute des heures, minutes, secondes, millisecondes et microsecondes à une une date
        /// </summary>
        /// <param name="pDate">date initiale</param>
        /// <param name="pTime">date qui contient les heures, minutes, secondes, millisecondes et microsecondes</param>
        public static DateTimeOffset AddTimeToDate(DateTime pDate, DateTimeOffset pTime)
        {
            return AddTimeToDate(pDate, pTime.DateTime);
        }
        // EG 20171004 [22374][23452] Upd
        public static DateTimeOffset AddTimeToDate(DateTime pDate, DateTime pTime)
        {
            DateTimeOffset dt = new DateTimeOffset(pDate.Date);
            if (DtFunc.IsDateTimeFilled(pDate) && DtFunc.IsDateTimeFilled(pTime))
                dt = dt.AddTicks(pTime.TimeOfDay.Ticks);
            return dt;
        }
        public static DateTimeOffset AddTimeToDate(string pDate, string pTime)
        {
            Instant _instantDate = StringDateTimeToInstant(pDate, DtFunc.FmtISODate);
            Instant _instantTime = StringDateTimeToInstant(pTime, null);

            _instantDate = _instantDate.PlusTicks(_instantTime.ToDateTimeUtc().TimeOfDay.Ticks);
            DateTimeOffset dt = _instantDate.ToDateTimeOffset();
            return dt;
        }
        // EG 20171004 [22374][23452] New
        public static string AddTimeToDateReturnString(DateTime pDate, DateTime pTime)
        {
            DateTimeOffset dt = AddTimeToDate(pDate, pTime);
            return Tools.ToString(dt);
        }
        // EG 20171025 [23509] New
        public static string AddTimeToDateReturnString(string pDate)
        {
            return AddTimeToDateReturnString(pDate, Tools.EmptyTime);
        }
        public static string AddTimeToDateReturnString(string pDate, string pTime)
        {
            DateTimeOffset dt = AddTimeToDate(pDate, pTime);
            return Tools.ToString(dt);
        }
        // EG 20171031 [23509] Upd
        // EG 20171115 [23509] Upd
        public static bool IsDateFilled(string pValue)
        {
            Nullable<OffsetDateTime> odt = ToOffsetDateTime(pValue);
            return odt.HasValue && IsDateFilled(odt.Value.LocalDateTime);
        }
        // EG 20171031 [23509] Upd
        // EG 20171115 [23509] Upd
        public static bool IsDateEmpty(string pValue)
        {
            Nullable<OffsetDateTime> odt = ToOffsetDateTime(pValue);
            return (false == odt.HasValue) || IsDateEmpty(odt.Value.LocalDateTime);
        }
        internal static bool IsDateFilled(LocalDateTime pLocalDateTime)
        {
            return (false == IsDateEmpty(pLocalDateTime));
        }
        internal static bool IsDateEmpty(LocalDateTime pLocalDateTime)
        {
            LocalDateTime dt = LocalDateTime.FromDateTime(DateTime.MinValue);
            return (pLocalDateTime.Date == dt.Date);
        }

        #region GetTimeZoneInfoFromTzdbId
        /// <summary>
        /// Retourne le TimeZoneInfo à partir d'un tzdbId
        /// </summary>
        public static TimeZoneInfo GetTimeZoneInfoFromTzdbId(string pTzdbId)
        {
            string windowsId = TzdbIdToWindowsId(pTzdbId);
            return (null == windowsId) ? null : TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        }
        #endregion GetTimeZoneInfoFromTzdbId
        #region ListMapZonesByTerritory
        /// <summary>
        /// Retourne la liste des MapZones pour un country donné
        /// </summary>
        public static List<MapZone> ListMapZonesByTerritory(string pTerritory)
        {
            return ListMapZonesByTerritory(pTerritory, string.Empty);
        }
        /// <summary>
        /// Retourne la liste des MapZones pour un country donné
        /// </summary>
        /// <param name="pCountry">Country (Territory)</param>
        /// <param name="pWindowsId">WindowsId </param>
        /// <returns></returns>
        public static List<MapZone> ListMapZonesByTerritory(string pTerritory, string pWindowsId)
        {
            IList<MapZone> defaultMapZones = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
            List<MapZone> mapZones = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.ToList();

            if (Tools.AllTerritories != pTerritory)
                mapZones = mapZones.Where(mapZone => mapZone.Territory.Equals(pTerritory, StringComparison.OrdinalIgnoreCase)).ToList();

            if (StrFunc.IsFilled(pWindowsId) && (Tools.AllWindowsID != pWindowsId))
                    mapZones = mapZones.Where(mapZone => mapZone.WindowsId.Equals(pWindowsId, StringComparison.OrdinalIgnoreCase)).ToList();

            if (null == mapZones)
                throw new InvalidTimeZoneException(String.Format("no MapZone for territory {0}.", pTerritory));

            return mapZones;
        }
        #endregion ListMapZonesByTerritory
        #region ListMapZonesByWindowsId
        /// <summary>
        /// Retourne la liste des MapZones pour un WindowsId donné
        /// </summary>
        public static List<MapZone> ListMapZonesByWindowsId(string pWindowsId)
        {
            IList<MapZone> defaultMapZones = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
            List<MapZone> mapZones = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones.ToList();
            if (Tools.AllWindowsID != pWindowsId)
                mapZones = defaultMapZones.Where(mapZone => mapZone.WindowsId.Equals(pWindowsId, StringComparison.OrdinalIgnoreCase)).ToList();

            if (null == mapZones)
                throw new InvalidTimeZoneException(String.Format("no MapZone for windowsId {0}.", pWindowsId));

            return mapZones;
        }
        #endregion ListMapZonesByWindowsId

        #region ListTerritoryByMapZone
        /// <summary>
        /// Retourne la liste des territoires pour plusieurs MapZones
        /// </summary>
        public static List<string> ListTerritoryByMapZone(List<MapZone> pMapZones)
        {
            List<string> territories = new List<string>();
            pMapZones.ForEach(mapZone =>
            {
                if (false == territories.Contains(mapZone.Territory))
                    territories.Add(mapZone.Territory);
            });
            territories.Sort();
            return territories;
        }
        #endregion ListTerritoryByMapZone
        #region ListTerritoryByWindowsId
        /// <summary>
        /// Retourne la liste des territoires pour plusieurs MapZones
        /// </summary>
        public static List<string> ListTerritoryByWindowsId()
        {
            return ListTerritoryByWindowsId(Tools.AllWindowsID);
        }
        public static List<string> ListTerritoryByWindowsId(string pWindowsId)
        {
            List<MapZone> mapZones = Tools.ListMapZonesByWindowsId(pWindowsId);
            return ListTerritoryByMapZone(mapZones);
        }
        #endregion ListTerritoryByWindowsId

        #region ListTzdbIdByMapZone
        /// <summary>
        /// Retourne la liste des TimeZones pour plusieurs MapZones
        /// </summary>
        public static List<string> ListTzdbIdByMapZone(List<MapZone> pMapZones)
        {
            List<string> tzdbIds = new List<string>();
            pMapZones.ForEach(mapZone => 
            {
                tzdbIds = Enumerable.Union(tzdbIds, ListTzdbIdByMapZone(mapZone)).ToList();
            });
            tzdbIds.Sort();
            return tzdbIds;
        }
        /// <summary>
        /// Retourne la liste des TimeZones pour un MapZone
        /// </summary>
        public static List<string> ListTzdbIdByMapZone(MapZone pMapZone)
        {
            List<string> tzdbIds = new List<string>();
            pMapZone.TzdbIds.ToList().ForEach(tzdbId =>
            {
                if (false == tzdbIds.Contains(tzdbId))
                    tzdbIds.Add(tzdbId);
            });
            tzdbIds.Sort();
            return tzdbIds;
        }
        #endregion ListTzdbIdByMapZone
        #region ListTzdbIdByWindowsId
        /// <summary>
        /// Retourne la liste des TimeZones pour un WindowsId donné
        /// </summary>
        public static List<string> ListTzdbIdByWindowsId(string pWindowsId)
        {
            List<MapZone> mapZones = Tools.ListMapZonesByWindowsId(pWindowsId);
            return ListTzdbIdByMapZone(mapZones);
        }
        #endregion MapZonesByWindowsId

        #region ListWindowsIdByMapZone
        /// <summary>
        /// Retourne la liste des WindowsId pour plusieurs MapZones
        /// </summary>
        public static List<string> ListWindowsIdByMapZone()
        {
            List<string> windowsIds = new List<string>();
            IList<MapZone> mapZones = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
            if ((null != mapZones) && (0 < mapZones.Count))
                windowsIds = ListWindowsIdByMapZone(mapZones.ToList());
            return windowsIds;
        }
        public static List<string> ListWindowsIdByMapZone(List<MapZone> pMapZones)
        {
            List<string> windowsIds = new List<string>();

            pMapZones.ForEach(mapZone => {
                if (false == windowsIds.Contains(mapZone.WindowsId))
                    windowsIds.Add(mapZone.WindowsId);
            });
            windowsIds.Sort();
            return windowsIds;
        }
        #endregion ListWindowsIdByMapZone

        #region MapZoneByTzDbId
        /// <summary>
        /// Retourne la mapZone pour un TimeZone donné
        /// </summary>
        public static MapZone MapZoneByTzDbId(string pTzdbId)
        {
            var mappings = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
            var mapZone = mappings.LastOrDefault(mz =>
                mz.TzdbIds.Any(tz => tz.Equals(pTzdbId, StringComparison.OrdinalIgnoreCase)));

            if (null == mapZone)
                throw new InvalidTimeZoneException(String.Format("{0} was not recognized as a valid IANA timezone name.", pTzdbId));

            return mapZone;
        }
        #endregion MapZoneByTzDbId
        
        #region TerritoryFromTzdbId
        /// <summary>
        /// Retourne la territoire d'un TzdbId
        /// </summary>
        /// <returns></returns>
        public static string TerritoryFromTzdbId(string pTzdbId)
        {
            MapZone mapZone = MapZoneByTzDbId(pTzdbId);
            return mapZone.Territory;
        }
        #endregion TerritoryFromTzdbId

        #region TzdbIdToWindowsId
        /// <summary>
        /// Get the equivalent Window timezone ID (mapZone.WindowsId) for an IANA timezone Id (TzdbId).
        /// </summary>
        /// <param name="pTzdbId">The IANA time zone ID. (example : Europe/Paris)</param>
        /// <returns>A Windows time zone ID. (example : Romance)</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string (pTzdbId) was not recognized or has no equivalent Windows timezone id (WindowsId).</exception>
        /// EG 20171031 [23509] Upd
        /// FI 20171103 [23039] Modify
        public static string TzdbIdToWindowsId(string pTzdbId)
        {
            string tzdbId = pTzdbId;
            if (StrFunc.IsEmpty(pTzdbId)) //FI 20171103 [23039] Add
                throw new ArgumentNullException("Method TzdbIdToWindowsId: pTzdbId argument is Empty"); 
            
            if (tzdbId.Equals("Etc/UTC", StringComparison.Ordinal))
                tzdbId = "Etc/GMT";
            MapZone mapZone = MapZoneByTzDbId(tzdbId);
            return mapZone.WindowsId;
        }
        #endregion TzdbIdToWindowsId
        #region WindowsIdToTzdbId
        /// <summary>
        /// Get the equivalent IANA timezone Id (mapZone.TzdbId) for Window timezone ID (mapZone.WindowsId).
        /// </summary>
        /// <param name="pWindowsId">The Windows timezone ID. (example : Romance)</param>
        /// <param name="pWindowsId">The territory</param>
        /// <returns>A IANA time zone ID. (example : Europe/Paris)</returns>
        /// <exception cref="InvalidTimeZoneException">Thrown if the input string (pWindowsId) was not recognized or has no equivalent IANA timezone ID (TzdbId).</exception>
        /// EG 20170922 [22374] Upd
        // EG 20171031 [23509] Upd
        public static string WindowsIdToTzdbId(string pWindowsId)
        {
            if (pWindowsId.Equals("UTC", StringComparison.Ordinal))
                return "Etc/UTC";

            TzdbDateTimeZoneSource tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
            if (false == tzdbSource.WindowsMapping.PrimaryMapping.TryGetValue(pWindowsId, out string result))
                throw new InvalidTimeZoneException(String.Format("{0} was not recognized as a valid Windows timezone ID.", pWindowsId));
            return tzdbSource.CanonicalIdMap[result];
        }
        #endregion WindowsIdToTzdbId

        #region SetCulturePatterns
        public static void SetCulturePatterns(CultureInfo pCultureInfo)
        {
            cPatterns.SetCulturePatterns(pCultureInfo);
        }
        #endregion SetCulturePatterns

        #region ToDateTimeOffset
        /// <summary>
        /// Convertie {pvalue} DateTimeOffset (Utilsation de DateTimeOffset.TryParse).Retourne DateTimeOffset.MinValue si la convertion n'aboutie pas 
        /// </summary>
        /// <param name="pValue">Date en entrée</param>
        /// <returns></returns>
        /// EG 20171004 [22374][23452] New
        /// EG 20171031 [23509] Upd
        public static Nullable<DateTimeOffset> ToDateTimeOffset(string pValue)
        {
            DateTimeOffset? ret = null;
            if (DateTimeOffset.TryParse(pValue, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dto))
                ret = dto;
            return ret;
        }
        #endregion ToDateTimeOffset
        #region ToOffsetDateTime
        /// <summary>
        /// Convertie une date string en  OffsetDateTime
        /// </summary>
        /// <param name="pValue">Date en entrée</param>
        /// <returns></returns>
        // EG 20171031 [23509] Upd
        public static Nullable<OffsetDateTime> ToOffsetDateTime(string pValue)
        {
            return ToOffsetDateTime(ToDateTimeOffset(pValue));
        }
        /// <summary>
        /// Convertie une date string en  OffsetDateTime
        /// </summary>
        /// <param name="pValue">Date en entrée</param>
        /// <returns></returns>
        // EG 20171031 [23509] Upd
        public static Nullable<OffsetDateTime> ToOffsetDateTime(Nullable<DateTimeOffset> pValue)
        {
            Nullable<OffsetDateTime> ret = null;
            if (pValue.HasValue)
                ret = OffsetDateTime.FromDateTimeOffset(pValue.Value);
            return ret;
        }
        #endregion ToOffsetDateTime
        #region ToZonedDateTime
        /// <summary>
        /// Convertie une date string en NodaTime.ZonedDateTime 
        /// </summary>
        /// <param name="pValue">Date source</param>
        /// <param name="pTzdbId">Nom du TzdbId </param>
        /// <returns></returns>
        // EG 20171031 [23509] Upd
        public static Nullable<ZonedDateTime> ToZonedDateTime(string pValue, DateTimeZone pDtz)
        {
            Nullable<ZonedDateTime> zdt = null;
            Nullable<OffsetDateTime> odt = Tools.ToOffsetDateTime(pValue);
            if (odt.HasValue)
                zdt = ToZonedDateTime(odt.Value, pDtz);
            return zdt;
        }
        /// <summary>
        /// Convertie une date DateTimeOffset en NodaTime.ZonedDateTime 
        /// </summary>
        /// <param name="pValue">Date source</param>
        /// <param name="pTzdbId">Nom du TzdbId </param>
        /// <returns></returns>
        public static ZonedDateTime ToZonedDateTime(DateTimeOffset pValue)
        {

            return ZonedDateTime.FromDateTimeOffset(pValue);
        }
        /// <summary>
        /// Convertie une date OffsetDateTime en NodaTime.ZonedDateTime 
        /// 1. Recherche du DateTimeZone associé au pTzdbId.
        /// 2. Application de l'offset du DateTimeZone sur la date source.
        /// 3. Conversion de la date source (+ offset du TzdbId) en ZonedDateTime.
        /// </summary>
        /// <param name="pValue">Date source</param>
        /// <param name="pTzdbId">Nom du TzdbId </param>
        /// <returns></returns>
        // EG 20171031 [23509] Upd
        public static Nullable<ZonedDateTime> ToZonedDateTime(string pValue, string pTzdbId)
        {
            Nullable<ZonedDateTime> ret = null;
            Nullable<OffsetDateTime> odt = ToOffsetDateTime(pValue);
            if (odt.HasValue)
                ret = ToZonedDateTime(odt.Value, pTzdbId);
            return ret;
        }
        /// <summary>
        /// Convertie une date OffsetDateTime en NodaTime.ZonedDateTime 
        /// 1. Recherche du DateTimeZone associé au pTzdbId.
        /// 2. Application de l'offset du DateTimeZone sur la date source.
        /// 3. Conversion de la date source (+ offset du TzdbId) en ZonedDateTime.
        /// </summary>
        /// <param name="pValue">Date source</param>
        /// <param name="pTzdbId">Nom du TzdbId </param>
        /// <returns></returns>
        public static ZonedDateTime ToZonedDateTime(OffsetDateTime pValue, string pTzdbId)
        {
            DateTimeZone dtz = DateTimeZoneProviders.Tzdb[pTzdbId];
            return ToZonedDateTime(pValue, dtz);
        }
        /// <summary>
        /// Convertie une date OffsetDateTime en NodaTime.ZonedDateTime 
        /// 1. Application de l'offset du DateTimeZone sur la date source.
        /// 2. Conversion de la date source (+ offset du TzdbId) en ZonedDateTime.
        /// </summary>
        /// <param name="pValue">Date source</param>
        /// <param name="pTzdbId">Nom du TzdbId </param>
        /// <returns></returns>
        public static ZonedDateTime ToZonedDateTime(OffsetDateTime pValue, DateTimeZone pDtz)
        {
            ZoneLocalMappingResolver resolver = Resolvers.CreateMappingResolver(Resolvers.ReturnEarlier, Resolvers.ReturnStartOfIntervalAfter);
            OffsetDateTime odt = pValue.WithOffset(pDtz.GetUtcOffset(pValue.ToInstant()));
            return pDtz.ResolveLocal(odt.LocalDateTime, resolver);
        }
        #endregion ToZonedDateTime

        #region ToString
        public static string ToString<T>(T pSource)
        {
            return ToString(pSource, CultureInfo.InvariantCulture);
        }
        // EG 20170922 [22374] Upd
        // EG 20171004 [22374][23452] Upd
        // EG 20171025 [23509] Add pSource is DateTime
        // EG 20171031 [23509] Upd
        public static string ToString<T>(T pSource, CultureInfo pCultureInfo)
        {
            string patternText = NodaTimeForText.LocalDateTimePattern.FullRoundtripWithoutCalendar.PatternText;

            if (pSource is Instant)
                patternText = cPatterns.instantPattern.PatternText;
            else if (pSource is DateTime)
                patternText = DtFunc.FmtISOLongDateTime;
            else if (pSource is DateTimeOffset)
                patternText = cPatterns.iso8601UTCPattern.PatternText;
            else if (pSource is LocalTime)
                patternText = NodaTimeForText.LocalTimePattern.ExtendedIso.PatternText;
            else if (pSource is LocalDate)
                patternText = NodaTimeForText.LocalDatePattern.Iso.PatternText;
            else if (pSource is LocalDateTime)
                patternText = NodaTimeForText.LocalDateTimePattern.FullRoundtripWithoutCalendar.PatternText;
            else if (pSource is OffsetDateTime)
                patternText = cPatterns.offsetDatetimePattern.PatternText;
            else if (pSource is ZonedDateTime)
                patternText = cPatterns.zonedDateTimePattern.PatternText;
            return ToString(pSource, patternText, pCultureInfo);
        }
        // EG 20171004 [22374][23452] Upd
        // EG 20171025 [23509] Add pSource is DateTime
        public static string ToString<T>(T pSource, string pPatternText, CultureInfo pCultureInfo)
        {
            string ret = string.Empty;
            if (pSource is Instant)
            {
                ret = (pSource as Nullable<Instant>).Value.ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is DateTime)
            {
                ret = (pSource as Nullable<DateTime>).Value.ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is LocalTime)
            {
                ret = (pSource as Nullable<LocalTime>).Value.ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is LocalDate)
            {
                ret = (pSource as Nullable<LocalDate>).Value.ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is LocalDateTime)
            {
                ret = (pSource as Nullable<LocalDateTime>).Value.ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is OffsetDateTime)
            {
                ret = (pSource as Nullable<OffsetDateTime>).Value.ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is DateTimeOffset)
            {
                Nullable<DateTimeOffset> source = pSource as Nullable<DateTimeOffset>;
                ret = ToZonedDateTime(source.Value).ToInstant().ToString(pPatternText, pCultureInfo);
            }
            else if (pSource is ZonedDateTime)
            {
                ret = (pSource as Nullable<ZonedDateTime>).Value.ToString(pPatternText, pCultureInfo);
            }
            return ret;
        }
        /// <summary>
        /// Convertie {pValue} en Nodatime.OffsetDateTime puis retourne le résultat au format yyyy-MM-ddTHH:mm:ss:FFFFFFFFF
        /// </summary>
        /// EG 20171025 [23509] New
        public static string DateTimeToStringISO(string pValue)
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(pValue))
                ret = ToString(ToOffsetDateTime(pValue), NodaTimeForText.LocalDateTimePattern.ExtendedIso.PatternText, null);
            return ret;
        }

        
        /// <summary>
        /// Convertie {pValue} en Nodatime.OffsetDateTime puis retourne le résultat au format yyyy-MM-dd
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// EG 20171031 [23509] Upd
        public static string DateToStringISO(string pValue)
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(pValue))
            {
                Nullable<OffsetDateTime> odt = ToOffsetDateTime(pValue);
                if (odt.HasValue)
                    ret = ToString(odt.Value.Date, NodaTimeForText.LocalDatePattern.Iso.PatternText, null);
            }
            return ret;
        }

        // EG 20171031 [23509] Upd
        public static string TimeToStringISO(string pValue)
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(pValue))
            {
                Nullable<OffsetDateTime> odt = ToOffsetDateTime(pValue);
                if (odt.HasValue)
                    ret = ToString(odt.Value.TimeOfDay, NodaTimeForText.LocalTimePattern.ExtendedIso.PatternText, null);
            }
            return ret;
        }

        #endregion ToString

        // EG 20171031 [23509] Upd
        public static long GetTickOfDayFromTzdbId(string pValue, string pTzdbId)
        {
            long ret = 0;
            Nullable<ZonedDateTime> zdt = ToZonedDateTime(pValue, pTzdbId);
            if (zdt.HasValue)
                ret = zdt.Value.TimeOfDay.TickOfDay;
            return ret;
        }

        /// <summary>
        /// Retourne la date au format ISO dans le TimeZone spécifié
        /// Ex : 2017-10-12 pour 2017-10-12T14:00:00.000000Z avec Europe/Paris
        ///      2017-10-12 pour 2017-10-11T23:00:00.000000Z avec Europe/Paris
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pTzdbId"></param>
        /// <returns></returns>
        // EG 20171025 [23509] New
        // EG 20171031 [23509] New
        public static string ToStringFromTimeZone(string pValue, string pTzdbId)
        {
            string ret = string.Empty;
            Nullable<DateTimeOffset> dto = FromTimeZone(pValue, pTzdbId);
            if (dto.HasValue)
                ret = ToString(dto.Value);
            return ret;
        }
        // EG 20171031 [23509] New
        public static Nullable<DateTimeOffset> FromTimeZone<T>(T pSource, string pTzdbId)
        {
            Nullable<DateTimeOffset> dto_Result = null;
            Nullable<DateTimeOffset> dto_Source = null;
            if (pSource is string)
            {
                dto_Source = ToDateTimeOffset(pSource as string);
            }
            else if (pSource is DateTime)
            {
                Nullable<DateTime> source = pSource as Nullable<DateTime>;
                dto_Source = new DateTimeOffset(source.Value);
            }
            else if (pSource is DateTimeOffset)
            {
                Nullable<DateTimeOffset> source = pSource as Nullable<DateTimeOffset>;
                dto_Source = source.Value;
            }
            if (dto_Source.HasValue && (dto_Source.Value != DateTimeOffset.MinValue))
            {
                DateTimeZone dtz = DateTimeZoneProviders.Tzdb[pTzdbId];
                OffsetDateTime odt_Source = OffsetDateTime.FromDateTimeOffset(dto_Source.Value);
                OffsetDateTime odt_Result = odt_Source.WithOffset(dtz.GetUtcOffset(odt_Source.ToInstant()));
                dto_Result = odt_Result.ToDateTimeOffset();
            }
            return dto_Result;
        }
        /// <summary>
        /// Retourne un nom court pour le timezone {pTzdbId}
        /// <para>Exemple Europe/Rome devient Rome</para>
        /// </summary>
        /// <param name="pTzdbId"></param>
        /// <returns></returns>
        /// FI 20190327 [24603] Add Method
        public static string GetShortTzdbId(string pTzdbId)
        {
            string ret = pTzdbId;
            if (StrFunc.IsFilled(pTzdbId))
            {
                string[] split = pTzdbId.Split('/');
                if (ArrFunc.Count(split) == 1)
                    ret = split[0];
                else if (ArrFunc.Count(split) > 1)
                    ret = split[ArrFunc.Count(split) - 1];
            }
            return ret;
        }
        
        #endregion Methods
    }
}

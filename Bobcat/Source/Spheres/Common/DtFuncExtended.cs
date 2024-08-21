using EFS.ACommon;
using EFS.Actor;
using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Tz = EFS.TimeZone;

namespace EFS.Common
{



    /// <summary>
    /// Représente un horodatage avec son fuseau horaire (IANA)
    /// <para>Si tzdbId est non renseigné. Spheres® suppose que l'horodatage en exprimé en local</para>
    /// </summary>
    public class DateTimeTz : ICloneable
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTimeTz()
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pTz"></param>
        public DateTimeTz(DateTime pDate, string pTz)
        {
            Date = pDate;
            TzdbId = pTz;
            SetKind();
        }

        /// <summary>
        /// Obtient l'horodatage
        /// </summary>
        public DateTime Date
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le fuseau horaire
        /// </summary>
        public string TzdbId
        {
            get;
            private set;
        }
        /// <summary>
        /// 
        /// </summary>
        public Boolean TzdbIdSpecified
        {
            get { return StrFunc.IsFilled(this.TzdbId); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string ret = Date.ToString();
            if (StrFunc.IsFilled(this.TzdbId))
                ret += Cst.Space + Tz.Tools.GetShortTzdbId(this.TzdbId);
            return ret;
        }

        /// <summary>
        /// Convertie l'horodatage courant dans le fuseau horaire {pTzdbIdTarget}
        /// </summary>
        /// <param name="pTzdbIdTarget"></param>
        /// <returns></returns>
        public DateTimeTz ConvertTimeToTz(string pTzdbIdTarget)
        {
            return DtFuncExtended.ConvertTimeToTz(this, pTzdbIdTarget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new DateTimeTz(this.Date, this.TzdbId);
        }

        private void SetKind()
        {
            if (TzdbIdSpecified)
            {
                // L'heure représentée n'est pas spécifiée comme heure locale ou heure UTC.
                this.Date = DateTime.SpecifyKind(this.Date, DateTimeKind.Unspecified);
            }
            else if (TzdbId == "Etc/UTC")
            {
                // L'heure représentée n'est pas spécifiée comme heure locale ou heure UTC.
                this.Date = DateTime.SpecifyKind(this.Date, DateTimeKind.Utc);
            }
            else
            {
                // L'heure représentée est l'heure locale.
                this.Date = DateTime.SpecifyKind(this.Date, DateTimeKind.Local);
            }
        }
    }

    public class DtFuncExtended
    {


        /// <summary>
        /// Conversion de <paramref name="pDateTime"/> (horodatage) dans le fuseau horaire attentu (fonction du profil utilisateur (horodatages d'audit))
        /// <para>Si le paramétage du profil abouti à un fuseau horaire non renseigné alors la date reste inchangée</para>
        /// </summary>
        /// <param name="pDateTime">Représente un horodatage (avec son tzdbId)</param>
        /// <param name="auditTimestampZone">représente le fuseau horaire attendu</param>
        /// <param name="pCollaborator">utilisateur (avec sa hiérarchie)</param>
        /// FI 20200729 [XXXXX] Add
        public static DateTimeTz ConvertTimestampAudit(DateTimeTz pDateTime, Cst.AuditTimestampZone auditTimestampZone, Collaborator pCollaborator)
        {
            return DtFuncExtended.ConvertTimestamp<Cst.AuditTimestampZone>(pDateTime, auditTimestampZone, pCollaborator, null);
        }

        /// <summary>
        /// Conversion de <paramref name="pDateTime"/> (horodatage) dans le fuseau horaire attentu (fonction du profil utilisateur (horodatages de trading)) 
        /// <para>Si le paramétage du profil abouti à un fuseau horaire non renseigné alors la date reste inchangée</para>
        /// </summary>
        /// <param name="pDateTime">Représente un horodatage (avec son tzdbId)</param>
        /// <param name="tradingTimestampZone">représente le fuseau horaire attendu</param>
        /// <param name="pCollaborator">utilisateur (avec sa hiérarchie)</param>
        /// <param name="pRow">représente un ligne de donnée ayant la colonne "Facility" (à renseigner uniquement si tradingTimestampZone = TradingTimestampZone.Facility)</param>
        /// FI 20200729 [XXXXX] Add
        public static DateTimeTz ConvertTimestampTrading(DateTimeTz pDateTime, Cst.TradingTimestampZone tradingTimestampZone, Collaborator pCollaborator, DataRow pRow)
        {
            return DtFuncExtended.ConvertTimestamp<Cst.TradingTimestampZone>(pDateTime, tradingTimestampZone, pCollaborator, pRow);
        }

        /// <summary>
        /// Conversion de <paramref name="pDateTime"/> (horodatage) dans le fuseau horaire attentu (fonction du profil utilisateur).Si le paramétage du profil abouti à un fuseau horaire non renseigné alors la date reste inchangée
        /// </summary>
        /// <typeparam name="T"><seealso cref="Cst.AuditTimestampZone"/> OU <seealso cref="Cst.TradingTimestampZone"/>OU <seealso cref="Cst.DeliveryTimestampZone"/></typeparam>
        /// <param name="pDateTime">Représente un horodatage (avec son tzdbId)</param>
        /// <param name="pEnum">Valeur de T</param>
        /// <param name="pCollaborator">utilisateur (avec sa hiérarchie)</param>
        /// <param name="pRow">représente l'enregistrement ayant 
        /// - soit la colonne "TZFACILITY" ( Doit être renseigné si <paramref name="pEnum"/> = TradingTimestampZone.Facility)
        /// - soit la colonne "TZDLVY" ( Doit être renseigné si <paramref name="pEnum"/> = DeliveryTimestampZone.Delivery)
        /// </param>
        /// <returns></returns>
        /// FI 20200720 [XXXXX] Add
        private static DateTimeTz ConvertTimestamp<T>(DateTimeTz pDateTime, T pEnum, Collaborator pCollaborator, DataRow pRow) where T : struct
        {
            if (false == ((pEnum is Cst.TradingTimestampZone) || (pEnum is Cst.AuditTimestampZone) || (pEnum is Cst.DeliveryTimestampZone)))
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pEnum.ToString()));

            string tzdbIdTarget = null;
            TimeZoneInfo tzInfoTarget = null;
            switch (pEnum.ToString())
            {
                //Conversion de la l'horodatage selon le timezone de la plateforme
                case "Facility":
                    if (false == pRow.Table.Columns.Contains("TZFACILITY"))
                        throw new NullReferenceException("column TZFACILITY doesn't exist.");

                    object tzFacility = pRow["TZFACILITY"];
                    if ((Convert.DBNull != tzFacility) && StrFunc.IsFilled(tzFacility.ToString()))
                    {
                        tzdbIdTarget = tzFacility.ToString();
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    break;
                //Conversion de la l'horodatage selon le timezone de la livraison
                case "Delivery": // FI 20221207 [XXXXX] Add
                    if (false == pRow.Table.Columns.Contains("TZDLVY"))
                        throw new NullReferenceException("column TZDLVY doesn't exist.");

                    object tzDelivery = pRow["TZDLVY"];
                    if ((Convert.DBNull != tzDelivery) && StrFunc.IsFilled(tzDelivery.ToString()))
                    {
                        tzdbIdTarget = tzDelivery.ToString();
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    break;
                case "UTC":
                    // pas de conversion
                    tzInfoTarget = TimeZoneInfo.Utc;
                    tzdbIdTarget = "Etc/UTC";
                    break;
                case "User":
                    //Conversion de la date selon le zimezone spécifé sur l'acteur 
                    if (StrFunc.IsFilled(pCollaborator.Timezone))
                    {
                        tzdbIdTarget = pCollaborator.Timezone;
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    else if (StrFunc.IsFilled(pCollaborator.Department.Timezone))
                    {
                        tzdbIdTarget = pCollaborator.Department.Timezone;
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    else if (StrFunc.IsFilled(pCollaborator.Entity.Timezone))
                    {
                        tzdbIdTarget = pCollaborator.Entity.Timezone;
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    break;
                case "Department":
                    //Conversion de la date selon le zimezone spécifé sur le département
                    if (StrFunc.IsFilled(pCollaborator.Department.Timezone))
                    {
                        tzdbIdTarget = pCollaborator.Department.Timezone;
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    else if (StrFunc.IsFilled(pCollaborator.Entity.Timezone))
                    {
                        tzdbIdTarget = pCollaborator.Entity.Timezone;
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    break;
                case "Entity":
                    //Conversion de la date selon le zimezone spécifé sur l'entité
                    if (StrFunc.IsFilled(pCollaborator.Entity.Timezone))
                    {
                        tzdbIdTarget = pCollaborator.Entity.Timezone;
                        tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbIdTarget);
                    }
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("value ({0}) is not implemented", pEnum.ToString()));

            }

            DateTimeTz ret = pDateTime.Clone() as DateTimeTz;
            if (null != tzInfoTarget)
                ret = pDateTime.ConvertTimeToTz(tzdbIdTarget);

            return ret;
        }

        /// <summary>
        /// Conversion de {pDateTime} (horodatage) dans un autre fuseau horaire {pTzdbIdTarget}
        /// </summary>
        /// <param name="pDateTime">Représente un horodatage et son fuseau horaire (IANA)</param>
        /// <param name="pTzdbIdTarget">Représente un fuseau horaire (IANA)</param>
        /// FI 20200720 [XXXXX] Add
        public static DateTimeTz ConvertTimeToTz(DateTimeTz pDateTime, string pTzdbIdTarget)
        {
            DateTime dateUTc;
            if (StrFunc.IsFilled(pDateTime.TzdbId))
            {
                TimeZoneInfo tzInfosource = Tz.Tools.GetTimeZoneInfoFromTzdbId(pDateTime.TzdbId);
                dateUTc = TimeZoneInfo.ConvertTimeToUtc(pDateTime.Date, tzInfosource);
            }
            else
            {
                // Remarque : Il est supposé ici que les serveurs Web et SQL se trouvent dans la même timezone
                dateUTc = new DateTimeOffset(pDateTime.Date).UtcDateTime;
            }

            DateTimeTz ret = new DateTimeTz(
                TimeZoneInfo.ConvertTimeFromUtc(dateUTc, Tz.Tools.GetTimeZoneInfoFromTzdbId(pTzdbIdTarget)),
                pTzdbIdTarget);

            return ret;
        }


        /// <summary>
        /// Formatage de {pDateTime} (horodatage) dans la précision attentue (fonction du profil utilisateur (horodatages d'audit))
        /// </summary>
        /// <param name="pDateTime">Représente un horodatage</param>
        /// <param name="pType">date, datetime ou time</param>
        /// <param name="pAuditTimestampPrecision"></param>
        /// <returns></returns>
        /// FI 20200729 [XXXXX] Add
        public static string FormatTimestampAudit(DateTime pDateTime, string pType, Cst.AuditTimestampPrecision pAuditTimestampPrecision)
        {
            return FormatTimestamp<Cst.AuditTimestampPrecision>(pDateTime, pType, pAuditTimestampPrecision);
        }

        /// <summary>
        /// Formatage de {pDateTime} (horodatage) dans la précision attentue (fonction du profil utilisateur (horodatages de trading)) 
        /// </summary>
        /// <param name="pDateTime">Représente un horodatage</param>
        /// <param name="pType">date, datetime ou time</param>
        /// <param name="pTradingTimestampPrecision"></param>
        /// <returns></returns>
        /// FI 20200729 [XXXXX] Add
        public static string FormatTimestampTrading(DateTime pDateTime, string pType, Cst.TradingTimestampPrecision pTradingTimestampPrecision)
        {
            return FormatTimestamp<Cst.TradingTimestampPrecision>(pDateTime, pType, pTradingTimestampPrecision);
        }

        /// <summary>
        /// Formatage de <paramref name="pDateTime"/> (horodatage) dans la précision attentue (fonction du profil utilisateur)
        /// </summary>
        /// <typeparam name="T"><seealso cref="Cst.AuditTimestampZone"/> OU <seealso cref="Cst.TradingTimestampZone"/>OU <seealso cref="Cst.DeliveryTimestampZone"/></typeparam>
        /// <param name="pDateTime">Représente un horodatage</param>
        /// <param name="pType">date, datetime ou time</param>
        /// <param name="pEnum">une valeur de T</param>
        /// <returns></returns>
        /// FI 20200720 [XXXXX] Add
        /// EG 20220503 [XXXXX] Add Gestion Format DD/MM pour le tracker sur lignes hors journée en cours
        private static string FormatTimestamp<T>(DateTime pDateTime, string pType, T pEnum) where T : struct
        {
            string ret = string.Empty;

            if (false == ((pEnum is Cst.TradingTimestampPrecision) || (pEnum is Cst.AuditTimestampPrecision) || (pEnum is Cst.DeliveryTimestampPrecision)))
                throw new NotImplementedException(StrFunc.AppendFormat("Enum ({0}) is not implemented", pEnum.ToString()));

            Boolean isDateTime = TypeData.IsTypeDateTime(pType);
            Boolean isDate = TypeData.IsTypeDate(pType);
            Boolean isTime = TypeData.IsTypeTime(pType);

            if (isDate)
            {
                //Sur les dates => Spheres® n'affiche pas le tzDbId
                switch (pEnum.ToString())
                {
                    case "DDMM":
                        ret = pDateTime.ToString("dd/MM");
                        break;
                    default:
                        ret = pDateTime.ToString(DtFunc.FmtShortDate);
                        break;
                }
            }
            else if (isDateTime || isTime)
            {
                switch (pEnum.ToString())
                {
                    case "Minute":
                        if (isDateTime)
                            ret = pDateTime.ToString(DtFunc.FmtDateTime);
                        else if (isTime)
                            ret = pDateTime.ToString(DtFunc.FmtShortTime);
                        break;
                    case "Second":
                        if (isDateTime)
                            ret = pDateTime.ToString(DtFunc.FmtDateLongTime);
                        else if (isTime)
                            ret = pDateTime.ToString(DtFunc.FmtLongTime);
                        break;
                    case "Millisecond":
                    case "Microsecond":
                        string precison = (pEnum.ToString() == "Millisecond") ? "$1.fff" : "$1.ffffff";

                        CultureInfo cultureInfo = CultureInfo.CurrentCulture.Clone() as CultureInfo;
                        DateTimeFormatInfo dfi = cultureInfo.DateTimeFormat;
                        dfi.LongTimePattern = Regex.Replace(dfi.LongTimePattern, "(:ss|:s)", precison);

                        if (isDateTime)
                            ret = pDateTime.ToString(DtFunc.FmtDateLongTime, cultureInfo);
                        else if (isTime)
                            ret = pDateTime.ToString(DtFunc.FmtLongTime, cultureInfo);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Precision : {0} is not supported", pEnum.ToString()));
                }
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("type ({0}) is not implemented", pType));
            }

            return ret;
        }

        /// <summary>
        /// Affichage d'un horodatage d'audit
        /// </summary>
        /// <param name="dateTimeTz"></param>
        /// <param name="pAuditTimestampInfo"></param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        public static string DisplayTimestampAudit(DateTimeTz dateTimeTz, AuditTimestampInfo pAuditTimestampInfo)
        {
            return DisplayTimestamp<AuditTimestampInfo>(dateTimeTz, pAuditTimestampInfo);

        }

        /// <summary>
        /// Affichage d'un horodatage de trading
        /// </summary>
        /// <param name="dateTimeTz"></param>
        /// <param name="pTradingTimestampInfo"></param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        public static string DisplayTimestampTrading(DateTimeTz dateTimeTz, TradingTimestampInfo pTradingTimestampInfo)
        {
            return DisplayTimestamp<TradingTimestampInfo>(dateTimeTz, pTradingTimestampInfo);
        }


        /// <summary>
        /// Affichage d'un horodatage système (exprimé en UTC)
        /// </summary>
        /// <param name="pDtsys">Date système exprimé en UTC</param>
        /// <param name="pAuditTimestampInfo"></param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        // EG 20230709[XXXXX] Corrections diverses sur equity Option (Demo BFF)
        public static string DisplayTimestampUTC(DateTime pDtsys, AuditTimestampInfo pAuditTimestampInfo)
        {
            //if (pDtsys.Kind != DateTimeKind.Utc)
            //    throw new ArgumentException("date in UTC format expected");
            return DisplayTimestampAudit(new DateTimeTz(pDtsys, "Etc/UTC"), pAuditTimestampInfo);
        }

        /// <summary>
        /// Affichage d'un horodatage d'audit, de trading ou de delivery
        /// </summary>
        /// <typeparam name="T"><seealso cref="Cst.AuditTimestampZone"/> OU <seealso cref="Cst.TradingTimestampZone"/>OU <seealso cref="Cst.DeliveryTimestampZone"/></typeparam>
        /// <param name="dateTimeTz">Représente un horodatage</param>
        /// <param name="pTimestampInfo">Pilote l'affichage de l'horodatage</param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add 
        public static string DisplayTimestamp<T>(DateTimeTz dateTimeTz, T pTimestampInfo) where T : TimestampInfoBase
        {
            DateTimeTz dtZonedTarget;
            string fmtTimestamp;

            if (pTimestampInfo.GetType().Equals(typeof(TradingTimestampInfo)))
            {
                TradingTimestampInfo info = pTimestampInfo as TradingTimestampInfo;
                dtZonedTarget = DtFuncExtended.ConvertTimestamp<Cst.TradingTimestampZone>(dateTimeTz, info.TimestampZone, info.Collaborator, info.DataRow);
                fmtTimestamp = DtFuncExtended.FormatTimestamp<Cst.TradingTimestampPrecision>(dtZonedTarget.Date, info.Type, info.Precision);
            }
            else if (pTimestampInfo.GetType().Equals(typeof(AuditTimestampInfo)))
            {
                AuditTimestampInfo info = pTimestampInfo as AuditTimestampInfo;
                dtZonedTarget = DtFuncExtended.ConvertTimestamp<Cst.AuditTimestampZone>(dateTimeTz, info.TimestampZone, info.Collaborator, null);
                fmtTimestamp = DtFuncExtended.FormatTimestamp<Cst.AuditTimestampPrecision>(dtZonedTarget.Date, info.Type, info.Precision);
            }
            else if (pTimestampInfo.GetType().Equals(typeof(DeliveryTimestampInfo)))
            {
                DeliveryTimestampInfo info = pTimestampInfo as DeliveryTimestampInfo;
                dtZonedTarget = DtFuncExtended.ConvertTimestamp<Cst.DeliveryTimestampZone>(dateTimeTz, info.TimestampZone, info.Collaborator, info.DataRow);
                fmtTimestamp = DtFuncExtended.FormatTimestamp<Cst.DeliveryTimestampPrecision>(dtZonedTarget.Date, info.Type, info.Precision);
            }
            else
                throw new NotImplementedException(message: $"{pTimestampInfo.GetType()} is not implemeted");

            string ret;
            if (pTimestampInfo.WithTzdbId && dtZonedTarget.TzdbIdSpecified)
                ret = $"{fmtTimestamp} {Tz.Tools.GetShortTzdbId(dtZonedTarget.TzdbId)}";
            else
                ret = $"{fmtTimestamp}";

            return ret;
        }
    }

    /// <summary>
    /// classe de base pour affichage des horodatages
    /// </summary>
    /// FI 20200820 [25468] Add 
    public abstract class TimestampInfoBase
    {
        /// <summary>
        /// Affichage du timezone oui/non
        /// </summary>
        public Boolean WithTzdbId;

        /// <summary>
        ///  Représente l'utilisateur
        /// </summary>
        public Collaborator Collaborator;

        /// <summary>
        ///  datetime ou time. 
        ///  <para>Si datetime Affichage de la date et de l'heure. si time Affichage de l'heure uniquement</para>
        /// </summary>
        public string Type;

        public TimestampInfoBase()
        {
            WithTzdbId = true;
            Type = "datetime";
        }
    }

    /// <summary>
    /// Pilote l'affichage des horodatages de type Audit
    /// </summary>
    /// FI 20200820 [25468] Add 
    public class AuditTimestampInfo : TimestampInfoBase
    {
        /// <summary>
        ///  Représente le TimeZone attendue
        /// </summary>
        public Cst.AuditTimestampZone TimestampZone;

        /// <summary>
        /// représente la precision attendue
        /// </summary>
        public Cst.AuditTimestampPrecision Precision;

        /// <summary>
        /// 
        /// </summary>
        public AuditTimestampInfo() : base()
        { }


        /// <summary>
        ///  Contructor pour affichage dans le timezone de l'utilisateur et en seconde
        /// </summary>
        public AuditTimestampInfo(Collaborator pCollaborator) : base()
        {
            Collaborator = pCollaborator;
            TimestampZone = Cst.AuditTimestampZone.User;
            Precision = Cst.AuditTimestampPrecision.Second;
        }
    }

    /// <summary>
    /// Pilote l'affichage des horodatages de type Trading
    /// </summary>
    /// FI 20200820 [25468] Add 
    public class TradingTimestampInfo : TimestampInfoBase
    {
        /// <summary>
        ///  Représente le TimeZone attendue 
        /// </summary>
        public Cst.TradingTimestampZone TimestampZone;

        /// <summary>
        /// représente la precision attendue
        /// </summary>
        public Cst.TradingTimestampPrecision Precision;

        /// <summary>
        /// Représente l'enregistrement contenant la colonne TZFACILITY (à renseigner uniquement si tradingTimestampZone = TradingTimestampZone.Facility)
        /// </summary>
        public DataRow DataRow;


        public TradingTimestampInfo() : base()
        { }

        /// <summary>
        ///  Contructor pour affichage dans le timezone du facility et en secondes
        /// </summary>
        /// <param name="pRow"></param>
        public TradingTimestampInfo(DataRow pRow) : base()
        {
            DataRow = pRow;
            TimestampZone = Cst.TradingTimestampZone.Facility;
            Precision = Cst.TradingTimestampPrecision.Second;
        }
    }


    /// <summary>
    /// Pilote l'affichage des horodatages de type Delivery
    /// </summary>
    /// FI 20200820 [25468] Add 
    public class DeliveryTimestampInfo : TimestampInfoBase
    {
        /// <summary>
        ///  Représente le TimeZone attendue 
        /// </summary>
        public Cst.DeliveryTimestampZone TimestampZone;

        /// <summary>
        /// Représente la precision attendue
        /// </summary>
        public Cst.DeliveryTimestampPrecision Precision;

        /// <summary>
        /// Représente l'enregistrement contenant la colonne TZDLVY (à renseigner uniquement si tradingTimestampZone = TradingTimestampZone.Delivery)
        /// </summary>
        public DataRow DataRow;


        public DeliveryTimestampInfo() : base()
        { }

        /// <summary>
        ///  Contructor pour affichage dans le timezone de la livraison et en minutes
        /// </summary>
        /// <param name="pRow"></param>
        public DeliveryTimestampInfo(DataRow pRow) : base()
        {
            DataRow = pRow;
            TimestampZone = Cst.DeliveryTimestampZone.Delivery;
            Precision = Cst.DeliveryTimestampPrecision.Minute;
        }
    }

}

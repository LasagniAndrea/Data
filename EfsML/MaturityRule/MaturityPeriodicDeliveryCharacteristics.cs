using EFS.ACommon;
using EFS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfsML
{

    /// <summary>
    /// Caractéristiques relatives à une livraison physique périodique (ex. PEGAS)
    /// </summary>
    public class MaturityPeriodicDeliveryCharacteristics
    {
        public MaturityPeriodicDeliveryCharacteristics()
        {
            dates = new MaturityPeriodicDeliveryDates();
            detail = new MaturityPeriodicDeliveryDetail();
        }
        public MaturityPeriodicDeliveryDates dates;
        public MaturityPeriodicDeliveryDetail detail;

        /// <summary>
        /// Initialisation des 4 dates relatives à la livraison
        /// </summary>
        /// <param name="pSqlMaturity"></param>
        public void Initialisation(SQL_Maturity pSqlMaturity)
        {
            this.dates.dtFirstDelivery = pSqlMaturity.FirstDelivryDate;
            this.dates.dtFirstDlvSettlt = pSqlMaturity.FirstDlvSettltDate;
            this.dates.dtLastDelivery = pSqlMaturity.LastDelivryDate;
            this.dates.dtLastDlvSettlt = pSqlMaturity.LastDlvSettltDate;
        }
        /// <summary>
        /// Initialisation des dates relatives à la livraison "périodique" (ex. livraison de Gaz)
        /// </summary>
        /// <param name="pSqlMaturityRule"></param>
        public void Initialisation(SQL_MaturityRuleBase pSqlMaturityRule)
        {
            this.detail.deliveryDateMultiplier = pSqlMaturityRule.MaturityPeriodicDeliveryDateMultiplier;
            this.detail.deliveryDatePeriod = pSqlMaturityRule.MaturityPeriodicDeliveryDatePeriod;
            this.detail.deliveryDateDaytype = pSqlMaturityRule.MaturityPeriodicDeliveryDateDaytype;
            this.detail.deliveryDateRollConv = pSqlMaturityRule.MaturityPeriodicDeliveryDateRollConv;

            this.detail.deliveryDateTimeStart = pSqlMaturityRule.MaturityPeriodicDeliveryDateTimeStart;
            this.detail.deliveryDateTimeEnd = pSqlMaturityRule.MaturityPeriodicDeliveryDateTimeEnd;
            this.detail.deliveryDateTimeZone = pSqlMaturityRule.MaturityPeriodicDeliveryDateTimeZone;
            this.detail.deliveryDateApplySummerTime = pSqlMaturityRule.MaturityPeriodicDeliveryDateApplySummerTime;

            this.detail.dlvSettltDateOffsetMultiplier = pSqlMaturityRule.MaturityPeriodicDlvSettltDateOffsetMultiplier;
            this.detail.dlvSettltDateOffsetPeriod = pSqlMaturityRule.MaturityPeriodicDlvSettltDateOffsetPeriod;
            this.detail.dvlSettltDateOffsetDaytype = pSqlMaturityRule.MaturityPeriodicDlvSettltDateOffsetDaytype;
            this.detail.dlvSettltHolidayConv = pSqlMaturityRule.MaturityPeriodicDlvSettltHolidayConv;
        }

        /// <summary>
        /// Retourne TRUE si au moins une des 4 dates relatives à la livraison est non renseignée
        /// </summary>
        /// <returns></returns>
        public bool IsExistDateEmpty()
        {
            return (DtFunc.IsDateTimeEmpty(this.dates.dtFirstDelivery) || DtFunc.IsDateTimeEmpty(this.dates.dtLastDelivery) ||
                    DtFunc.IsDateTimeEmpty(this.dates.dtFirstDlvSettlt) || DtFunc.IsDateTimeEmpty(this.dates.dtLastDlvSettlt));
        }
    }

    /// <summary>
    /// Paramétrage relatif à une livraison physique périodique (ex. PEGAS)
    /// </summary>
    public class MaturityPeriodicDeliveryDetail
    {
        //Fréquence de livraison
        public Nullable<int> deliveryDateMultiplier;
        public string deliveryDatePeriod;
        public string deliveryDateDaytype;
        public string deliveryDateRollConv;
        //Plage de livraison
        public string deliveryDateTimeStart;
        public string deliveryDateTimeEnd;
        public string deliveryDateTimeZone;
        public bool deliveryDateApplySummerTime;
        //Offset pour les paiements relatifs
        public Nullable<int> dlvSettltDateOffsetMultiplier;
        public string dlvSettltDateOffsetPeriod;
        public string dvlSettltDateOffsetDaytype;
        public string dlvSettltHolidayConv;
    }


    /// <summary>
    /// Dates relatives à une livraison physique périodique (ex. PEGAS)
    /// </summary>
    public class MaturityPeriodicDeliveryDates
    {
        //Première date de livraison et paiement relatif
        public DateTime dtFirstDelivery = DateTime.MinValue;
        public DateTime dtLastDelivery = DateTime.MinValue;
        //Dernière date de livraison et paiement relatif
        public DateTime dtFirstDlvSettlt = DateTime.MinValue;
        public DateTime dtLastDlvSettlt = DateTime.MinValue;

        public string dtFormat = "yyyy/MM/dd";
        public string FirstDelivery
        {
            get { return DtFunc.IsDateTimeEmpty(dtFirstDelivery) ? "n/a" : dtFirstDelivery.DayOfWeek.ToString() + ' ' + dtFirstDelivery.Date.ToString(dtFormat); }
        }
        public string LastDelivery
        {
            get { return DtFunc.IsDateTimeEmpty(dtLastDelivery) ? "n/a" : dtLastDelivery.DayOfWeek.ToString() + ' ' + dtLastDelivery.Date.ToString(dtFormat); }
        }
        public string FirstDlvSettlt
        {
            get { return DtFunc.IsDateTimeEmpty(dtFirstDlvSettlt) ? "n/a" : dtFirstDlvSettlt.DayOfWeek.ToString() + ' ' + dtFirstDlvSettlt.Date.ToString(dtFormat); }
        }
        public string LastDlvSettlt
        {
            get { return DtFunc.IsDateTimeEmpty(dtLastDlvSettlt) ? "n/a" : dtLastDlvSettlt.DayOfWeek.ToString() + ' ' + dtLastDlvSettlt.Date.ToString(dtFormat); }
        }
    }

    /// <summary>
    /// Dates Notice
    /// </summary>
    public class MaturityNoticeDays
    {
        public DateTime dtFirstNoticeDay = DateTime.MinValue;
        public DateTime dtLastNoticeDay = DateTime.MinValue;
    }

}

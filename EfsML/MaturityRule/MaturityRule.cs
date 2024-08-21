using EFS.ACommon;
using EFS.Common;
using FpML.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfsML
{

    /// <summary>
    ///  Classe "équivalente" à SQL_MaturityRuleRead (qui n'est qu'en lecture seule)  
    ///  <para>cette classe contient les propriétés utilisées pour déterminer les date d'expiration/LTD etc..</para>
    /// </summary>
    /// FI 20260125 [XXXXX] add
    public class MaturityRule
    {
        /// <summary>
        /// 
        /// </summary>
        public MaturityRule()
        { }
        /// <summary>
        ///  Initialisation à partir de la MaturityRule <paramref name="pSqlMaturityRule"/>
        /// </summary>
        /// <param name="pSqlMaturityRule"></param>
        public MaturityRule(SQL_MaturityRuleBase pSqlMaturityRule)
        {

            MaturityFormatEnum = pSqlMaturityRule.MaturityFormatEnum;
            MaturityRelativeTo = pSqlMaturityRule.MaturityRelativeTo;

            MaturityMonthReference = pSqlMaturityRule.MaturityMonthReference;
            MaturityRollConv = pSqlMaturityRule.MaturityRollConv;
            IsApplyMaturityBusinessCenterOnOffsetCalculation = pSqlMaturityRule.IsApplyMaturityBusinessCenterOnOffsetCalculation;
            MaturityBusinessCenter = pSqlMaturityRule.MaturityBusinessCenter;
            MaturityRollConvBusinessDayConv = pSqlMaturityRule.MaturityRollConvBusinessDayConv;
            MaturityOffsetMultiplier = pSqlMaturityRule.MaturityOffsetMultiplier;
            MaturityOffsetPeriod = pSqlMaturityRule.MaturityOffsetPeriod;
            MaturityOffsetDaytype = pSqlMaturityRule.MaturityOffsetDaytype;
            MaturityBusinessDayConv = pSqlMaturityRule.MaturityBusinessDayConv;

            IsExistLastTrdDayOffset = pSqlMaturityRule.IsExistLastTrdDayOffset;
            MaturityLastTrdDayOffsetMultiplier = pSqlMaturityRule.MaturityLastTrdDayOffsetMultiplier;
            MaturityLastTrdDayOffsetPeriod = pSqlMaturityRule.MaturityLastTrdDayOffsetPeriod;
            MaturityLastTrdDayOffsetDaytype = pSqlMaturityRule.MaturityLastTrdDayOffsetDaytype;

            MaturityDelivryDateOffsetPeriod = pSqlMaturityRule.MaturityDelivryDateOffsetPeriod;
            MaturityDelivryDateOffsetDaytype = pSqlMaturityRule.MaturityDelivryDateOffsetDaytype;
            MaturityDelivryDateOffsetMultiplier = pSqlMaturityRule.MaturityDelivryDateOffsetMultiplier;


            MaturityPeriodicFirstDelivryDateRollConv = pSqlMaturityRule.MaturityPeriodicFirstDelivryDateRollConv;
            MaturityPeriodicLastDelivryDateRollConv = pSqlMaturityRule.MaturityPeriodicLastDelivryDateRollConv;
            MaturityPeriodicDlvSettltDateOffsetPeriod = pSqlMaturityRule.MaturityPeriodicDlvSettltDateOffsetPeriod;
            MaturityPeriodicDlvSettltDateOffsetDaytype = pSqlMaturityRule.MaturityPeriodicDlvSettltDateOffsetDaytype;
            MaturityPeriodicDlvSettltDateOffsetMultiplier = pSqlMaturityRule.MaturityPeriodicDlvSettltDateOffsetMultiplier;
            MaturityPeriodicDlvSettltHolidayConv = pSqlMaturityRule.MaturityPeriodicDlvSettltHolidayConv;

        }

        public Cst.MaturityMonthYearFmtEnum MaturityFormatEnum { get; set; }
        public Nullable<Cst.MaturityRelativeTo> MaturityRelativeTo { get; set; }

        public string MaturityMonthReference { get; set; }
        public string MaturityRollConv { get; set; }
        public bool IsApplyMaturityBusinessCenterOnOffsetCalculation { get; set; }
        public string MaturityBusinessCenter { get; set; }
        public BusinessDayConventionEnum MaturityRollConvBusinessDayConv { get; set; }
        public Nullable<int> MaturityOffsetMultiplier { get; set; }
        public string MaturityOffsetPeriod { get; set; }
        public string MaturityOffsetDaytype { get; set; }
        public string MaturityBusinessDayConv { get; set; }
        public int? MaturityLastTrdDayOffsetMultiplier { get; set; }
        public string MaturityLastTrdDayOffsetPeriod { get; set; }
        public string MaturityLastTrdDayOffsetDaytype { get; set; }
        public bool IsExistLastTrdDayOffset { get; set; }
        public string MaturityDelivryDateOffsetPeriod { get; set; }
        public string MaturityDelivryDateOffsetDaytype { get; set; }
        public int? MaturityDelivryDateOffsetMultiplier { get; set; }
        public string MaturityPeriodicFirstDelivryDateRollConv { get; set; }
        public string MaturityPeriodicLastDelivryDateRollConv { get; set; }
        public string MaturityPeriodicDlvSettltDateOffsetPeriod { get; set; }
        public string MaturityPeriodicDlvSettltDateOffsetDaytype { get; set; }
        public int? MaturityPeriodicDlvSettltDateOffsetMultiplier { get; set; }
        public string MaturityPeriodicDlvSettltHolidayConv { get; set; }
    }
}

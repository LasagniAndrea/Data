using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using FpML.Interface;
using FpML.Enum;


namespace EfsML
{
    public static class MaturityHelper

    {
        /// <summary>
        /// Retourne l'échéance <paramref name="maturityFIX"/> (supposée être au format FIX) dans le format d'affichage <paramref name="maturityFormatOutput"/>
        /// <para>Il y a formatage uniquement sur les éhéance au format YearMonthOnly</para>
        /// </summary>
        /// <param name="maturityFIX">Représente l'échéance au format FIX</param>
        /// <param name="maturityFormatOutput">Représente le Format d'affichage attendu</param>
        /// <returns></returns>
        /// FI 20220601 [XXXXX] Nouvelle version de FormattingETDMaturity
        public static string FormatMaturityFIX(string maturityFIX, Cst.ETDMaturityInputFormatEnum maturityFormatOutput)
        {
            string returnValue = maturityFIX;

            bool isOk = IsMaturityInputFormatForMonthOnly(maturityFormatOutput) && IsInputInFixFormat(maturityFIX, Cst.MaturityMonthYearFmtEnum.YearMonthOnly);
            if (isOk)
            {
                RegExAttribute regExValue = ReflectionTools.GetAttribute<RegExAttribute>(typeof(Cst.MaturityMonthYearFmtEnum), Cst.MaturityMonthYearFmtEnum.YearMonthOnly.ToString());
                MatchCollection matchCol = new Regex(regExValue.RegExPattern).Matches(maturityFIX);
                string year = matchCol[0].Groups[1].Value;
                string month = matchCol[0].Groups[2].Value;
                string dtSep = DtFunc.CurrentCultureDateSeparator;

                switch (maturityFormatOutput)
                {
                    case Cst.ETDMaturityInputFormatEnum.MMMspaceYY:
                        string currentCulture_shortMonth = DtFunc.CurrentCultureFmtMonthMMM(Convert.ToInt32(month));
                        returnValue = StrFunc.FirstUpperCase(currentCulture_shortMonth) + " " + year;
                        break;
                    case Cst.ETDMaturityInputFormatEnum.MY:
                        string monthLetterList = "FGHJKMNQUVXZ";
                        returnValue = monthLetterList.Substring(Convert.ToInt32(month) - 1, 1) + maturityFIX.Substring(3, 1);
                        break;
                    case Cst.ETDMaturityInputFormatEnum.MMseparatorYYYY:
                        returnValue = month + dtSep + year;
                        break;
                    case Cst.ETDMaturityInputFormatEnum.YYYYseparatorMM:
                        returnValue = year + dtSep + month;
                        break;
                    default:
                        throw new NotImplementedException($"{maturityFormatOutput} in not implemented");
                }
            }
            return returnValue;
        }


        /// <summary>
        /// Retourne true si <paramref name="input"/> est dans un format FIX.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format">Retourne le format fix qui matche</param>
        /// <returns></returns>
        /// FI 20220601 [XXXXX] Add Method
        public static Boolean IsInputInFixFormat(string input, out Cst.MaturityMonthYearFmtEnum format)
        {
            format = default;

            List<Cst.MaturityMonthYearFmtEnum> lstMaturityMonthYearFmt = ReflectionTools.GetEnumValues<Cst.MaturityMonthYearFmtEnum, RegExAttribute>();
            if (null == lstMaturityMonthYearFmt)
                throw new InvalidOperationException("RegExAttribute missing on Cst.MaturityMonthYearFmtEnum");

            Boolean ret = lstMaturityMonthYearFmt.Where(x => IsInputInFixFormat(input, x)).Count() > 0;
            if (ret)
                format = lstMaturityMonthYearFmt.Where(x => IsInputInFixFormat(input, x)).First<Cst.MaturityMonthYearFmtEnum>();

            return ret;
        }

        /// <summary>
        /// Retourne true si l'échéance <paramref name="input"/> est au format <paramref name="format"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// FI 20220601 [XXXXX] Add Method
        public static Boolean IsInputInFixFormat(string input, Cst.MaturityMonthYearFmtEnum format)
        {
            RegExAttribute regExValue = ReflectionTools.GetAttribute<RegExAttribute>(typeof(Cst.MaturityMonthYearFmtEnum), format.ToString());
            Regex regex = new Regex(regExValue.RegExPattern);
            Boolean ret = regex.IsMatch(input);
            if (ret)
            {
                MatchCollection matchCol = regex.Matches(input);
                string year = matchCol[0].Groups[1].Value;
                string month = matchCol[0].Groups[2].Value;
                string day;
                string week;

                switch (format)
                {
                    case Cst.MaturityMonthYearFmtEnum.YearMonthOnly:
                    case Cst.MaturityMonthYearFmtEnum.YearMonthDay:
                        if (format == Cst.MaturityMonthYearFmtEnum.YearMonthOnly)
                            day = "01";
                        else
                            day = matchCol[0].Groups[3].Value;
                        ret = StrFunc.IsDate(year + month + day, DtFunc.FmtDateyyyyMMdd);
                        break;
                    case Cst.MaturityMonthYearFmtEnum.YearMonthWeek:
                        week = matchCol[0].Groups[3].Value;
                        day = StrFunc.GetDayDD(DtFunc.GetDayOfWeek(Convert.ToInt32(week)).ToString());
                        ret = StrFunc.IsDate(year + month + day, DtFunc.FmtDateyyyyMMdd);
                        break;
                    default:
                        throw new NotSupportedException($"{format} is not supported");
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si <paramref name="format"/> s'applique aux échéances mensuelles (YYYYMM)
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        /// FI 20220601 [XXXXX] Add Method
        public static Boolean IsMaturityInputFormatForMonthOnly(Cst.ETDMaturityInputFormatEnum format)
        {
            Cst.ETDMaturityInputFormatAttribute attrib = ReflectionTools.GetAttribute<Cst.ETDMaturityInputFormatAttribute>(typeof(Cst.ETDMaturityInputFormatEnum), format.ToString());
            return attrib.IsForMonthOnly;
        }
    }
}


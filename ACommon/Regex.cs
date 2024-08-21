using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;

namespace EFS.ACommon
{
    #region public class EFSRegex
    // EG 20170918 [22374] Add RegexLongTimeOffset
    public sealed class EFSRegex
    {
        #region Enums  Regular Expression Type
        public enum TypeRegex
        {
            None = 0,
            RegexShortTime = 10,
            RegexLongTime = 11,
            RegexLongTimeOffset = 12,
            //
            RegexDate = 20,
            RegexDateTime = 21,
            RegexMonthYear = 22,
            RegexDateRelative = 23,
            RegexDateRelativeExtend = 24,
            RegexDateRelativeOffset = 25,
            //
            RegexInteger = 30,
            RegexNegativeInteger = 31,
            RegexPositiveInteger = 32,
            RegexNonNegativeInteger = 33,
            //
            RegexDecimal = 40,
            RegexDecimalExtend = 41,

            RegexAmount = 42,
            RegexAmountExtend = 43,
            RegexAmountSigned = 44,
            RegexAmountSignedExtend = 45,
            //
            RegexFixedRate = 50,
            RegexFixedRateExtend = 51,
            RegexFixedRatePercent = 52,
            //
            RegexPercent = 60,
            RegexPercentExtend = 61,
            RegexPercentFraction = 62,
            //
            RegexFxRate = 70,
            RegexFxRateExtend = 71,
            //
            RegexString = 80,
            RegexStringAlphaNumUpper = 81,
            RegexStringAlphaNum = 82,
            //
            RegexRate = 90, // FixedRate Or  FloatingRate
            //
            RegexCFIIdentifier = 100,

        };
        #endregion

        #region RegularExpression
        // EG 20170918 [22374] Add RegexLongTimeOffset
        public static string RegularExpression(TypeRegex pTypeRegex)
        {
            DateTimeFormatInfo dtfi = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            string decimalSeparator;
            string groupSeparator;
            string pattern;

            string ret;
            switch (pTypeRegex)
            {
                case TypeRegex.RegexShortTime:
                case TypeRegex.RegexLongTime:
                    if (TypeRegex.RegexShortTime == pTypeRegex)
                        pattern = dtfi.ShortTimePattern;
                    else
                        pattern = dtfi.LongTimePattern;
                    //ret = Ressource.GetString("RegexTime_" + pattern.Replace(dtfi.TimeSeparator, string.Empty));
                    ret = Ressource.GetString2("RegexTime_" + pattern.Replace(dtfi.TimeSeparator, string.Empty));
                    //20110614 PL Ligne suivante devenue A PRIORI inutile (cf ci-dessous RegexDate)
                    //ret = ret.Replace("(:)", dtfi.TimeSeparator);
                    break;
                case TypeRegex.RegexLongTimeOffset:
                    ret = Ressource.GetString2(TypeRegex.RegexLongTimeOffset.ToString());
                    break;
                case TypeRegex.RegexDate:
                case TypeRegex.RegexDateTime:
                    pattern = dtfi.ShortDatePattern;
                    //ret = Ressource.GetString(pTypeRegex.ToString() + "_" + pattern.Replace(dtfi.DateSeparator, string.Empty));
                    ret = Ressource.GetString2(pTypeRegex.ToString() + "_" + pattern.Replace(dtfi.DateSeparator, string.Empty));
                    //20060202 PL Ligne suivante devenue inutile avec les nouvelles RegEx
                    //ret     = ret.Replace("(:)", dtfi.TimeSeparator);
                    break;

                case TypeRegex.RegexDateRelativeOffset:
                    //ret = Ressource.GetString(TypeRegex.RegexDateRelativeOffset.ToString());
                    ret = Ressource.GetString2(TypeRegex.RegexDateRelativeOffset.ToString());
                    break;

                case TypeRegex.RegexDateRelative:
                case TypeRegex.RegexDateRelativeExtend:
                    //ret = Ressource.GetString(TypeRegex.RegexDateRelative.ToString());
                    ret = Ressource.GetString2(TypeRegex.RegexDateRelative.ToString());
                    if (TypeRegex.RegexDateRelativeExtend == pTypeRegex)
                        ret = ret + "|" + RegularExpression(TypeRegex.RegexDate);
                    break;

                case TypeRegex.RegexMonthYear:
                    //ret = Ressource.GetString(TypeRegex.RegexMonthYear.ToString());
                    ret = Ressource.GetString2(TypeRegex.RegexMonthYear.ToString());
                    break;
                //
                case TypeRegex.RegexInteger:
                case TypeRegex.RegexPositiveInteger:
                case TypeRegex.RegexNegativeInteger:
                case TypeRegex.RegexNonNegativeInteger:
                    groupSeparator = GetPatternGroupSeparator();
                    ret = Ressource.GetString2(pTypeRegex.ToString(), groupSeparator);
                    break;
                //
                case TypeRegex.RegexDecimal:
                case TypeRegex.RegexDecimalExtend:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    groupSeparator = GetPatternGroupSeparator();
                    ret = Ressource.GetString2(TypeRegex.RegexDecimal.ToString(), groupSeparator, decimalSeparator);
                    break;

                case TypeRegex.RegexAmount:
                case TypeRegex.RegexAmountExtend:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    groupSeparator = GetPatternGroupSeparator();
                    ret = Ressource.GetString2(TypeRegex.RegexAmount.ToString(), groupSeparator, decimalSeparator);
                    break;
                case TypeRegex.RegexAmountSigned:
                case TypeRegex.RegexAmountSignedExtend:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    groupSeparator = GetPatternGroupSeparator();
                    ret = Ressource.GetString2(TypeRegex.RegexAmountSigned.ToString(), groupSeparator, decimalSeparator);
                    break;

                case TypeRegex.RegexFixedRate:
                case TypeRegex.RegexFixedRateExtend:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    ret = Ressource.GetString2(TypeRegex.RegexFixedRate.ToString(), decimalSeparator);
                    break;

                case TypeRegex.RegexFixedRatePercent:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    ret = Ressource.GetString2(pTypeRegex.ToString(), decimalSeparator);
                    break;

                case TypeRegex.RegexPercent:
                case TypeRegex.RegexPercentExtend:
                case TypeRegex.RegexPercentFraction:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    ret = Ressource.GetString2(pTypeRegex.ToString(), decimalSeparator);
                    break;

                case TypeRegex.RegexFxRate:
                case TypeRegex.RegexFxRateExtend:
                    decimalSeparator = GetPatternDecimalSeparator(pTypeRegex);
                    groupSeparator = GetPatternGroupSeparator();
                    ret = Ressource.GetString2(TypeRegex.RegexFxRate.ToString(), groupSeparator, decimalSeparator);
                    break;
                //
                case TypeRegex.RegexString:
                case TypeRegex.RegexRate:
                default:
                    //ret = Ressource.GetString(pTypeRegex.ToString());
                    ret = Ressource.GetString2(pTypeRegex.ToString());
                    break;
            }
            return ret;
        }
        #endregion
        #region ErrorMessage
        public static string ErrorMessage(TypeRegex pTypeRegEx)
        {

            DateTimeFormatInfo dtfi = CultureInfo.CurrentUICulture.DateTimeFormat;
            NumberFormatInfo nbfi = CultureInfo.CurrentUICulture.NumberFormat;
            string ret = string.Empty;
            string pattern;
            string res;
            switch (pTypeRegEx)
            {
                case TypeRegex.RegexMonthYear:
                    ret = Ressource.GetString(pTypeRegEx.ToString() + "Error");
                    break;

                case TypeRegex.RegexDate:
                    pattern = dtfi.ShortDatePattern;
                    ret = Ressource.GetString2(pTypeRegEx.ToString() + "Error", pattern);
                    break;

                case TypeRegex.RegexDateTime:
                    pattern = dtfi.ShortDatePattern + " " + dtfi.ShortTimePattern;
                    ret = Ressource.GetString2(pTypeRegEx.ToString() + "Error", pattern);
                    break;

                case TypeRegex.RegexDateRelative:
                case TypeRegex.RegexDateRelativeExtend:
                case TypeRegex.RegexDateRelativeOffset:
                    ret = Ressource.GetString2("RegexDefaultError");
                    break;

                case TypeRegex.RegexShortTime:
                    pattern = dtfi.ShortTimePattern;
                    ret = Ressource.GetString2("RegexTimeError", pattern);
                    break;
                case TypeRegex.RegexLongTime:
                    pattern = dtfi.LongTimePattern;
                    ret = Ressource.GetString2("RegexTimeError", pattern);
                    break;
                case TypeRegex.RegexInteger:
                case TypeRegex.RegexPositiveInteger:
                case TypeRegex.RegexNegativeInteger:
                case TypeRegex.RegexNonNegativeInteger:
                    res = pTypeRegEx.ToString() + "Error";
                    ret = Ressource.GetString(res, nbfi.NumberGroupSeparator);
                    break;

                case TypeRegex.RegexDecimal:
                case TypeRegex.RegexDecimalExtend:
                    res = TypeRegex.RegexDecimal.ToString() + "Error";
                    ret = Ressource.GetString2(res, nbfi.NumberGroupSeparator, nbfi.NumberDecimalSeparator);
                    break;
                // Montant
                case TypeRegex.RegexAmount:
                case TypeRegex.RegexAmountExtend:
                case TypeRegex.RegexAmountSigned:
                case TypeRegex.RegexAmountSignedExtend:
                    res = TypeRegex.RegexAmount.ToString() + "Error";
                    ret = Ressource.GetString2(res, nbfi.NumberGroupSeparator, nbfi.NumberDecimalSeparator);
                    break;
                // Taux Fixe
                case TypeRegex.RegexFixedRate:
                case TypeRegex.RegexFixedRateExtend:
                case TypeRegex.RegexFixedRatePercent:
                    res = TypeRegex.RegexFixedRate.ToString() + "Error";
                    ret = Ressource.GetString2(res, nbfi.NumberDecimalSeparator);
                    break;
                // Cours
                case TypeRegex.RegexFxRate:
                case TypeRegex.RegexFxRateExtend:
                    res = TypeRegex.RegexFxRate.ToString() + "Error";
                    ret = Ressource.GetString2(res, nbfi.NumberDecimalSeparator);
                    break;
                // Fixed Rate  ou floationg Rate 
                case TypeRegex.RegexRate:
                    res = Ressource.GetString("RegexDefaultError");
                    ret = Ressource.GetString(res);
                    break;
                case TypeRegex.RegexStringAlphaNumUpper:
                case TypeRegex.RegexStringAlphaNum:
                    res = pTypeRegEx.ToString() + "Error";
                    ret = Ressource.GetString(res);
                    break;
                case TypeRegex.RegexString:
                    ret = Ressource.GetString("RegexDefaultError");
                    break;
                case TypeRegex.None:
                    break;
                default:
                    break;
            }
            return ret;
        }
        #endregion ErrorMessage
        #region GetPatternDecimalSeparator
        private static string GetPatternDecimalSeparator(TypeRegex pTypeRegEx)
        {
            NumberFormatInfo nbfi = Thread.CurrentThread.CurrentCulture.NumberFormat;
            string decimalSeparator;
            switch (pTypeRegEx)
            {
                // Toutes les RegEx Extend Acceptent le "." comme Séparateur dec en plus du seprateur de la currentCulture  
                case TypeRegex.RegexAmountExtend:
                case TypeRegex.RegexAmountSignedExtend:
                case TypeRegex.RegexDecimalExtend:
                case TypeRegex.RegexFixedRateExtend:
                case TypeRegex.RegexFixedRatePercent:
                case TypeRegex.RegexFxRateExtend:
                case TypeRegex.RegexPercent:
                case TypeRegex.RegexPercentFraction:
                    decimalSeparator = @"[\.\" + nbfi.NumberDecimalSeparator + "]"; // \x2C|\x2E
                    break;
                default:
                    decimalSeparator = @"\" + nbfi.NumberDecimalSeparator;
                    break;
            }
            //
            return decimalSeparator;
        }
        #endregion GetPatternDecimalSeparator
        #region GetPatternGroupSeparator
        private static string GetPatternGroupSeparator()
        {
            NumberFormatInfo nbfi = Thread.CurrentThread.CurrentCulture.NumberFormat;
            string ret;
            if (Cst.IsSpaceCultureSeparator(nbfi.NumberGroupSeparator))
            {
                ret = @"[" + Cst.NonBreakSpace + @"\s]";
            }
            else
            {
                //FI 20120131 [] il faut "\" comme ds la méthode GetPatternDecimalSeparator
                //C'est bizarre que l'one ne l'ai jamais vu avant
                //En It le séparateur de millier est le point ".", ce caractère doit être échappé dans les regex  
                //ret = nbfi.NumberGroupSeparator;
                ret = @"\" + nbfi.NumberGroupSeparator;
            }
            return ret;
        }
        #endregion GetPatternGroupSeparator
        #region IsMatch
        public static bool IsMatch(string pData, TypeRegex pTypeRegEx)
        {
            return IsMatch(pData, pTypeRegEx, RegexOptions.IgnoreCase);
        }
        public static bool IsMatch(string pData, TypeRegex pTypeRegEx, RegexOptions pOptions)
        {
            string regularExpression = EFSRegex.RegularExpression(pTypeRegEx);
            Regex regex = new Regex(regularExpression, pOptions);
            bool isOk = StrFunc.IsFilled(pData) && regex.IsMatch(pData);
            return isOk;
        }

        #endregion IsMatch
        #region IsInteger
        public static bool IsInteger(TypeRegex pTypeRegEx)
        {
            return (pTypeRegEx == EFSRegex.TypeRegex.RegexInteger) ||
                 (pTypeRegEx == EFSRegex.TypeRegex.RegexNegativeInteger) ||
                 (pTypeRegEx == EFSRegex.TypeRegex.RegexPositiveInteger) ||
                 (pTypeRegEx == EFSRegex.TypeRegex.RegexNonNegativeInteger);
        }
        #endregion IsInteger
        #region IsNumber
        public static bool IsNumber(TypeRegex pTypeRegEx)
        {
            return ((pTypeRegEx == EFSRegex.TypeRegex.RegexDecimal) || (pTypeRegEx == EFSRegex.TypeRegex.RegexDecimalExtend) ||
                (pTypeRegEx == EFSRegex.TypeRegex.RegexFxRate) || (pTypeRegEx == EFSRegex.TypeRegex.RegexFxRateExtend) ||
                (pTypeRegEx == EFSRegex.TypeRegex.RegexAmount) || (pTypeRegEx == EFSRegex.TypeRegex.RegexAmountExtend) ||
                IsInteger(pTypeRegEx) ||
                (pTypeRegEx == EFSRegex.TypeRegex.RegexFixedRatePercent));
        }
        #endregion IsNumber
    }
    #endregion
}

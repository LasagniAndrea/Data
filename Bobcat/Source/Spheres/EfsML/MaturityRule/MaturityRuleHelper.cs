using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EfsML.Enum.Tools;
using FpML.Enum;

namespace EfsML
{
    class MaturityRuleHelper
    {
        /// <summary>
        /// Retourne true si les propriétés ROLLCONVMMY et MONTHREF d'une maturityRule implique un changement de mois lors du calcul de la date d'échéance
        /// </summary>
        /// <param name="maturityRule"></param>
        /// <returns></returns>
        /// FI 20230615 [26398] Add
        public static Boolean IsMonthOffset((string MaturityRollConv, string MaturityMonthReference) maturityRule)
        {
            Boolean ret = false;
            if ((StringToEnum.RollConvention(maturityRule.MaturityRollConv) == RollConventionEnum.CBOTAGRIOPT) ||
                (StringToEnum.RollConvention(maturityRule.MaturityRollConv) == RollConventionEnum.EUREXFIXEDINCOMEOPT))
            {
                ret = true;
            }
            else if (!String.IsNullOrEmpty(maturityRule.MaturityMonthReference) && StrFunc.IsFilled(maturityRule.MaturityMonthReference.Replace("MaturityMonth", string.Empty)))
            {
                ret = true;
            }
            return ret;
        }

        /// <summary>
        ///  Applique à <paramref name="dt"/>, et de manière inverse, le changement de mois défini sur les propriétés ROLLCONVMMY et MONTHREF d'une maturityRule  
        ///  <para>Remarque retourne <paramref name="dt"/> s'il n'y a pas de changement de mois </para>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="maturityRule"></param>
        /// <returns></returns>
        /// FI 20230615 [26398] Add
        public static DateTime ApplyMonthOffsetReverse(DateTime dt, (string MaturityRollConv, string MaturityMonthReference) maturityRule)
        {
            DateTime ret = dt;

            if (IsMonthOffset(maturityRule))
            {
                // FI 20220909 [XXXXX] MR atypique liée aux Options Agriculture sur CBOT on considère comme mois de départ le mois suivant de celui saisi, puisque cette règle, en dur, considère le mois précédent dans son calcul.
                // FI 20220909 [26113] MR atypique EUREXFIXEDINCOMEOPT on considère comme mois de départ le mois suivant de celui saisi, puisque cette règle, en dur, considère le mois précédent dans son calcul.
                if ((StringToEnum.RollConvention(maturityRule.MaturityRollConv) == RollConventionEnum.CBOTAGRIOPT) ||
                    (StringToEnum.RollConvention(maturityRule.MaturityRollConv) == RollConventionEnum.EUREXFIXEDINCOMEOPT))
                {
                    ret = ret.AddMonths(1);
                }
                else if (!String.IsNullOrEmpty(maturityRule.MaturityMonthReference) && StrFunc.IsFilled(maturityRule.MaturityMonthReference.Replace("MaturityMonth", string.Empty)))
                {
                    // FI 20230113 [XXXXX] Gestion de l'éventuel décalage présent sous MONTHREF 
                    //Si MR avec Third Friday et MONTHREF = "MaturityMonth+1M", l'échéance 202102 (au format YYYYMM) donne 19/03/2021
                    //
                    //ici le format est YYYYMMDD, l'utilsateur a normalement saisit 20210319 pour cette même échéance 
                    //En conséquence pour valider 20210319 Spheres ne consdère pas 202103 mais 202102 ce qui permet d'obtenir la date de maturité 19/03/2021 (équivalent à l'échéance 202102) 
                    string maturityMonthReferenceReverse;
                    if (maturityRule.MaturityMonthReference.Contains("+"))
                        maturityMonthReferenceReverse = maturityRule.MaturityMonthReference.Replace("+", "-");
                    else if (maturityRule.MaturityMonthReference.Contains("-"))
                        maturityMonthReferenceReverse = maturityRule.MaturityMonthReference.Replace("-", "+");
                    else
                        throw new InvalidOperationException($"MaturityMonthReference:{maturityRule.MaturityMonthReference} is invalid");

                    ret = MaturityRuleHelper.ApplyMonthReference(ret, maturityMonthReferenceReverse);
                }
                else
                {
                    throw new NotSupportedException($"{maturityRule} not supported");
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="maturityMonthReference"></param>
        /// <returns></returns>
        /// FI 20230113 [XXXXX] Add Method
        public static DateTime ApplyMonthReference(DateTime dt, string maturityMonthReference)
        {
            string monthRef = maturityMonthReference.Replace("MaturityMonth", string.Empty);
            if (!String.IsNullOrEmpty(monthRef))
            {
                string monthRef_Unit = monthRef.Substring(monthRef.Length - 1, 1);
                string monthRef_Offset = monthRef.Substring(0, monthRef.Length - 1);
                if ((monthRef_Offset.StartsWith("+") || monthRef_Offset.StartsWith("-"))
                    && System.Enum.IsDefined(typeof(PeriodEnum), monthRef_Unit)
                    && IntFunc.IsPositiveInteger(monthRef_Offset.Substring(1)))
                {
                    PeriodEnum monthRef_UnitEnum = (PeriodEnum)System.Enum.Parse(typeof(PeriodEnum), monthRef_Unit);
                    switch (monthRef_UnitEnum)
                    {
                        case PeriodEnum.D:
                            dt = dt.AddDays(Convert.ToDouble(monthRef_Offset));
                            break;
                        case PeriodEnum.M:
                            dt = dt.AddMonths(Convert.ToInt32(monthRef_Offset));
                            break;
                        case PeriodEnum.Y:
                            dt = dt.AddYears(Convert.ToInt32(monthRef_Offset));
                            break;
                    }
                }
            }

            return dt;
        }

    }
}

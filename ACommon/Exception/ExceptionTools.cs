using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace EFS.ACommon
{


    public sealed class ExceptionTools
    {

        /// <summary>
        ///  Rerourne true si l'exception provient de la base de donnéee 
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        public static bool IsRDBMSException(Exception pEx)
        {
            bool ret;

            // FI 20131210 [19337] usage de type
            //if (StrFunc.IsFilled(pEx.Source))
            //    ret = pEx.Source.ToLower().Contains("data provider");//PL Voir message d'erreur avec Provider Oracle
            string sType = pEx.GetType().ToString();
            ret = (sType.Contains("SqlException") || sType.Contains("OracleException"));

            return ret;
        }

        /// <summary>
        /// Retourne la méthode qui a levé l'exception
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        public static string GetTargetException(Exception pEx)
        {
            string ret = string.Empty;
            //
            if (null != pEx.TargetSite)
            {
                //20110411 FI  Ajout de l'information ReflectedType
                if (null != pEx.TargetSite.ReflectedType)
                    ret += pEx.TargetSite.ReflectedType.FullName + ".";
                ret += pEx.TargetSite.Name.ToString();
                ret += Cst.Space + "(" + pEx.TargetSite.MemberType.ToString() + ")";
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne tous les messagess d'une Exception, en incluant les InnerException
        /// <para>Chaque message est séparé par 1 CRLF</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        /// FI 20120706 Correction car la méthode est mal écrite 
        public static string GetMessageExtended(Exception pEx)
        {
            // FI 20200910 [XXXXX] si AggregateException l'exception est aplanie
            Exception ex = pEx.GetType().Equals(typeof(AggregateException)) ? (pEx as AggregateException).Flatten() : pEx;

            string ret = ex.Message;
            //FI 20120706 si != et non pas si ==
            if (null != ex.InnerException)
            {
                //FI 20120706 ret +
                //ret = Cst.CrLf + GetMessageExtended(pEx.InnerException);
                ret += Cst.CrLf + GetMessageExtended(ex.InnerException);
            }

            return ret;
        }
        /// <summary>
        /// Retourne toutes les piles des appels d'une Exception, en incluant les InnerException
        /// <para>Chaque pile est séparée par 2 CRLF</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        /// FI 20180320 [XXXXX] Add
        public static string GetStackExtended(Exception pEx)
        {
            // FI 20200910 [XXXXX] si AggregateException l'exception est aplanie
            Exception ex = pEx.GetType().Equals(typeof(AggregateException)) ? (pEx as AggregateException).Flatten() : pEx;
            Boolean isExistStackTrace = StrFunc.IsFilled(ex.StackTrace);

            string ret = string.Empty;
            if (isExistStackTrace)
                ret += ex.StackTrace;

            if (null != ex.InnerException)
            {
                if (isExistStackTrace)
                    ret += Cst.CrLf2;
                ret += GetStackExtended(ex.InnerException);
            }

            return ret;
        }

        /// <summary>
        /// Retourne tous les messages d'une exception (InnerException inclues), puis la pile des appels (InnerException inclues)
        /// <para>Les messages et la pile des appels sont séparés par 2 CRLF</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        /// FI 20190724 [XXXXX] Add 
        public static string GetMessageAndStackExtended(Exception pEx)
        {
            // FI 20200910 [XXXXX] Ajout des titres "-- Message(s) --" et de "-- StackTrace(s) --" 
            string ret = $"-- Message(s) --{Cst.CrLf}";
            ret += ExceptionTools.GetMessageExtended(pEx);

            string stackTrace = ExceptionTools.GetStackExtended(pEx);
            if (StrFunc.IsFilled(stackTrace))
            {
                ret += $"{Cst.CrLf}-- StackTrace(s) --{Cst.CrLf}";
                ret += ExceptionTools.GetStackExtended(pEx);
            }
            return ret;
        }

        /// <summary>
        /// Récupère l'exception initiale issue de la base de donnée
        /// <para>Recupère null si l'exception {pEx} ne parvient pas de la base de donnée</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        /// FI 20131213 [19337] add method
        public static Exception GetFirstRDBMSException(Exception pEx)
        {
            Exception ret = null;

            bool isOk = IsRDBMSException(pEx);
            if (isOk)
            {
                ret = pEx;
            }
            else
            {
                Exception exCurrent = pEx;
                bool isInnerException = (null != exCurrent.InnerException);

                while (isInnerException)
                {
                    isOk = ExceptionTools.IsRDBMSException(exCurrent.InnerException);
                    if (isOk)
                    {
                        ret = exCurrent.InnerException;
                        break;
                    }
                    else
                    {
                        exCurrent = exCurrent.InnerException;
                        isInnerException = (null != exCurrent.InnerException);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Récupère l'exception d'origine de type {pExType}
        /// <para>Recupère null si l'exception {pEx} n'a pas comme origine une exception de type {pExType}</para>
        /// </summary>
        /// <param name="pEx"></param>
        /// <param name="pExType"></param>
        /// <returns></returns>
        /// RD 20150709 [21056] add method        
        public static Exception GetFirstException(Exception pEx, Type pExType)
        {
            Exception ret = null;

            bool isOk = pEx.GetType().Equals(pExType);
            if (isOk)
            {
                ret = pEx;
            }
            else
            {
                Exception exCurrent = pEx;
                bool isInnerException = (null != exCurrent.InnerException);

                while (isInnerException)
                {
                    isOk = exCurrent.InnerException.GetType().Equals(pExType);
                    if (isOk)
                    {
                        ret = exCurrent.InnerException;
                        break;
                    }
                    else
                    {
                        exCurrent = exCurrent.InnerException;
                        isInnerException = (null != exCurrent.InnerException);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///  Rerourne true si l'exception provient de CSharp
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        // FI 20200612 Add Method (Déplacé ici, anciennement dans SpheresExceptionParser)
        public static bool IsCSharpException(Exception pEx)
        {
            bool ret = false;

            if (StrFunc.IsFilled(pEx.Message))
                ret = pEx.Message.ToLower().Contains("object reference not set");

            return ret;
        }


        /// <summary>
        /// Récupère l'exception initiale issue de CSharp
        /// <para>Recupère null si l'exception {pEx} n'est pas issue de CSharp
        /// </summary>
        /// <param name="pEx"></param>
        /// <returns></returns>
        /// FI 20200623 [XXXXX] Add
        public static Exception GetFirtsCSharpException(Exception pEx)
        {
            Exception ret = null;

            bool isOk = IsCSharpException(pEx);
            if (isOk)
            {
                ret = pEx;
            }
            else
            {
                Exception exCurrent = pEx;
                bool isInnerException = (null != exCurrent.InnerException);

                while (isInnerException)
                {
                    isOk = ExceptionTools.IsCSharpException(exCurrent.InnerException);
                    if (isOk)
                    {
                        ret = exCurrent.InnerException;
                        break;
                    }
                    else
                    {
                        exCurrent = exCurrent.InnerException;
                        isInnerException = (null != exCurrent.InnerException);
                    }
                }
            }
            return ret;
        }
    }
}

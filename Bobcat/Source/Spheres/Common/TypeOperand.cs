using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;

namespace EFS.Common
{
    /// EG 20161122 New Version BootStrap
    public enum OperandEnum
    {
        [OperandAttribute(All = "=")]
        @checked,
        [OperandAttribute(All = "=")]
        @unchecked,
        [OperandAttribute(All = "=")]
        equalto,
        [OperandAttribute(All = "<>")]
        notequalto,
        [OperandAttribute(All = ">")]
        greaterthan,
        [OperandAttribute(All = "<")]
        lessthan,
        [OperandAttribute(All = ">=")]
        greaterorequalto,
        [OperandAttribute(All = "<=")]
        lessorequalto,
        [OperandAttribute(Sql = "like", XQuery = "contains({0},\"{1}\")")]
        contains,
        [OperandAttribute(Sql = "not like", XQuery = "not(contains({0},\"{1}\"))")]
        notcontains,
        [OperandAttribute(Sql = "like", XQuery = "(substring({0}, 1, string-length(\"{1}\")) = \"{1}\")")]
        startswith,
        [OperandAttribute(Sql = "like", XQuery = "(substring({0}, string-length({0}) - string-length(\"{1}\") + 1, string-length(\"{1}\")) = \"{1}\")")]
        endswith,
        [OperandAttribute(Sql = "like", XQuery = "contains({0},\"{1}\")")]
        like,
        [OperandAttribute(Sql = "not like", XQuery = "(substring({0}, 1, string-length(\"{1}\")) = \"{1}\")")]
        notlike,
        unknown,
    }
    
    /// <summary>
    /// Description résumée de TypeOperand
    /// </summary>
    public sealed class TypeOperand
    {
        #region GetTypeOperandEnum
        public static OperandEnum GetTypeOperandEnum(string pOperand)
        {
            return GetTypeOperandEnum(pOperand, false);
        }
        public static OperandEnum GetTypeOperandEnum(string pOperand, bool pWithExceptionWhenError)
        {
            OperandEnum ret = OperandEnum.unknown;
            //
            if (System.Enum.IsDefined(typeof(OperandEnum), pOperand))
                ret = (OperandEnum)System.Enum.Parse(typeof(OperandEnum), pOperand, true);
            else if (System.Enum.IsDefined(typeof(OperandEnum), pOperand.ToLower()))
                ret = (OperandEnum)System.Enum.Parse(typeof(OperandEnum), pOperand, true);
            else
            {
                if (StrFunc.IsFilled(pOperand))
                {
                    string pOperandToLower = pOperand.ToLower();
                    if ("=" == pOperandToLower)
                        ret = OperandEnum.@equalto;
                    else if (">" == pOperandToLower)
                        ret = OperandEnum.greaterthan;
                    else if (">=" == pOperandToLower)
                        ret = OperandEnum.greaterorequalto;
                    else if ("<" == pOperandToLower)
                        ret = OperandEnum.lessthan;
                    else if ("<=" == pOperandToLower)
                        ret = OperandEnum.lessorequalto;
                    else if ("!=" == pOperandToLower)
                        ret = OperandEnum.notequalto;
                }
            }
            if ((ret == OperandEnum.unknown) && pWithExceptionWhenError)
                throw new Exception(StrFunc.AppendFormat("Operand {0} is unknown", pOperand));
            return ret;
        }

        public static string GetXQueryOperand(string pOperand, bool pWithExceptionWhenError)
        {
            return GetXQueryOperand(GetTypeOperandEnum(pOperand, pWithExceptionWhenError));
        }
        public static string GetXQueryOperand(OperandEnum pOperand)
        {
            string @value = pOperand.ToString();
            OperandAttribute attribute = ReflectionTools.GetAttribute<OperandAttribute>(pOperand);
            if (null != attribute)
                @value = StrFunc.Frame(StrFunc.IsFilled(attribute.All) ? attribute.All : attribute.XQuery, " ");
            return @value;
        }
        public static string GetSQLOperand(string pOperand, bool pWithExceptionWhenError)
        {
            return GetSQLOperand(GetTypeOperandEnum(pOperand, pWithExceptionWhenError));
        }
        public static string GetSQLOperand(OperandEnum pOperand)
        {
            string @value = pOperand.ToString();
            OperandAttribute attribute = ReflectionTools.GetAttribute<OperandAttribute>(pOperand);
            if (null != attribute)
                @value = StrFunc.Frame(StrFunc.IsFilled(attribute.All) ? attribute.All : attribute.Sql, " ");
            return @value;
        }

        
        public static bool IsParticularOperandLike(string pOperand, bool pWithExceptionWhenError)
        {
            return IsParticularOperandLike(GetTypeOperandEnum(pOperand, pWithExceptionWhenError));
        }
        public static bool IsParticularOperandLike(OperandEnum pOperand)
        {
            return (pOperand == OperandEnum.startswith) || (pOperand == OperandEnum.endswith) || (pOperand == OperandEnum.contains) || (pOperand == OperandEnum.notcontains);
        }
        public static bool IsExcludeOperand(string pOperand, bool pWithExceptionWhenError)
        {
            return IsExcludeOperand(GetTypeOperandEnum(pOperand, pWithExceptionWhenError));
        }
        public static bool IsExcludeOperand(OperandEnum pOperand)
        {
            return (pOperand == OperandEnum.notcontains) || (pOperand == OperandEnum.notequalto) || (pOperand == OperandEnum.notlike);
        }
        public static bool IsOperandEnabled(string pOperand, string pDataType, bool pWithExceptionWhenError)
        {
            return IsOperandEnabled(GetTypeOperandEnum(pOperand, pWithExceptionWhenError), pDataType);
        }
        public static bool IsOperandEnabled(OperandEnum pOperand, string pDataType)
        {
            bool isEnabled = true;
            if (StrFunc.IsFilled(pDataType))
            {
                switch (pOperand)
                {
                    case OperandEnum.@checked:
                    case OperandEnum.@unchecked:
                        isEnabled = TypeData.IsTypeBool(pDataType);
                        break;
                    case OperandEnum.contains:
                    case OperandEnum.notcontains:
                    case OperandEnum.startswith:
                    case OperandEnum.endswith:
                        isEnabled = TypeData.IsTypeString(pDataType) || TypeData.IsTypeText(pDataType);
                        break;
                    case OperandEnum.like:
                    case OperandEnum.notlike:
                        isEnabled = (false == TypeData.IsTypeDateOrDateTime(pDataType)) && (false == TypeData.IsTypeBool(pDataType));
                        break;
                    case OperandEnum.equalto:
                    case OperandEnum.notequalto:
                    case OperandEnum.lessthan:
                    case OperandEnum.greaterthan:
                    case OperandEnum.lessorequalto:
                    case OperandEnum.greaterorequalto:
                        isEnabled = (false == TypeData.IsTypeText(pDataType)) && (false == TypeData.IsTypeBool(pDataType));
                        break;
                }
            }
            return isEnabled;
        }
        #endregion GetTypeOperandEnum
    }
    
    /// <summary>
    /// Attributs associés à OperandEnum
    /// </summary>
    /// FI 20131109 Add classe
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class OperandAttribute : Attribute
    {
        #region Members
        private string _all;
        private string _sql;
        private string _xQuery;
        #endregion Members

        #region Accessors
        public string All
        {
            get { return _all; }
            set { _all = value; }
        }

        public string XQuery
        {
            get { return _xQuery; }
            set { _xQuery = value; }
        }
        public string Sql
        {
            get { return _sql; }
            set { _sql = value; }
        }
        #endregion Accessors
    }
}

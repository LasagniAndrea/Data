using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization; 

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Status;

using EfsML.Enum;
using EfsML.Enum.Tools;
using Tz = EFS.TimeZone;



namespace EFS.Common
{

    public class SQL_Criteria
    {
        #region Members
        private bool m_IsCaseInsensitive;
        private bool m_ColumnCriteriaSpecified;
        private SQL_ColumnCriteria[] m_ColumnCriteria;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("columnCriteria", typeof(SQL_ColumnCriteria))]
        public SQL_ColumnCriteria[] ColumnCriteria
        {
            get { return m_ColumnCriteria; }
            set { m_ColumnCriteria = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnCriteriaSpecified
        {
            get { return m_ColumnCriteriaSpecified; }
            set { m_ColumnCriteriaSpecified = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("isCaseInsensitive")]
        public bool IsCaseInsensitive
        {
            get { return m_IsCaseInsensitive; }
            set { m_IsCaseInsensitive = value; }
        }
        #endregion Accessors

        #region Constructors
        public SQL_Criteria() { }
        public SQL_Criteria(SQL_ColumnCriteria[] pColumnCriteria, bool pIsCaseInsensitive)
        {
            m_ColumnCriteriaSpecified = ArrFunc.IsFilled(pColumnCriteria);
            m_ColumnCriteria = pColumnCriteria;
            m_IsCaseInsensitive = pIsCaseInsensitive;
        }
        #endregion Constructors

        #region Add
        public void Add(SQL_ColumnCriteria pColumnCriteria)
        {
            Add(new SQL_ColumnCriteria[] { pColumnCriteria });
        }
        public void Add(SQL_ColumnCriteria[] pColumnCriteria)
        {
            ArrayList al = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(m_ColumnCriteria); i++)
                al.Add(m_ColumnCriteria[i]);
            for (int i = 0; i < ArrFunc.Count(pColumnCriteria); i++)
                al.Add(pColumnCriteria[i]);
            m_ColumnCriteria = (SQL_ColumnCriteria[])al.ToArray(typeof(SQL_ColumnCriteria));
        }
        #endregion Add
        #region Indexors
        public SQL_ColumnCriteria this[string pColumnName]
        {
            get
            {
                SQL_ColumnCriteria ret = null;

                if (m_ColumnCriteriaSpecified)
                {
                    foreach (SQL_ColumnCriteria column in m_ColumnCriteria)
                    {
                        if (pColumnName == column.ColumnName)
                            ret = column;
                    }
                }
                return ret;
            }
        }
        #endregion Indexors

        #region Methods
        public string GetSQLWhere(string pConnectionString, string pAliasTable)
        {
            string SQLWhere = string.Empty;
            if (m_ColumnCriteriaSpecified)
            {
                string aliasTable = StrFunc.IsFilled(pAliasTable) ? pAliasTable + "." : string.Empty;

                const string commentFilter = Cst.Space + SQLCst.SQL_ANSI_COMMENT_BEGIN + " F{0}: {1} " + SQLCst.SQL_ANSI_COMMENT_END;
                int countFilter = -1;
                foreach (SQL_ColumnCriteria column in m_ColumnCriteria)
                {
                    if (column != null)
                    {
                        countFilter++;

                        if (countFilter > 0)
                            SQLWhere += Cst.Space2 + SQLCst.AND;

                        SQLWhere += column.GetExpression(pConnectionString, aliasTable, m_IsCaseInsensitive)
                                 + string.Format(commentFilter, countFilter, column.ColumnName)
                                 + Cst.CrLf;
                    }
                }
            }
            return SQLWhere;
        }
        public string GetSQLWhere2(string pConnectionString, string pAliasTable)
        {
            string SQLWhere = string.Empty;
            if (m_ColumnCriteriaSpecified)
            {
                string aliasTable = string.Empty;
                if (StrFunc.IsFilled(pAliasTable))
                    aliasTable = pAliasTable + ".";
                //
                foreach (SQL_ColumnCriteria column in m_ColumnCriteria)
                {
                    if (column != null)
                        SQLWhere += column.GetExpression2(pConnectionString, aliasTable, m_IsCaseInsensitive) + SQLCst.AND;
                }
                SQLWhere = SQLWhere.Substring(0, SQLWhere.LastIndexOf(SQLCst.AND)) + Cst.CrLf;
            }
            return SQLWhere;
        }

        /// <summary>
        /// Retourne true s'il existe un filtre "=" appliqué à la colonne {pColumn} pour la valeur {pValue}
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pColumn"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20201201 [XXXXX] Add
        public bool IsExistFilterOnValueOnly(string pConnectionString, string pAliasTable, string pColumn, string pValue)
        {
            bool ret = false;
            if (m_ColumnCriteriaSpecified)
            {
                string col = m_IsCaseInsensitive ? DataHelper.SQLUpper(pConnectionString, $"{pAliasTable}.{pColumn}") : $"{pAliasTable}.{pColumn}";
                string value = m_IsCaseInsensitive ? DataHelper.SQLUpper(pConnectionString, $"'{pValue}'") : $"'{pValue}'";

                foreach (SQL_ColumnCriteria column in m_ColumnCriteria)
                {
                    string expression = column.GetExpression(pConnectionString, $"{pAliasTable}.", m_IsCaseInsensitive);
                    if (expression == $"({col} = {value})")
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne true s'il existe un filtre appliqué à la colonne {pColumn} qui exclue la valeur {pValue}
        /// </summary>
        /// <param name="pConnectionString"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pColumn"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20201201 [XXXXX] Add
        public bool IsExistFilterOnValueNone(string pConnectionString, string pAliasTable, string pColumn, string pValue)
        {
            bool ret = false;
            if (m_ColumnCriteriaSpecified)
            {
                string col = m_IsCaseInsensitive ? DataHelper.SQLUpper(pConnectionString, $"{pAliasTable}.{pColumn}") : $"{pAliasTable}.{pColumn}";
                string value = m_IsCaseInsensitive ? DataHelper.SQLUpper(pConnectionString, $"'{pValue}'") : $"'{pValue}'";

                foreach (SQL_ColumnCriteria column in m_ColumnCriteria)
                {
                    string expression = column.GetExpression(pConnectionString, $"{pAliasTable}.", m_IsCaseInsensitive);
                    if ((expression == $"({col} != {value})") ||
                        (expression.StartsWith($"({col} = ") && (expression.IndexOf($"'{pValue}'") < 0)))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion Methods
    }


    public class SQL_ColumnCriteria
    {
        #region Members
        private string _columnName;
        // FI 20190327 [24603] _dataType est de type SQL_ColumnCriteriaDataType
        private SQL_ColumnCriteriaDataType _dataType;
        private string _operator;
        // FI 20190327 [24603] _value est de type SQL_ColumnCriteriaInput
        private SQL_ColumnCriteriaInput _value;

        private string _columnSqlWhere;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        //[System.Xml.Serialization.XmlAttributeAttribute("datatype")]
        [System.Xml.Serialization.XmlElementAttribute("datatype", typeof(SQL_ColumnCriteriaDataType))]
        public SQL_ColumnCriteriaDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("operator")]
        public string Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        // [System.Xml.Serialization.XmlTextAttribute(DataType = "anyURI")]
        [System.Xml.Serialization.XmlElementAttribute("value", typeof(SQL_ColumnCriteriaInput))]
        public SQL_ColumnCriteriaInput Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("columnSqlWhere")]
        public string ColumnSqlWhere
        {
            get { return _columnSqlWhere; }
            set { _columnSqlWhere = value; }
        }

        /// <summary>
        /// Obtient true lorsque Spheres utilise le contenu de la colonne SQLWHERE dans la constition de la clause Where
        /// </summary>
        public bool IsExistSqlWhere
        {
            get
            {
                return StrFunc.IsFilled(ColumnSqlWhere);
            }
        }

        #endregion Accessors

        #region Constructors
        public SQL_ColumnCriteria() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataType">datatype de la donnée SQL</param>
        /// <param name="pColumnName"></param>
        /// <param name="pColumnSqlWhere"></param>
        /// <param name="pOperator">Opérateur du critère</param>
        /// <param name="pValue">valeur du critère</param>
        public SQL_ColumnCriteria(SQL_ColumnCriteriaDataType pDataType, string pColumnName, string pColumnSqlWhere, string pOperator, SQL_ColumnCriteriaInput pValue)
        {
            _dataType = pDataType;
            _columnName = pColumnName;
            _operator = pOperator;
            _value = pValue;
            _columnSqlWhere = pColumnSqlWhere;
        }
        #endregion Constructors





        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pValue"></param>
        /// <param name="pIsExclude"></param>
        /// <param name="pIsDateValue"></param>
        /// <param name="pColumnSqlWhere"></param>
        /// <param name="pConditionalExpression"></param>
        /// <param name="pNode"></param>
        /// <param name="pOperator"></param>
        /// <param name="pIsCaseInsensitive"></param>
        /// <returns></returns>
        private string XQueryFormatValue(string pCS, string pValue, bool pIsExclude, bool pIsDateValue, string pNode, string pOperator, bool pIsCaseInsensitive)
        {
            string @value = string.Empty;
            string tmpOperator;

            if (pOperator.Trim() == "like" || pOperator.Trim() == "not like")
                pValue = pValue.Replace("%", string.Empty);

            if ("null" == pValue.ToLower())
            {
                tmpOperator = pIsExclude ? " > " : " = ";
                @value = "string-length(" + pNode + ")" + tmpOperator + "0";
                return @value;
            }

            if (pIsDateValue)
            {
                //20090929 PL Test à revoir et finaliser pour gérer une année en "dur" (see also FormatLstValue2())
                if (pValue.Length == 4)
                {
                    #region date sur 4 caractères
                    if ((pOperator.Trim() == "<") || (pOperator.Trim() == ">=") || (pOperator.Trim() == "<=") || (pOperator.Trim() == ">"))
                    {
                        if ((pOperator.Trim() == "<") || (pOperator.Trim() == ">="))
                            @value += StrFunc.Frame(pValue + "-01-01", "\"");
                        else if (pOperator.Trim() == "<=")
                            @value += StrFunc.Frame(pValue + "-12-31", "\"");
                        else if (pOperator.Trim() == ">")
                            @value += StrFunc.Frame(pValue + "-12-31", "\"");

                        @value = pNode + pOperator + @value;
                    }
                    else
                    {
                        if ((pOperator.Trim() == "=") || (pOperator.Trim() == "!=") || (pOperator.Trim() == "like") || (pOperator.Trim() == "not like"))
                        {
                            //Between entre la valeur et la valeur + 23:59:59
                            if ((pOperator.Trim() == "=") || (pOperator.Trim() == "like"))
                                tmpOperator = " >= ";
                            else
                                tmpOperator = " < ";

                            @value = StrFunc.Frame(pNode + tmpOperator + StrFunc.Frame(pValue + "-01-01", "\""));
                            //Between entre la valeur et la valeur + 23:59:59
                            if ((pOperator.Trim() == "=") || (pOperator.Trim() == "like"))
                                tmpOperator = " <= ";
                            else
                                tmpOperator = " > ";
                            //
                            if ((pOperator.Trim() == "=") || (pOperator.Trim() == "like"))
                                @value += SQLCst.AND;
                            else
                                @value += SQLCst.OR;

                            @value += StrFunc.Frame(pNode + tmpOperator + StrFunc.Frame(pValue + "-12-31", "\""));
                        }
                    }
                    #endregion
                }
                else
                    @value = pNode + pOperator + StrFunc.Frame(pValue, "\"");
            }
            else if ((TypeData.IsTypeDateTime(_dataType.dataType)) || (TypeData.IsTypeBool(_dataType.dataType)) || (TypeData.IsTypeInt(_dataType.dataType)))
            {
                #region DateTime, Bool , Int
                if (TypeData.IsTypeDateTime(_dataType.dataType))
                    @value = StrFunc.Frame(pValue, "\"");
                else if (TypeData.IsTypeBool(_dataType.dataType))
                    @value = StrFunc.Frame(pValue, "\"");
                else if (TypeData.IsTypeInt(_dataType.dataType))
                    @value = IntFunc.IntValue2(pValue).ToString();

                @value = pNode + pOperator + @value;
                #endregion
            }
            else if (TypeData.IsTypeDec(_dataType.dataType))
            {
                #region Decimal
                @value = StrFunc.FmtDecimalToInvariantCulture(pValue);
                @value = pNode + pOperator + @value;
                #endregion
            }
            else
            {
                //RD 20110415 [17397] / Gérer le CI dans la requête XPath
                if (pIsCaseInsensitive)
                {
                    //CC/PL 20120430 upper-case() not supported on Oracle !!!
                    if (!DataHelper.IsDbOracle(pCS))
                    {
                        pNode = StrFunc.AppendFormat("upper-case({0})", pNode);
                        pValue = pValue.ToUpper();
                    }
                }
                //
                if (pOperator.Trim() == "StartsWith")
                {
                    //En attendant que la fonction xquery starts-with soit gérée
                    @value = StrFunc.AppendFormat("(substring({0},1,string-length(\"{1}\")) = \"{1}\")", pNode, pValue);
                }
                else if (pOperator.Trim() == "EndsWith")
                {
                    //En attendant que la fonction xquery ends-with soit gérée
                    @value = StrFunc.AppendFormat("(substring({0},string-length({0})-string-length(\"{1}\")+1,string-length(\"{1}\")) = \"{1}\")", pNode, pValue);
                }
                else if (pOperator.Trim() == "Contains")
                {
                    @value = StrFunc.AppendFormat("contains({0},\"{1}\")", pNode, pValue);
                }
                else if (pOperator.Trim() == "not Contains") //FI 20111114 (gestion de not Contains)
                {
                    @value = StrFunc.AppendFormat("not (contains({0},\"{1}\"))", pNode, pValue);
                }
                else if (pOperator.Trim() == "like")
                {
                    @value = StrFunc.AppendFormat("contains({0},\"{1}\")", pNode, pValue);
                }
                else if (pOperator.Trim() == "not like")
                {
                    @value = StrFunc.AppendFormat("not(contains({0},\"{1}\"))", pNode, pValue);
                }
                else
                    @value = pNode + pOperator + StrFunc.Frame(pValue, "\"");
            }
            return @value;

        }

        private string XQueryFormatValue(string pCS, string pValue, bool pIsExclude, bool pIsDateValue, string pNode, OperandEnum pOperand, bool pIsCaseInsensitive)
        {
            string @value = string.Empty;
            string xQueryOperand = TypeOperand.GetXQueryOperand(pOperand);

            if ((pOperand == OperandEnum.like) || (pOperand == OperandEnum.notlike))
                pValue = pValue.Replace("%", string.Empty);

            if ("null" == pValue.ToLower())
            {
                @value = "string-length(" + pNode + ")" + TypeOperand.GetXQueryOperand((pIsExclude ? OperandEnum.greaterthan : OperandEnum.equalto)) + "0";
            }
            else if (pIsDateValue)
            {
                if (pValue.Length == 4)
                {
                    #region Date sur 4 caractères
                    switch (pOperand)
                    {
                        case OperandEnum.equalto:
                        case OperandEnum.like:
                            @value = StrFunc.Frame(pNode + TypeOperand.GetXQueryOperand(OperandEnum.greaterorequalto) + StrFunc.Frame(pValue + "-01-01", "\""));
                            @value += SQLCst.AND;
                            @value += StrFunc.Frame(pNode + TypeOperand.GetXQueryOperand(OperandEnum.lessorequalto) + StrFunc.Frame(pValue + "-12-31", "\""));
                            break;
                        case OperandEnum.notlike:
                        case OperandEnum.notequalto:
                            @value = StrFunc.Frame(pNode + TypeOperand.GetXQueryOperand(OperandEnum.lessthan) + StrFunc.Frame(pValue + "-01-01", "\""));
                            @value += SQLCst.OR;
                            @value += StrFunc.Frame(pNode + TypeOperand.GetXQueryOperand(OperandEnum.greaterthan) + StrFunc.Frame(pValue + "-12-31", "\""));
                            break;
                        case OperandEnum.lessthan:
                        case OperandEnum.greaterthan:
                            @value = pNode + xQueryOperand + StrFunc.Frame(pValue + "-01-01", "\"");
                            break;
                        case OperandEnum.lessorequalto:
                        case OperandEnum.greaterorequalto:
                            @value = pNode + xQueryOperand + StrFunc.Frame(pValue + "-12-31", "\"");
                            break;
                    }
                    #endregion
                }
                else
                {
                    @value = pNode + xQueryOperand + StrFunc.Frame(pValue, "\"");
                }
            }
            else if ((TypeData.IsTypeDateTime(_dataType.dataType)) || (TypeData.IsTypeBool(_dataType.dataType)) || (TypeData.IsTypeInt(_dataType.dataType)))
            {
                #region DateTime, Bool , Int
                if (TypeData.IsTypeDateTime(_dataType.dataType))
                    @value = StrFunc.Frame(pValue, "\"");
                else if (TypeData.IsTypeBool(_dataType.dataType))
                    @value = StrFunc.Frame(pValue, "\"");
                else if (TypeData.IsTypeInt(_dataType.dataType))
                    @value = IntFunc.IntValue2(pValue).ToString();
                @value = pNode + xQueryOperand + @value;
                #endregion
            }
            else if (TypeData.IsTypeDec(_dataType.dataType))
            {
                #region Decimal
                @value = pNode + xQueryOperand + StrFunc.FmtDecimalToInvariantCulture(pValue);
                #endregion
            }
            else
            {
                // CC/PL 20120430 upper-case() not supported on Oracle !!!
                if (pIsCaseInsensitive && (false == DataHelper.IsDbOracle(pCS)))
                {
                    pNode = StrFunc.AppendFormat("upper-case({0})", pNode);
                    pValue = pValue.ToUpper();
                }
                switch (pOperand)
                {
                    case OperandEnum.startswith:
                    case OperandEnum.endswith:
                    case OperandEnum.contains:
                    case OperandEnum.notcontains:
                    case OperandEnum.like:
                    case OperandEnum.notlike:
                        @value = StrFunc.AppendFormat(xQueryOperand, pNode, pValue);
                        break;
                    default:
                        @value = pNode + xQueryOperand + StrFunc.Frame(pValue, "\"");
                        break;
                }
            }
            return @value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pValue"></param>
        /// <param name="pIsExclude"></param>
        /// <param name="pIsDateValue"></param>
        /// <param name="opOperator"></param>
        /// <returns></returns>
        private string SQLFormatValue(string pCS, string pValue, bool pIsExclude, bool pIsDateValue, ref string opOperator)
        {
            string @value = string.Empty;
            
            if ("null" == pValue.ToLower())
            {
                opOperator = pIsExclude ? SQLCst.IS_NOT_NULL : SQLCst.IS_NULL;
                @value = string.Empty;
                //
                return @value;
            }
            
            if (pIsDateValue)
            {
                //20090929 PL Test à revoir et finaliser pour gérer une année en "dur" (see also FormatLstValue2())
                if (pValue.Length == 4)
                {
                    #region date sur 4 caractères
                    if ((opOperator.Trim() == "<") || (opOperator.Trim() == ">=") || (opOperator.Trim() == "<=") || (opOperator.Trim() == ">"))
                    {
                        if ((opOperator.Trim() == "<") || (opOperator.Trim() == ">="))
                            @value += DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "-01-01T00:00:00"));
                        else if (opOperator.Trim() == "<=")
                            @value += DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "-12-31T00:00:00"));
                        else if (opOperator.Trim() == ">")
                            @value += DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "-12-31T23:59:59"));
                    }
                    else
                    {
                        @value = DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "-01-01T00:00:00"));
                        //
                        if ((opOperator.Trim() == "=") || (opOperator.Trim() == "!=") || (opOperator.Trim() == "like") || (opOperator.Trim() == "not like"))
                        {
                            //Between entre la valeur et la valeur + 23:59:59
                            if ((opOperator.Trim() == "=") || (opOperator.Trim() == "like"))
                                opOperator = SQLCst.BETWEEN;
                            else
                                opOperator = SQLCst.NOT + SQLCst.BETWEEN;
                            //
                            @value += SQLCst.AND + DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "-12-31T23:59:59"));
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Date
                    //20090923 PL Newness
                    if (opOperator.Trim() == "<=")
                    {
                        @value += DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "T23:59:59"));
                    }
                    else
                    {
                        @value = DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "T00:00:00"));
                        //
                        //20090922 PL Newness
                        if ((opOperator.Trim() == "=") || (opOperator.Trim() == "!=") || (opOperator.Trim() == "like") || (opOperator.Trim() == "not like"))
                        {
                            //Between entre la valeur et la valeur + 23:59:59
                            if ((opOperator.Trim() == "=") || (opOperator.Trim() == "like"))
                                opOperator = SQLCst.BETWEEN;
                            else
                                opOperator = SQLCst.NOT + SQLCst.BETWEEN;

                            @value += SQLCst.AND + DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue + "T23:59:59"));
                        }
                    }
                    #endregion
                }
            }
            else if (TypeData.IsTypeDateTime(_dataType.dataType))
            {
                @value = DataHelper.SQLToDateTime(pCS, ConvertToColTzdbid(pValue));
            }
            else if (TypeData.IsTypeBool(_dataType.dataType))
            {
                @value = DataHelper.SQLBoolean(pValue.ToUpper().Equals("TRUE"));
            }
            else if (TypeData.IsTypeInt(_dataType.dataType))
            {
                //@value = pValue;
                //20090429 PL Suppression du séparateur de millier
                @value = IntFunc.IntValue2(pValue).ToString();
            }
            else if (TypeData.IsTypeDec(_dataType.dataType))
            {
                #region Decimal
                @value = StrFunc.FmtDecimalToInvariantCulture(pValue);
                //
                //PL 20110302 Add test on dec separator 
                //if (pValue.IndexOf(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator) == -1)
                //PL 20110623 Correction, test invariant dec separator. 
                if (pValue.IndexOf(".") == -1)
                {
                    //Decimal decValue = DecFunc.DecValue(pValue);
                    //Decimal intValue = Decimal.Truncate(decValue);
                    //if (decValue == intValue)
                    //{
                    if ((opOperator.Trim() == "=") || (opOperator.Trim() == "!="))
                    {
                        //Pas de décimal dans la valeur à filtrer --> Between entre la valeur et la valeur +0.999999999
                        if (opOperator.Trim() == "=")
                            opOperator = SQLCst.BETWEEN;
                        else
                            opOperator = SQLCst.NOT + SQLCst.BETWEEN;//2009092 PL Newness
                        //
                        //@value += SQLCst.AND + intValue.ToString() + @".999999999";
                        //PL 20110623 Add management negative value and zero value
                        decimal decValueTruncated = Decimal.Truncate(DecFunc.DecValue(pValue));
                        if (decValueTruncated == 0)
                        {
                            @value = @"-" + decValueTruncated.ToString() + @".999999999" + SQLCst.AND + decValueTruncated.ToString() + @".999999999";
                        }
                        else if (decValueTruncated < 0)
                        {
                            @value = decValueTruncated.ToString() + @".999999999" + SQLCst.AND + @value;
                        }
                        else
                        {
                            @value = @value + SQLCst.AND + decValueTruncated.ToString() + @".999999999";
                        }
                    }
                    //}
                }
                #endregion
            }
            else
            {
                @value = DataHelper.SQLString(pValue);
            }
            //
            return @value;
        }

        // 20161122 EG New V6 BootStrap
        private string SQLFormatValue(string pCS, string pValue, bool pIsExclude, bool pIsDateValue, OperandEnum pOperand, ref string operandValue)
        {
            string @value = string.Empty;
            operandValue = TypeOperand.GetSQLOperand(pOperand);

            if ("null" == pValue.ToLower())
            {
                operandValue = pIsExclude ? SQLCst.IS_NOT_NULL : SQLCst.IS_NULL;
                @value = string.Empty;
            }
            else if (pIsDateValue)
            {
                //20090929 PL Test à revoir et finaliser pour gérer une année en "dur" (see also FormatLstValue2())
                if (pValue.Length == 4)
                {
                    #region Date sur 4 caractères
                    switch (pOperand)
                    {
                        case OperandEnum.equalto:
                        case OperandEnum.like:
                            operandValue = SQLCst.BETWEEN;
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "-01-01T00:00:00") + SQLCst.AND + DataHelper.SQLToDateTime(pCS, pValue + "-12-31T23:59:59");
                            break;
                        case OperandEnum.notlike:
                        case OperandEnum.notequalto:
                            operandValue = SQLCst.NOT + SQLCst.BETWEEN;
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "-01-01T00:00:00") + SQLCst.AND + DataHelper.SQLToDateTime(pCS, pValue + "-12-31T23:59:59");
                            break;
                        case OperandEnum.lessthan:
                        case OperandEnum.greaterorequalto:
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "-01-01T00:00:00");
                            break;
                        case OperandEnum.greaterthan:
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "-12-31T23:59:59");
                            break;
                        case OperandEnum.lessorequalto:
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "-12-31T00:00:00");
                            break;
                    }
                    #endregion
                }
                else
                {
                    #region Date
                    switch (pOperand)
                    {
                        case OperandEnum.lessorequalto:
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "T23:59:59");
                            break;
                        case OperandEnum.greaterorequalto:
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "T00:00:00");
                            break;
                        case OperandEnum.equalto:
                        case OperandEnum.like:
                            operandValue = SQLCst.BETWEEN;
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "T00:00:00") + SQLCst.AND + DataHelper.SQLToDateTime(pCS, pValue + "T23:59:59");
                            break;
                        case OperandEnum.notequalto:
                        case OperandEnum.notlike:
                            operandValue = SQLCst.NOT + SQLCst.BETWEEN;
                            @value = DataHelper.SQLToDateTime(pCS, pValue + "T00:00:00") + SQLCst.AND + DataHelper.SQLToDateTime(pCS, pValue + "T23:59:59");
                            break;
                    }
                    #endregion
                }
            }
            else if ((TypeData.IsTypeDateTime(_dataType.dataType)) || (TypeData.IsTypeBool(_dataType.dataType)) || (TypeData.IsTypeInt(_dataType.dataType)))
            {
                #region DateTime, Bool , Int
                if (TypeData.IsTypeDateTime(_dataType.dataType))
                    @value = DataHelper.SQLToDateTime(pCS, pValue);
                else if (TypeData.IsTypeBool(_dataType.dataType))
                    @value = DataHelper.SQLBoolean(pValue.ToUpper().Equals("TRUE"));
                else if (TypeData.IsTypeInt(_dataType.dataType))
                    @value = IntFunc.IntValue2(pValue).ToString();

                #endregion
            }
            else if (TypeData.IsTypeDec(_dataType.dataType))
            {
                #region Decimal
                @value = StrFunc.FmtDecimalToInvariantCulture(pValue);

                if ((-1 == pValue.IndexOf(".")) && ((OperandEnum.equalto == pOperand) || (OperandEnum.notequalto == pOperand)))
                {
                    //Pas de décimal dans la valeur à filtrer --> Between entre la valeur et la valeur +0.999999999
                    operandValue = (OperandEnum.equalto == pOperand) ? SQLCst.BETWEEN : SQLCst.NOT + SQLCst.BETWEEN;

                    decimal decValueTruncated = Decimal.Truncate(DecFunc.DecValue(pValue));
                    string strValueTruncated = decValueTruncated.ToString() + @".999999999";
                    switch (Math.Sign(decValueTruncated))
                    {
                        case -1:
                            @value = strValueTruncated + SQLCst.AND + @value;
                            break;
                        case 0:
                            @value = @"-" + strValueTruncated + SQLCst.AND + strValueTruncated;
                            break;
                        case 1:
                            @value = @value + SQLCst.AND + strValueTruncated;
                            break;
                    }
                }
                #endregion
            }
            else
            {
                @value = DataHelper.SQLString(pValue);
            }
            return @value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAliasTable"></param>
        /// <param name="pIsCaseInsensitive"></param>
        /// <returns></returns>
        /// FI 20160630 [XXXXX] Modify
        public string GetExpression(string pCS, string pAliasTable, bool pIsCaseInsensitive)
        {
            string ret = string.Empty;

            #region ColumnName ou Expression: Ajout de l'alias de la table
            string columnName = _columnName;
            int posDot = columnName.IndexOf(".");
            if (posDot < 0)
            {
                //Ajout de l'alias de la table
                columnName = pAliasTable + columnName.ToUpper();
            }

            //Remplacement du mot clé <aliastable> par la valeur de l'alias.
            string columnSqlWhere = _columnSqlWhere;
            if (StrFunc.IsFilled(pAliasTable) && StrFunc.IsFilled(columnSqlWhere))
                columnSqlWhere = columnSqlWhere.Replace(Cst.DYNAMIC_ALIASTABLE, pAliasTable);
            #endregion

            #region Operator
            string @operator = _operator;

            bool isParticularOperatorLike = (@operator.Contains("StartsWith") || @operator.Contains("EndsWith") || @operator.Contains("Contains"));
            bool isExcludeOperator = ((@operator == SQLCst.NOTEQUAL.Trim()) || (@operator == SQLCst.DIFFERENT.Trim()) || @operator.StartsWith(SQLCst.NOT.Trim()));

            @operator = StrFunc.Frame(@operator, Cst.Space);

            #endregion
            #region Values
            // RD 20091021 C'est le cas de l'utilisation des accolades pour localiser l'objet à tester
            // Exemple: une colonne dans LSTCOLUMN  
            //     SQLWHERE = '<aliasTable>.TRADEXML.exist(''declare default element namespace "http://www.fpml.org/2007/FpML-4-4";  declare namespace efs="http://www.efs.org/2007/EFSmL-3-0"; (//tradeHeader/tradeDate/text()[spheres:filter(".")])'') = 1'
            //     DATATYPE = 'date'
            // et la requête générée après saisie de la date sur le critère correspondant à la colonne est:
            //     where (t.TRADEXML.exist('declare default element namespace "http://www.fpml.org/2007/FpML-4-4";  declare namespace efs="http://www.efs.org/2007/EFSmL-3-0"; (//tradeHeader/tradeDate/text()[. = ''2009-10-14''])') = 1)
            //
            // Il faudrait comprendre par isWithConditionalObject, ce qui suit:
            //    si le SQLWHERE contient un objet entre accolades et qui va être utilisé dans la condition de comparaison


            GetFilterExpressionInColumnSqlWhere(out string filterExpression, out string nodeOfFilterExpression);
            bool isWithConditionalObject = StrFunc.IsFilled(nodeOfFilterExpression);

            #region ReplaceResourceEnumValue (Transcription du critère saisi en valeur d'enum)
            string[] lstvalue = _value.value.Split(";".ToCharArray());
            string _valueReplaced = string.Empty;
            for (int i = 0; i < ArrFunc.Count(lstvalue); i++)
            {
                int countResourceEnumValue = 0;
                string valueItem = lstvalue[i];

                //EG 20160224 Add test on REQUESTTYPE_EXTVALUE
                if (_columnName == "posact.REQUESTTYPEENUM" || _columnName == "pr.EXTVALUE" || _columnName == "etycss.EXTVALUE")
                {
                    //FI/CC 20111110 interprétation de l'enum PosRequestTypeEnum s'il est saisi dans la langue de l'utilisateur
                    // Pour l'instant Spheres® effectue cela uniquement pour la consultation LSTCONSULT = POSACTIONDET (présence du test sur "posact.EXTVALUE")
                    // Ce test doit sauter dans l'avenir (trop risqué avant démo donc on laisse ainsi)
                    //countResourceEnumValue = ReplaceResourceEnumValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                    //PL 20130104 Use ReplaceResourceEnumExtValue() instead of ReplaceResourceEnumValue()
                    //CC 20130816 Ajout des colonnes pr.EXTVALUE (POSREQUEST.xml et TRACKER_POSREQUEST.xml) et etycss.EXTVALUE (EOD_POSREQUEST.xml)
                    countResourceEnumValue = ReplaceResourceEnumExtValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                }
                else if (_columnName.EndsWith("REQUESTTYPE_EXTVALUE"))
                {
                    // EG 2016024 New
                    countResourceEnumValue += ReplaceResourceEnumExtValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                    if (0 == countResourceEnumValue)
                        countResourceEnumValue += ReplaceResourceEnumValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                }

                else if (_columnName.IndexOf("IOTASKDET_TYPE") > 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(Cst.IOElementType), ref valueItem);
                }
                else if (_columnName.IndexOf("ELEMENTSUBTYPE") > 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(EFS.SpheresIO.CompareOptions), "CompareOptions_", ref valueItem);
                }
                else if (columnName.IndexOf("CNFTYPE") >= 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(NotificationTypeEnum), "CNFTYPE_", ref valueItem);
                }
                else if (columnName.IndexOf("CNFCLASS") >= 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(NotificationClassEnum), "CNFCLASS_", ref valueItem);
                }

                if (countResourceEnumValue >= 1)
                {
                    //On est ici sur une donnée de type Ressource et Enum, où une ou plusieurs valeur d'enum ont été trouvées.
                    //On force donc l'opérateur à = ou !=, et on supprime le case sensitive qui devient de fait inutile.
                    if (isExcludeOperator)
                        @operator = "!=";
                    else
                        @operator = "=";
                    pIsCaseInsensitive = false;
                    isParticularOperatorLike = false;
                    if (StrFunc.IsFilled(_valueReplaced))
                        _valueReplaced += ";";
                    _valueReplaced += valueItem;
                }
            }
            if (StrFunc.IsFilled(_valueReplaced))
                _value = new SQL_ColumnCriteriaInput(_valueReplaced);
            #endregion

            //Boucle sur l'ensemble des valeurs dans le cas où il y en a plusieurs séparaées par des ";"

            // RD 20160523 [22121] Pour les opérateurs logiques "Checked" et "Unchecked", la valeur (LSTWHERE.LSTVALUE) est vide.   
            //lstvalue = _value.Split(";".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            //if (TypeData.IsTypeBool(_dataType) && (("Checked" == @operator.Trim()) || ("Unchecked" == @operator.Trim())))
            //    lstvalue = _value.Split(";".ToCharArray());
            //else
            //    lstvalue = _value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // FI 20160630 [22121] 
            // Il ne faut pas faire RemoveEmptyEntries si _value est '' ou null (cas qui se produit lorsque l'utilisateur renseigne dans son filtre la valeur {null}
            // Solution proche de la correction opérée par RD. Elle s'applique toutefois à tous les types de donnée
            // Plus loin on voit que la valeur est alors remplacée par null
            if (StrFunc.IsFilled(_value.value))
                lstvalue = _value.value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            else
                lstvalue = _value.value.Split(";".ToCharArray(), StringSplitOptions.None);

            for (int i = 0; i < ArrFunc.Count(lstvalue); i++)
            {
                if (i > 0)
                    ret += isExcludeOperator ? SQLCst.AND : SQLCst.OR;
                string operatorItem = @operator;

                string valueItem = lstvalue[i];
                if (!StrFunc.IsFilledOrSpace(valueItem))
                    valueItem = "null";

                #region Date/DateTime
                //20091012 FI Les données DateTime peuvent être considérées comme des dates 
                bool isDateValue = TypeData.IsTypeDate(_dataType.dataType);
                if (TypeData.IsTypeDateTime(_dataType.dataType))
                {
                    // FI 20190327 [24603] Usage TryParseExact 
                    // la valeur 2017-02-02T00:00:00 est désormais considérée comme une datetime
                    // Modifification en phase avec les évolutions effectuées dans LstConsult.FormatLstValue2
                    isDateValue = DateTime.TryParseExact(valueItem, DtFunc.FmtISODate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _) || (valueItem.Length == 4);
                }
                #endregion
                #region Boolean
                // RD 20110706 [17504] Transformer les deux opérateurs: Checked et Unchecked
                else if (TypeData.IsTypeBool(_dataType.dataType))
                {
                    if ("Checked" == operatorItem.Trim())
                    {
                        operatorItem = "=";
                        valueItem = "true";
                    }
                    else if ("Unchecked" == operatorItem.Trim())
                    {
                        operatorItem = "=";
                        valueItem = "false";
                    }
                }
                #endregion
                #region String
                else if (TypeData.IsTypeString(_dataType.dataType))
                {
                    if (StrFunc.IsFilled(columnSqlWhere))
                    {
                        #region FIX node
                        if (columnSqlWhere.IndexOf("@PutCall") > 0)
                        {
                            #region Tip for Put/Call on FIX node
                            //PL 20110909 Tip for Put/Call on FIX node
                            if (valueItem.ToUpper().StartsWith("P"))
                                //valueItem = Enum.Format(typeof(FixML.Enum.PutOrCallEnum), FixML.Enum.PutOrCallEnum.Put, "d");
                                valueItem = "0";
                            else if (valueItem.ToUpper().StartsWith("C"))
                                //valueItem = Enum.Format(typeof(FixML.Enum.PutOrCallEnum), FixML.Enum.PutOrCallEnum.Call, "d");
                                valueItem = "1";
                            else if (valueItem.ToUpper().StartsWith("{null}"))//PL 20111208 
                                valueItem = "1";
                            #endregion
                        }
                        else if (columnSqlWhere.IndexOf("@Side") > 0)
                        {
                            #region Tip for Side on FIX node
                            //PL 20111004 Tip for Side on FIX node
                            //PL 20120427 Code dupliqué ci-dessous, puis légèrement modifié (see 'Buy')
                            if (valueItem.ToUpper().StartsWith(Ressource.GetString("Buy").Substring(0, 1)) ||
                                valueItem.ToUpper().StartsWith("B"))
                                valueItem = "1";
                            else if (valueItem.ToUpper().StartsWith(Ressource.GetString("Sell").Substring(0, 1))
                                || valueItem.ToUpper().StartsWith("S"))
                                valueItem = "2";
                            #endregion
                        }
                        else if (columnSqlWhere.IndexOf("@PosEfct") > 0)
                        {
                            #region Tip for Position Effect on FIX node
                            //PL 20111004 Tip for PosEfct on FIX node
                            if (valueItem.ToUpper().StartsWith("O"))
                                valueItem = "O";
                            else if (valueItem.ToUpper().StartsWith("C"))
                                valueItem = "C";
                            #endregion
                        }
                        #endregion
                    }
                    else if (columnName.IndexOf("IDST") >= 0)
                    {
                        #region Tip for IDSTACTIVATION, IDSTBUSINESS, IDSTENVIRONMENT and IDSTPRIORITY
                        //PL 20111124 Tip 
                        string statusName = null;
                        for (int count = 1; count <= 4; count++)
                        {
                            switch (count)
                            {
                                case 1:
                                    statusName = StatusEnum.StatusActivation.ToString();
                                    break;
                                case 2:
                                    statusName = StatusEnum.StatusBusiness.ToString();
                                    break;
                                case 3:
                                    statusName = StatusEnum.StatusEnvironment.ToString();
                                    break;
                                case 4:
                                    statusName = StatusEnum.StatusPriority.ToString();
                                    break;
                            }

                            if (columnName.IndexOf("IDST" + statusName.Replace("Status", string.Empty).ToUpper()) >= 0)
                            {
                                //FI 20120124 mise en commentaire les enums sont chargés à la connexion ou en cas de modifs
                                //ExtendEnumsTools.LoadFpMLEnumsAndSchemes(pCS);

                                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                                //ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                                //ExtendEnum extendEnum = ListEnumsSchemes[statusName];

                                ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, statusName);
                                ExtendEnumValue extendEnumValue = extendEnum.GetExtendEnumValueForCriteria(valueItem);

                                if (extendEnumValue != null)
                                    valueItem = extendEnumValue.Value;

                                break;
                            }
                        }
                        #endregion
                    }
                    else if (_columnName.IndexOf("then 'Buy' else 'Sell' end") > 0)
                    {
                        #region Side
                        //PL 20120427 Code semblable ci-dessus (see @Side)
                        if (valueItem.ToUpper().StartsWith(Ressource.GetString("Buy").Substring(0, 1)) ||
                            valueItem.ToUpper().StartsWith("B"))
                            valueItem = "Buy";
                        else if (valueItem.ToUpper().StartsWith(Ressource.GetString("Sell").Substring(0, 1))
                            || valueItem.ToUpper().StartsWith("S"))
                            valueItem = "Sell";
                        #endregion
                    }
                    else if (_columnName.IndexOf("FEEPAYERORRECEIVER") > 0)
                    {
                        #region Payer/Beneficiary
                        if (valueItem.ToUpper().StartsWith(Ressource.GetString("Payer").Substring(0, 1)) ||
                                valueItem.ToUpper().StartsWith("P"))
                            valueItem = "Payer";
                        else if (valueItem.ToUpper().StartsWith(Ressource.GetString("Beneficiary").Substring(0, 1)) ||
                                valueItem.ToUpper().StartsWith("B"))
                            valueItem = "Beneficiary";
                        #endregion
                    }
                }
                #endregion

                #region Formatage de la valeur
                // Formatage de la valeur selon son type, si on utilise le nom de la colonne ou bien le contenu de SQLWHERE 
                string escapeChar = string.Empty;
                string value;
                if (isWithConditionalObject)
                {
                    @value = XQueryFormatValue(pCS, valueItem, isExcludeOperator, isDateValue, nodeOfFilterExpression, operatorItem, pIsCaseInsensitive);
                }
                else
                {
                    if (isParticularOperatorLike)
                    {
                        #region
                        //Attention StartsWith, EndsWith, Contains marche aussi avec les types de données Int, Float, etc mais pas sur les dates
                        //Colonne StartsWith 2 => donne colonne like 2, ce qui correspond à une égalité en réalité 
                        if (TypeData.IsTypeString(_dataType.dataType) || TypeData.IsTypeText(_dataType.dataType))
                        {
                            // FI 20201230 [XXXXX] Pris en charge des caractères '[' et ']' 
                            if (valueItem.IndexOf("%") >= 0 || valueItem.IndexOf("_") >= 0 || valueItem.IndexOf("[") >= 0 || valueItem.IndexOf("]") >= 0)
                            {
                                escapeChar = "#";
                                valueItem = valueItem.Replace("%", "#%");
                                valueItem = valueItem.Replace("_", "#_");
                                valueItem = valueItem.Replace("[", "#[");
                                valueItem = valueItem.Replace("]", "#]");
                            }
                            
                            //FI 20111114 (mise en place de la fonction Contains pour gérer le not StartsWith prochainement)
                            if (operatorItem.Contains("StartsWith"))
                                valueItem += "%";
                            //FI 20111114 (mise en place de la fonction Contains pour gérer le not EndsWith prochainement)
                            else if (operatorItem.Contains("EndsWith"))
                                valueItem = "%" + valueItem;
                            //FI 20111114 (mise en place de la fonction Contains pour gérer le not Contains)
                            else if (operatorItem.Contains("Contains"))
                                valueItem = "%" + valueItem + "%";
                        }
                        //FI 20111114 mise en place de la fonction Contains
                        if (operatorItem.Contains("StartsWith"))
                            operatorItem = operatorItem.Replace("StartsWith", " like ");
                        else if (operatorItem.Contains("EndsWith"))
                            operatorItem = operatorItem.Replace("EndsWith", " like ");
                        else if (operatorItem.Contains("Contains"))
                            operatorItem = operatorItem.Replace("Contains", " like ");
                        #endregion
                    }
                    @value = SQLFormatValue(pCS, valueItem, isExcludeOperator, isDateValue, ref operatorItem);
                }

                string whereItem;

                #endregion
                if (IsExistSqlWhere)
                {
                    if (isWithConditionalObject)
                        // Le remplacement des accolades et de leurs contenu est déjà fait dans FormatValue()
                        whereItem = @value;
                    else
                        // C'est le cas ou on utilise les mots clé <operator><value> dans une requête SQL
                        // Exemple : SQLWHER = '<alisaTable>.Identifier<operator><value>
                        // Donc pas de XQuery
                        whereItem = columnSqlWhere.Replace(Cst.DYNAMIC_OPERATOR, operatorItem).Replace(Cst.DYNAMIC_VALUE, @value);
                }
                else
                {
                    //PL 201203087 Add test on IsFilled(). Quand @value est null, on a alors dans operatorItem IS_NOT_NULL ou IS_NULL
                    //if (pIsCaseInsensitive && TypeData.IsTypeString(_dataType))
                    if (pIsCaseInsensitive && TypeData.IsTypeString(_dataType.dataType) && StrFunc.IsFilled(@value))
                    {
                        //PL 20090323 Bug Utilisation de columnName à la place ColumnName, et @value à la place de @Value
                        whereItem = DataHelper.SQLUpper(pCS, columnName);
                        whereItem += operatorItem;
                        whereItem += DataHelper.SQLUpper(pCS, @value);
                    }
                    else
                    {
                        whereItem = columnName + operatorItem + @value;
                    }

                    if (TypeData.IsTypeString(_dataType.dataType) && isParticularOperatorLike && !string.IsNullOrEmpty(escapeChar))
                        whereItem += string.Format(" escape '{0}'", escapeChar);
                }

                whereItem = StrFunc.Frame(whereItem);
                ret += whereItem;
            }//end for
            #endregion Values

            if (ArrFunc.Count(lstvalue) > 1)
                ret = StrFunc.Frame(ret);

            //RD 20110307 Optimiser les requêtes XQuery
            if (isWithConditionalObject)
                ret = columnSqlWhere.Replace(filterExpression, ret);

            return ret;
        }

        // 20161122 EG New V6 BootStrap
        public string GetExpression2(string pCS, string pAliasTable, bool pIsCaseInsensitive)
        {
            string ret = string.Empty;

            #region ColumnName ou Expression: Ajout de l'alias de la table
            string columnName = _columnName;
            int posDot = columnName.IndexOf(".");
            if (posDot < 0)
            {
                //Ajout de l'alias de la table
                columnName = pAliasTable + columnName.ToUpper();
            }

            //Remplacement du mot clé <aliastable> par la valeur de l'alias.
            string columnSqlWhere = _columnSqlWhere;
            if (StrFunc.IsFilled(pAliasTable) && StrFunc.IsFilled(columnSqlWhere))
                columnSqlWhere = columnSqlWhere.Replace(Cst.DYNAMIC_ALIASTABLE, pAliasTable);
            #endregion

            #region Operator
            OperandEnum operand = TypeOperand.GetTypeOperandEnum(_operator);
            bool isParticularOperandLike = TypeOperand.IsParticularOperandLike(operand);
            bool isExcludeOperand = TypeOperand.IsExcludeOperand(operand);
            #endregion
            #region Values
            // RD 20091021 C'est le cas de l'utilisation des accolades pour localiser l'objet à tester
            // Exemple: une colonne dans LSTCOLUMN  
            //     SQLWHERE = '<aliasTable>.TRADEXML.exist(''declare default element namespace "http://www.fpml.org/2007/FpML-4-4";  declare namespace efs="http://www.efs.org/2007/EFSmL-3-0"; (//tradeHeader/tradeDate/text()[spheres:filter(".")])'') = 1'
            //     DATATYPE = 'date'
            // et la requête générée après saisie de la date sur le critère correspondant à la colonne est:
            //     where (t.TRADEXML.exist('declare default element namespace "http://www.fpml.org/2007/FpML-4-4";  declare namespace efs="http://www.efs.org/2007/EFSmL-3-0"; (//tradeHeader/tradeDate/text()[. = ''2009-10-14''])') = 1)
            //
            // Il faudrait comprendre par isWithConditionalObject, ce qui suit:
            //    si le SQLWHERE contient un objet entre accolades et qui va être utilisé dans la condition de comparaison
            GetFilterExpressionInColumnSqlWhere(out string filterExpression, out string nodeOfFilterExpression);
            bool isWithConditionalObject = StrFunc.IsFilled(nodeOfFilterExpression);

            #region ReplaceResourceEnumValue (Transcription du critère saisi en valeur d'enum)
            string[] lstvalue = _value.value.Split(";".ToCharArray());
            string _valueReplaced = string.Empty;
            for (int i = 0; i < ArrFunc.Count(lstvalue); i++)
            {
                int countResourceEnumValue = 0;
                string valueItem = lstvalue[i];

                //EG 20160224 Add test on REQUESTTYPE_EXTVALUE
                if (_columnName == "posact.REQUESTTYPEENUM" || _columnName == "pr.EXTVALUE" || _columnName == "etycss.EXTVALUE")
                {
                    //FI/CC 20111110 interprétation de l'enum PosRequestTypeEnum s'il est saisi dans la langue de l'utilisateur
                    // Pour l'instant Spheres® effectue cela uniquement pour la consultation LSTCONSULT = POSACTIONDET (présence du test sur "posact.EXTVALUE")
                    // Ce test doit sauter dans l'avenir (trop risqué avant démo donc on laisse ainsi)
                    //countResourceEnumValue = ReplaceResourceEnumValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                    //PL 20130104 Use ReplaceResourceEnumExtValue() instead of ReplaceResourceEnumValue()
                    //CC 20130816 Ajout des colonnes pr.EXTVALUE (POSREQUEST.xml et TRACKER_POSREQUEST.xml) et etycss.EXTVALUE (EOD_POSREQUEST.xml)
                    countResourceEnumValue = ReplaceResourceEnumExtValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                }
                else if (_columnName.EndsWith("REQUESTTYPE_EXTVALUE"))
                {
                    // EG 2016024 New
                    countResourceEnumValue += ReplaceResourceEnumExtValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                    if (0 == countResourceEnumValue)
                        countResourceEnumValue += ReplaceResourceEnumValue(typeof(Cst.PosRequestTypeEnum), ref valueItem);
                }

                else if (_columnName.IndexOf("IOTASKDET_TYPE") > 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(Cst.IOElementType), ref valueItem);
                }
                else if (_columnName.IndexOf("ELEMENTSUBTYPE") > 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(EFS.SpheresIO.CompareOptions), "CompareOptions_", ref valueItem);
                }
                else if (columnName.IndexOf("CNFTYPE") >= 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(NotificationTypeEnum), "CNFTYPE_", ref valueItem);
                }
                else if (columnName.IndexOf("CNFCLASS") >= 0)
                {
                    countResourceEnumValue = ReplaceResourceEnumValue(typeof(NotificationClassEnum), "CNFCLASS_", ref valueItem);
                }

                if (countResourceEnumValue >= 1)
                {
                    //On est ici sur une donnée de type Ressource et Enum, où une ou plusieurs valeur d'enum ont été trouvées.
                    //On force donc l'opérateur à = ou !=, et on supprime le case sensitive qui devient de fait inutile.
                    operand = isExcludeOperand ? OperandEnum.notequalto : OperandEnum.equalto;
                    pIsCaseInsensitive = false;
                    isParticularOperandLike = false;
                    if (StrFunc.IsFilled(_valueReplaced))
                        _valueReplaced += ";";
                    _valueReplaced += valueItem;

                }
            }
            if (StrFunc.IsFilled(_valueReplaced))
                _value = new SQL_ColumnCriteriaInput(_valueReplaced);
            #endregion

            //Boucle sur l'ensemble des valeurs dans le cas où il y en a plusieurs séparaées par des ";"
            lstvalue = _value.value.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ArrFunc.Count(lstvalue); i++)
            {
                if (i > 0)
                    ret += isExcludeOperand ? SQLCst.AND : SQLCst.OR;

                OperandEnum operandItem = operand;
                string formatOperand = operand.ToString();

                string valueItem = lstvalue[i];
                if (!StrFunc.IsFilledOrSpace(valueItem))
                    valueItem = "null";

                #region Date/DateTime
                //20091012 FI Les données DateTime renseignées sans l'heure (inexistence du T) sont considrérées comme des dates 
                bool isDateValue = TypeData.IsTypeDate(_dataType.dataType);
                if (TypeData.IsTypeDateTime(_dataType.dataType))
                {
                    isDateValue = (false == StrFunc.ContainsIn(valueItem, "T") || StrFunc.ContainsIn(valueItem, "T00:00:00"));
                    if (isDateValue)
                        valueItem = valueItem.Replace("T00:00:00", string.Empty);
                }
                #endregion
                #region Boolean
                // RD 20110706 [17504] Transformer les deux opérateurs: Checked et Unchecked
                else if (TypeData.IsTypeBool(_dataType.dataType))
                {
                    valueItem = (operandItem == OperandEnum.@checked) ? "true" : "false";
                }
                #endregion
                #region String
                else if (TypeData.IsTypeString(_dataType.dataType))
                {
                    if (StrFunc.IsFilled(columnSqlWhere))
                    {
                        #region FIX node
                        if (columnSqlWhere.IndexOf("@PutCall") > 0)
                        {
                            #region Tip for Put/Call on FIX node
                            //PL 20110909 Tip for Put/Call on FIX node
                            if (valueItem.ToUpper().StartsWith("P"))
                                valueItem = "0";
                            else if (valueItem.ToUpper().StartsWith("C"))
                                valueItem = "1";
                            else if (valueItem.ToUpper().StartsWith("{null}"))//PL 20111208 
                                valueItem = "1";
                            #endregion
                        }
                        else if (columnSqlWhere.IndexOf("@Side") > 0)
                        {
                            #region Tip for Side on FIX node
                            //PL 20111004 Tip for Side on FIX node
                            //PL 20120427 Code dupliqué ci-dessous, puis légèrement modifié (see 'Buy')
                            if (valueItem.ToUpper().StartsWith(Ressource.GetString("Buy").Substring(0, 1)) ||
                                valueItem.ToUpper().StartsWith("B"))
                                valueItem = "1";
                            else if (valueItem.ToUpper().StartsWith(Ressource.GetString("Sell").Substring(0, 1))
                                || valueItem.ToUpper().StartsWith("S"))
                                valueItem = "2";
                            #endregion
                        }
                        else if (columnSqlWhere.IndexOf("@PosEfct") > 0)
                        {
                            #region Tip for Position Effect on FIX node
                            //PL 20111004 Tip for PosEfct on FIX node
                            if (valueItem.ToUpper().StartsWith("O"))
                                valueItem = "O";
                            else if (valueItem.ToUpper().StartsWith("C"))
                                valueItem = "C";
                            #endregion
                        }
                        #endregion
                    }
                    else if (columnName.IndexOf("IDST") >= 0)
                    {
                        #region Tip for IDSTACTIVATION, IDSTBUSINESS, IDSTENVIRONMENT and IDSTPRIORITY
                        //PL 20111124 Tip 
                        string statusName = null;
                        for (int count = 1; count <= 4; count++)
                        {
                            switch (count)
                            {
                                case 1:
                                    statusName = StatusEnum.StatusActivation.ToString();
                                    break;
                                case 2:
                                    statusName = StatusEnum.StatusBusiness.ToString();
                                    break;
                                case 3:
                                    statusName = StatusEnum.StatusEnvironment.ToString();
                                    break;
                                case 4:
                                    statusName = StatusEnum.StatusPriority.ToString();
                                    break;
                            }

                            if (columnName.IndexOf("IDST" + statusName.Replace("Status", string.Empty).ToUpper()) >= 0)
                            {
                                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                                //ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                                //ExtendEnum extendEnum = ListEnumsSchemes[statusName];

                                ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(pCS, statusName);
                                if (null == extendEnum)
                                    throw new KeyNotFoundException($"Key:{statusName} not found.");
                                ExtendEnumValue extendEnumValue = extendEnum.GetExtendEnumValueForCriteria(valueItem);

                                if (extendEnumValue != null)
                                    valueItem = extendEnumValue.Value;

                                break;
                            }
                        }
                        #endregion
                    }
                    else if (_columnName.IndexOf("then 'Buy' else 'Sell' end") > 0)
                    {
                        #region Side
                        //PL 20120427 Code semblable ci-dessus (see @Side)
                        if (valueItem.ToUpper().StartsWith(Ressource.GetString("Buy").Substring(0, 1)) ||
                            valueItem.ToUpper().StartsWith("B"))
                            valueItem = "Buy";
                        else if (valueItem.ToUpper().StartsWith(Ressource.GetString("Sell").Substring(0, 1))
                            || valueItem.ToUpper().StartsWith("S"))
                            valueItem = "Sell";
                        #endregion
                    }
                    else if (_columnName.IndexOf("FEEPAYERORRECEIVER") > 0)
                    {
                        #region Payer/Beneficiary
                        if (valueItem.ToUpper().StartsWith(Ressource.GetString("Payer").Substring(0, 1)) ||
                                valueItem.ToUpper().StartsWith("P"))
                            valueItem = "Payer";
                        else if (valueItem.ToUpper().StartsWith(Ressource.GetString("Beneficiary").Substring(0, 1)) ||
                                valueItem.ToUpper().StartsWith("B"))
                            valueItem = "Beneficiary";
                        #endregion
                    }
                }
                // Formatage de la valeur selon son type, si on utilise le nom de la colonne ou bien le contenu de SQLWHERE 
                string value;

                #endregion
                #region Formatage de la valeur
                if (isWithConditionalObject)
                {
                    @value = XQueryFormatValue(pCS, valueItem, isExcludeOperand, isDateValue, nodeOfFilterExpression, operandItem, pIsCaseInsensitive);
                }
                else
                {
                    if (isParticularOperandLike)
                    {
                        //Attention StartsWith, EndsWith, Contains marche aussi avec les types de données Int, Float, etc mais pas sur les dates
                        //Colonne StartsWith 2 => donne colonne like 2, ce qui correspond à une égalité en réalité 
                        if (TypeData.IsTypeString(_dataType.dataType) || TypeData.IsTypeText(_dataType.dataType))
                        {
                            valueItem = valueItem.Replace("%", "#%");
                            valueItem = valueItem.Replace("_", "#_");
                            //
                            //FI 20111114 (mise en place de la fonction Contains pour gérer le not StartsWith prochainement)
                            if (operandItem == OperandEnum.startswith)
                                valueItem += "%";
                            //FI 20111114 (mise en place de la fonction Contains pour gérer le not EndsWith prochainement)
                            else if (operandItem == OperandEnum.endswith)
                                valueItem = "%" + valueItem;
                            //FI 20111114 (mise en place de la fonction Contains pour gérer le not Contains)
                            else if ((operandItem == OperandEnum.contains) || (operandItem == OperandEnum.notcontains))
                                valueItem = "%" + valueItem + "%";
                        }
                    }
                    @value = SQLFormatValue(pCS, valueItem, isExcludeOperand, isDateValue, operandItem, ref formatOperand);
                }

                string whereItem;

                #endregion
                if (IsExistSqlWhere)
                {
                    if (isWithConditionalObject)
                        // Le remplacement des accolades et de leurs contenu est déjà fait dans FormatValue()
                        whereItem = @value;
                    else
                        // C'est le cas ou on utilise les mots clé <operand><value> dans une requête SQL
                        // Exemple : SQLWHER = '<alisaTable>.Identifier<operand><value>
                        // Donc pas de XQuery
                        whereItem = columnSqlWhere.Replace(Cst.DYNAMIC_OPERATOR, formatOperand).Replace(Cst.DYNAMIC_VALUE, @value);
                }
                else
                {
                    if (pIsCaseInsensitive && TypeData.IsTypeString(_dataType.dataType) && StrFunc.IsFilled(@value))
                    {
                        whereItem = DataHelper.SQLUpper(pCS, columnName);
                        whereItem += formatOperand;
                        whereItem += DataHelper.SQLUpper(pCS, @value);
                    }
                    else
                    {
                        whereItem = columnName + formatOperand + @value;
                    }

                    if (TypeData.IsTypeString(_dataType.dataType) && isParticularOperandLike)
                        whereItem += " escape '#'";
                }

                whereItem = StrFunc.Frame(whereItem);
                ret += whereItem;
            }//end for
            #endregion Values

            if (ArrFunc.Count(lstvalue) > 1)
                ret = StrFunc.Frame(ret);

            //RD 20110307 Optimiser les requêtes XQuery
            if (isWithConditionalObject)
                ret = columnSqlWhere.Replace(filterExpression, ret);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFilterExpression"></param>
        /// <param name="pNode"></param>
        private void GetFilterExpressionInColumnSqlWhere(out string pFilterExpression, out string pNode)
        {
            // L'objet extrait de SQLWHERE, avec spheres:filter("")
            pFilterExpression = string.Empty;
            // L'objet extrait de SQLWHERE, sans spheres:filter("")
            pNode = string.Empty;
            //
            if (IsExistSqlWhere)
            {
                // Extraire l'objet à comparer entre: spheres:filter("")
                const string START_SPHERES_FILTER = @"spheres:filter(""";
                const string END_SPHERES_FILTER = @""")";
                //
                int pos1 = ColumnSqlWhere.IndexOf(START_SPHERES_FILTER);
                // RD 20091214 / 16802/ LSTCOLUMN.SQLSELECT, dans le cas où l'objet n'existe pas
                if (pos1 > -1)
                {
                    int pos2 = ColumnSqlWhere.IndexOf(END_SPHERES_FILTER, pos1);
                    //
                    pFilterExpression = ColumnSqlWhere.Substring(pos1, pos2 - pos1 + END_SPHERES_FILTER.Length);
                    if (StrFunc.IsFilled(pFilterExpression))
                        pNode = pFilterExpression.Replace(START_SPHERES_FILTER, string.Empty).Replace(END_SPHERES_FILTER, string.Empty);
                }
            }
        }

        /// <summary>
        /// Remplace ds {pValueItem} les ressources d'un enum par sa valeur
        /// <example>Exemple: avec l'enum Cst.IOElementType, Remplace Comparaison par Compare</example>
        /// </summary>
        /// <param name="pEnumType"></param>
        /// <param name="pValueItem"></param>
        /// <returns></returns>
        private static int ReplaceResourceEnumValue(Type pEnumType, ref string opValueItem)
        {
            return ReplaceResourceEnumValue(pEnumType, string.Empty, ref opValueItem);
        }
        private static int ReplaceResourceEnumValue(Type pEnumType, string pPrefixResource, ref string opValueItem)
        {
            return ReplaceResourceEnum("VALUE", pEnumType, pPrefixResource, ref opValueItem);
        }
        private static int ReplaceResourceEnumExtValue(Type pEnumType, ref string opValueItem)
        {
            return ReplaceResourceEnum("EXTVALUE", pEnumType, string.Empty, ref opValueItem);
        }
        private static int ReplaceResourceEnum(string pScope, Type pEnumType, string pPrefixResource, ref string opValueItem)
        {
            //PL 20120509 Add test sur enumValue et sur resource2 (see also FDA)
            int ret = 0;
            bool isScope_Value = (pScope == "VALUE");

            string enumValueItem = string.Empty;

            foreach (object enumValue in Enum.GetValues(pEnumType))
            {
                string enumSerializeValue = ReflectionTools.ConvertEnumToString(enumValue as System.Enum);

                string resource1 = pPrefixResource + enumSerializeValue.ToString();
                string resource2 = pPrefixResource + enumValue.ToString();

                if (opValueItem.ToUpper() == enumSerializeValue.ToString().ToUpper() ||
                    opValueItem.ToUpper() == Ressource.GetString(resource1).ToUpper() ||

                    opValueItem.ToUpper() == enumValue.ToString().ToUpper() ||
                    opValueItem.ToUpper() == Ressource.GetString(resource2).ToUpper())
                {
                    ret = 1;
                    enumValueItem = isScope_Value ? enumSerializeValue.ToString() : enumValue.ToString();
                    break;
                }
                else if (enumSerializeValue.ToString().ToUpper().IndexOf(opValueItem.ToUpper()) >= 0 ||
                    Ressource.GetString(resource1).ToUpper().IndexOf(opValueItem.ToUpper()) >= 0 ||

                    enumValue.ToString().ToUpper().IndexOf(opValueItem.ToUpper()) >= 0 ||
                    Ressource.GetString(resource2).ToUpper().IndexOf(opValueItem.ToUpper()) >= 0)
                {
                    ret++;
                    // EG 20160224 Add parenthesis
                    enumValueItem += isScope_Value ? enumSerializeValue.ToString() : enumValue.ToString() + ";";
                }
            }
            switch (ret)
            {
                case 0:
                    enumValueItem = opValueItem;
                    break;
                case 2:
                    //PL: Je crois me souvenir d'une particularité dans ce cas où 2 items correspondent et où on supprime le dernier ";".
                    //    Mais je ne sais plus laquelle... Il doit y avoir un exploitation particulière quand il n'y a pas de ";" final.
                    enumValueItem = enumValueItem.Substring(0, enumValueItem.Length - 1);
                    break;
            }
            opValueItem = enumValueItem;

            return ret;
        }

        /// <summary>
        /// Convertie la date/heure {pDateTime} dans le fuseau horaire associé à la colonne SQL pour application du critère  
        /// <para>Il est supposé que {pDateTime} est exprimé selon le fuseau horaire _input.tzdbId </para>
        /// </summary>
        /// <param name="pDateTime">date au format yyyy-MM-ddTHH:mm:ss</param>
        /// FI 20190327 [24603] Add Method
        private string ConvertToColTzdbid(string pDateTime)
        {
            string ret = pDateTime;
            if (_dataType.dataType == TypeData.TypeDataEnum.datetime &&
                _dataType.tzdbidSpecified && _value.tzdbidSpecified && (_dataType.tzdbId != _value.tzdbId)
                )
            {
                TimeZoneInfo tzInSource = Tz.Tools.GetTimeZoneInfoFromTzdbId(_value.tzdbId);
                TimeZoneInfo tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(_dataType.tzdbId);


                DateTime dt = new DtFunc().StringDateTimeISOToDateTime(ret);
                DateTime dateUTc = TimeZoneInfo.ConvertTimeToUtc(dt, tzInSource);
                dt = TimeZoneInfo.ConvertTimeFromUtc(dateUTc, tzInfoTarget);

                ret = DtFunc.DateTimeToString(dt, DtFunc.FmtISODateTime2);
            }
            return ret;
        }
    }
    
    /// <summary>
    /// Représente le type de donnée sur lequel s'applique le critère
    /// </summary>
    /// FI 20190327 [24603] Add Class
    public class SQL_ColumnCriteriaDataType
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tzdbidSpecified;

        /// <summary>
        /// TimeZone associé à la colonne SQL lorsque celle-ci est de type DateTime
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "tzdbid")]
        public string tzdbId;

        /// <summary>
        ///  type de donnée
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public TypeData.TypeDataEnum dataType;


        /// <summary>
        /// Nouvelle instance 
        /// </summary>
        public SQL_ColumnCriteriaDataType()
        {

        }

        /// <summary>
        /// Nouvelle instance 
        /// </summary>
        /// <param name="pDataType">Type de la colonne SQL</param>
        /// <param name="pTzdbId">TimeZone éventuel associé à la colonne SQL lorsque celle-ci est de type DateTime</param>
        public SQL_ColumnCriteriaDataType(TypeData.TypeDataEnum pDataType, string pTzdbId)
        {
            dataType = pDataType;
            tzdbidSpecified = (dataType == TypeData.TypeDataEnum.datetime) && StrFunc.IsFilled(pTzdbId);
            if (tzdbidSpecified)
                tzdbId = pTzdbId;
        }

        /// <summary>
        /// Nouvelle instance 
        /// </summary>
        /// <param name="pDataType">Type de la colonne SQL. Pas de TimeZone éventuel associé à la colonne lorsqu'elle est de type Datetime</param>
        public SQL_ColumnCriteriaDataType(TypeData.TypeDataEnum pDataType) :
            this(pDataType, string.Empty)
        {

        }
    }

    /// <summary>
    /// Représente la valeur du critère
    /// </summary>
    /// FI 20190327 [24603] Add Class
    public class SQL_ColumnCriteriaInput
    {

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tzdbidSpecified;

        /// <summary>
        /// TimeZone éventuel dans lequel est exprimé le critère 
        /// <para>Le critère est converti selon le fuseau horaire de la colonne SQL avant interrogation du moteur SQL (uniquement s'il existe un fuseau horraire associé à la colonne SQL (voir SQL_ColumnCriteriaDataType))</para>
        /// <para>Exemple de colonne avec timeZone TRADE.DTORDERENTERED, le tzdbid est Etc/UTC</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "tzdbid")]
        public string tzdbId;

        /// <summary>
        ///  Valeur du critère
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string @value;


        /// <summary>
        /// Nouvelle instance 
        /// </summary>
        public SQL_ColumnCriteriaInput()
        {

        }

        /// <summary>
        /// Nouvelle instance 
        /// </summary>
        /// <param name="pValue">valeur du critère</param>
        /// <param name="pTzdbId">TimeZone éventuel associé à la valeur lorsque celle-ci est de type DateTime</param>
        public SQL_ColumnCriteriaInput(string pValue, string pTzdbId)
        {
            @value = pValue;
            tzdbidSpecified = StrFunc.IsFilled(pTzdbId);
            if (tzdbidSpecified)
                tzdbId = pTzdbId;
        }

        /// <summary>
        /// Nouvelle instance 
        /// </summary>
        /// <param name="pValue">valeur du critère</param>
        public SQL_ColumnCriteriaInput(string pValue) :
            this(pValue, string.Empty)
        {

        }
    }
}

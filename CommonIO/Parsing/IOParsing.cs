using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.IO.Interface;
using EfsML.Business;
using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EFS.Common.IO
{
    /// <summary>
    /// 
    /// </summary>
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO (IOParsing.cs)
    public abstract class IOParsing
    {
        #region Members
        protected bool m_IsIn;
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //protected Task m_Task;
        protected IIOTaskLaunching m_Task;
        //
        protected DataSet m_DsIOParsing;
        protected DataTable m_DtIOParsing;
        protected DataRow[] m_DrIOParsing;
        //
        protected DataSet m_DsParsingDet;
        protected DataTable m_DtParsingDet;
        protected DataRow[] m_DrParsingDet;
        //
        protected string m_IdParsing;
        //
        protected string m_DataType;
        protected int m_DataStart;
        protected int m_DataLength;
        protected string m_DataSeparator;
        protected string m_DataCharDelimiter;
        protected string m_DefaultValue;
        protected string m_DefaultRule;
        protected string m_DataName;
        protected string m_DataValue;
        protected string m_DataFormat;
        protected string m_DataAlignment;
        protected string m_DataFillChar;
        protected string m_DataGrpSeparator;
        protected string m_DataDecSeparator;
        protected string m_DataRoundDir;
        protected int m_DataRoundPrec;
        protected bool m_IsOptional;
        // PM 20230622 [26091][WI390] Ajout m_DataKey
        protected string m_DataKey;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected virtual string IOParsingTableName
        {
            get { return string.Empty; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static string TableName
        {
            // PM 20180219 [23824] IOTools => IOCommonTools
            get { return IOCommonTools.TableName; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int ParsingCount
        {
            get { return m_DrIOParsing.Length; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int ParsingDetCount
        {
            get { return m_DrParsingDet.Length; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsIn
        {
            get { return m_IsIn; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string MessageData
        {
            get { return m_DataName.ToUpper() + " (Value: " + m_DataValue + " Type: " + m_DataType.ToUpper() + " )"; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdIO"></param>
        /// <param name="pTask"></param>
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public IOParsing(string pIdIO, Task pTask)
        public IOParsing(string pIdIO, IIOTaskLaunching pTask)
        {
            m_Task = pTask;
            //
            m_DsIOParsing = LoadIOParsing(pIdIO);
            m_DtIOParsing = m_DsIOParsing.Tables[0];
            m_DrIOParsing = m_DtIOParsing.Select();
            //
            string sqlFiltre = string.Empty;
            string idParsing;
            //
            for (int i = 0; i < m_DrIOParsing.Length; i++)
            {
                idParsing = m_DrIOParsing[i]["IDIOPARSING"].ToString();
                if (StrFunc.IsFilled(idParsing))
                    sqlFiltre += " ( " + TableName + ".IDIOPARSING = " + DataHelper.SQLString(idParsing) + " )" + SQLCst.OR;
            }
            sqlFiltre = sqlFiltre.TrimEnd(SQLCst.OR.ToString().ToCharArray());
            //
            m_DsParsingDet = LoadParsingDet(sqlFiltre);
            m_DtParsingDet = m_DsParsingDet.Tables[0];
            m_DrParsingDet = m_DtParsingDet.Select();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Retourne le XSL du Parsing 
        /// </summary>
        /// <returns></returns>
        /// RD 20100608 [] Refactoring
        public string GetXSLParsingFile()
        {
            string ret = string.Empty;
            //
            if (ParsingCount > 0)
            {
                ret = (Convert.IsDBNull(m_DrIOParsing[0]["XSLPARSING"]) ? null :
                    m_DrIOParsing[0]["XSLPARSING"].ToString());
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdIO"></param>
        /// <returns></returns>
        protected virtual DataSet LoadIOParsing(string pIdIO)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected string GetJoinIOParsingSQLQuery()
        {
            string joinQuery = Cst.OTCml_TBL.IOPARSING + ".XSLPARSING" + Cst.CrLf;
            joinQuery += SQLCst.FROM_DBO + IOParsingTableName + Cst.CrLf;
            joinQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.IOPARSING.ToString();
            joinQuery += SQLCst.ON + Cst.OTCml_TBL.IOPARSING + ".IDIOPARSING=" + IOParsingTableName + ".IDIOPARSING" + Cst.CrLf;
            joinQuery += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(m_Task.Cs, Cst.OTCml_TBL.IOPARSING) + Cst.CrLf;
            // PM 20180219 [23824] IOTools => IOCommonTools
            return IOCommonTools.GetJoinSQLQuery(IOParsingTableName, joinQuery);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataNumber"></param>
        /// PM 20230622 [26091][WI390] Ajout m_DataKey
        protected void GetParsingDetItem(int pDataNumber)
        {
            m_DataName = m_DrParsingDet[pDataNumber]["IDIOPARSINGDET"].ToString();
            m_DataType = m_DrParsingDet[pDataNumber]["DATATYPE"].ToString();
            m_DataStart = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DATASTART"]) ? 0 : Convert.ToInt32(m_DrParsingDet[pDataNumber]["DATASTART"]));
            m_DataLength = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DATALENGTH"]) ? 0 : Convert.ToInt32(m_DrParsingDet[pDataNumber]["DATALENGTH"]));
            m_DataCharDelimiter = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["CHARDELIMITER"]) ? null : m_DrParsingDet[pDataNumber]["CHARDELIMITER"].ToString().Trim());
            m_DataSeparator = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DATASEPARATOR"]) ? null : m_DrParsingDet[pDataNumber]["DATASEPARATOR"].ToString().Trim());
            m_DefaultRule = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DEFAULTRULE"]) ? null : m_DrParsingDet[pDataNumber]["DEFAULTRULE"].ToString().Trim());
            m_DefaultValue = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DEFAULTVALUE"]) ? null : m_DrParsingDet[pDataNumber]["DEFAULTVALUE"].ToString());
            m_DataFormat = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["FORMAT"]) ? null : m_DrParsingDet[pDataNumber]["FORMAT"].ToString().Trim());
            m_DataFillChar = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["FILLCHAR"]) ? null : m_DrParsingDet[pDataNumber]["FILLCHAR"].ToString().Trim());
            m_DataAlignment = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["ALIGNMENT"]) ? null : m_DrParsingDet[pDataNumber]["ALIGNMENT"].ToString().Trim());
            m_DataGrpSeparator = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["GRPSEPARATOR"]) ? null : m_DrParsingDet[pDataNumber]["GRPSEPARATOR"].ToString().Trim());
            m_DataDecSeparator = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DECSEPARATOR"]) ? null : m_DrParsingDet[pDataNumber]["DECSEPARATOR"].ToString().Trim());
            m_DataRoundDir = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["ROUNDDIR"]) ? null : m_DrParsingDet[pDataNumber]["ROUNDDIR"].ToString().Trim());
            m_DataRoundPrec = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["ROUNDPREC"]) ? -1 : Convert.ToInt32(m_DrParsingDet[pDataNumber]["ROUNDPREC"]));
            m_IsOptional = BoolFunc.IsTrue(m_DrParsingDet[pDataNumber]["ISOPTIONAL"]);
            m_DataKey = (Convert.IsDBNull(m_DrParsingDet[pDataNumber]["DATAKEY"]) ? null : m_DrParsingDet[pDataNumber]["DATAKEY"].ToString());
        }

        /// <summary>
        /// Charge la liste des ParsingsDet et retourne le IdParsing correspondant au Parsing pParsingNumber
        /// </summary>
        /// <param name="pParsingNumber"></param>
        /// <returns></returns>
        protected string GetParsingDet(int pParsingNumber)
        {
            string idParsing = m_DrIOParsing[pParsingNumber]["IDIOPARSING"].ToString();
            string filtreParsing = " ( " + TableName + ".IDIOPARSING = " + DataHelper.SQLString(idParsing) + " )";
            //
            GetParsingDet(filtreParsing);
            //
            return idParsing;
        }
        /// <summary>
        /// Retourne la liste des ParsingsDet filtrés avec pFiltreParsing
        /// </summary>
        /// <param name="pFiltreParsing"></param>
        protected void GetParsingDet(string pFiltreParsing)
        {
            pFiltreParsing = pFiltreParsing.Replace(TableName + ".", "");
            m_DrParsingDet = m_DtParsingDet.Select(pFiltreParsing);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFiltreIdIOParsing"></param>
        /// <returns></returns>
        protected DataSet LoadParsingDet(string pFiltreIdIOParsing)
        {
            pFiltreIdIOParsing = pFiltreIdIOParsing.Replace(TableName, Cst.OTCml_TBL.IOPARSINGDET.ToString());
            //
            if (StrFunc.IsEmpty(pFiltreIdIOParsing))
            {
                pFiltreIdIOParsing = " 1 = 0 ";
            }
            //
            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;
            sqlQuery += SQLCst.SELECT + Cst.OTCml_TBL.IOPARSINGDET + ".IDIOPARSINGDET, " + Cst.OTCml_TBL.IOPARSINGDET + ".IDIOPARSING, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".SEQUENCENO, " + Cst.OTCml_TBL.IOPARSINGDET + ".DATATYPE, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".DATASTART, " + Cst.OTCml_TBL.IOPARSINGDET + ".DATALENGTH," + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".DATASEPARATOR, " + Cst.OTCml_TBL.IOPARSINGDET + ".DATAKEY, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".CHARDELIMITER, " + Cst.OTCml_TBL.IOPARSINGDET + ".DEFAULTVALUE, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".DEFAULTRULE, " + Cst.OTCml_TBL.IOPARSINGDET + ".FORMAT, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".FILLCHAR, " + Cst.OTCml_TBL.IOPARSINGDET + ".GRPSEPARATOR, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".DECSEPARATOR, " + Cst.OTCml_TBL.IOPARSINGDET + ".ROUNDDIR, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".ROUNDPREC, " + Cst.OTCml_TBL.IOPARSINGDET + ".ALIGNMENT, " + Cst.CrLf;
            sqlQuery += Cst.OTCml_TBL.IOPARSINGDET + ".ISOPTIONAL " + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOPARSINGDET + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + pFiltreIdIOParsing + Cst.CrLf;
            sqlQuery += SQLCst.ORDERBY + Cst.OTCml_TBL.IOPARSINGDET + ".IDIOPARSING, " + Cst.OTCml_TBL.IOPARSINGDET + ".SEQUENCENO" + Cst.CrLf;
            //
            return DataHelper.ExecuteDataset(m_Task.Cs, CommandType.Text, sqlQuery);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SetDataValueDefaultValue()
        {
            SetDataValueDefaultValue(0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLineNumber"></param>
        protected void SetDataValueDefaultValue(int pLineNumber)
        {
            try
            {
                string[] defaultRule;
                string defaultRuleI = string.Empty;
                bool isApplyDefaultValue = false;
                //
                if (StrFunc.IsFilled(m_DefaultRule))
                {
                    defaultRule = m_DefaultRule.Split('|');
                    //
                    for (int i = 0; i < defaultRule.Length; i++)
                    {
                        defaultRuleI = defaultRule[i];
                        if (StrFunc.IsFilled(defaultRuleI))
                        {
                            if (StrFunc.IsFilled(m_DataValue))
                            {
                                if (m_DataValue == Regex.Unescape(defaultRuleI))
                                {
                                    isApplyDefaultValue = true;
                                }
                                else
                                {
                                    defaultRuleI = defaultRuleI.TrimStart('{');
                                    defaultRuleI = defaultRuleI.TrimEnd('}');
                                    //
                                    if (IsIn)
                                    {
                                        if ((Cst.RuleOnDefaultValue.FIRSTRECORD.ToString() == Regex.Unescape(defaultRuleI).ToUpper()) &&
                                            (pLineNumber == 1))
                                        {
                                            isApplyDefaultValue = true;
                                        }
                                    }
                                }
                            }
                            else if (('\x00'.ToString() == Regex.Unescape(defaultRuleI)) && (m_DataValue == null))
                            {
                                isApplyDefaultValue = true;
                            }
                            else if (('\x20'.ToString() == Regex.Unescape(defaultRuleI)) && (m_DataValue != null) && (m_DataValue.TrimEnd().Length == 0))
                            {
                                isApplyDefaultValue = true;
                            }
                        }
                        //20060801 PL Add "break"
                        if (isApplyDefaultValue)
                        {
                            break;
                        }
                    }
                }
                //
                if (isApplyDefaultValue)
                {
                    m_DataValue = Regex.Unescape(m_DefaultValue);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error to set default value", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SetDataValueFormat()
        {
            TypeData.TypeDataEnum dataType;
            if (StrFunc.IsFilled(m_DataType))
            {
                dataType = TypeData.GetTypeDataEnum(m_DataType);
            }
            else
            {
                dataType = TypeData.TypeDataEnum.@string;
            }
            try
            {
                switch (dataType)
                {
                    case TypeData.TypeDataEnum.@bool:
                        bool boolValue = BoolFunc.IsTrue(m_DataValue);
                        if (IsIn)
                        {
                            m_DataValue = ObjFunc.FmtToISo(boolValue, dataType);
                        }
                        else
                        {
                            m_DataValue = boolValue.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    case TypeData.TypeDataEnum.@string:
                    case TypeData.TypeDataEnum.text:
                        break;
                    case TypeData.TypeDataEnum.integer:
                        int intValue = IntFunc.IntValue(m_DataValue);
                        //
                        if (IsIn)
                        {
                            m_DataValue = ObjFunc.FmtToISo(intValue, dataType);
                        }
                        else
                        {
                            m_DataValue = intValue.ToString();
                        }
                        break;
                    case TypeData.TypeDataEnum.@decimal:
                        // Handle formats with special symbols 
                        CultureInfo myCIclone = (CultureInfo)CultureInfo.InvariantCulture.Clone();
                        //PL 20101207 
                        #region DecimalSeparator
                        if (StrFunc.IsFilled(m_DataDecSeparator))
                        {
                            // REMARK: An empty decimal separator is not allowed. If the decimal separator is not specified, 
                            //         the CultureInfo.CurrentCulture decimal separator is used 

                            // We have defined two special symbols for decimal separators: 
                            // 1) '\x00': no separator. For example, 567.21 --> 56721,  567.2 --> 56720 (with a format expression like '0.00')
                            // 2) '\x20': blank separator. For example, 567.21 --> 567 21 
                            if (IsIn && ('\x00'.ToString() == Regex.Unescape(m_DataDecSeparator)))
                            {
                                int posDecimalSeparator = m_DataFormat.IndexOf(".");
                                //PL 20121015 Refactoring (utilisation séparateur décimal invariant ".") et ajout d'une recherche éventuelle du séparateur décimal ","
                                if (posDecimalSeparator < 0)
                                {
                                    //Séparateur décimal invariant "." non trouvé dans le format. Recherche d'un éventuel séparateur décimal ","
                                    posDecimalSeparator = m_DataFormat.IndexOf(",");
                                }

                                if (posDecimalSeparator >= 0)
                                {
                                    int len = m_DataValue.Length;
                                    if (len > 0)
                                    {
                                        string inputValue = m_DataValue;
                                        int numberOfDecimal = m_DataFormat.Length - posDecimalSeparator - 1;
                                        m_DataValue = inputValue.Substring(0, len - numberOfDecimal)
                                            + myCIclone.NumberFormat.NumberDecimalSeparator
                                            + inputValue.Substring(len - numberOfDecimal);
                                    }
                                }
                            }
                            else if ('\x20'.ToString() == Regex.Unescape(m_DataDecSeparator))
                            {
                                myCIclone.NumberFormat.NumberDecimalSeparator = ' '.ToString();
                            }
                            else
                            {
                                myCIclone.NumberFormat.NumberDecimalSeparator = m_DataDecSeparator;
                            }
                            // WARNING: 
                            // Lorsque le separateur de decimale est ',' alors par défaut le separateur de milliers n'est plus ',' mais string.empty
                            if ("," == myCIclone.NumberFormat.NumberDecimalSeparator)
                            {
                                myCIclone.NumberFormat.NumberGroupSeparator = string.Empty;
                            }
                        }
                        #endregion
                        #region GroupSeparator
                        if (StrFunc.IsFilled(m_DataGrpSeparator))
                        {
                            if ('\x00'.ToString() == Regex.Unescape(m_DataGrpSeparator))
                            {
                                myCIclone.NumberFormat.NumberGroupSeparator = string.Empty;
                            }
                            else if ('\x20'.ToString() == Regex.Unescape(m_DataGrpSeparator))
                            {
                                myCIclone.NumberFormat.NumberGroupSeparator = ' '.ToString();
                            }
                            else
                            {
                                myCIclone.NumberFormat.NumberGroupSeparator = m_DataGrpSeparator;
                            }
                        }
                        #endregion

                        Decimal decValue;
                        // Convert m_DataValue string into a decimal
                        try
                        {
                            if (IsIn)
                            {
                                decValue = DecFunc.DecValue(m_DataValue, myCIclone);
                            }
                            else
                            {
                                decValue = DecFunc.DecValue(m_DataValue, CultureInfo.InvariantCulture);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("'" + m_DataValue + "' is not a coherent " + dataType.ToString().ToUpper() +
                                (StrFunc.IsFilled(m_DataDecSeparator) ? " [DecSeparator: '" + m_DataDecSeparator + "', " : " [no DecSeparator specified, ") +
                                (StrFunc.IsFilled(m_DataGrpSeparator) ? " GrpSeparator: '" + m_DataGrpSeparator + "']" : "no GrpSeparator specified]"), ex);
                        }
                        //
                        // Round the decimal
                        if (m_DataRoundPrec >= 0 && StrFunc.IsFilled(m_DataRoundDir))
                        {
                            EFS_Round round = new EFS_Round(m_DataRoundDir, m_DataRoundPrec, decValue);
                            decValue = round.AmountRounded;
                        }
                        //              
                        if (IsIn)
                        {
                            m_DataValue = ObjFunc.FmtToISo(decValue, dataType);
                        }
                        else
                        {
                            // Convert the decimal number into a string according to the specified format
                            // 20080414 FI ticket 16125
                            // Dans le format Spheres permet l'utilisation des caractères spécifiées dans les zones paramétrés "symbol decimal" et "symbol regroupement" 
                            // Exemple Output : "2" est le nombre à exporter
                            // Format "#,##0d00",symbole décimal "d" 
                            // Spheres effectue le replace suivant format = format.replace("d",".")=> 2d00
                            //
                            // RD 20110526 
                            // Correction dans le cas où les séparateurs m_DataDecSeparator et m_DataDecSeparator sont exactement l’inverse de ceux de InvariantCulture
                            if (StrFunc.IsFilled(m_DataFormat))
                            {
                                if (StrFunc.IsFilled(m_DataDecSeparator))
                                {
                                    m_DataFormat = m_DataFormat.Replace(Regex.Unescape(m_DataDecSeparator), Regex.Unescape(m_DataDecSeparator) + "{dec}");
                                }
                                //
                                if (StrFunc.IsFilled(m_DataGrpSeparator))
                                {
                                    m_DataFormat = m_DataFormat.Replace(Regex.Unescape(m_DataGrpSeparator), Regex.Unescape(m_DataGrpSeparator) + "{grp}");
                                }
                                //
                                if (StrFunc.IsFilled(m_DataDecSeparator))
                                {
                                    m_DataFormat = m_DataFormat.Replace(Regex.Unescape(m_DataDecSeparator) + "{dec}", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                                }
                                //
                                if (StrFunc.IsFilled(m_DataGrpSeparator))
                                {
                                    m_DataFormat = m_DataFormat.Replace(Regex.Unescape(m_DataGrpSeparator) + "{grp}", CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator);
                                }
                            }
                            //
                            m_DataValue = decValue.ToString(m_DataFormat, myCIclone);
                            //
                            if (StrFunc.IsFilled(m_DataDecSeparator))
                            {
                                if ('\x00'.ToString() == Regex.Unescape(m_DataDecSeparator))
                                {
                                    // When '\x00' is specified, we format the number and then we take away the symbol
                                    m_DataValue = m_DataValue.Replace(myCIclone.NumberFormat.NumberDecimalSeparator, string.Empty);
                                }
                            }
                            //
                        }
                        break;
                    case TypeData.TypeDataEnum.datetime:
                    case TypeData.TypeDataEnum.date:
                    case TypeData.TypeDataEnum.time:
                        DateTime dtValue;
                        //Trim() Lors de  l'interprétation => par ex  "12/02/2002    " avec format (dd/MM/yyyy) pourra être interpréter  
                        if (StrFunc.IsFilled(m_DataFormat) && IsIn)
                        {
                            dtValue = new DtFuncML(m_Task.Cs, string.Empty, 0, 0, 0, null).StringToDateTime(m_DataValue.Trim(), m_DataFormat, false);
                        }
                        else
                        {
                            dtValue = new DtFuncML(m_Task.Cs, string.Empty, 0, 0, 0, null).StringToDateTime(m_DataValue.Trim(), false);
                        }
                        //	
                        if (DtFunc.IsDateTimeEmpty(dtValue))
                        {
                            throw new Exception("'" + m_DataValue + "' is not a coherent " + dataType.ToString().ToUpper() + (StrFunc.IsFilled(m_DataFormat) ? " (Format: '" + m_DataFormat + "')" : "(no format specified)"));
                        }
                        //
                        if (IsIn)
                        {
                            m_DataValue = ObjFunc.FmtToISo(dtValue, dataType);
                        }
                        else
                        {
                            m_DataValue = dtValue.ToString(m_DataFormat, CultureInfo.InvariantCulture);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(StrFunc.AppendFormat("Error to format data '{0}' in data type '{1}'", m_DataValue, dataType), ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210105 [25634] light refactoring
        protected void SetDataValueDelimiter()
        {
            try
            {
                if (IsIn)
                {
                    // FI 20210105 [25634] call RemoveDelimiter
                    m_DataValue = RemoveDelimiter(m_DataValue, m_DataCharDelimiter);
                }
                else
                {
                    string fillChar = string.Empty;
                    //
                    #region Vérifier si le Delimiteur est valide
                    string delimiter = string.Empty;
                    if (StrFunc.IsFilled(m_DataCharDelimiter))
                        delimiter = Regex.Unescape(m_DataCharDelimiter);

                    if (StrFunc.IsFilled(delimiter))
                    {
                        if (m_DataLength > 0)
                        {
                            if (m_DataLength - 2 > 0)
                            {
                                m_DataLength -= 2;
                            }
                            else
                            {
                                throw new Exception("Data length error");
                            }
                        }
                    }
                    #endregion
                    //
                    if (m_DataLength > 0)
                    {
                        if (StrFunc.IsFilled(m_DataFillChar))
                        {
                            fillChar = Regex.Unescape(m_DataFillChar);
                        }
                        else
                        {
                            fillChar = string.Empty;
                        }
                        //
                        if (StrFunc.IsFilled(m_DataValue))
                        {
                            if (m_DataValue.Length <= m_DataLength)
                            {
                                #region Alignement et remplissage
                                Cst.Alignment dataAlignment;
                                //
                                if (StrFunc.IsFilled(m_DataAlignment))
                                {
                                    dataAlignment = (Cst.Alignment)Enum.Parse(typeof(Cst.Alignment), m_DataAlignment, true);
                                }
                                else
                                {
                                    dataAlignment = Cst.Alignment.LEFT;
                                }
                                //
                                switch (dataAlignment)
                                {
                                    case Cst.Alignment.CENTER:
                                        {
                                            int dataLength = m_DataValue.Length;

                                            dataLength = m_DataLength - Math.DivRem(m_DataLength - dataLength, 2, out int resultRem);

                                            if (StrFunc.IsFilled(fillChar))
                                            {
                                                m_DataValue = m_DataValue.PadLeft(dataLength, fillChar.ToCharArray()[0]);
                                                m_DataValue = m_DataValue.PadRight(m_DataLength, fillChar.ToCharArray()[0]);
                                            }
                                            else
                                            {
                                                m_DataValue = m_DataValue.PadLeft(dataLength);
                                                m_DataValue = m_DataValue.PadRight(m_DataLength);
                                            }

                                            break;
                                        }
                                    case Cst.Alignment.LEFT:
                                        {
                                            if (StrFunc.IsFilled(fillChar))
                                            {
                                                m_DataValue = m_DataValue.PadRight(m_DataLength, fillChar.ToCharArray()[0]);
                                            }
                                            else
                                            {
                                                m_DataValue = m_DataValue.PadRight(m_DataLength);
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            if (StrFunc.IsFilled(fillChar))
                                            {
                                                m_DataValue = m_DataValue.PadLeft(m_DataLength, fillChar.ToCharArray()[0]);
                                            }
                                            else
                                            {
                                                m_DataValue = m_DataValue.PadLeft(m_DataLength);
                                            }
                                            break;
                                        }
                                }
                                #endregion
                            }
                            else
                            {
                                // Error
                                m_DataValue = m_DataValue.Substring(0, m_DataLength);
                                throw new Exception("Data length is upper then Parsing length parameter");
                            }
                        }
                        else
                        {
                            if (StrFunc.IsFilled(fillChar))
                            {
                                m_DataValue = string.Empty.PadLeft(m_DataLength, fillChar.ToCharArray()[0]);
                            }
                            else
                            {
                                m_DataValue = string.Empty.PadLeft(m_DataLength);
                            }
                        }
                    }
                    // FI 20210105 [25634] call ApplyDelimiter
                    //Appliquer le delimiteur éventuel
                    m_DataValue = ApplyDelimiter(m_DataValue, m_DataCharDelimiter);

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error to set delimiter and alignment", ex);
            }
        }


        /// <summary>
        /// Supprime le {pDelimiter} si la donnée {pInput} commence ou termine par {pDelimiter}
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pDelimiter"></param>
        /// <returns></returns>
        /// FI 20210105 [25634] Add
        protected static string RemoveDelimiter(string pInput, string pDelimiter)
        {
            string ret = pInput;

            string delimiter = string.Empty;
            if (StrFunc.IsFilled(pDelimiter))
                delimiter = Regex.Unescape(pDelimiter);

            if (StrFunc.IsFilled(delimiter) && StrFunc.IsFilled(pInput))
            {
                ret = ret.TrimStart(delimiter.ToCharArray());
                ret = ret.TrimEnd(delimiter.ToCharArray());
            }

            return ret;
        }

        /// <summary>
        /// Ajouter le {pDelimiter} à la donnée {pInput}
        /// </summary>
        /// <param name="pInput"></param>
        /// <param name="pDelimiter"></param>
        /// <returns></returns>
        /// FI 20210105 [25634] Add
        protected static string ApplyDelimiter(string pInput, string pDelimiter)
        {
            string ret = pInput;

            string delimiter = string.Empty;
            if (StrFunc.IsFilled(pDelimiter))
                delimiter = Regex.Unescape(pDelimiter);

            if (StrFunc.IsFilled(delimiter))
                ret = delimiter + pInput + delimiter;

            return ret;
        }

        #endregion
    }
}

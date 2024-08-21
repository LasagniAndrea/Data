#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using System;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
#endregion Using Directives

namespace EfsML.DynamicData
{
    /// <summary>
    /// StringDynamicData 
    /// Représentation "string" d'une donnée de tout type.
    /// La valeur de la donnée peut être calculée via une requête SQL ou une fonction de la librairie SpheresLib
    /// </summary>
    /// <remarks>
    /// Attention toute modification visible (Exemple ajout d'une propriété publique) 
    /// doit être reportée et commentée dans le XSD StringDynamicData.xsd
    /// </remarks>
    /// <seealso cref="EfsML/shemas/StringDynamicData.xsd"/>
    [XmlRoot(ElementName = "Data", IsNullable = true)]
    public class StringDynamicData
    {
        #region Members
        /// <summary>
        /// valeur de la donnée formattée
        /// </summary>
        [XmlTextAttribute()]
        public string value;

        /// <summary>
        /// Représente identifiant pour la donnée
        /// </summary>
        [XmlAttribute()]
        public string name;

        /// <summary>
        /// Représente le type de la donnée
        /// </summary>
        [XmlAttribute()]
        public string datatype;
        [XmlIgnore()]
        public bool datatypeSpecified;

        /// <summary>
        /// Représente le format de la donnée formattée
        /// </summary>
        [XmlAttribute()]
        public string dataformat;

        /// <summary>
        /// Représente la requête SQL qui permet d'évaluer la donnée
        /// </summary>
        [XmlElementAttribute("SQL", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataSQL sql;

        /// <summary>
        /// Représente la librairie de Spheres® qui permet d'évaluer la donnée
        /// </summary>
        [XmlElementAttribute("SpheresLib", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataSpheresLib spheresLib;



        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient true si la donnée peut être évalué
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool ContainsExpression
        {
            get { return ((null != value) || (null != spheresLib) || (null != sql)); }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public StringDynamicData()
        {
            datatype = TypeData.TypeDataEnum.@string.ToString();
            datatypeSpecified = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataType"></param>
        /// <param name="pName"></param>
        /// <param name="pValue"></param>
        public StringDynamicData(string pDataType, string pName, string pValue)
        {
            datatype = pDataType;
            datatypeSpecified = true;
            name = pName;
            value = pValue;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evalue la donnée à partir de la requête ou à partir le la librairie, à partir de l'élément value
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pdbTransaction"></param>
        /// <returns></returns>
        public string GetDataValue(string pCs, IDbTransaction pdbTransaction)
        {
            string retValue;
            if (sql != null)
            {
                try
                {
                    
                    retValue = sql.Exec(pCs, pdbTransaction);

                    if (StrFunc.IsFilled(dataformat))
                    {
                        if (TypeData.IsTypeDateOrDateTime(datatype))
                        {
                            //La valeur de retour est formattée selon l'attribut dataformat de la donnée
                            retValue = DtFunc.DateTimeToString(new DtFunc().StringDateTimeISOToDateTime(retValue), dataformat);
                        }
                    }
                }
                catch (Exception ex) { throw new Exception("[Error on SQL command: " + sql.command.ToUpper() + "]", ex); }
            }
            else if (spheresLib != null)
            {
                string functionName = spheresLib.function.ToUpper();
                //
                try
                {
                    #region SpheresLib
                    switch (functionName)
                    {
                        //
                        #region Spheres Functions for Date
                        case "GETDATESYS()":
                        case "GETDATETIMESYS()":
                        case "GETDATERDBMS()":
                        case "GETDATETIMERDBMS()":
                        case "GETUTCDATESYS()": // FI 20200901 [25468] add
                        case "GETUTCDATETIMESYS()":// FI 20200901 [25468] add
                        case "GETUTCDATERDBMS()": // FI 20200901 [25468] add
                        case "GETUTCDATETIMERDBMS()":// FI 20200901 [25468] add
                            spheresLib = new DataSpheresLib
                            {
                                function = functionName,
                                param = new ParamData[] { new ParamData("FORMAT", dataformat) }
                            };
                            retValue = spheresLib.Exec(pCs, pdbTransaction);
                            break;
                        //
                        case "ISDATE()":
                        case "ISNOTDATE()":
                        case "ISDATETIME()":
                        case "ISNOTDATETIME()":
                        case "ISDATESYS()":
                        case "ISNOTDATESYS()":
                        case "ISLESSTHANDATERDBMS()":
                        case "ISLESSOREQUALDATERDBMS()":
                        case "ISGREATEROREQUALDATERDBMS()":
                        case "ISGREATERTHANDATERDBMS()":
                            spheresLib = new DataSpheresLib
                            {
                                function = functionName,
                                param = new ParamData[] { new ParamData("FORMAT", dataformat), new ParamData("VALUE", value) }
                            };
                            retValue = spheresLib.Exec(pCs, pdbTransaction);
                            break;
                        //
                        case "ISHOLIDAY()":
                            spheresLib = new DataSpheresLib
                            {
                                function = functionName,
                                param = new ParamData[] { new ParamData("FORMAT", dataformat), new ParamData("VALUE", value) }
                            };
                            retValue = spheresLib.Exec(pCs, pdbTransaction);
                            break;
                        #endregion
                        //
                        case "ISNULL()":
                        case "ISNOTNULL()":
                        case "ISEMPTY()":
                        case "ISNOTEMPTY()":
                            spheresLib = new DataSpheresLib
                            {
                                function = functionName,
                                param = new ParamData[] { new ParamData("VALUE", value) }
                            };
                            retValue = spheresLib.Exec(pCs, pdbTransaction);
                            break;
                        //
                        default:
                            retValue = spheresLib.Exec(pCs, pdbTransaction);
                            break;
                        //
                    }
                    retValue = (StrFunc.IsFilled(retValue) ? retValue.Trim() : retValue);
                    #endregion
                }
                catch (Exception ex) { throw new Exception("[Error on SpheresLib function: " + functionName + "]", ex); }
            }
            else
            {
                #region Default : Data
                retValue = (StrFunc.IsFilled(value) ? value.Trim() : value);
                retValue = (retValue == "null" ? null : retValue);
                #endregion
            }
            //	
            return retValue;
        }

        /// <summary>
        /// Retourne un DataParameter qui contient la donnée
        /// <para>Le DataParameter sera utilisée dans une requête SQL paramétré</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTransaction"></param>
        /// <param name="pCommandType"></param>
        /// <param name="pSize"></param>
        /// <param name="pDataDirection"></param>
        /// <returns></returns>
        public DataParameter GetDataParameter(string pCs, IDbTransaction pTransaction, CommandType pCommandType, int pSize, ParameterDirection pDataDirection)
        {

            DataParameter dataParam = null;
            string dataName = name.ToUpper();
            string Value = GetDataValue(pCs, pTransaction);
            //
            if (TypeData.IsTypeBool(datatype))
            {
                dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.Boolean)
                {
                    Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : BoolFunc.IsTrue(Value))
                };
            }
            //
            else if (TypeData.IsTypeCursor(datatype))
            {
                //Warning: N'existe que pour Oracle
                //dataParam = new OracleParameter(pDataName, OracleType.Cursor);
                dataParam = new DataParameter();
                dataParam.InitializeOracleCursor(dataName, pDataDirection);
            }
            //
            else if (TypeData.IsTypeInt(datatype))
            {
                dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.Int64)
                {
                    Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : Convert.ToInt64(Value))
                };
            }
            //
            else if (TypeData.IsTypeDec(datatype))
            {
                dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.Decimal)
                {
                    Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : DecFunc.DecValue(Value, CultureInfo.InvariantCulture))
                };
            }
            //
            else if (TypeData.IsTypeDate(datatype))
            {
                dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.Date);
                if (StrFunc.IsFilled(dataformat))
                    dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : new DtFunc().StringToDateTime(Value, dataformat).Date);
                else
                    dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : new DtFunc().StringToDateTime(Value).Date);
            }
            //
            else if (TypeData.IsTypeDateTime(datatype))
            {
                dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.DateTime);
                if (StrFunc.IsFilled(dataformat))
                    dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : new DtFunc().StringToDateTime(Value, dataformat));
                else
                    dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : new DtFunc().StringToDateTime(Value));

                // FI 20211029 [XXXXX] new DtFunc().StringToDateTime convertie une date UTC en date local. 
                // Cette méthode n'a pour but de convertir des date. Une date en UTC doit rester en UTC
                // Il est donc nécessaire de revenir à une date UTC.

                // RD 20220422 [25990] Add StrFunc.IsFilled(value)
                if (StrFunc.IsFilled(value) && value.EndsWith("Z"))
                    dataParam.Value = ((DateTime)dataParam.Value).ToUniversalTime();
            }
            //
            else // String et autre
            {
                //PL 20111220 Résolution cas de IDROLEACTOR qui est de type Char(16) et qui en Oracle doit absolument être DbType.AnsiStringFixedLength
                //            NB: Résolution d'un pb survenu via Sphere I/O 
                //RD 20130116 [18358]
                bool isSpheresParam = (System.Enum.IsDefined(typeof(DataParameter.ParameterEnum), dataName));

                if (isSpheresParam)
                {
                    dataParam = DataParameter.GetParameter(pCs, (DataParameter.ParameterEnum)System.Enum.Parse(typeof(DataParameter.ParameterEnum), dataName));

                    // Si le type du paramètre change en Date, Decimal, ... alors le remettre à String (AnsiString)
                    if ((dataParam.DbType != DbType.String) &&
                        (dataParam.DbType != DbType.AnsiString) &&
                        (dataParam.DbType != DbType.AnsiStringFixedLength) &&
                        (dataParam.DbType != DbType.StringFixedLength))
                    {
                        isSpheresParam = false;
                    }
                }

                if (false == isSpheresParam)
                    dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.AnsiString);

                dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull : DataHelper.GetDBData(Value));
                //GP 20070201. Dans le cas d'un paramètre string ayant direction 'Output' || 'InputOutput' il faut assigner 
                //une dimension au paramètre, sinon l'appele à la SP Oracle retourne une exception 
                if (pSize > 0)
                    dataParam.Size = pSize;
                //	
                if ((dataParam.Size == 0) && (pDataDirection == ParameterDirection.Output) || (pDataDirection == ParameterDirection.InputOutput))
                    dataParam.Size = 4000;
            }
            //
            dataParam.Direction = pDataDirection;
            //
            return dataParam;

        }

        /// <summary>
        /// Retourne la donnée pour un usage SQL
        /// <para>Exemple Oracle: TO_DATE('{donnée évaluée}','YYYY-MM-DD HH24:MI:SS'); </para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTransaction"></param>
        /// <returns></returns>
        public string GetDataSQLValue(string pCs, IDbTransaction pTransaction)
        {

            string ret = string.Empty;
            string Value = GetDataValue(pCs, pTransaction);
            //
            if (TypeData.IsTypeBool(datatype))
                ret = DataHelper.SQLBoolean(BoolFunc.IsTrue(Value));
            else if (TypeData.IsTypeInt(datatype))
            {
                // PM 20091125 : ret = 'null' si vide
                ret = DataHelper.SQLNumber(Value);
            }
            else if (TypeData.IsTypeDec(datatype))
            {
                // PM 20091125 : ret = 'null' si vide
                ret = DataHelper.SQLNumber(Value);
            }
            else if (TypeData.IsTypeDate(datatype))
            {

                if (StrFunc.IsFilled(dataformat))
                    ret = DataHelper.SQLToDate(pCs, new DtFunc().StringToDateTime(Value, dataformat).Date);
                else
                    ret = DataHelper.SQLToDate(pCs, new DtFunc().StringToDateTime(Value).Date);

            }
            else if (TypeData.IsTypeDateTime(datatype))
            {
                if (StrFunc.IsFilled(dataformat))
                    ret = DataHelper.SQLToDateTime(pCs, new DtFunc().StringToDateTime(Value, dataformat));
                else
                    ret = DataHelper.SQLToDateTime(pCs, new DtFunc().StringToDateTime(Value));
            }
            else if (TypeData.IsTypeString(datatype) || TypeData.IsTypeText(datatype))
            {
                ret = DataHelper.SQLString(Value);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static string GetDynamicstring(string pCs, string pInput)
        {

            const string ElementStart = "<";
            const string ElementEnd = "/>";
            //
            const string SQLStart = ElementStart + "SQL ";
            const string SQLEnd = "</SQL>";
            //
            const string SpheresLibStart = ElementStart + "SpheresLib ";
            const string SpheresLibEnd = "</SpheresLib>";
            //
            const string DataStart = ElementStart + "Data ";
            const string DataEnd = "</Data>";
            //

            string retString = pInput;

            if (StrFunc.IsEmpty(retString))
                return retString;

            int elementStartPos = retString.IndexOf(ElementStart);

            if (elementStartPos == -1)
                return retString;

            if (false == (
                StrFunc.ContainsIn(retString, SpheresLibStart) ||
                StrFunc.ContainsIn(retString, SQLStart) ||
                StrFunc.ContainsIn(retString, DataStart)))
                return retString;

            string elementEnd;
            string elementString;

            Type elementType;
            while (elementStartPos > -1)
            {
                bool isSQL = false;
                bool isSpheresLib = false;
                bool isData = false;
                int elementEndPos;
                int elementStartPos2;

                if (retString.Substring(elementStartPos, SQLStart.Length) == SQLStart)
                {
                    isSQL = true;

                    elementEnd = SQLEnd;
                    elementEndPos = retString.IndexOf(elementEnd, elementStartPos);

                    // Si je trouve pas la balise fermante </SQL>
                    // ou bien celle trouvée appartient à une autre balise ouvrante
                    // ERROR
                    elementStartPos2 = retString.IndexOf(SQLStart, elementStartPos + SQLStart.Length);
                    if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Element '" + SQLStart + "' is not closed - '" + elementEnd + "' is expected ", "");
                }
                else if (retString.Substring(elementStartPos, SpheresLibStart.Length) == SpheresLibStart)
                {
                    isSpheresLib = true;

                    elementEnd = SpheresLibEnd;
                    elementEndPos = retString.IndexOf(elementEnd, elementStartPos);

                    // Si je trouve pas la balise fermante </SpheresLib> 
                    // ou bien celle trouvée appartient à une autre balise ouvrante
                    // je cherche la balise fermante />
                    elementStartPos2 = retString.IndexOf(SpheresLibStart, elementStartPos + SpheresLibStart.Length);
                    if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                    {
                        elementEnd = ElementEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);

                        // Si je trouve pas la balise fermante /> 
                        // ou bien celle trouvée appartient à une autre balise ouvrante
                        // ERROR
                        elementStartPos2 = retString.IndexOf(ElementStart, elementStartPos + SpheresLibStart.Length);
                        if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                            throw new Exception("Element '" + SpheresLibStart + "' is not closed - '" + ElementEnd + "' or '" + SpheresLibEnd + "' are expected ");
                    }
                }

                else if (retString.Substring(elementStartPos, DataStart.Length) == DataStart)
                {
                    isData = true;
                    //
                    elementEnd = DataEnd;
                    elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                    //
                    // Si je trouve pas la balise fermante </SpheresLib> 
                    // ou bien celle trouvée appartient à une autre balise ouvrante
                    // je cherche la balise fermante />
                    elementStartPos2 = retString.IndexOf(DataStart, elementStartPos + DataStart.Length);
                    if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                    {
                        elementEnd = ElementEnd;
                        elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                        //
                        // Si je trouve pas la balise fermante /> 
                        // ou bien celle trouvée appartient à une autre balise ouvrante
                        // ERROR
                        elementStartPos2 = retString.IndexOf(ElementStart, elementStartPos + DataStart.Length);
                        if ((elementEndPos == -1) || ((elementStartPos2 > -1) && (elementEndPos > elementStartPos2)))
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Element '" + DataStart + "' is not closed - '" + ElementEnd + "' or '" + DataEnd + "' are expected ", "");
                    }
                }

                else
                {
                    elementEnd = ElementEnd;
                    elementEndPos = retString.IndexOf(elementEnd, elementStartPos);
                }
                //
                elementString = retString.Substring(elementStartPos, elementEndPos + elementEnd.Length - elementStartPos);
                string dataResult;
                //
                if (isSQL || isSpheresLib || isData)
                {
                    StringDynamicData data = new StringDynamicData();
                    EFS_SerializeInfoBase serializeInfo;
                    if (isData)
                    {
                        elementType = typeof(StringDynamicData);
                        serializeInfo = new EFS_SerializeInfoBase(elementType, elementString);
                        data = (StringDynamicData)CacheSerializer.Deserialize(serializeInfo);
                    }
                    //
                    else if (isSQL)
                    {
                        elementType = typeof(DataSQL);
                        serializeInfo = new EFS_SerializeInfoBase(elementType, elementString);
                        DataSQL dataSQL = (DataSQL)CacheSerializer.Deserialize(serializeInfo);
                        data.datatype = TypeData.TypeDataEnum.@string.ToString();
                        data.sql = dataSQL;
                        //
                    }
                    else if (isSpheresLib)
                    {
                        elementType = typeof(DataSpheresLib);
                        serializeInfo = new EFS_SerializeInfoBase(elementType, elementString);
                        DataSpheresLib dataSpheresLib = (DataSpheresLib)CacheSerializer.Deserialize(serializeInfo);
                        data.datatype = TypeData.TypeDataEnum.@string.ToString();
                        data.spheresLib = dataSpheresLib;
                    }
                    //
                    dataResult = data.GetDataValue(pCs, null);
                    //
                    if (StrFunc.IsFilled(dataResult))
                    {
                        retString = retString.Replace(elementString, dataResult);
                    }
                    else
                        throw new Exception("No data found for: '" + elementString + "'");
                }
                else
                    throw new Exception("Element not supported: " + elementString);
                //
                elementStartPos = retString.IndexOf(ElementStart, elementStartPos + dataResult.Length);
            }
            return retString;

        }

        /// <summary>
        /// Retourne le résultat de la serialization 
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            // FI 20200205 [XXXXX] Usage de this.GetType() de maière à pouvoir sérializé une classe qui hérite de StringDynamicData
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(this.GetType(), this)
            {
                IsXMLTrade = false,
                IsWithoutNamespaces = true
            };
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
            return sb.ToString();
        }

        /// <summary>
        /// Remplace dans une string les mots clefs %%DA:{nom du dynamicArgument}.{method du dynamicArgument}%%  par leurs valeurs ou expressions
        /// <para>Retourne le résultat Obtenu</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData"></param>
        public string ReplaceInString(string pCS, string pData)
        {
            string ret = pData;
            //                          
            if (StrFunc.ContainsIn(ret, Cst.DA_START + name + "." + "GetDataSQLValue()" + Cst.DA_END))
                ret = ret.Replace(Cst.DA_START + name + "." + "GetDataSQLValue()" + Cst.DA_END, GetDataSQLValue(pCS, null));
            if (StrFunc.ContainsIn(ret, Cst.DA_START + name + "." + "GetDataValue()" + Cst.DA_END))
                ret = ret.Replace(Cst.DA_START + name + "." + "GetDataValue()" + Cst.DA_END, GetDataValue(pCS, null));
            if (StrFunc.ContainsIn(ret, Cst.DA_START + name + Cst.DA_END))
                ret = ret.Replace(Cst.DA_START + name + Cst.DA_END, GetDataValue(pCS, null));
            //
            return ret;
        }

        #endregion
    }
}

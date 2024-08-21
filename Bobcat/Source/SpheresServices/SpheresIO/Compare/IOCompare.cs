using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.IO;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresIO.SpheresObjets;
//
using IOCompareCommon;
using IOCompareCommon.DataContracts;
using IOCompareCommon.Interfaces;

namespace EFS.SpheresIO.Compare
{
    
    #region DataContract classes

    /// <summary>
    /// Class to store the comparison report highlights.
    /// </summary>
    /// <remarks>
    /// The IOTrack object contains the global result for a single compare process.
    /// Any IOTrack instance will generate a new line in the Spheres IOTrack table.
    /// </remarks>
    [DataContract(
        Name = DataHelper<IOTrack>.DATASETROWNAME,
        Namespace = DataHelper<IOTrack>.DATASETNAMESPACE)]
    internal sealed class IOTrack
    {
        private int m_IdIoTrack;

        /// <summary>
        /// IOTRACK.IDIOTRACK field
        /// </summary>
        [DataMember(Name = "IDIOTRACK", Order = 1)]
        public int IdIoTrack
        {
            get { return m_IdIoTrack; }
            set { m_IdIoTrack = value; }
        }

        string m_Message;

        /// <summary>
        /// Message - IOTRACK.MESSAGE field
        /// </summary>
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        DateTime m_DtBusiness;

        /// <summary>
        /// Business date - IOTRACK.DTDATA field
        /// </summary>
        public DateTime DtBusiness
        {
            get { return m_DtBusiness; }
            set { m_DtBusiness = value; }
        }

        /// Business date label 
        public const string PARAM_DTBUSINESS = "DTBUSINESS";
        /// Matching mode label
        public const string PARAM_MATCHINGMODE = "MATCHINGMODE";
        /// Matching net values or not
        public const string PARAM_ISMATCHINGNETQTY = "ISMATCHINGNETQTY";
        /// Matching only clearer side
        public const string PARAM_ISMATCHINGCLEARER = "ISMATCHINGCLEARER";
        /// Filter label
        public const string PARAM_FILTER = "Filters";
        /// Except label
        public const string PARAM_EXCEPT = "Exclusions";

        MatchStatus m_Status = MatchStatus.MATCH;

        /// <summary>
        /// Returned status - IOTRACK.STATUSRETURN field
        /// </summary>
        public MatchStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }

        Cst.MatchingMode m_MatchingMode = Cst.MatchingMode.SumOfQuantities;

        /// <summary>
        /// The current matching mode - IOTRACK.DATA1 field
        /// </summary>
        public Cst.MatchingMode MatchingMode
        {
            get { return m_MatchingMode; }
            set
            {
                m_MatchingMode = value;

                m_Datas[1][0] = Enum.GetName(typeof(Cst.MatchingMode), MatchingMode);
            }
        }

        /// <summary>
        /// Comparison data format 
        /// </summary>
        /// <value></value>
        public CompareOptions ExtDataFormat
        { get; set; }

        readonly string[][] m_Datas = new[]{ 
                    new []{"DATA1","DATA2","DATA3","DATA4","DATA5"}, 
                    new []{"","","","",""},
                    new []{"DATA1IDENT","DATA2IDENT","DATA3IDENT","DATA4IDENT","DATA5IDENT"},
                    new []{PARAM_MATCHINGMODE,PARAM_FILTER,PARAM_EXCEPT,"",""}
                };

        /// <summary>
        /// IOTRACK.DATAx an IOTRACK.DATAxIDENT fields
        /// </summary>
        /// <remarks>
        /// Inpur parameter for the AddIOTrackLog method
        /// </remarks>
        public string[][] Datas
        {
            get
            {
                return m_Datas;
            }
        }

        /// <summary>
        /// Set a single IOTRACK.DATAx value accorfing with the given IOTRACK.DATAxIDENT key.
        /// </summary>
        /// <param name="key">the IOTRACK.DATAxIDENT label</param>
        /// <param name="value">the IOTRACK.DATAx value relative to the IOTRACK.DATAxIDENT label</param>
        public void SetData(string key, object value)
        {
            switch (key)
            {
                // DATA1
                case PARAM_MATCHINGMODE:

                    m_Datas[1][0] = Enum.GetName(typeof(Cst.MatchingMode), MatchingMode);

                    break;

                // DATA2 and DATA3
                case PARAM_FILTER:
                case PARAM_EXCEPT:

                    // 1. building the  values

                    ParamsCollection parameters = (ParamsCollection)value;

                    string resFilter = String.Empty;
                    string resExcept = String.Empty;

                    foreach (TypedIOTaskParam param in parameters.Values)
                    {
                        string paramDescr = String.Empty;

                        if (param is FilterParam filterParam)
                        {
                            if (filterParam.TypedValue is DateTime time)
                                paramDescr = String.Format("{0} {1} {2}; ",
                                    filterParam.id, filterParam.StringOperator,
                                    time.ToShortDateString());
                            else
                                paramDescr = String.Format("{0} {1} {2}; ",
                                    filterParam.id, filterParam.StringOperator, filterParam.TypedValue);

                            resFilter = String.Concat(resFilter, paramDescr);
                        }
                        else if (param is ExceptParam && param.TypedValue != null && ((bool)param.TypedValue) == true)
                        {
                            paramDescr = String.Format("{0}; ", param.id);
                            resExcept = String.Concat(resExcept, paramDescr);
                        }
                    }

                    // 2. Updating the requested key only (the other is built but not used)

                    switch (key)
                    {
                        case PARAM_FILTER:
                            m_Datas[1][1] = resFilter;

                            break;
                        case PARAM_EXCEPT:
                            m_Datas[1][2] = resExcept;

                            break;
                    }

                    break;
            }
        }
    }

    #endregion DataContract classes

    #region Parameters

    /// <summary>
    /// IO Task parameter description class using strong-typed values
    /// </summary>
    /// <remarks>
    /// Strong typed representation for one IOTask parameter
    /// </remarks>
    public abstract class TypedIOTaskParam : IOTaskParamsParam
    {
        /// <summary>
        /// Data types list
        /// </summary>
        /// <remarks>
        /// WARNING: adding a new datatype in the TypedIOTaskParam class could generate anomalies during the query building.
        /// <see cref="QueryHelper"/>
        /// </remarks>
        protected enum SpheresDataTypes
        {
            Bool,
            Integer,
            String,
            Date,
            // MF 20120926 - Ticket 18149
            Decimal
        }

        /// <summary>
        /// Teh connection string used to build the TypedValueXSQL
        /// </summary>
        private readonly string m_connectionString = null;

        protected object typedValue = null;

        /// <summary>
        /// Get/set the strong-typed value.
        /// </summary>
        public object TypedValue
        {
            get { return typedValue; }
            set { typedValue = value; }
        }

        /// <summary>
        /// Get the SQL string description of the TypedValue property.
        /// </summary>
        public string TypedValueXSQL
        {
            get
            {
                string res = null;

                if (typedValue is string @string)
                    res = DataHelper.SQLString(@string);
                else if (typedValue is int @int)
                    res = Convert.ToString(@int, CultureInfo.InvariantCulture);
                else if (typedValue is bool boolean)
                    res = Convert.ToString(boolean, CultureInfo.InvariantCulture);
                else if (typedValue is DateTime time)
                    res = DataHelper.SQLToDate(m_connectionString, time);
                else if (typedValue is decimal @decimal)
                    res = Convert.ToString(@decimal, CultureInfo.InvariantCulture);

                return res;
            }
        }

        /// <summary>
        /// Get the sql cast command to string for the current parameter
        /// </summary>
        /// <remarks>compliant types: datetime</remarks>
        public string CastToStringXSQL
        {
            get
            {
                string res;

                // 1 date time conversion
                if (String.Compare(datatype, "date", true) == 0)
                {
                    res = DataHelper.SQLFormatColumnDateTimeWithMilli(m_connectionString, this.id, false);
                }
                // 2 specific cases for key construction
                else if (String.Compare(id, "TYP_COMPTE", true) != 0)
                {
                    string checklen = "15";

                    if (String.Compare(id, "NUM_PROPRIET", true) == 0)
                    {
                        checklen = "16";
                    }

                    if (String.Compare(id, "CAT_PROPRIET", true) == 0)
                    {
                        checklen = "10";
                    }

                    if (String.Compare(id, "PRODUIT", true) == 0)
                    {
                        checklen = "10";
                    }

                    if (String.Compare(id, "CD_DEV", true) == 0)
                    {
                        checklen = "3";
                    }

                    if (String.Compare(id, "INSTRT", true) == 0)
                    {
                        checklen = "1";
                    }

                    string substrdata = String.Format("'X' || {0} || replicate('X',{1})", id, checklen);

                    res = DataHelper.SQLSubstring(m_connectionString, substrdata, "1", String.Format("{0} + 1", checklen));
                }
                // 3 other cases...
                else
                    res = this.id;

                return res;
            }
        }

        // UNDONE 20101005 Use SQL parameters instead of the TypedValueXSQL property.
        /// <summary>
        /// Get the DataParameter instance to be used as parameter fro SQl query.
        /// </summary>
        //public DataParameter DataParameter
        //{
        //    get { return dataParameter; }
        //}

        /// <summary>
        /// Not used (for future use, <seealso cref="DataParameter"/>)
        /// </summary>
        // EG 20160404 Migration vs2013
        //DbType dbType = default(DbType);

        /// <summary>
        /// Get a new TypedIOTaskParam instance starting from a IOTaskParamsParam object. 
        /// The IOTaskParamsParam Value property will be converted according to the given IOTaskParamsParam datatype.
        /// </summary>
        /// <param name="param"></param>
        /// <exception cref="SystemArgumenException">
        /// Just a few types can be converted by the TypedIOTaskParam constructor,
        /// when a type is unknown an exception will be thrown.
        /// </exception>
        internal TypedIOTaskParam(IOTaskParamsParam pParam, string pConnectionString)
        {
            base.datatype = pParam.datatype;
            base.id = pParam.id;
            base.name = pParam.datatype;
            base.displayname = pParam.displayname;
            base.Value = pParam.Value;

            base.direction = null;
            base.returntype = null;

            Type strongtype = TypedIOTaskParam.CLRTypeFromSpheresType(datatype);

            m_connectionString = pConnectionString;

            try
            {
                if (strongtype == typeof(string))
                {
                    typedValue = Convert.ToString(Value);
                }
                else if (strongtype == typeof(bool))
                {
                    if (Value != null)
                        typedValue = Convert.ToBoolean(Value);
                }
                else if (strongtype == typeof(int))
                {
                    if (Value != null)
                        typedValue = Convert.ToInt32(Value);
                }
                else if (strongtype == typeof(DateTime))
                {
                    if (Value != null)
                        typedValue = Convert.ToDateTime(Value);
                }
                else if (strongtype == typeof(Decimal))
                {
                    if (Value != null)
                    {
                        Decimal.TryParse(Value, out decimal tempTypedValue);
                        typedValue = tempTypedValue;
                    }
                }
            }
            catch (InvalidCastException)
            { }
            
        }

        // UNDONE 20101005 Use SQL parameters instead of the TypedValueXSQL property.
        //internal bool BuildDataParameter(string connectionString, bool overwrite)
        //{
        //    bool created = false;

        //    if (dataParameter == null || overwrite)
        //    {
        //        Type strongtype = TypedIOTaskParam.TypeFromQualifiedName(base.datatype);

        //        this.dataParameter = new DataParameter(connectionString, base.id, this.dbType);

        //        created = true;
        //    }

        //    return created;
        //}

        /// <summary>
        /// Get the CLR Type of the given Spheres type.
        /// </summary>
        /// <param name="datatype">
        /// The data type name. Admitted values: SpheresDataTypes to string elements.
        /// </param>
        /// <returns></returns>
        protected static Type CLRTypeFromSpheresType(string pDatatype)
        {
            string qualifiedName = null;
            SpheresDataTypes spheresDataType = (SpheresDataTypes)Enum.Parse(typeof(SpheresDataTypes), pDatatype, true);
            switch (spheresDataType)
            {
                case SpheresDataTypes.Bool:
                    qualifiedName = typeof(bool).AssemblyQualifiedName;
                    break;
                case SpheresDataTypes.Integer:
                    qualifiedName = typeof(int).AssemblyQualifiedName;
                    break;
                case SpheresDataTypes.String:
                    qualifiedName = typeof(string).AssemblyQualifiedName;
                    break;
                case SpheresDataTypes.Date:
                    qualifiedName = typeof(DateTime).AssemblyQualifiedName;
                    break;
                // MF 20120926 - Ticket 18149
                case SpheresDataTypes.Decimal:
                    qualifiedName = typeof(Decimal).AssemblyQualifiedName;
                    break;
            }

            Type strongtype = Type.GetType(qualifiedName);

            return strongtype;
        }

    }

    /// <summary>
    /// Except Parameter class. 
    /// Used to exclude certain elements from the comparison key.
    /// </summary>
    /// <remarks>
    /// It is used to enrich the sql "group by" condition when the comparison model needs aggregation.
    /// IOTaskParamsParam name format : "xxx_EXCEPT".
    /// </remarks>
    public class ExceptParam : TypedIOTaskParam, IConfigurationParameter
    {

        public ExceptParam(IOTaskParamsParam pParam, ConfigurationParameter pConfigurationParameter, string pConnectionString) :
            base(pParam, pConnectionString)
        {
            m_configurationParameter = pConfigurationParameter;
        }

        #region ICompareParameter Membres

        ConfigurationParameter m_configurationParameter = null;

        public ConfigurationParameter ConfigurationParameter
        {
            get { return m_configurationParameter; }
            set { m_configurationParameter = value; }
        }

        #endregion
    }

    // MF 20120926 - Ticket 18149
    /// <summary>
    /// 
    /// </summary>
    public class StaticValueParam : TypedIOTaskParam
    {
        public StaticValueParam(IOTaskParamsParam pParam, string pId, string pType, string pConnectionString) :
            base(pParam, pConnectionString)
        {
            Id = pId;

            Type = pType;
        }

        [DataMember(Order = 1)]
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// Type of the parameter data (int/decimal/etc)
        /// </summary>
        [DataMember(Order = 2)]
        public string Type
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Filter parameter. 
    /// Used to filter the elements inside the sets (either external and internal) to be matched.
    /// </summary>
    /// <remarks>
    /// It is used to enrich the SQL WHERE condition. 
    /// IOTaskParamsParam name format: "xxx_FILTER".
    /// </remarks>
    public class FilterParam : ExceptParam
    {
        // UNDONE 20100803 MF actually just the Equals operator is supported.
        // RD 20131129 [19272] Compare clearer data
        // - Add "IN" operator
        /// <summary>
        /// Logical operators used by a filter parameter.
        /// </summary>
        public enum Operator
        {
            Equal,
            IN,
            //Different,
            //Minor,
            //Greater,
            //MinorOrEqual,
            //GreaterOrEqual
        }

        private readonly Operator m_Operator = Operator.Equal;

        /// <summary>
        /// Get the string symbol of the current operator.
        /// </summary>
        public string StringOperator
        {
            get
            {
                string res = "=";

                if (m_Operator == Operator.Equal)
                    res = "=";
                else if (m_Operator == Operator.IN)
                    res = "in";

                return res;
            }
        }

        /// <summary>
        /// Initialise a new FilterParam instance
        /// </summary>
        /// <param name="param"></param>
        /// <param name="configurationParameter"></param>
        /// <param name="op">the operator string description. Admitted values: Operator to string elements</param>
        /// <param name="connectionString"></param>
        public FilterParam(IOTaskParamsParam pParam, ConfigurationParameter pConfigurationParameter, string pOp, string pConnectionString) :
            this(pParam, pConfigurationParameter, (Operator)Enum.Parse(typeof(Operator), pOp), pConnectionString)
        {
        }

        /// <summary>
        /// Initialise a new FilterParam instance
        /// </summary>
        /// <param name="param"></param>
        /// <param name="configurationParameter"></param>
        /// <param name="op"></param>
        /// <param name="connectionString"></param>
        public FilterParam(IOTaskParamsParam pParam, ConfigurationParameter pConfigurationParameter, Operator pOp, string pConnectionString) :
            base(pParam, pConfigurationParameter, pConnectionString)
        {
            m_Operator = pOp;
        }
    }

    /// <summary>
    /// Parameters collection for the compâre process.
    /// </summary>
    public class ParamsCollection : IDictionary<string, TypedIOTaskParam>
    {
        #region Constants

        public const string EXCEPT = "EXCEPT";

        public const string FILTER = "FILTER";

        public const string STATICVALUE = "STATICVALUE";

        #endregion Constants

        #region Additional parameters description class

        /// <summary>
        /// DataContract class for the XML configuration file
        /// </summary>
        [DataContract(
            Name = "Configuration",
            Namespace = "")]
        internal class ConfigurationCompare
        {
            private List<ConfigurationTemplate> m_templates;

            /// <summary>
            /// Templates list (SumOfQuantities/SingleQuantity/etc)
            /// </summary>
            /// <remarks>
            /// Any template is a collection of ConfigurationParameter objects, type EXCEPT.
            /// Any ConfigurationParameter inside of a template can NOT be excluded 
            /// unlike the others EXCEPT parameters stocked in the Parameters property.
            /// </remarks>
            [DataMember(Order = 1)]
            public List<ConfigurationTemplate> Templates
            {
                get { return m_templates; }
                set { m_templates = value; }
            }

            private List<ConfigurationParameter> m_parameters;

            /// <summary>
            /// Common parameters list: type FILTER and EXCEPT.
            /// </summary>
            [DataMember(Order = 2)]
            public List<ConfigurationParameter> Parameters
            {
                get { return m_parameters; }
                set { m_parameters = value; }
            }
        }

        /// <summary>
        /// DataContract class for a template ("template": comparison model)
        /// </summary>
        [DataContract(
            Name = "Template",
            Namespace = "")]
        internal class ConfigurationTemplate
        {
            private string m_id;

            [DataMember(Order = 1)]
            public string Id
            {
                get { return m_id; }
                set { m_id = value; }
            }

            private List<ConfigurationParameter> m_parameters;

            /// <summary>
            /// ConfigurationParameter collection
            /// </summary>
            /// <remarks>
            /// A ConfigurationTemplate is a collection of ConfigurationParameter objects, type EXCEPT.
            /// Any ConfigurationParameter inside of a ConfigurationTemplate obkect can NOT be excluded 
            /// unlike the others EXCEPT parameters stocked in the ConfigurationCompare.Parameters property.
            /// </remarks>
            [DataMember(Order = 2)]
            public List<ConfigurationParameter> Parameters
            {
                get { return m_parameters; }
                set { m_parameters = value; }
            }
        }

        #endregion Additional parameters description class

        /// <summary>
        /// The parameters collection
        /// </summary>
        private readonly Dictionary<string, TypedIOTaskParam> m_Params = new Dictionary<string, TypedIOTaskParam>();

        /// <summary>
        /// RegEx to check the format of each IOTaskParamsParam object name.
        /// </summary>
        readonly Regex regSuffix = new Regex("^(EXCEPT|FILTER|STATICVALUE)_(.*)$", RegexOptions.IgnoreCase);


        /// <summary>
        /// Build a new empty object
        /// </summary>
        internal ParamsCollection() {}

        /// <summary>
        /// Initialise a new TypedIOTaskParam parameters collection.
        /// </summary>
        /// <param name="pTask">current io task</param>
        /// <param name="pMode">current comparison template</param>
        internal void Init(Task pTask, Cst.MatchingMode pTemplate)
        {
            // 1. Deserialize the xml configuration file
            ConfigurationCompare configuration = GetConfiguration(pTask);

            // conf parameters dictionary, used as support to check id existence in the TypedIOTaskParam parameters creation process
            Dictionary<string, ConfigurationParameter> configurationParameters = new Dictionary<string, ConfigurationParameter>();

            // 2. Adding the common configuration parameters (inside of the "Configuration/Parameters" nodes list) 
            //      to the conf parameters dictionary
            foreach (ConfigurationParameter parameter in configuration.Parameters)
            {
                if (!configurationParameters.ContainsKey(parameter.Id))
                    configurationParameters.Add(parameter.Id, parameter);
            }

            // 3. Building the additional "except" io task parameters, 
            //      according with the given template (Quantity_OneAgainstOne, SumOfQuantities).
            // RD à voir avec MF
            // Pourquoi charger un array pour le transformer après en List<>
            IOTaskParamsParam[] additionalParams = BuildAdditionalParameters(pTemplate, configuration, out ConfigurationTemplate currentTemplate);

            // 4. Adding the specific template parameters (inside of the "Configuration/Templates/Template/Id" nodes list) 
            //      to the to the conf parameters dictionary
            foreach (ConfigurationParameter parameter in currentTemplate.Parameters)
            {
                if (!configurationParameters.ContainsKey(parameter.Id))
                    configurationParameters.Add(parameter.Id, parameter);
            }

            // 5. Search valid input task parameters
            // io task parameters temporary collection (where we add all the not null io task parameters)
            List<IOTaskParamsParam> tempParamsCollection = new List<IOTaskParamsParam>(additionalParams);

            FilterIoTaskParameters(pTask.IoTask.parameters.param, configurationParameters, tempParamsCollection);

            // 6. For each input parameters, or built on the fly, we build the relative strong typed parameter (TypedIOTaskParam)
            foreach (IOTaskParamsParam ioparameter in tempParamsCollection)
            {
                // RD à voir avec MF
                // Pourquoi la validation avec la RegEx? pour juste parser le param ?
                // Pour exclure les partamètre qui viennet de l'interface et que ne sont pas 
                Match match = regSuffix.Match(ioparameter.id);
                if (match.Success)
                {
                    string suffix = match.Groups[1].Value;
                    string id = match.Groups[2].Value;
                    ioparameter.id = id;

                    ConfigurationParameter configurationParameter = null;

                    if (configurationParameters.ContainsKey(ioparameter.id))
                        configurationParameter = configurationParameters[ioparameter.id];

                    ParameterFactory(id, suffix, ioparameter, configurationParameter, "Equal", pTask.Cs);
                }
            }
        }

        private static void FilterIoTaskParameters(
            IOTaskParamsParam[] pIoTaskParams, 
            Dictionary<string, ConfigurationParameter> configurationParameters, 
            List<IOTaskParamsParam> tempParamsCollection)
        {
            foreach (IOTaskParamsParam ioParameter in pIoTaskParams)
            {
                // IOTaskParamsParam with null values will not be used in the compare process.
                if (ioParameter.Value != null)
                {
                    ioParameter.id = ioParameter.id.ToUpperInvariant();

                    // id property normalization
                    if (configurationParameters.ContainsKey(ioParameter.id))
                    {
                        ConfigurationParameter confParameter = configurationParameters[ioParameter.id];
                        ioParameter.id = String.Format("{0}_{1}", confParameter.Type, ioParameter.id);
                    }

                    tempParamsCollection.Add(ioParameter);
                }
            }
        }

        /// <summary>
        /// Build the additional EXCEPT io task parameters according to the given matching mode.
        /// </summary>
        /// <param name="pMode">Current comparison mode</param>
        /// <param name="opCurrentTemplate">output parameter containing the current comparison template</param>
        /// <param name="pConfiguration">current configuration object, issued from Xml configuration file deserialization</param>
        /// <returns>The additional parameters set or an empty set</returns>
        internal IOTaskParamsParam[] BuildAdditionalParameters(Cst.MatchingMode pMode, ConfigurationCompare pConfiguration,
            out ConfigurationTemplate opCurrentTemplate)
        {
            // RD à voir avec MF
            // Pourquoi Except?
            // Parceque dans le template, il y'a que des param Except
            IOTaskParamsParam[] exceptParameters = null;

            opCurrentTemplate = null;

            foreach (ConfigurationTemplate template in pConfiguration.Templates)
            {
                Cst.MatchingMode idTemplate = (Cst.MatchingMode)Enum.Parse(typeof(Cst.MatchingMode), template.Id);

                // Searching the template relative to the current matching mode in order to build the parameters who define the template itself.
                if (idTemplate == pMode)
                {
                    opCurrentTemplate = template;

                    exceptParameters = new IOTaskParamsParam[template.Parameters.Count];

                    for (int idx = 0; idx < template.Parameters.Count; idx++)
                    {
                        IOTaskParamsParam newParam = new IOTaskParamsParam
                        {
                            direction = null,
                            displayname = null,
                            returntype = null,
                            // Hint: SQLInfos[0] should not raise any exception because the configuration file XSD (minOccurs = 1)
                            datatype = template.Parameters[idx].SQLInfos[0].SQLType,
                            id = String.Format("{0}_{1}", template.Parameters[idx].Type, template.Parameters[idx].Id),
                            Value = null
                        };

                        if (newParam.datatype == String.Empty)
                        {
                            throw new ArgumentNullException(
                                newParam.id,
                                @"The current parameter does not have any type associated. Check the Xml configuration file.");
                        }

                        exceptParameters[idx] = newParam;
                    }
                }
            }

            // When we are using a template without any parameters (or the template itself is unknown)

            if (exceptParameters == null)
            {
                exceptParameters = new IOTaskParamsParam[0];
            }

            if (opCurrentTemplate == null)
            {
                opCurrentTemplate = new ConfigurationTemplate
                {
                    Parameters = new List<ConfigurationParameter>()
                };
            }

            // return the built io task parameters
            return exceptParameters;
        }

        /// <summary>
        /// Load the XML configuration file (CompareParameters.xml)
        /// </summary>
        /// <returns>a nex ConfigurationCompare representign the file contents</returns>
        /// FI 20160804 [Migration TFS] Modify
        ConfigurationCompare GetConfiguration(Task pTask)
        {
            XmlDocument doc = new XmlDocument();

            // RD à voir avec MF
            // - Structure du fichier et Utilisation
            // - EVENT.TAXCOMAMT 
            // Car TAXCOMAMT vient d'un événement, et ce param n'est pas utilisé dans la requête

            // FI 20160804 [Migration TFS] nouveau path pour m_XMLConfigurationPath  
            string m_XMLConfigurationPath = @".\IOCompare\CompareParameters.xml";

            m_XMLConfigurationPath = pTask.Process.AppInstance.GetFilepath(m_XMLConfigurationPath);

            if (!File.Exists(m_XMLConfigurationPath))
                throw new FileLoadException("File does not exist", m_XMLConfigurationPath);

            doc.Load(m_XMLConfigurationPath);


            ConfigurationCompare configuration = null;

            using (Stream xmlinput = new MemoryStream(Encoding.UTF8.GetBytes(doc.OuterXml)))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(ConfigurationCompare), "Configuration", "");

                configuration = (ConfigurationCompare)ser.ReadObject(xmlinput);
            }

            return configuration;
        }

        /// <summary>
        /// Build a new TypedIOTaskParam parameter and add that to the internal collection
        /// </summary>
        /// <param name="id">parameter identifier (unique)</param>
        /// <param name="suffix">
        /// TypedIOTaskParam type (EXCEPT or FILTRE). this string is used to choose which TypedIOTaskParam sub-type to istantiate.
        /// <remarks></remarks>
        /// </param>
        /// <param name="ioparameter">the relative IOTaskParamsParam object (built on the fly or extracted from the IOTask object)</param>
        /// <param name="configurationParameter">the configuration set of the new TypedIOTaskParam object</param>
        /// <param name="op">the logical operator used by the new TypedIOTaskParam object (null for EXCEPT)</param>
        /// <param name="connectionString">the data base connection string</param>
        void ParameterFactory(string id, string suffix, IOTaskParamsParam ioparameter, ConfigurationParameter configurationParameter,
            string op, string connectionString)
        {
            switch (suffix)
            {
                case EXCEPT:
                    Add(id, new ExceptParam(ioparameter, configurationParameter, connectionString));
                    break;
                case FILTER:
                    Add(id, new FilterParam(ioparameter, configurationParameter, op, connectionString));
                    break;
                case STATICVALUE:
                    Add(id, new StaticValueParam(ioparameter, id, STATICVALUE, connectionString));
                    break;
                default:
                    break;
            }
        }

        #region IDictionary<string,Param> Membres

        public void Add(string key, TypedIOTaskParam value)
        {
            if (ContainsKey(key))
                Remove(key);

            m_Params.Add(key, value);

        }

        public bool ContainsKey(string key)
        {
            return m_Params.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return m_Params.Keys; }
        }

        public bool Remove(string key)
        {
            return m_Params.Remove(key);
        }

        public bool TryGetValue(string key, out TypedIOTaskParam value)
        {
            return m_Params.TryGetValue(key, out value);
        }

        public ICollection<TypedIOTaskParam> Values
        {
            get { return m_Params.Values; }
        }

        public TypedIOTaskParam this[string key]
        {
            get
            {
                if (ContainsKey(key))
                    return m_Params[key];
                else
                    return null;
            }
            set
            {
                Add(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,Param>> Membres

        public void Add(KeyValuePair<string, TypedIOTaskParam> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            m_Params.Clear();
        }

        public bool Contains(KeyValuePair<string, TypedIOTaskParam> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, TypedIOTaskParam>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<string, TypedIOTaskParam> pair in m_Params)
                array[arrayIndex++] = pair;
        }

        public int Count
        {
            get { return m_Params.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, TypedIOTaskParam> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,Param>> Membres

        public IEnumerator<KeyValuePair<string, TypedIOTaskParam>> GetEnumerator()
        {
            return m_Params.GetEnumerator();
        }

        #endregion

        #region IEnumerable Membres

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_Params.GetEnumerator();
        }

        #endregion

    }

    #endregion Parameters

    #region QueryHelper

    /// <summary>
    /// Helper class to build a dynamic sql query string according with the user parameters
    /// </summary>
    public sealed class QueryHelper
    {
        #region Properties

        // RD 20131129 [19272] Compare clearer data
        // - Add m_Cs member    
        readonly string m_Cs;

        /// <summary>
        /// The user parameters collection
        /// </summary>
        readonly ParamsCollection m_parameters = null;

        /// <summary>
        /// Regex searching for tables and relative aliases
        /// </summary>(from|inner join|outer join)\s+(\w+\.?\w+)\s+(?!on)(\w+)
        //Regex m_regExTables = new Regex(@"(from|inner join|outer join)\s+(\w+\.?\w+)\s+(\w+)",
        //    RegexOptions.IgnoreCase | RegexOptions.Multiline);
        // MF 20112401 exclusions table without alias (?!on)
        readonly Regex m_regExTables = new Regex(@"(from|inner join|outer join)\s+(\w+\.?\w+)\s+(?!on|inner join|outer join)(\w+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Regex searching for fields alias
        /// </summary>
        // RD à voir avec MF / je pense que (as "x"from) va matcher
        readonly Regex m_regExFieldsAlias = new Regex(@"as\s+(""?\w+""?)\s*(,|\s*from)",
            RegexOptions.IgnoreCase);

        /// <summary>
        /// Regex searching for the where clausule
        /// </summary>
        readonly Regex m_regExWhere = new Regex(@"\s+where\s+",RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Regex searching an SQLinfo/XPath node content parsing path and attributes 
        /// </summary>
        readonly Regex m_regExParseParameterXPath = new Regex(@"([\w/:\(\)]+)/@([\w]+)",RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Search inside the request the special IoCOmpare tag IOCOMPARE_NOTCOMPLIANTFILTERS. you need this tag when you can not put
        /// the WHERE conditions at the very end of your request. In this case the IOCOMPARE_NOTCOMPLIANTFILTERS 
        /// must be placed after the end of your anticipated
        /// WHERE conditions, in order to be add afterwards the custom WHERE conditions.
        /// </summary>
        readonly Regex m_RegExParseNotCompliantFilterTag = new Regex(@"(/\*IOCOMPARE_NOTCOMPLIANTFILTERS\*/)",RegexOptions.IgnoreCase);

        #endregion Properties

        /// <summary>
        /// Build an helper instance that will use the given parameters set
        /// </summary>
        /// <param name="parameters"></param>
        public QueryHelper(ParamsCollection parameters, string pCs)
        {
            m_parameters = parameters;
            m_Cs = pCs;
        }

        #region Interface

        /// <summary>
        /// Build the query string according to the given FILTER/EXCEPT parameters
        /// </summary>
        /// <param name="pQuerycore">the core of the query without any filter or group by</param>
        /// <param name="opDecoratedquery">output decorated query including filters and groups</param>
        public void BuildQuery(string pQuerycore, out string opDecoratedquery)
        {

            opDecoratedquery = pQuerycore;

            if (m_parameters != null)
                opDecoratedquery = DecorateQuery(opDecoratedquery);

        }

        /// <summary>
        /// Build all the needed sql parameters in order to execute the input query
        /// </summary>
        /// <param name="pQuery">query where we search the sql parameters in</param>
        /// <param name="pCS">current connection string</param>
        /// <returns></returns>
        public IDbDataParameter[] BuildOptionalSqlParameters(string pQuery, string pCS)
        {
            List<IDbDataParameter> dbParameters = new List<IDbDataParameter>();

            foreach (KeyValuePair<string, TypedIOTaskParam> parameter in m_parameters)
            {

                Regex regExParseParameterSQL = new Regex(
                    String.Format("@(?:{0})", parameter.Value.id),
                    RegexOptions.IgnoreCase);

                if (parameter.Value is FilterParam && regExParseParameterSQL.IsMatch(pQuery))
                {
                    DbType dbType;
                    switch (parameter.Value.datatype)
                    {
                        case "date":

                            dbType = DbType.Date;
                            break;

                        case "integer":

                            dbType = DbType.Int32;
                            break;

                        default:

                            dbType = DbType.String;
                            break;
                    }

                    DataParameter sqlParameter = new DataParameter(pCS, CommandType.Text, parameter.Value.id, dbType)
                    {
                        Value = parameter.Value.TypedValue
                    };

                    dbParameters.Add(sqlParameter.DbDataParameter);

                }
            }

            return dbParameters.ToArray();
        }

        #endregion Interface

        #region Private methods

        /// <summary>
        /// Decorate the query according to the given filter/except parameters
        /// </summary>
        /// <param name="pQuerytodecorate">the core of the query without any filters or group by</param>
        /// <returns>the query decorated including filters and groups</returns>
        private string DecorateQuery(string pQuerytodecorate)
        {

            // 1. Get all tables and aliases of the query core

            MatchCollection matches = m_regExTables.Matches(pQuerytodecorate);

            StringDictionary tables = new StringDictionary();

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    if (!tables.ContainsKey(match.Groups[2].Value))
                    {
                        tables.Add(match.Groups[2].Value, match.Groups[3].Value);
                    }
                }
            }
            else
                throw new ArgumentException(
                    String.Format("The input query is not well-formed: {0}. No tables/aliases found", pQuerytodecorate),
                    "querytodecorate");

            // 2. Get all the field aliases of the query core

            matches = m_regExFieldsAlias.Matches(pQuerytodecorate);

            string[] fieldsalias = new string[matches.Count];

            for (int idx = 0; idx < fieldsalias.Length; idx++)
                fieldsalias[idx] = matches[idx].Groups[1].Value;

            Array.Sort<string>(fieldsalias);

            string FORMATFILTEREDQUERY = @"
                    {0} {1}
                    ";

            // 3. build the where condition

            string filters = BuildFilters(tables, pQuerytodecorate, out bool notCompliantFilters);

            string queryWithFilters =
                notCompliantFilters ?
                    m_RegExParseNotCompliantFilterTag.Replace(pQuerytodecorate, filters)
                    :
                    String.Format(FORMATFILTEREDQUERY, pQuerytodecorate, filters);

            // 4. build the group/keys by clause 

            string FORMATENCAPSULATEDQUERY = @"
                    {0}
                    {1} 
                    {2}
                    ";


            Aggregate(tables, fieldsalias, out string headerAggregate, out string footerAggregate);

            string queryWithFiltersEncapsulated = String.Format(FORMATENCAPSULATEDQUERY,
                headerAggregate, queryWithFilters, footerAggregate);

            //// 5. Add supplementary keys (Optional)
            //string queryWithFiltersAndKeys = BuildKeysOracle(tables, fieldsalias, queryWithFilters, pQuerytodecorate);

            //if (queryWithFiltersAndKeys != null)
            //{
            //    // 6. Build the high level query to manage supplementary keys with grouped datas 
            //    queryWithFiltersEncapsulated = HigherLevelSelect(tables, fieldsalias, queryWithFiltersEncapsulated, queryWithFiltersAndKeys);
            //}

            return queryWithFiltersEncapsulated;
        }


        /// <summary>
        /// Build the complete SQL WHERE condition
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pQuerytodecorate">query core string</param>
        /// <returns>the WHERE condition as string</returns>
        private string BuildFilters(StringDictionary pTables, string pQuerytodecorate, out bool pNotCompliantFilters)
        {
            Match match = m_regExWhere.Match(pQuerytodecorate);
            SQLWhere sqlWhere = new SQLWhere(match.Success);

            foreach (KeyValuePair<string, TypedIOTaskParam> parameter in m_parameters)
            {
                if (parameter.Value is FilterParam filterParam)
                {
                    // searching the right alias (stored in the dictionary) and the relative infoSQl node
                    List<ConfigurationSQLInfo> infoSQLs = GetSQLInfoFilter(pTables, filterParam.ConfigurationParameter.SQLInfos, out string alias);

                    if (alias != null)
                    {
                        foreach (ConfigurationSQLInfo infoSQL in infoSQLs)
                            if (infoSQL.XPath == String.Empty)
                            {
                                // RD 20131129 [19272] Compare clearer data
                                // - Manage new membre ConfigurationSQLInfo.SQLOperator
                                // - Manage new Operator "IN"
                                string stringOperator = filterParam.StringOperator;
                                if (StrFunc.IsFilled(infoSQL.SQLOperator))
                                    stringOperator = infoSQL.SQLOperator;

                                if (stringOperator.ToUpper() == FilterParam.Operator.IN.ToString())
                                {
                                    string[] dataList = filterParam.Value.Split(';');
                                    TypeData.TypeDataEnum spheresDataType = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), filterParam.datatype, true);

                                    sqlWhere.Append(DataHelper.SQLColumnIn(m_Cs,
                                        String.Format(@"{0}.{1}", alias, infoSQL.SQLField),
                                        dataList, spheresDataType));
                                }
                                else
                                {
                                    sqlWhere.Append(String.Format(@"{0}.{1} {2} {3}",
                                        alias, infoSQL.SQLField, stringOperator, filterParam.TypedValueXSQL));
                                }
                                break;
                            }
                            else
                            // 2. build a filter using a field XML
                            {
                                if (!(filterParam.TypedValue is string))
                                    throw new NotSupportedException(@"
                                        Configuration error: 
                                        found an unknown type for a parameter providing an XPath, just ""string"" types are supported");

                                if (pQuerytodecorate.Contains(infoSQL.XPath))
                                {

                                    // WARNING 20100921 using node() could cause returning a 0 rows dataset 
                                    // string xQueryExistCondition = m_regExParseParameterXPath.Replace(infoSQL.XPath, @"exist('($1[@$2=""{0}""]/node())')");
                                    // RD à voir avec MF / C'est quoi $1 et $2
                                    // C'est les numéro de groupes
                                    string xQueryExistCondition = m_regExParseParameterXPath.Replace(infoSQL.XPath, @"exist('($1[@$2=""{0}""])')");

                                    xQueryExistCondition = String.Format(xQueryExistCondition, filterParam.Value);

                                    sqlWhere.Append(String.Format(@"({0}.{1}.{2} = 1)",
                                        alias, infoSQL.SQLField, xQueryExistCondition));

                                    break;
                                }
                            }

                    }
                }
            }

            // 3. verify the query contains not compliant WHERE filters
            match = m_RegExParseNotCompliantFilterTag.Match(pQuerytodecorate);
            pNotCompliantFilters = match.Success;

            return sqlWhere.ToString();
        }

        /// <summary>
        /// Build the encapsulating query to exploit all EXCEPT/STATICVALUE parameters
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pFieldsalias">SORTED fields alias collection as found in the query core string</param>
        /// <param name="pQuerytodecorate">query core string</param>
        /// <param name="opHeader">out parameter containing the header to encapsulate the query core</param>
        /// <param name="opFooter">out parameter containing the footer to encapsulate the query core</param>
        private void Aggregate(StringDictionary pTables, string[] pFieldsalias, out string opHeader, out string opFooter)
        {

            bool grouped = IsGrouped(pTables, pFieldsalias, out string HEADERFORMAT, out string FOOTERFORMAT);
            bool haskeys = SearchKeys(pTables, pFieldsalias, out _);

            // UNDONE 20100913 MF - Using aggregate functions to take all the grouped values 
            //   (see: I:\INST_SPHERES\Analyses\Spheres OTCml - F&Oml\Tool de Rapprochement\QueryIOCOmpareClass.sql)

            opHeader = null;
            opFooter = null;

            string keyIDs = haskeys ? "''" : "IDDATA";
            string headersConcat = "";
            string headers = "";
            string footers = "";

            foreach (KeyValuePair<string, TypedIOTaskParam> parameter in m_parameters)
            {
                if (parameter.Value is ExceptParam param && !(parameter.Value is FilterParam))
                {
                    string keyID = null;
                    string headerConcat = null;
                    string header = null;
                    string footer = null;

                    ExceptParam exceptParam = param;

                    string alias = null;
                    ConfigurationSQLInfo infoSQL = null;

                    if (exceptParam.ConfigurationParameter != null)
                        infoSQL = GetSQLInfoExcept(pTables, pFieldsalias, exceptParam.id, exceptParam.ConfigurationParameter.SQLInfos, out alias);

                    bool except = false;

                    if (exceptParam.TypedValue != null)
                        except = (bool)exceptParam.TypedValue;

                    // the parameter will be added to the query IFF it owns a relative not null SQL information object
                    if (infoSQL != null)
                    {
                        if (!except)
                        {
                            switch (infoSQL.TypedGroupByRule)
                            {
                                case ConfigurationSQLInfo.GroupByRuleEnum.GroupBy:
                                case ConfigurationSQLInfo.GroupByRuleEnum.GroupByNull:

                                    keyID = String.Empty;
                                    footer = String.Format("{0},", exceptParam.id);
                                    header = String.Format("{0},", exceptParam.id);

                                    break;

                                case ConfigurationSQLInfo.GroupByRuleEnum.SelectMin:

                                    if (!grouped)
                                        throw new ArgumentException(
                                            @"Found a SelectMin parameter inside of CompareParameters.xml for a comparison template with no 'group by'.
                                              The template configuration is probably wrong. The query builder can not proceed. Process aborted.");

                                    keyID = String.Empty;
                                    footer = String.Empty;
                                    header = String.Format("min({0}) as {0},", exceptParam.id);

                                    break;

                                case ConfigurationSQLInfo.GroupByRuleEnum.SelectSum:

                                    if (!grouped)
                                        throw new ArgumentException(
                                            @"Found a SelectSum parameter inside of CompareParameters.xml for a comparison template with no 'group by'.
                                              The template configuration is probably wrong. The query builder can not proceed. Process aborted.");

                                    keyID = String.Empty;
                                    footer = String.Empty;
                                    header = String.Format("sum({0}) as {0},", exceptParam.id);

                                    break;

                                case ConfigurationSQLInfo.GroupByRuleEnum.Dynamic:

                                    keyID = String.Empty;

                                    // Pour mettre la colonne (paramètre) dans le select                            
                                    header = String.Format("{0},", exceptParam.id);

                                    // Pour mettre la colonne (paramètre) dans le group by                                                                
                                    if (grouped)
                                        footer = String.Format("{0},", exceptParam.id);

                                    break;

                                case ConfigurationSQLInfo.GroupByRuleEnum.Key:

                                    keyID = String.Format("|| {0}", exceptParam.CastToStringXSQL);

                                    // TODO - 20101213 - unused values, planify suppression
                                    header = String.Format("null as {0},", exceptParam.id);
                                    footer = String.Empty;

                                    break;
                            }

                            keyIDs = String.Concat(keyIDs, keyID);
                            footers = String.Concat(footers, footer);
                            headers = String.Concat(headers, header);

                        }
                        else
                        {
                            // UNDONE 20100913 MF - This case does not exploit XPath information, just elementary fields can be used

                            if (grouped)
                            {
                                headerConcat = String.Format(@"|| '|{0}:' 
                                    || cast(min({0}) as varchar) 
                                    ", exceptParam.id);

                                headersConcat = String.Concat(headersConcat, headerConcat);
                            }

                            // Pour exclure (mise à null) la colonne (paramètre) du select
                            header = String.Format("null as {0},", exceptParam.id);
                            headers = String.Concat(headers, header);
                        }
                    }
                }
                    // MF 20120926 - Ticket 18149
                    // treating static value parameter, in the iocompare context a static value is a field that does not appear in
                    // the SQL request underlying the current comparison interface. It will be a constant value added to the 
                    // current SQL fields.
                else if (parameter.Value is StaticValueParam staticValueParam)
                {
                    object value = staticValueParam.TypedValue;
                    headers = String.Concat(headers, String.Format("{0} as {1},", Convert.ToString(value), staticValueParam.id));
                }
            }

            if (grouped)
            {
                opHeader = String.Format(HEADERFORMAT, keyIDs, headersConcat, headers);

                footers = footers.Substring(0, footers.Length - 1);
                opFooter = String.Format(FOOTERFORMAT, footers);
            }
            else
            {
                opHeader = String.Format(HEADERFORMAT, keyIDs, headers);

                opFooter = FOOTERFORMAT;
            }

        }

        /// <summary>
        /// Returns whether the current query must be grouped (IOW it includes a GROUP BY) or not. 
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pFieldsalias">SORTED fields alias collection as found in the query core string</param>
        /// <param name="opHEADERFORMAT">out parameter containing the header FORMAT to decorate the current query</param>
        /// <param name="opFOOTERFORMAT">out parameter containing the footer FORMAT to decorate the current query</param>
        /// <returns>true when the query must be grouped</returns>
        private bool IsGrouped(StringDictionary pTables, string[] pFieldsalias, out string opHEADERFORMAT, out string opFOOTERFORMAT)
        {
            bool grouped = false;

            foreach (KeyValuePair<string, TypedIOTaskParam> parameter in m_parameters)
                // Le type FilterParam hérite de ExceptParam
                if (parameter.Value is ExceptParam param && !(parameter.Value is FilterParam))
                {
                    ExceptParam exceptParam = param;

                    ConfigurationSQLInfo infoSQL = GetSQLInfoExcept(pTables, pFieldsalias, exceptParam.id, exceptParam.ConfigurationParameter.SQLInfos);

                    if (infoSQL != null && exceptParam.ConfigurationParameter != null
                        && (infoSQL.TypedGroupByRule == ConfigurationSQLInfo.GroupByRuleEnum.GroupBy ||
                            infoSQL.TypedGroupByRule == ConfigurationSQLInfo.GroupByRuleEnum.GroupByNull))
                    {
                        grouped = true;
                        break;
                    }
                }

            opHEADERFORMAT = @"
                   select {0} as IDDATA,
                   '' as COLLECTEDVALUES, 
                   {1}
                   DATAROWNUMBER
                   from (";

            opFOOTERFORMAT = @") tablemain";

            if (grouped)
            {
                opHEADERFORMAT = @"
                   select min({0}) as IDDATA,
                   'ID:' || cast(min(IDDATA) as varchar)
                   {1}
                   || '|DATAROWNUMBER:' || cast(min(DATAROWNUMBER) as varchar)
                   as COLLECTEDVALUES, 
                   {2}
                   null as DATAROWNUMBER
                   from (";

                opFOOTERFORMAT = @"
                   ) tablemain
                   group by
                   {0}";
            }

            return grouped;
        }

        /// <summary>
        /// Returns whether the current query needs a run-time built key (IOW the grouped SQL request does not own a unique single ID) or not. 
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pFieldsalias">SORTED fields alias collection as found in the query core string</param>
        /// <param name="opTable">Out parameter containing the table name where the key has to be extracted</param>
        /// <returns>true when the key set must be built</returns>
        private bool SearchKeys(StringDictionary pTables, string[] pFieldsalias, out string opTable)
        {
            bool keyFound = false;

            opTable = null;

            foreach (KeyValuePair<string, TypedIOTaskParam> parameter in m_parameters)
                if (parameter.Value is ExceptParam param && !(parameter.Value is FilterParam))
                {
                    ExceptParam exceptParam = param;

                    ConfigurationSQLInfo infoSQL = GetSQLInfoExcept(
                        pTables, pFieldsalias, exceptParam.id, exceptParam.ConfigurationParameter.SQLInfos);

                    if (infoSQL != null && exceptParam.ConfigurationParameter != null

                        && infoSQL.TypedGroupByRule == ConfigurationSQLInfo.GroupByRuleEnum.Key)
                        keyFound = true;

                    if (keyFound)
                    {
                        // Breaking the loop just when the condition is satisfied.
                        // (The same table name value is shared by all the EXCEPT parameters)
                        opTable = infoSQL.SQLTable;
                        break;
                    }

                }

            return keyFound;
        }

        /// <summary>
        /// Get the right SQL Information object about the current evaluating EXCEPT parameter.
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pFieldsalias">SORTED fields alias collection as found in the query core string</param>
        /// <param name="pParameterId">The current parameter string ID (it must match almost an element inside the field alias collection)</param>
        /// <param name="pList">The list containing the all possible SQL informations for the current parameter</param>
        /// <returns>The SQL Information object</returns>
        private ConfigurationSQLInfo GetSQLInfoExcept(StringDictionary pTables, string[] pFieldsalias,
            string pParameterId, List<ConfigurationSQLInfo> pList)
        {
            return GetSQLInfoExcept(pTables, pFieldsalias, pParameterId, pList, out _);
        }

        /// <summary>
        /// Get the right SQL Information object about the current evaluating EXCEPT parameter.
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pFieldsalias">SORTED fields alias collection as found in the query core string</param>
        /// <param name="pParameterId">The current parameter string ID (it must match almost an element inside the field alias collection)</param>
        /// <param name="pList">The list containing the all possible SQL informations for the current parameter</param>
        /// <param name="opAlias">The alias relative to the returned SQL Information object</param>
        /// <returns>The SQL Information object</returns>
        private ConfigurationSQLInfo GetSQLInfoExcept(StringDictionary pTables, string[] pFieldsalias, string pParameterId,
            List<ConfigurationSQLInfo> pList, out string opAlias)
        {
            ConfigurationSQLInfo infoRes = null;

            opAlias = null;

            int howmanyinfo = 0;

            foreach (ConfigurationSQLInfo info in pList)
            {
                if (pTables.ContainsKey(info.SQLTable))
                {
                    howmanyinfo++;

                    //  An info object has been found
                    infoRes = info;

                    // 1. Tables alias validation check...

                    opAlias = pTables[info.SQLTable];

                    opAlias = opAlias.Trim();

                    if (opAlias == String.Empty ||
                        opAlias == SQLCst.WHERE.Trim() ||
                        opAlias == SQLCst.ON.Trim() ||
                        opAlias == "inner")
                        throw new ArgumentNullException(
                            info.SQLTable,
                            "Alias table is missing on " + info.SQLTable + @". An alias table is mandatory for each table. 
                             Please check the XML configuration file, or contact the editor.");

                    // 2. Field alias validation check...

                    int idx = Array.BinarySearch<string>(pFieldsalias, pParameterId, StringComparer.InvariantCultureIgnoreCase);

                    if (idx < 0)
                    {
                        opAlias = null;
                        infoRes = null;
                    }

                }

            }

            if (howmanyinfo > 1)
                throw new ArgumentOutOfRangeException(
                    String.Format(@"Too many SQL informations has been found for the current parameter. 
                    Check the XML configuration file. Parameter name: {0}", pParameterId));

            return infoRes;
        }

        /// <summary>
        /// Get the right SQL Information object about the current evaluating FILTER parameter.
        /// </summary>
        /// <param name="pTables">Tables/Alias dictionary as found in the query core string</param>
        /// <param name="pList">The list containing the all possible SQL informations for the current parameter</param>
        /// <param name="opAlias">The alias relative to the returned SQL Information object</param>
        /// <returns>the list of the all the matching info sql configuration objects</returns>
        private List<ConfigurationSQLInfo> GetSQLInfoFilter(StringDictionary pTables, List<ConfigurationSQLInfo> pList, out string opAlias)
        {
            List<ConfigurationSQLInfo> infoRes = new List<ConfigurationSQLInfo>();

            opAlias = null;

            foreach (ConfigurationSQLInfo info in pList)
            {
                if (pTables.ContainsKey(info.SQLTable))
                {

                    //  An info object has been found for the query to be decorated
                    infoRes.Add(info);

                    // Tables alias validation check...
                    opAlias = pTables[info.SQLTable];

                    opAlias = opAlias.Trim();

                    if (opAlias == String.Empty ||
                        opAlias == SQLCst.WHERE.Trim() ||
                        opAlias == SQLCst.ON.Trim() ||
                        opAlias == "inner")
                        throw new ArgumentNullException(
                            info.SQLTable,
                            "Alias table is missing on " + info.SQLTable + ". An alias table is mandatory for each table. Please check the XML configuration file or contact EFS to check the query core definition.");
                }

            }

            return infoRes;
        }

        #endregion Private methods
    }

    #endregion QueryHelper

    #region Comparison classes


    /// <summary>
    /// The comparison engine
    /// </summary>
    /// <typeparam name="TExternal">External elements type</typeparam>
    /// <typeparam name="TInternal">Internal elements type</typeparam>
    internal sealed class CompareHelper<TExternal, TInternal>
        where TExternal : ISpheresComparer
        where TInternal : ISpheresComparer
    {
        #region Properties

        /// <summary>
        /// The external elements set to be compared with the internal set
        /// </summary>
        List<TExternal> m_externalDataElems = null;

        /// <summary>
        /// The internal elements set to be compared with the external set
        /// </summary>
        List<TInternal> m_internalDataElems = null;

        /// <summary>
        /// The data table containing the comparison process results
        /// </summary>
        readonly DataTable m_tableIoTrackCompare = null;

        /// <summary>
        /// The current IO task
        /// </summary>
        readonly Task m_task = null;

        readonly IOCompareQueryInternal<TInternal> m_IOCompareQueryInternal;

        #endregion Properties

        /// <summary>
        /// Create a new comparison engine instance
        /// </summary>
        /// <param name="pTask">the current io task object</param>
        // RD 20190718 [24580] Modify
        internal CompareHelper(Task pTask)
        {

            m_task = pTask;

            // RD 20190718 [24580] Matching only Clearer data according to value of I/O Task parameter: ISMATCHINGCLEARER
            bool isMatchingOnlyClearer = (typeof(TExternal) == typeof(PositionFixCompareData));
            if (isMatchingOnlyClearer)
                isMatchingOnlyClearer = m_task.IoTask.parameters.Contains(IOTrack.PARAM_ISMATCHINGCLEARER);
            if (isMatchingOnlyClearer)
                isMatchingOnlyClearer = BoolFunc.IsTrue(m_task.IoTask.parameters[IOTrack.PARAM_ISMATCHINGCLEARER]);

            m_IOCompareQueryInternal = new IOCompareQueryInternal<TInternal>(m_task.Cs, isMatchingOnlyClearer);

            Type schemaTableIoTrackCompare = typeof(IOTRACKCOMPARE);

            m_tableIoTrackCompare = new DataTable();

            foreach (var column in schemaTableIoTrackCompare.GetProperties())
            {
                Type columnType = column.PropertyType;

                if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    columnType = Nullable.GetUnderlyingType(columnType);

                if (column.Name == Enum.GetName(typeof(EFS.ACommon.Cst.OTCml_COL), EFS.ACommon.Cst.OTCml_COL.ROWVERSION))
                    continue;

                m_tableIoTrackCompare.Columns.Add(column.Name, columnType);
            }

        }

        #region Interface

        /// <summary>
        /// Fill the external elements set
        /// </summary>
        /// <param name="pConnection">open connection to the data base</param>
        /// <param name="pParameters">the user parameters set</param>
        /// <returns>how many elements has been loaded</returns>
        internal int LoadExternalDataSet(IDbConnection pConnection, ParamsCollection pParameters)
        {
            string query = IOCompareQueryExternal<TExternal>.QueryCoreExternal;

            QueryHelper helper = new QueryHelper(pParameters, this.m_task.Cs);

            helper.BuildQuery(query, out query);

            query = DataHelper<TExternal>.XQueryTransform(pConnection, CommandType.Text, query);

            query = DataHelper<TExternal>.CastTransform(pConnection, CommandType.Text, query);

            query = DataHelper<TExternal>.IsNullTransform(pConnection, CommandType.Text, query);

            query = DataHelper<TInternal>.ReplicateTransform(pConnection, CommandType.Text, query);

            IDbDataParameter[] optionalParameters = helper.BuildOptionalSqlParameters(query, this.m_task.Cs);

            if (optionalParameters.Length > 0)
            {
                m_externalDataElems = DataHelper<TExternal>.ExecuteDataSet(pConnection, CommandType.Text, query, optionalParameters);
            }
            else
            {
                m_externalDataElems = DataHelper<TExternal>.ExecuteDataSet(pConnection, CommandType.Text, query);
            }

            return m_externalDataElems.Count;
        }

        /// <summary>
        /// Fill the internal elements set
        /// </summary>
        /// <param name="pConnection">open connection to the data base</param>
        /// <param name="pParameters">the user parameters set</param>
        /// <returns>how many elements has been loaded</returns>
        internal int LoadInternalDataSet(IDbConnection pConnection, ParamsCollection pParameters)
        {
            string query = m_IOCompareQueryInternal.QueryCoreInternal;

            QueryHelper helper = new QueryHelper(pParameters, this.m_task.Cs);

            helper.BuildQuery(query, out query);

            query = DataHelper<TInternal>.XQueryTransform(pConnection, CommandType.Text, query);

            query = DataHelper<TInternal>.CastTransform(pConnection, CommandType.Text, query);

            query = DataHelper<TInternal>.IsNullTransform(pConnection, CommandType.Text, query);

            query = DataHelper<TInternal>.ReplicateTransform(pConnection, CommandType.Text, query);

            IDbDataParameter[] optionalParameters = helper.BuildOptionalSqlParameters(query, this.m_task.Cs);

            if (optionalParameters.Length > 0)
            {
                m_internalDataElems = DataHelper<TInternal>.ExecuteDataSet(pConnection, CommandType.Text, query, optionalParameters);
            }
            else
            {
                m_internalDataElems = DataHelper<TInternal>.ExecuteDataSet(pConnection, CommandType.Text, query);
            }

            return m_internalDataElems.Count;
        }

        /// <summary>
        /// Comparing the external set vs the internal set
        /// </summary>
        /// <param name="pGlobalStatus">the global status</param>
        /// <returns>the datatable containing the comparison details for the comparison operation</returns>
        internal DataTable Compare(out MatchStatus pGlobalStatus)
        {
            if (m_externalDataElems == null || m_internalDataElems == null)
                throw new ArgumentNullException();

            // RD 20131129 [19285] Manage new parameter ISMATCHINGNETQTY
            // Matching net values
            // 1 - Only for Positions matching interface
            bool isMatchingNetQty = (typeof(TExternal) == typeof(PositionFixCompareData));
            // 2 - According to value of I/O Task parameter: ISMATCHINGNETQTY
            if (isMatchingNetQty)
                isMatchingNetQty = m_task.IoTask.parameters.Contains(IOTrack.PARAM_ISMATCHINGNETQTY);
            if (isMatchingNetQty)
                isMatchingNetQty = BoolFunc.IsTrue(m_task.IoTask.parameters[IOTrack.PARAM_ISMATCHINGNETQTY]);

            // collection of matching elements
            Dictionary<TInternal, MatchStatus> m_MatchingElements = new Dictionary<TInternal, MatchStatus>();

            MatchStatus varSTATUSOK = (MatchStatus)((int[])Enum.GetValues(typeof(MatchStatus)))[0];

            // Init the global status: get the minimum value (lower error level) for the MatchStatus enumeration
            pGlobalStatus = varSTATUSOK;

            // 1. External loop : cycling on the external set
            foreach (TExternal elementExt in m_externalDataElems)
            {
                MatchStatus matchSt = MatchStatus.UNMATCH_MISSING;

                // 1.1 Get the compare key for the external element
                object[] keyExt = elementExt.ComparisonKey;

                elementExt.Initialise();

                TInternal lastMatchingInternal = default;

                // 2. Internal loop: cycling on the internal set
                foreach (TInternal elementInt in m_internalDataElems)
                {
                    // 2.1 Get the compare key for the internal element
                    object[] keyInt = elementInt.ComparisonKey;

                    int i = 0;

                    // 2.2 Compare engine - first step... Compare keys
                    if (keyExt.Length == keyInt.Length)
                        for (; i < keyInt.Length; i++)
                        {
                            if (
                                ((keyInt[i] == null) && (keyExt[i] != null))
                                ||
                                ((keyExt[i] == null) && (keyInt[i] != null))
                                ||
                                ((keyInt[i] != null) && (keyExt[i] != null) && (!keyInt[i].Equals(keyExt[i])))
                                )
                                break;
                        }

                    // if all the keys passed the check then the elements are equals (and we can compare "values")
                    if (i == keyInt.Length)
                    {
                        elementInt.Initialise();

                        ValueErrorStatus[] ValueExt = elementExt.Values;
                        ValueErrorStatus[] ValueInt = elementInt.Values;

                        matchSt = varSTATUSOK;

                        i = 0;

                        // Compare engine - second step ... Start
                        if (ValueExt.Length == ValueInt.Length)
                        {
                            // RD 20131129 [19285] Manage new parameter ISMATCHINGNETQTY
                            if (isMatchingNetQty)
                            {
                                bool isEnabled = ValueErrorStatus.CompareEnabled(elementInt.QtyErrorStatusByKey("1"), elementExt.QtyErrorStatusByKey("1"))
                                    && ValueErrorStatus.CompareEnabled(elementInt.QtyErrorStatusByKey("2"), elementExt.QtyErrorStatusByKey("2"));

                                if (isEnabled)
                                {
                                    decimal qtyExtNet = elementExt.QtyErrorStatusByKey("1").Value - elementExt.QtyErrorStatusByKey("2").Value;
                                    decimal qtyIntNet = elementInt.QtyErrorStatusByKey("1").Value - elementInt.QtyErrorStatusByKey("2").Value;

                                    if (elementInt.QtyErrorStatusByKey("1").Epsilon > 0)
                                    {
                                        if (!elementInt.QtyErrorStatusByKey("1").EpsilonEquals(qtyExtNet, qtyIntNet))
                                        {
                                            matchSt = matchSt | elementInt.QtyErrorStatusByKey("1").ErrorStatus | elementInt.QtyErrorStatusByKey("2").ErrorStatus;
                                        }
                                    }
                                    else
                                    {
                                        if (qtyExtNet != qtyIntNet)
                                        {
                                            matchSt = matchSt | elementInt.QtyErrorStatusByKey("1").ErrorStatus | elementInt.QtyErrorStatusByKey("2").ErrorStatus;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (; i < ValueInt.Length; i++)
                                {
                                    // MF 20120924 - Ticket 18149 - adding a tolerance(epsilon) to the comparison 
                                    if (ValueInt[i].Epsilon > 0)
                                    {
                                        if (!ValueInt[i].EpsilonEquals(ValueExt[i]) && ValueErrorStatus.CompareEnabled(ValueInt[i], ValueExt[i]))
                                        {
                                            matchSt |= ValueInt[i].ErrorStatus;
                                        }
                                    }
                                    else
                                    {
                                        if (ValueInt[i].Value != ValueExt[i].Value && ValueErrorStatus.CompareEnabled(ValueInt[i], ValueExt[i]))
                                        {
                                            matchSt |= ValueInt[i].ErrorStatus;
                                        }
                                    }
                                }
                            }
                        }
                        else
                            matchSt = MatchStatus.UNMATCH_QTY;
                        // Compare engine - second step ... End

                        if (matchSt == varSTATUSOK)
                        {
                            // if the Qty element value is common then the elements match and we may break the loop
                            lastMatchingInternal = elementInt;

                            // 20110902 MF - résolution ticket 17559, liée à la modification "20101215 MF - Mod PL"
                            if (!m_MatchingElements.ContainsKey(elementInt))
                            {
                                break;
                            }
                        }
                        else
                        {
                            // When the elements do not match then we save the last equal element that we found
                            if (lastMatchingInternal == null)
                                lastMatchingInternal = elementInt;
                            else
                            {
                                // When the elements do not match and an equal element has been already saved, then we 
                                //      replace the old element with the new one is the delta is minor.
                                ValueErrorStatus[] ValueIntPre = lastMatchingInternal.Values;

                                decimal deltaExtPre = 0;
                                decimal deltaExtAct = 0;

                                for (int id = 0; id < ValueExt.Length; id++)
                                {
                                    deltaExtPre += Math.Abs(ValueExt[id].Value - ValueIntPre[id].Value);
                                    deltaExtAct += Math.Abs(ValueExt[id].Value - ValueInt[id].Value);
                                }

                                if (deltaExtAct < deltaExtPre)
                                    lastMatchingInternal = elementInt;
                            }
                        }
                    }

                }

                // Updating the globalstatus any time we got one error more critical than the previous one
                if (matchSt > pGlobalStatus)
                    pGlobalStatus = MatchStatus.UNMATCH;

                // Add a new row insinde of the tableIoTrackCompare for the current "External data"
                AddElementToIoTrackTable(elementExt, matchSt, lastMatchingInternal, true);

                // Removing from the internal Collection the matching element

                // 20101215 MF - Mod PL to avoid missing in case of twin trades
                if (lastMatchingInternal != null && !m_MatchingElements.ContainsKey(lastMatchingInternal))
                    m_MatchingElements.Add(lastMatchingInternal, matchSt);

            }

            if (m_MatchingElements.Count != m_internalDataElems.Count || m_MatchingElements.Count != m_externalDataElems.Count)
                pGlobalStatus = MatchStatus.UNMATCH;

            // Add a new row inside of the tableIoTrackCompare for each remaining "Internal data" (Not Matched)
            foreach (TInternal elementInt in m_internalDataElems)
            {
                elementInt.Initialise();

                // 20101215 MF - Mod PL to avoid missing in case of twin trades - start
                if ((!m_MatchingElements.ContainsKey(elementInt)) || m_MatchingElements[elementInt] == MatchStatus.UNMATCH_MISSING)
                {
                    AddElementToIoTrackTable(default, MatchStatus.UNMATCH_MISSING, elementInt, false);
                }
            }

            m_MatchingElements.Clear();

            return m_tableIoTrackCompare;
        }

        

        #endregion Interface

        #region Private Methods

        private void AddElementToIoTrackTable(TExternal pElementExt, MatchStatus pMatchSt, TInternal pElementInt, bool pExternal)
        {
            bool IIoTrackSpecific = (pExternal && pElementExt is IIoTrackElements) || (!pExternal && pElementInt is IIoTrackElements);

            bool ISupportKeyGiven = pElementInt is ISupportKey;

            if (IIoTrackSpecific)
            {

                string[] keyElements = pExternal ?
                    ((IIoTrackElements)pElementExt).GetKeyElements() : ((IIoTrackElements)pElementInt).GetKeyElements();


                foreach (string keyElement in keyElements)
                {
                    DataRow rowIoTrackCompare = m_tableIoTrackCompare.NewRow();

                    IoTrackElement ioTrackElementExt = null;
                    IoTrackElement ioTrackElementInt = null;

                    ValueErrorStatus extValueErrorStatus = new ValueErrorStatus();
                    ValueErrorStatus intValueErrorStatus = new ValueErrorStatus();

                    if (pElementExt != null)
                    {
                        extValueErrorStatus = pElementExt.QtyErrorStatusByKey(keyElement);
                        ioTrackElementExt = new IoTrackElement(pElementExt.Id, extValueErrorStatus, keyElement);
                    }

                    if (pElementInt!= null)
                    {
                        intValueErrorStatus = pElementInt.QtyErrorStatusByKey(keyElement);
                        ioTrackElementInt = new IoTrackElement(pElementInt.Id, intValueErrorStatus, keyElement);
                    }

                    bool bDisabled = 
                        (pElementExt != null && !extValueErrorStatus.Enabled)
                        ||
                        (pElementInt != null && !intValueErrorStatus.Enabled);

                    if (!bDisabled)
                    {
                        FillRowValues(rowIoTrackCompare, m_tableIoTrackCompare, pMatchSt, ioTrackElementExt, ioTrackElementInt, pExternal,
                            ISupportKeyGiven ? ((ISupportKey)pElementInt).GetSupportValueIdByElem(keyElement) : null);

                        m_tableIoTrackCompare.Rows.Add(rowIoTrackCompare);
                    }
                }
            }
            else
            {
                DataRow rowIoTrackCompare = m_tableIoTrackCompare.NewRow();
                FillRowValues(rowIoTrackCompare, m_tableIoTrackCompare, pMatchSt, pElementExt, pElementInt, pExternal,
                    ISupportKeyGiven ? ((ISupportKey)pElementInt).SupportValueId : null);
                m_tableIoTrackCompare.Rows.Add(rowIoTrackCompare);
            }
        }

        private void FillRowValues(DataRow pRowIoTrackCompare, DataTable pTableIoTrackCompare,
            MatchStatus pMatchSt, IoTrackElement pIoTrackElementExt, IoTrackElement pIoTrackElementInt, bool pExternal, object pSupportId)
        {
            foreach (DataColumn dataColumn in pTableIoTrackCompare.Columns)
            {
                switch (dataColumn.ColumnName)
                {
                    case "MATCHSTATUS":

                        MatchStatus pStatus = pMatchSt;

                        if (pStatus != MatchStatus.UNMATCH_MISSING)
                        {
                            pStatus = (pExternal) ?
                                pMatchSt & pIoTrackElementExt.ErrorStatus.ErrorStatus :
                                pMatchSt & pIoTrackElementInt.ErrorStatus.ErrorStatus;
                        }

                        pRowIoTrackCompare[dataColumn] =
                            Enum.GetName(typeof(MatchStatus), pStatus);
                        break;

                    case "INTDATAID":
                        if ((!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING) && pIoTrackElementInt.Id is int)
                            pRowIoTrackCompare[dataColumn] = pIoTrackElementInt.Id;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;

                    case "EXTDATAID":
                        if (pExternal)
                            pRowIoTrackCompare[dataColumn] = pIoTrackElementExt.Id;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;

                    case "INTVALUE1":
                        if (!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING)
                            pRowIoTrackCompare[dataColumn] = pIoTrackElementInt.ErrorStatus.Value;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;

                    case "EXTVALUE1":
                        if (pExternal)
                            pRowIoTrackCompare[dataColumn] = pIoTrackElementExt.ErrorStatus.Value;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;

                    case "MATCHVALUE1":
                        if (pExternal)
                            switch (pMatchSt)
                            {
                                case MatchStatus.MATCH:
                                    pRowIoTrackCompare[dataColumn] = pIoTrackElementExt.ErrorStatus.Value;
                                    break;
                                case MatchStatus.UNMATCH_MISSING:
                                    break;
                                default:
                                    pRowIoTrackCompare[dataColumn] =
                                        // MF 20120327 specification changed, in case of unmatch we put the diffrence among the two values
                                        //Math.Min(pIoTrackElementExt.ErrorStatus.Value, pIoTrackElementInt.ErrorStatus.Value);
                                        Math.Abs(pIoTrackElementExt.ErrorStatus.Value - pIoTrackElementInt.ErrorStatus.Value);
                                    break;
                            }
                        break;

                    case "DTINS":
                        // FI 20200820 [25468] dates systemes en UTC
                        pRowIoTrackCompare[dataColumn] = OTCmlHelper.GetDateSysUTC(m_task.Cs);
                        break;

                    case "IDAINS":
                        pRowIoTrackCompare[dataColumn] = m_task.Process.UserId;
                        break;

                    case "INTDATAIDS":
                        if (!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING)
                        {
                            if (pIoTrackElementInt.Id is string)
                            {
                                pRowIoTrackCompare[dataColumn] = pIoTrackElementInt.Id;
                            }
                            else if (pIoTrackElementInt.Id is int && pSupportId != null)
                            {
                                pRowIoTrackCompare[dataColumn] = pSupportId;
                            }
                        }
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;

                    case "UNITVALUE1":
                        if (!pExternal)
                        {
                            pRowIoTrackCompare[dataColumn] = pIoTrackElementInt.Type;
                        }
                        else
                        {
                            pRowIoTrackCompare[dataColumn] = pIoTrackElementExt.Type;
                        }
                        break;

                    default:
                        break; 
                }
            }
        }

        /// <summary>
        /// Validate the values for the current comparison entry row
        /// </summary>
        /// <param name="pRowIoTrackCompare">the data row to be filled</param>
        /// <param name="pTableIoTrackCompare">the datatable where the row has been added</param>
        /// <param name="pMatchSt">the match status for the current comparison entry</param>
        /// <param name="pElementExt">the relative external element, it must be null IFF the external parameter is false</param>
        /// <param name="pElementInt">the relative internal element, 
        /// it must be null IFF the external parameter is true and the match status is UNMATCH_MISSING</param>
        /// <param name="pExternal">true when the actual data row is relative to an internal element</param>
        /// <param name="pSupportId">support id to use to charge the consultation datas together with the default id</param>
        private void FillRowValues(DataRow pRowIoTrackCompare, DataTable pTableIoTrackCompare,
            MatchStatus pMatchSt, TExternal pElementExt, TInternal pElementInt, bool pExternal, object pSupportId)
        {
            foreach (DataColumn dataColumn in pTableIoTrackCompare.Columns)
            {
                switch (dataColumn.ColumnName)
                {
                    case "IDIOTRACK":
                        // temp value : the real id will be assigned after the insertion of the IOTRACK element in the DB (WriteHighLight)
                        pRowIoTrackCompare[dataColumn] = 0;
                        break;
                    case "MESSAGE":
                        // 20100928 MF/from PL - laisser le champs MESSAGE à blanc
                        pRowIoTrackCompare[dataColumn] = String.Empty;
                        break;
                    case "MATCHSTATUS":
                        pRowIoTrackCompare[dataColumn] = Enum.GetName(typeof(MatchStatus), pMatchSt);
                        break;
                    case "INTDATAID":
                        if ((!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING) && pElementInt.Id is int)
                            pRowIoTrackCompare[dataColumn] = pElementInt.Id;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    case "EXTDATAID":
                        if (pExternal)
                            pRowIoTrackCompare[dataColumn] = pElementExt.Id;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    case "INTVALUE1":
                        if (!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING)
                            pRowIoTrackCompare[dataColumn] = pElementInt.QtyErrorStatusByKey("1").Value;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    case "INTVALUE2":
                        if (!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING)
                            pRowIoTrackCompare[dataColumn] = pElementInt.QtyErrorStatusByKey("2").Value;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    case "EXTVALUE1":
                        if (pExternal)
                            pRowIoTrackCompare[dataColumn] = pElementExt.QtyErrorStatusByKey("1").Value;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    case "EXTVALUE2":
                        if (pExternal)
                            pRowIoTrackCompare[dataColumn] = pElementExt.QtyErrorStatusByKey("2").Value;
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    case "MATCHVALUE1":
                        if (pExternal)
                            switch (pMatchSt)
                            {
                                case MatchStatus.MATCH:
                                case MatchStatus.UNMATCH_SHORTQTY:
                                    pRowIoTrackCompare[dataColumn] = pElementExt.QtyErrorStatusByKey("1").Value;
                                    break;
                                case MatchStatus.UNMATCH_LONGQTY:
                                case MatchStatus.UNMATCH_QTY:
                                    pRowIoTrackCompare[dataColumn] =
                                        // MF 20120327 specification changed, in case of unmatch we put the diffrence among the two values
                                        //Math.Min(pElementExt.QtyErrorStatusByKey("1").Value, pElementInt.QtyErrorStatusByKey("1").Value);
                                        Math.Abs(pElementExt.QtyErrorStatusByKey("1").Value - pElementInt.QtyErrorStatusByKey("1").Value);
                                    break;
                            }
                        break;
                    case "MATCHVALUE2":
                        if (pExternal)
                            switch (pMatchSt)
                            {
                                case MatchStatus.MATCH:
                                case MatchStatus.UNMATCH_LONGQTY:
                                    pRowIoTrackCompare[dataColumn] = pElementExt.QtyErrorStatusByKey("2").Value;
                                    break;
                                case MatchStatus.UNMATCH_SHORTQTY:
                                case MatchStatus.UNMATCH_QTY:
                                    pRowIoTrackCompare[dataColumn] =
                                        // MF 20120327 specification changed, in case of unmatch we put the diffrence among the two values
                                        //Math.Min(pElementExt.QtyErrorStatusByKey("2").Value, pElementInt.QtyErrorStatusByKey("2").Value);
                                        Math.Abs(pElementExt.QtyErrorStatusByKey("2").Value - pElementInt.QtyErrorStatusByKey("2").Value);
                                    break;
                            }
                        break;
                    case "DTINS":
                        // FI 20200820 [25468] dates systemes en UTC
                        pRowIoTrackCompare[dataColumn] = OTCmlHelper.GetDateSysUTC(m_task.Cs);
                        break;
                    case "IDAINS":
                        pRowIoTrackCompare[dataColumn] = m_task.Process.UserId;
                        break;
                    case "INTDATAIDS":
                        if (!pExternal || pMatchSt != MatchStatus.UNMATCH_MISSING)
                        {
                            if (pElementInt.Id is string)
                            {
                                pRowIoTrackCompare[dataColumn] = pElementInt.Id;
                            }
                            else if (pElementInt.Id is int && pSupportId != null)
                            {
                                pRowIoTrackCompare[dataColumn] = pSupportId;
                            }
                        }
                        else
                            pRowIoTrackCompare[dataColumn] = DBNull.Value;
                        break;
                    
                    default:
                        break; 
                }

            }
        }

        #endregion Private Methods

    }

    /// <summary>
    /// Main Spheres IO class to compare external data vs internal data
    /// <seealso cref="I:\INST_SPHERES\Analyses\Spheres OTCml - F&Oml\Tool de Rapprochement"/>
    /// </summary>
    internal sealed class ProcessCompare
    {
        const int MAXSTEP = 4;

        #region Members
        /// <summary>
        /// Containing the infromation about the actual IOTASK
        /// </summary>
        /// <remarks>
        /// The IOTASK is composed by one or more IMPORT elements and one COMPARE element.
        /// </remarks>
        private readonly Task m_Task;

        private readonly int m_IdIoTaskDet;

        private readonly string m_IdIoElement;


        // PM 20200102 [XXXXX] New Log : ajout m_ElementStatus
        // FI 20200706 [XXXXX] Mise en commentaire (remplacé par ProcessState)
        //protected ProcessStateTools.StatusEnum m_ElementStatus;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Containing the ID of the COMPARE element, stocked in the IOTASKDET table
        /// </summary>
        /// <remarks>
        /// IOTASKDET.IDIOTASKDET
        /// </remarks>
        internal int IdIoTaskDet
        {
            get { return m_IdIoTaskDet; }
        }

        /// <summary>
        /// Containing the IOCOMPARE.IOIOCOMPARE element
        /// </summary>
        internal string IdIoElement
        {
            get { return m_IdIoElement; }
        }


        /// <summary>
        /// Statut de l'élément
        /// </summary>
        /// PM 20200102 [XXXXX] New Log : ajout ElementStatus
        /// FI 20200706 [XXXXX] ProcessState remplace ElementStatus
        public ProcessState ProcessState
        {
            get;
            private set;
        }
        #endregion Accessors

        /// <summary>
        /// Build a new Compare process instance
        /// </summary>
        /// <param name="pTask">The current task hosting the compare process element</param>
        /// <param name="pIdIoTaskDet">the Id of the compare process</param>
        /// <param name="pIdIoElement">the name of the compare process element</param>
        internal ProcessCompare(Task pTask, int pIdIoTaskDet, string pIdIoElement)
        {
            m_Task = pTask;
            m_IdIoTaskDet = pIdIoTaskDet;
            m_IdIoElement = pIdIoElement;
            // FI 20200706 [XXXXX] init de ProcessState
            ProcessState = new ProcessState(ProcessStateTools.StatusSuccessEnum);
        }

        /// <summary>
        /// Main procedure of the ProcessCompare class
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        internal Cst.ErrLevel Compare()
        {
            // RD 20120626 [17950] Task: Execution of the element under condition
            // Refactoring pour retourner un code
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            IDbConnection connection = DataHelper.OpenConnection(m_Task.Cs);

            try
            {
                // START of the compare procedure

                // 1. Get the compare element

                ///////////////////////////////////////////
                
                Logger.Log(new LoggerData(LogLevelEnum.None, String.Format("Step 1/{0} : Initializing", MAXSTEP)));

                ///////////////////////////////////////////

                IOCompare ioCompareElem = GetIOCompare(connection, m_Task.Cs);

                // RD à voir avec MF / Pourquoi le même ENUM
                CompareOptions intDataSource = (CompareOptions)Enum.Parse(typeof(CompareOptions), ioCompareElem.DataSource);
                CompareOptions intDataType = (CompareOptions)Enum.Parse(typeof(CompareOptions), ioCompareElem.DataType);
                CompareOptions extDataFormat = (CompareOptions)Enum.Parse(typeof(CompareOptions), ioCompareElem.DataFormat);

                ///////////////////////////////////////////

                
                Logger.Log(new LoggerData(LogLevelEnum.None, String.Format(
                        "Compare [Identifier: {0}{1} - Source: {2} - Type: {3} - Format: {4}]",
                        ioCompareElem.IdIoCompare,
                        ioCompareElem.DisplayName == ioCompareElem.IdIoCompare ? string.Empty : @" / " + ioCompareElem.DisplayName,
                        ioCompareElem.DataSource, ioCompareElem.DataType, ioCompareElem.DataFormat)));

                ///////////////////////////////////////////

                // 2. Initialize the comparison parameters 

                Cst.MatchingMode mode = (Cst.MatchingMode)Enum.Parse(typeof(Cst.MatchingMode), m_Task.IoTask.parameters[IOTrack.PARAM_MATCHINGMODE]);

                ParamsCollection parameters = new ParamsCollection();
                parameters.Init(m_Task, mode);

                ///////////////////////////////////////////
                ///
                
                Logger.Log(new LoggerData(LogLevelEnum.None,
                    String.Format("Parameters [Business: <b>{0}</b> - Mode: <b>{1}</b>]", m_Task.IoTask.parameters[IOTrack.PARAM_DTBUSINESS], mode.ToString())));

                ///////////////////////////////////////////

                // 3. Execute compare

                // datatable which will be filled with the comparison details (one row for any matching or not matching element will be added)
                DataTable tableIoTrackCompare = null;
                // the global status of the comparison process

                tableIoTrackCompare = ExecuteSpecificCompareForStaticModel(connection, m_Task, parameters,
                                            intDataSource | intDataType | extDataFormat, out MatchStatus globalStatus);

                // 3. Writing results

                ///////////////////////////////////////////
                
                Logger.Log(new LoggerData(LogLevelEnum.None, String.Format("Step 4/{0} : Writing", MAXSTEP)));

                ///////////////////////////////////////////

                IOTrack reportIOTRACK = new IOTrack
                {
                    DtBusiness = Convert.ToDateTime(m_Task.IoTask.parameters[IOTrack.PARAM_DTBUSINESS], CultureInfo.InvariantCulture),
                    MatchingMode = mode,
                    Status = globalStatus,
                    Message = String.Format(
                        "{0}{1}[Source: {2}]{3}[Type: {4}]{5}[Format: {6}]",
                        Cst.HTMLBold +
                            (globalStatus == MatchStatus.MATCH ? "Data matched successfully" : "Warning, data unmatched")
                            + Cst.HTMLEndBold, Cst.CrLf,
                        ioCompareElem.DataSource, Cst.CrLf,
                        ioCompareElem.DataType, Cst.CrLf,
                        ioCompareElem.DataFormat)
                };
                reportIOTRACK.SetData(IOTrack.PARAM_FILTER, parameters);
                reportIOTRACK.SetData(IOTrack.PARAM_EXCEPT, parameters);
                reportIOTRACK.ExtDataFormat = intDataType;

                reportIOTRACK.IdIoTrack = WriteHighLight(connection, reportIOTRACK);

                WriteDetails(m_Task.Cs, tableIoTrackCompare, reportIOTRACK);

                string resCompare = String.Format("Result: <b>{0}</b>", (globalStatus == MatchStatus.MATCH ? "MATCH" : "UNMATCH"));

                ///////////////////////////////////////////

                
                // FI 20200706 [XXXXX] use ProcessState
                if (!(globalStatus == MatchStatus.MATCH))
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                ret = (globalStatus == MatchStatus.MATCH ? Cst.ErrLevel.SUCCESS : Cst.ErrLevel.DATAUNMATCH);

                Logger.Log(new LoggerData(globalStatus == MatchStatus.MATCH ? LogLevelEnum.Info : LogLevelEnum.Error, resCompare, 0,
                    new LogParam(m_IdIoTaskDet, default, default, Cst.LoggerParameterLink.IDDATA)));


                ///////////////////////////////////////////

                // END of the compare procedure
            }
            finally
            {
                DataHelper.CloseConnection(connection);
            }

            return ret;
        }

        /// <summary>
        /// Get the IOCOMPARE element from the target DB
        /// </summary>
        /// <param name="pConnectionString">DB connection info</param>
        /// <param name="pConnection">DB active connection relative to the connection string</param>
        /// <exception cref="InvalidOperationException">
        /// One error is occurred during the fetch of the IOCOMPARE table, 
        /// or during the cast to the IOCompare class
        /// </exception>
        /// <returns></returns>
        internal IOCompare GetIOCompare(IDbConnection pConnection, string pConnectionString)
        {
            // RD à voir avec MF / Pourquoi select all
            string query = SQLCst.SELECT_ALL + SQLCst.FROM_DBO + Cst.CrLf;
            query += Cst.OTCml_TBL.IOCOMPARE.ToString() + Cst.CrLf;
            query += SQLCst.WHERE + "IDIOCOMPARE=" + DataHelper.SQLString(m_IdIoElement) + SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pConnectionString, Cst.OTCml_TBL.IOCOMPARE);

            List<IOCompare> list = DataHelper<IOCompare>.ExecuteDataSet(pConnection, CommandType.Text, query);

            IOCompare compareElement;
            switch (list.Count)
            {
                // Just one single IOCompare record  is accepted    
                case 1:
                    compareElement = list[0];
                    break;
                case 0:
                default:
                    throw new InvalidOperationException(String.Format(@"{0} IOCompare element(s) found for the current compare process, 
                                                          just one single line is accepted. Process aborted.", list.Count));
            }
            return compareElement;
        }

        /// <summary>
        /// Use a specific CompareHelper instance according with the given CompareOptions parameter
        /// </summary>
        /// <param name="pConnection">the curent connection to the DBMS</param>
        /// <param name="pTask">The current task hosting the compare process element</param>
        /// <param name="pParameters">the user parameters set</param>
        /// <param name="pCompareOptions">The compare options set</param>
        /// <param name="pGlobalStatus">out parameter containing the comparison result</param>
        /// <returns></returns>
        private DataTable ExecuteSpecificCompareForStaticModel(IDbConnection pConnection, Task pTask, ParamsCollection pParameters,
            CompareOptions pCompareOptions, out MatchStatus pGlobalStatus)
        {
            int rowCount = 0;
            DataTable tableIoTrackCompare = null;

            switch (pCompareOptions)
            {
                case (CompareOptions.Eurosys | CompareOptions.FIXml | CompareOptions.Trades):
                    {
                        CompareHelper<TradeFixCompareData, TradeFixCompareDataEurosys> helperFixTradeEurosys =
                            new CompareHelper<TradeFixCompareData, TradeFixCompareDataEurosys>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<TradeFixCompareData, TradeFixCompareDataEurosys>(
                            pConnection, pParameters, helperFixTradeEurosys, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                case (CompareOptions.Eurosys | CompareOptions.FIXml | CompareOptions.Positions):
                    {
                        CompareHelper<PositionFixCompareData, PositionFixCompareDataEurosys> helperFixPositionEurosys =
                            new CompareHelper<PositionFixCompareData, PositionFixCompareDataEurosys>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<PositionFixCompareData, PositionFixCompareDataEurosys>(
                            pConnection, pParameters, helperFixPositionEurosys, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                case (CompareOptions.Spheres | CompareOptions.FIXml | CompareOptions.Trades):
                    {
                        CompareHelper<TradeFixCompareData, TradeFixCompareData> helperFixTradeSpheres =
                            new CompareHelper<TradeFixCompareData, TradeFixCompareData>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<TradeFixCompareData, TradeFixCompareData>(
                                pConnection, pParameters, helperFixTradeSpheres, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                case (CompareOptions.Spheres | CompareOptions.FIXml | CompareOptions.Positions):
                    {

                        CompareHelper<PositionFixCompareData, PositionFixCompareData> helperFixPositionSpheres =
                                                new CompareHelper<PositionFixCompareData, PositionFixCompareData>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<PositionFixCompareData, PositionFixCompareData>(
                                pConnection, pParameters, helperFixPositionSpheres, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                case (CompareOptions.Eurosys | CompareOptions.FIXml | CompareOptions.TradesInPosition):
                    {
                        CompareHelper<TradesInPositionFixCompareData, TradesInPositionFixCompareDataEurosys> helperFixInPositionGiveUpEurosys =
                                                new CompareHelper<TradesInPositionFixCompareData, TradesInPositionFixCompareDataEurosys>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<TradesInPositionFixCompareData, TradesInPositionFixCompareDataEurosys>(
                                pConnection, pParameters, helperFixInPositionGiveUpEurosys, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                case (CompareOptions.Spheres | CompareOptions.FIXml | CompareOptions.TradesInPosition):
                    {
                        CompareHelper<TradesInPositionFixCompareData, TradesInPositionFixCompareData> helperFixInPositionGiveUp =
                                                new CompareHelper<TradesInPositionFixCompareData, TradesInPositionFixCompareData>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<TradesInPositionFixCompareData, TradesInPositionFixCompareData>(
                                pConnection, pParameters, helperFixInPositionGiveUp, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                // UNDONE 20120214 FIXml n'a rien à voir, le standard des données externes est un mélange entre Clearnet et EFS
                case (CompareOptions.Spheres | CompareOptions.FIXml | CompareOptions.CashFlows):
                    {
                        CompareHelper<CashFlowsCompareData, CashFlowsCompareData> helperCashFlow =
                                                new CompareHelper<CashFlowsCompareData, CashFlowsCompareData>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<CashFlowsCompareData, CashFlowsCompareData>(
                                pConnection, pParameters, helperCashFlow, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                // UNDONE 20120214 FIXml n'a rien à voir, le standard des données externes est un mélange entre Clearnet et EFS
                case (CompareOptions.Spheres | CompareOptions.FIXml | CompareOptions.CashFlowsInstr):
                    {
                        CompareHelper<CashFlowsInstrCompareData, CashFlowsInstrCompareData> helperCashFlowInstr =
                                                new CompareHelper<CashFlowsInstrCompareData, CashFlowsInstrCompareData>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<CashFlowsInstrCompareData, CashFlowsInstrCompareData>(
                                pConnection, pParameters, helperCashFlowInstr, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                // UNDONE 20120214 FIXml n'a rien à voir, le standard des données externes est un mélange entre Clearnet et EFS
                case (CompareOptions.Eurosys | CompareOptions.FIXml | CompareOptions.CashFlows):
                    {
                        CompareHelper<CashFlowsCompareData, CashFlowsCompareDataEurosys> helperCashFlowEurosys =
                                                new CompareHelper<CashFlowsCompareData, CashFlowsCompareDataEurosys>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<CashFlowsCompareData, CashFlowsCompareDataEurosys>(
                                pConnection, pParameters, helperCashFlowEurosys, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                // UNDONE 20120214 FIXml n'a rien à voir, le standard des données externes est un mélange entre Clearnet et EFS
                case (CompareOptions.Eurosys | CompareOptions.FIXml | CompareOptions.CashFlowsInstr):
                    {
                        CompareHelper<CashFlowsInstrCompareData, CashFlowsInstrCompareDataEurosys> helperCashFlowInstrEurosys =
                                                new CompareHelper<CashFlowsInstrCompareData, CashFlowsInstrCompareDataEurosys>(pTask);

                        pGlobalStatus = ExecuteSpecificCompare<CashFlowsInstrCompareData, CashFlowsInstrCompareDataEurosys>(
                                pConnection, pParameters, helperCashFlowInstrEurosys, ref rowCount, ref tableIoTrackCompare);
                    }
                    break;

                default:
                    throw new NotSupportedException(
                        String.Format("Compare options type [{0}] is not yet supported. Comparison process aborted.", pCompareOptions));
            }

            return tableIoTrackCompare;
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private static MatchStatus ExecuteSpecificCompare<TExternal, TInternal>(
            IDbConnection pConnection, ParamsCollection pParameters, 
            CompareHelper<TExternal, TInternal> pHelper,
            ref int rowCount, ref DataTable tableIoTrackCompare)
            where TExternal : ISpheresComparer
            where TInternal : ISpheresComparer
        {
            Logger.Log(new LoggerData(LogLevelEnum.None, String.Format("Step 2/{0} : Loading", MAXSTEP)));

            // 1. Load External data (from EXTLDATA/EXTLDATADET tables)
            rowCount = pHelper.LoadExternalDataSet(pConnection, pParameters);
            Logger.Log(new LoggerData(LogLevelEnum.None, String.Format("External data - Rows: {0}", rowCount)));

            // 2. Load internal data
            rowCount = pHelper.LoadInternalDataSet(pConnection, pParameters);
            Logger.Log(new LoggerData(LogLevelEnum.None, String.Format("Internal data - Rows: {0}", rowCount)));

            // 2. Compare data
            Logger.Log(new LoggerData(LogLevelEnum.None, String.Format("Step 3/{0} : Comparing", MAXSTEP)));

            tableIoTrackCompare = pHelper.Compare(out MatchStatus pGlobalStatus);

            // 3 retunr comparison status

            return pGlobalStatus;
        }

        /// <summary>
        /// Saving the comparison highlights
        /// </summary>
        /// <remarks>Add a row in the IOTRACK table</remarks>
        /// <param name="pConnection"></param>
        /// <param name="pReportIOTRACK">the current IOTrack object containing the comparison highlights to insert in the IOTRACK table</param>
        /// <returns>the last IDIOTRACK inserted in the IOTRACK table</returns>
        private int WriteHighLight(IDbConnection pConnection, IOTrack pReportIOTRACK)
        {
            m_Task.AddIOTrackLog(
                pReportIOTRACK.Message,
                pReportIOTRACK.Datas[1],
                pReportIOTRACK.Datas[3],
                Enum.GetName(typeof(MatchStatus), pReportIOTRACK.Status),
                pReportIOTRACK.DtBusiness,
                IOTrack.PARAM_DTBUSINESS,
                Enum.GetName(typeof(CompareOptions), pReportIOTRACK.ExtDataFormat));

            //PLGLOP Un ExecuteScalar serait plus performant (non urgent)
            
            //string query = String.Format(@"select IDIOTRACK from dbo.IOTRACK where IDIOTASKDET={0} and IDPROCESS_L={1}",
            //                                m_Task.IoTask.taskDet.id,
            //                                m_Task.process.processLog.header.IdProcess);
            string query = String.Format(@"select IDIOTRACK from dbo.IOTRACK where IDIOTASKDET={0} and IDPROCESS_L={1}",
                                            m_Task.IoTask.taskDet.id,
                                            m_Task.Process.IdProcess);

            List<IOTrack> list = DataHelper<IOTrack>.ExecuteDataSet(pConnection, CommandType.Text, query);

            int iIoTrack;
            switch (list.Count)
            {
                // Just one single IOCompare record  is accepted    
                case 1:

                    IOTrack track = list[0];
                    iIoTrack = track.IdIoTrack;

                    break;
                case 0:
                default:
                    throw new InvalidOperationException(
                        String.Format(@"{0} IOTrack element(s) found for the current compare process, just one single line is accepted. 
                                        Process aborted.",
                            list.Count));
            }

            return iIoTrack;
        }

        /// <summary>
        /// Write all the comparison details in the IOTRACKCOMPARE table
        /// </summary>
        /// <param name="pConnectionString">Connection string for the target DBMS, 
        /// used to choose which kind of writing action will be performed</param>
        /// <param name="pTableIoTrackCompare">The DataTable object to be saved in the IOTRACKCOMPARE table</param>
        /// <param name="pReportIOTRACK">the HighLight information object (relative to the IOTRACK parent table)</param>
        private void WriteDetails(string pConnectionString, DataTable pTableIoTrackCompare, IOTrack pReportIOTRACK)
        {
            if (pTableIoTrackCompare == null)
                throw new ArgumentNullException("tableIoTrackCompare",
                    "The data set containing the comparison results has not been validated. Process aborted.");

            foreach (DataRow row in pTableIoTrackCompare.Rows)
                row["IDIOTRACK"] = pReportIOTRACK.IdIoTrack;

            switch (DataHelper.GetDbSvrType(pConnectionString))
            {
                case DbSvrType.dbSQL:

                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(pConnectionString, SqlBulkCopyOptions.TableLock);

                    Type schemaTableIoTrackCompare = typeof(IOTRACKCOMPARE);

                    sqlBulkCopy.DestinationTableName = schemaTableIoTrackCompare.Name;

                    sqlBulkCopy.WriteToServer(pTableIoTrackCompare);

                    break;

                case DbSvrType.dbORA:
                default:

                    DataHelper.ExecuteDataAdapter(pConnectionString,
                        String.Format("select * from dbo.IOTRACKCOMPARE where IDIOTRACK={0}", pReportIOTRACK.IdIoTrack),
                        pTableIoTrackCompare);

                    break;
            }
        }
    }

    #endregion Comparison classes
}

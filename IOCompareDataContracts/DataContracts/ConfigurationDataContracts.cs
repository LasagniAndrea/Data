using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace IOCompareCommon.DataContracts
{

    /// <summary>
    /// Class including the optional SQL informations for an ExceptParam object. 
    /// Any ConfigurationSQLInfo instance maps one SQL field with the relative ExceptParam object.
    /// </summary>
    [DataContract(
        Name = "SQLInfo",
        Namespace = "")]
    public class ConfigurationSQLInfo
    {
        /// <summary>
        /// Sub-types list for EXCEPT parameters
        /// </summary>
        public enum GroupByRuleEnum
        {
            /// <summary>
            /// the parameter will be used inside inside a group by
            /// </summary>
            GroupBy,
            /// <summary>
            /// the parameter make part of the key for a compared element, all the parameters marked with this property will replace the default 
            /// unique integer Id. The resulting Id will be the concatenation of all the parameters marked as Key. 
            /// Usually defined to identify Eurosys internal compare elements.
            /// </summary>
            /// <remarks>To be used in alternance with the ISupportKey interface <see cref="IOCompareCommon.Interfaces.ISupportKey"/>, do NOT use 
            /// the Key property when your DataContract already implements the ISupportKey interface</remarks>
            Key,
            /// <summary>
            /// the parameter will be calculated as minimum for aggregated datas (min(x))
            /// </summary>
            SelectMin,
            /// <summary>
            /// the parameter will be calculated as sum for aggregated datas (sum(x))
            /// </summary>
            SelectSum,
            /// <summary>
            /// the parameter could be used for a group by (see GroupBy)
            /// (IFF except == false AND EXISTS almost one "group by" parameter inside of the current template)
            /// </summary>
            Dynamic,
            /// <summary>
            /// the parameter will be used inside a group by. in case an higher query is built the join condition on the GroupByNull parameter 
            /// is enriched with the "is null" condition.
            /// </summary>
            GroupByNull,
            
        }

        private string m_sqlTable;

        /// <summary>
        /// The table where the mapped SQL field is defined.
        /// </summary>
        [DataMember(Order = 1)]
        public string SQLTable
        {
            get { return m_sqlTable; }
            set { m_sqlTable = value; }
        }

        private string m_sqlField;

        /// <summary>
        /// The SQl field name.
        /// </summary>
        [DataMember(Order = 2)]
        public string SQLField
        {
            get { return m_sqlField; }
            set { m_sqlField = value; }
        }

        private string m_sqlType;

        /// <summary>
        /// The SQL field type.
        /// </summary>
        [DataMember(Order = 3)]
        public string SQLType
        {
            get { return m_sqlType; }
            set { m_sqlType = value; }
        }

        // RD 20131129 [19272] Compare clearer data
        // - Add m_sqlOperator member       
        private string m_sqlOperator;

        /// <summary>
        /// <seealso cref="GroupByRuleEnum"/>
        /// </summary>
        [DataMember(Order = 4)]
        public string SQLOperator
        {
            get { return m_sqlOperator; }
            set { m_sqlOperator = value; }
        }

        private string m_xPath;

        /// <summary>
        /// Optional XPath used when the the SQL field type is type of XML
        /// </summary>
        [DataMember(Order = 5)]
        public string XPath
        {
            get { return m_xPath; }
            set { m_xPath = value; }
        }

        private string m_xPathType;

        /// <summary>
        /// Optional XML internal type used when the the SQL field type is type of XML
        /// </summary>
        [DataMember(Order = 6)]
        public string XPathType
        {
            get { return m_xPathType; }
            set { m_xPathType = value; }
        }

        private string m_groupByRule;

        /// <summary>
        /// <seealso cref="GroupByRuleEnum"/>
        /// </summary>
        [DataMember(Order = 7)]
        public string GroupByRule
        {
            get { return m_groupByRule; }
            set { m_groupByRule = value; }
        }

        /// <summary>
        /// The sub-type of the current Except parameter
        /// <seealso cref="GroupByRuleEnum"/>
        /// </summary>
        public GroupByRuleEnum TypedGroupByRule
        {
            get { return (GroupByRuleEnum)Enum.Parse(typeof(GroupByRuleEnum), m_groupByRule); }
        }
    }

    /// <summary>
    /// Class containing the mandatory configuration for any ExceptParam (or extended) object.
    /// </summary>
    [DataContract(
        Name = "Parameter",
        Namespace = "")]
    public class ConfigurationParameter
    {
        private string m_id;

        [DataMember(Order = 1)]
        public string Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private string m_type;

        /// <summary>
        /// Type of the parameter data (int/decimal/etc)
        /// </summary>
        [DataMember(Order = 2)]
        public string Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        private List<ConfigurationSQLInfo> m_sqlInfo;

        /// <summary>
        /// SQL configurations collection.
        /// Each element inside of the list maps one single SQL field.
        /// <see cref="ConfigurationSQLInfo"/>
        /// </summary>
        [DataMember(Order = 3)]
        public List<ConfigurationSQLInfo> SQLInfos
        {
            get { return m_sqlInfo; }
            set { m_sqlInfo = value; }
        }
    }

}

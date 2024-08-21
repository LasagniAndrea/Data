using System;

using EFS.SpheresIO;

namespace IOCompareCommon.Interfaces
{

    /// <summary>
    /// Struct associating a generic value to a specific error status
    /// </summary>
    public struct ValueErrorStatus
    {
        /// <summary>
        /// Matching generic decimal value
        /// </summary>
        public decimal Value;

        /// <summary>
        /// Error message in case of the data to match is different
        /// </summary>
        public MatchStatus ErrorStatus;

        /// <summary>
        /// Get/Set the value may enter the comparison process
        /// </summary>
        public bool Enabled;

        // MF 20120924 - Ticket 18149
        /// <summary>
        /// Tolerance value to be applied during compare
        /// </summary>
        public decimal Epsilon; 

        /// <summary>
        /// Build a new matching data including its own error message
        /// </summary>
        /// <param name="pValue">value to be compared</param>
        /// <param name="pErrorStatus">error message</param>
        public ValueErrorStatus(decimal pValue, MatchStatus pErrorStatus)
        {
            Value = pValue;
            ErrorStatus = pErrorStatus;
            Enabled = true;
            Epsilon = 0;
        }

        /// <summary>
        /// Build a new matching data including its own error message, and activation status
        /// </summary>
        /// <param name="pValue">value to be compared</param>
        /// <param name="pErrorStatus">error message</param>
        /// <param name="pEnabled">activation status of the current value, get/det the value may enter the comparison process</param>
        public ValueErrorStatus(decimal pValue, MatchStatus pErrorStatus, bool pEnabled)
        {
            Value = pValue;
            ErrorStatus = pErrorStatus;
            Enabled = pEnabled;
            Epsilon = 0;
        }


        /// <summary>
        /// Build a new matching data including its own error message, activation status and tolerance value (epsilon)
        /// </summary>
        /// <param name="pValue">value to be compared</param>
        /// <param name="pErrorStatus">error message</param>
        /// <param name="pEnabled">activation status of the current value, get/det the value may enter the comparison process</param>
        /// <param name="pEpsilon"></param>
        public ValueErrorStatus(decimal pValue, MatchStatus pErrorStatus, bool pEnabled, decimal pEpsilon)
            : this(pValue, pErrorStatus, pEnabled)
        {
            Epsilon = pEpsilon;
        }

        /// <summary>
        /// Check the activation status of the input elements for the comparison process
        /// </summary>
        /// <param name="pFirst">first element to check</param>
        /// <param name="pSecond">second element to check</param>
        /// <returns></returns>
        public static bool CompareEnabled(ValueErrorStatus pFirst, ValueErrorStatus pSecond)
        {
            bool enabled = false;

            if (pFirst.Enabled && pSecond.Enabled)
            {
                enabled = true;
            }

            return enabled;
        }

        // MF 20120924 - Ticket 18149
        /// <summary>
        /// Compare the current value with the input one
        /// </summary>
        /// <param name="pMatchedValue"></param>
        /// <returns></returns>
        public bool EpsilonEquals(ValueErrorStatus pMatchedValue)
        {
            decimal diff = Math.Abs(this.Value - pMatchedValue.Value);

            if (diff <= this.Epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Compare the the two input values
        /// </summary>
        /// <param name="pMatchedValue1"></param>
        /// <param name="pMatchedValue2"></param>
        /// <returns></returns>
        public bool EpsilonEquals(decimal pMatchedValue1, decimal pMatchedValue2)
        {
            decimal diff = Math.Abs(pMatchedValue1 - pMatchedValue2);

            if (diff <= this.Epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Interface including all the base functionalities to perform a compare
    /// </summary>
    /// <remarks>
    /// Any object designed to be compared must implement this interface.
    /// </remarks>
    public interface ISpheresComparer
    {
        /// <summary>
        /// Initialise the internal comparison structures
        /// </summary>
        /// <remarks>
        /// The Qtys and ComparisonKey property should be accessible only after this call
        /// </remarks>
        void Initialise();

        /// <summary>
        /// Get the values to be compared for the matching check
        /// </summary>
        ValueErrorStatus[] Values
        { get; }

        /// <summary>
        /// Get a specific quantity/errorstatus value
        /// </summary>
        /// <param name="key">the key identifying which quantity/errorstatus will be returned</param>
        /// <returns>the quantity/errorstatus value relative to the specific key </returns>
        ValueErrorStatus QtyErrorStatusByKey(string keyname);

        /// <summary>
        /// Get the fields to be compared for the equality check
        /// </summary>
        object[] ComparisonKey
        { get; }

        /// <summary>
        /// Message aggregating not grouped values for query using the group by clause
        /// </summary>
        string Message
        {
            get;
        }

        /// <summary>
        /// Unique Id of the element
        /// </summary>
        /// <value>can be null</value>
        object Id
        {
            get;
        }

        /// <summary>
        /// Alternative unique Id for the element
        /// </summary>
        /// <value>can be null</value>
        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        DateTime Age_Enreg_Eurosys
        {
            get;
        }

        /// <summary>
        /// Alternative unique Id for the element
        /// </summary>
        /// <value>can be null</value>
        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        string Time_Enreg_Eurosys
        {
            get;
        }

        /// <summary>
        /// Alternative unique Id for the element
        /// </summary>
        /// <value>can be null</value>
        [Obsolete("Used to be part of the EUROSYS key to identify one internal element, not used anymore. Use the file CompareParameters.xml instead. GroupByRule := Key", true)]
        string Typ_Compte_Eurosys
        {
            get;
        }
    }

    /// <summary>
    /// Supplementary key value to identify a compare element <seealso cref="ISpheresComparer"/> inside of the IOTRACKCOMPARE table.
    /// Usually defined to identify Spheres F&O/OTCml internal compare elements.
    /// </summary>
    /// <remarks>
    /// This MUST be used in alternance with the Key value of the GroupByRuleEnum enumeration 
    /// <seealso cref="IOCompareCommon.DataContracts.ConfigurationSQLInfo.GroupByRuleEnum"/>. Do not implement
    /// the ISupportKey interface for any compare element using the Key value of the GroupByRuleEnum enumeration.
    /// </remarks>
    public interface ISupportKey
    {
        /// <summary>
        /// Secondary Id of the element, to be used together with the primary key. 
        /// Active just when the IOCompare element <seealso cref="ISpheresComparer"/> DO not implement 
        /// the IIoTrackElements interface <seealso cref="IIoTrackElements"/>.
        /// </summary>
        /// <value>can be null</value>
        object SupportValueId
        {
            get;
        }

        /// <summary>
        /// Secondary Id of the element, to be used together with the primary key.
        /// Active just when the IOCompare element <seealso cref="ISpheresComparer"/> IMPLEMENTS
        /// the IIoTrackElements interface <seealso cref="IIoTrackElements"/>.
        /// </summary>
        /// <param name="pElemName">name of the element basing on we return a specific identifier</param>
        /// <returns>the support Id</returns>
        object GetSupportValueIdByElem(string pElemName);
    }
}

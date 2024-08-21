using System;
using System.Runtime.Serialization;
using EFS.ApplicationBlocks.Data;

namespace IOCompareCommon.DataContracts
{

    /// <summary>
    /// Class containing some informations for the current compare process.
    /// </summary>
    /// <remarks>
    /// Each IOCompare instance is relative to a single IOCOMPARE line
    /// </remarks>
    [DataContract(
        Name = DataHelper<IOCompare>.DATASETROWNAME,
        Namespace = DataHelper<IOCompare>.DATASETNAMESPACE)]
    public sealed class IOCompare
    {
        private string m_IdIoCompare;

        [DataMember(Name = "IDIOCOMPARE", Order = 1)]
        public string IdIoCompare
        {
            get { return m_IdIoCompare; }
            set { m_IdIoCompare = value; }
        }

        private string m_DisplayName;

        [DataMember(Name = "DISPLAYNAME", Order = 2)]
        public string DisplayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        private string m_DataSource;

        /// <summary>
        /// Get/Set The system string identifier where the process will be executed (Spheres/Eurosys/etc)
        /// </summary>
        [DataMember(Name = "INTDATASOURCE", Order = 3)]
        public string DataSource
        {
            get { return m_DataSource; }
            set { m_DataSource = value; }
        }

        private string m_DataType;

        /// <summary>
        /// get/set The data type of the elements being compared (Trades/Positions/etc)
        /// </summary>
        [DataMember(Name = "INTDATATYPE", Order = 4)]
        public string DataType
        {
            get { return m_DataType; }
            set { m_DataType = value; }
        }

        private string m_DataFormat;

        /// <summary>
        /// get/set the standard used to store the external data (FIXml/OTCml/etc)
        /// </summary>
        [DataMember(Name = "EXTDATAFORMAT", Order = 5)]
        public string DataFormat
        {
            get { return m_DataFormat; }
            set { m_DataFormat = value; }
        }

        private DateTime m_Enabled;

        [DataMember(Name = "DTENABLED", Order = 6)]
        public DateTime Enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

    }

}

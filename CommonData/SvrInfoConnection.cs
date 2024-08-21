using System;
using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{
    /// <summary>
    /// Liste des informations du serveur de bases de données 
    /// obtenues via une connection avec ConnectionString du message
    /// (ServerType, ServerName, ServerVersion, HostName etc.
    /// </summary>
    // EG 20181010 PERF
    public class SvrInfoConnection
    {
        public SvrInfoConnection() { ProviderVersion = Cst.NotAvailable; }
        #region Members
        /// <summary>
        /// Type of server
        /// </summary>
        public DbSvrType ServerType { set; get; }
        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected.
        /// </summary>
        public string ServerVersion { set; get; }
        /// <summary>
        /// Specifies the name of the service to which the connection is set
        /// </summary>
        public string ServiceName { set; get; }
        /// <summary>
        /// Gets the name of the current database after a connection is opened, 
        /// or the database name specified in the connection string before the connection is opened.
        /// </summary>
        public string Database { set; get; }
        /// <summary>
        /// Specifies the name of the database domain to which the connection is set (Oracle)
        /// </summary>
        public string DatabaseDomainName { set; get; }
        /// <summary>
        /// Specifies the name of the database to which the connection is set
        /// </summary>
        public string DatabaseName { set; get; }
        /// <summary>
        /// Specifies the Oracle Net Services Name, Connect Descriptor, or an easy connect naming that identifies the database to which to connect
        /// </summary>
        public string DataSource { set; get; }
        /// <summary>
        /// Specifies the name of the host to which the connection is set
        /// </summary>
        public string HostName { set; get; }
        /// <summary>
        /// Specifies the name of the host to which the connection is set
        /// </summary>
        public string InstanceName { set; get; }
        /// <summary>
        /// Specifies the Provider Version (Oracle v19 only)
        /// </summary>
        public string ProviderVersion { set; get; }
        #endregion Members
        #region Accessors
        // EG 20181119 PERF Correction post RC (Step 2)
        public bool IsNoHints
        {
            get { return true; }
        }
        // EG 20181119 PERF Correction post RC
        public bool IsOracle
        {
            get { return (ServerType == DbSvrType.dbORA); }
        }
        public bool IsSQLServer
        {
            get { return (ServerType == DbSvrType.dbSQL); }
        }
        public bool IsOraDBVer10gR2OrHigher
        {
            get { return IsOraDBVersionHigher("10.2"); }
        }
        public bool IsOraDBVer11gR1OrHigher
        {
            get { return IsOraDBVersionHigher("11.1"); }
        }
        public bool IsOraDBVer11gR2OrHigher
        {
            get { return IsOraDBVersionHigher("11.2"); }
        }
        public bool IsOraDBVer12cR1OrHigher
        {
            get { return IsOraDBVersionHigher("12.1"); }
        }
        public bool IsSQLDBVer2012OrHigher
        {
            get { return IsOraDBVersionHigher("11.0"); }
        }
        public bool IsSQLDBVer2014OrHigher
        {
            get { return IsOraDBVersionHigher("12.0"); }
        }
        public bool IsSQLDBVer2016OrHigher
        {
            get { return IsOraDBVersionHigher("13.0"); }
        }
        public bool IsSQLDBVer2017OrHigher
        {
            get { return IsOraDBVersionHigher("14.0"); }
        }

        #endregion Accessors
        #region Methods
        public bool IsDBVersionHigher(string pVersion)
        {
            return (new Version(ServerVersion).CompareTo(new Version(pVersion)) >= 0);
        }
        // EG 20181119 PERF Correction post RC
        public bool IsOraDBVersionHigher(string pVersion)
        {
            return IsOracle && IsDBVersionHigher(pVersion);
        }
        public bool IsSqlDBVersionHigher(string pVersion)
        {
            return IsSQLServer && IsDBVersionHigher(pVersion);
        }
        #endregion Methods
    }
}


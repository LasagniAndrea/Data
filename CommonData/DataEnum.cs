using System;

namespace EFS.ApplicationBlocks.Data
{
    #region Enum
    /// <summary>
    /// Used to indicated which database server type is being used
    /// </summary>
    public enum DbSvrType
    {
        /// <summary>SQL Server</summary>
        dbSQL,
        /// <summary>Oracle</summary>
        dbORA,
        /// <summary>DB2 - not yet implemented</summary>
        dbDB2,
        /// <summary>Sybase - not yet implemented</summary>
        dbSybase,
        /// <summary>OLEDB</summary>
        dbOLEDB,
        /// <summary>Unknown - will throw error</summary>
        dbUNKNOWN
    }

    #region public enum DataHelperSQLErrorEnum
    public enum SQLErrorEnum
    {
        /// <summary>Deadlock detected</summary>
        DeadLock,
        /// <summary>Cannot insert duplicate key row in object with unique index.</summary>
        DuplicateKey,
        /// <summary>Update not performed, the data  has been modified by a different user.</summary>
        Concurrency,
        /// <summary>Integrity constraint violated - child record found.</summary>
        Integrity,
        /// <summary>Cannot insert the value NULL into column, column does not allow nulls. INSERT fails.</summary>
        NullValue,
        /// <summary>OleDb error occured</summary>
        OleDb,
        /// <summary>Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.</summary>
        Timeout,
        /// <summary>Not yet managed</summary>
        Unknown,
    }
    #endregion
    #endregion
}

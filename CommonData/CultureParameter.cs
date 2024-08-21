namespace EFS.ApplicationBlocks.Data
{

    /// <summary>
    /// Culture proprieties collection interface. 
    /// Add this interface to any data access class needs special culture configuration.
    /// </summary>
    /// <remarks>
    /// Add new proprieties at will
    /// </remarks>
    public interface ICultureParameter
    {
        /// <summary>
        /// Set/Get the sorting strategy (order by command) of the RDBMS
        /// </summary>
        string Sort { set; get; }

        /// <summary>
        /// Set/Get the compare strategy (where, like, contains commands) of the RDBMS
        /// </summary>
        string Compare { set; get; }
    }
    
}


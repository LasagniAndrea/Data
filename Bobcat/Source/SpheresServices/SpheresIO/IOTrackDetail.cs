using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFS.SpheresIO
{
    /// <summary>
    /// Compteurs IOTRACK par Table SQL
    /// </summary>
    public class IOTrackDetail
    {
        public string TableName;
        public string TableDescription;
        public int InsertedRows;
        public int UpdatedRows;
        public int DeletedRows;

        public IOTrackDetail() { }
        public IOTrackDetail(string pTableName) { TableName = pTableName; }
        public IOTrackDetail(string pTableName, string pTableDescription) { TableName = pTableName; TableDescription = pTableDescription; }
    }
}

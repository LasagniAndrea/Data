using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;


namespace EFS.Common
{


    /// <summary>
    /// Contient, en mémoire, des datas enabled (chaque donnée est de type MapDataReaderRow
    /// </summary>
    public abstract class DataEnabledReaderBase : DataEnabledBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected DataEnabledReaderBase() : base()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtReference"></param>
        protected DataEnabledReaderBase(string cs, IDbTransaction dbTransaction, DateTime dtReference) : base(cs, dbTransaction, dtReference)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual QueryParameters GetQuery()
        {
            throw new NotImplementedException("GetQuery method is not implemented");
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void LoadData()
        {

            QueryParameters qry = GetQuery();

            using (IDataReader dr = DataHelper.ExecuteReader(CS, DbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
            {
                List<MapDataReaderRow> row = DataReaderExtension.DataReaderMapToList(dr);
                base.SetData(row);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected List<MapDataReaderRow> GetData()
        {
            return base.GetData<List<MapDataReaderRow>>();
        }
    }

}

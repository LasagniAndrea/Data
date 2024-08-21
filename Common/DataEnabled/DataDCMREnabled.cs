using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;

namespace EFS.Common
{
    /// <summary>
    /// Représente, en mémoire, les DerivativeContract avec leur(s) règle(s) d'échéances associées enabled 
    /// <para>Chargement via VW_DRVCONTRACTMATRULE</para>
    /// </summary>
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.DRVCONTRACTMATRULE)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.DERIVATIVECONTRACT)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.MATURITYRULE)]
    public class DataDCMREnabled : DataEnabledReaderBase 
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetKey()
        {
            return new DataDCMREnabled().GetType().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string Key => GetKey();


        /// <summary>
        /// 
        /// </summary>
        public DataDCMREnabled() : base()
        {

        }

        /// <summary>
        /// Représente, en mémoire, les DerivativeContract avec leur(s) règle(s) d'échéances associées 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        public DataDCMREnabled(string cs, IDbTransaction dbTransaction) : this(cs, dbTransaction, DateTime.MinValue)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtReference"></param>
        public DataDCMREnabled(string cs, IDbTransaction dbTransaction, DateTime dtReference) : base(cs, dbTransaction, dtReference)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQuery()
        {
            string query = $@"select *
from dbo.VW_DRVCONTRACTMATRULE dcMR
{GetWhereDtReference("dcMR")}";

            DataParameters dp = new DataParameters();
            QueryParameters qry = new QueryParameters(CS, query, dp);

            return qry;
        }



        /// <summary>
        /// Retourne les dérivative contract qui utilisent la MR <paramref name="idMR"/>
        /// </summary>
        /// <param name="idMR"></param>
        /// <returns></returns>
        public IEnumerable<int> LoadDCUsingMR(int idMR)
        {
            return (from item in GetData().Where(x => Convert.ToInt32(x["IDMATURITYRULE"].Value) == idMR)
                    select Convert.ToInt32(item["IDDC"].Value)).Distinct();
        }

        /// <summary>
        ///  Retourne tous les enregistrements sous le type <seealso cref="DataDCMR"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataDCMR> GetDCMR()
        {
            //Remarque L'usage de this.Rows.Cast<DCMR>() ne fonctionne pas ? (invalidcastException)
            return this.GetData().Select(x => (DataDCMR)x);
        }

        /// <summary>
        ///  Retourne les enregistrements associés à <paramref name="pIdDC"/> sous le type <seealso cref="DataDCMR"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataDCMR> GetDCMR(int pIdDC)
        {
            //Remarque L'usage de this.Rows.Cast<DCMR>() ne fonctionne pas ? (invalidcastException)
            return this.GetData().Where(x => Convert.ToInt32(x["IDDC"].Value) == pIdDC).Select(x => (DataDCMR)x).OrderBy(y => y.SequenceNumber);
        }

        /// <summary>
        /// Suppresssion des VW_DRVCONTRACTMATRULE du cache en relation avec la base de donnée <paramref name="cs"/> 
        /// </summary>
        /// <param name="cs"></param>
        public static new int ClearCache(string cs)
        {
            return ClearCache(cs, GetKey());
        }

    }

    /// <summary>
    /// Représente les élements de la vue VW_DRVCONTRACTMATRULE
    /// </summary>
    public class DataDCMR
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 IdDC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DCIdentifier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int32 IdMR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MRType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Cst.MaturityMonthYearFmtEnum MaturityFormat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int32 SequenceNumber { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public static explicit operator DataDCMR(MapDataReaderRow item)
        {
            DataDCMR dCMR = new DataDCMR
            {
                IdDC = Convert.ToInt32(item["IDDC"].Value),
                DCIdentifier = Convert.ToString(item["DC"].Value),
                IdMR = Convert.ToInt32(item["IDMATURITYRULE"].Value),
                MRType = Convert.ToString(item["MATURITYRULETYPE"].Value),
                MaturityFormat = ReflectionTools.ConvertStringToEnum<Cst.MaturityMonthYearFmtEnum>(Convert.ToString(item["MMYFMT"].Value)),
                SequenceNumber = Convert.ToInt32(item["SEQUENCENO"].Value)
            };
            return dCMR;
        }
    }
}

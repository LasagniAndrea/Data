using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EfsML.Enum;

namespace EFS.Common.DataEnabled
{


    /// <summary>
    /// Représente, en mémoire, les instruments enabled
    /// <para>Chargement via VW_INSTR_PRODUCT</para>
    /// </summary>

    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.INSTRUMENT)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.PRODUCT)]
    public class DataInstrumentEnabled : DataEnabledReaderBase
    {
        protected override string Key => GetKey();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetKey()
        {
            return new DataInstrumentEnabled().GetType().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        public DataInstrumentEnabled() : base()
        {

        }

        /// <summary>
        /// Représente, en mémoire, tous les instruments
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        public DataInstrumentEnabled(string cs, IDbTransaction dbTransaction) : this(cs, dbTransaction, DateTime.MinValue)
        {
        }


        /// <summary>
        /// Représente, en mémoire, tous les instruments actif le <paramref name="dtReference"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtReference"></param>
        public DataInstrumentEnabled(string cs, IDbTransaction dbTransaction, DateTime dtReference) : base(cs, dbTransaction, dtReference)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQuery()
        {
            string query = $@"select 
IDI, IDENTIFIER, IDP, PRODUCT_IDENTIFIER,
ISIFRS, ISEVENTS, ISFUNDING, ISMARGINING, ISNOINTEVENTS, ISOPEN, ISEXTEND, ISACCOUNTING,
CLASS,
GPRODUCT,FAMILY,
PRODUCT_FUNGIBILITYMODE,
FUNGIBILITYMODE
from dbo.VW_INSTR_PRODUCT i
{GetWhereDtReference("i")}";

            DataParameters dp = new DataParameters();
            QueryParameters qry = new QueryParameters(CS, query, dp);

            return qry;
        }

        /// <summary>
        ///  Retourne tous les enregistrements sous le type <seealso cref="DataMarket"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataInstrument> GetDataInstrument()
        {
            //Remarque L'usage de this.Rows.Cast<DataInstrument>() ne fonctionne pas ? (invalidcastException)
            return this.GetData().Select(x => (DataInstrument)x);
        }

        /// <summary>
        /// Suppresssion des instruments du cache en relation avec la base de donnée <paramref name="cs"/> 
        /// </summary>
        /// <param name="cs"></param>
        public static new int ClearCache(string cs)
        {
            return ClearCache(cs, GetKey());
        }
    }

    /// <summary>
    /// Représente les élements de la vue VW_INSTRUEMENT_PRODUCT
    /// </summary>
    public class DataInstrument
    {
        #region Member
        /// <summary>
        /// 
        /// </summary>
        public Int32 IdI { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int32 IdP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Product_Identifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GProduct { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsIFRS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Boolean IsEvents { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Boolean IsFunding { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Boolean IsMargining { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsNoINTEvents { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Boolean IsOpen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsExtend { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsAccounting { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FungibilityModeEnum FungibilityMode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public FungibilityModeEnum Product_FungibilityMode { get; set; }
        #endregion Member

        #region get Properties
        public bool IsFungibilityMode_OPENCLOSE
        {
            get { return this.FungibilityMode == FungibilityModeEnum.OPENCLOSE; }
        }

        /// <summary>
        ///  Obtient True si l'instrument est fongible
        /// </summary>
        /// FI 20170116 [21916] Add
        public bool IsFungible
        {
            get { return this.FungibilityMode != FungibilityModeEnum.NONE; }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsProductClassStrategy
        {

            get { return this.Class == Cst.ProductClass_STRATEGY; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsProductClassRegular
        {
            get { return this.Class == Cst.ProductClass_REGULAR; }
        }
        #endregion get Properties

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public static explicit operator DataInstrument(MapDataReaderRow item)
        {
            DataInstrument ret = new DataInstrument
            {
                IdI = Convert.ToInt32(item["IDI"].Value),
                Identifier = Convert.ToString(item["IDENTIFIER"].Value),
                IdP = Convert.ToInt32(item["IDP"].Value),
                Product_Identifier = Convert.ToString(item["PRODUCT_IDENTIFIER"].Value),
                Class = Convert.ToString(item["CLASS"].Value),
                GProduct = Convert.ToString(item["GPRODUCT"].Value),
                Family = Convert.ToString(item["FAMILY"].Value),
                IsIFRS = Convert.ToBoolean(item["ISIFRS"].Value),
                IsEvents = Convert.ToBoolean(item["ISEVENTS"].Value),
                IsFunding = Convert.ToBoolean(item["ISFUNDING"].Value),
                IsMargining = Convert.ToBoolean(item["ISMARGINING"].Value),
                IsNoINTEvents = Convert.ToBoolean(item["ISNOINTEVENTS"].Value),
                IsOpen = Convert.ToBoolean(item["ISOPEN"].Value),
                IsExtend = Convert.ToBoolean(item["ISEXTEND"].Value),
                IsAccounting = Convert.ToBoolean(item["ISACCOUNTING"].Value),
                FungibilityMode = ReflectionTools.ConvertStringToEnum<FungibilityModeEnum>(Convert.ToString(item["FUNGIBILITYMODE"].Value)),
                Product_FungibilityMode = ReflectionTools.ConvertStringToEnum<FungibilityModeEnum>(Convert.ToString(item["PRODUCT_FUNGIBILITYMODE"].Value)),
            };
            return ret;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public static class DataInstrumentEnabledHelper
    {

        /// <summary>
        ///  Retourne l'instrument avec l'IdI  <paramref name="idI"/>. Retouren null si l'instrument n'existe pas
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="idI"></param>
        /// <returns></returns>
        public static DataInstrument GetDataInstrument(string cs, IDbTransaction dbTransaction, int idI)
        {
            return GetDataInstrument(cs, dbTransaction, DateTime.MinValue, idI);
        }

        /// <summary>
        ///  Retourne l'instrument avec l'IdI  <paramref name="idI"/>. Retouren null si l'instrument n'existe pas ou n'est pas actif en date <paramref name="dtRefence"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtRefence">Date de réfrence</param>
        /// <param name="idI"></param>
        /// <returns></returns>
        public static DataInstrument GetDataInstrument(string cs, IDbTransaction dbTransaction, DateTime dtRefence, int idI)
        {
            IEnumerable<DataInstrument> dataInstrument = new DataInstrumentEnabled(cs, dbTransaction, dtRefence).GetDataInstrument();
            DataInstrument ret = dataInstrument.FirstOrDefault(x => x.IdI == idI);
            return ret;
        }
    }

}

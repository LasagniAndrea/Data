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
    /// Représente, en mémoire, les Marchés enabled
    /// <para>Chargement via VW_MARKET_IDENTIFIER</para>
    /// </summary>

    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.MARKET)]
    public class DataMarketEnabled : DataEnabledReaderBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetKey()
        {
            return new DataMarketEnabled().GetType().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string Key => GetKey();


        /// <summary>
        /// 
        /// </summary>
        public DataMarketEnabled() : base()
        {

        }


        /// <summary>
        /// Représente, en mémoire, tous les marchés
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        public DataMarketEnabled(string cs, IDbTransaction dbTransaction) : this(cs, dbTransaction, DateTime.MinValue)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtReference"></param>
        public DataMarketEnabled(string cs, IDbTransaction dbTransaction, DateTime dtReference) : base(cs, dbTransaction, dtReference)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override QueryParameters GetQuery()
        {
            string query = $@"select 
IDM, IDENTIFIER, SHORTIDENTIFIER, 
ACRONYM, SHORT_ACRONYM, 
IDCOUNTRY, CITY, COUNTRY_CITY_ACRONYM,
FIXML_SecurityExchange, EXCHANGESYMBOL, 
BBGCode, RICCODE,
IDA, TIMEZONE, IDBC
from dbo.VW_MARKET_IDENTIFIER m
{GetWhereDtReference("m")}";

            DataParameters dp = new DataParameters();
            QueryParameters qry = new QueryParameters(CS, query, dp);

            return qry;
        }

        /// <summary>
        ///  Retourne tous les enregistrements sous le type <seealso cref="DataMarket"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataMarket> GetDataMarket()
        {
            //Remarque L'usage de this.Rows.Cast<DataMarket>() ne fonctionne pas ? (invalidcastException)
            return this.GetData().Select(x => (DataMarket)x);
        }


        /// <summary>
        /// Suppresssion des marchés du cache en relation avec la base de donnée <paramref name="cs"/> 
        /// </summary>
        /// <param name="cs"></param>
        public static new int ClearCache(string cs)
        {
            return ClearCache(cs, GetKey());
        }

    }

    /// <summary>
    /// Représente les élements de la vue VW_MARKET_IDENTIFIER
    /// </summary>
    public class DataMarket
    {
        /// <summary>
        /// 
        /// </summary>
        public Int32 IdM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Identifier { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShortIdentifier { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Acronym { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ShortAcronym { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FIXML_SecurityExchange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExchangeSymbol { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BBGCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RICCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int IdA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TimeZone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IdBC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public static explicit operator DataMarket(MapDataReaderRow item)
        {
            DataMarket ret = new DataMarket
            {
                IdM = Convert.ToInt32(item["IDM"].Value),
                Identifier = Convert.ToString(item["IDENTIFIER"].Value),
                ShortIdentifier = Convert.ToString(item["SHORTIDENTIFIER"].Value),
                Acronym = Convert.ToString(item["ACRONYM"].Value),
                ShortAcronym = Convert.ToString(item["SHORT_ACRONYM"].Value),
                City = Convert.ToString(item["CITY"].Value),
                FIXML_SecurityExchange = Convert.ToString(item["FIXML_SecurityExchange"].Value),
                ExchangeSymbol = Convert.ToString(item["EXCHANGESYMBOL"].Value),
                BBGCode = Convert.ToString(item["BBGCODE"].Value),
                RICCode = Convert.ToString(item["RICCODE"].Value),
                IdA = (item["IDA"].Value != Convert.DBNull) ? Convert.ToInt32(item["IDA"].Value) : 0,
                TimeZone = Convert.ToString(item["TIMEZONE"].Value),
                IdBC = Convert.ToString(item["IDBC"].Value),
            };
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class DataMarketEnabledHelper
    {

        /// <summary>
        ///  Retourne le marché avec l'IdM  <paramref name="idM"/>. Retouren null si le marché n'existe pas 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="idM"></param>
        /// <returns></returns>
        public static DataMarket GetDataMarket(string cs, IDbTransaction dbTransaction, int idM)
        {
            return GetDataMarket(cs, dbTransaction, DateTime.MinValue, idM);
        }

        /// <summary>
        ///  Retourne le marché avec l'IdM  <paramref name="idM"/>. Retouren null si le marché n'existe pas ou n'est pas actif en date <paramref name="dtRefence"/>
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="dbTransaction"></param>
        /// <param name="dtRefence">Date de réfrence</param>
        /// <param name="idM"></param>
        /// <returns></returns>
        public static DataMarket GetDataMarket(string cs, IDbTransaction dbTransaction, DateTime dtRefence, int idM)
        {
            IEnumerable<DataMarket> dataMarket = new DataMarketEnabled(cs, dbTransaction, dtRefence).GetDataMarket();
            DataMarket ret = dataMarket.FirstOrDefault(x => x.IdM == idM);
            return ret;
        }
    }
}

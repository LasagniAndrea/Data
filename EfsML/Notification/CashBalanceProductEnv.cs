using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EfsML.Notification
{

    /// <summary>
    ///  Permet d'obtenir des informations sur les produits/instruments des trades de marché impliqués dans un CB
    /// </summary>
    internal class CashBalanceProductEnv
    {
        #region Membre
        /// <summary>
        /// 
        /// </summary>
        private readonly List<DataRow> list;
        #endregion

        /*
        // FI 20160412 [22069] Mise en commentaire de la property count
        /// <summary>
        ///  Retourne le nbr de produits/instruments des trades(*)impliqués dans un CB
        ///  <para>(*) Trade de marché, ou retraits/versements</para>
        /// </summary>
        /// FI 20160225 [XXXXX] Add
        public int count
        {
            get { return list.Count; }
        }
        */



        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">IdT du trade CB</param>
        public CashBalanceProductEnv(string pCS, List<int> pIdT)
        {
            list = LoadCashBalanceProductEnvironment(pCS, pIdT);
        }
        #endregion

        #region Method
        /// <summary>
        ///  Retourne le nbr de produits/instruments des trades(*) impliqués dans un CB
        ///  <para>(*) Trade de marché, ou retraits/versements</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20160412 [22069] Add Method
        public int Count()
        {
            return Count(false);
        }

        /// <summary>
        ///  Retourne le nbr de produits/instruments des trades(*) impliqués dans un CB
        ///  <para>(*) Trade de marché, ou retraits/versements</para>
        ///  <para>Possibilité d'exclure les retraits/versements et les deposit pour considérer uniquement les trades de marché</para>
        /// </summary>
        /// <param name="pIsOnlyTrade"> true pour considérer uniquement les trades de marché</param>
        /// <returns></returns>
        /// FI 20160412 [22069] Add Method
        public int Count(Boolean pIsOnlyTrade)
        {
            int count;
            if (pIsOnlyTrade)
            {
                count = (from item in list.Where(x => (Convert.ToString(x["PRODUCT_IDENTIFIER"]) != Cst.ProductCashPayment) &&
                                                      ((Convert.ToString(x["PRODUCT_IDENTIFIER"]) != Cst.ProductMarginRequirement)))
                         select item).Count();

            }
            else
            {
                count = list.Count;
            }

            return count;
        }



        /// <summary>
        /// Retourne true s'il existe un produit tel que GPRODUCT = 'SEC'
        /// </summary>
        /// <returns></returns>
        public Boolean ExistSEC()
        {

            var count = (from item in list.Where(x => Convert.ToString(x["GPRODUCT"]) == Cst.ProductGProduct_SEC)
                         select item).Count();


            return (count > 0);
        }

        /// <summary>
        /// Retourne true s'il existe un produit tel que GPRODUCT = 'FUT'
        /// </summary>
        /// <returns></returns>
        public Boolean ExistFUT()
        {

            var count = (from item in list.Where(x => Convert.ToString(x["GPRODUCT"]) == Cst.ProductGProduct_FUT)
                         select item).Count();


            return (count > 0);
        }

        /// <summary>
        /// Retourne true s'il existe un produit tel que GPRODUCT = 'OTC'
        /// </summary>
        /// <returns></returns>
        public Boolean ExistOTC()
        {

            var count = (from item in list.Where(x => Convert.ToString(x["GPRODUCT"]) == Cst.ProductGProduct_OTC)
                         select item).Count();


            return (count > 0);
        }

        /// <summary>
        /// Retourne true s'il existe un produit tel que GPRODUCT = 'FX'
        /// </summary>
        /// <returns></returns>
        public Boolean ExistFX()
        {

            var count = (from item in list.Where(x => Convert.ToString(x["GPRODUCT"]) == Cst.ProductGProduct_FX)
                         select item).Count();


            return (count > 0);
        }

        /// <summary>
        /// Retourne true s'il existe des trades de type CommoditySpot
        /// </summary>
        /// <returns></returns>
        /// FI 20161214 [21916]  Add
        public Boolean ExistCOM()
        {
            var count = (from item in list.Where(x => Convert.ToString(x["GPRODUCT"]) == Cst.ProductGProduct_COM)
                         select item).Count();
            return (count > 0);
        }

        /// <summary>
        /// Retourne true s'il existe un instrument tel que ISFUNDING = 1
        /// </summary>
        /// <returns></returns>
        public Boolean ExistFunding()
        {
            var count = (from item in list.Where(x => BoolFunc.IsTrue(x["ISFUNDING"]))
                         select item).Count();


            return (count > 0);
        }


        /// <summary>
        /// Retourne true s'il existe des versements/retraits
        /// </summary>
        /// <returns></returns>
        /// FI 20160229 [XXXXX] Add Method
        /// RD 20160329 [22012] Modify
        /// FI 20170217 [22862] Modify
        public Boolean ExistCashPayment()
        {
            // FI 20170217 [22862] call ExistProduct
            return ExistProduct(Cst.ProductCashPayment);
        }


        /// <summary>
        /// Retourne true s'il existe des trades MarginRequirement
        /// </summary>
        /// <returns></returns>
        /// FI 20160613 [22256] 
        /// FI 20170217 [22862] Modify
        public Boolean ExistMarginRequirement()
        {
            // FI 20170217 [22862] call ExistProduct
            return ExistProduct(Cst.ProductMarginRequirement);
        }


        /// <summary>
        /// Retourne true s'il existe des trades MarginRequirement
        /// </summary>
        /// <returns></returns>
        /// FI 20170217 [22862] Add
        public Boolean ExistProduct(string pProduct)
        {
            var count = (from item in list.Where(x => Convert.ToString(x["PRODUCT_IDENTIFIER"]) == pProduct)
                         select item).Count();

            return (count > 0);
        }

        /// <summary>
        ///  Retourne true s'il existe des produits avec tenu de position (fongible)
        /// </summary>
        /// <returns></returns>
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        public Boolean ExistFungibilityProduct()
        {
            // FI 20170116 [21916] use column FUNGIBILITYMODE (INSTRUMENT)
            var count = (from item in list.Where(x => Convert.ToString(x["FUNGIBILITYMODE"]) != EfsML.Enum.FungibilityModeEnum.NONE.ToString())
                         select item).Count();

            return (count > 0);
        }

        
        /// <summary>
        /// Chgt de l'environnement instrumental (cad liste des produits associés aux trades de marché impliqué dans des CashBalance)
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT">Représente une liste de trade CashBalance</param>
        /// <returns></returns>
        /// FI 20160229 [XXXXX] Modify
        /// FI 20170116 [21916] Modify
        private static List<DataRow> LoadCashBalanceProductEnvironment(string pCs, List<int> pIdT)
        {
            List<DataRow> ret = new List<DataRow>();

            // FI 20170116 [21916] Lecture de la colonne i.FUNGIBILITYMODE
            // FI 20160229 [XXXXX] add CashPaymentInCashBalance
            string query = StrFunc.AppendFormat(@"
select distinct  
i.PRODUCT_IDENTIFIER as PRODUCT_IDENTIFIER, i.GPRODUCT as GPRODUCT, i.FAMILY as FAMILY, i.ISFUNDING, i.ISMARGINING , i.FUNGIBILITYMODE
from dbo.TRADELINK tlcb 
inner join dbo.TRADE t on t.IDT = tlcb.IDT_B
inner join dbo.VW_INSTR_PRODUCT i on i.IDI = t.IDI
where {0} and tlcb.LINK in ('ExchangeTradedDerivativeInCashBalance', 'CashPaymentInCashBalance', 'MarginRequirementInCashBalance')",
DataHelper.SQLColumnIn(pCs, "tlcb.IDT_A", pIdT, TypeData.TypeDataEnum.integer));

            DataTable dt = DataHelper.ExecuteDataTable(pCs, query);
            if (dt.Rows.Count > 0)
                ret = dt.Rows.Cast<DataRow>().ToList();

            return ret;
        }



        #endregion
    }
}
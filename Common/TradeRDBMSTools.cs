#region Using Directives
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.Permission;
using EFS.Status;
 
using EfsML.Enum.Tools;
using EfsML.Enum;
#endregion Using Directives

namespace EFS.Common
{

    /// <summary>
    /// Description résumée de TradeRDBMSTools.
    /// </summary>
    public sealed class TradeRDBMSTools
    {
        private static readonly string SqlDeleteTRADELIST = "delete from dbo.TRADELIST where (SESSIONID=@SESSIONID)";
        // EG 20160404 Migration vs2013
        //private static string SqlInsertTRADELIST = "insert into dbo.TRADELIST (IDT, SESSIONID) values (@IDT, @SESSIONID)";
        public static string SqlInnerTRADELIST = "inner join dbo.TRADELIST lst_t on (lst_t.IDT=ev.IDT) and (lst_t.SESSIONID=@SESSIONID)";
        /// <summary>
        /// Nombre maximum d'éléments IDT dans une liste IN
        /// </summary>
        public static int SqlINListMax = 50;

        /// <summary>
        /// Supprime un trade
        /// <para></para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// EG 20130617 Passage de l'IdTSource (IDT_B = pIdTSource dans TRADELINK quand IDT_A = pIdT)
        public static void DeleteTrade(IDbTransaction pDbTransaction, int pIdT, int pIdTSource)
        {
            string cs = pDbTransaction.Connection.ConnectionString;

            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(cs, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(cs, "IDTSOURCE", DbType.Int32), pIdTSource);

            string sqlDelete = GetSqlDeleteTrade(cs);
            DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlDelete, parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// <para>Suppression des évènements du trade {pIdT}</para>
        /// <para>Suppression ou annulation des évènements de clôture/compensation sur les autres trades</para>
        /// <para>Suppression des actions sur positions</para>
        /// <para>Mise en place d'une date d'obsolescence sur les éditions/confirmations</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="gProduct"></param>
        /// <param name="pIdA">Utilisateur</param>
        /// <param name="pDateSys">date systeme</param>
        /// EG 20150115 [20683] Add pIsTradeAdmin parameter, DTOBSOLETE DataParameter, 
        /// FI 20160524 [XXXXX] Modify signature de méthode (add gProduct parameter)
        /// FI 20160816 [22146] Modify
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        // EG 20201016 [XXXXX] Test Existence EAR avant construction des query de DELETE
        // EG 20201016 [XXXXX] Ajout Delete EVENTDET
        public static void DeleteEvent(string pCS, IDbTransaction pDbTransaction, int pIdT, string pGProduct, int pIdA, DateTime pDateSys)
        {
            string sqlWhere = @" where (IDT = @IDT)";
            StrBuilder sqlQuery = new StrBuilder();

            switch (pGProduct)
            {
                case Cst.ProductGProduct_OTC:
                case Cst.ProductGProduct_FUT:
                case Cst.ProductGProduct_FX:
                case Cst.ProductGProduct_SEC:
                case Cst.ProductGProduct_COM:

                    //Delete EAR
                    //sqlQuery += @"delete from dbo.EAR" + sqlWhere + ";" + Cst.CrLf;
                    sqlQuery += GetSqlDeleteEAR(pCS, pDbTransaction, pIdT, pGProduct);

                    //Update MCO 
                    sqlQuery += GetSqlUpdateMCOForObsolete(pCS, pDateSys, pGProduct) + Cst.CrLf;

                    //Delete EVENT 
                    //Delete des éventuels EVENT sur d'autres trades que le trade concerné, EVENT nés d'une action sur le trade concerné, ou en rapport avec le trade concerné.
                    //Delete uniquement s'il n'existe aucun évènement déjà impliqué dans un EARs
                    //s'il existe déjà des évènements déjà impliqués dans un EARs, les évènements liés à l'action son alors annulés
                    DataRow[] rowToRemove = GetOFSEventVsIdT_WithEAR(pCS, pDbTransaction, pIdT);
                    int[] idEventToRemove = null;
                    if (ArrFunc.IsFilled(idEventToRemove))
                        idEventToRemove = (from DataRow item in rowToRemove select Convert.ToInt32(item["IDE"])).ToArray();

                    string sqlQueryDelEventVsIdT = @"delete from dbo.EVENT
                    where IDT!=@IDT and IDE in 
                    (
                        select epad.IDE
                        from dbo.EVENTPOSACTIONDET epad
                        inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and ((pad.IDT_BUY = @IDT) or (pad.IDT_SELL = @IDT))
                    )" + Cst.CrLf;

                    if (ArrFunc.IsFilled(idEventToRemove))
                        sqlQueryDelEventVsIdT += StrFunc.AppendFormat("and {0}", DataHelper.SQLColumnIn(pCS, "IDE", idEventToRemove, TypeData.TypeDataEnum.integer, true));

                    sqlQuery += sqlQueryDelEventVsIdT + ";" + Cst.CrLf;

                    //Remove EVENT 
                    if (ArrFunc.IsFilled(rowToRemove))
                    {
                        DateTime dtBusiness = Convert.ToDateTime(rowToRemove[0]["DTBUSINESS"]);
                        DateTime dtEventForced = OTCmlHelper.GetAnticipatedDate(pCS, pDbTransaction, dtBusiness);
                        //
                        sqlQuery += StrFunc.AppendFormat(@"Update dbo.EVENT set IDASTACTIVATION={0}, DTSTACTIVATION={1}, IDSTACTIVATION='{2}' where {3};" + Cst.CrLf,
                        pIdA.ToString(), DataHelper.SQLToDate(pCS, dtBusiness), Cst.StatusActivation.DEACTIV, DataHelper.SQLColumnIn(pCS, "IDE", idEventToRemove, TypeData.TypeDataEnum.integer));

                        sqlQuery += StrFunc.AppendFormat(@"insert dbo.EVENTCLASS(IDE,EVENTCLASS, DTEVENT, DTEVENTFORCED, ISPAYMENT)
                        select e.IDE,'{0}',{1},{2},{3} from dbo.EVENT e where {4};" + Cst.CrLf,
                        EventClassFunc.RemoveEvent.ToString(), DataHelper.SQLToDate(pCS, dtBusiness), DataHelper.SQLToDate(pCS, dtEventForced), "0", DataHelper.SQLColumnIn(pCS, "IDE", idEventToRemove, TypeData.TypeDataEnum.integer));
                    }

                    //Delete des éventuels POSACTION du trade concerné, lorsque ceux-ci ne concerne aucun autre trade que le trade concerné.
                    //Exemple  pour un même POSACTION de type UPDENTRY, il peut y avoir plusieurs couples de trade
                    //Spheres® supprime l'enregistrement POSACTION si seul un couple est concerné et que ce couple contient le trade @IDT
                    //RD 20151210 [21632] Use "and" instead "or" in second select from POSACTIONDET
                    // EG 20170425 [23064] Replace not in by not exists
                    sqlQuery += @"delete from dbo.POSACTION
                    where IDPA in (select pad.IDPA from dbo.POSACTIONDET pad where ((pad.IDT_BUY = @IDT) or (pad.IDT_SELL = @IDT))) 
                    and not exists (select pad.IDPA from dbo.POSACTIONDET pad where ((pad.IDT_BUY <> @IDT) and (pad.IDT_SELL <> @IDT)));" + Cst.CrLf;

                    //Delete des éventuelles POSACTIONDET du trade concerné, POSACTIONDET non supprimé par le DELETE CASCADE généré par le delete précédent.
                    sqlQuery += @"delete from dbo.POSACTIONDET where ((IDT_BUY=@IDT) or (IDT_SELL=@IDT));" + Cst.CrLf;

                    // EG 20151127 New 
                    //Delete POSACTION si plus d'enfants
                    sqlQuery += @"delete from dbo.POSACTION 
                    where IDPR in 
                    (
                        select pa.IDPR from dbo.POSACTION pa
                        inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
                        where (pr.IDT = @IDT) and not exists (select pad.IDPA from dbo.POSACTIONDET pad where (pad.IDPA = pa.IDPA))
                    );" + Cst.CrLf;

                    //Delete EVENTSI
                    sqlQuery += @"delete from dbo.EVENTSI where IDE in (select IDE from dbo.EVENT" + sqlWhere + ");" + Cst.CrLf;
                    break;

                case Cst.ProductGProduct_RISK:
                    // PM 20190716 [24786][23991] Ajout delete EARCOMON
                    //sqlQuery += @"delete from dbo.EARCOMMON where IDEAR in (select IDEAR from dbo.EAR" + sqlWhere + ");" + Cst.CrLf;
                    //sqlQuery += @"delete from dbo.EARCOMMON where IDEARDAY in (select IDEARDAY from dbo.EARDAY ed inner join dbo.EAR e on e.IDEAR=ed.IDEAR" + sqlWhere + ");" + Cst.CrLf;
                    //sqlQuery += @"delete from dbo.EAR" + sqlWhere + ";" + Cst.CrLf;
                    sqlQuery += GetSqlDeleteEAR(pCS, pDbTransaction, pIdT, pGProduct);
                    sqlQuery += GetSqlUpdateMCOForObsolete(pCS, pDateSys, pGProduct) + Cst.CrLf;
                    break;

                case Cst.ProductGProduct_ADM:
                    //sqlQuery += @"delete from dbo.EAR" + sqlWhere + ";" + Cst.CrLf;
                    sqlQuery += GetSqlDeleteEAR(pCS, pDbTransaction, pIdT, pGProduct);
                    sqlQuery += @"delete from dbo.MCO " + sqlWhere + ";" + Cst.CrLf;
                    sqlQuery += @"delete from dbo.EVENTSI where IDE in (select IDE from dbo.EVENT" + sqlWhere + ");" + Cst.CrLf;
                    break;

                case Cst.ProductGProduct_ASSET:
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("GetSqlDeleteEvent => {0} is not implemented", pGProduct));
            }
            sqlQuery += @"delete from dbo.EVENTDET where IDE in (select IDE from dbo.EVENT " + sqlWhere + ");" + Cst.CrLf;
            sqlQuery += @"delete from dbo.EVENT" + sqlWhere + ";" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Suppression des événements 'OPP' du trade {pIdT} (enfants de TRD/DAT)
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// FI 20160907 [21831] Add
        /// FI 20160907 [22455] Modify
        /// FI 20170306 [22225] Modify  (call DeleteFeeEvent)
        /// FI 20180328 [23871] Modify
        public static void DeleteFeeEvent(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            DeleteFeeEvent(pCS, pDbTransaction, pIdT, true, null, null, true);
        }

        /// <summary>
        /// Suppression des événements 'OPP' du trade {pIdT} (enfants de TRD/DAT)
        /// <para>Possibilité de selectionner un barème en particulier</para>
        /// <para>Possibilité de selectionner un condition en particulier</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIsDelAuto">si true suppression des frais issus des barèmes et conditions</param>
        /// <param name="pIdFeeSchedule">permet de spécifier un barème</param>
        /// <param name="pIdFeeMatrix">permet de spécifier une condition</param>
        /// <param name="pIsDelManual">si true suppression des frais saisis manuellement</param>
        /// FI 20170306 [22225] New
        /// EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        /// FI 20180328 [23871] Modify
        /// PL 20221006 [XXXXX] Optimization / Remove union all (see selectAllFee)
        public static void DeleteFeeEvent(string pCS, IDbTransaction pDbTransaction, int pIdT, Boolean pIsDelAutoFee, Nullable<Int32> pIdFeeSchedule, 
                                          Nullable<Int32> pIdFeeMatrix, Boolean pIsDelManualFee)
        {
            if (pIsDelAutoFee || pIsDelManualFee)
            {
                StrBuilder selectBuilder = new StrBuilder(); 
                
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);

                string selectTemplate = @"select e.IDE 
from dbo.EVENT e 
inner join dbo.EVENT ep on (ep.IDE = e.IDE_EVENT) and (ep.EVENTCODE='TRD' and ep.EVENTTYPE='DAT')
inner join dbo.EVENTFEE evFee on (evFee.IDE = e.IDE) {0}
where (e.IDT = @IDT)";

                //PL 20221006 query optimized for all fees without restriction
                if (pIsDelManualFee && pIsDelAutoFee && (!pIdFeeSchedule.HasValue) && (!pIdFeeMatrix.HasValue))
                {
                    string selectAllFee = StrFunc.AppendFormat(selectTemplate, string.Empty);
                    selectBuilder.Append(selectAllFee);
                }
                else
                {
                    if (pIsDelAutoFee)
                    {
                        //Usage of STATUS is NOT null for identifying Automatic Fee issuing from Schedule
                        string selectAutoFee = StrFunc.AppendFormat(selectTemplate, "and (evFee.STATUS is not null)"); 
                        selectBuilder.Append(selectAutoFee);
                        
                        if (pIdFeeSchedule.HasValue)
                        {
                            selectBuilder.Append(" and (evFee.IDFEESCHEDULE = @IDFEESCHEDULE)");
                            parameters.Add(new DataParameter(pCS, "IDFEESCHEDULE", DbType.Int32), pIdFeeSchedule.Value);
                        }
                        if (pIdFeeMatrix.HasValue)
                        {
                            selectBuilder.Append(" and (evFee.IDFEEMATRIX = @IDFEEMATRIX)");
                            parameters.Add(new DataParameter(pCS, "IDFEEMATRIX", DbType.Int32), pIdFeeMatrix.Value);
                        }
                    }

                    if (pIsDelManualFee)
                    {
                        if (pIsDelAutoFee)
                            selectBuilder.Append(Cst.CrLf + "union all" + Cst.CrLf);

                        //Usage of STATUS is null for identifying Manual Fee 
                        string selectManualFee = StrFunc.AppendFormat(selectTemplate, "and (evFee.STATUS is null)");
                        selectBuilder.Append(selectManualFee);
                    }
                }

                DeleteFeeEventExecute(pCS, pDbTransaction, selectBuilder.ToString(), parameters);
            }
        }


        /// <summary>
        /// Suppression des événement de frais non facturés 
        /// CaptureMode =  ModeEnum.UpdateFeesUninvoiced
        /// Tous les frais seront supprimées à l'exception de ceux déjà facturés.
        /// </summary>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public static void DeleteFeeEventUninvoiced(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            dp.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.OtherPartyPayment);
            dp.Add(new DataParameter(pCS, "EVENTCLASS", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventClassFunc.Invoiced);
            string sqlSelect = @"delete from dbo.EVENT 
            where (IDT = @IDT) and (EVENTCODE = @EVENTCODE) and (IDE not in
            (
                select ev.IDE
                from dbo.EVENT ev
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
                inner join dbo.EVENT evinv on (evinv.IDE_SOURCE = ev.IDE)
                where (ev.IDT = @IDT) and (ev.EVENTCODE = @EVENTCODE) and (ec.EVENTCLASS = @EVENTCLASS)
            ))";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, dp);
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }
        /// <summary>
        ///  Suppression d'un évènement de frais lorsque l'IDE est connu
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdE"></param>
        public static void DeleteFeeEventId(string pCS, IDbTransaction pDbTransaction, int pIdT, int pIdE)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "IDE", DbType.Int32), pIdE);

            string select = @"select e.IDE 
from dbo.EVENT e 
inner join dbo.EVENT ep on ep.IDE = e.IDE_EVENT and ep.EVENTCODE='TRD' and ep.EVENTTYPE='DAT'
inner join dbo.EVENTFEE evFee on evFee.IDE = e.IDE 
where e.IDT=@IDT and e.IDE=@IDE";

            DeleteFeeEventExecute(pCS, pDbTransaction, select, parameters);

        }

        /// <summary>
        /// Suppression des événements 'OPP' automatique du trade {pIdT} (enfants de TRD/DAT) 
        /// <para>Possibilité de selectionner un barême en particulier</para>
        /// <para>Possibilité de selectionner un condition en particulier</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdFeeSchedule">permet de spécifier un barème</param>
        /// <param name="pIdFeeMatrix">permet de spécifier une condition</param>
        /// FI 20180328 [23871] Add
        public static void DeleteFeeEventAuto(string pCS, IDbTransaction pDbTransaction, int pIdT, Nullable<Int32> pIdFeeSchedule, Nullable<Int32> pIdFeeMatrix)
        {
            DeleteFeeEvent(pCS, pDbTransaction, pIdT, true, pIdFeeSchedule, pIdFeeMatrix, false);
        }

        /// <summary>
        /// Suppression des événements 'OPP' manuel du trade {pIdT} (enfants de TRD/DAT) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// FI 20180328 [23871] Add
        public static void DeleteFeeEventManual(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            DeleteFeeEvent(pCS, pDbTransaction, pIdT, false, null, null, true);
        }

        /// <summary>
        /// Execution de la requête de delete ddes évènements de frais
        /// <para>La liste des évènements de frais étant obtenus à partir de la requête {pQueryIdE}</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pQueryIdE">Requête qui retourne les EVENTs (IDE) à supprimer</param>
        /// <param name="pParameters">parameters</param>
        /// FI 20180328 [23871] Add Method
        private static void DeleteFeeEventExecute(string pCS, IDbTransaction pDbTransaction, string pQueryIdE, DataParameters pParameters)
        {
            if (false == pParameters.Contains("IDT"))
                throw new ArgumentException("pParameters doesn't constains @IDT parameter");

            // Delete des événements OPP, puis delete des événements orphelins de manière à supprimer les événements enfants des OPP (ex taxes) 
            string sqlDelete = StrFunc.AppendFormat(@"
delete from dbo.EVENT
    where (IDT = @IDT) and (EVENTCODE = 'OPP')
    and (IDE in ({0}));
delete from dbo.EVENT
    where (IDT = @IDT) and (IDE_EVENT <> 0) and (IDE_EVENT not in (select IDE from dbo.EVENT where (IDT = @IDT)));", pQueryIdE);


            QueryParameters qryParameters = new QueryParameters(pCS, sqlDelete, pParameters);
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        ///  Retourne la requête de mise à jour des colonnes DTOBSOLETE (MCO et MCODET)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDateSys"></param>
        /// <param name="gProduct"></param>
        /// <returns></returns>
        /// FI 20160816 [22146] Add 
        private static string GetSqlUpdateMCOForObsolete(string pCS, DateTime pDateSys, string gProduct)
        {
            string sqlQuery = string.Empty;

            // Update MCO / MCODET du TRADE
            sqlQuery += @"update dbo.MCO set DTOBSOLETE = @DTOBSOLETE 
            where IDMCO in
            (
                select mcod.IDMCO 
                from dbo.MCODET mcod 
                where mcod.IDE in 
                (
                    select ev.IDE 
                    from dbo.EVENT ev 
                    where (ev.IDT = @IDT)
                )
            ) and (DTOBSOLETE is null);
                
            update dbo.MCODET set DTOBSOLETE = @DTOBSOLETE 
            where IDE in 
            (
                select ev.IDE 
                from dbo.EVENT ev 
                where (ev.IDT = @IDT)
            ) and (DTOBSOLETE is null);" + Cst.CrLf;

            switch (gProduct)
            {
                case Cst.ProductGProduct_FUT:
                case Cst.ProductGProduct_FX:
                case Cst.ProductGProduct_SEC:

                    // Update MCO / MCODET des TRADES liés par POSACTIONDET
                    // cas où il existe des éditions actions sur positions 
                    sqlQuery += @"
update dbo.MCO set DTOBSOLETE = @DTOBSOLETE
where IDMCO in
(
    select IDMCO
    from dbo.MCODET mcod
    where IDE in
    (
        select epad.IDE
        from dbo.EVENTPOSACTIONDET epad
        inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and ((pad.IDT_BUY = @IDT) or (pad.IDT_SELL = @IDT))
    ) and (mcod.DTOBSOLETE is null)
);

update dbo.MCODET set DTOBSOLETE = @DTOBSOLETE 
where IDE in
(
    select epad.IDE
    from dbo.EVENTPOSACTIONDET epad
    inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and ((pad.IDT_BUY = @IDT) or (pad.IDT_SELL = @IDT))
) and (DTOBSOLETE is null);" + Cst.CrLf;
                    break;
            }

            sqlQuery = sqlQuery.Replace("@DTOBSOLETE", DataHelper.SQLToDateTime(pCS, pDateSys));

            return sqlQuery;
        }

        /// <summary>
        /// Construction des requêtes de DELETE des EAR en fonction du groupe de produits.
        /// Test existence EAR opéré en tête de procédure
        /// => ORACLE génére des DEADLOCKs sir pas d'EAR donc pas de STATS sur ces tables
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pGProduct"></param>
        /// <returns></returns>
        /// EG 20201016 [XXXXX] New
        private static string GetSqlDeleteEAR(string pCS, IDbTransaction pDbTransaction, int pIdT, string pGProduct)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);

            string sqlWhere = @" where (IDT = @IDT)";
            string sqlQuery = string.Empty;

            QueryParameters qryParameters = new QueryParameters(pCS, "Select 1 from dbo.EAR" + sqlWhere, parameters);

            if (null != DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                switch (pGProduct)
                {
                    case Cst.ProductGProduct_OTC:
                    case Cst.ProductGProduct_FUT:
                    case Cst.ProductGProduct_FX:
                    case Cst.ProductGProduct_SEC:
                    case Cst.ProductGProduct_COM:
                    case Cst.ProductGProduct_ADM:
                        sqlQuery += @"delete from dbo.EAR" + sqlWhere + ";" + Cst.CrLf;
                        break;
                    case Cst.ProductGProduct_RISK:
                        sqlQuery += @"delete from dbo.EARCOMMON where IDEAR in (select IDEAR from dbo.EAR" + sqlWhere + ");" + Cst.CrLf;
                        sqlQuery += @"delete from dbo.EARCOMMON where IDEARDAY in (select ed.IDEARDAY from dbo.EARDAY ed inner join dbo.EAR e on (e.IDEAR=ed.IDEAR)" + sqlWhere + ");" + Cst.CrLf;
                        sqlQuery += @"delete from dbo.EAR" + sqlWhere + ";" + Cst.CrLf;
                        break;
                }
            }
            return sqlQuery;
        }

        /// <summary>
        ///  Retourne la requête qui remonte les évènements de clôture/compensation sur le trade impacté suite à l'entrée du trade @IDT pour lesquels il existe déjà des EARS
        ///  <para>Les colonnes disponibles sont e.IDE, e.EVENTCODE, e.EVENTTYPE, t.IDT, t.IDENTIFIER, pa.DTBUSINESS</para>
        /// </summary>
        ///<param name="pCS"></param>
        ///<param name="pDbTransaction"></param>
        ///<param name="pIdT">Trade source</param>
        /// FI 20160816 [22146] Add 
        /// FI 20160320 [22146] Modify ( Réécrirure => Utilisation de 2 requêtes plutôt qu'1 seule complexe => Amélioration des perfs) 
        // EG 20180205 [23769] Upd DataHelper.ExecuteDataTable
        public static DataRow[] GetOFSEventVsIdT_WithEAR(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            List<DataRow> rowRet = new List<DataRow>();
            QueryParameters qry = BuildQueryOFSEventVsIdT(pCS, "e.IDE, e.EVENTCODE, e.EVENTTYPE, t.IDT, t.IDENTIFIER, pa.DTBUSINESS", pIdT);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, pDbTransaction, qry.Query, qry.Parameters.GetArrayDbParameter());

            int[] idE = null;

            if (dt.Rows.Count > 0)
            {
                List<DataRow> rowsOfs = dt.Rows.Cast<DataRow>().ToList();
                idE = (from item in rowsOfs
                       select Convert.ToInt32(item["IDE"])).ToArray();

                string queryEar = StrFunc.AppendFormat("select  ear.IDE from dbo.VW_EAREVENT ear where {0}", DataHelper.SQLColumnIn(pCS, "IDE", idE, TypeData.TypeDataEnum.@int));
                DataTable dtEar = DataHelper.ExecuteDataTable(pCS, pDbTransaction, queryEar);

                if (dtEar.Rows.Count > 0)
                {
                    List<DataRow> rowsEar = dtEar.Rows.Cast<DataRow>().ToList();
                    foreach (DataRow row in rowsOfs)
                    {
                        if (ArrFunc.IsFilled(dt.Select(StrFunc.AppendFormat("IDE = {0}", row["IDE"].ToString()))))
                            rowRet.Add(row);
                    }
                }
            }

            DataRow[] ret = null;
            if (rowRet.Count > 0)
                ret = rowRet.ToArray();

            return ret;
        }

        /// <summary>
        /// Retourne la requête qui remonte les évènements de clôture/compensation sur le trade impacté suite à l'entrée du trade {pIdT}
        /// </summary>
        /// <param name="pExpressionColumn">Liste des colonnes attendues dans le jeu de résultar</param>
        /// <returns></returns>
        /// FI 20160816 [22146] Add 
        /// FI 20160320 [22146] Modify de signature (=> Retourne un QueryParameters)
        private static QueryParameters BuildQueryOFSEventVsIdT(string pCS, string pExpressionColumn, int pIdT)
        {

            string queryEventOFS = @"select {0}
from dbo.EVENTPOSACTIONDET epad
inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and ((pad.IDT_BUY = @IDT) or (pad.IDT_SELL = @IDT))
inner join dbo.POSACTION pa on pa.IDPA = pad.IDPA  
inner join dbo.POSREQUEST pr on pr.IDPR = pa.IDPR  and (pr.REQUESTTYPE in ('UNCLEARING','CLEARSPEC','CLEARBULK','CLEAREOD','ENTRY','UPDENTRY'))
inner join dbo.EVENT e on e.IDE = epad.IDE
inner join dbo.TRADE t on t.IDT = e.IDT
where e.IDT != @IDT and e.IDSTACTIVATION = 'REGULAR'";

            queryEventOFS = StrFunc.AppendFormat(queryEventOFS, pExpressionColumn);


            DataParameters parameters = new DataParameters(new DataParameter[] { });
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);

            QueryParameters ret = new QueryParameters(pCS, queryEventOFS.ToString(), parameters);
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        public static void DeletePosRequest(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            string sqlDelete = GetSqlDeletePosRequest();

            DataParameter paramIdT = new DataParameter(pCS, "IDT", DbType.Int32)
            {
                Value = pIdT
            };

            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlDelete, paramIdT.DbDataParameter);
        }

        /// <summary>
        /// Constitution de l'Identifier d'un Trade
        /// <para>Le format est fonction du paramétrage en vigueur sur l'instrument concerné.</para>
        /// <para>NB: Lorsque pDbTransaction est null, seul les paramètres opPrefix et opSuffix sont valorisés.</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pSQLInstrument"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pTradeStatus"></param>
        /// <param name="pTradeDate"></param>
        /// <param name="pBusinessDate"></param>
        /// <param name="opIdentifier"></param>
        /// <param name="opPrefix"></param>
        /// <param name="opSuffix"></param>
        /// <returns></returns>
        // EG 20180606 [23980] Set missing dbTransaction (parallelism)
        // EG 20210420 [25723]/[24942] Suppression du else pour réel maintien de la compatibilité ascendante
        // EG 20210421 [25723] Refactoring complet (découpage en micro-méthodes)
        public static Cst.ErrLevel BuildTradeIdentifier(string pCS, IDbTransaction pDbTransaction,
            SQL_Instrument pSQLInstrument, int pIdAEntity, TradeStatus pTradeStatus, DateTime pTradeDate, DateTime pBusinessDate,
            out string opIdentifier, out string opPrefix, out string opSuffix)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string prefix = string.Empty;
            string suffix = string.Empty;
            // Lecture des préfixe/Suffixe sur l'instrument (IDSTENVIRONMENT/IDSTBUSINESS)
            Pair<string, string> sourcePrefixSuffix = GetPrefixSuffixForStatus(pSQLInstrument, pTradeStatus, prefix, suffix);

            bool isExistPrefixOrSuffix = StrFunc.IsFilled(sourcePrefixSuffix.First) || StrFunc.IsFilled(sourcePrefixSuffix.Second);
            if (isExistPrefixOrSuffix)
            {
                // EG 20210421 Remplacement des constantes présentes dans préfixe et suffixe source
                Pair<string, string> finalPrefixSuffix = ReplaceVariableOnPrefixSuffix(pCS, pDbTransaction, sourcePrefixSuffix.First, sourcePrefixSuffix.Second, pIdAEntity, pTradeDate, pBusinessDate, null, DateTime.MinValue, null);
                prefix = finalPrefixSuffix.First;
                suffix = finalPrefixSuffix.Second;
            }

            // Calcul de l'identifiant (avant Préfixe/Suffixe)
            opIdentifier = GetNewIdentifier(pCS, pDbTransaction, pSQLInstrument, pIdAEntity, pTradeStatus, pTradeDate, pBusinessDate);
            opPrefix = prefix;
            opSuffix = suffix;

            return ret;
        }

        /// <summary>
        /// Recherche des préfixe/Suffixe à appliquer en fonction de l'instrument (Statut d'environment, Statut Business)
        /// </summary>
        /// <param name="pSQLInstrument">Instrument en vigueur du trade</param>
        /// <param name="pTradeStatus">Statuts en vigueur sur le trade</param>
        /// <param name="pPrefix">Préfixe en entrée</param>
        /// <param name="pSuffix">Suffixe en entrée</param>
        /// <returns></returns>
        public static Pair<string, string> GetPrefixSuffixForStatus(SQL_Instrument pSQLInstrument, TradeStatus pTradeStatus, string pPrefix, string pSuffix)
        {
            string prefix = pPrefix;
            string suffix = pSuffix;

            if ((pTradeStatus == null) || pTradeStatus.IsStEnvironment_Regular)
            {
                prefix += pSQLInstrument.TradeIdPrefixReg;
                suffix += pSQLInstrument.TradeIdSuffixReg;
            }
            else if (pTradeStatus.IsStEnvironment_Simul)
            {
                prefix += pSQLInstrument.TradeIdPrefixSim;
                suffix += pSQLInstrument.TradeIdSuffixSim;
            }

            if ((pTradeStatus == null) || pTradeStatus.IsStBusiness_Executed)
            {
                prefix += pSQLInstrument.TradeIdPrefixExe;
                suffix += pSQLInstrument.TradeIdSuffixExe;
            }
            else if (pTradeStatus.IsStBusiness_Intermed)
            {
                prefix += pSQLInstrument.TradeIdPrefixInt;
                suffix += pSQLInstrument.TradeIdSuffixInt;
            }
            else if (pTradeStatus.IsStBusiness_Alloc)
            {
                prefix += pSQLInstrument.TradeIdPrefixAll;
                suffix += pSQLInstrument.TradeIdSuffixAll;
            }
            else if (pTradeStatus.IsStBusiness_PreTrade)
            {
                prefix += pSQLInstrument.TradeIdPrefixPre;
                suffix += pSQLInstrument.TradeIdSuffixPre;
            }

            Pair<string, string> ret = new Pair<string, string>(prefix, suffix);
            return ret;

        }

        /// <summary>
        /// Remplacement des variable déclarées dans le prefixe et suffixe associé à la génération de l'identifiant d'un trade
        /// par leurs valeurs respectives
        /// </summary>
        /// <param name="pCS">chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pPrefix">Prefixe source</param>
        /// <param name="pSuffix">Suffixe source</param>
        /// <param name="pIdAEntity">Id de l'entité</param>
        /// <param name="pTradeDate">Date de trade</param>
        /// <param name="pBusinessDate">Date business</param>
        /// <param name="pCorpoActionDate">Date de CA</param>
        /// <param name="pCRPPosition">Mode CRP (Close/Open)</param>
        /// <returns>Pair (First = Prefixe transformé et Second = Suffixe transformé)</returns>
        /// EG 20210421 [25723] New 
        public static Pair<string, string> ReplaceVariableOnPrefixSuffix(string pCS, IDbTransaction pDbTransaction,string pPrefix, string pSuffix,
            int pIdAEntity, DateTime pTradeDate, DateTime pBusinessDate, string pOriginalIdentifier, DateTime pCorpoActionDate, string pCRPPosition)
        {
            string prefix = pPrefix;
            string suffix = pSuffix;
            // FI 20190920 [24942] s'il existe des {} => Nouveau mode de fonctionnement
            Regex regEx = new Regex(@"{\w+}", RegexOptions.IgnoreCase);
            if (regEx.IsMatch(prefix) || regEx.IsMatch(suffix))
            {
                // FI 20200424 [XXXXX] cacheOn
                SQL_Entity sql_Entity = new SQL_Entity(CSTools.SetCacheOn(pCS), pIdAEntity)
                {
                    DbTransaction = pDbTransaction
                };
                sql_Entity.LoadTable(new string[] { "ENTITY.EXTLLINK", "a.EXTLLINK as ACTOR_EXTLLINK" });
                var context = new
                {
                    DTBUSINESS = pBusinessDate,
                    DTBUSINESS_DayOfYear = pBusinessDate.DayOfYear,
                    DTEXEC = pTradeDate,
                    DTEXEC_DayOfYear = pTradeDate.DayOfYear,

                    ENTITY_ActorIdentifier = sql_Entity.Identifier,
                    ENTITY_ActorExtlLink = sql_Entity.GetFirstRowColumnValue("ACTOR_EXTLLINK"),
                    ENTITY_ActorBic = sql_Entity.BIC,
                    ENTITY_ExtlLink = sql_Entity.ExtlLink,
                    // EG 20210421 Ajout de la gestion de la constante {CORPOACTION_Date} et {CRP_Position}
                    CORPOACTION_Date = pCorpoActionDate,
                    CRP_Position = pCRPPosition,
                    ORIGINAL_Identifier = pOriginalIdentifier,
                };


                if (regEx.IsMatch(prefix))
                    prefix = StrFuncExtended.ReplaceObjectField(prefix, context);

                if (regEx.IsMatch(suffix))
                    suffix = StrFuncExtended.ReplaceObjectField(suffix, context);
            }

            // FI 20190920 [24942] Code conservé pour compatibilité ascendante
            // EG 20210420 [25723]/[24942] Suppression du ELSE pour réel maintien de la compatibilité ascendante
            if ((prefix.IndexOf("_ENTITY%%") > 0) || (suffix.IndexOf("_ENTITY%%") > 0))
            {
                if (pIdAEntity > 0)
                {
                    // FI 20200424 [XXXXX] cacheOn
                    SQL_Actor sql_ActorEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), pIdAEntity); ;
                    sql_ActorEntity.DbTransaction = pDbTransaction;
                    // FI 20200424 [XXXXX] cacheOn
                    SQL_Entity sql_Entity = new SQL_Entity(CSTools.SetCacheOn(pCS), pIdAEntity)
                    {
                        DbTransaction = pDbTransaction
                    };

                    prefix = ReplaceDynamicConstantsWithEntityInfo(prefix, sql_ActorEntity, sql_Entity);
                    suffix = ReplaceDynamicConstantsWithEntityInfo(suffix, sql_ActorEntity, sql_Entity);
                }
            }
            // EG 20210421 Ajout de la gestion de la constante %%CORPOACTIONDATE%%
            if ((prefix.IndexOf(Cst.BUSINESSDATE.Substring(0, Cst.BUSINESSDATE.Length - 2)) >= 0)
                || (prefix.IndexOf(Cst.TRANSACTDATE.Substring(0, Cst.TRANSACTDATE.Length - 2)) >= 0)
                || (prefix.IndexOf(Cst.CORPOACTIONDATE.Substring(0, Cst.CORPOACTIONDATE.Length - 2)) >= 0)
                || (suffix.IndexOf(Cst.BUSINESSDATE.Substring(0, Cst.BUSINESSDATE.Length - 2)) >= 0)
                || (suffix.IndexOf(Cst.TRANSACTDATE.Substring(0, Cst.TRANSACTDATE.Length - 2)) >= 0)
                || (suffix.IndexOf(Cst.CORPOACTIONDATE.Substring(0, Cst.CORPOACTIONDATE.Length - 2)) >= 0))
            {
                prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(prefix, Cst.TRANSACTDATE, pTradeDate);
                prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(prefix, Cst.BUSINESSDATE, pBusinessDate);
                prefix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(prefix, Cst.CORPOACTIONDATE, pCorpoActionDate);

                suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(suffix, Cst.TRANSACTDATE, pTradeDate);
                suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(suffix, Cst.BUSINESSDATE, pBusinessDate);
                suffix = TradeRDBMSTools.ReplaceDynamicConstantsWithdateInfo(suffix, Cst.CORPOACTIONDATE, pCorpoActionDate);
            }

            // EG 20210421 Ajout de la gestion de la constante %%CLOSINGREOPENING%%
            if (StrFunc.IsFilled(pCRPPosition) &&
                (prefix.IndexOf(Cst.CLOSINGREOPENING.Substring(0, Cst.CLOSINGREOPENING.Length - 2)) >= 0) || 
                 suffix.IndexOf(Cst.CLOSINGREOPENING.Substring(0, Cst.CLOSINGREOPENING.Length - 2)) >= 0)
            {
                prefix = prefix.Replace(Cst.CLOSINGREOPENING, pCRPPosition);
                suffix = suffix.Replace(Cst.CLOSINGREOPENING, pCRPPosition);
            }

            Pair<string, string> ret = new Pair<string, string>(prefix, suffix);
            return ret;
        }


        /// <summary>
        /// Calcul d'un nouvel identifiant via Get_ID
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pSQLInstrument">Instrument en vigueur</param>
        /// <param name="pIdAEntity">Id de l'entité</param>
        /// <param name="pTradeStatus">Status du trade</param>
        /// <param name="pTradeDate">Trade date</param>
        /// <param name="pBusinessDate">Business date</param>
        /// <param name="opIdentifier">Nouvel identifiant : valeur de retour</param>
        /// EG 20210421 [25723] New 
        public static string GetNewIdentifier(string pCS, IDbTransaction pDbTransaction, SQL_Instrument pSQLInstrument, int pIdAEntity, TradeStatus pTradeStatus, DateTime pTradeDate, DateTime pBusinessDate)
        {
            string idGetId = SQLUP.IdGetId.TRADE.ToString();

            // Par environnement
            if (pSQLInstrument.IsTradeIdByStEnv)
                idGetId += ":" + (pTradeStatus == null ? Cst.StatusEnvironment.REGULAR.ToString() : pTradeStatus.stEnvironment.NewSt);

            // Par status
            if (pSQLInstrument.IsTradeIdByStBus)
                idGetId += ":" + (pTradeStatus == null ? Cst.StatusBusiness.EXECUTED.ToString() : pTradeStatus.stBusiness.NewSt);

            // Par format
            // A partir de la v3.2: GPPRODUCT --> GP, PRODUCT --> P, INSTRUMENT --> I, ENTITY --> E
            if (pSQLInstrument.TradeIdFormat.ToUpper() == "GPPRODUCT")
                idGetId += ":GP:" + pSQLInstrument.GProduct;
            else if (pSQLInstrument.TradeIdFormat.ToUpper() == "PRODUCT")
                idGetId += ":P:" + pSQLInstrument.IdP.ToString();
            else if (pSQLInstrument.TradeIdFormat.ToUpper() == "INSTRUMENT")
                idGetId += ":I:" + pSQLInstrument.IdI.ToString();

            // Par entité
            if (pSQLInstrument.IsTradeIdByEntity && (0 < pIdAEntity))
                idGetId += ":E:" + pIdAEntity.ToString();

            // Par dates
            if (pSQLInstrument.IsTradeIdByDtTrade)
                idGetId += ":TD:" + DtFunc.DateTimeToStringyyyyMMdd(pTradeDate);

            if (pSQLInstrument.IsTradeIdByDtBusiness)
            {
                if (DtFunc.IsDateTimeEmpty(pBusinessDate))
                    pBusinessDate = pTradeDate;

                idGetId += ":BD:" + DtFunc.DateTimeToStringyyyyMMdd(pBusinessDate);
            }

            // Evaluation du nouvel identifiant en fonction de idGetId
            int newToken = 0;
            if (pDbTransaction != null)
            {
                SQLUP.GetId(out newToken, pDbTransaction, idGetId);
            }
            string newIdentifier = newToken.ToString();

            #region TradeIdNumberLength
            if (pSQLInstrument.TradeIdNumberLength > 0)
            {
                int maxlen = pSQLInstrument.TradeIdNumberLength;
                if (newIdentifier.Length > maxlen)
                {
                    string overflow = newIdentifier.Substring(0, newIdentifier.Length - maxlen + 1);
                    if (IntFunc.IntValue(overflow) <= 35)
                    {
                        #region Note
                        // 65 is the ascii code of "A" character.
                        // Examples: 
                        // - Maxlen = 3, Identifier = 1056, Result ==> A56
                        // - Maxlen = 3, Identifier = 1456, Result ==> E56
                        // - Maxlen = 3, Identifier = 3556, Result ==> Z56
                        #endregion
                        newIdentifier = char.ConvertFromUtf32(65 + IntFunc.IntValue(overflow) - 10) + newIdentifier.Substring(newIdentifier.Length - maxlen + 1);
                    }
                    else
                    {
                        #region Select ID from E_GETID
                        // FI 20200424 [XXXXX] cacheOn
                        if (DataHelper.IsExistTable(CSTools.SetCacheOn(pCS), "E_GETID"))
                        {
                            string sqlSelect = SQLCst.SELECT + "ID" + Cst.CrLf;  //Identifier
                            sqlSelect += SQLCst.FROM_DBO + "E_GETID" + Cst.CrLf; //External GETID
                            sqlSelect += SQLCst.WHERE + "(@IDGETID like IDGETID || '%') and (TOKEN=@TOKEN)";

                            DataParameters parameters = new DataParameters();
                            parameters.Add(new DataParameter(pCS, "IDGETID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), idGetId);
                            parameters.Add(new DataParameter(pCS, "TOKEN", DbType.Int32), newToken);

                            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect, parameters);
                            // FI 20200424 [XXXXX] SetCacheOff => ou cas ou la méthode serait appelé avec cache true, il convient ici de ne pas en tenir compte
                            object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOff(pCS), pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if ((null != obj) && (!Convert.IsDBNull(obj)))
                            {
                                newIdentifier = Convert.ToString(obj);
                            }
                            else
                            {
                                throw new Exception(StrFunc.AppendFormat("Error when getting of the identifier!"
                                    + Cst.CrLf + "Max length : {0}"
                                    + Cst.CrLf + "Token value: {1}"
                                    + Cst.CrLf + "Key value  : {2}",
                                    maxlen.ToString(), newIdentifier, idGetId));
                            }
                        }
                        else
                        {
                            throw new Exception(StrFunc.AppendFormat("Error when computing of the identifier!"
                                + Cst.CrLf + "Max length : {0}"
                                + Cst.CrLf + "Token value: {1}",
                                maxlen.ToString(), newIdentifier));
                        }
                        #endregion
                    }
                }
                else if (newIdentifier.Length < maxlen)
                {
                    newIdentifier = newIdentifier.PadLeft(maxlen, '0');
                }
            }
            #endregion TradeIdNumberLength

            return newIdentifier;
        }
        /// <summary>
        /// Retourne l'identifier d'un trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        public static string GetTradeIdentifier(string pCS, int pIdT)
        {
            return GetTradeIdentifier(pCS, null, pIdT);
        }
        /// <summary>
        /// Retourne l'identifier d'un trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataHelper.DataHelper.ExecuteScalar
        public static string GetTradeIdentifier(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            string ret = string.Empty;
            DataParameter param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT);
            param.Value = pIdT;
            //
            string sqlSelect = SQLCst.SELECT + "IDENTIFIER" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDT=@IDT";

            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlSelect, param.DbDataParameter);
            if (null != obj)
                ret = obj.ToString();

            return ret;
        }

        /// <summary>
        /// Retourne l'identification (Identifier, DisplayName, etc...)  d'un trade 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// FI 20130117[] Correction Bug Enorme
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public static SpheresIdentification GetTradeIdentification(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            SQL_TradeCommon sqlTradeCommon = new SQL_TradeCommon(pCS, pIdT)
            {
                DbTransaction = pDbTransaction
            };
            sqlTradeCommon.LoadTable(new string[] { "TRADE.IDT,IDENTIFIER,DISPLAYNAME,DESCRIPTION,EXTLLINK" });
            SpheresIdentification ret = new SpheresIdentification
            {
                OTCmlId = sqlTradeCommon.Id,
                Identifier = sqlTradeCommon.Identifier,
                Displayname = sqlTradeCommon.DisplayName,
                Description = sqlTradeCommon.Description,
                Extllink = sqlTradeCommon.ExtlLink
            };
            return ret;
        }

        /// <summary>
        /// Retourne l'IdT du trade dont l'identifier vaut {pIdentifier}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetTradeIdT(string pCS, string pIdentifier)
        {
            return GetTradeIdT(pCS, null, pIdentifier);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetTradeIdT(string pCS, IDbTransaction pDbTransaction, string pIdentifier)
        {
            DataParameter param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDENTIFIER);
            param.Value = pIdentifier;

            int idT = 0;

            string sqlSelect = SQLCst.SELECT + "IDT" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDENTIFIER=@IDENTIFIER";

            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlSelect, param.DbDataParameter);
            if (null != obj)
                idT = Convert.ToInt32(obj);
            return idT;
        }

        /// <summary>
        /// Retourne true s'il existe au moins un évènement qui a subi un des traitements définis sous {pProcessType} avec succès
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProcessType"></param>
        /// <returns></returns>
        public static bool ExistEventProcess(string pCs, int pIdT, Cst.ProcessTypeEnum[] pProcessType)
        {
            return ExistEventProcess(pCs, pIdT, pProcessType, null, ProcessStateTools.StatusSuccessEnum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pProcessTypeExclude"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static bool ExistEventProcess(string pCs, int pIdT, Cst.ProcessTypeEnum[] pProcessType, Cst.ProcessTypeEnum[] pProcessTypeExclude, ProcessStateTools.StatusEnum pStatus)
        {
            return ArrFunc.IsFilled(GetListEventProcess(pCs, pIdT, pProcessType, pProcessTypeExclude, pStatus));
        }

        /// <summary>
        /// Retourne true si au moins un événement a été généré pour ce Trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        public static bool IsEventExist(string pCS, int pIdT)
        {
            return IsEventExist(pCS, pIdT, string.Empty);
        }

        /// <summary>
        /// Retourne true si au moins un événement avec un EventCode = {pEventCode} a été généré pour ce Trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsEventExist(string pCS, int pIdT, string pEventCode)
        {
            return IsEventExist(pCS, null, pIdT, pEventCode);
        }
        /// <summary>
        /// Retourne true si au moins un événement avec un EventCode = {pEventCode} a été généré pour ce Trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        /// EG 20180205 [23769] Add dbTransaction  
        public static bool IsEventExist(string pCS, IDbTransaction pDbTransaction, int pIdT, string pEventCode)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);

            if (StrFunc.IsFilled(pEventCode))
                paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCODE), pEventCode);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "1" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e" + Cst.CrLf;
            sql += SQLCst.WHERE + "e.IDT =@IDT" + Cst.CrLf;

            if (StrFunc.IsFilled(pEventCode))
                sql += SQLCst.AND + "e.EVENTCODE =@EVENTCODE" + Cst.CrLf;

            //20070717 FI utilisation de ExecuteScalar [pour un eventuel stockage dans le cache]
            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
           return (null != obj);
        }

        /// <summary>
        /// Retourne true si au moins un événement avec un EventCode = {pEventCode} et EVENTTYPE = {pEventType} a été généré pour ce Trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        // EG 20180514 [23812] Report
        public static bool IsEventExist(string pCS, int pIdT, string pEventCode, string pEventType)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCODE), pEventCode);
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTTYPE), pEventType);
            string sql = @"select 1 from dbo.EVENT e where e.IDT=@IDT and e.EVENTCODE=@EVENTCODE and e.EVENTTYPE=@EVENTTYPE";

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sql, paramameters.GetArrayDbParameter());
            return (null != obj);
        }


        /// <summary>
        /// Retourne true si au moins un événement avec un EventCode = {pEventCode} a été généré pour ce Trade
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventCodeParent"></param>
        /// <returns></returns>
        // EG 20180514 [23812] Report
        public static bool IsEventExist_Parent(string pCS, int pIdT, string pEventCode, string pEventCodeParent)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCODE), pEventCode);
            paramameters.Add(new DataParameter(pCS, "EVENTCODEPARENT", DbType.AnsiString, SQLCst.UT_EVENT_LEN), pEventCodeParent);

            string sql = @"select 1 from dbo.EVENT e 
            inner join dbo.EVENT ep on ep.IDE=e.IDE_EVENT and ep.EVENTCODE=@EVENTCODEPARENT
            where e.IDT=@IDT and e.EVENTCODE=@EVENTCODE ";

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sql, paramameters.GetArrayDbParameter());
            return (null != obj);
        }


        /// <summary>
        /// Retourne true si au moins un événement de tenue de postion (EVENTPOSACTIONDET) a été généré pour ce Trade à la date business {pBusinessDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pBusinessDate"></param>
        /// <returns></returns>
        public static bool IsEventsPosAction(string pCS, int pIdT, DateTime pBusinessDate)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEVENT), pBusinessDate);  // FI 20201006 [XXXXX] DbType.Date

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "1" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTPOSACTIONDET.ToString() + " epad" + SQLCst.ON + "(epad.IDE = e.IDE)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + " ec" + SQLCst.ON + "(ec.IDE = e.IDE)" + Cst.CrLf;
            sql += SQLCst.AND + "ec.DTEVENT = @DTEVENT" + Cst.CrLf;
            sql += SQLCst.WHERE + "e.IDT =@IDT" + Cst.CrLf;
            sql += SQLCst.AND + "e.EVENTCODE" + SQLCst.IN + "('EXE','ABN','ASS','POC','MOF','AEX','AAS','AAB','POT')" + Cst.CrLf;
            sql += SQLCst.AND + "e.EVENTTYPE" + SQLCst.IN + "('TOT','PAR')" + Cst.CrLf;

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// Retourne true si au moins un événement de tenue de position (EVENTPOSACTIONDET) a été généré pour ce Trade en date business &lt;= {pBusinessDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pBusinessDate"></param>
        /// <returns></returns>
        public static bool IsExistPosAction(string pCS, int pIdT, DateTime pBusinessDate)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pBusinessDate);   // FI 20201006 [XXXXX] DbType.Date

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "1" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " ev" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTPOSACTIONDET.ToString() + " epad" + SQLCst.ON + "(epad.IDE = ev.IDE)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSACTIONDET.ToString() + " pad" + SQLCst.ON + "(pad.IDPADET = epad.IDPADET)" + Cst.CrLf;
            sql += SQLCst.AND + "((pad.DTCAN is null)" + SQLCst.OR + "(pad.DTCAN > @DTBUSINESS))" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.POSACTION.ToString() + " pa" + SQLCst.ON + "(pa.IDPA = pad.IDPA)" + SQLCst.AND + "(pa.DTBUSINESS <= @DTBUSINESS)" + Cst.CrLf;
            sql += SQLCst.WHERE + "ev.IDT =@IDT" + Cst.CrLf;
            sql += SQLCst.AND + "ev.EVENTTYPE" + SQLCst.IN + "(" + DataHelper.SQLString(EventTypeFunc.Total) + "," + DataHelper.SQLString(EventTypeFunc.Partiel) + ")" + Cst.CrLf;

            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// Retourne true s'il existe un trade {pidT} est impliqué dans un CashBalance dont la date business &lt;= {pBusinessDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pBusinessDate"></param>
        /// <returns></returns>
        /// EG 20150407 [POC] New
        /// FI 20170331 [23031] Rename
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static bool IsExistInCashBalance(string pCS, IDbTransaction pDbTransaction, int pIdT, DateTime pBusinessDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS) , pBusinessDate); // FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(pCS, "FAMILY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.ProductFamily_CASHBALANCE);

            string sqlQuery = @"select 1
            from dbo.TRADE tr
            inner join dbo.TRADELINK tl on (tl.IDT_A = tr.IDT)
            inner join dbo.VW_ALLTRADEINSTRUMENT ti on (ti.IDT = tr.IDT)
            inner join dbo.VW_INSTR_PRODUCT ns on (ns.IDI = ti.IDI) and (ns.FAMILY = @FAMILY)
            where (tl.IDT_B = @IDT) and (tr.DTBUSINESS <= @DTBUSINESS)" + Cst.CrLf;

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery, parameters);
            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return (null != obj);
        }


        /// <summary>
        /// Retourne true s'il existe au minimum 1 trade CB à la date {pBusinessDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pBusinessDate"></param>
        /// <returns></returns>
        /// FI 20170330 [23031] Add Method
        public static bool IsExistCashBalance(string pCS, DateTime pBusinessDate)
        {
            return IsExistCashBalance(pCS, new DateTime[] { pBusinessDate });
        }

        /// <summary>
        /// Retourne true s'il existe au minimum 1 trade CB aux dates {pBusinessDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pBusinessDate"></param>
        /// <returns></returns>
        /// FI 20170330 [23031] Add Method
        public static bool IsExistCashBalance(string pCS, DateTime[] pBusinessDate)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "FAMILY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.ProductFamily_CASHBALANCE);

            string sqlQuery = StrFunc.AppendFormat(@"
select 1
from dbo.TRADE tr
inner join dbo.VW_INSTR_PRODUCT ns on (ns.IDI = tr.IDI) and (ns.FAMILY = @FAMILY)
where {0}", DataHelper.SQLColumnIn(pCS, "tr.DTBUSINESS", pBusinessDate, TypeData.TypeDataEnum.date));

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery, parameters);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pProcessTypeExclude"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static Cst.ProcessTypeEnum[] GetListEventProcess(string pCS, int pIdT, Cst.ProcessTypeEnum[] pProcessType,
            Cst.ProcessTypeEnum[] pProcessTypeExclude, ProcessStateTools.StatusEnum pStatus)
        {
            return GetListEventProcess(pCS, null, pIdT, pProcessType, pProcessTypeExclude, pStatus);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static Cst.ProcessTypeEnum[] GetListEventProcess(string pCS, IDbTransaction pDbTransaction, int pIdT, Cst.ProcessTypeEnum[] pProcessType,
            Cst.ProcessTypeEnum[] pProcessTypeExclude, ProcessStateTools.StatusEnum pStatus)
        {
            DataParameters dp = new DataParameters();
            Cst.ProcessTypeEnum[] ret = null;

            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.STATUS), pStatus);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);

            //Note: On ne scanne que les EVTs "vivants" (!= StatusActivation.DEACTIV)
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            //PL 20120827 Remove IDEP
            //sql += "ep.IDEP,ep.PROCESS" + Cst.CrLf;
            sql += "ep.PROCESS" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e" + Cst.CrLf;

            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE.ToString() + " t on t.IDT=e.IDT and t.IDT=@IDT" + Cst.CrLf;

            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTPROCESS.ToString() + " ep on ep.IDE=e.IDE and ep.IDSTPROCESS=@STATUS" + Cst.CrLf;

            if (ArrFunc.IsFilled(pProcessType))
                sql += SQLCst.AND + DataHelper.SQLColumnIn(pCS, "ep.PROCESS", pProcessType, TypeData.TypeDataEnum.@string);

            if (ArrFunc.IsFilled(pProcessTypeExclude))
                sql += SQLCst.AND + SQLCst.NOT + "(" + DataHelper.SQLColumnIn(pCS, "ep.PROCESS", pProcessTypeExclude, TypeData.TypeDataEnum.@string) + ")";

            sql += Cst.CrLf;
            //PL 20120827 Add e.IDT=@IDT
            sql += SQLCst.WHERE + "e.IDT=@IDT and e.IDSTACTIVATION!=" + DataHelper.SQLString(Cst.StatusActivation.DEACTIV.ToString()) + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dp);

            //20070717 FI utilisation de ExecuteDataTable [pour un eventuel stockage dans le cache]
            IDataReader dr = DataHelper.ExecuteDataTable(pCS, pDbTransaction, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader();

            ArrayList lst = new ArrayList();
            while (dr.Read())
            {
                Cst.ProcessTypeEnum item = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), dr["PROCESS"].ToString(), true);
                if (false == lst.Contains(item))
                    lst.Add(item);
            }

            dr.Close();

            if (ArrFunc.IsFilled(lst))
                ret = (Cst.ProcessTypeEnum[])lst.ToArray(typeof(Cst.ProcessTypeEnum));

            return ret;

        }

        /// <summary>
        /// Retourne true si le trade existe
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20190613 [24683] Use DbTransaction
        public static bool IsTradeExist(string pCs, int pIdT)
        {
            return IsTradeExist(pCs, null, pIdT);
        }
        public static bool IsTradeExist(string pCs, IDbTransaction pDbTransaction, int pIdT)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT), pIdT);
            //				
            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "1" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE.ToString() + " t" + Cst.CrLf;
            sql += SQLCst.WHERE + "t.IDT =@IDT" + Cst.CrLf;
            //
            object obj;
            if (null != pDbTransaction)
                obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
            else
                obj = DataHelper.ExecuteScalar(pCs, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        public static IDataReader IsTradeProvision(string pCs, int pIdT)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT), pIdT);
            paramameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.EVENTCODE), EventCodeFunc.Provision);
            //				
            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "EVENTCODE,EVENTTYPE" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + Cst.CrLf;
            sql += SQLCst.WHERE + "IDT=@IDT" + SQLCst.AND + "EVENTCODE=@EVENTCODE";
            //
            return DataHelper.ExecuteReader(pCs, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Retourne true s'il existe un évènement tel que EVENTCODE= 'RMV' 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsTradeRemove(string pCS, int pIdT)
        {
            return IsTradeRemove(pCS, null, pIdT);
        }
        /// <summary>
        /// Retourne true s'il existe un évènement tel que EVENTCODE= 'RMV' 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        /// EG 20180205 [23769] Add dbTransaction  
        public static bool IsTradeRemove(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            paramameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EVENTCODE), EventCodeFunc.RemoveTrade);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "1" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e" + Cst.CrLf;
            sql += SQLCst.WHERE + "e.IDT =@IDT" + Cst.CrLf;
            sql += SQLCst.AND + "e.EVENTCODE =@EVENTCODE" + Cst.CrLf;

            //20070717 FI utilisation de ExecuteDataTable [pour un eventuel stockage dans le cache]
            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// Retourne la version du Trade dont l'idT vaut {pIdt}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdt"></param>
        /// <returns></returns>
        public static EfsMLDocumentVersionEnum GetTradeVersion(string pCS, int pIdt)
        {
            DataParameter param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT);
            param.Value = pIdt;

            string sqlSelect = @"select EFSMLVERSION from dbo.TRADEXML where IDT = @IDT";
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlSelect, param.DbDataParameter);

            EfsMLDocumentVersionEnum ret;
            if (null != obj)
                ret = StringToEnum.EfsMLVersion(obj.ToString());
            else
                throw new Exception("trade is unknown");
            return ret;
        }

        /// <summary>
        /// Retourne la requete select sur la table TRADE pour le TRADE pIdT
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCol">Les colonnes du select</param>
        /// <returns></returns>
        // EG 20180530 [23980] Add pDbTransaction parameter
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public static QueryParameters GetQueryParametersTrade(string pCs, IDbTransaction pDbTransaction, int pIdT, string[] pCol)
        {
            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(pCs, pIdT)
            {
                // RD 20210304 Add "trx."            
                IsWithTradeXML = pCol.Contains("trx.TRADEXML"),
                DbTransaction = pDbTransaction
            };
            return sqlTrade.GetQueryParameters(pCol);
        }

        /// <summary>
        /// Retourne true si le trade est stocké dans TRADEACTOR
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="opIsActorWithBook"></param>
        /// <returns></returns>
        ///20081208 PL [TRIM 16423] Add new parameter opIsActorWithBook
        public static bool ExistSQLTradeActorEntry(string pCS, string pIdentifier, out bool opIsActorWithBook)
        {
            return ExistSQLTradeActorEntry(pCS, pIdentifier, SQL_TableWithID.IDType.Identifier, out opIsActorWithBook);
        }

        /// <summary>
        /// Obtient true si le trade est stocké dans TRADEACTOR
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIdType"></param>
        /// <param name="opIsActorWithBook"></param>
        /// <returns></returns>
        ///EG 20100401 Add new [SQL_TableWithID.IDType] parameter and one surcharge
        public static bool ExistSQLTradeActorEntry(string pCS, string pId, SQL_TableWithID.IDType pIdType, out bool opIsActorWithBook)
        {
            opIsActorWithBook = false;
            string sqlSelect = SQLCst.SELECT + DataHelper.SQLIsNull(pCS, "ta.IDB", "0") + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEACTOR.ToString() + " ta" + Cst.CrLf;

            DataParameter param;
            if (pIdType == SQL_TableWithID.IDType.Identifier)
            {
                param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDENTIFIER);
                param.Value = pId;
                sqlSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.TRADE, true, "ta.IDT", "t") + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "t.IDENTIFIER=@IDENTIFIER" + Cst.CrLf;
            }
            else if (pIdType == SQL_TableWithID.IDType.Id)
            {
                param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT);
                param.Value = IntFunc.IntValue(pId);
                sqlSelect += SQLCst.WHERE + "ta.IDT=@IDT" + Cst.CrLf;
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("parameter type {0} is not supported", pIdType.ToString()));
            }
            sqlSelect += SQLCst.ORDERBY + "1" + SQLCst.DESC;
            //20070717 FI utilisation de ExecuteScalar [pour un eventuel stockage dans le cache]
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlSelect, param.DbDataParameter);
            //NB: Pour info, avant les simuls examples ou les template n'avaient pas de Insert ds TRADEACTOR
            bool ret = null != obj;

            if (ret)
                opIsActorWithBook = (Convert.ToInt32(obj) > 0);
            return ret;
        }

        /// <summary>
        /// Retourne le book (id non sisgnificatif) présent dans TRADEACTOR 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        public static int GetTradeActorBookId(string pCS, string pIdT, int pIdA)
        {

            int ret = 0;
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            //
            string sqlSelect = SQLCst.SELECT + "IDB" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEACTOR.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDT=@IDT and IDA=@IDA";
            //20070717 FI utilisation de ExecuteScalar [pour un eventuel stockage dans le cache]
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, sqlSelect, parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToInt32(obj);
            //
            return ret;

        }

        /// <summary>
        /// Charge le texte Notepade d'un seule Trade
        /// </summary>
        /// <param name="pCs">ConnectionString</param>
        /// <param name="pIdT">IDT du Trade</param>
        /// <returns>StrBuilder</returns>
        public static StrBuilder LoadTradeNodepad(string pCs, int pIdT)
        {
            IDataReader dr = null;
            try
            {
                StrBuilder ret = null;
                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "nopad.LONOTE" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.NOTEPAD + " nopad" + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "nopad.ID=@IDT and nopad.TABLENAME='TRADE'";

                DataParameters dataparameters = new DataParameters();
                dataparameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT), pIdT);

                QueryParameters qry = new QueryParameters(pCs, sqlSelect.StringBuilder.ToString(), dataparameters);

                //20070717 FI utilisation de ExecuteDataTable pour une eventuelle mise en Cache
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                if (dr.Read())
                    ret = new StrBuilder(dr[0].ToString());
                //
                return ret;
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
        }

        /// <summary>
        /// Charge les Notepades et les Identifier de plusieurs Trades
        /// </summary>
        /// <param name="pCs">ConnectionString</param>
        /// <param name="pIdT">Array des IDT des Trades</param>
        /// <returns>IDataReader</returns>
        public static IDataReader LoadTradeNodepad(string pCs, int[] pIdT)
        {
            IDataReader dr = null;
            //

            if (ArrFunc.IsFilled(pIdT))
            {
                DataParameters dataparameters = new DataParameters();
                dataparameters.Add(new DataParameter(pCs, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), "TRADE");
                //
                ArrayList listIdT = new ArrayList();
                for (int i = 0; i < pIdT.Length; i++)
                {
                    if (pIdT[i] > 0)
                    {
                        string key = "IDT" + i.ToString();
                        dataparameters.Add(new DataParameter(pCs, key, DbType.Int32), pIdT[i]);
                        listIdT.Add(dataparameters[key].ParameterName);
                    }
                }
                //
                SQLWhere sqlWhere = new SQLWhere();
                if (ArrFunc.IsFilled(listIdT))
                    sqlWhere.Append(@"(nopad.ID in (" + ArrFunc.GetStringList(listIdT) + "))");
                sqlWhere.Append(@"nopad.TABLENAME = @TABLENAME");
                //
                StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                sqlSelect += "nopad.LONOTE, nopad.ID, t.IDENTIFIER" + Cst.CrLf;
                sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.NOTEPAD + " nopad" + Cst.CrLf;
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE + " t" + SQLCst.ON + "t.IDT = nopad.ID" + Cst.CrLf;
                sqlSelect += sqlWhere.ToString();
                //
                QueryParameters qry = new QueryParameters(pCs, sqlSelect.StringBuilder.ToString(), dataparameters);
                //
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
            }
            //
            return dr;
        }

        /// <summary>
        /// Obtient la liste des Trades associées aux évènements spcécifiés en entrée 
        /// </summary>
        /// <param name="pIdE">Liste des évènements</param>
        /// <returns></returns>
        public static int[] GetIdTradeFromIdEvent(string pCS, int[] pIdE)
        {
            return GetIdTradeFromIdEvent(pCS, string.Empty, pIdE);
        }

        /// <summary>
        /// Obtient la liste des Trades associées aux évènements spcécifiés en entrée 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIdE">Liste des évènements</param>
        /// <returns></returns>
        /// RD 20130430 [] Utilisation de la table EVENTLIST
        public static int[] GetIdTradeFromIdEvent(string pCS, string pSessionId, int[] pIdE)
        {
            if (ArrFunc.Count(pIdE) == 0)
                throw new ArgumentException("parameter pIdE is empty");

            int[] ret = null;
            bool isUseEVENTLIST = (StrFunc.IsFilled(pSessionId) && (ArrFunc.Count(pIdE) > EventRDBMSTools.SqlINListMax));

            DataParameters dp = new DataParameters();

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT_DISTINCT + "ev.IDT" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;

            if (ArrFunc.Count(pIdE) == 1)
            {
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDE), pIdE[0]);
                sql += SQLCst.WHERE + "(ev.IDE = @IDE)";
            }
            else if (false == isUseEVENTLIST)
            {
                sql += SQLCst.WHERE + DataHelper.SQLColumnIn(pCS, "ev.IDE", pIdE, TypeData.TypeDataEnum.integer, false, true);
            }
            else
            {
                // 1- Vider la table EVENTLIST
                EventRDBMSTools.DeleteEventList(pCS, pSessionId);

                // 2- Insérer la liste des IDE dans la table EVENTLIST
                EventRDBMSTools.InsertEventList(pCS, pIdE, pSessionId);

                // 3- Utiliser la table EVENTLIST en jointure
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);
                sql += EventRDBMSTools.SqlInnerEVENTLIST + Cst.CrLf;
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dp);
            IDataReader dr = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader();
            ArrayList lst = new ArrayList();
            while (dr.Read())
            {
                int idT = Convert.ToInt32(dr["IDT"]);
                lst.Add(idT);
            }
            dr.Close();

            // Vider la table EVENTLIST
            if (isUseEVENTLIST)
                EventRDBMSTools.DeleteEventList(pCS, pSessionId);

            if (ArrFunc.IsFilled(lst))
                ret = (int[])lst.ToArray(typeof(int));

            return ret;
        }

        /// <summary>
        /// Retourne l'instruction Delete des tables LINKID, TRADEID, TRADELINK, TRADE, TRLINKPOSREQUEST 
        /// </summary>
        /// <returns></returns>
        /// EG 20130617 Delete TRADELINK
        /// EG 20131127 [19254/19239]
        private static string GetSqlDeleteTrade(string pCS)
        {
            SQLWhere sqlWhere = new SQLWhere("IDT=@IDT");
            //Specifique à Oracle
            if (DataHelper.IsDbOracle(pCS))
                sqlWhere.Append("(@IDTSOURCE = @IDTSOURCE)" + Cst.CrLf);

            StrBuilder sqlDelete = new StrBuilder();

            #region Delete LINKID
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LINKID.ToString() + Cst.CrLf;
            sqlDelete += sqlWhere.ToString();
            sqlDelete += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;

            #endregion Delete LINKID
            #region Delete TRADEID
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADEID.ToString() + Cst.CrLf;
            sqlDelete += sqlWhere.ToString();
            sqlDelete += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;
            #endregion Delete TRADEID

            #region Delete/Update TRADELINK
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADELINK.ToString() + Cst.CrLf;
            sqlDelete += SQLCst.WHERE + "(IDT_B=@IDTSOURCE) and (IDT_A=@IDT)" + Cst.CrLf;
            sqlDelete += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADELINK.ToString() + Cst.CrLf;
            sqlDelete += SQLCst.WHERE + "(IDT_B=@IDT)";
            //Specifique à Oracle
            if (DataHelper.IsDbOracle(pCS))
                sqlDelete += SQLCst.AND + "(@IDTSOURCE = @IDTSOURCE)" + Cst.CrLf;
            sqlDelete += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;
            #endregion Delete/Update TRADELINK

            /// EG 20131127 [19254/19239]
            #region Delete/Update TRLINKPOSREQUEST
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRLINKPOSREQUEST.ToString() + Cst.CrLf;
            sqlDelete += sqlWhere.ToString();
            sqlDelete += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;
            #endregion Delete/Update TRLINKPOSREQUEST

            #region Delete TRADE
            sqlDelete += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADE.ToString() + Cst.CrLf;
            sqlDelete += sqlWhere.ToString();
            sqlDelete += SQLCst.SEPARATOR_MULTISELECT + Cst.CrLf;
            #endregion Delete TRADE
            return sqlDelete.ToString();
        }


        /// <summary>
        /// Retourne l'instruction Delete des tables POSREQUEST pour un IDT donnée 
        /// (utilisé principalement lors de la modification d'un trade ALLLOC jour avec suppression EVENT etc...)
        /// </summary>
        /// <returns></returns>
        private static string GetSqlDeletePosRequest()
        {
            return @"Delete from dbo.POSREQUEST where (IDT = @IDT);" + Cst.CrLf;
        }

        /// <summary>
        /// Retourne true si le trade est un template
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsTradeTemplate(string pCS, string pId, SQL_TableWithID.IDType pIdType)
        {
            return IsTradeTemplate(pCS, null, pId, pIdType);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20191115 [25077] Refactoring
        /// EG 20211217 [XXXXX] Possibilité de rechercher un TRADE par son EXTLLINK
        /// EG 20240305 [XXXXX] Possibilité de rechercher un TRADE par son DISPLAYNAME
        public static bool IsTradeTemplate(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType)
        {
            string sqlSelect = @"select 1
            from dbo.TRADE tr
            where (tr.IDSTENVIRONMENT = 'TEMPLATE') ";

            DataParameter param;
            if (pIdType == SQL_TableWithID.IDType.Identifier)
            {
                param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDENTIFIER);
                param.Value = pId;
                sqlSelect += @" and (tr.IDENTIFIER = @IDENTIFIER)" + Cst.CrLf;
            }
            else if (pIdType == SQL_TableWithID.IDType.Id)
            {
                param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT);
                param.Value = IntFunc.IntValue(pId);
                sqlSelect += @" and (tr.IDT = @IDT)" + Cst.CrLf;
            }
            else if (pIdType == SQL_TableWithID.IDType.ExtLink)
            {
                param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXTLLINK);
                param.Value = pId;
                sqlSelect += @" and (tr.EXTLLINK = @EXTLLINK)" + Cst.CrLf;
            }
            else if (pIdType == SQL_TableWithID.IDType.Displayname)
            {
                param = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DISPLAYNAME);
                param.Value = pId;
                sqlSelect += @" and (tr.DISPLAYNAME = @DISPLAYNAME)" + Cst.CrLf;
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("parameter type {0} is not supported", pIdType.ToString()));
            }

            object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlSelect.ToString(), param.DbDataParameter);
            bool ret = (null != obj);
            return ret;
        }

        public static string ReplaceDynamicConstantsWithEntityInfo(string pInitialString, SQL_Actor pSQL_Actor, SQL_Entity pSQL_Entity)
        {
            string finalString = pInitialString;
            if ((!String.IsNullOrEmpty(finalString)) && (finalString.IndexOf("%%") >= 0))
            {
                finalString = finalString.Replace(Cst.TRD_IDENTIFIER_ENTITY, pSQL_Actor.Identifier);
                finalString = finalString.Replace(Cst.TRD_EXTLLINK_ENTITY, pSQL_Actor.ExtlLink);
                finalString = finalString.Replace(Cst.TRD_EXTLLINKENTITY_ENTITY, pSQL_Entity.ExtlLink);
                finalString = finalString.Replace(Cst.TRD_BIC_ENTITY, pSQL_Actor.BIC);
                finalString = finalString.Replace(Cst.TRD_BIC4_ENTITY, pSQL_Actor.BIC.Substring(0, 4));
                finalString = finalString.Replace(Cst.TRD_BIC6_ENTITY, pSQL_Actor.BIC.Substring(0, 6));
                finalString = finalString.Replace(Cst.TRD_BIC8_ENTITY, pSQL_Actor.BIC.Substring(0, 8));
            }
            return finalString;
        }

        public static string ReplaceDynamicConstantsWithdateInfo(string pInitialString, string pConstant, DateTime pDate)
        {
            string finalString = pInitialString;
            if ((!String.IsNullOrEmpty(finalString)) && (finalString.IndexOf("%%") >= 0))
            {
                string dtFmt = DtFunc.FmtDateyyyyMMdd;
                if (pInitialString.IndexOf(pConstant.Substring(0, pConstant.Length - 2)) >= 0)
                {
                    if (pInitialString.IndexOf(pConstant) >= 0)
                    {
                        finalString = finalString.Replace(pConstant, DtFunc.DateTimeToString(pDate, dtFmt));
                    }
                    else
                    {
                        //Présence d'un format (ex. %%TRANSACTDATE.yyMMdd%%)
                        int start = pInitialString.IndexOf(pConstant.Substring(0, pConstant.Length - 2));
                        int end = pInitialString.IndexOf("%%", start + 1);
                        dtFmt = pInitialString.Substring(start + pConstant.Length - 1, end - start - pConstant.Length + 1);

                        finalString = finalString.Replace(pConstant.Substring(0, pConstant.Length - 2) + "." + dtFmt + "%%", DtFunc.DateTimeToString(pDate, dtFmt));
                    }
                }
            }
            return finalString;
        }

        /// <summary>
        /// Vider la table TRADELIST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataParameters"></param>
        public static void DeleteTradeList(string pCS, string pSessionId)
        {
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);
            QueryParameters qryParameters = new QueryParameters(pCS, SqlDeleteTRADELIST, dp);

            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Insérer la liste des IDT {pIdT} dans la table TRADELIST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pDataParameters"></param>
        /// FI 20160223 [21919] Modify
        public static void InsertTradeList(string pCS, int[] pIdT, string pSessionId)
        {
            // FI 20160223 [21919] Mise en commentaire
            //DataParameters dp = new DataParameters();
            //dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);
            //dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT));
            //QueryParameters qryParameters = new QueryParameters(pCS, SqlInsertTRADELIST, dp);

            //foreach (int idt in pIdT)
            //{
            //    qryParameters.Parameters["IDT"].Value = idt;
            //    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query,
            //    qryParameters.Parameters.GetArrayDbParameter());
            //}

            // FI 20160223 [21919] Usage d'un select UNIONALL

            string fromDual = DataHelper.SQLFromDual(pCS);
            StrBuilder sb = new StrBuilder();
            foreach (int idt in pIdT)
            {
                if (sb.Length > 0)
                    sb += StrFunc.AppendFormat("{0}{1}", SQLCst.UNIONALL, Cst.CrLf);
                sb += StrFunc.AppendFormat(@"select {0}, '{1}' {2} {3}", idt, pSessionId, fromDual, Cst.CrLf);
            }

            string queryInsert = "insert into dbo.TRADELIST(IDT,SESSIONID)" + Cst.CrLf + sb.ToString();
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, queryInsert);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pTradeXML"></param>
        /// <param name="pSession"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdProcess_L"></param>
        /// <param name="pIdTRK_L"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] Add (cette fonction existait dans EventQuery)
        /// FI 20170306 [22225] GLOP (vérifier si le contrôle chgt de journée vérifie bien que des trades n'ont pas été modifés post traitement de journée)
        /// FI 20170323 [XXXXX] Modification de signature (ajou paramètres nécessaires à SaveTradeTrail)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20220207 [XXXX] Incoherece sur Identifiant de trade Piste d'audit / Données TRADETRAIL incomplète
        // =>
        // La mise à jour des frais par les services (EVENT, EOD) entrainent automatiquement une mise à jour des tables TRADEXML et TRADETRAIL. 
        // La mise à jour de TRADEXML doit nécessairement être opérée avant celle de TRADETRAIL
        // pour pouvoir récupérer les identifiants(IDTRADE_P et IDTRADEXML_P) et mettre à jour correctement la table TRADETRAIL.
        public static void UpdateTradeXML(IDbTransaction pDbTransaction, int pIdT, StringBuilder pTradeXML, DateTime pDtSys,
            int pIdA, AppSession pSession, int pIdTRK_L, int pIdProcess_L)
        {
            string cs = pDbTransaction.Connection.ConnectionString;

            //SaveTradeTrail(cs, pDbTransaction, pIdT, null, Cst.Capture.ModeEnum.Update, pDtSys, pIdA, pAppInstance, pIdTRK_L, pIdProcess_L);

            string sqlUpdate = @"update dbo.TRADEXML set TRADEXML = @TRADEXML where (IDT = @IDT)";
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(cs, "TRADEXML", DbType.Xml), pTradeXML.ToString());

            QueryParameters qryParameters = new QueryParameters(cs, sqlUpdate, parameters);
            _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            //FI 20170323 [XXXXX] Appel à SaveTradeTrail
            SaveTradeTrail(cs, pDbTransaction, pIdT, null, Cst.Capture.ModeEnum.Update, pDtSys, pIdA, pSession, pIdTRK_L, pIdProcess_L);
        }

        /// <summary>
        /// Alimentation de SaveTradeTrail lors d'une sauvegarde de trade
        /// <para>L'appel à la méthode doit être effectuée après mise à jour de la table TRADE (insert ou update)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT">identifiant IdT du trade</param>
        /// <param name="pIdScreen">Screen (null possible en modification => screen précédent est concervé) </param>
        /// <param name="pCaptureMode">Typde d'action (création, Modification,etc..)</param>
        /// <param name="pDtSys">Date système</param>
        /// <param name="pIda">Acteur à l'origine de la création, modification du trade</param>
        /// <param name="pSession">représente la session</param>
        /// <param name="pIdTRK_L">Id de la demande de traitement (doit être renseigné si création, modification d'un trade lors d'un traitement)</param>
        /// <param name="pIdProcess_L">Id du process (doit être renseigné si création, modification d'un trade lors d'un traitement)</param>
        /// FI 20170323 [XXXXX] add Method (Cette méthode existait initialement dans TradeCommonCaptureGen, placé ici pour faciliter son appel ailleurs)
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190613 [24683] Use Datatable instead of DataReader
        // EG 20190924 Use sqlUpdate
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200903 [25468] Correction (Passage @IDTRK_L en parameter oublié)
        // FI 20200904 [25468] Nouvelle Correction (Paramètres mal positionnés dans la requête)
        public static void SaveTradeTrail(string pCS, IDbTransaction pDbTransaction, int pIdT,
                            string pIdScreen, Cst.Capture.ModeEnum pCaptureMode, DateTime pDtSys, int pIda, AppSession pSession,
                            Nullable<int> pIdTRK_L, Nullable<int> pIdProcess_L)
        {
            // FI 20170323 [XXXXX] Nouveauté par rapport à la méthode qui existait initialement dans TradeCommonCaptureGen
            // lecture du screen précédent lorsqu'il est non renseigné
            if (StrFunc.IsEmpty(pIdScreen))
            {
                SQL_LastTrade_L sqlTrail = new SQL_LastTrade_L(pCS, pIdT, null)
                {
                    DbTransaction = pDbTransaction
                };
                sqlTrail.LoadTable(new string[] { "SCREENNAME" });
                pIdScreen = sqlTrail.ScreenName;
            }

            Nullable<int> idTrade_P = null;
            Nullable<int> idTradeXML_P = null;

            #region Select IDTRADE_P/IDTRADEXML_P
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            string sqlQuery = @"select tr.IDTRADE_P, trx.IDTRADEXML_P 
            from dbo.TRADE_P tr
            inner join dbo.TRADEXML_P trx on (trx.IDT = tr.IDT)
            where tr.IDT = @IDT 
            order by tr.IDTRADE_P desc, trx.IDTRADEXML_P desc";

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text,
                queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    idTrade_P = Convert.ToInt32(dr["IDTRADE_P"]);
                    idTradeXML_P = Convert.ToInt32(dr["IDTRADEXML_P"]);
                }
            }
            #endregion Select IDTRADE_P/IDTRADEXML_P

            if (idTrade_P.HasValue && idTradeXML_P.HasValue)
            {
                #region Update last TRADETRAIL with new IDTRADE_P
                dp = new DataParameters();
                dp.Add(new DataParameter(pCS, "IDTRADE_P", DbType.Int32), idTrade_P.Value);
                dp.Add(new DataParameter(pCS, "IDTRADEXML_P", DbType.Int32), idTradeXML_P.Value);
                dp.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                string sqlUpdate = @"update dbo.TRADETRAIL set 
                IDTRADE_P = @IDTRADE_P,IDTRADEXML_P = @IDTRADEXML_P 
                where IDT = @IDT and IDTRADE_P is null";
                // EG 20190924 Use sqlUpdate
                queryParameters = new QueryParameters(pCS, sqlUpdate, dp);
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                #endregion Update last TRADETRAIL with new IDTRADE_P
            }

            

            // FI 20200820 [25468] Utilisation de parameters et Dates systemes en UTC
            dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIda);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSYS), pDtSys);
            dp.Add(new DataParameter(pCS, "ACTION", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PermissionTools.GetPermission(pCaptureMode).ToString());
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPNAME), pSession.AppInstance.AppNameInstance);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.HOSTNAME), pSession.AppInstance.HostName);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPVERSION), pSession.AppInstance.AppVersion);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPBROWSER), pSession.BrowserInfo);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDPROCESS_L), pIdProcess_L ?? Convert.DBNull);
            dp.Add(new DataParameter(pCS, "IDTRK_L", DbType.Int32), pIdTRK_L ?? Convert.DBNull);
            dp.Add(new DataParameter(pCS, "SCREENNAME", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pIdScreen);

            //Le dernier enregistrement dans TRADETRAIL correspond au trade present dans TRADE donc pas de IDTRADE_P car pas de ligne correspondante dans TRADE_P
            sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADETRAIL + Cst.CrLf;
            sqlQuery += "(IDT, IDA, DTSYS, ACTION, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, IDPROCESS_L, IDTRADE_P, IDTRK_L, SCREENNAME)" + Cst.CrLf;
            sqlQuery += "values (@IDT, @IDA, @DTSYS, @ACTION, @HOSTNAME, @APPNAME, @APPVERSION, @APPBROWSER, @IDPROCESS_L, null, @IDTRK_L, @SCREENNAME)";
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery, dp.GetArrayDbParameter());

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class EventRDBMSTools
    {
        private static readonly string SqlDeleteEVENTLIST = "delete from dbo.EVENTLIST where (SESSIONID=@SESSIONID)";
        // EG 20160404 Migration vs2013
        //private static string SqlInsertEVENTLIST = "insert into dbo.EVENTLIST (IDE, SESSIONID) values (@IDE, @SESSIONID)";
        public static string SqlInnerEVENTLIST = "inner join dbo.EVENTLIST lst_e on (lst_e.IDE=ev.IDE) and (lst_e.SESSIONID=@SESSIONID)";
        /// <summary>
        /// Nombre maximum d'éléments IDE dans une liste IN
        /// </summary>
        public static int SqlINListMax = 50; 
                
        /// <summary>
        /// Retourne true si l'évènement {pIdE} a subi un des traitements définis sous {pProcessType} avec succès
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdE"></param>
        /// <param name="pProcessType"></param>
        /// <returns></returns>
        public static bool ExistEventProcess(string pCs, int pIdE, Cst.ProcessTypeEnum[] pProcessType)
        {
            return ExistEventProcess(pCs, pIdE, pProcessType, null, ProcessStateTools.StatusSuccessEnum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdE"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pProcessTypeExclude"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static bool ExistEventProcess(string pCs, int pIdE, Cst.ProcessTypeEnum[] pProcessType, Cst.ProcessTypeEnum[] pProcessTypeExclude, ProcessStateTools.StatusEnum pStatus)
        {
            return ArrFunc.IsFilled(GetListEventProcess(pCs, pIdE, pProcessType, pProcessTypeExclude, pStatus));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdE"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pProcessTypeExclude"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        /// FI 20120711 Tuning => usage de paramètres SQL
        public static Cst.ProcessTypeEnum[] GetListEventProcess(string pCs, int pIdE, Cst.ProcessTypeEnum[] pProcessType,
            Cst.ProcessTypeEnum[] pProcessTypeExclude, ProcessStateTools.StatusEnum pStatus)
        {
            Cst.ProcessTypeEnum[] ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), pIdE);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.STATUS), pStatus.ToString());

            //Note: On ne scanne que les EVTs "vivants" (!= StatusActivation.DEACTIV)
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            //PL 20120827 Remove IDEP
            //string sql = "ep.IDEP,ep.PROCESS" + Cst.CrLf;
            sql += "ep.PROCESS" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e" + Cst.CrLf;

            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTPROCESS.ToString() + " ep on ep.IDE=e.IDE and ep.IDSTPROCESS=@STATUS" + Cst.CrLf;

            if (ArrFunc.IsFilled(pProcessType))
                sql += SQLCst.AND + DataHelper.SQLColumnIn(pCs, "ep.PROCESS", pProcessType, TypeData.TypeDataEnum.@string);

            if (ArrFunc.IsFilled(pProcessTypeExclude))
                sql += SQLCst.AND + SQLCst.NOT + "(" + DataHelper.SQLColumnIn(pCs, "ep.PROCESS", pProcessTypeExclude, TypeData.TypeDataEnum.@string) + ")";

            sql += Cst.CrLf;
            sql += SQLCst.WHERE + "e.IDE=@ID and e.IDSTACTIVATION!=" + DataHelper.SQLString(Cst.StatusActivation.DEACTIV.ToString());

            QueryParameters query = new QueryParameters(pCs, sql.ToString(), dp);

            //20070717 FI utilisation de ExecuteDataTable [pour un eventuel stockage dans le cache]
            IDataReader datareader = DataHelper.ExecuteDataTable(pCs, query.Query, query.Parameters.GetArrayDbParameter()).CreateDataReader();
            //
            ArrayList lst = new ArrayList();
            while (datareader.Read())
            {
                Cst.ProcessTypeEnum item = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), datareader["PROCESS"].ToString(), true);
                if (false == lst.Contains(item))
                    lst.Add(item);
            }
            // EG 20120202 Add Close()
            datareader.Close();
            //
            if (ArrFunc.IsFilled(lst))
                ret = (Cst.ProcessTypeEnum[])lst.ToArray(typeof(Cst.ProcessTypeEnum));
            //
            return ret;

        }

        /// <summary>
        /// Recherche l'index de l'évènement  dans la liste des évènements d'un trade "equivalents" (même IDT,EVENTCODE,EVENTTYPE,INSTRUMENTNO,STREAMNO) à un évènement de référence
        /// <para>Avec la valeur retour Sphere® peut attaquer l'objet FpML qui est à l'origine de l'évènement</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlEvent"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int IndexOf(string pCS, SQL_Event pSqlEvent)
        {

            int ret = -1;
            //
            //Postulats: Les Evts sont ds le même ordre que les objects Fpml
            //
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pSqlEvent.IdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, 3), pSqlEvent.EventCode);
            parameters.Add(new DataParameter(pCS, "EVENTTYPE", DbType.AnsiString, 3), pSqlEvent.EventType);
            parameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), pSqlEvent.InstrumentNo);
            parameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), pSqlEvent.StreamNo);
            //
            StrBuilder sqlSelectEvent = new StrBuilder(SQLCst.SELECT + @"e.IDE as ID") + Cst.CrLf;
            sqlSelectEvent += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
            sqlSelectEvent += SQLCst.WHERE + @"(e.IDT=@IDT)" + Cst.CrLf;
            sqlSelectEvent += SQLCst.AND + @"(e.EVENTCODE=@EVENTCODE)" + Cst.CrLf;
            sqlSelectEvent += SQLCst.AND + @"(e.EVENTTYPE=@EVENTTYPE)" + Cst.CrLf;
            sqlSelectEvent += SQLCst.AND + @"(e.INSTRUMENTNO=@INSTRUMENTNO)" + Cst.CrLf;
            sqlSelectEvent += SQLCst.AND + @"(e.STREAMNO=@STREAMNO)" + Cst.CrLf;
            sqlSelectEvent += SQLCst.ORDERBY + "e.IDE" + Cst.CrLf;

            int i = -1;
            string SQLSelect = sqlSelectEvent.ToString();
            using (IDataReader dataReader = DataHelper.ExecuteReader(pCS, CommandType.Text, SQLSelect, parameters.GetArrayDbParameter()))
            {
                while (dataReader.Read())
                {
                    i++;
                    if (Convert.ToInt32(dataReader.GetValue(0)) == pSqlEvent.Id)
                    {
                        ret = i;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne la liste des évènements selon les critères  {pIdT,pEventCode,pEventType,pInstrumentNo,pStreamNo);
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <param name="pInstrumentNo"></param>
        /// <param name="pStreamNo"></param>
        /// <param name="pSqlEvent"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static int[] GetEvents(string pCS, int pIdT, string pEventCode, string pEventType, int pInstrumentNo, int pStreamNo)
        {
            return GetEvents(pCS, null, pIdT, pEventCode, pEventType, pInstrumentNo, pStreamNo);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetEvents(string pCS, IDbTransaction pDbTransaction, int pIdT, string pEventCode, string pEventType, int pInstrumentNo, int pStreamNo)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "EVENTCODE", DbType.AnsiString, 3), pEventCode);
            parameters.Add(new DataParameter(pCS, "EVENTTYPE", DbType.AnsiString, 3), pEventType);
            parameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), pInstrumentNo);
            parameters.Add(new DataParameter(pCS, "STREAMNO", DbType.Int32), pStreamNo);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"e.IDE as ID") + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + @"(e.IDT=@IDT)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + @"(e.EVENTCODE=@EVENTCODE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + @"(e.EVENTTYPE=@EVENTTYPE)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + @"(e.INSTRUMENTNO=@INSTRUMENTNO)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + @"(e.STREAMNO=@STREAMNO)" + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "e.IDE" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            List<int> lst = new List<int>();
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                    lst.Add(IntFunc.IntValue2(dr.GetValue(0).ToString(), System.Globalization.CultureInfo.InvariantCulture));
            }
            return lst.ToArray();
        }

        /// <summary>
        /// Vider la table EVENTLIST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSessionId"></param>
        public static void DeleteEventList(string pCS, string pSessionId)
        {
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);

            QueryParameters qryParameters = new QueryParameters(pCS, SqlDeleteEVENTLIST, dp);

            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Insérer la liste des IDE {pIdE} dans la table EVENTLIST
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdE"></param>
        /// <param name="pSessionId"></param>
        // FI 20160223 [21919] Modify
        public static void InsertEventList(string pCS, int[] pIdE, string pSessionId)
        {
            //DataParameters dp = new DataParameters();
            //dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);
            //dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDE));

            //QueryParameters qryParameters = new QueryParameters(pCS, SqlInsertEVENTLIST, dp);

            //foreach (int ide in pIdE)
            //{
            //    qryParameters.Parameters["IDE"].Value = ide;
            //    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qryParameters.Query,
            //        qryParameters.Parameters.GetArrayDbParameter());
            //}
            // FI 20160223 [21919] Usage d'un select UNIONALL
            string fromDual = DataHelper.SQLFromDual(pCS);
            StrBuilder sb = new StrBuilder();
            foreach (int idE in pIdE)
            {
                if (sb.Length > 0)
                    sb += StrFunc.AppendFormat("{0}{1}", SQLCst.UNIONALL, Cst.CrLf);
                sb += StrFunc.AppendFormat(@"select {0}, '{1}' {2} {3}", idE, pSessionId, fromDual, Cst.CrLf);
            }
            string queryInsert = "insert into dbo.EVENTLIST (IDE, SESSIONID)" + Cst.CrLf + sb.ToString();
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, queryInsert);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class PosCollateralRDBMSTools
    {

        /// <summary>
        /// Retourne true si le collateral existe
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdT"></param>
        /// <returns></returns>
        public static bool IsCollateralExist(string pCs, int pId)
        {
            DataParameters paramameters = new DataParameters();
            paramameters.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.ID), pId);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "1" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERAL.ToString() + " c" + Cst.CrLf;
            sql += SQLCst.WHERE + "c.IDPOSCOLLATERAL =@ID" + Cst.CrLf;

            object obj = DataHelper.ExecuteScalar(pCs, CommandType.Text, sql.ToString(), paramameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// Retourne les couvertures qui s'appliquent au contexte (payer, receiver, dtBusiness, asset) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="payer">Représente le payer [couple (ACTOR,BOOK)]</param>
        /// <param name="receiver">Représente le receiver [couple (ACTOR,BOOK)]</param>
        /// <param name="dtBusiness"></param>
        /// <param name="asset">Représente l'asset [Couple (Type d'asset, IDASSET)]</param>
        /// <returns></returns>
        /// FI 20130425 [18598] plusieurs couvertures peuvent s'appliquer 
        public static int[] GetId(string pCS, Pair<int, int> payer, Pair<int, int> receiver, DateTime dtBusiness, Pair<Cst.UnderlyingAsset, int> asset)
        {
            return GetId(pCS, null, payer, receiver, dtBusiness, asset);
        }
        /// <summary>
        /// Retourne les couvertures qui s'appliquent au contexte (payer, receiver, dtBusiness, asset) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="payer">Représente le payer [couple (ACTOR,BOOK)]</param>
        /// <param name="receiver">Représente le receiver [couple (ACTOR,BOOK)]</param>
        /// <param name="dtBusiness"></param>
        /// <param name="asset">Représente l'asset [Couple (Type d'asset, IDASSET)]</param>
        /// <returns></returns>
        /// FI 20130425 [18598] plusieurs couvertures peuvent s'appliquer 
        // EG 20180205 [23769] Upd DataHelper.ExecuteNonQuery
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetId(string pCS, IDbTransaction pDbTransaction, Pair<int, int> payer, Pair<int, int> receiver, DateTime dtBusiness, Pair<Cst.UnderlyingAsset, int> asset)
        {
            int[] ret = null;

            DataParameters parameters = new DataParameters();
            //payer
            parameters.Add(new DataParameter(pCS, "IDA_PAY", DbType.Int32), payer.First);
            parameters.Add(new DataParameter(pCS, "IDB_PAY", DbType.Int32), payer.Second);
            //Receiver
            parameters.Add(new DataParameter(pCS, "IDA_REC", DbType.Int32), receiver.First);
            parameters.Add(new DataParameter(pCS, "IDB_REC", DbType.Int32), receiver.Second);
            // date
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), dtBusiness);
            //asset
            parameters.Add(new DataParameter(pCS, "ASSETCATEGORY", DbType.String, 64), asset.First);
            parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), asset.Second);

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect += OTCmlHelper.GetColunmID(Cst.OTCml_TBL.POSCOLLATERAL.ToString()) + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSCOLLATERAL.ToString() + " pos " + Cst.CrLf; ;
            sqlSelect += SQLCst.WHERE + Cst.CrLf;
            sqlSelect += "IDA_PAY = @IDA_PAY" + SQLCst.AND + Cst.CrLf;
            sqlSelect += "IDB_PAY = @IDB_PAY" + SQLCst.AND + Cst.CrLf;
            sqlSelect += "IDA_REC = @IDA_REC" + SQLCst.AND + Cst.CrLf;
            sqlSelect += "IDB_REC = @IDB_REC" + SQLCst.AND + Cst.CrLf;
            sqlSelect += "DTBUSINESS = @DTBUSINESS" + SQLCst.AND + Cst.CrLf;
            sqlSelect += "ASSETCATEGORY = @ASSETCATEGORY" + SQLCst.AND + Cst.CrLf;
            sqlSelect += "IDASSET = @IDASSET" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteDataTable(pCS, pDbTransaction, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                List<int> lst = new List<int>();
                if (null != dr)
                {
                    while (dr.Read())
                    {
                        lst.Add(Convert.ToInt32(dr[0]));
                    }
                }

                if ((lst.Count) > 0)
                    ret = lst.ToArray();
            }
            return ret;
        }
    }
}

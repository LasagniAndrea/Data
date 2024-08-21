#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using System;
using System.Data;
#endregion Using Directives


namespace EFS.Process.PosKeeping
{
    //────────────────────────────────────────────────────────────────────────────────────────────────
    // B A S E   
    //────────────────────────────────────────────────────────────────────────────────────────────────

    public partial class PosKeepingGenProcessBase
    {
        #region GetQueryPositionTradeAndAction
        /// <summary>
        /// Utilisée par le traitement de transfert et de correction de position
        /// 1ere requête (GetQueryPositionByTrade)
        ///      Retourne la négociation en cours de traitement (POC|POT) et la quantité en position
        /// 2eme requete (GetQueryPositionActionByTrade)
        ///      Retourne les actions sur position operées sur la négociation
        /// </summary>
        /// <param name="pIsExceptCurrentIdPR">Si = true : à l'exception de celle concernant le POSREQUEST en cours</param>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : New
        protected virtual string GetQueryPositionTradeAndAction(bool pIsExceptCurrentIdPR)
        {
            string sqlSelect = GetQueryPositionByTrade(pIsExceptCurrentIdPR);
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += GetQueryPositionActionByTrade(pIsExceptCurrentIdPR);
            return sqlSelect;
        }
        #endregion GetQueryPositionTradeAndAction
        #region GetQueryPositionByTrade
        /// <summary>
        /// Utilisée par le traitement de transfert et de correction de position
        ///      Retourne la négociation en cours de traitement (POC|POT) et la quantité en position
        /// </summary>
        /// <param name="pIsExceptCurrentIdPR">Si = true : à l'exception de celle concernant le POSREQUEST en cours</param>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring : New
        /// EG 20170412 [23081] Refactoring : Change GetQueryPositionActionBySide by GetQryPosAction_Trade_BySide
        /// EG 20171016 [23509] Add TZFACILITY, DTEXECUTION
        protected virtual string GetQueryPositionByTrade(bool pIsExceptCurrentIdPR)
        {
            // TRADE IN POSITION
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.IDI, tr.IDASSET, tr.IDM, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.PRICE, 
            (tr.QTY - isnull(pab.QTY, 0) - isnull(pas.QTY, 0)) as QTY, tr.DTBUSINESS, tr.SIDE, 
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, ev.IDE as IDE_EVENT, tr.ASSETCATEGORY
            from dbo.{0} tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{1})
            left outer join (" + PosKeepingTools.GetQryPosAction_Trade_BySide(BuyerSellerEnum.BUYER, false, pIsExceptCurrentIdPR) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_Trade_BySide(BuyerSellerEnum.SELLER, false, pIsExceptCurrentIdPR) + @") pas on (pas.IDT = tr.IDT)
            where (tr.IDT = @IDT)" + Cst.CrLf;

            return String.Format(sqlSelect, VW_TRADE_POS, RESTRICT_EVENT_POS);
        }
        #endregion GetQueryPositionTradeAndAction
        #region GetQueryPositionActionByTrade
        /// <summary>
        /// Retourne les actions sur position operées sur la négociation
        /// Utilisée par 
        /// 1. le traitement de transfert et de correction de position
        /// 2. le traitement de dénouement d'options (manuel, automatique, manuel via importation par position)
        /// </summary>
        /// <param name="pIsExceptCurrentIdPR">Si = true : à l'exception de celle concernant le POSREQUEST en cours</param>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : New
        protected virtual string GetQueryPositionActionByTrade(bool pIsExceptCurrentIdPR)
        {
            // POSACTION BY TRADE
            string sqlSelect = @"select pa.IDPA, pad.IDPADET, pad.IDT_BUY, pad.IDT_SELL, pad.IDT_CLOSING, pad.QTY, pad.POSITIONEFFECT, tr.SIDE as SIDE_CLOSING
            from dbo.POSACTION pa 
            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
            inner join dbo." + VW_TRADE_POS + @" tr on (tr.IDT = pad.IDT_CLOSING)
            where (pad.IDT_CLOSING = @IDT)" + Cst.CrLf;
            if (pIsExceptCurrentIdPR)
                sqlSelect += "and (isnull(pa.IDPR,0) <> @IDPR)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryPositionActionByTrade

        #region GetQueryPositionAndActionOnDtBusiness
        /// <summary>
        /// Utilisée par le traitement de clôture de masse, de clôture spécifique et de clôture de masse avant Liquidation de future à l'échéance
        /// 1ere requête (GetQueryPosition)
        ///      Retourne les négociations en position pour une clé de position donnée avec leur quantité en position
        /// 2eme requete (GetQueryPositionActionOnDtBusiness)
        ///      Retourne les actions sur position de type compensation|clôture (CLEAREOD|CLEARBULK|UPDENTRY) 
        ///      operées sur la négociation en date de journée de bourse
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : New
        protected virtual string GetQueryPositionAndActionOnDtBusiness()
        {
            string sqlSelect = GetQueryPosition();
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += GetQueryPositionActionOnDtBusiness(false);
            return sqlSelect;
        }
        #endregion GetQueryPositionAndActionOnDtBusiness
        #region GetQueryPosition
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring
        /// EG 20170412 [23081] Refactoring : Change GetQueryPositionActionBySide by GetQryPosAction_BySide
        /// EG 20171016 [23509] Add TZFACILITY, DTEXECUTION
        /// EG 20211012 [XXXXX] Add TYPEPRICE, ACCINTRATE
        protected virtual string GetQueryPosition()
        {
            // TRADES en POSITION
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.IDI, tr.IDM, tr.IDASSET, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.PRICE, tr.TYPEPRICE, tr.ACCINTRATE,
            tr.DTBUSINESS, tr.SIDE, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, ev.IDE_EVENT,
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0)) as QTY
            from dbo.{0} tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{1}) and (ev.{2})
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER) + @") pas on (pas.IDT = tr.IDT)
            where (tr.DTTRADE <= @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) > 0) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER, 0) = @IDB_CLEARER) and 
            (tr.IDA_DEALER = @IDA_DEALER) and (tr.IDB_DEALER = @IDB_DEALER)" + Cst.CrLf;

            return String.Format(sqlSelect, VW_TRADE_POS, RESTRICT_EVENTCODE_POS, RESTRICT_EVENTTYPE_POS);
        }
        #endregion GetQueryPositionOnDtBusiness
        #region GetQueryPositionActionOnDtBusiness
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // RD 20170308 [22660] Modify
        protected virtual string GetQueryPositionActionOnDtBusiness(bool pIsExceptCurrentIdPR)
        {
            // POSACTION du jour
            // RD 20170308 [22660] Add the ENTRY action to the Where clause
            string sqlSelect = @"select pa.IDPA, pad.IDPADET, isnull(pad.IDT_BUY,0) as IDT_BUY, isnull(pad.IDT_SELL,0) as IDT_SELL, pad.IDT_CLOSING, pad.QTY,
            tr.SIDE as SIDE_CLOSING, pa.IDPR, pad.POSITIONEFFECT
            from dbo.POSACTION pa
            inner join dbo.POSACTIONDET pad on (pad.IDPA = pa.IDPA) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
            inner join dbo." + VW_TRADE_POS + @" tr on (tr.IDT = pad.IDT_CLOSING)
            where (pa.DTBUSINESS = @DTBUSINESS) and (pr.DTBUSINESS = @DTBUSINESS) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER,0) = @IDB_CLEARER) and
            (pr.REQUESTTYPE in ('CLEAREOD','CLEARBULK','UPDENTRY','ENTRY'))" + Cst.CrLf;

            AddSqlRestrictDealer(ref sqlSelect);

            if (pIsExceptCurrentIdPR)
                sqlSelect += "and (isnull(pa.IDPR,0) <> @IDPR)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryPositionActionOnDtBusiness

        #region GetQueryPositionVeilAndTradeDayAndActionOnDtBusiness
        /// <summary>
        /// Utilisée par le traitement de mise à jour des clôture en STP, en masse ou via traitement EOD (Entry|UpdateEntry|EOD_UpdateEntry)
        /// 1ere requête (GetQueryPositionVeilAndTradeDay)
        ///     Retourne les négociations veille en position et les négociations du jour
        /// 2eme requete (GetQueryPositionActionOnDtBusiness)
        ///     Retourne les actions sur position de type compensation|clôture (CLEAREOD|CLEARBULK|UPDENTRY|ENTRY) 
        ///     operées sur la négociation en date de journée de bourse
        /// </summary>
        ///<returns></returns>
        ///EG 20151125 [20979] Refactoring : New
        // EG 20201124 [XXXXX] Utilisation de la table temporaire créée auparavant en lieu et place de la vue VW_TRADE_XXX
        // EG 20250125 [WI820] Close-outs : Error on Launch Process (Pas de lecture de table de travail si mode ITD)
        protected virtual string GetQueryPositionVeilAndTradeDayAndActionOnDtBusiness()
        {
            string tableName = (m_PosRequest.IdTSpecified? VW_TRADE_POS: StrFunc.AppendFormat("TRADE_UE_{0}_W", PKGenProcess.Session.BuildTableId()).ToUpper());
            string sqlSelect = GetQueryPositionVeilAndTradeDay(tableName);
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += GetQueryPositionActionOnDtBusiness(false).Replace(VW_TRADE_POS,tableName);
            return sqlSelect;
        }
        #endregion GetQueryPositionVeilAndTradeDayAndActionOnDtBusiness
        #region GetQueryPositionVeilAndTradeDay
        /// <summary>
        /// Utilisée par le traitement de mise à jour des clôture en STP, en masse ou via traitement EOD (Entry|UpdateEntry|EOD_UpdateEntry)
        ///     Retourne les négociations veille en position et les négociations du jour
        ///     La jointure sur EVENT (dynamique en fonction du groupe de produit) permet de récupérer l'IDE parent utilisé pour les insertions d'EVTs dans la suite du traitement
        /// </summary>
        ///<returns></returns>
        /// EG 20170109 Refactoring
        /// EG 20170412 [23081] Refactoring : Change GetQueryPositionActionBySide by GetQryPosAction_BySide|GetQryPosAction_Entry_BySide
        /// EG 20171016 [23509] Add TZFACILITY, DTEXECUTION 
        // EG 20171123 BUG UPDENTRY
        // EG 20190730 Add TYPEPRICE|ACCINTRATE
        // EG 20201124 [XXXXX] Utilisation de la table temporaire créée auparavant en lieu et place de la vue VW_TRADE_XXX
        protected virtual string GetQueryPositionVeilAndTradeDay(string pTableName)
        {
            // TRADES VEILLE en POSITION et TRADES JOUR en POSITION
            // RD 20170308 [22660] Pour les trades du jour, ne pas considérer les actions du jour suivantes: 'CLEAREOD','CLEARBULK','UPDENTRY','ENTRY'
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.IDI, tr.IDM, tr.IDASSET, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.TYPEPRICE, tr.PRICE, tr.ACCINTRATE,
            tr.DTBUSINESS, tr.SIDE, tr.IDA_DEALER, tr.IDB_DEALER, bd.IDA_ENTITY as IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, bc.IDA_ENTITY as IDA_ENTITYCLEARER, ev.IDE_EVENT,
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0)) as QTY
            from dbo.{0} alloc
            inner join dbo.TRADE tr on (tr.IDT = alloc.IDT)
            inner join dbo.BOOK bd on (bd.IDB = alloc.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = alloc.IDB_CLEARER)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{1}) and (ev.{2})
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER, true, true) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER, true, true) + @") pas on (pas.IDT = tr.IDT)
            where (tr.DTTRADE < @DTBUSINESS) and (tr.DTBUSINESS < @DTBUSINESS) and
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) > 0) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER, 0) = @IDB_CLEARER) and 
            (tr.IDA_DEALER = @IDA_DEALER) and (tr.IDB_DEALER = @IDB_DEALER)

            union 

            select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION as DTEXECUTION, tr.TZFACILITY, 
            tr.IDI, tr.IDM, tr.IDASSET, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.TYPEPRICE, tr.PRICE, tr.ACCINTRATE,
            tr.DTBUSINESS, tr.SIDE, tr.IDA_DEALER, tr.IDB_DEALER, bd.IDA_ENTITY as IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, bc.IDA_ENTITY as IDA_ENTITYCLEARER, ev.IDE_EVENT,
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0))
            from dbo.{0} alloc
            inner join dbo.TRADE tr on (tr.IDT = alloc.IDT)
            inner join dbo.BOOK bd on (bd.IDB = alloc.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = alloc.IDB_CLEARER)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{1}) and (ev.{2})
            left outer join (" + PosKeepingTools.GetQryPosAction_Entry_BySide(BuyerSellerEnum.BUYER) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_Entry_BySide(BuyerSellerEnum.SELLER) + @") pas on (pas.IDT = tr.IDT)
            where (tr.DTBUSINESS = @DTBUSINESS) and
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) > 0) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER, 0) = @IDB_CLEARER) and 
            (tr.IDA_DEALER = @IDA_DEALER) and (tr.IDB_DEALER = @IDB_DEALER);" + Cst.CrLf;

            return String.Format(sqlSelect, pTableName, RESTRICT_EVENTCODE_POS, RESTRICT_EVENTTYPE_POS);
        }
        #endregion GetQueryPositionVeilAndTradeDay

        #region GetQueryPositionTradeOptionAndAction
        /// <summary>
        /// Utilisée par le traitement de dénouement d'options (manuel, automatique, manuel via importation par position)
        /// 1ere requête (GetQueryPositionTradeOption)
        ///      Retourne la négociation option et sa quantité en position
        /// 2eme requete (GetQueryPositionActionByTrade)
        ///      Retourne les actions sur position operées sur la négociation
        /// </summary>
        /// <param name="pIsExceptCurrentIdPR">Si = true : à l'exception de celle concernant le POSREQUEST en cours</param>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : New
        // EG 20180221 [23769] Public pour gestion asynchrone
        public virtual string GetQueryPositionTradeOptionAndAction(bool pIsExceptCurrentIdPR)
        {
            string sqlSelect = GetQueryPositionTradeOption(pIsExceptCurrentIdPR);
            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            sqlSelect += GetQueryPositionActionByTrade(pIsExceptCurrentIdPR);
            return sqlSelect;
        }
        #endregion GetQueryPositionTradeOptionAndAction
        #region GetQueryPositionTradeOption
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryPositionTradeOption(bool pIsExceptCurrentIdPR)
        {
            return null;
        }
        #endregion GetQueryPositionTradeOption

        #region GetQueryAllocMissing
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override for ETD
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected virtual string GetQueryAllocMissing()
        {
            string sqlSelect = SQLCst.SELECT + @"tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS, mk.IDENTIFIER, ac.IDENTIFIER, 
            buyer.FIXPARTYROLE, buyer.IDA_ENTITY, seller.FIXPARTYROLE, seller.IDA_ENTITY
            from dbo.TRADE tr
            inner join dbo.MARKET mk on (mk.IDM = isnull(tr.IDM, @IDM))
            inner join dbo.VW_INSTR_PRODUCT pr on (pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = @GPRODUCT)
            inner join dbo.ACTOR ac on (ac.IDA = tr.IDA_CSSCUSTODIAN)
            inner join 
            (
                select tr.IDT, isnull(ta.FIXPARTYROLE, '{0}') as FIXPARTYROLE, isnull(bk.IDA_ENTITY, @IDA) as IDA_ENTITY
                from dbo.TRADE tr
                left join dbo.TRADEACTOR ta on (ta.IDT = tr.IDT) and (ta.IDA = tr.IDA_BUYER) and (ta.BUYER_SELLER = 'Buyer')
                left join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER)
            ) buyer on (buyer.IDT = tr.IDT)

            inner join 
            (
                select tr.IDT, isnull(ta.FIXPARTYROLE, '{0}') as FIXPARTYROLE, isnull(bk.IDA_ENTITY, @IDA) as IDA_ENTITY
                from dbo.TRADE tr
                left join dbo.TRADEACTOR ta on (ta.IDT = tr.IDT) and (ta.IDA = tr.IDA_SELLER) and (ta.BUYER_SELLER = 'Seller')
                left join dbo.BOOK bk on (bk.IDB = tr.IDB_SELLER)
            ) seller on (seller.IDT = tr.IDT)

            inner join dbo.ENTITYMARKET em on (em.IDM = mk.IDM) and (em.IDA = case when buyer.FIXPARTYROLE = '{0}' then buyer.IDA_ENTITY else seller.IDA_ENTITY end)

            where (tr.DTBUSINESS = @DTBUSINESS) and (mk.IDM = @IDM) and (tr.IDSTBUSINESS = @STATUSALLOC) and (tr.IDSTACTIVATION = @STATUSMISSING)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_PARTYROLE_POS);
        }
        #endregion GetQueryAllocMissing
        #region GetQueryCandidatesToCashFlows
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToCashFlows(string pCS)
        {
            return string.Empty;
        }
        #endregion
        #region GetQueryCandidatesToClosingReopeningPosition
        /// <summary>
        /// Trades potentiellement candidats à Fermeture/Réouverture de positions
        /// = Trades en position (avant matchage contexte)
        /// voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20190308 New
        protected virtual string GetQueryCandidatesToClosingReopeningPosition(string pCS)
        {
            return string.Empty;
        }
        // EG 20190613 [24683] New
        protected virtual string GetQueryCandidatesToClosingReopeningPosition2(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToClosingReopeningPosition
        #region GetQueryCandidatesToClearing
        /// <summary>
        /// Utilisée par le traitement de clôture de masse et de fin de journée (CLEARINGBULK|CLEAREOD)
        /// Retourne les négociation candidates à clôture pour un marché et une date donnés
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // EG 20170109 Refactoring
        // EG 20180803 PERF (Utilisation d'une clause WITH (+ table "temporaire" sur la base d'un table MODEL pour SQLServer)
        // EG 20181010 PERF (Utilisation d'une table "temporaire" sur la base d'un table MODELE pour les 2 moteurs)
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (CLOSINGDAY only - augmentation performances)
        // EG 20201118 [24769] Correction bug sur offset row fetch next (manque order)
        protected virtual string GetQueryCandidatesToClearing(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("CLR");

            string sqlInsert = GetQueryClearingInsertSelect(tableName.First);
            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);

            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);

            string sqlSelect = "select 0 as SPID, ";
            if (DataHelper.IsDbSqlServer(pCS))
                sqlSelect = "select @@SPID as SPID, ";

            string fetchRestrictionForClosingDayControl = string.Empty;
            if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                fetchRestrictionForClosingDayControl = "order by alloc.IDASSET offset 0 rows fetch next 5 rows only";

            sqlSelect += @"alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) as QTY_BUY,
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) as QTY_SELL,
            alloc.ASSETCATEGORY, alloc.ISCUSTODIAN
            from dbo.{0} alloc
            {1}
            group by alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
                     alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, 
                     alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER, 
                     alloc.ASSETCATEGORY, alloc.ISCUSTODIAN
            having 
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) > 0
            and
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) > 0
            {2}" + Cst.CrLf;

            return String.Format(sqlSelect, tableName.First, GetQryPosActionBasedWithTrade(tableName.First, "alloc"), fetchRestrictionForClosingDayControl);
        }
        /// <summary>
        /// Construction de la query en fonction du type de produit (VOIR OVERRIDE)
        /// Query utilisée 
        /// - Oracle    : dans la clause WITH 
        /// - SqlServer : pour alimentation d'une table "temporaire" (copie d'un MODEL) qui sera utilisée dans la clause WITH
        /// </summary>
        // EG 20181010 PERF New Instruction d'alimentation de la table temporaire pour CLR
        // EG 20181119 PERF Correction post RC
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        protected virtual string GetQueryClearingInsertSelect(string pTableName)
        {
            //return  String.Format(@"insert into dbo.{0}
            //(IDT, IDI, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, QTY, 
            //IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER, ISCUSTODIAN)
            //select tr.IDT, tr.IDI, 
            //tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.QTY,
            //em.IDEM, em.IDA, em.DTENTITY, bd.IDA_ENTITY, bc.IDA_ENTITY, {1} as ISCUSTODIAN               
            //from dbo.TRADE tr
            //inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            //inner join dbo.PRODUCT p on ( p.IDP = ns.IDP) and (p.FUNGIBILITYMODE != 'NONE') and ({2})
            //inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            //inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            //inner join dbo.ENTITYMARKET em on (em.IDM = tr.IDM) and (em.IDA = bd.IDA_ENTITY) and ({3})
            //where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            //(tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')",
            //pTableName, RESTRICT_ISCUSTODIAN, RESTRICT_GPRODUCT_POS, RESTRICT_EMCUSTODIAN_POS);
            return String.Format(@"insert into dbo.{0}
            (IDT, IDI, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, QTY, 
            IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER, ISCUSTODIAN)
            select tr.IDT, tr.IDI, 
            tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.QTY,
            em.IDEM, em.IDA, em.DTENTITY, tr.IDA_ENTITY, bc.IDA_ENTITY, {1} as ISCUSTODIAN               
            from dbo.ENTITYMARKET em
            inner join dbo.TRADE tr on (tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT p on ( p.IDP = ns.IDP) and (p.FUNGIBILITYMODE != 'NONE') and ({2})
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')",
            pTableName, RESTRICT_ISCUSTODIAN, RESTRICT_GPRODUCT_POS);
        }
        #endregion GetQueryCandidatesToClearing
        #region GetQueryCandidatesToFeesCalculation
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// Utilisée par le traitement de fin de journée
        /// Retourne les négociation candidates à calcul des frais
        ///     - Tous les trades du jour sans frais alors qu'ils devraient exister (ABSENCE DE FRAIS en WARNING/BLOQUANT) en vue d'une application des conditions/barèmes actifs. 
        ///     - SAUF OUVERTURE DE POSITION (TRDTYPE != 1000)
        ///     - SAUF TRANSFERT (TRDTYPE != 42)
        ///     - SAUF LIVRAISON DE FUTURE (suite à exercice ou assignation) (TRDTYPE != 45)
        ///     - SAUF NOUVEAUX TRADES AJUSTES (suite à CA) (TRDTYPE != 1003)
        ///     - SAUF NOUVEAUX TRADES ISSUS DU CASCADING/SHIFTING (TRDTYPE != 1001 and TRDTYPE != 1002)
        ///     - SAUF NOUVEAUX TRADES TECHNIQUES ISSUS DE LA FERMETURE/REOUVERTURE DE POSITION (TRDTYPE != 63 and TRDTYPE != 1002)
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (CLOSINGDAY only - augmentation performances)
        // EG 20201118 [24769] Correction bug sur offset row fetch next (manque order)
        protected virtual string GetQueryCandidatesToFeesCalculation()
        {
            string fetchRestrictionForClosingDayControl = string.Empty;
            if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                fetchRestrictionForClosingDayControl = "order by tr.IDT offset 0 rows fetch next 5 rows only";

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDASSET, tr.QTY, ev.IDE as IDE_EVENT, 
            ta.IDA, ac.IDENTIFIER as ACTOR_IDENTIFIER,ta.IDB, ta.IDROLEACTOR, bk.IDENTIFIER as BOOK_IDENTIFIER, bk.VRFEE
            from dbo.{0} tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'TRD')
            inner join dbo.TRADEACTOR ta on (ta.IDT = tr.IDT)
            inner join dbo.BOOK bk on (bk.IDB = ta.IDB)
            inner join dbo.ACTOR ac on (ac.IDA = ta.IDA)
            left outer join EVENT ev2 on (ev2.IDT = tr.IDT) and (ev2.IDE_EVENT = ev.IDE) and (ev2.EVENTCODE = 'OPP') and ((ev2.IDB_PAY = ta.IDB) or (ev2.IDB_REC = ta.IDB))
            left outer join EVENTFEE evfee on (evfee.IDE = ev2.IDE)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS) and (tr.DTBUSINESS = tr.DTENTITY) and (isnull(bk.VRFEE,'None') != 'None') and (evfee.IDE is null)
            {1}" + Cst.CrLf;
            return String.Format(sqlSelect, VW_TRADE_POS, fetchRestrictionForClosingDayControl);
        }
        #endregion GetQueryCandidatesToFeesCalculation
        #region GetQueryCandidatesToManualDenouement
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToManualDenouement(PosKeepingTools.PosActionType pPosActionType, SettlSessIDEnum pSettlSessIDEnum)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToManualDenouement
        #region GetQueryCandidatesToMaturityOffsettingFuture
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToMaturityOffsettingFuture(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToMaturityOffsettingFuture
        #region GetQueryCandidatesToPhysicalPeriodicDelivery
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20170206 [22787] New
        protected virtual string GetQueryCandidatesToPhysicalPeriodicDelivery(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToPhysicalPeriodicDelivery
        #region GetQueryTradeCandidatesToPhysicalPeriodicDelivery
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20170206 [22787] New
        protected virtual QueryParameters GetQueryTradeCandidatesToPhysicalPeriodicDelivery(DateTime pDate, IPosKeepingKey pPosKeepingKey)
        {
            return null;
        }
        #endregion GetQueryTradeCandidatesToPhysicalPeriodicDelivery
        #region GetQueryCandidatesToMaturityOffsettingOption
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToMaturityOffsettingOption(string pCS)
        {
            return null;
        }
        #endregion GetQueryCandidatesToMaturityOffsettingOption

        #region GetQueryCandidatesToMerging
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring
        /// EG 20171016 [23509] Add TZFACILITY, DTEXECUTION
        protected virtual string GetQueryCandidatesToMerging()
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDI, tr.IDM, tr.IDASSET, 
            tr.IDA_ENTITY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, execbrk.IDA as IDA_EXECBROKER, 
            tr.DTBUSINESS, tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.SIDE, tr.IDSTACTIVATION, tr.IDSTENVIRONMENT, tr.POSITIONEFFECT, tr.PRICE, tr.QTY, 
            acg.IDGACTOR, bkg.IDGBOOK, ig.IDGINSTR, mg.IDGMARKET, {0}
            from dbo.{1} tr
            {2}
            left outer join dbo.TRADEACTOR execbrk on (execbrk.IDT = tr.IDT) and (execbrk.IDROLEACTOR='BROKER') and (execbrk.FIXPARTYROLE = '1')
            {3}
            left outer join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = 'AIN') and (ev.EVENTCODE = 'LPP')
            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = tr.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = tr.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acg on (acg.IDA = tr.IDA_DEALER) and (acg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkg on (bkg.IDB = tr.IDB_DEALER) and (bkg.IDROLEGBOOK = @ROLE)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS) and (isnull(tr.TRDTYPE,'0') not in ('1004','1005')) and (tr.IDSTENVIRONMENT = @IDSTENVIRONMENT)" + Cst.CrLf;
            return String.Format(sqlSelect, RESTRICT_COLMERGE_POS, VW_TRADE_POS, RESTRICT_ASSET_POS, RESTRICT_EVENTMERGE_POS);

        }
        #endregion GetQueryCandidatesToMerging
        #region GetQueryCandidatesToPositionCascading
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToPositionCascading(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToPositionCascading
        #region GetQueryCandidatesToPositionShifting
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToPositionShifting(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToPositionShifting
        #region GetQueryCandidatesToSafekeeping
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToSafekeeping(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToSafekeeping
        #region GetQueryCandidatesToUnderlyerDelivery
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToUnderlyerDelivery()
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToUnderlyerDelivery
        #region GetQueryCandidatesToUpdateEntry
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        ///  Utilisé par le traitement de mise à jour des clôtures au sein du traitement de fin de journée
        ///    Retourne les séries (clés de position) candidates à clôture
        ///    ENTITY|CSS ou CUSTODIAN|IDEM|IDI|IDASSET|ASSETCATEGORY|DTENTITY|DEALER|CLEARER
        /// </summary>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring
        /// RD 20170310 [22944] Refactoring
        /// FI 20170420 [23075] Modify  
        /// EG 20170529 [23189] Query refactoring (Oracle)
        /// EG 20171016 [23509] Add DTEXECUTION, TZFACILITY, DTEXECUTION remplace DTTIMESTAMP dans Restriction sur Query
        // EG 20181010 PERF Refactoring complet
        // EG 20181022 PERF Remove IDEM parameter for Second Insert into
        // EG 20181119 PERF Correction post RC
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190613 [24683] Save Table temporaire dans m_WorkTableUpdateEntry pour réutilisation
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        protected virtual string GetQueryCandidatesToUpdateEntry(IDbTransaction pDbTransaction, DateTime pDate, int pIdEM)
        {
            // RD 20170310 [22944] Chargement de la clé de Position:
            // 1 - Des trades jours en Position avec trades:
            // - précédents (jour ou veille) 
            // - en position 
            // - en sens inverse
            // 2 - Des trades jours en Position avec trades:
            // - jours postérieurs  
            // - ayant subi des clôtures.
            // - dans le même sens
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), pIdEM);

            Pair<string, bool> tableName1 = Initialize_QueryTradeEODModel("UE");
            m_WorkTableUpdateEntry = tableName1.First;
            string tableName2 = StrFunc.AppendFormat("UE_{0}{1}_{2}_W", pIdEM.ToString(), DtFunc.DateTimeToStringyyyyMMdd(pDate), ProcessBase.Session.BuildTableId()).ToUpper();
            bool isNoHints = DataHelper.GetSvrInfoConnection(CS).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(CS).IsNoHints;
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(tr IX_TRADE5) index(bd IX_BOOK2) index(ti PK_TRADEINSTRUMENT) */";

            string sqlInsert = String.Format(@"insert into dbo.{0}
            (IDT, IDENTIFIER, IDA_CSSCUSTODIAN, IDEM, IDI, IDM, IDASSET, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, SIDE, DTTIMESTAMP, DTEXECUTION, DTBUSINESS, ASSETCATEGORY, QTY, PRICE, ISCUSTODIAN)
            select {3}
            tr.IDT, tr.IDENTIFIER, tr.IDA_CSSCUSTODIAN, em.IDEM, tr.IDI, tr.IDM, tr.IDASSET, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, 
            tr.SIDE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.DTBUSINESS, tr.ASSETCATEGORY, tr.QTY, tr.PRICE, {1} as ISCUSTODIAN
            from dbo.ENTITYMARKET em
            inner join dbo.TRADE tr on (tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT p on ( p.IDP = ns.IDP) and (p.FUNGIBILITYMODE != 'NONE') and ({2})
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')",
            tableName1.First, RESTRICT_ISCUSTODIAN, RESTRICT_GPRODUCT_POS, hintsOracle);

            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, parameters);
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == tableName1.Second)
                Initialize_IndexTradeEODModel(tableName1.First);
            
            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(CS, tableName1.First);

            if (false == DataHelper.IsExistTable(CS, tableName2))
            {
                DataHelper.CreateTableAsSelect(CS, "UPDENTRY_MODEL", tableName2);
            }
            else
            {
                if (DbSvrType.dbSQL == serverType)
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("truncate table dbo.{0}", tableName2));
                else
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("delete from dbo.{0}", tableName2));
            }

            sqlInsert = String.Format(@"insert into dbo.{0}
            (IDA_CSSCUSTODIAN, IDEM, IDI, IDASSET, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, SIDE, DTTIMESTAMP, DTEXECUTION, DTBUSINESS, ASSETCATEGORY) 
			select distinct alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, 
            alloc.SIDE, isnull(alloc.DTTIMESTAMP,alloc.DTEXECUTION), alloc.DTEXECUTION, alloc.DTBUSINESS, alloc.ASSETCATEGORY  
            from dbo.{1} alloc
            
            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = alloc.IDT)
            where (alloc.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0) > 0)", tableName2, tableName1.First);


            parameters.Clear();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date

            qryParameters = new QueryParameters(CS, sqlInsert, parameters);
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return StrFunc.AppendFormat(@"select distinct bd.IDA_ENTITY as IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDI, tr.IDASSET, tr.IDEM, em.DTENTITY,
            tr.IDA_DEALER, tr.IDB_DEALER, bd.IDA_ENTITY as IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, bc.IDA_ENTITY as IDA_ENTITYCLEARER, 
            tr.ASSETCATEGORY, {1} as ISCUSTODIAN
            from {0} tr 
            inner join {0} tr2 on (tr2.IDA_CSSCUSTODIAN = tr.IDA_CSSCUSTODIAN) and (tr2.IDEM = tr.IDEM) and 
            (tr2.IDI = tr.IDI) and (tr2.IDASSET = tr.IDASSET) and (tr2.ASSETCATEGORY = tr.ASSETCATEGORY) and
            (tr2.IDA_DEALER = tr.IDA_DEALER) and (tr2.IDB_DEALER = tr.IDB_DEALER) and
            (tr2.IDA_CLEARER = tr.IDA_CLEARER) and (tr2.IDB_CLEARER = tr.IDB_CLEARER)  and
            (tr2.SIDE <> tr.SIDE) and (tr2.DTEXECUTION <= tr.DTEXECUTION)
            inner join dbo.ENTITYMARKET em on (em.IDEM = @IDEM)
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            where (tr.DTBUSINESS = @DTBUSINESS)", tableName2, RESTRICT_ISCUSTODIAN) + Cst.CrLf;
        }
        #endregion GetQueryCandidatesToUpdateEntry
        #region GetQueryCandidatesToUTICalculation
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCandidatesToUTICalculation(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToUTICalculation
        #region GetQueryCascadingContract
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryCascadingContract()
        {
            return null;
        }
        #endregion GetQueryCascadingContract

        #region GetQueryDigressiveFees
        /// <summary>
        /// Sélection de tous les trades du JOUR s'il existe au moins un trade calculé avec barème dégressif
        /// </summary>
        /// <returns></returns>
        protected virtual string GetQueryDigressiveFees()
        {
            //PL 20170403 [23015]
            return @" select tr.IDT, tr.IDENTIFIER, tr.IDASSET, tr.QTY, ev.IDE as IDE_EVENT, 
            ta.IDA, ac.IDENTIFIER as ACTOR_IDENTIFIER,ta.IDB, ta.IDROLEACTOR, bk.IDENTIFIER as BOOK_IDENTIFIER, isnull(bk.VRFEE, 'None') as VRFEE
            from dbo." + VW_TRADE_POS + @" tr 
            inner join dbo.TRADEACTOR ta on (ta.IDT = tr.IDT)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'TRD')
            inner join dbo.EVENT ev2 on (ev2.IDT = tr.IDT) and (ev2.EVENTCODE = 'OPP') and ((ev2.IDB_PAY = ta.IDB) or (ev2.IDB_REC = ta.IDB))
            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev2.IDE) and (evfee.ASSESSMENTBASISDET is not null)
            inner join dbo.BOOK bk on (bk.IDB = ta.IDB)
            inner join dbo.ACTOR ac on (ac.IDA = ta.IDA)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS) and (tr.DTBUSINESS = tr.DTENTITY) and (isnull(tr.TRDTYPE,0) not in (" + Cst.TrdType_ExcludedValuesForFees_ETD + "))";
        }
        #endregion GetQueryDigressiveFees

        #region GetQueryEventMissing
        /// <summary>
        /// Retourne les trades "allocation" du jour sans événements générés
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20170109 Refactoring
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected virtual string GetQueryEventMissing()
        {
            return @"select tr.IDT, tr.IDENTIFIER, tr.QTY
            from dbo." + VW_TRADE_FUNGIBLE_LIGHT + @" tr 
            where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDEM = @IDEM) and not exists (select 1 from dbo.EVENT ev where (ev.IDT = tr.IDT))" + Cst.CrLf;
        }
        #endregion GetQueryEventMissing
        #region GetQueryExistDigressiveFees
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Upd and Add NOLOCK
        protected virtual string GetQueryExistDigressiveFees()
        {
            //PL 20170403 [23015]
            return @"select 1
            from dbo." + VW_TRADE_POS + @" tr 
            inner join dbo.EVENT ev " + DataHelper.SQLNOLOCK(CS) + @" on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'OPP') and 
            ((ev.IDB_PAY = tr.IDB_DEALER) or (ev.IDB_PAY = tr.IDB_CLEARER) or (ev.IDB_REC = tr.IDB_DEALER) or (ev.IDB_REC = tr.IDB_CLEARER))
            inner join dbo.EVENTFEE evfee " + DataHelper.SQLNOLOCK(CS) + @" on (evfee.IDE = ev.IDE) and (evfee.ASSESSMENTBASISDET is not null)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS) and (isnull(tr.TRDTYPE,'0') not in (" + Cst.TrdType_ExcludedValuesForFees_ETD + "))";
        }
        #endregion GetQueryExistDigressiveFees

        #region GetQueryInsertPriceControl_EOD
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryInsertPriceControl_EOD()
        {
            return null;
        }
        #endregion GetQueryInsertPriceControl_EOD
        #region GetQueryMandatoryPosKeep_Calc_Cond1
        /// <summary>
        /// Voir PosKeepingGen_XXX - Override
        /// Utilisé dans le traitement de mise à jour des clôtures en mode STP (Entry) 
        ///     Retourne la requête qui détermine si:
        ///     Le trade a pris part à une clôture/compensation valide et postérieure à son entrée en portefeuille.
        ///     NB: Un traitement de tenue de position traite tous les trades d'une même série. 
        ///     Ce cas de figure peut se produire lors d'une arrivée en masse de trade et où le msg d'un Close serait traité avant celui d'un Open.
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        /// EG 20171016 [23509] DTEXECUTION remplace DTTIMESTAMP dans Restriction sur Query
        protected virtual string GetQueryMandatoryPosKeep_Calc_Cond1()
        {
            return @"select 1
            from dbo." + VW_TRADE_POS + @" tr
            inner join dbo.POSACTIONDET pad on ((pad.IDT_BUY = tr.IDT) or (pad.IDT_SELL = tr.IDT)) and 
            ((pad.DTCAN is null) or (pad.DTCAN > tr.DTEXECUTION)) and (pad.DTINS >= tr.DTINS)
            where (tr.IDT = @IDT)" + Cst.CrLf;
        }
        #endregion GetQueryMandatoryPosKeep_Calc_Cond1
        #region GetQueryMandatoryPosKeep_Calc_Cond2
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryMandatoryPosKeep_Calc_Cond2()
        {
            return null;
        }
        #endregion GetQueryMandatoryPosKeep_Calc_Cond2
        #region GetQueryNewTradeAfterProcessEndOfDayInSuccess
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        ///  Utilisé dans le traitement de clôture de journée (Etape de contrôle)
        ///  Retourne les trades  "allocation" du jour créés APRES le DERNIER TRAITEMENT DE FIN DE JOURNEE
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // RD 20180615 [24027] Modify
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (augmentation performances)
        protected virtual string GetQueryNewTradeAfterProcessEndOfDayInSuccess(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.QTY
            from dbo." + VW_TRADE_POS + @" tr
            inner join dbo.TRADETRAIL tr_l on (tr_l.IDT = tr.IDT) and (tr_l.ACTION = 'Create') and ({0})
            inner join 
            (
                select pr.DTUPD, pr.DTINS, pr.IDEM,pr.DTBUSINESS
                from dbo.POSREQUEST pr
                where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.STATUS in ('SUCCESS','WARNING')) and
                (pr.DTUPD = (select max(prmax.DTUPD) 
                             from dbo.POSREQUEST prmax 
                             where (prmax.REQUESTTYPE = @REQUESTTYPE) and (prmax.IDEM = @IDEM) and (prmax.DTBUSINESS = @DTBUSINESS)))
            ) pr on (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.DTINS < tr_l.DTSYS)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS)
            order by tr.IDT
            offset 0 rows fetch next 5 rows only" + Cst.CrLf;

            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
                sqlSelect = String.Format(sqlSelect, @"charindex('SpheresClosingGen', tr_l.APPNAME) = 0");
            else if (DbSvrType.dbORA == serverType)
                sqlSelect = String.Format(sqlSelect, @"instr(tr_l.APPNAME,'SpheresClosingGen') = 0");
            return sqlSelect;

        }
        #endregion GetQueryNewTradeAfterProcessEndOfDayInSuccess

        #region GetQueryNewRMVAllocAfterProcessEndOfDayInSuccess
        /// <summary>
        /// Retourne le equête qui vérifie l'existence d'un trade annulé post génération du dernier traitement EOD réalisé avec succès
        /// </summary>
        /// <returns></returns>
        /// FI 20160819 [22364] Add 
        /// RD 20180615 [24027] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (augmentation performances)
        protected virtual string GetQueryNewRMVAllocAfterProcessEndOfDayInSuccess()
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.QTY
            from dbo.TRADE tr
			inner join dbo.VW_INSTR_PRODUCT p on ( p.IDI = tr.IDI) and (p.FUNGIBILITYMODE != 'NONE') and ({0})
			inner join dbo.ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = tr.IDA_ENTITY) and ({1})
			inner join dbo.POSREQUEST prRmv on (prRmv.IDT = tr.IDT)  and (prRmv.REQUESTTYPE  = 'RMVALLOC') and (prRmv.STATUS = 'SUCCESS') 
            inner join 
            (
                select pr.DTUPD, pr.DTINS, pr.IDEM,pr.DTBUSINESS
                from dbo.POSREQUEST pr
                where (pr.REQUESTTYPE = @REQUESTTYPE) and (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.STATUS in ('SUCCESS','WARNING')) and
                (pr.DTUPD = (select max(prmax.DTUPD) 
                             from dbo.POSREQUEST prmax 
                             where (prmax.REQUESTTYPE = @REQUESTTYPE) and (prmax.IDEM = @IDEM) and (prmax.DTBUSINESS = @DTBUSINESS)))
            ) pr on (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.DTINS < prRmv.DTUPD)
            where (em.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDSTACTIVATION = 'DEACTIV') and (tr.IDSTBUSINESS = 'ALLOC')
            order by tr.IDT
            offset 0 rows fetch next 5 rows only" + Cst.CrLf;
            return String.Format(sqlSelect, RESTRICT_GPRODUCT_POS, RESTRICT_EMCUSTODIAN_POS);

        }
        #endregion GetQueryNewRMVAllocAfterProcessEndOfDayInSuccess

        #region GetQryPosActionBasedOnWith
        /// <summary>
        /// Construction de la sous-query POSACTIONDET (BUY|SELL) de base
        /// pour une utilisation avec une table de trades candidats (table temporaire)
        // EG 20181010 PERF Upd (Use Temporary based to model table, DTOUT, Union all)
        // EG 20181119 PERF Correction post RC
        protected virtual string GetQryPosActionBasedWithTrade(string pTableTradeCandidates, string pMainAlias)
        {
            string sqlQuery = @"
            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.{0} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.{0} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = {1}.IDT)";
            return String.Format(sqlQuery, pTableTradeCandidates, pMainAlias);
        }
        #endregion GetQryPosActionBasedOnWith

        #region GetQrySettlementPosAction
        /// <summary>
        /// Construction de la sous-query POSACTIONDET (BUY|SELL) avec DtSettlt
        /// pour une utilisation avec une table de trades candidats (Table temporaire)
        /// </summary>
        // EG 20180828 PERF New (Use by SKP)
        // EG 20180906 PERF Upd (Use With instruction and Temporary based to model table)
        // EG 20181119 PERF Correction post RC
        // EG 20190308 [VCL migration] Correction Query
        // EG 20190926 Rename Method instead of GetQrySafeKeepingPosAction
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected virtual string GetQrySettlementPosAction(string pTableTradeCandidates, string pMainAlias)
        {
            string sqlQuery = @"
            left outer join
            (
	            select pad.IDT_BUY as IDT,
                sum(case when isnull(tr.DTSETTLT,@DTBUSINESS) <= @DTBUSINESS then isnull(pad.QTY,0) else 0 end) as QTY_BUY, 0 as QTY_SELL
	            from dbo.{0} alloc 
	            inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_SELL) 
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY

	            union all

	            select pad.IDT_SELL as IDT,
	            0 as QTY_BUY, sum(case when isnull(tr.DTSETTLT,@DTBUSINESS) <= @DTBUSINESS  then isnull(pad.QTY,0) else 0 end) as QTY_SELL
	            from dbo.{0} alloc 
	            inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_BUY) 
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL

            ) pos on (pos.IDT = {1}.IDT)";
            return String.Format(sqlQuery, pTableTradeCandidates, pMainAlias);
        }
        #endregion GetQrySettlementPosAction
        #region GetQrySafekeepingInsertSelect
        // EG 20181010 PERF New Instruction d'alimentation de la table temporaire pour SKP
        // EG 20190301 [VCL migration] Correction Query
        // EG 20190308 [VCL_Migration] Add instr.ISSAFEKEEPING
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected virtual string GetQrySafekeepingInsertSelect()
        {
            return @"insert into dbo.{0}
            (IDT, IDENTIFIER, IDI, IDASSET, ASSETCATEGORY, QTY, IDE_EVENT)
            select tr.IDT, tr.IDENTIFIER, tr.IDI, tr.IDASSET, tr.ASSETCATEGORY, tr.QTY, evp.IDE
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) and (ns.ISSAFEKEEPING = 1)
            inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = '{1}')
            inner join dbo.EVENT evp on (evp.IDT = tr.IDT) and (evp.EVENTCODE = 'LPC') and (evp.EVENTTYPE = 'AMT')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN = tr.IDA_CSSCUSTODIAN)
            where (em.IDEM = @IDEM) and (@DTBUSINESS between isnull(tr.DTSETTLT, tr.DTBUSINESS) and tr.DTOUTADJ) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')";
        }
        #endregion GetQrySafekeepingInsertSelect
        #region GetQrySafekeepingSelect
        // EG 20180828 PERF New (Use by SKP)
        // EG 20181010 PERF Upd
        // EG 20190308 [VCL_Migration] Correction Query
        // EG 20190328 [VCL_Migration] Correction Query QTY + MKV
        // EG 20190716 [FIXED INCOME] lecture des données ayant servi au calcul du MKV quotidien dans les cashflows (MKV amount, DAILYQUANTITY) pour prorater avec la quantité en DTSETTLEMENT
        // EG 20190926 [Maturity redemption] Add test sur MATURITYDATE
        protected virtual string GetQrySafekeepingSelect()
        {
            string sqlSelect = "select 0 as SPID, ";
            if (DataHelper.IsDbSqlServer(CS))
                sqlSelect = "select @@SPID as SPID, ";

            sqlSelect += @"alloc.IDT, alloc.IDENTIFIER as TRADE_IDENTIFIER, alloc.IDASSET, alloc.ASSETCATEGORY, alloc.IDE_EVENT,
            ass.IDENTIFIER as ASSET_IDENTIFIER, ev_skp.IDE as IDE_SKP, alloc.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0) as QTY,
            ev_mkv.VALORISATION as BIZAMOUNT_MKV,  ev_mkv.UNIT as BIZCUR_MKV, ev_mkv.DAILYQUANTITY as BIZQTY_MKV, 
            ev_mkv.ASSETMEASURE as ASSETMEASURE_MKV, ev_mkv.QUOTEPRICE as QUOTEPRICE_MKV, ev_mkv.QUOTEPRICE100 as QUOTEPRICE100_MKV, 
            ev_mkv.NOTIONALREFERENCE as BIZNOTIONALREF_MKV, ev_mkv.RATE as ACCRUEDRATE_MKV
            from {0} alloc
            {1}

            left outer join 
            ( 
                select ev.IDT, ev.IDE, ev.IDE_EVENT, ev.VALORISATION, ev.UNIT, ed.DAILYQUANTITY, ed.ASSETMEASURE, ed.QUOTEPRICE, ed.QUOTEPRICE100, ed.NOTIONALREFERENCE, ed.RATE
                from dbo.EVENT ev
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = 'REC')
                inner join dbo.EVENTDET ed on (ed.IDE = ev.IDE)
                where (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = 'MKV')
            ) ev_mkv on (ev_mkv.IDT = alloc.IDT) and (ev_mkv.IDE_EVENT = alloc.IDE_EVENT)

            left outer join 
            ( 
                select ev.IDT, ev.IDE, ev.IDE_EVENT, ev.VALORISATION, ev.UNIT
                from dbo.EVENT ev
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = 'REC')
                where (ev.EVENTCODE = 'SKP')
            ) ev_skp on (ev_skp.IDT = alloc.IDT) and (ev_skp.IDE_EVENT = alloc.IDE_EVENT)

            inner join 
            (
                {2}
            ) ass on (ass.IDASSET = alloc.IDASSET) and (ass.ASSETCATEGORY = alloc.ASSETCATEGORY) and ((ass.MATURITYDATE is null) or (@DTBUSINESS < ass.MATURITYDATE))
            where (alloc.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0) > 0) or (ev_skp.IDE is not null)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQrySafekeepingSelect
        #region GetQrySafekeepingAsset
        // EG 20180828 PERF New (Use by SKP)
        protected virtual string GetQrySafekeepingAsset()
        {
            return string.Empty;
        }
        #endregion GetQrySafekeepingAsset

        #region GetQueryPosKeepingData
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        ///  Utilisée par les traitements OTC de tenue de position pour :
        ///  Recupérer pour alimenter les données de travail pour les traitements clé de position, actif, ...
        /// </summary>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring
        /// EG 20171016 [23509] Add DTEXECUTION, TZFACILITY
        /// RD 20230413 [26296] For RemoveAllocation, use VW_TRADE_ALLOC to include non-fungible instruments
        protected virtual string GetQueryPosKeepingData(Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            string sqlSelect;
            if (IsPosKeepingByTrade)
            {
                sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.SIDE, tr.DTBUSINESS, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
                tr.POSITIONEFFECT, tr.IDI, tr.IDM, tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDEM, tr.DTMARKET, tr.DTMARKETPREV, tr.DTMARKETNEXT, 
                tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, 
                tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, 
                tr.POSKEEPBOOK_DEALER, tr.QTY, 
                tr.IDSTACTIVATION, tr.IDSTBUSINESS, tr.ASSETCATEGORY, 
                ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, tr.DTSETTLT 
                from dbo." + (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.RemoveAllocation ? "VW_TRADE_ALLOC" : VW_TRADE_FUNGIBLE) + @" tr 
                inner join dbo.VW_ASSET ast on (ast.IDASSET = tr.IDASSET) and (ast.ASSETCATEGORY = tr.ASSETCATEGORY) 
                where (tr.IDT = @ID)" + Cst.CrLf;
            }
            else
            {
                sqlSelect = @"select ast.IDASSET,ast.IDENTIFIER as ASSET_IDENTIFIER
                from dbo.VW_ASSET ast
                where (ast.IDASSET = @ID)" + Cst.CrLf;
            }
            return sqlSelect;

        }
        #endregion GetQueryPosKeepingData
        #region GetQueryPosKeepingDataAsset
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryPosKeepingDataAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            return null;
        }
        #endregion GetQueryPosKeepingDataAsset
        #region GetQueryPosKeepingDataTrade
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring
        /// EG 20171016 [23509] Add DTEXECUTION, TZFACILITY, DTEXECUTION remplace DTTIMESTAMP dans Restriction sur Query
        protected virtual string GetQueryPosKeepingDataTrade(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.SIDE, tr.DTBUSINESS, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.POSITIONEFFECT, tr.IDI, tr.IDM, tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDEM, tr.DTMARKET, tr.DTMARKETPREV, tr.DTMARKETNEXT,
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, 
            tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, 
            tr.POSKEEPBOOK_DEALER, tr.QTY, tr.IDSTACTIVATION, tr.IDSTBUSINESS, tr.ASSETCATEGORY,
            ast.IDASSET, ast.IDENTIFIER  as ASSET_IDENTIFIER, 
            ast.IDC, isnull (ast.IDC_ISSUE,ast.IDC) as NOMINALCURRENCY, 
            isnull (ast.IDC_STRIKE,ast.IDC) as PRICECURRENCY, ast.STRIKEPRICE, 1 as CONTRACTMULTIPLIER, mk.IDBC, tr.DTSETTLT 
            from dbo." + VW_TRADE_FUNGIBLE + " tr" + Cst.CrLf;

            if (pUnderlyingAsset.HasValue)
            {

                switch (pUnderlyingAsset.Value)
                {
                    case Cst.UnderlyingAsset.EquityAsset:
                        sqlSelect += "inner join dbo.ASSET_EQUITY ast on (ast.IDASSET = tr.IDASSET)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.Index:
                        sqlSelect += "inner join dbo.ASSET_INDEX ast on (ast.IDASSET = tr.IDASSET)" + Cst.CrLf;
                        break;
                    default:
                        throw new NotSupportedException("UnderlyingAsset category [" + pUnderlyingAsset.Value.ToString() + "] not managed");
                }

                sqlSelect += "inner join dbo.MARKET mk on (mk.IDM = ast.IDM)" + Cst.CrLf;
                sqlSelect += "where (tr.IDT = @ID)";
            }
            else
                throw new NotSupportedException("UnderlyingAsset category not specified");

            return sqlSelect;

        }
        #endregion GetQueryPosKeepingDataTrade

        #region GetQuerySelectPriceControl_EOD
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQuerySelectPriceControl_EOD()
        {
            return null;
        }
        #endregion GetQuerySelectPriceControl_EOD
        #region GetQueryShiftingContract
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryShiftingContract()
        {
            return null;
        }
        #endregion GetQueryShiftingContract

        #region GetQueryTradeCandidatesToMaturityOffsettingFuture
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual QueryParameters GetQueryTradeCandidatesToMaturityOffsettingFuture(DateTime pDate, IPosKeepingKey pPosKeepingKey)
        {
            return null;
        }
        #endregion GetQueryTradeCandidatesToMaturityOffsettingFuture
        #region GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity
        /// <summary>
        /// Lecture des titres remboursables à l'échéance
        /// </summary>
        // EG 20190926 [Maturity redemption] New
        protected virtual QueryParameters GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity(DateTime pDate, IPosKeepingKey pPosKeepingKey)
        {
            return null;
        }
        #endregion GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity

        #region GetQueryUnclearingPosactionDet
        /// <summary>
        /// Utilisée pour la décompensation : Retourne l'ensemble des données relatives un POSACTIONDET donné (IDPADET)
        /// 1ere requête : Ligne de POSACTION parent de la ligne de POSACTIONDET à décompenser
        /// 2eme requete : Ligne de POSACTIONDET à décompenser
        /// 3eme requete : Lignes de EVENTPOSACTIONDET de la négociation CLOTUREE à décompenser
        /// 4eme requete : Lignes de EVENTPOSACTIONDET de la négociation CLOTURANTE à décompenser
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        protected virtual string GetQueryUnclearingPosactionDet()
        {
            string sqlSelect = @"select pa.IDPA, pa.IDPR, pa.DTBUSINESS, pa.DTINS, pa.IDAINS
            from dbo.POSACTION pa
            inner join POSACTIONDET pad on (pad.IDPA = pa.IDPA) and (pad.IDPADET = @IDPADET)";
            if (DataHelper.IsDbOracle(CS))
                sqlSelect += @" and (@IDT_CLOSED = @IDT_CLOSED) and (@IDT_CLOSING = @IDT_CLOSING)" + Cst.CrLf;

            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;

            sqlSelect += @"select pad.IDPA, pad.IDPADET, pad.QTY, pad.POSITIONEFFECT, pad.DTCAN, pad.IDACAN, pad.CANDESCRIPTION,
            pad.IDT_BUY, pad.IDT_SELL, pad.IDT_CLOSING, pad.DTINS, pad.IDAINS, pad.DTUPD, pad.IDAUPD,
            pa.IDPR, pa.DTBUSINESS
            from dbo.POSACTIONDET pad
            inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) and (pad.DTCAN is null)
            where (pad.IDPADET = @IDPADET)";
            if (DataHelper.IsDbOracle(CS))
                sqlSelect += @" and (@IDT_CLOSED = @IDT_CLOSED) and (@IDT_CLOSING = @IDT_CLOSING)" + Cst.CrLf;

            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;

            sqlSelect += @"select epad.IDE, epad.IDPADET, ev.IDT, ev.EVENTCODE, ev.EVENTTYPE, tr.DTTRADE, tr.SIDE, tr.PRICE
            from dbo.EVENTPOSACTIONDET epad
            inner join dbo.EVENT ev on (ev.IDE = epad.IDE)
            inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET)
            inner join dbo." + VW_TRADE_POS + @" tr on (tr.IDT = ev.IDT)and (pad.DTCAN is null) and (ev.IDT = @IDT_CLOSED)
            where (epad.IDPADET = @IDPADET)";

            if (DataHelper.IsDbOracle(CS))
                sqlSelect += @" and (@IDT_CLOSING = @IDT_CLOSING)" + Cst.CrLf;

            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;

            sqlSelect += @"select epad.IDE, epad.IDPADET, ev.IDT, ev.EVENTCODE, ev.EVENTTYPE, tr.DTTRADE, tr.SIDE, tr.PRICE
            from dbo.EVENTPOSACTIONDET epad
            inner join dbo.EVENT ev on (ev.IDE = epad.IDE)
            inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET)
            inner join dbo." + VW_TRADE_POS + @" tr on (tr.IDT = ev.IDT)and (pad.DTCAN is null) and (ev.IDT = @IDT_CLOSING)
            where (epad.IDPADET = @IDPADET)";

            if (DataHelper.IsDbOracle(CS))
                sqlSelect += @" and (@IDT_CLOSED = @IDT_CLOSED)" + Cst.CrLf;

            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;
            return sqlSelect;
        }
        #endregion GetQueryUnclearingPosactionDet
        #region GetQueryUnclearingPosactionDetByTrade
        /// <summary>
        /// Utilisée pour la décompensation : Retourne l'ensemble des données à décompenser pour une négociation donnée (IDT)
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        // EG 20170412 [23081] Add pr.GPRODUCT
        protected virtual string GetQueryUnclearingPosactionDetByTrade()
        {
            // FI 20160517 [22172] utilisation de l'étiquette REQUESTTYPE_VALUE et mise en forme du case (colonne IDT)
            string sqlSelect = @"select pad.QTY, pad.QTY as UNCLEARINGQTY, pad.POSITIONEFFECT, pa.DTBUSINESS, pad.IDPADET,
            pad.IDT_CLOSING, tr.IDENTIFIER as TR_CLOSING_IDENTIFIER,
            case when (pad.IDT_BUY is null) then pad.IDT_SELL 
                 else
                    case when (pad.IDT_SELL is null) then pad.IDT_BUY 
                    else
                        case when (pad.IDT_BUY=pad.IDT_CLOSING) then pad.IDT_SELL 
                        else
                            case when (pad.IDT_SELL=pad.IDT_CLOSING) then pad.IDT_BUY 
                            end 
                        end 
                    end 
            end as IDT,
            tr.IDI, tr.IDASSET, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER,
            tr.IDA_ENTITYDEALER, tr.IDA_ENTITYCLEARER, tr.DTMARKET, tr.DTENTITY, pr.IDPR, pr.REQUESTTYPE as REQUESTTYPE_VALUE, pr.IDEM,
            pr.IDA_ENTITY, pr.IDA_CSS, pr.IDA_CUSTODIAN, pr.GPRODUCT
            from dbo.POSACTIONDET pad
            inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
            inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
            inner join dbo." + VW_TRADE_POS + @" tr on (tr.IDT = pad.IDT_CLOSING)
            where ((pad.IDT_BUY = @IDT) or (pad.IDT_SELL = @IDT)) and (pad.DTCAN is null)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryUnclearingPosactionDetByTrade

        /// <summary>
        /// Construction dynamique d'une table de TRADE en position en fonction du traitement demandée
        /// Utilisé par UE, CLR, SKP, MOO, MOF, PDP
        /// </summary>
        // EG 20181010 PERF New Création d'une table temporaire sur la base d'une table MODELE (TRADE_EOD_MODEL)
        protected virtual Pair<string, bool> Initialize_QueryTradeEODModel(string pPrefix)
        {
            Pair<string, bool> ret = new Pair<string, bool>();
            string tableName = StrFunc.AppendFormat("TRADE_{0}_{1}_W", pPrefix, PKGenProcess.Session.BuildTableId()).ToUpper();
            ret.First = tableName;
            ret.Second = DataHelper.IsExistTable(CS, tableName);

            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            if (ret.Second)
            {
                if (DbSvrType.dbSQL == serverType)
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("truncate table dbo.{0}", tableName));
                else
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("delete from dbo.{0}", tableName));
            }
            else
            {
                DataHelper.CreateTableAsSelect(CS, "TRADE_EOD_MODEL", tableName);
            }
            return ret;
        }
        // EG 20181010 PERF New Création d'un index associé à la table temporaire (post Insert/Select)
        protected virtual void Initialize_IndexTradeEODModel(string pTableName)
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            // FI 20190701 [24520] IX à la place de UX
            if (DbSvrType.dbSQL == serverType)
                DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("create clustered index IX_{0} on dbo.{0} (IDT)", pTableName));
            else if (DbSvrType.dbORA == serverType)
                DataHelper.ExecuteNonQuery(CS, CommandType.Text, String.Format("create index IX_{0} on dbo.{0} (IDT)", pTableName));
        }
    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // O T C : ReturnSwap (Cfd)
    //────────────────────────────────────────────────────────────────────────────────────────────────

    public partial class PosKeepingGen_OTC : PosKeepingGenProcessBase
    {
        #region GetQueryCandidatesToCashFlows
        /// <summary>
        /// Retourne les négociation en position en date veille de DTBUSINESS candidates à calcul des Cash-Flows
        /// PS : QTY = position jour
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20141224 [20566] New
        // EG 20150219 Lesture des trades en position veille
        // EG 20150305 Exclusion des TRADES échus (DTOUTADJ > DTBUSINESS)
        // RD 20160805 [22415] Add parameter pIsDtSettlt = true for method GetQueryPositionActionBySide
        // EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_DtSettlt_BySide
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override string GetQueryCandidatesToCashFlows(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDASSET, ass.IDENTIFIER as ASSET_IDENTIFIER, tr.ASSETCATEGORY, 
            (tr.QTY - isnull(pabj.QTY,0) - isnull(pasj.QTY, 0)) as QTY
            from dbo." + VW_TRADE_POS + @" tr
            inner join dbo.VW_ASSET ass on (ass.IDASSET = tr.IDASSET) and (ass.ASSETCATEGORY = tr.ASSETCATEGORY)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.BUYER, true) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.SELLER, true) + @") pas on (pas.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.BUYER, false) + @") pabj on (pabj.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.SELLER, false) + @") pasj on (pasj.IDT = tr.IDT)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS <= @DTBUSINESS) and (tr.POSKEEPBOOK_DEALER = 1) and (@DTBUSINESS <= tr.DTOUTADJ) and
            (tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0)>0)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToCashFlows
        #region GetQueryCandidatesToClosingReopeningPosition
        // EG 20190613 [24683] New
        // EG 20240520 [WI930] Upd
        protected override string GetQueryCandidatesToClosingReopeningPosition(string pCS)
        {
            string sqlSelect = @"select alloc.IDT, alloc.IDENTIFIER, alloc.IDI, alloc.DTBUSINESS, 
            alloc.IDM, alloc.ASSETCATEGORY, alloc.IDASSET, alloc.IDA_ENTITY, 
            alloc.IDA_DEALER, ad.IDENTIFIER as DEALER_IDENTIFIER, alloc.IDB_DEALER, bd.IDENTIFIER as BKDEALER_IDENTIFIER, 
            alloc.IDA_CLEARER, ac.IDENTIFIER as CLEARER_IDENTIFIER, alloc.IDB_CLEARER, bc.IDENTIFIER as BKCLEARER_IDENTIFIER, 
            tr.IDA_CSSCUSTODIAN, css.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,
            alloc.SIDE, alloc.PRICE, alloc.DTEXECUTION,
            acdg.IDGACTOR as IDGACTOR_DEALER , bkdg.IDGBOOK as IDGBOOK_DEALER, 
            accg.IDGACTOR as IDGACTOR_CLEARER, bkcg.IDGBOOK as IDGBOOK_CLEARER, 
            ig.IDGINSTR, mg.IDGMARKET,
            pr.IDP, pr.GPRODUCT, alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY,
            alloc.IDEM, ev.IDE as IDE_EVENT
            from dbo.{1} alloc
            inner join dbo.VW_INSTR_PRODUCT pr on (pr.IDI= alloc.IDI)
            inner join dbo.EVENT ev on (ev.IDT = alloc.IDT) and (ev.{0})
            inner join dbo.ACTOR ad on (ad.IDA = alloc.IDA_DEALER)
            inner join dbo.ACTOR ac on (ac.IDA = alloc.IDA_CLEARER)
            inner join dbo.ACTOR css on (css.IDA = tr.IDA_CSSCUSTODIAN)
            inner join dbo.BOOK bd on (bd.IDB = alloc.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = alloc.IDB_CLEARER)

            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = alloc.IDT)

            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = alloc.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = alloc.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acdg on (acdg.IDA = alloc.IDA_DEALER) and (acdg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkdg on (bkdg.IDB = alloc.IDB_DEALER) and (bkdg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GACTORROLE accg on (accg.IDA = alloc.IDA_CLEARER) and (accg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkcg on (bkcg.IDB = alloc.IDB_CLEARER) and (bkcg.IDROLEGBOOK = @ROLE)
            where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) > 0)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_EVENT_POS, m_WorkTableUpdateEntry);
        }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20240520 [WI930] Upd
        protected override string GetQueryCandidatesToClosingReopeningPosition2(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTBUSINESS, 
            tr.IDM, tr.ASSETCATEGORY, tr.IDASSET, tr.IDA_ENTITY, 
            tr.IDA_DEALER, ad.IDENTIFIER as DEALER_IDENTIFIER, tr.IDB_DEALER, bd.IDENTIFIER as BKDEALER_IDENTIFIER, 
            tr.IDA_CLEARER, ac.IDENTIFIER as CLEARER_IDENTIFIER, tr.IDB_CLEARER, bc.IDENTIFIER as BKCLEARER_IDENTIFIER, 
            tr.IDA_CSSCUSTODIAN, css.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,
            tr.SIDE, tr.PRICE, tr.DTEXECUTION,
            acdg.IDGACTOR as IDGACTOR_DEALER , bkdg.IDGBOOK as IDGBOOK_DEALER, 
            accg.IDGACTOR as IDGACTOR_CLEARER, bkcg.IDGBOOK as IDGBOOK_CLEARER, 
            ig.IDGINSTR, mg.IDGMARKET, 
            pr.IDP, pr.GPRODUCT, tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY,
            em.IDEM, ev.IDE as IDE_EVENT
            from dbo.TRADE tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{0})
            inner join dbo.MARKET mk on (mk.IDM = tr.IDM)
            inner join dbo.VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'OTC')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN = tr.IDA_CSSCUSTODIAN)
            inner join dbo.ACTOR ad on (ad.IDA = tr.IDA_DEALER)
            inner join dbo.ACTOR ac on (ac.IDA = tr.IDA_CLEARER)
            inner join dbo.ACTOR css on (css.IDA = tr.IDA_CSSCUSTODIAN)

            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.TRADE alloc
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.TRADE alloc
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = tr.IDT)

            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = tr.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = tr.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acdg on (acdg.IDA = tr.IDA_DEALER) and (acdg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkdg on (bkdg.IDB = tr.IDB_DEALER) and (bkdg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GACTORROLE accg on (accg.IDA = tr.IDA_CLEARER) and (accg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkcg on (bkcg.IDB = tr.IDB_CLEARER) and (bkcg.IDROLEGBOOK = @ROLE)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
            (bd.ISPOSKEEPING = 1) and (tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) > 0)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_EVENT_POS);
        }

        #endregion GetQueryCandidatesToClosingReopeningPosition
        #region GetQueryCandidatesToSafekeeping
        /// <summary>
        /// Retourne les négociation en position en date de règlement (DTSETTLT) en date DTBUSINESS de traitement 
        /// candidates à calcul des frais de tenue de garde
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20150708 [21103] New
        // RD 20171012 [23502] Use VW_TRADE_POS
        // EG 20181010 PERF Refactoring
        // EG 20190918 Rename GetQrySettlementPosAction
        protected override string GetQueryCandidatesToSafekeeping(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("SKP");

            string sqlInsert = String.Format(GetQrySafekeepingInsertSelect(), tableName.First, "OTC");
            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            
            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);

            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);

            return String.Format(GetQrySafekeepingSelect(), tableName.First, GetQrySettlementPosAction(tableName.First, "alloc"), GetQrySafekeepingAsset());
        }
        #endregion GetQueryCandidatesToSafekeeping
        #region GetQrySafekeepingAsset
        // EG 20180828 PERF New (Use by SKP)
        // EG 20190926 [Maturity redemption] Add test sur MATURITYDATE
        protected override string GetQrySafekeepingAsset()
        {
            return @"
            /* EQUITYASSET */
	        select 'EquityAsset' as ASSETCATEGORY, IDASSET, IDENTIFIER, null as MATURITYDATE
	        from dbo.ASSET_EQUITY
	        union all
	        /* FXRATE */
	        select 'FxRateAsset' as ASSETCATEGORY, IDASSET, IDENTIFIER, null as MATURITYDATE
	        from dbo.ASSET_FXRATE
	        union all
	        /* INDEX */
	        select 'Index' as ASSETCATEGORY, IDASSET, IDENTIFIER, null as MATURITYDATE
	        from dbo.ASSET_INDEX";
        }
        #endregion GetQrySafekeepingAsset

        #region GetQueryPosKeepingDataAsset
        /// <summary>
        /// Utilisée par les traitements OTC de tenue de position pour :
        ///    Recupérer pour alimenter les données de travail relatives à l'actif pour les traitements 
        /// </summary>
        /// <param name="pUnderlyingAsset">Catégorie d'actif</param>
        /// <param name="pPosRequestAssetQuote">Unused</param>
        /// <returns></returns>
        /// EG 20150302 Add FxRateAsset case (CFD FOREX)
        /// EG 20151125 [20979] Refactoring
        protected override string GetQueryPosKeepingDataAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            string sqlSelect;
            if (pUnderlyingAsset.HasValue)
            {
                switch (pUnderlyingAsset.Value)
            {
                    case Cst.UnderlyingAsset.EquityAsset:
                        sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, ast.IDC, 1 as CONTRACTMULTIPLIER, mk.IDBC,
                        isnull(ast.PARVALUE,1) as NOMINALVALUE,
                        isnull(ast.IDC_ISSUE,ast.IDC) as NOMINALCURRENCY, isnull(ast.IDC_STRIKE,ast.IDC) as PRICECURRENCY, ast.STRIKEPRICE
                        from dbo.ASSET_EQUITY ast
                        inner join dbo.MARKET mk on (mk.IDM = ast.IDM)
                        where (ast.IDASSET = @ID)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.Index:
                        sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, ast.IDC, 1 as CONTRACTMULTIPLIER, mk.IDBC, ast.IDC as NOMINALCURRENCY
                        from dbo.ASSET_INDEX ast
                        inner join dbo.MARKET mk on (mk.IDM = ast.IDM)
                        where (ast.IDASSET = @ID)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.Bond:
                    case Cst.UnderlyingAsset.ConvertibleBond:
                        sqlSelect = @"select tr.IDT as IDASSET, tr.IDENTIFIER as ASSET_IDENTIFIER, 1 as CONTRACTMULTIPLIER, 
                        ev.VALORISATION as NOMINALVALUE, ev.UNIT as NOMINALCURRENCY, ev.IDA_PAY as IDA_ISSUER , ev.IDB_PAY as IDB_ISSUER
                        from dbo.TRADE tr
                        inner join dbo.VW_INSTR_PRODUCT ns on (ns.IDI = tr.IDI)
                        inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'TER') and (ev.EVENTTYPE = 'NOM')
                        where (ns.GPRODUCT = 'ASSET') and (ns.FAMILY = 'DSE') and (tr.IDT = @ID)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.FxRateAsset:
                        sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, ast.IDC, ast.CONTRACTMULTIPLIER, mk.IDBC, ast.IDC as NOMINALCURRENCY
                        from dbo.ASSET_FXRATE ast
                        inner join dbo.MARKET mk on (mk.IDM = ast.IDM)
                        where (ast.IDASSET = @ID)" + Cst.CrLf;
                        break;
                
                    default:
                        throw new NotSupportedException("UnderlyingAsset category [" + pUnderlyingAsset.Value.ToString() + "] not managed");
            }
            }
            else
                throw new NotSupportedException("UnderlyingAsset category not specified");

            return sqlSelect;
        }
        #endregion GetQueryPosKeepingDataAsset
    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // S E C : EquitySecurityTransaction | DebtSecurityTransaction
    //────────────────────────────────────────────────────────────────────────────────────────────────

    public partial class PosKeepingGen_SEC : PosKeepingGenProcessBase
        {
        #region GetQueryCandidatesToCashFlows
        /// <summary>
        /// Retourne les négociation en position en date veille de DTBUSINESS candidates à calcul des Cash-Flows
        /// PS : QTY = position jour
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20141224 [20566] New
        // EG 20150219 Lesture des trades en position veille
        // EG 20150305 Exclusion des TRADES échus (DTOUTADJ > DTBUSINESS)
        // RD 20160805 [22415] Add parameter pIsDtSettlt = true for method GetQueryPositionActionBySide
        // EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_DtSettlt_BySide
        // EG 20190926 [Maturity redemption] Upd (New view VW_ASSET_SEC)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override string GetQueryCandidatesToCashFlows(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDASSET, ass.IDENTIFIER as ASSET_IDENTIFIER, tr.ASSETCATEGORY, 
            (tr.QTY - isnull(pabj.QTY,0) - isnull(pasj.QTY, 0)) as QTY
            from dbo." + VW_TRADE_POS + @" tr
            inner join dbo.VW_ASSET_SEC ass on (ass.IDASSET = tr.IDASSET) and (ass.ASSETCATEGORY = tr.ASSETCATEGORY) and ((ass.MATURITYDATE is null) or (@DTBUSINESS < ass.MATURITYDATE))
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.BUYER, true) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.SELLER, true) + @") pas on (pas.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.BUYER, false) + @") pabj on (pabj.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum.SELLER, false) + @") pasj on (pasj.IDT = tr.IDT)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS <= @DTBUSINESS) and (tr.POSKEEPBOOK_DEALER = 1) and (@DTBUSINESS <= tr.DTOUTADJ) and
            (tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0)>0)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToCashFlows
        #region GetQueryCandidatesToClosingReopeningPosition
        // EG 20190613 [24683] New
        // EG 20240520 [WI930] Upd
        protected override string GetQueryCandidatesToClosingReopeningPosition(string pCS)
        {
            string sqlSelect = @"select alloc.IDT, alloc.IDENTIFIER, alloc.IDI, alloc.DTBUSINESS, 
            alloc.IDM, alloc.ASSETCATEGORY, alloc.IDASSET, alloc.IDA_ENTITY, 
            alloc.IDA_DEALER, ad.IDENTIFIER as DEALER_IDENTIFIER, alloc.IDB_DEALER, bd.IDENTIFIER as BKDEALER_IDENTIFIER, 
            alloc.IDA_CLEARER, ac.IDENTIFIER as CLEARER_IDENTIFIER, alloc.IDB_CLEARER, bc.IDENTIFIER as BKCLEARER_IDENTIFIER, 
            alloc.IDA_CSSCUSTODIAN, css.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,
            alloc.SIDE, alloc.PRICE, alloc.DTEXECUTION,
            acdg.IDGACTOR as IDGACTOR_DEALER , bkdg.IDGBOOK as IDGBOOK_DEALER, 
            accg.IDGACTOR as IDGACTOR_CLEARER, bkcg.IDGBOOK as IDGBOOK_CLEARER, 
            ig.IDGINSTR, mg.IDGMARKET,
            pr.IDP, pr.GPRODUCT, alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY,
            alloc.IDEM, ev.IDE as IDE_EVENT
            from dbo.{1} alloc
            inner join dbo.VW_INSTR_PRODUCT pr on (pr.IDI= alloc.IDI)
            inner join dbo.EVENT ev on (ev.IDT = alloc.IDT) and (ev.{0})
            inner join dbo.ACTOR ad on (ad.IDA = alloc.IDA_DEALER)
            inner join dbo.ACTOR ac on (ac.IDA = alloc.IDA_CLEARER)
            inner join dbo.ACTOR css on (css.IDA = alloc.IDA_CSSCUSTODIAN)
            inner join dbo.BOOK bd on (bd.IDB = alloc.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = alloc.IDB_CLEARER)

            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = alloc.IDT)

            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = alloc.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = alloc.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acdg on (acdg.IDA = alloc.IDA_DEALER) and (acdg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkdg on (bkdg.IDB = alloc.IDB_DEALER) and (bkdg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GACTORROLE accg on (accg.IDA = alloc.IDA_CLEARER) and (accg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkcg on (bkcg.IDB = alloc.IDB_CLEARER) and (bkcg.IDROLEGBOOK = @ROLE)
            where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) > 0)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_EVENT_POS, m_WorkTableUpdateEntry);

        }
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20240520 [WI930] Upd
        protected override string GetQueryCandidatesToClosingReopeningPosition2(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTBUSINESS, 
            tr.IDM, tr.ASSETCATEGORY, tr.IDASSET, tr.IDA_ENTITY, 
            tr.IDA_DEALER, ad.IDENTIFIER as DEALER_IDENTIFIER, tr.IDB_DEALER, bd.IDENTIFIER as BKDEALER_IDENTIFIER, 
            tr.IDA_CLEARER, ac.IDENTIFIER as CLEARER_IDENTIFIER, tr.IDB_CLEARER, bc.IDENTIFIER as BKCLEARER_IDENTIFIER, 
            tr.IDA_CSSCUSTODIAN, css.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,
            tr.SIDE, tr.PRICE, tr.DTEXECUTION,
            acdg.IDGACTOR as IDGACTOR_DEALER , bkdg.IDGBOOK as IDGBOOK_DEALER, 
            accg.IDGACTOR as IDGACTOR_CLEARER, bkcg.IDGBOOK as IDGBOOK_CLEARER, 
            ig.IDGINSTR, mg.IDGMARKET, 
            pr.IDP, pr.GPRODUCT, tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY,
            em.IDEM, ev.IDE as IDE_EVENT
            from dbo.TRADE tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{0})
            inner join dbo.MARKET mk on (mk.IDM = tr.IDM)
            inner join dbo.VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'SEC')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN = tr.IDA_CSSCUSTODIAN)
            inner join dbo.ACTOR ad on (ad.IDA = tr.IDA_DEALER)
            inner join dbo.ACTOR ac on (ac.IDA = tr.IDA_CLEARER)
            inner join dbo.ACTOR css on (css.IDA = tr.IDA_CSSCUSTODIAN)

            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.TRADE alloc
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.TRADE alloc
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = tr.IDT)

            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = tr.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = tr.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acdg on (acdg.IDA = tr.IDA_DEALER) and (acdg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkdg on (bkdg.IDB = tr.IDB_DEALER) and (bkdg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GACTORROLE accg on (accg.IDA = tr.IDA_CLEARER) and (accg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkcg on (bkcg.IDB = tr.IDB_CLEARER) and (bkcg.IDROLEGBOOK = @ROLE)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
            (bd.ISPOSKEEPING = 1) and (tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) > 0)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_EVENT_POS);
        }

        #endregion GetQueryCandidatesToClosingReopeningPosition
        #region GetQueryCandidatesToMaturityRedemptionOffsettingDebtSecurity
        /// <summary>
        /// Retourne les négociations en position sur des DEBTSECURITY arrivant à échéance.
        /// </summary>
        // EG 20190926 [Maturity redemption] New
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected string GetQueryCandidatesToMaturityRedemptionOffsettingDebtSecurity(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("MOD");
            bool isNoHints = DataHelper.GetSvrInfoConnection(pCS).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(pCS).IsNoHints;
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(tr IX_TRADE5) index(bd IX_BOOK2) index(ti PK_TRADEINSTRUMENT) */";

            string sqlInsert = String.Format(@"insert into dbo.{0}
            (IDT, IDENTIFIER, IDI, DTTRADE, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, PRICE, QTY, IDEM, 
            IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            select {1}
            tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE,
            tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY,
            em.IDEM, em.IDA, em.DTENTITY, bd.IDA_ENTITY, bc.IDA_ENTITY
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.FAMILY = 'DSE') and (pr.GPRODUCT = 'SEC')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ENTITYMARKET em on (em.IDM = tr.IDM) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN = tr.IDA_CSSCUSTODIAN)
            inner join dbo.VW_TRADE_ASSET ast on (ast.IDT = tr.IDASSET) and (ast.MATURITYDATE between em.DTENTITYPREV and em.DTENTITY)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')", tableName.First, hintsOracle);

            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);

            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);

            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);

            string sqlSelect = String.Format(@"select alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) as QTY_BUY,
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) as QTY_SELL,
            alloc.ASSETCATEGORY, 1 as ISCUSTODIAN
            from dbo.{0} alloc
            {1}
            group by alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, 
            alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER, 
            alloc.ASSETCATEGORY
            having 
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) > 0
            or
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) > 0" + Cst.CrLf,
            tableName.First, GetQryPosActionBasedWithTrade(tableName.First, "alloc"));
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToMaturityRedemptionOffsettingDebtSecurity

        #region GetQueryCandidatesToSafekeeping
        /// <summary>
        /// Retourne les négociation en position en date de règlement (DTSETTLT) en date DTBUSINESS de traitement 
        /// candidates à calcul des frais de tenue de garde
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20150708 [21103] New
        // RD 20171012 [23502] Use VW_TRADE_POS
        // EG 20181010 PERF Refactoring
        protected override string GetQueryCandidatesToSafekeeping(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("SKP");

            string sqlInsert = String.Format(GetQrySafekeepingInsertSelect(), tableName.First, "SEC");

            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);

            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);


            return String.Format(GetQrySafekeepingSelect(), tableName.First, GetQrySettlementPosAction(tableName.First, "alloc"), GetQrySafekeepingAsset());
        }

        #endregion GetQueryCandidatesToSafekeeping
        #region GetQueryDataRequest

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pRequestType"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdEM"></param>
        /// <param name="pPosActionType"></param>
        /// <param name="pSettlSessIDEnum"></param>
        /// <returns></returns>
        // EG 20190926 [Maturity redemption] New override
        protected override Pair<string, DataParameters> GetQueryDataRequest(string pCS, IDbTransaction pDbTransaction, Cst.PosRequestTypeEnum pRequestType, DateTime pDate, int pIdEM,
            Nullable<PosKeepingTools.PosActionType> pPosActionType, Nullable<SettlSessIDEnum> pSettlSessIDEnum)
        {
            Pair<string, DataParameters> _query = base.GetQueryDataRequest(pCS, pDbTransaction, pRequestType, pDate, pIdEM, pPosActionType, pSettlSessIDEnum);

            if (null == _query)
            {
                string sqlSelect = string.Empty;
                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
                parameters.Add(new DataParameter(pCS, "IDEM", DbType.Int32), pIdEM);
                switch (pRequestType)
                {
                    case Cst.PosRequestTypeEnum.MaturityRedemptionOffsettingDebtSecurity:
                        parameters.Clear();
                        parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
                        sqlSelect = GetQueryCandidatesToMaturityRedemptionOffsettingDebtSecurity(pCS);
                        break;
                }

                if (StrFunc.IsFilled(sqlSelect))
                {
                    _query = new Pair<string, DataParameters>(sqlSelect, parameters);
                }
            }
            return _query;
        }
        #endregion GetQueryDataRequest

        #region GetQrySafekeepingAsset
        // EG 20180828 PERF New (Use by SKP)
        // EG 20190926 [Maturity redemption] Upd (Use view VW_ASSET_SEC)
        protected override string GetQrySafekeepingAsset()
        {
            return @"select ASSETCATEGORY, IDASSET, IDENTIFIER, MATURITYDATE
            from VW_ASSET_SEC";
        }
        #endregion GetQrySafekeepingAsset

        #region GetQueryPosKeepingDataAsset
        /// <summary>
        /// Utilisée par les traitements OTC de tenue de position pour :
        ///    Recupérer pour alimenter les données de travail relatives à l'actif pour les traitements 
        /// </summary>
        /// <param name="pUnderlyingAsset">Catégorie d'actif</param>
        /// <param name="pPosRequestAssetQuote">Unused</param>
        /// <returns></returns>
        /// EG 20150302 Add FxRateAsset case (CFD FOREX)
        /// EG 20151125 [20979] Refactoring
        // EG 20190823 [FIXEDINCOME] Upd
        // EG 20190926 [Maturity redemption] Upd (New columns ISSUEPRICEPCT and REDEMPTIONPRICEPCT)
        protected override string GetQueryPosKeepingDataAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            string sqlSelect;
            if (pUnderlyingAsset.HasValue)
            {
                switch (pUnderlyingAsset.Value)
                {
                    case Cst.UnderlyingAsset.EquityAsset:
                        sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, ast.IDC, 1 as CONTRACTMULTIPLIER, mk.IDBC,
                        isnull(ast.PARVALUE,1) as NOMINALVALUE,
                        isnull(ast.IDC_ISSUE,ast.IDC) as NOMINALCURRENCY, isnull(ast.IDC_STRIKE,ast.IDC) as PRICECURRENCY, ast.STRIKEPRICE
                        from dbo.ASSET_EQUITY ast
                        inner join dbo.MARKET mk on (mk.IDM = ast.IDM)
                        where (ast.IDASSET = @ID)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.Index:
                        sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, ast.IDC, 1 as CONTRACTMULTIPLIER, mk.IDBC, ast.IDC as NOMINALCURRENCY
                        from dbo.ASSET_INDEX ast
                        inner join dbo.MARKET mk on (mk.IDM = ast.IDM)
                        where (ast.IDASSET = @ID)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.Bond:
                    case Cst.UnderlyingAsset.ConvertibleBond:
                        sqlSelect = @"select tr.IDT as IDASSET, tr.IDENTIFIER as ASSET_IDENTIFIER, ev.UNIT as IDC, 1 as CONTRACTMULTIPLIER, 
                        ev.VALORISATION as NOMINALVALUE, ev.UNIT as NOMINALCURRENCY, ev.IDA_PAY as IDA_ISSUER , ev.IDB_PAY as IDB_ISSUER,
                        ta.ISSUEPRICEPCT, ta.REDEMPTIONPRICEPCT
                        from dbo.TRADE tr
                        inner join dbo.TRADEASSET ta on (ta.IDT = tr.IDT)
                        inner join dbo.VW_INSTR_PRODUCT ns on (ns.IDI = tr.IDI)
                        inner join EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'TER') and (ev.EVENTTYPE = 'NOM')
                        where (ns.GPRODUCT = 'ASSET') and (ns.FAMILY = 'DSE') and (tr.IDT = @ID)" + Cst.CrLf;
                        break;
                    case Cst.UnderlyingAsset.FxRateAsset:
                        sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER as ASSET_IDENTIFIER, ast.IDC, ast.CONTRACTMULTIPLIER, mk.IDBC, ast.IDC as NOMINALCURRENCY
                        from dbo.ASSET_FXRATE ast
                        inner join dbo.MARKET mk on (mk.IDM = ast.IDM)
                        where (ast.IDASSET = @ID)" + Cst.CrLf;
                        break;

                    default:
                        throw new NotSupportedException("UnderlyingAsset category [" + pUnderlyingAsset.Value.ToString() + "] not managed");
                }
            }
            else
                throw new NotSupportedException("UnderlyingAsset category not specified");

            return sqlSelect;
        }
        #endregion GetQueryPosKeepingDataAsset
        #region GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity
        /// <summary>
        /// Lecture des titres remboursables
        /// </summary>
        /// <param name="pDate">Date business</param>
        /// <param name="pPosKeepingKey">Cle de position</param>
        /// <returns></returns>
        // EG 20190918 New (Maturity Redemption DEBTSECURITY)
        protected override QueryParameters GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity(DateTime pDate, IPosKeepingKey pPosKeepingKey)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), pPosKeepingKey.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pPosKeepingKey.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), pPosKeepingKey.IdA_Dealer);
            parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), pPosKeepingKey.IdB_Dealer);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), pPosKeepingKey.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), pPosKeepingKey.IdB_Clearer);

            string tableName = StrFunc.AppendFormat("TRADE_MOD_{0}_W", PKGenProcess.Session.BuildTableId()).ToUpper();
            string sqlQuery = String.Format(@"select 
            alloc.IDT, alloc.IDENTIFIER, (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) as QTY, 
            alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY, 
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            alloc.SIDE, alloc.PRICE, ev.IDE as IDE_EVENT,
            ast.DATEDDATE, ast.MATURITYDATE, ast.PARVALUE, ast.IDC_PARVALUE, ast.IDENTIFIER as ASSET_IDENTIFIER
            from dbo.{0} alloc
            {1}
            inner join dbo.VW_TRADE_ASSET ast on (ast.IDT = alloc.IDASSET)
            inner join dbo.EVENT ev on (ev.IDT = alloc.IDT) and (ev.EVENTTYPE = 'DSA')
            where (alloc.IDA_DEALER = @IDA_DEALER) and (alloc.IDB_DEALER = @IDB_DEALER) and 
                  (alloc.IDA_CLEARER = @IDA_CLEARER) and (alloc.IDB_CLEARER = @IDB_CLEARER) and 
                  (alloc.IDI = @IDI) and (alloc.IDASSET = @IDASSET) and 
                  (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0) <> 0)", tableName, GetQrySettlementPosAction(tableName, "alloc")) + Cst.CrLf;

            return new QueryParameters(CS, sqlQuery, parameters);
        }
        #endregion GetQueryTradeCandidatesToMaturityRedemptionDebtSecurity
        }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // M T M : MarkToMarket
    //────────────────────────────────────────────────────────────────────────────────────────────────

    public partial class PosKeepingGen_MTM : PosKeepingGenProcessBase
    {
        #region GetQueryCandidatesToCashFlows
        /// <summary>
        /// Retourne les négociation candidates à calcul des Cash-Flows
        /// Cash-Flows :
        /// </summary>
        /// <returns></returns>
        /// EG 20141224 [20566] New
        /// EG 20150219 Lesture des trades en position veille
        /// EG 20150305 Exclusion des TRADES échus (DTOUTADJ > DTBUSINESS)
        /// EG 20150331 [POC] Traitement des trades ALLOC
        /// EG 20151125 [20979] Refactoring : New
        /// EG 20171016 [23509] Add DTEXECUTION, TZFACILITY, DTEXECUTION remplace DTTIMESTAMP dans Restriction sur Query
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override string GetQueryCandidatesToCashFlows(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.IDI, tr.SIDE, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER,
            ts.IDC, ts.IDC2, tr.DTOUTUNADJ, ns.GPRODUCT, ir.MARGININGMODE, ev.NOTIONALREFERENCE
            from dbo.TRADE tr
            inner join dbo.TRADESTREAM ts on (ts.IDT = tr.IDT)
            /* Recherche du Reference amount : CU1 pour FX et CCU (si differente de celle de PRM)|PCU (si differente de celle de PRM) */ 
            inner join ( 
                select ev.IDT, 
                SUM(case when (evp.EVENTTYPE = 'PRM') and (evp.UNIT = ev.UNIT) then 0 else ev.VALORISATION end) as NOTIONALREFERENCE
                from dbo.EVENT ev
                left outer join dbo.EVENT evp on (evp.IDT = ev.IDT) and (evp.EVENTTYPE = 'PRM')
                where (ev.EVENTCODE='STA') and (ev.EVENTTYPE in ('CU1','CCU','PCU'))
                group by ev.IDT
            ) ev on (ev.IDT = tr.IDT)

            inner join dbo.VW_INSTR_PRODUCT ns on ( ns.IDI = tr.IDI) and (ns.FUNGIBILITYMODE = 'NONE') and (ns.ISMARGINING = 1)
            inner join dbo.IMREQINSTRPARAM ir on (ir.IDA = tr.IDA_DEALER) and (ir.IDI = tr.IDI) and (ir.MARGININGMODE != 'None')
            inner join dbo.ENTITYMARKET em on ( em.IDEM = @IDEM) and (em.IDA_CUSTODIAN = tr.IDA_CSSCUSTODIAN)
            inner join dbo.MARKET mk on (mk.IDM = em.IDM)
            where (em.IDEM = @IDEM) and (tr.DTOUTADJ >= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') " + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToCashFlows
        #region GetQueryNewTradeAfterProcessEndOfDayInSuccess
        /// <summary>
        ///  voir PosKeepingGen_XXX - Override
        ///  Utilisé dans le traitement de clôture de journée (Etape de contrôle)
        ///  Retourne les trades  "allocation" du jour créés APRES le DERNIER TRAITEMENT DE FIN DE JOURNEE
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring
        //protected override string GetQueryNewTradeAfterProcessEndOfDayInSuccess(string pCS)
        //{
        //    return string.Empty;
        //}
        #endregion GetQueryNewTradeAfterProcessEndOfDayInSuccess

    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // C O M
    //────────────────────────────────────────────────────────────────────────────────────────────────

    public partial class PosKeepingGen_COM : PosKeepingGenProcessBase
        {
        #region GetQueryCandidatesToCashFlows
        /// <summary>
        /// Retourne les négociation en position en date veille de DTBUSINESS candidates à calcul des Cash-Flows
        /// PS : QTY = position jour
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20141224 [20566] New
        // EG 20150219 Lesture des trades en position veille
        // EG 20150305 Exclusion des TRADES échus (DTOUTADJ > DTBUSINESS)
        // RD 20160805 [22415] Add parameter pIsDtSettlt = true for method GetQueryPositionActionBySide
        protected override string GetQueryCandidatesToCashFlows(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToCashFlows
        #region GetQueryCandidatesToClosingReopeningPosition
        // EG 20190613 [24683] New
        protected override string GetQueryCandidatesToClosingReopeningPosition(string pCS)
        {
            return string.Empty;
        }
        protected override string GetQueryCandidatesToClosingReopeningPosition2(string pCS)
        {
            return string.Empty;
        }
        #endregion GetQueryCandidatesToClosingReopeningPosition

        #region GetQueryPosKeepingDataAsset
        /// <summary>
        /// Utilisée par les traitements OTC de tenue de position pour :
        ///    Recupérer pour alimenter les données de travail relatives à l'actif pour les traitements 
        /// </summary>
        /// <param name="pUnderlyingAsset">Catégorie d'actif</param>
        /// <param name="pPosRequestAssetQuote">Unused</param>
        /// <returns></returns>
        /// EG 20150302 Add FxRateAsset case (CFD FOREX)
        /// EG 20151125 [20979] Refactoring
        protected override string GetQueryPosKeepingDataAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            string sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER  as ASSET_IDENTIFIER, ast.CLEARANCESYSTEM, ast.ASSETSYMBOL,
            ast.IDBC, ast.IDCC, ast.IDC, ast.IDM, ast.CONTRACTIDENTIFIER, ast.CONTRACTDISPLAYNAME, ast.CONTRACTDESCRIPTION,
            ast.TRADABLETYPE, ast.COMMODITYCLASS, ast.COMMODITYTYPE, ast.CONTRACTSYMBOL, ast.DELIVERYPOINT, ast.MINPRICEINCR, ast.MINPRICEINCRAMOUNT,
            ast.UNITOFMEASURE, ast.UNITOFMEASUREQTY, ast.UNITOFPRICE, ast.ISWITHNEGPRICE, ast.MINPRICE, ast.MAXPRICE, ast.ISDST, ast.DURATION,
            ast.DELIVERYTIMESTART, ast.DELIVERYTIMEEND, ast.DELIVERYTIMEZONE, ast.QUALITY, ast.FREQUENCYQTY
            from dbo.VW_ASSET_COMMODITY_EXPANDED ast
            where (ast.IDASSET = @ID)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryPosKeepingDataAsset
    }

    //────────────────────────────────────────────────────────────────────────────────────────────────
    // E T D
    //────────────────────────────────────────────────────────────────────────────────────────────────

    public partial class PosKeepingGen_ETD : PosKeepingGenProcessBase
    {
        #region GetQueryPositionTradeOption
        /// <summary>
        /// Utilisée par le traitement de dénouement d'options (manuel, automatique, manuel via importation par position)
        ///      Retourne la négociation option et sa quantité en position
        /// </summary>
        /// <param name="pIsExceptCurrentIdPR">Si = true : à l'exception de celle concernant le POSREQUEST en cours</param>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : New
        // EG 20160128 Add ISCUSTODIAN
        // EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_Trade_BySide
        protected override string GetQueryPositionTradeOption(bool pIsExceptCurrentIdPR)
        {
            // TRADE IN POSITION
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, (tr.QTY - isnull(pab.QTY, 0) - isnull(pas.QTY, 0)) as QTY,
            tr.IDA_ENTITY, tr.IDEM, tr.IDA_CSSCUSTODIAN, tr.IDI, tr.IDASSET, tr.DTMARKET, tr.DTENTITY, tr.PRICE, tr.DTTRADE, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, 
            tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.SIDE, tr.ISAUTOABN, ev.IDE_EVENT, ec.EVENTCLASS, ec.DTEVENT as EXPIRYDATE, tr.ASSETCATEGORY, 0 as ISCUSTODIAN
            from dbo.VW_TRADE_POSETD tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE in ('ASD','EXD'))
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            left outer join (" + PosKeepingTools.GetQryPosAction_Trade_BySide(BuyerSellerEnum.BUYER, true, pIsExceptCurrentIdPR) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_Trade_BySide(BuyerSellerEnum.SELLER, true, pIsExceptCurrentIdPR) + @") pas on (pas.IDT = tr.IDT)
            where ((tr.QTY - isnull(pab.QTY, 0) - isnull(pas.QTY, 0)) >0) and (tr.POSKEEPBOOK_DEALER = 1) and (tr.IDT = @IDT)" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryPositionTradeOption

        #region GetQueryPositionDenOption
        /// <summary>
        /// Utilisée par le traitement de dénouement d'options (manuel via importation par position)
        ///      Retourne les négociation options et leur quantité en position candidates à dénouement via dénouement par position
        ///     La jointure sur EVENT (LPC/AMT) permet de récupérer l'IDE parent utilisé pour les insertions d'EVTs dans la suite du traitement
        /// </summary>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring : New
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        /// EG 20171016 [23509] Add tr.DTEXECUTION, tr.TZFACILITY
        protected string GetQueryPositionDenOption()
        {
            // TRADES en POSITION
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.IDI, tr.IDM, tr.IDASSET, tr.POSITIONEFFECT, tr.EXECUTIONID, tr.PRICE, 
            tr.DTBUSINESS, tr.SIDE, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, ev.IDE_EVENT,
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0))
            from dbo.VW_TRADE_POSETD tr
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = 'AMT')
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER, false, false, false, true) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER, false, false, false, true) + @") pas on (pas.IDT = tr.IDT)
            where (tr.DTTRADE <= @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and
            (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) > 0) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER, 0) = @IDB_CLEARER)" + Cst.CrLf;

            AddSqlRestrictDealer(ref sqlSelect);

            sqlSelect += SQLCst.SEPARATOR_MULTISELECT;

            sqlSelect += GetQueryPositionActionOnDtBusiness(true);
            return sqlSelect;
        }
        #endregion GetQueryPositionDenOption

        #region GetQueryCandidatesToCashFlows
        /// <summary>
        /// Retourne les négociation en position en date veille de DTBUSINESS candidates à calcul des Cash-Flows
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// EG 20130917 [18953] Calcul des Cash-Flows Exclusion des TRADES sans gestion de tenue de position
        /// EG 20140204 [19587] Exclusion des options à l'échéance
        /// EG 20141224 [20566] New 
        /// EG 20150615 Add column QTY used in EOD_CashFlowsGen() method (display WARNING or not) 
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        // EG 20180906 PERF No View & Add DTOUT (Alloc ETD only) 
        // EG 20181119 PERF Correction post RC
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201201 [25562] Ajout index IX_MATURITY2 (IDMATURITY, MATURITYDATE,MATURITYDATESYS)
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        protected override string GetQueryCandidatesToCashFlows(string pCS)
        {
            //string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDASSET, ass.IDENTIFIER as ASSET_IDENTIFIER, 
            //isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATESYS,
            //tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0) as QTY
            //from dbo.TRADE tr
            //inner join dbo.VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            //inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            //inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            //inner join dbo.ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
            //inner join dbo.ASSET_ETD ass on (ass.IDASSET = tr.IDASSET) and ((ass.DTDISABLED is null) or (ass.DTDISABLED > @DTBUSINESS))
            //inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)
            //inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            //inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and 
            //((isnull(ma.MATURITYDATESYS,ma.MATURITYDATE) is null) or (isnull(ma.MATURITYDATESYS, ma.MATURITYDATE)  > @DTBUSINESS) or
            //((dc.CATEGORY='F') and (isnull(ma.MATURITYDATESYS,ma.MATURITYDATE)=@DTBUSINESS)))
            //left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER, true) + @") pab on (pab.IDT = tr.IDT)
            //left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER, true) + @") pas on (pas.IDT = tr.IDT)
            //where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            //(tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
            //(bd.ISPOSKEEPING = 1) and (tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0)>0)" + Cst.CrLf;
            //return sqlSelect;
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDASSET, ass.IDENTIFIER as ASSET_IDENTIFIER, 
            isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATESYS,
            tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0) as QTY
            from dbo.ENTITYMARKET em
            inner join dbo.TRADE tr on ( tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ASSET_ETD ass on (ass.IDASSET = tr.IDASSET) and ((ass.DTDISABLED is null) or (ass.DTDISABLED > @DTBUSINESS))
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and 
            ((isnull(ma.MATURITYDATESYS,ma.MATURITYDATE) is null) or (isnull(ma.MATURITYDATESYS, ma.MATURITYDATE)  > @DTBUSINESS) or
            ((dc.CATEGORY='F') and (isnull(ma.MATURITYDATESYS,ma.MATURITYDATE)=@DTBUSINESS)))
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER, true) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER, true) + @") pas on (pas.IDT = tr.IDT)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (tr.QTY - isnull(pab.QTY,0) - isnull(pas.QTY, 0)>0)" + Cst.CrLf;
            return sqlSelect;

        }
        #endregion GetQueryCandidatesToCashFlows

        #region GetQueryCandidatesToClosingReopeningPosition
        // EG 20190308 New
        // EG 20190318 Upd ClosingReopening position Step3
        // EG 20190613 [24683] Upd
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20231214 [WI725] Closing/Reopening : Add dc.CATEGORY (unused for the moment)
        protected override string GetQueryCandidatesToClosingReopeningPosition(string pCS)
        {
            string sqlSelect = @"select alloc.IDT, alloc.IDENTIFIER, alloc.IDI, alloc.DTBUSINESS, 
            alloc.IDM, alloc.ASSETCATEGORY, alloc.IDASSET, alloc.IDA_ENTITY, 
            alloc.IDA_DEALER, ad.IDENTIFIER as DEALER_IDENTIFIER, alloc.IDB_DEALER, bd.IDENTIFIER as BKDEALER_IDENTIFIER, 
            alloc.IDA_CLEARER, ac.IDENTIFIER as CLEARER_IDENTIFIER, alloc.IDB_CLEARER, bc.IDENTIFIER as BKCLEARER_IDENTIFIER, 
            alloc.SIDE, alloc.PRICE, alloc.DTEXECUTION,
            dc.IDDC, dc.FUTVALUATIONMETHOD, dc.IDASSET_UNL, dc.CATEGORY,
            acdg.IDGACTOR as IDGACTOR_DEALER , bkdg.IDGBOOK as IDGBOOK_DEALER, 
            accg.IDGACTOR as IDGACTOR_CLEARER, bkcg.IDGBOOK as IDGBOOK_CLEARER, 
            ig.IDGINSTR, mg.IDGMARKET, cg.IDGCONTRACT, 
            pr.IDP, pr.GPRODUCT, alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY,
            alloc.IDEM, ev.IDE as IDE_EVENT
            from dbo.{1} alloc
            inner join dbo.VW_INSTR_PRODUCT pr on (pr.IDI= alloc.IDI)
            inner join dbo.EVENT ev on (ev.IDT = alloc.IDT) and (ev.{0})
            inner join dbo.ACTOR ad on (ad.IDA = alloc.IDA_DEALER)
            inner join dbo.ACTOR ac on (ac.IDA = alloc.IDA_CLEARER)
            inner join dbo.BOOK bd on (bd.IDB = alloc.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = alloc.IDB_CLEARER)
            inner join dbo.ASSET_ETD ass on (ass.IDASSET = alloc.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.{1} alloc 
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = alloc.IDT)

            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = alloc.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = alloc.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acdg on (acdg.IDA = alloc.IDA_DEALER) and (acdg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkdg on (bkdg.IDB = alloc.IDB_DEALER) and (bkdg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GACTORROLE accg on (accg.IDA = alloc.IDA_CLEARER) and (accg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkcg on (bkcg.IDB = alloc.IDB_CLEARER) and (bkcg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GCONTRACTROLE cg on (cg.IDDC = dc.IDDC) and (cg.IDROLEGCONTRACT = @ROLE)
            where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) > 0)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_EVENT_POS, m_WorkTableUpdateEntry);
        }
        // EG 20190613 [24683] Upd
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20230901 [WI701] ClosingReopeningPosition - Delisting action - Process
        // EG 20240520 [WI930] Upd
        protected override string GetQueryCandidatesToClosingReopeningPosition2(string pCS)
        {
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTBUSINESS, 
            tr.IDM, tr.ASSETCATEGORY, tr.IDASSET, tr.IDA_ENTITY, 
            tr.IDA_DEALER, ad.IDENTIFIER as DEALER_IDENTIFIER, tr.IDB_DEALER, bd.IDENTIFIER as BKDEALER_IDENTIFIER, 
            tr.IDA_CLEARER, ac.IDENTIFIER as CLEARER_IDENTIFIER, tr.IDB_CLEARER, bc.IDENTIFIER as BKCLEARER_IDENTIFIER, 
            tr.IDA_CSSCUSTODIAN, css.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER,
            tr.SIDE, tr.PRICE, tr.DTEXECUTION,
            dc.IDDC, dc.FUTVALUATIONMETHOD, dc.IDASSET_UNL,
            acdg.IDGACTOR as IDGACTOR_DEALER , bkdg.IDGBOOK as IDGBOOK_DEALER, 
            accg.IDGACTOR as IDGACTOR_CLEARER, bkcg.IDGBOOK as IDGBOOK_CLEARER, 
            ig.IDGINSTR, mg.IDGMARKET, cg.IDGCONTRACT, 
            pr.IDP, pr.GPRODUCT, tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY,
            em.IDEM, ev.IDE as IDE_EVENT
            from dbo.TRADE tr
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.{0})
            inner join dbo.VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.MARKET mk on (mk.IDM = tr.IDM)
            inner join dbo.ENTITYMARKET em on ( em.IDM = tr.IDM ) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
            inner join dbo.ACTOR ad on (ad.IDA = tr.IDA_DEALER)
            inner join dbo.ACTOR ac on (ac.IDA = tr.IDA_CLEARER)
            inner join dbo.ACTOR css on (css.IDA = tr.IDA_CSSCUSTODIAN)
            inner join dbo.ASSET_ETD ass on (ass.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)

            left outer join
            (
                select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                from dbo.TRADE alloc
                inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_BUY
        
                union all
        
                select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                from dbo.TRADE alloc
                inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and 
                ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                group by pad.IDT_SELL
            ) pos on (pos.IDT = tr.IDT)

            left outer join dbo.VW_GINSTRROLE ig on (ig.IDI = tr.IDI) and (ig.IDROLEGINSTR = @ROLE)
            left outer join dbo.VW_GMARKETROLE mg on (mg.IDM = tr.IDM) and (mg.IDROLEGMARKET = @ROLE)
            left outer join dbo.VW_GACTORROLE acdg on (acdg.IDA = tr.IDA_DEALER) and (acdg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkdg on (bkdg.IDB = tr.IDB_DEALER) and (bkdg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GACTORROLE accg on (accg.IDA = tr.IDA_CLEARER) and (accg.IDROLEGACTOR = @ROLE)
            left outer join dbo.VW_GBOOKROLE bkcg on (bkcg.IDB = tr.IDB_CLEARER) and (bkcg.IDROLEGBOOK = @ROLE)
            left outer join dbo.VW_GCONTRACTROLE cg on (cg.IDDC = dc.IDDC) and (cg.IDROLEGCONTRACT = @ROLE)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
            (bd.ISPOSKEEPING = 1) and (tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) > 0)" + Cst.CrLf;

            return String.Format(sqlSelect, RESTRICT_EVENT_POS);
        }
        #endregion GetQueryCandidatesToClosingReopeningPosition

        #region GetQueryCandidatesToManualDenouement
        /// <summary>
        /// Retourne les enregistrements POSREQUEST qui concerne le denouement d'option
        /// </summary>
        /// <param name="pPosActionType"></param>
        /// <returns></returns>
        /// FI 20130311 [18467] add parameter pPosActionType, add column IDPR_POSREQUEST 
        /// FI 20130318 [18467] add column SOURCE
        /// FI 20130917 [18953] Modification de la signature de la méthode add paramètre pSettlSessIDEnum
        /// EG 20141224 [20566] Refactoring
        /// RD 20210624 [25789] Add column ASSETCATEGORY            
        protected override string GetQueryCandidatesToManualDenouement(PosKeepingTools.PosActionType pPosActionType, SettlSessIDEnum pSettlSessIDEnum)
        {
            string sqlSelect = @"select pr.IDT, pr.IDA_ENTITY, pr.IDA_CSSCUSTODIAN, pr.IDA_CSS, pr.IDA_CUSTODIAN, pr.IDEM, pr.DTBUSINESS, 
            pr.IDPR, pr.IDPR_POSREQUEST, pr.QTY, pr.REQUESTDETAIL, pr.SOURCE, pr.STATUS, pr.IDI, pr.IDASSET, '{0}' as ASSETCATEGORY, pr.IDA_DEALER, pr.IDB_DEALER, pr.IDA_CLEARER, pr.IDB_CLEARER
            from dbo.POSREQUEST pr
            where (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.REQUESTTYPE in ('ABN','NEX','NAS','ASS','EXE')) and 
            (pr.STATUS != 'SUCCESS') and (pr.REQUESTMODE = '{1}') and (pr.IDT is {2})" + Cst.CrLf;
            sqlSelect = String.Format(sqlSelect, "ExchangeTradedContract", ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(pSettlSessIDEnum), (pPosActionType == PosKeepingTools.PosActionType.Trade) ? "not null" : "null");
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToManualDenouement
        #region GetQueryCandidatesToMaturityOffsettingFuture
        /// <summary>
        /// Retourne les négociations en position sur des contrats FUTURES arrivant à échéance.
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20130701 on lit désormais les contrats échus et non liquidés 
        // => cas possible sur des contrats sans MATURITYDATE le jour réel de leur liquidation, mais renseigné par la suite), attention la liquidation
        // sera à la date courante du journée de bourse.
        // EG 20140225 [19575][19666]
        // EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
        // EG 20141224 [20566] Refactoring
        // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        // EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        // EG 20180803 PERF (Utilisation d'une table "temporaire" sur la base d'une table MODELE)
        // EG 20180906 PERF No View & Add DTOUT
        // EG 20181010 PERF Refactoring
        // EG 20181108 PERF Add Hints
        // EG 20181119 PERF Correction post RC
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (CLOSINGDAY only - augmentation performances)
        // EG 20201118 [24769] Correction bug sur offset row fetch next (manque order)
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        protected override string GetQueryCandidatesToMaturityOffsettingFuture(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("MOF");
            bool isNoHints = DataHelper.GetSvrInfoConnection(pCS).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(pCS).IsNoHints;
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(tr IX_TRADE5) index(bd IX_BOOK2) index(ti PK_TRADEINSTRUMENT) */";

            //string sqlInsert = String.Format(@"insert into dbo.{0}
            //(IDT, IDENTIFIER, IDI, DTTRADE, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, PRICE, QTY, IDEM, 
            //IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            //select {1}
            //tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE,
            //tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY,
            //em.IDEM, em.IDA, em.DTENTITY, bd.IDA_ENTITY, bc.IDA_ENTITY
            //from dbo.TRADE tr
            //inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            //inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            //inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            //inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            //inner join dbo.ENTITYMARKET em on (em.IDM = tr.IDM) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
            //inner join dbo.ASSET_ETD ast on (ast.IDASSET = tr.IDASSET)
            //inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)
            //inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (isnull(ma.MATURITYDATE, ma.MATURITYDATESYS) = em.DTENTITY)
            //inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'F')
            //where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            //(tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')", tableName.First, hintsOracle);
            string sqlInsert = String.Format(@"insert into dbo.{0}
            (IDT, IDENTIFIER, IDI, DTTRADE, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, PRICE, QTY, IDEM, 
            IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            select {1}
            tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE,
            tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY,
            em.IDEM, em.IDA, em.DTENTITY, tr.IDA_ENTITY, bc.IDA_ENTITY
            from dbo.ENTITYMARKET em
            inner join dbo.TRADE tr on (tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ASSET_ETD ast on (ast.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (isnull(ma.MATURITYDATE, ma.MATURITYDATESYS) = em.DTENTITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'F')
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')", tableName.First, hintsOracle);

            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);

            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);

            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);

            string fetchRestrictionForClosingDayControl = string.Empty;
            if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                fetchRestrictionForClosingDayControl = "order by alloc.IDASSET offset 0 rows fetch next 5 rows only";

            string sqlSelect = String.Format(@"select alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) as QTY_BUY,
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) as QTY_SELL,
            alloc.ASSETCATEGORY, 0 as ISCUSTODIAN
            from dbo.{0} alloc
            {1}
            group by alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, 
            alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER, 
            alloc.ASSETCATEGORY
            having 
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) > 0
            or
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) > 0
            {2}"+ Cst.CrLf,
            tableName.First, GetQryPosActionBasedWithTrade(tableName.First, "alloc"), fetchRestrictionForClosingDayControl);
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToMaturityOffsettingFuture
        #region GetQueryCandidatesToMaturityOffsettingOption
        /// <summary>
        /// Utilisée par le traitement EOD (Dénouement automatique d'options :
        ///    Recupérer pour les négociations candidates à dénouement automatique (en position et arrivées à échéance)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// EG 20151125 [20979] Refactoring
        /// EG 20160128 Add ISCUSTODIAN 
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide 
        // EG 20180221 [23769] Upd Jointure ACTOR|BOOK pour gestion asynchrone
        // EG 20181010 PERF Refactoring
        // EG 20181108 PERF Add Hints
        // EG 20181119 PERF Correction post RC
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (CLOSINGDAY only - augmentation performances)
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        // EG 20230221 [XXXXX] Ajout clause DTOUT sur Query trades candidats MOO
        protected override string GetQueryCandidatesToMaturityOffsettingOption(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "EXPIRYDATE", DbType.Date), m_MarketPosRequest.DtBusiness);
            bool isNoHints = DataHelper.GetSvrInfoConnection(pCS).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(pCS).IsNoHints;
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(tr IX_TRADE5) index(bd IX_BOOK2) index(ti PK_TRADEINSTRUMENT) */";
            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("MOO");

            //string sqlInsert = String.Format(@"insert into dbo.{0}
            //(IDT, IDENTIFIER, IDI, DTTRADE, IDM, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, 
            //SIDE, PRICE, QTY, IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, BOOKDEALER_IDENTIFIER, IDA_ENTITYCLEARER, IDE_EVENT, EVENTCLASS, DTEVENT, ISAUTOABN, ISCUSTODIAN)
            //select {1}
            //tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE, tr.IDM, tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, 
            //tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY, em.IDEM, em.IDA, em.DTENTITY, bd.IDA_ENTITY, bd.IDENTIFIER, bc.IDA_ENTITY,
            //ev.IDE_EVENT, ec.EVENTCLASS, ec.DTEVENT, isnull(em.ISAUTOABN, isnull(mk.ISAUTOABN, 0)), 0 as ISCUSTODIAN
            //from dbo.TRADE tr
            //inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            //inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            //inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            //inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            //inner join dbo.ENTITYMARKET em on (em.IDM = tr.IDM) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
            //inner join dbo.MARKET mk on (mk.IDM = tr.IDM)
            //inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE in ('ASD','EXD')) and (ev.EVENTTYPE in ('AME','EUR'))
            //inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            //where (em.IDEM = @IDEM) and (ec.EVENTCLASS in ('CSH','PHY')) and (ec.DTEVENT = @EXPIRYDATE) and 
            //(tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')", tableName.First, hintsOracle);
            string sqlInsert = String.Format(@"insert into dbo.{0}
            (IDT, IDENTIFIER, IDI, DTTRADE, IDM, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, 
            SIDE, PRICE, QTY, IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, BOOKDEALER_IDENTIFIER, IDA_ENTITYCLEARER, IDE_EVENT, EVENTCLASS, DTEVENT, ISAUTOABN, ISCUSTODIAN)
            select {1}
            tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE, tr.IDM, tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, 
            tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY, em.IDEM, em.IDA, em.DTENTITY, tr.IDA_ENTITY, bd.IDENTIFIER, bc.IDA_ENTITY,
            ev.IDE_EVENT, ec.EVENTCLASS, ec.DTEVENT, isnull(em.ISAUTOABN, isnull(mk.ISAUTOABN, 0)), 0 as ISCUSTODIAN
            from dbo.ENTITYMARKET em
            inner join dbo.TRADE tr on (tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.MARKET mk on (mk.IDM = em.IDM)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE in ('ASD','EXD')) and (ev.EVENTTYPE in ('AME','EUR'))
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE)
            where (em.IDEM = @IDEM) and (ec.EVENTCLASS in ('CSH','PHY')) and (ec.DTEVENT = @EXPIRYDATE) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and ((tr.DTOUT is null) or (tr.DTOUT > @EXPIRYDATE)) ", tableName.First, hintsOracle);

            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);

            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);
            
            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);


            string sqlSelect = "select 0 as SPID, ";
            if (DataHelper.IsDbSqlServer(pCS))
                sqlSelect = "select @@SPID as SPID, ";

            string fetchRestrictionForClosingDayControl = string.Empty;
            if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                fetchRestrictionForClosingDayControl = "offset 0 rows fetch next 5 rows only";

            sqlSelect += @"alloc.IDT, alloc.IDENTIFIER, alloc.IDI, alloc.IDEM, alloc.IDASSET, alloc.ASSETCATEGORY, 
            (alloc.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0)) as QTY,
            alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.DTENTITY, alloc.SIDE, alloc.PRICE, alloc.DTTRADE, 
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, 
            alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER, 
            alloc.ISAUTOABN, alloc.IDE_EVENT, alloc.EVENTCLASS, alloc.DTEVENT as EXPIRYDATE, 
            ac.IDENTIFIER as ACTORDEALER_IDENTIFIER, alloc.BOOKDEALER_IDENTIFIER, alloc.ISCUSTODIAN
            from dbo.{0} alloc
            {1}
            inner join dbo.ACTOR ac on (ac.IDA = alloc.IDA_DEALER)
            where (alloc.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0) > 0) 
            order by alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, 
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER
            {2}" + Cst.CrLf;

            return String.Format(sqlSelect, tableName.First, GetQryPosActionBasedWithTrade(tableName.First, "alloc"), fetchRestrictionForClosingDayControl);
        }
        #endregion GetQueryCandidatesToMaturityOffsettingOption
        #region GetQueryCandidatesToPhysicalPeriodicDelivery
        /// <summary>
        /// Retourne les négociations en position sur des contrats FUTURES avec livraison PHYSIQUE et PERIODIQUE,
        /// pour lesquels la 1ère date de règlement (FIRSTDLVSETTLTDATE) est égale à la date business (DTENTITY).
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20170206 [22787] New 
        // PL 20170413 Add filter (dc.SETTLTMETHOD = 'P') and (dc.PHYSETTLTAMOUNT != 'NA')
        // EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide 
        // EG 20170424 [23064] Calcul des livraisons périodiques sur Trade saisie en retard ou post FIRSTDLVSETTLTDATE
        // EG 20180803 PERF (Utilisation d'une clause WITH (+ table "temporaire" sur la base d'un table MODEL pour SQLServer)
        // EG 20180906 PERF No View & Add DTOUT
        // EG 20181010 PERF Refactoring
        // EG 20181119 PERF Correction post RC
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20201027 [24769] Ne retourne que les 5 premiers trades (CLOSINGDAY only - augmentation performances)
        // EG 20201118 [24769] Correction bug sur offset row fetch next (manque order)
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        protected override string GetQueryCandidatesToPhysicalPeriodicDelivery(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);
            bool isNoHints = DataHelper.GetSvrInfoConnection(pCS).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(pCS).IsNoHints;
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(tr IX_TRADE5) index(bd IX_BOOK2) index(ti PK_TRADEINSTRUMENT) */";

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("PDP");

            //string sqlInsert = String.Format(@"insert into dbo.{0}
            //(IDT, IDENTIFIER, IDI, DTTRADE, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, PRICE, QTY,
            //IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            //select {1}
            //tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE, 
            //tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY,
            //em.IDEM, em.IDA, em.DTENTITY, bd.IDA_ENTITY, bc.IDA_ENTITY
            //from dbo.TRADE tr
            //inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            //inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            //inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            //inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            //inner join dbo.ENTITYMARKET em on (em.IDM = tr.IDM) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
            //inner join dbo.ASSET_ETD ast on (ast.IDASSET = tr.IDASSET)
            //inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)
            //inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'F')
            //inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (em.DTENTITY between ma.FIRSTDLVSETTLTDATE and ma.LASTDLVSETTLTDATE)
            //where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            //(tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') ", tableName.First, hintsOracle);
            string sqlInsert = String.Format(@"insert into dbo.{0}
            (IDT, IDENTIFIER, IDI, DTTRADE, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, PRICE, QTY,
            IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            select {1}
            tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DTTRADE, 
            tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE, tr.PRICE, tr.QTY,
            em.IDEM, em.IDA, em.DTENTITY, tr.IDA_ENTITY, bc.IDA_ENTITY
            from dbo.ENTITYMARKET em            
            inner join dbo.TRADE tr on(tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ASSET_ETD ast on (ast.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'F')
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (em.DTENTITY between ma.FIRSTDLVSETTLTDATE and ma.LASTDLVSETTLTDATE)
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') ", tableName.First, hintsOracle);
            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);

            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            // EG 20230510 [26376][WI627] Ajout index
            if (false == tableName.Second)
            {
                Initialize_IndexTradeEODModel(tableName.First);
                DataHelper.ExecuteNonQuery(CS, CommandType.Text,
                    $"create index IX_{tableName.First}1 on dbo.{tableName.First} (IDI, IDASSET, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER)");
            }

            // FI 20190701 [24520] Add UpdateStatTable
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);

            string fetchRestrictionForClosingDayControl = string.Empty;
            if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                fetchRestrictionForClosingDayControl = "order by alloc.IDASSET offset 0 rows fetch next 5 rows only";

            string sqlSelect = @"select alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) as QTY_BUY,
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) as QTY_SELL,
            alloc.ASSETCATEGORY
            from dbo.{0} alloc
            {1}
            group by alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
                     alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, 
                     alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER, 
                     alloc.ASSETCATEGORY
            having 
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) > 0
            or
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) > 0
            {2}" + Cst.CrLf;

            return String.Format(sqlSelect, tableName.First, GetQryPosActionBasedWithTrade(tableName.First, "alloc"), fetchRestrictionForClosingDayControl);
        }
        #endregion GetQueryCandidatesToPhysicalPeriodicDelivery
        #region GetQueryCandidatesToPositionCascading
        /// <summary>
        /// Utilisée par le traitement de fin de journée (CASCADING)
        /// Retourne les positions arrivées à échéance et pour lesquelles il y a cascading (CONTRACTCASCADING)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// PM 20130212 [18414] Added for position Cascading
        /// EG 20140225 [19575][19666]
        /// PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        /// EG 20150910 [21336] Add ASSETCATEGORY to Query (and Group By)
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        /// EG 20201028 [XXXXX] Refactoring Query (Introduction Table de travail)
        /// EG 20201027 [24769] Ne retourne que les 5 premiers trades (CLOSINGDAY only - augmentation performances)
        /// EG 20201118 [24769] Correction bug sur offset row fetch next (manque order)
        // EG 20201207 [XXXXX] Performance V10 (ENTITYMARKET table majeure)
        protected override string GetQueryCandidatesToPositionCascading(string pCS)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(CS, "IDEM", DbType.Int32), m_MarketPosRequest.IdEM);
            dataParameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.Date), m_MarketPosRequest.DtBusiness);

            Pair<string, bool> tableName = Initialize_QueryTradeEODModel("CAS");

            //string sqlInsert = String.Format(@"insert into dbo.{0}
            //(IDT, IDI, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, 
            //QTY, IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            //select tr.IDT, tr.IDI, tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE,  
            //tr.QTY, em.IDEM, em.IDA, em.DTENTITY, bd.IDA_ENTITY, bc.IDA_ENTITY
            //from dbo.TRADE tr
            //inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            //inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            //inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            //inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            //inner join dbo.ENTITYMARKET em on (em.IDM = tr.IDM) and (em.IDA = bd.IDA_ENTITY) and (em.IDA_CUSTODIAN is null)
            //inner join dbo.ASSET_ETD ast on (ast.IDASSET = tr.IDASSET)
            //inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)
            //inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (isnull(ma.MATURITYDATE, ma.MATURITYDATESYS) = em.DTENTITY)
            //inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'F')
            //where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            //(tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
            //exists (select 1 from dbo.CONTRACTCASCADING cc where (cc.IDDC = da.IDDC) )", tableName.First);
            string sqlInsert = String.Format(@"insert into dbo.{0}
            (IDT, IDI, IDASSET, ASSETCATEGORY, IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, IDA_CSSCUSTODIAN, SIDE, 
            QTY, IDEM, IDA_ENTITY, DTENTITY, IDA_ENTITYDEALER, IDA_ENTITYCLEARER)
            select tr.IDT, tr.IDI, tr.IDASSET, tr.ASSETCATEGORY, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_CSSCUSTODIAN, tr.SIDE,  
            tr.QTY, em.IDEM, em.IDA, em.DTENTITY, tr.IDA_ENTITY, bc.IDA_ENTITY
            from dbo.ENTITYMARKET em
            inner join dbo.TRADE tr on (tr.IDM = em.IDM) and (tr.IDA_ENTITY = em.IDA)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on ( pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            inner join dbo.ASSET_ETD ast on (ast.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ast.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (isnull(ma.MATURITYDATE, ma.MATURITYDATESYS) = em.DTENTITY)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC) and (dc.CATEGORY = 'F')
            where (em.IDEM = @IDEM) and (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
            exists (select 1 from dbo.CONTRACTCASCADING cc where (cc.IDDC = da.IDDC) )", tableName.First);

            QueryParameters qryParameters = new QueryParameters(CS, sqlInsert, dataParameters);

            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == tableName.Second)
                Initialize_IndexTradeEODModel(tableName.First);

            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbORA == serverType)
                DataHelper.UpdateStatTable(pCS, tableName.First);

            string fetchRestrictionForClosingDayControl = string.Empty;
            if (m_MasterPosRequest.RequestType == Cst.PosRequestTypeEnum.ClosingDay)
                fetchRestrictionForClosingDayControl = "order by alloc.IDASSET offset 0 rows fetch next 5 rows only";

            string sqlSelect = String.Format(@"select alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY, 
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            sum(case when alloc.SIDE = 1 then alloc.QTY - isnull(pos.QTY_BUY,0) else 0 end) as QTY_BUY,
            sum(case when alloc.SIDE = 2 then alloc.QTY - isnull(pos.QTY_SELL,0) else 0 end) as QTY_SELL,
            alloc.ASSETCATEGORY, alloc.IDA_CSSCUSTODIAN as IDA_CSS, 0 as ISCUSTODIAN
            from dbo.{0} alloc
            {1}
            group by alloc.IDA_ENTITY, alloc.IDA_CSSCUSTODIAN, alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY,
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER, alloc.ASSETCATEGORY 
            having 
            sum(case when alloc.SIDE = 1 then (alloc.QTY - isnull(pos.QTY_BUY, 0)) else 0 end) > 0
            or
            sum(case when alloc.SIDE = 2 then (alloc.QTY - isnull(pos.QTY_SELL, 0)) else 0 end) > 0
            {2}" + Cst.CrLf,
            tableName.First, GetQryPosActionBasedWithTrade(tableName.First, "alloc"), fetchRestrictionForClosingDayControl);
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToPositionCascading
        #region GetQueryCandidatesToPositionShifting
        /// <summary>
        /// Utilisée par le traitement de clôture de journée (SHIFTING)
        /// Retourne les positions pour lesquelles il y a shifting (IDDC_SHIFT)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        /// PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        /// EG 20150910 [21336] Add ASSETCATEGORY to Query (and Group By)
        /// EG 20170221 [22879] 
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide 
        protected override string GetQueryCandidatesToPositionShifting(string pCS)
        {
            // RD 20180302 [23757] Utilisation de la récursivité pour chainer sur les DC se substituant successivement
            string sqlSelect = @"with DCShiftHierarchy (IDDC, IDDC_SHIFT, LEVEL_DC) 
            as (
                select dcshift.IDDC, dcshift.IDDC_SHIFT, 0 as LEVEL_DC
                from dbo.ENTITYMARKET em
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDM = em.IDM)
                inner join dbo.DERIVATIVECONTRACT dcshift on (dcshift.IDDC_SHIFT = dc.IDDC)
                left outer join dbo.DERIVATIVEATTRIB da on (da.IDDC = dc.IDDC)
                left outer join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY) and (ma.LASTTRADINGDAY = em.DTENTITY) and (ma.LASTTRADINGDAY = @DTBUSINESS)
                left outer join dbo.DERIVATIVEATTRIB dashift on (dashift.IDDC = dcshift.IDDC)
                left outer join dbo.MATURITY mashift on (mashift.IDMATURITY = dashift.IDMATURITY) and (mashift.LASTTRADINGDAY = em.DTENTITY) and (mashift.LASTTRADINGDAY = @DTBUSINESS)
                where (em.IDEM = @IDEM) and (dc.IDDC_SHIFT is null) and ((ma.IDMATURITY is not null) or (mashift.IDMATURITY is not null))
                
                union all

                select dcshift.IDDC, dcshift.IDDC_SHIFT, dc.LEVEL_DC + 1 as LEVEL_DC
                from dbo.DERIVATIVECONTRACT dcshift
                inner join DCShiftHierarchy dc on (dc.IDDC = dcshift.IDDC_SHIFT) and (dc.LEVEL_DC < 10)
            )
            select dc.LEVEL_DC, tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDEM, tr.IDI, tr.IDASSET, tr.DTENTITY, 
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER,
            sum(case when tr.SIDE = 1 then (tr.QTY - isnull(pab.QTY, 0) ) else 0 end) as QTY_BUY,
            sum(case when tr.SIDE = 2 then (tr.QTY - isnull(pas.QTY, 0) ) else 0 end) as QTY_SELL,
            tr.ASSETCATEGORY, tr.IDA_CSSCUSTODIAN as IDA_CSS, 0 as ISCUSTODIAN
            from dbo.VW_TRADE_POSETD tr
            inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)
            inner join DCShiftHierarchy dc on (dc.IDDC = ast.IDDC)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER) + @") pas on (pas.IDT = tr.IDT)
            where (tr.IDEM = @IDEM) and (tr.DTBUSINESS <= @DTBUSINESS) and (tr.POSKEEPBOOK_DEALER=1) 
            group by dc.LEVEL_DC, tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDEM, tr.IDI, tr.IDASSET, tr.DTENTITY,
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, tr.ASSETCATEGORY
            having
            sum(case when tr.SIDE = 1 then (tr.QTY - isnull(pab.QTY, 0)) else 0 end) > 0
            or
            sum(case when tr.SIDE = 2 then (tr.QTY - isnull(pas.QTY, 0)) else 0 end) > 0" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToPositionShifting
        #region GetQueryCandidatesToUnderlyerDelivery
        /// <summary>
        /// Utilisée par le traitement de livraison de sous-jacent post dénouement
        ///   Retourne les demandes de livraison du jour présentes dans POSREQUEST (UNLDLVR)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20140120 Report 3.7
        // EG 20141224 [20566] Refactoring
        // EG 20150114 Add Order By IDT
        protected override string GetQueryCandidatesToUnderlyerDelivery()
        {
            return @"select pr.IDT, pr.IDA_ENTITY, pr.IDA_CSSCUSTODIAN, pr.IDA_CSS, pr.IDA_CUSTODIAN, pr.IDEM, pr.DTBUSINESS, pr.IDPR, pr.QTY, pr.REQUESTDETAIL, pr.STATUS
            from dbo.POSREQUEST pr
            where (pr.IDEM = @IDEM) and (pr.DTBUSINESS = @DTBUSINESS) and (pr.REQUESTTYPE = 'UNLDLVR') and (pr.REQUESTMODE = 'EOD') and (pr.STATUS != 'SUCCESS')
            order by pr.IDT" + Cst.CrLf;
        }
        #endregion GetQueryCandidatesToUnderlyerDelivery
        #region GetQueryCandidatesToUTICalculation
        /// <summary>
        /// Utilisée par le traitement de fin de journée (UTI)
        /// Retourne les négociations candidates à calcul des UTI.
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // FI 20140624 [20125] Spheres ne prend pas en considération les trades pour lesquels la règle définie dans REGULATORY est 'NONE'
        // FI 20140602 [20023] Modification des expressions SQL des colonnes STATUSPUTI_DEAL, STATUSPUTI_CLEAR, modification du where
        // FI 20140919 [20353] Modify (Ajout des colonnes COMPARTCODE_CLEAR et STRIKEDECLOCATOR_DC et divers refactoring)
        // PL 20160502 [22107] EUREX C7 3.0 Release - Add RELTDPOSID (for ETD)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // FI 20240425 [26593] UTI/PUTI REFIT add column LEI_CSSCUST
        protected override string GetQueryCandidatesToUTICalculation(string pCS)
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            string _missing = "'{Missing'";
            string _missing_TradeId_Dealer = string.Empty;
            string _missing_TradeId_Clearer = string.Empty;
            if (DbSvrType.dbSQL == serverType)
            {
                _missing_TradeId_Dealer = @"charindex(" + _missing + ", taid_d.TRADEID) > 0";
                _missing_TradeId_Clearer = @"charindex(" + _missing + ", taid_c.TRADEID) > 0";
            }
            else if (DbSvrType.dbORA == serverType)
            {
                _missing_TradeId_Dealer = @"instr(taid_d.TRADEID, " + _missing + ") > 0";
                _missing_TradeId_Clearer = @"instr(taid_c.TRADEID, " + _missing + ") > 0";
            }


            string sqlSelect = SQLCst.SELECT + @"tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS, tr.DTTRADE, tr.EXECUTIONID, tr.RELTDPOSID, tr.ORDERID, tr.TRDTYPE,
            case when tr.SIDE = 1 then 'Buyer' else 'Seller' end as BUYER_SELLER,
            taid_d.TRADEID as UTI_DEAL, taid_c.TRADEID as UTI_CLEAR,
            putidet_d.UTI as PUTI_DEAL, putidet_c.UTI as PUTI_CLEAR,
            
            tr.IDI, mk.IDM, mk.ISO10383_ALPHA4, 
            tr.IDA_CSSCUSTODIAN as IDA_CSSCUST, a_csscust.IDENTIFIER as IDENTIFIER_CSSCUST, a_csscust.BIC as BIC_CSSCUST, a_csscust.ISO17442 as LEI_CSSCUST,

            dc.IDDC, dc.CONTRACTTYPE as CONTRACTTYPE_DC, dc.CATEGORY as CATEGORY_DC, dc.CONTRACTSYMBOL as CONTRACTSYMBOL_DC, dc.CONTRACTATTRIBUTE as CONTRACTATTRIBUTE_DC,
            dc.IDENTIFIER as IDENTIFIER_DC, dc.STRIKEDECLOCATOR as STRIKEDECLOCATOR_DC,

            ass.IDASSET, ass.IDENTIFIER as IDENTIFIER_ASSET, ass.PUTCALL as PUTCALL_ASSET,
            ass.STRIKEPRICE as STRIKEPRICE_ASSET,ass.ISINCODE as ISINCODE_ASSET,ass.CFICODE as CFICODE_ASSET,

            mat.MATURITYMONTHYEAR as MATURITYMONTHYEAR,isnull(mat.MATURITYDATESYS,mat.MATURITYDATE) as MATURITYDATE,            
            
            puti.IDPOSUTI, puti.IDT_OPENING,

            tr.IDA_ENTITY as IDA_ENT, a_e.IDENTIFIER as IDENTIFIER_ENT, a_e.ISO17442 as LEI_ENT,csmid.CSSMEMBERCODE as CSSMEMBERCODE_ENT,
            reg_e.EMIRETDISSUER as EMIRETDISSUER_ENT, reg_e.EMIRUTIETDRULE as EMIRUTIETDRULE_ENT, reg_e.EMIRPUTIETDRULE as EMIRPUTIETDRULE_ENT,

            tr.IDA_DEALER as IDA_DEAL,
            a_d.BIC as BIC_DEAL, a_d.ISO17442 as LEI_DEAL, case when bar_d.IDROLEACTOR is null then 0 else 1 end as ISCLIENT_DEAL,
            tr.IDB_DEALER as IDB_DEAL,

            tr.IDA_CLEARER as IDA_CLEAR,            
            a_c.BIC as BIC_CLEAR, a_c.ISO17442 as LEI_CLEAR,
            case when tr.IDA_CLEARER = tr.IDA_CSSCUSTODIAN then 1 else 0 end as ISCCP_CLEAR,
            tr.IDB_CLEARER as IDB_CLEAR,
            ccompart.COMPARTMENTCODE as COMPARTCODE_CLEAR,
            reg_c.EMIRETDISSUER as EMIRETDISSUER_CLEAR, reg_c.EMIRPUTIETDRULE as EMIRUTIETDRULE_CLEAR, reg_c.EMIRUTIETDRULE as EMIRPUTIETDRULE_CLEAR,
            
            case when taid_d.IDT is null then 'MISSING' when substring(isnull(taid_d.TRADEID,' '),1,1)=' ' or " + _missing_TradeId_Dealer + @" then 
            'INVALID' else 'OK' end as STATUSUTI_DEAL,
            case when taid_c.IDT is null then 'MISSING' when substring(isnull(taid_c.TRADEID,' '),1,1)=' ' or " + _missing_TradeId_Clearer + @" then 
            'INVALID' else 'OK' end as STATUSUTI_CLEAR, 
            case when putidet_d.IDPOSUTI is null then 'MISSING' when putidet_d.UTI='N/A' then 
            'INVALID' else 'OK' end as STATUSPUTI_DEAL,          
            case when putidet_c.IDPOSUTI is null then 'MISSING' when putidet_c.UTI='N/A' then 
            'INVALID' else 'OK' end as STATUSPUTI_CLEAR" + Cst.CrLf;

            sqlSelect += SQLCst.FROM_DBO + @"TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'exchangeTradedDerivative')
            inner join dbo.MARKET mk on (mk.IDM = tr.IDM)
            inner join dbo.ENTITYMARKET em on (em.IDA = tr.IDA_ENTITY) and (em.IDM = tr.IDM)
            inner join dbo.ACTOR a_csscust on (a_csscust.IDA=tr.IDA_CSSCUSTODIAN)
            inner join dbo.ASSET_ETD ass on (ass.IDASSET = tr.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ass.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MATURITY mat on (mat.IDMATURITY=da.IDMATURITY)
            
            inner join dbo.ACTOR a_e on (a_e.IDA = tr.IDA_ENTITY)
            left outer join dbo.REGULATORY reg_e on (reg_e.IDA = a_e.IDA) 

            inner join dbo.ACTOR a_d on (a_d.IDA = tr.IDA_DEALER)
            left outer join dbo.BOOKACTOR_R bar_d on (bar_d.IDB = tr.IDB_DEALER) and (bar_d.IDA_ACTOR = a_e.IDA) and (bar_d.IDROLEACTOR = 'CLIENT')

            inner join dbo.ACTOR a_c on (a_c.IDA=tr.IDA_CLEARER)
            left outer join dbo.REGULATORY reg_c on (reg_c.IDA = a_c.IDA)
            left outer join dbo.BOOK  b_c on (b_c.IDB=tr.IDB_CLEARER)
            left outer join dbo.CLEARINGCOMPART ccompart on (ccompart.IDA_CSS = tr.IDA_CSSCUSTODIAN and ccompart.IDA=b_c.IDA)
    
            left outer join dbo.CSMID csmid on (csmid.IDA = a_e.IDA) and (csmid.IDA_CSS = tr.IDA_CSSCUSTODIAN) and (isnull(csmid.IDM, mk.IDM) = mk.IDM )
            and (csmid.CSSMEMBERIDENT = case a_c.BIC when 'CCEGITRR' then 'ITIT' when 'EUXCDEFF' then 'ECAG' else '*' end)
            
            left outer join dbo.TRADEID taid_d on (taid_d.IDT = tr.IDT) and (taid_d.IDA = tr.IDA_DEALER)
            and taid_d.TRADEIDSCHEME='http://www.euro-finance-systems.fr/spheres/unique-transaction-identifier'
            left outer join dbo.TRADEID taid_c on (taid_c.IDT = tr.IDT) and (taid_c.IDA = tr.IDA_CLEARER) 
            and taid_c.TRADEIDSCHEME='http://www.euro-finance-systems.fr/spheres/unique-transaction-identifier'
            left outer join dbo.POSUTI puti on (puti.IDI = ns.IDI) and (puti.IDASSET = ass.IDASSET) 
            and (puti.IDA_DEALER = tr.IDA_DEALER) and (puti.IDB_DEALER = tr.IDB_DEALER) and (puti.IDA_CLEARER = tr.IDA_CLEARER) and (puti.IDB_CLEARER = tr.IDB_CLEARER)
            left outer join dbo.POSUTIDET putidet_d on (putidet_d.IDPOSUTI = puti.IDPOSUTI) and (putidet_d.IDA = puti.IDA_DEALER) 
            left outer join dbo.POSUTIDET putidet_c on (putidet_c.IDPOSUTI = puti.IDPOSUTI) and (putidet_c.IDA = puti.IDA_CLEARER)" + Cst.CrLf;

            sqlSelect += SQLCst.WHERE + @"(em.IDEM = @IDEM) and (tr.DTBUSINESS = @DTBUSINESS) and
            (tr.IDSTACTIVATION in ('REGULAR','LOCKED')) and (tr.IDSTENVIRONMENT = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and 
            (
                ((('OK' != case when taid_d.IDT is null then 'MISSING' when substring(isnull(taid_d.TRADEID,' '),1,1)=' ' or " + _missing_TradeId_Dealer + @" then 
                       'INVALID' else 'OK' end) and (isnull(reg_e.EMIRUTIETDRULE,'notSpecified') != 'NONE'))
                or
                (('OK' != case when taid_c.IDT is null then 'MISSING' when substring(isnull(taid_c.TRADEID,' '),1,1)=' ' or " + _missing_TradeId_Clearer + @" then 
                       'INVALID' else 'OK' end) and (isnull(reg_c.EMIRUTIETDRULE,'notSpecified') != 'NONE'))
                or
                (('OK' != case when putidet_d.UTI is null then 'MISSING' when putidet_d.UTI = 'N/A' then 
                       'INVALID' else 'OK' end) and (isnull(reg_e.EMIRPUTIETDRULE,'notSpecified') != 'NONE'))
                or
                (('OK' != case when putidet_c.UTI is null then 'MISSING' when putidet_c.UTI = 'N/A' then 
                       'INVALID' else 'OK' end) and (isnull(reg_c.EMIRPUTIETDRULE,'notSpecified') != 'NONE'))
                )
            )" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryCandidatesToUTICalculation
        #region GetQueryCascadingContract
        /// <summary>
        /// Retourn les contrats et échéances résultants du cascading de la position sur un asset donné @IDASSET 
        /// </summary>
        /// <returns></returns>
        // PM 20130212 [18414]
        protected override string GetQueryCascadingContract()
        {
            string sqlSelect = $@"select cc.IDDC, cc.MATURITYMONTH, m.MATURITYMONTHYEAR,
            cc.IDDC_CASC, dc.IDENTIFIER, cc.CASCMATURITYMONTH, cc.ISYEARINCREMENT
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
            inner join dbo.MATURITY m on (m.IDMATURITY = da.IDMATURITY)
            inner join dbo.MATURITYRULE mr on ((mr.IDMATURITYRULE = m.IDMATURITYRULE) and  (mr.MMYFMT = '{ReflectionTools.ConvertEnumToString<Cst.MaturityMonthYearFmtEnum>(Cst.MaturityMonthYearFmtEnum.YearMonthOnly)}'))
            inner join dbo.CONTRACTCASCADING cc on (cc.IDDC = da.IDDC)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = cc.IDDC_CASC)
            where (a.IDASSET = @IDASSET) and {OTCmlHelper.GetSQLDataDtEnabled(CS, "cc", DtBusiness)}";
            return sqlSelect;
        }
        #endregion GetQueryCascadingContract

        #region GetQueryInsertPriceControl_EOD
        /// <summary>
        /// Requête insertion des assets du jour pour contrôle des cotations
        /// </summary>
        /// <returns></returns>
        // EG 20141126 [20235] Ajout (tsys.IDSTACTIVATION = 'REGULAR') 
        // EG 20170222 [22717] Add IDM parameter
        // EG 20180906 PERF PERF & Add DTOUT (Alloc ETD only) 
        // EG 20180906 PERF Upd (Use With instruction and Temporary based to model table)
        // EG 20181119 PERF Correction post RC
        // EG 20181119 PERF Correction post RC (Step 2)
        // FI 20181126 [24338] Add join inner join dbo.BOOK bc on (bc.IDB = ti.IDB_CLEARER) (pour exlcure les trades sans book Clearer qui existent chez XI)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        protected override string GetQueryInsertPriceControl_EOD()
        {
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);

            string sqlInsert;

            if (DbSvrType.dbSQL == serverType)
            {
                sqlInsert = @"insert into dbo.TRADEASSETCTRL_MODEL
                select tr.IDT, tr.IDASSET, tr.DTBUSINESS, tr.QTY 
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.GPRODUCT = 'FUT')
                inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1) 
                inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
                where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
                (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
                (tr.IDA_CSSCUSTODIAN = @CLEARINGHOUSE) and (tr.IDM = @IDM) and ( bd.IDA_ENTITY = @ENTITY);

                With TRADEASSETCTRL AS
                (
	                SELECT * FROM dbo.TRADEASSETCTRL_MODEL
                )
                insert into dbo.ASSETCTRL_MODEL 
                (DTBUSINESS, IDASSET, CATEGORY, ATMATURITY, EXERCISESTYLE, IDASSET_UNL, CATEGORY_UNL)
                select @DTBUSINESS, a.IDASSET, a.CATEGORY, a.ATMATURITY, a.EXERCISESTYLE, a.IDASSET_UNL, a.CATEGORY_UNL 
                from (##SUBQUERY##) a;" + Cst.CrLf;
            }
            else
            {
                bool isNoHints = DataHelper.GetSvrInfoConnection(CS).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(CS).IsNoHints;
                string hintsOracle = isNoHints ? string.Empty : "/*+ index(tr IX_TRADE5) index(bd IX_BOOK2) */";
                sqlInsert = String.Format(@"insert into dbo.ASSETCTRL_MODEL
                (DTBUSINESS, IDASSET, CATEGORY, ATMATURITY, EXERCISESTYLE, IDASSET_UNL, CATEGORY_UNL)
                with TRADEASSETCTRL as
                (
                    select {0}
                    tr.IDT, tr.IDASSET, tr.DTBUSINESS, tr.QTY 
                    from dbo.TRADE tr
                    inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                    inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.GPRODUCT = 'FUT')
                    inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1) 
                    inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
                    where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESS) and (tr.DTBUSINESS <= @DTBUSINESS) and 
                    (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and
                    (tr.IDA_CSSCUSTODIAN = @CLEARINGHOUSE) and (tr.IDM = @IDM) and ( bd.IDA_ENTITY = @ENTITY)
                )
                select @DTBUSINESS, a.IDASSET, a.CATEGORY, a.ATMATURITY, a.EXERCISESTYLE, a.IDASSET_UNL, a.CATEGORY_UNL 
                from (##SUBQUERY##) a;", hintsOracle) + Cst.CrLf;
            }

            string sqlSelect = @"select asset.IDASSET, dc.CATEGORY as CATEGORY, dc.EXERCISESTYLE as EXERCISESTYLE, dc.ASSETCATEGORY as CATEGORY_UNL,
            case when ma.MATURITYDATE = @DTBUSINESS then 1 else 0 end as ATMATURITY, 
            case when  dc.ASSETCATEGORY = 'Future' then da.IDASSET else dc.IDASSET_UNL end as IDASSET_UNL
            from 
            (
		        select distinct alloc.IDASSET
		        from TRADEASSETCTRL alloc
		        left outer join
		        (
			        select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
			        from TRADEASSETCTRL alloc 
			        inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
			        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                    where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
			        group by pad.IDT_BUY
        
			        union all
        
			        select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
			        from TRADEASSETCTRL alloc 
			        inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
			        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                    where (pa.DTOUT is null or pa.DTOUT > @DTBUSINESS) and (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
			        group by pad.IDT_SELL
		        ) pos on (pos.IDT = alloc.IDT)
		        where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0) > 0) or (alloc.DTBUSINESS = convert(datetime,@DTBUSINESS,126))
	        ) asset     
            inner join dbo.ASSET_ETD etd on (etd.IDASSET = asset.IDASSET) 
            and (etd.DTENABLED <= convert(datetime,@DTBUSINESS,126)) 
            and (isnull(etd.DTDISABLED,convert(datetime,@DTBUSINESS,126)+1) > convert(datetime,@DTBUSINESS,126)) 
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = etd.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MARKET m on (m.IDM = dc.IDM)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            where (isnull(ma.MATURITYDATE,convert(datetime,@DTBUSINESS,126)) >= convert(datetime,@DTBUSINESS,126)) and
            (isnull(m.IDA,-1) = @CLEARINGHOUSE) and (isnull(m.IDM,-1) = @IDM)";

            sqlInsert = sqlInsert.Replace("##SUBQUERY##", sqlSelect);
            return sqlInsert;
        }
        #endregion GetQueryInsertPriceControl_EOD

        #region GetQueryMandatoryPosKeep_Calc_Cond2
        /// <summary>
        /// Utilisé dans le traitement de mise à jour des clôtures en mode STP (Entry) 
        ///     Retourne la requête qui détermine si:
        ///     Pour un OPEN, il existe en portefeuille une négociation de sens inverse en CLOSE, postérieure.
        /// </summary>
        /// <returns></returns>
        // EG 20151125 [20979] Refactoring : Add ASSETCATEGORY
        /// EG 20171016 [23509] DTEXECUTION remplace DTTIMESTAMP
        protected override string GetQueryMandatoryPosKeep_Calc_Cond2()
        {
            string sqlSelect = @"select 1
            from dbo.VW_TRADE_POSETD tr
            where (tr.IDT <> @IDT) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and (tr.DTEXECUTION > @DTEXECUTION) and 
            (tr.IDA_DEALER = @IDA_DEALER) and (tr.IDB_DEALER = @IDB_DEALER) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER,0) = @IDB_CLEARER) and 
            (tr.SIDE <> @SIDE) and (tr.POSITIONEFFECT = '" + ExchangeTradedDerivativeTools.GetPositionEffect_Close() + @"')" + Cst.CrLf;
            return sqlSelect;
        }
        #endregion GetQueryMandatoryPosKeep_Calc_Cond2

        #region GetQueryPosKeepingData
        /// <summary>
        /// Utilisée par les traitements ETD de tenue de position pour :
        ///    Recupérer pour alimenter les données de travail pour les traitement 
        ///    clé de position, actif, ...
        /// </summary>
        /// <param name="pPosRequestAssetQuote"></param>
        /// <returns></returns>
        /// FI 20110816 [17548] ajout lecture de la colonne IDC et NOMINALCURRENCY et PRICECURRENCY
        /// PM 20130807 [18876] Add PRICEQUOTEMETHOD for Variable Tick Value
        /// EG 20140115 [19456] Add MATURITYDATESYS, EXERCISESTYLE, FINALSETTLTSIDE et FINALSETTLTTIME
        /// EG 20151125 [20979] Refactoring
        protected override string GetQueryPosKeepingData(Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            string sqlQuery;
            if (IsPosKeepingByTrade)
                sqlQuery = GetQueryPosKeepingDataTrade(Cst.UnderlyingAsset.ExchangeTradedContract, pPosRequestAssetQuote);
            else
                sqlQuery = GetQueryPosKeepingDataAsset(Cst.UnderlyingAsset.ExchangeTradedContract, pPosRequestAssetQuote);
            return sqlQuery;
        }
        #endregion GetQueryPosKeepingData
        #region GetQueryPosKeepingDataAsset
        // EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
        // PM 20141120 [20508] Ajout de CASHFLOWCALCMETHOD
        // EG 20160302 [21969]
        // EG 20170206 [22787] Add Columns
        // FI 20170303 [22916] Modify 
        // PL 20170411 [23064] Modify 
        protected override string GetQueryPosKeepingDataAsset(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            // FI 20170303 [22916] Add column ITMCONDITION
            // PL 20170411 [23064] Add column PHYSETTLTAMOUNT
            string sqlSelect = @"select ast.IDASSET, ast.IDENTIFIER  as ASSET_IDENTIFIER, ast.MATURITYDATE, ast.MATURITYDATESYS, ast.CATEGORY, 
            ast.EXERCISESTYLE, ast.FUTVALUATIONMETHOD, ast.NOMINALVALUE, ast.NOMINALCURRENCY,ast.PRICECURRENCY, ast.IDC, ast.STRIKEPRICE, 
            ast.CONTRACTMULTIPLIER, ast.INSTRUMENTNUM, ast.INSTRUMENTDEN, ast.IDBC, ast.PUTCALL, ast.FACTOR, ast.SETTLTMETHOD, ast.PHYSETTLTAMOUNT, 
            ast.DELIVERYDATE, ast.PERIODMLTPDELIVERYDATEOFFSET, ast.PERIODDELIVERYDATEOFFSET, ast.DAYTYPEDELIVERYDATEOFFSET, 
            'ExchangeTradedContract' as ASSETCATEGORY, ast.IDDC_UNL, ast.IDASSET_UNL,
            ast.LASTTRADINGDAY, ast.FINALSETTLTPRICE, ast.PRICEQUOTEMETHOD, ast.FINALSETTLTSIDE, ast.FINALSETTLTTIME, ast.CASHFLOWCALCMETHOD,
            ast.UNITOFMEASURE, isnull(ast.UNITOFMEASUREQTY,1) as UNITOFMEASUREQTY,
            ast.FIRSTDELIVERYDATE, ast.LASTDELIVERYDATE, ast.DELIVERYTIMESTART, ast.DELIVERYTIMEEND, ast.DELIVERYTIMEZONE, ast.ISAPPLYSUMMERTIME,
            ast.PERIODMLTPDELIVERY, ast.PERIODDELIVERY, ast.DAYTYPEDELIVERY, ast.ROLLCONVDELIVERY,
            ast.PERIODMLTPDLVSETTLTOFFSET, ast.PERIODDLVSETTLTOFFSET, ast.DAYTYPEDLVSETTLTOFFSET,
            ast.SETTLTOFHOLIDAYDLVCONVENTION, ast.FIRSTDLVSETTLTDATE, ast.LASTDLVSETTLTDATE,
            ast.ITMCONDITION" + Cst.CrLf;

            if (Cst.PosRequestAssetQuoteEnum.UnderlyerAsset == pPosRequestAssetQuote)
            {
                // EG 20160302 (21969] ast.ASSETCATEGORY as ASSETCATEGORY_UNDERLYER
                sqlSelect += @", ast_unl.IDASSET as IDASSET_UNDERLYER, ast_unl.IDENTIFIER as IDENTIFIER_UNDERLYER, ast.ASSETCATEGORY as ASSETCATEGORY_UNDERLYER
                from dbo.VW_ASSET_ETD_EXPANDED ast
                left outer join dbo.VW_ASSET ast_unl on (ast_unl.ASSETCATEGORY=ast.ASSETCATEGORY) and 
                (((ast.ASSETCATEGORY<>'Future') and (ast_unl.IDASSET = ast.IDASSET_UNL)) or
                ((ast.ASSETCATEGORY='Future') and (ast_unl.IDASSET = ast.DA_IDASSET)))
                where (ast.IDASSET = @ID)";
            }
            else
            {
                sqlSelect += @"from dbo.VW_ASSET_ETD_EXPANDED ast
                where (ast.IDASSET = @ID)" + Cst.CrLf;
            }
            return sqlSelect;
        }
        #endregion GetQueryPosKeepingDataAsset
        #region GetQueryPosKeepingDataTrade
        /// <summary>
        ///  Requête de sélection des données utiles à partir d'un trade donné / d'un asset ... 
        /// </summary>
        /// <returns></returns>
        /// FI 20110816 [17548] ajout lecture de la colonne IDC et NOMINALCURRENCY et PRICECURRENCY
        /// PM 20130807 [18876] Add PRICEQUOTEMETHOD for Variable Tick Value
        /// EG 20140115 [19456] Add MATURITYDATESYS, EXERCISESTYLE, FINALSETTLTSIDE et FINALSETTLTTIME
        /// EG 20141014 Gestion ASSETCATEGORY sur ETD (+ sous-jacent)
        /// PM 20141120 [20508] Ajout de CASHFLOWCALCMETHOD
        /// EG 20150716 [21103] Add tr.DTSETTLT 
        /// EG 20160302 (21969]
        /// FI 20170303 [22916] Modify 
        /// PL 20170411 [23064] Modify 
        /// EG 20171016 [23509] Add DTEXECUTION, TZFACILITY
        protected override string GetQueryPosKeepingDataTrade(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            // FI 20170303 [22916] Add column ITMCONDITION
            // PL 20170411 [23064] Add column PHYSETTLTAMOUNT
            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, tr.SIDE, tr.DTBUSINESS, tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, 
            tr.POSITIONEFFECT, tr.IDI, tr.IDM, tr.IDA_ENTITY, tr.IDA_CSSCUSTODIAN, tr.IDEM, tr.DTMARKET, tr.DTMARKETPREV, tr.DTMARKETNEXT,
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, 
            tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, 
            tr.POSKEEPBOOK_DEALER, tr.QTY, tr.IDSTACTIVATION, tr.IDSTBUSINESS,
            ast.IDASSET, ast.IDENTIFIER  as ASSET_IDENTIFIER, ast.MATURITYDATE, ast.MATURITYDATESYS, ast.CATEGORY, 
            ast.EXERCISESTYLE, ast.FUTVALUATIONMETHOD, ast.NOMINALVALUE,ast.NOMINALCURRENCY,ast.PRICECURRENCY, ast.IDC, ast.STRIKEPRICE, 
            ast.CONTRACTMULTIPLIER, ast.INSTRUMENTNUM, ast.INSTRUMENTDEN, ast.IDBC, ast.PUTCALL, ast.FACTOR, ast.SETTLTMETHOD, ast.PHYSETTLTAMOUNT,
            ast.DELIVERYDATE, ast.PERIODMLTPDELIVERYDATEOFFSET, ast.PERIODDELIVERYDATEOFFSET, ast.DAYTYPEDELIVERYDATEOFFSET, 
            tr.ASSETCATEGORY, ast.IDDC_UNL, ast.IDASSET_UNL,
            ast.LASTTRADINGDAY, ast.FINALSETTLTPRICE, ast.PRICEQUOTEMETHOD, ast.FINALSETTLTSIDE, ast.FINALSETTLTTIME, tr.DTSETTLT, ast.CASHFLOWCALCMETHOD,
            ast.ITMCONDITION" + Cst.CrLf;

            if (Cst.PosRequestAssetQuoteEnum.UnderlyerAsset == pPosRequestAssetQuote)
            {
                // EG 20160302 (21969] ast.ASSETCATEGORY as ASSETCATEGORY_UNDERLYER
                sqlSelect += @", ast_unl.IDASSET as IDASSET_UNDERLYER, ast_unl.IDENTIFIER as IDENTIFIER_UNDERLYER, ast.ASSETCATEGORY as ASSETCATEGORY_UNDERLYER
                from dbo.VW_TRADE_FUNGIBLE_ETD tr
                inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)
                left outer join dbo.VW_ASSET ast_unl on (ast_unl.ASSETCATEGORY=ast.ASSETCATEGORY) and 
                (((ast.ASSETCATEGORY<>'Future') and (ast_unl.IDASSET = ast.IDASSET_UNL)) or
                ((ast.ASSETCATEGORY='Future') and (ast_unl.IDASSET = ast.DA_IDASSET)))
                where (tr.IDT = @ID)";
            }
            else
            {
                sqlSelect += @"from dbo.VW_TRADE_FUNGIBLE_ETD tr
                inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)
                where (tr.IDT = @ID)" + Cst.CrLf;
            }
            return sqlSelect;
        }
        #endregion GetQueryPosKeepingDataTrade
        #region GetQuerySelectPriceControl_EOD
        /// <summary>
        /// Requête select des assets du jour  pour contrôle des cotations
        /// </summary>
        /// <returns></returns>
        protected override string GetQuerySelectPriceControl_EOD()
        {
            #region Join table ASSET_XXX et QUOTE_XXX
            string sqlJoin = @"inner join dbo.##ASSET## asset on (asset.IDASSET = a.IDASSET)
            inner join dbo.QUOTE_##ASSETTYPE##_H q on (q.IDASSET = asset.IDASSET) and 
            (convert(varchar,q.TIME,112)=convert(varchar,convert(datetime,@DTBUSINESS,112),112)) and
            ((q.QUOTESIDE= a.QUOTESIDE) or 
            (isnull(q.QUOTESIDE,'OfficialClose') = case when a.QUOTESIDE = 'OfficialClose' then 'OfficialClose' else 'xxx' end)) and 
            (isnull(q.QUOTETIMING,'Close') = 'Close') and 
            (q.IDMARKETENV='DEFAULT_MARKET_ENV') and (q.IDVALSCENARIO='EOD_VALUATION') and (q.ISENABLED = 1)" + Cst.CrLf;
            #endregion Join table ASSET_XXX et QUOTE_XXX

            #region Requête contrôle existence Prix
            string sqlSelect = @"select 1 from (" + Cst.CrLf;

            #region ETD
            // 1.Prix OfficialClose des assets futures non arrivés à échéance
            // 2.Prix OfficialSettlement des assets futures arrivés à échéance
            // 3.Prix OfficialClose des assets options non arrivés à échéance
            // 4.Prix OfficialClose des sous-jacents de type Future des assets option non arrivés à échéance de type Americaine
            // 5.Prix OfficialSettlement des sous-jacent de type Future des assets option arrivés à échéance 
            sqlSelect += @"select a.IDASSET, 'ETD' as ASSET_TYPE 
            from 
            (
                select IDASSET as IDASSET, 'OfficialClose' as QUOTESIDE 
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=0) and (CATEGORY='F') 
                
                union
                
                select IDASSET as IDASSET, 'OfficialSettlement' as QUOTESIDE 
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=1) and (CATEGORY='F')
                
                union
                    
                select IDASSET as IDASSET, 'OfficialClose' as QUOTESIDE  
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=0) and (CATEGORY='O')
                
                union
                
                select IDASSET_UNL as IDASSET, 'OfficialClose' as QUOTESIDE  
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=0) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='Future') and (EXERCISESTYLE=1)
                
                union
                
                select IDASSET_UNL as IDASSET, 'OfficialSettlement' as QUOTESIDE 
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=1) and (CATEGORY='O')  and (IDASSET_UNL is not null) and (CATEGORY_UNL='Future')
            ) a" + Cst.CrLf;
            sqlSelect += sqlJoin.Replace("##ASSET##", "VW_ASSET_ETD_EXPANDED").Replace("##ASSETTYPE##", "ETD") + Cst.CrLf;
            #endregion ETD

            sqlSelect += "union" + Cst.CrLf;

            #region INDEX
            // 6.Prix OfficialClose des sous-jacent de type Index des assets option non arrivé à échéance de type Americaine
            // 7. Prix OfficialSettlement des assets futures arrivés à échéance
            sqlSelect += @"select a.IDASSET, 'INDEX' as ASSET_TYPE 
            from 
            (
                select IDASSET_UNL as IDASSET, 'OfficialClose' as QUOTESIDE 
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=0) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='Index') and (EXERCISESTYLE=1)
                
                union
                
                select IDASSET_UNL as IDASSET, 'OfficialSettlement' as QUOTESIDE  
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=1) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='Index')
            ) a" + Cst.CrLf;
            sqlSelect += sqlJoin.Replace("##ASSET##", "ASSET_INDEX").Replace("##ASSETTYPE##", "INDEX") + Cst.CrLf;
            #endregion INDEX

            sqlSelect += "union" + Cst.CrLf;

            #region EQUITY
            // 8. Prix OfficialClose des sous-jacent de type EquityAsset des assets option non arrivé à échéance de type Americaine
            // 9. Prix OfficialSettlement des sous-jacent de type EquityAsset des assets option arrivés à échéance
            sqlSelect += @"select a.IDASSET, 'EQUITY' as ASSET_TYPE 
            from 
            (
                select IDASSET_UNL as IDASSET, 'OfficialClose' as QUOTESIDE 
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=0) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='EquityAsset') and (EXERCISESTYLE=1)
                
                union
                
                select IDASSET_UNL as IDASSET, 'OfficialSettlement' as QUOTESIDE  
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=1) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='EquityAsset')
            ) a" + Cst.CrLf;
            sqlSelect += sqlJoin.Replace("##ASSET##", "ASSET_EQUITY").Replace("##ASSETTYPE##", "EQUITY") + Cst.CrLf;
            #endregion EQUITY

            sqlSelect += "union" + Cst.CrLf;

            #region COMMODITY
            // 10. Prix OfficialClose des sous-jacent de type Commodity des assets option non arrivé à échéance de type Americaine
            // 11. Prix OfficialSettlement des sous-jacent de type Commodity des assets option arrivés à échéance */
            sqlSelect += @"select a.IDASSET, 'COMMODITY' as ASSET_TYPE 
            from 
            (
                select IDASSET_UNL as IDASSET,  'OfficialClose' as QUOTESIDE  
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=0) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='Commodity') and (EXERCISESTYLE=1)
                
                union
                
                select IDASSET_UNL as IDASSET,  'OfficialSettlement' as QUOTESIDE  
                from dbo.ASSETCTRL_MODEL 
                where ##WHEREASSETCONTROL## and (ATMATURITY=1) and (CATEGORY='O') and (IDASSET_UNL is not null) and (CATEGORY_UNL='Commodity')
            ) a" + Cst.CrLf;
            sqlSelect += sqlJoin.Replace("##ASSET##", "ASSET_COMMODITY").Replace("##ASSETTYPE##", "COMMODITY") + Cst.CrLf;
            #endregion COMMODITY

            sqlSelect += @") result" + Cst.CrLf;

            #region Clause where ASSETCTRL_MODEL
            sqlSelect = sqlSelect.Replace("##WHEREASSETCONTROL##", "(DTBUSINESS = @DTBUSINESS)");
            #endregion Clause where ASSETCTRL_MODEL


            #endregion Requête contrôle existence Prix

            return sqlSelect;
        }
        #endregion GetQuerySelectPriceControl_EOD
        #region GetQueryShiftingContract
        // PM 20130307 [18434]
        /// <summary>
        /// Fournit la requête permettant d'obtenir le contrat résultant du shifting de la position
        /// sur un asset @IDASSET devant être fournit en paramètre à la requête
        /// </summary>
        /// <returns>La requête</returns>
        protected override string GetQueryShiftingContract()
        {
            string sqlSelect = @"select dc.IDDC_SHIFT, cs.IDENTIFIER
            from dbo.ASSET_ETD a 
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.DERIVATIVECONTRACT cs on (cs.IDDC = dc.IDDC_SHIFT)
            where (a.IDASSET = @IDASSET) and " + OTCmlHelper.GetSQLDataDtEnabled(CS, "cs", DtBusiness);
            return sqlSelect;
        }
        #endregion GetQueryShiftingContract
        #region GetQueryTradeCandidatesToMaturityOffsettingFuture
        /// <summary>
        /// Lecture des Contrats en position liquidable (MaturityDate inférieur ou égale à pDate)
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pPosKeepingKey"></param>
        /// <returns></returns>
        //FI 20110817 [17548] ajout de la devise d'expression du prix (PRICECURRENCY)
        /// EG 20130701 on lit désormais les contrats échus et non encore liquidés 
        // EG 20141224 [20566] Refactoring 
        // PM 20150601 [20575] Utilisation de DTENTITY au lieu de DTMARKET
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        // EG 20180803 PERF (Utilisation d'une clause WITH (+ table "temporaire" sur la base d'un table MODEL pour SQLServer) 
        // EG 20180906 PERF Add DTOUT (Alloc ETD only) 
        // EG 20180906 PERF Upd (Use With instruction and Temporary based to model table)
        // EG 20181010 PERF Refactoring
        protected override QueryParameters GetQueryTradeCandidatesToMaturityOffsettingFuture(DateTime pDate, IPosKeepingKey pPosKeepingKey)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), pPosKeepingKey.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pPosKeepingKey.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), pPosKeepingKey.IdA_Dealer);
            parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), pPosKeepingKey.IdB_Dealer);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), pPosKeepingKey.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), pPosKeepingKey.IdB_Clearer);

            string tableName = StrFunc.AppendFormat("TRADE_MOF_{0}_W", PKGenProcess.Session.BuildTableId()).ToUpper();
            string sqlQuery = String.Format(@"select 
            alloc.IDT, alloc.IDENTIFIER, (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) as QTY, 
            alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY, 
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            alloc.SIDE, alloc.PRICE, ev.IDE as IDE_EVENT,
            ast.MATURITYDATE,ast.CATEGORY, ast.NOMINALVALUE, ast.NOMINALCURRENCY, ast.PRICECURRENCY, ast.IDC,
            ast.STRIKEPRICE, ast.CONTRACTMULTIPLIER, ast.INSTRUMENTNUM, ast.INSTRUMENTDEN, ast.IDBC,
            ast.PUTCALL, ast.ASSETCATEGORY, ast.FACTOR, ast.IDENTIFIER as ASSET_IDENTIFIER
            from dbo.{0} alloc
            {1}
            inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = alloc.IDASSET)
            inner join dbo.EVENT ev on (ev.IDT = alloc.IDT) and (ev.EVENTTYPE = 'FUT')
            where (alloc.IDA_DEALER = @IDA_DEALER) and (alloc.IDB_DEALER = @IDB_DEALER) and 
                  (alloc.IDA_CLEARER = @IDA_CLEARER) and (alloc.IDB_CLEARER = @IDB_CLEARER) and 
                  (alloc.IDI = @IDI) and (alloc.IDASSET = @IDASSET) and 
                  (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0) <> 0)", tableName, GetQryPosActionBasedWithTrade(tableName, "alloc")) + Cst.CrLf;

            return new QueryParameters(CS, sqlQuery, parameters);
        }
        #endregion GetQueryTradeCandidatesToMaturityOffsettingFuture
        #region GetQueryTradeCandidatesToCascadingOrShifting
        /// EG 20170221 [22879] New
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide 
        protected QueryParameters GetQueryTradeCandidatesToCascadingOrShifting(IPosRequest pPosRequest)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pPosRequest.DtBusiness);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), pPosRequest.PosKeepingKey.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pPosRequest.PosKeepingKey.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), pPosRequest.PosKeepingKey.IdA_Dealer);
            parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), pPosRequest.PosKeepingKey.IdB_Dealer);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), pPosRequest.PosKeepingKey.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), pPosRequest.PosKeepingKey.IdB_Clearer);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER, (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0)) as QTY, 
            tr.IDEM, tr.IDI, tr.IDASSET, tr.DTMARKET, tr.DTENTITY, 
            tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_ENTITYDEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDA_ENTITYCLEARER, 
            tr.SIDE, tr.PRICE, ev.IDE as IDE_EVENT, 
            ast.MATURITYDATE,ast.CATEGORY, ast.NOMINALVALUE, ast.NOMINALCURRENCY, ast.PRICECURRENCY, ast.IDC, 
            ast.STRIKEPRICE, ast.CONTRACTMULTIPLIER, ast.INSTRUMENTNUM, ast.INSTRUMENTDEN, ast.IDBC, 
            ast.PUTCALL, ast.ASSETCATEGORY, ast.FACTOR, ast.IDENTIFIER as ASSET_IDENTIFIER 
            from dbo.VW_TRADE_POSETD tr
            inner join dbo.VW_ASSET_ETD_EXPANDED ast on (ast.IDASSET = tr.IDASSET)" + Cst.CrLf;
            if (pPosRequest.RequestType == Cst.PosRequestTypeEnum.Cascading)
                sqlSelect += @"and (ast.MATURITYDATE = tr.DTENTITY)" + Cst.CrLf;
            sqlSelect += @"inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and ((ev.EVENTTYPE = 'FUT') or (ev.EVENTCODE in ('ASD','EXD')))
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.BUYER) + @") pab on (pab.IDT = tr.IDT)
            left outer join (" + PosKeepingTools.GetQryPosAction_BySide(BuyerSellerEnum.SELLER) + @") pas on (pas.IDT = tr.IDT)
            where (tr.DTTRADE <= @DTBUSINESS) and (tr.IDI = @IDI) and (tr.IDASSET = @IDASSET) and 
            (tr.IDA_DEALER = @IDA_DEALER) and (tr.IDB_DEALER = @IDB_DEALER) and 
            (tr.IDA_CLEARER = @IDA_CLEARER) and (isnull(tr.IDB_CLEARER,0) = @IDB_CLEARER) and 
            (tr.POSKEEPBOOK_DEALER=1) and (tr.QTY - isnull(pas.QTY,0) - isnull(pab.QTY,0) <> 0)" + Cst.CrLf;
            return new QueryParameters(CS, sqlSelect, parameters);
        }
        #endregion GetQueryTradeCandidatesToCascadingOrShifting

        #region GetQueryTradeCandidatesToPhysicalPeriodicDelivery
        /// <summary>
        /// Lecture des Contrats en position avec livraison periodique
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pPosKeepingKey"></param>
        /// <returns></returns>
        // EG 20170206 [22787]
        /// EG 20170410 [23081] Refactoring Replace GetQueryPositionActionBySide by GetQryPosAction_BySide
        /// EG 20170424 [23064] Calcul des livraisons périodiques sur Trade saisie en retard ou post FIRSTDLVSETTLTDATE
        // EG 20180803 PERF (Utilisation d'une clause WITH (+ table "temporaire" sur la base d'un table MODEL pour SQLServer) 
        // EG 20180906 PERF Add DTOUT (Alloc ETD only) 
        // EG 20181010 PERF Refactoring
        // EG 20230510 [26376][WI627] Ajout restrictions manquantes sur parameters
        protected override QueryParameters GetQueryTradeCandidatesToPhysicalPeriodicDelivery(DateTime pDate, IPosKeepingKey pPosKeepingKey)
        {
            // Les événements des trades candidats à livraison périodique sortis de la position sont supprimés 
            // par le traitement de mise à jour des clôtures. 
            // Une jointure sur la table EVENT|EVENTDET est opérée via EVENTCODE = DLV pour récupèrer la quantité utilisée lors 
            // de l'éventuelle génération des événements de livraison périodiques dans un traitement précédent. 
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), pDate);// FI 20201006 [XXXXX] DbType.Date
            parameters.Add(new DataParameter(CS, "IDI", DbType.Int32), pPosKeepingKey.IdI);
            parameters.Add(new DataParameter(CS, "IDASSET", DbType.Int32), pPosKeepingKey.IdAsset);
            parameters.Add(new DataParameter(CS, "IDA_DEALER", DbType.Int32), pPosKeepingKey.IdA_Dealer);
            parameters.Add(new DataParameter(CS, "IDB_DEALER", DbType.Int32), pPosKeepingKey.IdB_Dealer);
            parameters.Add(new DataParameter(CS, "IDA_CLEARER", DbType.Int32), pPosKeepingKey.IdA_Clearer);
            parameters.Add(new DataParameter(CS, "IDB_CLEARER", DbType.Int32), pPosKeepingKey.IdB_Clearer);

            string tableName = StrFunc.AppendFormat("TRADE_PDP_{0}_W", PKGenProcess.Session.BuildTableId()).ToUpper();

            string sqlQuery = String.Format(@"select 
            alloc.IDT, alloc.IDENTIFIER, (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) as QTY, 
            alloc.IDEM, alloc.IDI, alloc.IDASSET, alloc.DTENTITY, 
            alloc.IDA_DEALER, alloc.IDB_DEALER, alloc.IDA_ENTITYDEALER, alloc.IDA_CLEARER, alloc.IDB_CLEARER, alloc.IDA_ENTITYCLEARER,
            alloc.SIDE, alloc.PRICE, 
            isnull(ma.MATURITYDATE, ma.MATURITYDATESYS) as MATURITYDATE, 
            dc.CATEGORY, dc.NOMINALVALUE, dc.IDC_NOMINAL as NOMINALCURRENCY, dc.IDC_PRICE as PRICECURRENCY,
            isnull(c.IDCQUOTED, c.IDC) as IDC,
            a.STRIKEPRICE, 
            (case when a.CONTRACTMULTIPLIER is null 
                    then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end)
                    else a.CONTRACTMULTIPLIER end)
            / 
            (case when (c.IDC != isnull(c.IDCQUOTED, c.IDC)) and ((isnull(c.FACTOR, 1)) > 0)
                    then isnull(c.FACTOR, 1)
                    else 1 end) as CONTRACTMULTIPLIER,
            dc.INSTRUMENTNUM,
            dc.INSTRUMENTDEN,
            mk.IDBC,
            a.PUTCALL,
            dc.ASSETCATEGORY,
            (case when a.FACTOR is null 
                    then (case when da.FACTOR is null then dc.FACTOR else da.FACTOR end)
                    else a.FACTOR end) as FACTOR,
            a.IDENTIFIER as ASSET_IDENTIFIER, 
            ev.IDE as IDE_EVENT, evddlv.DAILYQUANTITY as DELIVERYQTY
            from dbo.{0} alloc
            {1}
            inner join dbo.ASSET_ETD a on (a.IDASSET = alloc.IDASSET)
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.CURRENCY c on (c.IDC = dc.IDC_PRICE)
            inner join dbo.MARKET mk on (mk.IDM = dc.IDM)
            inner join dbo.EVENT ev on (ev.IDT = alloc.IDT) and (ev.EVENTTYPE = 'FUT')
            left outer join dbo.EVENT evdlv on (evdlv.IDT = alloc.IDT) and (evdlv.EVENTCODE = 'DLV')
            left outer join dbo.EVENTDET evddlv on (evddlv.IDE = evdlv.IDE)
            where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0) <> 0) and
            (alloc.IDI = @IDI) and (alloc.IDASSET = @IDASSET) and 
            (alloc.IDA_DEALER = @IDA_DEALER) and (alloc.IDB_DEALER = @IDB_DEALER) and
            (alloc.IDA_CLEARER = @IDA_CLEARER) and (alloc.IDB_CLEARER = @IDB_CLEARER)",
            tableName, GetQryPosActionBasedWithTrade(tableName, "alloc")) + Cst.CrLf;
            return new QueryParameters(CS, sqlQuery, parameters);

        }
        #endregion GetQueryTradeCandidatesToPhysicalPeriodicDelivery
    }
}

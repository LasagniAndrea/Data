using System;
using System.Collections.Generic;
using System.Text;

using EFS.Actor;
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;

namespace EFS.Restriction
{
    /// <summary>
    ///  Prise en considération ou pas des trades incomplets
    /// </summary>
    /// FI 20160810 [22086] Add enum
    public enum AddMissingTrade
    {
        /// <summary>
        /// Les tradesde marché incomplets sont exclus du jeu de résultat
        /// <para>Les jointures appliquée via SESSIONRESTRICT considèrent qu'il existe obligatoirement un acheteur, un vendeur, un marché</para>
        /// </summary>
        no,
        /// <summary>
        /// Les trades incomplets sont inclus du jeu de résultat
        /// <para>Les jointures appliquée via SESSIONRESTRICT considèrent qu'il n'existe pas nécessairement un acheteur, un vendeur, un marché</para>
        /// </summary>
        yes
    }


    /// <summary>
    /// Classe utilitaire qui permet de générer du code SQL de type jointure vers SESSIONRESTRICT de manière à retourner uniquement les données accessibles
    /// <para>Possibilité d'écrire du SQL en utilisant des SQLParameters</para>
    /// </summary>
    /// FI 20160810 [22086] Rename SessionRestrictHelper2 => SessionRestrictHelper
    public sealed class SessionRestrictHelper
    {

        /// <summary>
        /// Représente l'utilisateur connecté
        /// </summary>
        private User _user;
        /// <summary>
        /// Obtient ou définit l'utilisateur 
        /// </summary>
        public User User
        {
            get { return _user; }
            set { _user = value; }
        }

        /// <summary>
        /// Représente la session de l'utilisateur 
        /// </summary>
        private string _sessionId;
        /// <summary>
        /// Obtient ou définit la session de l'utilisateur
        /// </summary>
        public string SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }

        /// <summary>
        /// usage des paramètres SQL
        /// </summary>
        private bool _isUseDataParameter;
        /// <summary>
        /// Obtient ou définit un indicateur qui préconise l'usage de paramètres SQL
        /// </summary>
        public bool IsUseParameter
        {
            get { return _isUseDataParameter; }
            set { _isUseDataParameter = value; }
        }


        #region constructor
        /// <summary>
        ///
        /// </summary>
        /// <param name="pUser"></param>
        public SessionRestrictHelper()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUser">Utilisateur Spheres® </param>
        /// <param name="pSessionId">Identifiant de la session</param>
        /// <param name="pIsUseParameter">true si la classe génère des command SQL qui utilisent des paramaters</param>
        public SessionRestrictHelper(User pUser, string pSessionId, bool pIsUseDataParameter)
        {
            _user = pUser ?? throw new ArgumentException("{pUser} null is not allowed");
            _sessionId = pSessionId;
            _isUseDataParameter = pIsUseDataParameter;

            if ((!_isUseDataParameter) && StrFunc.IsEmpty(_sessionId))
                throw new ArgumentException("{pSessionId} empty is not allowed when  isUseParameter == false");
        }
        #endregion

        /// <summary>
        ///  Retourne la jointure SQL sur SESSIONRESTRICT pour retourner uniquement les acteurs accessibles
        /// </summary>
        /// <param name="pAlias">Alias pour la table SESSIONRESTRICT <para>si non renseigné l'alias sera "arestrict"</para></param>
        /// <param name="pColunmActorMain">Colonne sur laquelle s'applique la restriction (Format attendu: {alias table}{.}{Colonne})</param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// EG 20190226 Refactoring suite à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        public string GetSQLActor(string pAlias, string pColunmActorMain)
        {
            string ret = string.Empty;

            // FI 20141107 user.IsApplySessionRestrict
            if (User.IsApplySessionRestrict())
            {
                string alias = pAlias;
                if (StrFunc.IsEmpty(pAlias))
                    alias = "sr_ac";

                ret = String.Format(@"inner join dbo.SESSIONRESTRICT {0} on ({0}.ID = {1}) and ({0}.CLASS = 'ACTOR') and ({0}.SESSIONID = @SR_SESSIONID)", alias, pColunmActorMain) + Cst.CrLf;

                if (!IsUseParameter)
                    ret = ret.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
            }
            return ret;
        }

        /// <summary>
        ///  Retourne les jointures SQL sur SESSIONRESTRICT pour retourner uniquement les books accessibles
        ///  <para>Un book est accessible si le propriétaire est accessible et si l'entité est accessible</para>
        /// </summary>
        /// <param name="pAliasTblBook">Alias pour la table book <para>si non renseigné l'alias sera "brestrict"</para></param>
        /// <param name="pColunmBookMain">Colonne sur laquelle s'applique la restriction (Format attendu: {alias table}{.}{Colonne})</param>
        /// <param name="pWhere"></param>
        /// <returns></returns>
        /// FI 20100728 [17103] Un book est accessible si le propriétaire est accessible et si l'entité est accessible
        /// FI 20141107 [20441] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLBook(string pAliasTblBook, string pColunmBookMain, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            // FI 20141107 user.IsApplySessionRestrict
            if (User.IsApplySessionRestrict())
            {
                string alias = pAliasTblBook;
                if (StrFunc.IsEmpty(pAliasTblBook))
                    alias = "sr_bk";

                pJoin = String.Format(@"
                inner join dbo.BOOK {0} on ({0}.IDB = {1})
                inner join dbo.SESSIONRESTRICT sr_ac on (sr_ac.ID = {0}.IDA) and (sr_ac.CLASS = 'ACTOR') and (sr_ac.SESSIONID = @SR_SESSIONID)
                inner join dbo.SESSIONRESTRICT sr_ety on (sr_ety.ID = {0}.IDA_ENTITY) and (sr_ety.CLASS = 'ACTOR') and (sr_ety.SESSIONID = @SR_SESSIONID)", alias, pColunmBookMain) + Cst.CrLf;

                if (!IsUseParameter)
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));

                pWhere = String.Format(@"((sr_ety.ID is null and {0}.IDA_ENTITY is null) or (sr_ety.ID is not null))", alias);
            }
        }

        /// <summary>
        ///  Retourne la jointure SQL sur SESSIONRESTRICT pour retourner uniquement les instruments accessibles
        /// </summary>
        /// <param name="pAlias">Alias pour la table SESSIONRESTRICT <para>si non renseigné l'alias sera "irestrict"</para></param>
        /// <param name="pColunmInstrumentMain">Colonne sur laquelle s'applique la restriction (Format attendu: {alias table}{.}{Colonne})</param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public string GetSQLInstr(string pAlias, string pColunmInstrumentMain)
        {
            string ret = string.Empty;

            // FI 20141107 user.IsApplySessionRestrict
            if (User.IsApplySessionRestrict())
            {
                string alias = pAlias;
                if (StrFunc.IsEmpty(pAlias))
                    alias = "sr_ns";

                ret = String.Format(@"inner join dbo.SESSIONRESTRICT {0} on ({0}.ID = {1}) and ({0}.CLASS = 'INSTRUMENT') and ({0}.SESSIONID = @SR_SESSIONID)", alias, pColunmInstrumentMain) + Cst.CrLf;

                if (!IsUseParameter)
                    ret = ret.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
            }
            return ret;
        }

        /// <summary>
        ///  Retourne la jointure SQL sur SESSIONRESTRICT pour retourner uniquement les marchés accessibles
        /// </summary>
        /// <param name="pAlias">Alias pour la table SESSIONRESTRICT <para>si non renseigné l'alias sera "mrestrict"</para></param>
        /// <param name="pColumnMarketMain">Colonne sur laquelle s'applique la restriction (Format attendu: {alias table}{.}{Colonne})</param>
        /// <returns></returns>
        /// FI 20100728 [17103] add GetSQLMarketRestriction
        /// FI 20141107 [20441] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public string GetSQLMarket(string pAlias, string pColumnMarketMain)
        {
            string ret = string.Empty;

            // FI 20141107 user.IsApplySessionRestrict
            if (User.IsApplySessionRestrict())
            {
                string alias = pAlias;
                if (StrFunc.IsEmpty(pAlias))
                    alias = "sr_mk";

                ret = String.Format(@"inner join dbo.SESSIONRESTRICT {0} on ({0}.ID = {1}) and ({0}.CLASS = 'MARKET') and ({0}.SESSIONID = @SR_SESSIONID)", alias, pColumnMarketMain) + Cst.CrLf;

                if (!IsUseParameter)
                    ret = ret.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
            }
            return ret;
        }

        /// <summary>
        /// Retourne la jointure SQL sur SESSIONRESTRICT pour retourner uniquement les products accessibles
        /// <para>Un product est accessible s'il contient au moins un instrument accessible</para>
        /// </summary>
        /// <param name="pAlias">alias pour la table SESSIONRESTRICT <para>si non renseigné l'alias sera "prestrict"</para></param>
        /// <param name="pColunmProductMain"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public string GetSQLProduct(string pAlias, string pColumnProductMain)
        {
            string ret = string.Empty;

            // FI 20141107 user.IsApplySessionRestrict
            if (User.IsApplySessionRestrict())
            {
                string alias = pAlias;
                if (StrFunc.IsEmpty(pAlias))
                    alias = "sr_pr";

                ret = String.Format(@"inner join 
                (
                    select distinct ns.IDP 
                    from dbo.INSTRUMENT ns
                    inner join dbo.SESSIONRESTRICT sr on (sr.ID = ns.ID) and (sr.CLASS = 'INSTRUMENT') and (sr.SESSIONID = @SR_SESSIONID)
                ) {0} on ({0}.IDP = {1})", alias, pColumnProductMain) + Cst.CrLf;

                if (!IsUseParameter)
                    ret = ret.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
            }

            return ret;
        }

        /// <summary>
        /// Retourne la jointure SQL sur SESSIONRESTRICT pour retourner uniquement les groupes d'instruments accessibles
        /// <para>Un groupe d'instrument est accessible s'il contient au moins un instrument accessible</para>
        /// </summary>
        ///<param name="pAlias">alias pour la table SESSIONRESTRICT, si non renseigné gInstrrestrict</param>
        ///<param name="pColumnGroupMain">(Format attendu: {alias table}{.}{Colonne})</param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public string GetSQLGroupeInstr(string pAlias, string pColumnGroupMain)
        {
            string ret = string.Empty;

            // FI 20141107 user.IsApplySessionRestrict
            if (User.IsApplySessionRestrict())
            {
                string alias = pAlias;
                if (StrFunc.IsEmpty(pAlias))
                    alias = "sr_ig";

                //afin de limiter l’utilisation des groupes d’instruments aux seuls groupes contenant des instruments « autorisés » par le user connecté
                // Pas besoin de faire une restriction sur le rôle.
                ret = String.Format(@"inner join 
                (
                    select distinct ig.IDGINSTR
                    from dbo.INSTRG ig
                    inner join dbo.SESSIONRESTRICT sr on (sr.ID = ns.ID) and (sr.CLASS = 'INSTRUMENT') and (sr.SESSIONID = @SR_SESSIONID)
                ) {0} on ({0}.IDGINSTR = {1})", alias, pColumnGroupMain) + Cst.CrLf;

                if (!IsUseParameter)
                    ret = ret.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
            }
            return ret;
        }

        /// <summary>
        ///  Retourne la jointure SQL sur SESSIONRESTRICT pour retourner uniquement les tâches IO accessibles
        /// </summary>
        /// <param name="pAlias">Alias pour la table SESSIONRESTRICT <para>si non renseigné l'alias sera "irestrict"</para></param>
        /// <param name="pColunmInstrumentMain"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        /// FI 20100728 [17103] add GetSQLIOTaskRestriction
        /// FI 20141107 [20441] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public string GetSQLIOTask(string pAlias, string pColunmIOTaskMain)
        {
            string ret = string.Empty;

            if (User.IsApplySessionRestrict())
            {
                string alias = pAlias;
                if (StrFunc.IsEmpty(pAlias))
                    alias = "sr_io";

                ret = String.Format(@"inner join dbo.SESSIONRESTRICT {0} on ({0}.ID = {1}) and ({0}.CLASS = 'IOTASK') and ({0}.SESSIONID = @SR_SESSIONID)", alias, pColunmIOTaskMain) + Cst.CrLf;

                if (!IsUseParameter)
                    ret = ret.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
            }
            return ret;
        }

        /// <summary>
        /// Retourne le join et le where pour retourner uniquement les trades de marché accessibles
        /// </summary>
        /// <remarks>
        /// Principe sur les parties
        /// Un trade  OTC, CASHBALANCE, RISKPERFORMANCE est accessible s’il existe une partie qui vérifie les règles suivantes
        /// - le book associé à la partie est géré par l’entité de rattachement de l’utilisateur connecté
        /// - la partie est accessible
        /// - la contrepartie est accessible
        /// 
        /// Un trade ALLOC est accessible si sur la partie Dealer 
        /// - le book est géré par l’entité de rattachement de l’utilisateur connecté
        /// - la partie est accessible
        /// - la contrepartie est accessible (Chambre ou broker de compensation) 
        /// 
        /// Principe sur instrument
        /// Spheres® affiche un trade si l'instrument est accessible 
        /// 
        /// Principe sur le marché
        /// Spheres® affiche un trade si le marché est accessible 
        /// </remarks>
        /// <param name="pColunmIDT">Nom de la colonne qui représente un TRADE (Format attendu: {alias table}{.}{Colonne})</param>
        /// <param name="pAliasTR">Alias de la table TRADE, null possible</param>
        /// <param name="pJoin"></param>
        /// <param name="pWhere"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modification de signature
        /// FI 20160810 [22086] Modify (Add parameter pAddMissingTrade, refactoring TRADEACTOR n'est plus utilisé pour améliorer les perfs)
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLTrade(string pColunmIDT, string pAliasTR, AddMissingTrade pAddMissingTrade, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            if (User.IsApplySessionRestrict())
            {
                StrBuilder joinSR = new StrBuilder();

                string aliasTR = pAliasTR;
                if (StrFunc.IsEmpty(aliasTR) || (aliasTR == "null"))
                {
                    aliasTR = "sr_ti";
                    joinSR += StrFunc.AppendFormat("inner join dbo.TRADE sr_ti on sr_ti.IDT={0}", pColunmIDT) + Cst.CrLf;
                }

                // Instrument
                // Market (Remarque : pas tjs de marché sur un trade OTC)
                // Actor 
                // Book
                /* FI 20210330 [XXXXX] Suppression des jointures sr_b_buyer et sr_b_seller puisque  Cst.IsDisabledRestrictOnEntity
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_ns on sr_ns.ID={0}.IDI and sr_ns.CLASS='INSTRUMENT' and sr_ns.SESSIONID={1}
                left outer join dbo.SESSIONRESTRICT sr_mk on sr_mk.ID={0}.IDM and sr_mk.CLASS='MARKET' and sr_mk.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_buyer on sr_buyer.ID={0}.IDA_BUYER and sr_buyer.CLASS='ACTOR' and sr_buyer.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}
                left outer join dbo.BOOK sr_b_buyer on sr_b_buyer.IDB={0}.IDB_BUYER
                left outer join dbo.BOOK sr_b_seller on sr_b_seller.IDB={0}.IDB_SELLER", aliasTR, "@SR_SESSIONID", pAddMissingTrade == AddMissingTrade.yes ? "left outer" : "inner") + Cst.CrLf;
                */
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_ns on sr_ns.ID={0}.IDI and sr_ns.CLASS='INSTRUMENT' and sr_ns.SESSIONID={1}
                left outer join dbo.SESSIONRESTRICT sr_mk on sr_mk.ID={0}.IDM and sr_mk.CLASS='MARKET' and sr_mk.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_buyer on sr_buyer.ID={0}.IDA_BUYER and sr_buyer.CLASS='ACTOR' and sr_buyer.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}", aliasTR, "@SR_SESSIONID", pAddMissingTrade == AddMissingTrade.yes ? "left outer" : "inner") + Cst.CrLf;

                // Le marché est pas nécessairement oblidatoire (Il existe des trades OTC sans marché) => Restriction vis à vis des marché uniquement s'il existe un marché
                pWhere = StrFunc.AppendFormat(@"(({0}.IDM is null) or (sr_mk.ID is not null))", aliasTR);

                switch (pAddMissingTrade)
                {
                    case AddMissingTrade.yes:
                        //Pour l'acteur SYSTEM (id:1), l'importation des trades (LSD_TradeImport_Map.xsl) remplace l'acteur Dealer par SYSTEM lorsqu'il est inconnu
                        //=> ajout de l'acteur IDA_DEALER=1 (Pas besoin de donner les droits vis à vis de  l'acteur SYSTEM pour consulter le trade incomplet)
                        pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDA_BUYER is null) or ({0}.IDA_BUYER=1) or (sr_buyer.ID is not null))", aliasTR);
                        pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDA_SELLER is null) or ({0}.IDA_SELLER=1) or (sr_seller.ID is not null))", aliasTR);
                        if (!Cst.IsDisabledRestrictOnEntity)
                        {
                            pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDB_BUYER is null) or (sr_b_buyer.IDA_ENTITY = {1}))", aliasTR, "@SR_IDA_ENTITY");
                            pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDB_SELLER is null) or (sr_b_seller.IDA_ENTITY = {1}))", aliasTR, "@SR_IDA_ENTITY");
                        }
                        break;
                    case AddMissingTrade.no:
                        // Si ALLOC: le book du dealer doit être géré par l'entité rattachée à l'utilisateur
                        // Sinon   : au moins un book doit nécessairement être géré par l'entité rattachée à l'utilisateur
                        if (!Cst.IsDisabledRestrictOnEntity)
                        {
                            pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and
                            (
                                (sr_b_buyer.IDA_ENTITY={0} and {1}.IDA_BUYER = case when {1}.IDSTBUSINESS = 'ALLOC' then {1}.IDA_DEALER else {1}.IDA_BUYER end) 
                                or
                                (sr_b_seller.IDA_ENTITY={0} and {1}.IDA_SELLER = case when {1}.IDSTBUSINESS = 'ALLOC' then {1}.IDA_DEALER else {1}.IDA_SELLER end) 
                            )", "@SR_IDA_ENTITY", aliasTR);
                        }
                        // FI 20210407 [XXXXX] Mise en commentaire.
                        // Code SQL sans intérêt car le dealer est nécessairement un buyer ou un seller
                        //else
                        //{
                        //    pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and
                        //    (
                        //        ({0}.IDA_BUYER = case when {0}.IDSTBUSINESS = 'ALLOC' then {0}.IDA_DEALER else {0}.IDA_BUYER end) 
                        //        or
                        //        ({0}.IDA_SELLER = case when {0}.IDSTBUSINESS = 'ALLOC' then {0}.IDA_DEALER else {0}.IDA_SELLER end) 
                        //    )", aliasTR);
                        //}
                        break;
                }

                pJoin = joinSR.ToString();

                if (!IsUseParameter)
                {
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
                    pWhere = pWhere.Replace("@SR_IDA_ENTITY", User.Entity_IdA.ToString());
                }
            }
        }

        /// <summary>
        /// Retourne le join et le where pour retourner le scope aux trades ALLOC de marché accessibles au user connecté
        /// <para>
        /// Principe sur les parties
        /// Un trade ALLOC est accessible si sur la partie Dealer 
        /// - le book est géré par l’entité de rattachement de l’utilisateur connecté
        /// - la partie est accessible
        /// - la contrepartie est accessible (Chambre ou broker de compensation) 
        /// </para>
        /// <para>
        /// Principe sur instrument
        /// Spheres® affiche un trade si l'instrument est accessible 
        /// </para>
        /// <para>
        /// Principe sur le marché
        /// Spheres® affiche un trade si le marché est accessible 
        /// </para>
        /// </summary>
        /// <param name="pColunmIDT"></param>
        /// <param name="pAliasBookDealer">alias de la table BOOK qui représente le dealer (null ou "null" ou string.empty sont des valeurs possibles)</param>
        /// <param name="pAddMissingTrade">si true les trades incomplets sont acceptés</param>
        /// <param name="pJoin">out jointure</param>
        /// <param name="pWhere">out where </param>
        /// FI 20141107 [20441] Add Method
        /// FI 20160810 [22086] Modify (Add parameter pAddMissingTrade)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLTradeAlloc(string pColunmIDT, string pAliasTR, string pAliasBookDealer, AddMissingTrade pAddMissingTrade, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            if (User.IsApplySessionRestrict())
            {
                StrBuilder joinSR = new StrBuilder();

                string aliasTR = pAliasTR;
                if (StrFunc.IsEmpty(aliasTR) || (aliasTR == "null"))
                {
                    aliasTR = "sr_ti";
                    joinSR += StrFunc.AppendFormat("inner join dbo.TRADE sr_ti on sr_ti.IDT={0}", pColunmIDT) + Cst.CrLf;
                }

                string aliasBookDealer;
                if (!Cst.IsDisabledRestrictOnEntity)
                {
                    if (StrFunc.IsEmpty(aliasBookDealer) || (aliasBookDealer == "null"))
                    {
                        aliasBookDealer = "sr_b_dealer";
                        joinSR += StrFunc.AppendFormat(@"{2} join dbo.BOOK {0} on {0}.IDB={1}.IDB_DEALER", aliasBookDealer, aliasTR, pAddMissingTrade == AddMissingTrade.yes ? "left outer" : "inner") + Cst.CrLf;
                    }
                }

                // Instrument
                // Market
                // Dealer
                // Clearer
                /* FI 20210330 [XXXXX] Suppression des jointures inutiles sur sr_b_buyer et sr_b_seller 
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_ns on sr_ns.ID={0}.IDI and sr_ns.CLASS='INSTRUMENT' and sr_ns.SESSIONID={1}
                left outer join dbo.SESSIONRESTRICT sr_mk on sr_mk.ID={0}.IDM and sr_mk.CLASS='MARKET' and sr_mk.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_dealer on sr_dealer.ID={0}.IDA_DEALER and sr_dealer.CLASS='ACTOR' and sr_dealer.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_clearer on sr_clearer.ID={0}.IDA_CLEARER and sr_clearer.CLASS='ACTOR' and sr_clearer.SESSIONID={1}
                left outer join dbo.BOOK sr_b_buyer on sr_b_buyer.IDB={0}.IDB_BUYER
                left outer join dbo.BOOK sr_b_seller on sr_b_seller.IDB={0}.IDB_SELLER", aliasTR, "@SR_SESSIONID", pAddMissingTrade == AddMissingTrade.yes ? "left outer" : "inner") + Cst.CrLf;
                */

                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_ns on sr_ns.ID={0}.IDI and sr_ns.CLASS='INSTRUMENT' and sr_ns.SESSIONID={1}
                left outer join dbo.SESSIONRESTRICT sr_mk on sr_mk.ID={0}.IDM and sr_mk.CLASS='MARKET' and sr_mk.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_dealer on sr_dealer.ID={0}.IDA_DEALER and sr_dealer.CLASS='ACTOR' and sr_dealer.SESSIONID={1}
                {2} join dbo.SESSIONRESTRICT sr_clearer on sr_clearer.ID={0}.IDA_CLEARER and sr_clearer.CLASS='ACTOR' and sr_clearer.SESSIONID={1}", aliasTR, "@SR_SESSIONID", pAddMissingTrade == AddMissingTrade.yes ? "left outer" : "inner") + Cst.CrLf;

                pJoin = joinSR.ToString();

                //Le marché est pas nécessairement oblidatoire (Il peut exister des trades ALLOC sans marché) => Restriction vis à vis des marché uniquement s'il existe un marché
                //Remarque les ALLOC ETD on nécessairement un marché 
                pWhere = StrFunc.AppendFormat(@"(({0}.IDM is null) or (sr_mk.ID is not null))", aliasTR);

                switch (pAddMissingTrade)
                {
                    case AddMissingTrade.yes:
                        //Pour l'acteur SYSTEM (id:1), l'importation des trades (LSD_TradeImport_Map.xsl) remplace l'acteur Dealer par SYSTEM lorsqu'il est inconnu
                        //=> ajout de l'acteur IDA_DEALER=1 (Pas besoin de donner les droits vis à vis de  l'acteur SYSTEM pour consulter le trade incomplet)
                        pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDA_DEALER is null) or ({0}.IDA_DEALER=1) or (sr_dealer.ID is not null))", aliasTR);
                        pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDA_CLEARER is null) or (sr_clearer.ID is not null))", aliasTR);
                        if (!Cst.IsDisabledRestrictOnEntity)
                        {
                            pWhere += Cst.CrLf + StrFunc.AppendFormat(@" and (({0}.IDB is null) or ({1}.IDA_ENTITY = {2}))", aliasBookDealer, aliasTR, "@SR_IDA_ENTITY");
                        }
                        break;
                    case AddMissingTrade.no:
                        if (!Cst.IsDisabledRestrictOnEntity)
                        {
                            pWhere += StrFunc.AppendFormat(@" and {0}.IDA_ENTITY = {1}", aliasBookDealer, "@SR_IDA_ENTITY");
                        }
                        break;
                }

                if (!IsUseParameter)
                {
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));

                    pWhere = pWhere.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
                    pWhere = pWhere.Replace("@SR_IDA_ENTITY", User.Entity_IdA.ToString());
                }
            }
        }


        /// <summary>
        /// Retourne le join et le where pour retourner trades RISK (deposit ou CashBalance) accessibles
        /// </summary>
        /// <remarks>
        /// Principe sur les parties
        /// Un trade  CASHBALANCE, RISKPERFORMANCE est accessible s’il existe une partie qui vérifie les règles suivantes
        /// - le book associé à la partie est géré par l’entité de rattachement de l’utilisateur connecté
        /// - la partie est accessible
        /// - la contrepartie est accessible
        /// 
        /// Principe sur instrument
        /// Spheres® affiche un trade si l'instrument est accessible 
        /// 
        /// </remarks>
        /// <param name="pColunmIDT">Nom de la colonne qui représente un TRADE (Format attendu: {alias table}{.}{Colonne})</param>
        /// <param name="pAliasTI">Alias de la table TRADEINSTRUEMENT, null possible</param>
        /// <param name="pJoin"></param>
        /// <param name="pwhere"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Add Method
        /// FI 20141107 [20441] Modify Method
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLTradeRisk(string pColunmIDT, string pAliasTR, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            if (User.IsApplySessionRestrict())
            {
                StrBuilder joinSR = new StrBuilder();

                string aliasTR = pAliasTR;
                if (StrFunc.IsEmpty(aliasTR) || (aliasTR == "null"))
                {
                    aliasTR = "sr_ti";
                    joinSR += StrFunc.AppendFormat("inner join dbo.TRADE sr_ti on sr_ti.IDT={0}", pColunmIDT) + Cst.CrLf;
                }

                /* FI 20210330 [XXXXX] Suppression des jointures sr_b_buyer et sr_b_seller puisque  Cst.IsDisabledRestrictOnEntity                 
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_sri on sr_sri.ID={0}.IDI and sr_sri.CLASS='INSTRUMENT' and sr_sri.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_buyer on sr_buyer.ID={0}.IDA_BUYER and sr_buyer.CLASS='ACTOR' and sr_buyer.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}
                left outer join dbo.BOOK sr_b_buyer on sr_b_buyer.IDB={0}.IDB_BUYER
                left outer join dbo.BOOK sr_b_seller on sr_b_seller.IDB={0}.IDB_SELLER", aliasTR, "@SR_SESSIONID") + Cst.CrLf;
                */

                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_sri on sr_sri.ID={0}.IDI and sr_sri.CLASS='INSTRUMENT' and sr_sri.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_buyer on sr_buyer.ID={0}.IDA_BUYER and sr_buyer.CLASS='ACTOR' and sr_buyer.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}", aliasTR, "@SR_SESSIONID") + Cst.CrLf;
                
                pJoin = joinSR.ToString();

                if (!IsUseParameter)
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));

                if (!Cst.IsDisabledRestrictOnEntity)
                    pWhere = StrFunc.AppendFormat(@"( (sr_b_buyer.IDA_ENTITY={0}) or (sr_b_seller.IDA_ENTITY={0}) )", "@SR_IDA_ENTITY");

                if (!IsUseParameter)
                    pWhere = pWhere.Replace("@SR_IDA_ENTITY", User.Entity_IdA.ToString());
            }
        }


        /// <summary>
        /// Retourne le join et le where pour retourner uniquement les titres accessibles
        /// </summary>
        /// <remarks>
        /// Principe sur les parties
        /// Spheres® affiche un DebtSecurity  si l’émetteur est accessible
        /// 
        /// Principe sur instrument
        /// Spheres® affiche un debtSecurity si l'instrument est accessible 
        /// 
        /// Principe sur le marché
        /// Spheres® affiche un debtSecurity si la marché est accessible 
        /// </remarks>
        /// <param name="pColunmIDT">Nom de la colonne qui représente un TRADE DEBTSEC (Format attendu: {alias table}{.}{Colonne})</param>
        /// <param name="pJoin"></param>
        /// <param name="pwhere"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify Method
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLTradeDebtSec(string pColunmIDT, string pAliasTR, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            if (User.IsApplySessionRestrict())
            {
                StrBuilder joinSR = new StrBuilder();

                string aliasTR = pAliasTR;
                if (StrFunc.IsEmpty(aliasTR) || (aliasTR == "null"))
                {
                    aliasTR = "sr_ti";
                    joinSR += StrFunc.AppendFormat("inner join dbo.TRADE sr_ti on sr_ti.IDT={0}", pColunmIDT) + Cst.CrLf;
                }

                //Instrument - Market - Issuer
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_sri on sr_sri.ID={0}.IDI and sr_sri.CLASS='INSTRUMENT' and sr_sri.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_mk on sr_mk.ID={0}.IDI and sr_mk.CLASS='MARKET' and sr_mk.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}", aliasTR, "@SR_SESSIONID") + Cst.CrLf;

                pJoin = joinSR.ToString();

                if (!IsUseParameter)
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));

                //Titres sans Marché ou avec Marché accessible
                pWhere = StrFunc.AppendFormat(@"({0}.IDM is null or sr_mk.ID is not null)", aliasTR);
            }
        }

        /// <summary>
        /// Retourne le join et le where pour retourner uniquement les factures, avoirs, règlements accessibles
        /// </summary>
        /// <remarks>
        /// Principe sur les parties
        /// Spheres® affiche un tradeAdmin si 
        /// - le book associé au bénéficiaire est géré par l’entité de rattachement de l’utilisateur connecté
        /// - la contrepartie de la facture est accessible
        ///         
        /// Principe sur instrument
        /// Spheres® affiche un tradeAdmin si l'instrument est accessible
        /// </remarks>
        /// <param name="pColunmIDT">Nom de la colonne qui représente un TRADE ADMIN (Format attendu: {alias table}{.}{Colonne})</param>
        /// <param name="pJoin"></param>
        /// <param name="pwhere"></param>
        /// <returns></returns>
        // FI 20141107 [20441] Modify Method
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLTradeAdmin(string pColunmIDT, string pAliasTR, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            if (User.IsApplySessionRestrict())
            {
                StrBuilder joinSR = new StrBuilder();

                string aliasTR = pAliasTR;
                if (StrFunc.IsEmpty(aliasTR) || (aliasTR == "null"))
                {
                    aliasTR = "sr_ti";
                    joinSR += StrFunc.AppendFormat("inner join dbo.TRADE sr_ti on sr_ti.IDT={0}", pColunmIDT) + Cst.CrLf;
                }

                // Instrument - Actor
                /* FI 20210330 [XXXXX] Suppression des jointures sr_b_buyer et sr_b_seller puisque  Cst.IsDisabledRestrictOnEntity                 
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_sri on sr_sri.ID={0}.IDI and sr_sri.CLASS='INSTRUMENT' and sr_sri.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_buyer on sr_buyer.ID={0}.IDA_BUYER and sr_buyer.CLASS='ACTOR' and sr_buyer.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}
                left outer join dbo.BOOK sr_b_buyer on sr_b_buyer.IDB={0}.IDB_BUYER
                left outer join dbo.BOOK sr_b_seller on sr_b_seller.IDB={0}.IDB_SELLER", aliasTR, "@SR_SESSIONID") + Cst.CrLf;
                */
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_sri on sr_sri.ID={0}.IDI and sr_sri.CLASS='INSTRUMENT' and sr_sri.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_buyer on sr_buyer.ID={0}.IDA_BUYER and sr_buyer.CLASS='ACTOR' and sr_buyer.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_seller on sr_seller.ID={0}.IDA_SELLER and sr_seller.CLASS='ACTOR' and sr_seller.SESSIONID={1}", aliasTR, "@SR_SESSIONID") + Cst.CrLf;


                pJoin = joinSR.ToString();

                if (!IsUseParameter)
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));

                if (!Cst.IsDisabledRestrictOnEntity)
                    pWhere = StrFunc.AppendFormat(@"(sr_b_buyer.IDA_ENTITY={0} or sr_b_seller.IDA_ENTITY={0})", "@SR_IDA_ENTITY");

                if (!IsUseParameter)
                    pWhere = pWhere.Replace("@SR_IDA_ENTITY", User.Entity_IdA.ToString());
            }
        }

        /// <summary>
        /// Remplace dans un string les mots clefs "SR:"
        /// </summary>
        /// <param name="pData">Représente la string</param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        public string ReplaceKeyword(string pData)
        {
            string ret = pData;
            ret = ReplaceTradeKeyword(ret);
            ret = ReplaceTradeAllocKeyword(ret);
            ret = ReplaceTradeRiskKeyword(ret);
            ret = ReplaceTradeDebtSecKeyword(ret);
            ret = ReplaceTradeAdminKeyword(ret);
            ret = ReplacePosRequestKeyword(ret);
            ret = ReplaceMarketKeyword(ret);
            ret = ReplaceInstrumentKeyword(ret);
            ret = ReplaceActorKeyword(ret);
            ret = ReplaceIoTaskKeyword(ret);
            ret = ReplacePosCollateralKeyword(ret);
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%SR:TRADE_JOIN%% et %%SR:TRADE_WHERE_PREDICATE%%
        /// <para>Ces mots clefs sont utilisés pour les trades de marché uniquement</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Modify
        /// FI 20160810 [22086] Modify
        /// FI 20170531 [23206] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private string ReplaceTradeKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_TRADE_WHERE_PREDICATE) && false == ret.Contains(Cst.SR_TRADE_JOIN))
                    throw new Exception(StrFunc.AppendFormat("Keyword [{0}] is expected", Cst.SR_TRADE_JOIN));

                int guard = 0;
                while (ret.Contains(Cst.SR_TRADE_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add

                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_TRADE_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");
                    //
                    string sqlJoin = string.Empty;
                    string wherePredicate = "1=1";
                    if (User.IsApplySessionRestrict())
                    {
                        string columnIDT = arg[0];

                        string aliasTR = null;
                        if (ArrFunc.Count(arg) >= 2)
                            aliasTR = arg[1];

                        // FI 20160810 [22086] add addTradeMissing
                        AddMissingTrade addTradeMissing = AddMissingTrade.no;
                        if (ArrFunc.Count(arg) >= 3)
                            addTradeMissing = ReflectionTools.ConvertStringToEnumOrDefault<AddMissingTrade>(arg[2], AddMissingTrade.no);


                        GetSQLTrade(columnIDT, aliasTR, addTradeMissing, out sqlJoin, out wherePredicate);
                    }

                    ret = ret.Replace(Cst.SR_TRADE_JOIN + "(" + arg2 + ")", sqlJoin);
                    ret = ret.Replace(Cst.SR_TRADE_WHERE_PREDICATE, wherePredicate);
                }

                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%SR:TRADEALLOC_JOIN%% et %%SR:TRADEALLOC_WHERE_PREDICATE%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Add Method
        /// FI 20160810 [22086] Modify
        /// FI 20170531 [23206] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private string ReplaceTradeAllocKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_TRADEALLOC_WHERE_PREDICATE) && false == ret.Contains(Cst.SR_TRADEALLOC_JOIN))
                    throw new Exception(StrFunc.AppendFormat("Keyword [{0}] is expected", Cst.SR_TRADEALLOC_JOIN));

                int guard = 0;
                while (ret.Contains(Cst.SR_TRADEALLOC_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_TRADEALLOC_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");

                    string sqlJoin = string.Empty;
                    string wherePredicate = "1=1";
                    if (User.IsApplySessionRestrict())
                    {
                        string columnIDT = arg[0];

                        string aliasTR = null;
                        if (ArrFunc.Count(arg) >= 2)
                            aliasTR = arg[1];

                        string aliasBookDealer = null;
                        if (ArrFunc.Count(arg) >= 3)
                            aliasBookDealer = arg[2];

                        // FI 20160810 [22086] add addTradeMissing
                        AddMissingTrade addTradeMissing = AddMissingTrade.no;
                        if (ArrFunc.Count(arg) >= 4)
                            addTradeMissing = ReflectionTools.ConvertStringToEnumOrDefault<AddMissingTrade>(arg[3], AddMissingTrade.no);

                        GetSQLTradeAlloc(columnIDT, aliasTR, aliasBookDealer, addTradeMissing, out sqlJoin, out wherePredicate);
                    }

                    ret = ret.Replace(Cst.SR_TRADEALLOC_JOIN + "(" + arg2 + ")", sqlJoin);
                    ret = ret.Replace(Cst.SR_TRADEALLOC_WHERE_PREDICATE, wherePredicate);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%SR:TRADEALLOC_JOIN%% et %%SR:TRADEALLOC_WHERE_PREDICATE%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20141107 [20441] Add Method
        /// FI 20170531 [23206] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private string ReplaceTradeRiskKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_TRADERISK_WHERE_PREDICATE) && false == ret.Contains(Cst.SR_TRADERISK_JOIN))
                    throw new Exception(StrFunc.AppendFormat("Keyword [{0}] is expected", Cst.SR_TRADERISK_JOIN));

                int guard = 0;
                while (ret.Contains(Cst.SR_TRADERISK_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_TRADERISK_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");

                    string sqlJoin = string.Empty;
                    string wherePredicate = "1=1";
                    if (User.IsApplySessionRestrict())
                    {
                        string columnIDT = arg[0];

                        string aliasTR = null;
                        if (ArrFunc.Count(arg) >= 2)
                            aliasTR = arg[1];

                        GetSQLTradeRisk(columnIDT, aliasTR, out sqlJoin, out wherePredicate);
                    }

                    ret = ret.Replace(Cst.SR_TRADERISK_JOIN + "(" + arg2 + ")", sqlJoin);
                    ret = ret.Replace(Cst.SR_TRADERISK_WHERE_PREDICATE, wherePredicate);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%SR:TRADEDEBTSEC_JOIN%% et %%SR:TRADEDEBTSEC_WHERE_PREDICATE%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private string ReplaceTradeDebtSecKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) & (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_TRADEDEBTSEC_WHERE_PREDICATE) && false == ret.Contains(Cst.SR_TRADEDEBTSEC_JOIN))
                    throw new Exception(StrFunc.AppendFormat("Keyword [{0}] is expected", Cst.SR_TRADEDEBTSEC_JOIN));

                int guard = 0;
                while (ret.Contains(Cst.SR_TRADEDEBTSEC_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_TRADEDEBTSEC_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");

                    string sqlJoin = string.Empty;
                    string wherePredicate = "1=1";
                    if (User.IsApplySessionRestrict())
                    {
                        string columnIDT = arg[0];

                        string aliasTR = null;
                        if (ArrFunc.Count(arg) >= 2)
                            aliasTR = arg[1];

                        GetSQLTradeDebtSec(columnIDT, aliasTR, out sqlJoin, out wherePredicate);
                    }

                    ret = ret.Replace(Cst.SR_TRADEDEBTSEC_JOIN + "(" + arg2 + ")", sqlJoin);
                    ret = ret.Replace(Cst.SR_TRADEDEBTSEC_WHERE_PREDICATE, wherePredicate);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%SR:SR_TRADEADMIN_JOIN%% et %%SR:SR_TRADEADMIN_WHERE_PREDICATE%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private string ReplaceTradeAdminKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_TRADEADMIN_WHERE_PREDICATE) && false == ret.Contains(Cst.SR_TRADEADMIN_JOIN))
                    throw new Exception(StrFunc.AppendFormat("Keyword [{0}] is expected", Cst.SR_TRADEADMIN_JOIN));
                //
                //%%SR:SR_TRADEADMIN_JOIN%%(t.IDT)
                //%%SR:SR_TRADEADMIN_WHERE_PREDICATE%%
                int guard = 0;
                while (ret.Contains(Cst.SR_TRADEADMIN_JOIN) && (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add

                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_TRADEADMIN_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");

                    string sqlJoin = string.Empty;
                    string wherePredicate = "1=1";
                    if (User.IsApplySessionRestrict())
                    {
                        string columnIDT = arg[0];

                        string aliasTR = null;
                        if (ArrFunc.Count(arg) >= 2)
                            aliasTR = arg[1];

                        GetSQLTradeAdmin(columnIDT, aliasTR, out sqlJoin, out wherePredicate);
                    }

                    ret = ret.Replace(Cst.SR_TRADEADMIN_JOIN + "(" + arg2 + ")", sqlJoin);
                    ret = ret.Replace(Cst.SR_TRADEADMIN_WHERE_PREDICATE, wherePredicate);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string le mot clef %%SR:MARKET_JOIN%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        private string ReplaceMarketKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) & (ret.IndexOf(Cst.SR_START) >= 0))
            {
                //%%SR:MARKET_JOIN%%(xxx.IDM)
                int guard = 0;
                while (ret.Contains(Cst.SR_MARKET_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add

                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_MARKET_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");
                    //
                    string sqlJoin = string.Empty;
                    if (User.IsApplySessionRestrict())
                    {
                        if (ArrFunc.Count(arg) == 1)
                            sqlJoin = GetSQLMarket(string.Empty, arg[0]);
                        else if (ArrFunc.Count(arg) == 2)
                            sqlJoin = GetSQLMarket(arg[0], arg[1]);
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("Too many arguments for (0)", Cst.SR_MARKET_JOIN));

                    }
                    ret = ret.Replace(Cst.SR_MARKET_JOIN + "(" + arg2 + ")", sqlJoin);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string le mot clef %%SR:INSTR_JOIN%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        private string ReplaceInstrumentKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) & (ret.IndexOf(Cst.SR_START) >= 0))
            {
                //%%SR:INSTR_JOIN%%(column)
                int guard = 0;
                while (ret.Contains(Cst.SR_INSTR_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add

                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_INSTR_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");
                    //
                    string sqlJoin = string.Empty;
                    if (User.IsApplySessionRestrict())
                    {
                        if (ArrFunc.Count(arg) == 1)
                            sqlJoin = GetSQLInstr(string.Empty, arg[0]);
                        else if (ArrFunc.Count(arg) == 2)
                            sqlJoin = GetSQLInstr(arg[0], arg[1]);
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("Too many arguments for (0)", Cst.SR_INSTR_JOIN));
                    }
                    //
                    ret = ret.Replace(Cst.SR_INSTR_JOIN + "(" + arg2 + ")", sqlJoin);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string le mot clef %%SR:IOTASK_JOIN%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20170531 [23206] Modify
        private string ReplaceIoTaskKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) & (ret.IndexOf(Cst.SR_START) >= 0))
            {
                //%%SR:IOTASK_JOIN%%(column)
                int guard = 0;
                while (ret.Contains(Cst.SR_IOTASK_JOIN) & (guard < 100))
                {
                    guard++; // FI 20170531 [23206] Add

                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_IOTASK_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");
                    //
                    string sqlJoin = string.Empty;
                    if (User.IsApplySessionRestrict())
                    {
                        if (ArrFunc.Count(arg) == 1)
                            sqlJoin = GetSQLIOTask(string.Empty, arg[0]);
                        else if (ArrFunc.Count(arg) == 2)
                            sqlJoin = GetSQLIOTask(arg[0], arg[1]);
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("Too many arguments for (0)", Cst.SR_IOTASK_JOIN));
                    }
                    //
                    ret = ret.Replace(Cst.SR_IOTASK_JOIN + "(" + arg2 + ")", sqlJoin);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }

        /// <summary>
        /// Remplace dans un string le mot clef %%SR:ACTOR_JOIN%%
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        private string ReplaceActorKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) & (ret.IndexOf(Cst.SR_START) >= 0))
            {
                //%%SR:SR_ACTOR_JOIN%%(column)
                int guard = 0;
                while (ret.Contains(Cst.SR_ACTOR_JOIN) & (guard < 100))
                {
                    guard++;
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_ACTOR_JOIN);
                    string arg2 = StrFunc.QueryStringData.StringArrayToStringList(arg, false).Replace(";", ",");
                    //
                    string sqlJoin = string.Empty;
                    if (User.IsApplySessionRestrict())
                    {
                        if (ArrFunc.Count(arg) == 1)
                            sqlJoin = GetSQLActor(string.Empty, arg[0]);
                        else if (ArrFunc.Count(arg) == 2)
                            sqlJoin = GetSQLActor(arg[0], arg[1]);
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("Too many arguments for (0)", Cst.SR_ACTOR_JOIN));
                    }
                    //
                    ret = ret.Replace(Cst.SR_ACTOR_JOIN + "(" + arg2 + ")", sqlJoin);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }


        /// <summary>
        /// Ajoute ds une collection de parameters les paramètres détectés sous {pCommand}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCommand"></param>
        /// <param name="pParameters">Collection de paramètres qui est impactée</param>
        public void SetParameter(string pCS, string pCommand, DataParameters pParameters)
        {
            if (IsUseParameter)
            {
                if (null == pParameters)
                    throw new ArgumentException("DataParameters argument null not available");

                if (StrFunc.IsFilled(pCommand))
                {
                    if (pCommand.Contains("@SR_SESSIONID") && (false == pParameters.Contains("SR_SESSIONID")))
                    {
                        pParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SR_SESSIONID), SessionId);
                    }
                    //
                    if (pCommand.Contains("@SR_IDA_ENTITY") && (false == pParameters.Contains("SR_IDA_ENTITY")))
                    {
                        pParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SR_IDA_ENTITY), User.Entity_IdA);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        private string ReplacePosRequestKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_POSREQUEST_SELECT))
                {
                    string select = GetSelectPosRequest();
                    ret = ret.Replace(Cst.SR_POSREQUEST_SELECT, select);
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetSelectPosRequest()
        {
            /* POSREQUEST sur trade accessibles */
            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "p.*" + Cst.CrLf;
            sql += SQLCst.FROM_DBO.ToString() + Cst.OTCml_TBL.POSREQUEST.ToString() + " p" + Cst.CrLf;
            sql += Cst.SR_TRADE_JOIN + "(p.IDT)" + Cst.CrLf;
            sql += SQLCst.WHERE + "(" + Cst.SR_TRADE_WHERE_PREDICATE + ")" + Cst.CrLf;
            /*--------------------------------*/
            sql += SQLCst.UNIONALL + Cst.CrLf;
            /*--------------------------------*/
            /* POSREQUEST sur clef de position accessible  
             le POSREQUEST est accessible 
             * - si le book du dealer est l’entité de rattachement de l’utilisateur et 
             * - si le Dealer, le Clearer, l’instrument et le marché sont accessibles 
             */
            sql += SQLCst.SELECT + "p.*" + Cst.CrLf;
            sql += SQLCst.FROM_DBO.ToString() + Cst.OTCml_TBL.POSREQUEST.ToString() + " p" + Cst.CrLf;

            if (!Cst.IsDisabledRestrictOnEntity)
            {
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on (b.IDB=p.IDB_DEALER) and (b.IDA_ENTITY=@SR_IDA_ENTITY)" + Cst.CrLf;
            }
            else
            {
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.BOOK.ToString() + " b on (b.IDB=p.IDB_DEALER)" + Cst.CrLf;
            }

            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ASSET_ETD.ToString() + " asset on (asset.IDASSET = p.IDASSET)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString() + " da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString() + " dc on (dc.IDDC = da.IDDC)" + Cst.CrLf;
            sql += Cst.SR_ACTOR_JOIN + "(a1,p.IDA_DEALER)" + Cst.CrLf;
            sql += Cst.SR_ACTOR_JOIN + "(a2,p.IDA_CLEARER)" + Cst.CrLf;
            sql += Cst.SR_INSTR_JOIN + "(p.IDI)" + Cst.CrLf;
            sql += Cst.SR_MARKET_JOIN + "(dc.IDM)" + Cst.CrLf;
            sql += SQLCst.WHERE + "(p.IDT is null)";
            /*--------------------------------*/
            sql += SQLCst.UNIONALL + Cst.CrLf;
            /*--------------------------------*/
            /* POSREQUEST sur le couple ENTITY/CSS
             le POSREQUEST est accessible 
             * - si l'entité est accesible
             * - si le CSS est accesible*/
            sql += SQLCst.SELECT + "p.*" + Cst.CrLf;
            sql += SQLCst.FROM_DBO.ToString() + Cst.OTCml_TBL.POSREQUEST.ToString() + " p" + Cst.CrLf;
            sql += Cst.SR_ACTOR_JOIN + "(a1,p.IDA_ENTITY)" + Cst.CrLf;
            sql += Cst.SR_ACTOR_JOIN + "(a2,p.IDA_CSS)" + Cst.CrLf;
            sql += SQLCst.WHERE + "(p.IDT is null and p.IDB_DEALER is null)";
            //
            string query = sql.ToString();
            if (!IsUseParameter)
            {
                query = query.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));
                query = query.Replace("@SR_IDA_ENTITY", User.Entity_IdA.ToString());
            }
            query = ReplaceKeyword(query);
            //
            return query;
        }

        /// <summary>
        /// Retourne le join et le where pour réduire le scope aux Dépôts de garantie/Positions Equities accessibles au user connecté
        /// </summary>
        /// <remarks>
        /// Principe sur les parties
        /// Un dépôt de garantie (POSCOLLATERAL) / Position Equity (POSEQUSECURITY) est accessible s’il existe une partie qui vérifie les règles suivantes
        /// - le book associé à la partie est géré par l’entité de rattachement de l’utilisateur connecté
        /// - la partie est accessible
        /// - la contrepartie est accessible
        /// </remarks>
        /// <param name="pAliasTableCollateral">Alias de table PosCollateral/PosEquSecurity</param>
        /// <param name="pSessionId">Identifiant de la session</param>
        /// <param name="pIdAEntity">Entité de rattachement associé à la session connecté</param>
        /// <param name="pJoin"></param>
        /// <param name="pwhere"></param>
        /// <returns></returns>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void GetSQLPosCollateral(string pAliasTableCollateral, out string pJoin, out string pWhere)
        {
            pJoin = string.Empty;
            pWhere = "1=1";

            if (User.IsApplySessionRestrict())
            {
                StrBuilder joinSR = new StrBuilder();
                /* FI 20210330 [XXXXX] Suppression des jointures sr_b_pay et sr_b_rec puisque  Cst.IsDisabledRestrictOnEntity                 
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_pay  on sr_pay.ID={0}.IDA_PAY and sr_pay.CLASS='ACTOR' and sr_pay.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_rec  on sr_rec.ID={0}.IDA_REC and sr_rec.CLASS='ACTOR' and sr_rec.SESSIONID={1}
                left outer join dbo.BOOK sr_b_pay on sr_b_pay.IDB={0}.IDB_PAY
                left outer join dbo.BOOK sr_b_rec on sr_b_rec.IDB={0}.IDB_REC", pAliasTableCollateral, "@SR_SESSIONID") + Cst.CrLf;
                */
                joinSR += StrFunc.AppendFormat(@"
                inner join dbo.SESSIONRESTRICT sr_pay  on sr_pay.ID={0}.IDA_PAY and sr_pay.CLASS='ACTOR' and sr_pay.SESSIONID={1}
                inner join dbo.SESSIONRESTRICT sr_rec  on sr_rec.ID={0}.IDA_REC and sr_rec.CLASS='ACTOR' and sr_rec.SESSIONID={1}", pAliasTableCollateral, "@SR_SESSIONID") + Cst.CrLf;

                if (!IsUseParameter)
                    pJoin = pJoin.Replace("@SR_SESSIONID", DataHelper.SQLString(SessionId));

                if (!Cst.IsDisabledRestrictOnEntity)
                {
                    pWhere = StrFunc.AppendFormat(@"
                    (
                        /*Dealer-Buyer*/
                (sr_b_pay.IDA_ENTITY={0})
                    )
                    or
                    (
                        /*Dealer-Seller*/			
                        (sr_b_rec.IDA_ENTITY={0}) 
                    )", "@SR_IDA_ENTITY");
                }

                if (!IsUseParameter)
                    pWhere = pWhere.Replace("@SR_IDA_ENTITY", User.Entity_IdA.ToString());
            }
        }

        /// <summary>
        /// Remplace dans un string les mots clefs %%SR:POSCOLLATERAL_JOIN%% et %%SR:POSCOLLATERAL_WHERE_PREDICATE%%
        /// <para>Ces mots clefs sont utilisés pour les Dépôts de garantie/Positions Equities</para>
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        private string ReplacePosCollateralKeyword(string pData)
        {
            string ret = pData;
            if (StrFunc.IsFilled(ret) && (ret.IndexOf(Cst.SR_START) >= 0))
            {
                if (ret.Contains(Cst.SR_POSCOLLATERAL_WHERE_PREDICATE) && false == ret.Contains(Cst.SR_POSCOLLATERAL_JOIN))
                    throw new Exception(StrFunc.AppendFormat("Keyword [{0}] is expected", Cst.SR_POSCOLLATERAL_JOIN));
                //
                //%%SR:SR_POSCOLLATERAL_JOIN%%(p.IDPOSCOLLATERAL)
                //%%SR:POSCOLLATERAL_WHERE_PREDICATE%%
                int guard = 0;
                while (ret.Contains(Cst.SR_POSCOLLATERAL_JOIN) & (guard < 100))
                {
                    string[] arg = StrFunc.GetArgumentKeyWord(ret, Cst.SR_POSCOLLATERAL_JOIN);
                    string alias = arg[0];
                    //
                    string sqlJoin = string.Empty;
                    string wherePredicate = "1=1";
                    if (User.IsApplySessionRestrict())
                        GetSQLPosCollateral(alias, out sqlJoin, out wherePredicate);

                    ret = ret.Replace(Cst.SR_POSCOLLATERAL_JOIN + "(" + alias + ")", sqlJoin);
                    ret = ret.Replace(Cst.SR_POSCOLLATERAL_WHERE_PREDICATE, wherePredicate);
                }
                if (guard == 100)
                    throw new Exception("Infinite Loop");
            }
            return ret;
        }
    }

}

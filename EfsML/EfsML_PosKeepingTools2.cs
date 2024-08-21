#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EfsML.Business
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20130328 [18467]  partial class parce pour éviter les extractions multiples
    public sealed partial class PosKeepingTools
    {
        // EG 20230929 [WI715] [26497] Dénouement manuel + automatique à l'échéance (quantité à la livraison incorrecte)        // Utile à la livraison des sous-jacents en multithreading
        static readonly object m_UnderlyerDeliveryLock = new object();

        /// <summary>
        /// Recherche dans POSREQUEST un enregistrement qui pourrait être l'équivalent de {PosRequest}
        /// <para>
        /// La méthode s'applique uniquement si le posrequest s'applique sur une position 
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPosrequest">PosRequest Source</param>
        /// <returns></returns>
        /// FI 20130328 [18467] 
        /// Cette méthode s'applique notamment lors de l'importation d'un POSREQUEST
        /// En mode New sans identifiant externe du POSREQUEST Spheres® recherche l'éventuelle présence d'un POSREQUEST équivalent
        /// En mode Update ou Remove Spheres® ne passe jamais ici => Spheres® connait l'IDPR (grâce à POSREQUEST.EXTLLINK) 
        public static QueryParameters GetQueryExistingKeyPosRequest(string pCS, IPosRequest pPosRequest)
        {
            if (false == pPosRequest.PosKeepingKeySpecified)
                throw new ArgumentException("Posrequest without posKeepingKey specified");

            IPosKeepingKey key = pPosRequest.PosKeepingKey;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "REQUESTTYPE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), 
                ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(pPosRequest.RequestType));
            parameters.Add(new DataParameter(pCS, "DTBUSINESS", DbType.Date), pPosRequest.DtBusiness);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDI), key.IdI);
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDASSET), key.IdAsset);

            //Les assignations issues des chambres ne comportent pas de Dealer
            if (key.IdA_Dealer > 0)
                parameters.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), key.IdA_Dealer);
            if (key.IdB_Dealer > 0)
                parameters.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), key.IdB_Dealer);
            if (key.IdA_EntityDealer > 0)
                parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), key.IdA_EntityDealer);
            
            parameters.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), key.IdA_Clearer);
            parameters.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), key.IdB_Clearer);

            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "pr.IDPR, pr.IDT, pr.QTY, pr.NOTES, pr.REQUESTMODE" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.POSREQUEST + " pr" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "pr.REQUESTTYPE=@REQUESTTYPE" + SQLCst.AND + "pr.DTBUSINESS=@DTBUSINESS" + Cst.CrLf;
            sqlSelect += SQLCst.AND + "pr.IDI = @IDI and pr.IDASSET = @IDASSET" + Cst.CrLf;
            if (parameters.Contains("IDA_DEALER"))
                sqlSelect += SQLCst.AND + "pr.IDA_DEALER = @IDA_DEALER";
            else
                sqlSelect += SQLCst.AND + "pr.IDA_DEALER is null";

            if (parameters.Contains("IDB_DEALER"))
                sqlSelect += SQLCst.AND + "pr.IDB_DEALER = @IDB_DEALER";
            else
                sqlSelect += SQLCst.AND + "pr.IDB_DEALER is null";

            if (parameters.Contains("IDA_ENTITY"))
                sqlSelect += SQLCst.AND + "pr.IDA_ENTITY = @IDA_ENTITY";
            else
                sqlSelect += SQLCst.AND + "pr.IDA_ENTITY is null";

            sqlSelect += SQLCst.AND + "pr.IDA_CLEARER = @IDA_CLEARER";
            sqlSelect += SQLCst.AND + "pr.IDB_CLEARER = @IDB_CLEARER";

            // EG 20151019 [2112] Add restriction sur STATUS != ERROR|WARNING
            sqlSelect += SQLCst.AND + "pr.STATUS not in ('ERROR','WARNING')" + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);
            return qryParameters;
        }


        #region GetQryPosAction_Trade_BySide
        /// <summary>
        /// IDentique à GetQryPosAction_BySide avec pIsByTrade = true
        /// </summary>
        /// <param name="pBuyerSeller"></param>
        /// <param name="pIsAllAction"></param>
        /// <param name="pIsExceptCurrentIdPR"></param>
        /// <returns></returns>
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_Trade_BySide(BuyerSellerEnum pBuyerSeller, bool pIsAllAction, bool pIsExceptCurrentIdPR)
        {
            return GetQryPosAction_BySide(pBuyerSeller, false, pIsAllAction, true, pIsExceptCurrentIdPR);
        }
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_Trade_BySide(BuyerSellerEnum pBuyerSeller, bool pIsPrevious, bool pIsAllAction, bool pIsExceptCurrentIdPR)
        {
            return GetQryPosAction_BySide(pBuyerSeller, pIsPrevious, pIsAllAction, true, pIsExceptCurrentIdPR);
        }
        #endregion GetQryPosAction_Trade_BySide

        #region GetQryPosAction_BySide
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_BySide(BuyerSellerEnum pBuyerSeller)
        {
            return GetQryPosAction_BySide(pBuyerSeller, false);
        }
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_BySide(BuyerSellerEnum pBuyerSeller, bool pIsPrevious)
        {
            return GetQryPosAction_BySide(pBuyerSeller, pIsPrevious, false, false, false);
        }
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_BySide(BuyerSellerEnum pBuyerSeller, bool pIsPrevious, bool pIsAllAction)
        {
            return GetQryPosAction_BySide(pBuyerSeller, pIsPrevious, pIsAllAction, false, false);
        }
        // EG 20170412 [23081] New Refactoring
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public static string GetQryPosAction_BySide(BuyerSellerEnum pBuyerSeller, bool pIsPrevious, bool pIsAllAction, bool pIsByTrade, bool pIsExceptCurrentIdPR)
        {
            string sqlSelect = @"select pad.{0} as IDT, ";
            if (pIsPrevious)
            {
                //  POSITION VEILLE : on somme les QTY répondants à : 
                //
                // case 1. les actions du jours non annulées jour
                //         => si pIsAllAction = TOUTES LES ACTIONS  
                //            Transfert (POT), Correction (POC) , Dénouement manuel et automatique (ABN|NEX|NAS|ASS|EXE|AUTOABN|AUTOASS|AUTOEXE)
                //         => si (false == pIsAllAction)
                //            Transfert (POT), Correction (POC)
                //         (pa.DTBUSINESS = @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                //
                // case 2. les réelles compensations (non annulées)
                //         (pa.DTBUSINESS < @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                //
                // case 3. Les recompensation suite à décompensation partielle 
                //         (pa.DTBUSINESS = @DTBUSINESS) and (pa.DTBUSINESS > pr.DTBUSINESS)

                string dayRequestType = "'POT', 'POC'";
                if (pIsAllAction)
                    dayRequestType += ", 'CLEARSPEC', 'AUTOABN' ,'ABN', 'NEX', 'NAS', 'AUTOASS', 'ASS', 'AUTOEXE', 'EXE'";

                sqlSelect += @"sum(
                case when (pr.REQUESTTYPE in (" + dayRequestType + @")) and  (pa.DTBUSINESS = @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)) then 1 else
                case when (pa.DTBUSINESS < @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)) then 1 else 
                case when (pa.DTBUSINESS = @DTBUSINESS) and (pr.DTBUSINESS < @DTBUSINESS) then 1 else 
                0 end end end * isnull(pad.QTY,0)) as QTY
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
                where (pa.DTBUSINESS <= @DTBUSINESS)";
            }
            else
            {
                //  POSITION JOUR : Classique
                sqlSelect += @"sum(
				isnull(pad.QTY,0)) as QTY
				from dbo.POSACTIONDET pad
				inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
				where (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))";
            }

            return CompleteQryPosAction_BySide(sqlSelect, pBuyerSeller, pIsByTrade, pIsExceptCurrentIdPR);
        }
        #endregion GetQryPosAction_BySide
        #region GetQryPosAction_DtSettlt_BySide
        /// <summary>
        /// Gestion de la date DTSETTLT présente sur TRADEINSTRUMENT pour les groupe de Produit (OTC|SEC)
        /// Appelé par la méthode
        /// </summary>
        /// <param name="pBuyerSeller"></param>
        /// <param name="pIsPrevious"></param>
        /// <returns></returns>
        // EG 20170412 [23081] New Refactoring
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public static string GetQryPosAction_DtSettlt_BySide(BuyerSellerEnum pBuyerSeller, bool pIsPrevious)
        {
            // {1} = Jointure TRADEINSTRUMENT pour IDT_BUY et IDT_SELL pour la gestion DTSETTLT
            string sqlJoin = @"
            left outer join dbo.TRADE trb on (trb.IDT = pad.IDT_BUY) 
            left outer join dbo.TRADE trs on (trs.IDT = pad.IDT_SELL)";

            string sqlSelect = @"select pad.{0} as IDT, ";

            // Multiplicateur pour la gestion DTSETTLT
            sqlSelect += @"sum(
            case when (isnull(trb.DTSETTLT, @DTBUSINESS) <= @DTBUSINESS and isnull(trs.DTSETTLT, @DTBUSINESS) <= @DTBUSINESS) then 1 else 0 end * ";

            if (pIsPrevious)
            {
                //  POSITION VEILLE : on somme les QTY répondants à : 
                //
                // case 1. les actions du jours non annulées jour
                //         Transfert (POT), Correction (POC)
                //         (pa.DTBUSINESS = @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                //
                // case 2. les réelles compensations (non annulées)
                //         (pa.DTBUSINESS < @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))
                //
                // case 3. Les recompensation suite à décompensation partielle 
                //         (pa.DTBUSINESS = @DTBUSINESS) and (pa.DTBUSINESS > pr.DTBUSINESS)

                sqlSelect += @"
                case when (pr.REQUESTTYPE in ('POC', 'POT')) and  (pa.DTBUSINESS = @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)) then 1 else
                case when (pa.DTBUSINESS < @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS)) then 1 else 
                case when (pa.DTBUSINESS = @DTBUSINESS) and (pr.DTBUSINESS < @DTBUSINESS) then 1 else 
                0 end end end * isnull(pad.QTY,0)) as QTY
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)" + Cst.CrLf;
                sqlSelect += sqlJoin + Cst.CrLf;
                sqlSelect += @"where (pa.DTBUSINESS <= @DTBUSINESS)";
            }
            else
            {
                //  POSITION JOUR : Classique
                sqlSelect += @"
				isnull(pad.QTY,0)) as QTY
				from dbo.POSACTIONDET pad
				inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)" + Cst.CrLf;
                sqlSelect += sqlJoin + Cst.CrLf;
                sqlSelect += @"where (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))";
            }

            return CompleteQryPosAction_BySide(sqlSelect, pBuyerSeller, false, false);
        }
        #endregion GetQryPosAction_DtSettlt_BySide
        #region GetQryPosAction_Entry_BySide
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_Entry_BySide(BuyerSellerEnum pBuyerSeller)
        {
            return GetQryPosAction_Entry_BySide(pBuyerSeller, false, false, false);
        }
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_Entry_BySide(BuyerSellerEnum pBuyerSeller, bool pIsDailyClosing)
        {
            return GetQryPosAction_Entry_BySide(pBuyerSeller, pIsDailyClosing, false, false);
        }
        // EG 20170412 [23081] New Refactoring
        public static string GetQryPosAction_Entry_BySide(BuyerSellerEnum pBuyerSeller, bool pIsDailyClosing, bool pIsByTrade, bool pIsExceptCurrentIdPR)
        {
            string sqlSelect = @"select pad.{0} as IDT, ";
            //  POSITION JOUR : Pour la mise à jour des clôtures: 
            //     
            //  1. Si pIsDailyClosing = true: 
            //     Sommer les QTY des clôtures* du jour pour pouvoir remonter les trades qui ont subi des clôtures du jour.
            //     
            //  2. Si pIsDailyClosing = false:
            //     Ne pas considérer les clôtures du jour afin de toutes les rejouer en respectant la méthode de gestion de la position: FIFO, ...
            //     
            //     * (pr.REQUESTTYPE in ('CLEAREOD','CLEARBULK','UPDENTRY','ENTRY')) and (pa.DTBUSINESS = @DTBUSINESS)
            //     

            string dayClearingRequestType = "'CLEAREOD','CLEARBULK','UPDENTRY','ENTRY'";

            if (pIsDailyClosing)
            {
                sqlSelect += @"sum(isnull(pad.QTY,0)) as QTY
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
                where (pa.DTBUSINESS = @DTBUSINESS) and (pr.REQUESTTYPE in (" + dayClearingRequestType + @")) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))";
            }
            else
            {
                sqlSelect += @"sum(
                case when (pr.REQUESTTYPE in (" + dayClearingRequestType + @")) and (pa.DTBUSINESS = @DTBUSINESS) then 0 else 1 end * isnull(pad.QTY,0)) as QTY
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                inner join dbo.POSREQUEST pr on (pr.IDPR = pa.IDPR)
                where (pa.DTBUSINESS <= @DTBUSINESS) and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))";
            }

            return CompleteQryPosAction_BySide(sqlSelect, pBuyerSeller, pIsByTrade, pIsExceptCurrentIdPR);
        }
        #endregion GetQryPosAction_Entry_BySide

        #region CompleteQryPosAction_BySide
        // EG 20170412 [23081] New Refactoring
        private static string CompleteQryPosAction_BySide(string pSqlSelect, BuyerSellerEnum pBuyerSeller, bool pIsByTrade, bool pIsExceptCurrentIdPR)
        {
            string sqlSelect = pSqlSelect;
            // Position pour un trade donné
            if (pIsByTrade)
                sqlSelect += "and (pad.{0} = @IDT)";

            // Position avec exclusion de l'IDPR courant
            if (pIsExceptCurrentIdPR)
                sqlSelect += "and (isnull(pa.IDPR, 0) <> @IDPR)";

            // Substitution IDT en fonction de Buyer|Seller
            sqlSelect += Cst.CrLf + "group by pad.{0}";
            sqlSelect = String.Format(sqlSelect, (pBuyerSeller == BuyerSellerEnum.BUYER ? "IDT_BUY" : "IDT_SELL"));
            return sqlSelect;
        }
        #endregion CompleteQryPosAction_BySide

        /// <summary>
        /// Méthode utilisé pour la gestion de la livraison d'un sous-jacent après dénouement d'option
        /// - Suppresion des trades sous-jacent déjà générés sur un traitement EOD précédent (sur la même journée)
        /// - Insertion d'un POSREQUEST si inexistant (UNLDLVR)
        /// </summary>
        // EG 20230929 [WI715] [26497] New
        public static Cst.ErrLevel SetPosRequestUnderlyerDelivery(string pCS, IDbTransaction pDbTransaction, IProductBase pProduct, IPosRequest pPosRequest, AppSession pSession)
        {
            IPosRequestDetOption requestOption = pPosRequest.DetailBase as IPosRequestDetOption;
            if (requestOption.PaymentFeesSpecified)
            {
                foreach (IPayment payment in requestOption.PaymentFees)
                    payment.Efs_Payment = null;
            }

            IPosRequestOption posRequestChild = ((IPosRequestOption)pPosRequest).Clone() as IPosRequestOption;
            posRequestChild.Detail.PaymentFeesSpecified = false;
            posRequestChild.RequestType = Cst.PosRequestTypeEnum.UnderlyerDelivery;

            Cst.ErrLevel errLevel = DeletePosRequestUnderlyerDelivery(pCS, pDbTransaction, pProduct, pPosRequest, pSession.IdA);

            if (Cst.ErrLevel.SUCCESS == errLevel)
            {
                // EG 20230927 [WI715][26497] Dénouement manuel + automatique à l'échéance : Passage paramètre RequestType de la source appelante
                Nullable<int> idPR = PosKeepingTools.GetExistingKeyPosRequest(pCS, pDbTransaction, posRequestChild, 
                    out _, out _, out _, out _, pPosRequest.RequestType);
                if (false == idPR.HasValue)
                {
                    posRequestChild.StatusSpecified = true;
                    posRequestChild.RequestMode = SettlSessIDEnum.EndOfDay;
                    posRequestChild.Status = ProcessStateTools.StatusPendingEnum;
                    posRequestChild.Detail.Underlyer.SetPosRequestSource(pPosRequest.IdPR, pPosRequest.Qty);

                    errLevel = PosKeepingTools.AddNewPosRequest(pCS, pDbTransaction, out int newIdPR, posRequestChild, pSession, null, null);
                    posRequestChild.IdPR = newIdPR;
                }
            }
            return errLevel;
        }

        /// <summary>
        /// Méthode utilisé pour la gestion de la livraison d'un sous-jacent après dénouement d'option
        /// Recherche existence d'un POSREQUEST (UNLDLVR) 
        /// - Si l'IdT présent dans le détail du POSREQUEST trouvé est différent de celui du dénouement(iDT)
        ///   alors
        ///     Suppresion des trades sous-jacent déjà générés (+ autres tables) 
        ///     sur un traitement EOD précédent sur la même journée.
        /// - Suppression ou mise à jour du POSREQUEST (UNLDLVR)
        /// </summary>
        /// <returns></returns>
        // EG 20230929 [WI715] [26497] New
        public static Cst.ErrLevel DeletePosRequestUnderlyerDelivery(string pCS, IDbTransaction pDbTransaction, IProductBase pProduct, IPosRequest pPosRequest, int pIdA)
        {
            IPosRequestOption posRequestChild = ((IPosRequestOption)pPosRequest).Clone() as IPosRequestOption;
            posRequestChild.RequestType = Cst.PosRequestTypeEnum.UnderlyerDelivery;
            Nullable<int> idPR = PosKeepingTools.GetExistingKeyPosRequest(pCS, pDbTransaction, posRequestChild, 
                out int? idT, out _, out _, out _, pPosRequest.RequestType);
            if (idPR.HasValue)
            {
                // La demande de livraison sous-jacent existe déjà
                // 1. le trade a déjà été créé : idT de retour (ID trade sous-jacent ) <> de posRequestChild.idT (ID trade option)
                //    ON SUPPRIME LE TRADE GENERE ET MODIFIE LA DEMANDE QUI SERA RETRAITEE
                // 2. le trade n'a pas été créé : idT = posRequestChild.idT = ID trade option
                //    ON MODIFIE LA DEMANDE QUI SERA TRAITEE
                if (idT.HasValue && (idT.Value != posRequestChild.IdT))
                {
                    TradeRDBMSTools.DeleteEvent(pCS, pDbTransaction, idT.Value, Cst.ProductGProduct_FUT, pIdA, OTCmlHelper.GetDateSysUTC(pCS));
                    TradeRDBMSTools.DeleteTrade(pDbTransaction, idT.Value, posRequestChild.IdT);
                }

                lock (m_UnderlyerDeliveryLock)
                {
                    IPosRequestOption posRequestUnderlyer = PosKeepingTools.GetPosRequest(pCS, pDbTransaction, pProduct, idPR.Value) as IPosRequestOption;
                    if (ArrFunc.IsFilled(posRequestUnderlyer.Detail.Underlyer.PosRequestSource))
                    {
                        if (1 == posRequestUnderlyer.Detail.Underlyer.PosRequestSource.Length)
                        {
                            DeletePosRequest(pCS, pDbTransaction, idPR.Value);
                        }
                        else
                        {
                            posRequestUnderlyer.Detail.Underlyer.DeletePosRequestSource(pPosRequest.IdPR);
                            posRequestUnderlyer.Qty = posRequestUnderlyer.Detail.GetTotalQtySource();
                            posRequestUnderlyer.StatusSpecified = true;
                            posRequestUnderlyer.Status = ProcessStateTools.StatusPendingEnum;
                            UpdatePosRequest(pCS, pDbTransaction, idPR.Value, posRequestUnderlyer, pIdA, null, null);
                        }
                    }
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
    }
}

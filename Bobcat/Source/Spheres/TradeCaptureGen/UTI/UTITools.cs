#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;

#endregion Using Directives

namespace EFS.TradeInformation
{
    public sealed class UTITools
    {

        /// <summary>
        /// <see cref="UTIRuleAttribute.IsREFIT"/>
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        /// FI 20240425 [26593] UTI/PUTI REFIT Add Method
        public static Boolean IsREFITRULE(UTIRule rule)
        {
            UTIRuleAttribute attribute = ReflectionTools.GetAttribute<UTIRuleAttribute>(rule);
            return (null != attribute) && attribute.IsREFIT;
        }

        /// <summary>
        /// <see cref="UTIRuleAttribute.IsCPP"/>
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        /// FI 20240425 [26593] UTI/PUTI REFIT Add Method
        public static Boolean IsCPPRULE(UTIRule rule)
        {
            UTIRuleAttribute attribute = ReflectionTools.GetAttribute<UTIRuleAttribute>(rule);
            return (null != attribute) && attribute.IsCPP;
        }

        /// <summary>
        /// <see cref="UTIRuleAttribute.IsSpheres"/>
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        /// FI 20240425 [26593] UTI/PUTI REFIT Add Method
        public static Boolean IsSpheresRULE(UTIRule rule)
        {
            UTIRuleAttribute attribute = ReflectionTools.GetAttribute<UTIRuleAttribute>(rule);
            return (null != attribute) && attribute.IsSpheres;
        }

        /// <summary>
        /// <see cref="UTIRuleAttribute.IsLevel2"/>
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        /// FI 20240425 [26593] UTI/PUTI REFIT Add Method
        public static Boolean IsLevel2(UTIRule rule)
        {
            UTIRuleAttribute attribute = ReflectionTools.GetAttribute<UTIRuleAttribute>(rule);
            return (null != attribute) && attribute.IsLevel2;
        }



        /// <summary>
        /// Représente les valeurs de retour de la méthode 
        /// </summary>
        // EG 20180221 Upd public pour gestion Asynchrone (Calcul des UTI traitement EOD)
        public enum StatusCalcUTI
        {
            /// <summary>
            /// 
            /// </summary>
            ValuedWithSuccess,
            /// <summary>
            /// Le calcul des UTI n'est pas disponible
            /// </summary>
            NotAvailable,
            /// <summary>
            /// Le trade comporte déjà un UTI
            /// <para>Spheres n'effectue pas le calcul</para>
            /// </summary>
            AlreadyExists,
            /// <summary>
            /// 
            /// </summary>
            NotComputable,
            /// <summary>
            /// 
            /// </summary>
            ErrorOccured,
        }

        /// <summary>
        ///  Retourne une nouvelle instance de UTIComponents à partir d'un dataDocument
        ///  <para>Seules les allocations ETD sont gérées</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataDocument">Représente le trade</param>
        /// <param name="pTradeIdentification">Identification de trade sous Spheres (doit être renseigné uniquement si l'identification du trade est connue)</param>
        /// <exception cref="InvalidOperationException si le trade n'est pas une allocation"></exception>
        /// FI 20140602 [20028] Alimentation de Trade_executionID
        /// EG 20140526 [XXXXX] Replace etd by exchangeTraded
        /// FI 20140916 [20353] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        public static UTIComponents InitUTIComponentsFromDataDocument(string pCS, IDbTransaction pDbTransaction, DataDocumentContainer pDataDocument, SpheresIdentification pTradeIdentification)
        {
            UTIComponents utiComponents = new UTIComponents();
            ExchangeTradedContainer exchangeTraded = pDataDocument.CurrentProduct.NewExchangeTraded();
            //if (false == pDataDocument.currentProduct.isExchangeTradedDerivative)
            //    throw new InvalidOperationException(StrFunc.AppendFormat("Product: {0} is not valid", pDataDocument.currentProduct.productBase.ProductName));

            //ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)pDataDocument.currentProduct.product);

            if (null != exchangeTraded)
            {
                //if (false == etd.isOneSide)
                if (false == exchangeTraded.IsOneSide)
                    throw new InvalidOperationException(StrFunc.AppendFormat("trade is not an allocation"));

                ExchangeTradedDerivativeContainer etd = null;
                if (pDataDocument.CurrentProduct.IsExchangeTradedDerivative)
                    etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)pDataDocument.CurrentProduct.Product, pDataDocument);

                #region alimentation des infos qui porte sur le trade
                
                

                //Trade_tradeSide
                if (exchangeTraded.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                    utiComponents.Trade_tradeSide = SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.BUYER);
                else if (exchangeTraded.IsDealerBuyerOrSeller(BuyerSellerEnum.SELLER))
                    utiComponents.Trade_tradeSide = SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.SELLER);

                //Trade_tradeDate
                utiComponents.Trade_tradeDate = pDataDocument.TradeDate;

                //FI 20140602 [20028] alimentation de Trade_businessDate (faille détectée dans le cadre du ticket 20028) 
                //Trade_businessDate
                if (exchangeTraded.TradeCaptureReport.ClearingBusinessDateSpecified)
                    utiComponents.Trade_businessDate = exchangeTraded.TradeCaptureReport.ClearingBusinessDate.DateValue;

                //Trade_trdType
                utiComponents.Trade_trdType = exchangeTraded.TradeCaptureReport.TrdTypeSpecified ?
                    ReflectionTools.ConvertEnumToString<TrdTypeEnum>(exchangeTraded.TradeCaptureReport.TrdType) : string.Empty;
                //trade_orderID
                utiComponents.Trade_orderID = string.Empty;
                if (ArrFunc.IsFilled(exchangeTraded.TradeCaptureReport.TrdCapRptSideGrp))
                {
                    if (exchangeTraded.TradeCaptureReport.TrdCapRptSideGrp[0].OrderIdSpecified)
                        utiComponents.Trade_orderID = exchangeTraded.TradeCaptureReport.TrdCapRptSideGrp[0].OrderId;
                }

                // FI 20140602 [20028] alimentation de Trade_executionID 
                //Trade_executionID
                utiComponents.Trade_executionID = string.Empty;
                if (exchangeTraded.TradeCaptureReport.ExecIdSpecified)
                    utiComponents.Trade_executionID = exchangeTraded.TradeCaptureReport.ExecId;

                //PL 20160429 [22107] EUREX C7 3.0 Release - alimentation de Trade_positionID
                //Trade_positionID
                utiComponents.Trade_positionID = string.Empty;
                if (ArrFunc.IsFilled(exchangeTraded.TradeCaptureReport.TrdCapRptSideGrp))
                {
                    IFixRelatedPositionGrp reltdPos = RptSideTools.GetRelatedPositionGrp(exchangeTraded.TradeCaptureReport.TrdCapRptSideGrp[0], RelatedPositionIDSourceEnum.PositionID, false);
                    if (reltdPos != null)
                        utiComponents.Trade_positionID = reltdPos.ID;
                }

                //Trade_id et Trade_Identifier
                if (null != pTradeIdentification)
                {
                    utiComponents.Trade_id = pTradeIdentification.OTCmlId;
                    utiComponents.Trade_Identifier = pTradeIdentification.Identifier;
                }
                
                #endregion

                #region alimentation des infos qui portent sur le DC

                if (null != etd)
                {
                    etd.SetDerivativeContract(pCS, pDbTransaction);
                    if (null != etd.DerivativeContract)
                    {
                        utiComponents.DC_IdI = pDataDocument.CurrentProduct.ProductBase.ProductType.OTCmlId;
                        utiComponents.DC_ContractType = etd.DerivativeContract.ContractTypeEnum.ToString();
                        utiComponents.DC_Category = etd.DerivativeContract.Category;
                        utiComponents.DC_Symbol = etd.DerivativeContract.ContractSymbol;
                        utiComponents.DC_Attribute = etd.DerivativeContract.Attribute;
                        //FI 20140916 [20353]
                        utiComponents.DC_StrikeDecLocator = etd.DerivativeContract.StrikeDecLocator;
                    }
                }

                #endregion

                #region alimentation des infos qui portent sur le marché et sa chambre
                pDataDocument.CurrentProduct.GetMarket(pCS, pDbTransaction, out SQL_Market sqlMarket);
                if (null != sqlMarket)
                {
                    utiComponents.Market_Id = sqlMarket.Id;
                    utiComponents.Market_MIC = sqlMarket.ISO10383_ALPHA4;
                    if (sqlMarket.IdA > 0)
                    {
                        SQL_Actor sqlActor = new SQL_Actor(pCS, sqlMarket.IdA)
                        {
                            DbTransaction = pDbTransaction
                        };
                        // FI 20240425[26593] UTI / PUTI REFIT add column
                        sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "BIC", "ISO17442" });
                        utiComponents.Css_Id = sqlActor.Id;
                        utiComponents.Css_Identifier = sqlActor.Identifier;
                        utiComponents.Css_BIC = sqlActor.BICorNull;
                        // FI 20240425[26593] UTI / PUTI REFIT
                        utiComponents.css_LEI = sqlActor.ISO17442;
                    }
                }
                #endregion

                #region alimentation des informations qui portent sur l'asset et l'échéance
                if (null != etd)
                {
                    etd.SetAssetETD(pCS, pDbTransaction);
                    if (null != etd.AssetETD)
                    {
                        utiComponents.Asset_PutCall = etd.AssetETD.PutCall;
                        utiComponents.Asset_StrikePrice = etd.AssetETD.StrikePrice;
                        if (etd.AssetETD.Maturity_MaturityDateSys != DateTime.MinValue)
                            utiComponents.Asset_MaturityDate = etd.AssetETD.Maturity_MaturityDateSys;
                        else if (etd.AssetETD.Maturity_MaturityDate != DateTime.MinValue)
                            utiComponents.Asset_MaturityDate = etd.AssetETD.Maturity_MaturityDate;
                        utiComponents.Asset_ISIN = etd.AssetETD.ISINCode;
                        // FI 20140916 [20353] Alimentation de utiComponents.Maturity_MaturityMonthYear
                        utiComponents.Asset_MaturityMonthYear = etd.AssetETD.Maturity_MaturityMonthYear;
                        //LP 20240625 [WI936] UTI/PUTI REFIT
                        utiComponents.Asset_CfiCode = etd.AssetETD.CFICode;
                    }
                }
                #endregion

                #region alimentation des informations sur le dealer (ses propriétés intrinsèque, son book, son entité)
                IFixParty fixPartyDealer = exchangeTraded.GetDealer();
                if (null != fixPartyDealer)
                {
                    IParty party = pDataDocument.GetParty(fixPartyDealer.PartyId.href);
                    // EG 20150622 refactoring condition ((&& replace &)
                    if ((null != party) && (party.OTCmlId > 0))
                    {
                        ActorRoleCollection actorRole = pDataDocument.GetActorRole(pCS, pDbTransaction);

                        SQL_Actor sqlActor = new SQL_Actor(pCS, party.OTCmlId)
                        {
                            DbTransaction = pDbTransaction
                        };
                        sqlActor.LoadTable(new string[] { "IDA", "BIC", "ISO17442" });
                        utiComponents.Dealer_Actor_id = sqlActor.Id;
                        utiComponents.Dealer_Actor_BIC = sqlActor.BICorNull;
                        utiComponents.Dealer_Actor_LEI = sqlActor.ISO17442;
                        utiComponents.Dealer_Actor_IsCLIENT = actorRole.IsActorRole(sqlActor.Id, RoleActor.CLIENT);

                        IBookId bookId = pDataDocument.GetBookId(party.Id);
                        if (null != bookId)
                            utiComponents.Dealer_Book_Id = bookId.OTCmlId;

                        int idAEntity = BookTools.GetEntityBook(pCS, pDbTransaction, bookId.OTCmlId);
                        utiComponents.Entity_id = idAEntity;
                        if (idAEntity > 0)
                        {
                            utiComponents.Entity_RegulatoryOffice_Id
                                 = RegulatoryTools.GetActorRegulatoryOffice(pCS, pDbTransaction, utiComponents.Entity_id);

                            SQL_Actor sqlEntity = new SQL_Actor(pCS, idAEntity)
                            {
                                DbTransaction = pDbTransaction
                            };
                            sqlEntity.LoadTable(new string[] { "IDA", "BIC", "ISO17442" });
                            utiComponents.Entity_LEI = sqlEntity.ISO17442;
                            //
                            // Alimentation du code membre de l'entité de la chambre uniquement pour CCeG
                            // Seul le calcul de l'UTI CCeG nécessite ce code (tuning)
                            // FI 20140916 [20353] Alimentation du du code membre de l'entité pour EUREX (use for PUTI)
                            if (StrFunc.IsFilled(utiComponents.Css_BIC))
                            {
                                switch (utiComponents.Css_BIC)
                                {
                                    case UTIBuilder.BIC_CCeG:
                                    case UTIBuilder.BIC_EUREX:
                                        utiComponents.Entity_CSSMemberCode = GetCSSMemberCode(pCS, pDbTransaction, idAEntity, utiComponents.Css_BIC);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region alimentation des informations sur le clearer (ses propriétés intrinsèque, son book)
                IFixParty fixPartyClearer = exchangeTraded.GetClearerCustodian();
                if (null != fixPartyClearer)
                {
                    IParty party = pDataDocument.GetParty(fixPartyClearer.PartyId.href);
                    // EG 20150622 refactoring condition ((&& replace &)
                    if ((null != party) && (party.OTCmlId > 0))
                    {
                        SQL_Actor sqlActor = new SQL_Actor(pCS, party.OTCmlId)
                        {
                            DbTransaction = pDbTransaction
                        };
                        sqlActor.LoadTable(new string[] { "IDA", "BIC", "ISO17442" });
                        utiComponents.Clearer_Actor_Id = sqlActor.Id;
                        utiComponents.Clearer_Actor_BIC = sqlActor.BICorNull;
                        utiComponents.Clearer_Actor_LEI = sqlActor.ISO17442;
                        utiComponents.Clearer_Actor_IsCCP = (fixPartyClearer.PartyRoleSpecified && fixPartyClearer.PartyRole == PartyRoleEnum.ClearingOrganization);
                        utiComponents.Clearer_RegulatoryOffice_Id =
                             RegulatoryTools.GetActorRegulatoryOffice(pCS, pDbTransaction, party.OTCmlId);

                        IBookId bookId = pDataDocument.GetBookId(party.Id);
                        if (null != bookId)
                        {
                            utiComponents.Clearer_Book_Id = bookId.OTCmlId;
                            //FI 20140602 [20023] Mise en commentaire car donnée non utilisée dans les algorithmes
                            //utiComponents.Clearer_Book_Identifier = bookId.Value;

                            SQL_Book sqlBook = new SQL_Book(pCS, bookId.OTCmlId)
                            {
                                DbTransaction = pDbTransaction
                            };
                            sqlBook.LoadTable(new string[] { "IDB", "IDA" });
                            //
                            //FI 20140916 [20353]  Alimentation de Clearer_Compartment_Code 
                            if (ActorTools.IsActorWithRole(pCS, pDbTransaction, sqlBook.IdA, new RoleActor[] { RoleActor.CCLEARINGCOMPART,
                                                                                                RoleActor.HCLEARINGCOMPART, RoleActor.MCLEARINGCOMPART }, 0))
                            {

                                Pair<string, string> css = new Pair<string, string>(Cst.OTCml_ActorIdScheme, utiComponents.Css_Id.ToString());
                                Pair<string, string> actorCompart = new Pair<string, string>(Cst.OTCml_ActorIdScheme, sqlBook.IdA.ToString());
                                utiComponents.Clearer_Compartment_Code = MarketTools.GetClearingCompartCode(pCS, pDbTransaction, actorCompart, css);
                            }
                        }
                    }
                }
                #endregion
            }
            return utiComponents;
        }

        /// <summary>
        ///  Retourne une nouvelle instance de UTIComponents à partir d'un DataRow
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="dr"></param>
        /// <param name="pUtiType"></param>
        /// <returns></returns>
        /// FI 20140602 [20023] Modification de la signature de la méthode pUtiType => Nullable
        /// FI 20140916 [20353] Modify
        /// FI 20140916 [20353] TODO il faut changer la requête de manière à alimenter les nouveaux champs
        /// FI 20150108 [20648] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        public static UTIComponents InitUTIComponentsFromDataRow(string pCS, IDbTransaction pDbTransaction, DataRow dr, Nullable<UTIType> pUtiType)
        {
            UTIComponents utiComponents = new UTIComponents();

            Boolean isUTI = ((!pUtiType.HasValue) || (pUtiType == UTIType.UTI));

            if (isUTI)
            {
                utiComponents.StatusUTI_Dealer = dr["STATUSUTI_DEAL"].ToString();
                utiComponents.StatusUTI_Clearer = dr["STATUSUTI_CLEAR"].ToString();
            }
            utiComponents.StatusPUTI_Dealer = dr["STATUSPUTI_DEAL"].ToString();
            utiComponents.StatusPUTI_Clearer = dr["STATUSPUTI_CLEAR"].ToString();

            // Trade
            utiComponents.Trade_id = Convert.ToInt32(dr["IDT"]);
            utiComponents.Trade_Identifier = dr["IDENTIFIER"].ToString(); ;
            utiComponents.Trade_tradeDate = Convert.ToDateTime(dr["DTTRADE"]);
            utiComponents.Trade_businessDate = Convert.ToDateTime(dr["DTBUSINESS"]);
            utiComponents.Trade_tradeSide = dr["BUYER_SELLER"].ToString();
            utiComponents.Trade_executionID = dr["EXECUTIONID"].ToString();
            //PL 20160428 [22107] EUREX C7 3.0 Release - Alimentation de Trade_reltdPosID
            utiComponents.Trade_positionID = dr["RELTDPOSID"].ToString();
            // FI 20140602 [20028] Alimentation de Trade_orderID
            utiComponents.Trade_orderID = dr["ORDERID"].ToString();
            utiComponents.Trade_trdType = dr["TRDTYPE"].ToString();

            // Market
            utiComponents.Market_Id = Convert.ToInt32(dr["IDM"]);
            utiComponents.Market_MIC = dr["ISO10383_ALPHA4"].ToString();

            // CSS
            utiComponents.Css_Id = Convert.ToInt32(dr["IDA_CSSCUST"]);
            utiComponents.Css_BIC = dr["BIC_CSSCUST"].ToString();
            // FI 20240425[26593] UTI / PUTI REFIT
            utiComponents.css_LEI = dr["LEI_CSSCUST"].ToString();
            utiComponents.Css_Identifier = dr["IDENTIFIER_CSSCUST"].ToString();

            // DC
            utiComponents.DC_IdI = Convert.ToInt32(dr["IDI"]);
            utiComponents.DC_ContractType = dr["CONTRACTTYPE_DC"].ToString();
            utiComponents.DC_Category = dr["CATEGORY_DC"].ToString();
            utiComponents.DC_Symbol = dr["CONTRACTSYMBOL_DC"].ToString();
            utiComponents.DC_Attribute = dr["CONTRACTATTRIBUTE_DC"].ToString();
            /// FI 20150108 test STRIKEDECLOCATOR_DC is not null 
            if ((utiComponents.DC_Category == "O") && (dr["STRIKEDECLOCATOR_DC"] != Convert.DBNull))
                utiComponents.DC_StrikeDecLocator = Convert.ToInt32(dr["STRIKEDECLOCATOR_DC"]);


            // Asset
            utiComponents.Asset_Id = Convert.ToInt32(dr["IDASSET"]);
            if (utiComponents.DC_Category == "O")
            {
                utiComponents.Asset_PutCall = dr["PUTCALL_ASSET"].ToString();
                if (dr["STRIKEPRICE_ASSET"] != Convert.DBNull)
                    utiComponents.Asset_StrikePrice = Convert.ToDecimal(dr["STRIKEPRICE_ASSET"]);
            }
            utiComponents.Asset_ISIN = dr["ISINCODE_ASSET"].ToString();
            utiComponents.Asset_CfiCode = dr["CFICODE_ASSET"].ToString();

            if (dr["MATURITYDATE"] != Convert.DBNull)
                utiComponents.Asset_MaturityDate = Convert.ToDateTime(dr["MATURITYDATE"]);
            // FI 20140916 [20353] add
            utiComponents.Asset_MaturityMonthYear = dr["MATURITYMONTHYEAR"].ToString();

            // Entity
            if (dr["IDA_ENT"] != Convert.DBNull)
            {
                utiComponents.Entity_id = Convert.ToInt32(dr["IDA_ENT"]);
                utiComponents.Entity_LEI = dr["LEI_ENT"].ToString();
                utiComponents.Entity_CSSMemberCode = dr["CSSMEMBERCODE_ENT"].ToString();
                utiComponents.Entity_RegulatoryOffice_Id =
                    RegulatoryTools.GetActorRegulatoryOffice(pCS, pDbTransaction, utiComponents.Entity_id);
            }

            // Dealer
            if (dr["IDA_DEAL"] != Convert.DBNull)
            {
                utiComponents.Dealer_Actor_id = Convert.ToInt32(dr["IDA_DEAL"]);
                utiComponents.Dealer_Actor_BIC = dr["BIC_DEAL"].ToString();
                utiComponents.Dealer_Actor_LEI = dr["LEI_DEAL"].ToString();
                utiComponents.Dealer_Actor_IsCLIENT = BoolFunc.IsTrue(dr["ISCLIENT_DEAL"]);
            }
            if (dr["IDB_DEAL"] != Convert.DBNull)
                utiComponents.Dealer_Book_Id = Convert.ToInt32(dr["IDB_DEAL"]);


            // Clearer
            if (dr["IDA_CLEAR"] != Convert.DBNull)
            {
                utiComponents.Clearer_Actor_Id = Convert.ToInt32(dr["IDA_CLEAR"]);
                utiComponents.Clearer_Actor_BIC = dr["BIC_CLEAR"].ToString();
                utiComponents.Clearer_Actor_LEI = dr["LEI_CLEAR"].ToString();
                utiComponents.Clearer_Actor_IsCCP = BoolFunc.IsTrue(dr["ISCCP_CLEAR"]);
                utiComponents.Clearer_RegulatoryOffice_Id =
                    RegulatoryTools.GetActorRegulatoryOffice(pCS, pDbTransaction, utiComponents.Clearer_Actor_Id);

            }
            if (dr["IDB_CLEAR"] != Convert.DBNull)
            {
                utiComponents.Clearer_Book_Id = Convert.ToInt32(dr["IDB_CLEAR"]);
                // FI 20140916 [20353] add
                utiComponents.Clearer_Compartment_Code = dr["COMPARTCODE_CLEAR"].ToString();
            }

            // POSUTI
            if (dr["IDPOSUTI"] != Convert.DBNull)
            {
                // FI 20240627 [WI983]
                utiComponents.InitPUTIComponents(pCS, pDbTransaction, Convert.ToInt32(dr["IDPOSUTI"]), Convert.ToInt32(dr["IDT_OPENING"]));
            }
            
            return utiComponents;
        }

        /// <summary>
        /// Calcul des UTI/PUTI sur des trades qui en sont dépourvus et enregistrement dans les tables SQL.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pUtiType">type de calcul
        /// <para>UTI : calcul de l'UTI puis du PUTI dans la foulée</para>
        /// <para>PTI : calcul du PUTI uniquement </para>
        /// <para>null: calcul de l'UTI ou du PUTI</para>
        /// </param>
        /// <param name="pDataTable">Représente les enregistrements candidats</param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        /// FI 20140602 [20023] Modification de la signature de la méthode pUtiType=> Nullable
        /// Null signifie que Spheres® calcule l'UTI/PUTI ou le PUTI  
        /// FI 20140605 [20049] Désormais l'UTI est calculé si activité maison
        /// FI 20140801 [20648] Refactoring  
        public static Cst.ErrLevelMessage CalcAndRecordUTI(string pCS, IDbTransaction pDbTransaction, Nullable<UTIType> pUtiType, DataTable pDataTable, int pIdAIns)
        {
            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);

            if (pDataTable.Rows.Count > 0)
            {
                string successMessage = string.Empty;
                string errorMessage = string.Empty;
                int lastIdAEntity = 0;
                int idARegulatoryOfficeRelativeToEntity = 0;

                //Dictionnaire des PUTI traité avec succès 
                Dictionary<string, int> dicPUTIOk = new Dictionary<string, int>();

                foreach (DataRow dr in pDataTable.Rows)
                {
                    Pair<bool, string> error = new Pair<bool, string>(false, null);

                    UTIComponents utiComponents = UTITools.InitUTIComponentsFromDataRow(pCS, pDbTransaction, dr, pUtiType);

                    UTIType utiType = UTIType.UTI;
                    if (pUtiType.HasValue)
                    {
                        utiType = pUtiType.Value;
                    }
                    else
                    {
                        if ((utiComponents.IsRecalculUTI_Dealer) || (utiComponents.IsRecalculUTI_Clearer))
                            utiType = UTIType.UTI; //=> Calcul de l'UTI/PUTI  
                        else if ((utiComponents.IsRecalculPUTI_Dealer) || (utiComponents.IsRecalculPUTI_Clearer))
                            utiType = UTIType.PUTI; //=> Calcul du PUTI uniquement
                    }

                    #region ERR: Already exists
                    if (!error.First)
                    {
                        if (
                            ((utiType == UTIType.UTI) && (!utiComponents.IsRecalculUTI_Dealer) && (!utiComponents.IsRecalculUTI_Clearer))
                            ||
                            ((utiType == UTIType.PUTI) && (!utiComponents.IsRecalculPUTI_Dealer) && (!utiComponents.IsRecalculPUTI_Clearer))
                           )
                        {
                            error.First = true;
                            error.Second = "Already exists on {0}";
                        }
                    }
                    #endregion

                    #region ERR: Entity not Regulatory Office
                    if (!error.First)
                    {
                        //PL 20140311 Newness Test idARegulatoryOfficeRelativeToEntity
                        if (lastIdAEntity != utiComponents.Entity_id)
                        {
                            lastIdAEntity = utiComponents.Entity_id;
                            idARegulatoryOfficeRelativeToEntity = utiComponents.Entity_RegulatoryOffice_Id;
                        }
                        if (idARegulatoryOfficeRelativeToEntity <= 0)
                        {
                            // Pas de calcul de l'UTI lorsqu'il n'existe aucun Regulatory Office relatif à l'Entité.
                            error.First = true;
                            error.Second = "Entity not Regulatory Office on {0}";
                        }
                    }
                    #endregion

                    #region UTIs computation
                    string keyPUTI = utiComponents.PositionKey;
                    if (dicPUTIOk.ContainsKey(keyPUTI))
                    {
                        // Lorsque la clé a déjà été traitée avec Succès, Spheres® met en place la valeur OK pour ne pas traiter 2 fois la même clé
                        utiComponents.PosUti_IdPosUti = dicPUTIOk[keyPUTI];
                        utiComponents.StatusPUTI_Dealer = "OK";
                        utiComponents.StatusPUTI_Clearer = "OK";
                    }

                    if (!error.First)
                    {
                        //PL 20160427 [22107] - Refactoring apporté dans le cadre de ce chantier, mais sans rapport direct avec lui.
                        StatusCalcUTI resultUTI = StatusCalcUTI.NotAvailable;
                        StatusCalcUTI resultPUTI = StatusCalcUTI.NotAvailable;

                        if ((utiType == UTIType.UTI))
                        {
                            //--------------------------------------------------------------------
                            resultUTI = CalcAndRecordUTI(pCS, pDbTransaction, UTIType.UTI, utiComponents, pIdAIns);
                            //--------------------------------------------------------------------
                            resultPUTI = CalcAndRecordUTI(pCS, pDbTransaction, UTIType.PUTI, utiComponents, pIdAIns);
                            //--------------------------------------------------------------------
                        }
                        else if (utiType == UTIType.PUTI)
                        {
                            //--------------------------------------------------------------------
                            resultPUTI = CalcAndRecordUTI(pCS, pDbTransaction, UTIType.PUTI, utiComponents, pIdAIns);
                            //--------------------------------------------------------------------
                        }
                        // Si la clé keyPUTI a déjà été traité avec succès, le statut StatusCalcUTI.AlreadyExists est remplacé par ValuedWithSuccess
                        // de manière à ne pas générer une erreur dans le message de sortie
                        if (resultPUTI == StatusCalcUTI.AlreadyExists && dicPUTIOk.ContainsKey(keyPUTI))
                        {
                            resultPUTI = StatusCalcUTI.ValuedWithSuccess;
                        }
                        if (resultPUTI == StatusCalcUTI.ValuedWithSuccess)
                        {
                            if (false == dicPUTIOk.ContainsKey(keyPUTI))
                                dicPUTIOk.Add(keyPUTI, utiComponents.PosUti_IdPosUti);
                        }

                        #region ERR: Miscellaneous
                        if ((utiType == UTIType.UTI) && (resultUTI != StatusCalcUTI.ValuedWithSuccess))
                        {

                            error.First = true;
                            switch (resultUTI)
                            {
                                case StatusCalcUTI.ErrorOccured:
                                    error.Second = "Trade {0} : UTI error occured";
                                    break;
                                case StatusCalcUTI.NotComputable:
                                    error.Second = "Trade {0} : UTI not computable";
                                    break;
                                case StatusCalcUTI.AlreadyExists:
                                    error.Second = "Trade {0} : UTI already exists";
                                    break;
                                default:
                                    throw new NotImplementedException(StrFunc.AppendFormat("Result {0} is not implemented", resultUTI.ToString()));
                            }

                        }
                        if (resultPUTI != StatusCalcUTI.ValuedWithSuccess)
                        {
                            string errorMsg = string.Empty;
                            if (error.First)
                            {
                                //NB: Une erreur a également été rencontré lors du calcul de l'UTI.
                                errorMsg = error.Second + ", ";
                            }
                            else
                            {
                                error.First = true;
                                errorMsg = "Trade {0} : ";
                            }
                            switch (resultPUTI)
                            {
                                case StatusCalcUTI.ErrorOccured:
                                    error.Second = errorMsg + "PUTI error occured";
                                    break;
                                case StatusCalcUTI.NotComputable:
                                    error.Second = errorMsg + "PUTI not computable on {0}";
                                    break;
                                case StatusCalcUTI.AlreadyExists:
                                    error.Second = errorMsg + "PUTI already exists on {0}";
                                    break;
                                default:
                                    throw new NotImplementedException(StrFunc.AppendFormat("Result {0} is not implemented.", resultPUTI.ToString()));
                            }
                        }
                        #endregion
                    }
                    #endregion

                    if (error.First)
                        StrFunc.BuildStringListElement(ref errorMessage, StrFunc.AppendFormat(error.Second, utiComponents.Trade_Identifier), -1);
                    else
                        StrFunc.BuildStringListElement(ref successMessage, utiComponents.Trade_Identifier, 4);

                    if (error.First)
                        ret.ErrLevel = Cst.ErrLevel.FAILURE;
                }

                if (StrFunc.IsFilled(errorMessage))
                {
                    ret.Message = Ressource.GetString("ERROR") + ": " + errorMessage + Cst.CrLf;
                }
                if (StrFunc.IsFilled(successMessage))
                {
                    ret.Message += Ressource.GetString("SUCCESS") + ": " + successMessage + Cst.CrLf;
                }
            }
            else
            {
                ret.ErrLevel = Cst.ErrLevel.FAILURE;
                ret.Message = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'UTI ou le PUTI côté Delaer d'un Trade ALLOC
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="utiComponents"></param>
        /// <param name="pUtiType"></param>
        /// <param name="opUTI"></param>
        /// <param name="opRule"></param>
        /// FI 20140919 [XXXXX] Add Method 
        /// PL 20151029 add L2
        /// EG 20180307 [23769] Gestion dbTransaction
        /// EG 20180307 [23769] Gestion dbTransaction
        public static void CalcUTIDealer(string pCS, IDbTransaction pDbTransaction, UTIComponents pUtiComponents, UTIType pUtiType, out string opUTI, out UTIRule opRule)
        {
            opUTI = string.Empty;
            opRule = default;

            if (RegulatoryTools.GetAttributesRegulatory(pCS, pDbTransaction, pUtiComponents.Entity_RegulatoryOffice_Id, pUtiType,
                out UTIRule? rule, out Pair<UTIIssuerIdent, string> issuer))
            {
                if (rule.HasValue && UTITools.IsCPPRULE(rule.Value) && pUtiComponents.Css_Id > 0) // FI 20240425 [26593] UTI/PUTI REFIT call RegulatoryTools.IsCPPRULE
                {
                    UTIRule ruleValue = rule.Value;
                    UTIOverrideCCP(CSTools.SetCacheOn(pCS), pDbTransaction, pUtiComponents.Css_Id, pUtiType, ref ruleValue, ref issuer);
                    rule = ruleValue;
                }
            }

            if (false == rule.HasValue)
                rule = UTIRule.SPHERES_REFIT;

            if (null == issuer)
                issuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.IDA, pUtiComponents.Entity_id.ToString());

            if (rule != UTIRule.NONE)
            {
                switch (pUtiType)
                {
                    case UTIType.UTI:
                        opUTI = UTIBuilder.BuildTradeUTI(pCS, pDbTransaction, pUtiComponents, rule.Value, issuer, TypeSideAllocation.Dealer);
                        break;
                    case UTIType.PUTI:
                        opUTI = UTIBuilder.BuildPositionUTI(pCS, pDbTransaction, pUtiComponents, rule.Value, issuer, TypeSideAllocation.Dealer);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("UTIType: {0} is not implemented", pUtiType.ToString()));
                }
            }

            opRule = rule.Value;
        }

        /// <summary>
        /// Retourne l'UTI ou le PUTI côté Clearer d'un Trade ALLOC
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="utiComponents"></param>
        /// <param name="pUtiType"></param>
        /// <param name="opUTI"></param>
        /// <param name="opRule"></param>
        // FI 20140919 [XXXXX] Add Method 
        // PL 20151029 add L2
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180307 [23769] Gestion dbTransaction
        public static void CalcUTIClearer(string pCS, IDbTransaction pDbTransaction, UTIComponents utiComponents, UTIType pUtiType, out string opUTI, out UTIRule opRule)
        {

            opUTI = string.Empty;
            opRule = default;

            if (utiComponents.Clearer_Actor_Id > 0)
            {
                if (utiComponents.Clearer_Actor_IsCCP)
                {
                    #region ClearingOrganization
                    Nullable<UTIRule> rule = null;
                    Pair<UTIIssuerIdent, string> issuer = null;
                    if (utiComponents.Clearer_RegulatoryOffice_Id > 0)
                        RegulatoryTools.GetAttributesRegulatory(pCS, pDbTransaction, utiComponents.Clearer_RegulatoryOffice_Id, pUtiType, out rule, out issuer);

                    if ((false == rule.HasValue) || UTITools.IsCPPRULE(rule.Value)) // FI 20240425 [26593] UTI/PUTI REFIT call RegulatoryTools.IsCPPRULE
                        RegulatoryTools.GetDefaultCCPUTIRule(pCS, pDbTransaction, rule, utiComponents.Css_Id, out rule, out issuer);

                    if (false == rule.HasValue)
                        rule = UTIRule.SPHERES_REFIT;
                    if (null == issuer)
                        issuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.IDA, utiComponents.Clearer_Actor_Id.ToString());

                    if (rule != UTIRule.NONE)
                    {
                        switch (pUtiType)
                        {
                            case UTIType.UTI:
                                opUTI = UTIBuilder.BuildTradeUTI(pCS, pDbTransaction, utiComponents, rule.Value, issuer, TypeSideAllocation.Clearer);
                                break;
                            case UTIType.PUTI:
                                opUTI = UTIBuilder.BuildPositionUTI(pCS, pDbTransaction, utiComponents, rule.Value, issuer, TypeSideAllocation.Clearer);
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("UTIType: {0} is not implemented", pUtiType.ToString()));
                        }
                    }

                    opRule = rule.Value;
                    #endregion
                }
                else //if ((clearer.PartyRole == PartyRoleEnum.ClearingFirm) || (clearer.PartyRole == PartyRoleEnum.Custodian))
                {
                    #region ClearingFirm | Custodian
                    Nullable<UTIRule> rule = null;
                    Pair<UTIIssuerIdent, string> issuer = null;

                    if (utiComponents.Clearer_RegulatoryOffice_Id > 0)
                    {
                        if (RegulatoryTools.GetAttributesRegulatory(pCS, pDbTransaction, utiComponents.Clearer_RegulatoryOffice_Id, pUtiType, out rule, out issuer))
                        {
                            if (rule.HasValue && UTITools.IsCPPRULE(rule.Value) && utiComponents.Css_Id > 0)  // FI 20240425 [26593] UTI/PUTI REFIT call RegulatoryTools.IsCPPRULE
                            {
                                UTIRule ruleValue = rule.Value;
                                UTIOverrideCCP(pCS, pDbTransaction, utiComponents.Css_Id, pUtiType, ref ruleValue, ref issuer);
                                rule = ruleValue;
                            }
                        }
                    }

                    if (false == rule.HasValue)
                        rule = UTIRule.SPHERES_REFIT;
                    if (null == issuer)
                        issuer = new Pair<UTIIssuerIdent, string>(UTIIssuerIdent.IDA, utiComponents.Entity_id.ToString());

                    if (rule != UTIRule.NONE)
                    {

                        switch (pUtiType)
                        {
                            case UTIType.UTI:
                                opUTI = UTIBuilder.BuildTradeUTI(pCS, pDbTransaction, utiComponents, rule.Value, issuer, TypeSideAllocation.Clearer);
                                break;
                            case UTIType.PUTI:
                                opUTI = UTIBuilder.BuildPositionUTI(pCS, pDbTransaction, utiComponents, rule.Value, issuer, TypeSideAllocation.Clearer);
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("UTIType: {0} is not implemented", pUtiType.ToString()));
                        }
                    }
                    opRule = rule.Value;
                    #endregion ClearingFirm | Custodian
                }
                //else
                //{
                //    throw new NotImplementedException(StrFunc.AppendFormat("Clearer PartyRole: {0} is not implemented", clearer.PartyRole));
                //}
            }
        }

        /// <summary>
        ///  Ecrase le paramétrage CCP 
        /// </summary>
        /// <param name="pIdACss">Représente la chambre</param>
        /// <param name="pUTIRule"></param>
        /// <param name="pIssuer"></param>
        // PL 20151029 add L2
        // EG 20180307 [23769] Gestion dbTransaction
        private static void UTIOverrideCCP(string pCS, IDbTransaction pDbTransaction, int pIdACss, UTIType pUTIType, ref UTIRule pUTIRule, ref Pair<UTIIssuerIdent, string> pIssuer)
        {
            if (false == UTITools.IsCPPRULE(pUTIRule))  // FI 20240425 [26593] UTI/PUTI REFIT call RegulatoryTools.IsCPPRULE
                throw new ArgumentException(StrFunc.AppendFormat("UTI Rule: {0} is not valid", pUTIRule));

            // Lecture du paramétrage existant pour la chambre
            if (RegulatoryTools.GetAttributesRegulatory(pCS, pDbTransaction, pIdACss, pUTIType, out UTIRule? CssRule, out Pair<UTIIssuerIdent, string> CssIssuer))
            {
                if (CssRule.HasValue)
                    pUTIRule = CssRule.Value;
                if ((null == pIssuer) && (null != CssIssuer))
                    pIssuer = CssIssuer;
            }

            // valeur par défaut à défaut de paramétrage sur la chambre
            if (UTITools.IsCPPRULE(pUTIRule))  // FI 20240425 [26593] UTI/PUTI REFIT call RegulatoryTools.IsCPPRULE
            {
                if (RegulatoryTools.GetDefaultCCPUTIRule(pCS, pDbTransaction, pUTIRule, pIdACss, out CssRule, out CssIssuer))
                {
                    pUTIRule = CssRule.Value;
                    if ((null == pIssuer) && (null != CssIssuer))
                        pIssuer = CssIssuer;
                }
            }
        }

        /// <summary>
        /// Calcul des UTIs sur un trade ALLOC et sauvegarde dans la table
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUtiType"></param>
        /// <param name="pUtiComponents"></param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        /// FI 20140613 [19923] gestion du cas ou utiRule == UTIRule.NONE => pas de génération de UTI
        /// EG 20180221 Upd pour gestion Asynchrone (Calcul des UTI traitement EOD)
        /// EG 20180307 [23769] Gestion dbTransaction
        public static StatusCalcUTI CalcAndRecordUTI(string pCS, IDbTransaction pDbTransaction, UTIType pUtiType, UTIComponents pUtiComponents, int pIdAIns)
        {
            StatusCalcUTI ret = StatusCalcUTI.NotAvailable;

            try
            {
                if (
                    ((pUtiType == UTIType.UTI) && (pUtiComponents.IsRecalculUTI_Dealer || pUtiComponents.IsRecalculUTI_Clearer))
                    ||
                    ((pUtiType == UTIType.PUTI) && (pUtiComponents.IsRecalculPUTI_Dealer || pUtiComponents.IsRecalculPUTI_Clearer))
                   )
                {
                    #region Dealer Side
                    if (
                        ((pUtiType == UTIType.UTI) && pUtiComponents.IsRecalculUTI_Dealer)
                        ||
                        ((pUtiType == UTIType.PUTI) && pUtiComponents.IsRecalculPUTI_Dealer)
                       )
                    {

                        UTITools.CalcUTIDealer(pCS, pDbTransaction, pUtiComponents, pUtiType, out string utiValue, out UTIRule utiRule);
                        if (utiRule != UTIRule.NONE)
                        {
                            if (String.IsNullOrEmpty(utiValue))
                                ret = StatusCalcUTI.NotComputable;
                            else
                                SaveUTI(pCS, pDbTransaction, pUtiType, TypeSideAllocation.Dealer, pUtiComponents, utiValue, utiRule, pIdAIns);
                        }
                    }
                    #endregion
                    #region Clearer Side
                    if (
                        ((pUtiType == UTIType.UTI) && pUtiComponents.IsRecalculUTI_Clearer)
                        ||
                        ((pUtiType == UTIType.PUTI) && pUtiComponents.IsRecalculPUTI_Clearer)
                       )
                    {

                        UTITools.CalcUTIClearer(pCS, pDbTransaction, pUtiComponents, pUtiType, out string utiValue, out UTIRule utiRule);
                        if (utiRule != UTIRule.NONE)
                        {
                            if (String.IsNullOrEmpty(utiValue))
                                ret = StatusCalcUTI.NotComputable;
                            else
                                SaveUTI(pCS, pDbTransaction, pUtiType, TypeSideAllocation.Clearer, pUtiComponents, utiValue, utiRule, pIdAIns);
                        }
                    }
                    #endregion
                }
                else
                {
                    ret = StatusCalcUTI.AlreadyExists;
                }
            }
            // EG 20160404 Migration vs2013
            catch (Exception)
            {
                ret = StatusCalcUTI.ErrorOccured;
            }
            finally
            {
                if (ret == StatusCalcUTI.NotAvailable)
                {
                    if (
                        ((pUtiType == UTIType.UTI) && (pUtiComponents.IsRecalculUTI_Dealer || pUtiComponents.IsRecalculUTI_Clearer))
                        ||
                        ((pUtiType == UTIType.PUTI) && (pUtiComponents.IsRecalculPUTI_Dealer || pUtiComponents.IsRecalculPUTI_Clearer))
                       )
                    {
                        ret = StatusCalcUTI.ValuedWithSuccess;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Sauvegarde des UTIs dans la table physique
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pUtiType"></param>
        /// <param name="pSide"></param>
        /// <param name="pUtiComponents"></param>
        /// <param name="pUtiValue">Value de l'UTI</param>
        /// <param name="pUtiRule">Règle utilisée pour l'UTI</param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Ajout paramètre UTIRule pour alimentation de la colonne SOURCE dans TRADEID). 
        private static int SaveUTI(string pCS, IDbTransaction pDbTransaction, UTIType pUtiType, TypeSideAllocation pSide, UTIComponents pUtiComponents, string pUtiValue, UTIRule pUtiRule, int pIdAIns)
        {
            int ret = 0;
            switch (pUtiType)
            {
                case UTIType.UTI:
                    ret = SaveTradeUTI(pCS, pDbTransaction, pSide, pUtiComponents, pUtiValue, pUtiRule);
                    break;
                case UTIType.PUTI:
                    ret = SavePositionUTI(pCS, pDbTransaction, pSide, pUtiComponents, pUtiValue, pUtiRule, pIdAIns);
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Sauvegarde des UTIs dans la table TRADEID
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSide"></param>
        /// <param name="pUtiComponents"></param>
        /// <param name="pUtiRule"></param>
        ///<param name="pIdAIns"></param>
        /// <returns></returns>
        ///FI 20140922 [XXXXX] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Ajout paramètre UTIRule pour alimentation de la colonne SOURCE dans TRADEID). 
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Ajout colonne SOURCE). 
        private static int SaveTradeUTI(string pCS, IDbTransaction pDbTransaction, TypeSideAllocation pSide, UTIComponents pUtiComponents, string pUtiValue, UTIRule pUtiRule)
        {
            #region SQL Commands (Insert/Update)
            string sqlInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADEID.ToString() + " (IDT,IDA,TRADEID,TRADEIDSCHEME,SOURCE)";
            sqlInsert += SQLCst.VALUES + "(@IDT,@IDA,@TRADEID,@TRADEIDSCHEME,@SOURCE)";

            string sqlUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRADEID.ToString() + " set TRADEID=@TRADEID, SOURCE=@SOURCE" + Cst.CrLf;
            sqlUpdate += SQLCst.WHERE + "IDT=@IDT and IDA=@IDA and TRADEIDSCHEME=@TRADEIDSCHEME";
            #endregion

            int idA = 0;
            bool isInsert = false;
            bool isUpdate = false;
            switch (pSide)
            {
                case TypeSideAllocation.Dealer:
                    isInsert = (pUtiComponents.StatusUTI_Dealer == "MISSING");
                    isUpdate = (pUtiComponents.StatusUTI_Dealer == "INVALID");
                    idA = pUtiComponents.Dealer_Actor_id;
                    break;
                case TypeSideAllocation.Clearer:
                    isInsert = (pUtiComponents.StatusUTI_Clearer == "MISSING");
                    isUpdate = (pUtiComponents.StatusUTI_Clearer == "INVALID");
                    idA = pUtiComponents.Clearer_Actor_Id;
                    break;
            }

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pUtiComponents.Trade_id);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), idA);
            dp.Add(new DataParameter(pCS, "TRADEID", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pUtiValue);
            dp.Add(new DataParameter(pCS, "TRADEIDSCHEME", DbType.AnsiString, SQLCst.UT_UNC_LEN), Cst.OTCml_TradeIdUTISpheresScheme);
            dp.Add(new DataParameter(pCS, "SOURCE", DbType.AnsiString, SQLCst.UT_UNC_LEN), pUtiRule.ToString());

            string sql = string.Empty;
            if (isInsert)
                sql = sqlInsert;
            else if (isUpdate)
                sql = sqlUpdate;

            int row = 0;
            if (StrFunc.IsFilled(sql))
            {
                QueryParameters qryParameters = new QueryParameters(pCS, sql, dp);
                row = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                CSManager csManager = new CSManager(pCS);
                if (csManager.IsUseCache.HasValue && csManager.IsUseCache.Value)
                    DataHelper.queryCache.Remove("TRADEID", pCS, true);
            }
            return row;
        }

        /// <summary>
        /// Sauvegarde des UTIs dans les tables POSUTI et POSUTIDET
        /// <para>pUtiComponents.idPosUti est valorisé lorsqu'il est non renseigné</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSide"></param>
        /// <param name="pUtiComponents"></param>
        /// <param name="pUtiValue">Value de l'UTI</param>
        /// <param name="pUtiRule">Règle utilisée pour l'UTI</param>
        /// <param name="pIdAIns"></param>
        /// <returns></returns>
        /// FI 20140623 [20125] Modification de l'update=> Alimentation de UTISCHEME et de UTIRULE
        /// FI 20140913 [XXXXX] Modification de la signature 
        /// FI 20140922 [XXXXX] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Ajout colonnes IDAUPD/DTUPD). 
        // EG 20180307 [23769] Gestion dbTransaction
        private static int SavePositionUTI(string pCS, IDbTransaction pDbTransaction, TypeSideAllocation pSide, UTIComponents pUtiComponents, string pUtiValue, UTIRule pUtiRule, int pIdAIns)
        {

            CSManager csManager = new CSManager(pCS);
            bool isUseCache = csManager.IsUseCache.HasValue && csManager.IsUseCache.Value;

            #region SQL Commands
            string sqlInsert_puti = SQLCst.INSERT_INTO_DBO + "POSUTI (IDI,IDASSET,IDA_DEALER,IDB_DEALER,IDA_CLEARER,IDB_CLEARER,IDT_OPENING,DTINS,IDAINS)";
            sqlInsert_puti += SQLCst.VALUES + "(@IDI,@IDASSET,@IDA_DEALER,@IDB_DEALER,@IDA_CLEARER,@IDB_CLEARER,@IDT_OPENING,@DTINS,@IDAINS)";

            string sqlInsert = SQLCst.INSERT_INTO_DBO + "POSUTIDET (IDPOSUTI,IDA,UTI,UTISCHEME,UTIRULE,DTINS,IDAINS)";
            sqlInsert += SQLCst.VALUES + "(@IDPOSUTI,@IDA,@UTI,@UTISCHEME,@UTIRULE,@DTINS,@IDAINS)";

            //PL 20140523 Manage NULL value (Il existe dans les DB des couples UTI='N/A' and UTISCHEME=null résultant d'alimentation par Spheres v3.7)
            //string sqlUpdate = SQLCst.UPDATE_DBO + "POSUTIDET set UTI=@UTI where IDPOSUTI=@IDPOSUTI and IDA=@IDA and UTISCHEME=@UTISCHEME";
            string sqlUpdate = SQLCst.UPDATE_DBO + @"POSUTIDET set UTI=@UTI, UTISCHEME = @UTISCHEME, UTIRULE=@UTIRULE, IDAUPD=@IDAUPD, DTUPD=@DTUPD 
                        where IDPOSUTI=@IDPOSUTI and IDA=@IDA and (UTISCHEME=@UTISCHEME or (UTI='N/A' and UTISCHEME is null))";
            #endregion

            int row = 0;
            int idA = 0;
            bool isInsert = false;
            bool isUpdate = false;
            switch (pSide)
            {
                case TypeSideAllocation.Dealer:
                    isInsert = (pUtiComponents.StatusPUTI_Dealer == "MISSING");
                    isUpdate = (pUtiComponents.StatusPUTI_Dealer == "INVALID");
                    idA = pUtiComponents.Dealer_Actor_id;
                    break;
                case TypeSideAllocation.Clearer:
                    isInsert = (pUtiComponents.StatusPUTI_Clearer == "MISSING");
                    isUpdate = (pUtiComponents.StatusPUTI_Clearer == "INVALID");
                    idA = pUtiComponents.Clearer_Actor_Id;
                    break;
            }

            if (pUtiComponents.PosUti_IdPosUti <= 0)
            {

                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDI), pUtiComponents.DC_IdI);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDASSET), pUtiComponents.Asset_Id);
                dp.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), pUtiComponents.Dealer_Actor_id);
                dp.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), pUtiComponents.Dealer_Book_Id);
                dp.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), pUtiComponents.Clearer_Actor_Id);
                dp.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), pUtiComponents.Clearer_Book_Id);
                dp.Add(new DataParameter(pCS, "IDT_OPENING", DbType.Int32), pUtiComponents.Trade_id);
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pIdAIns);
                //FI 20200820 [XXXXXX] Date systeme en UTC
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(pCS));

                QueryParameters qryParameters = new QueryParameters(pCS, sqlInsert_puti, dp);

                row = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                // if (null == pDbTransaction)
                if (isUseCache)
                    DataHelper.queryCache.Remove("POSUTI", pCS, true);

                pUtiComponents.PosUti_IdPosUti = UTITools.GetIdPOSUTI(pCS, pDbTransaction,
                        new Pair<int, int>(pUtiComponents.Dealer_Actor_id, pUtiComponents.Dealer_Book_Id),
                        new Pair<int, int>(pUtiComponents.Clearer_Actor_Id, pUtiComponents.Clearer_Book_Id),
                        new Pair<int, int>(pUtiComponents.DC_IdI, pUtiComponents.Asset_Id)).idPosUTI;
            }

            if (pUtiComponents.PosUti_IdPosUti > 0)
            {
                if (isInsert)
                {
                    DataParameters dp = new DataParameters();
                    dp.Add(new DataParameter(pCS, "IDPOSUTI", DbType.Int32), pUtiComponents.PosUti_IdPosUti);
                    dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), idA);
                    dp.Add(new DataParameter(pCS, "UTI", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pUtiValue);
                    dp.Add(new DataParameter(pCS, "UTISCHEME", DbType.AnsiString, SQLCst.UT_UNC_LEN), Cst.OTCml_TradeIdUTISpheresScheme);
                    dp.Add(new DataParameter(pCS, "UTIRULE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pUtiRule);
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pIdAIns);
                    //FI 20200820 [XXXXXX] Date systeme en UTC
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), OTCmlHelper.GetDateSysUTC(pCS));

                    QueryParameters qryParameters = new QueryParameters(pCS, sqlInsert, dp);

                    row = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    // if (null == pDbTransaction)
                    if (isUseCache)
                        DataHelper.queryCache.Remove("POSUTIDET", pCS, false);
                }
                else if (isUpdate)
                {
                    DataParameters dp = new DataParameters();
                    dp.Add(new DataParameter(pCS, "IDPOSUTI", DbType.Int32), pUtiComponents.PosUti_IdPosUti);
                    dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), idA);
                    dp.Add(new DataParameter(pCS, "UTI", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pUtiValue);
                    dp.Add(new DataParameter(pCS, "UTISCHEME", DbType.AnsiString, SQLCst.UT_UNC_LEN), Cst.OTCml_TradeIdUTISpheresScheme);
                    dp.Add(new DataParameter(pCS, "UTIRULE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pUtiRule);
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAUPD), pIdAIns);
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTUPD), OTCmlHelper.GetDateSysUTC(pCS));

                    string sql = sqlUpdate;
                    QueryParameters qryParameters = new QueryParameters(pCS, sql, dp);
                    row = DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    // if (null == pDbTransaction)
                    if (isUseCache)
                        DataHelper.queryCache.Remove("POSUTIDET", pCS, false);
                }
            }
            return row;
        }

        /// <summary>
        /// Retourne le code Membre de l'acteur {pIdA} chez la chambre {pCssBIC}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <param name="pCssBIC"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        private static string GetCSSMemberCode(string pCS, IDbTransaction pDbTransaction, int pIdA, string pCssBIC)
        {
            string memberIdent;
            switch (pCssBIC)
            {
                case UTIBuilder.BIC_CCeG:
                    memberIdent = "ITIT";
                    break;
                case UTIBuilder.BIC_EUREX:
                    memberIdent = "ECAG";
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Actor (BIC:{0}) is not implemented", pCssBIC));
            }

            Pair<string, string> actor = new Pair<string, string>(Cst.OTCml_ActorIdScheme, pIdA.ToString());
            Pair<string, string> css = new Pair<string, string>(Cst.OTCml_ActorBicScheme, pCssBIC);

            return MarketTools.GetCSSMemberCode(pCS, pDbTransaction, actor, css, memberIdent);
        }

        /// <summary>
        /// Retourne l'id dans POSUTI (idPosUTI) et le trade à l'origine de la position (idTOpening)  
        /// <para>
        /// Retourne 0 s'il n'existe aucun enregistrement dans POSUTI pour la clé de positions
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDealer">couple IDA,IDB</param>
        /// <param name="pClearer">couple IDA,IDB</param>
        /// <param name="pAsset">couple IDI,IDASSET</param>
        /// <returns></returns>
        /// EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        /// FI 20240627 [WI983] Return now idPosUTI and idTOpening 
        public static (int idPosUTI, int idTOpening) GetIdPOSUTI(string pCS, IDbTransaction pDbTransaction, Pair<int, int> pDealer, Pair<int, int> pClearer, Pair<int, int> pAsset)
        {
            (int idPosUTI, int idTOpening) ret;
            ret.idPosUTI = 0;
            ret.idTOpening = 0;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDI), pAsset.First);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDASSET), pAsset.Second);
            dp.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), pDealer.First);
            dp.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), pDealer.Second);
            dp.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), pClearer.First);
            dp.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), pClearer.Second);

            StrBuilder sql = new StrBuilder();
            sql += SQLCst.SELECT + "IDPOSUTI, IDT_OPENING" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + "POSUTI" + Cst.CrLf;
            sql += SQLCst.WHERE + "IDI=@IDI and IDASSET=@IDASSET and IDA_DEALER=@IDA_DEALER and IDB_DEALER=@IDB_DEALER and IDA_CLEARER=@IDA_CLEARER and IDB_CLEARER=@IDB_CLEARER";

            QueryParameters qryParameters = new QueryParameters(pCS, sql.ToString(), dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    ret.idPosUTI = Convert.ToInt32(dr["IDPOSUTI"]);
                    ret.idTOpening = Convert.ToInt32(dr["IDT_OPENING"]);
                }
            }
            return ret;
        }
    }
}
#region Using Directives
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; 

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.MiFIDII_Extended;
using EfsML.Interface;

using FixML.Interface;
using FixML.v50SP1.Enum;
using FixML.Enum;

using FpML.Interface;

using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract partial class TradeCommonCaptureGen
    {
        /// <summary>
        /// Retourne la query insert dans TRADEACTOR
        /// <para>Tous les paramètres sont initialisés avec la valeur DBNull</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pUseSelect">
        /// <para>si true retourne insert into dbo.TRADEACTOR ({listeDesColonnes}) select {ListeDesParametres} from DUAL</para>
        /// <para>sinon insert into dbo.TRADEACTOR ({listeDesColonnes}) values ({ListeDesParametres})</para>
        /// </param>
        /// <returns></returns>
        /// FI 20200427 [XXXXX] add pUseSelect parameter
        protected static QueryParameters GetQueryParametersInsertTradeActor(string pCs, Boolean pUseSelect)
        {
            string[] columns = new string[] {
            "IDT", "BUYER_SELLER", "IDA", "IDROLEACTOR", "IDA_ACTOR", "IDB", "LOCALCLASSDERV",
            "IASCLASSDERV", "HEDGECLASSDERV", "FXCLASS", "LOCALCLASSNDRV", "IASCLASSNDRV", "HEDGECLASSNDRV", "FACTOR",
            "ISNCMINI", "ISNCMINT", "ISNCMFIN",
            "FIXPARTYROLE","FIXPARTYROLEQUALIF"
            };

            string[] paramColumns = (from string item in columns select "@" + item).ToArray();

            string sqlInsert = string.Empty;
            if (pUseSelect)
                sqlInsert += StrFunc.AppendFormat("insert into dbo.TRADEACTOR ({0}) select {1} from DUAL", ArrFunc.GetStringList(columns), ArrFunc.GetStringList(paramColumns));
            else
                sqlInsert += StrFunc.AppendFormat("insert into dbo.TRADEACTOR ({0}) values ({1})", ArrFunc.GetStringList(columns), ArrFunc.GetStringList(paramColumns));

            DataParameters dataParams = new DataParameters();
            dataParams.Add(new DataParameter(pCs, "IDT", DbType.Int32), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "BUYER_SELLER", DbType.AnsiString, 64), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "IDA", DbType.Int32), Convert.DBNull);
            dataParams.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDROLEACTOR), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "IDA_ACTOR", DbType.Int32), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "IDB", DbType.Int32), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "LOCALCLASSDERV", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "HEDGECLASSDERV", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "IASCLASSDERV", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "FXCLASS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "LOCALCLASSNDRV", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "IASCLASSNDRV", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "HEDGECLASSNDRV", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "FACTOR", DbType.Decimal), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "ISNCMINI", DbType.Boolean), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "ISNCMINT", DbType.Boolean), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "ISNCMFIN", DbType.Boolean), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "FIXPARTYROLE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
            dataParams.Add(new DataParameter(pCs, "FIXPARTYROLEQUALIF", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);

            return new QueryParameters(pCs, sqlInsert, dataParams);
        }
        /// <summary>
        /// Ajoute un item dans la table LINKID 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdA"></param>
        /// <param name="pLinkId"></param>
        protected static void InsertLinkId(string pCs, IDbTransaction pDbTransaction, int pIdT, int pIdA, ILinkId pLinkId)
        {
            #region Query Insert
            string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LINKID + Cst.CrLf;
            SQLInsert += "(IDT, IDA, LINKID, LINKIDSCHEME, FACTOR) values (@IDT, @IDA, @LINKID, @LINKIDSCHEME, @FACTOR);";
            #endregion Query Insert
            //
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(pCs, "IDT", DbType.Int32), pIdT);
            dataParameters.Add(new DataParameter(pCs, "IDA", DbType.Int32), pIdA);
            dataParameters.Add(new DataParameter(pCs, "LINKID", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pLinkId.Value);
            dataParameters.Add(new DataParameter(pCs, "LINKIDSCHEME", DbType.AnsiString, SQLCst.UT_UNC_LEN), pLinkId.LinkIdScheme);
            dataParameters.Add(new DataParameter(pCs, "FACTOR", DbType.Decimal), StrFunc.IsFilled(pLinkId.StrFactor) && DecFunc.IsDecimal(pLinkId.StrFactor) ? pLinkId.Factor : Convert.DBNull);
            //
            QueryParameters qryParameters = new QueryParameters(pCs, SQLInsert, dataParameters);
            //
            DataHelper.ExecuteNonQuery(pCs ,pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

        }

        /// <summary>
        /// Alimente TradeTrail
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pIdScreen"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pDtSys">Date Début procédure d'enregistrement du trade</param>
        /// FI 20170323 [XXXXX] Modify
        protected static void SaveTradeTrail(string pCS, IDbTransaction pDbTransaction, int pIdT, CaptureSessionInfo pSessionInfo,
            string pIdScreen, Cst.Capture.ModeEnum pCaptureMode,  DateTime pDtSys)
        {

            Nullable<int> idTRK_L = pSessionInfo.idTracker_L;
            Nullable<int> idProcess = pSessionInfo.idProcess_L;

            // FI 20170323 [XXXXX] Appel TradeRDBMSTools.SaveTradeTrail
            //côté web appInstance.IdA == user.idA mais côté importation des trades ce n'est pas la même chose
            //pSessionInfo.user.idA représente l'acteur spécifié dans le mapping, ou le requester
            TradeRDBMSTools.SaveTradeTrail(pCS, pDbTransaction,
                pIdT, pIdScreen, pCaptureMode, pDtSys, pSessionInfo.user.IdA, pSessionInfo.session, idTRK_L, idProcess);

        }

        /// <summary>
        /// Injecte un acteur contrepatie dans TRADEACTOR
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDoc"></param>
        /// <param name="pIdT"></param>
        /// <param name="pParty"></param>
        /// EG 20140702 New Introduction de l'objet ReturnSwapContainer (pour gestion des de FIXPARTYROLE sur ReturnSwap)
        /// EG 20150331 [POC] FxLeg|FxOptionLeg
        /// EG 20150706 [21021] Nullable &lt;int&gt; for IdB
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        /// FI 20170928 [23452] Modify
        protected void InsertTradeActorCounterparty(string pCS, IDbTransaction pDbTransaction, int pIdT, IParty pParty)
        {
            DataDocumentContainer doc = TradeCommonInput.DataDocument;

            // FI 20161214 [21916] call method currentProduct.RptSide(CS, TradeCommonInput.IsAllocation);
            // FI 20170116 [21916] call method currentProduct.RptSide()
            RptSideProductContainer _rptSideContainer = doc.CurrentProduct.RptSide();
            IPartyTradeInformation partyTradeInformation = doc.GetPartyTradeInformation(pParty.Id);
            /* // FI 20161214 [21916] Mise en commentaire
            ProductContainer productMain = TradeCommonInput.DataDocument.currentProduct;
            RptSideProductContainer _rptSideContainer = null;
            if (productMain.isStrategy)
                productMain = TradeCommonInput.DataDocument.currentProduct.MainProduct;
            if (productMain.isExchangeTradedDerivative)
                //exchangeTraded = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productMain.product, TradeCommonInput.DataDocument);
                _rptSideContainer = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productMain.product, TradeCommonInput.DataDocument);
            else if (productMain.IsEquitySecurityTransaction)
                //exchangeTraded = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)productMain.product);
                _rptSideContainer = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)productMain.product);
            else if (productMain.isReturnSwap)
                //_returnSwapContainer = new ReturnSwapContainer((IReturnSwap)productMain.product, TradeCommonInput.DataDocument);
                _rptSideContainer = new ReturnSwapContainer((IReturnSwap)productMain.product, TradeCommonInput.DataDocument);
            else if (productMain.isDebtSecurityTransaction)
                //_returnSwapContainer = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)productMain.product, TradeCommonInput.DataDocument);
                _rptSideContainer = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)productMain.product, TradeCommonInput.DataDocument);
            // EG 20150331 [POC] FxLeg
            else if (productMain.isFxLeg)
                _rptSideContainer = new FxLegContainer((IFxLeg)productMain.product, TradeCommonInput.DataDocument);
            // EG 20150331 [POC] FxOptionLeg
            else if (productMain.isFxSimpleOption)
                _rptSideContainer = new FxOptionLegContainer((IFxOptionLeg)productMain.product, TradeCommonInput.DataDocument);
            else if (productMain.IsCommoditySpot)
                _rptSideContainer = new CommoditySpotContainer((ICommoditySpot)productMain.product, TradeCommonInput.DataDocument);
            */

            #region Party
            QueryParameters qryParam = GetQueryParametersInsertTradeActor(pCS, false);
            qryParam.Parameters["IDA"].Value = pParty.OTCmlId;
            qryParam.Parameters["IDT"].Value = pIdT;
            qryParam.Parameters["IDROLEACTOR"].Value = RoleActor.COUNTERPARTY.ToString();
            qryParam.Parameters["IDA_ACTOR"].Value = Convert.DBNull;

            // EG 20150706 [21021]
            Nullable<int> idB = doc.GetOTCmlId_Book(pParty.Id);
            // FI 20150730 [XXXXX] Test supplémentaire sur idB.Value > 0 car pour l'importation d'un trade incomplet sans book la méthode GetOTCmlId_Book retourne 0
            qryParam.Parameters["IDB"].Value = (idB.HasValue && idB.Value > 0 ? idB.Value : Convert.DBNull);

            string tmpClass;
            tmpClass = doc.GetOTCmlId_LocalClassDerv(pParty.Id);
            qryParam.Parameters["LOCALCLASSDERV"].Value = (tmpClass ?? Convert.DBNull);

            tmpClass = doc.GetOTCmlId_IASClassDerv(pParty.Id);
            qryParam.Parameters["IASCLASSDERV"].Value = tmpClass ?? Convert.DBNull;
            tmpClass = doc.GetOTCmlId_HedgeClassDerv(pParty.Id);
            qryParam.Parameters["HEDGECLASSDERV"].Value = (tmpClass ?? Convert.DBNull);

            tmpClass = doc.GetOTCmlId_FxClass(pParty.Id);
            qryParam.Parameters["FXCLASS"].Value = (tmpClass ?? Convert.DBNull);

            tmpClass = doc.GetOTCmlId_LocalClassNDrv(pParty.Id);
            qryParam.Parameters["LOCALCLASSNDRV"].Value = (tmpClass ?? Convert.DBNull);

            tmpClass = doc.GetOTCmlId_IASClassNDrv(pParty.Id);
            qryParam.Parameters["IASCLASSNDRV"].Value = (tmpClass ?? Convert.DBNull);
            tmpClass = doc.GetOTCmlId_HedgeClassNDrv(pParty.Id);
            qryParam.Parameters["HEDGECLASSNDRV"].Value = (tmpClass ?? Convert.DBNull);

            qryParam.Parameters["BUYER_SELLER"].Value = Convert.DBNull;
            qryParam.Parameters["FIXPARTYROLE"].Value = Convert.DBNull;

            if (doc.IsPartyBuyer(pParty))
            {
                qryParam.Parameters["BUYER_SELLER"].Value = SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.BUYER);
                if (null != _rptSideContainer)
                    qryParam.Parameters["FIXPARTYROLE"].Value = _rptSideContainer.GetBuyerSellerPartyRole(FixML.Enum.SideEnum.Buy);
                //if (null != exchangeTraded)
                //    qryParam.Parameters["FIXPARTYROLE"].Value = exchangeTraded.GetBuyerSellerPartyRole(FixML.Enum.SideEnum.Buy);
                //else if (null != _returnSwapContainer)
                //    qryParam.Parameters["FIXPARTYROLE"].Value = _returnSwapContainer.GetBuyerSellerPartyRole(FixML.Enum.SideEnum.Buy);
            }
            else if (doc.IsPartySeller(pParty))
            {
                qryParam.Parameters["BUYER_SELLER"].Value = SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.SELLER);
                if (null != _rptSideContainer)
                    qryParam.Parameters["FIXPARTYROLE"].Value = _rptSideContainer.GetBuyerSellerPartyRole(SideEnum.Sell);
                //if (null != exchangeTraded)
                //    qryParam.Parameters["FIXPARTYROLE"].Value = exchangeTraded.GetBuyerSellerPartyRole(SideEnum.Sell);
                //else if (null != _returnSwapContainer)
                //    qryParam.Parameters["FIXPARTYROLE"].Value = _returnSwapContainer.GetBuyerSellerPartyRole(FixML.Enum.SideEnum.Sell);

            }

            qryParam.Parameters["FACTOR"].Value = Convert.DBNull;

            ActorNotification actorNotification = TradeCommonInput.TradeNotification.GetActorNotification(pParty.OTCmlId);
            if (null != actorNotification)
            {
                qryParam.Parameters["ISNCMINI"].Value = actorNotification.GetConfirm(NotificationStepLifeEnum.INITIAL);
                qryParam.Parameters["ISNCMINT"].Value = actorNotification.GetConfirm(NotificationStepLifeEnum.INTERMEDIARY);
                qryParam.Parameters["ISNCMFIN"].Value = actorNotification.GetConfirm(NotificationStepLifeEnum.FINAL);
            }
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParam.Query, qryParam.Parameters.GetArrayDbParameter());
            #endregion Party

            #region Trader
            ITrader[] trader = doc.GetPartyTrader(pParty.Id);
            if (ArrFunc.IsFilled(trader))
            {
                for (int i = 0; i < trader.Length; i++)
                    InsertTradeActorTraderOrSales(pCS, pDbTransaction, pIdT, pParty, trader[i], RoleActor.TRADER);
            }
            #endregion

            #region Sales
            ITrader[] sales = doc.GetPartySales(pParty.Id);
            if (ArrFunc.IsFilled(sales))
            {
                for (int i = 0; i < sales.Length; i++)
                    InsertTradeActorTraderOrSales(pCS, pDbTransaction, pIdT, pParty, sales[i], RoleActor.SALES);
            }
            #endregion

            if (null != partyTradeInformation)
            {
                #region RelatedParty / DECISIONOFFICE // FI 20170928 [23452] add
                if (partyTradeInformation.RelatedPartySpecified)
                {
                    IRelatedParty decisionMaker = (from item in partyTradeInformation.RelatedParty.Where(x => x.Role.Value == PartyRole.SellerDecisionMaker.ToString() ||
                                                                                                              x.Role.Value == PartyRole.BuyerDecisionMaker.ToString())
                                                   select item).FirstOrDefault();




                    if (null != decisionMaker)
                    {
                        IParty partyDecisionMaker = TradeCommonInput.DataDocument.GetParty(decisionMaker.PartyReference.HRef);
                        if (null == partyDecisionMaker)
                            throw new NullReferenceException(StrFunc.AppendFormat("party element:{0} not found", decisionMaker.PartyReference.HRef));

                        QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, false);
                        qryIns.Parameters["IDA"].Value = partyDecisionMaker.OTCmlId;
                        qryIns.Parameters["IDT"].Value = pIdT;
                        qryIns.Parameters["IDROLEACTOR"].Value = RoleActor.DECISIONOFFICE.ToString();
                        qryIns.Parameters["IDA_ACTOR"].Value = pParty.OTCmlId;

                        DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter()) ;
                    }
                }
                #endregion

                #region RelatedPerson // FI 20170928 [23452] add
                if (partyTradeInformation.RelatedPersonSpecified)
                {
                    IEnumerable<IRelatedPerson> relatedPerson = (from item in partyTradeInformation.RelatedPerson.Where(x => x.Role.Value == PersonRoleEnum.ExecutionWithinFirm.ToString() ||
                                                                                                                             x.Role.Value == PersonRoleEnum.InvestmentDecisionMaker.ToString())
                                                                 select item);
                    foreach (IRelatedPerson item in relatedPerson)
                    {
                        PersonRoleEnum personEnum = (PersonRoleEnum)Enum.Parse(typeof(PersonRoleEnum), item.Role.Value);

                        IPerson person = TradeCommonInput.DataDocument.GetPerson(item.PersonReference.HRef);
                        if (null == person)
                            throw new NullReferenceException(StrFunc.AppendFormat("person element:{0} not found", item.PersonReference.HRef));

                        QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, false);
                        qryIns.Parameters["IDA"].Value = person.OTCmlId;
                        qryIns.Parameters["IDT"].Value = pIdT;
                        qryIns.Parameters["IDROLEACTOR"].Value = RoleActorTools.ConvertToRoleActor<PersonRoleEnum>(personEnum).ToString();
                        qryIns.Parameters["IDA_ACTOR"].Value = pParty.OTCmlId;
                        qryIns.Parameters["FIXPARTYROLEQUALIF"].Value = ReflectionTools.ConvertEnumToString<FixML.v50SP2.Enum.PartyDetailRoleQualifier>(FixML.v50SP2.Enum.PartyDetailRoleQualifier.NaturalPerson);
                        DataHelper.ExecuteNonQuery(pCS ,pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter());
                    }
                }
                #endregion

                #region Algorithm // FI 20170928 [23452]
                if (partyTradeInformation.AlgorithmSpecified)
                {
                    IEnumerable<IAlgorithm> algo = (from item in partyTradeInformation.Algorithm.Where(x => x.Role.Value == AlgorithmRoleEnum.Execution.ToString() ||
                                                                                                                             x.Role.Value == AlgorithmRoleEnum.InvestmentDecision.ToString())
                                                    select item);
                    foreach (IAlgorithm item in algo)
                    {

                        AlgorithmRoleEnum algoEnum = (AlgorithmRoleEnum)Enum.Parse(typeof(AlgorithmRoleEnum), item.Role.Value);

                        QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, false);
                        qryIns.Parameters["IDA"].Value = item.OTCmlId;
                        qryIns.Parameters["IDT"].Value = pIdT;
                        qryIns.Parameters["IDROLEACTOR"].Value = RoleActorTools.ConvertToRoleActor<AlgorithmRoleEnum>(algoEnum).ToString();
                        qryIns.Parameters["IDA_ACTOR"].Value = pParty.OTCmlId;
                        qryIns.Parameters["FIXPARTYROLEQUALIF"].Value = ReflectionTools.ConvertEnumToString<FixML.v50SP2.Enum.PartyDetailRoleQualifier>(FixML.v50SP2.Enum.PartyDetailRoleQualifier.Algorithm);

                        DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter());
                    }
                }
                #endregion
            }

            #region TradeSide
            //FI 20101023 [17155]
            if (doc.CurrentTrade.TradeSideSpecified)
                InsertTradeActorTradeSide(pCS, pDbTransaction, pIdT, pParty);
            #endregion
        }

        /// <summary>
        /// Injecte dans TRADEACTOR le Broker {pPartyBroker}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPartyBroker"></param>
        /// <param name="pCounterparty">si null alors le broker n'est rattaché à aucune contrepartie, le colonne IDA_ACTOR sera alimentée avec DBNULL </param>
        /// EG 20140702 New Introduction de l'objet ReturnSwapContainer (pour gestion des de FIXPARTYROLE sur ReturnSwap)
        /// EG 20140731 Refactoring (RptSideContainer)
        /// EG 20150706 [21021] Nullable<int> idB
        /// FI 20161005 [XXXXX] Modify
        /// FI 20170116 [21916] Modify
        // EG 20180606 [23980] Set missing dbTransaction (parallelism)
        protected void InsertTradeActorBroker(string pCS, IDbTransaction pDbTransaction, int pIdT, IParty pPartyBroker, IParty pCounterparty)
        {

            DataDocumentContainer dataDocument = TradeCommonInput.DataDocument;

            QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, false);
            qryIns.Parameters["IDA"].Value = pPartyBroker.OTCmlId;
            qryIns.Parameters["IDT"].Value = pIdT;
            qryIns.Parameters["IDROLEACTOR"].Value = RoleActor.BROKER.ToString();

            Nullable<int> idB = dataDocument.GetOTCmlId_Book(pPartyBroker.Id);
            qryIns.Parameters["IDB"].Value = (idB ?? Convert.DBNull);

            if (null == pCounterparty)
                qryIns.Parameters["IDA_ACTOR"].Value = Convert.DBNull;
            else
                qryIns.Parameters["IDA_ACTOR"].Value = pCounterparty.OTCmlId;

            if (TradeCommonInput.IsTradeFoundAndAllocation && (null != pCounterparty))
            {
                // FI 20170116 [21916]
                //RptSideProductContainer _rptSideContainer = TradeCommonInput.Product.RptSide(CS, TradeCommonInput.IsAllocation);
                RptSideProductContainer _rptSideContainer = TradeCommonInput.Product.RptSide();
                // FI 20161005 [XXXXX] Add  NotImplementedException
                if (null == _rptSideContainer)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", TradeCommonInput.Product.ProductBase.ToString()));

                //IFixParty _cssCustodianParty = null;
                //if (_rptSideContainer.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER))
                //    _cssCustodianParty = _rptSideContainer.GetBuyerSeller(SideEnum.Sell);
                //else
                //    _cssCustodianParty = _rptSideContainer.GetBuyerSeller(SideEnum.Buy);
                // FI 20180424 [23871] Appel à GetClearerCustodian
                IFixParty _cssCustodianParty = _rptSideContainer.GetClearerCustodian();

                if ((null != _cssCustodianParty) && (null != _cssCustodianParty.PartyId))
                {
                    if (pCounterparty.Id == _cssCustodianParty.PartyId.href)
                    {
                        PartyRoleEnum _brokerRole = PartyRoleEnum.Custodian;
                        if (TradeCommonInput.Product.IsExchangeTradedDerivative)
                        {
                            _ = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)TradeCommonInput.Product.Product, dataDocument);
                            _brokerRole = PartyRoleEnum.ExecutingFirm;
                            if (TradeCommonInput.IsGiveUp(pCS, pDbTransaction) &&
                                ActorTools.IsActorWithRole(CSTools.SetCacheOn(pCS), pDbTransaction, pPartyBroker.OTCmlId, RoleActor.CLEARER))
                                _brokerRole = PartyRoleEnum.GiveupClearingFirm;
                        }

                        qryIns.Parameters["FIXPARTYROLE"].Value = ReflectionTools.ConvertEnumToString<PartyRoleEnum>(_brokerRole);
                    }
                }

            }
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Injecte dans TRADEACTOR la ClearingOrganization
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pPartyBroker"></param>
        /// <param name="pCounterparty"></param>
        protected void InsertTradeActorClearingOrganization(string pCS, IDbTransaction pDbTransaction, int pIdT, IParty pPartyClearingOrganisation)
        {
            QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, false);
            qryIns.Parameters["IDA"].Value = pPartyClearingOrganisation.OTCmlId;
            qryIns.Parameters["IDT"].Value = pIdT;
            qryIns.Parameters["IDROLEACTOR"].Value = RoleActor.CSS.ToString();
            qryIns.Parameters["FIXPARTYROLE"].Value = ReflectionTools.ConvertEnumToString<PartyRoleEnum>(PartyRoleEnum.ClearingOrganization);

            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Injecte dans TRADEACTOR le rôle {pRole} pour la party {pParty} vis à vis de la party {pPartyRelativeTo}
        /// <para>Insertion minimum, c'est à dire un couple actor/role </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pParty">partie</param>
        /// <param name="pPartyRelativeTo">partie parent</param>
        /// <param name="pAddNotExistsSQLTRADEACTOR">si true la requête d'insert evite l'injection d'un doublon (via un where not exists)</param>
        /// FI 20200427 [XXXXX] add parameter pAddNotExistsSQLTRADEACTOR
        protected void InsertTradeActorRoleShortForm(string pCS, IDbTransaction pDbTransaction, int pIdT, IParty pParty, RoleActor pRole, IParty pPartyRelativeTo, Boolean pAddNotExistsSQLTRADEACTOR)
        {

            QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, pAddNotExistsSQLTRADEACTOR);
            qryIns.Parameters["IDA"].Value = pParty.OTCmlId;

            qryIns.Parameters["IDT"].Value = pIdT;
            qryIns.Parameters["IDROLEACTOR"].Value = pRole.ToString();
            if (null != pPartyRelativeTo)
                qryIns.Parameters["IDA_ACTOR"].Value = pPartyRelativeTo.OtcmlId;

            if (pAddNotExistsSQLTRADEACTOR)
                qryIns.Query += Cst.CrLf + GetNotExistsSQLTRADEACTOR(qryIns.Parameters["IDA_ACTOR"]);

            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Injecte le trader {pTrader} dans TRADEACTOR
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pParty"></param>
        /// <param name="pTrader"></param>
        /// <param name="pRoleActor"></param>
        private static void InsertTradeActorTraderOrSales(string pCS, IDbTransaction pDbTransaction, int pIdT, IParty pParty, ITrader pTrader, RoleActor pRoleActor)
        {
            int idATrader = pTrader.OTCmlId;
            if (0 < idATrader)
            {
                SQL_Actor sql = new SQL_Actor(pCS, pTrader.OTCmlId);
                if (false == sql.LoadTable(new string[] { "ALGOTYPE" }))
                    throw new NullReferenceException(StrFunc.AppendFormat("Actor Id:{0} doesn't exist", pTrader.OTCmlId));

                QueryParameters qryIns = GetQueryParametersInsertTradeActor(pCS, true);
                qryIns.Parameters["IDA"].Value = idATrader;
                qryIns.Parameters["IDT"].Value = pIdT;
                qryIns.Parameters["IDROLEACTOR"].Value = pRoleActor.ToString();
                qryIns.Parameters["IDA_ACTOR"].Value = pParty.OtcmlId;
                qryIns.Parameters["FACTOR"].Value = (StrFunc.IsFilled(pTrader.StrFactor) ? pTrader.Factor : Convert.DBNull);
                qryIns.Parameters["FIXPARTYROLEQUALIF"].Value = sql.IsAlgo ?
                             ReflectionTools.ConvertEnumToString<FixML.v50SP2.Enum.PartyDetailRoleQualifier>(FixML.v50SP2.Enum.PartyDetailRoleQualifier.Algorithm) :
                             ReflectionTools.ConvertEnumToString<FixML.v50SP2.Enum.PartyDetailRoleQualifier>(FixML.v50SP2.Enum.PartyDetailRoleQualifier.NaturalPerson);

                // FI 20200427 [XXXXX] Appel à GetNotExistsSQLTRADEACTOR
                qryIns.Query += Cst.CrLf + GetNotExistsSQLTRADEACTOR(qryIns.Parameters["IDA_ACTOR"]);

                // FI 20200427 [XXXXX] l'exists est intégré dans l'insert
                /*
                QueryParameters qry = GetQueryExistActorInTradeActor(pCS);
                qry.Parameters["IDA"].Value = idATrader;
                qry.Parameters["IDT"].Value = pIdT;
                qry.Parameters["IDROLEACTOR"].Value = pRoleActor.ToString();
                qry.Parameters["IDA_ACTOR"].Value = pParty.otcmlId;

                Object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                if (null == obj)
                 */
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryIns.Query, qryIns.Parameters.GetArrayDbParameter());

            }
        }

        /// <summary>
        /// Injecte dans la table TRADEACTOR le CONTACT OFFICE et le SETTLEMENT OFFICE associés à la la contrepartie {pParty}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pParty"></param>
        /// <param name="pDoc">DataDocument</param>
        private void InsertTradeActorTradeSide(string pCS, IDbTransaction pDbTransaction, int pIdT, IParty pParty)
        {

            DataDocumentContainer doc = TradeCommonInput.DataDocument;
            ITradeSide tradeSide = doc.GetTradeSide(Tools.GetTradeSideIdFromActor(pParty.Id));
            //
            if (null != tradeSide)
            {
                //
                if (tradeSide.ConfirmerSpecified && tradeSide.Confirmer.PartySpecified)
                {
                    IParty confirmer = TradeCommonInput.DataDocument.GetParty(tradeSide.Confirmer.Party.HRef);
                    if (null == confirmer)
                        throw new NotSupportedException(string.Format("Confirmer:{0} is not find in collection of Parties", confirmer.Id));
                    //
                    // FI 20200427 [XXXXX] l'exists est intégré dans l'insert
                    //QueryParameters qry = GetQueryExistActorInTradeActor(pCS);
                    //qry.Parameters["IDA"].Value = confirmer.OTCmlId;
                    //qry.Parameters["IDT"].Value = pIdT;
                    //qry.Parameters["IDROLEACTOR"].Value = RoleActor.CONTACTOFFICE.ToString();
                    //qry.Parameters["IDA_ACTOR"].Value = pParty.otcmlId;
                    //
                    //Object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                    //if (null == obj)
                    InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, confirmer, RoleActor.CONTACTOFFICE, pParty, true);
                }
                //
                if (tradeSide.SettlerSpecified && tradeSide.Settler.PartySpecified)
                {
                    IParty settler = doc.GetParty(tradeSide.Settler.Party.HRef);
                    if (null == settler)
                        throw new NotSupportedException(string.Format("Settler:{0} is not find in collection of Parties", settler.Id));

                    // FI 20200427 [XXXXX] l'exists est intégré dans l'insert
                    //QueryParameters qry = GetQueryExistActorInTradeActor(pCS);
                    //qry.Parameters["IDA"].Value = settler.OTCmlId;
                    //qry.Parameters["IDT"].Value = pIdT;
                    //qry.Parameters["IDROLEACTOR"].Value = RoleActor.SETTLTOFFICE.ToString();
                    //qry.Parameters["IDA_ACTOR"].Value = pParty.otcmlId;
                    ////
                    //Object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                    //if (null == obj)
                    InsertTradeActorRoleShortForm(pCS, pDbTransaction, pIdT, settler, RoleActor.SETTLTOFFICE, pParty, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdA"></param>
        /// <param name="pTradeId"></param>
        /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (Ajouter une colonne SOURCE pour y faire figurer la RULE utilisée pour le calcul). 
        protected static void InsertTradeId(string pCs, IDbTransaction pDbTransaction, int pIdT, int pIdA, ISchemeId pTradeId)
        {

            string SQLInsert = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.TRADEID + Cst.CrLf;
            SQLInsert += "(IDT, IDA, TRADEID, TRADEIDSCHEME, SOURCE)" + Cst.CrLf;
            SQLInsert += "values (@IDT, @IDA, @TRADEID, @TRADEIDSCHEME, @SOURCE);" + Cst.CrLf;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDT), pIdT);
            dp.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDA), pIdA);
            dp.Add(new DataParameter(pCs, "TRADEID", DbType.AnsiString, SQLCst.UT_DESCRIPTION_LEN), pTradeId.Value);
            dp.Add(new DataParameter(pCs, "TRADEIDSCHEME", DbType.AnsiString, SQLCst.UT_UNC_LEN), pTradeId.Scheme);
            dp.Add(new DataParameter(pCs, "SOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pTradeId.Source);

            QueryParameters qryParameters = new QueryParameters(pCs, SQLInsert, dp);

            DataHelper.ExecuteNonQuery(pCs, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

        }

        /// <summary>
        /// Alimente la table TRADEINSTRUMENT pour un product donné
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pProduct">Représente le product</param>
        /// <param name="pIsUpdateOnly_TradeStream"></param>
        // EG 20100705 Add IDM to TRADEINSTRUMENT (for ETD)
        // EG 20101202 Add POSITIONEFFECT,PRICE,EXECUTIONID to TRADEINSTRUMENT (for ETD)
        // FI 20110214 Add TRDTYPE to TRADEINSTRUMENT (for ETD)
        // EG 20111027 Add ORDERID et ORDERTYPE (for ETD)
        // FI 20120507 Add TRDSUBTYPE 
        // RD 20130109 [18314] update only asset if trade included on invoice or partial modification mode
        // FI 20140812 [XXXXX] add Modify add paramter ASSETCATEGORY
        // EG 20150622 [21143] Add UNITSTRIKEPRICE|STRIKEPRICE
        // EG 20150624 [21149] Add DTSETTLT
        // RD 20150921 [21374] Modify
        // CC 20160805 [22091] Add INPUTSOURCE (for ETD)
        // FI 20161214 [21916] Modify
        // EG 20171020 [23509] Upd IDM_FACILITY, DTEXECUTION, TZFACILITY
        // EG 20171025 [23509] Add TZDLVY, Change DateTimeOffset To DateTime2 on DTDLVY and DTEXECUTION
        // EG 20180906 PERF Add DTOUT (Alloc ETD only)
        // PL 20181023 PERF Remove (temporarily) DTOUT 
        // EG 20190730 Add TYPEPRICE|ACCINTRATE (AssetMeasure|AccruedInterestRate - DST)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected virtual void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, StrategyContainer pProduct, bool pIsUpdateOnly_TradeInstrument)
        {

        }
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected virtual void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct, bool pIsUpdateOnly_TradeInstrument)
        {
            //int streamNo = 1;
            int instrumentNo = Convert.ToInt32(pProduct.ProductBase.Id.Replace(Cst.FpML_InstrumentNo, string.Empty));
            //
            #region Insert TradeInstrument
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "INSTRUMENTNO", DbType.Int32), instrumentNo);
            parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), Convert.DBNull);
            parameters.Add(new DataParameter(pCS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);

            if (pIsUpdateOnly_TradeInstrument)
            {
                pProduct.SetTradeInstrumentAssetParameter(CSTools.SetCacheOn(pCS), parameters);
                // FI 20190821 [24853] Réécriture de l'update pour tenir compte de ASSETCATEGORY
                if (parameters["IDASSET"].Value == Convert.DBNull)
                {
                    string sqlUpdate = @"update dbo.TRADEINSTRUMENT
                    set IDASSET= @IDASSET, ASSETCATEGORY = @ASSETCATEGORY 
                    where (IDT=@IDT) and (INSTRUMENTNO=@INSTRUMENTNO) and (IDASSET is not null)";
                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlUpdate, parameters.GetArrayDbParameter());
                }
                else
                {
                    string sqlUpdate = @"update dbo.TRADEINSTRUMENT
                    set IDASSET= @IDASSET, ASSETCATEGORY = @ASSETCATEGORY 
                    where (IDT=@IDT) and (INSTRUMENTNO=@INSTRUMENTNO) and ((IDASSET is null) or (IDASSET!=@IDASSET) or (ASSETCATEGORY!=@ASSETCATEGORY))";
                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlUpdate, parameters.GetArrayDbParameter());
                }
            }
            else
            {
                parameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), pProduct.ProductBase.ProductType.OTCmlId);
                parameters.Add(new DataParameter(pCS, "IDM", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_CSSCUSTODIAN", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "POSITIONEFFECT", DbType.AnsiStringFixedLength, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "PRICE", DbType.Decimal), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "UNITPRICE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull); // FI 20161214 [21916] add UNITPRICE
                parameters.Add(new DataParameter(pCS, "EXECUTIONID", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "RELTDPOSID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "TRDTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "TRDSUBTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "ORDERID", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "ORDERTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                // CC 20160805 [22091]
                parameters.Add(new DataParameter(pCS, "INPUTSOURCE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                // RD 20150921 [21374] Modify parameter type from Int32 to Int64
                // EG 20170127 Qty Long To Decimal
                parameters.Add(new DataParameter(pCS, "QTY", DbType.Decimal), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "UNITQTY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull); // FI 20161214 [21916] add UNITQTY
                // 20120711 MF Ticket 18006
                parameters.Add(new DataParameter(pCS, "SECONDARYTRDTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                // EG 20140223 [19575][19666] IDA_DEALER / IDB_DEALER / IDA_CLEARER / IDB_CLEARER / SIDE
                parameters.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "SIDE", DbType.AnsiString, SQLCst.UT_ENUMCHAR_OPTIONAL_LEN), Convert.DBNull);
                // EG 20140225 [19575][19666] IDA_RISK / IDB_RISK / IDA_ENTITY (MR, CB, CP)
                parameters.Add(new DataParameter(pCS, "IDA_RISK", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_RISK", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_ENTITY", DbType.Int32), Convert.DBNull);
                // EG 20140223 [19575][19666] IDA_BUYER / IDB_BUYER / IDA_SELLER / IDB_SELLER 
                parameters.Add(new DataParameter(pCS, "IDA_BUYER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_BUYER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDA_SELLER", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "IDB_SELLER", DbType.Int32), Convert.DBNull);

                // EG 20150622 [21143]
                parameters.Add(new DataParameter(pCS, "UNITSTRIKEPRICE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "STRIKEPRICE", DbType.Decimal), Convert.DBNull);

                // EG 20150624 [21149]
                parameters.Add(new DataParameter(pCS, "DTSETTLT", DbType.Date), Convert.DBNull); // FI 20201006 [XXXXX] DbType.Date

                // EG 20161122 New Commodity Derivative
                parameters.Add(new DataParameter(pCS, "DTDLVYSTART", DbType.DateTime2), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "DTDLVYEND", DbType.DateTime2), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "TZDLVY", DbType.AnsiString, SQLCst.UT_TIMEZONE_LEN), Convert.DBNull);

                // EG 20171016 [23509] 
                parameters.Add(new DataParameter(pCS, "IDM_FACILITY", DbType.Int32), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "DTEXECUTION", DbType.DateTime2), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "TZFACILITY", DbType.AnsiString, SQLCst.UT_TIMEZONE_LEN), Convert.DBNull);

                parameters.Add(new DataParameter(pCS, "TYPEPRICE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Convert.DBNull);
                parameters.Add(new DataParameter(pCS, "ACCINTRATE", DbType.Decimal), Convert.DBNull);

                pProduct.SetTradeInstrumentParameters(CSTools.SetCacheOn(pCS), pDbTransaction, parameters);

                // EG 20150622 [21143]
                // EG 20150624 [21149]
                // CC 20160805 [22091]
                // FI 20161214 [21916] add UNITPRICE, UNITQTY
                // EG 20171016 [23509] 
                string sqlInsert = @"insert into dbo.TRADEINSTRUMENT 
                (IDT, IDI, INSTRUMENTNO, IDM, IDASSET, ASSETCATEGORY, POSITIONEFFECT, TYPEPRICE, PRICE, UNITPRICE, EXECUTIONID, RELTDPOSID, TRDTYPE, TRDSUBTYPE,  ORDERID, ORDERTYPE, INPUTSOURCE, QTY, UNITQTY, SECONDARYTRDTYPE, 
                IDA_DEALER, IDB_DEALER, IDA_CLEARER, IDB_CLEARER, SIDE, IDA_RISK, IDB_RISK, IDA_ENTITY, IDA_CSSCUSTODIAN, IDA_BUYER, IDB_BUYER, IDA_SELLER, IDB_SELLER, 
                UNITSTRIKEPRICE, STRIKEPRICE, DTSETTLT, DTDLVYSTART, DTDLVYEND, TZDLVY, IDM_FACILITY, DTEXECUTION, TZFACILITY, ACCINTRATE) 
                values (@IDT,@IDI, @INSTRUMENTNO, @IDM, @IDASSET, @ASSETCATEGORY, @POSITIONEFFECT, @TYPEPRICE, @PRICE, @UNITPRICE, @EXECUTIONID, @RELTDPOSID, @TRDTYPE, @TRDSUBTYPE, @ORDERID, @ORDERTYPE, @INPUTSOURCE, @QTY, @UNITQTY, @SECONDARYTRDTYPE, 
                @IDA_DEALER, @IDB_DEALER, @IDA_CLEARER, @IDB_CLEARER, @SIDE, @IDA_RISK, @IDB_RISK, @IDA_ENTITY, @IDA_CSSCUSTODIAN, @IDA_BUYER, @IDB_BUYER, @IDA_SELLER, @IDB_SELLER, 
                @UNITSTRIKEPRICE, @STRIKEPRICE, @DTSETTLT, @DTDLVYSTART, @DTDLVYEND, @TZDLVY, @IDM_FACILITY, @DTEXECUTION, @TZFACILITY, @ACCINTRATE)";
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlInsert, parameters.GetArrayDbParameter());
            }
            #endregion Insert TradeInstrument
        }
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected virtual void InsertTradeStream(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// EG 20140224 [19667] 
        // RD 20161117 Add Reflect Link
        protected virtual void SaveTradelink(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, string pIdentifier)
        {
            // EG 20140224 [19667] Suppression des Tradelink sur CB hors création et hors de la boucle d'insertion
            if (ArrFunc.IsFilled(TradeCommonInput.TradeLink) && TradeCommonInput.Product.IsCashBalance)
            {
                if (false == Cst.Capture.IsModeNewOrDuplicate(pCaptureMode))
                {
                    DataParameters parameters = new DataParameters();
                    parameters.Add(new DataParameter(pCS, "IDT_A", DbType.Int32), pIdT);
                    string sqlDelete = SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADELINK + SQLCst.WHERE + "(IDT_A = @IDT_A)" + Cst.CrLf;
                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlDelete, parameters.GetArrayDbParameter());
                }
            }
            TradeLink.TradeLink tradeLink;
            if (Cst.Capture.IsModeRemoveReplace(pCaptureMode))
            {
                tradeLink = new TradeLink.TradeLink(
                    pIdT, TradeCommonInput.RemoveTrade.idTCancel, EFS.TradeLink.TradeLinkType.Replace);

                tradeLink.Insert(pCS, pDbTransaction);
            }
            else if (Cst.Capture.IsModeReflect(pCaptureMode))
            {
                tradeLink = new TradeLink.TradeLink(
                    pIdT, TradeCommonInput.IdT, EFS.TradeLink.TradeLinkType.Reflect);

                tradeLink.Insert(pCS, pDbTransaction);
            }
            //20091016 FI [Rebuild identification] use SQLTrade.Identifier 
            else if (Cst.Capture.IsModeUpdateGen(pCaptureMode) &&
                    (pIdentifier != TradeCommonInput.SQLTrade.Identifier))
            {
                tradeLink = new TradeLink.TradeLink(
                    pIdT, pIdT, TradeLink.TradeLinkType.NewIdentifier, null, null,
                    new string[2] { TradeCommonInput.SQLTrade.Identifier, pIdentifier },
                    new string[2] { TradeLink.TradeLinkDataIdentification.OldIdentifier.ToString(), TradeLink.TradeLinkDataIdentification.NewIdentifier.ToString() });
                //
                tradeLink.Insert(pCS, pDbTransaction);
            }

            if (ArrFunc.IsFilled(TradeCommonInput.TradeLink))
            {
                foreach (TradeLink.TradeLink link in TradeCommonInput.TradeLink)
                {
                    // Ajouter la donnée "data" sur le Trade en cours
                    List<string> data = new List<string>
                    {
                        pIdentifier
                    };

                    List<string> dataIdentification = new List<string>
                    {
                        link.LinkData_A
                    };

                    for (int i = 0; i < (ArrFunc.Count(link.Data) - 1); i++)
                    {
                        data.Add(link.Data[i]);
                        dataIdentification.Add(link.DataIdentification[i]);
                    }
                    tradeLink = new TradeLink.TradeLink(pIdT, link.IdT_B, link.Link, link.IdData, link.IdDataIdentification, data.ToArray(), dataIdentification.ToArray());
                    //
                    tradeLink.Insert(pCS, pDbTransaction);
                }
            }
        }

        /// <summary>
        /// Alimente la table TRADEINSTRUMENT
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIsUpdateOnly_TradeStream"></param>
        /// <param name="pIsUpdateOnly_TradeInstrument"></param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected void SaveTradeInstruments(string pCS, IDbTransaction pDbTransaction, int pIdT, bool pIsUpdateOnly_TradeInstrument)
        {
            ProductContainer tradeProduct = TradeCommonInput.DataDocument.CurrentProduct;

            InsertTradeInstrument(pCS, pDbTransaction, pIdT, tradeProduct, pIsUpdateOnly_TradeInstrument);

            #region Multi-Product: Swaption, Strategy, ...
            if (tradeProduct.IsStrategy)
            {
                foreach (ProductContainer product in ((StrategyContainer)tradeProduct).GetSubProduct())
                    InsertTradeInstrument(pCS, pDbTransaction, pIdT, product, pIsUpdateOnly_TradeInstrument);
            }
            else if (tradeProduct.IsSwaption)
            {
                IProduct swap = (IProduct)((ISwaption)tradeProduct.Product).Swap;
                ProductContainer swapContainer = new ProductContainer(swap, TradeCommonInput.DataDocument);
                InsertTradeInstrument(pCS, pDbTransaction, pIdT, swapContainer, pIsUpdateOnly_TradeInstrument);
            }
            #endregion Multi-Product: Swaption, Strategy, ...
        }

        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected void SaveTradeInstrumentAndStream(string pCS, IDbTransaction pDbTransaction, int pIdT, Cst.Capture.ModeEnum pCaptureMode)
        {
            ProductContainer tradeProduct = TradeCommonInput.DataDocument.CurrentProduct;

            bool isUpdateOnlyTradeInstrument = false;
            bool isSaveTradeInstrumentAndTradeStream = true;
            if (Cst.Capture.IsModeUpdateGen(pCaptureMode))
            {
                isSaveTradeInstrumentAndTradeStream = IsDeleteEvent(pCaptureMode);

                if ((Cst.Capture.IsModeUpdate(pCaptureMode) && TradeCommonInput.IsTradeInInvoice()) ||
                    Cst.Capture.IsModeUpdatePostEvts(pCaptureMode))
                {
                    isUpdateOnlyTradeInstrument = true;
                }
            }
            if (isSaveTradeInstrumentAndTradeStream || isUpdateOnlyTradeInstrument)
            {
                #region Multi-Product: Swaption, Strategy, ...
                if (tradeProduct.IsStrategy)
                {
                    InsertTradeInstrument(pCS, pDbTransaction, pIdT, (StrategyContainer)tradeProduct, isUpdateOnlyTradeInstrument);
                }
                else if (tradeProduct.IsSwaption)
                {
                    IProduct swap = (IProduct)((ISwaption)tradeProduct.Product).Swap;
                    ProductContainer swapContainer = new ProductContainer(swap, TradeCommonInput.DataDocument);
                    InsertTradeInstrument(pCS, pDbTransaction, pIdT, swapContainer, isUpdateOnlyTradeInstrument);
                }
                else if (false == isUpdateOnlyTradeInstrument)
                {
                    InsertTradeStream(pCS, pDbTransaction, pIdT, tradeProduct);
                }
                #endregion Multi-Product: Swaption, Strategy, ...
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pDtSys"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected void UpdateTradeStSys(string pCS, IDbTransaction pDbTransaction, int pIdT,
            CaptureSessionInfo pSessionInfo, DateTime pDtSys, Pair<Cst.StatusUsedBy, string> pStUsedBy)
        {


            SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(pCS, pIdT);
            string sqlQuery = sqlTradeStSys.GetQueryParameters(
                    new string[]{"IDT",
                        "IDSTENVIRONMENT", "DTSTENVIRONMENT", "IDASTENVIRONMENT",
                        "IDSTBUSINESS", "DTSTBUSINESS", "IDASTBUSINESS",
                        "IDSTACTIVATION", "DTSTACTIVATION", "IDASTACTIVATION",
                        "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY",
                        "IDSTPRIORITY", "DTSTPRIORITY", "IDASTPRIORITY", "ROWATTRIBUT"}).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, sqlQuery);
            DataTable dt = ds.Tables[0];
            DataRow dr = dt.Rows[0];

            //Dans l'importation des trade pSessionInfo.user.idA représente l'acteur spécifié dans le mapping, ou le requester
            TradeCommonInput.TradeStatus.UpdateRowTradeStSys(dr, pSessionInfo.user.IdA, pDtSys, pStUsedBy);
            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, sqlQuery, dt);
        }

        /// <summary>
        /// Injecte dans ATTACHEDDOC les documents rattachés au trade source {pIdT_Source} dans le trade Cible {pIdT} 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT_Source">OTCmlId du trade source</param>
        /// <param name="pIdT">OTCmlId du trade cible</param>
        private void CopyAttachedDoc(string pCs, IDbTransaction pDbTransaction, int pIdT_Source, int pIdT)
        {

            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.ATTACHEDDOC.ToString() + "(TABLENAME,ID,DOCNAME,DOCTYPE,LODOC,DTUPD,IDAUPD,EXTLLINK) " + Cst.CrLf;
            sqlQuery += SQLCst.SELECT + "adSource.TABLENAME,@IDT,adSource.DOCNAME,adSource.DOCTYPE,adSource.LODOC,adSource.DTUPD,adSource.IDAUPD,adSource.EXTLLINK" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ATTACHEDDOC.ToString() + " adSource" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "adSource.ID=@IDT_SOURCE and adSource.TABLENAME=@TRADE" + Cst.CrLf;
            
            DataParameters parameters;
            parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCs, "IDT_SOURCE", DbType.Int32), pIdT_Source);
            parameters.Add(new DataParameter(pCs, "TRADE", DbType.AnsiString, 32), "TRADE");
            
            DataHelper.ExecuteNonQuery(pCs,pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());

        }

        /// <summary>
        /// Injecte dans NOTEPAD les notes rattachées au trade source {pIdT_Source} dans le trade cible {pIdT} 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT_Source">OTCmlId du trade source</param>
        /// <param name="pIdT">OTCmlId du trade cible</param>
        private void CopyNotepad(string pCs, IDbTransaction pDbTransaction, int pIdT_Source, int pIdT)
        {

            string sqlQuery = SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.NOTEPAD.ToString() + "(TABLENAME,ID,LONOTE,DTUPD,IDAUPD,EXTLLINK) " + Cst.CrLf;
            sqlQuery += SQLCst.SELECT + "npSource.TABLENAME,@IDT,npSource.LONOTE,npSource.DTUPD,npSource.IDAUPD,npSource.EXTLLINK" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.NOTEPAD.ToString() + " npSource" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "npSource.ID = @IDT_SOURCE and npSource.TABLENAME=@TRADE" + Cst.CrLf;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCs, "IDT_SOURCE", DbType.Int32), pIdT_Source);
            parameters.Add(new DataParameter(pCs, "TRADE", DbType.AnsiString, 32), "TRADE");

            DataHelper.ExecuteNonQuery(pCs, pDbTransaction, CommandType.Text, sqlQuery, parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Alimente la table TRADEACTOR
        /// <para>Alimente TRADEACTOR pour les acteurs contreparties et les brokers</para>
        /// <para> </para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// FI 20160810 [22086] Modify
        /// FI 20171003 [23464] Modify
        /// FI 20171214 [XXXXX] Modify
        protected virtual void SaveTradeActor(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            // FI 20160810 [22086]  add .Where(x=> x.OTCmlId >0)
            // sur des trades incomplets avec des contreparties inconnues, il peut avoir x.OTCmlId =0 > pas nécessaire d'alimenter TRADEACTOR dans ce cas  car IDA=0
            foreach (IParty party in TradeCommonInput.DataDocument.Party.Where(x => x.OTCmlId > 0))
            {
                if (TradeCommonInput.DataDocument.IsPartyCounterParty(party))
                {
                    // Ajout du rôle COUNTERPARTY
                    InsertTradeActorCounterparty(pCS, pDbTransaction, pIdT, party);
                }

                if (TradeCommonInput.DataDocument.IsPartyBroker(party.OTCmlId))
                {
                    IParty[] counterparty = TradeCommonInput.DataDocument.GetCounterpartyUsingBroker(party);
                    if (ArrFunc.IsFilled(counterparty))
                    {
                        for (int i = 0; i < counterparty.Length; i++)
                            InsertTradeActorBroker(pCS, pDbTransaction, pIdT, party, counterparty[i]);
                    }
                    else
                    {
                        InsertTradeActorBroker(pCS, pDbTransaction, pIdT, party, null);
                    }

                    // FI 20171214 [XXXXX] Alimentation du trader (Bizarrement ce bug existe depuis tjs)
                    ITrader[] trader = TradeCommonInput.DataDocument.GetPartyTrader(party.Id);
                    if (ArrFunc.IsFilled(trader))
                    {
                        for (int i = 0; i < trader.Length; i++)
                            InsertTradeActorTraderOrSales(pCS, pDbTransaction, pIdT, party, trader[i], RoleActor.TRADER);
                    }
                }
            }
        }

        /// <summary>
        /// Aliment les tables TRADEID et LINKID
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        protected void SaveTradeIdAndLinkId(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {

            foreach (IParty party in TradeCommonInput.DataDocument.Party)
            {

                IPartyTradeIdentifier partyTradeIdentifier = TradeCommonInput.DataDocument.GetPartyTradeIdentifier(party.Id);
                if (null != partyTradeIdentifier)
                {
                    // RD 20120322 / Intégration de trade "Incomplet"
                    // Correction de BUG en cas de non existence de "tradeId"
                    if (ArrFunc.IsFilled(partyTradeIdentifier.TradeId))
                    {
                        // EG 20240227 [WI854][WI855][WI858] Trade input : Set New Columns TVTIC| TRDID
                        foreach (ITradeId tradeId in partyTradeIdentifier.TradeId)
                        {
                            if (null != tradeId && StrFunc.IsFilled(tradeId.Value) && tradeId.IsRelativeToActor)
                                InsertTradeId(pCS, pDbTransaction, pIdT, party.OTCmlId, tradeId);
                        }
                    }
                    //
                    if (partyTradeIdentifier.LinkIdSpecified)
                    {
                        //20080430 PL Correction de BUG avec ISchemeId ??? (a vour avec EG)
                        foreach (ILinkId linkId in partyTradeIdentifier.LinkId)
                        {
                            if (null != linkId && StrFunc.IsFilled(linkId.Value))
                                InsertLinkId(pCS, pDbTransaction, pIdT, party.OTCmlId, linkId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Supprime les enregistrements présents dans LINKID, TRADEID, TRADEINSTRUMENT, TRADESTREAM, TRADEACTOR 
        /// <para>la suppresion des enregistrements présents dans TRADEINSTRUMENT et TRADESTREAM sont conditionné par {pIsDelEVENT}</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIsDelTRADEINSTRUMENTAndTRADESTREAM">conditionne la suppression de TRADESTREAM et TRADEINSTRUMENT</param>
        /// <exception cref="DataHelperException lorsqu'un ordre sql génère une erreur"></exception>
        /// FI 20140415 [XXXXX] Mise en place de n ordres SQL indépendants, Mise en place d'une DataHelperException en cas d'erreur
        /// RD 20130108 [18314] le Delete des tables TRADEINSTRUMENT et TRADESTREAM qui est conditionné par pIsDelTRADEINSTRUMENTAndTRADESTREAM
        protected void DeleteTradeTables(string pCS, IDbTransaction pDbTransaction, int pIdT, bool pIsDelTRADEINSTRUMENTAndTRADESTREAM)
        {

            DataParameter paramIdT = new DataParameter(pCS, "IDT", DbType.Int32)
            {
                Value = pIdT
            };
            
            SQLWhere sqlWhere = new SQLWhere("IDT=@IDT");
            try
            {
                StrBuilder sqlQuery = new StrBuilder(string.Empty);
                sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.LINKID.ToString() + Cst.CrLf;
                sqlQuery += sqlWhere.ToString();
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), paramIdT.DbDataParameter);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete LINKID for trade (id:{0})", pIdT.ToString()), ex);
            }

            try
            {
                StrBuilder sqlQuery = new StrBuilder(string.Empty);
                sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADEID.ToString() + Cst.CrLf;
                sqlQuery += sqlWhere.ToString();
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), paramIdT.DbDataParameter);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete table TRADEID for trade (id:{0})", pIdT.ToString()), ex);
            }

            //
            if (pIsDelTRADEINSTRUMENTAndTRADESTREAM)
            {
                try
                {
                    StrBuilder sqlQuery = new StrBuilder(string.Empty);
                    sqlQuery = new StrBuilder(string.Empty);
                    sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADEINSTRUMENT.ToString() + Cst.CrLf;
                    sqlQuery += sqlWhere.ToString();
                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), paramIdT.DbDataParameter);
                }
                catch (Exception ex)
                {
                    throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete table TRADEINSTRUMENT for trade (id:{0})", pIdT.ToString()), ex);
                }

                try
                {
                    StrBuilder sqlQuery = new StrBuilder(string.Empty);
                    sqlQuery = new StrBuilder(string.Empty);
                    sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADESTREAM.ToString() + Cst.CrLf;
                    sqlQuery += sqlWhere.ToString();
                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), paramIdT.DbDataParameter);
                }
                catch (Exception ex)
                {
                    throw new DataHelperException(DataHelperErrorEnum.query,
                        StrFunc.AppendFormat("Error on delete table TRADESTREAM for trade (id:{0})", pIdT.ToString()), ex);
                }
                //
                //FI 20120618 [17904] Ajout de la table TRADEASSET
                //sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADEASSET.ToString() + Cst.CrLf;
                //sqlQuery += sqlWhere.ToString() + ";" + Cst.CrLf;
            }
            //FI 20120618 [18105] le delete est fait systématiquement 
            //En modification de titre sans regénération des évènements il est faut un 
            //delete de TRADEASSET, puis un insert avec les nouvelles données (Exemple CODEISIN) 
            DeleteTradeAsset(pCS, pDbTransaction, pIdT);

            try
            {
                StrBuilder sqlQuery = new StrBuilder(string.Empty);
                sqlQuery += SQLCst.DELETE_DBO + Cst.OTCml_TBL.TRADEACTOR.ToString() + Cst.CrLf;
                sqlQuery += sqlWhere.ToString();
                DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), paramIdT.DbDataParameter);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                        StrFunc.AppendFormat("Error on delete table TRADEACTOR for trade (id:{0})", pIdT.ToString()), ex);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        // FI 20120618 [17904] alimentation de TRADEASSET
        protected virtual void SaveTradeAsset(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
        }

        /// <summary>
        /// Alimentation de POSUTI
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pIdaUser"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pIsAutoTransaction"></param>
        /// FI 20140206 [19564] add method
        protected virtual void SavePositionUTI(string pCS, IDbTransaction pDbTransaction, SpheresIdentification pTradeIdentification, int pIdaUser, DateTime pDtSys, Boolean pIsAutoTransaction)
        {
        }


        /// <summary>
        ///  Alimentation de la table TRADE
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSys">Date début procédure d'enregistrement</param>
        /// <returns></returns>
        // FI 20140703 [20161] add column TRADE.TIMING
        // EG 20171004 [23452] Gestion tradeDateTime
        // EG 20171025 [23509] Upd gestion des dates (DTTRADE, DTEXECUTION, DTORDERENTERED, DTBUSINESS) et TZFACILITY
        // EG 20180205 [23769] Use pDbTransaction  
        // EG 20180906 PERF Add DTOUT (Alloc ETD only)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected int SaveTrade(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode,
                               SpheresIdentification pTradeIdentification,
                               CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            int ret = 0;

            string[] columns = GetTradeColumns();
            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(pCS, TradeCommonInput.Identification.OTCmlId);
            QueryParameters qry = sqlTrade.GetQueryParameters(columns);

            DataSet dsTrade = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            DataTable dtTrade = dsTrade.Tables[0];

            DataRow dr;
            if (Cst.Capture.IsModeNewCapture(pCaptureMode))
            {
                dr = dtTrade.NewRow();
                dtTrade.Rows.Add(dr);
            }
            else
            {
                dr = dtTrade.Rows[0];
            }

            #region Update Dr
            dr.BeginEdit();

            bool isEditIdentification = (false == Cst.Capture.IsModeCorrection(pCaptureMode));
            if (isEditIdentification)
            {
                dr["IDENTIFIER"] = pTradeIdentification.Identifier;
                dr["DISPLAYNAME"] = StrFunc.IsFilled(pTradeIdentification.Displayname) ? pTradeIdentification.Displayname : pTradeIdentification.Identifier;
                dr["DESCRIPTION"] = StrFunc.IsEmpty(pTradeIdentification.Description) ? Convert.DBNull : pTradeIdentification.Description;
                dr["EXTLLINK"] = StrFunc.IsFilled(pTradeIdentification.Extllink) ? pTradeIdentification.Extllink : Convert.DBNull;
            }

            dr["IDI"] = TradeCommonInput.Product.IdI; //Ne devrait jamais être effectué

            // EG 20120516 Gestion column IDINVOICINGRULES dans TRADE
            dr["IDINVOICINGRULES"] = Convert.DBNull;
            if (TradeCommonInput.Product.IsInvoice || TradeCommonInput.Product.IsAdditionalInvoice || TradeCommonInput.Product.IsCreditNote)
            {
                IInvoice invoice = (IInvoice)TradeCommonInput.DataDocument.CurrentTrade.Product;
                dr["IDINVOICINGRULES"] = invoice.Scope.OTCmlId;
            }

            bool isEditSource = Cst.Capture.IsModeNewCapture(pCaptureMode);
            if (isEditSource)
            {
                #region IDT_SOURCE, IDT_TEMPLATE
                if (Cst.Capture.ModeEnum.New == pCaptureMode)
                {
                    // En creation Le template == l'opération source
                    dr["IDT_TEMPLATE"] = TradeCommonInput.IdT;
                    dr["IDT_SOURCE"] = TradeCommonInput.IdT; //20071122 PL 
                }
                else if ((Cst.Capture.ModeEnum.Duplicate == pCaptureMode) ||
                         (Cst.Capture.ModeEnum.Reflect == pCaptureMode) ||
                         (Cst.Capture.ModeEnum.RemoveReplace == pCaptureMode) ||
                         (Cst.Capture.ModeEnum.PositionTransfer == pCaptureMode))
                {
                    // En duplication  Le template == Le Template de l'opération source
                    int IdT_Template = TradeRDBMSTools.GetTradeIdT(pCS, pDbTransaction, TradeCommonInput.GetTemplateIdentifier(CSTools.SetCacheOn(pCS), pDbTransaction));
                    if (0 < IdT_Template)
                        dr["IDT_TEMPLATE"] = IdT_Template;
                    dr["IDT_SOURCE"] = TradeCommonInput.IdT;


                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("CaptureMode {0} is not implemented", pCaptureMode.ToString()));
                #endregion IDT_SOURCE, IDT_TEMPLATE
            }

            bool isEditDate = (false == Cst.Capture.IsModeCorrection(pCaptureMode));
            if (isEditDate)
            {

                // NB: Sur un Template cette date peut être non renseignée, car non obligatoire.
                //DateTime tradeDate = TradeCommonInput.DataDocument.tradeDate;
                //if (DtFunc.IsDateTimeEmpty(tradeDate))
                //{
                //    // EG 20160404 Migration vs2013
                //    // #warning SQLSERVER 2005 : tradeDate est initialisé à MinDate de SQLSERVER, Avec les nouveaux types Date de SQLSERVER 2008 on pourra y stocker DateTime.MinValue
                //    //DTTRADE est valorisée avec le MinDate de SQLSERVER2005
                //    tradeDate = new DtFunc().StringDateISOToDateTime("1900-01-01");
                //}
                ////PL 20130213 Création de la méthode GetBusinessDate()
                //DateTime businessDate = TradeCommonInput.Product.GetBusinessDate();

                //dr["DTTRADE"] = tradeDate;
                //dr["DTTIMESTAMP"] = DtFunc.IsDateTimeEmpty(TradeCommonInput.DataDocument.timeStamping) ? Convert.DBNull : TradeCommonInput.DataDocument.timeStamping;

                // EG 20171004 [23452] 

                // TRADE DATE 
                DateTime tradeDate = TradeCommonInput.DataDocument.TradeDate;
                if (DtFunc.IsDateTimeEmpty(tradeDate))
                    tradeDate = new DtFunc().StringDateISOToDateTime("1900-01-01");
                dr["DTTRADE"] = tradeDate;

                // EXECUTIONDATETIME
                // EG 20171031 [23509] Upd
                // EG 20171109 [23509] Upd
                string tzdbid = TradeCommonInput.DataDocument.GetTradeTimeZone(pCS, pDbTransaction, pSessionInfo.user.Entity_IdA, Tz.Tools.UniversalTimeZone);
                dr["TZFACILITY"] = tzdbid;

                Nullable<DateTimeOffset> dtOrderEntered = TradeCommonInput.DataDocument.GetOrderEnteredDateTimeOffset();
                dr["DTORDERENTERED"] = dtOrderEntered.HasValue ? dtOrderEntered.Value.DateTime : Convert.DBNull;

                if (TradeCommonInput.Product.IsTradeMarket)
                {
                    // EG 20171031 [23509] Upd
                    Nullable<DateTimeOffset> dtExecutionDateTime = TradeCommonInput.DataDocument.GetExecutionDateTimeOffset();

                    // RD 20180108 [23695][23705] Add test
                    if (dtExecutionDateTime != null)
                    {
                        dr["DTEXECUTION"] = dtExecutionDateTime.Value.DateTime;
                        // Le timestamp est sans conversion UTC => dans le timezone de la plateforme (Entité si non trouvée) ou Etc/UTC si non trouvée)
                        dr["DTTIMESTAMP"] = Tz.Tools.FromTimeZone(dtExecutionDateTime.Value, tzdbid).Value.DateTime;
                    }
                    // EG 20240531 [WI926] DTORDERENTERD|DTEXECUTION are made optional if the trade is a TEMPLATE
                    else if (TradeCommonInput.TradeStatus.IsStEnvironment_Template)
                    {
                        dr["DTEXECUTION"] = Convert.DBNull;
                        // Le timestamp est sans conversion UTC => dans le timezone de la plateforme (Entité si non trouvée) ou Etc/UTC si non trouvée)
                        dr["DTTIMESTAMP"] = dtOrderEntered.HasValue ? Tz.Tools.FromTimeZone(dtOrderEntered.Value, tzdbid).Value.DateTime : Convert.DBNull;
                    }
                    else if (false == IsInputIncompleteAllow(pCaptureMode))
                        throw new Exception("execution DateTime Offset is missing");
                }
                else if (TradeCommonInput.Product.IsADM || TradeCommonInput.Product.IsRISK)
                {
                    // EG 20171031 [23509] Upd
                    dr["DTEXECUTION"] = Convert.DBNull;
                    // Le timestamp est sans conversion UTC => dans le timezone de la plateforme (Entité si non trouvée) ou Etc/UTC si non trouvée)
                    dr["DTTIMESTAMP"] = dtOrderEntered.HasValue ? Tz.Tools.FromTimeZone(dtOrderEntered.Value, tzdbid).Value.DateTime : Convert.DBNull;
                }

                // EG 20230505 [WI617] Appel à GetBusinessDate2 (permet de récupérer DTTRADE sur négociation sans compensation)
                DateTime businessDate = TradeCommonInput.Product.GetBusinessDate2();
                if (DtFunc.IsDateTimeFilled(businessDate))
                    dr["DTBUSINESS"] = businessDate;

                dr["DTOUT"] = Convert.DBNull;
                Nullable<DateTime> dtOut = GetDtOut(pCS, pDbTransaction);
                if (dtOut.HasValue)
                    dr["DTOUT"] = dtOut;
            }

            dr["DTSYS"] = pDtSys;

            // FI 20131007 [] Alimentation de source avec le nom du service et son instance lorsque l'application est de type service
            string source = pSessionInfo.session.AppInstance.AppNameVersion;
            if (pSessionInfo.session.AppInstance.GetType().Equals(typeof(AppInstanceService)))
                source = ((AppInstanceService)(pSessionInfo.session.AppInstance)).ServiceName;

            dr["SOURCE"] = source;

            // FI 20140703 [20161] add TIMING
            dr["TIMING"] = Convert.DBNull;
            if (TradeCommonInput.Product.IsMarginRequirement)
            {
                MarginRequirementContainer mr = new MarginRequirementContainer((IMarginRequirement)TradeCommonInput.Product.Product);
                // EG 20160404 Migration vs2013
                //if (null != mr.timing)
                dr["TIMING"] = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(mr.Timing);
            }
            // EG 20240227 [WI854][WI855][WI858] Trade input : Set New Columns TVTIC| TRDID
            dr["TVTIC"] = TradeCommonInput.DataDocument.GetTradingVenueTransactionIdentificationCode()?? Convert.DBNull;
            dr["TRDID"] = TradeCommonInput.DataDocument.GetMarketTransactionId() ?? Convert.DBNull;

            dr.EndEdit();

            UpdateRowTradeStSys(dr, pCaptureMode, pSessionInfo, pDtSys);

            // FI 20200424 [XXXXX] CacheOn
            TradeCommonInput.Product.SetTradeInstrumentToDataRow(CSTools.SetCacheOn(pCS), pDbTransaction, dr);

            #endregion Update dr

            DataHelper.ExecuteDataAdapter(pCS, pDbTransaction, qry.QueryReplaceParameters, dtTrade);

            if (Cst.Capture.IsModeNewCapture(pCaptureMode))
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), Convert.ToString(dr["IDENTIFIER"]));
                parameters.Add(new DataParameter(pCS, "DTTRADE", DbType.Date), Convert.ToDateTime(dr["DTTRADE"]));  // FI 20201006 [XXXXX] DbType.Date
                parameters.Add(new DataParameter(pCS, "IDI", DbType.Int32), Convert.ToInt32(dr["IDI"]));

                StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
                sqlQuery += "IDT";
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "IDENTIFIER=@IDENTIFIER" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "DTTRADE=@DTTRADE" + Cst.CrLf;
                sqlQuery += SQLCst.AND + "IDI=@IDI" + Cst.CrLf;

                QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

                object obj = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if ((null != obj) && (false == Convert.IsDBNull(obj)))
                {
                    ret = Convert.ToInt32(obj);
                    // FI 20140217 [19618] Sauvegarde de l'IdT dans {pTradeIdentification}
                    pTradeIdentification.OTCmlId = ret;
                }
            }
            else
            {
                ret = Convert.ToInt32(dr["IDT"]);
            }


            return ret;
        }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected void SaveTradeXML(string pCS, IDbTransaction pDbTransaction, int pIdT, string pDataXml, Cst.Capture.ModeEnum pCaptureMode)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
            parameters.Add(new DataParameter(pCS, "TRADEXML", DbType.Xml), pDataXml.ToString());
            string sqlQuery;
            if (Cst.Capture.IsModeNewCapture(pCaptureMode))
            {
                sqlQuery = @"insert into dbo.TRADEXML (IDT, TRADEXML, EFSMLVERSION) values (@IDT, @TRADEXML, @EFSMLVERSION)";
                parameters.Add(new DataParameter(pCS, "EFSMLVERSION", DbType.AnsiString, 16),
                    ReflectionTools.ConvertEnumToString<EfsMLDocumentVersionEnum>(TradeCommonInput.EfsMlVersion));
            }
            else
            {
                sqlQuery = @"update dbo.TRADEXML set TRADEXML = @TRADEXML where (IDT = @IDT)";
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);
            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
        }

        protected virtual void UpdateRowTradeStSys(DataRow pDataRow, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            Pair<Cst.StatusUsedBy, string> stUsedBy = null;
            if (Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode) || Cst.Capture.IsModeRemoveReplace(pCaptureMode) ||
                (pCaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination) || (Cst.Capture.IsModePositionTransfer(pCaptureMode)))
                stUsedBy = new Pair<Cst.StatusUsedBy, string>(Cst.StatusUsedBy.RESERVED, Cst.ProcessTypeEnum.TRADEACTGEN.ToString() + ":" + pCaptureMode.ToString());
            TradeCommonInput.TradeStatus.UpdateRowTradeStSys(pDataRow, pSessionInfo.user.IdA, pDtSys, stUsedBy);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// FI 20140415 [XXXXX] add Method
        protected virtual void DeleteTradeAsset(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <exception cref="DataHelperException"></exception> 
        protected virtual void DeletePosRequest(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pDateSys"></param>
        /// <exception cref="DataHelperException"></exception> 
        /// EG 20150115 [20683] Override on TradeAdminSaveCaptureGen
        /// FI 20150524 [XXXXX] Modify la méthode n'est plus virtuelle
        /// FI 20160816 [22146] Modification signature add pCaptureSessionInfo and pDateSys
        protected void DeleteEvent(string pCS, IDbTransaction pDbTransaction, int pIdT, CaptureSessionInfo pCaptureSessionInfo, DateTime pDateSys)
        {
            try
            {
                // FI 20150524 [XXXXX] Utilisation de TradeCommonInput.SQLInstrument.GProduct
                // FI 20160816 [22146] passage de Ida et pDateSys 
                TradeRDBMSTools.DeleteEvent(pCS, pDbTransaction, pIdT, this.TradeCommonInput.SQLInstrument.GProduct, pCaptureSessionInfo.user.IdA, pDateSys);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete EVENT for trade (id:{0})", pIdT.ToString()), ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pDateSys"></param>
        /// FI 20160907 [21831] Modify
        protected void DeleteFeeEvent(string pCS, IDbTransaction pDbTransaction, int pIdT, CaptureSessionInfo _1, DateTime _2)
        {
            try
            {
                TradeRDBMSTools.DeleteFeeEvent(pCS, pDbTransaction, pIdT);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete FEE EVENT for trade (id:{0})", pIdT.ToString()), ex);
            }
        }
        /// <summary>
        /// Suppression de tous les événements de frais non facturés du trade
        /// </summary>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected void DeleteFeeEventUninvoiced(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            try
            {
                TradeRDBMSTools.DeleteFeeEventUninvoiced(pCS, pDbTransaction, pIdT);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete FEE EVENT UNINVOICED for trade (id:{0})", pIdT.ToString()), ex);
            }
        }

        /// <summary>
        /// Retourne de la date de sortie d'un trade de marché ETD sur la base de la date de maturité + 2 mois
        /// </summary>
        /// <returns></returns>
        /// FI 20190125 [24474] Add 
        private Nullable<DateTime> GetDtOut(string pCS, IDbTransaction pDbTransaction)
        {
            Nullable<DateTime> ret = null;
            if (TradeCommonInput.Product.IsTradeMarket)
            {
                ProductContainer mainProduct = TradeCommonInput.Product.MainProduct;
                if (mainProduct.IsExchangeTradedDerivative)
                {
                    ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)mainProduct.Product);
                    if (etd.SecurityId > 0)
                    {
                        etd.SetAssetETD(CSTools.SetCacheOn(pCS), pDbTransaction);
                        if (DtFunc.IsDateTimeFilled(etd.MaturityDateSys))
                        {
                            // FI 20190222 [24502] 63 jours à la place de 2 mois 
                            ret = etd.MaturityDateSys.AddDays(63);
                        }
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne une clause where not exists pour éviter l'injection de doublons dans TRADEACTOR
        /// </summary>
        /// <param name="parameterIDA_Actor"></param>
        /// <returns></returns>
        /// FI 20200407 [XXXXX] Add Method 
        private static string GetNotExistsSQLTRADEACTOR(DataParameter parameterIDA_Actor)
        {
            string ret = StrFunc.AppendFormat("where not exists (select 1 from dbo.TRADEACTOR where (IDT=@IDT) and (IDA=@IDA) and (IDROLEACTOR=@IDROLEACTOR) and ({0}))",
                parameterIDA_Actor.Value == Convert.DBNull ? "IDA_ACTOR is null" : "IDA_ACTOR=@IDA_ACTOR");

            return ret;
        }
    }
}



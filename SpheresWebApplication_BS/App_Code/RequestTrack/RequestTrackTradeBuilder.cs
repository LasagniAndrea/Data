using System;
using System.Data;
using System.Collections.Generic;
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EfsML.Interface;
using EfsML.Business;
using FixML.Interface;
using FpML.Interface;


namespace EFS.Common.Log
{
    /// <summary>
    /// Classe chargée d'alimenter l'audit des actions utilisateurs lors d'un consultation de trade
    /// </summary>
    /// FI 20140519 [19923] add class
    /// FI 20141021 [20350] Modify
    public class RequestTrackTradeBuilder : RequestTrackBuilderBase
    {

        /// <summary>
        /// description du trade
        /// </summary>
        public DataDocumentContainer dataDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Identifcation du trade
        /// </summary>
        public SpheresIdentification tradeIdentIdentification
        {
            get;
            set;
        }

        /// <summary>
        /// Action appliquée sur le trade
        /// <para>cette propriété doit être renssignée lorsque l'action utilisateur concerne une action (New,Modify,Remove,exercise,etc...)</para>
        /// </summary>
        /// FI 20141021 [20350] Modify add
        public Nullable<RequestTrackProcessEnum> processType
        {
            get;
            set;
        }

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public RequestTrackTradeBuilder()
        {
            dataDocument = null;
            tradeIdentIdentification = null;
        }
        #endregion

        /// <summary>
        /// Alimentation de la partie data du document
        /// </summary>
        /// FI Initialement j'avais plus ou moins prévu de jouer la requête de la consultation associée avec une restriction sur le trade
        /// Cela aurait permis une alimentation dynamique du log en fonction du paramétrage de l'élémnt RequestTrack présent dans LSTCONSULT.CONSULTXML
        /// Trop difficile à mettre en place et pas le temps....=> j'opte pour un codage en dur pour l'alimentation du datarow
        /// On reviendra peut-être sur ce principe...
        protected override void SetData()
        {


            ProductContainer product = dataDocument.currentProduct;

            List<RequestTrackDataColumn> lstColumn = new List<RequestTrackDataColumn>();
            IParty dealer = null; // Représente acteur lié à l'activité house ou client (Seuls ces acteurs alimentent le log)

            if (product.isExchangeTradedDerivative)
            {
                ExchangeTradedContainer etd = new ExchangeTradedContainer((IExchangeTradedBase)product.product);
                IFixParty dealerFixParty = etd.GetDealer();
                if (null != dealerFixParty)
                    dealer = dataDocument.GetParty(dealerFixParty.PartyId.href);
            }
            else if (product.isMarginRequirement)
            {
                MarginRequirementContainer mgr = new MarginRequirementContainer((IMarginRequirement)product.product);
                foreach (IParty party in dataDocument.party)
                {
                    if (dataDocument.IsPartyMarginRequirementOffice(party))
                    {
                        if (mgr.payment[0].payerPartyReference.hRef == party.id) // MRO est payer du deposit <=> activité Client/House
                        {
                            dealer = party;
                            break;
                        }
                    }
                }
            }
            else if (product.IsCashBalance)
            {
                EfsML.v30.CashBalance.CashBalance cashBalance = (EfsML.v30.CashBalance.CashBalance)product.product;
                foreach (IParty party in dataDocument.party)
                {
                    if (dataDocument.IsPartyCashBalanceOffice(party))
                    {
                        if (cashBalance.cashBalanceStream[0].marginRequirement.payerPartyReference.href == party.id)// CBO est payer du desposit <=> activité Client/House
                        {
                            dealer = party;
                            break;
                        }
                    }
                }
            }
            else if (product.IsCashPayment)
            {
                IBulletPayment bullet = ((IBulletPayment)product.product);
                int idAEntity = dataDocument.GetFirstEntity(CSTools.SetCacheOn(cs));

                IParty payer = dataDocument.GetParty(bullet.payment.payerPartyReference.hRef);
                IParty receiver = dataDocument.GetParty(bullet.payment.receiverPartyReference.hRef);

                IParty party = (payer.OTCmlId == idAEntity) ? receiver : payer;
                // L'acteur qui n'est pas entity et  n'a pas le rôle CLEARER, ou COMPART,...
                if (false == ActorTools.IsActorWithRole(cs, party.OTCmlId, new RoleActor[] { RoleActor.CLEARER, RoleActor.CCLEARINGCOMPART, RoleActor.HCLEARINGCOMPART, RoleActor.MCLEARINGCOMPART }))
                    dealer = party;

            }
            else if (product.IsCashBalanceInterest)
            {
                ICashBalanceInterest cbi = (ICashBalanceInterest)product.product;

                IInterestRateStream irs = cbi.stream[0];

                IParty payer = dataDocument.GetParty(irs.payerPartyReference.hRef);
                IParty receiver = dataDocument.GetParty(irs.receiverPartyReference.hRef);
                IParty party = dataDocument.IsPartyEntity(payer) ? receiver : payer;

                // L'acteur qui n'est pas entity est forcement un client ou un House (règle mise en place en s'appuyant sur la requête de consultation)
                dealer = party;
            }
            else
            {
                // TODO
            }


            if (null != dealer)
            {
                int idA = dealer.OTCmlId;
                IBookId bookId = dataDocument.GetBookId(dealer.id);
                int idB = bookId.OTCmlId;
                string grp = ActorTools.IsActorWithRole(CSTools.SetCacheOn(cs), idA, RoleActor.CLIENT) ? "Client" : "House";

                lstColumn.Add(new RequestTrackDataColumn
                {
                    columnIdA = "IDA_DEALER",
                    columnIdB = "IDB_DEALER",
                    columnGrp = "GRP_DEALER"
                });

                DataTable dt = CreateDataTable(lstColumn);
                dt.Rows.Add(grp, idA, idB);

                DataRow[] row = new DataRow[dt.Rows.Count];
                dt.Rows.CopyTo(row, 0);

                SetDataFromDataRow(lstColumn, row);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void SetActionDetail()
        {
            RequestTrackActionDetailBase detail = null;

            switch (this.action.First)
            {
                case RequestTrackActionEnum.ItemLoad:
                    detail = new RequestTrackItemLoadDetail();
                    break;
                case RequestTrackActionEnum.ItemProcess:
                    if (null == this.processType)
                        throw new NullReferenceException("actionType must be assigned on ItemAction");

                    detail = new RequestTrackItemProcessDetail();
                    ((RequestTrackItemProcessDetail)detail).type = processType.Value;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RequestTrackAction {0} is not implemented", this.action.First.ToString()));
            }

            IRequestTrackItem requestTrackItem = detail as IRequestTrackItem;
            if (null == requestTrackItem)
                throw new NullReferenceException("detail doesn't implemente IRequestTrackItem");

            InitRequestTrackItem(requestTrackItem);

            doc.action.detail = detail;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void InitRequestTrackItem(IRequestTrackItem pItemContainer)
        {

            string type = string.Empty;
            ProductContainer product = dataDocument.currentProduct;
            if (product.isMarginRequirement)
            {
                type = "MarginRequirement";
            }
            else if (product.IsCashBalance)
            {
                type = "CashBalance";
            }
            else if (product.IsCashPayment)
            {
                type = "CashPayment";
            }
            else if (product.IsCashBalanceInterest)
            {
                type = "CashBalanceInterest";
            }
            else
                type = "Trade";

            pItemContainer.Item.type = type;

            pItemContainer.Item.id = tradeIdentIdentification.OTCmlId;
            pItemContainer.Item.idSpecified = true;
            pItemContainer.Item.identifier = tradeIdentIdentification.identifier;
            pItemContainer.Item.displayName = tradeIdentIdentification.displayname;

            pItemContainer.Item.descriptionSpecified = StrFunc.IsFilled(tradeIdentIdentification.description);
            if (pItemContainer.Item.descriptionSpecified)
                pItemContainer.Item.description = tradeIdentIdentification.description;

            pItemContainer.Item.extl1Specified = StrFunc.IsFilled(tradeIdentIdentification.extllink);
            if (pItemContainer.Item.extl1Specified)
                pItemContainer.Item.extl1 = tradeIdentIdentification.extllink;
        }


        /// <summary>
        /// retourne un datatable à partir des colonnes {lstcolumn}
        /// </summary>
        /// <param name="lstcolumn"></param>
        /// <returns></returns>
        private static DataTable CreateDataTable(List<RequestTrackDataColumn> lstcolumn)
        {
            DataTable ret = new DataTable("DATA");
            foreach (RequestTrackDataColumn col in lstcolumn)
            {
                ret.Columns.Add(col.columnGrp, System.Type.GetType("System.String"));
                ret.Columns.Add(col.columnIdA, System.Type.GetType("System.Int32"));
                ret.Columns.Add(col.columnIdB, System.Type.GetType("System.Int32"));
            }
            return ret;
        }
    }
}
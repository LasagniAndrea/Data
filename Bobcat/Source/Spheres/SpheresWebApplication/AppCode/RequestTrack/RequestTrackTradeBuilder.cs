using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EfsML.Business;
using EfsML.Interface;
using FixML.Interface;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;


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
        public DataDocumentContainer DataDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Identifcation du trade
        /// </summary>
        public SpheresIdentification TradeIdentIdentification
        {
            get;
            set;
        }

        /// <summary>
        /// Action appliquée sur le trade
        /// <para>cette propriété doit être renssignée lorsque l'action utilisateur concerne une action (New,Modify,Remove,exercise,etc...)</para>
        /// </summary>
        /// FI 20141021 [20350] Modify add
        public Nullable<RequestTrackProcessEnum> ProcessType
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
            DataDocument = null;
            TradeIdentIdentification = null;
        }
        #endregion

        /// <summary>
        /// Alimentation de la partie data du document
        /// </summary>
        /// FI Initialement j'avais plus ou moins prévu de jouer la requête de la consultation associée avec une restriction sur le trade
        /// Cela aurait permis une alimentation dynamique du log en fonction du paramétrage de l'élémnt RequestTrack présent dans LSTCONSULT.CONSULTXML
        /// Trop difficile à mettre en place et pas le temps....=> j'opte pour un codage en dur pour l'alimentation du datarow
        /// On reviendra peut-être sur ce principe...
        // EG 20230505 [XXXXX] [WI617] dealer optional => controls for Trade template
        protected override void SetData()
        {
            ProductContainer product = DataDocument.CurrentProduct;
            RptSideProductContainer productRptSide = product.RptSide();

            List<RequestTrackDataColumn> lstColumn = new List<RequestTrackDataColumn>();
            IParty dealer = null; // Représente acteur lié à l'activité house ou client (Seuls ces acteurs alimentent le log)

            if (null != productRptSide)
            {
                IFixParty dealerFixParty = productRptSide.GetDealer();
                if (null != dealerFixParty)
                    dealer = DataDocument.GetParty(dealerFixParty.PartyId.href);
            }
            else if (product.IsMarginRequirement)
            {
                MarginRequirementContainer mgr = new MarginRequirementContainer((IMarginRequirement)product.Product);
                foreach (IParty party in DataDocument.Party)
                {
                    if (DataDocument.IsPartyMarginRequirementOffice(party))
                    {
                        if (mgr.Payment[0].PayerPartyReference.HRef == party.Id) // MRO est payer du deposit <=> activité Client/House
                        {
                            dealer = party;
                            break;
                        }
                    }
                }
            }
            else if (product.IsCashBalance)
            {
                EfsML.v30.CashBalance.CashBalance cashBalance = (EfsML.v30.CashBalance.CashBalance)product.Product;
                foreach (IParty party in DataDocument.Party)
                {
                    if (DataDocument.IsPartyCashBalanceOffice(party))
                    {
                        if (cashBalance.cashBalanceStream[0].marginRequirement.payerPartyReference.href == party.Id)// CBO est payer du desposit <=> activité Client/House
                        {
                            dealer = party;
                            break;
                        }
                    }
                }
            }
            else if (product.IsCashPayment)
            {
                IBulletPayment bullet = ((IBulletPayment)product.Product);
                int idAEntity = DataDocument.GetFirstEntity(CSTools.SetCacheOn(Cs));

                IParty payer = DataDocument.GetParty(bullet.Payment.PayerPartyReference.HRef);
                IParty receiver = DataDocument.GetParty(bullet.Payment.ReceiverPartyReference.HRef);

                IParty party = (payer.OTCmlId == idAEntity) ? receiver : payer;
                // L'acteur qui n'est pas entity et  n'a pas le rôle CLEARER, ou COMPART,...
                if (false == ActorTools.IsActorWithRole(Cs, party.OTCmlId, new RoleActor[] { RoleActor.CLEARER, RoleActor.CCLEARINGCOMPART, RoleActor.HCLEARINGCOMPART, RoleActor.MCLEARINGCOMPART }))
                    dealer = party;

            }
            else if (product.IsCashBalanceInterest)
            {
                ICashBalanceInterest cbi = (ICashBalanceInterest)product.Product;

                IInterestRateStream irs = cbi.Stream[0];

                IParty payer = DataDocument.GetParty(irs.PayerPartyReference.HRef);
                IParty receiver = DataDocument.GetParty(irs.ReceiverPartyReference.HRef);
                IParty party = DataDocument.IsPartyEntity(payer) ? receiver : payer;

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
                // EG 20230505 [XXXXX] [WI617] optional => controls for Trade template
                if (0 < idA)
                {
                    IBookId bookId = DataDocument.GetBookId(dealer.Id);
                    int idB = bookId.OTCmlId;
                    string grp = ActorTools.IsActorWithRole(CSTools.SetCacheOn(Cs), idA, RoleActor.CLIENT) ? "Client" : "House";

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
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void SetActionDetail()
        {
            RequestTrackActionDetailBase detail;
            switch (this.action.First)
            {
                case RequestTrackActionEnum.ItemLoad:
                    detail = new RequestTrackItemLoadDetail();
                    break;
                case RequestTrackActionEnum.ItemProcess:
                    if (null == this.ProcessType)
                        throw new NullReferenceException("actionType must be assigned on ItemAction");

                    detail = new RequestTrackItemProcessDetail();
                    ((RequestTrackItemProcessDetail)detail).type = ProcessType.Value;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RequestTrackAction {0} is not implemented", this.action.First.ToString()));
            }

            if (!(detail is IRequestTrackItem requestTrackItem))
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
            ProductContainer product = DataDocument.CurrentProduct;

            string type;
            if (product.IsMarginRequirement)
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

            pItemContainer.Item.id = TradeIdentIdentification.OTCmlId;
            pItemContainer.Item.idSpecified = true;
            pItemContainer.Item.identifier = TradeIdentIdentification.Identifier;
            pItemContainer.Item.displayName = TradeIdentIdentification.Displayname;

            pItemContainer.Item.descriptionSpecified = StrFunc.IsFilled(TradeIdentIdentification.Description);
            if (pItemContainer.Item.descriptionSpecified)
                pItemContainer.Item.description = TradeIdentIdentification.Description;

            pItemContainer.Item.extl1Specified = StrFunc.IsFilled(TradeIdentIdentification.Extllink);
            if (pItemContainer.Item.extl1Specified)
                pItemContainer.Item.extl1 = TradeIdentIdentification.Extllink;
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
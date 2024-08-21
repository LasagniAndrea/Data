#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives


namespace EfsML.Business
{
    #region RptSideContainer
    /// <summary>
    /// Représente les produits qui possède un IFixTrdCapRptSideGrp (ETD / ESE / RTS / SPOTCOMMODITY)
    /// </summary>
    public abstract class RptSideProductContainer : ProductContainer
    {

        #region Accessors


        /// <summary>
        /// Obtient le buyer
        /// </summary>
        public virtual string BuyerPartyReference
        {
            get { return null; }
        }
        /// <summary>
        /// Obtient le seller
        /// </summary>
        public virtual string SellerPartyReference
        {
            get { return null; }
        }



        /// <summary>
        ///  Obtient la dateBusinessDate
        /// </summary>
        /// EG 20171016 [23509] Upd
        // EG 20180423 Analyse du code Correction [CA1065]
        public virtual DateTime ClearingBusinessDate
        {
            get
            {
                if (null == DataDocument)
                    throw new InvalidOperationException("dataDocument is null, Please call SetDataDocument Method");

                //DateTime _date = dataDocument.tradeHeader.tradeDate.BusinessDate;
                //if (DtFunc.IsDateTimeEmpty(_date))
                //    _date = dataDocument.tradeHeader.tradeDate.DateValue;
                DateTime _date = DateTime.MinValue;
                if (DataDocument.TradeHeader.ClearedDateSpecified)
                    _date = DataDocument.TradeHeader.ClearedDate.DateValue;
                if (DtFunc.IsDateTimeEmpty(_date))
                    _date = DataDocument.TradeHeader.TradeDate.DateValue;
                return _date;
            }
        }

        
        /// <summary>
        /// Retourne le FIXPartyRole en fontion du produit 
        /// <para>CLEARER = ExchangeTradedDerivative ou CommoditySpot </para>
        /// <para>CUSTODIAN = EquitySecurityTransaction ou ReturnSwap ou debtSecurityTransaction ou ...</para>
        /// </summary>
        /// EG 20140911 Test sur EquitySecurityTransaction
        /// FI 20161214 [21916] Modify
        private PartyRoleEnum ClearingFirmOrCustodian
        {
            get
            {
                PartyRoleEnum _partyRole;
                if ((Product is IExchangeTradedDerivative) || (Product is ICommoditySpot)) // FI 20161214 [21916] GLOP (Il faut trouver mieux que le l'opérateur OR)
                    _partyRole = PartyRoleEnum.ClearingFirm;
                else
                    _partyRole = PartyRoleEnum.Custodian;

                return _partyRole;
            }
        }
        

        #region IdA_Custodian
        // EG 20171031 [23509] Upd
        public virtual Nullable<int> IdA_Custodian
        {
            get
            {
                Nullable<int> ret = null;
                IFixParty custodian = GetCustodian();
                if (null != custodian)
                {
                    IParty _custodianParty = DataDocument.GetParty(custodian.PartyId.href);
                    if (null != _custodianParty)
                        ret = _custodianParty.OTCmlId;
                }
                return ret;
            }
        }
        #endregion IdA_Custodian

        
        
        
        /// <summary>
        /// Obtient true s'il n'existe qu'un TrdCapRptSideGrp
        /// <para>Cas des allocations</para>
        /// </summary>
        public bool IsOneSide
        {
            get { return (1 == ArrFunc.Count(RptSide)); }
        }
        /// <summary>
        /// Obtient true s'il n'existe au minimum 2 TrdCapRptSideGrp
        /// <para>Cas des executions ou des intermediations</para>
        /// </summary>
        public bool IsTwoSide
        {
            get { return (1 < ArrFunc.Count(RptSide)); }
        }

        /// <summary>
        /// Obtient l'objet Parent qui contient l'élément RptSide
        /// </summary>
        public virtual object Parent
        {
            get { throw new NotSupportedException("Parent property must be override"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual decimal Qty
        {
            get { return 0; }
        }
        
        
        /// <summary>
        /// Obtient l'élément RptSide existant sur le produit
        /// </summary>
        public new virtual IFixTrdCapRptSideGrp[] RptSide
        {
            get { throw new NotSupportedException("RptSide property must be override"); }
        }
        
        #endregion Accessors

        #region Constructors
        public RptSideProductContainer(object pProduct)
            : base((IProduct)pProduct)
        {

        }
        public RptSideProductContainer(object pProduct, DataDocumentContainer pDataDocumentContainer)
            : base((IProduct)pProduct, pDataDocumentContainer)
        {
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public virtual void ClearRptSide()
        {
        }

        /// <summary>
        /// Obtient le TrdCapRptSideGrp tel que side = {pSide}
        /// <para>Obtient le TrdCapRptSideGrp tel que SideSpecified = false si null == {pSide}</para>
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <param name="pSide">Sens</param>
        /// <returns></returns>
        public IFixTrdCapRptSideGrp GetTrdCapRptSideGrp(Nullable<SideEnum> pSide)
        {

            IFixTrdCapRptSideGrp ret = null;
            IFixTrdCapRptSideGrp[] _rptSide = RptSide;
            for (int i = 0; i < ArrFunc.Count(_rptSide); i++)
            {
                if (pSide.HasValue)
                {
                    if (_rptSide[i].SideSpecified && (_rptSide[i].Side == pSide.Value))
                        ret = _rptSide[i];
                }
                else
                {
                    if (false == _rptSide[i].SideSpecified)
                        ret = _rptSide[i];
                }
                if (null != ret)
                    break;
            }
            return ret;

        }

        /// <summary>
        /// <para>Allocation: Obtient le D.O. (si le sens qui lui est rattaché vaut pSide), la ClearingOrganization ou le Clearer|Custodian sinon</para>
        /// <para>Execution, Intermediation: Obtient la party rattachée au sens pSide</para>
        /// <para>Obtient null si non trouvé</para>
        /// <para>Cette méthode exploite la property RptSide</para>
        /// </summary>
        /// <param name="pSide"></param>
        /// <returns></returns>
        public IFixParty GetBuyerSeller(SideEnum pSide)
        {

            IFixParty ret = null;
            IFixTrdCapRptSideGrp[] _rptSide = RptSide;
            if (ArrFunc.IsFilled(_rptSide))
            {
                if (IsOneSide)
                {
                    if (_rptSide[0].Side == pSide)
                    {
                        ret = RptSideTools.GetParty(_rptSide[0], PartyRoleEnum.BuyerSellerReceiverDeliverer);
                    }
                    else
                    {
                        ret = RptSideTools.GetParty(_rptSide[0], PartyRoleEnum.ClearingOrganization);
                        if (null == ret)
                            ret = RptSideTools.GetParty(_rptSide[0], ClearingFirmOrCustodian);
                    }
                }
                else if (IsTwoSide)
                {
                    for (int i = 0; i < ArrFunc.Count(_rptSide); i++)
                    {
                        if (_rptSide[i].Side == pSide)
                        {
                            ret = RptSideTools.GetParty(_rptSide[i], PartyRoleEnum.BuyerSellerReceiverDeliverer);
                            if (null != ret)
                                break;
                        }
                    }
                }
            }
            return ret;
        }

        #region GetBuyerSellerPartyRole
        /// <summary>
        /// Retourne le rôle du Buyer|Seller
        /// </summary>
        /// <param name="pSide">Buy|Sell</param>
        /// <returns>PartyRoleEnum (EnumAttribute)</returns>
        public string GetBuyerSellerPartyRole(SideEnum pSide)
        {
            string ret = string.Empty;
            IFixParty buyerSeller = GetBuyerSeller(pSide);
            if ((null != buyerSeller) && buyerSeller.PartyRoleSpecified)
                ret = ReflectionTools.ConvertEnumToString<PartyRoleEnum>(buyerSeller.PartyRole);
            return ret;
        }
        #endregion GetBuyerSellerPartyRole

        /// <summary>
        /// Retourne le Dealer de l'opération (Allocation)
        /// </summary>
        /// <returns></returns>
        public IFixParty GetDealer()
        {
            IFixParty ret = null;
            if (IsOneSide)
            {
                IFixTrdCapRptSideGrp[] _rptSide = RptSide;
                if (_rptSide[0].SideSpecified)
                    ret = GetBuyerSeller(_rptSide[0].Side);
            }
            return ret;
        }

        /// <summary>
        /// Retourne le {CSS|Clearer}|Custodian 
        /// </summary>
        /// <returns></returns>
        public IFixParty GetClearerCustodian()
        {
            IFixParty ret = null;
            if (IsOneSide)
            {
                IFixTrdCapRptSideGrp[] _rptSide = RptSide;
                if (_rptSide[0].SideSpecified)
                {
                    if (_rptSide[0].Side == SideEnum.Sell)
                        ret = GetBuyerSeller(SideEnum.Buy);
                    else if (_rptSide[0].Side == SideEnum.Buy)
                        ret = GetBuyerSeller(SideEnum.Sell);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le Custodian 
        /// </summary>
        /// <returns></returns>
        // EG 20171031 [23509] New
        public IFixParty GetCustodian()
        {
            IFixParty ret = GetClearerCustodian();
            if (null != ret)
            {
                if ((false == ret.PartyRoleSpecified) || (ret.PartyRole != PartyRoleEnum.Custodian))
                    ret = null;
            }
            return ret;
        }

        /// <summary>
        /// Retourne le sens associé au dealer 
        /// </summary>
        /// <returns></returns>
        public Nullable<SideEnum> GetSideDealer()
        {
            Nullable<SideEnum> ret = null;
            if (IsOneSide)
            {
                IFixTrdCapRptSideGrp _rptSide = RptSide[0];
                if (_rptSide.SideSpecified)
                    ret = _rptSide.Side;
            }
            else
            {
#if DEBUG
                throw new NotSupportedException(StrFunc.AppendFormat("Method {0} is available on allocation only", "GetSideDealer"));
#endif
            }
            return ret;
        }

        /// <summary>
        /// Retourne le vecteur (interface IFixTrdCapRptSideGrp) en fontion du produit
        /// </summary>
        public IFixTrdCapRptSideGrp GetRptSide(int pIndex)
        {
            IFixTrdCapRptSideGrp _rptSide = null;
            if (ArrFunc.IsFilled(RptSide) && (pIndex <= RptSide.Length))
                _rptSide = RptSide[pIndex];
            return _rptSide;
        }

        /// <summary>
        /// Mise jour de rptSide (NB: objet non sérialisé.)
        /// <para>Utilisation réservée exclusivement à la mise à jour des tables: TRADEINSTRUMENT (IDA_DEALER|IDB_DEALER|IDA_CLEARER...) / TRADE ACTOR (FIXPARTYROLE)</para>
        /// <para>Allocation : 1 TrdCapRptSideGrp</para>
        /// <para>Execution ou intermediation : 2 TrdCapRptSideGrp</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsAllocation"></param>
        /// FI 20161214 [21916] Modify 
        // EG 20180205 [23769] Add dbTransaction  
        public void InitRptSide(string pCS, bool pIsAllocation)
        {
            InitRptSide(pCS, null, pIsAllocation);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public void InitRptSide(string pCS, IDbTransaction pDbTransaction, bool pIsAllocation)
        {
            // FI 20161214 [21916] Add NotSupportedException
            // Cette méthode ne doit pas être appelée sur des produits où l'élément RptSide est serialized
            if (this.GetType().Equals(typeof(ExchangeTradedContainer)) || this.GetType().Equals(typeof(ICommoditySpot)))
                throw new NotSupportedException(StrFunc.AppendFormat("Method is not supported with type (Id: {0}) ", this.GetType().ToString()));

            if (null == DataDocument)
                throw new NullReferenceException("dataDocument is null");

            string _Cs = CSTools.SetCacheOn(pCS);
            RoleActor[] _roleActor = new RoleActor[] { RoleActor.CSS, RoleActor.CUSTODIAN };
            RoleActor[] _roleActorClearingOrganization = new RoleActor[] { RoleActor.CSS };
            RoleActor[] _roleActorCustodian = new RoleActor[] { RoleActor.CUSTODIAN };

            IParty partybuyer = null;
            if (StrFunc.IsFilled(BuyerPartyReference))
                partybuyer = DataDocument.GetParty(BuyerPartyReference, PartyInfoEnum.id);

            IParty partyseller = null;
            if (StrFunc.IsFilled(SellerPartyReference))
                partyseller = DataDocument.GetParty(SellerPartyReference, PartyInfoEnum.id);

            ClearRptSide();

            if (pIsAllocation)
            {
                #region Allocation (1 TrdCapRptSideGrp)
                ReflectionTools.AddItemInArray(Parent, "RptSide", 0);
                bool isBuyerWithRole = false;
                if (null != partybuyer)
                    isBuyerWithRole = ActorTools.IsActorWithRole(_Cs, pDbTransaction, partybuyer.OTCmlId, _roleActor);

                bool isSellerWithRole = false;
                if (null != partyseller)
                    isSellerWithRole = ActorTools.IsActorWithRole(_Cs, pDbTransaction, partyseller.OTCmlId, _roleActor);

                #region Cas particulier où les 2 parties ont le rôle CUSTODIAN
                //On privilégie, s'il existe l'acteur qui n'est pas ENTITY, DEPARTMENT ou DESK. Sinon... (TBD)
                if (isBuyerWithRole && isSellerWithRole)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        RoleActor[] _roleActorEntity = null;
                        switch (i)
                        {
                            case 1:
                                _roleActorEntity = new RoleActor[] { RoleActor.ENTITY, RoleActor.DEPARTMENT, RoleActor.DESK };
                                break;
                            case 2:
                                _roleActorEntity = new RoleActor[] { RoleActor.ENTITY, RoleActor.DEPARTMENT };
                                break;
                            case 3:
                                _roleActorEntity = new RoleActor[] { RoleActor.ENTITY };
                                break;
                        }
                        bool isBuyerWithEntity = ActorTools.IsActorWithRole(_Cs, pDbTransaction, partybuyer.OTCmlId, _roleActorEntity);
                        bool isSellerWithEntity = ActorTools.IsActorWithRole(_Cs, pDbTransaction, partyseller.OTCmlId, _roleActorEntity);
                        if (isBuyerWithEntity && !isSellerWithEntity)
                        {
                            isBuyerWithRole = false;
                            break;
                        }
                        else if (isSellerWithEntity && !isBuyerWithEntity)
                        {
                            isSellerWithRole = false;
                            break;
                        }
                    }
                    if (isBuyerWithRole && isSellerWithRole)
                    {
                        //TBD 
                    }
                }
                #endregion

                if (false == isBuyerWithRole)
                {
                    if (null != partybuyer)
                        SetBuyerSeller(partybuyer.Id, SideEnum.Buy, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                    if (isSellerWithRole)
                    {
                        if (null != partyseller)
                        {
                            if (ActorTools.IsActorWithRole(_Cs, pDbTransaction, partyseller.OTCmlId, _roleActorClearingOrganization))
                                SetBuyerSeller(partyseller.Id, SideEnum.Sell, PartyRoleEnum.ClearingOrganization);
                            else if (ActorTools.IsActorWithRole(_Cs, pDbTransaction, partyseller.OTCmlId, _roleActorCustodian))
                                SetBuyerSeller(partyseller.Id, SideEnum.Sell, PartyRoleEnum.Custodian);
                        }
                    }
                }
                else if (false == isSellerWithRole)
                {
                    if (null != partyseller)
                        SetBuyerSeller(partyseller.Id, SideEnum.Sell, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                    if (isBuyerWithRole)
                    {
                        if (null != partybuyer)
                        {
                            if (ActorTools.IsActorWithRole(_Cs, pDbTransaction, partybuyer.OTCmlId, _roleActorClearingOrganization))
                                SetBuyerSeller(partybuyer.Id, SideEnum.Buy, PartyRoleEnum.ClearingOrganization);
                            else if (ActorTools.IsActorWithRole(_Cs, pDbTransaction, partybuyer.OTCmlId, _roleActorCustodian))
                                SetBuyerSeller(partybuyer.Id, SideEnum.Buy, PartyRoleEnum.Custodian);
                        }
                    }
                }
                #endregion Allocation
            }
            else
            {
                #region Execution ou Intermediation (2 TrdCapRptSideGrp)
                ReflectionTools.AddItemInArray(Parent, "RptSide", 0);
                IFixTrdCapRptSideGrp trdCapRptSide = GetRptSide(0);
                trdCapRptSide.Side = FixML.Enum.SideEnum.Buy;
                trdCapRptSide.SideSpecified = true;
                if (null != partybuyer)
                    SetBuyerSeller(partybuyer.Id, SideEnum.Buy, PartyRoleEnum.BuyerSellerReceiverDeliverer);

                ReflectionTools.AddItemInArray(Parent, "RptSide", 0);
                trdCapRptSide = GetRptSide(1);
                trdCapRptSide.Side = FixML.Enum.SideEnum.Sell;
                trdCapRptSide.SideSpecified = true;
                if (null != partyseller)
                    SetBuyerSeller(partyseller.Id, SideEnum.Sell, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                #endregion Execution, Intermediation
            }

        }

        /// <summary>
        /// Allocation : Retourne true si le dealer est un acheteur (pSide = BUYER) ou vendeur (pSide = SELLER)
        /// </summary>
        /// <param name="pSide">BUYER|SELLER</param>
        /// <returns>true|false</returns>
        public bool IsDealerBuyerOrSeller(BuyerSellerEnum pSide)
        {
            bool ret = false;
            IFixParty party = null;
            if (pSide == BuyerSellerEnum.BUYER)
                party = GetBuyerSeller(SideEnum.Buy);
            else if (pSide == BuyerSellerEnum.SELLER)
                party = GetBuyerSeller(SideEnum.Sell);

            if ((null != party) && party.PartyIdSourceSpecified)
            {
                IFixParty dealer = GetDealer();
                if ((null != dealer) && dealer.PartyIdSourceSpecified)
                    ret = (dealer.PartyId.href == party.PartyId.href);
            }
            return ret;
        }

        /// <summary>
        /// Get the position keeping activation status of a book
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pParty">FIX Party</param>
        /// <returns>true when the current trade is on a ETD/ESE/RTS product and the book has position keeping activated, false in any other case</returns>
        /// FI 20141218 [XXXXX] suppression du paramètre  pDataDoc de type dataDocumentContainer
        // Eg 20150706 [21021] Nullable<int> idB
        // EG 20180307 [23769] Gestion dbTransaction
        public bool IsPosKeepingOnBook(string pCS, IFixParty pParty)
        {
            return IsPosKeepingOnBook(pCS, null, pParty);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public bool IsPosKeepingOnBook(string pCS, IDbTransaction pDbTransaction, IFixParty pParty)
        {
            if (null == DataDocument)
                throw new NullReferenceException("dataDocument is null");

            bool ret = false;
            if (null != pParty)
            {
                Nullable<int> idB = DataDocument.GetOTCmlId_Book(pParty.PartyId.href);
                if (idB.HasValue)
                    ret = BookTools.IsBookPosKeeping(pCS, pDbTransaction, idB);
            }
            return ret;
        }

        /// <summary>
        /// Get the position keeping activation status of the dealer
        /// </summary>
        /// <returns>true when the current trade is on a ETD/ESE/RTS product and the book has position keeping activated, false in any other case</returns>
        /// FI 20141218 [XXXXX] suppression du paramètre  pDataDoc de type dataDocumentContainer
        // EG 20180307 [23769] Gestion dbTransaction
        public bool IsPosKeepingOnBookDealer(string pCS)
        {
            return IsPosKeepingOnBookDealer(pCS, null);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public bool IsPosKeepingOnBookDealer(string pCS, IDbTransaction pDbTransaction)
        {
            IFixParty party = GetDealer();
            return IsPosKeepingOnBook(pCS, pDbTransaction, party);
        }

        /// <summary>
        /// Get the position keeping activation status of the clearer
        /// </summary>
        /// <param name="pCS">the connection string</param>
        /// <returns>true when the curren trade is on a ETD product and the clearer book has position keeping activated, false in 
        /// any other case</returns>
        /// FI 20141218 [XXXXX] suppression du paramètre  pDataDoc de type dataDocumentContainer
        public bool IsPosKeepingOnBookClearerCustodian(string pCS, IDbTransaction pDbTransaction)
        {
            IFixParty party = GetClearerCustodian();
            return IsPosKeepingOnBook(pCS, pDbTransaction, party);
        }

        /// <summary>
        /// Inverse le sens de chaque RptSide
        /// </summary>
        public void ReverseSide()
        {
            IFixTrdCapRptSideGrp[] _rptSide = RptSide;
            for (int i = 0; i < ArrFunc.Count(_rptSide); i++)
            {
                SideEnum currentSide = _rptSide[i].Side;
                if (SideEnum.Buy == currentSide)
                    _rptSide[i].Side = SideEnum.Sell;
                else if (SideEnum.Sell == currentSide)
                    _rptSide[i].Side = SideEnum.Buy;
            }
        }

        /// <summary>
        /// Définit l'acheteur/vendeur et son rôle, alimentation de TrdCapRptSideGrp
        /// <para>Comportement différent si allocation ou execution, intermédiation</para>
        /// <para>Si execution,intermédiation seul le role BuyerSellerReceiverDeliverer est accepté</para>
        /// </summary>
        /// <param name="pId">Id de la partie</param>
        /// <param name="pSide">Buy|Sell</param>
        /// <param name="pPartyRole">BuyerSellerReceiverDeliverer|ClearingOrganization|ClearingFirm|Custodian</param>
        public void SetBuyerSeller(string pId, SideEnum pSide, PartyRoleEnum pPartyRole)
        {
            IFixTrdCapRptSideGrp[] _rptSide = RptSide;
            if (IsOneSide)
            {
                #region Allocation
                if (pPartyRole == PartyRoleEnum.BuyerSellerReceiverDeliverer)
                {
                    _rptSide[0].Side = pSide;
                    _rptSide[0].SideSpecified = true;
                    IFixParty party = RptSideTools.GetParty(_rptSide[0], pPartyRole);
                    if (null == party)
                        party = RptSideTools.AddParty(_rptSide[0]);
                    RptSideTools.SetParty(party, pId, pPartyRole);
                }
                else if ((pPartyRole == ClearingFirmOrCustodian) || (pPartyRole == PartyRoleEnum.ClearingOrganization))
                {
                    IFixParty party = RptSideTools.GetParty(_rptSide[0], PartyRoleEnum.ClearingOrganization);
                    if (null == party)
                        party = RptSideTools.GetParty(_rptSide[0], ClearingFirmOrCustodian);
                    if (null == party)
                        party = RptSideTools.AddParty(_rptSide[0]);
                    RptSideTools.SetParty(party, pId, pPartyRole);
                }
                #endregion Allocation
            }
            else if (IsTwoSide)
            {
                #region Execution / Intermédiation
                if (pPartyRole == PartyRoleEnum.BuyerSellerReceiverDeliverer)
                {
                    IFixTrdCapRptSideGrp trdCapRptSideGrpItem = null;
                    for (int i = 0; i < ArrFunc.Count(_rptSide); i++)
                    {
                        if (_rptSide[i].SideSpecified && (_rptSide[i].Side == pSide))
                        {
                            trdCapRptSideGrpItem = _rptSide[i];
                            break;
                        }
                    }
                    if (null != trdCapRptSideGrpItem)
                    {
                        IFixParty party = RptSideTools.GetParty(trdCapRptSideGrpItem, pPartyRole);
                        if (null == party)
                            party = RptSideTools.AddParty(trdCapRptSideGrpItem);
                        RptSideTools.SetParty(party, pId, pPartyRole);
                    }
                }
                #endregion Execution / Intermédiation
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataParameters"></param>
        /// EG 20150706 [21021] Nullable
        /// FI 20160810 [22086] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        public override void SetTradeInstrumentParameters(string pCS, IDbTransaction pDbTransaction, DataParameters pDataParameters)
        {
            
            if (IsOneSide)
            {
                Nullable<int> idB;

                #region Allocation
                SideEnum dealerSide = (IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Buy : SideEnum.Sell);
                IFixParty dealerFixParty = GetDealer();
                if (null != dealerFixParty)
                {
                    IParty _dealerParty = DataDocument.GetParty(dealerFixParty.PartyId.href);
                    // FI 20160810 [22086] test suplémentaire sur OTCmlId > 0 car sur un trade incomplet sans dealer on peut avoir OTCmlId = 0
                    if (null != _dealerParty && _dealerParty.OTCmlId > 0)
                    {
                        pDataParameters["IDA_DEALER"].Value = _dealerParty.OTCmlId;
                        idB = DataDocument.GetOTCmlId_Book(dealerFixParty.PartyId.href);
                        // FI 20160810 [22086] test suplémentaire sur  idB.Value > 0
                        pDataParameters["IDB_DEALER"].Value = (idB.HasValue && idB.Value > 0) ? idB.Value : Convert.DBNull;
                        if (idB.HasValue && idB.Value > 0)
                        {
                            int idAEntity = BookTools.GetEntityBook(pCS, pDbTransaction, idB);
                            if (idAEntity > 0)
                                pDataParameters["IDA_ENTITY"].Value = idAEntity;
                        }
                    }
                }
                IFixParty _clearerFixParty = GetClearerCustodian();
                if (null != _clearerFixParty)
                {
                    IParty _clearerParty = DataDocument.GetParty(_clearerFixParty.PartyId.href);
                    // FI 20160810 [22086] test suplémentaire sur OTCmlId > 0 car sur un trade incomplet sans clearer on peut avoir OTCmlId = 0
                    if (null != _clearerParty && _clearerParty.OTCmlId > 0)
                    {
                        pDataParameters["IDA_CLEARER"].Value = _clearerParty.OTCmlId;
                        idB = DataDocument.GetOTCmlId_Book(_clearerFixParty.PartyId.href);
                        // FI 20160810 [22086] test suplémentaire sur  idB.Value > 0
                        pDataParameters["IDB_CLEARER"].Value = (idB.HasValue && idB.Value > 0) ? idB.Value : Convert.DBNull;
                    }
                }

                pDataParameters["SIDE"].Value = ReflectionTools.ConvertEnumToString<SideEnum>(dealerSide);

                #endregion Allocation
            }
        }
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public override void SetTradeInstrumentToDataRow(string pCS, IDbTransaction pDbTransaction, DataRow pDataRow)
        {
            TradeInstrumentData ret = SetTradeInstrumentData(pCS, pDbTransaction);
            pDataRow["IDA_DEALER"] = (null != ret) ? ret.IdA_Dealer : Convert.DBNull;
            pDataRow["IDB_DEALER"] = (null != ret) && ret.IdB_Dealer.HasValue && (ret.IdB_Dealer.Value > 0) ? ret.IdB_Dealer.Value : Convert.DBNull;
            pDataRow["IDA_CLEARER"] = (null != ret) ? ret.IdA_Clearer : Convert.DBNull;
            pDataRow["IDB_CLEARER"] = (null != ret) && ret.IdB_Clearer.HasValue && (ret.IdB_Clearer.Value > 0) ? ret.IdB_Clearer.Value : Convert.DBNull;
            pDataRow["IDA_ENTITY"] = (null != ret) && (ret.IdA_Entity > 0) ? ret.IdA_Entity : Convert.DBNull;
            pDataRow["SIDE"] = (null != ret) ? ReflectionTools.ConvertEnumToString<SideEnum>(ret.Side) : Convert.DBNull;
        }

        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private class TradeInstrumentData
        {
            public int IdA_Dealer { set; get; }
            public int? IdB_Dealer { set; get; }
            public int IdA_Clearer { set; get; }
            public int? IdB_Clearer { set; get; }
            public int IdA_Entity { set; get; }
            public SideEnum Side { set; get; }

        }
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // EG 20200323 [25077] RDBMS : Correction Valorisation de side
        private TradeInstrumentData SetTradeInstrumentData(string pCS, IDbTransaction pDbTransaction)
        {
            TradeInstrumentData ret = null;
            if (IsOneSide)
            {
                #region Allocation
                ret = new TradeInstrumentData
                {
                    Side = (IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? SideEnum.Buy : SideEnum.Sell)
                };
                IFixParty dealerFixParty = GetDealer();
                if (null != dealerFixParty)
                {
                    IParty _dealerParty = DataDocument.GetParty(dealerFixParty.PartyId.href);
                    if (null != _dealerParty && _dealerParty.OTCmlId > 0)
                    {
                        ret.IdA_Dealer = _dealerParty.OTCmlId;
                        ret.IdB_Dealer = DataDocument.GetOTCmlId_Book(dealerFixParty.PartyId.href);
                        if (ret.IdB_Dealer.HasValue && ret.IdB_Dealer.Value > 0)
                            ret.IdA_Entity = BookTools.GetEntityBook(pCS, pDbTransaction, ret.IdB_Dealer.Value); 
                    }
                }
                IFixParty clearerFixParty = GetClearerCustodian();
                if (null != clearerFixParty)
                {
                    IParty _clearerParty = DataDocument.GetParty(clearerFixParty.PartyId.href);
                    if (null != _clearerParty && _clearerParty.OTCmlId > 0)
                    {
                        ret.IdA_Clearer = _clearerParty.OTCmlId;
                        ret.IdB_Clearer = DataDocument.GetOTCmlId_Book(clearerFixParty.PartyId.href);
                    }
                }
                #endregion Allocation
            }
            return ret;
        }
        

        /// <summary>
        /// Retourn une double pair d'acteurs (Pair.First = Buyer,Seller et Pair.Second = Dealer,Custodian)
        /// </summary>
        public virtual Pair<Pair<IReference, IReference>, Pair<IReference, IReference>> GetPartyReference()
        {
            Pair<Pair<IReference, IReference>, Pair<IReference, IReference>> partyReference = new Pair<Pair<IReference, IReference>, Pair<IReference, IReference>>
            {
                First = new Pair<IReference, IReference>(),
                Second = new Pair<IReference, IReference>()
            };

            #region Buyer / Seller
            IFixParty buyer = GetBuyerSeller(SideEnum.Buy);
            IFixParty seller = GetBuyerSeller(SideEnum.Sell);
            if (null == buyer)
                throw new NotSupportedException("buyer is not Found");
            if (null == seller)
                throw new NotSupportedException("seller is not Found");

            partyReference.First.First = buyer.PartyId;
            partyReference.First.Second = seller.PartyId;
            #endregion Buyer / Seller
            #region Dealer / Custodian
            if (IsOneSide) // ALLOC
            {
                IFixParty _dealerParty = GetDealer();
                if (null != _dealerParty)
                    partyReference.Second.First = _dealerParty.PartyId;
                IFixParty _custodianParty = GetClearerCustodian();
                if (null != _custodianParty)
                    partyReference.Second.Second = _custodianParty.PartyId;
            }
            #endregion Dealer / Custodian
            return partyReference;
        }
        #endregion Methods
    }
    #endregion RptSideContainer

    #region ExchangeTradedContainer
    /// <summary>
    /// Représente un ExchangeTraded
    /// <para>Contient les caractéristiques communes pour ETD et ESE</para>
    /// </summary>
    // EG 20140702 Refactoring (Héritage de RptSideContainer)
    public class ExchangeTradedContainer : RptSideProductContainer, IExchangeTradedBase
    {
        #region Members
        private readonly IExchangeTradedBase _exchangeTraded;
        #endregion Members
        #region Accessors

        
        /// <summary>
        /// Retourne le vecteur (interface IFixTrdCapRptSideGrp) en fontion du produit
        /// </summary>
        public override IFixTrdCapRptSideGrp[] RptSide
        {
            get { return TradeCaptureReport.TrdCapRptSideGrp; }
        }
       
        /// <summary>
        /// 
        /// </summary>
        public override DateTime ClearingBusinessDate
        {
            get { return _exchangeTraded.TradeCaptureReport.ClearingBusinessDate.DateTimeValue; }
        }
        /// <summary>
        /// 
        /// </summary>
        public IExchangeTradedBase ExchangeTraded
        {
            get { return _exchangeTraded; }
        }

        /// <summary>
        /// Obtient l'OTCmlId de l'asset [ASSET_ETD.IDASSET]
        /// </summary>
        public int SecurityId
        {
            get { return IntFunc.IntValue(_exchangeTraded.TradeCaptureReport.Instrument.SecurityId); }
        }
        /// <summary>
        /// Obtient le prix négocié
        /// </summary>
        public decimal Price
        {
            get { return _exchangeTraded.TradeCaptureReport.LastPx.DecValue; }
        }
        /// <summary>
        /// Obtient le type d'option Put or Call
        /// </summary>
        public PutOrCallEnum PutOrCall
        {
            get { return _exchangeTraded.TradeCaptureReport.Instrument.PutOrCall; }
        }
        /// <summary>
        /// Obtient le nombre le lots négociés
        /// </summary>
        public override decimal Qty
        {
            get { return _exchangeTraded.TradeCaptureReport.LastQty.DecValue; }
        }
        /// <summary>
        /// Obtient la date de transaction
        /// </summary>
        /// EG 20171004 [22374][23452] Upd
        public DateTime TradeDate
        {
            get { return _exchangeTraded.TradeCaptureReport.TransactTime.DateTimeValue.Value.Date; }
        }
        /// <summary>
        /// La category est une option
        /// </summary>
        public bool IsOption
        {
            get
            {
                return (Category.HasValue && (CfiCodeCategoryEnum.Option == Category.Value));
            }
        }
        /// <summary>
        /// Obtient true, si le exchange Traded représente un "trade ouverture de position".
        /// </summary>
        public bool IsPositionOpening
        {
            get
            {
                bool ret = false;
                if (_exchangeTraded.TradeCaptureReport.TrdTypeSpecified)
                    ret = (_exchangeTraded.TradeCaptureReport.TrdType == TrdTypeEnum.PositionOpening);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true, si le exchange Traded représente un "trade généré à la suite d'un Exercice/Assignation d'Option".
        /// </summary>
        public bool IsOptionExercise
        {
            get
            {
                bool ret = false;
                if (_exchangeTraded.TradeCaptureReport.TrdTypeSpecified)
                    ret = (_exchangeTraded.TradeCaptureReport.TrdType == TrdTypeEnum.OptionExercise);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true, si le exchange Traded représente un "trade généré à la suite d'un Cascading".
        /// </summary>
        public bool IsCascading
        {
            get
            {
                bool ret = false;
                if (_exchangeTraded.TradeCaptureReport.TrdTypeSpecified)
                    ret = (_exchangeTraded.TradeCaptureReport.TrdType == TrdTypeEnum.Cascading);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true, si le exchange Traded représente un "trade généré à la suite d'un ajustement suite à CA".
        /// </summary>
        public bool IsTradeCAAdjusted
        {
            get
            {
                bool ret = false;
                if (_exchangeTraded.TradeCaptureReport.TrdTypeSpecified)
                    ret = (_exchangeTraded.TradeCaptureReport.TrdType == TrdTypeEnum.CorporateAction);
                return ret;
            }
        }
        /// <summary>
        /// Obtient true, si le exchange Traded représente un "trade généré à la suite d'une Fermeture/réouverture de position".
        /// </summary>
        // EG 20190613 [24683] New
        // EG 202208016 [XXXXX] Modification TRDTYPE de réouverture sur ClosingReopeningPosition (Réouverture = PositionOpening)
        public bool IsClosingReopeningPosition
        {
            get
            {
                bool ret = false;
                if (_exchangeTraded.TradeCaptureReport.TrdTypeSpecified)
                    ret = (_exchangeTraded.TradeCaptureReport.TrdType == TrdTypeEnum.TechnicalTrade) ||
                          (_exchangeTraded.TradeCaptureReport.TrdType == TrdTypeEnum.PositionOpening);
                return ret;
            }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur où seul le member  _exchangeTraded est alimenté
        /// </summary>
        /// <param name="pExchangeTraded"></param>
        public ExchangeTradedContainer(IExchangeTradedBase pExchangeTraded)
            : base(pExchangeTraded)
        {
            _exchangeTraded = pExchangeTraded;
        }
        public ExchangeTradedContainer(IExchangeTradedBase pExchangeTraded, DataDocumentContainer pDataDocument)
            : base(pExchangeTraded, pDataDocument)
        {
            _exchangeTraded = pExchangeTraded;
        }

        #endregion Constructors
        #region Methods
        #region GetBuyer
        /// <summary>
        /// <para>Allocation: Obtient le D.O. s'il est l'acheteur, Obtient la ClearingOrganization ou ClearingFirm sinon</para>
        /// <para>Execution,Intermediation: Obtient l'acheteur</para>
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <returns>FixPary</returns>
        public IFixParty GetBuyer()
        {
            return GetBuyerSeller(SideEnum.Buy);
        }
        #endregion GetBuyer
        #region GetSeller
        /// <summary>
        /// <para>Allocation: Obtient le D.O. s'il est le vendeur, Obtient la ClearingOrganization ou ClearingFirm sinon</para>
        /// <para>Execution,Intermediation: Obtient le vendeur</para>
        /// <para>Obtient null si non trouvé</para>
        /// </summary>
        /// <returns>FixPary</returns>
        public IFixParty GetSeller()
        {
            return GetBuyerSeller(SideEnum.Sell);
        }
        #endregion GetSeller
        
        #region IsPosKeepingOnBookClearer
        /// <summary>
        /// Get the position keeping activation status of the clearer
        /// </summary>
        /// <param name="pCS">the connection string</param>
        /// <param name="pDataDoc">the current trade data document</param>
        /// <returns>true when the curren trade is on a ETD product and the clearer book has position keeping activated, false in 
        /// any other case</returns>
        public bool IsPosKeepingOnBookClearer(string pCS, IDbTransaction pDbTransaction)
        {
            return IsPosKeepingOnBookClearerCustodian(pCS, pDbTransaction);
        }
        #endregion IsPosKeepingOnBookClearer
        #region IsAssetInfoFilled
        /// <summary>
        /// Retourne true si les toutes les caractéristiques de l'asset sont renseignées
        /// <para>
        public bool IsAssetInfoFilled()
        {
            FixInstrumentContainer fixInstr = new FixInstrumentContainer(_exchangeTraded.TradeCaptureReport.Instrument);
            bool isETD = this.Category.HasValue;
            return fixInstr.IsAssetInfoFilled(isETD, IsOption);
        }
        #endregion IsAssetInfoFilled

        
        #region SetTradeInstrumentParameters
        // EG 20180307 [23769] Gestion dbTransaction
        public override void SetTradeInstrumentParameters(string pCS, IDbTransaction pDbTransaction, DataParameters pDataParameters)
        {
            base.SetTradeInstrumentParameters(pCS, pDbTransaction, pDataParameters);

            // ---------------------------------------------------------
            // ETD/ESE (POSITIONEFFECT, ORDERID, ORDERTYPE, INPUTSOURCE)
            // ---------------------------------------------------------
            //CC 20160805 [22091] 
            if (ArrFunc.IsFilled(RptSide))
            {
                IFixTrdCapRptSideGrp _rptSide = RptSide[0];
                if (_rptSide.PositionEffectSpecified)
                    pDataParameters["POSITIONEFFECT"].Value = _rptSide.PositionEffect;
                if (_rptSide.OrderIdSpecified)
                    pDataParameters["ORDERID"].Value = _rptSide.OrderId;
                if (_rptSide.OrdTypeSpecified)
                    pDataParameters["ORDERTYPE"].Value = ReflectionTools.ConvertEnumToString<OrdTypeEnum>(_rptSide.OrdType);
                if (_rptSide.TradeInputSourceSpecified)
                    pDataParameters["INPUTSOURCE"].Value = _rptSide.TradeInputSource;
            }


            // ---------------------------------------------------------
            // ETD/ESE (EXECUTIONID)
            // ---------------------------------------------------------
            if (ExchangeTraded.TradeCaptureReport.ExecIdSpecified)
                pDataParameters["EXECUTIONID"].Value = ExchangeTraded.TradeCaptureReport.ExecId;

            // ---------------------------------------------------------
            // ETD/ESE (RELTDPOSID)
            // ---------------------------------------------------------
            //PL 20160429 [22107] EUREX C7 3.0 Release
            if (ArrFunc.IsFilled(ExchangeTraded.TradeCaptureReport.TrdCapRptSideGrp))
            {
                IFixRelatedPositionGrp reltdPos = RptSideTools.GetRelatedPositionGrp(ExchangeTraded.TradeCaptureReport.TrdCapRptSideGrp[0], RelatedPositionIDSourceEnum.PositionID, false);
                if (reltdPos != null)
                    pDataParameters["RELTDPOSID"].Value = reltdPos.ID;
            }

            // ---------------------------------------------------------
            // ETD/ESE (SECONDARYTRDTYPE)
            // ---------------------------------------------------------
            if (ExchangeTraded.TradeCaptureReport.SecondaryTrdTypeSpecified)
                pDataParameters["SECONDARYTRDTYPE"].Value = ReflectionTools.ConvertEnumToString<SecondaryTrdTypeEnum>(ExchangeTraded.TradeCaptureReport.SecondaryTrdType);

            // ---------------------------------------------------------
            // ETD/ESE (TRDTYPE)
            // ---------------------------------------------------------
            if (ExchangeTraded.TradeCaptureReport.TrdTypeSpecified)
                pDataParameters["TRDTYPE"].Value = ReflectionTools.ConvertEnumToString<TrdTypeEnum>(ExchangeTraded.TradeCaptureReport.TrdType);
                    
            // ---------------------------------------------------------
            // ETD/ESE (TRDSUBTYPE)
            // ---------------------------------------------------------
            if (ExchangeTraded.TradeCaptureReport.TrdSubTypeSpecified)
                pDataParameters["TRDSUBTYPE"].Value = ReflectionTools.ConvertEnumToString<TrdSubTypeEnum>(ExchangeTraded.TradeCaptureReport.TrdSubType);
            // EG 20150622 [21143] Add STRIKEPRICE|UNITSTRIKEPRICE
            if (IsOption && TradeCaptureReport.Instrument.StrikePriceSpecified)
            {
                pDataParameters["STRIKEPRICE"].Value = TradeCaptureReport.Instrument.StrikePrice.DecValue;
                pDataParameters["UNITSTRIKEPRICE"].Value = UnitTypeEnum.Price.ToString();
            }

        }
        #endregion SetTradeInstrumentParameters
        #region SetTradeInstrumentToDataRow
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public override void SetTradeInstrumentToDataRow(string pCS, IDbTransaction pDbTransaction, DataRow pDataRow)
        {
            base.SetTradeInstrumentToDataRow(pCS, pDbTransaction, pDataRow);

            pDataRow["PRICE"] = Price;
            pDataRow["QTY"] = Qty;

            // ---------------------------------------------------------
            // ETD/ESE (POSITIONEFFECT, ORDERID, ORDERTYPE, INPUTSOURCE)
            // ---------------------------------------------------------
            IFixTrdCapRptSideGrp _rptSide = null;
            if (ArrFunc.IsFilled(RptSide))
                _rptSide = RptSide[0];

            pDataRow["POSITIONEFFECT"] = (null != _rptSide) && _rptSide.PositionEffectSpecified ? ReflectionTools.ConvertEnumToString<PositionEffectEnum>(_rptSide.PositionEffect) : Convert.DBNull;
            pDataRow["ORDERID"] = (null != _rptSide) && _rptSide.OrderIdSpecified ? _rptSide.OrderId : Convert.DBNull;
            pDataRow["ORDERTYPE"] = (null != _rptSide) && _rptSide.OrdTypeSpecified ? ReflectionTools.ConvertEnumToString<OrdTypeEnum>(_rptSide.OrdType) : Convert.DBNull;
            pDataRow["INPUTSOURCE"] = (null != _rptSide) && _rptSide.TradeInputSourceSpecified ? _rptSide.TradeInputSource : Convert.DBNull;

            // ---------------------------------------------------------
            // ETD/ESE (EXECUTIONID)
            // ---------------------------------------------------------
            pDataRow["EXECUTIONID"] = ExchangeTraded.TradeCaptureReport.ExecIdSpecified ? ExchangeTraded.TradeCaptureReport.ExecId : Convert.DBNull;

            // ---------------------------------------------------------
            // ETD/ESE (RELTDPOSID)
            // ---------------------------------------------------------
            IFixRelatedPositionGrp reltdPos = null;
            if (ArrFunc.IsFilled(ExchangeTraded.TradeCaptureReport.TrdCapRptSideGrp))
                reltdPos = RptSideTools.GetRelatedPositionGrp(ExchangeTraded.TradeCaptureReport.TrdCapRptSideGrp[0], RelatedPositionIDSourceEnum.PositionID, false);
            pDataRow["RELTDPOSID"] = (reltdPos != null) ? reltdPos.ID : Convert.DBNull;

            // ---------------------------------------------------------
            // ETD/ESE (SECONDARYTRDTYPE)
            // ---------------------------------------------------------
            pDataRow["SECONDARYTRDTYPE"] = ExchangeTraded.TradeCaptureReport.SecondaryTrdTypeSpecified ? 
                ReflectionTools.ConvertEnumToString<SecondaryTrdTypeEnum>(ExchangeTraded.TradeCaptureReport.SecondaryTrdType) : Convert.DBNull;

            // ---------------------------------------------------------
            // ETD/ESE (TRDTYPE)
            // ---------------------------------------------------------
            pDataRow["TRDTYPE"] = ExchangeTraded.TradeCaptureReport.TrdTypeSpecified ? 
                ReflectionTools.ConvertEnumToString<TrdTypeEnum>(ExchangeTraded.TradeCaptureReport.TrdType) : Convert.DBNull;

            // ---------------------------------------------------------
            // ETD/ESE (TRDSUBTYPE)
            // ---------------------------------------------------------
            pDataRow["TRDSUBTYPE"] = ExchangeTraded.TradeCaptureReport.TrdSubTypeSpecified ? 
                ReflectionTools.ConvertEnumToString<TrdSubTypeEnum>(ExchangeTraded.TradeCaptureReport.TrdSubType) : Convert.DBNull;

            pDataRow["STRIKEPRICE"] = (IsOption && TradeCaptureReport.Instrument.StrikePriceSpecified) ? TradeCaptureReport.Instrument.StrikePrice.DecValue : Convert.DBNull;
            pDataRow["UNITSTRIKEPRICE"] = (IsOption && TradeCaptureReport.Instrument.StrikePriceSpecified) ? UnitTypeEnum.Price.ToString() : Convert.DBNull;
        }
        #endregion SetTradeInstrumentToDataRow


        #endregion Methods
        #region IExchangeTradedBase Membres
        public IFixTradeCaptureReport TradeCaptureReport
        {
            get
            {
                return _exchangeTraded.TradeCaptureReport;
            }
            set
            {
                _exchangeTraded.TradeCaptureReport = value;
            }
        }
        /// <summary>
        /// Obtient l'acheteur
        /// </summary>
        public new IReference BuyerPartyReference
        {
            get
            {
                return _exchangeTraded.BuyerPartyReference;
            }
        }
        /// <summary>
        /// Obtient le vendeur
        /// </summary>
        public new IReference SellerPartyReference
        {
            get
            {
                return _exchangeTraded.SellerPartyReference;
            }
        }
        //
        public IReference ClearingOrganization
        {
            get { return _exchangeTraded.ClearingOrganization; }
        }
        //
        public IReference ClearingFirm
        {
            get { return _exchangeTraded.ClearingFirm; }
        }
        /// EG 20150624 [21151] New
        public bool CategorySpecified
        {
            set { _exchangeTraded.CategorySpecified = value; }
            get { return _exchangeTraded.CategorySpecified; }
        }
        public Nullable<CfiCodeCategoryEnum> Category
        {
            get
            {
                return _exchangeTraded.Category;
            }
            set
            {
                _exchangeTraded.Category = value;
            }
        }
        //
        public IFixInstrument CreateFixInstrument()
        {
            return _exchangeTraded.CreateFixInstrument();
        }
        // EG 20150624 [21151] New 
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public void InitPositionTransfer(decimal pQuantity, DateTime pDtBusiness)
        {
            _exchangeTraded.InitPositionTransfer(pQuantity, pDtBusiness);
        }
        // EG 20190613 [24683] New
        public void InitClosingReopening(decimal pQuantity, decimal pPrice, DateTime pDtBusiness, TrdTypeEnum pTrdType)
        {
            _exchangeTraded.InitPositionTransfer(pQuantity, pDtBusiness);
            _exchangeTraded.TradeCaptureReport.LastPx.DecValue = pPrice;
            _exchangeTraded.TradeCaptureReport.TrdType = pTrdType;
        }
        #endregion IExchangeTradedBase Membres

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        public override void ClearRptSide()
        {
            _exchangeTraded.TradeCaptureReport.TrdCapRptSideGrp = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        public override object Parent
        {
            get
            {
                return _exchangeTraded.TradeCaptureReport;
            }
        }
    }
    #endregion ExchangeTradedContainer

    #region ReturnSwapContainer
    /// <summary>
    /// Représente un ReturnSwap
    /// </summary>
    public class ReturnSwapContainer : RptSideProductContainer
    {
        #region Members
        private readonly IReturnSwap _returnSwap;
        private Pair<IReturnLeg, IReturnLegMainUnderlyer> _mainReturnLeg;
        private Pair<IInterestLeg, IInterestCalculation> _mainInterestLeg;
        #endregion Members

        #region Accessors
        public override string BuyerPartyReference
        {
            get { return _returnSwap.BuyerPartyReferenceSpecified ? _returnSwap.BuyerPartyReference.HRef : string.Empty; }
        }
        public override string SellerPartyReference
        {
            get { return _returnSwap.SellerPartyReferenceSpecified ? _returnSwap.SellerPartyReference.HRef : string.Empty; }
        }
        #region RptSide
        /// <summary>
        /// Retourne le vecteur (interface IFixTrdCapRptSideGrp) en fontion du produit
        /// </summary>
        public override IFixTrdCapRptSideGrp[] RptSide
        {
            get { return _returnSwap.RptSide; }
        }
        #endregion RptSide

        /// <summary>
        /// Obtient la date de transaction
        /// </summary>
        public DateTime TradeDate
        {
            get { return DataDocument.TradeDate; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        public override object Parent
        {
            get { return _returnSwap; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public IReturnSwap ReturnSwap
        {
            get { return _returnSwap; }
        }
        /// <summary>
        /// Obtient le 1ER RETURNLEG
        /// </summary>
        public Pair<IReturnLeg, IReturnLegMainUnderlyer> MainReturnLeg
        {
            get { return _mainReturnLeg; }
        }
        /// <summary>
        /// Obtient le RETURNLEG (IReturnLeg) du 1ER RETURNLEG)
        /// </summary>
        public IReturnLeg ReturnLeg
        {
            get { return _mainReturnLeg.First; }
        }
        /// <summary>
        /// Obtient le 1ER INTERESTLEG
        /// </summary>
        public Pair<IInterestLeg, IInterestCalculation> MainInterestLeg
        {
            get { return _mainInterestLeg; }
        }
        /// <summary>
        /// Obtient le INTERESTLEG (IInterestLeg) du 1ER INTERESTLEG)
        /// </summary>
        public IInterestLeg InterestLeg
        {
            get { return _mainInterestLeg.First; }
        }
        /// <summary>
        /// Obtient l'IDM du marché associé à l'UNDERLYINGASSET (SingleUnderlyer - Only) du 1ER RETURNLEG
        /// </summary>
        public Nullable<int> IdM
        {
            get
            {
                Nullable<int> _idM = null;
                if (null != _mainReturnLeg)
                {
                    if (_mainReturnLeg.Second.ExchangeIdSpecified)
                        _idM = _mainReturnLeg.Second.ExchangeId.OTCmlId;
                    else if (_mainReturnLeg.Second.SqlAssetSpecified)
                        _idM = _mainReturnLeg.Second.SqlAsset.IdM;
                }
                return _idM;
            }
        }
        /// <summary>
        /// Obtient le FIXML_SecurityExchange du marché associé à l'UNDERLYINGASSET (SingleUnderlyer - Only) du 1ER RETURNLEG
        /// </summary>
        public string SecurityExchange
        {
            get
            {
                string _securityExchange = string.Empty;
                if (null != _mainReturnLeg)
                {
                    if (_mainReturnLeg.Second.ExchangeIdSpecified)
                        _securityExchange = _mainReturnLeg.Second.ExchangeId.Value;
                    else if (_mainReturnLeg.Second.SqlAssetSpecified)
                        _securityExchange = _mainReturnLeg.Second.SqlAsset.Market_FIXML_SecurityExchange;
                }
                return _securityExchange;
            }
        }
        /// <summary>
        /// Obtient l'IDASSET de l'UNDERLYINGASSET (SingleUnderlyer - Only) du 1ER RETURNLEG
        /// </summary>
        public Nullable<int> IdAssetReturnLeg
        {
            get
            {
                Nullable<int> _idAsset = null;
                if (null != _mainReturnLeg)
                    _idAsset = _mainReturnLeg.Second.OTCmlId;
                return _idAsset;
            }
        }


        /// <summary>
        /// Obtient l'UNDERLYINGASSET principal
        /// </summary>
        /// FI 20140812 [XXXXX] add property
        public Pair<Cst.UnderlyingAsset, int> AssetReturnLeg
        {
            get
            {
                Pair<Cst.UnderlyingAsset, int> asset = null;
                if (null != _mainReturnLeg)
                {
                    if (_mainReturnLeg.Second.UnderlyerAsset.HasValue)
                        asset = new Pair<Cst.UnderlyingAsset, int>(_mainReturnLeg.Second.UnderlyerAsset.Value, _mainReturnLeg.Second.OTCmlId.Value);
                }
                return asset;
            }
        }

        /// <summary>
        /// Obtient l'IDASSET du floatingRate (si FloatingRate) du 1ER INTERESTLEG
        /// </summary>
        public Nullable<int> IdAssetInterestLeg
        {
            get
            {
                Nullable<int> _idAsset = null;
                if ((null != _mainInterestLeg) && _mainInterestLeg.Second.SqlAssetSpecified)
                {
                    _idAsset = _mainInterestLeg.Second.SqlAsset.IdAsset;
                }
                return _idAsset;
            }
        }

        /// <summary>
        /// Obtient l'IDASSET du floatingRate (si FloatingRate) du 1ER INTERESTLEG
        /// </summary>
        /// FI 20140812 [XXXXX] add property
        public Pair<Cst.UnderlyingAsset, int> AssetInterestLeg
        {
            get
            {
                Pair<Cst.UnderlyingAsset, int> asset = null;
                if ((null != _mainInterestLeg) && _mainInterestLeg.Second.SqlAssetSpecified)
                {
                    asset = new Pair<Cst.UnderlyingAsset, int>(Cst.UnderlyingAsset.RateIndex, _mainInterestLeg.Second.SqlAsset.IdAsset);
                }
                return asset;
            }
        }




        /// <summary>
        /// Obtient l'IDENTIFIANT de l'UNDERLYINGASSET (SingleUnderlyer - Only) du 1ER RETURNLEG
        /// </summary>
        public string Identifier
        {
            get
            {
                string _identifier = null;
                if (null != _mainReturnLeg)
                {
                    if (_mainReturnLeg.Second.InstrumentIdSpecified)
                        _identifier = _mainReturnLeg.Second.InstrumentId[0].Value;
                    else if (_mainReturnLeg.Second.SqlAssetSpecified)
                        _identifier = _mainReturnLeg.Second.SqlAsset.Identifier;
                }
                return _identifier;
            }
        }
        /// <summary>
        /// Obtient la DEVISE de l'UNDERLYER (SingleUnderlyer/Basket) du 1ER RETURNLEG
        /// </summary>
        public string UnderlyerCurrency
        {
            get
            {
                string _idC = string.Empty;
                if (null != _mainReturnLeg)
                {
                    if (_mainReturnLeg.Second.CurrencySpecified)
                        _idC = _mainReturnLeg.Second.Currency.Value;
                    else if (_mainReturnLeg.Second.SqlAssetSpecified)
                        _idC = _mainReturnLeg.Second.SqlAsset.IdC;
                }
                return _idC;
            }
        }
        /// <summary>
        /// Obtient l'OPENUNITS de l'UNDERLYER (SingleUnderlyer/Basket) du 1ER RETURNLEG
        /// </summary>
        public Nullable<decimal> MainOpenUnits
        {
            get
            {
                Nullable<decimal> _openUnits = null;
                if (null != _mainReturnLeg)
                    _openUnits = OpenUnits(_mainReturnLeg.Second);
                return _openUnits;
            }
        }
        /// <summary>
        /// Obtient le NETPRICE (INITIALPRICE - si PriceExpressionEnum.AbsoluteTerms) du 1ER RETURNLEG
        /// </summary>
        public Nullable<decimal> MainInitialNetPrice
        {
            get
            {
                Nullable<decimal> _netPrice = null;
                if (null != _mainReturnLeg)
                    _netPrice = InitialNetPrice(_mainReturnLeg.First);
                return _netPrice;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMainNotionalReset
        {
            get
            {
                bool _isNotionlReset = false;
                if (null != _mainReturnLeg)
                    _isNotionlReset = _mainReturnLeg.First.RateOfReturn.NotionalResetSpecified && _mainReturnLeg.First.RateOfReturn.NotionalReset.BoolValue;
                return _isNotionlReset;
            }
        }
        /// <summary>
        /// Obtient le NOTIONALAMOUNT (SingleUnderlyer/Basket) du 1ER RETURNLEG
        /// </summary>
        public IMoney MainNotionalAmount
        {
            get
            {
                IMoney _money = null;
                if ((null != _mainReturnLeg) && (_mainReturnLeg.First.Notional.NotionalAmountSpecified))
                    _money = _mainReturnLeg.First.Notional.NotionalAmount;
                return _money;
            }
        }
        /// <summary>
        /// Obtient le MainOpenUnits
        /// </summary>
        public override decimal Qty
        {
            get { return MainOpenUnits.Value; }
        }
        #endregion Accessors

        #region Constructors
        /// EG 20170510 [23153] Refactoring (use InitMainLegs)
        public ReturnSwapContainer(IReturnSwap pReturnSwap)
            : base(pReturnSwap, null)
        {
            _returnSwap = pReturnSwap;
            InitMainLegs();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReturnSwap"></param>
        /// <param name="pDataDocument"></param>
        /// FI 20170116 [21916] Modify 
        /// EG 20170510 [23153] Refactoring (use InitMainLegs)
        public ReturnSwapContainer(IReturnSwap pReturnSwap, DataDocumentContainer pDataDocument)
            : base(pReturnSwap, pDataDocument)
        {
            _returnSwap = pReturnSwap;
            InitMainLegs();
        }
        /// EG 20170510 [23153] Refactoring (use InitMainLegs|InitMainLegsWithAsset)
        public ReturnSwapContainer(string pCs, IReturnSwap pReturnSwap, DataDocumentContainer pDataDocument)
            : base(pReturnSwap, pDataDocument)
        {
            _returnSwap = pReturnSwap;
            if (StrFunc.IsEmpty(pCs))
                InitMainLegs();
            else
                InitMainLegsWithAsset(pCs, null);
        }

        /// EG 20170510 [23153] Refactoring (use InitMainLegsWithAsset)
        public ReturnSwapContainer(string pCs, IDbTransaction pDbTransaction, IReturnSwap pReturnSwap, DataDocumentContainer pDataDocument)
            : base(pReturnSwap, pDataDocument)
        {
            _returnSwap = pReturnSwap;
            InitMainLegsWithAsset(pCs, pDbTransaction);
        }
        #endregion Constructors

        #region Methods
        #region ClearRptSide
        public override void ClearRptSide()
        {
            _returnSwap.RptSide = null;
        }
        #endregion ClearRptSide

        #region GetBuyer / GetSeller
        /// <summary>
        /// Obtient l'acheteur
        /// </summary>
        /// <returns></returns>
        public IParty GetBuyer()
        {
            return DataDocument.GetParty(_returnSwap.BuyerPartyReference.HRef);
        }
        /// <summary>
        /// Obtient le vendeur
        /// </summary>
        /// <returns></returns>
        public IParty GetSeller()
        {
            return DataDocument.GetParty(_returnSwap.SellerPartyReference.HRef);
        }
        #endregion GetBuyer / GetSeller
        #region GetReturnLegInfo
        /// <summary>
        /// Retourne les données d'un Return Leg (SQL_Asset inclu)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pReturnLeg"></param>
        /// <returns></returns>
        /// FI 20140808 [XXXXX] public static method
        /// FI 20170116 [21916] private static method
        public static Pair<IReturnLeg, IReturnLegMainUnderlyer> GetReturnLegInfo(string pCS, IDbTransaction pDbTransaction, IReturnLeg pReturnLeg)
        {
            IReturnLegMainUnderlyer returnLegMainUnderlyer = null;
            if (pReturnLeg.Underlyer.UnderlyerSingleSpecified)
            {
                returnLegMainUnderlyer = pReturnLeg.Underlyer.UnderlyerSingle as IReturnLegMainUnderlyer;
                if (StrFunc.IsFilled(pCS))
                    returnLegMainUnderlyer.SqlAsset = GetSQLAsset(pCS, pDbTransaction, pReturnLeg.Underlyer.UnderlyerSingle.UnderlyingAsset);
            }
            else if (pReturnLeg.Underlyer.UnderlyerBasketSpecified)
            {
                returnLegMainUnderlyer = pReturnLeg.Underlyer.UnderlyerBasket as IReturnLegMainUnderlyer;
            }
            return new Pair<IReturnLeg, IReturnLegMainUnderlyer>(pReturnLeg, returnLegMainUnderlyer);
        }
        #endregion GetReturnLegInfo
        #region GetInterestLegInfo
        /// <summary>
        /// Retourne les données d'un Interest Leg (SQL_Asset inclu)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pInterestLeg"></param>
        /// <returns></returns>
        /// FI 20170116 [21916] private static method
        public static Pair<IInterestLeg, IInterestCalculation> GetInterestLegInfo(string pCS, IDbTransaction pDbTransaction, IInterestLeg pInterestLeg)
        {
            IInterestCalculation _interestCalculation = pInterestLeg.InterestCalculation;
            if (pInterestLeg.InterestCalculation.FloatingRateSpecified)
            {
                IFloatingRateCalculation _floatingRateCalculation = pInterestLeg.InterestCalculation.FloatingRate;
                if (StrFunc.IsFilled(pCS))
                    _interestCalculation.SqlAsset = GetSQLAsset(pCS, pDbTransaction, _floatingRateCalculation.FloatingRateIndex);
            }
            return new Pair<IInterestLeg, IInterestCalculation>(pInterestLeg, pInterestLeg.InterestCalculation);
        }
        #endregion GetInterestLegInfo
        #region GetSQLAsset
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pAsset"></param>
        /// <returns></returns>
        /// FI 20140808 [XXXXX] private static method
        /// FI 20140925 [XXXXX] Modify
        private static SQL_AssetBase GetSQLAsset<T>(string pCS, IDbTransaction pDbTransaction, T pAsset)
        {
            SQL_AssetBase sql_Asset = null;
            if (pAsset is IUnderlyingAsset)
            {
                IUnderlyingAsset underlyingAsset = pAsset as IUnderlyingAsset;
                if (underlyingAsset.OTCmlId > 0) // FI 20140925 [XXXXX] add test >0  car sinon Spheres® remonte 1 asset au pif
                {
                    switch (underlyingAsset.UnderlyerAssetCategory)
                    {
                        case Cst.UnderlyingAsset.EquityAsset:
                            sql_Asset = new SQL_AssetEquity(pCS, underlyingAsset.OTCmlId);
                            break;
                        case Cst.UnderlyingAsset.Index:
                            sql_Asset = new SQL_AssetIndex(pCS, underlyingAsset.OTCmlId);
                            break;
                        case Cst.UnderlyingAsset.FxRateAsset:
                            sql_Asset = new SQL_AssetFxRate(pCS, underlyingAsset.OTCmlId);
                            break;
                        case Cst.UnderlyingAsset.Bond:
                        case Cst.UnderlyingAsset.ConvertibleBond:
                            sql_Asset = new SQL_AssetDebtSecurity(pCS, underlyingAsset.OTCmlId);
                            break;
                    }
                }
            }
            else if (pAsset is IFloatingRateIndex)
            {
                IFloatingRateIndex _floatingRateIndex = pAsset as IFloatingRateIndex;
                if (_floatingRateIndex.OTCmlId > 0) // FI 20140925 [XXXXX] add test >0  car sinon Spheres® remonte 1 asset au pif
                    sql_Asset = new SQL_AssetRateIndex(pCS, SQL_AssetRateIndex.IDType.IDASSET, _floatingRateIndex.OTCmlId);
            }

            if (null != sql_Asset)
            {
                if (null != pDbTransaction)
                    sql_Asset.DbTransaction = pDbTransaction;
                sql_Asset.LoadTable();
            }

            return sql_Asset;
        }
        #endregion GetSQLAsset

        #region InitMainLegs
        /// <summary>
        /// Initialisation du returnLeg principal (sans sqlAsset) et de l'interestLeg principal (sans sqlAsset)
        /// </summary>
        /// EG 20170510 [23153] New
        public void InitMainLegs()
        {
            InitMainReturnLeg();
            InitMainInterestLeg();
        }
        #endregion InitMainLegs
        #region InitMainReturnLeg
        /// <summary>
        /// Initialisation du returnLeg principal (sans sqlAsset)
        /// </summary>
        /// EG 20170510 [23153] New
        public void InitMainReturnLeg()
        {
            if (_returnSwap.ReturnLegSpecified && ArrFunc.IsFilled(_returnSwap.ReturnLeg))
            {
                IReturnLeg _leg = _returnSwap.ReturnLeg[0];
                if (_leg.Underlyer.UnderlyerSingleSpecified)
                    _mainReturnLeg = new Pair<IReturnLeg, IReturnLegMainUnderlyer>(_leg, _leg.Underlyer.UnderlyerSingle as IReturnLegMainUnderlyer);
                else if (_leg.Underlyer.UnderlyerBasketSpecified)
                    _mainReturnLeg = new Pair<IReturnLeg, IReturnLegMainUnderlyer>(_leg, _leg.Underlyer.UnderlyerBasket as IReturnLegMainUnderlyer);
            }
        }
        #endregion InitMainReturnLeg
        #region InitMainInterestLeg
        /// <summary>
        /// Initialisation du interestLeg principal (sans sqlAsset)
        /// </summary>
        /// EG 20170510 [23153] New
        public void InitMainInterestLeg()
        {
            if (_returnSwap.InterestLegSpecified && ArrFunc.IsFilled(_returnSwap.InterestLeg))
            {
                IInterestLeg _leg = _returnSwap.InterestLeg[0];
                _mainInterestLeg = new Pair<IInterestLeg, IInterestCalculation>(_leg, _leg.InterestCalculation);
            }
        }
        #endregion InitMainInterestLeg

        #region InitMainLegsWithAsset
        /// <summary>
        /// Définie le membre _underlyers
        /// </summary>
        /// <param name="pCS"></param>
        /// EG 20170510 [23153] New       
        public void InitMainLegsWithAsset(string pCS, IDbTransaction pDbTransaction)
        {
            InitMainReturnLegWithAsset(pCS, pDbTransaction);
            InitMainInterestLegWithAsset(pCS, pDbTransaction);
        }
        #endregion InitMainLegsWithAsset
        #region InitMainReturnLegWithAsset
        /// <summary>
        /// Initialisation du returnLeg principal (avec sqlAsset)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// EG 20170510 [23153] New
        public void InitMainReturnLegWithAsset(string pCS, IDbTransaction pDbTransaction)
        {
            InitMainReturnLeg();
            if ((null != _mainReturnLeg) && (null != _mainReturnLeg.Second))
                _mainReturnLeg.Second.SetAsset(pCS, pDbTransaction);
        }
        #endregion InitMainReturnLegWithAsset
        #region InitMainInterestLegWithAsset
        /// <summary>
        /// Initialisation du interestLeg principal (avec sqlAsset)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// EG 20170510 [23153] New
        public void InitMainInterestLegWithAsset(string pCS, IDbTransaction pDbTransaction)
        {
            InitMainInterestLeg();
            if ((null != _mainInterestLeg) && (null != _mainInterestLeg.Second))
                _mainInterestLeg.Second.SetAsset(pCS, pDbTransaction);
        }
        #endregion InitMainInterestLegWithAsset

        #region InitialNetPrice
        /// <summary>
        /// Obtient le NETPRICE
        /// </summary>
        /// FI 20170116 [21916] private static method
        private static Nullable<decimal> InitialNetPrice(IReturnLeg pReturnLeg)
        {
            Nullable<decimal> _netPrice = null;
            if (null != pReturnLeg)
            {
                IReturnLegValuationPrice _price = pReturnLeg.RateOfReturn.InitialPrice;
                if (_price.NetPriceSpecified && (_price.NetPrice.PriceExpression == PriceExpressionEnum.AbsoluteTerms))
                    _netPrice = _price.NetPrice.Amount.DecValue;
            }
            return _netPrice;
        }
        #endregion InitialNetPrice

        #region OpenUnits
        /// <summary>
        /// Obtient l'OPENUNITS de l'UNDERLYER (SingleUnderlyer/Basket)
        /// </summary>
        /// FI 20170116 [21916] Modify (static method)
        private static Nullable<decimal> OpenUnits(IReturnLegMainUnderlyer pReturnLeg)
        {
            Nullable<decimal> _openUnits = null;
            if ((null != pReturnLeg) && pReturnLeg.OpenUnitsSpecified)
                _openUnits = pReturnLeg.OpenUnits.DecValue;
            return _openUnits;
        }
        #endregion OpenUnits


        #region SetMainOpenUnits
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pOpenUnits"></param>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public void SetMainOpenUnits(decimal pOpenUnits)
        {
            _mainReturnLeg.Second.OpenUnits = new EFS_Decimal(pOpenUnits);
        }
        #endregion SetMainOpenUnits
        #region SetTradeInstrumentParameters
        /// <summary>
        /// Alimentation des paramètres pour TRADEINSTRUMENT (IDA_DEALER, IDB_DEALER,...)
        /// </summary>
        /// <param name="pDataParameters"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public override void SetTradeInstrumentParameters(string pCS, IDbTransaction pDbTransaction, DataParameters pDataParameters)
        {
            Nullable<decimal> price = MainInitialNetPrice;
            pDataParameters["PRICE"].Value = price ?? Convert.DBNull;
            Nullable<decimal> openUnits = MainOpenUnits;
            pDataParameters["QTY"].Value = openUnits ?? Convert.DBNull;
            pDataParameters["IDA_CSSCUSTODIAN"].Value = IdA_Custodian;

            // FI 20141027 [XXXX] Add Alimentation de POSITIONEFFECT
            if (IsOneSide) // ALLOC
            {
                this.GetInstrument(pCS, pDbTransaction, out SQL_Instrument sqlInstrument);
                if (sqlInstrument.FungibilityMode == FungibilityModeEnum.CLOSE)
                    pDataParameters["POSITIONEFFECT"].Value = PositionEffectEnum.Close;
            }

            base.SetTradeInstrumentParameters(pCS, pDbTransaction, pDataParameters);
        }
        #endregion SetTradeInstrumentParameters
        #region SetTradeInstrumentData
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private class TradeInstrumentData
        {
            public Nullable<decimal> Price { set; get; }
            public Nullable<decimal> Qty { set; get; }
            public Nullable<FungibilityModeEnum> FungibilityMode { set; get; }
        }
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private TradeInstrumentData SetTradeInstrumentData(string pCS, IDbTransaction pDbTransaction)
        {
            TradeInstrumentData ret = new TradeInstrumentData
            {
                Price = MainInitialNetPrice,
                Qty = MainOpenUnits
            };

            if (IsOneSide)
            {
                this.GetInstrument(pCS, pDbTransaction, out SQL_Instrument sqlInstrument);
                ret.FungibilityMode = sqlInstrument.FungibilityMode;
            }

            return ret;
        }
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // EG 20211012 [XXXXX] Upd Setting POSITIONEFFECT via Enum
        // EG 20230505 [XXXXX] [WI617] Custodian optional => controls for Trade template
        public override void SetTradeInstrumentToDataRow(string pCS, IDbTransaction pDbTransaction, DataRow pDataRow)
        {
            TradeInstrumentData ret = SetTradeInstrumentData(pCS, pDbTransaction);
            pDataRow["PRICE"] = ret.Price ?? Convert.DBNull;
            pDataRow["QTY"] = ret.Qty ?? Convert.DBNull;
            pDataRow["IDA_CSSCUSTODIAN"] = IdA_Custodian ?? Convert.DBNull;
            pDataRow["POSITIONEFFECT"] = ret.FungibilityMode.HasValue && (ret.FungibilityMode.Value == FungibilityModeEnum.CLOSE) ? ReflectionTools.ConvertEnumToString<PositionEffectEnum>(PositionEffectEnum.Close) : Convert.DBNull;

            base.SetTradeInstrumentToDataRow(pCS, pDbTransaction, pDataRow);
        }
        #endregion SetTradeInstrumentData

        #endregion Methods
    }
    #endregion ReturnSwapContainer

    #region PricingValues
    /// <summary>
    /// 
    /// </summary>
    public class PricingValues
    {
        #region Members
        #region Input Values
        private readonly Nullable<decimal> m_UnderlyingPrice = 0m;
        private readonly decimal m_Strike = 0m;
        private readonly Nullable<decimal> m_RiskFreeInterest = 0m;
        private readonly Nullable<decimal> m_DividendYield = 0m;
        private decimal m_TimeToExpiration = 0m;
        private readonly decimal m_Volatility = 0m;
        private EFS_DayCountFraction m_DayCountFraction;
        private readonly bool m_IsPriceAndStrikeInverse;
        #endregion Input Values
        #region Result Values
        #region Same for Call and Put
        private decimal m_gamma = 0m;
        private decimal m_speed = 0m;
        private decimal m_vega = 0m;
        private decimal m_volga = 0m;
        private decimal m_vanna = 0m;
        private decimal m_color = 0m;
        #endregion Same for Call and Put
        #region Call
        private decimal m_callPrice = 0m;
        private decimal m_callDelta = 0m;
        private decimal m_callTheta = 0m;
        private decimal m_callRho1 = 0m;
        private decimal m_callRho2 = 0m;
        private decimal m_callCharm = 0m;
        #endregion Call
        #region Put
        private decimal m_putPrice = 0m;
        private decimal m_putDelta = 0m;
        private decimal m_putTheta = 0m;
        private decimal m_putRho1 = 0m;
        private decimal m_putRho2 = 0m;
        private decimal m_putCharm = 0m;
        #endregion Put

        #region FxForward
        private Nullable<decimal> m_SpotRate;
        private Nullable<decimal> m_ForwardPrice;

        private string m_Currency1;
        private Nullable<decimal> m_InterestRate1;

        private string m_Currency2;
        private EFS_DayCountFraction m_DayCountFraction2;
        private decimal m_TimeToExpiration2;
        private Nullable<decimal> m_InterestRate2;
        #endregion FxForward

        #endregion Result Values

        #region DBParameters
        // EG 20160404 Migration vs2013
        //private DataParameters m_ParamEventPricing;
        //private DataParameters m_ParamEventPricing2;

        //private IDbDataParameter m_ParamIde;

        //private IDbDataParameter m_ParamDcf;
        //private IDbDataParameter m_ParamDcfNum;
        //private IDbDataParameter m_ParamDcfDen;
        //private IDbDataParameter m_ParamTotalOfYear;
        //private IDbDataParameter m_ParamTotalOfDay;

        //private IDbDataParameter m_ParamIdC1;
        //private IDbDataParameter m_ParamIdC2;
        //private IDbDataParameter m_ParamStrike;
        //private IDbDataParameter m_ParamVolatility;
        //private IDbDataParameter m_ParamTimeToExpiration;
        //private IDbDataParameter m_ParamUnderlyingPrice;
        //private IDbDataParameter m_ParamDividendYield;
        //private IDbDataParameter m_ParamRiskFreeInterest;
        //private IDbDataParameter m_ParamExchangeRate;
        //private IDbDataParameter m_ParamInterestRate1;
        //private IDbDataParameter m_ParamInterestRate2;
        //private IDbDataParameter m_ParamCallPrice;
        //private IDbDataParameter m_ParamCallDelta;
        //private IDbDataParameter m_ParamCallRho1;
        //private IDbDataParameter m_ParamCallRho2;
        //private IDbDataParameter m_ParamCallTheta;
        //private IDbDataParameter m_ParamCallCharm;
        //private IDbDataParameter m_ParamPutPrice;
        //private IDbDataParameter m_ParamPutDelta;
        //private IDbDataParameter m_ParamPutRho1;
        //private IDbDataParameter m_ParamPutRho2;
        //private IDbDataParameter m_ParamPutTheta;
        //private IDbDataParameter m_ParamPutCharm;
        //private IDbDataParameter m_ParamGamma;
        //private IDbDataParameter m_ParamVega;
        //private IDbDataParameter m_ParamColor;
        //private IDbDataParameter m_ParamSpeed;
        //private IDbDataParameter m_ParamVanna;
        //private IDbDataParameter m_ParamVolga;

        //private IDbDataParameter m_ParamDcf2;
        //private IDbDataParameter m_ParamDcfNum2;
        //private IDbDataParameter m_ParamDcfDen2;
        //private IDbDataParameter m_ParamTotalOfYear2;
        //private IDbDataParameter m_ParamTotalOfDay2;
        //private IDbDataParameter m_ParamTimeToExpiration2;
        //private IDbDataParameter m_ParamSpotRate;
        #endregion DBParameters
        #endregion Members
        #region Accessors
        public EFS_DayCountFraction DayCountFraction
        {
            get { return m_DayCountFraction; }
            set { m_DayCountFraction = value; }
        }
        #region For FX Options only
        public Nullable<decimal> ExchangeRate
        {
            get { return UnderlyingPrice; }
        }
        public Nullable<decimal> DomesticInterest
        {
            get { return RiskFreeInterest; }
        }
        public Nullable<decimal> ForeignInterest
        {
            get { return DividendYield; }
        }
        #endregion For FX Options only
        #region For not FX Options only
        public Nullable<decimal> RiskFreeInterest
        {
            get { return m_RiskFreeInterest; }
        }
        public Nullable<decimal> DividendYield
        {
            get { return m_DividendYield; }
        }
        public Nullable<decimal> UnderlyingPrice
        {
            get
            {
                if (m_IsPriceAndStrikeInverse)
                    return 1 / m_UnderlyingPrice;
                else
                    return m_UnderlyingPrice;
            }
        }
        public Nullable<decimal> UnderlyingPriceCertain
        {
            get { return m_UnderlyingPrice; }
        }
        #endregion For not FX Options only
        #region For All Options
        public decimal Strike
        {
            get
            {
                if (m_IsPriceAndStrikeInverse)
                    return 1 / m_Strike;
                else
                    return m_Strike;
            }
        }
        public decimal StrikeCertain
        {
            get { return m_Strike; }
        }
        public decimal TimeToExpiration
        {
            get { return m_TimeToExpiration; }
            set { m_TimeToExpiration = value; }
        }
        public decimal Volatility
        {
            get { return m_Volatility; }
        }
        #endregion For All Options

        #region Call and Put identicals values
        public decimal Gamma
        {
            get { return m_gamma; }
            set { m_gamma = value; }
        }
        public decimal Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }
        public decimal Vega
        {
            get { return m_vega; }
            set { m_vega = value; }
        }
        public decimal Volga
        {
            get { return m_volga; }
            set { m_volga = value; }
        }
        public decimal Vanna
        {
            get { return m_vanna; }
            set { m_vanna = value; }
        }
        public decimal Color
        {
            get { return m_color; }
            set { m_color = value; }
        }
        #endregion Call and Put identicals values
        #region Call values
        public decimal CallPrice
        {
            get { return m_callPrice; }
            set { m_callPrice = value; }
        }
        public decimal CallDelta
        {
            get { return m_callDelta; }
            set { m_callDelta = value; }
        }
        public decimal CallTheta
        {
            get { return m_callTheta; }
            set { m_callTheta = value; }
        }
        public decimal CallRho1
        {
            get { return m_callRho1; }
            set { m_callRho1 = value; }
        }
        public decimal CallRho2
        {
            get { return m_callRho2; }
            set { m_callRho2 = value; }
        }
        public decimal CallCharm
        {
            get { return m_callCharm; }
            set { m_callCharm = value; }
        }
        public decimal CallGamma
        {
            get { return Gamma; }
            set { Gamma = value; }
        }
        public decimal CallSpeed
        {
            get { return Speed; }
            set { Speed = value; }
        }
        public decimal CallVega
        {
            get { return Vega; }
            set { Vega = value; }
        }
        public decimal CallVolga
        {
            get { return Volga; }
            set { Volga = value; }
        }
        public decimal CallVanna
        {
            get { return Vanna; }
            set { Vanna = value; }
        }
        public decimal CallColor
        {
            get { return Color; }
            set { Color = value; }
        }
        #region For FX Options only
        public decimal CallRhoCurrency1
        {
            get { return m_callRho1; }
            set { m_callRho1 = value; }
        }
        public decimal CallRhoCurrency2
        {
            get { return m_callRho2; }
            set { m_callRho2 = value; }
        }
        #endregion For FX Options only
        #endregion Call values
        #region Put values
        public decimal PutPrice
        {
            get { return m_putPrice; }
            set { m_putPrice = value; }
        }
        public decimal PutDelta
        {
            get { return m_putDelta; }
            set { m_putDelta = value; }
        }
        public decimal PutTheta
        {
            get { return m_putTheta; }
            set { m_putTheta = value; }
        }
        public decimal PutRho1
        {
            get { return m_putRho1; }
            set { m_putRho1 = value; }
        }
        public decimal PutRho2
        {
            get { return m_putRho2; }
            set { m_putRho2 = value; }
        }
        public decimal PutCharm
        {
            get { return m_putCharm; }
            set { m_putCharm = value; }
        }
        public decimal PutGamma
        {
            get { return Gamma; }
            set { Gamma = value; }
        }
        public decimal PutSpeed
        {
            get { return Speed; }
            set { Speed = value; }
        }
        public decimal PutVega
        {
            get { return Vega; }
            set { Vega = value; }
        }
        public decimal PutVolga
        {
            get { return Volga; }
            set { Volga = value; }
        }
        public decimal PutVanna
        {
            get { return Vanna; }
            set { Vanna = value; }
        }
        public decimal PutColor
        {
            get { return Color; }
            set { Color = value; }
        }
        #region For FX Options only
        public decimal PutRhoCurrency1
        {
            get { return m_putRho1; }
            set { m_putRho1 = value; }
        }
        public decimal PutRhoCurrency2
        {
            get { return m_putRho2; }
            set { m_putRho2 = value; }
        }
        #endregion For FX Options only
        #endregion Put values

        #region For FX forward only

        public EFS_DayCountFraction DayCountFraction2
        {
            get { return m_DayCountFraction2; }
            set { m_DayCountFraction2 = value; }
        }
        public Nullable<decimal> ForwardPrice
        {
            get { return m_ForwardPrice; }
            set { m_ForwardPrice = value; }
        }
        public decimal TimeToExpiration2
        {
            get { return m_TimeToExpiration2; }
            set { m_TimeToExpiration2 = value; }
        }
        public string Currency1
        {
            get { return m_Currency1; }
            set { m_Currency1 = value; }
        }
        public string Currency2
        {
            get { return m_Currency2; }
            set { m_Currency2 = value; }
        }
        public Nullable<decimal> InterestRate1
        {
            get { return m_InterestRate1; }
            set { m_InterestRate1 = value; }
        }
        public Nullable<decimal> InterestRate2
        {
            get { return m_InterestRate2; }
            set { m_InterestRate2 = value; }
        }
        public Nullable<decimal> SpotRate
        {
            get { return m_SpotRate; }
            set { m_SpotRate = value; }
        }
        #endregion For FX forward only

        #endregion Accessors
        #region Constructors
        public PricingValues()
        {
            m_IsPriceAndStrikeInverse = false;
            m_UnderlyingPrice = 0m;
            m_Strike = 0m;
            m_RiskFreeInterest = 0m;
            m_DividendYield = 0m;
            m_TimeToExpiration = 0m;
            m_Volatility = 0m;
        }
        public PricingValues(Nullable<decimal> pExchangeRate, decimal pStrike, Nullable<decimal> pDomesticInterest, Nullable<decimal> pForeignInterest,
            EFS_DayCountFraction pDayCountFraction, decimal pVolatility, bool pIsPriceAndStrikeInverse)
        {
            m_IsPriceAndStrikeInverse = pIsPriceAndStrikeInverse;
            m_UnderlyingPrice = pExchangeRate;
            m_Strike = pStrike;
            m_RiskFreeInterest = pDomesticInterest;
            m_DividendYield = pForeignInterest;
            m_Volatility = pVolatility;
            m_DayCountFraction = pDayCountFraction;
            m_TimeToExpiration = m_DayCountFraction.Factor;
        }


        public PricingValues(Nullable<decimal> pSpotRate, string pIdC1, string pIdC2)
        {
            m_SpotRate = pSpotRate;
            m_Currency1 = pIdC1;
            m_Currency2 = pIdC2;
        }

        public PricingValues(Nullable<decimal> pSpotRate, Nullable<decimal> pForwardRate,
                             string pIdC1, EFS_DayCountFraction pDayCountFraction, Nullable<decimal> pInterestRate1,
                             string pIdC2, EFS_DayCountFraction pDayCountFraction2, Nullable<decimal> pInterestRate2)
        {
            m_SpotRate = pSpotRate;
            m_ForwardPrice = pForwardRate;

            m_Currency1 = pIdC1;
            m_DayCountFraction = pDayCountFraction;
            m_TimeToExpiration = m_DayCountFraction.Factor;
            m_InterestRate1 = pInterestRate1;

            m_Currency2 = pIdC2;
            m_InterestRate2 = pInterestRate2;
            m_DayCountFraction2 = pDayCountFraction2;
            m_TimeToExpiration = m_DayCountFraction2.Factor;
        }
        #endregion Constructors
    }
    #endregion class PricingValues

    #region MarkToMarketTools
    public class MarkToMarketTools
    {
        #region Constructor
        public MarkToMarketTools()
        {
        }
        #endregion Constructor
        #region Method
        #region decimal functions
        private static decimal Abs(decimal pX)
        {
            return Convert.ToDecimal(Math.Abs(Convert.ToDouble(pX)));
        }
        private static decimal Exp(decimal pX)
        {
            return Convert.ToDecimal(Math.Exp(Convert.ToDouble(pX)));
        }
        private static decimal Log(decimal pX)
        {
            return Convert.ToDecimal(Math.Log(Convert.ToDouble(pX)));
        }
        private static decimal Max(decimal pX, decimal pY)
        {
            return (pX < pY ? pY : pX);
        }
        private static decimal Pow(decimal pX, decimal pY)
        {
            return Convert.ToDecimal(Math.Pow(Convert.ToDouble(pX), Convert.ToDouble(pY)));
        }
        private static decimal Sqrt(decimal pX)
        {
            return Convert.ToDecimal(Math.Sqrt(Convert.ToDouble(pX)));
        }
        #endregion decimal functions

        #region StandardNormal
        public static decimal StandardNormal(decimal pX)
        {
            const decimal p = 0.2316419m;
            const decimal c1 = 2.506628m;
            const decimal c2 = 0.319381530m;
            const decimal c3 = -0.356563782m;
            const decimal c4 = 1.781477937m;
            const decimal c5 = -1.821255978m;
            const decimal c6 = 1.330274429m;
            decimal w = 1m;
            if (pX < 0) w = -1m;
            decimal y = 1m / (1m + p * w * pX);
            return (0.5m + w * (0.5m - (Exp(-pX * pX / 2) / c1) * (y * (c2 + y * (c3 + y * (c4 + y * (c5 + y * c6)))))));
        }
        #endregion StandardNormal

        #region StandardNormalProbability
        public static decimal StandardNormalProbability(decimal pX)
        {
            return (1 / Sqrt(2 * ((decimal)Math.PI)) * Exp(-0.5m * Pow(pX, 2)));
        }
        #endregion StandardNormalProbability

        #region PricingValues Black76
        // see : http://www.riskglossary.com/articles/black_1976.htm
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUnderlyingPrice"></param> The current underlying forward price
        /// <param name="pStrike"></param> The strike price
        /// <param name="pRiskFreeInterest"></param> The continuously compounded risk free interest rate
        /// <param name="pDividendYield"></param> The dividend yield
        /// <param name="pTimeToExpiration"></param> The time in years until the expiration of the option
        /// <param name="pVolatility"></param> The implied volatility for the underlying
        /// <returns></returns>
        public static PricingValues Black76(Nullable<decimal> pUnderlyingPrice, decimal pStrike, Nullable<decimal> pRiskFreeInterest,
            Nullable<decimal> pDividendYield, EFS_DayCountFraction pDayCountFraction, decimal pVolatility)
        {
            PricingValues pricingValues = new PricingValues(pUnderlyingPrice, pStrike, pRiskFreeInterest, pDividendYield, pDayCountFraction, pVolatility, false);
            if (pUnderlyingPrice.HasValue && pRiskFreeInterest.HasValue && pDividendYield.HasValue)
            {
                decimal sqrtTime = Sqrt(pricingValues.TimeToExpiration);
                decimal dt = pricingValues.Volatility * Sqrt(pricingValues.TimeToExpiration);
                #region d1 & d2
                decimal d1 = (Log(pricingValues.UnderlyingPrice.Value / pricingValues.Strike) +
                                (pricingValues.RiskFreeInterest.Value - pricingValues.DividendYield.Value +
                                  Pow(pricingValues.Volatility, 2) / 2) * pricingValues.TimeToExpiration) / dt;
                _ = d1 - dt;
                #endregion d1 & d2
                #region StandardNormal of d1 & d2
                decimal phiD1 = StandardNormal(d1);
                #endregion StandardNormal of d1 & d2

                #region Delta
                pricingValues.CallDelta = phiD1 * Exp(-pricingValues.DividendYield.Value * pricingValues.TimeToExpiration);
                pricingValues.PutDelta = pricingValues.CallDelta - 1;
                #endregion Delta

                #region Gamma
                pricingValues.Gamma = pricingValues.CallDelta / (pricingValues.UnderlyingPrice.Value * dt);
                #endregion Gamma

                #region Vega
                decimal underlyingPricePerCallDelta = pricingValues.UnderlyingPrice.Value * pricingValues.CallDelta;
                pricingValues.Vega = underlyingPricePerCallDelta * sqrtTime;
                #endregion Vega
            }
            return pricingValues;
        }
        #endregion static PricingValues Black76

        #region PricingValues GarmanAndKolhagen
        //  
        /// <summary>
        /// Valuation: the Garman–Kohlhagen model (see : http://www.riskglossary.com/link/garman_kohlhagen_1983.htm)
        /// <para>
        ///  Domestic currency is the currency in which we obtain the value of the option; 
        /// </para>
        /// <para>
        ///  The formula also requires that FX rates – both strike and current spot be quoted in terms of "units of domestic currency per unit of foreign currency"
        /// </para>
        /// </summary>
        /// <param name="pExchRate">The current exchange rate in domestic currency per unit of foreign currency</param>
        /// <param name="pStrike">The strike exchange rate</param>
        /// <param name="pDomesticInterest">The continuously compounded domestic risk free interest rate</param> 
        /// <param name="pForeignInterest">The continuously compounded foreign risk free interest rate</param> 
        /// <param name="pTimeToExpiration">The time in years until the expiration of the option</param> 
        /// <param name="pVolatility">The implied volatility for the underlying exchange rate</param> 
        /// <returns></returns>
        public static PricingValues GarmanAndKolhagen(Nullable<decimal> pExchRate, decimal pStrike, Nullable<decimal> pDomesticInterest, Nullable<decimal> pForeignInterest,
            EFS_DayCountFraction pDayCountFraction, decimal pVolatility, bool pIsPriceAndStrikeInverse)
        {
            PricingValues pricingValues =
                new PricingValues(pExchRate, pStrike, pDomesticInterest, pForeignInterest, pDayCountFraction, pVolatility, pIsPriceAndStrikeInverse);

            if (pExchRate.HasValue && pDomesticInterest.HasValue && pForeignInterest.HasValue)
            {
                decimal sqrtTime = Sqrt(pricingValues.TimeToExpiration);
                decimal dt = pricingValues.Volatility * sqrtTime;
                #region d1 & d2
                decimal d1 = (Log(pricingValues.ExchangeRate.Value / pricingValues.Strike) +
                                     (pricingValues.DomesticInterest.Value - pricingValues.ForeignInterest.Value +
                                       Pow(pricingValues.Volatility, 2) / 2) * pricingValues.TimeToExpiration) / dt;

                decimal d2 = d1 - dt;
                #endregion d1 & d2

                #region StandardNormal of d1 & d2 (and opposites)
                decimal phiD1 = StandardNormal(d1);
                decimal phiD2 = StandardNormal(d2);
                decimal phiOppD1 = StandardNormal(-d1);
                decimal phiOppD2 = StandardNormal(-d2);
                #endregion StandardNormal of d1 & d2 (and opposites)

                decimal expOppForeignInterestRateT = Exp(-pricingValues.ForeignInterest.Value * pricingValues.TimeToExpiration);
                decimal expOppDomesticInterestRateT = Exp(-pricingValues.DomesticInterest.Value * pricingValues.TimeToExpiration);

                decimal SNPD1 = StandardNormalProbability(d1);

                #region Call Price
                decimal c1 = pricingValues.ExchangeRate.Value * expOppForeignInterestRateT * phiD1;
                decimal c2 = pricingValues.Strike * expOppDomesticInterestRateT * phiD2;
                pricingValues.CallPrice = c1 - c2;
                #endregion Call Price

                #region Put Price
                decimal p1 = pricingValues.ExchangeRate.Value * expOppForeignInterestRateT * phiOppD1;
                decimal p2 = pricingValues.Strike * expOppDomesticInterestRateT * phiOppD2;
                pricingValues.PutPrice = p2 - p1;
                #endregion Put Price

                #region Greeks
                #region Delta
                pricingValues.CallDelta = expOppForeignInterestRateT * phiD1;
                pricingValues.PutDelta = expOppForeignInterestRateT * (phiD1 - 1);
                #endregion Delta
                #region Gamma
                pricingValues.Gamma = (SNPD1 * expOppForeignInterestRateT) / (pricingValues.ExchangeRate.Value * dt);
                #endregion Gamma
                #region Vega
                pricingValues.Vega = pricingValues.ExchangeRate.Value * expOppForeignInterestRateT * SNPD1 * sqrtTime;
                #endregion Vega
                #region Theta
                decimal thetaPart1 = -((pricingValues.ExchangeRate.Value * expOppForeignInterestRateT * SNPD1 * pricingValues.Volatility) / (2 * sqrtTime));
                pricingValues.CallTheta = thetaPart1 + (pricingValues.ForeignInterest.Value * c1) - (pricingValues.DomesticInterest.Value * c2);
                pricingValues.PutTheta = thetaPart1 - (pricingValues.ForeignInterest.Value * p1) + (pricingValues.DomesticInterest.Value * p2);
                #endregion Theta
                #region Rho
                #region Rho1
                pricingValues.CallRhoCurrency1 = pricingValues.TimeToExpiration * c2;
                pricingValues.PutRhoCurrency1 = -pricingValues.TimeToExpiration * p2;
                #endregion Rho1
                #region Rho2
                pricingValues.CallRhoCurrency2 = -pricingValues.TimeToExpiration * c1;
                pricingValues.PutRhoCurrency2 = pricingValues.TimeToExpiration * p1;
                #endregion Rho2
                #endregion Rho
                #endregion Greeks
            }

            return pricingValues;
        }
        #endregion static PricingValues GarmanAndKolhagen

        #region PricingValues BinomialTrees
        // see : http://www.global-derivatives.com/options/american-options.php
        //       http://www.global-derivatives.com/options/european-options.php
        //       http://en.wikipedia.org/wiki/Binomial_options_pricing_model
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchRate">The current exchange rate in domestic currency per unit of foreign currency</param> 
        /// <param name="pStrike">The strike exchange rate</param> 
        /// <param name="pRiskFreeInterest">The continuously compounded risk free interest rate ( or the continuously compounded domestic risk free interest rate for FX options)</param> 
        /// <param name="pDividendYield">The dividend yield ( or the continuously compounded foreign risk free interest rate for FX Options)</param> 
        /// <param name="pTimeToExpiration">The time in years until the expiration of the option</param> 
        /// <param name="pVolatility">The implied volatility for the underlying exchange rate</param> 
        /// <param name="pSteps">The number of level of the binomial tree</param> 
        /// <returns></returns>
        public static PricingValues BinomialTrees(ExerciseStyleEnum pExeStyle, Nullable<decimal> pExchRate, decimal pStrike,
            Nullable<decimal> pRiskFreeInterest, Nullable<decimal> pDividendYield, EFS_DayCountFraction pDayCountFraction, decimal pVolatility, int pSteps)
        {
            PricingValues pricingValues = new PricingValues(pExchRate, pStrike, pRiskFreeInterest, pDividendYield, pDayCountFraction, pVolatility, false);

            if (pExchRate.HasValue && pRiskFreeInterest.HasValue && pDividendYield.HasValue)
            {
                decimal underlyingPrice = pricingValues.UnderlyingPrice.Value;
                decimal riskFreeInterest = pricingValues.RiskFreeInterest.Value;
                decimal dividendYield = pricingValues.DividendYield.Value;

                #region Call Price
                pricingValues.CallPrice = CalcBinomial(ExerciseStyleEnum.American, OptionTypeEnum.Call,
                    underlyingPrice, pricingValues.Strike, riskFreeInterest, dividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                #endregion Call Price
                #region Put Price
                pricingValues.PutPrice = CalcBinomial(ExerciseStyleEnum.American, OptionTypeEnum.Put,
                    underlyingPrice, pricingValues.Strike, riskFreeInterest, dividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                #endregion Put Price
                #region Greeks
                decimal value1;
                decimal value2;
                decimal value3;
                #region Delta
                decimal du = 0.001m * pricingValues.UnderlyingPrice.Value;
                decimal u1 = pricingValues.UnderlyingPrice.Value + du;
                decimal u2 = pricingValues.UnderlyingPrice.Value - du;
                decimal u3;

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, u1, pricingValues.Strike, riskFreeInterest,
                    pricingValues.DividendYield.Value, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, u2, pricingValues.Strike, riskFreeInterest,
                    pricingValues.DividendYield.Value, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.CallDelta = (value1 - value2) / (2 * du);

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, u1, pricingValues.Strike, riskFreeInterest, dividendYield,
                    pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, u2, pricingValues.Strike, riskFreeInterest, dividendYield,
                    pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.PutDelta = (value1 - value2) / (2 * du);
                #endregion Delta
                #region Gamma
                du = 0.1m * pricingValues.UnderlyingPrice.Value;
                u1 = pricingValues.UnderlyingPrice.Value + du;
                u3 = pricingValues.UnderlyingPrice.Value - du;
                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, u1, pricingValues.Strike, riskFreeInterest, dividendYield,
                    pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = pricingValues.CallPrice;
                value3 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, u3, pricingValues.Strike, riskFreeInterest, dividendYield,
                    pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.Gamma = (value1 - (2 * value2) + value3) / Pow(du, 2);
                #endregion Gamma
                #region Vega
                decimal dv = 0.001m * pricingValues.Volatility;
                decimal v1 = pricingValues.Volatility + dv;
                decimal v2 = pricingValues.Volatility - dv;
                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, dividendYield, pricingValues.TimeToExpiration, v1, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, dividendYield, pricingValues.TimeToExpiration, v2, pSteps);
                pricingValues.Vega = (value1 - value2) / (2 * dv);
                #endregion Vega
                #region Theta
                decimal dt = 0.001m * pricingValues.TimeToExpiration;
                decimal t1 = pricingValues.TimeToExpiration + dt;
                decimal t2 = pricingValues.TimeToExpiration - dt;

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, dividendYield, t1, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, dividendYield, t2, pricingValues.Volatility, pSteps);
                pricingValues.CallTheta = (value2 - value1) / (2 * dt);

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, dividendYield, t1, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, dividendYield, t2, pricingValues.Volatility, pSteps);
                pricingValues.PutTheta = (value2 - value1) / (2 * dt);
                #endregion Theta
                #region Rho1
                decimal dr = 0.1m * riskFreeInterest;
                decimal r1 = riskFreeInterest + dr;
                decimal r2 = riskFreeInterest - dr;

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    r1, dividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    r2, dividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.CallRho1 = (value1 - value2) / (2 * dr);

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    r1, dividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    r2, dividendYield, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.PutRho1 = (value1 - value2) / (2 * dr);
                #endregion Rho1
                #region Rho2
                decimal dd = 0.1m * dividendYield;
                decimal d1 = dividendYield + dd;
                decimal d2 = dividendYield - dd;

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, d1, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Call, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, d2, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.CallRho2 = (value1 - value2) / (2 * dd);

                value1 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, d1, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                value2 = CalcBinomial(pExeStyle, OptionTypeEnum.Put, pricingValues.UnderlyingPrice.Value, pricingValues.Strike,
                    riskFreeInterest, d2, pricingValues.TimeToExpiration, pricingValues.Volatility, pSteps);
                pricingValues.PutRho2 = (value1 - value2) / (2 * dd);
                #endregion Rho2
                #endregion Greeks
            }
            return pricingValues;
        }
        #endregion static PricingValues AmericainBinomialTrees

        #region CalcBinomial
        // see : http://www.global-derivatives.com/options/american-options.php
        //       http://www.global-derivatives.com/options/european-options.php
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExeStyle"></param>
        /// <param name="pOptionType"></param>
        /// <param name="pUnderlyingPrice"></param> The current underlying forward price
        /// <param name="pStrike"></param> The strike price
        /// <param name="pRiskFreeInterest"></param> The continuously compounded risk free interest rate
        /// <param name="pDividendYield"></param> The dividend yield
        /// <param name="pTimeToExpiration"></param> The time in years until the expiration of the option
        /// <param name="pVolatility"></param> The implied volatility for the underlying
        /// <param name="pSteps"></param> The number of level of the binomial tree
        /// <returns></returns>
        private static decimal CalcBinomial(ExerciseStyleEnum pExeStyle, OptionTypeEnum pOptionType, decimal pUnderlyingPrice,
            decimal pStrike, decimal pRiskFreeInterest, decimal pDividendYield, decimal pTimeToExpiration, decimal pVolatility, int pSteps)
        {
            decimal binomialValue;
            decimal dt = pTimeToExpiration / pSteps;
            decimal upMovement = Exp(pVolatility * Sqrt(dt));
            decimal downMovement = 1 / upMovement;
            decimal increaseProbability = (Exp((pRiskFreeInterest - pDividendYield) * dt) - downMovement) / (upMovement - downMovement);
            decimal decreaseProbability = 1 - increaseProbability;
            decimal expOppInterestDt = Exp(-pRiskFreeInterest * dt);
            decimal[] nodes = new decimal[pSteps + 1];
            int sign = 1;

            if (OptionTypeEnum.Put == pOptionType)
            {
                sign = -1;
            }

            // Compute each final node
            for (int i = 0; i <= pSteps; i += 1)
            {
                decimal spotPrice = pUnderlyingPrice * Pow(upMovement, i) * Pow(downMovement, pSteps - i);
                nodes[i] = Max(0, sign * (spotPrice - pStrike));
            }

            if (ExerciseStyleEnum.European == pExeStyle)
            {
                // Compute all nodes
                for (int tt = pSteps - 1; tt >= 0; tt -= 1)
                {
                    for (int i = 0; i <= tt; i += 1)
                    {
                        nodes[i] = (increaseProbability * nodes[i + 1] + decreaseProbability * nodes[i]) * expOppInterestDt;
                    }
                }
            }
            else if (ExerciseStyleEnum.American == pExeStyle)
            {
                // Compute all nodes
                for (int tt = pSteps - 1; tt >= 0; tt -= 1)
                {
                    for (int i = 0; i <= tt; i += 1)
                    {
                        nodes[i] = Max((sign * (pUnderlyingPrice * Pow(upMovement, i) * Pow(downMovement, Abs(i - tt)) - pStrike)),
                            (increaseProbability * nodes[i + 1] + decreaseProbability * nodes[i]) * expOppInterestDt);
                    }
                }
            }
            //
            binomialValue = nodes[0];
            //
            return binomialValue;
        }
        #endregion static decimal CalcBinomial
        #endregion Method
    }
    #endregion MarkToMarketTools


    #region public FxlegContainer
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150331 [XXPOC] Add
    public class FxLegContainer : RptSideProductContainer
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IFxLeg _fxLeg;

        /// <summary>
        ///  Obtient le leg
        /// </summary>
        public IFxLeg FxLeg
        {
            get
            {
                return _fxLeg;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        public override object Parent
        {
            get { return _fxLeg; }
        }
        

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLeg"></param>
        public FxLegContainer(IFxLeg pFxLeg)
            : this(pFxLeg, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLeg"></param>
        /// <param name="pDataDocument"></param>
        public FxLegContainer(IFxLeg pFxLeg, DataDocumentContainer pDataDocument)
            : base(pFxLeg, pDataDocument)
        {
            _fxLeg = pFxLeg;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override IFixTrdCapRptSideGrp[] RptSide
        {
            get
            {
                return _fxLeg.RptSide;
            }
        }
        /// <summary>
        ///  Obtient l'acheteur : le receiver de la devise1
        /// </summary>
        public override string BuyerPartyReference
        {
            get
            {
                return _fxLeg.ExchangedCurrency1.ReceiverPartyReference.HRef;
            }
        }
        /// <summary>
        ///  Obtient l'acheteur : le payer de la devise1
        /// </summary>
        public override string SellerPartyReference
        {
            get
            {
                return _fxLeg.ExchangedCurrency1.PayerPartyReference.HRef;
            }
        }

        /// <summary>
        /// Obtient l'OTCmlId de l'asset (Asset de change par défaut sur la base des 2 devises
        /// </summary>
        /// EG 20150402 (POC] New
        public int SecurityId(string pCS)
        {
            KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
            {
                IdC1 = _fxLeg.ExchangeRate.QuotedCurrencyPair.Currency1,
                IdC2 = _fxLeg.ExchangeRate.QuotedCurrencyPair.Currency2,
                QuoteBasisSpecified = true,
                QuoteBasis = _fxLeg.ExchangeRate.QuotedCurrencyPair.QuoteBasis
            };
            return keyAssetFxRate.GetIdAsset(CSTools.SetCacheOn(pCS), null);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ClearRptSide()
        {
            _fxLeg.RptSide = null;
        }

        #region SetTradeInstrumentParameters
        /// <summary>
        /// Alimentation des paramètres pour TRADEINSTRUMENT (IDA_DEALER, IDB_DEALER,...)
        /// </summary>
        /// <param name="pDataParameters"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public override void SetTradeInstrumentParameters(string pCS, IDbTransaction pDbTransaction, DataParameters pDataParameters)
        {
            pDataParameters["PRICE"].Value = _fxLeg.ExchangeRate.Rate.DecValue;
            pDataParameters["IDA_CSSCUSTODIAN"].Value = IdA_Custodian;
            base.SetTradeInstrumentParameters(pCS, pDbTransaction, pDataParameters);
        }
        #endregion SetTradeInstrumentParameters

        /// EG 20150306 [POC-BERKELEY] : New
        public IMarginRatio MarginRatio
        {
            set { _fxLeg.MarginRatio = value; }
            get { return _fxLeg.MarginRatio; }
        }
        /// EG 20150306 [POC-BERKELEY] : New
        public bool MarginRatioSpecified
        {
            set { _fxLeg.MarginRatioSpecified = value; }
            get { return _fxLeg.MarginRatioSpecified; }
        }

    }
    #endregion

    #region public FxOptionLegContainer
    /// <summary>
    /// 
    /// </summary>
    /// FI 20150331 [XXPOC] Add
    public class FxOptionLegContainer : RptSideProductContainer
    {

        /// <summary>
        /// 
        /// </summary>
        private readonly IFxOptionLeg _fxOptionLeg;


        /// <summary>
        ///  Obtient le leg
        /// </summary>
        public IFxOptionLeg FxOptionLeg
        {
            get
            {
                return _fxOptionLeg;
            }
        }

        /// <summary>
        ///  Obtient l'acheteur : le receiver de la devise1
        /// </summary>
        public override string BuyerPartyReference
        {
            get
            {
                return _fxOptionLeg.BuyerPartyReference.HRef;
            }
        }

        /// <summary>
        ///  Obtient l'acheteur : le receiver de la devise1
        /// </summary>
        public override string SellerPartyReference
        {
            get
            {
                return _fxOptionLeg.SellerPartyReference.HRef;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IFixTrdCapRptSideGrp[] RptSide
        {
            get
            {
                return _fxOptionLeg.RptSide;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        public override object Parent
        {
            get { return _fxOptionLeg; }
        }
        

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLeg"></param>
        public FxOptionLegContainer(IFxOptionLeg pFxOptionLeg)
            : this(pFxOptionLeg, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFxLeg"></param>
        /// <param name="pDataDocument"></param>
        public FxOptionLegContainer(IFxOptionLeg pFxOptionLeg, DataDocumentContainer pDataDocument)
            : base(pFxOptionLeg, pDataDocument)
        {
            _fxOptionLeg = pFxOptionLeg;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public override void ClearRptSide()
        {
            _fxOptionLeg.RptSide = null;
        }

        #region SetTradeInstrumentParameters
        /// <summary>
        /// Alimentation des paramètres pour TRADEINSTRUMENT (IDA_DEALER, IDB_DEALER,...)
        /// </summary>
        /// <param name="pDataParameters"></param>
        // EG 20150622 [21143] Add STRIKEPRICE|UNITSTRIKEPRICE
        // EG 20180307 [23769] Gestion dbTransaction
        public override void SetTradeInstrumentParameters(string pCS, IDbTransaction pDbTransaction, DataParameters pDataParameters)
        {
            if (FxOptionLeg.FxOptionPremiumSpecified && ArrFunc.IsFilled(FxOptionLeg.FxOptionPremium))
            {
                switch (FxOptionLeg.FxOptionPremium[0].PremiumQuote.PremiumQuoteBasis)
                {
                    case PremiumQuoteBasisEnum.PercentageOfCallCurrencyAmount:
                    case PremiumQuoteBasisEnum.PercentageOfPutCurrencyAmount:
                        pDataParameters["PRICE"].Value = FxOptionLeg.FxOptionPremium[0].PremiumQuote.PremiumValue.DecValue;
                        break;
                }
            }
            // EG 20150622 [21143]
            pDataParameters["STRIKEPRICE"].Value = FxOptionLeg.FxStrikePrice.Rate.DecValue;
            pDataParameters["UNITSTRIKEPRICE"].Value = FxOptionLeg.FxStrikePrice.StrikeQuoteBasis.ToString();

            pDataParameters["IDA_CSSCUSTODIAN"].Value = IdA_Custodian;
            base.SetTradeInstrumentParameters(pCS, pDbTransaction, pDataParameters);
        }
        #endregion SetTradeInstrumentParameters

        /// EG 20150306 [POC-BERKELEY] : New
        public IMarginRatio MarginRatio
        {
            set { _fxOptionLeg.MarginRatio = value; }
            get { return _fxOptionLeg.MarginRatio; }
        }
        /// EG 20150306 [POC-BERKELEY] : New
        public bool MarginRatioSpecified
        {
            set { _fxOptionLeg.MarginRatioSpecified = value; }
            get { return _fxOptionLeg.MarginRatioSpecified; }
        }


        /// <summary>
        /// Obtient l'OTCmlId de l'asset (Asset de change par défaut sur la base des 2 devises
        /// </summary>
        /// EG 20150402 (POC] New
        public int SecurityId(string pCS)
        {
            KeyAssetFxRate keyAssetFxRate = new KeyAssetFxRate
            {
                IdC1 = _fxOptionLeg.CallCurrencyAmount.Currency,
                IdC2 = _fxOptionLeg.PutCurrencyAmount.Currency,
                QuoteBasisSpecified = true
            };
            switch (_fxOptionLeg.FxStrikePrice.StrikeQuoteBasis)
            {
                case StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency:
                    keyAssetFxRate.QuoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                    break;
                case StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency:
                    keyAssetFxRate.QuoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                    break;
            }
            return keyAssetFxRate.GetIdAsset(CSTools.SetCacheOn(pCS), null);
        }
    }
    #endregion



    #region BondOptionContainer
    /// <summary>
    /// 
    /// </summary>
    /// EG 20150615 New
    public class BondOptionContainer : ProductContainer
    {

        /// <summary>
        /// 
        /// </summary>
        private readonly IDebtSecurityOption _debtSecurityOption;

        #region constructor
        public BondOptionContainer(IDebtSecurityOption pDebtSecurityOption)
            : this(pDebtSecurityOption, null)
        {
        }
        public BondOptionContainer(IDebtSecurityOption pDebtSecurityOption, DataDocumentContainer pDataDocument)
            : base((IProduct)pDebtSecurityOption, pDataDocument)
        {
            _debtSecurityOption = pDebtSecurityOption;
        }
        #endregion

        #region SetTradeInstrumentParameters
        /// <summary>
        /// Alimentation des paramètres pour TRADEINSTRUMENT (IDA_DEALER, IDB_DEALER,...)
        /// </summary>
        /// <param name="pDataParameters"></param>
        // EG 20150622 [21143] Add STRIKEPRICE|UNITSTRIKEPRICE
        public void SetTradeInstrumentParameters(DataParameters pDataParameters)
        {

            if (_debtSecurityOption.PremiumSpecified && _debtSecurityOption.Premium.PercentageOfNotionalSpecified)
            {
                pDataParameters["PRICE"].Value = _debtSecurityOption.Premium.PercentageOfNotional.DecValue;
            }
            // EG 20150622 [21143]
            if (_debtSecurityOption.Strike.PriceSpecified)
            {
                if (_debtSecurityOption.Strike.Price.PercentageSpecified)
                {
                    pDataParameters["STRIKEPRICE"].Value = _debtSecurityOption.Strike.Price.Percentage.DecValue;
                    pDataParameters["UNITSTRIKEPRICE"].Value = UnitTypeEnum.Percentage.ToString();
                }
                else if (_debtSecurityOption.Strike.Price.PriceSpecified)
                {
                    pDataParameters["STRIKEPRICE"].Value = _debtSecurityOption.Strike.Price.Price.DecValue;
                    pDataParameters["UNITSTRIKEPRICE"].Value = UnitTypeEnum.Price.ToString();
                }
            }
        }
        #endregion SetTradeInstrumentParameters
        // EG 20230505 [XXXXX] [WI617] New 
        public override void SetTradeInstrumentToDataRow(string pCS, IDbTransaction pDbTransaction, DataRow pDataRow){}
    }
    #endregion

    #region CommoditySpotContainer
    /// <summary>
    /// Représente un CommoditySpot
    /// </summary>
    public class CommoditySpotContainer : RptSideProductContainer
    {
        #region Members
        private readonly ICommoditySpot _commoditySpot;
        /// <summary>
        /// Repésente l'asset et le CommodityContract
        /// </summary>
        private SQL_AssetCommodityContract _assetCommodity;

        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient l'asset
        /// </summary>
        public SQL_AssetCommodityContract AssetCommodity
        {
            get { return _assetCommodity; }
            set { _assetCommodity = value; }
        }
        
        /// <summary>
        ///  Obtient le payer du FixedLeg (Identique au buyer présent dans rptSide)
        /// </summary>
        public override string BuyerPartyReference
        {
            get
            {
                return FixedLeg.PayerPartyReference.HRef; //C'est aussi le buyer présent dans rptSide
        }
        }
        
        /// <summary>
        ///  Obtient le receiverPartyReference du FixedLeg (Identique au seller présent dans rptSide)
        /// </summary>
        public override string SellerPartyReference
        {
            get
            {
                return FixedLeg.ReceiverPartyReference.HRef; //C'est aussi le seller présent dans rptSide
        }
        }
        
        /// <summary>
        /// Retourne le vecteur (interface IFixTrdCapRptSideGrp) en fontion du produit
        /// </summary>
        public override IFixTrdCapRptSideGrp[] RptSide
        {
            get
            {
                return _commoditySpot.RptSide;
            }
        }
        
        /// <summary>
        /// Obtient la date de transaction
        /// </summary>
        public DateTime TradeDate
        {
            get { return DataDocument.TradeDate; }
        }

        /// <summary>
        ///  Qté exprimée selon l'unité du prix (généralement MWh)
        /// </summary>
        /// FI 20161214 [21916] Add 
        public override decimal Qty
        {
            get
            {
                return TotalPhysicalQuantity.Quantity.DecValue;
            }
        }
        /// <summary>
        ///  Qté Total avec son unité [=unité du prix (généralement MWh)]
        /// </summary>
        /// FI 20161214 [21916] Add 
        // EG 20180423 Analyse du code Correction [CA1065]
        // EG 20221201 [25639] [WI484] Add Environmental Test
        public IUnitQuantity TotalPhysicalQuantity
        {
            get
            {
                IUnitQuantity ret;
                if (_commoditySpot.IsGas)
                    ret = ((IGasPhysicalLeg)_commoditySpot.PhysicalLeg).DeliveryQuantity.TotalPhysicalQuantity;
                else if (_commoditySpot.IsElectricity)
                    ret = ((IElectricityPhysicalLeg)_commoditySpot.PhysicalLeg).DeliveryQuantity.TotalPhysicalQuantity;
                else if (_commoditySpot.IsEnvironmental)
                    ret = ((IEnvironmentalPhysicalLeg)_commoditySpot.PhysicalLeg).NumberOfAllowances;
                else
                    throw new InvalidOperationException(StrFunc.AppendFormat("{0} is not implemented", _commoditySpot.PhysicalLeg.ToString()));

                return ret;
            }
        }

        /// <summary>
        /// Prix définie sur FixedLeg
        /// </summary>
        /// FI 20161214 [21916] Add 
        public IFixedPrice Price
        {
            get
            {
                return FixedLeg.FixedPrice;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Add
        public override object Parent
        {
            get { return _commoditySpot; }
        }
        
        /// <summary>
        ///  Obtient commoditySpot
        /// </summary>
        public ICommoditySpot CommoditySpot
        {
            get { return _commoditySpot; }
        }

        /// <summary>
        ///  Obtient le jour début de livraison (sans l'heure)
        /// </summary>
        public DateTime DeliveryStartDate
        {
            get { return _commoditySpot.EffectiveDate.AdjustableDate.UnadjustedDate.DateValue; }
        
        }

        /// <summary>
        ///  Obtient le jour fin  de livraison (sans l'heure)
        /// </summary>
        public DateTime DeliveryEndDate
        {
            get { return _commoditySpot.TerminationDate.AdjustableDate.UnadjustedDate.DateValue; }

        }


        /// <summary>
        ///  Obtient le jour début de livraison (avec l'heure)
        /// </summary>
        // EG 20171025 [23509] Upd
        // EG 20230505 [XXXXX] [WI617] Nullable Getter => controls for Trade template
        public Nullable<DateTimeOffset> DeliveryStartDateTime
        {
            get
            {
                Nullable<DateTimeOffset> deliveryStart = null;
                DateTime startDate = DeliveryStartDate;
                IPrevailingTime startTime = DeliveryStartTime;
                if (DtFunc.IsDateTimeFilled(startDate) && (null != startTime))
                    deliveryStart = DtFuncML.CalcDeliveryDateTimeOffset(startDate, startTime);
                return deliveryStart;
            }
        }

        /// <summary>
        ///  Obtient le jour fin de livraison (avec l'heure)
        /// </summary>
        // EG 20171025 [23509] Upd
        // EG 20230505 [XXXXX] [WI617] Nullable Getter => controls for Trade template
        public Nullable<DateTimeOffset> DeliveryEndDateTime
        {
            get
            {
                Nullable<DateTimeOffset> deliveryEnd = null;
                DateTime endDate = DeliveryEndDate;
                IPrevailingTime endTime = DeliveryEndTime;
                if (DtFunc.IsDateTimeFilled(endDate) && (null != endTime))
                    deliveryEnd = DtFuncML.CalcDeliveryDateTimeOffset(endDate, endTime);
                return deliveryEnd;
            }
        }

        /// <summary>
        /// Obtient l'heure de livraison début
        /// </summary>
        public IPrevailingTime DeliveryStartTime
            {
            get { return DeliveryTime(true); }
            }
        
        /// <summary>
        /// Obtient l'heure de livraison fin
        /// </summary>
        public IPrevailingTime DeliveryEndTime
        {
            get { return DeliveryTime(false); }
        }


        /// <summary>
        /// Obtient la jambe fixe du CommoditySpot
        /// </summary>
        public IFixedPriceSpotLeg FixedLeg
        {
            get { return _commoditySpot.FixedLeg; }
        }
        
        /// <summary>
        /// Obtient la jambe physique du CommoditySpot
        /// </summary>
        public IPhysicalLeg PhysicalLeg
        {
            get { return _commoditySpot.PhysicalLeg; }
        }

        /// <summary>
        /// Obtient null
        /// </summary>
        public override Nullable<int> IdA_Custodian
        {
            get { return null; }
        }
        

        #endregion Accessors

        #region Constructors
        public CommoditySpotContainer(ICommoditySpot pCommoditySpot)
            : this(pCommoditySpot, null)
        {
        }
        public CommoditySpotContainer(ICommoditySpot pCommoditySpot, DataDocumentContainer pDataDocument)
            : base(pCommoditySpot, pDataDocument)
        {
            _commoditySpot = pCommoditySpot;
        }
        public CommoditySpotContainer(string pCs, ICommoditySpot pCommoditySpot, DataDocumentContainer pDataDocument) :
            this(pCs, null, pCommoditySpot, pDataDocument)
        {
        }
        public CommoditySpotContainer(string pCs, IDbTransaction pTransaction, ICommoditySpot pCommoditySpot, DataDocumentContainer pDataDocument)
            : base(pCommoditySpot, pDataDocument)
        {
            _commoditySpot = pCommoditySpot;
            SetAssetCommodity(pCs, pTransaction);

        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Définie le membre _assetCommodity
        /// </summary>
        /// <param name="pCS"></param>
        public void SetAssetCommodity(string pCS)
        {
            SetAssetCommodity(pCS, null);
        } 

        /// <summary>
        /// Définie le membre _assetCommodity
        /// </summary>
        /// <param name="pCS"></param>
        public void SetAssetCommodity(string pCS, IDbTransaction pDbTransaction)
        {
            if (0 < _commoditySpot.CommodityAssetOTCmlId)
            {
                _assetCommodity = new SQL_AssetCommodityContract(pCS, _commoditySpot.CommodityAssetOTCmlId);
                if (null != pDbTransaction)
                    _assetCommodity.DbTransaction = pDbTransaction;
                _assetCommodity.LoadTable();
            }
        }
        
        /// <summary>
        /// Alimentation des paramètres pour TRADEINSTRUMENT (IDA_DEALER, IDB_DEALER,...)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataParameters"></param>
        // FI 20161214 [21916] Modify
        // EG 20171025 [23509] Add TZDLVY, Change DTDLVYSTART,DTDLVYSEND
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20230505 [XXXXX] [WI617] DeliveryStart, DeliveryEnd, ... optional => controls for Trade template
        public override void SetTradeInstrumentParameters(string pCS, IDbTransaction pDbTransaction, DataParameters pDataParameters)
        {
            //EFS_TradeLibrary savCurrent = EFS_Current.tradeLibrary;
            //new EFS_TradeLibrary(dataDocument.dataDocument);

            pDataParameters["PRICE"].Value = Price.Price.DecValue;
            pDataParameters["UNITPRICE"].Value = Price.PriceUnit.Value;

            /*pDataParameters["QTY"].Value = FixedLeg.totalQuantityCalculated;*/
            pDataParameters["UNITQTY"].Value = TotalPhysicalQuantity.QuantityUnit.Value;
            pDataParameters["QTY"].Value = Qty;  // FI 20161214 [21916] Utilisation de la property Qty (déjà utilisée dans les frais)
            pDataParameters["IDA_CSSCUSTODIAN"].Value = IdA_Custodian;

            Nullable<DateTimeOffset> dateTimeStart = DeliveryStartDateTime;
            Nullable<DateTimeOffset> dateTimeEnd = DeliveryEndDateTime;
            IPrevailingTime endTime = DeliveryEndTime;

            pDataParameters["DTDLVYSTART"].Value = dateTimeStart.HasValue? dateTimeStart.Value.UtcDateTime:Convert.DBNull;
            pDataParameters["DTDLVYEND"].Value = dateTimeEnd.HasValue? dateTimeEnd.Value.UtcDateTime:Convert.DBNull;
            pDataParameters["TZDLVY"].Value = (null != endTime)? endTime.Location.Value:Convert.DBNull;

            if (_commoditySpot.FixedLeg.GrossAmount.PaymentDateSpecified)
            {
                EFS_Payment payment = new EFS_Payment(pCS, pDbTransaction, _commoditySpot.FixedLeg.GrossAmount, DataDocument);
                pDataParameters["DTSETTLT"].Value = payment.AdjustedPaymentDate.DateValue;
            }

            if (IsOneSide) // ALLOC
            {
                IFixTrdCapRptSideGrp _rptSide = RptSide[0];
                if (_rptSide.PositionEffectSpecified)
                    pDataParameters["POSITIONEFFECT"].Value = _rptSide.PositionEffect;
                if (_rptSide.OrderIdSpecified)
                    pDataParameters["ORDERID"].Value = _rptSide.OrderId;
                if (_rptSide.OrdTypeSpecified)
                    pDataParameters["ORDERTYPE"].Value = ReflectionTools.ConvertEnumToString<OrdTypeEnum>(_rptSide.OrdType);
                if (_rptSide.TradeInputSourceSpecified)
                    pDataParameters["INPUTSOURCE"].Value = _rptSide.TradeInputSource;
                if (_rptSide.ExecRefIdSpecified)
                    pDataParameters["EXECUTIONID"].Value = _rptSide.ExecRefId;
            }

            base.SetTradeInstrumentParameters(pCS, pDbTransaction, pDataParameters);
        }

        #region SetTradeInstrumentToDataRow
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // EG 20221201 [25639] [WI484] Add Environmental Test
        // EG 20230505 [XXXXX] [WI617] DeliveryStart, DeliveryEnd, ... optional => controls for Trade template
        public override void SetTradeInstrumentToDataRow(string pCS, IDbTransaction pDbTransaction, DataRow pDataRow)
        {
            pDataRow["PRICE"] = Price.Price.DecValue;
            pDataRow["UNITPRICE"] = Price.PriceUnit.Value;
            pDataRow["QTY"] = Qty;
            pDataRow["UNITQTY"] = TotalPhysicalQuantity.QuantityUnit.Value;
            if (false == _commoditySpot.IsEnvironmental)
            {
                Nullable<DateTimeOffset> dateTimeStart = DeliveryStartDateTime;
                Nullable<DateTimeOffset> dateTimeEnd = DeliveryEndDateTime;
                IPrevailingTime endTime = DeliveryEndTime;

                pDataRow["DTDLVYSTART"] = dateTimeStart.HasValue? dateTimeStart.Value.UtcDateTime:Convert.DBNull;
                pDataRow["DTDLVYEND"] = dateTimeEnd.HasValue ? dateTimeEnd.Value.UtcDateTime:Convert.DBNull;
                pDataRow["TZDLVY"] = (null != endTime) && StrFunc.IsFilled(endTime.Location.Value) ? endTime.Location.Value : Convert.DBNull;
            }

            if (_commoditySpot.FixedLeg.GrossAmount.PaymentDateSpecified)
            {
                EFS_Payment payment = new EFS_Payment(pCS, pDbTransaction, _commoditySpot.FixedLeg.GrossAmount, DataDocument);
                pDataRow["DTSETTLT"] = payment.AdjustedPaymentDate.DateValue;
            }

            base.SetTradeInstrumentToDataRow(pCS, pDbTransaction, pDataRow);
        }
        #endregion SetTradeInstrumentToDataRow

        /// <summary>
        /// 
        /// </summary>
        public override void ClearRptSide()
        {
            _commoditySpot.RptSide = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsStart"></param>
        /// <returns></returns>
        private IPrevailingTime DeliveryTime(bool pIsStart)
        {
            IPrevailingTime deliveryTime = null;
            if (_commoditySpot.IsGas)
            {
                IGasDeliveryPeriods deliveryPeriods = (_commoditySpot.PhysicalLeg as IGasPhysicalLeg).DeliveryPeriods;
                if (pIsStart && deliveryPeriods.SupplyStartTimeSpecified)
                    deliveryTime = deliveryPeriods.SupplyStartTime;
                else if ((false == pIsStart) && deliveryPeriods.SupplyEndTimeSpecified)
                    deliveryTime = deliveryPeriods.SupplyEndTime;
            }
            else if (_commoditySpot.IsElectricity)
            {
                ISettlementPeriods[] settlementPeriods = (_commoditySpot.PhysicalLeg as IElectricityPhysicalLeg).SettlementPeriods;
                deliveryTime = (pIsStart ? settlementPeriods[0].StartTime.Time : settlementPeriods[0].EndTime.Time);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", _commoditySpot.PhysicalLeg.ToString()));

            return deliveryTime;
        }
        
        
        #endregion Methods

    }
    #endregion CommoditySpotContainer

    #region ExerciseTools
    public sealed class ExerciseTools
    {
        #region Constructor
        public ExerciseTools() { }
        #endregion Constructor

        #region GetExerciseDateRange
        /// <summary>
        /// Calcul de la date de commencement et d'expiration sur Exercice
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pExercise">Exercice</param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public static Pair<EFS_AdjustableDate, EFS_AdjustableDate> GetExerciseDateRange<T>(string pCS, T pExercise, DataDocumentContainer pDataDocument)
        {
            EFS_AdjustableDate commencementDate = null;
            EFS_AdjustableDate expirationDate = null;
            if (pExercise is IAmericanExercise american)
            {
                commencementDate = Tools.GetEFS_AdjustableDate(pCS, american.CommencementDate, pDataDocument);
                expirationDate = Tools.GetEFS_AdjustableDate(pCS, american.ExpirationDate, pDataDocument);
            }
            else if (pExercise is ISharedAmericanExercise sharedAmerican)
            {
                commencementDate = Tools.GetEFS_AdjustableDate(pCS, sharedAmerican.CommencementDate, pDataDocument);
                expirationDate = Tools.GetEFS_AdjustableDate(pCS, sharedAmerican.ExpirationDate, pDataDocument);
            }
            else if (pExercise is IEquityBermudaExercise equityBermuda)
            {
                commencementDate = Tools.GetEFS_AdjustableDate(pCS, equityBermuda.CommencementDate, pDataDocument);
                expirationDate = Tools.GetEFS_AdjustableDate(pCS, ((IEquitySharedAmericanExercise)equityBermuda).ExpirationDate, pDataDocument);
            }
            else if (pExercise is IBermudaExercise bermuda)
            {
                if (bermuda.BermudaExerciseDates.AdjustableDatesSpecified)
                {
                    #region Termination & Commencement dates calculation
                    DateTime greatestDate = Convert.ToDateTime(null);
                    DateTime smallestDate = DateTime.MaxValue;
                    IAdjustableDates exerciseDates = bermuda.BermudaExerciseDates.AdjustableDates;
                    for (int i = 0; i < exerciseDates.UnadjustedDate.Length; i++)
                    {
                        greatestDate = (exerciseDates[i] > greatestDate) ? exerciseDates[i] : greatestDate;
                        smallestDate = (exerciseDates[i] < smallestDate) ? exerciseDates[i] : smallestDate;
                    }
                    commencementDate = new EFS_AdjustableDate(pCS, smallestDate, exerciseDates.DateAdjustments, pDataDocument);
                    expirationDate = new EFS_AdjustableDate(pCS, greatestDate, exerciseDates.DateAdjustments, pDataDocument);
                    #endregion Termination & Commencement dates calculation
                }
            }
            else if (pExercise is IEuropeanExercise european)
            {
                commencementDate = Tools.GetEFS_AdjustableDate(pCS, european.ExpirationDate, pDataDocument);
                expirationDate = Tools.GetEFS_AdjustableDate(pCS, european.ExpirationDate, pDataDocument);
            }
            else if (pExercise is IEquityEuropeanExercise equityEuropean)
            {
                commencementDate = Tools.GetEFS_AdjustableDate(pCS, equityEuropean.ExpirationDate, pDataDocument);
                expirationDate = Tools.GetEFS_AdjustableDate(pCS, equityEuropean.ExpirationDate, pDataDocument);
            }

            return new Pair<EFS_AdjustableDate, EFS_AdjustableDate>(commencementDate, expirationDate);
        }
        #endregion GetExerciseDateRange

        #region GetExerciseRelevantUnderlyingDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public static List<EFS_AdjustableDate> GetExerciseRelevantUnderlyingDates<T>(string pCS, T pExercise, DataDocumentContainer pDataDocument)
        {
            List<EFS_AdjustableDate> lstDates = new List<EFS_AdjustableDate>();
            bool relevantUnderlyingDateSpecified = false;
            IAdjustableOrRelativeDates relevantUnderlyingDate = null;

            if (pExercise is IAmericanExercise american)
            {
                relevantUnderlyingDateSpecified = american.RelevantUnderlyingDateSpecified;
                relevantUnderlyingDate = american.RelevantUnderlyingDate;
            }
            else if (pExercise is IBermudaExercise bermuda)
            {
                relevantUnderlyingDateSpecified = bermuda.RelevantUnderlyingDateSpecified;
                relevantUnderlyingDate = bermuda.RelevantUnderlyingDate;
            }
            else if (pExercise is IEuropeanExercise european)
            {
                relevantUnderlyingDateSpecified = european.RelevantUnderlyingDateSpecified;
                relevantUnderlyingDate = european.RelevantUnderlyingDate;
            }

            if (relevantUnderlyingDateSpecified)
                lstDates = GetLstAdjustableDate(pCS, relevantUnderlyingDate, pDataDocument);
            return lstDates;
        }
        #endregion GetExerciseRelevantUnderlyingDates

        #region GetBermudaExerciseDates
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public static List<EFS_AdjustableDate> GetBermudaExerciseDates<T>(string pCS, T pExercise, DataDocumentContainer pDataDocument)
        {
            List<EFS_AdjustableDate> lstDates = new List<EFS_AdjustableDate>();
            if (pExercise is IBermudaExercise exercise)
            {
                IAdjustableOrRelativeDates bermudaExerciseDates = exercise.BermudaExerciseDates;
                lstDates = GetLstAdjustableDate(pCS, bermudaExerciseDates, pDataDocument);
            }
            return lstDates;
        }
        #endregion GetBermudaExerciseDates
        #region GetLstAdjustableDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public static List<EFS_AdjustableDate> GetLstAdjustableDate(string pCS, IAdjustableOrRelativeDates pSource, DataDocumentContainer pDataDocument)
        {
            List<EFS_AdjustableDate> lstDates = new List<EFS_AdjustableDate>();
            if (pSource.AdjustableDatesSpecified)
            {
                #region AdjustableDates
                IAdjustableDates adjustableDates = pSource.AdjustableDates;
                for (int i = 0; i < adjustableDates.UnadjustedDate.Length; i++)
                {
                    lstDates.Add(new EFS_AdjustableDate(pCS, adjustableDates[i], adjustableDates.DateAdjustments, pDataDocument));
                }
                #endregion AdjustableDates
            }
            else if (pSource.RelativeDatesSpecified)
            {
                #region RelativeDates
                IRelativeDates relativeDates = pSource.RelativeDates;
                if (Cst.ErrLevel.SUCCESS == Tools.OffSetDateRelativeTo(pCS, relativeDates, out DateTime[] offsetDates, pDataDocument))
                {
                    for (int i = 0; i < offsetDates.Length; i++)
                    {
                        lstDates.Add(new EFS_AdjustableDate(pCS, offsetDates[i], relativeDates.GetAdjustments, pDataDocument));
                    }
                }
                #endregion RelativeDates
            }
            return lstDates;
        }
        #endregion GetLstAdjustableDate

    }
    #endregion ExerciseTools

    #region MarketTools2
    /// <summary>
    /// Tools Market (classe partielle présente également dans EFSTools
    /// </summary>
    public sealed class MarketTools2
    {
        #region GetListBusinessCenters
        /// <summary>
        /// Get All BusinessCenters for Market list
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstExchangeTraded"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        public static List<string> GetListBusinessCenters(string pCS, List<IExchangeTraded> pLstExchangeTraded)
        {
            List<string> lstIdBC = new List<string>();
            List<Pair<string, string>> lstMarket = GetMarkets(pLstExchangeTraded);
            if (0 < lstMarket.Count)
            {
                string lstIdM = string.Empty;
                string lstIdM_FIXML_SecurityExchange = string.Empty;
                lstMarket.ForEach(market =>
                {
                    switch (market.First)
                    {
                        case "exchangeId": // FIXML_SecurityExchange
                            lstIdM_FIXML_SecurityExchange += DataHelper.SQLString(market.Second) + ",";
                            break;
                        case "relatedExchangeId": // Identifier
                            lstIdM += DataHelper.SQLString(market.Second) + ",";
                            break;
                    }
                });

                string sqlQuery = @"select distinct IDBC from dbo.VW_MARKET_IDENTIFIER";
                string sqlWhere = null;
                if (StrFunc.IsFilled(lstIdM))
                {
                    sqlWhere = Cst.CrLf + @" where IDENTIFIER in (" + lstIdM.Remove(lstIdM.Length - 1) + ")" + Cst.CrLf;
                }
                if (StrFunc.IsFilled(lstIdM_FIXML_SecurityExchange))
                {
                    sqlWhere = String.IsNullOrEmpty(sqlWhere) ? SQLCst.WHERE : SQLCst.OR;
                    sqlWhere += "FIXML_SecurityExchange in (" + lstIdM_FIXML_SecurityExchange.Remove(lstIdM_FIXML_SecurityExchange.Length - 1) + ")" + Cst.CrLf;
                }

                using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, sqlQuery + sqlWhere))
                {
                    while (dr.Read())
                    {
                        string idBC = dr["IDBC"].ToString();
                        if (StrFunc.IsFilled(idBC) && (false == lstIdBC.Contains(idBC)))
                            lstIdBC.Add(idBC);
                    }
                }
            }
            return lstIdBC;
        }
        #endregion GetListBusinessCenters
        #region GetBusinessDayAdjustments
        /// <summary>
        /// Get BusinessCenter for Market list and construct BusinessDayAdjustment
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstExchangeTraded">Market List</param>
        /// <param name="pProduct">Product</param>
        /// <param name="pDefaultBDC">Default BusinessDayConventionEnum</param>
        /// <returns></returns>
        public static IBusinessDayAdjustments GetBusinessDayAdjustments(string pCS, List<IExchangeTraded> pLstExchangeTraded, IProductBase pProduct, BusinessDayConventionEnum pDefaultBDC)
        {
            IBusinessDayAdjustments bda = null;
            List<string> lstIdBC = MarketTools2.GetListBusinessCenters(pCS, pLstExchangeTraded);
            if (0 < lstIdBC.Count)
                bda = ((IProductBase)pProduct).CreateBusinessDayAdjustments(pDefaultBDC, lstIdBC.ToArray());
            return bda;
        }
        #endregion GetBusinessDayAdjustments
        #region GetMarkets
        /// <summary>
        /// Retourne la liste des Marché (FIXML_SecurityExchange (exchangeId)|IDENTIFIER (relatedExchangeId)
        /// </summary>
        /// <param name="pExchangeTraded"></param>
        /// <returns></returns>
        public static List<Pair<string, string>> GetMarkets(List<IExchangeTraded> pLstExchangeTraded)
        {
            List<Pair<string, string>> lstMarket = new List<Pair<string, string>>();

            pLstExchangeTraded.ForEach(item =>
                {
                    if (item.ExchangeIdSpecified &&
                        (false == lstMarket.Exists(match => (match.First == "exchangeId") && (match.Second == item.ExchangeId.Value))))
                    {
                        lstMarket.Add(new Pair<string, string>("exchangeId", item.ExchangeId.Value));
                    }
                    if (item.RelatedExchangeIdSpecified)
                    {
                        foreach (IScheme relatedExchangeId in item.RelatedExchangeId)
                        {
                            if (false == lstMarket.Exists(match => (match.First == "relatedExchangeId") && (match.Second == relatedExchangeId.Value)))
                                lstMarket.Add(new Pair<string, string>("relatedExchangeId", relatedExchangeId.Value));
                        }
                    }
                    if (item.OptionsExchangeIdSpecified)
                    {
                        foreach (IScheme optionsExchangeId in item.OptionsExchangeId)
                        {
                            if (false == lstMarket.Exists(match => (match.First == "relatedExchangeId") && (match.Second == optionsExchangeId.Value)))
                                lstMarket.Add(new Pair<string, string>("relatedExchangeId", optionsExchangeId.Value));
                        }
                    }
                });
            return lstMarket;
        }
        #endregion GetMarkets
    }
    #endregion MarketTools2

    /// <summary>
    ///  Représente une valeur et son scheme 
    /// </summary>
    /// FI 20170928 [23452] Add
    public class SchemeData : IScheme
    {

        public string scheme;
        public string value;

        string IScheme.Scheme
        {
            get
            {
                return this.scheme;
            }
            set
            {
                scheme = value;
            }
        }
        string IScheme.Value
        {
            get
            {
                return @value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}




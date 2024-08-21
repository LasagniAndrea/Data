using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Permission;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
//PasGlop Faire un search dans la solution de "TODO FEEMATRIX"

namespace EFS.TradeInformation
{
    /// <summary>
    /// Représente un environment sur lequel on calcule les frais
    /// </summary>
    public class FeeRequest
    {
        #region Members
        
        
        



        
        #endregion Members

        #region Accessors
        public string CS
        {
            get;
            set;
        }

        public IDbTransaction DbTransaction
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le trade 
        /// </summary>
        public TradeInput TradeInput
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient le DataDocument sur lequel on calcule les fees
        /// </summary>
        public DataDocumentContainer DataDocument
        {
            get
            {
                if (null == TradeInput)
                    throw new InvalidOperationException("TradeInput");

                return TradeInput.DataDocument;
            }
        }

        /// <summary>
        ///  Obtient le product du DataDocument
        /// </summary>
        public ProductContainer Product
        {
            get { return DataDocument.CurrentProduct; }
        }

        /// <summary>
        /// Obtient l'ExchangeTradedDerivative du DataDocument
        /// <para>Si le produit est une strategy, retourne l'ExchangeTradedDerivative maître de la strategy</para>
        /// <para>Retourne null si le product n'est pas une ETD</para>
        /// </summary>
        public IExchangeTradedDerivative ExchangeTradedDerivative
        {
            get
            {
                IExchangeTradedDerivative ret = null;

                if (Product.IsStrategy)
                {
                    if (Product.MainProduct.IsExchangeTradedDerivative)
                        ret = (IExchangeTradedDerivative)Product.MainProduct.Product;
                }
                else if (Product.IsExchangeTradedDerivative)
                {
                    ret = (IExchangeTradedDerivative)Product.Product;
                }

                return ret;
            }
        }
        /// <summary>
        /// Obtient le Contrat ETD ou le contrat Commodity
        /// </summary>
        /// FI 20170908 [23409] sqlDerivativeContract devient Contract de type Pair &lt;Cst.ContractCategory,SQL_TableWithID&gt;
        public Pair<Cst.ContractCategory, SQL_TableWithID> Contract
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le Market 
        /// </summary>
        public SQL_Market SqlMarket
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit l'acheteur ou le vendeur
        /// </summary>
        public Party PartyA
        {
            private set;
            get;
        }

        /// <summary>
        /// Obtient ou définit l'acheteur ou le vendeur face à PartyA
        /// </summary>
        public Party PartyB
        {
            private set;
            get;
        }

        /// <summary>
        /// Obtient ou définit les brokers
        /// </summary>
        public OtherParty[] PartyBroker
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la Devise principale
        /// </summary>
        public string Idc
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la Devise2
        /// </summary>
        public string Idc2
        {
            get;
            private set;
        }

        // EG 20150708 [21103] New
        public bool IsSafekeeping
        {
            get { return (null != TradeInput.safekeepingAction); }
        }
        public SafekeepingAction SafekeepingAction
        {
            get { return TradeInput.safekeepingAction; }
        }

        public bool IsWithAuditMsg
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient ou Définit l'action opérée sur le trade
        /// <para>
        /// Elle permet de charger uniquement les instructions spécifiques à cette action
        /// </para>
        /// </summary>
        public string Action
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public Cst.Capture.ModeEnum Mode
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit l'assiette sur laquelle les frais seront calculés, elle est facultative
        /// <para>
        /// Elle permet de 
        /// - charger uniquement les barèmes spécifiques à cette assiette 
        /// - d'utiliser sa valeur pour le calcul des frais, ds ce cas la valeur de l'assiette n'est pas lue sur le Trade.
        /// </para>
        /// </summary>
        // RD 20170208 [22815]
        //public string AssessmentBasis
        public string[] AssessmentBasis
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de l'assiette sur laquelle les frais seront calculés
        /// </summary>
        public decimal AssessmentBasis_DecValue
        {
            get;
            private set;
        }

        /// 
        /// <summary>
        /// Obtient ou définit la valeur string de l'assiette sur laquelle les frais seront calculés
        /// </summary>
        public string AssessmentBasis_StrValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient true si _Action != null
        /// </summary>
        public bool IsCompleted
        {
            get { return (Action != null); }
        }
       
        /// <summary>
        /// Retourne true si l'action opérée sur le trade est une action de dénouement d'option avec application des frais, hors Abandon:
        /// <para>Menu: Assignation (Manuel et Automatique)</para>
        /// <para>Menu: Exercice (Manuel et Automatique)</para>
        /// </summary>
        public bool IsFeeOptionSettlementAction
        {
            get
            {
                return ((Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ASS)) ||
                    (Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE)) ||
                    (Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ASS_AUTOMATIC)) ||
                    (Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE_AUTOMATIC)));
            }
        }

        /// <summary>
        /// Retourne true si l'action/traitement opérée sur le trade est une action de cascading.
        /// </summary>
        public bool IsFeeCascadingAction
        {
            get { return (Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_CASC)); }
        }

        /// <summary>
        /// Obtient la date de référence 
        /// <para>en saisie de trade ETD, la date de référence est la business Date</para>
        /// <para>en saisie de trade NON ETD, la date de référence est la transaction Date</para>
        /// <para>en saisie d'action (execice) la date de référence est la date de l'action</para>
        /// </summary>
        // EG 20150708 [21103] Add IsSafekeeping control
        // EG 20151102 [21465]
        // EG 20180423 Analyse du code Correction [CA1065]
        public DateTime DtReference
        {
            get;
            private set;
        }

        /// <summary>
        /// Listes des permissions associées à {Action} par ordre de priorité
        /// </summary>
        // FI 20180502 [23926] Add Property
        // PL 20200217 [25207] Change DataType STRING instead of INT
        //public int[] IdPermission
        public string[] IdPermission
        {
            get;
            private set;
        }

        /// <summary>
        ///  Obtient les infos concernant la substitution de barème
        /// </summary>
        public FeeRequestSubstitute SubstituteInfo
        {
            get;
            set;
        }

        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180502 [23926] Add
        public FeeRequest()
        {

        }
        
        /// <summary>
        /// Constructeur de base, appelé depuis la saisie des trades.
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pTradeInput">Trade</param>
        /// <param name="pAction">Action</param>
        public FeeRequest(string pCS, IDbTransaction pDbTransaction, TradeInput pTradeInput, string pAction)
            : this(pCS, pDbTransaction, pTradeInput, pAction, Cst.Capture.ModeEnum.Update, null, 0, null)
        {
            //NB: Utilisation par défaut de "ModeEnum.Update" (PL)
        }
        /// <summary>
        /// Constructeur de base, appelé depuis la saisie des trades.
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pTradeInput">Trade</param>
        /// <param name="pAction">Action</param>
        /// <param name="pMode">Mode</param>
        public FeeRequest(string pCS, IDbTransaction pDbTransaction, TradeInput pTradeInput, string pAction, Cst.Capture.ModeEnum pMode)
            : this(pCS, pDbTransaction, pTradeInput, pAction, pMode, null, 0, null) { }
        /// <summary>
        /// Constructeur spécifique, appelé depuis le dénouement d'option. Ici, on passe l'assiette car celle-ci est la qté de lots dénoués. 
        /// </summary>
        /// <param name="pCS">ConnectionString</param>
        /// <param name="pTradeInput">Trade</param>
        /// <param name="pAction">Action</param>
        /// <param name="pMode">Mode</param>
        /// <param name="pAssessmentBasis">Assessment basis Type</param>
        /// <param name="pAssessmentBasis_DecValue">Assessment basis Value (ex. Qty value, Amount value...)</param>
        /// <param name="pAssessmentBasis_StrValue">Assessment basis unit (ex. null, Currency...)</param>
        /// RD 20140404 [19816] Gestion des dates Activation/désativation des DC (PL Report to NV)
        /// EG 20150706 [21021] Nullable<int> for bookId
        /// RD 20170208 [22815] pAssessmentBasis de type array
        /// FI 20170908 [23409] Modify
        // EG 20180307 [23769] Gestion dbTransaction
        public FeeRequest(string pCS, IDbTransaction pDbTransaction, TradeInput pTradeInput, string pAction, Cst.Capture.ModeEnum pMode,
            string[] pAssessmentBasis, decimal pAssessmentBasis_DecValue, string pAssessmentBasis_StrValue)
        {
            CS = pCS;
            DbTransaction = pDbTransaction;

            TradeInput = pTradeInput;

            Action = pAction;
            // Par défaut c'est InputTrade, pour pouvoir calculer les frais, notamment dans le cas de l'import.
            if (StrFunc.IsEmpty(Action))
                Action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade);
            
            // FI 20180502 [23926] Aappl à SetPermission
            SetPermission(pCS, DbTransaction);
            
            // FI 20180502 [23926] Aappl à SetDtReference
            SetDtReference(); 
            
            Mode = pMode;

            AssessmentBasis = pAssessmentBasis;
            AssessmentBasis_DecValue = pAssessmentBasis_DecValue;
            AssessmentBasis_StrValue = pAssessmentBasis_StrValue;

            Idc = DataDocument.CurrentProduct.GetCurrency1(CS, DbTransaction);
            Idc2 = DataDocument.CurrentProduct.GetCurrency2(CS, DbTransaction);

            // FI 20120104 [18172] Passage du paramètre null à ProductContainerBase.GetDerivativeContract
            // FI 20170908 [23409] _sqlDerivativeContract devient _contract
            // RD 20140404 [19816]
            //DataDocument.currentProduct.GetDerivativeContract(_cs, null, out _sqlDerivativeContract);
            // FI 20170908 [23409] Call of DataDocument.currentProduct.GetContract
            //DataDocument.currentProduct.GetDerivativeContract(_cs, null, SQL_Table.ScanDataDtEnabledEnum.Yes, this.dtReference, out _sqlContract);
            DataDocument.CurrentProduct.GetContract(CS, DbTransaction, SQL_Table.ScanDataDtEnabledEnum.Yes, this.DtReference, out Pair<Cst.ContractCategory, SQL_TableWithID> _contract);
            Contract = _contract;

            if (Contract == null)
            {
                //ex. d'un ReturnSwap
                DataDocument.CurrentProduct.GetMarket(CS, DbTransaction, out SQL_Market _sqlMarket);
                SqlMarket = _sqlMarket;
            }
            //PL 20231222 [WI789] Newness - Get Underlying Future AssetCategory on an Option on Future
            else if (_contract.First == Cst.ContractCategory.DerivativeContract)
            {
                SQL_DerivativeContract sql_DerivativeContract = ((SQL_DerivativeContract)_contract.Second);
                if ( (sql_DerivativeContract.Category == "O") 
                     && (sql_DerivativeContract.AssetCategory == Cst.UnderlyingAsset_ETD.Future.ToString()) 
                     && (sql_DerivativeContract.IdDcUnl > 0) )
                {
                    SQL_DerivativeContract unl_DerivativeContract = new SQL_DerivativeContract(pCS, sql_DerivativeContract.IdDcUnl);
                    if (unl_DerivativeContract.LoadTable())
                    {
                        sql_DerivativeContract.UnlFuture_AssetCategory = unl_DerivativeContract.AssetCategory;
                    }
                }
            }

            IParty[] partyBuyerAndPartySeller = DataDocument.GetPartyBuyerAndPartySeller();

            #region partyA => Alimentation de partyA avec l'acheur ou le vendeur

            PartyA = null;
            if (ArrFunc.Count(partyBuyerAndPartySeller) > 0)
            {
                IParty partyA = partyBuyerAndPartySeller[0];
                if (null != partyA)
                {
                    Nullable<int> bookId = DataDocument.GetOTCmlId_Book(partyA.Id);
                    TradeSideEnum side = (DataDocument.IsPartyBuyer(partyA) ? TradeSideEnum.Buyer : TradeSideEnum.Seller);
                    // EG 20150706 [21021]
                    PartyA = new Party(partyA.OTCmlId, Actor.RoleActor.COUNTERPARTY, bookId ?? 0, side, partyA.Id, partyA.PartyId);
                }
            }
            #endregion partyA

            #region partyB => Alimentation de partyB avec l'acheur ou le vendeur
            PartyB = null;
            if (ArrFunc.Count(partyBuyerAndPartySeller) > 1)
            {
                IParty partyB = partyBuyerAndPartySeller[1];
                if (null != partyB)
                {
                    Nullable<int> bookId = DataDocument.GetOTCmlId_Book(partyB.Id);
                    TradeSideEnum side = (DataDocument.IsPartyBuyer(partyB) ? TradeSideEnum.Buyer : TradeSideEnum.Seller);
                    // EG 20150706 [21021]
                    PartyB = new Party(partyB.OTCmlId, Actor.RoleActor.COUNTERPARTY, bookId ?? 0, side, partyB.Id, partyB.PartyId);
                }
            }
            #endregion partyB

            #region PartyBroker => Alimentation de PartyBroker avec les brokers
            int nbBrokers = ArrFunc.Count(DataDocument.BrokerPartyReference);
            if (DataDocument.BrokerPartyReferenceSpecified && (nbBrokers > 0))
            {
                PartyBroker = new OtherParty[nbBrokers];
                for (int i = 0; i < nbBrokers; i++)
                {
                    string partyId = DataDocument.BrokerPartyReference[i].HRef;
                    IParty otherParty = DataDocument.GetParty(partyId);
                    if (null != otherParty)
                        PartyBroker[i] = new OtherParty(otherParty.OTCmlId, RoleActor.BROKER, otherParty.Id, otherParty.PartyId);
                }
            }
            #endregion otherParty

            #region Check PartyA & PartyB
            if (PartyA == null)
                PartyA = new Party();
            if (PartyB == null)
                PartyB = new Party();
            #endregion
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Retourne l'id de la partyA
        /// <para>Retourne 0 si inconnu</para>
        /// </summary>
        /// <returns></returns>
        public int GetIdAPartyA()
        {
            int ret = 0;
            if (PartyA != null)
                ret = PartyA.m_Party_Ida;
            return ret;
        }

        /// <summary>
        /// Retourne l'id du book de la partyA
        /// <para>Retourne 0 si inconnu</para>
        /// </summary>
        public int GetIdBPartyA()
        {
            int ret = 0;
            if (PartyA != null)
                ret = PartyA.m_Party_Idb;
            return ret;
        }

        /// <summary>
        /// Retourne l'id de la partyB
        /// <para>Retourne 0 si inconnu</para>
        /// </summary>
        /// <returns></returns>
        public int GetIdAPartyB()
        {
            int ret = 0;
            if (PartyB != null)
                ret = PartyB.m_Party_Ida;
            return ret;
        }

        /// <summary>
        /// Retourne l'id du book de la partyB
        /// <para>Retourne 0 si inconnu</para>
        /// </summary>
        public int GetIdBPartyB()
        {
            int ret = 0;
            if (PartyB != null)
                ret = PartyB.m_Party_Idb;
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="sqlInstrCriteria"></param>
        /// <param name="sqlInstrCriteriaUnl"></param>
        /// <param name="sqlContractCriteria"></param>
        /// <param name="oParameters"></param>
        /// <returns></returns>
        /// FI 20170908 [23409] Modify  
        // EG 20180307 [23769] Gestion dbTransaction
        public string GetCriteriaJoinOnMATRIX(string pAlias,
            SQLInstrCriteria sqlInstrCriteria,
            SQLInstrCriteria sqlInstrCriteriaUnl,
            // FI 20170908 [23409] sqlDerivativeCriteria devient sqlContractCriteria
            //SQLDerivativeContractCriteria sqlDerivativeCriteria,
            SQLContractCriteria sqlContractCriteria,
            DataParameters parameters)
        {

            StrBuilder ret = new StrBuilder();

            //Critère sur DataDtEnabled
            ret += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(CS, pAlias, DtReference) + Cst.CrLf;

            //Critère sur idI
            ret += OTCmlHelper.GetSQLComment("Product/Instrument filter");
            ret += SQLCst.AND + sqlInstrCriteria.GetSQLRestriction2(pAlias, RoleGInstr.FEE);

            //Critère sur IdI_Underlying
            ret += OTCmlHelper.GetSQLComment("Underlying Instrument filter");
            ret += SQLCst.AND + sqlInstrCriteriaUnl.GetSQLRestriction2(pAlias, RoleGInstr.FEE);

            //Critère sur Contract 
            ret += OTCmlHelper.GetSQLComment("Market/Contract filter");
            // FI 20170908 [23409] sqlDerivativeCriteria devient sqlContractCriteria
            //sqlDerivativeCriteria.IsUseColumnExcept = false;
            //ret += SQLCst.AND + sqlDerivativeCriteria.GetSQLRestriction(CS, pAlias, RoleContractRestrict.FEE);
            sqlContractCriteria.IsUseColumnExcept = false;
            ret += SQLCst.AND + sqlContractCriteria.GetSQLRestriction(pAlias, RoleContractRestrict.FEE);

            //Critère sur Currency
            ret += OTCmlHelper.GetSQLComment("Currency filter");
            ret += SQLCst.AND + "( ({0}.IDC is null) or ({0}.IDC=@IDC) )" + Cst.CrLf;
            parameters.Add(new DataParameter(CS, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), Idc);

            //Critère sur AssetCategory
            // FI 20170908 [23409] use  of Contract
            //if (sqlDerivativeContract != null)
            if (Contract != null)
            {
                switch (Contract.First)
                {
                    case Cst.ContractCategory.DerivativeContract:
                        string assetCategory = ((SQL_DerivativeContract)Contract.Second).AssetCategory;
                        if (StrFunc.IsFilled(assetCategory))
                        {
                            parameters.Add(new DataParameter(CS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), assetCategory);

                            ret += OTCmlHelper.GetSQLComment("Asset category filter");
                            ret += SQLCst.AND + "( ({0}.ASSETCATEGORY is null) or ({0}.ASSETCATEGORY=@ASSETCATEGORY) )" + Cst.CrLf;
                        }
                        break;
                    case Cst.ContractCategory.CommodityContract: // FI 20170908 [23409] gestion de TRADABLETYPE
                        string tradaleType = ((SQL_CommodityContract)Contract.Second).TradableType;
                        if (StrFunc.IsFilled(tradaleType))
                        {
                            parameters.Add(new DataParameter(CS, "TRADABLETYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), tradaleType);
                            ret += OTCmlHelper.GetSQLComment("Tradable Type filter");
                            ret += SQLCst.AND + "( ({0}.TRADABLETYPE is null) or ({0}.TRADABLETYPE=@TRADABLETYPE) )" + Cst.CrLf;
                        }
                        break;

                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Contract type :{0} is not implemented", Contract.First.ToString()));
                }
            }

            //Critères sur PartyA et PartyB
            ret += GetCriteriaJoinOnACTOR(pAlias, false);

            return String.Format(ret.ToString(), pAlias);

        }

        /// <summary>
        /// Construction du filtre SQL sur la colonne ASSETCATEGORIE
        /// </summary>
        /// <param name="pConcatenedAssetCategory">Catégorie du SSJ. Attention, dans le cas d'une Option sur Future, il s'agit de la catégorie du SSJ du contrat Future SSJ, suffixée par "Future"</param>
        /// <param name="pAlias">Alias de la table SQL</param>
        /// <returns></returns>
        /// PL 20231220 [WI789] Add Method - New "*Future" values and combined values
        public string GetCriteriaOnASSETCATEGORY(string pConcatenedAssetCategory, string pAlias)
        {
            string ret = string.Empty;

            if (StrFunc.IsFilled(pConcatenedAssetCategory))
            {
                ret = OTCmlHelper.GetSQLComment("Asset category filter");

                int compatibleValue = 0;
                if (System.Enum.TryParse<Cst.UnderlyingAsset>(pConcatenedAssetCategory, out Cst.UnderlyingAsset assetCategoryEnum))
                {
                    //Identification d'existence de valeurs combinées (ex. Bond_BondFuture), compatible avec la catégorie composée (ex. BondFuture)
                    foreach (Cst.UnderlyingAsset item in Enum.GetValues(typeof(Cst.UnderlyingAsset)))
                    {
                        if ((item & assetCategoryEnum) != 0)
                            compatibleValue++;
                    }
                }
                if (compatibleValue <= 1)
                {
                     ret += SQLCst.AND + $"( ({pAlias}.ASSETCATEGORY is null) or ({pAlias}.ASSETCATEGORY = @ASSETCATEGORY) )" + Cst.CrLf;
                }
                else
                {
                    //Recherche d'une éventuelle valeur combinée (ex. Bond_BondFuture), en complément de la valeur elle-même (ex. Bond ou BondFuture)
                    #region Samples
                    //Pour un DC dont la catégorie de SSJ est 'Bond', le SQL construit sera :
                    //and ((feemx.ASSETCATEGORY is null) or ('_' + feemx.ASSETCATEGORY + '_' like '%[_]Bond[_]%'))
                    //Et il retournera les records FEEMATRIX/FEESCHEDULE qui ont pour critère 'Bond' ou 'Bond_BondFuture'

                    //Pour un DC dont la catégorie de SSJ est 'BondFuture', le SQL construit sera :
                    //and ((feemx.ASSETCATEGORY is null) or ('_' + feemx.ASSETCATEGORY + '_' like '%[_]BondFuture[_]%'))
                    //Et il retournera les records FEEMATRIX/FEESCHEDULE qui ont pour critère 'BondFuture' ou 'Bond_BondFuture'

                    //Pour un DC dont la catégorie de SSJ est 'Future', le SQL construit sera :
                    //and ((feemx.ASSETCATEGORY is null) or ('_' + feemx.ASSETCATEGORY + '_' like '%[_]Future[_]%'))
                    //Et il retournera les records FEEMATRIX/FEESCHEDULE qui ont pour critère 'Future' 
                    #endregion
                    string future = Cst.UnderlyingAsset_ETD.Future.ToString();
                    ret += SQLCst.AND + " ( " +
                        $"({pAlias}.ASSETCATEGORY is null)" +
                        $" or ('_'+{pAlias}.ASSETCATEGORY+'_' like '%[_]'+@ASSETCATEGORY+'[_]%')" +             //Valeur combinée
                        $" or (({pAlias}.ASSETCATEGORY = '{future}') and (@ASSETCATEGORY like '%{future}'))" +  //Valeur virtuelle ou Future
                        " )" + Cst.CrLf;
                }
            }
            else
            {
                ret += SQLCst.AND + $"( ({pAlias}.ASSETCATEGORY is null) )" + Cst.CrLf;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAlias"></param>
        /// <param name="pIsGetForFee"></param>
        /// <param name="sqlInstrCriteria"></param>
        /// <param name="sqlInstrCriteriaUnl"></param>
        /// <param name="sqlContractCriteria"></param>
        /// <param name="pSqlWhere_TradingClearing"></param>
        /// <returns></returns>
        /// FI 20170908 [23409] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        public string GetCriteriaJoinOnSCHEDULE(string pAlias,
             bool pIsGetForFee,
             SQLInstrCriteria sqlInstrCriteria,
             SQLInstrCriteria sqlInstrCriteriaUnl,
            // FI 20170908 [23409] sqlDerivativeCriteria devient sqlContractCriteria 
            //SQLDerivativeContractCriteria sqlDerivativeCriteria, 
             SQLContractCriteria sqlContractCriteria,
             string pSqlWhere_TradingClearing)
        {

            StrBuilder ret = new StrBuilder();

            //Critère sur IdI
            ret += sqlInstrCriteria.GetSQLRestriction2(pAlias, RoleGInstr.FEE);

            //Critère sur IdI_Underlying
            ret += SQLCst.AND + sqlInstrCriteriaUnl.GetSQLRestriction2(pAlias, RoleGInstr.FEE);

            //Critère sur Contrat 
            // FI 20170908 [23409] Use of sqlContractCriteria
            sqlContractCriteria.IsUseColumnExcept = false;
            ret += SQLCst.AND + sqlContractCriteria.GetSQLRestriction(pAlias, RoleContractRestrict.FEE);

            //Critère sur AssetCategory
            // FI 20170908 [23409] use  of Contract
            //if (sqlDerivativeContract != null)
            if (Contract != null)
            {

                switch (Contract.First)
                {
                    case Cst.ContractCategory.DerivativeContract:
                        //string assetCategory = sqlDerivativeContract.AssetCategory;
                        // PL 20231222 [WI789] Newness - Constitution d'une catégorie virtuelle composée également de celle du SSJ du Future, dans le cas d'une Option sur Future
                        string assetCategory = ((SQL_DerivativeContract)Contract.Second).UnlFuture_AssetCategory + ((SQL_DerivativeContract)Contract.Second).AssetCategory;
                        ret += GetCriteriaOnASSETCATEGORY(assetCategory, pAlias);
                        break;

                    case Cst.ContractCategory.CommodityContract:
                        string tradableType = ((SQL_CommodityContract)Contract.Second).TradableType;
                        if (StrFunc.IsFilled(tradableType))
                            ret += SQLCst.AND + StrFunc.AppendFormat("(({0}.TRADABLETYPE is null) or ({0}.TRADABLETYPE = @TRADABLETYPE))", pAlias) + Cst.CrLf;
                        else
                            ret += SQLCst.AND + StrFunc.AppendFormat("(({0}.TRADABLETYPE is null))", pAlias) + Cst.CrLf;
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Contract type :{0} is not implemented", Contract.First.ToString()));
                }
            }

            if (pIsGetForFee)
            {
                // RD 20170208 [22815]
                //if (StrFunc.IsFilled(AssessmentBasis))
                //{
                //    //PL 20141017
                //    ret += SQLCst.AND + StrFunc.AppendFormat("({0}.FEE1FORMULABASIS=@FEE1FORMULABASIS)", pAlias) + Cst.CrLf;                    
                //}
                if (ArrFunc.IsFilled(AssessmentBasis))
                    ret += SQLCst.AND + DataHelper.SQLColumnIn(CS, StrFunc.AppendFormat("{0}.FEE1FORMULABASIS", pAlias), AssessmentBasis, TypeData.TypeDataEnum.@string, false, true) + Cst.CrLf;

                //Critère sur StatusBusiness 
                ret += SQLCst.AND + StrFunc.AppendFormat("(({0}.IDSTBUSINESS is null) or ({0}.IDSTBUSINESS = @IDSTBUSINESS))", pAlias) + Cst.CrLf;

                // FI 20180423 [23924] Add
                Nullable<TrdTypeEnum> trdType = Product.GetTrdType() ;
                if (trdType.HasValue)
                    ret += SQLCst.AND + StrFunc.AppendFormat("(({0}.TRDTYPE is null) or ({0}.TRDTYPE = @TRDTYPE))", pAlias) + Cst.CrLf;
                else
                    ret += SQLCst.AND + StrFunc.AppendFormat("(({0}.TRDTYPE is null))", pAlias) + Cst.CrLf;


                //Critères sur Action 
                // RD 20120206 Ajout du critère ACTION sur le référentiel FEESCHEDULE
                ret += SQLCst.AND + StrFunc.AppendFormat("(({0}.ACTION is null) or " + DataHelper.SQLGetContains(CS, "{0}.ACTION", ";", "@ACTION") + ")", pAlias) + Cst.CrLf;

                //Critères sur Trading/Clearing - PL 20130118 New features
                if (StrFunc.IsFilled(pSqlWhere_TradingClearing))
                {
                    ret += StrFunc.AppendFormat(pSqlWhere_TradingClearing, pAlias);
                }
            }

            //Critère sur DataDtEnabled
            ret += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(CS, pAlias, DtReference) + Cst.CrLf;

            return ret.ToString();

        }

        /// <summary>
        /// Construit la clause de restriction SQL (Where) concernant les PartyA et PartyB.
        /// <para>ATTENTION: Veiller à correctement valoriser le paramètre en fonction pIsWithRegardPartyAOnly du besoin attendu.</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pAlias"></param>
        /// <param name="pIsWithRegardPartyAOnly">
        /// <para>True: Constitue une restriction sur la base du Dealer OU de la Counterparty, à l'égard du paramétrage en vigueur pour PartyA seulement</para>
        /// <para>      Ex. Utilisation pour la recherche de Fees/Brokerage</para>
        /// <para>False:Constitue une restriction sur la base du Dealer ET de la Counterparty, à l'égard du paramétrage en vigueur pour respectivement PartyA ET PartyB</para>
        /// <para>      Ex. Utilisation pour la recherche d'un Funding Rate sur les CFD</para>
        /// </param>
        /// <returns></returns>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180423 Analyse du code Correction [CA2200]
        public string GetCriteriaJoinOnACTOR(string pAlias, bool pIsWithRegardPartyAOnly)
        {
            string prefix = "FEE";
            if (pAlias == "m" || pAlias == "mrt")
            {
                //Tip: Alias des tables xxxxxMATRIX et xxxxxMATRIXRELTO où les colonnes se nomment TYPEPARTYx (sans préfixe FEE).
                prefix = string.Empty;
            }

            //Critères sur PartyA et PartyB 
            StrBuilder ret = new StrBuilder();

            //Acteur A 
            SQLActorBookCriteria sqlActorBookCriteriaA = new SQLActorBookCriteria(CS, DbTransaction, GetIdAPartyA(), GetIdBPartyA(), SQL_Table.ScanDataDtEnabledEnum.Yes)
            {
                ColumnTYPEPARTY = prefix + "TYPEPARTYA",
                ColumnIDPARTY = "IDPARTYA"
            };
            string restrictActorA = sqlActorBookCriteriaA.GetSQLRestrictionOnMandatory(CS, DbTransaction, pAlias, RoleActorBookRestrict.FEE);

            //Acteur B 
            SQLActorBookCriteria sqlActorBookCriteriaB = new SQLActorBookCriteria(CS, DbTransaction, GetIdAPartyB(), GetIdBPartyB(), SQL_Table.ScanDataDtEnabledEnum.Yes);
            if (pIsWithRegardPartyAOnly)
            {
                sqlActorBookCriteriaB.ColumnTYPEPARTY = prefix + "TYPEPARTYA";  //WARNING: On considère la Counterparty (PartyB) du trade comme une potentielle PartyA, 
                sqlActorBookCriteriaB.ColumnIDPARTY = "IDPARTYA";               //         d'où l'usage de "FEETYPEPARTYA" et "IDPARTYA", et du OR ci-dessous.
            }
            else
            {
                sqlActorBookCriteriaB.ColumnTYPEPARTY = prefix + "TYPEPARTYB";
                sqlActorBookCriteriaB.ColumnIDPARTY = "IDPARTYB";
            }
            string restrictActorB = sqlActorBookCriteriaB.GetSQLRestrictionOnMandatoryB(CS, DbTransaction, pAlias, RoleActorBookRestrict.FEE);

            if (pIsWithRegardPartyAOnly)
            {
                ret += OTCmlHelper.GetSQLComment("PartyA filter");
                ret += SQLCst.AND + Cst.CrLf + " (" + restrictActorA + SQLCst.OR + restrictActorB + ")" + Cst.CrLf;
            }
            else
            {
                ret += OTCmlHelper.GetSQLComment("PartyA/PartyB filter");
                ret += SQLCst.AND + Cst.CrLf + " (" + restrictActorA + SQLCst.AND + restrictActorB + ")" + Cst.CrLf;
            }

            return ret.ToString();
        }


        /// <summary>
        ///  Permissions associées à l'action  {Action}
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        // FI 20180502 [23926] Add Method
        // PL 20200217 [25207] Change DataType STRING instead of INT
        private void SetPermission(string pCs, IDbTransaction pDbTransaction)
        {
            //List<int> lstPermission = new List<int>();
            List<string> lstPermission = new List<string>();
            if (TradeInput.IsETDandAllocation
                && (Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade))
                && ExchangeTradedDerivative.TradeCaptureReport.TrdCapRptSideGrp[0].PositionEffectSpecified
                )
            {
                bool isPositionEffectClose = (ExchangeTradedDerivative.TradeCaptureReport.TrdCapRptSideGrp[0].PositionEffect == FixML.v50SP1.Enum.PositionEffectEnum.Close);
                if (isPositionEffectClose)
                {
                    //Search on Clearing action (Close)
                    lstPermission.Add(PermissionTools.GetIdPermission(pCs, pDbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE_CLR), PermissionEnum.Create).ToString());
                }
            }

            //lstPermission.Add(PermissionTools.GetIdPermission(pCs, pDbTransaction, Action, PermissionEnum.Create).ToString());
            lstPermission.Add(PermissionTools.GetIdPermissionFromView(pCs, pDbTransaction, Action, PermissionEnum.Create));

            IdPermission = lstPermission.ToArray();
        }
       
        /// <summary>
        ///  Affecte la property DtReference
        /// </summary>
        /// FI 20180502 [23926] Add Method (ce code était avant présent dans la property dtReference)
        private void SetDtReference()
        {
            DtReference = DateTime.MinValue;

            if (IsFeeOptionSettlementAction)
            {
                // EG 20151102 [21465]
                if (null == TradeInput.tradeDenOption)
                    throw new InvalidOperationException("tradeDenOption is null in TradeInput");

                DtReference = TradeInput.tradeDenOption.date.DateValue;

            }
            // EG 20150708 [21103] New
            else if (IsSafekeeping)
            {
                DtReference = TradeInput.safekeepingAction.dtBusiness.DateValue;
            }
            else
            {
                // RD 20110429 [17438]
                // Pour les ETD c'est la ClearingBusinessDate qui fait office de dtReference
                if ((Product.IsExchangeTradedDerivative) || (Product.IsStrategy))
                {
                    IExchangeTradedDerivative etd = ExchangeTradedDerivative;
                    if (null != etd)
                        DtReference = etd.TradeCaptureReport.ClearingBusinessDate.DateTimeValue;
                }
                else
                    DtReference = TradeInput.DataDocument.TradeHeader.TradeDate.DateValue;
            }
        }

        
        #endregion Methods
    }
    
    /// <summary>
    /// Substitution d'une ligne de frais 
    /// </summary>
    public class FeeRequestSubstitute
    {
        /// <summary>
        /// Représente le couple condition, barème déterminé par Spheres® lors d'un calcul auto
        /// </summary>
        public Pair<int, int> source;

        /// <summary>
        /// Représente le barème de substitution qui doit être appliqué à la place du barème déterminé par Spheres® lors d'un calcul auto
        /// </summary>
        public int targetIdFeeSchedule;
    }
}
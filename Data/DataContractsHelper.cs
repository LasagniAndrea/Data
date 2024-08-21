using System.Collections.Generic;
using System.Data;
using System;
//
using EFS.ACommon;

namespace EFS.ApplicationBlocks.Data
{
    /// <summary>
    /// DataContracts set
    /// </summary>
    // EG [33415/33420] Intégration de nouveaux sous-jacents EQUITY suite à CA
    // EG 20180803 PERF Upd
    public enum DataContractResultSets
    {
        /// <summary>
        /// Dataset containing all the DC, DA and 1ASSET candidate to CA
        /// </summary>
        CA_DERIVATIVECONTRACT,
        CA_CORPOEVENTCONTRACT,
        CA_DERIVATIVEATTRIB,
        CA_CORPOEVENTDATTRIB,
        CA_ASSET,
        // EG [33415/33420]
        CA_ASSET_EQUITY,
        CA_ASSET_EQUITY_RDCMK,
        CA_CORPOEVENTASSET,
        /// <summary>
        /// Dataset containing all the role relationships of an actor
        /// </summary>
        ACTORRELATIONSHIP,
        /// <summary>
        /// Entity/market couples Dataset relative to a given actor entity
        /// </summary>
        ENTITYMARKETWITHCSS,
        /// <summary>
        /// Paramètres généraux des méthodes de calculs de déposit
        /// </summary>
        /// PM 20160404 [22116] Ajout IMMETHODPARAMETER
        IMMETHODPARAMETER,
        /// <summary>
        /// Dataset containing all the books and the main margin parameters (book IMR, gross/net evaluation) of a MARGINREQOFFICE actor
        /// </summary>
        MARGINREQOFFICEBOOKSANDPARAMETERS,
        /// <summary>
        /// supplementary margin parameter per clearing house of a MARGINREQOFFICE actor  
        /// </summary>
        CLEARINGORGPARAMETER,
        /// <summary>
        /// supplementary margin parameter per market of a MARGINREQOFFICE actor  
        /// </summary>
        EQUITYMARKETPARAMETER,
        /// <summary>
        /// Dataset containing all the roles od one actor
        /// </summary>
        ACTORROLES,
        /// <summary>
        /// Dataset containing all the books of an actor
        /// </summary>
        BOOKS,
        ///// <summary>
        ///// Dataset containing all the previous evaluations affected a book at a businessdate
        ///// PM 20131010 [19046] DEPRECATED
        ///// </summary>
        //RISKRESULTS,
        /// <summary>
        /// Dataset containing all the previous evaluations affected on a CSS at a businessdate
        /// </summary>
        // PM 20131009 [19046] Added
        RISKALLRESULTS,
        #region Position
        /// <summary>
        /// Dataset containing all the trades that result open at a specific business date 
        ///     and whom dealer is an actor depending of the current entity
        /// </summary>
        TRADEALLOCATIONSACTIONSWITH,
        /// <summary>
        /// Dataset containing all the being delivered quantities performed 
        ///     on the elements of the trades dataset (TRADEALLOCATIONS) with physical settlement
        /// </summary>
        PHYSICALSETTLEMENTS,
        /// <summary>
        /// Dataset contenant les positions en livraison physique
        /// </summary>
        PHYSICALDELIVERY,
        #endregion Position
        #region Trade value
        /// <summary>
        /// Dataset containing the trade amount
        /// </summary>
        /// PM 20170313 [22833] Ajout
        TRADEVALUE,
        /// <summary>
        /// Dataset containing the first date a trade have been input
        /// </summary>
        /// PM 20170313 [22833] Ajout
        /// PM 20170602 [23212] N'est plus utilisé
        //TRADEVALUEMINDATE,
        /// <summary>
        /// Dataset for zero trade amount for account with agreement
        /// </summary>
        /// PM 20190418 [24628] Ajout TRADEVALUEAGREEMENT
        TRADEVALUEAGREEMENT,
        #endregion Trade value
        #region No initial margin trades
        /// <summary>
        /// Trades sans calcul de déposit
        /// </summary>
        // PM 20221212 [XXXXX] Ajout gestion trades sans calcul de déposit 
        NOINITIALMARGINTRADE,
        #endregion No initial margin trades
        /// <summary>
        /// Dataset containing the margin track element relative to a RiskPerformnce process instance
        /// </summary>
        PROCESSMARGINTRACK,
        /// <summary>
        /// SQl insert into table TRADEMARGINTRACK containing calculation sheets
        /// </summary>
        INSERTTRADEMARGINTRACK,
        /// <summary>
        /// SQl select from table TRADEMARGINTRACK containing calculation sheets
        /// </summary>
        // PM 20160922 Pour VS2013
        SELECTTRADEMARGINTRACK,
        /// <summary>
        /// Dataset containing the next maturity date for a given derivative contract and a business date 
        /// </summary>
        GETMINMATURITY,
        /// <summary>
        /// Dataset returning a single asset id for a given derivative contract and a given maturity date
        /// </summary>
        GETASSETMINMATURITY,
        /// <summary>
        /// dataset returning the evaluation status for all the margin reg office actors, at a specific business date, for a given clearing house 
        /// and entity
        /// </summary>
        IMREQUESTELEMENTS,
        /// <summary>
        /// Insert a single asset id (along with its own id dc) in position inside the im_assetetd table
        /// </summary>
        INSERTIMASSETETD,
        /// <summary>
        /// delete all the asset in position related to the current application instance (identified by a GUID)
        /// </summary>
        TRUNCATEIMASSETETD,
        /// <summary>
        /// Insert the id actor/book for each actor/book composing the current entity hierarchy
        /// </summary>
        /// <remarks>20120712 MF Ticket 18004 </remarks>
        INSERTIMACTOR,
        /// <summary>
        /// delete all the actor/book hierarchy (entity-side) related to the current application instance (identified by a GUID)
        /// </summary>
        /// <remarks>20120712 MF Ticket 18004 </remarks>
        TRUNCATEIMACTOR,
        /// <summary>
        /// Insert the id actor for each actor (Query1)
        /// </summary>
        /// FI 202110211 [XXXXX] add
        INSERTIMACTORPOS1,
        /// <summary>
        /// Insert the id actor for each actor (Query2)
        /// </summary>
        /// FI 202110211 [XXXXX] add
        INSERTIMACTORPOS2,
        /// <summary>
        /// Insert the id actor for each actor (Query3)
        /// </summary>
        /// FI 202110211 [XXXXX] add
        INSERTIMACTORPOS3,
        /// <summary>
        /// Insert the id actor for each actor (Query4)
        /// </summary>
        /// FI 202110211 [XXXXX] add
        INSERTIMACTORPOS4,
        /// <summary>
        /// Insert the id actor for each actor (Query5)
        /// </summary>
        /// FI 202110211 [XXXXX] add
        INSERTIMACTORPOS5,
        /// <summary>
        /// delete all the actor in table IMACTORPOS related to the current application instance (identified by a GUID)
        /// </summary>
        /// <remarks>RD 20130419</remarks>
        TRUNCATEIMACTORPOS,

        #region Common (used at evaluation the method level by any method)
        /// <summary>
        /// DataSet containing the parameters for the delivery deposit, 
        /// calculated for any derivative contract having the IMDELIVERY parameter specified.
        /// </summary>
        ASSETDELIVERY,

        /// <summary>
        /// Dataset containing the stocks (type Equity) used to cover short call an short futures contracts, 
        /// (having underlying type stock Equity)
        /// </summary>
        EQUITYSTOCKSCOVERAGE,

        /// <summary>
        /// Parametres des paniers d'equity en vue de leur utilisation pour reduire une position ETD
        /// </summary>
        // PM 20201028 [25570][25542] Ajout EQUITYBASKETSETTING
        EQUITYBASKETSETTING,

        /// <summary>
        /// Dataset containing the Asset expanded parameters, for the all method
        /// </summary>
        ASSETEXPANDED_ALLMETHOD,

        /// <summary>
        /// Dataset containing the underlying Asset expanded parameters, for the all method
        /// </summary>
        UNDERLYNGASSETEXPANDED_ALLMETHOD,

        /// <summary>
        /// Dataset containing the underlying Future Asset expanded parameters, for the all method
        /// </summary>
        // PM 20230929 [XXXXX] Ajout
        UNDERLYNGASSETFUTUREEXPANDED_ALLMETHOD,
        #endregion Common

        #region Custom

        /// <summary>
        /// Dataset containing all the derivative contracts, relative to a specific assets set
        /// </summary>
        CONTRACTASSET_CUSTOMMETHOD,
        /// <summary>
        /// Dataset containing all the parameters for the RISK "Custom" method, relative to a specific derivative contracts set
        /// </summary>
        PARAMS_CUSTOMMETHOD,

        #endregion

        #region TIMS IDEM

        /// <summary>
        /// Dataset containing the contract parameters (and underlying quotes) for the TIMS IDEM method, 
        /// related to the derivative contracts inserted inside of the referential
        /// </summary>
        CLASS_TIMSIDEMMETHOD,
        /// <summary>
        /// Dataset containing the asset parameters (etd quotes, risk array) for the TIMS IDEM method, related to the assets etd in position
        /// </summary>
        RISKARRAY_TIMSIDEMMETHOD,
        /// <summary>
        /// Dataset containing the security position for the current working day. Teh dataset is valid for the
        /// </summary>
        POSITIONACTIONSCCG,
        /// <summary>
        /// Dataset containing the contract parameters, for the TIMS IDEM method (AGREX)
        /// </summary>
        CONTRACTPARAM_TIMSIDEMMETHOD,
        /// <summary>
        /// Dataset contenant les convertion d'assets pour la livraison AGREX
        /// </summary>
        ASSETDELIVERY_TIMSIDEMMETHOD,
        /// <summary>
        /// Insertion dans la table IMASSET_ETD des assets pour la livraison AGREX
        /// </summary>
        INSERTIMASSETETD_TIMSIDEMMETHOD,
        #endregion TIMS IDEM

        #region TIMS EUREX

        /// <summary>
        /// Dataset containing the contract parameters, as well as the group/class hierarchy, for the TIMS EUREX method
        /// </summary>
        CONTRACT_TIMSEUREXMETHOD,
        /// <summary>
        /// Dataset containing the maturity parameters, for the TIMS EUREX method
        /// </summary>
        MATURITY_TIMSEUREXMETHOD,
        /// <summary>
        /// Dataset containing the asset parameters as well as the market quotes, for the TIMS EUREX method
        /// </summary>
        ASSET_TIMSEUREXMETHOD,
        /// <summary>
        /// Dataset containing the volatility parameters as well as risk array values, for the TIMS EUREX method
        /// </summary>
        VOLATILITY_TIMSEUREXMETHOD,
        /// <summary>
        /// Dataset containing the exchange rate, for the TIMS EUREX method
        /// </summary>
        FXRATE_TIMSEUREXMETHOD,

        #endregion TIMS EUREX

        #region SPAN
        /// <summary>
        /// Dataset containing the Clearing House specific parameters, for the SPAN method
        /// </summary>
        // PM 20160404 [22116] N'est plus utilisé
        //CLEARINGORG_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Exchange Complex parameters, for the SPAN method
        /// </summary>
        EXCHANGECOMPLEX_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Exchange parameters, for the SPAN method
        /// </summary>
        EXCHANGE_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Currency parameters, for the SPAN method
        /// </summary>
        CURRENCY_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Currency Conversion parameters, for the SPAN method
        /// </summary>
        CURCONV_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Inter Spread parameters, for the SPAN method
        /// </summary>
        INTERSPREAD_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Inter Spread Leg parameters, for the SPAN method
        /// </summary>
        INTERLEG_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Combined Group parameters, for the SPAN method
        /// </summary>
        COMBINEDGROUP_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Contract Group parameters, for the SPAN method
        /// </summary>
        CONTRACTGROUP_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Contract parameters, for the SPAN method
        /// </summary>
        CONTRACT_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Delivery Months parameters, for the SPAN method
        /// </summary>
        DELIVERYMONTH_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Maturity Tier parameters, for the SPAN method
        /// </summary>
        MATURITYTIER_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Intra Contract Group Spread parameters, for the SPAN method
        /// </summary>
        INTRASPREAD_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Intra Contract Group Spread Leg parameters, for the SPAN method
        /// </summary>
        INTRALEG_SPANMETHOD,
        /// <summary>
        /// Dataset containing the Maturity parameters, for the SPAN method
        /// </summary>
        MATURITY_SPANMETHOD,
        /// <summary>
        /// Dataset containing the asset parameters, for the SPAN method
        /// /// </summary>
        RISKARRAY_SPANMETHOD,
        /// <summary>
        /// Dataset pour les volumes de marché pour le calcul du Concentration Risk Margin ECC.
        /// </summary>
        // PM 20190801 [24717] Ajout MARKETVOLUME_ECCSPANMETHOD
        MARKETVOLUME_ECCSPANMETHOD,
        /// <summary>
        /// Datasetpour les Internal Id des Derivative Contracts des Combined Commodity Stress Group.
        /// </summary>
        // PM 20190801 [24717] Ajout STRESSCOMBCOMCONTRACT_ECCSPANMETHOD
        STRESSCOMBCOMCONTRACT_ECCSPANMETHOD,
        #endregion

        #region CBOE MARGIN
        /// <summary>
        /// Dataset containing the contract parameters, for the CBOE Margin method
        /// </summary>
        CONTRACTPARAM_CBOEMARGINMETHOD,
        /// <summary>
        /// Dataset containing the asset parameters, for the CBOE Margin method
        /// </summary>
        ASSETEXPANDEDPARAM_CBOEMARGINMETHOD,
        #endregion

        #region MEFFCOM2 MARGIN
        /// <summary>
        /// Dataset containing the asset parameters, for the MEFFCOM2 method
        /// </summary>
        CONTRACTASSET_MEFFCOM2METHOD,
        /// <summary>
        /// Dataset containing the valuation array parameters, for the MEFFCOM2 method
        /// </summary>
        VALUATIONARRAY_MEFFCOM2METHOD,
        /// <summary>
        /// Dataset containing the inter spread parameters, for the MEFFCOM2 method
        /// </summary>
        INTERSPREAD_MEFFCOM2METHOD,
        /// <summary>
        /// Dataset containing the intra spread parameters, for the MEFFCOM2 method
        /// </summary>
        INTRASPREAD_MEFFCOM2METHOD,
        /// <summary>
        /// Dataset containing the asset deltas parameters, for the MEFFCOM2 method
        /// </summary>
        DELTAARRAY_MEFFCOM2METHOD,
        /// <summary>
        /// Dataset containing the asset theoretical prices parameters, for the MEFFCOM2 method
        /// </summary>
        THEORPRICEARRAY_MEFFCOM2METHOD,
        #endregion

        #region PRISMA MARGIN
        /// <summary>
        /// Dataset contenant les liquidation groups, de la méthode Prisma
        /// </summary>
        LIQUIDATIONGROUP_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les liquidation group splits, de la méthode Prisma
        /// </summary>
        LIQUIDATIONGROUPSPLIT_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les risk measure set de chaque liquidation group splits, de la méthode Prisma
        /// </summary>
        RISKMEASURESET_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les market capacities, de la méthode Prisma
        /// </summary>
        MARKETCAPACITY_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les liquidity factors, de la méthode Prisma
        /// </summary>
        LIQUIDITYFACTOR_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les taux de change, de la méthode Prisma
        /// </summary>
        EXCHANGERATE_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les taux de change des risk measure set, de la méthode Prisma
        /// </summary>
        EXCHANGERATERMS_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les paramétres des assets, expirations, products, de la méthode Prisma
        /// </summary>
        ASSET_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les paramétres des liquidation group split des assets
        /// </summary>
        ASSETLIQUIDGROUPSPLIT_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les paramétres des risk measure set des liquidation group split des assets
        /// </summary>
        ASSETRMSLGS_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les paramétres des scénari de prix des assets
        /// </summary>
        ASSETPRICESCENARIO_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les paramétres des compression erreurs des assets
        /// </summary>
        ASSETCOMPRESSIONERROR_PRISMAMETHOD,
        /// <summary>
        /// Dataset contenant les paramétres des valuation arrays des assets
        /// </summary>
        ASSETVAR_PRISMAMETHOD,
        #endregion

        #region VCTFILE_OMXMETHOD
        /// <summary>
        /// Dataset containing the the vector file parameters, for the OMX RCaR method
        /// </summary>
        VCTFILE_OMXMETHOD,
        #endregion

        #region IMSM
        // PM 20170313 [22833] Ajout IMSM : SECURITYADDON_IMSMMETHOD & HOLIDAYADJUSTMENT_IMSMMETHOD
        /// <summary>
        /// Dataset containing the Holiday Adjustments parameters, for the IMSM method
        /// </summary>
        HOLIDAYADJUSTMENT_IMSMMETHOD,
        /// <summary>
        /// Dataset containing the Security Addon parameters, for the IMSM method
        /// </summary>
        SECURITYADDON_IMSMMETHOD,
        /// <summary>
        /// Dataset containing the Margin Parameter for the CESM (Current Exposure Spot Market)
        /// </summary>
        /// 20170808 [23371] Ajout
        CESMPARAMETER_IMSMMETHOD,
        /// <summary>
        /// Dataset containing the agreement for ECC Spot
        /// </summary>
        /// PM 20170602 [23212] Ajout
        ECCSPOTAGREEMENT_IMSMMETHOD,
        /// <summary>
        /// Dataset containing the information about Commodity Contract
        /// </summary>
        /// 20170808 [23371] Ajout
        COMMODITYCONTRACTPARAMETER_IMSMMETHOD,
        #endregion

        #region Cash Balance Interest
        /// <summary>
        /// Dataset containing the interest rules
        /// </summary>
        INTERESTRULE_CBI,
        /// <summary>
        /// Dataset containing the Cash Payment amounts
        /// </summary>
        FLOWCASHPAYMENT_CBI,
        /// <summary>
        /// Dataset containing the Cash Balance amounts
        /// </summary>
        FLOWCASHBALANCE_CBI,
        /// <summary>
        /// Dataset containing the Cash Covered Initial Margin amounts
        /// </summary>
        FLOWCASHCOVERED_CBI,
        /// <summary>
        /// Dataset containing the previous calculated cash interest "trade"
        /// </summary>
        PREVIOUSTRADE_CBI
        #endregion
    }

    /// <summary>
    /// Helper Class containing a collection of the SQL requests
    ///     used to 
    ///     <list type="">
    ///     <item>load a specific type (<seealso cref="DataContractResultSets"/>) of datacontracts objects.</item>
    ///     </list>
    ///     
    /// </summary>
    /// <remarks>
    /// <list type="">
    /// <item>
    /// All the SQL queries defined inside of this helper class 
    ///     must be expressed using a Microsoft SQL server compliant syntax (when SQL extentions are needed).
    /// The translaction to other SQL extention languages (e.g. Oracle...) must be done using the methods of the DataHelper T class.
    /// e.g. if you have some XQuery inside of your SQL request, use Microsoft SQL Server XQuery syntax to define your request,
    ///     then pass your query to the DataHelper T.XQueryTransform method in order to have it translated.
    /// </item>
    /// <item>
    /// Do not define any XMLNAMESPACEs inside your query but pass your original query to the DataHelper T.XQueryTransform method  in order
    ///     to add them properly.
    /// </item>
    /// </list>
    /// </remarks>
    // EG [33415/33420] Modification QUERYCA_DERIVATIVECONTRACT suite à trigger UBRR_DERIVATIVECONTRACT (Instead of)
    // EG [33415/33420] Intégration de nouveaux sous-jacents EQUITY suite à CA
    public static class DataContractHelper
    {
        /// <summary>
        /// Représente une requête paramétré
        /// </summary>
        public struct DataContractParameter
        {
            public string Query;

            public CommandType Type;

            public DataParameter[] Parameters;
        }

        # region QUERIES
        #region referentials
        // RD 20150813 [21241] Ne charger que les DC Activés
        // EG 20160105 [34091] Add dbo
        // EG 20220621 [34623] Modifications des queries pour CA sur multi-entité (Maj Restriction DTDISABLED)
        const string QUERYCA_DERIVATIVECONTRACT = @"
            select cec.IDCEC as CEC_IDCE, dc.* 
            from dbo.CORPOEVENTCONTRACT cec
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = cec.IDDC)
            where (cec.IDCE = @IDCE) and (cec.IDA_ENTITY = @IDA_ENTITY) and (cec.IDDC = @IDDC) and 
            (dc.DTENABLED < @EFFECTIVEDATE) and (isnull(dc.DTDISABLED, @EFFECTIVEDATE) >= @EFFECTIVEDATE)";

        // RD 20150813 [21241] Ne charger que les DERIVATIVEATTRIB Activés
        // EG 20160105 [34091] Add dbo
        // EG 20220621 [34623] Modifications des queries pour CA sur multi-entité (Maj Restriction DTDISABLED)
        const string QUERYCA_DERIVATIVEATTRIB = @"
            select ceda.IDCEDA, da.*
            from dbo.CORPOEVENTDATTRIB ceda
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = ceda.IDDA)
            where (ceda.IDCE = @IDCE) and (ceda.IDA_ENTITY = @IDA_ENTITY) and (ceda.IDDC = @IDDC) and 
            (da.DTENABLED < @EFFECTIVEDATE) and (isnull(da.DTDISABLED, @EFFECTIVEDATE) >= @EFFECTIVEDATE)";

        // RD 20150813 [21241] Ne charger que les ASSET Activés
        // EG 20160105 [34091] Add dbo
        // EG 20230106 [34623][26212][WI523] Upd Restriction DTDISABLED
        const string QUERYCA_ASSET = @"select cea.IDCEA, asset.*
            from dbo.CORPOEVENTASSET cea
            inner join dbo.ASSET_ETD asset on (asset.IDASSET = cea.IDASSET)
            where (cea.IDCE = @IDCE) and (cea.IDA_ENTITY = @IDA_ENTITY)  and (cea.IDDC = @IDDC) and 
            (asset.DTENABLED < @EFFECTIVEDATE) and (isnull(asset.DTDISABLED, @EFFECTIVEDATE) >= @EFFECTIVEDATE)";

        // EG 20160105 [34091] Replace @LSTISINCODE by ##LSTISINCODE##
        const string QUERYCA_ASSET_EQUITY = @"
            select eqty.*
            from dbo.ASSET_EQUITY eqty
            where (eqty.IDM = @IDM) and ((eqty.IDASSET = @IDASSET) or (eqty.ISINCODE in (##LSTISINCODE##)))";

        // EG 20160105 [34091] Replace @LSTISINCODE by ##LSTISINCODE##
        // EG 20160105 [34091] Add dbo
        const string QUERYCA_ASSET_EQUITY_RDCMK = @"
            select eqty_rd.*
            from dbo.ASSET_EQUITY eqty
            inner join dbo.ASSET_EQUITY_RDCMK eqty_rd on (eqty_rd.IDASSET = eqty.IDASSET)
            where (eqty.IDM = @IDM) and ((eqty.IDASSET = @IDASSET) or (eqty.ISINCODE in (##LSTISINCODE##)))";

        // EG 20160105 [34091] Add dbo, Delete ';' end of query
        // EG 20220621 [34623] Modifications des queries pour CA sur multi-entité
        // Récupération des IDs des contrats résultant d'un CA déjà exécutée sur une entité
        // EG 20230106 [34623][26212][WI523] Upd where clause (IDDCEXADJ is not null)
        // EG 20220206 [26212] [WI565] Upd where clause (cec2.IDA_ENTITY <> @IDA_ENTITY)
        const string QUERYCA_CORPOEVENTCONTRACT = @"
            select distinct cec.IDCEC, cec.IDDC, cec.IDDCEX, cec.IDDCEXADJ , cec.IDDCRECYCLED , cec.READYSTATE, 
            cec2.IDDCEX as IDDCEX_EXECUTED, cec2.IDDCEXADJ as IDDCEXADJ_EXECUTED , cec2.IDDCRECYCLED as IDDCRECYCLED_EXECUTED  
            from dbo.CORPOEVENTCONTRACT cec
            left join dbo.CORPOEVENTCONTRACT cec2 on (cec2.IDA_ENTITY <> @IDA_ENTITY) and (cec2.IDCE = cec.IDCE) and (cec2.IDDC = cec.IDDC) and ((cec2.IDDCEX is not null) or (cec2.IDDCEXADJ is not null))
            where (cec.IDCE = @IDCE) and (cec.IDA_ENTITY = @IDA_ENTITY)";

        // EG 20160105 [34091] Add dbo, Delete ';' end of query
        // EG 20220621 [34623] Modifications des queries pour CA sur multi-entité
        // Récupération des IDs des dAttrib résultant d'un CA déjà exécutée sur une entité
        // EG 20220206 [26212] [WI565] Upd where clause (ceda2.IDA_ENTITY <> @IDA_ENTITY)
        const string QUERYCA_CORPOEVENTDATTRIB = @"
            select distinct ceda.IDCEDA, ceda.IDDA, ceda.IDDAEX, ceda.IDDAEXADJ, ceda.READYSTATE, 
            ceda2.IDDAEX as IDDAEX_EXECUTED, ceda2.IDDAEXADJ as IDDAEXADJ_EXECUTED 
            from dbo.CORPOEVENTDATTRIB ceda
            left join dbo.CORPOEVENTDATTRIB ceda2 on (ceda2.IDA_ENTITY <> @IDA_ENTITY) and (ceda2.IDCE = ceda.IDCE) and (ceda2.IDDA = ceda.IDDA) and ((ceda2.IDDAEX is not null) or (ceda2.IDDAEXADJ is not null))
            where (ceda.IDCE = @IDCE) and (ceda.IDA_ENTITY = @IDA_ENTITY)";

        // EG 20160105 [34091] Add dbo, Delete ';' end of query
        // EG 20220621 [34623] Modifications des queries pour CA sur multi-entité
        // Récupération des IDs des assets résultant d'un CA déjà exécutée sur une entité
        // EG 20220206 [26212] [WI565] Upd where clause (cea2.IDA_ENTITY <> @IDA_ENTITY)
        const string QUERYCA_CORPOEVENTASSET = @"
            select distinct cea.IDCEA, cea.IDASSET, cea.IDASSETEX, cea.IDASSETEXADJ, cea.READYSTATE,
            cea2.IDASSETEX as IDASSETEX_EXECUTED, cea2.IDASSETEXADJ as IDASSETEXADJ_EXECUTED 
            from dbo.CORPOEVENTASSET cea
            left join dbo.CORPOEVENTASSET cea2 on (cea2.IDA_ENTITY <> @IDA_ENTITY) and (cea2.IDCE = cea.IDCE) and (cea2.IDASSET = cea.IDASSET) and ((cea2.IDASSETEX is not null) or (cea2.IDASSETEXADJ is not null))
            where (cea.IDCE = @IDCE) and (cea.IDA_ENTITY = @IDA_ENTITY)";

        // RD 20130502 [18575] Utilisation de la table BOOKACTOR_R
        // PM 20130919 [18970] Ajout en UNION des books sur lesquels un déposit a déjà été calculé pour la date de bourse courante
        // PM 20140110 [19469] Dans 2ème union, ajout jointure avec ASSET_ETD, DERIVATIVEATTRIB, IMDELIVERY (left), EVENTCLASS ecStl (left)
        //                     et condition dans clause where pour garder les positions possiblement en livraison
        //                     (Utilisation de la valeur 100 en dure au lieu d'un paramètre @MAXDELIVERYDAY, pour indiquer le nombre maximum de jour avant livraison final, afin d'éviter un refactoring de l'execution de cette requête).
        // EG 20140221 [19575][19666] Add TRADEINSTRUMENT join and @IDA_CSS parameter
        // EG 20140521 [19972] Refactoring DTENABLED/DTDISABLED
        // PM 20170313 [22833] Ajout des acteurs pour les trades commodity spot sur gas et electricité
        // EG 20180803 PERF (IMACTORPOS_{buildTableId}_W et TRADEALLOC_{buildTableId}_W sur la base de IMACTORPOS_MODEL, TRADEALLOC_MODEL -> SQLServer Table pour une unique évaluation)
        // EG 20181010 PERF Add DTOUT
        // EG 20181119 PERF Correction post RC
        // FI 20181126 [24338] Add join inner join dbo.BOOK bc on (bc.IDB = ti.IDB_CLEARER) (pour exlcure les trades sans book Clearer qui existent chez XI)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        // PM 20191204 [25089] Ajout acteur avec MasterAgreement ECCPowerAndNaturalGas
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)

        // FI 20210118 [XXXXX] use QUERYINSERTACTORPOS_WITHTRADEALLOC and QUERYINSERTACTORPOS_SELECT const
        const string QUERYINSERTACTORPOS1_SQL =
            QUERYINSERTACTORPOS_WITHTRADEALLOC + Cst.CrLf +
            "insert into dbo.IMACTORPOS_MODEL (IDA)" + Cst.CrLf +
            QUERYINSERTACTORPOS_SELECT1;

        // EG 20180803 PERF (IMACTORPOS_{buildTableId}_W sur la base de IMACTORPOS_MODEL pour une unique évaluation dans la clause WITH)
        // EG 20181010 PERF Add Hints, DTOUT
        // EG 20181108 PERF Add Pipe {0}|{1} for Hints
        // EG 20181119 PERF Correction post RC
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        // PM 20191204 [25089] Ajout acteur avec MasterAgreement ECCPowerAndNaturalGas
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // FI 20210118 [XXXXX] use QUERYINSERTACTORPOS_WITHTRADEALLOC and QUERYINSERTACTORPOS_SELECT const
        const string QUERYINSERTACTORPOS1_ORA =
            "insert into dbo.IMACTORPOS_MODEL (IDA)" + Cst.CrLf +
            QUERYINSERTACTORPOS_WITHTRADEALLOC + Cst.CrLf +
            QUERYINSERTACTORPOS_SELECT1;

        // FI 20210118 [XXXXX] Add
        const string QUERYINSERTACTORPOS_WITHTRADEALLOC = @"
            with TRADEALLOC as
            (
                select tr.IDB_DEALER, tr.IDB_CLEARER, tr.IDT, tr.IDASSET, tr.QTY
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
                inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
                where (tr.DTBUSINESS <= @DTBUSINESS) and ((tr.DTOUT is null) or (tr.DTOUT > @DTBUSINESS)) and
                (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN)
            )";

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210118 [XXXXX] Add
        /// FI 20200118 [XXXXX] add restriction sur POSACTION.DTOUT
        const string QUERYINSERTACTORPOS_SELECT1 =
            @"select distinct b.IDA_ACTOR
            from dbo.BOOKACTOR_R b
            inner join
            ( 
                select alloc.IDB_DEALER, alloc.IDB_CLEARER 
                from TRADEALLOC alloc
                left outer join
                (
                    select pad.IDT_BUY as IDT, sum(isnull(pad.QTY,0)) as QTY_BUY, 0 as QTY_SELL
                    from TRADEALLOC alloc 
                    inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                    where (pa.DTBUSINESS <= @DTBUSINESS) and ((pa.DTOUT is null) or (pa.DTOUT > @DTBUSINESS))  and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))     
                    group by pad.IDT_BUY
        
                    union all
        
                    select pad.IDT_SELL as IDT, 0 as QTY_BUY, sum(isnull(pad.QTY,0)) as QTY_SELL
                    from TRADEALLOC alloc 
                    inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA)
                    where (pa.DTBUSINESS <= @DTBUSINESS) and ((pa.DTOUT is null) or (pa.DTOUT > @DTBUSINESS))  and ((pad.DTCAN is null) or (pad.DTCAN > @DTBUSINESS))     
                    group by pad.IDT_SELL
                ) pos on (pos.IDT = alloc.IDT)
                where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) > 0
            ) pos on (pos.IDB_DEALER = b.IDB) or (pos.IDB_CLEARER = b.IDB)";
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20210211 [XXXXX] Add
        const string QUERYINSERTACTORPOS2 =
        "insert into dbo.IMACTORPOS_MODEL (IDA)" + Cst.CrLf +
        @"select distinct b.IDA_ACTOR
            from dbo.BOOKACTOR_R b
            inner join
            ( 
                select tr.IDB_DEALER, tr.IDB_CLEARER
                from dbo.TRADE tr
		        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
		        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
		        inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER) and (bd.ISPOSKEEPING = 1)
                inner join dbo.EVENT e on (e.IDT = tr.IDT) and (e.EVENTCODE in ('EXE','ASS','AAS','AEX','MOF'))
                inner join dbo.EVENTCLASS ecDly on (ecDly.IDE=e.IDE) and (ecDly.EVENTCLASS='DLY')
                inner join dbo.ASSET_ETD a on (a.IDASSET = tr.IDASSET)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                left outer join dbo.IMDELIVERY imd on (imd.IDDC = da.IDDC)
                left outer join dbo.EVENTCLASS ecStl on (ecStl.IDE = e.IDE) and (ecStl.EVENTCLASS = 'STL')
                where (
			            (tr.DTBUSINESS <= @DTBUSINESS and ((tr.DTOUT is null) or (tr.DTOUT > @DTBUSINESS)) and
				        (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN)) and
				        (
					        (ecDly.DTEVENT >= @DTBUSINESS) or 
					        ((imd.DELIVERYSTEP is not null) /* Exist referential IMDELIVERY */
						        and ((ecDly.DTEVENT + 100) > @DTBUSINESS) /* Keep position 100 days max */
						        and ((ecStl.DTEVENT is null) or (ecStl.DTEVENT < @DTBUSINESS)) /* If not completely delivered */
					        )
				        ) 
			        )
	        ) alloc on (alloc.IDB_DEALER = b.IDB) or (alloc.IDB_CLEARER = b.IDB)
            where not exists (select 1 from dbo.IMACTORPOS_MODEL w where w.IDA = b.IDA_ACTOR)";
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20210211 [XXXXX] Add
        /// RD 20221121 [26172] Use tr.IDB_RISK instead of tr.IDB_DEALER and tr.IDB_CLEARER
        const string QUERYINSERTACTORPOS3 =
            "insert into dbo.IMACTORPOS_MODEL (IDA)" + Cst.CrLf +
            @"select distinct b.IDA_ACTOR
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) 
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'marginRequirement') 
            inner join dbo.BOOKACTOR_R b on (b.IDB = tr.IDB_RISK)
            where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR') and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN)
            and not exists (select 1 from dbo.IMACTORPOS_MODEL w where w.IDA = b.IDA_ACTOR)";

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210211 [XXXXX] Add
        const string QUERYINSERTACTORPOS4 =
            "insert into dbo.IMACTORPOS_MODEL (IDA)" + Cst.CrLf +
            @"select distinct b.IDA_ACTOR 
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) 
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.GPRODUCT = 'COM')
            inner join dbo.EVENT e_phl on (e_phl.IDT = tr.IDT) and (e_phl.EVENTCODE = 'PHL') and (e_phl.EVENTTYPE in ( 'ELC', 'GAS' ))
            inner join dbo.BOOKACTOR_R b on (b.IDB = tr.IDB_DEALER) or (b.IDB = tr.IDB_CLEARER)
            where  
            (tr.DTBUSINESS <= @DTBUSINESS) and (tr.DTBUSINESS >= (@DTBUSINESS - 365)) and 
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN) 
            and not exists (select 1 from dbo.IMACTORPOS_MODEL w where w.IDA = b.IDA_ACTOR)";

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210211 [XXXXX] Add
        const string QUERYINSERTACTORPOS5 =
            "insert into dbo.IMACTORPOS_MODEL (IDA)" + Cst.CrLf +
            @"select distinct a.IDA 
              from
              (
                select ma.IDA_1 as IDA
                from dbo.MASTERAGREEMENT ma
                where (ma.AGREEMENTTYPE = 'ECCPowerAndNaturalGas')
                    and (ma.DTSIGNATURE <= @DTBUSINESS)
                    and (ma.DTENABLED <= @DTBUSINESS)
                    and ((DTDISABLED is null) or (DTDISABLED > @DTBUSINESS))
                union all
                select ma.IDA_2 as IDA
                from dbo.MASTERAGREEMENT ma
                where (ma.AGREEMENTTYPE = 'ECCPowerAndNaturalGas')
                    and (ma.DTSIGNATURE <= @DTBUSINESS)
                    and (ma.DTENABLED <=  @DTBUSINESS)
                    and ((DTDISABLED is null) or (DTDISABLED > @DTBUSINESS))
              ) a
              where not exists (select 1 from dbo.IMACTORPOS_MODEL w where w.IDA = a.IDA)";

        const string QUERYTRUNCATEACTORPOS = "truncate table dbo.IMACTORPOS_MODEL";

        // RD 20130419 [18575] Utilisation de la table IMACTORPOS
        const string QUERYACTORRELATIONSHIP = @"
            select 
            ap.IDENTIFIER as ACTOR, ap.IDA as ACTORID, ap.DISPLAYNAME as ACTORNAME, 
            ap.DTENABLED as ACTOR_DTEN, ap.DTDISABLED as ACTOR_DTDIS,
            ac.IDENTIFIER as ROLEOWNER, ac.IDA as ROLEOWNERID, ac.DISPLAYNAME as ROLEOWNERNAME, 
            ac.DTENABLED as ROLEOWNER_DTEN, ac.DTDISABLED as ROLEOWNER_DTDIS,
            ra.IDROLEACTOR as ROLE, ra.DISPLAYNAME as ROLENAME, 
            ar.DTENABLED as ROLE_DTEN, ar.DTDISABLED as ROLE_DTDIS,
            ar.IDA_ACTOR as ROLE_WITHREGARDTOID 
            from dbo.ACTORROLE ar
            inner join dbo.ACTOR ap on (ap.IDA=ar.IDA_ACTOR)
            inner join dbo.ACTOR ac on (ac.IDA=ar.IDA)
            inner join dbo.IMACTORPOS_MODEL apos on (apos.IDA=ac.IDA)
            inner join dbo.ROLEACTOR ra on (ra.IDROLEACTOR=ar.IDROLEACTOR)
            where (ap.IDA=@IDA)
            union -- Le UNION, c'est pour éviter les doublons sur les deux sous-select
            select
            ap.IDENTIFIER as ACTOR, ap.IDA as ACTORID, ap.DISPLAYNAME as ACTORNAME, 
            ap.DTENABLED as ACTOR_DTEN, ap.DTDISABLED as ACTOR_DTDIS,
            ap.IDENTIFIER as ROLEOWNER, ap.IDA as ROLEOWNERID, ap.DISPLAYNAME as ROLEOWNERNAME, 
            ap.DTENABLED as ROLEOWNER_DTEN, ap.DTDISABLED as ROLEOWNER_DTDIS,
            ra.IDROLEACTOR as ROLE, ra.DISPLAYNAME as ROLENAME, 
            ar.DTENABLED as ROLE_DTEN, ar.DTDISABLED as ROLE_DTDIS,
            ar.IDA_ACTOR as ROLE_WITHREGARDTOID 
            from dbo.ACTORROLE ar
            inner join dbo.ACTOR ap on (ap.IDA=ar.IDA)
            inner join dbo.ROLEACTOR ra on (ra.IDROLEACTOR=ar.IDROLEACTOR)
            where (ap.IDA=@IDA)";

        // PM 20130328 Ajout de "m.IDBC as MARKET_BC" pour l'AGREX
        // PM 20150423 [20575] ENTITYMARKET.DTENTITY à la place de DTMARKET pour EM_DTBUSINESS
        // PM 20160404 [22116] Données déplacées dans la table IMMETHOD : cssEx.INITIALMARGINMETH as CSS_MARGINMETH, cssEx.IMWEIGHTEDRISKMETH as CSS_WEIGHTMETH, cssEx.IMROUNDINGDIR as CSS_ROUNDDIR, cssEx.IMROUNDINGPREC as CSS_ROUNDPREC, cssEx.ISIMINTERCOMSPRD as CSS_SPRD
        // PM 20170313 [22833] Ajout em.DTENTITYNEXT, or (m.ISCOMMODITYSPOT = 1), jointure avec ENTITY, IDBCENTITY
        // PM 20180219 [23824] Ajout css.IDIOTASK_RISKDATA
        // PM 20180530 [23824] Ajout css.ISRISKDATAONHOLIDAY
        // PM 20220111 [25617] Ajout css.ACRONYM
        const string QUERYENTITYMARKETWITHCSS = @"
            select m.IDENTIFIER as MARKET, m.IDM as MARKETID, m.DISPLAYNAME as MARKETNAME, 
                   m.DTENABLED as MARKET_DTEN, m.DTDISABLED as MARKET_DTDIS, 
                   m.ASSIGNMENTMETHOD as MARKET_ASSMETH, m.POSSTOCKCOVER as MARKET_STOCKCOVERAGE, m.ISIMINTRADAYEXEASS as MARKET_EXEASS, 
                   m.ISIMCROSSMARGINING as MARKET_CROSSMARGIN,
                   m.IDBC as MARKET_BC,
                   em.IDEM as EM_IDEM, em.DTENTITY as EM_DTBUSINESS, em.DTENTITYNEXT, em.DTMARKET, em.ISIMCROSSMARGINING as EM_CROSSMARGIN, 
                   css.IDENTIFIER as CSS, css.IDA as CSSID, css.DISPLAYNAME as CSSNAME, cssEx.ACRONYM as CSSACRONYM,
                   css.DTENABLED as CSS_DTEN, css.DTDISABLED as CSS_DTDIS,
                   cssEx.ASSIGNMENTMETHOD as CSS_ASSMETH, 
                   cssEx.IDC as CSS_CURRENCY,
                   isnull(e.IDBCACCOUNT, m.IDBC) as IDBCENTITY,
                   cssEx.IDIOTASK_RISKDATA,
                   cssEx.ISRISKDATAONHOLIDAY
              from dbo.MARKET m
             inner join dbo.ENTITYMARKET em on (em.IDM = m.IDM) and (em.IDA_CUSTODIAN is null)
             inner join dbo.ENTITY e on (e.IDA = em.IDA)
              left outer join dbo.ACTOR css on (css.IDA = m.IDA)
              left outer join dbo.CSS cssEx on (cssEx.IDA = css.IDA)
             where (em.IDA = @IDA)
               and (cssEx.IDA in ({0}))
               and ((m.ISTRADEDDERIVATIVE = 1) or (m.ISCOMMODITYSPOT = 1))
               and (m.ISENABLED = 1)";

        /// <summary>
        /// Important:  sur la méthode ECC/IMSM, il peut exister plusieurs records dans la table IMMETHOD, chacun avec des valeurs de paramètres différentes, 
        ///             dont un seul actif à une date donnée. La méthode C# UpdateReferentialWithIdMethod() update les tables (CSS, MARKET, ...) qui référencent 
        ///             cette table IMMETHOD pour y référencer l'IDIMMETHOD ENABLED à la date business traitée.
        /// </summary>
        // PM 20160404 [22116] Lecture des paramètres concernant les méthodes de calcul de déposit
        // PM 20170313 [22833] Ajout paramètres pour la méthode IMSM et prise en compte de la méthode au niveau du marché ou du CSS (ajout paramètre IDA_CSS)
        // PM 20180316 [23840] Ajout paramètres version de la méthode : METHODVERSION
        // EG 20180803 PERF (IMASSET_ETD_{buildTableId}_W sur la base de IMASSET_ETD_MODEL
        // PM 20190801 [24717] Ajout ISCALCECCCONR & ECCCONRADDON
        // PM 20200910 [25481] Modification clause exist pour Commodity spot market (remplacement sous requête MARKET et CSS par VW_ASSET_COMMODITY_EXPANDED)
        // PM 20200910 [25482] Ajout ISCESMONLY
        // PM 20220111 [25617] Ajout BASEURL, USERID, PWD, CMECORESCHEME
        // PM 20230209 [XXXXX] Ajout ISEXCLUDEWRONGPOS
        // PM 20230322 [26282][WI607] Ajout ISIMMAINTENANCE, NBMONTHLONGCALL, NBMONTHLONGPUT
        // PL 20231011 Add summary content
        // PM 20231019 [XXXXX] Ajout IDENTIFIER
        // PM 20231030 [26547][WI735] Ajout CURRENCYTYPE
        const string QUERYIMMETHODPARAMETER = @"
            select im.IDIMMETHOD,
                   im.IDENTIFIER,
                   im.INITIALMARGINMETH,
                   im.IMWEIGHTEDRISKMETH, 
                   im.IMROUNDINGDIR,
                   im.IMROUNDINGPREC,
                   im.ISIMINTERCOMSPRD,
                   im.ISWITHHOLIDAYADJ,
                   im.ALPHAFACTOR,
                   im.BETAFACTOR,
                   im.EWMAFACTOR,
                   im.WINDOWSIZEFORMAX,
                   im.STATWINDOWSIZE,
                   im.MINIMUMAMOUNT,
                   im.MINIMUMAMOUNTFIRST,
                   im.MINAMTFIRSTTERM,
                   im.METHODVERSION,
                   im.ISCALCECCCONR,
                   im.ECCCONRADDON,
                   im.ISCESMONLY,
                   im.BASEURL,
                   im.USERID,
                   im.PWD,
                   im.CMECORESCHEME,
                   im.CURRENCYTYPE,
                   im.ISEXCLUDEWRONGPOS,
                   im.ISIMMAINTENANCE,
                   im.NBMONTHLONGCALL,
                   im.NBMONTHLONGPUT
              from dbo.IMMETHOD im
             where (im.DTENABLED <= @DTBUSINESS) and ((im.DTDISABLED is null) or (im.DTDISABLED > @DTBUSINESS))
               and ( exists ( select 1 from dbo.IMASSET_ETD_MODEL ima
				               inner join dbo.VW_ASSET_ETD_EXPANDED a on (a.IDASSET = ima.IDASSET)
                               where (a.IDIMMETHOD = im.IDIMMETHOD)
			                )
                     or exists ( select 1 from dbo.VW_ASSET_COMMODITY_EXPANDED ac
                                  where (ac.IDA = @IDA_CSS)
                                    and (ac.IDIMMETHOD = im.IDIMMETHOD)
			                )
                   )";

        const string QUERYMARGINREQOFFICEBOOKSANDPARAMETERS = @"
            select a.IDA as ACTORID, b.IDENTIFIER as BOOK, b.IDB as BOOKID, b.DISPLAYNAME as BOOKNAME, 
            rm.IDB as IMRBOOKID, rm.IMSCOPE as AFFECTALLBOOKS, rm.ISGROSSMARGINING as ISGROSSMARGINING, 
            isnull(rm.IMWEIGHTINGRATIO, -1) as IMWEIGHTINGRATIO,
            rm.DTENABLED,
            rm.DTDISABLED 
            from dbo.BOOK b
            inner join dbo.ACTOR a on a.IDA = b.IDA
            inner join dbo.ACTORROLE ar on ar.IDA = a.IDA and ar.IDROLEACTOR = @ROLE
            left outer join dbo.RISKMARGIN rm on rm.IDA = a.IDA
            where a.IDA = @IDA
            group by a.IDA, b.IDENTIFIER, b.IDB, b.DISPLAYNAME, rm.IDB, rm.IMSCOPE, rm.ISGROSSMARGINING, rm.IMWEIGHTINGRATIO,
            rm.DTENABLED, rm.DTDISABLED";

        // Rd 20170420 [23092] Add
        const string QUERYBOOKS = @"
            select a.IDA as ACTORID, b.IDENTIFIER as BOOK, b.IDB as BOOKID, b.DISPLAYNAME as BOOKNAME ,
            b.DTENABLED, b.DTDISABLED 
            from dbo.BOOK b
            inner join dbo.ACTOR a on (a.IDA = b.IDA)
            where (a.IDA = @IDA)";

        // PM 20150930 [21134] Add join CSS and SCANOFFSETCAPPCT
        // PM 20170106 [22633] Add ISIMFOREXEASSPOS
        const string QUERYCLEARINGORGPARAMETER = @"
            select cop.IDA_CSS,
                   isnull(cop.IMWEIGHTINGRATIO, -1) as IMWEIGHTINGRATIO,
                   cop.SPANACCOUNTTYPE,
                   cop.ISIMMAINTENANCE,
                   isnull(cop.SCANOFFSETCAPPCT,cs.SCANOFFSETCAPPCT) as SCANOFFSETCAPPCT,
                   cop.ISIMFOREXEASSPOS,
                   cop.DTENABLED,
                   cop.DTDISABLED
              from dbo.ACTOR a
             inner join dbo.ACTORROLE ar on (ar.IDA = a.IDA) and (ar.IDROLEACTOR = 'MARGINREQOFFICE')
             inner join dbo.CLEARINGORGPARAM cop on (cop.IDA = ar.IDA)
             inner join dbo.CSS cs on (cs.IDA = cop.IDA_CSS)
             where a.IDA = @IDA";

        const string QUERYEQUITYMARKETPARAMETER = @"
            select 
            imrmp.IDM,
            imrmp.POSSTOCKCOVER,
            imrmp.DTENABLED,
            imrmp.DTDISABLED
            from dbo.ACTOR a
            inner join dbo.ACTORROLE ar on ar.IDA = a.IDA and ar.IDROLEACTOR = 'MARGINREQOFFICE'
            inner join dbo.IMREQMARKETPARAM imrmp on imrmp.IDA = ar.IDA
            where a.IDA = @IDA";

        const string QUERYACTORROLES = @"
            select 
            ap.IDENTIFIER as ACTOR, ap.IDA as ACTORID, ap.DISPLAYNAME as ACTORNAME, ap.DTENABLED as ACTOR_DTEN, ap.DTDISABLED as ACTOR_DTDIS,
            ac.IDENTIFIER as ROLEOWNER, ac.IDA as ROLEOWNERID, ac.DISPLAYNAME as ROLEOWNERNAME, ac.DTENABLED as ROLEOWNER_DTEN, ac.DTDISABLED as ROLEOWNER_DTDIS, 
            ra.IDROLEACTOR as ROLE, ra.DISPLAYNAME as ROLENAME, ar.DTENABLED as ROLE_DTEN, ar.DTDISABLED as ROLE_DTDIS,
            ar.IDA_ACTOR as ROLE_WITHREGARDTOID 
            from dbo.ACTORROLE ar
            inner join dbo.ACTOR ap on ap.IDA = ar.IDA
            left outer join dbo.ACTOR ac on ac.IDA = ar.IDA
            left outer join dbo.ROLEACTOR ra on ra.IDROLEACTOR = ar.IDROLEACTOR
            where ap.IDA = @IDA";
        #endregion referentials

        #region results
        /// <summary>
        /// query returning the previous risk margin evaluations according to the given parameters 
        /// </summary>
        /// <remarks>the risk margin evaluations are stocked as trade</remarks>
        /// PM 20131009 [19046] Added
        /// PM 20131217 [19365] Add ISCLEARER
        /// EG 20140225 [19575][19666] REFACTORING (TRADEINSTRUMENT)
        /// FI 20140703 [20161] use of TRADE.TIMING
        /// RD 20170420 [23094] Add Criteria on "ti.IDA_ENTITY"
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// FI 20210118 [XXXXX] Remove TRADEACTOR join 
        const string QUERYRISKALLRESULTS = @"
            select tr.IDENTIFIER as TRADE, tr.IDT as TRADEID, tr.DISPLAYNAME as TRADENAME,
                   tr.TIMING as TIMING,
                   tr.IDA_RISK as IDA, tr.IDB_RISK as IDB,
                   case when tr.IDB_BUYER = tr.IDB_RISK then 1 else 0 end ISCLEARER
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'marginRequirement')
            where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDA_ENTITY = @IDA_ENTITY) and (tr.IDSTENVIRONMENT = @IDSTENVIRONMENT) and (tr.IDA_CSSCUSTODIAN = @IDA_CSSCUSTODIAN)";
        #endregion results

        #region positions
        // 20120712 MF ticket 18004 Adding join on IMACTOR 
        // EG 20141224 [20566] @DTBUSINESSINVARIANT replace @DTPOSINVARIANT
        // PM 20170206 [22833] Ajout alloc.IDI
        // RD 20170216 [22849] Add condition (alloc.POSKEEPBOOK_DEALER = 1)
        // EG 20180803 PERF New
        // EG 20181119 PERF Correction post RC
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        const string QUERYTRADEALLOCATIONSACTIONS_SQL = @"
        insert into dbo.TRADEPOS_MODEL
        select tr.IDT, tr.IDI, tr.IDM, tr.IDASSET, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.SIDE, tr.QTY
        from dbo.TRADE tr 
        inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) 
        inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
        inner join dbo.IMACTOR_MODEL  ima on (ima.IDA = tr.IDA_DEALER)
        inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
        inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
        where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESSINVARIANT) and (@DTBUSINESSINVARIANT >= tr.DTBUSINESS) {3}
        and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')
        and (tr.IDA_ENTITY = @IDENTITY) 
        and (bd.ISPOSKEEPING = 1)
        and (tr.IDM in ({2}));

        with TRADEALLOC as
        (
            select * from dbo.TRADEPOS_MODEL
        )
        select  alloc.IDA_DEALER as ACTORID, alloc.IDB_DEALER as BOOKID, 
        alloc.IDT, alloc.IDASSET, alloc.SIDE, alloc.IDA_CLEARER as CLEARERID, alloc.IDB_CLEARER as BOOKCLEARERID, 
        alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0) as QTY,
        dc.IDDC, dc.CONTRACTSYMBOL, alloc.IDM as MARKETID, alloc.IDI
        from TRADEALLOC alloc
        left outer join
        (
            {0}
            union all 
            {1}
        ) pos on (pos.IDT = alloc.IDT)
        inner join dbo.ASSET_ETD asset on (asset.IDASSET = alloc.IDASSET)
        inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
        where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) > 0";

        // EG 20180803 PERF New
        // EG 20181119 PERF Correction post RC
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        const string QUERYTRADEALLOCATIONSACTIONS_ORA = @"with TRADEALLOC as
        (
            select tr.IDA_DEALER, tr.IDB_DEALER, tr.IDT, tr.IDASSET, tr.SIDE, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.QTY, tr.IDM, tr.IDI
            from dbo.TRADE tr 
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) 
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.FUNGIBILITYMODE != 'NONE') and (pr.GPRODUCT = 'FUT')
            inner join dbo.IMACTOR_MODEL  ima on (ima.IDA = tr.IDA_DEALER)
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            where (tr.DTOUT is null or tr.DTOUT > @DTBUSINESSINVARIANT) and (@DTBUSINESSINVARIANT >= tr.DTBUSINESS) {3}
            and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')
            and (tr.IDA_ENTITY = @IDENTITY) 
            and (bd.ISPOSKEEPING = 1)
            and (tr.IDM in ({2}))
        ) 
        select  alloc.IDA_DEALER as ACTORID, alloc.IDB_DEALER as BOOKID, 
        alloc.IDT, alloc.IDASSET, alloc.SIDE, alloc.IDA_CLEARER as CLEARERID, alloc.IDB_CLEARER as BOOKCLEARERID, 
        alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0) as QTY,
        dc.IDDC, dc.CONTRACTSYMBOL, alloc.IDM as MARKETID, alloc.IDI
        from TRADEALLOC alloc
        left outer join
        (
            {0}
            union all 
            {1}
        ) pos on (pos.IDT = alloc.IDT)
        inner join dbo.ASSET_ETD asset on (asset.IDASSET = alloc.IDASSET)
        inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
        where (alloc.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL,0)) > 0";

        // 20120712 MF ticket 18004 Adding join on IMACTOR 
        // EG 20140223 REFACTORING
        // PM 20150609 [21089][21097] Ne prendre que les EVENT ayant IDSTACTIVATION = 'REGULAR'
        // PM 20170206 [22833] Ajout alloc.IDI
        // EG 20180803 PERF Upd
        // EG 20201201 [25562] Utilisation de la table TRADE,INSTRUMENT et PRODUCT en lieu et place de VW_TRADE_POSKEEPING_ALLOC
        // PL 20230124 [26217] Add DTOUT criteria on TRADE
        const string QUERYPHYSICALSETTLEMENTS = @"
        select alloc.IDA_DEALER        as ACTORID, 
               alloc.IDB_DEALER        as BOOKID, 
               alloc.IDT               as IDT, 
               alloc.IDASSET           as IDASSET, 
               alloc.SIDE              as SIDE,
               alloc.IDA_CLEARER       as CLEARERID,
               alloc.IDB_CLEARER       as BOOKCLEARERID,
               0                       as QTY,
               dc.IDDC                 as IDDC, 
               dc.CONTRACTSYMBOL       as CONTRACTSYMBOL, 
               alloc.IDM               as MARKETID,
               e.VALORISATION          as EXEASSQTY,
               ecDly.DTEVENT           as DELIVERYDATE,
               ecPhy.DTEVENT           as SETLLEMENTDATE,
               alloc.IDI               as IDI,
			   null                    as IMEXPRESSIONTYPE
         from dbo.TRADE alloc 
         inner join dbo.INSTRUMENT ns on (ns.IDI = alloc.IDI)
         inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.GPRODUCT = 'FUT')
         inner join dbo.IMACTOR_MODEL ima on (ima.IDA = alloc.IDA_DEALER)
         inner join dbo.ASSET_ETD asset on (asset.IDASSET = alloc.IDASSET)
         inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
         inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
         inner join dbo.EVENT e on (e.IDT = alloc.IDT) and (e.EVENTCODE in ('EXE','ASS','AAS','AEX','MOF')) and (e.IDSTACTIVATION = 'REGULAR')
         inner join dbo.EVENTCLASS ecPhy on (ecPhy.IDE = e.IDE) and (ecPhy.EVENTCLASS = 'PHY')
         inner join dbo.EVENTCLASS ecDly on (ecDly.IDE = e.IDE) and (ecDly.EVENTCLASS = 'DLY')
         where (alloc.IDSTACTIVATION = 'REGULAR') and (alloc.IDSTBUSINESS = 'ALLOC') and (alloc.IDA_ENTITY = @IDENTITY)
           and (alloc.IDM in ({0}))
           and (alloc.DTBUSINESS <= @DTBUSINESS) and ((alloc.DTOUT is null) or (alloc.DTOUT > @DTBUSINESS)) 
           {1}
           {2}
           and (ecDly.DTEVENT > @DTBUSINESS)";

        // PM 20130905 [17949] Livraison
        // PM 20140110 [19469] Modification des condition pour les colonnes DELIVERYSTEPDATE et DELIVERYSTEPDATE
        // Ancienne version :
        //case when (@DTBUSINESS < ecDly.DTEVENT) then ecPhy.DTEVENT else
        //     case when (((ecPRS.DTEVENT is null) or (@DTBUSINESS >= ecPRS.DTEVENT)) and ((ecSTL.DTEVENT is null) or (ecPRS.DTEVENT < ecSTL.DTEVENT))) then ecPRS.DTEVENT else
        //     case when ((ecSTL.DTEVENT is not null) and (@DTBUSINESS < ecSTL.DTEVENT)) then ecDly.DTEVENT else null
        //end end end             as DELIVERYSTEPDATE, /* Step begining date */
        //case when (@DTBUSINESS < ecDly.DTEVENT) then 'PreDelivery' else
        //     case when (((ecPRS.DTEVENT is null) or (@DTBUSINESS >= ecPRS.DTEVENT)) and ((ecSTL.DTEVENT is null) or (ecPRS.DTEVENT < ecSTL.DTEVENT))) then 'InvoicingInTwoSteps' else
        //     case when ((ecSTL.DTEVENT is not null) and (@DTBUSINESS < ecSTL.DTEVENT)) then 'Delivery' else 'NA'
        //end end end             as DELIVERYSTEP,
        // EG 20140223 REFACTORING
        // PM 20150609 [21089][21097] Ne prendre que les EVENT ayant IDSTACTIVATION = 'REGULAR'
        // PM 20170206 [22833] Ajout alloc.IDI
        // EG 20180803 PERF Upd
        // PM 20190401 [24625][24387] Gestion de l'additionalt margin BoM (AMBO) de l'ECC
        // PL 20230124 [XXXXX] Add DTOUT criteria on TRADE
        const string QUERYPHYSICALDELIVERY = @"
        select alloc.IDA_DEALER        as ACTORID, 
               alloc.IDB_DEALER        as BOOKID, 
               alloc.IDT               as IDT, 
               alloc.IDASSET           as IDASSET, 
               alloc.SIDE              as SIDE,
               alloc.IDA_CLEARER       as CLEARERID,
               alloc.IDB_CLEARER       as BOOKCLEARERID,
               0                       as QTY,
               dc.IDDC                 as IDDC,
               dc.CONTRACTSYMBOL       as CONTRACTSYMBOL,
               alloc.IDM               as MARKETID, 
               0                       as EXEASSQTY,
               ecDly.DTEVENT           as DELIVERYDATE,
               ecPhy.DTEVENT           as SETLLEMENTDATE,
               case when (@DTBUSINESS < ecDly.DTEVENT)
                    then ecPhy.DTEVENT
               else /* DTBUSINESS >=  ecDly.DTEVENT */
                    case when (ecPRS.DTEVENT is not null) /* PRS needed */
                          and (@DTBUSINESS >= ecPRS.DTEVENT) /* post PRS date */
                          and ((ecSTL.DTEVENT is null) or (ecPRS.DTEVENT < ecSTL.DTEVENT)) /* no STL or PRS before STL */
                         then ecPRS.DTEVENT
                    else 
                         case when (ecPRS.DTEVENT is null) /* no PRS */
                                or (@DTBUSINESS < ecPRS.DTEVENT) /* before PRS */
                              then ecDly.DTEVENT
                         else null 
                         end
                    end
               end as DELIVERYSTEPDATE, /* Step begining date */
               case when (@DTBUSINESS < ecDly.DTEVENT)
                    then 'PreDelivery'
               else /* DTBUSINESS >=  ecDly.DTEVENT */
                    case when (ecPRS.DTEVENT is not null) /* PRS needed */
                          and (@DTBUSINESS >= ecPRS.DTEVENT) /* post PRS date */
                          and ((ecSTL.DTEVENT is null) or (ecPRS.DTEVENT < ecSTL.DTEVENT)) /* no STL or PRS before STL */
                         then 'InvoicingInTwoSteps'
                    else 
                         case when (ecPRS.DTEVENT is null) /* no PRS */
                                or (@DTBUSINESS < ecPRS.DTEVENT) /* before PRS */
                              then case when ((imd.DELIVERYSTEP = 'Expiry') and (ecPhy.DTEVENT = @DTBUSINESS)) then 'Expiry' else 'Delivery' end
                         else 'NA'        
                         end
                    end
               end as DELIVERYSTEP,
               e.VALORISATION          as DELIVERYQTY,
               alloc.IDI               as IDI,
			   imd.IMEXPRESSIONTYPE    as IMEXPRESSIONTYPE
          from dbo.VW_TRADE_POSKEEPING_ALLOC alloc 
         inner join dbo.IMACTOR_MODEL ima on (ima.IDA = alloc.IDA_DEALER)
         inner join dbo.ASSET_ETD asset on (asset.IDASSET = alloc.IDASSET)
         inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = asset.IDDERIVATIVEATTRIB)
         inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
         inner join dbo.IMDELIVERY imd on (imd.IDDC = dc.IDDC) and (imd.DTENABLED <= @DTBUSINESS) and ((imd.DTDISABLED is null) or (imd.DTDISABLED > @DTBUSINESS))
         inner join dbo.EVENT e on (e.IDT = alloc.IDT) and (e.EVENTCODE = 'MOF') and (e.IDSTACTIVATION = 'REGULAR')
         inner join dbo.EVENTCLASS ecPhy on (ecPhy.IDE = e.IDE) and (ecPhy.EVENTCLASS = 'PHY')
         inner join dbo.EVENTCLASS ecDly on (ecDly.IDE = e.IDE) and (ecDly.EVENTCLASS = 'DLY')
          left outer join dbo.EVENTCLASS ecPRS on (ecPRS.IDE = e.IDE) and (ecPRS.EVENTCLASS = 'PRS')
          left outer join dbo.EVENTCLASS ecSTL on (ecSTL.IDE = e.IDE) and (ecSTL.EVENTCLASS = 'STL')
         where (alloc.IDA_ENTITYDEALER = @IDENTITY)
           and (alloc.IDM in ({0}))
           and (alloc.DTBUSINESS <= @DTBUSINESS) and ((alloc.DTOUT is null) or (alloc.DTOUT > @DTBUSINESS)) 
           {1}
           {2}
           and (ecPhy.DTEVENT > (@DTBUSINESS - @MAXDELIVERYDAY))
           and ((ecSTL.DTEVENT is null) or (ecSTL.DTEVENT > @DTBUSINESS))
           and ((ecPhy.DTEVENT = @DTBUSINESS) or (imd.DELIVERYSTEP != 'Expiry'))";
        #endregion positions

        #region Trade value
        /// <summary>
        /// Lecture de la valeur du GAM des trades COM sur Electricité et Gas
        /// </summary>
        // PM 20170313 [22833] Ajout
        // PM 20170626 [23261][23257] Ajout signer négativement la valorisation lorsque le dealer est recever
        // PM 20170808 [23371] Ajout IDASSET, UNIT, IDCC (jointure avec VW_ASSET_COMMODITY_EXPANDED), DTEVENT (jointure avec EVENTCLASS)
        // EG 20171025 [23509] Add DTORDERENTERED, DTEXECUTION, TZFACILITY
        // CC/PM 20180308 [23828] Ajout (e.UNIT is not null) pour ne pas prendre la GAM sans devise (trade sans valo)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // PM 20230104 [26181] Ajout asset.IDIMMETHOD
        const string QUERYIMSMTRADEVALUE = @"
            select tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDM, tr.IDI, tr.IDASSET, asset.IDCC, tr.IDT, 
                   tr.DTTIMESTAMP, tr.DTORDERENTERED, tr.DTEXECUTION, tr.TZFACILITY,
                   case when e.IDA_PAY = tr.IDA_DEALER then e.VALORISATION else (-1 * e.VALORISATION) end as VALORISATION,
                   e.UNIT as IDC,
                   ec.DTEVENT as DTSTL,
                   asset.IDIMMETHOD
              from dbo.TRADE tr
             inner join dbo.VW_INSTR_PRODUCT pr on (pr.IDI = tr.IDI) and (pr.GPRODUCT = 'COM')
			 inner join dbo.VW_ASSET_COMMODITY_EXPANDED asset on (asset.IDASSET = tr.IDASSET)
             inner join dbo.EVENT e_phl on (e_phl.IDT = tr.IDT) and (e_phl.EVENTCODE = 'PHL') and (e_phl.EVENTTYPE in ( 'ELC', 'GAS' ))
             inner join dbo.EVENT e on (e.IDT = tr.IDT) and (e.EVENTCODE = 'LPP') and (e.EVENTTYPE = 'GAM') and (e.IDSTACTIVATION = 'REGULAR') and (e.UNIT is not null)
			 inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'STL')
             where (tr.IDA_ENTITY = @IDENTITY)
               and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')
               and (tr.IDM = @IDM)
               and (tr.DTBUSINESS >= @DTBUSINESSFIRST) and (tr.DTBUSINESS <= @DTBUSINESS)";

        /// <summary>
        /// Lecture des acteurs/books avec agreement dans le cas où ils n'auraient pas de trade
        /// </summary>
        // PM 20190418 [24628] Ajout
        // PM 20191209 [25089] Ajout jointure avec BOOK pour restriction sur @ENTITY
        // PM 20230104 [26181] Ajout meth.IDIMMETHOD qui doit être la méthode IMSM associé au MasterAgremeement ECCPowerAndNaturalGas
        const string QUERYIMSMACTORAGREEMENT = @"
            select ma.IDA_DEALER, ma.IDB_DEALER, ma.IDA_CLEARER, ma.IDB_CLEARER,
                   @IDM as IDM, null as IDI, 0 as IDASSET, 0 as IDCC, null as IDT, 
                   @DTBUSINESS as DTTIMESTAMP, @DTBUSINESS as DTORDERENTERED, @DTBUSINESS as DTEXECUTION, m.TIMEZONE as TZFACILITY,
                   0 as VALORISATION,
                   isnull(CSS.IDC, 'EUR') as IDC,
                   @DTBUSINESS as DTSTL,
                   meth.IDIMMETHOD
              from dbo.MARKET m
             inner join dbo.ACTOR a on (a.IDA = m.IDA)
             inner join dbo.ACTORROLE ar on (ar.IDA = a.IDA) and (ar.IDROLEACTOR = 'CSS')
             inner join dbo.CSS on (CSS.IDA = a.IDA)
             cross join ( select ma.IDA_1 as IDA_DEALER, ma.IDB_1 as IDB_DEALER, ma.IDA_2 as IDA_CLEARER, ma.IDB_2 as IDB_CLEARER
                   from dbo.MASTERAGREEMENT ma
                  inner join dbo.BOOK b on (b.IDB = ma.IDB_1) and (b.IDA_ENTITY = @IDENTITY)
                  where (ma.AGREEMENTTYPE = 'ECCPowerAndNaturalGas')
				    and (ma.DTSIGNATURE <= @DTBUSINESS)
				    and (ma.DTENABLED <= @DTBUSINESS)
                    and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))
                    and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = ma.IDA_2) and ar.IDROLEACTOR in ('CSS','CLEARER'))
                    and not exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = ma.IDA_1) and ar.IDROLEACTOR in ('CSS','CLEARER'))
                 union all
                 select ma.IDA_2 as IDA_DEALER, ma.IDB_2 as IDB_DEALER, ma.IDA_1 as IDA_CLEARER, ma.IDB_1 as IDB_CLEARER
                   from dbo.MASTERAGREEMENT ma
                  inner join dbo.BOOK b on (b.IDB = ma.IDB_2) and (b.IDA_ENTITY = @IDENTITY)
                  where (ma.AGREEMENTTYPE = 'ECCPowerAndNaturalGas')
				    and (ma.DTSIGNATURE <= @DTBUSINESS)
				    and (ma.DTENABLED <= @DTBUSINESS)
                    and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))
                    and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = ma.IDA_1) and ar.IDROLEACTOR in ('CSS','CLEARER'))
                    and not exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = ma.IDA_2) and ar.IDROLEACTOR in ('CSS','CLEARER'))
                 ) ma
             cross join IMMETHOD meth
             where (meth.INITIALMARGINMETH = 'IMSM')
               and (meth.ISCESMONLY = 0)
               and (meth.DTENABLED <= @DTBUSINESS)
               and ((meth.DTDISABLED is null) or (meth.DTDISABLED > @DTBUSINESS))
               and (m.IDM = @IDM)";

        #endregion Trade value

        #region Trade of the day without initial margin
        /// <summary>
        /// Lecture des trades du jour pour lesquels la méthode calcul de déposit est NO_MARGIN
        /// </summary>
        // PM 20221212 [XXXXX] Ajout
        const string QUERYNOMARGINDAYTRADE = @"
            select tr.IDT, tr.IDENTIFIER, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER,
                   tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, tr.DTBUSINESS,
                   tr.IDI, tr.IDM, tr.MARKET_IDENTIFIER, tr.IDASSET, tr.ASSETCATEGORY,
	               tr.SIDE, tr.QTY, tr.PRICE,
				   a.IDC, a.IDDC as IDCONTRACT, a.CONTRACTIDENTIFIER, a.IDENTIFIER as ASSETIDENTIFIER
              from dbo.VW_TRADE_ALLOC tr
             inner join dbo.VW_ASSET_ETD_EXPANDED a on (a.IDASSET = tr.IDASSET)
             inner join dbo.IMMETHOD meth on (meth.IDIMMETHOD = a.IDIMMETHOD)
             where (meth.INITIALMARGINMETH = 'NOMARGIN')
			   and (tr.ASSETCATEGORY = 'ExchangeTradedContract')
			   and (tr.IDA_ENTITY = @IDENTITY)
               and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')
               and (tr.IDM = @IDM)
               and (tr.DTBUSINESS = @DTBUSINESS)
			union all
            select tr.IDT, tr.IDENTIFIER, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER,
                   tr.DTTIMESTAMP, tr.DTEXECUTION, tr.TZFACILITY, tr.DTBUSINESS,
                   tr.IDI, tr.IDM, tr.MARKET_IDENTIFIER, tr.IDASSET, tr.ASSETCATEGORY,
	               tr.SIDE, tr.QTY, tr.PRICE,
				   a.IDC, a.IDCC as IDCONTRACT, a.CONTRACTIDENTIFIER, a.IDENTIFIER as ASSETIDENTIFIER
              from dbo.VW_TRADE_ALLOC tr
             inner join dbo.VW_ASSET_COMMODITY_EXPANDED a on (a.IDASSET = tr.IDASSET)
             inner join dbo.IMMETHOD meth on (meth.IDIMMETHOD = a.IDIMMETHOD)
             where (meth.INITIALMARGINMETH = 'NOMARGIN')
			   and (tr.ASSETCATEGORY = 'Commodity')
			   and (tr.IDA_ENTITY = @IDENTITY)
               and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')
               and (tr.IDM = @IDM)
               and (tr.DTBUSINESS = @DTBUSINESS)";
        #endregion Trade of the day without initial margin

        #region calculation sheets

        const string QUERYPROCESSMARGINTRACK = @"
            select IDMARGINTRACK, IDPROCESS_L
            from dbo.MARGINTRACK where IDPROCESS_L = @IDPROCESS_L";

        /// <summary>
        /// Insert calculation sheet command for Oracle DBMS. Need for XML document bigger than 32KB 
        /// </summary>
        const string QUERYORACLE_INSERTTRADEMARGINTRACK = @"
            insert into dbo.TRADEMARGINTRACK (IDT, IDMARGINTRACK, TRADEXML, DTINS, IDAINS) 
            values (@IDT, @IDMARGINTRACK, @TRADEXML, @DTINS, @IDAINS)";

        /// <summary>
        /// Select sur TRADEMARGINTRACK pour la constitution des DataTable
        /// </summary>
        // PM 20160922 Pour VS2013
        const string QUERY_SELECTTRADEMARGINTRACK = @"
            select * from dbo.TRADEMARGINTRACK where 0 = 1";
        #endregion calculation sheets

        #region methods parameters 
        # region Common
        /// <summary>
        /// Select the referential elements in order to compute the delivery deposit
        /// </summary>
        // PM 20130904 [17949] ajout imd.DELIVERYSTEP
        // EG 20140521 [19972] Refactoring DTENABLED/DTDISABLED
        // EG 20180803 PERF Upd
        const string QUERYASSETDELIVERY = @"
            select 
            imd.IDDC, 
            ae.IDASSET,
            imd.IMEXPRESSIONTYPE,
            isnull(imd.IDC_IM, dc.IDC_PRICE) as IDC,
            imd.IMDELIVERYAMOUNT,
            ae.FACTOR,
            ae.ASSETCATEGORY,
            ae.IDASSET_UNL,
            ae.CATEGORY,
            imd.DELIVERYSTEP
            from dbo.IMDELIVERY imd
            inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = imd.IDDC
            inner join dbo.VW_ASSET_ETD_EXPANDED ae on ae.IDDC = dc.IDDC
            inner join dbo.IMASSET_ETD_MODEL ima on ima.IDASSET = ae.IDASSET
            where (imd.DTENABLED <= @DTBUSINESS) and ((imd.DTDISABLED is null) or (imd.DTDISABLED > @DTBUSINESS))";

        /// <summary>
        /// Select all the stocks on Equities for the given derivative contracts
        /// </summary>
        // PM 20130326 Ajout de la prise en compte de la Duration.
        // Et ajout de la jointure avec IMASSET_ETD et du parametre SESSIONID pour limiter la requête sur les DC en position, en remplacement de l'utilsation du "in {0}".
        //where 
        //po.DTBUSINESS = @DTBUSINESS
        //and dc.IDDC in ({0})
        // PM 20130924 [18976] Ne prendre que les contrats dérivés du marché spécifié sur la position action
        // FI 20160613 [22256] Modify (Add po.IDPOSEQUSECURITY,unlasset.ASSET_UNL_IDENTIFIER,PAYER, RECEIVER) add @SESSIONID
        // EG 20190902 [23587] La query QUERYEQUITYSTOCKSCOVERAGE retournent  les DCs et leurs sous-jacents par le code ISIN de l'asset spécifié dans POSEQUSECURITY.
        // PM 20201028 [25570][25542] Modification de la requête afin de prendre en compte les constituants d'un panier
        const string QUERYEQUITYSTOCKSCOVERAGE = @"
        select dc.IDDC,
               dc.IDASSET_UNL,
               unlasset2.IDENTIFIER as ASSET_UNL_IDENTIFIER,
               po.IDPOSEQUSECURITY,
               po.IDENTIFIER as POSEQUSECURITY_IDENTIFIER,   
               po.IDA_PAY,
               apay.IDENTIFIER as PAYER_ACTOR_IDENTIFIER, 
               po.IDB_PAY,
               bpay.IDENTIFIER as PAYER_BOOK_IDENTIFIER, 
               po.IDA_REC,
               arec.IDENTIFIER as RECEIVER_ACTOR_IDENTIFIER,                
               po.IDB_REC,
               brec.IDENTIFIER as RECEIVER_BOOK_IDENTIFIER, 
               po.QTY, 
               po.POSSTOCKCOVER,
               po.IDDC as GROUPBYIDDC,
               dc.IDM,
               po.IDASSET,
               unlasset.ASSETCLASS,
               unlasset.ASSETCLASS as ASSETCLASS_UNL,
			   null as UNITTYPEWEIGHT,
			   null as WEIGHT,
               unlasset.IDENTIFIER as ASSET_IDENTIFIER
        from dbo.POSEQUSECURITY po 
        inner join dbo.ASSET_EQUITY unlasset on (unlasset.IDASSET = po.IDASSET)
        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDM = po.IDM) and ((dc.IDDC = po.IDDC) or (po.IDDC is null)) and (dc.ASSETCATEGORY = 'EquityAsset')
        inner join dbo.ASSET_EQUITY unlasset2 on (unlasset2.IDASSET = dc.IDASSET_UNL) and (unlasset2.ISINCODE = unlasset.ISINCODE)
        inner join dbo.ACTOR aPay on (apay.IDA = po.IDA_PAY)
        inner join dbo.BOOK bPay on (bpay.IDB = po.IDB_PAY)    
        inner join dbo.ACTOR aRec on (aRec.IDA = po.IDA_REC)
        inner join dbo.BOOK bRec on (brec.IDB = po.IDB_REC)    
        inner join (select distinct IDDC from dbo.IMASSET_ETD_MODEL) ima on (ima.IDDC = dc.IDDC)
        where (po.IDSTACTIVATION = 'REGULAR')
          and (((po.DTBUSINESS = @DTBUSINESS) and (po.DURATION = 'Overnight'))
              or ((po.DTBUSINESS <= @DTBUSINESS) and (po.DURATION = 'Open'))
              or ((po.DTBUSINESS <= @DTBUSINESS) and (po.DTTERMINATION is not null) and (po.DTTERMINATION >= @DTBUSINESS) and (po.DURATION = 'Term')))
        union all
        /* Basket constituent */
	    select dc.IDDC,
               dc.IDASSET_UNL,
               unlassetbsk.IDENTIFIER as ASSET_UNL_IDENTIFIER,
               po.IDPOSEQUSECURITY,
               po.IDENTIFIER as POSEQUSECURITY_IDENTIFIER,   
               po.IDA_PAY,
               apay.IDENTIFIER as PAYER_ACTOR_IDENTIFIER, 
               po.IDB_PAY,
               bpay.IDENTIFIER as PAYER_BOOK_IDENTIFIER, 
               po.IDA_REC,
               arec.IDENTIFIER as RECEIVER_ACTOR_IDENTIFIER,                
               po.IDB_REC,
               brec.IDENTIFIER as RECEIVER_BOOK_IDENTIFIER, 
               po.QTY, 
               po.POSSTOCKCOVER,
               po.IDDC as GROUPBYIDDC,
               dc.IDM,
               po.IDASSET,
			   unlasset.ASSETCLASS,
			   unlassetbsk.ASSETCLASS as ASSETCLASS_UNL,
			   unlassetbsk.UNITTYPEWEIGHT,
			   unlassetconst.WEIGHT,
               unlasset.IDENTIFIER as ASSET_IDENTIFIER
        from dbo.POSEQUSECURITY po 
        inner join dbo.ASSET_EQUITY unlasset on (unlasset.IDASSET = po.IDASSET) and (unlasset.ASSETCLASS = 'FS')
		inner join dbo.ASSET_EQUITYBSKCONST unlassetconst on (unlassetconst.IDASSETREF = unlasset.IDASSET) 
		inner join dbo.ASSET_EQUITY unlassetbsk on (unlassetbsk.IDASSET = unlassetconst.IDASSET) and (unlassetbsk.ASSETCLASS = 'FB')
        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDM = po.IDM) and ((dc.IDDC = po.IDDC) or (po.IDDC is null)) and (dc.ASSETCATEGORY = 'EquityAsset')
        inner join dbo.ASSET_EQUITY unlasset2 on (unlasset2.IDASSET = dc.IDASSET_UNL) and (unlasset2.ISINCODE = unlassetbsk.ISINCODE) and (unlasset2.ASSETCLASS = unlassetbsk.ASSETCLASS)
        inner join dbo.ACTOR aPay on (apay.IDA = po.IDA_PAY)
        inner join dbo.BOOK bPay on (bpay.IDB = po.IDB_PAY)    
        inner join dbo.ACTOR aRec on (aRec.IDA = po.IDA_REC)
        inner join dbo.BOOK bRec on (brec.IDB = po.IDB_REC)
        inner join (select distinct IDDC from dbo.IMASSET_ETD_MODEL) ima on (ima.IDDC = dc.IDDC)
        where (po.IDSTACTIVATION = 'REGULAR')
          and (((po.DTBUSINESS = @DTBUSINESS) and (po.DURATION = 'Overnight'))
            or ((po.DTBUSINESS <= @DTBUSINESS) and (po.DURATION = 'Open'))
            or ((po.DTBUSINESS <= @DTBUSINESS) and (po.DTTERMINATION is not null) and (po.DTTERMINATION >= @DTBUSINESS) and (po.DURATION = 'Term')))
";
 
        /// <summary>
        /// Requête permettant de lire le paramètrage des paniers d'equity pouvant avoir été déposé en reduction de position ETD 
        /// </summary>
        // PM 20201028 [25570][25542] Ajout
        const string QUERYEQUITYBASKETSETTING = @"
        select bsk.IDASSET,
               bsk.IDENTIFIER,
               bsk.UNITTYPEWEIGHT,
               cst.IDASSETREF,
               cst.WEIGHT
          from ASSET_EQUITY bsk
         inner join ASSET_EQUITYBSKCONST cst on (cst.IDASSET = bsk.IDASSET)
         inner join ASSET_EQUITY asset on (asset.IDASSET = cst.IDASSETREF)
         where (bsk.ASSETCLASS = 'FB')
           and exists ( select 1
                          from dbo.ASSET_EQUITY unlassetbsk 
                         inner join dbo.ASSET_EQUITY unlasset on (unlasset.ISINCODE = unlassetbsk.ISINCODE) and (unlasset.ASSETCLASS = unlassetbsk.ASSETCLASS)
                         inner join dbo.DERIVATIVECONTRACT dc on (unlasset.IDASSET = dc.IDASSET_UNL) and (dc.ASSETCATEGORY = 'EquityAsset')
		                 inner join dbo.POSEQUSECURITY po on (dc.IDM = po.IDM) and ((dc.IDDC = po.IDDC) or (po.IDDC is null))
	                     inner join (select distinct IDDC from dbo.IMASSET_ETD_MODEL) ima on (ima.IDDC = dc.IDDC)
                        where (unlassetbsk.IDASSET = bsk.IDASSET)
                          and (unlassetbsk.ASSETCLASS = 'FB') and (po.IDSTACTIVATION = 'REGULAR')
                          and (((po.DTBUSINESS = '20170616') and (po.DURATION = 'Overnight'))
                              or ((po.DTBUSINESS <= '20170616') and (po.DURATION = 'Open'))
                              or ((po.DTBUSINESS <= '20170616') and (po.DTTERMINATION is not null) and (po.DTTERMINATION >= '20170616') and (po.DURATION = 'Term')))
                      )
           and (bsk.DTENABLED <= @DTBUSINESS) and ((bsk.DTDISABLED is null) or (bsk.DTDISABLED > @DTBUSINESS))
           and (asset.DTENABLED <= @DTBUSINESS) and ((asset.DTDISABLED is null) or (asset.DTDISABLED > @DTBUSINESS))
        ";
        #endregion Common

        #region CUSTOM

        // 20120717 MF Ticket 18004 - introducing join on IMASSET_ETD
        // EG 20180803 PERF Upd
        const string QUERYCONTRACTASSETCUSTOMMETHOD = @"
            select 
            dc.IDENTIFIER as CONTRACT, dc.IDDC as CONTRACTID, dc.CATEGORY as CATEGORY, dc.IDC_PRICE as CURRENCY,
            ae.PUTCALL as PUTCALL, ae.IDASSET as ASSETID, dc.ASSETCATEGORY as UNDERLYERCATEGORY, 
            dc.IDASSET_UNL as UNDERLYERID, dc.IDDC_UNL as UNDERLYERCONTRACTID,
            dc.CONTRACTMULTIPLIER as CONTRACTMULTIPLIER
            from dbo.DERIVATIVECONTRACT dc 
            inner join dbo.DERIVATIVEATTRIB da on da.IDDC = dc.IDDC
            inner join dbo.ASSET_ETD ae on ae.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB
            inner join dbo.IMASSET_ETD_MODEL ima on ima.IDASSET = ae.IDASSET";

        // 20120717 MF Ticket 18004 - introducing join on IMASSET_ETD
        // EG 20180803 PERF Upd
        const string QUERYPARAMSCUSTOMMETHOD = @"
            select 
            ims.IDDC as CONTRACTID, ims.DTENABLED as PARAM_DTEN, ims.DTDISABLED as PARAM_DTDIS, 
            ims.IMEXPRESSIONTYPE as EXPRESSIONTYPE, 
            ims.IMSTRADDLE as IMSTRADDLE, ims.IMNORMAL as IMNORMAL,
            ims.IMLONGCALL as IMLONGCALL, ims.IMLONGPUT as IMLONGPUT,
            ims.IMSHORTCALL as IMSHORTCALL, ims.IMSHORTPUT as IMSHORTPUT,
            ims.IDC_IM as IMCURRENCY 
            from dbo.IMSTANDARD ims
            inner join (select IDDC from dbo.IMASSET_ETD_MODEL group by IDDC) ima on ima.IDDC = ims.IDDC";

        #endregion CUSTOM

        #region TIMS IDEM
        // 20120717 MF Ticket 18004 - introducing join on IMASSET_ETD

        // 20121023 FL - Correction d’un bug - Pour trouver la devise d’un DC à utiliser pour le calcul d’un déposit,
        //  on allait prendre la devise de la cotation du sous-jacent du DC considéré et non pas tout simplement la devise 
        //  devise du DC(IDC) mise à disposition dans vue VW_ASSET_ETD_EXPANDED.
        //  Modification effectuée dans le select ci-dessous: isnull(qi.IDC, qe.IDC) as IDC ==> ae.IDC as IDC

        // PM 20130422 [18592] Ajout UnderlyingContractSymbol

        // PM 20130528 Ajout de "QUOTESIDE = 'OfficialClose'" dans les conditions sur les cotations des sous-jacents
        // EG 20140521 [19972] Refactoring sur jointure IMASSET_ETD
        // PM 20170222 [22881][22942] Ajout jointure avec IMMARS_H et renommage de PARAMSTIMSIDEM_CLASS and IMMARSCLASS_H
        // EG 20180803 PERF Upd
        const string QUERYCLASSTIMSIDEMMETHOD = @"
            select distinct c.IDDC, c.IDASSET_UNL,
                   isnull(qi.VALUE, qe.VALUE) as ASSET_UNL_QUOTE, c.UNDERLYINGPRICE as CLASS_ASSET_UNL_QUOTE,
                   c.CLASSGROUP, c.PRODUCTGROUP, ima.CONTRACTMULTIPLIER, c.CONTRACTMULTIPLIER as CLASS_CONTRACTMULTIPLIER, 
                   isnull(ima.CONTRACTSYMBOL, c.CONTRACTSYMBOL) as CONTRACTSYMBOL, 
                   c.DELIVERYRATE, c.OFFSET, c.SPOTSPREADRATE, c.NONSPOTSPREADRATE,  c.MINIMUMMARGIN, c.EXPIRYTIME,
                   c.CATEGORY as CATEGORY, ima.IDC as IDC, ima.CONTRACTDISPLAYNAME as CONTRACTDESCRIPTION, ima.MMYRULE,
                   c.UNDERLYINGCONTRACT as UNDERLYINGCONTRACT, c.ISINCODE,
                   1 as ISACTIVE
              from dbo.IMMARSCLASS_H c
                    inner join dbo.IMMARS_H m on (m.IDIMMARS_H = c.IDIMMARS_H)
            left outer join 
            (
                      select distinct ima.IDDC,
                             ae.CONTRACTMULTIPLIER, ae.CONTRACTSYMBOL, ae.CONTRACTDISPLAYNAME, ae.IDASSET_UNL, ae.IDC, ae.ASSETCATEGORY,
                          mr.MMYRULE, dc.DTDISABLED
                from dbo.IMASSET_ETD_MODEL ima
                   inner join dbo.VW_ASSET_ETD_EXPANDED ae on (ae.IDDC = ima.IDDC) and (ae.IDASSET = ima.IDASSET)
                           inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = ae.IDDC) 
                inner join dbo.MATURITYRULE mr on (mr.IDMATURITYRULE = ae.IDMATURITYRULE)
                 ) ima on ((ima.IDDC = c.IDDC) or (c.IDDC is null))
              left outer join dbo.QUOTE_INDEX_H qi on (ima.ASSETCATEGORY = 'Index')
                                                  and (qi.IDASSET = ima.IDASSET_UNL)
                                                    and (qi.TIME = @DTBUSINESS)
                                                    and (qi.QUOTETIMING = 'Close')
                                                    and (qi.QUOTESIDE = 'OfficialClose')
              left outer join dbo.QUOTE_EQUITY_H qe on (ima.ASSETCATEGORY = 'EquityAsset')
                                                   and (qe.IDASSET = ima.IDASSET_UNL)
                                                     and (qe.TIME = @DTBUSINESS)
                                                     and (qe.QUOTETIMING = 'Close')
                                                     and (qe.QUOTESIDE = 'OfficialClose')
            where (m.DTBUSINESS = @DTBUSINESS) and ((ima.DTDISABLED is null) or (ima.DTDISABLED > @DTBUSINESS))
            
                union all

            /* Class parameters for disabled contracts */
            select distinct c.IDDC, c.IDASSET_UNL,
                   isnull(qi.VALUE, qe.VALUE) as ASSET_UNL_QUOTE, c.UNDERLYINGPRICE as CLASS_ASSET_UNL_QUOTE,
                   c.CLASSGROUP, c.PRODUCTGROUP, ima.CONTRACTMULTIPLIER, c.CONTRACTMULTIPLIER as CLASS_CONTRACTMULTIPLIER, 
                   isnull(ima.CONTRACTSYMBOL, c.CONTRACTSYMBOL) as CONTRACTSYMBOL, 
                   c.DELIVERYRATE, c.OFFSET, c.SPOTSPREADRATE, c.NONSPOTSPREADRATE,  c.MINIMUMMARGIN, c.EXPIRYTIME,
                   c.CATEGORY as CATEGORY, ima.IDC as IDC, ima.CONTRACTDISPLAYNAME as CONTRACTDESCRIPTION, ima.MMYRULE,
                   c.UNDERLYINGCONTRACT as UNDERLYINGCONTRACT, c.ISINCODE,
                   0 as ISACTIVE
              from dbo.IMMARSCLASS_H c
                    inner join dbo.IMMARS_H m on (m.IDIMMARS_H = c.IDIMMARS_H)
             inner join 
                 (
                      select max(om.DTBUSINESS) as DTBUSINESS, ima.IDDC,
                             ae.CONTRACTMULTIPLIER, ae.CONTRACTSYMBOL, ae.CONTRACTDISPLAYNAME, ae.IDASSET_UNL, ae.IDC, ae.ASSETCATEGORY,
                          mr.MMYRULE
                from dbo.IMASSET_ETD_MODEL ima
                       inner join dbo.VW_ASSET_ETD_EXPANDED ae on (ae.IDDC = ima.IDDC) and (ae.IDASSET = ima.IDASSET)
                    inner join dbo.MATURITYRULE mr on (mr.IDMATURITYRULE = ae.IDMATURITYRULE)
                            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = ae.IDDC) and (dc.DTDISABLED is not null) and (dc.DTDISABLED <= @DTBUSINESS)
                           inner join dbo.IMMARSCLASS_H oc on (oc.IDDC = dc.IDDC)
                               inner join dbo.IMMARS_H om on (om.IDIMMARS_H = oc.IDIMMARS_H)
                               group by ima.IDDC, ae.CONTRACTMULTIPLIER, ae.CONTRACTSYMBOL, ae.CONTRACTDISPLAYNAME, ae.IDASSET_UNL, ae.IDC, ae.ASSETCATEGORY, mr.MMYRULE
                 ) ima on (ima.IDDC = c.IDDC)
              left outer join dbo.QUOTE_INDEX_H qi on (ima.ASSETCATEGORY = 'Index')
                                                  and (qi.IDASSET = ima.IDASSET_UNL)
                                                  and (qi.TIME = m.DTBUSINESS)
                                                  and (qi.QUOTETIMING = 'Close')
                                                  and (qi.QUOTESIDE = 'OfficialClose')
              left outer join dbo.QUOTE_EQUITY_H qe on (ima.ASSETCATEGORY = 'EquityAsset')
                                                   and (qe.IDASSET = ima.IDASSET_UNL)
                                                   and (qe.TIME = m.DTBUSINESS)
                                                   and (qe.QUOTETIMING = 'Close')
                                                   and (qe.QUOTESIDE = 'OfficialClose')
            where (m.DTBUSINESS = ima.DTBUSINESS)";

        // EG 20180803 PERF Upd
        const string QUERYRISKARRAYTIMSIDEMMETHOD = @"
            select ra.IDASSET, cl.IDDC, 
            q.VALUE as QUOTE, ra.MARKPRICE as RISKARRAY_QUOTE, 
            ra.STRIKEPRICE as RISKARRAY_STRIKEPRICE,
            ra.DOWNSIDE5, ra.DOWNSIDE4, ra.DOWNSIDE3, ra.DOWNSIDE2, ra.DOWNSIDE1,
            ra.UPSIDE1, ra.UPSIDE2, ra.UPSIDE3, ra.UPSIDE4, ra.UPSIDE5,
                   ra.SHORTOPTADJUSTMENT as SHORTADJ, ra.MATURITYYEARMONTH, ra.CATEGORY, ra.CONTRACTSYMBOL, ra.ISINCODE, 1 as ISACTIVE
              from dbo.IMMARSRISKARRAY_H ra 
             inner join dbo.IMMARS_H m on (m.IDIMMARS_H = ra.IDIMMARS_H)
             inner join dbo.IMMARSCLASS_H cl on ((cl.IDIMMARS_H = ra.IDIMMARS_H) and (cl.CONTRACTSYMBOL = ra.CONTRACTSYMBOL) and (cl.CATEGORY = ra.CATEGORY))
            left outer join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = ra.IDASSET) and (ima.IDDC = cl.IDDC)
            left outer join dbo.QUOTE_ETD_H q on (q.IDASSET = ra.IDASSET)
            left outer join dbo.VW_ASSET_ETD_EXPANDED a on (a.IDASSET = ra.IDASSET)
             where ((m.DTBUSINESS = @DTBUSINESS)
                 or ((m.DTBUSINESS = a.MATURITYDATE)
                   and (a.MATURITYDATE < @DTBUSINESS)
                   and (a.DELIVERYDATE > @DTBUSINESS)
                   and (a.CATEGORY = 'F')))
            and (((ima.IDASSET = ra.IDASSET) and (ima.IDDC = cl.IDDC)) or (ra.IDASSET is null) or (cl.IDDC is null))
               and (((q.TIME = m.DTBUSINESS) and (q.QUOTETIMING = 'Close') and (q.QUOTESIDE = 'OfficialClose')) or (q.TIME is null))

            union all

            /* Risk Array parameters for disabled contracts in position (delivery waiting)*/
            select ra.IDASSET, cl.IDDC, 
                   q.VALUE as QUOTE, ra.MARKPRICE as RISKARRAY_QUOTE, 
                   ra.STRIKEPRICE as RISKARRAY_STRIKEPRICE,
                   ra.DOWNSIDE5, ra.DOWNSIDE4, ra.DOWNSIDE3, ra.DOWNSIDE2, ra.DOWNSIDE1,
                   ra.UPSIDE1, ra.UPSIDE2, ra.UPSIDE3, ra.UPSIDE4, ra.UPSIDE5,
                   ra.SHORTOPTADJUSTMENT as SHORTADJ, ra.MATURITYYEARMONTH, ra.CATEGORY, ra.CONTRACTSYMBOL, ra.ISINCODE, 0 as ISACTIVE
              from dbo.IMMARSRISKARRAY_H ra 
             inner join dbo.IMMARS_H m on (m.IDIMMARS_H = ra.IDIMMARS_H)
             inner join dbo.IMMARSCLASS_H cl on ((cl.IDIMMARS_H = ra.IDIMMARS_H) and (cl.CONTRACTSYMBOL = ra.CONTRACTSYMBOL) and (cl.CATEGORY = ra.CATEGORY))
             inner join 
                 (
	               select max(om.DTBUSINESS) as DTBUSINESS, dc.IDDC, ae.CONTRACTMULTIPLIER, ae.CONTRACTSYMBOL, oc.CATEGORY
	            from dbo.IMASSET_ETD_MODEL ima
	                inner join dbo.VW_ASSET_ETD_EXPANDED ae on (ae.IDDC = ima.IDDC) and (ae.IDASSET = ima.IDASSET)
			        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = ae.IDDC) and (dc.DTDISABLED is not null) and (dc.DTDISABLED <= @DTBUSINESS)
 			        inner join dbo.IMMARSCLASS_H oc on (oc.IDDC = dc.IDDC)
				    inner join dbo.IMMARS_H om on (om.IDIMMARS_H = oc.IDIMMARS_H)
				    group by dc.IDDC, ae.CONTRACTMULTIPLIER, ae.CONTRACTSYMBOL, oc.CATEGORY
                 ) dc on (dc.IDDC = cl.IDDC)
              left outer join dbo.QUOTE_ETD_H q on (q.IDASSET = ra.IDASSET)
	         where (m.DTBUSINESS = dc.DTBUSINESS)
               and (ra.IDASSET is null) 
               and (((q.TIME = m.DTBUSINESS) and (q.QUOTETIMING = 'Close') and (q.QUOTESIDE = 'OfficialClose')) or (q.TIME is null))";

        // FI 20190304 [24522] => inner join dbo.ASSET_EQUITY equ on equ.ISINCODE=d13.ISINCODE and equ.IDM = m.IDM 
        const string QUERYPOSITIONACTIONSCCG = @"
            select 
            a_compart.IDA as ACTORID,
            b_compart.IDB as BOOKID,
            NULL as IDT,
            IDASSET as IDASSET,
            case 
                when (d13.LONGPOSITION > d13.SHORTPOSITION) then '1'
                when (d13.SHORTPOSITION > d13.LONGPOSITION) then '2'
                else ''
            end as SIDE,
            a_compart.IDA as CLEARERID,
            b_compart.IDB as BOOKCLEARERID,
            ABS(d13.SHORTPOSITION - d13.LONGPOSITION) as QTY,
            NULL as IDDC,
            d13.SYMBOL as CONTRACTSYMBOL,
            equ.IDM as MARKETID,
            NULL as EXEASSQTY,
            NULL as DELIVERYDATE,
            d13.POSITIONTYPE as POSITIONTYPE,
            d13.LONGPOSITIONCTRVAL as LONGPOSITIONCTRVAL,
            d13.SHORTPOSITIONCTRVAL as SHORTPOSITIONCTRVAL,
            d13.CURRENCY as CURRENCY,
            d13.EXPIRY as MATURITYDATE
            from dbo.CCG_D13A d13
            inner join dbo.CSMID csm on csm.CSSMEMBERCODE={0}
                                and csm.CSSMEMBERIDENT='XCCG' 
                                and csm.IDA = @IDENTITY                
            inner join dbo.ACTOR a_css on a_css.IDA=csm.IDA_CSS 
                                  and a_css.IDA = @IDACSS                   
            inner join dbo.MARKET m on m.EXCHANGESYMBOL={1} and isnull(m.IDA,a_css.IDA)=a_css.IDA
           --inner join dbo.ASSET_EQUITY equ on equ.EXCHANGESUBID=d13.ISINCODE            
            inner join dbo.ASSET_EQUITY equ on equ.ISINCODE=d13.ISINCODE and equ.IDM = m.IDM 
            left outer join dbo.ACTORROLE a_compart on a_compart.IDA_ACTOR=a_css.IDA and a_compart.IDROLEACTOR=case when d13.ACCOUNT='C' then 'CCLEARINGCOMPART' when d13.ACCOUNT='F' then 'HCLEARINGCOMPART' else null end
            left outer join dbo.BOOK b_compart on b_compart.IDA=a_compart.IDA
            where d13.DTBUSINESS=@DTBUSINESS and d13.PRODUCTTYPE in ('C','F')";

        #region Query QUERYTIMSIDEMCONTRACTPARAM
        // PM 20130321 AGREX
        /// <summary>
        /// Select TIMS IDEM margin parameters by Derivative Contract
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        // EG 20140521 [19972] Refactoring DTENABLED/DTDISABLED
        // PM 20170220 [22881][22942] Suppression de la vérification de DTENABLED/DTDISABLED sur le DC, en laissant la vérification sur les autres niveaux
        //                     car lors d'une CA le DC peut être mis disable, mais il faut continuer à considérer les positions en livraison suite à EXE/ASS
        // EG 20180803 PERF Upd
        const string QUERYTIMSIDEMCONTRACTPARAM = @"
            select distinct
            dc.IDDC, dc.IDENTIFIER, dc.DISPLAYNAME,
            dc.CONTRACTSYMBOL, dc.IDM,
            case when tc.ISCLASSBYMATURITY is null then 0 else tc.ISCLASSBYMATURITY end as ISCLASSBYMATURITY,
            tc.IDDC_DELIVERY, tc.IDDC_UNCOVEREDDLV, tc.IDDC_MATCHEDDLV, tc.IDDC_MATCHEDWITHDRAW, 
            tc.PERIODMLTPDLVMGROFFSET, tc.PERIODDLVMGROFFSET, tc.DAYTYPEDLVMGROFFSET, tc.PERIODMLTPINCRMGROFFSET,
            tc.PERIODINCRMGROFFSET, tc.DAYTYPEINCRMGROFFSET, tc.PERIODMLTPGROSSMGROFFSET, tc.PERIODGROSSMGROFFSET, tc.DAYTYPEGROSSMGROFFSET
            from dbo.DERIVATIVECONTRACT dc
            left outer join dbo.IMTIMSIDEMCONTRACT tc on (tc.IDDC = dc.IDDC)
            where exists 
            ( 
                select 1
                from dbo.DERIVATIVEATTRIB da
                inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
                inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = a.IDASSET)
                where (da.IDDC = dc.IDDC ) and 
                      (a.DTENABLED <= @DTBUSINESS ) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS)) and
                      (da.DTENABLED <= @DTBUSINESS ) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS)))";

        #endregion Query QUERYTIMSIDEMCONTRACTPARAM
        #region Query QUERYTIMSIDEMASSETDELIVERY
        // EG 20180803 PERF Upd
        const string QUERYTIMSIDEMASSETDELIVERY = @"
            select ima.IDASSET, ima.IDDC, newa.IDASSET as IDASSET_NEW, newdc.IDDC_NEW,
                   m.MATURITYDATE as APPLYSTARTDATE, m.DELIVERYDATE as APPLYENDDATE, newdc.DELIVERYSTEP
            from dbo.IMASSET_ETD_MODEL ima
             inner join dbo.ASSET_ETD a on a.IDASSET = ima.IDASSET
             inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB
             inner join dbo.MATURITY m on m.IDMATURITY = da.IDMATURITY
             inner join
            ( 
                select imc.IDDC, imc.IDDC_DELIVERY as IDDC_NEW, 'DELIVERY' as DELIVERYSTEP
                          from dbo.IMTIMSIDEMCONTRACT imc
                         union all
                        select imc.IDDC, imc.IDDC_UNCOVEREDDLV as IDDC_NEW, 'UNCOVEREDDLV' as DELIVERYSTEP
                          from dbo.IMTIMSIDEMCONTRACT imc
                         union all
                        select imc.IDDC, imc.IDDC_MATCHEDDLV as IDDC_NEW, 'MATCHEDDLV' as DELIVERYSTEP
                          from dbo.IMTIMSIDEMCONTRACT imc
                         union all
                        select imc.IDDC, imc.IDDC_MATCHEDWITHDRAW as IDDC_NEW, 'MATCHEDWITHDRAW' as DELIVERYSTEP
                          from dbo.IMTIMSIDEMCONTRACT imc
                       ) newdc on newdc.IDDC = ima.IDDC
             inner join dbo.DERIVATIVEATTRIB newda on  newda.IDDC = newdc.IDDC_NEW
             inner join dbo.MATURITY newm on newm.IDMATURITY = newda.IDMATURITY
             inner join dbo.ASSET_ETD newa on newa.IDDERIVATIVEATTRIB = newda.IDDERIVATIVEATTRIB
             where (newm.MATURITYMONTHYEAR = m.MATURITYMONTHYEAR)
               and ((newa.PUTCALL = a.PUTCALL) or (newa.PUTCALL is null and a.PUTCALL is null))
               and ((newa.STRIKEPRICE = a.STRIKEPRICE) or (newa.STRIKEPRICE is null and a.STRIKEPRICE is null))";
        #endregion Query QUERYTIMSIDEMASSETDELIVERY
        #region Query QUERYTIMSIDEMINSERTIMASSETETD
        const string QUERYTIMSIDEMINSERTIMASSETETD = @"
            insert into dbo.IMASSET_ETD_MODEL (IDASSET, IDDC)
            select distinct newa.IDASSET, newdc.IDDC_NEW
            from dbo.IMASSET_ETD_MODEL ima
             inner join dbo.ASSET_ETD a on a.IDASSET = ima.IDASSET
             inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB
             inner join dbo.MATURITY m on m.IDMATURITY = da.IDMATURITY
             inner join
            ( 
                select imc.IDDC, imc.IDDC_DELIVERY as IDDC_NEW
                          from dbo.IMTIMSIDEMCONTRACT imc
                         union all
                        select imc.IDDC, imc.IDDC_UNCOVEREDDLV as IDDC_NEW
                          from dbo.IMTIMSIDEMCONTRACT imc
                         union all
                        select imc.IDDC, imc.IDDC_MATCHEDDLV as IDDC_NEW
                          from dbo.IMTIMSIDEMCONTRACT imc
                         union all
                        select imc.IDDC, imc.IDDC_MATCHEDWITHDRAW as IDDC_NEW
                          from dbo.IMTIMSIDEMCONTRACT imc
                       ) newdc on newdc.IDDC = ima.IDDC
             inner join dbo.DERIVATIVEATTRIB newda on  newda.IDDC = newdc.IDDC_NEW
             inner join dbo.MATURITY newm on newm.IDMATURITY = newda.IDMATURITY
             inner join dbo.ASSET_ETD newa on newa.IDDERIVATIVEATTRIB = newda.IDDERIVATIVEATTRIB
             where (newm.MATURITYMONTHYEAR = m.MATURITYMONTHYEAR)
               and ((newa.PUTCALL = a.PUTCALL) or (newa.PUTCALL is null and a.PUTCALL is null))
               and ((newa.STRIKEPRICE = a.STRIKEPRICE) or (newa.STRIKEPRICE is null and a.STRIKEPRICE is null))";
        #endregion Query QUERYTIMSIDEMINSERTIMASSETETD
        #endregion TIMS IDEM

        #region TIMS EUREX
        // 20120712 MF Ticket 18004 Added join on IMASSET_ETD
        /// <summary>
        /// Select all the EUREX products including class/group hierarchy information and related Spheres derivative contract
        /// </summary>
        /// <remarks>Mandatory parameters: list of IDDC, including all the derivative contracts identifier for the global positions set</remarks>
        // EG 20180803 PERF Upd
        const string QUERYCONTRACTTIMSEUREX = @"
            select 
            dc.IDDC,
            pec.CONTRACTSYMBOL,
            pec.MARGINCLASS, 
            pec.MARGINGROUP,
            dc.CATEGORY,
            dc.CONTRACTMULTIPLIER,
            dc.ASSETCATEGORY,
            dc.IDASSET_UNL,
            dc.IDDC_UNL,
            pec.OFFSET,
            pec.MATURITY_SWITCH,
            pec.SPOTMONTH_SPREADRATE,
            pec.BACKMONTH_SPREADRATE,
            pec.OOM_MINIMUMRATE,
            pec.EXPIRYMONTH_FACTOR,
            pec.MARGIN_STYLE,
            pec.TICKSIZE,
            pec.TICKVALUE,
            dc.IDC_PRICE,
            m.ISO10383_ALPHA4,
            dc.DTENABLED,
            dc.DTDISABLED,
            mr.MMYRULE
            from dbo.PARAMSEUREX_CONTRACT pec 
            inner join dbo.DERIVATIVECONTRACT dc on dc.CONTRACTSYMBOL = pec.CONTRACTSYMBOL
            left outer join dbo.MATURITYRULE mr on mr.IDMATURITYRULE = dc.IDMATURITYRULE
            inner join dbo.MARKET m on m.IDM = dc.IDM
            inner join (select IDDC from dbo.IMASSET_ETD_MODEL group by IDDC) ima on ima.IDDC = dc.IDDC";

        // 20120712 MF Ticket 18004 Added join on IMASSET_ETD
        /// <summary>
        /// Select the EUREX maturity information. Including yeld and security free interest rates.
        /// One EUREX maturity element corresponds to one Spheres DERIVATIVEATTRIBUTE entry, with a supplementary PutCall indicator.  
        /// No relations with Spheres elements are needed.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date, WARNING: pass date values without time part (20110706T00:00:00)</item>
        /// <item>A contract symbols list, including all the contracts in position 
        /// (usually the list descends by the QUERYCONTRACTPARAMSTIMSEUREX request )</item>
        /// </list></remarks>
        // RD/FL 20150415 [20952]- Initial Margin TIMS EUREX : Wrong results for Additional Margin when the "maturity factor" is different from 1
        // Return always 1 for MATURITY_FACTOR
        // Replace in query "pem.MATURITY_FACTOR" by "1 as MATURITY_FACTOR"
        const string QUERYMATURITYTIMSEUREX = @"
            select distinct
            pem.IDPARAMSEUREX_MATURITY as MATURITYID,
            pem.CONTRACTSYMBOL,
            pem.PUTCALL,
            pem.MATURITYYEARMONTH,
            1 as MATURITY_FACTOR,
            pem.THEORETICAL_INTEREST_RATE,
            pem.THEORETICAL_YIELD_RATE
            from dbo.PARAMSEUREX_MATURITY pem
            inner join dbo.DERIVATIVECONTRACT dc on dc.CONTRACTSYMBOL = pem.CONTRACTSYMBOL
            inner join (select IDDC from dbo.IMASSET_ETD_MODEL group by IDDC) ima on ima.IDDC = dc.IDDC
            where DTMARKET = @DTBUSINESS";

        /// <summary>
        /// Select all the asset EUREX parameters, including asset volatility and quote values (asset and underlying).
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>current business date</item>
        /// <item>list of IDASSET, including all the assets identifier for the global positions set</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYASSETTIMSEUREX = @"
            select
            ae.IDASSET, 
            ae.IDPARAMSEUREX_MATURITY as MATURITYID,
            ae.TRADE_UNIT,
            ae.VALUE_QUOTE_ASSETETD,
            ae.VALUE_QUOTE_UNL,
            ae.VOLATILITY,
            ima.IDDC
            from dbo.PARAMSEUREX_ASSETETD ae
            inner join dbo.PARAMSEUREX_MATURITY m on m.IDPARAMSEUREX_MATURITY = ae.IDPARAMSEUREX_MATURITY
            inner join dbo.IMASSET_ETD_MODEL ima on ima.IDASSET = ae.IDASSET
            where m.DTMARKET = @DTBUSINESS";

        /// <summary>
        /// Select all the volatility parameters related to an asset/maturity for a specific scenario 
        /// (RISKARRAY_INDEX, key with asset/maturity)/underlying price indicator(QUOTE_ETD_UNL_COMPARE)
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>current business date</item>
        /// <item>list of IDASSET, including all the assets identifier for the global positions set</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYVOLATILITYTIMSEUREX = @"
            select 
            v.IDASSET,
            v.IDPARAMSEUREX_MATURITY as  MATURITYID,
            v.RISKARRAY_INDEX,
            v.QUOTE_ETD_UNL_COMPARE,
            v.UPVOLATILITY,
            v.UPTHEORETICAL_VALUE,
            v.UPSHORTOPTADJUSTMENT,
            v.NTRLVOLATILITY,
            v.NTRLTHEORETICAL_VALUE,
            v.NTRLSHORTOPTADJUSTMENT,
            v.DOWNVOLATILITY,
            v.DOWNTHEORETICAL_VALUE,
            v.DOWNSHORTOPTADJUSTMENT,
            v.QUOTE_UNL_INDICATOR,
            v.RISKVALUE_EXEASS,
            case when v.UPTHEORETICAL_VALUE is null then 0 else 1 end as UPTHEORETICAL_EXISTS,
            case when v.NTRLTHEORETICAL_VALUE is null  then 0 else 1 end as NTRLTHEORETICAL_EXISTS,
            case when  v.DOWNTHEORETICAL_VALUE is null  then 0 else 1 end as DOWNTHEORETICAL_EXISTS
            from dbo.PARAMSEUREX_VOLATILITY v
            inner join dbo.PARAMSEUREX_ASSETETD ae on ae.IDASSET = v.IDASSET
            inner join dbo.IMASSET_ETD_MODEL ima on ima.IDASSET = ae.IDASSET
            inner join dbo.PARAMSEUREX_MATURITY m on m.IDPARAMSEUREX_MATURITY = ae.IDPARAMSEUREX_MATURITY 
                                                    and m.IDPARAMSEUREX_MATURITY = v.IDPARAMSEUREX_MATURITY
            where m.DTMARKET = @DTBUSINESS";

        /// <summary>
        /// Select all the exchange rate related to the CSS currency for the current business date
        /// </summary>
        /// <remarks>for each currency pair we have a debit rate (ask) and a credit rate (bid), 
        /// the credit rate is used to convert POSITIVE margin amounts, usually credits for the dealer
        /// <listheader>Mandatory parameters</listheader>
        /// <item>current business date</item>
        /// <item>CSS currency</item>
        /// </list>
        /// </remarks>
        const string QUERYFXRATETIMSEUREX = @"
            select 
            fx.QCP_IDC1 as IDC_TO, 
            fx.QCP_IDC2 as IDC_FROM,
            fx.DTENABLED,
            fx.DTDISABLED,
            qfx.VALUE,
            qfx.QUOTESIDE,
            qfx.IDMARKETENV,
            qfx.IDVALSCENARIO,
            qfx.ISENABLED
            from dbo.ASSET_FXRATE fx
            inner join dbo.QUOTE_FXRATE_H qfx on qfx.IDASSET = fx.IDASSET
            where 
            fx.PRIMARYRATESRC = 'ClearingOrganization' and
            fx.QCP_QUOTEBASIS = 'Currency1PerCurrency2' and 
            fx.QCP_IDC1 = @CSSCURRENCY and
            fx.QCP_IDC1 != fx.QCP_IDC2 and 
            qfx.SOURCE = 'ClearingOrganization' and
            qfx.QUOTETIMING = 'Close' and
            qfx.QUOTEUNIT = 'ExchangeRate' and
            qfx.TIME = @DTBUSINESS";
        #endregion TIMS EUREX

        #region SPAN Queries
        /// <summary>
        /// Select Clearing House specific SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The clearing house internal Id</item>
        /// </list></remarks>
        // PM 20160404 [22116] N'est plus utilisé
        //        #region Query QUERYSPANCLEARINGORG
        //        // EG 20140521 [19972] Refactoring DTDISABLED
        //        // PM 20150930 [21134] Add SCANOFFSETCAPPCT

        //        static string QUERYSPANCLEARINGORG = @"select distinct
        //            ch.IDA, ch.ISIMINTERCOMSPRD, ch.IMWEIGHTEDRISKMETH, ch.IMROUNDINGPREC, ch.IMROUNDINGDIR, ch.IDC, ch.SCANOFFSETCAPPCT
        //            from dbo.CSS ch 
        //            where ( ch.IDA = @IDA_CSS ) and (ch.DTENABLED <= @DTBUSINESS ) and ((ch.DTDISABLED is null) or (ch.DTDISABLED > @DTBUSINESS))";
        //        #endregion Query QUERYSPANCLEARINGORG

        /// <summary>
        /// Select the expanded informations (maturity,currency,...) of ETD assets.
        /// 
        /// Attention: les colonnes de cette requête doivent être les même que la requête QUERYUNDERLYINGASSETFUTUREEXPANDED
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// </list></remarks>
        #region Query QUERYASSETEXPANDED
        // PM 20140509 [19970][19259] Ajout de l'utilisation du FACTOR de la devise du prix lorsque ce n'est pas la devise cotée
        // EG 20140521 [19972] Refactoring DTENABLED/DTDISABLED
        // PM 20150707 [21104] Ajout jointure avec MARKET pour lecture de CASHFLOWCALCMETHOD
        // PM 20170220 [22881][22942] Suppression de la vérification de DTENABLED/DTDISABLED sur le DC, en laissant la vérification sur les autres niveaux
        //                     car lors d'une CA le DC peut être mis disable, mais il faut continuer à considérer les positions en livraison suite à EXE/ASS
        //    (dc.DTENABLED <= @DTBUSINESS ) and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @DTBUSINESS)) and
        // EG 20180803 PERF Upd
        // PM 20171212 [23646] Add dc.PRICEDECLOCATOR
        // PM 20220111 [25617] Ajout m.ACRONYM
        // PM 20220915 [XXXXX] Ajout ma.MATURITYDATESYS
        static readonly string QUERYASSETEXPANDED = @"
            select distinct 
            dc.IDDC, dc.CONTRACTSYMBOL, dc.CATEGORY, dc.IDC_PRICE, dc.FUTVALUATIONMETHOD, dc.INSTRUMENTNUM, dc.INSTRUMENTDEN, dc.PRICEDECLOCATOR,
            dc.ASSETCATEGORY, dc.IDM, dc.IDDC_UNL, 
            case when dc.ASSETCATEGORY = 'Future' then da.IDASSET else dc.IDASSET_UNL end IDASSET_UNL,
            a.IDASSET, a.PUTCALL, a.STRIKEPRICE, ma.MATURITYMONTHYEAR, ma.MATURITYDATE, ma.MATURITYDATESYS,
            (case
                when a.CONTRACTMULTIPLIER is null
                then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end )
                else a.CONTRACTMULTIPLIER
            end)
            /
            (case
                when (c.IDC != isnull(c.IDCQUOTED,c.IDC)) and ((isnull(c.FACTOR,1)) > 0)
                then isnull(c.FACTOR,1)
                else 1
            end) CONTRACTMULTIPLIER,
            c.ROUNDDIR,
            c.ROUNDPREC,
            m.CASHFLOWCALCMETHOD,
            m.ACRONYM, m.EXCHANGEACRONYM
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
            inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = a.IDASSET)
            inner join dbo.CURRENCY c on (c.IDC = dc.IDC_PRICE)
            inner join dbo.MARKET m on (m.IDM = dc.IDM)
            where (a.DTENABLED <= @DTBUSINESS ) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS)) and
            (da.DTENABLED <= @DTBUSINESS ) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS)) and 
            (ma.DTENABLED <= @DTBUSINESS ) and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))";
        #endregion Query QUERYASSETEXPANDED

        #region QUERYUNDERLYINGASSETFUTUREEXPANDED
        /// <summary>
        /// Select the expanded informations of the future underlying assets of ETD assets.
        /// 
        /// Attention: les colonnes de cette requête doivent être les même que la requête QUERYASSETEXPANDED
        /// </summary>
        // PM 20230929 [XXXXX] Ajout
        const string QUERYUNDERLYINGASSETFUTUREEXPANDED = @"
        with ASSET_UNL_FUT (IDASSET) as
        (
            select distinct da.IDASSET /* Underlying Future */
	         from dbo.ASSET_ETD a
	        inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
	        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = a.IDASSET)
            where (dc.ASSETCATEGORY = 'Future')
              and (a.DTENABLED <= @DTBUSINESS ) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS))
              and (da.DTENABLED <= @DTBUSINESS ) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS))
        )
        select distinct 
               a.IDDC, a.CONTRACTSYMBOL, a.CATEGORY, a.PRICECURRENCY as IDC_PRICE, a.FUTVALUATIONMETHOD, a.INSTRUMENTNUM, a.INSTRUMENTDEN, NULL as PRICEDECLOCATOR,
               a.ASSETCATEGORY, a.IDM, a.IDDC_UNL, a.IDASSET_UNL,
               a.IDASSET, a.PUTCALL, a.STRIKEPRICE, a.MATURITYMONTHYEAR, a.MATURITYDATE, a.MATURITYDATESYS,
               a.CONTRACTMULTIPLIER,
               c.ROUNDDIR,
               c.ROUNDPREC,
               a.CASHFLOWCALCMETHOD,
               m.ACRONYM,
               m.EXCHANGEACRONYM
          from VW_ASSET_ETD_EXPANDED a
         inner join ASSET_UNL_FUT a_unl_fut on (a_unl_fut.IDASSET = a.IDASSET)
         inner join dbo.CURRENCY c on (c.IDC = a.IDC)
         inner join dbo.MARKET m on (m.IDM = a.IDM)
         where (a.DTENABLED <= @DTBUSINESS ) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS))";
        #endregion Query QUERYUNDERLYINGASSETFUTUREEXPANDED

        #region QUERYUNDERLYINGASSETEXPANDED
        /// <summary>
        /// Select the expanded informations of the underlying assets.
        /// </summary>
        static readonly string QUERYUNDERLYINGASSETEXPANDED = @"
        with ASSET_UNL (IDASSET, ASSETCATEGORY) as
        (
            select distinct 
	        case when dc.ASSETCATEGORY = 'Future' then da.IDASSET else dc.IDASSET_UNL end IDASSET,
	        dc.ASSETCATEGORY  as ASSETCATEGORY
	        from dbo.ASSET_ETD a
	        inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
	        inner join dbo.DERIVATIVECONTRACT dc on (dc.IDDC = da.IDDC)
            inner join dbo.MATURITY ma on (ma.IDMATURITY = da.IDMATURITY)
            inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = a.IDASSET)
            where (a.DTENABLED <= @DTBUSINESS ) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS)) and
            (da.DTENABLED <= @DTBUSINESS ) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS)) and 
            (ma.DTENABLED <= @DTBUSINESS ) and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))
        )
        select assetUnl.IDASSET, assetUnl.ASSETCATEGORY, asset.ISINCODE  
        from ASSET_UNL assetUnl 
        inner join dbo.VW_ASSET asset on asset.IDASSET = assetUnl.IDASSET and asset.ASSETCATEGORY = assetUnl.ASSETCATEGORY
        where (assetUnl.IDASSET is not null) and (assetUnl.ASSETCATEGORY is not null)";
        #endregion Query QUERYUNDERLYINGASSETEXPANDED

        /// <summary>
        /// Select all the Currency SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// </list></remarks>
        #region Query QUERYSPANCURRENCY
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        const string QUERYSPANCURRENCY = @"
            select distinct
                   y.IDIMSPAN_H,
                   y.IDIMSPANCURRENCY_H,
                   y.IDC,
                   y.EXPONENT
              from dbo.IMSPANCURRENCY_H y
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = y.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";
        #endregion Query QUERYSPANCURRENCY

        /// <summary>
        /// Select all the Currency Conversion SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// </list></remarks>
        #region Query QUERYSPANCURCONV
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        const string QUERYSPANCURCONV = @"
            select distinct
                   v.IDIMSPAN_H,
                   v.IDIMSPANCURCONV_H,
                   v.IDC_CONTRACT,
                   v.IDC_MARGIN,
                   v.VALUE,
                   v.SHIFTUP,
                   v.SHIFTDOWN
              from dbo.IMSPANCURCONV_H v
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = v.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";
        #endregion Query QUERYSPANCURCONV

        /// <summary>
        /// Select all the Inter Spread SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// </list></remarks>
        #region Query QUERYSPANINTERSPREAD
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20151224 [POC -MUREX] Add NUMBEROFLEG
        const string QUERYSPANINTERSPREAD = @"
            select distinct
                   k.IDIMSPAN_H,
                   k.IDIMSPANINTERSPR_H,
                   k.COMBINEDGROUPCODE,
                   k.SPREADPRIORITY,
                   k.INTERSPREADMETHOD,
                   k.CREDITRATE,
                   k.SPREADGROUPTYPE,
                   k.CREDITCALCMETHOD,
                   k.ISCDTRATESEPARATED,
                   k.ELIGIBILITYCODE,
                   k.MINNUMBEROFLEG,
                   k.OFFSETRATE,
                   k.NUMBEROFLEG
              from dbo.IMSPANINTERSPR_H k
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = k.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";
        #endregion Query QUERYSPANINTERSPREAD

        /// <summary>
        /// Select all the Inter Spread Leg SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// </list></remarks>
        #region Query QUERYSPANINTERLEG
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150707 [21104] Ajout colonne MATURITYMONTHYEAR
        const string QUERYSPANINTERLEG = @"
            select distinct
                   l.IDIMSPANINTERSPR_H,
                   l.LEGNUMBER,
                   l.ISTARGET,
                   l.EXCHANGEACRONYM,
                   l.COMBCOMCODE,
                   l.IDIMSPANGRPCTR_H,
                   l.DELTAPERSPREAD,
                   l.LEGSIDE,
                   l.ISREQUIRED,
                   l.CREDITRATE,
                   l.IDIMSPANTIER_H,
                   l.MATURITYMONTHYEAR
              from dbo.IMSPANINTERLEG_H l
             inner join dbo.IMSPANINTERSPR_H k on ( k.IDIMSPANINTERSPR_H = l.IDIMSPANINTERSPR_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = k.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";
        #endregion Query QUERYSPANINTERLEG

        /// <summary>
        /// Select all the Exchange Complex SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANEXCHANGECOMPLEX
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        // EG 20180803 PERF Upd
        const string QUERYSPANEXCHANGECOMPLEX = @"
            select distinct
                   s.IDIMSPAN_H,
                   s.EXCHANGECOMPLEX,
                   s.SETTLEMENTSESSION,
                   s.ISOPTIONVALUELIMIT,
                   s.DTBUSINESSTIME,
                   s.DTFILE,
                   s.FILEIDENTIFIER,
                   s.FILEFORMAT
              from dbo.IMSPAN_H s
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPAN_H = s.IDIMSPAN_H )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANEXCHANGE_H = e.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANEXCHANGECOMPLEX

        /// <summary>
        /// Select all the Exchange SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANEXCHANGE
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        // EG 20180803 PERF Upd
        const string QUERYSPANEXCHANGE = @"
            select distinct
                   e.IDIMSPAN_H,
                   e.IDIMSPANEXCHANGE_H,
                   e.EXCHANGEACRONYM,
                   e.EXCHANGESYMBOL
              from dbo.IMSPANEXCHANGE_H e
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANEXCHANGE_H = e.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANEXCHANGE

        /// <summary>
        /// Select all the Combined Group SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANCOMBINEDGROUP
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        // EG 20180803 PERF Upd
        const string QUERYSPANCOMBINEDGROUP = @"
            select distinct
                   b.IDIMSPAN_H,
                   b.IDIMSPANGRPCOMB_H,
                   b.COMBINEDGROUPCODE
              from dbo.IMSPANGRPCOMB_H b
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = b.IDIMSPAN_H )
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPAN_H = s.IDIMSPAN_H )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANEXCHANGE_H = e.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANCOMBINEDGROUP

        /// <summary>
        /// Select all the Contract Group SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANCONTRACTGROUP
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150707 [21104] Use column IDIMSPAN_H on table IMSPANGRPCTR_H instead of IDIMSPANEXCHANGE_H
        //PM 20150707 [21104] Suppression de la jointure avec IMSPANEXCHANGE_H en allant directement chercher IDIMSPAN_H sur IMSPANGRPCTR_H
        //inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
        //inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
        // PM 20150930 [21134] Ajout colonnes ISUSELAMBDA, LAMBDAMIN et LAMBDAMAX
        // EG 20180803 PERF Upd
        const string QUERYSPANCONTRACTGROUP = @"
            /* Contract Groups with contracts in position */
            select g.IDIMSPANGRPCTR_H,
                   g.COMBCOMCODE,
                   g.IDIMSPAN_H,
                   g.IDIMSPANGRPCOMB_H,
                   g.ISOPTIONVALUELIMIT,
                   g.INTRASPREADMETHOD,
                   g.INTERSPREADMETHOD,
                   g.DELIVERYCHARGEMETH,
                   g.NBOFDELIVERYMONTH,
                   g.SOMMETHOD,
                   g.SOMCHARGERATE,
                   g.WEIGHTEDRISKMETHOD,
                   g.MEMBERITOMRATIO,
                   g.HEDGERITOMRATIO,
                   g.SPECULATITOMRATIO,
                   g.MEMBERADJFACTOR,
                   g.HEDGERADJFACTOR,
                   g.SPECULATADJFACTOR,
                   g.STRATEGYSPREADMETH,
                   g.RISKEXPONENT,
                   g.IDC,
                   g.ISUSELAMBDA,
                   g.LAMBDAMIN,
                   g.LAMBDAMAX
              from dbo.IMSPANGRPCTR_H g
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANGRPCTR_H = g.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = g.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )
            union 
            /* Contract Groups members of Inter Spreads that do not need to have contract in position */
            select g.IDIMSPANGRPCTR_H,
                   g.COMBCOMCODE,
                   g.IDIMSPAN_H,
                   g.IDIMSPANGRPCOMB_H,
                   g.ISOPTIONVALUELIMIT,
                   g.INTRASPREADMETHOD,
                   g.INTERSPREADMETHOD,
                   g.DELIVERYCHARGEMETH,
                   g.NBOFDELIVERYMONTH,
                   g.SOMMETHOD,
                   g.SOMCHARGERATE,
                   g.WEIGHTEDRISKMETHOD,
                   g.MEMBERITOMRATIO,
                   g.HEDGERITOMRATIO,
                   g.SPECULATITOMRATIO,
                   g.MEMBERADJFACTOR,
                   g.HEDGERADJFACTOR,
                   g.SPECULATADJFACTOR,
                   g.STRATEGYSPREADMETH,
                   g.RISKEXPONENT,
                   g.IDC,
                   g.ISUSELAMBDA,
                   g.LAMBDAMIN,
                   g.LAMBDAMAX
              from dbo.IMSPANGRPCTR_H g
             inner join dbo.IMSPANINTERSPR_H k on (k.IDIMSPAN_H = g.IDIMSPAN_H)
             inner join dbo.IMSPANINTERLEG_H lt on ((lt.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H)
                                                  and (lt.IDIMSPANGRPCTR_H = g.IDIMSPANGRPCTR_H)
                                                  and (lt.ISTARGET = '1') and (lt.ISREQUIRED = '0'))
             inner join dbo.IMSPANINTERLEG_H l on (l.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H) and (l.ISTARGET = '0')
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANGRPCTR_H = l.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANCONTRACTGROUP

        /// <summary>
        /// Select all the Contract SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANCONTRACT
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150902 [21385] Ajout colonne DELTASCALINGFACTOR
        //PM 20151127 [21571][21605] Ajout MINPRICEINCR, DELTADEN et SCANNINGRANGE
        //PM 20170929 [23472] Suppression du "Distinct" sur la requête et ajout d'un "Union" avec une deuxième requête pour les contrats appartenant à un groupe de contrat cible d'un spread mais pas forcement requis en position
        // EG 20180803 PERF Upd
        const string QUERYSPANCONTRACT = @"
            select c.IDIMSPANCONTRACT_H,
                   c.IDIMSPANEXCHANGE_H,
                   c.IDIMSPANGRPCTR_H,
                   c.CONTRACTSYMBOL,
                   c.TICKVALUE,
                   c.IDC_PRICE,
                   c.ISOPTVARIABLETICK,
                   c.DELTASCALINGFACTOR,
                   c.MINPRICEINCR,
                   c.DELTADEN,
                   c.SCANNINGRANGE
              from dbo.IMSPANCONTRACT_H c
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )
            union
            select c.IDIMSPANCONTRACT_H,
                   c.IDIMSPANEXCHANGE_H,
                   c.IDIMSPANGRPCTR_H,
                   c.CONTRACTSYMBOL,
                   c.TICKVALUE,
                   c.IDC_PRICE,
                   c.ISOPTVARIABLETICK,
                   c.DELTASCALINGFACTOR,
                   c.MINPRICEINCR,
                   c.DELTADEN,
                   c.SCANNINGRANGE
              from dbo.IMSPANCONTRACT_H c
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             inner join dbo.IMSPANINTERSPR_H k on (k.IDIMSPAN_H = s.IDIMSPAN_H)
             inner join dbo.IMSPANINTERLEG_H lt on ((lt.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H)
                                                  and (lt.IDIMSPANGRPCTR_H = c.IDIMSPANGRPCTR_H)
                                                  and (lt.ISTARGET = '1') and (lt.ISREQUIRED = '0'))
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";

        #endregion Query QUERYSPANCONTRACT

        /// <summary>
        /// Select all the Delivery Months SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANDELIVERYMONTH
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150707 [21104] Suppression de la jointure avec IMSPANEXCHANGE_H en allant directement chercher IDIMSPAN_H sur IMSPANGRPCTR_H
        //inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )
        //inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
        // EG 20180803 PERF Upd
        const string QUERYSPANDELIVERYMONTH = @"
            select distinct
                   d.IDIMSPANDLVMONTH_H,
                   d.IDIMSPANGRPCTR_H,
                   d.MONTHNUMBER,
                   d.MATURITYMONTHYEAR,
                   d.CONSUMEDCHARGERATE,
                   d.REMAINCHARGERATE,
                   d.DELTASIGN
              from dbo.IMSPANDLVMONTH_H d
             inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = d.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = g.IDIMSPAN_H )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANGRPCTR_H = G.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANDELIVERYMONTH

        /// <summary>
        /// Select all the Tier SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANMATURITYTIER
        //PM 20130502 [18623] Suppression de la restriction sur les assets en position
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150707 [21104] Suppression de la jointure avec IMSPANEXCHANGE_H en allant directement chercher IDIMSPAN_H sur IMSPANGRPCTR_H
        const string QUERYSPANMATURITYTIER = @"
            select distinct
                   t.IDIMSPANTIER_H,
                   t.IDIMSPANGRPCTR_H,
                   t.SPREADTYPE,
                   t.TIERNUMBER,
                   t.STARTINGMONTHYEAR,
                   t.ENDINGMONTHYEAR,
                   t.SOMCHARGERATE,
                   t.STARTTIERNUMBER,
                   t.ENDTIERNUMBER
              from dbo.IMSPANTIER_H t
             inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = t.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = g.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";
        #endregion Query QUERYSPANMATURITYTIER

        /// <summary>
        /// Select all the Intra Contract Group Spread SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANINTRASPREAD
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150707 [21104] Suppression de la jointure avec IMSPANEXCHANGE_H en allant directement chercher IDIMSPAN_H sur IMSPANGRPCTR_H
        //inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )
        //inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
        // EG 20180803 PERF Upd
        const string QUERYSPANINTRASPREAD = @"
            select distinct
                   i.IDIMSPANINTRASPR_H,
                   i.IDIMSPANGRPCTR_H,
                   i.SPREADTYPE,
                   i.SPREADPRIORITY,
                   i.NUMBEROFLEG,
                   i.CHARGERATE
              from dbo.IMSPANINTRASPR_H i
             inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = i.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = g.IDIMSPAN_H )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANGRPCTR_H = g.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )
             order by i.IDIMSPANGRPCTR_H, i.SPREADPRIORITY, i.SPREADTYPE";
        #endregion Query QUERYSPANINTRASPREAD

        /// <summary>
        /// Select all the Intra Contract Group Spread Leg SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANINTRALEG
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20150707 [21104] Suppression de la jointure avec IMSPANEXCHANGE_H en allant directement chercher IDIMSPAN_H sur IMSPANGRPCTR_H
        //inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = g.IDIMSPANEXCHANGE_H )
        //inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
        // EG 20180803 PERF Upd
        const string QUERYSPANINTRALEG = @"
            select distinct
                   j.IDIMSPANINTRASPR_H,
                   j.IDIMSPANTIER_H,
                   j.LEGNUMBER,
                   j.MATURITYMONTHYEAR,
                   j.DELTAPERSPREAD,
                   j.LEGSIDE
              from dbo.IMSPANINTRALEG_H j
             inner join dbo.IMSPANINTRASPR_H i on ( i.IDIMSPANINTRASPR_H = j.IDIMSPANINTRASPR_H )
             inner join dbo.IMSPANGRPCTR_H g on ( g.IDIMSPANGRPCTR_H = i.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = g.IDIMSPAN_H )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANGRPCTR_H = G.IDIMSPANGRPCTR_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANINTRALEG

        /// <summary>
        /// Select all the Maturity SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANMATURITY
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        //PM 20160829 [22420] Suppression de la restriction sur les assets en position:
        //(Join)      inner join dbo.IMASSET_ETD ima on ( ima.IDASSET = r.IDASSET )
        //(Where)       and ( ima.SESSIONID = @SESSIONID ) 
        //PM 20170929 [23472] Suppression du "Distinct" sur la requête et ajout d'un "Union" avec une deuxième requête pour les échéances des contrats appartenant à un groupe de contrat cible d'un spread mais pas forcement requis en position
        const string QUERYSPANMATURITY = @"
            select m.IDIMSPANCONTRACT_H,
                   m.FUTMMY,
                   m.OPTMMY,
                   m.FUTPRICESCANRANGE,
                   m.DELTASCALINGFACTOR
              from dbo.IMSPANMATURITY_H m
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANCONTRACT_H = m.IDIMSPANCONTRACT_H )
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             inner join dbo.IMSPANARRAY_H r on ( r.IDIMSPANCONTRACT_H = c.IDIMSPANCONTRACT_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )
            union
            select m.IDIMSPANCONTRACT_H,
                   m.FUTMMY,
                   m.OPTMMY,
                   m.FUTPRICESCANRANGE,
                   m.DELTASCALINGFACTOR
              from dbo.IMSPANMATURITY_H m
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANCONTRACT_H = m.IDIMSPANCONTRACT_H )
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             inner join dbo.IMSPANINTERSPR_H k on (k.IDIMSPAN_H = s.IDIMSPAN_H)
             inner join dbo.IMSPANINTERLEG_H lt on ((lt.IDIMSPANINTERSPR_H = k.IDIMSPANINTERSPR_H)
                                                  and (lt.IDIMSPANGRPCTR_H = c.IDIMSPANGRPCTR_H)
                                                  and (lt.ISTARGET = '1') and (lt.ISREQUIRED = '0'))
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )";
        #endregion Query QUERYSPANMATURITY

        /// <summary>
        /// Select all the asset SPAN parameters.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The initial margin timing</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        #region Query QUERYSPANRISKARRAY
        //PM 20131230 [19416] Ajout restriction sur IMSPANARRAY_H.ASSETCATEGORY = 'Future' (car IMASSET_ETD ne contient que des ASSET_ETD)
        //PM 20140217 [19493] Gestion Intra-Day : Prendre les données Intra-Data les plus récentes antérieures à l'heure donnée pour la journée en cours
        //                                        ou au pire les données End Of Day les plus récentes des journées antérieures
        // EG 20180803 PERF Upd
        const string QUERYSPANRISKARRAY = @"
            select r.IDASSET,
                   r.ASSETCATEGORY,
                   r.IDIMSPANCONTRACT_H,
                   r.UNLCONTRACTSYMBOL,
                   r.FUTMMY,
                   r.OPTMMY,
                   r.PUTCALL,
                   r.STRIKEPRICE,
                   r.COMPOSITEDELTA,
                   r.PRICE,
                   r.RISKVALUE1, r.RISKVALUE2, r.RISKVALUE3, r.RISKVALUE4, r.RISKVALUE5, r.RISKVALUE6, r.RISKVALUE7, r.RISKVALUE8,
                   r.RISKVALUE9, r.RISKVALUE10, r.RISKVALUE11, r.RISKVALUE12, r.RISKVALUE13, r.RISKVALUE14, r.RISKVALUE15, r.RISKVALUE16,
                   r.LOTSIZE,
                   r.CONTRACTVALUEFACTOR
              from dbo.IMSPANARRAY_H r
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = r.IDASSET )
             inner join dbo.IMSPANCONTRACT_H c on ( c.IDIMSPANCONTRACT_H = r.IDIMSPANCONTRACT_H )
             inner join dbo.IMSPANEXCHANGE_H e on ( e.IDIMSPANEXCHANGE_H = c.IDIMSPANEXCHANGE_H )
             inner join dbo.IMSPAN_H s on ( s.IDIMSPAN_H = e.IDIMSPAN_H )
             where ( ( ( s.DTBUSINESS = @DTBUSINESS ) and ( s.SETTLEMENTSESSION = 'EOD' ) and ( @TIMING = 'EOD' ) )
                  or ( ( @TIMING != 'EOD' ) and ( s.DTBUSINESSTIME = ( select max( sm.DTBUSINESSTIME )
                                                                         from dbo.IMSPAN_H sm
                                                                        where ( sm.EXCHANGECOMPLEX = s.EXCHANGECOMPLEX )
                                                                          and ( ( ( sm.DTBUSINESSTIME <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'ITD' ) )
                                                                             or ( ( ( sm.DTBUSINESS + 1 ) <= @DTBUSINESS ) and ( sm.SETTLEMENTSESSION = 'EOD' ) ) )
                                                                     ) )
                   ) )
               and ( r.ASSETCATEGORY = 'Future' )";
        #endregion Query QUERYSPANRISKARRAY

        /// <summary>
        /// Select Market Volume pour le Concentration Risk Margin ECC.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        #region Query QUERYECCSPANMARKETVOLUME
        // PM 20190801 [24717] New
        const string QUERYECCSPANMARKETVOLUME = @"
            select mv.COMBCOMSTRESS,
                   mv.MARKETVOLUME
              from dbo.IMECCCONR_H mv
             where (mv.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYECCSPANMARKETVOLUME

        /// <summary>
        /// Select les groupes de contrats pour le Concentration Risk Margin ECC.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        #region Query QUERYECCSPANCOMBCOMSTRESS
        // PM 20190904 [24878] New
        const string QUERYECCSPANCOMBCOMSTRESS = @"
            select distinct mv.COMBCOMSTRESS,
                   a.CONTRACTSYMBOL,
                   a.IDDC
              from dbo.VW_ASSET_ETD_EXPANDED a
             inner join dbo.IMECCCONRCTR_H mc on (mc.CONTRACTSYMBOL = a.CONTRACTSYMBOL)
             inner join dbo.IMECCCONR_H mv on (mv.COMBCOMSTRESS = mc.COMBCOMSTRESS) and (mv.DTBUSINESS = mc.DTBUSINESS)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = a.IDASSET)
             inner join dbo.IMMETHOD im on (im.IDIMMETHOD = a.IDIMMETHOD) and (im.ISCALCECCCONR = 1)
             where (mc.DTBUSINESS = @DTBUSINESS)
               and (mv.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYECCSPANCOMBCOMSTRESS
        #endregion SPAN Queries

        #region CBOE Queries
        #region Query QUERYCBOECONTRACTPARAM
        /// <summary>
        /// Select CBOE margin parameters by Derivative Contract
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        // EG 20140521 [19972] Refactoring DTDISABLED
        // EG 20180803 PERF Upd
        const string QUERYCBOECONTRACTPARAM = @"
            select distinct dc.IDDC, dc.IDENTIFIER, dc.DISPLAYNAME, dc.CONTRACTSYMBOL,
                   case when cc.PCTOPTVALUE is null then cm.PCTOPTVALUE else cc.PCTOPTVALUE end as PCTOPTVALUE,
                   case when cc.PCTUNLVALUE is null then cm.PCTUNLVALUE else cc.PCTUNLVALUE end as PCTUNLVALUE,
                   case when cc.PCTMINVALUE is null then cm.PCTMINVALUE else cc.PCTMINVALUE end as PCTMINVALUE
              from dbo.DERIVATIVECONTRACT dc
              left outer join dbo.IMCBOEMARKET cm on (cm.IDM = dc.IDM) and (cm.ASSETCATEGORY = dc.ASSETCATEGORY)
              left outer join dbo.IMCBOECONTRACT cc on (cc.IDDC = dc.IDDC)
             where exists 
             ( 
                select 1
                from dbo.DERIVATIVEATTRIB da
                inner join dbo.ASSET_ETD a on (a.IDDERIVATIVEATTRIB = da.IDDERIVATIVEATTRIB)
                inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = a.IDASSET)
                where (da.IDDC = dc.IDDC ) and 
                      (a.DTENABLED <= @DTBUSINESS) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS)) and
                      (da.DTENABLED <= @DTBUSINESS) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS))
             ) and 
             (dc.DTENABLED <= @DTBUSINESS) and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @DTBUSINESS))";
        #endregion Query QUERYCBOECONTRACTPARAM
        #region Query QUERYCBOEASSETEXPANDED
        /// <summary>
        /// Select the expanded information (maturity,currency,underlying...) by asset.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        // PM 20140509 [19970][19259] Ajout de l'utilisation du FACTOR de la devise du prix lorsque ce n'est pas la devise cotée
        // EG 20140521 [19972] Refactoring DTDISABLED
        // EG 20180803 PERF Upd
        // PM 20191025 [24983] Ajout MATURITYDATE
        const string QUERYCBOEASSETEXPANDED = @"
            select  dc.IDDC, dc.CATEGORY, dc.IDC_PRICE, dc.FUTVALUATIONMETHOD, dc.INSTRUMENTNUM, dc.INSTRUMENTDEN,
            dc.ASSETCATEGORY, dc.IDDC_UNL, dc.IDASSET_UNL, a.IDASSET, a.PUTCALL, a.STRIKEPRICE,
            ma.MATURITYMONTHYEAR, isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATE,
            (case
                when a.CONTRACTMULTIPLIER is null
                then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end)
                else a.CONTRACTMULTIPLIER
            end)
            /
            (case
                when (c.IDC != isnull(c.IDCQUOTED,c.IDC)) and ((isnull(c.FACTOR,1)) > 0)
                then isnull(c.FACTOR,1)
                else 1
            end) CONTRACTMULTIPLIER,
            va.IDENTIFIER as IDENTIFIER_UNL
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
            inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )
            inner join dbo.MATURITY ma on ( ma.IDMATURITY = da.IDMATURITY )
            inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = a.IDASSET )
            inner join dbo.CURRENCY c on (c.IDC = dc.IDC_PRICE)
            left outer join dbo.VW_ASSET va on ( va.ASSETCATEGORY = dc.ASSETCATEGORY ) and (va.IDASSET = dc.IDASSET_UNL)
            where (dc.ASSETCATEGORY != 'Future') and 
                  (dc.DTENABLED <= @DTBUSINESS) and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @DTBUSINESS)) and
                  (da.DTENABLED <= @DTBUSINESS) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS)) and
                  (a.DTENABLED <= @DTBUSINESS) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS)) and
                  (ma.DTENABLED <= @DTBUSINESS) and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))

            union all
            select  dc.IDDC, dc.CATEGORY, dc.IDC_PRICE, dc.FUTVALUATIONMETHOD, dc.INSTRUMENTNUM, dc.INSTRUMENTDEN,
            dc.ASSETCATEGORY, dc.IDDC_UNL, da.IDASSET as IDASSET_UNL, a.IDASSET, a.PUTCALL, a.STRIKEPRICE,
            ma.MATURITYMONTHYEAR, isnull(ma.MATURITYDATESYS, ma.MATURITYDATE) as MATURITYDATE,
            (case
                when a.CONTRACTMULTIPLIER is null
                then (case when da.CONTRACTMULTIPLIER is null then dc.CONTRACTMULTIPLIER else da.CONTRACTMULTIPLIER end)
                else a.CONTRACTMULTIPLIER
            end)
            /
            (case
                when (c.IDC != isnull(c.IDCQUOTED,c.IDC)) and ((isnull(c.FACTOR,1)) > 0)
                then isnull(c.FACTOR,1)
                else 1
            end) CONTRACTMULTIPLIER,
            va.IDENTIFIER as IDENTIFIER_UNL
            from dbo.ASSET_ETD a
            inner join dbo.DERIVATIVEATTRIB da on ( da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB )
            inner join dbo.DERIVATIVECONTRACT dc on ( dc.IDDC = da.IDDC )
            inner join dbo.MATURITY ma on ( ma.IDMATURITY = da.IDMATURITY )
            inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = a.IDASSET )
            inner join dbo.CURRENCY c on (c.IDC = dc.IDC_PRICE)
            left outer join dbo.VW_ASSET va on ( va.ASSETCATEGORY = 'ExchangeTradedContract' ) and (va.IDASSET = da.IDASSET )
            where (dc.ASSETCATEGORY = 'Future') and 
                  (dc.DTENABLED <= @DTBUSINESS) and ((dc.DTDISABLED is null) or (dc.DTDISABLED > @DTBUSINESS)) and
                  (da.DTENABLED <= @DTBUSINESS) and ((da.DTDISABLED is null) or (da.DTDISABLED > @DTBUSINESS)) and
                  (a.DTENABLED <= @DTBUSINESS) and ((a.DTDISABLED is null) or (a.DTDISABLED > @DTBUSINESS)) and
                  (ma.DTENABLED <= @DTBUSINESS) and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))";
        #endregion Query QUERYCBOEASSETEXPANDED
        #endregion CBOE Queries

        #region MEFF Queries
        #region Query QUERYMEFFCONTRACTASSET
        /// <summary>
        /// Select the parameters for the asset.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The session identifier</item>
        /// Il s'agit de l'union (pas union all) des assets en position
        /// plus les assets (Futures) référencés par les options en tant que margin underlying
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYMEFFCONTRACTASSET = @"
            select mc.IDASSET,
                   mc.ASSETCATEGORY,
                   mc.ASSETCODE,
                   mc.ARRAYCODE,
                   mc.SUBGROUPCODE,
                   mc.MGRUNLASSETCODE,
                   mc.MATURITYDATE,
                   mc.PRICE
              from dbo.IMMEFFCONTRACT_H mc
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = mc.IDASSET )
             where ( mc.BUSINESSDATE = @DTBUSINESS )
            union
            select mc_mrg.IDASSET,
                   mc_mrg.ASSETCATEGORY,
                   mc_mrg.ASSETCODE,
                   mc_mrg.ARRAYCODE,
                   mc_mrg.SUBGROUPCODE,
                   mc_mrg.MGRUNLASSETCODE,
                   mc_mrg.MATURITYDATE,
                   mc_mrg.PRICE
              from dbo.IMMEFFCONTRACT_H mc_mrg
             inner join dbo.IMMEFFCONTRACT_H mc on ( mc.MGRUNLASSETCODE = mc_mrg.ASSETCODE )
                                               and ( mc.BUSINESSDATE = mc_mrg.BUSINESSDATE )
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = mc.IDASSET )
             where ( mc.BUSINESSDATE = @DTBUSINESS )";
        #endregion Query QUERYMEFFCONTRACTASSET
        #region Query QUERYMEFFVALUATIONARRAY
        /// <summary>
        /// Select the valuation array
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// </list></remarks>
        const string QUERYMEFFVALUATIONARRAY = @"
            select va.ARRAYCODE,
                   va.EXPIRYSPAN,
                   va.NBVALUE,
                   va.PRICEFLUCTTYPE,
                   va.PRICEINCFLUCT,
                   va.PRICEDECFLUCT,
                   va.VOLATVARIATIONTYPE,
                   va.VOLATVARIATION,
                   va.SUBGROUPCODE,
                   va.CONTRACTTYPECODE,
                   va.LARGEPOSTHRESHOLD
              from dbo.IMMEFFVALARRAY_H va
             where ( va.BUSINESSDATE = @DTBUSINESS )";
        #endregion Query QUERYMEFFVALUATIONARRAY        
        #region Query QUERYMEFFINTERSPREAD
        /// <summary>
        /// Select the inter spread parameters
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// </list></remarks>
        const string QUERYMEFFINTERSPREAD = @"
            select inter.PRIORITY,
                   inter.ARRAYCODE1,
                   inter.OFFSETDISCOUNT1,
                   inter.OFFSETMULTIPLIER1,
                   inter.ARRAYCODE2,
                   inter.OFFSETDISCOUNT2,
                   inter.OFFSETMULTIPLIER2,
                   inter.DISCOUNTTYPE
              from dbo.IMMEFFINTERSPR_H inter
             where ( inter.BUSINESSDATE = @DTBUSINESS )";
        #endregion Query QUERYMEFFINTERSPREAD 
        #region Query QUERYMEFFINTRASPREAD
        /// <summary>
        /// Select the intra spread parameters
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// </list></remarks>
        const string QUERYMEFFINTRASPREAD = @"
            select intra.ARRAYCODE,
                   intra.FACTOR,
                   intra.MINIMUMVALUE,
                   intra.SPREAD,
                   intra.DAYCALC
              from dbo.IMMEFFINTRASPR_H intra
             where ( intra.BUSINESSDATE = @DTBUSINESS )";
        #endregion Query QUERYMEFFINTRASPREAD
        #region Query QUERYMEFFDELTAARRAY
        /// <summary>
        /// Select the delta array values for the asset.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYMEFFDELTAARRAY = @"
            select mc.IDASSET,
                   rv.SIDE,
                   rv.NBDELTA as NBVALUE,
                   rv.DELTA1 as RISKVALUE1, rv.DELTA2 as RISKVALUE2, rv.DELTA3 as RISKVALUE3, rv.DELTA4 as RISKVALUE4,
                   rv.DELTA5 as RISKVALUE5, rv.DELTA6 as RISKVALUE6, rv.DELTA7 as RISKVALUE7, rv.DELTA8 as RISKVALUE8,
                   rv.DELTA9 as RISKVALUE9, rv.DELTA10 as RISKVALUE10, rv.DELTA11 as RISKVALUE11, rv.DELTA12 as RISKVALUE12,
                   rv.DELTA13 as RISKVALUE13, rv.DELTA14 as RISKVALUE14, rv.DELTA15 as RISKVALUE15
              from dbo.IMMEFFDELTA_H rv
             inner join dbo.IMMEFFCONTRACT_H mc on (mc.BUSINESSDATE = rv.BUSINESSDATE)
                                               and (mc.CONTRACTGROUP = rv.CONTRACTGROUP)
                                               and (mc.ASSETCODE = rv.ASSETCODE)
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = mc.IDASSET )
             where ( rv.BUSINESSDATE = @DTBUSINESS )";
        #endregion Query QUERYMEFFDELTAARRAY
        #region Query QUERYMEFFTHEORPRICEARRAY
        /// <summary>
        /// Select the theoretical price array values for the asset.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Mandatory parameters</listheader>
        /// <item>The current business date</item>
        /// <item>The session identifier</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYMEFFTHEORPRICEARRAY = @"
            select mc.IDASSET,
                   rv.SIDE,
                   rv.NBPRICE as NBVALUE,
                   rv.PRICE1 as RISKVALUE1, rv.PRICE2 as RISKVALUE2, rv.PRICE3 as RISKVALUE3, rv.PRICE4 as RISKVALUE4,
                   rv.PRICE5 as RISKVALUE5, rv.PRICE6 as RISKVALUE6, rv.PRICE7 as RISKVALUE7, rv.PRICE8 as RISKVALUE8,
                   rv.PRICE9 as RISKVALUE9, rv.PRICE10 as RISKVALUE10, rv.PRICE11 as RISKVALUE11, rv.PRICE12 as RISKVALUE12,
                   rv.PRICE13 as RISKVALUE13, rv.PRICE14 as RISKVALUE14, rv.PRICE15 as RISKVALUE15
              from dbo.IMMEFFTHEORPRICE_H rv
             inner join dbo.IMMEFFCONTRACT_H mc on (mc.BUSINESSDATE = rv.BUSINESSDATE)
                                               and (mc.CONTRACTGROUP = rv.CONTRACTGROUP)
                                               and (mc.ASSETCODE = rv.ASSETCODE)
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = mc.IDASSET )
             where ( rv.BUSINESSDATE = @DTBUSINESS )";
        #endregion Query QUERYMEFFTHEORPRICEARRAY
        #endregion MEFF Queries

        #region OMX Queries
        #region Query QUERYOMXVECTORFILE
        /// <summary>
        /// Select the vector file array values for the asset.
        /// </summary>
        /// FI 20130517 [18382] add const
        const string QUERYOMXVCTFILE = @"
            select vct.IDASSET,
                   vct.POINT as POINT,
                   vct.SPOT as SPOT,
                   vct.HELDLOW as HELDLOW, vct.WRITTENLOW as WRITTENLOW,
                   vct.HELDMIDDLE as HELDMIDDLE, vct.WRITTENMIDDLE as WRITTENMIDDLE,
                   vct.HELDHIGH as HELDHIGH, vct.WRITTENHIGH as WRITTENHIGH
             from dbo.IMOMXVCTFILE_H vct
             inner join dbo.IMASSET_ETD_MODEL ima on ( ima.IDASSET = vct.IDASSET )
             where ( vct.DTBUSINESS = @DTBUSINESS )";
        #endregion Query QUERYOMXVECTORFILE
        #endregion OMX Queries

        #region PRISMA Queries
        #region Query QUERYPRISMALIQUIDATIONGROUP
        /// <summary>
        /// Sélectionne les paramètres des liquidation groups.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        const string QUERYPRISMALIQUIDATIONGROUP = @"
            select lg.IDIMPRISMALG_H,
                   lg.IDENTIFIER,
                   lg.CURRENCYTYPEFLAG
              from dbo.IMPRISMALG_H lg
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMALIQUIDATIONGROUP
        #region Query QUERYPRISMALIQUIDATIONGROUPSPLIT
        /// <summary>
        /// Sélectionne les paramètres des liquidation group splits.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        // PM 20180903 [24015] Prisma v8.0: ajout ISCALCULATETEA forcé à 'N' pour retro compatibilité
        const string QUERYPRISMALIQUIDATIONGROUPSPLIT = @"
            select lgs.IDIMPRISMALGS_H,
                   lgs.IDENTIFIER,
                   lgs.IDIMPRISMALG_H,
                   lgs.RISKMETHOD,
                   lgs.AGGREGATIONMETHOD,
                   'N' as ISCALCULATETEA,
              from dbo.IMPRISMALGS_H lgs
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMALIQUIDATIONGROUPSPLIT
        #region Query QUERYPRISMARISKMEASURESET
        /// <summary>
        /// Sélectionne les paramètres des risk measure set de chaque liquidation group splits.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout colonne NBWORSTSCENARIO
        const string QUERYPRISMARISKMEASURESET = @"
            select rmslgs.IDIMPRISMARMSLGS_H,
                   rmslgs.IDIMPRISMALGS_H,
                   rmslgs.IDIMPRISMARMS_H,
                   rms.IDENTIFIER,
                   rmslgs.AGGREGATIONMETHOD,
                   rmslgs.CBCAP,
                   rmslgs.CBCONFIDENCELEVEL,
                   rmslgs.CBFLOOR,
                   rmslgs.CBMULTIPLIER,
                   rmslgs.CBSUBWINDOW,
                   rmslgs.CONFIDENCELEVEL,
                   rmslgs.DFACONFIDENCELEVEL,
                   rmslgs.DFAFLOOR,
                   rmslgs.HISTORICALSTRESSED,
                   rmslgs.ISCORRELATIONBREAK,
                   rmslgs.ISLIQUIDCOMPONENT,
                   rmslgs.ISUSEROBUSTNESS,
                   rmslgs.RISKMEASURE,
                   rmslgs.SCALINGFACTOR,
                   rmslgs.WEIGHTINGFACTOR,
                   rmslgs.NBWORSTSCENARIO
              from dbo.IMPRISMARMSLGS_H rmslgs
             inner join dbo.IMPRISMARMS_H rms on (rms.IDIMPRISMARMS_H = rmslgs.IDIMPRISMARMS_H)
             inner join dbo.IMPRISMALGS_H lgs on (lgs.IDIMPRISMALGS_H = rmslgs.IDIMPRISMALGS_H)
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H) and (m.IDIMPRISMA_H = rms.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMARISKMEASURESET
        #region Query QUERYPRISMAMARKETCAPACITY
        /// <summary>
        /// Sélectionne les paramètres des market capacities.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        const string QUERYPRISMAMARKETCAPACITY = @"
            select mktcapa.IDIMPRISMAMKTCAPA_H,
                   mktcapa.MARKETCAPACITY,
                   mktcapa.TTEBUCKETID,
                   mktcapa.MONEYNESSBUCKETID,
                   mktcapa.PRODUCTID,
                   mktcapa.PRODUCTLINE,
                   mktcapa.PUTCALL,
                   mktcapa.LIQUIDITYPREMIUM
              from dbo.IMPRISMAMKTCAPA_H mktcapa
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = mktcapa.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAMARKETCAPACITY
        #region Query QUERYPRISMALIQUIDITYFACTOR
        /// <summary>
        /// Sélectionne les paramètres des liquidity factors.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        const string QUERYPRISMALIQUIDITYFACTOR = @"
            select liqfact.IDIMPRISMALIQCLASS_H,
                   liqclass.IDENTIFIER,
                   liqfact.PCTMINTHRESHOLD,
                   liqfact.PCTMAXTHRESHOLD,
                   liqfact.MINTHRESHOLDFACTOR,
                   liqfact.MAXTHRESHOLDFACTOR
              from dbo.IMPRISMALIQFACT_H liqfact
             inner join dbo.IMPRISMALIQCLASS_H liqclass on (liqclass.IDIMPRISMALIQCLASS_H = liqfact.IDIMPRISMALIQCLASS_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = liqclass.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMALIQUIDITYFACTOR
        #region Query QUERYPRISMAEXCHANGERATE
        /// <summary>
        /// Sélectionne les paramètres des taux de change.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        const string QUERYPRISMAEXCHANGERATE = @"
            select fxp.IDIMPRISMAFX_H,
                   fx.IDENTIFIER,
                   fxp.IDIMPRISMAFXPAIR_H,
                   fxp.CURRENCYPAIR,
                   fxp.EXCHANGERATE
              from dbo.IMPRISMAFXPAIR_H fxp
             inner join dbo.IMPRISMAFX_H fx on (fx.IDIMPRISMAFX_H = fxp.IDIMPRISMAFX_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = fx.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAEXCHANGERATE
        #region Query QUERYPRISMAEXCHANGERATERMS
        /// <summary>
        /// Sélectionne les paramètres des taux de change des risk measure set.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// </list></remarks>
        const string QUERYPRISMAEXCHANGERATERMS = @"
            select fxrms.IDIMPRISMAFXPAIR_H,
                   fxrms.IDIMPRISMARMS_H,
                   fxrms.SCENARIONO,
                   fxrms.VALUE
              from dbo.IMPRISMAFXRMS_H fxrms
             inner join dbo.IMPRISMAFXPAIR_H fxp on (fxp.IDIMPRISMAFXPAIR_H = fxrms.IDIMPRISMAFXPAIR_H)
             inner join dbo.IMPRISMAFX_H fx on (fx.IDIMPRISMAFX_H = fxp.IDIMPRISMAFX_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = fx.IDIMPRISMA_H)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAEXCHANGERATERMS
        #region Query QUERYPRISMAASSET
        /// <summary>
        /// Sélectionne les paramètres des assets, expirations, products.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// <item>Identifiant de la session</item>
        /// </list></remarks>
        // PM 20140612 [19911] Ajout PVPRICE
        // PM 20180903 [24015] Prisma v8.0: ajout DAYTOEXPIRYBUSINESS (forcé à DAYTOEXPIRY) pour retro compatibilité
        // EG 20180803 PERF Upd
        // PM 20180903 [24015] Prisma v8.0: ajout SETTLTMETHOD et SETTLTTYPE forcé à SETTLTMETHOD pour retro compatibilité
        const string QUERYPRISMAASSET = @"
            select p.IDIMPRISMAP_H,
                   e.IDIMPRISMAE_H,
                   s.IDIMPRISMAS_H,
                   p.PRODUCTID,
                   p.TICKSIZE,
                   p.TICKVALUE,
                   p.IDC,
                   p.IDIMPRISMALIQCLASS_H,
                   p.IDIMPRISMALG_H,
                   p.MARGINSTYLE,
                   e.CYEAR,
                   e.CMONTH,
                   e.YEAR,
                   e.MONTH,
                   e.DAY,
                   e.DAYTOEXPIRY,
                   e.XMMATBUCKETID,
                   e.DAYTOEXPIRY as DAYTOEXPIRYBUSINESS,
                   s.PUTCALL,
                   s.STRIKEPRICE,
                   s.VERSION,
                   s.IDASSET,
                   s.ASSETCATEGORY,
                   s.NPRICE,
                   s.TTEBUCKETID,
                   s.MONEYNESSBUCKETID,
                   s.RISKBUCKET,
                   (case when p.TICKSIZE != 0 then (s.TU * p.TICKVALUE / p.TICKSIZE) else 0 end) as TUV,
                   s.TU,
                   s.STLPRICE,
                   s.PVPRICE,
                   s.SETTLTMETHOD as SETTLTTYPE,
                   s.SETTLTMETHOD
              from dbo.IMPRISMAS_H s
             inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
             inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAASSET
        #region Query QUERYPRISMAASSETLIQUIDGROUPSPLIT
        /// <summary>
        /// Sélectionne les paramètres des liquidation group split des assets.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// <item>Identifiant de la session</item>
        /// </list></remarks>
        /// PM 20161019 [22174] Prisma 5.0 : Ne plus utiliser la colonne LH de la table IMPRISMALGSS_H
        // EG 20180803 PERF Upd
        const string QUERYPRISMAASSETLIQUIDGROUPSPLIT = @"
            select s.IDASSET,
                   lgss.IDIMPRISMAS_H,
                   lgss.IDIMPRISMALGS_H,
                   lgss.ISDEFAULT
              from dbo.IMPRISMALGSS_H lgss
             inner join dbo.IMPRISMALGS_H lgs on (lgs.IDIMPRISMALGS_H = lgss.IDIMPRISMALGS_H)
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMAS_H s on (s.IDIMPRISMAS_H = lgss.IDIMPRISMAS_H)
             inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
             inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H) and (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAASSETLIQUIDGROUPSPLIT
        #region Query QUERYPRISMAASSETRMSLGS
        /// <summary>
        /// Sélectionne les paramètres des risk measure set des liquidation group split des assets.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// <item>Identifiant de la session</item>
        /// </list></remarks>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout colonne LH
        // EG 20180803 PERF Upd
        const string QUERYPRISMAASSETRMSLGS = @"
            select s.IDASSET,
                   rmslgss.IDIMPRISMARMSLGSS_H,
                   rmslgss.IDIMPRISMAS_H,
                   rmslgss.IDIMPRISMARMSLGS_H,
                   rmslgss.IDIMPRISMAFX_H,
                   rmslgss.LH
              from dbo.IMPRISMARMSLGSS_H rmslgss
             inner join dbo.IMPRISMARMSLGS_H rmslgs on (rmslgs.IDIMPRISMARMSLGS_H = rmslgss.IDIMPRISMARMSLGS_H)
             inner join dbo.IMPRISMARMS_H rms on (rms.IDIMPRISMARMS_H = rmslgs.IDIMPRISMARMS_H)
             inner join dbo.IMPRISMALGS_H lgs on (lgs.IDIMPRISMALGS_H = rmslgs.IDIMPRISMALGS_H)
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMAS_H s on (s.IDIMPRISMAS_H = rmslgss.IDIMPRISMAS_H)
             inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
             inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H) and (m.IDIMPRISMA_H = rms.IDIMPRISMA_H) and (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAASSETRMSLGS
        #region Query QUERYPRISMAASSETPRICESCENARIO
        /// <summary>
        /// Sélectionne les paramètres des scénari de prix des assets.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// <item>Identifiant de la session</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYPRISMAASSETPRICESCENARIO = @"
            select s.IDASSET,
                   sps.IDIMPRISMARMSLGSS_H,
                   sps.SCENARIONO,
                   sps.PRICE1,
                   sps.PRICE2,
                   sps.PRICE3,
                   sps.PRICE4,
                   sps.PRICE5
              from dbo.IMPRISMASPS_H sps
             inner join dbo.IMPRISMARMSLGSS_H rmslgss on (rmslgss.IDIMPRISMARMSLGSS_H = sps.IDIMPRISMARMSLGSS_H)
             inner join dbo.IMPRISMARMSLGS_H rmslgs on (rmslgs.IDIMPRISMARMSLGS_H = rmslgss.IDIMPRISMARMSLGS_H)
             inner join dbo.IMPRISMARMS_H rms on (rms.IDIMPRISMARMS_H = rmslgs.IDIMPRISMARMS_H)
             inner join dbo.IMPRISMALGS_H lgs on (lgs.IDIMPRISMALGS_H = rmslgs.IDIMPRISMALGS_H)
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMAS_H s on (s.IDIMPRISMAS_H = rmslgss.IDIMPRISMAS_H)
             inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
             inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H) and (m.IDIMPRISMA_H = rms.IDIMPRISMA_H) and (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAASSETPRICESCENARIO
        #region Query QUERYPRISMAASSETCOMPRESSIONERROR
        /// <summary>
        /// Sélectionne les paramètres des compression erreurs des assets.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// <item>Identifiant de la session</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYPRISMAASSETCOMPRESSIONERROR = @"
            select s.IDASSET,
                   ces.IDIMPRISMARMSLGSS_H,
                   ces.IDC,
                   ces.CE1,
                   ces.CE2,
                   ces.CE3,
                   ces.CE4,
                   ces.CE5
              from dbo.IMPRISMACES_H ces
             inner join dbo.IMPRISMARMSLGSS_H rmslgss on (rmslgss.IDIMPRISMARMSLGSS_H = ces.IDIMPRISMARMSLGSS_H)
             inner join dbo.IMPRISMARMSLGS_H rmslgs on (rmslgs.IDIMPRISMARMSLGS_H = rmslgss.IDIMPRISMARMSLGS_H)
             inner join dbo.IMPRISMARMS_H rms on (rms.IDIMPRISMARMS_H = rmslgs.IDIMPRISMARMS_H)
             inner join dbo.IMPRISMALGS_H lgs on (lgs.IDIMPRISMALGS_H = rmslgs.IDIMPRISMALGS_H)
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMAS_H s on (s.IDIMPRISMAS_H = rmslgss.IDIMPRISMAS_H)
             inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
             inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H) and (m.IDIMPRISMA_H = rms.IDIMPRISMA_H) and (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAASSETCOMPRESSIONERROR
        #region Query QUERYPRISMAASSETVAR
        /// <summary>
        /// Sélectionne les paramètres des valuatuation arrays des assets.
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <listheader>Paramètres obligatoires</listheader>
        /// <item>Date de bourse</item>
        /// <item>Identifiant de la session</item>
        /// </list></remarks>
        // EG 20180803 PERF Upd
        const string QUERYPRISMAASSETVAR = @"
            select s.IDASSET,
                   vars.IDIMPRISMARMSLGSS_H,
                   vars.IDC,
                   vars.SHORTLONG,
                   vars.VARTYPE,
                   vars.VARAMOUNT
              from dbo.IMPRISMAVARS_H vars
             inner join dbo.IMPRISMARMSLGSS_H rmslgss on (rmslgss.IDIMPRISMARMSLGSS_H = vars.IDIMPRISMARMSLGSS_H)
             inner join dbo.IMPRISMARMSLGS_H rmslgs on (rmslgs.IDIMPRISMARMSLGS_H = rmslgss.IDIMPRISMARMSLGS_H)
             inner join dbo.IMPRISMARMS_H rms on (rms.IDIMPRISMARMS_H = rmslgs.IDIMPRISMARMS_H)
             inner join dbo.IMPRISMALGS_H lgs on (lgs.IDIMPRISMALGS_H = rmslgs.IDIMPRISMALGS_H)
             inner join dbo.IMPRISMALG_H lg  on (lg.IDIMPRISMALG_H = lgs.IDIMPRISMALG_H)
             inner join dbo.IMPRISMAS_H s on (s.IDIMPRISMAS_H = rmslgss.IDIMPRISMAS_H)
             inner join dbo.IMPRISMAE_H e on (e.IDIMPRISMAE_H = s.IDIMPRISMAE_H)
             inner join dbo.IMPRISMAP_H p on (p.IDIMPRISMAP_H = e.IDIMPRISMAP_H)
             inner join dbo.IMPRISMA_H m on (m.IDIMPRISMA_H = lg.IDIMPRISMA_H) and (m.IDIMPRISMA_H = rms.IDIMPRISMA_H) and (m.IDIMPRISMA_H = p.IDIMPRISMA_H)
             inner join dbo.IMASSET_ETD_MODEL ima on (ima.IDASSET = s.IDASSET)
             where (m.DTBUSINESS = @DTBUSINESS)";
        #endregion Query QUERYPRISMAASSETVAR
        #endregion PRISMA

        #region IMSM Queries
        // PM 20170313 [22833] Ajout queries pour IMSM
        #region Query QUERYIMSMHOLIDAYADJUSTMENT
        /// <summary>
        /// Lecture des paramètres Holiday Adjustment de la méthode de calcul IMSM
        /// </summary>
        const string QUERYIMSMHOLIDAYADJUSTMENT = @"
            select hadj.CALCULATIONDATE,
                   hadj.EFFECTIVEDATE,
                   hadj.LAMBDAFACTOR
              from dbo.IMSMHOLIDAYADJ_H hadj
             where (EFFECTIVEDATE <= @DTBUSINESS) and (EFFECTIVEDATE >= @DTBUSINESSFIRST)";
        #endregion Query QUERYIMSMHOLIDAYADJUSTMENT
        #region Query QUERYIMSMSECURITYADDON
        /// <summary>
        /// Lecture des paramètres Security Addon de la méthode de calcul IMSM
        /// </summary>
        const string QUERYIMSMSECURITYADDON = @"
            select sao.DATAPOINTNUMBER,
                   sao.SECURITYADDON
              from dbo.IMSMADDON_H sao
             inner join dbo.IMSMSET_H ims on (ims.IDIMSMSET_H = sao.IDIMSMSET_H)
             where (ims.DTENABLED <= @DTBUSINESS) and ((ims.DTDISABLED is null) or (ims.DTDISABLED >@DTBUSINESS))";
        #endregion Query QUERYIMSMSECURITYADDON
        #region Query QUERYIMSMECCSPOTAGREEMENT
        /// <summary>
        /// Lecture des agréments concernant ECC Spot
        /// </summary>
        /// PM 20170602 [23212] Ajout
        const string QUERYIMSMECCSPOTAGREEMENT = @"
            select ma.IDA_1 as IDA, ma.DTSIGNATURE
              from dbo.MASTERAGREEMENT ma
             where (ma.AGREEMENTTYPE = 'ECCPowerAndNaturalGas')
               and (ma.IDA_2 = @IDA_CSS)
               and (ma.DTENABLED <= @DTBUSINESS) and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))
            union
            select ma.IDA_2 as IDA, ma.DTSIGNATURE
              from dbo.MASTERAGREEMENT ma
             where (ma.AGREEMENTTYPE = 'ECCPowerAndNaturalGas')
               and (ma.IDA_1 = @IDA_CSS)
               and (ma.DTENABLED <= @DTBUSINESS) and ((ma.DTDISABLED is null) or (ma.DTDISABLED > @DTBUSINESS))";
        #endregion Query QUERYIMSMECCSPOTAGREEMENT
        #region Query QUERYIMSMCESMPARAMETER
        /// <summary>
        /// Lecture des Margin Parameter pour le calcul du CESM (Current Exposure Spot Market)
        /// </summary>
        // PM 20170808 [23371] Ajout
        //const string QUERYIMSMCESMPARAMETER = @"
        //    select cc.IDENTIFIER, cc.IDCC, cesm.MP_BUY, cesm.MP_SELL from dbo.IMSMCESM_H cesm
        //     inner join dbo.IMSMSET_H ims on (ims.IDIMSMSET_H = cesm.IDIMSMSET_H)
        //     inner join dbo.COMMODITYCONTRACT cc on (cc.IDCC = cesm.IDCC)
        //     where (ims.DTENABLED <= @DTBUSINESS) and ((ims.DTDISABLED is null) or (ims.DTDISABLED > @DTBUSINESS))";
        // PM 20231129 [WI759] Refactoring
        const string QUERYIMSMCESMPARAMETER = @"
            select cc.IDENTIFIER, cc.IDCC, cesm.MP_BUY, cesm.MP_SELL
              from dbo.IMSMCESM cesm
             inner join dbo.COMMODITYCONTRACT cc on (cc.IDCC = cesm.IDCC)
             where (cesm.DTENABLED <= @DTBUSINESS) and ((cesm.DTDISABLED is null) or (cesm.DTDISABLED > @DTBUSINESS))";
        #endregion Query QUERYIMSMCESMPARAMETER
        #region Query QUERYCOMMODITYCONTRACTPARAMETER
        /// <summary>
        /// Lecture des informations sur les commodity contracts
        /// </summary>
        /// PM 20170808 [23371] Ajout
        const string QUERYCOMMODITYCONTRACTPARAMETER = @"
            select a.IDCC, a.CONTRACTIDENTIFIER, a.CONTRACTSYMBOL, a.IDC
              from dbo.VW_ASSET_COMMODITY_EXPANDED a
              inner join dbo.MARKET m on (m.IDM = a.IDM) and (m.IDA = @IDA_CSS)
             where (a.DTENABLED <= @DTBUSINESS) and ((a.DTDISABLED is null) or (a.DTDISABLED >@DTBUSINESS))";
        #endregion Query QUERYCOMMODITYCONTRACTPARAMETER
        #endregion IMSM Queries
        #endregion methods parameters

        #region min maturity

        const string QUERYGETMINMATURITY = @"
            select dc.IDDC as CONTRACTID, MIN(m_target.MATURITYDATE) as MATURITY
            from dbo.ASSET_ETD a_target
            inner join dbo.DERIVATIVEATTRIB da_target on da_target.IDDERIVATIVEATTRIB = a_target.IDDERIVATIVEATTRIB
            inner join dbo.MATURITY m_target on m_target.IDMATURITY = da_target.IDMATURITY
            inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da_target.IDDC
            where dc.IDDC = @IDDC and m_target.MATURITYDATE >= @DTBUSINESS
            group by dc.IDDC";

        const string QUERYGETASSETMINMATURITY = @"
            select a_target.IDASSET as ASSETID
            from dbo.ASSET_ETD a_target
            inner join dbo.DERIVATIVEATTRIB da_target on da_target.IDDERIVATIVEATTRIB = a_target.IDDERIVATIVEATTRIB
            inner join dbo.MATURITY m_target on m_target.IDMATURITY = da_target.IDMATURITY
            inner join dbo.DERIVATIVECONTRACT dc on dc.IDDC = da_target.IDDC
            where dc.IDDC = @IDDC and m_target.MATURITYDATE = @MINMATURITYDATE";

        #endregion min maturity

        #region imr

        const string QUERYIMREQUESTELEMENTS = @"
            select 
            IDA_MRO, IDB_MRO, ISGROSSMARGINING, DTINS, IDAINS,
            IDA_ENTITY, IDA_CSS, DTBUSINESS, IMTIMING,
            IDA_MRO_IMREQUEST, IDT, STATUS, DTSTART, DTEND,
            DTUPD, IDAUPD,
            EXTLLINK, ROWATTRIBUT, IDPR, IDIMREQUEST
            from dbo.IMREQUEST
            where 
            IDA_ENTITY = @IDENTITY
            and IDA_CSS = @IDA_CSSCUSTODIAN
            and DTBUSINESS = @DTBUSINESS
            and IMTIMING = @TIMING";

        #endregion imr

        #region imasset_etd imactor

        // 20120712 MF - Ticket 18004, added the IDDC reference 
        // EG 20180803 PERF Upd
        const string QUERYINSERTIMASSETETD = "insert into dbo.IMASSET_ETD_MODEL (IDASSET, IDDC) values (@IDASSET, @IDDC)";

        // EG 20180803 PERF Upd
        const string QUERYTRUNCATEIMASSETETD = "truncate table dbo.IMASSET_ETD_MODEL";

        // 20120712 MF - Ticket 18004, working table where we insert all the actors/books in position
        // EG 20180803 PERF Upd
        const string QUERYINSERTIMACTOR = "insert into dbo.IMACTOR_MODEL (IDA, IDB) values (@IDA, @IDB)";

        // EG 20180803 PERF Upd
        const string QUERYTRUNCATEIMACTOR = "truncate table dbo.IMACTOR_MODEL";

        #endregion
        #endregion QUERIES

        static readonly Dictionary<DataContractResultSets, DataContractParameter> m_ParameterSets = new Dictionary<DataContractResultSets, DataContractParameter>();

        public static Dictionary<DataContractResultSets, DataContractParameter> ParameterSets
        {
            get { return m_ParameterSets; }
        }

        #region Methods
        // EG [33415/33420] Intégration de nouveaux sous-jacents EQUITY suite à CA
        public static void InitCA(string pCS)
        {
            m_ParameterSets.Clear();
            InitCAParameters(pCS, DataContractResultSets.CA_DERIVATIVECONTRACT, QUERYCA_DERIVATIVECONTRACT);
            InitCAParameters(pCS, DataContractResultSets.CA_CORPOEVENTCONTRACT, QUERYCA_CORPOEVENTCONTRACT);
            InitCAParameters(pCS, DataContractResultSets.CA_DERIVATIVEATTRIB, QUERYCA_DERIVATIVEATTRIB);
            InitCAParameters(pCS, DataContractResultSets.CA_CORPOEVENTDATTRIB, QUERYCA_CORPOEVENTDATTRIB);
            InitCAParameters(pCS, DataContractResultSets.CA_ASSET, QUERYCA_ASSET);
            InitCAParameters(pCS, DataContractResultSets.CA_CORPOEVENTASSET, QUERYCA_CORPOEVENTASSET);
            // EG [33415/33420]
            InitCAUNLParameters(pCS, DataContractResultSets.CA_ASSET_EQUITY, QUERYCA_ASSET_EQUITY);
            InitCAUNLParameters(pCS, DataContractResultSets.CA_ASSET_EQUITY_RDCMK, QUERYCA_ASSET_EQUITY_RDCMK);
        }

        private static void InitCAParameters(string pCS, DataContractResultSets pResultSets, string pQuery)
        {
            // update margin req office status
            DataContractParameter parameterCA = new DataContractParameter
            {
                Query = pQuery,
                Type = CommandType.Text
            };
            // EG 20160105 [34091] Add Switch test 
            switch (pResultSets)
            {
                case DataContractResultSets.CA_DERIVATIVECONTRACT:
                case DataContractResultSets.CA_DERIVATIVEATTRIB:
                case DataContractResultSets.CA_ASSET:
                    parameterCA.Parameters = new DataParameter[]
                    {
                        new DataParameter(pCS, "IDCE", DbType.Int32),
                        new DataParameter(pCS, "IDA_ENTITY", DbType.Int32),
                        new DataParameter(pCS, "IDDC", DbType.Int32),
                        new DataParameter(pCS, "EFFECTIVEDATE", DbType.Date),
                    };
                    break;
                case DataContractResultSets.CA_CORPOEVENTCONTRACT:
                case DataContractResultSets.CA_CORPOEVENTDATTRIB:
                case DataContractResultSets.CA_CORPOEVENTASSET:
                    parameterCA.Parameters = new DataParameter[]
                    {
                        new DataParameter(pCS, "IDCE", DbType.Int32),
                        new DataParameter(pCS, "IDA_ENTITY", DbType.Int32),
                    };
                    break;
            }
            // RD 20150813 [21241] Ajouter le paramètre EFFECTIVEDATE
            //parameterCA.Parameters = new DataParameter[]
            //    {
            //        new DataParameter(pCS, "IDCE", DbType.Int32),
            //        new DataParameter(pCS, "IDA_ENTITY", DbType.Int32),
            //        new DataParameter(pCS, "IDDC", DbType.Int32),
            //        new DataParameter(pCS, "EFFECTIVEDATE", DbType.Date),
            //    };
            m_ParameterSets.Add(pResultSets, parameterCA);
        }

        // EG [33415/33420] Intégration de nouveaux sous-jacents EQUITY suite à CA
        private static void InitCAUNLParameters(string pCS, DataContractResultSets pResultSets, string pQuery)
        {
            // update margin req office status

            DataContractParameter parameterCA;

            parameterCA.Query = pQuery;
            parameterCA.Type = CommandType.Text;
            parameterCA.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDM", DbType.Int32),
                    new DataParameter(pCS, "IDASSET", DbType.Int32),
                };
            m_ParameterSets.Add(pResultSets, parameterCA);
        }

        /// <summary>
        /// Init the Helper
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20180803 PERF Upd
        public static void Init(string pCS, string pBuildTableId)
        {
            m_ParameterSets.Clear();

            InitReferentials(pCS, pBuildTableId);

            InitResults(pCS);

            InitPositions(pCS, pBuildTableId);

            // PM 20170313 [22833] Ajout
            InitTradeValue(pCS);

            // PM 20221212 [XXXXX] Ajout
            InitNoMarginTrade(pCS);

            InitCalculationSheet(pCS);

            InitMethodsParameters(pCS, pBuildTableId);

            InitMinMaturity(pCS);

            InitImr(pCS);

            InitImAsset_EtdImActor(pCS, pBuildTableId);
        }

        private static void InitImr(string pCS)
        {
            // update margin req office status

            DataContractParameter parameterUpdateIMRequestElements;

            parameterUpdateIMRequestElements.Query = QUERYIMREQUESTELEMENTS;
            parameterUpdateIMRequestElements.Type = CommandType.Text;
            parameterUpdateIMRequestElements.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "IDA_CSSCUSTODIAN", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "TIMING", DbType.StringFixedLength),

                };

            m_ParameterSets.Add(DataContractResultSets.IMREQUESTELEMENTS, parameterUpdateIMRequestElements);
        }

        // EG 20180803 PERF Upd
        private static void InitImAsset_EtdImActor(string pCS, string pBuildTableId)
        {
            string _tableName = StrFunc.AppendFormat("IMASSET_ETD_{0}_W", pBuildTableId).ToUpper();
            if (false == DataHelper.IsExistTable(pCS, _tableName))
            {
                DataHelper.CreateTableAsSelect(pCS, "IMASSET_ETD_MODEL", _tableName);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, String.Format("create unique index UX_{0} on dbo.{0} (IDASSET)", _tableName));
            }

            // init request template to insert an element in the imasset_etd table

            DataContractParameter parameterInsertImAssetEtd;

            parameterInsertImAssetEtd.Query = QUERYINSERTIMASSETETD.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterInsertImAssetEtd.Type = CommandType.Text;
            parameterInsertImAssetEtd.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDASSET", DbType.Int32),
                    new DataParameter(pCS, "IDDC", DbType.Int32),
                };

            m_ParameterSets.Add(DataContractResultSets.INSERTIMASSETETD, parameterInsertImAssetEtd);

            // init request template to delete all  the elements related to this application isntance from the table IMASSET_ETD

            DataContractParameter parameterTruncateImAssetEtd;

            parameterTruncateImAssetEtd.Query = QUERYTRUNCATEIMASSETETD.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterTruncateImAssetEtd.Type = CommandType.Text;
            parameterTruncateImAssetEtd.Parameters = new DataParameter[] { };

            m_ParameterSets.Add(DataContractResultSets.TRUNCATEIMASSETETD, parameterTruncateImAssetEtd);


            _tableName = StrFunc.AppendFormat("IMACTOR_{0}_W", pBuildTableId).ToUpper();
            if (false == DataHelper.IsExistTable(pCS, _tableName))
            {
                DataHelper.CreateTableAsSelect(pCS, "IMACTOR_MODEL", _tableName);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, String.Format("create unique index UX_{0} on dbo.{0} (IDA)", _tableName));
            }


            // init request template to insert an element in the imactor table

            DataContractParameter parameterInsertImActor;

            parameterInsertImActor.Query = QUERYINSERTIMACTOR.Replace("IMACTOR_MODEL", _tableName);
            parameterInsertImActor.Type = CommandType.Text;
            parameterInsertImActor.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32),
                    new DataParameter(pCS, "IDB", DbType.Int32),
                };

            m_ParameterSets.Add(DataContractResultSets.INSERTIMACTOR, parameterInsertImActor);

            // init request template to delete all  the elements related to this application isntance from the table IMACTOR

            DataContractParameter parameterTruncateImActor;

            parameterTruncateImActor.Query = QUERYTRUNCATEIMACTOR.Replace("IMACTOR_MODEL", _tableName);
            parameterTruncateImActor.Type = CommandType.Text;
            parameterTruncateImActor.Parameters = new DataParameter[] { };

            m_ParameterSets.Add(DataContractResultSets.TRUNCATEIMACTOR, parameterTruncateImActor);
        }

        private static void InitMinMaturity(string pCS)
        {
            // get the min maturity date of a derivative contract

            DataContractParameter parameterMinMaturityDate;

            parameterMinMaturityDate.Query = QUERYGETMINMATURITY;
            parameterMinMaturityDate.Type = CommandType.Text;
            parameterMinMaturityDate.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDDC", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date)
                };

            m_ParameterSets.Add(DataContractResultSets.GETMINMATURITY, parameterMinMaturityDate);

            // get an asset id collection at a specific maturity date, given a dc

            DataContractParameter parameterAssetMinMaturity;

            parameterAssetMinMaturity.Query = QUERYGETASSETMINMATURITY;
            parameterAssetMinMaturity.Type = CommandType.Text;
            parameterAssetMinMaturity.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDDC", DbType.Int32),
                    new DataParameter(pCS, "MINMATURITYDATE", DbType.Date)
                };

            m_ParameterSets.Add(DataContractResultSets.GETASSETMINMATURITY, parameterAssetMinMaturity);
        }

        // EG 20180803 PERF Upd
        private static void InitMethodsParameters(string pCS, string pBuildTableId)
        {
            string _tableName = StrFunc.AppendFormat("IMASSET_ETD_{0}_W", pBuildTableId).ToUpper();

            #region Initialisation de la requête et des paramètres de chargement des méthodes de calcul
            // Initialisation de la requête et des paramètres de chargement des méthodes de calcul
            // PM 20160404 [22116] Ajout requête pour IMMETHOD
            DataContractParameter parameterInitialMarginMethods;
            parameterInitialMarginMethods.Query = QUERYIMMETHODPARAMETER.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterInitialMarginMethods.Type = CommandType.Text;
            // PM 20170313 [22833] Ajout paramètre IDA_CSS
            parameterInitialMarginMethods.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "IDA_CSS", DbType.Int32),
                };

            m_ParameterSets.Add(DataContractResultSets.IMMETHODPARAMETER, parameterInitialMarginMethods);
            #endregion Initialisation de la requête et des paramètres de chargement des méthodes de calcul

            #region Initialisation des requêtes et des paramètres de chargement des caractéristiques des assets ETD
            // get the asset expanded parameters for all the methods
            DataContractParameter parameterAssetETD;
            parameterAssetETD.Query = DataContractHelper.QUERYASSETEXPANDED.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterAssetETD.Type = CommandType.Text;
            parameterAssetETD.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETEXPANDED_ALLMETHOD, parameterAssetETD);
            #endregion Initialisation des requêtes et des paramètres de chargement des caractéristiques des assets ETD

            #region Initialisation des requêtes et des paramètres de chargement des caractéristiques des futures sous-jacents d'assets ETD 
            // PM 20230929 [XXXXX] Ajout
            DataContractParameter parameterUnderlyingAssetFuture;
            parameterUnderlyingAssetFuture.Query = DataContractHelper.QUERYUNDERLYINGASSETFUTUREEXPANDED.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterUnderlyingAssetFuture.Type = CommandType.Text;
            parameterUnderlyingAssetFuture.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.UNDERLYNGASSETFUTUREEXPANDED_ALLMETHOD, parameterUnderlyingAssetFuture);
            #endregion

            #region Initialisation des requêtes et des paramètres de chargement des caractéristiques des assets ss jacents des assets ETD 
            // get the asset expanded parameters for all the methods
            DataContractParameter parameterUnderlyingAssetETD;
            parameterUnderlyingAssetETD.Query = DataContractHelper.QUERYUNDERLYINGASSETEXPANDED.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterUnderlyingAssetETD.Type = CommandType.Text;
            parameterUnderlyingAssetETD.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.UNDERLYNGASSETEXPANDED_ALLMETHOD, parameterUnderlyingAssetETD);
            #endregion

            #region Initialisation de la requête et des paramètres de chargement des paramètres de calcul du déposit de livraison
            // Init the SQL template, loading parameters to compute the delivery deposit
            DataContractParameter parameterInitialMarginDelivery;
            parameterInitialMarginDelivery.Query = QUERYASSETDELIVERY.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterInitialMarginDelivery.Type = CommandType.Text;
            parameterInitialMarginDelivery.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.ASSETDELIVERY, parameterInitialMarginDelivery);
            #endregion Initialisation de la requête et des paramètres de chargement des paramètres de calcul du déposit de livraison

            #region Initialisation des requêtes et des paramètres de chargement des paramètres de réduction de position ETD
            // Init the SQL template, to get the coverage quantities on equity stocks 
            DataContractParameter parameterEqStocksCoverage;
            parameterEqStocksCoverage.Query = QUERYEQUITYSTOCKSCOVERAGE.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterEqStocksCoverage.Type = CommandType.Text;
            // PM 20130326 Ajout parameter SESSIONID pour limiter la requête sur les DC en position
            parameterEqStocksCoverage.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };
            m_ParameterSets.Add(DataContractResultSets.EQUITYSTOCKSCOVERAGE, parameterEqStocksCoverage);

            // PM 20201028 [25570][25542] Ajout gestion panier
            // Initialisation de la requête de chargement du paramètrage des paniers d'equity pouvant avoir été déposé en reduction de position ETD
            DataContractParameter parameterEqBasketSetting;
            parameterEqBasketSetting.Query = QUERYEQUITYBASKETSETTING.Replace("IMASSET_ETD_MODEL", _tableName);
            parameterEqBasketSetting.Type = CommandType.Text;
            // PM 20130326 Ajout parameter SESSIONID pour limiter la requête sur les DC en position
            parameterEqBasketSetting.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };
            m_ParameterSets.Add(DataContractResultSets.EQUITYBASKETSETTING, parameterEqBasketSetting);
            #endregion Initialisation des requêtes et des paramètres de chargement des paramètres de réduction de position ETD

            #region Initialisation des requêtes de chaque méthode de calcul
            InitCustomMethodParameters(_tableName);

            InitTimsIdemMethodParameters(pCS, _tableName);

            InitTimsEurexMethod(pCS, _tableName);

            InitSpanMethodParameters(pCS, _tableName);

            InitCboeMarginMethodParameters(pCS, _tableName);

            InitMeffMarginMethodParameters(pCS, _tableName);

            // FI 20130517 [18382]
            InitOMXMarginMethodParameters(pCS, _tableName);

            InitPrismaMarginMethodParameters(pCS, _tableName);

            /// PM 20170313 [22833] Ajout méthode InitIMSMMarginMethodParameters
            InitIMSMMarginMethodParameters(pCS);
            #endregion Initialisation des requêtes de chaque méthode de calcul
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des données utilisées par les méthodes SPAN
        /// </summary>
        /// <param name="pCS">Connection string</param>
        // EG 20180803 PERF Upd
        // PM 20190801 [24717] Ajout MARKETVOLUME_ECCSPANMETHOD & STRESSCOMBCOMCONTRACT_ECCSPANMETHOD
        private static void InitSpanMethodParameters(string pCS, string pTableName)
        {
            // get the Exchange Complex parameters for the SPAN method
            DataContractParameter parameterExchangeComplexSpanMethod;
            parameterExchangeComplexSpanMethod.Query = DataContractHelper.QUERYSPANEXCHANGECOMPLEX.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterExchangeComplexSpanMethod.Type = CommandType.Text;
            parameterExchangeComplexSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.EXCHANGECOMPLEX_SPANMETHOD, parameterExchangeComplexSpanMethod);
            //
            // get the Exchange parameters for the SPAN method
            DataContractParameter parameterExchangeSpanMethod;
            parameterExchangeSpanMethod.Query = DataContractHelper.QUERYSPANEXCHANGE.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterExchangeSpanMethod.Type = CommandType.Text;
            parameterExchangeSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.EXCHANGE_SPANMETHOD, parameterExchangeSpanMethod);
            //
            // get the Inter Spread Leg parameters for the SPAN method
            DataContractParameter parameterInterLegSpanMethod;
            parameterInterLegSpanMethod.Query = DataContractHelper.QUERYSPANINTERLEG;
            parameterInterLegSpanMethod.Type = CommandType.Text;
            parameterInterLegSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.INTERLEG_SPANMETHOD, parameterInterLegSpanMethod);
            //
            // get the Currency parameters for the SPAN method
            DataContractParameter parameterCurrencySpanMethod;
            parameterCurrencySpanMethod.Query = DataContractHelper.QUERYSPANCURRENCY;
            parameterCurrencySpanMethod.Type = CommandType.Text;
            parameterCurrencySpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.CURRENCY_SPANMETHOD, parameterCurrencySpanMethod);
            //
            // get the Currency Conversion parameters for the SPAN method
            DataContractParameter parameterCurConvSpanMethod;
            parameterCurConvSpanMethod.Query = DataContractHelper.QUERYSPANCURCONV;
            parameterCurConvSpanMethod.Type = CommandType.Text;
            parameterCurConvSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.CURCONV_SPANMETHOD, parameterCurConvSpanMethod);
            //
            // get the Inter Spread parameters for the SPAN method
            DataContractParameter parameterInterSpreadSpanMethod;
            parameterInterSpreadSpanMethod.Query = DataContractHelper.QUERYSPANINTERSPREAD;
            parameterInterSpreadSpanMethod.Type = CommandType.Text;
            parameterInterSpreadSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.INTERSPREAD_SPANMETHOD, parameterInterSpreadSpanMethod);
            //
            // get the Combined Group parameters for the SPAN method
            DataContractParameter parameterCombinedGroupSpanMethod;
            parameterCombinedGroupSpanMethod.Query = DataContractHelper.QUERYSPANCOMBINEDGROUP.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterCombinedGroupSpanMethod.Type = CommandType.Text;
            parameterCombinedGroupSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.COMBINEDGROUP_SPANMETHOD, parameterCombinedGroupSpanMethod);
            //
            // get the Contract Group parameters for the SPAN method
            DataContractParameter parameterContractGroupSpanMethod;
            parameterContractGroupSpanMethod.Query = DataContractHelper.QUERYSPANCONTRACTGROUP.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterContractGroupSpanMethod.Type = CommandType.Text;
            parameterContractGroupSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.CONTRACTGROUP_SPANMETHOD, parameterContractGroupSpanMethod);
            //
            // get the Contract parameters for the SPAN method
            DataContractParameter parameterContractSpanMethod;
            parameterContractSpanMethod.Query = DataContractHelper.QUERYSPANCONTRACT.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterContractSpanMethod.Type = CommandType.Text;
            parameterContractSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.CONTRACT_SPANMETHOD, parameterContractSpanMethod);
            //
            // get the Delivery Month parameters for the SPAN method
            DataContractParameter parameterDeliveryMonthSpanMethod;
            parameterDeliveryMonthSpanMethod.Query = DataContractHelper.QUERYSPANDELIVERYMONTH.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterDeliveryMonthSpanMethod.Type = CommandType.Text;
            parameterDeliveryMonthSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.DELIVERYMONTH_SPANMETHOD, parameterDeliveryMonthSpanMethod);
            //
            // get the Maturity Tier parameters for the SPAN method
            DataContractParameter parameterMaturityTierSpanMethod;
            parameterMaturityTierSpanMethod.Query = DataContractHelper.QUERYSPANMATURITYTIER;
            parameterMaturityTierSpanMethod.Type = CommandType.Text;
            parameterMaturityTierSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.MATURITYTIER_SPANMETHOD, parameterMaturityTierSpanMethod);
            //
            // get the Intra Contract Group Spread parameters for the SPAN method
            DataContractParameter parameterIntraSpreadSpanMethod;
            parameterIntraSpreadSpanMethod.Query = DataContractHelper.QUERYSPANINTRASPREAD.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterIntraSpreadSpanMethod.Type = CommandType.Text;
            parameterIntraSpreadSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.INTRASPREAD_SPANMETHOD, parameterIntraSpreadSpanMethod);
            //
            // get the Intra Contract Group Spread Leg parameters for the SPAN method
            DataContractParameter parameterIntraLegSpanMethod;
            parameterIntraLegSpanMethod.Query = DataContractHelper.QUERYSPANINTRALEG.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterIntraLegSpanMethod.Type = CommandType.Text;
            parameterIntraLegSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.INTRALEG_SPANMETHOD, parameterIntraLegSpanMethod);
            //
            // get the Maturity parameters for the SPAN method
            DataContractParameter parameterMaturitySpanMethod;
            parameterMaturitySpanMethod.Query = DataContractHelper.QUERYSPANMATURITY;
            parameterMaturitySpanMethod.Type = CommandType.Text;
            parameterMaturitySpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.MATURITY_SPANMETHOD, parameterMaturitySpanMethod);
            //
            // get the asset parameters for the SPAN method
            DataContractParameter parameterRiskArraySpanMethod;
            parameterRiskArraySpanMethod.Query = DataContractHelper.QUERYSPANRISKARRAY.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterRiskArraySpanMethod.Type = CommandType.Text;
            parameterRiskArraySpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "TIMING", DbType.StringFixedLength),
            };
            m_ParameterSets.Add(DataContractResultSets.RISKARRAY_SPANMETHOD, parameterRiskArraySpanMethod);
            //
            // Lecture des paramètres de volume de marché pour la méthode SPAN ECC
            DataContractParameter parameterMarketVolumeECCSpanMethod;
            parameterMarketVolumeECCSpanMethod.Query = DataContractHelper.QUERYECCSPANMARKETVOLUME;
            parameterMarketVolumeECCSpanMethod.Type = CommandType.Text;
            parameterMarketVolumeECCSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.MARKETVOLUME_ECCSPANMETHOD, parameterMarketVolumeECCSpanMethod);
            //
            // Lecture des paramètres de groupe de contrat pour la méthode SPAN ECC
            DataContractParameter parameterComBComStressECCSpanMethod;
            parameterComBComStressECCSpanMethod.Query = DataContractHelper.QUERYECCSPANCOMBCOMSTRESS.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterComBComStressECCSpanMethod.Type = CommandType.Text;
            parameterComBComStressECCSpanMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.STRESSCOMBCOMCONTRACT_ECCSPANMETHOD, parameterComBComStressECCSpanMethod);
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des données utilisées par la méthode CBOE Margin
        /// </summary>
        /// <param name="pCS">Connection string</param>
        // EG 20180803 PERF Upd
        private static void InitCboeMarginMethodParameters(string pCS, string pTableName)
        {
            // Lecture des paramètres de calcul de chaque contrat
            DataContractParameter parameterContractCboeMarginMethod;
            parameterContractCboeMarginMethod.Query = DataContractHelper.QUERYCBOECONTRACTPARAM.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterContractCboeMarginMethod.Type = CommandType.Text;
            parameterContractCboeMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.CONTRACTPARAM_CBOEMARGINMETHOD, parameterContractCboeMarginMethod);
            //
            // Lecture des paramètres de calcul de chaque asset
            DataContractParameter parameterAssetCboeMarginMethod;
            parameterAssetCboeMarginMethod.Query = DataContractHelper.QUERYCBOEASSETEXPANDED.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetCboeMarginMethod.Type = CommandType.Text;
            parameterAssetCboeMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETEXPANDEDPARAM_CBOEMARGINMETHOD, parameterAssetCboeMarginMethod);
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des données utilisées par la méthode MEFFCOM2 Margin
        /// </summary>
        /// <param name="pCS">Connection string</param>
        // EG 20180803 PERF Upd
        private static void InitMeffMarginMethodParameters(string pCS, string pTableName)
        {
            // Lecture des paramètres de chaque asset
            DataContractParameter parameterAssetMeffMarginMethod;
            parameterAssetMeffMarginMethod.Query = DataContractHelper.QUERYMEFFCONTRACTASSET.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetMeffMarginMethod.Type = CommandType.Text;
            parameterAssetMeffMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.CONTRACTASSET_MEFFCOM2METHOD, parameterAssetMeffMarginMethod);
            //
            // Lecture des paramètres des margin class
            DataContractParameter parameterValArrayMeffMarginMethod;
            parameterValArrayMeffMarginMethod.Query = DataContractHelper.QUERYMEFFVALUATIONARRAY;
            parameterValArrayMeffMarginMethod.Type = CommandType.Text;
            parameterValArrayMeffMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.VALUATIONARRAY_MEFFCOM2METHOD, parameterValArrayMeffMarginMethod);
            //
            // Lecture des paramètres de spread inter margin class
            DataContractParameter parameterInterSpreadMeffMarginMethod;
            parameterInterSpreadMeffMarginMethod.Query = DataContractHelper.QUERYMEFFINTERSPREAD;
            parameterInterSpreadMeffMarginMethod.Type = CommandType.Text;
            parameterInterSpreadMeffMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.INTERSPREAD_MEFFCOM2METHOD, parameterInterSpreadMeffMarginMethod);
            //
            // Lecture des paramètres de spread intra margin class (Time Spread)
            DataContractParameter parameterIntraSpreadMeffMarginMethod;
            parameterIntraSpreadMeffMarginMethod.Query = DataContractHelper.QUERYMEFFINTRASPREAD;
            parameterIntraSpreadMeffMarginMethod.Type = CommandType.Text;
            parameterIntraSpreadMeffMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.INTRASPREAD_MEFFCOM2METHOD, parameterIntraSpreadMeffMarginMethod);
            //
            // Lecture des matrices de delta chaque asset
            DataContractParameter parameterDeltaMeffMarginMethod;
            parameterDeltaMeffMarginMethod.Query = DataContractHelper.QUERYMEFFDELTAARRAY.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterDeltaMeffMarginMethod.Type = CommandType.Text;
            parameterDeltaMeffMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.DELTAARRAY_MEFFCOM2METHOD, parameterDeltaMeffMarginMethod);
            //
            // Lecture des matrices de cours théorique de chaque asset
            DataContractParameter parameterTheorPriceMeffMarginMethod;
            parameterTheorPriceMeffMarginMethod.Query = DataContractHelper.QUERYMEFFTHEORPRICEARRAY.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterTheorPriceMeffMarginMethod.Type = CommandType.Text;
            parameterTheorPriceMeffMarginMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.THEORPRICEARRAY_MEFFCOM2METHOD, parameterTheorPriceMeffMarginMethod);
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des données utilisées par la méthode Prisma Margin
        /// </summary>
        /// <param name="pCS">Connection string</param>
        // EG 20180803 PERF Upd
        private static void InitPrismaMarginMethodParameters(string pCS, string pTableName)
        {
            // Lecture des paramètres des liquidation groups
            DataContractParameter parameterLiquidationGroup;
            parameterLiquidationGroup.Query = DataContractHelper.QUERYPRISMALIQUIDATIONGROUP;
            parameterLiquidationGroup.Type = CommandType.Text;
            parameterLiquidationGroup.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.LIQUIDATIONGROUP_PRISMAMETHOD, parameterLiquidationGroup);

            // Lecture des paramètres des liquidation group splits
            DataContractParameter parameterLiquidationGroupSplit;
            parameterLiquidationGroupSplit.Query = DataContractHelper.QUERYPRISMALIQUIDATIONGROUPSPLIT;
            parameterLiquidationGroupSplit.Type = CommandType.Text;
            parameterLiquidationGroupSplit.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.LIQUIDATIONGROUPSPLIT_PRISMAMETHOD, parameterLiquidationGroupSplit);

            // Lecture des paramètres des risk measure set de chaque liquidation group splits
            DataContractParameter parameterRiskMeasureSet;
            parameterRiskMeasureSet.Query = DataContractHelper.QUERYPRISMARISKMEASURESET;
            parameterRiskMeasureSet.Type = CommandType.Text;
            parameterRiskMeasureSet.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.RISKMEASURESET_PRISMAMETHOD, parameterRiskMeasureSet);

            // Lecture des paramètres des market capacities
            DataContractParameter parameterMarketCapacity;
            parameterMarketCapacity.Query = DataContractHelper.QUERYPRISMAMARKETCAPACITY;
            parameterMarketCapacity.Type = CommandType.Text;
            parameterMarketCapacity.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.MARKETCAPACITY_PRISMAMETHOD, parameterMarketCapacity);

            // Lecture des paramètres des liquidity factors
            DataContractParameter parameterLiquidityFactor;
            parameterLiquidityFactor.Query = DataContractHelper.QUERYPRISMALIQUIDITYFACTOR;
            parameterLiquidityFactor.Type = CommandType.Text;
            parameterLiquidityFactor.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.LIQUIDITYFACTOR_PRISMAMETHOD, parameterLiquidityFactor);

            // Lecture des paramètres des taux de change
            DataContractParameter parameterExchangeRate;
            parameterExchangeRate.Query = DataContractHelper.QUERYPRISMAEXCHANGERATE;
            parameterExchangeRate.Type = CommandType.Text;
            parameterExchangeRate.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.EXCHANGERATE_PRISMAMETHOD, parameterExchangeRate);

            // Lecture des paramètres des taux de change des risk measure set
            DataContractParameter parameterExchangeRateRMS;
            parameterExchangeRateRMS.Query = DataContractHelper.QUERYPRISMAEXCHANGERATERMS;
            parameterExchangeRateRMS.Type = CommandType.Text;
            parameterExchangeRateRMS.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.EXCHANGERATERMS_PRISMAMETHOD, parameterExchangeRateRMS);

            // Lecture des paramètres des assets, expirations, products
            DataContractParameter parameterAsset;
            parameterAsset.Query = DataContractHelper.QUERYPRISMAASSET.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAsset.Type = CommandType.Text;
            parameterAsset.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSET_PRISMAMETHOD, parameterAsset);

            // Lecture des paramètres des liquidation group split des assets
            DataContractParameter parameterAssetLGS;
            parameterAssetLGS.Query = DataContractHelper.QUERYPRISMAASSETLIQUIDGROUPSPLIT.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetLGS.Type = CommandType.Text;
            parameterAssetLGS.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETLIQUIDGROUPSPLIT_PRISMAMETHOD, parameterAssetLGS);

            // Lecture des paramètres des risk measure set des liquidation group split des assets
            DataContractParameter parameterAssetRMSLGS;
            parameterAssetRMSLGS.Query = DataContractHelper.QUERYPRISMAASSETRMSLGS.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetRMSLGS.Type = CommandType.Text;
            parameterAssetRMSLGS.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETRMSLGS_PRISMAMETHOD, parameterAssetRMSLGS);

            // Lecture des paramètres des scénari de prix des assets
            DataContractParameter parameterAssetPriceScenario;
            parameterAssetPriceScenario.Query = DataContractHelper.QUERYPRISMAASSETPRICESCENARIO.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetPriceScenario.Type = CommandType.Text;
            parameterAssetPriceScenario.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETPRICESCENARIO_PRISMAMETHOD, parameterAssetPriceScenario);

            // Lecture des paramètres des compression erreurs des assets
            DataContractParameter parameterAssetCompressionError;
            parameterAssetCompressionError.Query = DataContractHelper.QUERYPRISMAASSETCOMPRESSIONERROR.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetCompressionError.Type = CommandType.Text;
            parameterAssetCompressionError.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETCOMPRESSIONERROR_PRISMAMETHOD, parameterAssetCompressionError);

            // Lecture des paramètres des valuation arrays des assets
            DataContractParameter parameterAssetVars;
            parameterAssetVars.Query = DataContractHelper.QUERYPRISMAASSETVAR.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetVars.Type = CommandType.Text;
            parameterAssetVars.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.ASSETVAR_PRISMAMETHOD, parameterAssetVars);
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des données utilisées par la méthode IMSM
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// PM 20170313 [22833] Ajout méthode InitIMSMMarginMethodParameters
        private static void InitIMSMMarginMethodParameters(string pCS)
        {
            //
            // Lecture des paramètres Holiday Adjustement
            DataContractParameter parameterHolidayAdjustementIMSMMethod;
            parameterHolidayAdjustementIMSMMethod.Query = DataContractHelper.QUERYIMSMHOLIDAYADJUSTMENT;
            parameterHolidayAdjustementIMSMMethod.Type = CommandType.Text;
            parameterHolidayAdjustementIMSMMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                new DataParameter(pCS, "DTBUSINESSFIRST", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.HOLIDAYADJUSTMENT_IMSMMETHOD, parameterHolidayAdjustementIMSMMethod);
            //
            // Lecture des paramètres Security Addon
            DataContractParameter parameterSecurityAddonIMSMMethod;
            parameterSecurityAddonIMSMMethod.Query = DataContractHelper.QUERYIMSMSECURITYADDON;
            parameterSecurityAddonIMSMMethod.Type = CommandType.Text;
            parameterSecurityAddonIMSMMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.SECURITYADDON_IMSMMETHOD, parameterSecurityAddonIMSMMethod);
            //
            // PM 20170808 [23371] Ajout lecture CESM Margin Parameter
            // Lecture des Margin Parameter pour le calcul du CESM (Current Exposure Spot Market)
            DataContractParameter parameterCESMIMSMMethod;
            parameterCESMIMSMMethod.Query = DataContractHelper.QUERYIMSMCESMPARAMETER;
            parameterCESMIMSMMethod.Type = CommandType.Text;
            parameterCESMIMSMMethod.Parameters = new DataParameter[]
            {
                new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            };
            m_ParameterSets.Add(DataContractResultSets.CESMPARAMETER_IMSMMETHOD, parameterCESMIMSMMethod);
            //
            // PM 20170602 [23212] Ajout lecture MASTERAGREEMENT
            // Lecture des agréments pour ECC Spot
            DataContractParameter parameterAgreement;
            parameterAgreement.Query = DataContractHelper.QUERYIMSMECCSPOTAGREEMENT;
            parameterAgreement.Type = CommandType.Text;
            parameterAgreement.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "IDA_CSS", DbType.Int32),
                };
            m_ParameterSets.Add(DataContractResultSets.ECCSPOTAGREEMENT_IMSMMETHOD, parameterAgreement);
            //
            // PM 20170808 [23371] Ajout lecture des informations sur les Commodity Contract
            // Lecture des informations sur les Commodity Contract
            DataContractParameter parameterCommodityContract;
            parameterCommodityContract.Query = DataContractHelper.QUERYCOMMODITYCONTRACTPARAMETER;
            parameterCommodityContract.Type = CommandType.Text;
            parameterCommodityContract.Parameters = new DataParameter[]
            {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "IDA_CSS", DbType.Int32),
            };
            m_ParameterSets.Add(DataContractResultSets.COMMODITYCONTRACTPARAMETER_IMSMMETHOD, parameterCommodityContract);
        }

        // EG 20180803 PERF Upd
        private static void InitTimsEurexMethod(string pCS, string pTableName)
        {
            // init the eurex contract parameters SQL request 

            DataContractParameter parameterContractParamsTimsEurexMethod;

            parameterContractParamsTimsEurexMethod.Query = QUERYCONTRACTTIMSEUREX.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterContractParamsTimsEurexMethod.Type = CommandType.Text;
            parameterContractParamsTimsEurexMethod.Parameters = new DataParameter[] { };

            m_ParameterSets.Add(DataContractResultSets.CONTRACT_TIMSEUREXMETHOD, parameterContractParamsTimsEurexMethod);

            // init the eurex maturity parameters SQL request

            DataContractParameter parameterMaturityParamsTimsEurexMethod;

            parameterMaturityParamsTimsEurexMethod.Query = QUERYMATURITYTIMSEUREX.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterMaturityParamsTimsEurexMethod.Type = CommandType.Text;
            parameterMaturityParamsTimsEurexMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.MATURITY_TIMSEUREXMETHOD, parameterMaturityParamsTimsEurexMethod);

            // init the eurex asset parameters SQL request

            DataContractParameter parameterAssetParamsTimsEurexMethod;

            parameterAssetParamsTimsEurexMethod.Query = QUERYASSETTIMSEUREX.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetParamsTimsEurexMethod.Type = CommandType.Text;
            // more query parameters will be added formatting the string
            parameterAssetParamsTimsEurexMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.ASSET_TIMSEUREXMETHOD, parameterAssetParamsTimsEurexMethod);

            // init the eurex volatility parameters SQL request

            DataContractParameter parameterVolatilityParamsTimsEurexMethod;

            parameterVolatilityParamsTimsEurexMethod.Query = QUERYVOLATILITYTIMSEUREX.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterVolatilityParamsTimsEurexMethod.Type = CommandType.Text;
            // more query parameters will be added formatting the string
            parameterVolatilityParamsTimsEurexMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.VOLATILITY_TIMSEUREXMETHOD, parameterVolatilityParamsTimsEurexMethod);

            // init the exchange rate SQL request

            DataContractParameter parameterFxRateTimsEurexMethod;

            parameterFxRateTimsEurexMethod.Query = QUERYFXRATETIMSEUREX;
            parameterFxRateTimsEurexMethod.Type = CommandType.Text;
            parameterFxRateTimsEurexMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "CSSCURRENCY", DbType.String),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };

            m_ParameterSets.Add(DataContractResultSets.FXRATE_TIMSEUREXMETHOD, parameterFxRateTimsEurexMethod);
        }

        // EG 20180803 PERF Upd
        private static void InitTimsIdemMethodParameters(string pCS, string pTableName)
        {
            // get the contract related parameters for the TIMS IDEM RISK method

            DataContractParameter parameterClassParamsTimsIdemMethod;

            parameterClassParamsTimsIdemMethod.Query = QUERYCLASSTIMSIDEMMETHOD.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterClassParamsTimsIdemMethod.Type = CommandType.Text;
            parameterClassParamsTimsIdemMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };

            m_ParameterSets.Add(DataContractResultSets.CLASS_TIMSIDEMMETHOD, parameterClassParamsTimsIdemMethod);

            // get the assets etd related parameters for the TIMS IDEM RISK method

            DataContractParameter parameterRiskArrayParamsTimsIdemMethod;

            parameterRiskArrayParamsTimsIdemMethod.Query = QUERYRISKARRAYTIMSIDEMMETHOD.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterRiskArrayParamsTimsIdemMethod.Type = CommandType.Text;
            parameterRiskArrayParamsTimsIdemMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };

            m_ParameterSets.Add(DataContractResultSets.RISKARRAY_TIMSIDEMMETHOD, parameterRiskArrayParamsTimsIdemMethod);

            // get the current position actions

            DataContractParameter parameterPositionActionsTimsIdemMethod;

            parameterPositionActionsTimsIdemMethod.Query = QUERYPOSITIONACTIONSCCG;
            parameterPositionActionsTimsIdemMethod.Type = CommandType.Text;
            parameterPositionActionsTimsIdemMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "IDACSS", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.POSITIONACTIONSCCG, parameterPositionActionsTimsIdemMethod);

            // PM 20130321 AGREX
            // Lecture des paramètres de calcul de chaque contrat
            DataContractParameter parameterContractsTimsIdemMethod;
            parameterContractsTimsIdemMethod.Query = QUERYTIMSIDEMCONTRACTPARAM.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterContractsTimsIdemMethod.Type = CommandType.Text;
            parameterContractsTimsIdemMethod.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };
            m_ParameterSets.Add(DataContractResultSets.CONTRACTPARAM_TIMSIDEMMETHOD, parameterContractsTimsIdemMethod);
            // PM 20130327 AGREX
            // Lecture des convertion d'assets pour le calcul du déposit de livraison
            DataContractParameter parameterAssetDeliveryTimsIdemMethod;
            parameterAssetDeliveryTimsIdemMethod.Query = QUERYTIMSIDEMASSETDELIVERY.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterAssetDeliveryTimsIdemMethod.Type = CommandType.Text;
            parameterAssetDeliveryTimsIdemMethod.Parameters = new DataParameter[] { };
            m_ParameterSets.Add(DataContractResultSets.ASSETDELIVERY_TIMSIDEMMETHOD, parameterAssetDeliveryTimsIdemMethod);
            // PM 20130327 AGREX
            // Insertion des assets de calcul du déposit de livraison
            DataContractParameter parameterImAssetEtdTimsIdemMethod;
            parameterImAssetEtdTimsIdemMethod.Query = QUERYTIMSIDEMINSERTIMASSETETD.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterImAssetEtdTimsIdemMethod.Type = CommandType.Text;
            parameterImAssetEtdTimsIdemMethod.Parameters = new DataParameter[] { };
            m_ParameterSets.Add(DataContractResultSets.INSERTIMASSETETD_TIMSIDEMMETHOD, parameterImAssetEtdTimsIdemMethod);
        }

        // EG 20180803 PERF Upd
        private static void InitCustomMethodParameters(string pTableName)
        {
            // get the derivative contracts for an assets set

            DataContractParameter parameterDerivativeContractsByAsset;

            parameterDerivativeContractsByAsset.Query = QUERYCONTRACTASSETCUSTOMMETHOD.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterDerivativeContractsByAsset.Type = CommandType.Text;
            // 20120718 MF - Ticket 18004
            parameterDerivativeContractsByAsset.Parameters = new DataParameter[] { };

            m_ParameterSets.Add(DataContractResultSets.CONTRACTASSET_CUSTOMMETHOD, parameterDerivativeContractsByAsset);

            // get the parameters for the standard RISK method, relative to a derivative contracts set

            DataContractParameter parameterParamsCustomMethod;

            parameterParamsCustomMethod.Query = QUERYPARAMSCUSTOMMETHOD.Replace("IMASSET_ETD_MODEL", pTableName);
            parameterParamsCustomMethod.Type = CommandType.Text;
            // 20120718 MF - Ticket 18004
            parameterParamsCustomMethod.Parameters = new DataParameter[] { };

            m_ParameterSets.Add(DataContractResultSets.PARAMS_CUSTOMMETHOD, parameterParamsCustomMethod);
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des données utilisées par les méthodes OMX RCaR Margin
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// FI 20130517 [18382] add method
        // EG 20180803 PERF Upd
        private static void InitOMXMarginMethodParameters(string pCS, string pTableName)
        {
            // Lecture les paramètres de chaque asset
            DataContractParameter dataContractParameter;
            dataContractParameter.Query = DataContractHelper.QUERYOMXVCTFILE.Replace("IMASSET_ETD_MODEL", pTableName);
            dataContractParameter.Type = CommandType.Text;
            dataContractParameter.Parameters = new DataParameter[]
            {
                DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS),
            };
            m_ParameterSets.Add(DataContractResultSets.VCTFILE_OMXMETHOD, dataContractParameter);
        }

        private static void InitCalculationSheet(string pCS)
        {
            // null query, initialize a data adapter

            DataContractParameter parameterProcessMarginTrack;

            parameterProcessMarginTrack.Query = QUERYPROCESSMARGINTRACK;
            parameterProcessMarginTrack.Type = CommandType.Text;
            parameterProcessMarginTrack.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDPROCESS_L", DbType.Int32),
                };

            m_ParameterSets.Add(DataContractResultSets.PROCESSMARGINTRACK, parameterProcessMarginTrack);

            // intialize insert command to save calculation sheets

            DataContractParameter parameterInsertTradeMarginTrack;

            parameterInsertTradeMarginTrack.Query = QUERYORACLE_INSERTTRADEMARGINTRACK;
            parameterInsertTradeMarginTrack.Type = CommandType.Text;
            parameterInsertTradeMarginTrack.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDT", DbType.Int32),
                    new DataParameter(pCS, "IDMARGINTRACK", DbType.Int32),
                    new DataParameter(pCS, "TRADEXML", DbType.Xml),
                    new DataParameter(pCS, "DTINS", DbType.Date),
                    new DataParameter(pCS, "IDAINS", DbType.Int32),
                };

            m_ParameterSets.Add(DataContractResultSets.INSERTTRADEMARGINTRACK, parameterInsertTradeMarginTrack);

            // Initialisation de la commande SELECT pour la DataTable contenant les logs de calcul
            // PM 20160922 Pour VS2013
            DataContractParameter parameterSelectTradeMarginTrack;
            parameterSelectTradeMarginTrack.Query = QUERY_SELECTTRADEMARGINTRACK;
            parameterSelectTradeMarginTrack.Type = CommandType.Text;
            parameterSelectTradeMarginTrack.Parameters = new DataParameter[0] { };
            m_ParameterSets.Add(DataContractResultSets.SELECTTRADEMARGINTRACK, parameterSelectTradeMarginTrack);
        }

        // EG 20141224 [20566] @DTBUSINESS|@DTBUSINESSINVARIANT replace @DTPOS|@DTPOSINVARIANT
        // EG 20180803 PERF Upd
        private static void InitPositions(string pCS, string pBuildTableId)
        {
            // get the allocations for the actor rattached to a given entity

            string _tableName = StrFunc.AppendFormat("IMACTOR_{0}_W", pBuildTableId).ToUpper();
            if (false == DataHelper.IsExistTable(pCS, _tableName))
            {
                DataHelper.CreateTableAsSelect(pCS, "IMACTOR_MODEL", _tableName);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, String.Format("create unique index UX_{0} on dbo.{0} (IDA, IDB)", _tableName));
            }

            //DataContractParameter parameterTradeAllocationsActions;

            //parameterTradeAllocationsActions.Query = QUERYTRADEALLOCATIONSACTIONS.Replace("IMACTOR_MODEL", _tableName);
            //parameterTradeAllocationsActions.Type = CommandType.Text;
            //parameterTradeAllocationsActions.Parameters = new DataParameter[]
            //    {
            //        new DataParameter(pCS, "IDENTITY", DbType.Int32),
            //        new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            //        new DataParameter(pCS, "DTBUSINESSINVARIANT", DbType.Date),
            //        // more query parameters will be added formatting the string
            //    };

            //m_ParameterSets.Add(DataContractResultSets.TRADEALLOCATIONSACTIONS, parameterTradeAllocationsActions);

            DataContractParameter parameterTradeAllocationsActionsWith;

            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
            {
                string _tableName2 = StrFunc.AppendFormat("TRADEPOS_{0}_W", pBuildTableId).ToUpper();
                if (false == DataHelper.IsExistTable(pCS, _tableName2))
                {
                    DataHelper.CreateTableAsSelect(pCS, "TRADEPOS_MODEL", _tableName2);
                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, String.Format("create clustered index UX_{0} on dbo.{0} (IDT)", _tableName2));
                }
                parameterTradeAllocationsActionsWith.Query = QUERYTRADEALLOCATIONSACTIONS_SQL.Replace("IMACTOR_MODEL", _tableName).Replace("TRADEPOS_MODEL", _tableName2);
            }
            else
                parameterTradeAllocationsActionsWith.Query = QUERYTRADEALLOCATIONSACTIONS_ORA.Replace("IMACTOR_MODEL", _tableName);

            parameterTradeAllocationsActionsWith.Type = CommandType.Text;
            parameterTradeAllocationsActionsWith.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "DTBUSINESSINVARIANT", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.TRADEALLOCATIONSACTIONSWITH, parameterTradeAllocationsActionsWith);

            // get the pending allocations for a set of actors rattached to a given entity

            DataContractParameter parameterPhysicalSettlements;

            parameterPhysicalSettlements.Query = QUERYPHYSICALSETTLEMENTS.Replace("IMACTOR_MODEL", _tableName);
            parameterPhysicalSettlements.Type = CommandType.Text;
            parameterPhysicalSettlements.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.PHYSICALSETTLEMENTS, parameterPhysicalSettlements);

            // Lecture des positions en livraison pour les acteurs d'une entité donnée
            // PM 20130905 [17949] Livraison
            DataContractParameter parameterPhysicalDelivery;

            parameterPhysicalDelivery.Query = QUERYPHYSICALDELIVERY.Replace("IMACTOR_MODEL", _tableName);
            parameterPhysicalDelivery.Type = CommandType.Text;
            parameterPhysicalDelivery.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "MAXDELIVERYDAY", DbType.Int32),
                    // more query parameters will be added formatting the string
                };

            m_ParameterSets.Add(DataContractResultSets.PHYSICALDELIVERY, parameterPhysicalDelivery);
        }

        /// <summary>
        /// Initialisation des requêtes et paramètres de chargement des montants des trades
        /// </summary>
        /// <param name="pCS"></param>
        /// PM 20170313 [22833] Ajout
        private static void InitTradeValue(string pCS)
        {
            // Lecture des montants (GAM) des trades
            DataContractParameter parameterTrade;

            parameterTrade.Query = QUERYIMSMTRADEVALUE;
            parameterTrade.Type = CommandType.Text;
            parameterTrade.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "IDM", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "DTBUSINESSFIRST", DbType.Date),
                };

            m_ParameterSets.Add(DataContractResultSets.TRADEVALUE, parameterTrade);

            // PM 20190418 [24628] Gestion des actors avec agreement mais sans aucun trade
            DataContractParameter parameterActorAgreement;

            parameterActorAgreement.Query = QUERYIMSMACTORAGREEMENT;
            parameterActorAgreement.Type = CommandType.Text;
            parameterActorAgreement.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "IDM", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };

            m_ParameterSets.Add(DataContractResultSets.TRADEVALUEAGREEMENT, parameterActorAgreement);

            // PM 20170602 [23212] La recherche du plus vieux trades n'est plus utile
            //// Lecture de la plus vieille date à laquelle existe un trade
            //DataContractParameter parameterMinDate;

            //parameterMinDate.Query = QUERYIMSMMINTRADEDATE;
            //parameterMinDate.Type = CommandType.Text;
            //parameterMinDate.Parameters = new DataParameter[]
            //    {
            //        new DataParameter(pCS, "IDENTITY", DbType.Int32),
            //        new DataParameter(pCS, "IDM", DbType.Int32),
            //        new DataParameter(pCS, "DTBUSINESS", DbType.Date),
            //    };

            //m_ParameterSets.Add(DataContractResultSets.TRADEVALUEMINDATE, parameterMinDate);
        }

        /// <summary>
        /// Initialisation des requêtes de chargement des trades sans calcul de déposit
        /// </summary>
        /// <param name="pCS"></param>
        // PM 20221212 [XXXXX] Ajout
        private static void InitNoMarginTrade(string pCS)
        {
            // Lecture des trades avec méthode de calcul de déposit NONE
            DataContractParameter parameterTrade;

            parameterTrade.Query = QUERYNOMARGINDAYTRADE;
            parameterTrade.Type = CommandType.Text;
            parameterTrade.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDENTITY", DbType.Int32),
                    new DataParameter(pCS, "IDM", DbType.Int32),
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                };

            m_ParameterSets.Add(DataContractResultSets.NOINITIALMARGINTRADE, parameterTrade);
        }

        private static void InitResults(string pCS)
        {
            DataContractParameter parameterRiskAllResults;

            parameterRiskAllResults.Query = QUERYRISKALLRESULTS;
            parameterRiskAllResults.Type = CommandType.Text;
            // RD 20170420 [23094] Add IDA_ENTITY parameter
            parameterRiskAllResults.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "IDA_CSSCUSTODIAN", DbType.Int32),
                    new DataParameter(pCS, "IDSTENVIRONMENT", DbType.AnsiString),
                    new DataParameter(pCS, "IDA_ENTITY", DbType.Int32)
                };

            m_ParameterSets.Add(DataContractResultSets.RISKALLRESULTS, parameterRiskAllResults);
        }
        // EG 20140221 [19575][19666] Add IDA_CSS parameter on QUERYINSERTACTORPOS
        // EG 20180803 PERF Upd
        private static void InitReferentials(string pCS, string pBuildTableId)
        {
            string _tableName = StrFunc.AppendFormat("IMACTORPOS_{0}_W", pBuildTableId).ToUpper();
            if (false == DataHelper.IsExistTable(pCS, _tableName))
            {
                DataHelper.CreateTableAsSelect(pCS, "IMACTORPOS_MODEL", _tableName);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, String.Format("create index IX_{0} on dbo.{0} (IDA)", _tableName));
            }

            /*
            // RD 20130419 [18575] Dans la table IMACTORPOS Charger les acteurs avec des trades en position ou bien échus en attente de livraison
            DataContractParameter parameterInsertActorPos;
            
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
            {
                string _tableName2 = StrFunc.AppendFormat("TRADEALLOC_{0}_W", pBuildTableId).ToUpper();
                if (false == DataHelper.IsExistTable(pCS, _tableName2))
                {
                    DataHelper.CreateTableAsSelect(pCS, "TRADEALLOC_MODEL", _tableName2);
                    DataHelper.ExecuteNonQuery(pCS, CommandType.Text, String.Format("create clustered index UX_{0} on dbo.{0} (IDT)", _tableName2));
                }
                parameterInsertActorPos.Query = QUERYINSERTACTORPOS_SQL.Replace("IMACTORPOS_MODEL", _tableName).Replace("TRADEALLOC_MODEL", _tableName2);
            }
            else 
                parameterInsertActorPos.Query = QUERYINSERTACTORPOS_ORA.Replace("IMACTORPOS_MODEL", _tableName);
            */

            DataContractParameter parameterInsertActorPos;
            for (int i = 1; i < 5; i++)
            {
                string queryIns;
                switch (i)
                {
                    case 1:
                        if (DataHelper.GetDbSvrType(pCS) == DbSvrType.dbSQL)
                            queryIns = QUERYINSERTACTORPOS1_SQL;
                        else if (DataHelper.GetDbSvrType(pCS) == DbSvrType.dbORA)
                            queryIns = QUERYINSERTACTORPOS1_ORA;
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", DataHelper.GetDbSvrType(pCS)));
                        break;
                    case 2:
                        queryIns = QUERYINSERTACTORPOS2;
                        break;
                    case 3:
                        queryIns = QUERYINSERTACTORPOS3;
                        break;
                    case 4:
                        queryIns = QUERYINSERTACTORPOS4;
                        break;
                    default:
                        throw new NotSupportedException($"value:{i} not supported");
                }
                parameterInsertActorPos.Query = GetQueryInsertActionPos( queryIns, _tableName);
                parameterInsertActorPos.Type = CommandType.Text;
                parameterInsertActorPos.Parameters = new DataParameter[]{
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date),
                    new DataParameter(pCS, "IDA_CSSCUSTODIAN", DbType.Int32),
                };
                m_ParameterSets.Add((DataContractResultSets)Enum.Parse(typeof(DataContractResultSets), $"INSERTIMACTORPOS{i}"), parameterInsertActorPos);
            }

            parameterInsertActorPos.Query = GetQueryInsertActionPos( QUERYINSERTACTORPOS5, _tableName);
            parameterInsertActorPos.Type = CommandType.Text;
            parameterInsertActorPos.Parameters = new DataParameter[]{
                    new DataParameter(pCS, "DTBUSINESS", DbType.Date)
            };
            m_ParameterSets.Add(DataContractResultSets.INSERTIMACTORPOS5, parameterInsertActorPos);




            // RD 20130419 [18575] Vider la table IMACTORPOS
            DataContractParameter parameterTruncateActorPos;

            parameterTruncateActorPos.Query = QUERYTRUNCATEACTORPOS.Replace("IMACTORPOS_MODEL", _tableName);
            parameterTruncateActorPos.Type = CommandType.Text;
            parameterTruncateActorPos.Parameters = new DataParameter[] { };
            m_ParameterSets.Add(DataContractResultSets.TRUNCATEIMACTORPOS, parameterTruncateActorPos);

            // init the actor relations (for a specific actor) SQL request

            DataContractParameter parameterActorRelationship;

            parameterActorRelationship.Query = QUERYACTORRELATIONSHIP.Replace("IMACTORPOS_MODEL", _tableName);
            parameterActorRelationship.Type = CommandType.Text;
            parameterActorRelationship.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32),
                };

            m_ParameterSets.Add(DataContractResultSets.ACTORRELATIONSHIP, parameterActorRelationship);

            // init the markets mining (and css, for a specific entity) SQL request

            DataContractParameter parameterEntityMarketWithCSS;

            parameterEntityMarketWithCSS.Query = QUERYENTITYMARKETWITHCSS;
            parameterEntityMarketWithCSS.Type = CommandType.Text;
            parameterEntityMarketWithCSS.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32)
                };

            m_ParameterSets.Add(DataContractResultSets.ENTITYMARKETWITHCSS, parameterEntityMarketWithCSS);

            // Init the main risk parameters (for a specific actor having specific role MARGINREQOFFICE) SQL request

            DataContractParameter parameterMarginReqOfficeParameters;

            parameterMarginReqOfficeParameters.Query = QUERYMARGINREQOFFICEBOOKSANDPARAMETERS;
            parameterMarginReqOfficeParameters.Type = CommandType.Text;
            parameterMarginReqOfficeParameters.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32),
                    new DataParameter(pCS, "ROLE", DbType.AnsiStringFixedLength)
                };

            m_ParameterSets.Add(DataContractResultSets.MARGINREQOFFICEBOOKSANDPARAMETERS, parameterMarginReqOfficeParameters);

            // Init the supplementary risk parameters per clearing house  SQL request

            DataContractParameter parameterClearingOrgParameters;

            parameterClearingOrgParameters.Query = QUERYCLEARINGORGPARAMETER;
            parameterClearingOrgParameters.Type = CommandType.Text;
            parameterClearingOrgParameters.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32)
                };

            m_ParameterSets.Add(DataContractResultSets.CLEARINGORGPARAMETER, parameterClearingOrgParameters);

            // Init the supplementary risk parameters per market  SQL request

            DataContractParameter parameterEquityMarketParameter;

            parameterEquityMarketParameter.Query = QUERYEQUITYMARKETPARAMETER;
            parameterEquityMarketParameter.Type = CommandType.Text;
            parameterEquityMarketParameter.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32)
                };

            m_ParameterSets.Add(DataContractResultSets.EQUITYMARKETPARAMETER, parameterEquityMarketParameter);

            // Init the roles (for a specific actor) SQL request

            DataContractParameter parameterActorRoles;

            parameterActorRoles.Query = QUERYACTORROLES;
            parameterActorRoles.Type = CommandType.Text;
            parameterActorRoles.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32)
                };

            m_ParameterSets.Add(DataContractResultSets.ACTORROLES, parameterActorRoles);

            // RD 20170420 [23092] Add
            // Init the books (for a specific actor) SQL request

            DataContractParameter parameterBooks;

            parameterBooks.Query = QUERYBOOKS;
            parameterBooks.Type = CommandType.Text;
            parameterBooks.Parameters = new DataParameter[]
                {
                    new DataParameter(pCS, "IDA", DbType.Int32)
                };

            m_ParameterSets.Add(DataContractResultSets.BOOKS, parameterBooks);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryInsert"></param>
        /// <param name="tableIMACTORPOSWork"></param>
        /// <returns></returns>
        /// FI 20210211 [XXXXX] Add
        private static string GetQueryInsertActionPos(string queryInsert, string tableIMACTORPOSWork)
        {
            return queryInsert.Replace("IMACTORPOS_MODEL", tableIMACTORPOSWork);

        }

        /// <summary>
        /// Get the query string  relative to the datacontract key
        /// </summary>
        public static string GetQuery(DataContractResultSets pKey)
        {

            string query = String.Empty;

            if (m_ParameterSets.ContainsKey(pKey))
            {
                query = m_ParameterSets[pKey].Query;
            }

            return query;

        }

        /// <summary>
        /// Fill the IN statements defined in the SQL request returned by the research key, using the provided values matrix
        /// </summary>
        /// <param name="pKey">the research key to obtain the raw sql request to be formatted</param>
        /// <param name="pValues">
        /// values matrix (any matrix line is relative to one single IN statement to fill inside of the request)
        /// </param>
        /// <returns></returns>
        public static string GetQuery(DataContractResultSets pKey, object[][] pValues)
        {

            return GetQuery(pKey, pValues, false);

        }

        /// <summary>
        /// Fill the dynamic statements {x} defined in the SQL request 
        /// (whose static template is returned by the research key), using the provided values matrix
        /// </summary>
        /// <param name="pKey">the research key to obtain the raw sql request to be formatted</param>
        /// <param name="pValues">
        /// values matrix (any matrix line is relative to one single dynamic statement to fill inside of the request).
        /// Any vector row of the matrix may have max length of 999, otherwise an exception will be raised.
        /// </param>
        /// <param name="pAddApexOnString">when true, if the value of the array element is type of string, 
        /// all the values will be delimited with apexes</param>
        /// <exception cref="NotSupportedException">A NotSupported exception will be raised 
        /// when at least one row element constituting the input values matrix is more than 999 length</exception>
        /// <returns>the formatted string</returns>
        public static string GetQuery(DataContractResultSets pKey, object[][] pValues, bool pAddApexOnString)
        {

            string query = Cst.NotAvailable;

            if (m_ParameterSets.ContainsKey(pKey) && !ArrFunc.IsEmpty(pValues))
            {
                List<string> sequences = new List<string>();

                foreach (object[] parameters in pValues)
                {
                    // Oracle limit: the number of IN statements may not overcome 1000 elements.
                    //  We take this limit as a generic DBMS limit, throwing an exception when the number of IN we need to create overcomes 999.
                    if (parameters.Length > 999)
                    {
                        throw new NotSupportedException("SYS-00003");
                    }

                    string sequence = String.Empty;

                    Type sequenceType = typeof(int);

                    for (int idx = 0; idx < parameters.Length; idx++)
                    {
                        if (pAddApexOnString && idx == 0)
                        {
                            sequenceType = parameters[idx].GetType();
                        }

                        if ((idx + 1) == parameters.Length)
                        {
                            sequence = String.Concat(sequence, GetValueForInStatement(parameters[idx], sequenceType));
                        }
                        else
                        {
                            sequence = String.Concat(sequence, GetValueForInStatement(parameters[idx], sequenceType), ",");
                        }
                    }

                    sequences.Add(sequence);
                }

                query = String.Format(m_ParameterSets[pKey].Query, sequences.ToArray());
            }

            return query;

        }

        private static string GetValueForInStatement(object pValueToFormat, Type pValueType)
        {
            string res;

            if (pValueType == typeof(string))
            {
                res = String.Concat("'", Convert.ToString(pValueToFormat), "'");
            }
            else
            {
                res = Convert.ToString(pValueToFormat);
            }

            return res;
        }

        /// <summary>
        /// Get the query type relative to the datacontract key
        /// </summary>
        /// <param name="pKey">the research key</param>
        public static CommandType GetType(DataContractResultSets pKey)
        {

            CommandType commandType = CommandType.Text;

            if (m_ParameterSets.ContainsKey(pKey))
            {
                commandType = m_ParameterSets[pKey].Type;
            }

            return commandType;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        public static DataParameter[] GetParameter(DataContractResultSets pKey)
        {
            DataParameter[] ret = null;
            if (m_ParameterSets.ContainsKey(pKey))
            {
                ret = m_ParameterSets[pKey].Parameters;
            }
            return ret;
        }

        /// <summary>
        /// Get the db parameters collection relative to the datacontract key
        /// </summary>
        /// <param name="pKey">the research key</param>
        /// <param name="pValues">Collection of values to fill parameter values</param>
        /// <returns>A DbDataParameter collection</returns>
        public static IDbDataParameter[] GetDbDataParameters(DataContractResultSets pKey, Dictionary<string, object> pValues)
        {
            IDbDataParameter[] dbParameters = null;

            DataParameter[] parameters = m_ParameterSets[pKey].Parameters;

            if (!ArrFunc.IsEmpty(parameters))
            {
                dbParameters = new IDbDataParameter[parameters.Length];

                for (int idx = 0; idx < parameters.Length; idx++)
                {
                    // IN case some values initialization has been post-poned for some reason (like as big flux xml for Oracle)
                    if (pValues.ContainsKey(parameters[idx].ParameterKey))
                    {
                        parameters[idx].Value = pValues[parameters[idx].ParameterKey];
                    }

                    dbParameters[idx] = parameters[idx].DbDataParameter;
                }
            }

            return dbParameters;
        }

        /// <summary>
        /// get the activation status of a data contract element which implement the IDataContractEnabled interface
        /// </summary>
        /// <param name="pElemToCheck">data contract instance we want to verify its activation status</param>
        /// <param name="pBusinessDate">Reference date</param>
        /// <returns>true when the element is active</returns>
        public static bool GetDataContractElementEnabled(IDataContractEnabled pElemToCheck, DateTime pBusinessDate)
        {
            return
                (pElemToCheck.ElementEnabledFrom == DateTime.MinValue || pElemToCheck.ElementEnabledFrom <= pBusinessDate)
                &&
                (pElemToCheck.ElementDisabledFrom == DateTime.MinValue || pElemToCheck.ElementDisabledFrom >= pBusinessDate);
        }
        #endregion Methods
    }

    /// <summary>
    /// Interface exposing the datelimit value of a data contract element. 
    /// Implement this interface for all the datacontracts that want an activation state check.
    /// </summary>
    public interface IDataContractEnabled
    {
        /// <summary>
        /// Activation date of the element
        /// </summary>
        DateTime ElementEnabledFrom { get; }

        /// <summary>
        /// Deactivation date of the element
        /// </summary>
        DateTime ElementDisabledFrom { get; }
    }
}
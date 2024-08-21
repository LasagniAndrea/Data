using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FixML.Enum;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
//PasGlop Faire un search dans la solution de "TODO FEEMATRIX"

namespace EFS.TradeInformation
{
    /// <summary>
    /// Load Info of FeeMatrixs
    /// 
    /// NB: Cette classe "FeeMatrixs" contient un array de "FeeMatrix"
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("FEEMATRIXS", Namespace = "", IsNullable = true)]
    public class FeeMatrixs
    {
        #region enum
        public enum StatusEnum { Unknown, Valid, Unvalid, Error }
        #endregion

        #region Members
        [System.Xml.Serialization.XmlElementAttribute("FEEMATRIX", Order = 1)]
        public FeeMatrix[] feeMatrix;

        [System.Xml.Serialization.XmlIgnore()]
        private readonly FeeRequest _feeRequest;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient le nbr de feeMatrix
        /// </summary>
        public int Count
        {
            get
            {
                return ArrFunc.Count(feeMatrix);
            }
        }

        /// <summary>
        /// Obtient le nbr de FeeMatrix avec StatusEnum.Valid
        /// </summary>
        public int CountValid
        {
            get
            {
                int ret = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Status == FeeMatrixs.StatusEnum.Valid)
                        ret++;
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient le nbr de FeeMatrix avec statut != StatusEnum.Valid
        /// </summary>
        public int CountNoValid
        {
            get
            {
                int ret = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Status != FeeMatrixs.StatusEnum.Valid)
                        ret++;
                }
                return ret;
            }
        }
        #endregion Accessors

        #region Indexors
        public FeeMatrix this[int pIndex]
        {
            get
            {
                return feeMatrix[pIndex];
            }
        }
        #endregion Indexors

        #region Constructors
        public FeeMatrixs()
        {
        }
        public FeeMatrixs(FeeRequest pFeeRequest)
        {
            _feeRequest = pFeeRequest;
        }
        #endregion Constructors
        //
        #region Methods

        /// <summary>
        /// Chargement des feeMatrix 
        /// </summary>
        /// <param name="pCs"></param>
        public void Initialize(string pCs, IDbTransaction pDbTransaction)
        {
            FeeMatrixs feeMatrixs = FeeMatrixs.LoadFeeMatrix(pCs, pDbTransaction, _feeRequest);
            if (feeMatrixs != null)
                feeMatrix = feeMatrixs.feeMatrix;
        }

        /// <summary>
        /// Retourne les enregistrements compatibles avec l'environement présent dans FeeRequest
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pFeeRequest"></param>
        /// <returns></returns>
        /// FI 20170908 [23409] Modify
        /// EG 20180307 [23769] Gestion dbTransaction
        /// FI 20180502 [23926] Refactoring Requête spécifique en mode Subsitituion de frais voir les modifications où (null != pFeeRequest.substituteInfo)
        private static FeeMatrixs LoadFeeMatrix(string pCs, IDbTransaction pDbTransaction,  FeeRequest pFeeRequest)
        {
            FeeMatrixs ret = null;
            pFeeRequest.CS = pCs;

            int idI = pFeeRequest.Product.IdI;
            int idI_Unl = (null != pFeeRequest.Product.GetUnderlyingAssetIdI()) ? (int)pFeeRequest.Product.GetUnderlyingAssetIdI() : 0;
            //FI 20170908 [23409] use contractId  
            //int idDC = (null != pFeeRequest.sqlDerivativeContract) ? (int)pFeeRequest.sqlDerivativeContract.Id : 0;
            Pair<Cst.ContractCategory, int> contractId = null;
            if (null != pFeeRequest.Contract)
                contractId = new Pair<Cst.ContractCategory, int>(pFeeRequest.Contract.First, pFeeRequest.Contract.Second.Id); 
            
            int idM = (null != pFeeRequest.SqlMarket) ? (int)pFeeRequest.SqlMarket.Id : 0;//PL 20140721

            string status = pFeeRequest.TradeInput.TradeStatus.stBusiness.NewSt;
            
            // FI 20180423 [23924] Add trdType
            Nullable<TrdTypeEnum> trdType = pFeeRequest.Product.GetTrdType(); 

            bool isETDandAllocation = pFeeRequest.TradeInput.IsETDandAllocation;

            // FI 20110819 add assetCategory (category du sous jacent d'un contrat ETD)
            // FI 20170908 [23409] Lecture de pFeeRequest.Contract  
            string assetCategory = string.Empty;
            if (null != pFeeRequest.Contract && pFeeRequest.Contract.First == Cst.ContractCategory.DerivativeContract)
            {
                // PL 20231222 [WI789] Newness - Constitution d'une catégorie virtuelle composée également de celle du SSJ du Future, dans le cas d'une Option sur Future
                assetCategory = ((SQL_DerivativeContract)pFeeRequest.Contract.Second).UnlFuture_AssetCategory + ((SQL_DerivativeContract)pFeeRequest.Contract.Second).AssetCategory;
            }

            // FI 20170908 [23409] add tradableType
            string tradableType = string.Empty;
            if (null != pFeeRequest.Contract && pFeeRequest.Contract.First == Cst.ContractCategory.CommodityContract)
                tradableType = ((SQL_CommodityContract)pFeeRequest.Contract.Second).TradableType;


            SQLInstrCriteria sqlInstrCriteria = new SQLInstrCriteria(pCs, pDbTransaction, idI, false, SQL_Table.ScanDataDtEnabledEnum.Yes);
            //PL 20140721
            //SQLDerivativeContractCriteria sqlDerivativeCriteria = new SQLDerivativeContractCriteria(pCs, idDC, SQL_Table.ScanDataDtEnabledEnum.Yes);
            // FI 20170908 [23409] use sqlContractCriteria 
            //SQLDerivativeContractCriteria sqlDerivativeCriteria = new SQLDerivativeContractCriteria(pCs, idDC, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);
            SQLContractCriteria sqlContractCriteria = new SQLContractCriteria(pCs, pDbTransaction, contractId, idM, SQL_Table.ScanDataDtEnabledEnum.Yes);

            SQLInstrCriteria sqlInstrCriteriaUnl = new SQLInstrCriteria(pCs, pDbTransaction, idI_Unl, true, SQL_Table.ScanDataDtEnabledEnum.Yes);

            #region sqlSelect

            #region Filtre TRADING/CLEARING
            string sqlWhere_TradingClearing = string.Empty;
            if (isETDandAllocation)
            {
                sqlWhere_TradingClearing = SQLCst.AND + "(" + Cst.CrLf;

                //Trade Executed and Cleared
                sqlWhere_TradingClearing += "  ((@ISTAKE_UP=0 and @ISGIVE_UP=0) and ({0}.ISTRADING=1 or {0}.ISCLEARING=1 or {0}.ISTRADINGCLEARING=1))" + Cst.CrLf;
                sqlWhere_TradingClearing += "  or" + Cst.CrLf;
                //Trade Executed and Give-Up
                sqlWhere_TradingClearing += "  ((@ISTAKE_UP=0 and @ISGIVE_UP=1) and ({0}.ISTRADING=1 or {0}.ISTRADINGONLY=1))" + Cst.CrLf;
                sqlWhere_TradingClearing += "  or" + Cst.CrLf;
                //Trade Take-Up and Cleared
                sqlWhere_TradingClearing += "  ((@ISTAKE_UP=1 and @ISGIVE_UP=0) and ({0}.ISCLEARING=1 or {0}.ISCLEARINGONLY=1))" + Cst.CrLf;
                sqlWhere_TradingClearing += "  or" + Cst.CrLf;
                //Trade Take-Up and Give-Up
                sqlWhere_TradingClearing += "  ((@ISTAKE_UP=1 and @ISGIVE_UP=1) and ({0}.ISTRADING=0 and {0}.ISTRADINGONLY=0 and {0}.ISCLEARING=0 and {0}.ISCLEARINGONLY=0 and {0}.ISTRADINGCLEARING=0))" + Cst.CrLf;

                sqlWhere_TradingClearing += ")" + Cst.CrLf;
            }
            #endregion

            string[] aliasFeeSchedule = null;
            // Lors d'une substitution de barème, il a 2 jointures vers FEESCHEDULE
            // Certaines informations sont récupérées sur le barème du substitution (nouveau barème) et d'autres sur le barème substitué (ancien barème)
            // Alias feesched : permet de récupérer les infos du barème substitué
            // Alias feesched2 : permet de récupérer les infos du barème de substitution
            // Exemple STRATEGYTYPESCHED, est alimenté depuis le barème substitué de manière à ne pas exclure le barème de substituion si ce dernier, normalement, ne s'applique pas compte tenu d'un STRATEGYTYPESCHED en non adéquation avec le trade     
            if (null != pFeeRequest.SubstituteInfo)
                aliasFeeSchedule = new string[] { "feesched2", "feesched" };
            else
                aliasFeeSchedule = new string[]{"feesched","feesched"}; 


            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
            sqlSelect.AppendFormat(
@"feemx.IDFEEMATRIX, feemx.IDFEE, feemx.IDENTIFIER, feemx.DISPLAYNAME, feemx.DESCRIPTION,
feemx.ACTION, feemx.GPRODUCT, feemx.TYPEINSTR, feemx.IDINSTR, 0 as FILLER, feemx.TYPECONTRACT, feemx.IDCONTRACT,
feemx.LOWHIGHBASIS, feemx.LOWVALUE, feemx.HIGHVALUE,
feemx.FEETYPEPARTYA, feemx.IDPARTYA, feemx.IDROLEACTOR_PARTYA, feemx.SIDEPARTYA,feemx.INTENTIONPARTYA,
feemx.FEETYPEPARTYB, feemx.IDPARTYB, feemx.IDROLEACTOR_PARTYB, feemx.SIDEPARTYB,feemx.INTENTIONPARTYB,
feemx.FEETYPEOTHERPARTY1, feemx.IDOTHERPARTY1, feemx.IDROLEACTOR_OPART1,
feemx.FEETYPEOTHERPARTY2, feemx.IDOTHERPARTY2, feemx.IDROLEACTOR_OPART2,
feemx.FEEPAYER, feemx.FEERECEIVER, feemx.TAXAPPLICATION, feemx.TAXCONDITION,
feemx.ISINVOICING, feemx.PERIOD, feemx.PERIODMLTP, feemx.RELATIVETO, feemx.ROLLCONVENTION, feemx.PAYMENTRULE,

fee.IDENTIFIER as FEE_IDENTIFIER, fee.DISPLAYNAME as FEE_DISPLAYNAME,
fee.FEETYPE_DEPRECATED as FEE_FEETYPE, fee.EVENTCODE as FEE_EVENTCODE, fee.PAYMENTTYPE as FEE_PAYMENTTYPE,
fee.EVENTTYPE as FEE_EVENTTYPE, fee.ISINVOICING as FEE_ISINVOICING,

{0}.IDFEESCHEDULE, {0}.IDENTIFIER as FEESCHED_IDENTIFIER, {0}.DISPLAYNAME as FEESCHED_DISPLAYNAME,
{0}.GPRODUCT as FEESCHED_GPRODUCT, {0}.TYPEINSTR as FEESCHED_TYPEINSTR, {0}.IDINSTR as FEESCHED_IDINSTR, 0 as FEESCHED_FILLER, 
{0}.TYPECONTRACT as FEESCHED_TYPECONTRACT, {0}.IDCONTRACT as FEESCHED_IDCONTRACT,
{0}.IDA as FEESCHED_IDA,
{0}.BRACKETBASIS as FEESCHED_BRACKETBASIS, {0}.BRACKETBASIS_EXCHT as FEESCHED_BRACKETBASIS_EXCHT, {0}.BRACKETAPPLICATION as FEESCHED_BRACKETAPPLICATION,
{0}.IDC as FEESCHED_IDC, {0}.ROUNDDIR as FEESCHED_ROUNDDIR, {0}.ROUNDPREC as FEESCHED_ROUNDPREC,
{0}.FEE1FORMULABASIS as FEESCHED_FEE1FORMULABASIS, {0}.FEE1FORMULABASIS_EXCHT as FEESCHED_FEE1FORMULABASIS_EXCH,
{0}.FEE1NUM as FEESCHED_FEE1NUM, isnull({0}.FEE1DEN,1) as FEESCHED_FEE1DEN, {0}.FEE1EXPRESSIONTYPE as FEESCHED_FEE1EXPRESSIONTYPE,  
{0}.IDC_FEE1 as FEESCHED_IDC_FEE1, {0}.FEE1_EXCHT as FEESCHED_FEE1_EXCHT,
{0}.FEE2FORMULABASIS as FEESCHED_FEE2FORMULABASIS, {0}.FEE2FORMULABASIS_EXCHT as FEESCHED_FEE2FORMULABASIS_EXCH,
{0}.FEE2NUM as FEESCHED_FEE2NUM, isnull({0}.FEE2DEN,1) as  FEESCHED_FEE2DEN, {0}.FEE2EXPRESSIONTYPE as FEESCHED_FEE2EXPRESSIONTYPE,
{0}.IDC_FEE2 as FEESCHED_IDC_FEE2, {0}.FEE2_EXCHT as FEESCHED_FEE2_EXCHT, 
{0}.FORMULA as FEESCHED_FORMULA, 
{0}.FORMULAXML as FEESCHED_FORMULAXML, {0}.LOFORMULA as FEESCHED_LOFORMULA,
{0}.FORMULA_DCF as FEESCHED_FORMULA_DCF,
{0}.MINNUM as FEESCHED_MINNUM, isnull({0}.MINDEN,1) as FEESCHED_MINDEN, {0}.MAXNUM as FEESCHED_MAXNUM, isnull({0}.MAXDEN,1) as FEESCHED_MAXDEN,
{0}.MINMAXEXPRESSIONT as FEESCHED_MINMAXEXPRESSIONT, {0}.MINMAXBASIS as FEESCHED_MINMAXBASIS, {0}.IDC_MINMAXBASIS as FEESCHED_IDC_MINMAXBASIS, {0}.MINMAXBASIS_EXCHT as FEESCHED_MINMAXBASIS_EXCHT, 
            
feemx.STRATEGYTYPE as STRATEGYTYPEMX, {1}.STRATEGYTYPE as STRATEGYTYPESCHED, {1}.ISAPPLYONALLLEGS,
            
feemx.IDDEFINEEXTENDDET as IDDEFINEEXTENDDETMX, feemx.EXTENDOPERATOR as EXTENDOPERATORMX, feemx.EXTENDVALUE as EXTENDVALUEMX,
{1}.IDDEFINEEXTENDDET as IDDEFINEEXTENDDETSCHED, {1}.EXTENDOPERATOR as EXTENDOPERATORSCHED, {1}.EXTENDVALUE as EXTENDVALUESCHED,
{1}.ISTRADING as FEESCHED_ISTRADING, {1}.ISTRADINGONLY as FEESCHED_ISTRADINGONLY,
{1}.ISCLEARING as FEESCHED_ISCLEARING, {1}.ISCLEARINGONLY as FEESCHED_ISCLEARINGONLY,
{1}.ISTRADINGCLEARING as FEESCHED_ISTRADINGCLEARING,
{0}.PERIODMLTP as FEESCHED_PERIODMLTP,{0}.PERIOD as FEESCHED_PERIOD,{0}.ROLLCONVENTION as FEESCHED_ROLLCONVENTION,
{0}.RELATIVETO as FEESCHED_RELATIVETO,{0}.IDA_PERIOD as FEESCHED_IDA_PERIOD,
isnull({0}.FEESCOPE," + DataHelper.SQLString(Cst.FeeScopeEnum.Trade.ToString()) + ") as FEESCHED_SCOPE", aliasFeeSchedule);//PL 20191210 [25099]        

            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.FEEMATRIX.ToString() + " feemx" + Cst.CrLf;

            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.FEE.ToString() + " fee on (fee.IDFEE=feemx.IDFEE)";
            sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "fee", pFeeRequest.DtReference) + Cst.CrLf;

            // EG 20150708 [21103] New SKP ou non SKP
            if (pFeeRequest.IsSafekeeping)
                sqlSelect += String.Format(" and (fee.EVENTCODE='{0}')", EventCodeFunc.SafeKeepingPayment) + Cst.CrLf;
            else
                sqlSelect += String.Format(" and (fee.EVENTCODE!='{0}' or fee.EVENTCODE is null)", EventCodeFunc.SafeKeepingPayment) + Cst.CrLf;

            
            #region jointure sur FEESCHEDULE (alias feesched)
            //FI 20180426 [23929] modification de la jointure utilisation de la colonne  SCHEDULELIBRARY
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.FEESCHEDULE.ToString() + " feesched on (feesched.IDFEE=fee.IDFEE) and (feesched.SCHEDULELIBRARY=feemx.SCHEDULELIBRARY)" + Cst.CrLf;
            sqlSelect += SQLCst.AND + pFeeRequest.GetCriteriaJoinOnSCHEDULE("feesched", true, sqlInstrCriteria, sqlInstrCriteriaUnl, sqlContractCriteria, sqlWhere_TradingClearing);
            #endregion

            if (null != pFeeRequest.SubstituteInfo)
            {
                // 
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.FEESCHEDULE.ToString() + " feesched2 on (feesched2.IDFEESCHEDULE = @IDFEESCHEDULE2)" + Cst.CrLf;
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCs, "feesched2", pFeeRequest.DtReference) + Cst.CrLf;
                //Remarque pas de filtre sqlWhere_TradingClearing, de manière à ne pas exclure le barème de substituion si ce dernier, normalement, ne s'applique pas compte tenu des checks  Trading / Clearing
                sqlSelect += SQLCst.AND + pFeeRequest.GetCriteriaJoinOnSCHEDULE("feesched2", true, sqlInstrCriteria, sqlInstrCriteriaUnl, sqlContractCriteria, string.Empty);
            }

            #region Where
            //Critère sur DataDtEnabled
            sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCs, "feemx", pFeeRequest.DtReference) + Cst.CrLf;
            //Critère sur idI
            sqlSelect += OTCmlHelper.GetSQLComment("Product/Instrument filter");
            sqlSelect += SQLCst.AND + sqlInstrCriteria.GetSQLRestriction2("feemx", RoleGInstr.FEE);
            //Critère sur IdI_Underlying
            sqlSelect += OTCmlHelper.GetSQLComment("Underlying Instrument filter");
            sqlSelect += SQLCst.AND + sqlInstrCriteriaUnl.GetSQLRestriction2("feemx", RoleGInstr.FEE);
            //Critère sur Contrat 
            sqlSelect += OTCmlHelper.GetSQLComment("Market/Contract filter");
            /// FI 20170908 [23409] use of sqlContractCriteria
            //sqlDerivativeCriteria.IsUseColumnExcept = true;
            //sqlSelect += SQLCst.AND + sqlDerivativeCriteria.GetSQLRestriction(pCs, "feemx", RoleContractRestrict.FEE);
            sqlContractCriteria.IsUseColumnExcept = true;
            sqlSelect += SQLCst.AND + sqlContractCriteria.GetSQLRestriction( "feemx", RoleContractRestrict.FEE);
            
            //Critère sur Currency
            sqlSelect += OTCmlHelper.GetSQLComment("Currency filter");
            if (StrFunc.IsFilled(pFeeRequest.Idc2))
            {
                #region IDC & IDC2
                sqlSelect += SQLCst.AND + "(" + Cst.CrLf;
                sqlSelect += "  (" + Cst.CrLf;
                sqlSelect += "    ((feemx.IDC is null) or (feemx.IDC=@IDC and feemx.ISEXCLUDEIDC=0) or (feemx.IDC!=@IDC and feemx.ISEXCLUDEIDC=1))" + Cst.CrLf;
                sqlSelect += "    " + SQLCst.AND + Cst.CrLf;
                sqlSelect += "    ((feemx.IDC2 is null) or (feemx.IDC2=@IDC2 and feemx.ISEXCLUDEIDC2=0) or (feemx.IDC2!=@IDC2 and feemx.ISEXCLUDEIDC2=1))" + Cst.CrLf;
                sqlSelect += "  )" + Cst.CrLf;
                sqlSelect += "  " + SQLCst.OR + Cst.CrLf;
                sqlSelect += "  (" + Cst.CrLf;
                sqlSelect += "    ((feemx.IDC is null) or (feemx.IDC=@IDC2 and feemx.ISEXCLUDEIDC=0) or (feemx.IDC!=@IDC2 and feemx.ISEXCLUDEIDC=1))" + Cst.CrLf;
                sqlSelect += "    " + SQLCst.AND + Cst.CrLf;
                sqlSelect += "    ((feemx.IDC2 is null) or (feemx.IDC2=@IDC and feemx.ISEXCLUDEIDC2=0) or (feemx.IDC2!=@IDC and feemx.ISEXCLUDEIDC2=1))" + Cst.CrLf;
                sqlSelect += "  )" + Cst.CrLf;
                sqlSelect += ")" + Cst.CrLf;
                #endregion
            }
            else
            {
                sqlSelect += SQLCst.AND + "( (feemx.IDC is null) or (feemx.IDC=@IDC and feemx.ISEXCLUDEIDC=0) or (feemx.IDC!=@IDC and feemx.ISEXCLUDEIDC=1) )" + Cst.CrLf;
            }
            //Critère sur StatusBusiness 
            sqlSelect += OTCmlHelper.GetSQLComment("Business Status filter");
            sqlSelect += SQLCst.AND + "( (feemx.IDSTBUSINESS is null) or (feemx.IDSTBUSINESS=@IDSTBUSINESS) )" + Cst.CrLf;
            
            //Critère sur TRDTYPE
            //FI 20180423 [23924] Add trdType
            sqlSelect += OTCmlHelper.GetSQLComment("TRDTYPE filter");
            if (trdType.HasValue)
                sqlSelect += SQLCst.AND + "( (feemx.TRDTYPE is null) or (feemx.TRDTYPE=@TRDTYPE) )" + Cst.CrLf;
            else
                sqlSelect += SQLCst.AND + "( (feemx.TRDTYPE is null) )" + Cst.CrLf;

            //Critère sur ASSETCATEGORY
            //PL 20231220 [WI789] New "*Future" values and combined values
            sqlSelect += pFeeRequest.GetCriteriaOnASSETCATEGORY(assetCategory, "feemx");

            // FI 20170908 [23409] Add gestion de la colonne TRADABLETYPE
            //Critère sur TRADABLETYPE
            if (StrFunc.IsFilled(tradableType))
            {
                sqlSelect += OTCmlHelper.GetSQLComment("tradableType filter");
                sqlSelect += SQLCst.AND + "( (feemx.TRADABLETYPE is null) or (feemx.TRADABLETYPE = @TRADABLETYPE) )" + Cst.CrLf;
            }
            else
            {
                sqlSelect += SQLCst.AND + "( (feemx.TRADABLETYPE is null) )" + Cst.CrLf;
            }


            //Critères sur Action
            // RD 20120206 feemx.ACTION n'est plus Mandatory
            sqlSelect += OTCmlHelper.GetSQLComment("Action filter");
            sqlSelect += SQLCst.AND + "( (feemx.ACTION is null) or " + DataHelper.SQLGetContains(pCs, "feemx.ACTION", ";", "@ACTION") + " )" + Cst.CrLf;

            //Critères sur Trading/Clearing - PL 20130118 New features
            sqlSelect += StrFunc.AppendFormat(sqlWhere_TradingClearing, "feemx");
            
            //PL 20141011 Create and Use GetCriteriaJoinOnACTOR()
            sqlSelect += pFeeRequest.GetCriteriaJoinOnACTOR("feemx", true);
            
            if (null != pFeeRequest.SubstituteInfo)
            {
                // filtre sur le barème substitué
                sqlSelect += OTCmlHelper.GetSQLComment("Substitute Feechedule filter");
                sqlSelect += SQLCst.AND + "( (feemx.IDFEEMATRIX=@IDFEEMATRIX) )" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "( (feesched.IDFEESCHEDULE=@IDFEESCHEDULE) )" + Cst.CrLf;

            }
            #endregion

            #region Order
            //PL 20130107 On doit (sauf erreur) trier sur "feesched.IDENTIFIER" et non pas sur "feemx.IDFEESCHEDULE" (Indispensable à l'exploitation des barèmes dérogatoires).
            //sqlSelect += SQLCst.ORDERBY + "feemx.IDFEE asc,feemx.IDFEESCHEDULE asc,";
            //PL 20130927 Le tri n'est plus utile à la gestion des barèmes dérogatoires
            //sqlSelect += SQLCst.ORDERBY + "feemx.IDFEE asc,feesched.IDENTIFIER asc,feesched.IDA" + DataHelper.GetOrderBySideForNullValueInLastPosition(pCs);
            sqlSelect += SQLCst.ORDERBY + "fee.PAYMENTTYPE,fee.ISINVOICING,feesched.IDENTIFIER";
            #endregion
            #endregion sqlSelect

            //int idPermission = PermissionTools.GetIdPermission(pCs, pDbTransaction, pFeeRequest.Action, PermissionEnum.Create);
            ////PL 20150320 TBD
            //bool isPositionEffectClose = false;
            //if (isETDandAllocation 
            //    && (pFeeRequest.Action == IdMenu.GetIdMenu(IdMenu.Menu.InputTrade))
            //    && pFeeRequest.ExchangeTradedDerivative.tradeCaptureReport.TrdCapRptSideGrp[0].PositionEffectSpecified
            //    )
            //{
            //    isPositionEffectClose = (pFeeRequest.ExchangeTradedDerivative.tradeCaptureReport.TrdCapRptSideGrp[0].PositionEffect == FixML.v50SP1.Enum.PositionEffectEnum.Close);

            //    if (isPositionEffectClose)
            //    {
            //        //Search on Clearing action (Close)
            //        idPermission = PermissionTools.GetIdPermission(pCs, pDbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE_CLR), PermissionEnum.Create);
            //    }
            //}
            // FI 20180502 [23926] Lecture de  pFeeRequest.IdPermission
            // PL 20200217 [25207] Change DataType STRING instead of INT
            //int idPermission = pFeeRequest.IdPermission[0]; 
            string idPermission = pFeeRequest.IdPermission[0]; 

            DataParameters parameters = new DataParameters();
            //parameters.Add(new DataParameter(pCs, "ACTION", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), idPermission.ToString());
            parameters.Add(new DataParameter(pCs, "ACTION", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), idPermission);
            if (isETDandAllocation)
            {
                parameters.Add(new DataParameter(pCs, "ISTAKE_UP", DbType.Boolean), pFeeRequest.TradeInput.IsTakeUp(pCs,pDbTransaction));
                parameters.Add(new DataParameter(pCs, "ISGIVE_UP", DbType.Boolean), pFeeRequest.TradeInput.IsGiveUp(pCs,pDbTransaction));
            }
            parameters.Add(new DataParameter(pCs, "IDSTBUSINESS", DbType.AnsiString, SQLCst.UT_STATUS_LEN), status);

            // FI 20180423 [23924] Add trdType
            if (trdType.HasValue)
                parameters.Add(new DataParameter(pCs, "TRDTYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), ReflectionTools.ConvertEnumToString<TrdTypeEnum>(trdType.Value));
            
            parameters.Add(new DataParameter(pCs, "IDC", DbType.AnsiString, SQLCst.UT_CURR_LEN), pFeeRequest.Idc);
            if (StrFunc.IsFilled(pFeeRequest.Idc2))
                parameters.Add(new DataParameter(pCs, "IDC2", DbType.AnsiString, SQLCst.UT_CURR_LEN), pFeeRequest.Idc2);
            if (StrFunc.IsFilled(assetCategory))
                parameters.Add(new DataParameter(pCs, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), assetCategory);
            // FI 20170908 [23409] add parameter TRADABLETYPE
            if (StrFunc.IsFilled(tradableType))
                parameters.Add(new DataParameter(pCs, "TRADABLETYPE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), tradableType);

            if (null != pFeeRequest.SubstituteInfo)
            {
                parameters.Add(new DataParameter(pCs, "IDFEEMATRIX", DbType.Int32), pFeeRequest.SubstituteInfo.source.First);
                parameters.Add(new DataParameter(pCs, "IDFEESCHEDULE", DbType.Int32), pFeeRequest.SubstituteInfo.source.Second);
                parameters.Add(new DataParameter(pCs, "IDFEESCHEDULE2", DbType.Int32), pFeeRequest.SubstituteInfo.targetIdFeeSchedule);
            }

            // RD 20170208 [22815]
            //if (StrFunc.IsFilled(pFeeRequest.AssessmentBasis))//PL 20141017
            //    parameters.Add(new DataParameter(pCs, "FEE1FORMULABASIS", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pFeeRequest.AssessmentBasis);

            QueryParameters qry = new QueryParameters(pCs, sqlSelect.ToString(), parameters);
            DataSet dsResult = DataHelper.ExecuteDataset(qry.Cs, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

            // FI 20180502 [23926] Boucle sur pFeeRequest.IdPermission
            ////PL 20150320 TBD
            //if (isPositionEffectClose)
            //{
            //    if ((dsResult.Tables[0].Rows == null) || (dsResult.Tables[0].Rows.Count == 0))
            //    {
            //        //New search on initial action
            //        parameters["ACTION"].Value = PermissionTools.GetIdPermission(pCs, pDbTransaction, pFeeRequest.Action, PermissionEnum.Create).ToString();

            //        qry = new QueryParameters(pCs, sqlSelect.ToString(), parameters);
            //        dsResult = DataHelper.ExecuteDataset(qry.Cs, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            //    }
            //}

            // PL 20200217 [25207] Change DataType STRING instead of INT
            //foreach (int item in pFeeRequest.IdPermission.Where(x => x != idPermission))
            foreach (string item in pFeeRequest.IdPermission.Where(x => x != idPermission))
            {
                Boolean isEmpty = (dsResult.Tables[0].Rows == null) || (dsResult.Tables[0].Rows.Count == 0);
                if (isEmpty)
                {
                    //parameters["ACTION"].Value = item.ToString();
                    parameters["ACTION"].Value = item;
                    qry = new QueryParameters(pCs, sqlSelect.ToString(), parameters);
                    dsResult = DataHelper.ExecuteDataset(qry.Cs, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                }
                else
                {
                    break;
                }
            }

            dsResult.DataSetName = "FEEMATRIXS";
            DataTable dtTable = dsResult.Tables[0];
            dtTable.TableName = "FEEMATRIX";
            
            // EG 20150708 [21103]
            if ((dtTable.Rows != null) && (dtTable.Rows.Count > 0))
            {
                string serializerResult = new DatasetSerializer(dsResult).Serialize();

                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(FeeMatrixs), serializerResult);
                ret = (FeeMatrixs)CacheSerializer.Deserialize(serializeInfo);
            }
            return ret;
        }
        #endregion Methods
    }

    [Serializable]
    public class FeeMatrix : ICloneable
    {
        #region Members
        #region FEEMATRIX
        [System.Xml.Serialization.XmlElementAttribute("IDFEEMATRIX", Order = 1)]
        public int IDFEEMATRIX;
        //		
        [System.Xml.Serialization.XmlElementAttribute("IDFEE", Order = 2)]
        public int IDFEE;
        //
        [System.Xml.Serialization.XmlElementAttribute("IDENTIFIER", Order = 3)]
        public string IDENTIFIER;
        //
        [System.Xml.Serialization.XmlElementAttribute("DISPLAYNAME", Order = 4)]
        public string DISPLAYNAME;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DESCRIPTIONSpecified;
        [System.Xml.Serialization.XmlElementAttribute("DESCRIPTION", Order = 5)]
        public string DESCRIPTION;
        //
        [System.Xml.Serialization.XmlElementAttribute("ACTION", Order = 6)]
        public string ACTION;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GPRODUCTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("GPRODUCT", Order = 7)]
        public string GPRODUCT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TYPEINSTRSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TYPEINSTR", Order = 8)]
        public string TYPEINSTR;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDINSTRSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDINSTR", Order = 9)]
        public int IDINSTR;
        //
        [System.Xml.Serialization.XmlElementAttribute("FILLER", Order = 10)]
        public int FILLER;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TYPECONTRACTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TYPECONTRACT", Order = 11)]
        public string TYPECONTRACT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDCONTRACTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDCONTRACT", Order = 12)]
        public int IDCONTRACT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LOWHIGHBASISSpecified;
        [System.Xml.Serialization.XmlElementAttribute("LOWHIGHBASIS", Order = 13)]
        public string LOWHIGHBASIS;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LOWVALUESpecified;
        [System.Xml.Serialization.XmlElementAttribute("LOWVALUE", Order = 14)]
        public decimal LOWVALUE;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HIGHVALUESpecified;
        [System.Xml.Serialization.XmlElementAttribute("HIGHVALUE", Order = 15)]
        public decimal HIGHVALUE;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEETYPEPARTYASpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEETYPEPARTYA", Order = 16)]
        public string FEETYPEPARTYA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDPARTYASpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDPARTYA", Order = 17)]
        public int IDPARTYA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDROLEACTOR_PARTYASpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR_PARTYA", Order = 18)]
        public string IDROLEACTOR_PARTYA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SIDEPARTYASpecified;
        [System.Xml.Serialization.XmlElementAttribute("SIDEPARTYA", Order = 19)]
        public string SIDEPARTYA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool INTENTIONPARTYASpecified;
        [System.Xml.Serialization.XmlElementAttribute("INTENTIONPARTYA", Order = 20)]
        public string INTENTIONPARTYA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEETYPEPARTYBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEETYPEPARTYB", Order = 21)]
        public string FEETYPEPARTYB;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDPARTYBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDPARTYB", Order = 22)]
        public int IDPARTYB;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDROLEACTOR_PARTYBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR_PARTYB", Order = 23)]
        public string IDROLEACTOR_PARTYB;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SIDEPARTYBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("SIDEPARTYB", Order = 24)]
        public string SIDEPARTYB;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool INTENTIONPARTYBSpecified;
        [System.Xml.Serialization.XmlElementAttribute("INTENTIONPARTYB", Order = 25)]
        public string INTENTIONPARTYB;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEETYPEOTHERPARTY1Specified;
        [System.Xml.Serialization.XmlElementAttribute("FEETYPEOTHERPARTY1", Order = 26)]
        public string FEETYPEOTHERPARTY1;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDOTHERPARTY1Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDOTHERPARTY1", Order = 27)]
        public int IDOTHERPARTY1;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDROLEACTOR_OPART1Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR_OPART1", Order = 28)]
        public string IDROLEACTOR_OPART1;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEETYPEOTHERPARTY2Specified;
        [System.Xml.Serialization.XmlElementAttribute("FEETYPEOTHERPARTY2", Order = 29)]
        public string FEETYPEOTHERPARTY2;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDOTHERPARTY2Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDOTHERPARTY2", Order = 30)]
        public int IDOTHERPARTY2;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDROLEACTOR_OPART2Specified;
        [System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR_OPART2", Order = 31)]
        public string IDROLEACTOR_OPART2;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEEPAYERSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEEPAYER", Order = 32)]
        public string FEEPAYER;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEERECEIVERSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEERECEIVER", Order = 33)]
        public string FEERECEIVER;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TAXAPPLICATIONSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TAXAPPLICATION", Order = 34)]
        public string TAXAPPLICATION;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TAXCONDITIONSpecified;
        [System.Xml.Serialization.XmlElementAttribute("TAXCONDITION", Order = 35)]
        public string TAXCONDITION;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ISINVOICINGSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ISINVOICING", Order = 36)]
        public bool ISINVOICING;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PERIODSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PERIOD", Order = 37)]
        public string PERIOD;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PERIODMLTPSpecified;
        [System.Xml.Serialization.XmlElementAttribute("PERIODMLTP", Order = 38)]
        public int PERIODMLTP;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RELATIVETOSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RELATIVETO", Order = 39)]
        public string RELATIVETO;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ROLLCONVENTIONSpecified;
        [System.Xml.Serialization.XmlElementAttribute("ROLLCONVENTION", Order = 40)]
        public string ROLLCONVENTION;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PAYMENTRULESpecified;
        [System.Xml.Serialization.XmlElementAttribute("PAYMENTRULE", Order = 41)]
        public string PAYMENTRULE;
        #endregion FEEMATRIX
        #region FEE
        [System.Xml.Serialization.XmlElementAttribute("FEE_IDENTIFIER", Order = 42)]
        public string FEE_IDENTIFIER;
        //
        [System.Xml.Serialization.XmlElementAttribute("FEE_DISPLAYNAME", Order = 43)]
        public string FEE_DISPLAYNAME;
        [System.Xml.Serialization.XmlElementAttribute("FEE_FEETYPE", Order = 44)]
        public string FEE_FEETYPE;
        //
        [System.Xml.Serialization.XmlElementAttribute("FEE_EVENTCODE", Order = 45)]
        public string FEE_EVENTCODE;
        //
        [System.Xml.Serialization.XmlElementAttribute("FEE_PAYMENTTYPE", Order = 46)]
        public string FEE_PAYMENTTYPE;
        // EG 20100505
        //[System.Xml.Serialization.XmlElementAttribute("FEE_IDTAX", Order = 46)]
        //public string FEE_IDTAX;
        [System.Xml.Serialization.XmlElementAttribute("FEE_EVENTTYPE", Order = 47)]
        public string FEE_EVENTTYPE;
        //
        [System.Xml.Serialization.XmlElementAttribute("FEE_ISINVOICING", Order = 48)]
        public string FEE_ISINVOICING;
        #endregion FEE
        #region FEESCHEDULE
        [System.Xml.Serialization.XmlElementAttribute("IDFEESCHEDULE", Order = 49)]
        public int IDFEESCHEDULE;

        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDENTIFIER", Order = 50)]
        public string FEESCHED_IDENTIFIER;
        //
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_DISPLAYNAME", Order = 51)]
        public string FEESCHED_DISPLAYNAME;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_GPRODUCTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_GPRODUCT", Order = 52)]
        public string FEESCHED_GPRODUCT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_TYPEINSTRSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_TYPEINSTR", Order = 53)]
        public string FEESCHED_TYPEINSTR;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDINSTRSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDINSTR", Order = 54)]
        public int FEESCHED_IDINSTR;
        //
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FILLER", Order = 55)]
        public int FEESCHED_FILLER;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_TYPECONTRACTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_TYPECONTRACT", Order = 56)]
        public string FEESCHED_TYPECONTRACT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDCONTRACTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDCONTRACT", Order = 57)]
        public int FEESCHED_IDCONTRACT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDASpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDA", Order = 58)]
        public int FEESCHED_IDA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_BRACKETBASISSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_BRACKETBASIS", Order = 59)]
        public string FEESCHED_BRACKETBASIS;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_BRACKETBASIS_EXCHTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_BRACKETBASIS_EXCHT", Order = 60)]
        public string FEESCHED_BRACKETBASIS_EXCHT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_BRACKETAPPLICATIONSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_BRACKETAPPLICATION", Order = 61)]
        public string FEESCHED_BRACKETAPPLICATION;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDCSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDC", Order = 62)]
        public string FEESCHED_IDC;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ROUNDDIRSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ROUNDDIR", Order = 63)]
        public string FEESCHED_ROUNDDIR;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ROUNDPRECSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ROUNDPREC", Order = 64)]
        public int FEESCHED_ROUNDPREC;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE1FORMULABASISSpecified;//PL 20141017
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE1FORMULABASIS", Order = 65)]
        public string FEESCHED_FEE1FORMULABASIS;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE1FORMULABASIS_EXCHTSpecified;//PL 20141017
        // EG 20150612 [21076] Use XmlElementAttribute FEESCHED_FEE1FORMULABASIS_EXCH instead FEESCHED_FEE1FORMULABASIS_EXCHT (Oracle <=30)
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE1FORMULABASIS_EXCH", Order = 66)]
        public string FEESCHED_FEE1FORMULABASIS_EXCHT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE1NUMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE1NUM", Order = 67)]
        public float FEESCHED_FEE1NUM;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE1DENSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE1DEN", Order = 68)]
        public int FEESCHED_FEE1DEN;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE1EXPRESSIONTYPESpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE1EXPRESSIONTYPE", Order = 69)]
        public string FEESCHED_FEE1EXPRESSIONTYPE;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDC_FEE1Specified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDC_FEE1", Order = 70)]
        public string FEESCHED_IDC_FEE1;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE1_EXCHTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE1_EXCHT", Order = 71)]
        public string FEESCHED_FEE1_EXCHT;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE2FORMULABASISSpecified;//PL 20141017
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE2FORMULABASIS", Order = 72)]
        public string FEESCHED_FEE2FORMULABASIS;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE2FORMULABASIS_EXCHTSpecified;//PL 20141017
        // EG 20150612 [21076] Use XmlElementAttribute FEESCHED_FEE2FORMULABASIS_EXCH instead FEESCHED_FEE2FORMULABASIS_EXCHT (Oracle <=30)
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE2FORMULABASIS_EXCH", Order = 73)]
        public string FEESCHED_FEE2FORMULABASIS_EXCHT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE2NUMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE2NUM", Order = 74)]
        public float FEESCHED_FEE2NUM;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE2DENSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE2DEN", Order = 75)]
        public int FEESCHED_FEE2DEN;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE2EXPRESSIONTYPESpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE2EXPRESSIONTYPE", Order = 76)]
        public string FEESCHED_FEE2EXPRESSIONTYPE;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDC_FEE2Specified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDC_FEE2", Order = 77)]
        public string FEESCHED_IDC_FEE2;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FEE2_EXCHTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FEE2_EXCHT", Order = 78)]
        public string FEESCHED_FEE2_EXCHT;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FORMULASpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FORMULA", Order = 79)]
        public string FEESCHED_FORMULA;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FORMULAXMLSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FORMULAXML", Order = 80)]
        public string FEESCHED_FORMULAXML;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_LOFORMULASpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_LOFORMULA", Order = 81)]
        public string FEESCHED_LOFORMULA;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_FORMULA_DCFSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_FORMULA_DCF", Order = 82)]
        public string FEESCHED_FORMULA_DCF;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MINNUMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MINNUM", Order = 83)]
        public float FEESCHED_MINNUM;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MINDENSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MINDEN", Order = 84)]
        public int FEESCHED_MINDEN;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MAXNUMSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MAXNUM", Order = 85)]
        public float FEESCHED_MAXNUM;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MAXDENSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MAXDEN", Order = 86)]
        public int FEESCHED_MAXDEN;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MINMAXEXPRESSIONTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MINMAXEXPRESSIONT", Order = 87)]
        public string FEESCHED_MINMAXEXPRESSIONT;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MINMAXBASISSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MINMAXBASIS", Order = 88)]
        public string FEESCHED_MINMAXBASIS;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDC_MINMAXBASISSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDC_MINMAXBASIS", Order = 89)]
        public string FEESCHED_IDC_MINMAXBASIS;
        //        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_MINMAXBASIS_EXCHTSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_MINMAXBASIS_EXCHT", Order = 90)]
        public string FEESCHED_MINMAXBASIS_EXCHT;
        #endregion FEESCHEDULE

        // 20120607 MF Ticket 17864
        #region Strategy criteria
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool STRATEGYTYPEMXSpecified;
        /// <summary>
        /// FEEMATRIX.STRATEGYTYPE - type of the strategy target
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("STRATEGYTYPEMX", Order = 91)]
        public string STRATEGYTYPEMX;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_STRATEGYTYPESCHEDSpecified;
        /// <summary>
        /// FEESCHEDULE.STRATEGYTYPE - type of the strategy target
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("STRATEGYTYPESCHED", Order = 92)]
        public string FEESCHED_STRATEGYTYPESCHED;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ISAPPLYONALLLEGSpecified;
        /// <summary>
        /// FEESCHEDULE.ISAPPLYONALLLEGS - if true the fee has to be applied on all legs, otherwise just the first one will be concerned
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ISAPPLYONALLLEGS", Order = 93)]
        public bool FEESCHED_ISAPPLYONALLLEGS;
        #endregion Strategy criteria

        // 20120807 MF Ticket 18067
        #region Extend criteria

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IDDEFINEEXTENDDETMXSpecified;
        /// <summary>
        /// FEEMATRIX.IDDEFINEEXTENDDET - Internal id of the Extention stored in the DEFINEEXTENDDET table
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDDEFINEEXTENDDETMX", Order = 94)]
        public int IDDEFINEEXTENDDETMX;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EXTENDOPERATORMXSpecified;
        /// <summary>
        /// FEEMATRIX.EXTENDOPERATOR - Operator used to compare the extention values of the instruction and the trade. 
        ///                            IF they match the fee is applied. 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("EXTENDOPERATORMX", Order = 95)]
        public string EXTENDOPERATORMX;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EXTENDVALUEMXSpecified;
        /// <summary>
        /// FEEMATRIX.EXTENDVALUE - Extention value of of the instruction
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("EXTENDVALUEMX", Order = 96)]
        public string EXTENDVALUEMX;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDDEFINEEXTENDDETSpecified;
        /// <summary>
        /// FEESCHEDULE.IDDEFINEEXTENDDET - Internal id of the Extention stored in the DEFINEEXTENDDET table
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("IDDEFINEEXTENDDETSCHED", Order = 97)]
        public int FEESCHED_IDDEFINEEXTENDDET;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_EXTENDOPERATORSpecified;
        /// <summary>
        /// FEESCHEDULE.EXTENDOPERATOR - Operator used to compare the extention values of the schedule item and the trade. 
        ///                              IF they match the fee is applied. 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("EXTENDOPERATORSCHED", Order = 98)]
        public string FEESCHED_EXTENDOPERATOR;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_EXTENDVALUESpecified;
        /// <summary>
        /// FEESCHEDULE.EXTENDVALUE - Extention value of the schedule item 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("EXTENDVALUESCHED", Order = 99)]
        public string FEESCHED_EXTENDVALUE;
        #endregion

        //PL 20130118 New features
        #region Trading/Clearing
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ISTRADINGSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ISTRADING", Order = 100)]
        public bool FEESCHED_ISTRADING;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ISTRADINGONLYSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ISTRADINGONLY", Order = 101)]
        public bool FEESCHED_ISTRADINGONLY;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ISCLEARINGSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ISCLEARING", Order = 102)]
        public bool FEESCHED_ISCLEARING;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ISCLEARINGONLYSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ISCLEARINGONLY", Order = 103)]
        public bool FEESCHED_ISCLEARINGONLY;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ISTRADINGCLEARINGSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ISTRADINGCLEARING", Order = 104)]
        public bool FEESCHED_ISTRADINGCLEARING;
        #endregion

        //PL 20130620 New features
        #region Period (Braket)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_PERIODMLTPSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_PERIODMLTP", Order = 105)]
        public int FEESCHED_PERIODMLTP;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_PERIODSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_PERIOD", Order = 106)]
        public string FEESCHED_PERIOD;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_ROLLCONVENTIONSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_ROLLCONVENTION", Order = 107)]
        public string FEESCHED_ROLLCONVENTION;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_RELATIVETOSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_RELATIVETO", Order = 108)]
        public string FEESCHED_RELATIVETO;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_IDA_PERIODSpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_IDA_PERIOD", Order = 109)]
        public int FEESCHED_IDA_PERIOD;
        #endregion

        #region FEESCHEDULE (suite)
        //PL 20191210 [25099]        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FEESCHED_SCOPESpecified;
        [System.Xml.Serialization.XmlElementAttribute("FEESCHED_SCOPE", Order = 110)]
        public string FEESCHED_SCOPE;
        #endregion FEESCHEDULE (suite)

        #region Ignore
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public FeeMatrixs.StatusEnum Status;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string CommentForDebug;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsReverseParty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int MatchedOtherParty1Index;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int MatchedOtherParty2Index;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Party PartyPayer;
        //PL 20221124 PL 20230718 Test FL/Tradition - Use Party instead of PartyBase 
        //public PartyBase PartyPayer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Party PartyReceiver;
        //PL 20221124 PL 20230718 Test FL/Tradition - Use Party instead of PartyBase
        //public PartyBase PartyReceiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FEESCHED_IDA_Identifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FEESCHED_IDA_Displayname;

        #endregion Ignore
        #endregion Members

        #region ICloneable Membres
        // 20120607 MF Ticket 17864
        /// <summary>
        /// Get a new instance of the matrix with the same value
        /// </summary>
        /// <returns>a new copy of the current matrix</returns>
        public object Clone()
        {
            if (this is null)
            {
                return default(FeeMatrix);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);
                return (FeeMatrix)formatter.Deserialize(stream);
            }
        }
        #endregion
    }
}
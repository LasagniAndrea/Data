#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
//
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process
{

    /// <summary>
    /// 
    /// </summary>
    public class CollateralValProcess : ProcessBase
    {
        #region Members
        /// <summary>
        /// Message Queue reçu
        /// </summary>
        private readonly CollateralValMQueue _collateralValMQueue;
        /// <summary>
        /// cotation inclue ds message
        /// </summary>
        private readonly Quote _quote;
        /// <summary>
        /// 
        /// </summary>
        private DatasetPosCollateral _dsPosCol;
        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsProcessSendMessage
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.POSCOLLATERAL.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.POSCOLLATERAL;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsMonoDataProcess
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Obtient true si le traitement s'applique suite à l'entrée d'une cotation
        /// </summary>
        private bool IsQuoteExist
        {
            get { return (null != _collateralValMQueue.Quote); }
        }

        #endregion Accessors
        #region Constructor
        public CollateralValProcess(MQueueBase pMQueue, AppInstanceService pAppInstance)
            : base(pMQueue, pAppInstance)
        {
            _collateralValMQueue = (CollateralValMQueue)pMQueue;
            if (null != _collateralValMQueue.Quote)
                _quote = _collateralValMQueue.Quote;
        }
        #endregion Constructor
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void SelectDatas()
        {
            //FI 20111103 TODO 
            base.SelectDatas();
        }

        /// <summary>
        /// Vérifications ultimes (Vérification que le collateral existe) et Mise en place d'un Lock 
        /// </summary>
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();
            if (false == IsProcessObserver)
            {
                if (false == PosCollateralRDBMSTools.IsCollateralExist(this.Cs, CurrentId))
                {
                    ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, StrFunc.AppendFormat("Collateral [Id:{0}] not found", CurrentId.ToString()),
                        processState);
                }
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                    ProcessState.CodeReturn = LockCurrentObjectId();
            }
        }

        /// <summary>
        /// Instancie le Log, Tracker Etc.
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            _dsPosCol = new DatasetPosCollateral();
            _dsPosCol.LoadDs(Cs, null, CurrentId);
            if (_dsPosCol.DtPOSCOLLATERAL.Rows.Count == 0)
                throw new Exception(StrFunc.AppendFormat("No rows in POSCOLLATERAL with Id:[{0}]", CurrentId));

            // AL 20240725 [WI1008] Add current processing POSCOLLATERAL Id into log
            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(ProcessStateTools.StatusEnum.NONE),
                new SysMsgCode(SysCodeEnum.LOG, 623), 0,
                new LogParam(CurrentId)));
            //
            //Si modification d'un asset côté (Ex:EQUITY)        
            if (IsQuoteExist && _collateralValMQueue.quote.action == DataRowState.Deleted.ToString())
            {
                DataRow dr = _dsPosCol.GetPosCollateralValRow(_collateralValMQueue.quote.time.Date);
                if (null != dr)
                    dr.Delete();
            }
            else
            {
                // AL 20240725 [WI1008] Retrieve the ErrLevel from Valorize, otherwise it's always SUCCESS (unless exceptions aren't thrown)
                ret = Valorize();
            }
            UpdateDatabase();
            return ret;
        }

        /// <summary>
        ///  Calcul des valorisations (Mise à jour de la db (insert or update)
        /// </summary>
        // AL 20240725 [WI1008] Added return Cst.ErrLevel
        private Cst.ErrLevel Valorize()
        {
            // AL 20240725 [WI1008] Added return Cst.ErrLevel
            Cst.ErrLevel quoteRC = Cst.ErrLevel.SUCCESS;
            DataRow rowPosCollateral = _dsPosCol.DtPOSCOLLATERAL.Rows[0];
            //dtBusiness
            DateTime dtBusiness = Convert.ToDateTime(rowPosCollateral["DTBUSINESS"]);
            //AssetCategory
            string underlyingAssetRow = rowPosCollateral["ASSETCATEGORY"].ToString();
            if (false == Enum.IsDefined(typeof(Cst.UnderlyingAsset), underlyingAssetRow))
                throw new Exception(StrFunc.AppendFormat("{0} is not defined", underlyingAssetRow));
            Cst.UnderlyingAsset underlyingAsset = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), underlyingAssetRow);
            //duration
            string durationRow = rowPosCollateral["DURATION"].ToString();
            if (false == Enum.IsDefined(typeof(RepoDurationEnum), durationRow))
                throw new Exception(StrFunc.AppendFormat("value {0} is not valid in RepoDurationEnum", durationRow));
            RepoDurationEnum duration = (RepoDurationEnum)Enum.Parse(typeof(RepoDurationEnum), durationRow);
            //dtTermination
            Nullable<DateTime> dtTermination = null;
            if (Cst.UnderlyingAsset.Cash == underlyingAsset)
            {
                //Cas particulier sur les assets cash
                //Seule 1 ligne existe dans POSCOLLATERAL, elle est en date dtBusiness  
                //DTTERMINATION permet de spécifier que la dépôt de garanti est valide jusqu'à DTTERMINATION
                dtTermination = dtBusiness.AddDays(1);
            }
            else
            {
                if (Convert.DBNull != rowPosCollateral["DTTERMINATION"])
                    dtTermination = Convert.ToDateTime(rowPosCollateral["DTTERMINATION"]);
                if (duration == RepoDurationEnum.Overnight)
                    dtTermination = dtBusiness.AddDays(1);
            }
            //FI 20120208 gestion des UnderlyingAsset de type cash 
            Nullable<QuoteEnum> quoteEnum = null;
            if (underlyingAsset != Cst.UnderlyingAsset.Cash)
            {
                // EG 20160404 Migration vs2013
                //quoteEnum = AssetTools.ConvertToQuoteEnum(underlyingAsset);
                quoteEnum = AssetTools.ConvertUnderlyingAssetToQuoteEnum(underlyingAsset);
                if (quoteEnum != QuoteEnum.EQUITY & quoteEnum != QuoteEnum.DEBTSECURITY)
                    throw new Exception(StrFunc.AppendFormat("{0} is not implemented", quoteEnum.ToString()));
            }
            //Asset
            int idAsset = Convert.ToInt32(rowPosCollateral["IDASSET"]);
            DebtSecurityContainer debtSecurity = null;
            SQL_AssetBase sqlAsset;
            if (quoteEnum == QuoteEnum.DEBTSECURITY)
            {
                sqlAsset = new SQL_AssetDebtSecurity(Cs, idAsset);
                DataDocumentContainer assetLoad = Tools.LoadDebtSecurity(Cs, null, idAsset);
                debtSecurity = new DebtSecurityContainer((IDebtSecurity)assetLoad.CurrentProduct.Product);
            }
            else if (quoteEnum == QuoteEnum.EQUITY)
            {
                sqlAsset = new SQL_AssetEquity(Cs, idAsset);
            }
            else //Identique à quoteEnum == null
                sqlAsset = new SQL_AssetCash(Cs, idAsset);

            sqlAsset.LoadTable();

            DateTime dtQuotation;
            if (IsQuoteExist)
                dtQuotation = _quote.time.Date;
            else
                dtQuotation = dtBusiness;
            //
            // AL 20240718 [WI1005] Stop recursive quotation search on future date 
            DateTime today = OTCmlHelper.GetDateSys(Cs);
            bool isContinue = true;            
            while (isContinue)
            {                
                if (quoteEnum == QuoteEnum.EQUITY)
                    // AL 20240725 [WI1008] Added return Cst.ErrLevel
                    quoteRC = ValorizeEquity(dtQuotation, (SQL_AssetEquity)sqlAsset);
                else if (quoteEnum == QuoteEnum.DEBTSECURITY)
                    // AL 20240725 [WI1008] Added return Cst.ErrLevel
                    quoteRC = ValorizeDebtSecurity(dtQuotation, (SQL_AssetDebtSecurity)sqlAsset, debtSecurity);
                else
                    ValorizeCash(dtQuotation, (SQL_AssetCash)sqlAsset);
                //
                if (IsQuoteExist)
                {
                    //Si modification d'une quotation Spheres® évalue le dépôt de garanti à cette date uniquement
                    isContinue = false;
                }
                else
                {
                    if (dtTermination.HasValue)
                        isContinue = dtQuotation < dtTermination.Value.AddDays(-1);
                }
                //
                // AL 20240718 [WI1005] Stop recursive quotation search on future date 
                //if (isContinue)
                //    dtQuotation = dtQuotation.AddDays(1);
                dtQuotation = dtQuotation.AddDays(1);
                // AL 20240725 [WI1008] Stop cicle if quote not found
                isContinue = isContinue && dtQuotation.Date <= today.Date && (quoteRC == Cst.ErrLevel.SUCCESS);
            }
            // AL 20240725 [WI1008] Added return Cst.ErrLevel
            return quoteRC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pSqlAssetEquity"></param>
        // AL 20240725 [WI1008] Added return Cst.ErrLevel
        private Cst.ErrLevel ValorizeEquity(DateTime pDate, SQL_AssetEquity pSqlAssetEquity)
        {            
            IProductBase productBase = new FpML.v44.Ird.Fra();
            DateTime dt = pDate;
            string idC = pSqlAssetEquity.IdC;
            KeyQuote keyQuote = new KeyQuote(Cs, dt);
            SQL_Quote quote = new SQL_Quote(Cs, QuoteEnum.EQUITY, AvailabilityEnum.NA, productBase, keyQuote, pSqlAssetEquity.Id);
            // AL 20240725 [WI1008] If quotation for current business date is missing, set SystemMsgInfo.processState.Status = Warning instead of Error
            quote.IsBusinessDateMissingWarning = true;
            if ((false == quote.IsLoaded) || (quote.QuoteValueCodeReturn != Cst.ErrLevel.SUCCESS))
            {                
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(quote.SystemMsgInfo.processState.Status);
                
                Logger.Log(quote.SystemMsgInfo.ToLoggerData(0));

                // AL 20240725 [WI1008] Managed condition => don't throw exception
                //if (pDate < OTCmlHelper.GetDateSys(Cs))
                //{
                //    throw new SpheresException2(new ProcessState(ProcessStateTools.StatusErrorEnum, quote.QuoteValueCodeReturn));
                //}                
            }
            if (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS)
            {
                DataRow rowPosCollateral = _dsPosCol.DtPOSCOLLATERAL.Rows[0];
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal qty = Convert.ToDecimal(rowPosCollateral["QTY"]);
                decimal valorisation = qty * quote.QuoteValue;
                //PM 20150317 [POC] Ajout conversion en devise cotée
                //UpdateDsPosCollateralVal(pDate, qty, new Money(valorisation, idC));
                UpdateDsPosCollateralVal(pDate, qty, GetValoMoney(valorisation, idC));
            }
            // AL 20240725 [WI1008] Added return Cst.ErrLevel
            return quote.QuoteValueCodeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pSqlAssetEquity"></param>
        /// <param name="pSqlDebtSecurity"></param>
        /// FI 20151202 [21609] GLOP Il faut revoir cette méthod 
        // EG 20190716 [VCL : New FixedIncome] Upd (Set keyQuote.KeyQuoteAdditional)
        // AL 20240725 [WI1008] Added return Cst.ErrLevel
        private Cst.ErrLevel ValorizeDebtSecurity(DateTime pDate, SQL_AssetDebtSecurity pSqlDebtSecurity, DebtSecurityContainer pDebtSecurity)
        {        
            IProductBase productBase = new FpML.v44.Ird.Fra();
            ISecurity security = pDebtSecurity.DebtSecurity.Security;
            int idAsset = pSqlDebtSecurity.IdAsset;

            //Recherche des prix en cours par défaut sauf si les propriétés du titres stipulent recherche des prix en prix
            Cst.PriceQuoteUnits priceQuote = Cst.PriceQuoteUnits.ParValueDecimal;
            
            //Recherche du prix clean par défaut
            AssetMeasureEnum assetMeasure = AssetMeasureEnum.CleanPrice;
            if (security.QuoteRulesSpecified)
            {
                if (security.QuoteRules.QuoteUnitsSpecified)
                {
                    if (Enum.IsDefined(typeof(Cst.PriceQuoteUnits), security.QuoteRules.QuoteUnits.Value))
                    {
                        Cst.PriceQuoteUnits priceQuoteRulesUnit = (Cst.PriceQuoteUnits)Enum.Parse(typeof(Cst.PriceQuoteUnits), security.QuoteRules.QuoteUnits.Value);
                        switch (priceQuoteRulesUnit)
                        {
                            case Cst.PriceQuoteUnits.ParValueDecimal:
                                priceQuote = Cst.PriceQuoteUnits.ParValueDecimal;
                                break;
                            case Cst.PriceQuoteUnits.Rate:
                                priceQuote = Cst.PriceQuoteUnits.ParValueDecimal;
                                break;
                            case Cst.PriceQuoteUnits.Price:
                                priceQuote = Cst.PriceQuoteUnits.Price;
                                break;
                        }
                    }
                    if (security.QuoteRules.AccruedInterestIndicatorSpecified)
                    {
                        if (security.QuoteRules.AccruedInterestIndicator.BoolValue)
                            assetMeasure = AssetMeasureEnum.DirtyPrice;
                    }
                }
            }

            //Recherche du clean ou du dirty price
            KeyQuote keyQuote = new KeyQuote(Cs, pDate)
            {
                QuoteUnit = priceQuote,
                KeyQuoteAdditional = new KeyQuoteAdditional(assetMeasure)
            };
            SQL_Quote quote = new SQL_Quote(Cs, QuoteEnum.DEBTSECURITY, AvailabilityEnum.NA, productBase, keyQuote, idAsset);
            // AL 20240725 [WI1008] If quotation for current business date is missing, set SystemMsgInfo.processState.Status = Warning instead of Error
            quote.IsBusinessDateMissingWarning = true;
            bool isPriceExist = (quote.IsLoaded) & (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);
            if (false == isPriceExist)
            {
                if (assetMeasure == AssetMeasureEnum.CleanPrice)
                    assetMeasure = AssetMeasureEnum.DirtyPrice;
                else if (assetMeasure == AssetMeasureEnum.DirtyPrice)
                    assetMeasure = AssetMeasureEnum.CleanPrice;

                keyQuote.KeyQuoteAdditional = new KeyQuoteAdditional(assetMeasure);
                SQL_Quote quote2 = new SQL_Quote(Cs, QuoteEnum.DEBTSECURITY, AvailabilityEnum.NA, productBase, keyQuote, idAsset);
                // AL 20240725 [WI1008] If quotation for current business date is missing, set SystemMsgInfo.processState.Status = Warning instead of Error
                quote.IsBusinessDateMissingWarning = true;
                isPriceExist = (quote2.IsLoaded) & (quote2.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);
                if (isPriceExist)
                    quote = quote2;
            }
            if (false == isPriceExist)
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                ProcessState.SetErrorWarning(quote.SystemMsgInfo.processState.Status);
                Logger.Log(quote.SystemMsgInfo.ToLoggerData(0));

                // AL 20240725 [WI1008] Managed condition => don't throw exception
                //if (pDate < OTCmlHelper.GetDateSys(Cs))
                //{
                //  throw new SpheresException2(new ProcessState(ProcessStateTools.StatusErrorEnum, quote.QuoteValueCodeReturn));                    
                //}
                // AL 20240725 [WI1008] Added return Cst.ErrLevel
                return quote.QuoteValueCodeReturn;
            }

            SQL_Quote quoteAccruedInterest = null;
            if (isPriceExist)
            {
                if (quote.AssetMeasure == AssetMeasureEnum.CleanPrice.ToString())
                {
                    //Recherche du coupon Couru
                    KeyQuote keyQuoteAccruedInterest = new KeyQuote(Cs, pDate)
                    {
                        KeyQuoteAdditional = new KeyQuoteAdditional(AssetMeasureEnum.AccruedInterest)
                    };
                    quoteAccruedInterest = new SQL_Quote(Cs, QuoteEnum.DEBTSECURITY, AvailabilityEnum.NA, productBase, keyQuoteAccruedInterest, idAsset);
                    // AL 20240725 [WI1008] If quotation for current business date is missing, set SystemMsgInfo.processState.Status = Warning instead of Error
                    quote.IsBusinessDateMissingWarning = true;
                    isPriceExist = (quoteAccruedInterest.IsLoaded) & (quoteAccruedInterest.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);
                    if (false == isPriceExist)
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        ProcessState.SetErrorWarning(quoteAccruedInterest.SystemMsgInfo.processState.Status);
                        
                        Logger.Log(quote.SystemMsgInfo.ToLoggerData(0));

                        // AL 20240725 [WI1008] Managed condition => don't throw exception
                        //if (pDate < OTCmlHelper.GetDateSys(Cs))
                        //{
                        //  throw new SpheresException2(new ProcessState(ProcessStateTools.StatusErrorEnum, quoteAccruedInterest.QuoteValueCodeReturn));                            
                        //}
                        // AL 20240725 [WI1008] Added return Cst.ErrLevel
                        return quote.QuoteValueCodeReturn;
                    }
                }
                else if (quote.AssetMeasure == AssetMeasureEnum.DirtyPrice.ToString())
                {
                    isPriceExist = true;
                }
            }

            if (isPriceExist)
            {
                DataRow rowPosCollateral = _dsPosCol.DtPOSCOLLATERAL.Rows[0];
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal qty = Convert.ToDecimal(rowPosCollateral["QTY"]);
                Decimal valorisation = decimal.Zero;
                //
                if (PriceQuoteUnitsTools.IsPriceInParValueDecimal(priceQuote))

                {
                    if (quote.AssetMeasure == AssetMeasureEnum.DirtyPrice.ToString())
                    {
                        //Ex de valeur pour le cours saisi 101.5
                        valorisation = pDebtSecurity.GetNominal(productBase).Amount.DecValue * qty * quote.QuoteValue / 100;
                    }
                    else if (quote.AssetMeasure == AssetMeasureEnum.CleanPrice.ToString())
                    {
                        //calcul du coupon couru
                        Nullable<Cst.PriceQuoteUnits> accruedInterest = null;
                        if (StrFunc.IsFilled(quoteAccruedInterest.QuoteUnit))
                        {
                            accruedInterest = (Cst.PriceQuoteUnits)Enum.Parse(typeof(Cst.PriceQuoteUnits), quoteAccruedInterest.QuoteUnit);
                            if ((accruedInterest.Value != Cst.PriceQuoteUnits.Price) & (accruedInterest.Value != Cst.PriceQuoteUnits.Rate))
                                throw new Exception(StrFunc.AppendFormat("PriceQuoteUnits[{0}] is not implemented", accruedInterest.Value));
                        }
                        //
                        Decimal accruedInterestAmount = Decimal.Zero;
                        if ((false == accruedInterest.HasValue) || (accruedInterest.Value == Cst.PriceQuoteUnits.Rate))
                        {
                            //Coupon couru exprimé en taux
                            //Ex de valeur pour le tx saisi 0.05 (pour 5%)
                            accruedInterestAmount = qty * quoteAccruedInterest.QuoteValue * pDebtSecurity.GetNominal(productBase).Amount.DecValue;
                        }
                        else if (accruedInterest.Value == Cst.PriceQuoteUnits.Price)
                        {
                            //Coupon couru exprimé en montant
                            accruedInterestAmount = qty * quoteAccruedInterest.QuoteValue;
                        }
                        valorisation = pDebtSecurity.GetNominal(productBase).Amount.DecValue * qty * quote.QuoteValue / 100 + accruedInterestAmount;
                    }
                }
                else if (PriceQuoteUnitsTools.IsPriceInPrice(priceQuote))
                {
                    valorisation = qty * quote.QuoteValue;
                }
                //    
                string idC = pSqlDebtSecurity.IdC;
                //PM 20150317 [POC] Ajout conversion en devise cotée
                //UpdateDsPosCollateralVal(pDate, qty, new Money(valorisation, idC));
                UpdateDsPosCollateralVal(pDate, qty, GetValoMoney(valorisation, idC));
                // AL 20240725 [WI1008] Added return Cst.ErrLevel
                return Cst.ErrLevel.SUCCESS;
            }
            // AL 20240725 [WI1008] Added return Cst.ErrLevel.
            // Setted QUOTENOTFOUND by default, but it should never reach this point whether isPriceExist is true or false
            return Cst.ErrLevel.QUOTENOTFOUND;
        }

        /// <summary>
        /// Mise à jour du dataset PosCollateral => Add/Update une valorisation calculée
        /// </summary>
        /// <param name="pDate">Date de valo</param>
        /// <param name="pQty">Qté considérée pour obtenir cette valo</param>
        /// <param name="pMtValo">Montant de la valo</param>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private void UpdateDsPosCollateralVal(DateTime pDate, Nullable<decimal> pQty, Money pMtValo)
        {
            // FI 20200820 [25468] dates systemes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(Cs);
            DataRow rowPosCollateral = _dsPosCol.DtPOSCOLLATERAL.Rows[0];
            bool isNewRow = false;
            DataRow row = _dsPosCol.GetPosCollateralValRow(pDate);
            if (null == row)
            {
                isNewRow = true;
                row = _dsPosCol.DtPOSCOLLATERALVAL.NewRow();
                _dsPosCol.DtPOSCOLLATERALVAL.Rows.Add(row);
            }

            row["IDPOSCOLLATERAL"] = rowPosCollateral["IDPOSCOLLATERAL"];
            row["DTBUSINESS"] = pDate;
            if (pQty.HasValue)
                row["QTY"] = pQty;
            row["VALORISATION"] = pMtValo.Amount.DecValue;
            row["IDC"] = pMtValo.Currency;
            row["VALORISATIONSYS"] = pMtValo.Amount.DecValue;
            row["IDCSYS"] = pMtValo.Currency;
            row["IDSTACTIVATION"] = Cst.StatusActivation.REGULAR.ToString();
            row["SOURCE"] = "EuroFinanceSystems";
            if (isNewRow)
            {
                row["DTINS"] = dtSys;
                row["IDAINS"] = Session.IdA;
            }
            else
            {
                row["DTUPD"] = dtSys;
                row["IDAUPD"] = Session.IdA;
            }
        }

        /// <summary>
        /// Mise à jour de la base de donnée 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        private void UpdateDatabase()
        {
            bool isError = false;
            IDbTransaction dbTransaction = DataHelper.BeginTran(Cs);
            try
            {
                _dsPosCol.ExecuteDataAdapterPOSCOLLATERALVAL(Cs, dbTransaction);
                //PL 20151229 Use DataHelper.CommitTran()
                //dbTransaction. Commit();
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception)
            {
                isError = true;
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                {
                    if (isError)
                    {
                        //PL 20151229 Use DataHelper.RollbackTran()
                        //dbTransaction. Rollback();
                        DataHelper.RollbackTran(dbTransaction);
                    }
                    dbTransaction.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pSqlAssetCash"></param>
        private void ValorizeCash(DateTime pDate, SQL_AssetCash pSqlAssetCash)
        {
            DataRow rowPosCollateral = _dsPosCol.DtPOSCOLLATERAL.Rows[0];
            Decimal amount = Convert.ToDecimal(rowPosCollateral["QTY"]);
            //PM 20150317 [POC] Ajout conversion en devise cotée
            //UpdateDsPosCollateralVal(pDate, null, new Money(amount, pSqlAssetCash.IdC));
            UpdateDsPosCollateralVal(pDate, null, GetValoMoney(amount, pSqlAssetCash.IdC));
        }

        /// <summary>
        /// Construit un Money en devise cotée
        /// </summary>
        /// <param name="pAmount">Montant</param>
        /// <param name="pCurrency">Devise</param>
        /// <returns>Un Money en devise cotée</returns>
        //PM 20150317 [POC] Ajout méthode GetValoMoney
        // EG 20180307 [23769] Gestion dbTransaction
        private Money GetValoMoney(decimal pAmount, string pCurrency)
        {
            decimal amount = pAmount;
            Tools.GetQuotedCurrency(CSTools.SetCacheOn(Cs), null, SQL_Currency.IDType.IdC, pCurrency, out string quotedCurrency, out int? quotedCurrencyFactor);
            if (StrFunc.IsEmpty(quotedCurrency))
            {
                quotedCurrency = pCurrency;
            }
            if ((quotedCurrencyFactor.HasValue) && (quotedCurrencyFactor.Value != 0))
            {
                amount /= quotedCurrencyFactor.Value;
            }
            return new Money(amount, quotedCurrency);
        }
        #endregion
    }
}

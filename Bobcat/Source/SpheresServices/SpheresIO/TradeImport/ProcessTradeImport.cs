#region using
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.GUI.CCI;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
using EfsML.Business;
using EfsML.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
#endregion

namespace EFS.SpheresIO.Trade
{

    /// <summary>
    /// Class pour importation des trades
    /// </summary>
    internal class ProcessTradeImport : ProcessTradeImportBase
    {
        #region Members

        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeCaptureGen TradeCaptureGen
        {
            get { return (TradeCaptureGen)_captureGen; }
        }
        /// <summary>
        /// Obtient le trade qui sera importé
        /// </summary>
        public TradeInput TradeInput
        {
            get { return (TradeInput)CommonInput; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override string TradeKey
        {
            get { return "Trade"; }
        }
        // FI 20130730 [18847] Mise en commentaire
        ///// <summary>
        ///// 
        ///// </summary>
        //public override bool isActorSYSTEMAvailable
        //{
        //    get { return TradeInput.TradeStatus.IsStActivation_Missing; }
        //}
        /// <summary>
        ///  Obtient true (l'importation des trades exploite les données présentes dans PARTYTEMPLATE) 
        /// </summary>
        public override bool IsInitFromPartyTemplateAvailable
        {
            get { return true; }
        }

        /// <summary>
        ///  Obtient true (l'importation des trades exploite les données présentes dans CLEARINGTEMPLATE) 
        /// </summary>
        /// FI 20140815 [XXXXX] override  property
        public override bool IsInitFromClearingTemplateAvailable
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsFeeCalcAvailable
        {
            get { return true; }
        }

        /// <summary>
        /// Obtient true lorsque la génération de l'identifier doit faire appel à UP_GETID
        /// </summary>
        /// FI [18465] Génération d'un nouvel identifier si le paramètre identifier est non renseigné
        public override bool IsGetNewIdForIdentifier
        {
            get
            {
                bool ret = StrFunc.IsEmpty(GetParameter(TradeImportCst.identifier));
                return ret;
            }
        }

        /// <summary>
        /// Obtient true si action modification de frais
        /// </summary>
        /// FI 20160907 [21831] Add
        public override Boolean IsModeUpdateFeesOnly
        {
            get
            {
                return base.IsModeUpdate && GetParameter(TradeImportCst.updateMode) == "UpdateFeesOnly";
            }
        }

        /// <summary>
        /// Obtient true si action Miseà jour de trader      
        /// </summary>
        /// FI 20170824 [23339] Add
        public override Boolean IsModeUpdateTraderOnly
        {
            get
            {
                return base.IsModeUpdate && GetParameter(TradeImportCst.updateMode) == "UpdateTraderOnly";
            }
        }




        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeImport"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTask"></param>
        /// <param name="pRowIdentifier"></param>
        public ProcessTradeImport(TradeImport pTradeImport, IDbTransaction pDbTransaction, Task pTask)
            : base(pTradeImport, pDbTransaction, pTask)
        {

        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void InitializeCaptureGen()
        {
            _captureGen = new TradeCaptureGen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// FI 20131213 [19319] add logHeader in message
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, User pUser)
        {
            CheckTradeValidationRule check = new CheckTradeValidationRule(this.TradeCaptureGen.Input, pCaptureMode, pUser);
            check.ValidationRules(CSTools.SetCacheOn(pCS), pDbTransaction, CheckTradeValidationRule.CheckModeEnum.Warning);
            string msgValidationrules = check.GetConformityMsg();

            if (StrFunc.IsFilled(msgValidationrules))
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                // FI 20200706 [XXXXX] SetErrorWarning delegate
                //_task.process.ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);
                SetErrorWarning(ProcessStateTools.StatusWarningEnum);

                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, LogHeader + Cst.CrLf + msgValidationrules, 4));
            }
        }

        /// <summary>
        /// Contrôle la présence des paramètres nécessaires
        /// </summary>
        /// <exception cref="Exception si paramètre inexistant"></exception>
        protected override void CheckParameter()
        {
            if ((IsModeNew) && (StrFunc.IsEmpty(GetParameter(TradeImportCst.templateIdentifier))))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Template identifier not specified, parameter[scheme:{0}] is mandatory", TradeImportCst.templateIdentifier);
                FireException(sb.ToString());
            }
            else if ((IsModeUpdate || IsModeRemoveOnlyAll) && (StrFunc.IsEmpty(GetParameter(TradeImportCst.identifier))))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(TradeKey + " identifier not specified, parameter[scheme:{0}] is mandatory", TradeImportCst.identifier);
                FireException(sb.ToString());
            }
        }

        /// <summary>
        /// Injecte les frais dans le dataDocument lorsque le paramètre isApplyFeeCalculation = true,
        /// </summary>
        /// FI 20170824 [23339] Modify
        /// FI 20180319 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void ProcessSpecific()
        {
            // RD 20120322 / Intégration de trade "Incomplet"
            // Le calcul des frais se fait uniquement si le trade est "Normal"
            // FI 20170824 [23339] pas de calcul de frais si isModeUpdateTraderOnly
            if (TradeInput.TradeStatus.IsStActivation_Regular && false == IsModeUpdateTraderOnly)
            {
                FeeRequest feeRequest = null;

                #region Fee calculation
                //PL 20130718 FeeCalculation Project
                //if (BoolFunc.IsTrue(GetParameter(TradeImportCst.isApplyFeeCalculation)))
                string feeCalculation_Mode = GetParameter(TradeImportCst.feeCalculation);
                if (feeCalculation_Mode.StartsWith("Apply"))
                {
                    // FI 20180319 [XXXXX] Mise en place de TraceTime 
                    // feeRequest uniquement chargé si nécessaire cad si feeCalculation_Mode.StartsWith("Apply")
                    try
                    {
                        AppInstance.TraceManager.TraceTimeBegin("FeesCalculation", KeyTraceTime);
                        
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, "Fees calculation... (Mode: " + feeCalculation_Mode + ")", 4));

                        // Fees Calculation
                        // RD 20130205 [18389] Utiliser la bonne date de référence, selon s'il s'agit d'un ETD ou pas, d'une action sur trade ou pas.
                        feeRequest = new FeeRequest(CSTools.SetCacheOn(_task.Cs), null, TradeInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), this._captureMode);
                        try
                        {
                            //FI 20110214, Le calcul des frais automatique ne se produit que si TradeInput.isApplyFeeCalculation  
                            //Exemple: Il n'y a pas de calcul sur les ouvertures de positions (ETD)
                            if (TradeInput.IsApplyFeeCalculation(TradeInput.FeeTarget.trade))
                            {
                                string retMsg = Ressource.GetString("Msg_Fees_RevuePayment") + Cst.CrLf2;

                                #region Calcul des frais via le référentiel Conditions/Barèmes
                                //TradeCustomCaptureInfos ccis = (TradeCustomCaptureInfos)this.CommonInput.CustomCaptureInfos;

                                //FI 20120130 => Mise en cache pour accélerer les perfs
                                FeeProcessing fees = new FeeProcessing(feeRequest);
                                fees.Calc(CSTools.SetCacheOn(_task.Cs), null);

                                if (ArrFunc.IsFilled(fees.FeeResponse))
                                {
                                    string retMsgFee = TradeInput.SetFee(TradeInput.FeeTarget.trade, fees.FeeResponse);

                                    if (StrFunc.IsEmpty(retMsgFee))
                                    {
                                        retMsg += Ressource.GetString("Msg_Fees_NoFee");
                                    }
                                    else
                                    {
                                        retMsg += retMsgFee.TrimEnd(Cst.CrLf.ToCharArray());

                                        //PL 20130718 FeeCalculation Project
                                        if (feeCalculation_Mode.EndsWith("Substitute"))
                                        {
                                            retMsg += Cst.CrLf2 + Ressource.GetString("Msg_Fees_RevueSubstitutePayment") + Cst.CrLf2;

                                            string retMsgFeeSubstitute = TradeInput.SubstituteFeeSchedule();

                                            if (StrFunc.IsEmpty(retMsgFeeSubstitute))
                                            {
                                                retMsg += Ressource.GetString("Msg_Fees_NoFee");
                                            }
                                            else
                                            {
                                                retMsg += retMsgFeeSubstitute.TrimEnd(Cst.CrLf.ToCharArray());
                                            }
                                        }
                                    }
                                }

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, retMsg, 4));
                                #endregion
                            }
                            else
                            {
                                #region Pas de calcul de frais
                                string msg = "None fees calculation.";

                                if (TradeInput.IsETDandAllocation)
                                {
                                    ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)TradeInput.Product.Product);
                                    // EG 20130610 Add isTradeCAAdjusted
                                    if (etd.IsPositionOpening)
                                    {
                                        msg += " This Trade is a Position Opening.";
                                    }
                                    else if (etd.IsPositionOpening)
                                    {
                                        msg += " This Trade is adjusted due to a Corporate Action.";
                                    }
                                }

                                
                                Logger.Log(new LoggerData(LogLevelEnum.Debug, msg, 4));
                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            FireException("Error on fees calculation.", ex);
                        }


                        
                        Logger.Log(new LoggerData(LogLevelEnum.Debug, "Tax calculation", 4));

                        try
                        {
                            TradeInput.ProcessFeeTax(_task.Cs, null, TradeInput.FeeTarget.trade, feeRequest.DtReference);
                        }
                        catch (Exception ex)
                        {
                            FireException("Error on tax calculation.", ex);
                        }
                    }
                    finally
                    {
                        AppInstance.TraceManager.TraceTimeEnd("FeesCalculation", KeyTraceTime);
                   }
                }
                #endregion

                #region Funding and Margin calculation on ReturnSwap
                // FI 20140811 [XXXXX] Funding and Margin sont calculés en fin de traitement
                // Attention Cette méthode écrase toute valeur qui aurait pu être en entrée 
                if (TradeInput.Product.IsReturnSwap)
                {
                    try
                    {
                        // FI 20180319 [XXXXX] Mise en place de TraceTime 
                        AppInstance.TraceManager.TraceTimeBegin("FundingAndMarginCalculation", KeyTraceTime);
                        if (null == feeRequest)
                            feeRequest = new FeeRequest(CSTools.SetCacheOn(_task.Cs), null, TradeInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), this._captureMode);
                        string errMsg = null;
                        ProcessStateTools.StatusEnum errStatus = ProcessStateTools.StatusEnum.NA;
                        Exception exception = null;

                        TradeCaptureGen.SetFundingAndMargin(feeRequest, ref errMsg, ref errStatus, ref exception);
                    }
                    finally
                    {
                        AppInstance.TraceManager.TraceTimeEnd("FundingAndMarginCalculation", KeyTraceTime);
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180507 Analyse du code Correction [CA2214]
        protected override void SetCustomCaptureInfos(string pCS)
        {
            //IsGetDefaultonInitialize = false => Les defaults sont issus du webConfig, ici il n'y en a pas
            this.CommonInput.CustomCaptureInfos = new TradeCustomCaptureInfos(pCS, CommonInput, null, string.Empty, false);
            this.CommonInput.CustomCaptureInfos.InitializeCciContainer();
        }

        /// <summary>
        ///
        /// </summary>
        /// FI 20170718 [23326] Modify
        protected override void ShiftCcisClientId()
        {
            TradeCommonCustomCaptureInfos ccis = CommonInput.CustomCaptureInfos;
            if (ArrFunc.IsFilled(ccis))
            {
                if (CommonInput.Product.IsCapFloor)
                {
                    //Sur les capFloor, il existe qu'1 seul stream 
                    //Les ccis ne doivent donc pas porter de suffix numérique
                    //Exemple on veut maintenant capFloorStream_calculationPeriodDates_effectiveDate
                    //Avant on avait capFloorStream1_calculationPeriodDates_effectiveDate 
                    foreach (CustomCaptureInfo cci in ccis)
                    {
                        string clientIdPrefixToReplace = TradeCustomCaptureInfos.CCst.Prefix_capFloorStream + "1";
                        if (cci.ClientId_WithoutPrefix.StartsWith(clientIdPrefixToReplace))
                            cci.ClientId = "IMP" + TradeCustomCaptureInfos.CCst.Prefix_capFloorStream + cci.ClientId_WithoutPrefix.Substring(clientIdPrefixToReplace.Length);
                    }
                }
                else if (CommonInput.Product.IsFxLeg)
                {
                    //Sur les FxSingleLeg, il existe qu'1 seul leg
                    //Les ccis ne doivent donc pas porter de suffix numérique
                    //Exemple on veut maintenant fx_valueDate  
                    //Avant on avait fx1_valueDate  
                    foreach (CustomCaptureInfo cci in ccis)
                    {
                        string clientIdPrefixToReplace = TradeCustomCaptureInfos.CCst.Prefix_fx + "1";
                        if (cci.ClientId_WithoutPrefix.StartsWith(clientIdPrefixToReplace))
                            cci.ClientId = "IMP" + TradeCustomCaptureInfos.CCst.Prefix_fx + cci.ClientId_WithoutPrefix.Substring(clientIdPrefixToReplace.Length);
                    }
                }
                else if (CommonInput.Product.IsCommoditySpot) // FI 20170718 [23326] add if CommoditySpot
                {
                    string commodityPhysicalLeg = string.Empty;
                    string periods_startTime = string.Empty;
                    string periods_endTime = string.Empty;

                    ICommoditySpot _commoditySpot = (ICommoditySpot)CommonInput.Product.Product;
                    if (_commoditySpot.IsElectricity)
                    {
                        commodityPhysicalLeg = "electricityPhysicalLeg";
                        periods_startTime = "settlementPeriods_startTime";
                        periods_endTime = periods_startTime.Replace("start", "end");
                    }
                    else if (_commoditySpot.IsGas)
                    {
                        commodityPhysicalLeg = "gasPhysicalLeg";
                        periods_startTime = "deliveryPeriods_supplyStartTime";
                        periods_endTime = periods_startTime.Replace("Start", "End");
                    }
                    else
                        FireException(new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", _commoditySpot.PhysicalLeg.ToString())));

                    foreach (CustomCaptureInfo cci in ccis)
                    {
                        if (cci.ClientId.Contains("{commodityPhysicalLeg}"))
                            cci.ClientId = cci.ClientId.Replace("{commodityPhysicalLeg}", commodityPhysicalLeg);
                        if (cci.ClientId.Contains("{settlementPeriods_startTime}"))
                            cci.ClientId = cci.ClientId.Replace("{settlementPeriods_startTime}", periods_startTime);
                        if (cci.ClientId.Contains("{settlementPeriods_endTime}"))
                            cci.ClientId = cci.ClientId.Replace("{settlementPeriods_endTime}", periods_endTime);
                    }

                    foreach (CustomCaptureInfo cci in this._importCustomCaptureInfos)
                    {
                        if (cci.ClientId.Contains("{commodityPhysicalLeg}"))
                            cci.ClientId = cci.ClientId.Replace("{commodityPhysicalLeg}", commodityPhysicalLeg);
                        if (cci.ClientId.Contains("{settlementPeriods_startTime}"))
                            cci.ClientId = cci.ClientId.Replace("{settlementPeriods_startTime}", periods_startTime);
                        if (cci.ClientId.Contains("{settlementPeriods_endTime}"))
                            cci.ClientId = cci.ClientId.Replace("{settlementPeriods_endTime}", periods_endTime);
                    }
                }
                else if (CommonInput.Product.IsReturnSwap)
                {
                    // FI 20180301 [23814] remplacement de tradeHeader_clearedDate par tradeHeader_market1_clearedDate 
                    // Depuis la 7.0, le cci qui contient la clearedDate est tradeHeader_market1_clearedDate 
                    // La modification suivante permet de conserver une compatibilité ancendante
                    CustomCaptureInfo cci = this._importCustomCaptureInfos["tradeHeader_clearedDate"];
                    if (null != cci)
                        cci.ClientId = "IMP" + "tradeHeader_market1_clearedDate";
                }
            }
        }

        /// <summary>
        /// Alimente le document avec les valeurs des ccis de l'import
        /// </summary>
        /// FI 20160907 [21831] Modify
        /// FI 20171103 [23326] Modify
        /// FI 20180319 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void DumpToDocument()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, "Dump data (Dump cci to DataDocument)", 4));

            // FI 20180319 [XXXXX] Mise en place de TraceTime 
            AppInstance.TraceManager.TraceTimeBegin("DumpToDocument", KeyTraceTime);

            try
            {
                if (IsModeUpdateFeesOnly)
                {
                    // FI 20171103 [23326] utilisation d'instruction Link 
                    TradeCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;
                    if (!(ccis.CciTrade is CciTrade cciTrade))
                        throw new NullReferenceException("cciTrade is null");

                    if (ArrFunc.IsFilled(cciTrade.cciOtherPartyPayment))
                    {
                        // FI 20201105 [25554] add cciOpp
                        IEnumerable<CustomCaptureInfo> cciOpp = ccis.Cast<CustomCaptureInfo>().Where(item =>
                                        (cciTrade.cciOtherPartyPayment.Where(x => x.IsCciOfContainer(item.ClientId_WithoutPrefix)).Count() > 0));

                        foreach (CustomCaptureInfo cci in cciOpp)
                        {
                            SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, ccis.CciTradeCommon.IsClientId_PayerOrReceiver(cci));
                            ccis.Dump_ToDocument(0);
                            cci.IsLastInputByUser = false;
                        }
                        // FI 20201105 [25554] call CheckCciCurrency
                        CheckCciCurrency(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cciOpp);
                    }

                    // FI 20171103 [23326] Mise en commenatire de l'ancien code
                    // FI 20160907 [21831] cas particulier si isModeUpdateOnlyFee
                    // Mise à jour des ccis se rapportant à des OPP uniquement
                    //foreach (CustomCaptureInfo cci in TradeInput.CustomCaptureInfo)
                    //{

                    //    int index = ((CciTrade)ccis.CciTrade).cciOtherPartyPayment[0].IsCciOfContainer(cci.ClientId_WithoutPrefix)  

                    //        .GetIndexOtherPartyPayment(cci.ClientId_WithoutPrefix);
                    //    if (((CciTrade)ccis.CciTrade).IsClientId_OtherPartyPaymentPayerReceiver )
                    //    {
                    //        SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, ccis.CciTradeCommon.IsClientId_PayerOrReceiver(cci));
                    //        ccis.Dump_ToDocument(0);
                    //        cci.isLastInputByUser = false;
                    //    }
                    //}
                }
                else if (IsModeUpdateTraderOnly)
                {
                    // FI 20170824 [23339] 
                    // Mise à jour des ccis se rapportant à des traders côté dealer
                    // FI 2019
                    foreach (CustomCaptureInfo cci in TradeInput.CustomCaptureInfos)
                    {
                        bool isOk = false;
                        TradeCommonCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;
                        CciTradeParty[] cciParty = ccis.CciTradeCommon.cciParty;
                        if (ArrFunc.Count(cciParty[0].cciTrader) > 0 && cciParty[0].cciTrader[0].IsCci(CciTrader.CciEnum.identifier, cci))
                        {
                            isOk = true;
                        }
                        // FI 20190115 [24432] Alimentation du trader associé cciParty[0].cciBroker[0] (normalement l'entité) 
                        else if ((ArrFunc.Count(cciParty[0].cciBroker) > 0) &&
                                ArrFunc.Count(cciParty[0].cciBroker[0].cciTrader) > 0 && cciParty[0].cciBroker[0].cciTrader[0].IsCci(CciTrader.CciEnum.identifier, cci))
                        {
                            isOk = true;
                        }
                        if (isOk)
                        {
                            SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, ccis.CciTradeCommon.IsClientId_PayerOrReceiver(cci));
                            ccis.Dump_ToDocument(0);
                            cci.IsLastInputByUser = false;
                        }
                    }
                }
                else
                {
                    //Spheres® donne priorité à certains ccis 
                    // FI 20171103 [23326] call DumpFacilityToDocument en priorité
                    DumpFacilityToDocument();

                    //Aimentation du marché si ALLOC/(ETD ou ESE)
                    DumpMarketToDocument();
                    //Aimentation de l'underlying si ALLOC/RTS 
                    //Alimentation de l'asset pour pointer sur un marché (même comportement que pour alloc 
                    DumpUnderlyingAsset();

                    base.DumpToDocument();
                }
            }
            catch (Exception ex)
            {
                FireException("Error on dump ccis to Datadocument.", ex);
            }
            finally
            {
                AppInstance.TraceManager.TraceTimeEnd("DumpToDocument", KeyTraceTime);
            }
        }

        /// <summary>
        ///  <para>○ Alimentation des ccis de la saisie liés aux parties à partir ccis de l'import</para>
        ///  <para>○ Alimentation du datadocument dans la foulée</para>
        /// </summary>
        /// FI 20140815 [XXXXX] Add method
        /// FI 20160929 [22507] Modify
        /// FI 20170116 [21916] Modify
        /// FI 20170221 [XXXXX] Modify
        protected override void DumpPartyToDocument()
        {
            base.DumpPartyToDocument();

            /*  FI 20170116 [21916] Mise en commentaire du ce code particulier appliqué sur RTS et qui n'a pas lieu d'être  
            // cas particulier sur les RTS 
            // si isApplyClearingTemplate, cela signifie que party2 est non reseignée le cci de l'import
            // La méthode DumpPartyToDocument a donc écrasé les initialisations issues de CEARINGTEMPLATE
            // Elles sont appliquées de nouveaux
            Boolean isApplyClearingTemplate = BoolFunc.IsTrue(GetParameter(TradeImportCst.isApplyClearingTemplate));
            if (isApplyClearingTemplate && TradeInput.IsAllocation && TradeInput.SQLProduct.IsRTS)
            {
                TradeCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;
                ccis.CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                ccis.Dump_ToDocument(0);
            }
            */
            
            // FI 20160929 [22507] Lecture de PartyTemplate pour Rechercher le Dealer
            Boolean isApplyReverClearingTemplate = BoolFunc.IsTrue(GetParameter(TradeImportCst.isApplyReverseClearingTemplate));
            if (isApplyReverClearingTemplate && TradeInput.IsAllocation)
            {
                RptSideProductContainer product = TradeInput.Product.RptSide(CSTools.SetCacheOn(_task.Cs), TradeInput.IsAllocation);  
                FixML.Interface.IFixParty dealer = product.GetDealer();
                // On a le dealer SYSTEM lorsque le book est inconnu ou non renseigné et si le paramètre "Dealer incorrect" = Autorisé
                // FI 20170221 [XXXXX] dealer.PartyId.href = {unknown}
                if ((false == dealer.PartyIdSpecified) || (StrFunc.IsEmpty(dealer.PartyId.href) ||
                    (dealer.PartyId.href == TradeCommonCustomCaptureInfos.PartyUnknown) || (dealer.PartyId.href == "SYSTEM")))
                {
                    int idB = GetBookDealerFromClearingTemplate(_task.Cs, product);
                    if (idB > 0)
                    {
                        TradeCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;

                        SQL_Book sqlBook = new SQL_Book(CSTools.SetCacheOn(_task.Cs), idB);
                        sqlBook.LoadTable(new string[] { "IDENTIFIER" });

                        ccis.CciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.actor).NewValue = string.Empty;
                        ccis.Dump_ToDocument(0);

                        ccis.CciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.book).NewValue = sqlBook.Identifier;
                        ccis.Dump_ToDocument(0);
                    }
                }
            }
        }

        /// <summary>
        /// Affecte le Cci {pCci} de la saisie à partir du Cci de l'import 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCci">Représente le cci de la saisie en cours</param>
        /// <param name="pCciIsPayerOrReceiver"></param>
        /// FI 20171103 [23326] override méthode 
        protected override void SetCciFromCciImport(string pCS, IDbTransaction pDbTransaction, CustomCaptureInfo pCci, Boolean pCciIsPayerOrReceiver)
        {
            TradeCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;
            CciTrade cciTrade = ((CciTrade)ccis.CciTrade);
            
            CustomCaptureInfoDynamicData cciImport = (CustomCaptureInfoDynamicData)_importCustomCaptureInfos[pCci.ClientId_WithoutPrefix];
            if (null != cciImport)
            {
                if ((cciTrade.cciOtherPartyPayment.Where(x => x.IsCci(CciPayment.CciEnum.date, pCci)).Count()) > 0) 
                {
                    switch (cciImport.NewValue.ToLower())
                    {
                        case "default":
                        case "nextbusinessdate":
                            // Cas particulier de l'importation de frais où l'on souhaite conserver la date calculer par défaut par Spheres.
                            // ex. Pour les ETD: La date de paiement des frais est égale à la date de compensation + 1JO sur le BC du Market du DC
                            //     Voir PaymentDateInitialize() dans CciPayment. 
                            //    
                            break;
                        case "business":
                            cciImport.NewValue = DtFuncML.DateTimeToStringDateISO(TradeInput.DataDocument.CurrentProduct.GetBusinessDate2());
                            break;
                        case "transactdate":
                            cciImport.NewValue = DtFuncML.DateTimeToStringDateISO(TradeInput.DataDocument.TradeDate);
                            break;
                        // EG 20190327 [MIGRATION VCL] Add Default 
                        default:

                            base.SetCciFromCciImport(pCS, pDbTransaction, pCci, pCciIsPayerOrReceiver);
                            break;
                    }
                }
                else
                {
                    base.SetCciFromCciImport(pCS, pDbTransaction, pCci, pCciIsPayerOrReceiver);
                }
            }
        }
        



        /// <summary>
        /// Alimente le marché sur les ALLOC ETD, ESE et COMS
        /// </summary>
        /// FI 20140815 [XXXXX] Add method
        /// FI 20161214 [21916] Modify
        private void DumpMarketToDocument()
        {
            try
            {
                // FI 20161214 [21916] gestion de IsCOMD
                if ((TradeInput.SQLProduct.IsLSD || TradeInput.SQLProduct.IsESE || TradeInput.SQLProduct.IsCOMS) && TradeInput.IsAllocation)
                {
                    TradeCommonCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;

                    string clientId = string.Empty;
                    CustomCaptureInfo cci = null;
                    if ((TradeInput.SQLProduct.IsLSD || TradeInput.SQLProduct.IsESE))
                    {
                        CciProductExchangeTradedBase cciProduct = (CciProductExchangeTradedBase)(ccis.CciTradeCommon.cciProduct);
                        cci = cciProduct.CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.Exch);
                        clientId = cciProduct.CciFixTradeCaptureReport.CciFixInstrument.CciClientId(CciFixInstrument.CciEnum.Exch);
                    }
                    else if (TradeInput.SQLProduct.IsCOMS)
                    {
                        CciProductCommoditySpot cciProduct = (CciProductCommoditySpot)(ccis.CciTradeCommon.cciProduct);
                        cci = cciProduct.Cci(cciProduct.ExchangeEnumValue);
                        clientId = cciProduct.CciClientId(cciProduct.ExchangeEnumValue);
                    }
                    else
                    {
                        FireException(new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented", TradeInput.SQLProduct.Identifier)));
                    }

                    if (null == cci)
                    {
                        FireException(StrFunc.AppendFormat("customCaptureInfo (cliendId:{0}) does not exist.", clientId));
                    }

                    // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                    SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                    ccis.Dump_ToDocument(0);
                    cci.IsLastInputByUser = false;
                }
            }
            catch (Exception ex)
            {
                FireException(
                    StrFunc.AppendFormat("Eror on dump cci {0} to Datadocument", "Exch"), ex);
            }
        }

        /// <summary>
        /// Alimente l'asset sur les ALLOC RTS
        /// </summary>
        /// FI 20140815 [XXXXX] Add method
        private void DumpUnderlyingAsset()
        {
            try
            {
                if ((TradeInput.SQLProduct.IsRTS && TradeInput.IsAllocation))
                {
                    TradeCommonCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;

                    CciProductReturnSwap cciProduct = (CciProductReturnSwap)(ccis.CciTradeCommon.cciProduct);
                    if (false == cciProduct.ReturnLegLength > 0)
                        FireException("No return leg found");

                    CustomCaptureInfo cci = cciProduct.CciReturnSwapReturnLeg[0].Cci(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset);
                    if (null == cci)
                    {
                        FireException(
                            StrFunc.AppendFormat("customCaptureInfo (cliendId:{0}) does not exist",
                            cciProduct.CciReturnSwapReturnLeg[0].CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset)));
                    }

                    // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                    SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                    ccis.Dump_ToDocument(0);
                    cci.IsLastInputByUser = false;
                }
            }
            catch (Exception ex)
            {
                FireException(
                    StrFunc.AppendFormat("Eror on dump cci {0} to Datadocument.", "returnSwap_returnLeg1_underlyer_underlyingAsset"), ex);
            }
        }

        /// <summary>
        ///  Alimentation de la plateforme
        /// </summary>
        /// FI 20171103 [23326] Add
        private void DumpFacilityToDocument()
        {
            try
            {
                TradeCommonCustomCaptureInfos ccis = TradeInput.CustomCaptureInfos;
                CciTrade cciTrade = (CciTrade)ccis.CciTradeCommon;

                // RD 20200511 [25326] Dump Facility only if Cci market is specified for product other than ETD. 
                bool isToDump = TradeInput.Product.IsExchangeTradedDerivative;

                if (isToDump == false)
                {
                    CciMarketParty cciMarketContainer =  new CciMarketParty(cciTrade, 1, "tradeHeader_") ;
                    isToDump = ccis.Contains(cciMarketContainer.CciClientId(CciMarketParty.CciEnum.identifier));                    
                }

                if (isToDump)
                {
                    if (ArrFunc.Count(cciTrade.cciMarket) == 0)
                        FireException("Trade without facility.");

                    CustomCaptureInfo cci = cciTrade.cciMarket[0].Cci(CciMarketParty.CciEnum.identifier);
                    if (null == cci)
                    {
                        string clientId = cciTrade.cciMarket[0].CciClientId(CciMarketParty.CciEnum.identifier);
                        FireException(StrFunc.AppendFormat("customCaptureInfo (cliendId:{0}) does not exist.", clientId));
                    }

                    // FI 20200429 [XXXXX] false sur pCciIsPayerOrReceiver
                    SetCciFromCciImport(CSTools.SetCacheOn(_task.Cs), _dbTransaction, cci, false);
                    ccis.Dump_ToDocument(0);
                    cci.IsLastInputByUser = false;
                }

            }
            catch (Exception ex)
            {
                FireException(
                    StrFunc.AppendFormat("Eror on dump cci {0} to Datadocument.", "Facility"), ex);
            }
        }


        /// <summary>
        /// Génération d'un Message Queue à destination de TradeActionGen pour génération des évènements de frais
        /// </summary>
        /// FI 20160907 [21831] Add
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void SendMQueueFeesEventsGen()
        {
            // FI 20201105 [XXXXX] Ne pas faire appel à la génération des évènements si le trade est incomplet
            if ((IdT > 0) && (!CaptureGen.IsInputIncompleteAllow(CaptureMode)))
            {

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, "Sending request for fees events generation", 4));

                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString = _task.Cs,
                    id = TradeInput.Identification.OTCmlId,
                    identifier = TradeInput.Identification.Identifier
                };
                TradeActionGenMQueue[] mQueue = new TradeActionGenMQueue[] { new TradeActionGenMQueue(mQueueAttributes) };

                TradeActionMQueue tradeActionMQueue = new TradeActionMQueue
                {
                    tradeActionCode = TradeActionCode.TradeActionCodeEnum.FeesEventGen
                };

                mQueue[0].header.requesterSpecified = (null != _task.Requester);
                mQueue[0].header.requester = _task.Requester;

                mQueue[0].item = new TradeActionMQueue[] { tradeActionMQueue };

                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    connectionString = _task.Cs,
                    Session = _task.Session,
                    process = mQueue[0].ProcessType,
                    mQueue = (MQueueBase[])mQueue,
                    sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.ACTIONGEN, _task.AppInstance)
                };

                _task.Process.Tracker.AddPostedSubMsg(1, _task.Process.Session);
                int idTRK_L = _task.Requester.idTRK;
                MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);
            }
        }


        /// <summary>
        ///  Recherche du book Dealer (Lecture du paramétrage présent dans CLEARINGTEMPLATE)
        /// </summary>
        /// FI 20160929 [22507] Add 
        /// FI 20170116 [21916] Modify (Static Method)
        private static int GetBookDealerFromClearingTemplate(string pCS, RptSideProductContainer pProduct)
        {
            int ret = 0;

            ClearingTemplates clearingTemplates = new ClearingTemplates();
            clearingTemplates.LoadModeReverse(CSTools.SetCacheOn(pCS), pProduct.DataDocument, pProduct, SQL_Table.ScanDataDtEnabledEnum.Yes);

            ClearingTemplate clearingTemplateFind = null;
            if (ArrFunc.IsFilled(clearingTemplates.clearingTemplate))
                clearingTemplateFind = clearingTemplates.clearingTemplate[0];

            if (null != clearingTemplateFind)
                ret = clearingTemplateFind.idParty;

            return ret;
        }

        #endregion
    }
}
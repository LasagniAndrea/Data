#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Process;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Classe chargée de sauvegarder un trade de marché
    /// </summary>
    public sealed partial class TradeCaptureGen : TradeCommonCaptureGen
    {
        #region Members
        private TradeInput m_Input;
        #endregion Members

        #region Accessors
        /// <summary>
        ///  Représente le trade
        /// </summary>
        public TradeInput Input
        {
            set { m_Input = value; }
            get { return m_Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20130204 [] propertie set utilisée pour l'importation 
        public override TradeCommonInput TradeCommonInput
        {
            get
            {
                return (TradeCommonInput)Input;
            }
            set
            {
                this.Input = (TradeInput)value;
            }
        }

        /// <summary>
        /// Obtient TRADE
        /// </summary>
        public override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.TRADE.ToString();
            }
        }
        #endregion accessors

        #region Constructor
        public TradeCaptureGen()
        {
            m_Input = new TradeInput();
        }
        #endregion Constructors

        #region Methods


        /// <summary>
        /// Contrôles spécifiques effectués sur le trade
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// <param name="pDtRefForDtEnabled"></param>
        protected override void CheckSpecific(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, DateTime pDtRefForDtEnabled)
        {
            if (TradeCommonInput.Product.IsStrategy)
            {
                StrategyContainer strategy = (StrategyContainer)TradeCommonInput.Product;
                ProductContainer[] product = strategy.GetSubProduct();
                for (int i = 0; i < ArrFunc.Count(product); i++)
                {
                    CheckSimpleProduct(CSTools.SetCacheOn(pCS), pDbTransaction, product[i], pDtRefForDtEnabled);
                }
            }
            else
            {
                CheckSimpleProduct(CSTools.SetCacheOn(pCS), pDbTransaction, TradeCommonInput.Product, pDtRefForDtEnabled);
            }

            base.CheckSpecific(pCS, pDbTransaction, pCaptureMode, pIdT, pDtRefForDtEnabled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSession"></param>
        ///<exception cref="TradeCommonCaptureGenException[LOCKPROCESS_ERROR]"></exception>
        public override void CheckValidationLock(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, AppSession pSession)
        {
            CheckTradeValidationLock chk = new CheckTradeValidationLock(pCS, pDbTransaction, m_Input, pCaptureMode, pSession);
            if (false == chk.ValidationLocks())
                throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, chk.GetLockMsg(), TradeCaptureGen.ErrorLevel.LOCKPROCESS_ERROR);
        }

        ///<summary>
        ///Contrôle des validationRules
        ///</summary>
        ///<param name="pCS"></param>
        ///<param name="pDbTransaction"></param>
        ///<param name="pCaptureMode"></param>
        ///<param name="pCheckMode"></param>
        ///<param name="pUser"></param>
        ///<exception cref="TradeCommonCaptureGenException[VALIDATIONRULE_ERROR] lorsque les règles bloquantes ne sont pas respectées"></exception>
        // EG 20171115 Upd Add CaptureSessionInfo
        public override void CheckValidationRule(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CheckTradeValidationRule.CheckModeEnum pCheckMode, User pUser)
        {

            // RD 20110222 Pour charger les Assets créés en Mode Transactionnel.
            CheckTradeValidationRule chk = new CheckTradeValidationRule(m_Input, pCaptureMode, pUser);

            if (false == chk.ValidationRules(CSTools.SetCacheOn(pCS), pDbTransaction, pCheckMode))
            {
                throw new TradeCommonCaptureGenException("CheckValidationRule", chk.GetConformityMsg(),
                    TradeCaptureGen.ErrorLevel.VALIDATIONRULE_ERROR);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSetNewCustomcapturesInfos"></param>
        /// <returns></returns>
        ///FI 20091130 [16769] appel de la méthode de la classe de base,add  pIsSetNewCustomcapturesInfos
        ///EG 20100401 Add new [SQL_TableWithID.IDType] parameter
        public override bool Load(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, Cst.Capture.ModeEnum pCaptureMode,
                            User pUser, string pSessionId, bool pIsSetNewCustomcapturesInfos)
        {
            bool isOk = base.Load(pCS, pDbTransaction, pId, pIdType, pCaptureMode, pUser, pSessionId, pIsSetNewCustomcapturesInfos);
            Input.SetFlagProvision(pCS);
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsGlobalTransaction"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pTemplateIdentifierUnderLying"></param>
        /// <param name="pIsCheckLicense"></param>
        /// <param name="oUnderlying">Liste des assets créés (null si aucun asset créé)</param>
        /// <exception cref="TradeCommonCaptureGenException :lorsque l'erreur est gérée"></exception>
        /// EG 20150415 [20513] BANCAPERTA
        /// FI 20160816 [22146] Modify
        /// FI 20170404 [23039] Modify  
        // EG 20180423 Analyse du code Correction [CA2200]
        protected override void SaveUnderlyingAsset(string pCS, IDbTransaction pDbTransaction, Boolean pIsGlobalTransaction, CaptureSessionInfo pCaptureSessionInfo, DateTime pDtSys,
            string pTemplateIdentifierUnderLying, Boolean pIsCheckLicense,
            out Pair<int, string>[] oUnderlying)
        {
            oUnderlying = null;

            Hashtable htUnderlyingAsset = new Hashtable();
            //
            if ((TradeCommonInput.Product.IsSecurityTransaction) || (TradeCommonInput.Product.IsBondOption))
            {
                #region save Underlying SecurityAsset
                ISecurityAsset[] securityAsset = TradeCommonInput.DataDocument.GetSecurityAsset();
                for (int i = 0; i < ArrFunc.Count(securityAsset); i++)
                {
                    if ((securityAsset[i].OTCmlId <= 0 && securityAsset[i].DebtSecuritySpecified))
                    {
                        #region Calc Description msg
                        string securityId = string.Empty;
                        string msgSecurityAssetDesc;
                        try
                        {
                            if (null != securityAsset[i].SecurityId)
                                securityId = securityAsset[i].SecurityId.Value;

                            if (StrFunc.IsFilled(securityId))
                                msgSecurityAssetDesc = securityId;
                            else
                                msgSecurityAssetDesc = StrFunc.AppendFormat("security asset number {0}", i.ToString());
                        }
                        catch (Exception) { throw; }
                        #endregion
                        //
                        #region Set issuer in securityAsset
                        try
                        {
                            // Ex en cas de saisie Full l'issuer ne sera pas renseigné
                            if ((null == securityAsset[i].Issuer) || ("{Issuer}" == securityAsset[i].Issuer.PartyId))
                            {
                                if (securityAsset[i].IssuerReferenceSpecified)
                                    securityAsset[i].Issuer = (IParty)TradeCommonInput.DataDocument.GetParty(securityAsset[i].IssuerReference.HRef);
                            }
                            else if (false == securityAsset[i].IssuerReferenceSpecified)
                            {
                                SecurityAssetContainer securityAssetcontainer = new SecurityAssetContainer(securityAsset[i]);
                                securityAssetcontainer.SetIssuer(TradeCommonInput.DataDocument);
                            }
                        }
                        catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error in setting issuer, security asset is {0}", msgSecurityAssetDesc), ex); }

                        #endregion
                        #region search Screen and Template
                        string templateIdentifier;
                        string screenName;
                        try
                        {
                            SearchInstrumentGUI searchInstrumentGUI = new SearchInstrumentGUI(((IProductBase)securityAsset[i].DebtSecurity).ProductType.OTCmlId);
                            StringData[] data = null;
                            //
                            if (StrFunc.IsFilled(pTemplateIdentifierUnderLying))
                                data = searchInstrumentGUI.GetDefaultFromTemplate(CSTools.SetCacheOn(pCS), pTemplateIdentifierUnderLying);
                            else
                                data = searchInstrumentGUI.GetDefault(CSTools.SetCacheOn(pCS), false);
                            //
                            if (ArrFunc.IsEmpty(data))
                                throw new Exception("screen or template not found");
                            //
                            screenName = ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value;
                            templateIdentifier = ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value;
                            //templateIdentifier n'est pas sérialisé dans le datadocument donc si l'on vient de la saisie Full  idTTemplateSpecified = false
                            if (securityAsset[i].IdTTemplateSpecified)
                                templateIdentifier = TradeRDBMSTools.GetTradeIdentifier(pCS, securityAsset[i].IdTTemplate);
                        }
                        catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error in searching screen and template, security asset is {0}", msgSecurityAssetDesc), ex); }
                        #endregion
                        //
                        #region Load template
                        DebtSecCaptureGen debtSecCaptureGen = new DebtSecCaptureGen();
                        try
                        {
                            try
                            {
                                if (false == debtSecCaptureGen.Load(pCS, pDbTransaction, templateIdentifier, SQL_TableWithID.IDType.Identifier, Cst.Capture.ModeEnum.New, pCaptureSessionInfo.user, pCaptureSessionInfo.session.SessionId, true))
                                    throw new Exception("Underlying Template is not found");
                            }
                            catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error in loading debtSecurity template {0}", templateIdentifier), ex); }
                        }
                        catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error in loading template, security asset is {0}", msgSecurityAssetDesc), ex); }

                        #endregion
                        #region Set SebtSecurity debtSecurityDataDocument from SecurityAsset
                        try
                        {
                            DebtSecInput debtSecInput = debtSecCaptureGen.Input;
                            int idIUnderlying = debtSecInput.Product.IdI;

                            IDebtSecurity debtSecurity = (IDebtSecurity)debtSecInput.Product.Product;
                            debtSecurity.Security = (ISecurity)ReflectionTools.Clone(securityAsset[i].DebtSecurity.Security, ReflectionTools.CloneStyle.CloneField);
                            debtSecurity.Stream = debtSecCaptureGen.Input.DataDocument.CurrentProduct.ProductBase.CreateDebtSecurityStreams(ArrFunc.Count(securityAsset[i].DebtSecurity.Stream));
                            for (int k = 0; k < ArrFunc.Count(debtSecurity.Stream); k++)
                                debtSecurity.Stream[k] = (IDebtSecurityStream)ReflectionTools.Clone(securityAsset[i].DebtSecurity.Stream[k], ReflectionTools.CloneStyle.CloneField);
                            //Suppression des parties existantes, ajout de issuer puis de l'acteur SYSTEM
                            //l'ordre est important pour l'affichage
                            debtSecInput.DataDocument.RemoveParty();
                            debtSecInput.DataDocument.CleanUpTradeHeader();
                            debtSecInput.DataDocument.AddParty(securityAsset[i].Issuer);
                            SQL_Actor sqlActorSystem = new SQL_Actor(CSTools.SetCacheOn(pCS), "SYSTEM");
                            if (sqlActorSystem.IsLoaded)
                                debtSecInput.DataDocument.AddParty(sqlActorSystem);//add SYSTEM s'il n'existe pas
                            else
                                throw new Exception("ACTOR SYSTEM is not found");
                            //
                            //debtSecInput.DataDocument.AddPartyTradeIndentifier(securityAsset[i].issuer.id);
                            debtSecInput.DataDocument.AddPartyTradeIndentifier(securityAsset[i].IssuerReference.HRef);
                            //
                            for (int j = 0; j < ArrFunc.Count(debtSecurity.Stream); j++)
                            {
                                //debtSecurity.stream[j].payerPartyReference.hRef = securityAsset[i].issuer.id;
                                debtSecurity.Stream[j].PayerPartyReference.HRef = securityAsset[i].IssuerReference.HRef;
                                debtSecurity.Stream[j].ReceiverPartyReference.HRef = "SYSTEM";
                            }
                            if (securityAsset[i].SecurityIssueDateSpecified)
                                debtSecInput.DataDocument.CurrentTrade.TradeHeader.TradeDate.Value = securityAsset[i].SecurityIssueDate.Value;
                            //
                            debtSecInput.TradeStatus.stEnvironment.NewSt = Cst.STATUSREGULAR;
                        }
                        catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error when cloning debtSecurity, security asset is {0}", msgSecurityAssetDesc), ex); }
                        #endregion
                        //
                        #region Record debtSecurity
                        int newIdTAsset = 0;
                        string newIdentifierAsset = string.Empty;
                        try
                        {
                            newIdentifierAsset = securityAsset[i].SecurityId.Value;
                            //
                            TradeRecordSettings recordSettings = new TradeRecordSettings();
                            if (securityAsset[i].SecurityNameSpecified)
                                recordSettings.displayName = securityAsset[i].SecurityName.Value;
                            if (securityAsset[i].SecurityDescriptionSpecified)
                                recordSettings.description = securityAsset[i].SecurityDescription.Value;
                            recordSettings.extLink = string.Empty;
                            recordSettings.idScreen = screenName;
                            recordSettings.isGetNewIdForIdentifier = false;
                            recordSettings.isCheckValidationRules = true;
                            // RD 20121031 ne pas vérifier la license pour les services pour des raisons de performances
                            recordSettings.isCheckLicense = pIsCheckLicense;
                            
                            // FI 20170404 [23039] gestion de underlying et trader
                            Pair<int, string>[] underlying = null;
                            Pair<int, string>[] trader = null;

                            //
                            //FI 20091114 [Log in CheckAndRecord]  add le try cath et alimentation du tracker 
                            TradeCommonCaptureGen.ErrorLevel lRet = ErrorLevel.SUCCESS;
                            TradeCommonCaptureGenException captureEx = null;
                            try
                            {
                                debtSecCaptureGen.CheckAndRecord(pCS, pDbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputDebtSec), Cst.Capture.ModeEnum.New,
                                    pCaptureSessionInfo, recordSettings, ref newIdentifierAsset, ref newIdTAsset,
                                    out underlying, out trader);
                            }
                            catch (TradeCommonCaptureGenException ex)
                            {
                                captureEx = ex;
                                lRet = captureEx.ErrLevel;
                            }
                            catch (Exception) { throw; }
                            

                            if (null != captureEx)
                                throw captureEx;
                        }
                        catch (TradeCommonCaptureGenException) { throw; } // RD 20100601                
                        catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error on recording, security asset is {0}", msgSecurityAssetDesc), ex); }
                        #endregion
                        //
                        #region Load security Asset recorded in Datadocument
                        try
                        {
                            CaptureTools.SetSecurityAssetInSecurityAsset(securityAsset[i], this.Input.DataDocument, pCS, pDbTransaction, newIdTAsset);
                            if (false == (securityAsset[i].OTCmlId > 0))
                                throw new Exception(StrFunc.AppendFormat("Error on loading debt security asset, {0} not found", newIdentifierAsset));
                        }
                        catch (Exception ex) { throw new Exception(StrFunc.AppendFormat("Error on loading new debt security {0}", newIdentifierAsset), ex); }
                        #endregion
                        //
                        htUnderlyingAsset.Add(newIdTAsset, newIdentifierAsset);
                    }
                }
                //
                Input.DataDocument.CurrentProduct.SetReceiverOnSecurityAsset();
                #endregion
            }
            else if (TradeCommonInput.Product.IsStrategy)
            {
                StrategyContainer strategy = (StrategyContainer)TradeCommonInput.Product;
                if ((strategy.IsHomogeneous) && strategy.MainProduct.IsExchangeTradedDerivative)
                {
                    foreach (ProductContainer product in ((StrategyContainer)TradeCommonInput.Product).GetSubProduct())
                    {
                        IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)product.Product;
                        //PL 20130515
                        SaveUnderlyingAssetETD(pCS, pDbTransaction, pIsGlobalTransaction, pCaptureSessionInfo, pDtSys, exchangeTradedDerivative,
                            exchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDate.DateValue,
                            ref htUnderlyingAsset);
                    }
                }
            }
            else if (TradeCommonInput.Product.IsExchangeTradedDerivative)
            {
                IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)TradeCommonInput.Product.Product;
                //PL 20130515
                // RD 20190912 [24946] Dans le cas d'absence de DtBusiness, utiliser DtTrade, sinon DateSys
                //SaveUnderlyingAssetETD(pCS, pDbTransaction, pIsGlobalTransaction, pCaptureSessionInfo, exchangeTradedDerivative,
                //    exchangeTradedDerivative.tradeCaptureReport.ClearingBusinessDate.DateValue,
                //    ref htUnderlyingAsset);

                DateTime dtAssetActivation = exchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDate.DateValue;
                if (exchangeTradedDerivative.TradeCaptureReport.ClearingBusinessDateSpecified == false)
                {
                    dtAssetActivation = exchangeTradedDerivative.TradeCaptureReport.TradeDate.DateValue;
                    if (exchangeTradedDerivative.TradeCaptureReport.TradeDateSpecified == false)
                        dtAssetActivation = OTCmlHelper.GetDateSys(pCS);
                }

                SaveUnderlyingAssetETD(pCS, pDbTransaction, pIsGlobalTransaction, pCaptureSessionInfo, pDtSys, exchangeTradedDerivative, dtAssetActivation, ref htUnderlyingAsset);
            }

            if (ArrFunc.IsFilled(htUnderlyingAsset.Keys))
            {
                // FI 20170404 [23039] Alimentation de oUnderlying à partir de htUnderlyingAsset
                List<Pair<int, string>> lstUnderlyingAsset = new List<Pair<int, string>>();
                foreach (int item in htUnderlyingAsset.Keys)
                    lstUnderlyingAsset.Add(new Pair<int, string>() { First = item, Second = htUnderlyingAsset[item].ToString() });

                oUnderlying = lstUnderlyingAsset.ToArray();
            }
        }

        /// <summary>
        /// Initialisation au préalable avant de rentrer dans le type de saisie (Mofication, Suppression, etc..)
        /// </summary>
        /// <param name="pInputUser"></param>
        /// <returns></returns
        /// EG 20150624 [21151] Add Test Input.IsESEandAllocation|Input.IsDSTandAllocation sur PositionTransfer
        /// EG 20171016 [23509] Upd
        /// EG 20201009 Changement de nom pour enum ClearFeeMode
        public override void InitBeforeCaptureMode(string pCS, IDbTransaction pDbTransaction, InputUser pInputUser, CaptureSessionInfo pSessionInfo)
        {
            //20110120 Supprime les fees calculés
            //20120621 [17919] Il n'y a plus de reset des frais en cas de modification
            //Cela ne convient pas à EPL et la R&D ne se souvient plus la raison de ce choix
            // => Reset des frais uniquement si Duplication ou transfert ou modification dans le contexte de l'import
            if ((Cst.Capture.IsModeDuplicateOrReflect(pInputUser.CaptureMode)) ||
               (Cst.Capture.IsModeUpdate(pInputUser.CaptureMode) && pSessionInfo.session.AppInstance.IsIO) ||
                (Cst.Capture.IsModePositionTransfer(pInputUser.CaptureMode)))
            {
                //Input.ResetFee(TradeInput.FeeTarget.trade, true, false);
                Input.ClearFee(TradeInput.FeeTarget.trade, TradeInput.ClearFeeMode.FromSchedule);
            }
            /// FI 20140206 [19564] Gestion des UIT
            /// FI 20140218 [19636] Purge des UTI en mode Modification (IsModeUpdateGen)
            /// FI 20140623 [20126] Plus de suppression des UTIs en modification, cet identifiant est conservé
            if ((Cst.Capture.IsModeDuplicateOrReflect(pInputUser.CaptureMode)) ||
               //(Cst.Capture.IsModeUpdateGen(pInputUser.CaptureMode)) ||
               (Cst.Capture.IsModePositionTransfer(pInputUser.CaptureMode)))
            {
                Input.ClearUTI();
            }

            // EG 20150624 [21151] New 
            if (Input.IsDSTandAllocation && Cst.Capture.IsModePositionTransfer(pInputUser.CaptureMode))
            {
                IDebtSecurityTransaction dst = (IDebtSecurityTransaction)Input.DataDocument.CurrentProduct.Product;
                // EG 20150920 [21374] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                dst.InitPositionTransfer(Input.positionTransfer.quantity.DecValue);
                //Input.DataDocument.currentTrade.tradeHeader.tradeDate.BusinessDate = Input.positionTransfer.date.DateValue;
                Input.DataDocument.CurrentTrade.TradeHeader.ClearedDate.DateValue = Input.positionTransfer.date.DateValue;
                Input.DataDocument.CurrentTrade.TradeHeader.ClearedDateSpecified = DtFunc.IsDateTimeFilled(Input.positionTransfer.date.DateValue);
            }

            // EG 20150624 [21151] Add Test Input.IsESEandAllocation and Use IExchangeTradedBase
            if (Input.IsETDandAllocation || Input.IsESEandAllocation)
            {
                if (Cst.Capture.IsModePositionTransfer(pInputUser.CaptureMode))
                {
                    //FI 20120703 [17982] etd est valorisé dans le if. Inutile de faire un cast couteux si Spheres® n'est pas dans le mode Transfert
                    // EG 20150624 Use IExchangeTradedBase
                    IExchangeTradedBase etb = (IExchangeTradedBase)Input.DataDocument.CurrentProduct.Product;
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    etb.InitPositionTransfer(Input.positionTransfer.quantity.DecValue, Input.positionTransfer.date.DateValue);
                    // FI 20180302 [23815] Alimentation de clearedDate
                    Input.DataDocument.CurrentTrade.TradeHeader.ClearedDate.DateValue = Input.positionTransfer.date.DateValue;
                    Input.DataDocument.CurrentTrade.TradeHeader.ClearedDateSpecified = DtFunc.IsDateTimeFilled(Input.positionTransfer.date.DateValue);
                }

                // FI 20120625 [17864] ResetStrategyLeg 
                if ((Cst.Capture.IsModeDuplicateOrReflect(pInputUser.CaptureMode)) ||
                    (Cst.Capture.IsModeUpdate(pInputUser.CaptureMode) && pSessionInfo.session.AppInstance.IsIO) ||
                    (Cst.Capture.IsModePositionTransfer(pInputUser.CaptureMode)))
                {
                    //FI 20120703 [17982] etd est valorisé dans le if. Inutile de faire un cast couteux si Spheres® n'est pas dans le mode qu'il faut (duplication,etc...)
                    // EG 20150624 Use IExchangeTradedBase
                    IExchangeTradedBase etb = (IExchangeTradedBase)Input.DataDocument.CurrentProduct.Product;
                    etb.TradeCaptureReport.ResetStrategyLeg();
                }
            }
            //
            base.InitBeforeCaptureMode(pCS, pDbTransaction, pInputUser, pSessionInfo);
        }

        /// <summary>
        /// Vérifie la cohérence des assets présents dans le trade
        /// </summary>
        /// <param name="pDbTransaction"></param>
        protected override void CheckUnderlyingAsset(string pCS, IDbTransaction pDbTransaction)
        {
            if (TradeCommonInput.Product.IsStrategy)
            {
                StrategyContainer strategy = (StrategyContainer)TradeCommonInput.Product;
                if ((strategy.IsHomogeneous) && strategy.MainProduct.IsExchangeTradedDerivative)
                {
                    foreach (ProductContainer product in ((StrategyContainer)TradeCommonInput.Product).GetSubProduct())
                    {
                        IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)product.Product;
                        CheckUnderlyingAssetETD(pCS, pDbTransaction, exchangeTradedDerivative);
                    }
                }
            }
            else if (TradeCommonInput.Product.IsExchangeTradedDerivative)
            {
                IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)TradeCommonInput.Product.Product;
                CheckUnderlyingAssetETD(pCS, pDbTransaction, exchangeTradedDerivative);
            }
        }

        /// <summary>
        /// Vérifie la cohérence des assets ETD présents sur dans le trade
        /// <para>Génère une exception si une incohérence bloquante est détectée</para>
        /// <para>Génère un warning si une incohérence non bloquante est détectée</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pExchangeTradedDerivative"></param>
        private void CheckUnderlyingAssetETD(string pCS, IDbTransaction pDbTransaction, IExchangeTradedDerivative pExchangeTradedDerivative)
        {
            string msgWarning = string.Empty;
            //
            ExchangeTradedDerivativeContainer exchangeTradedDerivative = new ExchangeTradedDerivativeContainer(pExchangeTradedDerivative, TradeCommonInput.DataDocument);
            FixInstrumentContainer fixInstrument = new FixInstrumentContainer(exchangeTradedDerivative.TradeCaptureReport.Instrument);
            //
            //Chargement de l'asset ETD
            //FI 20120717 mise en place du cache, mise en place d'une liste de colonne dans sql_AssetETD.LoadTable
            SQL_AssetETD sql_AssetETD = new SQL_AssetETD(CSTools.SetCacheOn(pCS, 1, null), SQL_TableWithID.IDType.Id, fixInstrument.SecurityId)
            {
                DbTransaction = pDbTransaction
            };
            bool isAssetLoad = sql_AssetETD.LoadTable(new string[] { "VW_ASSET_ETD_EXPANDED.IDENTIFIER", "ASSETCATEGORY", "CONTRACTIDENTIFIER", "IDDC_UNL", "CONTRACTIDENTIFIER", "DA_IDASSET", "IDASSET_UNL", "MATURITYDATE" });
            if (false == isAssetLoad)
            {
                //ceci ne doit pas arriver, c'est en place par précaution la ressource reste en anglais 
                throw new Exception(StrFunc.AppendFormat("Asset[Id:{0}] is not found", fixInstrument.SecurityId));
            }
            //
            bool isAssetCategorieSpecified = StrFunc.IsFilled(sql_AssetETD.DrvContract_AssetCategorie);
            //
            if (!isAssetCategorieSpecified)
                msgWarning += Ressource.GetString2("Msg_ETD_DerivativeContractWithoutUnderlying", sql_AssetETD.DrvContract_Identifier);
            //
            //Lorsqu'une catégorie de sous jacent est renseigné, Spheres® effectue des contrôles supplémentaires
            if (isAssetCategorieSpecified)
            {

                #region isAssetCategorieSpecified
                Cst.UnderlyingAsset contractUnderlyingAssetCategory = Cst.ConvertToUnderlyingAsset(sql_AssetETD.DrvContract_AssetCategorie);
                //FI 20110705 le paramétrage permet uniquement de saisir Future (ExchangeTradedContract et ExchangeTradedDerivative sont là par sécurité)
                bool isUnderlyingAssetFuture =
                    (contractUnderlyingAssetCategory == Cst.UnderlyingAsset.Future) ||
                    (contractUnderlyingAssetCategory == Cst.UnderlyingAsset.ExchangeTradedContract);
                int idAssetUnl;

                if (isUnderlyingAssetFuture)
                {
                    if (sql_AssetETD.DrvContract_IdDerivativeContractUnl > 0)
                    {
                        //FI 20120717 mise en place du cache,
                        SQL_DerivativeContract sqlDerivativeContract = new SQL_DerivativeContract(CSTools.SetCacheOn(pCS, 1, null), sql_AssetETD.DrvContract_IdDerivativeContractUnl);
                        if (false == sqlDerivativeContract.IsFound)
                        {
                            string msg = Ressource.GetString2("Msg_ETD_UnderlyingContratNotValid", sql_AssetETD.DrvContract_Identifier, sql_AssetETD.DrvContract_IdDerivativeContractUnl.ToString());
                            throw new TradeCommonCaptureGenException("CheckUnderlyingAssetETD", msg, ErrorLevel.CHECKUNDERLYING_ERROR);
                        }
                    }

                    idAssetUnl = sql_AssetETD.DrvAttrib_IdAssetUnl;
                }
                else
                {
                    idAssetUnl = sql_AssetETD.DrvContract_IdAssetUnl;
                }
                //                            
                if (idAssetUnl == 0)
                {
                    #region idAssetUnl not found
                    if (isUnderlyingAssetFuture)
                    {
                        //L'enregistrement du trade se poursuit, Spheres® affiche un message pour avertir qu'il n'existe pas de sous jacent
                        msgWarning = Ressource.GetString("Msg_ETD_UnderlyingFutureNotSpecified");
                    }
                    else
                    {
                        //FI 20120717 [17994] Affiche un message de warning si le sous jacent n'existe pas 
                        msgWarning = Ressource.GetString2("Msg_ETD_UnderlyingNotSpecified", sql_AssetETD.Identifier, contractUnderlyingAssetCategory.ToString());
                        //string msg = Ressource.GetString2("Msg_ETD_UnderlyingNotSpecified", sql_AssetETD.Identifier, contractUnderlyingAssetCategory.ToString());
                        //throw new TradeCommonCaptureGenException("CheckUnderlyingAssetETD", msg, ErrorLevel.CHECKUNDERLYING_ERROR);
                    }
                    #endregion
                }
                else
                {
                    SQL_AssetBase sql_assetUnderlying;

                    #region idAssetUnl founded
                    try
                    {
                        //FI 20140326 [19793] add CSTools.SetCacheOn
                        sql_assetUnderlying = AssetTools.NewSQLAsset(CSTools.SetCacheOn(pCS, 1, null), contractUnderlyingAssetCategory, idAssetUnl);
                    }
                    catch (NotImplementedException) { sql_assetUnderlying = null; }
                    //
                    if (null != sql_assetUnderlying)
                    {
                        // PM 20111208 : Déplacement de l'instruction ci-dessous à l'intérieur du "if ( null != ..."
                        sql_assetUnderlying.DbTransaction = pDbTransaction;

                        if (false == sql_assetUnderlying.IsFound)
                        {
                            string msg = Ressource.GetString2("Msg_ETD_UnderlyingAssetNotValid", sql_AssetETD.Identifier, idAssetUnl.ToString());
                            throw new TradeCommonCaptureGenException("CheckUnderlyingAssetETD", msg, ErrorLevel.CHECKUNDERLYING_ERROR);
                        }
                        //
                        if (isUnderlyingAssetFuture)
                        {
                            if (((SQL_AssetETD)sql_assetUnderlying).IdDerivativeContract != sql_AssetETD.DrvContract_IdDerivativeContractUnl)
                            {
                                string msg = Ressource.GetString2("Msg_ETD_UnderlyingAssetContractNotValid", sql_assetUnderlying.Identifier, sql_AssetETD.DrvContract_Identifier);
                                throw new TradeCommonCaptureGenException("CheckUnderlyingAssetETD", msg, ErrorLevel.CHECKUNDERLYING_ERROR);
                            }
                            //
                            //L'échéance de l'asset future doit être supérieur à l'échéance de l'asset option sur furure
                            DateTime dtMaturity = sql_AssetETD.Maturity_MaturityDate;
                            DateTime dtMaturityFut = ((SQL_AssetETD)sql_assetUnderlying).Maturity_MaturityDate;
                            if (DtFunc.IsDateTimeFilled(dtMaturityFut) && DtFunc.IsDateTimeFilled(dtMaturity) && dtMaturityFut < dtMaturity)
                            {
                                string msg = Ressource.GetString2("Msg_ETD_UnderlyingAssetMaturityNotValid",
                                    sql_assetUnderlying.Identifier, DtFunc.DateTimeToString(dtMaturityFut, DtFunc.FmtShortDate),
                                    sql_AssetETD.Identifier, DtFunc.DateTimeToString(dtMaturity, DtFunc.FmtShortDate));
                                //    
                                throw new TradeCommonCaptureGenException("CheckUnderlyingAssetETD", msg, ErrorLevel.CHECKUNDERLYING_ERROR);
                            }
                        }
                    }
                    #endregion
                }
                #endregion isAssetCategorieSpecified
            }
            //
            if (StrFunc.IsFilled(msgWarning))
            {
                string tempDisplayMsg = Ressource.GetString2("Msg_ETD_AssetWarning", sql_AssetETD.Identifier);
                tempDisplayMsg += Cst.CrLf + msgWarning;
                //
                if (false == MsgDet.Contains(tempDisplayMsg))
                {
                    //L'enregistrement du trade se poursuit, Spheres® affiche un message pour avertir qu'il n'existe pas de sous jacent
                    if (StrFunc.IsFilled(MsgDet))
                        MsgDet += Cst.CrLf2;
                    MsgDet += tempDisplayMsg;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIsGlobalTransaction"></param>
        /// <param name="pCaptureSessionInfo"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pExchangeTradedDerivative"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pHtUnderlyingAsset"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180423 Analyse du code Correction [CA2200]
        private void SaveUnderlyingAssetETD(string pCS, IDbTransaction pDbTransaction, bool pIsGlobalTransaction, CaptureSessionInfo pCaptureSessionInfo, DateTime pDtSys,
            IExchangeTradedDerivative pExchangeTradedDerivative, DateTime pDtBusiness,
            ref Hashtable pHtUnderlyingAsset)
        {
            IProductBase productBase = (IProductBase)pExchangeTradedDerivative;
            ExchangeTradedDerivativeContainer exchangeTradedDerivative = new ExchangeTradedDerivativeContainer(pExchangeTradedDerivative, Input.DataDocument);

            IFixInstrument fixInstrument = exchangeTradedDerivative.TradeCaptureReport.Instrument;
            int idI = productBase.ProductType.OTCmlId;
            CfiCodeCategoryEnum codeCategory = exchangeTradedDerivative.Category.Value;

            bool isCreateUnderlyingAsset = false;
            bool isUpdateOptionAsset = false;
            bool isCreateTradedAsset = StrFunc.IsEmpty(fixInstrument.SecurityId); //NB: SecurityId contient l'IDASSET de Spheres
            SQL_AssetETD sqlAssetETD;

            if (isCreateTradedAsset)
            {
                #region Asset inexistant
                // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                // PL 20211025 Mise en commentaire (// PL Comment), car j'ai fait évoluer la méthode LoadAssetETD pour ne pas renvoyer d'asset si le DC est inconnu.
                // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                // PL Comment // RD 20131004 [19022] 
                // PL Comment // Bug dans le cas de Spheres I/O avec un trade incomplet et un DC manquant
                // PL Comment // Donc vérifier d'abord que toutes les données (Market, DC, Echéance, Strike et PutCall) existent avant de tenter de charger l'Asset
                // PL Comment // Dans le cas de la saisie, ce problème ne se présente pas car le DC est obligatoire
                // PL Comment AssetETDBuilder assetETDBuilder = new AssetETDBuilder(fixInstrument, idI, codeCategory);
                // PL Comment AssetETDBuilder.CheckMessageEnum checkMsgEnum = AssetETDBuilder.CheckMessageEnum.None;
                // PL Comment bool isValid = assetETDBuilder.CheckCharacteristicsBeforeCreateAssetETD(pCS, productBase, out checkMsgEnum);

                // PL Comment if (isValid)
                // PL Comment {

                // --------------------------------------------------------------------------------------------------------------------------------------------
                // L'asset a peut-être été créé précédemment (cas des stratégies avec même asset sur plusieurs jambes), on vérifie donc s'il existe maintenant.
                // --------------------------------------------------------------------------------------------------------------------------------------------
                // FI 20140328 [19793] mise en cache abusive du dbTransaction qui entrainait une "non" mise en cache de cette requête
                // NB: on met en cache le résultat de la query uniquement si celle-ci retourne au moins une ligne (donc un Asset). Aucune mise en cache si aucun Asset retournée.
                sqlAssetETD = AssetTools.LoadAssetETD(CSTools.SetCacheOn(pCS, 1, null), pDbTransaction, idI, codeCategory, fixInstrument, pDtBusiness);
                if (null == sqlAssetETD)
                {
                    /* FI 20220613 [XXXXX] Mise en commentaire car l'appel avec comme paramètre Cst.ETDMaturityInputFormatEnum.FIX est sans effet
                    //Création de l'asset: si Saisie "Full", calcule de l'échéance dans le cas d'une éventuelle saisie "abrégée" (ex. "FEB 1" --> "200102")
                    new FixInstrumentContainer(fixInstrument).SetMaturityMonthYear(pCS, Cst.ETDMaturityInputFormatEnum.FIX, fixInstrument.MaturityMonthYear);
                    */
                    // FI 20180927 [24202] Add isAssetAlreadyExist
                    sqlAssetETD = AssetTools.CreateAssetETD(CSTools.SetCacheOn(pCS), pDbTransaction, productBase, fixInstrument, codeCategory, pCaptureSessionInfo.user.IdA, pDtSys, pDtBusiness,
                                                            out string VRMsg, out string infoMsg, out bool isAssetAlreadyExist);
                    if (false == isAssetAlreadyExist)
                        AddToLog_CreatedAssetETD(sqlAssetETD.Id, sqlAssetETD.Identifier, VRMsg, infoMsg, ref pHtUnderlyingAsset);
                }

                //FI 20121029 [18205] Appel systématique à ExchangeTradedDerivativeTools.SetFixInstrumentFromETDAsset
                //Mise à jour du DataDocument
                ExchangeTradedDerivativeTools.SetFixInstrumentFromETDAsset(pCS, pDbTransaction, sqlAssetETD, codeCategory, fixInstrument, Input.DataDocument);

                // PL Comment }
                // PL Comment else
                // PL Comment {
                // PL Comment string msgCheck = (checkMsgEnum != AssetETDBuilder.CheckMessageEnum.None) ? Ressource.GetString("Msg_ETD_" + checkMsgEnum.ToString()) : string.Empty;
                // PL Comment throw new Exception(msgCheck);
                // PL Comment }
                #endregion Asset inexistant
            }
            else
            {
                #region Asset existant
                sqlAssetETD = new SQL_AssetETD(CSTools.SetCacheOn(pCS), IntFunc.IntValue(fixInstrument.SecurityId));
                // FI 20140328 [19793] mise en cache du dbTransaction abusif du coup cette requête ne rentrait pas dans le cache
                if (pIsGlobalTransaction)
                    sqlAssetETD.DbTransaction = pDbTransaction;
                sqlAssetETD.LoadTable();
                #endregion Asset existant
            }

            try
            {
                if (StrFunc.IsFilled(sqlAssetETD.DrvContract_AssetCategorie))
                {
                    switch (Cst.ConvertToUnderlyingAsset(sqlAssetETD.DrvContract_AssetCategorie))
                    {
                        case Cst.UnderlyingAsset.Future:
                        case Cst.UnderlyingAsset.ExchangeTradedContract: //le paramétrage autorise uniquement pour catégorie du sous-jacent: "Future"("ExchangeTradedContract" est ici par sécurité)
                            #region Cas d'un DC "Option sur Future": on vérifie si l'échéance du Future sous-jacent est déterminable.
                            if ((sqlAssetETD.DrvContract_IdDerivativeContractUnl > 0) && (sqlAssetETD.DrvAttrib_IdAssetUnl == 0))
                            {
                                SQL_AssetETD sqlFut_Asset = AssetTools.GetAssetFutureRelativeToMaturityOption(pCS, pDbTransaction, productBase,
                                                                                                            sqlAssetETD,
                                                                                                            pDtBusiness, pCaptureSessionInfo.user.IdA, pDtSys,
                                                                                                            out string VRMsg, out string infoMsg, out Boolean isAssetFutCreated);
                                if (null != sqlFut_Asset)
                                {
                                    if (isAssetFutCreated)
                                    {
                                        // si asset future ajout des informations en raltion avec cette création
                                        AddToLog_CreatedAssetETD(sqlFut_Asset.Id, sqlFut_Asset.Identifier, VRMsg, infoMsg, ref pHtUnderlyingAsset);
                                    }

                                    //Mise à jour de l'échéance de l'asset Option --> On y renseigne l'échéance de l'asset Future sous-jacent qui s'y rapporte (qui donnera lieu à livraison)
                                    isUpdateOptionAsset = true;
                                    AssetTools.SetUnderlyingAssetETD(pCS, pDbTransaction, sqlAssetETD.IdDerivativeAttrib, sqlFut_Asset.Id, pCaptureSessionInfo.user.IdA, pDtSys);
                                }
                            }
                            #endregion Cas d'un DC "Option sur Future"
                            break;
                        case Cst.UnderlyingAsset.Commodity:
                            #region Cas d'un DC avec Cascading, on vérifie si les Assets résultants du Cascading sont déterminables.
                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            // PL 20160606 New feature - [22155] - WARNING
                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            // Pour l'instant, ce process est limité aux UnderlyingAsset.Commodity. Peut-être sera-t-il utile ou nécessaire de faire évoluer... 
                            // Cette restriction a été initiée pour éviter un SELECT systématique, source potentielle de régression des performances lors de l'importation des trades.
                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

                            bool isMonthlyMaturity = (sqlAssetETD.Maturity_MaturityMonthYear.Length == 6);
                            if (isMonthlyMaturity)
                            {
                                string maturityMonth = sqlAssetETD.Maturity_MaturityMonthYear.Substring(4);

                                #region SQL
                                // RD 20200824 [25239] 
                                // - Gérer l'increment lors d'un cascading d'un contrat annuel (CONTRACTCASCADING.ISYEARINCREMENT)
                                // - Enlever le leftjoin "MATURITY mac" qui ne sert à rien
                                string cascadingMaturityMonthYear = "case when cc.ISYEARINCREMENT = 1 then convert(varchar, convert(int, substring(ma.MATURITYMONTHYEAR,1,4))+1) else substring(ma.MATURITYMONTHYEAR, 1, 4) end || case when cc.CASCMATURITYMONTH<10 then '0' || convert(varchar,cc.CASCMATURITYMONTH) else convert(varchar,cc.CASCMATURITYMONTH) end";
                                // FI 20220524 [XXXXX] Suppression de la jointure sur MATURITYRULE puisque inutile
                                string query = SQLCst.SELECT + "cc.CASCMATURITYMONTH,cc.IDDC_CASC," + Cst.CrLf;
                                query += cascadingMaturityMonthYear + " as CASCMATURITYMONTHYEAR," + Cst.CrLf;
                                query += "dcc.IDENTIFIER as IDENTIFIER_CASC,assetc.IDASSET" + Cst.CrLf;
                                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString() + " dc" + Cst.CrLf;
                                query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString() + " da on da.IDDC=dc.IDDC" + Cst.CrLf;
                                query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MATURITY.ToString() + " ma on ma.IDMATURITY=da.IDMATURITY and ma.MATURITYMONTHYEAR=@MATURITYMONTHYEAR" + Cst.CrLf;
                                query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CONTRACTCASCADING.ToString() + " cc on cc.IDDC=dc.IDDC and cc.MATURITYMONTH=@MATURITYMONTH" + Cst.CrLf;
                                query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString() + " dcc on dcc.IDDC=cc.IDDC_CASC" + Cst.CrLf;

                                query += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED.ToString() + " assetc on assetc.IDDC=cc.IDDC_CASC" + Cst.CrLf;
                                query += "                  and assetc.MATURITYMONTHYEAR=" + cascadingMaturityMonthYear + Cst.CrLf;
                                query += SQLCst.WHERE + "dc.IDDC=@IDDC" + Cst.CrLf;
                                query += SQLCst.ORDERBY + "cc.CASCMATURITYMONTH desc";
                                #endregion SQL

                                DataParameters dp = new DataParameters();
                                dp.Add(new DataParameter(pCS, "IDDC", DbType.Int32), sqlAssetETD.IdDerivativeContract);
                                dp.Add(new DataParameter(pCS, "MATURITYMONTHYEAR", DbType.AnsiString, SQLCst.UT_MATURITY_LEN), sqlAssetETD.Maturity_MaturityMonthYear);
                                dp.Add(new DataParameter(pCS, "MATURITYMONTH", DbType.Int32), Convert.ToInt32(maturityMonth));

                                QueryParameters qryParameters = new QueryParameters(pCS, query, dp);
                                DataSet ds;

                                if (pIsGlobalTransaction || isCreateTradedAsset || isCreateUnderlyingAsset || isUpdateOptionAsset)
                                    ds = DataHelper.ExecuteDataset(pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                                else
                                    ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(pCS, 1, 1), CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                                if (ArrFunc.IsFilled(ds.Tables[0].Rows))
                                {
                                    for (int i = 0; i < ArrFunc.Count(ds.Tables[0].Rows); i++)
                                    {
                                        DataRow dr = ds.Tables[0].Rows[i];

                                        int cascadingIdDC = Convert.ToInt32(dr["IDDC_CASC"]);
                                        cascadingMaturityMonthYear = dr["CASCMATURITYMONTHYEAR"].ToString();
                                        string cascadingSymbol = dr["IDENTIFIER_CASC"].ToString();
                                        Nullable<int> cascadingIdAsset = (dr["IDASSET"] is DBNull) ? (Nullable<int>)null : Convert.ToInt32(dr["IDASSET"]);
                                        if (!cascadingIdAsset.HasValue)
                                        {
                                            IFixInstrument fixInstrumentCascading = productBase.CreateFixInstrument();
                                            //PL 20160606 Impossible d'utiliser SetMaturityMonthYear() car cette méthode n'autorise pas l'usage d'une transaction, 
                                            //            transaction ici indispensable pour éviter un inter-blocage. A faire évoluer ultérieurement si nécessiare...
                                            //fixInstrument.SetMaturityMonthYear(CS, Cst.ETDMaturityInputFormatEnum.FIX, cascadingMaturityMonthYear);
                                            fixInstrumentCascading.MaturityMonthYear = cascadingMaturityMonthYear;
                                            fixInstrumentCascading.SecurityExchange = fixInstrument.SecurityExchange;
                                            fixInstrumentCascading.Symbol = cascadingSymbol;


                                            SQL_AssetETD sqlAssetCascading = AssetTools.CreateAssetETD(CSTools.SetCacheOn(pCS), pDbTransaction,
                                                                                    productBase, fixInstrumentCascading, codeCategory,
                                                                                    pCaptureSessionInfo.user.IdA, pDtSys, pDtBusiness,
                                                                                    out string VRMsg, out string infoMsg, out Boolean isAssetAlreadyExist);
                                            if (false == isAssetAlreadyExist)
                                                AddToLog_CreatedAssetETD(sqlAssetCascading.Id, sqlAssetCascading.Identifier, VRMsg, infoMsg, ref pHtUnderlyingAsset);
                                        }
                                    }
                                }

                            }
                            #endregion Cas d'un DC avec Cascading
                            break;
                    }
                }
            }
            catch
            {
                // FI 20221109 [XXXXX] Si exception rencontrée => il y aura rollback.
                // Il faut donc supprimer l'IdAsset de fixInstrument
                if (isCreateTradedAsset)
                    fixInstrument.SecurityId = null;
                throw;
            }
        }

        /// <summary>
        /// Recherche du Taux de financement (Funding) alimentation de tradeInput
        /// </summary>
        /// <param name="pScheduleRequest"></param>
        /// <param name="opErrMsg"></param>
        /// <param name="opErrStatus"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        // EG 20150320 [POC] : New
        public static bool SetFunding(FeeRequest pScheduleRequest, ref string opErrMsg, ref ProcessStateTools.StatusEnum opErrStatus, ref Exception opException)
        {
            return SetFunding(Cst.FundingType.Funding, pScheduleRequest, ref opErrMsg, ref opErrStatus, ref opException);
        }

        /// <summary>
        /// Recherche du Taux de financement (Funding|Borrowing) alimentation de tradeInput
        /// </summary>
        /// <param name="pScheduleRequest"></param>
        /// <param name="opErrMsg"></param>
        /// <param name="opErrStatus"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        // EG 20150320 [POC] : New
        public static bool SetFunding(Cst.FundingType pFundingType, FeeRequest pScheduleRequest, ref string opErrMsg, ref ProcessStateTools.StatusEnum opErrStatus, ref Exception opException)
        {
            bool ret = true;
            try
            {
                FundingProcessing funding = new FundingProcessing(pFundingType, pScheduleRequest);
                funding.Calc();
                funding.SetFunding();
            }
            catch (Exception ex)
            {
                ret = false;

                opErrMsg = Ressource.GetString("Msg_Funding_ProcessError") + Cst.CrLf + Cst.CrLf + ex.Message;
                opErrStatus = ProcessStateTools.StatusErrorEnum;
                opException = ex;

#if DEBUG
                //FI 20140925 [XXXXX] mise en place d'une exception si mode DEBUG
                throw;
#endif
            }
            return ret;
        }

        /// <summary>
        /// Retourne la classe de calcul du taux de refinancement (Funding|Borrowing) 
        /// </summary>
        /// <param name="pFundingType"></param>
        /// <param name="pScheduleRequest"></param>
        /// <returns></returns>
        /// EG 20150320 [POC] : New
        public static FundingProcessing GetFunding(Cst.FundingType pFundingType, FeeRequest pScheduleRequest)
        {
            FundingProcessing funding;
            try
            {
                funding = new FundingProcessing(pFundingType, pScheduleRequest);
                funding.Calc();
                funding.SetFunding();
            }
            catch
            {
                throw;
            }
            return funding;
        }

        /// <summary>
        /// Recherche du Ratio de risque (Margin) alimentation de tradeInput
        /// </summary>
        /// <param name="pScheduleRequest"></param>
        /// <param name="opErrMsg"></param>
        /// <param name="opErrStatus"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        // EG 20150306 [POC] : New
        public static bool SetMarging(FeeRequest pScheduleRequest, ref string opErrMsg, ref ProcessStateTools.StatusEnum opErrStatus, ref Exception opException)
        {
            bool ret = true;
            try
            {
                MarginProcessing margin = new MarginProcessing(pScheduleRequest);
                margin.Calc();
                margin.SetMargin();
            }
            catch (Exception ex)
            {
                ret = false;

                opErrMsg = Ressource.GetString("Msg_Marging_ProcessError") + Cst.CrLf + Cst.CrLf + ex.Message;
                opErrStatus = ProcessStateTools.StatusErrorEnum;
                opException = ex;

#if DEBUG
                //FI 20140925 [XXXXX] mise en place d'une exception si mode DEBUG
                throw;
#endif
            }
            return ret;
        }
        /// <summary>
        /// Retourne la classe de calcul du margin factor (Funding|Borrowing) 
        /// </summary>
        /// <param name="pScheduleRequest"></param>
        /// <returns></returns>
        // EG 20150306 [POC] : New
        public static MarginProcessing GetMarging(FeeRequest pScheduleRequest)
        {
            MarginProcessing margin;
            try
            {
                margin = new MarginProcessing(pScheduleRequest);
                margin.Calc();
                margin.SetMargin();
            }
            catch (Exception) { throw; }
            return margin;
        }

        /// <summary>
        /// Recherche du Taux de financement (Funding) et Ratio de risque (Margin) et alimentation de tradeInput
        /// </summary>
        /// <param name="pScheduleRequest"></param>
        /// <param name="opErrMsg"></param>
        /// <param name="opErrStatus"></param>
        /// <param name="opException"></param>
        /// <returns></returns>
        // EG 20150304 Funding n'est appelé que sur le ReturnSwap
        public static bool SetFundingAndMargin(FeeRequest pScheduleRequest, ref string opErrMsg, ref ProcessStateTools.StatusEnum opErrStatus, ref Exception opException)
        {
            bool ret = true;
            if (pScheduleRequest.Product.Product.ProductBase.IsReturnSwap)
                ret = SetFunding(pScheduleRequest, ref opErrMsg, ref opErrStatus, ref opException);
            if (ret)
                ret = SetMarging(pScheduleRequest, ref opErrMsg, ref opErrStatus, ref opException);
            return ret;
        }

        /// <summary>
        /// Journalise l'insertion d'un asset ETD
        /// </summary>
        /// <param name="pNewIdTAsset"></param>
        /// <param name="pNewIdentifierAsset"></param>
        /// <param name="pVRMsg">Message de warning associé à la création de l'asset</param>
        /// <param name="pInfoMsg">Message d'info des actions menées</param>
        /// <param name="opHtUnderlyingAsset"></param>
        private void AddToLog_CreatedAssetETD(int pNewIdTAsset, string pNewIdentifierAsset, string pVRMsg, string pInfoMsg, ref Hashtable opHtUnderlyingAsset)
        {
            //Si strategie avec création d'asset alors l'asset a peut-être déjà été créé depuis 
            if (false == opHtUnderlyingAsset.Contains(pNewIdTAsset))
            {
                opHtUnderlyingAsset.Add(pNewIdTAsset, pNewIdentifierAsset);
                //
                if (StrFunc.IsFilled(pInfoMsg) || StrFunc.IsFilled(pVRMsg))
                {
                    //"Actif: {0}"
                    string tempDisplayMsg = Ressource.GetString2("Msg_ETD_AssetWarning", pNewIdentifierAsset);
                    //
                    if (StrFunc.IsFilled(pInfoMsg))
                        tempDisplayMsg += Cst.CrLf + pInfoMsg;
                    //
                    if (StrFunc.IsFilled(pVRMsg))
                    {
                        //"Msg_ETD_ValidationRuleWarning" Attention, certaines règles de validation ne sont pas respectées:
                        tempDisplayMsg += Cst.CrLf + Ressource.GetString("Msg_ETD_ValidationRuleWarning");
                        tempDisplayMsg += Cst.CrLf + pVRMsg;
                    }
                    //
                    if (StrFunc.IsFilled(MsgDet))
                        MsgDet += Cst.CrLf2;
                    MsgDet += tempDisplayMsg;
                }
            }
        }

        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        protected override void UpdateTradeSourceStUsedBy(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, DateTime pDtSys)
        {
            base.UpdateTradeSourceStUsedBy(pCS, pDbTransaction, pCaptureMode, pSessionInfo, pDtSys);
            if (Cst.Capture.IsModePositionTransfer(pCaptureMode))
            {
                string lblStUsedBy = Cst.ProcessTypeEnum.POSKEEPREQUEST.ToString() + ":" + pCaptureMode.ToString();
                TradeCommonCaptureGen.UpdateTradeStSysUsedBy(pCS, pDbTransaction, Input.positionTransfer.tradeIdentification.OTCmlId, pSessionInfo, pDtSys, Cst.StatusUsedBy.RESERVED, lblStUsedBy);
            }
        }

        /// <summary>
        /// Contrôles spécifiques effectués sur un produit
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// <exception cref="TradeCommonCaptureGenException[SPECIFICCHECK_ERROR] si une anomalie est détectée"></exception>
        private static void CheckSimpleProduct(string pCS, IDbTransaction pDbTransaction, ProductContainer pProduct, DateTime pDtRefForDtEnabled)
        {
            string msg = null;
            //
            if (pProduct.IsExchangeTradedDerivative)
            {
                ExchangeTradedDerivativeContainer etdProduct =
                    new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)pProduct.Product, pProduct.DataDocument);

                if (StrFunc.IsEmpty(msg))
                {
                    // FI 20121004 [18172] use method ExchangeTradedDerivativeTools.LoadSqlDerivativeContract
                    IFixInstrument fixInstrument = ((IExchangeTradedDerivative)pProduct.Product).TradeCaptureReport.Instrument;
                    _ = fixInstrument.Symbol;

                    SQL_DerivativeContract sqlDerivativeContract =
                        ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(pCS, pDbTransaction, fixInstrument.SecurityExchange,
                        fixInstrument.Symbol, SQL_Table.ScanDataDtEnabledEnum.Yes, pDtRefForDtEnabled);

                    if (null == sqlDerivativeContract)
                    {
                        msg = StrFunc.AppendFormat("Derivative Contract {0} is unknown", fixInstrument.Symbol);
                    }

                }

                // FI 20121008 Ajout contrôle de la présence de la maturité, du Call/Put  et du stricke
                if (StrFunc.IsEmpty(msg))
                {
                    FixInstrumentContainer fixInstrument = new FixInstrumentContainer(((IExchangeTradedDerivative)pProduct.Product).TradeCaptureReport.Instrument);
                    if (false == fixInstrument.IsAssetInfoFilled(true, etdProduct.IsOption))
                    {
                        if (StrFunc.IsEmpty(fixInstrument.MaturityMonthYear))
                            msg = StrFunc.AppendFormat("Maturity is not specified") + Cst.CrLf;

                        if (etdProduct.IsOption)
                        {
                            if (false == fixInstrument.PutOrCallSpecified)
                                msg += StrFunc.AppendFormat("Call/Put is not specified") + Cst.CrLf;
                            if (false == fixInstrument.StrikePriceSpecified)
                                msg += StrFunc.AppendFormat("StrikePrice is not specified") + Cst.CrLf;
                        }
                    }
                }

            }
            //
            if (StrFunc.IsFilled(msg))
                throw new TradeCommonCaptureGenException("CheckSimpleProduct", msg, ErrorLevel.SPECIFICCHECK_ERROR);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// <exception cref="TradeCommonCaptureGenException[LOAD_TRADELINK_ERROR] si une anomalie est détectée"></exception>
        /// EG 20120203 Ajout IDENTIFIER/IDENTIFICATION dans TradeLink
        protected override void SaveTradelink(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, int pIdT, string pIdentifier)
        {
            if (Cst.Capture.IsModePositionTransfer(pCaptureMode))
            {
                TradeLink.TradeLink TradeLink = new TradeLink.TradeLink(
                pIdT, Input.positionTransfer.tradeIdentification.OTCmlId,
                EFS.TradeLink.TradeLinkType.PositionTransfert, null, null,
                new string[2] { pIdentifier, Input.positionTransfer.tradeIdentification.Identifier },
                new string[2] { EFS.TradeLink.TradeLinkDataIdentification.NewIdentifier.ToString(), EFS.TradeLink.TradeLinkDataIdentification.OldIdentifier.ToString() }); ;
                TradeLink.Insert(pCS, pDbTransaction);
            }
            //
            base.SaveTradelink(pCS, pDbTransaction, pCaptureMode, pIdT, pIdentifier);
        }

        /// <summary>
        /// Retourne true si le trade peut-être sauvegardé alors qu'il est incomplet 
        /// </summary>
        public override bool IsInputIncompleteAllow(Cst.Capture.ModeEnum pCaptureMode)
        {
            bool ret;
            //La saisie de la transférée doit être complete (Il y a des infos ds tradeInput nécessaires pour le post vers POSTREQUEST)
            if (Cst.Capture.IsModePositionTransfer(pCaptureMode))
                ret = false;
            else
                ret = base.IsInputIncompleteAllow(pCaptureMode);
            //
            return ret;
        }

        /// <summary>
        /// Retourne un message suite à l'appel de CheckAndRecord
        /// </summary>
        /// <param name="pEx">Représente l'exception rencontrée lors de la sauvegarde, null possible</param>
        /// <param name="pMode">Représente le type de saisie</param>
        /// <param name="pNewIdentifier">Représente l'lidentifier du trade</param>
        /// <param name="pUnderlying">Représente les assets sous-jacents injectés, null possible</param>
        /// <param name="pTrader">Représente les traders injectés, null possible</param>
        /// <param name="pIsAddinnerExceptionInMsgDet">si true, ajoute le message détail de l'exception non gérée à l'origine de l'exception {pEx}.
        /// Remarque : Les exception de type SQL sont ajoutées dans le message détail même si {pIsAddinnerExceptionInMsgDet} = false</param>
        /// <param name="pMsgDet">retourne le message détail associé</param>
        /// <returns></returns>
        /// FI 20170404 [23039] Modify (Modification de signature pUnderlying, pTrader) 
        public override string GetResultMsgAfterCheckAndRecord(string pCS,
                                                               TradeCommonCaptureGenException pEx, Cst.Capture.ModeEnum pMode,
                                                                string pNewIdentifier,
                                                                Pair<int, string>[] pUnderlying,
                                                                Pair<int, string>[] pTrader,
                                                                bool pIsAddinnerExceptionInMsgDet, out string pMsgDet)
        {
            string ret = base.GetResultMsgAfterCheckAndRecord(pCS, pEx, pMode, pNewIdentifier, pUnderlying, pTrader, pIsAddinnerExceptionInMsgDet, out pMsgDet);

            if (StrFunc.IsFilled(ret))
            {
                if (pMode == Cst.Capture.ModeEnum.PositionTransfer)
                    ret = ret.Replace("{1}", Input.positionTransfer.tradeIdentification.Identifier);
            }
            return ret;
        }



        /// <summary>
        /// Control si l'Identifier du trade saisi ou modifié n'existe pas 
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <param name="pIdentifier"></param>
        /// <exception cref="TradeCommonCaptureGenException[IDENTIFIER_DUPLICATE] si identifiant existe déjà"></exception>
        /// FI 202120301 [18465]  new methode (L'importation des trades permet éventuellement de choisir un identifier pour le trade)
        /// Besoin identifier en cas de création de trade qui ouvre une position
        /// Pour l'instant il n'est possible de spécifier un identifier en mode création (La méthode gère toutefois le mode update)
        protected override void CheckIdentifier(string pCS, Cst.Capture.ModeEnum pCaptureMode, string pIdentifier)
        {
            bool isNew = Cst.Capture.IsModeNewCapture(pCaptureMode);
            bool isUpd = Cst.Capture.IsModeUpdateOrUpdatePostEvts(pCaptureMode);
            //
            bool isToCheck = isNew || isUpd;
            //
            if (isToCheck)
            {
                StrBuilder sql = new StrBuilder();
                sql += SQLCst.SELECT + "1" + SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADE + " t" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT + " i on i.IDI=t.IDI" + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PRODUCT + " p on p.IDP=i.IDP" + Cst.CrLf;
                sql += SQLCst.WHERE + "p.GPRODUCT not in ('ADM','RISK','ASSET')" + Cst.CrLf;
                sql += SQLCst.AND + "t.IDENTIFIER=@IDENTIFIER" + Cst.CrLf;
                //
                // RD 20140120 [19512] Bug sous Oracle, enlever le préfixe "@" sur le nom du paramètre
                if (isUpd)
                    sql += SQLCst.AND + @"t.IDT!=IDT" + Cst.CrLf;
                //
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), pIdentifier);
                //
                if (isUpd)
                    parameters.Add(new DataParameter(pCS, "@IDT", DbType.Int32), m_Input.IdT);
                //
                QueryParameters qry = new QueryParameters(pCS, sql.ToString(), parameters);
                //
                object obj = DataHelper.ExecuteScalar(qry.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                bool isOk = (null == obj);
                if (false == isOk)
                {
                    string msg = Ressource.GetString2("Msg_Identifier_Detail", pIdentifier);
                    throw new TradeCommonCaptureGenException("CheckIdentifier", msg, TradeCaptureGen.ErrorLevel.IDENTIFIER_DUPLICATE);
                }
            }
        }

        /// <summary>
        /// Mise à jour de l'UTI sur une allocation (la mise à jour s'applique uniquement lorsque l'UTI est non renseigné)
        /// </summary>
        /// <param name="pSide">côté dealer ou clearer</param>
        /// <param name="tradeIdentification"></param>
        /// FI 20140206 [19564] add Method
        /// FI 20140307 [19689] add pDbTransaction
        /// EG 20140526 Replace etd by exchangeTraded
        private void UpdateTradeUTI(string pCS, IDbTransaction pDbTransaction, TypeSideAllocation pSide, SpheresIdentification pTradeIdentification)
        {
            if (false == Input.IsTradeFoundAndAllocation)
                throw new Exception(StrFunc.AppendFormat("Business status: {0} is not ALLOC", Input.TradeStatus.stBusiness.NewSt));
            _ = TradeCommonInput.DataDocument;
            ProductContainer productMain = TradeCommonInput.DataDocument.CurrentProduct;

            ExchangeTradedContainer exchangeTraded = null;
            if (productMain.IsExchangeTradedDerivative)
                exchangeTraded = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productMain.Product, TradeCommonInput.DataDocument);
            else if (productMain.IsEquitySecurityTransaction)
                exchangeTraded = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)productMain.Product);
            //ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)this.Input.DataDocument.currentProduct.product);
            if (null != exchangeTraded)
            {
                IFixParty fixParty = null;
                if (pSide == TypeSideAllocation.Dealer)
                    fixParty = exchangeTraded.GetDealer();
                else if (pSide == TypeSideAllocation.Clearer)
                    fixParty = exchangeTraded.GetClearerCustodian();

                if (null != fixParty)
                {
                    IParty party = Input.DataDocument.GetParty(fixParty.PartyId.href);
                    if (Input.DataDocument.IsUTIEmptyOrMissing(party.Id))
                    {
                        //Calcul uniquement si Empty ou Missing
                        Input.CalcAndSetTradeUTI(pCS, pDbTransaction, pSide, pTradeIdentification);
                    }
                }
            }
        }

        /// <summary>
        ///  Alimentation des tables POSUTI et POSUTIDET
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdaUser"></param>
        /// <param name="pDtSys"></param>
        /// <param name="pIdaUser"></param>
        /// <param name="pIsAutoTransaction">True si le trade est enregistré dans une transaction indépendante</param>
        /// FI 20140602 [20023] Alimentation de POSUTIDET 
        /// EG 20140526 Add EquitySecurityTransactionContainer
        /// FI 20140623 [20125] Alimentation de POSUTIDET rule != UTIRule.NONE (ansi lorsque la valeur vaut NA, cela veut dire que Spheres® n' pas été en mesure de calculer le PUTI)
        /// EG 20200519 [XXXXX] Ajout connectionString manquant à ExecuteNonQuery
        /// EG 20200925 [XXXXX] Test de Duplicate Key sur Insertion POSUTIDET (+ index UX_POSUTI: IDPOSUTI, IDA, UTISCHEME, UTIRULE)
        /// EG 20200925 [XXXXX] Test de Duplicate Key sur Insertion POSUTI
        protected override void SavePositionUTI(string pCS, IDbTransaction pDbTransaction, SpheresIdentification tradeIdentification, int pIdaUser, DateTime pDtSys, Boolean pIsAutoTransaction)
        {
            if (this.Input.IsTradeFoundAndAllocation && (false == Input.IsGiveUp(pCS, pDbTransaction)) && Input.IsCalcUTIAvailable(pCS, pDbTransaction))
            {
                DataDocumentContainer doc = TradeCommonInput.DataDocument;
                ProductContainer productMain = TradeCommonInput.DataDocument.CurrentProduct;

                ExchangeTradedContainer exchangeTraded = null;
                if (productMain.IsExchangeTradedDerivative)
                    exchangeTraded = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productMain.Product, TradeCommonInput.DataDocument);
                else if (productMain.IsEquitySecurityTransaction)
                    exchangeTraded = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)productMain.Product);

                int idADealer = 0;
                int idBDealer = 0;
                int idAClearer = 0;
                int idBClearer = 0;
                if (null != exchangeTraded)
                {
                    //Dealer
                    IFixParty dealer = exchangeTraded.GetDealer();
                    if (null != dealer)
                    {
                        IParty dealerParty = doc.GetParty(dealer.PartyId.href);
                        if (null != dealerParty)
                            idADealer = dealerParty.OTCmlId;

                        IBookId dealerBook = doc.GetBookId(dealer.PartyId.href);
                        if (null != dealerBook)
                            idBDealer = dealerBook.OTCmlId;
                    }

                    //Clearer
                    //FI 20140219 [19636][19640] Dans le cadre de la livraison des correctifs mentionnés correction ds l'alimentation  ds idAClearer et idBClearer
                    IFixParty clearer = exchangeTraded.GetClearerCustodian();
                    if (null != clearer)
                    {
                        IParty clearerParty = doc.GetParty(clearer.PartyId.href);
                        if (null != clearerParty)
                        {
                            idAClearer = clearerParty.OTCmlId;
                            IBookId clearerBook = doc.GetBookId(clearer.PartyId.href);
                            if (null != clearerBook)
                                idBClearer = clearerBook.OTCmlId;
                        }
                    }
                }
                int idI = productMain.ProductBase.ProductType.OTCmlId;
                // EG 20140226 Add pDbTransaction
                // FI 20140326 [16609] add Cache, la transaction est passée uniquement si nécessaire 
                // EG 20150402 [POC] Add CS parameter to read FXRateAsset default for FX
                Nullable<int> idAsset = productMain.GetUnderlyingAssetId(pCS);

                bool isOk = (idADealer > 0) && (idBDealer > 0) && (idAClearer > 0) && (idBClearer > 0);
                isOk = isOk && (idAsset.HasValue && idAsset > 0) && (idI > 0);

                if (isOk)
                {

                    DataParameters dp = new DataParameters();
                    dp.Add(new DataParameter(pCS, "IDI", DbType.Int32), idI);
                    dp.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), idAsset.Value);
                    dp.Add(new DataParameter(pCS, "IDA_DEALER", DbType.Int32), idADealer);
                    dp.Add(new DataParameter(pCS, "IDB_DEALER", DbType.Int32), idBDealer);
                    dp.Add(new DataParameter(pCS, "IDA_CLEARER", DbType.Int32), idAClearer);
                    dp.Add(new DataParameter(pCS, "IDB_CLEARER", DbType.Int32), idBClearer);

                    string restrict = @"
posUti.IDI = @IDI and posUti.IDASSET = @IDASSET and 
posUti.IDA_DEALER = @IDA_DEALER and posUti.IDB_DEALER = @IDB_DEALER and
posUti.IDA_CLEARER = @IDA_CLEARER and posUti.IDB_CLEARER = @IDB_CLEARER";

                    string queryAdapterPOSUTI = "select posUti.IDPOSUTI, posUti.IDI,posUti.IDASSET,posUti.IDA_DEALER,posUti.IDB_DEALER,posUti.IDA_CLEARER,posUti.IDB_CLEARER,posUti.IDT_OPENING,posUti.DTINS, posUti.IDAINS from dbo.POSUTI posUti";
                    string queryPOSUTI = queryAdapterPOSUTI + SQLCst.WHERE + Cst.CrLf + restrict;

                    string queryAdapterPOSUTIDET = "select posUtiDet.IDPOSUTI, posUtiDet.IDA, posUtiDet.UTI, posUtiDet.UTISCHEME, posUtiDet.UTIRULE,posUtiDet.DTINS, posUtiDet.IDAINS from dbo.POSUTIDET posUtiDet";
                    string queryPOSUTIDET = queryAdapterPOSUTIDET + Cst.CrLf +
                    @"inner join dbo.POSUTI on posUti.IDPOSUTI = posUtiDet.IDPOSUTI and " + Cst.CrLf + restrict;

                    string query = queryPOSUTI + SQLCst.SEPARATOR_MULTISELECT + queryPOSUTIDET;

                    QueryParameters qryParameters = new QueryParameters(pCS, query, dp);

                    DataSet ds;
                    if (pIsAutoTransaction && RecordSettings.isSaveUnderlyingInParticularTransaction)
                        ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(pCS, 1, 1), CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    else
                        ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                    (int idPosUTI, int idTOpening) posUti;
                    if (ArrFunc.IsEmpty(ds.Tables[0].Rows))
                    {
                        dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pIdaUser);
                        dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), pDtSys);
                        dp.Add(new DataParameter(pCS, "IDT_OPENING", DbType.Int32), tradeIdentification.OTCmlId);

                        string queryInsert = SQLCst.INSERT_INTO_DBO + "POSUTI(IDI,IDASSET,IDA_DEALER,IDB_DEALER,IDA_CLEARER,IDB_CLEARER,IDT_OPENING,IDAINS,DTINS)" + SQLCst.VALUES + "(@IDI,@IDASSET,@IDA_DEALER,@IDB_DEALER,@IDA_CLEARER,@IDB_CLEARER,@IDT_OPENING,@IDAINS,@DTINS)";
                        qryParameters = new QueryParameters(pCS, queryInsert, dp);
                        // EG 20200519 [XXXXX] Ajout connectionString
                        try
                        {
                            DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        }
                        catch (Exception ex)
                        {
                            if (false == DataHelper.IsDuplicateKeyError(pCS, ex))
                                throw;
                        }
                        posUti = UTITools.GetIdPOSUTI(pCS, pDbTransaction,
                                            new Pair<int, int>(idADealer, idBDealer),
                                            new Pair<int, int>(idAClearer, idBClearer),
                                            new Pair<int, int>(idI, idAsset.Value));
                    }
                    else
                    {
                        posUti.idPosUTI = Convert.ToInt32(ds.Tables[0].Rows[0]["IDPOSUTI"]);
                        posUti.idTOpening = Convert.ToInt32(ds.Tables[0].Rows[0]["IDT_OPENING"]);
                    }

                    //bool isAdddet = false;
                    foreach (IParty party in TradeCommonInput.DataDocument.Party.Where(x => TradeCommonInput.DataDocument.IsPartyCounterParty(x)))
                    {
                        DataRow[] dataRow = ds.Tables[1].Select("IDA=" + party.OtcmlId);
                        if (ArrFunc.IsEmpty(dataRow))
                        {
                            //FI 20140602 [20023] appel des methodes CalcPositionUTI 
                            string positionId = string.Empty;
                            UTIRule rule = default;
                            if (party.OTCmlId == idADealer)
                                this.Input.CalcPositionUTI(pCS, RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction, TypeSideAllocation.Dealer, tradeIdentification, posUti, out positionId, out rule);
                            else if (party.OTCmlId == idAClearer)
                                this.Input.CalcPositionUTI(pCS, RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction, TypeSideAllocation.Clearer, tradeIdentification, posUti, out positionId, out rule);

                            if (rule != UTIRule.NONE)
                            {
                                DataParameters dp2 = new DataParameters();
                                dp2.Add(new DataParameter(pCS, "IDPOSUTI", DbType.Int32), posUti.idPosUTI);
                                dp2.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), party.OTCmlId);
                                dp2.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS), pIdaUser);
                                dp2.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS), pDtSys);
                                dp2.Add(new DataParameter(pCS, "UTI", DbType.String, SQLCst.UT_DESCRIPTION_LEN), StrFunc.IsFilled(positionId) ? positionId : Cst.NotAvailable);
                                dp2.Add(new DataParameter(pCS, "UTISCHEME", DbType.AnsiString, SQLCst.UT_UNC_LEN), Cst.OTCml_TradeIdUTISpheresScheme);
                                dp2.Add(new DataParameter(pCS, "RULE", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), rule);

                                try
                                {
                                    string queryInsert = SQLCst.INSERT_INTO_DBO + "POSUTIDET(IDPOSUTI,IDA,UTI,UTISCHEME,UTIRULE,IDAINS,DTINS)" + SQLCst.VALUES + "(@IDPOSUTI,@IDA,@UTI,@UTISCHEME,@RULE,@IDAINS,@DTINS)";
                                    qryParameters = new QueryParameters(pCS, queryInsert, dp2);
                                    DataHelper.ExecuteNonQuery(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                                }
                                catch (Exception ex)
                                {
                                    if (false == DataHelper.IsDuplicateKeyError(pCS, ex))
                                        throw;
                                }

                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Miseà jour de TradeIdentifier avec les UTIs (La mise à jour s'applique uniquement aux allocations)
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pTradeIdentification"></param>
        protected override void UpdateUTIPartyTradeIdentifier(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, SpheresIdentification pTradeIdentification)
        {
            if (Input.IsTradeFoundAndAllocation)
            {
                string msgErr = string.Empty;
                string msg = "An error occurred during the creation of Unique Trade Identifier. Side :{0}, Error:{1}";

                //Dealer
                try
                {
                    UpdateTradeUTI(pCS, pDbTransaction, TypeSideAllocation.Dealer, pTradeIdentification);
                }
                catch (Exception ex)
                {
                    msgErr = StrFunc.AppendFormat(msg, "Dealer", ExceptionTools.GetMessageExtended(ex));
                }

                //Clearer
                try
                {
                    UpdateTradeUTI(pCS, pDbTransaction, TypeSideAllocation.Clearer, pTradeIdentification);
                }
                catch (Exception ex)
                {
                    if (!String.IsNullOrEmpty(msgErr))
                        msgErr += Cst.CrLf;

                    msgErr += StrFunc.AppendFormat(msg, "Clearer", ExceptionTools.GetMessageExtended(ex));
                }

                if (!String.IsNullOrEmpty(msgErr))
                    this.MsgDet += Cst.CrLf + msgErr;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <exception cref="DataHelperException"></exception>
        protected override void DeletePosRequest(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            try
            {
                TradeRDBMSTools.DeletePosRequest(pCS, pDbTransaction, pIdT);
            }
            catch (Exception ex)
            {
                throw new DataHelperException(DataHelperErrorEnum.query,
                    StrFunc.AppendFormat("Error on delete POSREQUEST for trade (id:{0})", pIdT.ToString()), ex);
            }
        }

        /// <summary>
        /// Alimentation de ENTITYMARKET
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pIdA"></param>
        /// <param name="pDtSys"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        protected override void SaveEntityMarket(string pCS, IDbTransaction pDbTransaction, SpheresIdentification pTradeIdentification, int pIdA, DateTime pDtSys)
        {
            if (Tools.IsUseEntityMarket(CSTools.SetCacheOn(pCS), pDbTransaction, TradeCommonInput.Product) && TradeCommonInput.IsAllocation)
            {
                // FI 20140822 [XXXXX] add if
                // Si l'instrument est fongigle il contient au minimum un marché 
                // Si ce n'est pas le cas => Spheres® ne cherche pas à alimenter ENTITYMARKET
                if (StrFunc.IsFilled(TradeCommonInput.Product.GetMarket()))
                {
                    //PM 20150529 [20575] Ne pas utiliser GetDateBusiness pour savoir si une ligne est présente dans ENTITYMARKET, mais IsExistEntityMarket
                    //DateTime dtBusiness = Tools.GetDateBusiness(CSTools.SetCacheOn(CS), TradeCommonInput.DataDocument);
                    //if (DtFunc.IsDateTimeEmpty(dtBusiness))
                    if (false == Tools.IsExistEntityMarket(CSTools.SetCacheOn(pCS), pDbTransaction, TradeCommonInput.DataDocument))
                    {
                        Tools.SetEntityMarket(pCS, pDbTransaction, TradeCommonInput.DataDocument, pTradeIdentification, pIdA, pDtSys);
                    }
                }
            }
        }

        /// <summary>
        ///  Mise à jpur de {pTradeIdentification}
        ///  <para>Création du trader s'il n'existe pas</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pTradeIdentification"></param>
        /// <param name="pDtSys"></param>
        /// <param name="oTrader">Retourne les traders créés (retourne null si pas de création)</param>
        /// FI 20170404 [23039] Add Method
        /// FI 20170718 [23326] Modify
        protected override void UpdatePartyTradeInformation(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, CaptureSessionInfo pSessionInfo, SpheresIdentification pTradeIdentification, DateTime pDtSys, out Pair<int, string>[] oTrader)
        {
            oTrader = null;

            // Création automatique des traders 
            ITradeHeader tradeHeader = TradeCommonInput.CurrentTrade.TradeHeader;
            // recherche des traders présents dans le datadocument pour lesquels OTCmlId == 0
            // => Ce sont des acteurs saisis/importés qui n'existent pas la base

            // RD 20171228 [23687] Add test
            //List<Pair<int, ITrader>> trader = (from item in tradeHeader.partyTradeInformation.Where(x => x.traderSpecified &&
            //                              (null != TradeCommonInput.DataDocument.GetParty(x.partyReference)) && TradeCommonInput.DataDocument.GetParty(x.partyReference).OTCmlId > 0)
            //                                   from traderItem in item.trader.Where(x => StrFunc.IsFilled(x.identifier) && x.OTCmlId == 0)
            //                                   select new Pair<int, ITrader>() { First = TradeCommonInput.DataDocument.GetParty(item.partyReference).OTCmlId, Second = traderItem }).ToList();

            //if (trader.Count() > 0)
            List<Pair<int, ITrader>> trader = null;
            if (tradeHeader.PartyTradeInformation != null)
            {
                trader = (from item in tradeHeader.PartyTradeInformation.Where(x => x.TraderSpecified &&
                                                      (null != TradeCommonInput.DataDocument.GetParty(x.PartyReference)) && TradeCommonInput.DataDocument.GetParty(x.PartyReference).OTCmlId > 0)
                                               from traderItem in item.Trader.Where(x => StrFunc.IsFilled(x.Identifier) && x.OTCmlId == 0)
                                               select new Pair<int, ITrader>() { First = TradeCommonInput.DataDocument.GetParty(item.PartyReference).OTCmlId, Second = traderItem }).ToList();
            }

            if (ArrFunc.Count(trader) > 0)
            {
                IDbTransaction dbTransaction = null;
                List<Pair<int, string>> newtrader = new List<Pair<int, string>>();
                try
                {
                    // Utilisation d'un transaction indépendante car il s'agit de créer un acteur 
                    // L'usage de la transaction pDbTransaction aurait entraîner le passage de cette même transaction dès qu'il il y lecture de la table ACTOR (trop coûteux)
                    dbTransaction = DataHelper.BeginTran(pCS);

                    DataParameter dpDtEnabled = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTENABLED);
                    dpDtEnabled.Value = TradeCommonInput.DataDocument.CurrentProduct.GetBusinessDate2();

                    DataParameter dpDtIns = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTINS);
                    dpDtIns.Value = pDtSys;

                    DataParameter dpIdAIns = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDAINS);
                    dpIdAIns.Value = pSessionInfo.user.IdA;

                    DataParameter dpDescription = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DESCRIPTION);
                    dpDescription.Value = StrFunc.AppendFormat("generated by Spheres from Trade [Identifier:{0}]", pTradeIdentification.Identifier);

                    foreach (var item in trader)
                    {
                        string traderValue = item.Second.Identifier;

                        // si data ne se termine pas par " [xxxxx]" => alors ajout du suffix " [IDENTIFIER]" pour simplifier la suite de l'algorithme
                        // FI 20170718 [23326] Chgt de l'expression de la regex
                        Regex regex = new Regex(@"^.+\s\[.+\]$");
                        if (false == regex.IsMatch(traderValue))
                            traderValue += " [IDENTIFIER]";

                        string identifier = string.Empty;
                        string displayname = string.Empty;
                        string extlLink = string.Empty;

                        Boolean isOk = true;
                        // FI 20170718 [23326] Chgt de l'expression de la regex
                        regex = new Regex(@"^(.+)\s\[(BIC|IBEI|ISO17442|IDA|IDENTIFIER|DISPLAYNAME|EXTLLINK|EXTLLINK2|MemberId-\w+)\]$");
                        Match match = regex.Match(traderValue);
                        if (match.Success && match.Groups.Count == 3)
                        {
                            // IDENTIFIER, DISPLAYNAME et EXTLLINK => création du trader en automatique possible  
                            string matchValue = match.Groups[1].Value; // La donnée représentée par (\w+)
                            switch (match.Groups[2].Value)
                            {
                                case "IDENTIFIER":
                                case "DISPLAYNAME":
                                    identifier = matchValue;
                                    displayname = matchValue;
                                    extlLink = string.Empty;
                                    break;
                                case "EXTLLINK":
                                    identifier = matchValue;
                                    displayname = matchValue;
                                    extlLink = matchValue;
                                    break;
                                default:
                                    isOk = false; // Pas de création d'acteur si l'identification associée à traderValue est autre que IDENTIFIER, DISPLAYNAME, EXTLLINK
                                    break;
                            }
                        }
                        else
                        {
                            isOk = false;
                            // La validation Rule rejettera le trade (trader inconnu)
                        }

                        if (isOk)
                        {
                            Boolean isActorCreated = false;

                            // Vérification ultime avant céation de l'acteur
                            if (false == EFSRegex.IsMatch(identifier, EFSRegex.TypeRegex.RegexStringAlphaNum))
                                throw new NotSupportedException(StrFunc.AppendFormat("Identifier (value:{0}) is not valid. Error: {1}", identifier, EFSRegex.ErrorMessage(EFSRegex.TypeRegex.RegexStringAlphaNum)));

                            // L'acteur existe-t-il déjà ?
                            SQL_Actor sqlActor = new SQL_Actor(pCS, identifier)
                            {
                                DbTransaction = dbTransaction
                            };
                            if (sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                            {
                                // L'acteur existe déjà 
                                // Vérification qu'il est bien déclaré trader de la partie
                                DataParameters dp = new DataParameters();
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDENTIFIER), identifier);
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), item.First);
                                string query = @"select a.IDA, a.IDENTIFIER 
                                from dbo.ACTOR a
                                inner join dbo.ACTORROLE ar on ar.IDA = a.IDA and ar.IDROLEACTOR = 'TRADER' and ar.IDA_ACTOR = @IDA 
                                where a.IDENTIFIER = @IDENTIFIER";
                                QueryParameters queryParameters = new QueryParameters(pCS, query, dp);
                                // FI 20170718 [23326] Utilisation de la dbTransaction
                                DataTable dt = DataHelper.ExecuteDataTable(pCS, dbTransaction, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                                if (dt.Rows.Count > 0)
                                {
                                    //l'acteur existe déjà et il est Trader vis à vis de la partie => Mise à jour du datadocument
                                    //(Remarque peut-être a-t-il créé par une autre instance de IO lors d'une même demande de traitement)
                                    item.Second.OTCmlId = Convert.ToInt32(dt.Rows[0]["IDA"]);
                                    item.Second.Identifier = Convert.ToString(dt.Rows[0]["IDENTIFIER"]);
                                }
                                else
                                {
                                    // l'acteur existe déjà mais il n'est pas trader de la partie ??? => Echec lors de la création du trader
                                    throw new Exception(StrFunc.AppendFormat("Unable to create trader : {0}. this actor already exist.", identifier));
                                }
                            }
                            else
                            {
                                // Création de l'acteur
                                DataParameters dp = new DataParameters();
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDENTIFIER), identifier);
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DISPLAYNAME), displayname);
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.EXTLLINK), StrFunc.IsFilled(extlLink) ? extlLink : Convert.DBNull);

                                dp.Add(dpDescription);
                                dp.Add(dpDtEnabled);
                                dp.Add(dpDtIns);
                                dp.Add(dpIdAIns);

                                string sqlInsertActor = @"insert into dbo.ACTOR(IDENTIFIER, DISPLAYNAME, DESCRIPTION, DTINS, IDAINS, DTENABLED, EXTLLINK)  
                                values (@IDENTIFIER, @DISPLAYNAME, @DESCRIPTION, @DTINS, @IDAINS, @DTENABLED, @EXTLLINK)";

                                QueryParameters queryParameters = new QueryParameters(pCS, sqlInsertActor, dp);
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text,
                                    queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

                                sqlActor = new SQL_Actor(pCS, identifier)
                                {
                                    DbTransaction = dbTransaction
                                };
                                if (false == sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                                    throw new NotSupportedException(StrFunc.AppendFormat("Actor (Identifier:{0}) is not found", identifier));

                                // Attribution du rôle Trader
                                dp = new DataParameters();
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ID), sqlActor.Id);
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), item.First);
                                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), RoleActor.TRADER.ToString());
                                dp.Add(dpDtEnabled);
                                dp.Add(dpDtIns);
                                dp.Add(dpIdAIns);

                                string sqlInsertRole = @"insert into dbo.ACTORROLE(IDROLEACTOR, IDA, IDA_ACTOR, DTINS, IDAINS, DTENABLED)
                                values (@IDROLEACTOR, @ID, @IDA, @DTINS, @IDAINS, @DTENABLED)";

                                queryParameters = new QueryParameters(pCS, sqlInsertRole, dp);
                                DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text,
                                    queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());

                                isActorCreated = true;
                            }

                            sqlActor = new SQL_Actor(pCS, identifier)
                            {
                                DbTransaction = dbTransaction
                            };
                            if (false == sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                                throw new NotSupportedException(StrFunc.AppendFormat("Actor (Identifier:{0}) is not found", identifier));

                            item.Second.OTCmlId = sqlActor.Id;
                            item.Second.Identifier = sqlActor.Identifier;

                            if (isActorCreated)
                            {
                                IEnumerable<Int32> exist = (from itemNewTrader in newtrader.Where(x => x.First == item.Second.OTCmlId)
                                                            select itemNewTrader.First);
                                if (!(exist.Count() > 0))
                                    newtrader.Add(new Pair<int, string> { First = item.Second.OTCmlId, Second = item.Second.Identifier });
                            }

                        }
                    }

                    DataHelper.CommitTran(dbTransaction);

                    if (newtrader.Count > 0)
                    {
                        try
                        {
                            Cst.OTCml_TBL[] table = new Cst.OTCml_TBL[] { Cst.OTCml_TBL.ACTOR, Cst.OTCml_TBL.ACTORROLE };
                            DataHelper.queryCache.Remove(ArrFunc.Map<Cst.OTCml_TBL, string>(table, (x) => { return x.ToString(); }), pCS, false);
                            DataEnabledHelper.ClearCache(pCS, table);
                        }
                        catch { }// Ne pas planter si pb lors de suppression dans le cache
                    }
                }
                catch
                {
                    if (null != dbTransaction)
                        DataHelper.RollbackTran(dbTransaction);

                    // Rétablir la situation initiale si une exception se produit
                    foreach (var item in trader)
                        item.Second.OTCmlId = 0;

                    throw;
                }
                finally
                {
                    if (null != dbTransaction)
                    {
                        if (null != dbTransaction)
                            dbTransaction.Dispose();
                    }
                }

                if (newtrader.Count > 0)
                    oTrader = newtrader.ToArray();
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class TradeCaptureGen
    {
        #region Methods
        #region Insert TradeInstrument
        /// <summary>
        /// Alimente les tables TRADEINSTRUMENT/TRADESTREAM pour les sous-product d'une strategie 
        /// </summary>
        /// <param name="pCS">Chaine de connection</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pTradeProduct">product strategy</param>
        /// <param name="pIsUpdateOnly_TradeInstrument">Mise à jour de TRADEINSTRUMENT uniquement</param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, StrategyContainer pTradeProduct, bool pIsUpdateOnly_TradeInstrument)
        {
            foreach (ProductContainer product in pTradeProduct.GetSubProduct())
                InsertTradeInstrument(pCS, pDbTransaction, pIdT, product, pIsUpdateOnly_TradeInstrument);

        }
        /// <summary>
        /// Alimente les tables TRADEINSTRUMENT/TRADESTREAM pour un product donné
        /// </summary>
        /// <param name="pCS">Chaine de connection</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pProduct">sous-product d'une strategie ou swap d'un swaption)</param>
        /// <param name="pIsUpdateOnly_TradeInstrument">Mise à jour de TRADEINSTRUMENT uniquement</param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20230526 [XXXXX] Ajout test sur Swaption pour alimenter TRADESTREAM sur INSTRUMENTNO = 1
        protected override void InsertTradeInstrument(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct, bool pIsUpdateOnly_TradeInstrument)
        {
            base.InsertTradeInstrument(pCS, pDbTransaction, pIdT, pProduct, pIsUpdateOnly_TradeInstrument);
            if (false == pIsUpdateOnly_TradeInstrument)
            {
                if (pProduct.DataDocument.CurrentProduct.IsSwaption && pProduct.IsSwap)
                    InsertTradeStream(pCS, pDbTransaction, pIdT, pProduct.DataDocument.CurrentProduct);
                InsertTradeStream(pCS, pDbTransaction, pIdT, pProduct);
            }
        }
        #endregion Insert TradeInstrument
        #region Insert TradeStream
        /// <summary>
        /// Alimente la table TRADESTREAM pour un produit donné
        /// </summary>
        /// <param name="pCS">Chaine de connection</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pProduct">product</param>
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        protected override void InsertTradeStream(string pCS, IDbTransaction pDbTransaction, int pIdT, ProductContainer pProduct)
        {
            int streamNo = 1;
            int instrumentNo = Convert.ToInt32(pProduct.ProductBase.Id.Replace(Cst.FpML_InstrumentNo, string.Empty));

            if (pProduct.ProductBase.IsBulletPayment)
            {
                IPayment payment = ((IBulletPayment)pProduct.Product).Payment;
                TradeStreamTools.InsertStreamPayment(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo, payment);
            }
            else if (pProduct.ProductBase.IsCapFloor)
            {
                #region CapFloor
                IInterestRateStream stream = ((ICapFloor)pProduct.Product).Stream;
                TradeStreamInfo streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo);
                TradeStreamTools.InsertInterestRateStream(pCS, pDbTransaction, TradeCommonInput, streamInfo, streamNo, stream, null);
                #endregion CapFloor
            }
            else if (pProduct.ProductBase.IsExchangeTradedDerivative)
            {
                #region ExchangeTradedDerivative
                IExchangeTradedDerivative etd = (IExchangeTradedDerivative)pProduct.Product;
                TradeStreamInfo streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo);
                IFixInstrument fixInstrument = etd.TradeCaptureReport.Instrument;
                SQL_DerivativeContract derivativeContract;
                if (StrFunc.IsFilled(fixInstrument.Symbol))
                {
                    // FI 20121004 [18172] add parameter pDbTransaction
                    derivativeContract = ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument(CSTools.SetCacheOn(pCS), RecordSettings.isSaveUnderlyingInParticularTransaction ? null : pDbTransaction, fixInstrument);
                    if (null != derivativeContract)
                        streamInfo.IdC1 = derivativeContract.PriceCurrency;
                }
                if (fixInstrument.PutOrCallSpecified)
                    streamInfo.OptionType = fixInstrument.PutOrCall.ToString();


                // FI 20130701 [18798] 
                // etd.buyerPartyReference et etd.sellerPartyReference peuvent parfois être null.
                // C'est le cas lors de la sauvegarde d'un trade MISSING où Dealer et Clearer sont non renseignés
                // Bizarrement Cela n'était pas systématiquement reproductible
                // Add du test sur null != etd.buyerPartyReference 
                // Add du test sur null != etd.sellerPartyReference
                string buyerId = string.Empty;
                if (null != etd.BuyerPartyReference)
                    buyerId = etd.BuyerPartyReference.HRef;

                string sellerId = string.Empty;
                if (null != etd.SellerPartyReference)
                    sellerId = etd.SellerPartyReference.HRef;

                streamInfo.SetIdParty(TradeCommonInput, buyerId, sellerId);
                streamInfo.Insert(pCS, pDbTransaction);
                #endregion ExchangeTradedDerivative
            }
            else if (pProduct.ProductBase.IsEquitySecurityTransaction)
            {
                #region EquitySecurityTransaction
                IEquitySecurityTransaction equitySecurityTransaction = (IEquitySecurityTransaction)pProduct.Product;
                TradeStreamInfo streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo)
                {
                    IdC1 = equitySecurityTransaction.GrossAmount.PaymentCurrency
                };
                streamInfo.SetIdParty(TradeCommonInput, equitySecurityTransaction.BuyerPartyReference.HRef, equitySecurityTransaction.SellerPartyReference.HRef);
                streamInfo.Insert(pCS, pDbTransaction);
                #endregion EquitySecurityTransaction
            }
            else if (pProduct.ProductBase.IsFra)
            {
                #region Fra
                IFra fra = (IFra)pProduct.Product;
                TradeStreamTools.InsertStreamShortForm(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo,
                    fra.BuyerPartyReference, fra.SellerPartyReference, fra.Notional.Currency, null);
                #endregion Fra
            }
            else if (pProduct.ProductBase.IsFxLeg)
            {
                #region FxLeg
                IFxLeg leg = (IFxLeg)pProduct.Product;
                TradeStreamTools.InsertStreamFxSingleLeg(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo, leg);
                #endregion FxLeg
            }
            else if (pProduct.ProductBase.IsFxOption)
            {
                #region FxOption
                IFxOptionBase fxOption = (IFxOptionBase)pProduct.Product;
                if (pProduct.ProductBase.IsFxDigitalOption)
                {
                    IFxOptionPayout fxOptionPayout = ((IFxDigitalOption)pProduct.Product).TriggerPayout;
                    TradeStreamTools.InsertStreamShortForm(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo,
                        fxOption.BuyerPartyReference, fxOption.SellerPartyReference, fxOptionPayout.Currency, null);
                }
                else
                {
                    #region Others FxOption
                    TradeStreamInfo streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo);
                    if (fxOption.OptionTypeSpecified)
                    {
                        string callCurrency = fxOption.CallCurrencyAmount.Currency;
                        string putCurrency = fxOption.PutCurrencyAmount.Currency;
                        streamInfo.OptionType = fxOption.OptionType.ToString();
                        if (OptionTypeEnum.Call == fxOption.OptionType)
                        {
                            streamInfo.IdC1 = callCurrency;
                            streamInfo.IdC2 = putCurrency;
                        }
                        else if (OptionTypeEnum.Put == fxOption.OptionType)
                        {
                            streamInfo.IdC1 = putCurrency;
                            streamInfo.IdC2 = callCurrency;
                        }
                    }
                    streamInfo.SetIdParty(TradeCommonInput, fxOption.BuyerPartyReference.HRef, fxOption.SellerPartyReference.HRef);
                    streamInfo.Insert(pCS, pDbTransaction);
                    #endregion Others FxOption
                }
                #endregion FxOption
            }
            else if (pProduct.ProductBase.IsFxSwap)
            {
                #region FxSwap
                IFxLeg[] legs = ((IFxSwap)pProduct.Product).FxSingleLeg;
                TradeStreamTools.InsertStreamFxSingleLegs(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, legs);
                #endregion FxSwap
            }
            else if (pProduct.ProductBase.IsFxTermDeposit)
            {
                #region TermDeposit
                ITermDeposit termDeposit = (ITermDeposit)pProduct.Product;
                TradeStreamTools.InsertStreamShortForm(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo,
                    termDeposit.InitialPayerReference, termDeposit.InitialReceiverReference, termDeposit.Principal.Currency, null);
                #endregion TermDeposit
            }
            else if (pProduct.ProductBase.IsLoanDeposit)
            {
                #region LoanDeposit
                IInterestRateStream[] streams = ((ILoanDeposit)pProduct.Product).Stream;
                TradeStreamTools.InsertInterestRateStreams(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, streams);
                #endregion LoanDeposit
            }
            else if (pProduct.ProductBase.IsSwap)
            {
                #region Swap
                IInterestRateStream[] streams = ((ISwap)pProduct.Product).Stream;
                TradeStreamTools.InsertInterestRateStreams(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, streams);
                #endregion Swap
            }
            else if (pProduct.ProductBase.IsSwaption)
            {
                #region Swaption
                ISwaption swaption = (ISwaption)pProduct.Product;
                TradeStreamInfoShortForm streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo);
                streamInfo.SetIdParty(TradeCommonInput, swaption.BuyerPartyReference.HRef, swaption.SellerPartyReference.HRef);
                streamInfo.Insert(pCS, pDbTransaction);
                #endregion Swaption
            }
            else if (pProduct.ProductBase.IsEQD)
            {
                #region EquityOption / EquityOptionTransactionSupplement
                IEquityDerivativeBase equityDerivativeBase = (IEquityDerivativeBase)pProduct.Product;
                TradeStreamInfo streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo);
                if (equityDerivativeBase.NotionalSpecified)
                    streamInfo.IdC1 = equityDerivativeBase.Notional.Currency;
                streamInfo.OptionType = equityDerivativeBase.OptionType.ToString();
                streamInfo.SetIdParty(TradeCommonInput, equityDerivativeBase.BuyerPartyReference.HRef, equityDerivativeBase.SellerPartyReference.HRef);
                streamInfo.Insert(pCS, pDbTransaction);
                #endregion EquityOption / EquityOptionTransactionSupplement
            }
            else if (pProduct.IsBondOption)
            {
                #region BondOption
                IBondOption bondOption = (IBondOption)pProduct.Product;
                TradeStreamInfo streamInfo = new TradeStreamInfo(pIdT, instrumentNo, streamNo);
                if (bondOption.NotionalAmountSpecified)
                    streamInfo.IdC1 = bondOption.NotionalAmount.Currency;
                streamInfo.OptionType = bondOption.OptionType.ToString();
                streamInfo.SetIdParty(TradeCommonInput, bondOption.BuyerPartyReference.HRef, bondOption.SellerPartyReference.HRef);
                streamInfo.Insert(pCS, pDbTransaction);
                #endregion BondOption
            }
            else if (pProduct.ProductBase.IsDebtSecurityTransaction)
            {
                #region DebtSecurityTransaction/grossAmount
                IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)pProduct.Product;
                TradeStreamTools.InsertStreamPayment(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, streamNo, debtSecurityTransaction.GrossAmount);
                #endregion grossAmount
            }
            else if ((pProduct.ProductBase.IsRepo) || (pProduct.ProductBase.IsBuyAndSellBack))
            {
                #region SaleAndRepurchaseAgreement
                ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = ((ISaleAndRepurchaseAgreement)pProduct.Product);
                IInterestRateStream[] streams = (IInterestRateStream[])saleAndRepurchaseAgreement.CashStream;
                TradeStreamTools.InsertInterestRateStreams(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, streams);

                //SpotLegs
                ArrayList al = new ArrayList();
                for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.SpotLeg); i++)
                    al.Add(saleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.GrossAmount);
                //
                if (ArrFunc.IsFilled(al))
                {
                    IPayment[] payment = (IPayment[])al.ToArray(typeof(IPayment));
                    TradeStreamTools.InsertStreamPayments(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, payment);
                }

                //ForwardLeg
                if (saleAndRepurchaseAgreement.ForwardLegSpecified && ArrFunc.IsFilled(saleAndRepurchaseAgreement.ForwardLeg))
                {
                    al = new ArrayList();
                    for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.ForwardLeg); i++)
                        al.Add(saleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.GrossAmount);
                    //
                    if (ArrFunc.IsFilled(al))
                    {
                        IPayment[] payment = (IPayment[])al.ToArray(typeof(IPayment));
                        TradeStreamTools.InsertStreamPayments(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, payment);
                    }
                }
                //
                #endregion SaleAndRepurchaseAgreement
            }
            else if (pProduct.ProductBase.IsReturnSwap || pProduct.ProductBase.IsEquitySwapTransactionSupplement)
            {
                #region ReturnSwap
                IReturnSwapBase returnSwapBase = pProduct.Product as IReturnSwapBase;
                TradeStreamTools.InsertStreamReturnSwapLegs(pCS, pDbTransaction, TradeCommonInput, pIdT, instrumentNo, ref streamNo, returnSwapBase.ReturnSwapLeg);
                #endregion Swap
            }
            else if (pProduct.ProductBase.IsCommoditySpot)
            {
                #region CommoditySpot
                ICommoditySpot commoditySpot = pProduct.Product as ICommoditySpot;

                // IFixedSpotLeg
                TradeStreamInfo tradeStreamInfo = new TradeStreamInfo(pIdT, instrumentNo, 1);
                IPayerReceiverPartyAccountReference party = commoditySpot.FixedLeg as IPayerReceiverPartyAccountReference;
                tradeStreamInfo.SetIdParty(TradeCommonInput, party.PayerPartyReference.HRef, party.ReceiverPartyReference.HRef);
                tradeStreamInfo.IdC1 = commoditySpot.SettlementCurrency.Value;
                tradeStreamInfo.Insert(pCS, pDbTransaction);
                tradeStreamInfo = new TradeStreamInfo(pIdT, instrumentNo, 2);
                party = commoditySpot.PhysicalLeg as IPayerReceiverPartyAccountReference;
                tradeStreamInfo.SetIdParty(TradeCommonInput, party.PayerPartyReference.HRef, party.ReceiverPartyReference.HRef);
                tradeStreamInfo.Insert(pCS, pDbTransaction);
                #endregion CommoditySpot
            }
            else if (pProduct.ProductBase.IsStrategy)
            {
                #region Strategy
                //RAS PL 20091216
                #endregion Strategy
            }
            else
            {
                throw new Exception("Error, Current product is not managed, please contact EFS");
            }
        }
        #endregion Insert TradeStream
        #endregion
    }

    // EG 20180514 [23812] Report
    public sealed partial class TradeCaptureGen
    {
        // EG 20180315 [23812] Step2
        private class FxProvisionInfo
        {
            #region Members
            public int idE;
            public int idEParent;
            public AmountPayerReceiverInfo callAmount;
            public AmountPayerReceiverInfo putAmount;
            public string exerciseEventType;
            #endregion Members
            #region Constructors
            public FxProvisionInfo() { }
            #endregion Constructors
        }
        /// <summary>
        /// 1. Construction du message de résiliation FX option
        /// 2. Postage d'un message TradeAction (incluant le message de résiliation) au service concerné
        /// </summary>
        /// <returns>IdTRK_L : Identifiant du tracker</returns>
        // EG 20180315 [23812] Step2
        private int FxOptionEarlyTerminationMessage(string pCS)
        {
            int idTRK_L = 0;
            TradeFxOptionEarlyTermination fxOET = Input.fxOptionEarlyTermination;
            fxOET.SetPayerReceiver(Input.DataDocument);
            _ = fxOET.actionDate.DateValue.Date;

            FxProvisionInfo fxProvisionInfo = GetFxProvisionInfo(pCS);
            if (null != fxProvisionInfo)
            {
                CashSettlementProvisionMsg settlementMsg = new CashSettlementProvisionMsg
                {
                    amount = fxOET.cashSettlement.Amount.DecValue,
                    currency = fxOET.cashSettlement.Currency,

                    idA_PayerSpecified = true,
                    idA_Payer = fxOET.IdPayer.First,
                    idB_PayerSpecified = fxOET.IdPayer.Second.HasValue,
                    idA_ReceiverSpecified = true,
                    idA_Receiver = fxOET.IdReceiver.First,
                    idB_ReceiverSpecified = fxOET.IdReceiver.Second.HasValue,
                    cashSettlementPaymentDate = fxOET.settlementDate.DateValue
                };
                if (settlementMsg.idB_PayerSpecified)
                    settlementMsg.idB_Payer = fxOET.IdPayer.Second.Value;

                if (settlementMsg.idB_ReceiverSpecified)
                    settlementMsg.idB_Receiver = fxOET.IdReceiver.Second.Value;


                FxOptionalEarlyTerminationProvisionMsg msg = new FxOptionalEarlyTerminationProvisionMsg(fxProvisionInfo.idE,
                    fxOET.actionDate.DateValue, fxProvisionInfo.exerciseEventType, fxOET.valueDate.DateValue,
                    fxProvisionInfo.callAmount, fxProvisionInfo.putAmount, settlementMsg, fxOET.note, "FXOET");

                idTRK_L = SendMessageFXProvision(pCS, fxProvisionInfo.idE, fxProvisionInfo.idEParent, msg);
            }
            return idTRK_L;
        }
        /// <summary>
        /// Construction des informations liées aux événements FX options pour résiliation
        /// Calcul et retourne l'élément FxProvisionInfo
        /// 1. idE et IdE_Event de l'événement PRO|OET
        /// 2. CallCurrencyAmount et PutCurrencyAmount disponibles pour la provision (avec leurs payers et receivers de type : AmountPayerReceiverInfo)
        /// 3. Type d'exercice en fonction des montants disponibles (TOT|PAR)
        /// </summary>
        /// <returns></returns>
        // EG 20180315 [23812] Step2
        private FxProvisionInfo GetFxProvisionInfo(string pCS)
        {
            FxProvisionInfo fxProvisionInfo = null;
            DataSetEventTrade dsEvents = new DataSetEventTrade(pCS, Input.IdT);
            dsEvents.Load();
            DataRow rowEventParent = dsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE= '{0}' and  EVENTTYPE='{1}'",
                EventCodeFunc.Provision, EventTypeFunc.OptionalEarlyTerminationProvision)).FirstOrDefault();

            if (null != rowEventParent)
            {
                bool isFxDigitalOption = Input.DataDocument.CurrentProduct.Product.ProductBase.IsFxDigitalOption;

                TradeFxOptionEarlyTermination fxOET = Input.fxOptionEarlyTermination;
                IFxOptionBase fxOption = Input.DataDocument.CurrentProduct.Product as IFxOptionBase;

                IParty partyBuyer = Input.DataDocument.GetParty(fxOption.BuyerPartyReference.HRef);
                IBookId bookBuyer = Input.DataDocument.GetBookId(fxOption.BuyerPartyReference.HRef);

                IParty partySeller = Input.DataDocument.GetParty(fxOption.SellerPartyReference.HRef);
                IBookId bookSeller = Input.DataDocument.GetBookId(fxOption.SellerPartyReference.HRef);

                #region Set Result
                fxProvisionInfo = new FxProvisionInfo
                {
                    // idE et idEParent de PRO|OET
                    idE = Convert.ToInt32(rowEventParent["IDE"]),
                    idEParent = Convert.ToInt32(rowEventParent["IDE_EVENT"])
                };

                if (isFxDigitalOption)
                {
                    fxProvisionInfo.exerciseEventType = EventTypeFunc.Total;
                }
                else
                {
                    DataRow rowSTA_CCU = dsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE = '{0}' and  EVENTTYPE = '{1}'", EventCodeFunc.Start, EventTypeFunc.CallCurrency)).FirstOrDefault();
                    DataRow[] rowINT_CCU = dsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE = '{0}' and  EVENTTYPE = '{1}'", EventCodeFunc.Intermediary, EventTypeFunc.CallCurrency));
                    DataRow rowSTA_PCU = dsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE = '{0}' and  EVENTTYPE = '{1}'", EventCodeFunc.Start, EventTypeFunc.PutCurrency)).FirstOrDefault();
                    DataRow[] rowINT_PCU = dsEvents.DtEvent.Select(StrFunc.AppendFormat("EVENTCODE = '{0}' and  EVENTTYPE = '{1}'", EventCodeFunc.Intermediary, EventTypeFunc.PutCurrency));

                    Pair<decimal, string> amountSTA_CCU = new Pair<decimal, string>(Convert.ToDecimal(rowSTA_CCU["VALORISATION"]), rowSTA_CCU["UNIT"].ToString());
                    Pair<decimal, string> amountSTA_PCU = new Pair<decimal, string>(Convert.ToDecimal(rowSTA_PCU["VALORISATION"]), rowSTA_PCU["UNIT"].ToString());

                    Pair<decimal, string> currentAmount_CCU = new Pair<decimal, string>(
                        amountSTA_CCU.First - (from row in rowINT_CCU select Convert.ToDecimal(row["VALORISATION"])).Sum(), amountSTA_CCU.Second);

                    Pair<decimal, string> currentAmount_PCU = new Pair<decimal, string>(
                        amountSTA_PCU.First - (from row in rowINT_PCU select Convert.ToDecimal(row["VALORISATION"])).Sum(), amountSTA_PCU.Second);

                    fxProvisionInfo.exerciseEventType = (amountSTA_CCU.First == currentAmount_CCU.First) ? EventTypeFunc.Total : EventTypeFunc.Partiel;
                    // Buyer dans Payer 
                    PayerReceiverInfoDet payer = new PayerReceiverInfoDet(PayerReceiverEnum.Payer,
                            partyBuyer.OTCmlId, partyBuyer.PartyName, bookBuyer.OTCmlId, bookBuyer.BookName);
                    // Seller dans Receiver
                    PayerReceiverInfoDet receiver = new PayerReceiverInfoDet(PayerReceiverEnum.Receiver,
                            partySeller.OTCmlId, partySeller.PartyName, bookSeller.OTCmlId, bookSeller.BookName);

                    fxProvisionInfo.callAmount = new AmountPayerReceiverInfo(null, currentAmount_CCU.First, currentAmount_CCU.Second, receiver, payer);
                    fxProvisionInfo.putAmount = new AmountPayerReceiverInfo(null, currentAmount_PCU.First, currentAmount_PCU.Second, payer, receiver);
                }
                #endregion Set Result
            }
            return fxProvisionInfo;
        }
        /// <summary>
        /// Préparation et postage du message de résiliation d'une FXOption au service SpheresTradeActionGen
        /// </summary>
        /// <param name="pIdEParent">IdE de l'événement PRO|OET</param>
        /// <param name="pIdEGrandParent">IdE_Event de l'événement (PRO|OET)</param>
        /// <param name="pMsg">Message d'action de résiliation de Fx Option</param>
        /// <returns></returns>
        // EG 20180315 [23812] Step2
        private int SendMessageFXProvision(string pCS, int pIdEParent, int pIdEGrandParent, FxOptionalEarlyTerminationProvisionMsg pMsg)
        {


            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = pCS,
                id = Input.IdT,
                idInfo = new IdInfo
                {
                    id = Input.IdT,
                    idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", Input.SQLProduct.GProduct) }
                }
            };

            TradeActionGenMQueue taMQueue = new TradeActionGenMQueue(mQueueAttributes)
            {
                item = new TradeActionBaseMQueue[1] { new TradeActionMQueue(pIdEParent, pIdEGrandParent,
                EventCodeFunc.ExerciseOptionalEarlyTermination, TradeActionCode.TradeActionCodeEnum.OptionalEarlyTerminationProvision) }
            };
            taMQueue.item[0].ActionMsgs = new FxOptionalEarlyTerminationProvisionMsg[1] { pMsg };
            taMQueue.identifierSpecified = true;
            taMQueue.identifier = Input.Identifier;


            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                connectionString = pCS,
                Session = SessionTools.AppSession,
                idInfo = new IdInfo[1] { mQueueAttributes.idInfo },
                mQueue = new MQueueBase[] { taMQueue },
                process = taMQueue.ProcessType,
                trackerAttrib = new TrackerAttributes()
                {
                    process = taMQueue.ProcessType,
                    gProduct = Input.SQLProduct.GProduct,
                    caller = TradeActionType.GetMenuActionType(TradeActionType.TradeActionTypeEnum.OptionalEarlyTerminationProvisionEvents),
                    info = new List<DictionaryEntry>()
                    {
                        new DictionaryEntry("IDDATA", Input.IdT),
                        new DictionaryEntry("IDDATAIDENT", Cst.OTCml_TBL.TRADE.ToString()),
                        new DictionaryEntry("IDDATAIDENTIFIER", Input.Identifier)
                    }
                }
            };
            taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);

            int idTRK_L = 0;
            MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);

            return idTRK_L;
        }

        /// <summary>
        /// Enregistrement de la demande de résiliation d'une FX option
        /// 1. Préparation et envoi du message au service concerné
        /// 2. Mise à jour TRADE (STATUS RESERVED)
        /// 3. Mise à jour TRADETRAIL
        /// </summary>
        /// <returns></returns>
        // EG 20180315 [23812] Step2
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        public override bool RecordFxOptionEarlyTermination(string pCS, string pIdScreen)
        {
            bool isOk;

            int idTRK_L = FxOptionEarlyTerminationMessage(pCS);
            isOk = (0 < idTRK_L);

            // FI 20200820 [XXXXXX] dates systèmes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);

            CaptureSessionInfo sessionInfo = new CaptureSessionInfo
            {
                user = SessionTools.User,
                session = SessionTools.AppSession,
                licence = SessionTools.License,
                idTracker_L = idTRK_L
            };


            if (isOk)
            {
                using (IDbTransaction dbTransaction = DataHelper.BeginTran(pCS))
                {
                    try
                    {
                        Pair<Cst.StatusUsedBy, string> stUsedBy = new Pair<Cst.StatusUsedBy, string>(Cst.StatusUsedBy.RESERVED, "XX");
                        UpdateTradeStSys(pCS, dbTransaction, Input.IdT, sessionInfo, dtSys, stUsedBy);
                        SaveTradeTrail(pCS, dbTransaction, Input.IdT, sessionInfo, pIdScreen, Cst.Capture.ModeEnum.FxOptionEarlyTermination,  dtSys);
                        DataHelper.CommitTran(dbTransaction);
                    }
                    catch
                    {
                        if (DataHelper.IsTransactionValid(dbTransaction))
                            DataHelper.RollbackTran(dbTransaction);
                        throw;
                    }
                }
            }

            return isOk;
        }
    }
}


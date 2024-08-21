#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciFXLeg. 
    /// </summary>
    public class CciFXLeg : IContainerCciFactory, IContainerCci, IContainerCciQuoteBasis, ICciPresentation 
    {
        #region Members
        private TypeFxEnum typeFx;
        private readonly CciTradeBase cciTrade;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly string prefix;
        private readonly int number;
        private readonly IFxLeg fxleg;
        private CciFxFixing[] cciFixingNDF;	     //Represente les n fixings si NDF
        private readonly CciExchangeRate cciExchangeRate;
        private readonly CciPayment cciPaymentCur1;
        private readonly CciPayment cciPaymentCur2;
        #endregion Members

        #region Enum
        public enum CciEnum
        {
            #region valueDate
            [System.Xml.Serialization.XmlEnumAttribute("fxDateValueDate")]
            valueDate,
            #endregion valueDate
            #region nonDeliverableForward
            [System.Xml.Serialization.XmlEnumAttribute("nonDeliverableForward.settlementCurrency")]
            nonDeliverableForward_settlementCurrency,
            #endregion nonDeliverableForward
            #region calculationAgentSettlementRateSpecified
            //PL 20100628 calculationAgent
            [System.Xml.Serialization.XmlEnumAttribute("nonDeliverableForward.calculationAgentSettlementRate")]
            nonDeliverableForward_calculationAgentSettlementRateSpecified,
            #endregion calculationAgentSettlementRateSpecified
            unknown
        };
        public enum TypeFxEnum
        {
            SPOT = 1,
            FORWARD = 2
        };
        public enum CurrencyEnum
        {
            Currency1,
            Currency2
        };
        #endregion Enum

        #region public property
        #region numberPrefix
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < number)
                    ret = number.ToString();
                return ret;
            }
        }
        #endregion numberPrefix

        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        public TypeFxEnum TypeFx { get { return typeFx; } }
        //
        public CciPayment CciPaymentCur1 { get { return cciPaymentCur1; } }
        public CciPayment CciPaymentCur2 { get { return cciPaymentCur2; } }
        public CciExchangeRate CciExchangeRate { get { return cciExchangeRate; } }
        //
        public bool IsSideRatesSpecified { get { return fxleg.ExchangeRate.SideRatesSpecified; } }
        public bool IsSpotFixing { get { return cciExchangeRate.IsSpotFixing; } }
        public bool CciCountainsNDF
        {
            get { return (Ccis.Contains(CciClientId(CciEnum.nonDeliverableForward_settlementCurrency)) && (TypeFxEnum.FORWARD == TypeFx)); }
        }
        public int FxNonDeliverableForwardFixingLength
        {
            get { return (CciCountainsNDF && ArrFunc.IsFilled(cciFixingNDF)) ? cciFixingNDF.Length : 0; }
        }
        public bool IsCaptureFilled
        {
            get
            {
                bool ret = CciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).IsFilledValue &&
                      CciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).IsFilledValue;
                return ret && cciExchangeRate.IsExchangeRateFilled;
            }
        }

        public IMoney ExchangedCurrency1
        {
            get
            {
                return ((IPayment)(cciPaymentCur1.Payment)).PaymentAmount;
            }
        }
        public IMoney ExchangedCurrency2
        {
            get
            {
                return ((IPayment)(cciPaymentCur2.Payment)).PaymentAmount;
            }
        }

        #endregion  accessor

        #region Constructor
        public CciFXLeg(CciTrade pCciTrade, string pPrefix, int pLegNumber, IFxLeg pFxleg)
        {
            cciTrade = pCciTrade;
            typeFx = TypeFxEnum.SPOT;
            fxleg = pFxleg;
            _ccis = (TradeCustomCaptureInfos)pCciTrade.Ccis;
            //
            if (pLegNumber > 0)
                number = pLegNumber;
            //
            prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            // 
            if (null != pFxleg)
            {
                if (null == pFxleg.ExchangedCurrency1)
                    pFxleg.ExchangedCurrency1 = pFxleg.CreatePayment;
                if (null == pFxleg.ExchangedCurrency1)
                    pFxleg.ExchangedCurrency2 = pFxleg.CreatePayment;
                if (null == pFxleg.ExchangeRate)
                    pFxleg.ExchangeRate = pFxleg.CreateExchangeRate;
                //
                cciPaymentCur1 = new CciPayment(pCciTrade, -1, pFxleg.ExchangedCurrency1, CciPayment.PaymentTypeEnum.Payment, prefix + TradeCustomCaptureInfos.CCst.Prefix_exchangedCurrency1);
                cciPaymentCur2 = new CciPayment(pCciTrade, -1, pFxleg.ExchangedCurrency2, CciPayment.PaymentTypeEnum.Payment, prefix + TradeCustomCaptureInfos.CCst.Prefix_exchangedCurrency2);
                cciExchangeRate = new CciExchangeRate(pCciTrade, pFxleg.ExchangeRate, prefix + TradeCustomCaptureInfos.CCst.Prefix_exchangeRate);
            }
        }
        #endregion Constructor

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            //			
            CciTools.CreateInstance(this, fxleg);
            //
            typeFx = GetTypeFx();
            //
            cciPaymentCur1.Initialize_FromCci();
            cciPaymentCur2.Initialize_FromCci();
            cciExchangeRate.Initialize_FromCci();
            //
            if (CciCountainsNDF)
            {
                FieldInfo fld = fxleg.GetType().GetField("nonDeliverableForward");
                IFxCashSettlement fxCashSettlement = (IFxCashSettlement)fld.GetValue(fxleg);
                if (null == fxCashSettlement)
                    fxCashSettlement = (IFxCashSettlement)fld.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                // ajout d'au moins 1 fixing 
                if (null == fxCashSettlement.Fixing)
                {
                    fxCashSettlement.Initialize();
                    fld.SetValue(fxleg, fxCashSettlement);
                }
                //
                InitializeFxCashSettlementFixing();
            }
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// </summary>
        public void AddCciSystem()
        {
            //		    
            cciPaymentCur1.AddCciSystem();
            cciPaymentCur2.AddCciSystem();
            // 
            cciExchangeRate.AddCciSystem();
            //
            for (int i = 0; i < this.FxNonDeliverableForwardFixingLength; i++)
                cciFixingNDF[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {
            //
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region valueDate
                        case CciEnum.valueDate:
                            data = fxleg.FxDateValueDate.Value;
                            break;
                        #endregion valueDate
                        #region nonDeliverableForward
                        case CciEnum.nonDeliverableForward_settlementCurrency:
                            if (fxleg.NonDeliverableForwardSpecified)
                                data = fxleg.NonDeliverableForward.SettlementCurrency.Value;
                            break;
                        #endregion nonDeliverableForward
                        #region calculationAgentSettlementRate
                        //case CciEnum.nonDeliverableForward_customerSettlementRateSpecified:
                        case CciEnum.nonDeliverableForward_calculationAgentSettlementRateSpecified:
                            //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
                            if (fxleg.NonDeliverableForwardSpecified)
                            {
                                //data = fxleg.nonDeliverableForward.customerSettlementRateSpecified.ToString().ToLower();
                                if (fxleg.NonDeliverableForward.CustomerSettlementRateSpecified
                                    ||
                                    fxleg.NonDeliverableForward.CalculationAgentSettlementRateSpecified)
                                    data = true.ToString().ToLower();
                                else
                                    data = false.ToString().ToLower();
                            }
                            break;
                        #endregion calculationAgentSettlementRate
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            cciPaymentCur1.Initialize_FromDocument();
            cciPaymentCur2.Initialize_FromDocument();
            cciExchangeRate.Initialize_FromDocument();
            //

            // Permet d'initialser les devises en mode creation
            CustomCaptureInfo cciCur = cciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_currency);
            if ((null != cciCur) && cciCur.IsEmpty)
                cciCur.NewValue = CciTradeCommonBase.GetDefaultCurrency();
            cciCur = cciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_currency);
            if ((null != cciCur) && cciCur.IsEmpty)
                cciCur.NewValue = CciTradeCommonBase.GetDefaultCurrency();

            //
            for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                cciFixingNDF[i].Initialize_FromDocument();
        }
        #endregion
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion

                    switch (cciEnum)
                    {
                        #region ValueDate
                        case CciEnum.valueDate:
                            fxleg.FxDateValueDateSpecified = StrFunc.IsFilled(data);
                            fxleg.FxDateValueDate.Value = data;
                            
                            fxleg.FxDateCurrency1ValueDateSpecified = false;
                            fxleg.FxDateCurrency2ValueDateSpecified = false;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;
                        #endregion
                        #region cashSettlementTerms
                        case CciEnum.nonDeliverableForward_settlementCurrency:
                            fxleg.NonDeliverableForwardSpecified = cci.IsFilledValue;
                            fxleg.NonDeliverableForward.SettlementCurrency.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion cashSettlementTerms
                        #region calculationAgentSettlementRate
                        case CciEnum.nonDeliverableForward_calculationAgentSettlementRateSpecified:
                            if (fxleg.NonDeliverableForwardSpecified)
                            {
                                //PL 20100628 
                                //fxleg.nonDeliverableForward.customerSettlementRateSpecified = cci.IsFilledValue;
                                fxleg.NonDeliverableForward.CalculationAgentSettlementRateSpecified = cci.IsFilledValue;
                            }
                            break;
                        #endregion calculationAgentSettlementRate

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
                //
            }
            //
            cciPaymentCur1.Dump_ToDocument();
            cciPaymentCur2.Dump_ToDocument();
            //
            if (StrFunc.IsEmpty(ExchangedCurrency1.Id))
                ExchangedCurrency1.Id = TradeCustomCaptureInfos.CCst.EXCHANGECURRENCY1_REFERENCE + number;
            if (StrFunc.IsEmpty(ExchangedCurrency2.Id))
                ExchangedCurrency2.Id = TradeCustomCaptureInfos.CCst.EXCHANGECURRENCY2_REFERENCE + number;
            //
            if (false == cciExchangeRate.IsSpotFixing)
            {
                //Initialiation des devises de quotedCurrencyPair en fonction des valeurs des devises des objects exchangedCurrency 
                Ccis.SetNewValue(cciExchangeRate.CciClientId(CciExchangeRate.CciEnum.quotedCurrencyPair_currency1), GetExchangedCurrency(CurrencyEnum.Currency1).PaymentAmount.Currency);
                Ccis.SetNewValue(cciExchangeRate.CciClientId(CciExchangeRate.CciEnum.quotedCurrencyPair_currency2), GetExchangedCurrency(CurrencyEnum.Currency2).PaymentAmount.Currency);
            }
            cciExchangeRate.Dump_ToDocument();
            //
            for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                cciFixingNDF[i].Dump_ToDocument();
            //
        }
        #endregion
        #region ProcessInitialize
        public void     ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);

                CciCompare[] ccic = null;
                switch (elt)
                {
                    #region valueDate
                    case CciEnum.valueDate:
                        if (IsSpotFixing)
                            Ccis.SetNewValue(cciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.fixingDate), pCci.NewValue);
                        break;
                    #endregion valueDate
                    #region NonDeliverableForward
                    case CciEnum.nonDeliverableForward_settlementCurrency:
                        if (fxleg.NonDeliverableForwardSpecified)
                        {
                            // Glop FI
                            //SetCashSettlementFixingCurrency(true); 
                            //SetCashSettlementFixingCurrency(false); 
                            //SetCashSettlementFixingQuoteBasis(); 
                        }
                        else
                        {
                            for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                                cciFixingNDF[i].Clear();
                        }
                        break;
                    #endregion NonDeliverableForward
                    #region default
                    default:

                        break;
                    #endregion default
                }
                //
                cciExchangeRate.ProcessInitialize(pCci);
                //
                if (cciExchangeRate.IsCci(CciExchangeRate.CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                    CalculRateFromAmount();
                //
                #region Cas Taux
                if (cciExchangeRate.IsCci(CciExchangeRate.CciEnum.rate, pCci) && pCci.IsFilledValue)
                {
                    if (!IsCaptureFilled)
                    {
                        // Mise en comment pour revenir en arrière potentiellement
                        //	ccic = new CciCompare[2] { 
                        //							 new CciCompare("AmountCur2", cciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount),1), 
                        //							 new CciCompare("Rate" ,Cci(CciEnum.exchangeRate_rate),2)};
                        // Le calcul du rate entraîne le recalcul de la contre-valeur si CTRValue = 0
                        if (cciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).IsEmptyValue)
                            ccic = new CciCompare[] { new CciCompare("AmountCur2", cciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount), 1) };
                        else if (cciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).IsEmptyValue)
                            ccic = new CciCompare[] { new CciCompare("AmountCur1", cciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_amount), 1) };
                    }
                    //
                    if (ArrFunc.IsFilled(ccic))
                    {
                        Array.Sort(ccic);
                        //
                        switch (ccic[0].key)
                        {
                            case "AmountCur1":
                                CalculExchange(CurrencyEnum.Currency1);
                                break;
                            case "AmountCur2":
                                CalculExchange(CurrencyEnum.Currency2);
                                break;
                            case "Rate":
                                CalculRateFromAmount();
                                fxleg.ExchangeRate.Rate.Value = cciExchangeRate.Cci(CciExchangeRate.CciEnum.rate).NewValue;
                                break;
                        }
                    }
                }
                #endregion
                //
                #region Cas Montant
                if (!IsSpotFixing && pCci.IsFilledValue)
                {
                    if (cciPaymentCur1.IsCciClientId(CciPayment.CciEnumPayment.paymentAmount_amount, pCci.ClientId_WithoutPrefix)
                        || cciPaymentCur2.IsCciClientId(CciPayment.CciEnumPayment.paymentAmount_amount, pCci.ClientId_WithoutPrefix))
                    {
                        //
                        bool isFromAmountCur1 = cciPaymentCur1.IsCci(CciPayment.CciEnumPayment.paymentAmount_amount, pCci);
                        string clientIdSource = pCci.ClientId_WithoutPrefix;
                        //
                        string clientIdTarget;
                        if (isFromAmountCur1)
                            clientIdTarget = cciPaymentCur2.CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount);
                        else
                            clientIdTarget = cciPaymentCur1.CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount);
                        //								
                        //						if (!IsCaptureFilled)
                        //						{
                        //							// Mise en comment pour revenir en arrière potentiellement
                        //							ccic = new CciCompare[3] { 
                        //														 new CciCompare("AmountTarget",ccis[clientIdTarget],1), 
                        //														 new CciCompare("Rate" ,Cci(CciEnum.exchangeRate_rate),2),
                        //														 new CciCompare("AmountSource" ,ccis[clientIdSource],3)
                        //														 };
                        //						}
                        //
                        if (((!IsCaptureFilled) || isFromAmountCur1))
                        {
                            if (isFromAmountCur1 && cciExchangeRate.Cci(CciExchangeRate.CciEnum.rate).IsFilledValue)
                                ccic = new CciCompare[] { new CciCompare("AmountTarget", Ccis[clientIdTarget], 1) };
                            else
                                ccic = new CciCompare[] {   new CciCompare("AmountTarget",Ccis[clientIdTarget],1), 
															new CciCompare("Rate" ,cciExchangeRate.Cci(CciExchangeRate.CciEnum.rate),2)};
                        }
                        //
                        if (ArrFunc.IsFilled(ccic))
                        {
                            Array.Sort(ccic);
                            CurrencyEnum currencyEnum;
                            //
                            switch (ccic[0].key)
                            {
                                case "AmountSource":
                                    // Cas particulier on L'utilisateur modifie un champs Montant et c'est ce dernier qui est racalculer => Mise à jour 
                                    if (clientIdSource == CciPaymentCur2.CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount))
                                        currencyEnum = CurrencyEnum.Currency2;
                                    else
                                        currencyEnum = CurrencyEnum.Currency1;
                                    //
                                    CalculExchange(currencyEnum);
                                    //
                                    if (isFromAmountCur1)
                                        fxleg.ExchangedCurrency1.PaymentAmount.Amount.Value = CciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).NewValue;
                                    else
                                        fxleg.ExchangedCurrency2.PaymentAmount.Amount.Value = CciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).NewValue;
                                    break;
                                case "AmountTarget":
                                    if (clientIdTarget == cciPaymentCur2.CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount))
                                        currencyEnum = CurrencyEnum.Currency2;
                                    else
                                        currencyEnum = CurrencyEnum.Currency1;
                                    CalculExchange(currencyEnum);
                                    break;
                                case "Rate":
                                    CalculRateFromAmount();
                                    break;
                            }
                        }
                    }
                }
                #endregion Montant
                //			
                #region Payeur
                if (cciPaymentCur1.IsCciClientId(CciPayment.CciEnumPayment.payer, pCci.ClientId_WithoutPrefix))
                    Ccis.Synchronize(cciPaymentCur2.CciClientId(CciPayment.CciEnumPayment.receiver), pCci.LastValue, pCci.NewValue);
                //
                if (cciPaymentCur2.IsCciClientId(CciPayment.CciEnumPayment.payer, pCci.ClientId_WithoutPrefix))
                    Ccis.Synchronize(cciPaymentCur1.CciClientId(CciPayment.CciEnumPayment.receiver), pCci.LastValue, pCci.NewValue);
                #endregion Payeur
                //
                #region Cas Devise
                if (IsSpotFixing && (false == cciExchangeRate.CciFixing.ExistCciAssetFxRate))
                {
                    // Pré-proposition s'il n'existe pas de zone specifique à la saisie de Asset Fx Rate
                    if (cciPaymentCur1.IsCci(CciPayment.CciEnumPayment.paymentAmount_currency, pCci))
                        Ccis.SetNewValue(cciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_currencyA), fxleg.ExchangedCurrency1.PaymentAmount.Currency);
                    else if (cciPaymentCur2.IsCci(CciPayment.CciEnumPayment.paymentAmount_currency, pCci))
                        Ccis.SetNewValue(cciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_currencyB), fxleg.ExchangedCurrency2.PaymentAmount.Currency);
                    else if (cciExchangeRate.IsCci(CciExchangeRate.CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                        Ccis.SetNewValue(cciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_quoteBasis), fxleg.ExchangeRate.QuotedCurrencyPair.QuoteBasis.ToString());
                }
                //
                if (fxleg.NonDeliverableForwardSpecified)
                {
                    if ((cciPaymentCur1.IsCci(CciPayment.CciEnumPayment.paymentAmount_currency, pCci) && pCci.IsFilledValue)
                        ||
                        (cciPaymentCur2.IsCci(CciPayment.CciEnumPayment.paymentAmount_currency, pCci) && pCci.IsFilledValue))
                    {
                        string clientIdCu1 = cciPaymentCur1.CciClientId(CciPayment.CciEnumPayment.paymentAmount_currency);
                        string clientIdCu2 = cciPaymentCur2.CciClientId(CciPayment.CciEnumPayment.paymentAmount_currency);
                        //
                        for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                        {
                            if (cciFixingNDF[i].ExistCciAssetFxRate)
                                Ccis.InitializeCciAssetFxRate(cciFixingNDF[i].CciClientId(CciFxFixing.CciEnum.assetFxRate), clientIdCu1, clientIdCu2);
                        }
                    }
                }
                #endregion Devise
                //
                cciPaymentCur1.ProcessInitialize(pCci);
                cciPaymentCur2.ProcessInitialize(pCci);
                //
                for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                    cciFixingNDF[i].ProcessInitialize(pCci);

            }
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return cciPaymentCur1.IsClientId_PayerOrReceiver(pCci) || cciPaymentCur2.IsClientId_PayerOrReceiver(pCci);
        }
        #endregion
        #region CleanUP
        public void CleanUp()
        {
            cciPaymentCur1.CleanUp();
            cciPaymentCur2.CleanUp();
            cciExchangeRate.CleanUp();
            //
            #region nonDeliverableForward
            if (CciCountainsNDF)
            {
                IFxFixing[] fixing = fxleg.NonDeliverableForward.Fixing;
                //
                if (ArrFunc.IsFilled(fixing))
                {
                    for (int i = fixing.Length - 1; -1 < i; i--)
                    {
                        if (false == CaptureTools.IsDocumentElementValid(fixing[i].FixingDate.Value))
                            ReflectionTools.RemoveItemInArray(fxleg.NonDeliverableForward, "fixing", i);
                    }
                }
            }
            //
            #endregion FXCashSettlementFixing
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            cciPaymentCur1.SetDisplay(pCci);
            cciPaymentCur2.SetDisplay(pCci);
            cciExchangeRate.SetDisplay(pCci);
        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion Interface  IContainerCciFactory

        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci

        #region Membre de IContainerCciQuoteBasis
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            if (!isOk)
            {
                isOk = cciExchangeRate.IsCci(CciExchangeRate.CciEnum.quotedCurrencyPair_quoteBasis, pCci);
            }
            //
            if (!isOk)
            {
                for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                {
                    isOk = cciFixingNDF[i].IsClientId_QuoteBasis(pCci);
                    if (isOk)
                        break;
                }
            }
            return isOk;
        }

        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(ret))
                ret= CciExchangeRate.GetCurrency1(pCci);
            //
            if (StrFunc.IsEmpty(ret))
            {
                // Recherche ds les fixings de FxCashSettlement
                for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                {
                    ret = cciFixingNDF[i].GetCurrency1(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            if (StrFunc.IsEmpty(ret))
                ret = CciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_currency).NewValue;
            //
            return ret;
        }

        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (StrFunc.IsEmpty(ret))
                CciExchangeRate.GetCurrency2(pCci);
            //
            //
            if (StrFunc.IsEmpty(ret))
            {
                // Recherche ds les fixings de FxCashSettlement
                for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
                {
                    ret = cciFixingNDF[i].GetCurrency2(pCci);
                    if (StrFunc.IsFilled(ret))
                        break;
                }
            }
            //
            if (StrFunc.IsEmpty(ret))
                ret = CciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_currency).NewValue;
            //
            return ret;
        }
        #region public GetBaseCurrency
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            return ret;
        }
        #endregion
        #endregion public

        #region private InitializeFxCAshSettlementFixing
        private void InitializeFxCashSettlementFixing()
        {
            int index = -1;
            ArrayList lst = new ArrayList();

            bool isOk = CciCountainsNDF;

            lst.Clear();
            while (isOk)
            {
                index += 1;
                //
                CciFxFixing cciFxfixing = new CciFxFixing(cciTrade, index + 1, null, prefix + "nonDeliverableForward" + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_fixing);
                //				    
                isOk = Ccis.Contains(cciFxfixing.CciClientId(CciFxFixing.CciEnum.fixingDate));
                if (isOk)
                {
                    IFxFixing[] fxFixing = fxleg.NonDeliverableForward.Fixing;
                    //
                    if (ArrFunc.IsEmpty(fxFixing) || (index == fxFixing.Length))
                        ReflectionTools.AddItemInArray(fxleg.NonDeliverableForward, "fixing", index);
                    cciFxfixing.Fixing = fxleg.NonDeliverableForward.Fixing[index];
                    //
                    lst.Add(cciFxfixing);
                }
            }
            //
            cciFixingNDF = null;
            cciFixingNDF = new CciFxFixing[lst.Count];
            for (int i = 0; i < FxNonDeliverableForwardFixingLength; i++)
            {
                cciFixingNDF[i] = (CciFxFixing)lst[i];
                cciFixingNDF[i].Initialize_FromCci();
            }
            //
        }
        #endregion
        #region private GetTypeFx
        private TypeFxEnum GetTypeFx()
        {
            if (CciExchangeRate.TypeFxEnum.SPOT == cciExchangeRate.TypeFx)
                return TypeFxEnum.SPOT;
            else
                return TypeFxEnum.FORWARD;
        }
        #endregion
        #region private GetExchangedCurrency
        private IPayment GetExchangedCurrency(CurrencyEnum pCurrencyEnum)
        {
            IPayment ret;
            if (CurrencyEnum.Currency1 == pCurrencyEnum)
                ret = fxleg.ExchangedCurrency1;
            else
                ret = fxleg.ExchangedCurrency2;
            return ret;
        }

        #endregion
        #region private CalculExchange
        // EG 20180423 Analyse du code Correction [CA2200]
        private void CalculExchange(CurrencyEnum pCurrencyEnum)
        {
            try
            {
                IQuotedCurrencyPair qcp = fxleg.ExchangeRate.QuotedCurrencyPair;
                string cur1 = qcp.Currency1;
                string cur2 = qcp.Currency2;
                QuoteBasisEnum QbEnum = qcp.QuoteBasis;
                decimal rate = fxleg.ExchangeRate.Rate.DecValue;
                //
                CurrencyEnum currencySource = CurrencyEnum.Currency1;
                if (pCurrencyEnum == CurrencyEnum.Currency1)
                    currencySource = CurrencyEnum.Currency2;
                //
                IPayment exchangedCur = GetExchangedCurrency(currencySource);
                decimal amount = exchangedCur.PaymentAmount.Amount.DecValue;
                //
                if (CurrencyEnum.Currency2 == pCurrencyEnum)
                {
                    EFS_Cash cc2 = new EFS_Cash(cciTrade.CSCacheOn , cur1, cur2, amount, rate, QbEnum);
                    cciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).NewValue =
                        StrFunc.FmtDecimalToInvariantCulture(cc2.ExchangeAmountRounded);
                }
                else
                {
                    EFS_Cash cc1 = new EFS_Cash(cciTrade.CSCacheOn, cur2, cur1, amount, 1 / rate, QbEnum);
                    cciPaymentCur1.Cci(CciPayment.CciEnumPayment.paymentAmount_amount).NewValue =
                        StrFunc.FmtDecimalToInvariantCulture(cc1.ExchangeAmountRounded);
                }
            }
            catch (DivideByZeroException)
            {/*don't suppress*/}
            catch (Exception) { throw; }
        }
        #endregion
        #region private CalculRateFromAmount
        // EG 20180423 Analyse du code Correction [CA2200]
        private void CalculRateFromAmount()
        {
            try
            {
                decimal amount1 = GetExchangedCurrency(CurrencyEnum.Currency1).PaymentAmount.Amount.DecValue;
                decimal amount2 = GetExchangedCurrency(CurrencyEnum.Currency2).PaymentAmount.Amount.DecValue;
                decimal rate = Tools.GetExchangeRate(cciTrade.CSCacheOn, fxleg.ExchangeRate.QuotedCurrencyPair, amount1, amount2);
                //
                Ccis.SetNewValue(cciExchangeRate.CciClientId(CciExchangeRate.CciEnum.rate), StrFunc.FmtDecimalToInvariantCulture(rate));
            }
            catch (DivideByZeroException) { }
            catch (Exception) { throw; }
        }
        #endregion


        #region ICciPresentation Membres

        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (ArrFunc.IsFilled(cciFixingNDF))
            {
                for (int i = 0; i < ArrFunc.Count(cciFixingNDF); i++)
                    cciFixingNDF[i].DumpSpecific_ToGUI(pPage);
            }

            cciExchangeRate.DumpSpecific_ToGUI(pPage);
        }

        #endregion
    }
}

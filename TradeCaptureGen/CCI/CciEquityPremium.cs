#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciEquityPremium 
    /// </summary>
    public class CciEquityPremium : IContainerCciFactory, IContainerCci, IContainerCciPayerReceiver, IContainerCciSpecified
    {
        #region Members
        private readonly CciTrade cciTrade;
        private readonly IEquityPremium equityPremium;
        private readonly string prefix;
        private readonly string _clientIdDefaultReceiver;
        private readonly string _clientIdDefaultBDC;
        private readonly string _clientIdDefaultCurrency;
        private readonly string _defaultPremiumType;
        #endregion

        #region public Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("premiumType")]
            premiumType,
            [System.Xml.Serialization.XmlEnumAttribute("paymentAmount.amount")]
            paymentAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("paymentAmount.currency")]
            paymentAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.unadjustedDate")]
            paymentDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.dateAdjustments.businessDayConvention")]
            paymentDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("swapPremium")]
            swapPremium,
            [System.Xml.Serialization.XmlEnumAttribute("pricePerOption.amount")]
            pricePerOption_amount,
            [System.Xml.Serialization.XmlEnumAttribute("pricePerOption.currency")]
            pricePerOption_currency,
            [System.Xml.Serialization.XmlEnumAttribute("percentageOfNotional")]
            percentageOfNotional,
            unknown
        }
        #endregion public Enum

        #region Property
        #region public ccis
        public TradeCustomCaptureInfos Ccis => cciTrade.Ccis;
        #endregion

        public bool ExistCciPricePerOption
        {
            get { return Ccis.Contains(CciClientId(CciEnum.pricePerOption_amount)); }
        }
        public bool ExistCciPaymentAmout
        {
            get { return Ccis.Contains(CciClientId(CciEnum.pricePerOption_amount)); }
        }
        public bool ExistCciPercentageOfNotional
        {
            get { return Ccis.Contains(CciClientId(CciEnum.percentageOfNotional)); }
        }
        #endregion

        #region Constructor
        public CciEquityPremium(CciTrade pTrade, IEquityPremium pEquityPremium, string pPrefix)
            : this(pTrade, pEquityPremium, pPrefix, string.Empty, string.Empty, string.Empty, string.Empty) { }
        public CciEquityPremium(CciTrade pTrade, IEquityPremium pEquityPremium, string pPrefix,
            string pDefaultPaymentType, string pClientIdDefaultReceiver, string pClientIdDefaultBDC, string pClientIdDefaultCurrency)
        {
            cciTrade = pTrade;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            //
            equityPremium = pEquityPremium;
            //
            _clientIdDefaultReceiver = pClientIdDefaultReceiver;
            _clientIdDefaultBDC = pClientIdDefaultBDC;
            _clientIdDefaultCurrency = pClientIdDefaultCurrency;
            _defaultPremiumType = pDefaultPaymentType;
        }
        #endregion Constructor

        #region Membres de IContainerCciFactory
        #region public Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, equityPremium, "CciEnum");
        }
        #endregion Initialize_FromCci
        #region public AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            ArrayList PayersReceivers = new ArrayList
            {
                CciClientIdPayer,
                CciClientIdReceiver
            };


            IEnumerator ListEnum = PayersReceivers.GetEnumerator();
            while (ListEnum.MoveNext())
            {
                string clientId_WithoutPrefix = ListEnum.Current.ToString();
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            Ccis[CciClientIdReceiver].IsMandatory = Ccis[CciClientIdPayer].IsMandatory;

            // Payment.PaymentDate n'est pas forcement renseignée (Ex sur le Change)
            if (Ccis.Contains(CciClientId(CciEnum.paymentDate_unadjustedDate)))
            {
                //Nécessaire pour le recalcul des BCs 
                string clientId_WithoutPrefix = CciClientId(CciEnum.paymentDate_dateAdjustments_bDC);
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }

            if (ExistCciPricePerOption)
            {
                string clientId_WithoutPrefix = CciClientId(CciEnum.pricePerOption_currency);
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }

            if (ExistCciPaymentAmout)
            {
                string clientId_WithoutPrefix = CciClientId(CciEnum.paymentAmount_currency);
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            //Do Not Erase
            CciTools.CreateInstance(this, equityPremium, "CciEnum");

        }
        #endregion AddCciSystem
        #region public Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            try
            {
                foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
                {
                    CustomCaptureInfo cci = Cci(cciEnum);
                    if (cci != null)
                    {
                        #region Reset variables
                        FieldInfo fld = null;
                        string  data = string.Empty;
                        bool isSetting = true;
                        SQL_Table  sql_Table = null;
                        #endregion
                        //
                        #region Payment/FxOptionPremium
                        //
                        switch (cciEnum)
                        {
                            #region Payer/receiver
                            case CciEnum.payer:
                            case CciEnum.receiver:
                                string field = cciEnum.ToString() + "PartyReference";
                                fld = equityPremium.GetType().GetField(field);
                                //
                                if (null != fld)
                                    data = ((IReference)fld.GetValue(equityPremium)).HRef;
                                break;
                            #endregion Payer/receiver
                            #region premiumType
                            case CciEnum.premiumType:
                                if (equityPremium.PremiumTypeSpecified)
                                    data = equityPremium.PremiumType.ToString();
                                break;
                            #endregion
                            #region paymentAmount
                            case CciEnum.paymentAmount_amount:
                                if (equityPremium.PaymentAmountSpecified)
                                    data = equityPremium.PaymentAmount.Amount.Value;
                                break;
                            case CciEnum.paymentAmount_currency:
                                if (equityPremium.PaymentAmountSpecified)
                                    data = equityPremium.PaymentAmount.Currency;
                                break;
                            #endregion
                            #region paymentDate_unadjustedDate
                            case CciEnum.paymentDate_unadjustedDate:
                                if (equityPremium.PaymentDateSpecified)
                                    data = equityPremium.PaymentDate.UnadjustedDate.Value;
                                break;
                            case CciEnum.paymentDate_dateAdjustments_bDC:
                                if (equityPremium.PaymentDateSpecified)
                                    data = equityPremium.PaymentDate.DateAdjustments.BusinessDayConvention.ToString();
                                break;
                            #endregion
                            #region pricePerOption
                            case CciEnum.pricePerOption_amount:
                                if (equityPremium.PricePerOptionSpecified)
                                    data = equityPremium.PricePerOption.Amount.Value;
                                break;
                            case CciEnum.pricePerOption_currency:
                                if (equityPremium.PricePerOptionSpecified)
                                    data = equityPremium.PricePerOption.Currency;
                                break;
                            #endregion
                            #region percentageOfNotional
                            case CciEnum.percentageOfNotional:
                                if (equityPremium.PercentageOfNotionalSpecified)
                                    data = equityPremium.PercentageOfNotional.Value;
                                break;
                            #endregion
                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                        #endregion
                        //
                        if (isSetting)
                            Ccis.InitializeCci(cci, sql_Table, data);
                    }
                }
                //Look
                if (false == Cci(CciEnum.payer).IsMandatory)
                    SetEnabled(Cci(CciEnum.payer).IsFilledValue);
                //
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region public Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            try
            {
                bool isSetting;
                string data;
                string clientId;
                string clientId_Element;
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;
                foreach (CustomCaptureInfo cci in Ccis)
                {
                    //On ne traite que les contrôle dont le contenu à changé
                    if (cci.HasChanged && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        clientId = cci.ClientId;
                        data = cci.NewValue;
                        isSetting = true;
                        clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                        //
                        CciEnum elt = CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                            elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                        //
                        switch (elt)
                        {
                            #region PaymentPayer/Receiver
                            case CciEnum.payer:
                            case CciEnum.receiver:
                                if (CciEnum.payer == elt)
                                    equityPremium.PayerPartyReference.HRef = data;
                                else
                                    equityPremium.ReceiverPartyReference.HRef = data;
                                //
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                                break;
                            #endregion PaymentPayer/Receiver
                            #region premiumType
                            case CciEnum.premiumType:
                                equityPremium.PremiumTypeSpecified = cci.IsFilledValue;
                                if (equityPremium.PremiumTypeSpecified)
                                    equityPremium.PremiumType = (PremiumTypeEnum)Enum.Parse(typeof(PremiumTypeEnum), data, true);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                break;
                            #endregion
                            #region PaymentAmount
                            case CciEnum.paymentAmount_amount:
                                equityPremium.PaymentAmountSpecified = cci.IsFilledValue;
                                equityPremium.PaymentAmount.Amount.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                                break;
                            case CciEnum.paymentAmount_currency:
                                equityPremium.PaymentAmount.Currency = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir PaymentAmount
                                break;
                            #endregion
                            #region PaymentDate
                            case CciEnum.paymentDate_unadjustedDate:
                                equityPremium.PaymentDateSpecified = cci.IsFilledValue;
                                equityPremium.PaymentDate.UnadjustedDate.Value = data;
                                break;
                            case CciEnum.paymentDate_dateAdjustments_bDC:
                                DumpEquityPremiumBDA();
                                break;
                            #endregion
                            #region pricePerOption
                            case CciEnum.pricePerOption_amount:
                                equityPremium.PricePerOptionSpecified = cci.IsFilledValue;
                                equityPremium.PricePerOption.Amount.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                                break;
                            case CciEnum.pricePerOption_currency:
                                equityPremium.PricePerOption.Currency = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir PaymentAmount
                                break;
                            #endregion
                            #region percentageOfNotional
                            case CciEnum.percentageOfNotional:
                                equityPremium.PercentageOfNotionalSpecified = cci.IsFilledValue;
                                equityPremium.PercentageOfNotional.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                                break;
                            #endregion
                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                        //
                        if (isSetting)
                            Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
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
        #region public ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (key)
                {
                    #region Payer/Receiver: Calcul des BCs
                    case CciEnum.payer:
                        Ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        //
                        if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.payer).IsMandatory))
                            Clear();
                        else
                            EquityPremiumInitialize();
                        //
                        if (false == Cci(CciEnum.payer).IsMandatory)
                            SetEnabled(pCci.IsFilledValue);
                        //
                        DumpEquityPremiumBDA();
                        break;
                    case CciEnum.receiver:
                        Ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        DumpEquityPremiumBDA();
                        break;
                    #endregion
                    #region PaymentDate
                    case CciEnum.paymentDate_unadjustedDate:
                        break;
                    #endregion
                    #region paymentAmount_amount
                    case CciEnum.paymentAmount_amount:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.paymentAmount_amount), equityPremium.PaymentAmount, (CciEnum.paymentAmount_amount == key));
                        break;
                    case CciEnum.paymentAmount_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.paymentAmount_amount), equityPremium.PaymentAmount, (CciEnum.paymentAmount_amount == key));
                        DumpEquityPremiumBDA();
                        Ccis.SetNewValue(CciClientId(CciEnum.pricePerOption_currency), equityPremium.PaymentAmount.Currency);
                        break;
                    #endregion
                    #region pricePerOption_amount
                    case CciEnum.pricePerOption_amount:
                        //On ne tient pas compte ici du nbr de dec définis sur la devise 
                        //Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.pricePerOption_amount), equityPremium.pricePerOption, (CciEnum.pricePerOption_amount == key));
                        break;
                    case CciEnum.pricePerOption_currency:
                        //On ne tient pas compte ici du nbr de dec définis sur la devise    
                        //Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.pricePerOption_amount), equityPremium.pricePerOption, (CciEnum.pricePerOption_amount == key));
                        DumpEquityPremiumBDA();
                        Ccis.SetNewValue(CciClientId(CciEnum.paymentAmount_currency), equityPremium.PricePerOption.Currency);
                        break;
                    #endregion
                    default:
                        break;
                }
            }
        }
        #endregion ProcessInitialize
        #region public IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        #endregion
        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
        }
        #endregion
        #region public SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
            CciTools.SetCciContainer(this, "CciEnum", "IsEnabled", pIsEnabled);
            //Doit tjs être Enabled 
            Cci(CciEnum.payer).IsEnabled = true;
        }
        #endregion
        #region public CleanUp
        public void CleanUp()
        {
            equityPremium.PercentageOfNotionalSpecified = CaptureTools.IsDocumentElementValid(equityPremium.PercentageOfNotional);
            equityPremium.PaymentDateSpecified = CaptureTools.IsDocumentElementValid(equityPremium.PaymentDate.UnadjustedDate.Value);
            equityPremium.PricePerOptionSpecified = CaptureTools.IsDocumentElementValid(equityPremium.PricePerOption);
        }
        #endregion
        #region public SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.paymentDate_dateAdjustments_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, equityPremium.PaymentDate.DateAdjustments);
        }
        #endregion
        #region public RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {

        }
        #endregion
        #region public RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region public Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion

        #region Membres de IContainerCciPayerReceiver
        #region  CciClientIdPayer/receiver
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer); }
        }
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver); }
        }
        #endregion
        #region public SynchronizePayerReceiver
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            // pas de synchrosisation du receiver lorsqu'il est à blanc et que l'utilisateur remplace la counterpartie à blanc par une donnée
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, IsSpecified);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, IsSpecified);
        }
        #endregion
        #endregion

        #region Membres de IContainerCci
        #region public IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        #region public CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region public Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion
        #region public IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #region public CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region public IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion

        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion IContainerCciSpecified

        #region public EquityPremiumInitialize
        public void EquityPremiumInitialize()
        {
            try
            {
                bool isExistCciDefaultReceiver = Ccis.Contains(_clientIdDefaultReceiver);
                bool isExistCciDefaultCurrency = Ccis.Contains(_clientIdDefaultCurrency);
                bool isExistCciDefaultBDC = Ccis.Contains(_clientIdDefaultBDC);
                //
                bool isOk;
                //
                string defaultReceiver = string.Empty; ;
                string defaultBDC = string.Empty;
                string defaultCurrency = string.Empty;
                string defaultSettlementType = string.Empty;
                string currentClientIdDefaultReceiver = string.Empty;
                //
                // Si aucune initialisation particuliere de _clientIdDefaultReceiver 
                // Le receiver est l'autre contrepartie
                if (false == isExistCciDefaultReceiver)
                {
                    if (Cci(CciEnum.payer).IsFilledValue)
                    {
                        CustomCaptureInfo cciCounterpartyVs = cciTrade.GetCciCounterpartyVs(Cci(CciEnum.payer).NewValue);
                        if (null != cciCounterpartyVs)
                        {
                            isExistCciDefaultReceiver = true;
                            currentClientIdDefaultReceiver = cciCounterpartyVs.ClientId_WithoutPrefix;
                        }
                    }
                }
                else
                {
                    currentClientIdDefaultReceiver = _clientIdDefaultReceiver;
                }
                //
                if (StrFunc.IsFilled(currentClientIdDefaultReceiver))
                {
                    defaultReceiver = string.Empty;
                    defaultReceiver = Ccis[currentClientIdDefaultReceiver].NewValue; //=> CodeBIC
                    if (null != (SQL_Actor)Ccis[currentClientIdDefaultReceiver].Sql_Table)
                        defaultReceiver = ((SQL_Actor)Ccis[currentClientIdDefaultReceiver].Sql_Table).XmlId;
                    //
                    if (defaultReceiver == Ccis[CciClientId(CciEnum.payer)].NewValue) // Garde Fou pour ne pas instaurer de payer et receiver identique
                        defaultReceiver = string.Empty;
                }
                else
                {
                    currentClientIdDefaultReceiver = CciClientIdReceiver;
                    defaultReceiver = Ccis[currentClientIdDefaultReceiver].NewValue; //=> CodeBIC
                }
                //	
                isOk = (Ccis.Contains(currentClientIdDefaultReceiver));
                if (isOk)
                {
                    if (isExistCciDefaultBDC)
                        defaultBDC = Ccis[_clientIdDefaultBDC].NewValue;
                    else
                        defaultBDC = BusinessDayConventionEnum.NONE.ToString();
                    //
                    if (isExistCciDefaultCurrency)
                        defaultCurrency = Ccis[_clientIdDefaultCurrency].NewValue;
                    else
                        defaultCurrency = CciTradeCommonBase.GetDefaultCurrency();
                    //
                    defaultSettlementType = Cst.SettlementTypeEnum.None.ToString();
                    //
                    SetEquityPremiumData(defaultReceiver, defaultBDC, defaultCurrency, _defaultPremiumType);
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion

        #region private DumpEquityPremiumBDA
        private void DumpEquityPremiumBDA()
        {
            string clientId = CciClientId(CciEnum.paymentDate_dateAdjustments_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.paymentAmount_currency), CciBC.TypeReferentialInfo.Currency }
            };

            Ccis.DumpBDC_ToDocument(equityPremium.PaymentDate.DateAdjustments, clientId, CciClientId(TradeCustomCaptureInfos.CCst.PAYMENT_BUSINESS_CENTERS_REFERENCE), cciBC);
        }
        #endregion
        #region private SetEquityPremiumData
        // EG 20171004 [23452] Use TradeDateTime
        private void SetEquityPremiumData(string pReceiver, string pBusinessDayConvention, string pCurrency, string pPremiumType)
        {
            try
            {
                CustomCaptureInfo cci;

                //Receiver			
                if (Ccis.Contains(CciClientId(CciEnum.receiver)))
                {
                    cci = Cci(CciEnum.receiver);
                    if (StrFunc.IsEmpty(cci.LastValue))
                        cci.NewValue = pReceiver;
                }
                //Date			
                if (Ccis.Contains(CciClientId(CciEnum.paymentDate_unadjustedDate)))
                {
                    cci = Cci(CciEnum.paymentDate_unadjustedDate);
                    if (StrFunc.IsEmpty(cci.LastValue))
                        cci.NewValue = Tz.Tools.DateToStringISO(cciTrade.cciMarket[0].Cci(CciMarketParty.CciEnum.executionDateTime).NewValue);

                }
                //BDC
                if (Ccis.Contains(CciClientId(CciEnum.paymentDate_dateAdjustments_bDC)))
                {
                    cci = Cci(CciEnum.paymentDate_dateAdjustments_bDC);
                    if (StrFunc.IsEmpty(cci.LastValue))
                        cci.NewValue = pBusinessDayConvention;
                }
                //PaymentType
                if (Ccis.Contains(CciClientId(CciEnum.premiumType)))
                {
                    cci = Cci(CciEnum.premiumType);
                    if (StrFunc.IsEmpty(cci.LastValue))
                        cci.NewValue = pPremiumType;
                }
                //PaymentAmount
                if (Ccis.Contains(CciClientId(CciEnum.paymentAmount_amount)))
                {
                    cci = Cci(CciEnum.paymentAmount_amount);
                    if (StrFunc.IsEmpty(cci.LastValue))
                        cci.NewValue = "0";
                }
                //PaymentCurrency
                if (Ccis.Contains(CciClientId(CciEnum.paymentAmount_currency)))
                {
                    cci = Cci(CciEnum.paymentAmount_currency);
                    if (StrFunc.IsEmpty(cci.LastValue))
                        cci.NewValue = pCurrency;
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion

    }
}
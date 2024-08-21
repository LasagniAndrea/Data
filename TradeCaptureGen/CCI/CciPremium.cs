#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Tz = EFS.TimeZone;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciPremium 
    /// </summary>
    public class CciPremium : ContainerCciBase, IContainerCciFactory,  IContainerCciPayerReceiver, IContainerCciSpecified
    {
        #region Members
        private readonly CciTrade cciTrade;
        private readonly IPremium premium;

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
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableDate.unadjustedDate")]
            paymentDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableDate.dateAdjustments.businessDayConvention")]
            paymentDate_adjustableDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("swapPremium")]
            swapPremium,
            [System.Xml.Serialization.XmlEnumAttribute("pricePerOption.amount")]
            pricePerOption_amount,
            [System.Xml.Serialization.XmlEnumAttribute("pricePerOption.currency")]
            pricePerOption_currency,
            [System.Xml.Serialization.XmlEnumAttribute("percentageOfNotional")]
            percentageOfNotional,

            valuationType,
            valuationAmount,

            unknown
        }
        #endregion public Enum

        #region Property
        #region public ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        #endregion
        #endregion

        #region Constructor
        public CciPremium(CciTrade pTrade, IPremium pPremium, string pPrefix)
            : this(pTrade, pPremium, pPrefix, string.Empty, string.Empty, string.Empty, string.Empty) { }
        public CciPremium(CciTrade pTrade, IPremium pPremium, string pPrefix,
            string pDefaultPaymentType, string pClientIdDefaultReceiver, string pClientIdDefaultBDC, string pClientIdDefaultCurrency):
            base(pPrefix, pTrade.Ccis)
        {
            cciTrade = pTrade;
            

            premium = pPremium;

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
            CciTools.CreateInstance(this, premium, "CciEnum");
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
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            CcisBase[CciClientIdReceiver].IsMandatory = CcisBase[CciClientIdPayer].IsMandatory;

            // Payment.PaymentDate n'est pas forcement renseignée (Ex sur le Change)
            if (CcisBase.Contains(CciClientId(CciEnum.paymentDate_adjustableDate_unadjustedDate)))
            {
                //Nécessaire pour le recalcul des BCs 
                string clientId_WithoutPrefix = CciClientId(CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC);
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }

            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.pricePerOption_amount), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.pricePerOption_currency), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.percentageOfNotional), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.paymentAmount_amount), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.paymentAmount_currency), false, TypeData.TypeDataEnum.@string);

            //Do Not Erase
            CciTools.CreateInstance(this, premium, "CciEnum");

        }
        #endregion AddCciSystem
        #region public Initialize_FromDocument
        public void Initialize_FromDocument()
        {

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

                    #region Payment
                    switch (cciEnum)
                    {
                        case CciEnum.valuationType:
                            data = premium.ValuationType.ToString();
                            break;
                        case CciEnum.valuationAmount:
                            SetCciPremiumValuationAmount(cci);
                            switch (premium.ValuationType)
                            {
                                case PremiumAmountValuationTypeEnum.Cash:
                                    data = premium.PaymentAmount.Amount.Value;
                                    break;
                                case PremiumAmountValuationTypeEnum.PercentageOfNotional:
                                    data = premium.PercentageOfNotional.Value;
                                    break;
                                case PremiumAmountValuationTypeEnum.PricePerOption:
                                    data = premium.PricePerOption.Amount.Value;
                                    break;
                            }
                            break;

                        #region Payer/receiver
                        case CciEnum.payer:
                        case CciEnum.receiver:
                            string field = cciEnum.ToString() + "PartyReference";
                            FieldInfo fld = premium.GetType().GetField(field);
                            //
                            if (null != fld)
                                data = ((IReference)fld.GetValue(premium)).HRef;
                            break;
                        #endregion Payer/receiver
                        #region premiumType
                        case CciEnum.premiumType:
                            if (premium.PremiumTypeSpecified)
                                data = premium.PremiumType.ToString();
                            break;
                        #endregion
                        #region paymentAmount
                        case CciEnum.paymentAmount_amount:
                            data = premium.PaymentAmount.Amount.Value;
                            break;
                        case CciEnum.paymentAmount_currency:
                            data = premium.PaymentAmount.Currency;
                            break;
                        #endregion
                        #region paymentDate_unadjustedDate
                        case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                            if (premium.PaymentDate.AdjustableDateSpecified)
                                data = premium.PaymentDate.AdjustableDate.UnadjustedDate.Value;
                            break;
                        case CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC:
                            if (premium.PaymentDate.AdjustableDateSpecified)
                                data = premium.PaymentDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                            break;
                        #endregion
                        #region pricePerOption
                        case CciEnum.pricePerOption_amount:
                            if (premium.PricePerOptionSpecified)
                                data = premium.PricePerOption.Amount.Value;
                            break;
                        case CciEnum.pricePerOption_currency:
                            if (premium.PricePerOptionSpecified)
                                data = premium.PricePerOption.Currency;
                            break;
                        #endregion
                        #region percentageOfNotional
                        case CciEnum.percentageOfNotional:
                            if (premium.PercentageOfNotionalSpecified)
                                data = premium.PercentageOfNotional.Value;
                            break;
                        #endregion
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    #endregion

                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //Look
            if (false == Cci(CciEnum.payer).IsMandatory)
                SetEnabled(Cci(CciEnum.payer).IsFilledValue);
            //

        }
        #endregion Initialize_FromDocument
        #region public Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    
                    string data = cci.NewValue;
                    Boolean isSetting = true;
                    string clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                    
                    //
                    switch (cciEnum)
                    {
                        case CciEnum.valuationType:

                            premium.ValuationType = (PremiumAmountValuationTypeEnum)Enum.Parse(typeof(PremiumAmountValuationTypeEnum), data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.valuationAmount:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;



                        #region PaymentPayer/Receiver
                        case CciEnum.payer:
                        case CciEnum.receiver:
                            if (CciEnum.payer == cciEnum)
                                premium.PayerPartyReference.HRef = data;
                            else
                                premium.ReceiverPartyReference.HRef = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        #endregion PaymentPayer/Receiver
                        #region premiumType
                        case CciEnum.premiumType:
                            premium.PremiumTypeSpecified = cci.IsFilledValue;
                            if (premium.PremiumTypeSpecified)
                                premium.PremiumType = (PremiumTypeEnum)Enum.Parse(typeof(PremiumTypeEnum), data, true);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion
                        #region PaymentAmount
                        case CciEnum.paymentAmount_amount:
                            premium.PaymentAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        case CciEnum.paymentAmount_currency:
                            premium.PaymentAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir PaymentAmount
                            break;
                        #endregion
                        #region PaymentDate
                        case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                            premium.PaymentDate.AdjustableDateSpecified = cci.IsFilledValue;
                            premium.PaymentDate.AdjustableDate.UnadjustedDate.Value = data;
                            break;
                        case CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC:
                            DumpPremiumBDA();
                            break;
                        #endregion
                        #region pricePerOption
                        case CciEnum.pricePerOption_amount:
                            premium.PricePerOptionSpecified = cci.IsFilledValue;
                            premium.PricePerOption.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        case CciEnum.pricePerOption_currency:
                            premium.PricePerOption.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir PaymentAmount
                            break;
                        #endregion
                        #region percentageOfNotional
                        case CciEnum.percentageOfNotional:
                            premium.PercentageOfNotionalSpecified = cci.IsFilledValue;
                            premium.PercentageOfNotional.Value = data;
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
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

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
                    #region ValuationType
                    case CciEnum.valuationType:
                        CcisBase.SetNewValue(CciClientId(CciEnum.pricePerOption_amount), string.Empty);
                        CcisBase.SetNewValue(CciClientId(CciEnum.pricePerOption_currency), string.Empty);
                        CcisBase.SetNewValue(CciClientId(CciEnum.percentageOfNotional), string.Empty);
                        //
                        if (null != Cci(CciEnum.valuationAmount))
                        {
                            Cci(CciEnum.valuationAmount).NewValue = string.Empty;
                            Cci(CciEnum.valuationAmount).ErrorMsg = string.Empty;
                        }
                        break;
                    #endregion

                    #region Payer/Receiver: Calcul des BCs
                    case CciEnum.payer:
                        CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        //
                        if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.payer).IsMandatory))
                            Clear();
                        else
                            PremiumInitialize();
                        //
                        if (false == Cci(CciEnum.payer).IsMandatory)
                            SetEnabled(pCci.IsFilledValue);
                        //
                        DumpPremiumBDA();
                        break;
                    case CciEnum.receiver:
                        CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        DumpPremiumBDA();
                        break;
                    #endregion
                    #region PaymentDate
                    case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                        break;
                    #endregion
                    #region paymentAmount_amount
                    case CciEnum.paymentAmount_amount:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.paymentAmount_amount), premium.PaymentAmount, (CciEnum.paymentAmount_amount == key));
                        break;
                    case CciEnum.paymentAmount_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.paymentAmount_amount), premium.PaymentAmount, (CciEnum.paymentAmount_amount == key));
                        DumpPremiumBDA();
                        Ccis.SetNewValue(CciClientId(CciEnum.pricePerOption_currency), premium.PaymentAmount.Currency);
                        break;
                    #endregion
                    #region pricePerOption_amount
                    case CciEnum.pricePerOption_amount:
                        //On ne tient pas compte ici du nbr de dec définis sur la devise 
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.pricePerOption_amount), premium.PricePerOption, (CciEnum.pricePerOption_amount == key));
                        break;
                    case CciEnum.pricePerOption_currency:
                        //On ne tient pas compte ici du nbr de dec définis sur la devise    
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.pricePerOption_amount), premium.PricePerOption, (CciEnum.pricePerOption_amount == key));
                        DumpPremiumBDA();
                        CcisBase.SetNewValue(CciClientId(CciEnum.paymentAmount_currency), premium.PricePerOption.Currency);
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
            premium.PercentageOfNotionalSpecified = CaptureTools.IsDocumentElementValid(premium.PercentageOfNotional);
            premium.PaymentDate.AdjustableDateSpecified = CaptureTools.IsDocumentElementValid(premium.PaymentDate.AdjustableDate.UnadjustedDate.Value);
            premium.PricePerOptionSpecified = CaptureTools.IsDocumentElementValid(premium.PricePerOption);
        }
        #endregion
        #region public SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC, pCci))
                Ccis.SetDisplayBusinessDayAdjustments(pCci, premium.PaymentDate.AdjustableDate.DateAdjustments);

            else if (IsCci(CciEnum.valuationAmount, pCci))
            {
                pCci.Display = string.Empty;
                if (StrFunc.IsFilled(pCci.NewValue))
                    // FI 20190520 [XXXXX] usage de Value plutot que decValue pour ne pas perdre des decimales
                    pCci.Display = string.Empty + "Amount: " + StrFunc.FmtDecimalToCurrentCulture(premium.PaymentAmount.Amount.Value) + Cst.Space + premium.PaymentAmount.Currency;
            }
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
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue, IsSpecified);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, IsSpecified);
        }
        #endregion
        #endregion

        

        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion IContainerCciSpecified

        #region public PremiumInitialize
        public void PremiumInitialize()
        {
            bool isExistCciDefaultReceiver = CcisBase.Contains(_clientIdDefaultReceiver);
            bool isExistCciDefaultCurrency = CcisBase.Contains(_clientIdDefaultCurrency);
            bool isExistCciDefaultBDC = CcisBase.Contains(_clientIdDefaultBDC);

            bool isOk;
            ;
            string currentClientIdDefaultReceiver = string.Empty;

            // Si aucune initialisation particuliere de _clientIdDefaultReceiver 
            // Le receiver est l'autre contrepartie
            if (false == isExistCciDefaultReceiver)
            {
                if (Cci(CciEnum.payer).IsFilledValue)
                {
                    CustomCaptureInfo cciCounterpartyVs = cciTrade.GetCciCounterpartyVs(Cci(CciEnum.payer).NewValue);
                    if (null != cciCounterpartyVs)
                    {
                        currentClientIdDefaultReceiver = cciCounterpartyVs.ClientId_WithoutPrefix;
                    }
                }
            }
            else
            {
                currentClientIdDefaultReceiver = _clientIdDefaultReceiver;
            }

            string defaultReceiver;
            //
            if (StrFunc.IsFilled(currentClientIdDefaultReceiver))
            {
                defaultReceiver = CcisBase[currentClientIdDefaultReceiver].NewValue; //=> CodeBIC
                if (null != (SQL_Actor)CcisBase[currentClientIdDefaultReceiver].Sql_Table)
                    defaultReceiver = ((SQL_Actor)CcisBase[currentClientIdDefaultReceiver].Sql_Table).XmlId;
                //
                if (defaultReceiver == CcisBase[CciClientId(CciEnum.payer)].NewValue) // Garde Fou pour ne pas instaurer de payer et receiver identique
                    defaultReceiver = string.Empty;
            }
            else
            {
                currentClientIdDefaultReceiver = CciClientIdReceiver;
                defaultReceiver = CcisBase[currentClientIdDefaultReceiver].NewValue; //=> CodeBIC
            }
            //	
            isOk = (CcisBase.Contains(currentClientIdDefaultReceiver));
            if (isOk)
            {
                string defaultBDC;
                if (isExistCciDefaultBDC)
                    defaultBDC = CcisBase[_clientIdDefaultBDC].NewValue;
                else
                    defaultBDC = BusinessDayConventionEnum.NONE.ToString();

                string defaultCurrency;
                if (isExistCciDefaultCurrency)
                    defaultCurrency = CcisBase[_clientIdDefaultCurrency].NewValue;
                else
                    defaultCurrency = CciTradeCommonBase.GetDefaultCurrency();

                _ = Cst.SettlementTypeEnum.None.ToString();
                SetPremiumData(defaultReceiver, defaultBDC, defaultCurrency, _defaultPremiumType);
            }

        }
        #endregion

        #region private DumpPremiumBDA
        private void DumpPremiumBDA()
        {
            string clientId = CciClientId(CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnum.paymentAmount_currency), CciBC.TypeReferentialInfo.Currency }
            };

            Ccis.DumpBDC_ToDocument(premium.PaymentDate.AdjustableDate.DateAdjustments, clientId, CciClientId(TradeCustomCaptureInfos.CCst.PAYMENT_BUSINESS_CENTERS_REFERENCE), cciBC);
        }
        #endregion
        #region private SetPremiumData
        /// EG 20171004 [23452] TradeDateTime
        private void SetPremiumData(string pReceiver, string pBusinessDayConvention, string pCurrency, string pPremiumType)
        {

            CustomCaptureInfo cci;

            //Receiver			
            if (CcisBase.Contains(CciClientId(CciEnum.receiver)))
            {
                cci = Cci(CciEnum.receiver);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pReceiver;
            }
            //Date			
            if (CcisBase.Contains(CciClientId(CciEnum.paymentDate_adjustableDate_unadjustedDate)))
            {
                cci = Cci(CciEnum.paymentDate_adjustableDate_unadjustedDate);
                if (StrFunc.IsEmpty(cci.LastValue))
                {
                    cci.NewValue = Tz.Tools.DateToStringISO(cciTrade.cciMarket[0].Cci(CciMarketParty.CciEnum.executionDateTime).NewValue);
                }
            }
            //BDC
            if (CcisBase.Contains(CciClientId(CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC)))
            {
                cci = Cci(CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pBusinessDayConvention;
            }
            //PaymentType
            if (CcisBase.Contains(CciClientId(CciEnum.premiumType)))
            {
                cci = Cci(CciEnum.premiumType);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pPremiumType;
            }
            //PaymentAmount
            if (CcisBase.Contains(CciClientId(CciEnum.paymentAmount_amount)))
            {
                cci = Cci(CciEnum.paymentAmount_amount);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = "0";
            }
            //PaymentCurrency
            if (CcisBase.Contains(CciClientId(CciEnum.paymentAmount_currency)))
            {
                cci = Cci(CciEnum.paymentAmount_currency);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pCurrency;
            }
            //PricePerOptionAmount
            if (CcisBase.Contains(CciClientId(CciEnum.pricePerOption_amount)))
            {
                cci = Cci(CciEnum.pricePerOption_amount);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = "0";
            }
            //PricePerOptionCurrency
            if (CcisBase.Contains(CciClientId(CciEnum.pricePerOption_currency)))
            {
                cci = Cci(CciEnum.pricePerOption_currency);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pCurrency;
            }

        }
        #endregion

        #region SetCciPremiumValuationAmount
        /// <summary>
        /// 
        /// </summary>
        public void SetCciPremiumValuationAmount(CustomCaptureInfo pCci)
        {
            if (pCci.IsFilledValue)
            {
                PremiumAmountValuationTypeEnum valuationType = (PremiumAmountValuationTypeEnum)ReflectionTools.EnumParse(new PremiumAmountValuationTypeEnum(), pCci.NewValue);
                CustomCaptureInfo cciValuationAmount = Cci(CciEnum.valuationAmount);
                switch (valuationType)
                {
                    case PremiumAmountValuationTypeEnum.Cash:
                        cciValuationAmount.DataType = TypeData.TypeDataEnum.@decimal;
                        cciValuationAmount.Regex = EFSRegex.TypeRegex.RegexAmountExtend;
                        break;
                    case PremiumAmountValuationTypeEnum.PercentageOfNotional:
                        cciValuationAmount.DataType = TypeData.TypeDataEnum.@decimal;
                        cciValuationAmount.Regex = EFSRegex.TypeRegex.RegexPercentExtend;
                        break;
                    case PremiumAmountValuationTypeEnum.PricePerOption:
                        cciValuationAmount.DataType = TypeData.TypeDataEnum.@decimal;
                        cciValuationAmount.Regex = EFSRegex.TypeRegex.RegexAmountExtend;
                        break;
                }
            }
        }
        #endregion SetCciPremiumValuationAmount
    }
}
#region Using Directives
using System;
using System.Linq;
using System.Reflection;
using System.Collections;

using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;

using EfsML.Business;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciProductTermDeposit.
    /// </summary>
    public class CciProductTermDeposit : CciProductBase
    {
        #region Members
        private ITermDeposit _termDeposit;
        
        private readonly CciPayment[] _cciPayment = null;

        
        #endregion Members

        #region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("initialPayerReference")]
            initialPayerReference,
            [System.Xml.Serialization.XmlEnumAttribute("initialReceiverReference")]
            initialReceiverReference,
            [System.Xml.Serialization.XmlEnumAttribute("startDate")]
            startDate,
            [System.Xml.Serialization.XmlEnumAttribute("maturityDate")]
            maturityDate,
            [System.Xml.Serialization.XmlEnumAttribute("dayCountFraction")]
            dayCountFraction,
            [System.Xml.Serialization.XmlEnumAttribute("principal.amount")]
            principal_amount,
            [System.Xml.Serialization.XmlEnumAttribute("principal.currency")]
            principal_currency,
            [System.Xml.Serialization.XmlEnumAttribute("fixedRate")]
            fixedRate,
            [System.Xml.Serialization.XmlEnumAttribute("interest.amount")]
            interest_amount,
            [System.Xml.Serialization.XmlEnumAttribute("interest.currency")]
            interest_currency,
            unknown
        }
        #endregion Enum

        #region  accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool ContainsCciInterest
        {
            get { return CcisBase.Contains(CciClientId(CciEnum.interest_amount)); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ContainsCciPayment
        {
            get { return (0 < PaymentLength); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int PaymentLength
        {
            get { return ArrFunc.IsFilled(_cciPayment) ? _cciPayment.Length : 0; }
        }

        #endregion

        #region Constructors
        public CciProductTermDeposit(CciTrade pCciTrade, ITermDeposit pTermDeposit, string pPrefix)
            : this(pCciTrade, pTermDeposit, pPrefix, -1)
        { }
        public CciProductTermDeposit(CciTrade pCciTrade, ITermDeposit pTermDeposit, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pTermDeposit, pPrefix, pNumber)
        {
        }
        #endregion Constructors

        #region Membres de ITradeCci
        #region RetSidePayer
        //20081216 PL: L'acheteur ==> paye les intérêts ==> receiver of the initial principal
        public override string RetSidePayer { get { return SideTools.RetSellSide(); } }
        #endregion RetSidePayer
        #region RetSideReceiver
        //20081216 PL: Le vendeur ==> reçoie les intérêts ==> payer of the initial principal
        public override string RetSideReceiver { get { return SideTools.RetBuySide(); } }
        #endregion RetSideReceiver
        #region GetMainCurrency
        /// <summary>
        /// Return the main currency for a product
        /// </summary>
        /// <returns></returns>
        public override string GetMainCurrency
        {
            get
            {
                return _termDeposit.Principal.Currency;
            }
        }
        #endregion GetMainCurrency
        #region CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get { return CciClientId(CciEnum.principal_currency); }
        }
        #endregion CciClientIdMainCurrency
        #region public override GetData
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {
            try
            {
                string ret = string.Empty;
                //
                if (StrFunc.IsEmpty(pKey) && IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string cliendId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    CciEnum enumKey = CciEnum.unknown;
                    //
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                        enumKey = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    //  
                    switch (enumKey)
                    {
                        case CciEnum.startDate:
                            pKey = "T";
                            break;
                    }
                    //
                    if (StrFunc.IsEmpty(pKey))
                        pKey = "E";
                }
                //
                if (StrFunc.IsFilled(pKey))
                {
                    switch (pKey.ToUpper())
                    {
                        case "E":
                            ret = Cci(CciEnum.startDate).NewValue;
                            break;
                    }
                }
                //
                return ret;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion
        #endregion

        #region Membre de IContainerCciPayerReceiver
        #region  CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.initialPayerReference.ToString()); }
        }
        #endregion CciClientIdPayer
        #region CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.initialReceiverReference.ToString()); }
        }
        #endregion
        #region SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion
        #endregion

        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance((IContainerCci)this, _termDeposit);
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
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

            for (int i = 0; i < PaymentLength; i++)
                _cciPayment[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        public override void Initialize_FromDocument()
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

                    switch (cciEnum)
                    {
                        #region InitialPayerReference
                        case CciEnum.initialPayerReference:
                            data = _termDeposit.InitialPayerReference.HRef;
                            break;
                        case CciEnum.initialReceiverReference:
                            data = _termDeposit.InitialReceiverReference.HRef;
                            break;
                        #endregion InitialPayerReference

                        #region starDate/MaturityDate
                        case CciEnum.startDate:
                            data = _termDeposit.StartDate.Value;
                            break;
                        case CciEnum.maturityDate:
                            data = _termDeposit.MaturityDate.Value;
                            break;
                        #endregion starDate/MaturityDate

                        #region DayCountFraction
                        case CciEnum.dayCountFraction:
                            data = _termDeposit.DayCountFraction.ToString();
                            break;
                        #endregion

                        #region Principal
                        case CciEnum.principal_amount:
                            data = _termDeposit.Principal.Amount.Value;
                            break;

                        case CciEnum.principal_currency:
                            data = _termDeposit.Principal.Currency;
                            break;
                        #endregion

                        #region FixedRate
                        case CciEnum.fixedRate:
                            data = _termDeposit.FixedRate.Value;
                            break;
                        #endregion FixedRate

                        #region Interest
                        case CciEnum.interest_amount:
                            if (_termDeposit.InterestSpecified)
                                data = _termDeposit.Interest.Amount.Value;
                            break;
                        case CciEnum.interest_currency:
                            if (_termDeposit.InterestSpecified)
                                data = _termDeposit.Interest.Currency;
                            break;
                        #endregion Interest

                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            
            for (int i = 0; i < this.PaymentLength; i++)
                _cciPayment[i].Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region public override Dump_ToDocument
        public override void Dump_ToDocument()
        {

            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region InitialPayerReference
                        case CciEnum.initialPayerReference:
                            _termDeposit.InitialPayerReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.initialReceiverReference:
                            _termDeposit.InitialReceiverReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion InitialPayerReference
                        #region starDate/MaturityDate
                        case CciEnum.startDate:
                            _termDeposit.StartDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer le nbre de jour => Interest
                            break;
                        case CciEnum.maturityDate:
                            _termDeposit.MaturityDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer le nbre de jour => Interest
                            break;
                        #endregion starDate/MaturityDate
                        #region Principal
                        case CciEnum.principal_amount:
                            _termDeposit.Principal.Amount.Value = data;
                            if (StrFunc.IsEmpty(_termDeposit.Principal.Id))
                                _termDeposit.Principal.Id = TradeCustomCaptureInfos.CCst.PRINCIPAL_REFERENCE;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; //Afin de recalculer le nbre de jour => Interest
                            break;
                        case CciEnum.principal_currency:
                            _termDeposit.Principal.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High; //Afin de mettre à jour Interest.currency
                            break;
                        #endregion Principal
                        #region FixedRate
                        case CciEnum.fixedRate:
                            _termDeposit.FixedRate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; //Afin de recalculer le nbre de jour => Interest
                            break;
                        #endregion FixedRate
                        #region DayCountFraction
                        case CciEnum.dayCountFraction:
                            DayCountFractionEnum dcfEnum = (DayCountFractionEnum)System.Enum.Parse(typeof(DayCountFractionEnum), data, true);
                            _termDeposit.DayCountFraction = dcfEnum;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; //Afin de recalculer le nbre de jour => Interest
                            break;
                        #endregion DayCountFraction
                        #region Interest
                        case CciEnum.interest_amount:
                            _termDeposit.InterestSpecified = cci.IsFilledValue;
                            _termDeposit.Interest.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.interest_currency:
                            _termDeposit.Interest.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Interest
                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    //
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode))
                Ccis.InitializePaymentPaymentQuoteRelativeTo(_termDeposit.Principal.Id, null, null, CciTrade.cciOtherPartyPayment);
            
            for (int i = 0; i < PaymentLength; i++)
                _cciPayment[i].Dump_ToDocument();
            
            _termDeposit.PaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_termDeposit.PaymentSpecified, _cciPayment);

        }

        #endregion Dump_ToDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //		
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (key)
                {
                    #region InitialPayerReceiver
                    case CciEnum.initialPayerReference:
                    case CciEnum.initialReceiverReference:
                        break;
                    #endregion
                    //
                    #region StartDate, MaturityDate, DayCountFraction, FixedRate
                    case CciEnum.startDate:
                    case CciEnum.maturityDate:
                    case CciEnum.dayCountFraction:
                    case CciEnum.fixedRate:
                        if (ContainsCciInterest)
                            CalculateInterest();
                        break;
                    #endregion StartDate, MaturityDate, DayCountFraction, FixedRate
                    //
                    #region Principal
                    case CciEnum.principal_amount:
                    case CciEnum.principal_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.principal_amount), _termDeposit.Principal, (CciEnum.principal_amount == key));
                        if (ContainsCciInterest)
                        {
                            if (CciEnum.principal_currency == key)
                                CcisBase[CciClientId(CciEnum.interest_currency)].NewValue = _termDeposit.Principal.Currency;

                            if (CciEnum.principal_amount == key)
                                CalculateInterest();
                        }
                        break;
                    #endregion Principal
                    //
                    #region Interest
                    case CciEnum.interest_amount:
                    case CciEnum.interest_currency:
                        if (_termDeposit.InterestSpecified)
                        {
                            try
                            {
                                decimal amount = _termDeposit.Interest.Amount.DecValue;
                                EFS_Cash cash = new EFS_Cash(CciTrade.CSCacheOn, amount, _termDeposit.Interest.Currency);
                                if (amount != cash.AmountRounded)
                                    CcisBase[CciClientId(CciEnum.interest_amount)].NewValue =
                                        StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded);
                            }
                            catch
                            { }
                        }
                        else
                        {
                            CcisBase[CciClientId(CciEnum.interest_amount)].NewValue = string.Empty;
                            CcisBase[CciClientId(CciEnum.interest_currency)].NewValue = string.Empty;
                        }
                        break;
                    #endregion Interest
                    //		
                    default:

                        break;
                }
            }
            
            for (int i = 0; i < this.PaymentLength; i++)
                _cciPayment[i].ProcessInitialize(pCci);

        }
        #endregion ProcessInitialize
        #region public override IsClientId_XXXXX
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool ret = false;
            ret = ret || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            ret = ret || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return ret;
        }
        #endregion IsClientId_XXXXX
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < PaymentLength; i++)
                _cciPayment[i].SetDisplay(pCci);
        }
        #endregion
        #region public override CleanUp
        public override void CleanUp()
        {
            for (int i = 0; i < PaymentLength; i++)
                _cciPayment[i].CleanUp();
        }
        #endregion
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            for (int i = 0; i < PaymentLength; i++)
                _cciPayment[i].RefreshCciEnabled();
        }
        #endregion
        #region public override Initialize_Document
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(_termDeposit.InitialPayerReference.HRef) &&
                    StrFunc.IsEmpty(_termDeposit.InitialReceiverReference.HRef))
                {
                    //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    //id = GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    _termDeposit.InitialPayerReference.HRef = id;
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion
        #endregion

        #region Methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            _termDeposit = (ITermDeposit)pProduct;
            base.SetProduct(pProduct);
        }
        #endregion
        //
        #region private CalculateInterest
        private void CalculateInterest()
        {

            if (ContainsCciInterest)
            {
                IInterval interval = null; // 0 DAY
                decimal interest = Tools.CalculateInterest(CciTrade.CSCacheOn,
                    _termDeposit.StartDate.DateValue, _termDeposit.MaturityDate.DateValue,
                    interval, _termDeposit.DayCountFraction,
                    _termDeposit.FixedRate.DecValue, _termDeposit.Principal);
                //
                CcisBase[CciClientId(CciEnum.interest_amount)].NewValue = StrFunc.FmtDecimalToInvariantCulture(interest);
                CcisBase[CciClientId(CciEnum.interest_currency)].NewValue = CcisBase[CciClientId(CciEnum.principal_currency)].NewValue;
            }

        }
        #endregion
        #endregion  Private Sub
    }
}

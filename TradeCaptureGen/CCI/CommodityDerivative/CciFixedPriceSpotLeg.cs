using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;


namespace EFS.TradeInformation
{
    /// <summary>
    /// cci Fixed Price Leg of a Commodity Swap. It defines schedule of fixed payments associated with a commodity swap.
    /// </summary>
    public class CciFixedPriceSpotLeg : IContainerCci, IContainerCciFactory, IContainerCciPayerReceiver, IContainerCciGetInfoButton
    {
        #region Membres
        private readonly IFixedPriceSpotLeg _fixedPriceSpotLeg;
        private readonly CciTradeBase _cciTrade;
        private readonly string _prefix;

        private readonly CciPayment _cciGrossAmount;
        #endregion Membres

        #region Enums
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payer")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiver")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("fixedPrice.price")]
            fixedPrice_price,
            [System.Xml.Serialization.XmlEnumAttribute("fixedPrice.priceCurrency")]
            fixedPrice_priceCurrency,
            [System.Xml.Serialization.XmlEnumAttribute("fixedPrice.priceUnit")]
            fixedPrice_priceUnit,
            /// <summary>
            /// Cci fictif (utilisé pour pre-proposer la date de paiement sur grossAmount et sur les frais)
            /// </summary>
            paymentDate,

            unknown,
        }
        #endregion

        #region accessor
        public CciPayment CciGrossAmount
        {
            get
            {
                return _cciGrossAmount;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCCiTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pFixedPriceSpotLeg"></param>
        public CciFixedPriceSpotLeg(CciTrade pCCiTrade, string pPrefix, IFixedPriceSpotLeg pFixedPriceSpotLeg)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _fixedPriceSpotLeg = pFixedPriceSpotLeg;

            _cciGrossAmount = new CciPayment(pCCiTrade, -1, pFixedPriceSpotLeg.GrossAmount, CciPayment.PaymentTypeEnum.Payment,
                _prefix + TradeCustomCaptureInfos.CCst.Prefix_grossAmount, string.Empty,
                CciClientId(CciEnum.receiver), string.Empty, CciClientId(CciEnum.fixedPrice_priceCurrency), CciClientId(CciEnum.paymentDate))
            {
                ProcessQueueCciDate = CustomCaptureInfosBase.ProcessQueueEnum.High
            };

        }
        #endregion

        #region Membres de IContainerCci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
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
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion Membres de IContainerCci

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer.ToString()); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver.ToString()); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, true);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, true);

            _cciGrossAmount.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion IContainerCciPayerReceiver Members

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fixedPriceSpotLeg);

            _cciGrossAmount.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.payer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.receiver), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.paymentDate), true, TypeData.TypeDataEnum.date);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.fixedPrice_priceCurrency), true, TypeData.TypeDataEnum.@string);

            _cciGrossAmount.AddCciSystem();
        }
        /// <summary>
        /// 
        /// </summary>
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.payer:
                            data = _fixedPriceSpotLeg.PayerPartyReference.HRef;
                            break;
                        case CciEnum.receiver:
                            data = _fixedPriceSpotLeg.ReceiverPartyReference.HRef;
                            break;
                        case CciEnum.fixedPrice_price:
                            data = _fixedPriceSpotLeg.FixedPrice.Price.Value;
                            break;
                        case CciEnum.fixedPrice_priceUnit:
                            data = _fixedPriceSpotLeg.FixedPrice.PriceUnit.Value;
                            break;
                        case CciEnum.fixedPrice_priceCurrency:
                            data = _fixedPriceSpotLeg.FixedPrice.Currency.Value;
                            break;
                        case CciEnum.paymentDate:
                            if (_fixedPriceSpotLeg.GrossAmount.PaymentDateSpecified)
                                data = _fixedPriceSpotLeg.GrossAmount.PaymentDate.UnadjustedDate.Value;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            _cciGrossAmount.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[_prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.payer:
                            _fixedPriceSpotLeg.PayerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.receiver:
                            _fixedPriceSpotLeg.ReceiverPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.fixedPrice_price:
                            _fixedPriceSpotLeg.FixedPrice.Price.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.fixedPrice_priceCurrency:
                            _fixedPriceSpotLeg.FixedPrice.Currency.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.fixedPrice_priceUnit:
                            _fixedPriceSpotLeg.FixedPrice.PriceUnit.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.paymentDate:
                            //Nothing TODO
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            _cciGrossAmount.Dump_ToDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {

                    case CciEnum.payer:
                        Ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.receiver:
                        Ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.fixedPrice_priceCurrency:
                        Ccis.SetNewValue(_cciGrossAmount.CciClientId(CciPayment.CciEnum.currency), pCci.NewValue);
                        break;
                    default:
                        break;
                }
            }

            _cciGrossAmount.ProcessInitialize(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);

            if (!isOk)
                isOk = _cciGrossAmount.IsClientId_PayerOrReceiver(pCci);

            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            _cciGrossAmount.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            _cciGrossAmount.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            // Nothing to do
            _cciGrossAmount.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            _cciGrossAmount.Initialize_Document();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
          // Nothing to do
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            // Nothing to do
        }
        #endregion

        #region IContainerCciGetInfoButton Membres
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            #region buttons settlementInfo
            bool isOk = _cciGrossAmount.IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
            if (isOk)
            {
                pCo.Element = "settlementInformation";
                pCo.Object = "grossAmount";
                pCo.OccurenceValue = 1;
                pIsObjSpecified = _cciGrossAmount.IsSettlementInfoSpecified;
                pIsEnabled = _cciGrossAmount.IsSettlementInstructionSpecified;
            }
            #endregion buttons settlementInfo
            return isOk;
        }
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
        }
        #endregion

    }
}

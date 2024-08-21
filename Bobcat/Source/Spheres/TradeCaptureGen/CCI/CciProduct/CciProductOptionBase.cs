using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Interface;
using System;
using System.Linq;


namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CciProductOptionBase : CciProductBase
    {
        #region Enums
        public enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference.hRef")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference.hRef")]
            seller,
            #endregion buyer/seller
            #region optionType
            [System.Xml.Serialization.XmlEnumAttribute("optionType")]
            optionType,
            #endregion optionType
            unknown,
        }
        #endregion Enum

        #region Members
        private IOptionBase _optionBase;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
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
        #endregion Accessors

        #region Constructors
        public CciProductOptionBase(CciTrade pCciTrade, IOptionBase pOptionBase, string pPrefix)
            : this(pCciTrade, pOptionBase, pPrefix, -1)
        {
        }
        public CciProductOptionBase(CciTrade pCciTrade, IOptionBase pOptionBase, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pOptionBase, pPrefix, pNumber)
        {
            
        }
        #endregion Constructors

        #region Interfaces
        #region Membres de ITradeCci
        #region public override RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region public override RetSideReceiver
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #endregion

        #region IContainerCciFactory Members
        public override void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            //do Not Erase
            CciTools.CreateInstance(this, (IOptionBase)_optionBase);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {

            foreach (string clientId in Ccis.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = Ccis[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.buyer:
                            #region buyer
                            _optionBase.BuyerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Buyer
                            break;
                        case CciEnum.seller:
                            #region seller
                            _optionBase.SellerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Seller
                            break;
                        case CciEnum.optionType:
                            #region optionType
                            _optionBase.OptionType = (FpML.Enum.OptionTypeEnum)Enum.Parse(typeof(FpML.Enum.OptionTypeEnum), data, true);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion optionType
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;

                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, (IOptionBase)_optionBase);
        }
        /// <summary>
        /// 
        /// </summary>
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.buyer:
                            #region buyer
                            data = _optionBase.BuyerPartyReference.HRef;
                            #endregion Buyer
                            break;
                        case CciEnum.seller:
                            #region seller
                            data = _optionBase.SellerPartyReference.HRef;
                            #endregion Seller
                            break;
                        case CciEnum.optionType:
                            #region optionType
                            data = _optionBase.OptionType.ToString();
                            #endregion optionType
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum keyEnum = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (keyEnum)
                {
                    default:
                        #region Default
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        #endregion Default
                        break;
                }
            }
        }
        #endregion IContainerCciFactory Members

        #region IContainerCciPayerReceiver Members
        #region public override CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer.ToString()); }
        }
        #endregion CciClientIdPayer
        #region public override CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller.ToString()); }
        }
        #endregion CciClientIdReceiver
        #region public override SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, true);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, true);
        }
        #endregion SynchronizePayerReceiver
        #endregion IContainerCciPayerReceiver Members

        #endregion Interfaces

        #region methods
        public override void SetProduct(IProduct pProduct)
        {
            _optionBase = (IOptionBase)pProduct;
            base.SetProduct(pProduct);

        }
        #endregion
    }

}

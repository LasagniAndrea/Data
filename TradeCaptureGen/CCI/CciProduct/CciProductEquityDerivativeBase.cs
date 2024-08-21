using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Interface;
using FpML.v44.Assetdef;
using System;
using System.Collections;
using System.Linq;


namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CciProductEquityDerivativeBase : CciProductBase
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
            #region equityEffectiveDate
            [System.Xml.Serialization.XmlEnumAttribute("equityEffectiveDate")]
            equityEffectiveDate,
            #endregion equityEffectiveDate
            #region notional
            [System.Xml.Serialization.XmlEnumAttribute("notional.amount")]
            notional_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notional.currency")]
            notional_currency,
            #endregion notional

            #region Feature
            // Bouton feature
            [System.Xml.Serialization.XmlEnumAttribute("featureSpecified")]
            feature_specified,
            [System.Xml.Serialization.XmlEnumAttribute("feature.asian")]
            feature_asian,
            [System.Xml.Serialization.XmlEnumAttribute("feature.barrier")]
            feature_barrier,
            [System.Xml.Serialization.XmlEnumAttribute("feature.multipleBarrier")]
            feature_multipleBarrier,
            [System.Xml.Serialization.XmlEnumAttribute("feature.knock")]
            feature_knock,
            // Bouton fxFeature
            [System.Xml.Serialization.XmlEnumAttribute("fxFeatureSpecified")]
            fxFeature_specified,
            [System.Xml.Serialization.XmlEnumAttribute("fxFeature")]
            fxFeature,
            // Bouton extraordinaryEvents
            [System.Xml.Serialization.XmlEnumAttribute("extraordinaryEvents")]
            extraordinaryEvents,
            #endregion Feature

            unknown,
        }
        #endregion Enum

        #region Members
        private IEquityDerivativeBase _equityDerivative;
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
        
        
        public CciEquityExerciceValuationSettlement CciEquityExerciseValuationSettlement { get; private set; }
        
        public CciSingleUnderlyer CciSingleUnderlyer { get; private set; }
        
        #endregion Accessors

        #region Constructors
        public CciProductEquityDerivativeBase(CciTrade pCciTrade, IEquityDerivativeBase pEqDerivative, string pPrefix)
            : this(pCciTrade, pEqDerivative, pPrefix, -1)
        {
        }
        public CciProductEquityDerivativeBase(CciTrade pCciTrade, IEquityDerivativeBase pEqDerivative, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pEqDerivative, pPrefix, pNumber)
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
        #region public override GetMainCurrency
        /// <summary>
        /// Return the main currency for a product
        /// </summary>
        /// <returns></returns>
        public override string GetMainCurrency
        {
            get
            {
                string ret = string.Empty;
                if ((null != _equityDerivative.Underlyer) && (null != _equityDerivative.Underlyer.UnderlyerSingle))
                {
                    if (null != _equityDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset)
                    {
                        if (_equityDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset.CurrencySpecified)
                            ret = _equityDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset.Currency.Value;
                    }
                }
                return ret;
            }
        }
        #endregion GetMainCurrency
        #region public override CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get { return string.Empty; } // A revoir
        }
        #endregion CciClientIdMainCurrency
        #endregion
        
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.notional_amount), false, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.notional_currency), false, TypeData.TypeDataEnum.@string);
            //                
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.AddCciSystem();
            //
            CciEquityExerciseValuationSettlement.AddCciSystem();

            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_asian), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_barrier), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_multipleBarrier), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.feature_knock), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.fxFeature), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.extraordinaryEvents), false, TypeData.TypeDataEnum.@string);

            //do Not Erase
            CciTools.CreateInstance(this, (IEquityDerivativeBase)_equityDerivative);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.CleanUp();
            CciEquityExerciseValuationSettlement.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
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
                    ArrayList lst = null;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.buyer:
                            #region buyer
                            _equityDerivative.BuyerPartyReference.HRef = data;
                            //
                            lst = ReflectionTools.GetObjectByName(_equityDerivative, "determiningPartyReference", true);
                            // RD 20200921 [25246] hRef doit être toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                            if ((null != lst) && (((IReference)lst[0]).HRef == cci.LastValue))
                                ((IReference)lst[0]).HRef = XMLTools.GetXmlId(cci.NewValue);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Buyer
                            break;
                        case CciEnum.seller:
                            #region seller
                            _equityDerivative.SellerPartyReference.HRef = data;
                            lst = ReflectionTools.GetObjectByName(_equityDerivative, "determiningPartyReference", true);
                            // RD 20200921 [25246] hRef doit être toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                            if ((null != lst) && (((IReference)lst[0]).HRef == cci.LastValue))
                                ((IReference)lst[0]).HRef = XMLTools.GetXmlId(cci.NewValue);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Seller
                            break;
                        case CciEnum.optionType:
                            #region optionType
                            _equityDerivative.OptionType = (FpML.Enum.OptionTypeEnum)Enum.Parse(typeof(FpML.Enum.OptionTypeEnum), data, true);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion optionType
                            break;
                        case CciEnum.equityEffectiveDate:
                            #region equityEffectiveDate
                            _equityDerivative.EquityEffectiveDateSpecified = StrFunc.IsFilled(data);
                            if (_equityDerivative.EquityEffectiveDateSpecified)
                                _equityDerivative.EquityEffectiveDate.Value = data;
                            #endregion equityEffectiveDate
                            break;
                        case CciEnum.notional_amount:
                            #region notional
                            _equityDerivative.NotionalSpecified = StrFunc.IsFilled(data);
                            _equityDerivative.Notional.Amount.Value = data;
                            #endregion notional
                            break;
                        case CciEnum.notional_currency:
                            #region currency
                            _equityDerivative.Notional.Currency = data;
                            #endregion currency
                            break;

                        case CciEnum.feature_specified:
                            #region featureSpecified
                            _equityDerivative.FeatureSpecified = cci.IsFilledValue;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion featureSpecified
                            break;

                        case CciEnum.fxFeature_specified:
                            #region fxFeatureSpecified
                            _equityDerivative.FxFeatureSpecified = cci.IsFilledValue;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion fxFeatureSpecified
                            break;

                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;

                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            if (null != CciSingleUnderlyer)
            {
                CciSingleUnderlyer.Dump_ToDocument();
                _equityDerivative.Underlyer.UnderlyerSingleSpecified = CciSingleUnderlyer.IsSpecified;
            }
            //
            CciEquityExerciseValuationSettlement.Dump_ToDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.Initialize_Document();
            CciEquityExerciseValuationSettlement.Initialize_Document();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, (IEquityDerivativeBase)_equityDerivative);
            CciSingleUnderlyer cciContainerTmp = new CciSingleUnderlyer(CciTrade, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_singleUnderlyer);
            IUnderlyingAsset asset = null;
            bool isOk = false;
            if (false == isOk)
            {
                isOk = CcisBase.Contains(cciContainerTmp.CciClientId(CciSingleUnderlyer.CciEnum.equity));
                if (isOk)
                    asset = (IUnderlyingAsset)new EquityAsset();
            }
            if (false == isOk)
            {
                isOk = CcisBase.Contains(cciContainerTmp.CciClientId(CciSingleUnderlyer.CciEnum.index));
                if (isOk)
                    asset = (IUnderlyingAsset)new Index();
            }
            if (false == isOk)
            {
                isOk = CcisBase.Contains(cciContainerTmp.CciClientId(CciSingleUnderlyer.CciEnum.bond));
                if (isOk)
                {
                    asset = (IUnderlyingAsset)new Bond();
                }
            }
            if (isOk)
            {
                if (false == _equityDerivative.Underlyer.UnderlyerSingleSpecified)
                {
                    _equityDerivative.Underlyer.UnderlyerSingle = new SingleUnderlyer();
                    _equityDerivative.Underlyer.UnderlyerSingleSpecified = true;
                }
                if (null == _equityDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset)
                    _equityDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset = asset;
                //
                cciContainerTmp.singleUnderlyer = _equityDerivative.Underlyer.UnderlyerSingle;
                cciContainerTmp.singleUnderlyer.UnderlyingAsset = _equityDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset;
                //
                CciSingleUnderlyer = cciContainerTmp;
            }
            else
            {
                CciSingleUnderlyer = null;
            }

            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.Initialize_FromCci();

            CciEquityExerciseValuationSettlement.Initialize_FromCci();

            if (false == _equityDerivative.FxFeatureSpecified)
                _equityDerivative.FxFeature = _equityDerivative.CreateFxFeature;
            if (false == _equityDerivative.FeatureSpecified)
                _equityDerivative.Feature = _equityDerivative.CreateOptionFeatures;
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
                            data = _equityDerivative.BuyerPartyReference.HRef;
                            #endregion Buyer
                            break;
                        case CciEnum.seller:
                            #region seller
                            data = _equityDerivative.SellerPartyReference.HRef;
                            #endregion Seller
                            break;
                        case CciEnum.optionType:
                            #region optionType
                            data = _equityDerivative.OptionType.ToString();
                            #endregion optionType
                            break;
                        case CciEnum.equityEffectiveDate:
                            #region equityEffectiveDate
                            if (_equityDerivative.EquityEffectiveDateSpecified)
                                data = _equityDerivative.EquityEffectiveDate.Value;
                            #endregion equityEffectiveDate
                            break;
                        case CciEnum.notional_amount:
                            #region notional
                            if (_equityDerivative.NotionalSpecified)
                                data = _equityDerivative.Notional.Amount.Value;
                            #endregion notional
                            break;
                        case CciEnum.notional_currency:
                            #region currency
                            if (_equityDerivative.NotionalSpecified)
                                data = _equityDerivative.Notional.Currency;
                            #endregion currency
                            break;

                        case CciEnum.feature_specified:
                            #region featureSpecified
                            data = _equityDerivative.FeatureSpecified.ToString().ToLower();
                            #endregion featureSpecified
                            break;

                        case CciEnum.fxFeature_specified:
                            #region fxFeatureSpecified
                            data = _equityDerivative.FxFeatureSpecified.ToString().ToLower();
                            #endregion fxFeatureSpecified
                            break;

                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.Initialize_FromDocument();
            //
            CciEquityExerciseValuationSettlement.Initialize_FromDocument();

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
                //		
                CciEnum keyEnum = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (keyEnum)
                {

                    case CciEnum.notional_amount:
                    case CciEnum.notional_currency:
                        #region notional/currency
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.notional_amount), _equityDerivative.Notional, (CciEnum.notional_amount == keyEnum));
                        #endregion notional/currency
                        break;
                    default:
                        #region Default
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        #endregion Default
                        break;
                }
            }
            //
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.ProcessInitialize(pCci);
            //
            CciEquityExerciseValuationSettlement.ProcessInitialize(pCci);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.RefreshCciEnabled();
            CciEquityExerciseValuationSettlement.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != CciSingleUnderlyer)
                CciSingleUnderlyer.SetDisplay(pCci);
            CciEquityExerciseValuationSettlement.SetDisplay(pCci);
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
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue, true);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, true);
        }
        #endregion SynchronizePayerReceiver
        #endregion IContainerCciPayerReceiver Members

        #endregion Interfaces

        #region methods
        public override void SetProduct(IProduct pProduct)
        {
            _equityDerivative = (IEquityDerivativeBase)pProduct;
            CciSingleUnderlyer = null;
            
            IEquityExerciseValuationSettlement equityExercise = null;
            if (null != _equityDerivative)
                equityExercise = _equityDerivative.EquityExercise;
            CciEquityExerciseValuationSettlement = new CciEquityExerciceValuationSettlement(CciTrade, equityExercise, Prefix + TradeCustomCaptureInfos.CCst.Prefix_equityExercice);
            
            base.SetProduct(pProduct);

        }


        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            System.Web.UI.Control control = pPage.FindControl(Prefix + "tblOptionFeatures");
            if (null != control)
                control.Visible = _equityDerivative.FeatureSpecified;

            control = pPage.FindControl(Prefix + "tblFxFeature");
            if (null != control)
                control.Visible = _equityDerivative.FxFeatureSpecified;

            base.DumpSpecific_ToGUI(pPage);
        }


        #region public override SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.feature_asian, pCci);
                if (isOk)
                {
                    pCo.Element = "asian";
                    pCo.Object = "feature";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = _equityDerivative.FeatureSpecified && _equityDerivative.Feature.AsianSpecified;
                    pIsEnabled = _equityDerivative.FeatureSpecified;
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.feature_barrier, pCci);
                    if (isOk)
                    {
                        pCo.Element = "barrier";
                        pCo.Object = "feature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _equityDerivative.FeatureSpecified && _equityDerivative.Feature.BarrierSpecified;
                        pIsEnabled = _equityDerivative.FeatureSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.feature_multipleBarrier, pCci);
                    if (isOk)
                    {
                        pCo.Element = "multipleBarrier";
                        pCo.Object = "feature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _equityDerivative.FeatureSpecified && _equityDerivative.Feature.MultipleBarrierSpecified;
                        pIsEnabled = _equityDerivative.FeatureSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.feature_knock, pCci);
                    if (isOk)
                    {
                        pCo.Element = "knock";
                        pCo.Object = "feature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _equityDerivative.FeatureSpecified && _equityDerivative.Feature.KnockSpecified;
                        pIsEnabled = _equityDerivative.FeatureSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.fxFeature, pCci);
                    if (isOk)
                    {
                        pCo.Element = "fxFeature";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = _equityDerivative.FxFeatureSpecified;
                        pIsEnabled = _equityDerivative.FxFeatureSpecified;
                    }
                }

                if (false == isOk)
                {
                    isOk = this.IsCci(CciEnum.extraordinaryEvents, pCci);
                    if (isOk)
                    {
                        pCo.Element = "extraordinaryEvents";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = true;
                        pIsEnabled = true;
                    }
                }

            }
            return isOk;
        }
        #endregion
        #endregion
    }
    
}

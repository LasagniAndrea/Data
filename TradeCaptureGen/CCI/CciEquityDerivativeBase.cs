using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using FpML.Interface;
using FpML.v44.Assetdef;
using System;
using System.Collections;
using System.Reflection;


namespace EFS.TradeInformation
{
    internal abstract class CciEquityDerivativeBase : IContainerCci, IContainerCciFactory, IContainerCciPayerReceiver
    {
        #region Enums
        #region CciEnum
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
            unknown,
        }
        #endregion CciEnum
        #endregion Enum
        #region Members
        private readonly IEquityDerivativeBase eqDerivative;
        protected string prefix;
        protected CciSingleUnderlyer cciSingleUnderlyer;
        protected CciEquityExerciceValuationSettlement cciEquityExerciseValuationSettlement;
        protected CciTrade cciTrade;
        #endregion Members
        #region Accessors
        #region ccis
        public TradeCustomCaptureInfos Ccis => cciTrade.Ccis;
        #endregion ccis
        #endregion Accessors
        #region Constructors
        public CciEquityDerivativeBase(CciTrade pCciTrade, IEquityDerivativeBase pEqDerivative, string pPrefix)
        {
            prefix = pPrefix;
            cciTrade = pCciTrade;
            eqDerivative = pEqDerivative;
            cciSingleUnderlyer = null;
            cciEquityExerciseValuationSettlement = new CciEquityExerciceValuationSettlement(pCciTrade, eqDerivative.EquityExercise, prefix + TradeCustomCaptureInfos.CCst.Prefix_equityExercice);
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
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
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public virtual void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.notional_amount), false, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.notional_currency), false, TypeData.TypeDataEnum.@string);

            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.AddCciSystem();

            cciEquityExerciseValuationSettlement.AddCciSystem();
            //do Not Erase
            CciTools.CreateInstance(this, (IEquityDerivativeBase)eqDerivative);
        }
        #endregion AddCciSystem
        #region CleanUp
        public virtual void CleanUp()
        {
            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.CleanUp();
            cciEquityExerciseValuationSettlement.CleanUp();
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public virtual void Dump_ToDocument()
        {
            try
            {
                bool isSetting;
                string data = string.Empty;
                Type tCciEnum = typeof(CciEnum);
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = Ccis[prefix + enumName];
                    if ((cci != null) && (cci.HasChanged))
                    {
                        #region Reset variables
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        data = cci.NewValue;
                        isSetting = true;
                        ArrayList lst = null;
                        #endregion Reset variables

                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.buyer:
                                #region buyer
                                eqDerivative.BuyerPartyReference.HRef = data;
                                //
                                lst = ReflectionTools.GetObjectByName(eqDerivative, "determiningPartyReference", true);
                                if ((null != lst) && (((IReference)lst[0]).HRef == cci.LastValue))
                                    ((IReference)lst[0]).HRef = cci.NewValue;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                #endregion Buyer
                                break;
                            case CciEnum.seller:
                                #region seller
                                eqDerivative.SellerPartyReference.HRef = data;
                                lst = ReflectionTools.GetObjectByName(eqDerivative, "determiningPartyReference", true);
                                if ((null != lst) && (((IReference)lst[0]).HRef == cci.LastValue))
                                    ((IReference)lst[0]).HRef = cci.NewValue;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                #endregion Seller
                                break;
                            case CciEnum.optionType:
                                #region optionType
                                eqDerivative.OptionType = (FpML.Enum.OptionTypeEnum)Enum.Parse(typeof(FpML.Enum.OptionTypeEnum), data, true);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                #endregion optionType
                                break;
                            case CciEnum.equityEffectiveDate:
                                #region equityEffectiveDate
                                eqDerivative.EquityEffectiveDateSpecified = StrFunc.IsFilled(data);
                                if (eqDerivative.EquityEffectiveDateSpecified)
                                    eqDerivative.EquityEffectiveDate.Value = data;
                                #endregion equityEffectiveDate
                                break;
                            case CciEnum.notional_amount:
                                #region notional
                                eqDerivative.NotionalSpecified = StrFunc.IsFilled(data);
                                eqDerivative.Notional.Amount.Value = data;
                                #endregion notional
                                break;
                            case CciEnum.notional_currency:
                                #region currency
                                eqDerivative.Notional.Currency = data;
                                #endregion currency
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
                if (null != cciSingleUnderlyer)
                {
                    cciSingleUnderlyer.Dump_ToDocument();
                    eqDerivative.Underlyer.UnderlyerSingleSpecified = cciSingleUnderlyer.IsSpecified;
                }
                //
                cciEquityExerciseValuationSettlement.Dump_ToDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public virtual void Initialize_Document()
        {
            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.Initialize_Document();
            cciEquityExerciseValuationSettlement.Initialize_Document();
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public virtual void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, (IEquityDerivativeBase)eqDerivative);
            CciSingleUnderlyer cciContainerTmp = new CciSingleUnderlyer(cciTrade, null, prefix + TradeCustomCaptureInfos.CCst.Prefix_singleUnderlyer);
            IUnderlyingAsset asset = null;
            bool isOk = false;
            if (false == isOk)
            {
                isOk = Ccis.Contains(cciContainerTmp.CciClientId(CciSingleUnderlyer.CciEnum.equity));
                if (isOk)
                    asset = (IUnderlyingAsset)new EquityAsset();
            }
            if (false == isOk)
            {
                isOk = Ccis.Contains(cciContainerTmp.CciClientId(CciSingleUnderlyer.CciEnum.index));
                if (isOk)
                    asset = (IUnderlyingAsset)new Index();
            }
            if (false == isOk)
            {
                isOk = Ccis.Contains(cciContainerTmp.CciClientId(CciSingleUnderlyer.CciEnum.bond));
                if (isOk)
                    asset = (IUnderlyingAsset)new Bond();
            }
            //
            if (isOk)
            {
                if (false == eqDerivative.Underlyer.UnderlyerSingleSpecified)
                    eqDerivative.Underlyer.UnderlyerSingle = new SingleUnderlyer();
                if (null == eqDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset)
                    eqDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset = asset;
                //
                cciContainerTmp.singleUnderlyer = eqDerivative.Underlyer.UnderlyerSingle;
                cciContainerTmp.singleUnderlyer.UnderlyingAsset = eqDerivative.Underlyer.UnderlyerSingle.UnderlyingAsset;
                //
                cciSingleUnderlyer = cciContainerTmp;
            }
            else
            {
                cciSingleUnderlyer = null;
            }
            //
            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.Initialize_FromCci();
            //
            cciEquityExerciseValuationSettlement.Initialize_FromCci();

        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public virtual void Initialize_FromDocument()
        {
            try
            {
                string data;
                bool isSetting;
                SQL_Table sql_Table;
                //
                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = Ccis[prefix + enumName];
                    if (cci != null)
                    {
                        #region Reset variables
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion Reset variables

                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.buyer:
                                #region buyer
                                data = eqDerivative.BuyerPartyReference.HRef;
                                #endregion Buyer
                                break;
                            case CciEnum.seller:
                                #region seller
                                data = eqDerivative.SellerPartyReference.HRef;
                                #endregion Seller
                                break;
                            case CciEnum.optionType:
                                #region optionType
                                data = eqDerivative.OptionType.ToString();
                                #endregion optionType
                                break;
                            case CciEnum.equityEffectiveDate:
                                #region equityEffectiveDate
                                if (eqDerivative.EquityEffectiveDateSpecified)
                                    data = eqDerivative.EquityEffectiveDate.Value;
                                #endregion equityEffectiveDate
                                break;
                            case CciEnum.notional_amount:
                                #region notional
                                if (eqDerivative.NotionalSpecified)
                                    data = eqDerivative.Notional.Amount.Value;
                                #endregion notional
                                break;
                            case CciEnum.notional_currency:
                                #region currency
                                if (eqDerivative.NotionalSpecified)
                                    data = eqDerivative.Notional.Currency;
                                #endregion currency
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
                if (null != cciSingleUnderlyer)
                    cciSingleUnderlyer.Initialize_FromDocument();
                cciEquityExerciseValuationSettlement.Initialize_FromDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {
            try
            {
                if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
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
                            Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.notional_amount), eqDerivative.Notional, (CciEnum.notional_amount == keyEnum));
                            #endregion notional/currency
                            break;
                        default:
                            #region Default
                            //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                            #endregion Default
                            break;
                    }
                }
                if (null != cciSingleUnderlyer)
                    cciSingleUnderlyer.ProcessInitialize(pCci);
                cciEquityExerciseValuationSettlement.ProcessInitialize(pCci);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
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
        #region RefreshCciEnabled
        public virtual void RefreshCciEnabled()
        {
            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.RefreshCciEnabled();
            cciEquityExerciseValuationSettlement.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public virtual void RemoveLastItemInArray(string pPrefix)
        {
            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.RemoveLastItemInArray(pPrefix);
            cciEquityExerciseValuationSettlement.RemoveLastItemInArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != cciSingleUnderlyer)
                cciSingleUnderlyer.SetDisplay(pCci);
            cciEquityExerciseValuationSettlement.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCciPayerReceiver Members
        #region CciClientIdPayer
        public virtual string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer.ToString()); }
        }
        #endregion CciClientIdPayer
        #region CciClientIdReceiver
        public virtual string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller.ToString()); }
        }
        #endregion CciClientIdReceiver
        #region SynchronizePayerReceiver
        public virtual void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, true);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, true);
        }
        #endregion SynchronizePayerReceiver
        #endregion IContainerCciPayerReceiver Members
        #endregion Interfaces
    }
}

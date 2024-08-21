#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using FpML.Interface;
using System;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    internal abstract class CciEquityDerivativeLongFormBase : CciEquityDerivativeBase
    {
        #region Enums
        #region CciEnum
        public new enum CciEnum
        {
            #region methodOfAdjustment
            [System.Xml.Serialization.XmlEnumAttribute("methodOfAdjustment")]
            methodOfAdjustment,
            #endregion methodOfAdjustment
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly IEquityDerivativeLongFormBase eqD;
        #endregion members
        #region Constructors
        public CciEquityDerivativeLongFormBase(CciTrade pCciTrade, IEquityDerivativeLongFormBase pEqD, string pPrefix)
            : base(pCciTrade, (IEquityDerivativeBase)pEqD, pPrefix)
        {
            eqD = pEqD;
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public new CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public new string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public new string CciContainerKey(string pClientId_WithoutPrefix)
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
        public new bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            bool isOk = Ccis.Contains(pClientId_WithoutPrefix);
            return isOk && (pClientId_WithoutPrefix.StartsWith(prefix));
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #region IContainerCciFactory Members
        #region AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public override void Dump_ToDocument()
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
                        #endregion Reset variables
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
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
                base.Dump_ToDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public override void Initialize_Document()
        {
            base.Initialize_Document();
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, (IEquityDerivativeLongFormBase)eqD);
            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public override void Initialize_FromDocument()
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
                base.Initialize_FromDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            try
            {
                if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    switch (key)
                    {
                        #region Default
                        default:
                            //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                            break;
                        #endregion Default
                    }
                }
                base.ProcessInitialize(pCci);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public override void RemoveLastItemInArray(string pPrefix)
        {
            base.RemoveLastItemInArray(pPrefix); 
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci); 
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #endregion Interfaces
    }
}

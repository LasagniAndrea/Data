#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using System;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de TradeSwap.
    /// </summary>
    public class CciEarlyTerminationProvision : CciProvisionBase
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("implicitOptionalEarlyTerminationProvision")]
            implicitOptionalEarlyTerminationProvision,
            [System.Xml.Serialization.XmlEnumAttribute("earlyTerminationProvision")]
            earlyTerminationProvision,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Constructors
        public CciEarlyTerminationProvision(CciTradeBase pTrade) : base(pTrade) { }
        public CciEarlyTerminationProvision(CciTradeBase pTrade, string pPrefix) : base(pTrade, pPrefix) { }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            if (false == ccis.Contains(CciClientId(CciEnum.earlyTerminationProvision.ToString())))
            {
                CciTools.AddCciSystem(ccis, Cst.HCK + CciClientId(CciEnum.implicitOptionalEarlyTerminationProvision.ToString()), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.earlyTerminationProvision.ToString()), false, TypeData.TypeDataEnum.@string);
            }

            base.AddCciSystem();
        }
        #endregion AddCciSystem
        #region Dump_ToDocument
        public override void Dump_ToDocument()
        {
            bool isSetting;
            Type tCciEnum = typeof(CciEnum);
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    isSetting = true;
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.implicitOptionalEarlyTerminationProvision:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            ImplicitOptionalEarlyTerminationProvisionSpecified = cci.IsFilledValue;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region Initialize_FromCci
        public override void Initialize_FromCci()
        {
            base.Initialize_FromCci();
            CciTools.CreateInstance(this, product);
            
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
                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = ccis[prefix + enumName];
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
                            case CciEnum.implicitOptionalEarlyTerminationProvision:
                                data = ImplicitOptionalEarlyTerminationProvisionSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                                break;
                            default:
                                isSetting = false;
                                break;
                        }
                        if (isSetting)
                            ccis.InitializeCci(cci, sql_Table, data);
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            #region ImplicitOptionalEarlyTermination  Updating
            CustomCaptureInfo cciImplicit = Cci(CciEnum.implicitOptionalEarlyTerminationProvision);
            if (cciImplicit.IsFilledValue || cciImplicit.HasChanged)
                ProvisionsTools.SetImplicitOptionalEarlyTermination(cciTrade.CS, product, cciImplicit.IsFilledValue, 
                    GetCalculationAgent(), GetCalculationAgentBC(),
                    cciTrade.DataDocument);
            #endregion ImplicitOptionalEarlyTermination  Updating
        }
        #endregion ProcessInitialize
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return ccis[CciClientId(pEnumValue.ToString())];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        #endregion CciClientId
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci Members
        #region IContainerCciGetInfoButton members
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            try
            {
                bool isOk = false;
                //
                #region EarlyTerminationProvision
                isOk = IsCci(CciEnum.earlyTerminationProvision, pCci);
                if (isOk)
                {
                    pCo.Object = "product";
                    pCo.Element = "earlyTerminationProvision";
                    pCo.IsZoomOnModeReadOnly = ImplicitEarlyTerminationProvisionSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                    pIsSpecified = EarlyTerminationProvisionSpecified;
                    pIsEnabled = true;
                }
                #endregion EarlyTerminationProvision
                //
                return isOk;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion SetButtonZoom
        #endregion IContainerCciGetInfoButton members
        #endregion Interfaces
    }
}

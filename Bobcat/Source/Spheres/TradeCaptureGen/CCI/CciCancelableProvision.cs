#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    public class CciCancelableProvision : CciProvisionBase
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            //
            [System.Xml.Serialization.XmlEnumAttribute("implicitCancelableProvision")]
            implicitCancelableProvision,
            [System.Xml.Serialization.XmlEnumAttribute("cancelableProvision")]
            cancelableProvision,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Constructors
        public CciCancelableProvision(CciTradeBase pTrade) : base(pTrade) { }
        public CciCancelableProvision(CciTradeBase pTrade, string pPrefix) : base(pTrade, pPrefix) { }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            if (false == ccis.Contains(CciClientId(CciEnum.cancelableProvision.ToString())))
            {
                CciTools.AddCciSystem(ccis, Cst.HCK + CciClientId(CciEnum.implicitCancelableProvision.ToString()), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.cancelableProvision.ToString()), false, TypeData.TypeDataEnum.@string);
            }

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
                        case CciEnum.implicitCancelableProvision:

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            ImplicitCancelableProvisionSpecified = cci.IsFilledValue;
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

            string data;
            bool isSetting;
            SQL_Table sql_Table;
            //
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
                        case CciEnum.implicitCancelableProvision:
                            data = ImplicitCancelableProvisionSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
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
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            CustomCaptureInfo cciImplicit = Cci(CciEnum.implicitCancelableProvision);
            ProvisionsTools.SetImplicitCancelableProvision(cciTrade.CS, product, cciImplicit.IsFilledValue,
                GetBuyerPartyReference(), GetSellerPartyReference(),GetCalculationAgentBC(), cciTrade.DataDocument);
        }
        #endregion ProcessInitialize
        #endregion IContainerCciFactory members
        #region IContainerCci Members
        #region Cci
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return ccis[CciClientId(pEnumValue.ToString())];
        }
        #endregion Cci
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
            #region  cancelableProvision
            bool isOk = IsCci(CciEnum.cancelableProvision, pCci);
            if (isOk)
            {
                pCo.Object = "product";
                pCo.Element = "cancelableProvision";
                pCo.IsZoomOnModeReadOnly = ImplicitCancelableProvisionSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                pIsSpecified = CancelableProvisionSpecified;
                pIsEnabled = true;
            }
            #endregion cancelableProvision
            return isOk;
        }
        #endregion SetButtonZoom
        #endregion IContainerCciGetInfoButton members
        #endregion Interfaces
    }
}

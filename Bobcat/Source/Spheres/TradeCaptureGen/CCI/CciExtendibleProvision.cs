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
    public class CciExtendibleProvision : CciProvisionBase
    {
        #region Enum
        public enum CciEnum
        {
            //
            [System.Xml.Serialization.XmlEnumAttribute("implicitExtendibleProvision")]
            implicitExtendibleProvision,
            [System.Xml.Serialization.XmlEnumAttribute("extendibleProvision")]
            extendibleProvision,
            unknown,
        }
        #endregion Enum

        #region Constructor
        public CciExtendibleProvision(CciTradeBase pTrade) : base(pTrade) { }
        public CciExtendibleProvision(CciTradeBase pTrade, string pPrefix) : base(pTrade, pPrefix) { }
        #endregion Constructor

        #region IContainerCciFactory members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            if (false == ccis.Contains(CciClientId(CciEnum.extendibleProvision.ToString())))
            {
                CciTools.AddCciSystem(ccis, Cst.HCK + CciClientId(CciEnum.implicitExtendibleProvision.ToString()), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(ccis, Cst.BUT + CciClientId(CciEnum.extendibleProvision.ToString()), false, TypeData.TypeDataEnum.@string);
            }
            base.AddCciSystem();
        }
        #endregion AddCciSystem
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
                //
                foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
                {
                    CustomCaptureInfo cci = Cci(cciEnum);
                    if (cci != null)
                    {
                        #region Reset variables
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion Reset variables
                        //
                        switch (cciEnum)
                        {
                            case CciEnum.implicitExtendibleProvision:
                                data = ImplicitExtendibleProvisionSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
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
        #region Dump_ToDocument
        public override void Dump_ToDocument()
        {
            bool isSetting;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;  
                    isSetting = true;

                    switch (cciEnum)
                    {
                        case CciEnum.implicitExtendibleProvision:

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            ImplicitExtendibleProvisionSpecified = cci.IsFilledValue;
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
        #region ProcessInitialize
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            CustomCaptureInfo cciImplicit = Cci(CciEnum.implicitExtendibleProvision);
            ProvisionsTools.SetImplicitExtendibleProvision(cciTrade.CS, product, cciImplicit.IsFilledValue,
                GetBuyerPartyReference(), GetSellerPartyReference(), GetCalculationAgentBC(), cciTrade.DataDocument);
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
            try
            {
                bool isOk = false;
                //
                #region  extendibleProvision
                isOk = IsCci(CciEnum.extendibleProvision, pCci);
                if (isOk)
                {
                    pCo.Object = "product";
                    pCo.Element = "extendibleProvision";
                    pCo.IsZoomOnModeReadOnly = ImplicitExtendibleProvisionSpecified ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False;
                    pIsSpecified = ExtendibleProvisionSpecified;
                    pIsEnabled = true;
                }
                #endregion extendibleProvision
                //
                return isOk;
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion SetButtonZoom
        #endregion IContainerCciGetInfoButton members
    }
}
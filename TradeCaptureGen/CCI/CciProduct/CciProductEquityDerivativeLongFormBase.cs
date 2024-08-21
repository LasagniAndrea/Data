#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using FpML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{

    public abstract class CciProductEquityDerivativeLongFormBase : CciProductEquityDerivativeBase
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
        private IEquityDerivativeLongFormBase _equityDerivativeLongForm;
        #endregion members
        
        #region Constructors
        public CciProductEquityDerivativeLongFormBase(CciTrade pCciTrade, IEquityDerivativeLongFormBase pEqD, string pPrefix, int pNumber)
            : base(pCciTrade, (IEquityDerivativeBase)pEqD, pPrefix, pNumber)
        {
         
        }
        #endregion Constructors
        
        #region Interfaces
        #region IContainerCciFactory Members
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
        }
        #endregion CleanUp
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
            base.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region public override Initialize_Document
        public override void Initialize_Document()
        {
            base.Initialize_Document();
        }
        #endregion Initialize_Document
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, (IEquityDerivativeLongFormBase)_equityDerivativeLongForm);
            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
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
                    SQL_Table sql_Table = null;
                    bool isSetting;
                    #endregion Reset variables

                    switch (cciEnum)
                    {

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
            base.Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
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
        #endregion ProcessInitialize
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #endregion Interfaces

        #region methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            _equityDerivativeLongForm = (IEquityDerivativeLongFormBase)pProduct;
            base.SetProduct(pProduct);
        }
        #endregion
        #endregion
    }

}

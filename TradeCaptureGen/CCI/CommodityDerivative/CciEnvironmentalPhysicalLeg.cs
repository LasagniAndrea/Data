using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
namespace EFS.TradeInformation
{
    /// <summary>
    /// cci Physically Environmental Leg transaction.
    /// </summary>
    // EG 20221201 [25639] [WI484] New 
    public class CciEnvironmentalPhysicalLeg : CCiPhysicalSwapLeg, IContainerCci, IContainerCciFactory, IContainerCciPayerReceiver
    {

        private readonly IEnvironmentalPhysicalLeg _environmentalPhysicalLeg;
        //private readonly CciTrade _cciTrade;
        private readonly CciEnvironmentalProductionFeatures _cciProductionFeatures;
        private readonly string _prefix;

        #region Enums
        public new enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payer")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiver")]
            receiver,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("commodityAsset")]
            commodityAsset,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("environmental.productType")]
            environmental_productType,

            [System.Xml.Serialization.XmlEnumAttribute("numberOfAllowances.quantityUnit")]
            numberOfAllowances_quantityUnit,
            [System.Xml.Serialization.XmlEnumAttribute("numberOfAllowances.quantity")]
            numberOfAllowances_quantity,

            unknown
        }
        #endregion

        public override CommodityContractClassEnum CommodityContractClass
        {
            get { return CommodityContractClassEnum.Environmental; }
        }

        public override string CommodityProductType
        {
            get { return EnvironmentalProductType.ToString(); }
        }

        public CciEnvironmentalProductionFeatures CciEnvironmentalProductionFeatures
        {
            get { return _cciProductionFeatures; }
        }


        public CciEnvironmentalPhysicalLeg(CciTrade pCCiTrade, string pPrefix, IEnvironmentalPhysicalLeg pEnvironmentalPhysicalLeg)
            : base(pCCiTrade, pPrefix + CustomObject.KEY_SEPARATOR, pEnvironmentalPhysicalLeg)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _environmentalPhysicalLeg = pEnvironmentalPhysicalLeg;

            _cciProductionFeatures = new CciEnvironmentalProductionFeatures(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_environmentalProductionFeatures, pEnvironmentalPhysicalLeg.ProductionFeatures);
        }


        #region Membres de IContainerCci
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion Membres de IContainerCci
        #region Membres de IContainerCciFactory
        public override void Initialize_FromCci()
        {
            base.Initialize_FromCci();
            CciTools.CreateInstance(this, _environmentalPhysicalLeg);

            _cciProductionFeatures.Initialize_FromCci();

        }
        /// <summary>
        /// 
        /// </summary>
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.environmental_productType), false, TypeData.TypeDataEnum.@string);

            _cciProductionFeatures.AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            base.Initialize_FromDocument();

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
                        case CciEnum.environmental_productType:
                            data = _environmentalPhysicalLeg.Environmental.ProductType.ToString();
                            break;
                        case CciEnum.numberOfAllowances_quantity:
                            data = _environmentalPhysicalLeg.NumberOfAllowances.Quantity.Value;
                            break;
                        case CciEnum.numberOfAllowances_quantityUnit:
                            data = _environmentalPhysicalLeg.NumberOfAllowances.QuantityUnit.Value;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            _cciProductionFeatures.Initialize_FromDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {
            base.Dump_ToDocument();

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
                        case CciEnum.environmental_productType:
                            if (StrFunc.IsFilled(data))
                                _environmentalPhysicalLeg.Environmental.ProductType = (EnvironmentalProductTypeEnum)(Enum.Parse(typeof(EnvironmentalProductTypeEnum), data));
                            break;
                        case CciEnum.numberOfAllowances_quantity:
                            _environmentalPhysicalLeg.NumberOfAllowances.Quantity.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.numberOfAllowances_quantityUnit:
                            _environmentalPhysicalLeg.NumberOfAllowances.QuantityUnit.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            _cciProductionFeatures.Dump_ToDocument();
            _environmentalPhysicalLeg.ProductionFeaturesSpecified =
                _environmentalPhysicalLeg.ProductionFeatures.TechnologySpecified ||
                _environmentalPhysicalLeg.ProductionFeatures.RegionSpecified ||
                _environmentalPhysicalLeg.ProductionFeatures.DeviceSpecified;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    default:
                        break;
                }
            }
            _cciProductionFeatures.ProcessInitialize(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            base.ProcessExecute(pCci);
            _cciProductionFeatures.ProcessExecute(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            base.ProcessExecuteAfterSynchronize(pCci);
            _cciProductionFeatures.ProcessExecuteAfterSynchronize(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            Boolean ret = base.IsClientId_PayerOrReceiver(pCci);
            if (false == ret)
                ret = _cciProductionFeatures.IsClientId_PayerOrReceiver(pCci);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            _cciProductionFeatures.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
            _cciProductionFeatures.RefreshCciEnabled();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            _cciProductionFeatures.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            _cciProductionFeatures.Initialize_Document();
        }

        protected override void SetCommodityContractCharacteristics()
        {
            base.SetCommodityContractCharacteristics();

            if (this.Cci(CciEnum.commodityAsset).Sql_Table is SQL_AssetCommodityContract sqlAsset)
            {
                Ccis.SetNewValue(CciClientId(CciEnum.environmental_productType), sqlAsset.CommodityContract_Type);
                Ccis.SetNewValue(CciClientId(CciEnum.numberOfAllowances_quantityUnit), sqlAsset.CommodityContract_UnitOfPrice);
            }
        }

        public override void ResetCciFacilityHasChanged()
        {
            List<CciEnum> lst = CciTools.GetCciEnum<CciEnum>("FacilityHaschanged").ToList();
            lst.ForEach(item =>
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            });

            _cciProductionFeatures.ResetCciFacilityHasChanged();

        }
        #endregion
    }
}

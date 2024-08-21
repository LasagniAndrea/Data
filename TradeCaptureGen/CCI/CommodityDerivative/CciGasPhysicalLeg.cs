using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using Tz = EFS.TimeZone;
namespace EFS.TradeInformation
{
    /// <summary>
    /// cci Physically settled leg of a physically settled gas transaction.
    /// </summary>
    public class CCiGasPhysicalLeg : CCiPhysicalSwapLeg, IContainerCci, IContainerCciFactory
    {
        private readonly IGasPhysicalLeg _gasPhysicalLeg;
        //private readonly CciTradeBase _cciTrade;
        private readonly string _prefix;

        #region Enums
        // EG 20171113 [23509] Add FacilityHasChanged
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
            [System.Xml.Serialization.XmlEnumAttribute("gas.type")]
            gas_type,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("gas.calorificValue")]
            gas_calorificValue,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("gas.quality")]
            gas_quality,

            unknown
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private readonly CciGasPhysicalQuantity _cciDeliveryQuantity;


        /// <summary>
        /// 
        /// </summary>
        private readonly CciGasDelivery _cciDeliveryConditions;

        /// <summary>
        /// 
        /// </summary>
        private readonly CciGasDeliveryPeriods _cciDeliveryPeriods;


        public override CommodityContractClassEnum CommodityContractClass
        {
            get { return CommodityContractClassEnum.Gas; }
        }
        // EG 20221201 [25639] [WI484] New
        public override string CommodityProductType
        {
            get { return GasProductType.ToString(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public CciGasPhysicalQuantity CciDeliveryQuantity
        {
            get { return _cciDeliveryQuantity; }
        }
        /// EG 20171016 [23509] New
        public override string DeliveryLocation
        {
            get 
            {
                string location = _cciDeliveryPeriods.CciSupplyStartTime.Cci(CCiPrevailingTime.CciEnum.location).NewValue;
                return StrFunc.IsFilled(location) ? location : Tz.Tools.UniversalTimeZone;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCCiTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pGasPhysical"></param>
        public CCiGasPhysicalLeg(CciTrade pCCiTrade, string pPrefix, IGasPhysicalLeg pGasPhysical)
            : base(pCCiTrade, pPrefix + CustomObject.KEY_SEPARATOR, pGasPhysical)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _gasPhysicalLeg = pGasPhysical;

            _cciDeliveryQuantity = new CciGasPhysicalQuantity(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_deliveryQuantity, pGasPhysical.DeliveryQuantity);

            _cciDeliveryConditions = new CciGasDelivery(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_deliveryConditions, pGasPhysical.DeliveryConditions);

            _cciDeliveryPeriods = new CciGasDeliveryPeriods(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_deliveryPeriods, pGasPhysical.DeliveryPeriods);

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
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            base.Initialize_FromCci();

            _cciDeliveryQuantity.Initialize_FromCci();
            _cciDeliveryConditions.Initialize_FromCci();
            _cciDeliveryPeriods.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
         /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.gas_calorificValue), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.gas_quality), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.gas_type), false, TypeData.TypeDataEnum.@string);

            _cciDeliveryQuantity.AddCciSystem();
            _cciDeliveryConditions.AddCciSystem();
            _cciDeliveryPeriods.AddCciSystem();
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
                        case CciEnum.gas_type:
                            data = _gasPhysicalLeg.Gas.TypeGas.ToString();
                            break;
                        case CciEnum.gas_calorificValue:
                            if (_gasPhysicalLeg.Gas.CalorificValueSpecified)
                                _gasPhysicalLeg.Gas.CalorificValue.Value = data;
                            break;
                        case CciEnum.gas_quality:
                            if (_gasPhysicalLeg.Gas.QualitySpecified)
                                _gasPhysicalLeg.Gas.Quality.Value = data;
                            break;

                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            _cciDeliveryQuantity.Initialize_FromDocument();

            _cciDeliveryConditions.Initialize_FromDocument();

            _cciDeliveryPeriods.Initialize_FromDocument();
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
                        case CciEnum.gas_calorificValue:
                            _gasPhysicalLeg.Gas.CalorificValueSpecified = BoolFunc.IsTrue(data);
                            if (_gasPhysicalLeg.Gas.CalorificValueSpecified)
                                _gasPhysicalLeg.Gas.CalorificValue.Value = data;

                            _gasPhysicalLeg.Gas.NoneSpecified = ((false == _gasPhysicalLeg.Gas.CalorificValueSpecified) && (false == _gasPhysicalLeg.Gas.QualitySpecified));

                            break;
                        case CciEnum.gas_quality:
                            _gasPhysicalLeg.Gas.QualitySpecified = BoolFunc.IsTrue(data);
                            if (_gasPhysicalLeg.Gas.QualitySpecified)
                                _gasPhysicalLeg.Gas.Quality.Value = data;

                            _gasPhysicalLeg.Gas.NoneSpecified = ((false == _gasPhysicalLeg.Gas.CalorificValueSpecified) && (false == _gasPhysicalLeg.Gas.QualitySpecified));

                            break;
                        case CciEnum.gas_type:
                            if (StrFunc.IsFilled(data))
                                _gasPhysicalLeg.Gas.TypeGas = (GasProductTypeEnum)(Enum.Parse(typeof(GasProductTypeEnum), data));
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            _cciDeliveryQuantity.Dump_ToDocument();

            _cciDeliveryConditions.Dump_ToDocument();

            _cciDeliveryPeriods.Dump_ToDocument();
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
                    case CciEnum.commodityAsset:
                        break;
                    default:
                        break;
                }
            }


            _cciDeliveryQuantity.ProcessInitialize(pCci);

            _cciDeliveryConditions.ProcessInitialize(pCci);

            _cciDeliveryPeriods.ProcessInitialize(pCci);

            


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            base.ProcessExecute(pCci);

            _cciDeliveryQuantity.ProcessExecute(pCci);

            _cciDeliveryConditions.ProcessExecute(pCci);

            _cciDeliveryPeriods.ProcessExecute(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            base.ProcessExecuteAfterSynchronize(pCci);

            _cciDeliveryQuantity.ProcessExecuteAfterSynchronize(pCci);

            _cciDeliveryConditions.ProcessExecuteAfterSynchronize(pCci);

            _cciDeliveryPeriods.ProcessExecuteAfterSynchronize(pCci);
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
                ret = _cciDeliveryQuantity.IsClientId_PayerOrReceiver(pCci);
            if (false == ret)
                ret = _cciDeliveryConditions.IsClientId_PayerOrReceiver(pCci);
            if (false == ret)
                ret = _cciDeliveryPeriods.IsClientId_PayerOrReceiver(pCci);

            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();

            _cciDeliveryQuantity.CleanUp();
            _cciDeliveryConditions.CleanUp();
            _cciDeliveryPeriods.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();

            _cciDeliveryQuantity.RefreshCciEnabled();
            _cciDeliveryConditions.RefreshCciEnabled();
            _cciDeliveryPeriods.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);

            _cciDeliveryQuantity.SetDisplay(pCci);
            _cciDeliveryConditions.SetDisplay(pCci);
            _cciDeliveryPeriods.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            _cciDeliveryQuantity.Initialize_Document();
            _cciDeliveryConditions.Initialize_Document();
            _cciDeliveryPeriods.Initialize_Document();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSql_AssetCommodityContract"></param>
        /// FI 20170116 [21916] Modify
        protected override void SetCommodityContractCharacteristics()
        {
            base.SetCommodityContractCharacteristics();

            if (this.Cci(CciEnum.commodityAsset).Sql_Table is SQL_AssetCommodityContract sqlAsset)
            {
                Ccis.SetNewValue(CciClientId(CciEnum.gas_type), sqlAsset.CommodityContract_Type);
                Ccis.SetNewValue(CciClientId(CciEnum.gas_quality), sqlAsset.CommodityContract_Quality);

                //Sur L'écran de saisie light (et en mode importation) les cci quantity, quantityUnit, quantityFrequency ne sont pas nécessairement présents
                //=> Il ya donc alimentation des ccis s'ils sont présents ou alimentation directement du datadocument
                if (ArrFunc.IsFilled(_cciDeliveryQuantity.CciPhysicalQuantity))
                {
                    _cciDeliveryQuantity.CciPhysicalQuantity.ToList().ForEach(item =>
                    {
                        Ccis.SetNewValue(item.CciClientId(CciCommodityNotionalQuantity.CciEnum.quantityUnit),
                            sqlAsset.CommodityContract_UnitOfMeasure);
                        Ccis.SetNewValue(item.CciClientId(CciCommodityNotionalQuantity.CciEnum.quantityFrequency),
                            sqlAsset.CommodityContract_FrequencyQuantity);

                    });
                }
                else
                {
                    IGasPhysicalQuantity gasPhysicalQuantity = _cciDeliveryQuantity.GasPhysicalQuantity;
                    gasPhysicalQuantity.PhysicalQuantitySpecified = true;
                    gasPhysicalQuantity.PhysicalQuantityScheduleSpecified = false;
                    foreach (ICommodityNotionalQuantity item in gasPhysicalQuantity.PhysicalQuantity)
                    {
                        item.QuantityUnit.Value = sqlAsset.CommodityContract_UnitOfMeasure;
                        item.QuantityFrequency.Value = sqlAsset.CommodityContract_FrequencyQuantity; //généralement perCalendarDay
                    }
                }

                //FI 20170116 [21916] Test présence du cci
                //Total exprimé dans la même unité que le prix

                CustomCaptureInfo cci = _cciDeliveryQuantity.Cci(CciGasPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantityUnit);
                if (null != cci)
                {
                    cci.NewValue = sqlAsset.CommodityContract_UnitOfPrice;
                }
                else
                {
                    IGasPhysicalQuantity gasPhysicalQuantity = _cciDeliveryQuantity.GasPhysicalQuantity;
                    gasPhysicalQuantity.TotalPhysicalQuantity.QuantityUnit.Value = sqlAsset.CommodityContract_UnitOfPrice;
                }

                //FI 20170116 [21916] Test présence du cci
                //Delivery Point 
                cci = _cciDeliveryConditions.Cci(CciGasDelivery.CciEnum.deliveryPoint);
                if (null != cci)
                {
                    cci.NewValue = sqlAsset.CommodityContract_DeliveryPoint;
                }
                else
                {
                    _gasPhysicalLeg.DeliveryConditions.DeliveryPointSpecified = true;
                    _gasPhysicalLeg.DeliveryConditions.DeliveryPoint.Value = sqlAsset.CommodityContract_DeliveryPoint;
                }

                CommodityQuantityFrequencyEnum frequency = default;
                if (System.Enum.IsDefined(typeof(CommodityQuantityFrequencyEnum), sqlAsset.CommodityContract_FrequencyQuantity))
                    frequency = (CommodityQuantityFrequencyEnum)ReflectionTools.EnumParse(new CommodityQuantityFrequencyEnum(), sqlAsset.CommodityContract_FrequencyQuantity);

                if ((frequency == CommodityQuantityFrequencyEnum.PerCalendarDay) && sqlAsset.CommodityContract_TradableType == "Spot")
                {

                    string sTimeStart = string.Empty;
                    if (StrFunc.IsFilled(sqlAsset.CommodityContract_TimeStart))
                        sTimeStart = new DtFunc().GetTimeString(sqlAsset.CommodityContract_TimeStart, DtFunc.FmtISOTime);

                    string sTimeEnd = string.Empty;
                    if (StrFunc.IsFilled(sqlAsset.CommodityContract_TimeEnd))
                        sTimeEnd = new DtFunc().GetTimeString(sqlAsset.CommodityContract_TimeEnd, DtFunc.FmtISOTime);

                    Ccis.SetNewValue(_cciDeliveryPeriods.CciSupplyStartTime.CciClientId(CCiPrevailingTime.CciEnum.hourMinuteTime), sTimeStart);
                    Ccis.SetNewValue(_cciDeliveryPeriods.CciSupplyEndTime.CciClientId(CCiPrevailingTime.CciEnum.hourMinuteTime), sTimeEnd);
                }
                else
                {
                    Ccis.SetNewValue(_cciDeliveryPeriods.CciSupplyStartTime.CciClientId(CCiPrevailingTime.CciEnum.hourMinuteTime), string.Empty);
                    Ccis.SetNewValue(_cciDeliveryPeriods.CciSupplyEndTime.CciClientId(CCiPrevailingTime.CciEnum.hourMinuteTime), string.Empty);
                }

                Ccis.SetNewValue(_cciDeliveryPeriods.CciSupplyStartTime.CciClientId(CCiPrevailingTime.CciEnum.location), sqlAsset.CommodityContract_TimeZone);
                Ccis.SetNewValue(_cciDeliveryPeriods.CciSupplyEndTime.CciClientId(CCiPrevailingTime.CciEnum.location), sqlAsset.CommodityContract_TimeZone);
            }
        }

        /// <summary>
        /// Reset des Ccis suite à modification de la plateforme
        /// </summary>
        // EG 20171113 [23509] New 
        public override void ResetCciFacilityHasChanged()
        {
            List<CciEnum> lst = CciTools.GetCciEnum<CciEnum>("FacilityHaschanged").ToList();
            lst.ForEach(item =>
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            });

            _cciDeliveryQuantity.ResetCciFacilityHasChanged();
            _cciDeliveryConditions.ResetCciFacilityHasChanged();
            if (null != _cciDeliveryPeriods.CciSupplyStartTime)
                _cciDeliveryPeriods.CciSupplyStartTime.ResetCciFacilityHasChanged();
            if (null != _cciDeliveryPeriods.CciSupplyEndTime)
                _cciDeliveryPeriods.CciSupplyEndTime.ResetCciFacilityHasChanged();

        }

    }
}

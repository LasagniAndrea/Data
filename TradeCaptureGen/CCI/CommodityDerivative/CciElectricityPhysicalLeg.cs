using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tz = EFS.TimeZone;

namespace EFS.TradeInformation
{
    /// <summary>
    /// cci Physically settled leg of a physically settled electricity transaction.
    /// </summary>
    public class CCiElectricityPhysicalLeg : CCiPhysicalSwapLeg, IContainerCci, IContainerCciFactory, IContainerCciPayerReceiver
    {

        private readonly IElectricityPhysicalLeg _electricityPhysicalLeg;
        //private readonly CciTrade _cciTrade;
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
            [System.Xml.Serialization.XmlEnumAttribute("electricity.type")]
            electricity_type,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("electricity.voltage")]
            electricity_voltage,

            unknown
        }
        #endregion




        /// <summary>
        /// 
        /// </summary>
        private readonly CciElectricityPhysicalQuantity _cciDeliveryQuantity;


        /// <summary>
        /// 
        /// </summary>
        private readonly CciElectricityDelivery _cciDeliveryConditions;

        /// <summary>
        /// 
        /// </summary>
        private CciSettlementPeriods[] _cciSettlementPeriods;


        public override CommodityContractClassEnum CommodityContractClass
        {
            get { return CommodityContractClassEnum.Electricity; }
        }
        // EG 20221201 [25639] [WI484] New
        public override string CommodityProductType
        {
            get { return ElectricityProductType.ToString(); }
        }
        /// EG 20171016 [23509] New
        public override string DeliveryLocation
        {
            get {
                string location = null;
                if (ArrFunc.IsFilled(_cciSettlementPeriods))
                    location = _cciSettlementPeriods[0].CciStartTime.Cci(CCiPrevailingTime.CciEnum.location).NewValue;
                return StrFunc.IsFilled(location) ? location : Tz.Tools.UniversalTimeZone;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CciElectricityPhysicalQuantity CciDeliveryQuantity
        {
            get { return _cciDeliveryQuantity; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CciSettlementPeriods[] CciElectricitySettlementPeriods
        {
            get { return _cciSettlementPeriods; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCCiTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pGasPhysical"></param>
        public CCiElectricityPhysicalLeg(CciTrade pCCiTrade, string pPrefix, IElectricityPhysicalLeg pElectricityPhysicalLeg)
            : base(pCCiTrade, pPrefix + CustomObject.KEY_SEPARATOR, pElectricityPhysicalLeg)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _electricityPhysicalLeg = pElectricityPhysicalLeg;

            _cciDeliveryQuantity = new CciElectricityPhysicalQuantity(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_deliveryQuantity, pElectricityPhysicalLeg.DeliveryQuantity);
            _cciDeliveryConditions = new CciElectricityDelivery(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_deliveryConditions, _electricityPhysicalLeg.DeliveryConditions);

        }



        #region Membres de IContainerCci
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
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
            CciTools.CreateInstance(this, _electricityPhysicalLeg);

            _cciDeliveryQuantity.Initialize_FromCci();
            _cciDeliveryConditions.Initialize_FromCci(); 

            InitializeSettlementPediod_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.electricity_type), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.electricity_voltage), false, TypeData.TypeDataEnum.@string);

            _cciDeliveryQuantity.AddCciSystem();
            _cciDeliveryConditions.AddCciSystem();

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.AddCciSystem();
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
                        case CciEnum.electricity_type:
                            data = _electricityPhysicalLeg.Electricity.TypeElectricity.ToString();
                            break;
                        case CciEnum.electricity_voltage:
                            if (_electricityPhysicalLeg.Electricity.VoltageSpecified)
                                _electricityPhysicalLeg.Electricity.Voltage.Value = data;
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

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.Initialize_FromDocument();

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
                        case CciEnum.electricity_voltage:
                            _electricityPhysicalLeg.Electricity.VoltageSpecified = BoolFunc.IsTrue(data);
                            if (_electricityPhysicalLeg.Electricity.VoltageSpecified)
                                _electricityPhysicalLeg.Electricity.Voltage.Value = data;
                            break;
                        case CciEnum.electricity_type:
                            if (StrFunc.IsFilled(data))
                                _electricityPhysicalLeg.Electricity.TypeElectricity = (ElectricityProductTypeEnum)(Enum.Parse(typeof(ElectricityProductTypeEnum), data));
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

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.Dump_ToDocument();

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

            _cciDeliveryQuantity.ProcessInitialize(pCci);

            _cciDeliveryConditions.ProcessInitialize(pCci);

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.ProcessInitialize(pCci);

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

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.ProcessExecute(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            base.ProcessExecuteAfterSynchronize(pCci);
            //Nothing TODO
            _cciDeliveryQuantity.ProcessExecuteAfterSynchronize(pCci);
            _cciDeliveryConditions.ProcessExecuteAfterSynchronize(pCci);

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.ProcessExecuteAfterSynchronize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return base.IsClientId_PayerOrReceiver(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();

            _cciDeliveryQuantity.CleanUp();
            _cciDeliveryConditions.CleanUp();

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.CleanUp();

        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();

            _cciDeliveryQuantity.RefreshCciEnabled();
            _cciDeliveryConditions.RefreshCciEnabled();

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.RefreshCciEnabled();

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

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.SetDisplay(pCci);

            for (int i = 0; i < ArrFunc.Count(_cciSettlementPeriods); i++)
            {
                if (ArrFunc.IsFilled(_cciDeliveryQuantity.CciPhysicalQuantity))
                {
                    if (_cciDeliveryQuantity.CciPhysicalQuantity[i].IsCci(CciCommodityNotionalQuantity.CciEnum.quantityFrequency, pCci))
                    {
                        if (_cciDeliveryQuantity.CciPhysicalQuantity[i].CommodityNotionalQuantity.QuantityFrequency.Value ==
                                CommodityQuantityFrequencyEnum.PerSettlementPeriod.ToString())
                        {
                            pCci.Display = _cciSettlementPeriods[i].SettlementPeriods.Duration.ToString();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            base.Initialize_Document();
            _cciDeliveryQuantity.Initialize_Document();
            _cciDeliveryConditions.Initialize_Document();

            foreach (CciSettlementPeriods item in _cciSettlementPeriods)
                item.Initialize_Document();
        }
        #endregion




        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsPremium"></param>
        private void InitializeSettlementPediod_FromCci()
        {
            ArrayList lst = new ArrayList();

            ISettlementPeriods[] settlementPeriods = _electricityPhysicalLeg.SettlementPeriods;

            bool isOk = true;
            int index = -1;
            while (isOk)
            {
                index += 1;
                string prefix = TradeCustomCaptureInfos.CCst.Prefix_settlementPeriods;
                if (index > 0)
                    prefix += index.ToString();

                CciSettlementPeriods cciSettlementPeriodsItem = new CciSettlementPeriods(_cciTrade as CciTrade, _prefix + prefix, null);
                isOk = Ccis.Contains(cciSettlementPeriodsItem.CciStartTime.CciClientId(CCiPrevailingTime.CciEnum.hourMinuteTime));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(settlementPeriods) || (index == settlementPeriods.Length))
                    {
                        ReflectionTools.AddItemInArray(_electricityPhysicalLeg, "settlementPeriods", index);
                        settlementPeriods = _electricityPhysicalLeg.SettlementPeriods;
                    }

                    cciSettlementPeriodsItem.SettlementPeriods = _electricityPhysicalLeg.SettlementPeriods[index];
                    cciSettlementPeriodsItem.CciStartTime.PrevailingTime  = _electricityPhysicalLeg.SettlementPeriods[index].StartTime.Time;
                    cciSettlementPeriodsItem.CciEndTime.PrevailingTime = _electricityPhysicalLeg.SettlementPeriods[index].EndTime.Time;

                    lst.Add(cciSettlementPeriodsItem);
                }
            }

            CciSettlementPeriods[] cciSettlementPeriods = new CciSettlementPeriods[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                cciSettlementPeriods[i] = (CciSettlementPeriods)lst[i];
                cciSettlementPeriods[i].Initialize_FromCci();
            }

            _cciSettlementPeriods = cciSettlementPeriods;
        }

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
                Ccis.SetNewValue(CciClientId(CciEnum.electricity_type), sqlAsset.CommodityContract_Type);

                //Sur L'écran de saisie light (et en mode importation) les cci quantity, quantityUnit, quantityFrequency ne sont pas nécessairement présents
                //=> Il ya donc alimentation des ccis s'ils sont présents ou aliemntation directement du datadocument
                if (ArrFunc.IsFilled(_cciDeliveryQuantity.CciPhysicalQuantity))
                {
                    _cciDeliveryQuantity.CciPhysicalQuantity.ToList().ForEach(item =>
                    {
                        Ccis.SetNewValue(item.CciClientId(CciCommodityNotionalQuantity.CciEnum.quantityUnit), sqlAsset.CommodityContract_UnitOfMeasure);
                        Ccis.SetNewValue(item.CciClientId(CciCommodityNotionalQuantity.CciEnum.quantityFrequency), sqlAsset.CommodityContract_FrequencyQuantity); //généralement PerSettlementPeriod
                    });
                }
                else
                {
                    IElectricityPhysicalQuantity electricityPhysicalQuantity = _cciDeliveryQuantity.ElectricityPhysicalQuantity;
                    electricityPhysicalQuantity.PhysicalQuantitySpecified = true;
                    electricityPhysicalQuantity.PhysicalQuantityScheduleSpecified = false;
                    foreach (ICommodityNotionalQuantity item in electricityPhysicalQuantity.PhysicalQuantity)
                    {
                        item.QuantityUnit.Value = sqlAsset.CommodityContract_UnitOfMeasure;
                        item.QuantityFrequency.Value = sqlAsset.CommodityContract_FrequencyQuantity;
                    }
                }

                // FI 20170116 [21916] test présence cci
                //Total exprimé dans la même unité que le prix
                CustomCaptureInfo cci = _cciDeliveryQuantity.Cci(CciElectricityPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantityUnit);
                if (null != cci)
                {
                    cci.NewValue = sqlAsset.CommodityContract_UnitOfPrice;
                }
                else
                {
                    IElectricityPhysicalQuantity physicalQuantity = _cciDeliveryQuantity.ElectricityPhysicalQuantity;
                    physicalQuantity.TotalPhysicalQuantity.QuantityUnit.Value = sqlAsset.CommodityContract_UnitOfPrice;
                }

                // FI 20170116 [21916] test présence cci
                //Delivery Point 
                cci = _cciDeliveryConditions.Cci(CciElectricityDelivery.CciEnum.deliveryPoint);
                if (null != cci)
                {
                    cci.NewValue = sqlAsset.CommodityContract_DeliveryPoint;
                }
                else
                {
                    _electricityPhysicalLeg.DeliveryConditions.DeliveryPointSpecified = true;
                    _electricityPhysicalLeg.DeliveryConditions.DeliveryPoint.Value = sqlAsset.CommodityContract_DeliveryPoint;
                }

                _cciSettlementPeriods.ToList().ForEach(item =>
                {
                    Ccis.SetNewValue(item.CciClientId(CciSettlementPeriods.CciEnum.duration), sqlAsset.CommodityContract_Duration);
                    Ccis.SetNewValue(item.CciStartTime.CciClientId(CCiPrevailingTime.CciEnum.location), sqlAsset.CommodityContract_TimeZone);
                    Ccis.SetNewValue(item.CciEndTime.CciClientId(CCiPrevailingTime.CciEnum.location), sqlAsset.CommodityContract_TimeZone);
                });
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
            _cciSettlementPeriods.Cast<CciSettlementPeriods>().ToList().ForEach(item => item.ResetCciFacilityHasChanged());
        }
    }
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region public class CciEquityExercise
    /// <summary>
    /// Description résumée de CciExercise. 
    /// </summary>
    public class CciEquityExercise : IContainerCciFactory, IContainerCci, IContainerCciSpecified
    {
        #region Members
        private readonly CciTradeBase _cciTrade;
        private ExerciseTypeEnum _exerciseType;
        private object exercise;
        private readonly string prefix;
        #endregion Members

        #region Enums
        #region CciEnumEquitySharedExercice
        public enum CciEnumEquitySharedExercice
        {
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            expirationDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            expirationDate_adjustableDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCentersReference")]
            expirationDate_adjustableDate_dateAdjustments_bCSR,
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCenters")]
            expirationDate_adjustableDate_dateAdjustments_bCS,
            //
            [System.Xml.Serialization.XmlEnumAttribute("equityExpirationTimeType")]
            equityExpirationTimeType,
            [System.Xml.Serialization.XmlEnumAttribute("equityExpirationTime.hourMinuteTime")]
            equityExpirationTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("equityExpirationTime.businessCenter")]
            equityExpirationTime_businessCenter,
            unknown
        }
        #endregion
        #region CciEnumEquitySharedAmericanExercice
        public enum CciEnumEquitySharedAmericanExercice
        {
            [System.Xml.Serialization.XmlEnumAttribute("commencementDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            commencementDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("commencementDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            commencementDate_adjustableDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("commencementDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCentersReference")]
            commencementDate_adjustableDate_dateAdjustments_bCSR,
            [System.Xml.Serialization.XmlEnumAttribute("commencementDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCenters")]
            commencementDate_adjustableDate_dateAdjustments_bCS,
            //
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTimeType")]
            latestExerciseTimeType,
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTime.hourMinuteTime")]
            latestExerciseTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTime.businessCenter")]
            latestExerciseTime_businessCenter,
            unknown
        }
        #endregion CciEnumAmericanExercise
        #region ExerciseTypeEnum
        public enum ExerciseTypeEnum
        {
            american,
            bermuda,
            european,
            unknown
        }
        #endregion ExerciseTypeEnum
        #endregion Enums

        #region Accessors
        #region equityEuropeanExercise
        public IEquityEuropeanExercise EquityEuropeanExercise
        {
            get { return (IEquityEuropeanExercise)this.exercise; }
        }
        #endregion
        #region equityAmericanExercise
        public IEquityAmericanExercise EquityAmericanExercise
        {
            get { return (IEquityAmericanExercise)this.exercise; }
        }
        #endregion
        #region equityBermudaExercise
        public IEquityBermudaExercise EquityBermudaExercise
        {
            get { return (IEquityBermudaExercise)this.exercise; }
        }
        #endregion
        //
        #region exerciseTypeEnum
        public ExerciseTypeEnum ExerciseType
        {
            set { _exerciseType = value; }
            get { return _exerciseType; }
        }
        #endregion ExerciseTypeEnum
        #region isEuropeanExercice
        public bool IsEuropeanExercice
        {
            get { return (ExerciseTypeEnum.european == ExerciseType); }
        }
        #endregion
        #region isAmericanExercice
        public bool IsAmericanExercice
        {
            get { return (ExerciseTypeEnum.american == ExerciseType); }
        }
        #endregion
        #region isBermudaExercice
        public bool IsBermudaExercice
        {
            get { return (ExerciseTypeEnum.bermuda == ExerciseType); }
        }
        #endregion
        //
        #region Exercise
        public object Exercise
        {
            get { return exercise; }
        }
        #endregion Exercise
        //
        #region ccis
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        #endregion ccis
        #endregion Accessors

        #region Constructors
        public CciEquityExercise(CciTradeBase pTrade, string pPrefix)
        {
            _cciTrade = pTrade;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion Constructors

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CreateInstance();
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            if (IsAmericanExercice || IsBermudaExercice)
            {
                CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnumEquitySharedAmericanExercice.latestExerciseTime_hourMinuteTime), true, TypeData.TypeDataEnum.@time);
                CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnumEquitySharedAmericanExercice.latestExerciseTime_businessCenter), true, TypeData.TypeDataEnum.@string);
            }

            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnumEquitySharedExercice.equityExpirationTime_businessCenter), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnumEquitySharedExercice.equityExpirationTimeType), true, TypeData.TypeDataEnum.@string);

            //Don't erase
            CreateInstance();
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {
                string data;
                bool isSetting;
                SQL_Table sql_Table;
                string clientId_Key;
                foreach (CustomCaptureInfo cci in Ccis)
                {
                    if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                    {
                        #region Reset variables
                        clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion
                        //
                        CciEnumEquitySharedExercice keyShared = CciEnumEquitySharedExercice.unknown;
                        if (Enum.IsDefined(typeof(CciEnumEquitySharedExercice), clientId_Key))
                            keyShared = (CciEnumEquitySharedExercice)Enum.Parse(typeof(CciEnumEquitySharedExercice), clientId_Key);

                        switch (keyShared)
                        {
                            case CciEnumEquitySharedExercice.expirationDate_adjustableDate_unadjustedDate:
                                if (((IEquityExercise)(exercise)).ExpirationDate.AdjustableDateSpecified)
                                    data = ((IEquityExercise)(exercise)).ExpirationDate.AdjustableDate.UnadjustedDate.Value;
                                break;
                            case CciEnumEquitySharedExercice.expirationDate_adjustableDate_dateAdjustments_bDC:
                                if (((IEquityExercise)(exercise)).ExpirationDate.AdjustableDateSpecified)
                                    data = ((IEquityExercise)(exercise)).ExpirationDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                break;
                            case CciEnumEquitySharedExercice.equityExpirationTimeType:
                                data = ((IEquityExercise)(exercise)).EquityExpirationTimeType.ToString();
                                break;
                            case CciEnumEquitySharedExercice.equityExpirationTime_businessCenter:
                                if (((IEquityExercise)(exercise)).EquityExpirationTimeSpecified)
                                    data = ((IEquityExercise)(exercise)).EquityExpirationTime.BusinessCenter.Value;
                                break;
                            case CciEnumEquitySharedExercice.equityExpirationTime_hourMinuteTime:
                                if (((IEquityExercise)(exercise)).EquityExpirationTimeSpecified)
                                    data = ((IEquityExercise)(exercise)).EquityExpirationTime.HourMinuteTime.Value;
                                break;
                            default:
                                isSetting = false;
                                break;
                        }
                        //
                        if ((false == isSetting) && (IsAmericanExercice || IsBermudaExercice))
                        {
                            CciEnumEquitySharedAmericanExercice keyShared2 = CciEnumEquitySharedAmericanExercice.unknown;
                            if (Enum.IsDefined(typeof(CciEnumEquitySharedAmericanExercice), clientId_Key))
                                keyShared2 = (CciEnumEquitySharedAmericanExercice)Enum.Parse(typeof(CciEnumEquitySharedAmericanExercice), clientId_Key);

                            switch (keyShared2)
                            {
                                case CciEnumEquitySharedAmericanExercice.commencementDate_adjustableDate_unadjustedDate:
                                    if (((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDateSpecified)
                                        data = ((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDate.UnadjustedDate.Value;
                                    break;
                                case CciEnumEquitySharedAmericanExercice.commencementDate_adjustableDate_dateAdjustments_bDC:
                                    if (((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDateSpecified)
                                        data = ((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                    break;
                                case CciEnumEquitySharedAmericanExercice.latestExerciseTimeType:
                                    data = ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTimeType.ToString();
                                    break;
                                case CciEnumEquitySharedAmericanExercice.latestExerciseTime_businessCenter:
                                    if (((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTimeSpecified)
                                        data = ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTime.BusinessCenter.Value;
                                    break;
                                case CciEnumEquitySharedAmericanExercice.latestExerciseTime_hourMinuteTime:
                                    if (((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTimeSpecified)
                                        data = ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTime.HourMinuteTime.Value;
                                    break;

                                default:
                                    isSetting = false;
                                    break;
                            }
                        }
                        //
                        if (isSetting)
                            Ccis.InitializeCci(cci, sql_Table, data);
                    }
                }
            
        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            string clientId;
            string clientId_Key;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            foreach (CustomCaptureInfo cci in Ccis)
            {
                //On ne traite que les contrôle dont le contenu à changé
                if (cci.HasChanged && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId = cci.ClientId;
                    data = cci.NewValue;
                    isSetting = true;
                    clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    #endregion
                    //
                    CciEnumEquitySharedExercice keyShared = CciEnumEquitySharedExercice.unknown;
                    if (Enum.IsDefined(typeof(CciEnumEquitySharedExercice), clientId_Key))
                        keyShared = (CciEnumEquitySharedExercice)Enum.Parse(typeof(CciEnumEquitySharedExercice), clientId_Key);

                    switch (keyShared)
                    {
                        #region expirationDate
                        case CciEnumEquitySharedExercice.expirationDate_adjustableDate_unadjustedDate:
                            ((IEquityExercise)(exercise)).ExpirationDate.AdjustableDateSpecified = StrFunc.IsFilled(data);
                            if (StrFunc.IsFilled(data))
                                ((IEquityExercise)(exercise)).ExpirationDate.AdjustableDate.UnadjustedDate.Value = data;
                            break;
                        case CciEnumEquitySharedExercice.expirationDate_adjustableDate_dateAdjustments_bDC:
                            if (((IEquityExercise)(exercise)).ExpirationDate.AdjustableDateSpecified)
                                ((IEquityExercise)(exercise)).ExpirationDate.AdjustableDate.DateAdjustments.BusinessDayConvention = StringToEnum.BusinessDayConvention(data);

                            break;
                        #endregion
                        #region equityExpirationTimeType
                        case CciEnumEquitySharedExercice.equityExpirationTimeType:
                            ((IEquityExercise)(exercise)).EquityExpirationTimeType = (TimeTypeEnum)Enum.Parse(typeof(TimeTypeEnum), data, true);
                            break;
                        #endregion
                        #region equityExpirationTime_hourMinuteTime
                        case CciEnumEquitySharedExercice.equityExpirationTime_hourMinuteTime:
                            ((IEquityExercise)(exercise)).EquityExpirationTimeSpecified = StrFunc.IsFilled(data);
                            if (StrFunc.IsFilled(data))
                                ((IEquityExercise)(exercise)).EquityExpirationTime.HourMinuteTime.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        case CciEnumEquitySharedExercice.equityExpirationTime_businessCenter:
                            ((IEquityExercise)(exercise)).EquityExpirationTime.BusinessCenter.Value = data;
                            break;
                        #endregion
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion

                    }
                    //
                    if ((false == isSetting) && (IsAmericanExercice || IsBermudaExercice))
                    {
                        CciEnumEquitySharedAmericanExercice keyShared2 = CciEnumEquitySharedAmericanExercice.unknown;
                        if (Enum.IsDefined(typeof(CciEnumEquitySharedAmericanExercice), clientId_Key))
                            keyShared2 = (CciEnumEquitySharedAmericanExercice)Enum.Parse(typeof(CciEnumEquitySharedAmericanExercice), clientId_Key);

                        switch (keyShared2)
                        {
                            case CciEnumEquitySharedAmericanExercice.commencementDate_adjustableDate_unadjustedDate:
                                ((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDateSpecified = StrFunc.IsFilled(data);
                                if (StrFunc.IsFilled(data))
                                    ((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDate.UnadjustedDate.Value = data;
                                break;
                            case CciEnumEquitySharedAmericanExercice.commencementDate_adjustableDate_dateAdjustments_bDC:
                                if (((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDateSpecified)
                                    ((IEquitySharedAmericanExercise)(exercise)).CommencementDate.AdjustableDate.DateAdjustments.BusinessDayConvention = StringToEnum.BusinessDayConvention(data);
                                break;
                            case CciEnumEquitySharedAmericanExercice.latestExerciseTimeType:
                                ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTimeType = (TimeTypeEnum)Enum.Parse(typeof(TimeTypeEnum), data, true);
                                break;
                            case CciEnumEquitySharedAmericanExercice.latestExerciseTime_businessCenter:
                                ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTimeSpecified = StrFunc.IsFilled(data);
                                if (StrFunc.IsFilled(data))
                                    ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTime.BusinessCenter.Value = data;
                                break;
                            case CciEnumEquitySharedAmericanExercice.latestExerciseTime_hourMinuteTime:
                                ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTimeSpecified = StrFunc.IsFilled(data);
                                if (StrFunc.IsFilled(data))
                                    ((IEquitySharedAmericanExercise)(exercise)).LatestExerciseTime.HourMinuteTime.Value = data;
                                break;

                            default:
                                isSetting = false;
                                break;
                        }
                    }
                    //
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //              
                CciEnumEquitySharedExercice key = CciEnumEquitySharedExercice.unknown;
                if (Enum.IsDefined(typeof(CciEnumEquitySharedExercice), clientId_Element))
                    key = (CciEnumEquitySharedExercice)Enum.Parse(typeof(CciEnumEquitySharedExercice), clientId_Element);
                //                        
                switch (key)
                {
                    case CciEnumEquitySharedExercice.equityExpirationTime_hourMinuteTime:
                        if (pCci.IsFilledValue)
                            Ccis.SetNewValue(CciClientId(CciEnumEquitySharedExercice.equityExpirationTimeType), TimeTypeEnum.SpecificTime.ToString());
                        else
                            Ccis.SetNewValue(CciClientId(CciEnumEquitySharedExercice.equityExpirationTimeType), TimeTypeEnum.Close.ToString());

                        if (IsAmericanExercice || IsBermudaExercice)
                            Ccis.SetNewValue(CciClientId(CciEnumEquitySharedAmericanExercice.latestExerciseTime_hourMinuteTime), pCci.NewValue);
                        //
                        break;
                    default:
                        break;
                }
            }
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
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            return isOk;
        }
        #endregion
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion
        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnumEquitySharedExercice pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        public bool IsCciClientId(CciEnumEquitySharedAmericanExercice pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        #region CciClientId
        public string CciClientId(CciEnumEquitySharedExercice pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(CciEnumEquitySharedAmericanExercice pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnumEquitySharedExercice pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(CciEnumEquitySharedAmericanExercice pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnumEquitySharedExercice pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumEquitySharedAmericanExercice pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }

        #endregion
        #endregion ITradeCci
        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return Cci(CciEnumEquitySharedExercice.equityExpirationTime_hourMinuteTime).IsFilled; } }
        #endregion IContainerCciSpecified

        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnumEquitySharedExercice", "NewValue", string.Empty);
            CciTools.SetCciContainer(this, "CciEnumEquitySharedAmericanExercice", "NewValue", string.Empty);
        }
        #endregion
        #region public SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
            CciTools.SetCciContainer(this, "CciEnumEquitySharedExercice", "IsEnabled", pIsEnabled);
            CciTools.SetCciContainer(this, "CciEnumEquitySharedAmericanExercice", "IsEnabled", pIsEnabled);
        }
        #endregion
        #region public SetExercise
        public void SetExercise(ExerciseTypeEnum pExerciseType, IEquityExerciseValuationSettlement pProduct)
        {
            ExerciseType = pExerciseType;

            pProduct.EquityExerciseEuropeanSpecified = (ExerciseTypeEnum.european == pExerciseType);
            pProduct.EquityExerciseAmericanSpecified = (ExerciseTypeEnum.american == pExerciseType);
            pProduct.EquityExerciseBermudaSpecified = (ExerciseTypeEnum.bermuda == pExerciseType);

            switch (ExerciseType)
            {
                case ExerciseTypeEnum.european:
                    exercise = pProduct.EquityExerciseEuropean;
                    break;
                case ExerciseTypeEnum.american:
                    exercise = pProduct.EquityExerciseAmerican;
                    break;
                case ExerciseTypeEnum.bermuda:
                    exercise = pProduct.EquityExerciseBermuda;
                    break;
            }
        }
        #endregion

        #region private CreateInstance
        private void CreateInstance()
        {
            CciTools.CreateInstance(this, exercise, "CciEnumEquitySharedExercice");
            if (IsAmericanExercice || IsBermudaExercice)
                CciTools.CreateInstance(this, exercise, "CciEnumEquitySharedAmericanExercice");
        }
        #endregion
    }
    #endregion
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using FpML.Interface;
using System;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de CciExercise. 
    /// </summary>
    public class CciExercise : IContainerCciFactory, IContainerCci, IContainerCciSpecified
    {
        #region Members
        private readonly CciTradeBase trade;
        private Nullable<ExerciseTypeEnum> _exerciseType;
        private object exercise;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly string prefix;
        #endregion Members

        #region Enums
        #region CciEnumAmericanExercise
        public enum CciEnumAmericanExercise
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
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            expirationDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            expirationDate_adjustableDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCentersReference")]
            expirationDate_adjustableDate_dateAdjustments_bCSR,
            [System.Xml.Serialization.XmlEnumAttribute("expirationDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessCenters")]
            expirationDate_adjustableDate_dateAdjustments_bCS,
            //
            [System.Xml.Serialization.XmlEnumAttribute("earliestExerciseTime.hourMinuteTime")]
            earliestExerciseTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("earliestExerciseTime.businessCenter")]
            earliestExerciseTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTime.hourMinuteTime")]
            latestExerciseTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTime.businessCenter")]
            latestExerciseTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("expirationTime.hourMinuteTime")]
            expirationTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("expirationTime.businessCenter")]
            expirationTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("multipleExerciseSpecified")]
            multiple_specified,
            [System.Xml.Serialization.XmlEnumAttribute("multipleExercise")]
            multiple,
            unknown
        }
        #endregion CciEnumAmericanExercise
        #region CciEnumBermudaExercise
        public enum CciEnumBermudaExercise
        {
            [System.Xml.Serialization.XmlEnumAttribute("bermudaExerciseDates.adjustableOrRelativeDatesAdjustableDates.unadjustedDate")]
            bermudaExerciseDates_adjustableDates_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("bermudaExerciseDates.adjustableOrRelativeDatesAdjustableDates.dateAdjustments.businessDayConvention")]
            bermudaExerciseDates_adjustableDates_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("bermudaExerciseDates.adjustableOrRelativeDatesAdjustableDates.dateAdjustments.businessCentersReference")]
            bermudaExerciseDates_adjustableDates_dateAdjustments_bCSR,
            [System.Xml.Serialization.XmlEnumAttribute("bermudaExerciseDates.adjustableOrRelativeDatesAdjustableDates.dateAdjustments.businessCenters")]
            bermudaExerciseDates_adjustableDates_dateAdjustments_bCS,
            //
            [System.Xml.Serialization.XmlEnumAttribute("earliestExerciseTime.hourMinuteTime")]
            earliestExerciseTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("earliestExerciseTime.businessCenter")]
            earliestExerciseTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTime.hourMinuteTime")]
            latestExerciseTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("latestExerciseTime.businessCenter")]
            latestExerciseTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("expirationTime.hourMinuteTime")]
            expirationTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("expirationTime.businessCenter")]
            expirationTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("multipleExerciseSpecified")]
            multiple_specified,
            [System.Xml.Serialization.XmlEnumAttribute("multipleExercise")]
            multiple,
            unknown
        }
        #endregion CciEnumBermudaExercise
        #region CciEnumEuropeanExercise
        public enum CciEnumEuropeanExercise
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
            [System.Xml.Serialization.XmlEnumAttribute("earliestExerciseTime.hourMinuteTime")]
            earliestExerciseTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("earliestExerciseTime.businessCenter")]
            earliestExerciseTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("expirationTime.hourMinuteTime")]
            expirationTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("expirationTime.businessCenter")]
            expirationTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("partialExerciseSpecified")]
            partial_specified,
            [System.Xml.Serialization.XmlEnumAttribute("partialExercise")]
            partial,
            unknown
        }
        #endregion CciEnumEuropeanExercise
        #region CciEnumExerciseAB
        public enum CciEnumExerciseAB
        {
            latestExerciseTime_hourMinuteTime,
            latestExerciseTime_businessCenter,
            unknown
        }
        #endregion CciEnumExerciseAB
        #region CciEnumExerciseAE
        public enum CciEnumExerciseAE
        {
            expirationDate_adjustableDate_unadjustedDate,
            expirationDate_adjustableDate_dateAdjustments_bDC,
            expirationDate_adjustableDate_dateAdjustments_bCSR,
            expirationDate_adjustableDate_dateAdjustments_bCS,
            unknown
        }
        #endregion CciEnumExerciseAE
        #region CciEnumExercise
        public enum CciEnumExercise
        {
            earliestExerciseTime_hourMinuteTime,
            earliestExerciseTime_businessCenter,
            expirationTime_hourMinuteTime,
            expirationTime_businessCenter,
            unknown
        }
        #endregion CciEnumExercise
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
        #region AmericanExercise
        public IAmericanExercise AmericanExercise
        {
            get { return (IAmericanExercise)this.exercise; }
        }
        #endregion AmericanExercise
        #region BermudaExercise
        public IBermudaExercise BermudaExercise
        {
            get { return (IBermudaExercise)this.exercise; }
        }
        #endregion BermudaExercise
        #region EuropeanExercise
        public IEuropeanExercise EuropeanExercise
        {
            get { return (IEuropeanExercise)this.exercise; }
        }
        #endregion EuropeanExercise
        #region ExerciseBase
        public IExerciseBase ExerciseBase
        {
            get { return (IExerciseBase)this.exercise; }
        }
        #endregion ExerciseBase
        #region ExerciseTypeEnum
        public Nullable<ExerciseTypeEnum> ExerciseType
        {
            set { _exerciseType = value; }
            get { return _exerciseType; }
        }
        #endregion ExerciseTypeEnum
        #region Exercise
        public object Exercise
        {
            get { return this.exercise; }
        }
        #endregion Exercise
        #region IsExercise...
        /// <summary>
        /// American exercise
        /// </summary>
        public bool IsExerciseA
        {
            get { return ExerciseType.HasValue && (ExerciseTypeEnum.american == ExerciseType.Value); }
        }
        /// <summary>
        /// Bermudan exercise
        /// </summary>
        public bool IsExerciseB
        {
            get { return ExerciseType.HasValue && (ExerciseTypeEnum.bermuda == ExerciseType.Value); }
        }
        /// <summary>
        /// European exercise
        /// </summary>
        public bool IsExerciseE
        {
            get { return ExerciseType.HasValue && (ExerciseTypeEnum.european == ExerciseType.Value); }
        }
        /// <summary>
        /// American or Bermudan exercise
        /// </summary>
        public bool IsExerciseAB
        {
            get { return (IsExerciseA || IsExerciseB); }
        }
        /// <summary>
        /// American or European exercise
        /// </summary>
        public bool IsExerciseAE
        {
            get { return (IsExerciseA || IsExerciseE); }
        }
        #endregion
        #region ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        #endregion ccis
        #endregion Accessors

        #region Constructors
        public CciExercise(CciTradeBase pTrade, string pPrefixExercise)
        {
            trade = pTrade;
            _ccis = pTrade.Ccis;
            prefix = pPrefixExercise + CustomObject.KEY_SEPARATOR;
            ExerciseType = null;
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
        public void AddCciSystem()
        {
            if (IsExerciseE)
                CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnumEuropeanExercise.partial), false, TypeData.TypeDataEnum.@string);

            if (IsExerciseA)
                CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnumAmericanExercise.multiple), false, TypeData.TypeDataEnum.@string);

            if (IsExerciseAB)
            {
                CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnumExerciseAB.latestExerciseTime_hourMinuteTime), true, TypeData.TypeDataEnum.@time);
                CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnumExerciseAB.latestExerciseTime_businessCenter), true, TypeData.TypeDataEnum.@string);
            }

            if (IsExerciseAE)
            {
                if (Ccis.Contains(CciClientId(CciEnumExerciseAE.expirationDate_adjustableDate_unadjustedDate)))
                    CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnumExerciseAE.expirationDate_adjustableDate_dateAdjustments_bDC), true, TypeData.TypeDataEnum.@string);
            }
            else if (IsExerciseB)
            {
                if (Ccis.Contains(CciClientId(CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_unadjustedDate)))
                    CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_dateAdjustments_bDC), true, TypeData.TypeDataEnum.@string);

                CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnumBermudaExercise.multiple), false, TypeData.TypeDataEnum.@string);

            }

            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnumExercise.expirationTime_businessCenter), true, TypeData.TypeDataEnum.@string);

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

            foreach (CustomCaptureInfo cci in _ccis)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    string clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    #region Each exercise type
                    if (ExerciseType.HasValue)
                    {
                        switch (ExerciseType.Value)
                        {
                            #region American
                            case ExerciseTypeEnum.american:
                                CciEnumAmericanExercise keyAmerican = CciEnumAmericanExercise.unknown;
                                if (System.Enum.IsDefined(typeof(CciEnumAmericanExercise), clientId_Key))
                                    keyAmerican = (CciEnumAmericanExercise)System.Enum.Parse(typeof(CciEnumAmericanExercise), clientId_Key);
                                //
                                switch (keyAmerican)
                                {
                                    #region commencementDate_adjustableDate_unadjustedDate
                                    case CciEnumAmericanExercise.commencementDate_adjustableDate_unadjustedDate:
                                        data = AmericanExercise.CommencementDate.AdjustableDate.UnadjustedDate.Value;
                                        break;
                                    #endregion
                                    #region commencementDate_adjustableDate_dateAdjustments_bDC
                                    case CciEnumAmericanExercise.commencementDate_adjustableDate_dateAdjustments_bDC:
                                        data = AmericanExercise.CommencementDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                        break;
                                    #endregion

                                    case CciEnumAmericanExercise.multiple_specified:
                                        data = AmericanExercise.MultipleExerciseSpecified.ToString().ToLower();
                                        break;

                                    #region default
                                    default:
                                        isSetting = false;
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion
                            #region Bermuda
                            case ExerciseTypeEnum.bermuda:
                                CciEnumBermudaExercise keyBermuda = CciEnumBermudaExercise.unknown;
                                if (System.Enum.IsDefined(typeof(CciEnumBermudaExercise), clientId_Key))
                                    keyBermuda = (CciEnumBermudaExercise)System.Enum.Parse(typeof(CciEnumBermudaExercise), clientId_Key);
                                //
                                switch (keyBermuda)
                                {
                                    #region bermudaExerciseDates_adjustableDates_unadjustedDate
                                    case CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_unadjustedDate:
                                        if (BermudaExercise.BermudaExerciseDates.AdjustableDatesSpecified)
                                        {
                                            if (ArrFunc.IsFilled(BermudaExercise.BermudaExerciseDates.AdjustableDates.UnadjustedDate))
                                                data = BermudaExercise.BermudaExerciseDates.AdjustableDates.UnadjustedDate[0].Value;
                                        }
                                        break;
                                    #endregion
                                    #region bermudaExerciseDates_adjustableDates_dateAdjustments_bDC
                                    case CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_dateAdjustments_bDC:
                                        data = BermudaExercise.BermudaExerciseDates.AdjustableDates.DateAdjustments.BusinessDayConvention.ToString();
                                        break;
                                    #endregion

                                    case CciEnumBermudaExercise.multiple_specified:
                                        data = BermudaExercise.MultipleExerciseSpecified.ToString().ToLower();
                                        break;

                                    #region default
                                    default:
                                        isSetting = false;
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion
                            #region European
                            case ExerciseTypeEnum.european:
                                CciEnumEuropeanExercise keyEuropean = CciEnumEuropeanExercise.unknown;
                                if (System.Enum.IsDefined(typeof(CciEnumEuropeanExercise), clientId_Key))
                                    keyEuropean = (CciEnumEuropeanExercise)System.Enum.Parse(typeof(CciEnumEuropeanExercise), clientId_Key);

                                switch (keyEuropean)
                                {
                                    #region default
                                    default:
                                        isSetting = false;
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion
                            default:
                                throw new Exception("Exercise Type unknown");
                        }
                    }
                    #endregion

                    #region American and Bermudan exercise types
                    if (!isSetting && IsExerciseAB)
                    {
                        isSetting = true;

                        CciEnumExerciseAB keyExerciseAB = CciEnumExerciseAB.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumExerciseAB), clientId_Key))
                            keyExerciseAB = (CciEnumExerciseAB)System.Enum.Parse(typeof(CciEnumExerciseAB), clientId_Key);

                        switch (keyExerciseAB)
                        {
                            #region latestExerciseTime
                            case CciEnumExerciseAB.latestExerciseTime_hourMinuteTime:
                                if (IsExerciseA)
                                    data = AmericanExercise.LatestExerciseTime.HourMinuteTime.Value;
                                else if (IsExerciseB)
                                    data = BermudaExercise.LatestExerciseTime.HourMinuteTime.Value;
                                break;
                            case CciEnumExerciseAB.latestExerciseTime_businessCenter:
                                if (IsExerciseA)
                                    data = AmericanExercise.LatestExerciseTime.BusinessCenter.Value;
                                else if (IsExerciseB)
                                    data = BermudaExercise.LatestExerciseTime.BusinessCenter.Value;
                                break;
                            #endregion

                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion

                    #region American and European exercise types
                    if (!isSetting && IsExerciseAE)
                    {
                        isSetting = true;

                        CciEnumExerciseAE keyExerciseAE = CciEnumExerciseAE.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumExerciseAE), clientId_Key))
                            keyExerciseAE = (CciEnumExerciseAE)System.Enum.Parse(typeof(CciEnumExerciseAE), clientId_Key);

                        switch (keyExerciseAE)
                        {
                            #region expirationDate_adjustableDate_unadjustedDate
                            case CciEnumExerciseAE.expirationDate_adjustableDate_unadjustedDate:
                                if (IsExerciseA && AmericanExercise.ExpirationDate.AdjustableDateSpecified)
                                    data = AmericanExercise.ExpirationDate.AdjustableDate.UnadjustedDate.Value;
                                else if (IsExerciseE && EuropeanExercise.ExpirationDate.AdjustableDateSpecified)
                                    data = EuropeanExercise.ExpirationDate.AdjustableDate.UnadjustedDate.Value;
                                //else if (IsExerciseB && BermudaExercise...)
                                //    data = BermudaExercise...;
                                break;

                            #region expirationDate_adjustableDate_dateAdjustments_bDC
                            case CciEnumExerciseAE.expirationDate_adjustableDate_dateAdjustments_bDC:
                                if (IsExerciseA && AmericanExercise.ExpirationDate.AdjustableDateSpecified)
                                    data = AmericanExercise.ExpirationDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                else if (IsExerciseE && EuropeanExercise.ExpirationDate.AdjustableDateSpecified)
                                    data = EuropeanExercise.ExpirationDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                break;
                            #endregion


                            #endregion
                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion
                    //
                    #region All exercise types
                    if (!isSetting)
                    {
                        isSetting = true;

                        CciEnumExercise keyExercise = CciEnumExercise.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumExercise), clientId_Key))
                            keyExercise = (CciEnumExercise)System.Enum.Parse(typeof(CciEnumExercise), clientId_Key);

                        switch (keyExercise)
                        {
                            #region earliestExerciseTime
                            case CciEnumExercise.earliestExerciseTime_hourMinuteTime:
                                data = ExerciseBase.EarliestExerciseTime.HourMinuteTime.Value;
                                break;
                            case CciEnumExercise.earliestExerciseTime_businessCenter:
                                data = ExerciseBase.EarliestExerciseTime.BusinessCenter.Value;
                                break;
                            #endregion
                            #region expirationTime
                            case CciEnumExercise.expirationTime_hourMinuteTime:
                                data = ExerciseBase.ExpirationTime.HourMinuteTime.Value;
                                break;
                            case CciEnumExercise.expirationTime_businessCenter:
                                data = ExerciseBase.ExpirationTime.BusinessCenter.Value;
                                break;
                            #endregion
                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion

                    #region EuropeanExercise Only
                    if (!isSetting && IsExerciseE)
                    {
                        isSetting = true;

                        CciEnumEuropeanExercise keyExercise = CciEnumEuropeanExercise.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumEuropeanExercise), clientId_Key))
                            keyExercise = (CciEnumEuropeanExercise)System.Enum.Parse(typeof(CciEnumEuropeanExercise), clientId_Key);

                        switch (keyExercise)
                        {
                            case CciEnumEuropeanExercise.partial_specified:
                                #region PartialExerciseSpecified
                                data = EuropeanExercise.PartialExerciseSpecified.ToString().ToLower();
                                #endregion PartialExerciseSpecified
                                break;

                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion
                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);
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
            foreach (CustomCaptureInfo cci in _ccis)
            {
                //On ne traite que les contrôle dont le contenu à changé
                if (cci.HasChanged && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string clientId = cci.ClientId;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    string clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                    #endregion

                    #region Each exercise type
                    if (ExerciseType.HasValue)
                    {
                        switch (ExerciseType.Value)
                        {
                            #region American
                            case ExerciseTypeEnum.american:
                                CciEnumAmericanExercise eltAmerican = CciEnumAmericanExercise.unknown;
                                if (System.Enum.IsDefined(typeof(CciEnumAmericanExercise), clientId_Element))
                                    eltAmerican = (CciEnumAmericanExercise)System.Enum.Parse(typeof(CciEnumAmericanExercise), clientId_Element);

                                switch (eltAmerican)
                                {
                                    #region commencementDate_adjustableDate_unadjustedDate
                                    case CciEnumAmericanExercise.commencementDate_adjustableDate_unadjustedDate:
                                        AmericanExercise.CommencementDate.AdjustableDateSpecified = cci.IsFilledValue;
                                        AmericanExercise.CommencementDate.AdjustableDate.UnadjustedDate.Value = data;
                                        break;
                                    #endregion
                                    #region commencementDate_adjustableDate_dateAdjustments_bDC
                                    case CciEnumAmericanExercise.commencementDate_adjustableDate_dateAdjustments_bDC:
                                        DumpCommencementDateBDA();
                                        break;
                                    #endregion

                                    case CciEnumAmericanExercise.multiple_specified:
                                        AmericanExercise.MultipleExerciseSpecified = cci.IsFilledValue;
                                        break;

                                    #region default
                                    default:
                                        isSetting = false;
                                        //
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion
                            #region Bermuda
                            case ExerciseTypeEnum.bermuda:
                                CciEnumBermudaExercise eltBermuda = CciEnumBermudaExercise.unknown;
                                if (System.Enum.IsDefined(typeof(CciEnumBermudaExercise), clientId_Element))
                                    eltBermuda = (CciEnumBermudaExercise)System.Enum.Parse(typeof(CciEnumBermudaExercise), clientId_Element);

                                switch (eltBermuda)
                                {
                                    #region bermudaExerciseDates_adjustableDates_unadjustedDate
                                    case CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_unadjustedDate:
                                        BermudaExercise.BermudaExerciseDates.AdjustableDatesSpecified = cci.IsFilledValue;

                                        if (ArrFunc.IsEmpty(BermudaExercise.BermudaExerciseDates.AdjustableDates.UnadjustedDate))
                                            ReflectionTools.AddItemInArray(BermudaExercise.BermudaExerciseDates.AdjustableDates, "unadjustedDate", 0);

                                        BermudaExercise.BermudaExerciseDates.AdjustableDates.UnadjustedDate[0].Value = data;
                                        break;
                                    #endregion
                                    #region bermudaExerciseDates_adjustableDates_dateAdjustments_bDC
                                    case CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_dateAdjustments_bDC:
                                        DumpBermudaExerciceDateBDA();
                                        break;
                                    #endregion

                                    case CciEnumBermudaExercise.multiple_specified:
                                        BermudaExercise.MultipleExerciseSpecified = cci.IsFilledValue;
                                        break;

                                    #region default
                                    default:
                                        isSetting = false;
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion
                            #region European
                            case ExerciseTypeEnum.european:
                                CciEnumEuropeanExercise eltEuropean = CciEnumEuropeanExercise.unknown;
                                if (System.Enum.IsDefined(typeof(CciEnumEuropeanExercise), clientId_Element))
                                    eltEuropean = (CciEnumEuropeanExercise)System.Enum.Parse(typeof(CciEnumEuropeanExercise), clientId_Element);

                                switch (eltEuropean)
                                {
                                    case CciEnumEuropeanExercise.partial_specified:
                                        #region PartialExerciseSpecified
                                        EuropeanExercise.PartialExerciseSpecified = cci.IsFilledValue;
                                        //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                        #endregion PartialExerciseSpecified
                                        break;

                                    #region default
                                    default:
                                        isSetting = false;
                                        //
                                        break;
                                    #endregion
                                }
                                break;
                            #endregion

                            default:
                                throw new Exception("Exercise Type unknown");
                        }
                    }
                    #endregion

                    #region American and Bermudan exercise types
                    if (!isSetting && IsExerciseAB)
                    {
                        isSetting = true;

                        CciEnumExerciseAB eltExerciseAB = CciEnumExerciseAB.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumExerciseAB), clientId_Element))
                            eltExerciseAB = (CciEnumExerciseAB)System.Enum.Parse(typeof(CciEnumExerciseAB), clientId_Element);

                        switch (eltExerciseAB)
                        {
                            #region latestExerciseTime
                            case CciEnumExerciseAB.latestExerciseTime_hourMinuteTime:
                                if (IsExerciseA)
                                {
                                    AmericanExercise.LatestExerciseTime.HourMinuteTime.Value = data;
                                    AmericanExercise.LatestExerciseTimeSpecified = cci.IsFilledValue;
                                }
                                else if (IsExerciseB)
                                {
                                    BermudaExercise.LatestExerciseTime.HourMinuteTime.Value = data;
                                    BermudaExercise.LatestExerciseTimeSpecified = cci.IsFilledValue;
                                }
                                break;
                            case CciEnumExerciseAB.latestExerciseTime_businessCenter:
                                if (IsExerciseA)
                                    AmericanExercise.LatestExerciseTime.BusinessCenter.Value = data;
                                else if (IsExerciseB)
                                    BermudaExercise.LatestExerciseTime.BusinessCenter.Value = data;
                                break;
                            #endregion

                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion

                    #region American and European exercise types
                    if (!isSetting && IsExerciseAE)
                    {
                        isSetting = true;

                        CciEnumExerciseAE eltExerciseAE = CciEnumExerciseAE.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumExerciseAE), clientId_Element))
                            eltExerciseAE = (CciEnumExerciseAE)System.Enum.Parse(typeof(CciEnumExerciseAE), clientId_Element);
                         
                        switch (eltExerciseAE)
                        {
                            #region expirationDate_adjustableDate_unadjustedDate
                            case CciEnumExerciseAE.expirationDate_adjustableDate_unadjustedDate:
                                if (IsExerciseA)
                                {
                                    AmericanExercise.ExpirationDate.AdjustableDateSpecified = cci.IsFilledValue;
                                    AmericanExercise.ExpirationDate.AdjustableDate.UnadjustedDate.Value = data;
                                }
                                else if (IsExerciseE)
                                {
                                    EuropeanExercise.ExpirationDate.AdjustableDateSpecified = cci.IsFilledValue;
                                    EuropeanExercise.ExpirationDate.AdjustableDate.UnadjustedDate.Value = data;
                                }
                                break;
                            #endregion

                            #region expirationDate_adjustableDate_dateAdjustments_bDC
                            case CciEnumExerciseAE.expirationDate_adjustableDate_dateAdjustments_bDC:
                                DumpExpirationDateBDA();
                                break;
                            #endregion

                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion

                    #region All exercise types
                    if (!isSetting)
                    {
                        isSetting = true;

                        CciEnumExercise eltExercise = CciEnumExercise.unknown;
                        if (System.Enum.IsDefined(typeof(CciEnumExercise), clientId_Element))
                            eltExercise = (CciEnumExercise)System.Enum.Parse(typeof(CciEnumExercise), clientId_Element);

                        switch (eltExercise)
                        {
                            #region earliestExerciseTime
                            case CciEnumExercise.earliestExerciseTime_hourMinuteTime:
                                ExerciseBase.EarliestExerciseTime.HourMinuteTime.Value = data;

                                CustomCaptureInfo cciBC = _ccis[CciClientId(CciEnumExercise.earliestExerciseTime_businessCenter)];
                                ExerciseBase.EarliestExerciseTime.BusinessCenter.Value = cciBC.NewValue;
                                break;
                            case CciEnumExercise.earliestExerciseTime_businessCenter:
                                ExerciseBase.EarliestExerciseTime.BusinessCenter.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                break;
                            #endregion
                            #region expirationTime
                            case CciEnumExercise.expirationTime_hourMinuteTime:
                                ExerciseBase.ExpirationTime.HourMinuteTime.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                                break;
                            case CciEnumExercise.expirationTime_businessCenter:
                                ExerciseBase.ExpirationTime.BusinessCenter.Value = data;
                                break;
                            #endregion
                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                    }
                    #endregion

                    if (isSetting)
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            if (Cst.Capture.IsModeInput(Ccis.CaptureMode) && (false == Cst.Capture.IsModeAction(Ccis.CaptureMode)))
            {
                if (IsExerciseA)
                    DumpCommencementDateBDA();
                if (IsExerciseAE)
                    DumpExpirationDateBDA();
                if (IsExerciseB)
                    DumpBermudaExerciceDateBDA();

                DumpPartialOrMultipleExercise();
            }

        }
        #endregion Dump_ToDocument
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //FieldInfo fld = null;
                //              
                CciEnumExercise key = CciEnumExercise.unknown;
                if (System.Enum.IsDefined(typeof(CciEnumExercise), clientId_Element))
                    key = (CciEnumExercise)System.Enum.Parse(typeof(CciEnumExercise), clientId_Element);
                //                        
                switch (key)
                {
                    case CciEnumExercise.earliestExerciseTime_businessCenter:
                        //fld = Exercise.GetType().GetField("earliestExerciseTime");
                        //string bc = ((BusinessCenterTime)fld.GetValue(Exercise)).businessCenter.Value;
                        //
                        if (IsExerciseAB)
                            _ccis.SetNewValue(CciClientId(CciEnumExerciseAB.latestExerciseTime_businessCenter), pCci.NewValue);
                        //
                        _ccis.SetNewValue(CciClientId(CciEnumExercise.expirationTime_businessCenter), pCci.NewValue);
                        //
                        break;
                    case CciEnumExercise.expirationTime_hourMinuteTime:
                        if (IsExerciseAB)
                        {
                            //fld = Exercise.GetType().GetField("expirationTime");
                            //string hMT = ((BusinessCenterTime)fld.GetValue(Exercise)).hourMinuteTime.Value;
                            //
                            _ccis.SetNewValue(CciClientId(CciEnumExerciseAB.latestExerciseTime_hourMinuteTime), pCci.NewValue);
                        }
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
            //isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            //isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        #endregion
        #region CleanUp
        public void CleanUp()
        {
            switch (ExerciseType)
            {
                case ExerciseTypeEnum.american:
                    break;
                case ExerciseTypeEnum.bermuda:
                    break;
                case ExerciseTypeEnum.european:
                    break;
                default:
                    throw new Exception("Exercise Type unknown");
            }
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnumAmericanExercise.commencementDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (AmericanExercise.CommencementDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, AmericanExercise.CommencementDate.AdjustableDate.DateAdjustments);
            }
            else if (IsCci(CciEnumExerciseAE.expirationDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (this.IsExerciseE && EuropeanExercise.ExpirationDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, EuropeanExercise.ExpirationDate.AdjustableDate.DateAdjustments);
                if (this.IsExerciseA && AmericanExercise.ExpirationDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, AmericanExercise.ExpirationDate.AdjustableDate.DateAdjustments);
            }
            else if (IsCci(CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_dateAdjustments_bDC, pCci))
            {
                if (BermudaExercise.BermudaExerciseDates.AdjustableDatesSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, BermudaExercise.BermudaExerciseDates.AdjustableDates.DateAdjustments);
            }
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
        public bool IsCciClientId(CciEnumAmericanExercise pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        public bool IsCciClientId(CciEnumBermudaExercise pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        public bool IsCciClientId(CciEnumEuropeanExercise pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        public bool IsCciClientId(CciEnumExerciseAB pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        public bool IsCciClientId(CciEnumExercise pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        #region CciClientId
        public string CciClientId(CciEnumAmericanExercise pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(CciEnumBermudaExercise pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(CciEnumEuropeanExercise pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(CciEnumExerciseAB pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(CciEnumExerciseAE pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(CciEnumExercise pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnumAmericanExercise pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(CciEnumBermudaExercise pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(CciEnumEuropeanExercise pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(CciEnumExerciseAB pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(CciEnumExerciseAE pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(CciEnumExercise pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return _ccis[CciClientId(pClientId_Key)];
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
        public bool IsCci(CciEnumAmericanExercise pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumBermudaExercise pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumEuropeanExercise pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumExerciseAB pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumExerciseAE pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumExercise pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion ITradeCci
        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return Cci(CciEnumExercise.expirationTime_hourMinuteTime).IsFilled; } }
        #endregion IContainerCciSpecified

        #region public Clear
        public void Clear()
        {
            try
            {
                if (ExerciseType.HasValue)
                {
                    switch (ExerciseType.Value)
                    {
                        case ExerciseTypeEnum.american:
                            CciTools.SetCciContainer(this, "CciEnumAmericanExercise", "NewValue", string.Empty);
                            break;
                        case ExerciseTypeEnum.bermuda:
                            CciTools.SetCciContainer(this, "CciEnumBermudaExercise", "NewValue", string.Empty);
                            break;
                        case ExerciseTypeEnum.european:
                            CciTools.SetCciContainer(this, "CciEnumEuropeanExercise", "NewValue", string.Empty);
                            break;
                        default:
                            throw new Exception("Exercise Type unknown");
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion
        #region public SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
            if (ExerciseType.HasValue)
            {
                switch (ExerciseType.Value)
                {
                    case ExerciseTypeEnum.american:
                        CciTools.SetCciContainer(this, "CciEnumAmericanExercise", "IsEnabled", pIsEnabled);
                        break;
                    case ExerciseTypeEnum.bermuda:
                        CciTools.SetCciContainer(this, "CciEnumBermudaExercise", "IsEnabled", pIsEnabled);
                        break;
                    case ExerciseTypeEnum.european:
                        CciTools.SetCciContainer(this, "CciEnumEuropeanExercise", "IsEnabled", pIsEnabled);
                        break;
                    default:
                        throw new Exception("Exercise Type unknown");
                }
            }
        }
        #endregion
        #region public SetExercise
        // EG 20150412 [20513] BANCAPERTA
        public void SetExercise(IProduct pProduct)
        {
            if (pProduct.ProductBase.IsSwaption)
            {
                ISwaption swaption = pProduct as ISwaption;
                swaption.ExerciseAmericanSpecified = IsExerciseA;
                swaption.ExerciseBermudaSpecified = IsExerciseB;
                swaption.ExerciseEuropeanSpecified = IsExerciseE;
                exercise = swaption.EFS_Exercise;
            }
            else if (pProduct.ProductBase.IsBondOption)
            {
                IBondOption bondOption = pProduct as IBondOption;
                bondOption.AmericanExerciseSpecified = IsExerciseA;
                bondOption.BermudaExerciseSpecified = IsExerciseB;
                bondOption.EuropeanExerciseSpecified = IsExerciseE;
                exercise = bondOption.EFS_Exercise;
            }
        }
        #endregion
        #region Membres de IContainerCciGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;

            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                if (IsExerciseA)
                {
                    isOk = this.IsCci(CciEnumAmericanExercise.multiple, pCci);
                    if (isOk)
                    {
                        pCo.Object = "optionExerciseAmerican";
                        pCo.Element = "multipleExercise";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = AmericanExercise.MultipleExerciseSpecified;
                        pIsEnabled = true;
                    }
                }

                if ((false == isOk) & IsExerciseB)
                {
                    isOk = this.IsCci(CciEnumBermudaExercise.multiple, pCci);
                    if (isOk)
                    {
                        pCo.Object = "optionExerciseBermuda";
                        pCo.Element = "multipleExercise";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = BermudaExercise.MultipleExerciseSpecified;
                        pIsEnabled = true;
                    }
                }

                if ((false == isOk) & IsExerciseE)
                {
                    isOk = this.IsCci(CciEnumEuropeanExercise.partial, pCci);
                    if (isOk)
                    {
                        pCo.Element = "partialExercise";
                        pCo.OccurenceValue = 1;
                        pIsSpecified = EuropeanExercise.PartialExerciseSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            return isOk;
        }
        #endregion Membres de IContainerCciGetInfoButton

        #region private CreateInstance
        private void CreateInstance()
        {
            
            if (ExerciseType.HasValue)
            {
                switch (ExerciseType.Value)
                {
                    case ExerciseTypeEnum.american:
                        CciTools.CreateInstance(this, AmericanExercise, "CciEnumAmericanExercise");
                        if (null != Cci(CciEnumAmericanExercise.commencementDate_adjustableDate_unadjustedDate))
                        {
                            AmericanExercise.CommencementDate.AdjustableDateSpecified = true;
                            AmericanExercise.CommencementDate.RelativeDateSpecified = false;
                        }
                        break;
                    case ExerciseTypeEnum.bermuda:
                        CciTools.CreateInstance(this, BermudaExercise, "CciEnumBermudaExercise");
                        if (null != Cci(CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_unadjustedDate))
                        {
                            BermudaExercise.BermudaExerciseDates.AdjustableDatesSpecified = true;
                            BermudaExercise.BermudaExerciseDates.RelativeDatesSpecified = false;
                        }
                        break;
                    case ExerciseTypeEnum.european:
                        CciTools.CreateInstance(this, EuropeanExercise, "CciEnumEuropeanExercise");
                        break;
                    default:
                        throw new Exception("Exercise Type unknown");
                }

                if (IsExerciseAE)
                {
                    if (null != Cci(CciEnumExerciseAE.expirationDate_adjustableDate_unadjustedDate))
                    {
                        if (IsExerciseA)
                        {
                            AmericanExercise.ExpirationDate.AdjustableDateSpecified = true;
                            AmericanExercise.ExpirationDate.RelativeDateSpecified = false;
                        }
                        else if (IsExerciseE)
                        {
                            EuropeanExercise.ExpirationDate.AdjustableDateSpecified = true;
                            EuropeanExercise.ExpirationDate.RelativeDateSpecified = false;
                        }
                    }
                }

            }

        }
        #endregion

        #region private DumpExpirationDateBDA
        private void DumpExpirationDateBDA()
        {
            string clientId = CciClientId(CciEnumExerciseAE.expirationDate_adjustableDate_dateAdjustments_bDC);
            CciBC cciBC = new CciBC(trade)
            {
                { trade.CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { trade.CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { trade.CciClientIdMainCurrency, CciBC.TypeReferentialInfo.Currency }
            };

            string id = trade.DataDocument.GenerateId(this.prefix + TradeCustomCaptureInfos.CCst.EXPIRATION_BUSINESS_CENTERS_REFERENCE + "1", false);
            if (IsExerciseE)
                Ccis.DumpBDC_ToDocument(EuropeanExercise.ExpirationDate.AdjustableDate.DateAdjustments, clientId, id, cciBC);
            else if (IsExerciseA)
                Ccis.DumpBDC_ToDocument(AmericanExercise.ExpirationDate.AdjustableDate.DateAdjustments, clientId, id, cciBC);

        }
        #endregion private DumpCalculationPeriodDatesEffectiveDateBDA
        #region private DumpExerciceDateBDA
        private void DumpBermudaExerciceDateBDA()
        {

            string clientId = CciClientId(CciEnumBermudaExercise.bermudaExerciseDates_adjustableDates_dateAdjustments_bDC);
            CciBC cciBC = new CciBC(trade)
            {
                { trade.CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { trade.CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { trade.CciClientIdMainCurrency, CciBC.TypeReferentialInfo.Currency }
            };

            string id = trade.DataDocument.GenerateId(this.prefix + TradeCustomCaptureInfos.CCst.EXERCICE_BUSINESS_CENTERS_REFERENCE + "1", false);
            if (IsExerciseB)
                Ccis.DumpBDC_ToDocument(BermudaExercise.BermudaExerciseDates.AdjustableDates.DateAdjustments, clientId, id, cciBC);
        }
        #endregion
        #region private DumpCommencementDateBDA
        private void DumpCommencementDateBDA()
        {
            string clientId = CciClientId(CciEnumAmericanExercise.commencementDate_adjustableDate_dateAdjustments_bDC);
            CciBC cciBC = new CciBC(trade)
            {
                { trade.CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { trade.CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { trade.CciClientIdMainCurrency, CciBC.TypeReferentialInfo.Currency }
            };

            string id = trade.DataDocument.GenerateId(this.prefix + TradeCustomCaptureInfos.CCst.COMMENCEMENT_BUSINESS_CENTERS_REFERENCE + "1", false);
            if (IsExerciseA)
                Ccis.DumpBDC_ToDocument(AmericanExercise.CommencementDate.AdjustableDate.DateAdjustments, clientId, id, cciBC);

        }
        #endregion private DumpCalculationPeriodDatesEffectiveDateBDA

        #region DumpPartialOrMultipleExercise
        private void DumpPartialOrMultipleExercise()
        {
            IPartialExercise partialExercise = null;
            if (IsExerciseE && EuropeanExercise.PartialExerciseSpecified)
                partialExercise = EuropeanExercise.PartialExercise;
            else if (IsExerciseA && AmericanExercise.MultipleExerciseSpecified)
                partialExercise = AmericanExercise.MultipleExercise;
            else if (IsExerciseB && BermudaExercise.MultipleExerciseSpecified)
                partialExercise = BermudaExercise.MultipleExercise;

            if (null != partialExercise)
            {
                if (false == partialExercise.MinimumNotionalAmountSpecified &&
                    false == partialExercise.MinimumNumberOfOptionsSpecified)
                {
                    partialExercise.MinimumNumberOfOptionsSpecified = true;
                    partialExercise.MinimumNumberOfOptions = new EFS_PosInteger(1);
                }
            }
        }
        #endregion DumpPartialOrMultipleExercise
    }
    
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    public class CciEquityExerciceValuationSettlement : IContainerCci, IContainerCciFactory
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            #region automaticExercise
            [System.Xml.Serialization.XmlEnumAttribute("automaticExercise")]
            automaticExercise,
            #endregion automaticExercise
            #region settlementDate
            [System.Xml.Serialization.XmlEnumAttribute("settlementDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            settlementDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("settlementDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            settlementDate_adjustableDate_dateAdjustments_bDC,
            #endregion settlementDate
            #region settlementCurrency
            [System.Xml.Serialization.XmlEnumAttribute("settlementCurrency")]
            settlementCurrency,
            #endregion settlementCurrency
            #region settlementPriceSource
            [System.Xml.Serialization.XmlEnumAttribute("settlementPriceSource")]
            settlementPriceSource,
            #endregion settlementPriceSource
            #region settlementType
            [System.Xml.Serialization.XmlEnumAttribute("settlementType")]
            settlementType,
            #endregion settlementType
            #region settlementMethodElectionDate
            [System.Xml.Serialization.XmlEnumAttribute("settlementMethodElectionDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            settlementMethodElectionDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("settlementMethodElectionDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            settlementMethodElectionDate_dateAdjustments_bDC,
            #endregion settlementMethodElectionDate
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly IEquityExerciseValuationSettlement equityExerciceValuationSettlement;
        private readonly CciTradeBase cciTrade;
        private CciEquityExercise cciEquityExercise;
        private readonly string prefix;
        #endregion Members
        #region Accessors
        #region ccis
        public TradeCustomCaptureInfos Ccis => cciTrade.Ccis;
        #endregion ccis
        #endregion Accessors
        #region Constructors
        public CciEquityExerciceValuationSettlement(CciTradeBase pCciTrade, IEquityExerciseValuationSettlement pEquityExerciceValuationSettlement, string pPrefix)
        {
            cciTrade = pCciTrade;
            equityExerciceValuationSettlement = pEquityExerciceValuationSettlement;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
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
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #endregion IContainerCci Members
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.settlementCurrency), true, TypeData.TypeDataEnum.@string);

            cciEquityExercise.AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
            cciEquityExercise.CleanUp();
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
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
                            case CciEnum.automaticExercise:
                                #region automaticExercise
                                equityExerciceValuationSettlement.AutomaticExerciseSpecified = true;
                                equityExerciceValuationSettlement.AutomaticExercise = BoolFunc.IsTrue(data);
                                #endregion automaticExercise
                                break;
                            case CciEnum.settlementDate_adjustableDate_unadjustedDate:
                                #region settlementDate
                                equityExerciceValuationSettlement.SettlementDateSpecified = StrFunc.IsFilled(data);
                                equityExerciceValuationSettlement.SettlementDate.AdjustableDateSpecified = StrFunc.IsFilled(data);
                                if (equityExerciceValuationSettlement.SettlementDate.AdjustableDateSpecified)
                                    equityExerciceValuationSettlement.SettlementDate.AdjustableDate.UnadjustedDate.Value = data;
                                #endregion settlementDate
                                break;
                            case CciEnum.settlementDate_adjustableDate_dateAdjustments_bDC:
                                #region settlementDate BDC
                                equityExerciceValuationSettlement.SettlementDate.AdjustableDate.DateAdjustments.BusinessDayConvention = StringToEnum.BusinessDayConvention(data);
                                #endregion settlementDate BDC
                                break;
                            case CciEnum.settlementCurrency:
                                #region settlementCurrency
                                equityExerciceValuationSettlement.SettlementCurrency.Value = data;
                                #endregion settlementCurrency
                                break;
                            case CciEnum.settlementType:
                                #region SettlementType
                                equityExerciceValuationSettlement.SettlementType = (SettlementTypeEnum)Enum.Parse(typeof(SettlementTypeEnum), data);
                                #endregion SettlementType
                                break;
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
                cciEquityExercise.Dump_ToDocument();
                equityExerciceValuationSettlement.EquityExerciseEuropeanSpecified = (CciEquityExercise.ExerciseTypeEnum.european == cciEquityExercise.ExerciseType);
                equityExerciceValuationSettlement.EquityExerciseAmericanSpecified = (CciEquityExercise.ExerciseTypeEnum.american == cciEquityExercise.ExerciseType);
                equityExerciceValuationSettlement.EquityExerciseBermudaSpecified = (CciEquityExercise.ExerciseTypeEnum.bermuda == cciEquityExercise.ExerciseType);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public virtual void Initialize_Document()
        {
            cciEquityExercise.Initialize_Document();
        }
        #endregion Initialize_Document        
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, equityExerciceValuationSettlement);
            InitializeExercise_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
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
                            case CciEnum.automaticExercise:
                                #region automaticExercise
                                if (equityExerciceValuationSettlement.AutomaticExerciseSpecified)
                                    data = BoolFunc.IsTrue(equityExerciceValuationSettlement.AutomaticExercise).ToString();
                                #endregion automaticExercise
                                break;
                            case CciEnum.settlementDate_adjustableDate_unadjustedDate:
                                #region settlementDate
                                if (equityExerciceValuationSettlement.SettlementDateSpecified)
                                {
                                    if (equityExerciceValuationSettlement.SettlementDate.AdjustableDateSpecified)
                                        data = equityExerciceValuationSettlement.SettlementDate.AdjustableDate.UnadjustedDate.Value;
                                }
                                #endregion settlementDate
                                break;
                            case CciEnum.settlementDate_adjustableDate_dateAdjustments_bDC:
                                #region settlementDate BDC
                                if (equityExerciceValuationSettlement.SettlementDateSpecified)
                                {
                                    if (equityExerciceValuationSettlement.SettlementDate.AdjustableDateSpecified)
                                        data = equityExerciceValuationSettlement.SettlementDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                }
                                #endregion settlementDate BDC
                                break;
                            case CciEnum.settlementCurrency:
                                #region settlementCurrency
                                data = equityExerciceValuationSettlement.SettlementCurrency.Value;
                                #endregion settlementCurrency
                                break;
                            case CciEnum.settlementType:
                                #region SettlementType
                                data = equityExerciceValuationSettlement.SettlementType.ToString();
                                #endregion SettlementType
                                break;
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
                //
                cciEquityExercise.Initialize_FromDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return cciEquityExercise.IsClientId_PayerOrReceiver(pCci);
        }
        #endregion IsClientId_PayerOrReceiver
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
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            try
            {
                if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //		
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        default:
                            #region Default
                            //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                            #endregion Default
                            break;
                    }
                }
                cciEquityExercise.ProcessInitialize(pCci);
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public virtual void RefreshCciEnabled()
        {
            cciEquityExercise.RefreshCciEnabled(); 
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public virtual void RemoveLastItemInArray(string pPrefix)
        {
            cciEquityExercise.RemoveLastItemInArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            cciEquityExercise.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #endregion Interfaces
        #region Methods
        #region InitializeExercise_FromCci
        private void InitializeExercise_FromCci()
        {
            //
            try
            {
                foreach (CciEquityExercise.ExerciseTypeEnum exerciseType in Enum.GetValues(typeof(CciEquityExercise.ExerciseTypeEnum)))
                {
                    cciEquityExercise = new CciEquityExercise(cciTrade, prefix + "equity" + StrFunc.FirstUpperCase(exerciseType.ToString()) + "Exercise");
                    if (Ccis.Contains(cciEquityExercise.CciClientId(CciEquityExercise.CciEnumEquitySharedExercice.equityExpirationTime_hourMinuteTime)))
                    {
                        cciEquityExercise.SetExercise(exerciseType, equityExerciceValuationSettlement);
                        break;
                    }
                }
                cciEquityExercise.Initialize_FromCci();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion InitializeExercise_FromCci
        #endregion Methods
    }
}

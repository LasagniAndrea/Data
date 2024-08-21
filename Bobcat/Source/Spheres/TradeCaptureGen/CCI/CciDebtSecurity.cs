#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public class CciDebtSecurity : ContainerCciBase, IContainerCciFactory, IContainerCciGetInfoButton
    {
        #region Enums
        #region CciEnum
        // EG 20190823 [FIXEDINCOME] Add type|previousCouponDate
        public enum CciEnum
        {
            type,
            previousCouponDate,
            calculationPeriodDates_effectiveMinDate,
            calculationPeriodDates_terminationMaxDate,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums

        #region Members
        
        private readonly CciTradeBase cciTrade;
        public IDebtSecurity debtSecurity;
        
        public CciStream cciStreamGlobal; //Represente les ccis dits globaux
        public CciStream[] cciStream;
        //
        public CciSecurity cciSecurity;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public int DebtSecurityStreamLenght
        {
            get { return ArrFunc.IsFilled(cciStream) ? cciStream.Length : 0; }
        }
        #region IsPerpetual
        /// <summary>
        /// DebtSecurity est perpétuel
        /// </summary>
        // EG 20190823 [FIXEDINCOME] New
        public bool IsPerpetual
        {
            get { return (CcisBase.GetNewValue(CciClientId(CciEnum.type)) == DebtSecurityTypeEnum.Perpetual.ToString()); }
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }


        #endregion Accessors

        #region Constructors
        public CciDebtSecurity(CciTradeBase pTrade, IDebtSecurity pDebtSecurity) : 
            this(pTrade, pDebtSecurity, string.Empty) { }
        public CciDebtSecurity(CciTradeBase pTrade, IDebtSecurity pDebtSecurity, string pPrefix) : base(pPrefix, pTrade.Ccis)
        {
            cciTrade = pTrade;
            debtSecurity = pDebtSecurity;
            
            
            cciStreamGlobal = new CciStream(pTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_debtSecurityStream, -1, debtSecurity.Stream[0])
            {
                //20090922 FI L'acheteur d'un titre peut être l'emetteur (dans cs payer=receiver) => isSwapReceiverAndPayer = false
                IsSwapReceiverAndPayer = false
            };
            
            
            cciSecurity = new CciSecurity(pTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_security, debtSecurity.Security);
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdPayer
        {
            get
            {
                return cciStreamGlobal.CciClientIdPayer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdReceiver
        {
            get
            {
                return cciStreamGlobal.CciClientIdReceiver;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);


        }
        #endregion

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.BUT + cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);


            List<String> lst = new List<string>(2)
            {
                CciClientIdPayer,
                CciClientIdReceiver
            };

            lst.ForEach(item=>{
                CciTools.AddCciSystem(CcisBase, Cst.DDL + item, true, TypeData.TypeDataEnum.@string);
            });
            
            CcisBase[CciClientIdReceiver].IsMandatory = CcisBase[CciClientIdPayer].IsMandatory;

            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].AddCciSystem();

            cciSecurity.AddCciSystem();

        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {

            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].CleanUp();
            
            cciSecurity.CleanUp();
            
            // Suppression des streams issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(debtSecurity.Stream))
            {
                for (int i = debtSecurity.Stream.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(debtSecurity.Stream[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(debtSecurity, "debtSecurityStream", i);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        // EG 20190823 [FIXEDINCOME] Add type|previousCouponDate
        public void Dump_ToDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset Variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset Variables

                    switch (cciEnum)
                    {
                        case CciEnum.type:
                            #region Type
                            debtSecurity.DebtSecurityType = ReflectionTools.ConvertStringToEnum<DebtSecurityTypeEnum>(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Type
                            break;
                        case CciEnum.previousCouponDate:
                            #region Type
                            debtSecurity.PrevCouponDateSpecified = StrFunc.IsFilled(data);
                            if (debtSecurity.PrevCouponDateSpecified)
                                debtSecurity.PrevCouponDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Type
                            break;

                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            #region Min EffectiveDate
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Min EffectiveDate
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            #region Max TerminationDate
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Max TerminationDate
                            break;
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

            #region Synchronize cciStreamGlobal from cciStream[0]
            // Permet de conserver en phase le stream global et le stream[0] pour les ccis existants dans les 2 objects
            // Ex il existe debtSecurity_payer et debtSecurity1.payer
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && (DebtSecurityStreamLenght > 0) && cciStream[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = cciStream[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if (CcisBase.Contains(cciStreamGlobal.CciClientId(cciEnum)))
                            cciStreamGlobal.Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion
            #region synchronize cciStream[i] from  tradeStreamGlobal (exclude Payer/receiver)
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        //
                        for (int i = 0; i < DebtSecurityStreamLenght; i++)
                            cciStream[i].Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion
            
            cciStreamGlobal.Dump_ToDocument();
            
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].Dump_ToDocument();
            
            cciSecurity.Dump_ToDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <returns></returns>
        public int GetArrayElementDocumentCount(string pPrefix)
        {
            int ret = -1;
            if (-1 == ret)
            {
                if (TradeCustomCaptureInfos.CCst.Prefix_debtSecurityStream == pPrefix)
                    ret = ArrFunc.Count(debtSecurity.Stream);
            }
            //
            if (-1 == ret)
            {
                if (TradeCustomCaptureInfos.CCst.Prefix_instrumentId == pPrefix)
                    ret = ArrFunc.Count(debtSecurity.Security.InstrumentId);
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            //20090507 FI [avant livraison des titres]
            //mise en commentaire 
            //car ce code ajoute un party PartyUnknown sur les debtsecurityTransaction
            //On effet les cci sont présents (lorsque le pavé détail est affiché) et l'asset n'est pas saisi
            //
            //if (Cst.Capture.IsModeNew(ccis.CaptureMode) && (false == ccis.IsPreserveData))
            //{
            //    string id = string.Empty;
            //    //
            //    for (int i = 0; i < DebtSecurityStreamLenght ; i++)
            //    {
            //        CustomCaptureInfo cciPayer = cciStream[i].Cci(CciStream.CciEnum.payer);
            //        CustomCaptureInfo cciReceiver = cciStream[i].Cci(CciStream.CciEnum.receiver);
            //        //
            //        if (cciPayer.IsMandatory && cciReceiver.IsMandatory)
            //        {
            //            if (StrFunc.IsEmpty(debtSecurity.stream[i].payerPartyReference.hRef) &&
            //                StrFunc.IsEmpty(debtSecurity.stream[i].receiverPartyReference.hRef))
            //            {
            //                if (1 == i)
            //                {
            //                    debtSecurity.stream[i].payerPartyReference.hRef = debtSecurity.stream[i - 1].payerPartyReference.hRef;
            //                    debtSecurity.stream[i].receiverPartyReference.hRef = debtSecurity.stream[i - 1].receiverPartyReference.hRef;
            //                }
            //                else
            //                {
            //                    if (StrFunc.IsEmpty(id))
            //                        id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
            //                    debtSecurity.stream[i].payerPartyReference.hRef = id;
            //                }
            //            }
            //        }
            //    }
            //    //
            //    if (TradeCustomCaptureInfos.PartyUnknown == id)
            //        cciTrade.AddPartyUnknown();
            //}
            //
            cciSecurity.Initialize_Document();

        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            InitializeDebtSecurityStream_FromCci();
            InitializeSecurity_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        // EG 20190823 [FIXEDINCOME] Add type|previousCouponDate
        public void Initialize_FromDocument()
        {
            
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    
                    switch (cciEnum)
                    {
                        #region DebtSecurity_Type
                        case CciEnum.type:
                            data = debtSecurity.DebtSecurityType.ToString();
                            break;
                        #endregion DebtSecurity_Type
                        #region previousCouponDate
                        case CciEnum.previousCouponDate:
                            data = debtSecurity.PrevCouponDate.Value;
                            break;
                        #endregion previousCouponDate
                        #region calculationPeriodDates_effectiveMinDate & calculationPeriodDates_terminationMaxDate
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            data = debtSecurity.Stream[0].CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value;
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            for (int i = debtSecurity.Stream.Length - 1; i > -1; i--)
                            {
                                //On ne récupère pas la termination date de la dernière jambe
                                //En création lorsqu'il y a ajout d'un stream, la date terminationMaxDate est initialisée à blanc (On pert la saisie déjà effectuée)
                                if (DtFunc.IsDateTimeFilled(debtSecurity.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue))
                                {
                                    data = debtSecurity.Stream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
                                    break;
                                }
                            }
                            break;
                        #endregion calculationPeriodDates_effectiveMinDate & calculationPeriodDates_terminationMaxDate
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            if (debtSecurity.Security.PriceRateTypeSpecified)
                SetCalcRateAnd1stPayDt_EnabledAndMandatory(debtSecurity.Security.PriceRateType);
            //
            cciStreamGlobal.Initialize_FromDocument();
            //
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].Initialize_FromDocument();
            //
            cciSecurity.Initialize_FromDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = cciStreamGlobal.IsClientId_PayerOrReceiver(pCci);
            if (false == isOk)
            {
                for (int i = 0; i < DebtSecurityStreamLenght; i++)
                {
                    isOk = isOk || cciStream[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            //
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 20161214 [21916] Modify
        // EG 20190823 [FIXEDINCOME] Add type
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = string.Empty;
                bool isOk = true;
                //
                if (isOk)
                {
                    #region CciDebtSecurity
                    clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //
                    CciEnum elt = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //
                    switch (elt)
                    {
                        case CciEnum.type:
                            CcisBase.SetNewValue(cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), IsPerpetual ? DtFunc.DateTimeToStringDateISO(DateTime.MaxValue) : null, false);
                            break;
                        case CciEnum.calculationPeriodDates_effectiveMinDate:
                            #region calculationPeriodDates_effectiveMinDate
                            // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                            //ccis.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                            //ccis.SetNewValue(cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                            CcisBase.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                            CcisBase.SetNewValue(cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);

                            #endregion
                            break;
                        case CciEnum.calculationPeriodDates_terminationMaxDate:
                            #region calculationPeriodDates_terminationMaxDate
                            Math.DivRem(DebtSecurityStreamLenght, 2, out int k);
                            if (0 == k)
                            {
                                // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                                //ccis.SetNewValue(cciStream[DebtSecurityStreamLenght - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                                CcisBase.SetNewValue(cciStream[DebtSecurityStreamLenght - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                            }
                            // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false    
                            //ccis.SetNewValue(cciStream[DebtSecurityStreamLenght - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                            CcisBase.SetNewValue(cciStream[DebtSecurityStreamLenght - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                            break;
                        case CciEnum.unknown:
                            isOk = false;
                            break;
                            #endregion
                    }
                    #endregion
                }
                //
                if (!isOk)
                {
                    #region CciUnderlyingAsset
                    isOk = cciSecurity.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    if (isOk)
                    {
                        clientId_Element = cciSecurity.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        //
                        CciUnderlyingAsset.CciEnum eltUnderlyingAsset = CciUnderlyingAsset.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciUnderlyingAsset.CciEnum), clientId_Element))
                            eltUnderlyingAsset = (CciUnderlyingAsset.CciEnum)System.Enum.Parse(typeof(CciUnderlyingAsset.CciEnum), clientId_Element);
                        //
                        switch (eltUnderlyingAsset)
                        {
                            case CciUnderlyingAsset.CciEnum.currency:
                                for (int i = 0; i < DebtSecurityStreamLenght; i++)
                                    CcisBase.SetNewValue(cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), pCci.NewValue, false);
                                break;
                            case CciUnderlyingAsset.CciEnum.unknown:
                                isOk = false;
                                break;
                        }
                    }
                    #endregion
                }

                CustomCaptureInfo cciFaceAmount = cciSecurity.Cci(CciSecurity.CciEnum.faceAmount_amount);
                CustomCaptureInfo cciInitialAmount = cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue);
                CustomCaptureInfo cciInitialAmountCurrency = cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency);
                CustomCaptureInfo cciQte = cciSecurity.Cci(CciSecurity.CciEnum.numberOfIssuedSecurities);
                bool isOkToCalc = (null != cciQte && null != cciInitialAmount && null != cciFaceAmount);


                if (!isOk)
                {
                    #region CciSecurity
                    isOk = cciSecurity.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    //
                    if (isOk)
                    {

                        CciSecurity.CciEnum eltSecurity = CciSecurity.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciSecurity.CciEnum), clientId_Element))
                            eltSecurity = (CciSecurity.CciEnum)System.Enum.Parse(typeof(CciSecurity.CciEnum), clientId_Element);
                        
                        switch (eltSecurity)
                        {
                            case CciSecurity.CciEnum.priceRateType:
                                #region priceRateType
                                //
                                // Dans le cas d'un titre à taux précompté, il n'y a pas de taux à saisir ainsi que de date 1er coupon
                                //
                                // Pour celà:
                                // 1- les zones: Coupon, Date 1er Coupon sont vidées, rendues Disabled et Non Mandatory
                                // 2- la zone PeriodeFrequency est initialisée à "ZeroCoupon"
                                // 3- si la zone CouponType est vide, elle sera initialisée à "Fixed"
                                //
                                if (StrFunc.IsFilled(pCci.NewValue))
                                {
                                    if (pCci.NewValue == PriceRateType3CodeEnum.DISC.ToString())
                                    {
                                        CcisBase.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate), string.Empty, false);
                                        CcisBase.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), string.Empty, false);
                                        CcisBase.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), "0", false);
                                        CcisBase.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_period), PeriodEnum.T.ToString(), false);
                                        CcisBase.SetNewValue(cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency), "0" + PeriodEnum.T.ToString(), false);
                                        //
                                        CcisBase.SetNewValue(cciStreamGlobal.CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), string.Empty, false);
                                        //
                                        CustomCaptureInfo cciCouponType = cciSecurity.Cci(CciSecurity.CciEnum.couponType);
                                        if (StrFunc.IsEmpty(cciCouponType.NewValue))
                                            cciCouponType.NewValue = CouponTypeEnum.Fixed.ToString();
                                    }
                                    //
                                    SetCalcRateAnd1stPayDt_EnabledAndMandatory(pCci.NewValue);
                                }
                                #endregion
                                break;
                            case CciSecurity.CciEnum.numberOfIssuedSecurities:
                                #region numberOfIssuedSecurities
                                if (isOkToCalc)
                                {
                                    if (StrFunc.IsFilled(cciQte.NewValue))
                                    {
                                        if (cciInitialAmount.IsEmptyValue && cciFaceAmount.IsFilledValue)
                                            SetNominal();
                                        else if (cciInitialAmount.IsFilledValue)
                                            SetFaceAmount();
                                    }
                                }
                                #endregion
                                break;
                            case CciSecurity.CciEnum.faceAmount_amount:
                                #region faceAmount_amount
                                //
                                if (isOkToCalc)
                                {
                                    if (StrFunc.IsFilled(cciFaceAmount.NewValue))
                                    {
                                        if (cciInitialAmount.IsEmptyValue && cciQte.IsFilledValue)
                                            SetNominal();
                                        //                                            
                                        else if (cciInitialAmount.IsFilledValue)
                                            SetNumberOfIssued();
                                    }
                                    else if (cciQte.IsFilledValue && cciInitialAmount.IsFilledValue)
                                        SetFaceAmount();
                                }
                                #endregion
                                break;
                            case CciSecurity.CciEnum.faceAmount_currency:
                                cciInitialAmountCurrency.NewValue = pCci.NewValue;
                                break;

                            case CciSecurity.CciEnum.unknown:
                                isOk = false;
                                break;
                        }
                    }
                    #endregion
                }
                //
                if (!isOk)
                {
                    #region cciStreamGlobal
                    isOk = cciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    if (isOk)
                    {
                        clientId_Element = cciStreamGlobal.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        //
                        CciStream.CciEnum eltStream = CciStream.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Element))
                            eltStream = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Element);
                        //
                        switch (eltStream)
                        {
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency:
                                CcisBase.SetNewValue(cciSecurity.CciClientId(CciSecurity.CciEnum.faceAmount_currency), pCci.NewValue, false);
                                break;
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                                //
                                if (isOkToCalc)
                                {
                                    if (StrFunc.IsFilled(cciInitialAmount.NewValue))
                                    {
                                        if (cciQte.IsEmptyValue && cciFaceAmount.IsFilledValue)
                                            SetNumberOfIssued();
                                        else if (cciQte.IsFilledValue)
                                            SetFaceAmount();
                                    }
                                }
                                break;
                            case CciStream.CciEnum.unknown:
                                isOk = false;
                                break;
                        }
                    }
                    #endregion
                }
                //
                if (!isOk && this.DebtSecurityStreamLenght>0)
                {
                    #region cciStream[0]
                    isOk = cciStream[0].IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    if (isOk)
                    {
                        clientId_Element = cciStream[0].CciContainerKey(pCci.ClientId_WithoutPrefix);
                        //
                        CciStream.CciEnum eltStream = CciStream.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Element))
                            eltStream = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Element);
                        //
                        switch (eltStream)
                        {
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_rate:
                                if (cciStream[0].IsFixedRateSpecified)
                                {
                                    CcisBase.SetNewValue(cciSecurity.CciClientId(CciSecurity.CciEnum.couponType), CouponTypeEnum.Fixed.ToString(), false);
                                }
                                else if (cciStream[0].IsFloatingRateSpecified)
                                {
                                    CcisBase.SetNewValue(cciSecurity.CciClientId(CciSecurity.CciEnum.couponType), CouponTypeEnum.Float.ToString(), false);
                                    CcisBase.SetNewValue(cciSecurity.CciClientId(CciSecurity.CciEnum.priceRateType), PriceRateType3CodeEnum.YIEL.ToString(), false);
                                }
                                
                                /* FI 20191209 [XXXXX] Mise en commentaire.
                                   Il n'y a pas de sens à mettre un tx identique. Les tx sur les stubs doivent être différents sinon il n'est pas nécessaire de les renseigner 
                                
                                CciStub cciStub = new CciStub(cciTrade, null, cciStreamGlobal.prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_initialStub, cciStreamGlobal.irs);
                                if (RateTools.IsFloatingRate(pCci.NewValue))
                                    ccis.SetNewValue(cciStub.CciClientId(CciStub.CciEnum.floatingRate1), pCci.NewValue, false);
                                else
                                    ccis.SetNewValue(cciStub.CciClientId(CciStub.CciEnum.rate), pCci.NewValue, false);
                                */

                                break;
                            case CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_period:
                            case CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier:
                                bool isZeroCoupon = Tools.IsPeriodZeroCoupon(cciStream[0].Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval);
                                if (isZeroCoupon)
                                    CcisBase.SetNewValue(cciSecurity.CciClientId(CciSecurity.CciEnum.priceRateType), PriceRateType3CodeEnum.DISC.ToString(), false);
                                break;
                            case CciStream.CciEnum.unknown:
                                break;
                        }
                    }
                    #endregion
                }
            }
            
            cciStreamGlobal.ProcessInitialize(pCci);
            
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].ProcessInitialize(pCci);
            
            SynchronizeBDAFixedRate(pCci);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            cciSecurity.RefreshCciEnabled();

            cciStreamGlobal.RefreshCciEnabled();

            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].RefreshCciEnabled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string pPrefix)
        {
            cciSecurity.RemoveLastItemInArray(pPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            cciStreamGlobal.SetDisplay(pCci);
            
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].SetDisplay(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            
            cciStreamGlobal.SetEnabled(pIsEnabled);
            
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].SetEnabled(pIsEnabled);
            
            cciSecurity.SetEnabled(pIsEnabled);
        }

        #endregion IContainerCciFactory Members

        

        #region ICciGetInfoButton Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;

        }

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

            #region Button on StreamGlobal
            if (cciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                if (isOk)
                {
                    pCo.Object = "calculationNotional";
                    pCo.Element = "notionalStepSchedule";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = cciStreamGlobal.IsNotionalStepScheduleStepSpecified;
                    pIsEnabled = true;
                }
                //
                if (false == isOk)
                {
                    isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = cciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "";
                        pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = cciStreamGlobal.IsKnownAmountScheduleStepSpecified;
                        pIsEnabled = true;
                    }
                }
                //
                if (false == isOk)
                {
                    isOk = cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = cciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            #endregion Button on StreamGlobal

            #region  CciStream[i]
            if (false == isOk)
            {
                for (int i = 0; i < this.DebtSecurityStreamLenght; i++)
                {
                    if (cciStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string key = cciStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
                        CciStream.CciEnum elt = CciStream.CciEnum.unknown;
                        //
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), key))
                            elt = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), key);
                        //
                        isOk = true;
                        switch (elt)
                        {
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsNotionalStepScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepParameters";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsNotionalStepParametersSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsKnownAmountScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "capRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsCapRateScheduleSpecified;
                                pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floorRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsFloorRateScheduleSpecified;
                                pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "spreadSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsSpreadScheduleSpecified;
                                pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floatingRateMultiplierSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsFloatingRateMultiplierScheduleSpecified;
                                pIsEnabled = cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "rateFixedRate";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsFixedRateScheduleSpecified;
                                pIsEnabled = cciStream[i].IsFixedRateSpecified;
                                break;

                            case CciStream.CciEnum.paymentDates_offset:
                                pCo.Object = "paymentDates";
                                pCo.Element = "paymentDaysOffset";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = cciStream[i].IsOffsetSpecified;
                                pIsEnabled = true;
                                break;

                            default:
                                isOk = false;
                                break;
                        }
                    }
                    if (isOk)
                        break;
                }
            }
            #endregion  CciStream[i]
            //
            #region buttons of Security
            if (!isOk)
                isOk = cciSecurity.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion
            //
            return isOk;


        }
        #endregion ICciGetInfoButton Members

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        // EG 20190823 [FIXEDINCOME] Hide|Display MaturityDate (Perpetual|Ordinary)
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            System.Web.UI.Control control = pPage.PlaceHolder.FindControl(Cst.TXT + cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate));
            if (null != control)
                control.Visible = (false == IsPerpetual);
            control = pPage.PlaceHolder.FindControl(Cst.LBL + cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate));
            if (null != control)
                control.Visible = (false == IsPerpetual);


            for (int i = 0; i < ArrFunc.Count(cciStream); i++)
            {
                cciStream[i].DumpSpecific_ToGUI(pPage);
            }
        }
        #endregion

        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPriceRateType"></param>
        private void SetCalcRateAnd1stPayDt_EnabledAndMandatory(PriceRateType3CodeEnum pPriceRateType)
        {
            SetCalcRateAnd1stPayDt_EnabledAndMandatory(pPriceRateType.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPriceRateType"></param>
        private void SetCalcRateAnd1stPayDt_EnabledAndMandatory(string pPriceRateType)
        {

            bool isEnabledAndMandatory = true;

            if (pPriceRateType == PriceRateType3CodeEnum.DISC.ToString())
                isEnabledAndMandatory = false;
            else if (pPriceRateType == PriceRateType3CodeEnum.YIEL.ToString())
                isEnabledAndMandatory = true;
            //
            for (int i = 0; i < this.DebtSecurityStreamLenght; i++)
            {
                CcisBase.Set(cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate), "IsEnabled", isEnabledAndMandatory);
                CcisBase.Set(cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate), "IsMandatory", isEnabledAndMandatory);
                //
                CcisBase.Set(cciStream[i].CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), "IsEnabled", isEnabledAndMandatory);
            }
            //
            CcisBase.Set(cciStreamGlobal.CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), "IsEnabled", isEnabledAndMandatory);
        }

  

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetMainCurrency()
        {
            string ret = string.Empty;
            if (debtSecurity.Stream[0].CalculationPeriodAmount.CalculationSpecified)
            {
                ICalculation calc = debtSecurity.Stream[0].CalculationPeriodAmount.Calculation;
                if (calc.NotionalSpecified)
                    ret = calc.Notional.StepSchedule.Currency.Value;
            }
            else if (debtSecurity.Stream[0].CalculationPeriodAmount.KnownAmountScheduleSpecified)
                ret = debtSecurity.Stream[0].CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;

            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeDebtSecurityStream_FromCci()
        {

            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                //
                CciStream cciStreamCurrent = new CciStream(cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_debtSecurityStream, index + 1, null);
                //
                isOk = CcisBase.Contains(cciStreamCurrent.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(debtSecurity.Stream) || (index == debtSecurity.Stream.Length))
                    {
                        ReflectionTools.AddItemInArray(debtSecurity, "debtSecurityStream", index);
                        // Il faudra ici initialiser les parties
                    }
                    cciStreamCurrent.Irs = debtSecurity.Stream[index];
                    //20090922 FI l'acheteur d'un titre peut être l'emetteur (dans cs payer=receiver) => isSwapReceiverAndPayer = false
                    cciStreamCurrent.IsSwapReceiverAndPayer = false;
                    // 
                    lst.Add(cciStreamCurrent);
                }
            }
            cciStream = (CciStream[])lst.ToArray(typeof(CciStream));
            //
            #region génération ds chaque Ccistream des ccis du CciStreamGlobal
            for (int i = 0; i < CcisBase.Count; i++)
            {
                CustomCaptureInfo cci = CcisBase[i];
                if (cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        for (int j = 0; j < DebtSecurityStreamLenght; j++)
                            Ccis.CloneGlobalCci(clientId_Key, cci, cciStream[j]);
                    }
                }
            }
            #endregion génération ds chaque Tradestream des ccis du  CciStreamGlobal

            /* FI 20191209 [XXXXX] Mise en commentaire.
               Cela a pour conséquence de générer systématiquement un stub début dès lors que la zone firstRegularPeriofStartDate est affichée (Ce qui est le cas dans nos écrans aujourd’hui).
               Tous les titres n'ont pas nécessairement un stub début 
              
            //
            // 20090619 RD Pour gérer le CciStub qui contient les BDC et la RollConvention 
            // 
            CciStub cciStub = new CciStub(cciTrade, null, cciStreamGlobal.prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_initialStub, cciStreamGlobal.irs);
            //
            isOk = ccis.Contains(cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_firstRegularPeriodStartDate));
            if (isOk)
            {
                CciTools.AddCciSystem(ccis, Cst.TXT + cciStub.CciClientId(CciStub.CciEnum.rate.ToString()), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.TXT + cciStub.CciClientId(CciStub.CciEnum.floatingRate1.ToString()), false, TypeData.TypeDataEnum.@string);
            }
            */
            
            cciStreamGlobal.Initialize_FromCci();
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].Initialize_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializeSecurity_FromCci()
        {
            if ((null == debtSecurity.Security))
                debtSecurity.Security = cciTrade.DataDocument.CurrentProduct.ProductBase.CreateSecurity();
            
            cciSecurity.security = debtSecurity.Security;
            cciSecurity.Initialize_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void SynchronizeBDAFixedRate(CustomCaptureInfo pCci)
        {

            ArrayList alFloatingRateStream = new ArrayList();
            //
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
            {
                if (cciStream[i].IsFloatingRateSpecified)
                    alFloatingRateStream.Add(i);
            }
            //
            foreach (int streamFloatIndex in alFloatingRateStream)
            {
                //Si Existence Taux Flottant
                //synchronisation des BDA des stream à taux fixe (ou non renseigné) en fonction des BDA du 1er stream à taux flottant
                //Ces derniers sont en général alimentés avec les infos rattachées au rate_Index 
                //Ces Synchro n'est réalisée que lorsque La Zone BDC n'existe pas ds l'écran 

                for (int i = 0; i < DebtSecurityStreamLenght; i++)
                {
                    if (i != streamFloatIndex)
                    {
                        if (false == cciStream[i].IsFloatingRateSpecified)
                        {
                            string clientId_WithoutPrefix;
                            if (cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC, pCci))
                            {
                                if (0 == ((IInterval)cciStream[i].Irs.CalculationPeriodDates.CalculationPeriodFrequency).CompareTo(cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodFrequency))
                                {
                                    //CalculationPeriodDates
                                    clientId_WithoutPrefix = cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }
                            //
                            if (cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC, pCci))
                            {
                                if (0 == cciStream[i].Irs.PaymentDates.PaymentFrequency.CompareTo(cciStream[streamFloatIndex].Irs.PaymentDates.PaymentFrequency))
                                {
                                    //paymentDates
                                    clientId_WithoutPrefix = cciStream[i].CciClientId(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = cciStream[streamFloatIndex].Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
            //
            cciStreamGlobal.Clear();
            //
            for (int i = 0; i < DebtSecurityStreamLenght; i++)
                cciStream[i].Clear();
            //
            cciSecurity.Clear();

        }

        /// <summary>
        /// Calcul du face amount en fonction du nominal et du numberOfIssued
        /// </summary>
        /// <returns></returns>
        private IMoney CalcFaceAmount()
        {
            IMoney ret = cciTrade.CurrentTrade.Product.ProductBase.CreateMoney();
            try
            {
                //
                if (debtSecurity.Security.NumberOfIssuedSecuritiesSpecified)
                {
                    IMoney nominal = new DebtSecurityContainer(debtSecurity).GetNominal(cciTrade.DataDocument.CurrentProduct.ProductBase);
                    ret.Amount.DecValue = debtSecurity.Security.NumberOfIssuedSecurities.DecValue * nominal.Amount.DecValue;
                    ret.Currency = nominal.Currency;
                }
            }
            catch
            {
                ret.Amount.DecValue = Decimal.Zero;
                ret.Currency = string.Empty;
            }
            return ret;
        }

        /// <summary>
        /// Calcul du nominal en fonction de numberOfIssued et du faceAmount
        /// </summary>
        /// <returns></returns>
        private IMoney CalcNominal()
        {
            IMoney ret = cciTrade.CurrentTrade.Product.ProductBase.CreateMoney();
            try
            {
                if ((debtSecurity.Security.NumberOfIssuedSecuritiesSpecified) && (debtSecurity.Security.FaceAmountSpecified))
                {
                    if (debtSecurity.Security.NumberOfIssuedSecurities.DecValue > 0)
                    {
                        ret.Amount.DecValue = debtSecurity.Security.FaceAmount.Amount.DecValue / debtSecurity.Security.NumberOfIssuedSecurities.DecValue;
                        ret.Currency = debtSecurity.Security.FaceAmount.Currency;
                    }
                }
            }
            catch
            {
                ret.Amount.DecValue = Decimal.Zero;
                ret.Currency = string.Empty;
            }
            return ret;
        }

        /// <summary>
        /// Calcul du nombre de titre en fonction du face amount et du nominal
        /// </summary>
        /// <returns></returns>
        private long CalcNumberOfIssued()
        {
            long ret = 0;
            try
            {
                IMoney nominal = new DebtSecurityContainer(debtSecurity).GetNominal(cciTrade.DataDocument.CurrentProduct.ProductBase);
                if (null != nominal)
                {
                    if ((debtSecurity.Security.FaceAmountSpecified) && (nominal.Amount.DecValue > 0))
                    {
                        decimal number = debtSecurity.Security.FaceAmount.Amount.DecValue / nominal.Amount.DecValue;
                        ret = Decimal.ToInt64(number);
                    }
                }
            }
            catch { ret = 0; }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetNominal()
        {

            CustomCaptureInfo cciInitialAmount = cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue);
            CustomCaptureInfo cciInitialAmountCurrency = cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency);

            IMoney nominal = CalcNominal();
            if (nominal.Amount.DecValue > decimal.Zero)
            {
                cciInitialAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(nominal.Amount.DecValue);
                cciInitialAmountCurrency.NewValue = nominal.Currency;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetFaceAmount()
        {
            //
            CustomCaptureInfo cciFaceAmount = cciSecurity.Cci(CciSecurity.CciEnum.faceAmount_amount);
            CustomCaptureInfo cciFaceAmountCurrency = cciSecurity.Cci(CciSecurity.CciEnum.faceAmount_currency);
            //
            IMoney faceAmount = CalcFaceAmount();
            if (faceAmount.Amount.DecValue > decimal.Zero)
            {
                cciFaceAmount.NewValue = StrFunc.FmtDecimalToInvariantCulture(faceAmount.Amount.DecValue);
                cciFaceAmountCurrency.NewValue = faceAmount.Currency;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetNumberOfIssued()
        {
            CustomCaptureInfo cciNumberOfIssued = cciSecurity.Cci(CciSecurity.CciEnum.numberOfIssuedSecurities);
            //
            long number = CalcNumberOfIssued();
            if (number > 0)
            {
                cciNumberOfIssued.NewValue = StrFunc.FmtIntegerToInvariantCulture(number);
            }
        }

        #endregion Methods
    }
}



    
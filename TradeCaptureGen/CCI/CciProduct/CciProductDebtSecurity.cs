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
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public class CciProductDebtSecurity : CciProductBase
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Del calculationPeriodDates_effectiveMinDate|calculationPeriodDates_terminationMaxDate UNUSED
        // EG 20190823 [FIXEDINCOME] Add type|previousCouponDate
        public enum CciEnum
        {
            type,
            previousCouponDate,
            //calculationPeriodDates_effectiveMinDate,
            //calculationPeriodDates_terminationMaxDate,
            unknown,
        }
        #endregion CciEnum

        #region Members
        
        private IDebtSecurity _debtSecurity;

        private CciStream _cciStreamGlobal; //Represente les ccis dits globaux
        private CciStream[] _cciStream;
        private CciSecurity _cciSecurity;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public int DebtSecurityStreamLength
        {
            get { return ArrFunc.IsFilled(_cciStream) ? _cciStream.Length : 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTradeBase CciTrade
        {
            get { return base.CciTradeCommon as CciTradeBase; }
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

        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pDebtSecurity"></param>
        /// <param name="pPrefix"></param>
        public CciProductDebtSecurity(CciTradeDebtSecurity pCciTrade, IDebtSecurity pDebtSecurity, string pPrefix)
            : this(pCciTrade, pDebtSecurity, pPrefix, -1)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pDebtSecurity"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pNumber"></param>
        public CciProductDebtSecurity(CciTradeDebtSecurity pCciTrade, IDebtSecurity pDebtSecurity, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pDebtSecurity, pPrefix, pNumber)
        {
            
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciPayerReceiver Members
        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdPayer
        {
            get
            {
                return _cciStreamGlobal.CciClientIdPayer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdReceiver
        {
            get
            {
                return _cciStreamGlobal.CciClientIdReceiver;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);

        }

        #endregion IContainerCciPayerReceiver Members

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.BUT + _cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);

            #region Add Payer/receiver sur cciStreamGlobal
            ArrayList PayersReceivers = new ArrayList
            {
                CciClientIdPayer,
                CciClientIdReceiver
            };
            IEnumerator ListEnum = PayersReceivers.GetEnumerator();
            while (ListEnum.MoveNext())
            {
                string clientId_WithoutPrefix = ListEnum.Current.ToString();
                CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            CcisBase[CciClientIdReceiver].IsMandatory = CcisBase[CciClientIdPayer].IsMandatory;
            #endregion

            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].AddCciSystem();

            _cciSecurity.AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {

            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].CleanUp();
            //
            _cciSecurity.CleanUp();
            //
            // Suppression des streams issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(_debtSecurity.Stream))
            {
                for (int i = _debtSecurity.Stream.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_debtSecurity.Stream[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_debtSecurity, "debtSecurityStream", i);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Del calculationPeriodDates_effectiveMinDate|calculationPeriodDates_terminationMaxDate UNUSED
        // EG 20190823 [FIXEDINCOME] Add type|previousCouponDate
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
                        case CciEnum.type:
                            #region Type
                            _debtSecurity.DebtSecurityType = ReflectionTools.ConvertStringToEnum<DebtSecurityTypeEnum>(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Type
                            break;
                        case CciEnum.previousCouponDate:
                            #region Type
                            _debtSecurity.PrevCouponDateSpecified = StrFunc.IsFilled(data);
                            if (_debtSecurity.PrevCouponDateSpecified)
                                _debtSecurity.PrevCouponDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Type
                            break;
                        //case CciEnum.calculationPeriodDates_effectiveMinDate:
                        //    #region Min EffectiveDate
                        //    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                        //    #endregion Min EffectiveDate
                        //    break;
                        //case CciEnum.calculationPeriodDates_terminationMaxDate:
                        //    #region Max TerminationDate
                        //    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                        //    #endregion Max TerminationDate
                        //    break;
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
                if (cci.HasChanged && (DebtSecurityStreamLength > 0) && _cciStream[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = _cciStream[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if (CcisBase.Contains(_cciStreamGlobal.CciClientId(cciEnum)))
                            _cciStreamGlobal.Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion
            #region synchronize cciStream[i] from  tradeStreamGlobal (exclude Payer/receiver)
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && _cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = _cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        //
                        for (int i = 0; i < DebtSecurityStreamLength; i++)
                            _cciStream[i].Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion
            //			
            _cciStreamGlobal.Dump_ToDocument();
            //
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].Dump_ToDocument();
            //
            _cciSecurity.Dump_ToDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {


            _cciSecurity.Initialize_Document();

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            InitializeDebtSecurityStream_FromCci();
            InitializeSecurity_FromCci();

        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20190823 [FIXEDINCOME] Del calculationPeriodDates_effectiveMinDate|calculationPeriodDates_terminationMaxDate UNUSED
        // EG 20190823 [FIXEDINCOME] Add type|previousCouponDate
        public override void Initialize_FromDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                //if ((cci != null) && (cci.HasChanged))
                if (cci != null)
                {
                    #region Reset variables

                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region DebtSecurity_Type
                        case CciEnum.type:
                            data = _debtSecurity.DebtSecurityType.ToString();
                            break;
                        #endregion DebtSecurity_Type
                        #region previousCouponDate
                        case CciEnum.previousCouponDate:
                            data = _debtSecurity.PrevCouponDate.Value;
                            break;
                        #endregion previousCouponDate
                        #region calculationPeriodDates_effectiveMinDate & calculationPeriodDates_terminationMaxDate
                        //case CciEnum.calculationPeriodDates_effectiveMinDate:
                        //    data = _debtSecurity.stream[0].calculationPeriodDates.effectiveDateAdjustable.unadjustedDate.Value;
                        //    break;
                        //case CciEnum.calculationPeriodDates_terminationMaxDate:
                        //    for (int i = _debtSecurity.stream.Length - 1; i > -1; i--)
                        //    {
                        //        //On ne récupère pas la termination date de la dernière jambe
                        //        //En création lorsqu'il y a ajout d'un stream, la date terminationMaxDate est initialisée à blanc (On pert la saisie déjà effectuée)
                        //        if (DtFunc.IsDateTimeFilled(_debtSecurity.stream[i].calculationPeriodDates.terminationDateAdjustable.unadjustedDate.DateValue))
                        //        {
                        //            data = _debtSecurity.stream[i].calculationPeriodDates.terminationDateAdjustable.unadjustedDate.Value;
                        //            break;
                        //        }
                        //    }
                        //    break;
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
            if (_debtSecurity.Security.PriceRateTypeSpecified)
                SetCalcRateAnd1stPayDt_EnabledAndMandatory(_debtSecurity.Security.PriceRateType);
            //
            _cciStreamGlobal.Initialize_FromDocument();
            //
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].Initialize_FromDocument();
            //
            _cciSecurity.Initialize_FromDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = _cciStreamGlobal.IsClientId_PayerOrReceiver(pCci);
            if (false == isOk)
            {
                for (int i = 0; i < DebtSecurityStreamLength; i++)
                {
                    isOk = isOk || _cciStream[i].IsClientId_PayerOrReceiver(pCci);
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
        // EG 20190823 [FIXEDINCOME] Del calculationPeriodDates_effectiveMinDate|calculationPeriodDates_terminationMaxDate UNUSED
        // EG 20190823 [FIXEDINCOME] Add type
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = string.Empty;
                bool isOk = true;

                if (isOk)
                {
                    #region CciDebtSecurity
                    clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //
                    CciEnum elt = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //
                    //int k = 0;
                    switch (elt)
                    {
                        case CciEnum.type:
                            CcisBase.SetNewValue(_cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), IsPerpetual ? DtFunc.DateTimeToStringDateISO(DateTime.MaxValue) : null, false);
                            break;
                        //case CciEnum.calculationPeriodDates_effectiveMinDate:
                        //    #region calculationPeriodDates_effectiveMinDate
                        //    // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                        //    //ccis.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                        //    //ccis.SetNewValue(_cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                        //    ccis.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                        //    ccis.SetNewValue(_cciStream[1].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                        //    #endregion
                        //    break;
                        //case CciEnum.calculationPeriodDates_terminationMaxDate:
                        //    #region calculationPeriodDates_terminationMaxDate
                        //    Math.DivRem(DebtSecurityStreamLength, 2, out k);
                        //    if (0 == k)
                        //    {
                        //        // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                        //        //ccis.SetNewValue(_cciStream[DebtSecurityStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                        //        //ccis.SetNewValue(_cciStream[DebtSecurityStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        //        ccis.SetNewValue(_cciStream[DebtSecurityStreamLength - 2].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        //    }
                        //    // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false    
                        //    //ccis.SetNewValue(_cciStream[DebtSecurityStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                        //    //ccis.SetNewValue(_cciStream[DebtSecurityStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        //    ccis.SetNewValue(_cciStream[DebtSecurityStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        //    break;
                        //    #endregion
                        case CciEnum.unknown:
                            isOk = false;
                            break;
                    }
                    #endregion
                }

                if (!isOk)
                {
                    #region CciUnderlyingAsset
                    isOk = _cciSecurity.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    if (isOk)
                    {
                        clientId_Element = _cciSecurity.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        //
                        CciUnderlyingAsset.CciEnum eltUnderlyingAsset = CciUnderlyingAsset.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciUnderlyingAsset.CciEnum), clientId_Element))
                            eltUnderlyingAsset = (CciUnderlyingAsset.CciEnum)System.Enum.Parse(typeof(CciUnderlyingAsset.CciEnum), clientId_Element);
                        //
                        switch (eltUnderlyingAsset)
                        {
                            case CciUnderlyingAsset.CciEnum.currency:
                                for (int i = 0; i < DebtSecurityStreamLength; i++)
                                    CcisBase.SetNewValue(_cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), pCci.NewValue, false);
                                break;
                            case CciUnderlyingAsset.CciEnum.unknown:
                                isOk = false;
                                break;
                        }
                    }
                    #endregion
                }

                CustomCaptureInfo cciFaceAmount = _cciSecurity.Cci(CciSecurity.CciEnum.faceAmount_amount);

                CustomCaptureInfo cciInitialAmount = _cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue);
                CustomCaptureInfo cciInitialAmountCurrency = _cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency);
                //
                CustomCaptureInfo cciQte = _cciSecurity.Cci(CciSecurity.CciEnum.numberOfIssuedSecurities);
                bool isOkToCalc = (null != cciQte && null != cciInitialAmount && null != cciFaceAmount);
                //
                if (!isOk)
                {
                    #region CciSecurity
                    isOk = _cciSecurity.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    //
                    if (isOk)
                    {
                        //
                        CciSecurity.CciEnum eltSecurity = CciSecurity.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciSecurity.CciEnum), clientId_Element))
                            eltSecurity = (CciSecurity.CciEnum)System.Enum.Parse(typeof(CciSecurity.CciEnum), clientId_Element);
                        //
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
                                        CcisBase.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate), string.Empty, false);
                                        CcisBase.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), string.Empty, false);
                                        CcisBase.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier), "0", false);
                                        CcisBase.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_period), PeriodEnum.T.ToString(), false);
                                        CcisBase.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodFrequency), "0" + PeriodEnum.T.ToString(), false);
                                        //
                                        CcisBase.SetNewValue(_cciStreamGlobal.CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), string.Empty, false);
                                        //
                                        CustomCaptureInfo cciCouponType = _cciSecurity.Cci(CciSecurity.CciEnum.couponType);
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
                    isOk = _cciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    if (isOk)
                    {
                        clientId_Element = _cciStreamGlobal.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        //
                        CciStream.CciEnum eltStream = CciStream.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Element))
                            eltStream = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Element);
                        //
                        switch (eltStream)
                        {
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency:
                                CcisBase.SetNewValue(_cciSecurity.CciClientId(CciSecurity.CciEnum.faceAmount_currency), pCci.NewValue, false);
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
                if (!isOk)
                {
                    #region cciStream[0]
                    isOk = _cciStream[0].IsCciOfContainer(pCci.ClientId_WithoutPrefix);
                    if (isOk)
                    {
                        clientId_Element = _cciStream[0].CciContainerKey(pCci.ClientId_WithoutPrefix);
                        //
                        CciStream.CciEnum eltStream = CciStream.CciEnum.unknown;
                        if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Element))
                            eltStream = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Element);
                        //
                        switch (eltStream)
                        {
                            case CciStream.CciEnum.calculationPeriodAmount_calculation_rate:
                                if (_cciStream[0].IsFixedRateSpecified)
                                {
                                    CcisBase.SetNewValue(_cciSecurity.CciClientId(CciSecurity.CciEnum.couponType), CouponTypeEnum.Fixed.ToString(), false);
                                }
                                else if (_cciStream[0].IsFloatingRateSpecified)
                                {
                                    CcisBase.SetNewValue(_cciSecurity.CciClientId(CciSecurity.CciEnum.couponType), CouponTypeEnum.Float.ToString(), false);
                                    CcisBase.SetNewValue(_cciSecurity.CciClientId(CciSecurity.CciEnum.priceRateType), PriceRateType3CodeEnum.YIEL.ToString(), false);
                                }
                                //
                                /* FI 20191209 [XXXXX] Mise en commentaire.
                                   Il n'y a pas de sens à mettre un tx identique. Les tx sur les stubs doivent être différents sinon il n'est pas nécessaire de les renseigner 
                                
                                CciStub cciStub = new CciStub(cciTrade, null, cciStreamGlobal.prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_initialStub, _cciStreamGlobal.irs);
                                if (RateTools.IsFloatingRate(pCci.NewValue))
                                    ccis.SetNewValue(cciStub.CciClientId(CciStub.CciEnum.floatingRate1), pCci.NewValue, false);
                                else
                                    ccis.SetNewValue(cciStub.CciClientId(CciStub.CciEnum.rate), pCci.NewValue, false);
                                */

                                break;
                            case CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_period:
                            case CciStream.CciEnum.calculationPeriodDates_calculationPeriodFrequency_periodMultiplier:
                                bool isZeroCoupon = Tools.IsPeriodZeroCoupon(_cciStream[0].Irs.CalculationPeriodDates.CalculationPeriodFrequency.Interval);
                                if (isZeroCoupon)
                                    CcisBase.SetNewValue(_cciSecurity.CciClientId(CciSecurity.CciEnum.priceRateType), PriceRateType3CodeEnum.DISC.ToString(), false);
                                break;
                            case CciStream.CciEnum.unknown:
                                break;
                        }
                    }
                    #endregion
                }
            }
            //
            _cciStreamGlobal.ProcessInitialize(pCci);
            //
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].ProcessInitialize(pCci);
            //
            SynchronizeBDAFixedRate(pCci);
            //

        }
        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            _cciSecurity.RefreshCciEnabled();
            //
            _cciStreamGlobal.RefreshCciEnabled();
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].RefreshCciEnabled();
            //

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            _cciStreamGlobal.SetDisplay(pCci);
            //
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].SetDisplay(pCci);

        }
        #endregion IContainerCciFactory Members

        #region ICciGetInfoButton Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;

            #region Button on StreamGlobal
            if (_cciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                if (isOk)
                {
                    pCo.Object = "calculationNotional";
                    pCo.Element = "notionalStepSchedule";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = _cciStreamGlobal.IsNotionalStepScheduleStepSpecified;
                    pIsEnabled = true;
                }
                //
                if (false == isOk)
                {
                    isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = _cciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "";
                        pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = _cciStreamGlobal.IsKnownAmountScheduleStepSpecified;
                        pIsEnabled = true;
                    }
                }
                //
                if (false == isOk)
                {
                    isOk = _cciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = _cciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            #endregion Button on StreamGlobal

            #region  CciStream[i]
            if (false == isOk)
            {
                for (int i = 0; i < this.DebtSecurityStreamLength; i++)
                {
                    if (_cciStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string key = _cciStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
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
                                pIsSpecified = _cciStream[i].IsNotionalStepScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepParameters";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsNotionalStepParametersSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsKnownAmountScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "capRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsCapRateScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floorRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsFloorRateScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "spreadSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsSpreadScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floatingRateMultiplierSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsFloatingRateMultiplierScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "rateFixedRate";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsFixedRateScheduleSpecified;
                                pIsEnabled = _cciStream[i].IsFixedRateSpecified;
                                break;

                            case CciStream.CciEnum.paymentDates_offset:
                                pCo.Object = "paymentDates";
                                pCo.Element = "paymentDaysOffset";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = _cciStream[i].IsOffsetSpecified;
                                pIsEnabled = true;
                                break;

                            default:
                                isOk = false;
                                break;
                        }
                    }
                }
            }
            #endregion  CciStream[i]
            //
            #region buttons of Security
            if (!isOk)
                isOk = _cciSecurity.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion
            //
            return isOk;
        }

        #endregion ICciGetInfoButton Members

        #region ITradeCci Members
        #region CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get { return string.Empty; }
        }
        #endregion CciClientIdMainCurrency
        /// <summary>
        /// 
        /// </summary>
        public override string GetMainCurrency
        {
            get
            {
                string ret = string.Empty;
                if (_debtSecurity.Stream[0].CalculationPeriodAmount.CalculationSpecified)
                {
                    ICalculation calc = _debtSecurity.Stream[0].CalculationPeriodAmount.Calculation;
                    if (calc.NotionalSpecified)
                        ret = calc.Notional.StepSchedule.Currency.Value;
                }
                else if (_debtSecurity.Stream[0].CalculationPeriodAmount.KnownAmountScheduleSpecified)
                    ret = _debtSecurity.Stream[0].CalculationPeriodAmount.KnownAmountSchedule.Currency.Value;

                return ret;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string RetSidePayer
        {
            get
            {
                string ret = SideTools.RetSellSide();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string RetSideReceiver
        {
            get { return SideTools.RetReverseSide(this.RetSidePayer); }
        }

        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        // EG 20190823 [FIXEDINCOME] Hide|Display MaturityDate (Perpetual|Ordinary)
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            System.Web.UI.Control control = pPage.PlaceHolder.FindControl(Cst.TXT + _cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate));
            if (null != control)
                control.Visible = (false == IsPerpetual);
            control = pPage.PlaceHolder.FindControl(Cst.LBL + _cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate));
            if (null != control)
                control.Visible = (false == IsPerpetual);


            for (int i = 0; i < ArrFunc.Count(_cciStream); i++)
            {
                _cciStream[i].DumpSpecific_ToGUI(pPage);
            }

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion

        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {

            _debtSecurity = (IDebtSecurity)pProduct;

            IDebtSecurityStream streamGlobal = null;
            if ((null != _debtSecurity) && ArrFunc.IsFilled(_debtSecurity.Stream))
                streamGlobal = _debtSecurity.Stream[0];

            _cciStreamGlobal = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_debtSecurityStream, -1, streamGlobal)
            {
                //20090922 FI L'acheteur d'un titre peut être l'emetteur (dans cs payer=receiver) => isSwapReceiverAndPayer = false
                IsSwapReceiverAndPayer = false
            };

            _cciStream = null;

            ISecurity security = null;
            if (null != _debtSecurity)
                security = _debtSecurity.Security;
            _cciSecurity = new CciSecurity(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_security, security);

            base.SetProduct(pProduct);

        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
            
            _cciStreamGlobal.Clear();
            
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].Clear();
            
            _cciSecurity.Clear();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            
            _cciStreamGlobal.SetEnabled(pIsEnabled);
            
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].SetEnabled(pIsEnabled);
            
            _cciSecurity.SetEnabled(pIsEnabled);

        }



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

            for (int i = 0; i < this.DebtSecurityStreamLength; i++)
            {
                CcisBase.Set(_cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate), "IsEnabled", isEnabledAndMandatory);
                CcisBase.Set(_cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate), "IsMandatory", isEnabledAndMandatory);
                //
                CcisBase.Set(_cciStream[i].CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), "IsEnabled", isEnabledAndMandatory);
            }
            //
            CcisBase.Set(_cciStreamGlobal.CciClientId(CciStream.CciEnum.paymentDates_firstPaymentDate), "IsEnabled", isEnabledAndMandatory);

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
                CciStream cciStreamCurrent = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_debtSecurityStream, index + 1, null);
                //
                isOk = CcisBase.Contains(cciStreamCurrent.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_rate));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_debtSecurity.Stream) || (index == _debtSecurity.Stream.Length))
                    {
                        ReflectionTools.AddItemInArray(_debtSecurity, "debtSecurityStream", index);
                        // Il faudra ici initialiser les parties
                    }
                    cciStreamCurrent.Irs = _debtSecurity.Stream[index];
                    //20090922 FI l'acheteur d'un titre peut être l'emetteur (dans cs payer=receiver) => isSwapReceiverAndPayer = false
                    cciStreamCurrent.IsSwapReceiverAndPayer = false;
                    // 
                    lst.Add(cciStreamCurrent);
                }
            }
            _cciStream = (CciStream[])lst.ToArray(typeof(CciStream));
            //
            #region génération ds chaque Ccistream des ccis du CciStreamGlobal
            for (int i = 0; i < CcisBase.Count; i++)
            {
                CustomCaptureInfo cci = CcisBase[i];
                if (_cciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = _cciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        for (int j = 0; j < DebtSecurityStreamLength; j++)
                            Ccis.CloneGlobalCci(clientId_Key, cci, _cciStream[j]);
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
            CciStub cciStub = new CciStub(_cciTrade, null, _cciStreamGlobal.prefix + TradeCustomCaptureInfos.CCst.Prefix_stubCalculationPeriodAmount + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_initialStub, _cciStreamGlobal.irs);
            //
            isOk = ccis.Contains(_cciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodDates_firstRegularPeriodStartDate));
            if (isOk)
            {
                CciTools.AddCciSystem(ccis, Cst.TXT + cciStub.CciClientId(CciStub.CciEnum.rate.ToString()), false, TypeData.TypeDataEnum.@string);
                CciTools.AddCciSystem(ccis, Cst.TXT + cciStub.CciClientId(CciStub.CciEnum.floatingRate1.ToString()), false, TypeData.TypeDataEnum.@string);
            }
            
            */
            _cciStreamGlobal.Initialize_FromCci();
            for (int i = 0; i < DebtSecurityStreamLength; i++)
                _cciStream[i].Initialize_FromCci();

        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializeSecurity_FromCci()
        {
            if ((null == _debtSecurity.Security))
                _debtSecurity.Security = CciTrade.DataDocument.CurrentProduct.ProductBase.CreateSecurity();
            //
            _cciSecurity.security = _debtSecurity.Security;
            _cciSecurity.Initialize_FromCci();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        private void SynchronizeBDAFixedRate(CustomCaptureInfo pCci)
        {

            ArrayList alFloatingRateStream = new ArrayList();
            //
            for (int i = 0; i < DebtSecurityStreamLength; i++)
            {
                if (_cciStream[i].IsFloatingRateSpecified)
                    alFloatingRateStream.Add(i);
            }
            //
            foreach (int streamFloatIndex in alFloatingRateStream)
            {
                //Si Existence Taux Flottant
                //synchronisation des BDA des stream à taux fixe (ou non renseigné) en fonction des BDA du 1er stream à taux flottant
                //Ces derniers sont en général alimentés avec les infos rattachées au rate_Index 
                //Ces Synchro n'est réalisée que lorsque La Zone BDC n'existe pas ds l'écran 

                for (int i = 0; i < DebtSecurityStreamLength; i++)
                {
                    if (i != streamFloatIndex)
                    {
                        if (false == _cciStream[i].IsFloatingRateSpecified)
                        {
                            string clientId_WithoutPrefix;
                            if (_cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC, pCci))
                            {
                                if (0 == ((IInterval)_cciStream[i].Irs.CalculationPeriodDates.CalculationPeriodFrequency).CompareTo(_cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodFrequency))
                                {
                                    //CalculationPeriodDates
                                    clientId_WithoutPrefix = _cciStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_calculationPeriodDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = _cciStream[streamFloatIndex].Irs.CalculationPeriodDates.CalculationPeriodDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }

                            if (_cciStream[streamFloatIndex].IsCci(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC, pCci))
                            {
                                if (0 == _cciStream[i].Irs.PaymentDates.PaymentFrequency.CompareTo(_cciStream[streamFloatIndex].Irs.PaymentDates.PaymentFrequency))
                                {
                                    //paymentDates
                                    clientId_WithoutPrefix = _cciStream[i].CciClientId(CciStream.CciEnum.paymentDates_paymentDatesAdjustments_bDC);
                                    CcisBase[clientId_WithoutPrefix].NewValue = _cciStream[streamFloatIndex].Irs.PaymentDates.PaymentDatesAdjustments.BusinessDayConvention.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Calcul du face amount en fonction du nominal et du numberOfIssued
        /// </summary>
        /// <returns></returns>
        private IMoney CalcFaceAmount()
        {
            IMoney ret = CciTrade.CurrentTrade.Product.ProductBase.CreateMoney();
            try
            {
                //
                if (_debtSecurity.Security.NumberOfIssuedSecuritiesSpecified)
                {
                    IMoney nominal = new DebtSecurityContainer(_debtSecurity).GetNominal(CciTrade.DataDocument.CurrentProduct.ProductBase);
                    ret.Amount.DecValue = _debtSecurity.Security.NumberOfIssuedSecurities.DecValue * nominal.Amount.DecValue;
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
            IMoney ret = CciTrade.CurrentTrade.Product.ProductBase.CreateMoney();
            try
            {
                if ((_debtSecurity.Security.NumberOfIssuedSecuritiesSpecified) && (_debtSecurity.Security.FaceAmountSpecified))
                {
                    if (_debtSecurity.Security.NumberOfIssuedSecurities.DecValue > 0)
                    {
                        ret.Amount.DecValue = _debtSecurity.Security.FaceAmount.Amount.DecValue / _debtSecurity.Security.NumberOfIssuedSecurities.DecValue;
                        ret.Currency = _debtSecurity.Security.FaceAmount.Currency;
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
                IMoney nominal = new DebtSecurityContainer(_debtSecurity).GetNominal(CciTrade.DataDocument.CurrentProduct.ProductBase);
                if (null != nominal)
                {
                    if ((_debtSecurity.Security.FaceAmountSpecified) && (nominal.Amount.DecValue > 0))
                    {
                        decimal number = _debtSecurity.Security.FaceAmount.Amount.DecValue / nominal.Amount.DecValue;
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

            CustomCaptureInfo cciInitialAmount = _cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue);
            CustomCaptureInfo cciInitialAmountCurrency = _cciStreamGlobal.Cci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency);

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
            CustomCaptureInfo cciFaceAmount = _cciSecurity.Cci(CciSecurity.CciEnum.faceAmount_amount);
            CustomCaptureInfo cciFaceAmountCurrency = _cciSecurity.Cci(CciSecurity.CciEnum.faceAmount_currency);
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
            CustomCaptureInfo cciNumberOfIssued = _cciSecurity.Cci(CciSecurity.CciEnum.numberOfIssuedSecurities);
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

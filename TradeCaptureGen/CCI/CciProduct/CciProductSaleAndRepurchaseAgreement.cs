#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
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

    public abstract class CciProductSaleAndRepurchaseAgreement : CciProductBase
    {
        #region Enum
        public enum CciEnum
        {
            effectiveMinDate,
            terminationMaxDate,
            [System.Xml.Serialization.XmlEnumAttribute("duration")]
            duration,
            [System.Xml.Serialization.XmlEnumAttribute("noticePeriod.periodMultiplier")]
            noticePeriod_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("noticePeriod.period")]
            noticePeriod_period,
            [System.Xml.Serialization.XmlEnumAttribute("noticePeriod.dayType")]
            noticePeriod_dayType,
            unknown
        }
        #endregion

        #region Members
        #endregion
        //
        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }
        
        #region public cciStreamGlobal
        public CciStream CciStreamGlobal { get; private set; }
        #endregion
        #region public CciCashStream
        public CciStream[] CciCashStream { get; private set; }
        #endregion
        #region public CciSpotLeg
        public CciSecurityLeg[] CciSpotLeg { get; private set; }
        #endregion
        #region public CciForwardLeg
        public CciSecurityLeg[] CciForwardLeg { get; private set; }
        #endregion

        #region public RepurchaseAgreementContainer
        public SaleAndRepurchaseAgreementContainer RepurchaseAgreementContainer { get; private set; }
        #endregion
        #region public RepurchaseAgreement
        public ISaleAndRepurchaseAgreement RepurchaseAgreement { get; private set; }
        #endregion
        #region public SpotLegLength
        public int SpotLegLength
        {
            get { return ArrFunc.IsFilled(CciSpotLeg) ? CciSpotLeg.Length : 0; }
        }
        #endregion SpotLegLength
        #region public ForwardLegLength
        public int ForwardLegLength
        {
            get { return ArrFunc.IsFilled(CciForwardLeg) ? CciForwardLeg.Length : 0; }
        }
        #endregion ForwardLegLength
        #region public CashStreamLength
        public int CashStreamLength
        {
            get { return ArrFunc.IsFilled(CciCashStream) ? CciCashStream.Length : 0; }
        }
        #endregion CashStreamLength
        #region public IsOpen
        public bool IsOpen
        {
            get { return (CcisBase.GetNewValue(CciClientId(CciEnum.duration)) == RepoDurationEnum.Open.ToString()); }
        }
        #endregion
        #endregion
        //
        #region constructor
        public CciProductSaleAndRepurchaseAgreement(CciTrade pCciTrade, ISaleAndRepurchaseAgreement pRepurchaseAgreement, string pPrefix)
            : this(pCciTrade, pRepurchaseAgreement, pPrefix, -1)
        { }
        public CciProductSaleAndRepurchaseAgreement(CciTrade pCciTrade, ISaleAndRepurchaseAgreement pRepurchaseAgreement, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pRepurchaseAgreement, pPrefix, pNumber)
        {
        }
        #endregion constructor

        #region IContainerCciFactory Members
        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.duration.ToString()), true, TypeData.TypeDataEnum.@string);

            //20070924 FI Ticket 15763
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciStreamGlobal.CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters.ToString()), false, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].AddCciSystem();

            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].AddCciSystem();

            for (int i = 0; i < CashStreamLength; i++)
            {
                string clientId_WithoutPrefix = CciCashStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate);
                CciTools.AddCciSystem(CcisBase, Cst.TXT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.datetime);

                clientId_WithoutPrefix = CciCashStream[i].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate);
                CciTools.AddCciSystem(CcisBase, Cst.TXT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.datetime);

                CciCashStream[i].AddCciSystem();
            }
        }
        #endregion AddCciSystem
        #region public override Initialize_FromCci
        public override void Initialize_FromCci()
        {
            if (CcisBase.Contains(CciClientId(CciEnum.noticePeriod_period)) ||
                CcisBase.Contains(CciClientId(CciEnum.noticePeriod_periodMultiplier)) ||
                CcisBase.Contains(CciClientId(CciEnum.noticePeriod_dayType)))
            {
                if (null == RepurchaseAgreement.NoticePeriod)
                    RepurchaseAgreement.NoticePeriod = CciTrade.CurrentTrade.Product.ProductBase.CreateAdjustableOffset();
            }
            InitializeSpotLeg_FromCci();
            InitializeForwardLeg_FromCci();
            InitializeCashStream_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public override void Initialize_FromDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null))
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region effectiveMinDate & terminationMaxDate
                        case CciEnum.effectiveMinDate:
                            data = RepurchaseAgreement.CashStream[0].CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.Value;
                            break;
                        case CciEnum.terminationMaxDate:
                            //
                            for (int i = RepurchaseAgreement.CashStream.Length - 1; i > -1; i--)
                            {
                                //On ne recupère pas la termination date de la dernière jambe
                                //En création lorsqu'il y a ajout d'un stream, la date terminationMaxDate est initialisée à blanc (On pert la saisie déjà effectuée)
                                //On récupère la dernière jambe renseignée
                                //FI 20091223 [16471] Add test sur  terminationDateAdjustableSpecified 
                                DateTime terminationDate = DateTime.MinValue;
                                if (RepurchaseAgreement.CashStream[i].CalculationPeriodDates.TerminationDateAdjustableSpecified)
                                    terminationDate = RepurchaseAgreement.CashStream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.DateValue;
                                //
                                if (DtFunc.IsDateTimeFilled(terminationDate) && (DateTime.MaxValue.Date != terminationDate.Date))
                                {
                                    data = RepurchaseAgreement.CashStream[i].CalculationPeriodDates.TerminationDateAdjustable.UnadjustedDate.Value;
                                    break;
                                }
                            }
                            break;
                        #endregion
                        #region duration
                        case CciEnum.duration:
                            data = RepurchaseAgreement.Duration.ToString();
                            break;
                        #endregion
                        #region noticePeriod_periodMultiplier
                        case CciEnum.noticePeriod_periodMultiplier:
                            if (RepurchaseAgreement.NoticePeriodSpecified && (null != RepurchaseAgreement.NoticePeriod.PeriodMultiplier))
                                data = RepurchaseAgreement.NoticePeriod.PeriodMultiplier.Value;
                            break;
                        #endregion noticePeriod_periodMultiplier
                        #region noticePeriod_period
                        case CciEnum.noticePeriod_period:
                            if (RepurchaseAgreement.NoticePeriodSpecified)
                                data = RepurchaseAgreement.NoticePeriod.Period.ToString();
                            break;
                        #endregion noticePeriod_period
                        #region noticePeriod_dayType
                        case CciEnum.noticePeriod_dayType:
                            if (RepurchaseAgreement.NoticePeriodSpecified && RepurchaseAgreement.NoticePeriod.DayTypeSpecified)
                                data = RepurchaseAgreement.NoticePeriod.DayType.ToString();
                            break;
                        #endregion noticePeriod_dayType
                        default:
                            isSetting = false;
                            break;
                    }
                    //
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].Initialize_FromDocument();
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].Initialize_FromDocument();
            //
            CciStreamGlobal.Initialize_FromDocument();
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (elt)
                {
                    case CciEnum.duration:
                        if (RepurchaseAgreement.Duration == RepoDurationEnum.Overnight)
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.noticePeriod_dayType), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.noticePeriod_periodMultiplier), string.Empty);
                            CcisBase.SetNewValue(CciClientId(CciEnum.noticePeriod_period), string.Empty);
                        }
                        //
                        if (RepurchaseAgreement.Duration == RepoDurationEnum.Overnight)
                        {
                            DateTime dtEffectiveDate = DateTime.MinValue;
                            if (RepurchaseAgreement.CashStream[0].CalculationPeriodDates.EffectiveDateAdjustableSpecified)
                                dtEffectiveDate = RepurchaseAgreement.CashStream[0].CalculationPeriodDates.EffectiveDateAdjustable.UnadjustedDate.DateValue;
                            if (DateTime.MinValue != dtEffectiveDate)
                                SetTerminationDate(dtEffectiveDate);
                        }
                        //// RD 20100402 / terminationMaxDate is not Mandatory for duration = Open
                        //else if (_repurchaseAgreement.duration == RepoDurationEnum.Open)
                        //{
                        //    if (StrFunc.IsEmpty(ccis.GetNewValue(CciClientId(CciEnum.terminationMaxDate))))
                        //    {
                        //        string errMsgIsMandatory = Ressource.GetString("ISMANDATORY");
                        //        CustomCaptureInfo cciTerminationMaxDate = Cci(CciEnum.terminationMaxDate);
                        //        //
                        //        if (null != cciTerminationMaxDate &&
                        //            StrFunc.IsFilled(cciTerminationMaxDate.ErrorMsg) &&
                        //            cciTerminationMaxDate.ErrorMsg.IndexOf(errMsgIsMandatory) >= 0)
                        //        {
                        //            cciTerminationMaxDate.ErrorMsg = cciTerminationMaxDate.ErrorMsg.Replace(errMsgIsMandatory, string.Empty);
                        //        }
                        //    }
                        //}
                        //
                        break;
                    case CciEnum.effectiveMinDate:
                        if (ArrFunc.IsFilled(CciCashStream))
                        {
                            // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                            //ccis.SetNewValue(_cciStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, true);
                            CcisBase.SetNewValue(CciCashStream[0].CciClientId(CciStream.CciEnum.calculationPeriodDates_effectiveDate), pCci.NewValue, false);
                        }

                        for (int i = 0; i < SpotLegLength; i++)
                            CcisBase.SetNewValue(CciSpotLeg[i].cciDebtSecurityTransaction.cciGrossAmount.CciClientId(CciPayment.CciEnumPayment.paymentDate_unadjustedDate), pCci.NewValue, true);

                        //Calcul auto de la date Echéance si overnight (DateEchance  = effective +1 jour business)
                        if (RepurchaseAgreement.Duration == RepoDurationEnum.Overnight)
                            SetTerminationDate(new DtFunc().StringToDateTime(pCci.NewValue));

                        break;
                    case CciEnum.terminationMaxDate:
                        if (ArrFunc.IsFilled(CciCashStream))
                        {
                            // FI 20161214 [21916] Modification afin d'éviter toute dégradation puisqu'avant correction de la méthode SetNewValue la valeur True sur le paramètre était equivalent à false
                            //ccis.SetNewValue(_cciStream[CashStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, true);
                            CcisBase.SetNewValue(CciCashStream[CashStreamLength - 1].CciClientId(CciStream.CciEnum.calculationPeriodDates_terminationDate), pCci.NewValue, false);
                        }

                        for (int i = 0; i < ForwardLegLength; i++)
                            CcisBase.SetNewValue(CciForwardLeg[i].cciDebtSecurityTransaction.cciGrossAmount.CciClientId(CciPayment.CciEnumPayment.paymentDate_unadjustedDate), pCci.NewValue, true);

                        break;
                }
            }
            //
            #region Calcul de notionalStepSchedule_initialValue
            bool isCciStreamInitialValueToCalc = false;
            //
            for (int i = 0; i < SpotLegLength; i++)
            {
                CciPayment cciGrossAmount = CciSpotLeg[i].cciDebtSecurityTransaction.cciGrossAmount;
                //
                if (cciGrossAmount.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string clientId_Element = cciGrossAmount.CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //
                    CciPayment.CciEnumPayment elt = CciPayment.CciEnumPayment.unknown;
                    if (System.Enum.IsDefined(typeof(CciPayment.CciEnumPayment), clientId_Element))
                        elt = (CciPayment.CciEnumPayment)System.Enum.Parse(typeof(CciPayment.CciEnumPayment), clientId_Element);
                    //
                    switch (elt)
                    {
                        case CciPayment.CciEnumPayment.paymentAmount_amount:
                        case CciPayment.CciEnumPayment.paymentAmount_currency:
                            isCciStreamInitialValueToCalc = true;
                            break;
                    }
                }
                //
                if (isCciStreamInitialValueToCalc)
                    break;
            }
            //
            if (isCciStreamInitialValueToCalc)
            {
                string currency = RepurchaseAgreement.SpotLeg[0].DebtSecurityTransaction.GrossAmount.PaymentAmount.Currency;
                decimal streamInitialValue = 0;
                for (int i = 0; i < RepurchaseAgreement.SpotLeg.Length; i++)
                {
                    IPayment payment = RepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.GrossAmount;
                    if (currency == payment.PaymentAmount.Currency)
                    {
                        if (RepurchaseAgreement.SpotLeg[i].MarginSpecified)
                            streamInitialValue += payment.PaymentAmount.Amount.DecValue * RepurchaseAgreement.SpotLeg[i].Margin.MarginFactor.DecValue;
                        else
                            streamInitialValue += payment.PaymentAmount.Amount.DecValue;
                    }
                }
                //
                if (streamInitialValue > decimal.Zero)
                {
                    for (int i = 0; i < this.CashStreamLength; i++)
                    {
                        CcisBase.SetNewValue(CciCashStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue), StrFunc.FmtDecimalToInvariantCulture(streamInitialValue), false);
                        CcisBase.SetNewValue(CciCashStream[i].CciClientId(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_currency), currency, false);
                    }
                }
            }
            #endregion
            //
            for (int i = 0; i < SpotLegLength; i++)
            {
                CciSpotLeg[i].ProcessInitialize(pCci);
                //
                // Initialisation des forwardLeg en fonction des proporiétés du titre saisie sur le spot leg
                if (CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.IsCci(CciSecurityAsset.CciEnum.securityId, pCci) && (null != pCci.Sql_Table))
                {
                    CciSecurityLeg[] cciForwardLegRef = GetCciForwardLegFromAssetId(CciSpotLeg[i].securityLeg.DebtSecurityTransaction.SecurityAsset.Id);
                    for (int j = 0; j < ArrFunc.Count(cciForwardLegRef); j++)
                        cciForwardLegRef[j].cciDebtSecurityTransaction.InitializeFromDebtSecurity();
                }
                //Mise à jour de la quantité sur les forwardLeg qui fon référence au spot leg saisi
                if (CciSpotLeg[i].cciDebtSecurityTransaction.cciQuantity.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    CciOrderQuantity cciOrderQuantitySpotLeg = CciSpotLeg[i].cciDebtSecurityTransaction.cciQuantity;
                    CciSecurityLeg[] cciForwardLegRef = GetCciForwardLegFromSpotId(CciSpotLeg[i].securityLeg.Id);
                    for (int j = 0; j < ArrFunc.Count(cciForwardLegRef); j++)
                    {
                        CciOrderQuantity cciOrderQuantityForward = cciForwardLegRef[j].cciDebtSecurityTransaction.cciQuantity;
                        if (cciOrderQuantitySpotLeg.IsCci(CciOrderQuantity.CciEnum.quantityType, pCci))
                            CcisBase.SetNewValue(cciOrderQuantityForward.CciClientId(CciOrderQuantity.CciEnum.quantityType), pCci.NewValue, false);
                        else if (cciOrderQuantitySpotLeg.IsCci(CciOrderQuantity.CciEnum.quantityAmount, pCci))
                            CcisBase.SetNewValue(cciOrderQuantityForward.CciClientId(CciOrderQuantity.CciEnum.quantityAmount), pCci.NewValue, false);
                        else if (cciOrderQuantitySpotLeg.IsCci(CciOrderQuantity.CciEnum.notional_amount, pCci))
                            CcisBase.SetNewValue(cciOrderQuantityForward.CciClientId(CciOrderQuantity.CciEnum.notional_amount), pCci.NewValue, false);
                        else if (cciOrderQuantitySpotLeg.IsCci(CciOrderQuantity.CciEnum.notional_currency, pCci))
                            CcisBase.SetNewValue(cciOrderQuantityForward.CciClientId(CciOrderQuantity.CciEnum.notional_currency), pCci.NewValue, false);
                        else if (cciOrderQuantitySpotLeg.IsCci(CciOrderQuantity.CciEnum.numberOfUnits, pCci))
                            CcisBase.SetNewValue(cciOrderQuantityForward.CciClientId(CciOrderQuantity.CciEnum.numberOfUnits), pCci.NewValue, false);
                    }
                }
            }
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].ProcessInitialize(pCci);
            //
            CciStreamGlobal.ProcessInitialize(pCci);
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].ProcessInitialize(pCci);
            //

        }
        #endregion ProcessInitialize
        #region public override ProcessExecute
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].ProcessExecute(pCci);
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].ProcessInitialize(pCci);
            //
            CciStreamGlobal.ProcessExecute(pCci);
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].ProcessExecute(pCci);

        }
        #endregion ProcessExecute
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            //
            if (!isOk)
            {
                for (int i = 0; i < SpotLegLength; i++)
                {
                    isOk = isOk || CciSpotLeg[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            //
            if (!isOk)
            {
                for (int i = 0; i < ForwardLegLength; i++)
                {
                    isOk = isOk || CciForwardLeg[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            //
            if (!isOk)
            {
                isOk = CciStreamGlobal.IsClientId_PayerOrReceiver(pCci);
            }
            //
            if (!isOk)
            {
                for (int i = 0; i < CashStreamLength; i++)
                {
                    isOk = isOk || CciCashStream[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            return isOk;

        }
        #endregion IsClientId_PayerOrReceiver
        #region public override Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
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
                        #region effectiveMinDate & terminationMaxDate
                        case CciEnum.effectiveMinDate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;

                        case CciEnum.terminationMaxDate:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion effectiveMinDate & terminationMaxDate
                        #region duration
                        case CciEnum.duration:
                            if (StrFunc.IsFilled(data))
                            {
                                RepurchaseAgreement.Duration = (RepoDurationEnum)System.Enum.Parse(typeof(RepoDurationEnum), data);
                                RepurchaseAgreement.NoticePeriodSpecified = (RepurchaseAgreement.Duration == RepoDurationEnum.Open);
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion duration
                        #region noticePeriod_periodMultiplier
                        case CciEnum.noticePeriod_periodMultiplier:
                            RepurchaseAgreement.NoticePeriodSpecified = StrFunc.IsFilled(data);
                            RepurchaseAgreement.NoticePeriod.PeriodMultiplier = new EFS_Integer(data);
                            //
                            break;
                        #endregion noticePeriod_periodMultiplier
                        #region noticePeriod_period
                        case CciEnum.noticePeriod_period:
                            RepurchaseAgreement.NoticePeriodSpecified = StrFunc.IsFilled(data);
                            if (RepurchaseAgreement.NoticePeriodSpecified)
                                RepurchaseAgreement.NoticePeriod.Period = (PeriodEnum)System.Enum.Parse(typeof(PeriodEnum), data);
                            break;
                        #endregion noticePeriod_period
                        #region noticePeriod_dayType
                        case CciEnum.noticePeriod_dayType:
                            RepurchaseAgreement.NoticePeriod.DayTypeSpecified = StrFunc.IsFilled(data);
                            if (RepurchaseAgreement.NoticePeriod.DayTypeSpecified)
                                RepurchaseAgreement.NoticePeriod.DayType = (DayTypeEnum)System.Enum.Parse(typeof(DayTypeEnum), data);

                            //
                            break;
                        #endregion noticePeriod_dayType
                        default:
                            isSetting = false;
                            break;
                    }
                    //
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //FI 20091126 [16762] 
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
                Ccis.InitializePaymentPaymentQuoteRelativeTo(CciStreamGlobal, null, null, CciTrade.cciOtherPartyPayment);
            //
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].Dump_ToDocument();
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].Dump_ToDocument();
            //
            RepurchaseAgreement.ForwardLegSpecified = CciTools.Dump_IsCciContainerArraySpecified(RepurchaseAgreement.ForwardLegSpecified, CciForwardLeg);
            //
            #region Update spotLeg and forwardLeg
            if (Cst.Capture.IsModeInput(CcisBase.CaptureMode) && (false == Cst.Capture.IsModeAction(CcisBase.CaptureMode)))
            {
                for (int i = 0; i < RepurchaseAgreement.SpotLeg.Length; i++)
                {
                    // Generation de id
                    if (StrFunc.IsEmpty(RepurchaseAgreement.SpotLeg[i].Id))
                        RepurchaseAgreement.SpotLeg[i].Id = CciTrade.DataDocument.GenerateId(TradeCustomCaptureInfos.CCst.Prefix_spotLeg, true);
                }
                //
                if (RepurchaseAgreement.ForwardLegSpecified)
                {
                    for (int i = 0; i < RepurchaseAgreement.ForwardLeg.Length; i++)
                    {
                        //Generation de id
                        if (StrFunc.IsEmpty(RepurchaseAgreement.ForwardLeg[i].Id))
                            RepurchaseAgreement.ForwardLeg[i].Id = CciTrade.DataDocument.GenerateId(TradeCustomCaptureInfos.CCst.Prefix_forwardLeg, true);
                        //Alimentation de spotLegReference
                        if (false == RepurchaseAgreement.ForwardLeg[i].SpotLegReferenceSpecified)
                        {
                            RepurchaseAgreement.ForwardLeg[i].SpotLegReference.HRef = RepurchaseAgreement.SpotLeg[i].Id;
                            RepurchaseAgreement.ForwardLeg[i].SpotLegReferenceSpecified = StrFunc.IsFilled(RepurchaseAgreement.ForwardLeg[i].SpotLegReference.HRef);
                        }
                        //Alimentation de assetReference du forwardLeg s'il n'existe pas de zone de saisi pour l'asset
                        if (ArrFunc.IsFilled(CciForwardLeg))
                        {
                            if (null == CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset) // cciSecurityAsset est à null si la zone du saisie du titre n'est pas présent sur l'écran
                            {
                                //Alimentation uniquement lorsque le forward fait référence à un spot
                                string spotLegRefrence = string.Empty;
                                if (RepurchaseAgreement.ForwardLeg[i].SpotLegReferenceSpecified)
                                    spotLegRefrence = RepurchaseAgreement.ForwardLeg[i].SpotLegReference.HRef;
                                //
                                if (StrFunc.IsFilled(spotLegRefrence))
                                {
                                    IDebtSecurityTransaction forwardSecurityTransaction = RepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction;
                                    //20090721 FI Ajout de ce pavé d'initialisation securityAssetSpecified car l'écran Full positionne securityAssetSpecified à false alors que rien est renseigné.
                                    if (forwardSecurityTransaction.SecurityAssetSpecified)
                                        forwardSecurityTransaction.SecurityAssetSpecified = (null != forwardSecurityTransaction.SecurityAsset);
                                    if (forwardSecurityTransaction.SecurityAssetSpecified)
                                        forwardSecurityTransaction.SecurityAssetSpecified = (null != forwardSecurityTransaction.SecurityAsset.SecurityId);
                                    if (forwardSecurityTransaction.SecurityAssetSpecified)
                                        forwardSecurityTransaction.SecurityAssetSpecified = StrFunc.IsFilled(forwardSecurityTransaction.SecurityAsset.SecurityId.Value);
                                    //
                                    if ((false == forwardSecurityTransaction.SecurityAssetReferenceSpecified) && (false == forwardSecurityTransaction.SecurityAssetSpecified))
                                    {
                                        ISecurityLeg spotLeg = (ISecurityLeg)CciTrade.DataDocument.GetObjectById(spotLegRefrence);
                                        ISecurityAsset securityAsset = null;
                                        if (null != spotLeg)
                                        {
                                            //DebtSecurityTransactionContainer debtSecurityTransactionContainer = 
                                            //new DebtSecurityTransactionContainer(spotLeg.debtSecurityTransaction, _cciTrade.DataDocument.dataDocument);
                                            DebtSecurityTransactionContainer debtSecurityTransactionContainer =
                                                new DebtSecurityTransactionContainer(spotLeg.DebtSecurityTransaction, CciTrade.DataDocument);
                                            securityAsset = debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                                        }
                                        if (null != securityAsset)
                                        {
                                            forwardSecurityTransaction.SecurityAssetReferenceSpecified = StrFunc.IsFilled(securityAsset.Id);
                                            forwardSecurityTransaction.SecurityAssetReference.HRef = securityAsset.Id;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //
            #region Synchronize cciStreamGlobal from CciCashStream[0]
            // Permet de conserver en phase le stream global et le stream[0] pour les ccis existants dans les 2 objects
            // Ex il existe debtSecurity_payer et debtSecurity1.payer
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && (CashStreamLength > 0) && CciCashStream[0].IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciCashStream[0].CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        if (CcisBase.Contains(CciStreamGlobal.CciClientId(cciEnum)))
                            CciStreamGlobal.Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion
            //
            #region synchronize CciCashStream[i] from  tradeStreamGlobal (exclude Payer/receiver)
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (cci.HasChanged && CciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    CciStream.CciEnum cciEnum = CciStream.CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        cciEnum = (CciStream.CciEnum)System.Enum.Parse(typeof(CciStream.CciEnum), clientId_Key);
                        //
                        for (int i = 0; i < CashStreamLength; i++)
                            CciCashStream[i].Cci(cciEnum).NewValue = cci.NewValue;
                    }
                }
            }
            #endregion
            //		
            CciStreamGlobal.Dump_ToDocument();
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].Dump_ToDocument();
            //

        }
        #endregion Dump_ToDocument
        #region public override CleanUp
        public override void CleanUp()
        {

            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].CleanUp();
            //                
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].CleanUp();
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].CleanUp();
            //
            //
            // Suppression des spotLegs issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(RepurchaseAgreement.SpotLeg))
            {
                for (int i = RepurchaseAgreement.SpotLeg.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(RepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction.GrossAmount.PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(RepurchaseAgreement, "spotLeg", i);
                }
            }
            //
            // Suppression des ForwardLeg issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(RepurchaseAgreement.ForwardLeg))
            {
                for (int i = RepurchaseAgreement.ForwardLeg.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(RepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction.GrossAmount.PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(RepurchaseAgreement, "forwardLeg", i);
                }
            }
            //
            RepurchaseAgreement.ForwardLegSpecified = ArrFunc.IsFilled(RepurchaseAgreement.ForwardLeg);
            //
            // Suppression des streams issus du paramétrage screen et non alimenté
            if (ArrFunc.IsFilled(RepurchaseAgreement.CashStream))
            {
                for (int i = RepurchaseAgreement.CashStream.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(RepurchaseAgreement.CashStream[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(RepurchaseAgreement, "cashStream", i);
                }
            }

        }
        #endregion CleanUp
        #region public override RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            bool isNoticePeriodEnabled = (RepoDurationEnum.Overnight != RepurchaseAgreement.Duration);
            if (null != Cci(CciEnum.noticePeriod_dayType))
                Cci(CciEnum.noticePeriod_dayType).IsEnabled = isNoticePeriodEnabled;
            if (null != Cci(CciEnum.noticePeriod_periodMultiplier))
                Cci(CciEnum.noticePeriod_periodMultiplier).IsEnabled = isNoticePeriodEnabled;
            if (null != Cci(CciEnum.noticePeriod_period))
                Cci(CciEnum.noticePeriod_period).IsEnabled = isNoticePeriodEnabled;
            //  
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].RefreshCciEnabled();
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].RefreshCciEnabled();
            //
            CciStreamGlobal.RefreshCciEnabled();
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].RefreshCciEnabled();
            //
            // RD 20100402 / terminationMaxDate is not Mandatory for duration = Open
            CcisBase.Set(CciClientId(CciEnum.terminationMaxDate), "IsMandatory", (false == IsOpen));
        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].SetDisplay(pCci);
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].SetDisplay(pCci);
            //
            CciStreamGlobal.SetDisplay(pCci);
            //
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].SetDisplay(pCci);

        }
        #endregion SetDisplay
        #region public override Initialize_Document
        public override void Initialize_Document()
        {

            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].Initialize_Document();
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].Initialize_Document();
            //
            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                for (int i = 0; i < CashStreamLength; i++)
                {
                    if (CciCashStream[i].Cci(CciStream.CciEnum.payer).IsMandatory &&
                        CciCashStream[i].Cci(CciStream.CciEnum.receiver).IsMandatory)
                    {
                        if (StrFunc.IsEmpty(RepurchaseAgreement.CashStream[i].PayerPartyReference.HRef) &&
                            StrFunc.IsEmpty(RepurchaseAgreement.CashStream[i].ReceiverPartyReference.HRef))
                        {
                            if (1 == i)
                            {
                                RepurchaseAgreement.CashStream[i].PayerPartyReference.HRef = RepurchaseAgreement.CashStream[i - 1].ReceiverPartyReference.HRef;
                                RepurchaseAgreement.CashStream[i].ReceiverPartyReference.HRef = RepurchaseAgreement.CashStream[i - 1].PayerPartyReference.HRef;
                            }
                            else
                            {
                                if (StrFunc.IsEmpty(id))
                                {
                                    //id = _cciTrade.GetIdFirstPartyCounterparty();
                                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                                }
                                RepurchaseAgreement.CashStream[i].PayerPartyReference.HRef = id;
                            }
                        }
                    }
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion Initialize_Document
        #endregion

        #region Membres de ITradeCci
        #region public override RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region public override RetSideReceiver
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #region public override GetData
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {

            string ret = string.Empty;
            //
            if (CciCashStream[0].IsCci(CciStream.CciEnum.calculationPeriodDates_effectiveDate, pCci))
                ret = "T";
            if (StrFunc.IsEmpty(pKey))
                pKey = "E";
            //
            //
            if (StrFunc.IsFilled(pKey))
            {
                switch (pKey.ToUpper())
                {
                    case "E":
                        ret = CciCashStream[0].Cci(CciStream.CciEnum.calculationPeriodDates_effectiveDate).NewValue;
                        break;
                }
            }
            //
            return ret;

        }
        #endregion
        #endregion Membres de ITrade

        #region IContainerCciPayerReceiver Membres
        #region public override CciClientIdPayer
        public override string CciClientIdPayer
        {
            get
            {
                return CciSpotLeg[0].cciDebtSecurityTransaction.CciClientIdPayer;
            }
        }
        #endregion
        #region public override CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get
            {
                return CciSpotLeg[0].cciDebtSecurityTransaction.CciClientIdReceiver;
            }
        }
        #endregion
        #region public override SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].SynchronizePayerReceiver(pLastValue, pNewValue);
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].cciDebtSecurityTransaction.SynchronizePayerReceiver(pLastValue, pNewValue);
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].cciDebtSecurityTransaction.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion
        #endregion

        #region IContainerCciGetInfoButton Membres
        #region public override SetButtonReferential
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            for (int i = 0; i < SpotLegLength; i++)
                CciSpotLeg[i].SetButtonReferential(pCci, pCo);
            //
            for (int i = 0; i < ForwardLegLength; i++)
                CciForwardLeg[i].SetButtonReferential(pCci, pCo);
        }
        #endregion SetButtonReferential
        #region public override SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            //
            if (false == isOk)
            {
                for (int i = 0; i < SpotLegLength; i++)
                {
                    isOk = CciSpotLeg[i].SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                    if (isOk)
                        break;
                }
            }
            //
            if (false == isOk)
            {
                for (int i = 0; i < ForwardLegLength; i++)
                {
                    isOk = CciForwardLeg[i].SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                    if (isOk)
                        break;
                }
            }
            //
            #region Button on StreamGlobal
            if (CciStreamGlobal.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepSchedule_initialValue, pCci);
                if (isOk)
                {
                    pCo.Object = "calculationNotional";
                    pCo.Element = "notionalStepSchedule";
                    pCo.OccurenceValue = 1;
                    pCo.CopyTo = "All";
                    pIsSpecified = CciStreamGlobal.IsNotionalStepScheduleStepSpecified;
                    pIsEnabled = true;
                }
                //
                if (false == isOk)
                {
                    isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = CciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }

                if (false == isOk)
                {
                    isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "";
                        pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = CciStreamGlobal.IsKnownAmountScheduleStepSpecified;
                        pIsEnabled = true;
                    }
                }
                //
                if (false == isOk)
                {
                    isOk = CciStreamGlobal.IsCci(CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Object = "calculationNotional";
                        pCo.Element = "notionalStepParameters";
                        pCo.OccurenceValue = 1;
                        pCo.CopyTo = "All";
                        pIsSpecified = CciStreamGlobal.IsNotionalStepParametersSpecified;
                        pIsEnabled = true;
                    }
                }
            }
            #endregion Button on StreamGlobal
            //
            #region  CciStream[i]
            if (!isOk)
            {
                for (int i = 0; i < this.CashStreamLength; i++)
                {
                    if (CciCashStream[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string key = CciCashStream[i].CciContainerKey(pCci.ClientId_WithoutPrefix);
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
                                pIsSpecified = CciCashStream[i].IsNotionalStepScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_notionalSchedule_notionalStepParameters:
                                pCo.Object = "calculationNotional";
                                pCo.Element = "notionalStepParameters";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsNotionalStepParametersSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_knownAmountSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "calculationPeriodAmountKnownAmountSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsKnownAmountScheduleStepSpecified;
                                pIsEnabled = true;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_capRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "capRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsCapRateScheduleSpecified;
                                pIsEnabled = CciCashStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floorRateSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floorRateSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsFloorRateScheduleSpecified;
                                pIsEnabled = CciCashStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_spreadSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "spreadSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsSpreadScheduleSpecified;
                                pIsEnabled = CciCashStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_floatingRateCalculation_floatingRateMultiplierSchedule_initialValue:
                                pCo.Object = "rateFloatingRate";
                                pCo.Element = "floatingRateMultiplierSchedule";
                                pCo.ObjectIndexValue = i;
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsFloatingRateMultiplierScheduleSpecified;
                                pIsEnabled = CciCashStream[i].IsFloatingRateSpecified;
                                break;

                            case CciStream.CciEnum.calculationPeriodAmount_calculation_fixedRateSchedule_initialValue:
                                pCo.Object = string.Empty;
                                pCo.Element = "rateFixedRate";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsFixedRateScheduleSpecified;
                                pIsEnabled = CciCashStream[i].IsFixedRateSpecified;
                                break;
                            case CciStream.CciEnum.paymentDates_offset:
                                pCo.Object = "paymentDates";
                                pCo.Element = "paymentDaysOffset";
                                pCo.OccurenceValue = i + 1;
                                pIsSpecified = CciCashStream[i].IsOffsetSpecified;
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
            return isOk;

        }
        #endregion SetButtonZoom
        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            for (int i = 0; i < ArrFunc.Count(CciCashStream); i++)
                CciCashStream[i].DumpSpecific_ToGUI(pPage);

            //FI 20120625 call DumpSpecific_ToGUI
            for (int i = 0; i < ArrFunc.Count(CciSpotLeg); i++)
                CciSpotLeg[i].DumpSpecific_ToGUI(pPage);

            //FI 20120625 call DumpSpecific_ToGUI
            for (int i = 0; i < ArrFunc.Count(CciForwardLeg); i++)
                CciForwardLeg[i].DumpSpecific_ToGUI(pPage);

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion

        #region methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {

            RepurchaseAgreement = (ISaleAndRepurchaseAgreement)pProduct;
            //
            ICashStream streamGlobal = null;
            if ((null != RepurchaseAgreement) && ArrFunc.IsFilled(RepurchaseAgreement.CashStream))
                streamGlobal = RepurchaseAgreement.CashStream[0];
            CciStreamGlobal = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_cashStream, -1, streamGlobal)
            {
                PrefixId = "cashStream"
            };
            CciCashStream = null;

            RepurchaseAgreementContainer = new SaleAndRepurchaseAgreementContainer(RepurchaseAgreement, CciTrade.DataDocument);

            base.SetProduct(pProduct);

        }
        #endregion

        #region private InitializeCashStream_FromCci
        private void InitializeCashStream_FromCci()
        {

            bool isOk = true;
            int index = -1;
            //
            ArrayList lst = new ArrayList();
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                //
                CciStream cciStreamCurrent = new CciStream(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_cashStream, index + 1, null);
                //
                isOk = CcisBase.Contains(cciStreamCurrent.CciClientId(CciStream.CciEnum.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(RepurchaseAgreement.CashStream) || (index == RepurchaseAgreement.CashStream.Length))
                    {
                        ReflectionTools.AddItemInArray(RepurchaseAgreement, "cashStream", index);

                        if (index > 1)
                        {
                            RepurchaseAgreement.CashStream[index - 1].PayerPartyReference.HRef = RepurchaseAgreement.CashStream[0].PayerPartyReference.HRef;
                            RepurchaseAgreement.CashStream[index - 1].ReceiverPartyReference.HRef = RepurchaseAgreement.CashStream[0].ReceiverPartyReference.HRef;
                        }
                    }
                    //
                    if (StrFunc.IsEmpty(RepurchaseAgreement.CashStream[index].Id))
                        RepurchaseAgreement.CashStream[index].Id = Prefix + TradeCustomCaptureInfos.CCst.Prefix_cashStream + Convert.ToString(index + 1);
                    //					
                    cciStreamCurrent.Irs = RepurchaseAgreement.CashStream[index];
                    cciStreamCurrent.PrefixId = "cashStream";
                    // 
                    lst.Add(cciStreamCurrent);
                }
            }
            CciCashStream = (CciStream[])lst.ToArray(typeof(CciStream));
            //
            #region génération ds chaque Tradestream des ccis du  CciStreamGlobal
            for (int i = 0; i < CcisBase.Count; i++)
            {
                CustomCaptureInfo cci = CcisBase[i];
                if (CciStreamGlobal.IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string clientId_Key = CciStreamGlobal.CciContainerKey(cci.ClientId_WithoutPrefix);
                    if (System.Enum.IsDefined(typeof(CciStream.CciEnum), clientId_Key))
                    {
                        for (int j = 0; j < CashStreamLength; j++)
                            Ccis.CloneGlobalCci(clientId_Key, cci, CciCashStream[j]);
                    }
                }
            }
            #endregion
            //
            CciStreamGlobal.Initialize_FromCci();
            for (int i = 0; i < CashStreamLength; i++)
                CciCashStream[i].Initialize_FromCci();
            //			

        }
        #endregion
        #region private InitializeLeg_FromCci
        #region InitializeSpotLeg_FromCci
        private void InitializeSpotLeg_FromCci()
        {
            CciSpotLeg = InitializeLeg_FromCci(RepurchaseAgreement.SpotLeg, TradeCustomCaptureInfos.CCst.Prefix_spotLeg);
            //
            for (int i = 0; i < SpotLegLength; i++)
            {
                CciSpotLeg[i].Initialize_FromCci();
                //
                if ((null != CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset) &&
                    (null != CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity))
                {
                    CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity.cciStreamGlobal.PrefixId = "spotLegStream";
                    for (int j = 0; j < CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity.DebtSecurityStreamLenght; j++)
                        CciSpotLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity.cciStream[j].PrefixId = "spotLegStream";
                }
            }

        }
        #endregion
        #region InitializeForwardLeg_FromCci
        private void InitializeForwardLeg_FromCci()
        {

            CciForwardLeg = InitializeLeg_FromCci(RepurchaseAgreement.ForwardLeg, TradeCustomCaptureInfos.CCst.Prefix_forwardLeg);
            //
            for (int i = 0; i < ForwardLegLength; i++)
            {
                CciForwardLeg[i].Initialize_FromCci();
                //
                if ((null != CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset) &&
                    (null != CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity))
                {
                    CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity.cciStreamGlobal.PrefixId = "forwardLegStream";
                    for (int j = 0; j < CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity.DebtSecurityStreamLenght; j++)
                        CciForwardLeg[i].cciDebtSecurityTransaction.cciSecurityAsset.cciDebtSecurity.cciStream[j].PrefixId = "forwardLegStream";
                }
            }


        }
        #endregion
        #region InitializeLeg_FromCci
        private CciSecurityLeg[] InitializeLeg_FromCci(ISecurityLeg[] pLeg, string pLegPrefix)
        {

            bool isOk = true;
            int index = 0;
            //
            ArrayList lst = new ArrayList();
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                //
                string cciDebtSecTransPrefix = Prefix + pLegPrefix + index.ToString() + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_debtSecurityTransaction;
                CciDebtSecurityTransaction cciDebtSecurityCurrent
                    = new CciDebtSecurityTransaction(CciTrade, cciDebtSecTransPrefix);
                //
                isOk = CcisBase.Contains(cciDebtSecurityCurrent.cciGrossAmount.CciClientId(CciPayment.CciEnum.payer));
                if (isOk)
                {
                    ISecurityLeg[] currentLeg = pLeg;
                    //
                    if (ArrFunc.IsEmpty(currentLeg) || (index - 1 == currentLeg.Length))
                    {
                        string elementName = string.Empty;
                        if (TradeCustomCaptureInfos.CCst.Prefix_forwardLeg == pLegPrefix)
                            elementName = "forwardLeg";
                        else if (TradeCustomCaptureInfos.CCst.Prefix_spotLeg == pLegPrefix)
                            elementName = "spotLeg";
                        //
                        ReflectionTools.AddItemInArray(RepurchaseAgreement, elementName, index - 1);
                        //
                        ISecurityLeg[] spotLeg = RepurchaseAgreement.SpotLeg;
                        ISecurityLeg[] forwardLeg = RepurchaseAgreement.ForwardLeg;
                        //
                        if (TradeCustomCaptureInfos.CCst.Prefix_forwardLeg == pLegPrefix)
                            currentLeg = forwardLeg;
                        else
                            currentLeg = spotLeg;
                        //
                        if (index > 1)
                        {
                            currentLeg[index - 1].DebtSecurityTransaction.BuyerPartyReference.HRef = currentLeg[0].DebtSecurityTransaction.BuyerPartyReference.HRef;
                            currentLeg[index - 1].DebtSecurityTransaction.SellerPartyReference.HRef = currentLeg[0].DebtSecurityTransaction.SellerPartyReference.HRef;
                        }
                        else if (1 == index && TradeCustomCaptureInfos.CCst.Prefix_forwardLeg == pLegPrefix)
                        {
                            // Inverser le buyer/seller du forwardLeg1 par rapport au spotLeg1
                            forwardLeg[0].DebtSecurityTransaction.BuyerPartyReference.HRef = spotLeg[0].DebtSecurityTransaction.SellerPartyReference.HRef;
                            forwardLeg[0].DebtSecurityTransaction.SellerPartyReference.HRef = spotLeg[0].DebtSecurityTransaction.BuyerPartyReference.HRef;
                        }
                    }
                    //
                    lst.Add(new CciSecurityLeg(CciTrade, Prefix + pLegPrefix, index, currentLeg[index - 1]));
                }
            }
            //
            return (CciSecurityLeg[])lst.ToArray(typeof(CciSecurityLeg));

        }

        #endregion
        #endregion InitializeLeg_FromCci
        #region private SetTerminationDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetTerminationDate(DateTime pEffectiveDate)
        {
            DateTime dtTermination = DateTime.MinValue;

            if (DtFunc.IsDateTimeFilled(pEffectiveDate))
            {
                string[] idA = ArrFunc.ConvertIntArrayToStringArray(CciTrade.GetPartyActorIda());
                IBusinessCenters bcs = CciTrade.CurrentTrade.Product.ProductBase.LoadBusinessCenters(CciTrade.CSCacheOn, null, idA, null, null);
                if (null != bcs)
                {
                    IInterval interval = CciTrade.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, 1);
                    dtTermination = Tools.ApplyAdjustedInterval(CciTrade.CSCacheOn, pEffectiveDate, interval, bcs, CciTrade.DataDocument);
                }
                else
                {
                    dtTermination = pEffectiveDate.AddDays((double)1);
                }
            }
            //
            if (DtFunc.IsDateTimeFilled(dtTermination))
                Cci(CciEnum.terminationMaxDate).NewValue = DtFunc.DateTimeToStringDateISO(dtTermination);

        }
        #endregion SetTerminationDate
        #region private GetCciForwardLegFromSpotId
        /// <summary>
        /// Retourne les forwardLeg qui font reférence à un spotLegId
        /// </summary>
        /// <param name="spotLegId"></param>
        /// <returns></returns>
        private CciSecurityLeg[] GetCciForwardLegFromSpotId(string spotLegId)
        {
            CciSecurityLeg[] ret = null;
            ArrayList al = new ArrayList();
            //
            if (StrFunc.IsFilled(spotLegId))
            {
                for (int j = 0; j < ForwardLegLength; j++)
                {
                    if ((CciForwardLeg[j].securityLeg.SpotLegReferenceSpecified) && (CciForwardLeg[j].securityLeg.SpotLegReference.HRef == spotLegId))
                        al.Add(CciForwardLeg[j]);
                }
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (CciSecurityLeg[])al.ToArray(typeof(CciSecurityLeg));
            //
            return ret;
        }

        #endregion
        #region private GetCciForwardLegFromAssetId
        /// <summary>
        /// Retourne les transaction existant dans les forwardLeg qui font reférence à un asset
        /// </summary>
        /// <param name="spotLegId"></param>
        /// <returns></returns>
        private CciSecurityLeg[] GetCciForwardLegFromAssetId(string pSecurityAssetId)
        {
            CciSecurityLeg[] ret = null;
            ArrayList al = new ArrayList();
            //
            if (StrFunc.IsFilled(pSecurityAssetId))
            {
                for (int j = 0; j < ForwardLegLength; j++)
                {
                    if ((CciForwardLeg[j].securityLeg.DebtSecurityTransaction.SecurityAssetReferenceSpecified) && (
                         CciForwardLeg[j].securityLeg.DebtSecurityTransaction.SecurityAssetReference.HRef == pSecurityAssetId))
                        al.Add(CciForwardLeg[j]);
                }
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (CciSecurityLeg[])al.ToArray(typeof(CciSecurityLeg));
            //
            return ret;

        }
        #endregion



        #endregion
    }
}

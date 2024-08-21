#region using directives
using System;
using System.Collections;
using System.Collections.Generic; //PL 20200109 Add
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Messaging;

using EFS.DPAPI;
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.SpheresService;

using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Enum;

#endregion using directives

// **********************************************************************
// EG 20150515 [20513]
// REFONTE COMPLETE suite à IMPLEMENTATION DE LA GESTION DES BOND OPTIONS
// **********************************************************************

namespace EFS.Common.MQueue
{
    #region TradeActionMQueue
    /// <summary>
    /// 
    /// </summary>
    /// FI 20170306 [22225] Modify
    // EG 20180514 [23812] Report 
    public class TradeActionMQueue : TradeActionBaseMQueue
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("boAbandon", typeof(BO_AbandonMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("boExercise", typeof(BO_ExerciseMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("eqdAbandon", typeof(EQD_AbandonMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("eqdExercise", typeof(EQD_ExerciseMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("fxAbandon", typeof(FX_AbandonMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("fxExercise", typeof(FX_ExerciseMsg), IsNullable = false)]

        [System.Xml.Serialization.XmlElementAttribute("barrier", typeof(BarrierMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("payout", typeof(PayoutMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("rebate", typeof(RebateMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("trigger", typeof(TriggerMsg), IsNullable = false)]

        [System.Xml.Serialization.XmlElementAttribute("fxCustomer", typeof(CustomerSettlementRateMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("removeReplaceTrade", typeof(RemoveTradeMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("removeOnlyTrade", typeof(RemoveTradeEventMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("cancelableProvision", typeof(CancelableProvisionMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("extendibleProvision", typeof(ExtendibleProvisionMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("mandatoryEarlyTerminationProvision", typeof(MandatoryEarlyTerminationProvisionMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("optionalEarlyTerminationProvision", typeof(OptionalEarlyTerminationProvisionMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("stepUpProvision", typeof(StepUpProvisionMsg), IsNullable = false)]
        // FI 20170306 [22225] Add feesCalculationSetting
        // FI 20180328 [23871] Add feesCalculationSettingMode1
        // FI 20180328 [23871] Mod feesCalculationSetting devientFeesCalculationSettingsMode2
        [System.Xml.Serialization.XmlElementAttribute("FeesCalculationSettingsMode2", typeof(FeesCalculationSettingsMode2), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("FeesCalculationSettingsMode1", typeof(FeesCalculationSettingsMode1), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("fxOptionalEarlyTerminationProvision", typeof(FxOptionalEarlyTerminationProvisionMsg), IsNullable = false)]
        public ActionMsgBase[] actionMsgs;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override ActionMsgBase[] ActionMsgs
        {
            get { return actionMsgs; }
            set { actionMsgs = value; }
        }
        #endregion Accessors

        #region Constructors
        public TradeActionMQueue() : base() { }
        public TradeActionMQueue(int pIdE, int pIdE_Event, string pCode)
            : base(pIdE, pIdE_Event, pCode, TradeActionCode.TradeActionCodeEnum.Unknown) { }
        public TradeActionMQueue(int pIdE, int pIdE_Event, string pCode, TradeActionCode.TradeActionCodeEnum pTradeActionCode)
            : base(pIdE, pIdE_Event, pCode, pTradeActionCode) { }
        #endregion Constructors
    }
    #endregion TradeActionMQueue
    #region TradeAdminActionMqueue
    public class TradeAdminActionMQueue : TradeActionBaseMQueue
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("invoicingDetail", typeof(InvoicingDetailMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("removeTradeAdmin", typeof(RemoveTradeMsg), IsNullable = false)]
        [System.Xml.Serialization.XmlElementAttribute("removeOnlyTradeAdmin", typeof(RemoveTradeEventMsg), IsNullable = false)]
        public ActionMsgBase[] actionMsgs;
        #endregion Members
        #region Accessors
        #region EventLines
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public override ActionMsgBase[] ActionMsgs
        {
            get { return actionMsgs; }
            set { actionMsgs = value; }
        }
        #endregion EventLines
        #endregion Accessors

        #region Constructors
        public TradeAdminActionMQueue() { }
        public TradeAdminActionMQueue(int pIdE, int pIdE_Event, string pCode) 
            : base(pIdE, pIdE_Event, pCode, TradeActionCode.TradeActionCodeEnum.Unknown) { }
        public TradeAdminActionMQueue(int pIdE, int pIdE_Event, string pCode, TradeActionCode.TradeActionCodeEnum pTradeActionCode)
            : base(pIdE, pIdE_Event, pCode, pTradeActionCode) { }
        #endregion Constructors
    }
    #endregion TradeAdminActionMqueue
    #region TradeActionBaseMQueue
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeActionMQueue))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TradeAdminActionMQueue))]
    public abstract class TradeActionBaseMQueue
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int idE;
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int idE_Event;
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string code;
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TradeActionCode.TradeActionCodeEnum tradeActionCode;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        // EG 20091231 Add XmlIgnoreAttribute
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public virtual ActionMsgBase[] ActionMsgs
        {
            get { return null; }
            set { ;}
        }
        
        #endregion Accessors
        #region Constructors
        public TradeActionBaseMQueue(){}
        public TradeActionBaseMQueue(int pIdE, int pIdE_Event, string pCode, TradeActionCode.TradeActionCodeEnum pTradeActionCode)
        {
            idE = pIdE;
            idE_Event = pIdE_Event;
            code = pCode;
            tradeActionCode = pTradeActionCode;
        }
        #endregion Constructors
    }
    #endregion TradeActionBaseMQueue

    
    #region ActionMsgBase
    public abstract class ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int idE;
        [System.Xml.Serialization.XmlElementAttribute("actionDate")]
        public DateTime actionDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool noteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("note")]
        public string note;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string keyAction;
        #endregion Members
        #region Constructors
        public ActionMsgBase() { }
        public ActionMsgBase(int pIdE, DateTime pActionDate, string pNote, string pKeyAction)
        {
            idE = pIdE;
            actionDate = pActionDate;
            noteSpecified = StrFunc.IsFilled(pNote);
            note = pNote;
            keyAction = pKeyAction;
        }
        #endregion Constructors
    }
    #endregion ActionMsgBase

    #region AbandonExerciseMsgBase
    public abstract class AbandonExerciseMsgBase : ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("abandonExerciseType")]
        public string abandonExerciseType;
        [System.Xml.Serialization.XmlElementAttribute("valueDate")]
        public DateTime valueDate;
        #endregion

        #region Constructors
        public AbandonExerciseMsgBase() { }
        public AbandonExerciseMsgBase(int pIdE, DateTime pActionDate, DateTime pValueDate, string pAbandonExerciseType, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, pNote, pKeyAction)
        {
            abandonExerciseType = pAbandonExerciseType;
            valueDate = pValueDate;
        }
        #endregion Constructors

    }
    #endregion AbandonExerciseMsgBase
    #region ExerciseMsgBase
    public abstract class ExerciseMsgBase : AbandonExerciseMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fxOptionType")]
        public string fxOptionType;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement")]
        public bool isCashSettlement;
        [System.Xml.Serialization.XmlElementAttribute("isInTheMoney")]
        public bool isInTheMoney;
        #endregion Members
        #region Constructors
        public ExerciseMsgBase() { }
        public ExerciseMsgBase(int pIdE, DateTime pExerciseDate, DateTime pValueDate, string pExerciseType, bool pIsCashSettlement,
            bool pIsInTheMoney, string pFxOptionType, string pNote, string pKeyAction)
            : base(pIdE, pExerciseDate, pValueDate, pExerciseType, pNote, pKeyAction)
        {
            isCashSettlement = pIsCashSettlement;
            isInTheMoney = pIsInTheMoney;
            fxOptionType = pFxOptionType;
        }
        #endregion Constructors
    }
    #endregion ExerciseMsgBase

    #region BarrierMsg
    public class BarrierMsg : BarrierTriggerMsgBase
    {
        #region Constructors
        public BarrierMsg() : base() { }
        public BarrierMsg(int pIdE, string pStatusBarrier, DateTime pTouchDate, decimal pTouchRate, string pNote, string pKeyAction)
            : base(pIdE, pStatusBarrier, pTouchDate, pTouchRate, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion BarrierMsg
    #region BarrierTriggerMsgBase
    public abstract class BarrierTriggerMsgBase : ActionMsgBase
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Cst.StatusTrigger.StatusTriggerEnum status;
        [System.Xml.Serialization.XmlElementAttribute("touchRate")]
        public decimal touchRate;

        #region Constructors
        public BarrierTriggerMsgBase() { }
        public BarrierTriggerMsgBase(int pIdE, string pStatus, DateTime pTouchDate, decimal pTouchRate, string pNote, string pKeyAction)
            : base(pIdE, pTouchDate, pNote, pKeyAction)
        {
            idE = pIdE;
            touchRate = pTouchRate;
            if (System.Enum.IsDefined(typeof(Cst.StatusTrigger.StatusTriggerEnum), pStatus))
                status = (Cst.StatusTrigger.StatusTriggerEnum)System.Enum.Parse(typeof(Cst.StatusTrigger.StatusTriggerEnum), pStatus, true);
            else
                status = Cst.StatusTrigger.StatusTriggerEnum.NA;
        }
        #endregion Constructors
    }
    #endregion BarrierTriggerMsgBase
        
    /// <summary>
    ///  Recalcul des frais pour un nombre restreint de barèmes
    ///  <para>Cette méthode s'applique notamment lorsqu'il y a modification du contract Mutltiplier => Tous les frais issus de barèmes où l'assiette est "Premium" ou "QuantityContractMultipolier" sont recalculés</para>
    ///  <para>Voir TRIM [22225]</para>
    /// </summary>
    // FI 20170306 [22225] add
    public class FeesCalculationSettingsMode2 : ActionMsgBase
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeSheduleSpecified;
        /// <summary>
        /// Permet de réduire le périmètre de calcul à des barêmes spécifiques
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("feeShedule")]
        public FeeSheduleId[] feeShedule;

        /// <summary>
        /// Identifiants du Trade concerné
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public KeyValuePair<int, string> trade;
    }
    
    /// <summary>
    /// Recalcul des frais
    /// <para>Cette méthode s'applique lorsque le service est sollicité via le menu "Recalcul des frais" de l'application web</para>
    /// <para>Voir TRIM [23871] </para>
    /// </summary>
    /// FI 20180328 [23871] Add
    public class FeesCalculationSettingsMode1 : ActionMsgBase
    {
        /// <summary>
        /// Modalité pour le recalcul des frais (ALL|STL|INV)
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("feesCalculationMode")]
        public Cst.FeesCalculationMode mode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeSpecified;
        /// <summary>
        /// Permet de réduire le périmètre de calcul à 1 frais spécifique
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("fee")]
        public FeeId fee;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeSheduleSpecified;
        /// <summary>
        /// Permet de réduire le périmètre de calcul à 1 barême spécifique
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("feeShedule")]
        public FeeSheduleId feeShedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeMatrixSpecified;
        /// <summary>
        /// Permet de réduire le périmètre de calcul à 1 condition spécifique
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("feeMatrix")]
        public FeeMatrixId feeMatrix;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool partyRoleSpecified;
        /// <summary>
        /// Permet de réduire le périmètre de calcul aux payments où le payer ou le receiver a le rôle spcécifié
        /// </summary>
        /// FI 20180424 [23871] Add
        [System.Xml.Serialization.XmlElementAttribute("partyRole")]
        public FixML.v50SP1.Enum.PartyRoleEnum partyRole;

        /// <summary>
        /// 
        /// </summary>
        public FeesCalculationSettingsMode1()
        {
            mode = default;
            fee = new FeeId();
            feeShedule = new FeeSheduleId();
            feeMatrix = new FeeMatrixId();
        }
    }

    /// <summary>
    ///  classe de base qui représente un référentiel de frais
    /// </summary>
    /// FI 20180328 [23871]  Add
    public abstract class FeeRepositoryBase
    {
        /// <summary>
        /// Id non significatif
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        /// <summary>
        /// Id système, non significatif
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        /// <summary>
        /// Identifiant du référentiel
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("id", DataType = "normalizedString")]
        public string identifier;
    }
    
    /// <summary>
    /// Représente un frais
    /// </summary>
    /// FI 20170306 [22225] Add
    /// FI 20180328 [23871] Mod => Heritage de FeeRepositoryBase
    public class FeeId : FeeRepositoryBase
    {
    }

    /// <summary>
    /// Représente un barème de frais
    /// </summary>
    /// FI 20170306 [22225] Add
    /// FI 20180328 [23871] Mod => Heritage de FeeRepositoryBase
    public class FeeSheduleId : FeeRepositoryBase
    {
        public FeeSheduleId() { }
        //PL 20200109 [25099] New
        public FeeSheduleId(KeyValuePair<int, string> pIdentifier)
        {
            OTCmlId = pIdentifier.Key;
            identifier = pIdentifier.Value;
        }
    }

    /// <summary>
    /// Représente une condition
    /// </summary>
    /// FI 20170306 [22225] Add
    /// FI 20180328 [23871] Mod => Heritage de FeeRepositoryBase
    public class FeeMatrixId : FeeRepositoryBase
    {
    }



    #region PayoutMsg
    public class PayoutMsg : PayoutRebateMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("originalAmount")]
        public decimal originalAmount;
        [System.Xml.Serialization.XmlElementAttribute("originalBaseAmount")]
        public decimal originalBaseAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool nbPeriodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("period")]
        public int nbPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool percentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("percentage")]
        public decimal percentage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool payoutRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payoutRate")]
        public decimal payoutRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool gapRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("gapRate")]
        public decimal gapRate;
        #endregion Members
        #region Constructors
        public PayoutMsg() { }
        public PayoutMsg(int pIdE, DateTime pActionDateTime, DateTime pSettlementDate, decimal pOriginalAmount, decimal pAmount,
            decimal pOriginalBaseAmount, decimal pBaseAmount, string pBaseCurrency,
            int pNbPeriod, decimal pPercentage, decimal pPayoutRate, decimal pGapRate,
            string pCurrency,
            string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis, decimal pCustomerRate, decimal pSpotRate, decimal pForwardPoints,
            string pPayer, string pReceiver, string pNote, string pKeyAction)
            : base(pIdE, pActionDateTime, true, pSettlementDate, pAmount, pCurrency, pBaseAmount, pBaseCurrency,
            pCurrency1, pCurrency2, pQuoteBasis, pCustomerRate, pSpotRate, pForwardPoints, pPayer, pReceiver, pNote, pKeyAction)
        {

            originalAmount = pOriginalAmount;
            originalBaseAmount = pOriginalBaseAmount;
            nbPeriodSpecified = (0 < pNbPeriod);
            nbPeriod = pNbPeriod;
            percentageSpecified = (0 < pPercentage);
            percentage = pPercentage;
            payoutRateSpecified = (0 < pPayoutRate);
            payoutRate = pPayoutRate;
            gapRateSpecified = (0 < pGapRate);
            gapRate = pGapRate;
        }
        #endregion Constructors
    }
    #endregion PayoutMsg
    #region PayoutRebateMsg
    public abstract class PayoutRebateMsgBase : ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementDate")]
        public DateTime settlementDate;
        [System.Xml.Serialization.XmlElementAttribute("amount")]
        public decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("currency")]
        public string currency;
        [System.Xml.Serialization.XmlElementAttribute("baseAmount")]
        public decimal baseAmount;
        [System.Xml.Serialization.XmlElementAttribute("baseCurrency")]
        public string baseCurrency;
        [System.Xml.Serialization.XmlElementAttribute("currency1")]
        public string currency1;
        [System.Xml.Serialization.XmlElementAttribute("currency2")]
        public string currency2;
        [System.Xml.Serialization.XmlElementAttribute("quoteBasis")]
        public QuoteBasisEnum quoteBasis;
        [System.Xml.Serialization.XmlElementAttribute("rate")]
        public decimal rate;
        [System.Xml.Serialization.XmlElementAttribute("spotRate")]
        public decimal spotRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forwardPoints")]
        public decimal forwardPoints;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forwardPointsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("buyer")]
        public string payer;
        [System.Xml.Serialization.XmlElementAttribute("seller")]
        public string receiver;
        #endregion Members

        #region Constructors
        public PayoutRebateMsgBase() { }
        public PayoutRebateMsgBase(int pIdE, DateTime pActionDate, bool pIsRebateSpecified, DateTime pSettlementDate, decimal pAmount, string pCurrency, decimal pBaseAmount, string pBaseCurrency,
            string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis, decimal pCustomerRate, decimal pSpotRate, decimal pForwardPoints,
            string pPayer, string pReceiver, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, pNote, pKeyAction)
        {
            if (pIsRebateSpecified)
            {
                settlementDate = pSettlementDate;
                amount = pAmount;
                currency = pCurrency;
                baseAmount = pBaseAmount;
                baseCurrency = pBaseCurrency;
                currency1 = pCurrency1;
                currency2 = pCurrency2;
                quoteBasis = pQuoteBasis;
                rate = pCustomerRate;
                spotRate = pSpotRate;
                spotRateSpecified = (spotRate != 0);
                forwardPoints = pForwardPoints;
                forwardPointsSpecified = (forwardPoints != 0);
                payer = pPayer;
                receiver = pReceiver;
            }
        }
        #endregion Constructors
    }
    #endregion PayoutRebateMsg

    #region RateObservationDateMsg
    public class RateObservationDateMsg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("observedDate")]
        public DateTime observedDate;
        [System.Xml.Serialization.XmlElementAttribute("observedRate")]
        public decimal observedRate;
        [System.Xml.Serialization.XmlElementAttribute("weightingFactor")]
        public decimal weightingFactor;
        [System.Xml.Serialization.XmlElementAttribute("isSecondaryRateSource")]
        public bool isSecondaryRateSource;
        #endregion Members
        #region Constructors
        public RateObservationDateMsg() { }
        public RateObservationDateMsg(DateTime pObservedDate, decimal pObservedRate, decimal pWeightingFactor, bool pIsSecondaryRateSource)
        {
            observedDate = pObservedDate;
            observedRate = pObservedRate;
            weightingFactor = pWeightingFactor;
            isSecondaryRateSource = pIsSecondaryRateSource;
        }
        #endregion Constructors
    }
    #endregion RateObservationDateMsg
    #region RebateMsg
    public class RebateMsg : PayoutRebateMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("callAmount")]
        public PayerReceiverInfo callAmount;
        [System.Xml.Serialization.XmlElementAttribute("putAmount")]
        public PayerReceiverInfo putAmount;
        [System.Xml.Serialization.XmlElementAttribute("payoutStyle")]
        public PayoutEnum payoutStyle;
        #endregion Members
        #region Constructors
        public RebateMsg() { }
        public RebateMsg(int pIdE, DateTime pKnockDate,
            PayerReceiverInfo pCallAmount, PayerReceiverInfo pPutAmount, 
            PayoutEnum pPayoutStyle, bool pIsRebateSpecified, DateTime pSettlementDate, decimal pAmount, string pCurrency, decimal pBaseAmount, string pBaseCurrency,
            string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis, decimal pCustomerRate, decimal pSpotRate, decimal pForwardPoints, 
            string pPayer, string pReceiver, string pNote, string pKeyAction)
            : base(pIdE, pKnockDate, pIsRebateSpecified, pSettlementDate, pAmount, pCurrency, pBaseAmount, pBaseCurrency,
            pCurrency1, pCurrency2, pQuoteBasis, pCustomerRate, pSpotRate, pForwardPoints, pPayer, pReceiver, pNote, pKeyAction)
        {
            callAmount = pCallAmount;
            putAmount = pPutAmount;
            payoutStyle = pPayoutStyle;
        }
        #endregion Constructors
    }
    #endregion RebateMsg

    #region RemoveTradeMsg
    public class RemoveTradeMsg : ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute("idTCancel")]
        public int idTCancel;
        [System.Xml.Serialization.XmlAttributeAttribute("idTCancelIdentifier")]
        public string idTCancelIdentifier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTReplaceSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("idTReplace")]
        public int idTReplace;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idTReplaceIdentifierSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute("idTReplaceIdentifier")]
        public string idTReplaceIdentifier;
        // EG 20160404 Migration vs2013
        //[System.Xml.Serialization.XmlElementAttribute("actionDate")]
        //public DateTime actionDate;
        [System.Xml.Serialization.XmlElementAttribute("removeFutureEvent")]
        public bool removeFutureEvent;
        [System.Xml.Serialization.XmlElementAttribute("genEventsReplace")]
        public bool isEventsReplace;
        // EG 20160404 Migration vs2013
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //public bool noteSpecified;
        //[System.Xml.Serialization.XmlElementAttribute("note")]
        //public string note;
        #region TradeAdmin
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lstLinkedTradeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("linkedTradeId")]
        public DictionaryEntry[] lstLinkedTradeId;
        #endregion TradeAdmin
        #endregion Members
        #region Constructors
        public RemoveTradeMsg() { }
        public RemoveTradeMsg(int pIdTCancel, string pIdTCancelIdentifier, DateTime pDate)
        {
            idTCancel = pIdTCancel;
            idTCancelIdentifier = pIdTCancelIdentifier;
            actionDate = pDate;
        }
        public RemoveTradeMsg(int pIdTCancel, string pIdTCancelIdentifier, DateTime pDate, StringDictionary pLstLinkedTradeId, string pNote)
            : this(pIdTCancel, pIdTCancelIdentifier, pDate)
        {
            lstLinkedTradeIdSpecified = (null != pLstLinkedTradeId) && (0 < pLstLinkedTradeId.Count);
            if (lstLinkedTradeIdSpecified)
            {
                lstLinkedTradeId = new DictionaryEntry[pLstLinkedTradeId.Count];
                int i = 0;
                foreach (DictionaryEntry de in pLstLinkedTradeId)
                {
                    lstLinkedTradeId[i] = new DictionaryEntry(de.Key, de.Value);
                    i++;
                }
            }
            noteSpecified = StrFunc.IsFilled(pNote);
            note = pNote;
        }
        #endregion Constructors
    }
    #endregion RemoveTradeMsg
    #region RemoveTradeEventMsg
    public class RemoveTradeEventMsg : ActionMsgBase
    {
        #region Constructors
        public RemoveTradeEventMsg() { }
        public RemoveTradeEventMsg(int pIdE, DateTime pRemoveDate, string pNote, string pKeyAction) : base(pIdE, pRemoveDate, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion RemoveTradeMsg

    #region TriggerMsg
    public class TriggerMsg : BarrierTriggerMsgBase
    {
        #region Constructors
        public TriggerMsg() : base() { }
        public TriggerMsg(int pIdE, string pStatusTrigger, DateTime pTouchDate, decimal pTouchRate, string pNote, string pKeyAction)
            : base(pIdE, pStatusTrigger, pTouchDate, pTouchRate, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion TriggerMsgBase


    /* BY PRODUCT FAMILY */

    /* FX */
    #region FX_AbandonExerciseMsgBase
    public abstract class FX_AbandonExerciseMsgBase : AbandonExerciseMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("settlementDate")]
        public DateTime settlementDate;
        [System.Xml.Serialization.XmlElementAttribute("callAmount")]
        public PayerReceiverInfo callAmount;
        [System.Xml.Serialization.XmlElementAttribute("putAmount")]
        public PayerReceiverInfo putAmount;
        #endregion

        #region Constructors
        public FX_AbandonExerciseMsgBase() { }
        public FX_AbandonExerciseMsgBase(int pIdE, DateTime pActionDate, DateTime pSettlementDate, string pAbandonExerciseType,
            PayerReceiverInfo pCallAmount, PayerReceiverInfo pPutAmount, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, pSettlementDate, pAbandonExerciseType, pNote, pKeyAction)
        {
            settlementDate = pSettlementDate;
            callAmount = pCallAmount;
            putAmount = pPutAmount;
        }
        #endregion Constructors

    }
    #endregion FX_AbandonExerciseMsgBase
    #region FX_AbandonMsg
    public class FX_AbandonMsg : FX_AbandonExerciseMsgBase
    {
        #region Constructors
        public FX_AbandonMsg() { }
        public FX_AbandonMsg(int pIdE, DateTime pAbandonDate, DateTime pValueDate, string pAbandonType,
            PayerReceiverInfo pCallAmount, PayerReceiverInfo pPutAmount, string pNote, string pKeyAction)
            : base(pIdE, pAbandonDate, pValueDate, pAbandonType, pCallAmount, pPutAmount, pNote, pKeyAction) 
        {
        }
        #endregion Constructors
    }
    #endregion FX_AbandonMsg
    #region FX_ExerciseMsg
    public class FX_ExerciseMsg : FX_AbandonExerciseMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fxOptionType")]
        public string fxOptionType;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement")]
        public bool isCashSettlement;
        [System.Xml.Serialization.XmlElementAttribute("isInTheMoney")]
        public bool isInTheMoney;
        [System.Xml.Serialization.XmlElementAttribute("settlementAmount")]
        public decimal settlementAmount;
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency")]
        public string settlementCurrency;
        [System.Xml.Serialization.XmlElementAttribute("settlementRate")]
        public decimal settlementRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fixingDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixingDate")]
        public DateTime fixingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool primaryIdAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("primaryIdAsset")]
        public int primaryIdAsset;
        [System.Xml.Serialization.XmlElementAttribute("primaryKeyAsset")]
        public KeyAssetFxRate primaryKeyAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool secondaryIdAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryIdAsset")]
        public int secondaryIdAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool secondaryKeyAssetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryKeyAsset")]
        public KeyAssetFxRate secondaryKeyAsset;
        [System.Xml.Serialization.XmlElementAttribute("quoteSide")]
        public string quoteSide;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteTiming")]
        public string quoteTiming;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteTimingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice")]
        public decimal strikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rateObservationDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateObservationDate")]
        public RateObservationDateMsg[] rateObservationDates;
        #endregion Variables
        #region Constructors
        public FX_ExerciseMsg() { }
        public FX_ExerciseMsg(int pIdE, DateTime pExerciseDate, DateTime pValueDate, string pExerciseType, bool pIsCashSettlement,
            bool pIsInTheMoney,
            PayerReceiverInfo pCallAmount, PayerReceiverInfo pPutAmount, 
            decimal pSettlementAmount, string pSettlementCurrency, decimal pSettlementRate, decimal pStrikePrice, DateTime pFixingDate,
            int pPrimaryIdAsset, KeyAssetFxRate pPrimaryKeyAsset, int pSecondaryIdAsset, KeyAssetFxRate pSecondaryKeyAsset,
            RateObservationDateMsg[] pRateObservationDates, string pQuoteSide, string pQuoteTiming,
            string pFxOptionType, string pNote, string pKeyAction)
            : base(pIdE, pExerciseDate, pValueDate, pExerciseType, pCallAmount, pPutAmount, pNote, pKeyAction) 
        {
            isCashSettlement = pIsCashSettlement;
            isInTheMoney = pIsInTheMoney;
            fxOptionType = pFxOptionType;

            settlementAmount = pSettlementAmount;
            settlementCurrency = pSettlementCurrency;
            settlementRate = pSettlementRate;

            fixingDateSpecified = DtFunc.IsDateTimeFilled(pFixingDate);
            fixingDate = pFixingDate;

            primaryIdAssetSpecified = (0 != pPrimaryIdAsset);
            primaryIdAsset = pPrimaryIdAsset;
            primaryKeyAsset = pPrimaryKeyAsset;

            secondaryKeyAssetSpecified = (null != pSecondaryKeyAsset);
            if (secondaryKeyAssetSpecified)
            {
                secondaryIdAssetSpecified = (0 != pSecondaryIdAsset);
                secondaryIdAsset = pSecondaryIdAsset;
                secondaryKeyAsset = pSecondaryKeyAsset;
            }

            quoteSide = pQuoteSide;
            quoteSideSpecified = StrFunc.IsFilled(quoteSide);
            quoteTiming = pQuoteTiming;
            quoteTimingSpecified = StrFunc.IsFilled(quoteTiming);
            strikePrice = pStrikePrice;
            rateObservationDatesSpecified = (null != pRateObservationDates);
            if (rateObservationDatesSpecified)
                rateObservationDates = pRateObservationDates;

        }
        #endregion Constructors
    }
    #endregion FX_ExerciseMsg

    /* BO */

    /* FX */
    #region BO_AbandonExerciseMsgBase
    public abstract class BO_AbandonExerciseMsgBase : AbandonExerciseMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("cashSettlement")]
        public bool isCashSettlement;
        [System.Xml.Serialization.XmlElementAttribute("nbOptions")]
        public PayerReceiverInfo nbOptions;
        [System.Xml.Serialization.XmlElementAttribute("notional")]
        public PayerReceiverInfo notional;
        [System.Xml.Serialization.XmlElementAttribute("entitlement")]
        public PayerReceiverInfo entitlement;
        #endregion

        #region Constructors
        public BO_AbandonExerciseMsgBase() { }
        public BO_AbandonExerciseMsgBase(int pIdE, DateTime pActionDate, DateTime pSettlementDate, string pAbandonExerciseType,
            bool pIsCashSettlement, PayerReceiverInfo pNbOptions, PayerReceiverInfo pNotional, PayerReceiverInfo pEntitlement, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, pSettlementDate, pAbandonExerciseType, pNote, pKeyAction)
        {
            isCashSettlement = pIsCashSettlement;
            nbOptions = pNbOptions;
            notional = pNotional;
            entitlement = pEntitlement;
        }
        #endregion Constructors

    }
    #endregion FX_AbandonExerciseMsgBase
    #region BO_AbandonMsg
    public class BO_AbandonMsg : BO_AbandonExerciseMsgBase
    {
        #region Constructors
        public BO_AbandonMsg() { }
        public BO_AbandonMsg(int pIdE, DateTime pAbandonDate, DateTime pValueDate, string pAbandonType, bool pIsCashSettlement, 
            PayerReceiverInfo pNbOptions, PayerReceiverInfo pNotional, PayerReceiverInfo pEntitlement, string pNote, string pKeyAction)
            : base(pIdE, pAbandonDate, pValueDate, pAbandonType, pIsCashSettlement, pNbOptions, pNotional, pEntitlement, pNote, pKeyAction)
        {
        }
        #endregion Constructors
    }
    #endregion BO_AbandonMsg
    #region BO_ExerciseMsg
    public class BO_ExerciseMsg : BO_AbandonExerciseMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("quoteSide")]
        public string quoteSide;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteSideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteTiming")]
        public string quoteTiming;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool quoteTimingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice")]
        public decimal strikePrice;
        [System.Xml.Serialization.XmlElementAttribute("bondPayment")]
        public PayerReceiverInfo bondPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bondPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlement")]
        public PayerReceiverInfo settlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool settlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterest")]
        public PayerReceiverInfo accruedInterest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool accruedInterestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("idAsset")]
        public int idAsset;
        [System.Xml.Serialization.XmlElementAttribute("settlementRate")]
        public decimal settlementRate;
        #endregion Members
        #region Constructors
        public BO_ExerciseMsg() { }
        public BO_ExerciseMsg(int pIdE, DateTime pExerciseDate, DateTime pValueDate, string pExerciseType, bool pIsCashSettlement, 
            PayerReceiverInfo pNbOptions, PayerReceiverInfo pNotional, PayerReceiverInfo pEntitlement,
            PayerReceiverInfo pBondPayment, PayerReceiverInfo pAccruedInterest, PayerReceiverInfo pSettlement, int pIdAsset, decimal pSettlementRate, decimal pStrikePrice,
            string pQuoteSide, string pQuoteTiming, string pNote, string pKeyAction)
            : base(pIdE, pExerciseDate, pValueDate, pExerciseType, pIsCashSettlement, pNbOptions, pNotional, pEntitlement, pNote, pKeyAction)
        {
            bondPayment = pBondPayment;
            bondPaymentSpecified = (null != pBondPayment);
            settlement = pSettlement;
            settlementSpecified = (null != pSettlement);
            accruedInterest = pAccruedInterest;
            accruedInterestSpecified = (null != pAccruedInterest);
            idAsset = pIdAsset;
            
            settlementRate = pSettlementRate;
            quoteSide = pQuoteSide;
            quoteSideSpecified = StrFunc.IsFilled(quoteSide);
            quoteTiming = pQuoteTiming;
            quoteTimingSpecified = StrFunc.IsFilled(quoteTiming);
            strikePrice = pStrikePrice;

        }
        #endregion Constructors
    }
    #endregion BO_ExerciseMsg

    /* EQD */
    #region EQD_AbandonMsg
    public class EQD_AbandonMsg : AbandonExerciseMsgBase
    {
        #region Constructors
        public EQD_AbandonMsg() { }
        public EQD_AbandonMsg(int pIdE, DateTime pAbandonDate, DateTime pValueDate, string pAbandonType, string pNote, string pKeyAction)
            : base(pIdE, pAbandonDate, pValueDate, pAbandonType, pNote, pKeyAction)
        {
        }
        #endregion Constructors
    }
    #endregion EQD_AbandonMsg
    #region EQD_ExerciseMsg
    public class EQD_ExerciseMsg : AbandonExerciseMsgBase
    {
        #region Members
        #endregion Members
        #region Constructors
        public EQD_ExerciseMsg() { }
        public EQD_ExerciseMsg(int pIdE, DateTime pExerciseDate, DateTime pValueDate, string pExerciseType, string pNote, string pKeyAction)
            : base(pIdE, pExerciseDate, pValueDate, pExerciseType, pNote, pKeyAction)
        {
        }
        #endregion Constructors
    }
    #endregion EQD_ExerciseMsg


    /* PROVISIONS */

    #region ProvisionMsgBase
    public abstract class ProvisionMsgBase : ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("eventCode")]
        public string eventCode;
        [System.Xml.Serialization.XmlElementAttribute("provisionDate")]
        public DateTime provisionDate;
        [System.Xml.Serialization.XmlElementAttribute("exerciseType")]
        public string exerciseType;
        [System.Xml.Serialization.XmlElementAttribute("notionalProvision")]
        public NotionalProvisionMsg[] notionalProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notionalProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeProvision")]
        public FeeProvisionMsg feeProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeProvisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementProvision")]
        public CashSettlementProvisionMsg cashSettlementProvision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool cashSettlementProvisionSpecified;
        #endregion Members
        #region Constructors
        public ProvisionMsgBase() { }
        public ProvisionMsgBase(int pIdE, DateTime pActionDate, string pEventCode, string pExerciseType, DateTime pValueDate,
            NotionalProvisionMsg[] pNotionalProvision, FeeProvisionMsg pFeeProvision,
            CashSettlementProvisionMsg pCashSettlementProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, pNote, pKeyAction)
        {
            eventCode = pEventCode;
            provisionDate = pValueDate;
            exerciseType = pExerciseType;
            notionalProvision = pNotionalProvision;
            notionalProvisionSpecified = (null != notionalProvision);
            feeProvision = pFeeProvision;
            feeProvisionSpecified = (null != feeProvision);
            cashSettlementProvision = pCashSettlementProvision;
            cashSettlementProvisionSpecified = (null != cashSettlementProvision);
        }
        #endregion Constructors
    }
    #endregion ProvisionMsgBase

    #region CancelableProvisionMsg
    public class CancelableProvisionMsg : ProvisionMsgBase
    {
        #region Constructors
        public CancelableProvisionMsg() : base() { }
        public CancelableProvisionMsg(int pIdE, DateTime pActionDate, string pExerciseType, DateTime pValueDate,
            NotionalProvisionMsg[] pNotionalProvision, FeeProvisionMsg pFeeProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, EventCodeFunc.ExerciseCancelable, pExerciseType, pValueDate, pNotionalProvision, pFeeProvision, null, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion CancelableProvisionMsg
    #region CashSettlementProvisionMsg
    // EG 20150706 [21021] Add idA_PayerSpecified|idA_ReceiverSpecified
    public class CashSettlementProvisionMsg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerParty")]
        public int idA_Payer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_PayerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payerBook")]
        public int idB_Payer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idB_PayerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("receiverParty")]
        public int idA_Receiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_ReceiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("receiverBook")]
        public int idB_Receiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idB_ReceiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementPaymentDate")]
        public DateTime cashSettlementPaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementAmount")]
        public decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementCurrency")]
        public string currency;
        #endregion Members
        #region Constructors
        public CashSettlementProvisionMsg() { }
        #endregion Constructors
    }
    #endregion CashSettlementProvisionMsg
    #region CustomerSettlementRateMsg
    public class CustomerSettlementRateMsg : ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fixingRate")]
        public decimal fixingRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixingDate")]
        public DateTime valueDate;
        #endregion Members

        #region Constructors
        public CustomerSettlementRateMsg() { }
        public CustomerSettlementRateMsg(int pIdE, DateTime pActionDate, DateTime pValueDate, decimal pFixingRate, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, pNote, pKeyAction)
        {
            valueDateSpecified = DtFunc.IsDateTimeFilled(pValueDate);
            valueDate = pValueDate;
            fixingRate = pFixingRate;
        }
        #endregion Constructors
    }
    #endregion CustomerSettlementRateEvent

    #region ExtendibleProvisionMsg
    public class ExtendibleProvisionMsg : ProvisionMsgBase
    {
        #region Constructors
        public ExtendibleProvisionMsg() : base() { }
        public ExtendibleProvisionMsg(int pIdE, DateTime pActionDate, string pExerciseType, DateTime pValueDate,
            NotionalProvisionMsg[] pNotionalProvision, FeeProvisionMsg pFeeProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, EventCodeFunc.ExerciseExtendible, pExerciseType, pValueDate, pNotionalProvision, pFeeProvision, null, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion ExtendibleProvisionMsg

    #region FeeProvisionMsg
    // EG 20150706 [21021] Add idA_PayerSpecified|idA_ReceiverSpecified
    public class FeeProvisionMsg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerParty")]
        public int idA_Payer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_PayerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payerBook")]
        public int idB_Payer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idB_PayerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("receiverParty")]
        public int idA_Receiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idA_ReceiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("receiverBook")]
        public int idB_Receiver;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool idB_ReceiverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feePaymentDate")]
        public DateTime feePaymentDate;
        [System.Xml.Serialization.XmlElementAttribute("feeAmount")]
        public decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("feeCurrency")]
        public string currency;
        [System.Xml.Serialization.XmlElementAttribute("feeNotionalReference")]
        public decimal notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notionalReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeCurrencyReference")]
        public string currencyReference;
        [System.Xml.Serialization.XmlElementAttribute("feeRate")]
        public decimal feeRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool feeRateSpecified;
        #endregion Members
        #region Constructors
        public FeeProvisionMsg() { }
        #endregion Constructors
    }
    #endregion FeeProvisionMsg

    #region MandatoryEarlyTerminationProvisionMsg
    public class MandatoryEarlyTerminationProvisionMsg : ProvisionMsgBase
    {
        #region Constructors
        public MandatoryEarlyTerminationProvisionMsg() : base() { }
        public MandatoryEarlyTerminationProvisionMsg(int pIdE, DateTime pActionDate, string pExerciseType, DateTime pValueDate,
            NotionalProvisionMsg[] pNotionalProvision, CashSettlementProvisionMsg pCashSettlementProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, EventCodeFunc.ExerciseMandatoryEarlyTermination, pExerciseType, pValueDate, pNotionalProvision, null, pCashSettlementProvision, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion MandatoryEarlyTerminationProvisionMsg
    
    #region NotionalProvisionMsg
    // EG 20180514 [23812] Report
    public class NotionalProvisionMsg
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("instrumentNo")]
        public string instrumentNo;
        [System.Xml.Serialization.XmlElementAttribute("streamNo")]
        public string streamNo;
        [System.Xml.Serialization.XmlElementAttribute("startPeriod")]
        public DateTime dtStartPeriod;
        [System.Xml.Serialization.XmlElementAttribute("endPeriod")]
        public DateTime dtEndPeriod;
        [System.Xml.Serialization.XmlElementAttribute("originalAmount")]
        public decimal originalAmount;
        [System.Xml.Serialization.XmlElementAttribute("originalCurrency")]
        public string originalCurrency;
        [System.Xml.Serialization.XmlElementAttribute("provisionAmount")]
        public decimal provisionAmount;
        [System.Xml.Serialization.XmlElementAttribute("provisionCurrency")]
        public string provisionCurrency;
        #endregion Members
        #region Constructors
        public NotionalProvisionMsg() { }
        // EG 20180514 [23812] Report
        public NotionalProvisionMsg(decimal pAmount, string pCurrency, DateTime pDate)
        {
            originalAmount = pAmount;
            originalCurrency = pCurrency;
            provisionAmount = pAmount;
            provisionCurrency = pCurrency;
            instrumentNo = "1";
            streamNo = "1";
            dtStartPeriod = pDate;
            dtEndPeriod = pDate;
        }

        #endregion Constructors
    }
    #endregion NotionalProvisionMsg

    #region OptionalEarlyTerminationProvisionMsg
    public class OptionalEarlyTerminationProvisionMsg : ProvisionMsgBase
    {
        #region Constructors
        public OptionalEarlyTerminationProvisionMsg() : base() { }
        public OptionalEarlyTerminationProvisionMsg(int pIdE, DateTime pActionDate, string pExerciseType, DateTime pValueDate,
            NotionalProvisionMsg[] pNotionalProvision, FeeProvisionMsg pFeeProvision,
            CashSettlementProvisionMsg pCashSettlementProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, EventCodeFunc.ExerciseOptionalEarlyTermination, pExerciseType, pValueDate, pNotionalProvision, pFeeProvision, pCashSettlementProvision, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion OptionalEarlyTerminationProvisionMsg

    #region StepUpProvisionMsg
    public class StepUpProvisionMsg : ProvisionMsgBase
    {
        #region Constructors
        public StepUpProvisionMsg() : base() { }
        public StepUpProvisionMsg(int pIdE, DateTime pActionDate, string pExerciseType, DateTime pValueDate,
            NotionalProvisionMsg[] pNotionalProvision, FeeProvisionMsg pFeeProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, EventCodeFunc.ExerciseCancelable, pExerciseType, pValueDate, pNotionalProvision, pFeeProvision, null, pNote, pKeyAction) { }
        #endregion Constructors
    }
    #endregion StepUpProvisionMsg

    /* FX PROVISION */

    #region FxOptionalEarlyTerminationProvisionMsg
    // EG 20180514 [23812] Report
    public class FxOptionalEarlyTerminationProvisionMsg : ProvisionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("callAmount")]
        public PayerReceiverInfo callAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool callAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("putAmount")]
        public PayerReceiverInfo putAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool putAmountSpecified;
        #endregion Members

        #region Constructors
        public FxOptionalEarlyTerminationProvisionMsg() : base() { }
        public FxOptionalEarlyTerminationProvisionMsg(int pIdE, DateTime pActionDate, string pExerciseType, DateTime pValueDate,
            PayerReceiverInfo pCallAmount, PayerReceiverInfo pPutAmount,
            CashSettlementProvisionMsg pCashSettlementProvision, string pNote, string pKeyAction)
            : base(pIdE, pActionDate, EventCodeFunc.ExerciseOptionalEarlyTermination, pExerciseType, pValueDate, null, null,
            pCashSettlementProvision, pNote, pKeyAction)
        {
            callAmount = pCallAmount;
            callAmountSpecified = (null != callAmount);
            putAmount = pPutAmount;
            putAmountSpecified = (null != putAmount);
        }
        #endregion Constructors
    }
    #endregion FxOptionalEarlyTerminationProvisionMsg

    /* INVOICING */

    #region InvoicingDetailMsg
    public class InvoicingDetailMsg : ActionMsgBase
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("idE_Source")]
        public int idE_Source;
        [System.Xml.Serialization.XmlElementAttribute("amount")]
        public decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("originalAmount")]
        public decimal originalAmount;
        #endregion Members
        #region Members
        #endregion Members
        #region Constructors
        public InvoicingDetailMsg() { }
        public InvoicingDetailMsg(int pIdE, DateTime pActionDateTime, int pIdE_Source, decimal pOriginalAmount, decimal pAmount, string pNote, string pKeyAction)
            : base(pIdE, pActionDateTime, pNote, pKeyAction)
        {
            idE_Source = pIdE_Source;
            originalAmount = pOriginalAmount;
            amount = pAmount;
        }
        #endregion Constructors
    }
    #endregion InvoicingDetailMsg

}

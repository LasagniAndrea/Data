#region using directives
using EFS.ACommon;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using FpML.Enum;
#endregion using directives

namespace EfsML.Enum.Tools
{
    #region CompoundingFrequencyPeriod
    public class CompoundingFrequencyPeriod
    {
        #region Members
        private const int Daily = 365;
        private const int Weekly = 52;
        private const int Monthly = 12;
        private const int Quaterly = 4;
        private const int SemiAnnually = 2;
        private const int Annual = 1;
        private const int InFine = 1;
        private const int Continuous = 0;
        #endregion Members
        //        
        #region Constructors
        public CompoundingFrequencyPeriod() { }
        #endregion Constructors
        //
        #region Methods
        #region GetPeriod
        /// <summary>
        /// Obtient 1 pour {Infine ou annual},2 pour {SemiAnnually}, 4 pour {Quaterly}, 12 pour {Monthly}, 52 pour {Weekly}, 365 pour {Daily}
        /// <para>Obtient -1 dans les autres cas </para>
        /// </summary>
        /// <param name="pCompoundingFrequency"></param>
        /// <returns></returns>
        public static int GetPeriod(CompoundingFrequencyEnum pCompoundingFrequency)
        {
            int ret = -1;
            switch (pCompoundingFrequency)
            {
                case CompoundingFrequencyEnum.Daily:
                    ret = Daily;
                    break;
                case CompoundingFrequencyEnum.Weekly:
                    ret = Weekly;
                    break;
                case CompoundingFrequencyEnum.Monthly:
                    ret = Monthly;
                    break;
                case CompoundingFrequencyEnum.Quaterly:
                    ret = Quaterly;
                    break;
                case CompoundingFrequencyEnum.SemiAnnually:
                    ret = SemiAnnually;
                    break;
                case CompoundingFrequencyEnum.Annual:
                    ret = Annual;
                    break;
                case CompoundingFrequencyEnum.InFine:
                    ret = InFine;
                    break;
                case CompoundingFrequencyEnum.Continuous:
                    ret = Continuous;
                    break;
            }
            return ret;
        }
        #endregion GetPeriod
        #endregion Methods
    }
    #endregion

    #region PeriodTofrequency
    public sealed class PeriodTofrequency
    {
        //20090514 RD/PL 
        /// <summary>
        /// Translate a Period to a Frequency
        /// NB: Frequency do not have 12M but only 1Y (Annual)
        /// </summary>
        /// <param name="opPeriodMultiplier"></param>
        /// <param name="opPeriod"></param>
        public static bool Translate(ref string opPeriodMultiplier, ref string opPeriod)
        {
            bool isReverse = false;
            return Translate(ref opPeriodMultiplier, ref opPeriod, isReverse);
        }
        public static bool Translate(ref string opPeriodMultiplier, ref string opPeriod, bool pIsReverse)
        {
            bool isTranslated = false;
            if ((opPeriodMultiplier == "12") && (opPeriod == PeriodEnum.M.ToString()))
            {
                isTranslated = true;
                opPeriodMultiplier = "1";
                opPeriod = PeriodEnum.Y.ToString();
            }
            else if ((opPeriodMultiplier == "7") && (opPeriod == PeriodEnum.D.ToString()))
            {
                isTranslated = true;
                opPeriodMultiplier = "1";
                opPeriod = PeriodEnum.W.ToString();
            }
            else if (pIsReverse)
            {
                if ((opPeriodMultiplier == "1") && (opPeriod == PeriodEnum.Y.ToString()))
                {
                    isTranslated = true;
                    opPeriodMultiplier = "12";
                    opPeriod = PeriodEnum.M.ToString();
                }
                else if ((opPeriodMultiplier == "1") && (opPeriod == PeriodEnum.W.ToString()))
                {
                    isTranslated = true;
                    opPeriodMultiplier = "7";
                    opPeriod = PeriodEnum.D.ToString();
                }
            }
            return isTranslated;
        }
    }
    #endregion PeriodTofrequency

    /// <summary>
    /// 
    /// </summary>
    public sealed class EnumToString
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRollConvention"></param>
        /// <returns></returns>
        ///20080326 PL Note: Méthode un peu "simpliste" mais rapidement mis en place...
        public static string RollConvention(RollConventionEnum pRollConvention)
        {
            string ret = pRollConvention.ToString();
            if (ret.StartsWith("DAY"))
                ret = ret.Remove(0, 3);
            //
            return ret;
        }
    }

    #region EventClassFunc
    public sealed class EventClassFunc
    {
        #region Administration Level Trade Events
        #region AccrualCompound
        public static string AccrualCompound { get { return EventClassEnum.CMP.ToString(); } }
        #endregion AccrualCompound
        #region AccrualLinear
        public static string AccrualLinear { get { return EventClassEnum.LIN.ToString(); } }
        #endregion AccrualLinear
        #region AccrualProrata
        public static string AccrualProrata { get { return EventClassEnum.PRO.ToString(); } }
        #endregion AccrualProrata
        #region Black_Scholes
        public static string Black_Scholes { get { return EventClassEnum.B_S.ToString(); } }
        #endregion Black_Scholes
        #region Compounded
        public static string Compounded { get { return EventClassEnum.CMP.ToString(); } }
        #endregion Compounded
        #region CoxRossRubinstein
        public static string CoxRossRubinstein { get { return EventClassEnum.CRR.ToString(); } }
        #endregion CoxRossRubinstein
        #region DailyClosingAccounting
        public static string DailyClosingAccounting { get { return EventClassEnum.CLA.ToString(); } }
        #endregion DailyClosingAccounting
        #region ExDate
        // EG 20150907 [21317] New
        public static string ExDate { get { return EventClassEnum.EXD.ToString(); } }
        #endregion ExDate

        #region ForwardRate
        public static string ForwardRate { get { return EventClassEnum.FWR.ToString(); } }
        #endregion ForwardRate
        #region ForwardRateProjection
        public static string ForwardRateProjection { get { return EventClassEnum.FRP.ToString(); } }
        #endregion ForwardRateProjection
        #region Garman_Kolhagen
        public static string Garman_Kolhagen { get { return EventClassEnum.G_K.ToString(); } }
        #endregion Garman_Kolhagen
        #region GroupLevel
        public static string GroupLevel { get { return EventClassEnum.GRP.ToString(); } }
        #endregion GroupLevel
        #region Linear
        public static string Linear { get { return EventClassEnum.LIN.ToString(); } }
        #endregion Linear
        #region LinearDepreciation
        public static string LinearDepreciation { get { return EventClassEnum.LDP.ToString(); } }
        #endregion LinearDepreciation
        #region LinearDepreciation
        public static string LinearDepRemaining { get { return EventClassEnum.LDR.ToString(); } }
        #endregion LinearDepreciation
        #region Prorata
        public static string Prorata { get { return EventClassEnum.PRO.ToString(); } }
        #endregion Prorata
        #region RecordDate
        // EG 20150907 [21317] New
        public static string RecordDate { get { return EventClassEnum.RCD.ToString(); } }
        #endregion RecordDate
        #region RemoveEvent
        public static string RemoveEvent { get { return EventClassEnum.RMV.ToString(); } }
        #endregion RemoveEvent
        #region SpotRate
        public static string SpotRate { get { return EventClassEnum.SPR.ToString(); } }
        #endregion SpotRate
        #region ValueDate
        // EG 20150616 [XXXXX] New
        public static string ValueDate { get { return EventClassEnum.VAL.ToString(); } }
        #endregion ValueDate

        #region IsAccrualCompound
        public static bool IsAccrualCompound(string pEventClass) { return (pEventClass == AccrualCompound); }
        #endregion IsAccrualCompound
        #region IsAccrualLinear
        public static bool IsAccrualLinear(string pEventClass) { return (pEventClass == AccrualLinear); }
        #endregion IsAccrualLinear
        #region IsAccrualProrata
        public static bool IsAccrualProrata(string pEventClass) { return (pEventClass == AccrualProrata); }
        #endregion IsAccrualProrata
        #region IsBlack_Scholes
        public static bool IsBlack_Scholes(string pEventClass) { return (pEventClass == Black_Scholes); }
        #endregion IsBlack_Scholes
        #region IsCompounded
        public static bool IsCompounded(string pEventClass) { return (pEventClass == Compounded); }
        #endregion IsCompound
        #region IsCoxRossRubinstein
        public static bool IsCoxRossRubinstein(string pEventClass) { return (pEventClass == CoxRossRubinstein); }
        #endregion IsCoxRossRubinstein
        #region IsDailyClosingAccounting
        public static bool IsDailyClosingAccounting(string pEventClass) { return (pEventClass == DailyClosingAccounting); }
        #endregion IsDailyClosingAccounting
        #region IsExDate
        // EG 20150907 [21317] New
        public static bool IsExDate(string pEventClass) { return (pEventClass == ExDate); }
        #endregion IsExDate
        #region IsForwardRate
        public static bool IsForwardRate(string pEventClass) { return (pEventClass == ForwardRate); }
        #endregion IsForwardRate
        #region IsGarman_Kolhagen
        public static bool IsGarman_Kolhagen(string pEventClass) { return (pEventClass == Garman_Kolhagen); }
        #endregion IsGarman_Kolhagen
        #region IsLinear
        public static bool IsLinear(string pEventClass) { return (pEventClass == Linear); }
        #endregion IsLinear
        #region IsLinearDepreciation
        public static bool IsLinearDepreciation(string pEventClass) { return (pEventClass == LinearDepreciation); }
        #endregion IsLinearDepreciation
        #region IsLinearDepreciation
        public static bool IsLinearDepRemaining(string pEventClass) { return (pEventClass == LinearDepRemaining); }
        #endregion IsLinearDepreciation
        #region IsSpotRate
        public static bool IsSpotRate(string pEventClass) { return (pEventClass == SpotRate); }
        #endregion IsSpotRate
        #region IsGroupLevel
        public static bool IsGroupLevel(string pEventClass) { return (pEventClass == GroupLevel); }
        #endregion IsGroupLevel
        #region IsRecordDate
        // EG 20150907 [21317] New
        public static bool IsRecordDate(string pEventClass) { return (pEventClass == RecordDate); }
        #endregion IsRecordDate
        #region IsRemoveEvent
        public static bool IsRemoveEvent(string pEventClass) { return (pEventClass == RemoveEvent); }
        #endregion IsRemoveEvent
        #region IsValueDate
        // EG 20150616 [XXXXX] New
        public static bool IsValueDate(string pEventType) { return (pEventType == ValueDate); }
        #endregion IsValueDate
        #endregion Administration Level Trade Events

        #region Calculation Level Trade Events
        #region Average
        public static string Average { get { return EventClassEnum.AVG.ToString(); } }
        #endregion Average
        #region CashSettlementPaymentDate
        public static string CashSettlementPaymentDate { get { return EventClassEnum.CSP.ToString(); } }
        #endregion CashSettlementPaymentDate
        #region CashSettlementValuationDate
        public static string CashSettlementValuationDate { get { return EventClassEnum.CSV.ToString(); } }
        #endregion CashSettlementValuationDate
        #region CompoundedRate
        public static string CompoundedRate { get { return EventClassEnum.CMP.ToString(); } }
        #endregion CompoundedRate

        #region ElectionSettlementDate
        public static string ElectionSettlementDate { get { return EventClassEnum.ESD.ToString(); } }
        #endregion ElectionSettlementDate
        #region Fixing
        public static string Fixing { get { return EventClassEnum.FXG.ToString(); } }
        #endregion Fixing
        #region RelevantUnderlyingDate
        public static string RelevantUnderlyingDate { get { return EventClassEnum.RUD.ToString(); } }
        #endregion RelevantUnderlyingDate

        #region IsAverage
        public static bool IsAverage(string pEventClass) { return (pEventClass == Average); }
        #endregion IsAverage
        #region IsCashSettlementPaymentDate
        public static bool IsCashSettlementPaymentDate(string pEventClass) { return (pEventClass == CashSettlementPaymentDate); }
        #endregion IsCashSettlementPaymentDate
        #region IsCashSettlementValuationDate
        public static bool IsCashSettlementValuationDate(string pEventClass) { return (pEventClass == CashSettlementValuationDate); }
        #endregion IsCashSettlementValuationDate
        #region IsCompoundedRate
        public static bool IsCompoundedRate(string pEventClass) { return (pEventClass == CompoundedRate); }
        #endregion IsCompoundedRate
        #region IsElectionSettlementDate
        public static bool IsElectionSettlementDate(string pEventClass) { return (pEventClass == ElectionSettlementDate); }
        #endregion IsElectionSettlementDate
        #region IsFixing
        public static bool IsFixing(string pEventClass) { return (pEventClass == Fixing); }
        #endregion IsFixing
        #region IsRelevantUnderlyingDate
        public static bool IsRelevantUnderlyingDate(string pEventClass) { return (pEventClass == RelevantUnderlyingDate); }
        #endregion IsRelevantUnderlyingDate
        #endregion Calculation Level Trade Events

        #region Description Level Trade Events
        #region BarrierKnock
        public static string BarrierKnock { get { return EventClassEnum.KNK.ToString(); } }
        #endregion BarrierKnock
        #region CashSettlement
        public static string CashSettlement { get { return EventClassEnum.CSH.ToString(); } }
        #endregion CashSettlement
        #region Date
        public static string Date { get { return EventClassEnum.DAT.ToString(); } }
        #endregion Date

        #region ElectionSettlement
        public static string ElectionSettlement { get { return EventClassEnum.ELE.ToString(); } }
        #endregion ElectionSettlement
        #region ExerciseCancelable
        public static string ExerciseCancelable { get { return EventClassEnum.EXC.ToString(); } }
        #endregion ExerciseCancelable
        #region ExerciseExtendible
        public static string ExerciseExtendible { get { return EventClassEnum.EXX.ToString(); } }
        #endregion ExerciseExtendible
        #region ExerciseMandatoryEarlyTermination
        public static string ExerciseMandatoryEarlyTermination { get { return EventClassEnum.EXM.ToString(); } }
        #endregion ExerciseMandatoryEarlyTermination
        #region ExerciseOptionalEarlyTermination
        public static string ExerciseOptionalEarlyTermination { get { return EventClassEnum.EXO.ToString(); } }
        #endregion ExerciseOptionalEarlyTermination
        #region ExerciseStepUp
        public static string ExerciseStepUp { get { return EventClassEnum.EXS.ToString(); } }
        #endregion ExerciseStepUp
        #region PhysicalSettlement
        public static string PhysicalSettlement { get { return EventClassEnum.PHY.ToString(); } }
        #endregion PhysicalSettlement
        #region Rate
        public static string Rate { get { return EventClassEnum.RAT.ToString(); } }
        #endregion Rate

        #region TriggerTouch
        public static string TriggerTouch { get { return EventClassEnum.TCH.ToString(); } }
        #endregion TriggerTouch

        #region IsBarrierKnock
        public static bool IsBarrierKnock(string pEventClass) { return (pEventClass == BarrierKnock); }
        #endregion IsBarrierKnock
        #region IsCashSettlement
        public static bool IsCashSettlement(string pEventClass) { return (pEventClass == CashSettlement); }
        #endregion IsCashSettlement
        #region IsDate
        public static bool IsDate(string pEventClass) { return (pEventClass == Date); }
        #endregion IsDate
        #region IsElectionSettlement
        public static bool IsElectionSettlement(string pEventClass) { return (pEventClass == ElectionSettlement); }
        #endregion IsElectionSettlement
        #region IsExerciseProvision
        public static bool IsExerciseProvision(string pEventClass)
        {
            return IsExerciseCancelable(pEventClass) || IsExerciseMandatoryEarlyTermination(pEventClass) ||
                   IsExerciseExtendible(pEventClass) || IsExerciseOptionalEarlyTermination(pEventClass) ||
                   IsExerciseStepUp(pEventClass);
        }
        #endregion IsExerciseProvision
        #region IsExerciseCancelable
        public static bool IsExerciseCancelable(string pEventClass) { return (pEventClass == ExerciseCancelable); }
        #endregion IsExerciseCancelable
        #region IsExerciseExtendible
        public static bool IsExerciseExtendible(string pEventClass) { return (pEventClass == ExerciseExtendible); }
        #endregion IsExerciseExtendible
        #region IsExerciseMandatoryEarlyTermination
        public static bool IsExerciseMandatoryEarlyTermination(string pEventClass) { return (pEventClass == ExerciseMandatoryEarlyTermination); }
        #endregion IsExerciseMandatoryEarlyTermination
        #region IsExerciseOptionalEarlyTermination
        public static bool IsExerciseOptionalEarlyTermination(string pEventClass) { return (pEventClass == ExerciseOptionalEarlyTermination); }
        #endregion IsExerciseOptionalEarlyTermination
        #region IsExerciseStepUp
        public static bool IsExerciseStepUp(string pEventClass) { return (pEventClass == ExerciseStepUp); }
        #endregion IsExerciseStepUp
        #region IsPhysicalSettlement
        public static bool IsPhysicalSettlement(string pEventClass) { return (pEventClass == PhysicalSettlement); }
        #endregion IsPhysicalSettlement
        #region IsRate
        public static bool IsRate(string pEventClass) { return (pEventClass == Rate); }
        #endregion IsRate
        #region IsTriggerTouch
        public static bool IsTriggerTouch(string pEventClass) { return (pEventClass == TriggerTouch); }
        #endregion IsTriggerTouch
        #endregion Description Level Trade Events

        #region Group Level Trade Events
        #region DeliveryDelay
        public static string DeliveryDelay { get { return EventClassEnum.DLY.ToString(); } }
        #endregion DeliveryDelay
        #region DeliveryMessage
        public static string DeliveryMessage { get { return EventClassEnum.DLM.ToString(); } }
        #endregion DeliveryMessage
        #region Invoiced
        public static string Invoiced { get { return EventClassEnum.INV.ToString(); } }
        #endregion Invoiced
        #region PreSettlement
        public static string PreSettlement { get { return EventClassEnum.PRS.ToString(); } }
        #endregion PreSettlement
        #region Recognition
        public static string Recognition { get { return EventClassEnum.REC.ToString(); } }
        #endregion Recognition
        #region Settlement
        public static string Settlement { get { return EventClassEnum.STL.ToString(); } }
        #endregion Settlement
        #region SettlementMessage
        public static string SettlementMessage { get { return EventClassEnum.STM.ToString(); } }
        #endregion SettlementMessage

        #region IsDeliveryMessage
        public static bool IsDeliveryMessage(string pEventClass) { return (pEventClass == DeliveryMessage); }
        #endregion IsDeliveryMessage
        #region IsInvoiced
        public static bool IsInvoiced(string pEventClass) { return (pEventClass == Invoiced); }
        #endregion IsInvoiced
        #region IsPreSettlement
        public static bool IsPreSettlement(string pEventClass) { return (pEventClass == PreSettlement); }
        #endregion IsPreSettlement
        #region IsRecognition
        public static bool IsRecognition(string pEventClass) { return (pEventClass == Recognition); }
        #endregion IsRecognition
        #region IsSettlement
        public static bool IsSettlement(string pEventClass) { return (pEventClass == Settlement); }
        #endregion IsSettlement
        #region IsSettlementMessage
        public static bool IsSettlementMessage(string pEventClass) { return (pEventClass == SettlementMessage); }
        #endregion IsSettlementMessage
        #region IsSettlementOrDeliveryMessage
        public static bool IsSettlementOrDeliveryMessage(string pEventClass)
        {
            return (IsSettlement(pEventClass) || IsDeliveryMessage(pEventClass));
        }
        #endregion IsSettlementOrDeliveryMessage
        #endregion Group Level Trade Events

        #region Others
        #region RateCutOffDate
        public static string RateCutOffDate { get { return EventClassEnum.CUT.ToString(); } }
        #endregion RateCutOffDate

        #region IsFixingOrRateCutOffDate
        public static bool IsFixingOrRateCutOffDate(string pEventClass)
        {
            return IsFixing(pEventClass) || IsRateCutOffDate(pEventClass);
        }
        #endregion IsFixingOrRateCutOffDate
        #region IsRateCutOffDate
        public static bool IsRateCutOffDate(string pEventClass) { return (pEventClass == RateCutOffDate); }
        #endregion IsRateCutOffDate
        #endregion Others
    }
    #endregion EventClassFunc

    #region EventCodeFunc
    /// <summary>
    /// 
    /// </summary>
    // EG 20190613 [24683] Use OffSettingClosingPosition
    // EG 20190926 [Maturity Redemption] Upd
    public sealed class EventCodeFunc
    {
        #region Administration Level Trade Events

        /// <summary>
        /// Obtient ADP
        /// </summary>
        public static string AdditionalPayment { get { return EventCodeEnum.ADP.ToString(); } }

        #region LinkedFutureClosing
        /// <summary>
        /// Obtient LFC
        /// </summary>
        public static string LinkedFutureClosing { get { return EventCodeEnum.LFC.ToString(); } }
        #endregion LinkedFutureClosing
        #region LinkedFutureIntraday
        /// <summary>
        /// Obtient LFI
        /// </summary>
        public static string LinkedFutureIntraday { get { return EventCodeEnum.LFI.ToString(); } }
        #endregion LinkedFutureIntraday
        #region LinkedOptionClosing
        /// <summary>
        /// Obtient LOC
        /// </summary>
        public static string LinkedOptionClosing { get { return EventCodeEnum.LOC.ToString(); } }
        #endregion LinkedOptionClosing
        #region LinkedOptionIntraday
        /// <summary>
        /// Obtient LOI
        /// </summary>
        public static string LinkedOptionIntraday { get { return EventCodeEnum.LOI.ToString(); } }
        #endregion LinkedOptionIntraday
        #region LinkedProductClosing
        /// <summary>
        /// Obtient LPC
        /// </summary>
        public static string LinkedProductClosing { get { return EventCodeEnum.LPC.ToString(); } }
        #endregion LinkedProductClosing
        #region LinkedProductIntraday
        /// <summary>
        /// Obtient LPI
        /// </summary>
        public static string LinkedProductIntraday { get { return EventCodeEnum.LPI.ToString(); } }
        #endregion LinkedProductIntraday
        #region LinkedProductPayment
        /// <summary>
        /// Obtient LPP
        /// </summary>
        public static string LinkedProductPayment { get { return EventCodeEnum.LPP.ToString(); } }
        #endregion LinkedProductPayment
        #region DailyClosing
        /// <summary>
        /// Obtient CLO
        /// </summary>
        public static string DailyClosing { get { return EventCodeEnum.CLO.ToString(); } }
        #endregion DailyClosing
        #region Delivery
        /// <summary>
        /// Obtient DLV
        /// </summary>
        public static string Delivery { get { return EventCodeEnum.DLV.ToString(); } }
        #endregion Delivery
        #region DepreciableAmount
        /// <summary>
        /// Obtient DEA
        /// </summary>
        public static string DepreciableAmount { get { return EventCodeEnum.DEA.ToString(); } }
        #endregion DepreciableAmount
        #region Intermediary
        /// <summary>
        /// Obtient INT
        /// </summary>
        public static string Intermediary { get { return EventCodeEnum.INT.ToString(); } }
        #endregion IntermediaryPayment
        #region OtherPartyPayment
        /// <summary>
        /// Obtient OPP
        /// </summary>
        public static string OtherPartyPayment { get { return EventCodeEnum.OPP.ToString(); } }
        #endregion OtherPartyPayment
        #region Start
        /// <summary>
        /// Obtient STA
        /// </summary>
        public static string Start { get { return EventCodeEnum.STA.ToString(); } }
        #endregion Start
        #region StartIntermediary
        /// <summary>
        /// Obtient STI
        /// </summary>
        public static string StartIntermediary { get { return EventCodeEnum.STI.ToString(); } }
        #endregion StartIntermediary
        #region Termination
        /// <summary>
        /// Obtient TER
        /// </summary>
        public static string Termination { get { return EventCodeEnum.TER.ToString(); } }
        #endregion Termination
        #region TerminationIntermediary
        /// <summary>
        /// Obtient TEI
        /// </summary>
        public static string TerminationIntermediary { get { return EventCodeEnum.TEI.ToString(); } }
        #endregion TerminationIntermediary

        #region IsAdditionalPayment
        /// <summary>
        /// Obtient true si EVENTCODE = ADP
        /// </summary>
        public static bool IsAdditionalPayment(string pEventCode) { return (pEventCode == AdditionalPayment); }
        #endregion IsAdditionalPayment
        #region IsLinkedFuture
        /// <summary>
        /// Obtient true si EVENTCODE = LFC || LFI
        /// </summary>
        public static bool IsLinkedFuture(string pEventCode) { return (IsLinkedFutureClosing(pEventCode) || IsLinkedFutureIntraday(pEventCode)); }
        #endregion IsLinkedFuture
        #region IsLinkedFutureClosing
        /// <summary>
        /// Obtient true si EVENTCODE = LFC
        /// </summary>
        public static bool IsLinkedFutureClosing(string pEventCode) { return (pEventCode == LinkedFutureClosing); }
        #endregion IsLinkedFutureClosing
        #region IsLinkedFutureIntraday
        /// <summary>
        /// Obtient true si EVENTCODE = LFI
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsLinkedFutureIntraday(string pEventCode) { return (pEventCode == LinkedFutureIntraday); }
        #endregion IsLinkedFutureIntraday
        #region IsLinkedOption
        /// <summary>
        /// Obtient true si EVENTCODE = LOC || LOI
        /// </summary>
        public static bool IsLinkedOption(string pEventCode) { return (IsLinkedOptionClosing(pEventCode) || IsLinkedOptionIntraday(pEventCode)); }
        #endregion IsLinkedOption
        #region IsLinkedOptionClosing
        /// <summary>
        /// Obtient true si EVENTCODE = LOC
        /// </summary>
        public static bool IsLinkedOptionClosing(string pEventCode) { return (pEventCode == LinkedOptionClosing); }
        #endregion IsLinkedOptionClosing
        #region IsLinkedOptionIntraday
        /// <summary>
        /// Obtient true si EVENTCODE = LOI
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsLinkedOptionIntraday(string pEventCode) { return (pEventCode == LinkedOptionIntraday); }
        #endregion IsLinkedOptionIntraday
        #region IsLinkedProductClosing
        /// <summary>
        /// Obtient true si EVENTCODE = LPC
        /// </summary>
        public static bool IsLinkedProductClosing(string pEventCode) { return (pEventCode == LinkedProductClosing); }
        #endregion IsLinkedProductClosing
        #region IsLinkedProductIntraday
        /// <summary>
        /// Obtient true si EVENTCODE = LPI
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsLinkedProductIntraday(string pEventCode) { return (pEventCode == LinkedProductIntraday); }
        #endregion IsLinkedProductIntraday
        #region IsLinkedProductPayment
        /// <summary>
        /// Obtient true si EVENTCODE = LPP
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsLinkedProductPayment(string pEventCode) { return (pEventCode == LinkedProductPayment); }
        #endregion IsLinkedProductPayment

        #region IsDailyClosing
        /// <summary>
        /// Obtient true si EVENTCODE = CLO
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsDailyClosing(string pEventCode) { return (pEventCode == DailyClosing); }
        #endregion IsDailyClosing
        #region IsDailyClosing
        /// <summary>
        /// Obtient true si EVENTCODE = DLV
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsDelivery(string pEventCode) { return (pEventCode == Delivery); }
        #endregion IsDailyClosing
        #region IsDepreciableAmount
        public static bool IsDepreciableAmount(string pEventCode) { return (pEventCode == DepreciableAmount); }
        #endregion IsDepreciableAmount
        #region IsDenouement
        public static bool IsDenouement(string pEventCode)
        {
            return IsExercise(pEventCode) || IsAbandon(pEventCode) || IsAssignment(pEventCode);
        }
        #endregion IsDenouement

        #region IsIntermediary
        public static bool IsIntermediary(string pEventCode) { return (pEventCode == Intermediary); }
        #endregion IsIntermediary
        #region IsMaturityOffsetting
        // EG 20190926 [Maturity Redemption] Upd
        public static bool IsMaturityOffsetting(string pEventCode) { return (pEventCode == MaturityOffsettingFuture) || (pEventCode == MaturityRedemptionOffsettingDebtSecurity); }
        #endregion IsMaturityOffsetting
        #region IsOffsetting
        public static bool IsOffsetting(string pEventCode) { return (pEventCode == Offsetting); }
        #endregion IsOffsetting
        #region IsOtherPartyPayment
        public static bool IsOtherPartyPayment(string pEventCode) { return (pEventCode == OtherPartyPayment); }
        #endregion IsOtherPartyPayment
        #region IsSafeKeepingPayment
        // EG 20150708 [21103] New
        public static bool IsSafeKeepingPayment(string pEventCode) { return (pEventCode == SafeKeepingPayment); }
        #endregion IsSafeKeepingPayment


        #region IsPositionCancelation
        public static bool IsPositionCancelation(string pEventCode) { return (pEventCode == PositionCancelation); }
        #endregion IsPositionCancelation

        #region IsPositionTransfer
        public static bool IsPositionTransfer(string pEventCode) { return (pEventCode == PositionTransfer); }
        #endregion IsPositionTransfer

        #region IsOffSettingClosingPosition
        public static bool IsOffSettingClosingPosition(string pEventCode) { return (pEventCode == OffSettingClosingPosition); }
        #endregion IsOffSettingClosingPosition


        #region IsStart
        public static bool IsStart(string pEventCode) { return (pEventCode == Start); }
        #endregion IsStart
        #region IsStartIntermediary
        public static bool IsStartIntermediary(string pEventCode) { return (pEventCode == StartIntermediary); }
        #endregion IsStartIntermediary
        #region IsStartTermination
        public static bool IsStartTermination(string pEventCode)
        {
            return IsStart(pEventCode) || IsStartIntermediary(pEventCode) || IsTermination(pEventCode) || IsTerminationIntermediary(pEventCode);
        }
        #endregion IsStartTermination
        #region IsTermination
        public static bool IsTermination(string pEventCode) { return (pEventCode == Termination); }
        #endregion IsTermination
        #region IsTerminationIntermediary
        public static bool IsTerminationIntermediary(string pEventCode) { return (pEventCode == TerminationIntermediary); }
        #endregion IsTerminationIntermediary
        #endregion Administration Level Trade Events

        #region Calculation Level Trade Events
        #region CalculationPeriod
        /// <summary>
        /// Retourne PER
        /// </summary>
        public static string CalculationPeriod { get { return EventCodeEnum.PER.ToString(); } }
        #endregion CalculationPeriod
        #region Reset
        /// <summary>
        /// Retourne RES
        /// </summary>
        public static string Reset { get { return EventCodeEnum.RES.ToString(); } }
        #endregion Reset
        #region SafeKeepingPayment
        /// <summary>
        /// Retourne SKP
        /// </summary>
        // EG 20150708 [SKP] New
        public static string SafeKeepingPayment { get { return EventCodeEnum.SKP.ToString(); } }
        #endregion SafeKeepingPayment
        
        #region SelfAverage
        /// <summary>
        /// Retourne SAV
        /// </summary>
        public static string SelfAverage { get { return EventCodeEnum.SAV.ToString(); } }
        #endregion SelfAverage
        #region SelfReset
        /// <summary>
        /// Retourne SRT
        /// </summary>
        public static string SelfReset { get { return EventCodeEnum.SRT.ToString(); } }
        #endregion SelfReset

        #region IsCalculationPeriod
        /// <summary>
        /// Retorune true si EVENTCODE = PER
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsCalculationPeriod(string pEventCode) { return (pEventCode == CalculationPeriod); }
        #endregion IsCalculationPeriod
        #region IsReset
        /// <summary>
        /// Retourne true si EVENTCODE = RES
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsReset(string pEventCode) { return (pEventCode == Reset); }
        #endregion IsReset
        #region IsResetSelf
        public static bool IsResetSelf(string pEventCode)
        {
            return IsReset(pEventCode) || IsSelfAverage(pEventCode) || IsSelfReset(pEventCode);
        }
        #endregion IsResetSelf
        #region IsResetPlus
        public static bool IsResetPlus(string pEventCode)
        {
            return IsReset(pEventCode) || IsSelfAverage(pEventCode);
        }
        #endregion IsResetPlus
        #region IsSelfAverage
        /// <summary>
        /// Retourne true si EVENTCODE = SAV
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsSelfAverage(string pEventCode) { return (pEventCode == SelfAverage); }
        #endregion IsSelfAverage
        #region IsSelfReset
        /// <summary>
        /// Retourne true si EVENTCODE = STR
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsSelfReset(string pEventCode) { return (pEventCode == SelfReset); }
        #endregion IsSelfReset
        #endregion Calculation Level Trade Events

        #region Description Level Trade Events
        #region Asian
        public static string Asian { get { return EventCodeEnum.ASI.ToString(); } }
        #endregion Asian
        #region Barrier
        public static string Barrier { get { return EventCodeEnum.BAR.ToString(); } }
        #endregion Barrier
        #region EffectiveRate
        public static string EffectiveRate { get { return EventCodeEnum.EFR.ToString(); } }
        #endregion EffectiveRate
        #region ExerciseDates
        public static string ExerciseDates { get { return EventCodeEnum.EXD.ToString(); } }
        #endregion ExerciseDates
        #region LookBackOption
        public static string LookBackOption { get { return EventCodeEnum.LBK.ToString(); } }
        #endregion LookBackOption
        #region Trigger
        public static string Trigger { get { return EventCodeEnum.TRG.ToString(); } }
        #endregion Trigger

        #region IsAdditionalInvoiceDates
        public static bool IsAdditionalInvoiceDates(string pEventCode) { return (pEventCode == AdditionalInvoiceDates); }
        #endregion IsAdditionalInvoiceDates
        #region IsBarrier
        public static bool IsBarrier(string pEventCode) { return (pEventCode == Barrier); }
        #endregion IsBarrier
        #region IsClearing
        public static bool IsClearing(string pEventCode)
        {
            return (IsOffsetting(pEventCode) || IsPositionCancelation(pEventCode) || IsMaturityOffsetting(pEventCode) ||
                IsAbandonAssignmentExercise(pEventCode) || IsAutomaticAbandonAssignmentExercise(pEventCode) ||
                IsCascading(pEventCode) || IsShifting(pEventCode) || IsOffSettingCorporateAction(pEventCode));
        }
        #endregion IsClearing

        #region IsCreditNoteDates
        public static bool IsCreditNoteDates(string pEventCode) { return (pEventCode == CreditNoteDates); }
        #endregion IsCreditNoteDates

        #region IsEffectiveRate
        public static bool IsEffectiveRate(string pEventCode) { return (pEventCode == EffectiveRate); }
        #endregion IsEffectiveRate
        #region IsExerciseDates
        public static bool IsExerciseDates(string pEventCode) { return (pEventCode == ExerciseDates); }
        #endregion IsExerciseDates
        #region IsTrigger
        public static bool IsTrigger(string pEventCode) { return (pEventCode == Trigger); }
        #endregion IsTrigger
        #endregion Description Level Trade Events

        #region Group Level Trade Events
        #region Abandon
        public static string Abandon { get { return EventCodeEnum.ABN.ToString(); } }
        #endregion Abandon
        #region AdditionalInvoiceDates
        public static string AdditionalInvoiceDates { get { return EventCodeEnum.AID.ToString(); } }
        #endregion AdditionalInvoiceDates
        #region AllocatedInvoiceDates
        public static string AllocatedInvoiceDates { get { return EventCodeEnum.ALD.ToString(); } }
        #endregion AllocatedInvoiceDates
        #region AmericanAverageOption
        public static string AmericanAverageOption { get { return EventCodeEnum.AAO.ToString(); } }
        #endregion AmericanAverageOption
        #region AmericanBarrierOption
        public static string AmericanBarrierOption { get { return EventCodeEnum.ABO.ToString(); } }
        #endregion AmericanBarrierOption
        #region AmericanDigitalOption
        public static string AmericanDigitalOption { get { return EventCodeEnum.ADO.ToString(); } }
        #endregion AmericanDigitalOption
        #region AmericanEquityOption
        public static string AmericanEquityOption { get { return EventCodeEnum.AEO.ToString(); } }
        #endregion AmericanEquityOption
        #region AmericanBondOption
        public static string AmericanBondOption { get { return EventCodeEnum.BOA.ToString(); } }
        #endregion AmericanBondOption
        #region AmericanExchangeTradedDerivativeOption
        // EG 20091221 New
        public static string AmericanExchangeTradedDerivativeOption { get { return EventCodeEnum.AED.ToString(); } }
        #endregion AmericanExchangeTradedDerivativeOption
        #region AmericanExercise
        public static string AmericanExercise { get { return EventCodeEnum.EXE.ToString(); } }
        #endregion AmericanExercise
        #region AmericanSimpleOption
        public static string AmericanSimpleOption { get { return EventCodeEnum.ASO.ToString(); } }
        #endregion AmericanSimpleOption
        #region AmericanSwaption
        public static string AmericanSwaption { get { return EventCodeEnum.ASW.ToString(); } }
        #endregion AmericanSwaption
        #region Assignment
        public static string Assignment { get { return EventCodeEnum.ASS.ToString(); } }
        #endregion Assignment
        #region AutomaticAbandon
        public static string AutomaticAbandon { get { return EventCodeEnum.AAB.ToString(); } }
        #endregion AutomaticAbandon
        #region AssignmentDates
        public static string AssignmentDates { get { return EventCodeEnum.ASD.ToString(); } }
        #endregion AssignmentDates
        #region AutomaticAssignment
        public static string AutomaticAssignment { get { return EventCodeEnum.AAS.ToString(); } }
        #endregion AutomaticAssignment

        #region BermudaAverageOption
        public static string BermudaAverageOption { get { return EventCodeEnum.BAO.ToString(); } }
        #endregion BermudaAverageOption
        #region BermudaBarrierOption
        public static string BermudaBarrierOption { get { return EventCodeEnum.BBO.ToString(); } }
        #endregion BermudaBarrierOption
        #region BermudaEquityOption
        public static string BermudaEquityOption { get { return EventCodeEnum.BEO.ToString(); } }
        #endregion BermudaEquityOption
        #region BermudaBondOption
        public static string BermudaBondOption { get { return EventCodeEnum.BOB.ToString(); } }
        #endregion BermudaBondOption
        #region BermudaSimpleOption
        public static string BermudaSimpleOption { get { return EventCodeEnum.BSO.ToString(); } }
        #endregion BermudaSimpleOption
        #region BermudaSwaption
        public static string BermudaSwaption { get { return EventCodeEnum.BSW.ToString(); } }
        #endregion BermudaSwaption
        #region BulletPayment
        public static string BulletPayment { get { return EventCodeEnum.BUL.ToString(); } }
        #endregion BulletPayment
        #region BuyAndSellBack
        public static string BuyAndSellBack { get { return EventCodeEnum.BSB.ToString(); } }
        #endregion BuyAndSellBack

        #region CapFloor
        public static string CapFloor { get { return EventCodeEnum.CFL.ToString(); } }
        #endregion CapFloor

        #region Cascading
        public static string Cascading { get { return EventCodeEnum.CAS.ToString(); } }
        #endregion Cascading

        #region CashBalanceStream
        public static string CashBalanceStream { get { return EventCodeEnum.CBS.ToString(); } }
        #endregion CashBalanceStream
        #region CashFlowConstituent
        public static string CashFlowConstituent { get { return EventCodeEnum.CFC.ToString(); } }
        #endregion CashFlowConstituent
        #region CashBalanceInterestStream
        // PM 20120824 [18058] Add CashBalanceInterest
        public static string CashBalanceInterest { get { return EventCodeEnum.CIS.ToString(); } }
        #endregion CashBalanceInterest

        #region Commodity
        public static string FinancialLeg { get { return EventCodeEnum.FIL.ToString(); } }
        public static string PhysicalLeg { get { return EventCodeEnum.PHL.ToString(); } }
        #endregion Commodity

        #region OffSettingCorporateAction
        public static string OffSettingCorporateAction { get { return EventCodeEnum.OCA.ToString(); } }
        #endregion OffSettingCorporateAction

        #region OffSettingClosingPosition
        public static string OffSettingClosingPosition { get { return EventCodeEnum.OCP.ToString(); } }
        #endregion OffSettingClosingPosition

        #region CreditNoteDates
        public static string CreditNoteDates { get { return EventCodeEnum.CND.ToString(); } }
        #endregion CreditNoteDates

        #region DebtSecurityStream
        public static string DebtSecurityStream { get { return EventCodeEnum.DSS.ToString(); } }
        #endregion DebtSecurityStream
        #region DebtSecurityTransaction
        public static string DebtSecurityTransaction { get { return EventCodeEnum.DST.ToString(); } }
        #endregion DebtSecurityTransaction

        #region ExchangeCashBalanceStream
        public static string ExchangeCashBalanceStream { get { return EventCodeEnum.ECS.ToString(); } }
        #endregion ExchangeCashBalanceStream

        #region EquitySecurityTransaction
        public static string EquitySecurityTransaction { get { return EventCodeEnum.EST.ToString(); } }
        #endregion EquitySecurityTransaction
        #region EuropeanAverageOption
        public static string EuropeanAverageOption { get { return EventCodeEnum.EAO.ToString(); } }
        #endregion EuropeanAverageOption
        #region EuropeanBarrierOption
        public static string EuropeanBarrierOption { get { return EventCodeEnum.EBO.ToString(); } }
        #endregion EuropeanBarrierOption
        #region EuropeanDigitalOption
        public static string EuropeanDigitalOption { get { return EventCodeEnum.EDO.ToString(); } }
        #endregion EuropeanDigitalOption
        #region EuropeanEquityOption
        public static string EuropeanEquityOption { get { return EventCodeEnum.EEO.ToString(); } }
        #endregion EuropeanEquityOption
        #region EuropeanBondOption
        public static string EuropeanBondOption { get { return EventCodeEnum.BOE.ToString(); } }
        #endregion EuropeanBondOption
        #region EuropeanExchangeTradedDerivativeOption
        // EG 20091221 New
        public static string EuropeanExchangeTradedDerivativeOption { get { return EventCodeEnum.EED.ToString(); } }
        #endregion EuropeanExchangeTradedDerivativeOption
        #region EuropeanSimpleOption
        public static string EuropeanSimpleOption { get { return EventCodeEnum.ESO.ToString(); } }
        #endregion EuropeanSimpleOption
        #region EuropeanSwaption
        public static string EuropeanSwaption { get { return EventCodeEnum.ESW.ToString(); } }
        #endregion EuropeanSwaption
        #region Exercise
        public static string Exercise { get { return EventCodeEnum.EXE.ToString(); } }
        #endregion Exercise
        #region ExerciseCancelable
        public static string ExerciseCancelable { get { return EventCodeEnum.EXC.ToString(); } }
        #endregion ExerciseCancelable
        #region ExerciseExtendible
        public static string ExerciseExtendible { get { return EventCodeEnum.EXX.ToString(); } }
        #endregion ExerciseExtendible
        #region ExerciseMandatoryEarlyTermination
        public static string ExerciseMandatoryEarlyTermination { get { return EventCodeEnum.EXM.ToString(); } }
        #endregion ExerciseMandatoryEarlyTermination
        #region ExerciseOptionalEarlyTermination
        public static string ExerciseOptionalEarlyTermination { get { return EventCodeEnum.EXO.ToString(); } }
        #endregion ExerciseOptionalEarlyTermination
        #region ExerciseStepUp
        public static string ExerciseStepUp { get { return EventCodeEnum.EXS.ToString(); } }
        #endregion ExerciseStepUp

        #region ForwardRateAgreement
        public static string ForwardRateAgreement { get { return EventCodeEnum.FRA.ToString(); } }
        #endregion ForwardRateAgreement
        #region FutureExchangeTradedDerivative
        /// <summary>
        /// EVENTCODE = 'FED'
        /// </summary>
        public static string FutureExchangeTradedDerivative { get { return EventCodeEnum.FED.ToString(); } }
        #endregion FutureExchangeTradedDerivative
        #region FuturesStyleOption
        /// <summary>
        /// Obtient FSO
        /// </summary>
        public static string FuturesStyleOption { get { return EventCodeEnum.FSO.ToString(); } }
        #endregion FuturesStyleOption
        #region FxSpot
        public static string FxSpot { get { return EventCodeEnum.FXS.ToString(); } }
        #endregion FxSpot
        #region FxForward
        public static string FxForward { get { return EventCodeEnum.FXF.ToString(); } }
        #endregion FxForward

        #region InitialValuation
        public static string InitialValuation { get { return EventCodeEnum.INI.ToString(); } }
        #endregion InitialValuation
        #region InterestRateSwap
        public static string InterestRateSwap { get { return EventCodeEnum.IRS.ToString(); } }
        #endregion InterestRateSwap
        #region InvoiceAmended
        public static string InvoiceAmended { get { return EventCodeEnum.IAM.ToString(); } }
        #endregion InvoiceAmended
        #region InvoiceMaster
        public static string InvoiceMaster { get { return EventCodeEnum.IMS.ToString(); } }
        #endregion InvoiceMaster
        #region InvoiceMasterBase
        public static string InvoiceMasterBase { get { return EventCodeEnum.IMB.ToString(); } }
        #endregion InvoiceMasterBase
        #region InvoiceSettlement
        public static string InvoiceSettlement { get { return EventCodeEnum.IST.ToString(); } }
        #endregion InvoiceSettlement
        #region InvoicingDates
        public static string InvoicingDates { get { return EventCodeEnum.INV.ToString(); } }
        #endregion InvoicingDates

        #region LoanDeposit
        public static string LoanDeposit { get { return EventCodeEnum.L_D.ToString(); } }
        #endregion LoanDeposit
        #region Leg
        public static string Leg { get { return EventCodeEnum.LEG.ToString(); } }
        #endregion Leg

        #region MaturityOffsettingFuture
        // EG 20190926 [Maturity Redemption] Upd
        public static string MaturityOffsettingFuture { get { return EventCodeEnum.MOF.ToString(); } }
        #endregion MaturityOffsettingFuture

        #region MaturityRedemptionOffsettingDebtSecurity
        // EG 20190926 [Maturity Redemption] New
        public static string MaturityRedemptionOffsettingDebtSecurity { get { return EventCodeEnum.MOD.ToString(); } }
        #endregion MaturityRedemptionOffsettingDebtSecurity

        #region MarginRequirement
        /// <summary>
        /// Obtient MGR
        /// </summary>
        public static string MarginRequirement { get { return EventCodeEnum.MGR.ToString(); } }
        #endregion MarginRequirement


        #region NominalStep
        public static string NominalStep { get { return EventCodeEnum.NOS.ToString(); } }
        #endregion NominalStep
        #region NominalQuantityStep
        public static string NominalQuantityStep { get { return EventCodeEnum.NQS.ToString(); } }
        #endregion NominalQuantityStep

        #region Offsetting
        public static string Offsetting { get { return EventCodeEnum.OFS.ToString(); } }
        #endregion Offsetting
        #region Out
        public static string Out { get { return EventCodeEnum.OUT.ToString(); } }
        #endregion Out


        #region PhysicalDelivery
        public static string PhysicalDelivery { get { return EventCodeEnum.DLV.ToString(); } }
        #endregion PhysicalDelivery
        #region PositionCancelation
        public static string PositionCancelation { get { return EventCodeEnum.POC.ToString(); } }
        #endregion PositionCancelation
        #region PositionTransfer
        public static string PositionTransfer { get { return EventCodeEnum.POT.ToString(); } }
        #endregion PositionTransfer
        #region PremiumStyleOption
        /// <summary>
        /// Obtient PSO
        /// </summary>
        public static string PremiumStyleOption { get { return EventCodeEnum.PSO.ToString(); } }
        #endregion PremiumStyleOption
        #region Product
        public static string Product { get { return EventCodeEnum.PRD.ToString(); } }
        #endregion Product
        #region Provision
        public static string Provision { get { return EventCodeEnum.PRO.ToString(); } }
        #endregion Provision

        #region RemoveTrade
        public static string RemoveTrade { get { return EventCodeEnum.RMV.ToString(); } }
        #endregion RemoveTrade
        #region Repo
        public static string Repo { get { return EventCodeEnum.REP.ToString(); } }
        #endregion Repo
        #region RepurchaseAgreementSpotLeg
        public static string RepurchaseAgreementSpotLeg { get { return EventCodeEnum.RSL.ToString(); } }
        #endregion RepurchaseAgreementSpotLeg
        #region RepurchaseAgreementForwardLeg
        public static string RepurchaseAgreementForwardLeg { get { return EventCodeEnum.RFL.ToString(); } }
        #endregion RepurchaseAgreementForwardLeg
        #region ReturnSwap
        public static string TotalReturnLeg { get { return EventCodeEnum.TRL.ToString(); } }
        public static string PriceReturnLeg { get { return EventCodeEnum.PRL.ToString(); } }
        public static string DividendReturnLeg { get { return EventCodeEnum.DRL.ToString(); } }
        public static string InterestLeg { get { return EventCodeEnum.INL.ToString(); } }
        #endregion ReturnSwap

        #region Shifting
        public static string Shifting { get { return EventCodeEnum.SHI.ToString(); } }
        #endregion Shifting

        #region Strategy
        public static string Strategy { get { return EventCodeEnum.STG.ToString(); } }
        #endregion Strategy
        #region SwapUnderlyer
        public static string SwapUnderlyer { get { return EventCodeEnum.SWP.ToString(); } }
        #endregion SwapUnderlyer

        #region TermDeposit
        public static string TermDeposit { get { return EventCodeEnum.TED.ToString(); } }
        #endregion TermDeposit
        #region Trade
        /// <summary>
        /// Obtient TRD
        /// </summary>
        public static string Trade { get { return EventCodeEnum.TRD.ToString(); } }
        #endregion Trade

        #region UnclearingOffsetting
        public static string UnclearingOffsetting { get { return EventCodeEnum.UOF.ToString(); } }
        #endregion UnclearingOffsetting

        #region Basket
        public static string Basket { get { return EventCodeEnum.BSK.ToString(); } }
        #endregion Basket
        #region SingleUnderlyer
        public static string SingleUnderlyer { get { return EventCodeEnum.SUL.ToString(); } }
        #endregion SingleUnderlyer
        #region Underlyer
        public static string Underlyer { get { return EventCodeEnum.UNL.ToString(); } }
        #endregion Underlyer
        #region UnderlyerBasketConstituent
        public static string UnderlyerBasketConstituent { get { return EventCodeEnum.UBC.ToString(); } }
        #endregion UnderlyerBasketConstituent

        #region BasketValuationDate
        public static string BasketValuationDate { get { return EventCodeEnum.BVD.ToString(); } }
        #endregion BasketValuationDate


        #region UnderlyerValuationDate
        public static string UnderlyerValuationDate { get { return EventCodeEnum.UVD.ToString(); } }
        #endregion UnderlyerValuationDate


        #region IsAbandon
        public static bool IsAbandon(string pEventCode) { return (pEventCode == Abandon); }
        #endregion IsAbandon
        #region IsAbandonAssignmentExercise
        public static bool IsAbandonAssignmentExercise(string pEventCode)
        {
            return EventCodeFunc.IsAbandon(pEventCode) || EventCodeFunc.IsAssignment(pEventCode) || IsExercise(pEventCode);
        }
        #endregion IsAbandonAssignmentExercise
        #region IsAssignmentExerciseDate
        public static bool IsAssignmentExerciseDate(string pEventCode)
        {
            return EventCodeFunc.IsExerciseDates(pEventCode) || EventCodeFunc.IsAssignmentDates(pEventCode);
        }
        #endregion IsAssignmentExerciseDate
        #region IsAutomaticAbandonAssignmentExercise
        public static bool IsAutomaticAbandonAssignmentExercise(string pEventCode)
        {
            return EventCodeFunc.IsAutomaticAbandon(pEventCode) || EventCodeFunc.IsAutomaticAssignment(pEventCode) || IsAutomaticExercise(pEventCode);
        }
        #endregion IsAutomaticAbandonAssignmentExercise


        #region IsAllocatedInvoiceDates
        public static bool IsAllocatedInvoiceDates(string pEventCode) { return (pEventCode == AllocatedInvoiceDates); }
        #endregion IsAllocatedInvoiceDates
        #region IsAmericanAverageRateOption
        public static bool IsAmericanAverageRateOption(string pEventCode) { return (pEventCode == AmericanAverageOption); }
        #endregion IsAmericanAverageRateOption
        #region IsAmericanBarrierOption
        public static bool IsAmericanBarrierOption(string pEventCode) { return (pEventCode == AmericanBarrierOption); }
        #endregion IsAmericanBarrierOption
        #region IsAmericanBondOption
        public static bool IsAmericanBondOption(string pEventCode) { return (pEventCode == AmericanBondOption); }
        #endregion IsAmericanBondOption
        #region IsAmericanDigitalOption
        public static bool IsAmericanDigitalOption(string pEventCode) { return (pEventCode == AmericanDigitalOption); }
        #endregion IsAmericanDigitalOption
        #region IsAmericanEquityOption
        public static bool IsAmericanEquityOption(string pEventCode) { return (pEventCode == AmericanEquityOption); }
        #endregion IsAmericanEquityOption
        #region IsAmericanExchangeTradedDerivativeOption
        // EG 20091221 New
        public static bool IsAmericanExchangeTradedDerivativeOption(string pEventCode) { return (pEventCode == AmericanExchangeTradedDerivativeOption); }
        #endregion IsAmericanExchangeTradedDerivativeOption
        #region IsAmericanSimpleOption
        public static bool IsAmericanSimpleOption(string pEventCode) { return (pEventCode == AmericanSimpleOption); }
        #endregion IsAmericanSimpleOption
        #region IsAmericanSwaption
        public static bool IsAmericanSwaption(string pEventCode) { return (pEventCode == AmericanSwaption); }
        #endregion IsAmericanSwaption
        #region IsAsian
        public static bool IsAsian(string pEventCode) { return (pEventCode == Asian); }
        #endregion IsAsian
        #region IsAssignment
        public static bool IsAssignment(string pEventCode) { return (pEventCode == Assignment); }
        #endregion IsAssignment
        #region IsAssignmentDates
        public static bool IsAssignmentDates(string pEventCode) { return (pEventCode == AssignmentDates); }
        #endregion IsAssignmentDates
        #region IsAutomaticAbandon
        public static bool IsAutomaticAbandon(string pEventCode) { return (pEventCode == AutomaticAbandon); }
        #endregion IsAutomaticAbandon
        #region IsAutomaticAssignment
        public static bool IsAutomaticAssignment(string pEventCode) { return (pEventCode == AutomaticAssignment); }
        #endregion IsAutomaticAssignment
        #region IsLeg
        public static bool IsLeg(string pEventCode) { return (pEventCode == Leg); }
        #endregion IsLeg
        #region IsLookBackOption
        public static bool IsLookBackOption(string pEventCode) { return (pEventCode == LookBackOption); }
        #endregion IsLookBackOption
        #region IsBermudaAverageRateOption
        public static bool IsBermudaAverageRateOption(string pEventCode) { return (pEventCode == BermudaAverageOption); }
        #endregion IsBermudaAverageRateOption
        #region IsBermudaBarrierOption
        public static bool IsBermudaBarrierOption(string pEventCode) { return (pEventCode == BermudaBarrierOption); }
        #endregion IsBermudaBarrierOption
        #region IsBermudaBondOption
        public static bool IsBermudaBondOption(string pEventCode) { return (pEventCode == BermudaBondOption); }
        #endregion IsBermudaBondOption
        #region IsBermudaEquityOption
        public static bool IsBermudaEquityOption(string pEventCode) { return (pEventCode == BermudaEquityOption); }
        #endregion IsBermudaEquityOption
        #region IsBermudaSimpleOption
        public static bool IsBermudaSimpleOption(string pEventCode) { return (pEventCode == BermudaSimpleOption); }
        #endregion IsBermudaSimpleOption
        #region IsBermudaSwaption
        public static bool IsBermudaSwaption(string pEventCode) { return (pEventCode == BermudaSwaption); }
        #endregion IsBermudaSwaption
        #region IsBulletPayment
        public static bool IsBulletPayment(string pEventCode) { return (pEventCode == BulletPayment); }
        #endregion IsBulletPayment
        #region IsBuyAndSellBack
        public static bool IsBuyAndSellBack(string pEventCode) { return (pEventCode == BuyAndSellBack); }
        #endregion IsBuyAndSellBack
        #region IsCascading
        public static bool IsCascading(string pEventCode) { return (pEventCode == Cascading); }
        #endregion IsCascading
        #region IsCapFloor
        public static bool IsCapFloor(string pEventCode) { return (pEventCode == CapFloor); }
        #endregion IsCapFloor
        #region IsCashBalanceInterestStream
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        // PM 20120824 [18058] Add IsCashBalanceInterest
        public static bool IsCashBalanceInterest(string pEventCode) { return (pEventCode == CashBalanceInterest); }
        #endregion IsCashBalanceInterest

        #region IsCommoditySpot
        public static bool IsCommoditySpotLeg(string pEventCode) { return IsPhysicalLeg(pEventCode) || IsFinancialLeg(pEventCode); }
        public static bool IsPhysicalLeg(string pEventCode) { return (pEventCode == PhysicalLeg); }
        public static bool IsFinancialLeg(string pEventCode) { return (pEventCode == FinancialLeg); }
        #endregion IsCommoditySpot
        #region IsCommodityLeg
        public static bool IsCommodityLeg(string pEventCode)
        {
            return (IsFinancialLeg(pEventCode) || IsPhysicalLeg(pEventCode));
        }
        #endregion IsCommodityLeg


        #region IsDebtSecurityStream
        public static bool IsDebtSecurityStream(string pEventCode) { return (pEventCode == DebtSecurityStream); }
        #endregion IsDebtSecurityStream
        #region IsDebtSecurityTransaction
        public static bool IsDebtSecurityTransaction(string pEventCode) { return (pEventCode == DebtSecurityTransaction); }
        #endregion IsDebtSecurityTransaction
        #region IsEuropeanAverageRateOption
        public static bool IsEuropeanAverageRateOption(string pEventCode) { return (pEventCode == EuropeanAverageOption); }
        #endregion IsEuropeanAverageRateOption
        #region IsEuropeanBarrierOption
        public static bool IsEuropeanBarrierOption(string pEventCode) { return (pEventCode == EuropeanBarrierOption); }
        #endregion IsEuropeanBarrierOption
        #region IsEuropeanBondOption
        public static bool IsEuropeanBondOption(string pEventCode) { return (pEventCode == EuropeanBondOption); }
        #endregion IsEuropeanBondOption
        #region IsEuropeanDigitalOption
        public static bool IsEuropeanDigitalOption(string pEventCode) { return (pEventCode == EuropeanDigitalOption); }
        #endregion IsEuropeanDigitalOption
        #region IsEuropeanEquityOption
        public static bool IsEuropeanEquityOption(string pEventCode) { return (pEventCode == EuropeanEquityOption); }
        #endregion IsEuropeanEquityOption
        #region IsEuropeanExchangeTradedDerivativeOption
        // EG 20091221 New
        public static bool IsEuropeanExchangeTradedDerivativeOption(string pEventCode) { return (pEventCode == EuropeanExchangeTradedDerivativeOption); }
        #endregion IsEuropeanExchangeTradedDerivativeOption

        #region IsEuropeanSimpleOption
        public static bool IsEuropeanSimpleOption(string pEventCode) { return (pEventCode == EuropeanSimpleOption); }
        #endregion IsEuropeanSimpleOption
        #region IsEuropeanSwaption
        public static bool IsEuropeanSwaption(string pEventCode) { return (pEventCode == EuropeanSwaption); }
        #endregion IsEuropeanSwaption

        #region IsExercise
        public static bool IsExercise(string pEventCode) { return (pEventCode == Exercise); }
        #endregion IsExercise
        #region IsExerciseProvision
        public static bool IsExerciseProvision(string pEventCode)
        {
            return IsExerciseCancelable(pEventCode) || IsExerciseMandatoryEarlyTermination(pEventCode) ||
                   IsExerciseExtendible(pEventCode) || IsExerciseOptionalEarlyTermination(pEventCode) ||
                   IsExerciseStepUp(pEventCode);
        }
        #endregion IsExerciseProvision
        #region IsExerciseCancelable
        public static bool IsExerciseCancelable(string pEventCode) { return (pEventCode == ExerciseCancelable); }
        #endregion IsExerciseCancelable
        #region IsExerciseExtendible
        public static bool IsExerciseExtendible(string pEventCode) { return (pEventCode == ExerciseExtendible); }
        #endregion IsExerciseExtendible
        #region IsExerciseMandatoryEarlyTermination
        public static bool IsExerciseMandatoryEarlyTermination(string pEventCode) { return (pEventCode == ExerciseMandatoryEarlyTermination); }
        #endregion IsExerciseMandatoryEarlyTermination
        #region IsExerciseOptionalEarlyTermination
        public static bool IsExerciseOptionalEarlyTermination(string pEventCode) { return (pEventCode == ExerciseOptionalEarlyTermination); }
        #endregion IsExerciseOptionalEarlyTermination
        #region IsExerciseStepUp
        public static bool IsExerciseStepUp(string pEventCode) { return (pEventCode == ExerciseStepUp); }
        #endregion IsExerciseStepUp
        #region IsForwardRateAgreement
        public static bool IsForwardRateAgreement(string pEventCode) { return (pEventCode == ForwardRateAgreement); }
        #endregion IsForwardRateAgreement
        #region IsETDFuture
        // EG 20091221 New
        public static bool IsETDFuture(string pEventCode) { return (pEventCode == FutureExchangeTradedDerivative); }
        #endregion IsETDFuture
        #region IsFuturesStyleOption
        public static bool IsFuturesStyleOption(string pEventCode) { return (pEventCode == FuturesStyleOption); }
        #endregion IsFuturesStyleOption
        #region IsFxSpot
        public static bool IsFxSpot(string pEventCode) { return (pEventCode == FxSpot); }
        #endregion IsFxSpot
        #region IsFxForward
        public static bool IsFxForward(string pEventCode) { return (pEventCode == FxForward); }
        #endregion IsFxForward
        #region IsInitialValuation
        public static bool IsInitialValuation(string pEventCode) { return (pEventCode == InitialValuation); }
        #endregion IsInitialValuation
        #region IsInterestRateSwap
        public static bool IsInterestRateSwap(string pEventCode) { return (pEventCode == InterestRateSwap); }
        #endregion IsInterestRateSwap
        #region IsInvoiceAmended
        public static bool IsInvoiceAmended(string pEventType) { return (pEventType == InvoiceAmended); }
        #endregion IsInvoiceAmended
        #region IsInvoiceMaster
        public static bool IsInvoiceMaster(string pEventType) { return (pEventType == InvoiceMaster); }
        #endregion IsInvoiceMaster
        #region IsInvoiceMasterBase
        public static bool IsInvoiceMasterBase(string pEventType) { return (pEventType == InvoiceMasterBase); }
        #endregion IsInvoiceMasterBase
        #region IsInvoicingDates
        public static bool IsInvoicingDates(string pEventCode) { return (pEventCode == InvoicingDates); }
        #endregion IsInvoicingDates
        #region IsInvoiceSettlement
        public static bool IsInvoiceSettlement(string pEventCode) { return (pEventCode == InvoiceSettlement); }
        #endregion IsInvoiceSettlement
        #region IsLoanDeposit
        public static bool IsLoanDeposit(string pEventCode) { return (pEventCode == LoanDeposit); }
        #endregion IsLoanDeposit
        #region IsMarginRequirement
        public static bool IsMarginRequirement(string pEventCode) { return (pEventCode == MarginRequirement); }
        #endregion IsMarginRequirement
        #region IsNominalStep
        public static bool IsNominalStep(string pEventCode) { return (pEventCode == NominalStep); }
        #endregion IsNominalStep
        #region IsNominalQuantityStep
        public static bool IsNominalQuantityStep(string pEventCode) { return (pEventCode == NominalQuantityStep); }
        #endregion IsNominalQuantityStep
        #region IsOffSettingCorporateAction
        public static bool IsOffSettingCorporateAction(string pEventCode) { return (pEventCode == OffSettingCorporateAction); }
        #endregion IsOffSettingCorporateAction

        #region IsOut
        public static bool IsOut(string pEventCode) { return (pEventCode == Out); }
        #endregion IsOut
        #region IsPremiumStyleOption
        public static bool IsPremiumStyleOption(string pEventCode) { return (pEventCode == PremiumStyleOption); }
        #endregion IsPremiumStyleOption
        #region IsProduct
        public static bool IsProduct(string pEventCode) { return (pEventCode == Product); }
        #endregion IsProduct
        #region IsProvision
        public static bool IsProvision(string pEventCode) { return (pEventCode == Provision); }
        #endregion IsProvision
        #region IsProvisionEvent
        public static bool IsProvisionEvent(string pEventCode) 
        {
            return (IsExerciseCancelable(pEventCode) || IsExerciseExtendible(pEventCode) || IsExerciseMandatoryEarlyTermination(pEventCode) || 
                IsExerciseOptionalEarlyTermination(pEventCode) || IsExerciseStepUp(pEventCode)); 
        }
        #endregion IsProvision
        #region IsRemoveTrade
        public static bool IsRemoveTrade(string pEventCode) { return (pEventCode == RemoveTrade); }
        #endregion IsRemoveTrade
        #region IsRepo
        public static bool IsRepo(string pEventCode) { return (pEventCode == Repo); }
        #endregion IsRepo
        #region IsRepurchaseAgreementSpotLeg
        public static bool IsRepurchaseAgreementSpotLeg(string pEventCode) { return (pEventCode == RepurchaseAgreementSpotLeg); }
        #endregion IsRepurchaseAgreementSpotLeg
        #region IsRepurchaseAgreementForwardLeg
        public static bool IsRepurchaseAgreementForwardLeg(string pEventCode) { return (pEventCode == RepurchaseAgreementForwardLeg); }
        #endregion IsRepurchaseAgreementForwardLeg
        #region IsRepoLeg
        public static bool IsRepoLeg(string pEventCode)
        {
            return (IsRepurchaseAgreementSpotLeg(pEventCode) || IsRepurchaseAgreementForwardLeg(pEventCode));
        }
        #endregion IsRepoLeg

        #region IsReturnSwap
        public static bool IsReturnSwapLeg(string pEventCode) { return IsReturnLeg(pEventCode) || IsInterestLeg(pEventCode); }
        public static bool IsReturnLeg(string pEventCode) { return IsTotalReturnLeg(pEventCode) || IsPriceReturnLeg(pEventCode) || IsDividendReturnLeg(pEventCode); }
        public static bool IsTotalReturnLeg(string pEventCode) { return (pEventCode == TotalReturnLeg); }
        public static bool IsPriceReturnLeg(string pEventCode) { return (pEventCode == PriceReturnLeg); }
        public static bool IsDividendReturnLeg(string pEventCode) { return (pEventCode == DividendReturnLeg); }
        public static bool IsInterestLeg(string pEventCode) { return (pEventCode == InterestLeg); }
        #endregion IsReturnSwap
        #region IsShifting
        public static bool IsShifting(string pEventCode) { return (pEventCode == Shifting); }
        #endregion IsShifting
        #region IsStrategy
        public static bool IsStrategy(string pEventCode) { return (pEventCode == Strategy); }
        #endregion IsStrategy
        #region IsSwapUnderlyer
        public static bool IsSwapUnderlyer(string pEventCode) { return (pEventCode == SwapUnderlyer); }
        #endregion IsSwapUnderlyer
        #region IsTermDeposit
        public static bool IsTermDeposit(string pEventCode) { return (pEventCode == TermDeposit); }
        #endregion IsTermDeposit

        #region IsTrade
        /// <summary>
        /// Retourne true si EVENTCODE = TRD
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsTrade(string pEventCode) { return (pEventCode == Trade); }
        #endregion IsTrade
        #region IsUnclearingOffsetting
        public static bool IsUnclearingOffsetting(string pEventCode) { return (pEventCode == UnclearingOffsetting); }
        #endregion IsUnclearingOffsetting

        #region IsSingleUnderlyer
        public static bool IsSingleUnderlyer(string pEventCode) { return (pEventCode == SingleUnderlyer); }
        #endregion IsSingleUnderlyer

        #region IsBasket
        public static bool IsBasket(string pEventCode) { return (pEventCode == Basket); }
        #endregion IsBasket

        #region IsBasketValuationDate
        public static bool IsBasketValuationDate(string pEventCode) { return (pEventCode == BasketValuationDate); }
        #endregion IsBasketValuationDate

        #region IsUnderlyerValuationDate
        public static bool IsUnderlyerValuationDate(string pEventCode) { return (pEventCode == UnderlyerValuationDate); }
        #endregion IsUnderlyerValuationDate

        #endregion Group Level Trade Events
        #region IsAdministrativeStreamGroup
        public static bool IsAdministrativeStreamGroup(string pEventCode)
        {
            return (IsInvoicingDates(pEventCode) || IsCreditNoteDates(pEventCode));
        }
        #endregion IsAdministrativeStreamGroup

        #region IsAdministrationLevel

        public static bool IsAdministrationLevel(string pEventCode)
        {
            return (IsStartTermination(pEventCode) || IsIntermediary(pEventCode) ||
                IsLinkedProductPayment(pEventCode) || IsAdditionalPayment(pEventCode) || IsOtherPartyPayment(pEventCode) ||
                IsDepreciableAmount(pEventCode)) || IsDailyClosing(pEventCode) || 
                //20110518 FI Add 
                IsLinkedProductClosing(pEventCode) || IsLinkedProductIntraday(pEventCode) ||
                //PL 20170703 Add DLV for DLV/PHY [23264]
                IsDelivery(pEventCode);
        }
        #endregion IsAdministrationLevel
        #region IsCalculationLevel
        public static bool IsCalculationLevel(string pEventCode)
        {
            return (IsCalculationPeriod(pEventCode) ||
                IsReset(pEventCode) || IsSelfAverage(pEventCode) || IsSelfReset(pEventCode) ||
                IsLinkedProductPayment(pEventCode));
        }
        #endregion IsCalculationLevel
        #region IsDailyClosingLevel
        public static bool IsDailyClosingLevel(string pEventCode)
        {
            return IsDailyClosing(pEventCode);
        }
        #endregion IsDailyClosingLevel
        #region IsDescriptionLevel
        public static bool IsDescriptionLevel(string pEventCode)
        {
            return (IsExerciseDates(pEventCode) || IsBarrier(pEventCode) || IsTrigger(pEventCode) || IsEffectiveRate(pEventCode) ||
                IsAutomaticExercise(pEventCode) || IsAsian(pEventCode) || IsLinkedProductPayment(pEventCode) ||
                IsAdditionalInvoiceDates(pEventCode) || IsCreditNoteDates(pEventCode) || IsInvoiceAmended(pEventCode) ||
                IsInvoiceMaster(pEventCode) || IsInvoiceMasterBase(pEventCode) || IsAllocatedInvoiceDates(pEventCode));
        }
        #endregion IsDescriptionLevel


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        // EG 20091228 Add IsNominalQuantityStep
        // FI 20111109 Add IsCashFlowConstituent
        // PM 20140925 [20066][20185] Add IsFuturesStyleOption & IsPremiumStyleOption
        public static bool IsGroupLevel(string pEventCode)
        {
            return IsFxOption(pEventCode) || IsLegGroup(pEventCode) || IsSimpleOption(pEventCode) || IsStreamGroup(pEventCode) ||
                IsTrade(pEventCode) || IsProduct(pEventCode) || IsNominalStep(pEventCode) || IsNominalQuantityStep(pEventCode) ||
                IsAbandon(pEventCode) || IsExercise(pEventCode) || IsOut(pEventCode) || IsLeg(pEventCode) || IsSwapUnderlyer(pEventCode) ||
                IsCashFlowConstituent(pEventCode) || IsFuturesStyleOption(pEventCode) || IsPremiumStyleOption(pEventCode);
        }
        #region IsAmericanOption
        // EG 20140429 [20513]
        public static bool IsAmericanOption(string pEventCode)
        {
            return (IsAmericanAverageRateOption(pEventCode) || IsAmericanBarrierOption(pEventCode) ||
                IsAmericanSimpleOption(pEventCode) || IsAmericanDigitalOption(pEventCode) || IsAmericanSwaption(pEventCode) ||
                IsAmericanBondOption(pEventCode) || IsAmericanEquityOption(pEventCode));
        }
        #endregion IsAmericanOption
        #region IsAverageRateOption
        public static bool IsAverageRateOption(string pEventCode)
        {
            return (IsAmericanAverageRateOption(pEventCode) ||
                IsBermudaAverageRateOption(pEventCode) ||
                IsEuropeanAverageRateOption(pEventCode));
        }
        #endregion IsAverageRateOption
        #region IsBarrierOption
        public static bool IsBarrierOption(string pEventCode)
        {
            return (IsAmericanBarrierOption(pEventCode) || IsBermudaBarrierOption(pEventCode) || IsEuropeanBarrierOption(pEventCode));
        }
        #endregion IsFxBarrierOption
        #region IsBermudaOption
        // EG 20140429 [20513]
        public static bool IsBermudaOption(string pEventCode)
        {
            return (IsBermudaAverageRateOption(pEventCode) || IsBermudaBarrierOption(pEventCode) ||
                IsBermudaSimpleOption(pEventCode) || IsBermudaSwaption(pEventCode) ||
                IsBermudaBondOption(pEventCode) || IsBermudaEquityOption(pEventCode));
        }
        #endregion IsBermudaOption
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsCashBalanceStream(string pEventCode)
        {
            return ((pEventCode == CashBalanceStream) || (pEventCode == ExchangeCashBalanceStream));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsCashFlowConstituent(string pEventCode)
        {
            return (pEventCode == CashFlowConstituent);
        }

       
        #region IsBondOption
        public static bool IsBondOption(string pEventCode)
        {
            return (IsAmericanBondOption(pEventCode) || IsBermudaBondOption(pEventCode) || IsEuropeanBondOption(pEventCode));
        }
        #endregion IsBondOption
        #region IsDigitalOption
        public static bool IsDigitalOption(string pEventCode)
        {
            return IsEuropeanDigitalOption(pEventCode) || IsAmericanDigitalOption(pEventCode);
        }
        #endregion IsDigitalOption
        #region IsEquityOption
        public static bool IsEquityOption(string pEventCode)
        {
            return (IsAmericanEquityOption(pEventCode) || IsBermudaEquityOption(pEventCode) || IsEuropeanEquityOption(pEventCode));
        }
        #endregion IsEquityOption
        #region IsEquitySecurityTransaction
        public static bool IsEquitySecurityTransaction(string pEventCode)
        {
            { return (pEventCode == EquitySecurityTransaction); }
        }
        #endregion IsEquitySecurityTransaction
        #region IsETDOption
        // EG 20091227
        public static bool IsETDOption(string pEventCode)
        {
            return IsAmericanExchangeTradedDerivativeOption(pEventCode) || IsEuropeanExchangeTradedDerivativeOption(pEventCode);
        }
        #endregion IsETDOption
        #region IsEuropeanOption
        // EG 20140429 [20513]
        public static bool IsEuropeanOption(string pEventCode)
        {
            return (IsEuropeanAverageRateOption(pEventCode) || IsEuropeanBarrierOption(pEventCode) ||
                IsEuropeanSimpleOption(pEventCode) || IsEuropeanDigitalOption(pEventCode) || IsEuropeanSwaption(pEventCode) ||
                IsEuropeanBondOption(pEventCode) || IsEuropeanEquityOption(pEventCode));
        }
        #endregion IsEuropeanOption
        #region IsFxOption
        public static bool IsFxOption(string pEventCode)
        {
            return IsSimpleOption(pEventCode) || IsBarrierOption(pEventCode) ||
                IsDigitalOption(pEventCode) || IsAverageRateOption(pEventCode);
        }
        #endregion IsFxOption
        #region IsFxHasActivatedStatus
        public static bool IsFxHasActivatedStatus(string pEventCode) { return (IsBarrier(pEventCode) || IsTrigger(pEventCode)); }
        #endregion IsFxHasActivatedStatus
        #region IsPhysicalDelivery
        public static bool IsPhysicalDelivery(string pEventCode)
        {
            return (pEventCode == PhysicalDelivery);
        }
        #endregion IsPhysicalDelivery

        #region IsProductUnderlyer
        public static bool IsProductUnderlyer(string pEventCode)
        {
            return IsSwapUnderlyer(pEventCode);
        }
        #endregion IsProductUnderlyer
        #region IsLegGroup
        /// <summary>
        /// <remarks>Attention Spheres® incrémente STREAMNO ds les évènements à chaque code défini IsLegGroup rencontré</remarks>
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsLegGroup(string pEventCode)
        {
            return (IsBulletPayment(pEventCode) || IsForwardRateAgreement(pEventCode) || IsTermDeposit(pEventCode) ||
                IsFxSpot(pEventCode) || IsFxForward(pEventCode) || IsStreamGroup(pEventCode) || IsFxOption(pEventCode) ||
                IsETDFuture(pEventCode) || IsETDOption(pEventCode) || IsLeg(pEventCode) || IsReturnSwapLeg(pEventCode) || IsEquityOption(pEventCode) ||
                IsSwaptionGroup(pEventCode) || IsInvoicingDates(pEventCode) || IsInvoiceSettlement(pEventCode) || IsRepoLeg(pEventCode) || IsDebtSecurityTransaction(pEventCode) ||
                IsMarginRequirement(pEventCode) || IsEquitySecurityTransaction(pEventCode) || IsBondOption(pEventCode) || IsCommoditySpotLeg(pEventCode));
        }
        #endregion IsLegGroup

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsSimpleOption(string pEventCode)
        {
            return (IsAmericanSimpleOption(pEventCode) || IsBermudaSimpleOption(pEventCode) || IsEuropeanSimpleOption(pEventCode));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        /// FI 20111109 Add IsCashBalanceStream
        // PM 20120824 [18058] Add IsCashBalanceInterestStream
        public static bool IsStreamGroup(string pEventCode)
        {
            return (IsCapFloor(pEventCode) || IsInterestRateSwap(pEventCode) ||
                IsLoanDeposit(pEventCode) || IsDebtSecurityStream(pEventCode) ||
                IsCashBalanceStream(pEventCode) || IsCashBalanceInterest(pEventCode));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsSwaptionGroup(string pEventCode)
        {
            return (IsAmericanSwaption(pEventCode) || IsBermudaSwaption(pEventCode) || IsEuropeanSwaption(pEventCode));
        }

        #region IsEventChildrensIsUnderlyerValuationDate
        public static bool IsEventChildrensIsUnderlyerValuationDate(string pEventCode)
        {
            return (IsExerciseDates(pEventCode) || IsAsian(pEventCode) || IsLookBackOption(pEventCode) || IsBarrier(pEventCode));
        }
        #endregion IsEventChildrensIsUnderlyerValuationDate

        #region Others
        #region AutomaticExerciseDates
        public static string AutomaticExerciseDates { get { return EventCodeEnum.EAD.ToString(); } }
        #endregion AutomaticExerciseDates

        #region AutomaticExercise
        public static string AutomaticExercise { get { return EventCodeEnum.AEX.ToString(); } }
        #endregion AutomaticExercise
        #region FallbackExercise
        public static string FallbackExercise { get { return EventCodeEnum.FEX.ToString(); } }
        #endregion FallbackExercise

        #region IsAutomaticExercise
        public static bool IsAutomaticExercise(string pEventCode) { return (pEventCode == AutomaticExercise); }
        #endregion IsAutomaticExercise

        #region IsAutomaticExerciseDates
        public static bool IsAutomaticExerciseDates(string pEventCode) { return (pEventCode == AutomaticExerciseDates); }
        #endregion IsAutomaticExerciseDates

        #region IsFallbackExercise
        public static bool IsFallbackExercise(string pEventCode) { return (pEventCode == FallbackExercise); }
        #endregion IsFallbackExercise
        #region IsFxExerciseProcedure
        public static bool IsFxExerciseProcedure(string pEventCode)
        {
            return (IsAutomaticExercise(pEventCode) || IsFallbackExercise(pEventCode));
        }
        #endregion IsFxExerciseProcedure
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <returns></returns>
        public static bool IsPayment(string pEventCode)
        {
            return (IsStartTermination(pEventCode) || IsIntermediary(pEventCode));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTT"></param>
        /// <returns></returns>
        public static string LinkProductClosingIntraday(FixML.Enum.SettlSessIDEnum pTT)
        {
            string ret = string.Empty;

            if (pTT == FixML.Enum.SettlSessIDEnum.EndOfDay)
                ret = LinkedProductClosing;
            else if (pTT == FixML.Enum.SettlSessIDEnum.Intraday)
                ret = LinkedProductIntraday;
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTT"></param>
        /// <returns></returns>
        public static string LinkFutureClosingIntraday(FixML.Enum.SettlSessIDEnum pTT)
        {
            string ret = string.Empty;
            if (pTT == FixML.Enum.SettlSessIDEnum.EndOfDay)
            {
                ret = LinkedFutureClosing;
            }
            else if (pTT == FixML.Enum.SettlSessIDEnum.Intraday)
            {
                ret = LinkedFutureIntraday;
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTT"></param>
        /// <returns></returns>
        public static string LinkOptionClosingIntraday(FixML.Enum.SettlSessIDEnum pTT)
        {
            string ret = string.Empty;
            if (pTT == FixML.Enum.SettlSessIDEnum.EndOfDay)
            {
                ret = LinkedOptionClosing;
            }
            else if (pTT == FixML.Enum.SettlSessIDEnum.Intraday)
            {
                ret = LinkedOptionIntraday;
            }
            return ret;
        }
        #endregion Others
    }
    #endregion EventCodeFunc
    #region EventCodeAndEventTypeFunc
    public sealed class EventCodeAndEventTypeFunc
    {
        #region Methods
        #region Is EventCodeAndEventTypeFunc
        #region IsAbandonPartiel
        public static bool IsAbandonPartiel(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsAbandon(pEventCode) && EventTypeFunc.IsPartiel(pEventType));
        }
        #endregion IsAbandonPartiel
        #region IsAbandonTotal
        public static bool IsAbandonTotal(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsAbandon(pEventCode) && EventTypeFunc.IsTotal(pEventType));
        }
        #endregion IsAbandonTotal
        #region IsEventChildrensIsInvoiceDetailFee
        public static bool IsEventChildrensIsInvoiceDetailFee(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) || EventCodeFunc.IsInvoiceAmended(pEventCode)) &&
                EventTypeFunc.IsGrossTurnOverAmount(pEventType);
        }
        #endregion IsEventChildrensIsInvoiceDetailFee
        #region IsEventChildrensIsInvoiceNetTurnOver
        public static bool IsEventChildrensIsInvoiceTax(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) || EventCodeFunc.IsInvoiceAmended(pEventCode)) &&
                EventTypeFunc.IsTax(pEventType);
        }
        #endregion IsEventChildrensIsInvoiceDetailFee
        #region IsExerciseMultiple
        public static bool IsExerciseMultiple(string pEventCode, string pEventType)
        {
            return EventTypeFunc.IsMultiple(pEventType) &&
                   (EventCodeFunc.IsExercise(pEventCode) || EventCodeFunc.IsExerciseProvision(pEventCode));
        }
        #endregion IsExerciseMultiple
        #region IsExercisePartiel
        public static bool IsExercisePartiel(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsExercise(pEventCode) || EventCodeFunc.IsExerciseProvision(pEventCode)) && EventTypeFunc.IsPartiel(pEventType);
        }
        #endregion IsExercisePartiel
        #region IsExerciseTotal
        public static bool IsExerciseTotal(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsExercise(pEventCode) || EventCodeFunc.IsExerciseProvision(pEventCode)) && EventTypeFunc.IsTotal(pEventType);
        }
        #endregion IsExerciseTotal
        #region IsFxCustomerReset
        public static bool IsFxCustomerReset(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsReset(pEventCode) && EventTypeFunc.IsFxCustomer(pEventType);
        }
        #endregion IsFxCustomerReset
        #region IsFxRateReset
        public static bool IsFxRateReset(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsReset(pEventCode) &&
                (EventTypeFunc.IsSettlementCurrency(pEventType) || EventTypeFunc.IsFxRatePlus(pEventType));
        }
        #endregion IsFxRateReset
        #region IsFloatingRateCalculationPeriod
        public static bool IsFloatingRateCalculationPeriod(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsCalculationPeriod(pEventCode) && EventTypeFunc.IsFloatingRate(pEventType));
        }
        #endregion IsFloatingRateCalculationPeriod
        #region IsHistoricalVariationMargin
        public static bool IsHistoricalVariationMargin(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductClosing(pEventCode) && EventTypeFunc.IsHistoricalVariationMargin(pEventType));
        }
        #endregion IsHistoricalVariationMargin
        #region IsIntradayLiquidationOptionValue
        public static bool IsIntradayLiquidationOptionValue(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductIntraday(pEventCode) && EventTypeFunc.IsLiquidationOptionValue(pEventType));
        }
        #endregion IsIntradayLiquidationOptionValue
        #region IsIntradayUnrealizedMargin
        public static bool IsIntradayUnrealizedMargin(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductIntraday(pEventCode) && EventTypeFunc.IsUnrealizedMargin(pEventType));
        }
        #endregion IsIntradayUnrealizedMargin
        #region IsIntradayVariationMargin
        public static bool IsIntradayVariationMargin(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductIntraday(pEventCode) && EventTypeFunc.IsVariationMargin(pEventType));
        }
        #endregion IsIntradayVariationMargin
        #region IsLinkedProductClosing
        public static bool IsLinkedProductClosing(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductClosing(pEventCode) && EventTypeFunc.IsAmounts(pEventType));
        }
        #endregion IsLinkedProductClosing
        #region IsLinkedProductPayment
        // EG 20231127 [WI754] Implementation Return Swap : New
        public static bool IsLinkedProductPayment(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) && EventTypeFunc.IsAmounts(pEventType));
        }
        #endregion IsLinkedProductPayment
        #region IsLinkedPhysicalDelivery
        // EG 20170206 [22787]
        public static bool IsLinkedPhysicalDelivery(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsPhysicalDelivery(pEventCode) && EventTypeFunc.IsPhysicalSettlement(pEventType));
        }
        #endregion IsLinkedPhysicalDelivery
        #region IsLinkedProductPaymentAndCashSettlement
        public static bool IsLinkedProductPaymentAndCashSettlement(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) && EventTypeFunc.IsCashSettlement(pEventType));
        }
        #endregion IsLinkedProductPaymentAndCashSettlement

        #region IsLiquidationOptionValue
        public static bool IsLiquidationOptionValue(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductClosing(pEventCode) && EventTypeFunc.IsLiquidationOptionValue(pEventType));
        }
        #endregion IsLiquidationOptionValue
        #region IsNominalPeriodVariation
        public static bool IsNominalPeriodVariation(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsPayment(pEventCode) && EventTypeFunc.IsNominal(pEventType));
        }
        #endregion IsNominalPeriodVariation
        #region IsQuantityPeriodVariation
        public static bool IsQuantityPeriodVariation(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsPayment(pEventCode) && EventTypeFunc.IsQuantity(pEventType));
        }
        #endregion IsQuantityPeriodVariation
        #region IsMarginRequirementRatio
        public static bool IsMarginRequirementRatio(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) && EventTypeFunc.IsMarginRequirementRatio(pEventType));
        }
        #endregion IsMarginRequirementRatio

        #region IsRealizedMargin
        public static bool IsRealizedMargin(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductClosing(pEventCode) && EventTypeFunc.IsRealizedMargin(pEventType));
        }
        #endregion IsRealizedMargin

        #region IsRemoveTrade
        public static bool IsRemoveTrade(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsRemoveTrade(pEventCode) && EventTypeFunc.IsDate(pEventType));
        }
        #endregion IsRemoveTrade
        #region IsTerminationSettlementCurrency
        public static bool IsTerminationSettlementCurrency(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsTermination(pEventCode) && EventTypeFunc.IsSettlementCurrency(pEventType));
        }
        #endregion IsTerminationSettlementCurrency
        #region IsUnrealizedMargin
        public static bool IsUnrealizedMargin(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductClosing(pEventCode) && EventTypeFunc.IsUnrealizedMargin(pEventType));
        }
        #endregion IsUnrealizedMargin
        #region IsVariationMargin
        // EG 20210415 [25584][25702] 
        public static bool IsVariationMargin(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) || EventCodeFunc.IsLinkedProductClosing(pEventCode)) && EventTypeFunc.IsVariationMargin(pEventType);
        }
        #endregion IsVariationMargin

        #endregion Is EventCodeAndEventTypeFunc
        #endregion Methods
    }
    #endregion EventCodeAndEventTypeFunc
    #region EventTypeFunc
    /// EG 20150302 Add BaseCurrency|QuotedCurrency (CFD Forex)
    public sealed class EventTypeFunc
    {
        #region Administration Level Trade Events
        #region AccruedInterestAmount
        public static string AccruedInterestAmount { get { return AccrualInterests; } }
        #endregion AccruedInterestAmount

        #region BondPayment
        public static string BondPayment { get { return EventTypeEnum.BDP.ToString(); } }
        #endregion BondPayment

        #region Brokerage
        public static string Brokerage { get { return EventTypeEnum.BRO.ToString(); } }
        public static string TradingBrokerage { get { return EventTypeEnum.TBR.ToString(); } }
        public static string ClearingBrokerage { get { return EventTypeEnum.CBR.ToString(); } }
        #endregion Brokerage
        #region BaseCurrency
        /// EG 20150302 New (CFD Forex)
        public static string BaseCurrency { get { return EventTypeEnum.BCU.ToString(); } }
        #endregion BaseCurrency

        #region BaseCurrency1
        public static string BaseCurrency1 { get { return EventTypeEnum.BC1.ToString(); } }
        #endregion BaseCurrency1
        #region BaseCurrency2
        public static string BaseCurrency2 { get { return EventTypeEnum.BC2.ToString(); } }
        #endregion BaseCurrency2

        #region BorrowingAmount
        /// EG 20150319 (POC] New
        public static string BorrowingAmount { get { return EventTypeEnum.BWA.ToString(); } }
        #endregion BorrowingAmount


        #region CallCurrency
        public static string CallCurrency { get { return EventTypeEnum.CCU.ToString(); } }
        #endregion CallCurrency
        #region CashSettlement
        public static string CashSettlement { get { return EventTypeEnum.CSH.ToString(); } }
        #endregion CashSettlement


        #region CleanAmount
        public static string CleanAmount { get { return EventTypeEnum.CAM.ToString(); } }
        #endregion CleanAmount
        #region Currency1
        public static string Currency1 { get { return EventTypeEnum.CU1.ToString(); } }
        #endregion Currency1
        #region Currency2
        public static string Currency2 { get { return EventTypeEnum.CU2.ToString(); } }
        #endregion Currency2

        #region DebtSecurityTransactionAmounts
        public static string DebtSecurityTransactionAmounts { get { return EventTypeEnum.DSA.ToString(); } }
        #endregion DebtSecurityTransactionAmounts
        #region DeliveryAmount
        public static string DeliveryAmount { get { return EventTypeEnum.DVA.ToString(); } }
        #endregion DeliveryAmount
        #region HistoricalDeliveryAmount
        // EG 20170424 [23064] New
        public static string HistoricalDeliveryAmount { get { return EventTypeEnum.HDV.ToString(); } }
        #endregion HistoricalDeliveryAmount

        #region DirtyAmount
        public static string DirtyAmount { get { return EventTypeEnum.DAM.ToString(); } }
        #endregion DirtyAmount

        #region ElectionSettlement
        public static string ElectionSettlement { get { return EventTypeEnum.ELE.ToString(); } }
        #endregion ElectionSettlement

        #region Entitlement
        public static string Entitlement { get { return EventTypeEnum.ENT.ToString(); } }
        #endregion Entitlement

        #region EqualisationPayment
        // PM 20170911 [23408] New Equalisation Payment
        public static string EqualisationPayment { get { return EventTypeEnum.EQP.ToString(); } }
        #endregion EqualisationPayment

        #region Fee
        public static string Fee { get { return EventTypeEnum.FEE.ToString(); } }
        #endregion Fee
        #region FeeAccountingAmount
        public static string FeeAccountingAmount { get { return EventTypeEnum.FAA.ToString(); } }
        #endregion FeeAccountingAmount
        #region ForwardPoints
        public static string ForwardPoints { get { return EventTypeEnum.FWP.ToString(); } }
        #endregion ForwardPoints


        #region FundingAmount
        public static string FundingAmount { get { return EventTypeEnum.FDA.ToString(); } }
        #endregion FundingAmount

        #region GrossAmount
        public static string GrossAmount { get { return EventTypeEnum.GAM.ToString(); } }
        #endregion GrossAmount

        #region HistoricalGrossAmount
        // EG 20190730 New HGA - Historical GAM for PositionOpening on DebtSecurityTransaction
        public static string HistoricalGrossAmount { get { return EventTypeEnum.HGA.ToString(); } }
        #endregion HistoricalGrossAmount

        #region HistoricalPremium
        public static string HistoricalPremium { get { return EventTypeEnum.HPR.ToString(); } }
        #endregion HistoricalPremium

        #region HistoricalVariationMargin
        public static string HistoricalVariationMargin { get { return EventTypeEnum.HVM.ToString(); } }
        #endregion HistoricalVariationMargin


        #region Interest
        public static string Interest { get { return EventTypeEnum.INT.ToString(); } }
        #endregion Interest

        #region KnownAmount
        public static string KnownAmount { get { return EventTypeEnum.KNA.ToString(); } }
        #endregion KnownAmount

        #region LiquidationOptionValue
        public static string LiquidationOptionValue { get { return EventTypeEnum.LOV.ToString(); } }
        #endregion LiquidationOptionValue

        #region MarketValue
        /// <summary>
        /// MKV
        /// </summary>
        public static string MarketValue { get { return EventTypeEnum.MKV.ToString(); } }
        /// <summary>
        /// MKP (Entre dans la composition du MKV)
        /// </summary>
        /// FI 20151019 [21317] Add
        public static string MarketValuePrincipalAmount { get { return EventTypeEnum.MKP.ToString(); } }
        /// <summary>
        /// MKA (Entre dans la composition du MKV)
        /// </summary>
        /// FI 20151019 [21317] Add
        public static string MarketValueAccruedInterest { get { return EventTypeEnum.MKA.ToString(); } }
        #endregion MarketValue

        #region Nominal
        public static string Nominal { get { return EventTypeEnum.NOM.ToString(); } }
        #endregion Nominal

        #region Payout
        public static string Payout { get { return EventTypeEnum.PAO.ToString(); } }
        #endregion Payout
        #region Premium
        public static string Premium { get { return EventTypeEnum.PRM.ToString(); } }
        // EG 20210812 [25173] New PCR et PRT (Prime)
        public static string PremiumCashResidual { get { return EventTypeEnum.PCR.ToString(); } }
        public static string PremiumTheoretical { get { return EventTypeEnum.PRT.ToString(); } }
        #endregion Premium

        
        #region PrincipalAmount
        /// <summary>
        /// Nominal * CleanPrice on debtSecurityTransaction
        /// </summary>
        /// FI 20151228 [21660] Add PrincipalAmount
        public static string PrincipalAmount { get { return EventTypeEnum.PAM.ToString(); } }
        #endregion PrincipalAmount

        #region PutCurrency
        public static string PutCurrency { get { return EventTypeEnum.PCU.ToString(); } }
        #endregion PutCurrency

        #region RedemptionAmount
        // EG 20190926 [Maturity Redemption] New
        public static string RedemptionAmount { get { return EventTypeEnum.RAM.ToString(); } }
        #endregion RedemptionAmount
        #region Quantity
        public static string Quantity { get { return EventTypeEnum.QTY.ToString(); } }
        #endregion Quantity

        #region QuotedCurrency
        /// EG 20150302 New (CFD Forex)
        public static string QuotedCurrency { get { return EventTypeEnum.QCU.ToString(); } }
        #endregion QuotedCurrency

        #region Rebate
        public static string Rebate { get { return EventTypeEnum.REB.ToString(); } }
        #endregion Rebate
        #region RealizedMargin
        public static string RealizedMargin { get { return EventTypeEnum.RMG.ToString(); } }
        #endregion RealizedMargin
        #region ReturnSwapAmount
        public static string ReturnSwapAmount { get { return EventTypeEnum.RSA.ToString(); } }
        #endregion ReturnSwapAmount
        // EG 20231127 [WI754] Implementation Return Swap : New
        public static string ReturnLegAmount { get { return EventCodeEnum.RLA.ToString(); } }

        #region SettlementCurrency
        public static string SettlementCurrency { get { return EventTypeEnum.SCU.ToString(); } }
        #endregion SettlementCurrency
        #region Securities
        public static string Securities { get { return EventTypeEnum.SEC.ToString(); } }
        #endregion Securities
        #region Straddle
        public static string Straddle { get { return EventTypeEnum.SDL.ToString(); } }
        #endregion Straddle
        #region Strangle
        public static string Strangle { get { return EventTypeEnum.SGL.ToString(); } }
        #endregion Strangle

        #region InitialMargin
        public static string InitialMargin { get { return EventTypeEnum.IMG.ToString(); } }
        #endregion InitialMargin

        #region UnrealizedMargin
        public static string UnrealizedMargin { get { return EventTypeEnum.UMG.ToString(); } }
        #endregion UnrealizedMargin

        #region VariationMargin
        public static string VariationMargin { get { return EventTypeEnum.VMG.ToString(); } }
        #endregion VariationMargin


        #region IsAccruedInterestAmount
        public static bool IsAccruedInterestAmount(string pEventType) { return (pEventType == AccruedInterestAmount || pEventType ==  MarketValueAccruedInterest); }
        #endregion IsAccruedInterestAmount

        #region IsPrincipalAmount
        /// <summary>
        ///  Retourne true si pEventType = PAM
        /// </summary>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        /// FI 20151228 [21660] Add
        public static bool IsPrincipalAmount(string pEventType) { return (pEventType == PrincipalAmount || pEventType == MarketValuePrincipalAmount); }
        #endregion IsPrincipalAmount

        #region IsBaseCurrency1
        public static bool IsBaseCurrency1(string pEventType) { return (pEventType == BaseCurrency1); }
        #endregion IsBaseCurrency1
        #region IsBaseCurrency2
        public static bool IsBaseCurrency2(string pEventType) { return (pEventType == BaseCurrency2); }
        #endregion IsBaseCurrency2

        #region IsBorrowingAmount
        public static bool IsBorrowingAmount(string pEventType) { return (pEventType == BorrowingAmount); }
        #endregion IsBorrowingAmount

        #region IsBrokerage
        public static bool IsBrokerage(string pEventType)
        {
            return (pEventType == Brokerage) || (pEventType == TradingBrokerage) || (pEventType == ClearingBrokerage);
        }
        #endregion IsBrokerage

        #region IsCallCurrency
        public static bool IsCallCurrency(string pEventType) { return (pEventType == CallCurrency); }
        #endregion IsCallCurrency
        #region IsCashSettlement
        public static bool IsCashSettlement(string pEventType) { return (pEventType == CashSettlement); }
        #endregion IsCashSettlement
        #region IsCleanAmount
        public static bool IsCleanAmount(string pEventType) { return (pEventType == CleanAmount); }
        #endregion IsCleanAmount
        #region IsCurrency1
        public static bool IsCurrency1(string pEventType) { return (pEventType == Currency1); }
        #endregion IsCurrency1
        #region IsCurrency2
        public static bool IsCurrency2(string pEventType) { return (pEventType == Currency2); }
        #endregion IsCurrency2

        #region IsDebtSecurityTransactionAmounts
        public static bool IsDebtSecurityTransactionAmounts(string pEventType) { return (pEventType == DebtSecurityTransactionAmounts); }
        #endregion IsDebtSecurityTransactionAmounts
        #region IsDebtSecurityTransactionAmount
        /// <summary>
        /// Retourne true si pEventType = CAM ou DAM ou GAM ou AIN ou PAM
        /// </summary>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        /// FI 20151228 [21660] Modify (add IsPrincipalAmount)
        public static bool IsDebtSecurityTransactionAmount(string pEventType)
        {
            return (IsCleanAmount(pEventType) || IsDirtyAmount(pEventType) || IsGrossAmount(pEventType) || IsAccruedInterestAmount(pEventType) || IsPrincipalAmount(pEventType));
        }
        #endregion IsDebtSecurityTransactionAmount
        #region IsDirtyAmount
        public static bool IsDirtyAmount(string pEventType) { return (pEventType == DirtyAmount); }
        #endregion IsDirtyAmount
        #region IsDeliveryAmount
        public static bool IsDeliveryAmount(string pEventType) { return (pEventType == DeliveryAmount); }
        public static bool IsHistoricalDeliveryAmount(string pEventType) { return (pEventType == HistoricalDeliveryAmount); }
        #endregion IsDeliveryAmount

        #region IsElectionSettlement
        public static bool IsElectionSettlement(string pEventType) { return (pEventType == ElectionSettlement); }
        #endregion IsElectionSettlement
        #region IsEntitlement
        public static bool IsEntitlement(string pEventType) { return (pEventType == Entitlement); }
        #endregion IsEntitlement
        #region IsExchangeTradedDerivativeAmounts
        public static bool IsExchangeTradedDerivativeAmounts(string pEventType) { return (pEventType == DebtSecurityTransactionAmounts); }
        #endregion IsExchangeTradedDerivativeAmounts
        // EG 20210318 [XXXXXX] New
        public static bool IsEqualizationPayment(string pEventType) { return (pEventType == EqualisationPayment); }

        #region IsFee
        public static bool IsFee(string pEventType) { return (pEventType == Fee); }
        #endregion IsFee
        #region IsFeeAccountingAmount
        public static bool IsFeeAccountingAmount(string pEventType) { return (pEventType == FeeAccountingAmount); }
        #endregion IsFeeAccountingAmount
        #region IsForwardPoints
        public static bool IsForwardPoints(string pEventType) { return (pEventType == ForwardPoints); }
        #endregion IsForwardPoints

        #region IsFundingAmount
        public static bool IsFundingAmount(string pEventType) { return (pEventType == FundingAmount); }
        #endregion IsFundingAmount
        #region IsGrossAmount
        public static bool IsGrossAmount(string pEventType) { return (pEventType == GrossAmount); }
        #endregion IsGrossAmount

        #region IsHistoricalGrossAmount
        // EG 20190730 New HGA - Historical GAM for PositionOpening on DebtSecurityTransaction
        public static bool IsHistoricalGrossAmount(string pEventType) { return (pEventType == HistoricalGrossAmount); }
        #endregion IsHistoricalGrossAmount

        #region IsHistoricalPremium
        public static bool IsHistoricalPremium(string pEventType) { return (pEventType == HistoricalPremium); }
        #endregion IsHistoricalPremium

        #region IsHistoricalVariationMargin
        public static bool IsHistoricalVariationMargin(string pEventType) { return (pEventType == HistoricalVariationMargin); }
        #endregion IsHistoricalVariationMargin


        #region IsInterest
        public static bool IsInterest(string pEventType) { return (pEventType == Interest); }
        #endregion IsInterest

        #region IsKnownAmount
        public static bool IsKnownAmount(string pEventType) { return (pEventType == KnownAmount); }
        #endregion IsKnownAmount

        #region IsLiquidationOptionValue
        public static bool IsLiquidationOptionValue(string pEventType) { return (pEventType == LiquidationOptionValue); }
        #endregion IsLiquidationOptionValue

        #region IsMarketValue
        // EG 20190716 [VCL : New FixedIncome] New
        public static bool IsAllMarketValue(string pEventType) 
        {
            return (pEventType == MarketValue) || (pEventType == MarketValuePrincipalAmount) || (pEventType == MarketValueAccruedInterest); 
        }
        #endregion IsMarketValue

        #region IsMarketValue
        public static bool IsMarketValue(string pEventType) { return (pEventType == MarketValue); }
        #endregion IsMarketValue
        #region IsMarketValuePrincipalAmount
        // EG 20190716 [VCL : New FixedIncome] New
        public static bool IsMarketValuePrincipalAmount(string pEventType) { return (pEventType == MarketValuePrincipalAmount); }
        #endregion IsMarketValuePrincipalAmount
        #region IsMarketValueAccruedInterest
        // EG 20190716 [VCL : New FixedIncome] New
        public static bool IsMarketValueAccruedInterest(string pEventType) { return (pEventType == MarketValueAccruedInterest); }
        #endregion IsMarketValueAccruedInterest

        #region IsNominal
        public static bool IsNominal(string pEventType) { return (pEventType == Nominal); }
        #endregion IsNominal

        #region IsPayout
        public static bool IsPayout(string pEventType) { return (pEventType == Payout); }
        #endregion IsPayout
        #region IsPhysical
        public static bool IsPhysical(string pEventType) { return (pEventType == PhysicalSettlement); }
        #endregion IsPhysical
        #region IsPremium
        public static bool IsPremium(string pEventType) { return (pEventType == Premium); }
        #endregion IsPremium
        #region IsPutCurrency
        public static bool IsPutCurrency(string pEventType) { return (pEventType == PutCurrency); }
        #endregion IsPutCurrency

        #region IsRedemptionAmount
        // EG 20190926 [Maturity Redemption] New
        public static bool IsRedemptionAmount(string pEventType) { return (pEventType == RedemptionAmount); }
        #endregion IsRedemptionAmount
        #region IsQuantity
        public static bool IsQuantity(string pEventType) { return (pEventType == Quantity); }
        #endregion IsQuantity

        #region IsRebate
        public static bool IsRebate(string pEventType) { return (pEventType == Rebate); }
        #endregion IsRebate
        #region IsRealizedMargin
        public static bool IsRealizedMargin(string pEventType) { return (pEventType == RealizedMargin); }
        #endregion IsRealizedMargin
        #region IsReturnSwapAmount
        public static bool IsReturnSwapAmount(string pEventType) { return (pEventType == ReturnSwapAmount); }
        #endregion IsReturnSwapAmount
        #region IsReturnLegAmount
        // EG 20231127 [WI754] Implementation Return Swap : New
        public static bool IsReturnLegAmount(string pEventType) { return (pEventType == ReturnLegAmount); }
        #endregion IsReturnLegAmount

        #region IsSecurities
        public static bool IsSecurities(string pEventType) { return (pEventType == Securities); }
        #endregion IsSecurities
        #region IsSettlementCurrency
        public static bool IsSettlementCurrency(string pEventType) { return (pEventType == SettlementCurrency); }
        #endregion IsSettlementCurrency

        #region IsUnrealizedMargin
        public static bool IsUnrealizedMargin(string pEventType) { return (pEventType == UnrealizedMargin); }
        #endregion IsUnrealizedMargin

        #region IsInitialMargin
        public static bool IsInitialMargin(string pEventType) { return (pEventType == InitialMargin); }
        #endregion IsInitialMargin

        #region IsVariationMargin
        public static bool IsVariationMargin(string pEventType) { return (pEventType == VariationMargin); }
        #endregion IsVariationMargin


        #region Arrêtés (CLO)
        #region AccrualInterests
        public static string AccrualInterests { get { return EventTypeEnum.AIN.ToString(); } }
        #endregion AccrualInterests
        #region EffectiveRateAmortization
        public static string EffectiveRateAmortization { get { return EventTypeEnum.ERA.ToString(); } }
        #endregion EffectiveRateAmortization
        #region FairValue
        public static string FairValue { get { return EventTypeEnum.FVL.ToString(); } }
        #endregion FairValue
        #region MarkToMarket
        public static string MarkToMarket { get { return EventTypeEnum.MTM.ToString(); } }
        #endregion MarkToMarket

        #region IsAccrualInterests
        public static bool IsAccrualInterests(string pEventType) { return (pEventType == AccrualInterests); }
        #endregion IsAccrualInterests
        #region IsEffectiveRateAmortization
        public static bool IsEffectiveRateAmortization(string pEventType) { return (pEventType == EffectiveRateAmortization); }
        #endregion IsEffectiveRateAmortization
        #region IsFairValue
        public static bool IsFairValue(string pEventType) { return (pEventType == FairValue); }
        #endregion IsFairValue
        #region IsMarkToMarket
        public static bool IsMarkToMarket(string pEventType) { return (pEventType == MarkToMarket); }
        #endregion IsMarkToMarket
        #endregion Arrêtés (CLO)

        #region Invoice
        #region Amounts
        /// <summary>
        /// Obtient AMT
        /// </summary>
        public static string Amounts { get { return EventTypeEnum.AMT.ToString(); } }
        #endregion Amounts
        #region ForeignExchangeLoss
        public static string ForeignExchangeLoss { get { return EventTypeEnum.FXL.ToString(); } }
        #endregion ForeignExchangeLoss
        #region ForeignExchangeProfit
        public static string ForeignExchangeProfit { get { return EventTypeEnum.FXP.ToString(); } }
        #endregion ForeignExchangeProfit
        #region GlobalRebate
        public static string GlobalRebate { get { return EventTypeEnum.GRB.ToString(); } }
        #endregion GlobalRebate
        #region NonAllocatedAmount
        public static string NonAllocatedAmount { get { return EventTypeEnum.NAL.ToString(); } }
        #endregion NonAllocatedAmount
        #region NetTurnOverAccountingAmount
        public static string NetTurnOverAccountingAmount { get { return EventTypeEnum.NTA.ToString(); } }
        #endregion NetTurnOverAccountingAmount
        #region NetTurnOverIssueAmount
        public static string NetTurnOverIssueAmount { get { return EventTypeEnum.NTI.ToString(); } }
        #endregion NetTurnOverIssueAmount
        #region TaxIssueAmount
        public static string TaxIssueAmount { get { return EventTypeEnum.TXI.ToString(); } }
        #endregion TaxIssueAmount
        #region TaxAccountingAmount
        public static string TaxAccountingAmount { get { return EventTypeEnum.TXA.ToString(); } }
        #endregion TaxAccountingAmount


        #region ValueAddedTax
        public static string ValueAddedTax { get { return EventTypeEnum.VAT.ToString(); } }
        #endregion ValueAddedTax
        #region IsAmounts
        public static bool IsAmounts(string pEventType) { return (pEventType == Amounts); }
        #endregion IsAmounts
        #region IsForeignExchangeLoss
        public static bool IsForeignExchangeLoss(string pEventType) { return (pEventType == ForeignExchangeLoss); }
        #endregion IsForeignExchangeLoss
        #region IsForeignExchangeProfit
        public static bool IsForeignExchangeProfit(string pEventType) { return (pEventType == ForeignExchangeProfit); }
        #endregion IsForeignExchangeProfit
        #region IsGlobalRebate
        public static bool IsGlobalRebate(string pEventType) { return (pEventType == GlobalRebate); }
        #endregion IsGlobalRebate
        #region IsNetTurnOverAccountingAmount
        public static bool IsNetTurnOverAccountingAmount(string pEventType) { return (pEventType == NetTurnOverAccountingAmount); }
        #endregion IsNetTurnOverAccountingAmount
        #region IsNetTurnOverIssueAmount
        public static bool IsNetTurnOverIssueAmount(string pEventType) { return (pEventType == NetTurnOverIssueAmount); }
        #endregion IsNetTurnOverIssueAmount
        #region IsPhysicalSettlement
        public static bool IsPhysicalSettlement(string pEventType) { return (pEventType == PhysicalSettlement); }
        #endregion IsPhysicalSettlement
        #region IsTaxAccountingAmount
        public static bool IsTaxAccountingAmount(string pEventType) { return (pEventType == TaxAccountingAmount); }
        #endregion IsTaxAccountingAmount
        #region IsTaxIssueAmount
        public static bool IsTaxIssueAmount(string pEventType) { return (pEventType == TaxIssueAmount); }
        #endregion IsTaxIssueAmount
        #region IsValueAddedTax
        public static bool IsValueAddedTax(string pEventType) { return (pEventType == ValueAddedTax); }
        #endregion IsValueAddedTax
        #endregion Invoice
        #endregion Administration Level Trade Events

        #region Calculation Level Trade Events
        #region CapBought
        public static string CapBought { get { return EventTypeEnum.CAB.ToString(); } }
        #endregion CapBought
        #region CapSold
        public static string CapSold { get { return EventTypeEnum.CAS.ToString(); } }
        #endregion CapSold

        #region CashAvailable
        public static string CashAvailable { get { return EventTypeEnum.CSA.ToString(); } }
        #endregion CashAvailable
        #region CashBalance
        public static string CashBalance { get { return EventTypeEnum.CSB.ToString(); } }
        #endregion CashBalance
        #region CashBalancePayment
        public static string CashBalancePayment { get { return EventTypeEnum.CBP.ToString(); } }
        #endregion CashBalancePayment
        #region CashUsed
        public static string CashUsed { get { return EventTypeEnum.CSU.ToString(); } }
        #endregion CashUsed
        #region CollateralAvailable
        public static string CollateralAvailable { get { return EventTypeEnum.CLA.ToString(); } }
        #endregion CollateralAvailable
        #region CollateralUsed
        public static string CollateralUsed { get { return EventTypeEnum.CLU.ToString(); } }
        #endregion CollateralUsed
        #region MarginCall
        public static string MarginCall { get { return EventTypeEnum.MGC.ToString(); } }
        #endregion MarginCall
        #region MarginRequirement
        public static string MarginRequirement { get { return EventTypeEnum.MGR.ToString(); } }
        #endregion MarginRequirement
        #region TotalMargin
        public static string TotalMargin { get { return EventTypeEnum.TMG.ToString(); } }
        #endregion TotalMargin

        #region PreviousCashBalance
        public static string PreviousCashBalance { get { return EventTypeEnum.PCB.ToString(); } }
        #endregion PreviousCashBalance

        #region UncoveredMarginRequirement
        public static string UncoveredMarginRequirement { get { return EventTypeEnum.UMR.ToString(); } }
        #endregion UncoveredMarginRequirement

        #region CashDeposit
        public static string CashDeposit { get { return EventTypeEnum.CSD.ToString(); } }
        #endregion CashDeposit
        #region CashWithdrawal
        public static string CashWithdrawal { get { return EventTypeEnum.CSW.ToString(); } }
        #endregion CashWithdrawal
        #region EquityBalance
        public static string EquityBalance { get { return EventTypeEnum.E_B.ToString(); } }
        #endregion EquityBalance
        #region EquityBalanceWithForwardCash
        public static string EquityBalanceWithForwardCash { get { return EventTypeEnum.EBF.ToString(); } }
        #endregion EquityBalanceWithForwardCash
        #region ExcessDeficit
        public static string ExcessDeficit { get { return EventTypeEnum.E_D.ToString(); } }
        #endregion ExcessDeficit
        #region ExcessDeficitWithForwardCash
        public static string ExcessDeficitWithForwardCash { get { return EventTypeEnum.EDF.ToString(); } }
        #endregion ExcessDeficitWithForwardCash
        #region ForwardCashPayment
        public static string ForwardCashPayment { get { return EventTypeEnum.FCP.ToString(); } }
        #endregion ForwardCashPayment
        #region LongOptionValue
        public static string LongOptionValue { get { return EventTypeEnum.OVL.ToString(); } }
        #endregion LongOptionValue
        #region ShortOptionValue
        public static string ShortOptionValue { get { return EventTypeEnum.OVS.ToString(); } }
        #endregion ShortOptionValue
        #region TotalAccountValue
        public static string TotalAccountValue { get { return EventTypeEnum.TAV.ToString(); } }
        #endregion TotalAccountValue
        #region UnsettledTransaction
        // PM 20150330 Add UnsettledTransaction
        public static string UnsettledTransaction { get { return EventTypeEnum.UST.ToString(); } }
        #endregion UnsettledTransaction

        #region FixedRate
        public static string FixedRate { get { return EventTypeEnum.FIX.ToString(); } }
        #endregion FixedRate
        #region FixedLeg
        public static string FixedLeg { get { return EventTypeEnum.FIX.ToString(); } }
        #endregion FixedLeg
        #region FloatingRate
        public static string FloatingRate { get { return EventTypeEnum.FLO.ToString(); } }
        #endregion FloatingRate
        #region FloorBought
        public static string FloorBought { get { return EventTypeEnum.FLB.ToString(); } }
        #endregion FloorBought
        #region FloorSold
        public static string FloorSold { get { return EventTypeEnum.FLS.ToString(); } }
        #endregion FloorSold
        #region FxCalculationAgent
        public static string FxCalculationAgent { get { return EventTypeEnum.FXC.ToString(); } }
        #endregion FxCalculationAgent
        #region FxRate
        public static string FxRate { get { return EventTypeEnum.FXR.ToString(); } }
        #endregion FxRate
        #region FxRate1
        public static string FxRate1 { get { return EventTypeEnum.FX1.ToString(); } }
        #endregion FxRate1
        #region FxRate2
        public static string FxRate2 { get { return EventTypeEnum.FX2.ToString(); } }
        #endregion FxRate2
        #region ZeroCoupon
        public static string ZeroCoupon { get { return EventTypeEnum.ZEC.ToString(); } }
        #endregion ZeroCoupon

        #region IsCapBought
        public static bool IsCapBought(string pEventType) { return (pEventType == CapBought); }
        #endregion IsCapBought
        #region IsCapSold
        public static bool IsCapSold(string pEventType) { return (pEventType == CapSold); }
        #endregion IsCapSold

        #region IsCashAvailable
        public static bool IsCashAvailable(string pEventType) { return (pEventType == CashAvailable); }
        #endregion IsCashAvailable
        #region IsCashBalance
        public static bool IsCashBalance(string pEventType) { return (pEventType == CashBalance); }
        #endregion IsCashBalance
        #region IsCashUsed
        public static bool IsCashUsed(string pEventType) { return (pEventType == CashUsed); }
        #endregion IsCashUsed
        #region IsCollateralAvailable
        public static bool IsCollateralAvailable(string pEventType) { return (pEventType == CollateralAvailable); }
        #endregion IsCollateralAvailable
        #region IsCollateralUsed
        public static bool IsCollateralUsed(string pEventType) { return (pEventType == CollateralUsed); }
        #endregion IsCollateralUsed
        #region IsLongOptionValue
        public static bool IsLongOptionValue(string pEventType) { return (pEventType == LongOptionValue); }
        #endregion IsLongOptionValue
        #region IsShortOptionValue
        public static bool IsShortOptionValue(string pEventType) { return (pEventType == ShortOptionValue); }
        #endregion IsShortOptionValue
        #region IsUnsettledTransaction
        // PM 20150330 Add IsUnsettledTransaction
        public static bool IsUnsettledTransaction(string pEventType) { return (pEventType == UnsettledTransaction); }
        #endregion IsUnsettledTransaction

        #region IsFixedRate
        public static bool IsFixedRate(string pEventType) { return (pEventType == FixedRate); }
        #endregion IsFixedRate
        #region IsFloatingRate
        public static bool IsFloatingRate(string pEventType) { return (pEventType == FloatingRate); }
        #endregion IsFloatingRate
        #region IsFloorBought
        public static bool IsFloorBought(string pEventType) { return (pEventType == FloorBought); }
        #endregion IsFloorBought
        #region IsFloorSold
        public static bool IsFloorSold(string pEventType) { return (pEventType == FloorSold); }
        #endregion IsFloorSold
        #region IsFxCustomer
        public static bool IsFxCustomer(string pEventType) { return (pEventType == FxCalculationAgent); }
        #endregion IsFxCustomer
        #region IsFxRate
        public static bool IsFxRate(string pEventType) { return (pEventType == FxRate); }
        #endregion IsFxRate
        #region IsFxRate1
        public static bool IsFxRate1(string pEventType) { return (pEventType == FxRate1); }
        #endregion IsFxRate1
        #region IsFxRate2
        public static bool IsFxRate2(string pEventType) { return (pEventType == FxRate2); }
        #endregion IsFxRate2

        #region IsZeroCoupon
        public static bool IsZeroCoupon(string pEventType) { return (pEventType == ZeroCoupon); }
        #endregion IsZeroCoupon
        #region Invoice
        #region BracketRebate
        public static string BracketRebate { get { return EventTypeEnum.BRB.ToString(); } }
        #endregion BracketRebate
        #region CapRebate
        public static string CapRebate { get { return EventTypeEnum.CRB.ToString(); } }
        #endregion CapRebate
        #region DetailRebateCumulative
        public static string DetailRebateCumulative { get { return EventTypeEnum.DRC.ToString(); } }
        #endregion DetailRebateCumulative
        #region DetailRebateUnit
        public static string DetailRebateUnit { get { return EventTypeEnum.DRU.ToString(); } }
        #endregion DetailRebateUnit
        #region GrossTurnOverAmount
        public static string GrossTurnOverAmount { get { return EventTypeEnum.GTO.ToString(); } }
        #endregion GrossTurnOverAmount
        #region NetTurnOverAmount
        public static string NetTurnOverAmount { get { return EventTypeEnum.NTO.ToString(); } }
        #endregion NetTurnOverAmount
        #region TaxAmount
        public static string TaxAmount { get { return EventTypeEnum.TXO.ToString(); } }
        #endregion TaxAmount

        #region IsAdministrativeStreamGroup
        public static bool IsAdministrativeStreamGroup(string pEventType)
        {
            return IsPeriod(pEventType);
        }
        #endregion IsAdministrativeStreamGroup
        #region IsBracketRebate
        public static bool IsBracketRebate(string pEventType) { return (pEventType == BracketRebate); }
        #endregion IsBracketRebate
        #region IsCapRebate
        public static bool IsCapRebate(string pEventType) { return (pEventType == CapRebate); }
        #endregion IsCapRebate
        #region IsDetailRebateUnit
        public static bool IsDetailRebateCumulative(string pEventType) { return (pEventType == DetailRebateCumulative); }
        #endregion IsDetailRebateCumulative
        #region IsDetailRebateUnit
        public static bool IsDetailRebateUnit(string pEventType) { return (pEventType == DetailRebateUnit); }
        #endregion IsDetailRebateUnit
        #region IsGrossTurnOverAmount
        public static bool IsGrossTurnOverAmount(string pEventType) { return (pEventType == GrossTurnOverAmount); }
        #endregion IsGrossTurnOverAmount
        #region IsInvoiceRebateAmount
        public static bool IsInvoiceRebateAmount(string pEventType)
        {
            return (IsGlobalRebate(pEventType) || IsBracketRebate(pEventType) || IsCapRebate(pEventType) ||
                IsDetailRebateCumulative(pEventType) || IsDetailRebateUnit(pEventType));
        }
        #endregion IsInvoiceRebateAmount
        #region IsTax
        public static bool IsTax(string pEventType)
        {
            return (pEventType == TaxAmount) || (pEventType == TaxIssueAmount) || (pEventType == TaxAccountingAmount);
        }
        #endregion IsTax

        #region IsInvoiceTurnOverAmount
        public static bool IsInvoiceTurnOverAmount(string pEventType)
        {
            return (IsGrossTurnOverAmount(pEventType) || IsNetTurnOverAmount(pEventType));
        }
        #endregion IsInvoiceTurnOverAmount
        #region IsNetTurnOverAmount
        public static bool IsNetTurnOverAmount(string pEventType) { return (pEventType == NetTurnOverAmount); }
        #endregion IsNetTurnOverAmount
        #region IsTaxAmount
        public static bool IsTaxAmount(string pEventType) { return (pEventType == TaxAmount); }
        #endregion IsTaxAmount
        #endregion Invoice

        #endregion Calculation Level Trade Events

        #region Description Level Trade Events
        #region Exercise dates
        #region American
        public static string American { get { return EventTypeEnum.AME.ToString(); } }
        #endregion American
        #region Bermuda
        public static string Bermuda { get { return EventTypeEnum.BRM.ToString(); } }
        #endregion Bermuda


        #region CommodityAmount
        public static string CommodityAmount { get { return EventTypeEnum.AMT.ToString(); } }
        #endregion CommodityAmount
        #region CommodityElectricity
        public static string CommodityElectricity { get { return EventTypeEnum.ELC.ToString(); } }
        #endregion CommodityElectricity
        #region CommodityGas
        public static string CommodityGas { get { return EventTypeEnum.GAS.ToString(); } }
        #endregion CommodityGas
        // EG 20221201 [25639] [WI484] New
        public static string CommodityEnvironmental { get { return EventTypeEnum.ENV.ToString(); } }

        #region European
        public static string European { get { return EventTypeEnum.EUR.ToString(); } }
        #endregion European

        #region IsAmerican
        public static bool IsAmerican(string pEventType) { return pEventType == American; }
        #endregion IsAmerican
        #region IsBermuda
        public static bool IsBermuda(string pEventType) { return pEventType == Bermuda; }
        #endregion IsBermuda
        #region IsEuropean
        public static bool IsEuropean(string pEventType) { return pEventType == European; }
        #endregion IsEuropean
        #endregion Exercise dates
        #region Triggers (TRG eventCode)
        #region Above
        public static string Above { get { return EventTypeEnum.ABO.ToString(); } }
        #endregion Above
        #region Below
        public static string Below { get { return EventTypeEnum.BEL.ToString(); } }
        #endregion Below
        #region DownIn
        public static string DownIn { get { return EventTypeEnum.DWI.ToString(); } }
        #endregion DownIn
        #region DownNoTouch
        public static string DownNoTouch { get { return EventTypeEnum.DWN.ToString(); } }
        #endregion DownNoTouch
        #region DownOut
        public static string DownOut { get { return EventTypeEnum.DWO.ToString(); } }
        #endregion DownOut
        #region DownTouch
        public static string DownTouch { get { return EventTypeEnum.DWT.ToString(); } }
        #endregion DownTouch
        #region UpIn
        public static string UpIn { get { return EventTypeEnum.UPI.ToString(); } }
        #endregion UpIn
        #region UpNoTouch
        public static string UpNoTouch { get { return EventTypeEnum.UPN.ToString(); } }
        #endregion UpNoTouch
        #region UpOut
        public static string UpOut { get { return EventTypeEnum.UPO.ToString(); } }
        #endregion UpOut
        #region UpTouch
        public static string UpTouch { get { return EventTypeEnum.UPT.ToString(); } }
        #endregion UpTouch

        #region IsAbove
        public static bool IsAbove(string pEventType) { return (pEventType == Above); }
        #endregion IsAbove
        #region IsBelow
        public static bool IsBelow(string pEventType) { return (pEventType == Below); }
        #endregion IsBelow
        #region IsDownIn
        public static bool IsDownIn(string pEventType) { return (pEventType == DownIn); }
        #endregion IsDownIn
        #region IsDownNoTouch
        public static bool IsDownNoTouch(string pEventType) { return (pEventType == DownNoTouch); }
        #endregion IsDownNoTouch
        #region IsDownTouch
        public static bool IsDownTouch(string pEventType) { return (pEventType == DownTouch); }
        #endregion IsDownTouch
        #region IsDownOut
        public static bool IsDownOut(string pEventType) { return (pEventType == DownOut); }
        #endregion IsDownOut
        #region IsUpIn
        public static bool IsUpIn(string pEventType) { return (pEventType == UpIn); }
        #endregion IsUpIn
        #region IsUpNoTouch
        public static bool IsUpNoTouch(string pEventType) { return (pEventType == UpNoTouch); }
        #endregion IsUpNoTouch
        #region IsUpOut
        public static bool IsUpOut(string pEventType) { return (pEventType == UpOut); }
        #endregion IsUpOut
        #region IsUpTouch
        public static bool IsUpTouch(string pEventType) { return (pEventType == UpTouch); }
        #endregion IsUpTouch
        #endregion Triggers (TRG eventCode)
        #region Effective rate (EFR eventCode)
        #region LoanOrDeposit
        public static string LoanOrDeposit { get { return EventTypeEnum.LOD.ToString(); } }
        #endregion LoanOrDeposit
        #region SecuritiesPremiumDiscount
        public static string SecuritiesPremiumDiscount { get { return EventTypeEnum.SPD.ToString(); } }
        #endregion SecuritiesPremiumDiscount

        #region IsLoanOrDeposit
        public static bool IsLoanOrDeposit(string pEventType) { return pEventType == LoanOrDeposit; }
        #endregion IsLoanOrDeposit
        #region IsSecuritiesPremiumDiscount
        public static bool IsSecuritiesPremiumDiscount(string pEventType) { return pEventType == SecuritiesPremiumDiscount; }
        #endregion IsSecuritiesPremiumDiscount
        #endregion  Effective rate (EFR eventCode)
        #endregion Description Level Trade Events

        #region Group Level Trade Events
        #region Trade/Product (TRD/PRD eventCodes)
        #region Date
        public static string Date { get { return EventTypeEnum.DAT.ToString(); } }
        #endregion Date

        #region IsDate
        public static bool IsDate(string pEventType) { return (pEventType == Date); }
        #endregion IsDate
        #endregion Trade/Product (TRD/PRD eventCodes)
        #region Exercise/Abandon (EXE/ABN eventCodes)
        #region Multiple
        public static string Multiple { get { return EventTypeEnum.MUL.ToString(); } }
        #endregion Multiple
        #region Partiel
        public static string Partiel { get { return EventTypeEnum.PAR.ToString(); } }
        #endregion Partiel
        #region Total
        public static string Total { get { return EventTypeEnum.TOT.ToString(); } }
        #endregion Total

        #region IsMultiple
        public static bool IsMultiple(string pEventType) { return pEventType == Multiple; }
        #endregion IsMultiple
        #region IsPartiel
        public static bool IsPartiel(string pEventType) { return pEventType == Partiel; }
        #endregion IsPartiel
        #region IsTotal
        public static bool IsTotal(string pEventType) { return pEventType == Total; }
        #endregion IsTotal
        #endregion Exercise/Abandon (EXE/ABN eventCodes)
        #region Optional provisions (PRO eventCode)
        #region CancelableProvision
        public static string CancelableProvision { get { return EventTypeEnum.CAN.ToString(); } }
        #endregion CancelableProvision
        #region ExtendibleProvision
        public static string ExtendibleProvision { get { return EventTypeEnum.EXT.ToString(); } }
        #endregion ExtendibleProvision
        #region MandatoryEarlyTerminationProvision
        public static string MandatoryEarlyTerminationProvision { get { return EventTypeEnum.MET.ToString(); } }
        #endregion MandatoryEarlyTerminationProvision
        #region OptionalEarlyTerminationProvision
        public static string OptionalEarlyTerminationProvision { get { return EventTypeEnum.OET.ToString(); } }
        #endregion OptionalEarlyTerminationProvision
        #region StepUpProvision
        public static string StepUpProvision { get { return EventTypeEnum.SUP.ToString(); } }
        #endregion StepUpProvision

        #region IsCancelableProvision
        public static bool IsCancelableProvision(string pEventType) { return (pEventType == CancelableProvision); }
        #endregion IsCancelableProvision
        #region IsExtendibleProvision
        public static bool IsExtendibleProvision(string pEventType) { return (pEventType == ExtendibleProvision); }
        #endregion IsExtendibleProvision
        #region IsMandatoryEarlyTerminationProvision
        public static bool IsMandatoryEarlyTerminationProvision(string pEventType)
        {
            return (pEventType == MandatoryEarlyTerminationProvision);
        }
        #endregion IsMandatoryEarlyTerminationProvision
        #region IsOptionalEarlyTerminationProvision
        public static bool IsOptionalEarlyTerminationProvision(string pEventType) { return (pEventType == OptionalEarlyTerminationProvision); }
        #endregion IsOptionalEarlyTerminationProvision
        #region IsStepUpProvision
        public static bool IsStepUpProvision(string pEventType) { return (pEventType == StepUpProvision); }
        #endregion IsStepUpProvision
        #endregion Optional provisions (PRO eventCode)
        #region Non detailed instruments
        #region BulletPayment
        public static string BulletPayment { get { return EventTypeEnum.BUL.ToString(); } }
        #endregion BulletPayment
        #region ForwardRateAgreement
        public static string ForwardRateAgreement { get { return EventTypeEnum.FRA.ToString(); } }
        #endregion ForwardRateAgreement
        #region InterestRateSwap
        public static string InterestRateSwap { get { return EventTypeEnum.IRS.ToString(); } }
        #endregion InterestRateSwap
        #region TermDeposit
        public static string TermDeposit { get { return EventTypeEnum.TED.ToString(); } }
        #endregion TermDeposit

        #region IsBulletPayment
        public static bool IsBulletPayment(string pEventType) { return (pEventType == BulletPayment); }
        #endregion IsBulletPayment

        #region IsCashBalanceAmounts
        public static bool IsCashBalanceAmounts(string pEventType)
        {
            return (IsCashAvailable(pEventType) || IsCashUsed(pEventType) ||
                    IsCollateralAvailable(pEventType) || IsCollateralUsed(pEventType));
        }
        #endregion IsCashBalanceAmounts
        #region IsCashBalancePayment
        public static bool IsCashBalancePayment(string pEventType) { return (pEventType == CashBalancePayment); }
        #endregion IsCashBalancePayment
        #region IsCashDeposit
        public static bool IsCashDeposit(string pEventType) { return (pEventType == CashDeposit); }
        #endregion IsCashDeposit
        #region IsCashWithdrawal
        public static bool IsCashWithdrawal(string pEventType) { return (pEventType == CashWithdrawal); }
        #endregion IsCashWithdrawal

        #region IsEquityBalance
        public static bool IsEquityBalance(string pEventType) { return (pEventType == EquityBalance); }
        #endregion IsEquityBalance
        #region IsExcessDeficit
        public static bool IsExcessDeficit(string pEventType) { return (pEventType == ExcessDeficit); }
        #endregion IsExcessDeficit
        #region IsForwardCashPayment
        public static bool IsForwardCashPayment(string pEventType) { return (pEventType == ForwardCashPayment); }
        #endregion IsForwardCashPayment
        #region IsTotalAccountValue
        public static bool IsTotalAccountValue(string pEventType) { return (pEventType == TotalAccountValue); }
        #endregion IsTotalAccountValue

        #region IsForwardRateAgreement
        public static bool IsForwardRateAgreement(string pEventType) { return (pEventType == ForwardRateAgreement); }
        #endregion IsForwardRateAgreement
        #region IsInterestRateSwap
        public static bool IsInterestRateSwap(string pEventType) { return (pEventType == InterestRateSwap); }
        #endregion IsInterestRateSwap
        #region IsMarginCall
        public static bool IsMarginCall(string pEventType) { return (pEventType == MarginCall); }
        #endregion IsMarginCall
        #region IsMarginRequirement
        public static bool IsMarginRequirement(string pEventType) { return (pEventType == MarginRequirement); }
        #endregion IsMarginRequirement
        #region IsPreviousCashBalance
        public static bool IsPreviousCashBalance(string pEventType) { return (pEventType == PreviousCashBalance); }
        #endregion IsPreviousCashBalance

        #region IsTotalMargin
        public static bool IsTotalMargin(string pEventType) { return (pEventType == TotalMargin); }
        #endregion IsTotalMargin
        #region IsUncoveredMarginRequirement
        public static bool IsUncoveredMarginRequirement(string pEventType) { return (pEventType == UncoveredMarginRequirement); }
        #endregion IsUncoveredMarginRequirement

        #region IsTermDeposit
        public static bool IsTermDeposit(string pEventType) { return (pEventType == TermDeposit); }
        #endregion IsTermDeposit
        #endregion Non detailed instruments
        #region Cap/Floor/CapAndFloor(Collar,Corridor,Exotic)
        #region Cap
        public static string Cap { get { return EventTypeEnum.CAP.ToString(); } }
        #endregion Cap
        #region CapFloorMultiStrikeSchedule
        //20090827 PL COL --> MCF
        //public static string Collar { get { return EventTypeEnum.COL.ToString(); } }
        public static string CapFloorMultiStrikeSchedule { get { return EventTypeEnum.MCF.ToString(); } }
        #endregion CapFloorMultiStrikeSchedule
        #region Floor
        public static string Floor { get { return EventTypeEnum.FLR.ToString(); } }
        #endregion Floor

        #region IsCap
        public static bool IsCap(string pEventType) { return (pEventType == Cap); }
        #endregion IsCap
        #region IsCapFloorMultiStrikeSchedule
        //20090827 PL COL --> MCF
        public static bool IsCapFloorMultiStrikeSchedule(string pEventType) { return (pEventType == CapFloorMultiStrikeSchedule); }
        #endregion IsCapFloorMultiStrikeSchedule
        #region IsFloor
        public static bool IsFloor(string pEventType) { return (pEventType == Floor); }
        #endregion IsFloor
        #endregion Cap/Floor/Collar

        #region Return Swap Leg
        #region Open
        public static string Open { get { return EventTypeEnum.OPN.ToString(); } }
        #endregion Open
        #region Term
        public static string Term { get { return EventTypeEnum.TRM.ToString(); } }
        #endregion Term

        #region IsOpen
        public static bool IsOpen(string pEventType) { return (pEventType == Open); }
        #endregion IsOpen
        #endregion Return Swap Leg

        #region EquityOption
        #region AveragingIn
        public static string AveragingIn { get { return EventTypeEnum.AVI.ToString(); } }
        #endregion AveragingIn
        #region AveragingOut
        public static string AveragingOut { get { return EventTypeEnum.AVO.ToString(); } }
        #endregion AveragingOut
        #region Basket
        public static string Basket { get { return EventTypeEnum.BSK.ToString(); } }
        #endregion Basket
        #region Bond
        public static string Bond { get { return EventTypeEnum.BND.ToString(); } }
        #endregion Bond
        #region ConvertibleBond
        public static string ConvertibleBond { get { return EventTypeEnum.CBD.ToString(); } }
        #endregion ConvertibleBond
        #region Features
        public static string Features { get { return EventTypeEnum.FEA.ToString(); } }
        #endregion Features
        #region Index
        public static string Index { get { return EventTypeEnum.IND.ToString(); } }
        #endregion Index
        #region LookBackIn
        public static string LookBackIn { get { return EventTypeEnum.LBI.ToString(); } }
        #endregion LookBackIn
        #region LookBackOut
        public static string LookBackOut { get { return EventTypeEnum.LBO.ToString(); } }
        #endregion LookBackOut
        #region Share
        public static string Share { get { return EventTypeEnum.SHR.ToString(); } }
        #endregion Share
        #region SingleUnderlyer
        public static string SingleUnderlyer { get { return EventTypeEnum.SUL.ToString(); } }
        #endregion SingleUnderlyer
        #region Underlyer
        public static string Underlyer { get { return EventTypeEnum.UNL.ToString(); } }
        #endregion Underlyer
        #region Vanilla
        public static string Vanilla { get { return EventTypeEnum.VAN.ToString(); } }
        #endregion Vanilla

        #region IsAveragingIn
        public static bool IsAveragingIn(string pEventType) { return (pEventType == AveragingIn); }
        #endregion IsAveragingIn
        #region IsAveragingOut
        public static bool IsAveragingOut(string pEventType) { return (pEventType == AveragingOut); }
        #endregion IsAveragingOut
        #region IsBasket
        public static bool IsBasket(string pEventType) { return (pEventType == Basket); }
        #endregion IsBasket
        #region IsBond
        public static bool IsBond(string pEventType) { return (pEventType == Bond); }
        #endregion IsBond
        #region IsBondPayment
        public static bool IsBondPayment(string pEventType) { return (pEventType == BondPayment); }
        #endregion IsBondPayment

        #region IsFeatures
        public static bool IsFeatures(string pEventType) { return (pEventType == Features); }
        #endregion IsFeatures
        #region IsIndex
        public static bool IsIndex(string pEventType) { return (pEventType == Index); }
        #endregion IsIndex
        #region IsLookBackIn
        public static bool IsLookBackIn(string pEventType) { return (pEventType == LookBackIn); }
        #endregion IsLookBackIn
        #region IsLookBackOut
        public static bool IsLookBackOut(string pEventType) { return (pEventType == LookBackOut); }
        #endregion IsLookBackOut
        #region IsSingleUnderlyer
        public static bool IsSingleUnderlyer(string pEventType) { return (pEventType == SingleUnderlyer); }
        #endregion IsSingleUnderlyer
        #region IsShare
        public static bool IsShare(string pEventType) { return (pEventType == Share); }
        #endregion IsShare
        #region IsUnderlyer
        public static bool IsUnderlyer(string pEventType) { return (pEventType == Underlyer); }
        #endregion IsUnderlyer
        #region IsVanilla
        public static bool IsVanilla(string pEventType) { return (pEventType == Vanilla); }
        #endregion IsVanilla
        #region IsUnderlyerComponent
        public static bool IsUnderlyerComponent(string pEventType)
        {
            return IsShare(pEventType) || IsIndex(pEventType) || IsBond(pEventType);
        }
        #endregion IsUnderlyerComponent

        #endregion EquityOption

        #region Exchange Traded Derivative
        #region Call
        public static string Call { get { return EventTypeEnum.CAL.ToString(); } }
        #endregion Call
        #region Future
        /// <summary>
        /// EVENTTYPE = 'FUT'
        /// </summary>
        public static string Future { get { return EventTypeEnum.FUT.ToString(); } }
        #endregion Future
        #region Put
        public static string Put { get { return EventTypeEnum.PUT.ToString(); } }
        #endregion Put

        #region IsCall
        public static bool IsCall(string pEventType) { return (pEventType == Call); }
        #endregion IsCall
        #region IsFuture
        public static bool IsFuture(string pEventType) { return (pEventType == Future); }
        #endregion IsFuture
        #region IsPut
        public static bool IsPut(string pEventType) { return (pEventType == Put); }
        #endregion IsPut
        #endregion Exchange Traded Derivative

        #region FX Spot/Forward/Swap
        #region Deliverable
        public static string Deliverable { get { return EventTypeEnum.DLV.ToString(); } }
        #endregion Deliverable
        #region NonDeliverable
        public static string NonDeliverable { get { return EventTypeEnum.NDV.ToString(); } }
        #endregion NonDeliverable

        #region IsDeliverable
        public static bool IsDeliverable(string pEventType) { return (pEventType == Deliverable); }
        #endregion IsDeliverable
        #region IsNonDeliverable
        public static bool IsNonDeliverable(string pEventType) { return (pEventType == NonDeliverable); }
        #endregion IsNonDeliverable
        #endregion FX Spot/Forward/Swap

        #region FX American digital option
        #region DoubleNoTouch
        public static string DoubleNoTouch { get { return EventTypeEnum.DNT.ToString(); } }
        #endregion DoubleNoTouch
        #region DoubleNoTouchExtinguishing
        public static string DoubleNoTouchExtinguishing { get { return EventTypeEnum.DNE.ToString(); } }
        #endregion DoubleNoTouchExtinguishing
        #region DoubleNoTouchResurrecting
        public static string DoubleNoTouchResurrecting { get { return EventTypeEnum.DNR.ToString(); } }
        #endregion DoubleNoTouchResurrecting
        #region DoubleTouch
        public static string DoubleTouch { get { return EventTypeEnum.DTC.ToString(); } }
        #endregion DoubleTouch
        #region DoubleTouchBoundary
        public static string DoubleTouchBoundary { get { return EventTypeEnum.DTB.ToString(); } }
        #endregion DoubleTouchBoundary
        #region DoubleTouchLimit
        public static string DoubleTouchLimit { get { return EventTypeEnum.DTL.ToString(); } }
        #endregion DoubleTouchLimit
        #region DoubleTouchLimitExtinguishing
        public static string DoubleTouchLimitExtinguishing { get { return EventTypeEnum.DLE.ToString(); } }
        #endregion DoubleTouchLimitExtinguishing
        #region DoubleTouchLimitResurrecting
        public static string DoubleTouchLimitResurrecting { get { return EventTypeEnum.DLR.ToString(); } }
        #endregion DoubleTouchLimitResurrecting
        #region NoTouch
        public static string NoTouch { get { return EventTypeEnum.NTC.ToString(); } }
        #endregion NoTouch
        #region NoTouchKnockIn
        public static string NoTouchKnockIn { get { return EventTypeEnum.NKI.ToString(); } }
        #endregion NoTouchKnockIn
        #region NoTouchKnockOut
        public static string NoTouchKnockOut { get { return EventTypeEnum.NKO.ToString(); } }
        #endregion NoTouchKnockOut
        #region Touch
        public static string Touch { get { return EventTypeEnum.TCH.ToString(); } }
        #endregion Touch
        #region TouchKnockIn
        public static string TouchKnockIn { get { return EventTypeEnum.TKI.ToString(); } }
        #endregion TouchKnockIn
        #region TouchKnockOut
        public static string TouchKnockOut { get { return EventTypeEnum.TKO.ToString(); } }
        #endregion TouchKnockOut

        #region IsDoubleNoTouch
        public static bool IsDoubleNoTouch(string pEventType) { return (pEventType == DoubleNoTouch); }
        #endregion IsDoubleNoTouch
        #region IsDoubleNoTouchExtinguishing
        public static bool IsDoubleNoTouchExtinguishing(string pEventType) { return (pEventType == DoubleNoTouchExtinguishing); }
        #endregion IsDoubleNoTouchExtinguishing
        #region IsDoubleNoTouchResurrecting
        public static bool IsDoubleNoTouchResurrecting(string pEventType) { return (pEventType == DoubleNoTouchResurrecting); }
        #endregion IsDoubleNoTouchResurrecting
        #region IsDoubleTouch
        public static bool IsDoubleTouch(string pEventType) { return (pEventType == DoubleTouch); }
        #endregion IsDoubleTouch
        #region IsDoubleTouchBoundary
        public static bool IsDoubleTouchBoundary(string pEventType) { return (pEventType == DoubleTouchBoundary); }
        #endregion IsDoubleTouchBoundary
        #region IsDoubleTouchLimit
        public static bool IsDoubleTouchLimit(string pEventType) { return (pEventType == DoubleTouchLimit); }
        #endregion IsDoubleTouchLimit
        #region IsDoubleTouchLimitExtinguishing
        public static bool IsDoubleTouchLimitExtinguishing(string pEventType) { return (pEventType == DoubleTouchLimitExtinguishing); }
        #endregion IsDoubleTouchLimitExtinguishing
        #region IsDoubleTouchLimitResurrecting
        public static bool IsDoubleTouchLimitResurrecting(string pEventType) { return (pEventType == DoubleTouchLimitResurrecting); }
        #endregion IsDoubleTouchLimitResurrecting
        #region IsNoTouch
        public static bool IsNoTouch(string pEventType) { return (pEventType == NoTouch); }
        #endregion IsNoTouch
        #region IsNoTouchKnockIn
        public static bool IsNoTouchKnockIn(string pEventType) { return (pEventType == NoTouchKnockIn); }
        #endregion IsNoTouchKnockIn
        #region IsNoTouchKnockOut
        public static bool IsNoTouchKnockOut(string pEventType) { return (pEventType == NoTouchKnockOut); }
        #endregion IsNoTouchKnockOut
        #region IsTouch
        public static bool IsTouch(string pEventType) { return (pEventType == Touch); }
        #endregion IsTouch
        #region IsTouchKnockIn
        public static bool IsTouchKnockIn(string pEventType) { return (pEventType == TouchKnockIn); }
        #endregion IsTouchKnockIn
        #region IsTouchKnockOut
        public static bool IsTouchKnockOut(string pEventType) { return (pEventType == TouchKnockOut); }
        #endregion IsTouchKnockOut
        #endregion FX American digital option
        #region FX European digital option
        #region AboveAssetOrNothing
        public static string AboveAssetOrNothing { get { return EventTypeEnum.ABA.ToString(); } }
        #endregion AboveAssetOrNothing
        #region AboveExtinguishing
        public static string AboveExtinguishing { get { return EventTypeEnum.ABE.ToString(); } }
        #endregion AboveExtinguishing
        #region AboveGap
        public static string AboveGap { get { return EventTypeEnum.ABG.ToString(); } }
        #endregion AboveGap
        #region AboveResurrecting
        public static string AboveResurrecting { get { return EventTypeEnum.ABR.ToString(); } }
        #endregion AboveResurrecting
        #region AboveKnockIn
        public static string AboveKnockIn { get { return EventTypeEnum.AKI.ToString(); } }
        #endregion AboveKnockIn
        #region AboveKnockOut
        public static string AboveKnockOut { get { return EventTypeEnum.AKO.ToString(); } }
        #endregion AboveKnockOut
        #region BelowAssetOrNothing
        public static string BelowAssetOrNothing { get { return EventTypeEnum.BEA.ToString(); } }
        #endregion BelowAssetOrNothing
        #region BelowExtinguishing
        public static string BelowExtinguishing { get { return EventTypeEnum.BEE.ToString(); } }
        #endregion BelowExtinguishing
        #region BelowGap
        public static string BelowGap { get { return EventTypeEnum.BEG.ToString(); } }
        #endregion BelowGap
        #region BelowKnockIn
        public static string BelowKnockIn { get { return EventTypeEnum.BKI.ToString(); } }
        #endregion BelowKnockIn
        #region BelowKnockOut
        public static string BelowKnockOut { get { return EventTypeEnum.BKO.ToString(); } }
        #endregion BelowKnockOut
        #region BelowResurrecting
        public static string BelowResurrecting { get { return EventTypeEnum.BER.ToString(); } }
        #endregion BelowResurrecting
        #region Range
        public static string Range { get { return EventTypeEnum.RNG.ToString(); } }
        #endregion Range

        #region IsAboveAssetOrNothing
        public static bool IsAboveAssetOrNothing(string pEventType) { return (pEventType == AboveAssetOrNothing); }
        #endregion IsAboveAssetOrNothing
        #region IsAboveExtinguishing
        public static bool IsAboveExtinguishing(string pEventType) { return (pEventType == AboveExtinguishing); }
        #endregion IsAboveExtinguishing
        #region IsAboveGap
        public static bool IsAboveGap(string pEventType) { return (pEventType == AboveGap); }
        #endregion IsAboveGap
        #region IsAboveKnockIn
        public static bool IsAboveKnockIn(string pEventType) { return (pEventType == AboveKnockIn); }
        #endregion IsAboveKnockIn
        #region IsAboveKnockOut
        public static bool IsAboveKnockOut(string pEventType) { return (pEventType == AboveKnockOut); }
        #endregion IsAboveKnockOut
        #region IsAboveResurrecting
        public static bool IsAboveResurrecting(string pEventType) { return (pEventType == AboveResurrecting); }
        #endregion IsAboveResurrecting
        #region IsBelowAssetOrNothing
        public static bool IsBelowAssetOrNothing(string pEventType) { return (pEventType == BelowAssetOrNothing); }
        #endregion IsBelowAssetOrNothing
        #region IsBelowExtinguishing
        public static bool IsBelowExtinguishing(string pEventType) { return (pEventType == BelowExtinguishing); }
        #endregion IsBelowExtinguishing
        #region IsBelowGap
        public static bool IsBelowGap(string pEventType) { return (pEventType == BelowGap); }
        #endregion IsBelowGap
        #region IsBelowKnockIn
        public static bool IsBelowKnockIn(string pEventType) { return (pEventType == BelowKnockIn); }
        #endregion IsBelowKnockIn
        #region IsBelowKnockOut
        public static bool IsBelowKnockOut(string pEventType) { return (pEventType == BelowKnockOut); }
        #endregion IsBelowKnockOut
        #region IsBelowResurrecting
        public static bool IsBelowResurrecting(string pEventType) { return (pEventType == BelowResurrecting); }
        #endregion IsBelowResurrecting
        #region IsRange
        public static bool IsRange(string pEventType) { return (pEventType == Range); }
        #endregion IsRange
        #endregion FX European digital option
        #region FX Average option
        #region AverageRate
        public static string AverageRate { get { return EventTypeEnum.ARA.ToString(); } }
        #endregion AverageRate
        #region AverageStrike
        public static string AverageStrike { get { return EventTypeEnum.AST.ToString(); } }
        #endregion AverageStrike

        #region IsAverageRate
        public static bool IsAverageRate(string pEventType) { return (pEventType == AverageRate); }
        #endregion IsAverageRate
        #region IsAverageStrike
        public static bool IsAverageStrike(string pEventType) { return (pEventType == AverageStrike); }
        #endregion IsAverageStrike
        #endregion FX Average option
        #region FX Barrier option
        #region CappedCall
        public static string CappedCall { get { return EventTypeEnum.CAC.ToString(); } }
        #endregion CappedCall
        #region FlooredPut
        public static string FlooredPut { get { return EventTypeEnum.FLP.ToString(); } }
        #endregion FlooredPut
        #region KnockIn
        public static string KnockIn { get { return EventTypeEnum.KNI.ToString(); } }
        #endregion KnockIn
        #region KnockOut
        public static string KnockOut { get { return EventTypeEnum.KNO.ToString(); } }
        #endregion KnockOut
        #region RebateDownIn
        public static string RebateDownIn { get { return EventTypeEnum.RDI.ToString(); } }
        #endregion RebateDownIn
        #region RebateDownOut
        public static string RebateDownOut { get { return EventTypeEnum.RDO.ToString(); } }
        #endregion RebateDownOut
        #region RebateUpIn
        public static string RebateUpIn { get { return EventTypeEnum.RUI.ToString(); } }
        #endregion RebateUpIn
        #region RebateUpOut
        public static string RebateUpOut { get { return EventTypeEnum.RUO.ToString(); } }
        #endregion RebateUpOut

        #region IsCappedCall
        public static bool IsCappedCall(string pEventType) { return (pEventType == CappedCall); }
        #endregion IsCappedCall
        #region IsFlooredPut
        public static bool IsFlooredPut(string pEventType) { return (pEventType == FlooredPut); }
        #endregion IsFlooredPut
        #region IsKnockIn
        public static bool IsKnockIn(string pEventType) { return (pEventType == KnockIn); }
        #endregion IsKnockIn
        #region IsKnockOut
        public static bool IsKnockOut(string pEventType) { return (pEventType == KnockOut); }
        #endregion IsKnockOut
        #region IsKnockInPlus
        public static bool IsKnockInPlus(string pEventType)
        {
            return (IsKnockIn(pEventType) || IsDownIn(pEventType) || IsUpIn(pEventType));
        }
        #endregion IsKnockInPlus
        #region IsKnockOutPlus
        public static bool IsKnockOutPlus(string pEventType)
        {
            return (IsKnockOut(pEventType) || IsDownOut(pEventType) || IsUpOut(pEventType));
        }
        #endregion IsKnockOutPlus
        #region IsKnockPlus
        public static bool IsKnockPlus(string pEventType)
        {
            return IsKnockInPlus(pEventType) || IsKnockOutPlus(pEventType);
        }
        #endregion IsKnockPlus
        #region IsRebateDownIn
        public static bool IsRebateDownIn(string pEventType) { return (pEventType == RebateDownIn); }
        #endregion IsRebateDownIn
        #region IsRebateDownOut
        public static bool IsRebateDownOut(string pEventType) { return (pEventType == RebateDownOut); }
        #endregion IsRebateDownOut
        #region IsRebateUpIn
        public static bool IsRebateUpIn(string pEventType) { return (pEventType == RebateUpIn); }
        #endregion IsRebateUpIn
        #region IsRebateUpOut
        public static bool IsRebateUpOut(string pEventType) { return (pEventType == RebateUpOut); }
        #endregion IsRebateUpOut
        #endregion FX Barrier option
        #region FX Simple option
        #region AmericanSimpleOption
        public static string AmericanSimpleOption { get { return EventTypeEnum.ASO.ToString(); } }
        #endregion AmericanSimpleOption
        #region BermudaSimpleOption
        public static string BermudaSimpleOption { get { return EventTypeEnum.BSO.ToString(); } }
        #endregion BermudaSimpleOption
        #region EuropeanSimpleOption
        public static string EuropeanSimpleOption { get { return EventTypeEnum.ESO.ToString(); } }
        #endregion EuropeanSimpleOption

        #region IsAmericanSimpleOption
        public static bool IsAmericanSimpleOption(string pEventType) { return (pEventType == AmericanSimpleOption); }
        #endregion IsAmericanSimpleOption
        #region IsBermudaSimmpleOption
        public static bool IsBermudaSimpleOption(string pEventType) { return (pEventType == BermudaSimpleOption); }
        #endregion IsBermudaSimmpleOption
        #region IsEuropeanSimpleOption
        public static bool IsEuropeanSimpleOption(string pEventType) { return (pEventType == EuropeanSimpleOption); }
        #endregion IsEuropeanSimpleOption
        #endregion FX Simple option

        #region Swaption
        #region IsStraddle
        public static bool IsStraddle(string pEventType) { return (pEventType == Straddle); }
        #endregion IsStraddle
        #region IsStrangle
        public static bool IsStrangle(string pEventType) { return (pEventType == Strangle); }
        #endregion IsStrangle
        #region IsSwaption
        public static bool IsSwaption(string pEventType)
        {
            return (IsStraddle(pEventType) || IsStrangle(pEventType) || IsRegular(pEventType));
        }
        #endregion IsSwaption
        #endregion Swaption
        #region Invoice
        #region Period
        public static string Period { get { return EventTypeEnum.PER.ToString(); } }
        #endregion Period

        #region IsPeriod
        public static bool IsPeriod(string pEventType) { return (pEventType == Period); }
        #endregion IsPeriod
        #endregion Invoice

        #region Overnight
        public static string Overnight { get { return EventTypeEnum.O_N.ToString(); } }
        #endregion Overnight
        #region Regular
        public static string Regular { get { return EventTypeEnum.REG.ToString(); } }
        #endregion Regular
        #region MarginRequirementRatio
        public static string MarginRequirementRatio { get { return EventTypeEnum.MGF.ToString(); } }
        #endregion MarginRequirementRatio


        #region IsOvernight
        public static bool IsOvernight(string pEventType) { return (pEventType == Overnight); }
        #endregion IsOvernight
        #region IsRegular
        public static bool IsRegular(string pEventType) { return (pEventType == Regular); }
        #endregion IsRegular
        #region IsTerm
        public static bool IsTerm(string pEventType) { return (pEventType == Term); }
        #endregion IsTerm

        #region IsMarginRequirementRatio
        public static bool IsMarginRequirementRatio(string pEventType) { return (pEventType == MarginRequirementRatio); }
        #endregion IsMarginRequirementRatio


        #endregion Group Level Trade Events

        #region IsAdministrationLevel
        public static bool IsAdministrationLevel(string pEventType)
        {
            return (IsCurrency(pEventType) || IsBrokerage(pEventType) || IsFee(pEventType)) || IsCashSettlement(pEventType) ||
                IsForwardPoints(pEventType) || IsInterest(pEventType) || IsKnownAmount(pEventType) || IsNominal(pEventType) ||
                IsPayout(pEventType) || IsPremium(pEventType) || IsRebate(pEventType) || IsDailyClosing(pEventType) ||
                IsKnownAmount(pEventType) || IsSecurities(pEventType) || IsQuantity(pEventType) ||
                IsGrossTurnOverAmount(pEventType) || IsTax(pEventType) || IsFeeAccountingAmount(pEventType) ||
                IsNetTurnOverAmount(pEventType) || IsNetTurnOverIssueAmount(pEventType) || IsNetTurnOverAccountingAmount(pEventType) ||
                IsValueAddedTax(pEventType) || IsForeignExchangeLoss(pEventType) || IsForeignExchangeProfit(pEventType) ||
                IsDebtSecurityTransactionAmount(pEventType) ||
                //20110518 FI Add 
                IsMarginRequirement(pEventType) ||
                //EG 20140111 Add 
                IsVariationMargin(pEventType) || IsRealizedMargin(pEventType) || IsUnrealizedMargin(pEventType) || IsLiquidationOptionValue(pEventType) ||
                IsReturnSwapAmount(pEventType) || IsInitialMargin(pEventType) || IsSettlementCurrency(pEventType) ||
                //PL 20170703 Add DVA for TER/DVA, ... [23264]
                IsDeliveryAmount(pEventType) || IsHistoricalDeliveryAmount(pEventType) ||
                //PL 20170703 Add PHY for DLV/PHY [23264]
                IsPhysical(pEventType);
        }

        #region IsBaseCurrency
        /// EG 20150302 Add BaseCurrency (CFD Forex)
        public static bool IsBaseCurrency(string pEventType) { return (pEventType == BaseCurrency) || (IsBaseCurrency1(pEventType) || IsBaseCurrency2(pEventType)); }
        #endregion IsSideRateCurrency
        #region IsQuotedCurrency
        /// EG 20150302 New (CFD Forex)
        public static bool IsQuotedCurrency(string pEventType) { return (pEventType == QuotedCurrency); }
        #endregion IsQuotedCurrency
        #region IsCallPutCurrency
        public static bool IsCallPutCurrency(string pEventType) { return (IsCallCurrency(pEventType) || IsPutCurrency(pEventType)); }
        #endregion IsCallPutCurrency
        #region IsCurrency
        public static bool IsCurrency(string pEventType)
        {
            return IsBaseCurrency(pEventType) || IsExchangeRateCurrency(pEventType) ||
                IsCallPutCurrency(pEventType) || IsSettlementCurrency(pEventType);
        }
        #endregion IsCurrency
        #endregion IsAdministrationLevel
        #region IsCalculationLevel
        public static bool IsCalculationLevel(string pEventType)
        {
            return (IsCapFloorLeg(pEventType) || IsFixedRate(pEventType) || IsFloatingRate(pEventType) || IsZeroCoupon(pEventType) ||
                IsFxRatePlus(pEventType)) || IsSettlementCurrency(pEventType) || IsKnownAmount(pEventType) ||
                IsInvoiceTurnOverAmount(pEventType) || IsInvoiceRebateAmount(pEventType);
        }

        #region IsCapLeg
        public static bool IsCapLeg(string pEventType) { return (IsCapBought(pEventType) || IsCapSold(pEventType)); }
        #endregion IsCapLeg
        #region IsCapFloorLeg
        public static bool IsCapFloorLeg(string pEventType) { return IsCapLeg(pEventType) || IsFloorLeg(pEventType); }
        #endregion IsCapFloorLeg
        #region IsFloorLeg
        public static bool IsFloorLeg(string pEventType) { return (IsFloorBought(pEventType) || IsFloorSold(pEventType)); }
        #endregion IsFloorLeg
        #region IsFxRatePlus
        public static bool IsFxRatePlus(string pEventType)
        {
            return (IsFxRate(pEventType) || IsFxRate1(pEventType) || IsFxRate2(pEventType) || IsFxCustomer(pEventType));
        }
        #endregion IsQuotationPlus
        #endregion IsCalculationLevel
        #region IsDailyClosingLevel
        public static bool IsDailyClosingLevel(string pEventType)
        {
            return IsAccrual(pEventType) || IsEffectiveRateAmortization(pEventType) ||
                IsFairValue(pEventType) || IsMarkToMarket(pEventType) || IsPremium(pEventType) ||
                IsForwardPoints(pEventType);
        }

        #region IsAccrual
        public static bool IsAccrual(string pEventType) { return (IsAccrualInterests(pEventType)); }
        #endregion IsAccrual

        #endregion IsDailyClosingLevel
        #region IsDescriptionLevel
        public static bool IsDescriptionLevel(string pEventType)
        {
            return (IsExerciseDates(pEventType) || IsSimpleTrigger(pEventType) || IsDownUpIn(pEventType) || IsDownUpOut(pEventType) ||
                IsDownUpTouch(pEventType) || IsDownUpNoTouch(pEventType) || IsEffectiveRate(pEventType) ||
                IsPeriod(pEventType) || IsCashSettlement(pEventType) || IsRepoDuration(pEventType));
        }
        #region IsDownUpIn
        public static bool IsDownUpIn(string pEventType)
        {
            return IsDownIn(pEventType) || IsUpIn(pEventType);
        }
        #endregion IsDownUpIn
        #region IsDownUpInOut
        public static bool IsDownUpInOut(string pEventType)
        {
            return IsDownUpIn(pEventType) || IsDownUpOut(pEventType);
        }
        #endregion IsDownUpInOut
        #region IsDownUpNoTouch
        public static bool IsDownUpNoTouch(string pEventType)
        {
            return IsDownNoTouch(pEventType) || IsUpNoTouch(pEventType);
        }
        #endregion IsDownUpNoTouch
        #region IsDownUpOut
        public static bool IsDownUpOut(string pEventType)
        {
            return IsDownOut(pEventType) || IsUpOut(pEventType);
        }
        #endregion IsDownUpOut
        #region IsDownUpTouch
        public static bool IsDownUpTouch(string pEventType)
        {
            return IsDownTouch(pEventType) || IsUpTouch(pEventType);
        }
        #endregion IsDownUpTouch
        #region IsRepoDuration
        public static bool IsRepoDuration(string pEventType)
        {
            return (IsOvernight(pEventType) || IsOpen(pEventType) || IsTerm(pEventType));
        }
        #endregion IsRepoDuration
        #region IsEffectiveRate
        public static bool IsEffectiveRate(string pEventType)
        {
            return IsLoanOrDeposit(pEventType) || IsSecuritiesPremiumDiscount(pEventType);
        }
        #endregion IsEffectiveRate
        #region IsExerciseDates
        public static bool IsExerciseDates(string pEventType)
        {
            return IsAmerican(pEventType) || IsBermuda(pEventType) || IsEuropean(pEventType);
        }
        #endregion IsExerciseDates
        #region IsSimpleTrigger
        public static bool IsSimpleTrigger(string pEventType)
        {
            return IsAbove(pEventType) || IsBelow(pEventType);
        }
        #endregion IsSimpleTrigger
        #region IsTrigger
        public static bool IsTrigger(string pEventType)
        {
            return IsSimpleTrigger(pEventType) || IsDownTouch(pEventType) || IsDownNoTouch(pEventType) ||
                IsUpTouch(pEventType) || IsUpNoTouch(pEventType);
        }
        #endregion IsTrigger

        #endregion IsDescriptionLevel
        #region IsGroupLevel
        public static bool IsGroupLevel(string pEventType)
        {
            return IsAboveOrBelowIn(pEventType) || IsAboveOrBelowOut(pEventType) ||
                IsAmericanTrigger(pEventType) || IsEuropeanTrigger(pEventType) ||
                IsExtinguishing(pEventType) || IsProvision(pEventType) ||
                IsResurrecting(pEventType) || IsCappedCallOrFlooredPut(pEventType) ||
                IsRebateBarrier(pEventType) || IsExchangeRateCurrency(pEventType) ||
                IsExercise(pEventType) || IsDate(pEventType) ||
                IsSimpleOption(pEventType) || IsProvision(pEventType) ||
                IsProduct(pEventType) || IsFloatingRate(pEventType) ||
                IsFixedRate(pEventType) || IsNominal(pEventType) || IsQuantity(pEventType) || IsZeroCoupon(pEventType) ||
                IsKnockPlus(pEventType) || IsKnownAmount(pEventType) ||
                IsPeriod(pEventType) || IsSwaption(pEventType) || IsDebtSecurityTransactionAmounts(pEventType) ||
                IsExchangeTradedDerivativeAmounts(pEventType) || IsCall(pEventType) || IsPut(pEventType) || IsFuture(pEventType);
        }
        #region IsAboveKnock
        public static bool IsAboveKnock(string pEventType)
        {
            return IsAboveKnockIn(pEventType) || IsAboveKnockOut(pEventType);
        }
        #endregion IsAboveKnock
        #region IsAboveOrBelowIn
        public static bool IsAboveOrBelowIn(string pEventType)
        {
            return IsAboveKnockIn(pEventType) || IsBelowKnockIn(pEventType);
        }
        #endregion IsAboveOrBelowIn
        #region IsAboveOrBelowOut
        public static bool IsAboveOrBelowOut(string pEventType)
        {
            return IsAboveKnockOut(pEventType) || IsBelowKnockOut(pEventType);
        }
        #endregion IsAboveOrBelowOut
        #region IsAboveOrBelowPlus
        public static bool IsAboveOrBelowPlus(string pEventType)
        {
            return IsAbovePlus(pEventType) || IsBelowPlus(pEventType);
        }
        #endregion IsAboveOrBelowPlus
        #region IsAbovePlus
        public static bool IsAbovePlus(string pEventType)
        {
            return IsAbove(pEventType) || IsAboveAssetOrNothing(pEventType) || IsAboveGap(pEventType) ||
                IsAboveResurrecting(pEventType) || IsAboveExtinguishing(pEventType);
        }
        #endregion IsAbovePlus

        #region IsAmericanTrigger
        public static bool IsAmericanTrigger(string pEventType)
        {
            return IsDoubleNoTouchPlus(pEventType) || IsDoubleTouch(pEventType) || IsTouchNoTouch(pEventType) ||
                IsDoubleTouchLimitPlus(pEventType) || IsDoubleTouchBoundary(pEventType);
        }
        #endregion IsAmericanTrigger
        #region IsBelowPlus
        public static bool IsBelowPlus(string pEventType)
        {
            return IsBelow(pEventType) || IsBelowAssetOrNothing(pEventType) || IsBelowGap(pEventType) ||
                IsBelowResurrecting(pEventType) || IsBelowExtinguishing(pEventType);
        }
        #endregion IsBelowPlus
        #region IsCapFloor
        public static bool IsCapFloor(string pEventType)
        {
            //20090827 PL COL --> MCF
            return IsCap(pEventType) || IsFloor(pEventType) || IsCapFloorMultiStrikeSchedule(pEventType);
        }
        #endregion IsCapFloor
        #region IsCappedCallOrFlooredPut
        public static bool IsCappedCallOrFlooredPut(string pEventType)
        {
            return IsCappedCall(pEventType) || IsFlooredPut(pEventType);
        }
        #endregion IsFxCappedFlooredBarrier

        #region IsExercise
        public static bool IsExercise(string pEventType)
        {
            return IsPartiel(pEventType) || IsTotal(pEventType) || IsMultiple(pEventType);
        }
        #endregion IsExercise

        #region IsDailyClosing
        public static bool IsDailyClosing(string pEventType)
        {
            return IsAccrual(pEventType) || IsEffectiveRateAmortization(pEventType) ||
                IsFairValue(pEventType) || IsMarkToMarket(pEventType);
        }
        #endregion IsDailyClosing

        #region IsDoubleNoTouchPlus
        public static bool IsDoubleNoTouchPlus(string pEventType)
        {
            return IsDoubleNoTouch(pEventType) || IsDoubleNoTouchExtinguishing(pEventType) || IsDoubleNoTouchResurrecting(pEventType);
        }
        #endregion IsDoubleNoTouchPlus
        #region IsDoubleTouchLimitPlus
        public static bool IsDoubleTouchLimitPlus(string pEventType)
        {
            return IsDoubleTouchLimit(pEventType) || IsDoubleTouchLimitExtinguishing(pEventType) ||
                IsDoubleTouchLimitResurrecting(pEventType);
        }
        #endregion IsDoubleTouchLimitPlus
        #region IsExchangeRateCurrency
        public static bool IsExchangeRateCurrency(string pEventType)
        {
            return (IsCurrency1(pEventType) || IsCurrency2(pEventType));
        }
        #endregion IsExchangeRateCurrency
        #region IsExtinguishing
        public static bool IsExtinguishing(string pEventType)
        {
            return IsAboveExtinguishing(pEventType) || IsBelowExtinguishing(pEventType) ||
                IsDoubleNoTouchExtinguishing(pEventType) || IsDoubleTouchLimitExtinguishing(pEventType);
        }
        #endregion IsExtinguishing
        #region IsEuropeanTrigger
        public static bool IsEuropeanTrigger(string pEventType)
        {
            return IsAboveOrBelowPlus(pEventType) || IsAboveKnock(pEventType) || IsRange(pEventType);
        }
        #endregion IsEuropeanTrigger
        #region IsFx
        public static bool IsFx(string pEventType)
        {
            return IsDeliverable(pEventType) || IsNonDeliverable(pEventType);
        }
        #endregion IsFx
        #region IsFxAverage
        public static bool IsFxAverage(string pEventType)
        {
            return IsAverageRate(pEventType) || IsAverageStrike(pEventType);
        }
        #endregion IsFx

        #region IsProduct
        public static bool IsProduct(string pEventType)
        {
            return IsCapFloor(pEventType) || IsForwardRateAgreement(pEventType) || IsBulletPayment(pEventType) ||
                IsInterestRateSwap(pEventType) || IsTermDeposit(pEventType) || IsFx(pEventType) || IsPeriod(pEventType)
                || IsSwaption(pEventType) || IsMarginRequirement(pEventType);
        }
        #endregion IsProduct
        #region IsProvision
        public static bool IsProvision(string pEventType)
        {
            return IsOptionalEarlyTerminationProvision(pEventType) || IsCancelableProvision(pEventType) ||
                IsExtendibleProvision(pEventType) || IsMandatoryEarlyTerminationProvision(pEventType) ||
                IsStepUpProvision(pEventType);
        }
        #endregion IsProvision
        #region IsRebateBarrier
        public static bool IsRebateBarrier(string pEventType)
        {
            return (IsRebateIn(pEventType) || IsRebateOut(pEventType));
        }
        #endregion IsRebateBarrier
        #region IsRebateIn
        public static bool IsRebateIn(string pEventType)
        {
            return IsRebateDownIn(pEventType) || IsRebateUpIn(pEventType);
        }
        #endregion IsFxRebateBarrierKnockIn
        #region IsRebateOut
        public static bool IsRebateOut(string pEventType)
        {
            return IsRebateDownOut(pEventType) || IsRebateUpOut(pEventType);
        }
        #endregion IsRebateOut
        #region IsResurrecting
        public static bool IsResurrecting(string pEventType)
        {
            return IsAboveResurrecting(pEventType) || IsBelowResurrecting(pEventType) ||
                IsDoubleNoTouchResurrecting(pEventType) || IsDoubleTouchLimitResurrecting(pEventType);
        }
        #endregion IsResurrecting
        #region IsSimpleOption
        public static bool IsSimpleOption(string pEventType)
        {
            return IsAmericanSimpleOption(pEventType) || IsBermudaSimpleOption(pEventType) || IsEuropeanSimpleOption(pEventType);
        }
        #endregion IsSimpleOption
        #region IsTouchNoTouch
        public static bool IsTouchNoTouch(string pEventType)
        {
            return IsNoTouch(pEventType) || IsTouch(pEventType);
        }
        #endregion IsTouchNoTouch
        #region IsTouchOrNoTouchIn
        public static bool IsTouchOrNoTouchIn(string pEventType)
        {
            return IsTouchKnockIn(pEventType) || IsNoTouchKnockIn(pEventType);
        }
        #endregion IsTouchOrNoTouchIn
        #region IsTouchOrNoTouchOut
        public static bool IsTouchOrNoTouchOut(string pEventType)
        {
            return IsTouchKnockOut(pEventType) || IsNoTouchKnockOut(pEventType);
        }
        #endregion IsTouchOrNoTouchOut
        #endregion IsGroupLevel

        #region Other
        #region PhysicalSettlement
        public static string PhysicalSettlement { get { return EventTypeEnum.PHY.ToString(); } }
        #endregion PhysicalSettlement
        #endregion Other
    }
    #endregion EventTypeFunc
    #region ExchangeTypeFunc
    public sealed class ExchangeTypeFunc
    {
        #region AccountingCurrencyEarDate
        public static string AccountingCurrencyEarDate { get { return ExchangeTypeEnum.ACU_EARDATE.ToString(); } }
        #endregion AccountingCurrencyEarDate
        #region AccountingCurrencyEventDate
        public static string AccountingCurrencyEventDate { get { return ExchangeTypeEnum.ACU_EVENTDATE.ToString(); } }
        #endregion AccountingCurrencyEventDate
        #region AccountingCurrencyTransactDate
        public static string AccountingCurrencyTransactDate { get { return ExchangeTypeEnum.ACU_TRANSACTDATE.ToString(); } }
        #endregion AccountingCurrencyTransactDate
        #region AccountingCurrencyValueDate
        public static string AccountingCurrencyValueDate { get { return ExchangeTypeEnum.ACU_VALUEDATE.ToString(); } }
        #endregion AccountingCurrencyValueDate
        #region Currency1EarDate
        public static string Currency1EarDate { get { return ExchangeTypeEnum.CU1_EARDATE.ToString(); } }
        #endregion Currency1EarDate
        #region Currency1EventDate
        public static string Currency1EventDate { get { return ExchangeTypeEnum.CU1_EVENTDATE.ToString(); } }
        #endregion Currency1EventDate
        #region Currency1TransactDate
        public static string Currency1TransactDate { get { return ExchangeTypeEnum.CU1_TRANSACTDATE.ToString(); } }
        #endregion Currency1TransactDate
        #region Currency1ValueDate
        public static string Currency1ValueDate { get { return ExchangeTypeEnum.CU1_VALUEDATE.ToString(); } }
        #endregion Currency1ValueDate
        #region Currency2EarDate
        public static string Currency2EarDate { get { return ExchangeTypeEnum.CU2_EARDATE.ToString(); } }
        #endregion Currency2EarDate
        #region Currency2EventDate
        public static string Currency2EventDate { get { return ExchangeTypeEnum.CU2_EVENTDATE.ToString(); } }
        #endregion Currency2EventDate
        #region Currency2TransactDate
        public static string Currency2TransactDate { get { return ExchangeTypeEnum.CU2_TRANSACTDATE.ToString(); } }
        #endregion Currency2TransactDate
        #region Currency2ValueDate
        public static string Currency2ValueDate { get { return ExchangeTypeEnum.CU2_VALUEDATE.ToString(); } }
        #endregion Currency2ValueDate
        #region FlowCurrency
        public static string FlowCurrency { get { return ExchangeTypeEnum.FCU.ToString(); } }
        #endregion FlowCurrency

        #region IsAccountingCurrencyEarDate
        public static bool IsAccountingCurrencyEarDate(string pExchangeType) { return (pExchangeType == AccountingCurrencyEarDate); }
        #endregion IsAccountingCurrencyEarDate
        #region IsAccountingCurrencyEventDate
        public static bool IsAccountingCurrencyEventDate(string pExchangeType) { return (pExchangeType == AccountingCurrencyEventDate); }
        #endregion IsAccountingCurrencyEventDate
        #region IsAccountingCurrencyTransactDate
        public static bool IsAccountingCurrencyTransactDate(string pExchangeType) { return (pExchangeType == AccountingCurrencyTransactDate); }
        #endregion IsAccountingCurrencyTransactDate
        #region IsAccountingCurrencyValueDate
        public static bool IsAccountingCurrencyValueDate(string pExchangeType) { return (pExchangeType == AccountingCurrencyValueDate); }
        #endregion IsAccountingCurrencyValueDate
        #region IsCurrency1EarDate
        public static bool IsCurrency1EarDate(string pExchangeType) { return (pExchangeType == Currency1EarDate); }
        #endregion IsCurrency1EarDate
        #region IsCurrency1EventDate
        public static bool IsCurrency1EventDate(string pExchangeType) { return (pExchangeType == Currency1EventDate); }
        #endregion IsCurrency1EventDate
        #region IsCurrency1TransactDate
        public static bool IsCurrency1TransactDate(string pExchangeType) { return (pExchangeType == Currency1TransactDate); }
        #endregion IsCurrency1TransactDate
        #region IsCurrency1ValueDate
        public static bool IsCurrency1ValueDate(string pExchangeType) { return (pExchangeType == Currency1ValueDate); }
        #endregion IsCurrency1ValueDate
        #region IsCurrency2EarDate
        public static bool IsCurrency2TodayDate(string pExchangeType) { return (pExchangeType == Currency2EarDate); }
        #endregion IsCurrency2EarDate
        #region IsCurrency2EventDate
        public static bool IsCurrency2EventDate(string pExchangeType) { return (pExchangeType == Currency2EventDate); }
        #endregion IsCurrency2EventDate
        #region IsCurrency2TransactDate
        public static bool IsCurrency2TransactDate(string pExchangeType) { return (pExchangeType == Currency2TransactDate); }
        #endregion IsCurrency2TransactDate
        #region IsCurrency2ValueDate
        public static bool IsCurrency2ValueDate(string pExchangeType) { return (pExchangeType == Currency2ValueDate); }
        #endregion IsCurrency2ValueDate
        #region IsFlowCurrency
        public static bool IsFlowCurrency(string pExchangeType) { return (pExchangeType == FlowCurrency); }
        #endregion IsFlowCurrency
    }
    #endregion ExchangeTypeFunc

    #region FlowTypeFunc
    public sealed class FlowTypeFunc
    {
        #region FlowPaid
        public static string FlowPaid { get { return FlowTypeEnum.PAY.ToString(); } }
        #endregion FlowPaid
        #region FlowReceived
        public static string FlowReceived { get { return FlowTypeEnum.REC.ToString(); } }
        #endregion FlowReceived

        #region IsFlowPaid
        public static bool IsFlowPaid(string pFlowType) { return (pFlowType == FlowPaid); }
        #endregion IsFlowPaid
        #region IsFlowReceived
        public static bool IsFlowReceived(string pFlowType) { return (pFlowType == FlowReceived); }
        #endregion IsFlowReceived
    }
    #endregion FlowTypeFunc

    #region LevelEventFunc
    public sealed class LevelEventFunc
    {
        #region AdministrationTradeEvent
        // EG [22922] Test si OPP alors pas de contrôle sur EVENTTYPE
        public static bool IsAdministrationTradeEvent(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsAdministrationLevel(pEventCode) && EventTypeFunc.IsAdministrationLevel(pEventType)) 
                   || 
                   (EventCodeFunc.IsOtherPartyPayment(pEventCode));
        }
        #endregion AdministrationTradeEvent
        #region CalculationTradeEvent
        public static bool IsCalculationTradeEvent(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsCalculationLevel(pEventCode) &&
                   EventTypeFunc.IsCalculationLevel(pEventType);
        }
        #endregion CalculationTradeEvent
        #region DescriptionTradeEvent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        public static bool IsDescriptionTradeEvent(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsDescriptionLevel(pEventCode) &&
                   EventTypeFunc.IsDescriptionLevel(pEventType);
        }
        #endregion DescriptionTradeEvent

        #region DailyClosingTradeEvent
        /// <summary>
        /// Retourne true si {pEventCode} = CLO
        /// </summary>
        /// <param name="pEventCode"></param>
        /// <param name="pEventType"></param>
        /// <returns></returns>
        public static bool IsDailyClosingTradeEvent(string pEventCode)
        {
            return EventCodeFunc.IsDailyClosing(pEventCode);
        }
        #endregion DailyClosingTradeEvent

        #region GroupTradeEvent
        public static bool IsGroupTradeEvent(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsGroupLevel(pEventCode) &&
                   EventTypeFunc.IsGroupLevel(pEventType);
        }
        #endregion DescriptionTradeEvent
        #region ResetTradeEvent
        public static bool IsResetTradeEvent(string pEventCode)
        {
            return EventCodeFunc.IsResetPlus(pEventCode);
        }
        #endregion ResetTradeEvent

        #region IsAssetFxRateEvent
        public static bool IsAssetFxRateEvent(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsReset(pEventCode) &&
                (EventTypeFunc.IsFxRate(pEventType) || EventTypeFunc.IsFxRate1(pEventType) || EventTypeFunc.IsFxRate2(pEventType) || EventTypeFunc.IsSettlementCurrency(pEventType));
        }
        #endregion IsAssetFxRateEvent
        #region IsAssetRateIndexEvent
        public static bool IsAssetRateIndexEvent(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsResetSelf(pEventCode) || EventCodeFunc.IsCalculationPeriod(pEventCode)) && EventTypeFunc.IsFloatingRate(pEventType);
        }
        #endregion IsAssetRateIndexEvent
        #region IsAssetETDEvent
        public static bool IsAssetETDEvent(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsETDOption(pEventCode) ||
                    EventCodeFunc.IsETDFuture(pEventCode) ||
                    EventTypeFunc.IsSettlementCurrency(pEventType));
        }
        #endregion IsAssetETDEvent

        #region IsEventDet_CapFloorSchedule
        public static bool IsEventDet_CapFloorSchedule(string pEventCode, string pEventType)
        {

            return (EventCodeFunc.IsCalculationPeriod(pEventCode) && EventTypeFunc.IsCapFloorLeg(pEventType));
        }
        #endregion IsEventDet_DayCountFraction
        #region IsEventDet_CurrencyPair
        public static bool IsEventDet_CurrencyPair(string pEventCode, string pEventType)
        {
            return EventCodeFunc.IsReset(pEventCode) && EventTypeFunc.IsCurrency(pEventType);
        }
        #endregion IsEventDet_CurrencyPair
        #region IsEventDet_DayCountFraction
        public static bool IsEventDet_DayCountFraction(string pEventCode, string pEventType)
        {

            return (EventCodeFunc.IsCalculationPeriod(pEventCode) &&
                (EventTypeFunc.IsFixedRate(pEventType) || EventTypeFunc.IsFloatingRate(pEventType)));

        }
        #endregion IsEventDet_DayCountFraction
        #region IsEventDet_ExchangeRate
        public static bool IsEventDet_ExchangeRate(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsStartTermination(pEventCode) && EventTypeFunc.IsExchangeRateCurrency(pEventType)) ||
                (EventCodeFunc.IsDepreciableAmount(pEventCode) && EventTypeFunc.IsForwardPoints(pEventType));
        }
        #endregion IsEventDet_ExchangeRate
        #region IsEventDet_ExchangeRatePremium
        public static bool IsEventDet_ExchangeRatePremium(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) && EventTypeFunc.IsPremium(pEventType));
        }
        #endregion IsEventDet_ExchangeRatePremium
        #region IsEventDet_FixingRate
        public static bool IsEventDet_FixingRate(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsReset(pEventCode) && EventTypeFunc.IsFxRatePlus(pEventType));
        }
        #endregion IsEventDet_FixingRate
        #region IsEventDet_MarkToMarket
        public static bool IsEventDet_MarkToMarket(string pEventType)
        {
            return EventTypeFunc.IsMarkToMarket(pEventType);
        }
        #endregion IsEventDet_MarkToMarket
        #region IsEventDet_PremiumQuote
        public static bool IsEventDet_PremiumQuote(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsLinkedProductPayment(pEventCode) && EventTypeFunc.IsPremium(pEventType));
        }
        #endregion IsEventDet_PremiumQuote
        #region IsEventDet_SettlementRate
        public static bool IsEventDet_SettlementRate(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsStartTermination(pEventCode) && EventTypeFunc.IsSettlementCurrency(pEventType));
        }
        #endregion IsEventDet_SettlementRate
        #region IsEventDet_SideRate
        public static bool IsEventDet_SideRate(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsStartTermination(pEventCode) && EventTypeFunc.IsBaseCurrency(pEventType));
        }
        #endregion IsEventDet_SideRate
        #region IsEventDet_StrikePrice
        public static bool IsEventDet_StrikePrice(string pEventCode, string pEventType)
        {
            return (EventCodeFunc.IsStartTermination(pEventCode) && EventTypeFunc.IsCallPutCurrency(pEventType));
        }
        #endregion IsEventDet_StrikePrice
        #region IsEventDet_TriggerRate
        public static bool IsEventDet_TriggerRate(string pEventCode)
        {
            return (EventCodeFunc.IsBarrier(pEventCode) || EventCodeFunc.IsTrigger(pEventCode));
        }
        #endregion IsEventDet_TriggerRate
        #region IsEventDet_Fungible
        public static bool IsEventDet_Fungible(string pEventType)
        {
            return EventTypeFunc.IsVariationMargin(pEventType) ||
                   EventTypeFunc.IsUnrealizedMargin(pEventType) ||
                   EventTypeFunc.IsRealizedMargin(pEventType) ||
                   EventTypeFunc.IsSettlementCurrency(pEventType) ||
                   EventTypeFunc.IsLiquidationOptionValue(pEventType);
        }
        #endregion IsEventDet_Fungible
    }
    #endregion LevelEventFunc

    

    #region StatusCalculFunc
    public sealed class StatusCalculFunc
    {
        #region Methods
        #region Get StatusCalcul
        #region Calculated
        public static string Calculated { get { return StatusCalculEnum.CALC.ToString(); } }
        #endregion Calculated
        #region CalculatedAndRevisable
        public static string CalculatedAndRevisable { get { return StatusCalculEnum.CALCREV.ToString(); } }
        #endregion CalculatedAndRevisable
        #region ToCalculate
        public static string ToCalculate { get { return StatusCalculEnum.TOCALC.ToString(); } }
        #endregion ToCalculate
        #endregion Get StatusCalcul

        #region Is StatusCalcul
        #region IsCalculated
        public static bool IsCalculated(string pStatusCalcul)
        {
            return (Calculated == pStatusCalcul);
        }
        #endregion IsCalculated
        #region IsCalculatedAndRevisable
        public static bool IsCalculatedAndRevisable(string pStatusCalcul)
        {
            return (CalculatedAndRevisable == pStatusCalcul);
        }
        #endregion IsCalculatedAndRevisable
        #region IsCalculatedPlus
        public static bool IsCalculatedPlus(string pStatusCalcul)
        {
            return IsCalculated(pStatusCalcul) || IsCalculatedAndRevisable(pStatusCalcul);
        }
        #endregion IsCalculatedPlus
        #region IsToCalculate
        public static bool IsToCalculate(string pStatusCalcul)
        {
            return (ToCalculate == pStatusCalcul);
        }
        #endregion IsToCalculate
        #endregion Is StatusCalcul
        #endregion Methods
    }
    #endregion StatusCalculFunc

    /// <summary>
    /// 
    /// </summary>
    public sealed class StringToEnum
    {

        /// <summary>
        /// Convertie une string en BusinessDayConventionEnum
        /// <para>Retourne NotApplicable si la valeur est non définie dans l'énumération</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static BusinessDayConventionEnum BusinessDayConvention(string pString)
        {
            return (BusinessDayConventionEnum)BusinessDayConvention(pString, BusinessDayConventionEnum.NotApplicable);
        }
        /// <summary>
        /// Convertie une string en BusinessDayConventionEnum
        /// <para>Retourne pDefaultValue si la valeur est non définie dans l'énumération</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static BusinessDayConventionEnum BusinessDayConvention(string pString, BusinessDayConventionEnum pDefaultValue)
        {
            return (BusinessDayConventionEnum)Parse(pString, pDefaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static DayCountFractionEnum DayCountFraction(string pString)
        {
            return DayCountFraction(pString, DayCountFractionEnum.DCF30360);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static DayCountFractionEnum DayCountFraction(string pString, DayCountFractionEnum pDefaultValue)
        {
            DayCountFractionEnum dcf = pDefaultValue;
            DayCountFractionEnum dcfEnum = new DayCountFractionEnum();
            FieldInfo[] flds = dcfEnum.GetType().GetFields();
            foreach (FieldInfo fld in flds)
            {
                object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if ((0 != attributes.GetLength(0)) && (pString == ((XmlEnumAttribute)attributes[0]).Name))
                {
                    dcf = (DayCountFractionEnum)fld.GetValue(pString);
                    break;
                }
            }
            return dcf;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static DayTypeEnum DayType(string pString)
        {
            return (DayTypeEnum)DayType(pString, DayTypeEnum.Calendar);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static DayTypeEnum DayType(string pString, DayTypeEnum pDefaultValue)
        {
            return (DayTypeEnum)Parse(pString, pDefaultValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static PeriodEnum Period(string pString)
        {
            return (PeriodEnum)Period(pString, PeriodEnum.D);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static PeriodEnum Period(string pString, PeriodEnum pDefaultValue)
        {
            return (PeriodEnum)Parse(pString, pDefaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <returns></returns>
        public static RollConventionEnum RollConvention(string pString)
        {
            return (RollConventionEnum)RollConvention(pString, RollConventionEnum.NONE);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static RollConventionEnum RollConvention(string pString, RollConventionEnum pDefaultValue)
        {
            //return (RollConventionEnum)Parse(pString, pDefaultValue);
            //20080604 PL/EG correction de cette méthode qui était erronée.
            RollConventionEnum rollConvention = pDefaultValue;
            RollConventionEnum rollConventionEnum = new RollConventionEnum();
            FieldInfo[] flds = rollConventionEnum.GetType().GetFields();
            foreach (FieldInfo fld in flds)
            {
                object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if ((0 != attributes.GetLength(0)) && (pString == ((XmlEnumAttribute)attributes[0]).Name))
                {
                    rollConvention = (RollConventionEnum)fld.GetValue(pString);
                    break;
                }
            }
            return rollConvention;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRollConvention"></param>
        /// <returns></returns>
        public static int DayRollConvention(RollConventionEnum pRollConvention)
        {
            int roll = 0;
            try
            {
                RollConventionEnum rollEnum = new RollConventionEnum();
                FieldInfo fld = rollEnum.GetType().GetField(pRollConvention.ToString());
                if (null != fld)
                {
                    object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                    if (0 != attributes.GetLength(0))
                        roll = Convert.ToInt32(((XmlEnumAttribute)attributes[0]).Name);
                }
            }
            catch { }
            return roll;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pVersion"></param>
        /// <returns></returns>
        public static EfsMLDocumentVersionEnum EfsMLVersion(string pVersion)
        {

            EfsMLDocumentVersionEnum ret = EfsMLDocumentVersionEnum.Version20;
            EfsMLDocumentVersionEnum efsversion = new EfsMLDocumentVersionEnum();

            FieldInfo[] flds = efsversion.GetType().GetFields();
            foreach (FieldInfo fld in flds)
            {
                object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if ((0 != attributes.GetLength(0)) && (pVersion == ((XmlEnumAttribute)attributes[0]).Name))
                {
                    ret = (EfsMLDocumentVersionEnum)fld.GetValue(efsversion);
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        ///  Parse la valeur {pString} dans l'enum {pDefaultValue}
        ///  <para>Retourne {pDefaultValue} si la valeur est non définie dans l'énumération</para>
        /// </summary>
        /// <param name="pString"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static object Parse(string pString, object pDefaultValue)
        {
            Type tDefaultValue = pDefaultValue.GetType();
            if (System.Enum.IsDefined(tDefaultValue, pString))
                return System.Enum.Parse(tDefaultValue, pString, true);
            else
                return pDefaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExerciseStyle"></param>
        /// <returns></returns>
        public static ExerciseStyleEnum ConvertToExerciseStyleEnum(string pExerciseStyle)
        {

            ExerciseStyleEnum ret = default;
            switch (pExerciseStyle)
            {
                case "0":
                    ret = ExerciseStyleEnum.European;
                    break;
                case "1":
                    ret = ExerciseStyleEnum.American;
                    break;
                default:
                    break;
            }
            return ret;
        }

        /// <summary>
        /// Convertie une donnée en PosRequestTypeEnum
        /// </summary>
        /// <param name="pPosRequestType"></param>
        /// <returns></returns>
        /// EG 20160304 New
        /// FI 20160819 [22364] Modify
        // EG 20190926 [Maturity Redemption] Upd
        public static Cst.PosRequestTypeEnum ConvertToPosRequestTypeEnum(string pData)
        {

            Cst.PosRequestTypeEnum ret = default;
            switch (pData)
            {
                case "ClosingDayAllocMissing":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_AllocMissing;
                    break;
                case "ClosingDayControl":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_ControlGroupLevel;
                    break;
                case "ClosingDayEndOfDayMarket":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_EOD_MarketGroupLevel;
                    break;
                case "ClosingDayEntryControl":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_EntryGroupLevel;
                    break;
                case "ClosingDayRemoveAllocControl": // FI 20160819 [22364] add
                    ret = Cst.PosRequestTypeEnum.ClosingDay_RemoveAllocationGroupLevel;
                    break;
                case "ClosingDayEODProcessControl":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_EndOfDayGroupLevel;
                    break;
                case "ClosingDayInitialMarginControl":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_InitialMarginControlGroupLevel;
                    break;
                case "ClosingDayMarket":
                    ret = Cst.PosRequestTypeEnum.ClosingDayMarketGroupLevel;
                    break;
                case "ClosingDayValidation":
                    ret = Cst.PosRequestTypeEnum.ClosingDay_ValidationGroupLevel;
                    break;
                case "EndOfDayAutomaticOption":
                    ret = Cst.PosRequestTypeEnum.EOD_AutomaticOptionGroupLevel;
                    break;
                case "EndOfDayCascading":
                    ret = Cst.PosRequestTypeEnum.EOD_CascadingGroupLevel;
                    break;
                case "EndOfDayCashBalance":
                    ret = Cst.PosRequestTypeEnum.EOD_CashBalanceGroupLevel;
                    break;
                case "EndOfDayCashFlowsCalculation":
                    ret = Cst.PosRequestTypeEnum.EOD_CashFlowsGroupLevel;
                    break;
                case "EndOfDayClearing":
                    ret = Cst.PosRequestTypeEnum.EOD_ClearingEndOfDayGroupLevel;
                    break;
                case "EndOfDayControl":
                    ret = Cst.PosRequestTypeEnum.EOD_ControlGroupLevel;
                    break;
                case "EndOfDayCorporateAction":
                    ret = Cst.PosRequestTypeEnum.EOD_CorporateActionGroupLevel;
                    break;
                case "EndOfDayFeesCalculation":
                    ret = Cst.PosRequestTypeEnum.EOD_FeesGroupLevel;
                    break;
                case "EndOfDayInitialMarginCalculation":
                    ret = Cst.PosRequestTypeEnum.EOD_InitialMarginGroupLevel;
                    break;
                case "EndOfDayManualOption":
                    ret = Cst.PosRequestTypeEnum.EOD_ManualOptionGroupLevel;
                    break;
                case "EndOfDayMarket":
                    ret = Cst.PosRequestTypeEnum.EOD_MarketGroupLevel;
                    break;
                case "EndOfDayMaturityOffsettingFuture":
                    ret = Cst.PosRequestTypeEnum.EOD_MaturityOffsettingFutureGroupLevel;
                    break;
                // EG 20170206 [22787]
                case "EndOfDayPhysicalPeriodicDelivery":
                    ret = Cst.PosRequestTypeEnum.EOD_PhysicalPeriodicDelivery;
                    break;

                case "EndOfDaySafeKeepingFeesCalculation":
                    ret = Cst.PosRequestTypeEnum.EOD_SafekeepingGroupLevel;
                    break;
                case "EndOfDayShifting":
                    ret = Cst.PosRequestTypeEnum.EOD_ShiftingGroupLevel;
                    break;
                case "EndOfDayUnderlyerDelivery":
                    ret = Cst.PosRequestTypeEnum.EOD_UnderlyerDeliveryGroupLevel;
                    break;
                case "EndOfDayUpdateEntry":
                    ret = Cst.PosRequestTypeEnum.EOD_UpdateEntryGroupLevel;
                    break;
                case "EndOfDayUTICalculation":
                    ret = Cst.PosRequestTypeEnum.EOD_UTICalculationGroupLevel;
                    break;
                case "EndOfDayMaturityRedemptionOffsettingDebtSecurity":
                    ret = Cst.PosRequestTypeEnum.EOD_MaturityRedemptionOffsettingDebtSecurityGroupLevel;
                    break;
                default:
                    break;
            }
            return ret;
        }
    }

    #region SPANSpreadType
    /// <summary>
    /// Class de manipulation des Types de Spread
    /// </summary>
    public class SPANSpreadType
    {
        #region Enum
        /// <summary>
        /// Enum utilisé pour les types de tiers des spreads
        /// </summary>
        [SerializableAttribute()]
        public enum TierTypeEnum
        {
            /// <summary>
            /// SPAN Intermonth Tier (Intracommodity) & London SPAN Month Tier
            /// </summary>
            [XmlEnumAttribute("A")]
            IntraTier,
            /// <summary>
            /// SPAN Intercommodity Tier
            /// </summary>
            [XmlEnumAttribute("R")]
            InterTier,
            /// <summary>
            /// SPAN Scanning Tiers
            /// </summary>
            [XmlEnumAttribute("N")]
            ScanTier,
            /// <summary>
            /// Short Option Minimum
            /// </summary>
            [XmlEnumAttribute("M")]
            SomTier,
            /// <summary>
            /// London SPAN Intercontract Tier (Indirect Tier)
            /// </summary>
            [XmlEnumAttribute("L")]
            SubTier,
        }

        /// <summary>
        /// Enum utilisé pour les types de spreads Intracommodity
        /// </summary>
        [SerializableAttribute()]
        public enum SpreadTypeEnum
        {
            /// <summary>
            /// SPAN Tier to Tier Intracommodity Spreads & London SPAN Leg Spread
            /// </summary>
            [XmlEnumAttribute("T")]
            TierToTier,
            /// <summary>
            /// SPAN Series to Series Intracommodity Spreads
            /// </summary>
            [XmlEnumAttribute("S")]
            SerieToSerie,
            /// <summary>
            /// London SPAN Strategy Spread
            /// </summary>
            [XmlEnumAttribute("L")]
            StrategySpread,
        }

        /// <summary>
        /// Enum utilisé pour les types de spreads Intercommodity
        /// </summary>
        [SerializableAttribute()]
        public enum InterSpreadTypeEnum
        {
            /// <summary>
            /// Inter Clearing Commodity Spread
            /// </summary>
            [XmlEnumAttribute("C")]
            InterClearSpread,
            /// <summary>
            /// Inter Commodity Spread
            /// </summary>
            [XmlEnumAttribute("I")]
            InterSpread,
            /// <summary>
            /// Super Inter Commodity Spread
            /// </summary>
            [XmlEnumAttribute("S")]
            SuperInterSpread,
        }

        /// <summary>
        /// Enum utilisé pour les types de jambes de spreads
        /// </summary>
        [SerializableAttribute()]
        public enum SpreadLegTypeEnum
        {
            /// <summary>
            /// Period Leg
            /// </summary>
            [XmlEnumAttribute("P")]
            PLeg,
            /// <summary>
            /// RP Leg
            /// </summary>
            [XmlEnumAttribute("RP")]
            RPLeg,
            /// <summary>
            /// Scan Leg
            /// </summary>
            [XmlEnumAttribute("S")]
            SLeg,
            /// <summary>
            /// Tier Leg
            /// </summary>
            [XmlEnumAttribute("T")]
            TLeg,
        }

        #endregion Enum

        #region Accessors
        #region string TierType
        /// <summary>
        /// Intra Tier
        /// </summary>
        public static string IntraTierType
        {
            get { return EnumXmlElementAttribute<TierTypeEnum>(TierTypeEnum.IntraTier); }
        }
        /// <summary>
        /// Inter Tier
        /// </summary>
        public static string InterTierType
        {
            get { return EnumXmlElementAttribute<TierTypeEnum>(TierTypeEnum.InterTier); }
        }
        /// <summary>
        /// Scan Tier
        /// </summary>
        public static string ScanTierType
        {
            get { return EnumXmlElementAttribute<TierTypeEnum>(TierTypeEnum.ScanTier); }
        }
        /// <summary>
        /// Som Tier
        /// </summary>
        public static string SomTierType
        {
            get { return EnumXmlElementAttribute<TierTypeEnum>(TierTypeEnum.SomTier); }
        }
        /// <summary>
        /// Sub Tier
        /// </summary>
        public static string SubTierType
        {
            get { return EnumXmlElementAttribute<TierTypeEnum>(TierTypeEnum.SubTier); }
        }
        #endregion string TierType
        #region string IntraSpreadType
        /// <summary>
        /// Serie To Serie
        /// </summary>
        public static string SerieToSerieSpreadType
        {
            get { return EnumXmlElementAttribute<SpreadTypeEnum>(SpreadTypeEnum.SerieToSerie); }
        }
        /// <summary>
        /// Tier To Tier
        /// </summary>
        public static string TierToTierSpreadType
        {
            get { return EnumXmlElementAttribute<SpreadTypeEnum>(SpreadTypeEnum.TierToTier); }
        }
        /// <summary>
        /// Strategy Spread
        /// </summary>
        public static string StrategySpreadType
        {
            get { return EnumXmlElementAttribute<SpreadTypeEnum>(SpreadTypeEnum.StrategySpread); }
        }
        #endregion string IntraSpreadType
        #region string InterSpreadType
        /// <summary>
        /// Inter Clearing Commodity Spread
        /// </summary>
        public static string InterClearSpreadType
        {
            get { return EnumXmlElementAttribute<InterSpreadTypeEnum>(InterSpreadTypeEnum.InterClearSpread); }
        }
        /// <summary>
        /// Inter Commodity Spread
        /// </summary>
        public static string InterSpreadType
        {
            get { return EnumXmlElementAttribute<InterSpreadTypeEnum>(InterSpreadTypeEnum.InterSpread); }
        }
        /// <summary>
        /// Super Inter Commodity Spread
        /// </summary>
        public static string SuperInterSpreadType
        {
            get { return EnumXmlElementAttribute<InterSpreadTypeEnum>(InterSpreadTypeEnum.SuperInterSpread); }
        }
        #endregion string InterSpreadType
        #endregion Accessors

        #region Method
        /// <summary>
        /// Obtient la valeur XmlEnumAttribute d'une valeur d'enum
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="pTierTypeEnum"></param>
        /// <returns></returns>
        public static string EnumXmlElementAttribute<TEnum>(TEnum pTierTypeEnum)
        {
            string value = null;
            if (pTierTypeEnum != null)
            {
                Type type = pTierTypeEnum.GetType();
                if (type.IsEnum)
                {
                    MemberInfo member = type.GetMember(pTierTypeEnum.ToString()).FirstOrDefault();
                    if (member != null)
                    {
                        XmlEnumAttribute attribute = member.GetCustomAttributes(false).OfType<XmlEnumAttribute>().FirstOrDefault();
                        if (attribute != null)
                        {
                            value = attribute.Name;
                        }
                    }
                }
            }
            return value;
        }
        #endregion Method
    }
    #endregion SPANSpreadType
}

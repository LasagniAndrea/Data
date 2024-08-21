#region Using Directives
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Curve;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    #region public class CalculationPeriodInfo
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new member intervalFrequency
    ///     </comment>
    /// </revision>
    /// 20090519 EG Add IdE parameter
    public class CalculationPeriodInfo
    {
        #region Members
        /// <summary>
        /// IdE de l'évènement de period
        /// </summary>
        protected int m_IdE;
        /// <summary>
        /// Représente le traitement en cours
        /// </summary>
        protected CommonValProcessBase m_CommonValProcess;

        public string event_source;

        protected DateTime m_CommonValDate;
        public DateTime startDate;
        protected DateTime m_EndDate;
        public DateTime startDateUnAdj;
        protected DateTime endDateUnAdj;
        protected DateTime m_RateCutOffDate;
        protected string m_Currency;
        public decimal nominal;
        public decimal multiplier;
        public decimal spread;
        public string dayCountFraction;
        public IInterval intervalFrequency;
        protected int m_IdAsset;
        protected int m_IdAsset2;

        protected bool m_AveragingMethodSpecified;
        public AveragingMethodEnum averagingMethod;
        protected bool m_FinalRateRoundingSpecified;
        protected IRounding m_FinalRateRounding;

        // EG 20150605 [21011] Add PayerReceiverInfoDet
        protected PayerReceiverInfoDet streamPayer;
        protected PayerReceiverInfoDet streamReceiver;

        #endregion
        #region Accessors
        #region public CommonValProcess
        public CommonValProcessBase CommonValProcess
        {
            get { return m_CommonValProcess; }
        }
        #endregion 
        #region public Currency
        public string Currency
        {
            get { return m_Currency; }
        }
        #endregion
        #region public EndDate
        /// <summary>
        /// Obtient la date fin de l'évènement
        /// <para>Si traitement de réescompte, obtient la date de réesscompe (m_CommonValDate) lorsqu'elle elle est comprise en dtStart et dtEnd</para>
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                DateTime ret = m_EndDate;
                //
                if ((m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess) &&
                    (-1 < m_CommonValDate.CompareTo(startDate)) && (1 > m_CommonValDate.CompareTo(m_EndDate)))
                    ret = m_CommonValDate;
                //
                return ret;
            }
        }
        #endregion Endate
        #region public IdE
        public int IdE
        {
            get { return m_IdE; }
        }
        #endregion
        #endregion Accessors
        #region Constructors
        public CalculationPeriodInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowCalcPeriod)
        {
            SetInfoBase(pCommonValDate, pCommonValProcess, pRowCalcPeriod);

            DataRow[] rowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRowCalcPeriod["IDE"]));
            if ((null != rowAssets) && (0 < rowAssets.Length))
            {
                DataRow rowAsset = rowAssets[0];
                #region FloatingRate
                if ((null != rowAsset) && (false == Convert.IsDBNull(rowAsset["IDASSET"])))
                    m_IdAsset = Convert.ToInt32(rowAsset["IDASSET"]);
                #endregion FloatingRate

                #region FloatingRate2
                if (1 < rowAssets.Length)
                {
                    rowAsset = rowAssets[1];
                    if ((null != rowAsset) && (false == Convert.IsDBNull(rowAsset["IDASSET"])))
                        m_IdAsset2 = Convert.ToInt32(rowAsset["IDASSET"]);
                }
                #endregion FloatingRate2
            }

            #region Nominal & Currency
            if (EventTypeFunc.IsKnownAmount(pRowCalcPeriod["EVENTTYPE"].ToString()))
                m_Currency = pRowCalcPeriod["UNIT"].ToString();
            else
            {
                DataRow rowNominal = m_CommonValProcess.GetCurrentNominal(startDate, startDateUnAdj);
                if ((null != rowNominal) && (false == Convert.IsDBNull(rowNominal["VALORISATION"])))
                {
                    nominal = Convert.ToDecimal(rowNominal["VALORISATION"]);
                    m_Currency = rowNominal["UNIT"].ToString();
                }
                else
                {
                    ProcessStateTools.StatusEnum status = (EndDate <= OTCmlHelper.GetDateBusiness(m_CommonValProcess.Process.Cs)) ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusWarningEnum;
                    ProcessState processState = new ProcessState(status, ProcessStateTools.CodeReturnDataNotFoundEnum);
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        "Nominal uncalculated Period [{0} - {1}]", processState, 
                        startDate.ToShortDateString(), processState, EndDate.ToShortDateString());
                }
            }
            #endregion Nominal & Currency

            SetParameter();
        }
        #endregion Constructors
        #region Methods
        #region private SetInfoBase
        // EG 20190415 [Migration BANCAPERTA]
        private void SetInfoBase(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowCalcPeriod)
        {
            m_CommonValProcess = pCommonValProcess;
            m_CommonValDate = pCommonValDate;
            m_IdE = Convert.ToInt32(pRowCalcPeriod["IDE"]);
            startDate = Convert.ToDateTime(pRowCalcPeriod["DTSTARTADJ"]);
            startDateUnAdj = Convert.ToDateTime(pRowCalcPeriod["DTSTARTUNADJ"]);
            m_EndDate = Convert.ToDateTime(pRowCalcPeriod["DTENDADJ"]);
            endDateUnAdj = Convert.ToDateTime(pRowCalcPeriod["DTENDUNADJ"]);
            m_Currency = pRowCalcPeriod["UNIT"].ToString();
            DataRow rowNominal = m_CommonValProcess.GetCurrentNominal(startDate, startDateUnAdj);
            if ((null != rowNominal) && (false == Convert.IsDBNull(rowNominal["VALORISATION"])))
                nominal = Convert.ToDecimal(rowNominal["VALORISATION"]);
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(m_IdE);
            // EG 20190415 Test rowDetail
            if (null != rowDetail)
            {
                dayCountFraction = rowDetail["DCF"].ToString();
                multiplier = Convert.IsDBNull(rowDetail["MULTIPLIER"]) ? 1 : Convert.ToDecimal(rowDetail["MULTIPLIER"]);
                spread = Convert.IsDBNull(rowDetail["SPREAD"]) ? 0 : Convert.ToDecimal(rowDetail["SPREAD"]);
            }
            event_source = Convert.IsDBNull(pRowCalcPeriod["SOURCE"]) ? "NA" : Convert.ToString(pRowCalcPeriod["SOURCE"]);
        }
        #endregion SetInfoBase
        #region public SetParameter
        public void SetParameter()
        {
            Cst.ErrLevel errLevel;
            CommonValParameterIRD parameter = (CommonValParameterIRD)m_CommonValProcess.Parameters[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
            #region FloatingRate
            if ((0 != m_IdAsset) && (null == parameter.Rate))
            {
                parameter.Rate = new SQL_AssetRateIndex(parameter.CS, SQL_AssetRateIndex.IDType.IDASSET, m_IdAsset)
                {
                    WithInfoSelfCompounding = Cst.IndexSelfCompounding.CASHFLOW
                };
            }
            #endregion FloatingRate
            #region FloatingRate2
            if ((0 != m_IdAsset2) && (null == parameter.Rate2))
                parameter.Rate2 = new SQL_AssetRateIndex(parameter.CS , SQL_AssetRateIndex.IDType.IDASSET, m_IdAsset2);
            #endregion FloatingRate2
            #region AveragingMethod
            errLevel = parameter.AveragingMethod(out averagingMethod);
            m_AveragingMethodSpecified = (Cst.ErrLevel.SUCCESS == errLevel);
            #endregion AveragingMethod
            #region FinalRateRounding
            m_FinalRateRounding = parameter.FinalRateRounding;
            #endregion FinalRateRounding
            #region Calculation Period Frequency
            intervalFrequency = parameter.CalculationPeriodFrequency;
            #endregion Calculation Period Frequency
            #region Multiplier & spread
            try
            {
                if (event_source.StartsWith(Cst.ServiceEnum.SpheresEventsGen.ToString()))
                {
                    string source =event_source;
                    
                    int postInstance = source.LastIndexOf("-Inst:");
                    if (postInstance > 0)
                        source = event_source.Substring(0, event_source.LastIndexOf("-Inst:"));
                    
                    string version = source.Remove(0, Cst.ServiceEnum.SpheresEventsGen.ToString().Length).Trim();

                    if (version.StartsWith("v"))
                    {
                        version = version.Remove(0, 1);
                        string[] version_number = version.Split('.');
                        int major = Convert.ToInt32(version_number[0]);
                        int minor = Convert.ToInt32(version_number[1]);
                        int revision = Convert.ToInt32(version_number[2]);
                        if ((major < 2) || (major == 2 && minor == 0 && revision < 1))
                        {
                            // NB: Evenement généré avec une version inférieur à la version v2.0.1
                            //     EventDet ne dispose pas ni du Spread ni du Multiplier(cf TRIM 16102)
                            //     On va donc les rechercher sur le DataDocument
                            parameter.GetRateSpreadAndMultiplier(startDateUnAdj, endDateUnAdj, out multiplier, out spread);
                        }
                    }
                }
            }
            catch { }
            #endregion Multiplier & spread

            // EG 20150605 [21011] New
            streamPayer = parameter.GetPayerReceiverInfoDet(m_CommonValProcess.TradeLibrary.DataDocument, PayerReceiverEnum.Payer);
            streamReceiver = parameter.GetPayerReceiverInfoDet(m_CommonValProcess.TradeLibrary.DataDocument, PayerReceiverEnum.Receiver);

        }
        #endregion SetParameter
        #endregion Methods
    }
    #endregion
    #region public class CapFloorPeriodInfo
    /// <revision>
    ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
    ///     <comment>Ticket 15889
    ///     Step dates: Unajusted versus Ajusted
    ///     Add public  member : BusinessDayAdjustments calculationPeriodDatesAdjustments;
    ///     </comment>
    /// </revision>
    public class CapFloorPeriodInfo
    {
        #region Member
        protected CommonValProcessBase m_CommonValProcess;
        protected DateTime m_CommonValDate;
        public DateTime startDate;
        protected DateTime m_EndDate;
        public decimal sourceRate;
        public IProduct product;
        protected string m_Currency;
        public string eventType;
        public IStrikeSchedule[] strikeSchedules;
        public IBusinessDayAdjustments calculationPeriodDateAdjustment;
        #endregion
        #region Accessors
        /// <summary>
        /// Obtient la date fin de l'évènement
        /// <para>Si traitement de réescompte, obtient la date de réesscompe (m_CommonValDate) lorsqu'elle elle est comprise en dtStart et dtEnd</para>
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                DateTime ret = m_EndDate;
                //
                if ((m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess) &&
                    (-1 < m_CommonValDate.CompareTo(startDate)) && (1 > m_CommonValDate.CompareTo(m_EndDate)))
                    ret = m_CommonValDate;
                //
                return ret;
            }
        }
        #endregion Accessors
        #region Constructors
        public CapFloorPeriodInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowCapFloor,
            decimal pSourceRate, string pCurrency)
        {
            m_CommonValProcess = pCommonValProcess;
            m_CommonValDate = pCommonValDate;
            startDate = Convert.ToDateTime(pRowCapFloor["DTSTARTUNADJ"]);
            m_EndDate = Convert.ToDateTime(pRowCapFloor["DTENDUNADJ"]);
            sourceRate = pSourceRate;
            m_Currency = pCurrency;
            eventType = pRowCapFloor["EVENTTYPE"].ToString();
            SetParameter();
        }
        #endregion Constructors
        #region public Methods
        public void SetParameter()
        {
            CommonValParameterIRD parameter = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
            product = parameter.Product;
            parameter.GetStrikeSchedule(eventType, out strikeSchedules);
            calculationPeriodDateAdjustment = parameter.CalculationPeriodDateAdjustment;
        }
        #endregion
    }
    #endregion CapFloorPeriodInfo
    #region public class PaymentInfo
    public class PaymentInfo
    {
        #region Members
        protected int m_IdE;
        protected CommonValProcessBase m_CommonValProcess;

        protected DateTime m_CommonValDate;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected DateTime m_RateCutOffDate;
        protected string m_Currency;

        protected bool m_CompoundingMethodSpecified;
        protected CompoundingMethodEnum m_CompoundingMethod;
        protected bool m_NegativeInterestRateTreatmentSpecified;
        protected NegativeInterestRateTreatmentEnum m_NegativeInterestRateTreatment;
        protected bool m_DiscountingSpecified;
        protected IDiscounting m_Discounting;
        protected FraDiscountingEnum m_FraDiscounting;
        protected decimal m_FraFixedRate;
        protected string m_Fra_DCF;
        protected decimal m_Fra_Notional;
        protected decimal m_Sec_Quantity;
        public IInterval intervalFrequency;
        //-- 20080411 RD Ticket 16107 
        //-- 20080414 EG Ticket 16107 
        protected bool m_IsPaymentRelativeToStartDate;
        protected bool m_IsAmountRelativeToStartDate;

        // EG 20150605 [21011] Add PayerReceiverInfoDet
        protected PayerReceiverInfoDet streamPayer;
        protected PayerReceiverInfoDet streamReceiver;
        #endregion Members
        #region Accessors
        #region public IdE
        public int IdE
        {
            get { return m_IdE; }
        }
        #endregion
        #region public IsCompounding
        public bool IsCompounding
        {
            get
            {
                bool ret = false;
                if (m_CompoundingMethodSpecified)
                    ret = (CompoundingMethodEnum.None != m_CompoundingMethod);
                return ret;
            }
        }
        #endregion IsCompounding
        #region public IsPaymentRelativeToStartDate
        public bool IsPaymentRelativeToStartDate
        {
            get { return (m_IsPaymentRelativeToStartDate); }
        }
        #endregion IsPaymentRelativeToStartDate
        #region public StartDate
        public DateTime StartDate
        {
            get { return m_StartDate; }
        }
        #endregion StartDate
        #region public Endate
        /// <summary>
        /// Obtient la date fin de l'évènement
        /// <para>Si traitement de réescompte, obtient la date de réesscompe (m_CommonValDate) lorsqu'elle elle est comprise en dtStart et dtEnd</para>
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                DateTime ret = m_EndDate;
                //
                if ((m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess) &&
                    (-1 < m_CommonValDate.CompareTo(StartDate)) && (1 > m_CommonValDate.CompareTo(m_EndDate)))
                    ret = m_CommonValDate;
                //
                return ret;
            }
        }
        #endregion Endate
        #endregion Accessors
        //
        #region Constructors
        public PaymentInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowInterest)
        {
            m_CommonValProcess = pCommonValProcess;
            m_CommonValDate = pCommonValDate;
            m_StartDate = Convert.ToDateTime(pRowInterest["DTSTARTADJ"]);
            m_EndDate = Convert.ToDateTime(pRowInterest["DTENDADJ"]);
            m_IdE = Convert.ToInt32 (pRowInterest["IDE"]);
            //
            DataRow[] rowEventClass = pRowInterest.GetChildRows(m_CommonValProcess.DsEvents.ChildEventClass);
            foreach (DataRow dr in rowEventClass)
            {
                string eventClass = dr["EVENTCLASS"].ToString();
                if (EventClassFunc.IsRateCutOffDate(eventClass))
                    m_RateCutOffDate = Convert.ToDateTime(dr["DTEVENT"]);
            }
            #region Currency
            //
            m_Currency = pRowInterest["UNIT"].ToString();
            DataRow rowNominal = m_CommonValProcess.GetCurrentNominal(m_StartDate, Convert.ToDateTime(pRowInterest["DTSTARTUNADJ"]));
            if (null != rowNominal)
                m_Currency = rowNominal["UNIT"].ToString();
            #endregion Currency
            //
            #region Quantity (for Security transaction)
            DataRow rowQuantity = m_CommonValProcess.GetCurrentQuantity(m_StartDate, Convert.ToDateTime(pRowInterest["DTSTARTUNADJ"]));
            if ((null != rowQuantity) && (false == Convert.IsDBNull(rowQuantity["VALORISATION"])))
                m_Sec_Quantity = Convert.ToDecimal(rowQuantity["VALORISATION"]);
            #endregion Quantity (for Security transaction)
            //
            SetParameter();
        }
        #endregion Constructors
        #region public SetParameter
        public void SetParameter()
        {
            CommonValParameterIRD parameter = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
            Cst.ErrLevel errLevel = parameter.CompoundingMethod(out m_CompoundingMethod);
            m_CompoundingMethodSpecified = (Cst.ErrLevel.SUCCESS == errLevel);
            errLevel = parameter.NegativeInterestRateTreatment(out m_NegativeInterestRateTreatment);
            m_NegativeInterestRateTreatmentSpecified = (Cst.ErrLevel.SUCCESS == errLevel);
            errLevel = parameter.Discounting(out m_Discounting);
            m_DiscountingSpecified = (Cst.ErrLevel.SUCCESS == errLevel);
            m_FraDiscounting = parameter.FraDiscounting;
            m_FraFixedRate = parameter.FraFixedRate;
            m_Fra_DCF = parameter.FraDayCountFraction;
            m_Fra_Notional = Decimal.Zero;
            if (null != parameter.FraNotional)
                m_Fra_Notional = parameter.FraNotional.Amount.DecValue ;
            intervalFrequency = parameter.PaymentFrequency;
            #region Calculation PaymentRelativeToStartDate
            m_IsPaymentRelativeToStartDate = parameter.IsPaymentRelativeToStartDate;
            m_IsAmountRelativeToStartDate = m_IsPaymentRelativeToStartDate && (EndDate == m_CommonValDate);
            #endregion Calculation PayRelativeToStartDate

            // EG 20150605 [21011] New
            streamPayer = parameter.GetPayerReceiverInfoDet(m_CommonValProcess.TradeLibrary.DataDocument, PayerReceiverEnum.Payer);
            streamReceiver = parameter.GetPayerReceiverInfoDet(m_CommonValProcess.TradeLibrary.DataDocument, PayerReceiverEnum.Receiver);
        }
        #endregion Methods
    }
    #endregion PaymentInfo
    #region public class PayerReceiverInfo
    // EG 20150706 [21021] Nullable<int> for m_IdB_Pay|m_IdB_Rec
    public class PayerReceiverInfo : IComparable
    {
        #region Members
        protected int m_IdA_Pay;
        // EG 20150706 [21021]
        protected Nullable<int> m_IdB_Pay;
        protected int m_IdA_Rec;
        // EG 20150706 [21021]
        protected Nullable<int> m_IdB_Rec;
        #endregion Members
        //
        #region Property
        #region Payer
        public int Payer
        {
            set { m_IdA_Pay = value; }
            get { return m_IdA_Pay; }
        }
        #endregion Payer
        #region BookPayer
        public Nullable<int> BookPayer
        {
            set { m_IdB_Pay = value; }
            get { return m_IdB_Pay; }
        }
        #endregion BookPayer
        #region Receiver
        public int Receiver
        {
            set { m_IdA_Rec = value; }
            get { return m_IdA_Rec; }
        }
        #endregion Receiver
        #region BookReceiver
        public Nullable<int> BookReceiver
        {
            set { m_IdB_Rec = value; }
            get { return m_IdB_Rec; }
        }
        #endregion BookReceiver
        #endregion Property
        //
        #region Constructors
        public PayerReceiverInfo(CommonValProcessBase pCommonValProcess, DataRow pRow)
        {
            SetInfoBase(pCommonValProcess, pRow);
        }
        #endregion Constructors
        //
        #region Methods
        #region SetInfoBase
        private void SetInfoBase(CommonValProcessBase pCommonValProcess, DataRow pRow)
        {
            DataRow rowParent = pCommonValProcess.GetRowParentWithPayerReceiver(pRow);
            if (null != rowParent)
            {
                m_IdA_Pay = Convert.ToInt32(rowParent["IDA_PAY"]);
                m_IdA_Rec = Convert.ToInt32(rowParent["IDA_REC"]);
                if (false == Convert.IsDBNull(rowParent["IDB_PAY"]))
                    m_IdB_Pay = Convert.ToInt32(rowParent["IDB_PAY"]);
                if (false == Convert.IsDBNull(rowParent["IDB_REC"]))
                    m_IdB_Rec = Convert.ToInt32(rowParent["IDB_REC"]);
            }
        }
        #endregion SetInfoBase
        #endregion Methods

        #region IComparable Membres
        /// <summary>
        /// Compare {obj} à l'instance
        /// <para>Obtient 0 si identique</para>
        /// <para>Obtient une valeur != 0 sinon</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int>
        public int CompareTo(object obj)
        {
            PayerReceiverInfo item = (PayerReceiverInfo)obj;
            int ret = 0;
            if (0 == ret)
                ret = item.Payer.CompareTo(this.Payer);
            if (0 == ret)
            {
                // EG 20150706 [21021]
                if (item.BookPayer.HasValue && this.BookPayer.HasValue)
                    ret = item.BookPayer.Value.CompareTo(this.BookPayer.Value);
                else
                    ret = -1;
            }
            if (0 == ret)
                ret = item.Receiver.CompareTo(this.Receiver);
            if (0 == ret)
            {
                // EG 20150706 [21021]
                if (item.BookReceiver.HasValue && this.BookReceiver.HasValue)
                    ret = item.BookReceiver.Value.CompareTo(this.BookReceiver.Value);
                else
                    ret = -1;
            }
            return ret;
        }

        #endregion

        
    }
    #endregion PayerReceiverInfo
    #region public class ResetInfo
    public class ResetInfo
    {
        #region enum
        protected enum ReadQuoteAssetEnum
        {
            Asset1,
            Asset2
        }
        #endregion
        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected CommonValProcessBase m_CommonValProcess;
        /// <summary>
        /// 
        /// </summary>
        protected int m_IdE;
        /// <summary>
        /// Représente la date de traitement (Accrual, MartToMarket,...)
        /// <para>Non alimentée lors d'un traitement de cash flow</para>
        /// </summary>
        protected DateTime m_CommonValDate;
        /// <summary>
        /// Date de début de l'évènement de reset
        /// </summary>
        protected DateTime m_StartDate;
        /// <summary>
        /// Date fin de l'évènement de reset
        /// </summary>
        protected DateTime m_EndDate;
        /// <summary>
        /// Date de 
        /// </summary>
        public DateTime resetDate;
        /// <summary>
        /// Date de fixing pour la lecture du taux
        /// </summary>
        protected DateTime m_FixingDate;
        /// <summary>
        /// Représente le résultat de l'application de l'offset de l'asset à la date de traitement
        /// <para>Utilisé pour le traitement de réescompte, sur les flux d'intérêts qui sont de type "reset-in-arrears"</para>
        /// </summary>
        protected DateTime m_FixingDateCommonValDate;

        protected DateTime m_RateCutOffDate;
        /// <summary>
        /// Représente la date de lecture de taux
        /// </summary>
        protected DateTime m_ObservedRateDate;
        protected DateTime m_EndPeriodDate;
        protected int m_IdAsset;
        protected int m_IdAsset2;
        protected bool m_RateTreatmentSpecified;
        protected RateTreatmentEnum m_RateTreatment;
        protected IInterval m_PaymentFrequency;
        protected int m_RoundingPrecision;
        
        protected IRounding m_RateRounding;
      

        protected CompoundingMethodEnum m_SelfCompoundingMethod;

        protected PayerReceiverInfo m_PayerReceiver;
        protected DataRow[] m_RowAssets;
        #endregion Members
        #region Accessors
        #region public EndDate
        /// <summary>
        /// Obtient la date fin de l'évènement
        /// <para>Si traitement de réescompte, obtient la date de réesscompe (m_CommonValDate) lorsqu'elle elle est comprise en dtStart et dtEnd</para>
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                DateTime ret = m_EndDate;
                //
                if ((m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess) &&
                    (-1 < m_CommonValDate.CompareTo(m_StartDate)) && (1 > m_CommonValDate.CompareTo(m_EndDate)))
                    ret = m_CommonValDate;
                //
                return ret;
            }
        }
        #endregion
        #region public IdE
        public int IdE
        {
            get { return m_IdE; }
        }
        #endregion
        #endregion Accessors
        #region Constructors
        public ResetInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowReset)
        {
            SetInfoBase(pCommonValDate, pCommonValProcess, pRowReset);
            SetParameter();
        }
        public ResetInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowReset,
            int pIdAsset, int pIdAsset2, DateTime pRateCutOffDate)
        {
            SetInfoBase(pCommonValDate, pCommonValProcess, pRowReset);
            m_RateCutOffDate = pRateCutOffDate;
            m_IdAsset = pIdAsset;
            m_IdAsset2 = pIdAsset2;
            if ((false == DtFunc.IsDateTimeEmpty(m_RateCutOffDate)) && (m_RateCutOffDate < m_FixingDate))
                m_ObservedRateDate = m_RateCutOffDate;
            else
                m_ObservedRateDate = m_FixingDate;
            DataRow rowCalcPeriod = pRowReset.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
            m_EndPeriodDate = Convert.ToDateTime(rowCalcPeriod["DTENDADJ"]);

            SetParameter();
        }
        #endregion Constructors
        #region Methods
        #region SetInfoBase
        private void SetInfoBase(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowReset)
        {
            m_CommonValProcess = pCommonValProcess;
            m_CommonValDate = pCommonValDate;
            m_IdE = Convert.ToInt32(pRowReset["IDE"]);
            m_StartDate = Convert.ToDateTime(pRowReset["DTSTARTADJ"]);
            m_EndDate = Convert.ToDateTime(pRowReset["DTENDADJ"]);
            m_PayerReceiver = new PayerReceiverInfo(m_CommonValProcess, pRowReset);
            m_RowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRowReset["IDE"]));
            DataRow[] rowEventClass = pRowReset.GetChildRows(m_CommonValProcess.DsEvents.ChildEventClass);
            foreach (DataRow dr in rowEventClass)
            {
                string eventClass = dr["EVENTCLASS"].ToString();
                if (EventClassFunc.IsGroupLevel(eventClass))
                    resetDate = Convert.ToDateTime(dr["DTEVENT"]);
                else if (EventClassFunc.IsFixing(eventClass))
                    m_FixingDate = Convert.ToDateTime(dr["DTEVENT"]);
            }
        }
        #endregion SetInfoBase
        #region SetParameter
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public void SetParameter()
        {
            Cst.ErrLevel ret;
            CommonValParameterIRD parameter = (CommonValParameterIRD)m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo,m_CommonValProcess.ParamStreamNo];

            m_SelfCompoundingMethod = parameter.SelfCompoundingMethod;
            m_PaymentFrequency = parameter.PaymentFrequency;

            if ((0 != m_IdAsset) && (0 != m_IdAsset2))
            {
                int precisionRate = Convert.ToInt32(parameter.Rate.Idx_RoundPrec);
                int precisionRate2 = Convert.ToInt32(parameter.Rate2.Idx_RoundPrec);
                m_RoundingPrecision = Math.Max(precisionRate, precisionRate2);
                m_RoundingPrecision = Math.Max(m_RoundingPrecision, 3);
            }
            else
            {
                RoundingDirectionEnum direction = (RoundingDirectionEnum)StringToEnum.Parse(parameter.Rate.Idx_RoundDir, RoundingDirectionEnum.Nearest);
                m_RateRounding = m_PaymentFrequency.GetRounding(direction, parameter.Rate.Idx_RoundPrec);
            }

            #region RateTreatment
            ret = parameter.RateTreatment(out m_RateTreatment);
            m_RateTreatmentSpecified = (Cst.ErrLevel.SUCCESS == ret);
            #endregion RateTreatment

            if (m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess)
            {
                ret = parameter.GetFixingDateOffset(out IBusinessDayAdjustments bdA, out IOffset offset);
                if (Cst.ErrLevel.SUCCESS == ret)
                    m_FixingDateCommonValDate = Tools.ApplyOffset(m_CommonValProcess.Process.Cs, m_CommonValDate, offset, bdA, m_CommonValProcess.TradeLibrary.DataDocument);
            }
        }
        #endregion SetParameter
        #region protected ReadQuoteAssetRateIndex
        /// <summary>
        /// Retourne la valeur d'un asset de taux 
        /// <para>Par défaut Spheres® recherche la quotation de l'asset </para>
        /// <para>Cas du MarToMarket, lorsque la lecture n'aboutit pas, Spheres® estime le taux via un calcul de taux forward</para>
        /// </summary>
        /// <param name="pAsset"></param>
        /// <param name="pKeyQuote"></param>
        /// <param name="calcForwardRate"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected decimal ReadQuoteAssetRateIndex(ReadQuoteAssetEnum pAssetEnum, out KeyQuote pKeyQuote, out EFS_CalcForwardRateIndex pCalcForwardRate)
        {
            IProductBase productBase = m_CommonValProcess.ProductBase;
            CommonValParameterIRD paramIRD = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo,m_CommonValProcess.ParamStreamNo];

            int idAsset = 0;
            if (pAssetEnum == ReadQuoteAssetEnum.Asset1)
                idAsset = m_IdAsset;
            else if (pAssetEnum == ReadQuoteAssetEnum.Asset2)
                idAsset = m_IdAsset2;

            decimal ret = decimal.Zero;
            string cs = m_CommonValProcess.Process.Cs;
            KeyQuote keyQuote = new KeyQuote(cs, m_ObservedRateDate,
                                         m_PayerReceiver.Payer, m_PayerReceiver.BookPayer,
                                         m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver, QuoteTimingEnum.Close);
            //
            bool isNewAttempt = false;
            SystemMSGInfo systemMsgInfo = null;
            SQL_Quote quote;
            try
            {
                quote = CommonValFunc.ReadQuote_AssetByType(cs, QuoteEnum.RATEINDEX, productBase, keyQuote, idAsset, ref systemMsgInfo);
                ret = quote.QuoteValue;

            }
            catch (SpheresException2 ex)
            {
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    isNewAttempt = m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsMarkToMarket || m_CommonValProcess.IsProvisionProcess;
                    if (false == isNewAttempt)
                    {
                        if (ProcessStateTools.IsStatusError(systemMsgInfo.processState.Status))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_CommonValProcess.Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            
                            
                            
                            Logger.Log(systemMsgInfo.ToLoggerData(0));
                        }
                        throw new SpheresException2(systemMsgInfo.processState);
                    }
                }
                else
                    throw;
            }
            catch (Exception) { throw; };

            EFS_CalcForwardRateIndex calcForward = null;
            if (isNewAttempt)
            {
                if (m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess)
                {
                    //Lecture du quotation à la date FixingDateCommonValDate
                    if (DtFunc.IsDateTimeFilled(m_FixingDateCommonValDate))
                    {
                        keyQuote.Time = m_FixingDateCommonValDate;
                        try
                        {
                            systemMsgInfo = null;
                            quote = CommonValFunc.ReadQuote_AssetByType(cs, QuoteEnum.RATEINDEX, productBase, keyQuote, idAsset, ref systemMsgInfo);
                            ret = quote.QuoteValue;

                        }
                        catch (SpheresException2 ex)
                        {
                            if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                            {
                                if (ProcessStateTools.IsStatusError(systemMsgInfo.processState.Status))
                                {
                                    // FI 20200623 [XXXXX] SetErrorWarning
                                    m_CommonValProcess.Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                                    
                                    Logger.Log(systemMsgInfo.ToLoggerData(0));
                                }
                                throw new SpheresException2(systemMsgInfo.processState);
                            }
                            else
                                throw;
                        }
                    }
                }
                else if (m_CommonValProcess.IsMarkToMarket)
                {
                    //Calcul du taux forward
                    string idC;
                    if (productBase.IsFra)
                        idC = paramIRD.FraNotional.Currency;
                    else if (productBase.IsSwap || productBase.IsLoanDeposit || productBase.IsFxTermDeposit)
                        idC = paramIRD.Stream.StreamCurrency;
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Product {0} is not is not managed, please contact EFS", paramIRD.Product.GetType().FullName));

                    calcForward = new EFS_CalcForwardRateIndex(keyQuote, idAsset, idC, m_CommonValProcess.CommonValDate);
                    calcForward.Calc(cs, m_CommonValProcess.ProductBase, m_CommonValProcess.Process.Session);
                    ret = calcForward.ForwardRate;
                }
                else
                    throw new NotImplementedException("process is not implemented");
            }
            pKeyQuote = keyQuote;
            pCalcForwardRate = calcForward;
            return ret;
        }
        #endregion Methods
        #endregion
    }
    #endregion ResetInfo

    #region ExchangeFixingInfo
    public class ExchangeFixingInfo
    {
        #region Members
        protected CommonValProcessBase m_CommonValProcess;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected string m_ReferenceCurrency;
        protected string m_Currency;
        protected FxTypeEnum m_FxType;
        protected decimal m_NotionalAmount;
        protected decimal m_Rate;
        protected bool m_IsReverse;
        protected decimal m_RateRead;
        #endregion Members
        #region Accessors
        public string ReferenceCurrency { get { return m_ReferenceCurrency; } }
        public decimal Rate { get { return m_Rate; } }
        public decimal RateRead { get { return m_RateRead; } }
        public bool IsReverse { get { return m_IsReverse; } }
        #endregion Accessors
        #region Constructors
        public ExchangeFixingInfo(CommonValProcessBase pCommonValProcess, DataRow pRowSettlement)
        {
            SetInfoBase(pCommonValProcess, pRowSettlement);
        }
        #endregion Constructors
        #region Methods
        #region SetInfoBase
        private void SetInfoBase(CommonValProcessBase pCommonValProcess, DataRow pRowSettlement)
        {
            m_CommonValProcess = pCommonValProcess;
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowSettlement["IDE"]));
            m_StartDate = Convert.ToDateTime(pRowSettlement["DTSTARTADJ"]);
            m_EndDate = Convert.ToDateTime(pRowSettlement["DTENDADJ"]);
            m_Currency = pRowSettlement["UNIT"].ToString();
            if (null != rowDetail)
            {
                m_ReferenceCurrency = rowDetail["IDC_REF"].ToString();
                if (false == Convert.IsDBNull(rowDetail["FXTYPE"]) && Enum.IsDefined(typeof(FxTypeEnum), rowDetail["FXTYPE"].ToString()))
                    m_FxType = (FxTypeEnum)Enum.Parse(typeof(FxTypeEnum), rowDetail["FXTYPE"].ToString(), true);
                if (false == Convert.IsDBNull(rowDetail["NOTIONALAMOUNT"]))
                    m_NotionalAmount = Convert.ToDecimal(rowDetail["NOTIONALAMOUNT"]);
                if (false == Convert.IsDBNull(rowDetail["RATE"]))
                    m_Rate = Convert.ToDecimal(rowDetail["RATE"]);
            }
        }
        #endregion SetInfoBase
        #endregion Methods
    }
    #endregion ExchangeFixingInfo
    #region FxFixingInfo
    public class FxFixingInfo
    {
        #region Members
        protected CommonValProcessBase m_CommonValProcess;
        protected int m_IdAsset;
        protected DateTime m_Time;
        protected string m_IdBC;
        protected string m_Currency1;
        protected string m_Currency2;
        protected string m_CurrencyReference;
        protected QuoteBasisEnum m_Basis;
        protected string m_PrimaryRateSrc;
        protected string m_PrimaryRateSrcPage;
        protected string m_PrimaryRateSrcHead;

        protected PayerReceiverInfo m_PayerReceiver;
        protected DataRow[] m_RowAssets;
        #endregion Members
        //
        #region Constructors
        public FxFixingInfo(CommonValProcessBase pCommonValProcess, DataRow pRowFixing)
        {
            SetInfoBase(pCommonValProcess, pRowFixing);
        }
        #endregion Constructors
        #region Methods
        #region SetInfoBase
        private void SetInfoBase(CommonValProcessBase pCommonValProcess, DataRow pRowFixing)
        {
            m_CommonValProcess = pCommonValProcess;
            m_RowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRowFixing["IDE"]));
            if ((null != m_RowAssets) && (0 < m_RowAssets.Length))
            {
                DataRow rowAsset = m_RowAssets[0];
                m_IdAsset = Convert.ToInt32(rowAsset["IDASSET"]);
                m_Time = Convert.ToDateTime(rowAsset["TIME"]);
                m_PrimaryRateSrc = rowAsset["PRIMARYRATESRC"].ToString();
                m_PrimaryRateSrcPage = rowAsset["PRIMARYRATESRCPAGE"].ToString();
                m_PrimaryRateSrcHead = rowAsset["PRIMARYRATESRCHEAD"].ToString();
            }
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowFixing["IDE"]));
            if (null != rowDetail)
            {
                m_Currency1 = rowDetail["IDC1"].ToString();
                m_Currency2 = rowDetail["IDC2"].ToString();
                if (false == Convert.IsDBNull(rowDetail["BASIS"]))
                    m_Basis = (QuoteBasisEnum)Enum.Parse(typeof(QuoteBasisEnum), rowDetail["BASIS"].ToString());
                m_Time = Convert.ToDateTime(rowDetail["DTFIXING"]);
                m_IdBC = rowDetail["IDBC"].ToString();
                if (false == Convert.IsDBNull(rowDetail["IDC_REF"]))
                    m_CurrencyReference = rowDetail["IDC_REF"].ToString();
            }
            m_PayerReceiver = new PayerReceiverInfo(m_CommonValProcess, pRowFixing);
        }
        #endregion SetInfoBase
        #endregion Methods
    }
    #endregion FxFixingInfo
    #region FxResetInfo
    public class FxResetInfo
    {
        #region Members
        protected CommonValProcessBase m_CommonValProcess;
        protected int m_IdAsset;
        protected DateTime m_FixingDate;
        protected string m_IdBC;
        protected string m_Currency1;
        protected string m_Currency2;
        protected QuoteBasisEnum m_Basis;

        protected PayerReceiverInfo m_PayerReceiver;
        protected DataRow[] m_RowAssets;

        #endregion Members
        //
        #region Accessors
        public string Currency1 { get { return m_Currency1; } }
        public string Currency2 { get { return m_Currency2; } }
        public QuoteBasisEnum QuoteBasis { get { return m_Basis; } }
        #endregion Accessors
        //
        #region Constructors
        public FxResetInfo(CommonValProcessBase pCommonValProcess, DataRow pRowReset)
        {
            SetInfoBase(pCommonValProcess, pRowReset);
        }
        #endregion Constructors
        #region Methods
        #region SetInfoBase
        private void SetInfoBase(CommonValProcessBase pCommonValProcess, DataRow pRowReset)
        {
            m_CommonValProcess = pCommonValProcess;
            m_RowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRowReset["IDE"]));
            if ((null != m_RowAssets) && (0 < m_RowAssets.Length))
            {
                DataRow rowAsset = m_RowAssets[0];
                m_IdAsset = Convert.ToInt32(rowAsset["IDASSET"]);
                m_FixingDate = Convert.ToDateTime(rowAsset["TIME"]);
                m_IdBC = rowAsset["IDBC"].ToString();
            }
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowReset["IDE"]));
            if (null != rowDetail)
            {
                m_Currency1 = rowDetail["IDC1"].ToString();
                m_Currency2 = rowDetail["IDC2"].ToString();
                if (false == Convert.IsDBNull(rowDetail["BASIS"]))
                    m_Basis = (QuoteBasisEnum)Enum.Parse(typeof(QuoteBasisEnum), rowDetail["BASIS"].ToString());
            }
            m_PayerReceiver = new PayerReceiverInfo(m_CommonValProcess, pRowReset);
        }
        #endregion SetInfoBase
        #endregion Methods
    }
    #endregion FxResetInfo
    #region SelfAveragingInfo
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new member intervalFrequency
    ///     </comment>
    /// </revision>
    public class SelfAveragingInfo
    {
        #region Members
        protected int m_IdE;
        protected CommonValProcessBase m_CommonValProcess;
        protected DateTime m_CommonValDate;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected DateTime m_RateCutOffDate;
        protected int m_IdAsset;
        public string selfDayCountFraction;
        public string dayCountFraction;
        protected bool m_RateTreatmentSpecified;
        protected RateTreatmentEnum m_RateTreatment;
        public AveragingMethodEnum averagingMethod;
        protected IRounding m_RateRounding;
        public IInterval paymentFrequency;
        // 20071107 EG Ticket 15859
        public IInterval intervalFrequency;
        #endregion Members
        //
        #region Accessors
        #region StartDate
        public DateTime StartDate
        {
            get
            {
                return m_StartDate; 
            }
        }
        #endregion
        #region EndDate
        /// <summary>
        /// Obtient la date fin de l'évènement
        /// <para>Si traitement de réescompte, obtient la date de réesscompe (m_CommonValDate) lorsqu'elle elle est comprise en dtStart et dtEnd</para>
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                DateTime ret = m_EndDate;
                //
                if ((m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess) &&
                    (-1 < m_CommonValDate.CompareTo(m_StartDate)) && (1 > m_CommonValDate.CompareTo(m_EndDate)))
                    ret = m_CommonValDate;
                //
                return ret;
            }
        }
        #endregion
        #region IdE
        public int IdE
        {
            get { return m_IdE; }
        }
        #endregion
        #endregion Accessors
        //        
        #region Constructors
        public SelfAveragingInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowSelfAverage)
            : this(pCommonValDate, pCommonValProcess, pRowSelfAverage, Convert.ToDateTime(null)) { }
        public SelfAveragingInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowSelfAverage,
            DateTime pRateCutOffDate)
        {
            SetInfoBase(pCommonValDate, pCommonValProcess, pRowSelfAverage);
            m_RateCutOffDate = pRateCutOffDate;
            DataRow[] rowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRowSelfAverage["IDE"]));
            if (ArrFunc.IsFilled(rowAssets))  
                m_IdAsset = Convert.ToInt32(rowAssets[0]["IDASSET"]);
            SetParameter();
        }
        #endregion Constructors
        #region Methods
        #region SetInfoBase
        private void SetInfoBase(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowSelfAverage)
        {
            m_CommonValProcess = pCommonValProcess;
            m_CommonValDate = pCommonValDate;
            m_StartDate = Convert.ToDateTime(pRowSelfAverage["DTSTARTADJ"]);
            m_EndDate = Convert.ToDateTime(pRowSelfAverage["DTENDADJ"]);
        }
        #endregion SetInfoBase
        #region SetParameter
        public void SetParameter()
        {
            Cst.ErrLevel ret;
            CommonValParameterIRD parameter = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];

            if (0 != m_IdAsset)
            {
                if (null == parameter.RateBasis)
                    parameter.RateBasis = new SQL_AssetRateIndex(parameter.CS, SQL_AssetRateIndex.IDType.IDASSET, m_IdAsset);
            }
            dayCountFraction = parameter.DCFRateBasis;
            selfDayCountFraction = parameter.SelfDayCountFraction;
            intervalFrequency = parameter.SelfFrequency;
            averagingMethod = parameter.SelfAveragingMethod;
            ret = parameter.RateTreatmentRateBasis(out m_RateTreatment);
            m_RateTreatmentSpecified = (Cst.ErrLevel.SUCCESS == ret);
            paymentFrequency = parameter.PaymentFrequency;
            RoundingDirectionEnum direction = (RoundingDirectionEnum)StringToEnum.Parse(parameter.Rate.SelfAvg_RoundDir, RoundingDirectionEnum.Nearest);
            m_RateRounding = paymentFrequency.GetRounding(direction, parameter.Rate.SelfAvg_RoundPrec);
        }
        #endregion SetParameter
        #endregion Methods
    }
    #endregion SelfAveragingInfo
    #region SelfResetInfo
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Member intervalFrequency
    ///     </comment>
    /// </revision>
    public class SelfResetInfo
    {
        #region Members
        protected CommonValProcessBase m_CommonValProcess;

        protected DateTime m_SelfResetDate;
        protected DateTime m_RateCutOffDate;
        protected int m_IdAsset;
        public DateTime fixingDate;

        public string selfDayCountFraction;
        public string dayCountFraction;
        protected bool m_RateTreatmentSpecified;
        protected RateTreatmentEnum m_RateTreatment;
        protected AveragingMethodEnum m_AveragingMethod;
        protected IInterval m_PaymentFrequency;
        public IInterval intervalFrequency;

        protected PayerReceiverInfo m_PayerReceiver;
        protected DataRow[] m_RowAssets;

        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        public DateTime SelfResetDate { get { return m_SelfResetDate; } }
        #endregion Members
        #region Constructors
        public SelfResetInfo(CommonValProcessBase pEventsValProcess, DataRow pRowSelfReset, DateTime pRateCutOffDate)
            : this(pEventsValProcess, pRowSelfReset, 0, pRateCutOffDate) { }
        public SelfResetInfo(CommonValProcessBase pEventsValProcess, DataRow pRowSelfReset, int pIdAsset, DateTime pRateCutOffDate)
        {
            SetInfoBase(pEventsValProcess, pRowSelfReset, pRateCutOffDate);
            m_IdAsset = pIdAsset;
            SetParameter();
        }
        #endregion Constructors
        #region Methods
        #region SetInfoBase
        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        private void SetInfoBase(CommonValProcessBase pCommonValProcess, DataRow pRowSelfReset, DateTime pRateCutOffDate)
        {
            m_CommonValProcess = pCommonValProcess;
            m_SelfResetDate = Convert.ToDateTime(pRowSelfReset["DTSTARTADJ"]);
//            fixingDate = m_SelfResetDate;
            DataRow[] rowEventClass = pRowSelfReset.GetChildRows(m_CommonValProcess.DsEvents.ChildEventClass);
            foreach (DataRow dr in rowEventClass)
            {
                string eventClass = dr["EVENTCLASS"].ToString();
                if (EventClassFunc.IsFixing(eventClass))
                    fixingDate = Convert.ToDateTime(dr["DTEVENT"]);
            }
            m_RateCutOffDate = pRateCutOffDate;
            if ((false == DtFunc.IsDateTimeEmpty(m_RateCutOffDate)) && (m_RateCutOffDate < m_SelfResetDate))
                fixingDate = m_RateCutOffDate;

            m_PayerReceiver = new PayerReceiverInfo(m_CommonValProcess, pRowSelfReset);
            m_RowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRowSelfReset["IDE"]));
        }
        #endregion SetInfoBase
        #region SetParameter
        public void SetParameter()
        {
            CommonValParameterIRD parameter = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
            dayCountFraction = parameter.DCFRateBasis;
            selfDayCountFraction = parameter.SelfDayCountFraction;
            m_AveragingMethod = parameter.SelfAveragingMethod;
            Cst.ErrLevel errLevel = parameter.RateTreatmentRateBasis(out m_RateTreatment);
            m_RateTreatmentSpecified = (Cst.ErrLevel.SUCCESS == errLevel);
            m_PaymentFrequency = parameter.PaymentFrequency;
            intervalFrequency = parameter.SelfFrequency;
        }
        #endregion SetParameter


        #region protected ReadQuoteAssetRateIndex
        /// <summary>
        /// Retourne la valeur d'un asset de taux 
        /// <para>Par défaut Spheres® recherche la quotation de l'asset </para>
        /// <para>Cas du MarToMarket, lorsque la lecture n'aboutit pas, Spheres® estime le taux via un calcul de taux forward</para>
        /// </summary>
        /// <param name="pKeyQuote"></param>
        /// <param name="pCalcForwardRate"></param>
        /// <returns></returns>
        protected decimal ReadQuoteAssetRateIndex(out KeyQuote pKeyQuote, out EFS_CalcForwardRateIndex pCalcForwardRate)
        {
            decimal ret = decimal.Zero;
            EFS_CalcForwardRateIndex calcForward = null;
            //
            string cs = m_CommonValProcess.Process.Cs;
            int idAsset = m_IdAsset;
            IProductBase productBase = m_CommonValProcess.ProductBase;
            CommonValParameterIRD paramIRD = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
            //
            KeyQuote keyQuote = new KeyQuote(cs, fixingDate,
                                         m_PayerReceiver.Payer, m_PayerReceiver.BookPayer,
                                         m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver, QuoteTimingEnum.Close);
            //
            bool isNewAttempt =  false ;
            SystemMSGInfo systemMsgInfo = null;
            try
            {
                SQL_Quote quote = CommonValFunc.ReadQuote_AssetByType(cs,QuoteEnum.RATEINDEX, productBase, keyQuote, idAsset, ref systemMsgInfo);
                ret = quote.QuoteValue;
            }
            catch (SpheresException2 ex)
            {
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    isNewAttempt = (m_CommonValProcess.IsMarkToMarket);
                    if (false == isNewAttempt)
                    {
                        if (ProcessStateTools.IsStatusError(systemMsgInfo.processState.Status))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_CommonValProcess.Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            
                            
                            Logger.Log(systemMsgInfo.ToLoggerData(0));
                        }
                        throw new SpheresException2(systemMsgInfo.processState);
                    }
                }
                else
                    throw;
            }
            catch (Exception) { throw; };
            //
            //Calcul du taux forward
            if (isNewAttempt)
            {
                if (m_CommonValProcess.IsMarkToMarket)
                {
                    string idC;
                    if (productBase.IsFra)
                        idC = paramIRD.FraNotional.Currency;
                    else if (productBase.IsSwap || productBase.IsLoanDeposit || productBase.IsFxTermDeposit)
                        idC = paramIRD.Stream.StreamCurrency;
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Product {0} is not is not managed, please contact EFS", paramIRD.Product.GetType().FullName));
                    //
                    calcForward = new EFS_CalcForwardRateIndex(keyQuote, idAsset, idC, m_CommonValProcess.CommonValDate);
                    calcForward.Calc(cs, m_CommonValProcess.ProductBase, m_CommonValProcess.Process.Session);
                    ret = calcForward.ForwardRate; 
                }
                else
                    throw new NotImplementedException("process is not implemented");
            }
            //
            pKeyQuote = keyQuote;
            pCalcForwardRate = calcForward; 
            return ret;
        }
        #endregion Methods


        #endregion Methods
    }
    #endregion SelfResetInfo
    #region SettlementInfo
    public class SettlementInfo
    {
        #region Members
        protected CommonValProcessBase m_CommonValProcess;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected string m_Currency;
        protected string m_ReferenceCurrency;
        protected FxTypeEnum m_FxType;
        protected decimal m_NotionalAmount;
        protected decimal m_ForwardRate;
        protected decimal m_ConversionRate;
        protected decimal m_SettlementRate;
        #endregion members
        //
        #region Accessors
        public string ReferenceCurrency
        {
            get { return m_ReferenceCurrency; }
        }
        #endregion Accessors
        //
        #region Constructors
        public SettlementInfo(CommonValProcessBase pCommonValProcess, DataRow pRowSettlement)
        {
            SetInfoBase(pCommonValProcess, pRowSettlement);
        }
        #endregion Constructors
        //
        #region Methods
        #region private SetInfoBase
        private void SetInfoBase(CommonValProcessBase pCommonValProcess, DataRow pRowSettlement)
        {
            m_CommonValProcess = pCommonValProcess;
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowSettlement["IDE"]));
            m_StartDate = Convert.ToDateTime(pRowSettlement["DTSTARTADJ"]);
            m_EndDate = Convert.ToDateTime(pRowSettlement["DTENDADJ"]);
            m_Currency = pRowSettlement["UNIT"].ToString();
            if (null != rowDetail)
            {
                m_ReferenceCurrency = rowDetail["IDC_REF"].ToString();
                if (false == Convert.IsDBNull(rowDetail["FXTYPE"]) && Enum.IsDefined(typeof(FxTypeEnum), rowDetail["FXTYPE"].ToString()))
                    m_FxType = (FxTypeEnum)Enum.Parse(typeof(FxTypeEnum), rowDetail["FXTYPE"].ToString(), true);
                if (false == Convert.IsDBNull(rowDetail["NOTIONALAMOUNT"]))
                    m_NotionalAmount = Convert.ToDecimal(rowDetail["NOTIONALAMOUNT"]);
                if (false == Convert.IsDBNull(rowDetail["RATE"]))
                    m_ForwardRate = Convert.ToDecimal(rowDetail["RATE"]);
                if (false == Convert.IsDBNull(rowDetail["CONVERSIONRATE"]))
                    m_ConversionRate = Convert.ToDecimal(rowDetail["CONVERSIONRATE"]);
                else
                    m_ConversionRate = 1;
                if (false == Convert.IsDBNull(rowDetail["SETTLEMENTRATE"]))
                    m_SettlementRate = Convert.ToDecimal(rowDetail["SETTLEMENTRATE"]);
            }
        }
        #endregion SetInfoBase
        #endregion Methods
    }
    #endregion SettlementInfo
    #region VariationNominalInfo
    public class VariationNominalInfo : FxFixingInfo
    {
        #region Variables
        protected DateTime m_CommonValDate;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected DateTime m_PaymentDate;
        protected string m_Currency;
        protected DataRow m_RowStream;
        #endregion Variables
        //
        #region Accessors
        /// <summary>
        /// Obtient la date fin de l'évènement
        /// <para>Si traitement de réescompte, obtient la date de réesscompe (m_CommonValDate) lorsqu'elle elle est comprise en dtStart et dtEnd</para>
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                DateTime ret = m_EndDate;
                //
                if ((m_CommonValProcess.IsAccrualProcess || m_CommonValProcess.IsProvisionProcess) &&
                    (-1 < m_CommonValDate.CompareTo(m_StartDate)) && (1 > m_CommonValDate.CompareTo(m_EndDate)))
                    ret = m_CommonValDate;
                //
                return ret;
            }
        }
        #endregion Accessors
        #region Constructors
        public VariationNominalInfo(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowVariation)
            : base(pCommonValProcess, pRowVariation)
        {
            m_CommonValDate = pCommonValDate;
            m_CommonValProcess = pCommonValProcess;
            
            m_StartDate = Convert.ToDateTime(pRowVariation["DTSTARTADJ"]);
            m_EndDate = Convert.ToDateTime(pRowVariation["DTENDADJ"]);
            m_Currency = pRowVariation["UNIT"].ToString();
            #region Set PaymentDate
            DataRow[] rowEventClass = pRowVariation.GetChildRows(m_CommonValProcess.DsEvents.ChildEventClass);
            foreach (DataRow dr in rowEventClass)
            {
                string eventClass = dr["EVENTCLASS"].ToString();
                if (EventClassFunc.IsSettlement(eventClass))
                    m_PaymentDate = Convert.ToDateTime(dr["DTEVENT"]);
            }
            #endregion Set PaymentDate

            m_RowStream = pRowVariation.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
            m_PayerReceiver = new PayerReceiverInfo(m_CommonValProcess, m_RowStream);
        }
        #endregion Constructors
    }
    #endregion VariationNominalInfo

    #region EFS_CalculationPeriodEvent
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Parameter intervalFrequency used by EFS_CalculAmount
    ///     </comment>
    /// </revision>
    public class EFS_CalculationPeriodEvent : CalculationPeriodInfo
    {
        #region Members
        protected EFS_ResetEvent[] m_ResetEvents;
        //
        public Decimal averagedRate;
        public Decimal capFlooredRate;
        public Decimal calculatedRate;
        public Decimal calculatedAmount;
        public Decimal roundedCalculatedAmount;
        public Decimal compoundCalculatedAmount;
        #endregion Members
        //
        #region accessors
        public EFS_ResetEvent[] ResetEvents
        {
            get { return m_ResetEvents; }
        }
        #endregion

        #region Constructors
        /// <summary>
        ///  Construtor où roundedCalculatedAmount est obtenu par lecture de la table
        /// </summary>
        /// <param name="pCommonValDate"></param>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowCalcPeriod"></param>
        public EFS_CalculationPeriodEvent(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowCalcPeriod)
            : base(pCommonValDate, pCommonValProcess, pRowCalcPeriod)
        {
            // EG 20150605 [21011] New Test Payer différent de PayerPartyReference du Stream
            // Si différent alors on signe le montant
            calculatedAmount = Convert.ToDecimal(pRowCalcPeriod["VALORISATION"]);
            compoundCalculatedAmount = calculatedAmount;

            int idA_Pay = Convert.ToInt32(pRowCalcPeriod["IDA_PAY"]);
            // CC/PL 20160219 idB_Pay nullable on Cash-Balance-Interest
            //int idB_Pay = Convert.ToInt32(pRowCalcPeriod["IDB_PAY"]);
            Nullable<int> idB_Pay = null;
            if (false == Convert.IsDBNull(pRowCalcPeriod["IDB_PAY"]))
                idB_Pay = Convert.ToInt32(pRowCalcPeriod["IDB_PAY"]);

            if ((streamPayer.actor.First != idA_Pay) && (streamPayer.book.First != idB_Pay))
                calculatedAmount *= -1;

            roundedCalculatedAmount = calculatedAmount;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCommonValDate"></param>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowCalcPeriod"></param>
        /// <param name="pRateCutOffDate"></param>
        /// <param name="pIsAmountRelativeToStartDate"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_CalculationPeriodEvent(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowCalcPeriod, bool pIsAmountRelativeToStartDate)
            : base(pCommonValDate, pCommonValProcess, pRowCalcPeriod)
        {
            bool isRowUpdating = true;
            try
            {
                EFS_CalculAmount calculAmount;
                DataRow[] rowResets = pRowCalcPeriod.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                //-- 20080411 RD Ticket 16107
                if ((pIsAmountRelativeToStartDate) || EventTypeFunc.IsKnownAmount(pRowCalcPeriod["EVENTTYPE"].ToString()))
                {
                    #region CalculationPeriodStartDate or KnownAmountSchedule
                    calculatedAmount = Convert.ToDecimal(pRowCalcPeriod["VALORISATION"]);
                    #endregion CalculationPeriodStartDate or KnownAmountSchedule
                }
                else if ((0 != rowResets.Length) && (EventTypeFunc.IsFloatingRate(pRowCalcPeriod["EVENTTYPE"].ToString())))
                {
                    #region FloatingRate
                    EFS_ResetEvent resetEvent;
                    ArrayList aResetEvent = new ArrayList();
                    #region Reset Process
                    foreach (DataRow rowReset in rowResets)
                    {
                        if (m_CommonValProcess.IsRowMustBeCalculate(rowReset))
                        {
                            #region CapFloorPeriods Excluded
                            if (EventTypeFunc.IsCapFloorLeg(rowReset["EVENTTYPE"].ToString()))
                            {
                                rowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                                continue;
                            }
                            #endregion CapFloorPeriods Excluded

                            CommonValFunc.SetRowCalculated(rowReset);
                            resetEvent = new EFS_ResetEvent(m_CommonValDate, m_CommonValProcess, rowReset, m_IdAsset, m_IdAsset2, m_RateCutOffDate);
                            aResetEvent.Add(resetEvent);
                            if (false == CommonValFunc.IsRowEventCalculated(rowReset))
                                break;
                        }
                        else if (DtFunc.IsDateTimeFilled(m_CommonValProcess.CommonValDate) &&
                                 m_CommonValProcess.IsRowPrecededAccrual(rowReset) &&
                                 false == EventTypeFunc.IsCapFloorLeg(rowReset["EVENTTYPE"].ToString()))
                        {
                            aResetEvent.Add(new EFS_ResetEvent(m_CommonValDate, m_CommonValProcess, rowReset));
                        }
                    }
                    m_ResetEvents = (EFS_ResetEvent[])aResetEvent.ToArray(typeof(EFS_ResetEvent));
                    #endregion Reset Process
                    //
                    #region Final CalculationPeriod Process
                    if (m_CommonValProcess.IsRowsEventCalculated(rowResets))
                    {
                        #region AveragingRate / CapFlooring / FinalRateRounding / AmountCalculation
                        Averaging();
                        CapFlooring(pRowCalcPeriod);
                        FinalRateRounding();
                        //
                        #region AmountCalculation
                        calculAmount = new EFS_CalculAmount(nominal, multiplier, calculatedRate, spread, startDate, EndDate, dayCountFraction, intervalFrequency, null);
                        calculatedAmount = calculAmount.calculatedAmount.Value;
                        #endregion AmountCalculation
                        //
                        DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowCalcPeriod["IDE"]));
                        if (null != rowDetail)
                        {
                            // EG 20150605 [21011] Del Math.Abs
                            rowDetail["RATE"] = calculatedRate;
                            if (1 != multiplier)
                                rowDetail["MULTIPLIER"] = multiplier;
                            if (0 != spread)
                                rowDetail["SPREAD"] = spread;
                        }
                        #endregion AveragingRate / CapFlooring / FinalRateRounding / AmountCalculation
                    }
                    #endregion Final CalculationPeriod Process
                    #endregion FloatingRate
                }
                else if (EventTypeFunc.IsFixedRate(pRowCalcPeriod["EVENTTYPE"].ToString()))
                {
                    #region FixedRate
                    DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowCalcPeriod["IDE"]));
                    if (null != rowDetail)
                        calculatedRate = Convert.ToDecimal(rowDetail["RATE"]);
                    #region Amount Calculation
                    calculAmount = new EFS_CalculAmount(nominal, calculatedRate, startDate, EndDate, dayCountFraction, intervalFrequency);
                    calculatedAmount = calculAmount.calculatedAmount.Value;
                    #endregion Amount Calculation
                    #endregion FixedRate
                }
                else if (EventCodeFunc.IsDailyClosing(pRowCalcPeriod["EVENTCODE"].ToString()))
                {
                    isRowUpdating = false;
                }
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowCalcPeriod);
                m_CommonValProcess.SetRowStatus(pRowCalcPeriod, Tuning.TuningOutputTypeEnum.OEE);
                throw;
            }
            finally
            {
                if (isRowUpdating)
                    UpdatingRow(pRowCalcPeriod);
            }
        }
        #endregion Constructors
        //
        #region Methods
        #region Averaging
        /// <summary>
        ///
        /// </summary>
        private void Averaging()
        {
            if (m_AveragingMethodSpecified)
            {
                EFS_Averaging averaging = new EFS_Averaging(this, m_ResetEvents);
                averagedRate = averaging.averagedRate;
                calculatedRate = averagedRate;
            }
            else if ((1 == m_ResetEvents.Length) || (0 != m_IdAsset2))
                calculatedRate = ((EFS_ResetEvent)m_ResetEvents.GetValue(0)).observedRate;
        }
        #endregion Averaging
        #region CapFlooring
        private void CapFlooring(DataRow pRow)
        {
            CommonValParameterIRD parameter = (CommonValParameterIRD) m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
            DataRow[] rowCapFloors = pRow.GetChildRows(m_CommonValProcess.DsEvents.ChildEventByEventCode);
            if (0 != rowCapFloors.Length)
            {
                foreach (DataRow rowCapFloor in rowCapFloors)
                {
                    CommonValFunc.SetRowCalculated(rowCapFloor);
                    EFS_CapFloorPeriodEvent capFloorPeriodEvent =
                        new EFS_CapFloorPeriodEvent(m_CommonValDate, m_CommonValProcess, rowCapFloor, calculatedRate, m_Currency);
                    if (parameter.Product.ProductBase.IsCapFloor)
                        capFlooredRate += capFloorPeriodEvent.capFlooredRate;
                    else
                        capFlooredRate = Math.Max(capFlooredRate, capFloorPeriodEvent.capFlooredRate);

                    if (false == CommonValFunc.IsRowEventCalculated(rowCapFloor))
                        break;
                }
                if (m_CommonValProcess.IsRowsEventCalculated(rowCapFloors))
                {
                    // TicketId:10330 EG 20061121
                    calculatedRate = capFlooredRate;
                    DataRow rowPayment = pRow.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
                    DataRow rowStream = rowPayment.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
                    if (parameter.Product.ProductBase.IsCapFloor)
                    {
                        if (0 < calculatedRate)
                        {
                            #region Payment Payer = Stream Receiver
                            rowPayment["IDA_PAY"] = rowStream["IDA_REC"];
                            rowPayment["IDB_PAY"] = rowStream["IDB_REC"];
                            rowPayment["IDA_REC"] = rowStream["IDA_PAY"];
                            rowPayment["IDB_REC"] = rowStream["IDB_PAY"];
                            #endregion Payment Payer = Stream Receiver
                            #region Period Payer = Stream Receiver
                            pRow["IDA_PAY"] = rowStream["IDA_REC"];
                            pRow["IDB_PAY"] = rowStream["IDB_REC"];
                            pRow["IDA_REC"] = rowStream["IDA_PAY"];
                            pRow["IDB_REC"] = rowStream["IDB_PAY"];
                            #endregion Period Payer = Stream Receiver
                        }
                        else
                        {
                            #region Payment Payer = Stream Payer
                            rowPayment["IDA_PAY"] = rowStream["IDA_PAY"];
                            rowPayment["IDB_PAY"] = rowStream["IDB_PAY"];
                            rowPayment["IDA_REC"] = rowStream["IDA_REC"];
                            rowPayment["IDB_REC"] = rowStream["IDB_REC"];
                            #endregion Payment Payer = Stream Payer
                            #region Period Payer = Stream Receiver
                            pRow["IDA_PAY"] = rowStream["IDA_PAY"];
                            pRow["IDB_PAY"] = rowStream["IDB_PAY"];
                            pRow["IDA_REC"] = rowStream["IDA_REC"];
                            pRow["IDB_REC"] = rowStream["IDB_REC"];
                            #endregion Period Payer = Stream Receiver
                        }
                    }
                    calculatedRate = Math.Abs(calculatedRate);
                }
            }
        }
        #endregion CapFlooring
        #region FinalRateRounding
        private void FinalRateRounding()
        {
            if ((0 == m_IdAsset2) && (null != m_FinalRateRounding))
            {
                EFS_Round round = new EFS_Round(m_FinalRateRounding, calculatedRate);
                calculatedRate = round.AmountRounded;
            }
        }
        #endregion FinalRateRounding
        #region UpdatingRow
        /// <summary>
        /// Mise à jour de la ligne PER/FLO
        /// </summary>
        /// <param name="pRow"></param>
        /// RD 20150525 [21011] Modify
        private void UpdatingRow(DataRow pRow)
        {
            //RD 20150525 [21011] La valeur d'un Event est toujours positive            
            //compoundCalculatedAmount = calculatedAmount;
            compoundCalculatedAmount = Math.Abs(calculatedAmount);

            //RD 20150525 [21011] Synchronisation des acteurs/books avec la ligne Parent (Interest)
            //if (-1 == Math.Sign(calculatedAmount))
            //    CommonValFunc.SwapPayerAndReceiver(pRow);
            //EG 20150605 [21011] Synchronisation sur la base de streamPayer et streamReceiver
            CommonValFunc.SetPayerAndReceiver(pRow, calculatedAmount, streamPayer, streamReceiver);
      
            pRow["UNIT"] = m_Currency;
            pRow["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
            //RD 20150525 [21011] La valeur d'un Event est toujours positive            
            //pRow["VALORISATION"] = calculatedAmount;
            pRow["VALORISATION"] = Math.Abs(calculatedAmount);
            pRow["UNITSYS"] = m_Currency;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();
            //RD 20150525 [21011] La valeur d'un Event est toujours positive            
            //pRow["VALORISATIONSYS"] = calculatedAmount;
            pRow["VALORISATIONSYS"] = Math.Abs(calculatedAmount);

            CommonValFunc.SetRowCalculated(pRow);
            m_CommonValProcess.SetRowStatus(pRow, Tuning.TuningOutputTypeEnum.OES);

            DataRow rowDetail = m_CommonValProcess.GetRowDetail(m_IdE);
            if (null != rowDetail)
            {
                if (DtFunc.IsDateTimeFilled(m_CommonValDate))
                {
                    EFS_DayCountFraction dcf = new EFS_DayCountFraction(startDate, EndDate , dayCountFraction, intervalFrequency);
                    rowDetail["DCFNUM"] = dcf.Numerator;
                    rowDetail["TOTALOFYEAR"] = dcf.NumberOfCalendarYears;
                    rowDetail["TOTALOFDAY"] = dcf.TotalNumberOfCalendarDays;
                }
                rowDetail["INTEREST"] = calculatedAmount;
            }
        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_CalculationPeriodEvent
    #region EFS_CapFloorPeriodEvent
    public class EFS_CapFloorPeriodEvent : CapFloorPeriodInfo
    {
        #region Members
        public decimal capFlooredRate;
        public decimal capFlooredStrike;
        #endregion 
        //
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_CapFloorPeriodEvent(DateTime pCommonValDate,
            CommonValProcessBase pCommonValProcess, DataRow pRowCapFloor,
            decimal pSourceRate, string pCurrency)
            : base(pCommonValDate, pCommonValProcess, pRowCapFloor, pSourceRate, pCurrency)
        {
            try
            {
                #region Process
                if (null != strikeSchedules)
                {
                    EFS_CapFlooring capFlooring = new EFS_CapFlooring(m_CommonValProcess.Process.Cs, this, m_CommonValProcess.TradeLibrary.DataDocument);
                    capFlooredRate = capFlooring.capFlooredRate;
                    capFlooredStrike = capFlooring.strike;
                    UpdatingRow(pRowCapFloor);
                }
                CommonValFunc.SetRowCalculated(pRowCapFloor);
                #endregion Process
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowCapFloor);
                throw;
            }
        }
        #endregion Constructors
        #region UpdatingRow
        private void UpdatingRow(DataRow pRow)
        {
            pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
            pRow["VALORISATION"] = Math.Abs(capFlooredRate);
            pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();
            pRow["VALORISATIONSYS"] = Math.Abs(capFlooredRate);
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null == rowDetail)
            {
                rowDetail = m_CommonValProcess.DsEvents.DtEventDet.NewRow();
                rowDetail["IDE"] = pRow["IDE"];
                rowDetail.SetParentRow(pRow);
                m_CommonValProcess.DsEvents.DtEventDet.Rows.Add(rowDetail);
            }
            rowDetail["STRIKEPRICE"] = capFlooredStrike;
            rowDetail["RATE"] = Math.Abs(capFlooredRate);
            CommonValFunc.SetRowCalculated(pRow);
        }
        #endregion UpdatingRow
    }
    #endregion EFS_CapFloorPeriodEvent2
    #region EFS_ExchangeCurrencyFixingEvent
    public class EFS_ExchangeCurrencyFixingEvent : ExchangeFixingInfo
    {
        #region Members
        protected EFS_FXResetEvent[] m_FXResetEvents;
        /// <summary>
        /// Représente le résultat
        /// </summary>
        public decimal exchangeCurrencyCounterValueAmount;
        #endregion Members
        //
        #region Accessors
        public bool IsExistReset
        {
            get { return ArrFunc.IsFilled(m_FXResetEvents); }
        }
        #endregion Accessors
        //
        #region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_ExchangeCurrencyFixingEvent(CommonValProcessBase pCommonValProcess, DataRow pRowExchangeCurrencyFixing)
            : base(pCommonValProcess, pRowExchangeCurrencyFixing)
        {
            try
            {
                #region Reset Process
                DataRow[] rowResets = pRowExchangeCurrencyFixing.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                if (0 < rowResets.Length)
                {
                    EFS_FXResetEvent fxResetEvent;
                    ArrayList aFxResetEvent = new ArrayList();
                    foreach (DataRow rowReset in rowResets)
                    {
                        if (m_CommonValProcess.IsRowMustBeCalculate(rowReset))
                        {
                            CommonValFunc.SetRowCalculated(rowReset);
                            fxResetEvent = new EFS_FXResetEvent(m_CommonValProcess, rowReset);
                            aFxResetEvent.Add(fxResetEvent);
                            if (false == CommonValFunc.IsRowEventCalculated(rowReset))
                                break;
                        }
                    }
                    m_FXResetEvents = (EFS_FXResetEvent[])aFxResetEvent.ToArray(typeof(EFS_FXResetEvent));
                    #region Final Settlement Process
                    if (m_CommonValProcess.IsRowsEventCalculated(rowResets) && (null != m_FXResetEvents) && (0 < m_FXResetEvents.Length))
                    {
                        ExchangeCurrencyCounterValue();
                        UpdatingRow(pRowExchangeCurrencyFixing);
                    }
                    #endregion Final Settlement Process
                }
                #endregion Reset Process
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowExchangeCurrencyFixing);
                m_CommonValProcess.SetRowStatus(pRowExchangeCurrencyFixing, TuningOutputTypeEnum.OEE);
                throw;
            }
        }
        #endregion Constructors
        #region Methods
        #region private ExchangeCurrencyCounterValue
        private void ExchangeCurrencyCounterValue()
        {
            EFS_FXResetEvent reset = m_FXResetEvents[0];
            m_Rate = reset.quotedRate;
            m_RateRead = reset.quotedRate;

            if (((QuoteBasisEnum.Currency2PerCurrency1 == reset.QuoteBasis) && (reset.Currency2 == m_ReferenceCurrency)) ||
                ((QuoteBasisEnum.Currency1PerCurrency2 == reset.QuoteBasis) && (reset.Currency1 == m_ReferenceCurrency)))
                m_Rate = reset.quotedRate;
            else if (0 < reset.quotedRate)
            {
                m_Rate = 1 / reset.quotedRate;
                m_IsReverse = true;
            }

            decimal amount = m_NotionalAmount * m_Rate;
            exchangeCurrencyCounterValueAmount = m_CommonValProcess.RoundingCurrencyAmount(m_Currency, amount);
        }
        #endregion ExchangeCurrencyCounterValue
        #region private UpdatingRow
        private void UpdatingRow(DataRow pRow)
        {
            pRow["VALORISATION"] = exchangeCurrencyCounterValueAmount;
            pRow["VALORISATIONSYS"] = exchangeCurrencyCounterValueAmount;
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowDetail)
                rowDetail["RATE"] = m_Rate;
            CommonValFunc.SetRowCalculated(pRow);
        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_ExchangeCurrencyFixingEvent
    #region EFS_FixingEvent
    public class EFS_FXFixingEvent : FxFixingInfo
    {
        #region Members
        /// <summary>
        /// Représente le résultat
        /// </summary>
        public decimal observedRate;
        #endregion Members
        #region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public EFS_FXFixingEvent(CommonValProcessBase pCommonValProcess, DataRow pRowFixing)
            : base(pCommonValProcess, pRowFixing)
        {
            bool isException = false;
            SystemMSGInfo systemMsgInfo = null;
            try
            {
                //20090609 PL Add QuoteTimingEnum.Close
                KeyQuote keyQuote = new KeyQuote(pCommonValProcess.Process.Cs, m_Time,
                                                 m_PayerReceiver.Payer, m_PayerReceiver.BookPayer,
                                                 m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver, QuoteTimingEnum.Close);
                SQL_Quote quote = null;
                if (0 == m_IdAsset)
                {
                    KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                    {
                        IdC1 = m_Currency1,
                        IdC2 = m_Currency2,
                        IdBCRateSrc = m_IdBC,
                        QuoteBasisSpecified = true,
                        QuoteBasis = m_Basis,
                        PrimaryRateSrc = m_PrimaryRateSrc,
                        PrimaryRateSrcPage = m_PrimaryRateSrcPage,
                        PrimaryRateSrcHead = m_PrimaryRateSrcHead
                    };
                    quote = CommonValFunc.ReadQuote_AssetByType(pCommonValProcess.Process.Cs,
                        QuoteEnum.FXRATE, pCommonValProcess.ProductBase, keyQuote, keyAssetFXRate, ref systemMsgInfo);
                }
                else
                    quote = CommonValFunc.ReadQuote_AssetByType(pCommonValProcess.Process.Cs,
                        QuoteEnum.FXRATE, pCommonValProcess.ProductBase, keyQuote, m_IdAsset, ref systemMsgInfo);

                if (null != quote)
                    observedRate = quote.QuoteValue;

                UpdatingRow(pRowFixing, keyQuote);
            }
            catch (SpheresException2 ex)
            {
                isException = true;
                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    if (ProcessStateTools.IsStatusError(systemMsgInfo.processState.Status))
                    {
                        // FI 20200623 [XXXXX] SetErrorWarning
                        pCommonValProcess.Process.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        
                        
                        Logger.Log(systemMsgInfo.ToLoggerData(0));

                        throw new SpheresException2(systemMsgInfo.processState);
                    }
                }
                else
                    throw;
            }
            catch (Exception) { isException = true; throw; }
            finally
            {
                if (isException)
                {
                    CommonValProcessBase.ResetRowCalculated(pRowFixing);
                    m_CommonValProcess.SetRowStatus(pRowFixing, TuningOutputTypeEnum.OEE);
                }
            }
        }
        #endregion Constructors
        #region Methods
        #region private UpdatingRow
        private void UpdatingRow(DataRow pRow, KeyQuote pKeyQuote)
        {

            pRow["VALORISATION"] = observedRate;
            pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
            // 20071121 EG Add SYS Valuation
            pRow["VALORISATIONSYS"] = observedRate;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();
            //
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowDetail)
                rowDetail["RATE"] = observedRate;
            //
            CommonValFunc.SetRowCalculated(pRow);
            m_CommonValProcess.SetRowStatus(pRow, TuningOutputTypeEnum.OES);
            //
            if ((null != m_RowAssets) || (1 == m_RowAssets.Length))
            {
                m_RowAssets[0]["IDMARKETENV"] = pKeyQuote.IdMarketEnv;
                m_RowAssets[0]["IDVALSCENARIO"] = pKeyQuote.IdValScenario;
                m_RowAssets[0]["QUOTESIDE"] = pKeyQuote.QuoteSide;
            }

        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_FXFixingEvent
    #region EFS_FXResetEvent
    public class EFS_FXResetEvent : FxResetInfo
    {
        #region Members
        protected EFS_FXFixingEvent[] m_FXFixingEvents;
        
        /// <summary>
        /// Représente le résultat
        /// </summary>
        public decimal quotedRate;
        #endregion Members
        //
        #region Constructors
        public EFS_FXResetEvent(CommonValProcessBase pCommonValProcess, DataRow pRowReset)
            : base(pCommonValProcess, pRowReset)
        {
            try
            {
                #region Fixing Process
                DataRow[] rowFixings = pRowReset.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                EFS_FXFixingEvent fxFixingEvent;
                if (0 < rowFixings.Length)
                {
                    #region Average Fixings
                    ArrayList aFxFixingEvent = new ArrayList();
                    foreach (DataRow rowFixing in rowFixings)
                    {
                        if (m_CommonValProcess.IsRowMustBeCalculate(rowFixing))
                        {
                            CommonValFunc.SetRowCalculated(rowFixing);
                            fxFixingEvent = new EFS_FXFixingEvent(m_CommonValProcess, rowFixing);
                            aFxFixingEvent.Add(fxFixingEvent);
                            if (false == CommonValFunc.IsRowEventCalculated(rowFixing))
                                break;
                        }
                    }
                    m_FXFixingEvents = (EFS_FXFixingEvent[])aFxFixingEvent.ToArray(typeof(EFS_FXFixingEvent));
                    if (m_CommonValProcess.IsRowsEventCalculated(rowFixings))
                    {
                        Averaging();
                        UpdatingRow(pRowReset);
                        pRowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                    }
                    #endregion Average Fixings
                }
                else
                {
                    #region Unit Fixings
                    fxFixingEvent = new EFS_FXFixingEvent(m_CommonValProcess, pRowReset);
                    quotedRate = fxFixingEvent.observedRate;
                    if (CommonValFunc.IsRowEventCalculated(pRowReset))
                    {
                        UpdatingRow(pRowReset);
                        pRowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                    }
                    #endregion Unit Fixings
                }
                CommonValFunc.SetRowCalculated(pRowReset);
                m_CommonValProcess.SetRowStatus(pRowReset, TuningOutputTypeEnum.OES);
                #endregion Fixing Process

                if (false == Convert.IsDBNull(pRowReset["VALORISATION"]))
                    quotedRate = Convert.ToDecimal(pRowReset["VALORISATION"]);

            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowReset);
                m_CommonValProcess.SetRowStatus(pRowReset, TuningOutputTypeEnum.OEE);
                throw;
            }
        }
        #endregion Constructors
        //
        #region Methods
        #region private Averaging
        private void Averaging()
        {
            EFS_Averaging averaging = new EFS_Averaging(AveragingMethodEnum.Unweighted, m_FXFixingEvents);
            quotedRate = averaging.averagedRate;
        }
        #endregion Averaging
        #region private UpdatingRow
        private void UpdatingRow(DataRow pRow)
        {
            pRow["VALORISATION"] = quotedRate;
            pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
            pRow["VALORISATIONSYS"] = quotedRate;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowDetail)
                rowDetail["RATE"] = quotedRate;
        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_FXResetEvent
    #region EFS_PaymentEvent
    /// <summary>
    /// </summary>
    ///
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Parameter calculationPeriodFrequency to EFS_DayCountFraction accessors
    ///     IMPORTANT !!! this accessor is only used by Accrualgen process
    ///     </comment>
    /// </revision>
    public class EFS_PaymentEvent : PaymentInfo
    {
        #region Members
        //
        protected EFS_CalculationPeriodEvent[] m_CalcPeriodEvents;
        //
        /// <summary>
        /// Represésente le montant des intérêts calculés
        /// <para>Ce montant est toujours &gt;=0</para>
        /// </summary>
        public decimal interestAmount;
        /// <summary>
        /// Represésente le montant des intérêts calculés après application des règles d'arrondi
        /// <para>Ce montant est toujours &gt;=0</para>
        /// </summary>
        public decimal roundedInterestAmount;
        /// <summary>
        /// 
        /// </summary>
        // EG 20160404 Migration vs2013
        //private bool _isNegativeInterest;
        #endregion Memebers
        //
        #region Accessors
        /// <summary>
        /// Obtient les periodes de calcul utilisées pour déterminer le montant des intérêts
        /// </summary>
        public EFS_CalculationPeriodEvent[] CalcPeriodEvents
        {
            get { return m_CalcPeriodEvents; }
        }
        /// <summary>
        /// Obtient true si la calcul des intérêts a donné lieu à un montant &lt; que 0.
        /// <para>Dans ce cas le payer et receiver sont échangés pour que interestAmount soit toujours positif</para>
        /// </summary>
        // EG 20160404 Migration vs2013
        //public bool isNegativeInterest
        //{
        //    get { return _isNegativeInterest; }
        //}
        #endregion
        //
        #region Constructors
        /// <summary>
        /// Classe chargée de calculer/estimer le montant des intérêts
        /// <para>NB: Le Payer/Receiver sont inversés pour le taux négatif</para>
        /// </summary>
        ///<exception cref="SpheresException2 si une cotation est non trouvée [QUOTENOTFOUND]"></exception> 
        /// <param name="pCommonValDate"></param>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowInterest"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_PaymentEvent(DateTime pCommonValDate, CommonValProcessBase pCommonValProcess, DataRow pRowInterest)
            : base(pCommonValDate, pCommonValProcess, pRowInterest)
        {
            try
            {
                #region CalculationPeriod Process
                DataRow[] rowCalcPeriods = pRowInterest.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                if (0 != rowCalcPeriods.Length)
                {
                    //-- 20080411 RD Ticket 16107 
                    //-- 20080414 EG Ticket 16107 (comment next line)
                    // bool isSimpleCalculatedAmount = pIsAccruals && isPayRelativeToStartDate;
                    ArrayList aCalcPeriodEvent = new ArrayList();
                    foreach (DataRow rowCalcPeriod in rowCalcPeriods)
                    {
                        if (m_CommonValProcess.IsRowMustBeCalculate(rowCalcPeriod))
                        {
                            CommonValFunc.SetRowCalculated(rowCalcPeriod);
                            EFS_CalculationPeriodEvent calcPeriodEvent = new EFS_CalculationPeriodEvent(m_CommonValDate, m_CommonValProcess, rowCalcPeriod, m_IsAmountRelativeToStartDate);
                            aCalcPeriodEvent.Add(calcPeriodEvent);
                            //
                            if (false == CommonValFunc.IsRowEventCalculated(rowCalcPeriod))
                                break;
                        }
                        else if (DtFunc.IsDateTimeFilled(m_CommonValProcess.CommonValDate) &&
                                 m_CommonValProcess.IsRowPrecededAccrual(rowCalcPeriod) &&
                                 EventCodeFunc.IsCalculationPeriod(rowCalcPeriod["EVENTCODE"].ToString()))
                        {
                            EFS_CalculationPeriodEvent calcPeriodEvent = new EFS_CalculationPeriodEvent(m_CommonValDate, m_CommonValProcess, rowCalcPeriod);
                            aCalcPeriodEvent.Add(calcPeriodEvent);
                        }
                        // EG 20150605 [21011] New On instancie une EFS_CalculationPeriodEvent avec les données de EVENT sans recalcul (cas des multi-périodes  pour un payment)
                        else if (CommonValFunc.IsRowEventCalculated(rowCalcPeriod) &&
                                 EventCodeFunc.IsCalculationPeriod(rowCalcPeriod["EVENTCODE"].ToString()))
                        {
                            EFS_CalculationPeriodEvent calcPeriodEvent = new EFS_CalculationPeriodEvent(m_CommonValDate, m_CommonValProcess, rowCalcPeriod);
                            aCalcPeriodEvent.Add(calcPeriodEvent);
                        }
                    }
                    //
                    m_CalcPeriodEvents = (EFS_CalculationPeriodEvent[])aCalcPeriodEvent.ToArray(typeof(EFS_CalculationPeriodEvent));
                    //
                    #region Final Payment Process
                    if (m_CommonValProcess.IsRowsEventCalculated(rowCalcPeriods))
                    {
                        #region Compounding / Discounting / NegativeInterestRateTreatment / Rounding
                        Compounding();
                        UpdateEventDetInterestAmount(pRowInterest);
                        //
                        if (Discounting())
                            NegativeInterestRateTreatment();
                        //
                        roundedInterestAmount = interestAmount;
                        //
                        if (m_CommonValProcess.Product.ProductBase.IsDebtSecurity)
                            roundedInterestAmount = m_CommonValProcess.RoundingDebtSecurityUnitCouponAmount(interestAmount);
                        else
                            roundedInterestAmount = m_CommonValProcess.RoundingCurrencyAmount(m_Currency, interestAmount);
                        #endregion Compounding / Discounting / NegativeInterestRateTreatment / Rounding
                        //
                        UpdatingRow(pRowInterest);
                    }
                    #endregion Final Payment Process
                }
                else if (m_CommonValProcess.Product.ProductBase.IsDebtSecurityTransaction &&
                         m_CommonValProcess.IsRowMustBeCalculate(pRowInterest))
                {
                    FullCouponCalculation(pRowInterest);
                    roundedInterestAmount = m_CommonValProcess.RoundingDebtSecurityTransactionFullCouponAmount(interestAmount);
                    UpdatingRow(pRowInterest);
                }
                #endregion CalculationPeriod Process
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowInterest);
                m_CommonValProcess.SetRowStatus(pRowInterest, Tuning.TuningOutputTypeEnum.OEE);
                throw;
            }
        }
        #endregion Constructors
        //
        #region Methods
        #region public UpdateCalculationPeriodAmount
        public void UpdateCalculationPeriodAmount()
        {
            foreach (EFS_CalculationPeriodEvent calcPeriodEvents in m_CalcPeriodEvents)
            {
                DataRow rowCalculation = m_CommonValProcess.GetRowEvent(calcPeriodEvents.IdE);
                if (null != rowCalculation)
                {
                    rowCalculation["VALORISATION"] = calcPeriodEvents.compoundCalculatedAmount;
                    rowCalculation["VALORISATIONSYS"] = calcPeriodEvents.compoundCalculatedAmount;
                }
            }
        }
        #endregion UpdateCalculationPeriodAmount
        #region public GetDayCountFraction
        // 20071106 EG Ticket 15859 
        public EFS_DayCountFraction GetDayCountFraction()
        {
            return new EFS_DayCountFraction(StartDate, EndDate, m_CalcPeriodEvents[0].dayCountFraction, intervalFrequency);
        }
        #endregion GetDayCountFraction
        //
        #region private Compounding
        private void Compounding()
        {
            if (m_CompoundingMethodSpecified)
            {
                EFS_CompoundingAmount compounding =
                    new EFS_CompoundingAmount(m_CompoundingMethod, m_CalcPeriodEvents, m_NegativeInterestRateTreatment);
                interestAmount = compounding.calculatedValue;
                UpdateCalculationPeriodAmount();
            }
        }
        #endregion Compounding
        #region private Discounting
        private bool Discounting()
        {
            bool isNegativeInterestRateTreatmentApply = true;
            #region Discounting
            EFS_CalculationPeriodEvent calcPeriodEvent = (EFS_CalculationPeriodEvent)m_CalcPeriodEvents.GetValue(0);
            decimal discountRate;
            string discountRate_DCF;
            decimal floatingRate = calcPeriodEvent.calculatedRate;
            string floatingRate_DCF = calcPeriodEvent.dayCountFraction;

            if (m_DiscountingSpecified)
            {
                #region Discount Elements
                if (m_Discounting.RateSpecified)
                    discountRate = m_Discounting.Rate;
                else
                    discountRate = floatingRate;

                if (m_Discounting.DayCountFractionSpecified)
                    discountRate_DCF = m_Discounting.DayCountFraction.ToString();
                else
                    discountRate_DCF = floatingRate_DCF;
                #endregion Discount Elements

                if (DiscountingTypeEnum.FRA == m_Discounting.DiscountingType)
                {
                    #region ISDA Discounting
                    if (Cst.ErrLevel.SUCCESS == m_CommonValProcess.FixedRateAndDCFFromRowInterest(m_StartDate, out decimal fixedRate, out string fixedRate_DCF))
                    {
                        // 20071106 EG Ticker 15859
                        EFS_ISDADiscounting isdaDiscounting =
                            new EFS_ISDADiscounting(m_CommonValProcess.TradeLibrary.Product, calcPeriodEvent.roundedCalculatedAmount, StartDate, EndDate,
                            discountRate, discountRate_DCF, floatingRate, fixedRate, fixedRate_DCF);
                        interestAmount = isdaDiscounting.discountedValue;
                    }
                    #endregion ISDA Discounting
                    isNegativeInterestRateTreatmentApply = false;
                }
                else
                {
                    #region Standard Discounting
                    // 20071106 EG Ticker 15859
                    EFS_StandardDiscounting standardDiscounting =
                        new EFS_StandardDiscounting(m_CommonValProcess.TradeLibrary.Product, calcPeriodEvent.roundedCalculatedAmount, false,
                        StartDate, EndDate, discountRate, discountRate_DCF);
                    interestAmount = standardDiscounting.discountedValue;
                    #endregion Standard Discounting
                }
            }
            else if (FraDiscountingEnum.NONE != m_FraDiscounting)
            {
                #region Discount Elements
                discountRate = floatingRate;
                discountRate_DCF = floatingRate_DCF;
                #endregion Discount Elements

                if (FraDiscountingEnum.ISDA == m_FraDiscounting)
                {
                    #region ISDA Discounting
                    // Replace first parameter interestAmount par m_Fra_Notional
                    EFS_ISDADiscounting isdaDiscounting =
                        new EFS_ISDADiscounting(m_CommonValProcess.TradeLibrary.Product, m_Fra_Notional, StartDate, EndDate,
                        discountRate, discountRate_DCF, floatingRate, m_FraFixedRate, floatingRate_DCF);
                    interestAmount = isdaDiscounting.discountedValue;
                    #endregion ISDA Discounting
                    isNegativeInterestRateTreatmentApply = false;
                }
                else if (FraDiscountingEnum.AFMA == m_FraDiscounting)
                {
                    #region AFMA Discounting
                    EFS_AFMADiscounting afmaDiscounting =
                        new EFS_AFMADiscounting(m_CommonValProcess.TradeLibrary.Product, m_Fra_Notional, StartDate, EndDate,
                        discountRate, discountRate_DCF, floatingRate, floatingRate_DCF, m_FraFixedRate, m_Fra_DCF);
                    interestAmount = afmaDiscounting.discountedValue;
                    #endregion AFMA Discounting
                }
            }
            #endregion Discounting
            return isNegativeInterestRateTreatmentApply;
        }
        #endregion Discounting
        #region private FullCouponCalculation
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20220908 [XXXX][WI418] Suppression de la classe obsolète EFSParameter
        private void FullCouponCalculation(DataRow pRowInterest)
        {
            decimal unitCoupon = 0;
            if (m_CommonValProcess.IsQuote_SecurityAsset)
            {
                Quote_SecurityAsset _quote = m_CommonValProcess.Quote as Quote_SecurityAsset;
                if (_quote.unitCouponSpecified)
                    unitCoupon = _quote.unitCoupon;
            }
            else
            {
                int idE_Source = Convert.ToInt32(pRowInterest["IDE_SOURCE"]);
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(m_CommonValProcess.Process.Cs, "IDE", DbType.Int64), idE_Source);
                string sqlSelect = QueryLibraryTools.GetQuerySelect(m_CommonValProcess.Process.Cs, Cst.OTCml_TBL.EVENT);
                sqlSelect += SQLCst.WHERE + "IDE=@IDE";
                QueryParameters qry = new QueryParameters(m_CommonValProcess.Process.Cs, sqlSelect, parameters);
                using (IDataReader drSource = DataHelper.ExecuteReader(m_CommonValProcess.Process.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                {
                    if (drSource.Read())
                    {
                        if (false == Convert.IsDBNull(drSource["VALORISATION"]))
                            unitCoupon = Convert.ToDecimal(drSource["VALORISATION"]);
                        else
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Event of Attached DebtSecurity not calculated id [" + idE_Source.ToString() + "]");
                    }
                }
            }
            interestAmount = unitCoupon * m_Sec_Quantity;
        }
        #endregion FullCouponCalculation
        #region private NegativeInterestRateTreatment
        /// <summary>
        /// Affecte interestAmount avec la valeur 0 si NegativeInterestRateTreatmentEnum == ZeroInterestRateMethod et si interestAmount est négatif
        /// </summary>
        private void NegativeInterestRateTreatment()
        {
            if (m_NegativeInterestRateTreatmentSpecified && (-1 == Math.Sign(interestAmount)) &&
                (NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
                interestAmount = 0;
        }
        #endregion NegativeInterestRateTreatment
        #region private UpdatingRow
        /// <summary>
        /// Classe chargée de la mise à jour de la ligne du montant des intérêts:
        /// <para>Devise, valorisation, , Statut, ...</para>
        /// <para>NB: Le Payer/Receiver sont inversés pour le taux négatif</para>
        /// </summary>
        /// <param name="pRow"></param>
        private void UpdatingRow(DataRow pRow)
        {
            //if (-1 == Math.Sign(roundedInterestAmount))
            //{
            //    _isNegativeInterest = true;
            //    CommonValFunc.SwapPayerAndReceiver(pRow);
            //}
            //EG 20150605 [21011] Synchronisation sur la base de streamPayer et streamReceiver
            CommonValFunc.SetPayerAndReceiver(pRow, roundedInterestAmount, streamPayer, streamReceiver);

            pRow["UNIT"] = m_Currency;
            pRow["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
            pRow["VALORISATION"] = Math.Abs(roundedInterestAmount);
            pRow["UNITSYS"] = m_Currency;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();
            pRow["VALORISATIONSYS"] = Math.Abs(roundedInterestAmount);
            CommonValFunc.SetRowCalculated(pRow);
            m_CommonValProcess.SetRowStatus(pRow, Tuning.TuningOutputTypeEnum.OES);
        }
        #endregion UpdatingRow
        #region private UpdateEventDetInterestAmount
        private void UpdateEventDetInterestAmount(DataRow pRow)
        {
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null == rowDetail)
            {
                rowDetail = m_CommonValProcess.DsEvents.DtEventDet.NewRow();
                rowDetail.BeginEdit();
                rowDetail["IDE"] = Convert.ToInt32(pRow["IDE"]);
                rowDetail.EndEdit();
                m_CommonValProcess.DsEvents.DtEventDet.Rows.Add(rowDetail);
            }
            if (null != rowDetail)
                rowDetail["INTEREST"] = interestAmount;
        }
        #endregion UpdateEventDetInterestAmount
        //
        #endregion Methods
    }
    #endregion EFS_PaymentEvent
    #region EFS_ResetEvent
    public class EFS_ResetEvent : ResetInfo
    {
        #region Members
        protected EFS_SelfAveragingEvent[] m_SelfAveragingEvents;
        //
        /// <summary>
        /// <para>Taux lu ou estimé s'il n'existe pas des selfAverage</para>
        /// <para>Taux Compound s'il existe des selfAverage</para>
        /// </summary>
        public decimal observedRate;
        /// <summary>
        /// Taux obtenu après traitement 
        /// </summary>
        public decimal treatedRate;
        #endregion Members
        

        #region Constructors
        /// <summary>
        /// Constructor où observedRate et  treatedRate sont obtenus par lecture des table  EVENT (treatedRate) et EVENTDET (observedRate)
        /// </summary>
        /// <param name="pAccrualDate"></param>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowReset"></param>
        public EFS_ResetEvent(DateTime pAccrualDate, CommonValProcessBase pCommonValProcess, DataRow pRowReset)
            : base(pAccrualDate, pCommonValProcess, pRowReset)
        {
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRowReset["IDE"]));
            if (null != rowDetail)
                observedRate = Convert.ToDecimal(rowDetail["RATE"]);
            treatedRate = Convert.ToDecimal(pRowReset["VALORISATION"]);
        }
        /// <summary>
        /// Constructor où
        /// </summary>
        /// <param name="pAccrualDate"></param>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowReset"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pIdAsset2"></param>
        /// <param name="pRateCutOffDate"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_ResetEvent(DateTime pAccrualDate, CommonValProcessBase pCommonValProcess, DataRow pRowReset, int pIdAsset,
            int pIdAsset2, DateTime pRateCutOffDate)
            : base(pAccrualDate, pCommonValProcess, pRowReset, pIdAsset, pIdAsset2, pRateCutOffDate)
        {
            try
            {
                #region Process
                // EG 20161116 Gestion InitialRate (RATP)
                #region InitialRate
                bool isContinue = true;
                CommonValParameterIRD parameter = (CommonValParameterIRD)m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo, m_CommonValProcess.ParamStreamNo];
                bool initialRateSpecified = parameter.Stream.CalculationPeriodAmount.Calculation.RateFloatingRate.InitialRateSpecified;
                if (initialRateSpecified)
                {
                    decimal initialRate = parameter.Stream.CalculationPeriodAmount.Calculation.RateFloatingRate.InitialRate.DecValue;
                    DateTime dt = DateTime.MinValue;
                    if (parameter.Stream.CalculationPeriodDates.FirstRegularPeriodStartDateSpecified)
                    {
                        dt = parameter.Stream.CalculationPeriodDates.FirstRegularPeriodStartDate.DateValue;
                    }
                    else
                    {
                        DataRow rowCalcPeriod = pRowReset.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
                        DataRow rowPaymentPeriod = rowCalcPeriod.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
                        DataRow rowStream = rowPaymentPeriod.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
                        dt = Convert.ToDateTime(rowStream["DTSTARTADJ"]);
                    }
                    if (m_StartDate == dt)
                    {
                        isContinue = false;
                        treatedRate = initialRate;
                        observedRate = initialRate;
                        UpdatingRow(pRowReset);
                    }
                }
                #endregion InitialRate

                if (isContinue)
                {
                    DataRow[] rowSelfAverages = pRowReset.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                    if (ArrFunc.IsEmpty(rowSelfAverages))
                    {
                        #region Interpolating or Observated Rate
                        if (0 != m_IdAsset2)
                        {
                            Interpolating();
                        }
                        else
                        {
                            observedRate = ReadQuoteAssetRateIndex(ReadQuoteAssetEnum.Asset1, out KeyQuote keyQuote, out EFS_CalcForwardRateIndex calcForwardRate);
                            UpdatingRowAsset(m_RowAssets[0], keyQuote, calcForwardRate, observedRate);
                        }
                        //
                        RateTreatement();
                        UpdatingRow(pRowReset);
                        #endregion Interpolating or Observated Rate
                    }
                    else
                    {
                        #region SelfCompounding
                        ArrayList aSelfAveragingEvent = new ArrayList();
                        #region SelfAveraging Process
                        foreach (DataRow rowSelfAverage in rowSelfAverages)
                        {
                            if (m_CommonValProcess.IsRowMustBeCalculate(rowSelfAverage))
                            {
                                CommonValFunc.SetRowCalculated(rowSelfAverage);
                                EFS_SelfAveragingEvent selfAveragingEvent = new EFS_SelfAveragingEvent(m_CommonValDate, m_CommonValProcess, rowSelfAverage, m_RateCutOffDate);
                                aSelfAveragingEvent.Add(selfAveragingEvent);
                                if (false == CommonValFunc.IsRowEventCalculated(rowSelfAverage))
                                    break;
                            }
                            else if (DtFunc.IsDateTimeEmpty(m_CommonValProcess.CommonValDate) ||
                                m_CommonValProcess.IsRowPrecededAccrual(rowSelfAverage))
                            {
                                EFS_SelfAveragingEvent selfAveragingEvent = new EFS_SelfAveragingEvent(m_CommonValDate, m_CommonValProcess, rowSelfAverage);
                                aSelfAveragingEvent.Add(selfAveragingEvent);
                            }
                        }
                        m_SelfAveragingEvents = (EFS_SelfAveragingEvent[])aSelfAveragingEvent.ToArray(typeof(EFS_SelfAveragingEvent));
                        #endregion SelfAveraging Process
                        #region Final SelfAveraging Process
                        if (m_CommonValProcess.IsRowsEventCalculated(rowSelfAverages))
                        {
                            Compounding();
                            RateRounding();
                            RateTreatement();
                            UpdatingRow(pRowReset);
                            pRowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                        }
                        #endregion Final SelfAveraging Process
                        #endregion SelfCompounding
                    }
                }
                CommonValFunc.SetRowCalculated(pRowReset);
                m_CommonValProcess.SetRowStatus(pRowReset, Tuning.TuningOutputTypeEnum.OES);
                #endregion Process
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowReset);
                m_CommonValProcess.SetRowStatus(pRowReset, Tuning.TuningOutputTypeEnum.OEE);
                throw;
            }
        }
        #endregion Constructors
        #region Methods
        #region Compounding
        private void Compounding()
        {
            #region observedRate after compounding
            EFS_CompoundingRate selfCompounding = new EFS_CompoundingRate(m_SelfCompoundingMethod,
                m_StartDate, EndDate, m_SelfAveragingEvents);
            observedRate = selfCompounding.calculatedValue;
            #endregion observedRate after compounding
        }
        #endregion Compounding
        #region Interpolating
        private void Interpolating()
        {
            CommonValParameterIRD parameter = (CommonValParameterIRD)m_CommonValProcess.ParametersIRD[m_CommonValProcess.ParamInstrumentNo,m_CommonValProcess.ParamStreamNo];

            IInterval interval = m_PaymentFrequency.GetInterval(parameter.Rate.Asset_PeriodMltp_Tenor, parameter.Rate.FpML_Enum_Period_Tenor);
            EFS_Interval efs_Interval = new EFS_Interval(interval, m_ObservedRateDate, Convert.ToDateTime(null));

            decimal observedRate1 = this.ReadQuoteAssetRateIndex(ReadQuoteAssetEnum.Asset1, out KeyQuote keyQuote, out EFS_CalcForwardRateIndex calcForwardRate);
            UpdatingRowAsset(m_RowAssets[0], keyQuote, calcForwardRate, observedRate1);

            IInterval interval2 = m_PaymentFrequency.GetInterval(parameter.Rate2.Asset_PeriodMltp_Tenor, parameter.Rate2.FpML_Enum_Period_Tenor);
            EFS_Interval efs_Interval2 = new EFS_Interval(interval2, m_ObservedRateDate, Convert.ToDateTime(null));
            decimal observedRate2 = this.ReadQuoteAssetRateIndex(ReadQuoteAssetEnum.Asset2, out keyQuote, out calcForwardRate );
            UpdatingRowAsset(m_RowAssets[1], keyQuote, calcForwardRate, observedRate2);

            EFS_Interpolating interpolating = new EFS_Interpolating(m_CommonValDate, observedRate1, efs_Interval.offsetDate,
                observedRate2, efs_Interval2.offsetDate, m_EndPeriodDate, RoundingDirectionEnum.Nearest, m_RoundingPrecision);
            observedRate = interpolating.interpolatedRate;
        }
        #endregion Interpolating
        #region RateRounding
        private void RateRounding()
        {
            EFS_Round round = new EFS_Round(m_RateRounding, observedRate);
            observedRate = round.AmountRounded;
        }
        #endregion RateRounding
        #region RateTreatement
        private void RateTreatement()
        {
            treatedRate = observedRate;
            if (m_RateTreatmentSpecified)
                treatedRate = CommonValFunc.TreatedRate(m_CommonValProcess.ProductBase , m_RateTreatment, observedRate, m_StartDate, m_EndDate, m_PaymentFrequency);
        }
        #endregion RateTreatement
        #region UpdatingRow
        private void UpdatingRow(DataRow pRow)
        {
            pRow["VALORISATION"] = treatedRate;
            pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
            pRow["VALORISATIONSYS"] = treatedRate;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();
            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowDetail)
                rowDetail["RATE"] = observedRate;
        }
        #endregion UpdatingRow
        #region UpdatingRowAsset
        private void UpdatingRowAsset(DataRow pRow, KeyQuote pKeyQuote, EFS_CalcForwardRateIndex pCalcForwardRate, decimal pRate)
        {
            pRow["QUOTESIDE"] = pKeyQuote.QuoteSide;
            pRow["IDMARKETENV"] = pKeyQuote.IdMarketEnv;
            pRow["IDVALSCENARIO"] = pKeyQuote.IdValScenario;

            if (m_CommonValProcess.IsMarkToMarket)
            {
                if (null != pCalcForwardRate)
                {
                    pRow["IDYIELDCURVEVAL_H"] = pCalcForwardRate.YieldCurveVal.idYieldCurveVal_H;
                    pRow["ZEROCOUPON1"] = pCalcForwardRate.ZeroCouponStart;
                    pRow["ZEROCOUPON2"] = pCalcForwardRate.ZeroCouponEnd;
                    pRow["FORWARDRATE"] = pCalcForwardRate.ForwardRateInitial;
                    pRow["RATE"] = pCalcForwardRate.ForwardRate; // équivalent à pRate
                }
                else
                {
                    pRow["RATE"] = pRate;
                }
            }
        }
        #endregion UpdatingRowAsset
        #endregion Methods
    }
    #endregion EFS_ResetEvent
    #region EFS_SelfAveragingEvent
    public class EFS_SelfAveragingEvent : SelfAveragingInfo
    {
        #region Members
        protected EFS_SelfResetEvent[] m_SelfResetEvents;
        //        
        public decimal averagedRate;
        public decimal averagedAndTreatedRate;

        public EFS_SelfResetEvent[] SelfResetEvents
        {
            get { return m_SelfResetEvents; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor où averagedAndTreatedRate est obtenu par lecture de la table EVENT
        /// </summary>
        /// <param name="pAccrualDate"></param>
        /// <param name="pEventsValProcess"></param>
        /// <param name="pRowSelfAverage"></param>
        public EFS_SelfAveragingEvent(DateTime pAccrualDate, CommonValProcessBase pEventsValProcess, DataRow pRowSelfAverage)
            : base(pAccrualDate, pEventsValProcess, pRowSelfAverage)
        {
            if (false == Convert.IsDBNull(pRowSelfAverage["VALORISATION"]))
                averagedAndTreatedRate = Convert.ToDecimal(pRowSelfAverage["VALORISATION"]);
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_SelfAveragingEvent(DateTime pAccrualDate, CommonValProcessBase pEventsValProcess, DataRow pRowSelfAverage, DateTime pRateCutOffDate)
            : base(pAccrualDate, pEventsValProcess, pRowSelfAverage, pRateCutOffDate)
        {
            try
            {
                #region SelfReset Process
                DataRow[] rowSelfResets = pRowSelfAverage.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                EFS_SelfResetEvent selfResetEvent;
                ArrayList aSelfResetEvent = new ArrayList();
                foreach (DataRow rowSelfReset in rowSelfResets)
                {
                    if (m_CommonValProcess.IsRowMustBeCalculate(rowSelfReset))
                    {
                        CommonValFunc.SetRowCalculated(rowSelfReset);
                        selfResetEvent = new EFS_SelfResetEvent(m_CommonValProcess, rowSelfReset, m_IdAsset, m_RateCutOffDate);
                        aSelfResetEvent.Add(selfResetEvent);
                        if (false == CommonValFunc.IsRowEventCalculated(rowSelfReset))
                            break;
                    }
                    else
                    {
                        aSelfResetEvent.Add(new EFS_SelfResetEvent(m_CommonValProcess, rowSelfReset, pRateCutOffDate));
                    }
                }
                m_SelfResetEvents = (EFS_SelfResetEvent[])aSelfResetEvent.ToArray(typeof(EFS_SelfResetEvent));
                #endregion SelfReset Process

                #region Final SelfAveraging Process
                if (m_CommonValProcess.IsRowsEventCalculated(rowSelfResets))
                {
                    Averaging();
                    Treating();
                    UpdatingRow(pRowSelfAverage);
                }
                CommonValFunc.SetRowCalculated(pRowSelfAverage);
                #endregion Final SelfAveraging Process
                
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowSelfAverage);
                m_CommonValProcess.SetRowStatus(pRowSelfAverage, Tuning.TuningOutputTypeEnum.OEE);
                throw;
            }
        }
        #endregion Constructors
        #region Methods
        #region Averaging
        private void Averaging()
        {
            EFS_Averaging averaging = new EFS_Averaging(this, m_SelfResetEvents);
            averagedRate = averaging.averagedRate;
            RateRounding();
        }
        #endregion Averaging
        #region RateRounding
        private void RateRounding()
        {
            EFS_Round round = new EFS_Round(m_RateRounding, averagedRate);
            averagedRate = round.AmountRounded;
        }
        #endregion RateRounding
        #region Treating
        private void Treating()
        {
            averagedAndTreatedRate = averagedRate;
            if (m_RateTreatmentSpecified)
                averagedAndTreatedRate = CommonValFunc.TreatedRate(m_CommonValProcess.ProductBase, m_RateTreatment, averagedRate, m_StartDate, EndDate, paymentFrequency);
        }
        #endregion Treating
        #region UpdatingRow
        private void UpdatingRow(DataRow pRow)
        {
            pRow["VALORISATION"] = averagedAndTreatedRate;
            pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
            pRow["VALORISATIONSYS"] = averagedAndTreatedRate;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();

            DataRow[] rowAssets = m_CommonValProcess.GetRowAsset(Convert.ToInt32(pRow["IDE"]));
            if ((null != rowAssets) && (0 < rowAssets.Length))
                rowAssets[0]["IDASSET"] = m_IdAsset;

            m_CommonValProcess.SetRowStatus(pRow, Tuning.TuningOutputTypeEnum.OES);
        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_SelfAveragingEvent
    #region EFS_SelfResetEvent
    public class EFS_SelfResetEvent : SelfResetInfo
    {
        #region Members
        public decimal observedRate;
        #endregion members
        //
        #region Constructors
        /// <summary>
        /// Constructor où observedRate est obtenu par lecture de la table
        /// </summary>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowSelfReset"></param>
        /// <param name="pRateCutOffDate"></param>
        public EFS_SelfResetEvent(CommonValProcessBase pCommonValProcess, DataRow pRowSelfReset, DateTime pRateCutOffDate)
            : base(pCommonValProcess, pRowSelfReset, pRateCutOffDate)
        {
            if (false == Convert.IsDBNull(pRowSelfReset["VALORISATION"]))
                observedRate = Convert.ToDecimal(pRowSelfReset["VALORISATION"]);
        }
        /// <summary>
        /// Constructor où observedRate est obtenu après lecture d'une quotation
        /// </summary>
        /// <param name="pCommonValProcess"></param>
        /// <param name="pRowSelfReset"></param>
        /// <param name="pIdAsset"></param>
        /// <param name="pRateCutOffDate"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_SelfResetEvent(CommonValProcessBase pCommonValProcess, DataRow pRowSelfReset, int pIdAsset, DateTime pRateCutOffDate)
            : base(pCommonValProcess, pRowSelfReset, pIdAsset, pRateCutOffDate)
        {
            try
            {
                observedRate = ReadQuoteAssetRateIndex(out KeyQuote keyQuote, out EFS_CalcForwardRateIndex calcForwardRate);
                UpdatingRow(pRowSelfReset, keyQuote, calcForwardRate);
            }
            catch (Exception)
            {
                CommonValProcessBase.ResetRowCalculated(pRowSelfReset);
                m_CommonValProcess.SetRowStatus(pRowSelfReset, Tuning.TuningOutputTypeEnum.OEE);
                throw;
            }
        }
        #endregion Constructors
        #region methods
        #region private UpdatingRow
        private void UpdatingRow(DataRow pRow, KeyQuote pKeyQuote, EFS_CalcForwardRateIndex pCalcForwardRate)
        {
            pRow["VALORISATION"] = observedRate;
            pRow["UNITTYPE"] = UnitTypeEnum.Rate.ToString();
            pRow["VALORISATIONSYS"] = observedRate;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Rate.ToString();

            CommonValFunc.SetRowCalculated(pRow);
            m_CommonValProcess.SetRowStatus(pRow, Tuning.TuningOutputTypeEnum.OES);

            if ((null != m_RowAssets) || (1 == m_RowAssets.Length))
            {
                m_RowAssets[0]["IDMARKETENV"] = pKeyQuote.IdMarketEnv;
                m_RowAssets[0]["IDVALSCENARIO"] = pKeyQuote.IdValScenario;
                m_RowAssets[0]["QUOTESIDE"] = pKeyQuote.QuoteSide;

                if (m_CommonValProcess.IsMarkToMarket && (null != pCalcForwardRate))
                {
                    m_RowAssets[0]["IDYIELDCURVEVAL_H"] = pCalcForwardRate.YieldCurveVal.idYieldCurveVal_H;
                    m_RowAssets[0]["ZEROCOUPON1"] = pCalcForwardRate.ZeroCouponStart;
                    m_RowAssets[0]["ZEROCOUPON2"] = pCalcForwardRate.ZeroCouponEnd;
                    m_RowAssets[0]["FORWARDRATE"] = pCalcForwardRate.ForwardRateInitial;
                    m_RowAssets[0]["RATE"] = pCalcForwardRate.ForwardRate; // équivalent à observedRate
                }
            }
        }
        #endregion UpdatingRow
        #endregion
    }
    #endregion EFS_SelfResetEvent
    #region EFS_SettlementEvent
    public class EFS_SettlementEvent : SettlementInfo
    {
        #region Variables
        protected EFS_FXResetEvent[] m_FXResetEvents;
        #endregion Variables
        #region Result Variables
        public decimal settlementDifferentialAmount;
        #endregion Result Variables
        #region Accessors
        #region Parameter
        /*
		public CommonValParameterBase Parameter
		{
			get
			{
				return (CommonValParameterBase) m_CommonValProcess.Parameters[
					m_CommonValProcess.paramInstrumentNo,m_CommonValProcess.paramStreamNo];
			}
		}
		*/
        #endregion Parameter
        #endregion Accessors
        #region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_SettlementEvent(CommonValProcessBase pCommonValProcess, DataRow pRowSettlement)
            : base(pCommonValProcess, pRowSettlement)
        {
            try
            {
                #region Reset Process
                DataRow rowFixingCustomer = null;
                DataRow[] rowResets = pRowSettlement.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent);
                bool isExistFixingCustomer = IsExistFixingCustomer(rowResets);
                if (0 < rowResets.Length)
                {
                    EFS_FXResetEvent fxResetEvent = null;
                    ArrayList aFxResetEvent = new ArrayList();
                    foreach (DataRow rowReset in rowResets)
                    {
                        if (EventCodeFunc.IsReset(rowReset["EVENTCODE"].ToString()))
                        {
                            if (EventTypeFunc.IsFxCustomer(rowReset["EVENTTYPE"].ToString()))
                                rowFixingCustomer = rowReset;
                            else if (m_CommonValProcess.IsRowMustBeCalculate(rowReset))
                            {
                                CommonValFunc.SetRowCalculated(rowReset);
                                try { fxResetEvent = new EFS_FXResetEvent(m_CommonValProcess, rowReset); }
                                catch (SpheresException2)
                                {
                                    // Ticket : 13351
                                    // Le prix RES/FXR n'existe pas mais il existe un RES/FXC donc pas de génération d'erreur
                                    if (isExistFixingCustomer)
                                        rowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                                    else
                                        throw;

                                }
                                catch (Exception) { throw; }
                                aFxResetEvent.Add(fxResetEvent);
                                if (false == CommonValFunc.IsRowEventCalculated(rowReset))
                                    break;
                            }
                            else
                                aFxResetEvent.Add(new EFS_FXResetEvent(m_CommonValProcess, rowReset));
                        }
                    }
                    m_FXResetEvents = (EFS_FXResetEvent[])aFxResetEvent.ToArray(typeof(EFS_FXResetEvent));
                }
                #endregion Reset Process
                #region Final Settlement Process
                if (m_CommonValProcess.IsRowsEventCalculated(rowResets))
                {
                    ISDADifferential(rowFixingCustomer);
                    UpdatingRow(pRowSettlement);
                }
                #endregion Final Settlement Process
            }
            catch (SpheresException2)
            {
                CommonValProcessBase.ResetRowCalculated(pRowSettlement);
                m_CommonValProcess.SetRowStatus(pRowSettlement, TuningOutputTypeEnum.OEE);
                throw;
            }
            catch (Exception) { throw; }
        }
        #endregion Constructors
        #region Methods
        #region IsExistFixingCustomer
        private static bool IsExistFixingCustomer(DataRow[] pRow)
        {
            foreach (DataRow row in pRow)
            {
                if (EventTypeFunc.IsFxCustomer(row["EVENTTYPE"].ToString()))
                    return true;
            }
            return false;
        }
        #endregion IsExistFixingCustomer
        #region ISDADifferential
        private void ISDADifferential(DataRow pRowFixingCustomer)
        {
            if (null != pRowFixingCustomer)
            {
                if (false == Convert.IsDBNull(pRowFixingCustomer["VALORISATION"]))
                {
                    m_SettlementRate = Convert.ToDecimal(pRowFixingCustomer["VALORISATION"]);
                    m_ConversionRate = 1;
                }
            }
            else
            {
                if (Tools.IsRegularNDF(m_FxType) || (1 == m_FXResetEvents.Length))
                    m_SettlementRate = m_FXResetEvents[0].quotedRate;
                else if (Tools.IsNDF_ThirdSettlementCurrency(m_FxType))
                    m_SettlementRate = m_FXResetEvents[1].quotedRate;
                else if (Tools.IsNDF_Bridge(m_FxType))
                {
                    if ((m_FXResetEvents[0].Currency1 == m_ReferenceCurrency) || (m_FXResetEvents[0].Currency2 == m_ReferenceCurrency))
                        m_SettlementRate = m_FXResetEvents[0].quotedRate / m_FXResetEvents[1].quotedRate;
                    else if ((m_FXResetEvents[1].Currency1 == m_ReferenceCurrency) || (m_FXResetEvents[1].Currency2 == m_ReferenceCurrency))
                        m_SettlementRate = m_FXResetEvents[1].quotedRate / m_FXResetEvents[0].quotedRate;
                }
                else if (Tools.IsNDF_Bridge_ThirdSettlementCurrency(m_FxType))
                {
                    if ((m_FXResetEvents[1].Currency1 == m_ReferenceCurrency) || (m_FXResetEvents[1].Currency2 == m_ReferenceCurrency))
                        m_SettlementRate = m_FXResetEvents[1].quotedRate / m_FXResetEvents[2].quotedRate;
                    else if ((m_FXResetEvents[2].Currency1 == m_ReferenceCurrency) || (m_FXResetEvents[2].Currency2 == m_ReferenceCurrency))
                        m_SettlementRate = m_FXResetEvents[2].quotedRate / m_FXResetEvents[1].quotedRate;
                }

                #region ConversionRate Evaluation
                if (Tools.IsNDF_ThirdSettlementCurrency(m_FxType) || Tools.IsNDF_Bridge_ThirdSettlementCurrency(m_FxType))
                {
                    EFS_FXResetEvent reset = m_FXResetEvents[0];
                    if (((QuoteBasisEnum.Currency2PerCurrency1 == reset.QuoteBasis) && (reset.Currency2 == m_ReferenceCurrency)) ||
                        ((QuoteBasisEnum.Currency1PerCurrency2 == reset.QuoteBasis) && (reset.Currency1 == m_ReferenceCurrency)))
                        m_ConversionRate = reset.quotedRate;
                    else
                        m_ConversionRate = 1 / reset.quotedRate;
                }
                #endregion ConversionRate Evaluation
            }
            decimal differentialAmount = CommonValFunc.CalcDifferentialAmount(m_NotionalAmount, m_ForwardRate, m_SettlementRate);
            settlementDifferentialAmount = Math.Abs(differentialAmount * m_ConversionRate);
            settlementDifferentialAmount = m_CommonValProcess.RoundingCurrencyAmount(m_Currency, settlementDifferentialAmount);
        }
        #endregion ISDADifferential
        #region UpdatingRow
        private void UpdatingRow(DataRow pRow)
        {
            pRow["VALORISATION"] = settlementDifferentialAmount;
            pRow["VALORISATIONSYS"] = settlementDifferentialAmount;

            //Recherche de la ligne STA en ReferenceCurrency
            DataRow rowNDF = pRow.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
            foreach (DataRow dr in rowNDF.GetChildRows(m_CommonValProcess.DsEvents.ChildEvent))
            {
                if (EventCodeFunc.IsStart(dr["EVENTCODE"].ToString()) && (m_ReferenceCurrency == dr["UNIT"].ToString()))
                {
                    //Synchronisation des acteurs/books avec la ligne STA en ReferenceCurrency
                    pRow["IDA_PAY"] = dr["IDA_PAY"];
                    pRow["IDB_PAY"] = dr["IDB_PAY"];
                    pRow["IDA_REC"] = dr["IDA_REC"];
                    pRow["IDB_REC"] = dr["IDB_REC"];
                    break;
                }
            }
            if (m_SettlementRate > m_ForwardRate)
                CommonValFunc.SwapPayerAndReceiver(pRow);

            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowDetail)
            {
                rowDetail["SETTLEMENTRATE"] = m_SettlementRate;
                rowDetail["CONVERSIONRATE"] = m_ConversionRate;
            }
            CommonValFunc.SetRowCalculated(pRow);
            m_CommonValProcess.SetRowStatus(pRow, TuningOutputTypeEnum.OES);
        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_SettlementEvent
    #region EFS_VariationNominalEvent
    public class EFS_VariationNominalEvent : VariationNominalInfo
    {
        #region Variables
        protected DataRow m_RowNominal;
        protected DataRow m_RowNominalReference;
        protected DataRow m_RowPreviousNominal;
        protected DataRow[] m_RowTerminationVariationNominal;
        protected bool m_IsCurrentNominalIsOnEndPeriod;
        #endregion Variables
        #region Result Variables
        public decimal observedRate;
        public decimal variationNominal;
        public decimal nominal;
        public decimal previousNominal;
        public decimal nominalReference;
        public decimal calculatedAmount;
        #endregion Result Variables
        #region Constructors
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public EFS_VariationNominalEvent(DateTime pAccrualsDate, CommonValProcessBase pCommonValProcess, DataRow pRowVariation)
            : base(pAccrualsDate, pCommonValProcess, pRowVariation)
        {
            SystemMSGInfo systemMsgInfo = null;
            try
            {
                #region Nominal & NominalReference
                m_RowStream = pRowVariation.GetParentRow(m_CommonValProcess.DsEvents.ChildEvent);
                m_RowNominal = m_CommonValProcess.GetCurrentNominal(m_StartDate);
                m_RowPreviousNominal = m_CommonValProcess.GetPreviousNominal(m_RowNominal);
                m_RowNominalReference = m_CommonValProcess.GetRowCurrentNominalReference(m_StartDate);

                DateTime dtEndPeriodNominal = Convert.ToDateTime(m_RowNominal["DTENDADJ"]);
                DateTime dtEndPeriodStream = Convert.ToDateTime(m_RowStream["DTENDADJ"]);
                m_RowTerminationVariationNominal = m_CommonValProcess.GetRowTerminationVariationNominal();
                m_IsCurrentNominalIsOnEndPeriod = (dtEndPeriodNominal == dtEndPeriodStream) && (null != m_RowTerminationVariationNominal);


                nominal = Convert.IsDBNull(m_RowNominal["VALORISATION"]) ? 0 : Convert.ToDecimal(m_RowNominal["VALORISATION"]);
                nominalReference = Convert.ToDecimal(m_RowNominalReference["VALORISATION"]);

                if (m_RowPreviousNominal.Equals(m_RowNominal))
                    previousNominal = nominal;
                else if (false == Convert.IsDBNull(m_RowPreviousNominal["VALORISATION"]))
                    previousNominal = Convert.ToDecimal(m_RowPreviousNominal["VALORISATION"]);
                else
                {
                    pRowVariation["IDA_PAY"] = Convert.ToInt32(m_RowStream["IDA_PAY"]);
                    pRowVariation["IDA_REC"] = Convert.ToInt32(m_RowStream["IDA_REC"]);
                    ProcessStateTools.StatusEnum status = (m_Time <= OTCmlHelper.GetDateBusiness(m_CommonValProcess.Process.Cs)) ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusWarningEnum;
                    ProcessState processState = new ProcessState(status, ProcessStateTools.CodeReturnDataNotFoundEnum);
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 
                        "Previous Nominal uncalculated [{0}] Currency[{1}] CurrencyReference [{2}]",
                        processState,
                        m_Time.ToShortDateString(), m_Currency, m_CurrencyReference);
                }
                #endregion Nominal & NominalReference

                #region Process

                #region observed Fixing value from the fixing prices referential
                #region KeyQuote
                // EG 20160404 Migration vs2013
                //#warning PAYER/RECEIVER change (ASK/BID confusion)
                //20090609 PL Add QuoteTimingEnum.Close
                KeyQuote keyQuote = new KeyQuote(pCommonValProcess.Process.Cs, m_Time,
                                                 m_PayerReceiver.Payer, m_PayerReceiver.BookPayer,
                                                 m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver, QuoteTimingEnum.Close);
                #endregion KeyQuote
                //observedRate = CommonValFunc.ReadQuote_FXRate(pCommonValProcess.Process.Cs,
                //    pCommonValProcess.productBase, keyQuote, m_IdAsset);
                SQL_Quote quote = CommonValFunc.ReadQuote_AssetByType(pCommonValProcess.Process.Cs, QuoteEnum.FXRATE,
                    pCommonValProcess.ProductBase, keyQuote, m_IdAsset, ref systemMsgInfo);
                observedRate = quote.QuoteValue;

                if (((QuoteBasisEnum.Currency2PerCurrency1 == m_Basis) && (m_Currency2 == m_CurrencyReference)) ||
                    ((QuoteBasisEnum.Currency1PerCurrency2 == m_Basis) && (m_Currency1 == m_CurrencyReference)))
                {
                    if (0 != observedRate)
                        calculatedAmount = nominalReference * (1 / observedRate);
                }
                else
                    calculatedAmount = nominalReference * observedRate;
                #endregion observed Fixing value from the fixing prices referential

                #region Nominal Calculation
                if (m_RowPreviousNominal.Equals(m_RowNominal))
                {
                    #region First Nominal
                    UpdatingRow(pRowVariation, calculatedAmount, observedRate,
                                m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver,
                                m_PayerReceiver.Payer, m_PayerReceiver.BookPayer, nominalReference, keyQuote);
                    #endregion First Nominal
                }
                else
                {
                    #region Others Nominal
                    decimal diffNominal = calculatedAmount - previousNominal;
                    if (1 == Math.Sign(diffNominal))
                    {
                        #region Increase
                        UpdatingRow(pRowVariation, diffNominal, 0,
                            m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver,
                            m_PayerReceiver.Payer, m_PayerReceiver.BookPayer, nominalReference, keyQuote);
                        #endregion Increase
                    }
                    else
                    {
                        #region Amortization
                        UpdatingRow(pRowVariation, Math.Abs(diffNominal), 0,
                            m_PayerReceiver.Payer, m_PayerReceiver.BookPayer,
                            m_PayerReceiver.Receiver, m_PayerReceiver.BookReceiver, nominalReference, keyQuote);
                        #endregion Amortization
                    }
                    #region Update TER NOM Variation
                    if (m_IsCurrentNominalIsOnEndPeriod)
                    {
                        m_RowTerminationVariationNominal[0]["VALORISATION"] = calculatedAmount;
                        m_RowTerminationVariationNominal[0]["UNIT"] = m_Currency;
                        m_RowTerminationVariationNominal[0]["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
                        CommonValFunc.SetRowCalculated(m_RowTerminationVariationNominal[0]);
                        m_CommonValProcess.SetRowStatus(m_RowTerminationVariationNominal[0], TuningOutputTypeEnum.OES);
                    }
                    #endregion Update TER NOM Variation
                    #endregion Others Nominal
                }
                #endregion Nominal Calculation
                #endregion Process
            }
            catch (SpheresException2 ex)
            {
                CommonValProcessBase.ResetRowCalculated(pRowVariation);
                CommonValProcessBase.ResetRowCalculated(m_RowNominal);
                //
                m_CommonValProcess.SetRowStatus(pRowVariation, Tuning.TuningOutputTypeEnum.OEE);
                m_CommonValProcess.SetRowStatus(m_RowNominal, Tuning.TuningOutputTypeEnum.OEE);

                if (m_IsCurrentNominalIsOnEndPeriod)
                {
                    CommonValProcessBase.ResetRowCalculated(m_RowTerminationVariationNominal[0]);
                    m_CommonValProcess.SetRowStatus(m_RowTerminationVariationNominal[0], TuningOutputTypeEnum.OES);
                }

                if (false == ProcessStateTools.IsCodeReturnUndefined(ex.ProcessState.CodeReturn))
                {
                    // FI 20200623 [XXXXX] SetErrorWarning
                    m_CommonValProcess.Process.ProcessState.SetErrorWarning(systemMsgInfo.processState.Status);
                    
                    
                    Logger.Log(systemMsgInfo.ToLoggerData(0));

                    throw new SpheresException2(systemMsgInfo.processState);
                }
                else
                    throw;
            }
        }
        #endregion Constructors
        #region Methods
        #region UpdatingRow
        // EG 20150706 [21021] Nullable<int> for pPayer_Book|pReceiver_Book
        private void UpdatingRow(DataRow pRow, decimal pVariationNominal, decimal pObservedRate,
            int pPayer, Nullable<int> pPayer_Book, int pReceiver, Nullable<int> pReceiver_Book, decimal pNominalReference, KeyQuote pKeyQuote)
        {
            pRow["UNIT"] = m_Currency;
            pRow["UNITTYPE"] = UnitTypeEnum.Currency.ToString();
            pRow["VALORISATION"] = pVariationNominal;
            pRow["UNITSYS"] = m_Currency;
            pRow["UNITTYPESYS"] = UnitTypeEnum.Currency.ToString();
            pRow["VALORISATIONSYS"] = pVariationNominal;
            pRow["IDA_PAY"] = pPayer;
            // EG 20150706 [21021]
            pRow["IDB_PAY"] = pPayer_Book ?? Convert.DBNull;
            pRow["IDA_REC"] = pReceiver;
            // EG 20150706 [21021]
            pRow["IDB_REC"] = pReceiver_Book ?? Convert.DBNull;

            m_RowNominal["VALORISATION"] = calculatedAmount;
            m_RowNominal["UNIT"] = m_Currency;
            m_RowNominal["UNITTYPE"] = UnitTypeEnum.Currency.ToString();

            DataRow rowDetail = m_CommonValProcess.GetRowDetail(Convert.ToInt32(pRow["IDE"]));
            if (null != rowDetail)
            {
                rowDetail["RATE"] = pObservedRate;
                rowDetail["NOTIONALAMOUNT"] = pNominalReference;
            }

            CommonValFunc.SetRowCalculated(pRow);
            CommonValFunc.SetRowCalculated(m_RowNominal);

            m_CommonValProcess.SetRowStatus(pRow, TuningOutputTypeEnum.OES);
            m_CommonValProcess.SetRowStatus(m_RowNominal, TuningOutputTypeEnum.OES);

            if ((null != m_RowAssets) || (1 == m_RowAssets.Length))
            {
                m_RowAssets[0]["IDMARKETENV"] = pKeyQuote.IdMarketEnv;
                m_RowAssets[0]["IDVALSCENARIO"] = pKeyQuote.IdValScenario;
                m_RowAssets[0]["QUOTESIDE"] = pKeyQuote.QuoteSide;
            }
        }
        #endregion UpdatingRow
        #endregion Methods
    }
    #endregion EFS_VariationNominalEvent


    #region EFS_AFMADiscounting
    public class EFS_AFMADiscounting : EFS_Discounting
    {
        #region Members
        protected new decimal m_SourceValue;
        protected decimal m_FloatingRateValue;
        protected string m_FloatingDayCountFraction;
        protected decimal m_FixedRateValue;
        protected string m_FixedDayCountFraction;
        protected decimal m_DiscountedValue1;
        protected decimal m_DiscountedValue2;
        #endregion Members
        #region Constructors
        public EFS_AFMADiscounting(IProduct pProduct, decimal pSourceValue, DateTime pStartDate, DateTime pEndDate, decimal pDiscountRate,
            string pDiscountDayCountFraction, decimal pFloatingRateValue, string pFloatingDayCountFraction,
            decimal pFixedRateValue, string pFixedDayCountFraction)
            : base(pProduct, FraDiscountingEnum.ISDA.ToString(), pStartDate, pEndDate, pDiscountRate, pDiscountDayCountFraction)
        {
            m_SourceValue = pSourceValue;
            m_FloatingRateValue = pFloatingRateValue;
            m_FloatingDayCountFraction = pFloatingDayCountFraction;
            m_FixedRateValue = pFixedRateValue;
            m_FixedDayCountFraction = pFixedDayCountFraction;
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public Cst.ErrLevel Calc()
        {
            EFS_DayCountFraction floatingDCF = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_FloatingDayCountFraction, m_Interval);
            EFS_DayCountFraction fixedDCF = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_FixedDayCountFraction, m_Interval);
            EFS_StandardDiscounting standardDiscounting;

            #region FloatingRate
            base.m_SourceValue = m_SourceValue * m_FloatingRateValue * floatingDCF.Factor;
            standardDiscounting = new EFS_StandardDiscounting(m_Product, base.m_SourceValue, false, m_StartDate, m_EndDate,
                m_DiscountRate, m_DiscountDayCountFraction);
            m_DiscountedValue1 = standardDiscounting.discountedValue;
            #endregion FloatingRate
            #region FixedRate
            base.m_SourceValue = m_SourceValue * m_FixedRateValue * fixedDCF.Factor;
            standardDiscounting = new EFS_StandardDiscounting(m_Product, base.m_SourceValue, false, m_StartDate, m_EndDate,
                m_DiscountRate, m_DiscountDayCountFraction);
            m_DiscountedValue2 = standardDiscounting.discountedValue;
            #endregion FixedRate

            #region Final discountedValue
            discountedValue = m_DiscountedValue1 - m_DiscountedValue2;
            #endregion Final discountedValue
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_AFMADiscounting
    #region EFS_Averaging
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new member m_IntervalFrequency
    ///     </comment>
    /// </revision>
    public class EFS_Averaging
    {
        #region Members
        private readonly DateTime m_EndDate;
        private readonly Type m_tObservedArgs;
        private readonly object[] m_ObservedArgs;
        private readonly AveragingMethodEnum m_AveragingMethod;
        private readonly string m_DayCountFraction;
        private readonly IInterval m_IntervalFrequency;

        public Decimal averagedRate;
        #endregion Members
        //
        #region Accessors
        #endregion Accessors
        //
        #region Constructors
        public EFS_Averaging(SelfAveragingInfo pAveragingInfo, object[] pObservedArgs)
            : this(pAveragingInfo.averagingMethod, pAveragingInfo.EndDate,
            pObservedArgs, pAveragingInfo.dayCountFraction, pAveragingInfo.intervalFrequency) { }

        public EFS_Averaging(CalculationPeriodInfo pCalcPeriodInfo, object[] pObservedArgs)
            : this(pCalcPeriodInfo.averagingMethod, pCalcPeriodInfo.EndDate,
            pObservedArgs, pCalcPeriodInfo.dayCountFraction, pCalcPeriodInfo.intervalFrequency) { }

        public EFS_Averaging(AveragingMethodEnum pAveragingMethod, DateTime pEndDate, object[] pObservedArgs, string pDayCountFraction, IInterval pIntervalFrequency)
        {
            m_AveragingMethod = pAveragingMethod;
            m_EndDate = pEndDate;
            m_ObservedArgs = pObservedArgs;
            m_tObservedArgs = m_ObservedArgs.GetType().GetElementType();

            m_DayCountFraction = pDayCountFraction;
            // 20071107 Eg Ticket 15859
            m_IntervalFrequency = pIntervalFrequency;
            Calc();
        }

        public EFS_Averaging(AveragingMethodEnum pAveragingMethod, object[] pObservedArgs)
        {
            m_AveragingMethod = pAveragingMethod;
            //observedArgs      = pObservedArgs;
            m_ObservedArgs = pObservedArgs;
            m_tObservedArgs = m_ObservedArgs.GetType().GetElementType();
            Calc();
        }

        #endregion Constructors
        #region Methods
        #region ArgValue
        private decimal ArgValue(int pIndex)
        {
            if (m_tObservedArgs.Equals(typeof(EFS_ResetEvent)))
                return ((EFS_ResetEvent)m_ObservedArgs[pIndex]).treatedRate;
            else if (m_tObservedArgs.Equals(typeof(EFS_SelfResetEvent)))
                return ((EFS_SelfResetEvent)m_ObservedArgs[pIndex]).observedRate;
            else if (m_tObservedArgs.Equals(typeof(EFS_ObservedValue)))
                return ((EFS_ObservedValue)m_ObservedArgs[pIndex]).Value;
            else if (m_tObservedArgs.Equals(typeof(EFS_FXFixingEvent)))
                return ((EFS_FXFixingEvent)m_ObservedArgs[pIndex]).observedRate;
            return 0;
        }
        #endregion ArgValue
        #region ArgDate
        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        private DateTime ArgDate(int pIndex)
        {
            if (m_tObservedArgs.Equals(typeof(EFS_ResetEvent)))
                return ((EFS_ResetEvent)m_ObservedArgs[pIndex]).resetDate;
            else if (m_tObservedArgs.Equals(typeof(EFS_SelfResetEvent)))
                //return ((EFS_SelfResetEvent)m_ObservedArgs[pIndex]).fixingDate;
                return ((EFS_SelfResetEvent)m_ObservedArgs[pIndex]).SelfResetDate;
            else if (m_tObservedArgs.Equals(typeof(EFS_ObservedValue)))
                return ((EFS_ObservedValue)m_ObservedArgs[pIndex]).date;
            return Convert.ToDateTime(null);
        }
        #endregion ArgDate
        #region Calc
        private Cst.ErrLevel Calc()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            switch (m_AveragingMethod)
            {
                case AveragingMethodEnum.Unweighted:
                    ret = UnWeightedMethod();
                    break;
                case AveragingMethodEnum.Weighted:
                    ret = WeightedMethod();
                    break;
            }
            return ret;
        }
        #endregion Calc
        #region GetObservedArgs
        public object[] GetObservedArgs()
        {
            return m_ObservedArgs;
        }
        #endregion GetObservedArgs

        #region WeightedMethod
        private Cst.ErrLevel WeightedMethod()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            if (0 < m_ObservedArgs.Length)
            {
                EFS_DayCountFraction dcf;
                int totalNbDays = 0;
                decimal weightedRate = 0;
                for (int i = 1; i < m_ObservedArgs.Length - 1; i++)
                {
                    dcf = new EFS_DayCountFraction(ArgDate(i - 1), ArgDate(i), m_DayCountFraction, m_IntervalFrequency);
                    totalNbDays += dcf.TotalNumberOfCalculatedDays;
                    weightedRate += (dcf.TotalNumberOfCalculatedDays * ArgValue(i - 1));
                }
                #region LastSource
                dcf = new EFS_DayCountFraction(ArgDate(m_ObservedArgs.Length - 1), m_EndDate, m_DayCountFraction, m_IntervalFrequency);
                totalNbDays += dcf.TotalNumberOfCalculatedDays;
                weightedRate += (dcf.TotalNumberOfCalendarDays * ArgValue(m_ObservedArgs.Length - 1));
                #endregion LastSource
                if (0 != totalNbDays)
                    averagedRate = weightedRate / totalNbDays;
                ret = Cst.ErrLevel.SUCCESS;
            }
            return ret;
        }

        #endregion WeightedMethod
        #region UnWeightedMethod
        private Cst.ErrLevel UnWeightedMethod()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            if (0 < m_ObservedArgs.Length)
            {
                decimal unWeightedRate = 0;
                for (int i = 0; i < m_ObservedArgs.Length; i++)
                    unWeightedRate += ArgValue(i);
                averagedRate = unWeightedRate / m_ObservedArgs.Length;
                ret = Cst.ErrLevel.SUCCESS;
            }
            return ret;
        }
        #endregion UnWeightedMethod
        #endregion Methods
    }
    #endregion EFS_Averaging
    #region EFS_CapFlooring
    /// <revision>
    ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
    ///     <comment>Ticket 15889
    ///     Step dates: Unajusted versus Ajusted
    ///     Add public  member : BusinessDayAdjustments calculationPeriodDatesAdjustments;
    ///     </comment>
    /// </revision>
    // EG 20180205 [23769] Upd DataDocumentContainer (substitution to the static class EFS_CURRENT)  
    public class EFS_CapFlooring
    {
        #region Members
        protected string m_ConnectionString;
        protected DataDocumentContainer m_DataDocument;
        protected IProduct m_Product;
        protected decimal m_SourceRate;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected IStrikeSchedule[] m_StrikeSchedules;
        protected IBusinessDayAdjustments m_CalculationPeriodDateAdjustment;
        protected string m_EventType;
        #endregion Members
        #region Result Variables
        public decimal capFlooredRate;
        public decimal strike;
        #endregion Result Variables
        #region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CapFlooring(string pConnectionString, CapFloorPeriodInfo pCapFloorPeriodInfo, DataDocumentContainer pDataDocument)
            : this(pConnectionString, pCapFloorPeriodInfo.product, pCapFloorPeriodInfo.sourceRate, pCapFloorPeriodInfo.startDate, pCapFloorPeriodInfo.EndDate,
            pCapFloorPeriodInfo.eventType, pCapFloorPeriodInfo.strikeSchedules, pCapFloorPeriodInfo.calculationPeriodDateAdjustment, pDataDocument) { }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public EFS_CapFlooring(string pConnectionString, IProduct pProduct, decimal pSourceRate, DateTime pStartDate, DateTime pEndDate,
            string pEventType, IStrikeSchedule[] pStrikeSchedules, IBusinessDayAdjustments pCalculationPeriodDateAdjustment, DataDocumentContainer pDataDocument)
        {
            m_ConnectionString = pConnectionString;
            m_DataDocument = pDataDocument;
            m_Product = pProduct;
            m_SourceRate = pSourceRate;
            m_StartDate = pStartDate;
            m_EndDate = pEndDate;
            m_EventType = pEventType;
            m_StrikeSchedules = pStrikeSchedules;
            m_CalculationPeriodDateAdjustment = pCalculationPeriodDateAdjustment;
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel Calc()
        {
            // If the event refers to several cap(floor)RateSchedule, only the strongest has to 
            // be applied, i.e. either the highest cap strike or the lowest floor strike.
            foreach (IStrikeSchedule strikeSchedule in m_StrikeSchedules)
            {
                if (strikeSchedule.BuyerSpecified)
                {
                    // Cap events have a relative to Code
                    // xxS (cap/floor sell by the stream payer)
                    // xxB (cap/floor buy by the stream payer)
                    if ((m_EventType.EndsWith("S") && (PayerReceiverEnum.Payer == strikeSchedule.Buyer)) ||
                        (m_EventType.EndsWith("B") && (PayerReceiverEnum.Receiver == strikeSchedule.Buyer)))
                        continue;
                }
                else if (strikeSchedule.SellerSpecified)
                {
                    if ((m_EventType.EndsWith("B") && (PayerReceiverEnum.Payer == strikeSchedule.Seller)) ||
                        (m_EventType.EndsWith("S") && (PayerReceiverEnum.Receiver == strikeSchedule.Seller)))
                        continue;
                }

                #region GetStrikeValue
                strike = Tools.GetStepValue(m_ConnectionString, (ISchedule)strikeSchedule, m_StartDate, m_EndDate, m_CalculationPeriodDateAdjustment, m_DataDocument);
                #endregion GetStrikeValue


                #region capFlooredRate
                if (m_Product.ProductBase.IsCapFloor)
                {
                    if (m_EventType.StartsWith("CA"))
                    {
                        capFlooredRate = Math.Max(capFlooredRate, Math.Max(0, m_SourceRate - strike));
                    }
                    else if (m_EventType.StartsWith("FL"))
                    {
                        if ((-1 == Math.Sign(m_SourceRate)) && (0 != strike))
                            capFlooredRate = Math.Max(capFlooredRate, strike + Math.Abs(m_SourceRate));
                        else
                            capFlooredRate = Math.Max(capFlooredRate, Math.Max(0, strike - m_SourceRate));
                    }
                    capFlooredRate *= (m_EventType.EndsWith("S") ? -1 : 1);
                }
                else if (m_Product.ProductBase.IsSwap)
                {
                    if (m_EventType.StartsWith("CA"))
                    {
                        if (0 != capFlooredRate)
                            capFlooredRate = Math.Min(capFlooredRate, Math.Min(m_SourceRate, strike));
                        else
                            capFlooredRate = Math.Min(m_SourceRate, strike);
                    }
                    else if (m_EventType.StartsWith("FL"))
                    {
                        if (0 != capFlooredRate)
                            capFlooredRate = Math.Max(capFlooredRate, Math.Max(m_SourceRate, strike));
                        else
                            capFlooredRate = Math.Max(m_SourceRate, strike);
                    }
                }
                #endregion capFlooredRate
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_CapFlooring
    #region EFS_CalculAmount

    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new Parameter to Constructor for EFS_DayCountFraction valuation
    ///     </comment>
    /// </revision>
    public class EFS_CalculAmount
    {
        #region Members
        protected Nullable<decimal> m_Nominal;
        protected Nullable<decimal> m_RateMultiplier;
        protected Nullable<decimal> m_RateValue;
        protected Nullable<decimal> m_RateSpread;
        protected Nullable<decimal> m_PercentageInPoint;
        protected EFS_DayCountFraction m_DayCountFraction;
        #endregion Members
        #region Result Variables
        public Nullable<decimal> calculatedAmount;
        public Nullable<decimal> roundedCalculatedAmount;
        #endregion Result Variables
        #region Constructors
        //public EFS_CalculAmount(decimal pNominal, decimal pRateValue, int pTotalOfYears, int pNumerator, int pDenominator)
        //    : this(pNominal, 1, pRateValue, 0, pTotalOfYears, pNumerator, pDenominator) { }

        // 20071106 EG Ticket 15859
        // EG 20150219 pNominal type decimal to Nullable<decimal>
        public EFS_CalculAmount(Nullable<decimal> pNominal, Nullable<decimal> pRateValue, DateTime pStartDate, DateTime pEndDate, string pDayCountFraction, IInterval pIntervalFrequency)
            : this(pNominal, 1, pRateValue, 0, pStartDate, pEndDate, pDayCountFraction, pIntervalFrequency, null) { }

        //public EFS_CalculAmount(Nullable<decimal> pNominal, Nullable<decimal> pRateValue, DateTime pStartDate, DateTime pEndDate,
        //    string pDayCountFraction, IInterval pIntervalFrequency)
        //    : this(pNominal, 1, pRateValue, 0, pStartDate, pEndDate, pDayCountFraction, pIntervalFrequency, null) { }

        public EFS_CalculAmount(Nullable<decimal> pNominal, Nullable<decimal> pRateMultiplier, Nullable<decimal> pRateValue, Nullable<decimal> pRateSpread,
            int pTotalOfYears, int pNumerator, int pDenominator)
        {
            m_Nominal = pNominal;
            m_RateMultiplier = pRateMultiplier;
            m_RateValue = pRateValue;
            //if (0 != m_RateValue)
            //    m_RateSpread = pRateSpread;
            m_RateSpread = pRateSpread;

            m_DayCountFraction = new EFS_DayCountFraction
            {
                NumberOfCalendarYears = pTotalOfYears,
                Numerator = pNumerator,
                Denominator = pDenominator
            };

            Calc();
        }
        // 20071106 EG Ticket 15859
        // EG 20150219 pNominal type decimal to Nullable<decimal>
        public EFS_CalculAmount(Nullable<decimal> pNominal, Nullable<decimal> pRateMultiplier, Nullable<decimal> pRateValue, Nullable<decimal> pRateSpread,
            DateTime pStartDate, DateTime pEndDate, string pDayCountFraction, IInterval pIntervalFrequency, Nullable<decimal> pPercentageInPoint)
        {
            m_Nominal = pNominal ?? 0;
            m_RateMultiplier = pRateMultiplier;
            m_RateValue = pRateValue;
            //if (0 != m_RateValue)
            //    m_RateSpread = pRateSpread;
            m_RateSpread = pRateSpread;
            m_PercentageInPoint = pPercentageInPoint;
            // 20071106 EG Ticket 15859
            m_DayCountFraction = new EFS_DayCountFraction(pStartDate, pEndDate, pDayCountFraction, pIntervalFrequency);

            Calc();
        }
        #endregion Constructors
        #region Methods
        // EG 20150309 POC - BERKELEY Ajout PIP
        public Cst.ErrLevel Calc()
        {
            if (m_Nominal.HasValue && m_RateMultiplier.HasValue && m_RateValue.HasValue)
            {
                decimal calculatedRate = (m_RateMultiplier.Value * m_RateValue.Value);
                if (m_RateSpread.HasValue)
                    calculatedRate += m_RateSpread.Value;
                if (m_PercentageInPoint.HasValue)
                    calculatedRate *= m_PercentageInPoint.Value;
                calculatedAmount = m_Nominal.Value * calculatedRate * m_DayCountFraction.Factor;
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Methods
    }
    #endregion EFS_CalculAmount
    #region EFS_Compounding
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Use intervalFrequency with EFS_DayCountFraction
    ///     </comment>
    /// </revision>
    public class EFS_Compounding
    {
        #region Variables
        protected CompoundingMethodEnum m_CompoundingMethod;
        protected NegativeInterestRateTreatmentEnum m_NegativeInterestRateTreatment;
        protected EFS_CompoundingParameters m_Parameter;

        public decimal calculatedValue;

        protected EFS_DayCountFraction m_Dcf;

        #region Straight Variables
        protected decimal m_CompoundingPeriod = 0;
        protected decimal m_AdjustedCalculation = 0;
        protected decimal m_TotCompoundingPeriod = 0;
        #endregion Straight Variables
        #region Flat Variables
        protected decimal m_BasicCompoundingPeriod = 0;
        protected decimal m_FlatCompounding = 0;
        protected decimal m_AdditionalCompoundingPeriod = 0;
        protected decimal m_TotBasicCompoundingPeriod = 0;
        protected decimal m_TotAdditionalCompoundingPeriod = 0;
        #endregion Flat Variables
        #endregion Variables
        #region Accessors
        public bool IsFlatMethod
        {
            get { return CompoundingMethodEnum.Flat == m_CompoundingMethod; }
        }
        public bool IsNoneMethod
        {
            get { return CompoundingMethodEnum.None == m_CompoundingMethod; }
        }
        public bool IsStraightMethod
        {
            get { return CompoundingMethodEnum.Straight == m_CompoundingMethod; }
        }
        #endregion Accessors
        #region Constructors
        public EFS_Compounding(CompoundingMethodEnum pCompounding)
        {
            m_CompoundingMethod = pCompounding;
        }
        public EFS_Compounding(CompoundingMethodEnum pCompounding, NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
        {
            m_CompoundingMethod = pCompounding;
            m_NegativeInterestRateTreatment = pNegativeInterestRateTreatment;
        }
        #endregion Constructors
        #region Methods
        #region FlatCompoundingMethod
        protected Cst.ErrLevel Flat()
        {
            m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate, m_Parameter.endDate, m_Parameter.dayCountFraction, m_Parameter.intervalFrequency);
            #region BasicCompoundingPeriod
            m_BasicCompoundingPeriod = m_Parameter.nominal * ((m_Parameter.multiplier * m_Parameter.rate) + m_Parameter.spread) * m_Dcf.Factor;
            #region ZeroInterestRateMethod
            if ((-1 == Math.Sign(m_BasicCompoundingPeriod)) &&
                (NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
                m_BasicCompoundingPeriod = 0;
            #endregion ZeroInterestRateMethod
            #endregion BasicCompoundingPeriod
            #region FlatCompounding
            m_FlatCompounding = m_TotBasicCompoundingPeriod + m_TotAdditionalCompoundingPeriod;
            #endregion FlatCompoundingAmount
            #region AdditionalCompoundingPeriod
            m_AdditionalCompoundingPeriod = m_FlatCompounding * (m_Parameter.multiplier * m_Parameter.rate) * m_Dcf.Factor;
            #region ZeroInterestRateMethod
            if ((-1 == Math.Sign(m_AdditionalCompoundingPeriod)) &&
                (NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
                m_AdditionalCompoundingPeriod = 0;
            #endregion ZeroInterestRateMethod
            #endregion AdditionalCompoundingPeriod

            #region TotBasicCompoundingPeriod
            m_TotBasicCompoundingPeriod += m_BasicCompoundingPeriod;
            #endregion TotBasicCompoundingPeriod
            #region TotAdditionalCompoundingPeriod
            m_TotAdditionalCompoundingPeriod += m_AdditionalCompoundingPeriod;
            #endregion TotAdditionalCompoundingPeriod
            return Cst.ErrLevel.SUCCESS; 
        }
        #endregion FlatCompoundingMethod
        #region StraightCompoundingMethod
        protected Cst.ErrLevel OBSOLETE_Straight()
        {
            m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate, m_Parameter.endDate, m_Parameter.dayCountFraction, m_Parameter.intervalFrequency);
            #region AdjustedCalculation
            m_AdjustedCalculation = m_Parameter.nominal + m_TotCompoundingPeriod;
            #endregion AdjustedCalculation
            #region CompoundingPeriod
            m_CompoundingPeriod = m_AdjustedCalculation * ((m_Parameter.multiplier * m_Parameter.rate) + m_Parameter.spread) * m_Dcf.Factor;
            #region ZeroInterestRateMethod
            if ((0 > m_CompoundingPeriod) &&
                (NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
                m_CompoundingPeriod = 0;
            #endregion ZeroInterestRateMethod
            #endregion CompoundingPeriod
            calculatedValue += m_CompoundingPeriod;
            return Cst.ErrLevel.SUCCESS;
        }
        protected Cst.ErrLevel Straight()
        {
            m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate, m_Parameter.endDate, m_Parameter.dayCountFraction, m_Parameter.intervalFrequency);
            #region AdjustedCalculation
            m_AdjustedCalculation = m_Parameter.nominal + m_TotCompoundingPeriod;
            #endregion AdjustedCalculation
            #region CompoundingPeriod
            m_CompoundingPeriod = m_AdjustedCalculation * ((m_Parameter.multiplier * m_Parameter.rate) + m_Parameter.spread) * m_Dcf.Factor;
            #region ZeroInterestRateMethod
            if ((-1 == Math.Sign(m_CompoundingPeriod)) &&
                (NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
                m_CompoundingPeriod = 0;
            #endregion ZeroInterestRateMethod
            #endregion CompoundingPeriod
            m_TotCompoundingPeriod += m_CompoundingPeriod;
            return Cst.ErrLevel.SUCCESS; 
        }
        #endregion StraightCompoundingMethod
        #endregion Methods
    }
    #endregion EFS_Compounding
    #region EFS_CompoundingAmount
    public class EFS_CompoundingAmount : EFS_Compounding
    {
        #region Members
        protected EFS_CalculationPeriodEvent m_CalcPeriodEvent;
        #endregion Members
        #region Constructors
        public EFS_CompoundingAmount(CompoundingMethodEnum pCompounding, 
            EFS_CalculationPeriodEvent[] pCalcPeriodEvents, NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
            : base(pCompounding, pNegativeInterestRateTreatment)
        {
            Calc(pCalcPeriodEvents);
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public Cst.ErrLevel Calc(EFS_CalculationPeriodEvent[] pCalcPeriodEvents)
        {

            //PL 20121123 TEST - Arrondi des montants en cas de CalculationPeriod multiples sur un Payment (cf Echelles d'intérêts)
            //                   En théorie, on devrait sans doute systématiquement arrondir les montants, et ce dans toutes les méthodes.
            //                   Cependant, après le Compounding on traite le Discounting, il faudrait donc étudier si l'arrondi opéré ici pourrait entraîner une dégradation. 
            bool isApplyCurrencyRounding = (pCalcPeriodEvents.Length > 1);

            for (int i = 0; i < pCalcPeriodEvents.Length; i++)
            {
                m_CalcPeriodEvent = (EFS_CalculationPeriodEvent)pCalcPeriodEvents.GetValue(i);
                m_Parameter = new EFS_CompoundingParameters(m_CalcPeriodEvent);
                if (IsFlatMethod)
                {
                    base.Flat();
                    calculatedValue = m_TotBasicCompoundingPeriod + m_TotAdditionalCompoundingPeriod;
                    m_CalcPeriodEvent.compoundCalculatedAmount = m_BasicCompoundingPeriod + m_AdditionalCompoundingPeriod;
                }
                else if (IsNoneMethod)
                {
                    if ((1 == Math.Sign(m_CalcPeriodEvent.calculatedAmount)) ||
                        (NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod != m_NegativeInterestRateTreatment))
                    {
                        //PL 20121123 TEST - Uniquement pour l'instant en méthode "None", afin de s'exposer au minimum de dégradation !
                        if (isApplyCurrencyRounding)
                        {
                            calculatedValue += m_CalcPeriodEvent.CommonValProcess.RoundingCurrencyAmount(m_CalcPeriodEvent.Currency, m_CalcPeriodEvent.calculatedAmount);
                        }
                        else
                        {
                            calculatedValue += m_CalcPeriodEvent.calculatedAmount;
                        }
                    }
                }
                else if (IsStraightMethod)
                {
                    base.Straight();
                    calculatedValue = m_TotCompoundingPeriod;
                    m_CalcPeriodEvent.compoundCalculatedAmount = m_CompoundingPeriod;
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_CompoundingRate
    #region EFS_CompoundingParameters
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Add new member intervalFrequency
    ///     </comment>
    /// </revision>
    public class EFS_CompoundingParameters
    {
        #region Variables
        public DateTime startDate;
        public DateTime endDate;
        public decimal nominal;
        public decimal multiplier;
        public decimal rate;
        public decimal spread;
        public string dayCountFraction;
        // 20071107 EG Ticket 15859
        public IInterval intervalFrequency;

        #endregion Variables
        #region Constructors
        public EFS_CompoundingParameters(EFS_CalculationPeriodEvent pCalculationPeriodEvent)
            : this(pCalculationPeriodEvent.startDate, pCalculationPeriodEvent.EndDate,
            pCalculationPeriodEvent.nominal, pCalculationPeriodEvent.multiplier, pCalculationPeriodEvent.calculatedRate,
            pCalculationPeriodEvent.spread, pCalculationPeriodEvent.dayCountFraction, pCalculationPeriodEvent.intervalFrequency) { }

        public EFS_CompoundingParameters(EFS_SelfAveragingEvent pSelfAveragingEvent)
            : this(pSelfAveragingEvent.StartDate, pSelfAveragingEvent.EndDate, 1, 1,
            pSelfAveragingEvent.averagedAndTreatedRate, 0, pSelfAveragingEvent.dayCountFraction, pSelfAveragingEvent.intervalFrequency) { }

        public EFS_CompoundingParameters(DateTime pStartDate, DateTime pEndDate, decimal pNominal,
            decimal pMultiplier, decimal pRate, decimal pSpread, string pDayCountFraction, IInterval pIntervalFrequency)
        {
            startDate = pStartDate;
            endDate = pEndDate;
            nominal = pNominal;
            multiplier = pMultiplier;
            rate = pRate;
            spread = pSpread;
            dayCountFraction = pDayCountFraction;
            // 20071107 EG Ticket 15859
            intervalFrequency = pIntervalFrequency;
        }
        #endregion Constructors
    }
    #endregion EFS_CompoundingParameters
    #region EFS_CompoundingRate
    /// <revision>
    ///     <version>1.2.0</version><date>20071106</date><author>EG</author>
    ///     <comment>Ticket 15859
    ///     Use intervalFrequency with EFS_DayCountFraction
    ///     </comment>
    /// </revision>
    public class EFS_CompoundingRate : EFS_Compounding
    {

        #region Variables
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected int m_TotalNbDays;
        #endregion Variables
        #region Constructors
        public EFS_CompoundingRate(CompoundingMethodEnum pCompounding, DateTime pStartDate, DateTime pEndDate,
            EFS_SelfAveragingEvent[] pSelfAveragingEvents)
            : base(pCompounding)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            EFS_SelfAveragingEvent selfAveragingEvent = null;
            m_StartDate = pStartDate;
            m_EndDate = pEndDate;

            for (int i = 0; i < pSelfAveragingEvents.Length; i++)
            {
                selfAveragingEvent = (EFS_SelfAveragingEvent)pSelfAveragingEvents.GetValue(i);
                m_Parameter = new EFS_CompoundingParameters(selfAveragingEvent);

                if (IsFlatMethod)
                    ret = Flat();
                else if (IsNoneMethod)
                    ret = None();
                else if (IsStraightMethod)
                    ret = Straight();
            }

            if (Cst.ErrLevel.SUCCESS == ret)
                FinalCompounding(selfAveragingEvent.selfDayCountFraction, selfAveragingEvent.intervalFrequency);
        }
        #endregion Constructors
        #region Methods
        #region FinalCompounding
        private void FinalCompounding(string pDayCountFraction, IInterval pIntervalFrequency)
        {
            if (IsNoneMethod)
            {
                calculatedValue /= m_TotalNbDays;
            }
            else
            {
                m_Dcf = new EFS_DayCountFraction(m_StartDate, m_EndDate, pDayCountFraction, pIntervalFrequency);
                if (IsFlatMethod)
                {
                    calculatedValue = (m_TotBasicCompoundingPeriod + m_TotAdditionalCompoundingPeriod) / m_Dcf.Factor;
                }
                else if (IsStraightMethod)
                {
                    calculatedValue = m_TotCompoundingPeriod / m_Dcf.Factor;
                }
            }
        }
        #endregion FinalCompounding
        #region NoneCompoundingMethod
        // 20081112 EG Supression initialisation  m_TotalNbDays à 0
        private Cst.ErrLevel None()
        {
            m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate, m_Parameter.endDate, m_Parameter.dayCountFraction, m_Parameter.intervalFrequency);
            m_TotalNbDays += m_Dcf.TotalNumberOfCalculatedDays;
            calculatedValue += (m_Dcf.TotalNumberOfCalculatedDays * m_Parameter.rate);
            return  Cst.ErrLevel.SUCCESS;
        }
        #endregion NoneCompoundingMethod
        #endregion Methods
    }
    #endregion EFS_CompoundingRate
    #region EFS_Discounting
    public class EFS_Discounting
    {
        #region Members
        protected IProduct m_Product;
        protected decimal m_SourceValue;
        protected string m_DiscountingType;
        protected bool m_IsCompoundValue;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected decimal m_DiscountRate;
        protected string m_DiscountDayCountFraction;
        protected IInterval m_Interval;
        //
        public decimal discountedValue;
        #endregion Members
        #region Constructors
        public EFS_Discounting(IProduct pProduct, string pDiscountingType, DateTime pStartDate, DateTime pEndDate, decimal pDiscountRate, string pDiscountDayCountFraction)
            : this(pProduct, 0, pDiscountingType, false, pStartDate, pEndDate, pDiscountRate, pDiscountDayCountFraction) { }

        public EFS_Discounting(IProduct pProduct, decimal pSourceValue, string pDiscountingType, bool pIsCompoundValue,
            DateTime pStartDate, DateTime pEndDate, decimal pDiscountRate, string pDiscountDayCountFraction)
        {
            m_Product = pProduct;
            m_SourceValue = pSourceValue;
            m_DiscountingType = pDiscountingType;
            m_IsCompoundValue = pIsCompoundValue;
            m_StartDate = pStartDate;
            m_EndDate = pEndDate;
            m_DiscountRate = pDiscountRate;
            m_DiscountDayCountFraction = pDiscountDayCountFraction;
            m_Interval = pProduct.ProductBase.CreateInterval(PeriodEnum.D, 0);
        }
        #endregion Constructors
    }
    #endregion EFS_Discounting
    #region EFS_EquivalentYieldForADiscountRate
    public class EFS_EquivalentYieldForADiscountRate
    {
        #region Members
        protected decimal m_ObservedRate;
        protected DateTime m_StartDate;
        protected DateTime m_EndDate;
        protected RateTreatmentEnum m_RateTreatmentMethod;
        protected DayCountFractionEnum m_DayCountFraction = DayCountFractionEnum.ACT360;
        protected DayCountFractionEnum m_DayCountFraction2;
        #endregion Members
        #region Result Variables
        public decimal treatedRate;
        #endregion Result Variables
        #region Constructors
        public EFS_EquivalentYieldForADiscountRate(IProductBase pProductBase, RateTreatmentEnum pRateTreatment, Decimal pObservedRate,
            DateTime pStartDate, DateTime pEndDate)
        {
            m_ObservedRate = pObservedRate;
            m_StartDate = pStartDate;
            m_EndDate = pEndDate;
            m_RateTreatmentMethod = pRateTreatment;
            Calc(pProductBase);
        }
        #endregion Constructors
        #region Methods
        public Cst.ErrLevel Calc(IProductBase pProductBase)
        {
            switch (m_RateTreatmentMethod)
            {
                case RateTreatmentEnum.BondEquivalentYield:
                    m_DayCountFraction2 = DayCountFractionEnum.ACTACTISDA;
                    break;
                case RateTreatmentEnum.MoneyMarketYield:
                    m_DayCountFraction2 = DayCountFractionEnum.ACT360;
                    break;
            }
            IInterval interval = pProductBase.CreateInterval(PeriodEnum.D, 0);
            EFS_DayCountFraction dcf = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_DayCountFraction, interval);
            treatedRate = (m_ObservedRate * dcf.Denominator) / (360 - (m_ObservedRate * dcf.TotalNumberOfCalculatedDays));
            if (m_DayCountFraction2 != m_DayCountFraction)
            {
                EFS_DayCountFraction dcf2 = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_DayCountFraction2, interval);
                treatedRate = treatedRate / dcf.Denominator * dcf2.Denominator;
            }
            return Cst.ErrLevel.SUCCESS; 
        }
        #endregion Methods
    }
    #endregion EFS_EquivalentYieldForADiscountRate
    #region EFS_Interpolating
    public class EFS_Interpolating
    {
        #region Variables
        protected DateTime m_AccrualsDate;
        protected decimal m_ObservedRate;
        protected DateTime m_ObservedDate;
        protected decimal m_ObservedRate2;
        protected DateTime m_ObservedDate2;
        protected DateTime m_EndDate;
        protected RoundingDirectionEnum m_RoundingDirection;
        protected int m_RoundingPrecision;
        #endregion Variables
        #region Result Variables
        public decimal interpolatedRate;
        #endregion Result Variables
        #region Constructors
        public EFS_Interpolating(DateTime pAccrualsDate, decimal pObservedRate, DateTime pObservedDate,
            decimal pObservedRate2, DateTime pObservedDate2, DateTime pEndDate, RoundingDirectionEnum pRoundingDirection, int pRoundingPrecision)
        {
            m_AccrualsDate = pAccrualsDate;
            m_ObservedRate = pObservedRate;
            m_ObservedDate = pObservedDate;
            m_ObservedRate2 = pObservedRate2;
            m_ObservedDate2 = pObservedDate2;
            m_EndDate = pEndDate;
            m_RoundingDirection = pRoundingDirection;
            m_RoundingPrecision = pRoundingPrecision;
            Calc();
        }
        #endregion Constructors
        #region Methods
        public Cst.ErrLevel Calc()
        {
            if (!DtFunc.IsDateTimeEmpty(m_AccrualsDate))
                m_EndDate = m_AccrualsDate;
            TimeSpan timeSpan = (m_EndDate - m_ObservedDate);
            TimeSpan timeSpan2 = (m_ObservedDate2 - m_ObservedDate);
            interpolatedRate = m_ObservedRate + ((m_ObservedRate2 - m_ObservedRate) * timeSpan.Days / timeSpan2.Days);
            EFS_Round roundedRate = new EFS_Round(m_RoundingDirection, m_RoundingPrecision, interpolatedRate);
            interpolatedRate = roundedRate.AmountRounded;
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Methods
    }
    #endregion EFS_Interpolating
    #region EFS_ISDADiscounting
    public class EFS_ISDADiscounting : EFS_Discounting
    {
        #region Members
        protected new decimal m_SourceValue;
        protected decimal m_FloatingRateValue;
        protected decimal m_FixedRateValue;
        protected string m_PaymentDayCountFraction;
        #endregion Members
        #region Constructors
        public EFS_ISDADiscounting(IProduct pProduct, decimal pSourceValue, DateTime pStartDate, DateTime pEndDate,
            decimal pDiscountRate, string pDiscountDayCountFraction,
            decimal pFloatingRateValue, decimal pFixedRateValue, string pPaymentDayCountFraction)
            : base(pProduct, FraDiscountingEnum.ISDA.ToString(), pStartDate, pEndDate, pDiscountRate, pDiscountDayCountFraction)
        {
            m_SourceValue = pSourceValue;
            m_FloatingRateValue = pFloatingRateValue;
            m_FixedRateValue = pFixedRateValue;
            m_PaymentDayCountFraction = pPaymentDayCountFraction;
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public Cst.ErrLevel Calc()
        {
            EFS_DayCountFraction paymentDCF = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_PaymentDayCountFraction, m_Interval);
            base.m_SourceValue = m_SourceValue * (m_FloatingRateValue - m_FixedRateValue) * paymentDCF.Factor;
            EFS_StandardDiscounting standardDiscounting = new EFS_StandardDiscounting(m_Product, base.m_SourceValue, false, m_StartDate, m_EndDate,
                m_DiscountRate, m_DiscountDayCountFraction);
            base.discountedValue = standardDiscounting.discountedValue;
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_ISDADiscounting
    #region EFS_ObservedValue
    public class EFS_ObservedValue
    {
        public decimal Value;
        public DateTime date;

        public EFS_ObservedValue(Decimal pValue, DateTime pDate)
        {
            Value = pValue;
            date = pDate;
        }
    }
    #endregion EFS_ObservedValue
    #region EFS_StandardDiscounting
    public class EFS_StandardDiscounting : EFS_Discounting
    {
        #region Constructors
        public EFS_StandardDiscounting(IProduct pProduct, decimal pSourceValue, bool pIsCompoundValue, DateTime pStartDate, DateTime pEndDate,
            decimal pDiscountRate, string pDiscountDayCountFraction)
            : base(pProduct, pSourceValue, DiscountingTypeEnum.Standard.ToString(), pIsCompoundValue, pStartDate, pEndDate, pDiscountRate, pDiscountDayCountFraction)
        {
            Calc();
        }
        #endregion Constructors
        #region Methods
        #region Calc
        public Cst.ErrLevel Calc()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            EFS_DayCountFraction discountDCF = new EFS_DayCountFraction(m_StartDate, m_EndDate, m_DiscountDayCountFraction, m_Interval);
            if (m_IsCompoundValue)
            {
                discountedValue = m_SourceValue /
                    Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + m_DiscountRate),
                    Convert.ToDouble(discountDCF.Factor)));
            }
            else
            {
                discountedValue = m_SourceValue / (1 + (m_DiscountRate * discountDCF.Factor));
            }
            return ret;
        }
        #endregion Calc
        #endregion Methods
    }
    #endregion EFS_StandardDiscounting

    #region CommonValFunc
    public sealed class CommonValFunc
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        public static bool IsRowEventCalculated(DataRow pRow)
        {
            return StatusCalculFunc.IsCalculatedPlus(pRow["IDSTCALCUL"].ToString());
        }
        
        /// <summary>
        /// Calc Differential Amount (Used by FxNDF)
        /// </summary>
        /// <param name="pNotionalAmount"></param>
        /// <param name="pForwardRate"></param>
        /// <param name="pSettlementRate"></param>
        /// <returns></returns>
        public static decimal CalcDifferentialAmount(decimal pNotionalAmount, decimal pForwardRate, Nullable<decimal> pSettlementRate)
        {
            decimal differentialAmount = 0;
            if (pSettlementRate.HasValue && (0 < pSettlementRate.Value))
                differentialAmount = pNotionalAmount * (1 - (pForwardRate / pSettlementRate.Value));
            return differentialAmount;
        }
        
        /// <summary>
        /// Retourne la valeur d'un asset par lecture des cotations
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pKeyQuote"></param>
        /// <param name="pIdAsset"></param>
        /// <returns></returns>
        /// <exception cref="SpheresException2[QUOTENOTFOUND] lorsque la cotation est non disponible"></exception>
        public static SQL_Quote ReadQuote_AssetByType(string pCs, QuoteEnum pQuoteType, IProductBase pProductBase, KeyQuote pKeyQuote, int pIdAsset, ref SystemMSGInfo pSystemMsgInfo)
        {

            if (false == (pIdAsset > 0))
                throw new ArgumentException("parameter pIdAsset equal 0");
            //
            SQL_Quote quote = new SQL_Quote(pCs, pQuoteType, AvailabilityEnum.NA, pProductBase, pKeyQuote, pIdAsset);
            if ((false == quote.IsLoaded) || (quote.QuoteValueCodeReturn != Cst.ErrLevel.SUCCESS))
            {
                pSystemMsgInfo = quote.SystemMsgInfo;
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                    pSystemMsgInfo.Identifier, pSystemMsgInfo.processState, pSystemMsgInfo.datas);
            }
            return quote;
        }
        /// <summary>
        /// Retourne la valeur d'un asset par lecture des cotations
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pProductBase"></param>
        /// <param name="pKeyQuote"></param>
        /// <param name="pIdAsset"></param>
        /// <returns></returns>
        /// <exception cref="SpheresException2[QUOTENOTFOUND] lorsque la cotation est non disponible"></exception>
        // EG 20150312 [POC - BERKELEY]
        // EG 20180423 Analyse du code Correction [CA2200]
        public static SQL_Quote ReadQuote_AssetByType(string pCs, QuoteEnum pQuoteType, IProductBase pProductBase, KeyQuote pKeyQuote, IKeyAsset pKeyAsset, ref SystemMSGInfo pSystemMsgInfo)
        {
            try
            {
                if (null == pKeyAsset)
                    throw new ArgumentException("parameter pKeyAsset is null");

                SQL_Quote quote = new SQL_Quote(pCs, pQuoteType, AvailabilityEnum.NA, pProductBase, pKeyQuote, pKeyAsset);
                if ((false == quote.IsLoaded) || (quote.QuoteValueCodeReturn != Cst.ErrLevel.SUCCESS))
                {
                    ProcessStateTools.StatusEnum status = (pKeyQuote.Time <= OTCmlHelper.GetDateBusiness(pCs)) ? ProcessStateTools.StatusErrorEnum : ProcessStateTools.StatusWarningEnum;
                    ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, quote.QuoteValueCodeReturn);
                    pSystemMsgInfo = quote.SystemMsgInfo;
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        pSystemMsgInfo.Identifier, pSystemMsgInfo.processState, pSystemMsgInfo.datas);
                    //throw new SpheresException(processState);
                }
                return quote;
            }
            catch (Exception) { throw; }
        }

        // EG 20180423 Analyse du code Correction [CA2200]
        public static void SetRowCalculated(DataRow pRow)
        {
            try
            {
                bool isValorisationOmitted = Convert.IsDBNull(pRow["VALORISATION"]);
                bool isUnitOmitted = Convert.IsDBNull(pRow["UNITTYPE"]);
                
                if ((false == isValorisationOmitted) && (false == isUnitOmitted))
                    pRow["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
                else
                    pRow["IDSTCALCUL"] = StatusCalculFunc.ToCalculate;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// Swap les valeurs des colonnes IDA_PAY, IDB_PAY, IDA_REC, IDB_REC 
        /// </summary>
        /// <param name="pRow"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void SwapPayerAndReceiver(DataRow pRow)
        {
            try
            {
                object tmp;
                //
                tmp = pRow["IDA_PAY"];
                pRow["IDA_PAY"] = pRow["IDA_REC"];
                pRow["IDA_REC"] = tmp;
                //
                tmp = pRow["IDB_PAY"];
                pRow["IDB_PAY"] = pRow["IDB_REC"];
                pRow["IDB_REC"] = tmp;
            }
            catch (Exception) { throw; }
        }


        /// <summary>
        /// Affectation des colonnes IDA_PAY, IDB_PAY, IDA_REC, IDB_REC en fonction du signe du montant associé
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pPayer">Payer du stream (trade)</param>
        /// <param name="pReceiver">Receiver du stream (trade)</param>
        // EG 20150605 [21011] New
        public static void SetPayerAndReceiver(DataRow pRow, decimal pAmount, PayerReceiverInfoDet pPayer, PayerReceiverInfoDet pReceiver)
        {
            if (-1 == Math.Sign(pAmount))
                SetPayerReceiver(pRow, pReceiver.actor.First, pReceiver.book.First, pPayer.actor.First, pPayer.book.First);
            else
                SetPayerReceiver(pRow, pPayer.actor.First, pPayer.book.First, pReceiver.actor.First, pReceiver.book.First);
        }

        /// <summary>
        /// Affecte les colonnes IDA_PAY, IDB_PAY, IDA_REC, IDB_REC
        /// <para>Les colonnes BOOK sont valorisées avec null Si les id des books ne sont pas strictement supérieur à 0</para>
        /// </summary>
        /// <param name="pRowEvent"></param>
        /// <param name="pIdAPayer"></param>
        /// <param name="pIdBPayer"></param>
        /// <param name="pIdAReceiver"></param>
        /// <param name="pIdBReceiver"></param>
        // EG 20150219 [20520] Convert.DBNull to IdA
        // EG 20150706 [21021] Nullable<int> for pIdAPayer|pIdBPayer|pIdAReceiver|pIdBReceiver
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void SetPayerReceiver(DataRow pRow, Nullable<int> pIdAPayer, Nullable<int> pIdBPayer, Nullable<int> pIdAReceiver, Nullable<int> pIdBReceiver)
        {
            try
            {
                pRow["IDA_PAY"] = pIdAPayer ?? Convert.DBNull;
                pRow["IDB_PAY"] = pIdBPayer ?? Convert.DBNull;
                pRow["IDA_REC"] = pIdAReceiver ?? Convert.DBNull;
                pRow["IDB_REC"] = pIdBReceiver ?? Convert.DBNull;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProductBase"></param>
        /// <param name="pRateTreatment"></param>
        /// <param name="pObservedRate"></param>
        /// <param name="pStartDate"></param>
        /// <param name="pEndDate"></param>
        /// <param name="pPaymentFrequency"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        public static decimal TreatedRate(IProductBase pProductBase, RateTreatmentEnum pRateTreatment, decimal pObservedRate, DateTime pStartDate, DateTime pEndDate, IInterval pPaymentFrequency)
        {
            try
            {
                decimal treatedRate = pObservedRate;
                //
                DateTime endDate = pEndDate;
                EFS_Interval interval = new EFS_Interval(pPaymentFrequency, pStartDate, Convert.ToDateTime(null));
                TimeSpan timeSpan = interval.offsetDate - endDate;
                if (Math.Abs(timeSpan.Days) > 7)
                    endDate = interval.offsetDate;
                //
                EFS_EquivalentYieldForADiscountRate equivalentYieldForADiscountRate =
                    new EFS_EquivalentYieldForADiscountRate(pProductBase, pRateTreatment, pObservedRate, pStartDate, endDate);
                //
                treatedRate = equivalentYieldForADiscountRate.treatedRate;
                //
                return treatedRate;
            }
            catch (Exception) { throw; }
        }
    }
    #endregion CommonValFunc

}

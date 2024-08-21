#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;

using EFS.Common;
using EFS.GUI.Interface;
using EFS.Tuning;


using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;

using FixML.Interface;

#endregion Using Directives


namespace EFS.Process
{
	#region CommonValParameterBase
    // EG 20180205 [23769] Upd DataDocumentContainer
    public abstract class CommonValParameterBase
	{
		#region Members
		protected string  m_CS;
		public int instrumentNo;
		public int streamNo;
		protected IProduct m_Product;
        protected DataDocumentContainer m_DataDocument;
        #endregion Members
        #region Accessors
        #region ConnectionString
        public string CS
		{
            get { return m_CS; }
		}
		#endregion ConnectionString
		#region Product
		public IProduct Product
		{
			get {return (IProduct)m_Product;}
		}
		#endregion Product
		#endregion Accessors
		#region Constructors
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public CommonValParameterBase(string pCS, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
		{
            m_CS = pCS;
			instrumentNo = pInstrumentNo;
			streamNo = pStreamNo;
			m_Product = (IProduct)ReflectionTools.GetObjectById(pTradeLibrary.CurrentTrade,Cst.FpML_InstrumentNo + pInstrumentNo.ToString());
            m_DataDocument = pTradeLibrary.DataDocument;
		}
		#endregion Constructors
	}
	#endregion CommonValParameterBase
    #region CommonValParameters
    public abstract class CommonValParameters
    {
        #region Accessors
        public virtual CommonValParameterBase[] Parameters
        {
            get;
            set;
        }
        #endregion Accessors
        #region Constructors
        public CommonValParameters() { }
        #endregion Constructors
        #region Methods
        #region Add
        public void Add(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, DataRow pRow)
        {
            ArrayList aParameters = new ArrayList();
            int instrumentNo = Convert.ToInt32(pRow["INSTRUMENTNO"]);
            int streamNo = Convert.ToInt32(pRow["STREAMNO"]);
            if (null == this[instrumentNo, streamNo])
            {
                CommonValParameterBase[] _parameters = Parameters;
                if (ArrFunc.IsFilled(_parameters))
                {
                    for (int i = 0; i < _parameters.Length; i++)
                    {
                        aParameters.Add(_parameters[i]);
                    }
                }
                SetParameters(pConnnectionString, pTradeLibrary, instrumentNo, streamNo, aParameters);
            }
        }
        #endregion Add
        #region SetParameters
        public virtual void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
        }
        #endregion SetParameters
        #endregion Methods
        #region Indexors
        public CommonValParameterBase this[int pInstrumentNo, int pStreamNo]
        {
            get
            {
                CommonValParameterBase[] _parameters = Parameters;
                if (ArrFunc.IsFilled(_parameters))
                {
                    for (int i = 0; i < _parameters.Length; i++)
                    {
                        if ((pInstrumentNo == _parameters[i].instrumentNo) &&
                            (pStreamNo == _parameters[i].streamNo))
                            return _parameters[i];
                    }
                }
                return null;
            }
        }
        #endregion Indexors
    }
    #endregion CommonValParameters

    #region ADM
    #region CommonValParameterADM
    public class CommonValParameterADM : CommonValParameterBase
    {
        #region Constructors
        public CommonValParameterADM(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors
    }
    #endregion CommonValParameterADM
    #region CommonValParametersADM
    public class CommonValParametersADM : CommonValParameters
    {
        #region Constructors
        public CommonValParametersADM() : base() { }
        #endregion Constructors
        #region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterADM(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterADM[paParameters.Count];
            Parameters = (CommonValParameterADM[])paParameters.ToArray(typeof(CommonValParameterADM));
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CommonValParametersADM
    #endregion ADM
    #region ETD
    #region CommonValParameterETD
    public class CommonValParameterETD : CommonValParameterBase
    {
        #region Accessors
        #region ExchangeTradedDerivative
        public IExchangeTradedDerivative ExchangeTradedDerivative
        {
            get { return (IExchangeTradedDerivative)m_Product; }
        }
        #endregion ExchangeTradedDerivative
        #endregion Accessors
        #region Constructors
        public CommonValParameterETD(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors
    }
    #endregion CommonValParameterETD
    #region CommonValParametersETD
    public class CommonValParametersETD : CommonValParameters
    {
        #region Constructors
        public CommonValParametersETD() : base() { }
        #endregion Constructors
        #region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterETD(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterETD[paParameters.Count];
            Parameters = (CommonValParameterETD[])paParameters.ToArray(typeof(CommonValParameterETD));
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CommonValParametersETD
    #endregion ETD
    #region IRD
    #region CommonValParameterIRD
    /// <revision>
	///     <version>1.2.0</version><date>20071029</date><author>EG</author>
	///     <comment>Ticket 15889
	///     Step dates: Unajusted versus Ajusted
	///     Add public  member : BusinessDayAdjustments calculationPeriodDatesAdjustments;
	///     </comment>
	/// </revision>
	/// <revision>
	///     <version>1.2.0</version><date>20071107</date><author>EG</author>
	///     <comment>Ticket 15859
	///     Add new accessors CalculationPeriodFrequency     
	///     </comment>
	/// </revision>
    public class CommonValParameterIRD : CommonValParameterBase
    {
        #region Members
        private SQL_AssetRateIndex m_Rate;
        private SQL_AssetRateIndex m_Rate2;
        private SQL_AssetRateIndex m_RateBasis;
        #endregion Members
        //
        #region Accessors
        #region CalculationPeriodDateAdjustment
        // 20071029 EG Ticker 15889
        public IBusinessDayAdjustments CalculationPeriodDateAdjustment
        {
            get { return Stream.CalculationPeriodDates.CalculationPeriodDatesAdjustments; }
        }
        #endregion CalculationPeriodDateAdjustment
        #region CalculationPeriodFrequency
        // 20071106 EG Ticker 15859
        public IInterval CalculationPeriodFrequency
        {
            get
            {
                IInterestRateStream stream = Stream;
                if (null != stream)
                    return stream.CalculationPeriodDates.CalculationPeriodFrequency.Interval;
                else
                    return m_Product.ProductBase.CreateInterval(PeriodEnum.D, 0);
            }
        }
        #endregion CalculationPeriodFrequency
        #region DCFRateBasis
        public string DCFRateBasis
        {
            get { return m_Rate.FirstRow["DCFBASISRATE"].ToString(); }
        }
        #endregion DCFRateBasis
        #region FinalRateRounding
        public IRounding FinalRateRounding
        {
            get
            {
                IInterestRateStream stream = Stream;
                if (null != stream)
                {
                    if (stream.CalculationPeriodAmount.CalculationSpecified &&
                        stream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                    {
                        if (stream.CalculationPeriodAmount.Calculation.RateFloatingRate.FinalRateRoundingSpecified)
                            return stream.CalculationPeriodAmount.Calculation.RateFloatingRate.FinalRateRounding;
                    }
                }
                return null;
            }
        }
        #endregion FinalRateRounding
        #region FraDayCountFraction
        public string FraDayCountFraction
        {
            get
            {
                if (m_Product.ProductBase.IsFra)
                    return ((IFra)m_Product).DayCountFraction.ToString();
                else
                    return string.Empty;
            }
        }
        #endregion FraDayCountFraction
        #region FraDiscounting
        public FraDiscountingEnum FraDiscounting
        {
            get
            {
                if (m_Product.ProductBase.IsFra)
                    return ((IFra)m_Product).FraDiscounting;
                else
                    return FraDiscountingEnum.NONE;
            }
        }
        #endregion FraDiscounting
        #region FraFixedRate
        /// <summary>
        /// Obtient le taux fixe du Fra
        /// <para>Obtient 0 si l'instrument n'est pas un FRA</para>
        /// </summary>
        public decimal FraFixedRate
        {
            get
            {
                if (m_Product.ProductBase.IsFra)
                    return ((IFra)m_Product).FixedRate.DecValue;
                else
                    return 0;
            }
        }
        #endregion FraFixedRate
        #region FraNotional
        /// <summary>
        /// Obtient le Notionel du Fra
        /// <para>Obtient null si le produit d'un pas un Fra</para>
        /// </summary>
        public IMoney FraNotional
        {
            get
            {
                IMoney ret = null;
                if (m_Product.ProductBase.IsFra)
                    ret = ((IFra)m_Product).Notional;
                return ret;
            }
        }
        #endregion FraNotional


        #region IsPaymentRelativeToStartDate
        public bool IsPaymentRelativeToStartDate
        {
            get
            {
                bool isPaymentRelativeToStartDate = false;
                IInterestRateStream stream = Stream;
                if (null != stream)
                {
                    if (PayRelativeToEnum.ResetDate == stream.PaymentDates.PayRelativeTo)
                    {
                        if (stream.ResetDatesSpecified && stream.ResetDates.ResetRelativeToSpecified)
                            isPaymentRelativeToStartDate = (ResetRelativeToEnum.CalculationPeriodStartDate == stream.ResetDates.ResetRelativeTo);
                    }
                    else
                        isPaymentRelativeToStartDate = (PayRelativeToEnum.CalculationPeriodStartDate == stream.PaymentDates.PayRelativeTo);
                }
                return isPaymentRelativeToStartDate;
            }
        }
        #endregion IsPaymentRelativeToStartDate
        #region Rate
        public SQL_AssetRateIndex Rate
        {
            set { m_Rate = value; }
            get { return m_Rate; }
        }
        #endregion Rate
        #region Rate2
        public SQL_AssetRateIndex Rate2
        {
            set { m_Rate2 = value; }
            get { return m_Rate2; }
        }
        #endregion Rate2
        #region RateBasis
        public SQL_AssetRateIndex RateBasis
        {
            set { m_RateBasis = value; }
            get { return m_RateBasis; }
        }
        #endregion RateBasis
        #region PaymentFrequency
        public IInterval PaymentFrequency
        {
            get
            {
                if (m_Product.ProductBase.IsFra)
                    return ((IFra)m_Product).FirstIndexTenor;
                else
                    return Stream.PaymentDates.PaymentFrequency;
            }
        }
        #endregion PaymentFrequency
        #region SelfAveragingMethod
        public AveragingMethodEnum SelfAveragingMethod
        {
            get
            {
                if (null != m_Rate)
                {
                    string averagingMethod = m_Rate.FirstRow["SELFAVGMETHOD"].ToString();
                    if (System.Enum.IsDefined(typeof(AveragingMethodEnum), averagingMethod))
                        return (AveragingMethodEnum)System.Enum.Parse(typeof(AveragingMethodEnum), averagingMethod, true);
                }
                return AveragingMethodEnum.Unweighted;
            }
        }
        #endregion SelfAveragingMethod
        #region SelfCompoundingMethod
        public CompoundingMethodEnum SelfCompoundingMethod
        {
            get
            {
                if (null != m_Rate)
                {
                    string compoundingMethod = m_Rate.FirstRow["SELFCOMPOUNDMETHOD"].ToString();
                    if (System.Enum.IsDefined(typeof(CompoundingMethodEnum), compoundingMethod))
                        return (CompoundingMethodEnum)System.Enum.Parse(typeof(CompoundingMethodEnum), compoundingMethod, true);
                }
                return CompoundingMethodEnum.None;
            }
        }
        #endregion SelfAveragingMethod
        #region SelfDayCountFraction
        public string SelfDayCountFraction
        {
            get
            {
                if (null != m_Rate)
                    return m_Rate.Idx_DayCountFraction;
                return null;
            }
        }
        #endregion SelfDayCountFraction
        #region SelfFrequency
        public IInterval SelfFrequency
        {
            get
            {
                IInterval intervalFrequency = Stream.CreateInterval();
                if (null != m_Rate)
                {
                    intervalFrequency.Period = StringToEnum.Period(m_Rate.Idx_PeriodFixingOffset);
                    intervalFrequency.PeriodMultiplier = new EFS_Integer(m_Rate.Idx_PeriodMlptFixingOffset);
                }
                return intervalFrequency;
            }
        }
        #endregion SelfFrequency
        #region Stream
        /// <summary>
        /// Obtient le Stream courant (fonction de streamNo)
        /// <para>Obtient null s'il est non trouvé</para>
        /// </summary>
        public IInterestRateStream Stream
        {
            get
            {
                IInterestRateStream ret = null;
                //
                if ((null != m_Product) && 0 != streamNo)
                {
                    object streams = null;
                    if (m_Product.ProductBase.IsSaleAndRepurchaseAgreement)
                        ret = DebtSecurityStream;
                    else
                    {
                        Type tProduct = m_Product.GetType();
                        PropertyInfo pty = tProduct.GetProperty("Stream");
                        if (null != pty)
                            streams = tProduct.InvokeMember(pty.Name, BindingFlags.GetProperty, null, m_Product, null);
                        if (null != streams)
                        {
                            if (streams.GetType().IsArray)
                                ret = (IInterestRateStream)((IInterestRateStream[])streams).GetValue(streamNo - 1);
                            else
                                ret = (IInterestRateStream)streams;
                        }
                    }
                }
                return ret;
            }
        }
        #endregion Stream
        #region DebtSecurityStream
        public IInterestRateStream DebtSecurityStream
        {
            get
            {
                if ((null != m_Product) && 0 != streamNo)
                {
                    object streams = null;
                    object[] argValues = new object[1] { streamNo };
                    Type tProduct = m_Product.GetType();
                    MethodInfo method = tProduct.GetMethod("DebtSecurityStream");

                    if (null != method)
                        streams = tProduct.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, m_Product, argValues, null, null, null);
                    if (null != streams)
                        return (IInterestRateStream)streams;
                }
                return null;
            }
        }
        #endregion Stream
        #endregion Accessors
        //
        #region Constructors
        public CommonValParameterIRD(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors
        //
        #region Methods
        #region AveragingMethod
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement AveragingMethodEnum 
        /// <para>Retourne Cst.ErrLevel.DATANOTFOUND si l'élement n'est pas trouvé </para>
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé</para>
        /// </summary>
        public Cst.ErrLevel AveragingMethod(out AveragingMethodEnum pAveragingMethod)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            IInterestRateStream stream = Stream;
            pAveragingMethod = AveragingMethodEnum.Unweighted;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                {
                    if (stream.CalculationPeriodAmount.Calculation.RateFloatingRate.AveragingMethodSpecified)
                    {
                        pAveragingMethod = stream.CalculationPeriodAmount.Calculation.RateFloatingRate.AveragingMethod;
                        ret = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            return ret;
        }
        #endregion AveragingMethod
        #region CompoundingMethod
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement CompoundingMethodEnum 
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé ou par defaut (NONE)</para>
        /// </summary>
        // EG 20110315 Retourne toujour SUCCESS 
        public Cst.ErrLevel CompoundingMethod(out CompoundingMethodEnum pCompoundingMethod)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            pCompoundingMethod = CompoundingMethodEnum.None;
            IInterestRateStream stream = Stream;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.CompoundingMethodSpecified)
                    pCompoundingMethod = stream.CalculationPeriodAmount.Calculation.CompoundingMethod;
            }
            return ret;
        }
        #endregion CompoundingMethod
        #region Discounting
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement discounting 
        /// <para>Retourne Cst.ErrLevel.DATANOTFOUND si l'élement n'est pas trouvé</para>
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé</para>
        /// </summary>
        /// <param name="pNegativeInterestRateTreatment"></param>
        /// <returns></returns>
        public Cst.ErrLevel Discounting(out IDiscounting pDiscounting)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pDiscounting = null;
            IInterestRateStream stream = Stream;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.DiscountingSpecified)
                {
                    pDiscounting = stream.CalculationPeriodAmount.Calculation.Discounting;
                    ret = Cst.ErrLevel.SUCCESS;
                }
            }
            return ret;
        }
        #endregion Discounting
        #region GetFixingDateOffset
        public Cst.ErrLevel GetFixingDateOffset(out IBusinessDayAdjustments pBda, out IOffset pOffset)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            //
            pBda = null;
            pOffset = null;
            //
            IInterestRateStream stream = Stream;
            if ((null != stream) && stream.ResetDatesSpecified)
            {
                ret = Cst.ErrLevel.SUCCESS;
                pBda = stream.ResetDates.FixingDates.GetAdjustments;
                pOffset = stream.ResetDates.FixingDates.GetOffset;
            }
            //
            return ret;
        }
        #endregion GetFixingDateOffset
        #region GetPayerReceiverInfoDet
        /// <summary>
        /// Obtient le Payer|Receiver du Stream courant (fonction de streamNo)
        /// </summary>
        // EG 20150605 [21011] New
        public PayerReceiverInfoDet GetPayerReceiverInfoDet(DataDocumentContainer pDataDocument, PayerReceiverEnum pPayerReceiver)
        {
            PayerReceiverInfoDet payerReceiverInfoDet = null;
            IInterestRateStream stream = Stream;
            if (null != stream)
            {
                string hRef = stream.GetPayerPartyReference;
                if (PayerReceiverEnum.Receiver == pPayerReceiver)
                    hRef = stream.GetReceiverPartyReference;

                IParty party = pDataDocument.GetParty(hRef);
                IBookId book = pDataDocument.GetBookId(hRef);
                // EG 20150624 Test book is null
                // RD 20150806 Book is null => idb=null
                Nullable<int> idb = null;
                if (null != book)
                    idb = book.OTCmlId;
                payerReceiverInfoDet = new PayerReceiverInfoDet(pPayerReceiver, party.OTCmlId, party.PartyName, idb, (null != book) ? book.Value : string.Empty);
            }
            return payerReceiverInfoDet;
        }
        #endregion GetPayerReceiverInfoDet

        #region GetRateSpreadAndMultiplier
        // 20071029 EG Ticker 15889
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel GetRateSpreadAndMultiplier(DateTime pStartDate, DateTime pEndDate, out decimal pMultiplier, out decimal pSpread)
        {
            IInterestRateStream stream = Stream;
            pMultiplier = 1;
            pSpread = 0;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                {
                    ICalculation calculation = stream.CalculationPeriodAmount.Calculation;
                    #region Spread
                    if (calculation.RateFloatingRate.SpreadScheduleSpecified)
                        // 20071029 EG Ticker 15889
                        pSpread = Tools.GetStepValue(m_CS, calculation.RateFloatingRate.SpreadSchedule, 
                            pStartDate, pEndDate, CalculationPeriodDateAdjustment, m_DataDocument);
                    #endregion Spread
                    #region Multiplier
                    if (calculation.RateFloatingRate.FloatingRateMultiplierScheduleSpecified)
                        // 20071029 EG Ticker 15889
                        pMultiplier = Tools.GetStepValue(m_CS, calculation.RateFloatingRate.FloatingRateMultiplierSchedule,
                            pStartDate, pEndDate, CalculationPeriodDateAdjustment, m_DataDocument);
                    #endregion Multiplier
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion GetRateSpreadAndMultiplier
        #region GetStreamNotionalReference
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel GetStreamNotionalReference(out int pInstrumentNo, out int pStreamNo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pInstrumentNo = 0;
            pStreamNo = 0;
            IInterestRateStream stream = Stream;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.FxLinkedNotionalSpecified)
                {
                    IFxLinkedNotionalSchedule fxLinkedNotionalSchedule =
                        stream.CalculationPeriodAmount.Calculation.FxLinkedNotional;
                    string hRef = fxLinkedNotionalSchedule.ConstantNotionalScheduleReference.HRef;
                    int instrumentNo = 0;
                    int streamNo = 0;
                    object notionalReference = Tools.GetObjectById(m_DataDocument, hRef, ref instrumentNo, ref streamNo);
                    if (Tools.IsTypeOrInterfaceOf(notionalReference, InterfaceEnum.INotional))
                    {
                        pInstrumentNo = instrumentNo;
                        pStreamNo = streamNo;
                        ret = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            return ret;
        }
        #endregion GetStreamNotionalReference
        #region GetStrikeSchedule
        public Cst.ErrLevel GetStrikeSchedule(string pEventType, out IStrikeSchedule[] pStrikeSchedules)
        {
            IInterestRateStream stream = Stream;
            pStrikeSchedules = null;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                {
                    ICalculation calculation = stream.CalculationPeriodAmount.Calculation;
                    #region CapRateSchedule
                    if (calculation.RateFloatingRate.CapRateScheduleSpecified && pEventType.StartsWith("CA"))
                        pStrikeSchedules = calculation.RateFloatingRate.CapRateSchedule;
                    #endregion CapRateSchedule
                    #region FloorRateSchedule
                    else if (calculation.RateFloatingRate.FloorRateScheduleSpecified && pEventType.StartsWith("FL"))
                        pStrikeSchedules = calculation.RateFloatingRate.FloorRateSchedule;
                    #endregion FloorRateSchedule
                }
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion GetStrikeSchedule
        #region NegativeInterestRateTreatment
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement NegativeInterestRateTreatmentEnum 
        /// <para>Retourne Cst.ErrLevel.DATANOTFOUND si l'élement n'est pas trouvé </para>
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé</para>
        /// </summary>
        /// <param name="pNegativeInterestRateTreatment"></param>
        /// <returns></returns>
        public Cst.ErrLevel NegativeInterestRateTreatment(out NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pNegativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.NegativeInterestRateMethod;
            if (null != Stream)
            {
                if (Stream.CalculationPeriodAmount.CalculationSpecified &&
                    Stream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                {
                    ICalculation calculation = Stream.CalculationPeriodAmount.Calculation;
                    if (calculation.RateFloatingRate.NegativeInterestRateTreatmentSpecified)
                    {
                        pNegativeInterestRateTreatment = calculation.RateFloatingRate.NegativeInterestRateTreatment;
                        ret = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            return ret;
        }
        #endregion NegativeInterestRateTreatment
        #region RateTreatment
        public Cst.ErrLevel RateTreatment(out RateTreatmentEnum pRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            IInterestRateStream stream = Stream;
            pRateTreatment = RateTreatmentEnum.BondEquivalentYield;
            if (null != stream)
            {
                if (stream.CalculationPeriodAmount.CalculationSpecified &&
                    stream.CalculationPeriodAmount.Calculation.RateFloatingRateSpecified)
                {
                    ICalculation calculation = stream.CalculationPeriodAmount.Calculation;
                    if (calculation.RateFloatingRate.RateTreatmentSpecified)
                    {
                        pRateTreatment = calculation.RateFloatingRate.RateTreatment;
                        ret = Cst.ErrLevel.SUCCESS;
                    }
                }
            }
            return ret;
        }
        #endregion RateTreatment
        #region RateTreatmentRateBasis
        public Cst.ErrLevel RateTreatmentRateBasis(out RateTreatmentEnum pRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pRateTreatment = RateTreatmentEnum.BondEquivalentYield;
            if (System.Enum.IsDefined(typeof(RateTreatmentEnum), m_RateBasis.FirstRow["Idx_RATETREATMENT"].ToString()))
            {
                ret = Cst.ErrLevel.SUCCESS;
                pRateTreatment = (RateTreatmentEnum)System.Enum.Parse(typeof(RateTreatmentEnum),
                    m_RateBasis.FirstRow["Idx_RATETREATMENT"].ToString(), true);
            }
            return ret;
        }
        #endregion RateTreatmentRateBasis
        #endregion Methods
    }
	#endregion CommonValParameterIRD
	#region CommonValParametersIRD
    public class CommonValParametersIRD : CommonValParameters
	{
		#region Constructors
        public CommonValParametersIRD() : base() { }
		#endregion Constructors
		#region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterIRD(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterIRD[paParameters.Count];
            Parameters = (CommonValParameterIRD[])paParameters.ToArray(typeof(CommonValParameterIRD));
        }
        #endregion SetParameters
        #endregion Methods
	}
	#endregion CommonValParametersIRD
	#endregion IRD
	#region FX
	#region CommonValParameterFX
	public class CommonValParameterFX : CommonValParameterBase
	{
		#region Constructors
		public CommonValParameterFX(string pConnectionString,EFS_TradeLibrary pTradeLibrary,int pInstrumentNo,int pStreamNo)
			:base(pConnectionString,pTradeLibrary,pInstrumentNo,pStreamNo){}
		#endregion Constructors
	}
	#endregion CommonValParameterFX
	#region CommonValParametersFX
    public class CommonValParametersFX : CommonValParameters
	{
		#region Constructors
        public CommonValParametersFX() : base() { }
		#endregion Constructors
		#region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterFX(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterFX[paParameters.Count];
            Parameters = (CommonValParameterFX[])paParameters.ToArray(typeof(CommonValParameterFX));
        }
        #endregion SetParameters
        #endregion Methods
	}
	#endregion EventsValParametersFX
	#endregion FX
    #region SEC
    #region CommonValParameterSEC
    public class CommonValParameterSEC : CommonValParameterIRD
    {
        #region Accessors
        #region DebtSecurity
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public IDebtSecurity DebtSecurity
        {
            get
            {
                IDebtSecurity debtSecurity = null;
                if (m_Product.ProductBase.IsDebtSecurity)
                {
                    debtSecurity = (IDebtSecurity)m_Product;
                }
                else if (m_Product.ProductBase.IsDebtSecurityTransaction)
                {
                    IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)m_Product;
                    if (debtSecurityTransaction.SecurityAssetSpecified)
                        debtSecurity = debtSecurityTransaction.SecurityAsset.DebtSecurity;
                    else if (debtSecurityTransaction.SecurityAssetReferenceSpecified)
                        debtSecurity = (IDebtSecurity)ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, debtSecurityTransaction.SecurityAssetReference.HRef);
                }
                return debtSecurity;                    
            }
        }
        #endregion DebtSecurity
        #region FullCouponCalculationRules
        public IFullCouponCalculationRules FullCouponCalculationRules
        {
            get
            {
                IFullCouponCalculationRules fullCouponCalculationRules = null;
                ISecurityCalculationRules calculationRules = SecurityCalculationRules;
                if ((null != calculationRules) && (calculationRules.FullCouponCalculationRulesSpecified))
                    fullCouponCalculationRules = calculationRules.FullCouponCalculationRules;
                return fullCouponCalculationRules;
            }
        }
        #endregion FullCouponCalculationRules
        #region FullCouponRecordDate
        // EG 20150904 New
        public IRelativeDateOffset FullCouponRecordDate
        {
            get
            {
                IRelativeDateOffset offset = null;
                IFullCouponCalculationRules fullCouponCalculationRules = FullCouponCalculationRules;
                if ((null != fullCouponCalculationRules) && fullCouponCalculationRules.RecordDateSpecified)
                    offset = fullCouponCalculationRules.RecordDate;
                return offset;
            }
        }
        #endregion FullCouponRecordDate
        #region FullCouponExDate
        // EG 20150904 New
        public IRelativeDateOffset FullCouponExDate
        {
            get
            {
                IRelativeDateOffset offset = null;
                IFullCouponCalculationRules fullCouponCalculationRules = FullCouponCalculationRules;
                if ((null != fullCouponCalculationRules) && fullCouponCalculationRules.ExDateSpecified)
                    offset = fullCouponCalculationRules.ExDate;
                return offset;
            }
        }
        #endregion FullCouponExDate
        #region FullCouponRounding
        public IRounding FullCouponRounding
        {
            get
            {
                IRounding rounding = null;
                IFullCouponCalculationRules fullCouponCalculationRules = FullCouponCalculationRules;
                if ((null != fullCouponCalculationRules) && fullCouponCalculationRules.RoundingSpecified)
                    rounding = fullCouponCalculationRules.Rounding;
                return rounding;
            }
        }
        #endregion FullCouponRounding
        #region SecurityCalculationRules
        public ISecurityCalculationRules SecurityCalculationRules
        {
            get
            {
                ISecurityCalculationRules calculationRules = null;
                IDebtSecurity debtSecurity = DebtSecurity;
                if ((null != debtSecurity) && debtSecurity.Security.CalculationRulesSpecified)
                    calculationRules =  debtSecurity.Security.CalculationRules;
                return calculationRules;                    
            }
        }
        #endregion SecurityCalculationRules
        #region UnitCouponRounding
        public IRounding UnitCouponRounding
        {
            get
            {
                IRounding rounding = null;
                IFullCouponCalculationRules fullCouponCalculationRules = FullCouponCalculationRules;
                if ((null != fullCouponCalculationRules) && fullCouponCalculationRules.UnitCouponRoundingSpecified)
                    rounding = fullCouponCalculationRules.UnitCouponRounding;
                return rounding;
            }
        }
        #endregion UnitCouponRounding
        #endregion Accessors
        #region Constructors
        public CommonValParameterSEC(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors
    }
    #endregion CommonValParameterSEC
    #region CommonValParametersSEC
    public class CommonValParametersSEC : CommonValParameters
    {
        #region Constructors
        public CommonValParametersSEC() : base() { }
        #endregion Constructors
        #region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterSEC(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterSEC[paParameters.Count];
            Parameters = (CommonValParameterSEC[])paParameters.ToArray(typeof(CommonValParameterSEC));
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CommonValParametersSEC
    #endregion SEC
    #region ESE
    #region CommonValParameterESE
    public class CommonValParameterESE : CommonValParameterBase
    {
        #region Members
        private SQL_AssetEquity m_Rate;
        #endregion Members

        #region Accessors
        #region Accessors
        #region CalculationPeriodFrequency
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public IInterval CalculationPeriodFrequency
        {
            get
            {
                IInterval _calculationPeriodFrequency = m_Product.ProductBase.CreateInterval(PeriodEnum.D, 0);
                IInterestLeg interestLeg = InterestLeg;
                if (null != interestLeg)
                {
                    IAdjustableRelativeOrPeriodicDates2 _payment = interestLeg.CalculationPeriodDates.PaymentDates;
                    if (_payment.PeriodicDatesSpecified)
                        _calculationPeriodFrequency = _payment.PeriodicDates.CalculationPeriodFrequency.Interval;
                    else if (_payment.RelativeDatesSpecified)
                    {
                        string hRef = ((IRelativeDateOffset)_payment.RelativeDates).DateRelativeToValue;
                        object objRef = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, hRef);
                        if ((objRef is IAdjustableRelativeOrPeriodicDates dates) && dates.PeriodicDatesSpecified)
                            _calculationPeriodFrequency = dates.PeriodicDates.CalculationPeriodFrequency.Interval;
                    }
                }
                return _calculationPeriodFrequency;
            }
        }
        #endregion CalculationPeriodFrequency

        #region FinalRateRounding
        public IRounding FinalRateRounding
        {
            get
            {
                IInterestLeg interestLeg = InterestLeg;
                if (null != interestLeg)
                {
                    if (interestLeg.InterestCalculation.FloatingRateSpecified)
                    {
                        if (interestLeg.InterestCalculation.FloatingRate.FinalRateRoundingSpecified)
                            return interestLeg.InterestCalculation.FloatingRate.FinalRateRounding;
                    }
                }
                return null;
            }
        }
        #endregion FinalRateRounding
        #region InterestLeg
        /// <summary>
        /// Obtient le InterestLeg courant (fonction de streamNo)
        /// <para>Obtient null s'il est non trouvé</para>
        /// </summary>
        public IInterestLeg InterestLeg
        {
            get
            {
                IInterestLeg ret = null;
                if ((null != m_Product) && 0 != streamNo)
                {
                    object legs = null;
                    Type tProduct = m_Product.GetType();
                    PropertyInfo pty = tProduct.GetProperty("InterestLeg");
                    if (null != pty)
                        legs = tProduct.InvokeMember(pty.Name, BindingFlags.GetProperty, null, m_Product, null);
                    if (null != legs)
                    {
                        if (legs.GetType().IsArray)
                            ret = (IInterestLeg)((IInterestLeg[])legs).GetValue(streamNo - ((Array)legs).Length -1 );
                        else
                            ret = (IInterestLeg)legs;
                    }
                }
                return ret;
            }
        }
        #endregion InterestLeg
        #region Rate
        public SQL_AssetEquity Rate
        {
            set { m_Rate = value; }
            get { return m_Rate; }
        }
        #endregion Rate
        #endregion Accessors


        #endregion Accessors
        #region Constructors
        public CommonValParameterESE(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors

        #region methods
        #region CompoundingMethod
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement CompoundingMethodEnum 
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé ou par defaut (NONE)</para>
        /// </summary>
        // EG 20110315 Retourne toujour SUCCESS 
        public Cst.ErrLevel CompoundingMethod(out CompoundingMethodEnum pCompoundingMethod)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            pCompoundingMethod = CompoundingMethodEnum.None;
            IInterestLeg interestLeg = InterestLeg;
            if (null != interestLeg)
            {
                if (interestLeg.InterestCalculation.CompoundingSpecified && 
                    interestLeg.InterestCalculation.Compounding.CompoundingMethodSpecified)
                    pCompoundingMethod = interestLeg.InterestCalculation.Compounding.CompoundingMethod;
            }
            return ret;
        }
        #endregion CompoundingMethod
        #region GetFixingDateOffset
        public Cst.ErrLevel GetFixingDateOffset(out IBusinessDayAdjustments pBda, out IOffset pOffset)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pBda = null;
            pOffset = null;
            IInterestLeg interestLeg = InterestLeg;
            if ((null != interestLeg) && (null != interestLeg.CalculationPeriodDates.ResetDates))
            {
                ret = Cst.ErrLevel.SUCCESS;
                if (interestLeg.CalculationPeriodDates.ResetDates.FixingDatesSpecified &&
                    interestLeg.CalculationPeriodDates.ResetDates.FixingDates.RelativeDateOffsetSpecified)
                {
                    pBda = interestLeg.CalculationPeriodDates.ResetDates.FixingDates.RelativeDateOffset.GetAdjustments;
                    pOffset = interestLeg.CalculationPeriodDates.ResetDates.FixingDates.RelativeDateOffset.GetOffset;
                }
            }
            return ret;
        }
        #endregion GetFixingDateOffset

        #region NegativeInterestRateTreatment
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement NegativeInterestRateTreatmentEnum 
        /// <para>Retourne Cst.ErrLevel.DATANOTFOUND si l'élement n'est pas trouvé </para>
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé</para>
        /// </summary>
        /// <param name="pNegativeInterestRateTreatment"></param>
        /// <returns></returns>
        public Cst.ErrLevel NegativeInterestRateTreatment(out NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pNegativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.NegativeInterestRateMethod;
            IInterestLeg interestLeg = InterestLeg;
            if (null != interestLeg)
            {
                if (interestLeg.InterestCalculation.FloatingRateSpecified && interestLeg.InterestCalculation.FloatingRate.NegativeInterestRateTreatmentSpecified)
                {
                    pNegativeInterestRateTreatment = interestLeg.InterestCalculation.FloatingRate.NegativeInterestRateTreatment;
                    ret = Cst.ErrLevel.SUCCESS;
                }
            }
            return ret;
        }
        #endregion NegativeInterestRateTreatment
        #region RateTreatment
        public Cst.ErrLevel RateTreatment(out RateTreatmentEnum pRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            IInterestLeg interestLeg = InterestLeg;
            pRateTreatment = RateTreatmentEnum.BondEquivalentYield;
            if (null != interestLeg)
            {
                if (interestLeg.InterestCalculation.FloatingRateSpecified && interestLeg.InterestCalculation.FloatingRate.RateTreatmentSpecified)
                {
                    pRateTreatment = interestLeg.InterestCalculation.FloatingRate.RateTreatment;
                    ret = Cst.ErrLevel.SUCCESS;
                }
            }
            return ret;
        }
        #endregion RateTreatment

        #endregion methods
    }
    #endregion CommonValParameterRTS
    #region CommonValParametersESE
    public class CommonValParametersESE : CommonValParameters
    {
        #region Constructors
        public CommonValParametersESE() : base() { }
        #endregion Constructors
        #region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterESE(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterESE[paParameters.Count];
            Parameters = (CommonValParameterESE[])paParameters.ToArray(typeof(CommonValParameterESE));
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CommonValParametersESE
    #endregion ESE
    #region RTS
    #region CommonValParameterRTS
    public class CommonValParameterRTS : CommonValParameterBase
    {
        #region Members
        private SQL_AssetRateIndex m_Rate;
        #endregion Members

        #region Accessors
        #region Accessors
        #region CalculationPeriodFrequency
        // EG 20180205 [23769] Call ReflectionTools.GetObjectById (substitution to the static class EFS_CURRENT)  
        public IInterval CalculationPeriodFrequency
        {
            get
            {
                IInterval _calculationPeriodFrequency = m_Product.ProductBase.CreateInterval(PeriodEnum.D, 0);
                IInterestLeg interestLeg = InterestLeg;
                if (null != interestLeg)
                {
                    IAdjustableRelativeOrPeriodicDates2 _payment = interestLeg.CalculationPeriodDates.PaymentDates;
                    if (_payment.PeriodicDatesSpecified)
                        _calculationPeriodFrequency = _payment.PeriodicDates.CalculationPeriodFrequency.Interval;
                    else if (_payment.RelativeDatesSpecified)
                    {
                        string hRef = ((IRelativeDateOffset)_payment.RelativeDates).DateRelativeToValue;
                        object objRef = ReflectionTools.GetObjectById(m_DataDocument.DataDocument.Item, hRef);
                        if ((objRef is IAdjustableRelativeOrPeriodicDates dates) && dates.PeriodicDatesSpecified)
                            _calculationPeriodFrequency = dates.PeriodicDates.CalculationPeriodFrequency.Interval;
                    }
                }
                return _calculationPeriodFrequency;
            }
        }
        #endregion CalculationPeriodFrequency

        #region FinalRateRounding
        public IRounding FinalRateRounding
        {
            get
            {
                IInterestLeg interestLeg = InterestLeg;
                if (null != interestLeg)
                {
                    if (interestLeg.InterestCalculation.FloatingRateSpecified)
                    {
                        if (interestLeg.InterestCalculation.FloatingRate.FinalRateRoundingSpecified)
                            return interestLeg.InterestCalculation.FloatingRate.FinalRateRounding;
                    }
                }
                return null;
            }
        }
        #endregion FinalRateRounding
        #region InterestLeg
        /// <summary>
        /// Obtient le InterestLeg courant (fonction de streamNo)
        /// <para>Obtient null s'il est non trouvé</para>
        /// </summary>
        public IInterestLeg InterestLeg
        {
            get
            {
                IInterestLeg ret = null;
                if ((null != m_Product) && 0 != streamNo)
                {
                    object legs = null;
                    Type tProduct = m_Product.GetType();
                    PropertyInfo pty = tProduct.GetProperty("InterestLeg");
                    if (null != pty)
                        legs = tProduct.InvokeMember(pty.Name, BindingFlags.GetProperty, null, m_Product, null);
                    if (null != legs)
                    {
                        if (legs.GetType().IsArray)
                            ret = (IInterestLeg)((IInterestLeg[])legs).GetValue(streamNo - ((Array)legs).Length - 1);
                        else
                            ret = (IInterestLeg)legs;
                    }
                }
                return ret;
            }
        }
        #endregion InterestLeg
        #region Rate
        public SQL_AssetRateIndex Rate
        {
            set { m_Rate = value; }
            get { return m_Rate; }
        }
        #endregion Rate
        #endregion Accessors
        #endregion Accessors
        #region Constructors
        public CommonValParameterRTS(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors

        #region Methods
        #region CompoundingMethod
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement CompoundingMethodEnum 
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé ou par defaut (NONE)</para>
        /// </summary>
        // EG 20110315 Retourne toujour SUCCESS 
        public Cst.ErrLevel CompoundingMethod(out CompoundingMethodEnum pCompoundingMethod)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            pCompoundingMethod = CompoundingMethodEnum.None;
            IInterestLeg interestLeg = InterestLeg;
            if (null != interestLeg)
            {
                if (interestLeg.InterestCalculation.CompoundingSpecified &&
                    interestLeg.InterestCalculation.Compounding.CompoundingMethodSpecified)
                    pCompoundingMethod = interestLeg.InterestCalculation.Compounding.CompoundingMethod;
            }
            return ret;
        }
        #endregion CompoundingMethod
        #region GetFixingDateOffset
        public Cst.ErrLevel GetFixingDateOffset(out IBusinessDayAdjustments pBda, out IOffset pOffset)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pBda = null;
            pOffset = null;
            IInterestLeg interestLeg = InterestLeg;
            if ((null != interestLeg) && (null != interestLeg.CalculationPeriodDates.ResetDates))
            {
                ret = Cst.ErrLevel.SUCCESS;
                if (interestLeg.CalculationPeriodDates.ResetDates.FixingDatesSpecified &&
                    interestLeg.CalculationPeriodDates.ResetDates.FixingDates.RelativeDateOffsetSpecified)
                {
                    pBda = interestLeg.CalculationPeriodDates.ResetDates.FixingDates.RelativeDateOffset.GetAdjustments;
                    pOffset = interestLeg.CalculationPeriodDates.ResetDates.FixingDates.RelativeDateOffset.GetOffset;
                }
            }
            return ret;
        }
        #endregion GetFixingDateOffset

        #region NegativeInterestRateTreatment
        /// <summary>
        /// Celle méthode recherche sur le stream courant l'élement NegativeInterestRateTreatmentEnum 
        /// <para>Retourne Cst.ErrLevel.DATANOTFOUND si l'élement n'est pas trouvé </para>
        /// <para>Retourne Cst.ErrLevel.SUCCES si l'élement est trouvé</para>
        /// </summary>
        /// <param name="pNegativeInterestRateTreatment"></param>
        /// <returns></returns>
        public Cst.ErrLevel NegativeInterestRateTreatment(out NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            pNegativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.NegativeInterestRateMethod;
            IInterestLeg interestLeg = InterestLeg;
            if (null != interestLeg)
            {
                if (interestLeg.InterestCalculation.FloatingRateSpecified && interestLeg.InterestCalculation.FloatingRate.NegativeInterestRateTreatmentSpecified)
                {
                    pNegativeInterestRateTreatment = interestLeg.InterestCalculation.FloatingRate.NegativeInterestRateTreatment;
                    ret = Cst.ErrLevel.SUCCESS;
                }
            }
            return ret;
        }
        #endregion NegativeInterestRateTreatment
        #region RateTreatment
        public Cst.ErrLevel RateTreatment(out RateTreatmentEnum pRateTreatment)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
            IInterestLeg interestLeg = InterestLeg;
            pRateTreatment = RateTreatmentEnum.BondEquivalentYield;
            if (null != interestLeg)
            {
                if (interestLeg.InterestCalculation.FloatingRateSpecified && interestLeg.InterestCalculation.FloatingRate.RateTreatmentSpecified)
                {
                    pRateTreatment = interestLeg.InterestCalculation.FloatingRate.RateTreatment;
                    ret = Cst.ErrLevel.SUCCESS;
                }
            }
            return ret;
        }
        #endregion RateTreatment
        #endregion Methods
    }
    #endregion CommonValParameterESE
    #region CommonValParametersRTS
    public class CommonValParametersRTS : CommonValParameters
    {
        #region Constructors
        public CommonValParametersRTS():base() { }
        #endregion Constructors
        #region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterRTS(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterRTS[paParameters.Count];
            Parameters = (CommonValParameterRTS[])paParameters.ToArray(typeof(CommonValParameterRTS));
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CommonValParametersRTS
    #endregion RTS
    #region DST
    #region CommonValParameterDST
    public class CommonValParameterDST : CommonValParameterBase
    {
        #region Members
        // EG 20160404 Migration vs2013
        //private SQL_AssetRateIndex m_Rate;
        #endregion Members

        #region Accessors
        #endregion Accessors
        #region Constructors
        public CommonValParameterDST(string pConnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo)
            : base(pConnectionString, pTradeLibrary, pInstrumentNo, pStreamNo) { }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }
    #endregion CommonValParameterDST
    #region CommonValParametersDST
    public class CommonValParametersDST : CommonValParameters
    {
        #region Constructors
        public CommonValParametersDST() : base() { }
        #endregion Constructors
        #region Methods
        #region SetParameters
        public override void SetParameters(string pConnnectionString, EFS_TradeLibrary pTradeLibrary, int pInstrumentNo, int pStreamNo, ArrayList paParameters)
        {
            paParameters.Add(new CommonValParameterDST(pConnnectionString, pTradeLibrary, pInstrumentNo, pStreamNo));
            Parameters = new CommonValParameterDST[paParameters.Count];
            Parameters = (CommonValParameterDST[])paParameters.ToArray(typeof(CommonValParameterDST));
        }
        #endregion SetParameters
        #endregion Methods
    }
    #endregion CommonValParametersDST
    #endregion DST

}

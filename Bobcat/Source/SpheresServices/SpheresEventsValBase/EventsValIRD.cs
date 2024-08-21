#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.Common;
using EFS.Tuning;

using EfsML;
using EfsML.Enum;
using EfsML.Extended;

using FpML.Enum;
using FpML.Ird;
using FpML.Shared;

#endregion Using Directives


namespace EFS.Process
{
	#region EventsValProcessIRD
	public class EventsValProcessIRD : EventsValProcessBase
	{
		#region Members
		protected  EventsValParametersIRD m_Parameters;
		protected  IDbDataParameter       m_ParamInstrumentNoReference;
		protected  IDbDataParameter       m_ParamStreamNoReference;
		#endregion Members
		
		#region Accessors
		#region Parameters
		public  EventsValParametersIRD Parameters
		{
			get {return m_Parameters;}
		}
		#endregion Parameters
		#region RowInterest
		private  DataRow[] RowInterest
		{
			get
			{
				return DsEvents.DtEvent.Select("IDPARENT=" + m_ParamIdT.Value.ToString() + 
					" and EVENTTYPE=" + DataHelper.SQLString(EventTypeFunc.Interest),"ID");
			}
		}
		#endregion RowInterest
		#region RowNominalReference
		private  DataRow[] RowNominalReference
		{
			get
			{
				return DsEvents.DtEvent.Select("IDPARENT=" + m_ParamIdT.Value.ToString() + 
					" and INSTRUMENTNO=" + m_ParamInstrumentNoReference.Value.ToString() + 
					" and STREAMNO=" + m_ParamStreamNoReference.Value.ToString() + 
					" and EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.NominalStep),"ID");
			}
		}
		#endregion RowNominalReference
		#region RowVariationNominal
		private DataRow[] RowVariationNominal
		{
			get
			{
				return DsEvents.DtEvent.Select("IDPARENT=" + m_ParamIdT.Value.ToString() + 
					" and EVENTTYPE=" + DataHelper.SQLString(EventTypeFunc.Nominal) + 
					" and EVENTCODE<>" + DataHelper.SQLString(EventCodeFunc.NominalStep) ,"ID");
			}
		}
		#endregion RowVariationNominal
		#endregion Accessors
		
		#region Constructors
		public EventsValProcessIRD(EventsValProcess pProcess,DataSetTrade pDsTrade,EFS_TradeLibrary pTradeLibrary):base(pProcess, pDsTrade, pTradeLibrary)
		{
			InitParameters();
			m_Parameters = new EventsValParametersIRD();
		}
		#endregion Constructors
		
		#region Methods
		
		#region AddNewRowAccrual
		private Cst.ErrLevel AddNewRowAccrual(DataRow pRow)
		{
			return AddNewRowAccrual(pRow,true,true);
		}
		private Cst.ErrLevel AddNewRowAccrual(DataRow pRow,bool pIsRowLinearAdded,bool pIsRowCompoundAdded)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			try
			{
				if (pIsRowLinearAdded)
				{
					DataRow rowAccrualLinear   = DsEvents.DtEvent.NewRow();
					rowAccrualLinear.ItemArray = (object[])pRow.ItemArray.Clone();
					rowAccrualLinear.BeginEdit();
					rowAccrualLinear["ID"]             = 0;
					rowAccrualLinear["IDPARENT_EVENT"] = pRow["ID"];
					rowAccrualLinear["EVENTCODE"]      = EventCodeFunc.DailyClosing;
					rowAccrualLinear["EVENTTYPE"]      = EventTypeFunc.AccrualLinear;
					rowAccrualLinear["SOURCE"]         = m_EventsValProcess.appInstance.AppName;
					rowAccrualLinear["IDSTTRIGGER"]    = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
					rowAccrualLinear.EndEdit();
					DsEvents.DtEvent.Rows.Add(rowAccrualLinear);
				}

				if (pIsRowCompoundAdded)
				{
					DataRow rowAccrualCompound   = DsEvents.DtEvent.NewRow();
					rowAccrualCompound.ItemArray = (object[])pRow.ItemArray.Clone();
					rowAccrualCompound.BeginEdit();
					rowAccrualCompound["ID"]             = 0;
					rowAccrualCompound["IDPARENT_EVENT"] = pRow["ID"];
					rowAccrualCompound["EVENTCODE"]      = EventCodeFunc.DailyClosing;
					rowAccrualCompound["EVENTTYPE"]      = EventTypeFunc.AccrualCompound;
					rowAccrualCompound["SOURCE"]         = m_EventsValProcess.appInstance.AppName;
					rowAccrualCompound["IDSTTRIGGER"]    = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
					rowAccrualCompound.EndEdit();
					DsEvents.DtEvent.Rows.Add(rowAccrualCompound);
				}
			}
			catch (OTCmlException  otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..AddNewRowAccrual",ex);}
			return ret;
		}
		#endregion AddNewRowAccrual
		#region AddRowAccrual
		private Cst.ErrLevel AddRowAccrual(DataRow pRow,EFS_PaymentEvent pPaymentEvent)
		{
			Cst.ErrLevel ret         = Cst.ErrLevel.SUCCES;
			EFS_DayCountFraction dcf = pPaymentEvent.DayCountFraction;
			bool isCompound          = pPaymentEvent.IsCompounding;
			decimal  nominal         = 0;
			
			#region Read Nominal Period
			DataRow rowNominal = GetCurrentNominal(m_AccrualDate);
			if (false == Convert.IsDBNull(rowNominal["VALORISATION"]))
				nominal = Convert.ToDecimal(rowNominal["VALORISATION"]);
			#endregion Read Nominal Period

			try
			{
				m_ParamIdE.Value      = Convert.ToInt32(pRow["ID"]);
				DataRow[] rowAccruals = RowAccrual;
				if (0 == rowAccruals.Length)
				{
					ret = AddNewRowAccrual(pRow);
				}
				else
				{
					#region Update Row Accrual 
					bool isModifAccrual = false;
					foreach (DataRow row in rowAccruals)
					{
						DataRow[] rowEventsClass = row.GetChildRows(DsEvents.ChildEventClass);
						foreach (DataRow rowEventClass in rowEventsClass)
						{
							if (m_AccrualDate == Convert.ToDateTime(rowEventClass["DTEVENTFORCED"]))
							{
								isModifAccrual = true;
								row["VALORISATION"] = pRow["VALORISATION"];
								break;
							}
						}
					}
					if (false == isModifAccrual)
						AddNewRowAccrual(pRow);

					#endregion Update Row Accrual 
				}

				if (null != DsEvents.DtEvent.GetChanges())
				{
					rowAccruals = RowAccrual;
					if (null != rowAccruals)
					{
						foreach (DataRow row in rowAccruals)
						{
							if (DataRowState.Modified == row.RowState)
							{
								for (int i=0;i<100;i++)
								{
									System.Diagnostics.Debug.Write(row[i].ToString());
									System.Diagnostics.Debug.Write(row[i,DataRowVersion.Original].ToString());
								}
							}
							if (DataRowState.Unchanged != row.RowState)
								CalculAmountAccrual(row,nominal,dcf,isCompound);
						}
					}
					#region Cancel Payment & CalculationPeriod Edit
					DataRow[] rowChilds = pRow.GetChildRows(DsEvents.ChildEvent);
					foreach (DataRow rowChild in rowChilds)
					{
						if (EventCodeFunc.IsCalculationPeriod(rowChild["EVENTCODE"].ToString()))
							rowChild.RejectChanges();
					}
					pRow.CancelEdit();
					#endregion Cancel Payment & CalculationPeriod Edit
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..AddRowAccrual",ex);}
			return ret;
		}
		#endregion AddRowAccrual
		#region CalculAmountAccrual
		private Cst.ErrLevel CalculAmountAccrual(DataRow pRowAccrual,decimal pNominal,EFS_DayCountFraction pDcf,bool pIsCompound)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			try
			{
				#region Accrued Interest Parameters calculated
				DateTime startDate   = Convert.ToDateTime(pRowAccrual["DTSTARTADJ"]);
				DateTime endDate     = Convert.ToDateTime(pRowAccrual["DTENDADJ"]);
				decimal nativeAmount = Convert.ToDecimal(pRowAccrual["VALORISATION"]);
				string currency      = pRowAccrual["UNIT"].ToString();
				#endregion Accrued Interest Parameters calculated

				#region DayCountFraction
				EFS_DayCountFraction dcfTotal           = new EFS_DayCountFraction(startDate,endDate,pDcf.DayCountFraction);
				EFS_DayCountFraction dcfAccrued         = new EFS_DayCountFraction(startDate,m_AccrualDate,pDcf.DayCountFraction);
				decimal totalDays = 0;
				if (null != dcfTotal)
					totalDays = (dcfTotal.NumberOfCalendarYears + (dcfTotal.Numerator / dcfTotal.Denominator));
				decimal accruedDays = 0;
				if (null != dcfAccrued)
					accruedDays = (dcfAccrued.NumberOfCalendarYears + (dcfAccrued.Numerator / dcfAccrued.Denominator));
				#endregion DayCountFraction

				#region Amount
				decimal linearAmount     = 0;
				decimal compoundAmount   = 0;
				decimal fullCouponAmount = 0;
				#endregion Amount
				#region Rate
				decimal nativeRate       = 0;
				decimal linearRate       = 0;
				decimal compoundRate     = 0;
				#endregion Rate

				#region Accruals Amounts and Rates Calculation

				#region Native rate / Equivalent Rate (Compound and simple rate calculation)
				EFS_EquivalentRate equivalentRate = null;
				decimal x = 0;
				decimal y = 0;
				if (pIsCompound)
				{
					#region Compound Rate
					x            = (nativeAmount / pNominal) + 1;
					y            = pDcf.Denominator / ((pDcf.Denominator * pDcf.NumberOfCalendarYears) + pDcf.Numerator);
					compoundRate = Convert.ToDecimal(Math.Pow(Convert.ToDouble(x) , Convert.ToDouble(y)) - 1);
					#endregion Compound Rate
					#region Simple rate
					equivalentRate = new EFS_EquivalentRate(EquiRateMethodEnum.CompoundToSimple,startDate,endDate,compoundRate,pDcf.DayCountFraction);
					#endregion Simple rate
					nativeRate = equivalentRate.compoundRate;
				}
				else
				{
					#region Simple Rate
					y              = pDcf.NumberOfCalendarYears + (pDcf.Numerator / pDcf.Denominator);
					linearRate     = nativeAmount / (pNominal * y);
					#endregion Simple Rate
					#region Compound Rate
					equivalentRate = new EFS_EquivalentRate(EquiRateMethodEnum.SimpleToCompound,startDate,endDate,
						linearRate,pDcf.DayCountFraction,DayCountFractionEnum.ACTACTISDA.ToString());
					#endregion Compound Rate
					nativeRate = Math.Round(equivalentRate.simpleRate,9);
				}
				#endregion Native rate / Equivalent Rate (Compound and simple rate calculation)

				#region FullCoupon amount
				EFS_CalculAmount calculAmount = new EFS_CalculAmount(pNominal,linearRate,startDate,endDate,pDcf.DayCountFraction);
				fullCouponAmount              = RoundingCurrencyAmount(currency,calculAmount.calculatedAmount);
				#endregion FullCoupon amount

				#region Linear result
				linearRate   = equivalentRate.simpleRate;
				linearAmount = RoundingCurrencyAmount(currency,(fullCouponAmount * accruedDays / totalDays));
				#endregion Linear result
				#region Actuarial result
				compoundRate   = equivalentRate.compoundRate;
				compoundAmount = pNominal * (Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + compoundRate),Convert.ToDouble(accruedDays))) - 1);
				compoundAmount = RoundingCurrencyAmount(currency,compoundAmount);
				#endregion Actuarial result
				#endregion Accruals Amounts and Rates Calculation


				#region DataRow Updated And Inserted
				int idE          = Convert.ToInt32(pRowAccrual["ID"]);
				m_ParamIdE.Value = idE;
				DataRow rowAccrualDetail = RowEventDetail;
				if (null == rowAccrualDetail) 
				{
					rowAccrualDetail = DtEventDetails.NewRow();
					DtEventDetails.Rows.Add(rowAccrualDetail);
				}
				if (0 == idE)
					ret = SQLUP.GetId(out idE,m_ConnectionString,SQLUP.IdGetId.EVENT,SQLUP.PosRetGetId.First,1);

				if (Cst.ErrLevel.SUCCES == ret)
				{
					pRowAccrual.BeginEdit();
					pRowAccrual["ID"]         = idE;
					pRowAccrual["DTENDADJ"]   = m_AccrualDate;
					pRowAccrual["DTENDUNADJ"] = m_AccrualDate;
					#region Accrual Event & EventDet
					rowAccrualDetail.BeginEdit();
					rowAccrualDetail["ID"]          = idE;
					rowAccrualDetail["DCF"]         = dcfAccrued.DayCountFraction_FpML;
					rowAccrualDetail["DCFNUM"]      = dcfAccrued.Numerator;
					rowAccrualDetail["DCFDEN"]      = dcfAccrued.Denominator;
					rowAccrualDetail["TOTALOFYEAR"] = dcfAccrued.NumberOfCalendarYears;
					rowAccrualDetail["TOTALOFDAY"]  = dcfAccrued.TotalNumberOfCalculatedDays;

					string eventType = pRowAccrual["EVENTTYPE"].ToString();
					if (EventTypeFunc.IsAccrualLinear(eventType))
					{
						pRowAccrual["VALORISATION"]    = linearAmount;
						pRowAccrual["VALORISATIONSYS"] = linearAmount;
						rowAccrualDetail["RATE"]       = linearRate;
					}
					else if (EventTypeFunc.IsAccrualCompound(eventType))
					{
						pRowAccrual["VALORISATION"]     = compoundAmount;
						pRowAccrual["VALORISATIONSYS"]  = compoundAmount;
						rowAccrualDetail["RATE"]        = compoundRate;
					}
					pRowAccrual.EndEdit();
					SetRowStatus(pRowAccrual, Tuning.TuningOutputTypeEnum.OES);    
					rowAccrualDetail.EndEdit();
					#endregion Accrual Event & EventDet
					#region Accrual EventClass
					DataRow rowAccrualClass = RowEventClass;
					if (null == rowAccrualClass) 
					{
						rowAccrualClass                  = DsEvents.DtEventClass.NewRow();
						rowAccrualClass.BeginEdit();
						rowAccrualClass["IDPARENT"]      = idE;
						rowAccrualClass["EVENTCLASS"]    = EventCodeFunc.DailyClosing;
						rowAccrualClass.EndEdit();
						DsEvents.DtEventClass.Rows.Add(rowAccrualClass);
					}
					rowAccrualClass.BeginEdit();
					rowAccrualClass["DTEVENT"]       = m_AccrualDate;
					rowAccrualClass["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_ConnectionString,m_AccrualDate);
					rowAccrualClass.EndEdit();
					#endregion Accrual EventClass

					#endregion DataRow Updated And Inserted
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..CalculAmountAccrual",ex);}
			return ret;
		}
		#endregion CalculAmountAccrual
		#region CurrentNominalReference
		public DataRow CurrentNominalReference(DateTime pDate)
		{
			try
			{
				foreach (DataRow rowNominal in RowNominalReference)
				{
					if (( Convert.ToDateTime(rowNominal["DTSTARTADJ"])<=pDate) && (pDate < Convert.ToDateTime(rowNominal["DTENDADJ"])))
						return rowNominal;
				}
				int nbRow = RowNominalReference.Length;
				if (0 < nbRow)
				{
					DataRow rowTerminationNominal = RowNominalReference[nbRow-1];
					if (pDate == Convert.ToDateTime(rowTerminationNominal["DTENDADJ"]))
						return rowTerminationNominal;
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..CurrentNominalReference",ex);}
			return null;
		}
		#endregion CurrentNominalReference
		#region FixedRateAndDCFFromRowInterest
		public Cst.ErrLevel FixedRateAndDCFFromRowInterest(DateTime pDate,out decimal pFixedRate,out string pDayCountFraction)
		{
			Cst.ErrLevel ret	= Cst.ErrLevel.DATANOTFOUND;
			try
			{
				pFixedRate			= 0;
				pDayCountFraction	= string.Empty;
				foreach (DataRow rowInterest in RowInterest)
				{
					if (( Convert.ToDateTime(rowInterest["DTSTARTADJ"])<=pDate) && (pDate < Convert.ToDateTime(rowInterest["DTENDADJ"])))
					{
						DataRow[] rowCalcPeriods = rowInterest.GetChildRows(DsEvents.ChildEvent);
						foreach (DataRow rowCalcPeriod in rowCalcPeriods)
						{
							if (EventTypeFunc.IsFixedRate(rowCalcPeriod["EVENTTYPE"].ToString()))
							{
								DataRow rowDetail = GetRowDetail(Convert.ToInt32(rowCalcPeriod["ID"]));
								if (null != rowDetail)
								{
									pFixedRate		  = Convert.ToDecimal(rowDetail["RATE"]);
									pDayCountFraction = rowDetail["DCF"].ToString();
								}
								ret = Cst.ErrLevel.SUCCES;
								break;
							}
						}
						break;
					}
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..FixedRateAndDCFFromRowInterest",ex);}
			return ret;
		}
		#endregion FixedRateAndDCFFromRowInterest
		#region InitParameters
		protected new void InitParameters()
		{
			m_ParamInstrumentNoReference	= new EFSParameter(m_ConnectionString, "INSTRUMENTNO", DbType.Int32).DataParameter;
			m_ParamStreamNoReference		= new EFSParameter(m_ConnectionString, "STREAMNO", DbType.Int32).DataParameter;
		}
		#endregion InitParameters
		#region IsRowHasChildrens
		private bool IsRowHasChildrens(DataRow pRow)
		{
			try
			{
				#region RowChildrens Reader
				DataRow[] rowChilds = pRow.GetChildRows(DsEvents.ChildEvent);
				foreach (DataRow rowChild in rowChilds)
				{
					if (IsRowMustBeCalculate(rowChild))
						return true;
				}
				#endregion RowChildrens Reader
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..IsRowHasChildrens",ex);}
			return false;
		}
		#endregion IsRowHasChildrens
		#region IsRowHasNominalChanged
		private  bool IsRowHasNominalChanged(DataRow pRow)
		{
			try
			{
				DataRow rowNominal = GetCurrentNominal(Convert.ToDateTime(pRow["DTSTARTADJ"]));
				DataRow[] rowAssets  = GetRowAsset(Convert.ToInt32(rowNominal["ID"]));
				if ((null != rowAssets) && (0 < rowAssets.Length))
				{
					DataRow rowAsset = rowAssets[0];
					if (null != rowNominal)
					{
						if ((null != m_Quote_FxRate) && 
							(m_Quote_FxRate.time <= Convert.ToDateTime(rowNominal["DTSTARTADJ"])) &&
							(m_Quote_FxRate.idAsset == Convert.ToInt32(rowAsset["IDASSET"])))
							return true;
					}
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..IsRowHasNominalChanged",ex);}
			return false;
		}
		#endregion IsRowHasNominalChanged
		
		#region IsRowMustBeCalculate
		public override bool IsRowMustBeCalculate(DataRow pRow)
		{
			try
			{
				DateTime startDate = Convert.ToDateTime(pRow["DTSTARTUNADJ"]);
				DateTime endDate   = Convert.ToDateTime(pRow["DTENDUNADJ"]);
				string eventCode   = pRow["EVENTCODE"].ToString();
				string eventType   = pRow["EVENTTYPE"].ToString();
				//
				if (null != m_EventsValMQueue.quote)
				{
					#region AccrualDate is in period
					if (false == DtFunc.IsDateTimeEmpty(m_AccrualDate))
					{
						if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode))
						{
							if ((m_AccrualDate <= startDate) || (m_AccrualDate > endDate))
								return false;
						}
						else if (EventCodeFunc.IsDailyClosing(eventCode))
							return false;
					}
					#endregion AccrualDate is in period

					if (null != m_Quote_RateIndex)
					{
						if (EventCodeFunc.IsReset(eventCode) || EventCodeFunc.IsSelfReset(eventCode))
						{
							if (IsRowHasFixingEvent(pRow))
								return true;
							else
								return IsRowHasChildrens(pRow);
						}
						else
							return IsRowHasChildrens(pRow);

					}
					else if (null != m_Quote_FxRate)
					{
						if (EventCodeAndEventTypeFunc.IsNominalPeriodVariation(eventCode,eventType))
						{
							if (IsRowHasFixingEvent(pRow))
								return true;
							else
								return IsRowHasNominalChanged(pRow);
						}
						else
						{
							if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode))
								return IsRowHasNominalChanged(pRow);
							else
								return true;
						}
					}
				}
				else if (false == DtFunc.IsDateTimeEmpty(m_AccrualDate))
				{
					#region AccrualDate is in period
					if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode))
					{
						if ((m_AccrualDate <= startDate) || (m_AccrualDate > endDate))
							return false;
					}
					else if (EventCodeFunc.IsDailyClosing(eventCode))
						return false;
					return true;
					#endregion AccrualDate is in period
				}
				else
					return true;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..IsRowMustBeCalculate",ex);}
			return false;
		}
		#endregion IsRowMustBeCalculate
		#region IsRowsEventCalculated
		public  override bool IsRowsEventCalculated(DataRow[] pRows)
		{
			foreach (DataRow row in pRows)
			{
				DateTime startDate = Convert.ToDateTime(row["DTSTARTUNADJ"]);
				DateTime endDate   = Convert.ToDateTime(row["DTENDUNADJ"]);

				if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
				{
					if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
						return false;
					else if ((m_AccrualDate > startDate) && (m_AccrualDate <= endDate))
					{
						DataRow[] rowChilds = row.GetChildRows(DsEvents.ChildEvent);
						if (0 != rowChilds.Length)
							return IsRowsEventCalculated(rowChilds);
						return false;
					}
				}
			}
			return true;
		}
		#endregion IsRowsEventCalculated
		
		#region public Valorize
		/// <revision>
		///     <build>23</build><date>20050808</date><author>PL</author>
		///     <comment>
		///     Add CancelEdit()
		///     </comment>
		/// </revision>
		public override Cst.ErrLevel Valorize(ref ArrayList pOTCmlException)
		{
			try
			{
				Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
				pOTCmlException = new ArrayList();
				#region Nominal Process (FxLinkedNotional Case)
				bool isError              = false; 
				bool isRowMustBeCalculate = false; 
				bool isRowHasFixingEvent  = false; 
				foreach (DataRow rowVariation in RowVariationNominal)
				{
					isError				      = false;
					isRowHasFixingEvent       = false; 
					m_ParamInstrumentNo.Value = Convert.ToInt32(rowVariation["INSTRUMENTNO"]);
					m_ParamStreamNo.Value     = Convert.ToInt32(rowVariation["STREAMNO"]);
					EFS_VariationNominalEvent variationNominalEvent;
					try
					{
						Parameters.Add(m_ConnectionString,m_tradeLibrary,rowVariation);
						// ScanCompatibility_Event
						isRowMustBeCalculate = (Cst.ErrLevel.SUCCES == 
							m_EventsValProcess.ScanCompatibility_Event(Convert.ToInt32(rowVariation["ID"])));
						// isRowMustBeCalculate
						isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowVariation);
						//
						if (isRowMustBeCalculate)
						{
							isRowHasFixingEvent = IsRowHasFixingEvent(rowVariation); 
							if (isRowHasFixingEvent)
							{
								rowVariation.BeginEdit();
								int instrumentNoReference	= 0;
								int streamNoReference		= 0;
								EventsValParameterIRD parameter  = Parameters[paramInstrumentNo,paramStreamNo];
								parameter.GetStreamNotionalReference(out instrumentNoReference,out streamNoReference);
								m_ParamInstrumentNoReference.Value = instrumentNoReference;
								m_ParamStreamNoReference.Value     = streamNoReference;
								variationNominalEvent = new EFS_VariationNominalEvent(m_AccrualDate,this,rowVariation);
							}
						}
					}
					catch (OTCmlException otcmlException)
					{
						isError = true;
						if (otcmlException.IsLevelAlert)
						{
							ret =  Cst.ErrLevel.MISCELLANEOUS;
							pOTCmlException.Add(otcmlException);
						}
					}
					catch (Exception ex) 
					{
						isError = true;
						ret =  Cst.ErrLevel.BUG;
						throw new OTCmlException("EventsValProcessIRD.Valorize (RowVariation)", ex);
					}
					finally
					{
						bool cancelEdit = true;
						//
						if (isRowMustBeCalculate)
						{
							if (isRowHasFixingEvent)
							{
								cancelEdit = false;
								rowVariation.EndEdit();
								//EventProcess
								int idE = Convert.ToInt32(rowVariation["ID"]);
								ret     = Update(idE,isError);
							}
						}
						//
						if (cancelEdit)//20050808 PL Add
							rowVariation.CancelEdit();
						
					}
				}
				#endregion Nominal Process (FxLinkedNotional Case)
				#region Payment Process
				foreach (DataRow rowInterest in RowInterest)
				{
					isError                   = false;
					m_ParamInstrumentNo.Value = Convert.ToInt32(rowInterest["INSTRUMENTNO"]);
					m_ParamStreamNo.Value     = Convert.ToInt32(rowInterest["STREAMNO"]);
					
					// ScanCompatibility_Event
					isRowMustBeCalculate = (Cst.ErrLevel.SUCCES == 
						m_EventsValProcess.ScanCompatibility_Event(Convert.ToInt32(rowInterest["ID"])));
					// isRowMustBeCalculate
					isRowMustBeCalculate  = isRowMustBeCalculate && IsRowMustBeCalculate(rowInterest);
					//
					if (isRowMustBeCalculate)
					{
						EFS_PaymentEvent paymentEvent = null;
						try
						{
							Parameters.Add(m_ConnectionString,m_tradeLibrary,rowInterest);
							rowInterest.BeginEdit();
							paymentEvent = new EFS_PaymentEvent(m_AccrualDate,this,rowInterest);
						}
						catch (OTCmlException otcmlException)
						{
							isError = true;
							if (otcmlException.IsLevelAlert)
							{
								ret =  Cst.ErrLevel.MISCELLANEOUS;
								pOTCmlException.Add(otcmlException);
							}
						}
						catch (Exception ex) 
						{
							isError = true;
							throw new OTCmlException("EventsValProcessIRD.Valorize (RowInterest)",ex);
						}
						finally
						{
							if (null != paymentEvent)
							{
								if (paymentEvent.endDate == Convert.ToDateTime(rowInterest["DTENDADJ"]))
									rowInterest.EndEdit();
								else
									AddRowAccrual(rowInterest,paymentEvent);
							}
							int idE = Convert.ToInt32(rowInterest["ID"]);
							Update(idE,isError); // Commit 
						}
					}
				}
				#endregion Payment Process
				//
				return ret;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw new OTCmlException("EventsValProcessIRD.Valorize", ex);}
		}	
		#endregion Valorize
		#region TreatedRate
		public static decimal TreatedRate(RateTreatmentEnum pRateTreatment,decimal pObservedRate,DateTime pStartDate,DateTime pEndDate,Interval pPaymentFrequency)
		{
			decimal treatedRate = pObservedRate;
			try
			{
				#region RateTreatment process
				DateTime endDate      = pEndDate;
				EFS_Interval interval = new EFS_Interval(pPaymentFrequency,pStartDate,Convert.ToDateTime(null));
				TimeSpan timeSpan     = interval.offsetDate - endDate;
				if (Math.Abs(timeSpan.Days) > 7)
					endDate = interval.offsetDate;

				EFS_EquivalentYieldForADiscountRate equivalentYieldForADiscountRate = 
					new EFS_EquivalentYieldForADiscountRate(pRateTreatment,pObservedRate,pStartDate,endDate);
				treatedRate = equivalentYieldForADiscountRate.treatedRate;
				#endregion RateTreatment process
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationIRD..TreatedRate",ex);}		
			return treatedRate;
		}
		#endregion TreatedRate
		#endregion Methods
	}
	#endregion EventsValIRD
	#region EventsValParameterIRD
	public class EventsValParameterIRD : EventsValParameterBase
	{
		#region Variables
		private SQL_AssetRateIndex m_Rate;
		private SQL_AssetRateIndex m_Rate2;
		private SQL_AssetRateIndex m_RateBasis;
		#endregion Variables
		#region Accessors
		#region DCFRateBasis
		public string DCFRateBasis
		{
			get{return m_Rate.FirstRow["DCFBASISRATE"].ToString();}
		}
		#endregion DCFRateBasis
		#region FinalRateRounding
		public Rounding FinalRateRounding
		{
			get
			{
				InterestRateStream stream = Stream;
				if (null != stream)
				{
					if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
						stream.calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified)
					{
						FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
						if (calculation.rateFloatingRate.finalRateRoundingSpecified)
							return calculation.rateFloatingRate.finalRateRounding;
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
				if (m_Product.GetType().Equals(typeof(FpML.Ird.Fra)))
					return ((FpML.Ird.Fra)m_Product).dayCountFraction.ToString();
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
				if (m_Product.GetType().Equals(typeof(FpML.Ird.Fra)))
					return ((FpML.Ird.Fra)m_Product).fraDiscounting;
				else
					return FraDiscountingEnum.NONE;
			}
		}
		#endregion FraDiscounting
		#region FraFixedRate
		public decimal FraFixedRate
		{
			get
			{
				if (m_Product.GetType().Equals(typeof(FpML.Ird.Fra)))
					return ((FpML.Ird.Fra)m_Product).fixedRate.DecValue;
				else
					return 0;
			}
		}
		#endregion FraFixedRate
		#region FraNotional
		public decimal FraNotional
		{
			get
			{
				if (m_Product.GetType().Equals(typeof(FpML.Ird.Fra)))
					return ((FpML.Ird.Fra)m_Product).notional.amount.DecValue;
				else
					return 0;
			}
		}
		#endregion FraNotional
		#region Rate
		public SQL_AssetRateIndex Rate
		{
			set {m_Rate = value;}
			get {return m_Rate;}
		}
		#endregion Rate
		#region Rate2
		public SQL_AssetRateIndex Rate2
		{
			set {m_Rate2 = value;}
			get {return m_Rate2;}
		}
		#endregion Rate2
		#region RateBasis
		public SQL_AssetRateIndex RateBasis
		{
			set {m_RateBasis = value;}
			get {return m_RateBasis;}
		}
		#endregion RateBasis
		#region Payment Frequency
		public Interval PaymentFrequency
		{
			get 
			{
				if (m_Product.GetType().Equals(typeof(FpML.Ird.Fra)))
					return ((FpML.Ird.Fra)m_Product).indexTenor;
				else
					return Stream.paymentDates.paymentFrequency;
			}
		}
		#endregion Payment Frequency
		#region SelfAveragingMethod
		public AveragingMethodEnum SelfAveragingMethod
		{
			get
			{
				if (null != m_Rate)
				{
					string averagingMethod = m_Rate.FirstRow["SELFAVGMETHOD"].ToString();
					if (System.Enum.IsDefined(typeof(AveragingMethodEnum),averagingMethod))
						return (AveragingMethodEnum) System.Enum.Parse(typeof(AveragingMethodEnum),averagingMethod,true);
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
					if (System.Enum.IsDefined(typeof(CompoundingMethodEnum),compoundingMethod))
						return (CompoundingMethodEnum) System.Enum.Parse(typeof(CompoundingMethodEnum),compoundingMethod,true);
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
				{
					return m_Rate.Idx_DayCountFraction;
				}
				return null;
			}
		}
		#endregion SelfDayCountFraction
		#region Stream
		public InterestRateStream Stream
		{
			get
			{
				if ((null != m_Product) && 0 != streamNo)
				{
					object streams = null;
					Type tProduct = m_Product.GetType();
					PropertyInfo pty = tProduct.GetProperty("Stream");
					if (null != pty)
						streams = tProduct.InvokeMember(pty.Name,BindingFlags.GetProperty,null,m_Product,null);
					if (null != streams)
					{
						if (streams.GetType().Equals(typeof(FpML.Ird.InterestRateStream[])))
							return (InterestRateStream)((FpML.Ird.InterestRateStream[])streams).GetValue(streamNo-1);
						else if (streams.GetType().Equals(typeof(FpML.Ird.InterestRateStream)))
							return (InterestRateStream) streams;
					}
				}
				return null;
			}
		}
		#endregion Stream
		#endregion Accessors
		#region Constructors
		public EventsValParameterIRD(string pConnectionString,EFS_TradeLibrary pTradeLibrary,int pInstrumentNo,int pStreamNo)
			:base(pConnectionString,pTradeLibrary,pInstrumentNo,pStreamNo){}
		#endregion Constructors
		#region Methods
		#region AveragingMethod
		public Cst.ErrLevel AveragingMethod(out AveragingMethodEnum pAveragingMethod)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			InterestRateStream stream = Stream;
			pAveragingMethod = AveragingMethodEnum.Unweighted;
			if (null != stream)
			{
				if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
					stream.calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified)
				{
					FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
					if (calculation.rateFloatingRate.averagingMethodSpecified)
					{
						pAveragingMethod = calculation.rateFloatingRate.averagingMethod;
						ret = Cst.ErrLevel.SUCCES;
					}
				}
			}
			return ret;
		}
		#endregion AveragingMethod
		#region CompoundingMethod
		public Cst.ErrLevel CompoundingMethod(out CompoundingMethodEnum pCompoundingMethod)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			pCompoundingMethod = CompoundingMethodEnum.None;
			try
			{
				InterestRateStream stream = Stream;
				if (null != stream)
				{
					if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
						stream.calculationPeriodAmount.calculationPeriodAmountCalculation.compoundingMethodSpecified)
					{
						FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
						pCompoundingMethod = calculation.compoundingMethod;
						ret = Cst.ErrLevel.SUCCES;
					}
				}
			}
			catch
			{
				ret = Cst.ErrLevel.DATANOTFOUND;
			}
			return ret;
		}
		#endregion CompoundingMethod
		#region Discounting
		public Cst.ErrLevel Discounting(out Discounting pDiscounting)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			pDiscounting = null;
			try
			{
				InterestRateStream stream = Stream;
				if (null != stream)
				{
					if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
						stream.calculationPeriodAmount.calculationPeriodAmountCalculation.discountingSpecified)
					{
						FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
						pDiscounting = calculation.discounting;
						ret = Cst.ErrLevel.SUCCES;
					}
				}
			}
			catch
			{
				ret = Cst.ErrLevel.DATANOTFOUND;
			}
			return ret;
		}
		#endregion Discounting
		#region GetRateSpreadAndMultiplier
		public Cst.ErrLevel GetRateSpreadAndMultiplier(DateTime pStartDate,DateTime pEndDate,out decimal pMultiplier,out decimal pSpread)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			try
			{
				InterestRateStream stream = Stream;
				pMultiplier = 1;
				pSpread = 0;
				if (null != stream)
				{
					if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
						stream.calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified)
					{
						FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
						#region Spread
						if (calculation.rateFloatingRate.spreadScheduleSpecified)
							pSpread = Tools.GetStepValue(calculation.rateFloatingRate.spreadSchedule,pStartDate,pEndDate);
						#endregion Spread
						#region Multiplier
						if (calculation.rateFloatingRate.floatingRateMultiplierScheduleSpecified)
							pMultiplier = Tools.GetStepValue(calculation.rateFloatingRate.floatingRateMultiplierSchedule,
								pStartDate,pEndDate);
						#endregion Multiplier
					}
				}
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationParameterIRD..GetRateSpreadAndMultiplier",ex);}
			return ret;
		}
		#endregion GetRateSpreadAndMultiplier
		#region GetStreamNotionalReference
		public Cst.ErrLevel GetStreamNotionalReference(out int pInstrumentNo,out int pStreamNo)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			try
			{
				pInstrumentNo	= 0;
				pStreamNo		= 0;
				InterestRateStream stream = Stream;
				if (null != stream)
				{
					if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified &&
						stream.calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotionalSpecified)
					{
						FxLinkedNotionalSchedule fxLinkedNotionalSchedule = 
							stream.calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional;
						string hRef = fxLinkedNotionalSchedule.constantNotionalScheduleReference.href.ToString();
						int instrumentNo	= 0;
						int streamNo		= 0;
						object notionalReference = EFS_Current.GetObjectById(hRef,ref instrumentNo,ref streamNo);
						if (notionalReference.GetType().Equals(typeof(Notional)))
						{
							pInstrumentNo	= instrumentNo;
							pStreamNo		= streamNo;
							ret				= Cst.ErrLevel.SUCCES;
						}
					}
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationParameterIRD..GetStreamNotionalReference",ex);}
			return ret;
		}
		#endregion GetStreamNotionalReference
		#region GetStrikeSchedule
		public Cst.ErrLevel GetStrikeSchedule(string pEventType,out StrikeSchedule[] pStrikeSchedules)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			try
			{
				InterestRateStream stream = Stream;
				pStrikeSchedules = null;
				if (null != stream)
				{
					if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
						stream.calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified)
					{
						FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
						#region CapRateSchedule
						if (calculation.rateFloatingRate.capRateScheduleSpecified && pEventType.StartsWith("CA"))
							pStrikeSchedules = calculation.rateFloatingRate.capRateSchedule;
							#endregion CapRateSchedule
							#region FloorRateSchedule
						else if (calculation.rateFloatingRate.floorRateScheduleSpecified && pEventType.StartsWith("FL"))
							pStrikeSchedules = calculation.rateFloatingRate.floorRateSchedule;
						#endregion FloorRateSchedule
					}
				}
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationParameterIRD..GetStrikeSchedule",ex);}
			return ret;
		}
		#endregion GetStrikeSchedule
		#region NegativeInterestRateTreatment
		public Cst.ErrLevel NegativeInterestRateTreatment(out NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			InterestRateStream stream = Stream;
			pNegativeInterestRateTreatment = NegativeInterestRateTreatmentEnum.NegativeInterestRateMethod;
			if (null != stream)
			{
				if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
					stream.calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified)
				{
					FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
					if (calculation.rateFloatingRate.negativeInterestRateTreatmentSpecified)
					{
						pNegativeInterestRateTreatment = calculation.rateFloatingRate.negativeInterestRateTreatment;
						ret = Cst.ErrLevel.SUCCES;
					}
				}
			}
			return ret;
		}
		#endregion RateTreatment
		#region RateTreatment
		public Cst.ErrLevel RateTreatment(out RateTreatmentEnum pRateTreatment)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;
			InterestRateStream stream = Stream;
			pRateTreatment = RateTreatmentEnum.BondEquivalentYield;
			if (null != stream)
			{
				if (stream.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified && 
					stream.calculationPeriodAmount.calculationPeriodAmountCalculation.rateFloatingRateSpecified)
				{
					FpML.Ird.Calculation calculation = stream.calculationPeriodAmount.calculationPeriodAmountCalculation;
					if (calculation.rateFloatingRate.rateTreatmentSpecified)
					{
						pRateTreatment = calculation.rateFloatingRate.rateTreatment;
						ret = Cst.ErrLevel.SUCCES;
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
			if (System.Enum.IsDefined(typeof(RateTreatmentEnum),m_RateBasis.FirstRow["Idx_RATETREATMENT"].ToString()))
			{
				ret = Cst.ErrLevel.SUCCES;
				pRateTreatment = (RateTreatmentEnum) System.Enum.Parse(typeof(RateTreatmentEnum),
					m_RateBasis.FirstRow["Idx_RATETREATMENT"].ToString(),true);
			}
			return ret;
		}
		#endregion RateTreatmentRateBasis
		#endregion Methods
	}
	#endregion EventsValParameterIRD
	#region EventsValParametersIRD
	public class EventsValParametersIRD
	{
		#region Variables
		public EventsValParameterIRD[] eventsValParameters;
		#endregion Variables
		#region Constructors
		public EventsValParametersIRD(){}
		#endregion Constructors
		#region Methods
		#region Add
		public void Add(string pConnnectionString,EFS_TradeLibrary pTradeLibrary,DataRow pRow)
		{
			ArrayList aEventsValParameters = new ArrayList();
			int instrumentNo               = Convert.ToInt32(pRow["INSTRUMENTNO"]);
			int streamNo                   = Convert.ToInt32(pRow["STREAMNO"]);
			if (null == this[instrumentNo,streamNo])
			{
				if (null != eventsValParameters)
				{
					for (int i=0;i<eventsValParameters.Length;i++)
					{
						aEventsValParameters.Add(eventsValParameters[i]);
					}
				}
				aEventsValParameters.Add(new EventsValParameterIRD(pConnnectionString,pTradeLibrary,instrumentNo,streamNo));
				eventsValParameters = new EventsValParameterIRD[aEventsValParameters.Count];
				eventsValParameters = (EventsValParameterIRD[])aEventsValParameters.ToArray(typeof(EventsValParameterIRD));
			}
		}
		#endregion Add
		#endregion Methods
		#region Indexors
		public EventsValParameterIRD this[int pInstrumentNo,int pStreamNo]
		{
			get 
			{
				if (null != eventsValParameters)
				{
					for (int i=0;i<eventsValParameters.Length;i++)
					{
						if ((pInstrumentNo == eventsValParameters[i].instrumentNo) && (pStreamNo == eventsValParameters[i].streamNo))
							return eventsValParameters[i];
					}
				}
				return null;
			}
		}
		#endregion Indexors

	}
	#endregion EventsValParametersIRD

	#region EFS_Averaging
	public class EFS_Averaging
	{
		private DateTime            m_StartDate;
		private DateTime            m_EndDate;
		private Type                m_tObservedArgs;
		private object[]            m_ObservedArgs;
		private AveragingMethodEnum m_AveragingMethod;
		private string              m_DayCountFraction;

		public Decimal              averagedRate;


		#region Accessors
		public object[] observedArgs
		{
			get {return m_ObservedArgs;}
			set 
			{
				m_ObservedArgs  = value;
				m_tObservedArgs = m_ObservedArgs.GetType().GetElementType();
			}
		}
		#endregion Accessors
		#region Constructors
		public EFS_Averaging(SelfAveragingInfo pAveragingInfo,object[] pObservedArgs)
			: this (pAveragingInfo.averagingMethod,pAveragingInfo.startDate,pAveragingInfo.endDate,
			pObservedArgs,pAveragingInfo.dayCountFraction){}

		public EFS_Averaging(CalculationPeriodInfo pCalcPeriodInfo,object[] pObservedArgs)
			: this (pCalcPeriodInfo.averagingMethod,pCalcPeriodInfo.startDate,pCalcPeriodInfo.endDate,
			pObservedArgs,pCalcPeriodInfo.dayCountFraction){}

		public EFS_Averaging(FpML.Enum.AveragingMethodEnum pAveragingMethod,
			DateTime pStartDate,DateTime pEndDate,object[] pObservedArgs,string pDayCountFraction)
		{
			m_AveragingMethod  = pAveragingMethod;
			m_StartDate        = pStartDate;
			m_EndDate          = pEndDate;
			observedArgs       = pObservedArgs;
			m_DayCountFraction = pDayCountFraction;
			Calc();
		}

		public EFS_Averaging(FpML.Enum.AveragingMethodEnum pAveragingMethod,object[] pObservedArgs)
		{
			m_AveragingMethod = pAveragingMethod;
			observedArgs      = pObservedArgs;
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
		private DateTime ArgDate(int pIndex)
		{
			if (m_tObservedArgs.Equals(typeof(EFS_ResetEvent)))
				return ((EFS_ResetEvent)m_ObservedArgs[pIndex]).resetDate;
			else if (m_tObservedArgs.Equals(typeof(EFS_SelfResetEvent)))
				return ((EFS_SelfResetEvent)m_ObservedArgs[pIndex]).fixingDate;
			else if (m_tObservedArgs.Equals(typeof(EFS_ObservedValue)))
				return ((EFS_ObservedValue)m_ObservedArgs[pIndex]).date;
			return Convert.ToDateTime(null);
		}
		#endregion ArgDate
		#region Calc
		private Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				switch (m_AveragingMethod)
				{
					case FpML.Enum.AveragingMethodEnum.Unweighted:
						ret = UnWeightedMethod();
						break;
					case FpML.Enum.AveragingMethodEnum.Weighted:
						ret = WeightedMethod();
						break;
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#region WeightedMethod
		private Cst.ErrLevel WeightedMethod()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				if (0 < m_ObservedArgs.Length)
				{
					EFS_DayCountFraction dcf;
					int totalNbDays = 0;
					decimal weightedRate = 0;
					for (int i=1;i<m_ObservedArgs.Length-1;i++)
					{

						dcf = new EFS_DayCountFraction(ArgDate(i-1),ArgDate(i),m_DayCountFraction);
						totalNbDays += dcf.TotalNumberOfCalculatedDays;
						weightedRate += (dcf.TotalNumberOfCalculatedDays * ArgValue(i-1));
					}
					#region LastSource
					dcf = new EFS_DayCountFraction(ArgDate(m_ObservedArgs.Length-1),m_EndDate,m_DayCountFraction);
					totalNbDays += dcf.TotalNumberOfCalculatedDays;
					weightedRate += (dcf.TotalNumberOfCalendarDays * ArgValue(m_ObservedArgs.Length-1));
					#endregion LastSource
					if (0 != totalNbDays)
						averagedRate = weightedRate / totalNbDays;

					ret = Cst.ErrLevel.SUCCES;
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}

		#endregion WeightedMethod
		#region UnWeightedMethod
		private Cst.ErrLevel UnWeightedMethod()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				if (0 < m_ObservedArgs.Length)
				{
					decimal unWeightedRate = 0;
					for (int i=0;i<m_ObservedArgs.Length;i++)
						unWeightedRate += ArgValue(i);
					averagedRate = unWeightedRate / m_ObservedArgs.Length;
					ret = Cst.ErrLevel.SUCCES;
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;

		}
		#endregion UnWeightedMethod
		#endregion Methods
	}
	#endregion EFS_Averaging
	#region EFS_CapFlooring
	public class EFS_CapFlooring
	{
		#region Variables
		protected decimal          m_SourceRate;
		protected DateTime         m_StartDate;
		protected DateTime         m_EndDate;
		protected StrikeSchedule[] m_StrikeSchedules;
		protected string           m_EventType;
		#endregion Variables
		#region Result Variables
		public decimal             capFlooredRate;
		public decimal             capFlooredAmount;
		public decimal             strike;
		#endregion Result Variables
		#region Constructors
		public EFS_CapFlooring(CapFloorPeriodInfo pCapFloorPeriodInfo)
			:this(pCapFloorPeriodInfo.sourceRate,pCapFloorPeriodInfo.startDate,pCapFloorPeriodInfo.endDate,
			pCapFloorPeriodInfo.eventType,pCapFloorPeriodInfo.strikeSchedules){}

		public EFS_CapFlooring(decimal pSourceRate,DateTime pStartDate,DateTime pEndDate,string pEventType,StrikeSchedule[] pStrikeSchedules)
		{
			m_SourceRate      = pSourceRate;
			m_StartDate		  = pStartDate;
			m_EndDate		  = pEndDate;
			m_EventType		  = pEventType;
			m_StrikeSchedules = pStrikeSchedules;
			Calc();
		}
		#endregion Constructors
		#region Methods
		#region Calc
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			capFlooredRate		= 0;
			capFlooredAmount	= 0;
			try
			{
				foreach (StrikeSchedule strikeSchedule in m_StrikeSchedules)
				{
					if ( strikeSchedule.buyerSpecified)
					{
						if ((m_EventType.EndsWith("S") && (PayerReceiverEnum.Payer == strikeSchedule.buyer.Value)) ||
							(m_EventType.EndsWith("B") && (PayerReceiverEnum.Receiver == strikeSchedule.buyer.Value)))
							continue;
					}
					else if (strikeSchedule.sellerSpecified)
					{
						if ((m_EventType.EndsWith("B") && (PayerReceiverEnum.Payer == strikeSchedule.seller.Value)) ||
							(m_EventType.EndsWith("S") && (PayerReceiverEnum.Receiver == strikeSchedule.seller.Value)))
							continue;
					}

					#region GetStrikeValue
					strike = Tools.GetStepValue((Schedule)strikeSchedule,m_StartDate,m_EndDate);
					#endregion GetStrikeValue
					#region capFlooredRate
					if (m_EventType.StartsWith("CA"))
					{
						capFlooredRate = Math.Max(capFlooredRate,Math.Max(0,m_SourceRate-strike));
					}
					else if (m_EventType.StartsWith("FL"))
					{
						if ((-1 == Math.Sign(m_SourceRate)) && (0 != strike))
							capFlooredRate = Math.Max(capFlooredRate,strike + Math.Abs(m_SourceRate));
						else
							capFlooredRate = Math.Max(capFlooredRate,Math.Max(0,strike - m_SourceRate));
					}
					capFlooredRate = capFlooredRate * (m_EventType.EndsWith("S")?-1:1);
					#endregion capFlooredRate
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#endregion Methods
	}
	#endregion EFS_CapFlooring
	#region EFS_CalculAmount
	public class EFS_CalculAmount
	{
		#region Variables
		protected decimal              m_Nominal;
		protected decimal              m_RateMultiplier;
		protected decimal              m_RateValue;
		protected decimal              m_RateSpread;
		protected EFS_DayCountFraction m_DayCountFraction;
		#endregion Variables
		#region Result Variables
		public decimal                 calculatedAmount;
		public decimal                 roundedCalculatedAmount;
		#endregion Result Variables
		#region Constructors
		public EFS_CalculAmount(decimal pNominal,decimal pRateValue,int pTotalOfYears,int pNumerator,int pDenominator)
			:this(pNominal,1,pRateValue,0,pTotalOfYears,pNumerator,pDenominator){}

		public EFS_CalculAmount(decimal pNominal,decimal pRateValue,DateTime pStartDate,DateTime pEndDate,
			string pDayCountFraction)
			:this(pNominal,1,pRateValue,0,pStartDate,pEndDate,pDayCountFraction){}

		public EFS_CalculAmount(decimal pNominal,decimal pRateMultiplier,decimal pRateValue,decimal pRateSpread,
			int pTotalOfYears,int pNumerator,int pDenominator)
		{
			m_Nominal        = pNominal;
			m_RateMultiplier = pRateMultiplier;
			m_RateValue      = pRateValue;
			m_RateSpread     = pRateSpread;

			m_DayCountFraction = new EFS_DayCountFraction();
			m_DayCountFraction.NumberOfCalendarYears = pTotalOfYears;
			m_DayCountFraction.Numerator             = pNumerator;
			m_DayCountFraction.Denominator           = pDenominator;

			Calc();
		}

		public EFS_CalculAmount(decimal pNominal,decimal pRateMultiplier,decimal pRateValue,decimal pRateSpread,
			DateTime pStartDate,DateTime pEndDate,string pDayCountFraction)
		{
			m_Nominal        = pNominal;
			m_RateMultiplier = pRateMultiplier;
			m_RateValue      = pRateValue;
			m_RateSpread     = pRateSpread;

			m_DayCountFraction = new EFS_DayCountFraction(pStartDate,pEndDate,pDayCountFraction);

			Calc();
		}
		#endregion Constructors
		#region Methods
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			try
			{
				decimal totalOfYears = Convert.ToDecimal(m_DayCountFraction.NumberOfCalendarYears);
				decimal numerator    = Convert.ToDecimal(m_DayCountFraction.Numerator);
				decimal denominator  = Convert.ToDecimal(m_DayCountFraction.Denominator);
				calculatedAmount = (m_Nominal * ((m_RateMultiplier * m_RateValue) + m_RateSpread) * 
					(totalOfYears + (numerator / denominator)));
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Methods
	}
	#endregion EFS_CalculAmount
	#region EFS_Compounding
	public class EFS_Compounding
	{
		#region Variables
		protected CompoundingMethodEnum             m_CompoundingMethod;
		protected NegativeInterestRateTreatmentEnum m_NegativeInterestRateTreatment;
		protected EFS_CompoundingParameters         m_Parameter;

		public decimal calculatedValue;

		protected EFS_DayCountFraction m_Dcf;

		#region Straight Variables
		protected decimal m_CompoundingPeriod				= 0;
		protected decimal m_AdjustedCalculation				= 0;
		protected decimal m_TotCompoundingPeriod			= 0;
		#endregion Straight Variables
		#region Flat Variables
		protected decimal m_BasicCompoundingPeriod			= 0;
		protected decimal m_FlatCompounding					= 0;
		protected decimal m_AdditionalCompoundingPeriod		= 0;
		protected decimal m_TotBasicCompoundingPeriod		= 0;
		protected decimal m_TotAdditionalCompoundingPeriod	= 0;
		#endregion Flat Variables
		#endregion Variables
		#region Accessors
		#region IsFlatMethod
		public bool IsFlatMethod
		{
			get {return CompoundingMethodEnum.Flat == m_CompoundingMethod;}
		}
		#endregion IsFlatMethod
		#region IsNoneMethod
		public bool IsNoneMethod
		{
			get {return CompoundingMethodEnum.None == m_CompoundingMethod;}
		}
		#endregion IsNoneMethod
		#region IsStraightMethod
		public bool IsStraightMethod
		{
			get {return CompoundingMethodEnum.Straight == m_CompoundingMethod;}
		}
		#endregion IsStraightMethod
		#endregion Accessors
		#region Constructors
		public EFS_Compounding(CompoundingMethodEnum pCompounding)
		{
			m_CompoundingMethod = pCompounding;
		}
		public EFS_Compounding(CompoundingMethodEnum pCompounding,NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment)
		{
			m_CompoundingMethod             = pCompounding;
			m_NegativeInterestRateTreatment = pNegativeInterestRateTreatment;
		}
		#endregion Constructors
		#region Methods
		#region FlatCompoundingMethod
		protected Cst.ErrLevel Flat()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			try
			{
				m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate,m_Parameter.endDate,m_Parameter.dayCountFraction);
				#region BasicCompoundingPeriod
				m_BasicCompoundingPeriod = m_Parameter.nominal * ((m_Parameter.multiplier * m_Parameter.rate) + m_Parameter.spread) *
					(m_Dcf.NumberOfCalendarYears + (m_Dcf.Numerator / m_Dcf.Denominator));
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
				m_AdditionalCompoundingPeriod = m_FlatCompounding * (m_Parameter.multiplier * m_Parameter.rate) *
					(m_Dcf.NumberOfCalendarYears + (m_Dcf.Numerator / m_Dcf.Denominator));
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
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion FlatCompoundingMethod
		#region StraightCompoundingMethod
		protected Cst.ErrLevel Straight()
		{

			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			try
			{
				m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate,m_Parameter.endDate,m_Parameter.dayCountFraction);
				#region AdjustedCalculation
				m_AdjustedCalculation = m_Parameter.nominal + m_TotCompoundingPeriod;
				#endregion AdjustedCalculation
				#region CompoundingPeriod
				m_CompoundingPeriod	= m_AdjustedCalculation * 
					((m_Parameter.multiplier * m_Parameter.rate) + m_Parameter.spread) *
					(m_Dcf.NumberOfCalendarYears + (m_Dcf.Numerator / m_Dcf.Denominator));
				#region ZeroInterestRateMethod
				if ((0 > m_CompoundingPeriod) && 
					(NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
					m_CompoundingPeriod = 0;
				#endregion ZeroInterestRateMethod
				#endregion CompoundingPeriod
				calculatedValue += m_CompoundingPeriod;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion StraightCompoundingMethod
		#endregion Methods
	}
	#endregion EFS_Compounding
	#region EFS_CompoundingAmount
	public class EFS_CompoundingAmount : EFS_Compounding
	{
		#region Variables
		protected EFS_CalculationPeriodEvent m_CalcPeriodEvent;
		#endregion Variables
		#region Constructors
		public EFS_CompoundingAmount(CompoundingMethodEnum pCompounding,EFS_CalculationPeriodEvent[] pCalcPeriodEvents,
			NegativeInterestRateTreatmentEnum pNegativeInterestRateTreatment):base(pCompounding,pNegativeInterestRateTreatment)
		{
			Calc(pCalcPeriodEvents);
		}
		#endregion Constructors
		#region Methods
		#region Calc
		public Cst.ErrLevel Calc(EFS_CalculationPeriodEvent[] pCalcPeriodEvents)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				for (int i=0;i<pCalcPeriodEvents.Length;i++)
				{
					m_CalcPeriodEvent = (EFS_CalculationPeriodEvent) pCalcPeriodEvents.GetValue(i);
					m_Parameter = new EFS_CompoundingParameters(m_CalcPeriodEvent);

					if (IsFlatMethod)
					{
						ret = base.Flat();
						calculatedValue = m_TotBasicCompoundingPeriod + m_TotAdditionalCompoundingPeriod;
					}
					else if (IsNoneMethod)
					{
						if ((0 < m_CalcPeriodEvent.calculatedAmount) || 
							(NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod != m_NegativeInterestRateTreatment))
							calculatedValue += m_CalcPeriodEvent.calculatedAmount;
					}
					else if (IsStraightMethod)
					{
						ret = base.Straight();
						calculatedValue = (m_TotBasicCompoundingPeriod + m_TotAdditionalCompoundingPeriod);
					}
				}
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#endregion Methods
	}
	#endregion EFS_CompoundingRate
	#region EFS_CompoundingParameters
	public class EFS_CompoundingParameters
	{
		#region Variables
		public  DateTime startDate;
		public  DateTime endDate;
		public  decimal  nominal;
		public  decimal  multiplier;
		public  decimal  rate;
		public  decimal  spread;
		public  string   dayCountFraction;
		#endregion Variables
		#region Constructors
		public EFS_CompoundingParameters(EFS_CalculationPeriodEvent pCalculationPeriodEvent)
			:this(pCalculationPeriodEvent.startDate,pCalculationPeriodEvent.endDate,
			pCalculationPeriodEvent.nominal,pCalculationPeriodEvent.multiplier,pCalculationPeriodEvent.calculatedRate,
			pCalculationPeriodEvent.spread,pCalculationPeriodEvent.dayCountFraction){}

		public EFS_CompoundingParameters(EFS_SelfAveragingEvent pSelfAveragingEvent)
			:this(pSelfAveragingEvent.startDate,pSelfAveragingEvent.endDate,1,1,
			pSelfAveragingEvent.averagedAndTreatedRate,0,pSelfAveragingEvent.dayCountFraction){}

		public EFS_CompoundingParameters(DateTime pStartDate,DateTime pEndDate,decimal pNominal,
			decimal pMultiplier,decimal pRate,decimal pSpread,string pDayCountFraction)
		{
			startDate        = pStartDate;
			endDate          = pEndDate;
			nominal          = pNominal;
			multiplier       = pMultiplier;
			rate             = pRate;
			spread           = pSpread;
			dayCountFraction = pDayCountFraction;
		}
		#endregion Constructors
	}
	#endregion EFS_CompoundingParameters
	#region EFS_CompoundingRate
	public class EFS_CompoundingRate : EFS_Compounding
	{

		#region Variables
		protected DateTime m_StartDate;
		protected DateTime m_EndDate;
		protected int m_TotalNbDays;
		#endregion Variables
		#region Constructors
		public EFS_CompoundingRate(CompoundingMethodEnum pCompounding,DateTime pStartDate,DateTime pEndDate,
			EFS_SelfAveragingEvent[] pSelfAveragingEvents):base(pCompounding)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			EFS_SelfAveragingEvent selfAveragingEvent = null;
			try
			{
				m_StartDate = pStartDate;
				m_EndDate   = pEndDate;

				for (int i=0;i<pSelfAveragingEvents.Length;i++)
				{
					selfAveragingEvent = (EFS_SelfAveragingEvent) pSelfAveragingEvents.GetValue(i);
					m_Parameter = new EFS_CompoundingParameters(selfAveragingEvent);

					if (IsFlatMethod)
						ret = Flat();
					else if (IsNoneMethod)
						ret = None();
					else if (IsStraightMethod)
						ret = Straight();
				}

				if (Cst.ErrLevel.SUCCES == ret)
					ret = FinalCompounding(selfAveragingEvent.selfDayCountFraction);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region Methods
		#region FinalCompounding
		private Cst.ErrLevel FinalCompounding(string pDayCountFraction)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.BUG;
			try
			{
				if (IsNoneMethod)
					calculatedValue = calculatedValue / m_TotalNbDays;
				else
				{
					m_Dcf = new EFS_DayCountFraction(m_StartDate,m_EndDate,pDayCountFraction);
					if (IsFlatMethod)
						calculatedValue = (m_TotBasicCompoundingPeriod + m_TotAdditionalCompoundingPeriod) / 
							(m_Dcf.NumberOfCalendarYears + (m_Dcf.Numerator / m_Dcf.Denominator));
					else if (IsStraightMethod)
						calculatedValue = calculatedValue / (m_Dcf.NumberOfCalendarYears + (m_Dcf.Numerator / m_Dcf.Denominator));
				}
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion FinalCompounding
		#region NoneCompoundingMethod
		private Cst.ErrLevel None()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.BUG;
			m_TotalNbDays = 0;
			try
			{
				m_Dcf = new EFS_DayCountFraction(m_Parameter.startDate,m_Parameter.endDate,m_Parameter.dayCountFraction);
				m_TotalNbDays += m_Dcf.TotalNumberOfCalculatedDays;
				calculatedValue += (m_Dcf.TotalNumberOfCalculatedDays * m_Parameter.rate);
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion NoneCompoundingMethod
		#endregion Methods
	}
	#endregion EFS_CompoundingRate
	#region EFS_Discounting
	public class EFS_Discounting
	{
		#region Variables
		protected decimal  m_SourceValue;
		protected string   m_DiscountingType;
		protected bool     m_IsCompoundValue;
		protected DateTime m_StartDate;
		protected DateTime m_EndDate;
		protected decimal  m_DiscountRate;
		protected string   m_DiscountDayCountFraction;

		public decimal discountedValue;
		#endregion Variables
		#region Constructors
		public EFS_Discounting(string pDiscountingType,DateTime pStartDate,DateTime pEndDate,decimal pDiscountRate,string pDiscountDayCountFraction)
			:this(0,pDiscountingType,false,pStartDate,pEndDate,pDiscountRate,pDiscountDayCountFraction){}

		public EFS_Discounting(decimal pSourceValue,string pDiscountingType,bool pIsCompoundValue,
			DateTime pStartDate,DateTime pEndDate,decimal pDiscountRate,string pDiscountDayCountFraction)
		{
			m_SourceValue              = pSourceValue;
			m_DiscountingType          = pDiscountingType;
			m_IsCompoundValue          = pIsCompoundValue;
			m_StartDate                = pStartDate;
			m_EndDate                  = pEndDate;
			m_DiscountDayCountFraction = pDiscountDayCountFraction;
		}
		#endregion Constructors
	}
	#endregion EFS_Discounting
	#region EFS_EquivalentRate
	public class EFS_EquivalentRate
	{
		#region Variables
		protected EquiRateMethodEnum   m_EquiRateMethod;
		protected DateTime             m_StartDate;
		protected DateTime             m_EndDate;
		protected decimal              m_SourceRate;
		protected Rounding             m_Rounding;
		protected string               m_SourceDayCountFraction;
		protected string               m_TargetDayCountFraction;
		protected EFS_DayCountFraction m_SourceDcf;
		protected EFS_DayCountFraction m_TargetDcf;
		#endregion Variables
		#region Result Variables
		public    decimal              compoundRate;
		public    decimal              simpleRate;
		#endregion Result Variables
		#region Accessors
		#endregion Accessors
		#region Constructors
		public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod,DateTime pStartDate,DateTime pEndDate,
			decimal pSourceRate,string pSourceDayCountFraction)
			:this(pEquiRateMethod,pStartDate,pEndDate,pSourceRate,null,pSourceDayCountFraction,pSourceDayCountFraction){}

		public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod,DateTime pStartDate,DateTime pEndDate,
			decimal pSourceRate,string pSourceDayCountFraction,string pTargetDayCountFraction)
			:this(pEquiRateMethod,pStartDate,pEndDate,pSourceRate,null,pSourceDayCountFraction,pTargetDayCountFraction){}

		public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod,DateTime pStartDate,DateTime pEndDate,
			decimal pSourceRate,Rounding pRounding,string pSourceDayCountFraction)
			:this(pEquiRateMethod,pStartDate,pEndDate,pSourceRate,pRounding,pSourceDayCountFraction,pSourceDayCountFraction){}

		public EFS_EquivalentRate(EquiRateMethodEnum pEquiRateMethod,DateTime pStartDate,DateTime pEndDate,
			decimal pSourceRate,Rounding pRounding,string pSourceDayCountFraction,string pTargetDayCountFraction )
		{
			m_EquiRateMethod         = pEquiRateMethod;
			m_StartDate              = pStartDate;
			m_EndDate                = pEndDate;
			m_SourceRate             = pSourceRate;
			m_Rounding               = pRounding;
			m_SourceDayCountFraction = pSourceDayCountFraction;
			m_TargetDayCountFraction = pTargetDayCountFraction;
			Calc();
		}
		#endregion Constructors
		#region Methods
		#region Calc
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				#region Rounding SourceRate
				if (null != m_Rounding)
				{
					EFS_Round round = new EFS_Round(m_Rounding,m_SourceRate);
					m_SourceRate    = round.AmountRounded;
				}
				#endregion Rounding SourceRate

				#region SourceDayCountFraction
				m_SourceDcf = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_SourceDayCountFraction);
				m_TargetDcf = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_TargetDayCountFraction);
				#endregion SourceDayCountFraction

				switch (m_EquiRateMethod)
				{
					case EquiRateMethodEnum.CompoundToSimple:
						EquivalentSimpleRateFromCompoundRate();
						break;
					case EquiRateMethodEnum.SimpleToCompound:
						EquivalentCompoundRateFromSimpleRate();
						break;
				}
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#region EquivalentCompoundRateFromSimpleRate
		private Cst.ErrLevel EquivalentCompoundRateFromSimpleRate()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				simpleRate = m_SourceRate;
				decimal x = (1 + simpleRate);
				decimal y = (m_SourceDcf.NumberOfCalendarYears + (m_SourceDcf.Numerator / m_SourceDcf.Denominator));	
				decimal z = 1 / (m_TargetDcf.NumberOfCalendarYears + (m_TargetDcf.Numerator / m_TargetDcf.Denominator)); 
				compoundRate = Convert.ToDecimal(Math.Pow(Convert.ToDouble(x),Convert.ToDouble(y))) - 1;
				compoundRate = compoundRate * z;
				/*
				simpleRate = m_SourceRate;
				decimal x = 1 + (simpleRate * (m_SourceDcf.NumberOfCalendarYears + (m_SourceDcf.Numerator / m_SourceDcf.Denominator)));
				decimal y = 1 / (m_TargetDcf.NumberOfCalendarYears + (m_TargetDcf.Numerator / m_TargetDcf.Denominator)); 
				compoundRate = Convert.ToDecimal(Math.Pow(Convert.ToDouble(x),Convert.ToDouble(y))) - 1;
				*/
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;

		}
		#endregion EquivalentCompoundRateFromSimpleRate
		#region EquivalentSimpleRateFromCompoundRate
		private Cst.ErrLevel EquivalentSimpleRateFromCompoundRate()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				compoundRate = m_SourceRate;
				decimal x = 1 + (compoundRate * (m_SourceDcf.NumberOfCalendarYears + (m_SourceDcf.Numerator / m_SourceDcf.Denominator)));
				decimal y = 1 / (m_TargetDcf.NumberOfCalendarYears + (m_TargetDcf.Numerator / m_TargetDcf.Denominator)); 
				simpleRate = Convert.ToDecimal(Math.Pow(Convert.ToDouble(x),Convert.ToDouble(y))) - 1;
				/*
				compoundRate = m_SourceRate;
				decimal x = m_SourceDcf.NumberOfCalendarYears + (m_SourceDcf.Numerator / m_SourceDcf.Denominator);
				decimal y = 1 / (m_TargetDcf.NumberOfCalendarYears + (m_TargetDcf.Numerator / m_TargetDcf.Denominator)); 
				simpleRate = Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + compoundRate),Convert.ToDouble(x))) - 1;
				simpleRate = simpleRate * y;
				*/
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion EquivalentSimpleRateFromCompoundRate
		#endregion Methods
	}
	#endregion EFS_EquivalentRate


	#region EFS_ISDADiscounting
	public class EFS_ISDADiscounting : EFS_Discounting
	{
		#region Variables
		protected new decimal m_SourceValue;
		protected decimal     m_FloatingRateValue;
		protected decimal     m_FixedRateValue;
		protected string      m_PaymentDayCountFraction;
		#endregion Variables
		#region Constructors
		public EFS_ISDADiscounting(decimal pSourceValue,DateTime pStartDate,DateTime pEndDate,
			decimal pDiscountRate,string pDiscountDayCountFraction,
			decimal pFloatingRateValue,decimal pFixedRateValue,string pPaymentDayCountFraction)
			:base(FraDiscountingEnum.ISDA.ToString(),pStartDate,pEndDate,pDiscountRate,pDiscountDayCountFraction)
		{
			m_SourceValue				= pSourceValue;
			m_FloatingRateValue			= pFloatingRateValue;
			m_FixedRateValue			= pFixedRateValue;
			m_PaymentDayCountFraction	= pPaymentDayCountFraction;
			Calc();
		}
		#endregion Constructors
		#region Methods
		#region Calc
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				EFS_DayCountFraction paymentDCF = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_PaymentDayCountFraction);
				base.m_SourceValue = m_SourceValue * (m_FloatingRateValue - m_FixedRateValue) * 
					(paymentDCF.NumberOfCalendarYears + (paymentDCF.Numerator / paymentDCF.Denominator));
				EFS_StandardDiscounting standardDiscounting = new EFS_StandardDiscounting(base.m_SourceValue,false,m_StartDate,m_EndDate,
					m_DiscountRate,m_DiscountDayCountFraction);
				base.discountedValue = standardDiscounting.discountedValue;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#endregion Methods
	}
	#endregion EFS_ISDADiscounting
	#region EFS_AFMADiscounting
	public class EFS_AFMADiscounting : EFS_Discounting
	{
		#region Variables
		protected new decimal m_SourceValue;
		protected decimal     m_FloatingRateValue;
		protected string      m_FloatingDayCountFraction;
		protected decimal     m_FixedRateValue;
		protected string      m_FixedDayCountFraction;
		protected decimal     m_DiscountedValue1;
		protected decimal     m_DiscountedValue2;
		#endregion Variables
		#region Constructors
		public EFS_AFMADiscounting(decimal pSourceValue,DateTime pStartDate,DateTime pEndDate,decimal pDiscountRate,
			string pDiscountDayCountFraction,decimal pFloatingRateValue,string pFloatingDayCountFraction,
			decimal pFixedRateValue,string pFixedDayCountFraction)
			:base(FraDiscountingEnum.ISDA.ToString(),pStartDate,pEndDate,pDiscountRate,pDiscountDayCountFraction)
		{
			m_SourceValue				= pSourceValue;
			m_FloatingRateValue			= pFloatingRateValue;
			m_FloatingDayCountFraction	= pFloatingDayCountFraction;
			m_FixedRateValue			= pFixedRateValue;
			m_FixedDayCountFraction		= pFixedDayCountFraction;
			Calc();
		}
		#endregion Constructors
		#region Methods
		#region Calc
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				EFS_DayCountFraction floatingDCF = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_FloatingDayCountFraction);
				EFS_DayCountFraction fixedDCF = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_FixedDayCountFraction);
				EFS_StandardDiscounting standardDiscounting;

				#region FloatingRate
				base.m_SourceValue = m_SourceValue * m_FloatingRateValue * 
					(floatingDCF.NumberOfCalendarYears + (floatingDCF.Numerator / floatingDCF.Denominator));
				standardDiscounting = new EFS_StandardDiscounting(base.m_SourceValue,false,m_StartDate,m_EndDate,
					m_DiscountRate,m_DiscountDayCountFraction);
				m_DiscountedValue1 = standardDiscounting.discountedValue;
				#endregion FloatingRate
				#region FixedRate
				base.m_SourceValue = m_SourceValue * m_FixedRateValue * 
					(fixedDCF.NumberOfCalendarYears + (fixedDCF.Numerator / fixedDCF.Denominator));
				standardDiscounting = new EFS_StandardDiscounting(base.m_SourceValue,false,m_StartDate,m_EndDate,
					m_DiscountRate,m_DiscountDayCountFraction);
				m_DiscountedValue2 = standardDiscounting.discountedValue;
				#endregion FixedRate

				#region Final discountedValue
				discountedValue = m_DiscountedValue1 - m_DiscountedValue2;
				#endregion Final discountedValue
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#endregion Methods
	}
	#endregion EFS_AFMADiscounting
	#region EFS_StandardDiscounting
	public class EFS_StandardDiscounting : EFS_Discounting
	{
		#region Constructors
		public EFS_StandardDiscounting(decimal pSourceValue,bool pIsCompoundValue,DateTime pStartDate,DateTime pEndDate,
			decimal pDiscountRate,string pDiscountDayCountFraction)
			:base(pSourceValue,DiscountingTypeEnum.Standard.ToString(),pIsCompoundValue,pStartDate,pEndDate,pDiscountRate,pDiscountDayCountFraction)
		{
			Calc();
		}
		#endregion Constructors
		#region Methods
		#region Calc
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				EFS_DayCountFraction discountDCF = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_DiscountDayCountFraction);
				if (m_IsCompoundValue)
				{
					discountedValue = m_SourceValue / 
						Convert.ToDecimal(Math.Pow(Convert.ToDouble(1 + m_DiscountRate),
						Convert.ToDouble(discountDCF.NumberOfCalendarYears + (discountDCF.Numerator / discountDCF.Denominator))));
				}
				else
				{
					discountedValue = m_SourceValue / (1 + (m_DiscountRate * 
						(discountDCF.NumberOfCalendarYears + (discountDCF.Numerator / discountDCF.Denominator))));
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Calc
		#endregion Methods
	}
	#endregion EFS_StandardDiscounting
	#region EFS_EquivalentYieldForADiscountRate
	public class EFS_EquivalentYieldForADiscountRate
	{
		#region Variables
		protected decimal              m_ObservedRate;
		protected DateTime             m_StartDate;
		protected DateTime             m_EndDate;
		protected RateTreatmentEnum    m_RateTreatmentMethod;
		protected DayCountFractionEnum m_DayCountFraction = DayCountFractionEnum.ACT360;
		protected DayCountFractionEnum m_DayCountFraction2;
		#endregion Variables
		#region Result Variables
		public decimal                 treatedRate;
		#endregion Result Variables
		#region Constructors
		public EFS_EquivalentYieldForADiscountRate(FpML.Enum.RateTreatmentEnum pRateTreatment,Decimal pObservedRate,
			DateTime pStartDate,DateTime pEndDate)
		{
			m_ObservedRate		  = pObservedRate;
			m_StartDate			  = pStartDate;
			m_EndDate			  = pEndDate;
			m_RateTreatmentMethod = pRateTreatment;
			Calc();
		}
		#endregion Constructors
		#region Methods
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				switch (m_RateTreatmentMethod)
				{
					case FpML.Enum.RateTreatmentEnum.BondEquivalentYield:
						m_DayCountFraction2 = DayCountFractionEnum.ACTACTISDA; 
						break;
					case FpML.Enum.RateTreatmentEnum.MoneyMarketYield:
						m_DayCountFraction2 = DayCountFractionEnum.ACT360; 
						break;
				}
				EFS_DayCountFraction dcf = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_DayCountFraction);
				treatedRate = (m_ObservedRate * dcf.Denominator) / (360 - (m_ObservedRate * dcf.TotalNumberOfCalculatedDays));
				if (m_DayCountFraction2 != m_DayCountFraction)
				{
					EFS_DayCountFraction dcf2 = new EFS_DayCountFraction(m_StartDate,m_EndDate,m_DayCountFraction2);
					treatedRate = treatedRate / dcf.Denominator * dcf2.Denominator;
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Methods
	}
	#endregion EFS_EquivalentYieldForADiscountRate
	#region EFS_Interpolating
	public class EFS_Interpolating
	{
		#region Variables
		protected DateTime              m_AccrualsDate;
		protected decimal               m_ObservedRate;
		protected DateTime              m_ObservedDate;
		protected decimal               m_ObservedRate2;
		protected DateTime              m_ObservedDate2;
		protected DateTime              m_EndDate;
		protected RoundingDirectionEnum m_RoundingDirection;
		protected int                   m_RoundingPrecision;
		#endregion Variables
		#region Result Variables
		public decimal                  interpolatedRate;
		#endregion Result Variables
		#region Constructors
		public EFS_Interpolating(DateTime pAccrualsDate,decimal pObservedRate,DateTime pObservedDate,
			decimal pObservedRate2,DateTime pObservedDate2,DateTime pEndDate,RoundingDirectionEnum pRoundingDirection,int pRoundingPrecision)
		{
			m_AccrualsDate		= pAccrualsDate;
			m_ObservedRate		= pObservedRate;
			m_ObservedDate		= pObservedDate;
			m_ObservedRate2		= pObservedRate2;
			m_ObservedDate2		= pObservedDate2;
			m_EndDate			= pEndDate;
			m_RoundingDirection	= pRoundingDirection;
			m_RoundingPrecision	= pRoundingPrecision;
			Calc();
		}
		#endregion Constructors
		#region Methods
		public Cst.ErrLevel Calc()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				if (!DtFunc.IsDateTimeEmpty(m_AccrualsDate))
					m_EndDate = m_AccrualsDate;
				TimeSpan timeSpan = (m_EndDate - m_ObservedDate);
				TimeSpan timeSpan2 = (m_ObservedDate2 - m_ObservedDate);
				interpolatedRate = m_ObservedRate + ((m_ObservedRate2 - m_ObservedRate) * timeSpan.Days / timeSpan2.Days);
				EFS_Round roundedRate = new EFS_Round(m_RoundingDirection,m_RoundingPrecision,interpolatedRate);
				interpolatedRate = roundedRate.AmountRounded;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return ret;
		}
		#endregion Methods
	}
	#endregion EFS_Interpolating
	#region EFS_ObservedValue
	public class EFS_ObservedValue
	{
		public decimal Value;
		public DateTime date;

		public EFS_ObservedValue(Decimal pValue,DateTime pDate)
		{
			Value = pValue;
			date  = pDate;
		}
	}
	#endregion EFS_ObservedValue

	#region CalculationPeriodInfo
	public class CalculationPeriodInfo
	{
		#region Variables
		protected EventsValProcessBase m_EventsValProcess;
		
		protected DateTime             m_AccrualDate;
		public    DateTime             startDate;
		protected DateTime             m_EndDate;
		protected DateTime             m_RateCutOffDate;
		protected string               m_Currency;
		public    decimal              nominal;
		public    decimal              multiplier;
		public    decimal              spread;
		public    string               dayCountFraction;
		protected int                  m_IdAsset;
		protected int                  m_IdAsset2;

		protected bool                 m_AveragingMethodSpecified;
		public    AveragingMethodEnum  averagingMethod;
		protected bool                 m_FinalRateRoundingSpecified;
		protected Rounding             m_FinalRateRounding;
		#endregion Variables
		#region Accessors
		public DateTime endDate
		{
			get
			{
				if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
					return m_EndDate;
				else if ((-1 < m_AccrualDate.CompareTo(startDate)) && (1 > m_AccrualDate.CompareTo(m_EndDate)))
					return m_AccrualDate;
				return m_EndDate;
			}
		}
		#endregion Accessors
		#region Constructors
		public CalculationPeriodInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess, DataRow pRowCalcPeriod)
		{
			SetInfoBase(pAccrualDate,pEventsValProcess,pRowCalcPeriod);
			SetParameter();
		}
		public CalculationPeriodInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowCalcPeriod,
			DateTime pRateCutOffDate)
		{
			SetInfoBase(pAccrualDate,pEventsValProcess,pRowCalcPeriod);

			DataRow[] rowAssets  = m_EventsValProcess.GetRowAsset(Convert.ToInt32(pRowCalcPeriod["ID"]));
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
				DataRow rowNominal	= m_EventsValProcess.GetCurrentNominal(startDate,Convert.ToDateTime(pRowCalcPeriod["DTSTARTUNADJ"]));
				if ((null != rowNominal) && (false == Convert.IsDBNull(rowNominal["VALORISATION"])))
				{
					nominal  = Convert.ToDecimal(rowNominal["VALORISATION"]);
					m_Currency = rowNominal["UNIT"].ToString();
				}
				else
				{
					string level = (endDate <= OTCmlHelper.GetRDBMSDtSys(m_EventsValProcess.ConnectionString))?LevelStatusTools.LevelAlert:LevelStatusTools.LevelWarning;
					LevelStatus levelStatus = new LevelStatus(level,LevelStatusTools.StatusError,LevelStatusTools.CodeReturnDataNotFound);
					throw new OTCmlException("EFS_CalculationPeriodEvent.ctor",levelStatus,
						"Nominal uncalculated Period [{0} - {1}]",
						startDate.ToShortDateString(),endDate.ToShortDateString());
				}
			}
			#endregion Nominal & Currency
			SetParameter();
		}
		#endregion Constructors
		#region Methods
		#region SetInfoBase
		private void SetInfoBase(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess, DataRow pRowCalcPeriod)
		{
			m_EventsValProcess  = pEventsValProcess;
			m_AccrualDate       = pAccrualDate;
			startDate           = Convert.ToDateTime(pRowCalcPeriod["DTSTARTADJ"]);
			m_EndDate           = Convert.ToDateTime(pRowCalcPeriod["DTENDADJ"]);
			multiplier          = 1;
			spread              = 0;
			m_Currency          = pRowCalcPeriod["UNIT"].ToString();
			
			DataRow rowNominal = m_EventsValProcess.GetCurrentNominal(startDate,Convert.ToDateTime(pRowCalcPeriod["DTSTARTUNADJ"]));
			if ((null != rowNominal) && (false == Convert.IsDBNull(rowNominal["VALORISATION"])))
				nominal = Convert.ToDecimal(rowNominal["VALORISATION"]);

			DataRow rowDetail  = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRowCalcPeriod["ID"]));
			dayCountFraction   = rowDetail["DCF"].ToString();
		}
		#endregion SetInfoBase
		#region SetParameter
		public void SetParameter()
		{
			Cst.ErrLevel ret;
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				#region FloatingRate
				if ((0 != m_IdAsset) && (null == parameter.Rate))
				{
					parameter.Rate = new SQL_AssetRateIndex(parameter.connectionString,SQL_AssetRateIndex.IDType.IDASSET,m_IdAsset);
					parameter.Rate.WithInfoSelfCompounding = Cst.IndexSelfCompounding.CASHFLOW;
				}
				#endregion FloatingRate
				#region FloatingRate2
				if ((0 != m_IdAsset2) && (null == parameter.Rate2))
					parameter.Rate2 = new SQL_AssetRateIndex(parameter.connectionString,SQL_AssetRateIndex.IDType.IDASSET,m_IdAsset2);
				#endregion FloatingRate2
				#region AveragingMethod
				ret = parameter.AveragingMethod(out averagingMethod);
				m_AveragingMethodSpecified = (Cst.ErrLevel.SUCCES == ret); 
				#endregion AveragingMethod
				#region FinalRateRounding
				m_FinalRateRounding = parameter.FinalRateRounding;
				#endregion FinalRateRounding
				#region Multiplier & spread
				ret = parameter.GetRateSpreadAndMultiplier(startDate,endDate,out multiplier,out spread);
				#endregion Multiplier & spread
			}
			catch (Exception ex){throw ex;}
		}
		#endregion SetParameter
		#endregion Methods
	}
	#endregion CalculationPeriodInfo
	#region CapFloorPeriodInfo
	public class CapFloorPeriodInfo
	{
		#region Variables
		protected EventsValProcessBase m_EventsValProcess;
		protected DateTime             m_AccrualDate;
		public    DateTime             startDate;
		protected DateTime             m_EndDate;
		public    decimal              sourceRate;
		protected string               m_Currency;
		public    string               eventType;
		public    StrikeSchedule[]     strikeSchedules;
		#endregion Variables
		#region Accessors
		public DateTime endDate
		{
			get
			{
				if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
					return m_EndDate;
				else if ((-1 < m_AccrualDate.CompareTo(startDate)) && (1 > m_AccrualDate.CompareTo(m_EndDate)))
					return m_AccrualDate;
				return m_EndDate;
			}
		}
		#endregion Accessors
		#region Constructors
		public CapFloorPeriodInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowCapFloor,
			decimal pSourceRate,string pCurrency)
		{
			m_EventsValProcess = pEventsValProcess; 
			m_AccrualDate      = pAccrualDate;
			startDate          = Convert.ToDateTime(pRowCapFloor["DTSTARTUNADJ"]);
			m_EndDate          = Convert.ToDateTime(pRowCapFloor["DTENDUNADJ"]);
			sourceRate         = pSourceRate;
			m_Currency         = pCurrency;
			eventType          = pRowCapFloor["EVENTTYPE"].ToString();
			SetParameter();
		}
		#endregion Constructors
		#region Methods
		public void SetParameter()
		{
			Cst.ErrLevel ret;
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				ret = parameter.GetStrikeSchedule(eventType,out strikeSchedules);
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Methods
	}
	#endregion CapFloorPeriodInfo
	#region PaymentInfo
	public class PaymentInfo
	{
		#region Members
		protected EventsValProcessIRD               m_EventsValProcess;					
		
		protected DateTime                          m_AccrualDate;
		protected DateTime                          m_StartDate;
		protected DateTime                          m_EndDate;
		protected DateTime                          m_RateCutOffDate;
		protected string                            m_Currency;

		protected bool                              m_CompoundingMethodSpecified;
		protected CompoundingMethodEnum             m_CompoundingMethod;
		protected bool                              m_NegativeInterestRateTreatmentSpecified;
		protected NegativeInterestRateTreatmentEnum m_NegativeInterestRateTreatment;
		protected bool                              m_DiscountingSpecified;
		protected Discounting                       m_Discounting;
		protected FraDiscountingEnum                m_FraDiscounting;
		protected decimal                           m_FraFixedRate;
		protected string                            m_Fra_DCF;
		protected decimal                           m_Fra_Notional;
		#endregion Mmebers
		
		#region Accessors
		#region IsCompounding
		public bool IsCompounding
		{
			get
			{
				if (m_CompoundingMethodSpecified)
					return (CompoundingMethodEnum.None != m_CompoundingMethod);
				else
					return false;
			}
		}
		#endregion IsCompounding
		#region Endate
		public DateTime endDate
		{
			get
			{
				if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
					return m_EndDate;
				else if ((-1 < m_AccrualDate.CompareTo(m_StartDate)) && (1 > m_AccrualDate.CompareTo(m_EndDate)))
					return m_AccrualDate;
				return m_EndDate;
			}
		}
		#endregion Endate
		#endregion Accessors
		#region Constructors
		public PaymentInfo(DateTime pAccrualDate,EventsValProcessIRD pEventsValProcess,DataRow pRowInterest)
		{
			m_EventsValProcess = pEventsValProcess;
			m_AccrualDate      = pAccrualDate;
			m_StartDate        = Convert.ToDateTime(pRowInterest["DTSTARTADJ"]);
			m_EndDate          = Convert.ToDateTime(pRowInterest["DTENDADJ"]);

			DataRow[] rowEventClass = pRowInterest.GetChildRows(m_EventsValProcess.DsEvents.ChildEventClass);
			foreach (DataRow dr in rowEventClass)
			{
				string eventClass = dr["EVENTCLASS"].ToString();
				if (EventClassFunc.IsRateCutOffDate(eventClass))
					m_RateCutOffDate = Convert.ToDateTime(dr["DTEVENT"]);
			}
			#region Currency
			m_Currency         = pRowInterest["UNIT"].ToString();
			DataRow rowNominal = m_EventsValProcess.GetCurrentNominal(m_StartDate,Convert.ToDateTime(pRowInterest["DTSTARTUNADJ"]));
			if (null != rowNominal)
				m_Currency = rowNominal["UNIT"].ToString();
			#endregion Currency
			SetParameter();
		}
		#endregion Constructors
		#region Methods
		public void SetParameter()
		{
			Cst.ErrLevel ret;
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				ret                                      = parameter.CompoundingMethod(out m_CompoundingMethod);
				m_CompoundingMethodSpecified             = (Cst.ErrLevel.SUCCES == ret); 
				ret                                      = parameter.NegativeInterestRateTreatment(out m_NegativeInterestRateTreatment);
				m_NegativeInterestRateTreatmentSpecified = (Cst.ErrLevel.SUCCES == ret); 
				ret                                      = parameter.Discounting(out m_Discounting); 
				m_DiscountingSpecified                   = (Cst.ErrLevel.SUCCES == ret); 
				m_FraDiscounting                         = parameter.FraDiscounting;
				m_FraFixedRate                           = parameter.FraFixedRate;
				m_Fra_DCF                                = parameter.FraDayCountFraction;
				m_Fra_Notional                           = parameter.FraNotional;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Methods
	}
	#endregion PaymentInfo
	#region ResetInfo
	public class ResetInfo
	{
		#region Variables
		protected EventsValProcessBase m_EventsValProcess;  
		
		protected DateTime             m_AccrualDate;
		protected DateTime             m_StartDate;
		protected DateTime             m_EndDate;
		public    DateTime             resetDate;
		protected DateTime             m_FixingDate;
		protected DateTime             m_RateCutOffDate;
		protected DateTime             m_ObservedRateDate;
		protected DateTime             m_EndPeriodDate;
		protected int                  m_IdAsset;
		protected int                  m_IdAsset2;
		protected bool                 m_RateTreatmentSpecified;
		protected RateTreatmentEnum    m_RateTreatment;
		protected Interval             m_PaymentFrequency;
		protected int                  m_RoundingPrecision;

		protected CompoundingMethodEnum m_SelfCompoundingMethod;
		#endregion Variables
		#region Accessors
		public DateTime endDate
		{
			get
			{
				if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
					return m_EndDate;
				else if ((-1 < m_AccrualDate.CompareTo(m_StartDate)) && (1 > m_AccrualDate.CompareTo(m_EndDate)))
					return m_AccrualDate;
				return m_EndDate;
			}
		}
		#endregion Accessors
		#region Constructors
		public ResetInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowReset)
		{
			SetInfoBase(pAccrualDate,pEventsValProcess,pRowReset);
			SetParameter();
		}
		public ResetInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowReset,
			int pIdAsset,int pIdAsset2,DateTime pRateCutOffDate)
		{
			SetInfoBase(pAccrualDate,pEventsValProcess,pRowReset);

			m_RateCutOffDate = pRateCutOffDate;
			m_IdAsset        = pIdAsset;
			m_IdAsset2       = pIdAsset2;

			if ((false == DtFunc.IsDateTimeEmpty(m_RateCutOffDate)) && (m_RateCutOffDate < m_FixingDate))
				m_ObservedRateDate = m_RateCutOffDate;
			else
				m_ObservedRateDate = m_FixingDate;

			DataRow rowCalcPeriod = pRowReset.GetParentRow( m_EventsValProcess.DsEvents.ChildEvent);
			m_EndPeriodDate = Convert.ToDateTime(rowCalcPeriod["DTENDADJ"]);
			SetParameter();
		}
		#endregion Constructors
		#region Methods
		#region SetInfoBase
		private void SetInfoBase(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess, DataRow pRowReset)
		{
			m_EventsValProcess = pEventsValProcess;
			m_AccrualDate      = pAccrualDate;
			m_StartDate        = Convert.ToDateTime(pRowReset["DTSTARTADJ"]);
			m_EndDate          = Convert.ToDateTime(pRowReset["DTENDADJ"]);
			DataRow[] rowEventClass = pRowReset.GetChildRows(m_EventsValProcess.DsEvents.ChildEventClass);
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
		public void SetParameter()
		{
			Cst.ErrLevel ret;
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				if ((0 != m_IdAsset) && (0 != m_IdAsset2))
				{
					int precisionRate   = Convert.ToInt32(parameter.Rate.FirstRow["Idx_ROUNDPREC"]);
					int precisionRate2  = Convert.ToInt32(parameter.Rate2.FirstRow["Idx_ROUNDPREC"]);
					m_RoundingPrecision = Math.Max(precisionRate,precisionRate2);
					m_RoundingPrecision = Math.Max(m_RoundingPrecision,3);
				}
				CompoundingMethodEnum selfCompoundingMethod = parameter.SelfCompoundingMethod;
				m_PaymentFrequency                          = parameter.PaymentFrequency;
				#region RateTreatment
				ret = parameter.RateTreatment(out m_RateTreatment);
				m_RateTreatmentSpecified = (Cst.ErrLevel.SUCCES == ret); 
				#endregion RateTreatment
			}
			catch (Exception ex){throw ex;}
		}

		#endregion SetParameter
		#endregion Methods
	}
	#endregion ResetInfo

	#region SelfAveragingInfo
	public class SelfAveragingInfo
	{
		#region Variables
		protected EventsValProcessBase m_EventsValProcess;
		
		protected DateTime             m_AccrualDate;
		public    DateTime             startDate;
		protected DateTime             m_EndDate;
		protected DateTime             m_RateCutOffDate;
		protected int                  m_IdAsset;
		public    string               selfDayCountFraction;
		public    string               dayCountFraction;
		protected bool                 m_RateTreatmentSpecified;
		protected RateTreatmentEnum    m_RateTreatment;
		public    AveragingMethodEnum  averagingMethod;
		protected Interval             m_PaymentFrequency;
		#endregion Variables
		#region Accessors
		public DateTime endDate
		{
			get
			{
				if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
					return m_EndDate;
				else if ((-1 < m_AccrualDate.CompareTo(startDate)) && (1 > m_AccrualDate.CompareTo(m_EndDate)))
					return m_AccrualDate;
				return m_EndDate;
			}
		}
		#endregion Accessors
		#region Constructors
		public SelfAveragingInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess, DataRow pRowSelfAverage)
		{
			m_EventsValProcess = pEventsValProcess; 
			m_AccrualDate      = pAccrualDate;
			startDate          = Convert.ToDateTime(pRowSelfAverage["DTSTARTADJ"]);
			m_EndDate          = Convert.ToDateTime(pRowSelfAverage["DTENDADJ"]);
			SetParameter();
		}
		public SelfAveragingInfo(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowSelfAverage,
			DateTime pRateCutOffDate)
		{
			m_EventsValProcess   = pEventsValProcess;
			m_AccrualDate        = pAccrualDate;
			startDate            = Convert.ToDateTime(pRowSelfAverage["DTSTARTADJ"]);
			m_EndDate            = Convert.ToDateTime(pRowSelfAverage["DTENDADJ"]);
			m_RateCutOffDate     = pRateCutOffDate;
			DataRow[] rowAssets  = m_EventsValProcess.GetRowAsset(Convert.ToInt32(pRowSelfAverage["ID"]));
			if ((null != rowAssets) && (0 < rowAssets.Length))
				m_IdAsset        = Convert.ToInt32(rowAssets[0]["IDASSET"]);
			SetParameter();			
		}
		#endregion Constructors
		#region Methods
		public void SetParameter()
		{
			Cst.ErrLevel ret;
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				if (0 != m_IdAsset)
				{
					if (null == parameter.RateBasis)
						parameter.RateBasis = new SQL_AssetRateIndex(parameter.connectionString,SQL_AssetRateIndex.IDType.IDASSET,m_IdAsset);
				}
				dayCountFraction         = parameter.DCFRateBasis;
				selfDayCountFraction     = parameter.SelfDayCountFraction;
				averagingMethod          = parameter.SelfAveragingMethod;
				ret                      = parameter.RateTreatmentRateBasis(out m_RateTreatment);
				m_RateTreatmentSpecified = (Cst.ErrLevel.SUCCES == ret); 
				m_PaymentFrequency       = parameter.PaymentFrequency;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Methods
	}
	#endregion SelfAveragingInfo
	#region SelfResetInfo
	public class SelfResetInfo
	{
		#region Variables
		protected EventsValProcessBase  m_EventsValProcess;
		
		protected DateTime              m_SelfResetDate;
		protected DateTime              m_RateCutOffDate;
		protected int                   m_IdAsset;
		public DateTime                 fixingDate;

		public    string				selfDayCountFraction;
		public    string				dayCountFraction;
		protected bool                  m_RateTreatmentSpecified;
		protected RateTreatmentEnum     m_RateTreatment;
		protected AveragingMethodEnum   m_AveragingMethod;
		protected Interval				m_PaymentFrequency;
		#endregion Variables
		#region Constructors
		public SelfResetInfo(EventsValProcessBase pEventsValProcess, DataRow pRowSelfReset,DateTime pRateCutOffDate)
		{
			SetInfoBase(pEventsValProcess,pRowSelfReset,pRateCutOffDate);
			SetParameter();
		}
		public SelfResetInfo(EventsValProcessBase pEventsValProcess,DataRow pRowSelfReset,int pIdAsset,DateTime pRateCutOffDate)
		{
			SetInfoBase(pEventsValProcess,pRowSelfReset,pRateCutOffDate);
			m_IdAsset = pIdAsset;
			SetParameter();
		}
		#endregion Constructors
		#region Methods
		#region SetInfoBase
		private void SetInfoBase(EventsValProcessBase pEventsValProcess, DataRow pRowSelfReset,DateTime pRateCutOffDate)
		{
			m_EventsValProcess = pEventsValProcess;
			m_SelfResetDate    = Convert.ToDateTime(pRowSelfReset["DTSTARTADJ"]);
			fixingDate         = m_SelfResetDate;
			m_RateCutOffDate   = pRateCutOffDate;
			if ((false == DtFunc.IsDateTimeEmpty(m_RateCutOffDate)) && (m_RateCutOffDate < m_SelfResetDate))
				fixingDate = m_RateCutOffDate;
		}
		#endregion SetInfoBase
		#region SetParameter
		public void SetParameter()
		{
			Cst.ErrLevel ret;
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				dayCountFraction           = parameter.DCFRateBasis;
				selfDayCountFraction       = parameter.SelfDayCountFraction;
				m_AveragingMethod          = parameter.SelfAveragingMethod;
				ret                        = parameter.RateTreatmentRateBasis(out m_RateTreatment);
				m_RateTreatmentSpecified   = (Cst.ErrLevel.SUCCES == ret); 
				m_PaymentFrequency         = parameter.PaymentFrequency;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion SetParameter
		#endregion Methods
	}
	#endregion SelfResetInfo
	#region VariationNominalInfo
	public class VariationNominalInfo
	{
		#region Variables
		protected EventsValProcessIRD m_EventsValProcess;				
		
		protected DateTime            m_AccrualDate;
		protected DateTime            m_StartDate;
		protected DateTime            m_EndDate;
		protected DateTime            m_PaymentDate;
		protected DateTime            m_FixingDate;
		protected string              m_Currency;
		protected string              m_CurrencyReference;
		#endregion Variables
		#region Accessors
		public DateTime endDate
		{
			get
			{
				if (DtFunc.IsDateTimeEmpty(m_AccrualDate))
					return m_EndDate;
				else if ((-1 < m_AccrualDate.CompareTo(m_StartDate)) && (1 > m_AccrualDate.CompareTo(m_EndDate)))
					return m_AccrualDate;
				return m_EndDate;
			}
		}
		#endregion Accessors
		#region Constructors
		public VariationNominalInfo(DateTime pAccrualDate,EventsValProcessIRD pEventsValProcess,DataRow pRowVariation)
		{
			try
			{
				m_EventsValProcess = pEventsValProcess;
				m_AccrualDate      = pAccrualDate;
				m_StartDate        = Convert.ToDateTime(pRowVariation["DTSTARTADJ"]);
				m_EndDate          = Convert.ToDateTime(pRowVariation["DTENDADJ"]);
				DataRow rowDetail  = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRowVariation["ID"]));
				if (null != rowDetail)
				{
					m_Currency          = rowDetail["IDC2"].ToString();
					m_CurrencyReference = rowDetail["IDC1"].ToString();
					m_FixingDate        = Convert.ToDateTime(rowDetail["DTFIXING"]);
				}

				#region Set PaymentDate & FixingDate
				DataRow[] rowEventClass = pRowVariation.GetChildRows(m_EventsValProcess.DsEvents.ChildEventClass);
				foreach (DataRow dr in rowEventClass)
				{
					string eventClass = dr["EVENTCLASS"].ToString();
					if (EventClassFunc.IsSettlement(eventClass))
						m_PaymentDate = Convert.ToDateTime(dr["DTEVENT"]);
				}
				#endregion Set PaymentDate & FixingDate
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
	}
	#endregion VariationNominalInfo

	#region EFS_CalculationPeriodEvent
	public class EFS_CalculationPeriodEvent : CalculationPeriodInfo
	{
		#region Variables
		protected EFS_ResetEvent[] m_ResetEvents;
		#endregion Variables
		#region Result Variables
		public Decimal averagedRate;
		public Decimal capFlooredRate;
		public Decimal calculatedRate;
		public Decimal calculatedAmount;
		public Decimal roundedCalculatedAmount;
		#endregion Result Variables
		#region Constructors
		public EFS_CalculationPeriodEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowCalcPeriod)
			:base(pAccrualDate,pEventsValProcess,pRowCalcPeriod)
		{
			roundedCalculatedAmount = Convert.ToDecimal(pRowCalcPeriod["VALORISATION"]);
		}

		public EFS_CalculationPeriodEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowCalcPeriod,
			DateTime pRateCutOffDate)
			:base(pAccrualDate,pEventsValProcess,pRowCalcPeriod,pRateCutOffDate)
		{
			try
			{
				EFS_CalculAmount calculAmount;
				DataRow[] rowResets = pRowCalcPeriod.GetChildRows(m_EventsValProcess.DsEvents.ChildEvent);
				if ((0 != rowResets.Length) && (EventTypeFunc.IsFloatingRate(pRowCalcPeriod["EVENTTYPE"].ToString())))
				{
					#region FloatingRate
					EFS_ResetEvent resetEvent;
					ArrayList aResetEvent = new ArrayList();
					#region Reset Process
					foreach (DataRow rowReset in rowResets)
					{
						if (m_EventsValProcess.IsRowMustBeCalculate(rowReset))
						{
							#region CapFloorPeriods Excluded
							if (EventTypeFunc.IsCapFloorLeg(rowReset["EVENTTYPE"].ToString()))
							{
								rowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
								continue;
							}
							#endregion CapFloorPeriods Excluded

							m_EventsValProcess.SetRowCalculated(rowReset);
							resetEvent = new EFS_ResetEvent(m_AccrualDate,m_EventsValProcess,rowReset,m_IdAsset,m_IdAsset2,m_RateCutOffDate);
							aResetEvent.Add(resetEvent);
							if (false == m_EventsValProcess.IsRowEventCalculated(rowReset))
								break;
						}
						else if (m_EventsValProcess.IsRowPrecededAccrual(rowReset) && 
							false == EventTypeFunc.IsCapFloorLeg(rowReset["EVENTTYPE"].ToString()))
						{
							aResetEvent.Add(new EFS_ResetEvent(m_AccrualDate,m_EventsValProcess,rowReset));
						}
					}
					m_ResetEvents = (EFS_ResetEvent[]) aResetEvent.ToArray(typeof(EFS_ResetEvent));
					#endregion Reset Process
					#region Final CalculationPeriod Process
					if (m_EventsValProcess.IsRowsEventCalculated(rowResets))
						
					{
						#region AveragingRate / CapFlooring / FinalRateRounding / AmountCalculation
						Averaging();
						CapFlooring(pRowCalcPeriod);
						FinalRateRounding();
						#region AmountCalculation
						calculAmount     = new EFS_CalculAmount(nominal,multiplier,calculatedRate,spread,startDate,endDate,dayCountFraction);
						calculatedAmount = calculAmount.calculatedAmount;
						#endregion AmountCalculation
						DataRow rowDetail = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRowCalcPeriod["ID"]));
						if (null != rowDetail)
						{
							rowDetail["RATE"] = Math.Abs(calculatedRate);
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
					DataRow rowDetail = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRowCalcPeriod["ID"]));
					if (null != rowDetail)
						calculatedRate = Convert.ToDecimal(rowDetail["RATE"]);
					#region Amount Calculation
					calculAmount     = new EFS_CalculAmount(nominal,calculatedRate,startDate,endDate,dayCountFraction);
					calculatedAmount = calculAmount.calculatedAmount;
					#endregion Amount Calculation
					#endregion FixedRate
				}
				else if (EventTypeFunc.IsKnownAmount(pRowCalcPeriod["EVENTTYPE"].ToString()))
				{
					#region KnownAmountSchedule
					calculatedAmount = Convert.ToDecimal(pRowCalcPeriod["VALORISATION"]);
					#endregion KnownAmountSchedule
				}
				
				UpdatingRow(pRowCalcPeriod);
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowCalcPeriod);
				m_EventsValProcess.SetRowStatus(pRowCalcPeriod,Tuning.TuningOutputTypeEnum.OEE);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region Methods
		#region Averaging
		private void Averaging()
		{
			try
			{
				if (m_AveragingMethodSpecified)
				{
					EFS_Averaging averaging = new EFS_Averaging(this,m_ResetEvents);
					averagedRate            = averaging.averagedRate;
					calculatedRate          = averagedRate;
				}
				else if ((1 == m_ResetEvents.Length) || (0 != m_IdAsset2))
					calculatedRate = ((EFS_ResetEvent)m_ResetEvents.GetValue(0)).observedRate;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Averaging
		#region CapFlooring
		private void CapFlooring(DataRow pRow)
		{
			try
			{
				DataRow[] rowCapFloors = pRow.GetChildRows(m_EventsValProcess.DsEvents.ChildEventByEventCode);
				if (0 != rowCapFloors.Length)
				{
					foreach (DataRow rowCapFloor in rowCapFloors)
					{
						m_EventsValProcess.SetRowCalculated(rowCapFloor);
						EFS_CapFloorPeriodEvent capFloorPeriodEvent = 
							new EFS_CapFloorPeriodEvent(m_AccrualDate,m_EventsValProcess,rowCapFloor,calculatedRate,m_Currency);
						capFlooredRate += capFloorPeriodEvent.capFlooredRate;
						if (false == m_EventsValProcess.IsRowEventCalculated(rowCapFloor))
							break;
					}
					if (m_EventsValProcess.IsRowsEventCalculated(rowCapFloors))
					{
						//if (0 != capFlooredRate)// glop EG 08/02/2005 Mis en commentaire
						calculatedRate = capFlooredRate;
					}
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion CapFlooring
		#region FinalRateRounding
		private void FinalRateRounding()
		{
			try
			{
				if ((0 == m_IdAsset2) && (null != m_FinalRateRounding))
				{
					EFS_Round round = new EFS_Round(m_FinalRateRounding,calculatedRate);
					calculatedRate = round.AmountRounded;
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion FinalRateRounding
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow)
		{
			try
			{
				pRow["UNIT"]			= m_Currency;
				pRow["UNITTYPE"]		= EFS_UnitTypeEnum.Currency.ToString();
				pRow["VALORISATION"]	= calculatedAmount;
				m_EventsValProcess.SetRowCalculated(pRow);
				m_EventsValProcess.SetRowStatus(pRow,Tuning.TuningOutputTypeEnum.OES);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
		#endregion Methods
	}
	#endregion EFS_CalculationPeriodEvent
	#region EFS_CapFloorPeriodEvent
	public class EFS_CapFloorPeriodEvent : CapFloorPeriodInfo
	{
		#region Result Variables
		public decimal capFlooredRate;
		public decimal capFlooredAmount;
		public decimal capFlooredStrike;
		#endregion Result Variables
		#region Constructors
		public EFS_CapFloorPeriodEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowCapFloor,
			decimal pSourceRate,string pCurrency):base(pAccrualDate,pEventsValProcess, pRowCapFloor,pSourceRate,pCurrency)
		{
			try
			{
				#region Process
				if (null != strikeSchedules)
				{
					EFS_CapFlooring capFlooring = new EFS_CapFlooring(this);
					capFlooredRate              = capFlooring.capFlooredRate;
					capFlooredAmount            = capFlooring.capFlooredAmount;
					capFlooredStrike            = capFlooring.strike;
					UpdatingRow(pRowCapFloor);
				}
				m_EventsValProcess.SetRowCalculated(pRowCapFloor);
				#endregion Process
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowCapFloor);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow)
		{
			try
			{
				pRow["UNIT"]         = m_Currency;
				pRow["UNITTYPE"]     = EFS_UnitTypeEnum.Currency.ToString();
				pRow["VALORISATION"] = capFlooredAmount;

				DataRow rowDetail    = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRow["ID"]));
				if (null == rowDetail)
				{
					rowDetail       = m_EventsValProcess.DtEventDetails.NewRow();
					rowDetail["ID"] = pRow["ID"];
					rowDetail.SetParentRow(pRow);
					m_EventsValProcess.DtEventDetails.Rows.Add(rowDetail);
				}
				rowDetail["STRIKE"] = capFlooredStrike;
				rowDetail["RATE"]   = Math.Abs(capFlooredRate);
				m_EventsValProcess.SetRowCalculated(pRow);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
	}
	#endregion EFS_CapFloorPeriodEvent2
	#region EFS_PaymentEvent
	public class EFS_PaymentEvent : PaymentInfo
	{
		#region Variables
		protected EFS_CalculationPeriodEvent[] m_CalcPeriodEvents;
		#endregion Variables
		#region Result Variables
		public decimal interestAmount;
		public decimal roundedInterestAmount;
		#endregion Result Variables
		#region Accessors
		#region DayCountFraction
		public EFS_DayCountFraction DayCountFraction
		{
			get
			{
				foreach (EFS_CalculationPeriodEvent calcPeriodEvent in m_CalcPeriodEvents)
					return new EFS_DayCountFraction(m_StartDate,endDate,calcPeriodEvent.dayCountFraction);
				return null;
			}
		}
		#endregion DayCountFraction
		#region Parameter
		public EventsValParameterBase Parameter
		{
			get
			{
				return (EventsValParameterBase) m_EventsValProcess.Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
			}
		}
		#endregion Parameter
		#endregion Accessors
		#region Constructors
		public EFS_PaymentEvent(DateTime pAccrualDate,EventsValProcessIRD pEventsValProcess,DataRow pRowInterest)
			:base(pAccrualDate,pEventsValProcess,pRowInterest)
		{
			try
			{
				#region CalculationPeriod Process
				DataRow[] rowCalcPeriods = pRowInterest.GetChildRows(m_EventsValProcess.DsEvents.ChildEvent);
				if (0 != rowCalcPeriods.Length)
				{
					EFS_CalculationPeriodEvent calcPeriodEvent;
					ArrayList aCalcPeriodEvent = new ArrayList(); 
					foreach (DataRow rowCalcPeriod in rowCalcPeriods)
					{
						if (m_EventsValProcess.IsRowMustBeCalculate(rowCalcPeriod))
						{
							m_EventsValProcess.SetRowCalculated(rowCalcPeriod);
							calcPeriodEvent = new EFS_CalculationPeriodEvent(m_AccrualDate,m_EventsValProcess,rowCalcPeriod,
								m_RateCutOffDate);
							aCalcPeriodEvent.Add(calcPeriodEvent);
							if (false == m_EventsValProcess.IsRowEventCalculated(rowCalcPeriod))
								break;
						}
						else if (m_EventsValProcess.IsRowPrecededAccrual(rowCalcPeriod))
						{
							aCalcPeriodEvent.Add(new EFS_CalculationPeriodEvent(m_AccrualDate,m_EventsValProcess,rowCalcPeriod));
						}
					}
					m_CalcPeriodEvents = (EFS_CalculationPeriodEvent[]) aCalcPeriodEvent.ToArray(typeof(EFS_CalculationPeriodEvent));
				}
				#endregion CalculationPeriod Process
				
				#region Final Payment Process
				if (m_EventsValProcess.IsRowsEventCalculated(rowCalcPeriods))
				{
					#region Compounding / Discounting / NegativeInterestRateTreatment / Rounding
					Compounding();
					if (Discounting())
						NegativeInterestRateTreatment();

					roundedInterestAmount = m_EventsValProcess.RoundingCurrencyAmount(m_Currency,interestAmount);
					#endregion Compounding / Discounting / NegativeInterestRateTreatment / Rounding

					UpdatingRow(pRowInterest);
				}
				#endregion Final Payment Process
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowInterest);
				m_EventsValProcess.SetRowStatus(pRowInterest,Tuning.TuningOutputTypeEnum.OEE);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region Methods
		#region Compounding
		private void Compounding()
		{
			try
			{
				if (m_CompoundingMethodSpecified)
				{
					EFS_CompoundingAmount compounding = 
						new EFS_CompoundingAmount(m_CompoundingMethod,m_CalcPeriodEvents,m_NegativeInterestRateTreatment);
					interestAmount = compounding.calculatedValue;
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Compounding
		#region Discounting
		private bool Discounting()
		{
			bool isNegativeInterestRateTreatmentApply = true;
			try
			{
				#region Discounting
				EFS_CalculationPeriodEvent calcPeriodEvent = (EFS_CalculationPeriodEvent)m_CalcPeriodEvents.GetValue(0);
				decimal discountRate;
				string discountRate_DCF;

				decimal fixedRate		= 0;
				string fixedRate_DCF	= string.Empty;
				decimal floatingRate	= calcPeriodEvent.calculatedRate;
				string floatingRate_DCF = calcPeriodEvent.dayCountFraction;

				if (m_DiscountingSpecified)
				{
					#region Discount Elements
					if (m_Discounting.discountRateSpecified)
						discountRate = m_Discounting.discountRate.DecValue;
					else
						discountRate = floatingRate;

					if (m_Discounting.discountRateDayCountFractionSpecified)
						discountRate_DCF = m_Discounting.discountRateDayCountFraction.ToString();
					else
						discountRate_DCF = floatingRate_DCF;
					#endregion Discount Elements

					if (DiscountingTypeEnum.FRA == m_Discounting.discountingType)
					{
						#region ISDA Discounting
						if (Cst.ErrLevel.SUCCES == m_EventsValProcess.FixedRateAndDCFFromRowInterest(m_StartDate,out fixedRate,
							out fixedRate_DCF)) 
						{
							EFS_ISDADiscounting isdaDiscounting = new EFS_ISDADiscounting(interestAmount,m_StartDate,endDate,
								discountRate,discountRate_DCF,floatingRate,fixedRate,fixedRate_DCF);
							interestAmount = isdaDiscounting.discountedValue;
						}
						#endregion ISDA Discounting
						isNegativeInterestRateTreatmentApply = false;
					}
					else
					{
						#region Standard Discounting
						EFS_StandardDiscounting standardDiscounting = new EFS_StandardDiscounting(interestAmount,false,
							m_StartDate,endDate,discountRate,discountRate_DCF);
						interestAmount = standardDiscounting.discountedValue;
						#endregion Standard Discounting
					}
				}
				else if (FraDiscountingEnum.NONE != m_FraDiscounting)
				{
					#region Discount Elements
					discountRate		= floatingRate;
					discountRate_DCF	= floatingRate_DCF;
					#endregion Discount Elements

					if (FraDiscountingEnum.ISDA == m_FraDiscounting)
					{
						#region ISDA Discounting
						EFS_ISDADiscounting isdaDiscounting = new EFS_ISDADiscounting(interestAmount,m_StartDate,endDate,
							discountRate,discountRate_DCF,floatingRate,m_FraFixedRate,floatingRate_DCF);
						interestAmount = isdaDiscounting.discountedValue;
						#endregion ISDA Discounting
						isNegativeInterestRateTreatmentApply = false;
					}
					else if (FraDiscountingEnum.AFMA == m_FraDiscounting)
					{
						#region AFMA Discounting
						EFS_AFMADiscounting afmaDiscounting = new EFS_AFMADiscounting(m_Fra_Notional,m_StartDate,endDate,
							discountRate,discountRate_DCF,floatingRate,floatingRate_DCF,m_FraFixedRate,m_Fra_DCF);
						interestAmount = afmaDiscounting.discountedValue;
						#endregion AFMA Discounting
					}
				}
				#endregion Discounting
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			return isNegativeInterestRateTreatmentApply;
		}
		#endregion Discounting
		#region NegativeInterestRateTreatment
		private void NegativeInterestRateTreatment()
		{
			try
			{
				if (m_NegativeInterestRateTreatmentSpecified && (-1 == Math.Sign(interestAmount)) &&
					(NegativeInterestRateTreatmentEnum.ZeroInterestRateMethod == m_NegativeInterestRateTreatment))
					interestAmount = 0;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion NegativeInterestRateTreatment
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow)
		{
			try
			{
				if (-1 == Math.Sign(roundedInterestAmount))
					m_EventsValProcess.SwapPayerAndReceiver(pRow);
				pRow["UNIT"]		 = m_Currency;
				pRow["UNITTYPE"]	 = EFS_UnitTypeEnum.Currency.ToString();
				pRow["VALORISATION"] = Math.Abs(roundedInterestAmount);
				//
				m_EventsValProcess.SetRowCalculated(pRow);
				m_EventsValProcess.SetRowStatus(pRow,Tuning.TuningOutputTypeEnum.OES);
				//
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
		#endregion Methods
	}
	#endregion EFS_PaymentEvent
	#region EFS_ResetEvent
	public class EFS_ResetEvent : ResetInfo
	{
		#region Variables
		protected EFS_SelfAveragingEvent[] m_SelfAveragingEvents;
		#endregion Variables
		#region Result Variables
		public decimal observedRate;
		public decimal treatedRate;
		#endregion Result Variables
		#region Constructors
		public EFS_ResetEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowReset)
			:base(pAccrualDate,pEventsValProcess, pRowReset)
		{
			DataRow rowDetail = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRowReset["ID"]));
			if (null != rowDetail)
				observedRate = Convert.ToDecimal(rowDetail["RATE"]);
			treatedRate  = Convert.ToDecimal(pRowReset["VALORISATION"]);
		}
		public EFS_ResetEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowReset,int pIdAsset,
			int pIdAsset2,DateTime pRateCutOffDate):base(pAccrualDate,pEventsValProcess,pRowReset,pIdAsset,pIdAsset2,pRateCutOffDate)
		{
			try
			{
				#region Process
				DataRow[] rowSelfAverages = pRowReset.GetChildRows(m_EventsValProcess.DsEvents.ChildEvent);
				if (0 == rowSelfAverages.Length)
				{
					#region Interpolating or Observated Rate
					if (0 != m_IdAsset2) 
						Interpolating();
					else
						observedRate = m_EventsValProcess.ReadQuote_RateIndex(m_ObservedRateDate,m_IdAsset);

					RateTreatement();
					UpdatingRow(pRowReset);
					#endregion Interpolating or Observated Rate
				}
				else
				{
					#region SelfCompounding
					EFS_SelfAveragingEvent selfAveragingEvent;
					ArrayList aSelfAveragingEvent = new ArrayList();
					#region SelfAveraging Process
					foreach (DataRow rowSelfAverage in rowSelfAverages)
					{
						if (m_EventsValProcess.IsRowMustBeCalculate(rowSelfAverage))
						{
							m_EventsValProcess.SetRowCalculated(rowSelfAverage);
							selfAveragingEvent = new EFS_SelfAveragingEvent(m_AccrualDate,m_EventsValProcess,rowSelfAverage,m_RateCutOffDate);
							aSelfAveragingEvent.Add(selfAveragingEvent);
							if (false == m_EventsValProcess.IsRowEventCalculated(rowSelfAverage))
								break;
						}
						else if (m_EventsValProcess.IsRowPrecededAccrual(rowSelfAverage))
						{
							aSelfAveragingEvent.Add(new EFS_SelfAveragingEvent(m_AccrualDate,m_EventsValProcess,rowSelfAverage));
						}
					}
					m_SelfAveragingEvents = (EFS_SelfAveragingEvent[]) aSelfAveragingEvent.ToArray(typeof(EFS_SelfAveragingEvent));
					#endregion SelfAveraging Process
					#region Final SelfAveraging Process
					if (m_EventsValProcess.IsRowsEventCalculated(rowSelfAverages))
					{
						Compounding();
						RateTreatement();
						UpdatingRow(pRowReset);
						pRowReset["IDSTCALCUL"] = StatusCalculFunc.CalculatedAndRevisable;
					}
					#endregion Final SelfAveraging Process
					#endregion SelfCompounding 
				}
				m_EventsValProcess.SetRowCalculated(pRowReset);
				m_EventsValProcess.SetRowStatus(pRowReset,Tuning.TuningOutputTypeEnum.OES);
				#endregion Process
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowReset);
				m_EventsValProcess.SetRowStatus(pRowReset,Tuning.TuningOutputTypeEnum.OEE);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region Methods
		#region Compounding
		private void Compounding()
		{
			try
			{
				#region observedRate after compounding
				EFS_CompoundingRate selfCompounding = new EFS_CompoundingRate(m_SelfCompoundingMethod,
					m_StartDate,endDate,m_SelfAveragingEvents);
				observedRate = selfCompounding.calculatedValue;
				#endregion observedRate after compounding
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Compounding
		#region Interpolating
		private void Interpolating()
		{
			try
			{
				EventsValParameterIRD parameter = ((EventsValProcessIRD)m_EventsValProcess).Parameters[
					m_EventsValProcess.paramInstrumentNo,m_EventsValProcess.paramStreamNo];
				#region observedRate1
				EFS_Interval interval1 = new EFS_Interval(parameter.Rate.Asset_PeriodMltp_Tenor,parameter.Rate.FpML_Enum_Period_Tenor,
					m_ObservedRateDate,Convert.ToDateTime(null));
				decimal observedRate1 = m_EventsValProcess.ReadQuote_RateIndex(interval1.offsetDate,m_IdAsset);
				#endregion observedRate1
				#region observedRate2
				EFS_Interval interval2 = new EFS_Interval(parameter.Rate2.Asset_PeriodMltp_Tenor,parameter.Rate2.FpML_Enum_Period_Tenor,
					m_ObservedRateDate,Convert.ToDateTime(null));
				decimal observedRate2 = m_EventsValProcess.ReadQuote_RateIndex(interval2.offsetDate,m_IdAsset2);
				#endregion observedRate2
				#region observedRate after interpolating
				EFS_Interpolating interpolating = new EFS_Interpolating(m_AccrualDate,observedRate1,interval1.offsetDate,
					observedRate2,interval2.offsetDate,m_EndPeriodDate,RoundingDirectionEnum.Nearest,m_RoundingPrecision);
				observedRate = interpolating.interpolatedRate;
				#endregion observedRate after interpolating
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Interpolating
		#region RateTreatement
		private void RateTreatement()
		{
			try
			{
				treatedRate = observedRate;
				if (m_RateTreatmentSpecified)
					treatedRate = EventsValProcessIRD.TreatedRate(m_RateTreatment,observedRate,m_StartDate,m_EndDate,m_PaymentFrequency);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion RateTreatement
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow)
		{
			try
			{
				pRow["VALORISATION"] = treatedRate;
				pRow["UNITTYPE"]     = EFS_UnitTypeEnum.Rate.ToString();
				//
				DataRow rowDetail    = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRow["ID"]));
				if (null != rowDetail)
					rowDetail["RATE"] = observedRate;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
		#endregion Methods
	}
	#endregion EFS_ResetEvent
	#region EFS_SelfAveragingEvent
	public class EFS_SelfAveragingEvent : SelfAveragingInfo
	{
		#region Variables
		protected EFS_SelfResetEvent[] m_SelfResetEvents;
		#endregion Variables
		#region Result Variables
		public decimal averagedRate;
		public decimal averagedAndTreatedRate;
		#endregion Result Variables
		#region Constructors
		public EFS_SelfAveragingEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowSelfAverage)
			:base(pAccrualDate,pEventsValProcess,pRowSelfAverage)
		{
			averagedAndTreatedRate = Convert.ToDecimal(pRowSelfAverage["VALORISATION"]);
		}
		public EFS_SelfAveragingEvent(DateTime pAccrualDate,EventsValProcessBase pEventsValProcess,DataRow pRowSelfAverage,
			DateTime pRateCutOffDate):base(pAccrualDate,pEventsValProcess,pRowSelfAverage,pRateCutOffDate)
		{
			try
			{
				#region Process
				#region SelfReset Process
				DataRow[] rowSelfResets = pRowSelfAverage.GetChildRows(m_EventsValProcess.DsEvents.ChildEvent);
				EFS_SelfResetEvent selfResetEvent;
				ArrayList aSelfResetEvent = new ArrayList();
				foreach (DataRow rowSelfReset in rowSelfResets)
				{
					if (m_EventsValProcess.IsRowMustBeCalculate(rowSelfReset))
					{
						m_EventsValProcess.SetRowCalculated(rowSelfReset);
						selfResetEvent = new EFS_SelfResetEvent(m_EventsValProcess,rowSelfReset ,m_IdAsset,m_RateCutOffDate);
						aSelfResetEvent.Add(selfResetEvent);
						if (false == m_EventsValProcess.IsRowEventCalculated(rowSelfReset))
							break;
					}
					else if (m_EventsValProcess.IsRowPrecededAccrual(rowSelfReset))
					{
						aSelfResetEvent.Add(new EFS_SelfResetEvent(m_EventsValProcess,rowSelfReset,pRateCutOffDate));
					}
				}
				m_SelfResetEvents = (EFS_SelfResetEvent[])aSelfResetEvent.ToArray(typeof(EFS_SelfResetEvent));
				#endregion SelfReset Process
				#region Final SelfAveraging Process
				if (m_EventsValProcess.IsRowsEventCalculated(rowSelfResets))
				{
					Averaging();
					Treating();
					UpdatingRow(pRowSelfAverage);
				}
				m_EventsValProcess.SetRowCalculated(pRowSelfAverage);
				#endregion Final SelfAveraging Process
				#endregion Process
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowSelfAverage);
				m_EventsValProcess.SetRowStatus(pRowSelfAverage,Tuning.TuningOutputTypeEnum.OEE);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region Methods
		#region Averaging
		private void Averaging()
		{
			try
			{
				EFS_Averaging averaging = new EFS_Averaging(this,m_SelfResetEvents);
				averagedRate   = averaging.averagedRate;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Averaging
		#region Treating
		private void Treating()
		{
			try
			{
				averagedAndTreatedRate = averagedRate;
				if (m_RateTreatmentSpecified)
					averagedAndTreatedRate = EventsValProcessIRD.TreatedRate(m_RateTreatment,averagedRate,startDate,endDate,
						m_PaymentFrequency);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion Treating
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow)
		{
			try
			{
				pRow["VALORISATION"] = averagedAndTreatedRate;
				pRow["UNITTYPE"]     = EFS_UnitTypeEnum.Rate.ToString();

				DataRow[] rowAssets = m_EventsValProcess.GetRowAsset(Convert.ToInt32(pRow["ID"]));
				if ((null != rowAssets) && (0 < rowAssets.Length))
					rowAssets[0]["IDASSET"] = m_IdAsset;
				//
				m_EventsValProcess.SetRowStatus(pRow,Tuning.TuningOutputTypeEnum.OES);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
		#endregion Methods
	}
	#endregion EFS_SelfAveragingEvent
	#region EFS_SelfResetEvent
	public class EFS_SelfResetEvent : SelfResetInfo
	{
		#region Result Variables
		public decimal observedRate;
		#endregion Result Variables
		#region Constructors
		public EFS_SelfResetEvent(EventsValProcessBase pEventsValProcess, DataRow pRowSelfReset,DateTime pRateCutOffDate)
			:base(pEventsValProcess,pRowSelfReset,pRateCutOffDate)
		{
			observedRate = Convert.ToDecimal(pRowSelfReset["VALORISATION"]);
		}
		public EFS_SelfResetEvent(EventsValProcessBase pEventsValProcess,DataRow pRowSelfReset,int pIdAsset,DateTime pRateCutOffDate)
			:base(pEventsValProcess,pRowSelfReset,pIdAsset,pRateCutOffDate)
		{
			try
			{
				#region Process
				observedRate = m_EventsValProcess.ReadQuote_RateIndex(fixingDate,m_IdAsset);
				UpdatingRow(pRowSelfReset);
				#endregion Process
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowSelfReset);
				m_EventsValProcess.SetRowStatus(pRowSelfReset,Tuning.TuningOutputTypeEnum.OEE);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow)
		{
			try
			{
				pRow["VALORISATION"] = observedRate;
				pRow["UNITTYPE"]     = EFS_UnitTypeEnum.Rate.ToString();
				m_EventsValProcess.SetRowCalculated(pRow);
				m_EventsValProcess.SetRowStatus(pRow,Tuning.TuningOutputTypeEnum.OES);
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
	}
	#endregion EFS_SelfResetEvent
	#region EFS_VariationNominalEvent
	public class EFS_VariationNominalEvent : VariationNominalInfo
	{
		#region Variables
		protected DataRow m_RowStream;
		protected DataRow m_RowNominal;
		protected DataRow m_RowNominalReference;
		protected DataRow m_RowPreviousNominal;
		#endregion Variables
		#region Result Variables
		public decimal    observedRate;
		public decimal    variationNominal;
		public decimal    nominal;
		public decimal    previousNominal;
		public decimal    nominalReference;
		public decimal    calculatedAmount;
		#endregion Result Variables
		#region Constructors
		public EFS_VariationNominalEvent(DateTime pAccrualsDate,EventsValProcessIRD pEventsValProcess,DataRow pRowVariation)
			:base(pAccrualsDate,pEventsValProcess,pRowVariation)
		{
			try
			{
				#region Nominal & NominalReference
				m_RowStream           = pRowVariation.GetParentRow(m_EventsValProcess.DsEvents.ChildEvent);
				m_RowNominal          = m_EventsValProcess.GetCurrentNominal(m_StartDate);
				m_RowPreviousNominal  = m_EventsValProcess.GetPreviousNominal(m_RowNominal);
				m_RowNominalReference = m_EventsValProcess.CurrentNominalReference(m_StartDate);

				nominal             = Convert.IsDBNull(m_RowNominal["VALORISATION"])?0:Convert.ToDecimal(m_RowNominal["VALORISATION"]);
				nominalReference    = Convert.ToDecimal(m_RowNominalReference["VALORISATION"]);

				if (m_RowPreviousNominal.Equals(m_RowNominal))
					previousNominal = nominal;
				else if (false == Convert.IsDBNull(m_RowPreviousNominal["VALORISATION"]))
					previousNominal = Convert.ToDecimal(m_RowPreviousNominal["VALORISATION"]);
				else
				{
					pRowVariation["IDA_PAY"] = Convert.ToInt32(m_RowStream["IDA_PAY"]);
					pRowVariation["IDA_REC"] = Convert.ToInt32(m_RowStream["IDA_REC"]);
					string level = (m_FixingDate <= OTCmlHelper.GetRDBMSDtSys(m_EventsValProcess.ConnectionString))?LevelStatusTools.LevelAlert:LevelStatusTools.LevelWarning;
					LevelStatus levelStatus = new LevelStatus(level,LevelStatusTools.StatusError,LevelStatusTools.CodeReturnDataNotFound);
					throw new OTCmlException("EFS_VariationNominalEvent..ctor",levelStatus,
						"Previous Nominal uncalculated [{0}] Currency[{1}/{2}]",
						m_FixingDate.ToShortDateString(),m_CurrencyReference,m_Currency);
				}
				#endregion Nominal & NominalReference

				#region Process
				
				#region observed Fixing value from the fixing prices referential
				#region KeyQuote
				KeyQuote keyQuote = new KeyQuote();
				keyQuote.Time     = m_FixingDate;
				#endregion KeyQuote
				#region KeyAssetFXRate
				KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate();
				keyAssetFXRate.IdC1           = m_CurrencyReference;
				keyAssetFXRate.IdC2           = m_Currency;
				#endregion KeyAssetFXRate
				observedRate     = m_EventsValProcess.ReadQuote_FXRate(keyQuote,keyAssetFXRate);
				calculatedAmount = nominalReference * observedRate;
				#endregion observed Fixing value from the fixing prices referential


				#region Nominal Calculation
				int payer         = Convert.ToInt32(m_RowStream["IDA_PAY"]);
				int payer_Book    = Convert.IsDBNull(m_RowStream["IDB_PAY"])?0:Convert.ToInt32(m_RowStream["IDB_PAY"]);
				int receiver      = Convert.ToInt32(m_RowStream["IDA_REC"]);
				int receiver_Book = Convert.IsDBNull(m_RowStream["IDB_REC"])?0:Convert.ToInt32(m_RowStream["IDB_REC"]);
				if (m_RowPreviousNominal.Equals(m_RowNominal))
				{
					#region First Nominal
					UpdatingRow(pRowVariation,calculatedAmount,observedRate,receiver,receiver_Book,payer,payer_Book,nominalReference);
					#endregion First Nominal
				}
				else
				{
					#region Others Nominal
					decimal diffNominal = calculatedAmount - previousNominal;
					if (1 == Math.Sign(diffNominal))
					{
						#region Increase 
						UpdatingRow(pRowVariation,diffNominal,0,receiver,receiver_Book,payer,payer_Book,nominalReference);
						#endregion Increase 
					}
					else
					{
						#region Amortization
						UpdatingRow(pRowVariation,Math.Abs(diffNominal),0,payer,payer_Book,receiver,receiver_Book,nominalReference);
						#endregion Amortization
					}
					#endregion Others Nominal
				}
				#endregion Nominal Calculation
				#endregion Process
			}
			catch (OTCmlException otcmlException)
			{
				m_EventsValProcess.ResetRowCalculated(pRowVariation);
				m_EventsValProcess.ResetRowCalculated(m_RowNominal);
				//
				m_EventsValProcess.SetRowStatus(pRowVariation,Tuning.TuningOutputTypeEnum.OEE);
				m_EventsValProcess.SetRowStatus(m_RowNominal,Tuning.TuningOutputTypeEnum.OEE);
				throw otcmlException;
			}
			catch (Exception ex){throw ex;}
		}
		#endregion Constructors
		#region Methods
		#region UpdatingRow
		private void UpdatingRow(DataRow pRow,decimal pVariationNominal,decimal pObservedRate,
			int pPayer,int pPayer_Book,int pReceiver,int pReceiver_Book,decimal pNominalReference)
		{
			try
			{
				pRow["UNIT"]                 = m_Currency;
				pRow["UNITTYPE"]             = EFS_UnitTypeEnum.Currency.ToString();
				pRow["VALORISATION"]         = pVariationNominal;
				//
				pRow["IDA_PAY"]              = pPayer;
				if (0 < pPayer_Book)
					pRow["IDB_PAY"]          = pPayer_Book;
				//
				pRow["IDA_REC"]              = pReceiver;
				if (0 < pReceiver_Book)
					pRow["IDB_REC"]          = pReceiver_Book;

				m_RowNominal["VALORISATION"] = calculatedAmount;
				m_RowNominal["UNIT"]         = m_Currency;
				m_RowNominal["UNITTYPE"]     = EFS_UnitTypeEnum.Currency.ToString();

				DataRow rowDetail    = m_EventsValProcess.GetRowDetail(Convert.ToInt32(pRow["ID"]));
				if (null != rowDetail)
				{
					rowDetail["RATE"]           = observedRate;
					rowDetail["NOTIONALAMOUNT"] = pNominalReference;
				}
				
				m_EventsValProcess.SetRowCalculated(pRow);
				m_EventsValProcess.SetRowCalculated(m_RowNominal);
				
				m_EventsValProcess.SetRowStatus(pRow, TuningOutputTypeEnum.OES);      
				m_EventsValProcess.SetRowStatus(m_RowNominal, TuningOutputTypeEnum.OES);      
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion UpdatingRow
		#endregion Methods
	}
	#endregion EFS_VariationNominalEvent

}

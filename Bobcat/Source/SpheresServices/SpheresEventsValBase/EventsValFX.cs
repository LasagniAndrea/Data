#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.Tuning;


using EfsML;
using EfsML.Enum;

using FpML.Enum;

#endregion Using Directives

namespace EFS.Process
{
	/// <summary>
	/// Description résumée de EventsValProcessFX.
	/// </summary>
	#region EventsValProcessFX
	public class EventsValProcessFX : EventsValProcessBase
	{
		#region Members
		private CommonValParametersFX  m_Parameters;
		#endregion Members
		
		#region Accessors
		#region Parameters
		public override CommonValParametersFX ParametersFX
		{
			get {return m_Parameters;}
		}
		#endregion Parameters
		#region RowSettlement
		public  DataRow[] RowSettlement
		{
			get
			{
				return DsEvents.DtEvent.Select("IDPARENT=" + m_ParamIdT.Value.ToString() + 
					" and EVENTCODE=" + DataHelper.SQLString(EventCodeFunc.Termination) +
					" and EVENTTYPE=" + DataHelper.SQLString(EventTypeFunc.SettlementCurrency),"ID");
			}
		}
		#endregion RowSettlement
		#region RowExchangeCurrencyFixing
		public  DataRow[] RowExchangeCurrencyFixing
		{
			get
			{
				return DsEvents.DtEvent.Select("IDPARENT=" + m_ParamIdT.Value.ToString() + 
					" and EVENTTYPE in (" + DataHelper.SQLString(EventTypeFunc.Currency1) + "," + 
					DataHelper.SQLString(EventTypeFunc.Currency2) + ")","ID");
			}
		}
		#endregion RowExchangeCurrencyFixing
		#endregion Accessors
		
		#region Constructors
		public EventsValProcessFX(EventsValProcess pProcess,DataSetTrade pDsTrade,EFS_TradeLibrary pTradeLibrary)
			:base(pProcess, pDsTrade, pTradeLibrary)
		{
			m_Parameters = new CommonValParametersFX();
		}
		#endregion Constructors
		
		#region Methods
		
		#region IsRowsEventCalculated
		public  override bool IsRowsEventCalculated(DataRow[] pRows)
		{
			foreach (DataRow row in pRows)
			{
				DateTime startDate = Convert.ToDateTime(row["DTSTARTUNADJ"]);
				DateTime endDate   = Convert.ToDateTime(row["DTENDUNADJ"]);

				if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
					return false;
			}
			return true;
		}
		#endregion IsRowsEventCalculated
		#region IsRowMustBeCalculate
		public override bool IsRowMustBeCalculate(DataRow pRow)
		{
			try
			{
				string eventCode = pRow["EVENTCODE"].ToString();
				string eventType = pRow["EVENTTYPE"].ToString();
				//
				if (null != m_EventsValMQueue.quote)
				{
					if (null != m_Quote_FxRate)
					{
						if (EventCodeAndEventTypeFunc.IsFxRateReset(eventCode,eventType))
							return (IsRowHasFixingEvent(pRow));
						else
						{
							DataRow[] drChild = pRow.GetChildRows(this.DsEvents.ChildEvent);
							if (ArrFunc.IsFilled(drChild))
							{
								eventCode = drChild[0]["EVENTCODE"].ToString();
								eventType = drChild[0]["EVENTTYPE"].ToString();
								if (EventCodeAndEventTypeFunc.IsFxRateReset(eventCode,eventType))
									return (IsRowHasFixingEvent(drChild[0]));
							}
						}
					}
				}
				else
					return true;
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValProcessFX..IsRowMustBeCalculate",ex);}
			return false;
		}
		#endregion IsRowMustBeCalculate

		#region public Valorize
		public override Cst.ErrLevel Valorize(ref ArrayList pOTCmlException)
		{
			try
			{
				Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
				pOTCmlException  = new ArrayList();
				bool isError     = false; 
				//
				#region ExchangeCurrency with fixing Process
				foreach (DataRow rowExchangeCurrencyFixing in RowExchangeCurrencyFixing)
				{
					isError                   = false;
					m_ParamInstrumentNo.Value = Convert.ToInt32(rowExchangeCurrencyFixing["INSTRUMENTNO"]);
					m_ParamStreamNo.Value     = Convert.ToInt32(rowExchangeCurrencyFixing["STREAMNO"]);
					
					if (IsRowMustBeCalculate(rowExchangeCurrencyFixing))
					{
						EFS_ExchangeCurrencyFixingEvent exchangeCurrencyFixingEvent = null;
						try
						{
							ParametersFX.Add(m_ConnectionString,m_tradeLibrary,rowExchangeCurrencyFixing);
							exchangeCurrencyFixingEvent = new EFS_ExchangeCurrencyFixingEvent(this,rowExchangeCurrencyFixing);
						}
						catch (OTCmlException otcmlException)
						{
							isError = true;
							if (otcmlException.IsLevelAlert)
							{
								ret =  Cst.ErrLevel.UNDEFINED;
								pOTCmlException.Add(otcmlException);
							}
						}
						catch (Exception ex) 
						{
							isError = true;
							throw new OTCmlException("EFS.EventsValuationFX..Valorize (RowExchangeCurrencyWithFixing)",ex);
						}
						finally
						{
							if (null != exchangeCurrencyFixingEvent)
							{
								int idE = Convert.ToInt32(rowExchangeCurrencyFixing["ID"]);
								ret = Update(idE,isError);
							}
						}
					}
				}
				#endregion ExchangeCurrency with fixing Process
				//
				#region Payment Process
				foreach (DataRow rowSettlement in RowSettlement)
				{
					isError                   = false;
					m_ParamInstrumentNo.Value = Convert.ToInt32(rowSettlement["INSTRUMENTNO"]);
					m_ParamStreamNo.Value     = Convert.ToInt32(rowSettlement["STREAMNO"]);
					if (IsRowMustBeCalculate(rowSettlement))
					{
						EFS_SettlementEvent settlementEvent = null;
						try
						{
							ParametersFX.Add(m_ConnectionString,m_tradeLibrary,rowSettlement);
							settlementEvent = new EFS_SettlementEvent(this, rowSettlement);
						}
						catch (OTCmlException otcmlException)
						{
							isError = true;
							if (otcmlException.IsLevelAlert)
							{
								ret =  Cst.ErrLevel.UNDEFINED;
								pOTCmlException.Add(otcmlException);
							}
						}
						catch (Exception ex) 
						{
							isError = true;
							throw new OTCmlException("EFS.EventsValuationFX..Valorize (RowSettlement)",ex);
						}
						finally	
						{
							int idE = Convert.ToInt32(rowSettlement["ID"]);
							ret = Update(idE,isError);
						}
					}
				}
				#endregion Payment Process
				//
				return ret;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValuationFX..Valorize",ex);}
		}	
		#endregion Valorize
		#endregion Methods
	}
	#endregion EventsValProcessFX
}

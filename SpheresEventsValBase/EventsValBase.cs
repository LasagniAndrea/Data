#region Using Directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Globalization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.OTCmlStatus;
using EFS.Tuning;
//
using EfsML;
using EfsML.Enum;
using EfsML.Extended;
//
using FpML.Enum;
using FpML.Shared;
#endregion Using Directives

namespace EFS.Process
{
	#region EventsValProcessBase
	public abstract class EventsValProcessBase : CommonValProcessBase
	{
		#region Members
		protected  EventsValProcess  m_EventsValProcess;
		protected  EventsValMQueue   m_EventsValMQueue;
		#endregion Members
		
		#region Accessors 
		#region CommonValDate
		public override DateTime CommonValDate
		{
			get 
			{
				if (Queue.dateSpecified)
					return Queue.date;
				return DateTime.MinValue;
			}
		}
		#endregion CommonValDate
		#region EventsValMQueue
		public EventsValMQueue EventsValMQueue
		{
			set 
			{
				m_Quote_FxRate     = null;
				m_Quote_RateIndex  = null;
				m_EventsValMQueue = value;
				if (null != m_EventsValMQueue.quote)
				{
					Type tQuote = m_EventsValMQueue.quote.GetType();
					if (tQuote.Equals(typeof(Quote_FxRate)))
						m_Quote_FxRate    = (Quote_FxRate) m_EventsValMQueue.quote;
					else if (tQuote.Equals(typeof(Quote_RateIndex)))
						m_Quote_RateIndex = (Quote_RateIndex) m_EventsValMQueue.quote;
				}
			}
		}
		#endregion EventsValMQueue
		#region Process
		public override ProcessBase Process
		{
			get {return (ProcessBase) m_EventsValProcess;}
		}
		#endregion Process
		#region Queue
		public override MQueueBase Queue
		{
			get {return (MQueueBase) m_EventsValMQueue;}
		}
		#region MarkToMarketGenMQueue
		/*
		public MarkToMarketGenMQueue Queue2
		{
			get {return m_MarkToMarketGenMQueue;}
			set 
			{
				m_Quote_FxRate          = null;
				m_Quote_RateIndex       = null;
				m_MarkToMarketGenMQueue = value;
				if (null != m_MarkToMarketGenMQueue.Quote)
				{
					Type tQuote = m_MarkToMarketGenMQueue.Quote.GetType();
					if (tQuote.Equals(typeof(Quote_FxRate)))
						m_Quote_FxRate    = (Quote_FxRate) m_MarkToMarketGenMQueue.Quote;
					else if (tQuote.Equals(typeof(Quote_RateIndex)))
						m_Quote_RateIndex = (Quote_RateIndex) m_MarkToMarketGenMQueue.Quote;
				}
			}
		}
		*/
		#endregion MarkToMarketGenMQueue
		#endregion Queue
		#endregion Accessors
		
		#region Constructors
		public EventsValProcessBase(EventsValProcess pEventsValProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary) 
			:base(pEventsValProcess.mQueue.ConnectionString,pDsTrade,pTradeLibrary)						
		{
			try
			{
				m_EventsValProcess = pEventsValProcess;
				EventsValMQueue    = (EventsValMQueue)m_EventsValProcess.mQueue;  
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.EventsValProcessBase..ctor",ex);}		
		}
		#endregion Constructors
		
		#region Methods 
	
		#region public virtual Valorize
		public override Cst.ErrLevel Valorize(ref ArrayList pOTCmlException)
		{
			return Cst.ErrLevel.UNDEFINED;
		}
		#endregion virtual Valorize
		#endregion Methods 
		
	}
	#endregion EventsValBase
}

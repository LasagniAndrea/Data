#region Using Directives
using System;

using System.Collections;
using System.Data;
using System.IO;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EfsML;
#endregion Using Directives


namespace EFS.Process
{
	#region class EventsValProcess (Process EventsValProcess)
	public class EventsValProcess : ProcessBase 
	{
		#region Members
		public  EventsValMQueue         eventsValMQueue;
		#endregion Members
		//
		#region Constructor
		public EventsValProcess(MQueueBase pMQueue,AppInstance pAppInstance):base(pMQueue,pAppInstance) 
		{
			try
			{
				eventsValMQueue = (EventsValMQueue)pMQueue;
				//((MQueueBase) eventsValMQueue).date = eventsValMQueue.accrualDate.AddDays(-1);
				ProcessStart(); 
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EventsValProcess..ctor",ex);}
		}
		#endregion Constructor
		//
		#region protected override SelectTrades
		protected override Cst.ErrLevel SelectTrades()
		{
			Cst.ErrLevel ret          = Cst.ErrLevel.SUCCES;
			IDbDataParameter paramIdT = null;
			ArrayList        aParameters   = new ArrayList(); 
			//
			if (eventsValMQueue.idSpecified) 
			{
				paramIdT       = new EFSParameter(CS ,"IDT",DbType.Int32).DataParameter;			
				paramIdT.Value = eventsValMQueue.id;
				aParameters.Add(paramIdT); 
			}
			//			
			try
			{
				StrBuilder sqlSelect  =  new StrBuilder(SQLCst.SELECT_DISTINCT + @"e.IDT As IDPARENT" + Cst.CrLf);
				sqlSelect             += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
				//
				SQLWhere sqlWhere    = new SQLWhere(); 
				if (eventsValMQueue.idSpecified) 
					sqlWhere.Append( @"(e.IDT=@IDT)"); 
				//
				sqlSelect += sqlWhere.ToString() ;
				//
				string sqlSelectTrade = sqlSelect.ToString();
				//
				IDbDataParameter[] parameters  = null;
				if (ArrFunc.IsFilled(aParameters))
					parameters = (IDbDataParameter[])aParameters.ToArray(aParameters[0].GetType());
				//
				dsTrades   = DataHelper.ExecuteDataset(CS ,CommandType.Text,sqlSelectTrade,paramIdT);
				//
				#region DataSet Initialize
				dsTrades.DataSetName = "dsTrades";
				DataTable dtTrades   = dsTrades.Tables[0];
				dtTrades.TableName   = "dtTrades";
				#endregion DataSet Initialize
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("TradeActionGenProcess.SelectTrades",ex);}		
			//			
			return ret;
		}
		#endregion SelectTrades
		//
		#region protected override ExecuteSpecificProcess
		protected override Cst.ErrLevel ExecuteSpecificProcess(ref ArrayList pOTCmlException)
		{
			try
			{
				Cst.ErrLevel codeReturn =   Cst.ErrLevel.UNDEFINED;	
				//
				#region Select Trade and deserialization
				DataSetTrade dsTrade = new DataSetTrade(CS,currentId);
				EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(dsTrade.Document,CS);
				#endregion Select Trade and deserialization
				//
				#region Valuation
				EventsValProcessBase eventsValProcessBase = null;
				if (Tools.IsIRD(tradeLibrary.Product.GetType()))
					eventsValProcessBase = new EventsValProcessIRD(this,dsTrade,tradeLibrary);
				else if (Tools.IsFx(tradeLibrary.Product.GetType()))
					eventsValProcessBase = new EventsValProcessFX(this,dsTrade,tradeLibrary);
				//
				if (null != eventsValProcessBase)
					codeReturn = eventsValProcessBase.Valorize(ref pOTCmlException);  
				#endregion
				//
				return codeReturn;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EventsValProcess.ExecuteSpecificProcess",ex);}		
		}
		#endregion protected override ExecuteSpecificProcess
	}
	#endregion class EventsValProcess 
}

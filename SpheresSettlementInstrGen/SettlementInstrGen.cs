#region Using Directives
using System;
using System.Text;
using System.Collections;
using System.Data;
using System.IO;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.Settlement;
using EfsML;
#endregion Using Directives

namespace EFS.Process
{
	#region class SettlementInstrGenProcess (Process Settlement)
	public class SettlementInstrGenProcess : ProcessBase 
	{
		#region Members
		public  SettlementInstrGenMQueue settlementInstrGenMQueue;
		public  DataSet                  dsEvents;
		#endregion Members

		#region Constructor
		public SettlementInstrGenProcess(MQueueBase pMQueue,AppInstance pAppInstance,ref Cst.ErrLevel pRet):base(pMQueue,pAppInstance) 
		{
			Cst.ErrLevel retGen = Cst.ErrLevel.SUCCES;
			try
			{
				settlementInstrGenMQueue = (SettlementInstrGenMQueue)pMQueue;
				
				#region Creating ProcessLogMaster
				m_ProcessLogMaster = new ProcessLog(MQueue.connectionString,ProcessTypeEnum.SpheresSettlementInstrGen,Cst.StatusProcess.NA,AppInstance, 0,
                                                    "Posted Message for Settlement instructions generation",
					                                FileInfo.Name,FileInfo.DirectoryName);
				m_ProcessLogMaster.WriteHeader();
				#endregion Creating ProcessLogMaster
				
				#region DataSet Event
				retGen   = SelectEvent();
				#endregion DataSet Event

				if (Cst.ErrLevel.SUCCES == retGen)
				{
					int lastIdT = 0;
					#region Reading rows selection
					DataTable dtTrades = dsEvents.Tables["dtEvents"]; 
					DataRow[] rows    =  dtTrades.Select(null,"IDPARENT",DataViewRowState.OriginalRows);
					foreach (DataRow dr in rows)
					{
						ProcessLog processLog = null;
						CurrentIdT            = Convert.ToInt32(dr["IDPARENT"].ToString());
						//
						if (lastIdT != CurrentIdT)
						{
							Cst.ErrLevel ret          = Cst.ErrLevel.SUCCES;
							ArrayList otcmlExceptions = new ArrayList();
							
							#region ProcessLog
							processLog = new ProcessLog(MQueue.connectionString,ProcessTypeEnum.SpheresSettlementInstrGen,
								                        Cst.StatusProcess.NA,AppInstance,CurrentIdT,settlementInstrGenMQueue);
							#endregion ProcessLog
							
							#region Tracker
							m_Tracker      = new Tracker(MQueue.connectionString,TrackerLevelEnum.INFO,
								                         TrackerSourceEnum.SpheresSettlementInstrGen,TrackerStatusEnum.PROGRESS);
							m_Tracker.User = new TrackerUser(1,OTCmlHelper.GetDtSys(),AppInstance.HostName);
							m_Tracker.Data = new TrackerData(Cst.OTCml_TBL.TRADE.ToString(),CurrentIdT,
								new string[1]{Ressource.GetString(ProcessTypeEnum.SpheresSettlementInstrGen.ToString())});
							m_Tracker.Insert();
							#endregion Tracker
							
							try
							{
								#region Scan compatibility
								if (Cst.ErrLevel.SUCCES == ret)
									//Scan the compatibilty of Trade with PROCESSTUNING parameters for Ignored or Processed
									ret = this.ScanCompatibility_Trade(); 
								#endregion 
								//
								#region LockTrade 
								if (Cst.ErrLevel.SUCCES == ret)
									ret = this.LockTrade(); 
								#endregion LockTrade 
								//
								#region Select Trade and deserialization
								DataSetTrade     dsTrade      = null;
								EFS_TradeLibrary tradeLibrary = null;
								if (Cst.ErrLevel.SUCCES == ret)
								{
									dsTrade      = new DataSetTrade(CS,CurrentIdT);
									tradeLibrary = new EFS_TradeLibrary(dsTrade.Document,CS);
								}
								#endregion Select Trade and deserialization
								//
								if (Cst.ErrLevel.SUCCES == ret)
								{
									//
									SettlementInstrTradeGen   stl = new SettlementInstrTradeGen(this,dsTrade,tradeLibrary,ref ret,ref otcmlExceptions); 
									//
									if (0 < otcmlExceptions.Count)
									{
										ret = Cst.ErrLevel.MISCELLANEOUS;
										processLog.AddDetail(Cst.StatusProcess.ERROR,otcmlExceptions);
										m_Tracker.Level  = TrackerLevelEnum.ALERT;
										m_Tracker.Status = TrackerStatusEnum.ERROR;
									}
									else
									{
										m_Tracker.Status = TrackerStatusEnum.SUCCESS;
										m_Tracker.Level  = TrackerLevelEnum.SUCCESS;
									}
								}
							}
							catch (OTCmlException ex)
							{
								ret = Cst.ErrLevel.BUG;
								processLog.AddDetail(Cst.StatusProcess.ERROR,ex);
								m_Tracker.Status = TrackerStatusEnum.ERROR;
								m_Tracker.Level  = TrackerLevelEnum.ALERT;
								m_Tracker.Data   = new TrackerData(Cst.OTCml_TBL.TRADE.ToString(),CurrentIdT,ex);
							}
							catch (Exception ex) 
							{
								ret = Cst.ErrLevel.BUG;
								processLog.AddDetail(Cst.StatusProcess.ERROR,ex.Message,ex.Source);
								m_Tracker.Status = TrackerStatusEnum.ERROR;
								m_Tracker.Level  = TrackerLevelEnum.ALERT;
								m_Tracker.Data   = new TrackerData(Cst.OTCml_TBL.TRADE.ToString(),CurrentIdT,ex);
							}
							finally
							{
								Cst.StatusProcess statusProcess   = this.ProcessTradeTerminate(ret);
								processLog.header.IdStProcess = statusProcess;
								processLog.Write();
								m_ProcessLogMaster.AddDetail(processLog.header.IdStProcess,"Settlement instructions generation on trade n°" + CurrentIdT.ToString());
								m_Tracker.User.dt   = OTCmlHelper.GetDtSys();
								m_Tracker.IdProcess = processLog.header.IdProcess;
								m_Tracker.Update();
								lastIdT = CurrentIdT;
								// 
								if (Cst.ErrLevel.SUCCES == retGen && Cst.ErrLevel.SUCCES != ret )
									retGen = ret;
							}
						}
					}
					#endregion Reading rows selection
				}

				#region Writing processLog
				m_ProcessLogMaster.WriteDetail();
				m_ProcessLogMaster.header.IdStProcess = Cst.ErrLevel.SUCCES==retGen?Cst.StatusProcess.SUCCESS:Cst.StatusProcess.ERROR;
				m_ProcessLogMaster.UpdateStatusHeader();
				#endregion Writing processLog

			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.SettlementInstrGenProcess..ctor",ex);}
			
			pRet = retGen;
		}
		#endregion Constructor

		#region private SelectEvent
		private Cst.ErrLevel SelectEvent()
		{
			Cst.ErrLevel ret          = Cst.ErrLevel.SUCCES;
			IDbDataParameter paramIdT = null;
			//
			if (settlementInstrGenMQueue.idTSpecified) 
			{
				paramIdT       = new EFSParameter(CS ,"IDT",DbType.Int32).DataParameter;			
				paramIdT.Value = settlementInstrGenMQueue.idT;
			}
			//			
			try
			{
				string cs        = settlementInstrGenMQueue.connectionString;
				string SQLSelect = SQLCst.SELECT_DISTINCT + @"e.IDT As IDPARENT" + Cst.CrLf;
				SQLSelect       += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
				SQLSelect		+= SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec ";
				SQLSelect		+= SQLCst.ON + "(ec.IDE = e.IDE)" + Cst.CrLf;
				SQLSelect		+= SQLCst.AND + "(ec.EVENTCLASS =" + DataHelper.SQLString(EventClassFunc.Settlement) + ")" + Cst.CrLf;
				//
				string SQLWhere  = string.Empty; 
				if (settlementInstrGenMQueue.idTSpecified) 
					SQLWhere = SQLCst.WHERE + @"(e.IDT=@IDT)" + Cst.CrLf;
				//
				SQLSelect += SQLWhere;
				//
				SQLSelect = DataHelper.ReplaceVarPrefix(cs, SQLSelect);
				dsEvents  = DataHelper.ExecuteDataset(cs,CommandType.Text,SQLSelect,paramIdT);
				//
				#region DataSet Initialize
				dsEvents.DataSetName = "dsEvents";
				DataTable dtTrades   = dsEvents.Tables[0];
				dtTrades.TableName   = "dtEvents";
				#endregion DataSet Initialize
				
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.SettlementInstructionGenProcess..SelectEvent",ex);}		
			//			
			return ret;
		}
		#endregion SelectEvent
	}
	#endregion class SettlementInstrGenProcess 
}

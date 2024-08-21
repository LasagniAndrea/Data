#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Text;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.Restriction; 
using EFS.Tuning; 

using EfsML;
using EfsML.Enum;

using FpML.Enum;
#endregion Using Directives

namespace EFS.Process.SettlementMsgGen
{
	#region public abstract class ESRGenProcessBase
	public abstract class ESRGenProcessBase : ProcessBase 
	{
		#region Members
		private  ESRGenMQueueBase      esrGenMQueue;
		private  RestrictionElement    restrictionEvent;
		#endregion Members
		//
		#region Constructor
		public ESRGenProcessBase(MQueueBase pMQueue,AppInstance pAppInstance):base(pMQueue,pAppInstance) 
		{
			try
			{
				esrGenMQueue = (ESRGenMQueueBase)pMQueue;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRGenProcessBase.ctor",ex);}
		}
		#endregion Constructor
		//
		#region protected override SelectDatas
		protected override Cst.ErrLevel SelectDatas()
		{
			try
			{
				QueryParameters  queryParamers = GetQueryEvent();   
				Cst.ErrLevel ret            = Cst.ErrLevel.SUCCES;
				//
				StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT + @"e.IDDATA As IDPARENT" + Cst.CrLf);
				sqlSelect           += SQLCst.FROM + Cst.CrLf;
				sqlSelect           += "(" + queryParamers.query + ") e";
				//				
				_dsDatas = DataHelper.ExecuteDataset(cs,CommandType.Text,sqlSelect.ToString(),queryParamers.parameters.GetArrayDbParameter());
				//
				return ret;
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRGenProcessBase.SelectDatas",ex);}		
		}
		#endregion SelectTrades
		//
		#region protected override ProcessExecuteSpecific
		protected override Cst.ErrLevel ProcessExecuteSpecific(ref ArrayList pOTCmlException)
		{
			try
			{
				Cst.ErrLevel codeReturn =   Cst.ErrLevel.SUCCES;	
				
				SetRestrictionEvent();
				//Alime
				SetESR(); 
				//
				return codeReturn;
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRGenProcessBase.ProcessExecuteSpecific",ex);}		
		}
		#endregion
		//
		#region protected virtual GetQueryAggregateEvent
		public virtual QueryParameters GetQueryAggregateEvent()
		{
			return null;
		}	
		#endregion
		//
		#region public virtual GetQueryEvent
		public  virtual QueryParameters GetQueryEvent()
		{
			return null;
		}
		public virtual  QueryParameters GetQueryEvent( bool pIsForRestrict )
		{
			return null;
		}
		
		#endregion public virtual QueryParameters
		//
		#region protected GetMQueueDataParameters
		protected DataParameters GetMQueueDataParameters()
		{
			try
			{
				DataParameters  dataParameters = new DataParameters(); 
				//
				if (esrGenMQueue.parametersSpecified) 
				{
					if ( null != esrGenMQueue.GetObjectValueParameterById(ESRGenMQueueBase.PARAM_DTESR))
					{
						dataParameters.Add(new DataParameter(cs ,"DTESR",DbType.Date));			
						dataParameters["DTESR"].Value =  Convert.ToDateTime(esrGenMQueue.GetDateTimeValueParameterById(ESRGenMQueueBase.PARAM_DTESR));
					}
					if ( null != esrGenMQueue.GetObjectValueParameterById(ESRGenMQueueBase.PARAM_IDA))
					{
						dataParameters.Add(DataParameter.GetParameter(cs ,DataParameter.ParameterEnum.IDA));			
						dataParameters["IDA"].Value =  Convert.ToInt32 (esrGenMQueue.GetIntValueParameterById(ESRGenMQueueBase.PARAM_IDA));
					}
					if ( null != esrGenMQueue.GetObjectValueParameterById(ESRGenMQueueBase.PARAM_IDA2))
					{
						dataParameters.Add(new DataParameter(cs,"IDA2", DbType.Int32));
						dataParameters["IDA2"].Value =  Convert.ToInt32 (esrGenMQueue.GetIntValueParameterById(ESRGenMQueueBase.PARAM_IDA2));
					}
				}
				//
				return dataParameters;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRGenProcessBase.GetMQueueDataParameters",ex);}
		}
		#endregion
		//
		#region protected GetMqueueSqlWhere
		protected SQLWhere GetMqueueSqlWhere()
		{
			try
			{
				SQLWhere  sqlWhere = new SQLWhere(); 
				//
				 DataParameters parameters = GetMQueueDataParameters();
				
				if (parameters.Contains("DTESR"))			
					sqlWhere.Append("e.DTESR=@DTESR");
				if (parameters.Contains("IDA"))			
					sqlWhere.Append("e.IDA=@IDA");
				if (parameters.Contains("IDA2"))			
					sqlWhere.Append("e.IDA=@IDA2");
				//
				return sqlWhere; 
			
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRGenProcessBase.GetMqueueSqlWhere",ex);}
		}
		#endregion
		//		
		#region private DeleteESR
		/// <summary>
		/// Suppression des ESRs qui portent sur les évènements retenus par le Traitement
		/// Rappel Ces ESRs n'ont pas été utlisés par un message (MSO)
		/// </summary>
		private void DeleteESR(IDbTransaction pDbTransaction)
		{
			try
			{
				DataParameters dataParameters = new DataParameters();
				dataParameters.Add( DataParameter.GetParameter(cs,DataParameter.ParameterEnum.SESSIONID),appInstance.SessionId);          
				dataParameters.Add( new DataParameter(cs, "CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),Cst.OTCml_TBL.EVENT.ToString());
				//
				StrBuilder query = new StrBuilder();
				if (false)
				{
					StrBuilder subquery = new StrBuilder();
					subquery += SQLCst.SELECT_DISTINCT + "esr.IDESR" + Cst.CrLf; 
					subquery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ESR +" esr"  + Cst.CrLf; 
					subquery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ESRDET.ToString() + " esrdet on esrdet.IDESR=esr.IDESR" + Cst.CrLf; 
					subquery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT.ToString() + " r " + Cst.CrLf; 
					subquery +=	"on r.ID=esrdet.IDE And r.SESSIONID=@SESSIONID And r.CLASS=@CLASS";       
					//	
					query += SQLCst.DELETE_DBO  + "ESR" + Cst.CrLf; 
					query += SQLCst.WHERE + "IDESR in (" + subquery.ToString() + ")";  
				}
				else
				{
					// Syntaxe Plus Rapide
					query += SQLCst.DELETE_DBO + "ESR" + Cst.CrLf; 
					query += SQLCst.WHERE + SQLCst.EXISTS + Cst.CrLf;  
					query += "(select 1 " + SQLCst.FROM_DBO + "ESRDET esrdet" + Cst.CrLf;
					query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT.ToString() + " r " + Cst.CrLf; 
					query += "on r.ID=esrdet.IDE And r.SESSIONID=@SESSIONID And r.CLASS=@CLASS" + Cst.CrLf; 
					query += SQLCst.WHERE + "esrdet.IDESR=ESR.IDESR)"  ;
				}
				//
				DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, query.ToString(),dataParameters.GetArrayDbParameter());            
				//	
			}	
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRGenProcessBase.DeleteESR",ex);}
		}
		#endregion
		//
		#region private SetESR
		/// <summary>
		/// Alimentation de la table ESR à partir des évènements retenus (voir restrictionEvent)
		/// Les évènements retenus sont sont ceux issus de la vue VW_EVENTESR après application des restrictions (paramètres Mqueue)
		/// Les évènements doivent être compatibles vis à vis de processTuning
		/// Les évènements ne doivent pas être impliqués dans un ESR lui même impliqué dans un message (1 MSO)
		/// </summary>
		private void SetESR()
		{
			IDbTransaction  dbTransaction = null;
			IDataReader dr = null;
			bool isOk =true;
			try
			{
				dbTransaction  = DataHelper.BeginTran(cs);
				DateTime dtSys = OTCmlHelper.GetRDBMSDtSys(cs); 
				//
				DeleteESR(dbTransaction);
				//
				dr = GetEsr();
				//
				ArrayList   lds = new ArrayList(); 
				DatasetStlMsg ds  = null; 
				DateTime dtCurrent = DateTime.MinValue; 
				while (dr.Read())
				{
					bool isNewDate   = (null == ds) || (dtCurrent!= dr.GetDateTime(dr.GetOrdinal("DTESR")));
					dtCurrent   = dr.GetDateTime(dr.GetOrdinal("DTESR"));
					//
					if (isNewDate)
					{
						ds  = new DatasetStlMsg(dbTransaction ,cs, dtCurrent);   
						lds.Add(ds); 
					}
					// AddRowESR
					ds.AddRowESR(new AggregateEventKey(dr), Convert.ToString(dr["SIDE"]),Convert.ToDecimal(dr["AMOUNT"]),dtSys,appInstance.IdA);
				}
				//
				DatasetStlMsg[] ads = (DatasetStlMsg[]) lds.ToArray(typeof(DatasetStlMsg)); 
				for (int i = 0 ; i< ArrFunc.Count(ads); i++)  
				{
					ads[i].SetIdEsr();
					//
					DataTable dt = ads[i].dtESR.GetChanges(DataRowState.Added);  
					//
					foreach (DataRow row in dt.Rows)
					{
						dr = GetEventEsr(new AggregateEventKey(row));
						while (dr.Read())
							ads[i].AddRowESRDET(Convert.ToInt32(row["IDESR"]),Convert.ToInt32(dr["IDE"]),dtSys,appInstance.IdA);	
					}
					//
					ads[i].Update();  	
					//
					ads[i].SetInstrInfoInNewEsr();
					ads[i].UpdateESR();  	
				}
				//
				dbTransaction.Commit(); 
			}
			catch (OTCmlException ex){isOk= false;  throw ex;}
			catch (Exception ex) {isOk= false; throw new OTCmlException("ESRGenProcessBase.SetESR",ex);}		
			finally
			{
				if (null != dr)
					dr.Close();
				//
				if (false == isOk)
					dbTransaction.Rollback(); 
				
				if (null!= dbTransaction)
					dbTransaction.Dispose(); 
			}
		}
		#endregion
		//
		#region private GetEsr 
		/// <summary>
		/// Retourne les futurs ESR => application des netting sur EVENT (VW_EVENTESR)
		/// </summary>
		private IDataReader GetEsr()
		{
			IDataReader dr = null;
			bool isOk = true;
			//
			try
			{
				QueryParameters queryParameters = GetQueryAggregateEvent();
				string          query           = queryParameters.query; 
				DataParameters  parameters      = queryParameters.parameters; 
				//
				dr = DataHelper.ExecuteReader(cs, CommandType.Text, query,parameters.GetArrayDbParameter());            
				return dr;
			}
			catch (OTCmlException ex) {isOk=false; throw ex;}
			catch (Exception ex)      {isOk=false; throw new OTCmlException("ESRGenProcessBase.GetEsr",ex);}
			finally
			{
				if ((false == isOk) && (null!=dr))
				{
					dr.Close(); 
				}
			}
		}
		#endregion
		//
		#region private GetEventEsr
		/// <summary>
		/// Retourne Les évènement à l'origine d'un ESR (Le detail d'un ESR)
		/// </summary>
		private IDataReader GetEventEsr(AggregateEventKey  pKey)
		{
			IDataReader dr = null;
			bool isOk = true;
			//
			try
			{
				QueryParameters queryParameters = GetQueryAggregateDetEvent(pKey);
				string          query           = queryParameters.query; 
				DataParameters  parameters      = queryParameters.parameters; 
				//
				dr = DataHelper.ExecuteReader(cs, CommandType.Text, query,parameters.GetArrayDbParameter());            
				return dr;
			}
			catch (OTCmlException ex) {isOk=false; throw ex;}
			catch (Exception ex)      {isOk=false; throw new OTCmlException("ESRGenProcessBase.GetEventEsr",ex);}
			finally
			{
				if ((false == isOk) && (null!=dr))
				{
					dr.Close(); 
				}
			}
		}
		#endregion
		//
		#region private GetQueryAggregateDetEvent
		private QueryParameters GetQueryAggregateDetEvent(AggregateEventKey pESR)
		{
			try
			{
				//Parametrs
				DataParameters parametres  = new DataParameters();
				if (pESR.idT>0)  
					parametres.Add(new DataParameter(cs,"IDT",DbType.Int32), pESR.idT); 
				//
				parametres.Add(new DataParameter(cs,"DTSTM",DbType.DateTime), pESR.dtSTM); 
				parametres.Add(new DataParameter(cs,"DTESR",DbType.DateTime), pESR.dtESR); 
				parametres.Add(new DataParameter(cs,"IDA", DbType.Int32), pESR.idA); 
				parametres.Add(new DataParameter(cs,"IDA_STLOFFICE", DbType.Int32), pESR.idAStlOffice); 
				parametres.Add(new DataParameter(cs,"IDA_MSGRECEIVER", DbType.Int32), pESR.idAMsgReceiver); 
				parametres.Add(new DataParameter(cs,"IDA2", DbType.Int32), pESR.idA2); 
				parametres.Add(new DataParameter(cs,"IDA_CSS", DbType.Int32), pESR.idACss); 
				parametres.Add(new DataParameter(cs,"NETMETHOD", DbType.AnsiString,SQLCst.UT_ENUM_MANDATORY_LEN), pESR.netMethod); 
				parametres.Add(new DataParameter(cs,"NETID", DbType.Int32), pESR.netId); 
				parametres.Add(new DataParameter(cs,"NETCOMMONREF", DbType.AnsiString,1024), pESR.netCommonReference); 
				parametres.Add(new DataParameter(cs,"IDC",DbType.AnsiString, SQLCst.UT_CURR_LEN), pESR.idC);
				//
				parametres.Add(new DataParameter(cs,"SESSIONID",DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), appInstance.SessionId); 
				parametres.Add(new DataParameter(cs,"CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),Cst.OTCml_TBL.EVENT.ToString());
				//
				StrBuilder sqlSelect = new StrBuilder(); 
				sqlSelect +=  SQLCst.SELECT  +  "e.IDE";
				// From 
				StrBuilder sqlFrom = new StrBuilder(); 
				sqlFrom  +=  SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
				sqlFrom  +=  SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT + " sr on sr.ID=e.IDE and sr.CLASS=@CLASS and sr.SESSIONID=@SESSIONID" + Cst.CrLf; 
				//
				SQLWhere sqlWhere = new SQLWhere();
				if (parametres.Contains("IDT")) 
					sqlWhere.Append("e.IDT=@IDT");
				sqlWhere.Append("e.DTSTM=@DTSTM");
				sqlWhere.Append("e.DTESR=@DTESR");
				sqlWhere.Append("e.IDA=@IDA");
				sqlWhere.Append("e.IDA_STLOFFICE=@IDA_STLOFFICE");
				sqlWhere.Append("e.IDA_MSGRECEIVER=@IDA_MSGRECEIVER");
				sqlWhere.Append("e.IDA2=@IDA2");
				sqlWhere.Append("e.IDA_CSS=@IDA_CSS");
				sqlWhere.Append("e.NETMETHOD=@NETMETHOD");
				sqlWhere.Append("e.NETID=@NETID");
				sqlWhere.Append("e.NETCOMMONREF=@NETCOMMONREF");
				sqlWhere.Append("e.IDC=@IDC");
				//
				StrBuilder sqlQuery = new StrBuilder(); 
				sqlQuery += sqlSelect.ToString()  + Cst.CrLf + sqlFrom + Cst.CrLf + sqlWhere.ToString()  + Cst.CrLf;  
				//
				QueryParameters queryParameters = new QueryParameters(sqlQuery.ToString(),parametres); 
				return queryParameters;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRGenProcessBase.GetQueryAggregateDetEvent",ex);}
		}
		#endregion public GetQueryAggregateDetEvent
		//
		#region private SetRestrictionEvent
		/// <summary>
		/// Recherche des évènements considérés par le Process (Prise en compte des paramètres du Mqueue)
		/// Alimentation de la Table SESSIONRESTRICT => Sauvegarde SQL des Ide retenus pour les futurs Queries du traitement
		/// </summary>
		private  void SetRestrictionEvent()
		{
			try
			{
				//Add Restriction on EVENT 
				ESRProcessEventRestriction processEventRestriction = new ESRProcessEventRestriction(this); 
				restrictionEvent	    = new RestrictionElement(processEventRestriction); 	
				//
				sqlSessionRestrict.SetRestrictUseSelectUnion(restrictionEvent);  
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRGenProcessBase.SetRestrictionEvent",ex);}
		}
		#endregion
		//
		#region private LockEvent
		/// <summary>
		/// Lock des évènements retenus par le traitement
		/// </summary>
		/// <returns></returns>
		private bool LockEvent()
		{
			try
			{
				bool ret  = true;
				RestrictionItem[] listIdE =  restrictionEvent.GetRestrictItemEnabled();  
				if (ArrFunc.IsFilled(listIdE))
				{
					for (int i = 0 ; i < listIdE.Length ; i++)
					{
						LockObject lckObj = new LockObject(TypeLockEnum.EVENT,listIdE[i].id,listIdE[i].identifier); 
						Lock lck = new Lock(cs,lckObj,appInstance,mQueue.LibProcessType);    
						//
						ret = LockTools.Lock(lck);   									
						if (false == ret)
							break;
					}
				}
				return ret;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRGenProcessBase.LockEvent",ex);}
		}
		#endregion private LockEvent()
	}
	#endregion 
	//
	#region public class ESRStandardGenProcess
	public class ESRStandardGenProcess : ESRGenProcessBase  
	{
		#region Members
		private  ESRStandardGenMQueue  esrStandardGenMQueue;
		#endregion Members
		//		
		#region Accessor
		protected override string dataIdent
		{
			get
			{
				return Cst.OTCml_TBL.TRADE.ToString();
			}
		}

		protected override TypeLockEnum dataTypeLock
		{
			get
			{
				return TypeLockEnum.TRADE; 
			}
		}
		
		#endregion Accessor
		//
		#region Constructor
		public ESRStandardGenProcess(MQueueBase pMQueue,AppInstance pAppInstance):base(pMQueue,pAppInstance) 
		{
			try
			{
				esrStandardGenMQueue = (ESRStandardGenMQueue)pMQueue;
				ProcessStart(); 
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRStandardGenProcess.ctor",ex);}
		}
		#endregion Constructor
		//
		#region protected override ProcessInitialize
		protected override void ProcessInitialize()
		{
			try
			{
				base.ProcessInitialize();
				//
				#region ProcessTuning => Initialisation from Trade
				SQL_Trade sqlTrade = new SQL_Trade(cs,currentId);       
				if (sqlTrade.LoadTable(new string []{"IDT","IDI"}))
					processTuning = new ProcessTuning(cs,sqlTrade.IdI,mQueue.ProcessType,appInstance.AppName,appInstance.HostName); 
				#endregion
				//
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("ProcessTradeBase.ProcessInitialize",ex);}		
		}
		#endregion
		//
		#region protected override ProcessPreExecute
		protected override void ProcessPreExecute()
		{
			try
			{
				#region LockTrade 
				CodeReturn = LockCurrentObjectId(); 
				#endregion LockTrade 
				//
				#region Scan compatibility
				if (LevelStatusTools.IsCodeReturnSuccess(CodeReturn))
					CodeReturn = ScanCompatibility_Trade(currentId); 
				#endregion Scan compatibility
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("ProcessTradeBase.ProcessPreExecute",ex);}		
		}
		#endregion
		//		
		#region protected override GetQueryEvent
		public override QueryParameters GetQueryEvent()
		{
			return GetQueryEvent(false);
		}
		public override  QueryParameters GetQueryEvent(bool pIsForRestrict)
		{
			try
			{
				//DataParameters
				DataParameters parameters = GetMQueueDataParameters ();
				if (esrStandardGenMQueue.idSpecified) 
					parameters.Add(new DataParameter (cs ,"IDT",DbType.Int32),esrStandardGenMQueue.id); 
				parameters.Add(new DataParameter(cs,"NETMETHODSTD",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),NettingMethodEnum.Standard.ToString()); 
				parameters.Add(new DataParameter(cs,"NETMETHODNONE",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),NettingMethodEnum.None.ToString()); 
				//
				//Select
				StrBuilder sqlSelect = new StrBuilder();         
				if (pIsForRestrict)  
					sqlSelect  += SQLCst.SELECT + "e.IDE as ID,e.IDE As IDENTIFIER"; 
				else
					sqlSelect  +=  SQLCst.SELECT + @"e.IDE as IDE,e.IDT as IDT,e.IDT as IDDATA,e.NETID,e.NETMETHOD";
				//
				//From
				sqlSelect  +=  SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
				//
				//Where
				SQLWhere sqlWhere = GetMqueueSqlWhere();
				if (parameters.Contains("IDT"))			
					sqlWhere.Append("e.IDT=@IDT");
				//								
				SQLWhere sqlWhereNetStd = new SQLWhere(sqlWhere.ToString());
				sqlWhereNetStd.Append("e.NETMETHOD=@NETMETHODSTD");
				//
				SQLWhere sqlWhereNetNone = new SQLWhere(sqlWhere.ToString());
				sqlWhereNetNone.Append("e.NETMETHOD=@NETMETHODNONE");
				//
				StrBuilder sqlQuery = new StrBuilder(); 
				sqlQuery += sqlSelect.ToString()  + Cst.CrLf + sqlWhereNetStd.ToString()   + Cst.CrLf;  
				sqlQuery += SQLCst.UNIONALL + Cst.CrLf;   
				sqlQuery += sqlSelect.ToString()  + Cst.CrLf + sqlWhereNetNone.ToString()  + Cst.CrLf;  
				//
				QueryParameters queryParameters = new QueryParameters(sqlQuery.ToString(), parameters); 
				return queryParameters;

			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRStandardGenProcess.GetQueryEvent",ex);}
		}
		#endregion
		//
		#region protected override GetQueryAggregateEvent
		public override QueryParameters GetQueryAggregateEvent()
		{
			try
			{
				//Parametrs
				DataParameters parametres  = new DataParameters();
				parametres.Add(new DataParameter(cs,"NETMETHODSTD",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),NettingMethodEnum.Standard.ToString()); 
				parametres.Add(new DataParameter(cs,"NETMETHODNONE",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),NettingMethodEnum.None.ToString()); 
				parametres.Add(new DataParameter(cs,"SESSIONID",DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), appInstance.SessionId); 
				parametres.Add(new DataParameter(cs,"CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),Cst.OTCml_TBL.EVENT.ToString());
				parametres.Add(new DataParameter(cs,"PAYER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Payer.ToString());
				parametres.Add(new DataParameter(cs,"RECEIVER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Receiver.ToString());
				//From 
				StrBuilder sqlFrom = new StrBuilder(); 
				sqlFrom  +=  SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
				sqlFrom  +=  SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT + " sr on sr.ID=e.IDE and sr.CLASS=@CLASS and sr.SESSIONID=@SESSIONID" + Cst.CrLf; 
				//sqlSelectNetStd
				StrBuilder sqlSelectNetStd = new StrBuilder(SQLCst.SELECT);         
				sqlSelectNetStd  +=  @"e.IDT,e.DTSTM,e.DTESR," + Cst.CrLf;
				sqlSelectNetStd  +=  @"e.IDA,e.IDA_STLOFFICE, e.IDA_MSGRECEIVER,e.IDA2," + Cst.CrLf;
				sqlSelectNetStd  +=  @"e.IDA_CSS,e.NETMETHOD, e.NETID,e.NETCOMMONREF," + Cst.CrLf;
				sqlSelectNetStd  +=  @"e.IDC," + Cst.CrLf;
				sqlSelectNetStd  +=  @"abs(SUM(case when SIDE=@PAYER Then -e.AMOUNT Else e.AMOUNT end)) as AMOUNT," + Cst.CrLf;
				sqlSelectNetStd  +=  @"case when SUM(case when SIDE=@PAYER Then -e.AMOUNT Else e.AMOUNT end)<0" + Cst.CrLf;
				sqlSelectNetStd  +=  @"then @PAYER else @RECEIVER end as SIDE" + Cst.CrLf;
				sqlSelectNetStd  +=  sqlFrom + Cst.CrLf;
				//sqlSelectNetNone
				StrBuilder sqlSelectNetNone = new StrBuilder(SQLCst.SELECT);         
				sqlSelectNetNone  += @"e.IDT,e.DTSTM,e.DTESR," + Cst.CrLf;
				sqlSelectNetNone  += @"e.IDA,e.IDA_STLOFFICE, e.IDA_MSGRECEIVER,e.IDA2," + Cst.CrLf;
				sqlSelectNetNone  += @"e.IDA_CSS,e.NETMETHOD, e.NETID,e.NETCOMMONREF," + Cst.CrLf;
				sqlSelectNetNone  += @"e.IDC," + Cst.CrLf;
				sqlSelectNetNone  += @"e.AMOUNT," + Cst.CrLf;
				sqlSelectNetNone  += @"e.SIDE as SIDE" + Cst.CrLf;
				sqlSelectNetNone  += sqlFrom + Cst.CrLf;
				//
				SQLWhere sqlWhereNetStd = new SQLWhere();
				sqlWhereNetStd.Append("e.NETMETHOD=@NETMETHODSTD");
				//
				SQLWhere sqlWhereNetNone = new SQLWhere();
				sqlWhereNetNone.Append("e.NETMETHOD=@NETMETHODNONE");
				//
				StrBuilder sqlGoupByNetStd = new StrBuilder(SQLCst.GROUPBY);
				sqlGoupByNetStd += "e.IDT,e.DTSTM,e.DTESR,e.IDA,e.IDA_STLOFFICE,e.IDA_MSGRECEIVER,e.IDA2," + Cst.CrLf; 
				sqlGoupByNetStd += "e.IDA_CSS,e.NETMETHOD,e.NETID,e.NETCOMMONREF,e.IDC";
				//
				StrBuilder sqlQuery = new StrBuilder(); 
				sqlQuery += sqlSelectNetStd.ToString()  + Cst.CrLf + sqlWhereNetStd.ToString()   + Cst.CrLf;  
				sqlQuery += sqlGoupByNetStd + Cst.CrLf;
				sqlQuery += SQLCst.UNIONALL + Cst.CrLf;   
				sqlQuery += sqlSelectNetNone.ToString()  + Cst.CrLf + sqlWhereNetNone.ToString()  + Cst.CrLf;  
				//Order
				sqlQuery += SQLCst.ORDERBY + "DTESR,IDT";  
				//
				QueryParameters queryParameters = new QueryParameters(sqlQuery.ToString(),parametres); 
				return queryParameters;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRStandardGenProcess.GetQueryAggregateEvent",ex);}
		}
		#endregion
		
	}
	#endregion 
	//
	#region public class ESRNetGenProcess
	public class ESRNetGenProcess : ESRGenProcessBase  
	{
		#region Members
		private  ESRNetGenMQueue  esrNetGenMQueue;
		#endregion Members
		
		#region Accessor
		protected override string dataIdent
		{
			get
			{
				string ret = string.Empty; 
				//
				if (NettingMethodEnum.Convention  == esrNetGenMQueue.nettingMethod)  
					ret =  Cst.OTCml_TBL.NETCONVENTION.ToString();
				else if (NettingMethodEnum.Designation== esrNetGenMQueue.nettingMethod)  
					ret =  Cst.OTCml_TBL.NETDESIGNATION.ToString();
				//
				return ret;
			}
		}

		protected override TypeLockEnum dataTypeLock
		{
			get
			{
				TypeLockEnum ret  = base.dataTypeLock ;
				//
				if (NettingMethodEnum.Convention  == esrNetGenMQueue.nettingMethod)  
					ret =  TypeLockEnum.NETCONVENTION;
				else if (NettingMethodEnum.Designation== esrNetGenMQueue.nettingMethod)  
					ret =  TypeLockEnum.NETDESIGNATION;
				//
				return ret;
			}
		}
		#endregion Accessor
		
		#region Constructor
		public ESRNetGenProcess(MQueueBase pMQueue,AppInstance pAppInstance):base(pMQueue,pAppInstance) 
		{
			try
			{
				esrNetGenMQueue = (ESRNetGenMQueue)pMQueue;
				ProcessStart(); 
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("ESRNetGenProcess..ctor",ex);}
		}
		#endregion Constructor

		#region protected override ProcessInitialize
		protected override void ProcessInitialize()
		{
			base.ProcessInitialize ();
			//
			processTuning = new ProcessTuning(cs,0,mQueue.ProcessType,appInstance.AppName,appInstance.HostName); 
		}
		#endregion protecetd ProcessInitialize
		
		#region protected override ProcessPreExecute
		protected override void ProcessPreExecute()
		{
			try
			{
				#region Lock NETCONVENTION or NETCONSIGNATION
				//Pour eviter qu'un autre process Traite ce netting 
				//(même si le perimètre des différent ex Date différente)
				CodeReturn = LockCurrentObjectId(); 
				#endregion NETCONVENTION or NETCONSIGNATION 

				if (LevelStatusTools.IsCodeReturnSuccess(CodeReturn))
				{
					Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCES; 
					//
					QueryParameters  queryParamers = GetQueryEvent();   
					StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT_DISTINCT + @"e.IDT As IDT" + Cst.CrLf);
					sqlSelect           += SQLCst.FROM + Cst.CrLf;
					sqlSelect           += "(" + queryParamers.query + ") e";
					//				
					IDataReader  dr = DataHelper.ExecuteReader(cs,CommandType.Text,sqlSelect.ToString(),queryParamers.parameters.GetArrayDbParameter());
					while (dr.Read())
					{
						//
						int idT = Convert.ToInt32(dr["IDT"]);
					
						#region Lock Trade
						LockObject lockObject = new LockObject(TypeLockEnum.TRADE ,idT, "Trade " + idT.ToString() );
						//
						if (null != lockObject)
						{
							Lock lckExisting = null;
							Lock lck     = new Lock (cs, lockObject,appInstance,mQueue.ProcessType.ToString());
							if (false == LockTools.Lock2(lck, ref lckExisting))
							{
								lockObject = null; // Use For Not Unlock
								errLevel        = Cst.ErrLevel.LOCKUNSUCCESSFUL;
							}
						}
						#endregion LockTrade
						//
						#region Verify Trade Compatibility 
						if (LevelStatusTools.IsCodeReturnSuccess(errLevel))
							errLevel = ScanCompatibility_Trade(idT);
						#endregion Verify
						//
						if (Cst.ErrLevel.SUCCES  != errLevel)
							break;
					}
					//
					CodeReturn = errLevel;
				}
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRNetGenProcess.ProcessPreExecute",ex);}
		}
		#endregion
		
		#region protected override GetQueryEvent
		public override QueryParameters GetQueryEvent()
		{
			return GetQueryEvent(false);
		}
		public override  QueryParameters GetQueryEvent(bool pIsForRestrict)
		{
			try
			{
				//DataParameters
				DataParameters parameters  = GetMQueueDataParameters() ;
				parameters.Add(new DataParameter (cs ,"NETMETHOD",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),esrNetGenMQueue.nettingMethod.ToString()); 
				if (esrNetGenMQueue.idSpecified) 
					parameters.Add(new DataParameter (cs ,"NETID",DbType.Int32),esrNetGenMQueue.id); 
				parameters.Add(new DataParameter(cs,"NETMETHODSTD",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),NettingMethodEnum.Standard.ToString()); 
				parameters.Add(new DataParameter(cs,"NETMETHODNONE",DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN),NettingMethodEnum.None.ToString()); 
				//
				//Select
				StrBuilder sqlSelect = new StrBuilder();         
				if (pIsForRestrict)  
					sqlSelect  += SQLCst.SELECT + "e.IDE As ID,e.IDE As IDENTIFIER"; 
				else
					sqlSelect  +=  SQLCst.SELECT + @"e.IDE,e.IDT,e.NETID as IDDATA,e.NETMETHOD as NETMETHOD";
				//From
				sqlSelect  +=  SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
				//				
				//Where
				SQLWhere sqlWhere = GetMqueueSqlWhere();
				sqlWhere.Append("e.NETMETHOD!=@NETMETHODSTD And e.NETMETHOD!=@NETMETHODNONE");
				
				if (parameters.Contains("NETID"))			
					sqlWhere.Append("e.NETID=@NETID");
				if (parameters.Contains("NETMETHOD"))			
					sqlWhere.Append("e.NETMETHOD=@NETMETHOD");
				
				
				//								
				StrBuilder sqlQuery = new StrBuilder(); 
				sqlQuery += sqlSelect.ToString()  + Cst.CrLf + sqlWhere.ToString() + Cst.CrLf;  
				//
				QueryParameters queryParameters = new QueryParameters(sqlQuery.ToString(), parameters); 
				return queryParameters;

			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRNetGenProcess.GetQueryEvent",ex);}
		}
		#endregion
	
		#region protected override GetQueryAggregateEvent
		public override QueryParameters GetQueryAggregateEvent()
		{
			try
			{
				//Parametrs
				DataParameters parametres  = new DataParameters();
				parametres.Add(new DataParameter(cs,"SESSIONID",DbType.AnsiString, SQLCst.UT_SESSIONID_LEN), appInstance.SessionId); 
				parametres.Add(new DataParameter(cs,"CLASS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN),Cst.OTCml_TBL.EVENT.ToString());
				parametres.Add(new DataParameter(cs,"PAYER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Payer.ToString());
				parametres.Add(new DataParameter(cs,"RECEIVER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Receiver.ToString());
				//From 
				StrBuilder sqlFrom = new StrBuilder(); 
				sqlFrom  +=  SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_EVENTESR + " e " + Cst.CrLf;
				sqlFrom  +=  SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.SESSIONRESTRICT + " sr on sr.ID=e.IDE and sr.CLASS=@CLASS and sr.SESSIONID=@SESSIONID" + Cst.CrLf; 
				//sqlSelectNetStd
				StrBuilder sqlSelectNet = new StrBuilder(SQLCst.SELECT);         
				sqlSelectNet  +=  @"e.DTSTM,e.DTESR," + Cst.CrLf;
				sqlSelectNet  +=  @"e.IDA,e.IDA_STLOFFICE, e.IDA_MSGRECEIVER,e.IDA2," + Cst.CrLf;
				sqlSelectNet  +=  @"e.IDA_CSS,e.NETMETHOD, e.NETID,e.NETCOMMONREF," + Cst.CrLf;
				sqlSelectNet  +=  @"e.IDC," + Cst.CrLf;
				sqlSelectNet  +=  @"abs(SUM(case when SIDE=@PAYER Then -e.AMOUNT Else e.AMOUNT end)) as AMOUNT," + Cst.CrLf;
				sqlSelectNet  +=  @"case when SUM(case when SIDE=@PAYER Then -e.AMOUNT Else e.AMOUNT end)<0" + Cst.CrLf;
				sqlSelectNet  +=  @"then @PAYER else @RECEIVER end as SIDE" + Cst.CrLf;
				sqlSelectNet  +=  sqlFrom + Cst.CrLf;
				//
				StrBuilder sqlGoupByNetStd = new StrBuilder(SQLCst.GROUPBY);
				sqlGoupByNetStd += "e.DTSTM,e.DTESR,e.IDA,e.IDA_STLOFFICE,e.IDA_MSGRECEIVER,e.IDA2," + Cst.CrLf; 
				sqlGoupByNetStd += "e.IDA_CSS,e.NETMETHOD,e.NETID,e.NETCOMMONREF,e.IDC";
				//
				StrBuilder sqlQuery = new StrBuilder(); 
				sqlQuery += sqlSelectNet.ToString()  + Cst.CrLf ;  
				sqlQuery += sqlGoupByNetStd + Cst.CrLf;
				//Order
				sqlQuery += SQLCst.ORDERBY + "DTESR,NETID,NETMETHOD";  
				//
				QueryParameters queryParameters = new QueryParameters(sqlQuery.ToString(),parametres); 
				return queryParameters;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("ESRNetGenProcess.GetQueryAggregateEvent",ex);}
		}
		#endregion
	}
	#endregion 
	//	
	#region public struct AggregateEventKey
	public struct AggregateEventKey
	{
		#region Members
		public int      idT;
		public DateTime dtSTM;
		public DateTime dtESR;
		public int idA;
		public int idAStlOffice;
		public int idAMsgReceiver;
		public int idA2;
		public int idACss;
		public string netMethod;
		public int netId;
		public string netCommonReference;
		public string idC;
		#endregion Members
		
		#region Constructor
		public AggregateEventKey(int pidt, DateTime  pdtSTM, DateTime pdtESR,int pidA ,int pidAStlOffice, int pidAMsgReceiver,
			int pidA2, int pidACss, string pnetMethod, int pnetId, string pnetCommonReference, string pidC)
		{
			idT          = pidt;
			dtSTM        = pdtSTM ;
			dtESR        = pdtESR ;
			idA          = pidA ;
			idAStlOffice = pidAStlOffice;
			idAMsgReceiver =  pidAMsgReceiver;
			idA2         = pidA2;
			idACss       = pidACss;
			netMethod    = pnetMethod;
			netId        = pnetId;
			netCommonReference = pnetCommonReference;
			idC          = pidC;
		}
		public AggregateEventKey(DataRow pRow) : this 
			(
			0,
			Convert.ToDateTime(pRow["DTSTM"]), 	
			Convert.ToDateTime(pRow["DTESR"]),
			Convert.ToInt32(pRow["IDA"]),
			Convert.ToInt32(pRow["IDA_STLOFFICE"]),
			Convert.ToInt32(pRow["IDA_MSGRECEIVER"]),
			Convert.ToInt32(pRow["IDA2"]),
			Convert.ToInt32(pRow["IDA_CSS"]),
			Convert.ToString(pRow["NETMETHOD"]),
			Convert.ToInt32(pRow["NETID"]),
			Convert.ToString(pRow["NETCOMMONREF"]),
			Convert.ToString(pRow["IDC"])
			)
		{
			try
			{	
				idT = Convert.ToInt32(pRow["IDT"]);
			}
			catch{ idT=0;}
		}
		public AggregateEventKey(IDataReader  pRow) :this
			(
			0,
			Convert.ToDateTime(pRow["DTSTM"]), 	
			Convert.ToDateTime(pRow["DTESR"]),
			Convert.ToInt32(pRow["IDA"]),
			Convert.ToInt32(pRow["IDA_STLOFFICE"]),
			Convert.ToInt32(pRow["IDA_MSGRECEIVER"]),
			Convert.ToInt32(pRow["IDA2"]),
			Convert.ToInt32(pRow["IDA_CSS"]),
			Convert.ToString(pRow["NETMETHOD"]),
			Convert.ToInt32(pRow["NETID"]),
			Convert.ToString(pRow["NETCOMMONREF"]),
			Convert.ToString(pRow["IDC"])
			)
		{
			try
			{
				idT = Convert.ToInt32(pRow.GetOrdinal("IDT"));
			}
			catch{ idT = 0;}
		}
		#endregion
	}
	#endregion public struct AggregateEventKey
	//
	#region public class ESRProcessEventRestriction
	public class ESRProcessEventRestriction :  IRestrictionElement 
	{
		#region Membres
		ESRGenProcessBase _process;
		#endregion Membres
		
		#region Constructor
		public ESRProcessEventRestriction(ESRGenProcessBase pProcess)
		{
			_process = pProcess;
		}
		#endregion Constructor
		
		#region Membres de IRestrictionElement
		#region public Class
		public string Class
		{
			get{return  Cst.OTCml_TBL.EVENT.ToString();}
		}
		#endregion
		#region public cs
		public string cs
		{
			get{return  _process.cs;}
		}
		#endregion
		#region public GetQueryElement
		public QueryParameters GetQueryElement()
		{
			return _process.GetQueryEvent(true);   
		}
		#endregion
		#region public bool IsElementEnabled
		public  bool IsItemEnabled(int pIde)
		{
			bool ret  = (Cst.ErrLevel.SUCCES == _process.ScanCompatibility_Event(pIde));
			if (ret)				
				ret = (false == StlMsgTools.IsEventUseByMSO(cs,pIde));
			return ret;
		}
		#endregion public bool IsEventEnabled
		#endregion
	}
	#endregion ESRStandardGenProcessEventRestriction
	//
	#region public class StlMsgTools
	public class StlMsgTools
	{
		#region Constructor
		public StlMsgTools(){}
		#endregion constructor
		
		#region public IsEventEnabled
		public static bool IsEventEnabled(int pIde, ProcessBase pProcess)	
		{	
			try
			{
				bool ret = true;
				//
				if (Cst.ErrLevel.SUCCES != pProcess.ScanCompatibility_Event(pIde))	
					ret = false;
				//
				if (true == ret )
					ret = (false == IsEventUseByMSO(pProcess.cs,pIde));  
				//
				return ret;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("StlMsgTools.IsEventEnabled",ex);}
		}
		#endregion

		#region public IsEventUseByMSO
		/// <summary>
		///  IDE fait-il parti d'un message envoyé?  
		/// </summary>
		/// <param name="pSource"></param>
		/// <param name="pIde"></param>
		/// <returns></returns>
		public static bool IsEventUseByMSO(string pSource, int pIde)
		{
			try
			{
				bool ret = false;
				DataParameter param = DataParameter.GetParameter(pSource,DataParameter.ParameterEnum.IDE);
				param.Value  = pIde;
				//
				StrBuilder sqlQuery = new StrBuilder();
				sqlQuery += SQLCst.SELECT_DISTINCT  + "1 As EXISTMSO" +  Cst.CrLf;
				sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MSO + " mso" + Cst.CrLf;
				sqlQuery += SQLCst.INNERJOIN_DBO  + Cst.OTCml_TBL.MSODET + " msodet on msodet.IDMSO = mso.IDMSO" + Cst.CrLf;
				sqlQuery += OTCmlHelper.GetSQLJoin(pSource,Cst.OTCml_TBL.ESR ,true,"msodet.IDESR", "esr");    
				sqlQuery += SQLCst.INNERJOIN_DBO  + Cst.OTCml_TBL.ESRDET + " esrdet on esrdet.IDESR = esr.IDESR" + Cst.CrLf;
				sqlQuery += SQLCst.WHERE + "esrdet.IDE=@IDE";
				//
				object obj = DataHelper.ExecuteScalar(pSource , CommandType.Text, sqlQuery.ToString(), param.DbDataParameter);
				ret   = (null != obj);
				return ret;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex){throw new OTCmlException("StlMsgTools.IsEventUseByMSO",ex);}
		}
		#endregion
	}
	#endregion
	//
	#region public class DatasetStlMsg
	public class DatasetStlMsg
	{
		#region Members
		private   string           _cs;
		private   IDbTransaction   _dbTransaction; 
		private   DateTime         _date;
		private   DataSet          _ds;
		#endregion Members
		//
		#region accessor
		public DataTable dtESR
		{
			get{return _ds.Tables["ESR"];}
		}
		public DataTable dtESRDet
		{
			get{return _ds.Tables["ESRDET"];}
		}
		#endregion accessor
		//
		#region constructor
		public DatasetStlMsg(IDbTransaction  pdbTransaction, string pCs, DateTime pDate)
		{
			_date          = pDate; 
			_dbTransaction = pdbTransaction;
			_cs            = pCs;
			//
			LoadDs();
			InitializeDs();
		}
		#endregion
		//		
		#region public AddRowESR
		public void AddRowESR(AggregateEventKey pKey, string pSide, Decimal pAmount, DateTime pDtSys, int pIdAIns)
		{
			try
			{
				DataRow   row = dtESR.NewRow();  
				//
				row["DTESR"]		  = pKey.dtESR;  
				row["DTSTM"]          = pKey.dtSTM; 
				row["IDA"]            = pKey.idA; 
				row["IDA_STLOFFICE"]  = pKey.idAStlOffice; 
				row["IDA_MSGRECEIVER"]= pKey.idAMsgReceiver;  
				row["IDA2"]           = pKey.idA2;  
				row["IDA_CSS"]        = pKey.idACss; 
				row["NETMETHOD"]      = pKey.netMethod; 
				row["NETID"]          = pKey.netId;
				row["NETCOMMONREF"]   = pKey.netCommonReference;
				row["IDC"]            = pKey.idC ;
				row["SIDE"]           = pSide;
				row["AMOUNT"]         = pAmount;
				//
				row["IDT"]            = (pKey.idT>0)? pKey.idT:Convert.DBNull;   
				row["GPRODUCT"]       = Convert.DBNull;     
				row["IDI"]            = Convert.DBNull;     
				row["IDGINSTR"]       = Convert.DBNull;     
				//
				row["DTINS"]          = pDtSys;  
				row["IDAINS"]         = pIdAIns; 
				row["DTUPD"]          = Convert.DBNull;
				row["IDAUPD"]         = Convert.DBNull;
				//
				dtESR.Rows.Add(row);
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.AddRowESR",ex);}
		}
		#endregion public AddRowESRDET
		#region public AddRowESRDET
		public void AddRowESRDET(int pIdEsr, int pIdE, DateTime pDtSys, int pIdAIns)
		{
			try
			{
				DataRow   row = dtESRDet.NewRow();  
				//
				row["IDESR"]		  = pIdEsr; 
				row["IDE"]            = pIdE;
				row["DTINS"]          = pDtSys;  
				row["IDAINS"]         = pIdAIns; 
				//
				dtESRDet.Rows.Add(row);
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.AddRowESRDET",ex);}
		}
		#endregion 
		//
		#region public SetIdEsr
		public void SetIdEsr()
		{
			try
			{
				DataRow[]  dr  = dtESR.Select("IDESR is null"); 
				//
				if (null != dr)
				{
					int newIdESR; 
					//
					if (null!=_dbTransaction) 
						SQLUP.GetId(out newIdESR,_dbTransaction,SQLUP.IdGetId.ESR,SQLUP.PosRetGetId.First, ArrFunc.Count(dr));
					else
						SQLUP.GetId(out newIdESR,_cs,SQLUP.IdGetId.ESR,SQLUP.PosRetGetId.First, ArrFunc.Count(dr));
					//
					for (int i=0 ; i< dr.Length ; i++)
					{
						dr[i]["IDESR"] = newIdESR;
						dr[i]["EXTLLINK"] = "NEW";
						newIdESR ++;
					}
				}
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.SetIdOnNewEsr",ex);}
		}
		#endregion 
		#region public SetInstrInfoInNewEsr
		public void SetInstrInfoInNewEsr()
		{
			try
			{
				if (null != dtESR)
				{
					DataRow[]  dr = dtESR.Select("EXTLLINK = 'NEW'");
					//
					if (ArrFunc.IsFilled(dr))
					{
						StrBuilder sqlSelect = new StrBuilder(); 
						sqlSelect += SQLCst.SELECT        + "i.IDI,i.IDP,i.GPRODUCT" + Cst.CrLf;  
						sqlSelect += SQLCst.FROM_DBO      + Cst.OTCml_TBL.ESR.ToString()  + " esr "+  Cst.CrLf;  
						sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ESRDET.ToString() + " esrdet on esrdet.IDESR = esr.IDESR" + Cst.CrLf;  
						sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT.ToString() +  " ev on ev.IDE=esrdet.IDE"+   Cst.CrLf;  
						sqlSelect += SQLCst.LEFTJOIN_DBO  + Cst.OTCml_TBL.TRADEINSTRUMENT.ToString()  + " ti on ti.IDT=ev.IDT And ti.INSTRUMENTNO=ev.INSTRUMENTNO" + Cst.CrLf;  
						sqlSelect += SQLCst.LEFTJOIN_DBO  + Cst.OTCml_TBL.VW_INSTR_PRODUCT.ToString() + " i on i.IDI=ti.IDI" + Cst.CrLf;  
						sqlSelect += SQLCst.WHERE + "esr.IDESR = @IDESR"; 
						//
						DataParameter paramIdEsr = null;
						if (null!=_dbTransaction)
							paramIdEsr = new DataParameter(_dbTransaction ,"IDESR",DbType.Int32);
						else
							paramIdEsr =  new DataParameter(_cs ,"IDESR",DbType.Int32);
						//
						for (int i = 0 ; i < dr.Length ;  i++)
						{
							int idI        = 0;
							int idP        = 0;  
							string idGproduct = string.Empty;  
							string idGInstr   = string.Empty;  
							//
							paramIdEsr.Value  = Convert.ToInt32(dr[i]["IDESR"]);	
							DataSet dsResult = null;
							if (null != _dbTransaction)
								dsResult = DataHelper.ExecuteDataset(_dbTransaction, CommandType.Text, sqlSelect.ToString(), paramIdEsr.DbDataParameter);
							else
								dsResult = DataHelper.ExecuteDataset(_cs, CommandType.Text, sqlSelect.ToString(), paramIdEsr.DbDataParameter);
							//	
							if ( (dsResult.Tables[0].Rows.Count>0) && (false == dsResult.Tables[0].Rows[0].IsNull("IDI"))) 
							{
								idI        = Convert.ToInt32(dsResult.Tables[0].Rows[0]["IDI"]);  
								idP        = Convert.ToInt32(dsResult.Tables[0].Rows[0]["IDP"]);  
								idGproduct = Convert.ToString(dsResult.Tables[0].Rows[0]["GPRODUCT"]);  
								//
								string[] gInstr = InstrTools.GetListGrpInstr(_cs,idI);    							
								if (ArrFunc.IsFilled (gInstr) &&  (gInstr.Length ==1))
									idGInstr = Convert.ToString(gInstr[0]);
							}
							dr[i]["EXTLLINK"]  = Convert.DBNull; 
							dr[i]["IDI"]       = DataHelper.GetDBData(idI);  
							dr[i]["IDP"]       = DataHelper.GetDBData(idP);
							dr[i]["GPRODUCT"]  = DataHelper.GetDBData(idGproduct);
							dr[i]["IDGINSTR"]  = DataHelper.GetDBData(idGInstr);
						}
					}
				}
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.SetInstrInfoInNewEsr",ex);}		
		}
		#endregion public SetInstrEsr
		#region public DelESR
//		public void DelESR (int[] pIdE)
//		{
//			try
//			{
//				if (ArrFunc.IsFilled(dtESR.Rows))
//				{
//					for (int i=dtESR.Rows.Count-1 ; i> -1; i--)
//					{
//						bool del = false;
//						DataRow[]  drEsrDet = dtESR.Rows[i].GetChildRows("ESR_ESRDET");   
//						for (int j=0 ; j< drEsrDet.Length ; j++)
//						{
//							for (int k=0 ; j< pIdE.Length ; k++)
//							{
//								if (pIdE[k] == Convert.ToInt32(drEsrDet[j]["IDE"]))
//								{
//									del = true;
//									break;
//								}
//							}
//							if (del)
//							{
//								break;	
//							}
//						}
//						if (del)
//						{
//							DataRow[]  dr2 = dtESR.Rows[i].GetChildRows("ESR_ESRDET");   
//							foreach (DataRow row in  dr2)
//								dtESRDet.Rows.Remove (row);   
//
//							dtESR.Rows.RemoveAt(i);   
//
//						}
//					}
//				}
//			}
//			catch (OTCmlException ex){throw ex;}
//			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.DelESR",ex);}		
//		}
		#endregion
		//
		#region public UpdateESR
		public void UpdateESR()
		{
			try
			{
				string sqlSelect = GetSelectEsrColumn();
				//
				if (null != _dbTransaction) 
					DataHelper.ExecuteDataAdapter(_dbTransaction,sqlSelect,dtESR);
				else
					DataHelper.ExecuteDataAdapter(_cs,sqlSelect,dtESR);
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.UpdateESR",ex);}
		}
		#endregion UpdateESR
		#region public UpdateESRDET
		public void UpdateESRDET()
		{
			try
			{
				string sqlSelect = GetSelectEsrDetColumn();
				//
				if (null != _dbTransaction) 
					DataHelper.ExecuteDataAdapter(_dbTransaction,sqlSelect,dtESRDet);
				else
					DataHelper.ExecuteDataAdapter(_cs ,sqlSelect,dtESRDet);
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.UpdateESRDet",ex);}
		}
		#endregion UpdateESR
		#region public Update
		public void Update()
		{
			try
			{
				UpdateESR   ();
				UpdateESRDET();
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.Update",ex);}
		}
		#endregion Update
		//
		#region private InitializeDs
		private void InitializeDs()
		{
			DataTable dt         = null;  
			//
			dt             = _ds.Tables[0];
			dt.TableName   = "ESR";
			//dt.PrimaryKey  = new DataColumn[1]{dt.Columns["IDESR"]};
			//
			dt             = _ds.Tables[1];
			dt.TableName   = "ESRDET";
			//dt.PrimaryKey  = new DataColumn[2]{dt.Columns["IDESR"],dt.Columns["IDE"]};
			//
			dt             = _ds.Tables[2];
			dt.TableName   = "MSO";
			//dt.PrimaryKey  = new DataColumn[1]{dt.Columns["IDMSO"]};
			//
			dt             = _ds.Tables[3];
			dt.TableName   = "MSODET";
			//dt.PrimaryKey  = new DataColumn[2]{dt.Columns["IDMSO"],dt.Columns["IDESR"]};
			//
			DataRelation   relEsrEsrDet = new DataRelation ("ESR_ESRDET",dtESR.Columns["IDESR"],dtESRDet.Columns["IDESR"],false);
			_ds.Relations.Add( relEsrEsrDet); 
			
		}
		
		#endregion private InitializeDs
		#region private LoadDs
		private void LoadDs()
		{
			try
			{
				DataParameter paramDate = null;
				//
				if (null != _dbTransaction)
					paramDate      = new DataParameter(_dbTransaction, "DT", DbType.DateTime);
				else
					paramDate      = new DataParameter(_cs, "DT", DbType.DateTime);
				paramDate.Value              = _date;
				//
				StrBuilder sqlSelectEsr      = new StrBuilder(GetSelectEsrColumn());   
				sqlSelectEsr    += SQLCst.WHERE + @"(esr.DTESR = @DT)";
				//
				StrBuilder sqlSelectEsrDet   = new StrBuilder(GetSelectEsrDetColumn()) ;   
				sqlSelectEsrDet += SQLCst.INNERJOIN_DBO + "ESR esr on esr.IDESR=esrd.IDESR";
				sqlSelectEsrDet += SQLCst.WHERE + @"(esr.DTESR = @DT)";
				//
				StrBuilder sqlSelectMso      = new StrBuilder(GetSelectMsoColumn());   
				sqlSelectMso    += SQLCst.WHERE + @"(mso.DTMSO = @DT)";
				//
				StrBuilder sqlSelectMsoDet   = new StrBuilder(GetSelectMsoDetColumn()) ;   
				sqlSelectMsoDet += SQLCst.INNERJOIN_DBO + "MSO mso on mso.IDMSO=msod.IDMSO";
				sqlSelectMsoDet += SQLCst.WHERE + @"(mso.DTMSO = @DT)";
				//
				string sqlSelect = sqlSelectEsr.ToString()  + SQLCst.SEPARATOR_MULTISELECT + sqlSelectEsrDet.ToString();
				sqlSelect += sqlSelectMso.ToString()  + SQLCst.SEPARATOR_MULTISELECT + sqlSelectMsoDet.ToString();
				//
				if (null != _dbTransaction)
					_ds = DataHelper.ExecuteDataset(_dbTransaction, CommandType.Text, sqlSelect, paramDate.DbDataParameter);
				else
					_ds = DataHelper.ExecuteDataset(_cs, CommandType.Text, sqlSelect, paramDate.DbDataParameter);
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.LoadDs",ex);}		
		}
		#endregion LoadDs
		//
		#region private GetSelectEsrColumn
		private string GetSelectEsrColumn()
		{
			try
			{
				StrBuilder sqlSelect = new StrBuilder();
				sqlSelect += SQLCst.SELECT + @"esr.IDESR,esr.DTSTM,esr.DTESR,"+ Cst.CrLf;
				sqlSelect += "esr.IDA,esr.IDA_STLOFFICE,esr.IDA_MSGRECEIVER,esr.IDA2,esr.IDA_CSS,esr.NETMETHOD,esr.NETID,esr.NETCOMMONREF," + Cst.CrLf;
				sqlSelect += "esr.IDC,esr.AMOUNT,esr.SIDE," + Cst.CrLf;
				sqlSelect += "esr.IDT,esr.GPRODUCT,esr.IDP,esr.IDI,esr.IDGINSTR," + Cst.CrLf;
				sqlSelect += "esr.DTUPD,esr.IDAUPD,esr.DTINS,esr.IDAINS,"+ Cst.CrLf;
				sqlSelect += "esr.EXTLLINK,esr.ROWATTRIBUT" + Cst.CrLf;
				//
				sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ESR + " esr " + Cst.CrLf;
				//				
				return sqlSelect.ToString();
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.GetSelectEsrColumn",ex);}		
		}
		#endregion GetSelectEsrColumn
		#region private GetSelectEsrDetColumn
		private string GetSelectEsrDetColumn()
		{
			try
			{
				StrBuilder sqlSelect = new StrBuilder();
				sqlSelect += SQLCst.SELECT + @"esrd.IDESR,esrd.IDE,"+ Cst.CrLf;
				sqlSelect += "esrd.DTUPD,esrd.IDAUPD,esrd.DTINS,esrd.IDAINS,"+ Cst.CrLf;
				sqlSelect += "esrd.EXTLLINK,esrd.ROWATTRIBUT" + Cst.CrLf;
				//
				sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ESRDET + " esrd " + Cst.CrLf;
				//				
				return sqlSelect.ToString();
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.GetSelectEsrDetColumn",ex);}		
		}
		#endregion GetSelectEsrDetColumn
		//
		#region private GetSelectMsoColumn
		private string GetSelectMsoColumn()
		{
			try
			{
				StrBuilder sqlSelect = new StrBuilder();
				sqlSelect += SQLCst.SELECT + @"mso.IDMSO,mso.DTMSO,"+ Cst.CrLf;
				sqlSelect += "mso.STLMSGXML,mso.STLMSGXSL,mso.STLMSO," + Cst.CrLf;
				sqlSelect += "mso.DTUPD,mso.IDAUPD,mso.DTINS,mso.IDAINS,"+ Cst.CrLf;
				sqlSelect += "mso.EXTLLINK,mso.ROWATTRIBUT" + Cst.CrLf;
				//
				sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MSO + " mso " + Cst.CrLf;
				//				
				return sqlSelect.ToString();
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.GetSelectMsoColumn",ex);}		
		}
		#endregion GetSelectEsrColumn
		#region private GetSelectMsoDetColumn
		private string GetSelectMsoDetColumn()
		{
			try
			{
				StrBuilder sqlSelect = new StrBuilder();
				sqlSelect += SQLCst.SELECT + @"msod.IDMSO,msod.IDESR,"+ Cst.CrLf;
				sqlSelect += "msod.DTUPD,msod.IDAUPD,msod.DTINS,msod.IDAINS,"+ Cst.CrLf;
				sqlSelect += "msod.EXTLLINK,msod.ROWATTRIBUT" + Cst.CrLf;
				//
				sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MSODET + " msod" + Cst.CrLf;
				//				
				return sqlSelect.ToString();
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex) {throw new OTCmlException("DatasetStlMsg.GetSelectEsrDetColumn",ex);}		
		}
		#endregion GetSelectEsrDetColumn
	}
	#endregion public DatasetStlMsg
	//
}






	
	
	
	
	
	
	
	
	
	
	
	
	
	



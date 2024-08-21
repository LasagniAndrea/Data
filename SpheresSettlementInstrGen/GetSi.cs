#region Using Directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Process;  
using EFS.Common;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.GUI.Interface;
using EfsML;

using FpML.Cd;
using FpML.Fx;
using FpML.Doc;
using FpML.Eqd;
using FpML.Eqs;
using FpML.Ird;
using FpML.Shared;
using FpML.Enum;
#endregion Using Directives

namespace EFS.Settlement
{
	#region enum SiModeEnum
	public enum SiModeEnum
	{
	Osi,				// OTCml Si (settlement instruction specified)
	Oissi,				// OTCml Internal Standing Si 
	Oessi,				// OTCml External Standing Si 
	//
	Fsi,				// Fpml  Si (settlement instruction specified)
	Fissi,				// Fpml  Internal Standing Si 
	Fessi,				// Fpml  External Standing Si 
	//
	Dbsi,				// Default Book Entry SI
	Dessi,				// Default External SSI
	Dissi,				// Default Internal SSI
	//
	Undefined			//
	}
	#endregion enum
	
	#region sealed class	SettlementTools 
	public sealed class	SettlementTools 
	{
		public static bool IsStanding(SiModeEnum pSiMode)
		{
		return ( (SiModeEnum.Oissi== pSiMode) || (SiModeEnum.Oessi== pSiMode) || 
				 (SiModeEnum.Fissi== pSiMode) || (SiModeEnum.Fessi== pSiMode) || 
				 (SiModeEnum.Dissi== pSiMode) || (SiModeEnum.Dessi== pSiMode));
		}
	}
	#endregion sealed class	SettlementTools 

	#region class SettlementChain
	/// <summary>
	/// Description résumée de SettlementChain.
	/// </summary>
	public class SettlementChain
	{
		#region Members
		private  SettlementChainItem[]  settlementChainItems;              
		#endregion Members
		//
		#region indexor
		public SettlementChainItem this[FpML.Enum.PayerReceiverEnum payRec]
		{
			get 
			{
				if(FpML.Enum.PayerReceiverEnum.Payer ==  payRec)
					return settlementChainItems[0];
				else
					 return settlementChainItems[1];
			}
		}
		#endregion indexor
		//
		#region constructor
		public SettlementChain()
		{
			settlementChainItems = new SettlementChainItem[2]{new SettlementChainItem(),new SettlementChainItem()};
		}
		#endregion SettlementChain
		//	
		#region public SetSettlementInstruction
		public void  SetSettlementInstruction (SettlementInstruction pfpmlInstruction, SQL_Event pEvent, EFS_TradeLibrary pTradeLibrary)
		{
			try
			{
				int    idCss = 0;
				RoutingIdsAndExplicitDetails routingInfo = null;
				string cs = pTradeLibrary.ConnectionString; 
				//
				SettlementInstruction       fpmlsi = (SettlementInstruction) pfpmlInstruction.Clone(); 
				EfsSettlementInstruction  efsSiPay = new EfsSettlementInstruction(); 
				EfsSettlementInstruction  efsSiRec = new EfsSettlementInstruction(); 
				//
				// Payer
				#region Payer
				if (fpmlsi.settlementMethodSpecified)
				{
					efsSiPay.settlementMethod = fpmlsi.settlementMethod; 
					SQL_Css sqlCss = new SQL_Css(cs,efsSiPay.settlementMethod.Value,SQL_Table.ScanDataDtEnabledEnum.Yes);   
					if (sqlCss.IsLoaded)
					{
						idCss   = sqlCss.Id; 
						efsSiPay.settlementMethodInformation = new Routing(); 
						SiActorsInfo cssInfo = new SiActorsInfo(cs, new ArrayList(new int[]{sqlCss.Id}),idCss);      
						routingInfo = cssInfo.GetRouting2(idCss); 
						if (null != routingInfo)
						{
							efsSiPay.settlementMethodInformationSpecified = true;
							efsSiPay.settlementMethodInformation.routingIdsAndExplicitDetails = cssInfo.GetRouting2(idCss); 
						}
					}
				}
				//
				efsSiPay.correspondentInformationSpecified = fpmlsi.correspondentInformationSpecified;
				if (fpmlsi.correspondentInformationSpecified)
					efsSiPay.correspondentInformation = fpmlsi.correspondentInformation; 
				//
				// Recherche du beneficiaryBank & beneficiary  (non alimenté côté payer par saisie fpml)
				SiActorsInfo actorsInfo = null;
				SQL_Book sqlBookPayer   = null;
				if (pEvent.IdBPayer>0)
				{
					sqlBookPayer = new SQL_Book(pTradeLibrary.ConnectionString, pEvent.IdBPayer);  	
					if (sqlBookPayer.IsLoaded)
						actorsInfo = new SiActorsInfo(cs, new ArrayList(new int[]{pEvent.IdAPayer,sqlBookPayer.IdA_Entity}),idCss);      
				}
				else
					actorsInfo = new SiActorsInfo(cs, new ArrayList(new int[]{pEvent.IdAPayer}),idCss);      	
				//
				// Recherche du beneficiaryBank  (non alimenté côté payer par saisie fpml)
				efsSiPay.beneficiaryBank = new Routing(); 
				if ( (null != sqlBookPayer) &&  (sqlBookPayer.IdA_Entity >0) ) // Book Interne
					routingInfo = actorsInfo.GetRouting2(sqlBookPayer.IdA_Entity); 
				else	
					 routingInfo = actorsInfo.GetRouting2(pEvent.IdAPayer); 
				//
				if (null != routingInfo)
				{
					efsSiPay.beneficiaryBankSpecified = true;
					efsSiPay.beneficiaryBank.routingIdsAndExplicitDetails = routingInfo;
				}
				//
				// Recherche du beneficiary  (non alimenté côté payer par saisie fpml)
				efsSiPay.beneficiary = new Routing(); 
				routingInfo = actorsInfo.GetRouting2(pEvent.IdAPayer); 
				if (null != routingInfo)
					efsSiPay.beneficiary.routingIdsAndExplicitDetails = routingInfo; 
				#endregion Payer
				//
				// Receiver
				if (fpmlsi.beneficiaryBankSpecified)
					efsSiRec.beneficiaryBank     = fpmlsi.beneficiaryBank; 
				efsSiRec.beneficiary             = fpmlsi.beneficiary;
				if (fpmlsi.intermediaryInformationSpecified)
					efsSiRec.intermediaryInformation = fpmlsi.intermediaryInformation;  	
				//
				this[FpML.Enum.PayerReceiverEnum.Payer].settlementInstructions = efsSiPay;
				this[FpML.Enum.PayerReceiverEnum.Receiver].settlementInstructions = efsSiRec;
			}
			catch (Exception ex){throw ex;}
		}

		#endregion public SetSettlementInstruction
	}
	#endregion SettlementChain

	#region class SettlementChainItem
	/// <summary>
	/// 
	/// </summary>
	public class SettlementChainItem
	{
		public  SiModeEnum siMode; 
		public  EfsSettlementInstruction settlementInstructions;    
	
		#region constructor
		public SettlementChainItem()
		{
			settlementInstructions = new EfsSettlementInstruction();
			siMode                 = SiModeEnum.Undefined;
		}
		#endregion constructor
	}
	#endregion SettlementChainItem

	#region class SiActorsInfo
	/// <summary>
	/// Load Info For Routing 
	/// </summary>
	public  class SiActorsInfo
	{
		#region members 
		private DataTable dt;
		#endregion members
		
		#region indexor
		public DataRow  this[int pidA]
		{
			get 
			{
				DataRow  ret =null; 
				if (null != dt)
				{
					for (int i=0;i< dt.Rows.Count ;i++)
					{
						if ( Convert.ToInt32(dt.Rows[i]["IDA"]) == pidA)
						{
							ret = dt.Rows[i];
							break;
						}
					}
				}
				return ret;
			}
		}

		#endregion indexor

		#region constructor
		public SiActorsInfo(string pSource, ArrayList pListIdA, int pIdCss)
		{
			try
			{
				bool existCss = (pIdCss != 0);
				// ACTOR ( INTERMEDIARE,ACCOUNTOWNER, Etc ....)
				string sql = string.Empty; 
				sql += SQLCst.SELECT + "a.IDA," + DataHelper.SQLIsNull(pSource,"a.BIC","a.IDENTIFIER","IDENTIFIER") + "," + Cst.CrLf;
				sql += "a.DISPLAYNAME,a.ADDRESS1,a.ADDRESS2,a.ADDRESS3,a.ADDRESS4,a.ADDRESS5,a.ADDRESS6," + Cst.CrLf;
				if (existCss)	
					sql +="cssm.CSSMEMBERIDENT" + Cst.CrLf;
				else
					sql +="null as CSSMEMBERIDENT" + Cst.CrLf;
				sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
				if(existCss)
				{
					sql += SQLCst.LEFTJOIN + Cst.OTCml_TBL.CSMID + " cssm on cssm.IDA=a.IDA" + Cst.CrLf;
					sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pSource, "cssm").ToString() + Cst.CrLf;
					sql += SQLCst.AND + "cssm.IDA_CSS = " + pIdCss.ToString() + Cst.CrLf; 
				}
				sql += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pSource, "a").ToString();
				sql += SQLCst.AND   + "a.IDA In (" + DataHelper.SQLArrayListToSqlList(pListIdA,Cst.TypeData.Int ) + ")";
				//
				DataSet   ds = DataHelper.ExecuteDataset(pSource, CommandType.Text, sql);
				dt = ds.Tables[0];
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "IssiActorsInfo.Constructor", ex.Message,ex); 
			}
		}
		#endregion	
			
		#region public Function
		
		#region public GetIdentifier
		public string   GetIdentifier (int pIda)
		{
			string ret = null;
			DataRow dr = this[pIda];
			if (null!= dr)
			{
				ret  = dr["IDENTIFIER"].ToString(); 			
			}
			return ret ;
		}
		#endregion public GetIdentifier
		
		#region public GetCssIdentifier
		public string   GetCssIdentifier (int pIda)
		{
			string ret = null;
			DataRow dr = this[pIda];
			if (null!= dr)
			{
				ret  = dr["CSSMEMBERIDENT"].ToString(); 			
			}
			return ret ;
		}
		#endregion public GetCssIdentifier
		
		#region public GetCountry
		public string   GetCountry (int pIda)
		{
			string ret = null;
			DataRow dr = this[pIda];
			if (null!= dr)
			{
				ret  = dr["ADDRESS6"].ToString(); 			
			}
			return ret ;
		}
		#endregion public GetCountry
		
		#region public GetAdress
		public string[] GetAdress  (int pIda)
		{
			string[] ret = null;
			//
			try
			{
				DataRow dr = this[pIda];
				if (null!= dr)
				{
					ArrayList listAdr = new ArrayList(); 
					for (int i=1; i < 7;i++)
					{
						if (StrFunc.IsFilled(dr["ADDRESS" + i.ToString()].ToString()))
							listAdr.Add(dr["ADDRESS" + i.ToString()].ToString());  
					}
					ret = (string[]) listAdr.ToArray(typeof(string));
				}
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "GetRouting",ex.Message,ex,null);
			}
			return ret;
		}
		public EFS_StringArray[] GetAdress_EFS_StringArray  (int pIda)
		{
			EFS_StringArray [] ret = null;
			//
			string[] adress =  this.GetAdress(pIda);
			//
			if (ArrFunc.IsFilled(adress))
			{
				ArrayList listAdr = new ArrayList(); 
				for (int i=0; i < adress.Length ; i++)
				{
					EFS_StringArray adress2 = new EFS_StringArray(adress[0]);
					listAdr.Add( adress2);
				}
				ret = (EFS_StringArray[]) listAdr.ToArray(typeof(EFS_StringArray));
			}
			//
			return ret;
		}

		#endregion public GetAdress

		#region public GetRouting
		public RoutingIdsAndExplicitDetails  GetRouting2(int pIda)
		{
			RoutingIdsAndExplicitDetails ret = new RoutingIdsAndExplicitDetails();  
			//
			RoutingId rId = null;
			try
			{
				//routingIds
				ArrayList listId = new ArrayList();
				//
				rId = new RoutingId();
				rId.Value = pIda.ToString();
				rId.routingIdCodeScheme = Cst.OTCml_ActorIdScheme; 
				listId.Add(rId);
				//
				string cssIdentifier = this.GetCssIdentifier(pIda); 
				if (StrFunc.IsFilled(cssIdentifier))
				{
					rId = new RoutingId();
					rId.Value = cssIdentifier;
					rId.routingIdCodeScheme  =  Cst.OTCml_CssActorIdentifierScheme;
					listId.Add(rId);
				}
				ret.routingIds  = new RoutingIds[1] { new RoutingIds()}; 
				ret.routingIds[0].routingId  = (RoutingId[]) listId.ToArray(typeof(RoutingId));   
				//
				//routingName       => Identifier
				ret.routingName     = new EFS_String(this.GetIdentifier(pIda)); 
				//
				//routingAddress
				ret.routingAddress  = new Address(); 
				ret.routingAddress.streetAddress  = this.GetAdress_EFS_StringArray (pIda); 

				//Adress (country)
				string country = this.GetCountry(pIda);
				if (StrFunc.IsFilled(country))
				{
					ret.routingAddress.country = new Country(); 
					ret.routingAddress.country.Value = country ; 
				}
				ret.routingAddressSpecified  = (ret.routingAddress.streetAddress!=null || ret.routingAddress.country != null);
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "GetRouting",ex.Message,ex,null);
			}
			return ret;
		}
		
		#endregion
		
		#endregion
	}
	#endregion
		
	#region class SsiDbCollection
	/// <summary>
	/// Load Info of SsiDbs
	/// </summary>
	[System.Xml.Serialization.XmlRootAttribute("SSIDBITEMS", Namespace="", IsNullable=true)]
	public class SsiDbCollection : ICollection , IComparer  
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("SSIDBITEM")]
		public SsiDbItem[] ssidbItems; 
		#endregion Members		

		#region public indexor
		public SsiDbItem  this[int pIndex]
		{
			get 
			{
				return ssidbItems[pIndex];
			}
		}
		public SsiDbItem  this[int pIdA, int pSequenceNO]
		{
			get 
			{
				SsiDbItem ret =null; 
				if (null != ssidbItems)
				{
					for (int i=0;i< ssidbItems.Length;i++)
					{
						if ( ssidbItems[i].idA== pIdA && ssidbItems[i].sequenceNumber == pSequenceNO )
						{
							ret = ssidbItems[i];
							break;
						}
					}
				}
				return ret;
			}
		}
		#endregion indexor

		#region constructor
		public SsiDbCollection() {}
		#endregion constructor
		#region public InitializeActorSsiDb
		public   void InitializeActorSsiDb(string pSource, int pIdA)
		{
			SsiDbCollection ssidbCol = SsiDbCollection.LoadActorSsiDb(pSource,pIdA);
			ssidbItems = ssidbCol.ssidbItems; 
		}
		#endregion
		#region  public static Load
		public  static SsiDbCollection LoadActorSsiDb(string pSource, int pIdA)
		{
			SsiDbCollection ssiDbItems =  null;
			//
			try
			{
				SQL_SSIdb     sql_ssidb = new SQL_SSIdb(pSource,SQL_Table.ScanDataDtEnabledEnum.Yes, pIdA);
				string sql =  sql_ssidb.GetQuery(
					new string[]{"IDSSIDB","IDA","DESCRIPTION",
									DataHelper.SQLRTrim(pSource, "DBTYPE","DBTYPE"),"IDA_CUSTODIAN",
									"SEQUENCENO",
									DataHelper.SQLRTrim(pSource, "CODE","CODE"),
									"URL","IDSSIFORMATREQUEST","IDSSIFORMATANSWER","EXTLLINK"}); 
				//
				DataSet dsResult             = DataHelper.ExecuteDataset(pSource,CommandType.Text,sql);
				dsResult.DataSetName         = "SSIDBITEMS";
				DataTable dtTable            = dsResult.Tables[0]; 
				dtTable.TableName			 = "SSIDBITEM";	
				//
				StringBuilder sb  = new StringBuilder();
				TextWriter writer = new StringWriter(sb);
				dsResult.WriteXml(writer);
				//
				ssiDbItems = (SsiDbCollection) XML.Deserialize(typeof(SsiDbCollection),new StringReader(sb.ToString()));
			}
			catch (Exception ex)
			{
				throw new OTCmlException("Load",ex); 
			}
			return ssiDbItems;
		}
		#endregion Load
		#region public Sort
		public bool Sort()
		{
			bool isOk = false;
			try
			{
				isOk = (Count > 0)  ;
				if (isOk) 
					Array.Sort(ssidbItems,this);  
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "SsiDbCollection.Sort","Error",ex,null); 
			}
			return isOk;
		}
		#endregion

		#region Membres de ICollection
		public bool IsSynchronized
		{
			get
			{
				return ssidbItems.IsSynchronized;
			}
		}
		public int Count
		{
			get
			{
				int ret = 0;
				if  (ArrFunc.IsFilled(ssidbItems))
					ret = ssidbItems.Length; 
				return ret;
			}
		}

		public void CopyTo(Array array, int index)
		{
			ssidbItems.CopyTo(array,index);   
		}
		public object SyncRoot
		{
			get
			{
				return ssidbItems.SyncRoot;  
			}
		}

		#endregion
		#region Membres de IEnumerable
		public IEnumerator GetEnumerator()
		{
			return ssidbItems.GetEnumerator(); 
		}
		#endregion
		#region Membres de IComparer
		public int Compare(object x, object y)
		{
			int 	ret = 0; 
			//Order by idA,sequenceNumber
			//si ret = -1, ssiDbItemX < ssiDbItemY,  ssiDbItemX est prioritaire 
			//si ret =  1, ssiDbItemY < ssiDbItemX,  ssiDbItemY est prioritaire 
			//
			try
			{
				SsiDbItem ssiDbItemX = (SsiDbItem) x;
				SsiDbItem ssiDbItemY = (SsiDbItem) y;
				if ( (x is SsiDbItem) && (y is SsiDbItem)) 
				{
					ret  = (ssiDbItemX.idA - ssiDbItemY.idA);
					if (ret == 0)
					{
						ret  = 	(ssiDbItemX.sequenceNumber - ssiDbItemY.sequenceNumber);			
					}
				}
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "SsiDbCollection.Compare","Error",ex,null); 
			}
			return ret;
		}
		#endregion

	}
	#endregion

	#region class SsiDbItem
	public class SsiDbItem
	{
		public const string  LocalDatabase = ".";
		#region Accessor
		public EfsML.Enum.StandInstDbType dbTypeEnum
		{
			get
			{
				EfsML.Enum.StandInstDbType dbTypeEnum = EfsML.Enum.StandInstDbType.Other;
				EfsML.Enum.StandInstDbType ret   = EfsML.Enum.StandInstDbType.Other;
				//
				FieldInfo[] flds = dbTypeEnum.GetType().GetFields();
				foreach (FieldInfo fld in flds)
				{
					object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute),true);
					if ((0 != attributes.GetLength(0)) && (dbType == ((XmlEnumAttribute) attributes[0]).Name))
					{
						ret = ( EfsML.Enum.StandInstDbType) fld.GetValue(dbTypeEnum);
						break;
					}
				}
				return ret;
			}
		}	
		public bool IsLocalDatabase
		{
			get{ return (url ==  LocalDatabase);}
		}
		#endregion Accessor
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("IDSSIDB")]	
		public int idIssiDb;   
		//		
		[System.Xml.Serialization.XmlElementAttribute("IDA")]	
		public int idA; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool descriptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("DESCRIPTION")]	
		public string description;
		//
		[System.Xml.Serialization.XmlElementAttribute("DBTYPE")]	
		public string    dbType;
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool idACustodianSpecified;
		[System.Xml.Serialization.XmlElementAttribute("IDA_CUSTODIAN")]	
		public int  idACustodian;
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sequenceNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SEQUENCENO")]	
		public int  sequenceNumber; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool codeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("CODE")]	
		public   EfsML.Enum.SettlementStandingInstructionDatabase1CodeEnum  code; 
		//
		[System.Xml.Serialization.XmlElementAttribute("URL")]	
		public  string  url; 
		//
		[System.Xml.Serialization.XmlElementAttribute("IDSSIFORMATREQUEST")]	
		public  string  idformatRequest; 
		//
		[System.Xml.Serialization.XmlElementAttribute("IDSSIFORMATANSWER")]	
		public  string  idformatAnswer; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]	
		public  bool    extLinkSpecified;
		[System.Xml.Serialization.XmlElementAttribute("EXTLLINK")]	
		public  string  extLink;
		#endregion Memebers
	}
	#endregion class SsiDbItem  

	#region class SettlementInstrTradeGen
	public class SettlementInstrTradeGen
	{
		#region  Members 
		private   SettlementInstrGenProcess siProcess;
		private   SettlementInstrGenMQueue  settlementInstrGenMQueue;
		
		private   string                    connectionString;
		private   EFS_TradeLibrary          tradeLibrary;
		private   DataSetTrade	            dsTrade;
		private   DataSetEventTrade         dsEvents;
		private   DataSet                   dsEventSi;         
		
		#endregion
		//
		#region Accessor
		#region public DtEvent
		public  DataTable DtEvent
		{
			get
			{
				if (null != dsEvents)
					return dsEvents.DtEvent; 
				else	
					return null;
			}
		}
		#endregion DtEvent
		#region public DtEventSi
		public  DataTable DtEventSi
		{
			get
			{
				if (null != dsEventSi)
					return dsEventSi.Tables["EventSi"];
				else 
					return null;
			}
		}
		#endregion DtEventSi
		#endregion Accessor
		//
		#region constructor
		public  SettlementInstrTradeGen(SettlementInstrGenProcess pSiProcess,DataSetTrade pDsTrade,EFS_TradeLibrary pTradeLibrary,
			ref Cst.ErrLevel pRet, ref ArrayList pOTCmlException)
		{
			siProcess                 = pSiProcess; 
			settlementInstrGenMQueue  = pSiProcess.settlementInstrGenMQueue;
			connectionString          = settlementInstrGenMQueue.connectionString;
			dsTrade                   = pDsTrade;
			tradeLibrary              = pTradeLibrary;
			dsEvents                  = new DataSetEventTrade(connectionString,dsTrade.IdT);
			GetSelectEventSi();
			pRet                      = Execute(ref pOTCmlException);
		}
		#endregion constructor

		#region Private Methode
		#region Private Execute
		private Cst.ErrLevel Execute(ref ArrayList pOTCmlException)
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCES;
			pOTCmlException  = new ArrayList();
			try
			{
				#region RowsEvent with STL Event Class
				bool isRowMustBeCalculate = false;
				foreach (DataRow rowEvent in dsEvents.RowsEvent)  // Voir ici pour ne considerer que certaine ligne (Restriction grâce au fichier)
				{
					isRowMustBeCalculate = IsRowHasSettlementClass(rowEvent);
					
					if (isRowMustBeCalculate)
					{
						try
						{
							int idEvent = Convert.ToInt32(rowEvent["ID"]);
							//
							if (Cst.ErrLevel.SUCCES != siProcess.ScanCompatibility_Event(idEvent))
								isRowMustBeCalculate = false;
							//
							SearchEventSi  searchsi = new SearchEventSi(idEvent,tradeLibrary); 
							searchsi.LoadSettlementChain(); 
							//
							UpdDataEventSi(idEvent,searchsi.SettlementChain ,PayerReceiverEnum.Receiver);    
							UpdDataEventSi(idEvent,searchsi.SettlementChain,PayerReceiverEnum.Payer);    
							//
						}
						catch (OTCmlException otcmlException)
						{
							if (otcmlException.IsFatalError)
							{
								ret =  Cst.ErrLevel.UNDEFINED;
								pOTCmlException.Add(otcmlException);
							}
						}
						catch (Exception ex) 
						{
							throw new OTCmlException("SettlementFindSi..Execute",ex);
						}
						finally
						{
							if (isRowMustBeCalculate)
								ret = Update();
						}
					}
				}
				#endregion ExchangeCurrency with fixing Process
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.SettlementFindSi..Execute",ex);}
			return ret;
		}	
		#endregion Valorize
		
		#region UpdDataEventSi
		private void UpdDataEventSi(int pIdEvt, SettlementChain pSettlementChain , FpML.Enum.PayerReceiverEnum pPayRec)
		{
			//
			try
			{
				StringBuilder sb = XML.Serialize(typeof(EfsSettlementInstruction),pSettlementChain[pPayRec].settlementInstructions, null);
				//	
				DataRow row =null;
				object[]findTheseVals = new object[2];
				findTheseVals[0] = pIdEvt;
				findTheseVals[1] = pPayRec.ToString();
				row = DtEventSi.Rows.Find(findTheseVals);
				bool isAddNewRow = (null == row);
				//
				if(isAddNewRow)
					row =  DtEventSi.NewRow();
				//
				row["IDPARENT"] = pIdEvt;
				row["PAYER_RECEIVER"]= pPayRec.ToString() ;
				row["SIXML"] = sb.ToString(); 
				row["SIMODE"] = pSettlementChain[pPayRec].siMode.ToString();  
				//
				if(isAddNewRow)
					DtEventSi.Rows.Add(row);
			}
			catch(Exception ex)
			{
				throw new OTCmlException("SettlementInstr.UpdDataEventSi","Error",ex,null);
			}
		}
		#endregion UpdDataEventSi

		#region private IsRowHasSettlementClass
		private bool IsRowHasSettlementClass(DataRow pRow)
		{
			try
			{
				DataRow[] rowChilds = pRow.GetChildRows(dsEvents.ChildEventClass);
				foreach (DataRow rowChild in rowChilds)
				{
					if ( Convert.ToString(rowChild["EVENTCLASS"]) == EventClassFunc.Settlement) 
						return true;
				}
			}
			catch (OTCmlException otcmlException) {throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.SettlementFindSi..IsRowHasSettlementClass",ex);}
			return false;
		}
		#endregion IsRowHasSettlementClass
		#region private Update
		private Cst.ErrLevel Update()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.BUG;
			IDbTransaction dbTransaction = null;
			try
			{
				DataTable dtChanges = DtEventSi.GetChanges();
				dbTransaction       = DataHelper.BeginTransaction(connectionString);
				ret                 = UpdateEventSi(dbTransaction);
				//
				EventProcess eventProcess = new EventProcess(connectionString);
				if (Cst.ErrLevel.SUCCES == ret)
				{
					DateTime dtSys = OTCmlHelper.GetDtSys();
					foreach (DataRow row in DtEventSi.Rows)
					{
						int idE = Convert.ToInt32(row["IDPARENT"]);
						eventProcess.Write(dbTransaction,idE,Cst.ProcessTypeEnum.SIGEN,Cst.StatusProcess.SUCCESS,dtSys);
					}
				}
				ret = (Cst.ErrLevel) DataHelper.CommitTransaction( dbTransaction );
			}
			catch (Exception ex) 
			{
				DataHelper.RollbackTransaction(dbTransaction);
				throw new OTCmlException("EFS.SettlementFindSi..Update",ex);
			}
			return ret;
		}
		#endregion Update
		#region private UpdateEventSi
		private Cst.ErrLevel UpdateEventSi(IDbTransaction pDbTransaction)
		{
			try
			{
				#region EventSi
				string SQLSelect = GetSelectEventSiColumn(true);
				int rowsAffected = DataHelper.ExecuteDataAdapter(pDbTransaction,SQLSelect,DtEventSi);
				#endregion EventSi
			}
			catch (Exception ex) {throw ex;}
			return Cst.ErrLevel.SUCCES;
		}
		#endregion UpdateEvent

		#region private GetSelectEventSi
		private Cst.ErrLevel GetSelectEventSi()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			try
			{
				string SQLSelect  = GetSelectEventSiColumn();
				SQLSelect        += SQLCst.WHERE + @"(e.IDT=@IDT)" + Cst.CrLf;
				SQLSelect        += SQLCst.ORDERBY + "e.IDE" + Cst.CrLf;
				SQLSelect         = DataHelper.ReplaceVarPrefix(connectionString, SQLSelect);
				dsEventSi         = DataHelper.ExecuteDataset(connectionString,CommandType.Text,SQLSelect,dsEvents.ParamIdT);
				#region DataSet Initialize
				//ds
				dsEventSi.DataSetName = "EventSi";
				// dt
				DataTable dt          = dsEventSi.Tables[0];
				dt.TableName          = "EventSi";
				DataColumn[] columnPK = new DataColumn[2] {dt.Columns["IDPARENT"], dt.Columns["PAYER_RECEIVER"]};
				dt.PrimaryKey = columnPK;		
				#endregion DataSet Initialize
				
				ret = Cst.ErrLevel.SUCCES;
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex) {throw new OTCmlException("EFS.SettlementFindSi..GetSelectEventSi",ex);}		
			return ret;
		}
		#endregion GetSelectEventSi
		#region private GetSelectEventSiColumn
		private string GetSelectEventSiColumn()
		{
			return GetSelectEventSiColumn(false);
		}
		private string GetSelectEventSiColumn(bool pWithOnlyTblMain)
		{
			string SQLSelect = SQLCst.SELECT + @"esi.IDE as IDPARENT, esi.PAYER_RECEIVER, esi.SIXML," + Cst.CrLf;
			SQLSelect       += @"esi.INVESTORXML,esi.SIMODE,esi.IDSSIDB,esi.IDCSSLINK" + Cst.CrLf;
			SQLSelect       += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENTSI  + " esi " + Cst.CrLf;
			if (false == pWithOnlyTblMain)
			{
				SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENT + " e ";
				SQLSelect += SQLCst.ON + "(e.IDE = esi.IDE)" + Cst.CrLf;
			}
			return SQLSelect;
		}
		#endregion GetSelectEventSi
		#endregion Private Methode
	}
	#endregion
		
	#region class IssiCollection
	[System.Xml.Serialization.XmlRootAttribute("ISSIS", Namespace="", IsNullable=true)]
	public class IssiCollection : IComparer , ICollection		 
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("ISSI")]
		public  Issi[] issis; 
		
		//Contexte Pour le tri 
		private  SsiCriteria        _ssiCriteria;
		private  SQL_Event          _sqlEvent;
		private  PayerReceiverEnum  _payRec;
		#endregion Members
		
		#region public indexor
		public Issi this[int pIndex]
		{
			get 
			{
				return issis[pIndex];  
			}
		}
		#endregion index
		
		#region constructor
		public IssiCollection(){}
		#endregion
		
		#region public static  Load
		public  static IssiCollection Load(SQL_Event pEvent, EFS_TradeLibrary pTradeLibrairy, int pIdA,PayerReceiverEnum pPayRec,SsiCriteria pSsiCriteria)
		{
			IssiCollection ret =  null;
			//
			int idCounterparty = 0;
			idCounterparty = pEvent.GetidConterparty( pPayRec);
			//
			string sql = SQLCst.SELECT + "IDISSI,IDA,SOURCECODE,CASHSECURITIES,SEQUENCENO,SIDE,IDC,QUOTEPLACE,IDA_COUNTERPARTY,IDB" + Cst.CrLf  ;
			sql += SQLCst.FROM  + "ISSI"  + Cst.CrLf ;
			sql += SQLCst.WHERE + "IDA="  + pIdA.ToString() + Cst.CrLf ;
			sql += SQLCst.AND + "(CASHSECURITIES=" + DataHelper.SQLString("Cash") + ")" + Cst.CrLf ; 
			sql += SQLCst.AND + "(SIDE is null Or SIDE=" + DataHelper.SQLString(pPayRec.ToString()) + ")" + Cst.CrLf ;
			sql += SQLCst.AND + "(IDC  is null Or IDC=" + DataHelper.SQLString(pEvent.Unit)  + ")" + Cst.CrLf;
			sql += SQLCst.AND + "(IDA_COUNTERPARTY  is null Or IDA_COUNTERPARTY=" + idCounterparty.ToString() + ")" + Cst.CrLf;
			if( null != pSsiCriteria  && pSsiCriteria.ssicountrySpecified) 
				sql += SQLCst.AND + "(IDCOUNTRY  is null Or IDCOUNTRY=" + DataHelper.SQLString(pSsiCriteria.ssiCountry.Value) + ")" + Cst.CrLf;
			//
			try
			{
				DataSet dsResult             = DataHelper.ExecuteDataset(pTradeLibrairy.ConnectionString,CommandType.Text,sql);
				dsResult.DataSetName         = "ISSIS";
				DataTable dtTable            = dsResult.Tables[0]; 
				dtTable.TableName			 = "ISSI";	
				//
				StringBuilder sb  = new StringBuilder();
				TextWriter writer = new StringWriter(sb);
				dsResult.WriteXml(writer);
				//
				ret = (IssiCollection) XML.Deserialize(typeof(IssiCollection),new StringReader(sb.ToString()));
			}
			catch (Exception ex)
			{
				throw new OTCmlException("Load",ex); 
			}
			return ret;
		}
		#endregion Load
		
		#region public Initialize
		public void Initialize(SQL_Event psqlEvent, EFS_TradeLibrary pTradeLibrary, int pIdA,PayerReceiverEnum pPayRec, SsiCriteria pCriteria )
		{
			try
			{
				issis = null;	
				// Use For Sort
				_sqlEvent    = psqlEvent;
				_payRec      = pPayRec;
				_ssiCriteria = pCriteria;
				//
				IssiCollection issiCol = IssiCollection.Load(_sqlEvent,pTradeLibrary,pIdA,_payRec,_ssiCriteria);
				if (null != issiCol && (issiCol.Count >0) )
					issis = issiCol.issis; 
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "ISSICollection.Initialize","Error",ex,null); 
			}
		}
		#endregion public 
		
		#region public Sort
		public bool Sort()
		{
			bool isOk = false;
			try
			{
				isOk = (null != _sqlEvent) && (Count > 0)  ;
				if (isOk) 
					Array.Sort(issis,this);  
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "ISSICollection.Sort","Error",ex,null); 
			}
			return isOk;
		}
		#endregion
		
		#region Membres de IComparer
		public int Compare(object x, object y)
		{
			int 	ret = 0; 
			//
			//si ret = -1, issiX < issiY,  issiX est prioritaire prioritaire
			//si ret =  1, issiY < issiX,  issiY est prioritaire prioritaire
			//
			try
			{
			
				if ( (x is Issi) && (y is Issi)  && (null!=_sqlEvent && _sqlEvent.IsLoaded) ) 
				{
					Issi issiX = (Issi) x;		
					Issi issiY = (Issi) y;
				
					//BOOK
					int idBook = _sqlEvent.GetIdBook(_payRec);
					if (idBook>0)
					{
						//Priorite aux Issis de même book 
						if ( (( issiX.idBSpecified) &&  (issiX.idB == idBook)) 
							&&  
							((!issiY.idBSpecified) ||  ((issiY.idBSpecified) && issiY.idB != idBook))  //No Book Or different than idBook
							)		 
							ret = -1 ;
						else if 
							( (( issiY.idBSpecified) &&  (issiY.idB == idBook)) 
							&&  
							((!issiX.idBSpecified) ||  ((issiX.idBSpecified) && issiX.idB != idBook)) //No Book Or different than idBook
							)		 
							ret = 1 ;
					}
					else
					{
						// Priorite aux issi sans pays (= tous pays) 
						if ( !issiX.idBSpecified && issiY.idBSpecified)		 
							ret = -1 ;
						else if (issiX.idBSpecified && !issiY.idBSpecified)		 
							ret = 1 ;
					}
					//IDCONTERPARTY
					if (ret == 0)
					{
						int idAConterparty = _sqlEvent.GetidConterparty(_payRec);
						//Priorite aux Issis de même book 
						if ( (( issiX.idAConterpartySpecified) &&  (issiX.idAConterparty == idAConterparty)) 
							&&  
							((!issiY.idAConterpartySpecified) ||  ((issiY.idAConterpartySpecified) && issiY.idAConterparty  != idAConterparty))  //No Book Or different than idBook
							)		 
							ret = -1 ;
						else if 
							( (( issiY.idAConterpartySpecified) &&  (issiY.idAConterparty == idAConterparty)) 
							&&  
							((!issiX.idAConterpartySpecified) ||  ((issiX.idAConterpartySpecified) && issiX.idAConterparty != idAConterparty)) //No Book Or different than idBook
							)		 
							ret = 1 ;
					}
					//COUNTRY
					if (ret == 0)
					{
						string country = string.Empty;
						if ( (null != _ssiCriteria) &&  _ssiCriteria.ssicountrySpecified)  
							country = _ssiCriteria.ssiCountry.Value ; 
						//COUNTRY
						if (StrFunc.IsFilled(country) )
						{
							//Priorite aux Issis de même book 
							if ( (( issiX.countrySpecified) &&  (issiX.country == country)) 
								&&  
								((!issiY.countrySpecified) ||  ((issiY.countrySpecified) && issiY.country != country))  //No Book Or different than idBook
								)		 
								ret = -1 ;
							else if 
								( (( issiY.countrySpecified) &&  (issiY.country == country)) 
								&&  
								((!issiX.countrySpecified) ||  ((issiX.countrySpecified) && issiX.country != country)) //No Book Or different than idBook
								)		 
								ret = 1 ;
						}
						else
						{
							// Priorite aux issi sans pays (= tous pays) 
							if ( !issiX.countrySpecified && issiY.countrySpecified)		 
								ret = -1 ;
							else if (issiX.countrySpecified && !issiY.countrySpecified)		 
								ret = 1 ;
						}
					}
					//IDC
					if (ret == 0)
					{
						string idC = _sqlEvent.Unit;
						//Priorite aux Issis de même book 
						if ( (( issiX.idCSpecified) &&  (issiX.idC == idC)) 
							&&  
							((!issiY.idCSpecified) ||  ((issiY.idCSpecified) && issiY.idC != idC))  //No Book Or different than idBook
							)		 
							ret = -1 ;
						else if 
							( (( issiY.idCSpecified) &&  (issiY.idC == idC)) 
							&&  
							((!issiX.idCSpecified) ||  ((issiX.idCSpecified) && issiX.idC != idC)) //No Book Or different than idBook
							)		 
							ret = 1 ;
					}
					//SIDE
					if (ret == 0)
					{
						string side = _payRec.ToString()  ;
						//Priorite aux Issis de même book 
						if ( (( issiX.sideSpecified) &&  (issiX.side == side)) 
							&&  
							((!issiY.sideSpecified) ||  ((issiY.sideSpecified) && issiY.side != side))  //No Book Or different than idBook
							)		 
							ret = -1 ;
						else if 
							( (( issiY.sideSpecified) &&  (issiY.side == side)) 
							&&  
							((!issiX.sideSpecified) ||  ((issiX.sideSpecified) && issiX.side != side)) //No Book Or different than idBook
							)		 
							ret = 1 ;
					}
					//En cas d'égalité pririte en fonction de sequenceNo
					if(ret == 0)
					{
						if (issiX.sequenceNo < issiY.sequenceNo)
							ret = -1 ;
						else if (issiX.sequenceNo > issiY.sequenceNo)
							ret = 1;
					}
				}
				else
					throw new ArgumentException("object is not a ISSI");    
			
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "ISSICollection.Compare","Error",ex,null); 
			}
			//
			return ret;
		}
		
		#endregion Membres de IComparer
		
		#region Membres de ICollection
		public int Count
		{
			get
			{
				int ret = 0;
				if  (ArrFunc.IsFilled(issis))
					ret = issis.Length; 
				return ret;
			}
		}
		
		public void CopyTo(Array array, int index)
		{
			issis.CopyTo(array, index);
		}
		public IEnumerator GetEnumerator()
		{
			return issis.GetEnumerator();
		}
		public bool IsSynchronized
		{
			get {return issis.IsSynchronized;}
		}
		public object SyncRoot
		{
			get {return issis.SyncRoot;}
		}
		#endregion Membres
	}
	#endregion class IssiCollection
	
	#region class Issi
	public class Issi
	{
		[System.Xml.Serialization.XmlElementAttribute("IDISSI")]	
		public int idIssi;   
		//		
		[System.Xml.Serialization.XmlElementAttribute("IDA")]	
		public int idA; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sourceCodeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SOURCECODE")]	
		public string sourceCode; 
		//
		[System.Xml.Serialization.XmlElementAttribute("CASHSECURITIES")]	
		public string cashSecurities; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sequenceNoSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SEQUENCENO")]	
		public int  sequenceNo; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sideSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SIDE")]	
		public string  side; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool idCSpecified;
		[System.Xml.Serialization.XmlElementAttribute("IDC")]	
		public string idC; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool countrySpecified;
		[System.Xml.Serialization.XmlElementAttribute("COUNTRY")]	
		public string country; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool quotePlaceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("QUOTEPLACE")]	
		public string quotePlace; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool idAConterpartySpecified;
		[System.Xml.Serialization.XmlElementAttribute("IDA_COUNTERPARTY")]	
		public int  idAConterparty; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool idBSpecified;
		[System.Xml.Serialization.XmlElementAttribute("IDB")]	
		public int  idB; 
		//
	}
	#endregion class Issi  
	
	#region class IssiItemCollection
	[System.Xml.Serialization.XmlRootAttribute("ISSIITEMS", Namespace="", IsNullable=true)]
	public class IssiItemCollection : IComparer	 , IComparable	 	 
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("ISSIITEM")]
		public  IssiItem[] issiItems; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		private SiActorsInfo issiActorsInfo;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int  idIssi; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string source; 
		#endregion
		//
		#region constructor
		public IssiItemCollection(){}
		#endregion
		//
		#region public indexor
		public IssiItem this[Actor.RoleActorSSI pIdRole]
		{
			get 
			{
				return this[pIdRole,0];  
			}
		}
		public IssiItem this[Actor.RoleActorSSI pIdRole, int pSequenceNumber]
		{
			get 
			{
				IssiItem ret =null; 
				for (int i=0;i< issiItems.Length;i++)
				{
					if ( (issiItems[i].idRoleActor == pIdRole) && (issiItems[i].sequenceNumber==pSequenceNumber) )
					{
						ret = issiItems[i];
						break;
					}
				}
				return ret;
			}
		}
		#endregion index
		//
		#region public static  Load
		public  static IssiItemCollection Load(string pSource, int pIdIssi)
		{
			IssiItemCollection issiItems =  null;
			//
			SQL_IssiItem  sqlIssiIems = new SQL_IssiItem(pSource, pIdIssi,SQL_Table.ScanDataDtEnabledEnum.Yes);
			string sql =  sqlIssiIems.GetQuery(
				new string[]{"IDISSI","IDA",DataHelper.SQLRTrim(pSource, "IDROLEACTOR","IDROLEACTOR"),
								DataHelper.SQLIsNull(pSource,"SEQUENCENO","0","SEQUENCENO"),"CHAINPARTY","PARTYROLE","CACCOUNTNUMBER",
								"CACCOUNTNUMBERIDENT","CACCOUNTNAME","SACCOUNTNUMBER","SACCOUNTNUMBERIDENT","SACCOUNTNAME"}); 
			//
			try
			{
				DataSet dsResult             = DataHelper.ExecuteDataset(pSource,CommandType.Text,sql);
				dsResult.DataSetName         = "ISSIITEMS";
				DataTable dtTable            = dsResult.Tables[0]; 
				dtTable.TableName			 = "ISSIITEM";	
				//
				StringBuilder sb  = new StringBuilder();
				TextWriter writer = new StringWriter(sb);
				dsResult.WriteXml(writer);
				//
				issiItems = (IssiItemCollection) XML.Deserialize(typeof(IssiItemCollection),new StringReader(sb.ToString()));
				issiItems.source = pSource;
				issiItems.idIssi = pIdIssi;
			}
			catch (Exception ex)
			{
				throw new OTCmlException("Load",ex); 
			}
			return issiItems;
		}
		#endregion Load
		//
		#region public Initialize
		public void Initialize( string pSource, int pIdIssi)
		{
			IssiItemCollection issiItemsCol = IssiItemCollection.Load(pSource,pIdIssi);
			//	
			issiItems = null;	
			issiItems = issiItemsCol.issiItems; 
			//
			source    = null;	
			source    = issiItemsCol.source;
			//
			idIssi    =  0;	
			idIssi    = issiItemsCol.idIssi;
			//
			#region Maj des Sequence Number
			int occurence = 0;
			string RoleActor = string.Empty ;
			for (int i=0; i < issiItems.Length  ;i++)
			{
				if (issiItems[i].idRoleActor.ToString() == RoleActor)
				{
					occurence ++;
				}
				else
				{
					RoleActor = issiItems[i].idRoleActor.ToString();
					occurence =0;
				}
				issiItems[i].sequenceNumber = occurence ;
			}
			#endregion Maj des Sequence Number
			//
			int idCss = 0; 
			IssiItem css = this[Actor.RoleActorSSI.CSS];
			if(null != css)
				idCss = css.idA; 
			issiActorsInfo = new SiActorsInfo(issiItemsCol.source, GetActorList(),  idCss); 		
		}
		#endregion public 
		//
		#region  public GetInstruction
		public EfsSettlementInstruction GetInstruction(PayerReceiverEnum pPayRec)
		{
			EfsSettlementInstruction si = new EfsSettlementInstruction();
			RoutingIdsAndExplicitDetails routingInfo  = null;
			//
			// CSS
			IssiItem css = this[Actor.RoleActorSSI.CSS];
			si.settlementMethodInformationSpecified  = (null != css);
			if (null != css)
			{
				routingInfo = null;
				routingInfo = GetRoutingIdsAndExplicitDetails(css);
				//
				si.settlementMethodInformation = new Routing(); 
				si.settlementMethodInformation.routingIdsAndExplicitDetailsSpecified = (null != routingInfo);
				if(null != routingInfo)
				{
					si.settlementMethodInformation.routingIdsAndExplicitDetails = routingInfo;  
					si.settlementMethod.Value = si.settlementMethodInformation.routingIdsAndExplicitDetails.routingName.Value;   
				}
			}
			//
			if (PayerReceiverEnum.Payer == pPayRec)
			{
				// CORRESPONDANT
				IssiItem agent = this[Actor.RoleActorSSI.AGENT];
				si.correspondentInformationSpecified  = (null != agent);
				if (null != agent)
				{
					routingInfo = null;
					routingInfo = GetRoutingIdsAndExplicitDetails(agent);
					//
					si.correspondentInformation = new Routing(); 
					si.correspondentInformation.routingIdsAndExplicitDetailsSpecified = (null != routingInfo);
					if(null != routingInfo)
						si.correspondentInformation.routingIdsAndExplicitDetails = routingInfo;  
				}
			}
			//
			//INTERMEDIAIRE
			if (PayerReceiverEnum.Receiver == pPayRec)
			{
				ArrayList listIntermediary = new ArrayList(); 
				int  i = 0;
				while (i > -1)
				{
					IssiItem intermediary = this[Actor.RoleActorSSI.INTERMEDIARY,i];
					if (null == intermediary)
						i=-1;
					else 
					{
						i++;
						IntermediaryInformation interInfo = new IntermediaryInformation(); 
						if (intermediary.sequenceNumberSpecified)
							interInfo.intermediarySequenceNumber = new EFS_PosInteger(intermediary.sequenceNumber + 1 ); 
						//
						routingInfo = null;
						routingInfo = GetRoutingIdsAndExplicitDetails(intermediary);
						interInfo.routingIdsAndExplicitDetailsSpecified = (null != routingInfo);
						if(null != routingInfo)
							interInfo.routingIdsAndExplicitDetails  = routingInfo;
						//
						listIntermediary.Add(interInfo);
					}
				}
				si.intermediaryInformationSpecified = (listIntermediary.Count>0); 
				if (listIntermediary.Count >0 )
					si.intermediaryInformation  = (IntermediaryInformation[]) listIntermediary.ToArray(typeof(IntermediaryInformation));
			}
			//
			//ACCOUNTSERVICER
			IssiItem accountServicer = this[Actor.RoleActorSSI.ACCOUNTSERVICER];
			si.beneficiaryBankSpecified = (null != accountServicer);
			if (null != accountServicer)
			{
				routingInfo = null;
				routingInfo = GetRoutingIdsAndExplicitDetails(accountServicer);
				//				
				si.beneficiaryBank = new Routing(); 
				si.beneficiaryBank.routingIdsAndExplicitDetailsSpecified = (null != routingInfo);
				if(null != routingInfo)
					si.beneficiaryBank.routingIdsAndExplicitDetails = routingInfo;  
			}
			//
			//ACCOUNTOWNER
			IssiItem accountOwner = this[Actor.RoleActorSSI.ACCOUNTOWNER];
			if (null != accountOwner)
			{
				routingInfo = null;
				routingInfo = GetRoutingIdsAndExplicitDetails(accountOwner);
				//
				si.beneficiary  = new Routing(); 
				si.beneficiary.routingIdsAndExplicitDetailsSpecified = (null != routingInfo);
				if(null != routingInfo)
					si.beneficiary.routingIdsAndExplicitDetails = routingInfo;  
			}
			//INVESTOR
			IssiItem investor = this[Actor.RoleActorSSI.INVESTOR];
			if (null != investor)
			{
				routingInfo = null;
				routingInfo = GetRoutingIdsAndExplicitDetails(investor);
				//
				si.investorInformation  = new Routing(); 
				si.investorInformation.routingIdsAndExplicitDetailsSpecified = (null != routingInfo);
				if(null != routingInfo)
					si.investorInformation.routingIdsAndExplicitDetails = routingInfo;  
			}
			return si;
		}
		#endregion
		//
		#region public GetActorList
		public ArrayList  GetActorList()
		{
			ArrayList  list = new ArrayList();
			//	
			foreach ( string s in Enum.GetNames(typeof (Actor.RoleActorSSI)) )
			{
				RoleActorSSI role = (RoleActorSSI) Enum.Parse(typeof (RoleActorSSI), s );
			
				int  i = 0;
				while (i>-1)
				{
					IssiItem item= this[role,i];
					if (null == item)
						i=-1;
					else
					{
						i++;
						if (!list.Contains(item.idA))
							list.Add(item.idA); 
					}
				}
			}	
			return list;
		}
		#endregion
		//
		#region private GetRoutingIdsAndExplicitDetails
		private  RoutingIdsAndExplicitDetails GetRoutingIdsAndExplicitDetails(IssiItem  pIssiItem)
		{
			
			RoutingIdsAndExplicitDetails  routingInfo  = null ;
			try
			{
				// Add Info From Actor
				routingInfo = issiActorsInfo.GetRouting2(pIssiItem.idA);
				// Add Info From ISSIITEM
				routingInfo.routingAccountNumberSpecified = pIssiItem.cAccountNumberSpecified;  
				if(pIssiItem.cAccountNumberSpecified)
					routingInfo.routingAccountNumber = new EFS_String(pIssiItem.cAccountNumber); 
				if(pIssiItem.cAccountNameSpecified )
					routingInfo.routingReferenceText = new  EFS_StringArray[] {new EFS_StringArray(pIssiItem.cAccountName)};  

			}
			catch(Exception ex)
			{
				throw new OTCmlException( "GetRoutingIdsAndExplicitDetails", ex.Message,ex); 
			}
			return routingInfo;
		}
		#endregion GetRoutingIdsAndExplicitDetails
		//		
		#region public Sort
		public bool Sort()
		{
			bool isOk = false;
			try
			{
				isOk = ( null != issiItems);
				if (isOk) 
					Array.Sort(issiItems,this);  
			}
			catch(Exception ex)
			{
				throw new OTCmlException( "ISSICollection.Sort","Error",ex,null); 
			}
			return isOk;
		}
		#endregion
		
		#region Membres de IComparer
		public int Compare(object x, object y)
		{
			int 	ret = 0; 
			
			if ( (x is IssiItem) && (y is IssiItem)) 
			{
				IssiItem issiItemX = (IssiItem) x;		
				IssiItem issiItemY = (IssiItem) y;
				//CSS
				if ((issiItemX.idRoleActor == RoleActorSSI.CSS) && (issiItemY.idRoleActor != RoleActorSSI.CSS))
					ret =  -1;
				else if ((issiItemY.idRoleActor == RoleActorSSI.CSS) && (issiItemX.idRoleActor != RoleActorSSI.CSS))
					ret = 1;
				else if  ( (issiItemX.idRoleActor == RoleActorSSI.CSS) && (issiItemY.idRoleActor == RoleActorSSI.CSS))
					ret = (issiItemX.sequenceNumber -  issiItemY.sequenceNumber);
				//AGENT
				if (ret == 0)	
				{
					if ((issiItemX.idRoleActor == RoleActorSSI.AGENT) && (issiItemY.idRoleActor != RoleActorSSI.AGENT))
						ret =  -1;
					else if ((issiItemY.idRoleActor == RoleActorSSI.AGENT) && (issiItemX.idRoleActor != RoleActorSSI.AGENT))
						ret = 1;
					else if  ( (issiItemX.idRoleActor == RoleActorSSI.AGENT) && (issiItemY.idRoleActor == RoleActorSSI.AGENT))
						ret = (issiItemX.sequenceNumber -  issiItemY.sequenceNumber);
				}
				//INTERMEDIARY
				if (ret == 0)	
				{
				
					if ((issiItemX.idRoleActor == RoleActorSSI.INTERMEDIARY) && (issiItemY.idRoleActor != RoleActorSSI.INTERMEDIARY))
						ret =  -1;
					else if ((issiItemY.idRoleActor == RoleActorSSI.INTERMEDIARY) && (issiItemX.idRoleActor != RoleActorSSI.INTERMEDIARY))
						ret = 1;
					else if  ( (issiItemX.idRoleActor == RoleActorSSI.INTERMEDIARY) && (issiItemY.idRoleActor == RoleActorSSI.INTERMEDIARY))
						ret = (issiItemX.sequenceNumber -  issiItemY.sequenceNumber);
				}
				//ACCOUNTSERVICER
				if (ret == 0)	
				{
				
					if ((issiItemX.idRoleActor == RoleActorSSI.ACCOUNTSERVICER) && (issiItemY.idRoleActor != RoleActorSSI.ACCOUNTSERVICER))
						ret =  -1;
					else if ((issiItemY.idRoleActor == RoleActorSSI.ACCOUNTSERVICER) && (issiItemX.idRoleActor != RoleActorSSI.ACCOUNTSERVICER))
						ret = 1;
					else if  ( (issiItemX.idRoleActor == RoleActorSSI.ACCOUNTSERVICER) && (issiItemY.idRoleActor == RoleActorSSI.ACCOUNTSERVICER))
						ret = (issiItemX.sequenceNumber -  issiItemY.sequenceNumber);
				}
				//ACCOUNTOWNER
				if (ret == 0)	
				{
				
					if ((issiItemX.idRoleActor == RoleActorSSI.ACCOUNTOWNER) && (issiItemY.idRoleActor != RoleActorSSI.ACCOUNTOWNER))
						ret =  -1;
					else if ((issiItemY.idRoleActor == RoleActorSSI.ACCOUNTOWNER) && (issiItemX.idRoleActor != RoleActorSSI.ACCOUNTOWNER))
						ret = 1;
					else if  ( (issiItemX.idRoleActor == RoleActorSSI.ACCOUNTOWNER) && (issiItemY.idRoleActor == RoleActorSSI.ACCOUNTOWNER))
						ret = (issiItemX.sequenceNumber -  issiItemY.sequenceNumber);
				}
				//INVESTOR
				if (ret == 0)	
				{
				
					if ((issiItemX.idRoleActor == RoleActorSSI.INVESTOR) && (issiItemY.idRoleActor != RoleActorSSI.INVESTOR))
						ret =  -1;
					else if ((issiItemY.idRoleActor == RoleActorSSI.INVESTOR) && (issiItemX.idRoleActor != RoleActorSSI.INVESTOR))
						ret = 1;
					else if  ( (issiItemX.idRoleActor == RoleActorSSI.INVESTOR) && (issiItemY.idRoleActor == RoleActorSSI.INVESTOR))
						ret = (issiItemX.sequenceNumber -  issiItemY.sequenceNumber);
				}
			}
			return  ret;
		}
		#endregion Membres de IComparer
		//
		#region Membres de CompareTo
		public int CompareTo(object pobj)
		{
			if(pobj is IssiItemCollection) 
			{
				int ret = 0;
				IssiItemCollection issiItemCol2 = (IssiItemCollection) pobj; 
				
				if (issiItems.Length != issiItemCol2.issiItems.Length)
					ret = -1;
				//
				if (0 == ret)
				{
					Sort(); 
					issiItemCol2.Sort(); 
					for (int i = 0 ; i < issiItems.Length ; i++)
					{
						ret = issiItems[i].CompareTo(issiItemCol2.issiItems[i]);
						if (0 != ret )
							break;
					}
				}
				//
				return ret;
			}
			throw new ArgumentException("object is not a IssiItemCollection");    
		}
		#endregion CompareTo
	}
	#endregion class IssiItemCollection
	
	#region class IssiItem
	public class IssiItem : IComparable	 
	{
		[System.Xml.Serialization.XmlElementAttribute("IDISSI")]	
		public int idIssi;   
		//		
		[System.Xml.Serialization.XmlElementAttribute("IDA")]	
		public int idA; 
		//
		[System.Xml.Serialization.XmlElementAttribute("IDROLEACTOR")]	
		public Actor.RoleActorSSI idRoleActor; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sequenceNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SEQUENCENO")]	
		public int  sequenceNumber; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool chainPartySpecified;
		[System.Xml.Serialization.XmlElementAttribute("CHAINPARTY")]	
		public string chainParty; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool idPartyRoleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("PARTYROLE")]	
		public int  idPartyRole; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool cAccountNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("CACCOUNTNUMBER")]	
		public string cAccountNumber; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool cAccountNumberIdentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("CACCOUNTNUMBERIDENT")]	
		public string cAccountNumberIdent; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool cAccountNameSpecified;
		[System.Xml.Serialization.XmlElementAttribute("CACCOUNTNAME")]	
		public string cAccountName; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sAccountNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SACCOUNTNUMBER")]	
		public string sAccountNumber; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sAccountNumberIdentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SACCOUNTNUMBERIDENT")]	
		public string sAccountNumberIdent; 
		//
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool sAccountNameSpecified;
		[System.Xml.Serialization.XmlElementAttribute("SACCOUNTNAME")]	
		public string sAccountName; 
		
		#region Membres de CompareTo
		public int CompareTo(object pobj)
		{
			if(pobj is IssiItem) 
			{
				IssiItem IssiItem2 = ( IssiItem) pobj;
				int ret = 0 ; 
				//idA
				if (IssiItem2.idA != idA)
					ret = -1; 
				//idRoleActor
				if (ret == 0)
				{
					if (IssiItem2.idRoleActor != idRoleActor)
						ret = -1; 
				}
				//sequenceNumber
				if (ret == 0)
				{
					if (IssiItem2.sequenceNumber  != sequenceNumber)
						ret = -1; 
				}
				//chainParty
				if  (ret == 0) 
				{
					if (IssiItem2.chainPartySpecified && chainPartySpecified && (IssiItem2.chainParty != chainParty))
						ret = -1; 
					else if ( (false == IssiItem2.chainPartySpecified) && (chainPartySpecified)) 
						ret = -1; 
					else if ( (IssiItem2.chainPartySpecified) && (false == chainPartySpecified)) 
						ret = -1; 
				}
				//idPartyRole		
				if  (ret == 0) 
				{
					if (IssiItem2.idPartyRoleSpecified && idPartyRoleSpecified && (IssiItem2.idPartyRole != idPartyRole))
						ret = -1; 
					else if ( (false == IssiItem2.idPartyRoleSpecified) && (idPartyRoleSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.idPartyRoleSpecified) && (false == idPartyRoleSpecified)) 
						ret = -1; 
				}
				//cAccountNumber
				if  (ret == 0) 
				{
					if (IssiItem2.cAccountNumberSpecified && cAccountNumberSpecified && (IssiItem2.cAccountNumber != cAccountNumber))
						ret = -1; 
					else if ( (false == IssiItem2.cAccountNumberSpecified) && (cAccountNumberSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.cAccountNumberSpecified) && (false == cAccountNumberSpecified)) 
						ret = -1; 
				}
				//cAccountNumberIdent
				if  (ret == 0) 
				{
					if (IssiItem2.cAccountNumberIdentSpecified && cAccountNumberIdentSpecified && (IssiItem2.cAccountNumberIdent != cAccountNumberIdent))
						ret = -1; 
					else if ( (false == IssiItem2.cAccountNumberIdentSpecified) && (cAccountNumberIdentSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.cAccountNumberIdentSpecified) && (false == cAccountNumberIdentSpecified)) 
						ret = -1; 
				}
				//cAccountName
				if  (ret == 0) 
				{
					if (IssiItem2.cAccountNameSpecified &&cAccountNameSpecified && (IssiItem2.cAccountName != cAccountName))
						ret = -1; 
					else if ( (false == IssiItem2.cAccountNameSpecified) && (cAccountNameSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.cAccountNameSpecified) && (false == cAccountNameSpecified)) 
						ret = -1; 
				}


				//sAccountNumber
				if  (ret == 0) 
				{
					if (IssiItem2.sAccountNumberSpecified && sAccountNumberSpecified && (IssiItem2.sAccountNumber != sAccountNumber))
						ret = -1; 
					else if ( (false == IssiItem2.sAccountNumberSpecified) && (sAccountNumberSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.sAccountNumberSpecified) && (false == sAccountNumberSpecified)) 
						ret = -1; 
				}
				//sAccountNumberIdent
				if  (ret == 0) 
				{
					if (IssiItem2.sAccountNumberIdentSpecified && sAccountNumberIdentSpecified && (IssiItem2.sAccountNumberIdent != sAccountNumberIdent))
						ret = -1; 
					else if ( (false == IssiItem2.sAccountNumberIdentSpecified) && (sAccountNumberIdentSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.sAccountNumberIdentSpecified) && (false == sAccountNumberIdentSpecified)) 
						ret = -1; 
				}
				//cAccountName
				if  (ret == 0) 
				{
					if (IssiItem2.sAccountNameSpecified && sAccountNameSpecified && (IssiItem2.sAccountName != sAccountName))
						ret = -1; 
					else if ( (false == IssiItem2.sAccountNameSpecified) && (sAccountNameSpecified)) 
						ret = -1; 
					else if ( (IssiItem2.sAccountNameSpecified) && (false == sAccountNameSpecified)) 
						ret = -1; 
				}
				//
				return ret;
			}
			throw new ArgumentException("object is not a IssiItem");    
		}
		#endregion CompareTo

	}
	#endregion class IssiItem  
	
	#region class SearchEventSi
	/// <summary>
	/// Description résumée de SearchEventSi.
	/// </summary>
	public class SearchEventSi
	{
		#region Members
		private EFS_TradeLibrary _tradeLibrary;
		private SQL_Event        _sqlEvent;
		private SettlementChain  _settlementChain;
		#endregion Members

		#region Accessor
		public SettlementChain SettlementChain
		{
			get{ return _settlementChain;}	
		}
		#endregion Accessor
		
		#region constructor
		public SearchEventSi(int pIdE, EFS_TradeLibrary pTradeLibrary)
		{
			_tradeLibrary = pTradeLibrary;
			_sqlEvent     = new SQL_Event(pTradeLibrary.ConnectionString,pIdE);  
			_sqlEvent.LoadTableAllColums(); 	
			//LoadSettlementChain();	
		}
		#endregion constructor
	
		#region LoadSettlementChain
		public void LoadSettlementChain ()
		{
			_settlementChain = null;
			try
			{
				bool isOk = (null != _tradeLibrary) && _sqlEvent.IsLoaded; 
				if(isOk)
				{
					_settlementChain  = new SettlementChain();
					string cs = _tradeLibrary.ConnectionString; 
					//
					#region Exist Receiver Osi
					SettlementInformationInput  osiRec = GetOsi(PayerReceiverEnum.Receiver,_sqlEvent.IdAReceiver); 
					if (null != osiRec)
					{
						if (osiRec.settlementInformation.informationStandardSpecified)
							_settlementChain[PayerReceiverEnum.Receiver].siMode = SiModeEnum.Oessi;
						else if (osiRec.settlementInformation.informationInstructionSpecified)
							_settlementChain[PayerReceiverEnum.Receiver].siMode = SiModeEnum.Osi;
					}
					#endregion
					//
					#region Exist Receiver Fsi
					SettlementInformation fsi = GetFsi(); 
					if (null != fsi)
					{
						//=> fsi Settlement
						if (fsi.informationStandardSpecified)
							_settlementChain[PayerReceiverEnum.Receiver].siMode = SiModeEnum.Fessi;
						else if (fsi.informationInstructionSpecified)
							_settlementChain[PayerReceiverEnum.Receiver].siMode = SiModeEnum.Fsi;
					}
					#endregion
					// Il n'existe pas de OSI instruction ni de FSI instruction => Search Default Standind Settlement					
					if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Receiver].siMode)
						_settlementChain[PayerReceiverEnum.Receiver].siMode = SiModeEnum.Dessi;    
					//
					if (_settlementChain[PayerReceiverEnum.Receiver].siMode == SiModeEnum.Osi)
					{
						//Set Receiver OSI Settlement Instruction
						_settlementChain[PayerReceiverEnum.Receiver].settlementInstructions = osiRec.settlementInformation.informationInstruction;
					}
					else if (_settlementChain[PayerReceiverEnum.Receiver].siMode == SiModeEnum.Fsi)
					{
						//Set Payer/Receiver FSI Settlement Instruction
						_settlementChain[PayerReceiverEnum.Payer].siMode = EFS.Settlement.SiModeEnum.Fsi;
						_settlementChain.SetSettlementInstruction(fsi.informationInstruction,_sqlEvent,_tradeLibrary);  
					}
					else if(SettlementTools.IsStanding(_settlementChain[PayerReceiverEnum.Receiver].siMode))	
					{
						EfsSettlementInstruction[] siReceiver = null;
						CssCriteria cssCriteria = null;
						SsiCriteria ssiCriteria = null;
						if (null != osiRec)
						{
							if (osiRec.cssCriteriaSpecified)
								cssCriteria = osiRec.cssCriteria;
							if (osiRec.ssiCriteriaSpecified)
								ssiCriteria = osiRec.ssiCriteria;
						}
						LoadStandingInstructionCandidate(PayerReceiverEnum.Receiver,cssCriteria,ssiCriteria, ref siReceiver,ref _settlementChain[PayerReceiverEnum.Receiver].siMode);
					}	
					//
					// Payer
					if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Receiver].siMode) 
					{
						#region Exist Payer Osi
						SettlementInformationInput  osiPay = GetOsi(PayerReceiverEnum.Payer,_sqlEvent.IdAPayer); 
						if (null != osiPay)
						{
							if (osiPay.settlementInformation.informationStandardSpecified)
								_settlementChain[PayerReceiverEnum.Payer].siMode = SiModeEnum.Oessi;
							else if (osiPay.settlementInformation.informationInstructionSpecified)
								_settlementChain[PayerReceiverEnum.Payer].siMode = SiModeEnum.Osi;
						}
						#endregion
						// Il n'existe pas de OSI instruction => Search Default Standind Settlement					
						if (SiModeEnum.Undefined == _settlementChain[PayerReceiverEnum.Payer].siMode)
							_settlementChain[PayerReceiverEnum.Payer].siMode = SiModeEnum.Dessi;    
					
						if (_settlementChain[PayerReceiverEnum.Payer].siMode == SiModeEnum.Osi)
						{
							//Set Payer OSI Settlement Instruction
							_settlementChain[PayerReceiverEnum.Payer].settlementInstructions = osiPay.settlementInformation.informationInstruction;
						}
						else if(SettlementTools.IsStanding(_settlementChain[PayerReceiverEnum.Payer].siMode))	
						{
							EfsSettlementInstruction[] siPayer = null;
							CssCriteria siPayerCssCriteria = null;
							SsiCriteria siPayerSsiCriteria = null;
							if (null != osiRec)
							{
								if (osiPay.cssCriteriaSpecified)
									siPayerCssCriteria = osiPay.cssCriteria;
								if (osiPay.ssiCriteriaSpecified)
									siPayerSsiCriteria = osiPay.ssiCriteria;
							}
							LoadStandingInstructionCandidate(PayerReceiverEnum.Receiver,siPayerCssCriteria,siPayerSsiCriteria, ref siPayer ,ref _settlementChain[PayerReceiverEnum.Payer].siMode);
						}	
					}
				}
			
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
		}
		#endregion GetSettlementChain
		
		#region LoadStandingInstructionCandidate
		public void LoadStandingInstructionCandidate(PayerReceiverEnum pPayRec, CssCriteria pCssCriteria, SsiCriteria pSsiCriteria, ref  EfsSettlementInstruction[] pSsis, ref  SiModeEnum pSiMode)
		{
			//Search potentially differents ssi (internal Or external) for settlementChain[pPayRec]
			string cs = _tradeLibrary.ConnectionString; 
			EfsSettlementInstruction[] ssis = null;
			SiModeEnum siMode = SiModeEnum.Dessi;  // Default
			//
			SettlementOfficeCollection offices = new SettlementOfficeCollection(cs,_sqlEvent.GetIdPayRec(pPayRec),_sqlEvent.GetIdBook(pPayRec)); 
			for (int i = 0 ; i < offices.Count ; i++)
			{
				//Load ssiDatabase for each offices
				SsiDbCollection ssidbs = new SsiDbCollection();
				ssidbs.InitializeActorSsiDb(cs,offices[i]); 
				//
				if (ssidbs.Count > 0)
				{
					ssidbs.Sort(); 
					for (int j = 0 ; i < ssidbs.Count ; j++)
					{
						if (ssidbs[j].IsLocalDatabase)
						{
							ssis = GetIssis(offices[i],PayerReceiverEnum.Receiver, pCssCriteria, pSsiCriteria);	
							if (null != ssis)
								siMode = SiModeEnum.Dissi;
						}
						else
							ssis = GetESsi();
						//
						if (null != ssis)
							break;
					}
				}
				else
				{
					ssis = GetIssis(offices[i],PayerReceiverEnum.Receiver,pCssCriteria, pSsiCriteria);	
					if (null != ssis)
						siMode = SiModeEnum.Dissi;
				}
				//
				if (null != ssis)
					break;
			}
			//
			pSsis   = ssis; 
			pSiMode = siMode;  
			//
		}
		#endregion LoadStandingInstructionCandidate

		#region GetOsi
		/// <summary>
		///  a method to get the relevant OTCml settlement instruction for a given Event And Side
		/// </summary>
		/// <param name="pPayerReceiver"></param>
		/// <returns></returns>
		private SettlementInformationInput  GetOsi (PayerReceiverEnum pPayerReceiver, int pIdA)
		{
			SettlementInformationInput       ret  = null;
			try
			{
				SettlementInformationInput   osi =null;
				SettlementInformationInput[] settlementInformations =  _tradeLibrary.FpMLTrade.settlementInformations;   
				//
				if (null != settlementInformations)
				{
					#region eventCodesSchedule Match
					for (int i=0; i< settlementInformations.Length; i++)
					{
						osi = settlementInformations[i];
						//
						if (osi.IsMatchWith(pIdA,pPayerReceiver,_tradeLibrary.Party))
						{
							int NbMatch= 0;
							if (osi.eventCodesScheduleSpecified)
							{
								int NbMatchCurrent =  osi.eventCodesSchedule.eventCodes[i].NumberOfMatching(_sqlEvent);    
								if (NbMatchCurrent > NbMatch)
									ret  = osi;
								if ((NbMatchCurrent  ==  NbMatch) && (null != ret))
								{
									if  (osi.eventCodesSchedule.eventCodes[i].CompareTo(ret)>0)
										ret = osi;
								}
							}
						}
					}
					#endregion
					
					#region Payer/receiver Match
					if (null == ret )
					{
						for (int i=0; i< settlementInformations.Length; i++)
						{
							osi = settlementInformations[i];
							if (osi.IsMatchWith(pIdA,pPayerReceiver,_tradeLibrary.Party))
								ret = osi;
						}
					}
					#endregion
				}
			}
			catch (OTCmlException otcmlException){throw otcmlException;}
			catch (Exception ex){throw ex;}
			//
			return ret ;
		}

		#endregion GetOsi
		
		#region GetFsi
		/// <summary>
		///  a method to get the relevant OTCml settlement instruction for a given Event And Side
		/// </summary>
		/// <param name="pPayerReceiver"></param>
		/// <returns></returns>
		private SettlementInformation  GetFsi ()
		{
			SettlementInformation ret = null;
			int item = 0; 
			Product product  = (Product)Tools.GetObjectById(_tradeLibrary.FpMLTrade,Cst.FpML_InstrumentNo + _sqlEvent.InstrumentNo.ToString());
			System.Type t    = product.GetType() ;
			//
			switch (t.FullName)
			{				
				case "FpML.Ird.BulletPayment":
					BulletPayment bulletPayment = (FpML.Ird.BulletPayment) product;
					if (EventCodeFunc.IsStart(_sqlEvent.EventCode) && (bulletPayment.payment.PaymentType == _sqlEvent.EventType)) 
						ret = bulletPayment.payment.settlementInformation;
					break;
				case "FpML.Ird.CapFloor":
					CapFloor capFloor = (CapFloor) product;
					if ( EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode ) || (EventCodeFunc.IsPremium(_sqlEvent.EventCode)) )
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
						{
							if (EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode ) && capFloor.additionalPaymentSpecified)
								ret = GetSettlementInformationPayment(capFloor.additionalPayment,item);    
							else if (EventCodeFunc.IsPremium(_sqlEvent.EventCode) && capFloor.premiumSpecified)
								ret = GetSettlementInformationPayment(capFloor.premium,item);  
						}
					}
					break;
				case "FpML.Cd.CreditDefaultSwap":
				case "FpML.Eqd.EquityDerivativeBase":
				case "FpML.Eqs.EquitySwapBase":
				case "FpML.Ird.Fra" :
					break; // NoSettlement Instruction
				case "FpML.Fx.FxAverageRateOption":
					FxAverageRateOption fxAverageRateOption = (FpML.Fx.FxAverageRateOption) product;
					if (EventCodeFunc.IsPremium(_sqlEvent.EventCode) &&  EventTypeFunc.IsPremium(_sqlEvent.EventType) && fxAverageRateOption.fxOptionPremiumSpecified)
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = fxAverageRateOption.fxOptionPremium[item].settlementInformation;  
					}
					break;
				case "FpML.Fx.FxBarrierOption":
					FxBarrierOption fxBarrierOption = (FpML.Fx.FxBarrierOption) product;
					if (EventCodeFunc.IsTermination(_sqlEvent.EventCode) && EventTypeFunc.IsPayout(_sqlEvent.EventType) && fxBarrierOption.triggerPayout.settlementInformationSpecified)
						ret = fxBarrierOption.triggerPayout.settlementInformation;    
					else if (EventCodeFunc.IsPremium(_sqlEvent.EventCode) &&  EventTypeFunc.IsPremium(_sqlEvent.EventType) && fxBarrierOption.fxOptionPremiumSpecified)
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = fxBarrierOption.fxOptionPremium[item].settlementInformation ;  
					}
					break;
				case "FpML.Fx.FxDigitalOption":
					FxDigitalOption fxDigitalOption = (FpML.Fx.FxDigitalOption) product;
					
					if (EventCodeFunc.IsTermination(_sqlEvent.EventCode) && EventTypeFunc.IsPayout(_sqlEvent.EventType))
						ret = fxDigitalOption.triggerPayout.settlementInformation ;  
					else  if (EventCodeFunc.IsPremium(_sqlEvent.EventCode) &&  EventTypeFunc.IsPremium(_sqlEvent.EventType) && fxDigitalOption.fxOptionPremiumSpecified && (item>=0))
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = fxDigitalOption.fxOptionPremium[item].settlementInformation ;  
					}
					break;
				case "FpML.Fx.FxLeg":
					FpML.Fx.FxLeg fxLeg = (FpML.Fx.FxLeg) product;
					if (EventCodeFunc.IsTermination(_sqlEvent.EventCode)) 
					{
						if  (EventTypeFunc.IsCurrency1(_sqlEvent.EventType))
							ret = fxLeg.exchangedCurrency1.settlementInformation;   
						else if  (EventTypeFunc.IsCurrency2(_sqlEvent.EventType))
							ret = fxLeg.exchangedCurrency2.settlementInformation;   
					}
					break;
				case "FpML.Fx.FxOptionLeg":
					FpML.Fx.FxOptionLeg fxOptionLeg = (FpML.Fx.FxOptionLeg) product;
					if (EventCodeFunc.IsPremium(_sqlEvent.EventCode)&& EventTypeFunc.IsPremium(_sqlEvent.EventType) && fxOptionLeg.fxOptionPremiumSpecified)
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = fxOptionLeg.fxOptionPremium[item].settlementInformation ;  
					}
					break;
				case "FpML.Fx.FxSwap":
					FpML.Fx.FxSwap fxSwap = (FpML.Fx.FxSwap) product;
					if (EventCodeFunc.IsTermination(_sqlEvent.EventCode)) 
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
						{
							if  (EventTypeFunc.IsCurrency1(_sqlEvent.EventType))
								ret = fxSwap.fxSingleLeg[item].exchangedCurrency1.settlementInformation;   
							else if  (EventTypeFunc.IsCurrency2(_sqlEvent.EventType))
								ret = fxSwap.fxSingleLeg[item].exchangedCurrency2.settlementInformation;   
						}
					}
					break;
				case "FpML.Doc.Strategy":
					break;
				case "FpML.Ird.Swap":
					FpML.Ird.Swap swap = (FpML.Ird.Swap) product;
					if (EventCodeFunc.IsAdditionalPayment(_sqlEvent.EventCode) && swap.additionalPaymentSpecified)
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = GetSettlementInformationPayment(swap.additionalPayment,item) ;  
					}
					break;
				case "FpML.Ird.Swaption":
					FpML.Ird.Swaption swaption = (FpML.Ird.Swaption) product;
					if (EventCodeFunc.IsPremium(_sqlEvent.EventCode) && swaption.premiumSpecified)
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = GetSettlementInformationPayment(swaption.premium,item) ;  
					}
					break;
				case "FpML.Fx.TermDeposit":
					TermDeposit termDeposit = (FpML.Fx.TermDeposit) product;
					if (EventCodeFunc.IsTermination(_sqlEvent.EventCode) && termDeposit.paymentSpecified)
					{
						item = GetArrayObjectItem(); 
						if (item>=0)
							ret = GetSettlementInformationPayment(termDeposit.payment,item) ;  
					}
					break;
			}
			// cas OtherPartyPayment			
			if( (null==ret) && EventCodeFunc.IsOtherPartyPayment(_sqlEvent.EventCode) && _tradeLibrary.FpMLTrade.otherPartyPaymentSpecified )
			{
				item = GetArrayObjectItem(); 
				ret = GetSettlementInformationPayment(_tradeLibrary.FpMLTrade.otherPartyPayment,item) ;  
			}
			//
			return ret;
		}
		#endregion GetFsi
		
		#region GetISsi
		private EfsSettlementInstruction[] GetIssis(int pIdAStlOffice ,PayerReceiverEnum pPayerReceiver, SettlementInformationInput pOsi)
		{
			CssCriteria cssCriteria = null;
			SsiCriteria ssiCriteria = null;
			//
			if (null != pOsi)
			{
				if (pOsi.cssCriteriaSpecified) 
					cssCriteria = pOsi.cssCriteria; 
				if (pOsi.ssiCriteriaSpecified) 
					ssiCriteria = pOsi.ssiCriteria; 
			}
			//
			return GetIssis(pIdAStlOffice, pPayerReceiver, cssCriteria, ssiCriteria);
		}		
		private EfsSettlementInstruction[] GetIssis(int pIdAStlOffice , PayerReceiverEnum pPayerReceiver, CssCriteria pCssCriteria, SsiCriteria pSsiCriteria)
		{
			EfsSettlementInstruction[] ret = null;
			//
			IssiCollection issiCol= new IssiCollection();
			issiCol.Initialize(_sqlEvent,_tradeLibrary, pIdAStlOffice , pPayerReceiver, pSsiCriteria);
			issiCol.Sort();
			//
			ArrayList siList = new ArrayList();
			for (int i = 0; i< issiCol.Count ; i++)
			{
				int idIssi = issiCol[i].idIssi; 
				//
				IssiItemCollection issiItemCol = new IssiItemCollection();
				issiItemCol.Initialize(_tradeLibrary.ConnectionString,idIssi);  
				EfsSettlementInstruction si = issiItemCol.GetInstruction(pPayerReceiver); 
				if (null != si)
					siList.Add(si); 
			}
			//
			ret = (EfsSettlementInstruction[]) siList.ToArray(typeof(EfsSettlementInstruction)); 
			//
			return ret;
		}
		#endregion
		
		#region GetESsi
		private EfsSettlementInstruction[]  GetESsi()
		{
			// TODO After
			return null;
		}
		#endregion GetESsi

		#region GetArrayObjectItem
		private int  GetArrayObjectItem()
		{
			int ret  = -1;
			//
			//Recherche du n°/item à partir des Evènement de même Type 
			//Postulats: Les Evts sont ds le même ordre que les objects Fpml
			//La query est faite directement, on ne s'appuie pas ici sur le dataset puisqu'il ne contient pas forcement tous les evts 
			//(Par ex en cas de restriction sur les Evts)
			string cs  = _sqlEvent.ConnectionString; 
			//
			IDbDataParameter paramIdT;
			IDbDataParameter paramEventCode;
			IDbDataParameter paramEventType;	
			IDbDataParameter paramInstrumentNo;	
			IDbDataParameter paramStreamNo;	
			//
			paramIdT			= new EFSParameter(cs, "IDT", DbType.Int32).DataParameter;
			paramEventCode	    = new EFSParameter(cs, "EVENTCODE", DbType.AnsiString, 3).DataParameter;
			paramEventType	    = new EFSParameter(cs, "EVENTTYPE", DbType.AnsiString, 3).DataParameter;
			paramInstrumentNo	= new EFSParameter(cs, "INSTRUMENTNO", DbType.Int32).DataParameter;
			paramStreamNo		= new EFSParameter(cs, "STREAMNO", DbType.Int32).DataParameter;
			//
			paramIdT.Value           = _sqlEvent.IdT;
			paramEventCode.Value     = _sqlEvent.EventCode;
			paramEventType.Value     = _sqlEvent.EventType;
			paramInstrumentNo.Value  = _sqlEvent.InstrumentNo;
			paramStreamNo.Value      = _sqlEvent.StreamNo;
			//
			string SQLSelectEvent = SQLCst.SELECT + @"e.IDE as ID" + Cst.CrLf;
			SQLSelectEvent	+= SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e " + Cst.CrLf;
			//
			string sqlWhere  = SQLCst.WHERE + @"(e.IDT=@IDT)" + Cst.CrLf;
			sqlWhere   += SQLCst.AND + @"(e.EVENTCODE=@EVENTCODE)" + Cst.CrLf;
			sqlWhere   += SQLCst.AND + @"(e.EVENTTYPE=@EVENTTYPE)" + Cst.CrLf;
			sqlWhere   += SQLCst.AND + @"(e.INSTRUMENTNO=@INSTRUMENTNO)" + Cst.CrLf;
			sqlWhere   += SQLCst.AND + @"(e.STREAMNO=@STREAMNO)" + Cst.CrLf;
			//
			string SQLSelect = string.Empty;
			SQLSelect  = SQLSelectEvent + sqlWhere + Cst.CrLf;
			SQLSelect += SQLCst.ORDERBY + "e.IDE" + Cst.CrLf;
			//
			IDataReader    dataReader =null;
			int i = -1;
			try
			{
				SQLSelect      = DataHelper.ReplaceVarPrefix(cs, SQLSelect);
				dataReader     = DataHelper.ExecuteReader(cs,CommandType.Text,SQLSelect,paramIdT,
					paramEventCode,paramEventType,paramInstrumentNo,paramStreamNo);
				//
				while (dataReader.Read())
				{
					i++;
					if (Convert.ToInt32(dataReader.GetValue(0)) == _sqlEvent.Id)
					{
						ret =i;
						break;
					}
				}
			}
			catch{}
			finally{dataReader.Close();} 
			//
			return ret ;
		}
		#endregion GetArrayObjectItem
		
		#region GetSettlementInformationPayment
		private SettlementInformation  GetSettlementInformationPayment(Payment[] payments,int pItem)
		{
			//
			SettlementInformation ret = null;
			int j= -1; 
			Payment payment = null;
			//
			for (int i=0; i< payments.Length ; i++)
			{
				if (payments[i].PaymentType == _sqlEvent.EventType) 
					j++;
				if (j == pItem)
				{
					payment = payments[i];
					break;
				}
			}
			if ((null!=payment) && (payment.settlementInformationSpecified))
				ret = payment.settlementInformation ;  
			//
			return ret ;
		}
		#endregion GetSettlementInformationPayment
	
		#region SettlementInstruction
		private SettlementInstruction GetSsi(PayerReceiverEnum pPayerReceiver)
		{
			SettlementInstruction ret = null;
			Party party = null;
			PartyTradeIdentifier partyTradeIdentifier =null; 
			//
			int IdActor  = 0;
			if (pPayerReceiver== PayerReceiverEnum.Payer)
				IdActor = _sqlEvent.IdAPayer;
			else
				IdActor = _sqlEvent.IdAReceiver;
			//
			party = Tools.GetParty(_tradeLibrary.Party, IdActor.ToString() ,Tools.PartyInfo.OTCmlId);         			
			if (null!=party)
				partyTradeIdentifier = Tools.GetPartyTradeIdentifier(_tradeLibrary.TradeHeader.partyTradeIdentifier, party.id);
			//	
			SQL_Book sqlBook= null; 
			if ((null != partyTradeIdentifier) && partyTradeIdentifier.bookIdSpecified)
				sqlBook = new SQL_Book(_tradeLibrary.ConnectionString, SQL_TableBase.IDType.Id,partyTradeIdentifier.bookId.otcmlId,SQL_Table.ScanDataDtEnabledEnum.No);     
			//
			int idACss = 0;
			if (null != sqlBook)
			{
				idACss = sqlBook.IdA_SettlementOffice  ; 
				if (idACss == 0)
					idACss = sqlBook.IdA; 
			}
			//
			if (idACss == 0)
				idACss = IdActor;
			//
			ActorAncestor actorAnc = new ActorAncestor( _tradeLibrary.ConnectionString, idACss,null);    
			//
			

				

		    






			return ret;
		}

		#endregion SettlementInstruction
	}
	#endregion SearchEventSi
	
	#region SettlementOfficeCollection
	public class SettlementOfficeCollection : ICollection	 
	{
		#region Members
		private int[] settlementOffices;
		#endregion Members
		
		#region indexor
		public int this[int pIndex]
		{
			get 
			{	
				return settlementOffices[pIndex];
			}
		}
		#endregion indexor

		#region constructor
		public SettlementOfficeCollection(string pSource, int pIdA, int pIdB)
		{
			bool isOk = false;
			//
			try
			{
				if (pIdB>0)
				{
					SQL_Book sqlBook = new SQL_Book(pSource,pIdB,SQL_Table.ScanDataDtEnabledEnum.Yes);  
					isOk = (sqlBook.LoadTable(new string[]{"IDB","IDA_SETTLEMENTOFFICE"}) && (sqlBook.IdA_SettlementOffice>0)); 
					if (isOk)
						settlementOffices = new int[]{sqlBook.IdA_SettlementOffice};
				}
				//
				isOk = ActorTools.IsActorWithRole(pSource,pIdA,RoleActor.SETTLTOFFICE,0);
				if (isOk)
					settlementOffices = new int[]{pIdA};
				//
				if (!isOk)
				{
					SearchAncestorRole search =  new SearchAncestorRole(pSource,pIdA,RoleActor.SETTLTOFFICE);    
					settlementOffices = search.GetActors(); 
				}
			}
			catch(Exception ex)
			{
				throw new OTCmlException("SettlementOfficeCollection",ex); 
			}
		}
		#endregion constructor

		#region Membres de ICollection
		public int Count
		{
			get
			{
				int ret = 0;
				if  (ArrFunc.IsFilled(settlementOffices))
					ret = settlementOffices.Length; 
				return ret;
			}
		}
		public void CopyTo(Array array, int index)
		{
			settlementOffices.CopyTo(array, index);
		}
		public IEnumerator GetEnumerator()
		{
			return settlementOffices.GetEnumerator();
		}
		public bool IsSynchronized
		{
			get {return settlementOffices.IsSynchronized;}
		}
		public object SyncRoot
		{
			get {return settlementOffices.SyncRoot;}
		}
		#endregion Membres

	}
	#endregion SettlementPartyOffice

}

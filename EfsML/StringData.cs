using System;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.IO; 
using System.Globalization;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.Tuning;
using EFS.OTCmlStatus; 
using EFS.EFSTools;


namespace EfsML.DynamicData
{
	
	#region public class StringDynamicDatas 
	[XmlRoot(ElementName = "Datas", IsNullable=true)]
	public class StringDynamicDatas  
	{     
		[XmlElementAttribute("Data", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public StringDynamicData[] data;
	
		public static StringDynamicDatas LoadFromText(string pText)
		{
			try
			{
				return (StringDynamicDatas)CacheSerializer.Deserialize(typeof(StringDynamicDatas),pText);
			}
			catch( OTCmlException ex) {throw ex;} 
			catch( Exception ex) {throw new OTCmlException("StringDynamicDatas.LoadFromText", ex);} 	
		}
	
		#region Indexors
		public StringDynamicData this[string pName]
		{
			get
			{
				try
				{
					StringDynamicData ret = null;
					foreach (StringDynamicData sData in data)
					{
						if (sData.name == pName)
						{
							ret =  sData;
							break; 
						}
					}
					return ret;
				}
				catch( OTCmlException ex) {throw ex;} 
				catch( Exception ex) {throw new OTCmlException("StringDynamicDatas.this", ex);} 	
			}
		}
		#endregion



	}
	#endregion

	#region public class StringDynamicData 
	[XmlRoot(ElementName = "Data", IsNullable=true)]
	public class StringDynamicData  
	{     
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
		public string Value;
		//
		[System.Xml.Serialization.XmlElementAttribute()]
		public System.Xml.XmlCDataSection ValueXML;
		//
		[System.Xml.Serialization.XmlAttribute()]
		public string name;
		//
		[System.Xml.Serialization.XmlAttribute()]
		public string datatype;		
		//
		[System.Xml.Serialization.XmlAttribute()]
		public string dataformat;
		//
		[XmlElementAttribute("SQL", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public DataSQL sql;      
		//
		[XmlElementAttribute("SpheresLib", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public DataSpheresLib spheresLib;   
		#endregion Members
		//
		#region Accessors
		public bool ContainsExpression
		{
			get{ return ((null!= Value) || (null!=spheresLib) || (null!=sql));  }
		}
		#endregion 
		
		//
		#region public GetDataValue
		public string GetDataValue(string pCs, IDbTransaction pdbTransaction)
		{
			try
			{
				string retValue     = null;
				//
				if (sql != null)
				{
					retValue = sql.Exec(pCs,pdbTransaction); 
					if (StrFunc.IsFilled(dataformat))
					{
						if ( Cst.TypeData.IsTypeDateOrDateTime(datatype))
							retValue = DtFunc.DateTimeToString( new DtFunc().StringDateTimeISOToDateTime(retValue), dataformat);        
					}
				}
				else if (spheresLib != null)
				{
					string          functionName  = spheresLib.function.ToUpper(); 
					//
					#region SpheresLib 
					switch (functionName)
					{
							//
							#region Spheres Functions for Date 
						case "GETDATESYS()":
						case "GETDATETIMESYS()":
						case "GETDATERDBMS()":
						case "GETDATETIMERDBMS()":
							spheresLib = new DataSpheresLib();
							spheresLib.function = functionName;
							spheresLib.param  = new ParamData[]{new ParamData("FORMAT",dataformat)};
							retValue = spheresLib.Exec(pCs,pdbTransaction); 
							break;
							//
						case "ISDATE()":
						case "ISNOTDATE()":
						case "ISDATETIME()":                                    
						case "ISNOTDATETIME()":          
						case "ISDATESYS()":
						case "ISNOTDATESYS()":
							spheresLib = new DataSpheresLib();
							spheresLib.function = functionName; 
							spheresLib.param  = new ParamData[]{new ParamData("FORMAT",dataformat),new ParamData("VALUE",Value)};
							retValue = spheresLib.Exec(pCs,pdbTransaction); 
							break;
							#endregion
							//
						case "ISNULL()":
						case "ISNOTNULL()":
						case "ISEMPTY()":
						case "ISNOTEMPTY()":
							spheresLib = new DataSpheresLib();
							spheresLib.function = functionName; 
							spheresLib.param  = new ParamData[]{new ParamData("VALUE",Value)};
							retValue = spheresLib.Exec(pCs,pdbTransaction); 
							break;

						default:
							retValue = spheresLib.Exec(pCs,pdbTransaction); 
							break;
							//
					}
					retValue =  (StrFunc.IsFilled(retValue)? retValue.Trim():retValue);
					#endregion
				}
				else
				{
					#region Default : Data 
					retValue = (StrFunc.IsFilled(Value)? Value.Trim():Value);
					retValue = (retValue=="null"? null:retValue);
					#endregion
				}
				//	
				return retValue;
			}
			catch( OTCmlException ex) {throw ex;} 
			catch( Exception  ex) {throw new OTCmlException("StringDynamicData.GetDataValue", ex);} 	
		}
		#endregion public GetDataValue
		//
		#region public GetDataParameter
		public  DataParameter GetDataParameter (string pCs, IDbTransaction pTransaction,  CommandType pCommandType,  int pSize , ParameterDirection pDataDirection)
		{
			try
			{
				DataParameter  dataParam   = null;
				string dataName = name.ToUpper(); 
				string Value    = GetDataValue(pCs,pTransaction); 
				//
				if ( Cst.TypeData.IsTypeBool(datatype))
				{
					dataParam       = new DataParameter(pCs, pCommandType, dataName, DbType.Boolean);
					dataParam.Value = (ObjFunc.IsNull(Value ) ? Convert.DBNull: BoolFunc.IsTrue(Value));
				}
				//
				else if ( Cst.TypeData.IsTypeCursor(datatype))
				{
					//Warning: N'existe que pour Oracle
					//dataParam = new OracleParameter(pDataName, OracleType.Cursor);
					dataParam       = new DataParameter();
					dataParam.InitializeOracleCursor(dataName,pDataDirection);    
				}
				//
				else if ( Cst.TypeData.IsTypeInt(datatype))
				{
					dataParam       = new DataParameter(pCs, pCommandType, dataName, DbType.Int64);
					dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull:Convert.ToInt64(Value));
				}
				//
				else if ( Cst.TypeData.IsTypeDec(datatype))
				{
					dataParam       = new DataParameter(pCs, pCommandType, dataName, DbType.Decimal);
					dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull:DecFunc.DecValue(Value,CultureInfo.InvariantCulture));
				}
				//
				else if ( Cst.TypeData.IsTypeDate(datatype))
				{
					dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.Date);
					if ( StrFunc.IsFilled(dataformat))
						dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull:new DtFunc().StringToDateTime(Value,dataformat).Date);
					else
						dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull:new DtFunc().StringToDateTime(Value).Date);
				}
				//
				else if ( Cst.TypeData.IsTypeDateTime(datatype))
				{
					dataParam = new DataParameter(pCs, pCommandType, dataName, DbType.DateTime);                
					if ( StrFunc.IsFilled(dataformat))
						dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull:new DtFunc().StringToDateTime(Value,dataformat));
					else
						dataParam.Value = (ObjFunc.IsNull(Value) ? Convert.DBNull:new DtFunc().StringToDateTime(Value));
				}            
				//
				else // String et autre
				{
					dataParam           = new DataParameter(pCs, pCommandType, dataName, DbType.AnsiString);
					dataParam.Value     = (ObjFunc.IsNull(Value) ? Convert.DBNull: DataHelper.GetDBData(Value));
					//GP 20070201. Dans le cas d'un paramètre string ayant direction 'Output' || 'InputOutput' il faut assigner 
					//une dimension au paramètre, sinon l'appele à la SP Oracle retourne une exception 
					
					if (pSize>0)
						dataParam.Size  = pSize;
					//	
					if ( (dataParam.Size==0) && (pDataDirection == ParameterDirection.Output) || (pDataDirection == ParameterDirection.InputOutput) )
						dataParam.Size  = 4000;
				}
				//
				dataParam.Direction = pDataDirection; 
				//
				return dataParam;
			}
			catch (OTCmlException ex){throw  ex;}						
			catch (Exception ex){throw new OTCmlException("StringDynamicData.GetDataParameter", ex);}			
		}
		#endregion GetDataParameter
		//
		#region public GetDataSQLValue
		public string GetDataSQLValue(string pCs, IDbTransaction pTransaction)
		{
			try
			{
				string ret = string.Empty; 
				string Value    = GetDataValue(pCs,pTransaction); 
				//
				if ( Cst.TypeData.IsTypeBool(datatype))
					ret = DataHelper.SQLBoolean(BoolFunc.IsTrue(Value)); 
				else if (Cst.TypeData.IsTypeInt(datatype))
					ret = Value;   
				else if (Cst.TypeData.IsTypeDec(datatype))
					ret = Value;
				else if ( Cst.TypeData.IsTypeDate(datatype))
				{
					if ( StrFunc.IsFilled(dataformat))
						ret = DataHelper.SQLToDate(pCs,new DtFunc().StringToDateTime(Value,dataformat).Date);
					else
						ret = DataHelper.SQLToDate(pCs,new DtFunc().StringToDateTime(Value).Date);
				}
				else if ( Cst.TypeData.IsTypeDateTime(datatype))
				{
					if ( StrFunc.IsFilled(dataformat))
						ret = DataHelper.SQLToDateTime(pCs,new DtFunc().StringToDateTime(Value,dataformat));
					else
						ret = DataHelper.SQLToDateTime(pCs,new DtFunc().StringToDateTime(Value));
				}
				else if ( Cst.TypeData.IsTypeString(datatype) || Cst.TypeData.IsTypeText(datatype) )
				{
					ret = DataHelper.SQLString(Value);
				}
				return ret;
			}
			catch (OTCmlException ex){throw  ex;}						
			catch (Exception ex){throw new OTCmlException("StringDynamicData.GetDataSQLValue", ex);}			
		}
		#endregion public GetSQLExpression
		//
		#region public static GetDynamicstring
		public static string GetDynamicstring(string pCs, string pInput)
		{
		
			const string ElementStart = "<";
			const string ElementEnd   = "/>";
			//
			const string SQLStart     = ElementStart + "SQL ";
			const string SQLEnd       = "</SQL>";
			//
			const string SpheresLibStart = ElementStart + "SpheresLib ";
			const string SpheresLibEnd   = "</SpheresLib>";
			//
			const string DataStart = ElementStart + "Data ";
			const string DataEnd   = "</Data>";
			//
			try
			{
				string retString = pInput;
				//
				if (StrFunc.IsEmpty(retString))
					return retString;
				//
				int elementStartPos = retString.IndexOf(ElementStart);
				int elementEndPos    = -1;
				int elementStartPos2 = -1;
				//
				if (elementStartPos == -1)
					return retString;
				//
				if (false == (
					StrFunc.ContainsIn(retString,SpheresLibStart) ||  
					StrFunc.ContainsIn(retString,SQLStart)||
					StrFunc.ContainsIn(retString,DataStart) )) 
					return retString;
				//
				string       elementEnd;
				string       elementString;
				StringReader srElement;
				string       dataFormat   = string.Empty;
				Type         elementType;					
				string       dataResult   = string.Empty;
				//
				while(elementStartPos > -1)
				{
					bool isSQL        = false;
					bool isSpheresLib = false;
					bool isData       = false; 
					//				
					if ( retString.Substring(elementStartPos,SQLStart.Length)== SQLStart)
					{
						isSQL         = true;
						//
						elementEnd    = SQLEnd;
						elementEndPos = retString.IndexOf(elementEnd,elementStartPos);
						//
						// Si je trouve pas la balise fermante </SQL>
						// ou bien celle trouvée appartient à une autre balise ouvrante
						// ERROR
						elementStartPos2 = retString.IndexOf(SQLStart,elementStartPos + SQLStart.Length);
						if((elementEndPos == -1 ) || ((elementStartPos2 > -1 ) && (elementEndPos > elementStartPos2 )))
							throw new OTCmlException("StringDynamicData.GetDynamicstring","Element '" + SQLStart + "' is not closed - '" + elementEnd + "' is expected ","");
					}
					else if ( retString.Substring(elementStartPos,SpheresLibStart.Length)== SpheresLibStart)
					{
						isSpheresLib  = true;
						//
						elementEnd    = SpheresLibEnd;
						elementEndPos = retString.IndexOf(elementEnd,elementStartPos);
						//
						// Si je trouve pas la balise fermante </SpheresLib> 
						// ou bien celle trouvée appartient à une autre balise ouvrante
						// je cherche la balise fermante />
						elementStartPos2 = retString.IndexOf(SpheresLibStart,elementStartPos + SpheresLibStart.Length);
						if((elementEndPos == -1 ) || ((elementStartPos2 > -1 ) && (elementEndPos > elementStartPos2 )))
						{
							elementEnd       = ElementEnd;
							elementEndPos    = retString.IndexOf(elementEnd,elementStartPos);
							//
							// Si je trouve pas la balise fermante /> 
							// ou bien celle trouvée appartient à une autre balise ouvrante
							// ERROR
							elementStartPos2 = retString.IndexOf(ElementStart,elementStartPos + SpheresLibStart.Length);
							if ( ( elementEndPos == -1) || ((elementStartPos2 > -1 ) && (elementEndPos > elementStartPos2 )))
								throw new OTCmlException("StringDynamicData.GetDynamicstring","Element '" + SpheresLibStart + "' is not closed - '" + ElementEnd + "' or '" + SpheresLibEnd + "' are expected ","");
						}
					}
					
					else if ( retString.Substring(elementStartPos,DataStart.Length)== DataStart)
					{
						isData  = true;
						//
						elementEnd    = DataEnd;
						elementEndPos = retString.IndexOf(elementEnd,elementStartPos);
						//
						// Si je trouve pas la balise fermante </SpheresLib> 
						// ou bien celle trouvée appartient à une autre balise ouvrante
						// je cherche la balise fermante />
						elementStartPos2 = retString.IndexOf(DataStart,elementStartPos + DataStart.Length);
						if((elementEndPos == -1 ) || ((elementStartPos2 > -1 ) && (elementEndPos > elementStartPos2 )))
						{
							elementEnd       = ElementEnd;
							elementEndPos    = retString.IndexOf(elementEnd,elementStartPos);
							//
							// Si je trouve pas la balise fermante /> 
							// ou bien celle trouvée appartient à une autre balise ouvrante
							// ERROR
							elementStartPos2 = retString.IndexOf(ElementStart,elementStartPos + DataStart.Length);
							if ( ( elementEndPos == -1) || ((elementStartPos2 > -1 ) && (elementEndPos > elementStartPos2 )))
								throw new OTCmlException("StringDynamicData.GetDynamicstring","Element '" + DataStart  + "' is not closed - '" + ElementEnd + "' or '" + DataEnd + "' are expected ","");
						}
					}
					
					else
					{
						elementEnd    = ElementEnd;
						elementEndPos = retString.IndexOf(elementEnd,elementStartPos);
					}
					//
					elementString = retString.Substring(elementStartPos, elementEndPos + elementEnd.Length - elementStartPos);
					srElement     = new StringReader(elementString);
					//
					if(isSQL || isSpheresLib || isData )
					{					
						StringDynamicData  data = new StringDynamicData ();
						if(isData)
						{
							elementType   = typeof(StringDynamicData);   
							data = (StringDynamicData)CacheSerializer.Deserialize(elementType,srElement);
						}
							//
						else if(isSQL)
						{
							elementType   = typeof(DataSQL);   
							DataSQL dataSQL = (DataSQL)CacheSerializer.Deserialize(elementType,srElement);
							data.datatype = Cst.TypeData.TypeDataEnum.@string.ToString();     
							data.sql      = dataSQL; 
							//
						}
						else if (isSpheresLib)
						{
							elementType       = typeof(DataSpheresLib);                        
							DataSpheresLib dataSpheresLib = (DataSpheresLib)CacheSerializer.Deserialize(elementType,srElement);
							data.datatype = Cst.TypeData.TypeDataEnum.@string.ToString();
							data.spheresLib   = dataSpheresLib;
						}
						//
						dataResult = data.GetDataValue(pCs,null);   
						//
						if(StrFunc.IsFilled(dataResult))
						{
							retString = retString.Replace(elementString,dataResult);
						}
						else
							throw new OTCmlException("StringDynamicData.GetDynamicstring","No data found for: '" + elementString + "'","");
					}
					else
						throw new OTCmlException("StringDynamicData.GetDynamicstring","Element not supported: " + elementString,"");
					//
					elementStartPos = retString.IndexOf(ElementStart, elementStartPos + dataResult.Length);
					dataResult      = string.Empty;
				}
				return retString;
			}
			catch (OTCmlException ex) {throw ex;}
			catch (Exception ex) {throw new OTCmlException("GetDynamicstring",ex);}
		}
		#endregion public static GetDynamicstring
		//
	}
	#endregion

	#region public class DataSQL 
	[XmlRoot(ElementName = "SQL", IsNullable=true)]
	public class DataSQL  
	{     
		#region Members
		//devrait être CommandType 
		//Valeur POSSIBLE SELECT,TEXT,.... ou PROCEDURE
		[System.Xml.Serialization.XmlAttribute()]
		public string command;
		//PROCEDURE name
		[System.Xml.Serialization.XmlAttribute()]
		public string name;
		//Column for Result in command SELECT or PROCEDURE
		[System.Xml.Serialization.XmlAttribute()]
		public string result;
        //Query when command is SELECT or PROCEDURE
		[System.Xml.Serialization.XmlText()]
		public string Value;
        //SQL Parameters
		[System.Xml.Serialization.XmlElementAttribute("Param", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public ParamData[] param; 
		#endregion Members
		//
		#region accessor
		public bool isStoredProcedure
		{
			get
			{
				bool ret = false;
				if (StrFunc.IsFilled(command))
					ret = (command.ToUpper() == "PROCEDURE") ; 
				return ret;
			}
		}
		public bool isSelect
		{
			get
			{
				return (command.ToUpper() == "SELECT") || StrFunc.IsEmpty(command) ; 
			}
		}
		#endregion accessor
		//
		#region public GetParam
		public  ParamData GetParam (string pName)
		{
			ParamData retParam = null;
			//
			if (ArrFunc.IsFilled(param))
			{
				string name = pName.ToLower();
				for (int i=0;i<ArrFunc.Count(param);i++)
				{
					if (param[i].name.ToLower() == name)
						retParam = param[i];
				}
				if (retParam == null)
				{
					name = "p" + pName.ToLower();
					for (int i=0;i<ArrFunc.Count(param);i++)
					{
						if (param[i].name.ToLower() == name)
							retParam = param[i];
					}
				}
			}
			//
			return retParam;
		}
		#endregion GetParam
		//
		#region public Exec
		public string Exec(string pCs,IDbTransaction pdbTransaction)
		{
			
			string retValue     = null;
			IDataReader  drValue = null;
			bool isOracle = ConnectionStringCache.GetConnectionStringState(pCs) == ConnectionStringCacheState.isOracle;			
			//
			try
			{
				string selectQuery = Value;
				//
				//
				DataParameters dataParameters = new DataParameters(); 
				if (ArrFunc.IsFilled(param))
				{
					for (int i = 0 ; i < param.Length ;i++)
						dataParameters.Add(param[i].GetDataParameter(pCs,pdbTransaction,isStoredProcedure?CommandType.StoredProcedure:CommandType.Text));            
				}
				//
				if (isSelect)
				{
					//
					if (null != pdbTransaction)
						drValue  = DataHelper.ExecuteReader(pdbTransaction, CommandType.Text, selectQuery, dataParameters.GetArrayDbParameter() );					
					else
						drValue  = DataHelper.ExecuteReader(pCs, CommandType.Text, selectQuery , dataParameters.GetArrayDbParameter());					
				}
				//GLOP FI IO Voir pour lancer une procedure avec des paramètres (notamment Oracle)
				else if (isStoredProcedure)
				{
					if (isOracle)
					{
						DataParameter paramCursor = new DataParameter(); 
						paramCursor.InitializeOracleCursor("curs",ParameterDirection.Output);
						dataParameters.Add(paramCursor); 
					}
					//	
					if (null != pdbTransaction)
						drValue  = DataHelper.ExecuteReader(pdbTransaction, CommandType.StoredProcedure, name, dataParameters.GetArrayDbParameter());	
					else
						drValue  = DataHelper.ExecuteReader(pCs, CommandType.StoredProcedure, name,dataParameters.GetArrayDbParameter());	
				}
				else // Normalement on ne passe pas ici
				{
					if (null != pdbTransaction)
						DataHelper.ExecuteNonQuery(pdbTransaction, CommandType.Text, selectQuery, dataParameters.GetArrayDbParameter() );					
					else
						DataHelper.ExecuteNonQuery(pCs, CommandType.Text, selectQuery, dataParameters.GetArrayDbParameter() );					
				}

				//
				if ( (null!= drValue) && drValue.Read())
				{
					if( StrFunc.IsFilled(result))
					{
						if (IsExistColumn(drValue.GetSchemaTable(),result))
						{
							retValue = (Convert.IsDBNull(drValue[result])? null:ObjFunc.FmtToISo(drValue[result],Cst.TypeData.GetTypeFromSystemType(drValue.GetValue(0).GetType())));
						}
					}
					else
					{
						retValue = (Convert.IsDBNull(drValue.GetValue(0))? null:ObjFunc.FmtToISo(drValue.GetValue(0),Cst.TypeData.GetTypeFromSystemType(drValue.GetValue(0).GetType())));
					}
				}
				retValue =  (StrFunc.IsFilled(retValue)? retValue.Trim():retValue);
			}
			catch (OTCmlException ex){throw ex;}
			catch (Exception ex){throw new OTCmlException("DataSQL.Exec", ex);}
			finally 
			{ 
				if (null!=drValue) 
					drValue.Close();
			}
			//
			return retValue;
		}
		#endregion public Exec
		#region private IsExistColumn 
		private bool IsExistColumn(DataTable pSchemaTable, string pColumnName)
		{
			bool ret = false;
			if ( StrFunc.IsFilled(pColumnName))
			{
				foreach (DataRow myRow in pSchemaTable.Rows)
				{
					if ( myRow["ColumnName"].ToString() == pColumnName)
					{
						ret = true; 	
						break;
					}
				}
			}
			return ret;
		}
		#endregion IsExistColumn
	
	
	}
	#endregion

	#region  public class DataSpheresLib 
	[XmlRoot(ElementName = "SpheresLib", IsNullable=true)]
	public class DataSpheresLib  
	{     
		
		#region Members
		[XmlAttribute()]
		public string function;

		[XmlElementAttribute("Param", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public ParamData[] param; 
		#endregion Members
	
		#region public GetParam
		public bool GetParam (string pName, ref ParamData opParam)
		{
			string filler = string.Empty;
			return GetParam(pName, ref opParam, ref filler);
		}
		public bool GetParam (string pName, ref ParamData opParam, ref string opErrMsg)
		{
			bool ret = false;
			ParamData retParam = null;
			string name = pName.ToLower(); 
			//
			if (ArrFunc.IsEmpty(param))
				opErrMsg = "parameters are missing !";	
			else
			{
				for (int i=0;i<ArrFunc.Count(param);i++)
				{
					if (param[i].name.ToLower() == name)
						retParam = param[i];
				}
				//
				if (retParam == null)
				{
					name = "p" + pName.ToLower();
					for (int i=0;i<ArrFunc.Count(param);i++)
					{
						if (param[i].name.ToLower() == name)
							retParam = param[i];
					}
				}
				//
				ret = (retParam != null);
				if (!ret)
					opErrMsg = "parameter " + pName + " is missing !";
			}
			//
			opParam = retParam;
			return ret;
		}
		#endregion GetParam
	
		#region public Exec
		public string Exec(string pCs, IDbTransaction pdbTransaction)
		{
			try
			{
				string    methodName = "DataSpheresLib.Exec";
				ParamData paramItem  = null;
				string    checkValue;
				string    format     = string.Empty;
				int       idI        = 0;
				int       idE        = 0; 
				int		  idT        = 0; 	 
				//
				string    ret        = null; 
				string    functionName = function.ToUpper(); 
				string    errMsg      = string.Empty; 
				//
				switch (functionName)
				{
						//
						#region Spheres Functions for SQLUP
					case "GETNEWIDEVENT()":					
						ret = GetNewIDEvent(pCs,pdbTransaction);
						break;
					case "GETNEWIDTRADE()":					
						ret = GetNewIDTrade(pCs,pdbTransaction);
						break;	
					case "GETNEWID()":					
						if (!GetParam("IDGETID", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						//
						string idGetId =  paramItem.GetDataValue(pCs,pdbTransaction); 
						ret = GetNewID(pCs,pdbTransaction, idGetId); 
						break;	
						#endregion
						//
						#region Spheres ISTRADECOMPATIBLE, ISEVENTCOMPATIBLE 
					case "ISTRADECOMPATIBLE()":
					case "ISEVENTCOMPATIBLE()":
						bool isTrade = ("ISTRADECOMPATIBLE()" == functionName);
							
						if (ArrFunc.IsEmpty(param))
							throw new OTCmlException(methodName,"parameters are not defined");	
						//
						if (!GetParam("PROCESSTYPE", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						Cst.ProcessTypeEnum processType = (Cst.ProcessTypeEnum) System.Enum.Parse(typeof(Cst.ProcessTypeEnum),paramItem.GetDataValue(pCs, pdbTransaction),true); 
						//
						if (!GetParam("IDI", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						idI = Convert.ToInt32(paramItem.GetDataValue(pCs,pdbTransaction));    
						//
						idT = 0;
						idE = 0;
						if (isTrade)
						{
							if (!GetParam("IDT", ref paramItem , ref errMsg))
								throw new OTCmlException(methodName, functionName + ": " + errMsg);	
							idT = Convert.ToInt32(paramItem.GetDataValue(pCs,pdbTransaction));    
						}
						else
						{
							if (!GetParam("IDE", ref paramItem , ref errMsg))
								throw new OTCmlException(methodName, functionName + ": " + errMsg);	
							idE = Convert.ToInt32(paramItem.GetDataValue(pCs,pdbTransaction));    
						}
						//
						string appName = string.Empty; 
						if (GetParam("APPNAME", ref paramItem)) 
							appName = paramItem.GetDataValue(pCs,pdbTransaction);    
						//
						string hostName = string.Empty;
						if (GetParam("HOSTNAME", ref paramItem)) 
							hostName = paramItem.GetDataValue(pCs,pdbTransaction);    
								
						Cst.ErrLevel errlevel = Cst.ErrLevel.SUCCESS;
						//
						ProcessTuning processTuning = new ProcessTuning(pCs,idI,processType,appName,hostName); 
						if (processTuning.DrSpecified) 
						{
							string msgControl = string.Empty ;
							//
							if (isTrade)
							{
								TradeStatus tradeStatus = new TradeStatus(pCs);
								if (tradeStatus.Initialize(idT)) 
									errlevel = processTuning.ScanTradeCompatibility(idT,tradeStatus, ref msgControl);
							}
							else
							{
								EventStatus eventStatus = new EventStatus(pCs);
								if (eventStatus.Initialize(pdbTransaction,idE)) 
									errlevel = processTuning.ScanEventCompatibility(idE,eventStatus,ref msgControl);
							}
						}
						//
						ret = ObjFunc.FmtToISo(Cst.ErrLevel.SUCCESS== errlevel,Cst.TypeData.TypeDataEnum.@bool); 
						break;
						#endregion
						//
						#region Spheres Functions for Date 
					case "GETDATESYS()":
						if (GetParam("FORMAT", ref paramItem)) 
							format = paramItem.GetDataValue(pCs,pdbTransaction);
						ret = SystemTools.GetStringOSDateSys(format);
						break;

					case "GETDATETIMESYS()":
						if (GetParam("FORMAT", ref paramItem)) 
							format = paramItem.GetDataValue(pCs,pdbTransaction);
						ret = SystemTools.GetStringOSDateTimeSys(format);
						break;

					case "GETDATERDBMS()":
						if (GetParam("FORMAT", ref paramItem)) 
							format = paramItem.GetDataValue(pCs,pdbTransaction);
						ret = OTCmlHelper.GetStringRDBMSDateSys(pCs,format);
						break;

					case "GETDATETIMERDBMS()":
						if (GetParam("FORMAT", ref paramItem)) 
							format = paramItem.GetDataValue(pCs,pdbTransaction);
						ret = OTCmlHelper.GetStringRDBMSDateTimeSys(pCs , format);
						break;

					case "GETANTICIPATEDDATE()":
						if (GetParam("FORMAT", ref paramItem)) 
							format = paramItem.GetDataValue(pCs,pdbTransaction);
						//
						if (!GetParam("DATE", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						string dt_tmp = paramItem.GetDataValue(pCs, pdbTransaction);
						DateTime date = new DtFunc().StringDateISOToDateTime(dt_tmp);  
						if (DtFunc.IsDateTimeEmpty(date))
						{
							//Cas d'un datetime
							date = new DtFunc().StringDateTimeISOToDateTime(dt_tmp);  
						}
						ret = OTCmlHelper.GetStringAnticipatedDate(pCs, date, format);
						break;

					case "ISDATE()":
					case "ISNOTDATE()":
					case "ISDATETIME()":   
					case "ISNOTDATETIME()":   
					case "ISDATESYS()":    
					case "ISNOTDATESYS()":             
						if (GetParam("FORMAT", ref paramItem)) 
							format = paramItem.GetDataValue(pCs,pdbTransaction);
						//
						if (!GetParam("VALUE", ref paramItem, ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						//		
						checkValue = paramItem.GetDataValue(pCs,pdbTransaction); 
						//
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						//
						if ("ISDATE()" == functionName)
							ret = ObjFunc.FmtToISo(StrFunc.IsDate(checkValue, format,cultureInfo), Cst.TypeData.TypeDataEnum.@bool);  
						if ("ISNOTDATE()" == functionName)
							ret = ObjFunc.FmtToISo(false == StrFunc.IsDate(checkValue, format,cultureInfo), Cst.TypeData.TypeDataEnum.@bool);  
							//					
						else if ("ISDATETIME()" == functionName)
							ret = ObjFunc.FmtToISo(StrFunc.IsDateTime(checkValue,format,cultureInfo),Cst.TypeData.TypeDataEnum.@bool);  
						else if ("ISNOTDATETIME()" == functionName)
							ret = ObjFunc.FmtToISo(false ==StrFunc.IsDateTime(checkValue,format,cultureInfo),Cst.TypeData.TypeDataEnum.@bool);  
							//
						else if ("ISDATESYS()" == functionName)
							ret =  ObjFunc.FmtToISo(StrFunc.IsDateSys(checkValue,format,cultureInfo),Cst.TypeData.TypeDataEnum.@bool);  
						else if ("ISNOTDATESYS()" == functionName)
							ret =  ObjFunc.FmtToISo(false == StrFunc.IsDateSys(checkValue,format,cultureInfo),Cst.TypeData.TypeDataEnum.@bool);  
						break;
						//
						#endregion
						//
						#region Spheres Functions for Filled 
					case "ISNULL()":
					case "ISNOTNULL()":
					case "ISEMPTY()":
					case "ISNOTEMPTY()":
						if (!GetParam("VALUE", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						checkValue =  paramItem.GetDataValue(pCs,pdbTransaction); 
						//
						if ("ISNULL()" == functionName)
							ret = ObjFunc.FmtToISo(ObjFunc.IsNull(checkValue),Cst.TypeData.TypeDataEnum.@bool);  
						else if ("ISNOTNULL()" == functionName)
							ret = ObjFunc.FmtToISo( false == ObjFunc.IsNull(checkValue),Cst.TypeData.TypeDataEnum.@bool);  
						else if ("ISEMPTY()" == functionName)
							ret = ObjFunc.FmtToISo( StrFunc.IsEmpty(checkValue),Cst.TypeData.TypeDataEnum.@bool);  
						else if ("ISNOTEMPTY()" == functionName)
							ret = ObjFunc.FmtToISo( false ==  StrFunc.IsEmpty(checkValue),Cst.TypeData.TypeDataEnum.@bool);  
						break;
						#endregion
						//
					case "DELETEEVENTCLASS()":
						if (!GetParam("IDE", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						idE = Convert.ToInt32(paramItem.GetDataValue(pCs,pdbTransaction));
						//
						DataParameters sqlParam = new DataParameters();
						sqlParam.Add(DataParameter.GetParameter(pCs, DataParameter.ParameterEnum.IDE),idE);          
						//
						StrBuilder sqlDelete = new StrBuilder();
 						sqlDelete += SQLCst.DELETE_DBO  + Cst.OTCml_TBL.EVENTCLASS.ToString() + Cst.CrLf;
						sqlDelete += SQLCst.WHERE + "IDE=@IDE" + Cst.CrLf;
						if (null != pdbTransaction)
							DataHelper.ExecuteNonQuery(pdbTransaction, CommandType.Text, sqlDelete.ToString(), sqlParam.GetArrayDbParameter()  );
						else
							DataHelper.ExecuteNonQuery(pCs, CommandType.Text, sqlDelete.ToString(), sqlParam.GetArrayDbParameter() );
						break;
						//					
					case "ADDROWACCOUNTINGEVENTCLASS()":
						if (!GetParam("IDE", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						idE = Convert.ToInt32(paramItem.GetDataValue(pCs,pdbTransaction));
						//
						if (!GetParam("IDI", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						idI     = Convert.ToInt32(paramItem.GetDataValue(pCs,pdbTransaction)); 
						//
						if (!GetParam("DATE", ref paramItem , ref errMsg))
							throw new OTCmlException(methodName, functionName + ": " + errMsg);	
						DateTime dt = new DtFunc().StringToDateTime(paramItem.GetDataValue(pCs,pdbTransaction), paramItem.dataformat); 
						#warning EG 20071218 A gérer
						//EFS_AccountingEventClass.AddRowAccountingEventClass(pCs,pdbTransaction,idE,idI, dt);
						break;	
				}
				//
				return ret;
			}
			catch( OTCmlException ex) {throw ex;} 
			catch( Exception  ex) {throw new OTCmlException("DataSpheresLib.Exec", ex);} 	
		}
		#endregion public Exec
	
		#region private GetNewID
		private  string GetNewIDEvent(string pCs, IDbTransaction pDbTransaction)
		{
			return GetNewID(pCs,pDbTransaction, SQLUP.IdGetId.EVENT);  
		}
		private  string GetNewIDTrade(string pCs, IDbTransaction pDbTransaction)
		{
			return GetNewID(pCs,pDbTransaction,SQLUP.IdGetId.TRADE);  
		}
		private  string GetNewID(string pCs, IDbTransaction pDbTransaction, string pClass)
		{
			SQLUP.IdGetId id = (SQLUP.IdGetId) System.Enum.Parse(typeof(SQLUP.IdGetId),pClass,true);   	
			return GetNewID(pCs,pDbTransaction,id);
		}
		private  string GetNewID(string pCs, IDbTransaction pDbTransaction,  SQLUP.IdGetId pClass)
		{
			int newId = 0;
			if (null != pDbTransaction)
				SQLUP.GetId(out newId, pDbTransaction, pClass);
			else
				SQLUP.GetId(out newId, pCs, pClass);
			//			
			return  newId.ToString();
		}
		#endregion
	
	}
	#endregion class IODataSpheresLib    

	#region public class ParamData 
	[XmlRoot(ElementName = "Param", IsNullable=true)]
	public class ParamData   : StringDynamicData
	{     
		#region Members
		[System.Xml.Serialization.XmlAttribute()]
		public ParameterDirection direction; 
		
		[System.Xml.Serialization.XmlAttribute()]
		public int size; 
		#endregion Members
	
		#region constructor
		public ParamData(string pName , string pValue ) : base() 
		{
			name  = pName;
			Value = pValue;
		}

		public ParamData() : base() 
		{
			direction = ParameterDirection.Input;  
		}
		#endregion

		#region public GetDataParameter
		public  DataParameter GetDataParameter (string pCs, IDbTransaction pTransaction,  CommandType pCommandType)
		{
			return base.GetDataParameter(pCs,pTransaction, pCommandType, size,direction);
		}
		#endregion

	}
	#endregion
}

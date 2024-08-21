#region Using Directives
using System;
using System.Data;
using System.Globalization;
using System.Configuration;
using System.Reflection;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web; 

using EFS.GUI;
using EFS.GUI.CCI; 
using EFS.GUI.Interface;


using EfsML;

using FpML.Enum;

#endregion Using Directives

namespace EFS.TradeInformation
{
	#region CciEventClass
	public class CciEventClass : IContainerCciFactory, IContainerCci
	{
		#region Enums
        #region CciEnum
        public enum CciEnum
		{
			eventClass,
			dtEvent,
			isPayment,
			unknown,
        }
        #endregion CciEnum
        #endregion Enum
        #region Members
        public EventCustomCaptureInfos ccis;
        private EventClass eventClass;
        private readonly string prefix;
        private readonly int number;
        #endregion Members
		#region Accessors
		#region CS
		public string CS
		{
			get {return ccis.CS;}
		}
		#endregion CS
		
		#region EventClass
		public EventClass EventClass
		{
			set 
			{
				eventClass = (EventClass) value;
			}
			get 
			{ 
				return eventClass;
			}	
		}
		#endregion EventClass
		#region ExistNumber
		private bool ExistNumber 
		{
			get
			{ 
				return (0 < number); 
			}
		}
		#endregion ExistNumber
		#region Number
		private string Number
		{
			get
			{ 
				string ret =  string.Empty; 
				if (ExistNumber)
					ret = number.ToString();
				return ret;				
			}
		}
		#endregion Number
		#endregion Accessors
		#region Constructor
		public CciEventClass(CciEventItem pCciEventItem,int pNumber,EventClass pEventClass,string pPrefix)
		{
			number	   = pNumber;
			prefix	   = pPrefix + this.Number  + CustomObject.KEY_SEPARATOR ;
			ccis       = pCciEventItem.ccis;  	
			eventClass = pEventClass;
		}
		public CciEventClass(CciEvent pCciEvent,int pNumber,EventClass pEventClass,string pPrefix)
		{
			number	   = pNumber;
			prefix	   = pPrefix + this.Number  + CustomObject.KEY_SEPARATOR ;
			ccis       = pCciEvent.ccis;  	
			eventClass = pEventClass;
		}
		#endregion Constructors
        #region Methods
        #region Clear
        public void Clear()
        {
                ccis.Set(CciClientId(CciEnum.dtEvent), "NewValue", string.Empty);
                ccis.Set(CciClientId(CciEnum.isPayment), "NewValue", string.Empty);
            
        }
        #endregion Clear
        #region  SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
                ccis.Set(CciClientId(CciEnum.dtEvent), "IsEnabled", pIsEnabled);
                ccis.Set(CciClientId(CciEnum.isPayment), "IsEnabled", pIsEnabled);
                Cci(CciEnum.eventClass).IsEnabled = true;
            
        }
        #endregion SetEnabled
        #endregion Methods

		#region Interface Methods
		#region IContainerCciFactory members
		#region AddCciSystem
		public void AddCciSystem(){}
		#endregion AddCciSystem
		#region Initialize_FromCci
		public void Initialize_FromCci()
		{
		}
		#endregion Initialize_FromCci
		#region Initialize_FromDocument
		public void Initialize_FromDocument()
		{ 
			
				string data;
				bool isSetting;
				SQL_Table sql_Table;
			
				Type tCciEnum = typeof(CciEnum);
				foreach (string enumName in Enum.GetNames(tCciEnum))
				{
					CustomCaptureInfo cci = ccis[prefix + enumName];
					if (cci != null)
					{
						#region Reset variables
						data      = string.Empty;
						isSetting = true;
						sql_Table = null;
						#endregion Reset variables
						//
						CciEnum keyEnum = (CciEnum ) System.Enum.Parse(typeof(CciEnum ), enumName);
						switch (keyEnum)
						{
							case CciEnum.eventClass:
								#region EventClass
								data = eventClass.code;
								break;
								#endregion EventClass
							case CciEnum.dtEvent:
								#region DtEvent
								if (eventClass.dtEventSpecified)
									data = eventClass.dtEvent.Value;
								break;
								#endregion DtEvent
							case CciEnum.isPayment:
								#region IsPayment
								if (eventClass.isPaymentSpecified)
									data = eventClass.isPayment.Value;
								break;
								#endregion IsPayment
							default:
								isSetting = false;
								break;
						}
						if (isSetting)
							ccis.InitializeCci(cci,sql_Table,data);
					}
				}
				if (false == Cci(CciEnum.eventClass).IsMandatory)
					SetEnabled(Cci(CciEnum.eventClass).IsFilledValue);
			
		}
		#endregion Initialize_FromDocument
		#region Dump_ToDocument
		public void Dump_ToDocument()
		{ 
			bool isSetting;
			string data;
			CustomCaptureInfosBase.ProcessQueueEnum processQueue;

			Type tCciEnum = typeof(CciEnum);
			foreach (string enumName in Enum.GetNames(tCciEnum))
			{
				CustomCaptureInfo cci = ccis[prefix + enumName];
				if ((cci != null) && (cci.HasChanged))
				{
					#region Reset variables
					data                            = cci.NewValue;
					isSetting                       = true;
					processQueue                    = CustomCaptureInfosBase.ProcessQueueEnum.None;
					#endregion
					//
					CciEnum keyEnum = (CciEnum ) System.Enum.Parse(typeof(CciEnum ), enumName);
					switch (keyEnum)
					{
						case CciEnum.eventClass:
							#region Code
							if (StrFunc.IsEmpty(cci.NewValue) && (false == Cci(CciEnum.eventClass).IsMandatory) )  
								Clear(); 
							else
								eventClass.code = data;
							eventClass.codeSpecified = StrFunc.IsFilled(data);
							processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
							break;
							#endregion Code
						case CciEnum.dtEvent:
							#region Date
							if (null == eventClass.dtEvent)
								eventClass.dtEvent = new EFS_Date(data);
							else
								eventClass.dtEvent.Value = data;
							eventClass.dtEventSpecified = StrFunc.IsFilled(data);
							break;
							#endregion Date
						case CciEnum.isPayment:
							#region IsPayment
							if (null == eventClass.isPayment)
								eventClass.isPayment = new EFS_Boolean();
							eventClass.isPayment.Value = data;
							eventClass.isPaymentSpecified = StrFunc.IsFilled(data);
							break;
							#endregion IsPayment
						default:
							isSetting = false;
							break;
					}
					if (isSetting)
						ccis.Finalize(cci.ClientId_WithoutPrefix , processQueue );
				}
			}
			if (false == Cci(CciEnum.eventClass).IsMandatory)
				SetEnabled(Cci(CciEnum.eventClass).IsFilledValue);
		}

		#endregion Dump_ToDocument
		#region IsClientId_PayerOrReceiver
		public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
		{
			return false;
		}
		#endregion IsClientId_PayerOrReceiver
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region ProcessInitialize
		public void ProcessInitialize(CustomCaptureInfo pCci)
		{
			
		}
		#endregion ProcessInitialize
		#region SetDisplay
		public void SetDisplay(CustomCaptureInfo pCci)
		{
		}
		#endregion
		#region CleanUp
		public void CleanUp()
		{
		}
		#endregion
		#region RefreshCciEnabled
		public void RefreshCciEnabled()
		{
			
		}
		#endregion CleanUp
		#region RemoveLastItemInArray
		public void RemoveLastItemInArray(string _)
		{
		}
		#endregion RemoveLastItemInArray
		#region Initialize_Document
		public void Initialize_Document()
		{
		}
		#endregion Initialize_Document
		#endregion IContainerCciFactory Members
		#region IContainerCci Members
		#region CciClientId
		public string  CciClientId(CciEnum  pEnumValue) 
		{
			return CciClientId(pEnumValue.ToString()); 
		}
		public string CciClientId(string pClientId_Key)
		{
			return prefix + pClientId_Key  ;
		}
		#endregion 
		#region Cci
		public CustomCaptureInfo Cci(CciEnum pEnum  )
		{
			return Cci(pEnum.ToString()); 
		}
		public CustomCaptureInfo Cci(string pClientId_Key)
		{
			return ccis[CciClientId(pClientId_Key)];
		}

		#endregion 
		#region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(prefix));
		}
		#endregion 
		#region CciContainerKey
		public string CciContainerKey(string pClientId_WithoutPrefix)
		{
			return pClientId_WithoutPrefix.Substring(prefix.Length);
		}
		#endregion 
		#region IsCci
		public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci )
		{
			return   (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix)  ; 
		}
		#endregion
		#endregion IContainerCci Members
		#endregion Interface Methods
	}
	#endregion CciEventClass
}

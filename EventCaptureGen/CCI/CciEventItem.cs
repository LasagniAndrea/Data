#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML;
using System;
using System.Collections;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciEventItem
    public class CciEventItem : IContainerCciFactory, IContainerCci
	{
		#region Enums
        #region CciEnum
        public enum CciEnum
		{
			codes_eventCode,
			codes_eventType,
			eventCode,
			eventType,
			startDate,
			endDate,

			rate,
			fxRate,

			unknown,
        }
        #endregion CciEnum
        #endregion Enum
        #region Members
        public EventCustomCaptureInfos ccis;
        private Event _event;
        public CciEventClass[] cciEventClass;
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
		
		#region EventClassLenght
		public  int EventClassLenght
		{
			get { return  ArrFunc.IsFilled(cciEventClass) ?  cciEventClass.Length :0 ; }
		}
		#endregion EventClassLenght
		#region EventItem
		public Event EventItem
		{
			set {_event = (Event) value;}
			get {return _event;}	
		}
		#endregion EventItem
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
		public CciEventItem(CciEvent pCciEvent,int pNumber,Event pEvent,string pPrefix)
		{
			number	  = pNumber;
			prefix	  = pPrefix + this.Number  + CustomObject.KEY_SEPARATOR ;
			ccis      = pCciEvent.ccis;  	
			_event    = pEvent;
		}
		#endregion Constructors
        #region Methods
        #region Clear
        public static void Clear()
        {
            
        }
        #endregion Clear
        #region InitializeEventClass_FromCci
        private void InitializeEventClass_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList aEventClass = new ArrayList();
            //int maxEventClass = 0;
            //if (ArrFunc.IsFilled(_event.eventClass))
            //    maxEventClass = _event.eventClass.Length;

            while (isOk)
            {
                index += 1;
                CciEventClass cciEventClass = new CciEventClass(this, index + 1, null, prefix + EventCustomCaptureInfos.CCst.Prefix_eventClass);
                isOk = ccis.Contains(cciEventClass.CciClientId(CciEventClass.CciEnum.eventClass));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_event.eventClass) || (index == _event.eventClass.Length))
                        ReflectionTools.AddItemInArray(_event, "eventClass", index);
                    cciEventClass.EventClass = _event.eventClass[index];
                    aEventClass.Add(cciEventClass);
                }
            }
            this.cciEventClass = (CciEventClass[])aEventClass.ToArray(typeof(CciEventClass));
            for (int i = 0; i < this.cciEventClass.Length; i++)
                this.cciEventClass[i].Initialize_FromCci();
        }
        #endregion InitializeEventClass_FromCci
        #region  SetEnabled
        public static void SetEnabled(Boolean _)
        {
            
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
			if (3 == ArrFunc.Count(prefix.Split(CustomObject.KEY_SEPARATOR)))
			{
				InitializeEventClass_FromCci();
			}
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
							case CciEnum.codes_eventCode:
								#region EventCode
								data = ccis.EventInput.CurrentEvent.eventCode;
								break;
								#endregion EventCode
							case CciEnum.codes_eventType:
								#region EventType
                                data = ccis.EventInput.CurrentEvent.eventType;
								break;
								#endregion EventType

							case CciEnum.eventCode:
								#region EventCode
								data = _event.eventCode;
								break;
								#endregion EventCode
							case CciEnum.eventType:
								#region EventType
								data = _event.eventType;
								break;
								#endregion EventType
							case CciEnum.startDate:
								#region StartPeriod
								if (_event.dtStartPeriodSpecified)
									data = _event.dtStartPeriod.Value;
								break;
								#endregion StartPeriod
							case CciEnum.endDate:
								#region EndPeriod
								if (_event.dtEndPeriodSpecified)
									data = _event.dtEndPeriod.Value;
								break;
								#endregion EndPeriod
							case CciEnum.rate:
							case CciEnum.fxRate:
								#region Valorisation
								if (_event.valorisationSpecified)
									data = _event.valorisation.Value;
								break;
								#endregion Valorisation
							default:
								isSetting = false;
								break;
						}
						if (isSetting)
							ccis.InitializeCci(cci,sql_Table,data);
					}
				}
				#region EventClass
				if (ArrFunc.IsFilled(cciEventClass))
				{
					foreach (CciEventClass cci in cciEventClass)
						cci.Initialize_FromDocument();
				}
				#endregion EventClass

			
		}
		#endregion Initialize_FromDocument
		#region Dump_ToDocument
		public void Dump_ToDocument()
		{ 
			bool isSetting;
			string data;
			CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            
			for (int i=0; i< EventClassLenght;i++)
				cciEventClass[i].Dump_ToDocument();

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
						case CciEnum.eventCode:
							#region EventCode
							_event.eventCode = data;
							break;
							#endregion EventCode
						case CciEnum.eventType:
							#region EventType
							_event.eventType = data;
							break;
							#endregion EventType
						case CciEnum.startDate:
							#region StartPeriod
							if (null == _event.dtStartPeriod)
								_event.dtStartPeriod = new EFS_Date(data);
							else
								_event.dtStartPeriod.Value = data;
							_event.dtStartPeriodSpecified = StrFunc.IsFilled(data);
							break;
							#endregion StartPeriod
						case CciEnum.endDate:
							#region EndPeriod
							if (null == _event.dtEndPeriod)
								_event.dtEndPeriod = new EFS_Date(data);
							else
								_event.dtEndPeriod.Value = data;
							_event.dtEndPeriodSpecified = StrFunc.IsFilled(data);
							break;
							#endregion EndPeriod
						case CciEnum.rate:
						case CciEnum.fxRate:
							#region Valorisation
							_event.valorisationSpecified = StrFunc.IsFilled(data);
							_event.valorisation.Value    = data;
							break;
							#endregion Valorisation
						default:
							isSetting = false;
							break;
					}
					if (isSetting)
						ccis.Finalize(cci.ClientId_WithoutPrefix , processQueue );
				}
			}
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
		#region CleanUp
		public void CleanUp()
		{
		}
		#endregion CleanUp
		#region SetDisplay
		public void  SetDisplay(CustomCaptureInfo pCci)
		{
		}
		#endregion
		#region RefreshCciEnabled
		public void RefreshCciEnabled()
		{
			
		}
		#endregion
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
	#endregion CciEventItem
}

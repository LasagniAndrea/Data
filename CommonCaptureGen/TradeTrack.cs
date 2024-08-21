#region Using Directives
using EFS.ACommon;
using EFS.Common;

using EfsML.Business;
#endregion Using Directives

namespace EFS.Audit
{
    #region Application
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
	public class Application
	{
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string version;
		[System.Xml.Serialization.XmlAttributeAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
		public string browser;
		[System.Xml.Serialization.XmlTextAttribute(DataType="normalizedString")]
		public string Value;

		public Application(){}
		public Application(string pVersion, string pBrowser,string pValue)
		{
			if (StrFunc.IsFilled(pVersion))
				version = pVersion;
			if (StrFunc.IsFilled(pBrowser))
				browser = pBrowser;
			Value   = pValue;
		}

	}
	#endregion Application
	#region Confirm
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
	public class Confirm 
	{    
		[System.Xml.Serialization.XmlElementAttribute("initial")]
		public bool initialSend;
		[System.Xml.Serialization.XmlElementAttribute("interim")]
		public bool interimSend;
		[System.Xml.Serialization.XmlElementAttribute("final")]
		public bool finalSend;

        public Confirm(){}
	}
	#endregion Confirm
	#region Data
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
    // EG 20171025 [23509] Add businessDate, dtExecution, dtOrderEntered, tzFacility
	public class Data
	{
		[System.Xml.Serialization.XmlElementAttribute("tradeDate")]
		public string tradeDate;
        [System.Xml.Serialization.XmlElementAttribute("businessDate")]
        public string businessDate;
		[System.Xml.Serialization.XmlElementAttribute("timeStamp")]
		public string timeStamp;
        [System.Xml.Serialization.XmlElementAttribute("orderEntered")]
        public string orderEntered;
        [System.Xml.Serialization.XmlElementAttribute("execution")]
        public string execution;
        [System.Xml.Serialization.XmlElementAttribute("tzFacility")]
        public string tzFacility;
        [System.Xml.Serialization.XmlElementAttribute("status")]
		public Status[] status;
        [System.Xml.Serialization.XmlElementAttribute("confirm")]
        public Confirm confirm;

		public Data(){}
	}
	#endregion Data
	#region Identifier
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/TradeTrack-1-0")]
	public class Identifier
	{

		[System.Xml.Serialization.XmlAttributeAttribute("OTCml-Id")]
		public string id;

		[System.Xml.Serialization.XmlElementAttribute("identifier")]
		public string identifier;

		[System.Xml.Serialization.XmlElementAttribute("displayName")]
		public string displayName;


		public Identifier() { }
		public Identifier(string pId, string pIdentifier) : this(pId, pIdentifier, null) { }
		public Identifier(string pId, string pIdentifier, string pDisplayName)
		{
			id = pId;
			identifier = pIdentifier;
			displayName = pDisplayName;
		}
	}
	#endregion Identifier
    #region Design
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
    public class Design
    {
        [System.Xml.Serialization.XmlAttributeAttribute("backColor")]
        public string backColor;
        [System.Xml.Serialization.XmlAttributeAttribute("foreColor")]
        public string foreColor;
        [System.Xml.Serialization.XmlAttributeAttribute("image")]
        public string image;

		public Design(){}
		public Design(string pBackColor,string pForeColor): this(pBackColor,pForeColor,null){}
		public Design(string pBackColor,string pForeColor,string pImage)
		{
			backColor = pBackColor;
			foreColor = pForeColor;
			image     = pImage;
		}
    }
	#endregion Design
	#region Status
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/TradeTrack-1-0")]
	public class Status
	{
		[System.Xml.Serialization.XmlAttributeAttribute("OTCml-Id")]
		public string id;

		[System.Xml.Serialization.XmlElementAttribute("date")]
		public string date;

		[System.Xml.Serialization.XmlAttributeAttribute("type")]
		public StatusTypeEnum statusType;

		[System.Xml.Serialization.XmlElementAttribute("design")]
		public Design design;

		[System.Xml.Serialization.XmlElementAttribute("displayName")]
		public string displayName;

		public Status() { }
		public Status(StatusTypeEnum pStatusType, string pIdStatus, string pDate, string pDisplayName,
			string pBackColor, string pForeColor)
		{
			id = pIdStatus;
			date = pDate;
			displayName = pDisplayName;
			statusType = pStatusType;
			design = new Design(pBackColor, pForeColor);
		}
	}
	#endregion <Status>
	#region Track
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
	public class Track
	{
		[System.Xml.Serialization.XmlElementAttribute("action")]
		public string action;
		[System.Xml.Serialization.XmlElementAttribute("date")]
		public string date;
		[System.Xml.Serialization.XmlElementAttribute("user")]
		public Identifier user;
		[System.Xml.Serialization.XmlElementAttribute("application")]
		public Application application;
		[System.Xml.Serialization.XmlElementAttribute("screenName")]
		public string screenName;
		[System.Xml.Serialization.XmlElementAttribute("hostName")]
		public string hostName;
		[System.Xml.Serialization.XmlElementAttribute("data")]
		public Data data;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string tradeXML;

		
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool tradeXML_DocumentSpecified;
		// EG 20231127 [WI752] Exclusion de FpML 4.2
		[System.Xml.Serialization.XmlElementAttribute("EfsML", typeof(FpML.v44.Doc.Document), Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
		public object tradeXML_Document;
		
		
		#region public SetTradeXML
		public void SetTradeXML()
		{
			tradeXML_DocumentSpecified = StrFunc.IsFilled(tradeXML);
			if (tradeXML_DocumentSpecified)
			{
				EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(tradeXML);
				tradeXML_Document = CacheSerializer.Deserialize(serializerInfo);
			}
		}
		#endregion
		#region constructor Track
		public Track(){}
		#endregion Track
	}
	#endregion Track
	#region Trade
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
	public class Trade : Identifier
	{
		[System.Xml.Serialization.XmlElementAttribute("instrument")]
		public Identifier instrument;

		public Trade(){}

		public Trade(int pIdT,string pIdentifier,string pDisplayName, string pIdI,string pIdI_Identifier,string pIdI_DisplayName)
			:base(pIdT.ToString(),pIdentifier,pDisplayName)
		{
			this.instrument = new Identifier(pIdI,pIdI_Identifier,pIdI_DisplayName);
		}
	}
	#endregion Trade
	#region TradeTrack
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.efs.org/2005/TradeTrack-1-0")]
	[System.Xml.Serialization.XmlRootAttribute("TradeTrack", Namespace="http://www.efs.org/2005/TradeTrack-1-0", IsNullable=false)]
	public class TradeTrack
	{
		[System.Xml.Serialization.XmlElementAttribute("trade")]
		public Trade trade;
		[System.Xml.Serialization.XmlElementAttribute("track")]
		public Track[] track;
		
		#region constructor
		public TradeTrack(){}
		#endregion constructor
	}
	#endregion TradeTrack
	#region Track Enum
	#region StatusTypeEnum
	public enum StatusTypeEnum
	{
		Match,
		Check,
		Environment,
		Activation,
		Priority,
	}
	#endregion StatusTypeEnum
	#region ConfirmTypeEnum
	public enum ConfirmTypeEnum
	{
		Email,
		Fax,
		Paper,
		Swift,
		Telex,
	}
	#endregion ConfirmTypeEnum
	#endregion Track Enum
}

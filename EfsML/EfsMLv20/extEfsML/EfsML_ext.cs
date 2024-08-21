#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;


using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.ComplexControls;

using EfsML;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.Settlement;
using EfsML.v20.Settlement;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Doc;
using FpML.v42.Enum;
using FpML.v42.EqShared;
using FpML.v42.Eqs;
using FpML.v42.Fx;
using FpML.v42.Shared;
using FixML.v44;

#endregion Using Directives
namespace EfsML.v20
{

	#region AssetFxRateId
	public partial class AssetFxRateId : ICloneable, IAssetFxRateId
	{
		#region Constructors
		public AssetFxRateId() { }
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			AssetFxRateId clone = new AssetFxRateId();
			clone.Value = this.Value;
			clone.otcmlId = this.otcmlId;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IAssetFxRateId Members
		string IAssetFxRateId.Value
		{
			set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IAssetFxRateId Members
        #region ISpheresId
        string ISpheresId.otcmlId
		{
			set { this.otcmlId = value; }
			get { return this.otcmlId; }
		}
        int ISpheresId.OTCmlId
		{
			set { this.OTCmlId = value; }
			get { return this.OTCmlId; }
        }
        #endregion
    }
	#endregion AssetFxRateId
	#region AssetOrNothing
	public partial class AssetOrNothing : IAssetOrNothing
	{
		#region Constructors
		public AssetOrNothing() { }
		#endregion Constructors

		#region IAssetOrNothing Members
		bool IAssetOrNothing.currencyReferenceSpecified { get { return this.currencyReferenceSpecified; } }
		string IAssetOrNothing.currencyReference { get { return this.currencyReference.Value; } }
		bool IAssetOrNothing.gapSpecified { get { return this.gapSpecified; } }
		decimal IAssetOrNothing.gap { get { return this.gap.DecValue; } }
		#endregion IAssetOrNothing Members

	}
	#endregion AssetOrNothing
	#region AverageStrikeOption
	public partial class AverageStrikeOption : IAverageStrikeOption
	{
		#region Constructors
		public AverageStrikeOption() { }
		#endregion Constructors

		#region IAverageStrikeOption Members
		SettlementTypeEnum IAverageStrikeOption.settlementType
		{
			set { this.settlementType = value; }
			get { return this.settlementType; }
		}
		#endregion IAverageStrikeOption Members
	}
	#endregion AverageStrikeOption

	#region BookId
	public partial class BookId : IBookId
	{
		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int OTCmlId
		{
			get { return Convert.ToInt32(otcmlId); }
			set { otcmlId = value.ToString(); }
		}
		#endregion Accessors
		#region Constructors
		public BookId()
		{
			bookIdScheme = "http://www.euro-finance-systems.fr/otcml/bookid";
		}
		#endregion Constructors

        #region ISpheresId Members
        string ISpheresId.otcmlId
		{
			get { return this.otcmlId; }
			set { this.otcmlId = value; }
		}
        int ISpheresId.OTCmlId 
		{
			set { this.OTCmlId = value; } 
			get { return this.OTCmlId; }
        }
        #endregion
        #region IBookId Members
        bool IBookId.bookNameSpecified
        {
            get { return this.bookNameSpecified; }
            set { this.bookNameSpecified = value; }
        }
        string IBookId.bookName
        {
            get { return this.bookName; }
            set 
            { 
                this.bookName = value;
                this.bookNameSpecified = (this.bookName != null);
            }
        }
        #endregion IBookId Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.bookIdScheme; }
			set { this.bookIdScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion BookId

	#region CappedCallOrFlooredPut
	public partial class CappedCallOrFlooredPut : ICappedCallOrFlooredPut
	{
		#region Constructors
		public CappedCallOrFlooredPut()
		{
			typeFxCapBarrier = new Empty();
			typeFxFloorBarrier = new Empty();
		}
		#endregion Constructors

		#region ICappedCallOrFlooredPut Members
		bool ICappedCallOrFlooredPut.typeFxCapBarrierSpecified { get { return this.typeFxCapBarrierSpecified; } }
		bool ICappedCallOrFlooredPut.typeFxFloorBarrierSpecified { get { return this.typeFxFloorBarrierSpecified; } }
		PayoutEnum ICappedCallOrFlooredPut.payoutStyle { get {return this.payoutStyle;}}
		#endregion ICappedCallOrFlooredPut Members
	}
	#endregion CappedCallOrFlooredPut
	#region Css
	public partial class Css : ICss
	{
		#region Accessors

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int OTCmlId
		{
			get { return Convert.ToInt32(otcmlId); }
			set { otcmlId = value.ToString(); }
		}
		#endregion Accessors
		#region Constructors
		public Css() { }
		#endregion Constructor
		#region Methods
		#region ToString
		public override string ToString()
		{
			string ret = string.Empty;
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(ret, "css otcmlId : {0}", OTCmlId.ToString());
			ret = sb.ToString();
			//
			return ret;
		}
		#endregion ToString
		#endregion Methods


		#region ICss Members
		string ICss.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		string ICss.otcmlId
		{
			set { this.otcmlId = value; }
			get { return this.otcmlId; }
		}
		int ICss.OTCmlId
		{
			set { this.OTCmlId = value; }
			get { return this.OTCmlId; }
		}
		#endregion ICss Members
	}
	#endregion Css
	#region CssCriteria
    public partial class CssCriteria : ICssCriteria
    {
        #region Constructors
        public CssCriteria()
        {
            cssCriteriaCss = new Css();
            cssCriteriaCssInfo = new CssInfo();
        }
        public CssCriteria(ICssCriteria pCssCriteria)
        {
            if (null != pCssCriteria)
            {
                cssCriteriaCssSpecified = pCssCriteria.cssSpecified;
                cssCriteriaCss = (Css)pCssCriteria.css;
                //
                cssCriteriaCssInfoSpecified = pCssCriteria.cssInfoSpecified;
                cssCriteriaCssInfo = (CssInfo)pCssCriteria.cssInfo;
            }
        }
        #endregion Constructors
        #region Methods
        #region ToString
        public override string ToString()
        {
            string ret = string.Empty;
            StringBuilder sb = new StringBuilder();
            //
            if (cssCriteriaCssSpecified)
            {
                sb.AppendFormat(ret, "cssId : {0}", cssCriteriaCss.OTCmlId);
            }
            else if (cssCriteriaCssInfoSpecified)
            {
                string cssCountry = cssCriteriaCssInfo.cssCountrySpecified ? cssCriteriaCssInfo.cssCountry.Value : "N/A";
                string cssPaymentType = cssCriteriaCssInfo.cssPaymentTypeSpecified ? cssCriteriaCssInfo.cssPaymentType.Value : "N/A";
                string cssSettlementType = cssCriteriaCssInfo.cssSettlementTypeSpecified ? cssCriteriaCssInfo.cssSettlementType.Value : "N/A";
                string cssSystemType = cssCriteriaCssInfo.cssSystemTypeSpecified ? cssCriteriaCssInfo.cssSystemType.Value : "N/A";
                string cssType = cssCriteriaCssInfo.cssTypeSpecified ? cssCriteriaCssInfo.cssType.Value : "N/A";
                //	
                sb.AppendFormat(ret, "cssCountry:{0}, cssPaymentType:{1}, cssSettlementType:{2}, cssSystemType:{3}, cssType:{4}",
                    cssCountry, cssPaymentType, cssSettlementType, cssSystemType, cssType);
            }
            ret = sb.ToString();
            //
            return ret;
        }
        #endregion ToString
        #endregion Methods
        #region ICssCriteria Members
        bool ICssCriteria.cssSpecified
        {
            get { return this.cssCriteriaCssSpecified; }
            set { this.cssCriteriaCssSpecified = value; }
        }
        ICss ICssCriteria.css
        {
            get { return this.cssCriteriaCss; }
            set { this.cssCriteriaCss = (Css)value; }
        }
        bool ICssCriteria.cssInfoSpecified
        {
            get { return this.cssCriteriaCssInfoSpecified; }
            set { this.cssCriteriaCssInfoSpecified = value; }
        }
        ICssInfo ICssCriteria.cssInfo
        {
            get { return this.cssCriteriaCssInfo; }
            set { this.cssCriteriaCssInfo = (CssInfo)value; }
        }
        #endregion ICssCriteria Members
    }
	#endregion CssCriteria
	#region CssInfo
	public partial class CssInfo : ICssInfo
	{
		#region Constructors
		public CssInfo() { }
		#endregion Constructors

		#region ICssInfo Members
		bool ICssInfo.countrySpecified
		{
			get { return this.cssCountrySpecified; }
		}
		IScheme ICssInfo.country
		{
			get { return this.cssCountry; }
		}
		bool ICssInfo.typeSpecified
		{
			get { return this.cssTypeSpecified; }
		}
		IScheme ICssInfo.type
		{
			get { return this.cssType; }
		}
		bool ICssInfo.settlementTypeSpecified
		{
			get { return this.cssSettlementTypeSpecified; }
		}
		IScheme ICssInfo.settlementType
		{
			get { return this.cssSettlementType; }
		}
		bool ICssInfo.paymentTypeSpecified
		{
			get { return this.cssPaymentTypeSpecified; }
		}
		IScheme ICssInfo.paymentType
		{
			get { return this.cssPaymentType;}
		}
		bool ICssInfo.systemTypeSpecified
		{
			get { return this.cssSystemTypeSpecified; }
		}
		IScheme ICssInfo.systemType
		{
			get { return this.cssSettlementType; }
		}
		#endregion ICssInfo Members
	}
	#endregion CssInfo
	#region CssPaymentType
	public partial class CssPaymentType : IScheme
	{
		#region Constructors
		public CssPaymentType()
		{
			cssPaymentTypeScheme = "http://www.euro-finance-systems.fr/otcml/cssPaymentType";
			Value = string.Empty;
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.cssPaymentTypeScheme; }
			set { this.cssPaymentTypeScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion CssPaymentType
	#region CssSettlementType
	public partial class CssSettlementType : IScheme
	{
		#region Constructors
		public CssSettlementType()
		{
			cssSettlementTypeScheme = "http://www.euro-finance-systems.fr/otcml/cssSettlementType";
			Value = string.Empty;
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.cssSettlementTypeScheme; }
			set { this.cssSettlementTypeScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion CssSettlementType
	#region CssSystemType
	public partial class CssSystemType : IScheme
	{
		#region Constructors
		public CssSystemType()
		{
			cssSystemTypeScheme = "http://www.euro-finance-systems.fr/otcml/cssSystemType";
			Value = string.Empty;
		}
		#endregion Constructors
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.cssSystemTypeScheme; }
			set { this.cssSystemTypeScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion CssSystemType
	#region CssType
	public partial class CssType : IScheme
	{
		#region Constructors
		public CssType()
		{
			cssTypeScheme = "http://www.euro-finance-systems.fr/otcml/cssType";
			Value = string.Empty;
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.cssTypeScheme; }
			set { this.cssTypeScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion CssType
	#region CustomerSettlementPayment
	public partial class CustomerSettlementPayment : ICustomerSettlementPayment
	{
		#region Constructors
		public CustomerSettlementPayment()
		{
			rate = new ExchangeRate();
			currency = new Currency();
		}
		#endregion Constructors
		#region ICustomerSettlementPayment Members
		IExchangeRate ICustomerSettlementPayment.rate { get { return rate; } }
		bool ICustomerSettlementPayment.amountSpecified 
		{
			set { amountSpecified = value; } 
			get { return amountSpecified; } 
		}
        IMoney ICustomerSettlementPayment.GetMoney()
        {
            return new Money(amount.DecValue, currency.Value);
        }
        EFS_Decimal ICustomerSettlementPayment.amount
        {
            set { amount  = value; }
            get { return amount ; }
        }
        string ICustomerSettlementPayment.currency
		{
			set { currency.Value = value; }
			get 
			{ 
				if (null != currency)
					return currency.Value; 
				else
					return null;
			}
		}
		#endregion ICustomerSettlementPayment Members

	}
	#endregion CustomerSettlementPayment

	#region EfsDocument
	public partial class EfsDocument : IEfsDocument
	{
		#region Accessors
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance", DataType = "anyURI")]
		public string schemaLocation
		{
			set {efs_schemaLocation = "http://www.efs.org/2005/EFSmL-2-0 EFSmL-2-0.xsd";}
			get { return efs_schemaLocation; }
		}
		#endregion Accessors
		#region Constructors
		public EfsDocument()
		{
			EfsMLversion   = EfsMLDocumentVersionEnum.Version20;
			schemaLocation = string.Empty;
		}
		#endregion Constructors
        //
        #region IEfsDocument Members
        #region Accessors
        EfsMLDocumentVersionEnum IEfsDocument.EfsMLversion
        {
            get { return this.EfsMLversion; }
            set { this.EfsMLversion = value;}
        }
        bool IRepositoryDocument.repositorySpecified
        {
            set { ; }
            get { return false; }
        }
        IRepository IRepositoryDocument.repository
        {
            set { ;}
            get { return null; } 
        }



		#endregion Accessors
		#endregion IEfsDocument Members
        //
        #region Methods
        IRepository IRepositoryDocument.CreateRepository()
        {
            return null;
        }


        /// <summary>
        /// Retourne tous les assets présents 
        /// <para>Retourne systmatiquement une liste vide</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20150708 [XXXXX] Add
        List<IAssetRepository> IRepositoryDocument.GetAllRepositoryAsset()
        {

            List<IAssetRepository> ret = new List<IAssetRepository>();
            return ret;
        }
        #endregion
	}
	#endregion EfsDocument
	#region EfsSettlementInformation
	public partial class EfsSettlementInformation : IEfsSettlementInformation
	{
		#region Constructors
		public EfsSettlementInformation()
		{
			informationInstruction = new EfsSettlementInstruction();
		}
		#endregion Constructors

		#region IEfsSettlementInformation Members
		bool IEfsSettlementInformation.standardSpecified
		{
			set { this.informationStandardSpecified = value; }
			get { return this.informationStandardSpecified; }
		}
		StandardSettlementStyleEnum IEfsSettlementInformation.standard
		{
			get { return this.informationStandard; }
		}
		bool IEfsSettlementInformation.instructionSpecified
		{
			set { this.informationInstructionSpecified = value; }
			get { return this.informationInstructionSpecified; }
		}
		IEfsSettlementInstruction IEfsSettlementInformation.instruction
		{
			get { return this.informationInstruction; }
		}
		#endregion IEfsSettlementInformation Members
	}
	#endregion EfsSettlementInformation
	#region EfsSettlementInstruction
	public partial class EfsSettlementInstruction : IEfsSettlementInstruction
	{
		#region IEfsSettlementInstruction Members
		#region Accessors
		bool IEfsSettlementInstruction.settlementMethodSpecified
		{
			set { this.settlementMethodSpecified = value; }
			get { return this.settlementMethodSpecified; }
		}
		IScheme IEfsSettlementInstruction.settlementMethod
		{
			set { this.settlementMethod = (SettlementMethod)value; }
			get { return (IScheme)this.settlementMethod; }
		}
		bool IEfsSettlementInstruction.beneficiaryBankSpecified 
		{ 
			set { this.beneficiaryBankSpecified = value; } 
			get { return this.beneficiaryBankSpecified; } 
		}
		IRouting IEfsSettlementInstruction.beneficiaryBank 
		{
			set { this.beneficiaryBank = (Routing)value; } 
			get { return (IRouting)this.beneficiaryBank; } 
		}
		IRouting IEfsSettlementInstruction.beneficiary 
		{
			set { this.beneficiary = (Routing)value; } 
			get { return (IRouting)this.beneficiary; } 
		}
		bool IEfsSettlementInstruction.correspondentInformationSpecified
		{
			set { this.correspondentInformationSpecified = value; }
			get { return this.correspondentInformationSpecified; }
		}
		IRouting IEfsSettlementInstruction.correspondentInformation
		{
			set { this.correspondentInformation = (Routing)value; }
			get { return (IRouting)this.correspondentInformation; }
		}
		bool IEfsSettlementInstruction.intermediaryInformationSpecified 
		{
			set { this.intermediaryInformationSpecified = value; } 
			get { return this.intermediaryInformationSpecified; } 
		}
		IIntermediaryInformation[] IEfsSettlementInstruction.intermediaryInformation 
		{
			set { this.intermediaryInformation = (IntermediaryInformation[])value; }
			get { return this.intermediaryInformation; } 
		}
		bool IEfsSettlementInstruction.settlementMethodInformationSpecified
		{
			set { this.settlementMethodInformationSpecified = value; }
			get { return this.settlementMethodInformationSpecified; }
		}
		IRouting IEfsSettlementInstruction.settlementMethodInformation
		{
			set { this.settlementMethodInformation = (Routing)value; }
			get { return (IRouting)this.settlementMethodInformation; }
		}
		bool IEfsSettlementInstruction.investorInformationSpecified
		{
			set { this.investorInformationSpecified = value; }
			get { return this.investorInformationSpecified; }
		}
		IRouting IEfsSettlementInstruction.investorInformation
		{
			set { this.investorInformation = (Routing)value; }
			get { return this.investorInformation; }
		}

		bool IEfsSettlementInstruction.originatorInformationSpecified
		{
			set { this.originatorInformationSpecified = value; }
			get { return this.originatorInformationSpecified; }
		}

		IRouting IEfsSettlementInstruction.originatorInformation
		{
			set { this.originatorInformation = (Routing)value; }
			get { return this.originatorInformation; }
		}
		int IEfsSettlementInstruction.idssiDb
		{
			set { this.idssiDb = value; }
			get { return this.idssiDb; }
		}
		int IEfsSettlementInstruction.idIssi
		{
			set { this.idIssi = value; }
			get { return this.idIssi; }
		}
		#endregion Accessors
		#region Methods
		
		IScheme IEfsSettlementInstruction.CreateSettlementMethod()
		{
			 return (IScheme)new SettlementMethod(); 
		}
		IRouting IEfsSettlementInstruction.CreateCorrespondentInformation()
		{
			return new Routing(); 
		}

        IIntermediaryInformation IEfsSettlementInstruction.CreateIntermediaryInformation()
        {
            return (IIntermediaryInformation)new IntermediaryInformation();
        }

		IRouting IEfsSettlementInstruction.CreateBeneficiary()
		{
			 return new Routing(); 
		}
		IRouting IEfsSettlementInstruction.CreateRouting()
		{
			 return new Routing(); 
		}
        IRoutingCreateElement IEfsSettlementInstruction.CreateRoutingCreateElement()
        {
            return new RoutingCreateElement();
        }
        #endregion Methods
		#endregion IEfsSettlementInstruction Members
	}
	#endregion EfsSettlementInstruction
	#region EventCode
	public partial class EventCode : IScheme
	{
		#region Constructors
		public EventCode()
		{
			eventCodeScheme = "http://www.euro-finance-systems.fr/otcml/eventCode";
			Value = string.Empty;
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.scheme
		{
			set{this.eventCodeScheme = value;}
			get{return this.eventCodeScheme;}
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion EventCode
	#region EventCodes
	public partial class EventCodes : IEFS_Array, IComparable, IEventCodes
	{
		#region Constructors
		public EventCodes()
		{
			settlementDate = new EFS_Date();
			productReference = new ProductReference();
		}
		#endregion Constructors
        //
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
        //
		#region IComparable Members
		#region CompareTo
		/// <summary>
		///  Use to compare to compatible EventCode
		/// </summary>
		/// <param name="pobj"></param>
		/// <returns></returns>
        public int CompareTo(object pObj)
		{
			int ret = 0;
			//
			// Date      + fin que EventType
			// EventType + fin que EventCode
			// EventCode + fin que StreamId
			// StreamId  + fin que ProductReference
			//
            EventCodes eventcode = pObj as EventCodes;
            if (null != eventcode)
			{
				//Date
                if (settlementDateSpecified && !eventcode.settlementDateSpecified)
					ret = 5;
                if (!settlementDateSpecified && eventcode.settlementDateSpecified)
					ret = -5;
				//EventType
				if (0 == ret)
				{
                    if (eventTypeSpecified && !eventcode.eventTypeSpecified)
						ret = 4;
                    else if (!eventTypeSpecified && eventcode.eventTypeSpecified)
						ret = -4;
				}
				//EventCode
				if (0 == ret)
				{
                    if (eventCodeSpecified && !eventcode.eventCodeSpecified)
						ret = 3;
                    else if (!eventCodeSpecified && eventcode.eventCodeSpecified)
						ret = -3;
				}
				//Stream
				if (0 == ret)
				{
                    if (StreamIdSpecified && !eventcode.StreamIdSpecified)
						ret = 2;
                    else if (!StreamIdSpecified && eventcode.StreamIdSpecified)
						ret = -2;
				}
				//Instr
				if (0 == ret)
				{
                    if (productReferenceSpecified && !eventcode.productReferenceSpecified)
						ret = 1;
                    else if (!productReferenceSpecified && eventcode.productReferenceSpecified)
						ret = -1;
				}
			}
			return ret;
		}
		#endregion CompareTo
		#endregion IComparable Members
        //
		#region IEventCodes Members
		bool IEventCodes.productReferenceSpecified
		{
			get { return this.productReferenceSpecified; }
		}
		IReference IEventCodes.productReference
		{
			get { return this.productReference; }
		}
		bool IEventCodes.StreamIdSpecified
		{
			get { return this.StreamIdSpecified; }
		}
		EFS_NonNegativeInteger IEventCodes.streamId
		{
			get { return this.streamId; }
		}
		bool IEventCodes.eventCodeSpecified
		{
			get { return this.eventCodeSpecified; }
		}
		IScheme IEventCodes.eventCode
		{
			get { return this.eventCode; }
		}
		bool IEventCodes.eventTypeSpecified
		{
			get { return this.eventTypeSpecified; }
		}
		IScheme IEventCodes.eventType
		{
			get { return this.eventType; }
		}
		bool IEventCodes.settlementDateSpecified
		{
			get { return this.settlementDateSpecified; }
		}
		EFS_Date IEventCodes.settlementDate
		{
			get { return this.settlementDate; }
		}
		#endregion IEventCodes Members
	}
	#endregion EventCodes
	#region EventCodesSchedule
	public partial class EventCodesSchedule : IComparable, IEventCodesSchedule
	{
        #region IEventCodesSchedule Members
        IEventCodes[] IEventCodesSchedule.eventCodes
        {
            get { return this.eventCodes; }
        }
        #endregion IEventCodesSchedule Members
        //
		#region IComparable Members
		/// <summary>
		///  Use For Compare Compatible CodeShedule
		/// </summary>
		/// <param name="pobj"></param>
		/// <returns></returns>
		#region CompareTo
		public int CompareTo(object pObj)
		{
			int ret = 0;
			int minValue = 0;
			int maxValue = 0;
            EventCodesSchedule eventCodesSchedule = pObj as EventCodesSchedule;
            if (null != eventCodesSchedule)
			{
				for (int i = 0; i < eventCodes.Length; i++)
				{
                    for (int j = 0; j < eventCodesSchedule.eventCodes.Length; j++)
					{
                        int currentValue = eventCodes[i].CompareTo(eventCodesSchedule.eventCodes[j]);
						if (currentValue > 0 && currentValue > maxValue)
							maxValue = currentValue;
						else if (currentValue < 0 && currentValue < minValue)
							minValue = currentValue;
					}
				}
				//
				if (System.Math.Abs(minValue) > maxValue)
					ret = minValue;
				//
				if (maxValue > System.Math.Abs(minValue))
					ret = maxValue;
			}
			return ret;
		}
		#endregion CompareTo
		#endregion IComparable Members
    }
	#endregion EventCodesSchedule
	#region EventType
	public partial class EventType : IScheme
	{
		#region Constructors
		public EventType()
		{
			eventTypeScheme = "http://www.euro-finance-systems.fr/otcml/eventType";
			Value = string.Empty;
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.scheme
		{
			set { this.eventTypeScheme = value; }
			get { return this.eventTypeScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion EventType

	#region FixML
	public partial class FIXML : IProduct
	{
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion FixML
	#region FlowContext
    public partial class FlowContext : IComparable, IFlowContext
    {
        #region Constructors
        public FlowContext() { }
        #endregion Constructors

        #region IComparable Members
        #region CompareTo
        public int CompareTo(object pObj)
        {
            int ret = 0;
            FlowContext flowContext = pObj as FlowContext;
            if (null != flowContext)
            {
                if (eventCodesScheduleSpecified && (false == flowContext.eventCodesScheduleSpecified))
                    ret = 1;
                else if (!eventCodesScheduleSpecified && (flowContext.eventCodesScheduleSpecified))
                    ret = -1;
                //				
                if (0 == ret)
                {
                    if ((eventCodesScheduleSpecified) && flowContext.eventCodesScheduleSpecified)
                    {
                        int compare = eventCodesSchedule.CompareTo(flowContext.eventCodesSchedule);
                        if (compare > 0)
                            ret = compare;
                        else if (compare < 0)
                            ret = compare;
                    }
                }
                //
                if (0 == ret)
                {
                    if (partyContextSpecified && (false == flowContext.partyContextSpecified))
                        ret = 1;
                    else if (!partyContextSpecified && (flowContext.partyContextSpecified))
                        ret = -1;
                }
                //
                //20090609FI [16603] gestion de cashSecurities
                if (0 == ret)
                {
                    if (cashSecuritiesSpecified && (false == flowContext.cashSecuritiesSpecified))
                        ret = 1;
                    else if (!cashSecuritiesSpecified && (flowContext.cashSecuritiesSpecified))
                        ret = -1;
                }
                //
                if (0 == ret)
                {
                    if (currencySpecified && (false == flowContext.currencySpecified))
                        ret = 1;
                    else if (!currencySpecified && (flowContext.currencySpecified))
                        ret = -1;
                }

            }
            else
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "pobj is not a FlowContext");
            }
            return ret;
        }

        #endregion CompareTo
        #endregion IComparable Members

        #region IFlowContext Members
        bool IFlowContext.currencySpecified
        {
            get { return this.currencySpecified; }
            set { this.currencySpecified = value; }
        }
        string IFlowContext.currency
        {
            get { return this.currency.Value; }
            set { this.currency.Value = value; }
        }
        //
        bool IFlowContext.partyContextSpecified
        {
            get { return this.partyContextSpecified; }
            set { this.partyContextSpecified = value; }
        }
        IPartyPayerReceiverReference[] IFlowContext.partyContext
        {
            get { return this.partyContext; }
            set { this.partyContext = (PartyPayerReceiverReference[])value; }
        }
        //
        bool IFlowContext.eventCodesScheduleSpecified
        {
            get { return this.eventCodesScheduleSpecified; }
            set { this.eventCodesScheduleSpecified = value; }
        }
        IEventCodesSchedule IFlowContext.eventCodesSchedule
        {
            get { return this.eventCodesSchedule; }
            set { this.eventCodesSchedule = (EventCodesSchedule)value; }
        }
        //
        bool IFlowContext.cashSecuritiesSpecified
        {
            get { return false; }
            set { }
        }
        CashSecuritiesEnum IFlowContext.cashSecurities
        {
            get { return CashSecuritiesEnum.CASH; }
            set { }
        }
        //
        //public bool IFlowContext.IsMatchWith(SQL_Event pSqlEvent, PayerReceiverEnum pPayerReceiver, DataDocumentContainer pDocument)
        //{
        //    return SettlementTools.IsEventMatchWithContext(this, pSqlEvent, pPayerReceiver, pDocument);  
        //}
        #endregion IFlowContext Members
    }
    #endregion FlowContext
	#region FxClass
	public partial class FxClass : ICloneable, IScheme
	{
		#region Constructors
		public FxClass()
		{
			fxClassScheme = Cst.OTCmL_FxClassScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			FxClass clone = new FxClass();
			clone.fxClassScheme = this.fxClassScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.fxClassScheme; }
			set { this.fxClassScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion FxClass

	#region HedgeClassDerv
	public partial class HedgeClassDerv : ICloneable, IScheme
	{
		#region Constructors
		public HedgeClassDerv()
		{
			hedgeClassDervScheme = Cst.OTCmL_HedgeClassDervScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			HedgeClassDerv clone = new HedgeClassDerv();
			clone.hedgeClassDervScheme = this.hedgeClassDervScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.hedgeClassDervScheme; }
			set { this.hedgeClassDervScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion HedgeClassDerv
	#region HedgeClassNDrv
	public partial class HedgeClassNDrv : ICloneable, IScheme
	{
		#region Constructors
		public HedgeClassNDrv()
		{
			hedgeClassNDrvScheme = Cst.OTCmL_HedgeClassNDrvScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			HedgeClassNDrv clone = new HedgeClassNDrv();
			clone.hedgeClassNDrvScheme = this.hedgeClassNDrvScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.hedgeClassNDrvScheme; }
			set { this.hedgeClassNDrvScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion HedgeClassNDrv 
    #region IASClassDerv
    public partial class IASClassDerv : ICloneable, IScheme
	{
		#region Constructors
		public IASClassDerv()
		{
			iasClassDervScheme = Cst.OTCmL_IASClassDervScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			IASClassDerv clone = new IASClassDerv();
			clone.iasClassDervScheme = this.iasClassDervScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.iasClassDervScheme; }
			set { this.iasClassDervScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion IASClassDerv
	#region IASClassNDrv
	public partial class IASClassNDrv : ICloneable, IScheme
	{
		#region Constructors
		public IASClassNDrv()
		{
			iasClassNDrvScheme = Cst.OTCmL_IASClassNDrvScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			IASClassNDrv clone = new IASClassNDrv();
			clone.iasClassNDrvScheme = this.iasClassNDrvScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.iasClassNDrvScheme; }
			set { this.iasClassNDrvScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion IASClassNDrv
	#region ImplicitProvision
	public partial class ImplicitProvision : IImplicitProvision
	{
		#region Constructors
		public ImplicitProvision()
		{
			cancelableProvision = new Empty();
			mandatoryEarlyTerminationProvision = new Empty();
			optionalEarlyTerminationProvision = new Empty();
			extendibleProvision = new Empty();
			stepUpProvision = new Empty();
		}
		#endregion Constructors
	
		#region IImplicitProvision Membres
		bool  IImplicitProvision.cancelableProvisionSpecified
		{
			set { this.cancelableProvisionSpecified = value; }
			get { return this.cancelableProvisionSpecified; }
		}
		IEmpty  IImplicitProvision.cancelableProvision
		{
			set { this.cancelableProvision = (Empty) value; }
			get { return this.cancelableProvision; }
		}
		bool  IImplicitProvision.mandatoryEarlyTerminationProvisionSpecified
		{
			set { this.mandatoryEarlyTerminationProvisionSpecified = value; }
			get { return this.mandatoryEarlyTerminationProvisionSpecified; }
		}
		IEmpty  IImplicitProvision.mandatoryEarlyTerminationProvision
		{
			set { this.mandatoryEarlyTerminationProvision = (Empty)value; }
			get { return this.mandatoryEarlyTerminationProvision; }
		}
		bool  IImplicitProvision.optionalEarlyTerminationProvisionSpecified
		{
			set { this.optionalEarlyTerminationProvisionSpecified = value; }
			get { return this.optionalEarlyTerminationProvisionSpecified; }
		}
		IEmpty  IImplicitProvision.optionalEarlyTerminationProvision
		{
			set { this.optionalEarlyTerminationProvision = (Empty)value; }
			get { return this.optionalEarlyTerminationProvision; }
		}
		bool  IImplicitProvision.extendibleProvisionSpecified
		{
			set { this.extendibleProvisionSpecified = value; }
			get { return this.extendibleProvisionSpecified; }
		}
		IEmpty  IImplicitProvision.extendibleProvision
		{
			set { this.extendibleProvision = (Empty)value; }
			get { return this.extendibleProvision; }
		}
		bool  IImplicitProvision.stepUpProvisionSpecified
		{
			set { this.stepUpProvisionSpecified = value; }
			get { return this.stepUpProvisionSpecified; }
		}
		IEmpty  IImplicitProvision.stepUpProvision
		{
			set { this.stepUpProvision = (Empty)value; }
			get { return this.stepUpProvision; }
		}
		#endregion IImplicitProvision Members
	}
	#endregion ImplicitProvision

	#region KnownAmountSchedule
	public partial class KnownAmountSchedule : IKnownAmountSchedule
	{
		#region Constructors
		public KnownAmountSchedule(){}
		#endregion Constructors

		#region IKnownAmountSchedule Members

		bool IKnownAmountSchedule.notionalValueSpecified
		{
			get {return this.notionalValueSpecified; }
		}
		IMoney IKnownAmountSchedule.notionalValue
		{
			get {return this.notionalValue; }
		}
		bool IKnownAmountSchedule.dayCountFractionSpecified
		{
			get {return this.dayCountFractionSpecified; }
            set { this.dayCountFractionSpecified = value; }
		}
        DayCountFractionEnum IKnownAmountSchedule.dayCountFraction
        {
            get { return this.dayCountFraction; }
            set { this.dayCountFraction = value; }
        }
		#endregion IKnownAmountSchedule Members
	}
	#endregion KnownAmountSchedule

	#region LocalClassDerv
	public partial class LocalClassDerv : ICloneable,IScheme
	{
		#region Constructors
		public LocalClassDerv()
		{
			localClassDervScheme = Cst.OTCmL_LocalClassDervScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			LocalClassDerv clone = new LocalClassDerv();
			clone.localClassDervScheme = this.localClassDervScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.localClassDervScheme; }
			set { this.localClassDervScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion LocalClassDerv
	#region LocalClassNDrv
	public partial class LocalClassNDrv : ICloneable, IScheme
	{
		#region Constructors
		public LocalClassNDrv()
		{
			localClassNDrvScheme = Cst.OTCmL_LocalClassNDrvScheme;
		}
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			LocalClassNDrv clone = new LocalClassNDrv();
			clone.localClassNDrvScheme = this.localClassNDrvScheme;
			clone.Value = this.Value;
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IScheme Members
		string IScheme.scheme
		{
			get { return this.localClassNDrvScheme; }
			set { this.localClassNDrvScheme = value; }
		}
		string IScheme.Value
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
		#endregion IScheme Members
	}
	#endregion LocalClassNDrv

	#region NettingDesignation
	public partial class NettingDesignation : INettingDesignation
	{
		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int OTCmlId
		{
			get { return Convert.ToInt32(otcmlId); }
			set { otcmlId = value.ToString(); }
		}
		#endregion Accessors
		#region Constructors
		public NettingDesignation() { }
		#endregion Constructors

		#region INettingDesignation Members
		string INettingDesignation.Value
		{
			get {return this.Value;}
			set {this.Value = value;}
		}
		int INettingDesignation.OTCmlId
		{
			get { return this.OTCmlId; }
			set { this.OTCmlId = value; }
		}
		#endregion INettingDesignation Members
	}
	#endregion NettingDesignation
	#region NettingInformationInput
	public partial class NettingInformationInput : INettingInformationInput
	{
		#region Constructor
		public NettingInformationInput()
		{
			nettingMethod = NettingMethodEnum.Standard;
		}
		#endregion Constructor

		#region INettingInformationInput Members
		bool INettingInformationInput.nettingDesignationSpecified
		{
			set { this.nettingDesignationSpecified = value; }
			get { return this.nettingDesignationSpecified; }
		}
		INettingDesignation INettingInformationInput.nettingDesignation
		{
			get { return this.nettingDesignation; }
			set { this.nettingDesignation = (NettingDesignation)value; }
		}
		NettingMethodEnum INettingInformationInput.nettingMethod
		{
			get { return this.nettingMethod; }
			set { this.nettingMethod = value; }
		}
		#endregion INettingInformationInput Members

	}
	#endregion NettingInformationInput


    #region PartyPayerReceiverReference
    public partial class PartyPayerReceiverReference : IPartyPayerReceiverReference,IEFS_Array
    {
        #region constructor
        public PartyPayerReceiverReference()
        {
            partyReferencePayer = new PartyReference();
            partyReferenceReceiver = new PartyReference();
        }
        #endregion constructor
        #region IPartyPayerReceiverReference Members
        bool IPartyPayerReceiverReference.payerPartyReferenceSpecified
        {
            get { return this.partyReferencePayerSpecified; }
            set { this.partyReferencePayerSpecified = value; }
        }
        IReference IPartyPayerReceiverReference.payerPartyReference
        {
            get { return this.partyReferencePayer; }
            set { this.partyReferencePayer = (PartyReference)value; }
        }
        bool IPartyPayerReceiverReference.receiverPartyReferenceSpecified
        {
            get { return this.partyReferenceReceiverSpecified; }
            set { this.partyReferenceReceiverSpecified = value; }
        }
        IReference IPartyPayerReceiverReference.receiverPartyReference
        {
            get { return this.partyReferenceReceiver; }
            set { this.partyReferenceReceiver = (PartyReference)value; }
        }
        #endregion
        #region Membres de IEFS_Array
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion Membres de IEFS_Array
    }
    #endregion PartyPayerReceiverReference
    #region PaymentQuote
    public partial class PaymentQuote : IPaymentQuote
	{
		#region Constructors
		public PaymentQuote(){}
		#endregion Constructors
		#region Methods
		#region InitializePercentageRateFromPercentageRateFraction
		public void InitializePercentageRateFromPercentageRateFraction()
		{
			percentageRate.Value = "0";
			if (percentageRateFractionSpecified)
			{
				Fraction fraction = new Fraction(percentageRateFraction.Value, true);
				percentageRate.Value = StrFunc.FmtDecimalToInvariantCulture(fraction.DecValue());
			}
		}
		#endregion
		#endregion Methods

		#region IPaymentQuote Members
		bool IPaymentQuote.percentageRateFractionSpecified
		{
			set { this.percentageRateFractionSpecified = value; }
			get { return this.percentageRateFractionSpecified; }
		}
		string IPaymentQuote.percentageRateFraction
		{
			set { this.percentageRateFraction = new EFS_String(value); }
			get 
			{
				if (this.percentageRateFractionSpecified)
					return this.percentageRateFraction.Value;
				else
					return null;
			}
		}

		EFS_Decimal IPaymentQuote.percentageRate
		{
			set { this.percentageRate = value; }
			get { return this.percentageRate; }
		}

		IReference IPaymentQuote.paymentRelativeTo
		{
			set { this.paymentRelativeTo = (Reference)value; }
			get { return this.paymentRelativeTo; }
		}
		void IPaymentQuote.InitializePercentageRateFromPercentageRateFraction()
		{ 
			this.InitializePercentageRateFromPercentageRateFraction();
		}
		#endregion IPaymentQuote Members
	}
	#endregion PaymentQuote
	#region PayoutPeriod
	public partial class PayoutPeriod : IPayoutPeriod
	{
		#region Constructors
		public PayoutPeriod() { }
		#endregion Constructors

		#region IPayoutPeriod Members
		PeriodEnum IPayoutPeriod.period { get { return this.period; } }
		int IPayoutPeriod.periodMultiplier { get { return this.periodMultiplier.IntValue; } }
		decimal IPayoutPeriod.percentage { get { return this.percentage.DecValue; } }
		#endregion IPayoutPeriod Members

	}
	#endregion PayoutPeriod

    #region RoutingCreateElement
    public partial class RoutingCreateElement : IRoutingCreateElement
    {
        #region IRoutingCreateElement Membres
        IRouting IRoutingCreateElement.CreateRouting()
        {
            return new Routing();
        }
        IRoutingIds[] IRoutingCreateElement.CreateRoutingIds(IRoutingId[] pRoutingId)
        {
            RoutingIds[] routingIds = new RoutingIds[1] { new RoutingIds() };
            RoutingId[] routingId = new RoutingId[pRoutingId.Length];
            for (int i = 0; i < pRoutingId.Length; i++)
            {
                routingId[i] = (RoutingId)pRoutingId[i]; 
                routingId[i] = new RoutingId();
                routingId[i].routingIdCodeScheme = pRoutingId[i].routingIdCodeScheme;
                routingId[i].Value = pRoutingId[i].Value;
            }
            routingIds[0].routingId = routingId;
            return routingIds;
        }
        IRoutingIdsAndExplicitDetails IRoutingCreateElement.CreateRoutingIdsAndExplicitDetails()
        {
            return new RoutingIdsAndExplicitDetails();
        }
        IRoutingId IRoutingCreateElement.CreateRoutingId()
        {
            return new RoutingId();
        }
        IAddress IRoutingCreateElement.CreateAddress()
        {
            return new Address();
        }
        IStreetAddress IRoutingCreateElement.CreateStreetAddress()
        {
            return new StreetAddress();
        }
        IScheme IRoutingCreateElement.CreateCountry()
        {
            return new Country();
        }
        #endregion
    }
    #endregion 
    #region RoutingPartyReference
    public partial class RoutingPartyReference : IRoutingPartyReference
    {
        #region IRoutingPartyReference Membres
        bool IRoutingPartyReference.hRefSpecified
        {
            get { return this.hrefSpecified; }
            set { this.hrefSpecified = value; }
        }
        string IRoutingPartyReference.hRef
        {
            get { return this.href; }
            set { this.href = value; }
        }
        #endregion
    }
    #endregion


    #region SettlementInformationInput
    public partial class SettlementInformationInput : IEFS_Array, ICloneable, ISettlementInformationInput
	{
		#region Constructors
		public SettlementInformationInput()
		{
			partyReferencePayer		= new PartyReference();
			partyReferenceReceiver	= new PartyReference();
			settlementInformation	= new EfsSettlementInformation();
		}
		#endregion Constructors

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			SettlementInformationInput clone = new SettlementInformationInput();
			clone = (SettlementInformationInput)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region ISettlementInformationInput Members
		bool ISettlementInformationInput.partyReferencePayerSpecified
		{
			get { return this.partyReferencePayerSpecified; }
		}
		IReference ISettlementInformationInput.partyReferencePayer
		{
			get { return this.partyReferencePayer; }
		}
		bool ISettlementInformationInput.partyReferenceReceiverSpecified
		{
			get { return this.partyReferenceReceiverSpecified; }
		}
		IReference ISettlementInformationInput.partyReferenceReceiver
		{
			get { return this.partyReferenceReceiver; }
		}
		IEfsSettlementInformation ISettlementInformationInput.settlementInformation
		{
			get { return this.settlementInformation; }
		}
		bool ISettlementInformationInput.eventCodesScheduleSpecified
		{
			get { return this.eventCodesScheduleSpecified; }
		}
		IEventCodesSchedule ISettlementInformationInput.eventCodesSchedule
		{
			get { return this.eventCodesSchedule; }
		}
		bool ISettlementInformationInput.cssCriteriaSpecified
		{
			get { return this.cssCriteriaSpecified; }
		}
		ICssCriteria ISettlementInformationInput.cssCriteria
		{
			get { return this.cssCriteria; }
		}
		bool ISettlementInformationInput.ssiCriteriaSpecified
		{
			get { return this.ssiCriteriaSpecified; }
		}
		ISsiCriteria ISettlementInformationInput.ssiCriteria
		{
			get { return this.ssiCriteria; }
		}
		#endregion ISettlementInformationInput Members
	}
	#endregion SettlementInformationInput
	#region SettlementInput
	public partial class SettlementInput : IEFS_Array, ICloneable, ISettlementInput
	{
		#region Constructors
		public SettlementInput()
		{
			settlementContext = new FlowContext();
			settlementInputInfo = new SettlementInputInfo();
		}
		#endregion Constructors
        //
		#region Methods
		#endregion Methods
        //
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
				return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
			else
				return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
        //
		#region ICloneable Members
		#region Clone
		public object Clone()
		{
			SettlementInput clone = new SettlementInput();
			clone = (SettlementInput)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
			return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
        //
		#region ISettlementInput Members
		IFlowContext ISettlementInput.settlementContext
		{
			get { return this.settlementContext; }
		}
		ISettlementInputInfo ISettlementInput.settlementInputInfo
		{
			get { return this.settlementInputInfo; }
		}
		object ISettlementInput.Clone()
		{
			return this.Clone();
		}
		#endregion ISettlementInput Members
	}
	#endregion SettlementInput
	#region SettlementInputInfo
	public partial class SettlementInputInfo : ISettlementInputInfo
	{
		#region Constructors
		public SettlementInputInfo()
		{
			settlementInformation = new EfsSettlementInformation();
		}
		#endregion Constructors

		#region ISettlementInputInfo Members
		IEfsSettlementInformation ISettlementInputInfo.settlementInformation
		{
			get { return this.settlementInformation; }
		}
		bool ISettlementInputInfo.cssCriteriaSpecified
		{
			get {return this.cssCriteriaSpecified;}
			set {this.cssCriteriaSpecified = value;}
		}
		ICssCriteria ISettlementInputInfo.cssCriteria
		{
			get { return this.cssCriteria; }
		}
		bool ISettlementInputInfo.ssiCriteriaSpecified
		{
			get { return this.ssiCriteriaSpecified;}
			set {this.ssiCriteriaSpecified = value;}
		}
		ISsiCriteria ISettlementInputInfo.ssiCriteria
		{
			get { return this.ssiCriteria; }
		}
		IEfsSettlementInstruction[] ISettlementInputInfo.CreateEfsSettlementInstructions()
		{
			return new EfsSettlementInstruction[] { new EfsSettlementInstruction() };
		}
		IEfsSettlementInstruction[] ISettlementInputInfo.CreateEfsSettlementInstructions(IEfsSettlementInstruction pEfsSettlementInstruction)
		{
			return new EfsSettlementInstruction[] { (EfsSettlementInstruction)pEfsSettlementInstruction };
		}
		#endregion ISettlementInputInfo Members
	}
	#endregion SettlementInputInfo
	#region SsiCriteria
	public partial class SsiCriteria : ISsiCriteria
	{
		#region Constructors
		public SsiCriteria()
		{
			ssiCountry = new Country();
		}
		public SsiCriteria(ISsiCriteria pSsiCriteria)
		{
			if (null != pSsiCriteria)
			{
                ssiCountry = new Country();
                ssicountrySpecified = pSsiCriteria.countrySpecified;
				ssiCountry.Value = pSsiCriteria.country.Value;
				ssiCountry.countryScheme = pSsiCriteria.country.scheme;
			}
		}
		#endregion Constructors

		#region ISsiCriteria Membres
		bool ISsiCriteria.countrySpecified
		{
			get {return this.ssicountrySpecified;}
			set {this.ssicountrySpecified = value;}
		}
		IScheme ISsiCriteria.country
		{
			get {return this.ssiCountry;}
			set	{this.ssiCountry = (Country)value;}
		}
		#endregion ISsiCriteria Membres
	}
	#endregion SsiCriteria

    #region SoftApplication
    public partial class SoftApplication : ISoftApplication
    {
        #region constructor
        public SoftApplication()
        {
            this.name = new EFS_String();
            this.version = new EFS_String();
        }
        #endregion

        #region ISoftApplication Membres
        EFS_String ISoftApplication.name
        {
            get { return this.name; }
            set { name = value; }
        }
        EFS_String ISoftApplication.version
        {
            get { return this.version; }
            set { version = value; }
        }
        #endregion
    }
	#endregion

	#region SpheresId
	/// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
	public partial class SpheresId : ISpheresIdSchemeId, IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public SpheresId()
        {
            scheme = string.Empty;
            Value = string.Empty;
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region ISpheresId Members
        string ISpheresId.otcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion ISpheresId Members
        #region ISchemeId Members
        string ISchemeId.id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.scheme
        {
            set { this.scheme = value; }
            get { return this.scheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        public string source 
		{
			get;
		    set;
		}
        #endregion IScheme Members
    }
    #endregion SpheresId
    #region TradeExtend
    public partial class TradeExtend : ITradeExtend, IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public TradeExtend()
        {
            scheme = string.Empty;
            Value = string.Empty;
            //hRefSpecified = false;
        }
        #endregion Constructors

        #region ITradeExtend Members
        bool ITradeExtend.hRefSpecified
        {
            set { this.hRefSpecified = value; }
            get { return this.hRefSpecified; }
        }
        string ITradeExtend.hRef
        {
            set { this.hRef = value; }
            get { return this.hRef; }
        }
        #endregion IScheme Members

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region ISpheresId Members
        string ISpheresId.otcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion ISpheresId Members
        #region IScheme Members
        string IScheme.scheme
        {
            set { this.scheme = value; }
            get { return this.scheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion TradeExtend
    #region SpheresSource
    public partial class SpheresSource : ISpheresSource
    {
        #region Constructors
        public SpheresSource()
        {
        }
        #endregion Constructors

        #region ISpheresSource Members
        bool ISpheresSource.statusSpecified
        {
            get { return this.statusSpecified; }
            set { this.statusSpecified = value; }
        }
        SpheresSourceStatusEnum ISpheresSource.status
        {
            get { return this.status; }
            set { this.status = value; }
        }
        ISpheresIdSchemeId[] ISpheresSource.spheresId
        {
            get { return this.spheresId; }
            set { this.spheresId = (SpheresId[])value; }
        }
        ISpheresIdSchemeId ISpheresSource.GetSpheresIdFromScheme(string pScheme)
        {
            return (ISpheresIdSchemeId)Tools.GetScheme(this.spheresId, pScheme);
        }
        ISpheresIdSchemeId[] ISpheresSource.GetSpheresIdLikeScheme(string pScheme)
        {
            return (ISpheresIdSchemeId[])Tools.GetLikeScheme(this.spheresId, pScheme);
        }
        ISpheresIdSchemeId ISpheresSource.GetSpheresIdFromSchemeId(string pSchemeId)
        {
            return (ISpheresIdSchemeId)Tools.GetSchemeById(this.spheresId, pSchemeId);
        }
        #endregion ISpheresSource Members
    }
    #endregion SpheresSource
	#region StepUpProvision
	public partial class StepUpProvision : IStepUpProvision
	{
		#region Accessors
		#region EFS_Exercise
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object EFS_Exercise
		{
			get
			{
				if (stepupExerciseAmericanSpecified)
					return stepupExerciseAmerican;
				else if (stepupExerciseBermudaSpecified)
					return stepupExerciseBermuda;
				else if (stepupExerciseEuropeanSpecified)
					return stepupExerciseEuropean;
				else
					return null;
			}
		}
		#endregion EFS_Exercise
		#endregion Accessors
		#region Constructors
		public StepUpProvision()
		{
			stepupExerciseAmerican = new AmericanExercise();
			stepupExerciseBermuda = new BermudaExercise();
			stepupExerciseEuropean = new EuropeanExercise();
		}
		#endregion Constructors

		#region IProvision Members
		ExerciseStyleEnum IProvision.GetStyle
		{
			get
			{
				if (this.stepupExerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.stepupExerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;

				return ExerciseStyleEnum.European;
			}
		}
		ICashSettlement IProvision.cashSettlement 
		{
			set { ;}
			get { return null; } 
		}
		#endregion IProvision Members
		#region IStepUpProvision Members
		EFS_ExerciseDates IStepUpProvision.efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion IStepUpProvision Members
	}
	#endregion StepUpProvision

}
#region using directives
using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#endregion using directives

namespace EfsML.v30.Doc
{

    #region ActorId
    public partial class ActorId : IActorId
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get
            {
                if (StrFunc.IsFilled(otcmlId))
                    return Convert.ToInt32(otcmlId);
                return 0;
            }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public ActorId()
        {
            actorIdScheme = "http://www.euro-finance-systems.fr/otcml/actorid";
        }
        #endregion Constructors

        #region ISpheresId
        string ISpheresId.OtcmlId
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
        bool IActorId.ActorNameSpecified
        {
            get { return this.actorNameSpecified; }
            set { this.actorNameSpecified = value; }
        }
        string IActorId.ActorName
        {
            get { return this.actorName; }
            set
            {
                this.actorName = value;
                this.actorNameSpecified = (this.actorName != null);
            }
        }
        #endregion IBookId Members
        #region IScheme Members
        string IScheme.Scheme
        {
            get { return this.actorIdScheme; }
            set { this.actorIdScheme = value; }
        }
        string IScheme.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }
        #endregion IScheme Members

    }
    #endregion ActorId
    
    #region BookId
    public partial class BookId : IBookId
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get
            {
                if (StrFunc.IsFilled(otcmlId))
                    return Convert.ToInt32(otcmlId);
                return 0;
            }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public BookId()
        {
            bookIdScheme = "http://www.euro-finance-systems.fr/otcml/bookid";
        }
        #endregion Constructors

        #region ISpheresId
        string ISpheresId.OtcmlId
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
        bool IBookId.BookNameSpecified
        {
            get { return this.bookNameSpecified; }
            set { this.bookNameSpecified = value; }
        }
        string IBookId.BookName
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
        string IScheme.Scheme
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

    #region Css
    public partial class Css : ICss
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get
            {
                if (StrFunc.IsFilled(otcmlId))
                    return Convert.ToInt32(otcmlId);
                return 0;
            }
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
        string ICss.OtcmlId
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
                cssCriteriaCssSpecified = pCssCriteria.CssSpecified;
                cssCriteriaCss = (Css)pCssCriteria.Css;
                //
                cssCriteriaCssInfoSpecified = pCssCriteria.CssInfoSpecified;
                cssCriteriaCssInfo = (CssInfo)pCssCriteria.CssInfo;
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
                string cssContry = cssCriteriaCssInfo.cssCountrySpecified ? cssCriteriaCssInfo.cssCountry.Value : "N/A";
                string cssPaymentType = cssCriteriaCssInfo.cssPaymentTypeSpecified ? cssCriteriaCssInfo.cssPaymentType.Value : "N/A";
                string cssSettlementType = cssCriteriaCssInfo.cssSettlementTypeSpecified ? cssCriteriaCssInfo.cssSettlementType.Value : "N/A";
                string cssSystemType = cssCriteriaCssInfo.cssSystemTypeSpecified ? cssCriteriaCssInfo.cssSystemType.Value : "N/A";
                string cssType = cssCriteriaCssInfo.cssTypeSpecified ? cssCriteriaCssInfo.cssType.Value : "N/A";
                //	
                sb.AppendFormat(ret, "cssContry : {0},cssPaymentType :{1},cssSettlementType :{2},cssSystemType :{3},cssType :{4}",
                    cssContry, cssPaymentType, cssSettlementType, cssSystemType, cssType);
            }
            ret = sb.ToString();
            //
            return ret;
        }
        #endregion ToString
        #endregion Methods

        #region ICssCriteria Members
        bool ICssCriteria.CssSpecified
        {
            get { return this.cssCriteriaCssSpecified; }
            set { this.cssCriteriaCssSpecified = value; }
        }
        ICss ICssCriteria.Css
        {
            get { return this.cssCriteriaCss; }
            set { this.cssCriteriaCss = (Css)value; }
        }
        bool ICssCriteria.CssInfoSpecified
        {
            get { return this.cssCriteriaCssInfoSpecified; }
            set { this.cssCriteriaCssInfoSpecified = value; }
        }
        ICssInfo ICssCriteria.CssInfo
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
        bool ICssInfo.CountrySpecified
        {
            get { return this.cssCountrySpecified; }
        }
        IScheme ICssInfo.Country
        {
            get { return this.cssCountry; }
        }
        bool ICssInfo.TypeSpecified
        {
            get { return this.cssTypeSpecified; }
        }
        IScheme ICssInfo.Type
        {
            get { return this.cssType; }
        }
        bool ICssInfo.SettlementTypeSpecified
        {
            get { return this.cssSettlementTypeSpecified; }
        }
        IScheme ICssInfo.SettlementType
        {
            get { return this.cssSettlementType; }
        }
        bool ICssInfo.PaymentTypeSpecified
        {
            get { return this.cssPaymentTypeSpecified; }
        }
        IScheme ICssInfo.PaymentType
        {
            get { return this.cssPaymentType; }
        }
        bool ICssInfo.SystemTypeSpecified
        {
            get { return this.cssSystemTypeSpecified; }
        }
        IScheme ICssInfo.SystemType
        {
            get { return this.cssSystemType; }
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
        string IScheme.Scheme
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
        string IScheme.Scheme
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
        string IScheme.Scheme
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
        string IScheme.Scheme
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

    


    #region EfsDocument
    public partial class EfsDocument : IEfsDocument
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance", DataType = "anyURI")]
        public string SchemaLocation
        {
            set { efs_schemaLocation = "http://www.efs.org/2007/EFSmL-3-0 EFSmL-3-0.xsd"; }
            get { return efs_schemaLocation; }
        }
        #endregion Accessors

        #region Constructors
        public EfsDocument()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version30;
            SchemaLocation = string.Empty;
        }
        #endregion Constructors

        #region IEfsDocument Members
        EfsMLDocumentVersionEnum IEfsDocument.EfsMLversion
        {
            get { return this.EfsMLversion; }
            set { this.EfsMLversion = value; }
        }
        #endregion IEfsDocument Members

        #region IRepositoryDocument Members
        IRepository IRepositoryDocument.Repository
        {
            get { return this.repository; }
            set { this.repository = (EfsML.v30.Repository.Repository)value; }
        }
        bool IRepositoryDocument.RepositorySpecified
        {
            set { this.repositorySpecified = value; }
            get { return this.repositorySpecified; }
        }

        IRepository IRepositoryDocument.CreateRepository()
        {
            return (IRepository)new EfsML.v30.Repository.Repository();
        }

        /// <summary>
        /// Retourne tous les assets présents dans le repository
        /// <para></para>
        /// </summary>
        /// <returns></returns>
        /// FI 20150708 [XXXXX] Add
        /// FI 20151019 [21317] Modify 
        /// FI 20170116 [21916] Modify
        List<IAssetRepository> IRepositoryDocument.GetAllRepositoryAsset()
        {
            List<IAssetRepository> ret = new List<IAssetRepository>();
            if (repositorySpecified)
            {
                ret =
                 ((from asset in repository.assetEquity
                   select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetIndex
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetRateIndex
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetETD
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetFxRate
                          select (IAssetRepository)asset)
                   .Concat(from asset in repository.assetDebtSecurity // FI 20151019 [21317] Add
                           select (IAssetRepository)asset)
                   .Concat(from asset in repository.assetCommodity // FI 20170116 [21916] Add
                           select (IAssetRepository)asset)).ToList();
            }
            return ret;
        }

        #endregion IRepositoryDocument Members

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
        bool IEfsSettlementInformation.StandardSpecified
        {
            set { this.informationStandardSpecified = value; }
            get { return this.informationStandardSpecified; }
        }
        StandardSettlementStyleEnum IEfsSettlementInformation.Standard
        {
            get { return this.informationStandard; }
        }
        bool IEfsSettlementInformation.InstructionSpecified
        {
            set { this.informationInstructionSpecified = value; }
            get { return this.informationInstructionSpecified; }
        }

        IEfsSettlementInstruction IEfsSettlementInformation.Instruction
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
        bool IEfsSettlementInstruction.SettlementMethodSpecified
        {
            set { this.settlementMethodSpecified = value; }
            get { return this.settlementMethodSpecified; }
        }
        IScheme IEfsSettlementInstruction.SettlementMethod
        {
            set { this.settlementMethod = (SettlementMethod)value; }
            get { return (IScheme)this.settlementMethod; }
        }
        bool IEfsSettlementInstruction.BeneficiaryBankSpecified
        {
            set { this.beneficiaryBankSpecified = value; }
            get { return this.beneficiaryBankSpecified; }
        }
        IRouting IEfsSettlementInstruction.BeneficiaryBank
        {
            set { this.beneficiaryBank = (Routing)value; }
            get { return (IRouting)this.beneficiaryBank; }
        }
        IRouting IEfsSettlementInstruction.Beneficiary
        {
            set { this.beneficiary = (Beneficiary)value; }
            get { return (IRouting)this.beneficiary; }
        }
        bool IEfsSettlementInstruction.CorrespondentInformationSpecified
        {
            set { this.correspondentInformationSpecified = value; }
            get { return this.correspondentInformationSpecified; }
        }
        IRouting IEfsSettlementInstruction.CorrespondentInformation
        {
            set { this.correspondentInformation = (CorrespondentInformation)value; }
            get { return (IRouting)this.correspondentInformation; }
        }
        bool IEfsSettlementInstruction.IntermediaryInformationSpecified
        {
            set { this.intermediaryInformationSpecified = value; }
            get { return this.intermediaryInformationSpecified; }
        }
        IIntermediaryInformation[] IEfsSettlementInstruction.IntermediaryInformation
        {
            set { this.intermediaryInformation = (IntermediaryInformation[])value; }
            get { return this.intermediaryInformation; }
        }
        bool IEfsSettlementInstruction.SettlementMethodInformationSpecified
        {
            set { this.settlementMethodInformationSpecified = value; }
            get { return this.settlementMethodInformationSpecified; }
        }
        IRouting IEfsSettlementInstruction.SettlementMethodInformation
        {
            set { this.settlementMethodInformation = (Routing)value; }
            get { return (IRouting)this.settlementMethodInformation; }
        }
        bool IEfsSettlementInstruction.InvestorInformationSpecified
        {
            set { this.investorInformationSpecified = value; }
            get { return this.investorInformationSpecified; }
        }
        IRouting IEfsSettlementInstruction.InvestorInformation
        {
            set { this.investorInformation = (Routing)value; }
            get { return this.investorInformation; }
        }

        bool IEfsSettlementInstruction.OriginatorInformationSpecified
        {
            set { this.originatorInformationSpecified = value; }
            get { return this.originatorInformationSpecified; }
        }

        IRouting IEfsSettlementInstruction.OriginatorInformation
        {
            set { this.originatorInformation = (Routing)value; }
            get { return this.originatorInformation; }
        }
        int IEfsSettlementInstruction.IdssiDb
        {
            set { this.idssiDb = value; }
            get { return this.idssiDb; }
        }
        int IEfsSettlementInstruction.IdIssi
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
            return (IRouting)new CorrespondentInformation();
        }

        IIntermediaryInformation IEfsSettlementInstruction.CreateIntermediaryInformation()
        {
            return (IIntermediaryInformation)new IntermediaryInformation();
        }

        IRouting IEfsSettlementInstruction.CreateBeneficiary()
        {
            return (IRouting)new Beneficiary();
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
        string IScheme.Scheme
        {
            set { this.eventCodeScheme = value; }
            get { return this.eventCodeScheme; }
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
        public int CompareTo(object pObj)
        {
            int ret = 0;
            //
            // Date      + fin que EventType
            // EventType + fin que EventCode
            // EventCode + fin que StreamId
            // StreamId  + fin que ProductReference
            //
            if (pObj is EventCodes eventCodes)
            {
                //Date
                if (settlementDateSpecified && !eventCodes.settlementDateSpecified)
                    ret = 5;
                if (!settlementDateSpecified && eventCodes.settlementDateSpecified)
                    ret = -5;
                //EventType
                if (0 == ret)
                {
                    if (eventTypeSpecified && !eventCodes.eventTypeSpecified)
                        ret = 4;
                    else if (!eventTypeSpecified && eventCodes.eventTypeSpecified)
                        ret = -4;
                }
                //EventCode
                if (0 == ret)
                {
                    if (eventCodeSpecified && !eventCodes.eventCodeSpecified)
                        ret = 3;
                    else if (!eventCodeSpecified && eventCodes.eventCodeSpecified)
                        ret = -3;
                }
                //Stream
                if (0 == ret)
                {
                    if (StreamIdSpecified && !eventCodes.StreamIdSpecified)
                        ret = 2;
                    else if (!StreamIdSpecified && eventCodes.StreamIdSpecified)
                        ret = -2;
                }
                //Instr
                if (0 == ret)
                {
                    if (productReferenceSpecified && !eventCodes.productReferenceSpecified)
                        ret = 1;
                    else if (!productReferenceSpecified && eventCodes.productReferenceSpecified)
                        ret = -1;
                }
            }
            return ret;
        }
        #endregion CompareTo
        #endregion IComparable Members
        //
        #region IEventCodes Members
        bool IEventCodes.ProductReferenceSpecified
        {
            get { return this.productReferenceSpecified; }
        }
        IReference IEventCodes.ProductReference
        {
            get { return this.productReference; }
        }
        bool IEventCodes.StreamIdSpecified
        {
            get { return this.StreamIdSpecified; }
        }
        EFS_NonNegativeInteger IEventCodes.StreamId
        {
            get { return this.streamId; }
        }
        bool IEventCodes.EventCodeSpecified
        {
            get { return this.eventCodeSpecified; }
        }
        IScheme IEventCodes.EventCode
        {
            get { return this.eventCode; }
        }
        bool IEventCodes.EventTypeSpecified
        {
            get { return this.eventTypeSpecified; }
        }
        IScheme IEventCodes.EventType
        {
            get { return this.eventType; }
        }
        bool IEventCodes.SettlementDateSpecified
        {
            get { return this.settlementDateSpecified; }
        }
        EFS_Date IEventCodes.SettlementDate
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
        IEventCodes[] IEventCodesSchedule.EventCodes
        {
            get { return this.eventCodes; }
        }
        #endregion IEventCodesSchedule Members

        #region IComparable Members
        #region CompareTo
        public int CompareTo(object pObj)
        {
            int ret = 0;
            int minValue = 0;
            int maxValue = 0;
            //
            if (pObj is EventCodesSchedule eventCodesSchedule)
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

    #region FlowContext
    public partial class FlowContext : IComparable, IFlowContext
    {
        #region Constructors
        public FlowContext() { }
        #endregion Constructors
        //
        #region IComparable Members
        #region CompareTo
        public int CompareTo(object pObj)
        {
            int ret = 0;

            if (pObj is FlowContext flowContext)
            {
                if (eventCodesScheduleSpecified && (false == flowContext.eventCodesScheduleSpecified))
                    ret = 1;
                else if (!eventCodesScheduleSpecified && (flowContext.eventCodesScheduleSpecified))
                    ret = -1;

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
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "pObj is not a FlowContext");
            }
            return ret;
        }

        #endregion CompareTo
        #endregion IComparable Members
        //
        #region IFlowContext Members
        bool IFlowContext.CurrencySpecified
        {
            get { return this.currencySpecified; }
            set { this.currencySpecified = value; }
        }
        string IFlowContext.Currency
        {
            get { return this.currency.Value; }
            set { this.currency.Value = value; }
        }
        //
        bool IFlowContext.PartyContextSpecified
        {
            get { return this.partyContextSpecified; }
            set { this.partyContextSpecified = value; }
        }
        IPartyPayerReceiverReference[] IFlowContext.PartyContext
        {
            get { return this.partyContext; }
            set { this.partyContext = (PartyPayerReceiverReference[])value; }
        }
        //
        bool IFlowContext.EventCodesScheduleSpecified
        {
            get
            {
                return this.eventCodesScheduleSpecified;
            }
            set { this.eventCodesScheduleSpecified = value; }
        }
        IEventCodesSchedule IFlowContext.EventCodesSchedule
        {
            get { return this.eventCodesSchedule; }
            set { this.eventCodesSchedule = (EventCodesSchedule)value; }
        }
        //
        bool IFlowContext.CashSecuritiesSpecified
        {
            get { return this.cashSecuritiesSpecified; }
            set { this.cashSecuritiesSpecified = value; }
        }
        CashSecuritiesEnum IFlowContext.CashSecurities
        {
            get { return this.cashSecurities; }
            set { this.cashSecurities = value; }
        }
        //
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
            FxClass clone = new FxClass
            {
                fxClassScheme = this.fxClassScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
            HedgeClassDerv clone = new HedgeClassDerv
            {
                hedgeClassDervScheme = this.hedgeClassDervScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
            HedgeClassNDrv clone = new HedgeClassNDrv
            {
                hedgeClassNDrvScheme = this.hedgeClassNDrvScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
            IASClassDerv clone = new IASClassDerv
            {
                iasClassDervScheme = this.iasClassDervScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
            IASClassNDrv clone = new IASClassNDrv
            {
                iasClassNDrvScheme = this.iasClassNDrvScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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

    #region LocalClassDerv
    public partial class LocalClassDerv : ICloneable, IScheme
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
            LocalClassDerv clone = new LocalClassDerv
            {
                localClassDervScheme = this.localClassDervScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
            LocalClassNDrv clone = new LocalClassNDrv
            {
                localClassNDrvScheme = this.localClassNDrvScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region IScheme Members
        string IScheme.Scheme
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
            get
            {
                if (StrFunc.IsFilled(otcmlId))
                    return Convert.ToInt32(otcmlId);
                return 0;
            }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public NettingDesignation() { }
        #endregion Constructors

        #region INettingDesignation Members
        string INettingDesignation.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
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
        #region Constructors
        public NettingInformationInput()
        {
            nettingMethod = NettingMethodEnum.Standard;
        }
        #endregion Constructors

        #region INettingInformationInput Members
        bool INettingInformationInput.NettingDesignationSpecified
        {
            set { this.nettingDesignationSpecified = value; }
            get { return this.nettingDesignationSpecified; }
        }
        INettingDesignation INettingInformationInput.NettingDesignation
        {
            get { return this.nettingDesignation; }
            set { this.nettingDesignation = (NettingDesignation)value; }
        }
        NettingMethodEnum INettingInformationInput.NettingMethod
        {
            get { return this.nettingMethod; }
            set { this.nettingMethod = value; }
        }
        #endregion INettingInformationInput Members
    }
    #endregion NettingInformationInput

    #region SettlementInformationInput
    public partial class SettlementInformationInput : IEFS_Array, ICloneable, ISettlementInformationInput
    {
        #region Constructors
        public SettlementInformationInput()
        {
            partyReferencePayer = new PartyReference();
            partyReferenceReceiver = new PartyReference();
            settlementInformation = new EfsSettlementInformation();
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
            SettlementInformationInput clone = (SettlementInformationInput)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneFieldAndProperty);
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region ISettlementInformationInput Members
        bool ISettlementInformationInput.PartyReferencePayerSpecified
        {
            get { return this.partyReferencePayerSpecified; }
        }
        IReference ISettlementInformationInput.PartyReferencePayer
        {
            get { return this.partyReferencePayer; }
        }
        bool ISettlementInformationInput.PartyReferenceReceiverSpecified
        {
            get { return this.partyReferenceReceiverSpecified; }
        }
        IReference ISettlementInformationInput.PartyReferenceReceiver
        {
            get { return this.partyReferenceReceiver; }
        }
        IEfsSettlementInformation ISettlementInformationInput.SettlementInformation
        {
            get { return this.settlementInformation; }
        }
        bool ISettlementInformationInput.EventCodesScheduleSpecified
        {
            get { return this.eventCodesScheduleSpecified; }
        }
        IEventCodesSchedule ISettlementInformationInput.EventCodesSchedule
        {
            get { return this.eventCodesSchedule; }
        }
        bool ISettlementInformationInput.CssCriteriaSpecified
        {
            get { return this.cssCriteriaSpecified; }
        }
        ICssCriteria ISettlementInformationInput.CssCriteria
        {
            get { return this.cssCriteria; }
        }
        bool ISettlementInformationInput.SsiCriteriaSpecified
        {
            get { return this.ssiCriteriaSpecified; }
        }
        ISsiCriteria ISettlementInformationInput.SsiCriteria
        {
            get { return this.ssiCriteria; }
        }
        #endregion ISettlementInformationInput Members
    }
    #endregion SettlementInformations
    #region SettlementInput
    /// <summary>
    /// Classs qui représente les instructions de settlement à appliquer dans un contexte donné 
    /// <para>Les instructions sont définies sous SettlementInputInfo</para>
    /// <para>Ces intructions s'appliquent dans le contexte décrit par FlowContext</para>
    /// </summary>
    public partial class SettlementInput : IEFS_Array, ICloneable, ISettlementInput
    {
        #region Constructors
        public SettlementInput()
        {
            settlementContext = new FlowContext();
            settlementInputInfo = new SettlementInputInfo();
        }
        #endregion Constructors
        #region Methods

        #endregion Methods

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
            SettlementInput clone = (SettlementInput)ReflectionTools.Clone(this, ReflectionTools.CloneStyle.CloneField);
            return clone;
        }
        #endregion Clone
        #endregion ICloneable Members
        #region ISettlementInput Members
        IFlowContext ISettlementInput.SettlementContext
        {
            get { return this.settlementContext; }
        }
        ISettlementInputInfo ISettlementInput.SettlementInputInfo
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
        IEfsSettlementInformation ISettlementInputInfo.SettlementInformation
        {
            get { return this.settlementInformation; }
        }
        bool ISettlementInputInfo.CssCriteriaSpecified
        {
            get { return this.cssCriteriaSpecified; }
            set { this.cssCriteriaSpecified = value; }
        }
        ICssCriteria ISettlementInputInfo.CssCriteria
        {
            get { return this.cssCriteria; }
        }
        bool ISettlementInputInfo.SsiCriteriaSpecified
        {
            get { return this.ssiCriteriaSpecified; }
            set { this.ssiCriteriaSpecified = value; }
        }
        ISsiCriteria ISettlementInputInfo.SsiCriteria
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
                ssicountrySpecified = pSsiCriteria.CountrySpecified;
                ssiCountry.Value = pSsiCriteria.Country.Value;
                ssiCountry.countryScheme = pSsiCriteria.Country.Scheme;
            }
        }
        #endregion Constructors

        #region ISsiCriteria Membres
        bool ISsiCriteria.CountrySpecified
        {
            get { return this.ssicountrySpecified; }
            set { this.ssicountrySpecified = value; }
        }
        IScheme ISsiCriteria.Country
        {
            get { return this.ssiCountry; }
            set { this.ssiCountry = (Country)value; }
        }
        #endregion ISsiCriteria Membres
    }
    #endregion SsiCriteria

    #region TradeIntention

    public partial class TradeIntention : ITradeIntention
    {
        #region Constructor
        public TradeIntention()
        {
            initiator = new ArrayPartyReference[1] { new ArrayPartyReference() };
            reactor = new PartyReference();
        }
        #endregion Constructor

        #region Accessors
        #region id
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }
        #endregion id
        #endregion Accessors


        #region ITradeIntention Members
        IReference[] ITradeIntention.Initiator
        {
            get { return this.initiator; }
            set { this.initiator = (ArrayPartyReference[])value; }
        }
        bool ITradeIntention.ReactorSpecified
        {
            get { return this.reactorSpecified; }
            set { this.reactorSpecified = value; }
        }
        IReference ITradeIntention.Reactor
        {
            get { return this.reactor; }
            set { this.reactor = (PartyReference)value; }
        }
        string ITradeIntention.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        #endregion ITradeIntention Members
    }
    #endregion TradeIntention
}
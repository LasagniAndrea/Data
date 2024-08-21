///////////////////////////////////////////////////
// List of the C# serializable classes corresponding to the EfsML-IMRLog.xsd types (excluding the container class MarginDetailsDocument).
// The root of the xsd correspond to the MarginRequirementOffice (serialized as "marginRequirementOffice")
///////////////////////////////////////////////////

using EFS.ACommon;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Doc;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Doc;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace EfsML.v30.MarginRequirement
{
    # region Margin Requirement 
    /// <summary>
    /// Calculation sheet container
    /// </summary>
    // EG 20140702 Upd Interface
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlRoot("marginRequirement", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public sealed class MarginDetailsDocument : IMarginDetailsDocument
    {

        bool m_Processed = false;

        /// <summary>
        /// Get the status of the log, returns true when the calculation sheet has been processed, adding the risk evaluation details
        /// </summary>
        /// <remarks>this initialization flag must not be serialized, it is useful just during the log initialization process</remarks>
        [XmlIgnore]
        public bool Processed
        {
            get { return m_Processed; }
            set { m_Processed = value; }
        }

        /// <summary>
        /// Get an empty risk evaluation log, without any risk evaluation result reference
        /// </summary>
        /// <remarks>
        /// the returned instance could not be processed, because the trade reference is missing
        /// </remarks>
        public MarginDetailsDocument() {}

        /// <summary>
        /// Get an empty risk evaluation connected to a risk evaluation result
        /// </summary>
        /// <param name="pIdTechTrade">internal id of the trade</param>
        /// <param name="pTechTrade">the risk evaluation result reference</param>
        /// <param name="parties">the risk evaluation result main parties</param>
        // EG 20180205 [23769] Add IdentifierTechTrade
        public MarginDetailsDocument(int pIdTechTrade, string pIdentifierTechTrade, ITrade pTechTrade, IParty[] parties)
        {
            this.party = parties as Party[];

            this.trade = new Trade[] { pTechTrade as Trade };

            this.IdTechTrade = pIdTechTrade;
            this.IdentifierTechTrade = pIdentifierTechTrade;
        }
        
        #region IDataDocument Membres

        /// <summary>
        /// Get/Set the result of the risk evaluation (IOW Get/Set the technical trade containing the risk evaluation result)
        /// </summary>
        /// <remarks>Just one only technical trade can be added to the collection</remarks>
        [XmlArray]
        [ReadOnly(true)]
        [Browsable(false)]
        public Trade[] trade;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        ITrade[] IDataDocument.Trade
        {
            get
            {
                return trade;
            }
            set
            {
                trade = value as Trade[];
            }
        }

        [XmlIgnore]
        public ITrade FirstTrade
        {
            get
            {
                if (!ArrFunc.IsEmpty(trade))
                {
                    return trade[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (!ArrFunc.IsEmpty(trade))
                {
                    trade[0] = (Trade)value;
                }
            }
        }

        int _idTechTrade = 0;
        string _identifierTechTrade;

        /// <summary>
        /// internal id of the trade containing the risk evaluation result
        /// </summary>
        [XmlIgnore]
        public int IdTechTrade
        {
            get { return _idTechTrade; }
            set { _idTechTrade = value; }
        }

        /// <summary>
        /// internal identifier of the trade containing the risk evaluation result
        /// </summary>
        [XmlIgnore]
        public string IdentifierTechTrade
        {
            get { return _identifierTechTrade; }
            set { _identifierTechTrade = value; }
        }

        /// <summary>
        /// Return the current instance, representing the risk evaluation log report
        /// </summary>
        [XmlIgnore]
        public object Item
        {
            get { return this; }
        }

        /// <summary>
        /// Return the element type of the current actors collection
        /// </summary>
        /// <returns></returns>
        public Type GetTypeParty()
        {
            if (party != null)
            {
                return party.GetType().GetElementType();
            }
            else
            {
                return null;
            }
        }

        INettingInformationInput m_NettingInformationInput = null;

        /// <summary>
        /// Create the netting informations set, IFF the current one is null.
        /// </summary>
        /// <returns>the current netting informations set when that has been built, a brand new informations set object otherwise</returns>
        public INettingInformationInput CreateNettingInformationInput()
        {
            if (m_NettingInformationInput == null)
            {
                m_NettingInformationInput = new NettingInformationInput();
            }

            return m_NettingInformationInput;
        }

        #endregion

        #region IDocument Membres

        private readonly DocumentVersionEnum m_version = DocumentVersionEnum.Version40;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        [Obsolete("Utilisez la propriété EfsMLversion", false)]
        public DocumentVersionEnum Version
        {
            get 
            {
                return this.m_version; 
            }
        }

        //int IDocument.actualbuild
        //{
        //    set { ;}
        //    get { return 12;}
        //}

        #endregion

        #region IEfsDocument Membres

        private EfsMLDocumentVersionEnum m_EfsMLversion = EfsMLDocumentVersionEnum.Version30;

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [ReadOnly(true)]
        [Browsable(false)]
        public EfsMLDocumentVersionEnum EfsMLversion
        {
            get 
            { 
                return this.m_EfsMLversion; 
            }

            set
            {
                this.m_EfsMLversion = value;  
            }
        }

        bool m_repositorySpecified = false;

        [XmlIgnore]
        public bool RepositorySpecified
        {
            get
            {
                return m_repositorySpecified;
            }
            set
            {
                m_repositorySpecified = value;
            }
        }

        /// <summary>
        /// Repository containing all the Asset ETD et Markets compsing the position
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public EfsML.v30.Repository.Repository repository = null;


        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IRepository IRepositoryDocument.Repository
        {
            get
            {
                return repository;
            }
            set
            {
                repository = value as EfsML.v30.Repository.Repository;
            }
        }

        /// <summary>
        /// Create a new repository, IFF the current one is null.
        /// </summary>
        /// <returns>the current repository instance when that has been built, a brand new repository object otherwise</returns>
        public IRepository CreateRepository()
        {
            if (repository == null)
            {
                repository = new EfsML.v30.Repository.Repository();
            }

            return repository;
        }

        /// <summary>
        /// Retourne une liste vide
        /// </summary>
        /// <returns></returns>
        /// FI 20150708 [XXXXX] Add
        List<IAssetRepository> IRepositoryDocument.GetAllRepositoryAsset()
        {
            return new List<IAssetRepository>();
        }





        #endregion

        #region IMarginRequirementDatas Membres

        /// <summary>
        /// Calculation sheet root
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public MarginRequirementOffice  marginRequirementOffice = new MarginRequirementOffice();

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMarginRequirementOffice IMarginDetailsDocument.MarginRequirementOffice 
        {
            get
            {
                return marginRequirementOffice; 
            }

            set
            {
                marginRequirementOffice = value as MarginRequirementOffice;
            }
        }

        #endregion

        #region IDataDocument Membres

        /// <summary>
        /// Get/Set all the actors (counterparty/entity/clearer) affected by the risk evaluation 
        /// </summary>
        /// <remarks>WARNING: il faut bâtir cette champ publique lorsqu'on veut uutiliser la classe helper DataDocumentContainer.
        /// l'implémentation de cette classe se fonde sur la reflection, elle cherche d'autres membres par rapport à la signature 
        /// de l'interface IDataDocument : on cherche des champs alors que'il faudrait chercher des propriétés! 
        /// Donc on est obligée à donner une implémentation de l'interface explicite </remarks>
        [XmlArray]
        [ReadOnly(true)]
        [Browsable(false)]
        public Party[] party;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IParty[] IDataDocument.Party
        {
            set
            {
                party = value as Party[];
            }
            get
            {
                return party;
            }
        }

        #endregion
    }

    /// <summary>
    /// Claculation sheet root.
    /// Class hosting the calculation details of a risk evaluation.
    /// </summary>
    public class MarginRequirementOffice : IMarginRequirementOffice
    {
        [XmlAttribute(DataType = "IDREF")]
        public string href;

        [XmlAttribute(DataType = "boolean")]
        public bool isGross;

        [XmlAttribute(DataType = "decimal", AttributeName = "wratio")]
        public decimal weightingRatio;

        /// <summary>
        /// Book affected by the risk result (deposit)
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public BookId bookId;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IBookId IMarginRequirementOffice.BookId
        {
            get
            {
                return bookId;
            }

            set
            {
                bookId = value as BookId;
            }
        }

        /// <summary>
        /// counterparty of the trade risk, payer of the deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public PartyReference payerPartyReference;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IReference IMarginRequirementOffice.PayerPartyReference
        {
            get
            {
                return payerPartyReference;
            }

            set
            {
                payerPartyReference = value as PartyReference;
            }
        }

        /// <summary>
        /// dealer of the trade risk, receiver of the deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public PartyReference receiverPartyReference;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IReference IMarginRequirementOffice.ReceiverPartyReference
        {
            get
            {
                return receiverPartyReference;
            }

            set
            {
                receiverPartyReference = value as PartyReference;
            }
        }

        /// <summary>
        /// amounts of the risk evaluation
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray]
        public Money[] marginAmounts;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMoney[] IMarginRequirementOffice.MarginAmounts
        {
            get
            {
                return marginAmounts;
            }

            set
            {
                marginAmounts = new Money[value.Length];
                value.CopyTo(marginAmounts, 0);
            }
        }

        /// <summary>
        /// risk evaluation details (containing all the objects participating at the risk evaluation)
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public MarginCalculation marginCalculation = new MarginCalculation();

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMarginCalculation IMarginRequirementOffice.MarginCalculation
        {
            get
            {
                return marginCalculation;
            }

            set
            {
                marginCalculation = value as MarginCalculation;
            }
        }
    }

    /// <summary>
    /// Class defining the risk evaluation details structures, whatever modality net/gross has been used
    /// </summary>
    public class MarginCalculation : IMarginCalculation
    {
        /// <summary>
        /// Get a valid calculation element, according with the current risk evaluation mode [gross, net]
        /// </summary>
        /// <param name="pIsGrossMargining">risk evaluation mode</param>
        public MarginCalculation(bool pIsGrossMargining)
        {
            if (!pIsGrossMargining)
            {
                netMargin = new NetMargin();
            }
        }
        
        /// <summary>
        /// get an empty calculation element, defined for XML serialization purpose
        /// </summary>
        /// <remarks>
        /// defined for XML serialization purpose
        /// </remarks>
        [ReadOnly(true)]
        [Browsable(false)]
        public MarginCalculation()
        { 
        }

        #region IMarginCalculation Membres

        /// <summary>
        /// 
        /// </summary>
        [XmlArray]
        public MarginRequirementOffice[] grossMargin = null;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMarginRequirementOffice[] IMarginCalculation.GrossMargin
        {
            get
            {
                return grossMargin;
            }
            set
            {
                grossMargin = new MarginRequirementOffice[value.Length];
                value.CopyTo(grossMargin, 0);
            }
        }

        /// <summary>
        /// Additional positions connected to the gross margin deposits collection
        /// </summary>
        /// <remarks>including all the positions of the actors NOT Margin Requirement Office</remarks>
        [XmlArray]
        public FixPositionReport[] positions;

        /// <summary>
        /// 
        /// </summary>
        public NetMargin netMargin = null;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        INetMargin IMarginCalculation.NetMargin
        {
            get
            {
                return netMargin;
            }
            set
            {
                netMargin = value as NetMargin;
            }
        }

        #endregion
    }

    /// <summary>
    ///  Class defining the risk evaluation details structures, specific for net modality 
    /// </summary>
    public class NetMargin :INetMargin
    {
        /// <summary>
        /// get an empty net margin element, defined for XML serialization purpose
        /// </summary>
        /// <remarks>
        /// defined for XML serialization purpose
        /// </remarks>
        public NetMargin()
        { 
        }

        #region INetMargin Membres

        [XmlArray]
        [XmlArrayItem("PosRpt")]
        public FixPositionReport[] positions;

        /// <summary>
        /// Positions relative to the evaluated marginAmount ?? 
        /// </summary>
        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IFixPositionReport[] INetMargin.Positions
        {
            get
            {
                return positions;
            }
        }

        /// <summary>
        /// Evaluation mode details
        /// </summary>
        // PM 20151116 [21561] marginCalculationMethod devient un tableau serializé en sequence
        //public MarginCalculationMethod marginCalculationMethod;
        [XmlElement]
        public MarginCalculationMethod[] marginCalculationMethod;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        // PM 20151116 [21561] marginCalculationMethod devient un tableau serializé en sequence
        //IMarginCalculationMethod INetMargin.marginCalculationMethod
        IMarginCalculationMethod[] INetMargin.MarginCalculationMethod
        {
            get
            {
                return marginCalculationMethod;
            }
            set
            {
                // PM 20151116 [21561] marginCalculationMethod devient un tableau
                //marginCalculationMethod = value as MarginCalculationMethod;
                marginCalculationMethod = value as MarginCalculationMethod[];
            }
        }

        /// <summary>
        /// Delivery details
        /// </summary>
        public DeliveryMarginCalculationMethod deliveryCalculationMethod;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMarginCalculationMethod INetMargin.DeliveryCalculationMethod
        {
            get
            {
                return deliveryCalculationMethod;
            }
            set
            {
                deliveryCalculationMethod = value as DeliveryMarginCalculationMethod;
            }
        }

        /// <summary>
        /// amounts of the risk evaluation
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray]
        public Money[] marginAmounts;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMoney[] INetMargin.MarginAmounts
        {
            get
            {
                return marginAmounts;
            }

            set
            {
                marginAmounts = new Money[value.Length];
                value.CopyTo(marginAmounts, 0);
            }
        }

        #endregion
    }

    /// <summary>
    /// Class representing a fix position
    /// </summary>
    public class FixPositionReport : Abstract_message, IFixPositionReport
    {

        #region IFixPositionReport Membres

        [XmlAttribute]
        public DateTime BizDt;
        [XmlIgnore]
        public bool BizDtSpecified;

        [XmlAttribute]
        public string RptID = String.Empty;
        [XmlIgnore]
        public bool RptIDSpecified;

        [XmlAttribute]
        public string SetSesID = String.Empty;
        [XmlIgnore]
        public bool SetSesIDSpecified;

        [XmlIgnore]
        public bool AcctSpecified;
        [XmlAttribute]
        public string Acct = String.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20220311 [XXXXX] use DerivativeContractTypeEnum
        [XmlAttribute(AttributeName="CtType")]
        public DerivativeContractTypeEnum ContractType;
        [XmlIgnore]
        public bool ContractTypeSpecified;

        [XmlElement(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 2)]
        public Parties_Block Pty = new Parties_Block();
        [XmlIgnore]
        public bool PtySpecified;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IFixParty IFixPositionReport.Pty
        {
            get
            {
                return Pty;
            }
            set
            {
                Pty = value as Parties_Block;
            }
        }

        [XmlElement(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 3)]
        public InstrumentBlock Instrmt = new InstrumentBlock();
        [XmlIgnore]
        public bool InstrmtSpecified;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IFixInstrument IFixPositionReport.Instrmt
        {
            get
            {
                return Instrmt;
            }
            set
            {
                Instrmt = value as InstrumentBlock;
            }
        }

        [XmlElement(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 4)]
        public FixQuantity Qty;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IFixQuantity IFixPositionReport.Qty
        {
            get
            {
                return Qty;
            }
            set
            {
                Qty = value as FixQuantity;
            }
        }

        #endregion

        [XmlElement(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 5, ElementName = "Qty")]
        public FixQuantity ExeAssQty
        {
            get;
            set;
        }

        [XmlElement(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 6, ElementName = "Qty")]
        public FixQuantity CoveredQty
        {
            get;
            set;
        }

        [XmlElement(Namespace = "http://www.fixprotocol.org/FIXML-5-0-SP1", Order = 7, ElementName = "Qty")]
        public FixQuantity CompensationQty
        {
            get;
            set;
        }
    }
    
    

    public class FixQuantity : IFixQuantity
    {

        #region IFixQuantity Membres

        decimal m_Long = 0;

        [XmlAttribute]
        public decimal Long
        {
            get
            {
                return m_Long;
            }
            set
            {
                m_Long = value;
            }
        }

        decimal m_Short = 0;

        [XmlAttribute]
        public decimal Short
        {
            get
            {
                return m_Short;
            }
            set
            {
                m_Short = value;
            }
        }

        PosType m_Typ = default;

        [XmlIgnore]
        public PosType Typ
        {
            get
            {
                return m_Typ;
            }
            set
            {
                m_Typ = value;
            }
        }

        DateTime m_QuantityDate = default;

        [XmlIgnore]
        public DateTime QuantityDate
        {
            get
            {
                return m_QuantityDate;
            }
            set
            {
                m_QuantityDate = value;
            }
        }

        #endregion

        [XmlAttribute(AttributeName = "QtyDt")]
        public string DescrQuantityDate
        {
            get 
            {
                if (QuantityDate != default)
                {
                    return QuantityDate.ToShortDateString(); 
                }
                else
                {
                    return null;
                }
            }

            set 
            {
                if (value != null)
                {
                    DateTime.TryParse(value, out m_QuantityDate);
                }
            }

        }

        [XmlAttribute(AttributeName = "Typ")]
        public string DescrTyp
        {
            get 
            {
                if (Typ != default)
                {
                    return System.Enum.GetName(typeof(PosType), Typ);
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (System.Enum.IsDefined(typeof(PosType), value))
                {
                    Typ = (PosType)System.Enum.Parse(typeof(PosType), value);
                }
            }
        }
    }

    /// <summary>
    /// Abstract class representing a calculation method
    /// </summary>
    /// <remarks>xsd:complexType name="MarginCalculationMethod"</remarks>
    [XmlRoot(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlInclude(typeof(CboeMarginCalculationMethod))]
    [XmlInclude(typeof(CustomMarginCalculationMethod))]
    [XmlInclude(typeof(IMSMCalculationMethod))]
    [XmlInclude(typeof(EuronextVarCalculationMethod))]
    [XmlInclude(typeof(MeffMarginCalculationMethod))]
    [XmlInclude(typeof(NOMXCFMCalculationMethod))]
    [XmlInclude(typeof(NoMarginCalculationMethod))]
    [XmlInclude(typeof(OMXMarginCalculationMethod))]
    [XmlInclude(typeof(PrismaMarginCalculationMethod))]
    [XmlInclude(typeof(SpanMarginCalculationMethod))]
    [XmlInclude(typeof(Span2MarginCalculationMethod))]
    [XmlInclude(typeof(TimsEurexMarginCalculationMethod))]
    [XmlInclude(typeof(TimsIdemMarginCalculationMethod))]
    public abstract class MarginCalculationMethod : IMarginCalculationMethod
    {
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Version de la méthode
        /// </summary>
        // PM 20180316 [23840] Ajout 
        [XmlAttribute(AttributeName = "version", DataType = "decimal")]
        public decimal Version;
        [XmlIgnore]
        public bool VersionSpecified;

        /// <summary>
        /// Date of calculation parameters
        /// </summary>
        /// PM 20150511 [20575] New
        [XmlAttribute(AttributeName = "parametersDate", DataType = "date")]
        public DateTime ParametersDate;
        [XmlIgnore]
        public bool ParametersDateSpecified;

        /// <summary>
        /// parameters collection
        /// </summary>
        [XmlIgnore]
        public abstract RiskParameter[] Parameters { get; set; }

        IRiskParameter[] IMarginCalculationMethod.Parameters
        {
            get { return Parameters; }
        }

        /// <summary>
        /// total amounts relative to the whole set of the parameters
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray]
        public Money[] marginAmounts;

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMoney[] IMarginCalculationMethod.MarginAmounts
        {
            get { return marginAmounts; }

            set
            {
                marginAmounts = new Money[value.Length];
                value.CopyTo(marginAmounts, 0);
            }
        }

    }

    /// <summary>
    /// abstract class representing a generic riks evaluation parameter
    /// </summary>
    // PM 20190222 [24326] Nouvelle méthode NOMX_CFM
    [XmlRoot(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlInclude(typeof(CustomContractParameter))]
    [XmlInclude(typeof(CustomAmountParameter))]
    [XmlInclude(typeof(TimsIdemProductParameter))]
    [XmlInclude(typeof(TimsIdemClassParameter))]
    [XmlInclude(typeof(TimsIdemContractParameter))]
    [XmlInclude(typeof(DeliveryParameter))]
    [XmlInclude(typeof(TimsEurexGroupParameter))]
    [XmlInclude(typeof(TimsEurexClassParameter))]
    [XmlInclude(typeof(TimsEurexContractParameter))]
    [XmlInclude(typeof(SpanExchangeComplexParameter))]
    [XmlInclude(typeof(SpanCombinedCommodityGroupParameter))]
    [XmlInclude(typeof(SpanCombinedCommodityParameter))]
    [XmlInclude(typeof(SpanContractParameter))]
    [XmlInclude(typeof(CboeMarginContractParameter))]
    [XmlInclude(typeof(CboeMarginNormalMarginParameter))]
    [XmlInclude(typeof(MeffMarginClassParameter))]
    [XmlInclude(typeof(MeffMarginMaturityParameter))]
    [XmlInclude(typeof(NOMXCFMResultCurveAmount))]
    [XmlInclude(typeof(OMXUnderlyingSymbolParameter))]
    [XmlInclude(typeof(PrismaLiquidGroupParameter))]
    [XmlInclude(typeof(PrismaLiquidGroupSplitParameter))]
    [XmlInclude(typeof(PrismaRiskMeasureSetParameter))]
    [XmlInclude(typeof(PrismaSubSampleParameter))]
    [XmlInclude(typeof(Span2CCPParameter))]
    [XmlInclude(typeof(Span2GroupParameter))]
    [XmlInclude(typeof(Span2PodParameter))]
    [XmlInclude(typeof(Span2ProductGroupParameter))]
    public abstract class RiskParameter : IRiskParameter
    {

        /// <summary>
        /// sub-parameters collection
        /// </summary>
        [XmlIgnore]
        public abstract RiskParameter[] Parameters {get; set;}

        IRiskParameter[] IRiskParameter.Parameters
        {
            get { return Parameters; }
        }

        /// <summary>
        /// grouped positions relative to the parameter
        /// </summary>
        [XmlArray]
        [XmlArrayItem("PosRpt")]
        public FixPositionReport[] positions;

        IFixPositionReport[] IRiskParameter.Positions
        {
            get { return positions; }
        }

        /// <summary>
        /// amounts relative to the parameter
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "marginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney marginAmount;

        IMoney IRiskParameter.MarginAmount
        {
            get 
            { 
                return marginAmount?.ToMoney(); 
            }
        }

    }






    public abstract class PrismaRiskParameter : IRiskParameter
    {

        /// <summary>
        /// sub-parameters collection
        /// </summary>
        [XmlIgnore]
        public abstract RiskParameter[] Parameters { get; set; }

        IRiskParameter[] IRiskParameter.Parameters
        {
            get { return Parameters; }
        }

        /// <summary>
        /// grouped positions relative to the parameter
        /// </summary>
        [XmlArray]
        [XmlArrayItem("PosRpt")]
        public FixPositionReport[] positions;

        IFixPositionReport[] IRiskParameter.Positions
        {
            get { return positions; }
        }

        /// <summary>
        /// amounts relative to the parameter
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "marginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney marginAmount;

        IMoney IRiskParameter.MarginAmount
        {
            get
            {
                return  marginAmount?.ToMoney();
            }
        }

    }
    #endregion Margin Requirement

    #region Common 
    /// <summary>
    /// Rate exchange element pair for the margin evaluation
    /// </summary>
    [XmlRoot(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [XmlType(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MarginFxRatePair
    {
        /// <summary>
        /// True when the currency is missing or disabled or can not be retrieven
        /// </summary>
        [XmlAttribute(AttributeName = "unavailable", DataType = "boolean")]
        public bool Missing;
        [XmlIgnore]
        public bool MissingSpecified;

        /// <summary>
        /// Credit echange rate, used for negative margin amount
        /// </summary>
        [XmlElement("credit")]
        public FxRate Credit;

        /// <summary>
        /// Debit echange rate, used for positive margin amount (real margin)
        /// </summary>
        [XmlElement("debit")]
        public FxRate Debit;
    }

    /// <summary>
    /// Specific amount model for margin calculation sheets
    /// </summary>
    [XmlRoot(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class MarginMoney
    {
        /// <summary>
        /// Currency amount
        /// </summary>
        [XmlAttribute(AttributeName = "curr", DataType = "string")]
        public string Currency
        {get; set;}

        /// <summary>
        /// Amount
        /// </summary>
        [XmlAttribute(AttributeName = "amount", DataType = "decimal")]
        public decimal Amount
        { get; set; }

        /// <summary>
        /// Convert any array of objects implements IMoney interface to a MarginMoney array object
        /// </summary>
        /// <param name="pMoney">Object array to be converted implementing IMoney</param>
        /// <returns>a new MarginMoney array object, when the input parameter is not null, else returns null</returns>
        public static MarginMoney[] FromMoney(IMoney[] pMoney)
        {
            MarginMoney[] res = null;
            if (pMoney != null)
            {
                res = (from m in pMoney select MarginMoney.FromMoney(m)).ToArray();
            }
            return res;
        }

        /// <summary>
        /// Convert any object implements IMoney interface to a MarginMoney object
        /// </summary>
        /// <param name="pMoney">Object to be converted implementing IMoney</param>
        /// <returns>a new MarginMoney object, when the input parameter is not null, else returns null</returns>
        public static MarginMoney FromMoney(IMoney pMoney)
        {
            MarginMoney res = null;

            if (pMoney != null)
            {
                res = new MarginMoney
                {
                    Amount = pMoney.Amount.DecValue,

                    Currency = pMoney.Currency,
                };
            }

            return res;
        }

        /// <summary>
        /// Convert this object to a FpML Money object
        /// </summary>
        /// <param name="pMoney">Object to be converted implementing IMoney</param>
        /// <returns>an IMoney reference realted to the built Money object </returns>
        public IMoney ToMoney()
        {
            IMoney res = new Money(Amount, Currency);
            return res;
        }
    }

    /// <summary>
    /// Tims generic margin object 
    /// <para>
    /// Margin types: additional, premium, mtm, liquidating, spread
    /// </para>
    /// </summary>
    [XmlRoot(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TimsDecomposableParameter
    {

        /// <summary>
        /// Margin factors, their sum identifies the MarginAmount for the current margin object .
        /// <seealso cref="TimsFactor"/>
        /// </summary>
        [XmlArray("factors", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("factor")]
        public TimsFactor[] Factors;

        /// <summary>
        /// Total for the margin object
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "marginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmount;

        /// <summary>
        /// Starting currency, not null just in case the amount results by an exchange rate conversion from this currency
        /// to the amount currency
        /// </summary>
        [XmlAttribute(AttributeName = "currFrom", DataType = "string")]
        public string CurrencyFrom;
    }

    /// <summary>
    /// Generic factor identifying part of a TimsDecomposableParameter object (<seealso cref="TimsDecomposableParameter"/>)
    /// </summary>
    [XmlRoot(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class TimsFactor
    {
        /// <summary>
        /// Factor unique identifier
        /// </summary>
        [XmlAttribute(AttributeName = "ID", DataType = "string")]
        public string Id;

        [XmlAttribute(AttributeName = "mult", DataType = "decimal")]
        public decimal Multiplier;
        [XmlIgnore]
        public bool MultiplierSpecified;

        [XmlAttribute(AttributeName = "spot", DataType = "boolean")]
        public bool SpotMonth;
        [XmlIgnore]
        public bool SpotMonthSpecified;

        public EFS_Decimal MinimumRate;

        public EFS_Decimal StrkPx;

        public EFS_Decimal Quote;

        public FixQuantity Qty;

        public FixQuantity CompensatedQty;

        [XmlAttribute(AttributeName = "idx")]
        public int Index;
        [XmlIgnore]
        public bool IndexSpecified;

        [XmlAttribute(AttributeName = "adj", DataType = "boolean")]
        public bool ShortAdj;
        [XmlIgnore]
        public bool ShortAdjSpecified;

        [XmlAttribute(AttributeName = "mat", DataType = "string")]
        public string MaturityYearMonth;

        [XmlAttribute(AttributeName = "fct", DataType = "string")]
        public string MaturityFactor;

        [XmlArray("riskarray", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("sidepoint")]
        public TimsFactor[] RiskArray;

        /// <summary>
        /// Factor amount
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "marginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmount;
    }

    /// <summary>
    /// Valeur de risque d'un scénario
    /// </summary>
    [XmlRoot(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [XmlType(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public class ScenarioRiskValue
    {
        [XmlAttribute(AttributeName = "scenario", DataType = "int")]
        public int Scenario { get; set; }

        [XmlAttribute(AttributeName = "value", DataType = "decimal")]
        public decimal RiskValue { get; set; }
    }

    #endregion Common

    #region Delivery

    // UNDONE MF 20110923 that class is missing from the XSD
    /// <summary>
    /// Delivery details
    /// </summary>
    /// <remarks>shared among the all methods</remarks>
    public class DeliveryMarginCalculationMethod : MarginCalculationMethod
    {
        DeliveryParameter[] m_parameters;

        /// <summary>
        /// contract parameters collection
        /// </summary>
        [XmlArray("deliveries")]
        [XmlArrayItem("delivery")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new DeliveryParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    // UNDONE MF 20110923 that class is missing from the XSD
    /// <summary>
    /// delivery parameter of the delivery method
    /// </summary>
    /// <remarks></remarks>
    public class DeliveryParameter : RiskParameter
    {
        /// <summary>
        /// contract internal id
        /// </summary>
        [XmlAttribute(AttributeName = "OTCmlId", DataType = "int")]
        public int OTCmlId;

        /// <summary>
        /// asset etd identifier
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "string")]
        public string Identifier;

        /// <summary>
        /// contract symbol
        /// </summary>
        [XmlAttribute(AttributeName = "symbol", DataType = "string")]
        public string ContractSymbol;

        /// <summary>
        /// Etape de livraison
        /// </summary>
        // PM 20130911 [17949] ajout DeliveryStep
        [XmlAttribute(DataType = "string")]
        public string deliveryStep;

        [XmlAttribute(DataType = "string")]
        public string marginExpressionType;

        [XmlAttribute]
        public decimal amount;

        [XmlAttribute]
        public decimal contractSize;
        [XmlIgnore]
        public bool contractSizeSpecified;

        [XmlAttribute]
        public decimal quote;
        [XmlIgnore]
        public bool quoteSpecified;

        [XmlAttribute]
        public bool quoteMissing;
        [XmlIgnore]
        public bool quoteMissingSpecified;

        [XmlIgnore]
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
    }

    #endregion Delivery

    # region Custom Method

    /// <summary>
    /// Standard method
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class CustomMarginCalculationMethod : MarginCalculationMethod
    {
        CustomContractParameter[] m_parameters;

        /// <summary>
        /// contract parameters collection
        /// </summary>
        [XmlArray("contracts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new CustomContractParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// contract parameter of the "standard" risk evaluation method
    /// </summary>
    /// <remarks>xsd reference: name="MethStdContract"</remarks>
    public class CustomContractParameter : RiskParameter
    {
        /// <summary>
        /// contract internal id
        /// </summary>
        [XmlAttribute(AttributeName = "OTCmlId", DataType = "int")]
        public int OTCmlId;

        /// <summary>
        /// contract identifier
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "string")]
        public string Identifier;

        [XmlAttribute(DataType = "string")]
        public string marginExpressionType;

        [XmlAttribute]
        public decimal quote;
        [XmlIgnore]
        public bool quoteSpecified;

        [XmlAttribute]
        public decimal multiplier;
        [XmlIgnore]
        public bool multiplierSpecified;

        [XmlAttribute]
        public bool missing;
        [XmlIgnore]
        public bool missingSpecified;

        CustomAmountParameter[] m_parameters;

        /// <summary>
        /// "Custom" amount parameters collection
        /// </summary>
        [XmlArray("customAmounts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("customAmount")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new CustomAmountParameter[value.Length];
                value.CopyTo(m_parameters, 0); ;
            }
        }
    }

    /// <summary>
    /// Amount parameter, child of the custom contract parameter
    /// </summary>
    public class CustomAmountParameter : RiskParameter
    {
        [XmlAttribute]
        public string type;

        [XmlAttribute]
        public decimal amount;

        [XmlIgnore]
        public override RiskParameter[] Parameters
        {
            get{return null;}
            set{}
        }
    }

    #endregion Custom Method

    #region TIMS IDEM Method

    /// <summary>
    /// Standard method
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class TimsIdemMarginCalculationMethod : MarginCalculationMethod
    {
        [XmlAttribute(AttributeName = "crossMargin")]
        public bool CrossMarginActivated;

        TimsIdemProductParameter[] m_parameters;

        [XmlArray("products", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("product")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new TimsIdemProductParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Margin product (first level of the IDEM hierarchy)
    /// </summary>
    public class TimsIdemProductParameter : RiskParameter
    {
        /// <summary>
        /// group name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        [XmlElement(ElementName = "spread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Spread;

        [XmlElement(ElementName = "mtm", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Mtm;

        [XmlElement(ElementName = "premium", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Premium;

        [XmlElement(ElementName = "additional", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Additional;

        [XmlElement(ElementName = "minimum", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Minimum;

        TimsIdemClassParameter[] m_parameters;

        [XmlArray("classes", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("class")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new TimsIdemClassParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Margin class (second level of the IDEM hierarchy)
    /// </summary>
    public class TimsIdemClassParameter : RiskParameter
    {
        /// <summary>
        /// Class name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Contracts identifiers list
        /// </summary>
        [XmlAttribute(AttributeName = "sym", DataType = "string")]
        public string Symbols;

        [XmlElement(ElementName = "spread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Spread;

        [XmlElement(ElementName = "mtm", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Mtm;

        [XmlElement(ElementName = "premium", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Premium;

        [XmlElement(ElementName = "additional", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Additional;

        [XmlElement(ElementName = "minimum", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Minimum;

        [XmlArray("contracts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public override RiskParameter[] Parameters
        {
            get;
            set;
        }
    }

    /// <summary>
    /// (Margin) contract (thirth and last level of the IDEM hierarchy)
    /// </summary>
    public class TimsIdemContractParameter : RiskParameter
    {
        /// <summary>
        /// Contract Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        [XmlAttribute(AttributeName = "description", DataType = "string")]
        public string Description;

        [XmlAttribute(AttributeName = "offset", DataType = "decimal")]
        public decimal Offset;

        //PM 20141113 [20491] Ajout du calcul des spreads au niveau Contract
        [XmlElement(ElementName = "spread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Spread;

        [XmlElement(ElementName = "mtm", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Mtm;

        [XmlElement(ElementName = "premium", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Premium;

        [XmlElement(ElementName = "additional", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Additional;

        /// <summary>
        /// Converted grouped future positions relative to the parameter
        /// </summary>
        [XmlArray("futureConvertedPositions", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("PosRpt")]
        public FixPositionReport[] ConvertedPositions;

        // empty list
        public override RiskParameter[] Parameters
        {
            get
            {
                return null;
            }
            set
            {}
        }
    }

    #endregion TIMS IDEM Method

    #region TIMS EUREX Method

    /// <summary>
    /// Class containing the calculation details of a deposit evaluated with the TIMS EUREX method
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class TimsEurexMarginCalculationMethod : MarginCalculationMethod
    {

        TimsEurexGroupParameter[] m_parameters = null;

        /// <summary>
        /// EUREX groups. First level of the EUREX product hierarchy
        /// </summary>
        [XmlArray("groups", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("group")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new TimsEurexGroupParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }

        /// <summary>
        /// Exchange rates
        /// </summary>
        [XmlArray("rates", Namespace = "http://www.fpml.org/2007/FpML-4-4")]
        [XmlArrayItem("rate")]
        public MarginFxRatePair[] ExchRates;

        /// <summary>
        /// Cross margin
        /// </summary>
        [XmlArray("crossMargin", Namespace = "http://www.fpml.org/2007/FpML-4-4")]
        [XmlArrayItem("cross")]
        public TimsDecomposableParameter[] Cross;

        /// <summary>
        /// total not crossed amounts relative to the whole set of the parameters
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray("notCrossed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public Money[] NotCrossedMarginAmounts;


    }

    /// <summary>
    /// Margin group (first level of the EUREX hierarchy)
    /// </summary>
    public class TimsEurexGroupParameter : RiskParameter
    {
        /// <summary>
        /// group name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// “Out-of-the-money” minimum is part of the calculation of the short option adjustment 
        /// for the determination of margin requirements in short option positions.
        /// </summary>
        [XmlAttribute(AttributeName = "OOMmin", DataType = "decimal")]
        public decimal OutOfTheMoneyMinValue;

        /// <summary>
        /// Min offset we used to multiply the class additional (credits only) margin 
        /// before aggregating it to compute the group additional margin.
        /// </summary>
        [XmlAttribute(AttributeName = "offset", DataType = "decimal")]
        public decimal Offset;

        /// <summary>
        /// Premium margins of the group (they could be more than one, 
        /// because the classes making part of the group may be in different currencies) 
        /// </summary>
        [XmlArray(ElementName = "premiums", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "premium")]
        public TimsDecomposableParameter[] Premiums;

        /// <summary>
        /// Spread margins of the group (they could be more than one, 
        /// because the classes making part of the group may be in different currencies) 
        /// </summary>
        [XmlArray(ElementName = "spreads", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "spread")]
        public TimsDecomposableParameter[] Spreads;

        /// <summary>
        /// Addtional margins of the group (they could be more than one, 
        /// because the classes making part of the group may be in different currencies) 
        /// </summary>
        [XmlArray(ElementName = "additionals", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "additional")]
        public TimsDecomposableParameter[] Additionals;

        /// <summary>
        /// Classes
        /// </summary>
        [XmlArray("classes", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("class")]
        public override RiskParameter[] Parameters
        {
            get;
            set;
        }

        /// <summary>
        /// Total amounts. 
        /// A margin group may consist of margin classes amounts based on different currencies.
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray("marginAmounts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("marginAmount")]
        public MarginMoney[] marginAmounts;
        
    }

    /// <summary>
    /// Margin class (second level of the EUREX hierarchy)
    /// </summary>
    public class TimsEurexClassParameter : RiskParameter
    {
        /// <summary>
        /// Margin class name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Class offset
        /// </summary>
        [XmlAttribute(AttributeName = "offset", DataType = "decimal")]
        public decimal Offset;

        /// <summary>
        /// Maturity factor used to adjust the maximum additional risk value computed for this class
        /// </summary>
        [XmlAttribute(AttributeName = "mgnfct", DataType = "decimal")]
        public decimal MaturityFactor;

        /// <summary>
        /// amounts to be paid for futures spread positions of the spot month for all spread types
        /// (e.g. Mar/Jun, Jun/Sep or Mar/Sep). These amounts are used to determine the futures spread margin.
        /// </summary>
        [XmlAttribute(AttributeName = "backmonthsrate", DataType = "decimal")]
        public decimal BackMonthSpreadRate
        {
            get;
            set;
        }

        /// <summary>
        /// amounts to be paid for futures spread positions of the back months for all spread types
        /// (e.g. Mar/Jun, Jun/Sep or Mar/Sep). These amounts are used to determine the futures spread margin.
        /// </summary>
        [XmlAttribute(AttributeName = "spotmonthrate", DataType = "decimal")]
        public decimal SpotMonthSpreadRate
        {
            get;
            set;
        }

        /// <summary>
        /// Contracts identifiers list
        /// </summary>
        [XmlAttribute(AttributeName = "sym", DataType = "string")]
        public string Symbols;

        /// <summary>
        /// Premium margin 
        /// </summary>
        [XmlElement(ElementName = "premium", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Premium;

        /// <summary>
        /// Spread margin
        /// </summary>
        [XmlElement(ElementName = "spread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Spread;

        /// <summary>
        /// Spread margin
        /// </summary>
        [XmlElement(ElementName = "additional", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Additional;

        /// <summary>
        /// Spread margin
        /// </summary>
        [XmlElement(ElementName = "liquidating", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Liquidating;

        /// <summary>
        /// Contracts 
        /// </summary>
        [XmlArray("contracts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public override RiskParameter[] Parameters
        {
            get;
            set;
        }
    }

    /// <summary>
    /// (Margin) contract (thirth and last level of the EUREX hierarchy)
    /// </summary>
    public class TimsEurexContractParameter : RiskParameter
    {
        /// <summary>
        /// Contract Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Information
        /// </summary>
        [XmlAttribute(AttributeName = "information", DataType = "string")]
        public string Information;
        [XmlIgnore]
        public bool InformationSpecified;

        /// <summary>
        /// Premium
        /// </summary>
        [XmlElement(ElementName = "premium", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Premium;

        /// <summary>
        /// Additional
        /// </summary>
        [XmlElement(ElementName = "additional", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public TimsDecomposableParameter Additional;

        // Empty list (a contract element does not have sub-parameters )
        public override RiskParameter[] Parameters
        {
            get
            {
                return null;
            }
            set
            { }
        }
    }

    #endregion TIMS EUREX Method

    #region SPAN Method
    /// <summary>
    ///  Class contenant le détail du calcul d'un déposit évalué avec la méthode SPAN
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class SpanMarginCalculationMethod : MarginCalculationMethod
    {
        private SpanExchangeComplexParameter[] m_parameters = null;

        /// <summary>
        /// Account Type
        /// </summary>
        [XmlAttribute(AttributeName = "accounttype", DataType = "string")]
        public string AccountType;
        [XmlIgnore]
        public bool AccountTypeSpecified;

        /// <summary>
        /// Maintenance Initial indicateur
        /// </summary>
        [XmlAttribute(AttributeName = "maint_init", DataType = "string")]
        public string MaintenanceInitial;
        [XmlIgnore]
        public bool MaintenanceInitialSpecified;

        /// <summary>
        /// Gestion du calcul du Concentration Risk Margin de l'ECC
        /// </summary>
        // PM 20190801 [24717] Ajout
        [XmlAttribute(AttributeName = "applyConcentrationRisk", DataType = "boolean")]
        public bool IsCalcECCConcentrationRiskMargin { get; set; }

        /// <summary>
        /// Long Option Value
        /// </summary>
        [XmlElement(ElementName = "lov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LongOptionValue;

        /// <summary>
        /// Short Option Value
        /// </summary>
        [XmlElement(ElementName = "sov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortOptionValue;
        
        /// <summary>
        /// SPAN Exchange Complex
        /// </summary>
        [XmlArray("span", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("exchangecomplex")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new SpanExchangeComplexParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }

        /// <summary>
        /// ECC Additional Margin BoM (AMBO)
        /// </summary>
        // PM 20190401 [24625][24387] Ajout AdditionalMarginBoM
        [XmlArray("ambo", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("exchangecomplex")]
        public SpanExchangeComplexParameter[] AdditionalMarginBoM { get; set; }

        /// <summary>
        /// Concentration Risk Margin de l'ECC
        /// </summary>
        // PM 20190801 [24717] Ajout
        [XmlElement(ElementName = "concentrationRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public ECCConcentrationRiskMarginParameter ConcentrationRiskMargin { get; set; }
        [XmlIgnore]
        public bool ConcentrationRiskMarginSpecified { get; set; }
    }

    /// <summary>
    /// ExchangeComplex
    /// </summary>
    public class SpanExchangeComplexParameter : RiskParameter
    {
        /// <summary>
        /// Exchange Complex Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Limitation de l'Option Value par Combined Commodity
        /// </summary>
        [XmlAttribute(AttributeName = "optionvaluelimit", DataType = "boolean")]
        public bool IsOptionValueLimit;

        /// <summary>
        /// Settlement Session
        /// </summary>
        ///PM 20150902 [21385] Added
        [XmlAttribute(AttributeName = "stlSession", DataType = "string")]
        public string SettlementSession;

        /// <summary>
        /// Business Date and Time
        /// </summary>
        ///PM 20150902 [21385] Added
        [XmlAttribute(AttributeName = "dtBusinessTime", DataType = "dateTime")]
        public DateTime DtBusinessTime;

        /// <summary>
        /// Date of parameter file
        /// </summary>
        ///PM 20150902 [21385] Added
        [XmlAttribute(AttributeName = "dtFile", DataType = "dateTime")]
        public DateTime DtFile;

        /// <summary>
        /// File identifier
        /// </summary>
        ///PM 20150902 [21385] Added
        [XmlAttribute(AttributeName = "fileIdentifier", DataType = "string")]
        public string FileIdentifier;

        /// <summary>
        /// File format
        /// </summary>
        ///PM 20150902 [21385] Added
        [XmlAttribute(AttributeName = "fileFormat", DataType = "string")]
        public string FileFormat;

        /// <summary>
        /// Super Inter Commodity Spread
        /// </summary>
        [XmlArray("superintercommodityspreads", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("spread")]
        public SpanInterCommoditySpreadParameter[] SuperInterCommoditySpread { get; set; }

        /// <summary>
        /// Inter Commodity Spread
        /// </summary>
        [XmlArray("intercommodityspreads", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("spread")]
        public SpanInterCommoditySpreadParameter[] InterCommoditySpread { get; set; }

        /// <summary>
        /// Inter Exchange Spread
        /// </summary>
        [XmlArray("interexchangespreads", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("spread")]
        public SpanInterCommoditySpreadParameter[] InterExchangeSpread { get; set; }

        /// <summary>
        /// One Factor Credit
        /// </summary>
        /// PM 20150930 [21134] Ajout OneFactorCredit
        [XmlElement(ElementName = "oneFactorCredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public SpanOneFactorCreditParameter OneFactorCredit { get; set; }
        [XmlIgnore]
        public bool OneFactorCreditSpecified;

        /// <summary>
        /// Combined Commodity Group
        /// </summary>
        [XmlArray("groups", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("group")]
        public override RiskParameter[] Parameters { get; set; }
    }

    /// <summary>
    /// Combined Commodity Group
    /// </summary>
    public class SpanCombinedCommodityGroupParameter : RiskParameter
    {
        /// <summary>
        /// Group Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Long Option Value Amount
        /// </summary>
        [XmlArray("lovbycurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("lov")]
        public MarginMoney[] LongOptionValue { get; set; }

        /// <summary>
        /// Short Option Value Amount
        /// </summary>
        [XmlArray("sovbycurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("sov")]
        public MarginMoney[] ShortOptionValue { get; set; }

        /// <summary>
        /// All Option Value Amount
        /// </summary>
        [XmlArray("novbycurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("nov")]
        public MarginMoney[] NetOptionValue { get; set; }
        
        /// <summary>
        /// All Risk Initial Amount
        /// </summary>
        [XmlArray("initialmarginbycurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("initialmargin")]
        public MarginMoney[] RiskInitial { get; set; }

        /// <summary>
        /// All Risk Maintenance Amount
        /// </summary>
        [XmlArray("maintenancemarginbycurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("maintenancemargin")]
        public MarginMoney[] RiskMaintenance { get; set; }
        [XmlIgnore]
        public bool RiskMaintenanceSpecified;

        /// <summary>
        /// Combined Commodity (Contract Group)
        /// </summary>
        [XmlArray("combinedcommodities", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("combinedcommodity")]
        public override RiskParameter[] Parameters { get; set; }
    }

    /// <summary>
    /// Combined Commodity
    /// </summary>
    public class SpanCombinedCommodityParameter : RiskParameter
    {
        /// <summary>
        /// Combined Commodity Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Strategy Spread Method Code
        /// </summary>
        [XmlAttribute(AttributeName = "strategychargemethod", DataType = "string")]
        public string StrategySpreadMethod;
        [XmlIgnore]
        public bool StrategySpreadMethodSpecified;

        /// <summary>
        /// Inter Month Spread Method Code
        /// </summary>
        [XmlAttribute(AttributeName = "intracommoditychargemethod", DataType = "string")]
        public string IntraSpreadMethod;

        /// <summary>
        /// Delivery Month Method Code
        /// </summary>
        [XmlAttribute(AttributeName = "deliverychargemethod", DataType = "string")]
        public string DeliveryMonthMethod;

        /// <summary>
        /// Weighted Risk Method
        /// </summary>
        [XmlAttribute(AttributeName = "weightedriskmethod", DataType = "string")]
        public string WeightedRiskMethod;

        /// <summary>
        /// Utilisation de la méthode One-factor (Lambda)
        /// </summary>
        /// PM 20150930 [21134] Ajout IsUseLambda
        [XmlAttribute(AttributeName = "useLambda", DataType = "boolean")]
        public bool IsUseLambda;

        /// <summary>
        /// Lambda Minimum
        /// </summary>
        /// PM 20150930 [21134] Ajout LambdaMin
        [XmlAttribute(AttributeName = "lambdaMin", DataType = "decimal")]
        public decimal LambdaMin;
        [XmlIgnore]
        public bool LambdaMinSpecified { get { return IsUseLambda; } }

        /// <summary>
        /// Lambda Maximum
        /// </summary>
        /// PM 20150930 [21134] Ajout LambdaMax
        [XmlAttribute(AttributeName = "lambdaMax", DataType = "decimal")]
        public decimal LambdaMax;
        [XmlIgnore]
        public bool LambdaMaxSpecified { get { return IsUseLambda; } }

        /// <summary>
        /// Long Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "lov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LongOptionValue;

        /// <summary>
        /// Short Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "sov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortOptionValue;

        /// <summary>
        /// Net Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "nov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney NetOptionValue;

        /// <summary>
        /// Short Option Minimum
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "som", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortOptionMinimum;

        /// <summary>
        /// Delta Net
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "deltanet", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaNet;

        /// <summary>
        /// Delta Net Remaining
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "deltanetremaining", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaNetRemaining;

        /// <summary>
        /// Active Scenario
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "activescenario", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public int ActiveScenario;
        
        /// <summary>
        /// Long Scan Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "longscanrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LongScanRisk;

        /// <summary>
        /// Short Scan Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "shortscanrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortScanRisk;

        /// <summary>
        /// Scan Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "scanrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ScanRisk;

        /// <summary>
        /// Price Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "pricerisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney PriceRisk;

        /// <summary>
        /// Time Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "timerisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney TimeRisk;

        /// <summary>
        /// Volatility Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "volatilityrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney VolatilityRisk;

        /// <summary>
        /// Normal Weighted Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "normalweightedrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney NormalWeightedRisk;

        /// <summary>
        /// Capped Weighted Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "cappedweightedrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney CappedWeightedRisk;

        /// <summary>
        /// Weighted Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "weightedrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney WeightedRisk;

        /// <summary>
        /// Strategy Spread Charge
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "strategycharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney StrategySpreadCharge;
        [XmlIgnore]
        public bool StrategySpreadChargeSpecified;

        /// <summary>
        /// Inter Month Spread Charge
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "intracommoditycharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney IntraCommoditySpreadCharge;

        /// <summary>
        /// Delivery Month Charge
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "deliverycharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney DeliveryMonthCharge;

        /// <summary>
        /// Inter Commodity Spread Credit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "intercommoditycredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney InterCommodityCredit;

        /// <summary>
        /// Inter Exchange Spread Credit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "interexchangecredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney IntexCommodityCredit;


        /// <summary>
        /// Risk Initial
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "initialmargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskInitial;

        /// <summary>
        /// Risk Maintenance
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "maintenancemargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskMaintenance;
        [XmlIgnore]
        public bool RiskMaintenanceSpecified;

        /// <summary>
        /// All Long Scan Risk Scenario Values
        /// </summary>
        [XmlArray("scanlong", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("scan")]
        public ScenarioRiskValue[] ScanRiskValueLong;

        /// <summary>
        /// All Short Scan Risk Scenario Values
        /// </summary>
        [XmlArray("scanshort", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("scan")]
        public ScenarioRiskValue[] ScanRiskValueShort;

        /// <summary>
        /// All Scan Risk Scenario Values
        /// </summary>
        [XmlArray("scantiers", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("scan")]
        public ScenarioRiskValue[] ScanRiskValue;

        /// <summary>
        /// All Maturities Delta
        /// </summary>
        [XmlArray("maturity", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("delta")]
        public SpanPeriodDelta[] MaturityDelta;

        /// <summary>
        /// Inter Month Spread Charge Detail
        /// </summary>
        [XmlArray("strategychargedetail", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("strategyspread")]
        public SpanIntraCommoditySpreadParameter[] StrategySpreadParameter;

        /// <summary>
        /// Inter Month Spread Charge Detail
        /// </summary>
        [XmlArray("intracommoditychargedetail", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("intraspread")]
        public SpanIntraCommoditySpreadParameter[] IntraCommoditySpreadParameter;

        /// <summary>
        /// Delivery Month Charge Detail
        /// </summary>
        [XmlArray("deliverychargedetail", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("delivery")]
        public SpanDeliveryMonthChargeParameter[] DeliveryMonthChargeParameters;

        /// <summary>
        /// Commodity (Contract)
        /// </summary>
        [XmlArray("contracts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public override RiskParameter[] Parameters { get; set; }
    }

    /// <summary>
    /// Contract
    /// </summary>
    public class SpanContractParameter : RiskParameter
    {
        /// <summary>
        /// Contract Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        // Empty list (a contract element does not have sub-parameters )
        public override RiskParameter[] Parameters
        {
            get
            {
                return null;
            }
            set
            { }
        }
    }

    /// <summary>
    /// Delta
    /// </summary>
    public class SpanPeriodDelta
    {
        /// <summary>
        /// Period Name
        /// </summary>
        [XmlAttribute(AttributeName = "period", DataType = "string")]
        public string Period;

        [XmlAttribute(AttributeName = "net", DataType = "decimal")]
        public decimal DeltaNet { get; set; }

        [XmlAttribute(AttributeName = "netremaining", DataType = "decimal")]
        public decimal DeltaNetRemaining { get; set; }
    }

    /// <summary>
    /// Inter Commodity Spread
    /// </summary>
    public class SpanInterCommoditySpreadParameter
    {
        /// <summary>
        /// Spread Priority
        /// </summary>
        [XmlAttribute(AttributeName = "priority", DataType = "int")]
        public int SpreadPriority;

        /// <summary>
        /// Inter Commodity Spread Method
        /// </summary>
        [XmlAttribute(AttributeName = "method", DataType = "string")]
        public string InterSpreadMethod;

        /// <summary>
        /// Separated Spread Rate
        /// </summary>
        [XmlAttribute(AttributeName = "separaterate", DataType = "boolean")]
        public bool IsSeparatedSpreadRate;

        /// <summary>
        /// Spread Rate
        /// </summary>
        [XmlAttribute(AttributeName = "rate", DataType = "decimal")]
        public decimal SpreadRate;
        [XmlIgnore]
        public bool SpreadRateSpecified;

        /// <summary>
        /// Number of realized Inter Commodity Spread
        /// </summary>
        [XmlElement(ElementName = "numberofspread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal NumberOfSpread;
        [XmlIgnore]
        public bool NumberOfSpreadSpecified;

        /// <summary>
        /// Limit Number of Inter Commodity Spread
        /// </summary>
        [XmlElement(ElementName = "spreadlimit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal NumberOfSpreadLimit;
        [XmlIgnore]
        public bool NumberOfSpreadLimitSpecified;

        /// <summary>
        /// Offset Charge
        /// </summary>
        [XmlElement(ElementName = "offsetcharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal OffsetCharge { get; set; }
        [XmlIgnore]
        public bool OffsetChargeSpecified;

        /// <summary>
        /// Portfolio ScanRisk
        /// </summary>
        [XmlElement(ElementName = "portfolioscanrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal PortfolioScanRisk { get; set; }
        [XmlIgnore]
        public bool PortfolioScanRiskSpecified;

        /// <summary>
        /// Portfolio Risk
        /// </summary>
        [XmlElement(ElementName = "portfoliorisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal PortfolioRisk { get; set; }
        [XmlIgnore]
        public bool PortfolioRiskSpecified;

        /// <summary>
        /// Spread ScanRisk
        /// </summary>
        [XmlElement(ElementName = "spreadscanrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal SpreadScanRisk { get; set; }
        [XmlIgnore]
        public bool SpreadScanRiskSpecified;

        /// <summary>
        /// Delta 
        /// </summary>
        [XmlElement(ElementName = "delta", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaAvailable { get; set; }
        [XmlIgnore]
        public bool DeltaAvailableSpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray("legs", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("leg")]
        public SpanInterCommoditySpreadLegParameter[] LegParameters { get; set; }
    }

    /// <summary>
    /// Inter Commodity Spread Leg
    /// </summary>
    public class SpanInterCommoditySpreadLegParameter
    {
        /// <summary>
        /// Combined Commodity
        /// </summary>
        [XmlAttribute(AttributeName = "exchange", DataType = "string")]
        public string ExchangeAcronym;

        /// <summary>
        /// Combined Commodity
        /// </summary>
        [XmlAttribute(AttributeName = "combinedcommodity", DataType = "string")]
        public string CombinedCommodityCode;

        /// <summary>
        /// Spread Rate
        /// </summary>
        [XmlAttribute(AttributeName = "rate", DataType = "decimal")]
        public decimal SpreadRate;

        /// <summary>
        /// Tier Number
        /// </summary>
        [XmlAttribute(AttributeName = "tier", DataType = "int")]
        public int TierNumber;

        /// <summary>
        /// Maturity
        /// </summary>
        [XmlAttribute(AttributeName = "maturity", DataType = "string")]
        public string Maturity;

        /// <summary>
        /// Delta Per Spread
        /// </summary>
        [XmlAttribute(AttributeName = "deltaperspread", DataType = "decimal")]
        public decimal DeltaPerSpread;

        /// <summary>
        /// Delta Available
        /// </summary>
        [XmlElement(ElementName = "deltaavailable", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaAvailable;

        /// <summary>
        /// Delta Remaining
        /// </summary>
        [XmlElement(ElementName = "deltaremaining", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaRemaining;

        /// <summary>
        /// Delta Used
        /// </summary>
        [XmlElement(ElementName = "computeddeltaconsumed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ComputedDeltaConsumed;

        /// <summary>
        /// Realy Delta Used
        /// </summary>
        [XmlElement(ElementName = "realydeltaconsumed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal RealyDeltaConsumed;

        /// <summary>
        /// Weighted Risk
        /// </summary>
        [XmlElement(ElementName = "weightedrisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal WeightedRisk;

        /// <summary>
        /// Spread Credit
        /// </summary>
        [XmlElement(ElementName = "credit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal SpreadCredit;

        /// <summary>
        /// La jambe est-elle obligatoire
        /// </summary>
        [XmlElement(ElementName = "isrequired", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public bool IsRequired;

        /// <summary>
        /// La jambe est-elle la jambe cible
        /// </summary>
        [XmlElement(ElementName = "istarget", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public bool IsTarget;
    }

    /// <summary>
    /// Intra Commodity Spread
    /// </summary>
    public class SpanIntraCommoditySpreadParameter
    {
        /// <summary>
        /// Intra Commodity Spread Priority
        /// </summary>
        [XmlAttribute(AttributeName = "priority", DataType = "int")]
        public int SpreadPriority { get; set; }

        /// <summary>
        /// Number of Leg
        /// </summary>
        [XmlAttribute(AttributeName = "numberofleg", DataType = "int")]
        public int NumberOfLeg { get; set; }

        /// <summary>
        /// Charge Rate
        /// </summary>
        [XmlAttribute(AttributeName = "rate", DataType = "decimal")]
        public decimal ChargeRate { get; set; }

        /// <summary>
        /// Number of realized Spread
        /// </summary>
        [XmlElement(ElementName = "numberofspread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal NumberOfSpread { get; set; }

        /// <summary>
        /// Spread Charge
        /// </summary>
        [XmlElement(ElementName = "charge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal SpreadCharge { get; set; }

        /// <summary>
        /// Intra Commodity Spread Legs
        /// </summary>
        [XmlArray("legs", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("leg")]
        public SpanIntraCommoditySpreadLegParameter[] LegParameters { get; set; }
    }

    /// <summary>
    /// Intra Commodity Spread Leg
    /// </summary>
    public class SpanIntraCommoditySpreadLegParameter
    {
        /// <summary>
        /// Leg Number
        /// </summary>
        [XmlAttribute(AttributeName = "legnumber", DataType = "int")]
        public int LegNumber { get; set; }

        /// <summary>
        /// Leg Side
        /// </summary>
        [XmlAttribute(AttributeName = "legside", DataType = "string")]
        public string LegSide { get; set; }

        /// <summary>
        /// Tier Number
        /// </summary>
        [XmlAttribute(AttributeName = "tiernumber", DataType = "int")]
        public int TierNumber { get; set; }
        [XmlIgnore]
        public bool TierNumberSpecified;

        /// <summary>
        /// Maturity
        /// </summary>
        [XmlAttribute(AttributeName = "maturity", DataType = "string")]
        public string Maturity { get; set; }
        [XmlIgnore]
        public bool MaturitySpecified;

        /// <summary>
        /// Delta Per Spread
        /// </summary>
        [XmlAttribute(AttributeName = "deltaperspread", DataType = "decimal")]
        public decimal DeltaPerSpread { get; set; }

        /// <summary>
        /// Assumed Long Side
        /// </summary>
        [XmlElement(ElementName = "assumedlongside", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string AssumedLongSide { get; set; }

        /// <summary>
        /// Delta Long Available
        /// </summary>
        [XmlElement(ElementName = "deltalongavailable", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaLongAvailable { get; set; }

        /// <summary>
        /// Delta Short Available
        /// </summary>
        [XmlElement(ElementName = "deltashortavailable", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaShortAvailable { get; set; }

        /// <summary>
        /// Delta consumed
        /// </summary>
        [XmlElement(ElementName = "deltaconsumed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaConsumed { get; set; }

    }

    /// <summary>
    /// Delivery Month Charge Detail
    /// </summary>
    public class SpanDeliveryMonthChargeParameter
    {
        /// <summary>
        /// Echeance en livraison
        /// </summary>
        [XmlAttribute(AttributeName = "maturity", DataType = "string")]
        public string Maturity { get; set; }

        /// <summary>
        /// Signe de Delta à Considérer
        /// </summary>
        [XmlAttribute(AttributeName = "deltasign", DataType = "string")]
        public string DeltaSign { get; set; }
        [XmlIgnore]
        public bool DeltaSignSpecified;
        
        /// <summary>
        /// Taux de charge pour des deltas déjà utilisés dans des spreads
        /// </summary>
        [XmlAttribute(AttributeName = "consumedrate", DataType = "decimal")]
        public decimal ConsumedChargeRate { get; set; }

        /// <summary>
        /// Taux de charge pour des deltas non encore utilisés dans des spreads
        /// </summary>
        [XmlAttribute(AttributeName = "remainingrate", DataType = "decimal")]
        public decimal RemainingChargeRate { get; set; }

        /// <summary>
        /// Deltas déja utilisés pour des spreads
        /// </summary>
        [XmlElement(ElementName = "deltaconsumed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaNetUsed { get; set; }

        /// <summary>
        /// Deltas non encore utilisés pour des spreads
        /// </summary>
        [XmlElement(ElementName = "deltaremaining", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaNetRemaining { get; set; }

        /// <summary>
        /// Charge de livraison
        /// </summary>
        [XmlElement(ElementName = "deliverycharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeliveryCharge { get; set; }
    }

    /// <summary>
    /// One Factor Credit Detail
    /// </summary>
    /// PM 20150930 [21134] New
    public class SpanOneFactorCreditParameter
    {
        /// <summary>
        /// Final General Risk Lambda Max
        /// </summary>
        [XmlElement(ElementName = "finalGeneralRiskLMax", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal FinalGeneralRiskLMax { get; set; }

        /// <summary>
        /// Final General Risk Lambda Min
        /// </summary>
        [XmlElement(ElementName = "finalGeneralRiskLMin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal FinalGeneralRiskLMin { get; set; }

        /// <summary>
        /// Residual Iodiosyncratic Risk Max
        /// </summary>
        [XmlElement(ElementName = "idiosyncraticLMax", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal IdiosyncraticRiskLMax { get; set; }

        /// <summary>
        /// Residual Iodiosyncratic Risk Min
        /// </summary>
        [XmlElement(ElementName = "idiosyncraticLMin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal IdiosyncraticRiskLMin { get; set; }

        /// <summary>
        /// ScanRisk Offset Lambda Max
        /// </summary>
        [XmlElement(ElementName = "scanRiskOffsetLMax", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ScanRiskOffsetLMax { get; set; }

        /// <summary>
        /// ScanRisk Offset Lambda Min
        /// </summary>
        [XmlElement(ElementName = "scanRiskOffsetLMin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ScanRiskOffsetLMin { get; set; }

        /// <summary>
        /// ScanRisk Offset
        /// </summary>
        [XmlElement(ElementName = "scanRiskOffset", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ScanRiskOffset { get; set; }

        /// <summary>
        /// ScanRisk Global
        /// </summary>
        [XmlElement(ElementName = "globalScanRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal GlobalScanRisk { get; set; }

        /// <summary>
        /// Offset Percentage
        /// </summary>
        [XmlElement(ElementName = "offsetPercentage", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal OffsetPercentage { get; set; }

        /// <summary>
        /// Offset Percentage Cap
        /// </summary>
        [XmlElement(ElementName = "offsetMax", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal OffsetMax { get; set; }
    }

    #region ECC Concentration Risk Margin
    /// <summary>
    /// Détail du Concentration Risk Margin de l'ECC par Combined Commodity
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCConRiskMarginUnitParameter
    {
        #region Members
        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        [XmlAttribute(AttributeName = "combinedCommodity", DataType = "string")]
        public string CombinedCommodityStress { get; set; }

        /// <summary>
        /// Daily Market Volume
        /// </summary>
        [XmlAttribute(AttributeName = "marketVolume", DataType = "decimal")]
        public decimal DailyMarketVolume { get; set; }

        /// <summary>
        /// Absolute Cumulative Position Size
        /// </summary>
        [XmlElement(ElementName = "absCumulPos", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal AbsoluteCumulativePosition { get; set; }

        /// <summary>
        /// LiquidationPeriod
        /// </summary>
        [XmlElement(ElementName = "liquidationPeriod", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal LiquidationPeriod { get; set; }

        /// <summary>
        /// Weighted Absolute Cumulative Position Size
        /// </summary>
        [XmlElement(ElementName = "weightedAbsCumulPos", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal WeightedAbsCumulPosition { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Paramètres de volume de marché de l'ECC
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCMarketVolumeParameter
    {
        #region Members
        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        [XmlAttribute(AttributeName = "combinedCommodity", DataType = "string")]
        public string CombinedCommodityStress { get; set; }

        /// <summary>
        /// Market Volume
        /// </summary>
        [XmlAttribute(AttributeName = "volume", DataType = "decimal")]
        public decimal MarketVolume { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Montant du Concentration Risk Margin de l'ECC
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCConRiskMarginAmountParameter
    {
        /// <summary>
        /// Absolute Cumulative Position Size
        /// </summary>
        [XmlElement(ElementName = "absCumulPos", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal AbsoluteCumulativePosition { get; set; }

        /// <summary>
        /// Liquidation Period
        /// </summary>
        [XmlElement(ElementName = "liquidationPeriod", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal LiquidationPeriod { get; set; }

        /// <summary>
        /// Weighted Absolute Cumulative Position Size
        /// </summary>
        [XmlElement(ElementName = "weightedAbsCumulPos", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal WeightedAbsCumulPosition { get; set; }

        /// <summary>
        /// Concentration Risk Margin
        /// </summary>
        [XmlElement(ElementName = "concentrationRiskMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ConcentrationRiskMargin { get; set; }

        /// <summary>
        /// Détail pour chaque Combined Commodity Stress
        /// </summary>
        [XmlArray("combinedPos", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("marginPos")]
        public ECCConRiskMarginUnitParameter[] ConcentrationRiskMarginUnits { get; set; }
        [XmlIgnore]
        public bool ConcentrationRiskMarginUnitsSpecified { get; set; }
    }

    /// <summary>
    /// Détail du Concentration Risk Margin de l'ECC
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCConcentrationRiskMarginParameter
    {
        /// <summary>
        /// Additional Add On
        /// </summary>
        // PM 20190801 [24717] Ajout AdditionalAddOn
        [XmlAttribute(AttributeName = "additionalAddOn", DataType = "decimal")]
        public decimal AdditionalAddOn { get; set; }

        /// <summary>
        /// Concentration Risk Margin de l'ECC
        /// </summary>
        [XmlArray("calculationDetails", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("detail")]
        public ECCConRiskMarginAmountParameter[] ConcentrationRiskMarginAmounts { get; set; }

        /// <summary>
        /// Paramètre Market Volume de marché de l'ECC
        /// </summary>
        [XmlArray("marketVolumes", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("volume")]
        public ECCMarketVolumeParameter[] MarketVolume { get; set; }
    }
    #endregion ECC Concentration Risk Margin
    #endregion SPAN Method

    #region CBOE Margin Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode CBOE Margin
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class CboeMarginCalculationMethod : MarginCalculationMethod
    {
        private CboeMarginContractParameter[] m_parameters = null;

        /// <summary>
        /// Maintenance Initial indicateur
        /// </summary>
        // PM 20191025 [24983] Ajout
        [XmlAttribute(AttributeName = "maint_init", DataType = "string")]
        public string MaintenanceInitial;
        [XmlIgnore]
        public bool MaintenanceInitialSpecified;

        /// <summary>
        /// CBOE Contracts
        /// </summary>
        [XmlArray("contracts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new CboeMarginContractParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Contract margin détail
    /// </summary>
    public class CboeMarginContractParameter : RiskParameter
    {
        /// <summary>
        /// Contract Id
        /// </summary>
        [XmlAttribute(AttributeName = "OTCmlId", DataType = "int")]
        public int OTCmlId;

        /// <summary>
        /// Contract Displayname
        /// </summary>
        [XmlAttribute(AttributeName = "Name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Contract Symbol
        /// </summary>
        [XmlAttribute(AttributeName = "Sym", DataType = "string")]
        public string Symbol;

        /// <summary>
        /// Pourcentage de la valeur de l'option
        /// </summary>
        [XmlAttribute(AttributeName = "pctOpt", DataType = "decimal")]
        public decimal PctOptionValue;

        /// <summary>
        /// Pourcentage de la valeur du sous-jacent
        /// </summary>
        [XmlAttribute(AttributeName = "pctUnl", DataType = "decimal")]
        public decimal PctUnderlyingValue;

        /// <summary>
        /// Pourcentage minimum de la valeur du sous-jacent
        /// </summary>
        [XmlAttribute(AttributeName = "pctMin", DataType = "decimal")]
        public decimal PctMinimumUnderlyingValue;

        /// <summary>
        /// Prix de clôture du sous-jacent
        /// </summary>
        [XmlAttribute(AttributeName = "unlQuote", DataType = "decimal")]
        public decimal UnderlyingQuote;
        
        /// <summary>
        /// Normal Margin Initial
        /// </summary>
        // PM 20191025 [24983] Rename NormalMargin to NormalMarginInit
        [XmlElement(ElementName = "normalMarginAmountInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        //public MarginMoney NormalMargin;
        public MarginMoney NormalMarginInit;

        /// <summary>
        /// Normal margin Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout NormalMarginMaint
        [XmlElement(ElementName = "normalMarginAmountMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney NormalMarginMaint;

        /// <summary>
        /// Strategy Margin Initial
        /// </summary>
        // PM 20191025 [24983] Rename StrategyMargin to StrategyMarginInit
        [XmlElement(ElementName = "strategyMarginAmountInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        //public MarginMoney StrategyMargin;
        public MarginMoney StrategyMarginInit;

        /// <summary>
        /// Strategy margin Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout StrategyMarginMaint
        [XmlElement(ElementName = "strategyMarginAmountMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney StrategyMarginMaint;
        /// <summary>
        /// Détail du Déposit Normal
        /// </summary>
        [XmlArray("normalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public override RiskParameter[] Parameters { get; set; }

        /// <summary>
        /// Détail du Déposit de Strategie
        /// </summary>
        [XmlArray("strategyMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public CboeMarginStrategyMarginParameter[] Strategy;
    }

    /// <summary>
    /// Normal margin détail
    /// </summary>
    public class CboeMarginNormalMarginParameter : RiskParameter
    {
        /// <summary>
        /// Prix de clôture
        /// </summary>
        [XmlAttribute(AttributeName = "quote", DataType = "decimal")]
        public decimal Quote;

        /// <summary>
        /// Déposit unitaire Initial
        /// </summary>
        [XmlAttribute(AttributeName = "unitMarginInitial", DataType = "decimal")]
        // PM 20191025 [24983] Rename UnitMargin to UnitMarginInit
        //public decimal UnitMargin;
        public decimal UnitMarginInit;

        /// <summary>
        /// Déposit unitaire Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout UnitMarginMaint
        [XmlAttribute(AttributeName = "unitMarginMaintenance", DataType = "decimal")]
        public decimal UnitMarginMaint;

        /// <summary>
        /// Déposit minimum unitaire
        /// </summary>
        [XmlAttribute(AttributeName = "unitMinimumMargin", DataType = "decimal")]
        public decimal UnitMinimumMargin;

        /// <summary>
        /// Quantité initial
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        [XmlAttribute(AttributeName = "initialQuantity", DataType = "decimal")]
        public decimal InitialQuantity;

        /// <summary>
        /// Quantité
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        [XmlAttribute(AttributeName = "quantity", DataType = "decimal")]
        public decimal Quantity;

        /// <summary>
        /// Valeur du contrat
        /// </summary>
        [XmlElement(ElementName = "contractValue", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ContractValue;

        /// <summary>
        /// Minimum margin
        /// </summary>
        [XmlElement(ElementName = "minimumMarginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MinimumMargin;

        /// <summary>
        /// Initial Margin Amount
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountInit
        [XmlElement(ElementName = "marginAmountInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmountInit;

        /// <summary>
        /// Maintenance Margin Amount
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountMaint
        [XmlElement(ElementName = "marginAmountMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmountMaint;

        /// <summary>
        /// Asset
        /// </summary>
        [XmlElement(ElementName = "asset", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public CboeMarginAssetInformationParameter Asset;
        
        // Tableau vide (un asset n'a pas de sous-parametres)
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
    }

    /// <summary>
    /// Strategy margin détail
    /// </summary>
    public class CboeMarginStrategyMarginParameter
    {
        /// <summary>
        /// Type de stratégie
        /// </summary>
        [XmlAttribute(AttributeName = "type", DataType = "string")]
        public string StrategyType;

        /// <summary>
        /// Unit Margin Initial
        /// </summary>
        // PM 20191025 [24983] Rename UnitMargin to UnitMarginInit
        [XmlAttribute(AttributeName = "unitMarginInitial", DataType = "decimal")]
        //public decimal UnitMargin;
        public decimal UnitMarginInit;

        /// <summary>
        /// Unit Margin Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout UnitMarginMaint
        [XmlAttribute(AttributeName = "unitMarginMaintenance", DataType = "decimal")]
        public decimal UnitMarginMaint;

        /// <summary>
        /// Quantité du premier asset
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        [XmlAttribute(AttributeName = "quantityLeg1", DataType = "decimal")]
        public decimal QuantityFirstLeg;

        /// <summary>
        /// Quantité du second asset
        /// </summary>
        [XmlAttribute(AttributeName = "quantityLeg2", DataType = "decimal")]
        public decimal QuantitySecondLeg;

        /// <summary>
        /// Valeur du contrat de la première jambe
        /// </summary>
        [XmlElement(ElementName = "contractValueLeg1", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ContractValueFirstLeg;

        /// <summary>
        /// Valeur du contrat de la deuxième jambe
        /// </summary>
        [XmlElement(ElementName = "contractValueLeg2", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ContractValueSecondLeg;

        /// <summary>
        /// Asset de la première jambe
        /// </summary>
        [XmlElement(ElementName = "assetLeg1", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public CboeMarginAssetInformationParameter AssetFirstLeg;

        /// <summary>
        /// Asset de la deuxième jambe
        /// </summary>
        [XmlElement(ElementName = "assetLeg2", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public CboeMarginAssetInformationParameter AssetSecondLeg;

        /// <summary>
        /// Margin Amount
        /// </summary>
        [XmlElement(ElementName = "marginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmount;

        /// <summary>
        /// Initial Margin Amount
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountInit
        [XmlElement(ElementName = "marginAmountInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmountInit;

        /// <summary>
        /// Maintenance Margin Amount
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountMaint
        [XmlElement(ElementName = "marginAmountMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney MarginAmountMaint;
    }

    /// <summary>
    /// Asset détail
    /// </summary>
    public class CboeMarginAssetInformationParameter
    {
        /// <summary>
        /// Asset Id
        /// </summary>
        [XmlAttribute(AttributeName = "OTCmlId", DataType = "int")]
        public int OTCmlId;

        /// <summary>
        /// PutCall
        /// </summary>
        [XmlAttribute(AttributeName = "PutCall", DataType = "string")]
        public string PutCall;

        /// <summary>
        /// Maturity
        /// </summary>
        [XmlAttribute(AttributeName = "MMY", DataType = "string")]
        public string Maturity;

        /// <summary>
        /// Strike Price
        /// </summary>
        [XmlAttribute(AttributeName = "StrkPx", DataType = "decimal")]
        public decimal StrikePrice;

        /// <summary>
        /// Contract Multiplier
        /// </summary>
        [XmlAttribute(AttributeName = "Mult", DataType = "decimal")]
        public decimal ContractMultiplier;
    }
    #endregion CBOE Margin Method

    #region MEFFCOM2 Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode MEFFCOM2
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class MeffMarginCalculationMethod : MarginCalculationMethod
    {
        private MeffMarginClassParameter[] m_parameters = null;

        /// <summary>
        /// Inter Commodity Spread
        /// </summary>
        [XmlArray("interCommoditySpreads", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("spread")]
        public MeffInterCommoditySpreadParameter[] InterCommoditySpread;

        /// <summary>
        /// Meff Margin Class
        /// </summary>
        [XmlArray("marginClass", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("class")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new MeffMarginClassParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Margin Class détail
    /// </summary>
    public class MeffMarginClassParameter : RiskParameter
    {
        private MeffMarginMaturityParameter[] m_parameters = null;

        /// <summary>
        /// Margin Class Code
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string MarginClassCode;

        /// <summary>
        /// Price Fluctuation Type
        /// </summary>
        [XmlAttribute(AttributeName = "priceFluctType", DataType = "string")]
        public string PriceFluctuationType;

        /// <summary>
        /// Underlying Price
        /// </summary>
        [XmlAttribute(AttributeName = "underlyingPrice", DataType = "decimal")]
        public decimal UnderlyingPrice;
        [XmlIgnore]
        public bool UnderlyingPriceSpecified;

        /// <summary>
        /// Class Delta
        /// </summary>
        [XmlAttribute(AttributeName = "classDelta", DataType = "decimal")]
        public decimal ClassDelta;

        /// <summary>
        /// Delta To Offset
        /// </summary>
        [XmlAttribute(AttributeName = "remainingDelta", DataType = "decimal")]
        public decimal DeltaToOffset;

        /// <summary>
        /// Worst Case Scenario
        /// </summary>
        [XmlElement(ElementName = "worstScenario", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public int WorstCaseScenario;

        /// <summary>
        /// Accumulated Loss At Close
        /// </summary>
        [XmlElement(ElementName = "accumulatedLossAtClose", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal AccumulatedLossAtClose;
        
        /// <summary>
        /// Margin Class Potential Future Loss
        /// </summary>
        [XmlElement(ElementName = "classPotentialFutureLoss", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MarginClassPotentialFutureLoss;

        /// <summary>
        /// Delta Potential Future Loss
        /// </summary>
        [XmlElement(ElementName = "deltaPotentialFutureLoss", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaPotentialFutureLoss;
        
        /// <summary>
        /// Maximum Delta Offset
        /// </summary>
        [XmlElement(ElementName = "maxDeltaToOffset", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MaximumDeltaOffset;

        /// <summary>
        /// Net Position Margin Amount
        /// </summary>
        [XmlElement(ElementName = "netPositionMarginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney NetPositionMarginAmount;
        
        /// <summary>
        /// Time Spread Margin Amount
        /// </summary>
        [XmlElement(ElementName = "timeSpreadMarginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney TimeSpreadMarginAmount;

        /// <summary>
        /// Commodity Margin Amount
        /// </summary>
        [XmlElement(ElementName = "commodityMarginAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney CommodityMarginAmount;

        /// <summary>
        /// Inter Commodity Credit Amount
        /// </summary>
        [XmlElement(ElementName = "interCommodityCreditAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney InterCommodityCredit;

        /// <summary>
        /// Net Position Margins Values
        /// </summary>
        [XmlArray("netPositionMargins", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public ScenarioRiskValue[] NetPositionMarginValues;

        /// <summary>
        /// Time Spread Margins Values
        /// </summary>
        [XmlArray("timeSpreadMargins", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public ScenarioRiskValue[] TimeSpreadMarginValues;

        /// <summary>
        /// Commodity Margins Values
        /// </summary>
        [XmlArray("commodityMargins", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public ScenarioRiskValue[] CommodityMarginValues;

        /// <summary>
        /// Meff Margin Class
        /// </summary>
        [XmlArray("marginMaturity", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("maturity")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new MeffMarginMaturityParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Margin Maturity détail
    /// </summary>
    public class MeffMarginMaturityParameter : RiskParameter
    {
        /// <summary>
        /// Margin Class Code
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Future Price
        /// </summary>
        [XmlAttribute(AttributeName = "futurePrice", DataType = "decimal")]
        public decimal FuturePrice;

        /// <summary>
        /// Delta Values
        /// </summary>
        [XmlArray("deltas", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("delta")]
        public ScenarioRiskValue[] DeltaValues;

        // Tableau vide (une margin maturity n'a pas de sous-parametres)
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
    }

    /// <summary>
    /// Inter Commodity Spread
    /// </summary>
    public class MeffInterCommoditySpreadParameter
    {
        /// <summary>
        /// Spread Priority
        /// </summary>
        [XmlAttribute(AttributeName = "priority", DataType = "string")]
        public string Priority;

        /// <summary>
        /// Discount Type
        /// </summary>
        [XmlAttribute(AttributeName = "discountType", DataType = "string")]
        public string DiscountType;

        /// <summary>
        /// Number of realized Inter Commodity Spread
        /// </summary>
        [XmlElement(ElementName = "numberOfSpread", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal NumberOfSpread;

        /// <summary>
        /// Array of spread legs
        /// </summary>
        [XmlArray("legs", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("leg")]
        public MeffInterCommoditySpreadLegParameter[] LegParameters;
    }

    /// <summary>
    /// Inter Commodity Spread Leg
    /// </summary>
    public class MeffInterCommoditySpreadLegParameter
    {
        /// <summary>
        /// Margin Class Code
        /// </summary>
        [XmlAttribute(AttributeName = "class", DataType = "string")]
        public string MarginClassCode;

        /// <summary>
        /// Margin Credit
        /// </summary>
        [XmlAttribute(AttributeName = "marginCredit", DataType = "decimal")]
        public decimal MarginCredit;

        /// <summary>
        /// Delta Per Spread
        /// </summary>
        [XmlAttribute(AttributeName = "deltaPerSpread", DataType = "decimal")]
        public decimal DeltaPerSpread;

        /// <summary>
        /// Delta Available
        /// </summary>
        [XmlElement(ElementName = "deltaAvailable", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaAvailable;

        /// <summary>
        /// Delta Remaining
        /// </summary>
        [XmlElement(ElementName = "deltaRemaining", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaRemaining;

        /// <summary>
        /// Delta Used
        /// </summary>
        [XmlElement(ElementName = "deltaConsumed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal DeltaConsumed;

        /// <summary>
        /// Spread Credit
        /// </summary>
        [XmlElement(ElementName = "spreadCredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal SpreadCredit;

    }
    #endregion MEFFCOM2 Method

    #region NASDAQ OMX Method
    #region OMX RCaR Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode OMX RCaR
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class OMXMarginCalculationMethod : MarginCalculationMethod
    {
        /// <summary>
        /// 
        /// </summary>
        private OMXUnderlyingSymbolParameter[] _parameters = null;

        /// <summary>
        /// Détail par symbol
        /// </summary>
        [XmlArray("symbols", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("symbol")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = new OMXUnderlyingSymbolParameter[value.Length];
                value.CopyTo(_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Symbol Margin Detail
    /// <para></para>
    /// </summary>
    public class OMXUnderlyingSymbolParameter : RiskParameter
    {
        /// <summary>
        /// Underlying Symbol
        /// <para></para>
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Symbol;

        /// <summary>
        /// 
        /// </summary>
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
    }
    #endregion OMX RCaR Method

    #region NOMX_CFM Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode NOMX_CFM
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    // PM 20190222 [24326] New
    public class NOMXCFMCalculationMethod : MarginCalculationMethod
    {
        private NOMXCFMResultCurveAmount[] m_parameters = null;

        /// <summary>
        /// Calculation Detail
        /// </summary>
        [XmlElement(ElementName = "calculationDetail", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public NOMXCFMResultDetail CalculationDetail { get; set; }

        /// <summary>
        /// Detailled Amounts
        /// </summary>
        [XmlArray("detailledAmounts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("amount")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new NOMXCFMResultCurveAmount[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
    }

    /// <summary>
    /// Détail des montant des curves finales
    /// </summary>
    // PM 20190222 [24326] New
    public class NOMXCFMResultCurveAmount : NOMXCFMCurveParameter
    {

    }

    /// <summary>
    /// Curve de la hiérarchie de correlation
    /// </summary>
    // PM 20190222 [24326] New
    public class NOMXCFMResultCorrelationCurve : NOMXCFMCurveParameter
    {
        [XmlArray("childCurves", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("curve")]
        public NOMXCFMResultCorrelationCurve[] Curve { get; set; }
    }
    
    /// <summary>
    /// Détail des curves du calcul
    /// </summary>
    // PM 20190222 [24326] New
    public class NOMXCFMResultDetail
    {
        /// <summary>
        /// Détail des curves sans paramètres de correlation
        /// </summary>
        [XmlArray("singleCurves", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("curve")]
        public NOMXCFMCurveParameter[] SingleCurves { get; set; }

        /// <summary>
        /// Hiérarchie des curves avec correlation
        /// </summary>
        [XmlArray("correlationCurves", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("curve")]
        public NOMXCFMResultCorrelationCurve[] CorrelationCurves { get; set; }
    }

    /// <summary>
    /// Détail d'une curve
    /// </summary>
    // PM 20190222 [24326] New
    public class NOMXCFMCurveParameter : RiskParameter
    {
        /// <summary>
        /// Curve Name
        /// </summary>
        [XmlAttribute(AttributeName = "curveName", DataType = "string")]
        public string CurveName { get; set; }

        /// <summary>
        /// Margin Class
        /// </summary>
        [XmlAttribute(AttributeName = "marginClass", DataType = "string")]
        public string MarginClass { get; set; }
        
        /// <summary>
        /// Overlap PC1
        /// </summary>
        [XmlAttribute(AttributeName = "overlapPC1", DataType = "int")]
        public int OverlapPC1;
        [XmlIgnore]
        public bool OverlapPC1Specified;

        /// <summary>
        /// Overlap PC2
        /// </summary>
        [XmlAttribute(AttributeName = "overlapPC2", DataType = "int")]
        public int OverlapPC2;
        [XmlIgnore]
        public bool OverlapPC2Specified;

        /// <summary>
        /// Overlap PC3
        /// </summary>
        [XmlAttribute(AttributeName = "overlapPC3", DataType = "int")]
        public int OverlapPC3;
        [XmlIgnore]
        public bool OverlapPC3Specified;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray("scenarios", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("scenario")]
        public NOMXCFMScenarioParameter[] Scenarios { get; set; }

        // Vide
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
    }

    /// <summary>
    /// Détail d'un scénario d'une curve
    /// </summary>
    // PM 20190222 [24326] New
    public class NOMXCFMScenarioParameter
    {
        #region Members
        /// <summary>
        /// Point Number for PC1
        /// </summary>
        [XmlAttribute(AttributeName = "pc1", DataType = "int")]
        public int PC1PointNo;
        /// <summary>
        /// Point Number for PC2
        /// </summary>
        [XmlAttribute(AttributeName = "pc2", DataType = "int")]
        public int PC2PointNo;
        /// <summary>
        /// Point Number for PC3
        /// </summary>
        [XmlAttribute(AttributeName = "pc3", DataType = "int")]
        public int PC3PointNo;
        /// <summar>
        /// Low
        /// </summary>
        [XmlAttribute(AttributeName = "low", DataType = "decimal")]
        public decimal LowValue;
        /// <summary>
        /// Middle
        /// </summary>
        [XmlAttribute(AttributeName = "middle", DataType = "decimal")]
        public decimal MiddleValue;
        /// <summary>
        /// High
        /// </summary>
        [XmlAttribute(AttributeName = "high", DataType = "decimal")]
        public decimal HighValue;
        #endregion Members
    }
    #endregion NOMX_CFM Method
    #endregion NASDAQ OMX Method

    #region PRISMA Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode Prisma
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class PrismaMarginCalculationMethod : MarginCalculationMethod
    {
        private PrismaLiquidGroupParameter[] m_parameters = null;

        /// <summary>
        /// Version de Prisma
        /// </summary>
        //PM 20150417 [20957] Add PrismaRelease
        [XmlAttribute(AttributeName = "release", DataType = "string")]
        public string PrismaRelease;

        /// <summary>
        /// Liquidation Groups
        /// </summary>
        [XmlArray("liquidationGroup", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("group")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                if (value != default(RiskParameter[]))
                {
                    m_parameters = new PrismaLiquidGroupParameter[value.Length];
                    value.CopyTo(m_parameters, 0);
                }
            }
        }
    }

    /// <summary>
    /// Class contenant le détail du calcul pour un Liquidation Group d'un déposit évalué avec la méthode Prisma
    /// </summary>
    public class PrismaLiquidGroupParameter : RiskParameter
    {
        private PrismaLiquidGroupSplitParameter[] m_parameters = null;

        /// <summary>
        /// Id interne du groupe de liquidation
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "int")]
        public int IdLg;

        /// <summary>
        /// Liquidation Group Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Currency Type Flag
        /// </summary>
        [XmlAttribute(AttributeName = "currencyTypeFlag", DataType = "string")]
        public string CurrencyTypeFlag;

        /// <summary>
        /// Clearing Currency
        /// </summary>
        [XmlAttribute(AttributeName = "clearingCurrency", DataType = "string")]
        public string ClearingCurrency;
        
        /// <summary>
        /// Initial Margin
        /// </summary>
        [XmlArray("initialMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public MarginMoney[] InitialMargin { get; set; }

        // PM 20150907 [21236] Ajout PremiumMargin et MarginRequirement
        /// <summary>
        /// Premium Margin
        /// </summary>
        [XmlArray("premiumMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public MarginMoney[] PremiumMargin { get; set; }

        /// <summary>
        /// Margin Requirement
        /// </summary>
        // PM 20200129 Rename de "marginRequirement" en "marginRequirementDetail" car sinon problème de serialisation en log Full
        //[XmlArray("marginRequirement", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArray("marginRequirementDetail", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public MarginMoney[] MarginRequirement { get; set; }

        /// <summary>
        /// Liquidation Group Splits
        /// </summary>
        [XmlArray("liquidationGroupSplit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("split")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                if (value != default(RiskParameter[]))
                {
                    m_parameters = new PrismaLiquidGroupSplitParameter[value.Length];
                    value.CopyTo(m_parameters, 0);
                }
            }
        }
    }

    /// <summary>
    /// Class contenant le détail du calcul pour un Liquidation Group Split d'un déposit évalué avec la méthode Prisma
    /// </summary>
    public class PrismaLiquidGroupSplitParameter : RiskParameter
    {
        private PrismaRiskMeasureSetParameter[] m_parameters = null;

        /// <summary>
        /// Id interne du split de groupe de liquidation
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "int")]
        public int IdLgs;

        /// <summary>
        /// Liquidation Group Split Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Aggregation Method
        /// </summary>
        [XmlAttribute(AttributeName = "aggregationMethod", DataType = "string")]
        public string AggregationMethod;

        /// <summary>
        /// Risk Method
        /// </summary>
        [XmlAttribute(AttributeName = "riskMethod", DataType = "string")]
        public string RiskMethod;
        
        /// <summary>
        /// Liquidity Risk asset informations
        /// </summary>
        [XmlArray("assetLiquidityRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("asset")]
        public PrismaAssetLiquidityRisk[] AssetLiquidityRisk { get; set; }

        /// <summary>
        /// Market Risk
        /// </summary>
        [XmlArray("marketRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("risk")]
        public MarginMoney[] MarketRisk { get; set; }

        /// <summary>
        /// Time To Expiry Adjustment
        /// </summary>
        [XmlArray("timeToExpiryAdjustment", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("risk")]
        // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustment
        public MarginMoney[] TimeToExpiryAdjustment { get; set; }

        /// <summary>
        /// Liquidity Risk
        /// </summary>
        [XmlArray("liquidityRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("risk")]
        public MarginMoney[] LiquidityRisk { get; set; }

        /// <summary>
        /// Initial Margin
        /// </summary>
        [XmlArray("initialMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public MarginMoney[] InitialMargin { get; set; }

        /// <summary>
        /// Premium Margin
        /// </summary>
        // PM 20140619 [19911] New
        // PM 20150907 [21236] XmlArrayItem("amount") >= XmlArrayItem("margin")
        [XmlArray("premiumMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public MarginMoney[] PremiumMargin { get; set; }

        /// <summary>
        /// Present Value
        /// </summary>
        // PM 20200826 [25467] New
        [XmlArray("presentValue", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("amount")]
        public MarginMoney[] PresentValue { get; set; }

        /// <summary>
        /// Maximal Lost
        /// </summary>
        // PM 20200826 [25467] New
        [XmlArray("maximalLost", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("amount")]
        public MarginMoney[] MaximalLost { get; set; }

        /// <summary>
        /// Long Option Credit
        /// </summary>
        // PM 20140618 [19911] New
        [XmlArray("longOptionCredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("amount")]
        public MarginMoney[] LongOptionCredit { get; set; }

        /// <summary>
        /// Total Initial Margin
        /// </summary>
        // PM 20140618 [19911] New
        [XmlArray("totalInitialMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("margin")]
        public MarginMoney[] TotalInitialMargin { get; set; }

        /// <summary>
        /// Risk Measure Set
        /// </summary>
        [XmlArray("riskMeasureSet", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("rms")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                if (value != default(RiskParameter[]))
                {
                    m_parameters = new PrismaRiskMeasureSetParameter[value.Length];
                    value.CopyTo(m_parameters, 0);
                }
            }
        }
    }

    /// <summary>
    /// Class contenant le détail du calcul pour un Risk Measure Set d'un déposit évalué avec la méthode Prisma
    /// </summary>
    public class PrismaRiskMeasureSetParameter : RiskParameter
    {
        private PrismaSubSampleParameter[] m_parameters = null;

        /// <summary>
        /// Id interne du jeu de mesure de risque
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "int")]
        public int IdRms;

        /// <summary>
        /// Identifier du jeu de mesure de risque
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Type du jeu de mesure de risque
        /// </summary>
        [XmlAttribute(AttributeName = "historicalStressed", DataType = "string")]
        public string HistoricalStressed;

        /// <summary>
        /// Aggregation Method
        /// </summary>
        [XmlAttribute(AttributeName = "aggregationMethod", DataType = "string")]
        public string AggregationMethod;

        /// <summary>
        /// Niveau de confiance de la mesure de risque
        /// </summary>
        [XmlAttribute(AttributeName = "confidenceLevel", DataType = "decimal")]
        public decimal ConfidenceLevel;

        /// <summary>
        /// Utiliser ou non la robustesse pour la mesure de risque
        /// </summary>
        [XmlAttribute(AttributeName = "useRobustness", DataType = "boolean")]
        public bool IsUseRobustness;

        /// <summary>
        /// Facteur d'échelle pour la robustesse
        /// </summary>
        [XmlAttribute(AttributeName = "scalingFactor", DataType = "decimal")]
        public decimal ScalingFactor;

        /// <summary>
        /// Niveau de confiance utilisé pour le calcul du correlation break
        /// </summary>
        [XmlAttribute(AttributeName = "cbConfidenceLevel", DataType = "decimal")]
        public decimal CorrelationBreakConfidenceLevel;

        /// <summary>
        /// Taille de la sous fenêtre utilisée pour le calcul du correlation break 
        /// </summary>
        [XmlAttribute(AttributeName = "cbSubWindowSize", DataType = "int")]
        public int CorrelationBreakSubWindow;

        /// <summary>
        /// Multiplier pour le calcul de l'ajustement de correlation break
        /// </summary>
        [XmlAttribute(AttributeName = "cbKappa", DataType = "decimal")]
        public decimal CorrelationBreakMultiplier;

        /// <summary>
        /// Ajustement de correlation break minimum
        /// </summary>
        [XmlAttribute(AttributeName = "cbLowerBound", DataType = "decimal")]
        public decimal CorrelationBreakMin;

        /// <summary>
        /// Ajustement de correlation break maximum
        /// </summary>
        [XmlAttribute(AttributeName = "cbUpperBound", DataType = "decimal")]
        public decimal CorrelationBreakMax;

        /// <summary>
        /// Application ou non du composant de liquidité à la mesure de risque
        /// </summary>
        [XmlAttribute(AttributeName = "isLiquidityComponent", DataType = "boolean")]
        public bool IsLiquidityComponent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean AlphaConfidenceLevelSpecified;
        /// <summary>
        /// Niveau de confiance utilisé pour le calcul de l'alpha factor
        /// </summary>
        [XmlAttribute(AttributeName = "alphaConfidenceLevel", DataType = "decimal")]
        public decimal AlphaConfidenceLevel;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean AlphaFloorSpecified;
        /// <summary>
        /// Valeur minimum de l'alpha factor
        /// </summary>
        [XmlAttribute(AttributeName = "alphaFloor", DataType = "decimal")]
        public decimal AlphaFloor;

        /// <summary>
        /// Market Risk Component
        /// </summary>
        [XmlElement(ElementName = "marketRiskComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MarketRiskComponent;

        /// <summary>
        /// Scaled (Weigthed) Market Risk Component 
        /// </summary>
        [XmlElement(ElementName = "scaledMarketRiskComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ScaledMarketRiskComponent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean ValueAtRiskLiquidityComponentSpecified;
        /// <summary>
        /// VaR(Value at Risk) using du Liquidity Risk properties
        /// </summary>
        [XmlElement(ElementName = "valueAtRiskLiqComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ValueAtRiskLiquidityComponent;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean AlphaFactorSpecified;

        /// <summary>
        /// Valeur minimum de l'alpha factor
        /// </summary>
        [XmlElement(ElementName = "alphaFactor", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal AlphaFactor;

        /// <summary>
        /// Sub Sample
        /// </summary>
        [XmlArray("subSample", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("sample")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                if (value != default(RiskParameter[]))
                {
                    m_parameters = new PrismaSubSampleParameter[value.Length];
                    value.CopyTo(m_parameters, 0);
                }
            }
        }
    }

    /// <summary>
    /// Class contenant le détail du calcul pour un Sub-Sample d'un déposit évalué avec la méthode Prisma
    /// </summary>
    public class PrismaSubSampleParameter : RiskParameter
    {
        /// <summary>
        /// Compression Error
        /// </summary>
        [XmlAttribute(AttributeName = "compressionError", DataType = "decimal")]
        public decimal CompressionError;
        
        /// <summary>
        /// Value At Risk
        /// </summary>
        [XmlElement(ElementName = "valueAtRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ValueAtRisk;

        /// <summary>
        /// Value At Risk Scaled
        /// </summary>
        [XmlElement(ElementName = "valueAtRiskScaled", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ValueAtRiskScaled;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean ValueAtRiskLiquidityComponentSpecified;

        /// <summary>
        /// Value At Risk (Liquidity component)
        /// </summary>
        [XmlElement(ElementName = "valueAtRiskLiqComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal ValueAtRiskLiquidityComponent;

        /// <summary>
        /// Pure Market Risk
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout PureMarketRisk
        [XmlElement(ElementName = "pureMarketRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal PureMarketRisk;

        /// <summary>
        /// Mean Excess Risk
        /// </summary>
        [XmlElement(ElementName = "meanExcessRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MeanExcessRisk;

        /// <summary>
        /// Correlation Break Adjustment
        /// </summary>
        [XmlElement(ElementName = "correlationBreak", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal CorrelationBreakAdjustment;

        /// <summary>
        /// Correlation Break Lower Bound
        /// </summary>
        [XmlElement(ElementName = "cbLowerBound", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal CbLowerBound;

        /// <summary>
        /// Correlation Break Upper Bound
        /// </summary>
        [XmlElement(ElementName = "cbUpperBound", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal CbUpperBound;

        /// <summary>
        /// Compression Adjustment
        /// </summary>
        [XmlElement(ElementName = "compressionAdjustment", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal CompressionAdjustment;

        /// <summary>
        /// Market Risk Component
        /// </summary>
        [XmlElement(ElementName = "marketRiskComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MarketRiskComponent;

        // Vide
        public override RiskParameter[] Parameters
        {
            get { return null;}
            set { }
        }
    }
    
    /// <summary>
    ///  Représente les éléments de calcul utilisés pour évaluer le Liquidity Risk pour un asset
    /// </summary>
    public class PrismaAssetLiquidityRisk
    {
        /// <summary>
        /// Identifier de l'asset
        /// </summary>
        [XmlAttribute(AttributeName = "ID", DataType = "string")]
        public string ID;

        /// <summary>
        /// Trade Unit 
        /// </summary>
        [XmlAttribute(AttributeName = "tradeUnit", DataType = "decimal")]
        public Decimal tradeUnit;

        /// <summary>
        /// Net gross ratio
        /// </summary>
        [XmlElement(ElementName = "ngr", DataType = "decimal")]
        public Decimal netGrossRatio;

        /// <summary>
        /// Liquidity Premium
        /// </summary>
        [XmlElement(ElementName = "liqPrem", DataType = "decimal")]
        public Decimal liquidityPremium;

        /// <summary>
        /// Liquidity Factor
        /// </summary>
        [XmlElement(ElementName = "liqFctr", DataType = "decimal")]
        public Decimal liquidityFactor;

        /// <summary>
        /// Risk Measure
        /// <para>figure using IVAR (Instrument VaR)</para>
        /// </summary>
        [XmlElement(ElementName = "riskMeasure", DataType = "decimal")]
        public Decimal riskMeasure;

        /// <summary>
        /// Liquidity Adjustment
        /// </summary>
        [XmlElement(ElementName = "liqAdj", DataType = "decimal")]
        public Decimal liquidityAdjustment;

        /// <summary>
        /// Additional Risk Measure
        /// <para>figure using AIVAR (Additional Instrument VaR)</para>
        /// </summary>
        [XmlElement(ElementName = "additionalRiskMeasure", DataType = "decimal")]
        public decimal additionalRiskMeasure;
    }

    #endregion SPAN Method

    #region IMSM Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode IMSM
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class IMSMCalculationMethod : MarginCalculationMethod
    {
        /// <summary>
        /// Business Center
        /// </summary>
        [XmlAttribute(AttributeName = "businessCenter", DataType = "string")]
        public string BusinessCenter;

        /// <summary>
        /// Date du deposit IMSM
        /// </summary>
        [XmlAttribute(AttributeName = "imsmDate", DataType = "date")]
        public DateTime IMSMDate;

        /// <summary>
        /// Paramètres généraux de la méthode IMSM
        /// </summary>
        [XmlElement(ElementName = "riskParameter", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public IMSMGlobalParameterDetail IMSMParameter;
        [XmlIgnore]
        public bool IMSMParameterSpecified;

        /// <summary>
        /// Données statistiques
        /// </summary>
        [XmlElement(ElementName = "statisticData", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public IMSMExposureDetail Exposure;
        [XmlIgnore]
        public bool ExposureSpecified;

        /// <summary>
        /// Détail du calcul
        /// </summary>
        // PM 20200910 [25482] Ajout
        [XmlElement(ElementName = "CESMCalculation", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public CESMCalculationDetail CESMCalculation;
        [XmlIgnore]
        public bool CESMCalculationSpecified;

        /// <summary>
        /// Détail du calcul
        /// </summary>
        // PM 20200910 [25482] Refactoring
        [XmlElement(ElementName = "IMSMCalculation", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public IMSMCalculationDetail IMSMCalculation;
        [XmlIgnore]
        public bool IMSMCalculationSpecified;

        // Vide
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
    }

    /// <summary>
    /// Paramètres généraux de la méthode IMSM
    /// </summary>
    public sealed class IMSMGlobalParameterDetail
    {
        #region Members
        /// <summary>
        /// Calcul uniquement du Current Exposure Spot Margin de l'ECC
        /// </summary>
        // PM 20200910 [25482] Ajout IsCalcCESMOnly
        [XmlElement(ElementName = "isCESMOnly", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public bool IsCalcCESMOnly;
        /// <summary>
        /// Indicateur de prise en considération des jours fériés
        /// </summary>
        [XmlElement(ElementName = "isWithHolidayAdjustment", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public bool IsWithHolidayAdjustment;
        /// <summary>
        /// Taille de la fenêtre de prise en compte des données statistiques
        /// </summary>
        [XmlElement(ElementName = "windowSizeStatistic", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public int WindowSizeStatistic;
        /// <summary>
        /// Taille de la fenêtre de la partie maximum du calcul de l'IMSM
        /// </summary>
        [XmlElement(ElementName = "windowSizeMaximum", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public int WindowSizeMaximum;
        /// <summary>
        /// Paramètre EWMA (Exponentially Weighted Moving Average / Moyennes Mobiles Pondérées Expontiellement) pour calculer la volatilité dans le calcul de l'écart-type
        /// </summary>
        [XmlElement(ElementName = "ewmaFactor", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal EWMAFactor;
        /// <summary>
        /// Multiplicateur pour l'écart-type utilisé dans la partie statistique du calcul de l'IMSM
        /// </summary>
        [XmlElement(ElementName = "alphaFactor", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal Alpha;
        /// <summary>
        /// Multiplicateur pour la partie déterministe du calcul de l'IMSM
        /// </summary>
        [XmlElement(ElementName = "betaFactor", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal Beta;
        /// <summary>
        /// Minimum absolu initial de l'IMSM pour les "MinIMSMInitialWindowSize"(30) premiers jours après l'admission
        /// </summary>
        [XmlElement(ElementName = "minimumInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MinIMSMInitial;
        /// <summary>
        /// Nombre de jours pendant lesquels appliquer le montant minimum absolu initial de l'IMSM
        /// </summary>
        [XmlElement(ElementName = "minimumInitialNumberOfDays", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public int MinIMSMInitialWindowSize;
        /// <summary>
        /// Minimum absolu de l'IMSM
        /// </summary>
        [XmlElement(ElementName = "minimum", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MinIMSM;
        /// <summary>
        /// Paramètres de calcul pour le CESM
        /// </summary>
        /// PM 20170808 [23371] New
        [XmlArray("cesmParameters", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public IMSMCESMParameter[] CESMParameters;
        /// <summary>
        /// Taux de change
        /// </summary>
        /// PM 20170808 [23371] New
        [XmlArray("exchangeRates", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("rate")]
        public IMExchangeRateParameter[] ExchangeRateParameter;
        #endregion Members
    }

    /// <summary>
    /// Paramètres pour le calcul CESM
    /// </summary>
    /// PM 20170808 [23371] New
    public sealed class IMSMCESMParameter
    {
        #region Members
        /// <summary>
        /// Identifiant du Commodity Contract
        /// </summary>
        [XmlAttribute(AttributeName = "identifier", DataType = "string")]
        public string ContractIdentifier;

        /// <summary>
        /// Id interne du Commodity Contract
        /// </summary>
        [XmlAttribute(AttributeName = "OTCmlId", DataType = "int")]
        public int IdCC;

        /// <summary>
        /// Margin Parameter pour les Achats
        /// </summary>
        [XmlAttribute(AttributeName = "mpBuy", DataType = "decimal")]
        public decimal MarginParameterBuy;

        /// <summary>
        /// Margin Parameter pour les Ventes
        /// </summary>
        [XmlAttribute(AttributeName = "mpSell", DataType = "decimal")]
        public decimal MarginParameterSell;
        #endregion Members
    }

    /// <summary>
    /// Taux de change
    /// </summary>
    /// PM 20170808 [23371] New
    public sealed class IMExchangeRateParameter
    {
        #region Members
        /// <summary>
        /// Devise 1
        /// </summary>
        [XmlAttribute(AttributeName = "currency1", DataType = "string")]
        public string Currency1 { get; set; }
        /// <summary>
        /// Devise 2
        /// </summary>
        [XmlAttribute(AttributeName = "currency2", DataType = "string")]
        public string Currency2 { get; set; }
        /// <summary>
        /// Taux de change
        /// </summary>
        [XmlAttribute(AttributeName = "rate", DataType = "decimal")]
        public decimal Rate { get; set; }
        [XmlIgnore]
        public bool RateSpecified;
        /// <summary>
        /// Base de cotation
        /// </summary>
        [XmlAttribute(AttributeName = "base", DataType = "string")]
        public string QuoteBasis { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Données statistiques
    /// </summary>
    public sealed class IMSMExposureDetail
    {
        /// <summary>
        /// Date minimum des statistiques
        /// </summary>
        [XmlAttribute(AttributeName = "minimumDate", DataType = "date")]
        public DateTime WindowDateMin { set; get; }

        /// <summary>
        /// Ensemble des "Current Exposure"
        /// </summary>
        /// PM 20170808 [23371] Ajout
        [XmlArray("currentExposure", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("contract")]
        public IMSMCurrentExposureItem[] CurrentExposure { set; get; }
        [XmlIgnore]
        public bool CurrentExposureSpecified;

        /// <summary>
        /// Ensemble des "Exposure" compris dans la fenêtre de calcul
        /// </summary>
        [XmlArray("exposure", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("item")]
        public IMSMExposureItem[] Exposure { set; get; }
        [XmlIgnore]
        public bool ExposureSpecified;

        /// <summary>
        /// "T0Exposure"
        /// </summary>
        [XmlElement(ElementName = "t0Exposure", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal T0Exposure { set; get; }
    }

    /// <summary>
    /// Donnée statistique unitaire
    /// </summary>
    public sealed class IMSMExposureItem
    {
        /// <summary>
        /// Date de la statistique
        /// </summary>
        [XmlAttribute(AttributeName = "date", DataType = "date")]
        public DateTime Date { set; get; }

        /// <summary>
        /// Valeur de la statistique
        /// </summary>
        [XmlAttribute(AttributeName = "value", DataType = "decimal")]
        public decimal Value { set; get; }
    }

    /// <summary>
    /// Donnée statistique courante
    /// </summary>
    /// PM 20170808 [23371] New
    public sealed class IMSMCurrentExposureItem
    {
        /// <summary>
        /// Id interne du Commodity Contract
        /// </summary>
        [XmlAttribute(AttributeName = "OTCmlId", DataType = "int")]
        public int IdCC { set; get; }

        /// <summary>
        /// Identifiant interne de l'Asset du Commodity Contract
        /// </summary>
        // PM 20200910 [25482] Ajout IdAsset
        [XmlAttribute(AttributeName = "assetId", DataType = "int")]
        public int IdAsset { set; get; }

        /// <summary>
        /// Identifiant de l'Asset du Commodity Contract
        /// </summary>
        // PM 20200910 [25482] Ajout IdAsset
        [XmlAttribute(AttributeName = "assetIdentifier", DataType = "string")]
        public string AssetIdentifier { set; get; }

        /// <summary>
        /// Exposure à l'achat
        /// </summary>
        [XmlAttribute(AttributeName = "buy", DataType = "decimal")]
        public decimal ExposureBuy { set; get; }

        /// <summary>
        ///  Exposure à la vente
        /// </summary>
        [XmlAttribute(AttributeName = "sell", DataType = "decimal")]
        public decimal ExposureSell { set; get; }
    }

    /// <summary>
    /// Données du calcul CESM
    /// </summary>
    // PM 20200910 [25482] Ajout
    public sealed class CESMCalculationDetail
    {
        /// <summary>
        /// Montant du déposit CESM
        /// </summary>
        [XmlElement(ElementName = "cesmAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal CESMAMount { set; get; }
    }

    /// <summary>
    /// Données du calcul IMSM
    /// </summary>
    public sealed class IMSMCalculationDetail
    {
        /// <summary>
        /// Date effective du calcul du déposit
        /// </summary>
        [XmlElement(ElementName = "effectiveDate", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public DateTime EffectiveImsmDate { set; get; }

        /// <summary>
        /// Date de début d'existance d'une activité
        /// </summary>
        /// PM 20170602 [23212] Renommage de FirstActivityDate en AgreementDate
        [XmlElement(ElementName = "agreementDate", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public DateTime AgreementDate { set; get; }

        /// <summary>
        /// Nombre de jours pour l'ajustement jour férié
        /// </summary>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        [XmlElement(ElementName = "holydayAdjNoDays", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal HolydayAdjDays { set; get; }

        /// <summary>
        /// Montant de l'ajustement jour férié
        /// </summary>
        [XmlElement(ElementName = "holydayAdjAmount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal HolydayAdjAmount { set; get; }

        /// <summary>
        /// T0Exposure
        /// </summary>
        [XmlElement(ElementName = "t0Exposure", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal T0Exposure { set; get; }

        /// <summary>
        /// Moyenne des statistiques
        /// </summary>
        [XmlElement(ElementName = "mean", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal Mean { set; get; }

        /// <summary>
        /// Nombre de datapoint
        /// </summary>
        [XmlElement(ElementName = "dataPointNo", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal NoDataPoint { set; get; }

        /// <summary>
        /// Standard Deviation
        /// </summary>
        [XmlElement(ElementName = "standardDeviation", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal StandardDeviation { set; get; }

        /// <summary>
        /// Standard Deviation with Security factor
        /// </summary>
        [XmlElement(ElementName = "sds", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal SDS { set; get; }

        /// <summary>
        /// Maximum sur la fenêtre réduite de données
        /// </summary>
        [XmlElement(ElementName = "maxOfShortWindow", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MaxShortWindow { set; get; }

        /// <summary>
        /// Beta factor * Maximum sur la fenêtre réduite de données
        /// </summary>
        [XmlElement(ElementName = "betaMax", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal BetaMax { set; get; }
        
        /// <summary>
        /// Montant maximum du déposit
        /// </summary>
        [XmlElement(ElementName = "mainImsm", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal MainImsm { set; get; }

        /// <summary>
        /// Montant du déposit arrondi
        /// </summary>
        /// PM 20170808 [23371] Ajout
        [XmlElement(ElementName = "roundedImsm", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public decimal RoundedImsm { set; get; }
    }
    #endregion IMSM Method

    #region SPAN2 Method
    /// <summary>
    ///  Class contenant le détail du calcul d'un déposit évalué avec les méthodes SPAN et SPAN2
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class Span2MarginCalculationMethod : MarginCalculationMethod
    {
        #region Members
        private Span2CCPParameter[] m_parameters = null;

        /// <summary>
        /// Account Type
        /// </summary>
        [XmlAttribute(AttributeName = "accounttype", DataType = "string")]
        public string AccountType;
        [XmlIgnore]
        public bool AccountTypeSpecified;

        /// <summary>
        /// Maintenance Initial indicateur
        /// </summary>
        [XmlAttribute(AttributeName = "maint_init", DataType = "string")]
        public string MaintenanceInitial;
        [XmlIgnore]
        public bool MaintenanceInitialSpecified;

        /// <summary>
        /// EOD / ITD
        /// </summary>
        [XmlAttribute(AttributeName = "cycleCode", DataType = "string")]
        public string CycleCode;

        /// <summary>
        /// Initial Margin Currency Type (Contract ou Clearing House)
        /// </summary>
        // PM 20231030 [26547][WI735] Ajout MarginCurrencyType
        [XmlAttribute(AttributeName = "currencyType", DataType = "string")]
        public string MarginCurrencyType;
        [XmlIgnore]
        public bool MarginCurrencyTypeSpecified;

        /// <summary>
        /// Base URL pour le calcul CME SPAN 2
        /// </summary>
        [XmlElement(ElementName = "serviceBaseUrl", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string BaseUrl;
        [XmlIgnore]
        public bool BaseUrlSpecified;

        /// <summary>
        /// Id du User URL pour le calcul CME SPAN 2
        /// </summary>
        [XmlElement(ElementName = "serviceAccount", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string UserId;
        [XmlIgnore]
        public bool UserIdSpecified;

        /// <summary>
        /// Compteurs
        /// </summary>
        [XmlElement(ElementName = "counters", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public Span2MarginCounter MarginCounter;

        /// <summary>
        /// Indicateur d'essai d'écarter les positions sur des assets erronés
        /// </summary>
        [XmlAttribute(AttributeName = "excludeWrongPosition", DataType = "boolean")]
        public bool IsTryExcludeWrongPosition;

        [XmlArray("excludedPositions", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public FixPositionReport[] DiscartedPositions;

        [XmlArray("consideredPositions", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public FixPositionReport[] ConsideredPositions;

        /// <summary>
        /// Messages Xml de demande de calcul
        /// </summary>
        // PM 20230929 [XXXXX] Changement de type : string => string[]
        [XmlArray(ElementName = "xmlRequests", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItemAttribute("xmlRequest")]
        public string[] XmlRequestMessage;
        [XmlIgnore]
        public bool XmlRequestMessageSpecified;

        /// <summary>
        /// Message Json de demande de calcul
        /// </summary>
        [XmlElement(ElementName = "jsonRequest", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string JsonRequestMessage;

        /// <summary>
        /// Message XML reçue en réponse
        /// </summary>
        // PM 20230929 [XXXXX] Changement de type : string => string[]
        [XmlArray(ElementName = "xmlResponses", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItemAttribute("xmlResponse")]
        public string[] XmlResponseMessage;
        [XmlIgnore]
        public bool XmlResponseMessageSpecified;

        /// <summary>
        /// Message Json de réponse détaillée du calcul
        /// </summary>
        [XmlElement(ElementName = "jsonResponseDetail", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string JsonResponseMessage;

        /// <summary>
        /// Message d'erreur généré par le calcul
        /// </summary>
        [XmlElement(ElementName = "errorMessage", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public string ErrorMessage;

        /// <summary>
        /// Montants globaux
        /// </summary>
        [XmlElement(ElementName = "totalAmounts", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public Span2TotalAmount SpanTotalAmounts;
        #endregion Members

        #region Accessors
        /// <summary>
        /// CCP
        /// </summary>
        [XmlArray("ccps", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("ccp")]
        public override RiskParameter[] Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters = new Span2CCPParameter[value.Length];
                value.CopyTo(m_parameters, 0);
            }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Classe de compteurs
    /// </summary>
    public sealed class Span2MarginCounter
    {
        #region Members
        /// <summary>
        /// Nombre de paramètres d'asset
        /// </summary>
        [XmlElement(ElementName = "nbAsset", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbAssetParameters;

        /// <summary>
        /// Nombre de positions initiales
        /// </summary>
        [XmlElement(ElementName = "nbPosInitiale", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbInitialPosition;

        /// <summary>
        /// Nombre de positions nettes
        /// </summary>
        [XmlElement(ElementName = "nbPosNetted", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbNettedPosition;

        /// <summary>
        /// Nombre de positions réduites
        /// </summary>
        [XmlElement(ElementName = "nbPosReduced", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbReducedPosition;

        /// <summary>
        /// Nombre de positions non échues
        /// </summary>
        [XmlElement(ElementName = "nbPosActive", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbActivePosition;

        /// <summary>
        /// Nombre de positions écartées du calcul
        /// </summary>
        [XmlElement(ElementName = "nbPosDiscarted", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbDiscartedPosition;

        /// <summary>
        /// Nombre de positions considérées dans le calcul
        /// </summary>
        [XmlElement(ElementName = "nbPosConsidered", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbConsideredPosition;

        /// <summary>
        /// Compteur de position du message de calcul
        /// </summary>
        [XmlElement(ElementName = "nbPosSpanRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbSpanRiskPosition;

        /// <summary>
        /// Compteur de position réellement évaluée
        /// </summary>
        [XmlElement(ElementName = "nbPosProcessed", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public long NbProcessedPosition;
        #endregion Members
    }

    /// <summary>
    /// Montants globaux calculés par SPAN 2
    /// </summary>
    public sealed class Span2TotalAmount
    {
        #region Members
        /// <summary>
        /// Net Option Value
        /// </summary>
        [XmlElement(ElementName = "nov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney NetOptionValue { get; set; }
        [XmlIgnore]
        public bool NetOptionValueSpecified;

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        [XmlElement(ElementName = "riskInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskInitialAmount { get; set; }
        [XmlIgnore]
        public bool RiskInitialAmountSpecified;

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        [XmlElement(ElementName = "riskMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskMaintenanceAmount { get; set; }
        [XmlIgnore]
        public bool RiskMaintenanceAmountSpecified;

        /// <summary>
        /// Total Initial Margin Amount
        /// </summary>
        [XmlElement(ElementName = "totalInitialMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney TotalInitialMarginAmount { get; set; }
        [XmlIgnore]
        public bool TotalInitialMarginAmountSpecified;

        /// <summary>
        /// Total Maintenance Margin Amount
        /// </summary>
        [XmlElement(ElementName = "totalMaintenanceMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney TotalMaintenanceMarginAmount { get; set; }
        [XmlIgnore]
        public bool TotalMaintenanceMarginAmountSpecified;
        #endregion Members
    }

    /// <summary>
    /// CCP (Ex ExchangeComplex SPAN)
    /// </summary>
    public class Span2CCPParameter : RiskParameter
    {
        #region Members
        /// <summary>
        /// Exchange Complex Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// All Net Option Value Amount
        /// </summary>
        [XmlArray("novByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("nov")]
        public MarginMoney[] NetOptionValue { get; set; }

        /// <summary>
        /// All Risk Initial Amount
        /// </summary>
        [XmlArray("riskInitialByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("riskInitial")]
        public MarginMoney[] RiskInitial { get; set; }

        /// <summary>
        /// All Risk Maintenance Amount
        /// </summary>
        [XmlArray("riskMaintenanceByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("riskMaintenance")]
        public MarginMoney[] RiskMaintenance { get; set; }

        /// <summary>
        /// All Total Initial Margin Amount
        /// </summary>
        [XmlArray("totalInitialMarginByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("totalInitialMargin")]
        public MarginMoney[] TotalInitialMarginAmount { get; set; }

        /// <summary>
        /// All Total Maintenance Margin Amount
        /// </summary>
        [XmlArray("totalMaintenanceMarginByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("totalMaintenanceMargin")]
        public MarginMoney[] TotalMaintenanceMarginAmount { get; set; }

        /// <summary>
        /// Combined Commodity Group pour compatibilité SPAN
        /// </summary>
        [XmlArray("groups", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("group")]
        public override RiskParameter[] Parameters { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Combined Commodity Group pour compatibilité SPAN
    /// </summary>
    public class Span2GroupParameter : RiskParameter
    {
        #region Members
        /// <summary>
        /// Group Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Long Option Value Amount
        /// </summary>
        [XmlArray("lovByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("lov")]
        public MarginMoney[] LongOptionValue { get; set; }

        /// <summary>
        /// Short Option Value Amount
        /// </summary>
        [XmlArray("sovByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("sov")]
        public MarginMoney[] ShortOptionValue { get; set; }

        /// <summary>
        /// All Net Option Value Amount
        /// </summary>
        [XmlArray("novByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("nov")]
        public MarginMoney[] NetOptionValue { get; set; }

        /// <summary>
        /// All Risk Initial Amount
        /// </summary>
        [XmlArray("riskInitialByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("riskInitial")]
        public MarginMoney[] RiskInitial { get; set; }

        /// <summary>
        /// All Risk Maintenance Amount
        /// </summary>
        [XmlArray("riskMaintenanceByCurrency", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("riskMaintenance")]
        public MarginMoney[] RiskMaintenance { get; set; }

        /// <summary>
        /// Combined Commodity (Contract Group)
        /// </summary>
        [XmlArray("pods", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("pod")]
        public override RiskParameter[] Parameters { get; set; }
        #endregion Members
    }

    /// <summary>
    /// POD (ex Combined Commodity SPAN)
    /// </summary>
    public class Span2PodParameter : RiskParameter
    {
        #region Members
        /// <summary>
        /// Combined Commodity Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Méthode de calcul utilisée pour ce groupe de contrats
        /// </summary>
        [XmlAttribute(AttributeName = "marginMethod", DataType = "string")]
        public string MarginMethod;

        /// <summary>
        /// Long Non Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "lnov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LongNonOptionValue;

        /// <summary>
        /// Short Non Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "snov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortNonOptionValue;

        /// <summary>
        /// Long Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "lov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LongOptionValue;

        /// <summary>
        /// Short Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "sov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortOptionValue;

        /// <summary>
        /// Long Option Value (Futures Style)
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "lovFutureStyle", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LongOptionFuturesStyleValue;

        /// <summary>
        /// Short Option Value (Futures Style)
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "sovFutureStyle", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortOptionFuturesStyleValue;

        /// <summary>
        /// Net Option Value
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "nov", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney NetOptionValue;

        /// <summary>
        /// Short Option Minimum
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "som", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ShortOptionMinimum;

        /// <summary>
        /// Scan Risk
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "scanRisk", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ScanRisk;

        /// <summary>
        /// Inter Comodity Volatility Credit Amount
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "interCommodityVolatilityCredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney InterCommodityVolatilityCredit;

        /// <summary>
        /// Inter Month Spread Charge
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "intraCommodityCharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney IntraCommoditySpreadCharge;

        /// <summary>
        /// Delivery Month Charge
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "deliveryCharge", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney DeliveryMonthCharge;

        /// <summary>
        /// Inter Commodity Spread Credit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "interCommodityCredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney InterCommodityCredit;

        /// <summary>
        /// Inter Exchange Spread Credit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "interExchangeCredit", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney IntexCommodityCredit;

        /// <summary>
        /// Full Value Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "fullValueComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney FullValueComponent;

        /// <summary>
        /// Concentration Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "concentrationComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ConcentrationComponent;

        /// <summary>
        /// Hvar Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "hvarComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney HvarComponent;

        /// <summary>
        /// Liquidity Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "liquidityComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LiquidityComponent;

        /// <summary>
        /// Stress Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "stressComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney StressComponent;

        /// <summary>
        /// Implied Offset
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "impliedOffset", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ImpliedOffset;

        /// <summary>
        /// Risk Initial
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "riskInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskInitial;

        /// <summary>
        /// Risk Maintenance
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "riskMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskMaintenance;

        /// <summary>
        /// Product Group)
        /// </summary>
        [XmlArray("productgroups", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("productgroup")]
        public override RiskParameter[] Parameters { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Product Group
    /// </summary>
    public class Span2ProductGroupParameter : RiskParameter
    {
        #region Members
        /// <summary>
        /// Contract Name
        /// </summary>
        [XmlAttribute(AttributeName = "name", DataType = "string")]
        public string Name;

        /// <summary>
        /// Concentration Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "concentrationComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney ConcentrationComponent;

        /// <summary>
        /// Hvar Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "hvarComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney HvarComponent;

        /// <summary>
        /// Liquidity Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "liquidityComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney LiquidityComponent;

        /// <summary>
        /// Stress Component
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "stressComponent", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney StressComponent;

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "riskInitial", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskInitialAmount;

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlElement(ElementName = "riskMaintenance", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public MarginMoney RiskMaintenanceAmount;
        #endregion Members

        #region Accessors
        // Vide (un élément product group ne possède pas de sous-éléments)
        public override RiskParameter[] Parameters
        {
            get
            {
                return null;
            }
            set
            { }
        }
        #endregion Accessors
    }
    #endregion SPAN 2 Method

    #region NoMargin Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode None
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class NoMarginCalculationMethod : MarginCalculationMethod
    {
        #region Members
        /// <summary>
        /// Collection des trades concernés groupés par asset
        /// </summary>
        [XmlArray("assetGroup", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("asset")]
        public NoMarginMethAssetGroupDetail[] AssetGroupDetails { get; set; }
        #endregion Members

        #region Accessors
        // Vide
        public override RiskParameter[] Parameters
        {
            get { return null; }
            set { }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Collection de trades d'un asset
    /// </summary>
    public class NoMarginMethAssetGroupDetail
    {
        #region Members
        /// <summary>
        /// Id interne du marché
        /// </summary>
        [XmlIgnore]
        public int IdM;

        /// <summary>
        /// Identifiant du marché
        /// </summary>
        [XmlAttribute(AttributeName = "market", DataType = "string")]
        public string MarketIdentifier;

        /// <summary>
        /// Id interne de l'instrument
        /// </summary>
        [XmlIgnore]
        public int IdI;

        /// <summary>
        /// Id interne du contrat
        /// </summary>
        [XmlIgnore]
        public int IdContract;

        /// <summary>
        /// Identifiant du contrat
        /// </summary>
        [XmlAttribute(AttributeName = "contract", DataType = "string")]
        public string ContractIdentifier;

        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [XmlIgnore]
        public int IdAsset;

        /// <summary>
        /// Identifiant de l'asset
        /// </summary>
        [XmlAttribute(AttributeName = "asset", DataType = "string")]
        public string AssetIdentifier;

        /// <summary>
        /// Catégorie de l'asset
        /// </summary>
        [XmlAttribute(AttributeName = "assetCategory", DataType = "string")]
        public string AssetCategory;

        /// <summary>
        /// Devise
        /// </summary>
        [XmlAttribute(AttributeName = "currency", DataType = "string")]
        public string Currency;

        /// <summary>
        /// Collection des trades concernés
        /// </summary>
        [XmlArray("trades", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem("trade")]
        public NoMargineMetTradesDetail[] TradesDetails { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Données d'un trade
    /// </summary>
    public class NoMargineMetTradesDetail
    {
        #region Members
        /// <summary>
        /// Id interne du trade
        /// </summary>
        [XmlIgnore]
        public int IdT;

        /// <summary>
        /// Identifiant du trade
        /// </summary>
        [XmlAttribute(AttributeName = "trdId", DataType = "string")]
        public string TradeIdentifier;

        /// <summary>
        /// Id interne de l'acteur dealer
        /// </summary>
        [XmlIgnore]
        public int IdA_Dealer;

        /// <summary>
        /// Id interne du book du dealer
        /// </summary>
        [XmlIgnore]
        public int IdB_Dealer;

        /// <summary>
        /// Id interne de l'acteur clearer
        /// </summary>
        [XmlIgnore]
        public int IdA_Clearer;

        /// <summary>
        /// Id interne du book du clearer
        /// </summary>
        [XmlIgnore]
        public int IdB_Clearer;

        /// <summary>
        /// Date business du trade
        /// </summary>
        [XmlAttribute(AttributeName = "dtBusiness", DataType = "date")]
        public DateTime DtBusiness;

        /// <summary>
        /// Horodatage du trade
        /// </summary>
        [XmlAttribute(AttributeName = "timestamp", DataType = "dateTime")]
        public DateTime DtTimestamp;

        /// <summary>
        /// Date d'execution du trade
        /// </summary>
        [XmlAttribute(AttributeName = "dtExec", DataType = "dateTime")]
        public DateTime DtExecution;

        /// <summary>
        /// Time zone des dates
        /// </summary>
        [XmlAttribute(AttributeName = "timeZone", DataType = "string")]
        public string TzFacility;

        /// <summary>
        /// Sens du trade côté dealer
        /// </summary>
        [XmlAttribute(AttributeName = "side", DataType = "string")]
        public string Side;

        /// <summary>
        /// Quantité d'asset du trade
        /// </summary>
        [XmlAttribute(AttributeName = "qty", DataType = "decimal")]
        public decimal Quantity;

        /// <summary>
        /// Prix du trade
        /// </summary>
        [XmlAttribute(AttributeName = "price", DataType = "decimal")]
        public decimal Price;
        #endregion Members
    }
    #endregion NoMargin Method

    #region Euronext Var Based Method
    /// <summary>
    /// Class contenant le détail du calcul d'un déposit évalué avec la méthode Euronext Var Based
    /// </summary>
    /// <remarks>xsd reference: name="MarginCalculationMethod"</remarks>
    public class EuronextVarCalculationMethod : MarginCalculationMethod
    {
        #region Members
        [XmlArray(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "Sector")]
        public EuronextVarCalcSector[] SectorMargin;

        [XmlElement(ElementName = "TotalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarCalcMoney TotalMargin;

        [XmlElement(ElementName = "DiscartedPositions", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarDiscartedPositions DiscartedPositions;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray(ElementName = "AssetIncomplete", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "Asset")]
        public EuronextVarAssetIncomplet[] AssetIncomplete;
        #endregion Members

        #region Accessors
        // Empty list (DiscartedPosition does not have sub-parameters )
        public override RiskParameter[] Parameters
        {
            get
            {
                return null;
            }
            set
            { }
        }
        #endregion Accessors
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarCalcSector
    {
        #region Members
        /// <summary>
        /// Sector
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "string")]
        public string Sector;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "EuronextVarParameters", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarParameters EuronextVarParameters;

        /// <summary>
        /// Delivery Parameters
        /// </summary>
        [XmlArray(ElementName = "DeliveryParameters", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "Parameter")]
        public EuronextVarDeliveryParameters[] DeliveryParameters;
        [XmlIgnore]
        public bool DeliveryParametersSpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "LookBackPeriod", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarLookBackPeriod LookBackPeriod;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "ObservationNumber", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarObservationNumber ObservationNumber;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "LookBackPeriodDelivery", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarLookBackPeriod LookBackPeriodDelivery;
        [XmlIgnore]
        public bool LookBackPeriodDeliverySpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "ObservationNumberDelivery", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarObservationNumber ObservationNumberDelivery;
        [XmlIgnore]
        public bool ObservationNumberDeliverySpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "Group")]
        public EuronextVarCalcGroup[] GroupMarginDetail;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "SectorAdditionalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarResultMoney SectorAdditionalMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "SectorGroupsTotalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarCalcMoney GroupTotalMargin;

        /// <summary>
        /// Détail des éléments de calcul des échéances proche en livraison physique
        /// </summary>
        [XmlArray(ElementName = "NearExpiry", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "Asset")]
        public EuronextVarCalcAssetNearExpiry[] NearExpiryMarginDetail;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "NearExpiryTotalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarResultMoney NearExpiryTotalMargin;

        /// <summary>
        /// Détail des éléments de calcul des échéances proche en livraison physique
        /// </summary>
        [XmlArray(ElementName = "PhysicalDelivery", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        [XmlArrayItem(ElementName = "Asset")]
        public EuronextVarCalcAssetPhysicalDelivery[] PhysicalDeliveryMarginDetail;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "PhysicalDeliveryTotalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarResultMoney PhysicalDeliveryTotalMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "MarkToMarket", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarMarkToMarket MarkToMarket;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "SectorSumMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarResultMoney SectorTotalMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "FinalTotalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarCalcMoney TotalMargin;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarCalcGroup
    {
        #region Members
        /// <summary>
        /// Sector
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "string")]
        public string Group;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "InitialMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarInitialMargin InitialMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "DecorrelationAddOn", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarDecorrelationAddOn DecorrelationAddOn;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "AdditionalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarCalc AdditionalMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "GroupTotalMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarCalc GroupTotalMargin;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarParameters
    {
        #region Members
        [XmlElement(ElementName = "OrdinaryConfidenceLevel")]
        public decimal OrdinaryConfidenceLevel;
        [XmlElement(ElementName = "StressedConfidenceLevel")]
        public decimal StressedConfidenceLevel;
        [XmlElement(ElementName = "DecorrelationParameter")]
        public decimal DecorrelationParameter;
        [XmlElement(ElementName = "OrdinaryWeight")]
        public decimal OrdinaryWeight;
        [XmlElement(ElementName = "StressedWeight")]
        public decimal StressedWeight;
        /// <summary>
        /// Holding Period
        /// </summary>
        [XmlElement(ElementName = "HoldingPeriod")]
        public int HoldingPeriod;
        /// <summary>
        /// Sub-portfolio separator : number of markets days between evaluation date and expiry date of the physical delivery futures
        /// </summary>
        [XmlElement(ElementName = "SubPortfolioSeparatorDaysNumber")]
        public int SubPortfolioSeparatorDaysNumber;
        #endregion Members
    }

    /// <summary>
    /// Paramètre de livraison et pré-livraison physique
    /// </summary>
    public class EuronextVarDeliveryParameters
    {
        #region Members
        /// <summary>
        /// Symbol Code du Contrat
        /// </summary>
        [XmlAttribute(AttributeName = "contractCode", DataType = "string")]
        public string ContractCode;
        /// <summary>
        /// Devise
        /// </summary>
        [XmlAttribute(AttributeName = "currency", DataType = "string")]
        public string Currency;
        /// <summary>
        /// Sens
        /// </summary>
        [XmlAttribute(AttributeName = "side", DataType = "string")]
        public string Sens;
        /// <summary>
        /// Extra Percentage
        /// </summary>
        [XmlAttribute(AttributeName = "extraPct", DataType = "decimal")]
        public decimal ExtraPercentage;
        /// <summary>
        /// Margin Percentage
        /// </summary>
        [XmlAttribute(AttributeName = "marginPct", DataType = "decimal")]
        public decimal MarginPercentage;
        /// <summary>
        /// Fee Percentage
        /// </summary>
        [XmlAttribute(AttributeName = "feePct", DataType = "decimal")]
        public decimal FeePercentage;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarDiscartedPositions
    {
        #region Members
        [XmlIgnore]
        public bool InformationSpecified;

        /// <summary>
        /// Information
        /// </summary>
        [XmlAttribute(AttributeName = "information", DataType = "string")]
        public string Information;

        [XmlArray]
        [XmlArrayItem("PosRpt")]
        public FixPositionReport[] positions;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarLookBackPeriod
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Ordinary")]
        public int Ordinary;
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Stressed")]
        public int Stressed;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarObservationNumber
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Ordinary")]
        public EuronextVarObservationNumberDetail Ordinary;
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Stressed")]
        public EuronextVarObservationNumberDetail Stressed;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarObservationNumberDetail
    {
        #region Members
        [XmlText(DataType = "int")]
        public int Value;

        [XmlAttribute(AttributeName = "decvalue", DataType = "decimal")]
        public Decimal Decvalue;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarInitialMargin
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Ordinary")]
        public EuronextVarExpectedShortfallsContainer Ordinary;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Stressed")]
        public EuronextVarExpectedShortfallsContainer Stressed;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarDecorrelationAddOn
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Ordinary")]
        public EuronextVarExpectedShortfallsDecoGroupContainer Ordinary;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Stressed")]
        public EuronextVarExpectedShortfallsDecoGroupContainer Stressed;
        #endregion Members
    }

    /// <summary>
    /// Détail des éléments de calcul pour les échéances proche en livraison physique
    /// </summary>
    public sealed class EuronextVarCalcAssetNearExpiry
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "string")]
        public string IsinCode;

        [XmlAttribute(AttributeName = "increasePercentage", DataType = "decimal")]
        public decimal IncreasePercentage;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Ordinary")]
        public EuronextVarExpectedShortfallsAssetContainer Ordinary;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Stressed")]
        public EuronextVarExpectedShortfallsAssetContainer Stressed;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "RiskMeasureMargin")]
        public EuronextVarCalc RiskMeasureMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "FloorMargin")]
        public EuronextVarResult FloorMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "NearExpiryMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarResult NearExpiryMargin;
        #endregion Members
    }

    
    /// <summary>
    /// Détail des éléments de calcul pour les livraison physique
    /// </summary>
    public sealed class EuronextVarCalcAssetPhysicalDelivery
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "id", DataType = "string")]
        public string IsinCode;

        [XmlAttribute(AttributeName = "extraPercentage", DataType = "decimal")]
        public decimal ExtraPercentage;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Ordinary")]
        public EuronextVarExpectedShortfallsAssetContainer Ordinary;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Stressed")]
        public EuronextVarExpectedShortfallsAssetContainer Stressed;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "RiskMeasureMargin")]
        public EuronextVarCalc RiskMeasureMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "IncreasedMargin")]
        public EuronextVarResult IncreasedMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "FloorMargin")]
        public EuronextVarResult FloorMargin;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "DeliveryMargin", Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
        public EuronextVarResult DeliveryMargin;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfallsContainer
    {
        #region Members
        [XmlElement(ElementName = "ExpectedShortfalls")]
        public EuronextVarExpectedShortfalls ExpectedShortfalls;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfallsDecoGroupContainer
    {
        #region Members
        [XmlElement(ElementName = "ExpectedShortfalls")]
        public EuronextVarExpectedShortfallsDecorrelationGroup[] ExpectedShortfalls;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "DecorrelationES")]
        public EuronextVarResult DecorrelationExpectedShortfalls;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Result")]
        public EuronextVarResult Result;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfallsAssetContainer
    {
        #region Members
        [XmlElement(ElementName = "ExpectedShortfalls")]
        public EuronextVarExpectedShortfallsAssetNearExpiry ExpectedShortfalls;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfalls
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "ExpectedShortfall")]
        public EuronextVarExpectedShortfall[] ExpectedShortfall;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Result")]
        public EuronextVarResult Result;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfallsDecorrelationGroup : EuronextVarExpectedShortfalls
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "decorrelationGroup", DataType = "string")]
        public string DecorrelationGroup;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfallsAssetNearExpiry : EuronextVarExpectedShortfalls
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "ISINCode", DataType = "string")]
        public string ISINCode;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfall
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "number", DataType = "int")]
        public int number;

        /// <summary>
        /// 
        /// </summary>
        [XmlText(DataType = "decimal")]
        public Decimal Value;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarResult
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "function", DataType = "string")]
        public string Function;

        /// <summary>
        /// 
        /// </summary>
        [XmlText(DataType = "decimal")]
        public decimal Value;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarResultMoney
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute(AttributeName = "function", DataType = "string")]
        public string Function;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Money")]
        public Money Amount;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarMarkToMarket
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("MarkToMarketPos")]
        public EuronextVarMarkToMarketPos[] MarkToMarketPos;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Result")]
        public EuronextVarResultMoney Result;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarMarkToMarketPos
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "PosRpt")]
        public FixPositionReport Position;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Price")]
        public EuronextVarPrice Price;
        [XmlIgnore]
        public Boolean PriceSpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "UnderlyingPrice")]
        public EuronextVarPrice UnderlyingPrice;
        [XmlIgnore]
        public Boolean UnderlyingPriceSpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Multiplier")]
        public decimal Multiplier;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "MarkToMarket")]
        public Money MarkToMarket;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarCalcMoney
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Value1")]
        public decimal Value1;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Value2")]
        public decimal Value2;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Result")]
        public EuronextVarResultMoney Result;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarCalc
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Value1")]
        public decimal Value1;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Value2")]
        public decimal Value2;

        /// <summary>
        /// 
        /// </summary>
        [XmlElement(ElementName = "Result")]
        public EuronextVarResult Result;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarPrice
    {
        #region Members
        [XmlAttribute(AttributeName = "missing")]
        public Boolean Missing;

        [XmlIgnore]
        public Boolean MissingSpecified;

        [XmlText()]
        public decimal Value;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarAssetIncomplet
    {
        #region Members
        [XmlAttribute(AttributeName = "id")]
        public int IdAsset;

        [XmlAttribute(AttributeName = "ISINcode")]
        public string ISINCode;

        [XmlAttribute(AttributeName = "ordinaryScenarioNumber")]
        public int OrdinaryScenarioNumber;

        [XmlAttribute(AttributeName = "scaledScenarioNumber")]
        public int ScaledScenarioNumber;
        #endregion Members
    }
    #endregion Euronext Var Based Method
}
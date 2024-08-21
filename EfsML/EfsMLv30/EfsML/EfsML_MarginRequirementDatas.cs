using System;
using System.Xml.Serialization;
using System.ComponentModel;

using EfsML.Interface;
using FpML.Interface;

using FpML.Enum;
using EfsML.v30.Doc;
using FpML.v44.Doc;
using FpML.v44.Shared;

using EFS.ACommon;
using EfsML.Enum;

namespace EfsML.v30.MarginRequirement
{
    /// <summary>
    /// Class describing a risk (initial margin) evaluation log
    /// </summary>
    [XmlInclude(typeof(Repository))]
    [XmlInclude(typeof(MarginRequirementOffice))]
    [XmlRoot("marginRequirement", Namespace = "http://www.efs.org/2005/EFSmL-3-0", IsNullable = false)]
    public sealed class MarginDetailsDocument : IMarginDetailsDocument
    {

        bool m_Processed = false;

        /// <summary>
        /// Get the status of the log, returns true when the log has been processed, adding the risk evaluation details
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
        /// <param name="pTechTrade">the risk evaluation result reference</param>
        /// <param name="parties">the risk evaluation result main parties</param>
        public MarginDetailsDocument(int pIdTechTrade, ITrade pTechTrade, IParty[] parties)
        {
            party = parties as Party[];

            trade = new Trade[] { pTechTrade as Trade };

            IdTechTrade = pIdTechTrade;
        }
        
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
        IParty[] IDataDocument.party
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
        ITrade[] IDataDocument.trade
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
        public ITrade firstTrade
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
        /// Return the current instance, representing the risk evaluation log report
        /// </summary>
        [XmlIgnore]
        public object item
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

        private DocumentVersionEnum m_version = DocumentVersionEnum.Version40;

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        [Obsolete("Utilisez la propriété EfsMLversion", false)]
        public DocumentVersionEnum version
        {
            get 
            {
                return this.m_version; 
            }
        }

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
        public bool repositorySpecified
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
        public Repository repository = null;


        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IRepository IEfsDocument.repository
        {
            get
            {
                return repository;
            }
            set
            {
                repository = value as Repository;
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
                repository = new Repository();
            }

            return repository;
        }

        #endregion

        #region IMarginRequirementDatas Membres

        [ReadOnly(true)]
        [Browsable(false)]
        public MarginRequirementOffice marginRequirementOffice = new MarginRequirementOffice();

        [XmlIgnore]
        [ReadOnly(true)]
        [Browsable(false)]
        IMarginRequirementOffice IMarginDetailsDocument.marginRequirementOffice 
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
    }

    /// <summary>
    /// Class hosting the specific log elements of a risk evaluation 
    /// </summary>
    public class MarginRequirementOffice : IMarginRequirementOffice
    {
    }
}
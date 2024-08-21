using System.Runtime.Serialization;
using System.Xml.Serialization;
//
using EFS.ApplicationBlocks.Data;

namespace EFS.SpheresRiskPerformance.DataContracts
{
    /// <summary>
    /// Class representing a risk evaluation result (trade)
    /// </summary>
    [DataContract(
        Name = DataHelper<MarginRequirementTrade>.DATASETROWNAME,
        Namespace = DataHelper<MarginRequirementTrade>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "RiskMarginResults")]
    internal sealed class MarginRequirementTrade
    {
        string m_Trade;

        /// <summary>
        /// Trade identifier
        /// </summary>
        [DataMember(Name = "TRADE", Order = 1)]
        public string Trade
        {
            get { return m_Trade; }
            set { m_Trade = value; }
        }

        int m_TradeId;

        /// <summary>
        /// Trade internal id
        /// </summary>
        [DataMember(Name = "TRADEID", Order = 2)]
        public int TradeId
        {
            get { return m_TradeId; }
            set { m_TradeId = value; }
        }

        string m_TradeName;

        /// <summary>
        /// Trade display name
        /// </summary>
        [DataMember(Name = "TRADENAME", Order = 3)]
        public string TradeName
        {
            get { return m_TradeName; }
            set { m_TradeName = value; }
        }

        string m_Timing;

        /// <summary>
        /// Timing
        /// </summary>
        [DataMember(Name = "TIMING", Order = 4)]
        public string Timing
        {
            get { return m_Timing; }
            set { m_Timing = value; }
        }

        int m_ActorId;

        /// <summary>
        /// Internal id of the actor
        /// </summary>
        [DataMember(Name = "IDA", Order = 5)]
        public int ActorId
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        int m_BookId;

        /// <summary>
        /// Internal id of the book
        /// </summary>
        [DataMember(Name = "IDB", Order = 6)]
        public int BookId
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        // PM 20131217 [19365] Add m_IsClearer
        bool m_IsClearer;

        /// <summary>
        /// Margin Requirement for Clearer or not
        /// </summary>
        [DataMember(Name = "ISCLEARER", Order = 7)]
        public bool IsClearer
        {
            get { return m_IsClearer; }
            set { m_IsClearer = value; }
        }

        string m_IdC;

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "CURRENCY", Order = 7)]
        public string IdC
        {
            get { return m_IdC; }
            set { m_IdC = value; }
        }

    }
}

namespace EFS.SpheresRiskPerformance.SpheresObjects
{
    /// <summary>
    /// Class representing a set of risk logs, IDMARGINTRACK is the identifier 
    /// </summary>
    [DataContract(
        Name = DataHelper<MARGINTRACK>.DATASETROWNAME,
        Namespace = DataHelper<MARGINTRACK>.DATASETNAMESPACE)]
    [XmlRoot(ElementName = "RiskResultSet")]
    public partial class MARGINTRACK
    {
        int m_IdMarginTrack;

        /// <summary>
        /// Risk logs set internal Id (auto-increment)
        /// </summary>
        [DataMember(Name = "IDMARGINTRACK", Order = 1)]
        public int IdMarginTrack
        {
            get { return m_IdMarginTrack; }
            set { m_IdMarginTrack = value; }
        }

        int m_IdProcessL;

        /// <summary>
        /// Internal Id of the process
        /// </summary>
        [DataMember(Name = "IDPROCESS_L", Order = 2)]
        public int IdProcessL
        {
            get { return m_IdProcessL; }
            set { m_IdProcessL = value; }
        }
    }
}
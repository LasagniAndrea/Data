using System;
using System.Runtime.Serialization;
//
using EFS.ApplicationBlocks.Data;

namespace EFS.SpheresRiskPerformance.EOD
{
    /// <summary>
    /// Class representing a set of risk logs, IDMARGINTRACK is the identifier 
    /// </summary>
    [DataContract(
        Name = DataHelper<IMREQUEST>.DATASETROWNAME,
        Namespace = DataHelper<IMREQUEST>.DATASETNAMESPACE)]
    public class IMREQUEST
    {
        /// <summary>
        /// Risk logs set internal Id (auto-increment)
        /// </summary>
        [DataMember(Name = "IDA_MRO", Order = 1)]
        public int MroId
        {
            get;
            set;
        }

        /// <summary>
        /// Internal Id of the process
        /// </summary>
        [DataMember(Name = "IDB_MRO", Order = 2)]
        public int MroBookId
        {
            get;
            set;
        }

        /// <summary>
        /// Evaluation mode of the deposit affecting the actor/book pair, false when a net evaluation is performed
        /// </summary>
        public bool IsGrossMargining
        {
            get;
            set;
        }

        /// <summary>
        /// Internal id of the MARGINREQOFFICE parent of the current MARGINREQOFFICE attribute
        /// </summary>
        /// <remarks>it could be 0 for MARGINREQOFFICE actor without any MARGINREQOFFICE parent</remarks>
        public int ParentId
        {
            get;
            set;
        }

        /// <summary>
        /// internal id of the trade DEPOSIT affecting the current actor/book pair
        /// </summary>
        public int TradeId
        {
            get;
            set;
        }

        /// <summary>
        /// Start time of the current risk margin evaluation 
        /// </summary>
        public DateTime Start
        {
            get;
            set;
        }

        /// <summary>
        /// End time of the current risk margin evaluation 
        /// </summary>
        /// <remarks>the Δ(End - Start) gives the elapsed time in order to evaluate the amount of the risk margin evaluation </remarks>
        public DateTime End
        {
            get;
            set;
        }

        /// <summary>
        /// Optional internal id of the POSREQUEST that activated the deposit task
        /// </summary>
        public int? IdPr
        {
            get;
            set;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using EFS.Common;

namespace EFS.Process
{
    /// <summary>
    ///  Demande de traitement
    /// </summary>
    public class Request
    {

        /// <summary>
        /// 
        /// </summary>
        public DateTime DtRequest { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public AppSession Session { set; get; }
        
        /// <summary>
        ///  Nb de Message à traiter
        /// </summary>
        public int PostedMsg { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public int PostedSubMsg { set; get; }

        /// <summary>
        ///  Nb de Message traités sans incidence
        /// </summary>
        /// FI 20201030 [25537]
        public int NoneMsg { set; get; }

        /// <summary>
        ///  Nb de Message traités avec succès
        /// </summary>
        public int SuccessMsg { set; get; }
        /// <summary>
        ///  Nb de Message traités en warning
        /// </summary>
        public int WarningMsg { set; get; }

        /// <summary>
        ///  Nb de Message traités en erreur
        /// </summary>
        public int ErrorMsg { set; get; }

        

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDtRequest"></param>
        /// <param name="pSession"></param>
        public Request(DateTime pDtRequest, AppSession pSession) :
            this(pDtRequest, pSession, 1)
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDtRequest"></param>
        /// <param name="pSession"></param>
        /// <param name="pPostedMsg"></param>
        public Request(DateTime pDtRequest, AppSession pSession, int pPostedMsg)
        {
            DtRequest = pDtRequest;
            Session = pSession;
            PostedMsg = pPostedMsg;
        }
        #endregion Constructors
    }
}

#region Using Directives
using EFS.Actor;
using EFS.Book;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Liste des acteurs REGULATORYOFFICE
    /// </summary>
    /// FI 20140206 [19564] add Class 
    public class RegulatoryOffices : OfficesBase
    {
        #region Accessor
        /// <summary>
        /// Obtient Rôle des acteurs REGULATORYOFFICE
        /// </summary>
        public override RoleActor RoleActor
        {
            get
            {
                return RoleActor.REGULATORYOFFICE;
            }
        }
        #endregion Accessor

        #region Constructors
        /// <summary>
        /// Liste des acteurs REGULATORYOFFICE associé à l'acteur {pIdA}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        // EG 20180307 [23769] Gestion dbTransaction
        public RegulatoryOffices(string pCS, int pIdA)
            : this(pCS, null, pIdA)
        {
            
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public RegulatoryOffices(string pCS, IDbTransaction pDbTransaction, int pIdA)
            : base(pCS, pDbTransaction, pIdA)
        {
        }
        #endregion Constructors
    }

}

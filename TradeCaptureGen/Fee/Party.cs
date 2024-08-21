using EFS.Actor;
using EfsML.Enum;
using System;
//PasGlop Faire un search dans la solution de "TODO FEEMATRIX"

namespace EFS.TradeInformation
{

    /// <summary>
    /// 
    /// </summary>
    /// FI 20180502 [23926] add [Serializable] => Nécessaire à FeeMatrix.Clone()
    [Serializable]
    public abstract class PartyBase
    {
        /// <summary>
        /// Représente l'OTcmlId de l'acteur
        /// </summary>
        public int m_Party_Ida;
        /// <summary>
        /// Représente Le rôle de l'acteur
        /// </summary>
        public RoleActor m_Party_Role;
        public string m_Party_Href;
        public string m_Party_PartyId;

        #region Constructors
        public PartyBase()
        {
            m_Party_Ida = 0;
            m_Party_Role = RoleActor.NONE;
            m_Party_Href = string.Empty;
            m_Party_PartyId = string.Empty;
        }
        public PartyBase(int pIda, Actor.RoleActor pRole, string pHref)
        {
            m_Party_Ida = pIda;
            m_Party_Role = pRole;
            m_Party_Href = pHref;
            m_Party_PartyId = pHref;
        }
        public PartyBase(int pIda, Actor.RoleActor pRole, string pHref, string pPartyId)
        {
            m_Party_Ida = pIda;
            m_Party_Role = pRole;
            m_Party_Href = pHref;
            m_Party_PartyId = pPartyId;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Représente l'acheteur ou le vendeur
    /// </summary>
    /// FI 20180502 [23926] add [Serializable] => Nécessaire à FeeMatrix.Clone()
    [Serializable]
    public class Party : PartyBase
    {
        /// <summary>
        /// Représente l'OTcmlId du book de  l'acteur
        /// </summary>
        public int m_Party_Idb;
        /// <summary>
        /// Représente  le sens rattaché à l'acteur
        /// </summary>
        public TradeSideEnum m_Party_Side;

        #region Constructors
        public Party()
            : base(0, RoleActor.NONE, string.Empty, string.Empty)
        {
            m_Party_Idb = 0;
            m_Party_Side = TradeSideEnum.All;
        }
        public Party(int pIda, Actor.RoleActor pRole, int pIdb, TradeSideEnum pSide, string pHref)
            : base(pIda, pRole, pHref)
        {
            m_Party_Idb = pIdb;
            m_Party_Side = pSide;
        }
        public Party(int pIda, Actor.RoleActor pRole, int pIdb, TradeSideEnum pSide, string pHref, string pPartyId)
            : base(pIda, pRole, pHref, pPartyId)
        {
            m_Party_Idb = pIdb;
            m_Party_Side = pSide;
        }
        #endregion Constructors
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// FI 20180502 [23926] add [Serializable] => Nécessaire à FeeMatrix.Clone()
    [Serializable]
    public class OtherParty : PartyBase
    {
        #region Constructors
        public OtherParty()
            : base(0, RoleActor.NONE, string.Empty, string.Empty) { }
        public OtherParty(int pIda, RoleActor pRole, string pHref)
            : base(pIda, pRole, pHref) { }
        public OtherParty(int pIda, RoleActor pRole, string pHref, string pPartyId)
            : base(pIda, pRole, pHref, pPartyId) { }
        #endregion Constructors
    }
}
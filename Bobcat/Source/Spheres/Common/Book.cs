using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;  

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EfsML.Enum;


namespace EFS.Book
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class BookTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        /// FI 20120211 [] sqlbook.isUseTable = true; 
        /// pour les performances
        // EG 20150706 [21021] Nullable<int> for pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsBookManaged(string pSource, Nullable<int> pIdB)
        {
            return IsBookManaged(pSource, null, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsBookManaged(string pSource, IDbTransaction pDbTransaction, Nullable<int> pIdB)
        {
            bool ret = false;
            if (pIdB.HasValue)
            {
                SQL_Book sqlbook = new SQL_Book(pSource, pIdB.Value)
                {
                    DbTransaction = pDbTransaction,
                    IsUseTable = true
                };
                sqlbook.LoadTable(new string[] { "IDA_ENTITY" });
                ret = sqlbook.IsLoaded && (sqlbook.IdA_Entity > 0); // Book Géré (par une entité)
            }
            return ret;
        }

        /// <summary>
        /// Retourne True si la tenue de position est gérée pour ce book
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        /// FI 20120211 [] add sqlbook.isUseTable = true;
        // EG 20150706 [210521] Nullable<int> pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsBookPosKeeping(string pSource, Nullable<int> pIdB)
        {
            return IsBookPosKeeping(pSource, null, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsBookPosKeeping(string pSource, IDbTransaction pDbTransaction, Nullable<int> pIdB)
        {
            bool ret = false;
            if (pIdB.HasValue)
            {
                SQL_Book sqlbook = new SQL_Book(pSource, pIdB.Value)
                {
                    DbTransaction = pDbTransaction,
                    IsUseTable = true
                };
                sqlbook.LoadTable(new string[] { "ISPOSKEEPING" });
                ret = sqlbook.IsLoaded && (sqlbook.IsPosKeeping);
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'OTCmlId de l'entité de l'acteur représenté par le couple {pIdA},{pIdB} et ce, uniquement lorsque ce dernier est Client
        /// <para>rappel: l'entité est paramétré sur le book</para>
        /// <para>Retourne 0 lorsque l'acteur n'est pas client ou s'il ne possède pas d'entité rattaché à son book</para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <param name="opIdAEntity"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetEntityBookClient(string pSource, int pIdA, Nullable<int> pIdB)
        {
            return GetEntityBookClient(pSource, null, pIdA, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetEntityBookClient(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
        {
            int ret = 0;
            if (pIdB.HasValue)
            {
                SQL_Book sqlbook = new SQL_Book(pSource, pIdB.Value)
                {
                    DbTransaction = pDbTransaction,
                    IsUseTable = true //FI 20130208
                };
                sqlbook.LoadTable(new string[] { "IDA_ENTITY" });
                if (sqlbook.IsLoaded && (sqlbook.IdA_Entity > 0) && ActorTools.IsActorClient(pSource, pIdA)) // Book Gere
                    ret = sqlbook.IdA_Entity;
            }
            return ret;
        }

        /// <summary>
        /// Retourne l'IDA de l'acteur entité de gestion d'un book
        /// <para>NB: Retourne 0 s'il n'existe aucune entité de gestion sur le book</para>
        /// </summary>
        /// <param name="pSource">ConnectionString</param>
        /// <param name="pIdB">IDB du book</param>
        /// <returns>IDA de l'acteur entité de gestion du book</returns>
        // EG 20150706 [21021] Nullable<int> for pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetEntityBook(string pSource, Nullable<int> pIdB)
        {
            return GetEntityBook(pSource, null, pIdB);
        }

        /// <summary>
        /// Retourne l'IDA de l'acteur entité de gestion d'un book
        /// <para>NB: Retourne 0 s'il n'existe aucune entité de gestion sur le book</para>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetEntityBook(string pSource, IDbTransaction pDbTransaction, Nullable<int> pIdB)
        {
            int ret = 0;

            if (pIdB.HasValue)
            {
                SQL_Book sqlbook = new SQL_Book(pSource, pIdB.Value)
                {
                    DbTransaction = pDbTransaction,
                    IsUseTable = true //FI 20130208
                };
                sqlbook.LoadTable(new string[] { "IDA_ENTITY" });
                if (sqlbook.IsLoaded && (sqlbook.IdA_Entity > 0)) // Book Gere
                    ret = sqlbook.IdA_Entity;
            }
            return ret;
        }

        /// <summary>
        /// Obtient true si la contrepartie est un clearer (partie,book) est un CSS et le book est géré
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyClearer(string pSource, int pIdA, int pIdB)
        {
            return IsCounterPartyClearer(pSource, null, pIdA, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyClearer(string pSource, IDbTransaction pDbTransaction, int pIdA, int pIdB)
        {
            return IsBookManaged(pSource, pDbTransaction, pIdB) && ActorTools.IsActorCSSorCLEARER(pSource, pDbTransaction, pIdA);
        }

        /// <summary>
        /// Retourne true si la Contrepartie est Externe (Sans book, ou book non rattaché à une entité)
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyExternal(string pSource, Nullable<int> pIdB)
        {
            return IsCounterPartyExternal(pSource, null, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyExternal(string pSource, IDbTransaction pDbTransaction, Nullable<int> pIdB)
        {
            return (false == IsBookManaged(pSource, pDbTransaction, pIdB));
        }

        /// <summary>
        /// Retourne true si le couple (partie,book) est un client géré
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyClient(string pSource, int pIdA, Nullable<int> pIdB)
        {
            return IsCounterPartyClient(pSource, null, pIdA, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyClient(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
        {
            return (GetEntityBookClient(pSource, pDbTransaction, pIdA, pIdB) > 0);
        }

        /// <summary>
        /// Retourne true si le couple (partie,book) est un client géré et s'il existe un mandataire qui agit pour le compte de ce dernier  
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyClientUnderDecisionMaker(string pSource, int pIdA, Nullable<int> pIdB)
        {
            return IsCounterPartyClientUnderDecisionMaker(pSource, null, pIdA, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyClientUnderDecisionMaker(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
        {
            Boolean ret = IsCounterPartyClient(pSource, pDbTransaction, pIdA, pIdB);
            if (ret)
            {
                DecisionOffices offices = new DecisionOffices(pSource, pDbTransaction, pIdA, pIdB);
                ret = (offices.Count > 0);
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si le couple {partie,book} est un client de l'entité pIdAEntity
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <param name="pIdAEntity"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsBookClient(string pSource, int pIdA, int pIdB, int pIdAEntity)
        {
            return IsBookClient(pSource, null, pIdA, pIdB, pIdAEntity);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsBookClient(string pSource, IDbTransaction pDbTransaction, int pIdA, int pIdB, int pIdAEntity)
        {
            bool ret = false;
            int idAEntity = GetEntityBookClient(pSource, pDbTransaction, pIdA, pIdB);
            if ((idAEntity) > 0)
                ret = (idAEntity == pIdAEntity);
            return ret;
        }



        /// <summary>
        /// Retourne true si la Contrepartie n'est pas un Client, mais que son Book est géré (Desk, Agency, Entité elle même...)
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        // EG 20150706 [21021] Nullable<int> pIdB
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyNoClientNoExternal(string pSource, int pIdA, Nullable<int> pIdB)
        {
            return IsCounterPartyNoClientNoExternal(pSource, null, pIdA, pIdB);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyNoClientNoExternal(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
        {
            return (false == ActorTools.IsActorClient(pSource, pDbTransaction, pIdA)) && BookTools.IsBookManaged(pSource, pDbTransaction, pIdB);
        }

        /// <summary>
        /// Retourne true si la Contrepartie un donneur d'ordre maison (activité pour compte propre)
        /// <para>
        ///  (Desk, Agency, Entité elle même...)
        /// </para>
        /// La contrepartie n'est pas un Client et 
        /// Le Book est géré par {pIdAEntity}
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyHouse(string pSource, int pIdA, int pIdB, int pIdAEntity)
        {
            return IsCounterPartyHouse(pSource, null, pIdA, pIdB, pIdAEntity);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static bool IsCounterPartyHouse(string pSource, IDbTransaction pDbTransaction, int pIdA, int pIdB, int pIdAEntity)
        {
            int idAEntity = GetEntityBook(pSource, pDbTransaction, pIdB);
            _ = pIdAEntity == idAEntity;
            bool ret = !ActorTools.IsActorClient(pSource, pDbTransaction, pIdA);

            return ret;
        }



        /// <summary>
        /// Retourne la liste des groupes de book auxquels appartient un book
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdB"></param>
        /// <param name="pRole">Réduit la liste aux seuls groupes du rôle {pRole}, valeur null autorisé</param>
        /// <param name="pIsUseDataDtEnabled"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static int[] GetGrpBook(string pCS, int pIdB, Nullable<RoleGBook> pRole, bool pIsUseDataDtEnabled)
        {
            return GetGrpBook(pCS, null as IDbTransaction, pIdB, pRole, pIsUseDataDtEnabled);
        }
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20180425 Analyse du code Correction [CA2202]
        public static int[] GetGrpBook(string pCS, IDbTransaction pDbTransaction, int pIdB, Nullable<RoleGBook> pRole, bool pIsUseDataDtEnabled)
        {

            int[] ret = null;
            //
            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDB", DbType.Int32), pIdB);
            if (null != pRole)
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEGBOOK), pRole.ToString());
            //
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "bookg.IDGBOOK" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.BOOKG.ToString() + " bookg" + Cst.CrLf;
            if (null != pRole)
            {
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.GBOOKROLE.ToString() + " gBookRole on gBookRole.IDGBOOK = bookg.IDGBOOK" + Cst.CrLf;
                sqlSelect += SQLCst.AND + "gBookRole.IDROLEGBOOK = @IDROLEGBOOK" + Cst.CrLf;
            }
            sqlSelect += SQLCst.WHERE + "bookg.IDB=@IDB";
            //
            if (pIsUseDataDtEnabled)
            {
                sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "bookg") + Cst.CrLf;
                if (null != pRole)
                    sqlSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "gBookRole") + Cst.CrLf;
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), dp);

            ArrayList al = new ArrayList();
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text,
                qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                    al.Add(Convert.ToInt32(dr[0]));
            }

            if (ArrFunc.IsFilled(al))
                ret = (int[])al.ToArray(typeof(int));
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class OfficesBase
    {
        #region Members
        private readonly int[] office;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return ArrFunc.Count(office);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string BookColumn
        {
            get
            {
                return Cst.NotAvailable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual RoleActor RoleActor
        {
            get
            {
                return RoleActor.NONE;
            }
        }

        /// <summary>
        /// Obtient les types de Rôle qui doivent être exclus. Obtient null si aucun type n'est exclu.
        /// </summary>
        /// FI 20240218 [WI838] add
        public virtual RoleType[] RoleTypeExclude
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Obtient les rôles qui doivent être utilisés. Obtient null si tous les rôles peuvent être utilisés.
        /// </summary>
        /// FI 20240218 [WI838] add
        public virtual RoleActor[] RoleFilter
        {
            get
            {
                return null;
            }
        }


        #endregion Accessor

        #region Indexors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIndex"></param>
        /// <returns></returns>
        public int this[int pIndex]
        {
            get
            {
                return office[pIndex];
            }
        }
        #endregion Indexors

        #region Constructors
        /// <summary>
        /// Liste des acteurs Office  ("office" == ContactOffice or SettlementOffice or ....) à partir d'un couple (IDA,IDB)
        /// Recherche à partir d'un book
        /// S’il existe un book avec "office", Chargement de ce dernier uniquement. 
        /// S’il existe un book sans office, Chargement des acteurs "office" dans la hiérarchie des acteurs parents du propriétaire du book (propriétaire compris).
        /// S’il n’existe pas de book, Chargement des acteurs "office" dans la hiérarchie des acteurs parents de la partie (la partie comprise).
        /// </summary>
        /// <param name="pCS">Source</param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA">Acteur party</param>
        /// <param name="pIdB">Book associé à Acteur party</param>
        /// FI 20140206 [19564] 
        /// EG 20150706 [21021] Nullable<int> for pIdB
        /// EG 20180205 [23769] Add dbTransaction  
        /// FI 20240218 [WI838] seul ce constructeur est conservé

        public OfficesBase(string pCS, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
        {
            int idA = pIdA;

            if (pIdB.HasValue && pIdB != 0)
            {
                SQL_Book sqlBook = new SQL_Book(pCS, pIdB.Value, SQL_Table.ScanDataDtEnabledEnum.Yes)
                {
                    DbTransaction = pDbTransaction
                };

                if (BookColumn != Cst.NotAvailable)
                {
                    if (sqlBook.LoadTable(new string[] { "IDB", "IDA", BookColumn }))
                    {
                        if (null != sqlBook.GetFirstRowColumnValue(BookColumn))
                            office = new int[] { Convert.ToInt32(sqlBook.GetFirstRowColumnValue(BookColumn)) };
                        if (null == office)
                            idA = sqlBook.IdA;
                    }
                }
                else
                {
                    //FI 20140206 [19564] New 
                    if (sqlBook.LoadTable(new string[] { "IDB", "IDA" }))
                    {
                        if (null == office)
                            idA = sqlBook.IdA;
                    }
                }
            }

            if (null == office)
            {
                SearchAncestorRole search = new SearchAncestorRole(pCS, pDbTransaction, idA, RoleActor);
                office = search.GetActors(RoleFilter, RoleTypeExclude);
            }
        }

        /// <summary>
        /// Liste des acteurs Office  ("office" == ContactOffice or SettlementOffice or ....) 
        /// </summary>
        /// FI 20240218 [WI838] appel au constructor avec book à null
        public OfficesBase(string pCS, IDbTransaction pDbTransaction, int pIdA) :
            this(pCS, pDbTransaction, pIdA, null)
        {
        }

        #endregion Constructors
    }


    /// <summary>
    ///  Représente la hierarchie des acteurs qui ont le rôle DECISIONOFFICE
    /// </summary>
    /// FI 20171003 [23464] Add
    public class DecisionOffices : OfficesBase
    {
        #region Accessor
        /// <summary>
        /// Colonne ds Book qui définit le Decision Office
        /// </summary>
        public override string BookColumn
        {
            get
            {
                return "IDA_DECISIONOFFICE";
            }
        }
        /// <summary>
        /// Obtient <see cref="RoleActor.DECISIONOFFICE"/>
        /// </summary>
        /// FI 20240218 [WI838] Add
        public override RoleActor RoleActor
        {
            get
            {
                return RoleActor.DECISIONOFFICE;
            }
        }

        /// <summary>
        /// Obtient les types de Rôle qui doivent être exclus (à savoir <see cref="RoleType.ACCESS"/>, <see cref="RoleType.COLLABORATION"/>, <see cref="RoleType.COLLABORATION_ALGORITHM"/>).
        /// </summary>
        /// FI 20240218 [WI838] Add
        public override RoleType[] RoleTypeExclude
        {

            get { return new RoleType[] { RoleType.ACCESS, RoleType.COLLABORATION, RoleType.COLLABORATION_ALGORITHM }; }
        }
        #endregion Accessor

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdA"></param>
        /// <param name="pIdB"></param>
        // EG 20180205 [23769] Add dbTransaction  
        public DecisionOffices(string pSource, IDbTransaction pDbTransaction, int pIdA, Nullable<int> pIdB)
            : base(pSource, pDbTransaction, pIdA, pIdB)
        { }
        #endregion Constructors
    }
}
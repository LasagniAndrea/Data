#region VSS Auto-Comments
/* 
 * *********************************************************************
 $History: Restriction.cs $
 * 
 * *****************  Version 27  *****************
 * User: Filipe       Date: 29/05/15   Time: 16:55
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 26  *****************
 * User: Filipe       Date: 11/07/14   Time: 14:11
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 25  *****************
 * User: Filipe       Date: 5/06/14    Time: 15:04
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 24  *****************
 * User: Pascal       Date: 26/06/12   Time: 18:32
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 23  *****************
 * User: Pascal       Date: 18/05/12   Time: 11:19
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 22  *****************
 * User: Filipe       Date: 27/04/12   Time: 8:41
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 21  *****************
 * User: Filipe       Date: 23/01/12   Time: 15:51
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 20  *****************
 * User: Filipe       Date: 17/01/12   Time: 13:56
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 19  *****************
 * User: Filipe       Date: 17/01/12   Time: 11:00
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 18  *****************
 * User: Filipe       Date: 16/01/12   Time: 22:42
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 17  *****************
 * User: Filipe       Date: 16/01/12   Time: 17:53
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 16  *****************
 * User: Filipe       Date: 12/01/12   Time: 11:36
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 15  *****************
 * User: Pascal       Date: 5/01/12    Time: 12:54
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 14  *****************
 * User: Filipe       Date: 14/12/11   Time: 15:49
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 13  *****************
 * User: Filipe       Date: 14/12/11   Time: 12:50
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 12  *****************
 * User: Filipe       Date: 22/09/11   Time: 15:17
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 11  *****************
 * User: Filipe       Date: 3/09/10    Time: 11:11
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 10  *****************
 * User: Filipe       Date: 29/07/10   Time: 12:51
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 9  *****************
 * User: Cristina     Date: 27/07/10   Time: 15:13
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 8  *****************
 * User: Cristina     Date: 27/07/10   Time: 15:04
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 7  *****************
 * User: Filipe       Date: 26/07/10   Time: 17:53
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 6  *****************
 * User: Pascal       Date: 19/07/10   Time: 15:40
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 5  *****************
 * User: Filipe       Date: 15/07/10   Time: 14:53
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 4  *****************
 * User: Pascal       Date: 1/07/10    Time: 18:22
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 3  *****************
 * User: Filipe       Date: 28/06/10   Time: 11:19
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 2  *****************
 * User: Razik        Date: 21/05/10   Time: 16:09
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 1  *****************
 * User: administrateur Date: 5/11/09    Time: 10:58
 * Created in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 13  *****************
 * User: Eric         Date: 5/10/09    Time: 9:34
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 12  *****************
 * User: Filipe       Date: 20/07/09   Time: 10:03
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 11  *****************
 * User: Filipe       Date: 15/07/09   Time: 10:16
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 10  *****************
 * User: Razik        Date: 22/12/08   Time: 17:56
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 9  *****************
 * User: Razik        Date: 22/10/08   Time: 17:26
 * Updated in $/Code_Source/Spheres/Common
 * 
 * *****************  Version 8  *****************
 * User: Pascal       Date: 7/01/08    Time: 17:55
 * Updated in $/Code_Source/Spheres/Common
 * 
 * **********************************************************************
*/
#endregion

using System;
using System.Data;
using System.Collections;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Actor;
using EFS.Common;
using EFS.Rights;

using EFS.Permission;

namespace EFS.Restriction
{
    
    /// <summary>
    /// Représente un élément dont l'accessibilité est soumise à restrictions
    /// </summary>
    public class RestrictionItem
    {
        #region  members
        /// <summary>
        /// Obtient ou définit l'Id de l'élément
        /// <para>Exemple RESTRICTION ACTOR => Contient l'id de l'acteur </para>
        /// </summary>
        public int id;
        /// <summary>
        /// Obtient ou définit l'identifiant de l'élément
        /// <para>Exemple RESTRICTION ACTOR => Contient l'identifier de l'acteur </para>
        /// </summary>
        public string identifier;   
        /// <summary>
        /// Obtient ou définit la restriction attribuée à l'élément
        /// <para>Si true, l'élément est autorisé</para>
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }
        private bool _isEnabled;
        #endregion  members

        #region constructor
        public RestrictionItem()
        {
        }
        public RestrictionItem(int pId, string pIdentifier, bool pIsEnabled)
        {
            id = pId;
            identifier = pIdentifier;
            IsEnabled = pIsEnabled;
        }
        #endregion constructor
    }
    
    /// <summary>
    /// Représente une collection d'élements soumis à restriction 
    /// </summary> 
    public abstract class RestrictionBase
    {
        #region Members
        /// <summary>
        /// Représente la liste des élements soumis à restriction
        /// </summary>
        protected Hashtable htRestriction;
        #endregion Members
        
        #region Accessor
        /// <summary>
        /// Obtient le type de d'élement soumis à restriction (ie ACTOR,INSTRUMENT,PERMISSION, etc)
        /// <para>Doit être obligatoirement overrider</para>
        /// </summary>
        public virtual string Class
        {
            get { return "VirtualClass"; }
        }
        #endregion
        
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150529 [20982] Add 
        public RestrictionBase()
        {
            htRestriction = new Hashtable();
        }
        #endregion constructor
        
        #region Methods
        /// <summary>
        /// Charge les élements soumis à restrictions avec les autorisations par défaut
        /// </summary>
        /// <param name="pCS"></param>
        public virtual void Initialize(string pCS)
        {
            htRestriction = new Hashtable();
            SetDefaultRestrictions(pCS);

        }

        /// <summary>
        /// Retourne les élements soumis à restrictions 
        /// </summary>
        /// <returns>RestrictItem[]</returns>
        public RestrictionItem[] GetRestrictItem()
        {
            return TransformInArray(htRestriction);
        }

        /// <summary>
        /// Retourne les élements autorisés
        /// </summary>
        /// <returns></returns>
        public RestrictionItem[] GetRestrictItemEnabled()
        {

            RestrictionItem[] ret = null;
            //
            RestrictionItem[] restrictItem = GetRestrictItem();
            if (ArrFunc.IsFilled(restrictItem))
            {
                Hashtable clone = (Hashtable)htRestriction.Clone();
                for (int i = 0; i < restrictItem.Length; i++)
                {
                    if (false == restrictItem[i].IsEnabled)
                    {
                        clone.Remove(restrictItem[i].id);
                    }
                }
                ret = (RestrictionItem[])TransformInArray(clone);
            }
            return ret;

        }

        /// <summary>
        /// Retourne les Ids des élements autorisés
        /// <para>A utiliser uniquement lorsque les ids sont de type Numerique</para>
        /// </summary>
        public int[] GetIdEnabled()
        {
            int[] ret = null;
            RestrictionItem[] items = GetRestrictItemEnabled();
            if (ArrFunc.IsFilled(items))
            {
                ret = new int[items.Length];
                for (int i = 0; i < items.Length; i++)
                    ret[i] = items[i].id;
            }
            return ret;


        }

        /// <summary>
        /// Charge tous les éléments soumis à restrictions avec les autorisations par défaut
        /// </summary>
        protected virtual void SetDefaultRestrictions(string pCs)
        {
            throw new NotImplementedException("SetDefaultRestrictions must be override");

        }

        /// <summary>
        ///  Affecte un élément soumis à restriction
        /// <para>Si l'élément n'existe pas, il est automatiquement créé</para>
        /// </summary>
        /// <param name="pId">Représente l'élement</param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIsAuthorised"></param>
        // FI 20150529 [20982] Modify => Public Method 
        public void SetItemRestriction(int pId, string pIdentifier, bool pIsEnabled)
        {
            RestrictionItem ri = new RestrictionItem(pId, pIdentifier, pIsEnabled);
            htRestriction[ri.id] = ri;
        }

        /// <summary>
        ///  Transformation du Hastable en RestrictItem[]
        /// </summary>
        /// <param name="pHasTable"></param>
        /// <returns></returns>
        private static RestrictionItem[] TransformInArray(Hashtable pHasTable)
        {
            RestrictionItem[] ret = null;
            if (ArrFunc.IsFilled(pHasTable))
            {
                Array targetArray = Array.CreateInstance(typeof(RestrictionItem), pHasTable.Count);
                pHasTable.Values.CopyTo(targetArray, 0);
                ret = (RestrictionItem[])targetArray;
            }
            return ret;

        }
        #endregion Method
    }
    
    /// <summary>
    /// Interface que les éléments soumis à restriction doivent respectés
    /// </summary>
    public interface IRestrictionElement
    {
        
        /// <summary>
        /// Retourne la query de chargement des éléments soumis à restriction 
        /// <para>Query Sous la Forme select e.X As ID, e.Y IDENTIFIER From ELEMENT e</para>
        /// </summary>
        /// <returns></returns>
        QueryParameters GetQueryRestrictionElement();
        
        /// <summary>
        /// Retourne true si l'élément est autorisé
        /// </summary>
        /// <param name="pIde"></param>
        /// <returns></returns>
        bool IsItemEnabled(int pIdItem);
        
        
        /// <summary>
        /// Retourne le type d'élément, ce type alimente SESSIONRESTRICT.CLASS
        /// </summary>
        string Class { get;}
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class RestrictionElement : RestrictionBase
    {
        #region Members
        private readonly IRestrictionElement restrictionElement;
        #endregion Members

        #region properties
        /// <summary>
        /// 
        /// </summary>
        public override string Class
        {
            get
            {
                return restrictionElement.Class;
            }
        }
        #endregion properties

        #region constructor
        public RestrictionElement(IRestrictionElement pRestrictionElement)
            : base()
        {
            restrictionElement = pRestrictionElement;
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        // EG 20180425 Analyse du code Correction [CA2202]
        protected override void SetDefaultRestrictions(string pCS)
        {
            QueryParameters qry = restrictionElement.GetQueryRestrictionElement();

            using (IDataReader dr = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader())
            {
                while (dr.Read())
                {
                    bool isEnabled = restrictionElement.IsItemEnabled(Convert.ToInt32(dr["ID"]));
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), Convert.ToString(dr["IDENTIFIER"]), isEnabled);
                }
            }
        }
        #endregion Methods
    }
    
    /// <summary>
    /// Class de base des éléments soumis à restriction vis à vis d'un utilisateur 
    /// <para>
    /// (restrictions sur les Actors, les instruments, les permissions,etc..)
    /// </para>
    /// </summary>
    public abstract class RestrictionUserBase : RestrictionBase
    {
        #region Members
        /// <summary>
        /// Représente l'utilisateur
        /// </summary>
        private readonly User _user;
        #endregion Members
        
        #region Accessor
        /// <summary>
        /// Obtient l'utilisateur
        /// </summary>
        public User User
        {
            get { return _user; }
        }
        
        /// <summary>
        /// Obtient true si l'utilisateur accède à tous les éléments
        /// <para>Lorsque cette méthode n'est pas overridée, seul l'acteur SYSADM accède à tous les éléments</para>
        /// </summary>
        public virtual bool IsAllElementEnabled
        {
            get { return User.IsSessionSysAdmin; }
        }
        
        /// <summary>
        /// Obtient true si l'utilisateur est Guest
        /// </summary>        
        public bool IsNoneElementEnabled
        {
            get { return User.IsSessionGuest; }
        }
        #endregion
        
        #region constructor
        public RestrictionUserBase(User pUser)
            : base()
        {
            _user = pUser;
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150529 [20982] Add 
        public RestrictionUserBase()
        {
        }

        #endregion constructor
        
        #region Method
        
        /// <summary>
        /// Charge les éléments soumis à restrictions avec les valeurs par défaut
        /// <para>Spheres applique éventuellement les restrictions appliquées aux acteurs parents (voir IsApplyAncestorRestriction)</para>
        /// </summary>
        /// <param name="pCS"></param>
        public override void Initialize(string pCS)
        {
            //
            base.Initialize(pCS);
            //
            if (IsApplyAncestorRestriction(pCS))  
                SetActorAncestorRestrictions(pCS);
        }

        /// <summary>
        /// Retourne true Si Spheres® doit appliquer les restrictions associées aux acteurs parents
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected virtual bool IsApplyAncestorRestriction(string pCS)
        {
            //Si l'acteur admin, il accède à tout par défaut
            //Il ne faut pas appliquer des restrictions qui pourraient être appliquées à acteurs parents
            return ((false == IsAllElementEnabled));
        }



        /// <summary>
        /// Applique les restrictions rattaché à un acteur  
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdA">Représente l'acteur</param>
        protected virtual void SetActorRestrictions(string pCs, int pIdA)
        {
            throw new NotImplementedException("SetActorRestrictions must be override");
        }
        
        
        /// <summary>
        /// Applique les restrictions attibuées aux acteurs parents
        /// </summary>
        /// <param name="pCS"></param>
        private void SetActorAncestorRestrictions(string pCS)
        {

            IDataReader dr = null;
            try
            {
                ActorAncestor actorAncestor = User.ActorAncestor;
                //
                //Récupération des users parents du current user
                if (null != actorAncestor)
                {
                    ArrayList relations = actorAncestor.GetRelations();
                    ArrayList aActorRelation;
                    //	
                    // Spheres® part de la relation la plus élévée
                    for (int level = relations.Count - 1; level >= 0; level--)
                    {
                        aActorRelation = (ArrayList)relations[level];       // Liste des acteurs Ancestors du level en cours
                        for (int i = 0; i < aActorRelation.Count; i++)
                        {
                            int idAParent = ((ActorRelation)aActorRelation[i]).ActorRole.IdA_Actor;
                            SetActorRestrictions(pCS, idAParent);
                        }
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    ///  Permissions rattachées à un menu
    /// </summary>
    public class RestrictionPermission : RestrictionUserBase
    {
        #region Members
        private readonly string idMenu;
        #endregion Members

        #region accessor
        public bool IsCreate
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.Create);
            }

        }
        public bool IsModify
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.Modify);
            }
        }
        public bool IsModifyPostEvts
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.ModifyPostEvts);
            }
        }
        // EG 20240123 [WI816] Trade input: Implemented a new action to modify uninvoiced fees - GUI
        public bool IsModifyFeesUninvoiced
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.ModifyFeesUninvoiced);
            }
        }
        public bool IsRemove
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.Remove);
            }
        }
        public bool IsEnabledDisabled
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.EnabledDisabled);
            }
        }
        public bool IsFourEyesMaking
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.FourEyesMaking);
            }
        }
        //PL 20161124 - RATP 4Eyes - MakingChecking
        public bool IsFourEyesChecking
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.FourEyesChecking);
            }
        }

        public bool IsImport
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.Import);
            }
        }
        // EG 20180525 [23979] IRQ Processing
        public bool IsIRQPrivate
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.IRQPrivate);
            }
        }
        // EG 20180525 [23979] IRQ Processing
        public bool IsIRQPublic
        {
            get
            {
                return IsPermissionEnabled(PermissionEnum.IRQPublic);
            }
        }

        /// <summary>
        /// Obtient true si l'utilisateur accède à tous les éléments
        /// <para>
        /// Obtient true si l'acteur est SysAdmin ou SysOper
        /// </para>
        /// </summary>
        public override bool IsAllElementEnabled
        {
            get
            {
                return User.IsSessionSysAdmin || User.IsSessionSysOper;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override string Class
        {
            get { return Cst.OTCml_TBL.PERMISSION.ToString(); }
        }

        #endregion accessor

        #region constructor
        public RestrictionPermission(string pIdMenu, User pUser)
            : base(pUser)
        {
            idMenu = pIdMenu;
        }
        #endregion constructor

        #region Methodes
        /// <summary>
        /// Charge les permissions avec leur restriction par défaut
        /// </summary>
        /// <param name="pCS"></param>
        protected override void SetDefaultRestrictions(string pCS)
        {
            //
            string columnEnabled = "p.ISENABLED As ISENABLED";

            //20080107 PL Add of 'if'
            if (IsNoneElementEnabled | IsCSNonePermission(pCS))
            {
                //User Guest --> None permission
                columnEnabled = "0 As ISENABLED";
            }
            else if (IsAllElementEnabled)
            {
                //User System --> All permissions
                columnEnabled = "1 As ISENABLED";
            }
            //				
            string sqlQuery = SQLCst.SELECT + "p.IDPERMISSION As ID,p.PERMISSION As IDENTIFIER," + columnEnabled + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.PERMISSION + " p" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "p.IDMENU=@IDMENU";
            //
            //Tuning: On ne récupère ici que les IsEnabled car les properties (iscCreate) sont initialisées à false.
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDMENU", DbType.AnsiString, 64), idMenu);
            //
            QueryParameters qry = new QueryParameters(pCS, sqlQuery, parameters);
            //
            IDataReader dr = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable pour le cache
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                {
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"])); ;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }

        /// <summary>
        /// Applique les restrictions rattachées à un acteur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Id de l'acteur</param>
        protected override void SetActorRestrictions(string pCS, int pIdA)
        {

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            //
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "p.IDPERMISSION As ID,p.PERMISSION As IDENTIFIER, pm.ISENABLED As ISENABLED" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORPERMISSION + " ap on (ap.IDA=a.IDA)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELPERMISSION + " mp on (mp.IDMODELPERMISSION = ap.IDMODELPERMISSION)" + Cst.CrLf;
            sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mp") + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PERMISSIONMODEL + " pm on (pm.IDMODELPERMISSION = mp.IDMODELPERMISSION)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.PERMISSION + " p on (p.IDPERMISSION = pm.IDPERMISSION) and p.IDMENU=" + DataHelper.SQLString(idMenu) + Cst.CrLf;
            sql += SQLCst.WHERE + "a.IDA=@IDA" + Cst.CrLf;
            //Rule: On traite d'abord les IsEnabled afin de rendre prioritaire (par écrasement) les not IsEnabled 
            sql += SQLCst.ORDERBY + " pm.ISENABLED" + SQLCst.DESC;
            //
            QueryParameters qry = new QueryParameters(pCS, sql.ToString(), parameters);
            //
            IDataReader dr = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable pour le cache
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                {
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"]));
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        protected override bool IsApplyAncestorRestriction(string pCS)
        {
            bool ret;
            //un user guest n'a aucune permission
            //Il ne pourra pas créer, supprimer, dupliquer, importer etc...
            //Soit ISENABLED = false pour toute les permission
            //Spheres® ne consulte pas les éventuelles permissions associées aux acteurs parents
            if (User.UserType == RoleActor.GUEST | IsCSNonePermission(pCS))
                ret = false;
            else
                ret = base.IsApplyAncestorRestriction(pCS);
            //
            return ret;
        }


        /// <summary>
        ///  Obtient true si {pPermission} est autorisée
        /// </summary>
        /// <param name="pPermissionEnum"></param>
        /// <returns></returns>
        public bool IsPermissionEnabled(PermissionEnum pPermission)
        {
            bool ret = false;
            //
            IDictionaryEnumerator listEnum = htRestriction.GetEnumerator();
            while (listEnum.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)listEnum.Current;
                RestrictionItem item = (RestrictionItem)entry.Value;
                if (item.identifier == pPermission.ToString())
                {
                    ret = item.IsEnabled;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        ///  Get Id (PERMISSION.IDPERMISSION) 
        /// </summary>
        /// <param name="pPermissionEnum"></param>
        /// <returns></returns>
        public int GetIdPermission(PermissionEnum pPermissionEnum)
        {
            int ret = 0;
            //
            IDictionaryEnumerator listEnum = htRestriction.GetEnumerator();
            while (listEnum.MoveNext())
            {
                DictionaryEntry entry = (DictionaryEntry)listEnum.Current;
                RestrictionItem item = (RestrictionItem)entry.Value;
                if (item.identifier == pPermissionEnum.ToString())
                {
                    ret = Convert.ToInt32(item.id);
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        ///  Retourne true, si connectionString autorise uniquement le select sur les tables du MPD (Insert, Update or Delete sont refusés)  
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        private static bool IsCSNonePermission(string pCS)
        {
            bool ret = false;
            CSManager csManager = new CSManager(pCS);
            if (csManager.IsUserReadOnly())
                ret = true;
            return ret;
        }



        #endregion
    }
    
    /// <summary>
    /// Restriction sur les Acteurs  
    /// Actors accessibles à un acteur connecté  
    /// </summary>
    public class RestrictionActor : RestrictionUserBase
    {
        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public override string Class
        {
            get { return Cst.OTCml_TBL.ACTOR.ToString(); }
        }

        #endregion

        #region constructor
        public RestrictionActor(User pUser)
            : base(pUser)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        // FI 20150529 [20982] Add method
        public RestrictionActor()
        {
        }


        #endregion constructor

        #region Methods
        /// <summary>
        /// Affecte tous les éléments soumis à restriction de type acteur avec les autorisations par défaut
        /// </summary>
        protected override void SetDefaultRestrictions(string pCS)
        {

            //
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ID), User.IdA);
            //
            string columnEnabled = "a.ISENABLED As ISENABLED ";
            if (IsAllElementEnabled)
                columnEnabled = "1 As ISENABLED";
            //
            StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
            sqlQuery += "a.IDA As ID,a.IDENTIFIER As IDENTIFIER," + columnEnabled + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "a.IDA != @ID" + Cst.CrLf;
            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlQuery += SQLCst.SELECT;
            sqlQuery += "a.IDA As ID,a.IDENTIFIER As IDENTIFIER, 1 AS ISENABLED" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "a.IDA = @ID" + Cst.CrLf;
            //
            QueryParameters qry = new QueryParameters(pCS, sqlQuery.ToString(), parameters);
            //
            //20070717 FI utilisation de ExecuteDataTable pour le cache
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                {
                    int idA = Convert.ToInt32(dr["ID"]);
                    string identifier = dr["IDENTIFIER"].ToString();
                    bool isEnabled = BoolFunc.IsTrue(dr["ISENABLED"]);
                    //                    
                    SetItemRestriction(idA, identifier, isEnabled); ;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        protected override void SetActorRestrictions(string pCS, int pIdA)
        {

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            //
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "a.IDA As ID,a.IDENTIFIER As IDENTIFIER, am.ISENABLED As ISENABLED," + Cst.CrLf;
            sql += "am.ISINHERITANCE, am.ISDIRECTDESCENDANT, am.IDROLEACTOR" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a1" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORACTOR + " aa on (aa.IDA=a1.IDA)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELACTOR + " ma on (ma.IDMODELACTOR = aa.IDMODELACTOR)" + Cst.CrLf;
            sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "ma") + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORMODEL + " am on (am.IDMODELACTOR = ma.IDMODELACTOR)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR + " a on (a.IDA = am.IDA)" + Cst.CrLf;
            sql += SQLCst.WHERE + "a1.IDA=@IDA" + Cst.CrLf;
            sql += SQLCst.ORDERBY + " am.ISENABLED" + SQLCst.DESC;
            //
            QueryParameters qry = new QueryParameters(pCS, sql.ToString(), parameters);
            //
            IDataReader dr = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable pour le cache
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                {
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"]));
                    SetDescendantRestriction(pCS, dr);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        /// <summary>
        /// Retourne une query au format du SGBD
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdActorAncestor"></param>
        /// <param name="pIsDirectDescendant"></param>
        /// <param name="pIdRoleActor"></param>
        /// <returns></returns>
        private string GetActorDescendantQueryByActor(string pCS, int pIdActorAncestor, bool pIsDirectDescendant, string pIdRoleActor)
        {

            StrBuilder sqlQuery = new StrBuilder();
            //
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            //
            if (DbSvrType.dbSQL == serverType)
            {
                #region SQLServer
                sqlQuery += SQLCst.WITH + "ActorChilds (IDA_ACTOR, IDA, IDROLEACTOR, LEVEL)" + Cst.CrLf;
                sqlQuery += SQLCst.AS + "(" + Cst.CrLf;
                //
                sqlQuery += SQLCst.SELECT + "ar.IDA_ACTOR, ar.IDA, ar.IDROLEACTOR, 1" + SQLCst.AS + "LEVEL" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar" + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + "ar.IDA_ACTOR=" + pIdActorAncestor.ToString() + Cst.CrLf;
                //
                sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "ar.IDA_ACTOR, ar.IDA, ar.IDROLEACTOR, LEVEL + 1" + SQLCst.AS + "LEVEL" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar" + Cst.CrLf;
                sqlQuery += SQLCst.X_INNER + "ActorChilds" + SQLCst.AS + "ac" + Cst.CrLf;
                sqlQuery += SQLCst.ON + "ar.IDA_ACTOR = ac.IDA )" + Cst.CrLf;
                //
                sqlQuery += SQLCst.SELECT + "ac.IDA as ID,a.IDENTIFIER as IDENTIFIER" + Cst.CrLf;
                sqlQuery += SQLCst.X_FROM + "ActorChilds ac" + Cst.CrLf;
                //PL 20120626 TRIM 17945
                //sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "ac.IDA", "a", DataEnum.EnabledOnly);
                sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "ac.IDA", "a", DataEnum.All);
                //
                if (StrFunc.IsFilled(pIdRoleActor))
                    sqlQuery += SQLCst.WHERE + "ac.IDROLEACTOR = " + DataHelper.SQLString(pIdRoleActor) + Cst.CrLf;
                //
                if (pIsDirectDescendant)
                    sqlQuery += (StrFunc.IsFilled(pIdRoleActor) ? SQLCst.AND : SQLCst.WHERE) + "ac.LEVEL = 1" + Cst.CrLf;
                #endregion
            }
            else if (DbSvrType.dbORA == serverType)
            {
                #region Oracle
                sqlQuery += SQLCst.SELECT + "ar.IDA as ID,a.IDENTIFIER as IDENTIFIER" + Cst.CrLf;
                sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar" + Cst.CrLf;
                //PL 20120626 TRIM 17945
                //sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "ar.IDA", "a", DataEnum.EnabledOnly);
                sqlQuery += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.ACTOR, SQLJoinTypeEnum.Inner, "ar.IDA", "a", DataEnum.All);
                //
                if (StrFunc.IsFilled(pIdRoleActor))
                    sqlQuery += SQLCst.WHERE + "ar.IDROLEACTOR = " + DataHelper.SQLString(pIdRoleActor) + Cst.CrLf;
                //
                if (pIsDirectDescendant)
                    sqlQuery += (StrFunc.IsFilled(pIdRoleActor) ? SQLCst.AND : SQLCst.WHERE) + "LEVEL = 1" + Cst.CrLf;
                //
                sqlQuery += SQLCst.START_WITH + "ar.IDA_ACTOR = " + pIdActorAncestor.ToString() + Cst.CrLf;
                sqlQuery += SQLCst.CONNECT_BY_NOCYCLE + SQLCst.PRIOR + "ar.IDA = ar.IDA_ACTOR" + Cst.CrLf;
                #endregion
            }
            else
                throw new NotImplementedException("RDBMS not implemented");
            //
            return sqlQuery.ToString();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDr"></param>
        private void SetDescendantRestriction(string pCS, IDataReader pDr)
        {

            int idActorAncestor = Convert.ToInt32(pDr["ID"]);
            bool isInheritance = Convert.ToBoolean(pDr["ISINHERITANCE"]);
            bool isEnabled = Convert.ToBoolean(pDr["ISENABLED"]);
            //
            if (isInheritance)
            {
                bool isDirectDescendant = Convert.ToBoolean(pDr["ISDIRECTDESCENDANT"]);
                string idRoleActor = (Convert.IsDBNull(pDr["IDROLEACTOR"]) ? string.Empty : pDr["IDROLEACTOR"].ToString());
                //
                string sqlDescendant = GetActorDescendantQueryByActor(pCS, idActorAncestor, isDirectDescendant, idRoleActor);
                //20070717 FI utilisation de ExecuteDataTable pour le cache
                IDataReader drActorDescendant = null;
                try
                {
                    drActorDescendant = DataHelper.ExecuteDataTable(pCS, sqlDescendant).CreateDataReader();
                    //
                    while (drActorDescendant.Read())
                        SetItemRestriction(Convert.ToInt32(drActorDescendant["ID"]), drActorDescendant["IDENTIFIER"].ToString(), isEnabled);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (null != drActorDescendant)
                        drActorDescendant.Close();
                }
            }

        }
        #endregion
    }
    
    /// <summary>
    /// Restriction sur les Instruments
    /// <para>
    /// Instruments accessibles à un acteur connecté
    /// </para>
    /// </summary>
    public class RestrictionInstrument : RestrictionUserBase
    {
        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public override string Class
        {
            get { return Cst.OTCml_TBL.INSTRUMENT.ToString(); }
        }
        #endregion accessor
        
        #region constructor
        public RestrictionInstrument(User pUser)
            : base(pUser)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        // FI 20150529 [20982] Add method
        public RestrictionInstrument()
        {
        }
        #endregion constructor
        
        #region Method
        /// <summary>
        /// Charge les instruments avec leur restriction par défaut
        /// </summary>
        /// <param name="pCS"></param>
        protected override void SetDefaultRestrictions(string pCS)
        {

            string columnEnabled = "i.ISENABLED as ISENABLED";
            if (IsAllElementEnabled)
                columnEnabled = "1 as ISENABLED";
            //
            string sqlQuery = SQLCst.SELECT + "i.IDI as ID,i.IDENTIFIER as IDENTIFIER," + columnEnabled + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i" + Cst.CrLf;
            //
            IDataReader dr = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable pour le cache    
                dr = DataHelper.ExecuteDataTable(pCS, sqlQuery).CreateDataReader();
                while (dr.Read())
                {
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"])); ;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        
        /// <summary>
        /// Applique les restrictions rattachées à un acteur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Id de l'acteur</param>
        protected override void SetActorRestrictions(string pCS, int pIdA)
        {

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            //
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "i.IDI as ID,i.IDENTIFIER as IDENTIFIER, im.ISENABLED as ISENABLED" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORINSTRUMENT.ToString() + " ai on (ai.IDA=a.IDA)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELINSTRUMENT.ToString() + " mi on (mi.IDMODELINSTRUMENT=ai.IDMODELINSTRUMENT)" + Cst.CrLf;
            sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "mi") + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENTMODEL.ToString() + " im on (im.IDMODELINSTRUMENT=mi.IDMODELINSTRUMENT)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i on (i.IDI=im.IDI)" + Cst.CrLf;
            sql += SQLCst.WHERE + "a.IDA=@IDA" + Cst.CrLf;
            //Rule: On traite d'abord les IsEnabled afin de rendre prioritaire (par écrasement) les not IsEnabled 
            sql += SQLCst.ORDERBY + "im.ISENABLED" + SQLCst.DESC;
            //
            QueryParameters qry = new QueryParameters(pCS, sql.ToString(), parameters);
            //
            IDataReader dr = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable pour le cache
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                {
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"]));
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        #endregion
    }
    
    /// <summary>
    /// Restriction sur les Marchés  
    /// <para>
    /// Marchés accessibles par un utilisatur
    /// </para>
    /// </summary>
    public class RestrictionMarket : RestrictionUserBase
    {
        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public override string Class
        {
            get { return Cst.OTCml_TBL.MARKET.ToString(); }
        }
        #endregion accessor

        #region constructor
        public RestrictionMarket(User pUser)
            : base(pUser)
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150529 [20982] Add method
        public RestrictionMarket()
        {
        }
        #endregion constructor
        
        #region method
        
        /// <summary>
        /// Charge les marchés avec leur restriction par défaut
        /// </summary>
        /// <param name="pCS"></param>
        protected override void SetDefaultRestrictions(string pCS)
        {

            string columnEnabled = "m.ISENABLED as ISENABLED";
            if (IsAllElementEnabled)
                columnEnabled = "1 as ISENABLED";
            //
            string sqlQuery = SQLCst.SELECT + "m.IDM as ID,m.IDENTIFIER as IDENTIFIER," + columnEnabled + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MARKET.ToString() + " m" + Cst.CrLf;
            //
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteDataTable(pCS, sqlQuery).CreateDataReader();
                while (dr.Read())
                {
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"])); ;
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        
        /// <summary>
        /// Applique les restrictions rattachées à un acteur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Id de l'acteur</param>
        protected override void SetActorRestrictions(string pCS, int pIdA)
        {

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            //
            StrBuilder sql = new StrBuilder(SQLCst.SELECT);
            sql += "m.IDM as ID,m.IDENTIFIER as IDENTIFIER, mmod.ISENABLED as ISENABLED" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR + " a" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORMARKET.ToString() + " am on (am.IDA=a.IDA)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELMARKET.ToString() + " modm on (modm.IDMODELMARKET=am.IDMODELMARKET)" + Cst.CrLf;
            sql += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(pCS, "modm") + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKETMODEL.ToString() + " mmod on (mmod.IDMODELMARKET=modm.IDMODELMARKET)" + Cst.CrLf;
            sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET.ToString() + " m on (m.IDM=mmod.IDM)" + Cst.CrLf;
            sql += SQLCst.WHERE + "a.IDA=@IDA" + Cst.CrLf;
            //Rule: On traite d'abord les IsEnabled afin de rendre prioritaire (par écrasement) les not IsEnabled 
            sql += SQLCst.ORDERBY + "mmod.ISENABLED" + SQLCst.DESC;
            //
            QueryParameters qry = new QueryParameters(pCS, sql.ToString(), parameters);
            //
            IDataReader dr = null;
            try
            {
                //20070717 FI utilisation de ExecuteDataTable pour le cache
                dr = DataHelper.ExecuteDataTable(qry.Cs, qry.Query, qry.Parameters.GetArrayDbParameter()).CreateDataReader();
                while (dr.Read())
                    SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"]));
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }

        }
        
        #endregion
    }
    
    /// <summary>
    /// Restriction sur les tâche IO  
    /// <para>
    /// Tâches IO accessibles par un utilisatur
    /// </para>
    /// </summary>
    public class RestrictionIOTask : RestrictionUserBase
    {
        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public override string Class
        {
            get { return Cst.OTCml_TBL.IOTASK.ToString(); }
        }
        #endregion accessor
        
        #region constructor
        
        /// <summary>
        /// 
        /// </summary>
        // FI 20150529 [20982] Add method
        public RestrictionIOTask()
        {
        }
        public RestrictionIOTask(User pUser)
            : base(pUser)
        {
        }
        #endregion constructor
        
        #region methodes
        /// <summary>
        /// Charge les tâches avec leur restriction par défaut
        /// </summary>
        /// <param name="pCS"></param>
        protected override void SetDefaultRestrictions(string pCS)
        {
            
                //Dès qu'il existe un droit public => la tâche est visible de tous
                string columnEnabled = "Case When t.RIGHTPUBLIC != 'NONE' then 1 else 0 end  as ISENABLED";
                if (IsAllElementEnabled)
                    columnEnabled = "1 as ISENABLED";
                //
                StrBuilder query = new StrBuilder(SQLCst.SELECT);
                query += "t.IDIOTASK as ID,t.IDENTIFIER as IDENTIFIER," + columnEnabled + Cst.CrLf;
                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK.ToString() + " t" + Cst.CrLf;
                //
                IDataReader dr = null;
                try
                {
                    dr = DataHelper.ExecuteDataTable(pCS, query.ToString()).CreateDataReader();
                    while (dr.Read())
                    {
                        SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), Convert.ToBoolean(dr["ISENABLED"])); ;
                    }
                }
                catch (Exception ) { throw ; }
                finally
                {
                    if (null != dr)
                        dr.Close();
                }
            
        }
        
        /// <summary>
        /// Applique les restrictions rattachées à un acteur
        /// <para>Les restrictions ne sont appliquée que si l'acteur un user </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Id de l'acteur</param>
        protected override void SetActorRestrictions(string pCS, int pIdA)
        {

            if (ActorTools.IsActorUser(pCS, pIdA))
            {
                Collaborator collaborator = new Collaborator
                {
                    Ida = pIdA
                };
                collaborator.SetUserType(pCS);
                collaborator.SetAncestor(pCS);
                User user = collaborator.NewUser();

                StrBuilder query = new StrBuilder(SQLCst.SELECT);
                query += "t.IDIOTASK as ID,t.IDENTIFIER as IDENTIFIER," + Cst.CrLf;
                query += "t.IDA,t.RIGHTPUBLIC,t.RIGHTENTITY,t.RIGHTDEPARTMENT,t.RIGHTDESK" + Cst.CrLf;
                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK.ToString() + " t" + Cst.CrLf;

                IDataReader dr = null;
                try
                {
                    dr = DataHelper.ExecuteDataTable(pCS, query.ToString()).CreateDataReader();
                    while (dr.Read())
                    {
                        int idAOwner = Convert.ToInt32(dr["IDA"]);
                        DataRights dataRights = new DataRights(pCS, idAOwner);

                        string rightPublic = Convert.ToString(dr["RIGHTPUBLIC"]);
                        string rightEntity = Convert.ToString(dr["RIGHTENTITY"]);
                        string rightDesk = Convert.ToString(dr["RIGHTDESK"]);
                        string rightDepartment = Convert.ToString(dr["RIGHTDEPARTMENT"]);

                        if (Enum.IsDefined(typeof(RightsTypeEnum), rightPublic))
                            dataRights.publicRight = (RightsTypeEnum)Enum.Parse(typeof(RightsTypeEnum), rightPublic);
                        if (Enum.IsDefined(typeof(RightsTypeEnum), rightEntity))
                            dataRights.entityRight = (RightsTypeEnum)Enum.Parse(typeof(RightsTypeEnum), rightEntity);
                        if (Enum.IsDefined(typeof(RightsTypeEnum), rightDepartment))
                            dataRights.deskRight = (RightsTypeEnum)System.Enum.Parse(typeof(RightsTypeEnum), rightDepartment);
                        if (Enum.IsDefined(typeof(RightsTypeEnum), rightDesk))
                            dataRights.deskRight = (RightsTypeEnum)System.Enum.Parse(typeof(RightsTypeEnum), rightDesk);

                        bool isEnabled = dataRights.HasUserRight(user, RightsTypeEnum.VIEW);
                        SetItemRestriction(Convert.ToInt32(dr["ID"]), dr["IDENTIFIER"].ToString(), isEnabled);
                    }
                }
                catch (Exception) { throw; }
                finally
                {
                    if (null != dr)
                        dr.Close();
                }
            }

        }
        #endregion
    }
    
}
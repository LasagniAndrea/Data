using System;
using System.Collections;  
using System.Data;


using EFS.Actor;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;


namespace EFS.Permission
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20140702 [20142] add permission ModifyMatching
    // EG 20180525 [23979] IRQ Processing Add IRQPrivate|IRQPublic
    // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
    public enum PermissionEnum
    {
        Create, Modify, Remove,
        ModifyPostEvts, ModifyMatching, ModifyFeesUninvoiced,
        Correction,
        Full,
        EnabledDisabled,
        Import,
        IRQPrivate, IRQPublic,
        Regular, Simul, Template,
        PreTrade, Executed, Intermed, Alloc,
        StatusStep, StatusCheck, StatusMatch, StatusEnvironment, StatusBusiness, StatusActivation, StatusPriority,  // Modify status
        StatusProcess,
        NA,
        //PL 20161124 - RATP 4Eyes - MakingChecking
        FourEyesMaking, FourEyesChecking
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class PermissionTools
    {

        /// <summary>
        /// Retourne la permission associée au type de saisie 
        /// <para>Retourne la permission NA </para>
        /// </summary>
        /// <param name="pCaptureMode"></param>
        /// <returns></returns>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public static PermissionEnum GetPermission(Cst.Capture.ModeEnum pCaptureMode)
        {
            PermissionEnum ret = PermissionEnum.NA;
            //
            if (Cst.Capture.IsModeNewCapture(pCaptureMode))
                ret = PermissionEnum.Create;
            else if (Cst.Capture.IsModeUpdate(pCaptureMode))
                ret = PermissionEnum.Modify;
            else if (Cst.Capture.IsModeUpdatePostEvts(pCaptureMode) ||
                     Cst.Capture.IsModeUpdateAllocatedInvoice(pCaptureMode))
                ret = PermissionEnum.ModifyPostEvts;
            else if (Cst.Capture.IsModeUpdateFeesUninvoiced(pCaptureMode))
                ret = PermissionEnum.ModifyFeesUninvoiced;
            //Bizareries
            else if (Cst.Capture.IsModeCorrection(pCaptureMode))
                ret = PermissionEnum.Correction; //FI 20110927 Il n'existe pas de permission Correction => Subtilité pour l'alimentation de TRADETRAIL
            else if (Cst.Capture.IsModeRemoveOnlyAll(pCaptureMode))
                ret = PermissionEnum.Remove; //FI 20110927 Il n'existe pas de permission Remove en saisie de trade => Subtilité pour l'alimentation de TRADETRAIL
            //
            return ret;
        }

        /// <summary>
        /// Retourne l'identifiant non significatif de la permission (OtcmlId)
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdMenu"></param>
        /// <param name="pPermission"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetIdPermission(string pCs, string pIdMenu, PermissionEnum pPermission)
        {
            return GetIdPermission(pCs, null, pIdMenu, pPermission);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public static int GetIdPermission(string pCs, IDbTransaction pDbTransaction, string pIdMenu, PermissionEnum pPermission)
        {
            int ret = 0;
            //
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDMENU", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), pIdMenu);
            parameters.Add(new DataParameter(pCs, "PERMISSION", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), pPermission.ToString());
            //
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDPERMISSION" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.PERMISSION.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDMENU=@IDMENU) and (PERMISSION=@PERMISSION)";

            Object obj = DataHelper.ExecuteScalar(pCs, pDbTransaction, CommandType.Text, sqlQuery.ToString(), parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToInt32(obj);

            return ret;
        }
        // PL 20200217 [25207] Add Method - Utilisation de la vue pour accéder aux Virtual Data (ex. OTC_INP_TRD_OPTEXE --> <999>FromExe) 
        public static string GetIdPermissionFromView(string pCs, IDbTransaction pDbTransaction, string pIdMenu, PermissionEnum pPermission)
        {
            string ret = "0";
          
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCs, "IDMENU", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), pIdMenu);
            parameters.Add(new DataParameter(pCs, "PERMISSION", DbType.AnsiString, SQLCst.UT_DISPLAYNAME_LEN), pPermission.ToString());
            
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "IDPERMISSION" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_VW_PERMIS_MENU.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(IDMENU=@IDMENU) and (PERMISSION=@PERMISSION)";

            Object obj = DataHelper.ExecuteScalar(pCs, pDbTransaction, CommandType.Text, sqlQuery.ToString(), parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToString(obj);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPermission"></param>
        /// <param name="pIsInfinitif"></param>
        /// <returns></returns>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public static string GetRessource(PermissionEnum pPermission, bool pIsInfinitif)
        {

            string ret;
            if (pIsInfinitif)
            {
                ret = Ressource.GetString(pPermission.ToString().ToUpper());
            }
            else
            {
                switch (pPermission)
                {
                    case PermissionEnum.Create:
                        ret = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.New);
                        break;
                    case PermissionEnum.Modify:
                        ret = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.Update);
                        break;
                    case PermissionEnum.ModifyPostEvts:
                        ret = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.UpdatePostEvts);
                        break;
                    case PermissionEnum.ModifyFeesUninvoiced:
                        ret = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.UpdateFeesUninvoiced);
                        break;
                    case PermissionEnum.Remove:
                        ret = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.RemoveOnly);
                        break;
                    default:
                        ret = pPermission.ToString();
                        break;
                }
            }
            return ret;
        }

    }
}

namespace EFS.Rights
{

    /// <summary>
    /// Représente le type droits attribués à une donnée
    /// <para>L'attribution d'un droit de niveau n donne automatiquement les droits n-1</para>
    /// <para>Exemple, l'attribution du droit SAVE donne le droit MODIFY, ce dernier donne à son tour le droit VIEW </para>
    /// </summary>
    public enum RightsTypeEnum
    {
        /// <summary>
        /// Aucun droit
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Droit d'ouvrir
        /// </summary>
        VIEW = 1,
        /// <summary>
        /// Droit de modifier sans pourvoir enregister
        /// </summary>
        MODIFY = 2,
        /// <summary>
        /// Droit de modifier et de sauvegarder
        /// </summary>
        SAVE = 3,
        /// <summary>
        /// Droit de suprimer
        /// </summary>
        REMOVE = 4
    };

    /// <summary>
    /// Représente les droits que donne un propriétaire à une de ses données
    /// <para>Utiliser par les templates et les tâches IO</para>
    /// </summary>
    //FI 20100728 Add DataRights         
    public class DataRights
    {
        //
        #region Members
        /// <summary>
        ///  Représente le propriétaire des données
        /// </summary>
        private int _idAOwner;
        /// <summary>
        ///  Représente l'entité rattaché au propriétaire
        /// </summary>
        private int _idAOwnerEntity;
        /// <summary>
        ///  Représente le département rattaché au propriétaire
        /// </summary>
        private int _idAOwnerDepartment;
        /// <summary>
        ///  Représente le desk rattaché au propriétaire
        /// </summary>
        private int _idAOwnerDesk;

        /// <summary>
        /// Obtient ou définit les droits attribués à tout le monde 
        /// </summary>
        public RightsTypeEnum publicRight;
        /// <summary>
        /// Obtient ou définit  les droits attribués au membres du desk du propriétaire
        /// </summary>
        public RightsTypeEnum deskRight;
        /// <summary>
        /// Obtient ou définit  les droits attribués au membres du département du propriétaire
        /// </summary>
        public RightsTypeEnum departmentRight;
        /// <summary>
        /// Obtient ou définit  les droits attribués au membres de l'entité du propriétaire
        /// </summary>
        public RightsTypeEnum entityRight;
        #endregion
        //
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA">Représente le propriétaire</param>
        public DataRights(string pCS, int pIdA)
        {
            SetOwner(pCS, pIdA);
            
            publicRight = RightsTypeEnum.NONE;
            deskRight = RightsTypeEnum.NONE;
            departmentRight = RightsTypeEnum.NONE;
            entityRight = RightsTypeEnum.NONE;
        }
        #endregion
        //
        #region SetOwner
        /// <summary>
        /// Définit le propriétaire
        /// <para>Le propriétaire n'est pas nécessairement un user (Faut-il le conserver??)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        // EG 20220519 [WI637] Ajout paramètre pRoleTypeExclude sur signature ActorAncestor
        public void SetOwner(string pCS, int pIdA)
        {
            _idAOwner = pIdA;

            ActorAncestor aaOwner = new ActorAncestor(pCS, null, pIdA, pRoleRestrict: null, pRoleTypeExclude: null, pIsPartyRelation: false);
            _idAOwnerEntity = aaOwner.GetFirstRelation(RoleActor.ENTITY);
            _idAOwnerDepartment = aaOwner.GetFirstRelation(RoleActor.DEPARTMENT);
            _idAOwnerDesk = aaOwner.GetFirstRelation(RoleActor.DESK);
        }
        #endregion
        //
        #region HasUserRight
        /// <summary>
        /// Retourne true si un utilisateur a un droit particulier sur l'objet
        /// </summary>
        /// <returns></returns>
        /// <param name="pUser">Représente l'utilisateur</param>
        /// <param name="pRightType">Représente le droit demandé</param>
        /// <returns></returns>
        // EG 20160404 Migration vs2013
        public bool HasUserRight( User pUser, RightsTypeEnum pRightType)
        {

            // if user is admin or owner of the template: always allowed
            bool ret = pUser.IsSessionSysAdmin || pUser.IdA == _idAOwner;

            if (false == ret)
            {
                int userEntity = pUser.Entity_IdA;
                int userDepartment = pUser.GetDepartment();
                int userDesk = pUser.GetDesk();
                
                // if user and owner have same Desk, so we use the Desk Right
                bool useDeskRight = ((0 != _idAOwnerDesk) && (userDesk == _idAOwnerDesk));
                // if user and owner have same Department, so we use the Department Right
                bool useDepartmentRight = ((0 != _idAOwnerDepartment) && (userDepartment == _idAOwnerDepartment));
                // if user and owner have same Entity, so we use the Entity Right
                bool useEntityRight = ((0 != _idAOwnerEntity) && (userEntity == _idAOwnerEntity));

                //Rights are applied in priority for Desk -> if not then Department -> if not then Entity -> if not then public       
                //the level in the Rights enum determines if allowed or not
                ret = (useDeskRight && (pRightType <= deskRight))	            //Desk
                    || (useDepartmentRight && (pRightType <= departmentRight))	//Department
                    || (useEntityRight && (pRightType <= entityRight))	        //Entity
                    || (pRightType <= publicRight);                             //Public
            }
            return ret;
        }
        #endregion
    }
}
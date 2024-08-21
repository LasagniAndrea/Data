using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using EFS.Restriction;
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Permission;
using EFS.Tuning;

namespace EFS.Common
{
    /// <summary>
    /// Représente l'environnement rattaché à un utilisateur lorsqu'il ouvre un menu
    /// <para>Par environnement, Spheres® considère le menu, le type de saisie(création, modification), actionTuning etc...</para>
    /// </summary>
    public class InputUser
    {
        #region Membres
        /// <summary>
        /// Représente le menu principal de la saisie
        /// </summary>
        private string _idMenu;
        /// <summary>
        /// Représente le type de saisie
        /// </summary>
        private Cst.Capture.ModeEnum _captureMode;
        /// <summary>
        /// Représente l'utilisateur Spheres®
        /// </summary>
        private readonly User _user;
        /// <summary>
        /// Représente les permissions rattachées au menu principal
        /// </summary>
        private RestrictionPermission _permission;
        /// <summary>
        /// Représente ActionTuning rattaché au menu principal
        /// </summary>
        private ActionTuning _actionTuning;


        #endregion Membres

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public Cst.Capture.ModeEnum CaptureMode
        {
            get { return _captureMode; }
            set { _captureMode = value; }
        }
        /// <summary>
        /// Obtient ou définit les permissions rattachées au menu principal
        /// </summary>
        public RestrictionPermission Permission
        {
            get { return _permission; }
            //set { m_Permission = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string IdMenu
        {
            get { return _idMenu; }
            set { _idMenu = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsCreateAuthorised
        {
            get { return _permission.IsPermissionEnabled(PermissionEnum.Create); }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsModifyAuthorised
        {
            get { return _permission.IsPermissionEnabled(PermissionEnum.Modify); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsModifyPostEvtsAuthorised
        {
            get { return _permission.IsPermissionEnabled(PermissionEnum.ModifyPostEvts); }
        }

        // EG 20240123 [WI816] Trade input: Implemented a new action to modify uninvoiced fees - GUI
        public bool IsModifyFeesUninvoicedAuthorised
        {
            get { return _permission.IsPermissionEnabled(PermissionEnum.ModifyFeesUninvoiced); }
        }
        /// <summary>
        /// Obtient true si la permission ModifyMatching est aotorisée 
        /// </summary>
        /// FI 20140708 [20179] add
        public bool IsModifyMatchingAuthorised
        {
            get { return _permission.IsPermissionEnabled(PermissionEnum.ModifyMatching); }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsRemoveAuthorised
        {
            get { return _permission.IsPermissionEnabled(PermissionEnum.Remove); }
        }
        /// <summary>
        /// 
        /// </summary>
        public ActionTuning ActionTuning
        {
            get { return _actionTuning; }
        }
        /// <summary>
        /// 
        /// </summary>
        public User User
        {
            get { return _user; }
        }
        #endregion

        #region constructor
        public InputUser(string pIdMenu, User pUser)
        {
            _idMenu = pIdMenu;
            _user = pUser;
        }
        #endregion


        /// <summary>
        /// <para>Initialise les permissions rattachées au menu</para>
        /// <para>Initialise le type de saisie (notamment en fonction des permissions)</para>
        /// </summary>
        /// <param name="pCS"></param>
        public virtual void InitializeFromMenu(string pCS)
        {
            //InitPermission
            _permission = new RestrictionPermission(_idMenu, _user);
            _permission.Initialize(pCS);
            //
            //InitCaptureMode
            InitCaptureMode();
        }

        /// <summary>
        /// Initialise le type de saisie(New ou Consult)
        /// <para>L'initialisation est fonction des permissions</para>
        /// </summary>
        public virtual void InitCaptureMode()
        {
            _captureMode = IsCreateAuthorised ? Cst.Capture.ModeEnum.New : Cst.Capture.ModeEnum.Consult;
        }

        /// <summary>
        /// Initialise actionTuning en fonction type de saisie (CaptureMode) et des permissions associées 
        /// <para>Action Tuning contient également les statuts par défaut à proposer sur le trade</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pIdI">Instruement</param>
        public void InitActionTuning(string pCs, int pIdI)
        {
            int idPermission = Permission.GetIdPermission(PermissionTools.GetPermission(CaptureMode));
            _actionTuning = new ActionTuning(pCs, pIdI, idPermission, _user.IdA, _user.ActorAncestor);
        }

    }
}

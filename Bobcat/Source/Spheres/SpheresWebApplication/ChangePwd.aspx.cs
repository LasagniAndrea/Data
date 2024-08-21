using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.HtmlControls;

public partial class ChangePwd : PageBase
{
    #region Members
    /// <summary>
    /// true lorsque Spheres® propose le chgt de pwd (ACTOR.ISPWDMODNEXTLOGON)
    /// </summary>
    private bool isPwdAutoMode;
    /// <summary>
    /// true lorsque Spheres® propose le chgt de pwd lorsque la date d'expirartion est proche
    /// </summary>
    private bool isBeforeExp;
    /// <summary>
    /// true lorsque Spheres® propose le chgt de pwd automatiquement
    /// <para>Ds ce cas le formulaire s'ouvre ds Sommaire.aspx</para>
    /// </summary>
    private bool isForcedChangeAllowed;
    /// <summary>
    /// true lorsque cette fenêtre est ouverte depuis le formulaire Profil.aspx
    /// <para>Ds ce cas le formulaire s'ouvre ds Sommaire.aspx</para>
    /// </summary>
    private bool isFromProfil;
    /// <summary>
    /// Règles de sécurité du mot de passe pour l'acteur connecté
    /// </summary>
    private ActorModelSafetyPassword actorModelSafetyPassword;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
    protected override void OnInit(EventArgs e)
    {
        isPwdAutoMode = ("1" == this.Request.QueryString["PWDAUTOMODE"]);
        isBeforeExp = ("1" == this.Request.QueryString["BEFOREEXP"]);
        isFromProfil = ("1" == this.Request.QueryString["FROMPROFIL"]);
        isForcedChangeAllowed = isPwdAutoMode || isBeforeExp;

        base.OnInit(e);

        this.Form = frmChangePwd;
        AntiForgeryControl();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    // EG 20190125 DOCTYPE Conformity HTML5
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    // EG 20220123 [26235][WI543] Refactoring
    protected void Page_Load(object sender, System.EventArgs e)
    {
        actorModelSafetyPassword = new ActorModelSafetyPassword(SessionTools.CS, SessionTools.Collaborator_IDA);

        string title = "Mot de Passe";
        PageTools.SetHead(this, title, null, null);

        divpwd.CssClass = CSSMode;
        divbody.CssClass = CSSMode;
        lblPwdMsg.Text = string.Empty;

        btnRecord.Text = @"<i class='fa fa-save'></i> " + Ressource.GetString(btnRecord.ID);
        btnCancel.Text = @"<i class='fa fa-times'></i> " + Ressource.GetString(btnCancel.ID);
        btnPostpone.Text = @"<i class='fa fa-history'></i> " + Ressource.GetString(btnPostpone.ID);
        HtmlContainerControl ctrl = (HtmlContainerControl)this.FindControl("PasswordTitle");
        if (null != ctrl)
            ctrl.InnerText = Ressource.GetString("PasswordTitle2");

        btnPostpone.Visible = (isPwdAutoMode || isBeforeExp);
        btnCancel.Visible = isFromProfil;

        if (isPwdAutoMode)
        {
            if (false == IsPostBack)
                lblPwdMsg.Text = Ressource.GetString("Msg_PWNeededToBeChanged");

            btnPostpone.Enabled = (actorModelSafetyPassword.PWDREMAININGGRACE > 0);
            if (btnPostpone.Enabled)
                btnPostpone.ToolTip += " ( " + (actorModelSafetyPassword.PWDREMAININGGRACE).ToString() + " " + Ressource.GetString("RemainingGrace") + " )";
        }
        else if (isBeforeExp)
        {
            if (false == IsPostBack)
                lblPwdMsg.Text = Ressource.GetString2("Msg_PWNearlyExpired", SessionTools.Collaborator.NbDayPwdExpiration.ToString());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// RD 20220214 [25943] Modify
    /// RD 20220309 [25943] Modify
    /// EG 20220123 [26235][WI543] Refactoring
    private void Save()
    {

        string displayMessage = string.Empty;
        if (txtNewPassword.Text == txtConfirmPassword.Text)
        {
            if (actorModelSafetyPassword.Authentify(txtOldPassword.Text))
                actorModelSafetyPassword.SetNewPassword(SessionTools.CS, txtNewPassword.Text, txtOldPassword.Text, isForcedChangeAllowed);

            switch (actorModelSafetyPassword.CodeError)
            {
                // OK
                case ActorModelSafetyPassword.ErrorCode.SUCCESS:
                    {
                        // EG 20220123 [26235][WI543]
                        SessionTools.ActionOnConnect = Cst.ActionOnConnect.VALIDPWDCHANGING;
                        if (isForcedChangeAllowed)
                            JavaScript.ExecuteImmediate(this, "window.location.href='Default.aspx';", true);
                        else if (isFromProfil)
                            Response.Redirect("Profile.aspx?Msg=succes", true);
                        else
                            Response.Redirect("welcome.aspx", true);
                        break;
                    }
                // PW USER ERRORS
                case ActorModelSafetyPassword.ErrorCode.UNCHECKOLD:
                    displayMessage = Ressource.GetString("Msg_PWincorrect", false); //Mot de passe erroné ou incorrect.
                    break;
                case ActorModelSafetyPassword.ErrorCode.NOTALLOWED:
                    displayMessage = Ressource.GetString("Msg_PWchangeNotAllowed", false); // Vous n'avez pas l'autorisation pour changer de mot de passe.
                    break;
                case ActorModelSafetyPassword.ErrorCode.IDENTIFIER:
                    displayMessage = Ressource.GetString("Msg_PWsameAsIdentifier", false); // Le mot de passe ne peut être identique à l'identifiant.
                    break;
                case ActorModelSafetyPassword.ErrorCode.ALREADYUSED:
                    displayMessage = Ressource.GetString("Msg_PWalreadyUsed", false); // Le nouveau mot de passe doit être différent des anciens mots de passe.
                    break;
                case ActorModelSafetyPassword.ErrorCode.LENGTH:
                    displayMessage = Ressource.GetString("Msg_PWinvalidLength", false); // La longueur du nouveau mot de passe est incorrecte ; min: {0}, max: {1}.
                    displayMessage = displayMessage.Replace("{0}", actorModelSafetyPassword.NBCHARMIN.ToString());
                    displayMessage = displayMessage.Replace("{1}", actorModelSafetyPassword.NBCHARMAX.ToString());
                    break;
                case ActorModelSafetyPassword.ErrorCode.DIGIT:
                    displayMessage = Ressource.GetString("Msg_PWinvalidDigit", false); // Le nouveau mot de passe doit comporter au moins {0} caractère(s) de type numérique.
                    displayMessage = displayMessage.Replace("{0}", actorModelSafetyPassword.NBDIGITMIN.ToString());
                    break;
                case ActorModelSafetyPassword.ErrorCode.WILDCARD:
                    displayMessage = Ressource.GetString("Msg_PWinvalidWildCard", false); // Le nouveau mot de passe doit comporter au moins {0} caractère(s) de type spécial.
                    displayMessage = displayMessage.Replace("{0}", actorModelSafetyPassword.NBWILDCARDMIN.ToString());
                    break;
                case ActorModelSafetyPassword.ErrorCode.LOWER:
                    displayMessage = Ressource.GetString("Msg_PWinvalidLowerChar", false); // Le nouveau mot de passe doit comporter au moins {0} minuscule(s).
                    displayMessage = displayMessage.Replace("{0}", actorModelSafetyPassword.NBLOWERCHARMIN.ToString());
                    break;
                case ActorModelSafetyPassword.ErrorCode.UPPER:
                    displayMessage = Ressource.GetString("Msg_PWinvalidUpperChar", false); // Le nouveau mot de passe doit comporter au moins {0} majuscules(s).
                    displayMessage = displayMessage.Replace("{0}", actorModelSafetyPassword.NBUPPERCHARMIN.ToString());
                    break;
                /// EG 20151215 [21305] New 
                case ActorModelSafetyPassword.ErrorCode.PWDFORBIDDEN:
                    displayMessage = Ressource.GetString("Msg_PWForbidden", false); // Le nouveau mot de passe est interdit.
                    break;

                //INTERNAL ERRORS
                case ActorModelSafetyPassword.ErrorCode.MISCELLANEOUS:
                case ActorModelSafetyPassword.ErrorCode.NOTFOUND:
                case ActorModelSafetyPassword.ErrorCode.PWDEXPIRED_H:
                case ActorModelSafetyPassword.ErrorCode.BUG:
                    displayMessage = Ressource.GetString2("Msg_PWnotUpdated", actorModelSafetyPassword.CodeError.ToString()); // Le mot de passe n'a pas été mis à jour.
                    break;
            }
        }
        else
            displayMessage = Ressource.GetString("Msg_PWinvalidConfirm", false); // La confirmation est invalide.    

        if (displayMessage.Length > 0)
            lblPwdMsg.Text = displayMessage;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void BtnRecord_Click(object sender, System.EventArgs e)
    {
        Save();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    // EG 20220123 [26235][WI543] Refactoring
    protected void BtnCancel_Click(object sender, System.EventArgs e)
    {
        Response.Redirect(isFromProfil? "profile.aspx":"welcome.aspx", true);
    }

    // EG 20220123 [26235][WI543] New
    protected void BtnPostpone_Click(object sender, System.EventArgs e)
    {
        //Changement forcé
        if (isPwdAutoMode)
        {
            actorModelSafetyPassword = new ActorModelSafetyPassword(SessionTools.CS, SessionTools.Collaborator_IDA);
            actorModelSafetyPassword.DecrementGrace(SessionTools.CS);
            SessionTools.ActionOnConnect = Cst.ActionOnConnect.POSTPONEPWDCHANGING;
            JavaScript.ExecuteImmediate(this, "window.location.href='Default.aspx';", true);
        }
        else if (isBeforeExp)
        {
            SessionTools.ActionOnConnect = Cst.ActionOnConnect.POSTPONEPWDCHANGING;
            JavaScript.ExecuteImmediate(this, "window.location.href='Default.aspx';", true);
        }
    }

    /// EG 20151215 [21305] New 
    /// FI 20230117 [XXXXX] Gestion de LOWER et UPPER 
    private class ActorModelSafetyPassword
    {
        #region Members
        public enum ErrorCode
        {
            SUCCESS,
            BUG,
            MISCELLANEOUS,
            UNCHECKOLD,
            NOTFOUND,
            NOTALLOWED,
            IDENTIFIER,
            ALREADYUSED,
            LENGTH,
            DIGIT,
            WILDCARD,
            LOWER,
            UPPER,
            PWDEXPIRED_H,
            /// EG 20151215 [21305] New 
            PWDFORBIDDEN,
        }
        public ErrorCode CodeError = ErrorCode.MISCELLANEOUS;

        private readonly int IDA;
        private string IDENTIFIER; 
        private string PWD;
        private string previousPWD;

        private int IDMODELSAFETY;
        private bool PWDMODPERMITTED;
        private bool PWDDIFFIDENTIFIER;
        private bool PWDCRYPTED;
        private bool PWDMODPERIODIC;
        private int NBDAYVALID;
        private int NBDAYWARNING;
        private int NBMONTHPWDFORBIDDEN;
        private int ALLOWEDGRACE;

        public int NBCHARMIN;
        public int NBCHARMAX;
        public int NBDIGITMIN;
        public int NBWILDCARDMIN;
        public int NBLOWERCHARMIN;
        public int NBUPPERCHARMIN;
        public int PWDREMAININGGRACE;
        /// EG 20151215 [21305] New 
        private List<string> PWDFORBIDDENLIST;
        #endregion Members
        #region Accessors
        /// EG 20151215 [21305] New 
        private object RemainingGrace
        {
            get
            {
                object grace = DBNull.Value;
                if (ALLOWEDGRACE > 0)
                    grace = ALLOWEDGRACE;
                return grace;
            }
        }
        /// EG 20151215 [21305] New
        private Nullable<DateTime> DtPWDExpiration
        {
            get
            {
                Nullable<DateTime> dt = null;
                if (PWDMODPERIODIC)
                {
                    int nbDay = 0;
                    if (NBDAYVALID > 0)
                        nbDay = NBDAYVALID;
                    dt = OTCmlHelper.GetDateSys(SessionTools.CS).Date.AddDays(nbDay); // FI 20201006 [XXXXX] .Date
                }
                return dt;
            }
        }
        #endregion Accessors
        #region Constructor
        public ActorModelSafetyPassword(string pCS, int pIdA)
        {
            IDA = pIdA;
            Load(pCS, pIdA);
        }
        #endregion Constructor

        #region Methods
        #region Load
        /// <summary>
        /// Chargement des règle de sécurité (MODELSAFETY) de l'acteur
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// EG 20151215 [21305] Refactoring, Add PWDFORBIDDENLIST
        private void Load(string pCS, int pIdA)
        {
            IDataReader dr = null;

            try
            {
                string sqlSelect = @"select a.IDMODELSAFETY,a.PWD,a.IDENTIFIER,a.PWDREMAININGGRACE,
                ms.PWDMODPERMITTED,ms.PWDDIFFIDENTIFIER,ms.PWDCRYPTED,ms.PWDMODPERIODIC,ms.NBDAYVALID,ms.ALLOWEDGRACE,ms.NBDAYWARNING,ms.NBMONTHPWDFORBIDDEN, 
                ms.NBCHARMIN,ms.NBCHARMAX,ms.NBDIGITMIN,ms.NBWILDCARDMIN,ms.PWDFORBIDDENLIST,ms.NBLOWERCHARMIN,ms.NBUPPERCHARMIN
                from dbo.ACTOR a
                inner join dbo.MODELSAFETY ms on (ms.IDMODELSAFETY=a.IDMODELSAFETY)
                where (a.IDA=@IDA)" + Cst.CrLf;

                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
                QueryParameters qry = new QueryParameters(pCS, sqlSelect, parameters);
                dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                if (dr.Read())
                {
                    PWDMODPERMITTED = Convert.ToBoolean(dr["PWDMODPERMITTED"]);
                    PWDDIFFIDENTIFIER = Convert.ToBoolean(dr["PWDDIFFIDENTIFIER"]);
                    PWDCRYPTED = Convert.ToBoolean(dr["PWDCRYPTED"]);
                    PWDMODPERIODIC = Convert.ToBoolean(dr["PWDMODPERIODIC"]);
                    NBDAYVALID = Convert.ToInt32((dr["NBDAYVALID"].ToString().Length > 0 ? dr["NBDAYVALID"] : 0));
                    ALLOWEDGRACE = Convert.ToInt32((dr["ALLOWEDGRACE"].ToString().Length > 0 ? dr["ALLOWEDGRACE"] : 0));
                    NBDAYWARNING = Convert.ToInt32((dr["NBDAYWARNING"].ToString().Length > 0 ? dr["NBDAYWARNING"] : 0));
                    NBMONTHPWDFORBIDDEN = Convert.ToInt32(dr["NBMONTHPWDFORBIDDEN"]);
                    NBCHARMIN = Convert.ToInt32(dr["NBCHARMIN"]);
                    NBCHARMAX = Convert.ToInt32(dr["NBCHARMAX"]);
                    NBDIGITMIN = Convert.ToInt32(dr["NBDIGITMIN"]);
                    NBWILDCARDMIN = Convert.ToInt32(dr["NBWILDCARDMIN"]);
                    NBLOWERCHARMIN = Convert.ToInt32(dr["NBLOWERCHARMIN"]);
                    NBUPPERCHARMIN = Convert.ToInt32(dr["NBUPPERCHARMIN"]);
                    IDMODELSAFETY = Convert.ToInt32(dr["IDMODELSAFETY"]);
                    PWD = Convert.ToString(dr["PWD"]);
                    IDENTIFIER = Convert.ToString(dr["IDENTIFIER"]);
                    PWDREMAININGGRACE = Convert.ToInt32((dr["PWDREMAININGGRACE"].ToString().Length > 0 ? dr["PWDREMAININGGRACE"] : -1));
                    previousPWD = PWD;
                    /// EG 20151215 [21305] New 
                    if (false == (dr["PWDFORBIDDENLIST"] is DBNull))
                    {
                        PWDFORBIDDENLIST = new List<string>(Convert.ToString(dr["PWDFORBIDDENLIST"]).Split(new char[] { ';' },
                            StringSplitOptions.RemoveEmptyEntries));
                    }

                    CodeError = ErrorCode.SUCCESS;
                }
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }
        }
        #endregion
        #region Authentify
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPassword"></param>
        /// <returns></returns>
        public bool Authentify(string pPassword)
        {
            CodeError = ErrorCode.MISCELLANEOUS;
            try
            {
                if (PWD == GetHashed(pPassword))
                    CodeError = ErrorCode.SUCCESS;
                else
                    CodeError = ErrorCode.UNCHECKOLD;
            }
            catch
            {
                CodeError = ErrorCode.BUG;
            }
            return (CodeError == ErrorCode.SUCCESS);
        }
        #endregion
        #region DecrementGrace
        /// <summary>
        /// Mise à jour de la grâce (Décrémentation)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        public void DecrementGrace(string pCS)
        {
            string SQLQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.ACTOR.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.SET + @"PWDREMAININGGRACE = PWDREMAININGGRACE - 1" + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "IDA=" + IDA.ToString();
            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery);
        }
        #endregion

        /// <summary>
        /// Mise à jour du nouveau mot de passe
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pNewPassword"></param>
        /// <param name="pOldPassword"></param>
        /// <param name="pIsForcedChangePermission"></param>
        /// <returns></returns>
        /// EG 20151215 [21305] Refactoring
        // EG 20210209 [25660] Upd Taille du paramètre PWD
        // RD 20220214 [25943] Add parameter pOldPassword
        public ErrorCode SetNewPassword(string pCS, string pNewPassword, string pOldPassword, bool pIsForcedChangePermission)
        {
            CodeError = ErrorCode.MISCELLANEOUS;

            try
            {
                if (false == (pIsForcedChangePermission || PWDMODPERMITTED))
                    CodeError = ErrorCode.NOTALLOWED;
                else if (MatchRules(pCS, pNewPassword, pOldPassword))
                {
                    string sqlUpdate = @"update dbo.ACTOR set PWD=@PWD, ISPWDMODNEXTLOGON=@ISPWDMODNEXTLOGON, DTPWDEXPIRATION=@DTPWDEXPIRATION, PWDREMAININGGRACE=@PWDREMAININGGRACE" + Cst.CrLf;
                    sqlUpdate += " where (IDA = @IDA)";

                    string hashData = GetHashed(pNewPassword);
                    DataParameters parameters = new DataParameters();
                    parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);
                    parameters.Add(new DataParameter(pCS, "PWD", DbType.AnsiString, 128), hashData);
                    parameters.Add(new DataParameter(pCS, "ISPWDMODNEXTLOGON", DbType.Boolean), false);
                    parameters.Add(new DataParameter(pCS, "DTPWDEXPIRATION", DbType.Date), DtPWDExpiration ?? Convert.DBNull);
                    parameters.Add(new DataParameter(pCS, "PWDREMAININGGRACE", DbType.Int32), RemainingGrace);

                    QueryParameters qry = new QueryParameters(pCS, sqlUpdate, parameters);
                    int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                    if (nbRow == 1)
                    {
                        PWD = hashData;
                        if (InsertIntoPWDEXPIRED_H())
                            CodeError = ErrorCode.SUCCESS;
                    }
                    else
                    {
                        CodeError = ErrorCode.NOTFOUND;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                CodeError = ErrorCode.BUG;
            }

            return CodeError;
        }

        /// <summary>
        /// Contrôle du nouveau Mot de passe saisi
        /// Matche-t-il avec les règles de sécurité
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pNewPassword">Nouveau mot de passe non crypté</param>
        /// <param name="pOldPassword">Ancien mot de passe non crypté</param>
        /// <returns></returns>
        /// EG 20151215 [21305] Refactoring, add PWDFORBIDDEN
        /// RD 20220214 [25943] Add parameter pOldPassword
        /// RD 20220221 [25943] Add parameter pCS
        public bool MatchRules(string pCS, string pNewPassword, string pOldPassword)
        {
            CodeError = ErrorCode.MISCELLANEOUS;

            try
            {
                // RD 20220214 [25943] Interdire la saisie du même mot de passe
                if (pNewPassword == pOldPassword)
                    CodeError = ErrorCode.ALREADYUSED;
                // RD 20220211 [25943] Use IsPwdLikeValue
                else if (PWDDIFFIDENTIFIER && CheckConnection.IsPwdLikeValue(pNewPassword,IDENTIFIER))
                    CodeError = ErrorCode.IDENTIFIER;
                else if (NBMONTHPWDFORBIDDEN > 0 && IsPwdAlreadyUsed(pNewPassword))
                    CodeError = ErrorCode.ALREADYUSED;
                else
                { 
                    bool testLengthFailed = false;
                    testLengthFailed |= NBCHARMIN > 0 && !(pNewPassword.Length >= NBCHARMIN);
                    testLengthFailed |= NBCHARMAX > 0 && !(pNewPassword.Length <= NBCHARMAX);

                    if (testLengthFailed)
                        CodeError = ErrorCode.LENGTH;
                    else
                    {
                        if (NBDIGITMIN > 0 || NBWILDCARDMIN > 0 || NBLOWERCHARMIN >0 || NBUPPERCHARMIN > 0)
                        {
                            int countDigits = 0;
                            int countWildCards = 0;
                            int countUpper = 0;
                            int countLower = 0;

                            for (int i = 0; i < pNewPassword.Length; i++)
                            {
                                if (char.IsDigit(pNewPassword, i))
                                    countDigits++;
                                else if (char.IsLower(pNewPassword[i]))
                                    countLower++;
                                else if (char.IsUpper(pNewPassword[i]))
                                    countUpper++;
                                else if (!char.IsLetterOrDigit(pNewPassword, i))
                                    countWildCards++;
                            }

                            if (NBDIGITMIN > 0 && NBDIGITMIN > countDigits)
                                CodeError = ErrorCode.DIGIT;
                            else if (NBLOWERCHARMIN > 0 && NBLOWERCHARMIN > countLower)
                                CodeError = ErrorCode.LOWER;
                            else if (NBUPPERCHARMIN > 0 && NBUPPERCHARMIN > countUpper)
                                CodeError = ErrorCode.UPPER;
                            else if (NBWILDCARDMIN > 0 && NBWILDCARDMIN > countWildCards)
                                CodeError = ErrorCode.WILDCARD;
                            else
                                CodeError = ErrorCode.SUCCESS;
                        }
                        else
                            CodeError = ErrorCode.SUCCESS;

                        // EG 20151215 [21305] Refactoring
                        // RD 20220221 [25943] Use CheckConnection.IsPwdLikeValue
                        if ((ErrorCode.SUCCESS == CodeError) && CheckConnection.IsPwdForbidden(pCS, pNewPassword, PWDFORBIDDENLIST, IDA))
                            CodeError = ErrorCode.PWDFORBIDDEN;
                    }
                }
            }
            catch
            {
                CodeError = ErrorCode.BUG;
            }

            return (CodeError == ErrorCode.SUCCESS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20151215 [21305] Refactoring
		/// RD 20160919 [22470] Modify
        // EG 20210209 [25660] Taille du paramètre PWD
        private bool InsertIntoPWDEXPIRED_H()
        {
            CodeError = ErrorCode.MISCELLANEOUS;
            try
            {
                string sqlInsert = @"insert into dbo.PWDEXPIRED_H (IDA, DTEXPIRATION, PWD) values (@IDA, @DTEXPIRATION, @PWD)";

                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), IDA);
                parameters.Add(new DataParameter(SessionTools.CS, "DTEXPIRATION", DbType.Date), OTCmlHelper.GetDateSys(SessionTools.CS).Date); // FI 20201006 [XXXXX] DbType.Date
                parameters.Add(new DataParameter(SessionTools.CS, "PWD", DbType.AnsiString, 128), previousPWD);

                QueryParameters qry = new QueryParameters(SessionTools.CS, sqlInsert, parameters);
                int nbRow = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                if (nbRow == 1)
                    CodeError = ErrorCode.SUCCESS;
                else
                    CodeError = ErrorCode.PWDEXPIRED_H;
            }
            catch
            {
                CodeError = ErrorCode.BUG;
            }
            return (CodeError == ErrorCode.SUCCESS);
        }

        #region GetHashed
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        // EG 20210209 [25660] Upd
        private string GetHashed(string pData)
        {
            return SystemSettings.HashData(pData);
        }
        #endregion
        #region IsPwdAlreadyUsed
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNewPassword"></param>
        /// <returns></returns>
        /// EG 20151215 [21305] Refactoring
        // EG 20180423 Analyse du code Correction [CA2200]
        // RD 20220315 [25943] Change dbType of parameter "DTEXPIRATION" from Boolean to DateTime.
        private bool IsPwdAlreadyUsed(string pNewPassword)
        {
            bool ret = false;

            if (NBMONTHPWDFORBIDDEN > 0)
            {
                IDataReader dr = null;
                try
                {
                    string sqlSelect = @"select PWD from dbo.PWDEXPIRED_H where (IDA = @IDA) and (DTEXPIRATION >= @DTEXPIRATION)";

                    DataParameters parameters = new DataParameters();
                    parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), IDA);
                    parameters.Add(new DataParameter(SessionTools.CS, "DTEXPIRATION", DbType.Date), OTCmlHelper.GetDateSys(SessionTools.CS).Date.AddMonths((-1) * NBMONTHPWDFORBIDDEN)); // FI 20201006 [XXXXX] DbType.Date
                    QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect.ToString(), parameters);
                    dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    while (dr.Read())
                    {
                        if (dr["PWD"].ToString() == GetHashed(pNewPassword))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
                catch (Exception) { throw; }
                finally
                {
                    if (null != dr)
                    {
                        // EG 20160404 Migration vs2013
                        //dr.Close();
                        dr.Dispose();
                    }
                }
            }

            return ret;
        }
        #endregion

        #endregion Methods
    }
}

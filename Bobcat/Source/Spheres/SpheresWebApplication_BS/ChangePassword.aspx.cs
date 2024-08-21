using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;


namespace EFS.Spheres
{
    public partial class ChangePassword : ContentPageBase
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
        private ActorModelSafetyPassword actorModelSafetyPassword;
        #endregion

        protected override void OnInit(EventArgs e)
        {
            isPwdAutoMode = ("1" == this.Request.QueryString["PWDAUTOMODE"]);
            isBeforeExp = ("1" == this.Request.QueryString["BEFOREEXP"]);
            isFromProfil = ("1" == this.Request.QueryString["FROMPROFIL"]);
            isForcedChangeAllowed = isPwdAutoMode || isBeforeExp;

            base.OnInit(e);
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, System.EventArgs e)
        {
            actorModelSafetyPassword = new ActorModelSafetyPassword(SessionTools.CS, SessionTools.Collaborator_IDA);
            alertPwd.Visible = false;

            lblPasswordTitle.Text = Ressource.GetString("PasswordTitle2");

            if (isPwdAutoMode)
            {
                if (false == IsPostBack)
                {
                    //Usage de JavaScript.AlertImmediate car JavaScript.DialogStartUpImmediate s'ouvre ds sommaire.aspx
                    JavaScript.BootstrapDialog(this, Ressource.GetString("Msg_PWNeededToBeChanged"));
                }
                if (actorModelSafetyPassword.PWDREMAININGGRACE > 0)
                {
                    btnCancel.ToolTip += " ( " + (actorModelSafetyPassword.PWDREMAININGGRACE).ToString() + " " + Ressource.GetString("RemainingGrace") + " )";
                }
                else
                    btnCancel.Enabled = false;
            }
        }

        /// <summary>
        /// Sauvegarde du nouveau mot de passe
        /// </summary>
        private void Save()
        {

            string displayMessage = string.Empty;
            if (txtNewPassword.Text == txtConfirmPassword.Text)
            {
                if (actorModelSafetyPassword.Authentify(txtOldPassword.Text))
                    actorModelSafetyPassword.SetNewPassword(SessionTools.CS, txtNewPassword.Text, isForcedChangeAllowed);

                switch (actorModelSafetyPassword.CodeError)
                {
                    // OK
                    case ActorModelSafetyPassword.ErrorCode.SUCCESS:
                        {
                            if (isFromProfil)
                                Response.Redirect("UserProfil.aspx?Msg=succes", true);
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
                    /// EG 20151215 [21305] New 
                    case ActorModelSafetyPassword.ErrorCode.PWDFORBIDDEN:
                        displayMessage = Ressource.GetString("Msg_PWForbidden", false); // Le nouveau mot de passe est interdit.
                        break;

                    //INTERNAL ERRORS
                    case ActorModelSafetyPassword.ErrorCode.MISCELLANEOUS:
                    case ActorModelSafetyPassword.ErrorCode.NOTFOUND:
                    case ActorModelSafetyPassword.ErrorCode.PWDEXPIRED_H:
                    case ActorModelSafetyPassword.ErrorCode.BUG:
                        displayMessage = Ressource.GetString("Msg_PWnotUpdated", false); // Le mot de passe n'a pas été mis à jour.
                        break;
                }
            }
            else
                displayMessage = Ressource.GetString("Msg_PWinvalidConfirm", false); // La confirmation est invalide.    

            alertPwd.Visible = (0 < displayMessage.Length);
            if (displayMessage.Length > 0)
                alertPwdMsg.InnerText = displayMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Record(object sender, System.EventArgs e)
        {
            Save();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Cancel(object sender, System.EventArgs e)
        {

            if (isFromProfil)
            {
                //Changement par choix de confirm(expiration) 
                Response.Redirect("UserProfil.aspx", true);
            }

            else
            {
                //Changement forcé
                if (isPwdAutoMode)
                {
                    actorModelSafetyPassword = new ActorModelSafetyPassword(SessionTools.CS, SessionTools.Collaborator_IDA);
                    actorModelSafetyPassword.DecrementGrace(SessionTools.CS);
                }
                Response.Redirect("welcome.aspx", true);
            }
        }

        /// EG 20151215 [21305] New 
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
                PWDEXPIRED_H,
                PWDFORBIDDEN,
            }
            public ErrorCode CodeError = ErrorCode.MISCELLANEOUS;

            private int IDA;
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
            private object DtPWDExpiration
            {
                get
                {
                    object dt = DBNull.Value;
                    if (PWDMODPERIODIC)
                    {
                        int nbDay = 0;
                        if (NBDAYVALID > 0)
                            nbDay = NBDAYVALID;
                        dt = OTCmlHelper.GetDateSys(SessionTools.CS).AddDays(nbDay);
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
                    string sqlSelect = @"select a.IDMODELSAFETY, a.PWD, a.IDENTIFIER, a.PWDREMAININGGRACE, ms.PWDMODPERMITTED, ms.PWDDIFFIDENTIFIER, ms.PWDCRYPTED, ms.PWDMODPERIODIC,
                ms.NBDAYVALID, ms.ALLOWEDGRACE, ms.NBDAYWARNING, ms.NBMONTHPWDFORBIDDEN, ms.NBCHARMIN, ms.NBCHARMAX, ms.NBDIGITMIN, ms.NBWILDCARDMIN, ms.PWDFORBIDDENLIST
                from dbo.ACTOR a
                inner join dbo.MODELSAFETY ms on (ms.IDMODELSAFETY = a.IDMODELSAFETY)
                where (a.IDA = @IDA)" + Cst.CrLf;

                    DataParameters parameters = new DataParameters();
                    parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
                    QueryParameters qry = new QueryParameters(pCS, sqlSelect, parameters);
                    dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
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
                        IDMODELSAFETY = Convert.ToInt32(dr["IDMODELSAFETY"]);
                        PWD = Convert.ToString(dr["PWD"]);
                        IDENTIFIER = Convert.ToString(dr["IDENTIFIER"]);
                        PWDREMAININGGRACE = Convert.ToInt32((dr["PWDREMAININGGRACE"].ToString().Length > 0 ? dr["PWDREMAININGGRACE"] : -1));
                        previousPWD = PWD;
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
                        dr.Dispose();
                    }
                }
            }

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
                catch { CodeError = ErrorCode.BUG; }
                return (CodeError == ErrorCode.SUCCESS);
            }

            /// <summary>
            /// Mise à jour de la grâce (Décrémentation)
            /// </summary>
            /// <param name="pCS"></param>
            /// <returns></returns>
            public void DecrementGrace(string pCS)
            {
                string sqlUpdate = @"update dbo.ACTOR set PWDREMAININGGRACE = PWDREMAININGGRACE - 1 where (IDA = @IDA)";

                DataParameters parameters = new DataParameters();
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);
                QueryParameters qry = new QueryParameters(pCS, sqlUpdate, parameters);
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
            }

            #region IsPwdForbidden
            private bool IsPwdForbidden(string pNewPassword)
            {
                return (null != PWDFORBIDDENLIST) && PWDFORBIDDENLIST.Exists(item => item == pNewPassword);
            }
            #endregion IsPwdForbidden

            /// <summary>
            /// Mise à jour du nouveau mot de passe
            /// </summary>
            /// <param name="pCS"></param>
            /// <param name="pNewPassword"></param>
            /// <param name="pIsForcedChangePermission"></param>
            /// <returns></returns>
            /// EG 20151215 [21305] Refactoring
            public ErrorCode SetNewPassword(string pCS, string pNewPassword, bool pIsForcedChangePermission)
            {
                CodeError = ErrorCode.MISCELLANEOUS;
                try
                {
                    if (false == (pIsForcedChangePermission || PWDMODPERMITTED))
                        CodeError = ErrorCode.NOTALLOWED;
                    else if (MatchRules(pNewPassword))
                    {
                        string sqlUpdate = @"update dbo.ACTOR set 
                    PWD = @PWD, ISPWDMODNEXTLOGON = @ISPWDMODNEXTLOGON, DTPWDEXPIRATION = @DTPWDEXPIRATION, PWDREMAININGGRACE = @PWDREMAININGGRACE 
                    where (IDA = @IDA)" + Cst.CrLf;

                        DataParameters parameters = new DataParameters();
                        parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), IDA);
                        parameters.Add(new DataParameter(pCS, "PWD", DbType.AnsiString, 32), GetHashed(pNewPassword));
                        parameters.Add(new DataParameter(pCS, "ISPWDMODNEXTLOGON", DbType.Boolean), false);
                        parameters.Add(new DataParameter(pCS, "DTPWDEXPIRATION", DbType.DateTime), DtPWDExpiration);
                        parameters.Add(new DataParameter(pCS, "PWDREMAININGGRACE", DbType.Int32), RemainingGrace);

                        QueryParameters qry = new QueryParameters(pCS, sqlUpdate, parameters);
                        int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                        if (nbRow == 1)
                        {
                            PWD = GetHashed(pNewPassword);
                            if (InsertIntoPWDEXPIRED_H())
                                CodeError = ErrorCode.SUCCESS;
                        }
                        else
                            CodeError = ErrorCode.NOTFOUND;
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
            /// <param name="pNewPassword">Nouveau mot de passe non crypté</param>
            /// <returns></returns>
            /// EG 20151215 [21305] Refactoring, add PWDFORBIDDEN
            public bool MatchRules(string pNewPassword)
            {
                CodeError = ErrorCode.MISCELLANEOUS;
                try
                {
                    if (PWDDIFFIDENTIFIER && pNewPassword.ToUpper() == IDENTIFIER.ToUpper())
                        CodeError = ErrorCode.IDENTIFIER;
                    else if (NBMONTHPWDFORBIDDEN > 0 && AlreadyUsed(pNewPassword))
                        CodeError = ErrorCode.ALREADYUSED;
                    else
                    {
                        bool testLengthFailed = false;
                        testLengthFailed |= (NBCHARMIN > 0 ? !(pNewPassword.Length >= NBCHARMIN) : false);
                        testLengthFailed |= (NBCHARMAX > 0 ? !(pNewPassword.Length <= NBCHARMAX) : false);
                        if (testLengthFailed)
                            CodeError = ErrorCode.LENGTH;
                        else
                        {
                            if (NBDIGITMIN > 0 || NBWILDCARDMIN > 0)
                            {
                                int countDigits = 0;
                                int countWildCards = 0;
                                for (int i = 0; i < pNewPassword.Length; i++)
                                {
                                    if (char.IsDigit(pNewPassword, i))
                                        countDigits++;
                                    else if (!char.IsLetterOrDigit(pNewPassword, i))
                                        countWildCards++;
                                }
                                if (NBDIGITMIN > 0 && NBDIGITMIN > countDigits)
                                    CodeError = ErrorCode.DIGIT;
                                else if (NBWILDCARDMIN > 0 && NBWILDCARDMIN > countWildCards)
                                    CodeError = ErrorCode.WILDCARD;
                                else
                                    CodeError = ErrorCode.SUCCESS;
                            }
                            else
                                CodeError = ErrorCode.SUCCESS;

                            if ((ErrorCode.SUCCESS == CodeError) && IsPwdForbidden(pNewPassword))
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
            private bool InsertIntoPWDEXPIRED_H()
            {
                CodeError = ErrorCode.MISCELLANEOUS;
                try
                {
                    string sqlInsert = @"insert into dbo.PWDEXPIRED_H (IDA, DTEXPIRATION, PWD, EXTLLINK) values (@IDA, @DTEXPIRATION, @PWD, @EXTLLINK)";
                    DataParameters parameters = new DataParameters();
                    parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), IDA);
                    parameters.Add(new DataParameter(SessionTools.CS, "PWD", DbType.AnsiString, 32), previousPWD);
                    parameters.Add(new DataParameter(SessionTools.CS, "DTEXPIRATION", DbType.DateTime), OTCmlHelper.GetDateSys(SessionTools.CS));
                    parameters.Add(new DataParameter(SessionTools.CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), string.Empty);

                    QueryParameters qry = new QueryParameters(SessionTools.CS, sqlInsert, parameters);
                    int nbRow = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                    if (nbRow == 1)
                        CodeError = ErrorCode.SUCCESS;
                    else
                        CodeError = ErrorCode.PWDEXPIRED_H;
                }
                catch (Exception)
                {
                    CodeError = ErrorCode.BUG;
                }
                return (CodeError == ErrorCode.SUCCESS);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pData"></param>
            /// <returns></returns>
            private string GetHashed(string pData)
            {
                string hashAlgorithm = string.Empty + SystemSettings.GetAppSettings_Software("_Hash");
                return StrFunc.HashData(pData, hashAlgorithm);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pNewPassword"></param>
            /// <returns></returns>
            /// EG 20151215 [21305] Refactoring
            private bool AlreadyUsed(string pNewPassword)
            {
                bool ret = false;
                if (NBMONTHPWDFORBIDDEN > 0)
                {
                    IDataReader dr = null;
                    try
                    {
                        string sqlSelect = @"select p.PWD from dbo.PWDEXPIRED_H p where (p.IDA = @IDA) and (p.DTEXPIRATION >= @DTEXPIRATION)" + Cst.CrLf;

                        DataParameters parameters = new DataParameters();
                        parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), IDA);
                        parameters.Add(new DataParameter(SessionTools.CS, "DTEXPIRATION", DbType.Boolean), OTCmlHelper.GetDateSys(SessionTools.CS).AddMonths((-1) * NBMONTHPWDFORBIDDEN));
                        QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect.ToString(), parameters);
                        dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
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
            #endregion Methods
        }
    }
}
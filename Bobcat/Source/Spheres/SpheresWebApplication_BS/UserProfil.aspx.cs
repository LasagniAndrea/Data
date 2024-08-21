#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using System;
using System.Data;
using System.Threading;
#endregion using directives

namespace EFS.Spheres
{
    public partial class UserProfil : ContentPageBase
    {
        private bool isConnected;

        protected void Page_Load(object sender, EventArgs e)
        {
            isConnected = SessionTools.IsConnected;

            lblCulture.Text = Ressource.GetString("lblCulture");
            lblPreferences.Text = Ressource.GetString("lblPreferences");
            lblReset.Text = Ressource.GetString("Reset");
            lblOthers.Text = Ressource.GetString("lblOthers");
            lblTracker.Text = Ressource.GetString("OnBoardTracker");
            lblPerGroup.InnerText = Ressource.GetString("lblPerGroup");


            if (isConnected)
            {
                divUserProfil.Attributes.Add("class", "body-content container-fluid startprint");
                if (!SessionTools.IsSessionSysAdmin)
                {
                    divReset.Visible = false;
                    divTracker.Attributes.Add("class", "col-sm-6 col-lg-6 panel-body");
                    divActions.Attributes.Add("class", "col-sm-6 col-lg-6 panel-body");
                }
                else
                {
                    divActions.Attributes.Add("class", "col-sm-6 col-lg-4 panel-body");
                    divTracker.Attributes.Add("class", "col-sm-6 col-lg-4 panel-body");
                }

                if (SessionTools.User.IsSessionGuest)
                {
                    //UserWithLimitedRights
                    //---------------------
                    //PREFERENCES
                    lblDefaultPage.Visible = false;
                    ddlDefaultPage.Visible = false;

                    lblTrackerRefresh.Visible = false;
                    txtTrackerRefreshInterval.Visible = false;
                    lblTrackerNbRow.Visible = false;
                    txtTrackerNbRowPerGroup.Visible = false;
                    lblTrackerAlert.Visible = false;
                    chkTrackerAlert.Visible = false;
                    lblTrackerVelocity.Visible = false;
                    chkTrackerVelocity.Visible = false;

                    //ACTIONS
                    chkTrackerLog.Visible = false;
                    chkProcessLog.Visible = false;
                    chkSystemLog.Visible = false;
                    chkRDBMSLog.Visible = false;
                    chkCache.Visible = false;

                    lblLoginLog.Visible = false;
                    btnLoginLog.Visible = false;

                    lblUserLock.Visible = false;
                    btnUserLock.Visible = false;
                    lblHostLock.Visible = false;
                    btnHostLock.Visible = false;

                }
            }
            else
            {
                divUserProfil.Attributes.Add("class", "body-content container-fluid startprint col-lg-offset-3 col-lg-6");
                divPreferences.Visible = false;
                divPreferences2.Visible = false;
                divTracker.Visible = false;
                divActions.Visible = false;
                divReset.Visible = false;
            }

            btnShowDataCache.Visible = isShowAdminTools;
            lblShowDataCache.Visible = isShowAdminTools;

            //Message de confirmation de changement du password OK
            //if ("succes" == Request.QueryString["Msg"])
            //    JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), Ressource.GetString("Msg_PWChangedWithSuccess", false), false, ProcessStateTools.StatusNoneEnum, JavaScript.DefautHeight, JavaScript.DefautWidth);

            if (!IsPostBack)
            {
                ControlsTools.DDLLoad_Culture(ddlCulture, isConnected && (!Software.IsSoftwarePortal()) && (!SessionTools.IsUserWithLimitedRights()));
                ControlsTools.DDLSelectByValue(ddlCulture, Thread.CurrentThread.CurrentUICulture.Name);

                if (isConnected)
                {
                    bool isChangePasswordEnabled = false;
                    string SQLSelect = @"select ms.PWDMODPERMITTED as PWDMODPERMITTED
                from dbo.ACTOR ac
                inner join dbo.MODELSAFETY ms on (ms.IDMODELSAFETY = ac.IDMODELSAFETY)
                where (ac.IDA = " + SessionTools.Collaborator_IDA.ToString() + ")";
                    object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, SQLSelect);
                    if (null != obj)
                        isChangePasswordEnabled = Convert.ToBoolean(obj);
                    //btnChangePassword.Enabled = isChangePasswordEnabled;
                    btnChangePassword.Disabled = (false == isChangePasswordEnabled);

                    chkUniquePage.Checked = SessionTools.IsOpenInUniquePage;
                    chkBackgroundWhite.Checked = SessionTools.IsBackgroundWhite;
                    txtNumberRowByPage.Text = SessionTools.NumberRowByPage.ToString();
                    chkPagerPosition.Checked = SessionTools.IsPagerPositionTopAndBottom;

                    ControlsTools.DDLLoad_DefaultPage(ddlDefaultPage);
                    ControlsTools.DDLSelectByValue(ddlDefaultPage, SessionTools.DefaultPage);
                    
                    ControlsTools.DDLLoadEnum<Cst.ETDMaturityInputFormatEnum>(ddlETDMaturityFormat, false);
                    ControlsTools.DDLSelectByValue(ddlETDMaturityFormat, SessionTools.ETDMaturityFormat.ToString());

                    //Trading
                    ControlsTools.DDLLoadEnum<Cst.TradingTimestampZone>(ddlTradingTimestampZone, false);
                    ControlsTools.DDLSelectByValue(ddlTradingTimestampZone, SessionTools.TradingTimestampZone.ToString());
                    ControlsTools.DDLLoadEnum<Cst.TradingTimestampPrecision>(ddlTradingTimestampPrecision, false);
                    ControlsTools.DDLSelectByValue(ddlTradingTimestampPrecision, SessionTools.TradingTimestampPrecision.ToString());

                    // Audit
                    ControlsTools.DDLLoadEnum<Cst.AuditTimestampZone>(ddlAuditTimestampZone, false);
                    ControlsTools.DDLSelectByValue(ddlAuditTimestampZone, SessionTools.AuditTimestampZone.ToString());
                    ControlsTools.DDLLoadEnum<Cst.AuditTimestampPrecision>(ddlAuditTimestampPrecision, false);
                    ControlsTools.DDLSelectByValue(ddlAuditTimestampPrecision, SessionTools.AuditTimestampPrecision.ToString());


                    txtTrackerRefreshInterval.Text = SessionTools.TrackerRefreshInterval.ToString();
                    txtTrackerNbRowPerGroup.Text = SessionTools.TrackerNbRowPerGroup.ToString();
                    chkTrackerAlert.Checked = SessionTools.IsTrackerAlert;
                    chkTrackerVelocity.Checked = SessionTools.IsTrackerVelocity;

                    string url = JavaScript.GetWindowOpen("ListViewer.aspx?Log=" + Cst.OTCml_TBL.LOGIN_L.ToString() + "&InputMode=2&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.AdminSafetyLoginLog) +
                        "&M=" + Cst.ConsultationMode.ReadOnly + "&FK=" + SessionTools.Collaborator_IDA.ToString(), Cst.WindowOpenStyle.OTCml_FormReferential);
                    btnLoginLog.Attributes.Add("onclick", url);

                    url = JavaScript.GetWindowOpen("ListViewer.aspx?Log=USERLOCKOUT_L&InputMode=2&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.UserLockoutLog) +
                        "&M=" + Cst.ConsultationMode.ReadOnly + "&FK=" + SessionTools.Collaborator_IDA.ToString(), Cst.WindowOpenStyle.OTCml_FormReferential);
                    btnUserLock.Attributes.Add("onclick", url);

                    url = JavaScript.GetWindowOpen("ListViewer.aspx?Log=HOSTLOCKOUT_L&InputMode=2&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.HostLockoutLog) +
                        "&M=" + Cst.ConsultationMode.ReadOnly + "&FK=" + SessionTools.ClientMachineName, Cst.WindowOpenStyle.OTCml_FormReferential);
                    btnHostLock.Attributes.Add("onclick", url);
                }
            }
            chkUniquePage.Text = string.Empty;
            chkBackgroundWhite.Text = string.Empty;
            chkPagerPosition.Text = string.Empty;
            chkTrackerAlert.Text = string.Empty;
            chkTrackerVelocity.Text = string.Empty;

            chkTrackerLog.Text = string.Empty;
            chkProcessLog.Text = string.Empty;
            chkSystemLog.Text = string.Empty;
            chkRDBMSLog.Text = string.Empty;
            chkDefault.Text = string.Empty;
            chkCache.Text = string.Empty;

#warning EG Temporaire ressource lbl à la place de chk
            lblTrackerLog.Text = Ressource.GetString(chkTrackerLog.ID);
            lblProcessLog.Text = Ressource.GetString(chkProcessLog.ID);
            lblSystemLog.Text = Ressource.GetString(chkSystemLog.ID);
            lblRDBMSLog.Text = Ressource.GetString(chkRDBMSLog.ID);
            lblDefault.Text = Ressource.GetString(chkDefault.ID);
            lblCache.Text = Ressource.GetString(chkCache.ID);

        }

        /// <summary>
        /// Validation des modifications du profil et fermeture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Valid(object sender, System.EventArgs e)
        {
            SaveProfile();
            Reload(false);
        }

        /// <summary>
        /// Annulation des modifications du profil et fermeture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Cancel(object sender, System.EventArgs e)
        {
            Reload(false);
        }

        /// <summary>
        /// Validation des modifications du profil sans fermeture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Apply(object sender, System.EventArgs e)
        {
            SaveProfile();
            Reload(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ChangePassword(object sender, System.EventArgs e)
        {
            Response.Redirect("ChangePwd.aspx?FROMPROFIL=1");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Reset(object sender, System.EventArgs e)
        {
            if (chkTrackerLog.Checked)
                ResetLog(Cst.OTCml_TBL.TRACKER_L);
            if (chkSystemLog.Checked)
                ResetLog(Cst.OTCml_TBL.PROCESS_L);
            if (chkSystemLog.Checked)
                ResetLog(Cst.OTCml_TBL.SYSTEM_L);
            if (chkRDBMSLog.Checked)
                ResetLog(Cst.OTCml_TBL.RDBMS_L);
            if (chkCache.Checked)
                ResetCache();
            if (chkDefault.Checked)
                ResetDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResetDefault()
        {
            /*
            #warning 20050919 PL NORELEASE Code à remplacer par une fonctionnalité à part entière
            //-------------------------------------------------------------------------
            LockTools.UnLockSession(SessionTools.CS, SessionTools.SessionID);
            //-------------------------------------------------------------------------
            */

            int opNbRowsDeleted;
            AspTools.PurgeSQLCookie(out opNbRowsDeleted);
            string message = string.Empty;
            if (opNbRowsDeleted > 0)
            {
                message = Ressource.GetString("Msg_ProcessSuccessfull");
                message += Cst.CrLf;
                message += Ressource.GetString2("Msg_DeletedRows", opNbRowsDeleted.ToString());
            }
            else if (opNbRowsDeleted == 0)
                message = Ressource.GetString("Msg_NothingToDelete");
            else
                message = Ressource.GetString("Msg_ProcessUndone");

            //JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), message, false, ProcessStateTools.StatusNoneEnum,
            //    JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        /// <summary>
        /// Purge du cache
        /// </summary>
        private void ResetCache()
        {
            string msg = string.Empty;
            int ret = 0;
            try
            {
                ret = ClearDataCache();
                msg = GetMessageRemove(ret, string.Empty);
            }
            catch (TimeoutException ex)
            {
                msg = ex.Message;
                msg = msg + Cst.CrLf2 + GetMessageRemove(ret, string.Empty);
            }
            //if (StrFunc.IsFilled(msg))
            //    JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), msg, false, ProcessStateTools.StatusNoneEnum,
            //        JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        /// <summary>
        /// Affiche les requêtes présentes ds le cache SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ShowDataCache(object sender, EventArgs e)
        {
            string WindowID = "Diag" + "_" + "Cache" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfffff");
            string write_FpMLFile = SessionTools.TemporaryDirectory.MapPath("Diagnostic_xml") + @"\" + WindowID + ".xml";
            string open_FpMLFile = SessionTools.TemporaryDirectory.Path + "Diagnostic_xml" + @"/" + WindowID + ".xml";
            DataHelper.queryCache.diagnostic.WriteElements(write_FpMLFile);
            AddScriptWinDowOpenFile(WindowID, open_FpMLFile, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveProfile()
        {

            string prevCulture = Thread.CurrentThread.CurrentUICulture.Name;
            string culture = ddlCulture.SelectedItem.Value.ToString();
            SessionTools.SetCulture(culture);

            if (isConnected)
            {
                // Purge de SessionTools.Menus si changement de culture (permet un reload des menus dans la nouvelle culture)
                if (culture != prevCulture)
                    SessionTools.Menus = null;

                #region Update ACTOR table
                CSManager csManager = new CSManager(SessionTools.CS);
                if (false == csManager.isUserReadOnly())
                {
                    DataParameters dataParameters = new DataParameters();
                    dataParameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), SessionTools.Collaborator_IDA);
                    dataParameters.Add(new DataParameter(SessionTools.CS, "CULTURE", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), culture);
                    //
                    string SQLUpdate = @"update dbo.ACTOR set CULTURE = @CULTURE where IDA=@IDA";
                    DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, SQLUpdate, dataParameters.GetArrayDbParameter());
                }
                #endregion

                SessionTools.IsOpenInUniquePage = chkUniquePage.Checked;
                SessionTools.IsBackgroundWhite = chkBackgroundWhite.Checked;
                SessionTools.NumberRowByPage = Convert.ToInt32(txtNumberRowByPage.Text);
                SessionTools.IsPagerPositionTopAndBottom = chkPagerPosition.Checked; ;
                SessionTools.DefaultPage = ddlDefaultPage.SelectedValue;
                SessionTools.ETDMaturityFormat = (Cst.ETDMaturityInputFormatEnum)Enum.Parse(typeof(Cst.ETDMaturityInputFormatEnum), ddlETDMaturityFormat.SelectedValue, true);
                SessionTools.TradingTimestampZone = (Cst.TradingTimestampZone)Enum.Parse(typeof(Cst.TradingTimestampZone), ddlTradingTimestampZone.SelectedValue, true);
                SessionTools.TradingTimestampPrecision = (Cst.TradingTimestampPrecision)Enum.Parse(typeof(Cst.TradingTimestampPrecision), ddlTradingTimestampZone.SelectedValue, true);
                SessionTools.AuditTimestampZone = (Cst.AuditTimestampZone)Enum.Parse(typeof(Cst.AuditTimestampZone), ddlTradingTimestampZone.SelectedValue, true);
                SessionTools.AuditTimestampPrecision = (Cst.AuditTimestampPrecision)Enum.Parse(typeof(Cst.AuditTimestampPrecision), ddlTradingTimestampZone.SelectedValue, true);
                SessionTools.TrackerRefreshInterval = Convert.ToInt32(txtTrackerRefreshInterval.Text);
                SessionTools.TrackerNbRowPerGroup = Convert.ToInt32(txtTrackerNbRowPerGroup.Text);
                SessionTools.IsTrackerAlert = chkTrackerAlert.Checked;
                SessionTools.IsTrackerVelocity = chkTrackerVelocity.Checked;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Reload(bool pIsApply)
        {
            JavaScript.ExecuteImmediate((PageBase)this, String.Format("window.location.href='{0}.aspx'", pIsApply ? "UserProfil" : (isConnected ? "Welcome" : "Default")), false);
        }

        private void ResetLog(Cst.OTCml_TBL pTbl)
        {
            int opNbRowsDeleted;
            AspTools.ResetLog(pTbl, out opNbRowsDeleted);
            string message2 = Cst.CrLf + "Table: " + pTbl.ToString() + Cst.CrLf;
            string message = GetMessageRemove(opNbRowsDeleted, message2);
            //JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), message, false, ProcessStateTools.StatusNoneEnum,
            //    JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static int ClearDataCache()
        {
            return DataHelper.queryCache.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNbRowsDeleted"></param>
        /// <param name="pMessage2"></param>
        /// <returns></returns>
        private static string GetMessageRemove(int pNbRowsDeleted, string pMessage2)
        {
            string ret = string.Empty;
            if (pNbRowsDeleted > 0)
            {
                ret = "Process completed successfully !" + Cst.CrLf;
                ret += pMessage2;
                ret += pNbRowsDeleted.ToString() + " row(s) removed.";
            }
            else if (pNbRowsDeleted == 0)
            {
                ret = "No data to remove !";
                ret += pMessage2;
            }
            else
            {
                ret = "Process not made !";
                ret += pMessage2;
            }
            return ret;
        }
    }
}
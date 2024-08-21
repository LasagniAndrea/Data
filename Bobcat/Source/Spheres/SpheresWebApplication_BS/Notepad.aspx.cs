using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;

using EFS.Restriction;
using EFS.Referentiel;

namespace EFS.Spheres
{
    public partial class Notepad : ContentPageBase
    {
        #region Members
        private Uri urlReferer;
        private bool isTRIM;
        private string tableName;
        private string title;
        private string title1;
        private string title2;
        private string notepadTableName;
        private string Ids;

        private int Id;
        private bool isNewRecord = false;
        private bool isIdString;
        private Cst.ConsultationMode consultationMode;
        private DataTable dt;
        private DataRow dr;
        #endregion Members

        #region OnInit
        protected override void OnInit(EventArgs e)
        {

            InitializeComponent();
            base.OnInit(e);

            AbortRessource = true;
            urlReferer = Request.UrlReferrer;
            tableName = Request.QueryString["TN"];
            title = Request.QueryString["T"];
            title1 = Request.QueryString["T1"];
            title2 = Request.QueryString["T2"];
            isTRIM = (title == Cst.ListType.TRIM.ToString());

            //Mode 0:Normal (XML), 1:Select, 2:ReadOnly 
            try
            {
                consultationMode = (Cst.ConsultationMode)Convert.ToInt32(Request.QueryString["M"]);
            }
            catch
            {
                consultationMode = Cst.ConsultationMode.Normal;
            }

            isIdString = (Convert.ToInt32(Request.QueryString["S"]) == 1);
            if (isIdString)
            {
                Ids = Request.QueryString["ID"];
                notepadTableName = Cst.OTCml_TBL.NOTEPADS.ToString();
            }
            else
            {
                Id = Convert.ToInt32(Request.QueryString["ID"]);
                notepadTableName = Cst.OTCml_TBL.NOTEPAD.ToString();
            }

            SetRecordNotepad();
            SetData();
            DisplayButton();
            DisplayTitle();
        }
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion

        #region DisplayButton
        private void DisplayButton()
        {
            btnRecord.Visible = (consultationMode == Cst.ConsultationMode.Normal);
            btnRecord.Disabled = SessionTools.IsUserWithLimitedRights();
            //btnRecord.Enabled = (false == SessionTools.IsUserWithLimitedRights());
        }
        #endregion DisplayButton
        #region DisplayTitle
        private void DisplayTitle()
        {
            // NotePad ou Thread
            string action = String.Format("<small> : {0}</small>",
                Ressource.GetString_SelectionCreationModification(consultationMode, isNewRecord));
            lblTitleNotePad.InnerHtml = (isTRIM ? "Thread" : Ressource.GetString("btnNotePad")) + action;

            // Source appelante
            lblTitle.Text = Ressource.GetMenu_Fullname(title1);
            if (StrFunc.IsFilled(title2))
                lblTitle.Attributes.Add("prt-data", " : " + title2);

            // Action
            lblLastAction.Visible = (false == isNewRecord);
            if (lblLastAction.Visible)
                lblLastAction.Text = Ressource.GetString_LastModifyBy(Convert.ToDateTime(dr["DTUPD"]), dr["DISPLAYNAME"].ToString());
        }
        #endregion DisplayTitle
        #region SetData
        private void SetData()
        {
            txtLoNote.Visible = (false == isTRIM);
            if (isTRIM)
            {
                string data = GetThreadTRIM(dr["LONOTE"].ToString());
                plhNotepad.Controls.Add(new LiteralControl(data));
            }
            else
            {
                txtLoNote.EnableViewState = true;
                //txtLoNote.ReadOnly = (consultationMode != Cst.ConsultationMode.Normal);
                txtLoNote.Attributes.Add("contenteditable", (consultationMode != Cst.ConsultationMode.Normal?"false":"true"));
                if (false == IsPostBack)
                {
                    txtLoNote.InnerHtml = JavaScript.HTMLStringCrLf(dr["LONOTE"].ToString());
                    //txtLoNote.InnerText = dr["LONOTE"].ToString();
                }
            }

        }
        #endregion SetData

        #region Page_Load
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Placer ici le code utilisateur pour initialiser la page
        }
        #endregion


        #region OnRecordClick
        protected void OnRecord(object sender, EventArgs e)
        {
            if (Page.IsValid)
                SaveAndReturnToReferer();
        }
        #endregion OnRecordClick
        #region OnRecordClick
        protected void OnCancel(object sender, EventArgs e)
        {
            // Retour à la page appelante
            if (null != urlReferer)
                Response.Redirect(urlReferer.AbsoluteUri, true);
        }
        #endregion OnRecordClick


        #region SetRecordNotepad
        private void SetRecordNotepad()
        {
            string sqlSelect = GetSelect(false);
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect);
            dt = ds.Tables[0];
            isNewRecord = (dt.Rows.Count == 0);
            if (isNewRecord)
                dr = dt.NewRow();
            else
                dr = dt.Rows[0];
        }
        #endregion SetRecordNotepad

        #region SaveAndReturnToReferer
        private void SaveAndReturnToReferer()
        {
            dr.BeginEdit();
            dr["TABLENAME"] = tableName;
            if (isIdString)
                dr["ID"] = Ids;
            else
                dr["ID"] = Id;

            dr["LONOTE"] = StrFunc.IsFilled(hidTxtLoNote.Value) ? hidTxtLoNote.Value : Convert.DBNull;
            dr["DTUPD"] = DateTime.Now;
            dr["IDAUPD"] = SessionTools.Collaborator_IDA;
            dr.EndEdit();

            if (isNewRecord)
                dt.Rows.Add(dr);

            DataTable dtChanges = dt.GetChanges();
            if (dtChanges == null)
            {
                JavaScript.DialogImmediate(this, Ressource.GetString("Msg_NoModification"), false);
            }
            else
            {
                try
                {
                    string SQLSelect = GetSelect(true);
                    int rowsAffected = DataHelper.ExecuteDataAdapter(SessionTools.CS, SQLSelect, dt);
                }
                catch
                {
                    throw;
                }
            }

            // Retour à la page appelante
            if (null != urlReferer)
                Response.Redirect(urlReferer.AbsoluteUri, true);
        }
        #endregion SaveAndReturnToReferer
        #region GetSelect
        private string GetSelect(bool pWithOnlyTblMain)
        {
            string sqlSelect = string.Empty;

            if (isTRIM)
            {
                // Specific to TRIM
                sqlSelect = @"select null as TABLENAME, null as ID, i2.THREAD as LONOTE, null as ROWVERSION, i1.DTAPPEL as DTUPD, null as IDAUPD, 'Unknown' as DISPLAYNAME
                from dbo.INCMESSAGE i1
                inner join dbo.INCMESSAGETEXT i2 on (i2.IDINC = i1.IDINC)
                where (i1.IDINC = " + Id.ToString() + ")";
            }
            else
            {
                sqlSelect = @"select n.TABLENAME, n.ID, n.LONOTE, n.ROWVERSION, n.DTUPD, n.IDAUPD {0}
                from dbo.{1} n
                {2}
                where (n.TABLENAME = {3}) and (n.ID = {4})";

                sqlSelect = String.Format(sqlSelect,
                    (pWithOnlyTblMain?string.Empty : ", a.DISPLAYNAME"),
                    notepadTableName,
                    (pWithOnlyTblMain?string.Empty : OTCmlHelper.GetSQLJoin(SessionTools.CS, Cst.OTCml_TBL.ACTOR, false, "n.IDAUPD")),
                    DataHelper.SQLString(tableName),
                    (isIdString?DataHelper.SQLString(Ids) : Id.ToString()));
            }
            return sqlSelect;
        }
        #endregion GetSelect

        #region GetThreadTRIM
        public static string GetThreadTRIM(string pData)
        {
            int start, end;
            string search, tmpData, info, retInfo, retDate, retUser, retId;
            string foreColor = "Black";
            string ret = pData;

            if (StrFunc.IsFilled(pData))
            {
                int guard = 0;
                bool isFound = true;
                ret = string.Empty;
                while (isFound && (guard < 999))
                {
                    guard++;
                    isFound = false;
                    //
                    search = @"<Info ";
                    start = pData.IndexOf(search);
                    end = pData.IndexOf(@">", start + search.Length + 1);
                    if ((start >= 0) && (end >= 0))
                    {
                        retInfo = string.Empty;
                        #region "Info " trouvé
                        info = pData.Substring(start, end - start + 1);

                        tmpData = pData.Substring(0, start);
                        tmpData = tmpData.Replace(@"<", @"&lt;");
                        tmpData = tmpData.Replace(@">", @"&gt;");
                        tmpData = tmpData.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                        tmpData = tmpData.Replace(Cst.Tab, Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace);
                        ret += tmpData;

                        pData = pData.Substring(end + 1);

                        search = @"date=""";
                        start = info.IndexOf(search);
                        end = info.IndexOf(@"""", start + search.Length + 1);
                        if ((start >= 0) && (end >= 0))
                        {
                            isFound = true;
                            #region "date="" trouvé
                            tmpData = info.Substring(start, end - start + 1);
                            tmpData = tmpData.Replace(@" ", @"</strong> at <strong>");
                            tmpData = tmpData.Replace(@"date=""", @"The <strong>");
                            tmpData += @"</strong> ";
                            retDate = tmpData;
                            retDate = retDate.Replace(@"""", string.Empty);

                            search = @"id=""";
                            start = info.IndexOf(search);
                            end = info.IndexOf(@"""", start + search.Length + 1);
                            if ((start >= 0) && (end >= 0))
                            {
                                #region "id="" trouvé
                                tmpData = info.Substring(start, end - start + 1);
                                foreColor = "Black";
                                retId = tmpData;
                                retId = retId.Replace(@"id=", string.Empty);
                                retId = retId.Replace(@"""", string.Empty);
                                retId = @"<span style=""Color:black"">" + "[Item: " + @"<strong>" + retId + @"</strong>" + @"] </span>";
                                #endregion
                            }
                            else
                            {
                                retId = string.Empty;
                            }

                            search = @"user=""";
                            start = info.IndexOf(search);
                            end = info.IndexOf(@"""", start + search.Length + 1);
                            if ((start >= 0) && (end >= 0))
                            {
                                #region "user="" trouvé
                                tmpData = info.Substring(start, end - start + 1);
                                switch (tmpData)
                                {
                                    case @"user=""CLIENT""":
                                        foreColor = "MediumOrchid";
                                        retUser = "the client";
                                        break;
                                    case @"user=""SUPPORT""":
                                        foreColor = "SeaGreen";
                                        retUser = "the support";
                                        break;
                                    default:
                                        foreColor = "#003082";
                                        retUser = tmpData;
                                        retUser = retUser.Replace(@"user=", string.Empty);
                                        retUser = retUser.Replace(@"""", string.Empty);
                                        break;
                                }
                                retUser = @"<strong>" + retUser;
                                retUser += @"</strong>";

                                retInfo = retId + retDate + retUser + @" wrote: ";
                                #endregion
                            }
                            else
                            {
                                retInfo = retId + retDate + @" : ";
                            }
                            #endregion
                        }
                        #region Flux html
                        if (isFound)
                        {
                            tmpData = @"<table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;background-color:Lavender;color:" + foreColor + @";font-family:Arial;font-size:10pt;margin-top:0px"">" + Cst.CrLf;
                            tmpData += @"<tr><td>" + Cst.CrLf;
                            tmpData += retInfo + Cst.CrLf;
                            tmpData += @"</td></tr>" + Cst.CrLf;
                            tmpData += @"</table>" + Cst.CrLf;
                            retInfo = tmpData;
                        }
                        else
                        {
                            retInfo = retInfo.Replace(@"<", @"&lt;");
                            retInfo = retInfo.Replace(@">", @"&gt;");
                        }
                        #endregion

                        ret += retInfo;
                        #endregion
                    }
                }
                tmpData = pData;
                tmpData = tmpData.Replace(@"<", @"&lt;");
                tmpData = tmpData.Replace(@">", @"&gt;");
                tmpData = tmpData.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                tmpData = tmpData.Replace(Cst.Tab, Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace);
                ret += tmpData;
            }
            return ret;
        }
        #endregion GetThreadTRIM

    }
}
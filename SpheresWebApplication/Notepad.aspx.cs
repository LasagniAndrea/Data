using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;

namespace EFS.Spheres
{
    public partial class NotepadPage : PageBase
	{
		#region Declaration
		private bool isTRIM;
		private string tableName, title, title1, title2, notepadTableName, Ids, mainMenuClassName;
		private int Id;
		private bool isNewRecord = false, isIdString;
		private Cst.ConsultationMode consultationMode;
		private DataTable dt;
		private DataRow dr;
		#endregion 

		#region protected override OnInit
        protected override void OnInit(EventArgs e)
        {

            InitializeComponent();
            base.OnInit(e);
            //
            #region Members initialization
            AbortRessource = true;

            tableName = Request.QueryString["TN"];
            title = Request.QueryString["T"];
            title1 = Request.QueryString["T1"];
            title2 = Request.QueryString["T2"];
			mainMenuClassName = ControlsTools.MainMenuName(title1);
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
            #endregion

            #region Checking
            bool bOk = true;
            //Check permission user for this page (GlopPL à faire)
            if (!bOk)
                Response.Redirect("Sommaire.aspx");
            //Check UrlReferrer  (GlopPL à faire)
            _ = Convert.ToString(Request.UrlReferrer);
            //bOk = (urlReferrer.ToString()=="ListReferentiel.aspx");
            if (!bOk)
                Response.Redirect("Sommaire.aspx");
            #endregion

            PageConstruction();

        }
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
		#region protected override GenerateHtmlForm
		protected override void  GenerateHtmlForm()
		{
			// PM 20240604 [XXXXX] Ajout gestion GUID pour les restrictions
			string parentGUID = Request.QueryString["GUID"];
			if (StrFunc.IsFilled(parentGUID))
			{
				ReferentialsReferential referential = DataCache.GetData<ReferentialsReferential>(parentGUID + "_Referential");
				if ((referential != default(ReferentialsReferential)) && (referential.isLookReadOnly || (referential.consultationMode == Cst.ConsultationMode.ReadOnly)))
				{
					consultationMode = Cst.ConsultationMode.ReadOnly;
				}
			}

			base.GenerateHtmlForm();
			AddToolbar();
			CreateAndLoadControlsFromDataRow();
		}
		#endregion
		#region protected override PageConstruction
		/// <summary>
		/// 
		/// </summary>
		// EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
		protected override void PageConstruction()
		{

			string _titleLeft = (isTRIM ? "Thread" : Ressource.GetString("btnNotePad")) + " - " + Ressource.GetMenu_Fullname(title1);
			if (!String.IsNullOrEmpty(title2))
				_titleLeft += ": " + HtmlTools.HTMLBold(title2);
			string _titleRight = Ressource.GetString_SelectionCreationModification(consultationMode, isNewRecord);
			string _subtitleRight = string.Empty;

			if (!isNewRecord)
			{
				if (isTRIM)
					_subtitleRight = RessourceExtended.GetString_LastModifyBy(new DateTimeTz(Convert.ToDateTime(dr["DTUPD"]),string.Empty), dr["DISPLAYNAME"].ToString(), false);
				else
				{
					// FI 20200820 [XXXXXX] dates systemes en UTC
					_subtitleRight = RessourceExtended.GetString_LastModifyBy(new DateTimeTz(Convert.ToDateTime(dr["DTUPD"]), "Etc/UTC"), dr["DISPLAYNAME"].ToString(), true);
				}
				_subtitleRight += Cst.HTMLSpace;
			}

			HtmlPageTitle titleLeft = new HtmlPageTitle(_titleLeft);
			HtmlPageTitle titleRight = new HtmlPageTitle(_titleRight, _subtitleRight);
			string title = HtmlTools.HTMLBold_Remove(_titleLeft);
			GenerateHtmlForm();
			this.PageTitle = title;

			FormTools.AddBanniere(this, Form, titleLeft, titleRight, null, title1);
			PageTools.BuildPage(this, Form, PageFullTitle, null, false, null);

		}
		#endregion

		#region private Page_Load
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Placer ici le code utilisateur pour initialiser la page
		}
		#endregion
		#region private AddToolbar
		private void AddToolbar()
		{
			Panel pnlToolbar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + mainMenuClassName };
			if (consultationMode == Cst.ConsultationMode.Normal || consultationMode == Cst.ConsultationMode.PartialReadOnly)
			{
				WCToolTipLinkButton butRecord = ControlsTools.GetAwesomeButtonAction("btnRecord", "fa fa-check", true);
				butRecord.Enabled = (false == SessionTools.IsUserWithLimitedRights());
				butRecord.Click += new EventHandler(this.OnRecordClick);
				pnlToolbar.Controls.Add(butRecord);
				pnlToolbar.Controls.Add(ControlsTools.GetAwesomeButtonCancel(true));
			}
			WCToolTipLinkButton btnSearch = ControlsTools.GetAwesomeLinkButton("btnSearch",string.Empty,"fa fa-search", Ressource.GetString("btnSearch"), "Search('txtSearch');return false;");
			btnSearch.CausesValidation = false;
			pnlToolbar.Controls.Add(btnSearch);
			pnlToolbar.Controls.Add(ControlsTools.GetTextBoxSearch());
			CellForm.Controls.Add(pnlToolbar);
		}
		#endregion AddToolbar
		#region private OnRecordClick
		private void OnRecordClick( object sender, EventArgs e )
		{
			if (Page.IsValid) 
			{
				UpdateDataRowFromControls();
				SaveAndClose();
			}
		}
		#endregion
        #region private CreateAndLoadControlsFromDataRow
        private void CreateAndLoadControlsFromDataRow()
        {
            bool isSetData = (!IsPostBack);

			Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + mainMenuClassName };
			Panel pnlData = new Panel();
			if (isTRIM)
			{
				#region Specific to TRIM
				string data = dr["LONOTE"].ToString();
				data = GetThreadTRIM(data);
				pnlBody.Controls.Add(new LiteralControl(data));
				#endregion
			}
			else
			{
				TextBox txtLoNote = new TextBox()
				{
					ID = "txtlonote",
					CssClass = "txtlonote",
					EnableViewState = true,
					Rows = 40,
					ReadOnly = true,
					TextMode = TextBoxMode.MultiLine,
				};

				if (consultationMode == Cst.ConsultationMode.Normal || consultationMode == Cst.ConsultationMode.PartialReadOnly)
					txtLoNote.ReadOnly = false;
				if (isSetData)
					txtLoNote.Text = dr["LONOTE"].ToString();
				pnlData.Controls.Add(txtLoNote);
			}
			pnlBody.Controls.Add(pnlData);
			CellForm.Controls.Add(pnlBody);
        }

		#endregion
		
        /// <summary>
        /// 
        /// </summary>
        private void SetRecordNotepad()
        {
            string SQLSelect = GetSelect(false);
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, SQLSelect);
            dt = ds.Tables[0];
            isNewRecord = (dt.Rows.Count == 0);
            if (isNewRecord)
                dr = dt.NewRow();
            else
                dr = dt.Rows[0];
        }

		#region private UpdateDataRowFromControls
		// EG 20210921 [XXXXX] Si le champ texte du Notepad est vide alors on supprime la ligne dans le DataRow.
		private void UpdateDataRowFromControls()
		{
			TextBox txtLoNote = (TextBox)this.FindControl("txtlonote");
			string data = txtLoNote.Text;

			dr.BeginEdit();
			dr["TABLENAME"] = tableName;
			if (isIdString)
				dr["ID"] = Ids;
			else
				dr["ID"] = Id;
			if (data.Length == 0)
				//Nécessaire pour Oracle
				dr["LONOTE"] = Convert.DBNull;
			else
				dr["LONOTE"] = data;
			if (isTRIM)
				dr["DTUPD"] = DateTime.Now;
			else
			{
				// FI 20200820 [XXXXXX] dates systèmes en UTC
				dr["DTUPD"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
			}
			dr["IDAUPD"] = SessionTools.Collaborator_IDA;

			if (String.IsNullOrEmpty(data))
				dr.Delete();
			dr.EndEdit();
		}
		#endregion
		#region private SaveAndClose
		private void SaveAndClose()
		{
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
			JavaScript.SubmitOpenerAndSelfClose(this, null);
		}
		#endregion
		#region private GetSelect
		private string GetSelect(bool pWithOnlyTblMain)
		{
			string SQLSelect = SQLCst.SELECT;

			if (isTRIM)
			{
				#region Specific to TRIM
				SQLSelect += "null as TABLENAME, null as ID, i2.THREAD as LONOTE, null as ROWVERSION, i1.DTAPPEL as DTUPD, null as IDAUPD, ";
				SQLSelect += "'Unknown' as DISPLAYNAME";
				SQLSelect += Cst.CrLf + SQLCst.FROM_DBO + " INCMESSAGE i1 ";
				SQLSelect += Cst.CrLf + SQLCst.INNERJOIN_DBO + " INCMESSAGETEXT i2 on i2.IDINC=i1.IDINC";
				SQLSelect += Cst.CrLf + SQLCst.WHERE + "i1.IDINC=" + Id.ToString();
				#endregion 
			}
			else
			{
				SQLSelect += "n.TABLENAME, n.ID, n.LONOTE, n.ROWVERSION, n.DTUPD, n.IDAUPD";
				if (!pWithOnlyTblMain)
					SQLSelect += ", a.DISPLAYNAME";
				SQLSelect += Cst.CrLf + SQLCst.FROM_DBO + notepadTableName + " n ";
				if (!pWithOnlyTblMain)
					SQLSelect += Cst.CrLf + OTCmlHelper.GetSQLJoin(SessionTools.CS, Cst.OTCml_TBL.ACTOR, false, "n.IDAUPD"); 
				SQLSelect += Cst.CrLf + SQLCst.WHERE + "n.TABLENAME=" + DataHelper.SQLString(tableName);
				SQLSelect += SQLCst.AND + "n.ID=";
				if (isIdString)
					SQLSelect += DataHelper.SQLString(Ids);
				else
					SQLSelect += Id.ToString();
			}
			//			
			return SQLSelect;
		}
		#endregion
		
		#region public static GetThreadTRIM
		public static string GetThreadTRIM(string pData)
		{
			int start, end;
            string search, tmpData, info, retInfo, retDate, retUser, retId;
			string foreColor = "Black";
			string ret = pData;
			//
			if (StrFunc.IsFilled(pData))
			{
				int guard = 0;
				bool isFound = true;
				ret = string.Empty;
				while (isFound && (guard<999))
				{
					guard++;
					isFound = false;
					//
					search = @"<Info ";
					start = pData.IndexOf(search);
					end   = pData.IndexOf(@">", start + search.Length + 1);
					if ((start>=0) && (end>=0))
					{
						retInfo = string.Empty;
						#region "Info " trouvé
						info    = pData.Substring(start, end - start + 1);
						//
						tmpData = pData.Substring(0, start);
						tmpData = tmpData.Replace(@"<", @"&lt;");
						tmpData = tmpData.Replace(@">", @"&gt;");
						tmpData = tmpData.Replace(Cst.CrLf, Cst.HTMLBreakLine);
						tmpData = tmpData.Replace(Cst.Tab,  Cst.HTMLSpace+Cst.HTMLSpace+Cst.HTMLSpace+Cst.HTMLSpace);
						ret    += tmpData;
						//
						pData   = pData.Substring(end+1);
						//
						search = @"date=""";
						start = info.IndexOf(search);
						end   = info.IndexOf(@"""", start + search.Length + 1);
						if ((start>=0) && (end>=0))
						{
							isFound = true;
							#region "date="" trouvé
							tmpData = info.Substring(start, end - start + 1);
                            tmpData = tmpData.Replace(@" ", @"</strong> at <strong>");
                            tmpData = tmpData.Replace(@"date=""", @"The <strong>");
                            tmpData += @"</strong> ";
							retDate = tmpData;
							retDate = retDate.Replace(@"""", string.Empty);
							//
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
                            //
							search = @"user=""";
							start = info.IndexOf(search);
							end   = info.IndexOf(@"""", start + search.Length + 1);
							if ((start>=0) && (end>=0))
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
								//
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
						//						
						ret += retInfo;
						#endregion 
					}
				}
				tmpData = pData;
				tmpData = tmpData.Replace(@"<", @"&lt;");
				tmpData = tmpData.Replace(@">", @"&gt;");
				tmpData = tmpData.Replace(Cst.CrLf, Cst.HTMLBreakLine);
				tmpData = tmpData.Replace(Cst.Tab,  Cst.HTMLSpace+Cst.HTMLSpace+Cst.HTMLSpace+Cst.HTMLSpace);
				ret    += tmpData;
			}
			//
			return ret;
		}
		#endregion

	}
}

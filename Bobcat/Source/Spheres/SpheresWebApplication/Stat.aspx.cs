using System;
using System.Reflection;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web; 
using EFS.ApplicationBlocks.Data;



namespace EFS.Spheres
{
	public partial class StatPage : PageBase
	{
        #region Members
        private string _mainMenuClassName;
        private string _idMenu;

        #endregion

        #region protected override OnInit
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            _idMenu = Request.QueryString["IDMenu"];
            _mainMenuClassName = ControlsTools.MainMenuName(_idMenu);
        }
		
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{    
		}
        #endregion
        #region protected override PageConstruction
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void PageConstruction()
        {
            string _titleLeft = Ressource.GetString(_idMenu, true);
            HtmlPageTitle titleLeft = new HtmlPageTitle(_titleLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle();
            PageTitle = _titleLeft;

            GenerateHtmlForm();

            FormTools.AddBanniere(this, Form, titleLeft, titleRight, string.Empty, _idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, _idMenu);
        }

		#endregion
		#region protected override GenerateHtmlForm
		protected override void GenerateHtmlForm ()
		{
			base.GenerateHtmlForm();
            AddToolBar();
            AddBody();
		}
        #endregion

        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + _mainMenuClassName };
            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Attributes.Add("onclick", "__doPostBack('0','SELFRELOAD_');");
            pnlToolBar.Controls.Add(btnRefresh);
            CellForm.Controls.Add(pnlToolBar);
        }

        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + _mainMenuClassName };

            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " " + _mainMenuClassName };
            togglePanel.SetHeaderTitle(Ressource.GetString("lblDisplayMode"));

            // Users Table        
            Table usersTable = new Table()
            {
                ID = "usersTable",
                Width = Unit.Percentage(100),
                CellSpacing = 0,
                CellPadding = 3,
                CssClass = "DataGrid",
                GridLines = GridLines.Both
            };
            usersTable.Controls.Add(CreateUsersTableRow("Username", "Active", "Allowed", "Safety model", "Allowed on Model", "Allowed on Actor", "First connection", "Last connection"));

            Panel pnlResult = new Panel()
            {
                ID = "divDtg",
                CssClass = CSSMode + " " + _mainMenuClassName
            };
            pnlResult.Controls.Add(usersTable);
            togglePanel.AddContent(pnlResult);
            pnlBody.Controls.Add(togglePanel);
            CellForm.Controls.Add(pnlBody);
        }


        #region private Page_Load
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageConstruction();

            Table tblUsers = (Table)this.FindControl("usersTable");

            if (tblUsers != null)
            {
                TableRow tr;
                int simultaneouslogin, actorSimultaneousLogin, modelSimultaneousLogin;
                int simultaneousLoginAuthorized = -1;
                int entityAuthorized = -1;
                SessionTools.License.IsValidRegistration(ref simultaneousLoginAuthorized, ref entityAuthorized);

                HttpConnectedUsers acu = new HttpConnectedUsers();
                ConnectedUsers cus = acu.GetConnectedUsers();
                //
                foreach (ConnectedUser cu in cus)
                {
                    SQL_Actor sql_Actor = new SQL_Actor(SessionTools.CS, cu.ida);
                    if (sql_Actor.IsLoaded)
                    {
                        DateTime dtFirstLogin = DateTime.MinValue;
                        DateTime dtLastLogin = DateTime.MinValue;

                        // Actor Simultaneous login -------------------------------------------------------
                        object obj = sql_Actor.GetFirstRowColumnValue("SIMULTANEOUSLOGIN");
                        actorSimultaneousLogin = (obj == null) ? -1 : Convert.ToInt32(obj);

                        //
                        // ModelSafety --------------------------------------------------------------------
                        string IdModelSafety = cu.idModelSafety.ToString();
                        SQL_MODELSAFETY sql_ModelSafety = new SQL_MODELSAFETY(SessionTools.CS, IdModelSafety);
                        modelSimultaneousLogin = sql_ModelSafety.SimultaneousLogin;
                        string modelName = sql_ModelSafety.GetFirstRowColumnValue("MODELNAME").ToString();
                        if (actorSimultaneousLogin == -1)
                            simultaneouslogin = modelSimultaneousLogin;
                        else
                            simultaneouslogin = actorSimultaneousLogin;

                        // First and last Login -----------------------------------------------------------
                        try
                        {
                            string sqlQuery = SQLCst.SQL_ANSI + Cst.CrLf;

                            sqlQuery += SQLCst.SELECT + SQLCst.MAX + "(DTSTPROCESS)" + SQLCst.AS + "DTLASTLOGIN," + Cst.CrLf;
                            sqlQuery += SQLCst.MIN + "(DTSTPROCESS)" + SQLCst.AS + "DTFIRSTLOGIN" + Cst.CrLf;
                            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESSDET_L + Cst.CrLf;
                            sqlQuery += SQLCst.WHERE + "DTSTPROCESS in (";
                            sqlQuery += SQLCst.SELECT + SQLCst.MAX + "(DTSTPROCESS)" + Cst.CrLf;
                            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.PROCESSDET_L + Cst.CrLf;
                            sqlQuery += SQLCst.WHERE + "( (SYSCODE='LOG' and SYSNUMBER in (3,4)) or (SYSCODE='SYS' and SYSNUMBER=10))" + Cst.CrLf;
                            sqlQuery += SQLCst.AND + "DATA1 = 'Identifier [" + sql_Actor.Identifier + "]'" + Cst.CrLf;
                            sqlQuery += SQLCst.GROUPBY + "IDPROCESS_L )" + Cst.CrLf;
                            sqlQuery += SQLCst.AND + "(SYSCODE='LOG' and SYSNUMBER=3)";
                            
                            //20070717 FI utilisation de SetCacheOff cela ne mange pas de pain
                            IDataReader DrDtFirstLastLogin = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlQuery);
                            if (DrDtFirstLastLogin.Read())
                            {
                                dtLastLogin = (Convert.IsDBNull(DrDtFirstLastLogin.GetValue(0)) ? DateTime.MinValue : DrDtFirstLastLogin.GetDateTime(0));
                                dtFirstLogin = (Convert.IsDBNull(DrDtFirstLastLogin.GetValue(1)) ? DateTime.MinValue : DrDtFirstLastLogin.GetDateTime(1));
                            }

                            DrDtFirstLastLogin.Dispose();
                        }
                        catch (Exception) { throw; }

                        // Add row
                        tr = CreateUsersTableRow(sql_Actor.Identifier, cu.nbConnection.ToString(), simultaneouslogin.ToString(),
                                    modelName, modelSimultaneousLogin.ToString(), actorSimultaneousLogin.ToString(), dtFirstLogin.ToString(), dtLastLogin.ToString());
                        tblUsers.Controls.Add(tr);
                    }
                    sql_Actor = null;
                }

                int countForAll = cus.GetNbConnectionAllUsers();
                tr = CreateUsersTableRow("All Users", countForAll.ToString(), simultaneousLoginAuthorized.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                tblUsers.Controls.Add(tr);
            }
        }
        #endregion
		#region SQL_MODELSAFETY
		public class SQL_MODELSAFETY : SQL_TableWithID
		{			
			#region constructor
			//Use int ident
			public SQL_MODELSAFETY(string pSource, int pId)
				: this(pSource, IDType.Id , pId.ToString(), RestrictEnum.No, ScanDataDtEnabledEnum.No,null,true){}
			public SQL_MODELSAFETY(string pSource, int pId, RestrictEnum pRestrictActor, ScanDataDtEnabledEnum pScanDataEnabled,string pSessionId, bool pIsSessionAdmin)
				: this(pSource, IDType.Id , pId.ToString(), pRestrictActor, pScanDataEnabled,pSessionId,pIsSessionAdmin){}
			//Use string ident
			public SQL_MODELSAFETY(string pSource, string pIdentifier)
				: this(pSource, IDType.Id, pIdentifier, RestrictEnum.No, ScanDataDtEnabledEnum.No,null,true){}
			public SQL_MODELSAFETY(string pSource,string pIdentifier, RestrictEnum pRestrictActor,ScanDataDtEnabledEnum pScanDataEnabled,string pSessionId, bool pIsSessionAdmin)
				: this(pSource, IDType.Id , pIdentifier,pRestrictActor, pScanDataEnabled,pSessionId,pIsSessionAdmin){}
			//
			public SQL_MODELSAFETY(string pSource,IDType pIdType, string pIdentifier,RestrictEnum pRestrictActor,ScanDataDtEnabledEnum pScanDataEnabled, string pSessionId, bool pIsSessionAdmin) : 
				base(pSource,Cst.OTCml_TBL.MODELSAFETY, pIdType, pIdentifier, pScanDataEnabled)
			{}
			#endregion constructor
	
			#region public property
			public int SimultaneousLogin
			{
				get 
				{ 
					object obj = GetFirstRowColumnValue("SIMULTANEOUSLOGIN");

					if (obj == null)
						return -1;
					else
						return Convert.ToInt32(obj);
				}
			}
			#endregion 
		}
		#endregion SQL_MODELSAFETY
		#region UsersTable
		private TableRow CreateUsersTableRow(string pUsers, string pConnected, string pAllowed, string pModelSafetyName, string pModelSimultaneousLogin, string pActorAllowed, string pDtFirstLogin, string pDtLastLogin)
		{
            Boolean isHeader = (pConnected == "Active");

            TableRow tr = new TableRow()
            {
                CssClass = (isHeader? "DataGrid_HeaderStyle": "DataGrid_ItemStyle")
            };

            // Users
            TableCell td = HtmlTools.NewLabelInCell(pUsers);
            tr.Controls.Add(td);
            
			// Connected           
            td = HtmlTools.NewLabelInCell(pConnected);
            tr.Controls.Add(td);

			// Allowed       
			pAllowed = (pAllowed == "-1" ? string.Empty : pAllowed);
            td = HtmlTools.NewLabelInCell(pAllowed);
            tr.Controls.Add(td);
			
			// ModelSafety         
            td = HtmlTools.NewLabelInCell(pModelSafetyName);
            tr.Controls.Add(td);

			if (StrFunc.IsFilled(pUsers))
			{
				// ModelSafetyAllowed   
				pModelSimultaneousLogin = (pModelSimultaneousLogin == "-1" ? string.Empty : pModelSimultaneousLogin);
                td = HtmlTools.NewLabelInCell(pModelSimultaneousLogin);
				tr.Controls.Add( td );
			}
			else
			{
				td.HorizontalAlign = HorizontalAlign.Left;
                td.ColumnSpan = 2;
			}

            // ActorAllowed    
            pActorAllowed = (pActorAllowed == "-1" ? string.Empty : pActorAllowed);
            td = HtmlTools.NewLabelInCell(pActorAllowed);
            tr.Controls.Add(td);
            
            // First Login         
            td = HtmlTools.NewLabelInCell(pDtFirstLogin);
			tr.Controls.Add( td );

			// Last Login         
            td = HtmlTools.NewLabelInCell(pDtLastLogin);
			tr.Controls.Add( td );

			return tr;
		}
		#endregion UsersTable
	}
}

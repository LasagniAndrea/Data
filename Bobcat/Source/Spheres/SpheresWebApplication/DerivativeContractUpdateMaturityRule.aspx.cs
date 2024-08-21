using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.Restriction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace EFS.Spheres
{
    /// <summary>
    /// 
    /// </summary>
    public enum MaturityRuleType
    {
        /// <summary>
        ///  Mise à jour de la règle d'échéance dite principale
        /// </summary>
        Main,
        /// <summary>
        ///  Mise à jour d'une règle d'échéance additonelle 
        /// </summary>
        Additional
    }
    /// <summary>
    /// 
    /// </summary>
    ///  FI 20220511 [XXXXX] Refactoring complet
    public partial class DerivativeContractMaturityRuleUpdate : ActionPage
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly DateTime _dtMarket = DateTime.MinValue;
        #endregion Members

        #region Accessors

        /// <summary>
        /// Utilisé comme clé unique de stockage (Session) de l'action courante
        /// </summary>
        private string UMR_GUID
        {
            get { return "UMR_" + GUID; }
        }

        private string IdMenu { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private MaturityRuleType MaturityRuleType
        {
            set { DataCache.SetData($"{UMR_GUID}_MaturityRuleType", value); }
            get { return DataCache.GetData<MaturityRuleType>($"{UMR_GUID}_MaturityRuleType"); }
        }

        /// <summary>
        /// 
        /// </summary>
        private int IdCurrentDC
        {
            set { DataCache.SetData($"{UMR_GUID}_IdCurrentDC", value); }
            get { return DataCache.GetData<int>($"{UMR_GUID}_IdCurrentDC"); }
        }

        /// <summary>
        /// 
        /// </summary>
        private int IdCurrentMR
        {
            set { DataCache.SetData($"{UMR_GUID}_IdCurrentMR", value); }
            get { return DataCache.GetData<int>($"{UMR_GUID}_IdCurrentMR"); }
        }

        /// <summary>
        /// 
        /// </summary>
        private SQL_MaturityRuleBase SQLCurrentMR
        {
            set { DataCache.SetData($"{UMR_GUID}_SqlCurrentMR", value); }
            get { return DataCache.GetData<SQL_MaturityRuleBase>($"{UMR_GUID}_SqlCurrentMR"); }
        }

        /// <summary>
        /// Obtient et définie les échéances négociées sur le DC courant et la MR courante
        /// </summary>
        private Tuple<ISpheresIdentification, List<Tuple<string, int>>> LivingTradesETD
        {
            set { DataCache.SetData(UMR_GUID + "LivingTradesETD", value); }
            get { return DataCache.GetData<Tuple<ISpheresIdentification, List<Tuple<string, int>>>>(UMR_GUID + "LivingTradesETD"); }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public DerivativeContractMaturityRuleUpdate()
        {
        }
        #endregion Constructor

        #region Events

        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion InitializeComponent


        /// <summary>
        /// ► Actions sur boutons de la barre d'outils
        ///   Enregistrement (btnRecord)
        ///   Annulation (btnCancel)
        ///   
        ///   Si validation exécution de la demande
        /// </summary>
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        protected void OnAction(object sender, CommandEventArgs e)
        {
            string eventTarget = Request.Params["__EVENTTARGET"];
            string eventArgument = Request.Params["__EVENTARGUMENT"];
            WebControl ctrl = sender as WebControl;
            bool isValid = false;
            if (StrFunc.IsFilled(eventTarget))
            {
                string message = string.Empty;
                if (StrFunc.IsEmpty(eventArgument))
                {
                    switch (eventTarget)
                    {
                        case "btnRecord":
                            int idNewMR = Convert.ToInt32(ddlNewMR.SelectedItem.Value);

                            if (idNewMR == 0)
                            {
                                message = Ressource.GetString("Msg_MRUNoUpdate") + Cst.CrLf;
                            }
                            else
                            {
                                string HTMLListOfLivingTradesETD = string.Empty;

                                // RD 20200207 [25081] Poursuivre le traitement y compris dans le cas où il n'existe pas de négociations sur ce DC
                                isValid = true;

                                if (LivingTradesETD.Item2.Count <= 0)
                                {
                                    HTMLListOfLivingTradesETD += LivingTradesETD.Item2.Count + Cst.Space + Ressource.GetString("Trades") + Cst.CrLf;
                                }
                                else
                                {
                                    //isValid = true;
                                    HTMLListOfLivingTradesETD = Cst.HTMLUnorderedList;
                                    foreach (var maturity in LivingTradesETD.Item2)
                                    {
                                        HTMLListOfLivingTradesETD += Cst.HTMLListItem + maturity.Item1 + " : " + maturity.Item2 + Cst.Space + Ressource.GetString("Trades") + Cst.HTMLEndListItem;
                                    }
                                    HTMLListOfLivingTradesETD += Cst.HTMLEndUnorderedList;
                                }
                                message = Ressource.GetString2("Msg_MRURecord", HTMLListOfLivingTradesETD) + Cst.CrLf;
                            }
                            break;
                        case "btnCancel":
                            isValid = true;
                            message = Ressource.GetString("Msg_MRUCancel") + Cst.CrLf;
                            break;
                    }
                    if (isValid && StrFunc.IsFilled(message))
                        JavaScript.ConfirmStartUpImmediate(this, message, ctrl.ID, "TRUE", "FALSE");
                    else if (StrFunc.IsFilled(message))
                        JavaScript.DialogImmediate(this, message, false, ProcessStateTools.StatusErrorEnum);
                }
                else if (eventArgument == "TRUE")
                {
                    //   Exécution de la demande suite à actions sur boutons de la barre d'outils (2ème passage)
                    //   Enregistrement (btnRecord)
                    //   Annulation (btnCancel)
                    switch (eventTarget)
                    {
                        case "btnRecord":
                            Save();
                            ReloadParentReferential();
                            JavaScript.DialogImmediate(this, Ressource.GetString("Msg_ProcessSuccessfull"), true, ProcessStateTools.StatusSuccessEnum);
                            break;
                        case "btnCancel":
                            this.ClosePage();
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Initialisation de la page
        /// </summary>
        /// <param name="e"></param>
        // EG 20200831 [25556] Nouvelle interface GUI v10(Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            // FI 20200217 [XXXXX] Ajout de HiddenGUID
            this.Form = formMRUpdate;

            AntiForgeryControl();
            AddInputHiddenGUID();

            SizeHeightForAlertImmediate = new Pair<int?, int?>(0, 640);
            SizeWidthForAlertImmediate = new Pair<int?, int?>(0, 600);

            IdMenu = Request.QueryString["IDMenu"]?.ToString();

            if (false == IsPostBack)
            {
                btnRecord.Enabled = SessionTools.IsSessionSysAdmin;
                if (StrFunc.IsFilled(IdMenu))
                {
                    btnRecord.Enabled = true;
                    RestrictionPermission restrictPermission = new RestrictionPermission(IdMenu, SessionTools.User);
                    restrictPermission.Initialize(CSTools.SetCacheOn(SessionTools.CS));
                    if (!restrictPermission.IsModify)
                        btnRecord.Enabled = false;
                }

                divbody.CssClass = CSSMode + " input";
                pnlCharacteristicsGen.CssClass = CSSMode + " input";

                SetMaturityRuleType();
                btnRecord.Enabled = btnRecord.Enabled && SetDCAndMR();

                SetDerivativeContractDetails();
                SetMaturityRule();

                DateTime dtMinEntityMarket = MarketTools.GetMinDateMarket(CSTools.SetCacheOn(SessionTools.CS));
                
                LivingTradesETD = PageBaseReferentialv2.GetCountOfLivingTradesETD(SessionTools.CS, IdCurrentDC, IdCurrentMR, dtMinEntityMarket);
                
                // FI 20221019 [XXXX] s'il existe des trades en vie => ne pas permettre de changer de format de MR 
                LoadMaturityRuleDDL(LivingTradesETD.Item2.Count()>0);

                if (this.MaturityRuleType == MaturityRuleType.Main)
                {
                    if (this.FindControl("divMainMR") is Panel panel)
                        panel.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
            }
        }


        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Header
            string subTitle = Ressource.GetMenu_Shortname2(ACommon.IdMenu.GetIdMenu(ACommon.IdMenu.Menu.DrvContract), ACommon.IdMenu.GetIdMenu(ACommon.IdMenu.Menu.DrvContract));
            string subTitleData = txtIdentifier.Text;
            if (StrFunc.IsFilled(txtDescription.Text))
                subTitleData += $" - {txtDescription.Text}";

            string leftTitle = Ressource.GetString(IdMenu, true);
            this.PageTitle = leftTitle;
            PageTools.SetHead(this, leftTitle, null, null);

            // Timeout sur Block
            JQuery.Block block = new JQuery.Block(IdMenu, "Msg_DerivativeContractUpdateMRInProgress", true)
            {
                Timeout = SystemSettings.GetTimeoutJQueryBlock("DerivateContractUpdateMaturityRule")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);

            PageTools.SetHeaderLink(this.Header, "linkCssAction", "~/Includes/DerivateContractUpdateMaturityRule.min.css");

            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle, $"{subTitle}: {subTitleData}");
            Panel divHeader = new Panel
            {
                ID = "divHeader"
            };
            divHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, ACommon.IdMenu.GetIdMenu(ACommon.IdMenu.Menu.Input)));
            plhHeader.Controls.Add(divHeader);
            #endregion Header

            SetRessources();
        }
        #endregion Events

        #region Methods

        /// <summary>
        /// Set resources on Controls
        /// </summary>
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        private void SetRessources()
        {
            // Boutons
            btnRecord.Text = "<i class='fa fa-save'></i> " + Ressource.GetString("btnRecord2");
            btnCancel.Text = "<i class='fa fa-times'></i> " + Ressource.GetString("btnCancel");
        }
        /// <summary>
        /// Initialisation des TextBox sur l'IHM
        /// </summary>
        private void SetDerivativeContractDetails()
        {
            string CS = SessionTools.CS;
            // FI 20221019 [XXXX] usage du left outer et isnull car DERIVATIVECONTRACT.IDMATURITYRULE est nullable
            string sqlQuery = @"select dc.IDENTIFIER, dc.DISPLAYNAME, dc.DESCRIPTION, m.DISPLAYNAME as MARKET_DISPLAYNAME, isnull(mr.MATURITYRULE,'') as MATURITYRULE
from dbo.DERIVATIVECONTRACT dc
left outer join dbo.VW_MATURITYRULE mr on mr.IDMATURITYRULE = dc.IDMATURITYRULE
inner join dbo.MARKET m on m.IDM=dc.IDM
where IDDC=@IDDC";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDDC), IdCurrentDC);

            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                {
                    txtIdentifier.Text = dr["IDENTIFIER"].ToString();
                    txtDisplayName.Text = dr["DISPLAYNAME"].ToString();
                    txtDescription.Text = dr["DESCRIPTION"].ToString();
                    txtMainMR.Text = dr["MATURITYRULE"].ToString();

                    txtMarket.Text = dr["MARKET_DISPLAYNAME"].ToString();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetMaturityRule()
        {
            // FI 20221019 [XXXX] add test sur IdCurrentDC > 0
            if (IdCurrentDC > 0)
            {
                SQLCurrentMR = new SQL_MaturityRuleRead(SessionTools.CS, IdCurrentMR);
                Boolean ret = SQLCurrentMR.LoadTable(new string[] { "IDENTIFIER", "DISPLAYNAME", "DESCRIPTION", "MMYFMT", "MATURITYRULE" });
                if (ret)
                    txtCurrentMR.Text = SQLCurrentMR.MaturityRule;
            }
        }



        /// <summary>
        /// Chargement de la DDL Maturity Rule
        /// </summary>
        /// <param name="isKeepCurrentMRFormat">si true ne charge que les MRs dont le format est identique au format de la MR courante</param>
        /// FI 20221019 [XXXX] Add paramètre isKeepCurrentMRFormat
        private void LoadMaturityRuleDDL(Boolean isKeepCurrentMRFormat)
        {
            DataDCMREnabled dataDCMREnabled = new DataDCMREnabled(SessionTools.CS, null, DateTime.MinValue);
            int[] idMRCurrentDC = (from item in dataDCMREnabled.GetDCMR(IdCurrentDC)
                                   select item.IdMR).ToArray();

            // FI 20221019 [XXXX] Ne pas permettre de sélectionner une MR déjà utilisée
            string restrictMR = string.Empty;
            if (ArrFunc.IsFilled(idMRCurrentDC))
                restrictMR = $" and ({DataHelper.SQLColumnIn(SessionTools.CS, "IDMATURITYRULE", idMRCurrentDC, TypeData.TypeDataEnum.integer,true)})";

            string restrictFormat = string.Empty;
            if (isKeepCurrentMRFormat)
                restrictFormat = $" and (MMYFMT = '{SQLCurrentMR.MaturityFormat}')";
            
            string sqlQuery = $@"select IDMATURITYRULE, MATURITYRULE
from dbo.VW_MATURITYRULE_ENABLED
where (IDENTIFIER != 'Default Rule') {restrictMR} {restrictFormat}
order by MATURITYRULE";

            using (DataTable dt = DataHelper.ExecuteDataTable(SessionTools.CS, sqlQuery))
            {
                foreach (DataRow row in dt.Rows)
                    ddlNewMR.Items.Add(new ListItem(row["MATURITYRULE"].ToString(), row["IDMATURITYRULE"].ToString()));
                ddlNewMR.Items.Insert(0, new ListItem(string.Empty, "0"));
            }
        }



        /// <summary>
        /// Recharger la page parent 
        /// </summary>
        private void ReloadParentReferential()
        {
            string nameFunction = "RefreshParentWindow";
            JavaScript.JSStringBuilder sb = new EFS.Common.Web.JavaScript.JSStringBuilder();
            sb.AppendLine("window.opener.location.reload();");
            string script = sb.ToString();
            this.RegisterScript(nameFunction, script);
        }


        /// <summary>
        ///  
        /// </summary>
        /// FI 20190605 [XXXXX] Add
        private void Save()
        {
            LockObject lockObject = new LockObject(TypeLockEnum.REPOSITORY, IdCurrentDC, string.Empty, LockTools.Exclusive);
            try
            {
                Lock lck = new Lock(SessionTools.CS, lockObject, SessionTools.AppSession, "Update");
                if (false == LockTools.LockMode1(lck, out Lock lockEx))
                    throw new Exception(lockEx.Message());

                int idNewMR = Convert.ToInt32(ddlNewMR.SelectedItem.Value);

                UpdateMaturityRuleProcess updateMaturityRuleProcess = new UpdateMaturityRuleProcess(SessionTools.CS, SessionTools.AppSession);
                switch (MaturityRuleType)
                {
                    case MaturityRuleType.Main:
                        updateMaturityRuleProcess.UpdateMainMaturityRule(IdCurrentDC, idNewMR);
                        break;
                    case MaturityRuleType.Additional:
                        updateMaturityRuleProcess.UpdateAdditionalMaturityRule(IdCurrentDC, IdCurrentMR, idNewMR);
                        break;
                }

                if (LivingTradesETD.Item2.Count > 0)
                {
                    updateMaturityRuleProcess.UpdateDerivAttrib(IdCurrentDC, IdCurrentMR, idNewMR);
                    // RD 20200207 [25081] Indiquer que l'appel de la méthode provient de la modification d'un DC
                    updateMaturityRuleProcess.UpdateMaturitiesAndEvents(idNewMR, out _, true);
                }
            }
            finally
            {
                LockTools.UnLock(SessionTools.CS, lockObject, SessionTools.AppSession.SessionId);
                // FI 20221019 [XXXX] Add
                DataDCMREnabled.ClearCache(SessionTools.CS);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetMaturityRuleType()
        {
            string MRType = Request.QueryString["MRType"]?.ToString();
            if (StrFunc.IsFilled(MRType))
            {
                if (Enum.TryParse<MaturityRuleType>(MRType, true, out MaturityRuleType maturityRuleTypeURL))
                    MaturityRuleType = maturityRuleTypeURL;
                else
                    throw new NotSupportedException($"URL: MRType: {MRType} is invalid");
            }
            else
                throw new NotSupportedException($"URL: MRType is not specified");

        }

        /// <summary>
        /// 
        /// </summary>
        private Boolean SetDCAndMR()
        {
            string CS = SessionTools.CS;
            string FK = Request.QueryString["FK"]?.ToString();
            if (StrFunc.IsEmpty(FK))
                throw new NotSupportedException($"URL: FK is not specified");

            Boolean isOk = false;
            string query;
            DataParameters dp;
            QueryParameters qry;

            switch (MaturityRuleType)
            {
                case MaturityRuleType.Main:
                    IdCurrentDC = Convert.ToInt32(Request.QueryString["FK"]);
                    query = "select IDMATURITYRULE from dbo.DERIVATIVECONTRACT where IDDC = @IDDC";
                    dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDDC), IdCurrentDC);
                    qry = new QueryParameters(CS, query, dp);
                    using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            isOk = true;
                            IdCurrentMR = Convert.IsDBNull(dr["IDMATURITYRULE"]) ? 0 : Convert.ToInt32(dr["IDMATURITYRULE"]);
                        }
                    }
                    break;
                case MaturityRuleType.Additional:
                    query = "select IDDC, IDMATURITYRULE from dbo.DRVCONTRACTMATRULE where IDDRVCONTRACTMATRULE = @ID";
                    dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.ID), Convert.ToInt32(Request.QueryString["FK"]));
                    qry = new QueryParameters(CS, query, dp);
                    using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            isOk = true;
                            IdCurrentDC = Convert.ToInt32(dr["IDDC"]);
                            IdCurrentMR = Convert.ToInt32(dr["IDMATURITYRULE"]);
                        }
                    }
                    break;
            }

            return isOk;
        }
        #endregion Methods
    }
}
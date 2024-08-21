#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EfsML.Business;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de CapturePageBase.
    /// Cette page est partagée par la saisie des évènements et la saisie des trades,factures et des titres
    /// </summary>
    public partial class CapturePageBase : CciPageDesign
    {
        #region Enum
        /// <summary>
        ///  Représente le type de trade (Trade,Facture,ou Titre) 
        /// </summary>
        protected enum TradeKeyEnum
        {
            Trade,
            TradeAdmin,
            DebtSecurity,
            TradeRisk
        }
        #endregion
        //
        #region Members
        /// <summary>
        /// Représente la party par défaut
        /// </summary>
        protected EFS_DefaultParty m_DefaultParty;
        /// <summary>
        /// Représente la devise par defaut
        /// </summary>
        protected string m_DefaultCurrency;
        /// <summary>
        /// Représente le businessCenter par défaut
        /// </summary>
        protected string m_DefaultBusinessCenter;

        // FI 20141001 [XXXXX] la variable m_IdMenu est abandonné il faut utiliser InputGUI.IdMenu
        ///// <summary>
        ///// Représente le menu
        ///// </summary>
        //protected string m_IdMenu;
        #endregion Members
        //
        #region Accessors

        /// <summary>
        /// Obtient le contrôle qui contient tradeIdentifier dans la toolbar
        /// </summary>
        protected Control CtrlTrade
        {
            get
            {
                Control ret = null;
                Control ctrlContainer = FindControl("tblMenu");
                if (null != ctrlContainer)
                    ret = ctrlContainer.FindControl("txtTrade");
                return ret;
            }
        }

        /// <summary>
        /// Obtient le contrôle caché qui contient IdT
        /// </summary>
        protected HtmlInputHidden CtrlTradeIdT
        {
            get
            {
                Control ctrlContainer = FindControl("tblMenu");
                HtmlInputHidden ctrlTradeIdT = null;
                if (null != ctrlContainer)
                    ctrlTradeIdT = (HtmlInputHidden)ctrlContainer.FindControl("hihTradeIdT");
                return ctrlTradeIdT;
            }
        }

        /// <summary>
        /// Obtient ou définit la valeur Text du contrôle qui contient l'identifier du trade
        /// </summary>
        protected string TradeIdentifier
        {
            get
            {
                Control ctrl = CtrlTrade;
                if (null != ctrl)
                    return ((TextBox)ctrl).Text.Trim();
                else
                    return String.Empty;
            }
            set
            {
                Control ctrl = CtrlTrade;
                if (null != ctrl)
                    ((TextBox)ctrl).Text = value;
            }
        }

        /// <summary>
        /// Obtient ou Définit la valeur value du contôle TradeIdT
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        protected override int TradeIdT
        {
            get
            {
                int ret = -1;
                HtmlInputHidden ctrl = CtrlTradeIdT;
                if ((null != ctrl) && StrFunc.IsFilled(ctrl.Value))
                    ret = Convert.ToInt32(ctrl.Value);
                //if (null == ctrl)
                //    throw new Exception("Control Trade IdT is null");
                //try
                //{
                //    if (StrFunc.IsFilled(ctrl.Value))
                //        ret = Convert.ToInt32(ctrl.Value);
                //}
                //catch
                //{
                //    ret = -1;
                //}
                return ret;
            }
            set
            {
                HtmlInputHidden ctrl = CtrlTradeIdT;
                if (null != ctrl)
                    ctrl.Value = value.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string FullConstructorSessionID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("FULLCTOR");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string InputGUISessionID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("GUI");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string InputSessionID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("Input");
            }
        }

        ///<summary>
        /// 
        ///</summary>
        ///<remarks> Don't touch this property. Use by Full FpML Interface </remarks> 
        public bool IsScreenFullCapture
        {
            get
            {
                bool ret = false;
                if (null != InputGUI)
                    ret = (Cst.Capture.TypeEnum.Full == InputGUI.CaptureType);
                return ret;
            }
        }

        /// <summary>
        /// Obtient false pour indiquer que la page en cours n'est pas un Zoom vers la saisie Full
        /// </summary>
        /// //FI 20091118 [16744] Add IsZoomOnFull
        public bool IsZoomOnFull
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// isSupportsPartialRendering
        /// </summary>
        /// <remarks>
        /// l'utilisation de l'updatePanel (AJAX) ralentit la page en mode Full => isSupportsPartialRendering= false si Full
        /// </remarks>
        /// EG 20201014 Désactivation de cet override (Plantage Viewstate en swappant entre Saisie Light et Full)
        //protected override bool isSupportsPartialRendering
        //{
        //    get
        //    {
        //        bool ret = false;
        //        //
        //        if (IsScreenFullCapture)
        //            ret = false;
        //        else
        //            ret = base.isSupportsPartialRendering;
        //        //
        //        return ret;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        protected virtual TradeKeyEnum TradeKey
        {
            get { return TradeKeyEnum.Trade; }
        }

        /// <summary>
        /// Obtient le screen
        /// </summary>
        protected override string ScreenName
        {
            get { return InputGUI.CurrentIdScreen; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string TitleLeft
        {
            get
            {
                return base.TitleLeft + ": " + TitleRight;
            }
        }
        #endregion Accessors
        //
        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start CapturePageBase.OnInit");

            base.OnInit(e);
            // FI 20141001 [XXXXX] la variable m_IdMenu est abandonné InputGUI.IdMenu
            //m_IdMenu = Request.QueryString["IDMenu"];
            AbortRessource = true;

            AddAuditTimeStep("End CapturePageBase.OnInit");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnScreen(object sender, EventArgs e)
        {
            string previousIdScreen = InputGUI.CurrentIdScreen;
            if ((false == IsModeConsult) && (null != sender))
            {
                if (null != sender as DropDownList)
                {
                    string id = ((DropDownList)sender).SelectedValue;
                    if (!StrFunc.IsEmpty(id))
                    {
                        string newIdScreen = ((DropDownList)sender).SelectedItem.Text;
                        if (Cst.FpML_ScreenFullCapture == newIdScreen)
                        {
                            //Changement d'écran passage vers Ecran full 
                            //save des cci (Avec ClenUp)  
                            if (StrFunc.IsFilled(previousIdScreen) &&
                                (Cst.FpML_ScreenFullCapture != previousIdScreen) &&
                                ArrFunc.IsFilled(Object.CcisBase))
                                Object.CcisBase.SaveCapture(Page);
                        }
                        // SetInitialFocus(); => ça ne marche pas c'est franchement dommage je regarderai plus tard
                    }
                }
            }

            base.OnScreen(sender, e);
        }
        /// <summary>
        /// 
        /// </summary>
        protected void OnZoomFpml()
        {
            string arg = Request.Params["__EVENTARGUMENT"];
            string[] args = arg.Split(';');
            if (ArrFunc.IsFilled(args) && args.Length > 0)
                JavaScript.OpenObjectFpMLImmediate(this, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20140930 [XXXXX] Modify
        protected override void OnLoad(EventArgs e)
        {
            // FI 20120904 Add BlockUI New
            // EG 20130725 Timeout sur Block
            JQuery.Block block = new JQuery.Block((PageBase)this.Page)
            {
                // FI 20140930 [XXXXX] utilisation de InputGUI.IdMenu 
                Timeout = SystemSettings.GetTimeoutJQueryBlock(InputGUI.IdMenu)
            };
            JQuery.UI.WriteInitialisationScripts(this, block);

            base.OnLoad(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20140708 [20179] Add override Method
        protected override void OnPreRender(EventArgs e)
        {
            string eventTarget = string.Empty + PARAM_EVENTTARGET;
            string eventArgument = string.Empty + PARAM_EVENTARGUMENT;

            // FI 20140708 [20179] gestion deu mode Cst.Capture.Match
            if ((null != Object.CcisBase) && (Cst.Capture.IsModeMatch(InputGUI.CaptureMode)))
            {
                if (eventArgument == "match")
                {
                    // Passage du statut match à unmatch et vis et versa
                    if (GetCciControl(eventTarget) is WebControl control)
                        ControlsTools.ToggleMatch(control);
                }
            }

            base.OnPreRender(e);
        }
        #endregion Events
        
        #region Methods
        // EG 20160119 Refactoring Summary
        protected override void AddValidatorSummary()
        {
            Panel pnlSummary = new Panel
            {
                ID = "pnlSummary",
                EnableViewState = false
            };
            pnlSummary.Style[HtmlTextWriterStyle.Overflow] = "auto";

            // ValidationSummary
            ValidationSummary vsum = new ValidationSummary
            {
                ID = "ValidationSummary"
            };
            vsum.Style[HtmlTextWriterStyle.Color] = CstCSSColor.red;
            vsum.ShowMessageBox = false;
            vsum.ShowSummary = true;
            vsum.CssClass = "ValidationSummary";
            vsum.HeaderText = Ressource.GetString("FpMLHeaderSummary");

            pnlSummary.Controls.Add(vsum);
            CellForm.Controls.Add(pnlSummary);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUrl"></param>
        /// <returns></returns>
        public override string GetCompleteHelpUrl(string pUrl)
        {
            string ret = string.Empty;
            bool isEFSmLHelp = (bool)SystemSettings.GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
            string helpUrl = SystemSettings.GetAppSettings(isEFSmLHelp ? "EFSmLHelpSchemasUrl" : "FpMLHelpUrl");
            if (StrFunc.IsFilled(helpUrl))
                ret = helpUrl + pUrl;
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSchema"></param>
        /// <param name="pElement"></param>
        /// <returns></returns>
        public override string GetCompleteHelpUrl(string pSchema, string pElement)
        {
            bool isEFSmLHelp = (bool)SystemSettings.GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
            string url;
            if (isEFSmLHelp)
                url = MethodsGUI.GetEFSmLHelpUrl(pSchema, pElement);
            else
                url = MethodsGUI.GetFpMLHelpUrl(pSchema, pElement);
            return url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140930 [XXXXX] Modify
        protected virtual void OnZoomScreenBox()
        {
            string arg = Request.Params["__EVENTARGUMENT"];
            string[] args = arg.Split(';');
            if (ArrFunc.IsFilled(args) && args.Length > 0)
            {
                //FI 20140930 [XXXXX] Utilisation de InputGUI.IdMenu
                JavaScript.OpenScreenBoxImmediate(this, InputGUI.IdMenu, ScreenName, args[0], args[1], args[2]);
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140708 [20179] Add Method
        protected void OnValidateMatch(object sender, CommandEventArgs e)
        {
            // Initialisation des cci.NewValueMatch à partir de l'IHM
            Object.CcisBase.Initialize_FromGUI(this);

            List<CustomCaptureInfo> list = new List<CustomCaptureInfo>();
            foreach (CustomCaptureInfo cci in Object.CcisBase)
            {
                CustomObject co = GetCciCustomObject(cci.ClientId);
                if ((null != co) && co.IsToMatch)
                    list.Add(cci);
            }

            if (list.Count > 0)
            {
                Boolean isOK = RecordMatch(list);
                if (isOK)
                    OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
            }
        }
        
        /// <summary>
        /// Sauvegarde du résultat du matching des données
        /// </summary>
        /// <param name="pList"></param>
        /// /// FI 20140708 [20179] Add Method
        protected virtual Boolean RecordMatch(List<CustomCaptureInfo> pList)
        {
            throw new NotImplementedException("Methode RecordMatch is not override");
        }

        


        #endregion Methods
    }
}


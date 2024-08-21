
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Status;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    /// <summary>
    /// En tete
    /// </summary>
    public abstract class  HeaderBannerBase
    {
        #region Members
        public string ctrlTxtDisplayName;
        public string ctrlTxtDescription;
        public string ctrlTxtIdentifier;
        public string ctrlTxtExtLink;
        //
        public string ctrlLblOperator;
        public string ctrlDspOperator;
        //
        public string ctrlLblProcess;
        public string ctrlPnlProcess;
        public string ctrlDspProcess;
        //
        public string ctrlLblDtSys;
        public string ctrlDspDtSys;
        public string ctrlLblIdSys;
        public string ctrlDspIdSys;
        
        /// <summary>
        /// 
        /// </summary>
        protected string m_GUID;

        /// <summary>
        /// Panel qui contient tous les controles 
        /// </summary>
        protected Panel m_HeaderPanelContainer;

        /// <summary>
        /// Page qui contient le Header
        /// </summary>
        protected PageBase m_Page;

        /// <summary>
        /// Control de la page qui contient le Header (panel m_HeaderPanelContainer)
        /// </summary>
        protected Control m_ControlContainer;

        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected string m_MainMenuClassName;

        /// <summary>
        /// 
        /// </summary>
        protected bool m_IsStatusButtonVisible;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public string GUID
        {
            get { return m_GUID; }
        }

        /// <summary>
        /// Nbr de process affichés
        /// </summary>
        public int NbDisplayProcess
        {
            protected set;
            get;
        }

        #endregion Accessors

        #region Constructors
        /// <summary>
        ///  Ajoute dans {pControlContainer} de la page {pPage} le panel "divdescandstatus"
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pGUID"></param>
        /// <param name="pControlContainer"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public HeaderBannerBase(PageBase pPage, string pGUID, Control pControlContainer, bool pIsStatusButtonVisible, string pMainMenuClassName)
        {
            #region Banner Common Controls
            ctrlTxtIdentifier = Cst.TXT + "HeaderIdentifier";
            ctrlTxtDisplayName = Cst.TXT + "HeaderDisplayName";
            ctrlTxtDescription = Cst.TXT + "HeaderDescription";
            ctrlTxtExtLink = Cst.TXT + "HeaderExtlLink";

            ctrlLblOperator = Cst.LBL + "Operator";
            ctrlDspOperator = Cst.DSP + "Operator";

            ctrlLblProcess = Cst.LBL + "Process";
            ctrlPnlProcess = "divprocess";
            ctrlDspProcess = Cst.DSP + "Process";

            ctrlLblDtSys = Cst.LBL + "DtSys";
            ctrlDspDtSys = Cst.DSP + "DtSys";
            ctrlLblIdSys = Cst.LBL + "IdSys";
            ctrlDspIdSys = Cst.DSP + "IdSys";

            NbDisplayProcess = 10;
            #endregion Banner Common Controls

            m_Page = pPage;
            m_GUID = pGUID;
            m_ControlContainer = pControlContainer;
            m_MainMenuClassName = pMainMenuClassName;
            m_IsStatusButtonVisible = pIsStatusButtonVisible;

            AddPanelHeader();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddPanelHeader()
        {
            WCBodyPanel pnlDescription = new WCBodyPanel(" gray descandstatus") { ID = "divdescandstatus" };
            m_HeaderPanelContainer = pnlDescription.Controls[0] as Panel;
            m_ControlContainer.Controls.Add(pnlDescription);
        }

        /// <summary>
        /// Ajoute les contôles web identifier, displayName, description 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddDisplaynameDescriptionPanel()
        {
            Panel pnl = new Panel
            {
                ID = "divdspnamedesc",
                CssClass = "description " + m_MainMenuClassName
            };
            LiteralControl ltc = new LiteralControl(Ressource.GetString("IDENTIFIER"));
            pnl.Controls.Add(ltc);

            TextBox txt = new TextBox
            {
                ID = ctrlTxtIdentifier,
                CssClass = EFSCssClass.Capture,
                Width = Unit.Percentage(20)
            };
            pnl.Controls.Add(txt);

            txt = new TextBox
            {
                ID = ctrlTxtDisplayName,
                CssClass = EFSCssClass.Capture,
                Width = Unit.Percentage(30)
            };
            pnl.Controls.Add(txt);

            txt = new TextBox
            {
                ID = ctrlTxtDescription,
                CssClass = EFSCssClass.CaptureOptional,
                Width = Unit.Percentage(40)
            };
            pnl.Controls.Add(txt);

            WCTooltipLabel lbl = new WCTooltipLabel
            {
                ID = ctrlLblIdSys,
                Text = "Id"
            };
            pnl.Controls.Add(lbl);

            lbl = new WCTooltipLabel
            {
                ID = ctrlDspIdSys,
                CssClass = EFSCssClass.LabelDisplay,
                Text = string.Empty
            };
            lbl.Pty.TooltipContent = Ressource.GetString("ID");
            lbl.Width = Unit.Percentage(4);
            pnl.Controls.Add(lbl);

            m_HeaderPanelContainer.Controls.Add(pnl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected void AddDateSystemOperatorPanel()
        {
            Panel pnl = new Panel
            {
                CssClass = "dateandoperator"
            };

            #region Display DateSytem
            WCTooltipLabel lbl = new WCTooltipLabel
            {
                ID = ctrlLblDtSys,
                Text = ""
            };
            pnl.Controls.Add(lbl);
            lbl = new WCTooltipLabel
            {
                ID = ctrlDspDtSys,
                CssClass = EFSCssClass.LabelDisplay
            };
            pnl.Controls.Add(lbl);
            #endregion Display DateSytem

            #region Display Operator
            lbl = new WCTooltipLabel
            {
                ID = ctrlLblOperator,
                Text = Ressource.GetString("Operator")
            };
            pnl.Controls.Add(lbl);
            lbl = new WCTooltipLabel
            {
                ID = ctrlDspOperator,
                CssClass = EFSCssClass.LabelDisplay,
                Text = Ressource.GetString("Operator")
            };
            pnl.Controls.Add(lbl);
            #endregion Display Operator

            m_HeaderPanelContainer.Controls.Add(pnl);
        }

        /// <summary>
        ///  Ajoute les contôles web spécifiques à l'afficahe des process (L
        /// </summary>
        /// <returns></returns>
        protected void AddProcessPanel()
        {
            Panel pnl = new Panel
            {
                ID = ctrlPnlProcess,
                CssClass = "process"
            };

            LiteralControl ltc = new LiteralControl(Ressource.GetString("Process"));
            pnl.Controls.Add(ltc);

            for (int nbDspProcess = 0; nbDspProcess < NbDisplayProcess; nbDspProcess++)
            {
                WCTooltipLabel lbl = new WCTooltipLabel
                {
                    ID = ctrlDspProcess + nbDspProcess.ToString(),
                    CssClass = EFSCssClass.LabelDisplay
                };
                pnl.Controls.Add(lbl);
            }

            m_HeaderPanelContainer.Controls.Add(pnl);
        }



        /// <summary>
        ///  Ajoute les contôles web spécifiques aux données status ( libellé +  bouton)
        /// </summary>
        /// <returns></returns>
        /// FI 20161124 [22634] add Method
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddStatusPanel(Status.Mode  pMode)
        {
            Panel pnl = new Panel
            {
                CssClass = "status"
            };
            pnl.Controls.Add(new LiteralControl(Ressource.GetString("TradeStatus")));

            StatusEnum[] status = StatusTools.GetAvailableStatus(pMode);
            foreach (StatusEnum item in status)
            {
                WCTooltipLabel lbl = new WCTooltipLabel();

                string id = StatusTools.IsStatusUser(item) ? item.ToString() : item.ToString().Replace("Status", "St");
                lbl.ID = StrFunc.AppendFormat("{0}{1}", "lbl", id);
                pnl.Controls.Add(lbl);
            }

            #region Status and Button "..."
            WCToolTipLinkButton imgStatus = new WCToolTipLinkButton
            {
                CssClass = "fa-icon input",
                ID = "imgStatus",
                AccessKey = "V",
                Visible = m_IsStatusButtonVisible,
                Text = @"<i class='fas fa-ellipsis-h'></i>",
                CausesValidation = false
            };
            imgStatus.ToolTip = Ressource.GetString("Status") + Cst.GetAccessKey(imgStatus.AccessKey);
            if (BoolFunc.IsFalse(SystemSettings.GetAppSettings("Spheres_InputButton_TabActivation")))
                imgStatus.TabIndex = -1;
            pnl.Controls.Add(imgStatus);
            #endregion Status and Button "..."

            m_HeaderPanelContainer.Controls.Add(pnl);
        }


        /// <summary>
        ///  Ajout des contôles web de l'entête 
        ///  <para>AddDisplaynameDescription</para>
        /// </summary>
        public virtual void AddControls()
        {
            AddDisplaynameDescriptionPanel();
        }
        
        /// <summary>
        /// Alimentation de l'entête
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        /// <param name="pIsRefresh"></param>
        public virtual void DisplayHeader(bool pIsReadOnly, bool pIsRefresh)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pLbl"></param>
        protected static void SetToHtmlControl(EFS.Status.Status pStatus, WCTooltipLabel pLbl)
        {

            bool isVisible = true;
            if (pStatus.NewSt == Cst.STATUSREGULAR)
                isVisible = (pStatus.NewSt != pStatus.CurrentSt);

            if (isVisible)
            {
                pLbl.Text = pStatus.NewStExtend.ExtValue;
                if (StrFunc.IsFilled(pStatus.Tooltip))
                {
                    pLbl.Pty.TooltipContent = pStatus.Tooltip;
                    pLbl.Pty.TooltipTitle = pStatus.TooltipTitle;
                    pLbl.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                }
                else
                    pLbl.Style.Add(HtmlTextWriterStyle.Cursor, "default");

                pLbl.ForeColor = Color.FromName(pStatus.NewStExtend.ForeColor);
                pLbl.BackColor = Color.FromName(pStatus.NewStExtend.BackColor);
                pLbl.Font.Bold = (pStatus.CurrentSt != pStatus.NewSt);
                if ((pLbl.BackColor != Color.Transparent) || (pLbl.BackColor != Color.White))
                {
                    pLbl.Style.Add(HtmlTextWriterStyle.PaddingLeft, "5px");
                    pLbl.Style.Add(HtmlTextWriterStyle.PaddingRight, "5px");
                }
            }
            pLbl.Visible = isVisible;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pStatus">Liste de statuts user ordonnés par poids</param>
        /// <param name="pLbl"></param>
        protected static void SetToHtmlControl(string pCS, Status.Status[] pStatus, WCTooltipLabel pLbl)
        {
            int count = 0;

            bool isVisible = false;
            bool isBold = false;

            // FI 20200820 [25468] dates systemes en UTC, Tooltip affichés selon le profil avec précison à la seconde
            AuditTimestampInfo auditTimestampInfo = new AuditTimestampInfo()
            {
                Collaborator = SessionTools.Collaborator,
                TimestampZone = SessionTools.AuditTimestampZone,
                Precision = Cst.AuditTimestampPrecision.Second
            };


            for (int i = 0; i < pStatus.Length; i++)
            {
                if (Convert.ToBoolean(pStatus[i].NewValue))
                {
                    count++;
                    pStatus[i].SetTooltip(pCS, auditTimestampInfo);
                    isVisible = true;

                    if (pStatus[i].CurrentValue != pStatus[i].NewValue)
                        isBold = true;

                    if (count < 3)
                    {
                        string foreColor = pStatus[i].NewStExtend.ForeColor;
                        string backColor = pStatus[i].NewStExtend.BackColor;

                        pLbl.Text = pStatus[i].NewStExtend.ExtValue + Cst.HTMLSpace;
                        pLbl.ForeColor = Color.FromName(StrFunc.IsFilled(foreColor) ? foreColor : Color.Black.Name);
                        pLbl.BackColor = Color.FromName(StrFunc.IsFilled(backColor) ? backColor : Color.Transparent.Name);
                        pLbl.Font.Bold = isBold;
                        pLbl.Pty.TooltipContent = pStatus[i].Tooltip;
                        pLbl.Pty.TooltipTitle = pStatus[i].TooltipTitle;
                    }
                    else if (count == 3)
                    {
                        pLbl.Text = "...";
                        pLbl.ForeColor = Color.Black;
                        pLbl.BackColor = Color.Transparent;
                        pLbl.Font.Bold = true;
                    }
                }
            }
            pLbl.Visible = isVisible;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryParameters"></param>
        /// FI 20161124 [22634] Add Method
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected void SetDspProcess(QueryParameters queryParameters)
        {
            string CS = SessionTools.CS;
            string lastData = string.Empty;
            
            int nbDspProcess = 0;
            IDataReader dr = DataHelper.ExecuteDataTable(CS, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()).CreateDataReader();
            while (dr.Read())
            {
                string data = dr["code"].ToString();
                if (data != lastData)
                {
                    lastData = data;
                    string color = dr["color"].ToString();
                    if (m_Page.FindControl(ctrlDspProcess + nbDspProcess.ToString()) is WCTooltipLabel tt_Lbl)
                    {
                        tt_Lbl.Text = data;
                        if (color.ToLower() == "gray" || color.ToLower() == "black")
                        {
                            tt_Lbl.CssClass = "nomark " + color;

                        }
                        else
                        {
                            tt_Lbl.CssClass = "mark " + color;
                        }
                        string _test = Color.FromName(color).Name;
                        tt_Lbl.Pty.TooltipContent = Ressource.GetString(data + _test + "_TradeState", string.Empty);
                        tt_Lbl.Pty.TooltipTitle = data;
                    }
                    nbDspProcess++;
                }
            }
            dr.Close();
        }



        #endregion Methods
    }
}
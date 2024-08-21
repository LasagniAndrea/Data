using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Status;
using EFS.TradeInformation;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace EFS.Spheres
{
    /// <summary>
    /// EventHeaderBanner
    /// </summary>
    /// FI 20161124 [22634] Add Class
    public class EventHeaderBanner : HeaderBannerBase
    {
        #region Members
        /// <summary>
        /// Représente le\les évènements
        /// </summary>
        private readonly EventInput _input;
        /// <summary>
        /// Pilote la saisie des évènements
        /// </summary>
        private readonly EventInputGUI _inputGUI;
        #endregion

        /// <summary>
        ///  Ajoute dans {pControlContainer} de la page {pPage} le panel "divdescandstatus"
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pGUID"></param>
        /// <param name="pControlContainer"></param>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public EventHeaderBanner(PageBase pPage, string pGUID, Control pControlContainer,
            EventInput pEventInput, EventInputGUI pEventInputGUI, bool pIsStatusButtonVisible) :
            base(pPage, pGUID, pControlContainer, pIsStatusButtonVisible, pEventInputGUI.MainMenuClassName)
        {
            _input = pEventInput;
            _inputGUI = pEventInputGUI;

            NbDisplayProcess = 4;
        }

        /// <summary>
        /// Ajoute les contôle du banner
        /// </summary>
        public override void AddControls()
        {
            AddStatusPanel(Mode.Event);
            AddProcessPanel();
            AddDisplaynameDescriptionPanel();
        }

        /// <summary>
        ///  Ajoute les contôles web spécifiques aux données status ( libellé +  bouton)
        /// </summary>
        protected void AddStatus()
        {
            
        }

        /// <summary>
        /// Mise à jour des informations présentes dans l'entête
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        /// <param name="pIsRefresh"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public override void DisplayHeader(bool pIsReadOnly, bool pIsRefresh)
        {
            if (m_Page.FindControl("divdescandstatus") is Panel ctrl)
            {
                if (_input.IsEventFilled)
                    ControlsTools.RemoveStyleDisplay(ctrl);
                else
                    ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            DisplayIdentifierDisplayNameDescription();
            DisplayIdSys();
            DisplayStatus();
            DisplayProcessState(pIsRefresh);
        }


        /// <summary>
        ///  Affiche les information du trade
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        private void DisplayIdentifierDisplayNameDescription()
        {
            //Identifier
            if (m_Page.FindControl(ctrlTxtIdentifier) is TextBox txtIdentifier)
            {
                txtIdentifier.Text = _input.TradeIdentification.Identifier;
                txtIdentifier.ToolTip = txtIdentifier.Text;
                txtIdentifier.ReadOnly = true;
            }
            if (_input.IsEventFilled)
            {
                // DisplayName
                if (m_Page.FindControl(ctrlTxtDisplayName) is TextBox txtDisplayname)
                {
                    txtDisplayname.Text = _input.TradeIdentification.Displayname;
                    txtDisplayname.ToolTip = txtDisplayname.Text;
                    txtDisplayname.ReadOnly = true;
                }
                // Description
                if (m_Page.FindControl(ctrlTxtDescription) is TextBox txtDescription)
                {
                    txtDescription.Text = _input.TradeIdentification.Description;
                    txtDescription.ToolTip = txtDescription.Text;
                    txtDescription.ReadOnly = true;
                }
                // ExtLink
                if (m_Page.FindControl(ctrlTxtExtLink) is TextBox txtExtlLink)
                {
                    txtExtlLink.Text = _input.TradeIdentification.Extllink;
                    txtExtlLink.ToolTip = txtExtlLink.Text;
                    txtExtlLink.ReadOnly = true;
                    txtExtlLink.CssClass = EFSCssClass.GetCssClass(false, false, false, true);
                }
            }
        }
        /// <summary>
        ///  Affiche IdT du trade
        /// </summary>
        private void DisplayIdSys()
        {
            bool isDisplay = true;

            if (this._input.IsEventFilled)
            {
                if (m_Page.FindControl(ctrlLblIdSys) is Label lbl)
                {
                    if (isDisplay)
                        ControlsTools.RemoveStyleDisplay(lbl);
                    else
                        lbl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                if (m_Page.FindControl(ctrlDspIdSys) is Label dsp)
                {
                    if (isDisplay)
                        ControlsTools.RemoveStyleDisplay(dsp);
                    else
                        dsp.Style[HtmlTextWriterStyle.Display] = "none";
                    dsp.Text = _input.TradeIdentification.OtcmlId;
                }
            }
        }


        /// <summary>
        ///  Affiche les status de l'évènement
        /// </summary>
        /// <param name="pIsReadOnly"></param>
        /// EG 20200826 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        private void DisplayStatus()
        {
            if (this._input.IsEventFilled)
            {
                string CS = SessionTools.CS;
                // FI 20200820 [25468] dates systemes en UTC, Tooltip affichés selon le profil avec précison à la seconde
                AuditTimestampInfo auditTimestampInfo = new AuditTimestampInfo()
                {
                    Collaborator = SessionTools.Collaborator,
                    TimestampZone = SessionTools.AuditTimestampZone,
                    Precision = Cst.AuditTimestampPrecision.Second
                };

                    _input.CurrentEventStatus.stActivation.SetTooltip(CS, auditTimestampInfo);

                if (_input.CurrentEventStatus.stUsedBySpecified)
                    _input.CurrentEventStatus.stUsedBy.SetTooltip(CS, auditTimestampInfo);

                StatusEnum[] availableStatus = Status.StatusTools.GetAvailableStatus(Status.Mode.Event);
                foreach (StatusEnum item in availableStatus)
                {
                    if (Status.StatusTools.IsStatusSystem(item))
                    {
                        string ctrlId = StrFunc.AppendFormat("{0}{1}", "lbl", item.ToString().Replace("Status", "st"));
                        if (m_Page.FindControl(ctrlId) is WCTooltipLabel lbl)
                        {
                            switch (item)
                            {
                                case StatusEnum.StatusActivation:
                                    SetToHtmlControl(_input.CurrentEventStatus.stActivation, lbl);
                                    break;
                                case StatusEnum.StatusUsedBy:
                                    SetToHtmlControl(_input.CurrentEventStatus.stUsedBy, lbl);
                                    break;
                                default:
                                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", item.ToString()));
                            }
                        }
                    }
                    else if (Status.StatusTools.IsStatusUser(item))
                    {
                        if (m_Page.FindControl("lbl" + item.ToString()) is WCTooltipLabel lbl)
                            SetToHtmlControl(SessionTools.CS, _input.CurrentEventStatus.GetStUsersCollection(item).Status, lbl);
                    }
                    else
                    {
                        throw new NotImplementedException(StrFunc.AppendFormat("statut : {0} is not Implemented ", item.ToString()));
                    }
                }

                #region imgStatus
                if (m_Page.FindControl("imgStatus") is LinkButton imgStatus)
                {
                    string url = StrFunc.AppendFormat("Status.aspx?GUID={0}&F=CustomCapture&Mode={1}", m_GUID, Mode.Event.ToString());
                    url = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_Status);
                    //FI 20110415 [17402] Spheres effectue désormais une publication de la page pour mettre à jour le dataDocument avec le contenu des contrôles qui ne sont pas autoPostack
                    //Ceci est fait uniquement en mode saisie
                    if (Cst.Capture.IsModeConsult(_inputGUI.CaptureMode))
                    {
                        url += "return false;";
                    }
                    else
                    {
                        url += m_Page.ClientScript.GetPostBackEventReference(imgStatus, null) + ";return false;";
                    }
                    imgStatus.OnClientClick = url;
                }
                #endregion imgStatus
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private QueryParameters GetSelectState()
        {
            string CS = SessionTools.CS;
            string sqlSelect = string.Empty;

            // ProcessState : CONF
            sqlSelect += GetSelectState_Process(1, Cst.ProcessTypeEnum.CMGEN, "Conf.");
            // ProcessState : SIGEN
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(2, Cst.ProcessTypeEnum.MSOGEN, "Settlt.");
            // ProcessState : EAR
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(3, Cst.ProcessTypeEnum.EARGEN, "EAR");
            // ProcessState : ACCT
            sqlSelect += SQLCst.UNIONALL + Cst.CrLf;
            sqlSelect += GetSelectState_Process(4, Cst.ProcessTypeEnum.ACCOUNTGEN, "Acct.");

            sqlSelect += SQLCst.ORDERBY + "SORT1, SORT2" + Cst.CrLf;

            string fromDual = DataHelper.SQLFromDual(CS);
            if (StrFunc.IsFilled(fromDual))
                sqlSelect = sqlSelect.Replace(") tbl", fromDual + ") tbl");

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDE), _input.CurrentEvent.idE);

            QueryParameters ret = new QueryParameters(CS, sqlSelect, parameters);


            return ret;
        }


        /// <summary>
        ///  Affiche les process de l'événement
        /// </summary>
        /// <param name="pIsRefresh"></param>
        private void DisplayProcessState(bool pIsRefresh)
        {
            if (m_Page.FindControl(ctrlPnlProcess) is Panel pnl)
            {
                if (false == Cst.Capture.IsModeNewOrDuplicate(_inputGUI.CaptureMode))
                    ControlsTools.RemoveStyleDisplay(pnl);
                else
                    pnl.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if (m_Page.FindControl(ctrlLblProcess) is Label lbl)
            {
                if (false == Cst.Capture.IsModeNewOrDuplicate(_inputGUI.CaptureMode))
                    ControlsTools.RemoveStyleDisplay(lbl);
                else
                    lbl.Style[HtmlTextWriterStyle.Display] = "none";
            }

            if (!Cst.Capture.IsModeNewOrDuplicate(_inputGUI.CaptureMode) && pIsRefresh)
            {
                if (_input.IsEventFilled)
                {
                    QueryParameters qryParameters = GetSelectState();
                    SetDspProcess(qryParameters);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pOrderState"></param>
        /// <param name="pProcessType"></param>
        /// <param name="pLabel"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private string GetSelectState_Process(int pOrderState, Cst.ProcessTypeEnum pProcessType, string pLabel)
        {
            return $@"select distinct {pOrderState} as SORT1, SORT2, '{pLabel}' as CODE, COLOR
            from 
            (
                select 1 as SORT2, 'red' as COLOR
                from dbo.EVENTPROCESS ep 
                where (ep.IDE = @IDE) and (ep.PROCESS='{pProcessType}') and 
                      (ep.IDSTPROCESS!='SUCCESS') and ep.DTSTPROCESS in (select MAX(ep2.DTSTPROCESS) from dbo.EVENTPROCESS ep2 where (ep2.IDE = ep.IDE))
                union all
                select 2 as SORT2, 'blue' as COLOR
                from dbo.EVENTPROCESS ep 
                where (ep.IDE = @IDE) and (ep.PROCESS='{pProcessType}') and (ep.IDSTPROCESS='SUCCESS')
                union all
                select 99 as SORT2, 'black' as COLOR
            ) tbl";
        }

    }
}
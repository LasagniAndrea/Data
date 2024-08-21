using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.TrackerControl;

namespace EFS.Spheres
{
    public partial class Tracker : TrackerBase
    {
        #region Members
        #endregion Members

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);

            PageMasterTools.SetHeaderTitle(this, "titlePage", "Tracker");

            rb_readyState.Text = Ressource.GetString("trkDisplayReadyState");
            rb_groupTracker.Text = Ressource.GetString("trkDisplayGroupTracker");
            rb_statusTracker.Text = Ressource.GetString("trkDisplayStatusTracker");

            rb_readyState.LabelAttributes.Add("onClick", AddPostBackReference(rb_readyState));
            rb_groupTracker.LabelAttributes.Add("onClick", AddPostBackReference(rb_groupTracker));
            rb_statusTracker.LabelAttributes.Add("onClick", AddPostBackReference(rb_statusTracker));

            TrackerParamLoad();

            if (m_IsLoadSession)
            {
                btnRefresh.Attributes.Add("onclick", "__doPostBack('0','SELFRELOAD_');");
                btnAutorefresh.Attributes.Add("onclick", "__doPostBack('0','TIMER');");

                TrackerParamSetting();

                if (rb_readyState.Checked)
                    DisplayValuesByReadyState();
                else if (rb_groupTracker.Checked)
                    DisplayValuesByGroup();
                else if (rb_statusTracker.Checked)
                    DisplayValuesByStatus();

                int time = m_TrackerRefreshInterval * 1000;
                timerRefresh.Enabled = (time > 0);
                if (timerRefresh.Enabled)
                    timerRefresh.Interval = time;

            }

            SetAttributesTimerRefresh();
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            base.OnPreRenderComplete(e);
            SetTrackerDisplayCheckValue();
        }

        #region OnAutoRefresh
        /// <summary>
        /// Evénement déclenché pour un raffraichissement automatique de la page (Mode: Enabled/Disabled)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnAutoRefresh(object sender, EventArgs e)
        {
            timerRefresh.Enabled = !timerRefresh.Enabled;
            SetAttributesTimerRefresh();
        }
        #endregion OnAutoRefresh
        #region SetAttributesTimerRefresh
        /// <summary>
        /// Changement de l'image de raffraichissement automatique du tracker en fonction de son statut
        /// </summary>
        protected void SetAttributesTimerRefresh()
        {
            btnAutorefresh.Attributes["class"] = "btn btn-xs " + (timerRefresh.Enabled ? "btn-remove" : "btn-apply");
        }
        #endregion SetAttributesTimerRefresh

        #region TrackerParamLoad
        private void TrackerParamLoad()
        {
            chkTrackerAlert.Text = string.Empty;
            lblTrackerHistoric.Text = Ressource.GetString("TrackerHistoric");

            chkHistoToDay.Text = Ressource.GetString("TrackerToDay");
            chkHisto1D.Text = Ressource.GetString("Tracker1D");
            chkHisto7D.Text = Ressource.GetString("Tracker7D");
            chkHisto1M.Text = Ressource.GetString("Tracker1M");
            chkHisto3M.Text = Ressource.GetString("Tracker3M");
            chkHistoBeyond.Text = Ressource.GetString("TrackerBeyond");
            trkDisplayMode.Text = Ressource.GetString("trkDisplayMode");
            rb_readyStateParam.Text = Ressource.GetString("trkDisplayReadyState");
            rb_groupTrackerParam.Text = Ressource.GetString("trkDisplayGroupTracker");
            rb_statusTrackerParam.Text = Ressource.GetString("trkDisplayStatusTracker");

            chkSelectAll2.Attributes.Add("onclick", "javaScript:AllTrackerCheckChange(this);");
            chkSelectAll2.Text = Ressource.GetString("chkSelectAll");


            // Etats du tracker Step : 1
            plhReadyState1.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.ReadyStateTracker,
                ProcessStateTools.ReadyStateEnum.ACTIVE, rb_readyStateParam.Text, "1/3"));
            // Etats du tracker Step : 2
            plhReadyState2.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.ReadyStateTracker,
                ProcessStateTools.ReadyStateEnum.REQUESTED, rb_readyStateParam.Text, "2/3"));

            // Etats du tracker Step : 3
            plhReadyState3.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.ReadyStateTracker,
                ProcessStateTools.ReadyStateEnum.TERMINATED, rb_readyStateParam.Text, "3/3"));

            // Groupes de tracker Step : 1
            plhGroupTracker1.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.GroupTracker,
                Cst.GroupTrackerEnum.TRD | Cst.GroupTrackerEnum.IO | Cst.GroupTrackerEnum.MSG, rb_groupTrackerParam.Text, "1/3"));

            // Groupes de tracker Step : 2
            plhGroupTracker2.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.GroupTracker,
                 Cst.GroupTrackerEnum.ACC | Cst.GroupTrackerEnum.CLO | Cst.GroupTrackerEnum.INV, rb_groupTrackerParam.Text, "2/3"));

            // Groupes de tracker Step : 3
            plhGroupTracker3.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.GroupTracker, Cst.GroupTrackerEnum.EXT, rb_groupTrackerParam.Text, "3/3"));

            // Status de tracker Step : 1
            plhStatusTracker1.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.StatusTracker,
                ProcessStateTools.StatusEnum.ERROR | ProcessStateTools.StatusEnum.WARNING, rb_statusTrackerParam.Text, "1/4"));

            // Status de tracker Step : 2
            plhStatusTracker2.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.StatusTracker,
                ProcessStateTools.StatusEnum.SUCCESS | ProcessStateTools.StatusEnum.PENDING, rb_statusTrackerParam.Text, "2/4"));

            // Status de tracker Step : 3
            plhStatusTracker3.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.StatusTracker,
                ProcessStateTools.StatusEnum.NA | ProcessStateTools.StatusEnum.NONE, rb_statusTrackerParam.Text, "3/4"));

            // Status de tracker Step : 4
            plhStatusTracker4.Controls.Add(MainCheckListControl(TrackerTools.CheckListTypeEnum.StatusTracker,
                ProcessStateTools.StatusEnum.PROGRESS, rb_statusTrackerParam.Text, "3/4"));

            SetHelpControl();

        }
        #endregion TrackerParamLoad
        #region MainCheckListControl
        private MainCheckList MainCheckListControl(TrackerTools.CheckListTypeEnum pCheckListType, Enum pEnumFlags, string pTitle, string pSubTitle)
        {
            MainCheckList uc = LoadControl("~/UserControl/MainCheckList.ascx") as MainCheckList;
            uc.Initialize(pCheckListType, pEnumFlags, pTitle, pSubTitle);
            return uc;
        }
        #endregion MainCheckListControl
        #region TrackerParamSetting
        private void TrackerParamSetting()
        {
            chkHistoToDay.Checked = ("0D" == m_TrackerHisToric);
            chkHisto1D.Checked = ("1D" == m_TrackerHisToric);
            chkHisto7D.Checked = ("7D" == m_TrackerHisToric);
            chkHisto1M.Checked = ("1M" == m_TrackerHisToric);
            chkHisto3M.Checked = ("3M" == m_TrackerHisToric);
            chkHistoBeyond.Checked = ("Beyond" == m_TrackerHisToric);

            rb_readyStateParam.Checked = ("ReadyState" == m_TrackerDisplayMode);
            rb_groupTrackerParam.Checked = ("Group" == m_TrackerDisplayMode);
            rb_statusTrackerParam.Checked = ("Status" == m_TrackerDisplayMode);

            txtTrackerNbRowPerGroup.Text = m_TrackerNbRowPerGroup.ToString();
            txtTrackerRefreshInterval.Text = m_TrackerRefreshInterval.ToString();

            rb_readyState.Checked = ("ReadyState" == m_TrackerDisplayMode);
            rb_groupTracker.Checked = ("Group" == m_TrackerDisplayMode);
            rb_statusTracker.Checked = ("Status" == m_TrackerDisplayMode);


        }
        #endregion TrackerParamSetting

        //#region CreateControlByReadyState
        //protected void CreateControlByReadyState()
        //{
        //    Nullable<ProcessStateTools.StatusEnum> statusReadyState = null;
        //    foreach (ProcessStateTools.ReadyStateEnum readyState in m_LstTrackerByReadyState.Keys)
        //    {
        //        int total = m_LstTrackerByReadyState[readyState].First;
        //        TrackerMain trkMain = SetTrackerMainHeader(readyState, total);
        //        if (null != trkMain)
        //        {
        //            plhTrackerContent.Controls.Add(trkMain);

        //            #region Lecture des READYSTATE présents dans le GROUPE en cours
        //            m_LstTrackerByReadyState[readyState].Second.ForEach(item =>
        //            {
        //                statusReadyState = SetTrackerItemHeader(item, trkMain);
        //            });

        //            if (statusReadyState.HasValue)
        //            {
        //                Label lblMainCounter = trkMain.FindControl("Counter") as Label;
        //                if (null != lblMainCounter)
        //                    lblMainCounter.CssClass = SetCssBadge(statusReadyState);
        //            }
        //            #endregion Lecture des groupes
        //        }
        //        //}
        //    }
        //}
        //#endregion CreateControlByReadyState
        //#region CreateControlByGroup
        //protected void CreateControlByGroup()
        //{
        //    Nullable<ProcessStateTools.StatusEnum> statusGroup = null;
        //    foreach (Cst.GroupTrackerEnum group in m_LstTrackerByGroup.Keys)
        //    {
        //        //if (Cst.GroupTrackerEnum.ALL != group)
        //        //{
        //        int total = m_LstTrackerByGroup[group].First;
        //        TrackerMain trkMain = SetTrackerMainHeader(group, total);
        //        if (null != trkMain)
        //        {
        //            plhTrackerContent.Controls.Add(trkMain);

        //            #region Lecture des READYSTATE présents dans le GROUPE en cours
        //            m_LstTrackerByGroup[group].Second.ForEach(item =>
        //            {
        //                //if (ProcessStateTools.ReadyStateEnum.ALL != item.readyState)
        //                statusGroup = SetTrackerItemHeader(item, trkMain);
        //            });

        //            if (statusGroup.HasValue)
        //            {
        //                Label lblMainCounter = trkMain.FindControl("Counter") as Label;
        //                if (null != lblMainCounter)
        //                    lblMainCounter.CssClass = SetCssBadge(statusGroup);
        //            }
        //            #endregion Lecture des groupes
        //        }
        //        //}
        //    }
        //}
        //#endregion CreateControlByGroup

        #region SetTrackerMainItemCounter
        private void SetTrackerMainCounter(TrackerMain pTrackerMain)
        {
            Enum mainEnum = pTrackerMain.mainEnum;
            PlaceHolder plhCounter = pTrackerMain.FindControl("plhTrackerMainCounter") as PlaceHolder;
            if (null != plhCounter)
            {
                List<Pair<Enum, int>> total = new List<Pair<Enum, int>>();

                if (mainEnum is ProcessStateTools.ReadyStateEnum)
                {
                    List<ReadyStateTrackerCounter> lstTrackerCounter = m_LstTrackerByReadyState[(ProcessStateTools.ReadyStateEnum)mainEnum].Second;
                    total = CalculationTrackerMainCounter(lstTrackerCounter);
                }
                else if (mainEnum is Cst.GroupTrackerEnum)
                {
                    List<GroupTrackerCounter> lstTrackerCounter = m_LstTrackerByGroup[(Cst.GroupTrackerEnum)mainEnum].Second;
                    total = CalculationTrackerMainCounter(lstTrackerCounter);
                }
                else if (mainEnum is ProcessStateTools.StatusEnum)
                {
                    List<StatusTrackerCounter> lstTrackerCounter = m_LstTrackerByStatus[(ProcessStateTools.StatusEnum)mainEnum].Second;
                    total = CalculationTrackerMainCounter(lstTrackerCounter);
                }

                if (null != total)
                {
                    total.ForEach(item =>
                    {
                        // Création du compteur
                        HtmlGenericControl div = new HtmlGenericControl("div");
                        HtmlGenericControl badge = new HtmlGenericControl("span");
                        badge.InnerText = item.Second.ToString();

                        //if (mainEnum is ProcessStateTools.StatusEnum)
                        //{
                        //    HtmlGenericControl div = new HtmlGenericControl("div");
                        //    div.Attributes.Add("class", "counter " + mainEnum.ToString());
                        //    div.InnerText = item.First.ToString();
                        //    div.Controls.Add(lbl);
                        //    plhCounter.Controls.Add(div);
                        //    lbl.CssClass = SetCssReverseBadge((ProcessStateTools.StatusEnum)mainEnum);
                        //}
                        //else
                        //{
                        //    lbl.CssClass = SetCssReverseBadge((ProcessStateTools.StatusEnum)item.First);
                        //    plhCounter.Controls.Add(lbl);
                        //}

                        if (mainEnum is ProcessStateTools.StatusEnum)
                        {
                            div.Attributes.Add("class", SetCssReverseBadge((ProcessStateTools.StatusEnum)mainEnum));
                            HtmlGenericControl label = new HtmlGenericControl("span");
                            label.InnerText = item.First.ToString();
                            div.Controls.Add(label);
                        }
                        else
                        {
                            div.Attributes.Add("class", SetCssReverseBadge((ProcessStateTools.StatusEnum)item.First));
                        }
                        div.Controls.Add(badge);
                        plhCounter.Controls.Add(div);

                    });
                }
            }
        }
        private List<Pair<Enum, int>> CalculationTrackerMainCounter<T>(List<T> pTrackerCounter) where T : TrackerCounter
        {
            List<Pair<Enum, int>> total = new List<Pair<Enum, int>>();
            pTrackerCounter.ForEach(subItem =>
            {
                subItem.child.ForEach(child =>
                {
                    if (0 < child.Second)
                    {
                        if (false == total.Exists(item => item.First.ToString() == child.First.ToString()))
                            total.Add(new Pair<Enum, int>(child.First, 0));
                        total.Find(item => item.First.ToString() == child.First.ToString()).Second += child.Second;
                    }
                });
            });
            return total;
        }
        #endregion SetTrackerMainCounter
        #region SetTrackerItemCounter
        private void SetTrackerItemCounter(Enum pMainEnum, TrackerItem pTrackerItem, TrackerCounter pTrackerCounter)
        {
            List<Pair<Enum, int>> lstChild = pTrackerCounter.child;
            PlaceHolder plhCounter = pTrackerItem.FindControl("plhTrackerItemCounter") as PlaceHolder;
            if (null != plhCounter)
            {
                lstChild.ForEach(item =>
                {
                    if (0 < item.Second)
                    {

                        // Création du compteur
                        HtmlGenericControl div = new HtmlGenericControl("div");
                        HtmlGenericControl badge = new HtmlGenericControl("span");
                        badge.InnerText = item.Second.ToString();

                        if (pMainEnum is ProcessStateTools.StatusEnum)
                        {
                            div.Attributes.Add("class", SetCssBadge((ProcessStateTools.StatusEnum)pMainEnum));
                            HtmlGenericControl label = new HtmlGenericControl("span");
                            label.InnerText = item.First.ToString();
                            div.Controls.Add(label);
                        }
                        else
                        {
                            div.Attributes.Add("class", SetCssBadge((ProcessStateTools.StatusEnum)item.First));
                        }
                        div.Controls.Add(badge);
                        plhCounter.Controls.Add(div);
                    }
                });
            }
        }
        #endregion SetTrackerItemCounter
        #region SetTrackerItemHeader
        /// <summary>
        /// Alimentation des en-tête du tracker ITEM (READSTATE ou GROUP)
        /// </summary>
        /// <typeparam name="T">type attendu GroupTracker|ReadyStateTracker</typeparam>
        /// <param name="pItem">pItem de type GroupTracker|ReadyStateTracker</param>
        /// <param name="pTrkMain">UserControl TrackerMain</param>
        /// <returns></returns>
        private void SetTrackerItemHeader(TrackerCounter pTrackerCounter, TrackerMain pTrkMain)
        {
            List<Pair<Enum, int>> lstChild = null;

            TrackerItem trackerItem = LoadControl("~/UserControl/TrackerItem.ascx") as TrackerItem;
            string title = string.Empty;
            if (null != trackerItem)
            {
                trackerItem.ID = pTrackerCounter.key.ToString();
                lstChild = pTrackerCounter.child;

                PlaceHolder plhTrkMain = pTrkMain.FindControl("trkMain") as PlaceHolder;
                if (null != plhTrkMain)
                {
                    plhTrkMain.Controls.Add(trackerItem);

                    if (pTrackerCounter is GroupTrackerCounter)
                    {
                        title = m_ReadyStateName[((GroupTrackerCounter)pTrackerCounter).keyReadyState].First;
                    }
                    else if (pTrackerCounter is ReadyStateTrackerCounter)
                    {
                        title = m_GroupTrackerName[((ReadyStateTrackerCounter)pTrackerCounter).keyGroup];
                    }
                    else if (pTrackerCounter is StatusTrackerCounter)
                    {
                        title = m_GroupTrackerName[((StatusTrackerCounter)pTrackerCounter).keyGroup];
                    }
                    // <div id="itemCollapse" runat="server" class="panel-collapse collapse" role="tabpanel" aria-labelledby="itemHeading">
                    HtmlGenericControl div = trackerItem.FindControl("Collapse") as HtmlGenericControl;
                    if (null != div)
                    {
                        // <div runat="server" class="panel-heading" role="tab" id="itemHeading">
                        HtmlGenericControl divHeading = trackerItem.FindControl("Heading") as HtmlGenericControl;
                        if (null != divHeading)
                            div.Attributes["aria-labelledby"] = divHeading.ClientID;
                    }

                    // <a runat="server" class="accordion-toggle" data-toggle="collapse" data-parent="#accordion" href="#itemCollapse" id="itemTitle"  aria-expanded="false" aria-controls="collapse" />
                    HyperLink lnk = trackerItem.FindControl("Title") as HyperLink;
                    if (null != lnk)
                    {
                        lnk.Attributes["href"] = "#" + div.ClientID;
                        lnk.Text = title;
                    }
                    SetTrackerItemCounter(pTrkMain.mainEnum, trackerItem, pTrackerCounter);
                }
            }
        }
        #endregion SetTrackerItemHeader
        #region SetTrackerMainHeader
        /// <summary>
        /// Alimentation des en-tête du tracker MAIN (READSTATE|GROUP|STATUS)
        /// </summary>
        /// <typeparam name="T">type attendu GroupTrackerEnum|ProcessStateTools.ReadyStateEnum|ProcessStateTools.StatusEnum</typeparam>
        /// <param name="pItem">de type GroupTrackerEnum|ProcessStateTools.ReadyStateEnum|ProcessStateTools.StatusEnum</param>
        /// <param name="pTotal">Total ligne Tracker pour pItem</param>
        /// <returns></returns>
        private TrackerMain SetTrackerMainHeader(Enum pMainEnum, int pTotal)
        {
            TrackerMain trkMain = LoadControl("~/UserControl/TrackerMain.ascx") as TrackerMain;
            trkMain.mainEnum = pMainEnum;
            trkMain.ID = "trk_" + pMainEnum.ToString();
            Label lbl = trkMain.FindControl("Title") as Label;
            if (null != lbl)
            {
                if (pMainEnum is Cst.GroupTrackerEnum)
                {
                    lbl.Text = m_GroupTrackerName[(Cst.GroupTrackerEnum)pMainEnum];
                }
                else if (pMainEnum is ProcessStateTools.ReadyStateEnum)
                {
                    lbl.Text = m_ReadyStateName[(ProcessStateTools.ReadyStateEnum)pMainEnum].First;
                }
                else if (pMainEnum is ProcessStateTools.StatusEnum)
                {
                    lbl.Text = m_StatusName[(ProcessStateTools.StatusEnum)pMainEnum].First;
                }
            }
            return trkMain;
        }
        #endregion SetTrackerMainHeader
        protected void Refresh(object sender, EventArgs e)
        {
            //DisplayByReadyState(sender, e);
        }

        protected void DisplayTracker(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (null != rb)
            {
                switch (rb.ID)
                {
                    case "rb_readyState":
                        DisplayValuesByReadyState();
                        break;
                    case "rb_groupTracker":
                        DisplayValuesByGroup();
                        break;
                    case "rb_statusTracker":
                        DisplayValuesByStatus();
                        break;
                }
            }
        }

        protected void DisplayValuesByReadyState()
        {
            List<TrackerTools.TrackerFlags> trackerFlags = m_TrackerDisplayValues[TrackerTools.CheckListTypeEnum.ReadyStateTracker];
            if (null != trackerFlags)
            {
                trackerFlags.ForEach(item =>
                {
                    ProcessStateTools.ReadyStateEnum readyState = (ProcessStateTools.ReadyStateEnum)item.values.First;
                    int total = m_LstTrackerByReadyState[readyState].First;
                    TrackerMain trkMain = SetTrackerMainHeader(readyState, total);
                    if (null != trkMain)
                    {
                        plhTrackerContent.Controls.Add(trkMain);

                        // Liste des GROUPES sélectionnés
                        Cst.GroupTrackerEnum group = (Cst.GroupTrackerEnum)item.values.Second;
                        if (group != Cst.GroupTrackerEnum.ALL)
                        {

                            // Lecture des GROUPES présents dans le READYSTATE en cours
                            m_LstTrackerByReadyState[readyState].Second.ForEach(subItem =>
                            {
                                if (group.HasFlag(subItem.key))
                                {
                                    // Le groupe est dans la sélection
                                    SetTrackerItemHeader(subItem, trkMain);

                                }
                            });
                        }

                        // Compteurs principaux TODO
                        SetTrackerMainCounter(trkMain);
                    }
                });
            }
        }
        protected void DisplayValuesByGroup()
        {
            List<TrackerTools.TrackerFlags> trackerFlags = m_TrackerDisplayValues[TrackerTools.CheckListTypeEnum.GroupTracker];
            if (null != trackerFlags)
            {
                trackerFlags.ForEach(item =>
                {
                    Cst.GroupTrackerEnum group = (Cst.GroupTrackerEnum)item.values.First;
                    if (group != Cst.GroupTrackerEnum.ALL)
                    {
                        int total = m_LstTrackerByGroup[group].First;
                        TrackerMain trkMain = SetTrackerMainHeader(group, total);
                        if (null != trkMain)
                        {
                            plhTrackerContent.Controls.Add(trkMain);

                            // Liste des READYSTATE sélectionnés
                            ProcessStateTools.ReadyStateEnum readyState = (ProcessStateTools.ReadyStateEnum)item.values.Second;

                            // Lecture des READYSTATE présents dans le GROUPE en cours
                            m_LstTrackerByGroup[group].Second.ForEach(subItem =>
                            {
                                if (readyState.HasFlag(subItem.key))
                                {
                                    // Le groupe est dans la sélection
                                    SetTrackerItemHeader(subItem, trkMain);
                                }
                            });

                            // Compteurs principaux TODO
                            SetTrackerMainCounter(trkMain);
                        }
                    }
                });
            }
        }
        protected void DisplayValuesByStatus()
        {
            List<TrackerTools.TrackerFlags> trackerFlags = m_TrackerDisplayValues[TrackerTools.CheckListTypeEnum.StatusTracker];
            if (null != trackerFlags)
            {
                trackerFlags.ForEach(item =>
                {
                    ProcessStateTools.StatusEnum status = (ProcessStateTools.StatusEnum)item.values.First;
                    int total = m_LstTrackerByStatus[status].First;
                    TrackerMain trkMain = SetTrackerMainHeader(status, total);
                    if (null != trkMain)
                    {
                        plhTrackerContent.Controls.Add(trkMain);

                        // Liste des GROUP sélectionnés
                        Cst.GroupTrackerEnum group = (Cst.GroupTrackerEnum)item.values.Second;
                        if (group != Cst.GroupTrackerEnum.ALL)
                        {

                            // Lecture des GROUPES présents dans le STATUS en cours
                            m_LstTrackerByStatus[status].Second.ForEach(subItem =>
                            {
                                if (group.HasFlag(subItem.key))
                                {
                                    // Le groupe est dans la sélection
                                    SetTrackerItemHeader(subItem, trkMain);
                                }
                            });
                        }
                        // Compteurs principaux TODO
                        SetTrackerMainCounter(trkMain);
                    }
                });
            }
        }

        #region SetHelpControl
        /// <summary>
        /// Chargement de l'aide en ligne (GROUP / READYSTATE / STATUS)
        /// </summary>
        protected void SetHelpControl()
        {
            // ReadyState
            plhTrackerHelp1.Controls.Add(SetHelpControl(TrackerTools.CheckListTypeEnum.ReadyStateTracker));
            plhTrackerHelp1.Controls.Add(SetHelpControl(TrackerTools.CheckListTypeEnum.GroupTracker));
            plhTrackerHelp2.Controls.Add(SetHelpControl(TrackerTools.CheckListTypeEnum.StatusTracker));
        }
        private Panel SetHelpControl(TrackerTools.CheckListTypeEnum pCheckListType)
        {
            Panel pnl = new Panel();
            pnl.CssClass = "trkhelp " + pCheckListType.ToString().Replace("Tracker", string.Empty).ToLower();
            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            HtmlGenericControl preamble = new HtmlGenericControl("blockquote");
            preamble.Attributes.Add("class", "blockquote");
            string title = string.Empty;
            string paragraph = string.Empty;

            Panel subPnl = new Panel();
            switch (pCheckListType)
            {
                case TrackerTools.CheckListTypeEnum.ReadyStateTracker:
                    h3.InnerText = Ressource.GetString("trkDisplayReadyState");
                    preamble.InnerHtml = Ressource.GetString("HELP_READYSTATE", string.Empty);
                    foreach (ProcessStateTools.ReadyStateEnum readyState in Enum.GetValues(typeof(ProcessStateTools.ReadyStateEnum)))
                    {
                        subPnl.Controls.Add(SetHelpControl(readyState, "READYSTATE", m_ReadyStateName[readyState].First));
                    }
                    break;
                case TrackerTools.CheckListTypeEnum.GroupTracker:
                    h3.InnerText = Ressource.GetString("trkDisplayGroupTracker");
                    preamble.InnerHtml = Ressource.GetString("HELP_GROUP", string.Empty);
                    foreach (Cst.GroupTrackerEnum group in Enum.GetValues(typeof(Cst.GroupTrackerEnum)))
                    {
                        subPnl.Controls.Add(SetHelpControl(group, "GROUP", m_GroupTrackerName[group]));
                    }
                    break;
                case TrackerTools.CheckListTypeEnum.StatusTracker:
                    h3.InnerText = Ressource.GetString("trkDisplayStatusTracker");
                    preamble.InnerHtml = Ressource.GetString("HELP_STATUS", string.Empty);
                    Panel pnlStatus = null;
                    foreach (ProcessStateTools.StatusEnum status in Enum.GetValues(typeof(ProcessStateTools.StatusEnum)))
                    {
                        pnlStatus = SetHelpControl(status, "STATUS", m_StatusName[status].First);
                        if (status == ProcessStateTools.StatusErrorEnum)
                            subPnl.Controls.AddAt(0, pnlStatus);
                        else if (status == ProcessStateTools.StatusWarningEnum)
                            subPnl.Controls.AddAt(1, pnlStatus);
                        else
                            subPnl.Controls.Add(pnlStatus);
                    }
                    break;
            }
            pnl.Controls.AddAt(0, preamble);
            pnl.Controls.AddAt(0, h3);
            pnl.Controls.Add(subPnl);
            return pnl;
        }
        private Panel SetHelpControl(Enum pValue, string pType, string pTitle)
        {
            Panel pnl = new Panel();
            HtmlGenericControl title = new HtmlGenericControl("p");
            title.Attributes.Add("class", pValue.ToString());
            title.InnerText = pTitle;
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Attributes.Add("class", "badge");
            span.InnerText = pValue.ToString();
            title.Controls.Add(span);
            pnl.Controls.Add(title);
            HtmlGenericControl paragraph = new HtmlGenericControl("p");
            paragraph.InnerHtml = Ressource.GetString(String.Format("HELP_{0}_{1}", pType, pValue.ToString()), string.Empty);
            pnl.Controls.Add(paragraph);
            return pnl;

        }
        #endregion SetHelpControl


        /* ---------------------------------------------------------------- */
        /* HERE TRACKER PARAM CODE                                          */
        /* ---------------------------------------------------------------- */
        /// <summary>
        /// Validation des modifications des paramètres utilisateurs du tracker et fermeture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region OnValidation
        protected void OnValidation(object sender, EventArgs e)
        {
            SessionTools.TrackerRefreshInterval = StrFunc.IsFilled(txtTrackerRefreshInterval.Text) ? Convert.ToInt32(txtTrackerRefreshInterval.Text) : 0;
            SessionTools.TrackerNbRowPerGroup = StrFunc.IsFilled(txtTrackerNbRowPerGroup.Text) ? Convert.ToInt32(txtTrackerNbRowPerGroup.Text) : 20;
            SessionTools.IsTrackerAlert = chkTrackerAlert.Checked;

            if (chkHistoToDay.Checked)
                SessionTools.TrackerHistoric = "0D";
            else if (chkHisto1D.Checked)
                SessionTools.TrackerHistoric = "1D";
            else if (chkHisto7D.Checked)
                SessionTools.TrackerHistoric = "7D";
            else if (chkHisto1M.Checked)
                SessionTools.TrackerHistoric = "1M";
            else if (chkHisto3M.Checked)
                SessionTools.TrackerHistoric = "3M";
            else if (chkHistoBeyond.Checked)
                SessionTools.TrackerHistoric = "Beyond";

            if (rb_readyStateParam.Checked)
                SessionTools.TrackerDisplayMode = "ReadyState";
            else if (rb_groupTrackerParam.Checked)
                SessionTools.TrackerDisplayMode = "Group";
            else if (rb_statusTrackerParam.Checked)
                SessionTools.TrackerDisplayMode = "Status";

            SaveTrackerDisplayCheckValue();

        }
        #endregion OnValidation

        #region SaveTrackerDisplayCheckValue
        private void SaveTrackerDisplayCheckValue()
        {
            m_TrackerDisplayValues = new Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>>();
            // Lecture des placeHolder de Tracker.aspx
            foreach (PlaceHolder plh in TrackerTools.GetControlsOfType<PlaceHolder>(this))
            {
                // Lecture des contrôles utilisateurs de type <MainCheckList> pour chaque placeHolder trouvé 
                TrackerTools.TrackerFlags trackerFlags = null;
                TrackerTools.TrackerFlags childTrackerFlags = null;
                foreach (MainCheckList mainCheckList in TrackerTools.GetControlsOfType<MainCheckList>(plh))
                {
                    // Lecture des contrôles utilisateurs de type <ChildCheckList> pour chaque controle utilisateur <MainCheckList> trouvé 
                    foreach (ChildCheckList childCheckList in TrackerTools.GetControlsOfType<ChildCheckList>(mainCheckList))
                    {

                        if (childCheckList.IsSelected)
                        {
                            foreach (SubChildCheckList subChildCheckList in TrackerTools.GetControlsOfType<SubChildCheckList>(childCheckList))
                            {
                                if (subChildCheckList.IsSelected)
                                {
                                    List<TrackerTools.TrackerFlags> lstTrackerFlags;

                                    trackerFlags = new TrackerTools.TrackerFlags();
                                    trackerFlags.values = new Pair<Enum, Enum>();
                                    trackerFlags.values.First = childCheckList.Main;
                                    trackerFlags.values.Second = childCheckList.Flags;

                                    // Dictionnaire (Key = TrackerTools.CheckListTypeEnum)
                                    if (false == m_TrackerDisplayValues.ContainsKey(mainCheckList.CheckListType))
                                    {
                                        lstTrackerFlags = new List<TrackerTools.TrackerFlags>();
                                        lstTrackerFlags.Add(trackerFlags);
                                        m_TrackerDisplayValues.Add(mainCheckList.CheckListType, lstTrackerFlags);
                                    }
                                    // Liste 
                                    lstTrackerFlags = m_TrackerDisplayValues[mainCheckList.CheckListType];
                                    if (false == lstTrackerFlags.Exists(match => match.values.First == childCheckList.Main))
                                    {
                                        lstTrackerFlags.Add(trackerFlags);
                                    }
                                    trackerFlags = lstTrackerFlags.Find(match => match.values.First == childCheckList.Main);
                                    if (null != trackerFlags)
                                    {
                                        if (null == trackerFlags.child)
                                        {
                                            trackerFlags.child = new List<TrackerTools.TrackerFlags>();
                                        }

                                        childTrackerFlags = new TrackerTools.TrackerFlags();
                                        childTrackerFlags.values = new Pair<Enum, Enum>();
                                        childTrackerFlags.values.First = subChildCheckList.Main;
                                        childTrackerFlags.values.Second = subChildCheckList.Flags;
                                        trackerFlags.child.Add(childTrackerFlags);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            SessionTools.TrackerDisplayValues = m_TrackerDisplayValues;

#if DEBUG

            TrackerDisplayToDiagnostics();
#endif

        }
        #endregion SaveTrackerDisplayCheckValue

        #region TrackerDisplayToDiagnostics
        private void TrackerDisplayToDiagnostics()
        {
            foreach (TrackerTools.CheckListTypeEnum key in m_TrackerDisplayValues.Keys)
            {
                System.Diagnostics.Debug.WriteLine("***********************************************");
                System.Diagnostics.Debug.WriteLine(key.ToString());
                System.Diagnostics.Debug.WriteLine("***********************************************");
                List<TrackerTools.TrackerFlags> lst = m_TrackerDisplayValues[key];
                lst.ForEach(item =>
                {
                    System.Diagnostics.Debug.WriteLine(item.values.First.ToString() + "=" + item.values.Second.ToString());
                    System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
                    if ((null != item.child))
                    {
                        item.child.ForEach(child =>
                        {
                            System.Diagnostics.Debug.WriteLine(child.values.First.ToString() + "=" + child.values.Second.ToString());
                            if ((null != child.child))
                            {
                                System.Diagnostics.Debug.WriteLine(" > ");
                                child.child.ForEach(subChild =>
                                {
                                    System.Diagnostics.Debug.WriteLine(subChild.values.First.ToString() + "=" + subChild.values.Second.ToString());

                                });
                            }
                            System.Diagnostics.Debug.WriteLine("--------------------------------------------------");

                        });

                    }
                });
            }
        }
        #endregion TrackerDisplayToDiagnostics

        #region SetTrackerDisplayCheckValue
        /// <summary>
        /// Mise à jour des contrôles UI en fonction des valeurs d'enum lues (READYSTATE|GROUPTRZACKER|STATUS)
        /// </summary>
        private void SetTrackerDisplayCheckValue()
        {
            if (null != m_TrackerDisplayValues)
            {
                foreach (PlaceHolder plh in TrackerTools.GetControlsOfType<PlaceHolder>(this))
                {
                    foreach (MainCheckList mainCheckList in TrackerTools.GetControlsOfType<MainCheckList>(plh))
                    {
                        List<TrackerTools.TrackerFlags> lstTrackerFlags = null;
                        if (m_TrackerDisplayValues.ContainsKey(mainCheckList.CheckListType))
                            lstTrackerFlags = m_TrackerDisplayValues[mainCheckList.CheckListType];
                        if (null != lstTrackerFlags)
                        {
                            foreach (ChildCheckList childCheckList in TrackerTools.GetControlsOfType<ChildCheckList>(mainCheckList))
                            {
                                childCheckList.SetCheckedValue(lstTrackerFlags);
                            }
                        }
                    }
                }
            }
        }
        #endregion SetTrackerDisplayCheckValue
    }
}
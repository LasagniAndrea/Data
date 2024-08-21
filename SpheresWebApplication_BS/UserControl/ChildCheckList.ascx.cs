using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.TrackerControl
{
    public partial class ChildCheckList : UserControl
    {
        #region Members
        private Enum m_Main;
        private TrackerTools.CheckListKey m_Child;
        private string m_Icon;
        #endregion Members

        #region Accessors
        public Enum Main { get { return m_Main; } }
        public TrackerTools.CheckListKey Child { get { return m_Child; } }
        public bool IsSelected { get { return mainCheck.Checked; } }
        public bool Checked { set { mainCheck.Checked = value; } }


        public Enum Flags
        {
            get
            {
                Enum flags = null;
                if (0 < m_Child.child.Count)
                {
                    Enum childEnumKey = m_Child.child.First().enumKey;
                    if (childEnumKey is Cst.GroupTrackerEnum)
                    {
                        flags = SetGroupFlags;
                    }
                    else if (childEnumKey is ProcessStateTools.ReadyStateEnum)
                    {
                        flags = SetReadyStateFlags;
                    }
                    else if (childEnumKey is ProcessStateTools.StatusEnum)
                    {
                        flags = SetStatusFlags;
                    }
                }
                return flags;
            }
        }
        private Cst.GroupTrackerEnum SetGroupFlags
        {
            get
            {
                Cst.GroupTrackerEnum group = default(Cst.GroupTrackerEnum);
                foreach (SubChildCheckList subChildCheckList in TrackerTools.GetControlsOfType<SubChildCheckList>(this))
                {
                    if (subChildCheckList.IsSelected)
                    {
                        group = group | (Cst.GroupTrackerEnum)subChildCheckList.Main;
                    }
                }
                return group;
            }
        }
        private ProcessStateTools.ReadyStateEnum SetReadyStateFlags
        {
            get
            {
                ProcessStateTools.ReadyStateEnum readyState = default(ProcessStateTools.ReadyStateEnum);
                foreach (SubChildCheckList subChildCheckList in TrackerTools.GetControlsOfType<SubChildCheckList>(this))
                {
                    if (subChildCheckList.IsSelected)
                    {
                        readyState = readyState | (ProcessStateTools.ReadyStateEnum)subChildCheckList.Main;
                    }
                }
                return readyState;
            }
        }
        private ProcessStateTools.StatusEnum SetStatusFlags
        {
            get
            {
                ProcessStateTools.StatusEnum status = default(ProcessStateTools.StatusEnum);
                foreach (SubChildCheckList subChildCheckList in TrackerTools.GetControlsOfType<SubChildCheckList>(this))
                {
                    if (subChildCheckList.IsSelected)
                    {
                        status = status | (ProcessStateTools.StatusEnum)subChildCheckList.Main;
                    }
                }
                return status;
            }
        }


        public void SetCheckedValue(List<TrackerTools.TrackerFlags> pLstTrackerFlags)
        {
            TrackerTools.TrackerFlags trackerFlags = pLstTrackerFlags.Find(flags => flags.values.First.HasFlag(Main));
            if (null != trackerFlags)
            {
                mainCheck.Checked = true;
                foreach (SubChildCheckList subChildCheckList in TrackerTools.GetControlsOfType<SubChildCheckList>(this))
                {
                    subChildCheckList.SetCheckedValue(trackerFlags);
                }
            }
        }

        #endregion Accessors
        public ChildCheckList()
        {
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CreateControlChildCheckList();
        }

        public void Initialize(Enum pMain, TrackerTools.CheckListKey pChild, int pNbColumns, string pIcon)
        {
            m_Main = pMain;
            m_Child = pChild;
            m_Icon = pIcon;
        }

        private void CreateControlChildCheckList()
        {
            icon.Attributes["class"] = "glyphicon " + m_Icon;
            title.Value = m_Child.Name.ToString();
            mainCheck.Attributes.Add("onclick", String.Format("javaScript:MainCheckChange(this,'{0}');", container.ClientID));
            if (ArrFunc.IsFilled(m_Child.child))
            {
                m_Child.child.ForEach(child =>
                {
                    chkChildList.Visible = ArrFunc.IsEmpty(child.child);
                    if (ArrFunc.IsFilled(child.child))
                    {

                        SubChildCheckList ucSubChild = LoadControl("~/UserControl/SubChildCheckList.ascx") as SubChildCheckList;
                        ucSubChild.ID = child.enumKey.ToString().Replace("Spheres", string.Empty);
                        ucSubChild.Initialize(child.enumKey, child, container.ClientID, mainCheck.ClientID);
                        plhListItem.Controls.Add(ucSubChild);
                    }
                    else
                    {
                        ListItem item = new ListItem(child.Name, child.enumKey.ToString());
                        item.Attributes.Add("id", child.enumKey.ToString());
                        item.Attributes.Add("onclick", String.Format("javaScript:ChildCheckChange(this,'{0}');", mainCheck.ClientID));
                        chkChildList.Items.Add(item);
                    }
                });
            }
        }

        public void OnChildChange(Object sender, EventArgs e)
        {
        }
    }
}
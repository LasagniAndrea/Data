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
    public partial class SubChildCheckList : UserControl
    {
        #region Members
        private Enum m_Main;
        private TrackerTools.CheckListKey m_Child;
        private string m_ContainerID;
        private string m_MainCheckID;
        #endregion Members

        #region Accessors
        public Enum Main { get { return m_Main; } }
        public TrackerTools.CheckListKey Child { get { return m_Child; } }
        public bool IsSelected { get { return check.Checked; } }
        public bool Checked { set { check.Checked = value; } }
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
                foreach (Panel pnl in plhListItem.Controls.OfType<Panel>())
                {
                    foreach (CheckBox check in pnl.Controls.OfType<CheckBox>())
                    {
                        if (check.Checked)
                        {
                            TrackerTools.CheckListKey subChild = Child.child.Find(match => match.enumKey.ToString() == check.ID);
                            group |= (Cst.GroupTrackerEnum)subChild.enumKey;
                        }
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
                foreach (Panel pnl in plhListItem.Controls.OfType<Panel>())
                {
                    foreach (CheckBox check in pnl.Controls.OfType<CheckBox>())
                    {
                        if (check.Checked)
                        {
                            TrackerTools.CheckListKey subChild = Child.child.Find(match => match.enumKey.ToString() == check.ID);
                            readyState |= (ProcessStateTools.ReadyStateEnum)subChild.enumKey;
                        }
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
                foreach (Panel pnl in plhListItem.Controls.OfType<Panel>())
                {
                    foreach (CheckBox check in pnl.Controls.OfType<CheckBox>())
                    {
                        if (check.Checked)
                        {
                            TrackerTools.CheckListKey subChild = Child.child.Find(match => match.enumKey.ToString() == check.ID);
                            status |= (ProcessStateTools.StatusEnum)subChild.enumKey;
                        }
                    }
                }
                return status;
            }
        }


        public void SetCheckedValue(TrackerTools.TrackerFlags pTrackerFlags)
        {
            if (pTrackerFlags.values.Second.HasFlag(Main))
            {
                check.Checked = true;
                if (null != pTrackerFlags.child)
                {
                    foreach (Panel panel in TrackerTools.GetControlsOfType<Panel>(plhListItem))
                    {
                        foreach (CheckBox chk in panel.Controls.OfType<CheckBox>())
                        {
                            TrackerTools.CheckListKey checkListKey = Child.child.Find(item => item.enumKey.ToString() == chk.ID);
                            if (null != checkListKey)
                            {
                                chk.Checked = (pTrackerFlags.child.Exists(match => match.values.First.HasFlag(Main) && match.values.Second.HasFlag(checkListKey.enumKey)));
                            }
                        }
                    }
                }
            }
        }

        #endregion Accessors

        public SubChildCheckList()
        {
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            CreateControlSubChildCheckList();
        }

        public void Initialize(Enum pMain, TrackerTools.CheckListKey pChild, string pContainerID, string pMainCheckID)
        {
            m_Main = pMain;
            m_Child = pChild;
            m_ContainerID = pContainerID;
            m_MainCheckID = pMainCheckID;
        }

        private void CreateControlSubChildCheckList()
        {
            string key = m_Child.enumKey.ToString();
            title.Text = m_Child.Name;
            check.Attributes.Add("onclick", String.Format("javaScript:ChildCheckChange(this,'{0}','{1}');", subContainer.ClientID, m_MainCheckID));
            m_Child.child.ForEach(subChild =>
            {
                plhListItem.Controls.Add(AddChild(subChild));
            });
        }


        private Panel AddChild(TrackerTools.CheckListKey pItem)
        {
            Panel container = new Panel();
            CheckBox chkChildItem = new CheckBox();
            chkChildItem.TextAlign = TextAlign.Left;
            chkChildItem.Text = pItem.Name;
            chkChildItem.ID = pItem.enumKey.ToString();
            chkChildItem.Attributes.Add("onclick", String.Format("javaScript:SubChildCheckChange(this,'{0}','{1}','{2}');", check.ClientID, m_ContainerID, m_MainCheckID));
            container.Controls.Add(chkChildItem);
            return container;
        }

        public void OnChildChange(Object sender, EventArgs e)
        {
        }
    }
}
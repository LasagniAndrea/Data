using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

namespace EFS.TrackerControl
{
    public partial class MainCheckList : UserControl
    {
        #region Members
        private List<TrackerTools.CheckListKey> m_LstCheckList;
        #endregion Members
        #region Accessors

        [DefaultValue(3)]
        public int NbColumns { get; set; }

        [DefaultValue("glyphicon-cog")]
        public string Icon { get; set; }

        public string Title { get; set; }
        [DefaultValue(false)]
        public bool IsWithCheckAll { get; set; }

        public Enum CheckFlags { get; set; }
        private Enum ChildExcludeFlags { get; set; }
        private Enum SubChildExcludeFlags { get; set; }


        [DefaultValue(TrackerTools.CheckListTypeEnum.Service)]
        public TrackerTools.CheckListTypeEnum CheckListType { get; set; }
        #endregion Accessors

        public MainCheckList()
        {
            Icon = "glyphicon-cog";
            NbColumns = 3;
            IsWithCheckAll = false;
            Title = string.Empty;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            //chkSelectAll.Text = string.Empty;
            CreateControl();
        }

        public void Initialize(TrackerTools.CheckListTypeEnum pCheckListType, Enum pCheckFlags, string pTitle, string pSubTitle)
        {
            CheckListType = pCheckListType;
            CheckFlags = pCheckFlags;
            Title = pTitle;
            if (StrFunc.IsFilled(pSubTitle))
                Title += " (" + pSubTitle + ")";
        }

        public void Initialize(TrackerTools.CheckListTypeEnum pCheckListType, Enum pCheckFlags, string pTitle, string pSubTitle, int pNbColumns, string pIcon, bool pIsWithCheckAll)
        {
            Initialize(pCheckListType, pCheckFlags, pTitle, pSubTitle);
            NbColumns = pNbColumns;
            Icon = pIcon;
            IsWithCheckAll = pIsWithCheckAll;
        }


        private void CreateControl()
        {

            ucTitle.InnerText = Title;
            ucTitle.Visible = StrFunc.IsFilled(Title);
            chkSelectAll.Visible = IsWithCheckAll;
            chkSelectAll.Attributes.Add("onclick", String.Format("javaScript:AllCheckChange(this,'{0}');", ucStart.ClientID));
            chkSelectAll.Text = Ressource.GetString("chkSelectAll");

            List<Enum> sortedCheckList = null;
            switch (CheckListType)
            {
                case TrackerTools.CheckListTypeEnum.Service:
                    m_LstCheckList = CreateDicService(Cst.ServiceEnum.NA);
                    sortedCheckList = (from s in m_LstCheckList orderby s.child.Count, s.enumKey select s.enumKey).ToList();
                    break;
                case TrackerTools.CheckListTypeEnum.ReadyStateTracker:
                    m_LstCheckList = CreateDicReadyStateGroup(new ProcessStateTools.ReadyStateEnum(), new Cst.GroupTrackerEnum(), new ProcessStateTools.StatusEnum());
                    sortedCheckList = (from s in m_LstCheckList orderby s.enumKey select s.enumKey).ToList();
                    break;
                case TrackerTools.CheckListTypeEnum.GroupTracker:
                    m_LstCheckList = CreateDicReadyStateGroup(new Cst.GroupTrackerEnum(), new ProcessStateTools.ReadyStateEnum(), new ProcessStateTools.StatusEnum());
                    sortedCheckList = (from s in m_LstCheckList orderby s.enumKey select s.enumKey).ToList();
                    break;
                case TrackerTools.CheckListTypeEnum.StatusTracker:
                    m_LstCheckList = CreateDicReadyStateGroup(new ProcessStateTools.StatusEnum(), new Cst.GroupTrackerEnum(), new ProcessStateTools.ReadyStateEnum());
                    sortedCheckList = (from s in m_LstCheckList orderby s.enumKey select s.enumKey).ToList();
                    break;
            }
            CreateControlCheckList(sortedCheckList);
        }

        #region CreateControlCheckList
        private void CreateControlCheckList(List<Enum> pSortedCheckList)
        {
            pSortedCheckList.ForEach(main =>
            {
                TrackerTools.CheckListKey mainCheckListKey = m_LstCheckList.Find(match => match.enumKey == main);

                ChildCheckList ucChild = LoadControl("~/UserControl/ChildCheckList.ascx") as ChildCheckList;
                ucChild.ID = mainCheckListKey.enumKey.ToString().Replace("Spheres", string.Empty);
                ucChild.Initialize(mainCheckListKey.enumKey, mainCheckListKey, NbColumns, Icon);
                mainContent.Controls.Add(ucChild);
            });
        }
        #endregion CreateControlCheckList

        #region CreateDicReadyStateGroup
        public List<TrackerTools.CheckListKey> CreateDicReadyStateGroup<T1, T2, T3>(T1 pMain, T2 pChild, T3 pSubChild)
        {
            List<TrackerTools.CheckListKey> _lst = new List<TrackerTools.CheckListKey>();
            TrackerTools.CheckListKey checkListKey = null;
            TrackerTools.CheckListKey childCheckListKey = null;
            TrackerTools.CheckListKey subChildCheckListKey = null;
            foreach (Enum main in Enum.GetValues(typeof(T1)))
            {
                bool isValid = true;
                if (pMain is Cst.GroupTrackerEnum)
                    isValid = (((Cst.GroupTrackerEnum)main) != Cst.GroupTrackerEnum.ALL);

                if (isValid && ((null == CheckFlags) || CheckFlags.HasFlag(main)))
                {
                    checkListKey = new TrackerTools.CheckListKey();
                    checkListKey.enumKey = main;
                    checkListKey.Name = GetCheckName(main);
                    checkListKey.Checked = false;

                    foreach (Enum child in Enum.GetValues(typeof(T2)))
                    {
                        isValid = true;
                        if (pChild is Cst.GroupTrackerEnum)
                            isValid = (((Cst.GroupTrackerEnum)child) != Cst.GroupTrackerEnum.ALL);

                        if (isValid && ((null == ChildExcludeFlags) || (false == ChildExcludeFlags.HasFlag(child))))
                        {
                            childCheckListKey = new TrackerTools.CheckListKey();
                            childCheckListKey.enumKey = child;
                            childCheckListKey.Name = GetCheckName(child);
                            childCheckListKey.Checked = false;

                            foreach (Enum subChild in Enum.GetValues(typeof(T3)))
                            {
                                isValid = true;
                                if (pSubChild is Cst.GroupTrackerEnum)
                                    isValid = (((Cst.GroupTrackerEnum)subChild) != Cst.GroupTrackerEnum.ALL);

                                if (isValid && ((null == SubChildExcludeFlags) || (false == SubChildExcludeFlags.HasFlag(subChild))))
                                {
                                    subChildCheckListKey = new TrackerTools.CheckListKey();
                                    subChildCheckListKey.enumKey = subChild;
                                    subChildCheckListKey.Name = GetCheckName(subChild);
                                    subChildCheckListKey.Checked = false;


                                    if (false == _lst.Exists(match => match.enumKey == main))
                                    {
                                        checkListKey.child = new List<TrackerTools.CheckListKey>();
                                        childCheckListKey.child = new List<TrackerTools.CheckListKey>();
                                        childCheckListKey.child.Add(subChildCheckListKey);
                                        checkListKey.child.Add(childCheckListKey);
                                        _lst.Add(checkListKey);
                                    }
                                    else
                                    {
                                        checkListKey = _lst.Find(match => match.enumKey == main);

                                        if (false == checkListKey.child.Exists(match => match.enumKey == child))
                                        {
                                            childCheckListKey.child = new List<TrackerTools.CheckListKey>();
                                            childCheckListKey.child.Add(subChildCheckListKey);
                                            checkListKey.child.Add(childCheckListKey);
                                        }
                                        else
                                        {
                                            childCheckListKey = checkListKey.child.Find(match => match.enumKey == child);
                                            childCheckListKey.child.Add(subChildCheckListKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _lst;
        }
        #endregion CreateDicReadyStateGroup

        private string GetCheckName(Enum pValue)
        {
            FieldInfo fldAttr = null;
            string checkName = pValue.ToString();
            fldAttr = pValue.GetType().GetField(pValue.ToString());
            object[] enumAttrs = fldAttr.GetCustomAttributes(typeof(XmlEnumAttribute), true);
            if (ArrFunc.IsFilled(enumAttrs))
                checkName = ((XmlEnumAttribute)enumAttrs[0]).Name;
            return Ressource.GetString(checkName);
        }

        #region CreateDicService
        public List<TrackerTools.CheckListKey> CreateDicService(Enum pExcludeMain)
        {
            List<TrackerTools.CheckListKey> _lst = new List<TrackerTools.CheckListKey>();
            FieldInfo fldAttr = null;
            object[] enumAttrs = null;
            string childName = string.Empty;
            TrackerTools.CheckListKey childCheckListKey = null;
            Enum service = Cst.ServiceEnum.NA;
            foreach (Cst.ProcessTypeEnum process in Enum.GetValues(typeof(Cst.ProcessTypeEnum)))
            {

                fldAttr = process.GetType().GetField(process.ToString());
                enumAttrs = fldAttr.GetCustomAttributes(typeof(Cst.ProcessRequestedAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                {
                    service = ((Cst.ProcessRequestedAttribute)enumAttrs[0]).ServiceName;
                    if (0 != service.CompareTo(pExcludeMain))
                    {
                        TrackerTools.CheckListKey checkListKey = new TrackerTools.CheckListKey();
                        checkListKey.enumKey = service;
                        checkListKey.Name = service.ToString();
                        checkListKey.Checked = false;

                        childCheckListKey = new TrackerTools.CheckListKey();
                        childCheckListKey.enumKey = process;

                        childCheckListKey.Name = Ressource.GetString(process.ToString());
                        childCheckListKey.Checked = false;

                        if (false == _lst.Exists(match => match.enumKey.ToString() == service.ToString()))
                        {
                            checkListKey.child = new List<TrackerTools.CheckListKey>();
                            checkListKey.child.Add(childCheckListKey);
                            _lst.Add(checkListKey);
                        }
                        else
                        {
                            checkListKey = _lst.Find(match => match.enumKey.ToString() == service.ToString());
                            checkListKey.child.Add(childCheckListKey);
                        }
                    }
                }
            }
            return _lst;
        }
        #endregion CreateDicService
    }
}
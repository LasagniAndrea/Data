#region Using Directives
using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.UI.WebControls;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    #region MonitoringTools
    /// <summary>
    /// Outils de gestion du monitoring
    /// </summary>
    public sealed class MonitoringTools
    {
        #region  MonitoringRequestedAttribute
        /// <summary>
        /// Attribut de regroupement des éléments de Monitoring
        /// </summary>
        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        public sealed class MonitoringRequestedAttribute : Attribute
        {
            #region Members
            private MonitoringGroupEnum m_Group;
            #endregion Members
            #region Accessors
            public MonitoringGroupEnum Group
            {
                get { return (m_Group); }
                set { m_Group = value; }
            }
            public string Ressource
            {
                get
                {
                    string _ressource;
                    switch (m_Group)
                    {
                        case MonitoringGroupEnum.POSREQUEST:
                            _ressource = "GroupTrackerCLO";
                            break;
                        default:
                            _ressource = m_Group.ToString();
                            break;
                    }
                    return _ressource;
                }
            }
            #endregion Accessors
        }
        #endregion MonitoringRequestedAttribute

        #region MonitoringGroupEnum
        /// <summary>
        /// Groupes monitorés
        /// </summary>
        public enum MonitoringGroupEnum
        {
            IO,
            POSREQUEST,
            CASHBALANCE,
            QUOTE,
            NA,
        }
        #endregion MonitoringGroupEnum

        #region MonitoringElementEnum
        /// <summary>
        /// Eléments monitorés
        /// </summary>
        public enum MonitoringElementEnum
        {
            [MonitoringRequested(Group = MonitoringGroupEnum.IO)]
            [System.Xml.Serialization.XmlEnumAttribute("INPUT")]
            INPUT,
            [MonitoringRequested(Group = MonitoringGroupEnum.IO)]
            [System.Xml.Serialization.XmlEnumAttribute("OUTPUT")]
            OUTPUT,
            [MonitoringRequested(Group = MonitoringGroupEnum.IO)]
            [System.Xml.Serialization.XmlEnumAttribute("EDSP")]
            EDSP,
            [MonitoringRequested(Group = MonitoringGroupEnum.POSREQUEST)]
            [System.Xml.Serialization.XmlEnumAttribute("EOD")]
            EndOfDay,
            [MonitoringRequested(Group = MonitoringGroupEnum.POSREQUEST)]
            [System.Xml.Serialization.XmlEnumAttribute("CLOSINGDAY")]
            ClosingDay,
            [MonitoringRequested(Group = MonitoringGroupEnum.POSREQUEST)]
            [System.Xml.Serialization.XmlEnumAttribute("CASHBALANCE")]
            CASHBALANCE,
            [System.Xml.Serialization.XmlEnumAttribute("NA")]
            NA,
        }
        #endregion MonitoringElementEnum

        #region CreateDicMonitoringElement
        /// <summary>
        ///  Création de la liste des élements monitorés
        /// </summary>
        public static Dictionary<Pair<MonitoringGroupEnum, string>, List<Pair<MonitoringElementEnum, string>>> CreateDicMonitoringElement()
        {
            Dictionary<Pair<MonitoringGroupEnum, string>, List<Pair<MonitoringElementEnum, string>>>
                _dicGroup = new Dictionary<Pair<MonitoringGroupEnum, string>, List<Pair<MonitoringElementEnum, string>>>();

            FieldInfo fldAttr = null;
            object[] enumAttrs = null;
            string resGroup = string.Empty;
            MonitoringGroupEnum group = MonitoringGroupEnum.NA;
            Pair<MonitoringGroupEnum, string> pairGroup = null;
            Pair<MonitoringElementEnum, string> pairElement = null;
            List<Pair<MonitoringElementEnum, string>> lstMonitoringElement = null;
            foreach (MonitoringElementEnum element in Enum.GetValues(typeof(MonitoringElementEnum)))
            {
                fldAttr = element.GetType().GetField(element.ToString());
                enumAttrs = fldAttr.GetCustomAttributes(typeof(MonitoringRequestedAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                {
                    group = ((MonitoringRequestedAttribute)enumAttrs[0]).Group;
                    resGroup = ((MonitoringRequestedAttribute)enumAttrs[0]).Ressource;
                    pairElement = new Pair<MonitoringElementEnum, string>(element, Ressource.GetString(element.ToString()));
                    if (MonitoringGroupEnum.NA != group)
                    {
                        List<Pair<MonitoringGroupEnum, string>> lstGroup = new List<Pair<MonitoringGroupEnum, string>>(_dicGroup.Keys);
                        if (false == lstGroup.Exists(key => key.First == group))
                        {
                            lstMonitoringElement = new List<Pair<MonitoringElementEnum, string>>
                            {
                                pairElement
                            };
                            pairGroup = new Pair<MonitoringGroupEnum, string>(group, resGroup);
                            _dicGroup.Add(pairGroup, lstMonitoringElement);
                        }
                        else
                        {
                            pairGroup = lstGroup.Find(key => key.First == group);
                            _dicGroup[pairGroup].Add(pairElement);
                        }
                    }
                }
            }
            return _dicGroup;
        }
        #endregion CreateDicMonitoringElement

        #region CreateControlCheckListMonitoringElement
        /// <summary>
        /// Construction de la checkBoxList de sélection des groupes/éléments à monitorer
        /// Valorisation des checkbox (utilisation Puissance de 2 stockées dans variable de session
        /// </summary>
        /// <returns></returns>
        public static OptionGroupCheckBoxList CreateControlCheckListMonitoringElement()
        {
            return CreateControlCheckListMonitoringElement(0);
        }

        public static OptionGroupCheckBoxList CreateControlCheckListMonitoringElement(Int64 pPowerSelected)
        {
            Dictionary<Pair<MonitoringGroupEnum, string>, List<Pair<MonitoringElementEnum, string>>> m_MonitoringObserver = CreateDicMonitoringElement();
            return CreateControlCheckListMonitoringElement(m_MonitoringObserver, pPowerSelected);
        }
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public static OptionGroupCheckBoxList CreateControlCheckListMonitoringElement(
            Dictionary<Pair<MonitoringGroupEnum, string>, List<Pair<MonitoringElementEnum, string>>> pMonitoringObserver, Int64 pPowerSelected)
        {
            MonitoringGroupEnum group = MonitoringGroupEnum.NA;
            OptionGroupCheckBoxList lstElement = new OptionGroupCheckBoxList
            {
                EnableViewState = true,
                RepeatColumns = 1,
                RepeatLayout = RepeatLayout.Table,
                ID = "LSTMONITORINGELEMENT",
                Width = Unit.Percentage(100)
            };

            ExtendedListItem extLi = null;
            ListItem item = null;
            int extLiIndex = -1;

            List<Pair<MonitoringGroupEnum, string>> lstGroup = new List<Pair<MonitoringGroupEnum, string>>(pMonitoringObserver.Keys);

            foreach (string s in Enum.GetNames(typeof(MonitoringGroupEnum)))
            {
                group = (MonitoringGroupEnum)Enum.Parse(typeof(MonitoringGroupEnum), s);

                if (lstGroup.Exists(key => key.First == group))
                {
                    Pair<MonitoringGroupEnum, string> pair = lstGroup.Find(key => key.First == group);
                    lstElement.Items.Add(group.ToString());
                    extLiIndex = lstElement.ExtendedItems.Count - 1;
                    extLi = lstElement.ExtendedItems[extLiIndex];
                    extLi.GroupingType = ListItemGroupingTypeEnum.New;
                    extLi.GroupingText = Ressource.GetString(pair.Second);
                    extLi.GroupingClass = "fa-icon fab fa-whmcs violet imgServiceObserver";
                    extLi.GroupCssClass = "chkGroupObserver";

                    pMonitoringObserver[pair].ForEach(element =>
                    {
                        item = new ListItem(element.Second, element.First.ToString());
                        // check de l'item de la liste 
                        int i = int.Parse(Enum.Format(typeof(MonitoringElementEnum), element.First, "d"));
                        item.Selected = (0 < (pPowerSelected & Convert.ToInt64(Math.Pow(2, i))));
                        item.Attributes.Add("class", "chkMonitoringObserver");
                        item.Attributes.Add("alt", element.First.ToString());
                        if (item.Selected)
                            item.Attributes.Add("checked", "checked");
                        lstElement.Items.Add(item);
                    }
                    );
                }
            }
            return lstElement;
        }
        #endregion CreateControlCheckListMonitoringElement

        #region GetPowerOfMonitoring
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Int64 GetPowerOfMonitoring()
        {
            MonitoringElementEnum elementEnum = new MonitoringElementEnum();
            FieldInfo[] elementFlds = elementEnum.GetType().GetFields();
            Int64 powerElement = 0;
            foreach (FieldInfo elementFld in elementFlds)
            {
                object[] monitoringRequestedAttrs = elementFld.GetCustomAttributes(typeof(MonitoringRequestedAttribute), true);
                if (0 < monitoringRequestedAttrs.Length)
                {
                    MonitoringElementEnum element = (MonitoringElementEnum)Enum.Parse(typeof(MonitoringElementEnum), elementFld.Name, false);
                    int i = int.Parse(Enum.Format(typeof(MonitoringElementEnum), element, "d"));
                    powerElement += Convert.ToInt64(Math.Pow(2, i));
                }
            }
            return powerElement;
        }
        #endregion GetPowerOfMonitoring

    }
    #endregion MonitoringTools
}

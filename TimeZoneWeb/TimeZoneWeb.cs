using EFS.ACommon;
using EFS.Common;
using NodaTime.TimeZones.Cldr;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using Tz = EFS.TimeZone;

namespace EFS.TimeZone
{
    public sealed class Web
    {
        /// <summary>
        /// Classe qui pilote le chargement des mapZones dans un listControl
        /// </summary>
        [XmlRoot(ElementName = "LoadMapZoneArguments", IsNullable = true)]
        /// EG 20170929 [22374][23450] New
        public class LoadMapZoneArguments
        {
            #region Members
            [System.Xml.Serialization.XmlTextAttribute()]
            public string code;
            [System.Xml.Serialization.XmlTextAttribute()]
            public bool isWithEmpty;
            [System.Xml.Serialization.XmlTextAttribute()]
            public string territory;
            [System.Xml.Serialization.XmlTextAttribute()]
            public string territoryExclude;
            [System.Xml.Serialization.XmlTextAttribute()]
            public string windowsId;
            [System.Xml.Serialization.XmlTextAttribute()]
            public string windowsIdExclude;
            #endregion Members

            #region constructor
            public LoadMapZoneArguments()
            {
            }
            #endregion

            #region Methods
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pListRetrievalData"></param>
            /// <param name="pWithEmpty"></param>
            /// <returns></returns>
            public static LoadMapZoneArguments GetArguments(string pListRetrievalData)
            {
                return GetArguments(pListRetrievalData, false);
            }
            // EG 20171004 [22374][23452] Upd
            public static LoadMapZoneArguments GetArguments(string pListRetrievalData, bool pWithEmpty)
            {
                string listRetrievalValue = pListRetrievalData;
                string[] listRetrieval = pListRetrievalData.Split(".".ToCharArray());
                if (ArrFunc.IsFilled(listRetrieval) && listRetrieval.Length > 1)
                {
                    listRetrievalValue = pListRetrievalData.Remove(0, ((string)(listRetrieval[0] + ".")).Length);
                }

                bool isComplexType = StrFunc.IsXML(listRetrievalValue);
                LoadMapZoneArguments args = null;
                if (isComplexType)
                {
                    args = LoadMapZoneArguments.GetArgumentsFromSerializedText(listRetrievalValue);
                    args.isWithEmpty = pWithEmpty;
                }
                else if (listRetrievalValue.StartsWith("[") && listRetrievalValue.EndsWith("]"))
                {
                    listRetrievalValue = listRetrievalValue.Remove(0, 1);
                    listRetrievalValue = listRetrievalValue.Remove(listRetrievalValue.Length - 1, 1);
                    args = new LoadMapZoneArguments
                    {
                        isWithEmpty = pWithEmpty
                    };

                    string[] list = listRetrievalValue.Split(";".ToCharArray());
                    foreach (string s in list)
                    {
                        string[] listValue = s.Split(":".ToCharArray());
                        switch (listValue[0].ToLower())
                        {
                            case "code":
                                args.code = listValue[1];
                                break;
                            case "territory":
                                args.territory = listValue[1];
                                break;
                            case "territoryexclude":
                                args.territoryExclude = listValue[1];
                                break;
                            case "windowsid":
                                args.windowsId = listValue[1];
                                break;
                            case "windowsidexclude":
                                args.windowsIdExclude = listValue[1];
                                break;
                        }
                    }
                }
                //
                if (null == args)
                {
                    args = new LoadMapZoneArguments
                    {
                        isWithEmpty = pWithEmpty,
                        code = listRetrievalValue,
                        territory = null,
                        territoryExclude = null,
                        windowsId = null,
                        windowsIdExclude = null
                    };
                }
                return args;
            }

            public static LoadMapZoneArguments GetArgumentsFromSerializedText(string pText)
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(LoadMapZoneArguments), pText);
                return (LoadMapZoneArguments)CacheSerializer.Deserialize(serializeInfo);
            }
            #endregion
        }
        // EG 20171004 [22374][23452] Upd
        public static void LoadMapZone(ListControl pControl, LoadMapZoneArguments pArgs)
        {
            string windowsId = StrFunc.IsFilled(pArgs.windowsId) ? pArgs.windowsId : Tz.Tools.AllWindowsID;

            List<MapZone> mapZones = Tools.ListMapZonesByWindowsId(windowsId);
            if (StrFunc.IsFilled(pArgs.windowsIdExclude))
                mapZones = mapZones.Where(item => (false == item.WindowsId.Equals(pArgs.windowsIdExclude))).ToList();
            if (StrFunc.IsFilled(pArgs.territoryExclude))
                mapZones = mapZones.Where(item => (false == item.Territory.Equals(pArgs.territoryExclude))).ToList();
            if (StrFunc.IsFilled(pArgs.territory))
            {
                //PL 20190507 Add Tz "Etc"
                //mapZones = mapZones.Where(item => item.Territory.Equals(pArgs.territory)).ToList();
                mapZones = mapZones.Where(item => item.Territory.Equals(pArgs.territory) || item.TzdbIds[0].StartsWith("Etc")).ToList();
            }

            List<string> lstResult = null;
            switch (pArgs.code.ToLower())
            {
                case "territory":
                    lstResult = Tools.ListTerritoryByMapZone(mapZones);
                    break;
                case "tzdbid":
                    lstResult = Tools.ListTzdbIdByMapZone(mapZones);
                    break;
                case "windowsid":
                    lstResult = Tools.ListWindowsIdByMapZone(mapZones);
                    break;
            }

            if (null != lstResult)
                LoadListToListControl(pControl, lstResult, pArgs.isWithEmpty);
        }

        #region Methods
        #region LoadListToListControl
        /// EG 20170929 [22374][23450] New
        public static void LoadListToListControl(ListControl pControl, List<string> pList)
        {
            LoadListToListControl(pControl, pList, false);
        }
        public static void LoadListToListControl(ListControl pControl, List<string> pList, bool pAddEmptyItem)
        {
            pControl.Items.Clear();
            if (null != pList)
            {
                if (pAddEmptyItem)
                    pControl.Items.Add(new ListItem(string.Empty, string.Empty));
                pList.ForEach(item => pControl.Items.Add(new ListItem(item, item)));
            }
        }
        #endregion LoadListToListControl


        #region LoadTerritoryToListControl
        /// <summary>
        /// Charge la liste des territoires pour un windowsId donné dans un ListControl (DropdownList, ListBox)
        /// </summary>
        /// EG 20170929 [22374][23450] New
        public static void LoadTerritoryToListControl(ListControl pControl)
        {
            LoadTerritoryToListControl(pControl, Tz.Tools.AllWindowsID, false);
        }
        public static void LoadTerritoryToListControl(ListControl pControl, string pWindowsId, bool pIsAddAll)
        {
            List<string> territories = Tools.ListTerritoryByWindowsId(pWindowsId);
            LoadListToListControl(pControl, territories);
            if (pIsAddAll)
                pControl.Items.Insert(0, new ListItem(Ressource.GetString(Tools.AllTerritories, "*** All territories ***"), Tools.AllTerritories));
        }
        #endregion LoadTerritoryToListControl
        
        #region LoadTzdbIdByTerritoryToListControl
        /// <summary>
        /// Charge la liste des timezones pour un territoire donné dans un ListControl (DropdownList, ListBox)
        /// </summary>
        /// EG 20170929 [22374][23450] New
        public static void LoadTzdbIdByTerritoryToListControl(ListControl pControl, string pTerritory)
        {
            LoadTzdbIdByTerritoryToListControl(pControl, pTerritory, false, string.Empty);
        }
        public static void LoadTzdbIdByTerritoryToListControl(ListControl pControl, string pTerritory, string pWindowsId)
        {
            LoadTzdbIdByTerritoryToListControl(pControl, pTerritory, false, pWindowsId);
        }
        public static void LoadTzdbIdByTerritoryToListControl(ListControl pControl, string pTerritory, bool pAddEmptyItem, string pWindowsId)
        {
            List<MapZone> mapZones = Tools.ListMapZonesByTerritory(pTerritory, pWindowsId);
            LoadTzdbIdToListControl(pControl, mapZones, pAddEmptyItem);
        }
        #endregion LoadTzdbIdByTerritoryToListControl
        #region LoadTzdbIdByWindowsIdToListControl
        /// <summary>
        /// Charge la liste des timezones pour un windowsId donné dans un ListControl (DropdownList, ListBox)
        /// </summary>
        /// EG 20170929 [22374][23450] New
        public static void LoadTzdbIdByWindowsIdToListControl(ListControl pControl)
        {
            LoadTzdbIdByWindowsIdToListControl(pControl, Tz.Tools.AllWindowsID);
        }
        public static void LoadTzdbIdByWindowsIdToListControl(ListControl pControl, string pWindowsId)
        {
            List<MapZone> mapZones = Tools.ListMapZonesByWindowsId(pWindowsId);
            LoadTzdbIdToListControl(pControl, mapZones, false);
        }
        #endregion LoadTzdbIdByWindowsIdToListControl

        #region LoadTzdbIdToListControl
        /// <summary>
        /// Chargement des TimeZone de la liste des MapZone dans un ListControl (DropdownList, ListBox)
        /// </summary>
        /// EG 20170929 [22374][23450] New
        private static void LoadTzdbIdToListControl(ListControl pControl, List<MapZone> pMapZones, bool pAddEmptyItem)
        {
            List<string> tzdbIds = Tools.ListTzdbIdByMapZone(pMapZones);
            LoadTzdbIdToListControl(pControl, tzdbIds, pAddEmptyItem);
        }
        private static void LoadTzdbIdToListControl(ListControl pControl, List<string> pTzdbIds, bool pAddEmptyItem)
        {
            LoadListToListControl(pControl, pTzdbIds, pAddEmptyItem);
        }
        #endregion LoadTzdbIdToListControl
        #region LoadWindowsIdToListControl
        /// <summary>
        /// Chargement des WindowsId de la liste des MapZones dans un ListControl (DropdownList, ListBox)
        /// </summary>
        /// EG 20170929 [22374][23450] New
        public static void LoadWindowsIdToListControl(ListControl pControl)
        {
            LoadWindowsIdToListControl(pControl, false);
        }
        public static void LoadWindowsIdToListControl(ListControl pControl, bool pIsAddAll)
        {
            List<string> windowsIds = Tools.ListWindowsIdByMapZone();
            LoadListToListControl(pControl, windowsIds);
            if (pIsAddAll)
                pControl.Items.Insert(0, new ListItem(Ressource.GetString(Tools.AllWindowsID, "*** All time zones ***"), Tools.AllWindowsID));
        }
        #endregion LoadWindowsIdToListControl

        #endregion Methods
    }
}

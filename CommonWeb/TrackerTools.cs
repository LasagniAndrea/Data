using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;

namespace EFS.Common.Web
{
    /// EG 20234429 [WI756] Spheres Core : Refactoring Code Analysis
    public sealed class TrackerTools
    {


        public enum CheckListTypeEnum
        {
            Service,
            GroupTracker,
            ReadyStateTracker,
            StatusTracker,
        }

        public class CheckListKey
        {
            public Enum EnumKey { set; get; }
            public string Name { set; get; }
            public bool Checked { set; get; }
            public List<CheckListKey> Child { set; get; }
        }


        public class TrackerFlags
        {
            public Pair<Enum, Enum> values;
            public List<TrackerFlags> child;
        }

        public static IEnumerable<T> GetControlsOfType<T>(Control root)
            where T : Control
        {
            if (root is T t)
                yield return t;

            if (root is Control container)
                foreach (Control c in container.Controls)
                    foreach (var i in GetControlsOfType<T>(c))
                        yield return i;
        }

        public static IEnumerable<T> MaskToList<T>(Enum mask)
        {
            if (typeof(T).IsSubclassOf(typeof(Enum)) == false)
                throw new ArgumentException();

            return Enum.GetValues(typeof(T))
                                 .Cast<Enum>()
                                 .Where(m => mask.HasFlag(m))
                                 .Cast<T>();
        }

        #region SaveTrackerDisplayValueToCookie
        /// <summary>
        /// Sauvegarde des filtres sur les mode d'affichage du Tracker dans la table Cookie
        /// </summary>
        /// <param name="pTrackerDisplayValues"></param>
        public static void SaveTrackerDisplayValueToCookie(Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>> pTrackerDisplayValues)
        {
            if (null != pTrackerDisplayValues)
            {
                foreach (TrackerTools.CheckListTypeEnum key in pTrackerDisplayValues.Keys)
                {
                    string value = string.Empty;
                    List<TrackerTools.TrackerFlags> lst = pTrackerDisplayValues[key];
                    lst.ForEach(item =>
                    {
                        value += "[" + item.values.First + "=" + Convert.ToInt64(item.values.Second) + "]";
                        item.child.ForEach(child =>
                        {
                            value += "{" + child.values.First.ToString() + "=" + Convert.ToInt64(child.values.Second) + "}";
                        });
                    });
                    AspTools.WriteSQLCookie(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerDisplayValues + "=" + key, value);
                }
            }
        }
        #endregion SaveTrackerDisplayValueToCookie

        #region ReadTrackerDisplayValueFromCookie
        // EG 20180423 Analyse du code Correction [CA2200]
        public static Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>> ReadTrackerDisplayValueFromCookie(string pCS)
        {
            IDataReader dr = null;
            Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>> trackerDisplayValues = null;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "GRPELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.SQLCookieGrpElement.Tracker.ToString());
                parameters.Add(new DataParameter(pCS, "ELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Cst.SQLCookieElement.TrackerDisplayValues.ToString() + "%");
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), SessionTools.Collaborator_IDA);
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.HOSTNAME), SessionTools.ServerAndUserHost);

                string sqlSelect = @"select ELEMENT, VALUE 
                from dbo.COOKIE
                where (ELEMENT like @ELEMENT) and (IDA = @IDA) and (GRPELEMENT = @GRPELEMENT)";

                QueryParameters qryParameters = new QueryParameters(pCS, sqlSelect.ToString(), parameters);
                dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                trackerDisplayValues = new Dictionary<CheckListTypeEnum, List<TrackerFlags>>();
                while (dr.Read())
                {
                    string element = dr[0].ToString().Replace(Cst.SQLCookieElement.TrackerDisplayValues.ToString() + "=", string.Empty);
                    TrackerTools.CheckListTypeEnum checkListType = (TrackerTools.CheckListTypeEnum)ReflectionTools.EnumParse(new TrackerTools.CheckListTypeEnum(), element);
                    string[] result = dr[1].ToString().Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (ArrFunc.IsFilled(result))
                    {
                        Enum _enum = null;
                        Enum _enumValues = null;
                        List<TrackerFlags> lstTrackerFlags = new List<TrackerFlags>();
                        for (int i = 0; i < result.Length; i += 2)
                        {
                            TrackerFlags trackerFlags = new TrackerFlags();
                            string[] first = result[i].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                            string[] second = result[i + 1].Split(new string[] { "{", "}" }, StringSplitOptions.RemoveEmptyEntries);
                            switch (checkListType)
                            {
                                case CheckListTypeEnum.ReadyStateTracker:
                                    _enum = (ProcessStateTools.ReadyStateEnum)Enum.Parse(typeof(ProcessStateTools.ReadyStateEnum), first[0]);
                                    _enumValues = (Cst.GroupTrackerEnum)Convert.ToInt64(first[1]);
                                    break;
                                case CheckListTypeEnum.GroupTracker:
                                    _enum = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), first[0]);
                                    _enumValues = (ProcessStateTools.ReadyStateEnum)Convert.ToInt64(first[1]);
                                    break;
                                case CheckListTypeEnum.StatusTracker:
                                    _enum = (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), first[0]);
                                    _enumValues = (Cst.GroupTrackerEnum)Convert.ToInt64(first[1]);
                                    break;
                            }
                            trackerFlags.values = new Pair<Enum, Enum>(_enum, _enumValues);
                            trackerFlags.child = new List<TrackerFlags>();


                            Enum _childEnum = null;
                            Enum _childEnumValues = null;
                            for (int j = 0; j < second.Length; j++)
                            {
                                string[] child = second[j].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                TrackerFlags childTrackerFlags = new TrackerFlags();
                                switch (checkListType)
                                {
                                    case CheckListTypeEnum.ReadyStateTracker:
                                        _childEnum = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), child[0]);
                                        _childEnumValues = (ProcessStateTools.StatusEnum)Convert.ToInt64(child[1]);
                                        break;
                                    case CheckListTypeEnum.GroupTracker:
                                        _childEnum = (ProcessStateTools.ReadyStateEnum)Enum.Parse(typeof(ProcessStateTools.ReadyStateEnum), child[0]);
                                        _childEnumValues = (ProcessStateTools.StatusEnum)Convert.ToInt64(child[1]);
                                        break;
                                    case CheckListTypeEnum.StatusTracker:
                                        _childEnum = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), child[0]);
                                        _childEnumValues = (ProcessStateTools.ReadyStateEnum)Convert.ToInt64(child[1]);
                                        break;
                                }
                                childTrackerFlags.values = new Pair<Enum, Enum>(_childEnum, _childEnumValues);
                                trackerFlags.child.Add(childTrackerFlags);
                            }
                            lstTrackerFlags.Add(trackerFlags);
                        }
                        trackerDisplayValues.Add(checkListType, lstTrackerFlags);
                    }
                }

                if (0 == trackerDisplayValues.Count)
                {
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (null != dr)
                    dr.Close();


            }
            return trackerDisplayValues;
        }
        #endregion ReadTrackerDisplayValueFromCookie

    }
}

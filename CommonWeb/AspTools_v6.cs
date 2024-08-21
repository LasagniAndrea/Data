#region Using Directives
using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.IO;
using System.Globalization;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
#endregion Using Directives

namespace EFS.Common.Web
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class AspTools
    {
        public static bool IsSpheresV6_BootStrap()
        {
            return BoolFunc.IsTrue(SystemSettings.GetAppSettings("IsSpheresV6_BootStrap", "false"));
        }
        public static string GetListAspxName()
        {
            return IsSpheresV6_BootStrap() ? "ListViewer.aspx" : "List.aspx";
        }
        public static string SetListOrListViewer(string pValue)
        {
            if (IsSpheresV6_BootStrap())
                return pValue.Replace("List.aspx", "ListViewer.aspx");
            else
                return pValue.Replace("ListViewer.aspx", "List.aspx");
        }
        public static bool IsStartWithListAspx(string pValue)
        {
            return pValue.ToUpper().StartsWith(IsSpheresV6_BootStrap() ? "LISTVIEWER.ASPX" : "LIST.ASPX");
        }
        public static string GetReferentialProject()
        {
            return (IsSpheresV6_BootStrap() ? "GridViewProcessor" : "Referentiel");
        }
        public static string SetReferentielOrRepository(string pValue)
        {
            if (IsSpheresV6_BootStrap())
                return pValue.Replace("Referential.aspx", "Repository.aspx");
            else
                return pValue.Replace("Repository.aspx", "Referential.aspx"); ;
        }
        public static string SetAspxPageName(string pValue)
        {
            string @value = SetListOrListViewer(pValue);
            return SetReferentielOrRepository(@value);
        }
    }
}

#region Using Directives
using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.DataEnabled;
using EFS.Process;

using EFS.Actor;
using EFS.Restriction;
using EFS.Permission;
using EFS.Rights;


using EfsML.Enum;

using FixML.v44.Enum;
using FixML.v50SP1.Enum;

using Tz = EFS.TimeZone;

#endregion Using Directives

namespace EFS.Common.Web
{
    #region public enum LstSelectionTypeEnum
    public enum LstSelectionTypeEnum
    {
        NONE,
        DISP,	// Select des colonnes à afficher
        SORT	// Select des colonnes de tri
    }
    #endregion

    #region public enum UnitEnum
    public enum UnitEnum
    {
        Pixel,
        Percentage
    }
    #endregion

    #region AdditionalCheckBoxEnum
    public enum AdditionalCheckBoxEnum
    {
        ShowAllData,
        ShowColumnName,
        TodayUpdate,
        ShowPopover,
        Validity,

    }
    #endregion
    /// <summary>
    /// Enumération des filtres d'affichage des référentiels (remplace les checkbox dans l'ancienne version)
    /// </summary>
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    // EG [XXXXX][WI437] Nouvelles options de filtrage des données sur les référentiels
    public enum AdditionalCheckBoxEnum2
    {
        ShowAllData,
        ActivatedData,
        DeactivatedData,
        TodayData,
        TodayCreateData,
        TodayUpdateData,
        TodayUserData,
        TodayUserCreateData,
        TodayUserUpdateData,
    }

    #region public enum MaintainScrollPageEnum
    public enum MaintainScrollPageEnum
    {
        Automatic,
        Manually
    }
    #endregion

    #region public class ControlID
    ///	<summary>
    ///	Classe pour l'attribution d'ID à un control en f° de son type
    ///	</summary>
    public sealed class ControlID
    {
        #region public getID
        public static string GetID(string pName, string pType)
        {
            return GetID(pName, pType, null);
        }
        public static string GetID(string pName, string pType, string pPrefix)
        {
            if (StrFunc.IsEmpty(pPrefix))
            {
                if (TypeData.IsTypeBool(pType))
                    pPrefix = Cst.CHK;
                else
                    pPrefix = Cst.TXT;
            }
            return pPrefix + pName;
        }
        #endregion
    }
    #endregion class ControlID

    #region public class ControlsTools
    /// <summary>
    /// Functions for utilization with control 
    /// </summary>
    /// 
        // EG 20200914 [XXXXX] Suppression de méthodes obsolètes
    public sealed partial class ControlsTools
    {
        /// <summary>
        /// Création d'un togglePanel
        /// </summary>
        /// <param name="pCurrentTable"></param>
        /// <param name="pCSSModeEnum"></param>
        /// <param name="pMainClass"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200928 [XXXXX] Déplacer ici en provenance de CustomObject
        public static WCTogglePanel CreateTogglePanel(Table pCurrentTable, Cst.CSSModeEnum pCSSModeEnum, string pMainClass)
        {
            #region Color
            Color backgroundColor = Color.Transparent;
            string mainClass = pCSSModeEnum.ToString() + " " + pMainClass;
            #endregion Color

            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-mainclass"]))
                mainClass = mainClass.Replace(pMainClass, pCurrentTable.Style["tblH-mainclass"]);

            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-bkgH-color"]))
                backgroundColor = Color.FromName(pCurrentTable.Style["tblH-bkgH-color"] + "!important");

            #region Title
            string clientId = string.Empty;
            string title = string.Empty;
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-titleR"]))//R Comme Resource
                title = Ressource.GetString(pCurrentTable.Style["tblH-titleR"]);
            else if (StrFunc.IsFilled(pCurrentTable.Style["tblH-title"]))
                title = pCurrentTable.Style["tblH-title"];
            #endregion Title

            #region GenericControl
            string cssClass = "size3";
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-css"]))
                cssClass = pCurrentTable.Style["tblH-css"];
            #endregion GenericControl

            bool isReverse = false;
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-reverse"]))
                isReverse = BoolFunc.IsTrue(pCurrentTable.Style["tblH-reverse"]);

            if (isReverse && StrFunc.IsEmpty(pCurrentTable.Style["tblH-mainclass"]))
                mainClass = pCSSModeEnum.ToString() + " gray";

            WCTogglePanel togglePanel = new WCTogglePanel(backgroundColor, title, cssClass, isReverse)
            {
                CssClass = mainClass
            };
            if (StrFunc.IsFilled(pCurrentTable.ID))
                togglePanel.ID = "div" + pCurrentTable.ID;
            if (StrFunc.IsFilled(clientId))
                togglePanel.ControlHeaderTitle.ID = clientId;
            return togglePanel;
        }
        /// <summary>
        /// Création d'un panel simple
        /// </summary>
        /// <param name="pCurrentTable"></param>
        /// <param name="pCSSModeEnum"></param>
        /// <param name="pMainClass"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200924 [XXXXX] Déplacer ici en provenance de CustomObject
        public static WCBodyPanel CreateBodyPanel(Table pCurrentTable, Cst.CSSModeEnum pCSSModeEnum, string pMainClass)
        {
            #region Color
            Color backgroundColor = Color.Transparent;
            Color borderColor = Color.Transparent;
            string mainClass = pCSSModeEnum.ToString() + " " + pMainClass;
            #endregion Color

            if (StrFunc.IsFilled(pCurrentTable.Style["tblB-mainclass"]))
                mainClass = mainClass.Replace(pMainClass, pCurrentTable.Style["tblB-mainclass"]);

            if (StrFunc.IsFilled(pCurrentTable.Style["tblB-bkg-color"]))
                backgroundColor = Color.FromName(pCurrentTable.Style["tblB-bkg-color"] + "!important");

            if (StrFunc.IsFilled(pCurrentTable.Style["tblB-border-color"]))
                borderColor = Color.FromName(pCurrentTable.Style["tblB-border-color"] + "!important");

            WCBodyPanel bodyPanel = new WCBodyPanel(mainClass, backgroundColor, borderColor)
            {
                CssClass = mainClass
            };
            if (StrFunc.IsFilled(pCurrentTable.ID))
                bodyPanel.ID = "div" + pCurrentTable.ID;

            return bodyPanel;
        }

        /// <summary>
        /// Supprime la class css {pClass}
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pClass"></param>
        /// FI 20200124 [XXXX] Add Method
        public static void RemoveCssClass(WebControl pControl, string pClass)
        {
            if (null == pControl)
                throw new ArgumentNullException("pControl is null");

            if (StrFunc.IsFilled(pControl.CssClass))
                pControl.CssClass = pControl.CssClass.Replace(pClass, string.Empty);
        }

        //EG 20160307 New 
        public static void RemoveStyleDisplay(Control pControl)
        {
            bool isOk = false;
            if (false == isOk)
            {
                try
                {
                    WebControl webctrl = (WebControl)pControl;
                    webctrl.Style.Remove("display");
                    webctrl.Style.Remove(HtmlTextWriterStyle.Display);
                    isOk = true;
                }
                catch { isOk = false; }
            }

            if (false == isOk)
            {
                try
                {
                    HtmlControl ctrl = (HtmlControl)pControl;
                    ctrl.Style.Remove("display");
                    ctrl.Style.Remove(HtmlTextWriterStyle.Display);
                    isOk = true;
                }
                catch { isOk = false; }
            }

            if (false == isOk)
            {
                throw new NotImplementedException("control is not a WebControl or control is not a HtmlControl, control type not implemented");
            }

        }

        #region DropDownList
        #region Load DropDownList
        /// <summary>
        ///  Chargement dans une DDL d'un Enum c#. Pour chaque Item
        ///  <para>
        ///  -  Text est alimenté :
        ///  </para>
        /// <para><b>si pIsResource</b></para> 
        /// <para>=> via l'attribut ResourceAttribut si renseigné (usage de pPrefixResource pour déterminer l'attribut)</para> 
        /// <para>=> via la resource "pResourcePrefix + enum" sinon</para> 
        /// <para><b>si !pIsResource</b></para> 
        /// <para>=> avec la valeur de l'enum</para>
        ///  <para>
        ///  - Value est alimenté avec la valeur de l'enum
        ///  </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsResource"></param>
        /// <param name="pResourcePrefix"></param>
        /// FI 20171025 [23533] Add
        /// FI 20190725 [XXXXX] Add pIsResource (Refactoring)
        public static void DDLLoadEnum<T>(DropDownList pddl, bool pWithEmpty, Boolean pIsResource, string pResourcePrefix) where T : struct
        {
            string forcedValue = string.Empty;
            DDLLoadEnum<T>(pddl, pWithEmpty, pIsResource, pResourcePrefix, forcedValue, false);
        }
        public static void DDLLoadEnum<T>(DropDownList pddl, bool pWithEmpty, Boolean pIsResource, string pResourcePrefix, string pForcedValue, bool pIsExcludeForcedValue) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            List<T> ret = new List<T>();

            IEnumerable<T> lstEnum =
                            from item in Enum.GetValues(typeof(T)).Cast<T>()
                            select item;

            bool isExistForcedValue = !string.IsNullOrEmpty(pForcedValue);
            
            foreach (T @value in lstEnum)
            {
                string text = @value.ToString();
                if (
                     (!isExistForcedValue)
                     ||
                     (
                       ((pIsExcludeForcedValue) && (pForcedValue.IndexOf(text) < 0))
                       ||
                       ((!pIsExcludeForcedValue) && (pForcedValue.IndexOf(text) >= 0))
                     )
                   )
                {
                    if (pIsResource)
                        text = ReflectionTools.GetEnumResource<T>(value as Enum, pResourcePrefix);

                    pddl.Items.Add(new ListItem(text, @value.ToString()));
                }
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }

        #region EurosysFeed
        //        public enum EurosysFeed_FileExtension
        //        {   
        //            DAT,PA2,PAR,LZH,TXT,ZIP
        //        }
        public static void DDLLoad_EurosysFeed_FileExtension(DropDownList pddl)
        {
            DDLLoad_EurosysFeed_FileExtension(pddl, false);
        }
        public static void DDLLoad_EurosysFeed_FileExtension(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem("." + Cst.EurosysFeed_FileExtension.DAT.ToString(), Cst.EurosysFeed_FileExtension.DAT.ToString()));
            pddl.Items.Add(new ListItem("." + Cst.EurosysFeed_FileExtension.LZH.ToString(), Cst.EurosysFeed_FileExtension.LZH.ToString()));
            pddl.Items.Add(new ListItem("." + Cst.EurosysFeed_FileExtension.PA2.ToString(), Cst.EurosysFeed_FileExtension.PA2.ToString()));
            pddl.Items.Add(new ListItem("." + Cst.EurosysFeed_FileExtension.PAR.ToString(), Cst.EurosysFeed_FileExtension.PAR.ToString()));
            pddl.Items.Add(new ListItem("." + Cst.EurosysFeed_FileExtension.TXT.ToString(), Cst.EurosysFeed_FileExtension.TXT.ToString()));
            pddl.Items.Add(new ListItem("." + Cst.EurosysFeed_FileExtension.ZIP.ToString(), Cst.EurosysFeed_FileExtension.ZIP.ToString()));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_EurosysFeed_YesNo(DropDownList pddl)
        {
            DDLLoad_EurosysFeed_YesNo(pddl, false);
        }
        public static void DDLLoad_EurosysFeed_YesNo(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString(Cst.EurosysFeed_YesNo.No.ToString()), Cst.EurosysFeed_YN.N.ToString()));
            pddl.Items.Add(new ListItem(Ressource.GetString(Cst.EurosysFeed_YesNo.Yes.ToString()), Cst.EurosysFeed_YN.Y.ToString()));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion EurosysFeed

        #region DDLLoad_ErrorOnLoad
        /// <summary>
        /// Ajoute un item "Error on load" dans la combo
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        public static void DDLLoad_ErrorOnLoad(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem("<Error on load!>", "Error on load"));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
            if (pWithEmpty)
            {
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                pddl.Items[0].Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
            }
        }
        #endregion DDLLoad_ErrorOnLoad

        #region DDLLoad_AccruedInterest
        public static void DDLLoad_AccruedInterest(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Compounding"), Cst.AccruedInt_Compounding));
            pddl.Items.Add(new ListItem(Ressource.GetString("Linear"), Cst.AccruedInt_Linear));
            pddl.Items.Add(new ListItem(Ressource.GetString("Native"), Cst.AccruedInt_Native));
            //pddl.Items.Add(new ListItem(Ressource.GetString("Prorata"),     Cst.AccruedInt_Prorata));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_AccruedInterestPeriod(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("PeriodAccrued"), Cst.AccruedIntPeriod_Accrued));
            pddl.Items.Add(new ListItem(Ressource.GetString("PeriodRemaining"), Cst.AccruedIntPeriod_Remaining));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_AccruedInterest

        #region DDLLoad_AllProcessType
        public static void DDLLoad_AllProcessType(DropDownList pddl)
        {
            DDLLoad_AllProcessType(pddl, false);
        }
        public static void DDLLoad_AllProcessType(DropDownList pddl, bool pWithEmpty)
        {
            Cst.ProcessTypeEnum process;
            foreach (string s in Enum.GetNames(typeof(Cst.ProcessTypeEnum)))
            {
                process = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), s);
                switch (process)
                {
                    case Cst.ProcessTypeEnum.USER:
                    case Cst.ProcessTypeEnum.WEBSESSION:
                    case Cst.ProcessTypeEnum.GATEBCS://Eurosys Gateway
                        break;
                    default:
                        pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
                        break;
                }
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_AllProcessType
        #region DDLLoad_Actor
        public static void DDLLoad_Actor(string pCS, DropDownList pddl)
        {
            DDLLoad_Actor(pCS, pddl, false, false, false, false);
        }
        public static void DDLLoad_Actor(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            DDLLoad_ActorWithRole(pCS, pddl, Actor.RoleActor.NONE, pWithEmpty, pIsAddALL, pIsUseSessionRestrict, pIsClear);
        }
        #endregion
        #region DDLLoad_ActorInvoiceBeneficiary
        // EG 20141020 [20442]
        public static void DDLLoad_ActorInvoiceBeneficiary(string pCS, DropDownList pDDL)
        {
            DDLLoad_ActorInvoiceBeneficiary(pCS, pDDL, false, false, false, false);
        }
        public static void DDLLoad_ActorInvoiceBeneficiary(string pCS, DropDownList pDDL, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            pIsUseSessionRestrict &= (false == SessionTools.IsSessionSysAdmin);

            StrBuilder sqlQueryRestrict = new StrBuilder();
            DataParameters parameters = new DataParameters();

            if (pIsUseSessionRestrict)
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                sqlQueryRestrict += srh.GetSQLActor(string.Empty, "ac.IDA");
                srh.SetParameter(pCS, sqlQueryRestrict.ToString(), parameters);
            }

            string sqlQuery = @"select ac.IDA, ac.IDENTIFIER" + DDLLoad_DisplayName(pCS, "ac", 25) + ",1 as SORT" + Cst.CrLf;
            sqlQuery += @"from dbo.ACTOR ac
            inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = 'ENTITY')" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQuery += sqlQueryRestrict;

            sqlQuery += SQLCst.UNION + Cst.CrLf;

            sqlQuery += @"select ac.IDA, ac.IDENTIFIER" + DDLLoad_DisplayName(pCS, "ac", 25) + ", 2 as SORT" + Cst.CrLf;
            sqlQuery += @"from dbo.ACTOR ac
            inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = 'CSS')
            inner join dbo.MARKET mk on (mk.IDA = ac.IDA)
            inner join dbo.ENTITYMARKET em on (em.IDM = mk.IDM)" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQuery += sqlQueryRestrict;

            if (pIsAddALL)
            {
                string actorAll = Ressource.GetString("InvoiceBeneficiary_ALL");
                sqlQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-1 as IDA" + "," + Cst.CrLf;
                sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + ",0 as SORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
            }
            sqlQuery += "order by SORT, IDENTIFIER" + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            DDLLoad(pDDL, "IDENTIFIER", "IDA", pIsClear, 0, null, pCS, qryParameters.Query, pWithEmpty, false, qryParameters.Parameters.GetArrayDbParameter());

            if (pIsAddALL && (pDDL.Items.Count == (pWithEmpty ? 3 : 2)))
            {
                pDDL.Items.RemoveAt((pWithEmpty ? 1 : 0));
            }

        }
        // EG 20141020 [20442] Actually not use (for the future only)
        public static void DDLLoad_ActorInvoiceBeneficiary(string pCS, PageBase pPage, DropDownList pDDL, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            pIsUseSessionRestrict &= (false == SessionTools.IsSessionSysAdmin);

            StrBuilder sqlQuery = new StrBuilder();
            StrBuilder sqlQueryRestrict = new StrBuilder();
            DataParameters parameters = new DataParameters();

            if (pIsUseSessionRestrict)
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                sqlQueryRestrict += srh.GetSQLActor(string.Empty, "ac.IDA");
                srh.SetParameter(pCS, sqlQueryRestrict.ToString(), parameters);
            }

            string sqlQueryEntity = @"from dbo.ACTOR ac
            inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = 'ENTITY')" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQueryEntity += sqlQueryRestrict;

            string sqlQueryCss = @"from dbo.ACTOR ac
            inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = 'CSS')
            inner join dbo.MARKET mk on (mk.IDA = ac.IDA)
            inner join dbo.ENTITYMARKET em on (em.IDM = mk.IDM)" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQueryCss += sqlQueryRestrict;

            string sqlQueryCustodian = @"from dbo.ACTOR ac
            inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = 'CUSTODIAN')
            inner join dbo.ENTITYMARKET em on (em.IDA_CUSTODIAN = ac.IDA)" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQueryCustodian += sqlQueryRestrict;

            string actorAll_Grp;

            actorAll_Grp = Ressource.GetString("InvoiceBeneficiary" + StrFunc.FirstUpperCase(RoleActor.ENTITY.ToString().ToLower()) + "_ALL_Grp");
            sqlQuery += @"select ac.IDA, ac.IDENTIFIER" + DDLLoad_DisplayName(pCS, "ac", 25) + "," + Cst.CrLf;
            sqlQuery += " ar.IDROLEACTOR as OPTGROUP, " + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT, 1 as OPTGROUPSORT" + Cst.CrLf;
            sqlQuery += sqlQueryEntity;
            sqlQuery += SQLCst.UNION + Cst.CrLf;
            actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL_Grp");
            sqlQuery += @"select ac.IDA, ac.IDENTIFIER" + DDLLoad_DisplayName(pCS, "ac", 25) + "," + Cst.CrLf;
            sqlQuery += " ar.IDROLEACTOR as OPTGROUP, " + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT, 3 as OPTGROUPSORT" + Cst.CrLf;
            sqlQuery += sqlQueryCss;
            sqlQuery += SQLCst.UNION + Cst.CrLf;
            actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL_Grp");
            sqlQuery += @"select ac.IDA, ac.IDENTIFIER" + DDLLoad_DisplayName(pCS, "ac", 25) + "," + Cst.CrLf;
            sqlQuery += " ar.IDROLEACTOR as OPTGROUP, " + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT, 4 as OPTGROUPSORT" + Cst.CrLf;
            sqlQuery += sqlQueryCustodian;


            if (pIsAddALL)
            {
                string actorAll;
                //Tous
                actorAll = Ressource.GetString("Actor" +
                    StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower())
                    + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower())
                    + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL_Grp");
                sqlQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-2 as IDA" + "," + Cst.CrLf;
                sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CSSCUSTODIAN' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT, 2 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.EXISTS + "(select 1 " + sqlQueryCss + ")" + Cst.CrLf;
                sqlQuery += SQLCst.AND + SQLCst.EXISTS + "(select 1 " + sqlQueryCustodian + ")" + Cst.CrLf;
                //Tous CSS
                actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL_Grp");
                sqlQuery += SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-3 as IDA," + DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CSS' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT, 3 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.EXISTS + "(select 1 " + sqlQueryCss + ")" + Cst.CrLf;
                //Tous CUSTODIAN
                actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL_Grp");
                sqlQuery += SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-4 as IDA," + DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CUSTODIAN' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT, 4 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.EXISTS + "(select 1 " + sqlQueryCustodian + ")";
            }

            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            DDLLoad(pPage, pDDL, "IDENTIFIER", "IDA", pIsClear, 0, null, pCS, qryParameters.Query, pWithEmpty, true, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion DDLLoad_InvoiceBeneficiary

        #region DDLLoad_ActorEntity
        public static void DDLLoad_ActorEntity(string pCS, DropDownList pddl)
        {
            DDLLoad_ActorEntity(pCS, pddl, false, false, false, false);
        }
        public static void DDLLoad_ActorEntity(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            DDLLoad_ActorWithRole(pCS, pddl, Actor.RoleActor.ENTITY, pWithEmpty, pIsAddALL, pIsUseSessionRestrict, pIsClear);
            //PL 20110719 Newness
            if (pIsAddALL && (pddl.Items.Count == (pWithEmpty ? 3 : 2)))
            {
                // FI 20201027 [XXXXX] Si pIsAddALL la valeur <toutes entités> est conservée (Elle peut être intéressante)
                // Exemple sur le menu "Traitements: Calcul échéanciers" le choix <toutes entités> permet d'obtenir des performances accrues 
                //Remove "ALL" if only one value exists. 
                //pddl.Items.RemoveAt((pWithEmpty ? 1 : 0));
                pddl.SelectedIndex = (pWithEmpty ? 2 : 1);
            }
            //PL 20150511 Newness
            if (pddl.Items.Count == 1)
            {
                pddl.Enabled = false;
            }
        }
        #endregion
        #region DDLLoad_ActorSettltOffice
        public static void DDLLoad_ActorSettltOffice(string pCS, DropDownList pddl)
        {
            DDLLoad_ActorSettltOffice(pCS, pddl, false, false, false, false);
        }
        public static void DDLLoad_ActorSettltOffice(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            DDLLoad_ActorWithRole(pCS, pddl, Actor.RoleActor.SETTLTOFFICE, pWithEmpty, pIsAddALL, pIsUseSessionRestrict, pIsClear);
        }
        #endregion
        #region DDLLoad_ActorMarginRequirementOffice
        public static void DDLLoad_ActorMarginRequirementOffice(string pCS, DropDownList pddl)
        {
            DDLLoad_ActorMarginRequirementOffice(pCS, pddl, false, false, false, false);
        }
        public static void DDLLoad_ActorMarginRequirementOffice(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            DDLLoad_ActorWithRole(pCS, pddl, RoleActor.MARGINREQOFFICE, pWithEmpty, pIsAddALL, pIsUseSessionRestrict, pIsClear);
        }
        #endregion

        #region private DDLLoad_ActorWithRole
        private static void DDLLoad_ActorWithRole(string pCS, DropDownList pddl, RoleActor pRoleActor, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            string actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(pRoleActor.ToString().ToLower() + "_ALL"));

            DataParameters parameters = new DataParameters();

            StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
            sqlQuery += "a.IDA,a.IDENTIFIER" + Cst.CrLf;
            //if (pIsAddALL)
            //{
            //PL 20140610 Newness
            sqlQuery += DDLLoad_DisplayName(pCS, "a", -1);
            //}
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;

            if (pRoleActor != RoleActor.NONE)//20090113 PL Add if() pour nouvelle méthode DDLLoad_Actor()
            {
                sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar on ar.IDA=a.IDA and ar.IDROLEACTOR =@IDROLEACTOR" + Cst.CrLf;
                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDROLEACTOR), pRoleActor);
            }

            if (pIsUseSessionRestrict && (false == SessionTools.IsSessionSysAdmin))
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                sqlQuery += srh.GetSQLActor(null, "a.IDA");
                srh.SetParameter(pCS, sqlQuery.ToString(), parameters);
            }

            if (pIsAddALL)
            {
                sqlQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-1 as IDA" + "," + Cst.CrLf;
                sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS);
            }
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            //FI 20120118 Le tri est désormais effectué par la combo pWithSort=true
            //Anciennement ce tri était effectué via SQL 
            //seulement avec Oracle les données sont bizarrement triés lorsqu'il existe une donnée qui commence par "<"
            //ex s'il existe une donnée <Tous>, elle apparaît avec les T ds la combo ??
            DDLLoad(pddl, "IDENTIFIER", "IDA", pIsClear, 0, null, pCS, qryParameters.Query, pWithEmpty, true, qryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion

        #region DDLLoad_AmountTypeType
        public static void DDLLoad_AmountType(string pCS, DropDownList pddl)
        {
            DDLLoad_AmountType(pCS, pddl, false);
        }
        public static void DDLLoad_AmountType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string SQLQuery = SQLCst.SELECT + "VALUE, EXTVALUE" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_AMOUNTTYPE.ToString();
            DDLLoad(pddl, "EXTVALUE", "VALUE", pCS, SQLQuery, pWithEmpty, true, null);
        }
        #endregion
        #region DDLLoad_BusinessCenter
        public static void DDLLoad_BusinessCenterId(string pCS, DropDownList pddl)
        {
            DDLLoad_BusinessCenterId(pCS, pddl, false);
        }
        public static void DDLLoad_BusinessCenterId(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string tableName = Cst.OTCml_TBL.BUSINESSCENTER.ToString();

            string SQLQuery = SQLCst.SELECT + tableName + ".IDBC" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(tableName, tableName);
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, tableName)).ToString();
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLOrderBy_Statistic(pCS, tableName);
            SQLQuery += ", " + tableName + ".IDBC";
            DDLLoad(pddl, "IDBC", "IDBC", pCS, SQLQuery, pWithEmpty, false, null);
        }
        public static void DDLLoad_BusinessCenter(string pCS, DropDownList pddl)
        {
            DDLLoad_BusinessCenter(pCS, pddl, false);
        }
        public static void DDLLoad_BusinessCenter(string pCS, DropDownList pddl, bool pWithEmpty)
        {

            string tableName = Cst.OTCml_TBL.BUSINESSCENTER.ToString();

            //PL 20120118
            //string colDisplay = DataHelper.SQLConcat(pCS, "DISPLAYNAME", "' - '", DataHelper.SQLIsNullChar(pCS, "DESCRIPTION", " "));
            string colDisplay = DataHelper.SQLConcat(pCS, "DISPLAYNAME", "' - '", DataHelper.SQLTruncstring(pCS, "DESCRIPTION", 40));


            string SQLQuery = SQLCst.SELECT + tableName + ".IDBC, " + colDisplay + " as DISPLAY" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(tableName, tableName);
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, tableName)).ToString();
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLOrderBy_Statistic(pCS, tableName);
            SQLQuery += ", DISPLAY";
            DDLLoad(pddl, "DISPLAY", "IDBC", pCS, SQLQuery, pWithEmpty, false, null);
        }
        #endregion DDLLoad_BusinessCenter
        #region DDLLoad_BuySell
        //
        public static void DDLLoad_BuySellType(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Buy"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Sell"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        //
        public static void DDLLoad_BuySellCur1Type(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Buydev1"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Selldev1"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        //
        public static void DDLLoad_PayerReceiverType(DropDownList pddl, bool pWithEmpty)
        {
            //Used by BulletPayment (20061213 PL)
            pddl.Items.Add(new ListItem(Ressource.GetString("Payer"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Receiver"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        // EG 20230526 [WI640] New : Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
        public static void DDLLoad_InvoicePayerReceiverType(DropDownList pddl, bool pWithEmpty)
        {
            //Used by BulletPayment (20061213 PL)
            pddl.Items.Add(new ListItem(Ressource.GetString("Beneficiary"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Payer"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        //PL 20180531 
        public static void DDLLoad_CreditDebit(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Credit"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#00AE00;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Debit"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0000;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_DepositWithdrawal(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Deposit"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Withdrawal"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_XXXYYYType(DropDownList pddl, bool pWithEmpty)
        {
            //Used by BulletPayment (20061213 PL)
            pddl.Items.Add(new ListItem(Ressource.GetString("marginRequirement_payment_payer"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            pddl.Items.Add(new ListItem(Ressource.GetString("marginRequirement_payment_receiver"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        //
        public static void DDLLoad_LendBorrowType(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Borrow"), BuySellEnum.BUY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Lend"), BuySellEnum.SELL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }

        public static void DDLLoad_BuySellFixType(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Buy"), ReflectionTools.ConvertEnumToString<FixML.Enum.SideEnum>(FixML.Enum.SideEnum.Buy)));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Sell"), ReflectionTools.ConvertEnumToString<FixML.Enum.SideEnum>(FixML.Enum.SideEnum.Sell)));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }

        #endregion DDLLoad_BuySell
        #region DDLLoad_BuyerSellerType
        public static void DDLLoad_BuyerSellerType(DropDownList pddl, bool pWithEmpty)
        {

            pddl.Items.Add(new ListItem(Ressource.GetString("Buyer"), "Buyer"));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#003082;");
            pddl.Items.Add(new ListItem(Ressource.GetString("Seller"), "Seller"));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:#AE0303;");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));

        }
        #endregion
        #region DDLLoad_PositionType
        //public static void DDLLoad_PositionType(HtmlSelect pddl)
        //{
        //    DDLLoad_PositionType(pddl, false);
        //}
        //public static void DDLLoad_PositionType(HtmlSelect pddl, bool pWithEmpty)
        //{
        //    foreach (string s in Enum.GetNames(typeof(Cst.LabelPositionEnum)))
        //    {
        //        pddl.Items.Add(new ListItem(Ressource.GetString("LABELPOS_" + s, s), s));
        //    }
        //    //
        //    if (pWithEmpty)
        //        pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        //}
        //public static void DDLLoad_PositionType(DropDownList pddl)
        //{
        //    DDLLoad_PositionType(pddl, false);
        //}
        public static void DDLLoad_PositionType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.LabelPositionEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("LABELPOS_" + s, s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        // EG 20150722 PositionDateType
        public static void DDLLoad_PositionDateType(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("POSITIONDATE_DTBUSINESS", false), "0"));
            pddl.Items.Add(new ListItem(Ressource.GetString("POSITIONDATE_DTSETTLT", false), "1"));
        }

        public static void DDLLoad_PositionOtcSideView(DropDownList pddl)
        {
            if (SessionTools.User.IsSessionGuest)
            {
                //UserWithLimitedRights
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTOR_CLIENT", false), "1"));
                pddl.Enabled = false;
            }
            else
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("DDL_All", false), "0"));
                pddl.Items.Add(new ListItem(Ressource.GetString("POSITION_DEALERSIDE", false), "1"));
                pddl.Items.Add(new ListItem(Ressource.GetString("POSITION_CUSTODIANSIDE", false), "2"));
            }
            //if (pWithEmpty)
            //    pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_PositionSideView(DropDownList pddl)
        {
            if (SessionTools.User.IsSessionGuest)
            {
                //UserWithLimitedRights
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTOR_CLIENT", false), "1"));
                pddl.Enabled = false;
            }
            else
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("DDL_All", false), "0"));
                pddl.Items.Add(new ListItem(Ressource.GetString("POSITION_TRADINGSIDE", false), "1"));
                pddl.Items.Add(new ListItem(Ressource.GetString("POSITION_CLEARINGSIDE", false), "2"));
            }
        }
        #endregion
        #region DDLLoad_ActorSideView
        public static void DDLLoad_ActorSideView(DropDownList pddl)
        {
            if (SessionTools.User.IsSessionGuest)
            {
                //UserWithLimitedRights
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTOR_CLIENT", false), "1"));
                pddl.Enabled = false;
            }
            else
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("DDL_All", false), "0"));
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTOR_TRADINGSIDE", false), "1"));
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTOR_CLEARINGSIDE", false), "2"));
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTOR_EXECUTINGSIDE", false), "3"));
            }
        }
        #endregion

        #region DDLLoad_AggregateDateTypeView
        public static void DDLLoad_AggregateDateTypeView(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("DATETYPE_PERIOD", false), "1"));
            pddl.Items.Add(new ListItem(Ressource.GetString("DATETYPE_DAYTODAY", false), "2"));
        }
        #endregion

        // CC 20170127 
        #region DDLLoad_FinancialProductTypeView
        public static void DDLLoad_FinancialProductTypeView(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(ProductTools.GroupProductEnum.Commodity.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.Commodity)));
            pddl.Items.Add(new ListItem(ProductTools.GroupProductEnum.ExchangeTradedDerivative.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.ExchangeTradedDerivative)));
            pddl.Items.Add(new ListItem(ProductTools.GroupProductEnum.OverTheCounter.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.OverTheCounter)));
            pddl.Items.Add(new ListItem(ProductTools.GroupProductEnum.Security.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.Security)));

        }
        #endregion

        #region DDLLoad_ActivityTypeView
        public static void DDLLoad_ActivityTypeView(DropDownList pddl)
        {
            if (SessionTools.License.licensee == "VALBURY")
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTIVITY_OTC", false), "2"));
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTIVITY_ETD", false), "1"));
            }
            else
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTIVITY_ETD", false), "1"));
                pddl.Items.Add(new ListItem(Ressource.GetString("ACTIVITY_OTC", false), "2"));
            }
        }
        #endregion

        #region DDLLoad_ResultsType
        public static void DDLLoad_ResultsType(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("DDL_All", false), "0"));
            pddl.Items.Add(new ListItem(Ressource.GetString("RESULTS_CBS", false), "1"));
            pddl.Items.Add(new ListItem(Ressource.GetString("RESULTS_ECS", false), "2"));
        }
        #endregion

        #region DDLLoad_PeriodReportType
        public static void DDLLoad_PeriodReportType(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("DAILY_PERIOD", false), "1"));
            pddl.Items.Add(new ListItem(Ressource.GetString("WEEKLY_PERIOD", false), "2"));
            pddl.Items.Add(new ListItem(Ressource.GetString("MONTHLY_PERIOD", false), "3"));
            pddl.Items.Add(new ListItem(Ressource.GetString("YEARLY_PERIOD", false), "4"));
            pddl.Items.Add(new ListItem(Ressource.GetString("OTHER_PERIOD", false), "5"));

        }
        #endregion DDLLoad_PeriodReportType

        #region DDLLoad_CalculationRule
        public static void DDLLoad_CalculationRule(DropDownList pddl)
        {
            DDLLoad_CalculationRule(pddl, false);
        }
        public static void DDLLoad_CalculationRule(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("EQUIVALENTPERIODICRATE"), Cst.CalculationRule_EQUIVALENTPERIODICRATE));
            pddl.Items.Add(new ListItem(Ressource.GetString("RELEVANTVALUE"), Cst.CalculationRule_RELEVANTVALUE));
            pddl.Items.Add(new ListItem(Ressource.GetString("SELFCOMPOUNDING"), Cst.CalculationRule_SELFCOMPOUNDING));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_CalculationRule

        #region DDLLoad_CheckModeEnum
        public static void DDLLoad_CheckModeEnum(DropDownList pddl)
        {
            DDLLoad_CheckModeEnum(pddl, false);
        }
        public static void DDLLoad_CheckModeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.CheckModeEnum)))
            {
                ListItem listItem = new ListItem(Ressource.GetString("CheckModeEnum_" + s), s);
                if (s == Cst.CheckModeEnum.Error.ToString())
                    listItem.Attributes.Add("style", "color:darkred");
                else if (s == Cst.CheckModeEnum.None.ToString())
                    listItem.Attributes.Add("style", "color:darkgreen");
                else if (s == Cst.CheckModeEnum.Warning.ToString())
                    listItem.Attributes.Add("style", "color:orange");
                
                pddl.Items.Add(listItem);

            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_ClearingHouse
        public static void DDLLoad_ClearingHouse(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            DataParameters parameters = new DataParameters();
            string actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower() + "_ALL"));

            StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT_DISTINCT);
            sqlQuery += "a.IDA,a.IDENTIFIER" + Cst.CrLf;
            //PL 20140610 Newness
            sqlQuery += DDLLoad_DisplayName(pCS, "a", 25);
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar " + SQLCst.ON + "(ar.IDA=a.IDA)" + Cst.CrLf;
            sqlQuery += SQLCst.AND + "(ar.IDROLEACTOR =" + DataHelper.SQLString(RoleActor.CSS.ToString()) + ")" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET.ToString() + " mk " + SQLCst.ON + "(mk.IDA=a.IDA)";

            if (pIsUseSessionRestrict && (false == SessionTools.IsSessionSysAdmin))
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                sqlQuery += srh.GetSQLActor(string.Empty, "a.IDA");
                srh.SetParameter(pCS, sqlQuery.ToString(), parameters);
            }

            if (pIsAddALL)
            {
                sqlQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-1 as IDA" + "," + Cst.CrLf;
                sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS);
            }

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            //FI 20120118 Le tri est désormais effectué par la combo pWithSort=true
            //Anciennement ce tri était effectué via SQL 
            //seulement avec Oracle les données sont bizarrement triés lorsqu'il existe une donnée qui commence par "<"
            //ex s'il existe une donnée <Tous>, elle apparaît avec les T ds la combo ??
            DDLLoad(pddl, "IDENTIFIER", "IDA", pIsClear, 0, null, pCS, queryParameters.Query, pWithEmpty, true, queryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion DDLLoad_ClearingHouse
        #region DDLLoad_CssCustodian
        //PL 20140724 TEST OPTGROUP
        //PL 20180403 Add condition on em.IDA_CUSTODIAN 
        // FI 20180424 [23871] Add parameter pDisplayNameMaxLen
        public static void DDLLoad_CssCustodian(string pCS, PageBase pPage, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear, Nullable<int> pDisplayNameMaxLen)
        {
            pIsUseSessionRestrict &= (false == SessionTools.IsSessionSysAdmin);

            // FI 20180424 [23871]  compatibilité ascendante => 25 : valeur défaut
            int displayNameMaxLen = 25;
            if (pDisplayNameMaxLen.HasValue)
                displayNameMaxLen = pDisplayNameMaxLen.Value;

            StrBuilder sqlQuery = new StrBuilder();
            StrBuilder sqlQueryRestrict = new StrBuilder();
            StrBuilder sqlQueryCSS = new StrBuilder();
            StrBuilder sqlQueryCustodian = new StrBuilder();
            DataParameters parameters = new DataParameters();

            if (pIsUseSessionRestrict)
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                sqlQueryRestrict += srh.GetSQLActor(string.Empty, "a.IDA");
                srh.SetParameter(pCS, sqlQueryRestrict.ToString(), parameters);
            }

            sqlQueryCSS += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlQueryCSS += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar on (ar.IDA=a.IDA)";
            sqlQueryCSS += " and (ar.IDROLEACTOR=" + DataHelper.SQLString(RoleActor.CSS.ToString()) + ")" + Cst.CrLf;
            sqlQueryCSS += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET.ToString() + " mk on (mk.IDA=a.IDA)" + Cst.CrLf;
            sqlQueryCSS += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ENTITYMARKET.ToString() + " em on (em.IDM=mk.IDM) and (em.IDA_CUSTODIAN is null)" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQueryCSS += sqlQueryRestrict;

            sqlQueryCustodian += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
            sqlQueryCustodian += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar on (ar.IDA=a.IDA)";
            sqlQueryCustodian += " and (ar.IDROLEACTOR=" + DataHelper.SQLString(RoleActor.CUSTODIAN.ToString()) + ")" + Cst.CrLf;
            sqlQueryCustodian += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ENTITYMARKET.ToString() + " em on (em.IDA_CUSTODIAN=a.IDA) and (em.IDA_CUSTODIAN is not null)" + Cst.CrLf;
            if (pIsUseSessionRestrict)
                sqlQueryCustodian += sqlQueryRestrict;

            string actorAll_Grp;

            actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL_Grp");
            sqlQuery += SQLCst.SELECT + "a.IDA,a.IDENTIFIER" + DDLLoad_DisplayName(pCS, "a", displayNameMaxLen) + "," + Cst.CrLf;
            sqlQuery += "ar.IDROLEACTOR as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT,2 as OPTGROUPSORT" + Cst.CrLf;
            sqlQuery += sqlQueryCSS;
            sqlQuery += SQLCst.UNION + Cst.CrLf;
            actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL_Grp");
            sqlQuery += SQLCst.SELECT + "a.IDA,a.IDENTIFIER" + DDLLoad_DisplayName(pCS, "a", displayNameMaxLen) + "," + Cst.CrLf;
            sqlQuery += "ar.IDROLEACTOR as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT,3 as OPTGROUPSORT" + Cst.CrLf;
            sqlQuery += sqlQueryCustodian;

            if (pIsAddALL)
            {
                string actorAll;
                //Tous
                actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL_Grp");

                sqlQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-1 as IDA" + "," + Cst.CrLf;
                //sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + ",'#0B6121' as FORECOLOR," + Cst.CrLf;
                sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CSSCUSTODIAN' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT,1 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.EXISTS + "(select 1" + sqlQueryCSS + ")" + Cst.CrLf;
                sqlQuery += SQLCst.AND + SQLCst.EXISTS + "(select 1" + sqlQueryCustodian + ")" + Cst.CrLf;
                //Tous CSS
                actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL_Grp");
                sqlQuery += SQLCst.UNION + Cst.CrLf;
                //sqlQuery += SQLCst.SELECT + "-2 as IDA," + DataHelper.SQLString(actorAll, "IDENTIFIER") + ",'#000000' as FORECOLOR," + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-2 as IDA," + DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CSS' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT,2 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.EXISTS + "(select 1" + sqlQueryCSS + ")" + Cst.CrLf;
                //Tous CUSTODIAN
                actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_ALL_Grp");
                sqlQuery += SQLCst.UNION + Cst.CrLf;
                //sqlQuery += SQLCst.SELECT + "-3 as IDA," + DataHelper.SQLString(actorAll, "IDENTIFIER") + ",'#0445B6' as FORECOLOR," + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-3 as IDA," + DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CUSTODIAN' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT,3 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.EXISTS + "(select 1" + sqlQueryCustodian + ")";
                // RD 20150810 Plantage en cas de DDL vide
                //Aucun
                actorAll = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + StrFunc.FirstUpperCase(RoleActor.CUSTODIAN.ToString().ToLower()) + "_NONE");
                actorAll_Grp = Ressource.GetString("Actor" + StrFunc.FirstUpperCase(RoleActor.CSS.ToString().ToLower()) + "_ALL_Grp");
                sqlQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                sqlQuery += SQLCst.SELECT + "-4 as IDA" + "," + Cst.CrLf;
                //sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + ",'#0B6121' as FORECOLOR," + Cst.CrLf;
                sqlQuery += DataHelper.SQLString(actorAll, "IDENTIFIER") + "," + Cst.CrLf;
                sqlQuery += "'CSSCUSTODIAN' as OPTGROUP," + DataHelper.SQLString(actorAll_Grp) + " as OPTGROUPTEXT,4 as OPTGROUPSORT" + Cst.CrLf;
                sqlQuery += DataHelper.SQLFromDual(pCS) + Cst.CrLf;
                sqlQuery += SQLCst.WHERE + SQLCst.NOT_EXISTS + "(select 1" + sqlQueryCSS + ")" + Cst.CrLf;
                sqlQuery += SQLCst.AND + SQLCst.NOT_EXISTS + "(select 1" + sqlQueryCustodian + ")" + Cst.CrLf;
            }

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            //FI 20120118 Le tri est désormais effectué par la combo pWithSort=true
            //Anciennement ce tri était effectué via SQL, mais sous Oracle les données sont bizarrement triées, lorsqu'il existe une donnée commençant par "<"
            //ex. s'il existe une donnée <Tous>, elle apparaît avec les T ds la combo ??
            DDLLoad(pPage, pddl, "IDENTIFIER", "IDA", pIsClear, 0, null, pCS, queryParameters.Query, pWithEmpty, true, queryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion DDLLoad_CssCustodian
        #region DDLLoad_DisplayName
        private static string DDLLoad_DisplayName(string pCS, string pAliasTable, int pMaxLen)
        {
            int maxLen = Math.Max(15, pMaxLen);
            string sqlQuery = "|| case when [Table].IDENTIFIER=[Table].DISPLAYNAME then ' '" + Cst.CrLf;
            sqlQuery += "when " + DataHelper.SQLLength(pCS, "[Table].DISPLAYNAME") + ">" + maxLen.ToString() + " then ' - ' || " + DataHelper.SQLSubstring(pCS, "[Table].DISPLAYNAME", 1, (maxLen - 3)) + " || '...'" + Cst.CrLf;
            sqlQuery += "else ' - ' || [Table].DISPLAYNAME end as IDENTIFIER" + Cst.CrLf;
            return sqlQuery.Replace("[Table]", pAliasTable);
        }
        #endregion DDLLoad_DisplayName
        #region DDLLoad_Color
        //public static void DDLLoad_Color(HtmlSelect pddl, bool pWithEmpty)
        public static void DDLLoad_Color(DropDownList pddl, bool pWithEmpty)
        {
            KnownColor enumColor = new KnownColor();
            Array Colors = Enum.GetValues(enumColor.GetType());
            ArrayList aColor = new ArrayList();

            foreach (object clr in Colors)
                if (!Color.FromKnownColor((KnownColor)clr).IsSystemColor)
                    aColor.Add(clr.ToString());

            pddl.DataSource = aColor;
            pddl.DataBind();
            for (int i = 0; i < pddl.Items.Count; i++)
            {
                pddl.Items[i].Attributes.Add("style", "color:" + pddl.Items[i].Text + ";background-color:" + pddl.Items[i].Text);
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Color

        #region DDLLoad_Country
        public static void DDLLoad_Country(string pCS, DropDownList pddl)
        {
            DDLLoad_Country(pCS, pddl, false);
        }
        public static void DDLLoad_Country(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string tableName = Cst.OTCml_TBL.COUNTRY.ToString();

            string SQLQuery = SQLCst.SELECT;
            //SQLQuery += tableName + ".ISO3166_ALPHA2,";
            //20051006 PL Nécessaire pour les codes XE, XU, ... qui ne dispose pas de codes ISO3166
            SQLQuery += DataHelper.SQLIsNull(pCS, tableName + ".ISO3166_ALPHA2", tableName + ".IDCOUNTRY", "ISO3166_ALPHA2") + "," + Cst.CrLf;
            SQLQuery += DataHelper.SQLIsNull(pCS, tableName + ".DESCRIPTION", tableName + ".DISPLAYNAME", "DESCRIPTION") + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(tableName, tableName);
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, tableName)).ToString();
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLOrderBy_Statistic(pCS, tableName);
            SQLQuery += ", DESCRIPTION";
            DDLLoad(pddl, "DESCRIPTION", "ISO3166_ALPHA2", pCS, SQLQuery, pWithEmpty, false, null);
        }
        public static void DDLLoad_Country_Country(string pCS, DropDownList pddl)
        {
            DDLLoad_Country_Country(pCS, pddl, false);
        }
        public static void DDLLoad_Country_Country(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string tableName = Cst.OTCml_TBL.COUNTRY.ToString();


            string sqlQuery = SQLCst.SELECT;
            //SQLQuery += tableName + ".ISO3166_ALPHA2,";
            //20051006 PL Nécessaire pour les codes XE, XU, ... qui ne dispose pas de codes ISO3166
            sqlQuery += DataHelper.SQLIsNull(pCS, tableName + ".ISO3166_ALPHA2", tableName + ".IDCOUNTRY", "ISO3166_ALPHA2") + "," + Cst.CrLf;
            sqlQuery += DataHelper.SQLIsNull(pCS, tableName + ".DESCRIPTION", tableName + ".DISPLAYNAME", "DESCRIPTION") + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            sqlQuery += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(tableName, tableName);

            SQLWhere sqlWhere = new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, tableName));
            sqlWhere.Append(tableName + ".COUNTRYTYPE=" + DataHelper.SQLString(Cst.CountryTypeEnum.COUNTRY.ToString()));
            sqlQuery += sqlWhere.ToString();

            sqlQuery += Cst.CrLf + OTCmlHelper.GetSQLOrderBy_Statistic(pCS, tableName);
            sqlQuery += ", DESCRIPTION";
            //
            DDLLoad(pddl, "DESCRIPTION", "ISO3166_ALPHA2", pCS, sqlQuery, pWithEmpty, false, null);
        }
        #endregion DDLLoad_Country
        #region DDLLoad_CountryType
        public static void DDLLoad_CountryType(DropDownList pddl)
        {
            DDLLoad_CountryType(pddl, false);
        }
        public static void DDLLoad_CountryType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.CountryTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_CountryType
        #region DDLLoad_MarketType
        public static void DDLLoad_MarketType(DropDownList pddl)
        {
            DDLLoad_MarketType(pddl, false);
        }
        public static void DDLLoad_MarketType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MarketTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MarketType
        #region DDLLoad_CSSColor
        /// <summary>
        /// Chargement des couleurs de style disponibles en lieu et place des feuille de style
        /// </summary>
        /// <param name="pddl"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void DDLLoad_CSSColor(DropDownList pddl)
        {
            DDLLoad_CSSColor(pddl, false);
        }
        /// <summary>
        /// Chargement des couleurs de style disponibles en lieu et place des feuille de style
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void DDLLoad_CSSColor(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem("Bleu", "blue"));
            pddl.Items.Add(new ListItem("Cyan", "cyan"));
            pddl.Items.Add(new ListItem("Gris", "gray"));
            pddl.Items.Add(new ListItem("Vert", "green"));
            pddl.Items.Add(new ListItem("Jaune", "yellow"));
            pddl.Items.Add(new ListItem("Marron", "marron"));
            pddl.Items.Add(new ListItem("Orange", "orange"));
            pddl.Items.Add(new ListItem("Rose", "rose"));
            pddl.Items.Add(new ListItem("Rouge", "red"));
            pddl.Items.Add(new ListItem("Violet", "violet"));

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));

            for (int i = 0; i < pddl.Items.Count; i++)
            {
                try
                {
                    string text = pddl.Items[i].Value;
                    string colorName = Color.FromName(text).Name.ToLower();
                    switch (colorName)
                    {
                        case "blue":
                            colorName = CstCSSColor.blue;
                            break;
                        case "brown":
                            colorName = CstCSSColor.marron;
                            break;
                        case "cyan":
                            colorName = CstCSSColor.cyan;
                            break;
                        case "gray":
                            colorName = CstCSSColor.gray;
                            break;
                        case "green":
                            colorName = CstCSSColor.green;
                            break;
                        case "orange":
                            colorName = CstCSSColor.orange;
                            break;
                        case "red":
                            colorName = CstCSSColor.red;
                            break;
                        case "rose":
                            colorName = CstCSSColor.rose;
                            break;
                        case "violet":
                            colorName = CstCSSColor.violet;
                            break;
                        case "yellow":
                            colorName = CstCSSColor.yellow;
                            break;
                    }
                    pddl.Items[i].Attributes.Add("style", "color:" + colorName + ";");
                }
                catch { }
            }
        }
        #endregion DDLLoad_CSSColor
        #region DDLLoad_CtrlLastUser
        public static void DDLLoad_CtrlLastUser(DropDownList pddl)
        {
            DDLLoad_CtrlLastUser(pddl, false);
        }
        public static void DDLLoad_CtrlLastUser(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.CtrlLastUserEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_CtrlLastUser
        #region DDLLoad_CtrlStatusWeight
        public static void DDLLoad_CtrlStatus(DropDownList pddl)
        {
            DDLLoad_CtrlStatus(pddl, false);
        }
        public static void DDLLoad_CtrlStatus(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.CtrlStatusEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_CtrlStatusWeight
        #region DDLLoad_CtrlSending
        public static void DDLLoad_CtrlSending(DropDownList pddl)
        {
            DDLLoad_CtrlSending(pddl, false);
        }
        public static void DDLLoad_CtrlSending(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.CtrlSendingEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_CtrlSending
        #region DDLLoad_Currency
        public static void DDLLoad_Currency(string pCS, DropDownList pddl)
        {
            DDLLoad_Currency(pCS, pddl, false, null);
        }
        public static void DDLLoad_Currency(string pCS, DropDownList pddl, bool pIsWithEmpty)
        {
            DDLLoad_Currency(pCS, pddl, pIsWithEmpty, null);
        }
        public static void DDLLoad_Currency(string pCS, DropDownList pddl, bool pIsWithEmpty, string pMisc)
        {
            string tableName = Cst.OTCml_TBL.CURRENCY.ToString();
            //
            string SQLQuery = SQLCst.SELECT + tableName + ".ISO4217_ALPHA3," + tableName + ".IDC" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(tableName, tableName);
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, tableName)).ToString();
            SQLQuery += Cst.CrLf + OTCmlHelper.GetSQLOrderBy_Statistic(pCS, tableName);
            SQLQuery += ", " + tableName + ".IDC";
            //
            DDLLoad(pddl, "IDC", "ISO4217_ALPHA3", pCS, SQLQuery, pIsWithEmpty, pMisc, false, null);
            //
            if (!pIsWithEmpty)
                DDLSelectByValue(pddl, SystemSettings.GetAppSettings("Spheres_ReferentialDefault_currency"));
        }
        #endregion DDLLoad_Currency
        #region DDLLoad_CustomisedInput
        #region Comment DDLLoad_CustomisedInput
        /// <creation>
        ///     <version>1.0.4</version><date>???</date><author>MSK</author>
        /// </creation>
        /// <revision>
        ///     <version>1.0.4</version><date>20041203</date><author>MSK</author>
        ///     <comment>
        ///     Mise à jour de la fonction pour prendre en compte les nouveaux chemins des fichiers xml
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>
        /// <summary>
        /// Charger une dropdown avec une liste d'écrans de saisie
        /// </summary>
        /// <returns>void</returns>

        #endregion Comment DDLLoad_CustomisedInput
        //PL 20190725 Unused overloads
        //public static void DDLLoad_CustomisedInput(string pCS, DropDownList pddl)
        //{
        //    DDLLoad_CustomisedInput(pCS, pddl, 0, false, null);
        //}
        //public static void DDLLoad_CustomisedInput(string pCS, DropDownList pddl, bool pWithEmpty)
        //{
        //    DDLLoad_CustomisedInput(pCS, pddl, 0, pWithEmpty, null);
        //}
        //public static void DDLLoad_CustomisedInput(string pCS, DropDownList pddl, int pIdI)
        //{
        //    DDLLoad_CustomisedInput(pCS, pddl, pIdI, false, null);
        //}
        //PL 20190725 Unused pDataTable
        //public static void DDLLoad_CustomisedInput(string pCS, DropDownList pddl, int pIdI, bool pWithEmpty, DataTable pDataTable)  
        public static void DDLLoad_CustomisedInput(string pCS, DropDownList pddl, int pIdI, bool pWithEmpty)
        {
            //TODO A charger à partir du fichier ConfigCapture.xml avec:
            //     -text  = valeur de l'attribut name + ' - ' + valeur de l'attribut caption
            //     -value = valeur de l'attribut name
            //     et ce uniquement pour les valeurs de l'attribut product = pProductIdentifier

            //eg:
            //<Screen name="QuickCapture" Caption="Quick Capture" product="Swap">

            string productIdentifier = string.Empty;
            string customTradePath = Cst.CustomTradePath;
            if ((pIdI) > 0)
            {
                #region Identification du path où se trouve les fichiers XML
                SQL_Instrument sqlInstr = new SQL_Instrument(pCS, pIdI);
                if (sqlInstr.LoadTable(new string[] { "GPRODUCT", "PRODUCT_IDENTIFIER" }))
                {
                    productIdentifier = sqlInstr.Product_Identifier;
                    if (Cst.ProductGProduct_ADM == sqlInstr.GProduct)
                        customTradePath = Cst.CustomTradeAdminPath;
                    else if (Cst.ProductGProduct_RISK == sqlInstr.GProduct)
                        customTradePath = Cst.CustomTradeRiskPath;
                }
                #endregion
            }

            XmlDocument docXml = new XmlDocument();
            string xmlScreenPath = HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"] + customTradePath + @"\Screens" + @"\" + productIdentifier;
            string[] xmlFiles;
            try
            {
                //Si le répertoire n'existe pas, la méthode GetFiles génère une erreur DirectoryNotFoundException
                xmlFiles = Directory.GetFiles(xmlScreenPath, "*.xml");
            }
            catch (DirectoryNotFoundException)
            {
                xmlFiles = null;
            }
            if (ArrFunc.IsFilled(xmlFiles))
            {
                foreach (string xmlFile in xmlFiles)
                {
                    docXml.Load(xmlFile);

                    //Predicat string-length(@product)>0 pour ne pas afficher les subscreen dans la ddl
                    XmlNodeList ElementList = docXml.SelectNodes("/Screens/Screen[string-length(@product)>0]");
                    foreach (System.Xml.XmlNode node in ElementList)
                    {
                        if (null != pddl)
                        {
                            //PL 20190725 
                            //pddl.Items.Add(new ListItem(node.Attributes.GetNamedItem("caption").Value, node.Attributes.GetNamedItem("name").Value));
                            pddl.Items.Add(
                                new ListItem(node.Attributes.GetNamedItem("name").Value + " - " + node.Attributes.GetNamedItem("caption").Value,
                                             node.Attributes.GetNamedItem("name").Value)
                                           );
                        }
                        //PL 20190725 Unused
                        //if (null != pDataTable)
                        //{
                        //    object[] rowVals = new object[3];
                        //    rowVals[0] = node.Attributes.GetNamedItem("name").Value;
                        //    rowVals[1] = node.Attributes.GetNamedItem("name").Value;
                        //    rowVals[2] = node.Attributes.GetNamedItem("name").Value;
                        //    rowVals[3] = node.Attributes.GetNamedItem("caption").Value;
                        //    pDataTable.Rows.Add(rowVals);
                        //}
                    }
                }
            }

            if (null != pddl)
            {
                pddl.Items.Insert(0, new ListItem("Full", "Full"));

                if (pWithEmpty)
                {
                    pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                }
            }

        }
        #endregion DDLLoad_CustomisedInput
        #region DDLLoad_Culture
        public static void DDLLoad_Culture(DropDownList pddl, bool pAllCulture)
        {
            DDLLoad_Culture(pddl, pAllCulture, false);
        }
        public static void DDLLoad_Culture(DropDownList pddl, bool pAllCulture, bool pWithEmpty)
        {
            SortedList sl = new SortedList();
            //Langues existantes
            foreach (CultureInfo cu in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                if (cu.Name == "en-GB" ||
                    cu.Name == "fr-BE" || cu.Name == "fr-FR" || cu.Name == "fr-LU" ||
                    cu.Name == "it-IT" ||
                    cu.Name == "es-ES")
                {
                    try { sl.Add(cu.DisplayName, cu.Name); }
                    catch { }
                }
            }
            foreach (string key in sl.Keys)
            {
                pddl.Items.Add(new ListItem(key, sl[key].ToString()));
            }

            if (pAllCulture)
            {
                pddl.Items.Add(new ListItem(null, Cst.CULTURE_SEPARATOR));
                //Autres langues
                sl = new SortedList();
                foreach (CultureInfo cu in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if ((!cu.IsNeutralCulture) && (!sl.Contains(cu.DisplayName)))
                    {
                        try { sl.Add(cu.DisplayName, cu.Name); }
                        catch { }
                    }
                }
                foreach (string key in sl.Keys)
                {
                    pddl.Items.Add(new ListItem(key, sl[key].ToString()));
                }
            }

            if (pWithEmpty)
            {
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            }
        }
        #endregion DDLLoad_Culture
        #region DDLLoad_CutName
        public static void DDLLoad_CutName(string pCS, DropDownList pddl)
        {
            DDLLoad_CutName(pCS, pddl, false);
        }
        public static void DDLLoad_CutName(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "CutName");
        }
        #endregion CutName
        #region DDLLoad_InputSourceDataStyle
        public static void DDLLoad_InputSourceDataStyle(DropDownList pddl)
        {
            DDLLoad_InputSourceDataStyle(pddl, false);
        }
        public static void DDLLoad_InputSourceDataStyle(DropDownList pddl, bool pWithEmpty)
        {
            // PM 20181206 [XXXXX] Ajout tri de la liste
            //foreach (string s in Enum.GetNames(typeof(Cst.InputSourceDataStyle)))
            //{
            //    pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            //}
            ListItem[] enumItems = Enum.GetNames(typeof(Cst.InputSourceDataStyle)).ToList().Select(s => new ListItem(Ressource.GetString(s), s)).OrderBy(s => s.Text).ToArray(); ;
            pddl.Items.AddRange(enumItems);
            if (pWithEmpty)
            {
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            }
        }
        #endregion
        #region DDLLoad_OutputSourceDataStyle
        public static void DDLLoad_OutputSourceDataStyle(DropDownList pddl)
        {
            DDLLoad_OutputSourceDataStyle(pddl, false);
        }
        public static void DDLLoad_OutputSourceDataStyle(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.OutputSourceDataStyle)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_OutputTargetDataStyle
        public static void DDLLoad_OutputTargetDataStyle(DropDownList pddl)
        {
            DDLLoad_OutputTargetDataStyle(pddl, false);
        }
        public static void DDLLoad_OutputTargetDataStyle(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.OutputTargetDataStyle)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_IoSerializeMode
        public static void DDLLoad_IoSerializeMode(DropDownList pddl)
        {
            DDLLoad_IoSerializeMode(pddl, false);
        }
        public static void DDLLoad_IoSerializeMode(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.IOSerializeMode)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_DataType
        public static void DDLLoad_DataType(DropDownList pddl)
        {
            DDLLoad_RightType(pddl, false);
        }
        public static void DDLLoad_DataType(DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_DataType(pddl, pWithEmpty, string.Empty);
        }
        public static void DDLLoad_DataType(DropDownList pddl, bool pWithEmpty, string pFiltre)
        {
            // 20081113 RD : DDL avec Filtre dans le Referential
            ArrayList aFiltre = null;
            //
            if (StrFunc.IsFilled(pFiltre))
            {
                string[] list = pFiltre.Split(",".ToCharArray());
                if (ArrFunc.IsFilled(list))
                    aFiltre = new ArrayList(list);
            }
            //
            foreach (string s in Enum.GetNames(typeof(TypeData.TypeDataEnum)))
            {
                bool isToAdd = true;
                if ((null != aFiltre) && aFiltre.Contains(s))
                    isToAdd = false;
                //
                if (isToAdd)
                {
                    TypeData.TypeDataEnum type = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), s, false);
                    if (TypeData.TypeDataEnum.unknown != type)
                    {
                        //20060622 PL Suppression de l'appel à Ressource.GetString()
                        //pddl.Items.Add(new ListItem(Ressource.GetString(type.ToString(),true),type.ToString()));
                        pddl.Items.Add(new ListItem(type.ToString(), type.ToString()));
                    }
                }
            }
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_DataType
        #region DDLLoad_WebControlType
        public static void DDLLoad_WebControlType(DropDownList pddl)
        {
            DDLLoad_WebControlType(pddl, false);
        }
        public static void DDLLoad_WebControlType(DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_WebControlType(pddl, pWithEmpty, string.Empty);
        }
        public static void DDLLoad_WebControlType(DropDownList pddl, bool pWithEmpty, string pFiltre)
        {
            DDLLoad_WebControlType(pddl, pWithEmpty, pFiltre, string.Empty);
        }
        public static void DDLLoad_WebControlType(DropDownList pddl, bool pWithEmpty, string pFiltre, string pObjectsPath)
        {
            //*************************************************//
            // Pour ne pas charger les objets évolués EFS
            //      pObjectsPath = string.Empty;
            // pour charger les objets évolués EFS, 
            //      il suffit de supprimer la ligne suivante 
            //***********************************************//
            pObjectsPath = string.Empty;
            //***********************************************//
            //
            ArrayList aFiltre = null;
            //
            if (StrFunc.IsFilled(pFiltre))
            {
                string[] list = pFiltre.Split(",".ToCharArray());
                if (ArrFunc.IsFilled(list))
                    aFiltre = new ArrayList(list);
            }
            ListItem item;
            //
            foreach (string s in Enum.GetNames(typeof(WebControlType.WebControlTypeEnum)))
            {
                bool isToAdd = true;
                if ((null != aFiltre) && aFiltre.Contains(s))
                    isToAdd = false;
                //
                if (isToAdd)
                {
                    WebControlType.WebControlTypeEnum type = (WebControlType.WebControlTypeEnum)Enum.Parse(typeof(WebControlType.WebControlTypeEnum), s, false);
                    if (WebControlType.WebControlTypeEnum.Unknown != type)
                    {
                        item = new ListItem(Ressource.GetString("WCType_" + type.ToString()), type.ToString());
                        //item.Attributes.Add("style", "background-color:" + Color.DarkGray.Name + ";");
                        pddl.Items.Add(item);
                    }
                }
            }
            //
            if (StrFunc.IsFilled(pObjectsPath))
            {
                XmlDocument simpleObjects = new XmlDocument();
                //
                string filePath = pObjectsPath + "shared-SimpleObjects.xml";
                simpleObjects.Load(filePath);
                //
                foreach (XmlNode simpleObject in simpleObjects.SelectNodes("Objects/Object"))
                {
                    string s = simpleObject.Attributes.GetNamedItem("name").Value;
                    string description = string.Empty;
                    XmlNode documentation = simpleObject.SelectSingleNode("documentation");
                    if (null != documentation)
                        description = documentation.InnerText;
                    //
                    bool isToAdd = true;
                    if ((null != aFiltre) && aFiltre.Contains(s))
                        isToAdd = false;
                    //
                    if (isToAdd)
                    {
                        item = new ListItem(s + (StrFunc.IsFilled(description) ? " - " + description : string.Empty), s);
                        //item.Attributes.Add("style", "background-color:" + Color.DarkGray.Name + ";");
                        pddl.Items.Add(item);
                    }
                }
            }
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_DataType
        #region DDLLoad_Day
        public static void DDLLoad_Day(DropDownList pddl)
        {
            DDLLoad_Day(pddl, false);
        }
        public static void DDLLoad_Day(DropDownList pddl, bool pWithEmpty)
        {
            for (int i = 1; i <= 31; i++)
            {
                pddl.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Day
        #region DDLLoad_DefaultPage
        public static void DDLLoad_DefaultPage(DropDownList pddl)
        {
            if (Software.IsSoftwareOTCmlOrFnOml())
            {
                string menu;
                menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));

                menu = IdMenu.GetIdMenu(IdMenu.Menu.ViewTradeOTCOrder);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));
                menu = IdMenu.GetIdMenu(IdMenu.Menu.ViewTradeOTCAlloc);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));
                menu = IdMenu.GetIdMenu(IdMenu.Menu.ViewTradeFnOOrder);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));
                menu = IdMenu.GetIdMenu(IdMenu.Menu.ViewTradeFnOAlloc);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));

                menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));
                menu = IdMenu.GetIdMenu(IdMenu.Menu.ViewTradeAdmin);
                pddl.Items.Add(new ListItem(Ressource.GetMenu_Fullname(menu), menu));
            }
            pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_DefaultPage
        #region DDLLoad_DeterminationMethodValuation
        /// EG 20140702 New
        public static void DDLLoad_DeterminationMethodValuation(DropDownList pddl, bool pWithEmpty)
        {
            LoadEnumArguments loadEnumArg = LoadEnumArguments.GetArguments("[code:DeterminationMethod]", pWithEmpty);
            DDLLoad_ENUM(pddl, SessionTools.CS, loadEnumArg);
        }
        #endregion DDLLoad_DeterminationMethodValuation
        #region DDLLoad_DeterminationMethodValuationPrice
        /// EG 20140702 New
        public static void DDLLoad_DeterminationMethodValuationPrice(DropDownList pddl, bool pWithEmpty)
        {
            LoadEnumArguments loadEnumArg =
                LoadEnumArguments.GetArguments("[code:DeterminationMethod;forcedEnum:CalculationAgent|ClosingPrice|HedgeExecution|ValuationTime]", pWithEmpty);
            DDLLoad_ENUM(pddl, SessionTools.CS, loadEnumArg);
        }
        #endregion DDLLoad_DeterminationMethodValuationPrice

        /// <summary>
        ///  chgt de la combo avec les type de message actif(*) dans Sphere®
        ///  <para>(*) pour lesquels il existe des messages enabled</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsResource"></param>
        /// FI 20181002 [24219] Add method
        public static void DDLLoad_CnfTypeMessage(string pCS, DropDownList pddl, Boolean pIsClear, Boolean pWithEmpty, Boolean pIsAddALL, Boolean pIsResource)
        {
            string sql = @"select distinct enum.VALUE as Value, enum.VALUE as Text 
                            from dbo.ENUM enum
                            inner join dbo.CNFMESSAGE cnfMsg on cnfMsg.CNFTYPE = enum.VALUE
                            and getdate() >= cnfMsg.DTENABLED
                            and ((cnfMsg.DTDISABLED is null) or (cnfMsg.DTDISABLED > getdate()))
                            where enum.CODE = 'NotificationTypeScheme'
                            and enum.VALUE in ('ALLOCATION','POSITION','POSACTION','POSSYNTHETIC','FINANCIAL','FINANCIALPERIODIC','SYNTHESIS')";

            DataParameters dp = new DataParameters();
            QueryParameters qryParameters = new QueryParameters(pCS, sql, dp);

            DDLLoad(pddl, "TEXT", "value", pIsClear, pCS, qryParameters.Query, pWithEmpty, string.Empty, pIsResource, qryParameters.Parameters.GetArrayDbParameter());

            if (pIsAddALL)
                DropDownTools.DDLAddMiscItem(pddl, "isAddAll: true");

            if (pIsAddALL && (pddl.Items.Count == (pWithEmpty ? 3 : 2)))
            {
                //Remove "ALL" if only one value exists. 
                pddl.Items.RemoveAt((pWithEmpty ? 1 : 0));
            }
            if (pddl.Items.Count == 1)
            {
                pddl.Enabled = false;
            }
        }

        #region DDLLoad_ParameterDirection
        public static void DDLLoad_ParameterDirection(DropDownList pddl)
        {
            DDLLoad_ParameterDirection(pddl, false);
        }
        public static void DDLLoad_ParameterDirection(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(ParameterDirection)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("Param" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_ExerciseStyle
        public static void DDLLoad_ExerciseStyle(string pCS, DropDownList pddl)
        {
            DDLLoad_ExerciseStyle(pCS, pddl, false);
        }
        public static void DDLLoad_ExerciseStyle(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "ExerciseStyleEnum");
        }
        #endregion DDLLoad_ExerciseStyle
        #region DDLLoad_ExchangeType
        public static void DDLLoad_ExchangeType(DropDownList pddl)
        {
            DDLLoad_ExchangeType(pddl, false);
        }
        public static void DDLLoad_ExchangeType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(EfsML.Enum.ExchangeTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(typeof(EfsML.Enum.ExchangeTypeEnum).Name + "_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion

        #region DDLLoad_EarExchangeEnum
        public static void DDLLoad_EarExchangeEnum(DropDownList pddl)
        {
            DDLLoad_EarExchangeEnum(pddl, false);
        }
        public static void DDLLoad_EarExchangeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.EarExchangeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(typeof(Cst.EarExchangeEnum).Name + "_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_Earcomon
        public static void DDLLoad_EarcomonEnum(DropDownList pddl)
        {
            DDLLoad_EarcomonEnum(pddl, false);
        }
        public static void DDLLoad_EarcomonEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.EarCommonEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(typeof(Cst.EarCommonEnum).Name + "_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_CBExchangeType
        public static void DDLLoad_CBExchangeType(DropDownList pddl)
        {
            DDLLoad_CBExchangeType(pddl, false);
        }
        public static void DDLLoad_CBExchangeType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(EfsML.Enum.CBExchangeTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(typeof(EfsML.Enum.CBExchangeTypeEnum).Name + "_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_ExtllinkTables
        //		public static void DDLLoad_ExtllinkTables(DropDownList pddl) 
        //		{
        //			DDLLoad_ExtllinkTables(pddl, false);		
        //		}
        //		public static void DDLLoad_ExtllinkTables (DropDownList pddl, bool pWithEmpty) 
        //		{           
        //warning oracle: Utilisation de sysobjects et syscolumns
        //			string SQLQuery = SQLCst.SELECT + "so.name as NAME, so.name as EXTNAME" + Cst.CrLf;
        //			SQLQuery += SQLCst.FROM_DBO + "sysobjects so" + Cst.CrLf;
        //			SQLQuery += SQLCst.INNERJOIN_DBO + "syscolumns sc on (sc.id = so.id and sc.name = 'EXTLLINK')" + Cst.CrLf;            
        //			SQLWhere SQLWhere_ = new SQLWhere( "so.xtype = 'U'");
        //			SQLWhere_.Append("so.name not like '%_P'");
        //			//SQLWhere_.Append("so.NAME not like '%_L'");
        //			SQLWhere_.Append("so.name not like '%_S'");
        //			SQLQuery += SQLWhere_.ToString();
        //			SQLQuery += SQLCst.ORDERBY + "so.name" + Cst.CrLf;
        //			DDLLoad(pddl, "EXTNAME", "NAME", SessionTools.CS, SQLQuery, pWithEmpty, null);
        //		}			
        #endregion DDLLoad_ExtllinkTables
        #region DDLLoad_ExtlType
        public static void DDLLoad_ExtlType(DropDownList pddl)
        {
            DDLLoad_ExtlType(pddl, false);
        }
        public static void DDLLoad_ExtlType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.ExtlType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_FxBarrierType
        public static void DDLLoad_FxBarrierType(string pCS, DropDownList pddl)
        {
            DDLLoad_FxBarrierType(pCS, pddl, false);
        }
        public static void DDLLoad_FxBarrierType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "FxBarrierTypeEnum");
        }
        #endregion DDLLoad_FxBarrierType
        #region DDLLoad_FxCurve
        public static void DDLLoad_FxCurve(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Default"), "Default"));
        }
        #endregion DDLLoad_FxCurve
        #region DDLLoad_FxMTMMethod
        public static void DDLLoad_FxMTMMethod(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("FxMTMMethod_MTM"), Cst.FxMTMMethod_MTM));
            pddl.Items.Add(new ListItem(Ressource.GetString("FxMTMMethod_Discount"), Cst.FxMTMMethod_Discount));
            pddl.Items.Add(new ListItem(Ressource.GetString("FxMTMMethod_LinearDepreciation"), Cst.FxMTMMethod_LinearDepreciation));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_FxMTMMethod

        #region DDLGlobal_Elementary
        public static void DDLLoad_Global_Elementary(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(GlobalElementaryEnum)))
            {
                if (s == EfsML.Enum.GlobalElementaryEnum.Full.ToString())
                    pddl.Items.Add(new ListItem(Ressource.GetString(GlobalElementaryEnum.Global.ToString() + "_" + GlobalElementaryEnum.Elementary.ToString()), s));
                else
                    pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLGlobal_Elementary
        #region DDLLoad_HolidayName
        public static void DDLLoad_HolidayName(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.HolidayName)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_HolidayName
        #region DDLLoad_HolidayType
        public static void DDLLoad_HolidayType(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.HolidayType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_HolidayType
        #region DDLLoad_HolidayMethodOfAdjustment
        public static void DDLLoad_HolidayMethodOfAdjustment(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.HolidayMethodOfAdjustment)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_HolidayType
        #region DDLLoad_IndexUnit
        public static void DDLLoad_IndexUnit(DropDownList pddl)
        {
            DDLLoad_IndexUnit(pddl, false);
        }
        public static void DDLLoad_IndexUnit(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("CurrencyAmount"), Cst.IdxUnit_currency));
            pddl.Items.Add(new ListItem(Ressource.GetString("Degrees"), Cst.IdxUnit_degrees));
            pddl.Items.Add(new ListItem(Ressource.GetString("Percent"), Cst.IdxUnit_percent));
            pddl.Items.Add(new ListItem(Ressource.GetString("NA"), Cst.IdxUnit_NA));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_IndexUnit
        #region DDLLoad_InformationProvider
        public static void DDLLoad_InformationProvider(string pCS, DropDownList pddl)
        {
            DDLLoad_InformationProvider(pCS, pddl, false);
        }
        public static void DDLLoad_InformationProvider(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "InformationProvider");
        }
        #endregion DDLLoad_InformationProvider
        #region DDLLoad_In_Out
        public static void DDLLoad_In_Out(DropDownList pddl)
        {
            DDLLoad_In_Out(pddl, false);
        }
        public static void DDLLoad_In_Out(DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_In_Out(pddl, pWithEmpty, false);
        }
        public static void DDLLoad_In_Out(DropDownList pddl, bool pWithEmpty, bool pIsAddALL)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.In_Out)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            //
            if (pIsAddALL)
                pddl.Items.Insert(0, new ListItem(Ressource.GetString("IN_OUT_ALL"), string.Empty));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_IOElementType
        public static void DDLLoad_IOElementType(DropDownList pddl)
        {
            DDLLoad_IOElementType(pddl, false);
        }
        public static void DDLLoad_IOElementType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.IOElementType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_IOElementType
        #region DDLLoad_Visibility
        public static void DDLLoad_Visibility(DropDownList pddl)
        {
            DDLLoad_Visibility(pddl, false);
        }
        public static void DDLLoad_Visibility(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.Visibility)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Visibility

        // CC 20140723
        #region DDLLoad_MarginType
        public static void DDLLoad_MarginType(DropDownList pddl)
        {
            DDLLoad_MarginType(pddl, false);
        }
        public static void DDLLoad_MarginType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MarginType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MarginType
        // EG 20150320 (POC]
        #region DDLLoad_FundingType
        public static void DDLLoad_FundingType(DropDownList pddl)
        {
            DDLLoad_FundingType(pddl, false);
        }
        public static void DDLLoad_FundingType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.FundingType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_FundingType

        // CC 20150312
        #region DDLLoad_MarginingMode
        public static void DDLLoad_MarginingMode(DropDownList pddl)
        {
            DDLLoad_MarginingMode(pddl, false);
        }
        public static void DDLLoad_MarginingMode(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MarginingMode)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MarginingMode

        // CC 20140724
        #region DDLLoad_MarginAssessmentBasis
        public static void DDLLoad_MarginAssessmentBasis(DropDownList pddl)
        {
            DDLLoad_MarginAssessmentBasis(pddl, false);
        }
        public static void DDLLoad_MarginAssessmentBasis(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MarginAssessmentBasis)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MarginAssessmentBasis

        #region DDLLoad_BannerPosition
        public static void DDLLoad_BannerPosition(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.BannerAlign)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_BannerPosition

        #region DDLLoad_AssetBannerStyle
        public static void DDLLoad_AssetBannerStyle(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.AssetBannerStyle)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_AssetBannerStyle

        #region DDLLoad_SectionBannerStyle
        public static void DDLLoad_SectionBannerStyle(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.SectionBannerStyle)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_SectionBannerStyle

        #region DDLLoad_TimestampType
        public static void DDLLoad_TimestampType(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.TimestampType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_TimestampType

        #region DDLLoad_UTISummary
        public static void DDLLoad_UTISummary(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.UTISummary)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_UTISummary

        #region DDLLoad_InputTradeAction
        //public static void OLD_DDLLoad_InputTradeAction(DropDownList pddl)
        //{
        //    OLD_DDLLoad_InputTradeAction(pddl, false);
        //}
        //public static void OLD_DDLLoad_InputTradeAction(DropDownList pddl, bool pWithEmpty)
        //{
        //    string tableName = Cst.OTCml_TBL.VW_PERMIS_MENU.ToString();
        //    string source = SessionTools.CS;
        //    //
        //    #region Permission for Create, Modify or Remove a trade
        //    SQLWhere sqlWhere = new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(source, tableName));
        //    sqlWhere.Append("IDMENU=" + DataHelper.SQLString("OTC_INP_TRD"));
        //    sqlWhere.Append(DataHelper.SQLColumnIn("PERMISSION", new string[] { PermissionEnum.Create.ToString(), PermissionEnum.Modify.ToString() }, TypeData.TypeDataEnum.@string));
        //    #endregion
        //    #region Permission for User action on a trade (eg.: Exercise, Early termination, ...)
        //    SQLWhere sqlWhere2 = new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(source, tableName));
        //    sqlWhere2.Append("IDMENU" + " " + DataHelper.SQLLike("OTC_INP_TRD", true, ApplicationBlocks.Data.CompareEnum.Upper));
        //    sqlWhere2.Append(DataHelper.SQLColumnIn("PERMISSION", new string[] { PermissionEnum.Create.ToString() }, TypeData.TypeDataEnum.@string));
        //    #endregion
        //    //	
        //    string sqlQuery = SQLCst.SELECT + tableName + ".MNU_PERM_DESC," + tableName + ".IDPERMISSION , 0 as ORDER" + Cst.CrLf;
        //    sqlQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
        //    sqlQuery += sqlWhere.ToString();
        //    //
        //    sqlQuery += SQLCst.UNION + Cst.CrLf;
        //    //
        //    sqlQuery += SQLCst.SELECT + tableName + ".MNU_PERM_DESC," + tableName + ".IDPERMISSION, 1 as ORDER" + Cst.CrLf;
        //    sqlQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
        //    sqlQuery += sqlWhere2.ToString();
        //    //
        //    sqlQuery += SQLCst.ORDERBY + "ORDER";
        //    //
        //    DDLLoad(pddl, "MNU_PERM_DESC", "IDM", source, sqlQuery, pWithEmpty, false, null);
        //}
        #endregion
        #region DDLLoad_LinearDepreciationPeriod
        public static void DDLLoad_LinearDepreciationPeriod(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("LinearDepreciation_Amortized"), Cst.LinearDepreciation_Amortized));
            pddl.Items.Add(new ListItem(Ressource.GetString("LinearDepreciation_Remaining"), Cst.LinearDepreciation_Remaining));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_LinearDepreciationPeriod
        #region DDLLoad_LogLevel
        public static void DDLLoad_LogLevel(DropDownList pddl)
        {
            DDLLoad_LogLevel(pddl, false);
        }
        public static void DDLLoad_LogLevel(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(LogLevelDetail)))
            {
                if (s != LogLevelDetail.INHERITED.ToString())
                    pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_LogLevel_Inherited(DropDownList pddl)
        {
            DDLLoad_LogLevel_Inherited(pddl, false);
        }
        public static void DDLLoad_LogLevel_Inherited(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(LogLevelDetail)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_LogLevel

        #region DDLLoad_PosRequestLevel
        public static void DDLLoad_PosRequestLevel(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("RESUME", false), "0"));
            pddl.Items.Add(new ListItem(Ressource.GetString("MARKETDETAIL", false), "1"));
            pddl.Items.Add(new ListItem(Ressource.GetString("FULL", false), "2"));
        }
        #endregion DDLLoad_PosRequestLevel
        #region DDLLoad_MandatoryOptionalType
        public static void DDLLoad_MandatoryOptionalType(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString(MandatoryOptionalEnum.MANDATORY.ToString()), MandatoryOptionalEnum.MANDATORY.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:DarkRed;background-color:Transparent");
            pddl.Items.Add(new ListItem(Ressource.GetString(MandatoryOptionalEnum.OPTIONAL.ToString()), MandatoryOptionalEnum.OPTIONAL.ToString()));
            pddl.Items[pddl.Items.Count - 1].Attributes.Add("style", "color:DarkBlue;background-color:Transparent");
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_Market
        public static void DDLLoad_MarketId(string pCS, DropDownList pddl)
        {
            DDLLoad_MarketId(pCS, pddl, false);
        }
        public static void DDLLoad_MarketId(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string tableName = Cst.OTCml_TBL.MARKET.ToString();

            string SQLQuery = SQLCst.SELECT + "IDM, IDENTIFIER" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, tableName)).ToString();
            //
            //FI 20120118 Le tri est désormais effectué par la combo pWithSort=true
            DDLLoad(pddl, "IDENTIFIER", "IDM", pCS, SQLQuery, pWithEmpty, true, null);
            if (!pWithEmpty)
                DDLSelectByValue(pddl, SystemSettings.GetAppSettings("Spheres_ReferentialDefault_market"));
        }

        /// <summary>
        /// Load the drop down list with markets
        /// <para>For each item Value=IDM, Text =SHORT_ACRONYM  </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        public static void DDLLoad_Market(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_Market(pCS, pddl, pWithEmpty, false, false, false, string.Empty);
        }

        /// <summary>
        /// Load the drop down list with markets
        /// <para>For each item Value=IDM, Text =SHORT_ACRONYM  </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsUseSessionRestrict"></param>
        /// <param name="pIsClear"></param>
        public static void DDLLoad_Market(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            DDLLoad_Market(pCS, pddl, pWithEmpty, pIsAddALL, pIsUseSessionRestrict, pIsClear, string.Empty);
        }

        /// <summary>
        /// Load the drop down list with markets
        /// <para>For each item Value=IDM, Text =SHORT_ACRONYM  </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsUseSessionRestrict"></param>
        /// <param name="pIsClear"></param>
        public static void DDLLoad_MarketETD(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear)
        {
            string additionalFilter = SQLCst.AND + "ISTRADEDDERIVATIVE=1" + Cst.CrLf;
            DDLLoad_Market(pCS, pddl, pWithEmpty, pIsAddALL, pIsUseSessionRestrict, pIsClear, additionalFilter);
        }


        /// <summary>
        /// Load the drop down list with markets
        /// <para>For each item Value=IDM, Text =SHORT_ACRONYM  </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsUseSessionRestrict"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pAdditionalFilter"></param>
        public static void DDLLoad_Market(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear, string pAdditionalFilter)
        {
            string table = Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString();

            DataParameters parameters = new DataParameters();
            //
            StrBuilder SQLQuery = new StrBuilder(SQLCst.SELECT);
            SQLQuery += "m.IDM, m.SHORT_ACRONYM" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + table + " m" + Cst.CrLf;
            //
            if (pIsUseSessionRestrict && (false == SessionTools.IsSessionSysAdmin))
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                SQLQuery += srh.GetSQLMarket(null, "m.IDM");
                srh.SetParameter(pCS, SQLQuery.ToString(), parameters);
            }
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, "m")).ToString();
            SQLQuery += pAdditionalFilter;

            if (pIsAddALL)
            {
                SQLQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                SQLQuery += SQLCst.SELECT + "-1 as IDM" + "," + Cst.CrLf;
                SQLQuery += DataHelper.SQLString(Ressource.GetString("Market_ALL"), "SHORT_ACRONYM") + Cst.CrLf;
                SQLQuery += DataHelper.SQLFromDual(pCS);
            }

            QueryParameters queryParameters = new QueryParameters(pCS, SQLQuery.ToString(), parameters);
            DDLLoad(pddl, "SHORT_ACRONYM", "IDM", pIsClear, 0, null, pCS, queryParameters.Query, pWithEmpty, true, parameters.GetArrayDbParameter());
        }




        /// <summary>
        /// Load the drop down list with markets
        /// <para>For each item Value=FIXML_SecurityExchange, Text =SHORT_ACRONYM  </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsUseSessionRestrict"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pAdditionalFilter"></param>
        /// FI 20180131 [23753] Add
        public static void DDLLoad_Market2(string pCS, DropDownList pddl, bool pWithEmpty, bool pIsAddALL, bool pIsUseSessionRestrict, bool pIsClear, string pAdditionalFilter)
        {
            string table = Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString();

            DataParameters parameters = new DataParameters();
            //
            StrBuilder SQLQuery = new StrBuilder(SQLCst.SELECT);
            SQLQuery += "m.IDM, m.SHORT_ACRONYM, m.FIXML_SecurityExchange" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + table + " m" + Cst.CrLf;
            //
            if (pIsUseSessionRestrict && (false == SessionTools.IsSessionSysAdmin))
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                SQLQuery += srh.GetSQLMarket(null, "m.IDM");
                srh.SetParameter(pCS, SQLQuery.ToString(), parameters);
            }
            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, "m")).ToString();
            SQLQuery += pAdditionalFilter;

            if (pIsAddALL)
            {
                SQLQuery += Cst.CrLf + SQLCst.UNION + Cst.CrLf;
                SQLQuery += SQLCst.SELECT + "-1 as IDM" + "," + Cst.CrLf;
                SQLQuery += DataHelper.SQLString(Ressource.GetString("Market_ALL"), "SHORT_ACRONYM") + Cst.CrLf;
                SQLQuery += DataHelper.SQLString(Ressource.GetString("Market_ALL"), "FIXML_SecurityExchange") + Cst.CrLf;
                SQLQuery += DataHelper.SQLFromDual(pCS);
            }

            QueryParameters queryParameters = new QueryParameters(pCS, SQLQuery.ToString(), parameters);
            DDLLoad(pddl, "SHORT_ACRONYM", "FIXML_SecurityExchange", pIsClear, 0, null, pCS, queryParameters.Query, pWithEmpty, true, parameters.GetArrayDbParameter());
        }






        #endregion DDLLoad_Market

        #region DDLLoad_MarketEnv
        public static void DDLLoad_MarketEnv(string pCS, DropDownList pddl)
        {
            DDLLoad_MarketEnv(pCS, pddl, false);
        }
        public static void DDLLoad_MarketEnv(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string SQLQuery = SQLCst.SELECT + "IDMARKETENV, DISPLAYNAME" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.MARKETENV.ToString();
            DDLLoad(pddl, "DISPLAYNAME", "IDMARKETENV", pCS, SQLQuery, pWithEmpty, true, null);
        }
        #endregion
        #region DDLLoad_Menu
        public static void DDLLoad_Menu(string pCS, DropDownList pddl, bool pIsWithEmpty)
        {
            string tableName = Cst.OTCml_TBL.VW_MENU.ToString();

            string SQLQuery = SQLCst.SELECT + "IDMENU,DISPLAYNAME,FORECOLOR,BACKCOLOR" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "IDMENU";
            //NB: On inverse volontairement Text / Value (cf.ci-dessous)
            DDLLoad(pddl, "IDMENU", "DISPLAYNAME", pCS, SQLQuery, pIsWithEmpty, null);
            //
            string idmenu, displayname, shortName, fullName;
            foreach (ListItem li in pddl.Items)
            {
                idmenu = li.Text;
                displayname = li.Value;
                li.Value = idmenu;
                shortName = Ressource.GetMenu_Shortname(idmenu, displayname);
                fullName = Ressource.GetMenu_Fullname(idmenu);
                li.Text = idmenu + " - " + shortName + (shortName == fullName ? string.Empty : " - " + fullName);
            }
        }
        public static void DDLLoad_Permission(string pCS, DropDownList pddl, bool pIsWithEmpty)
        {
            string tableName = Cst.OTCml_TBL.VW_PERMIS_MENU.ToString();

            string SQLQuery = SQLCst.SELECT + "IDPERMISSION,MNU_PERM_DESC,FORECOLOR,BACKCOLOR" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + tableName + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "IDMENU,PERMISSION";
            //
            DDLLoad(pddl, "MNU_PERM_DESC", "IDPERMISSION", pCS, SQLQuery, pIsWithEmpty, null);
            //
            int pos;
            string idmenu, shortName;
            foreach (ListItem li in pddl.Items)
            {
                pos = li.Text.IndexOf(" - ");
                if (pos > 0)
                {
                    idmenu = li.Text.Substring(0, pos);
                    shortName = Ressource.GetMenu_Shortname(idmenu, idmenu);
                    //fullName = Ressource.GetMenu_Fullname(idmenu);
                    li.Text = idmenu + " - " + shortName + " - [ " + li.Text.Substring(pos + 3) + " ]";
                }
            }
        }
        #endregion DDLLoad_Menu
        #region DDLLoad_MissingUTI
        public static void DDLLoad_MissingUTI(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MissingUTI)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_MissingUTI

        #region DDLLoad_Month
        public static void DDLLoad_Month(DropDownList pddl)
        {
            DDLLoad_Month(pddl, false);
        }
        public static void DDLLoad_Month(DropDownList pddl, bool pWithEmpty)
        {
            string month;
            for (int i = 1; i <= 12; i++)
            {
                DateTime dt = new DateTime(2000, i, 1, 0, 0, 0, 0);
                month = dt.ToString("MMMM");
                pddl.Items.Add(new ListItem(month.Substring(0, 1).ToUpper() + month.Substring(1).ToLower(), i.ToString()));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Month
        #region DDLLoad_Operator
        public static void DDLLoad_Operator(DropDownList pddl)
        {
            DDLLoad_Operator(pddl, false, TypeData.TypeDataEnum.@string.ToString());
        }
        public static void DDLLoad_Operator(DropDownList pddl, string pDataType)
        {
            DDLLoad_Operator(pddl, false, pDataType);
        }
        public static void DDLLoad_Operator(DropDownList pddl, bool pWithEmpty, string pDataType)
        {
            pddl.Items.Clear();
            if (!TypeData.IsTypeText(pDataType))
            {
                // RD 20110706 [17504] 
                // Pour le type Bool, n'afficher que deux opérateurs: Checked et Unchecked
                if (TypeData.IsTypeBool(pDataType))
                {
                    pddl.Items.Add(new ListItem(Ressource.GetString("checked"), "Checked"));
                    pddl.Items.Add(new ListItem(Ressource.GetString("unchecked"), "Unchecked"));
                }
                else
                {
                    pddl.Items.Add(new ListItem(Ressource.GetString("equalto"), "="));
                    pddl.Items.Add(new ListItem(Ressource.GetString("notequalto"), "!="));
                    //
                    pddl.Items.Add(new ListItem(Ressource.GetString("greaterthan"), ">"));
                    pddl.Items.Add(new ListItem(Ressource.GetString("lessthan"), "<"));
                    pddl.Items.Add(new ListItem(Ressource.GetString("greaterorequalto"), ">="));
                    pddl.Items.Add(new ListItem(Ressource.GetString("lessorequalto"), "<="));
                }
            }
            //
            if (TypeData.IsTypeString(pDataType) || TypeData.IsTypeText(pDataType))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("contains"), "Contains"));
                pddl.Items.Add(new ListItem(Ressource.GetString("notcontains"), "not Contains"));
                pddl.Items.Add(new ListItem(Ressource.GetString("startswith"), "StartsWith"));
                pddl.Items.Add(new ListItem(Ressource.GetString("endswith"), "EndsWith"));
            }
            //
            if ((!TypeData.IsTypeDateOrDateTime(pDataType)) && (!TypeData.IsTypeBool(pDataType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("like"), "like"));
                pddl.Items.Add(new ListItem(Ressource.GetString("notlike"), "not like"));
            }
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Operator
        #region DDLLoad_OptionType
        public static void DDLLoad_OptionType(string pCS, DropDownList pddl)
        {
            DDLLoad_OptionType(pCS, pddl, false);
        }
        public static void DDLLoad_OptionType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "OptionTypeEnum");
        }
        #endregion DDLLoad_OptionType
        #region DDLLoad_PositionDay
        public static void DDLLoad_PositionDay(DropDownList pddl)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.PositionDay)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
        }
        #endregion DDLLoad_PositionDay
        #region DDLLoad_PremiumQuoteBasis
        public static void DDLLoad_PremiumQuoteBasis(string pCS, DropDownList pddl)
        {
            DDLLoad_PremiumQuoteBasis(pCS, pddl, false);
        }
        public static void DDLLoad_PremiumQuoteBasis(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "PremiumQuoteBasisEnum");
        }
        #endregion DDLLoad_PremiumQuoteBasis
        #region DDLLoad_PriceCurve
        public static void DDLLoad_PriceCurve(DropDownList pddl)
        {
            DDLLoad_PriceCurve(pddl, false);
        }
        public static void DDLLoad_PriceCurve(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Forward"), Cst.PriceCurve_Forward));
            pddl.Items.Add(new ListItem(Ressource.GetString("Spot"), Cst.PriceCurve_Spot));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_PriceCurve
        #region DDLLoad_ProcessType
        public static void DDLLoad_ProcessType(DropDownList pddl)
        {
            DDLLoad_ProcessType(pddl, false);
        }
        public static void DDLLoad_ProcessType(DropDownList pddl, bool pWithEmpty)
        {
            Cst.ProcessTypeEnum process;
            foreach (string s in Enum.GetNames(typeof(Cst.ProcessTypeEnum)))
            {
                //20070118 PL A finaliser...
                process = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), s);
                switch (process)
                {
                    case Cst.ProcessTypeEnum.TRADECAPTURE:
                    case Cst.ProcessTypeEnum.IO:
                    case Cst.ProcessTypeEnum.NA:
                    case Cst.ProcessTypeEnum.QUOTHANDLING:
                    case Cst.ProcessTypeEnum.SCHEDULER:
                    //case Cst.ProcessTypeEnum.TRADEACTGEN:  20070914 PL Mis en commentaire pour en disposer dans Process Tuning
                    case Cst.ProcessTypeEnum.USER:
                    case Cst.ProcessTypeEnum.WEBSESSION:
                    case Cst.ProcessTypeEnum.CLOSINGGEN:
                    //
                    case Cst.ProcessTypeEnum.GATEBCS://Eurosys Gateway
                    // CC 20121009 Add ACTIONGEN pour ne pas en disposer dans Process Tuning
                    case Cst.ProcessTypeEnum.ACTIONGEN:
                        break;
                    default:
                        pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
                        break;
                }
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_ProcessType
        #region DDLLoad_Product...
        public static void DDLLoad_ProductGProduct(DropDownList pddl)
        {
            DDLLoad_ProductGProduct(pddl, false);
        }
        /// EG 20161122 New Commodity Derivative
        public static void DDLLoad_ProductGProduct(DropDownList pddl, bool pWithEmpty)
        {
            List<ProductTools.GroupProductEnum> lst = Enum.GetValues(typeof(ProductTools.GroupProductEnum)).Cast<ProductTools.GroupProductEnum>().ToList();
            lst.ForEach(item =>
                pddl.Items.Add(new ListItem(item.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(item)))
            );

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        /// <summary>
        /// Chargement des GProduct de type Trading (donc hors ADM, ASSET et RISK)
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// EG 20161122 New Commodity Derivative
        public static void DDLLoad_ProductGProduct_Trading(DropDownList pddl, bool pWithEmpty)
        {
            List<ProductTools.GroupProductEnum> lst = Enum.GetValues(typeof(ProductTools.GroupProductEnum)).Cast<ProductTools.GroupProductEnum>().ToList();
            lst.ForEach(item =>
            {
                if (ProductTools.IsProductTrading(item))
                    pddl.Items.Add(new ListItem(item.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(item)));
            });
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_ProductFamily(DropDownList pddl)
        {
            DDLLoad_ProductFamily(pddl, false);
        }
        /// <summary>
        ///  Charge le ddl avec les family
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// FI 20140930 [XXXXX] Add ProductFamily_CASHPAYMENT
        // EG 20150412 [20513] BANCAPERTA Add ProductFamily_BO
        /// EG 20161122 New Commodity Derivative
        public static void DDLLoad_ProductFamily(DropDownList pddl, bool pWithEmpty)
        {
            List<ProductTools.FamilyEnum> lst = Enum.GetValues(typeof(ProductTools.FamilyEnum)).Cast<ProductTools.FamilyEnum>().ToList();
            lst.ForEach(item =>
                pddl.Items.Add(new ListItem(item.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.FamilyEnum>(item)))
            );
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_ProductSource(DropDownList pddl)
        {
            List<ProductTools.SourceEnum> lst = Enum.GetValues(typeof(ProductTools.SourceEnum)).Cast<ProductTools.SourceEnum>().ToList();
            lst.ForEach(item =>
            {
                if (ProductTools.IsProductSource(item))
                    pddl.Items.Add(new ListItem(item.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.SourceEnum>(item)));
            });
        }

        public static void DDLLoad_Source(DropDownList pddl)
        {
            List<ProductTools.SourceEnum> lst = Enum.GetValues(typeof(ProductTools.SourceEnum)).Cast<ProductTools.SourceEnum>().ToList();
            lst.ForEach(item =>
                pddl.Items.Add(new ListItem(item.ToString(), ReflectionTools.ConvertEnumToString<ProductTools.SourceEnum>(item)))
            );
        }
        public static void DDLLoad_Service(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresAccountGen.ToString(), Cst.ServiceEnum.SpheresAccountGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresClosingGen.ToString(), Cst.ServiceEnum.SpheresClosingGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresEarGen.ToString(), Cst.ServiceEnum.SpheresEarGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresEventsGen.ToString(), Cst.ServiceEnum.SpheresEventsGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresEventsVal.ToString(), Cst.ServiceEnum.SpheresEventsVal.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresIO.ToString(), Cst.ServiceEnum.SpheresIO.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresMarkToMarketGen.ToString(), Cst.ServiceEnum.SpheresMarkToMarketGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresQuotationHandling.ToString(), Cst.ServiceEnum.SpheresQuotationHandling.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresSettlementInstrGen.ToString(), Cst.ServiceEnum.SpheresSettlementInstrGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresTradeActionGen.ToString(), Cst.ServiceEnum.SpheresTradeActionGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresScheduler.ToString(), Cst.ServiceEnum.SpheresScheduler.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresConfirmationMsgGen.ToString(), Cst.ServiceEnum.SpheresConfirmationMsgGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresSettlementMsgGen.ToString(), Cst.ServiceEnum.SpheresSettlementMsgGen.ToString()));
            pddl.Items.Add(new ListItem(Cst.ServiceEnum.SpheresGateBCS.ToString(), Cst.ServiceEnum.SpheresGateBCS.ToString()));
        }
        public static void DDLLoad_SourceAndServices(DropDownList pddl)
        {
            DDLLoad_Source(pddl);
            DDLLoad_Service(pddl);
        }
        public static void DDLLoad_ProductClass(DropDownList pddl)
        {
            DDLLoad_ProductClass(pddl, false);
        }
        public static void DDLLoad_ProductClass(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Cst.ProductClass_NA, Cst.ProductClass_NA));
            pddl.Items.Add(new ListItem(Cst.ProductClass_AGREE, Cst.ProductClass_AGREE));
            pddl.Items.Add(new ListItem(Cst.ProductClass_AUTHOR, Cst.ProductClass_AUTHOR));
            pddl.Items.Add(new ListItem(Cst.ProductClass_REGULAR, Cst.ProductClass_REGULAR));
            pddl.Items.Add(new ListItem(Cst.ProductClass_STRATEGY, Cst.ProductClass_STRATEGY));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Product...
        #region DDLLoad_QuoteExchangeType
        public static void DDLLoad_QuoteExchangeType(DropDownList pddl)
        {
            DDLLoad_QuoteExchangeType(pddl, false);
        }
        public static void DDLLoad_QuoteExchangeType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(EfsML.Enum.QuoteExchangeTypeEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString("QuoteExchangeTypeEnum_" + s), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_RateorIndex
        public static void DDLLoad_RateorIndex(DropDownList pddl)
        {
            DDLLoad_RateorIndex(pddl, false);
        }
        public static void DDLLoad_RateorIndex(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Index"), Cst.INDEX));
            pddl.Items.Add(new ListItem(Ressource.GetString("Rate"), Cst.RATE));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RateorIndex
        #region DDLLoad_RateType
        public static void DDLLoad_RateType(DropDownList pddl)
        {
            DDLLoad_RateType(pddl, false);
        }
        public static void DDLLoad_RateType(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Compounding"), Cst.RateType_compounding));
            pddl.Items.Add(new ListItem(Ressource.GetString("Overnight"), Cst.RateType_overnight));
            pddl.Items.Add(new ListItem(Ressource.GetString("Regular"), Cst.RateType_regular));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RateType
        #region DDLLoad_RateValueType
        public static void DDLLoad_RateValueType(DropDownList pddl)
        {
            DDLLoad_RateValueType(pddl, false);
        }
        public static void DDLLoad_RateValueType(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Prospective"), Cst.RateValueType_prospective));
            pddl.Items.Add(new ListItem(Ressource.GetString("Retrospective"), Cst.RateValueType_retrospective));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RateValueType
        #region DDLLoad_RDBMS
        public static void DDLLoad_RDBMS(DropDownList pddl)
        {
            DDLLoad_RDBMS(pddl, false);
        }
        public static void DDLLoad_RDBMS(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(RdbmsEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RDBMS

        #region DDLLoad_AllRatingValue
        //PL 20140728 Newness
        public static void DDLLoad_AllRatingValue(string pCS, PageBase pPage, DropDownList pddl, bool pWithEmpty, bool pIsClear, string pListRetrievalData)
        {
            StrBuilder sqlQuery = new StrBuilder();
            DataParameters parameters = new DataParameters();

            //ex. EquityAsset --> EQUITY, Commodity --> COMMODITY, ...
            string asset_Rating = "ASSET";
            if (pListRetrievalData.IndexOf(".") > 0)
                asset_Rating = pListRetrievalData.Substring(pListRetrievalData.IndexOf(".") + 1).ToUpper().Replace("ASSET", string.Empty);

            sqlQuery += SQLCst.SELECT + "VALUE,EXTVALUE,CODE as OPTGROUP,CODE as OPTGROUPTEXT,CODE as OPTGROUPSORT" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENUM.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(CODE like '%RATING%' and (upper(CODE) like '%" + asset_Rating + "%' or upper(CODE) like '%ASSET%'))";

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery.ToString(), parameters);

            DDLLoad(pPage, pddl, "EXTVALUE", "VALUE", pIsClear, 0, null, pCS, queryParameters.Query, pWithEmpty, true, queryParameters.Parameters.GetArrayDbParameter());
        }
        #endregion DDLLoad_AllRatingValue

        #region DDLLoad_RecursiveLevel
        public static void DDLLoad_RecursiveLevel(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("RESUME", false), "0"));
            pddl.Items.Add(new ListItem(Ressource.GetString("FULL", false), "2"));
        }
        #endregion DDLLoad_RecursiveLevel
        #region DDLLoad_RelativeToPaymentDT
        public static void DDLLoad_RelativeToPaymentDT(DropDownList pddl)
        {
            DDLLoad_RelativeToPaymentDT(pddl, false);
        }
        public static void DDLLoad_RelativeToPaymentDT(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Start"), Cst.RelativeToDT_Start));
            pddl.Items.Add(new ListItem(Ressource.GetString("End"), Cst.RelativeToDT_End));
            pddl.Items.Add(new ListItem(Ressource.GetString("Reset"), Cst.RelativeToDT_Reset));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RelativeToPaymentDT
        #region DDLLoad_RelativeToResetDT
        public static void DDLLoad_RelativeToResetDT(DropDownList pddl)
        {
            DDLLoad_RelativeToResetDT(pddl, false);
        }
        public static void DDLLoad_RelativeToResetDT(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Start"), Cst.RelativeToDT_Start));
            pddl.Items.Add(new ListItem(Ressource.GetString("End"), Cst.RelativeToDT_End));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RelativeToResetDT
        #region DDLLoad_RelativeToFixingDT
        public static void DDLLoad_RelativeToFixingDT(DropDownList pddl)
        {
            DDLLoad_RelativeToFixingDT(pddl, false);
        }
        public static void DDLLoad_RelativeToFixingDT(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Reset"), Cst.RelativeToDT_Reset));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RelativeToFixingDT
        #region DDLLoad_ReturnSPParamTypeEnum
        //GP 20070124.
        public static void DDLLoad_ReturnSPParamTypeEnum(DropDownList pddl)
        {
            DDLLoad_ReturnSPParamTypeEnum(pddl, false);
        }
        public static void DDLLoad_ReturnSPParamTypeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.ReturnSPParamTypeEnum)))
            {
                if (Cst.ReturnSPParamTypeEnum.NA.ToString() != s)
                    pddl.Items.Add(new ListItem(s, s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_RightType
        public static void DDLLoad_RightType(DropDownList pddl)
        {
            DDLLoad_RightType(pddl, false);
        }
        public static void DDLLoad_RightType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(RightsTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("Right" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RightType
        #region DDLLoad_RoleType
        public static void DDLLoad_RoleType(DropDownList pddl)
        {
            DDLLoad_RoleType(pddl, false);
        }
        public static void DDLLoad_RoleType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(RoleType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RoleType
        #region DDLLoad_Round...
        public static void DDLLoad_RoundDir(DropDownList pddl)
        {
            DDLLoad_RoundDir(pddl, false);
        }
        public static void DDLLoad_RoundDir(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Down"), Cst.RoundingDirectionSQL.D.ToString()));
            pddl.Items.Add(new ListItem(Ressource.GetString("Nearest"), Cst.RoundingDirectionSQL.N.ToString()));
            pddl.Items.Add(new ListItem(Ressource.GetString("Up"), Cst.RoundingDirectionSQL.U.ToString()));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_RoundPrec(DropDownList pddl)
        {
            DDLLoad_RoundPrec(pddl, false);
        }
        public static void DDLLoad_RoundPrec(DropDownList pddl, bool pWithEmpty)
        {
            for (int i = 0; i < 10; i++)
            {
                pddl.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Round...
        #region DDLLoad_RuleOnError
        public static void DDLLoad_RuleOnError(DropDownList pddl)
        {
            DDLLoad_RuleOnError(pddl, false);
        }
        public static void DDLLoad_RuleOnError(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.RuleOnError)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_RetCodeOnNoData
        public static void DDLLoad_RetCodeOnNoData(DropDownList pddl)
        {
            DDLLoad_RetCodeOnNoData(pddl, false);
        }
        // EG 20220221 [XXXXX] IRQ exlusion
        public static void DDLLoad_RetCodeOnNoData(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.IOReturnCodeEnum)))
            {
                if (s != Cst.IOReturnCodeEnum.TIMEOUT.ToString() &&
                    s != Cst.IOReturnCodeEnum.NA.ToString() &&
                    s != Cst.IOReturnCodeEnum.IRQ.ToString() &&
                    s != Cst.IOReturnCodeEnum.DEADLOCK.ToString())
                    pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_StartInfoStyle
        public static void DDLLoad_StartInfoStyle(DropDownList pddl)
        {
            DDLLoad_StartInfoStyle(pddl, false);
        }
        public static void DDLLoad_StartInfoStyle(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.StartInfoStyle)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_StatusCalcul
        public static void DDLLoad_StatusCalcul(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(StatusCalculEnum.CALC.ToString(), StatusCalculEnum.CALC.ToString()));
            pddl.Items.Add(new ListItem(StatusCalculEnum.CALCREV.ToString(), StatusCalculEnum.CALCREV.ToString()));
            pddl.Items.Add(new ListItem(StatusCalculEnum.TOCALC.ToString(), StatusCalculEnum.TOCALC.ToString()));
        }
        #endregion DDLLoad_StatusCalcul
        #region DDLLoad_StatusProcess
        public static void DDLLoad_StatusProcess(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(ProcessStateTools.StatusUnknown, ProcessStateTools.StatusUnknown));
            pddl.Items.Add(new ListItem(ProcessStateTools.StatusError, ProcessStateTools.StatusError));
            pddl.Items.Add(new ListItem(ProcessStateTools.StatusSuccess, ProcessStateTools.StatusSuccess));
        }
        #endregion DDLLoad_StatusProcess
        #region DDLLoad_StatusTrigger
        public static void DDLLoad_StatusTrigger(DropDownList pddl)
        {
            pddl.Items.Add(new ListItem(Cst.StatusTrigger.StatusTriggerEnum.NA.ToString(), Cst.StatusTrigger.StatusTriggerEnum.NA.ToString()));
            pddl.Items.Add(new ListItem(Cst.StatusTrigger.StatusTriggerEnum.NONE.ToString(), Cst.StatusTrigger.StatusTriggerEnum.NONE.ToString()));
            pddl.Items.Add(new ListItem(Cst.StatusTrigger.StatusTriggerEnum.ACTIV.ToString(), Cst.StatusTrigger.StatusTriggerEnum.ACTIV.ToString()));
            pddl.Items.Add(new ListItem(Cst.StatusTrigger.StatusTriggerEnum.DEACTIV.ToString(), Cst.StatusTrigger.StatusTriggerEnum.DEACTIV.ToString()));
        }
        #endregion DDLLoad_StatusTrigger
        #region DDLLoad_StrategyTypeScheme_Extended
        public static void DDLLoad_StrategyTypeScheme_Extended(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string itemGroup;
            int extLiIndex;
            if (pddl is OptionGroupDropDownList optGroupDDL)
            {
                bool isUNKNOWN_GroupSetted = false;
                bool isOTHER_GroupSetted = false;

                ListItem item;
                int index = -1;
                ExtendedListItem extLi;
                #region Cst.FeeStrategyTypeEnum
                foreach (string s in Enum.GetNames(typeof(Cst.FeeStrategyTypeEnum)))
                {
                    item = new ListItem(Ressource.GetString(typeof(Cst.FeeStrategyTypeEnum).Name + "_" + s), s);

                    if (s == Cst.FeeStrategyTypeEnum.ALL.ToString())
                    {
                        pddl.Items.Insert(++index, new ListItem(item.Text, item.Text));

                        extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                        extLi = optGroupDDL.ExtendedItems[extLiIndex];
                        extLi.GroupingType = ListItemGroupingTypeEnum.New;
                        extLi.GroupingText = extLi.Text;
                        extLi.GroupCssClass = "lstColumnOptionGroup";

                        item.Attributes.Add("style", "color:" + Color.DarkBlue.Name + ";background-color:" + Color.Lavender.Name + ";");
                    }
                    else if (s.IndexOf("UNKNOWN") < 0)
                    {
                        if (!isUNKNOWN_GroupSetted)
                        {
                            isUNKNOWN_GroupSetted = true;
                            itemGroup = Ressource.GetString("FeeStrategyTypeEnum_ALLKNOWN_Group");
                            pddl.Items.Insert(++index, new ListItem(itemGroup, itemGroup));

                            extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                            extLi = optGroupDDL.ExtendedItems[extLiIndex];
                            extLi.GroupingType = ListItemGroupingTypeEnum.New;
                            extLi.GroupingText = extLi.Text;
                        }

                        item.Attributes.Add("style", "color:" + Color.DarkSlateBlue.Name + ";background-color:" + Color.AliceBlue.Name + ";");
                    }
                    else
                    {
                        if (!isOTHER_GroupSetted)
                        {
                            isOTHER_GroupSetted = true;
                            itemGroup = Ressource.GetString("FeeStrategyTypeEnum_ALLUNKNOWN_Group");
                            pddl.Items.Insert(++index, new ListItem(itemGroup, itemGroup));

                            extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                            extLi = optGroupDDL.ExtendedItems[extLiIndex];
                            extLi.GroupingType = ListItemGroupingTypeEnum.New;
                            extLi.GroupingText = extLi.Text;
                        }

                        item.Attributes.Add("style", "color:" + Color.DarkRed.Name + ";background-color:" + Color.LavenderBlush.Name + ";");
                    }
                    pddl.Items.Insert(++index, item);

                    item.Attributes.Add("class", EFSCssClass.DropDownListCaptureLight);
                    extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                    extLi = optGroupDDL.ExtendedItems[extLiIndex];
                    extLi.GroupingType = ListItemGroupingTypeEnum.Inherit;
                }
                #endregion
                #region StrategyTypeScheme
                DropDownList ddl_temp = new DropDownList();
                DDLLoad_ENUM(ddl_temp, pCS, false, "StrategyTypeScheme");

                itemGroup = Ressource.GetString("FeeStrategyTypeEnum_NAMED_Group");
                pddl.Items.Insert(++index, new ListItem(itemGroup, itemGroup));

                extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                extLi = optGroupDDL.ExtendedItems[extLiIndex];
                extLi.GroupingType = ListItemGroupingTypeEnum.New;
                extLi.GroupingText = extLi.Text;

                for (int i = 0; i < ddl_temp.Items.Count; i++)
                {
                    item = ddl_temp.Items[i];
                    #region Look Upper/Lower
                    //CALENDAR-SPREAD --> Calendar-Spread
                    //COMBO-VS-UNDERLYING --> Combo vs Underlying
                    if (item.Text.Length > 1)
                    {
                        bool isNextUpper = false;
                        string textOut = item.Text.Substring(0, 1).ToUpper();
                        string text = item.Text;
                        for (int j = 1; j < text.Length; j++)
                        {
                            if (@"/-".IndexOf(text.Substring(j, 1)) >= 0)
                            {
                                isNextUpper = true;
                                textOut += text.Substring(j, 1);
                            }
                            else
                            {
                                textOut += isNextUpper ? text.Substring(j, 1).ToUpper() : text.Substring(j, 1).ToLower();
                                isNextUpper = false;
                            }
                        }
                        item.Text = textOut.Replace("-Vs-", " vs ");
                    }
                    #endregion
                    pddl.Items.Insert(++index, item);

                    item.Attributes.Add("class", EFSCssClass.DropDownListCaptureLight);
                    extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                    extLi = optGroupDDL.ExtendedItems[extLiIndex];
                    extLi.GroupingType = ListItemGroupingTypeEnum.Inherit;
                }
                #endregion

            }


            if (pWithEmpty)
            {
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            }
        }
        #endregion DDLLoad_StrategyTypeScheme_Extended
        #region DDLLoad_EFSOperators
        private static void DDLLoad_EFSOperators(DropDownList pddl)
        {
            pddl.Items.AddRange(new ListItem[] { new ListItem("=", "equals"), new ListItem("!=", "different") });
        }
        #endregion DDLLoad_EFSOperators
        #region DDLLoad_StrikeQuoteBasis
        public static void DDLLoad_StrikeQuoteBasis(string pCS, DropDownList pddl)
        {
            DDLLoad_StrikeQuoteBasis(pCS, pddl, false);
        }
        public static void DDLLoad_StrikeQuoteBasis(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "StrikeQuoteBasisEnum");
        }
        #endregion DDLLoad_StrikeQuoteBasis
        #region DDLLoad_SymbolAlign
        public static void DDLLoad_SymbolAlign(DropDownList pddl)
        {
            DDLLoad_SymbolAlign(pddl, false);
        }
        public static void DDLLoad_SymbolAlign(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("SymbolAlignLeft"), Cst.SymbolAlign_Left));
            //ListItem li;
            //li = new ListItem(Ressource.GetString("xxxx"), "xxxx", true);
            //li.Attributes.Add("style", "background-color:blue");
            //pddl.Items.Add(li);
            pddl.Items.Add(new ListItem(Ressource.GetString("SymbolAlignRight"), Cst.SymbolAlign_Right));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_SymbolAlign
        #region DDLLoad_LSTCOLUMN
        /// <summary>
        /// Chargement de la liste des colonnes disponibles pour un filtre sur une consultation LST.
        /// </summary>
        /// <param name="pDDL"></param>
        /// <param name="pDataSeparator"></param>
        /// <param name="pIsMultiAlias"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pDtLstSelect"></param>
        /// FI 20160804 [Migration TFS] Modify 
        /// EG 20180703 PERF (Step3 : Exclusion des colonnes en double)  
        /// PL 20181123 New signature - Add DataTable opDtLstSelect
        /// EG 20191213 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// FI 20201201 [XXXXX] Refactoring (pDtLstSelect contient toutes les données à charger)
        public static void DDLLoad_LSTCOLUMN(DropDownList pDDL, string pDataSeparator, bool pIsMultiAlias, bool pWithEmpty, DataTable pDtLstSelect)
        {

            bool isOptGroupDDL = (pDDL.GetType() == typeof(OptionGroupDropDownList));
            OptionGroupDropDownList optGroupDDL = null;
            if (isOptGroupDDL)
                optGroupDDL = (OptionGroupDropDownList)pDDL;

            pDDL.Items.Clear();
            if (pWithEmpty)
            {
                //NB: itemvalue --> ISMANDATORY,IDLSTCOLUMN,ALIAS,DATATYPE,TABLENAME,COLUMNNAME,,,,Referential
                pDDL.Items.Insert(0, new ListItem(" ",
                    "0" + pDataSeparator + "-1" + pDataSeparator + string.Empty + pDataSeparator + string.Empty + pDataSeparator + string.Empty + pDataSeparator
                    + "-1" + pDataSeparator + string.Empty + pDataSeparator + string.Empty + pDataSeparator + string.Empty + pDataSeparator + string.Empty));
            }

            string empty = string.Empty;
            string lastHeader = empty;
            bool isAlternatingTable = false;
            int extLiIndex;
            foreach (DataRow dr in pDtLstSelect.Select(null, "POSITION, DISPLAYNAME"))
            {

                //PL 20121022 Add ALIASHEADER
                //currentHeader = dr["ALIASDISPLAYNAME"].ToString();
                string currentHeader = dr["ALIASHEADER"].ToString();
                ExtendedListItem extLi;
                string itemText;
                if (lastHeader != currentHeader)
                {
                    lastHeader = currentHeader;

                    if (isOptGroupDDL)
                    {
                        itemText = dr["ALIASDISPLAYNAME"].ToString().Trim().Replace(Cst.HTMLBreakLine, " ");// Replace <br/> by space;
                        pDDL.Items.Add(new ListItem(itemText, itemText));

                        extLiIndex = optGroupDDL.ExtendedItems.Count - 1;
                        extLi = optGroupDDL.ExtendedItems[extLiIndex];
                        extLi.GroupingType = ListItemGroupingTypeEnum.New;
                        extLi.GroupingText = extLi.Text;
                        extLi.GroupCssClass = "lstColumnOptionGroup";
                    }
                    else
                    {
                        isAlternatingTable = !isAlternatingTable;
                    }
                }

                itemText = dr["DISPLAYNAME"].ToString().Trim();
                if ((!isOptGroupDDL) && pIsMultiAlias)
                {
                    itemText = "[" + dr["ALIASDISPLAYNAME"].ToString().Trim() + "] " + itemText;
                }
                itemText = itemText.Replace(Cst.HTMLBreakLine, " ");// Replace <br/> by space

                string itemValue = dr["ISMANDATORY"].ToString() + pDataSeparator;
                itemValue += "-1" + pDataSeparator;
                itemValue += dr["ALIAS"].ToString().TrimEnd() + pDataSeparator;
                itemValue += dr["DATATYPE"].ToString() + pDataSeparator;
                itemValue += dr["TABLENAME"].ToString() + pDataSeparator + dr["COLUMNNAME"].ToString() + pDataSeparator;
                itemValue += string.Empty + pDataSeparator + string.Empty + pDataSeparator;
                // FI 20160804 [Migration TFS] Repository isntead of Referential
                itemValue += string.Empty + pDataSeparator + Cst.ListType.Repository.ToString();

                ListItem listItem = new ListItem(itemText, itemValue);
                if ((!isOptGroupDDL) && pIsMultiAlias && isAlternatingTable)
                {
                    listItem.Attributes.Add("class", "roundDef");
                }
                pDDL.Items.Add(listItem);

                if (isOptGroupDDL)
                {
                    listItem.Attributes.Add("class", EFSCssClass.DropDownListCaptureLight);

                    extLiIndex = optGroupDDL.ExtendedItems.Count - 1;

                    extLi = optGroupDDL.ExtendedItems[extLiIndex];
                    extLi.GroupingType = ListItemGroupingTypeEnum.Inherit;
                }
            }
        }
        #endregion DDLLoad_LSTCOLUMN
        #region DDLLoad_UnderlyingAsset
        //Create_DropDownList_For_Referential Step3: Create a loading method
        public static void DDLLoad_UnderlyingAsset(DropDownList pddl)
        {
            DDLLoad_UnderlyingAsset(pddl, false);
        }
        public static void DDLLoad_UnderlyingAsset(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.UnderlyingAsset)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        public static void DDLLoad_UnderlyingAsset_Rate(DropDownList pddl)
        {
            DDLLoad_UnderlyingAsset_Rate(pddl, false);
        }
        /// EG 20140702 Refactoring
        public static void DDLLoad_UnderlyingAsset_Rate(DropDownList pddl, bool pWithEmpty)
        {
            Cst.UnderlyingAsset underlyingAsset_Rate =
            (Cst.UnderlyingAsset.Bond | Cst.UnderlyingAsset.Deposit | Cst.UnderlyingAsset.Future |
            Cst.UnderlyingAsset.RateIndex | Cst.UnderlyingAsset.SimpleFra | Cst.UnderlyingAsset.SimpleIRSwap);
            DDLLoad_UnderlyingAsset(pddl, underlyingAsset_Rate, pWithEmpty);
        }
        public static void DDLLoad_UnderlyingAsset_Rating(DropDownList pddl)
        {
            DDLLoad_UnderlyingAsset_Rating(pddl, false);
        }
        // EG 20150222 Add FxRateAsset (FOREX)
        public static void DDLLoad_UnderlyingAsset_Rating(DropDownList pddl, bool pWithEmpty)
        {
            Cst.UnderlyingAsset underlyingAsset_Rating =
            (Cst.UnderlyingAsset.Commodity | Cst.UnderlyingAsset.EquityAsset | Cst.UnderlyingAsset.Future |
            Cst.UnderlyingAsset.Index | Cst.UnderlyingAsset.FxRateAsset);
            DDLLoad_UnderlyingAsset(pddl, underlyingAsset_Rating, pWithEmpty);
        }
        /// EG 20140702 Refactoring
        /// PL 20231220 [WI789] New Signature Method - New "*Future" values and combined values
        public static void DDLLoad_UnderlyingAsset_ETD(DropDownList pddl, bool pWithEmpty, bool pWithCombinedValues)
        {
            Cst.UnderlyingAsset underlyingAsset_ETD =
            (Cst.UnderlyingAsset.Bond | Cst.UnderlyingAsset.Commodity | Cst.UnderlyingAsset.EquityAsset |
            Cst.UnderlyingAsset.ExchangeTradedFund | Cst.UnderlyingAsset.Future | Cst.UnderlyingAsset.FxRateAsset |
            Cst.UnderlyingAsset.Index | Cst.UnderlyingAsset.RateIndex
            | Cst.UnderlyingAsset.BondFuture | Cst.UnderlyingAsset.Bond_BondFuture
            | Cst.UnderlyingAsset.CommodityFuture | Cst.UnderlyingAsset.Commodity_CommodityFuture
            | Cst.UnderlyingAsset.EquityAssetFuture | Cst.UnderlyingAsset.EquityAsset_EquityAssetFuture
            | Cst.UnderlyingAsset.FxRateAssetFuture | Cst.UnderlyingAsset.FxRateAsset_FxRateAssetFuture
            | Cst.UnderlyingAsset.IndexFuture | Cst.UnderlyingAsset.Index_IndexFuture
            | Cst.UnderlyingAsset.RateIndexFuture | Cst.UnderlyingAsset.RateIndex_RateIndexFuture);
            DDLLoad_UnderlyingAsset(pddl, underlyingAsset_ETD, pWithEmpty, pWithCombinedValues);
        }
        // EG 20140526 Refactoring
        public static void DDLLoad_UnderlyingAsset_Collateral(DropDownList pddl)
        {
            DDLLoad_UnderlyingAsset_Collateral(pddl, false);
        }
        /// EG 20140702 Refactoring
        public static void DDLLoad_UnderlyingAsset_Collateral(DropDownList pddl, bool pWithEmpty)
        {
            Cst.UnderlyingAsset underlyingAsset_Collateral =
                (Cst.UnderlyingAsset.Bond | Cst.UnderlyingAsset.Cash | Cst.UnderlyingAsset.EquityAsset);
            DDLLoad_UnderlyingAsset(pddl, underlyingAsset_Collateral, pWithEmpty);
        }
        /// EG 20140702 New
        public static void DDLLoad_UnderlyingAsset_ReturnSwap(DropDownList pddl)
        {
            DDLLoad_UnderlyingAsset_ReturnSwap(pddl, false);
        }
        /// EG 20140702 New
        /// EG 20150222 Add FxRateAsset (FOREX)
        public static void DDLLoad_UnderlyingAsset_ReturnSwap(DropDownList pddl, bool pWithEmpty)
        {
            Cst.UnderlyingAsset underlyingReturnSwap =
                (Cst.UnderlyingAsset.ConvertibleBond | Cst.UnderlyingAsset.Commodity | Cst.UnderlyingAsset.EquityAsset |
                Cst.UnderlyingAsset.Future | Cst.UnderlyingAsset.Index | Cst.UnderlyingAsset.MutualFund | Cst.UnderlyingAsset.FxRateAsset);

            DDLLoad_UnderlyingAsset(pddl, underlyingReturnSwap, pWithEmpty);
        }
        /// PL 20231220 [WI789] New Method
        public static void DDLLoad_UnderlyingAsset(DropDownList pddl, Cst.UnderlyingAsset pUnderlyingAssetFilters, bool pWithEmpty)
        {
            DDLLoad_UnderlyingAsset(pddl, pUnderlyingAssetFilters, pWithEmpty, false);
        }
        /// EG 20140702 New
        /// PL 20231220 [WI789] Manage combined values
        public static void DDLLoad_UnderlyingAsset(DropDownList pddl, Cst.UnderlyingAsset pUnderlyingAssetFilters, bool pWithEmpty, bool pWithCombinedValues)
        {
            foreach (Cst.UnderlyingAsset item in Enum.GetValues(typeof(Cst.UnderlyingAsset)))
            {
                if ((item & pUnderlyingAssetFilters) != 0) 
                {
                    string s = item.ToString();
                    //if (pWithCombinedValues || (s.IndexOf("_") == -1))
                    if (pWithCombinedValues || (s.IndexOf("Future") <= 0))
                    {
                        //Les valeurs virtuelles se terminent par Future (ex. BondFuture)
                        //Les valeurs combinées comportent un UnderScore (ex. Bond_BondFuture) et se terminent par Future
                        pddl.Items.Add(new ListItem(s, s));
                    }
                }
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }

        #endregion DDLLoad_UnderlyingAsset
        #region DDLLoad_VolatilityMatrix
        public static void DDLLoad_VolatilityMatrix(DropDownList pddl )
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Default"), "Default"));
        }
        #endregion DDLLoad_VolatilityMatrix
        #region DDLLoad_WeeklyRollConvention
        public static void DDLLoad_WeeklyRollConvention(DropDownList pddl)
        {
            DDLLoad_WeeklyRollConvention(pddl, false);
        }
        public static void DDLLoad_WeeklyRollConvention(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.WeeklyRollConvention)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_WeeklyRollConvention
        #region DDLLoad_Weight
        public static void DDLLoad_Weight(HtmlSelect pddl)
        {
            DDLLoad_Weight(pddl, false);
        }
        public static void DDLLoad_Weight(HtmlSelect pddl, bool pWithEmpty)
        {
            int[] valuesForWeight = { 1, 10, 100, 1000 };
            for (int i = 0; i < valuesForWeight.Length; i++)
            {
                pddl.Items.Add(new ListItem("Text", valuesForWeight[i].ToString()));
            }

            for (int i = 0; i < pddl.Items.Count; i++)
                pddl.Items[i].Attributes.Add("style", "weight:" + pddl.Items[i].Value);

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_Weight
        #region DDLLoad_WriteMode
        public static void DDLLoad_WriteMode(DropDownList pddl)
        {
            DDLLoad_WriteMode(pddl, false);
        }
        public static void DDLLoad_WriteMode(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.WriteMode)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion
        #region DDLLoad_AgregateFunction
        public static void DDLLoad_AgregateFunction(DropDownList pddl)
        {
            DDLLoad_AgregateFunction(pddl, false);
        }
        public static void DDLLoad_AgregateFunction(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.AgregateFunction)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_AgregateFunction
        #region DDLLoad_AddListItemAllAll
        public static void DDLLoad_AddListItemAllAll(DropDownList pddl)
        {
            if (null != pddl)
            {
                ListItem listItem = new ListItem(Ressource.GetString("DDL_All"), Cst.DDLVALUE_ALL_Old);
                if (!pddl.Items.Contains(listItem))
                    pddl.Items.Insert(0, listItem);
            }
        }
        #endregion DDLLoad_AddListItemAllAll
        #region DDLLoad_AddListItemEmptyEmpty
        /// <summary>
        /// Ajoute dans la DropDown {pddl} in item en position 0 avec les valeurs Empty pour Text et Value
        /// </summary>
        /// <param name="pddl"></param>
        public static void DDLLoad_AddListItemEmptyEmpty(DropDownList pddl)
        {
            if (null != pddl)
            {
                ListItem listItem = new ListItem(string.Empty, string.Empty);
                if (!pddl.Items.Contains(listItem))
                    pddl.Items.Insert(0, listItem);
            }
        }
        #endregion DDLLoad_AddListItemEmptyEmpty
        #region DDLLoad_AddListItemEmptyZero
        public static void DDLLoad_AddListItemEmptyZero(DropDownList pddl)
        {
            if (null != pddl)
            {
                ListItem listItem = new ListItem(string.Empty, "0".ToString());
                if (!pddl.Items.Contains(listItem))
                    pddl.Items.Insert(0, listItem);
            }
        }
        #endregion DDLLoad_AddListItemEmptyZero
        #region DDLLoad_RemoveListItemEmptyEmpty
        public static void DDLLoad_RemoveListItemEmptyEmpty(DropDownList pddl)
        {
            if (null != pddl)
            {
                ListItem listItem = new ListItem(string.Empty, string.Empty);
                if (pddl.Items.Contains(listItem))
                    pddl.Items.Remove(listItem);
            }
        }
        #endregion DDLLoad_RemoveListItemEmptyEmpty
        #region DDLLoad_FlowTypeEnum
        public static void DDLLoad_FlowTypeEnum(DropDownList pddl)
        {
            DDLLoad_FlowTypeEnum(pddl, false);
        }
        public static void DDLLoad_FlowTypeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.FlowTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(typeof(Cst.FlowTypeEnum).Name + "_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_FlowTypeEnum
        #region DDLLoad_ConfirmationRecipientType
        public static void DDLLoad_ConfirmationRecipientType(DropDownList pddl)
        {
            DDLLoad_ConfirmationRecipientType(pddl, false);
        }
        // EG 20160404 Migration vs2013
        public static void DDLLoad_ConfirmationRecipientType(DropDownList pddl, bool pWithEmpty)
        {
            // EG 20160404 Migration vs2013
            //foreach (string s in Enum.GetNames(typeof(NotificationRecipientTypeEnum)))
            foreach (string s in Enum.GetNames(typeof(NotificationSendToClass)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));

            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_ConfirmationRecipientType

        #region DDLLoad_ConfAccessKeyEnum
        public static void DDLLoad_ConfAccessKeyEnum(DropDownList pddl)
        {
            DDLLoad_ConfAccessKeyEnum(pddl, false);
        }
        public static void DDLLoad_ConfAccessKeyEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(NotificationAccessKeyEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_ConfAccessKeyEnum
        #region DDLLoad_MatchingMode
        public static void DDLLoad_MatchingMode(DropDownList pddl)
        {
            DDLLoad_MatchingMode(pddl, false);
        }
        public static void DDLLoad_MatchingMode(DropDownList pddl, bool pWithEmpty)
        {
            Cst.MatchingMode matchingmode;
            foreach (string s in Enum.GetNames(typeof(Cst.MatchingMode)))
            {
                matchingmode = (Cst.MatchingMode)Enum.Parse(typeof(Cst.MatchingMode), s);
                switch (matchingmode)
                {
                    case Cst.MatchingMode.Dynamic:
                    case Cst.MatchingMode.Unknown:
                        break;
                    default:
                        pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
                        break;
                }
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MatchingMode
        #region DDLLoad_StepLifeEnum
        public static void DDLLoad_StepLifeEnum(DropDownList pddl)
        {
            DDLLoad_StepLifeEnum(pddl, false);
        }
        public static void DDLLoad_StepLifeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(NotificationStepLifeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(typeof(NotificationStepLifeEnum).Name + "_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_StepLifeEnum
        #region DDLLoad_FeeFormulaEnum
        public static void DDLLoad_FeeFormulaEnum(DropDownList pddl)
        {
            DDLLoad_FeeFormulaEnum(pddl, false);
        }
        public static void DDLLoad_FeeFormulaEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(FeeFormulaEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_FeeFormulaEnum
        #region DDLLoad_FeeScopeEnum
        public static void DDLLoad_FeeScopeEnum(DropDownList pddl)
        {
            DDLLoad_FeeScopeEnum(pddl, false);
        }
        public static void DDLLoad_FeeScopeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.FeeScopeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_FeeScopeEnum

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pAdditionalFilter"></param>
        /// FI 20180328 [23871] Add
        public static void DLLLoad_Fee(string pCS, DropDownList pddl, bool pIsAddALL, bool pIsClear, string pAdditionalFilter)
        {
            DLLLoad_FeeTable(pCS, pddl, Cst.OTCml_TBL.FEE, pIsAddALL, pIsClear, pAdditionalFilter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pAdditionalFilter"></param>
        /// FI 20180328 [23871] Add
        public static void DLLLoad_FeeMatrix(string pCS, DropDownList pddl, bool pIsAddALL, bool pIsClear, string pAdditionalFilter)
        {
            DLLLoad_FeeTable(pCS, pddl, Cst.OTCml_TBL.FEEMATRIX, pIsAddALL, pIsClear, pAdditionalFilter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pAdditionalFilter"></param>
        /// FI 20180328 [23871] Add
        public static void DLLLoad_FeeSchedule(string pCS, DropDownList pddl, bool pIsAddALL, bool pIsClear, string pAdditionalFilter)
        {
            DLLLoad_FeeTable(pCS, pddl, Cst.OTCml_TBL.FEESCHEDULE, pIsAddALL, pIsClear, pAdditionalFilter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="table"></param>
        /// <param name="pIsAddALL"></param>
        /// <param name="pIsClear"></param>
        /// <param name="pAdditionalFilter"></param>
        /// FI 20180328 [23871] Add
        private static void DLLLoad_FeeTable(string pCS, DropDownList pddl, Cst.OTCml_TBL table, bool pIsAddALL, bool pIsClear, string pAdditionalFilter)
        {
            DataParameters parameters = new DataParameters();

            string colValue = StrFunc.AppendFormat("f.{0}", OTCmlHelper.GetColunmID(table.ToString()));
            string colText = "f.IDENTIFIER || ' - ' || f.DISPLAYNAME";
            if (table == Cst.OTCml_TBL.FEESCHEDULE)
                colText += "|| ' (' || fee.IDENTIFIER || ')'";

            StrBuilder SQLQuery = new StrBuilder(SQLCst.SELECT);
            SQLQuery += StrFunc.AppendFormat("{0} as VALUE, {1} as TEXT", colValue, colText) + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + table + " f" + Cst.CrLf;
            if (table == Cst.OTCml_TBL.FEESCHEDULE)
                SQLQuery += "inner join dbo.FEE fee on fee.IDFEE = f.IDFEE" + Cst.CrLf;


            SQLQuery += new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, "f")).ToString();
            SQLQuery += pAdditionalFilter;

            QueryParameters queryParameters = new QueryParameters(pCS, SQLQuery.ToString(), parameters);
            DDLLoad(pddl, "TEXT", "VALUE", pIsClear, 0, null, pCS, queryParameters.Query, false, true, parameters.GetArrayDbParameter());

            if (pIsAddALL)
                pddl.Items.Insert(0, new ListItem(Ressource.GetString(table.ToString() + "_ALL"), "-1"));
        }

        #region DDLLoad_FeeExchangeTypeEnum
        public static void DDLLoad_FeeExchangeTypeEnum(DropDownList pddl)
        {
            DDLLoad_FeeExchangeTypeEnum(pddl, false);
        }
        public static void DDLLoad_FeeExchangeTypeEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(FeeExchangeTypeEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_FeeExchangeTypeEnum
        #region DDLLoad_TypePartyEnum
        public static void DDLLoad_TypePartyEnum(DropDownList pddl)
        {
            DDLLoad_TypePartyEnum(pddl, false);
        }
        public static void DDLLoad_TypePartyEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(TypePartyEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TypePartyEnum
        #region DDLLoad_TypeSidePartyEnum
        public static void DDLLoad_TypeSidePartyEnum(DropDownList pddl)
        {
            DDLLoad_TypeSidePartyEnum(pddl, false);
        }
        public static void DDLLoad_TypeSidePartyEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(TypeSidePartyEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TypeSidePartyEnum

        //CC 20120621 TRIM 17921
        #region DDLLoad_TypeInformationMessage
        public static void DDLLoad_TypeInformationMessage(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString(Cst.TypeInformationMessage.Information.ToString())));
            pddl.Items.Add(new ListItem(Ressource.GetString(Cst.TypeInformationMessage.Warning.ToString())));
            pddl.Items.Add(new ListItem(Ressource.GetString(Cst.TypeInformationMessage.Alert.ToString())));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TypeInformationMessage

        #region DDLLoad_TypeHelpDisplay
        public static void DDLLoad_TypeHelpDisplay(DropDownList pddl, bool pWithEmpty)
        {
            pddl.Items.Add(new ListItem(Ressource.GetString("Label", false), "Label"));
            pddl.Items.Add(new ListItem(Ressource.GetString("Tooltip", false), "Tooltip"));
            pddl.Items.Add(new ListItem(Ressource.GetString("Popup", false), "Popup"));
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TypeHelpDisplay
        #region DDLLoad_DenOptionActionType
        // EG 20151102 [21465]
        public static void DDLLoad_DenOptionActionType(DropDownList pddl)
        {
            DDLLoad_DenOptionActionType(pddl, false);
        }
        public static void DDLLoad_DenOptionActionType(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.DenOptionActionType)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s + "_denOptionActionType", true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_DenOptionActionType 
        #region DDLLoad_DDLOptionITM_ATM_OTM
        public static void DDLLoad_DDLOptionITM_ATM_OTM(DropDownList pddl)
        {
            DDLLoad_DDLOptionITM_ATM_OTM(pddl, false);
        }
        // EG 20160224 Upd Mise à jour couleur en phase avec celle du datagrid
        public static void DDLLoad_DDLOptionITM_ATM_OTM(DropDownList pddl, bool pWithEmpty)
        {
            ListItem li = new ListItem("In the money", "ITM");
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.BackgroundColor, CstCSSColor.green);
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, "#fff");
            pddl.Items.Add(li);
            li = new ListItem("At the money", "ATM");
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.BackgroundColor, CstCSSColor.blue);
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, "#fff");
            pddl.Items.Add(li);
            li = new ListItem("Out the money", "OTM");
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.BackgroundColor, CstCSSColor.red);
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, "#fff");
            pddl.Items.Add(li);
            li = new ListItem("NOT In the money", "NTM");
            li.Attributes.CssStyle.Add(HtmlTextWriterStyle.Color, CstCSSColor.orange);
            pddl.Items.Add(li);
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_DDLOptionITM_ATM_OTM
        //CC 20140617 [19923]
        #region DDLLoad_RequestTrackMode
        public static void DDLLoad_RequestTrackMode(DropDownList pddl)
        {
            DDLLoad_RequestTrackMode(pddl, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        /// FI 20141021 [20350] Modify
        public static void DDLLoad_RequestTrackMode(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.RequestTrackMode)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("RequestTrackMode_" + s), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_RequestTrackMode

        #region DDLLoad_TypeMarketEnum
        public static void DDLLoad_TypeMarketEnum(DropDownList pddl)
        {
            DDLLoad_TypeMarketEnum(pddl, false);
        }
        public static void DDLLoad_TypeMarketEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(TypeMarketEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TypeMarketEnum

        #region DDLLoad_ReadyStateEnum
        public static void DDLLoad_ReadyStateEnum(DropDownList pddl)
        {
            DDLLoad_ReadyStateEnum(pddl, false);
        }
        public static void DDLLoad_ReadyStateEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(ProcessStateTools.ReadyStateEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("ReadyState" + s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_ReadyStateEnum
        #region DDLLoad_StatusTrackerEnum
        public static void DDLLoad_StatusTrackerEnum(DropDownList pddl)
        {
            DDLLoad_StatusTrackerEnum(pddl, false);
        }
        public static void DDLLoad_StatusTrackerEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(ProcessStateTools.StatusEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("Status" + s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_StatusTrackerEnum

        #region DDLLoad_GroupTrackerEnum
        public static void DDLLoad_GroupTrackerEnum(DropDownList pddl)
        {
            DDLLoad_GroupTrackerEnum(pddl, false);
        }
        public static void DDLLoad_GroupTrackerEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.GroupTrackerEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString("Trk" + s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TaxApplicationEnum


        #region DDLLoad_TradeSideEnum
        public static void DDLLoad_TradeSideEnum(DropDownList pddl)
        {
            DDLLoad_TradeSideEnum(pddl, false);
        }
        public static void DDLLoad_TradeSideEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(TradeSideEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TradeSideEnum
        #region DDLLoad_TaxApplicationEnum
        public static void DDLLoad_TaxApplicationEnum(DropDownList pddl)
        {
            DDLLoad_TaxApplicationEnum(pddl, false);
        }
        public static void DDLLoad_TaxApplicationEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(TaxApplicationEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_TaxApplicationEnum
        #region DDLLoad_PaymentRuleEnum
        public static void DDLLoad_PaymentRuleEnum(DropDownList pddl)
        {
            DDLLoad_PaymentRuleEnum(pddl, false);
        }
        public static void DDLLoad_PaymentRuleEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(PaymentRuleEnum)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_PaymentRuleEnum
        #region DDLLoad_InvoiceApplicationPeriodEnum
        public static void DDLLoad_InvoiceApplicationPeriodEnum(DropDownList pddl)
        {
            DDLLoad_InvoiceApplicationPeriodEnum(pddl, false);
        }
        public static void DDLLoad_InvoiceApplicationPeriodEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(EfsML.Enum.InvoiceApplicationPeriodEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString("InvoiceApplicationPeriodEnum_" + s), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_InvoiceApplicationPeriodEnum
        #region DDLLoad_BracketApplicationEnum
        public static void DDLLoad_BracketApplicationEnum(DropDownList pddl)
        {
            DDLLoad_BracketApplicationEnum(pddl, false);
        }
        public static void DDLLoad_BracketApplicationEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(EfsML.Enum.BracketApplicationEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString("BracketApplicationEnum_" + s), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_BracketApplicationEnum
        #region DDLLoad_InvoicingSortEnum
        public static void DDLLoad_InvoicingSortEnum(DropDownList pddl)
        {
            DDLLoad_InvoicingSortEnum(pddl, false);
        }
        public static void DDLLoad_InvoicingSortEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(InvoicingSortEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_InvoicingSortEnum
        #region DDLLoad_InvoicingTradeDetailEnum
        public static void DDLLoad_InvoicingTradeDetailEnum(DropDownList pddl)
        {
            DDLLoad_InvoicingTradeDetailEnum(pddl, false);
        }
        public static void DDLLoad_InvoicingTradeDetailEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(InvoicingTradeDetailEnum)))
                pddl.Items.Add(new ListItem(s, s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_InvoicingTradeDetailEnum
        #region DDLLoad_MaturityMonthYearFmtEnum
        public static void DDLLoad_MaturityMonthYearFmtEnum(DropDownList pddl)
        {
            DDLLoad_MaturityMonthYearFmtEnum(pddl, false);
        }
        public static void DDLLoad_MaturityMonthYearFmtEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MaturityMonthYearFmtEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MaturityMonthYearFmtEnum
        #region DDLLoad_MaturityMonthYearIncrUnitEnum
        public static void DDLLoad_MaturityMonthYearIncrUnitEnum(DropDownList pddl)
        {
            DDLLoad_MaturityMonthYearIncrUnitEnum(pddl, false);
        }
        public static void DDLLoad_MaturityMonthYearIncrUnitEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.MaturityMonthYearIncrUnitEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_MaturityMonthYearIncrUnitEnum
        #region DDLLoad_CfiCodeCategoryEnum
        // EG 20091222 Use EFSmL.Enum.CfiCodeCategoryEnum
        public static void DDLLoad_CfiCodeCategoryEnum(DropDownList pddl)
        {
            DDLLoad_CfiCodeCategoryEnum(pddl, false);
        }
        public static void DDLLoad_CfiCodeCategoryEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(CfiCodeCategoryEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_CfiCodeCategoryEnum
        #region DDLLoad_DerivativeExerciseStyleEnum
        // EG 20091222 Use FixML.v50SP1.DerivativeExerciseStyleEnum
        public static void DDLLoad_DerivativeExerciseStyleEnum(DropDownList pddl)
        {
            DDLLoad_DerivativeExerciseStyleEnum(pddl, false);
        }
        public static void DDLLoad_DerivativeExerciseStyleEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(DerivativeExerciseStyleEnum)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_DerivativeExerciseStyleEnum

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        public static void DDLLoad_MathStatusEnum(DropDownList pddl)
        {
            DDLLoad_MathStatusEnum(pddl, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        public static void DDLLoad_MathStatusEnum(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(EFS.SpheresIO.MatchStatus)))
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pddl"></param>
        /// <param name="pWithEmpty"></param>
        public static void DDLLoad_GrpMarketCNF(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string table = Cst.OTCml_TBL.GMARKET.ToString();

            StrBuilder SQLQuery = new StrBuilder(SQLCst.SELECT);
            SQLQuery += "gm.DISPLAYNAME,gm.IDGMARKET" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + table + " gm" + Cst.CrLf;
            SQLQuery += SQLCst.INNERJOIN_DBO + "GMARKETROLE gmr on (gmr.IDGMARKET=gm.IDGMARKET)" + Cst.CrLf;


            SQLWhere where = new SQLWhere(OTCmlHelper.GetSQLDataDtEnabled(pCS, "gm"));
            where.Append("gmr.IDROLEGMARKET='CNF'");
            SQLQuery += where.ToString() + Cst.CrLf;

            DDLLoad(pddl, "DISPLAYNAME", "IDGMARKET", pCS, SQLQuery.ToString(), pWithEmpty, true, null);
        }





        #region DDLLoad_xxx with DDLLoad_ENUM()
        #region DDLLoad_ENUM
        public static void DDLLoad_ENUM(DropDownList pddl, string pSource, LoadEnumArguments pLoadEnumArg)
        {
            DDLLoad_ENUM(null, pddl, pSource, pLoadEnumArg.isWithEmpty,
                                        pLoadEnumArg.code,
                                        pLoadEnumArg.isFromSQLView,
                                        pLoadEnumArg.isResource,
                                        pLoadEnumArg.resourcePrefix, //PL 20140128
                                        ArrFunc.IsFilled(pLoadEnumArg.forcedEnum) ? new ArrayList(pLoadEnumArg.forcedEnum) : null,
                                        pLoadEnumArg.isExcludeForcedEnum,
                                        pLoadEnumArg.isDisplayValue,
                                        pLoadEnumArg.isDisplayValueAndExtendValue,
                                        pLoadEnumArg.customValue,
                                        pLoadEnumArg.condition);
        }
        public static void DDLLoad_ENUM(PageBase pPage, DropDownList pddl, string pSource, LoadEnumArguments pLoadEnumArg)
        {
            DDLLoad_ENUM(pPage, pddl, pSource, pLoadEnumArg.isWithEmpty,
                                        pLoadEnumArg.code,
                                        pLoadEnumArg.isFromSQLView,
                                        pLoadEnumArg.isResource,
                                        pLoadEnumArg.resourcePrefix, //PL 20140128
                                        ArrFunc.IsFilled(pLoadEnumArg.forcedEnum) ? new ArrayList(pLoadEnumArg.forcedEnum) : null,
                                        pLoadEnumArg.isExcludeForcedEnum,
                                        pLoadEnumArg.isDisplayValue,
                                        pLoadEnumArg.isDisplayValueAndExtendValue,
                                        pLoadEnumArg.customValue,
                                        pLoadEnumArg.condition);
        }
        public static void DDLLoad_ENUM(DropDownList pddl, string pSource, bool pWithEmpty, string pCode)
        {
            bool fromSQLView = false;
            bool isResource = false;
            DDLLoad_ENUM(null, pddl, pSource, pWithEmpty, pCode, fromSQLView, isResource, null, null, false, false, false, null, null);
        }
        private static void DDLLoad_ENUM(DropDownList pddl, string pSource, bool pWithEmpty, string pCode, bool pFromSQLView)
        {
            bool isResource = false;
            DDLLoad_ENUM(null, pddl, pSource, pWithEmpty, pCode, pFromSQLView, isResource, null, null, false, false, false, null, null);
        }
        private static void DDLLoad_ENUM(DropDownList pddl, string pSource, bool pWithEmpty, string pCode, bool pFromSQLView, bool pIsResource)
        {
            DDLLoad_ENUM(null, pddl, pSource, pWithEmpty, pCode, pFromSQLView, pIsResource, null, null, false, false, false, null, null);
        }
        private static void DDLLoad_ENUM(PageBase pPage, DropDownList pddl, string pSource, bool pWithEmpty,
            string pCode, bool pFromSQLView, bool pIsResource, string pResourcePrefix, ArrayList pAlForcedEnum,
            bool pIsExcludeForcedEnum, bool pIsWithoutExtendValue,
            bool pIsDisplayValueAndExtendValue, string pCustomValue, string pCondition)
        {
            #region additionalWhere
            string additionalWhere = string.Empty;
            //
            if (ArrFunc.IsFilled(pAlForcedEnum))
            {
                additionalWhere += SQLCst.AND + DataHelper.SQLUpper(pSource, "VALUE");
                if (pIsExcludeForcedEnum)
                    additionalWhere += SQLCst.NOT;
                additionalWhere += " in (" + DataHelper.SQLCollectionToSqlList(pSource, pAlForcedEnum, TypeData.TypeDataEnum.@string, true) + ")" + Cst.CrLf;
            }
            //
            if (StrFunc.IsFilled(pCondition))
                additionalWhere += SQLCst.AND + pCondition;
            #endregion

            #region sqlQuery
            string sqlSelect = SQLCst.SELECT + "VALUE, ";
            if (pIsDisplayValueAndExtendValue)
                sqlSelect += DataHelper.SQLConcat(pSource, "VALUE", "' - '", "EXTVALUE") + " as EXTVALUE";
            else
                sqlSelect += "EXTVALUE";
            sqlSelect += ",FORECOLOR,BACKCOLOR,SOURCE" + Cst.CrLf;

            //PL 20141017 Tip à finaliser...
            if (pCode == "FeeFormulaEnum")
            {
                sqlSelect += @",
case when VALUE='CONST' then 'CONST'
     when substring(VALUE,1,2)='F1' then 'F1'
	 when substring(VALUE,1,2)='F2' then 'F2'
	 when VALUE in ('F3KOD','F3MOD') then 'F3D'	 
	 when substring(VALUE,1,2)='F3' or VALUE='F4' then 'F3F4'
	 when substring(VALUE,1,2)='F4' then 'F4X'
	 when substring(VALUE,1,2)='F5' then 'F5'
	 else 'Autres' end as OPTGROUP,
case when VALUE='CONST' then 'CONST_Group'
     when substring(VALUE,1,2)='F1' then 'F1_Group'
	 when substring(VALUE,1,2)='F2' then 'F2_Group'
	 when VALUE in ('F3KOD','F3MOD') then 'F3D_Group'	 
	 when substring(VALUE,1,2)='F3' or VALUE='F4' then 'F3F4_Group'	 
	 when substring(VALUE,1,2)='F4' then 'F4X_Group'
	 when substring(VALUE,1,2)='F5' then 'F5_Group'
	 else 'OTHERS_Group' end as OPTGROUPTEXT,
case when VALUE='CONST' then 0
     when substring(VALUE,1,2)='F1' then 5
	 when substring(VALUE,1,2)='F2' then 6
	 when VALUE in ('F3KOD','F3MOD') then 4	 
	 when substring(VALUE,1,2)='F3' or VALUE='F4' then 3
	 when substring(VALUE,1,2)='F4' then 1
	 when substring(VALUE,1,2)='F5' then 2
	 else 9 end as OPTGROUPSORT
" + Cst.CrLf;
            }

            string sqlQuery = sqlSelect;
            sqlQuery += SQLCst.FROM_DBO + (pFromSQLView ? Cst.OTCml_TBL.VW_ALL_VW_ENUM.ToString() : Cst.OTCml_TBL.VW_ALL_ENUM.ToString()) + Cst.CrLf;
            //EG 20070713 Add % at end of like clause
            sqlQuery += SQLCst.WHERE + DataHelper.SQLUpper(pSource, "CODE") + SQLCst.LIKE + DataHelper.SQLUpper(pSource, DataHelper.SQLString(pCode + "%"));
            sqlQuery += SQLCst.AND + "ISENABLED=" + DataHelper.SQL_Bit_True.ToString();
            sqlQuery += additionalWhere;

            if (StrFunc.IsFilled(pCustomValue))
            {
                //PL 20100325 Add start ";"
                string additionalWhereCustomValue = SQLCst.AND
                    + DataHelper.SQLConcat(pSource, DataHelper.SQLString(";"), DataHelper.SQLConcat(pSource, DataHelper.SQLUpper(pSource, "CUSTOMVALUE"), DataHelper.SQLString(";")))
                    + SQLCst.LIKE
                    + DataHelper.SQLConcat(pSource, DataHelper.SQLString("%;"), DataHelper.SQLUpper(pSource, DataHelper.SQLString(pCustomValue)), DataHelper.SQLString(";%"));

                string sqlQuryTmp = sqlQuery + additionalWhereCustomValue + Cst.CrLf;
                sqlQuryTmp += SQLCst.UNION + Cst.CrLf;
                sqlQuryTmp += sqlQuery + SQLCst.AND + SQLCst.NOT_EXISTS + "(" + sqlQuery + additionalWhereCustomValue + ")";

                sqlQuery = sqlQuryTmp;
            }
            #endregion

            string columnText = "EXTVALUE";
            if (pIsWithoutExtendValue)
                columnText = "VALUE";

            //20110201 PL New isWithSort et pIsRessource
            bool isWithSort = (pCode.IndexOf("RollConvention") < 0);
            //PL 20141017
            DDLLoad(pPage, pddl, columnText, "VALUE", pSource, sqlQuery, pWithEmpty, isWithSort, pIsResource, pResourcePrefix, null);

            if (Cst.IsAssessmentBasisEnum(pCode.ToLower()))
            {
                #region Load Extends
                DataRow[] drInstrExtension = InstrTools.GetInstrExtension(pSource, "dextd.IDENTIFIER,dext.IDENTIFIER");
                //
                if (ArrFunc.IsFilled(drInstrExtension))
                {
                    foreach (DataRow rowExtension in drInstrExtension)
                    {
                        int idDefineExtendDet = Convert.ToInt32(rowExtension["IDDEFINEEXTENDDET"]);
                        string identifier = Convert.ToString(rowExtension["IDENTIFIER"]);
                        string identifierDet = Convert.ToString(rowExtension["IDENTIFIERDET"]);
                        string dataType = (DBNull.Value == rowExtension["DATATYPE"] ? string.Empty : Convert.ToString(rowExtension["DATATYPE"]));
                        //
                        if (dataType == TypeData.TypeDataEnum.@decimal.ToString() || dataType == TypeData.TypeDataEnum.integer.ToString())
                        {
                            ListItem item = new ListItem(identifierDet + " - " + identifier, idDefineExtendDet.ToString());
                            item.Attributes.Add("style", "color:" + Color.OrangeRed.Name + ";");
                            pddl.Items.Add(item);
                        }
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pSource"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pCode"></param>
        /// <param name="pIsRessource"></param>
        /// <param name="pAlForcedEnum"></param>
        /// <param name="pIsExcludeForcedEnum"></param>
        /// <param name="pIsWithoutExtendValue"></param>
        private static void DDLLoad_EnumStatus(DropDownList pddl, string pSource, bool pWithEmpty,
            string pCode, bool pIsRessource, ArrayList pAlForcedEnum,
            bool pIsExcludeForcedEnum, bool pIsWithoutExtendValue)
        {
            #region additionalWhere
            string additionalWhere = string.Empty;
            if (pAlForcedEnum.Count > 0)
            {
                additionalWhere = SQLCst.AND + DataHelper.SQLUpper(pSource, "VALUE");
                if (pIsExcludeForcedEnum)
                    additionalWhere += SQLCst.NOT;
                additionalWhere += " in (";
                additionalWhere += DataHelper.SQLCollectionToSqlList(pSource, pAlForcedEnum, TypeData.TypeDataEnum.@string).ToUpper();
                additionalWhere += ")" + Cst.CrLf;
            }
            #endregion

            #region sqlQuery
            StrBuilder sqlQuery = new StrBuilder();
            sqlQuery += SQLCst.SELECT + "VALUE, EXTVALUE, FORECOLOR, BACKCOLOR, 1 as ColForOrderBy" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_ENUM.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + DataHelper.SQLUpper(pSource, "CODE") + SQLCst.LIKE + DataHelper.SQLUpper(pSource, DataHelper.SQLString(pCode + "%"));
            sqlQuery += SQLCst.AND + "VALUE=" + DataHelper.SQLString(Cst.STATUSREGULAR) + Cst.CrLf;
            sqlQuery += additionalWhere;
            sqlQuery += SQLCst.UNIONALL + Cst.CrLf;
            sqlQuery += SQLCst.SELECT + "VALUE, EXTVALUE, FORECOLOR, BACKCOLOR, 2 as ColForOrderBy" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_ENUM.ToString() + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + DataHelper.SQLUpper(pSource, "CODE") + SQLCst.LIKE + DataHelper.SQLUpper(pSource, DataHelper.SQLString(pCode + "%"));
            sqlQuery += SQLCst.AND + "VALUE!=" + DataHelper.SQLString(Cst.STATUSREGULAR) + Cst.CrLf;
            sqlQuery += additionalWhere;
            sqlQuery += SQLCst.ORDERBY + "ColForOrderBy";
            #endregion

            bool withSort = false;
            if (pIsWithoutExtendValue)
                DDLLoad(pddl, "VALUE", "VALUE", pSource, sqlQuery.ToString(), pWithEmpty, withSort, null);
            else
                DDLLoad(pddl, "EXTVALUE", "VALUE", pSource, sqlQuery.ToString(), pWithEmpty, withSort, null);

            if (pIsRessource)
                DDLSetRessourceFromValue(pddl);
        }
        #endregion DDLLoad_ENUM
        //
        #region DDLLoad_StatusEnvironment
        public static void DDLLoad_StatusEnvironment(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_StatusEnvironment(pCS, pddl, pWithEmpty, new ArrayList(), false);
        }
        public static void DDLLoad_StatusEnvironment(string pCS, DropDownList pddl, bool pWithEmpty, ArrayList pAlForcedStatus, bool pIsExcludeForcedStatus)
        {
            DDLLoad_EnumStatus(pddl, pCS, pWithEmpty, "StatusEnvironment", true, pAlForcedStatus, pIsExcludeForcedStatus, false);
        }
        #endregion DDLLoad_StatusEnvironment
        #region DDLLoad_StatusBusiness
        public static void DDLLoad_StatusBusiness(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_StatusBusiness(pCS, pddl, pWithEmpty, new ArrayList(), false);
        }
        public static void DDLLoad_StatusBusiness(string pCS, DropDownList pddl, bool pWithEmpty, ArrayList pAlForcedStatus, bool pIsExcludeForcedStatus)
        {
            DDLLoad_EnumStatus(pddl, pCS, pWithEmpty, "StatusBusiness", true, pAlForcedStatus, pIsExcludeForcedStatus, false);
        }
        //public static void DDLLoad_StatusBusiness(string pCS, DropDownList pddl, bool pWithEmpty)
        //{
        //    //
        //    foreach (string s in Enum.GetNames(typeof(Cst.StatusBusiness)))
        //        pddl.Items.Add(new ListItem(Ressource.GetString(s), s));
        //    //
        //    if (pWithEmpty)
        //        pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        //}
        #endregion
        #region DDLLoad_StatusActivation
        //public static void DDLLoad_StatusActivation(HtmlSelect pddl)
        //{
        //    DDLLoad_StatusActivation(pddl, false);
        //}
        public static void DDLLoad_StatusActivation(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "StatusActivation", true);
        }
        public static void DDLLoad_StatusActivation(string pCS, DropDownList pddl, bool pWithEmpty, ArrayList pAlForcedStatus, bool pIsExcludeForcedStatus)
        {
            DDLLoad_EnumStatus(pddl, pCS, pWithEmpty, "StatusActivation", true, pAlForcedStatus, pIsExcludeForcedStatus, false);
        }
        #endregion DDLLoad_StatusActivation
        #region DDLLoad_StatusProcess
        public static void DDLLoad_StatusProcess(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "StatusProcess", true);
        }
        #endregion DDLLoad_StatusProcess
        #region DDLLoad_StatusPriority
        //public static void DDLLoad_StatusPriority(HtmlSelect pddl)
        //{
        //    DDLLoad_StatusPriority(pddl, false);
        //}
        public static void DDLLoad_StatusPriority(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "StatusPriority", true);
        }
        #endregion DDLLoad_StatusPriority

        // CC 20120625 17939
        #region DDLLoad_StatusTask
        public static void DDLLoad_StatusTask(DropDownList pddl, bool pWithEmpty)
        {
            foreach (string s in Enum.GetNames(typeof(Cst.StatusTask)))
            {
                pddl.Items.Add(new ListItem(Ressource.GetString(s, true), s));
            }
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_StatusTask

        // CC 20120625 17870
        #region DDLLoad_ActorAmountType
        public static void DDLLoad_ActorAmountType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "ActorAmountType", true);
        }
        #endregion DDLLoad_ActorAmountType

        #region DDLLoad_StatusMatch
        public static void CBLLoad_StatusMatch(string pCS, CheckBoxList pckl, bool pIsDisplayWeight)
        {
            string SQLQuery = SQLCst.SELECT + "IDSTMATCH, ";
            if (pIsDisplayWeight)
            {
                SQLQuery += DataHelper.SQLConcat(pCS,
                    "DISPLAYNAME",
                    "' ('",
                    DataHelper.SQLString(Ressource.GetString("WEIGHT") + ": "),
                    DataHelper.SQLNumberToChar(pCS, "WEIGHT"),
                    "')'");
                SQLQuery += " as DISPLAYNAME,";
            }
            else
            {
                SQLQuery += "DISPLAYNAME,";
            }
            SQLQuery += "FORECOLOR, BACKCOLOR" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.STMATCH.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "WEIGHT" + SQLCst.ASC + ",DISPLAYNAME";
            DDLLoad(pckl, "DISPLAYNAME", "IDSTMATCH", pCS, SQLQuery, false, false, null);
        }
        //public static void DDLLoad_StatusMatch(HtmlSelect pddl)
        //{
        //    DDLLoad_StatusMatch(pddl, false);
        //}
        public static void DDLLoad_StatusMatch(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string SQLQuery = SQLCst.SELECT + "IDSTMATCH, DISPLAYNAME, FORECOLOR, BACKCOLOR" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.STMATCH.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "WEIGHT" + SQLCst.ASC + ",DISPLAYNAME";
            DDLLoad(pddl, "DISPLAYNAME", "IDSTMATCH", pCS, SQLQuery, pWithEmpty, false, null);
        }
        #endregion DDLLoad_StatusMatch
        #region DDLLoad_StatusCheck
        public static void CBLLoad_StatusCheck(string pCS, CheckBoxList pckl, bool pIsDisplayWeight)
        {
            string SQLQuery = SQLCst.SELECT + "IDSTCHECK, ";
            if (pIsDisplayWeight)
            {
                SQLQuery += DataHelper.SQLConcat(pCS,
                    "DISPLAYNAME",
                    "' ('",
                    DataHelper.SQLString(Ressource.GetString("WEIGHT") + ": "),
                    DataHelper.SQLNumberToChar(pCS, "WEIGHT"),
                    "')'");
                SQLQuery += " as DISPLAYNAME,";
            }
            else
            {
                SQLQuery += "DISPLAYNAME,";
            }
            SQLQuery += "FORECOLOR, BACKCOLOR" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.STCHECK.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "WEIGHT" + SQLCst.ASC + ",DISPLAYNAME";
            DDLLoad(pckl, "DISPLAYNAME", "IDSTCHECK", pCS, SQLQuery, false, false, null);
        }
        //public static void DDLLoad_StatusCheck(HtmlSelect pddl)
        //{
        //    DDLLoad_StatusCheck(pddl, false);
        //}
        public static void DDLLoad_StatusCheck(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string SQLQuery = SQLCst.SELECT + "IDSTCHECK, DISPLAYNAME, FORECOLOR, BACKCOLOR" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.STCHECK.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "WEIGHT" + SQLCst.ASC + ",DISPLAYNAME";
            DDLLoad(pddl, "DISPLAYNAME", "IDSTCHECK", pCS, SQLQuery, pWithEmpty, false, null);
        }
        #endregion DDLLoad_StatusCheck

        //
        #region DDLLoad_xxx (enum)
        public static void DDLLoad_QuotationSide(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "QuotationSideEnum", isFromSqlView, isRessource);
        }
        public static void DDLLoad_InterpolationMethod(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "InterpolationMethod", isFromSqlView, isRessource);
        }
        public static void DDLLoad_MatrixInterpolationMethod(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "MatrixInterpolationMethod", isFromSqlView, isRessource);
        }
        public static void DDLLoad_PreSettlementMethod(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "PreSettlementDateMethodDeterminationEnum", isFromSqlView, isRessource);
        }

        public static void DDLLoad_MatrixStructure(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "MatrixStructure", isFromSqlView, isRessource);
        }
        public static void DDLLoad_QuoteTiming(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "QuoteTiming", isFromSqlView, isRessource);
        }
        public static void DDLLoad_SettlSessIDEnum(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "SettlSessIDEnum", isFromSqlView, isRessource);
        }
        public static void DDLLoad_PriceQuoteUnits(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "PriceQuoteUnits", isFromSqlView, isRessource);
        }
        public static void DDLLoad_AssetMeasure(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "AssetMeasure", isFromSqlView, isRessource);
        }
        public static void DDLLoad_CashFlow(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "cashFlow", isFromSqlView, isRessource);
        }
        public static void DDLLoad_InputSource(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "inputSource", isFromSqlView, isRessource);
        }
        #region DDLLoad_SubsetPrice
        public static void DDLLoad_SubsetPrice(string pCS, DropDownList pddl)
        {
            DDLLoad_SubsetPrice(pCS, pddl, false);
        }
        public static void DDLLoad_SubsetPrice(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "SubsetPrice");
        }
        #endregion DDLLoad_SubsetPrice

        #region DDLLoad_SettlementTypeEnum
        public static void DDLLoad_SettlementTypeEnum(string pCS, DropDownList pddl)
        {
            DDLLoad_SettlementTypeEnum(pCS, pddl, false);
        }
        public static void DDLLoad_SettlementTypeEnum(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "SettlementTypeEnum");
        }
        #endregion DDLLoad_SettlementTypeEnum

        #region DDLLoad_StandardSettlementStyle
        public static void DDLLoad_StandardSettlementStyle(string pCS, DropDownList pddl)
        {
            DDLLoad_StandardSettlementStyle(pCS, pddl, false);
        }
        public static void DDLLoad_StandardSettlementStyle(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "StandardSettlementStyleEnum");
        }
        #endregion DDLLoad_StandardSettlementStyle

        #region DDLLoad_OTCmlSettlementType
        public static void DDLLoad_OTCmlSettlementType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_StandardSettlementStyle(pCS, pddl, pWithEmpty);
            pddl.Items.Add(new ListItem(Cst.SettlementTypeEnum.Instruction.ToString(), Cst.SettlementTypeEnum.Instruction.ToString()));
            pddl.Items.Add(new ListItem(Cst.SettlementTypeEnum.None.ToString(), Cst.SettlementTypeEnum.None.ToString()));
        }

        #endregion

        #region  DDLLoad_DayCountFraction
        public static void DDLLoad_DayCountFraction(string pCS, DropDownList pddl)
        {
            DDLLoad_DayCountFraction(pCS, pddl, false);
        }
        public static void DDLLoad_DayCountFraction(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_DayCountFraction(pCS, pddl, pWithEmpty, false);
        }
        public static void DDLLoad_DayCountFraction(string pCS, DropDownList pddl, bool pWithEmpty, bool pFromSQLView)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "DayCountFractionEnum", pFromSQLView);
        }
        #endregion  DDLLoad_DayCountFraction

        #region  DDLLoad_AveragingMethod
        public static void DDLLoad_AveragingMethod(string pCS, DropDownList pddl)
        {
            DDLLoad_AveragingMethod(pCS, pddl, false);
        }
        public static void DDLLoad_AveragingMethod(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "AveragingMethodEnum");
        }
        #endregion  DDLLoad_AveragingMethod

        #region  DDLLoad_QuoteBasis
        public static void DDLLoad_QuoteBasis(string pCS, DropDownList pddl)
        {
            DDLLoad_QuoteBasis(pCS, pddl, false);
        }
        public static void DDLLoad_QuoteBasis(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = true;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "QuoteBasisEnum", isFromSqlView, isRessource);
        }
        #endregion  DDLLoad_QuoteBasis

        #region DDLLoad_RequestMode
        public static void DDLLoad_RequestMode(string pCS, DropDownList pddl)
        {
            DDLLoad_RequestMode(pCS, pddl, false);
        }
        public static void DDLLoad_RequestMode(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = true;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "SettlSessIDEnum", isFromSqlView, isRessource);
        }
        #endregion

        #region DDLLoad_TouchCondition
        public static void DDLLoad_TouchCondition(string pCS, DropDownList pddl)
        {
            DDLLoad_TouchCondition(pCS, pddl, false);
        }
        public static void DDLLoad_TouchCondition(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = true;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "TouchConditionEnum", isFromSqlView, isRessource);
        }
        #endregion

        #region DDLLoad_TriggerCondition
        public static void DDLLoad_TriggerCondition(string pCS, DropDownList pddl)
        {
            DDLLoad_TriggerCondition(pCS, pddl, false);
        }
        public static void DDLLoad_TriggerCondition(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = true;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "TriggerConditionEnum", isFromSqlView, isRessource);
        }
        #endregion

        #region DDLLoad_RollConvention
        public static void DDLLoad_RollConvention(string pCS, DropDownList pddl)
        {
            DDLLoad_RollConvention(pCS, pddl, false);
        }
        public static void DDLLoad_RollConvention(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_RollConvention(pCS, pddl, pWithEmpty, false);
        }
        public static void DDLLoad_RollConvention(string pCS, DropDownList pddl, bool pWithEmpty, bool pFromSQLView)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "RollConventionEnum", pFromSQLView);
        }
        #endregion

        #region DDLLoad_PaymentType
        public static void DDLLoad_PaymentType(string pCS, DropDownList pddl)
        {
            DDLLoad_PaymentType(pCS, pddl, false);
        }
        public static void DDLLoad_PaymentType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(null, pddl, pCS, pWithEmpty, "PaymentType", false, false, null, null, false, false, false, null, null);
        }
        #endregion

        #region DDLLoad_PaymentTypeForADP
        public static void DDLLoad_PaymentTypeForADP(string pCS, DropDownList pddl)
        {
            DDLLoad_PaymentTypeForADP(pCS, pddl, false);
        }
        public static void DDLLoad_PaymentTypeForADP(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(null, pddl, pCS, pWithEmpty, "PaymentType", false, false, null, null, false, false, false, "ADP", null);
        }
        #endregion

        #region DDLLoad_PaymentTypeForOPP
        public static void DDLLoad_PaymentTypeForOPP(string pCS, DropDownList pddl)
        {
            DDLLoad_PaymentTypeForOPP(pCS, pddl, false);
        }
        public static void DDLLoad_PaymentTypeForOPP(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(null, pddl, pCS, pWithEmpty, "PaymentType", false, false, null, null, false, false, false, "OPP", null);
        }
        #endregion

        #region DDLLoad_Period
        public static void DDLLoad_Period(string pCS, DropDownList pddl)
        {
            DDLLoad_Period(pCS, pddl, false);
        }
        public static void DDLLoad_Period(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = true;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "PeriodEnum", isFromSqlView, isRessource);
        }
        #endregion

        #region DDLLoad_PosEfct_OpenCloseOnly
        public static void DDLLoad_PosEfct_OpenCloseOnly(string pCS, DropDownList pddl)
        {
            DDLLoad_PosEfct_OpenCloseOnly(pCS, pddl, false);
        }
        public static void DDLLoad_PosEfct_OpenCloseOnly(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            ArrayList alListe = new ArrayList
            {
                "O",
                "C"
            };

            DDLLoad_ENUM(null, pddl, pCS, pWithEmpty, "PositionEffectEnum", false, false, null, alListe, false, false, false, null, null);
        }
        #endregion

        #region DDLLoad_DayType
        public static void DDLLoad_DayType(string pCS, DropDownList pddl)
        {
            DDLLoad_DayType(pCS, pddl, false);
        }
        public static void DDLLoad_DayType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "DayTypeEnum");
        }
        #endregion

        #region DDLLoad_DayTypeEntry
        public static void DDLLoad_DayTypeEntry(string pCS, DropDownList pddl)
        {
            DDLLoad_DayTypeEntry(pCS, pddl, false);
        }
        public static void DDLLoad_DayTypeEntry(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            ArrayList dayTypeListe = new ArrayList
            {
                "Business",
                "Calendar"
            };
            DDLLoad_ENUM(null, pddl, pCS, pWithEmpty, "DayTypeEnum", false, false, null, dayTypeListe, false, false, false, null, null);
        }
        #endregion

        #region DDLLoad_DisplayValue
        /// <summary>
        /// Vider la DDL et insère un nouvel item avec la valeur présente dans SelectedItem 
        /// </summary>
        /// <param name="pddl"></param>
        public static void DDLLoadSingle_SelectedItem(DropDownList pddl)
        {
            string value = ((null != pddl.SelectedItem) ? pddl.SelectedItem.Value : string.Empty);
            DDLLoadSingle_Value(pddl, value);
        }
        /// <summary>
        /// Purge la DDL pour ne conserver que l'item {pValue}
        ///<para>Si l'item pValue n'existe pas, il est créé</para>
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pValue"></param>
        public static void DDLLoadSingle_Value(DropDownList pddl, string pValue)
        {
            ListItem listItem = null;
            if (StrFunc.IsFilled(pValue))
            {
                // Si une donnée est saisie alors:
                // - Obtenir l'item correspondant à la valeur, dans le but d'avoir le text à afficher
                // Sinon 
                // - afficher la donnée telle qu'elle
                listItem = pddl.Items.FindByValue(pValue);
                if (null == listItem)
                    listItem = new ListItem(pValue, pValue);
            }

            pddl.Items.Clear();

            if (null != listItem)
                pddl.Items.Add(listItem);
        }
        #endregion

        #region DDLLoad_DividendEntitlement
        public static void DDLLoad_DividendEntitlement(string pCS, DropDownList pddl)
        {
            DDLLoad_DividendEntitlement(pCS, pddl, false);
        }
        public static void DDLLoad_DividendEntitlement(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "DividendEntitlementEnum");
        }
        #endregion

        #region DDLLoad_DividendPeriod
        /// EG 20140702 New
        public static void DDLLoad_DividendPeriod(string pCS, DropDownList pddl)
        {
            DDLLoad_DividendPeriod(pCS, pddl, false);
        }
        public static void DDLLoad_DividendPeriod(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "DividendPeriodEnum");
        }
        #endregion

        #region DDLLoad_DividendAmount
        public static void DDLLoad_DividendAmount(string pCS, DropDownList pddl)
        {
            DDLLoad_DividendEntitlement(pCS, pddl, false);
        }
        #endregion

        #region DDLLoad_DividendAmountType
        public static void DDLLoad_DividendAmountType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "DividendAmountTypeEnum");
        }
        #endregion

        #region DDLLoad_DividendDateReference
        public static void DDLLoad_DividendDateReference(string pCS, DropDownList pddl)
        {
            DDLLoad_DividendDateReference(pCS, pddl, false);
        }
        public static void DDLLoad_DividendDateReference(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "DividendDateReferenceEnum");
        }

        #endregion

        #region DDLLoad_CommissionDenomination
        public static void DDLLoad_CommissionDenomination(string pCS, DropDownList pddl)
        {
            DDLLoad_CommissionDenomination(pCS, pddl, false);
        }
        public static void DDLLoad_CommissionDenomination(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "CommissionDenominationEnum", isFromSqlView, isRessource);
        }

        #endregion

        #region DDLLoad_PriceExpression
        public static void DDLLoad_PriceExpression(string pCS, DropDownList pddl)
        {
            DDLLoad_PriceExpression(pCS, pddl, false);
        }
        public static void DDLLoad_PriceExpression(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "PriceExpressionEnum", isFromSqlView, isRessource);
        }

        #endregion

        #region DDLLoad_BusinessDayConvention
        public static void DDLLoad_BusinessDayConvention(string pCS, DropDownList pddl)
        {
            DDLLoad_BusinessDayConvention(pCS, pddl, false);
        }
        public static void DDLLoad_BusinessDayConvention(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = true;
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "BusinessDayConventionEnum", isFromSqlView, isRessource);
        }
        #endregion

        #region DDLLoad_ValScenario
        public static void DDLLoad_ValScenario(string pCS, DropDownList pddl)
        {
            DDLLoad_ValScenario(pCS, pddl, false);
        }
        public static void DDLLoad_ValScenario(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            string SQLQuery = SQLCst.SELECT + "IDVALSCENARIO, DISPLAYNAME" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VALSCENARIO.ToString();
            DDLLoad(pddl, "DISPLAYNAME", "IDVALSCENARIO", pCS, SQLQuery, pWithEmpty, true, null);
        }
        #endregion
        #region DDLLoad_ValuationTimeType
        public static void DDLLoad_ValuationTimeType(string pCS, DropDownList pddl)
        {
            DDLLoad_RateTreatment(pCS, pddl, false);
        }
        public static void DDLLoad_ValuationTimeType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "TimeTypeEnum");
        }

        #endregion

        #region DDLLoad_ReturnType
        public static void DDLLoad_ReturnType(string pCS, DropDownList pddl)
        {
            DDLLoad_ReturnType(pCS, pddl, false);
        }
        public static void DDLLoad_ReturnType(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "ReturnTypeEnum");
        }
        #endregion

        #region DDLLoad_RateTreatment
        public static void DDLLoad_RateTreatment(string pCS, DropDownList pddl)
        {
            DDLLoad_RateTreatment(pCS, pddl, false);
        }
        public static void DDLLoad_RateTreatment(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "RateTreatmentEnum");
        }

        #endregion

        #region DDLLoad_CallPut
        public static void DDLLoad_CallPut(string pCS, DropDownList pddl)
        {
            DDLLoad_CallPut(pCS, pddl, false);
        }
        public static void DDLLoad_CallPut(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            bool isFromSqlView = false;
            bool isRessource = false;

            ArrayList al = new ArrayList();
            //al.Add(OptionTypeEnum.Forward.ToString()); 

            DDLLoad_ENUM(null, pddl, pCS, pWithEmpty, "OptionTypeEnum", isFromSqlView, isRessource, null, al, true, true, false, null, null);
        }
        #endregion

        #region DDLLoad_ReferenceAmount
        public static void DDLLoad_ReferenceAmount(string pCS, DropDownList pddl)
        {
            DDLLoad_ReferenceAmount(pCS, pddl, false);
        }
        public static void DDLLoad_ReferenceAmount(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "ReferenceAmount", true);
        }
        #endregion DDLLoad_ReferenceAmount

        #region DDLLoad_PayoutStyle
        public static void DDLLoad_Payout(string pCS, DropDownList pddl)
        {
            DDLLoad_Payout(pCS, pddl, false);
        }
        public static void DDLLoad_Payout(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "PayoutEnum", true);
        }
        #endregion DDLLoad_PayoutStyle

        #region DDLLoad_CompoundingMethod
        public static void DDLLoad_CompoundingMethod(string pCS, DropDownList pddl)
        {
            DDLLoad_CompoundingMethod(pCS, pddl, false);
        }
        public static void DDLLoad_CompoundingMethod(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "CompoundingMethodEnum");
        }
        #endregion

        #region public DDLLoad_EurosysWebCatPropriet
        public static void DDLLoad_EurosysWebCatPropriet(string pCS, DropDownList pddl)
        {
            DDLLoad_EurosysWebCatPropriet(pCS, pddl, false);
        }
        public static void DDLLoad_EurosysWebCatPropriet(string pCS, DropDownList pddl, bool pWithEmpty)
        {
            DDLLoad_ENUM(pddl, pCS, pWithEmpty, "CatPropriet", false, true);
        }
        #endregion
        #endregion DDLLoad_xxx (enum)
        #endregion DDLLoad_xxx with DDLLoad_ENUM()

        #region DDLLoad_FromListRetrieval
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pListRetrieval"></param>
        /// <param name="opListRetrievalEnum"></param>
        /// <param name="opListRetrievalData"></param>
        /// <returns></returns>
        public static bool DecomposeListRetrieval(string pListRetrieval,
            out Cst.ListRetrievalEnum opListRetrievalEnum, out string opListRetrievalData)
        {
            bool isOk = false;
            opListRetrievalEnum = Cst.ListRetrievalEnum.PREDEF;
            opListRetrievalData = string.Empty;

            if (StrFunc.IsFilled(pListRetrieval))
            {
                pListRetrieval = pListRetrieval.Trim();
                //count 2 est necessaire car il peut y avoir des : ds arrayListRetrieval[1]
                string[] arrayListRetrieval = pListRetrieval.Split(":".ToCharArray(), 2);
                //
                if ((arrayListRetrieval.Length >= 2) && Enum.IsDefined(typeof(Cst.ListRetrievalEnum), arrayListRetrieval[0].ToUpper()))
                {
                    opListRetrievalEnum = (Cst.ListRetrievalEnum)Enum.Parse(typeof(Cst.ListRetrievalEnum), arrayListRetrieval[0], true);
                    //
                    switch (opListRetrievalEnum)
                    {
                        case Cst.ListRetrievalEnum.PREDEF:
                            opListRetrievalData = arrayListRetrieval[1].Trim().ToLower();
                            isOk = true;
                            break;
                        case Cst.ListRetrievalEnum.DELIMLIST:
                            opListRetrievalData = arrayListRetrieval[1].Trim();
                            if (StrFunc.IsFilled(opListRetrievalData) && !opListRetrievalData.EndsWith(";"))
                                opListRetrievalData += ";";
                            isOk = true;
                            break;
                        //20090624 PL
                        case Cst.ListRetrievalEnum.SQL:
                            opListRetrievalData = arrayListRetrieval[1].Trim();
                            isOk = true;
                            break;
                        default:
                            isOk = false;
                            break;
                    }
                }
            }
            else
            {
                isOk = false;
            }
            return isOk;
        }
        //
        public static void DDLLoad_FromListRetrieval(Control pDdl, string pSource, string pListRetrieval)
        {
            bool isWithEmpty = false;
            string misc = string.Empty;
            DDLLoad_FromListRetrieval(pDdl, pSource, pListRetrieval, isWithEmpty, misc);
        }
        public static void DDLLoad_FromListRetrieval(Control pDdl, string pSource, string pListRetrieval,
            bool pIsWithEmpty, string pMisc)
        {
            DDLLoad_FromListRetrieval(null, pDdl, pSource, pListRetrieval, pIsWithEmpty, false, pMisc);
        }
        public static void DDLLoad_FromListRetrieval(PageBase pPage, Control pDdl, string pSource, string pListRetrieval,
            bool pIsWithEmpty, string pMisc)
        {
            DDLLoad_FromListRetrieval(pPage, pDdl, pSource, pListRetrieval, pIsWithEmpty, false, pMisc);
        }
        public static void DDLLoad_FromListRetrieval(PageBase pPage, Control pDdl, string pSource, string pListRetrieval,
            bool pIsWithEmpty, bool pIsResource, string pMisc)
        {
            if (ControlsTools.DecomposeListRetrieval(pListRetrieval, out Cst.ListRetrievalEnum listRetrievalType, out string listRetrievalData))
                DDLLoad_FromListRetrieval(pPage, pDdl, pSource, listRetrievalType, listRetrievalData, pIsWithEmpty, pIsResource, pMisc, string.Empty);
        }
        public static void DDLLoad_FromListRetrieval(Control pDdl, string pSource, Cst.ListRetrievalEnum pListRetrievalType,
            string pListRetrievalData)
        {
            bool isWithEmpty = false;
            string misc = string.Empty;
            DDLLoad_FromListRetrieval(pDdl, pSource, pListRetrievalType, pListRetrievalData, isWithEmpty, misc);
        }
        public static void DDLLoad_FromListRetrieval(Control pddl, string pSource, Cst.ListRetrievalEnum pListRetrievalType,
            string pListRetrievalData, bool pIsWithEmpty, string pMisc)
        {
            bool isResource = false;
            DDLLoad_FromListRetrieval(pddl, pSource, pListRetrievalType, pListRetrievalData, pIsWithEmpty, isResource, pMisc);
        }
        public static void DDLLoad_FromListRetrieval(Control pddl, string pSource, Cst.ListRetrievalEnum pListRetrievalType,
            string pListRetrievalData, bool pIsWithEmpty, bool pIsResource, string pMisc)
        {
            DDLLoad_FromListRetrieval(null, pddl, pSource, pListRetrievalType, pListRetrievalData, pIsWithEmpty, pIsResource, pMisc, string.Empty);
        }
        /// EG 20140702 Add IsDDLTypeDeterminationMethodValuationPrice |  IsDDLDividendPeriod | IsUnderlyingAsset_ReturnSwap
        /// RD 20150325 [20820] Modify
        /// EG 20170929 [23450][22374] New (MapZone)
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void DDLLoad_FromListRetrieval(PageBase pPage, Control pddl, string pSource, Cst.ListRetrievalEnum pListRetrievalType,
            string pListRetrievalData, bool pIsWithEmpty, bool pIsResource, string pMisc, string pObjectsPath)
        {
            bool isAddALL = false;
            bool isClear = true;
            bool isUseSessionRestrict = true;
            bool isMiscResource = false;
            string dataType = string.Empty;
            Nullable<int> displayNameMaxLen = null; // FI 20180424 [23871] Add

            if (StrFunc.IsFilled(pMisc))
            {
                CustomObject co = new CustomObject
                {
                    Misc = pMisc
                };
                isAddALL = co.GetMiscValue("isAddAll", (false == pIsWithEmpty));
                isUseSessionRestrict = co.GetMiscValue("isUseSessionRestrict", true);
                isClear = co.GetMiscValue("isClear", true);
                // RD 20121121 / Gérer isResource dans l'attribut "misc" des DDL avec ListRetrieval 
                isMiscResource = co.GetMiscValue("isResource", false);
                pIsResource = pIsResource || isMiscResource;
                dataType = co.GetMiscValue("dataType", string.Empty);
                // FI 20180424 [23871] Alimentation de displayNameMaxLen
                string sdisplayNameMaxLen = co.GetMiscValue("displayNameMaxLen", string.Empty);
                if (StrFunc.IsFilled(sdisplayNameMaxLen))
                    displayNameMaxLen = Convert.ToInt32(sdisplayNameMaxLen);
            }

            switch (pListRetrievalType)
            {
                case Cst.ListRetrievalEnum.DELIMLIST:
                    #region Loading
                    string[] dataListRetrieval = pListRetrievalData.Split(";".ToCharArray());
                    DropDownList ddList = (DropDownList)pddl;
                    //
                    for (int i = 0; i < dataListRetrieval.Length - 1; i++)
                    {
                        string[] dataText = dataListRetrieval[i].Split("|".ToCharArray());

                        // RD 20120316 / Gérére le paramètre {pIsResource}
                        string itemText;
                        string itemValue;
                        if (dataText.GetLength(0) == 2)
                        {
                            itemText = dataText[1];
                            itemValue = dataText[0];
                            //ddList.Items.Add(new ListItem(dataText[1], dataText[0]));
                        }
                        else
                        {
                            itemText = dataListRetrieval[i];
                            itemValue = dataListRetrieval[i];
                            //ddList.Items.Add(new ListItem(dataListRetrieval[i], dataListRetrieval[i]));
                        }

                        if (pIsResource)
                            itemText = Ressource.GetString(itemText, itemText);

                        ddList.Items.Add(new ListItem(itemText, itemValue));
                    }
                    if (pIsWithEmpty)
                        ddList.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                    //
                    DropDownTools.DDLAddMiscItem(pddl, pMisc);
                    //
                    #endregion
                    //
                    break;

                case Cst.ListRetrievalEnum.SQL:
                    // RD 20131218 [19368] Manage "misc" attribute for DDL with ListRetrieval as SQL
                    //ControlsTools.DDLLoad(pddl, "Text", "Value", pSource, pListRetrievalData, pIsWithEmpty, null);
                    // RD 20150325 [20820] Use isMiscResource
                    //ControlsTools.DDLLoad(pddl, "Text", "Value", isClear, pSource, pListRetrievalData, pIsWithEmpty, pMisc,
                    //    pIsResource, null);
                    ControlsTools.DDLLoad(pddl, "Text", "Value", isClear, pSource, pListRetrievalData, pIsWithEmpty, pMisc,
                        isMiscResource, null);
                    break;

                case Cst.ListRetrievalEnum.PREDEF:
                    #region Loading
                    if (
                        pddl.GetType().Equals(typeof(HtmlSelect))
                        ||
                        pddl.GetType().Equals(typeof(WCHtmlSelect))
                        ||
                        pddl.GetType().BaseType.Equals(typeof(HtmlSelect))
                        )
                    {
                        //if (Cst.IsDDLBuySellType(pListRetrievalData))
                        //    ControlsTools.DDLLoad_BuySellType(ddl, pIsWithEmpty);
                        //else if (Cst.IsDDLBuySellCur1Type(pListRetrievalData))
                        //    ControlsTools.DDLLoad_BuySellCur1Type(ddl, pIsWithEmpty);
                        //else if (Cst.IsDDLPayRecType(pListRetrievalData))
                        //    ControlsTools.DDLLoad_PayRecType(ddl, pIsWithEmpty);
                        //else if (Cst.IsDDLLendBorrowType(pListRetrievalData))
                        //    ControlsTools.DDLLoad_LendBorrowType(ddl, pIsWithEmpty);
                        ////else if (Cst.IsDDLTypeColor(pListRetrievalData) || Cst.IsDDLTypeStyleComponentColor(pListRetrievalData))
                        ////    ControlsTools.DDLLoad_Color(ddl, pIsWithEmpty);
                        //else if (Cst.IsDDLBuyerSellerType(pListRetrievalData))
                        //    ControlsTools.DDLLoad_BuyerSellerType(ddl, pIsWithEmpty);
                    }
                    else
                    {
                        DropDownList ddl = (DropDownList)pddl;
                        #region Specific Load
                        if (Cst.IsDDLEmpty(pListRetrievalData))
                        {
                            ddl.Items.Clear();
                            ddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                        }
                        else if (Cst.IsDDLErrorOnLoad(pListRetrievalData))
                            ControlsTools.DDLLoad_ErrorOnLoad(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLMandatoryOptionalType(pListRetrievalData))
                            ControlsTools.DDLLoad_MandatoryOptionalType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLBuySellType(pListRetrievalData))
                            ControlsTools.DDLLoad_BuySellType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLBuySellFixType(pListRetrievalData))
                            ControlsTools.DDLLoad_BuySellFixType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLBuySellCur1Type(pListRetrievalData))
                            ControlsTools.DDLLoad_BuySellCur1Type(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLXXXYYYType(pListRetrievalData))
                            ControlsTools.DDLLoad_XXXYYYType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLPayRecType(pListRetrievalData))
                            ControlsTools.DDLLoad_PayerReceiverType(ddl, pIsWithEmpty);
                        // EG 20230526 [WI640] New : Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
                        else if (Cst.IsDDLInvoicePayRecType(pListRetrievalData))
                            ControlsTools.DDLLoad_InvoicePayerReceiverType(ddl, pIsWithEmpty);
                        //PL 20180531
                        else if (Cst.IsDDLCRDRType(pListRetrievalData))
                            ControlsTools.DDLLoad_CreditDebit(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDepositWithdrawalType(pListRetrievalData))
                            ControlsTools.DDLLoad_DepositWithdrawal(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLLendBorrowType(pListRetrievalData))
                            ControlsTools.DDLLoad_LendBorrowType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeColor(pListRetrievalData) || Cst.IsDDLTypeStyleComponentColor(pListRetrievalData))
                            ControlsTools.DDLLoad_Color(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLBuyerSellerType(pListRetrievalData))
                            ControlsTools.DDLLoad_BuyerSellerType(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLTypeMenu(pListRetrievalData))
                            ControlsTools.DDLLoad_Menu(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePermission(pListRetrievalData))
                            ControlsTools.DDLLoad_Permission(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCurrency(pListRetrievalData))
                            ControlsTools.DDLLoad_Currency(pSource, ddl, pIsWithEmpty, pMisc);
                        else if (Cst.IsDDLTypePeriod(pListRetrievalData))
                            ControlsTools.DDLLoad_Period(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeDayCountFraction_String(pListRetrievalData))
                        {
                            //Used from Capture (load DAY1 à la place de 1)
                            ControlsTools.DDLLoad_DayCountFraction(pSource, ddl, pIsWithEmpty, true);
                        }
                        else if (Cst.IsDDLTypeRollConvention_String(pListRetrievalData))
                        {
                            //Used from Capture (load DCF1.1 à la place de 1/1)
                            ControlsTools.DDLLoad_RollConvention(pSource, ddl, pIsWithEmpty, true);
                        }
                        else if (Cst.IsDDLTypeBusinessDayConvention(pListRetrievalData))
                            ControlsTools.DDLLoad_BusinessDayConvention(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePaymentType(pListRetrievalData))
                            ControlsTools.DDLLoad_PaymentType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeExerciseStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_ExerciseStyle(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLOptionType(pListRetrievalData))
                            ControlsTools.DDLLoad_OptionType(pSource, ddl, pIsWithEmpty);
                        //else if (Cst.IsDDLTypeBusinessCenter(pListRetrievalData))
                        //    ControlsTools.DDLLoad_BusinessCenter(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeQuoteBasis(pListRetrievalData))
                            ControlsTools.DDLLoad_QuoteBasis(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePremiumQuoteBasis(pListRetrievalData))
                            ControlsTools.DDLLoad_PremiumQuoteBasis(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeStrikeQuoteBasis(pListRetrievalData))
                            ControlsTools.DDLLoad_StrikeQuoteBasis(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCutName(pListRetrievalData))
                            ControlsTools.DDLLoad_CutName(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeSettlementType(pListRetrievalData))
                            ControlsTools.DDLLoad_SettlementTypeEnum(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeOTCmlSettlementType(pListRetrievalData))
                            ControlsTools.DDLLoad_OTCmlSettlementType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeStandardSettlementStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_StandardSettlementStyle(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePriceExpression(pListRetrievalData))
                            ControlsTools.DDLLoad_PriceExpression(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCommissionDenomination(pListRetrievalData))
                            ControlsTools.DDLLoad_CommissionDenomination(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePriceExpression(pListRetrievalData))
                            ControlsTools.DDLLoad_PriceExpression(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeDeterminationMethodValuation(pListRetrievalData))
                            ControlsTools.DDLLoad_DeterminationMethodValuation(ddl, pIsWithEmpty);
                        // EG 20140526 New
                        else if (Cst.IsDDLTypeDeterminationMethodValuationPrice(pListRetrievalData))
                            ControlsTools.DDLLoad_DeterminationMethodValuationPrice(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeValuationTime(pListRetrievalData))
                            ControlsTools.DDLLoad_ValuationTimeType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLReturnType(pListRetrievalData))
                            ControlsTools.DDLLoad_ReturnType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDayType(pListRetrievalData))
                            ControlsTools.DDLLoad_DayType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDayTypeEntry(pListRetrievalData))
                            ControlsTools.DDLLoad_DayTypeEntry(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDividendEntitlement(pListRetrievalData))
                            ControlsTools.DDLLoad_DividendEntitlement(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDividendPeriod(pListRetrievalData))
                            ControlsTools.DDLLoad_DividendPeriod(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDividendAmountType(pListRetrievalData))
                            ControlsTools.DDLLoad_DividendAmountType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLDividendDateReference(pListRetrievalData))
                            ControlsTools.DDLLoad_DividendDateReference(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLReferenceAmount(pListRetrievalData))
                            ControlsTools.DDLLoad_ReferenceAmount(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLPayout(pListRetrievalData))
                            ControlsTools.DDLLoad_Payout(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLCallPut(pListRetrievalData))
                            ControlsTools.DDLLoad_CallPut(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRequestMode(pListRetrievalData))
                            ControlsTools.DDLLoad_RequestMode(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeTouchCondition(pListRetrievalData))
                            ControlsTools.DDLLoad_TouchCondition(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeTriggerCondition(pListRetrievalData))
                            ControlsTools.DDLLoad_TriggerCondition(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeInformationProvider(pListRetrievalData))
                            ControlsTools.DDLLoad_InformationProvider(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLFxBarrierType(pListRetrievalData))
                            ControlsTools.DDLLoad_FxBarrierType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeEnum(pListRetrievalData))
                        {
                            LoadEnumArguments loadEnumArg = LoadEnumArguments.GetArguments(pListRetrievalData, pIsWithEmpty, pIsResource);
                            if (null != loadEnumArg)
                            {
                                //PL 20141017
                                ControlsTools.DDLLoad_ENUM(pPage, ddl, pSource, loadEnumArg);
                                DropDownTools.DDLAddMiscItem(pddl, pMisc);
                            }
                        }
                        // EG 20170929 [23450][22374] New
                        else if (Cst.IsDDLTypeMapZone(pListRetrievalData))
                        {
                            Tz.Web.LoadMapZoneArguments args = Tz.Web.LoadMapZoneArguments.GetArguments(pListRetrievalData, pIsWithEmpty);
                            if (null != args)
                                Tz.Web.LoadMapZone(ddl, args);
                        }
                        else if (pListRetrievalData.StartsWith("PARTYTEMPLATE_TRADER."))
                        {
                            string idValue = StrFunc.Before(StrFunc.After(pListRetrievalData, "PARTYTEMPLATE_TRADER.[IDA:"), "]");
                            if (int.TryParse(idValue, out int idA))
                            {
                                DDL_LoadTrader(SessionTools.CS, ddl, pIsIDinValue: true, idA, SessionTools.User, SessionTools.SessionID);
                            }
                            if (pIsWithEmpty)
                                ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);
                        }
                        else if (pListRetrievalData.StartsWith("PARTYTEMPLATE_SALES."))
                        {
                            string idValue = StrFunc.Before(StrFunc.After(pListRetrievalData, "PARTYTEMPLATE_SALES.[IDPARTYTEMPLATE:"), "]");
                            if (int.TryParse(idValue, out int idPartTemplate))
                            {
                                DataParameters dp = new DataParameters();
                                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.ID), idPartTemplate);
                                QueryParameters qryParameters = new QueryParameters(SessionTools.CS, "select IDA,IDA_ENTITY from dbo.PARTYTEMPLATE where IDPARTYTEMPLATE=@ID", dp);
                                using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text,
                                    qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                                {
                                    if (dr.Read())
                                    {
                                        DDL_LoadSales(SessionTools.CS, ddl, pIsIDinValue: true, Convert.ToInt32(dr["IDA"]), Convert.ToInt32(dr["IDA_ENTITY"]), SessionTools.User, SessionTools.SessionID);
                                        if (pIsWithEmpty)
                                            ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);
                                    }
                                }
                            }
                        }
                        else if (Cst.IsDDLTypeStatusProcess(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusProcess(ddl);
                        else if (Cst.IsDDLTypeAllProcessType(pListRetrievalData))
                            ControlsTools.DDLLoad_AllProcessType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeProcessType(pListRetrievalData))
                            ControlsTools.DDLLoad_ProcessType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeLogLevel(pListRetrievalData))
                            ControlsTools.DDLLoad_LogLevel(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeLogLevel_Inherited(pListRetrievalData))
                            ControlsTools.DDLLoad_LogLevel_Inherited(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRecursiveLevel(pListRetrievalData))
                            ControlsTools.DDLLoad_RecursiveLevel(ddl);
                        else if (Cst.IsDDLTypePosRequestLevel(pListRetrievalData))
                            ControlsTools.DDLLoad_PosRequestLevel(ddl);
                        else if (Cst.IsDDLTypeBusinessCenter(pListRetrievalData))
                            ControlsTools.DDLLoad_BusinessCenter(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeBusinessCenterId(pListRetrievalData))
                            ControlsTools.DDLLoad_BusinessCenterId(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCountry(pListRetrievalData))
                            ControlsTools.DDLLoad_Country(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCountry_Country(pListRetrievalData))
                            ControlsTools.DDLLoad_Country_Country(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCountryType(pListRetrievalData))
                            ControlsTools.DDLLoad_CountryType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeMarketType(pListRetrievalData))
                            ControlsTools.DDLLoad_MarketType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCulture(pListRetrievalData))
                            ControlsTools.DDLLoad_Culture(ddl, true, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCurrency(pListRetrievalData))
                            ControlsTools.DDLLoad_Currency(pSource, ddl, pIsWithEmpty, pMisc);
                        else if (Cst.IsDDLTypeRoundDir(pListRetrievalData))
                            ControlsTools.DDLLoad_RoundDir(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRoundPrec(pListRetrievalData))
                            ControlsTools.DDLLoad_RoundPrec(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeWeeklyRollConvention(pListRetrievalData))
                            ControlsTools.DDLLoad_WeeklyRollConvention(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePositionDay(pListRetrievalData))
                            ControlsTools.DDLLoad_PositionDay(ddl);
                        else if (Cst.IsDDLTypeDay(pListRetrievalData))
                            ControlsTools.DDLLoad_Day(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeMonth(pListRetrievalData))
                            ControlsTools.DDLLoad_Month(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeHolidayName(pListRetrievalData))
                            ControlsTools.DDLLoad_HolidayName(ddl);
                        else if (Cst.IsDDLTypeHolidayType(pListRetrievalData))
                            ControlsTools.DDLLoad_HolidayType(ddl);
                        else if (Cst.IsDDLTypeDayCountFraction(pListRetrievalData))
                            ControlsTools.DDLLoad_DayCountFraction(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsEFSRateorIndex(pListRetrievalData))
                            ControlsTools.DDLLoad_RateorIndex(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeIndexUnit(pListRetrievalData))
                            ControlsTools.DDLLoad_IndexUnit(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRateType(pListRetrievalData))
                            ControlsTools.DDLLoad_RateType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeAveragingMethod(pListRetrievalData))
                            ControlsTools.DDLLoad_AveragingMethod(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRateValueType(pListRetrievalData))
                            ControlsTools.DDLLoad_RateValueType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRollConvention(pListRetrievalData))
                            ControlsTools.DDLLoad_RollConvention(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePeriod(pListRetrievalData))
                            ControlsTools.DDLLoad_Period(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeQuoteBasis(pListRetrievalData))
                            ControlsTools.DDLLoad_QuoteBasis(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeMarket(pListRetrievalData))
                            // EG 20150907 [21317] Add isAddALL, isUseSessionRestrict, isClear
                            ControlsTools.DDLLoad_Market(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                        else if (Cst.IsDDLTypeMarketId(pListRetrievalData))
                            ControlsTools.DDLLoad_MarketId(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeMarketETD(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_MarketETD(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                        }
                        else if (Cst.IsDDLTypeClearingHouse(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_ClearingHouse(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                        }
                        else if (Cst.IsDDLTypeCssCustodian(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_CssCustodian(pSource, pPage, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear, displayNameMaxLen);
                        }
                        else if (Cst.IsDDLTypeAllRatingValue(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_AllRatingValue(pSource, pPage, ddl, pIsWithEmpty, isClear, pListRetrievalData);
                        }
                        else if (Cst.IsDDLGrpMarketCNF(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_GrpMarketCNF(pSource, ddl, pIsWithEmpty);
                        }
                        else if (Cst.IsDDLTypeInformationProvider(pListRetrievalData))
                            ControlsTools.DDLLoad_InformationProvider(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeFxCurve(pListRetrievalData))
                            ControlsTools.DDLLoad_FxCurve(ddl);
                        else if (Cst.IsDDLTypeVolatilityMatrix(pListRetrievalData))
                            ControlsTools.DDLLoad_VolatilityMatrix(ddl);
                        else if (Cst.IsDDLTypeDayType(pListRetrievalData))
                            ControlsTools.DDLLoad_DayType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeDayTypeEntry(pListRetrievalData))
                            ControlsTools.DDLLoad_DayTypeEntry(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeBusinessDayConvention(pListRetrievalData))
                            ControlsTools.DDLLoad_BusinessDayConvention(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRelativeToPaymentDT(pListRetrievalData))
                            ControlsTools.DDLLoad_RelativeToPaymentDT(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRelativeToResetDT(pListRetrievalData))
                            ControlsTools.DDLLoad_RelativeToResetDT(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRelativeToFixingDT(pListRetrievalData))
                            ControlsTools.DDLLoad_RelativeToFixingDT(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeMaturityRelativeTo(pListRetrievalData))
                            ControlsTools.DDLLoadEnum<Cst.MaturityRelativeTo>(ddl, pIsWithEmpty, pIsResource, "MaturityRelativeTo_");
                        else if (Cst.IsDDLTypePriceCurve(pListRetrievalData))
                            ControlsTools.DDLLoad_PriceCurve(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCalculationRule(pListRetrievalData))
                            ControlsTools.DDLLoad_CalculationRule(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRateTreatment(pListRetrievalData))
                            ControlsTools.DDLLoad_RateTreatment(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCompoundingMethod(pListRetrievalData))
                            ControlsTools.DDLLoad_CompoundingMethod(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCustomisedInput(pListRetrievalData))
                        {
                            int idi = 0;
                            if (StrFunc.IsFilled(pMisc))
                            {
                                CustomObject co = new CustomObject
                                {
                                    Misc = pMisc
                                };
                                idi = Convert.ToInt32(co.GetMiscValue("FK", "0"));
                                if (0 == idi)
                                    idi = Convert.ToInt32(co.GetMiscValue("IDI", "0"));
                            }
                            //ControlsTools.DDLLoad_CustomisedInput(pSource, ddl, idi, pIsWithEmpty, null);
                            ControlsTools.DDLLoad_CustomisedInput(pSource, ddl, idi, pIsWithEmpty);
                        }
                        else if (Cst.IsDDLTypeCaptureGUIType(pListRetrievalData))
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.Capture.GUIType>(ddl, pIsWithEmpty, pIsResource, string.Empty);
                        else if (Cst.IsDDLTypeEurosysFeed_FileExtension(pListRetrievalData))
                            ControlsTools.DDLLoad_EurosysFeed_FileExtension(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeEurosysFeed_YesNo(pListRetrievalData))
                            ControlsTools.DDLLoad_EurosysFeed_YesNo(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeCSSColor(pListRetrievalData))
                            ControlsTools.DDLLoad_CSSColor(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLCheckModeEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_CheckModeEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLReturnSPParamTypeEnum(pListRetrievalData))     //GP 20070124 
                            ControlsTools.DDLLoad_ReturnSPParamTypeEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeDataType(pListRetrievalData))
                        {
                            // 20081113 RD : DDL avec Filtre dans le Referential
                            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments(pListRetrievalData, pIsWithEmpty);
                            string filtre = string.Empty;
                            if (null != loadEnum)
                                filtre = loadEnum.filtre;
                            //
                            ControlsTools.DDLLoad_DataType(ddl, pIsWithEmpty, filtre);
                        }
                        else if (Cst.IsDDLTypeWebControlType(pListRetrievalData))
                        {
                            // 20081113 RD : DDL avec Filtre dans le Referential
                            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments(pListRetrievalData, pIsWithEmpty);
                            string filtre = string.Empty;
                            if (null != loadEnum)
                                filtre = loadEnum.filtre;
                            //
                            ControlsTools.DDLLoad_WebControlType(ddl, pIsWithEmpty, filtre, pObjectsPath);
                        }
                        else if (Cst.IsDDLTypeSubsetPrice(pListRetrievalData))
                            ControlsTools.DDLLoad_SubsetPrice(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeStatusCalculEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusCalcul(ddl);
                        else if (Cst.IsDDLTypeStatusTrigger(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusTrigger(ddl);
                        else if (Cst.IsDDLTypeSource(pListRetrievalData))
                            ControlsTools.DDLLoad_Source(ddl);
                        else if (Cst.IsDDLTypeSourceAndServices(pListRetrievalData))
                            ControlsTools.DDLLoad_SourceAndServices(ddl);
                        else if (Cst.IsDDLTypeCashFlow(pListRetrievalData))
                            ControlsTools.DDLLoad_CashFlow(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeQuoteTiming(pListRetrievalData))
                            ControlsTools.DDLLoad_QuoteTiming(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypePriceQuoteUnits(pListRetrievalData))
                            ControlsTools.DDLLoad_PriceQuoteUnits(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeAssetMeasure(pListRetrievalData))
                            ControlsTools.DDLLoad_AssetMeasure(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeInputSource(pListRetrievalData))
                            ControlsTools.DDLLoad_InputSource(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeQuotationSide(pListRetrievalData))
                            ControlsTools.DDLLoad_QuotationSide(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeProductSource(pListRetrievalData))
                            ControlsTools.DDLLoad_ProductSource(ddl);
                        else if (Cst.IsDDLTypeSymbolAlign(pListRetrievalData))
                            ControlsTools.DDLLoad_SymbolAlign(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLTypeRoleType(pListRetrievalData))
                            ControlsTools.DDLLoad_RoleType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLCtrlLastUser(pListRetrievalData))
                            ControlsTools.DDLLoad_CtrlLastUser(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLExtlType(pListRetrievalData))
                            ControlsTools.DDLLoad_ExtlType(ddl, pIsWithEmpty);
                        //20081118 Add
                        else if (Cst.IsDDLStatusActivation(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusActivation(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLStatusEnvironment(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusEnvironment(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLStatusPriority(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusPriority(pSource, ddl, pIsWithEmpty);
                        // CC 20120625 17939
                        else if (Cst.IsDDLStatusTask(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusTask(ddl, pIsWithEmpty);
                        // CC 20120625 17870
                        else if (Cst.IsDDLActorAmountType(pListRetrievalData))
                            ControlsTools.DDLLoad_ActorAmountType(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLStatusCheck(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusCheck(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLStatusMatch(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusMatch(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsDDLStatusProcess(pListRetrievalData))
                            ControlsTools.DDLLoad_StatusProcess(pSource, ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLTypeMissingUTI(pListRetrievalData))
                            ControlsTools.DDLLoad_MissingUTI(ddl);
                        else if (Cst.IsDDLCtrlStatus(pListRetrievalData))
                            ControlsTools.DDLLoad_CtrlStatus(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLCtrlSending(pListRetrievalData))
                            ControlsTools.DDLLoad_CtrlSending(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLRightType(pListRetrievalData))
                            ControlsTools.DDLLoad_RightType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLCommitMode(pListRetrievalData))
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.CommitMode>(ddl, pIsWithEmpty, pIsResource, string.Empty, Cst.CommitMode.INHERITED.ToString(), true);
                        else if (Cst.IsDDLCommitMode_Inherited(pListRetrievalData))
                            ControlsTools.DDLLoadEnum<Cst.CommitMode>(ddl, pIsWithEmpty, pIsResource, string.Empty);
                        else if (Cst.IsDDLIn_Out(pListRetrievalData))
                            ControlsTools.DDLLoad_In_Out(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLStartInfoStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_StartInfoStyle(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLInputSourceDataStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_InputSourceDataStyle(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLOutputSourceDataStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_OutputSourceDataStyle(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLOutputTargetDataStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_OutputTargetDataStyle(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLIOSerializeMode(pListRetrievalData))
                            ControlsTools.DDLLoad_IoSerializeMode(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLWriteMode(pListRetrievalData))
                            ControlsTools.DDLLoad_WriteMode(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLAlignment(pListRetrievalData))
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.Alignment>(ddl, pIsWithEmpty, pIsResource, string.Empty);
                        else if (Cst.IsDDLParameterDirection(pListRetrievalData))
                            ControlsTools.DDLLoad_ParameterDirection(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLRuleOnError(pListRetrievalData))
                            ControlsTools.DDLLoad_RuleOnError(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLRetCodeOnNoData(pListRetrievalData))
                            ControlsTools.DDLLoad_RetCodeOnNoData(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLRDBMS(pListRetrievalData))
                            ControlsTools.DDLLoad_RDBMS(ddl, pIsWithEmpty);
                        //Create_DropDownList_For_Referential Step4: Add a new test
                        else if (Cst.IsUnderlyingAsset(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset(ddl, pIsWithEmpty);
                        else if (Cst.IsIOElementType(pListRetrievalData))
                            ControlsTools.DDLLoad_IOElementType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLVisibility(pListRetrievalData))
                            ControlsTools.DDLLoad_Visibility(ddl, pIsWithEmpty);
                        // CC 20140723 
                        else if (Cst.IsDDLMarginType(pListRetrievalData))
                            ControlsTools.DDLLoad_MarginType(ddl, pIsWithEmpty);
                        // CC 20150312 
                        else if (Cst.IsDDLMarginingMode(pListRetrievalData))
                            ControlsTools.DDLLoad_MarginingMode(ddl, pIsWithEmpty);
                        // CC 20140724 
                        else if (Cst.IsDDLMarginAssessmentBasis(pListRetrievalData))
                            ControlsTools.DDLLoad_MarginAssessmentBasis(ddl, pIsWithEmpty);
                        // EG 20150320 (POC]
                        else if (Cst.IsDDLFundingType(pListRetrievalData))
                            ControlsTools.DDLLoad_FundingType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLMatchingMode(pListRetrievalData))
                            ControlsTools.DDLLoad_MatchingMode(ddl, pIsWithEmpty);
                        else if (Cst.IsUnderlyingAsset_Rate(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset_Rate(ddl, pIsWithEmpty);
                        else if (Cst.IsUnderlyingAsset_Rating(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset_Rating(ddl, pIsWithEmpty);
                        else if (Cst.IsUnderlyingAsset_ETD(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset_ETD(ddl, pIsWithEmpty, false);
                        else if (Cst.IsUnderlyingAsset_ETD_WithCombinedValues(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset_ETD(ddl, pIsWithEmpty, true); 
                        else if (Cst.IsUnderlyingAsset_ReturnSwap(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset_ReturnSwap(ddl, pIsWithEmpty);

                        else if (Cst.IsUnderlyingAsset_Collateral(pListRetrievalData))
                            ControlsTools.DDLLoad_UnderlyingAsset_Collateral(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLGlobal_Elementary(pListRetrievalData))
                            ControlsTools.DDLLoad_Global_Elementary(ddl);
                        //
                        else if (Cst.IsDDLExchangeType(pListRetrievalData))
                            ControlsTools.DDLLoad_ExchangeType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLEarExchangeEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_EarExchangeEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLEarCommonEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_EarcomonEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLCBExchangeType(pListRetrievalData))
                            ControlsTools.DDLLoad_CBExchangeType(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLQuoteExchangeType(pListRetrievalData))
                            ControlsTools.DDLLoad_QuoteExchangeType(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLAmountType(pListRetrievalData))
                            ControlsTools.DDLLoad_AmountType(pSource, ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsConfirmationRecipientType(pListRetrievalData))
                            ControlsTools.DDLLoad_ConfirmationRecipientType(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsConfAccessKeyEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_ConfAccessKeyEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLTypeEurosysWebCatPropriet(pListRetrievalData))
                            ControlsTools.DDLLoad_EurosysWebCatPropriet(pSource, ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLAgregateFunction(pListRetrievalData))
                            ControlsTools.DDLLoad_AgregateFunction(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLFlowTypeEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_FlowTypeEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsDDLFungibilityMode(pListRetrievalData))
                            ControlsTools.DDLLoad_ByEnumType(ddl, typeof(FungibilityModeEnum), false, pIsWithEmpty);
                        else if (Cst.IsDDLPreSettlementMethod(pListRetrievalData))
                            ControlsTools.DDLLoad_PreSettlementMethod(pSource, ddl, pIsWithEmpty);
                        else if (Cst.IsFee(pListRetrievalData))
                            ControlsTools.DLLLoad_Fee(pSource, ddl, isAddALL, isClear, string.Empty);
                        else if (Cst.IsFeeSchedule(pListRetrievalData))
                            ControlsTools.DLLLoad_FeeSchedule(pSource, ddl, isAddALL, isClear, string.Empty);
                        else if (Cst.IsFeeMatrix(pListRetrievalData))
                            ControlsTools.DLLLoad_FeeMatrix(pSource, ddl, isAddALL, isClear, string.Empty);
                        else if (Cst.IsFeesCalcultationMode(pListRetrievalData))
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.FeesCalculationMode>(ddl, pIsWithEmpty, pIsResource, string.Empty);
                        else if (Cst.IsFeeFormulaEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_ConfAccessKeyEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsFeeScopeEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_FeeScopeEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsFeeExchangeTypeEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_FeeExchangeTypeEnum(ddl, pIsWithEmpty);
                        else if (Cst.IsTypePartyEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_TypePartyEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsTypeSidePartyEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_TypeSidePartyEnum(ddl, pIsWithEmpty);
                        //
                        //CC 20120621 TRIM 17921
                        else if (Cst.IsTypeInformationMessage(pListRetrievalData))
                            ControlsTools.DDLLoad_TypeInformationMessage(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDDLTypeHelpDisplay(pListRetrievalData))
                            ControlsTools.DDLLoad_TypeHelpDisplay(ddl, pIsWithEmpty);
                        //
                        //CC 20140617 [19923]
                        else if (Cst.IsDDLRequestTrackMode(pListRetrievalData))
                            ControlsTools.DDLLoad_RequestTrackMode(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsTypeGroupTrackerEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_GroupTrackerEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsTypeReadyStateEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_ReadyStateEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsTypeStatusTrackerEnum(pListRetrievalData))
                            ControlsTools.DDLLoadEnum<ProcessStateTools.StatusEnum>(ddl, pIsWithEmpty, pIsResource, "TRACKERSTATUS_");
                        //
                        else if (Cst.IsTypeMarketEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_TypeMarketEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsTradeSideEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_TradeSideEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsTaxApplicationEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_TaxApplicationEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsPaymentRuleEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_PaymentRuleEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsInvoiceApplicationPeriodEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_InvoiceApplicationPeriodEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsBracketApplicationEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_BracketApplicationEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsInvoicingSortEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_InvoicingSortEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsInvoicingTradeDetailEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_InvoicingTradeDetailEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsMaturityMonthYearFmtEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_MaturityMonthYearFmtEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsMaturityMonthYearIncrUnitEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_MaturityMonthYearIncrUnitEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsCfiCodeCategoryEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_CfiCodeCategoryEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsDerivativeExerciseStyleEnum(pListRetrievalData))
                            ControlsTools.DDLLoad_DerivativeExerciseStyleEnum(ddl, pIsWithEmpty);
                        //
                        else if (Cst.IsMatchStatusEnum(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_MathStatusEnum(ddl, pIsWithEmpty);
                            DropDownTools.DDLAddMiscItem(pddl, pMisc);
                        }
                        // EG 20141020 [20442]
                        else if (Cst.IsDDLActorEntity(pListRetrievalData)
                       || Cst.IsDDLActorSettltOffice(pListRetrievalData)
                       || Cst.IsDDLActor(pListRetrievalData)
                       || Cst.IsDDLActorMarginRequirementOffice(pListRetrievalData)
                       || Cst.IsDDLActorInvoiceBeneficiary(pListRetrievalData)
                        || Cst.IsOptDDLActorInvoiceBeneficiary(pListRetrievalData)
                            )
                        {
                            if (Cst.IsDDLActorEntity(pListRetrievalData))
                                ControlsTools.DDLLoad_ActorEntity(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                            else if (Cst.IsDDLActorSettltOffice(pListRetrievalData))
                                ControlsTools.DDLLoad_ActorSettltOffice(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                            else if (Cst.IsDDLActorMarginRequirementOffice(pListRetrievalData))
                                ControlsTools.DDLLoad_ActorMarginRequirementOffice(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                            else if (Cst.IsDDLActor(pListRetrievalData))
                                ControlsTools.DDLLoad_Actor(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                            else if (Cst.IsDDLActorInvoiceBeneficiary(pListRetrievalData))
                                ControlsTools.DDLLoad_ActorInvoiceBeneficiary(pSource, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                            else if (Cst.IsOptDDLActorInvoiceBeneficiary(pListRetrievalData))
                                ControlsTools.DDLLoad_ActorInvoiceBeneficiary(pSource, pPage, ddl, pIsWithEmpty, isAddALL, isUseSessionRestrict, isClear);
                        }

                        else if (Cst.IsDDLPositionType(pListRetrievalData))
                            ControlsTools.DDLLoad_PositionType(ddl, pIsWithEmpty);

                        // EG 20150722 PositionDateType
                        else if (Cst.IsDDLPositionDateType(pListRetrievalData))
                            ControlsTools.DDLLoad_PositionDateType(ddl);

                        else if (Cst.IsDDLPositionOtcSideView(pListRetrievalData))
                            ControlsTools.DDLLoad_PositionOtcSideView(ddl);

                        else if (Cst.IsDDLPositionSideView(pListRetrievalData))
                            ControlsTools.DDLLoad_PositionSideView(ddl);

                        else if (Cst.IsDDLActorSideView(pListRetrievalData))
                            ControlsTools.DDLLoad_ActorSideView(ddl);

                        else if (Cst.IsDDLAggregateDateTypeView(pListRetrievalData))
                            ControlsTools.DDLLoad_AggregateDateTypeView(ddl);

                        // CC 20170127
                        else if (Cst.IsDDLFinancialProductTypeView(pListRetrievalData))
                            ControlsTools.DDLLoad_FinancialProductTypeView(ddl);

                        else if (Cst.IsDDLActivityTypeView(pListRetrievalData))
                            ControlsTools.DDLLoad_ActivityTypeView(ddl);

                        else if (Cst.IsDDLResultsType(pListRetrievalData))
                            ControlsTools.DDLLoad_ResultsType(ddl);

                        else if (Cst.IsDDLPeriodReportType(pListRetrievalData))
                            ControlsTools.DDLLoad_PeriodReportType(ddl);

                        else if (Cst.IsDDLHolidayMethodOfAdjustment(pListRetrievalData))
                            ControlsTools.DDLLoad_HolidayMethodOfAdjustment(ddl);

                        else if (Cst.IsDDLTypeGProduct_Trading(pListRetrievalData))
                            ControlsTools.DDLLoad_ProductGProduct_Trading(ddl, pIsWithEmpty);

                        else if (Cst.IsDDLHeaderType(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX] 
                            //ControlsTools.DDLLoad_HeaderType(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.HeaderType>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLFooterType(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX]
                            //ControlsTools.DDLLoad_FooterType(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.FooterType>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLHeaderTitlePosition(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX]
                            //ControlsTools.DDLLoad_HeaderTitlePosition(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.HeaderTitlePosition>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLFooterLegend(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX]
                            //ControlsTools.DDLLoad_FooterLegend(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.FooterLegend>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLHeaderFooterSort(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX]
                            //ControlsTools.DDLLoad_HeaderFooterSort(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.HeaderFooterSort>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLHeaderFooterSummary(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX]
                            //ControlsTools.DDLLoad_HeaderFooterSummary(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.HeaderFooterSummary>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLAmountFormat(pListRetrievalData))
                        {
                            // FI 20171122 [XXXXX]
                            //ControlsTools.DDLLoad_AmountFormat(ddl);
                            // FI 20190725 passage du paramètre pIsResource
                            ControlsTools.DDLLoadEnum<Cst.AmountFormat>(ddl, false, pIsResource, string.Empty);
                        }

                        else if (Cst.IsDDLBannerPosition(pListRetrievalData))
                            ControlsTools.DDLLoad_BannerPosition(ddl);

                        else if (Cst.IsDDLAssetBannerStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_AssetBannerStyle(ddl);

                        else if (Cst.IsDDLSectionBannerStyle(pListRetrievalData))
                            ControlsTools.DDLLoad_SectionBannerStyle(ddl);

                        else if (Cst.IsDDLTimestampType(pListRetrievalData))
                            ControlsTools.DDLLoad_TimestampType(ddl);

                        else if (Cst.IsDDLUTISummary(pListRetrievalData))
                            ControlsTools.DDLLoad_UTISummary(ddl);

                        else if (Cst.IsDDLStrategyTypeScheme_Extended(pListRetrievalData))
                            ControlsTools.DDLLoad_StrategyTypeScheme_Extended(pSource, ddl, pIsWithEmpty);

                        else if (pListRetrievalData == "efsoperators")
                            ControlsTools.DDLLoad_EFSOperators(ddl);

                        // EG 20151102 [21465] New
                        else if (Cst.IsDDLDenOptionActionType(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_DenOptionActionType(ddl);
                        }
                        else if (Cst.IsDDLOptionITM_ATM_OTM(pListRetrievalData))
                        {
                            ControlsTools.DDLLoad_DDLOptionITM_ATM_OTM(ddl, pIsWithEmpty);
                        }
                        else if (Cst.IsDDLCnfTypeMessage(pListRetrievalData))
                        {
                            DDLLoad_CnfTypeMessage(pSource, ddl, isClear, pIsWithEmpty, isAddALL, pIsResource);
                        }
                        #endregion Specific Load
                    }
                    #endregion
                    break;

                case Cst.ListRetrievalEnum.XML:
                    //TODO
                    break;
            }

            // RD 20130222 / Pour affiche les valeurs numériques au format numérique
            if (TypeData.IsTypeDec(dataType))
                ((DropDownList)pddl).DataTextFormatString = "{0:N}";

        }

        #endregion DDLLoad_FromListRetrieval

        #region DDLLoadList
        public static void DDLLoadList(DropDownList pddl, ListItemCollection pCol, bool pWithEmpty)
        {
            pddl.Items.Clear();
            //
            for (int i = 0; i < ArrFunc.Count(pCol); i++)
                pddl.Items.Add(pCol[i]);
            //
            if (pWithEmpty)
                pddl.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion

        #region DDLLoad
        #region DDLLoad surcharges
        /// <summary>
        /// Load a DropDownList with SQL query
        /// </summary>
        /// <param name="pddl">DropDownList</param>
        /// <param name="pSource">ConnectionString</param>
        /// <param name="pSQLQuery">SQL Query</param>
        /// <param name="pColText">SQL Column for Text</param>
        /// <param name="pColValue">SQL Column for Value</param>
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            string pSource, string pSQLQuery, bool pWithEmpty, params IDbDataParameter[] commandParameters)
        {
            Hashtable hs = new Hashtable();
            bool withSort = true;
            bool withSQLResource = false;
            bool clear = true;
            string addMisc = null;
            int indexSelected = 0;
            DDLLoad(pddl, pColText, pColValue, clear, indexSelected, hs, pSource, pSQLQuery, pWithEmpty, addMisc,
                withSort, withSQLResource, null, commandParameters);
        }
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            bool pClear, string pSource, string pSQLQuery, bool pWithEmpty, string pMisc,
            bool pWithRessource, params IDbDataParameter[] commandParameters)
        {
            Hashtable hs = new Hashtable();
            bool withSort = true;
            int indexSelected = 0;

            DDLLoad(pddl, pColText, pColValue, pClear, indexSelected, hs, pSource, pSQLQuery, pWithEmpty,
                pMisc, withSort, pWithRessource, null, commandParameters);
        }
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            string pSource, string pSQLQuery, bool pWithEmpty, bool pWithSort, params IDbDataParameter[] commandParameters)
        {
            Hashtable hs = new Hashtable();
            bool withSQLResource = false;
            bool clear = true;
            string addMisc = null;
            int indexSelected = 0;
            DDLLoad(pddl, pColText, pColValue, clear, indexSelected, hs, pSource, pSQLQuery, pWithEmpty, addMisc,
                pWithSort, withSQLResource, null, commandParameters);
        }
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            string pSource, string pSQLQuery, bool pWithEmpty, string pAddMisc, bool pWithSort, params IDbDataParameter[] commandParameters)
        {
            Hashtable hs = new Hashtable();
            bool withSQLResource = false;
            bool clear = true;
            int indexSelected = 0;
            DDLLoad(pddl, pColText, pColValue, clear, indexSelected, hs, pSource, pSQLQuery, pWithEmpty, pAddMisc,
                pWithSort, withSQLResource, null, commandParameters);
        }
        public static void DDLLoad(PageBase pPage, Control pddl, string pColText, string pColValue,
            string pSource, string pSQLQuery, bool pWithEmpty, bool pWithSort, bool pWithResource, string pResourcePrefix, params IDbDataParameter[] commandParameters)
        {
            Hashtable hs = new Hashtable();
            bool clear = true;
            string addMisc = null;
            int indexSelected = 0;
            DDLLoad(pPage, pddl, pColText, pColValue, clear, indexSelected, hs, pSource, pSQLQuery, pWithEmpty, addMisc,
                pWithSort, pWithResource, pResourcePrefix, commandParameters);
        }
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            bool pClear, int pIndexSelected, Hashtable phtbData,
            string pSource, string pSQLQuery, bool pWithEmpty, params IDbDataParameter[] commandParameters)
        {
            bool withSort = true;
            string addMisc = null;
            bool withSQLResource = false;
            DDLLoad(pddl, pColText, pColValue, pClear, pIndexSelected, phtbData, pSource, pSQLQuery, pWithEmpty, addMisc,
                withSort, withSQLResource, null, commandParameters);
        }
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            bool pClear, int pIndexSelected, Hashtable phtbData,
            string pSource, string pSQLQuery, bool pWithEmpty, bool pWithSort, params IDbDataParameter[] commandParameters)
        {
            DDLLoad(null, pddl, pColText, pColValue,
            pClear, pIndexSelected, phtbData,
            pSource, pSQLQuery, pWithEmpty, pWithSort, commandParameters);
        }
        public static void DDLLoad(PageBase pPage, Control pddl, string pColText, string pColValue,
            bool pClear, int pIndexSelected, Hashtable phtbData,
            string pSource, string pSQLQuery, bool pWithEmpty, bool pWithSort, params IDbDataParameter[] commandParameters)
        {
            bool withSQLResource = false;
            string addMisc = null;
            DDLLoad(pPage, pddl, pColText, pColValue, pClear, pIndexSelected, phtbData, pSource, pSQLQuery, pWithEmpty, addMisc,
                pWithSort, withSQLResource, null, commandParameters);
        }
        public static void DDLLoad(Control pddl, string pColText, string pColValue,
            bool pClear, int pIndexSelected, Hashtable phtbData,
            string pSource, string pSQLQuery, bool pWithEmpty, string pMisc,
            bool pWithSort, bool pWithRessource, string pResourcePrefix,
            params IDbDataParameter[] commandParameters)
        {
            DDLLoad(null, pddl, pColText, pColValue,
            pClear, pIndexSelected, phtbData,
            pSource, pSQLQuery, pWithEmpty, pMisc,
            pWithSort, pWithRessource, pResourcePrefix,
            commandParameters);
        }
        #endregion DDLLoad surcharges
        /// <summary>
        /// Load a DropDownList with SQL query and/or HashTable
        /// </summary>
        /// <param name="pddl">DropDownList</param>
        /// <param name="pSource">ConnectionString</param>
        /// <param name="pSQLQuery">SQL Query</param>
        /// <param name="pColText">SQL Column for Text</param>
        /// <param name="pColValue">SQL Column for Value</param>
        /// <param name="pClear">Flag for clear</param>
        /// <param name="pIndexSelected">Index at selected</param>
        /// <param name="phtbData">HashTable</param>
        public static void DDLLoad(PageBase pPage, Control pddl, string pColText, string pColValue,
            bool pClear, int pIndexSelected, Hashtable phtbData,
            string pSource, string pSQLQuery, bool pWithEmpty, string pMisc,
            bool pWithSort, bool pWithRessource, string pResourcePrefix,
            params IDbDataParameter[] commandParameters)
        {
            //PL 20110817 Add SOURCE (suite à un pb de ressource sur la DDL StatusBusiness)
            bool isForeColor = false, isBackColor = false, isOptGroup = false, isSource = false;
            List<Pair<string, string>> optGroup = null;

            DropDownTools.GetDDL(pddl, out DropDownList dropDownList, out HtmlSelect htmlSelect, out CheckBoxList checkBoxList);

            bool isDropDownList = (dropDownList != null);
            bool isHtmlSelect = (htmlSelect != null);
            bool isCheckBoxList = (checkBoxList != null);
            //
            #region Clear
            if (pClear)
            {
                if (isDropDownList)
                    dropDownList.Items.Clear();
                else if (isHtmlSelect)
                    htmlSelect.Items.Clear();
                else if (isHtmlSelect)
                    checkBoxList.Items.Clear();
            }
            #endregion Clear
            //
            #region Bind
            bool bIsBind = (pClear && isDropDownList);
            //20080929 PL Pour disposer des couleurs...
            isForeColor = (pSQLQuery.IndexOf("FORECOLOR") > 0);
            isBackColor = (pSQLQuery.IndexOf("BACKCOLOR") > 0);
            isOptGroup = (pSQLQuery.IndexOf("OPTGROUP") > 0);
            isSource = (pSQLQuery.IndexOf("SOURCE") > 0);
            string colSort = (isOptGroup ? "OPTGROUPSORT," : string.Empty) + pColText;
            if (isDropDownList && (isForeColor || isBackColor || isOptGroup))
                bIsBind = false;
            if (isOptGroup)
                optGroup = new List<Pair<string, string>>();

            if (ArrFunc.Count(phtbData) > 0)
            {
                foreach (DictionaryEntry o in phtbData)
                {
                    if (isDropDownList)
                        dropDownList.Items.Add(new ListItem(Convert.ToString(o.Value), Convert.ToString(o.Key)));
                    if (isHtmlSelect)
                        htmlSelect.Items.Add(new ListItem(Convert.ToString(o.Value), Convert.ToString(o.Key)));
                    if (isCheckBoxList)
                        checkBoxList.Items.Add(new ListItem(Convert.ToString(o.Value), Convert.ToString(o.Key)));
                }
            }
            //
            if (!bIsBind && (pSQLQuery.IndexOf(SQLCst.ORDERBY) < 0))
            {
                //20110231 PL Add Test: if (pWithSort)
                if (pWithSort)
                    pSQLQuery += Cst.CrLf + SQLCst.ORDERBY + colSort;
            }
            //
            DataSet ds;
            if ((commandParameters != null) && (commandParameters.Length == 1) && (commandParameters[0] == null))
                ds = DataHelper.ExecuteDataset(pSource, CommandType.Text, pSQLQuery);
            else
                ds = DataHelper.ExecuteDataset(pSource, CommandType.Text, pSQLQuery, commandParameters);
            // Dans le cas d'un binding, on ne peut pas insérer d'item en début de liste (ex: Empty), on peut uniquement si besoin rajouter des items en fin de liste
            //if (bIsBind && !pWithRessource)
            if (bIsBind && !pWithRessource && !pWithEmpty)
            {
                DataView dv = ds.Tables[0].DefaultView;
                if (pWithSort)
                    dv.Sort = colSort;

                dropDownList.DataSource = dv;
                dropDownList.DataTextField = pColText;
                dropDownList.DataValueField = pColValue;
                dropDownList.DataBind();
            }
            else
            {
                DataRow[] rowData;
                //PL 20101214 Newness
                if (pWithRessource)
                {
                    //PL 20140128
                    pResourcePrefix = StrFunc.IsEmpty(pResourcePrefix) ? string.Empty : pResourcePrefix + "_";
                    //PL 20110711 Add isRessourceSetted
                    string dataTmp = null;
                    bool isRessourceSetted = false;
                    rowData = ds.Tables[0].Select(string.Empty);
                    foreach (DataRow dr in rowData)
                    {
                        //PL 20110817 Use SOURCE pour essayer de gérer au "mieux" les ressources...
                        //dataTmp = dr[pColText].ToString();
                        //dr[pColText] = Ressource.GetString(dataTmp, true);
                        //if (!isRessourceSetted)
                        //    isRessourceSetted = (dr[pColText] != dataTmp);

                        if (isSource && dr["SOURCE"].ToString().ToUpper().StartsWith("FIX"))
                        {
                            //NB: Peut-être faudra-t-il ne le faire que si la donnée isuue de pColValue est numérique.

                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            //PL 20180214 Use GetMultiByRef instead of GetStringByRef (in anticipation, see below)
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            //if (Ressource.GetStringByRef(pResourcePrefix + dr[pColText].ToString(), ref dataTmp))
                            if (Ressource.GetMultiByRef(pResourcePrefix + dr[pColText].ToString(), 0, ref dataTmp))
                            {
                                dr[pColText] = dataTmp;
                                isRessourceSetted = true;
                            }
                        }
                        else
                        {
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            //PL 20180214 Use GetMultiByRef instead of GetStringByRef (for FORMULA on FEESCHEDULE)
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            //if (Ressource.GetStringByRef(pResourcePrefix + dr[pColValue].ToString(), ref dataTmp))
                            if (Ressource.GetMultiByRef(pResourcePrefix + dr[pColValue].ToString(), 0, ref dataTmp))
                            {
                                dr[pColText] = dataTmp;
                                isRessourceSetted = true;
                            }
                            //else if (Ressource.GetStringByRef(pResourcePrefix + dr[pColText].ToString(), ref dataTmp))
                            else if (Ressource.GetMultiByRef(pResourcePrefix + dr[pColText].ToString(), 0, ref dataTmp))
                            {
                                dr[pColText] = dataTmp;
                                isRessourceSetted = true;
                            }
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        }
                    }

                    if (!isRessourceSetted)
                    {
                        //Aucune ressource n'a été trouvée à l'étape précédente (donc théoriquement via TEXT), 
                        //on tente alors une mise à jour des ressources via VALUE (Anciennement opéré par la méthode DDLSetRessourceFromValue())
                        foreach (DataRow dr in rowData)
                        {
                            dataTmp = dr[pColValue].ToString();
                            dr[pColText] = Ressource.GetString(pResourcePrefix + dataTmp, false);
                        }
                    }
                }
                if (pWithSort)
                    rowData = ds.Tables[0].Select(string.Empty, colSort);
                else
                    rowData = ds.Tables[0].Select(string.Empty);

                foreach (DataRow dr in rowData)
                {
                    string dataColText = dr[pColText].ToString();
                    string dataColValue = dr[pColValue].ToString();
                    if (isDropDownList)
                    {
                        ListItem li = new ListItem(dataColText, dataColValue);

                        if (isForeColor || isBackColor || isOptGroup)
                        {
                            try
                            {
                                if (isForeColor || isBackColor || isOptGroup)
                                {
                                    string foreColor = Color.Empty.ToString();
                                    //string backColor = isOptGroup ? backColor = "White" : Color.Empty.ToString();
                                    string backColor = Color.Empty.ToString();

                                    if (isForeColor && (dr["FORECOLOR"] != Convert.DBNull))
                                        foreColor = dr["FORECOLOR"].ToString();
                                    if ((!isOptGroup) && (isBackColor && (dr["BACKCOLOR"] != Convert.DBNull)))
                                        backColor = dr["BACKCOLOR"].ToString();

                                    if ((foreColor != Color.Empty.ToString()) || (backColor != Color.Empty.ToString()))
                                        li.Attributes.Add("style", "color:" + foreColor + ";background-color:" + backColor);

                                    //PL 20140724 TEST OPTGROUP
                                    if (isOptGroup && dr["OPTGROUP"] != Convert.DBNull)
                                    {
                                        string optGroupValue = dr["OPTGROUP"].ToString().Trim();
                                        li.Attributes.Add("optiongroup", optGroupValue);

                                        if (!optGroup.Exists(key => key.First == optGroupValue))
                                        {
                                            //PL 20141017
                                            //string optGroupText = (dr["OPTGROUPTEXT"] != Convert.DBNull) ? dr["OPTGROUPTEXT"].ToString().Trim() : Ressource.GetString(optGroupValue, true);
                                            string optGroupText = (dr["OPTGROUPTEXT"] != Convert.DBNull) ? dr["OPTGROUPTEXT"].ToString().Trim() : optGroupValue;
                                            optGroup.Add(new Pair<string, string>(optGroupValue, Ressource.GetString(optGroupText, true)));
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                string msg = ex.Message;
                            }
                        }

                        dropDownList.Items.Add(li);
                    }

                    if (isHtmlSelect)
                    {
                        htmlSelect.Items.Add(new ListItem(dataColText, dataColValue));
                        try
                        {
                            int item = htmlSelect.Items.Count - 1;
                            htmlSelect.Items[item].Attributes.Add("style", "color:" + dr["FORECOLOR"] + ";background-color:" + dr["BACKCOLOR"]);
                        }
                        catch { }
                    }
                    if (isCheckBoxList)
                    {
                        checkBoxList.Items.Add(new ListItem(dataColText, dataColValue));
                        try
                        {
                            int item = checkBoxList.Items.Count - 1;
                            //20090625 PL
                            //checkBoxList.Items[item].Attributes.Add("style", "color:" + dr["FORECOLOR"] + ";background-color:" + dr["BACKCOLOR"]);
                            checkBoxList.Items[item].Attributes.Add("style", "white-space:nowrap;color:" + dr["FORECOLOR"] + ";background-color:" + dr["BACKCOLOR"]);
                        }
                        catch { }
                    }
                }
            }
            #endregion Bind
            //
            #region Insert Misc (All, Inherited, NA, None)
            if (StrFunc.IsFilled(pMisc))
            {
                DropDownTools.DDLAddMiscItem(pddl, pMisc);
            }
            #endregion Insert Misc (All, Inherited, NA, None)
            //
            #region Insert Empty
            //20050411 PL Inversement de l'ordre des 2 "if()" suivants:
            if (pWithEmpty)
            {
                if (isDropDownList)
                    dropDownList.Items.Insert(0, new ListItem(string.Empty, string.Empty));
                if (isHtmlSelect)
                    htmlSelect.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            }
            #endregion Insert Empty
            //
            #region SelectedIndex
            int count = 0;
            if (isDropDownList)
                count = dropDownList.Items.Count;
            if (isHtmlSelect)
                count = htmlSelect.Items.Count;
            if ((pIndexSelected >= 0) && (pIndexSelected <= count) && count > 0)
            {
                if (isDropDownList)
                    dropDownList.SelectedIndex = pIndexSelected;
                if (isHtmlSelect)
                    htmlSelect.SelectedIndex = pIndexSelected;
            }
            #endregion SelectedIndex
            //
            #region JS for OptGroup
            if (isDropDownList && isOptGroup && (optGroup.Count > 0) && (pPage != null))
            {
                dropDownList.CssClass = EFSCssClass.DropDownListJQOptionGroup;
                JQuery.WriteSelectOptGroupScripts(pPage, dropDownList.ClientID, optGroup, false);
            }
            #endregion JS for OptGroup
        }
        #endregion DDLLoad


        #region DDLLoad_ByTypeEnum
        public static void DDLLoad_ByEnumType(DropDownList pDDL, Type pType, bool pIsWithRessource, bool pIsWithEmpty)
        {
            foreach (string s in Enum.GetNames(pType))
            {
                pDDL.Items.Add(new ListItem((pIsWithRessource ? Ressource.GetString(s) : s), s));
            }
            if (pIsWithEmpty)
                pDDL.Items.Insert(0, new ListItem(string.Empty, string.Empty));
        }
        #endregion DDLLoad_ByTypeEnum
        #endregion Load DropDownList

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDDL"></param>
        private static void DDLSetRessourceFromValue(DropDownList pDDL)
        {
            foreach (ListItem li in pDDL.Items)
            {
                if (li.Value.Length > 0)
                    li.Text = Ressource.GetString(li.Value, false);
            }
        }

        /// <summary>
        /// Select a Value in DropDownList
        /// </summary>
        /// <param name="pddl">DropDownList</param>
        /// <param name="pValue">Value</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByValue(DropDownList pddl, string pValue)
        {
            ListItem listItem = pddl.Items.FindByValue(pValue);
            return DDLSelectByItem(pddl, listItem);
        }
        /// <summary>
        /// Select a Text in DropDownList
        /// </summary>
        /// <param name="pddl">DropDownList</param>
        /// <param name="pText">Text</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByText(DropDownList pddl, string pText)
        {
            ListItem listItem = pddl.Items.FindByText(pText);
            return DDLSelectByItem(pddl, listItem);
        }
        /// <summary>
        /// Select a Item in DropDownList
        /// </summary>
        /// <param name="pddl">DropDownList</param>
        /// <param name="pListItem">ListItem</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByItem(DropDownList pddl, ListItem pListItem)
        {
            int index = pddl.Items.IndexOf(pListItem);
            return DDLSelectByIndex(pddl, index);
        }
        /// <summary>
        /// Select a Index in DropDownList
        /// </summary>
        /// <param name="pddl">DropDownList</param>
        /// <param name="pIndex">Index</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByIndex(DropDownList pddl, int pIndex)
        {
            bool bRet;
            if ((pIndex < 0) || (pddl.Items.Count <= pIndex))
            {
                bRet = false;
            }
            else
            {
                bRet = true;
                pddl.SelectedIndex = pIndex;
                //PL 20100323 Add Reset Tooltip because OnSelectedIndexChanged is not event
                pddl.ToolTip = string.Empty;
            }
            return bRet;
        }



        /// <summary>
        /// Select a Value in HtmlSelect
        /// </summary>
        /// <param name="pddl">HtmlSelect</param>
        /// <param name="pValue">Value</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByValue(HtmlSelect pddl, string pValue)
        {
            ListItem listItem = pddl.Items.FindByValue(pValue);
            return DDLSelectByItem(pddl, listItem);
        }
        /// <summary>
        /// Select a Text in HtmlSelect
        /// </summary>
        /// <param name="pddl">HtmlSelect</param>
        /// <param name="pText">Text</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByText(HtmlSelect pddl, string pText)
        {
            ListItem listItem = pddl.Items.FindByText(pText);
            return DDLSelectByItem(pddl, listItem);
        }
        /// <summary>
        /// Select a Item in HtmlSelect
        /// </summary>
        /// <param name="pddl">HtmlSelect</param>
        /// <param name="pListItem">ListItem</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByItem(HtmlSelect pddl, ListItem pListItem)
        {
            int index = pddl.Items.IndexOf(pListItem);
            return DDLSelectByIndex(pddl, index);
        }
        /// <summary>
        /// Select a Index in HtmlSelect
        /// </summary>
        /// <param name="pddl">HtmlSelect</param>
        /// <param name="pIndex">Index</param>
        /// <returns>true if ok, else false</returns>
        public static bool DDLSelectByIndex(HtmlSelect pddl, int pIndex)
        {
            bool bRet;
            if ((pIndex < 0) || (pddl.Items.Count <= pIndex))
                bRet = false;
            else
            {
                bRet = true;
                pddl.SelectedIndex = pIndex;
            }
            return bRet;
        }
        #endregion DropDownList

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        public static Control EFS_FindControl(PageBase pPage, string pName)
        {
            foreach (Control c in pPage.Controls)
            {
                if (c.ID == pName)
                    return c;
                else
                {
                    Control ctrlFind = EFS_FindControl(c, pName);
                    if (null != ctrlFind)
                        return ctrlFind;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        public static Control EFS_FindControl(Control pControl, string pName)
        {
            foreach (Control c in pControl.Controls)
            {
                //System.Diagnostics.Debug.Write(c.ClientID + "(");
                //System.Diagnostics.Debug.Write(c.GetType().Name + ")" + Environment.NewLine);
                if (c.ID == pName)
                    return c;
                else if (null != c.Controls)
                {
                    Control ctrlFind = EFS_FindControl(c, pName);
                    if (null != ctrlFind)
                        return ctrlFind;
                }
            }
            return null;
        }

        #region HtmlPage
        #region StaticBlockHtmlPage
        public static TableRow GetStaticBlockHtmlPagev2(string pTitle, int pColspan)
        {
            if (pTitle == null || pTitle.Trim().Length == 0)
                pTitle = "Characteristics";
            pTitle = Ressource.GetString(pTitle, true);
            //Block row
            string html = @"<div class=""banniereMargin"">
								<div class=""banniereCaptureBlock""><div class=""banniereButtonBlock""></div>
									<span class=""banniereText"">" + pTitle + @"</span>
								</div>
							</div>";
            TableCell td = new TableCell
            {
                ColumnSpan = pColspan
            };
            td.Controls.Add(new LiteralControl(html));
            TableRow tr = new TableRow();
            tr.Cells.Add(td);
            return tr;
        }
        #endregion StaticBlockHtmlPage

        #region HRHtmlPage
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static TableRow GetHRHtmlPage(string pColor, int pColspan)
        {
            return GetHRHtmlPage(pColor, pColspan, "2", string.Empty, null);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static TableRow GetHRHtmlPage(string pColor, int pColspan, string pSize, string pTitle, string pTitleColor)
        {
            string cssClass = "fixedText";
            string style = string.Empty;;
            string styleHR = string.Empty;

            if (StrFunc.IsFilled(pSize))
                styleHR = String.Format("height:{0}px;",pSize);

            if (StrFunc.IsFilled(pColor))
            {
                style = String.Format("color:{0};", pColor);
                styleHR += String.Format("background-color:{0};", pColor);
            }

            TableCell td;
            TableRow tr = new TableRow
            {
                Height = Unit.Pixel(15)
            };
            if (StrFunc.IsFilled(pTitle))
            {
                pTitle = (pTitle.StartsWith("{NoTranslate}") ? pTitle.Substring(13) : Ressource.GetString(pTitle, true));
                td = new TableCell();
                td.Attributes.Add("class", cssClass);
                if (StrFunc.IsFilled(style))
                    td.Attributes.Add("style", style);

                Label lbl = new Label
                {
                    CssClass = "fixedText",
                    Text = Cst.HTMLSpace2 + pTitle + Cst.HTMLSpace2
                };
                lbl.Font.Bold = StrFunc.IsEmpty(pSize) || (Convert.ToInt32(pSize) > 1);
                try
                {
                    if (StrFunc.IsFilled(pTitleColor))
                        lbl.ForeColor = (pTitleColor.ToLower() == "default")? Color.FromName("dimgray") : Color.FromName(pTitleColor);
                    else if (StrFunc.IsFilled(pColor))
                        lbl.ForeColor = Color.FromName(pColor);
                }
                catch { }

                td.Controls.Add(lbl);
                tr.Cells.Add(td);
                pColspan--;
            }
            td = new TableCell();

            td.Attributes.Add("class", cssClass);
            if (StrFunc.IsFilled(style))
                td.Attributes.Add("style", style);
            td.ColumnSpan = pColspan;
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "hr");
            if (StrFunc.IsFilled(styleHR))
                div.Attributes.Add("style", styleHR);
            LiteralControl hr = new LiteralControl("<hr/>");
            div.Controls.Add(hr);
            td.Controls.Add(div);
            tr.Cells.Add(td);
            return tr;
        }
        #endregion HRHtmlPage

        #region GetBannerPage
        /// <summary>
        /// Création d'une bannière 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pTitleLeft"></param>
        /// <param name="pTitleRight"></param>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        // EG 20190125 Upd
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static Panel GetBannerPage(PageBase pPage, HtmlPageTitle pTitleLeft, HtmlPageTitle pTitleRight, string pIdMenu)
        {
            return GetBannerPage(pPage, pTitleLeft, pTitleRight, pIdMenu, null);
        }
        
        /// <summary>
        /// Création d'une bannière 
        /// <para>Possibilité d'ajouter ou non les boutons print, help</para>
        /// <para>Possibilité de préciser une URL pour l'aide en ligne</para>
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pTitleLeft"></param>
        /// <param name="pTitleRight"></param>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210331 [25556] Gestion Title Right
        public static Panel GetBannerPage(PageBase pPage, HtmlPageTitle pTitleLeft, HtmlPageTitle pTitleRight, string pIdMenu, string pMainMenuName)
        {
            string mainMenuName = pMainMenuName;
            if (StrFunc.IsEmpty(mainMenuName))
                mainMenuName = MainMenuName(pIdMenu);

            //Panel Header
            Panel pnlHeader = new Panel
            {
                CssClass = pPage.CSSModeEnum.ToString() + " " + mainMenuName
            };

            Panel pnlButtons = new Panel();
            pnlButtons.Controls.Add(ControlsTools.GetToolTipLinkButtonExit());
            pnlHeader.Controls.Add(pnlButtons);

            //Panel Title
            Panel pnlTitle = new Panel
            {
                ID = "bannerTitle"
            };
            if (StrFunc.IsFilled(pTitleLeft.Title))
            {
                WCTooltipLabel lblTitle = new WCTooltipLabel
                {
                    ID = "titleLeft",
                    CssClass = "banner-title",
                    Text = pTitleLeft.Title
                };
                if (StrFunc.IsFilled(pTitleLeft.TitleTooltip))
                    lblTitle.Pty.TooltipContent = pTitleLeft.TitleTooltip;
                pnlTitle.Controls.Add(lblTitle);
            }

            Label lblSubTitle = new Label
            {
                ID = "subTitleLeft",
                Text = pTitleLeft.SubTitle,
                CssClass = "banner-subtitle"
            };
            pnlTitle.Controls.Add(lblSubTitle);

            Label lblRightSubtitle = new Label
            {
                ID = "lblRightSubtitle",
                CssClass = "banner-subtitle"
            };
            if (null != pTitleRight)
                lblRightSubtitle.Text = pTitleRight.FullRightTitle;
            pnlTitle.Controls.Add(lblRightSubtitle);

            pnlHeader.Controls.Add(pnlTitle);

            return pnlHeader;
        }
        #endregion GetBannerPage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public static string GetBannerHeaderMainMenu(string pIdMenu)
        {
            string cssMainMenu = string.Empty;
            string[] mainMenu = pIdMenu.Split('_');
            if (ArrFunc.IsFilled(mainMenu) && (1 < mainMenu.Length))
            {
                switch (mainMenu[1])
                {
                    case "ABOUT":
                        cssMainMenu = CstCSS.BannerHeaderAbout;
                        break;
                    case "ADM":
                        cssMainMenu = CstCSS.BannerHeaderAdmin;
                        break;
                    case "EXTL":
                        cssMainMenu = CstCSS.BannerHeaderExternal;
                        break;
                    case "INP":
                        cssMainMenu = CstCSS.BannerHeaderInput;
                        break;
                    case "INV":
                        cssMainMenu = CstCSS.BannerHeaderInvoicing;
                        break;
                    case "PROCESS":
                        cssMainMenu = CstCSS.BannerHeaderProcess;
                        break;
                    case "REF":
                        cssMainMenu = CstCSS.BannerHeaderRepository;
                        break;
                    case "VIEW":
                        cssMainMenu = CstCSS.BannerHeaderViews;
                        break;
                    default:
                        cssMainMenu = CstCSS.BannerHeaderUnknown;
                        break;
                }
            }
            return cssMainMenu;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Gestion des menus en mode PORTAIL
        public static string MainMenuName(string pIdMenu)
        {
            string mainMenuName = "repository";
            if (StrFunc.IsFilled(pIdMenu))
            {
                string[] mainMenu = pIdMenu.Split('_');
                if (ArrFunc.IsFilled(mainMenu) && (1 < mainMenu.Length))
                {
                    switch (mainMenu[1])
                    {
                        case "ABOUT":
                            mainMenuName = "about";
                            break;
                        case "ADM":
                            mainMenuName = "admin";
                            break;
                        case "EXTL":
                            mainMenuName = "external";
                            break;
                        case "INP":
                            mainMenuName = "input";
                            break;
                        case "INV":
                            mainMenuName = "invoicing";
                            break;
                        case "PROCESS":
                            mainMenuName = "process";
                            break;
                        case "REF":
                            mainMenuName = "repository";
                            break;
                        case "VIEW":
                            mainMenuName = "views";
                            break;
                        default:
                            if (Software.IsSoftwarePortal())
                            {
                                switch (mainMenu[1])
                                {
                                    case "SUPPORT":
                                        mainMenuName = "about";
                                        break;
                                    case "INTERNE":
                                        mainMenuName = "input";
                                        break;
                                    case "SERVICES":
                                        mainMenuName = "process";
                                        break;
                                    case "CLIENT":
                                        mainMenuName = "views";
                                        break;
                                    default:
                                        mainMenuName = "unknown";
                                        break;
                                }
                            }
                            else
                                mainMenuName = "unknown";
                            break;
                    }
                }
            }
            return mainMenuName;
        }

        #region FooterHtmlPage
        public static Table GetFooterHtmlPage() { return GetFooterHtmlPage(new HtmlPageTitle()); }
        public static Table GetFooterHtmlPage(HtmlPageTitle pTitleRight) { return GetFooterHtmlPage(new HtmlPageTitle(), pTitleRight); }
        public static Table GetFooterHtmlPage(HtmlPageTitle pTitleLeft, HtmlPageTitle pTitleRight)
        {
            return GetFooterHtmlPage(pTitleLeft, pTitleRight, "tblFooter");
        }
        public static Table GetFooterHtmlPage(HtmlPageTitle pTitleLeft, HtmlPageTitle pTitleRight, string pIdForTable)
        {
            int border = int.Parse(SystemSettings.GetAppSettings("Spheres_DebugDesign"));
            Table table = new WCTable
            {
                BorderColor = Color.Blue,
                BorderWidth = border,
                CellPadding = 1,
                CellSpacing = 1,
                Width = Unit.Percentage(100)
            };
            if (StrFunc.IsFilled(pIdForTable))
                table.ID = pIdForTable;
            TableRow tr = new TableRow();

            TableCell td = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Text = pTitleLeft.Footer,
                ForeColor = (Color.Transparent == pTitleLeft.Color ? Color.Red : pTitleLeft.Color)
            };
            td.Font.Size = FontUnit.XXSmall;
            if (StrFunc.IsFilled(pTitleLeft.Id))
                td.ID = pTitleLeft.Id;
            tr.Cells.Add(td);

            td = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Right,
                Text = (StrFunc.IsEmpty(pTitleRight.Footer) ? Software.CopyrightFull : (pTitleRight.Footer == "Empty" ? string.Empty : pTitleRight.Footer))
            };
            td.Font.Size = FontUnit.XXSmall;
            td.ForeColor = (Color.Transparent == pTitleRight.Color ? Color.Black : pTitleRight.Color);
            if (StrFunc.IsFilled(pTitleRight.Id))
                td.ID = pTitleRight.Id;
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            return table;
        }
        #endregion FooterHtmlPage

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public static string GetIconPageMenu(string pIdMenu)
        {
            string suffix = "Blue";
            if (StrFunc.IsFilled(pIdMenu))
            {
                string[] mainMenu = pIdMenu.Split('_');
                if (ArrFunc.IsFilled(mainMenu) && (1 < mainMenu.Length))
                {
                    switch (mainMenu[1])
                    {
                        case "ABOUT":
                            suffix = "Blue";
                            break;
                        case "ADM":
                            suffix = "Red";
                            break;
                        case "EXTL":
                            suffix = "Gray";
                            break;
                        case "INP":
                            suffix = "Orange";
                            break;
                        case "INV":
                            suffix = "Rose";
                            break;
                        case "PROCESS":
                            suffix = "Violet";
                            break;
                        case "REF":
                            suffix = "Marron";
                            break;
                        case "VIEW":
                            suffix = "Green";
                            break;
                        default:
                            suffix = "Blue";
                            break;
                    }
                }
            }
            return "Spheres-" + suffix;
        }

        #endregion HtmlPage

        // EG 20190611 New (Bouton Font Awesome)
        // EG 20190723 Upd (Nouvelle image)
        public static WCToolTipLinkButton GetToolTipLinkButtonRefresh()
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = "btnRefresh",
                CssClass = "fa-icon",
                Text = " <i class='fas fa-sync-alt'></i>",
                Enabled = true
            };
            return btn;
        }
        // EG 20190611 New (Bouton Font Awesome)
        public static WCToolTipLinkButton GetToolTipLinkButtonInfo(string pID)
        {
            return GetToolTipLinkButtonInfo(pID, "orange");
        }
        // EG 20190611 New (Bouton Font Awesome)
        public static WCToolTipLinkButton GetToolTipLinkButtonInfo(string pID, string pColor)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = pID,
                CssClass = "fa-icon",
                Text = String.Format(" <i class='fas fa-info-circle {0}'></i>", pColor),
            };
            btn.Pty.TooltipContent = Ressource.GetString(btn.ID);
            return btn;
        }
        // EG 20190611 New (Bouton Font Awesome)
        public static WCToolTipLinkButton GetToolTipLinkButtonPrint(string pTblHeader)
        {
            return GetToolTipLinkButtonPrint(pTblHeader, null);
        }
        // EG 20190611 New (Bouton Font Awesome)
        public static WCToolTipLinkButton GetToolTipLinkButtonPrint(string pTblHeader, string pTblBody)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = "btnPrint",
                CssClass = "fa-icon",
                Text = " <i class='fas fa-print'></i>",
                Enabled = true
            };
            btn.Pty.TooltipContent = Ressource.GetString(btn.ID);
            if (StrFunc.IsFilled(pTblBody))
                btn.Attributes.Add("onclick", "CallPrint('" + pTblHeader + "','" + pTblBody + "');return false;");
            else
                btn.Attributes.Add("onclick", "CallPrint('" + pTblHeader + "',null);return false;");

            return btn;
        }
        // EG 20190611 New (Bouton Font Awesome) Non opérationel (Javascript à revoir)
        public static WCToolTipLinkButton GetToolTipLinkButtonExpandCollapse(bool pIsWithOnClientClick)
        {
            return GetToolTipLinkButtonExpandCollapse("btnExpand", string.Empty, true, "plus", pIsWithOnClientClick);
        }
        // EG 20190611 New (Bouton Font Awesome) Non opérationel (Javascript à revoir)
        public static WCToolTipLinkButton GetToolTipLinkButtonExpandCollapse(string pCssClass, bool pIsWithOnClientClick)
        {
            return GetToolTipLinkButtonExpandCollapse("btnExpand", string.Empty, true, pCssClass, pIsWithOnClientClick);
        }
        // EG 20190611 New (Bouton Font Awesome) Non opérationel (Javascript à revoir)
        public static WCToolTipLinkButton GetToolTipLinkButtonExpandCollapse(string pName, string pSuffix, bool pIsEnabled, string pCssClass, bool pIsWithOnClientClick)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton();
            btn.Pty.TooltipContent = Ressource.GetString(pName);
            btn.ID = pName + pSuffix;
            btn.Enabled = pIsEnabled;
            btn.TabIndex = -1;
            btn.CausesValidation = false;
            btn.CssClass = "fa-icon";
            btn.Text = String.Format(" <i class='fas fa-{0}-square'></i>", pCssClass);
            btn.Pty.TooltipContent = Ressource.GetString(pCssClass == "plus" ? "EXPAND" : "COLLAPSE", true);
            if (pIsWithOnClientClick)
                btn.OnClientClick = "javascript:ExpandCollapseAll(this);return false;";
            return btn;
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetToolTipLinkButtonExit()
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = "imgBtnClose",
                CssClass = "fa-icon",
                Text = "<i class='fas fa-exit'></i>",
                CausesValidation = false,
                OnClientClick = "return ClosePage();",
                PostBackUrl = "#"
            };
            btn.Pty.TooltipContent = Ressource.GetString(btn.ID);
            return btn;
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetToolTipLinkHeaderButtonPrint()
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = "btnPrint",
                CssClass = "fa-icon",
                Text = " <i class='fas fa-print'></i>",
                OnClientClick = "self.print();return false;"
            };
            btn.Pty.TooltipContent = Ressource.GetString(btn.ID);
            return btn;
        }

        #region public GetAwesomeButtonExpandCollapse
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetAwesomeButtonExpandCollapse(bool pIsWithOnClientClick)
        {
            return GetAwesomeButtonExpandCollapse("btnExpand", string.Empty, true, "fa fa-plus-square", pIsWithOnClientClick);
        }
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetAwesomeButtonExpandCollapse(string pCssClass, bool pIsWithOnClientClick)
        {
            return GetAwesomeButtonExpandCollapse("btnExpand", string.Empty, true, pCssClass, pIsWithOnClientClick);
        }
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        public static WCToolTipLinkButton GetAwesomeButtonExpandCollapse(string pName, string pSuffix, bool pIsEnabled, string pCssClass, bool pIsWithOnClientClick)
        {
            string tooltip = Ressource.GetString(pCssClass.Contains("plus") ? "EXPAND" : "COLLAPSE", true);
            WCToolTipLinkButton btn = GetAwesomeLinkButton(pName + pSuffix, string.Empty, pCssClass, tooltip, string.Empty);
            btn.Enabled = pIsEnabled;
            btn.TabIndex = -1;
            btn.CausesValidation = false;
            if (pIsWithOnClientClick)
                btn.OnClientClick = "javascript:ExpandCollapseAll(this);return false;";
            return btn;
        }
        #endregion GetAwesomeButtonExpandCollapse
        #region public GetButtonPrint
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public static WCToolTipLinkButton GetButtonPrint(string pTblHeader)
        {
            return GetButtonPrint(pTblHeader, null);
        }
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public static WCToolTipLinkButton GetButtonPrint(string pTblHeader, string pTblBody)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton() 
            {
                ID = "btnPrint",
                CssClass = "fa-icon",
                Text = "<i class='fas fa-print'></i>",
            };
            btn.Pty.TooltipContent = Ressource.GetString(btn.ID);
            if (StrFunc.IsFilled(pTblBody))
                btn.Attributes.Add("onclick", "CallPrint('" + pTblHeader + "','" + pTblBody + "');return false;");
            else
                btn.Attributes.Add("onclick", "CallPrint('" + pTblHeader + "',null);return false;");
            return btn;
        }
        #endregion
        #region public GetButtonAction
        public static WCToolTipImageButton GetButtonAction2(string pName, string pSuffix, bool pIsEnabled, string pImageUrl)
        {
            WCToolTipImageButton button = new WCToolTipImageButton();
            button.Pty.TooltipContent = Ressource.GetString(pName);
            button.ID = pName + pSuffix;
            button.ImageUrl = pImageUrl;
            button.ImageAlign = ImageAlign.Middle; //20080227 PL
            button.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
            button.Enabled = pIsEnabled;
            button.TabIndex = -1;
            //
            return button;
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetAwesomeButtonAction(string pId, string pFaValue, bool pIsWithText)
        {
            return GetAwesomeButtonAction(pId, string.Empty, pFaValue, pIsWithText);
        }

        public static WCToolTipLinkButton GetAwesomeButtonAction(string pId, string pSuffix, string pFaValue, bool pIsWithText)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = pId + pSuffix,
                CssClass = "fa-icon",
                Text = String.Format("<i class='{0}'></i>", pFaValue)
            };
            if (pIsWithText)
                btn.Text = btn.Text + " " + Ressource.GetString(pId);
            else 
                btn.Pty.TooltipContent = Ressource.GetString(pId);

            return btn;
        }
        #endregion
        #region public GetAwesomeButtonCancel
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetAwesomeButtonCancel(bool pIsWithDoPostBack)
        {
            return GetAwesomeButtonCancel(string.Empty,pIsWithDoPostBack);
        }
        public static WCToolTipLinkButton GetAwesomeButtonCancel(string pSuffix, bool pIsWithDoPostBack)
        {
            WCToolTipLinkButton btn = GetAwesomeButtonAction("btnCancel", pSuffix, "fa fa-times", true);
            btn.Attributes.Add("onclick", "self.close();" + (pIsWithDoPostBack ? string.Empty : "return false;"));
            return btn;
        }
        #endregion
        #region public GetTextBoxSearch
        public static TextBox GetTextBoxSearch()
        {
            return GetTextBoxSearch(string.Empty);
        }
        public static TextBox GetTextBoxSearch(string pSuffix)
        {
            TextBox textbox = new TextBox
            {
                ID = "txtSearch" + pSuffix,
                CssClass = "txtSearch"
            };
            return textbox;
        }
        #endregion
        #region public GetButton
        public static WCToolTipButton GetButton(string pCtrlID, string pText, string pCssClass, string pTooltip, string pOnclick)
        {
            WCToolTipButton newButton = new WCToolTipButton
            {
                ID = pCtrlID,
                Text = pText
            };

            if (StrFunc.IsFilled(pCssClass))
                newButton.CssClass = pCssClass;

            if (StrFunc.IsFilled(pTooltip))
                newButton.Pty.TooltipContent = Ressource.GetString(pTooltip);

            if (StrFunc.IsFilled(pOnclick))
                newButton.Attributes.Add("onclick", pOnclick);
            return newButton;
        }
        #endregion
        #region public GetLinkButton
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static WCToolTipLinkButton GetAwesomeLinkButton(string pId, string pText, string pCssClass, string pTooltip, string pOnclick)
        {
            WCToolTipLinkButton newButton = GetAwesomeButtonAction(pId, pCssClass, StrFunc.IsFilled(pText));
            if (StrFunc.IsFilled(pTooltip))
                newButton.Pty.TooltipContent = Ressource.GetString(pTooltip);
            if (StrFunc.IsFilled(pOnclick))
                newButton.Attributes.Add("onclick", pOnclick);
            return newButton;
        }
        #endregion
        #region public GetImageButton
        public static WCToolTipImageButton GetImageButton(string pCtrlID, string pImageUrl, string pTooltip, string pOnclick)
        {
            WCToolTipImageButton newButton = new WCToolTipImageButton
            {
                ID = pCtrlID,
                ImageAlign = ImageAlign.Middle,
                ImageUrl = pImageUrl,
                Width = Unit.Percentage(100)
            };
            newButton.Width = Unit.Pixel(20);
            newButton.Height = Unit.Pixel(20);

            if (StrFunc.IsFilled(pTooltip))
                newButton.Pty.TooltipContent = Ressource.GetString(pTooltip);
            if (StrFunc.IsFilled(pOnclick))
                newButton.Attributes.Add("onclick", pOnclick);
            return newButton;
        }
        #endregion

        #region public GetListBox
        public static ListBox GetListBox(string pCtrlID)
        {
            ListBox lbList = new ListBox
            {
                ID = pCtrlID,
                SelectionMode = ListSelectionMode.Multiple,
                Width = Unit.Percentage(100),
                CssClass = EFSCssClass.Capture
            };
            return lbList;
        }
        #endregion
        #region public GetListBox
        public static OptionGroupListBox GetOptGroupListBox(string pCtrlID)
        {
            OptionGroupListBox lbList = new OptionGroupListBox
            {
                ID = pCtrlID,
                SelectionMode = ListSelectionMode.Multiple,
                Width = Unit.Percentage(100),
                CssClass = EFSCssClass.DropDownListCaptureLight
            };
            return lbList;
        }
        #endregion

        #region public GetLogo
        public static string GetLogoFileNameForIDA(string pCS, int pIDA, string pDisplayName)
        {
            string fileName = string.Empty;

            if (StrFunc.IsFilled(pCS))
            {
                string whereClause = SQLCst.WHERE + "(IDA = " + pIDA.ToString() + ")";

                //PL 20150319 TBD
                byte[] fileContent;
                try
                {
                    fileContent = OTCmlHelper.GetColumnBytesArrayFromDatabase("ACTORLOGO", "LOBANNER", whereClause, pCS, CommandType.Text);
                }
                catch
                {
                    fileContent = null;
                }
                if (fileContent == null)
                {
                    fileContent = OTCmlHelper.GetColumnBytesArrayFromDatabase(Cst.OTCml_TBL.ACTOR.ToString(), "LOLOGO", whereClause, pCS, CommandType.Text);
                }

                if (fileContent != null)
                {
                    string displayName = pDisplayName;
                    displayName = displayName.Replace(@" ", string.Empty);
                    displayName = displayName.Replace(@":", string.Empty);
                    displayName = displayName.Replace(@"/", string.Empty);
                    displayName = displayName.Replace(@"\", string.Empty);
                    displayName = displayName.Replace(@".", string.Empty);
                    displayName = displayName.Replace(@",", string.Empty);
                    displayName = displayName.Replace(@";", string.Empty);
                    //Nom du fichier image de l'ENTITY
                    fileName = @"LogoSmall_ID" + pIDA.ToString() + "_" + displayName;
                    //20080723 PL 000 devient jpg car HTML <img src=xxx" /> ne sait pas ouvrir un fichier 000
                    //fileName += @".000";
                    fileName += @".jpg";
                    //
                    string localFilePath = SessionTools.TemporaryDirectory.ImagesPathMapped + fileName;
                    if (Cst.ErrLevel.SUCCESS == FileTools.WriteBytesToFile(fileContent, localFilePath, FileTools.WriteFileOverrideMode.Override))
                        fileName = @"/" + SessionTools.TemporaryDirectory.RelativeImagesPath + fileName;
                    else
                        fileName = string.Empty;
                }
            }
            return fileName;
        }
        public static string GetLogoForEntity()
        {
            return GetLogoForEntity(@"/Images/Logo_Entity/EuroFinanceSystems/LogoSmall_EuroFinanceSystems.gif");
        }
        public static string GetLogoForEntity(string pDefault)
        {
            string fileName = string.Empty;

            //Find logo for IDENTITY
            if (StrFunc.IsFilled(SessionTools.Collaborator_ENTITY_DISPLAYNAME))
                if (StrFunc.IsFilled(SessionTools.Collaborator_ENTITY_IDA.ToString()))
                    fileName = GetLogoFileNameForIDA(SessionTools.CS, SessionTools.Collaborator_ENTITY_IDA, SessionTools.Collaborator_ENTITY_DISPLAYNAME);

            //Otherwise find logo for IDA
            if (StrFunc.IsEmpty(fileName) && StrFunc.IsFilled(SessionTools.Collaborator_DISPLAYNAME))
                if (StrFunc.IsFilled(SessionTools.Collaborator_IDA.ToString()))
                    fileName = GetLogoFileNameForIDA(SessionTools.CS, SessionTools.Collaborator_IDA, SessionTools.Collaborator_DISPLAYNAME);

            //Otherwise find logo for IDENTITY on the harddisk
            if (StrFunc.IsEmpty(fileName) && StrFunc.IsFilled(SessionTools.Collaborator_ENTITY_IDENTIFIER))
            {
                string tmp;
                bool isFound = false;
                if (!isFound)
                {
                    try
                    {
                        tmp = SessionTools.Collaborator_ENTITY_IDENTIFIER.Replace(" ", "");
                        fileName = @"Images/Logo_Entity/" + tmp + @"/LogoSmall_" + tmp + @".gif";
                        if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(fileName)))
                        {
                            isFound = true;
                            fileName = @"/" + fileName;
                        }
                    }
                    catch { }
                }
                if (!isFound)
                {
                    try
                    {
                        tmp = SessionTools.Collaborator_ENTITY_IDENTIFIER.Replace(" ", "");
                        fileName = @"Images/Logo_Entity/" + tmp + @"/LogoSmall_" + tmp + @".jpg";
                        if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(fileName)))
                        {
                            isFound = true;
                            fileName = @"/" + fileName;
                        }
                    }
                    catch { }
                }
                if (!isFound)
                {
                    try
                    {
                        tmp = SessionTools.Collaborator_ENTITY_DISPLAYNAME.Replace(" ", "");
                        fileName = @"Images/Logo_Entity/" + tmp + @"/LogoSmall_" + tmp + @".gif";
                        if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(fileName)))
                        {
                            isFound = true;
                            fileName = @"/" + fileName;
                        }
                    }
                    catch { }
                }
                if (!isFound)
                {
                    try
                    {
                        tmp = SessionTools.Collaborator_ENTITY_DISPLAYNAME.Replace(" ", "");
                        fileName = @"Images/Logo_Entity/" + tmp + @"/LogoSmall_" + tmp + @".jpg";
                        if (System.IO.File.Exists(HttpContext.Current.Server.MapPath(fileName)))
                        {
                            isFound = true;
                            fileName = @"/" + fileName;
                        }
                    }
                    catch { }
                }
                if (!isFound)
                    fileName = string.Empty;
            }
            //Otherwise set default
            if (StrFunc.IsEmpty(fileName))
                fileName = pDefault;

            return fileName;
        }
        #endregion GetLogo

        #region public NewTable
        public static Table NewTable()
        {
            return NewTable(null, null);
        }
        public static Table NewTable(WebControl pWebCtrl)
        {
            return NewTable(pWebCtrl, null);
        }
        public static Table NewTable(WebControl pWebCtrl, string pCtrlID)
        {
            return NewTable(pWebCtrl, pCtrlID, EFSCssClass.NoBorder);
        }
        public static Table NewTable(WebControl pWebCtrl, string pCtrlID, string pCssClass)
        {
            Table tblNew = new Table();
            if (pWebCtrl != null)
            {
                tblNew.Controls.Add(pWebCtrl);
                tblNew.CssClass = pCssClass;
            }
            if (pCtrlID != null)
                tblNew.ID = pCtrlID;
            else
                if (pWebCtrl != null)
                tblNew.ID = "TABLE" + pWebCtrl.ID;
            return tblNew;
        }
        #endregion Table

        #region public NewTableRow
        public static TableRow NewTableRow()
        {
            return NewTableRow(null, null);
        }
        public static TableRow NewTableRow(WebControl pWebCtrl)
        {
            return NewTableRow(pWebCtrl, null);
        }
        public static TableRow NewTableRow(WebControl pWebCtrl, string pCtrlID)
        {
            TableRow tblRowNew = new TableRow();
            if (pWebCtrl != null)
                tblRowNew.Controls.Add(pWebCtrl);
            if (pCtrlID != null)
                tblRowNew.ID = pCtrlID;
            else
                if (pWebCtrl != null)
                tblRowNew.ID = "TR" + pWebCtrl.ID;
            return tblRowNew;
        }
        #endregion

        #region public NewTableCell
        public static TableCell NewTableCell(WebControl pWebCtrl)
        {
            return ControlsTools.NewTableCell(pWebCtrl, System.Web.UI.WebControls.Unit.Empty);
        }
        public static TableCell NewTableCell(WebControl pWebCtrl, System.Web.UI.WebControls.Unit pWidth)
        {
            return ControlsTools.NewTableCell(pWebCtrl, null, pWidth);
        }
        public static TableCell NewTableCell(WebControl pWebCtrl, string pCtrlID)
        {
            return ControlsTools.NewTableCell(pWebCtrl, pCtrlID, System.Web.UI.WebControls.Unit.Empty);
        }
        public static TableCell NewTableCell(WebControl pWebCtrl, string pCtrlID, System.Web.UI.WebControls.Unit pWidth)
        {
            TableCell tblNewCell = new TableCell();
            if (pWidth != System.Web.UI.WebControls.Unit.Empty)
                tblNewCell.Width = pWidth;
            if (pWebCtrl != null)
                tblNewCell.Controls.Add(pWebCtrl);
            if (pCtrlID != null)
                tblNewCell.ID = "TD" + pCtrlID;
            else if (pWebCtrl != null)
                tblNewCell.ID = "TD" + pWebCtrl.ID;
            return tblNewCell;
        }
        public static TableCell NewTableCell(LiteralControl pWebCtrl)
        {
            TableCell tblNewCell = new TableCell();
            if (pWebCtrl != null)
                tblNewCell.Controls.Add(pWebCtrl);
            if (pWebCtrl != null)
                tblNewCell.ID = "TD" + pWebCtrl.ID;
            return tblNewCell;
        }
        #endregion

        public static LiteralControl NewSpaceLitteralControl(string pId)
        {
            LiteralControl space = new LiteralControl(Cst.HTMLSpace)
            {
                ID = pId
            };
            return space;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCtrl"></param>
        public static void AddCellSpace(TableRow pCtrl)
        {
            TableCell tblCellSpace;
            tblCellSpace = new TableCell
            {
                Width = Unit.Pixel(2),
                Text = Cst.HTMLSpace
            };
            pCtrl.Controls.Add(tblCellSpace);
        }
        #region public WebTdSeparator
        public static TableCell WebTdSeparator(bool pIsWidth100Pct)
        {
            TableCell td = new TableCell
            {
                CssClass = (pIsWidth100Pct ? "Separator_Gradient" : "Separator"),
                Text = Cst.HTMLSpace
            };
            td.Style.Add(HtmlTextWriterStyle.Width, pIsWidth100Pct ? "100%" : "1px");
            td.Style.Add(HtmlTextWriterStyle.Height, "100%");
            return td;
        }
        public static TableCell WebTdSeparator()
        {
            return WebTdSeparator(false);
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pPage"></param>
        /// <param name="pClientId"></param>
        public static void SetAttributeOnCLickButtonReferential(CustomObjectButtonReferential pCo, PageBase pPage, string pClientId)
        {
            WebControl control = pPage.FindControl(pClientId) as WebControl;
            SetAttributeOnCLickButtonReferential(pCo, control);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pControl"></param>
        /// EG 20120620 invoicing attribute New
        public static void SetAttributeOnCLickButtonReferential(CustomObjectButtonReferential pCo, WebControl pControl)
        {

            if ((null != pControl) && (null != pCo) && pCo.IsOk)
            {
                OpenReferentialArguments arg = new OpenReferentialArguments
                {
                    referential = pCo.Referential,
                    consultation = pCo.Consultation,
                    invoicing = pCo.Invoicing,
                    clientIdForKeyField = pCo.GetCtrlClientId(CustomObject.ControlEnum.textbox),
                    sqlColumn = (pCo.ContainsSqlColumn ? pCo.SqlColumn : null),
                    clientIdForSqlColumn = (pCo.ContainsClientIdForSqlColumn ? pCo.ClientIdForSqlColumn : null),
                    condition = (pCo.ContainsCondition ? pCo.Condition : null),
                    fk = (pCo.ContainsFK ? pCo.Fk : null),
                    title = (pCo.ContainsTitle ? pCo.Title : null),
                    param = (pCo.ContainsParam ? pCo.Param : null),
                    DynamicArgument = (pCo.ContainsDynamicArgument ? pCo.DynamicArgument : null)
                };
                // FI 20180502 [23926] Alimentation de arg.isFilteredWithSqlColumnOrKeyField
                if (pCo.ContainsFilteredWithSqlColumnOrKeyField)
                    arg.isFilteredWithSqlColumnOrKeyField = BoolFunc.IsTrue(pCo.FilteredWithSqlColumnOrKeyField);

                arg.SetAttributeOnCLickButtonReferential(pControl);
            }

        }

        /// <summary>
        /// Génère le code JS exécuté sur l'évènement onclick d'un bouton "..."
        /// - Ouvre une nouvelle instance du browser contenant une liste (datagrid) destiné à opéréer la sélection d'un item.
        /// - Lors de la sélection d’un item dans cette liste (ie dbl-click), une ou plusieurs valeurs sont remontées dans le contrôle ASP.Net de l'instance du browser appelant. 
        ///--------------- 
        ///Particularités: 
        ///--------------- 
        ///- Possibilité d’effectuer un pré-filtrage des item de la liste : (voir pIsFiltered)
        ///  Ex: si le champ txtACTOR_IDENTIFIER contient la valeur ‘T’, la liste sera alors filtrée via ACTOR.IDENTIFIER like ‘T%’  
        ///- Possibilité de réaliser un "append" (à la place d'un overwrite) dans les contrôles ASP.Net : (utilisation du séparateur ‘;’)
        ///  Ex: si le champ txtACTOR_IDENTIFIER contient la valeur ‘EFS;’, la liste sera alors ouverte sans aucun filtre 
        ///      et la valeur remontée n’écrasera pas la donnée existante, mais viendra s’ajouter à la suite : ‘EFS;xxxxx’
        ///  Ex: si le champ txtACTOR_IDENTIFIER contient la valeur ‘EFS;T’, la liste sera alors ouverte filtrée via ACTOR.IDENTIFIER like ‘T%’
        ///      et la valeur remontée n’écrasera pas la donnée existante, mais viendra s’ajouter à la suite : ‘EFS;xxxxx’
        ///- Possibilité de passer une valeur FK  (voir valueFK)
        ///  Ex: permettre la sélection d'un book parmi les books d’un acteur spécifique
        /// </summary>
        /// <param name="pControl">WebControl Button</param>
        /// <param name="pReferential">Nom du référentiel (eg: ACTOR, ASSET_RATEINDEX)</param>
        /// <param name="pTitle">Titre de la fenêtre (Par défaut: pReferential)</param>
        /// <param name="pListType">Enum Type de liste(eg: Referential)</param>
        /// <param name="pIsFiltered">Filtrage.
        /// Le filtre est opérée à partir, s'il existe, du contrôle ASP.Net "pOutClientId_SQLColumnname" et de son contenu,
        /// sinon, le filtre est opérée à partir du contrôle ASP.Net "pOutClientId_KeyField" et de son contenu</param>
        /// <param name="pOutSQLColumn_KeyFieldType">Type de KeyField utilisé dans le référentiel pour alimenter le contrôle htlm (DKF (DataKeyField) | KF (KeyField))</param>
        /// <param name="pOutClientId_KeyField">ClientId du contrôle ASP.Net alimenter par la donnée KeyField (eg: txtXYZ)</param>
        /// <param name="pOutSQLColumnname">Facultatif: Nom de la colonne SQL utilisée pour alimenter le contrôle ASP.Net (eg: DISPLAYNAME)</param>
        /// <param name="pOutClientId_SQLColumnname">Facultatif: ClientId du contrôle ASP.Net alimenté par la donnée issue de pOutSQLColumnname (eg: txtABC)</param>
        /// <param name="pCondApp">Facultatif: Nom de la condition d'application (&CondAdd) à utiliser (eg: 'BROKER' pour le référentiel ACTOR)</param>
        /// <param name="pFKValueForFilter">Facultatif: Valeur de la FK (Foreign Key) à utiliser pour restreindre la liste des items(eg: 99)</param>
        /// <param name="pParamList">Facultatif: Liste des valeurs destinées à remplacer les éventuels paramètres dynamiques %%PARAM%% présent dans le fichier XML (liste de valeur séparées par des ';')</param>
        /// <param name="pDynamicArg">Facultatif: ...</param>
        public static void SetOnClientClick_ButtonReferential(WebControl pControl, string pReferential, string pTitle, Cst.ListType pListType,
            bool pIsFiltered,
            string pOutSQLColumn_KeyFieldType, string pOutClientId_KeyField,
            string pOutSQLColumnname, string pOutClientId_SQLColumnname,
            string pCondApp, string pFKValueForFilter, string pParamList,
            string pDynamicArg)
        {
            //pOutSQLColumn_KeyFieldType: Actuellement seule une colonne "DataKeyField" ou "KeyField" peut être retournée pour l'alimentation du contrôle ASP.Net. 
            //                            Pour utiliser une autre colonne, exploiter le paramètre "pOutSQLColumnname".
            string subTitle = JavaScript.JS_NULL;
            string listType = JavaScript.JSString(pListType.ToString());
            string isFiltered = pIsFiltered.ToString().ToLower();
            //			
            pReferential = (StrFunc.IsEmpty(pReferential) ? JavaScript.JS_NULL : JavaScript.JSString(pReferential));
            pTitle = (StrFunc.IsEmpty(pTitle) ? pReferential : JavaScript.JSString(pTitle));
            pOutSQLColumn_KeyFieldType = (StrFunc.IsEmpty(pOutSQLColumn_KeyFieldType) ? JavaScript.JSString(Cst.KeyField) : JavaScript.JSString(pOutSQLColumn_KeyFieldType));
            pOutClientId_KeyField = (StrFunc.IsEmpty(pOutClientId_KeyField) ? JavaScript.JS_NULL : JavaScript.JSString(pOutClientId_KeyField));
            pOutSQLColumnname = (StrFunc.IsEmpty(pOutSQLColumnname) ? JavaScript.JS_NULL : JavaScript.JSString(pOutSQLColumnname));
            pOutClientId_SQLColumnname = (StrFunc.IsEmpty(pOutClientId_SQLColumnname) ? JavaScript.JS_NULL : JavaScript.JSString(pOutClientId_SQLColumnname));
            pCondApp = (StrFunc.IsEmpty(pCondApp) ? JavaScript.JS_NULL : JavaScript.JSString(pCondApp));
            pFKValueForFilter = (StrFunc.IsEmpty(pFKValueForFilter) ? JavaScript.JS_NULL : JavaScript.JSString(pFKValueForFilter));
            pParamList = (StrFunc.IsEmpty(pParamList) ? JavaScript.JS_NULL : JavaScript.JSString(pParamList));
            pDynamicArg = (StrFunc.IsEmpty(pDynamicArg) ? JavaScript.JS_NULL : JavaScript.JSString(pDynamicArg));
            //
            string onclientClick = "OpenReferential("
                + pReferential + ", "
                + pTitle + ", "
                + subTitle + ", "
                + listType + ", "
                + isFiltered + ", "
                + pOutSQLColumn_KeyFieldType + ", "
                + pOutClientId_KeyField + ", "
                + pOutSQLColumnname + ", "
                + pOutClientId_SQLColumnname + ","
                + pCondApp + ", "
                + pFKValueForFilter + ", "
                + pParamList + ", "
                + pDynamicArg
                + ");return false;";
            //
            SetOnClientClick(pControl, onclientClick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pCo"></param>
        /// <param name="pClientId"></param>
        public static void SetAttributeOnClickOnControlZoomFpmlObject(PageBase pPage, CustomObjectButtonFpmlObject pCo, string pClientId)
        {
            WebControl control = pPage.FindControl(pClientId) as WebControl;
            SetAttributeOnClickOnControlZoomFpmlObject(pCo, control);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pControl"></param>
        public static void SetAttributeOnClickOnControlZoomFpmlObject(CustomObjectButtonFpmlObject pCo, WebControl pControl)
        {
            bool isOk = null != pControl && null != pCo && pCo.IsOk;
            if (isOk)
            {
                // Onclick
                string args =
                    (pCo.ContainsObject ? pCo.Object : string.Empty) + ";"
                    + (pCo.ContainsObjectIndex ? pCo.ObjectIndex : string.Empty) + ";"
                    + (pCo.ContainsElement ? pCo.Element : string.Empty) + ";"
                    + (pCo.ContainsOccurence ? pCo.Occurence : string.Empty) + ";"
                    + (pCo.ContainsCopyTo ? pCo.CopyTo : string.Empty) + ";"
                    + (pCo.ContainsTitle ? pCo.Title : string.Empty) + ";"
                    + (pCo.ContainsIsZoomOnModeReadOnly ? pCo.IsZoomOnModeReadOnly : Cst.FpML_Boolean_False) + ";";
                //
                string onclientClick = "PostBack('" + Cst.FpML_ScreenFullCapture + "','" + args + "');return false;";
                SetOnClientClick(pControl, onclientClick);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pCo"></param>
        /// <param name="pClientId"></param>
        public static void SetAttributeOnClickOnButtonScreenBox(PageBase pPage, CustomObjectButtonScreenBox pCo, string pClientId)
        {
            WebControl control = pPage.FindControl(pClientId) as WebControl;
            SetAttributeOnClickOnButtonScreenBox(pCo, control);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pControl"></param>
        public static void SetAttributeOnClickOnButtonScreenBox(CustomObjectButtonScreenBox pCo, WebControl pControl)
        {

            bool isOk = (null != pControl) && (null != pCo) && pCo.IsOk;
            if (isOk)
            {
                // Onclick
                string args =
                    (pCo.ContainsScreenBox ? pCo.ScreenBox : string.Empty) + ";" +
                    (pCo.ContainsDialogStyle ? pCo.DialogStyle : string.Empty) + ";" +
                    (pCo.ContainsImageUrl ? pCo.ImageUrl : string.Empty) + ";" +
                    (pCo.ContainsTitle ? pCo.Title : string.Empty) + ";";

                string onclientClick = "PostBack('" + Cst.OTCml_ScreenBox + "','" + args + "');return false;";
                SetOnClientClick(pControl, onclientClick);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pPage"></param>
        /// <param name="pClientId"></param>
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre ouverture page(CustomObjectButtonInputMenu)
        public static void SetAttributeOnClickOnButtonMenu(PageBase pPage, CustomObjectButtonInputMenu pCo, string pClientId)
        {
            WebControl control = pPage.FindControl(pClientId) as WebControl;
            SetAttributeOnClickOnButtonMenu(pPage, pCo, control);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pControl"></param>
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre ouverture page(CustomObjectButtonInputMenu)
        public static void SetAttributeOnClickOnButtonMenu(PageBase pPage, CustomObjectButtonInputMenu pCo, WebControl pControl)
        {
            if ((null != pControl) && (null != pCo) && pCo.IsOk)
            {
                OpenMenuArguments arg = new OpenMenuArguments
                {
                    guid = pPage.GUID,
                    menu = pCo.Menu,
                    pk = (pCo.ContainsPK ? pCo.PK : null),
                    pkv = (pCo.ContainsPKV ? pCo.PKV : null),
                    fkv = (pCo.ContainsFKV ? pCo.FKV : null),
                    title = (pCo.ContainsTitle ? pCo.Title : null)
                };
                arg.SetAttributeOnClickOnButtonMenu(pControl);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pMenu"></param>
        /// <param name="pPK"></param>
        /// <param name="pPKV"></param>
        /// <param name="pFKV"></param>
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre ouverture page(CustomObjectButtonInputMenu)
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        public static void SetAttributeOnClickOnButtonMenu(WebControl pControl, string pMenu, string pPK, string pPKV, string pFKV,  string pGUID)
        {

            string url = SessionTools.Menus.GetMenu_Url(pMenu);
            if (StrFunc.IsFilled(url))
            {
                string idMenu = "IDMenu=" + pMenu;
                if (url.IndexOf(idMenu) <= 0)
                {
                    if (url.IndexOf("?") <= 0)
                        url += "?" + idMenu;
                    else
                        url += "&" + idMenu;
                }
                if (StrFunc.IsFilled(pGUID))
                    url += "&GUID=" + pGUID;
                if (StrFunc.IsFilled(pPK))
                    url += "&PK=" + pPK;
                if (StrFunc.IsFilled(pPKV))
                    url += "&PKV=" + pPKV;
                if (StrFunc.IsFilled(pFKV))
                    url += "&FKV=" + pFKV;

                string onclientClick = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential);
                onclientClick += ";return false;";
                SetOnClientClick(pControl, onclientClick);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pControl"></param>
        //EG 20120613 BlockUI New
        public static void SetAttributeOnClickOnButton(Page pPage, CustomObjectButton pCo, WebControl pControl)
        {

            if ((null != pControl) && (null != pCo) && pCo.ContainsOnClick)
            {
                string onclientClick = string.Empty;
                if (pCo.IsBlockUIAttached)
                {
                    string message = "Msg_WaitingRefresh";
                    Type tControl = pControl.GetType();
                    if (tControl.Equals(typeof(WCToolTipButton)))
                        message = ((WCToolTipButton)pControl).Pty.TooltipContent + "...";
                    else if (tControl.Equals(typeof(WCToolTipImageButton)))
                        message = ((WCToolTipImageButton)pControl).Pty.TooltipContent + "...";
                    onclientClick += "Block(" + JavaScript.HTMLBlockUIMessage(pPage, message) + ");";
                }
                onclientClick += "PostBack('ExecFunction','" + pCo.ClientId + "');return false;";
                SetOnClientClick(pControl, onclientClick);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pOnClientClick"></param>
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Click sur Bouton Enabled
        public static void SetOnClientClick(WebControl pControl, string pOnClientClick)
        {
            string onclick = (pControl.Enabled) ? pOnClientClick : string.Empty;
            if (pControl.GetType().Equals(typeof(ImageButton)))
                ((ImageButton)pControl).OnClientClick = onclick;
            else if (pControl.GetType().Equals(typeof(Button)))
                ((Button)pControl).OnClientClick = onclick;
            else if (pControl.GetType().Equals(typeof(WCToolTipImageButton)))
                ((WCToolTipImageButton)pControl).OnClientClick = onclick;
            else if (pControl.GetType().Equals(typeof(WCToolTipButton)))
                ((WCToolTipButton)pControl).OnClientClick = onclick;
            else if (pControl.GetType().Equals(typeof(WCToolTipLinkButton)))
                ((WCToolTipLinkButton)pControl).OnClientClick = onclick;
            else
                pControl.Attributes.Add("onclick", onclick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDDL"></param>
        public static void DDLItemsSetTextOnTitle(DropDownList pDDL)
        {

            if (null != pDDL)
            {
                for (int i = 0; i < ArrFunc.Count(pDDL.Items); i++)
                    pDDL.Items[i].Attributes["title"] = pDDL.Items[i].Text;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        /// <param name="pStyles"></param>
        /// EG 20160115 New 
        // EG 20160404 Migration vs2013
        public static void SetStyleList(CssStyleCollection pCol, string pStyles)
        {
            if (StrFunc.IsFilled(pStyles))
            {
                string[] styleList = pStyles.Split(';');
                foreach (string s in styleList)
                {
                    if (StrFunc.IsFilled(s) && StrFunc.ContainsIn(s, ":"))
                    {
                        string[] styleValue = s.Split(':');
                        string key = styleValue[0].Trim();
                        string value = styleValue[1].Trim();

                        try
                        {
                            // EG 20160404 Migration vs2013
                            //HtmlTextWriterStyle htws = (HtmlTextWriterStyle)Enum.Parse(typeof(HtmlTextWriterStyle), key.Replace("-", ""), true);
                            //pCol.Add(htws, value);
                            if (Enum.IsDefined(typeof(HtmlTextWriterStyle), key.Replace("-", "")))
                            {
                                HtmlTextWriterStyle htws = (HtmlTextWriterStyle)Enum.Parse(typeof(HtmlTextWriterStyle), key.Replace("-", ""), true);
                                pCol.Add(htws, value);
                            }
                            else
                            {
                                pCol.Add(key, value);
                            }

                        }
                        catch (ArgumentException) { pCol.Add(key, value); }
                        /*
                        if (null == pCol[key])
                            pCol.Add(key, value);
                        else
                            pCol[key] = value;
                        */
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        /// <param name="pKeyStyle"></param>
        public static void RemoveStyle(CssStyleCollection pCol, string pKeyStyle)
        {

            if (null != pCol)
                pCol.Remove(pKeyStyle);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCell"></param>
        /// <param name="pFixedColValues"></param>
        public static void SetFixedCol(TableCell pCell, string pFixedColValues)
        {
            if (StrFunc.IsFilled(pFixedColValues))
            {

                string cssClass = string.Empty;
                bool isHeader = false;
                string bgColor = string.Empty;
                string[] fixedColValues = pFixedColValues.Split(' ');
                for (int i = 0; i < fixedColValues.Length; i++)
                {
                    string value = fixedColValues[i].ToLower();
                    if (StrFunc.IsEmpty(cssClass))
                    {
                        if ("left" == value)
                            cssClass = EFSCssClass.LeftFixedCol;
                        else if ("right" == value)
                            cssClass = EFSCssClass.RightFixedCol;

                        if (StrFunc.IsFilled(cssClass))
                            continue;
                    }
                    if (false == isHeader)
                    {
                        isHeader = ("header" == value.ToLower());
                        if (isHeader)
                            continue;
                    }
                    if (StrFunc.IsEmpty(bgColor))
                        bgColor = value;
                }
                if (StrFunc.IsEmpty(cssClass))
                    cssClass = EFSCssClass.LeftFixedCol;
                pCell.CssClass = cssClass;
                pCell.Style[HtmlTextWriterStyle.ZIndex] = (isHeader ? "100" : "20");

                if (StrFunc.IsEmpty(bgColor) || (Color.FromArgb(0, 0, 0) == Color.FromName(bgColor)))
                    bgColor = Color.Transparent.Name;

                pCell.Style[HtmlTextWriterStyle.BackgroundColor] = bgColor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCol"></param>
        /// <param name="pAttribs"></param>
        public static void SetAttributesList(AttributeCollection pCol, string pAttribs)
        {

            if (StrFunc.IsFilled(pAttribs))
            {
                string[] attribList = pAttribs.Split(';');
                foreach (string s in attribList)
                {
                    string[] attribValue = s.Split(':');
                    string key = attribValue[0].Trim();
                    string value = attribValue[1].Trim();

                    if (null == pCol[key])
                        pCol.Add(key, value);
                    else if (value != pCol[key])
                        pCol[key] += ";" + value;
                }
            }

        }

        /// <summary>
        /// Get or create the control label "lblMissingUserRights", this label is shown to the user when the user try to change a
        /// consultation template without the necessary permissions. the control is returned with Visible false. the control ID has been set to
        /// "lblMissingUserRights".
        /// </summary>
        /// <param name="pPlhMain">Form place holder will be hosting the control, NOT null</param>
        /// <exception cref="ArgumentNullException">raised when pPlhMain is null</exception>
        /// <returns>the label "lblMissingUserRights" control</returns>
        public static WCTooltipLabel GetLabelMissingUserRightsLstTemplate(PlaceHolder pPlhMain)
        {
            if (pPlhMain == null)
            {
                throw new ArgumentNullException("pPlhMain", "place holder input parameter must be NOT null");
            }

            if (!(pPlhMain.FindControl("lblMissingUserRightsLstTemplate") is WCTooltipLabel labelDisabled))
            {
                labelDisabled = new WCTooltipLabel
                {
                    Text = Ressource.GetString("lblMissingUserRightsLstTemplate"),
                    ID = "lblMissingUserRightsLstTemplate",
                    CssClass = "NoRightsSummary",
                    Visible = false
                };
            }
            return labelDisabled;
        }

        #region CreateControlCheckListProcess
        public static OptionGroupCheckBoxList CreateControlCheckListProcess()
        {
            return CreateControlCheckListProcess(0);
        }

        public static OptionGroupCheckBoxList CreateControlCheckListProcess(Int64 pPowerSelected)
        {
            Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>> m_ServiceObserver = ProcessTools.CreateDicServiceProcess();
            return CreateControlCheckListProcess(m_ServiceObserver, pPowerSelected);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        public static OptionGroupCheckBoxList CreateControlCheckListProcess(
            Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>> pServiceObserver, Int64 pPowerSelected)
        {
            Cst.ServiceEnum service = Cst.ServiceEnum.NA;
            OptionGroupCheckBoxList lstProcess = new OptionGroupCheckBoxList
            {
                EnableViewState = true,
                RepeatColumns = 3,
                RepeatLayout = RepeatLayout.Table,
                ID = "LSTPROCESS",
                Width = Unit.Percentage(100)
            };

            ExtendedListItem extLi = null;
            ListItem item = null;
            int extLiIndex = -1;
            foreach (string s in Enum.GetNames(typeof(Cst.ServiceEnum)))
            {
                service = (Cst.ServiceEnum)Enum.Parse(typeof(Cst.ServiceEnum), s);
                if (pServiceObserver.ContainsKey(service))
                {
                    lstProcess.Items.Add(service.ToString());
                    extLiIndex = lstProcess.ExtendedItems.Count - 1;
                    extLi = lstProcess.ExtendedItems[extLiIndex];
                    extLi.GroupingType = ListItemGroupingTypeEnum.New;
                    extLi.GroupingText = extLi.Text;
                    extLi.GroupingClass = "fa-icon fab fa-whmcs violet imgServiceObserver";
                    extLi.GroupCssClass = "chkServiceObserver";

                    pServiceObserver[service].ForEach(process =>
                    {
                        item = new ListItem(process.Second, process.First.ToString());
                        // check de l'item de la liste 
                        int i = int.Parse(Enum.Format(typeof(Cst.ProcessTypeEnum), process.First, "d"));
                        item.Selected = (0 < (pPowerSelected & Convert.ToInt64(Math.Pow(2, i))));
                        item.Attributes.Add("class", "chkProcessObserver");
                        item.Attributes.Add("alt", process.First.ToString());
                        if (item.Selected)
                            item.Attributes.Add("checked", "checked");
                        lstProcess.Items.Add(item);
                    }
                    );
                }
            }
            return lstProcess;
        }
        #endregion CreateControlCheckListProcess

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static CheckBoxList CreateCheckBoxListGroupTracker(Int64 pPowerSelected)
        {
            CheckBoxList cbl = new CheckBoxList
            {
                RepeatLayout = RepeatLayout.UnorderedList,
                CssClass = "paramGroupTracker"
            };
            foreach (Cst.GroupTrackerEnum group in Enum.GetValues(typeof(Cst.GroupTrackerEnum)))
            {
                if (group != Cst.GroupTrackerEnum.ALL)
                {
                    object[] enumAttrs = group.GetType().GetField(group.ToString()).GetCustomAttributes(typeof(XmlEnumAttribute), true);
                    string nameGroup;
                    if (ArrFunc.IsFilled(enumAttrs))
                        nameGroup = Ressource.GetString(((XmlEnumAttribute)enumAttrs[0]).Name, group.ToString());
                    else
                        nameGroup = Ressource.GetString(group.ToString(), group.ToString());

                    ListItem lstItem = new ListItem(nameGroup, group.ToString());
                    string hexValue = Enum.Format(typeof(Cst.GroupTrackerEnum), group, "x");
                    lstItem.Selected = (0 == pPowerSelected) || (0 < (pPowerSelected & int.Parse(hexValue, NumberStyles.HexNumber)));

                    cbl.Items.Add(lstItem);
                }
            }
            return cbl;
        }

        /// <summary>
        /// Applique un look au contrôle {pControl} en fonction du statut match {pStatus}
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pStatus"></param>
        /// FI 20140708 [20179] add method
        /// FI 20200124 [XXXX] Refactoring usage de class css
        public static void SetLookMatch(WebControl pControl, Nullable<Cst.MatchEnum> pStatus)
        {

            Cst.MatchEnum cciMatchVal = Cst.MatchEnum.unmatch;
            if (pStatus.HasValue)
                cciMatchVal = pStatus.Value;

            if (StrFunc.IsFilled(pControl.CssClass))
            {
                foreach (Cst.MatchEnum item in Enum.GetValues(typeof(Cst.MatchEnum)))
                    ControlsTools.RemoveCssClass(pControl, StrFunc.AppendFormat("ccimatch ccimatch_{0}", item.ToString()));
            }

            pControl.CssClass = StrFunc.AppendFormat("{0} ccimatch ccimatch_{1}",
                StrFunc.IsFilled(pControl.CssClass) ? pControl.CssClass : string.Empty, cciMatchVal);

            if (pControl.GetType().Equals(typeof(WCDropDownList2)))
                ((WCDropDownList2)pControl).IsSynchonizeLblViewerCssClass = true;
        }

        /// <summary>
        /// Retourne le statut Match attribué au contrôle {pControl}
        /// </summary>
        /// <param name="pControl"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] add method
        /// FI 20200124 [XXXX] Refactoring usage de class css
        public static Nullable<Cst.MatchEnum> GetMatchValue(WebControl pControl)
        {
            ControlsTools.HasCssMatch(pControl, out Cst.MatchEnum? ret);
            return ret;
        }

        /// <summary>
        /// Le look du contôle passe de "match" à "unmatch" et vis et versa
        /// </summary>
        /// <param name="pControl"></param>
        /// FI 20140708 [20179] add method
        /// FI 20200124 [XXXXX] Rename 
        public static void ToggleMatch(WebControl pControl)
        {
            Nullable<Cst.MatchEnum> statut = GetMatchValue(pControl);
            
            if (false == statut.HasValue)
                throw new NotSupportedException("css match expected");

            if (statut.Value != Cst.MatchEnum.match)
                statut = Cst.MatchEnum.match;
            else
                statut = Cst.MatchEnum.mismatch;

            SetLookMatch(pControl, statut);
        }

        /// <summary>
        /// Retourne true si le contrôle contient une classe dédiée au matching
        /// </summary>
        /// <param name="pControl"></param>
        /// <returns></returns>
        /// FI 20200124 [XXXX] Add Method
        public static Boolean HasCssMatch(WebControl pControl, out Nullable<Cst.MatchEnum> pMatchValue)
        {
            if (null == pControl)
                throw new ArgumentNullException("pControl is null");

            pMatchValue = null;

            Boolean ret = false;
            if (StrFunc.IsFilled(pControl.CssClass))
            {
                foreach (Cst.MatchEnum item in Enum.GetValues(typeof(Cst.MatchEnum)))
                {
                    ret = pControl.CssClass.Contains(StrFunc.AppendFormat("ccimatch ccimatch_{0}", item.ToString()));
                    if (ret)
                    {
                        pMatchValue = item;
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne true si le contrôle contient une classe dédiée au matching
        /// </summary>
        /// <param name="pControl"></param>
        /// <returns></returns>
        /// FI 20200124 [XXXX] Add Method
        public static Boolean HasCssMatch(WebControl pControl)
        {
            return HasCssMatch(pControl, out _);
        }


        /// <summary>
        /// Mise en place de l'url sur le click du control {pControl}
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="url"></param>
        /// FI 20140912 [XXXXX] Add Method
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        public static void SetHyperLinkOpenFormReferential(WebControl pControl, string url)
        {
            ControlsTools.SetStyleList(pControl.Style, "cursor:pointer");

            string js = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential) + ";return false;";
            ControlsTools.SetOnClientClick(pControl, js);

            pControl.Attributes.Add("onmouseover", "this.style.textDecoration='underline'");
            pControl.Attributes.Add("onmouseout", "this.style.textDecoration='none'");
        }


        /// <summary>
        /// Mise en place de l'url sur le click du control {pControl}
        /// <para>Usage d'attribut Css plutôt que des fonctions js</para>
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="url"></param>
        /// FI 20191128 [XXXXX] Add Method
        /// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        public static void SetHyperLinkOpenFormReferential2(WebControl pControl, string url)
        {
            if (null == pControl)
                throw new ArgumentNullException("pControl", "pControl parameter is null");

            WebControl webControl = pControl;
            WebControl webControlJs = webControl;

            if (StrFunc.IsFilled(webControl.CssClass))
                webControl.CssClass += " Link";
            else
                webControl.CssClass = "Link";

            if (pControl is WCDropDownList2)
            {
                WCDropDownList2 ddl = pControl as WCDropDownList2;
                if (ddl.HasViewer)
                {
                    ddl.IsSynchonizeLblViewerCssClass = true;
                    webControlJs = ddl.LblViewer;
                }
            }

            string js = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_FormReferential) + "return false;";
            ControlsTools.SetOnClientClick(webControlJs, js);
        }

        /// <summary>
        /// Chargement des traders/Algo d'un acteur
        /// <para></para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDDL">Représente la DDL</param>
        /// <param name="pIsIDinValue">si True, ACTOR.IDA pour alimenter ListItem.value. si false, ACTOR.IDENTIFIER pour alimenter ListItem.value. </param>
        /// <param name="pIdA">Représente l'acteur</param>
        /// <param name="pUser">Utilisateur connecté (pour application des restrictions)</param>
        /// <param name="pSessionID">Session de l'utilisateur connecté (pour application des restrictions)</param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add 
        public static bool DDL_LoadTrader(string pCS, DropDownList pDDL, Boolean pIsIDinValue, int pIdA, User pUser, string pSessionID)
        {
            bool isOptGroupDDL = (null != (pDDL as OptionGroupDropDownList));
            OptionGroupDropDownList optGroupDDL = null;
            if (isOptGroupDDL)
                optGroupDDL = (OptionGroupDropDownList)pDDL;

            pDDL.Items.Clear();

            if (pIdA > 0)
            {
                IEnumerable<Tuple<int, string, RoleActor>> lstLoad = ActorTools.LoadTraderAlgo(pCS, pIdA, pUser, pSessionID);
                string lastGrp = string.Empty;

                if (lstLoad.Count() > 0)
                {
                    foreach (var item in lstLoad)
                    {
                        if (isOptGroupDDL)
                        {
                            if (lastGrp != item.Item3.ToString())
                            {
                                lastGrp = item.Item3.ToString();
                                string ddlText = ((item.Item3 == RoleActor.INVESTDECISION) || (item.Item3 == RoleActor.EXECUTION)) ?
                                                    Ressource.GetString("GroupPartyAlgo") : Ressource.GetString("GroupParty");
                                pDDL.Items.Add(new ListItem(ddlText, ddlText));

                                ExtendedListItem extLi = optGroupDDL.ExtendedItems[optGroupDDL.ExtendedItems.Count - 1];
                                extLi.GroupingType = ListItemGroupingTypeEnum.New;
                                extLi.GroupingText = extLi.Text;
                            }
                        }

                        pDDL.Items.Add(new ListItem(item.Item2, pIsIDinValue ? item.Item1.ToString() : item.Item2));

                        if (isOptGroupDDL)
                            optGroupDDL.ExtendedItems[optGroupDDL.ExtendedItems.Count - 1].GroupingType = ListItemGroupingTypeEnum.Inherit;
                    }
                }
            }

            pDDL.ClearSelection();

            return (pDDL.Items.Count > 0);
        }

        /// <summary>
        /// Load a Sales DDL from an actor and an entity
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDDL">Représente la DDL</param>
        /// <param name="pIsIDinValue">si True, ACTOR.IDA pour alimenter ListItem.value. si false, ACTOR.IDENTIFIER pour alimenter ListItem.value. </param>
        /// <param name="pIdA">Représente l'acteur</param>
        /// <param name="pIdA">Représente l'entité</param>
        /// <param name="pUser">Utilisateur connecté (pour application des restrictions)</param>
        /// <param name="pSessionID">Session de l'utilisateur connecté (pour application des restrictions)</param>
        /// <returns></returns>
        /// FI 20121127 [18224] refactoring
        /// FI 20170928 [23452] Add 
        public static bool DDL_LoadSales(string pCS, DropDownList pDDL, Boolean pIsIDinValue, int pIdA, int pIdAEntity, User pUser, string pSessionID)
        {
            bool isOptGroupDDL = (null != (pDDL as OptionGroupDropDownList));

            OptionGroupDropDownList optGroupDDL = null;
            if (isOptGroupDDL)
                optGroupDDL = (OptionGroupDropDownList)pDDL;

            pDDL.Items.Clear();

            if (pIdA > 0)
            {
                // Les sales relatif à la partie
                IEnumerable<Tuple<int, string, RoleActor>> lstLoad = ActorTools.LoadSalesAlgo(pCS, pIdA, pUser, pSessionID);
                string lastGrp = string.Empty;
                if (lstLoad.Count() > 0)
                {
                    foreach (var item in lstLoad)
                    {
                        if (isOptGroupDDL)
                        {
                            if (lastGrp != item.Item3.ToString())
                            {
                                lastGrp = item.Item3.ToString();
                                string ddlText = ((item.Item3 == RoleActor.INVESTDECISION) || (item.Item3 == RoleActor.EXECUTION)) ?
                                    Ressource.GetString("GroupPartyAlgo") : Ressource.GetString("GroupParty");
                                pDDL.Items.Add(new ListItem(ddlText, ddlText));

                                ExtendedListItem extLi = optGroupDDL.ExtendedItems[optGroupDDL.ExtendedItems.Count - 1];
                                extLi.GroupingType = ListItemGroupingTypeEnum.New;
                                extLi.GroupingText = extLi.Text;
                            }
                        }

                        pDDL.Items.Add(new ListItem(item.Item2, pIsIDinValue? item.Item1.ToString() : item.Item2));

                        if (isOptGroupDDL)
                            optGroupDDL.ExtendedItems[optGroupDDL.ExtendedItems.Count - 1].GroupingType = ListItemGroupingTypeEnum.Inherit;
                    }
                }

                // Les sales relatif à l'entité
                if (pIdA != pIdAEntity)
                {
                    Nullable<int> idAEntity = null;
                    if (pIdAEntity > 0)
                        idAEntity = pIdAEntity;

                    IEnumerable<Tuple<int, string, RoleActor>> lstLoad2 = ActorTools.LoadSalesAlgo(pCS, idAEntity, pUser, pSessionID);

                    lastGrp = string.Empty;
                    if (lstLoad2.Count() > 0)
                    {
                        foreach (var item in lstLoad2.Where(x => (null == lstLoad.Where(y => y.Item1 == x.Item1).FirstOrDefault())))
                        {
                            if (isOptGroupDDL)
                            {
                                if (lastGrp != item.Item3.ToString())
                                {
                                    lastGrp = item.Item3.ToString();

                                    string res1 = (idAEntity.HasValue) ? "Entity" : "Global";
                                    string res2 = ((item.Item3 == RoleActor.INVESTDECISION) || (item.Item3 == RoleActor.EXECUTION)) ? "Algo" : string.Empty;
                                    string resGrp = Ressource.GetString(StrFunc.AppendFormat("Group{0}{1}", res1, res2));
                                    pDDL.Items.Add(new ListItem(resGrp, resGrp));

                                    ExtendedListItem extLi = optGroupDDL.ExtendedItems[optGroupDDL.ExtendedItems.Count - 1];
                                    extLi.GroupingType = ListItemGroupingTypeEnum.New;
                                    extLi.GroupingText = extLi.Text;
                                }
                            }

                            ListItem li = new ListItem(item.Item2, pIsIDinValue ? item.Item1.ToString(): item.Item2);
                            pDDL.Items.Add(li);
                            if (isOptGroupDDL)
                            {
                                optGroupDDL.ExtendedItems[optGroupDDL.ExtendedItems.Count - 1].GroupingType = ListItemGroupingTypeEnum.Inherit;
                            }
                            else
                            {
                                string backColor = ((idAEntity == null) ? "#FFD15D" : "#D9C19B");
                                li.Attributes.Add("style", "background-color:" + backColor + ";");
                            }
                        }
                    }
                }
            }

            pDDL.ClearSelection();

            return (pDDL.Items.Count > 0);
        }

    }
    #endregion class ControlsTools

    /// 
    /// </summary>
    /// EG 20161122 Add partial
    public sealed partial class PageTools
    {

        /// <summary>
        ///  retourne tag html s'il est défini &lt;html runat="server"&gt;
        /// </summary>
        public static HtmlGenericControl SearchHtmlControl(Page pPage)
        {
            HtmlGenericControl ret = null;
            HtmlGenericControl[] htmlControl = SearchHtmlGenericControl(pPage, "html");
            if (ArrFunc.Count(htmlControl) > 0)
                ret = htmlControl[0];
            return ret;
        }


        /// <summary>
        ///  retourne tag body s'il est défini &lt;body runat="server"&gt;
        /// </summary>
        public static HtmlGenericControl SearchBodyControl(Page pPage)
        {
            HtmlGenericControl ret = null;
            HtmlGenericControl htmlTag = SearchHtmlControl(pPage);
            HtmlGenericControl[] result;
            if (null != htmlTag)
                result = SearchHtmlGenericControl(htmlTag, "body");
            else
                result = SearchHtmlGenericControl(pPage, "body");
            if (null != result)
                ret = result[0];
            return ret;
        }


        /// <summary>
        ///  retourne tag head s'il est défini &lt;body runat="server"&gt;
        /// </summary>
        public static HtmlGenericControl SearchHeadControl(Page pPage)
        {
            HtmlGenericControl ret = null;
            HtmlGenericControl htmlTag = SearchHtmlControl(pPage);
            HtmlGenericControl[] result;
            if (null != htmlTag)
                result = SearchHtmlGenericControl(htmlTag, "head");
            else
                result = SearchHtmlGenericControl(pPage, "head");

            if (null != result)
                ret = result[0];
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pTagName"></param>
        /// <returns></returns>
        public static HtmlGenericControl[] SearchHtmlGenericControl(Control pControl, string pTagName)
        {

            HtmlGenericControl[] ret = null;
            ArrayList al = new ArrayList();
            //
            for (int i = 0; i < ArrFunc.Count(pControl.Controls); i++)
            {
                if (pControl.Controls[i].GetType().Equals(typeof(HtmlGenericControl)))
                {
                    HtmlGenericControl htmlControl = (HtmlGenericControl)pControl.Controls[i];
                    if (pTagName.ToLower() == htmlControl.TagName.ToLower())
                        al.Add(htmlControl);
                }
            }
            if (al.Count > 0)
                ret = (HtmlGenericControl[])al.ToArray(typeof(HtmlGenericControl));
            return ret;

        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static Control GetHeadTag(PageBase pPage)
        {
            Control head;
            if (null != pPage.Header)
                head = (Control)pPage.Header;
            else
                head = (Control)PageTools.SearchHeadControl(pPage);
            return head;
        }

        /// <summary>
        /// add stylesheet link on &lt;head&gt; runat="server"
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pTitle"></param>
        /// <param name="pLinkCss"></param>
        /// <param name="pLinkIcon"></param>
        /// <remarks>
        /// Cette méthode ne doit être appelée que si le tag &lt;head runat="server" &gt; est déclaré sur la page
        /// ou si le tag head est généré en runtime avec un HtmlGenericControl
        /// Attention, pour utliser le calendarExtender il faut que sur la page soit déclaré ;head runat="server" &gt;
        /// </remarks> 
        /// 20090603 EG Ticket 16497 Ajout Gestion du titre sur la page
        /// EG 20170822 [23342] Add VirtualPath
        // EG 20190411 [ExportFromCSV] Upd Link CSSAwesome
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        public static void SetHead(PageBase pPage, string pTitle, string pCssMode, string pImage)
        {

            if ((null == pPage.Header) && (null == PageTools.SearchHeadControl(pPage)))
                throw new NotSupportedException("This page is missing a head runat server. Please add <head runat=\"server\" />.");
            //
            Control head = GetHeadTag(pPage);
            // Add Title
            SetHeaderTitle(head, "titlePage", pTitle);
            // Meta Tag
            SetMetaTag(head);
            // Meta Tag
            //SetMetaTagIECompatibility(head, "EmulateIE8");

            //Add linkCssCommon
            SetHeaderLink(head, "linkCssAwesome", pPage.VirtualPath("Includes/fontawesome-all.min.css"));
            SetHeaderLink(head, "linkCssCommon", pPage.VirtualPath("Includes/EFSThemeCommon.min.css"));
            SetHeaderLink(head, "linkCssCustomCommon", pPage.VirtualPath("Includes/CustomThemeCommon.css"));
            SetHeaderLink(head, "linkCssUISprites", pPage.VirtualPath("Includes/EFSUISprites.min.css"));

            string cssColor = string.Empty;
            AspTools.CheckCssColor(ref cssColor);
            pPage.CSSColor = cssColor;

            string cssMode = pCssMode;
            AspTools.CheckCssMode(ref cssMode);
            pPage.CSSMode = cssMode;

            string linkCss = pPage.VirtualPath(String.Format("Includes/EFSTheme-{0}.min.css", cssMode));
            SetHeaderLink(head, "linkCss", linkCss);

                       
            JQuery.UI.WriteHeaderLink(head, JQuery.Engines.JQuery);
            //add linkIcon
            SetHeaderLinkIcon(pPage, pImage);
            HtmlGenericControl style = new HtmlGenericControl("style");
            style.Attributes.Add("type", "text/css");
            style.InnerText = "THEAD { DISPLAY: table-header-group }  TFOOT { DISPLAY: table-footer-group }";
            head.Controls.Add(style);


        }
        // EG 20200903 [XXXXX] New
        public static void SetHeaderLinkIcon(PageBase pPage, string pImage)
        {
            Control head = GetHeadTag(pPage);
            string linkImage = @"~/Images/ico/" + ControlsTools.GetIconPageMenu(pImage) + ".ico";
            SetHeaderLink(head, "linkIcon", "shortcut icon", "image/x-ico", linkImage);
            linkImage = linkImage.Replace("ico", "png");
            SetHeaderLink(head, "linkIcon2", "shortcut icon", "image/x-png", linkImage);
        }
        /// <summary>
        /// set Attributes on &lt;html&gt; 
        /// </summary>
        /// <param name="pOwnerPage"></param>
        /// <param name="pIsModal"></param>
        /// <param name="pUrlOnBeforeUnload"></param>
        /// 
        /// Cette méthode ne doit être appelée que si le tag &lt;html runat="server" &gt; est déclaré sur la page
        /// </remarks> 
        public static void SetBody(PageBase pOwnerPage, bool pIsModal, string pUrlOnBeforeUnload)
        {
            //Mise à jour du body (s'il existe un <head runat="server"> il existe forcement un <body>)
            //ce body doit aussi être runat="server", il est alors un HtmlGenericControl 
            HtmlGenericControl htmlControl = PageTools.SearchBodyControl(pOwnerPage);
            if (null == htmlControl)
                throw new NotSupportedException("This page is missing a body runat server. Please add <body runat=\"server\" />.");
            //
            if (pIsModal)
                htmlControl.Attributes.Add("onBlur", "self.focus();");
            //
            if (StrFunc.IsFilled(pUrlOnBeforeUnload))
                htmlControl.Attributes.Add("onbeforeunload", pUrlOnBeforeUnload);
            //
            htmlControl.Attributes.Add("onkeydown", "CheckKey(event);");
            htmlControl.Attributes.Add("onclick", "ClickOnNode(event);");
            //
            htmlControl.ID = "BodyID";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pOwnerPage"></param>
        /// <param name="pForm"></param>
        /// <param name="pHeadTitle"></param>
        /// <param name="pCss"></param>
        /// <param name="pIsModal"></param>
        /// <param name="pUrlOnBeforeUnload"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void BuildPage(PageBase pOwnerPage, HtmlForm pForm, string pHeadTitle,
                                            string pCssMode, bool pIsModal, string pUrlOnBeforeUnload)
        {
            BuildPage(pOwnerPage, pForm, pHeadTitle, pCssMode, pIsModal, pUrlOnBeforeUnload, null);
        }
        /// <summary>
        /// Building Page with control server (LiteralControl not used)
        /// </summary>
        /// <param name="pOwnerPage"></param>
        /// <param name="pForm"></param>
        /// <param name="pHeadTitle">Titre de la page HTML</param>
        /// <param name="pTitleLeft"></param>
        /// <param name="pTitleRight"></param>
        /// <param name="pLinkCss"></param>
        /// <param name="pHelpUrl"></param>
        /// <param name="pIsModal"></param>
        /// <param name="pUrlOnBeforeUnload"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200903 [XXXXX] Passage pIdMenu à SetHead
        public static void BuildPage(PageBase pOwnerPage, HtmlForm pForm, string pHeadTitle,
                                            string pCssMode, bool pIsModal, string pUrlOnBeforeUnload, string pIdMenu)
        {

            if (null != pOwnerPage.Header)
            {
                //Il existe dejà un <head runat="server"> déclaré sur la page et un <body runat="server"> et forcement un html
                //si la page contient un CalendarExtender alors le Header est nécessairement présent (nécessaire au bon fonctionnement de cet extender)
                //Head
                PageTools.SetHead(pOwnerPage, pHeadTitle, pCssMode, pIdMenu);
                //Body
                PageTools.SetBody(pOwnerPage, pIsModal, pUrlOnBeforeUnload);
                //Add Form on body
                HtmlGenericControl body = PageTools.SearchBodyControl(pOwnerPage);
                body.Controls.Add(pForm);
            }
            else
            {
                //Creation à partir d'un page totalement vierge
                //pOwnerPage.Controls.Add(new LiteralControl("<?xml version=\"1.0\" encoding=\"UTF-8\"?>"));
                pOwnerPage.Controls.Add(new LiteralControl("<!DOCTYPE html>"));
                pOwnerPage.Controls.Add(new HtmlGenericControl("html"));
                HtmlGenericControl html = PageTools.SearchHtmlControl(pOwnerPage);
                html.Attributes.Add("xmlns", "http://www.w3.org/1999/xhtml");

                html.Controls.Add(new HtmlGenericControl("head"));
                PageTools.SetHead(pOwnerPage, pHeadTitle, pCssMode, pIdMenu);
                html.Controls.Add(new HtmlGenericControl("body"));
                PageTools.SetBody(pOwnerPage, pIsModal, pUrlOnBeforeUnload);
                //Add Form on body
                HtmlGenericControl body = PageTools.SearchBodyControl(pOwnerPage);
                body.Controls.Add(pForm);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pLinkId"></param>
        /// <param name="pRel"></param>
        /// <param name="pType"></param>
        /// <param name="pUrlHref"></param>
        public static void SetHeaderLink(Control pHeader, string pLinkId, string pRel, string pType, string pUrlHref)
        {
            HtmlLink lnk = (HtmlLink)pHeader.FindControl(pLinkId);
            if (null != lnk)
                lnk.Href = pUrlHref;
            else
                AddHeaderLink(pHeader, pLinkId, pRel, pType, pUrlHref);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pLinkId"></param>
        /// <param name="pUrlHref"></param>
        public static void SetHeaderLink(Control pHeader, string pLinkId, string pUrlHref)
        {
            HtmlLink lnk = (HtmlLink)pHeader.FindControl(pLinkId);
            if (null != lnk)
                lnk.Href = pUrlHref;
            else
                AddHeaderLink(pHeader, pLinkId, pUrlHref);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pTitleId"></param>
        /// <param name="pTitle"></param>
        public static void SetHeaderTitle(Control pHeader, string pTitleId, string pTitle)
        {
            if (pHeader.FindControl(pTitleId) is HtmlTitle title)
                title.Text = pTitle;
            else
                AddHeaderTitle(pHeader, pTitleId, pTitle);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pName"></param>
        public static void SetMetaTag(Control pHeader)
        {
            HtmlMeta meta = (HtmlMeta)pHeader.FindControl("metaText");
            if (null == meta)
            {
                meta = new HtmlMeta();
                pHeader.Controls.Add(meta);
            }
            meta.Content = "text/html; charset=UTF-8";
            meta.HttpEquiv = "Content-Type";
            meta.Name = "metaText";
            //
            meta = (HtmlMeta)pHeader.FindControl("metaScript");
            if (null == meta)
            {
                meta = new HtmlMeta();
                pHeader.Controls.Add(meta);
            }
            meta.Content = "text/javascript";
            meta.HttpEquiv = "Content-Script-Type";
            meta.Name = "metaScript";
            //
            meta = (HtmlMeta)pHeader.FindControl("metaStyle");
            if (null == meta)
            {
                meta = new HtmlMeta();
                pHeader.Controls.Add(meta);
            }
            meta.Content = "text/css";
            meta.HttpEquiv = "Content-Style-Type";
            meta.Name = "metaStyle";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pCompatibility"></param>
        public static void SetMetaTagIECompatibility(Control pHeader, string pCompatibility)
        {
            HtmlMeta meta = (HtmlMeta)pHeader.FindControl("metaIECompatibility");
            if (null == meta)
            {
                meta = new HtmlMeta();
                pHeader.Controls.Add(meta);
            }
            meta.Content = "IE=" + pCompatibility;
            meta.HttpEquiv = "X-UA-Compatible";
            meta.Name = "metaIECompatibility";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pLinkId"></param>
        /// <param name="pUrlHref"></param>
        public static void AddHeaderLink(Control pHeader, string pLinkId, string pUrlHref)
        {
            AddHeaderLink(pHeader, pLinkId, "stylesheet", "text/css", pUrlHref);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pLinkId"></param>
        /// <param name="pRel"></param>
        /// <param name="pType"></param>
        /// <param name="pUrlHref"></param>
        public static void AddHeaderLink(Control pHeader, string pLinkId, string pRel, string pType, string pUrlHref)
        {
            HtmlLink lnk = new HtmlLink
            {
                ID = pLinkId,
                Href = pUrlHref
            };
            lnk.Attributes.Add("rel", pRel);
            lnk.Attributes.Add("type", pType);
            pHeader.Controls.Add(lnk);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pHeader"></param>
        /// <param name="pTitleId"></param>
        /// <param name="pTitle"></param>
        /// EG 20140702 Refactoring
        public static void AddHeaderTitle(Control pHeader, string pTitleId, string pTitle)
        {
            //HtmlTitle title = new HtmlTitle();
            //title.ID = pTitleId;
            //title.Text = pTitle;
            //try
            //{
            //    if (false == pHeader.Controls.Contains(title))
            //        pHeader.Controls.Add(title);
            //}
            //catch { pHeader.Page.Title = pTitle; }
            bool _isAddTitle = true;
            foreach (Control _control in pHeader.Controls)
            {
                if (_control is HtmlTitle)
                {
                    HtmlTitle title = _control as HtmlTitle;
                    title.Text = pTitle;
                    title.ID = pTitleId;
                    _isAddTitle = false;
                    break;
                }
            }
            if (_isAddTitle)
            {
                HtmlTitle title = new HtmlTitle
                {
                    ID = pTitleId,
                    Text = pTitle
                };
                try
                {
                    if (false == pHeader.Controls.Contains(title))
                        pHeader.Controls.Add(title);
                }
                catch { pHeader.Page.Title = pTitle; }
            }
        }


   
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class FormTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pForm"></param>
        /// <param name="pTitleLeft"></param>
        /// <param name="pTitleRight"></param>
        /// <param name="pHelpUrl"></param>
        /// <param name="pIdMenu"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200903 [XXXXX] Upd Gestion pMainMenuName
        public static void AddBanniere(PageBase pPage, HtmlForm pForm, HtmlPageTitle pTitleLeft, HtmlPageTitle pTitleRight, string pIdMenu)
        {
            AddBanniere(pPage, pForm, pTitleLeft, pTitleRight, pIdMenu, null);
        }
        // EG 20200903 [XXXXX] Upd Gestion pMainMenuName
        public static void AddBanniere(PageBase pPage, HtmlForm pForm, HtmlPageTitle pTitleLeft, HtmlPageTitle pTitleRight, string pIdMenu, string pMainMenuName)
        {
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(pPage, pTitleLeft, pTitleRight, pIdMenu, pMainMenuName));
            pForm.Controls.AddAt(0, pnlHeader);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class TableTools
    {
        #region CellSetWidth
        public static void CellSetWidth(TableCell opTd, string pWidth)
        {
            if (StrFunc.IsFilled(pWidth))
            {
                if (opTd == null)
                    opTd = new TableCell();
                pWidth = pWidth.ToLower();
                bool isPercentage = (pWidth.IndexOf("%") > 1);
                bool isPixel = (pWidth.IndexOf("px") > 1);
                pWidth = pWidth.Replace("%", string.Empty);
                pWidth = pWidth.Replace("px", string.Empty);
                pWidth = pWidth.Replace("width=", string.Empty);
                pWidth = pWidth.Replace(@"""", string.Empty);
                if (isPercentage)
                    opTd.Width = Unit.Percentage(Convert.ToDouble(pWidth.Trim()));
                else if (isPixel)
                    opTd.Width = Unit.Pixel(Convert.ToInt32(pWidth.Trim()));
                else
                    opTd.Width = Unit.Pixel(Convert.ToInt32(pWidth.Trim()));
            }
        }
        #endregion
        #region AddCell
        public static void AddCellDot(TableRow opTr, int pWidth)
        {
            TableCell td = AddCell(".", HorizontalAlign.NotSet, pWidth, UnitEnum.Pixel, false, false, false);
            td.ForeColor = Color.White;
            opTr.Cells.Add(td);
        }
        public static void AddCellDot(TableRow opTr, string pWidth)
        {
            TableCell td = AddCell(".", HorizontalAlign.NotSet, 0, UnitEnum.Pixel, false, false, false);
            td.ForeColor = Color.White;
            if (StrFunc.IsFilled(pWidth))
            {
                pWidth = pWidth.ToLower();
                bool isPercentage = (pWidth.IndexOf("%") > 1);
                bool isPixel = (pWidth.IndexOf("px") > 1);
                pWidth = pWidth.Replace("%", string.Empty);
                pWidth = pWidth.Replace("px", string.Empty);
                pWidth = pWidth.Replace("width=", string.Empty);
                pWidth = pWidth.Replace(@"""", string.Empty);
                if (isPercentage)
                    td.Width = Unit.Percentage(Convert.ToDouble(pWidth.Trim()));
                else if (isPixel)
                    td.Width = Unit.Pixel(Convert.ToInt32(pWidth.Trim()));
                else
                    td.Width = Unit.Pixel(Convert.ToInt32(pWidth.Trim()));
            }
            opTr.Cells.Add(td);
        }
        public static void AddCellGradientLeft(TableRow opTr)
        {
            AddCellGradientLeft(opTr, 1);
        }
        public static void AddCellGradientLeft(TableRow opTr, int pRowSpan)
        {
            TableCell td = new TableCell
            {
                Width = Unit.Percentage(50),
                Text = Cst.HTMLSpace,
                CssClass = "Header_GradientLeft"
            };
            if (pRowSpan > 1)
                td.RowSpan = pRowSpan;
            opTr.Cells.Add(td);
        }
        public static void AddCellGradientRight(TableRow opTr)
        {
            AddCellGradientRight(opTr, 1);
        }
        public static void AddCellGradientRight(TableRow opTr, int pRowSpan)
        {
            TableCell td = new TableCell
            {
                Width = Unit.Percentage(50),
                Text = Cst.HTMLSpace,
                CssClass = "Header_GradientRight"
            };
            if (pRowSpan > 1)
                td.RowSpan = pRowSpan;
            opTr.Cells.Add(td);
        }
        public static void AddCellSpace(TableRow opTr)
        {
            AddCellSpace(opTr, 1);
        }
        public static void AddCellSpace(TableRow opTr, int pRowSpan)
        {
            //opTr.Cells.Add( AddCell(Cst.HTMLSpace, HorizontalAlign.Left, 1, UnitEnum.Percentage, true, false, false) );
            //20090102 PL Utilisation de 1Px à la place de 1% (pour le référentiel)
            TableCell td = AddCell(Cst.HTMLSpace, HorizontalAlign.Left, 1, UnitEnum.Pixel, true, false, false);
            if (pRowSpan > 1)
                td.RowSpan = pRowSpan;
            opTr.Cells.Add(td);
        }
        public static void AddCellFullSpace(TableRow opTr)
        {
            opTr.Cells.Add(AddCell(Cst.HTMLSpace, HorizontalAlign.Left, 100, UnitEnum.Percentage, true, false, false));
        }
        public static TableCell AddCell(string pValue, bool pIsLabel)
        {
            return AddCell(pValue, HorizontalAlign.NotSet, 0, UnitEnum.Pixel, false, false, pIsLabel);
        }
        public static TableCell AddCell(string pValue, HorizontalAlign pHorizontalAlign)
        {
            return AddCell(pValue, pHorizontalAlign, 0, UnitEnum.Pixel, false);
        }
        public static TableCell AddCell(string pValue, HorizontalAlign pHorizontalAlign, bool pIsToolTip)
        {
            return AddCell(pValue, pHorizontalAlign, 0, UnitEnum.Pixel, pIsToolTip);
        }

        public static TableCell AddCell(string pValue, HorizontalAlign pHorizontalAlign, int pWidth, UnitEnum pUnitEnum)
        {
            return AddCell(pValue, pHorizontalAlign, pWidth, pUnitEnum, false, false, false);
        }

        public static TableCell AddCell(string pValue, HorizontalAlign pHorizontalAlign, int pWidth, UnitEnum pUnitEnum, bool pIsToolTip)
        {
            return AddCell(pValue, pHorizontalAlign, pWidth, pUnitEnum, false, pIsToolTip, false);
        }
        /// <summary>Return a new cell</summary>
        /// <param name="pValue">the contents (text) of the cell</param>
        /// <param name="pHorizontalAlign">horizontal alignment of the contents of the cell</param>
        /// <param name="pWidth">width of the cell</param>
        /// <param name="pIsWrap">the contents of the cell are wrapped in the cell (true/false)</param>
        /// <param name="pIsToolTip">toolTip to cell equal to Ressource.GetString(pValue)</param> 
        /// <returns></returns>
        public static TableCell AddCell(string pValue, HorizontalAlign pHorizontalAlign, int pWidth, UnitEnum pUnitEnum, bool pIsWrap, bool pIsToolTip, bool pIsLabel)
        {
            TableCell td = new TableCell
            {
                Text = pValue,
                HorizontalAlign = pHorizontalAlign,
                Wrap = pIsWrap
            };
            //
            if (pIsLabel)
                td.Style.Add(HtmlTextWriterStyle.Color, "black");
            //
            if (pIsToolTip)
            {
                td.ToolTip = Ressource.GetString(pValue);
                td.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            }
            SetCellWith(td, pWidth, pUnitEnum);

            return td;
        }
        #endregion AddCell

        #region public WriteSpaceInCell
        public static void WriteSpaceInCell(TableCell pCell)
        {
            pCell.Controls.Add(new LiteralControl(Cst.HTMLSpace));
        }
        #endregion

        #region public AddHorizontalRule
        public static TableRow AddHorizontalRule(int pSize)
        {
            return AddHorizontalRule(pSize, 1, 1);
        }
        public static TableRow AddHorizontalRule(int pSize, int pHeight)
        {
            return AddHorizontalRule(pSize, pHeight, 1);
        }
        public static TableRow AddHorizontalRule(int pSize, int pHeight, int pColSpan)
        {
            TableRow tr = new TableRow();
            TableCell td = new TableCell();
            if (1 < pColSpan)
                td.ColumnSpan = pColSpan;
            td.Height = Unit.Point(pHeight);
            td.Controls.Add(new LiteralControl(@"<HR size='" + pSize.ToString() + "'/>"));
            tr.Cells.Add(td);
            return tr;
        }
        #endregion AddHorizontalRule

        #region public AddHeaderCell
        public static TableHeaderCell AddHeaderCell(string pRessource, int pWidth, UnitEnum pUnitEnum, int pColumnSpan)
        {
            return AddHeaderCell(pRessource, true, pWidth, pUnitEnum, pColumnSpan, false);
        }
        public static TableHeaderCell AddHeaderCell(string pLabel, bool pIsRessource, int pWidth, UnitEnum pUnitEnum, int pColumnSpan, bool pIsWrap)
        {
            TableHeaderCell th = new TableHeaderCell();
            //
            if (pIsRessource)
                th.Text = Ressource.GetString(pLabel);
            else
                th.Text = pLabel;
            //
            SetCellWith((TableCell)th, pWidth, pUnitEnum);
            //
            if (0 < pColumnSpan)
                th.ColumnSpan = pColumnSpan;
            //
            th.Wrap = pIsWrap;
            //
            return th;
        }
        #endregion AddHeaderCell

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrentTable"></param>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void RemoveStyleTableHeader(Table pCurrentTable)
        {
            pCurrentTable.Style.Remove("istableH");
            pCurrentTable.Style.Remove("tblH-bkgH-color");
            pCurrentTable.Style.Remove("tblH-bkg-color");
            pCurrentTable.Style.Remove("tblH-border-color");
            pCurrentTable.Style.Remove("tblH-title");
            pCurrentTable.Style.Remove("tblH-titleR");
            pCurrentTable.Style.Remove("tblH-titleF");
            pCurrentTable.Style.Remove("tblH-headerlinkid");
            pCurrentTable.Style.Remove("tblH-linkid");
            pCurrentTable.Style.Remove("tblH-startdisplay");
            pCurrentTable.Style.Remove("tblH-btn-img");
            pCurrentTable.Style.Remove("tblH-btn-id");
            pCurrentTable.Style.Remove("tblH-btn-key");
            pCurrentTable.Style.Remove("tblH-btn-click");
            pCurrentTable.Style.Remove("tblH-btn-blockUI");
            pCurrentTable.Style.Remove("helpschema");
            pCurrentTable.Style.Remove("helpelement");
            pCurrentTable.Style.Remove("tblH-square");
            pCurrentTable.Style.Remove("tblH-reverse");
            pCurrentTable.Style.Remove("tblH-mainclass");
            pCurrentTable.Style.Remove("tblH-css");
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void RemoveStyleTableBody(Table pCurrentTable)
        {
            pCurrentTable.Style.Remove("istableB");
            pCurrentTable.Style.Remove("tblB-bkg-color");
            pCurrentTable.Style.Remove("tblB-border-color");
            pCurrentTable.Style.Remove("tblB-mainclass");
        }

        #region private SetCellWith
        private static void SetCellWith(TableCell Cell, int pWidth, UnitEnum pUnitEnum)
        {
            if ((pWidth) > 0)
            {
                switch (pUnitEnum)
                {
                    case UnitEnum.Percentage:
                        Cell.Width = Unit.Percentage(Math.Abs(pWidth));
                        break;
                    case UnitEnum.Pixel:
                        Cell.Width = Unit.Pixel(pWidth);
                        break;
                }
            }
        }
        #endregion SetCellWith
    }

    /// <summary>
    /// 
    /// </summary>
    public class HtmlPageTitle
    {
        /// <summary>
        /// 
        /// </summary>
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string TitleTooltip
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string SubTitle
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Footer
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public Color Color
        {
            get;
            set;
        }
        public string Id
        {
            get;
            set;
        }

        // [25556] New
        public string FullRightTitle
        {
            get
            {
                string ret;
                if (String.IsNullOrEmpty(Title))
                    ret = SubTitle;
                else if (String.IsNullOrEmpty(SubTitle))
                    ret = Title;
                else
                    ret = Title + "/" + SubTitle;

                return ret;
            }
        }

        #region Constructors
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public HtmlPageTitle() : this(null, null, null, Color.Transparent, null) { }
        public HtmlPageTitle(string pTitle) : this(pTitle, null, null, Color.Transparent, null) { }
        public HtmlPageTitle(string pTitle, string pSubTitle) : this(pTitle, pSubTitle, null, Color.Transparent, null) { }
        public HtmlPageTitle(string pTitle, string pSubTitle, string pFooter)
            : this(pTitle, pSubTitle,  pFooter, Color.Transparent, null) { }
        public HtmlPageTitle(string pTitle, string pSubTitle, string pFooter, Color pColor, string pId)
        {
            Title = pTitle;
            SubTitle = pSubTitle;
            Footer = pFooter;
            Color = pColor;
            Id = pId;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Ouverture d'un référentiel ou d'une consultation ou d'un invoicing en mode selection
    /// </summary>
    /// EG 20120620 invoicing attribute New
    [XmlRoot(ElementName = "OpenReferentialArguments", IsNullable = true)]
    public class OpenReferentialArguments
    {
        #region members
        /// <summary>
        ///
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string referential;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string invoicing;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string consultation;
        /// <summary>
        /// Valeurs possibles : Cst.KeyField ou Cst.DataKeyField 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string type_KeyField;
        /// <summary>
        /// ID du control associé au KeyField (ex:IDENTIFIER) ou au DataKeyField (ex:IDA)
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string clientIdForKeyField;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sqlColumnSpecified;
        /// <summary>
        /// colonne associée au contrôle {clientIdForSqlColumn}
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string sqlColumn;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool clientIdForSqlColumnSpecified;
        /// <summary>
        /// ID du control associé à {sqlColumn} 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string clientIdForSqlColumn;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool conditionSpecified;
        /// <summary>
        /// Condition particulière pour ouverture du referentiel
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string condition;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fkSpecified;
        /// <summary>
        /// FK pour ouverture du référentiel
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string fk;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool paramSpecified;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] param;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isFilteredWithSqlColumnOrKeyFieldSpecified;
        /// <summary>
        /// si true à l'ouverture du grid Spheres® applique un flitre (lecture du controle clientIdForSqlColumn ou clientIdForKeyField)
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isFilteredWithSqlColumnOrKeyField;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool titleSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string title;
        
        [System.Xml.Serialization.XmlTextAttribute()]
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isHideInCriteriaSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isHideInCriteria;
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DynamicArgumentSpecified;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] DynamicArgument;

        [System.Xml.Serialization.XmlTextAttribute()]
        public Cst.ListType listType;
        #endregion

        #region constructor
        public OpenReferentialArguments()
        {
            listType = Cst.ListType.Repository;
            // FI 20180502 [23926] add isFilteredWithSqlColumnOrKeyFieldSpecified = true
            isFilteredWithSqlColumnOrKeyFieldSpecified = true;
            isFilteredWithSqlColumnOrKeyField = true;
            type_KeyField = Cst.KeyField;
        }
        #endregion constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// EG 20120620 invoicing attribute New
        public void SetAttributeOnCLickButtonReferential(WebControl pControl)
        {

            if (null != pControl)
            {
                string newReferential = referential;
                // EG 20120619
                if ((StrFunc.IsEmpty(referential) || referential.Equals("undefined")) &&
                    (StrFunc.IsEmpty(consultation) || (consultation.Equals("undefined"))) &&
                    (StrFunc.IsFilled(invoicing) && (!invoicing.Equals("undefined"))))
                {
                    newReferential = invoicing;
                    listType = Cst.ListType.Invoicing;
                }
                else if (((StrFunc.IsEmpty(referential) || (referential.Equals("undefined"))) &&
                        ((StrFunc.IsEmpty(invoicing) || (invoicing.Equals("undefined"))))) &&
                        (StrFunc.IsFilled(consultation) && (!consultation.Equals("undefined"))))
                {
                    newReferential = consultation;
                    listType = Cst.ListType.Consultation;
                }
                
                string paramDynamicArg = string.Empty;
                if (ArrFunc.IsFilled(DynamicArgument))
                {
                    paramDynamicArg = StrFunc.StringArrayList.StringArrayToStringList(DynamicArgument);
                }
                //PL 20160919 / PL 20160914 (suite)
                ControlsTools.SetOnClientClick_ButtonReferential(pControl, newReferential, title, listType,
                    isFilteredWithSqlColumnOrKeyField,
                    type_KeyField, clientIdForKeyField,
                    sqlColumn, clientIdForSqlColumn,
                    condition, fk,
                    ArrFunc.IsFilled(param) ? ArrFunc.GetStringList(param, ";") : null,
                    StrFunc.IsFilled(paramDynamicArg) ? Ressource.EncodeDA(paramDynamicArg) : null);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot(ElementName = "OpenMenuArguments", IsNullable = true)]
    // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre
    public class OpenMenuArguments
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        public string menu;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool titleSpecified;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string title;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string pk;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string pkv;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string fkv;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string guid;
        #endregion Members

        #region Accessors
        #endregion Accessors


        #region Constructors
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre
        public OpenMenuArguments()
        {
            title = string.Empty;
            menu = string.Empty;
            pk = string.Empty;
            pkv = string.Empty;
            fkv = string.Empty;
            guid = string.Empty;
        }
        #endregion Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID
        public void SetAttributeOnClickOnButtonMenu(WebControl pControl)
        {
            if (null != pControl)
                ControlsTools.SetAttributeOnClickOnButtonMenu(pControl, menu, pk, pkv, fkv, guid);

        }

    }

    /// <summary>
    /// Représente les paramètres qui ouvrent la page Referential.aspx
    /// </summary>
    public class OpenReferentialFormArguments
    {
        #region members
        /// <summary>
        ///  type de référentiel (Referential,Cconsultation,Log,etc...)
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// Nom du référentiel
        /// </summary>
        public string @Object
        {
            get;
            set;
        }

        /// <summary>
        /// Mode de consultation/edition
        /// </summary>
        public Cst.ConsultationMode Mode
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit l'indicateur qui indique si ouverture pour nouvel enregistrement
        /// </summary>
        public bool IsNewRecord
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string CondApp
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] Param
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] DynamicArg
        {
            get;
            set;
        }

        /// <summary>
        /// Représente le formulaire appelant
        /// </summary>
        public string FormId
        {
            get;
            set;
        }

        /// <summary>
        /// Représente l'identifiant du datasetCourant (permet la navigation vers les autres lignes à partir du formulaire)
        /// </summary>
        public string DS
        {
            get;
            set;
        }

        /// <summary>
        /// Représente la colonne PK
        /// </summary>
        public string PK
        {
            get;
            set;
        }

        /// <summary>
        /// Repésente la valeur de la PK
        /// </summary>
        public string PKValue
        {
            get;
            set;
        }

        /// <summary>
        /// Représente la colonne FK (utilisé pour les référentiels enfants)
        /// </summary>
        public string FK
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string FKValue
        {
            get;
            set;
        }

        /// <summary>
        /// Représente la colonne FK (utilisé pour les référentiels enfants)
        /// </summary>
        public string IdMenu
        {
            get;
            set;
        }

        /// <summary>
        /// Représente le menu associé au référentiel
        /// </summary>
        public string TitleMenu
        {
            get;
            set;
        }

        /// <summary>
        /// Représente le nom de l afonctionnalitémenu associé au référentiel
        /// </summary>
        public string TitleRes
        {
            get;
            set;
        }
        #endregion


        #region constructor
        public OpenReferentialFormArguments()
        {

        }
        #endregion constructor

        #region Methods
        public string GetURLOpenFormRepository()
        {
            return GetURLOpenFormReferential().Replace("Referential.aspx", "Repository.aspx");
        }
        /// <summary>
        /// Obtient l'URL destinée à l'ouverture du formulaire.
        /// </summary>
        /// <returns></returns>
        public string GetURLOpenFormReferential()
        {
            string ret = "Referential.aspx";
            //
            ret += StrFunc.AppendFormat("?T={0}", Type);
            ret += StrFunc.AppendFormat("&O={0}", @Object);
            ret += StrFunc.AppendFormat("&M={0}", Convert.ToInt32(Mode).ToString());
            ret += StrFunc.AppendFormat("&N={0}", IsNewRecord ? "1" : "0");
            //
            if (StrFunc.IsFilled(CondApp))
                ret += StrFunc.AppendFormat("&CondApp={0}", CondApp);
            //   
            if (ArrFunc.IsFilled(Param))
            {
                for (int i = 0; i < Param.Length; i++)
                {
                    ret += "&P" + (i + 1).ToString() + "=" + HttpUtility.UrlEncode(Param[i]);
                }
            }
            //
            if (ArrFunc.IsFilled(DynamicArg))
            {
                ret += "&DA=";
                for (int i = 0; i < DynamicArg.Length; i++)
                {
                    //PL 20160914
                    ret += HttpUtility.UrlEncode(Ressource.EncodeDA(DynamicArg[i])) + StrFunc.StringArrayList.LIST_SEPARATOR;
                }
            }
            //
            if (StrFunc.IsFilled(DS))
                ret += "&DS=" + HttpUtility.UrlEncode(DS);
            //
            if (StrFunc.IsFilled(FormId))
                ret += "&F=" + HttpUtility.UrlEncode(FormId);
            //
            if (StrFunc.IsFilled(PK))
                ret += "&PK=" + HttpUtility.UrlEncode(PK);
            //
            if (StrFunc.IsFilled(PKValue))
                ret += "&PKV=" + HttpUtility.UrlEncode(PKValue);
            //
            if (StrFunc.IsFilled(FK))
                ret += "&FK=" + HttpUtility.UrlEncode(FK);
            //
            if (StrFunc.IsFilled(FKValue))
                ret += "&FKV=" + HttpUtility.UrlEncode(FKValue);
            //
            if (StrFunc.IsFilled(IdMenu))
                ret += "&IDMenu=" + HttpUtility.UrlEncode(IdMenu);
            //
            if (StrFunc.IsFilled(TitleMenu))
                ret += "&TitleMenu=" + HttpUtility.UrlEncode(TitleMenu);
            //
            if (StrFunc.IsFilled(TitleRes))
                ret += "&TitleRes=" + HttpUtility.UrlEncode(TitleRes);
            //
            //ADDCLIENTGLOP Test in progress...
            //PL 20130416 New features
            //if ((@object == Cst.OTCml_TBL.ACTOR.ToString()) && @IsnewRecord)
            //{
            //    ret += "&PK=" + "IDA";
            //    ret += "&PKV=" + "11915";
            //    ret += "&DPC=1"; //DPC:DuPliCate
            //}
            return ret;

        }
        /// <summary>
        ///  Retourne le référentiel approprié pour ouvrir la donnée {pPKValue} strockée dans la table physique {pTable}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTable">Représente la table physique de stockage</param>
        /// <param name="pPKValue">Représente la valeur de la Pk</param>
        /// <param name="opObject">Retourn le nom de l'objet</param>
        /// <param name="opIdMenu">Retourne le menu</param>
        /// FI 20170928 [23452] Modify
        public static void GetObject(string pCS, Cst.OTCml_TBL pTable, string pPKValue, out string opObject, out Nullable<IdMenu.Menu> opIdMenu)
        {
            opObject = null;
            opIdMenu = null;

            switch (pTable)
            {
                case Cst.OTCml_TBL.CNFMESSAGE:
                    SQL_ConfirmationMessage sqlCnfMessage = new SQL_ConfirmationMessage(pCS, IntFunc.IntValue(pPKValue));
                    if (sqlCnfMessage.LoadTable(new string[] { "MSGTYPE" }))
                    {
                        if (sqlCnfMessage.MsgType == "MONO-TRADE")
                        {
                            opObject = "CNFMESSAGE";
                            opIdMenu = EFS.ACommon.IdMenu.Menu.CNFMessage;
                        }
                        else if ((sqlCnfMessage.MsgType == "MULTI-TRADES") ||
                                  (sqlCnfMessage.MsgType == "MULTI-PARTIES"))
                        {
                            opObject = "TS_CNFMESSAGE";
                            opIdMenu = EFS.ACommon.IdMenu.Menu.TSMessage;
                        }
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("Message [type:{0}] is not impemented", sqlCnfMessage.MsgType));
                    }
                    break;
                case Cst.OTCml_TBL.ACTOR: // FI 20170928 [23452] Add
                    // Si ACTOR => Ouverture éventuelle du référentiel ALGORITHM
                    opObject = "ACTOR";
                    opIdMenu = EFS.ACommon.IdMenu.Menu.Actor;

                    SQL_Actor sqlActor = new SQL_Actor(pCS, IntFunc.IntValue(pPKValue));
                    if (sqlActor.LoadTable(new string[] { "ALGOTYPE" }))
                    {
                        if (StrFunc.IsFilled(sqlActor.AlgorithmType))
                        {
                            opObject = "ALGORITHM";
                            opIdMenu = EFS.ACommon.IdMenu.Menu.Algorithm;
                        }
                    }
                    break;
                case Cst.OTCml_TBL.INSTRUMENT: // FI 20240703 [WI989] Add
                    opObject = "INSTRUMENT";

                    DataInstrument dataInstrumentItem = DataInstrumentEnabledHelper.GetDataInstrument(SessionTools.CS, null, IntFunc.IntValue(pPKValue));
                    if (null == dataInstrumentItem)
                        throw new NullReferenceException($"No Instrument for IDI:{pPKValue}");

                    if (dataInstrumentItem.IsProductClassStrategy)
                        opIdMenu = EFS.ACommon.IdMenu.Menu.Strategy;
                    else if (dataInstrumentItem.IsProductClassRegular)
                        opIdMenu = EFS.ACommon.IdMenu.Menu.Instrument;
                    else
                        throw new NotImplementedException($"{dataInstrumentItem.Class} is not implemented");


                    break;
                default:
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProgressIndicator : ITemplate
    {
        #region Members
        readonly string _message;
        string _class;
        string _style;
        #endregion Members

        #region constructor
        public ProgressIndicator(string pMgs)
        {
            _message = pMgs;
        }
        #endregion
        #region public SetDivStyle
        public void SetDivStyle(string pStyle)
        {
            _style = pStyle;
        }
        #endregion
        #region public SetDivClass
        public void SetDivClass(string pClass)
        {
            _class = pClass;
        }
        #endregion

        #region ITemplate
        public void InstantiateIn(Control pContainer)
        {
            System.Web.UI.WebControls.Image img = new System.Web.UI.WebControls.Image
            {
                ImageUrl = "~/images/ajax-loader.gif",
                AlternateText = "Loading"
            };

            string attribute = StrFunc.IsFilled(_class) ? "class='" + _class + "'" : "style='" + _style + "'";

            pContainer.Controls.Add(new LiteralControl("<div " + attribute + ">"));
            pContainer.Controls.Add(img);
            pContainer.Controls.Add(new LiteralControl(_message));
            pContainer.Controls.Add(new LiteralControl("</div>"));
        }
        #endregion
    }

}

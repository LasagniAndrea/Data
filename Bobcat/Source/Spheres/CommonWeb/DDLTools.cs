using EFS.ACommon;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    #region class DDLTools
    /// <summary>
    /// Functions for utilization with DropDownList
    /// </summary>
    // EG 20160404 Migration vs2013
    //#warning 20050414 PL (non urgent) Amener ici la section DropDownList de la classe ControlTools
    public sealed class DropDownTools
    {
        /// <summary>
        /// Ajoute des items spécifiques (NA,NONE,ALL, etc...) à un contrôle DropDownList ou HtmlSelect
        /// </summary>
        /// <param name="pddl">Représente le contrôle</param>
        /// <param name="pMisc">items à ajouter (exemple isAddAll:true;)</param>
        /// FI 20121106 [] gestion du isAddUnmathALL (GLOP a revoir)
        public static void DDLAddMiscItem(Control pDdl, string pMisc)
        {
            GetDDL(pDdl, out DropDownList dropDownList, out HtmlSelect htmlSelect, out _);

            bool isDropDownList = (dropDownList != null);
            bool isHtmlSelect = (htmlSelect != null);

            #region Insert Misc (All, Inherited, NA, None)
            if (StrFunc.IsFilled(pMisc) && (isDropDownList || isHtmlSelect))
            {
                CustomObject co = new CustomObject
                {
                    Misc = pMisc
                };
                bool isAddALL = co.GetMiscValue("isAddAll", false);
                bool isAddInherited = co.GetMiscValue("isAddInherited", false);
                bool isAddNA = co.GetMiscValue("isAddNA", false);
                bool isAddNone = co.GetMiscValue("isAddNone", false);
                bool isAddDefault = co.GetMiscValue("isAddDefault", false);
                bool isAddUnmatchALL = co.GetMiscValue("isAddUnmathALL", false);
                if (isDropDownList)
                {
                    if (isAddNA)
                        DDLInsertMiscElement(dropDownList, new ListItem(Ressource.GetString("DDL_NA"), Cst.DDLVALUE_NA));

                    if (isAddALL)
                    {
                        DDLInsertMiscElement(dropDownList, new ListItem(Ressource.GetString("DDL_All"), Cst.DDLVALUE_ALL));
                        //FI 20121106 add isAddUnmatchALL => codage en dur, DDL_All_Unmatch se trouve en index 2 (GLOP)
                        if (isAddUnmatchALL)
                            dropDownList.Items.Insert(2, new ListItem(Ressource.GetString("DDL_All_Unmatch"), Cst.DDLVALUE_ALL_UNMATCH));
                    }
                    if (isAddNone)
                        DDLInsertMiscElement(dropDownList, new ListItem(Ressource.GetString("DDL_None"), Cst.DDLVALUE_NONE));
                    if (isAddInherited)
                        DDLInsertMiscElement(dropDownList, new ListItem(Ressource.GetString("DDL_Inherited"), Cst.DDLVALUE_INHERITED));
                    if (isAddDefault)
                        DDLInsertMiscElement(dropDownList, new ListItem(Ressource.GetString("DDL_Default"), Cst.DDLVALUE_DEFAULT));
                }
                if (isHtmlSelect)
                {
                    if (isAddNA)
                        HtmlSelectInsertMiscElement(htmlSelect, new ListItem(Ressource.GetString("DDL_NA"), Cst.DDLVALUE_NA));
                    if (isAddALL)
                        HtmlSelectInsertMiscElement(htmlSelect, new ListItem(Ressource.GetString("DDL_All"), Cst.DDLVALUE_ALL));
                    if (isAddNone)
                        HtmlSelectInsertMiscElement(htmlSelect, new ListItem(Ressource.GetString("DDL_None"), Cst.DDLVALUE_NONE));
                    if (isAddInherited)
                        HtmlSelectInsertMiscElement(htmlSelect, new ListItem(Ressource.GetString("DDL_Inherited"), Cst.DDLVALUE_INHERITED));
                    if (isAddDefault)
                        HtmlSelectInsertMiscElement(htmlSelect, new ListItem(Ressource.GetString("DDL_Default"), Cst.DDLVALUE_DEFAULT));
                }
            }
            #endregion Insert Misc (All, Inherited, NA, None)

        }

        /// <summary>
        /// Ajoute dans la DDL un nouvel item 
        /// <para>L'item est mis en position 0 par défaut</para>
        /// <para>si le 1er item de la ddl est vide, l'item est mis en position 1</para>
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pItem"></param>
        private static void DDLInsertMiscElement(DropDownList pDdl, ListItem pMiscItem)
        {
            if (pDdl.Items.Count > 0)
            {
                if (StrFunc.IsEmpty(pDdl.Items[0].Value))
                    pDdl.Items.Insert(1, pMiscItem);
                else
                    pDdl.Items.Insert(0, pMiscItem);
            }
            else
            {
                pDdl.Items.Insert(0, pMiscItem);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pItem"></param>
        private static void HtmlSelectInsertMiscElement(HtmlSelect pDdl, ListItem pMiscItem)
        {
            if (pDdl.Items.Count > 0)
            {
                if (StrFunc.IsEmpty(pDdl.Items[0].Value))
                    pDdl.Items.Insert(1, pMiscItem);
                else
                    pDdl.Items.Insert(0, pMiscItem);
            }
            else
            {
                pDdl.Items.Insert(0, pMiscItem);
            }
        }


        /// <summary>
        /// Retourne true si Spheres® récupère une référence fortement typé (DropDownList, HtmlSelect,CheckBoxList) d'un control 
        /// </summary>
        /// <param name="pddl">Control source</param>
        /// <param name="pDropDownList"></param>
        /// <param name="pHtmlSelect"></param>
        /// <param name="pCheckBoxList"></param>
        /// <returns></returns>
        public static bool GetDDL(Control pDdl, out DropDownList pDropDownList, out HtmlSelect pHtmlSelect, out CheckBoxList pCheckBoxList)
        {
            pHtmlSelect = null;
            pCheckBoxList = null;

            pDropDownList = pDdl as DropDownList;
            bool ret = pDropDownList != null;

            if (false == ret)
            {
                pHtmlSelect = pDdl as HtmlSelect;
                ret = (pHtmlSelect != null);
            }

            if (false == ret)
            {
                pCheckBoxList = pDdl as CheckBoxList;
                ret = (pCheckBoxList != null);
            }
            return ret;

        }
    }
    #endregion class DDLTools
}
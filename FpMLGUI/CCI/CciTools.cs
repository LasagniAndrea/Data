#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Xml.Serialization;
using System.Drawing;
using System.Linq;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Web;

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Enum.Tools;
using EfsML.Enum;

using FpML.Enum;
#endregion Using Directives

namespace EFS.GUI.CCI
{
    /// <summary>
    /// 
    /// </summary>
    public class CciTools
    {
        /// <summary>
        /// <para>Retourne la property ou le membre "Ccis" ou "ccis" présent sur {pContainerCci} ou </para>
        /// <para>Retourne la property ou le membre "Ccis" ou "ccis" présent sur la classe de base de {pContainerCci}</para>
        /// <para>Retourne null si non trouvé</para>
        /// </summary>
        /// <param name="pContainerCci"></param>
        /// <returns></returns>
        /// FI 20161214 [21916] Modify
        public static CustomCaptureInfosBase Getccis(IContainerCci pContainerCci)
        {

            CustomCaptureInfosBase ret = null;
            Object ccis = null;
            Type tContainerCci = pContainerCci.GetType();

            MemberInfo[] mbrsCcis = tContainerCci.GetMember("Ccis");
            if (ArrFunc.IsEmpty(mbrsCcis))
            {
                mbrsCcis = tContainerCci.GetMember("ccis");
            }
            if (ArrFunc.IsEmpty(mbrsCcis) && (null != tContainerCci.BaseType))
            {
                tContainerCci = tContainerCci.BaseType;
                mbrsCcis = tContainerCci.GetMember("Ccis");
                if (ArrFunc.IsEmpty(mbrsCcis))
                    mbrsCcis = tContainerCci.GetMember("ccis");
            }
            if (ArrFunc.IsFilled(mbrsCcis))
            {
                if (mbrsCcis[0].MemberType == MemberTypes.Property)
                    ccis = tContainerCci.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetProperty, null, pContainerCci, null);
                else if (mbrsCcis[0].MemberType == MemberTypes.Field)
                    ccis = tContainerCci.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetField, null, pContainerCci, null);
            }

            if (null != ccis)
                ret = (CustomCaptureInfosBase)ccis;

            return ret;
        }


        #region public static SetCciContainer
        public static void SetCciContainer(IContainerCci pContainerCci, string pField, object pValue)
        {
            SetCciContainer(pContainerCci, "CciEnum", pField, pValue);
        }
        public static void SetCciContainer(IContainerCci pContainerCci, string pCciEnumsMethodName, string pField, object pValue)
        {
            SetCciContainer(pContainerCci, null, pCciEnumsMethodName, pField, pValue);
        }
        public static void SetCciContainer(IContainerCci pContainerCci, Type pClassOwnerType, string pCciEnumsMethodName, string pField, object pValue)
        {
            CustomCaptureInfosBase ccis = CciTools.Getccis(pContainerCci);
            if (null == ccis)
                throw new NullReferenceException(StrFunc.AppendFormat("ccis is not defined on object {0}", pContainerCci.GetType().ToString()));

            Type tContainerCci = pContainerCci.GetType();
            if (null != pClassOwnerType)
                tContainerCci = pClassOwnerType;

            // Get CciEnum
            MemberInfo[] mbrsCciEnums = tContainerCci.GetMember(pCciEnumsMethodName);
            if (ArrFunc.IsFilled(mbrsCciEnums))
            {
                FieldInfo[] fldCciEnums = ((Type)mbrsCciEnums[0]).GetFields();
                for (int i = 1; i < fldCciEnums.Length; i++)
                    ccis.Set(pContainerCci.CciClientId(fldCciEnums[i].Name), pField, pValue);
            }


        }
        #endregion
        #region public static CreateInstance
        public static void CreateInstance(IContainerCci pContainerCci, Object pObjectOwner)
        {
            CreateInstance(pContainerCci, pObjectOwner, "CciEnum");
        }
        public static void CreateInstance(IContainerCci pContainerCci, Object pObjectOwner, string pCciEnumsMethodName)
        {
            CreateInstance(pContainerCci, null, pObjectOwner, pCciEnumsMethodName);
        }
        public static void CreateInstance(IContainerCci pContainerCci, Type pClassOwnerType, Object pObjectOwner, string pCciEnumsMethodName)
        {
            CustomCaptureInfosBase ccis = CciTools.Getccis(pContainerCci);
            if (null == ccis)
                throw new NullReferenceException(StrFunc.AppendFormat("ccis is not defined on object {0}", pContainerCci.GetType().ToString()));

            Type tClassOwner = pContainerCci.GetType();
            if (null != pClassOwnerType)
                tClassOwner = pClassOwnerType;
            // Get CciEnum
            MemberInfo[] mbrsCciEnums = tClassOwner.GetMember(pCciEnumsMethodName);
            if ((null != mbrsCciEnums) && (0 < mbrsCciEnums.Length))
            {
                FieldInfo[] fldCciEnums = ((Type)mbrsCciEnums[0]).GetFields();
                foreach (FieldInfo fldEnum in fldCciEnums)
                {
                    object[] attributes = fldEnum.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                    if (0 < attributes.Length)
                    {
                        XmlEnumAttribute enumAttribute = (XmlEnumAttribute)attributes[0];

                        // Get CciClientId
                        Type[] typeArray = new Type[1];
                        typeArray.SetValue(typeof(string), 0);
                        MethodInfo method = tClassOwner.GetMethod("CciClientId", typeArray);
                        string cciClientIdValue = string.Empty;
                        if (null != method)
                        {
                            object[] argValues = new object[] { fldEnum.Name };
                            String[] argNames = new String[] { method.GetParameters()[0].Name };
                            cciClientIdValue = (string)tClassOwner.InvokeMember(method.Name, BindingFlags.InvokeMethod,
                                null, pContainerCci, argValues, null, null, argNames);
                        }

                        // Test if ccis contains CciClientId
                        if (ccis.Contains(cciClientIdValue))
                        {
                            String[] cciElts = enumAttribute.Name.Split(".".ToCharArray());
                            object obj = pObjectOwner;
                            foreach (string cciElt in cciElts)
                            {
                                if (null != obj)
                                {
                                    Type tObj = obj.GetType();
                                    FieldInfo fld = tObj.GetField(cciElt);
                                    if (null != fld)
                                    {
                                        object obj2 = fld.GetValue(obj);
                                        if ((null == obj2) && (false == fld.FieldType.IsArray))
                                        {
                                            obj2 = fld.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                                            fld.SetValue(obj, obj2);
                                        }
                                        obj = obj2;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion CreateInstance

        #region public NewCreateInstance
        // EG 20160404 Migration vs2013
        // #warning TO BE DEFINE and REPLACE CreateInstance
        public static void CreateInstance2(Object pClassOwner, Object pObjectOwner)
        {
            CreateInstance2(pClassOwner, pObjectOwner, "CciEnum");
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        public static void CreateInstance2(Object pClassOwner, Object pObjectOwner, string pCciEnumsMethodName)
        {

            #region Get ccis
            Type tClassOwner = pClassOwner.GetType();
            object ccis = null;
            MemberInfo[] mbrsCcis = tClassOwner.GetMember("ccis");
            if ((null != mbrsCcis) && (0 < mbrsCcis.Length))
            {
                if (mbrsCcis[0].MemberType == MemberTypes.Property)
                    ccis = tClassOwner.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetProperty, null, pClassOwner, null);
                else if (mbrsCcis[0].MemberType == MemberTypes.Field)
                    ccis = tClassOwner.InvokeMember(mbrsCcis[0].Name, BindingFlags.GetField, null, pClassOwner, null);
            }
            #endregion Get ccis

            if (null == ccis)
                throw new NullReferenceException(StrFunc.AppendFormat("ccis is not defined on object {0}", tClassOwner.ToString()));

            // Get CciEnum
            MemberInfo[] mbrsCciEnums = tClassOwner.GetMember(pCciEnumsMethodName);
            if ((null != mbrsCciEnums) && (0 < mbrsCciEnums.Length))
            {
                FieldInfo[] fldCciEnums = ((Type)mbrsCciEnums[0]).GetFields();
                foreach (FieldInfo fldEnum in fldCciEnums)
                {
                    object[] attributes = fldEnum.GetCustomAttributes(typeof(CciEnumInstance), true);
                    if (0 < attributes.Length)
                    {
                        // Get CciClientId
                        Type[] typeArray = new Type[1];
                        typeArray.SetValue(typeof(string), 0);
                        MethodInfo method = tClassOwner.GetMethod("CciClientId", typeArray);
                        string cciClientIdValue = string.Empty;
                        if (null != method)
                        {
                            object[] argValues = new object[] { fldEnum.Name };
                            String[] argNames = new String[] { method.GetParameters()[0].Name };
                            cciClientIdValue = (string)tClassOwner.InvokeMember(method.Name, BindingFlags.InvokeMethod,
                                null, pClassOwner, argValues, null, null, argNames);
                        }
                        if (((CustomCaptureInfosBase)ccis).Contains(cciClientIdValue))
                        {
                            foreach (object attribute in attributes)
                            {
                                CciEnumInstance cciEnumInstance = (CciEnumInstance)attribute;
                                String[] cciElts = cciEnumInstance.Name.Split(".".ToCharArray());
                                object obj = pObjectOwner;
                                foreach (string cciElt in cciElts)
                                {
                                    Type tObj = obj.GetType();
                                    FieldInfo fld = tObj.GetField(cciElt);
                                    if (null != fld)
                                    {
                                        object obj2 = fld.GetValue(obj);
                                        if (null == obj2)
                                        {
                                            obj2 = fld.FieldType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                                            fld.SetValue(obj, obj2);
                                        }
                                        obj = obj2;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion NewCreateInstance

        #region public Dump_IsCciContainerArraySpecified
        /// <summary>
        ///  Retourne true s'il existe un élément parmi {pObjArray} renseigné
        ///  <para>Retourne {pDefaultValue} si {pObjArray} est vide ou si {pObjArray} contient uniquement des élément non renseigné</para>
        /// </summary>
        /// <param name="pDefaultValue"></param>
        /// <param name="pObjArray"></param>
        /// <returns></returns>
        public static bool Dump_IsCciContainerArraySpecified(bool pDefaultValue, IContainerCciSpecified[] pObjArray)
        {
            //pIsSpeficied = false; => On n'afffecte pas le specified à false  Ex si Ecran n'affiche pas la totalité des données du trade 
            //EX Trade avec 4 frais => si Ecran n'affiche que 3 frais et qu'il ne sont pas renseignés => il reste cependant 1 frais => Specified doit rester à true
            bool ret = pDefaultValue;
            if (null != pObjArray)
            {
                foreach (IContainerCciSpecified pObj in pObjArray)
                {
                    if (pObj.IsSpecified)
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coDDL"></param>
        /// <param name="pddl"></param>
        /// <param name="pCur1"></param>
        /// <param name="pCur2"></param>
        public static void DDL_LoadQuoteBasis(CustomObjectDropDown coDDL, DropDownList pddl, string pCur1, string pCur2)
        {
            string valueCurrency1PerCurrency2 = string.Empty;
            string valueCurrency2PerCurrency1 = string.Empty;

            switch (coDDL.ListRetrieval.ToLower())
            {
                case "predef:quotebasis":
                    valueCurrency1PerCurrency2 = QuoteBasisEnum.Currency1PerCurrency2.ToString();
                    valueCurrency2PerCurrency1 = QuoteBasisEnum.Currency2PerCurrency1.ToString();
                    break;
                case "predef:strikequotebasis":
                    valueCurrency1PerCurrency2 = StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency.ToString();
                    valueCurrency2PerCurrency1 = StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency.ToString();
                    break;
            }

            bool isUseIsdaRepresentation = BoolFunc.IsTrue(pddl.Attributes["useIsDaRepresentation"]);
            pddl.Items.Clear();
            if (isUseIsdaRepresentation)
            {
                // Ex USD/EUR = 1,2  1,2 USD pour 1 Eur 
                // L'affichage DEV1/DEV2 signifie The amount of currency1 for 1 unit of currency2
                pddl.Items.Add(new ListItem(pCur1 + " / ." + pCur2, valueCurrency1PerCurrency2));
                pddl.Items.Add(new ListItem(pCur2 + " / ." + pCur1, valueCurrency2PerCurrency1));
            }
            else
            {
                // Ex EUR/USD = 1,2  1 Eur donne 1,2 USD
                // L'affichage DEV1/DEV2 signifie The amount of currency2 for 1 unit of currency1
                pddl.Items.Add(new ListItem(pCur2 + ". / " + pCur1, valueCurrency1PerCurrency2));
                pddl.Items.Add(new ListItem(pCur1 + ". / " + pCur2, valueCurrency2PerCurrency1));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coDDL"></param>
        /// <param name="pddl"></param>
        /// <param name="pCur1"></param>
        /// <param name="pCur2"></param>
        public static void DDL_LoadSideRateBasis(CustomObjectDropDown coDDL, DropDownList pddl, string pCur1, string pCur2)
        {
            string valueBaseCurrencyPerCurrency1 = string.Empty;
            string valueCurrency1PerBaseCurrency = string.Empty;
            string valueBaseCurrencyPerCurrency2 = string.Empty;
            string valueCurrency2PerBaseCurrency = string.Empty;

            switch (coDDL.ListRetrieval.ToLower())
            {
                case "predef:sideratebasis":
                    valueCurrency1PerBaseCurrency = SideRateBasisEnum.Currency1PerBaseCurrency.ToString();
                    valueBaseCurrencyPerCurrency1 = SideRateBasisEnum.BaseCurrencyPerCurrency1.ToString();
                    valueCurrency2PerBaseCurrency = SideRateBasisEnum.Currency2PerBaseCurrency.ToString();
                    valueBaseCurrencyPerCurrency2 = SideRateBasisEnum.BaseCurrencyPerCurrency2.ToString();
                    break;
            }
            //
            if (null != pddl)
            {
                bool isUseIsdaRepresentation = BoolFunc.IsTrue(pddl.Attributes["useIsDaRepresentation"]);
                pddl.Items.Clear();
                if (isUseIsdaRepresentation)
                {
                    // Ex USD/EUR = 1,2  1,2 USD pour 1 Eur 
                    // L'affichage DEV1/DEV2 signifie The amount of currency1 for 1 unit of currency2
                    pddl.Items.Add(new ListItem(pCur1 + " / ." + pCur2, valueCurrency1PerBaseCurrency));
                    pddl.Items.Add(new ListItem(pCur2 + " / ." + pCur1, valueBaseCurrencyPerCurrency1));
                    pddl.Items.Add(new ListItem(pCur1 + " / ." + pCur2, valueBaseCurrencyPerCurrency2));
                    pddl.Items.Add(new ListItem(pCur2 + " / ." + pCur1, valueCurrency2PerBaseCurrency));
                }
                else
                {
                    // Ex EUR/USD = 1,2  1 Eur donne 1,2 USD
                    // L'affichage DEV1/DEV2 signifie The amount of currency2 for 1 unit of currency1
                    pddl.Items.Add(new ListItem(pCur2 + ". / " + pCur1, valueCurrency1PerBaseCurrency));
                    pddl.Items.Add(new ListItem(pCur1 + ". / " + pCur2, valueBaseCurrencyPerCurrency1));
                    pddl.Items.Add(new ListItem(pCur2 + ". / " + pCur1, valueBaseCurrencyPerCurrency2));
                    pddl.Items.Add(new ListItem(pCur1 + ". / " + pCur2, valueCurrency2PerBaseCurrency));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pddl"></param>
        public static void DDL_LoadInvoiceRateSourceFixingDateRelativeTo(CustomObjectDropDown coDDL, DropDownList pddl)
        {
            if (null != pddl)
            {
                pddl.Items.Clear();
                switch (coDDL.ListRetrieval.ToLower())
                {
                    case "predef:invoicefixingdaterelativeto":
                        pddl.Items.Add(new ListItem("StartPeriod", "StartPeriod"));
                        pddl.Items.Add(new ListItem("InvoiceDate", "InvoiceDate"));
                        pddl.Items.Add(new ListItem("InvoicePaymentDate", "InvoiceDate"));
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMsg"></param>
        /// <param name="pNewValue"></param>
        /// <returns></returns>
        public static string BuildCciErrMsg(string pMsg, string pNewValue)
        {
            return StrFunc.AppendFormat("{0} [{1}]", pMsg, pNewValue);
        }


        /// <summary>
        ///  Vérifie que l'acteur {pData} est valide 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData">Donnée en entrée</param>
        /// <param name="sql_Actor">Retourne le sql_Actor associé, retourne null si acteur inconnu</param>
        /// <param name="isLoaded">Retourne true si au moins 1 acteur est valide</param>
        /// <param name="isFound">Retourne true si 1 et 1 seul acteur est valide</param>
        /// <param name="pIsActorSYSTEMAuthorized"></param>
        /// <param name="pRole">liste de role pour restrictions eventuelles</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// FI 20130325 [] Add IsActorValid
        /// FI 20131219 [19367] sql_Actor est désormais alimenté lorsque l'acteur est désactivé (même comportement que pour les books)
        public static void IsActorValid(string pCS, string pData, out SQL_Actor sql_Actor, out bool isLoaded, out bool isFound,
                            bool pIsActorSYSTEMAuthorized, EFS.Actor.RoleActor[] pRole, User pUser, string pSessionId)
        {
            isLoaded = false;
            isFound = false;
            sql_Actor = null;

            //20090618 PL Add multi search
            SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
            string search_actor = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_actor",
                                        typeof(String), IDTypeSearch.ToString());
            string[] aSearch_actor = search_actor.Split(";".ToCharArray());
            int searchCount = aSearch_actor.Length;
            for (int k = 0; k < searchCount; k++)
            {
                try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearch_actor[k], true); }
                catch { continue; }

                //20090618 PL Change i < 2 to i < 3
                for (int i = 0; i < 3; i++)
                {
                    string dataToFind = pData;
                    if (i == 1)
                        dataToFind = pData.Replace(" ", "%") + "%";
                    else if (i == 2)//20090618 PL Newness
                        dataToFind = "%" + pData.Replace(" ", "%") + "%";
                    //
                    if ((pIsActorSYSTEMAuthorized) && ("SYSTEM" == dataToFind.ToUpper()))
                    {
                        // FI 20131219 [19367] ScanDataDtEnabledEnum => No
                        sql_Actor = new SQL_Actor(pCS, dataToFind, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, pUser, pSessionId);
                    }
                    else
                    {
                        // FI 20131219 [19367] ScanDataDtEnabledEnum => No
                        sql_Actor = new SQL_Actor(pCS, IDTypeSearch, dataToFind, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.No, pUser, pSessionId)
                        {
                            //FI 20130423 [18620] Spheres® exclue les acteurs ISTEMPLATE
                            ScanTemplate = SQL_Table.ScanDataTemplate.No
                        };
                        //
                        if (ArrFunc.IsFilled(pRole))
                            sql_Actor.SetRoleRange(pRole);
                        //
                        sql_Actor.MaxRows = 2; //NB: Afin de retourner au max 2 lignes
                    }
                    //
                    isLoaded = sql_Actor.IsLoaded;
                    int rowsCount = sql_Actor.RowsCount;
                    //
                    isFound = isLoaded && (rowsCount == 1);
                    //20090618 PL Replace isFound by isLoaded for Break 
                    //if (isFound)
                    if (isLoaded)
                        break;
                }
                if (isLoaded)
                    break;
            }
        }

        /// <summary>
        ///  Vérifie que me marché {pData} est valide 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData">Donnée en entrée</param>
        /// <param name="pIDTypeSearch">Colonne par défaut sur laquelle s'applique la recherche</param>
        /// <param name="sql_Market">Retourne le sql_Market associé, retourne null si marché inconnu</param>
        /// <param name="isLoaded">Retourne true si au moins 1 marché valide</param>
        /// <param name="isFound">Retourne true si 1 et 1 seul marché est valide</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// FI 20170928 [23452] Add
        /// FI 20200116 [25141] Add pIDTypeSearch
        public static void IsMarketValid(string pCS, string pData, SQL_TableWithID.IDType pIDTypeSearch,
            out SQL_Market sql_Market, out bool isLoaded, out bool isFound, User pUser, string pSessionId)
        {
            isLoaded = false;
            isFound = false;
            sql_Market = null;

            string search_Market = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_market",
                                        typeof(String), pIDTypeSearch.ToString());

            string[] aSearch_Market = search_Market.Split(";".ToCharArray());
            for (int k = 0; k < aSearch_Market.Length; k++)
            {
                try { pIDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearch_Market[k], true); }
                catch { continue; }

                for (int i = 0; i < 3; i++)
                {
                    string dataToFind = pData;
                    if (i == 1)
                        dataToFind = pData.Replace(" ", "%") + "%";
                    else if (i == 2)
                        dataToFind = "%" + pData.Replace(" ", "%") + "%";

                    // FI 20200116 [25141] simplification dans la cration de l'instance sql_Market
                    sql_Market = new SQL_Market(pCS, pIDTypeSearch, dataToFind, 
                        SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.No, pUser, pSessionId)
                    {
                        IsUseView = true,
                        MaxRows = 2 //NB: Afin de retourner au max 2 lignes
                    };

                    isLoaded = sql_Market.IsLoaded;
                    isFound = isLoaded && (sql_Market.RowsCount == 1);

                    if (isLoaded)
                        break;
                }
                if (isLoaded)
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData">Donnée en entrée</param>
        /// <param name="sql_Book">Retourne le sql_bool associé, retourne null si book inconnu</param>
        /// <param name="isLoaded">Retourne true si au moins 1 book match</param>
        /// <param name="isFound">Retourne true si 1 et 1 seul book match</param>
        /// <param name="pIdA">acteur associé au book, 0 est une valeur possible</param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// FI 20130325 [] Add IsBookValid
        /// FI 20150630 [XXXXX] Modify (plusieurs petites corrections pour que Spheres® affiche éventuellement 'plusieurs books correspondent')
        public static void IsBookValid(string pCS, string pData, out SQL_Book sql_Book, out bool isLoaded, out bool isFound,
            int pIdA, User pUser, string pSessionId)
        {
            isLoaded = false;
            isFound = false;
            sql_Book = null;

            //20090618 PL Add multi search
            SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
            //FI 20121122 [18281] correction usage du mot clef Spheres_TradeSearch_book
            string search_book = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_book",
                                        typeof(String), IDTypeSearch.ToString());
            string[] aSearch_book = search_book.Split(";".ToCharArray());
            int searchCount = aSearch_book.Length;
            for (int k = 0; k < searchCount; k++)
            {
                try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearch_book[k], true); }
                catch { continue; }

                //20090618 PL Change i < 2 to i < 3
                for (int i = 0; i < 3; i++)
                {
                    string dataToFind = pData;
                    if (i == 1)
                        dataToFind = pData.Replace(" ", "%") + "%";
                    else if (i == 2)//20090618 PL Newness
                        dataToFind = "%" + pData.Replace(" ", "%") + "%";
                    //
                    sql_Book = new SQL_Book(pCS, IDTypeSearch, dataToFind,
                        SQL_Table.ScanDataDtEnabledEnum.No, pIdA, SQL_Table.RestrictEnum.Yes, pUser, pSessionId)
                    {
                        MaxRows = 2 //NB: Afin de retourner au max 2 lignes
                    };
                    //
                    if (false == isLoaded)
                        isLoaded = sql_Book.IsLoaded;

                    int rowsCount = sql_Book.RowsCount;
                    isFound = sql_Book.IsLoaded && (rowsCount == 1);
                    //****************************************************************************************************************
                    //Test supplementaire et specifique aux Books: 
                    //  La Vue VW_BOOK_VIEWER peut remonter plusieurs lignes avec le même Book (Seul FK est différent) 
                    //  Dans ce cas on le considère valide.  
                    //****************************************************************************************************************
                    //20090618 PL Refactoring
                    //if ((!isFound) && sql_Book.IsLoaded && (sql_Book.RowsCount > 1))
                    if ((!isFound) && sql_Book.IsLoaded && (rowsCount > 1))
                    {
                        isFound = true;
                        string identifier = sql_Book.Identifier;
                        for (int j = 1; j < sql_Book.RowsCount; j++)
                        {
                            if (sql_Book.GetColumnValue(j, "IDENTIFIER").ToString() != identifier)
                            {
                                isFound = false;
                                break;
                            }
                        }
                    }
                    if (isFound)
                        break;
                }
                if (isFound)
                    break;
            }
        }


        /// <summary>
        /// Retourne l'acteur contrepartie qu'il conveint d'associé à un book 
        /// <para>En générale c'est le propriétaire du book</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlBook"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        /// FI 20130325 [] Add SearchCounterPartyActorOfBook
        public static SQL_Actor SearchCounterPartyActorOfBook(string pCS, SQL_Book pSqlBook, User pUser, string pSessionId)
        {
            SQL_Actor ret = null;

            SQL_Actor sql_Actor = new SQL_Actor(pCS, pSqlBook.IdA, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, pUser, pSessionId);
            sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY });
            bool isOk = (sql_Actor.IsLoaded);

            if (false == isOk)
            {
                #region Actor owner of Book is not a Counterparty --> Search in ancestor
                ActorAncestor aa = new ActorAncestor(pCS, null, pSqlBook.IdA, pRoleRestrict: null, pRoleTypeExclude: null, pIsPartyRelation: true);
                int max = aa.GetLevelLength();
                for (int i = 1; i < max; i++)
                {
                    string lst = aa.GetListIdA_ActorByLevel(i, ";");
                    string[] idA = StrFunc.StringArrayList.StringListToStringArray(lst);
                    for (int j = 0; j < ArrFunc.Count(idA); j++)
                    {
                        if (StrFunc.IsFilled(idA[j]))
                        {
                            sql_Actor = new SQL_Actor(pCS, Convert.ToInt32(idA[j]), SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, pUser, pSessionId);
                            sql_Actor.SetRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY });
                            isOk = (sql_Actor.IsLoaded);
                            if (isOk)
                                break;
                        }
                    }
                    if (isOk)
                        break;
                }
                #endregion
            }

            if (isOk)
                ret = sql_Actor;

            return ret;
        }

        #region Cci Reflection Tools
        /// <summary>
        /// Get the list of the public fields of the given type
        /// </summary>
        /// <param name="pType">type we want to inspect</param>
        /// <exception cref="ArgumentNullException">when type is null</exception>
        /// <returns>the fields list</returns>
        public static string[] GetCciKeys(Type pType)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            return GetCciKeys(BindingFlags.Public | BindingFlags.Instance, pType);
        }

        /// <summary>
        /// Get the list of the fields of the given type
        /// </summary>
        /// <param name="flags">field filter</param>
        /// <param name="pType">type we want to inspect</param>
        /// <exception cref="ArgumentNullException">when type is null</exception>
        /// <returns>the fields list</returns>
        public static string[] GetCciKeys(BindingFlags flags, Type pType)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            string[] fieldNames = null;

            FieldInfo[] fInfos = pType.GetFields(flags);

            if (!ArrFunc.IsEmpty(fInfos))
            {
                fieldNames = new string[fInfos.Length];

                for (int idxField = 0; idxField < fInfos.Length; idxField++)
                    fieldNames[idxField] = fInfos[idxField].Name;
            }

            return fieldNames;
        }

        /// <summary>
        /// Get the value of the field 
        /// </summary>
        /// <param name="pFieldName">the field name we want get the value from</param>
        /// <param name="pBusinessStruct">the object reference we want to extract the value from</param>
        /// <param name="pType">type we want to inspect</param>
        /// <remarks>the field we want to obtain the value from, it must be type of {EFS_Date, EFS_DateTime, EFS_Decimal, EFS_Integer, EFS_Boolean} </remarks>
        /// <returns>the value of the field converted to string if pFieldName exists, 
        /// otherwise the empty string</returns>
        public static string GetStringValue(string pFieldName, object pBusinessStruct, Type pType)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            string stringValue = null;

            FieldInfo fInfo = pType.GetField(pFieldName);

            if (fInfo != null)
            {
                object value = fInfo.GetValue(pBusinessStruct);

                if (value is EFS_Date date)
                    stringValue = date.Value;
                else if (value is EFS_DateTime time)
                    stringValue = time.Value;
                else if (value is EFS_Decimal @decimal)
                    stringValue = @decimal.Value;
                else if (value is EFS_Integer integer)
                    stringValue = integer.Value;
                else if (value is EFS_Boolean boolean)
                    stringValue = boolean.Value;
                else if (value is EFS_String @string)
                    stringValue = @string.Value;
                else
                {
                    // EG 20151102 [21465] New
                    if (value is DateTime time1)
                    {
                        EFS_DateTime _obj = new EFS_DateTime
                        {
                            DateValue = time1
                        };
                        stringValue = _obj.Value;
                    }
                    else if (value is decimal decimal1)
                    {
                        EFS_Decimal _obj = new EFS_Decimal
                        {
                            DecValue = decimal1
                        };
                        stringValue = _obj.Value;
                    }
                    else if (value is Int32 @int)
                    {
                        EFS_Integer _obj = new EFS_Integer
                        {
                            IntValue = @int
                        };
                        stringValue = _obj.Value;
                    }
                    else if (value is Int64 int1)
                    {
                        EFS_Integer _obj = new EFS_Integer
                        {
                            LongValue = int1
                        };
                        stringValue = _obj.Value;
                    }
                    else if (value is bool boolean1)
                    {
                        EFS_Boolean _obj = new EFS_Boolean
                        {
                            BoolValue = boolean1
                        };
                        stringValue = _obj.Value;
                    }
                    else if (value is string string1)
                    {
                        stringValue = string1;
                    }
                    else
                    {
                        // default string case
                        // UNDONE can raise exceptions?
                        stringValue = Convert.ToString(value);
                    }
                }
            }
            return stringValue;
        }

        /// <summary>
        /// Set the value of a field
        /// </summary>
        /// <param name="pFieldName">the field name we want put the value in</param>
        /// <param name="pBusinessStruct">the object reference we want to extract the value from</param>
        /// <param name="pType">type we want to inspect</param>
        /// <param name="pValue">value to set
        /// </param>
        /// <remarks>the field to set must be type of {EFS_Date, EFS_DateTime, EFS_Decimal, EFS_Integer, EFS_Boolean} </remarks>
        /// <returns></returns>
        public static bool SetStringValue(string pFieldName, object pBusinessStruct, Type pType, string pValue)
        {
            if (pType == null)
                throw new ArgumentNullException("pType");

            bool set = false;

            FieldInfo fInfo = pType.GetField(pFieldName);

            if (fInfo != null)
            {
                object convertedValue;

                if (fInfo.FieldType == typeof(EFS_Date))
                    convertedValue = new EFS_Date(pValue);
                else if (fInfo.FieldType == typeof(EFS_DateTime))
                    convertedValue = new EFS_DateTime(pValue);
                else if (fInfo.FieldType == typeof(EFS_Decimal))
                    convertedValue = new EFS_Decimal(pValue);
                else if (fInfo.FieldType == typeof(EFS_Integer))
                    convertedValue = new EFS_Integer(pValue);
                else if (fInfo.FieldType == typeof(EFS_Boolean))
                    convertedValue = new EFS_Boolean(bool.Parse(pValue));
                else if (fInfo.FieldType == typeof(EFS_String))
                    convertedValue = new EFS_String(pValue);
                else
                    // default string case
                    convertedValue = pValue;

                set = true;

                try
                {
                    fInfo.SetValue(pBusinessStruct, convertedValue);
                }
                // UNDONE treat exception?
                catch (ArgumentException) { }
            }
            return set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">type of the variable, pass always an anonymous type</typeparam>
        /// <param name="variable">the variable we want to know the name</param>
        /// <param name="type">the type of the class where the field is defined, null for methode scope variable</param>
        /// <returns></returns>
        public static string GetFieldVariableName<T>(T variable, Type type) where T : class
        {
            string ret = String.Empty;

            var properties = typeof(T).GetProperties();

            if (properties.Length == 1)
            {
                // get the name of the variable passed as input parameter
                string variableName = properties[0].Name;

                if (type != null)
                {

                    // verify that the name is relative to one field of the class
                    FieldInfo fInfo = type.GetField(variableName);

                    if (fInfo != null)
                        ret = fInfo.Name;
                }
                else
                    ret = variableName;

            }

            return ret;
        }

        #endregion Cci Reflection Tools

        /// <summary>
        ///  Affecte la property display du cci {pCci} lorsque {pCci} représente un marché 
        /// </summary>
        /// <returns></returns>
        /// FI 20161214 [21916] Add
        /// FI 20170928 [23452] Modify
        /// FI 20170928 [23452] Modify
        public static void SetMarKetDisplay(CustomCaptureInfo pCci)
        {
            if (null != pCci.Sql_Table)
            {
                if (!(pCci.Sql_Table is SQL_Market sqlMarket))
                    throw new NotSupportedException(StrFunc.AppendFormat("cci (id:{0}) is not a market", pCci.ClientId_WithoutPrefix));

                string display = sqlMarket.DisplayName;
                pCci.Display = display;
            }

        }

        /// <summary>
        /// Ajoute un cci dit "système" lorsqu'il n'existe pas 
        /// <para>Si le cci existe déjà l'attribut isSystem est valorisé à true</para>
        /// </summary>
        /// <param name="pCcis"></param>
        /// <param name="pClientId"></param>
        /// <param name="pIsMandatory"></param>
        /// <param name="pTypeData"></param>
        /// FI 20170214 [XXXXX] MAdd
        public static void AddCciSystem(CustomCaptureInfosBase pCcis, string pClientId, Boolean pIsMandatory, TypeData.TypeDataEnum pTypeData)
        {
            CustomCaptureInfo cciSystem = new CustomCaptureInfo(pClientId, pIsMandatory, pTypeData)
            {
                IsSystem = true
            };

            CustomCaptureInfo cciFind = pCcis[cciSystem.ClientId_WithoutPrefix];

            if (null != cciFind)
                cciFind.IsSystem = true;
            else
                pCcis.Add(cciSystem);
        }

        /// <summary>
        /// Retourne tous les valeurs d'enums pour lesquels il existe un attribut de regroupement de nom {pGroupName}
        /// </summary>
        /// <typeparam name="T">enum cci (Exemple CciTradeParty.CciEnum)</typeparam>
        /// <param name="pGroupName"></param>
        /// <returns></returns>
        /// FI 20170928 [23452] Add
        public static IEnumerable<T> GetCciEnum<T>(string pGroupName) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            List<T> ret = new List<T>();

            IEnumerable<T> lstEnum =
                            from item in Enum.GetValues(typeof(T)).Cast<T>()
                            select item;

            foreach (T @value in lstEnum)
            {
                FieldInfo fieldInfo = typeof(T).GetField(@value.ToString());
                CciGroupAttribute[] enumAttrs = (CciGroupAttribute[])fieldInfo.GetCustomAttributes(typeof(CciGroupAttribute), true);

                if (ArrFunc.IsFilled(enumAttrs))
                {
                    if (enumAttrs.Where(x => x.name == pGroupName).Count() > 0)
                        ret.Add(@value);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne la colonne SQL utilisée pour alimenter cci.NewValue
        /// <para>Cette méthode s'appuie sur les attributs CciColumnValueAttribute présents sur le cciEnum</para>
        /// <para>Génère une exception s'il n'existe pas d'attribut CciColumnValueAttribute sur la valeur d'enum {pEnumValue} </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pEnumValue">Représente le cci (Exemple CciMarketParty.CciEnum.identifier)</param>
        /// <param name="pIsModeIo"></param>
        /// <returns></returns>
        /// FI 20200116 [25141] Add Method
        public static string GetColumn<T>(Enum pEnumValue, Boolean pIsModeIo) where T : struct
        {
            string ret;
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            CciColumnValueAttribute cciValueFieldAttribute = ReflectionTools.GetAttribute<CciColumnValueAttribute>(pEnumValue);
            if (null == cciValueFieldAttribute)
                throw new Exception(StrFunc.AppendFormat("CciColumnValueAttribute expected on {0}", pEnumValue.ToString()));

            ret = (pIsModeIo && StrFunc.IsFilled(cciValueFieldAttribute.IOColumn)) ? cciValueFieldAttribute.IOColumn : cciValueFieldAttribute.Column;

            return ret;
        }

        /// <summary>
        /// Parse la colonne {pColumn} en SQL_TableWithID.IDType
        /// <para>Genère une exception si impossibilté de parser</para>
        /// </summary>
        /// <param name="pColumn"></param>
        /// <returns></returns>
        /// FI 20200116 [25141] Add Method
        public static SQL_TableWithID.IDType ParseColumn(string pColumn)
        {
            Boolean isOk = Enum.TryParse<SQL_TableWithID.IDType>(pColumn, out SQL_TableWithID.IDType ret);
            if (false == isOk)
                throw new NotSupportedException(StrFunc.AppendFormat("value: {0} is not valid. Unable to parse in SQL_TableWithID.IDType enum", pColumn));
            return ret;
        }
    }




    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class CciGroupAttribute : Attribute
    {
        public string name;
    }

    /// <summary>
    /// Déclaration des colonnes SQL utilisées pour valoriser la propriété newValue d'une cci
    /// </summary>
    /// FI 20200116 [25141] Add
    public class CciColumnValueAttribute : Attribute
    {
        /// <summary>
        /// Colonne utilisée lors de la saisie manuelle depuis l'application web
        /// </summary>
        public string Column;
        /// <summary>
        /// Colonne utilisée lors de l'importation
        /// <para>
        /// Cette propriété doit être renseignée uniquement si elle diffère de Column
        /// </para>
        /// </summary>
        public string IOColumn;
    }
}

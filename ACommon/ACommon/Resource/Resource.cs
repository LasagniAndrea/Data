using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Xml;

namespace EFS.ACommon
{

    public class Ressource
    {

        /// <summary>
        /// Gestionnaire des ressources (*.resx)
        /// </summary>
        internal class ResourceFactory
        {
            #region Constructors
            private ResourceFactory() { }
            #endregion Constructors

            static ResourceManager _rm;

            public static ResourceManager RManager
            {
                get
                {
                    if (_rm == null)
                    {
                        _rm = new ResourceManager("SpheresResource.SpheresResource", Assembly.Load("EFS.SpheresResource"));
                    }
                    return _rm;
                }
                set { _rm = value; }
            }
        }

        /// <summary>
        /// Gestionnaire des ressources SystemMsg 
        /// </summary>
        /// FI 20190716 [XXXXX] Add
        internal class ResourceSystemMsg
        {
            /// <summary>
            /// 
            /// </summary>
            private static ResourceSystemMsg _rSysMsg;
            /// <summary>
            /// Flux xml des ressources SYSTEMMSG
            /// </summary>
            private readonly XmlDocument _xmlDoc;

            /// <summary>
            /// Type de message 
            /// </summary>
            internal enum MsgtypeEnum
            {
                message,
                shortMessage
            }

            /// <summary>
            /// Gestionnaire de ressource SystemMsg courant
            /// </summary>
            internal static ResourceSystemMsg RSysMsg
            {
                get
                {
                    if (_rSysMsg == null)
                    {
                        _rSysMsg = new ResourceSystemMsg();
                    }
                    return _rSysMsg;
                }
            }


            /// <summary>
            /// Constructor
            /// <para>Chargement de la ressource incorporée SpheresResource.SystemMsg_Resource.SystemMsgResource.xml</para>
            /// </summary>
            public ResourceSystemMsg()
            {

                Assembly resAssembly = Assembly.Load("EFS.SpheresResource");
                string streamName = "SpheresResource.SystemMsg_Resource.SystemMsgResource.xml";

                string xmlRes = string.Empty;
                using (Stream stream = resAssembly.GetManifestResourceStream(streamName))
                {
                    if (null == stream)
                        throw new NotSupportedException(StrFunc.AppendFormat("Embedded resource:{0} not found in Assembly:{1}", streamName, resAssembly.FullName));

                    using (StreamReader sr = new StreamReader(stream))
                    {
                        xmlRes = sr.ReadToEnd();
                    }
                }

                _xmlDoc = new XmlDocument();
                _xmlDoc.LoadXml(xmlRes);
            }

            /// <summary>
            ///  Retourne le message {sysCode} {sysNumber}
            /// </summary>
            /// <param name="sysCode"></param>
            /// <param name="sysNumber"></param>
            /// <returns></returns>
            public string GetMsg(string sysCode, string sysNumber)
            {
                return GetText(sysCode, sysNumber, MsgtypeEnum.message);
            }

            /// <summary>
            ///  Retourne le short message {sysCode} {sysNumber}
            /// </summary>
            /// <param name="sysCode"></param>
            /// <param name="sysNumber"></param>
            /// <returns></returns>
            public string GetShortMsg(string sysCode, string sysNumber)
            {
                return GetText(sysCode, sysNumber, MsgtypeEnum.shortMessage);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="sysCode"></param>
            /// <param name="sysNumber"></param>
            /// <param name="pMsgtypeEnum"></param>
            /// <param name="pCulture"></param>
            /// <returns></returns>            
            public string GetText(string sysCode, string sysNumber, MsgtypeEnum pMsgtypeEnum)
            {
                string ret = string.Empty;
                XmlNode node = GetNodeData(sysCode, sysNumber);
                if (null != node)
                {
                    string expression = StrFunc.AppendFormat("./{0}/{1}/text()", pMsgtypeEnum.ToString(), GetCurrentCultureName());
                    node = node.SelectSingleNode(expression);
                    if (null != node)
                        ret = node.Value;
                }
                return ret;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="sysCode"></param>
            /// <param name="sysNumber"></param>
            /// <returns></returns>
            private XmlNode GetNodeData(string sysCode, string sysNumber)
            {
                string expression = StrFunc.AppendFormat("/root/data[@syscode='{0}' and @sysnumber='{1}']", sysCode, sysNumber);
                XmlNode node = _xmlDoc.SelectSingleNode(expression);
                return node;
            }
            /// <summary>
            /// Retourne la CurrentThread.CurrentCulture.Name  
            /// </summary>
            /// <returns></returns>
            private static string GetCurrentCultureName()
            {
                return System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            }
        }

        //
        #region accessor
        public static ResourceManager ExternalRessource
        {
            set { ResourceFactory.RManager = value; }
        }
        #endregion accessor
        //
        #region constructor
        public Ressource()
        {
        }
        #endregion constructor
        //20090416 PL Astuce "temporaire" ...
        public static string GetTrade(string pString)
        {
            string sTrade = "Trade";
            string sAsset = "Security";
            switch (System.Threading.Thread.CurrentThread.CurrentCulture.Name.Substring(0, 2))
            {
                case "fr":
                    sAsset = "Titre";
                    break;
                case "it":
                    sAsset = "Titoli";
                    break;
            }
            pString = pString.Replace(sTrade, sAsset);
            pString = pString.Replace(sTrade.ToLower(), sAsset.ToLower());
            //20091006 PL Tip
            pString = pString.Replace("$Deal$", sTrade.ToLower());
            return pString;
        }

        #region private getStringByRef
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private static bool GetStringByRef(string pName, string pSubstitute, bool pIsScanUpper, CultureInfo pCulture, ref string opString)
        {
            bool isFound = true;
            string s;
            try
            {
                //20080118 PL "lblEmpty" inutilisé ??
                //if (StrFunc.IsEmpty(pName) || ((pName.Length >= 8) && (pName.Substring(0, 8) == "lblEmpty")))
                if (StrFunc.IsEmpty(pName))
                {
                    s = " ";
                }
                else
                {
                    s = ResourceFactory.RManager.GetString(pName, pCulture);
                    //
                    if (s == null)
                    {
                        //Ressource not found
                        try
                        {
                            string menuRoot = Software.MenuRoot();
                            if (menuRoot != null)
                            {
                                if ((pName.IndexOf(menuRoot) == 0) && (menuRoot != Software.MEMUROOT_OTCml))
                                {
                                    //Tentative de recherche d'une ressource OTCml (cet astuce permet de limiter le nombre des ressources pour les menus)
                                    //ex: Si VISION_PROCESS_IO est introuvable, on cherchera OTC_PROCESS_IO
                                    s = ResourceFactory.RManager.GetString(pName.Replace(menuRoot, Software.MEMUROOT_OTCml), pCulture);
                                }
                            }
                        }
                        catch { }
                    }
                    //
                    if (s == null)
                    {
                        //Ressource not found
                        if ((pSubstitute == null) || pIsScanUpper)
                        {
                            //Tentative de recherche d'une ressource tout en majuscule
                            s = ResourceFactory.RManager.GetString(pName.ToUpper());
                            if ((s == null) && (pSubstitute == null))
                            {
                                //PL 20111107 Tip for DocType
                                if (pName.StartsWith(Cst.TypeMIME.Application.ALL.ToString().Replace("*", string.Empty)))
                                {
                                    s = StrFunc.FirstUpperCase(pName.Replace(Cst.TypeMIME.Application.ALL.ToString().Replace("*", string.Empty), string.Empty)) + Cst.HTMLSpace2;
                                    isFound = true;
                                }
                                else if (pName.StartsWith(Cst.TypeMIME.Text.ALL.ToString().Replace("*", string.Empty)))
                                {
                                    s = StrFunc.FirstUpperCase(pName.Replace(Cst.TypeMIME.Text.ALL.ToString().Replace("*", string.Empty), string.Empty)) + Cst.HTMLSpace2;
                                    isFound = true;
                                }
                                else if (pName.StartsWith(Cst.TypeMIME.Image.ALL.ToString().Replace("*", string.Empty)))
                                {
                                    s = StrFunc.FirstUpperCase(pName.Replace(Cst.TypeMIME.Image.ALL.ToString().Replace("*", string.Empty), string.Empty)) + Cst.HTMLSpace2;
                                    isFound = true;
                                }
                                else
                                {
                                    s = "~" + pName + "~";
                                    isFound = false;
                                }
                            }
                            else if (s == null)
                            {
                                s = pSubstitute;
                            }
                        }
                        else
                        {
                            s = pSubstitute;
                        }
                    }
                }
                if (StrFunc.IsFilled(s))
                {
                    #region Ressource recursive (HREF=)
                    int guard = 0;
                    while (s.StartsWith("HREF=") && (guard < 99))
                    {
                        guard++;
                        s = s.Replace(@"HREF=", string.Empty);

                        //s = getStringByRef(s, pSubstitute, pIsScanUpper, pCulture);
                        string sTmp = null;
                        if (GetStringByRef(s, pSubstitute, pIsScanUpper, pCulture, ref sTmp))
                            s = sTmp;
                    }
                    if (guard == 99)
                    {
                        s = "~" + s + "~";
                        isFound = false;
                    }
                    #endregion
                    //
                    s = s.Replace(@"\r\n", Cst.CrLf); //=> Les ressources avec le code \r\n doit être remplacé par un retrour Chariot
                    //
                }
            }
            // FI 20160816 [XXXXX] Add cath => permet d'avoir un message  significatif lorsque la dll Resource est non présente
            catch (System.IO.FileNotFoundException)
            {
                throw;
            }
            catch
            {
                s = "*" + pName + "*";
                isFound = false;
            }
            //
            //return s;
            opString = s;
            return isFound;
        }
        #endregion

        #region public GetStringByRef
        public static bool GetStringByRef(string pName)
        {
            string filler = null;
            return GetStringByRef(pName, null, true, CultureInfo.CurrentCulture, ref filler);
        }
        public static bool GetStringByRef(string pName, ref string opString)
        {
            return GetStringByRef(pName, null, true, CultureInfo.CurrentCulture, ref opString);
        }
        public static bool GetStringByRef(string pName, bool pForced, ref string opString)
        {
            return GetStringByRef(pName, (pForced ? pName : null), true, CultureInfo.CurrentCulture, ref opString);
        }
        #endregion
        #region public GetString2
        public static string GetString2(string pName, params string[] pItems)
        {
            return GetString2(pName, CultureInfo.CurrentCulture, pItems);
        }
        public static string GetString2(string pName, CultureInfo pCulture, params string[] pItems)
        {
            return GetString2(pName, true, pCulture, pItems);
        }
        public static string GetString2(string pName, bool pForced, params string[] pItems)
        {
            return GetString2(pName, pForced, CultureInfo.CurrentCulture, pItems);
        }
        public static string GetString2(string pName, bool pForced, CultureInfo pCulture, params string[] pItems)
        {
            string s = GetString(pName, pForced, pCulture);
            //PL 20110614 Use string.Format()
            //for (int i = 1; i <= pItems.Length; i++)
            //{
            //    s = s.Replace("|" + i.ToString(), pItems[i - 1]);
            //}
            //Warning: Toujours faire appel à string.Format(), même si pItems.Length==0, car string.Format() "dédouble" le cas échénat les accolades doublées. (PL)
            s = string.Format(s, pItems);
            return s;
        }
        #endregion
        #region public GetString
        #region Surcharges
        public static string GetString(string pName)
        {
            return GetString(pName, null, true);
        }
        public static string GetString(string pName, CultureInfo pCulture)
        {
            return GetString(pName, null, true, pCulture);
        }
        public static string GetString(string pName, bool pForced)
        {
            return GetString(pName, (pForced ? pName : null), true);
        }
        public static string GetString(string pName, bool pForced, CultureInfo pCulture)
        {
            return GetString(pName, (pForced ? pName : null), true, pCulture);
        }
        public static string GetString(string pName, string pSubstitute)
        {
            return GetString(pName, pSubstitute, true);
        }
        public static string GetString(string pName, string pSubstitute, CultureInfo pCulture)
        {
            return GetString(pName, pSubstitute, true, pCulture);
        }
        public static string GetString(string pName, string pSubstitute, bool pIsScanUpper)
        {
            return GetString(pName, pSubstitute, pIsScanUpper, CultureInfo.CurrentCulture);
        }
        public static string GetString(string pName, string pSubstitute, bool pIsScanUpper, CultureInfo pCulture)
        {
            string ret = pSubstitute;
            GetStringByRef(pName, pSubstitute, pIsScanUpper, pCulture, ref ret);
            return ret;
        }
        #endregion
        #endregion
        #region	public GetMenu
        public static string GetMenu_Fullname(string pString)
        {
            return GetMenu_Fullname(pString, pString);
        }
        public static string GetMenu_Fullname(string pString, string pDefault)
        {
            return GetMenu(pString, pDefault, 1);
        }
        public static string GetMenu_Shortname(string pString, string pDefault)
        {
            return GetMenu(pString, pDefault, 0);
        }
        public static string GetMenu_Shortname2(string pString, string pDefault)
        {
            string menu_Shortname = GetMenu_Shortname(pString, pDefault);
            menu_Shortname = menu_Shortname.Replace(@"<sub>", @"[[sub]]").Replace(@"</sub>", @"[[/sub]]");
            menu_Shortname = menu_Shortname.Replace(@"<sup>", @"[[sup]]").Replace(@"</sup>", @"[[/sup]]");
            menu_Shortname = menu_Shortname.Replace(@"<b>", @"[[b]]").Replace(@"</b>", @"[[/b]]");
            return menu_Shortname;
        }
        public static string DecodeMenu_Shortname2(string pMenu_Shortname)
        {
            pMenu_Shortname = pMenu_Shortname.Replace(@"[[sub]]", @"<sub>").Replace(@"[[/sub]]", @"</sub>");
            pMenu_Shortname = pMenu_Shortname.Replace(@"[[sup]]", @"<sup>").Replace(@"[[/sup]]", @"</sup>");
            pMenu_Shortname = pMenu_Shortname.Replace(@"[[b]]", @"<b>").Replace(@"[[/b]]", @"</b>");
            return pMenu_Shortname;
        }
        public static string DecodeDA(string pString)
        {
            if (!String.IsNullOrEmpty(pString))
            {
                if ((pString.IndexOf(@"[[Data ") == 0) && ((pString.IndexOf(@"[[/Data]]") > 0) || (pString.IndexOf(@"/]]") > 0)))
                {
                    pString = pString.Replace(@"[[/Data]]", @"</Data>").Replace(@"/]]", @"/>").Replace(@"[[Data ", @"<Data ").Replace(@"]]", @">");
                }
            }
            return pString;
        }
        public static string EncodeDA(string pString)
        {
            if (!String.IsNullOrEmpty(pString))
            {
                if ((pString.IndexOf(@"<Data ") == 0) && ((pString.IndexOf(@"</Data>") > 0) || (pString.IndexOf(@"/>") > 0)))
                {
                    pString = pString.Replace(@"</Data>", @"[[/Data]]").Replace(@"/>", @"/]]").Replace(@"<Data ", @"[[Data ").Replace(@">", @"]]");
                }
            }
            return pString;
        }
        private static string GetMenu(string pString, string pDefault, int pIndex)
        {
            //PL 20110914
            //string res = GetString(pString);
            //if (res == "~" + pString + "~")
            string res = GetMulti(pString, pIndex);
            if (res == pString)
            {
                //Ressource not found
                string menuRoot = Software.MenuRoot();
                if ((pString.IndexOf(menuRoot) == 0) && (menuRoot != Software.MEMUROOT_OTCml))
                {
                    //Tentative de recherche d'une ressource OTCml (cet astuce permet de limiter le nombre des ressources pour les menus)
                    //ex: Si VISION_PROCESS_IO est introuvable, on cherchera OTC_PROCESS_IO
                    //res = GetString(pString.Replace(menuRoot, Software.MEMUROOT_OTCml));
                    string stringTmp = pString.Replace(menuRoot, Software.MEMUROOT_OTCml);
                    string resTmp = GetMulti(stringTmp, pIndex);
                    if (resTmp != stringTmp)
                        res = resTmp;
                }
            }
            //PL 20100212 Add else if (Afin d'avoir des titres cohérents sur les référentiels ouverts depuis les boutons "...")
            else if (StrFunc.IsEmpty(pString))
            {
                switch (pDefault)
                {
                    case "ACTOR":
                        pString = "OTC_REF_ACT_ACT";
                        //res = GetString(pString);
                        res = GetMulti(pString, pIndex);
                        break;
                    case "BOOK":
                    case "BOOK_VIEWER":
                        pString = "OTC_REF_ACT_ACT_BOOK";
                        //res = GetString(pString);
                        res = GetMulti(pString, pIndex);
                        break;
                }
            }

            //PL 20110914
            //if ((res == "~" + pString + "~") || StrFunc.IsEmpty(pString))
            if ((res == pString) || StrFunc.IsEmpty(pString))
                //Ressource not found
                res = GetString(pDefault, true);

            return res;
        }
        #endregion
        #region public GetStringForJS
        public static string GetStringForJS(string pName)
        {
            string s = GetString(pName);
            s = s.Replace("'", @"\'");
            return s;
        }
        #endregion
        #region public GetStringForSQL
        public static string GetStringForSQL(string pName)
        {
            string s = GetString(pName);
            s = s.Replace("'", "''");
            return s;
        }
        #endregion
        #region	public GetMultiForDatagrid
        public static string GetMultiForDatagrid(string pIdLstConsult, string pTable, string pData)
        {
            return GetMultiForDatagrid(pIdLstConsult, pTable, pData, 0);
        }
        public static string GetMultiForDatagrid(string pIdLstConsult, string pTable, string pData, int pIndex)
        {
            bool isRessourceFound = Ressource.GetMulti(pData + ":" + pTable + ":" + pIdLstConsult, pIndex, out string ressource);
            if (!isRessourceFound)
            {
                isRessourceFound = Ressource.GetMulti(pData + ":" + pTable, pIndex, out ressource);
                if (!isRessourceFound)
                    Ressource.GetMulti(pData, pIndex, out ressource);
            }
            return ressource;
        }
        #endregion	public GetMultiForDatagrid
        #region	public GetMulti
        public static bool GetMulti(string pString, int pIndex, out string opRessource)
        {
            const string NOTTRANSLATED = "~";
            opRessource = GetMulti(pString, pIndex, 0, NOTTRANSLATED);
            bool ret = opRessource != NOTTRANSLATED;
            if (!ret)
                opRessource = pString;
            return ret;
        }
        // EG 20170425 Used with BS version
        public static string GetMultiEmpty(string pString)
        {
            return GetMultiEmpty(pString, null, 0, 0);
        }
        public static string GetMultiEmpty(string pString, string pSubstitute, int pIndex, int pDefaultIndex)
        {
            string ressource = GetMulti(pString, pIndex, pDefaultIndex, pSubstitute);
            return (StrFunc.IsEmpty(ressource) ? " " : ressource);
        }

        public static string GetMulti(string pString)
        {
            return GetMulti(pString, 0, 0, null);
        }
        public static string GetMulti(string pString, string pSubstitute)
        {
            return GetMulti(pString, 0, 0, pSubstitute);
        }
        public static string GetMulti(string pString, int pIndex)
        {
            return GetMulti(pString, pIndex, 0, null);
        }
        public static string GetMulti(string pString, int pIndex, string pSubstitute)
        {
            return GetMulti(pString, pIndex, 0, pSubstitute);
        }
        public static string GetMulti(string pString, int pIndex, int pDefaultIndex)
        {
            return GetMulti(pString, pIndex, pDefaultIndex, null);
        }
        public static string GetMulti(string pString, int pIndex, int pDefaultIndex, string pSubstitute)
        {
            string res;
            if (StrFunc.IsFilled(pSubstitute))
            {
                res = GetString(pString, pSubstitute);
            }
            else
            {
                res = GetString(pString, true);
            }

            //PL 20140610 Newness
            res = res.Replace(Cst.HTMLSpace, "~Cst.HTMLSpace~");

            char[] aSplitterChar = new char[] { ';' };
            String[] aRes;
            aRes = res.Split(aSplitterChar);

            if (aRes.Length >= (pIndex + 1))
            {
                res = aRes[pIndex];
            }
            else if (aRes.Length >= (pDefaultIndex + 1))
            {
                res = aRes[pDefaultIndex];
            }
            else if (!res.StartsWith("~"))
            {
                res = "~" + res + "~";
            }
            //PL 20140610 Newness
            res = res.Replace("~Cst.HTMLSpace~", Cst.HTMLSpace);

            //PL 20130521 New feature
            if ((pIndex > 0) && res.StartsWith("href="))
            {
                string newindex = res.Substring(5);
                if (IntFunc.IsPositiveInteger(newindex))
                {
                    int newIndex = IntFunc.IntValue(newindex);
                    if ((newIndex != pIndex) && (newIndex != pDefaultIndex))
                    {
                        res = GetMulti(pString, newIndex);
                    }
                }
            }
            return res;
        }
        #endregion
        #region public GetMultiByRef
        public static bool GetMultiByRef(string pString, int pIndex, ref string opString)
        {
            opString = GetMulti(pString, pIndex, "~");
            return (opString != "~");
        }
        #endregion

        #region public GetString_SelectionCreationModification
        public static string GetString_SelectionCreationModification(Cst.ConsultationMode pConsultationMode, bool pIsNew)
        {

            string tmp;
            switch (pConsultationMode)
            {
                case Cst.ConsultationMode.Normal:
                    tmp = (pIsNew ? "Creation" : "Modification");
                    break;
                case Cst.ConsultationMode.Select:
                    tmp = "Selection";
                    break;
                default:
                    tmp = "Consultation";
                    break;
            }
            //
            return Ressource.GetString(tmp);
        }
        #endregion
        

        /// <summary>
        /// Retourne la ressource associée à une entrée dans SYSTEMMSG 
        /// </summary>
        /// <param name="sysCode"></param>
        /// <param name="sysNumber"></param>
        /// <param name="pisRemoveBold"></param>
        /// <returns></returns>
        /// FI 20190716 [XXXXX] Add Method
        public static string GetSystemMsg(string sysCode, string sysNumber, Boolean pIsRemoveBold)
        {
            string ret = ResourceSystemMsg.RSysMsg.GetMsg(sysCode, sysNumber);
            if (StrFunc.IsFilled(ret) && pIsRemoveBold)
                ret = ret.Replace("<b>", string.Empty).Replace("</b>", string.Empty);
            return ret;
        }

        /// <summary>
        /// Retourne la ressource associée à une entrée dans SYSTEMMSG 
        /// </summary>
        /// <param name="sysCode"></param>
        /// <param name="sysNumber"></param>
        /// <returns></returns>
        /// FI 20190716 [XXXXX] Add Method 
        public static string GetSystemMsg(string sysCode, string sysNumber)
        {
            return GetSystemMsg(sysCode, sysNumber, true);
        }

    }
}

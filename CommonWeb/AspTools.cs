#region Using Directives
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Log;
using EFS.Common.Net;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class AspTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCssColor"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static bool CheckCssColor(ref string opCssColor)
        {
            bool isOk = true;
            string cssColor = opCssColor;
            try
            {
                if (StrFunc.IsEmpty(cssColor) || (false == ReflectionTools.ConvertStringToEnumOrNullable<Cst.CSSColorEnum>(cssColor).HasValue))
                    isOk = false;

                if (!isOk)
                {
                    cssColor = SessionTools.Company_CssColor;
                    if (StrFunc.IsEmpty(cssColor) || (false == ReflectionTools.ConvertStringToEnumOrNullable<Cst.CSSColorEnum>(cssColor).HasValue))
                        cssColor = SystemSettings.GetAppSettings("Spheres_StyleSheetColor");
                }
            }
            catch
            {
                cssColor = SystemSettings.GetAppSettings("Spheres_StyleSheetColor");
            }
            opCssColor = cssColor;
            return isOk;
        }

        /// <summary>
        /// Lecture du mode  GUI (Noir ou blanc
        /// </summary>
        /// <param name="opCssMode"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        // EG 20210326 [25556] Si non Connecté c'est le CssMode du Web.Config qui est pris en compte, sinon celui du profil (donc dans table COOKIES)
        public static bool CheckCssMode(ref string opCssMode)
        {
            bool isOk = true;
            string cssMode = opCssMode;
            try
            {
                string includesPath = SessionTools.AppInstance.MapPath("Includes") + @"\";

                if (StrFunc.IsEmpty(cssMode) || (false == File.Exists(includesPath + String.Format("EFSTheme-{0}.min.css", cssMode))))
                    isOk = false;

                if (!isOk)
                {
                    if (SessionTools.IsConnected)
                    {
                        cssMode = SessionTools.CSSMode.ToString();
                        if (StrFunc.IsEmpty(cssMode) || (false == File.Exists(includesPath + String.Format("EFSTheme-{0}.min.css", cssMode))))
                            cssMode = SystemSettings.GetAppSettings("Spheres_StyleSheetMode");
                    }
                    else
                    {
                        cssMode = SystemSettings.GetAppSettings("Spheres_StyleSheetMode");
                    }
                }
            }
            catch
            {
                cssMode = SystemSettings.GetAppSettings("Spheres_StyleSheetMode");
            }
            opCssMode = cssMode;
            return isOk;
        }
        /// <summary>
        /// Lecture de la couleur de style défini pour l'instrument, à défaut le produit et à défaut l'entité
        /// </summary>
        /// <param name="pSqlInstrument"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static string GetCssColorDefault(SQL_Instrument pSqlInstrument)
        {
            string ret = SessionTools.Company_CssColor;
            //
            if ((StrFunc.IsFilled(pSqlInstrument.CssColor)))
            {
                ret = pSqlInstrument.CssColor;
            }
            else
            {
                //20090720 FI Add SetCacheOn
                string cs = CSTools.SetCacheOn(pSqlInstrument.CS);
                SQL_Product sqlProduct = new SQL_Product(cs, SQL_TableWithID.IDType.Identifier, pSqlInstrument.Product_Identifier);
                if (sqlProduct.IsLoaded && StrFunc.IsFilled(sqlProduct.CssColor))
                    ret = sqlProduct.CssColor;
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookieName"></param>
        /// <returns></returns>
        public static string GetCookieName(string pCookieName)
        {
            //Format: SITE_SOFTWARE_COOKIENAME
            string ret = HttpContext.Current.Request.ApplicationPath;
            ret = ret.Replace(@"\", string.Empty);
            ret = ret.Replace(@"/", string.Empty);
            ret += "_" + Software.Name;
            ret += "_" + pCookieName;
            return ret;
        }

        /// <summary>
        /// Lecture du fichier pFilePath et téléchargement dans la foulée vers le client  
        /// <para>
        /// 
        /// </para>
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pFilePath"></param>
        /// <param name="pFileType"></param>
        /// <param name="pAddHeader">si true affiche le nom de fichier dans la msgBox de confirmation généré par le browser
        /// <para>si false et que le fichier est de type XML,PDF,TXT alors browser ouvre le document sans msgBox de confirmation</para>
        /// <para>Lorsque le fichier est un zip alors browser ouvre toujours le fichier avec msgBox de confirmation</para>
        /// </param>
        /// 
        ///29102009 FI [download File] void function and pAddHeader
        public static void OpenBinaryFile(PageBase pPage, string pFilePath, string pFileType, bool pAddHeader)
        {

            byte[] buffer = FileTools.ReadFileToBytes(pFilePath);

            if (pAddHeader)
            {
                //29102009 FI [download File] add FileInfo
                FileInfo fileInfo = new FileInfo(pFilePath);
                pPage.Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileInfo.Name);
                pPage.Response.AppendHeader("Content-Length", fileInfo.Length.ToString());
            }

            pPage.Response.ClearContent();
            pPage.Response.ContentType = pFileType;

            //Test PL ------------------------------------------------------
            //pPage.Response.ContentType = "application/octet-stream";
            //pPage.Response.Flush();
            //Test PL ------------------------------------------------------

            pPage.Response.BinaryWrite(buffer);
            pPage.IsResponseToEnd = true;

            //************************************************************************************************************************
            //WARNING: DEBUG temporaire à affiner !!!
            //************************************************************************************************************************
            //PL 20110412 Add pPage.Response.End() car ici IsResponseToEnd n'est pas exploitable puisque dans le cas des ZIP nous sommes dans le PreRender, donc après le Load !!!!
            //************************************************************************************************************************
            if (pFileType == Cst.TypeMIME.Application.XZipCompressed)
            {
                //PL 20110412 Afin de ne pas dégrader le reste notamment la consultation des PDF sous Spheres Vision, je ne traite ici en DURE que des ZIP
                pPage.Response.End();
            }
            //************************************************************************************************************************

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opNbRowsDeleted"></param>
        public static void PurgeSQLCookie(out int opNbRowsDeleted)
        {
            PurgeSQLCookie(false, out opNbRowsDeleted);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="opNbRowsDeleted"></param>
        public static void PurgeAllSQLCookie(out int opNbRowsDeleted)
        {
            PurgeSQLCookie(true, out opNbRowsDeleted);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsForAll"></param>
        /// <param name="opNbRowsDeleted"></param>
        public static void PurgeSQLCookie(bool pIsForAll, out int opNbRowsDeleted)
        {

            opNbRowsDeleted = -1;

            StringBuilder delete = new StringBuilder();
            delete.Append(SQLCst.DELETE_DBO + Cst.OTCml_TBL.COOKIE.ToString() + Cst.CrLf);
            if (false == pIsForAll)
            {
                delete.Append(SQLCst.WHERE + "IDA=" + SessionTools.Collaborator_IDA + Cst.CrLf);
                //PL 20110921
                //delete.Append(SQLCst.AND + "HOSTNAME=" + DataHelper.SQLString(SessionTools.HostName));
            }

            int deletedRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, delete.ToString());
            if (deletedRows >= 0)
                opNbRowsDeleted = deletedRows;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcookiedata"></param>
        public static void WriteSQLCookie(CookieData[] pcookiedata)
        {
            for (int i = 0; i < pcookiedata.Length; i++)
            {
                if (null != pcookiedata[i])
                    WriteSQLCookie(pcookiedata[i].GrpEltName, pcookiedata[i].EltName, pcookiedata[i].EltValue);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookieElement"></param>
        /// <param name="pCookieValue"></param>
        public static void WriteSQLCookie(string pCookieElement, string pCookieValue)
        {
            WriteSQLCookie(Cst.SQLCookieGrpElement.DBNull, pCookieElement, pCookieValue);
        }
        /// <summary>
        /// Mettre à jour (ou bien le créer s'il n'existe pas) le Cookie de l'utilisateur en cours
        /// </summary>
        /// <param name="pCookieGrpElement"></param>
        /// <param name="pCookieElement"></param>
        /// <param name="pCookieValue"></param>
        /// EG 20190730 Upd (Mise à jour DTSYS)
        public static void WriteSQLCookie(Cst.SQLCookieGrpElement pCookieGrpElement, string pCookieElement, string pCookieValue)
        {
            string cs = SessionTools.CS;

            DataParameters parameters = new DataParameters();
            // FI 20200820 [25468] Dates systemes en UTC
            parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.DTSYS), OTCmlHelper.GetDateSysUTC(cs));
            parameters.Add(new DataParameter(cs, "ELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            // 20160725 EG Upd 128 with SQLCst.UT_LSTVALUE_LEN
            parameters.Add(new DataParameter(cs, "VALUE", DbType.AnsiString, SQLCst.UT_LSTVALUE_LEN));
            parameters.Add(new DataParameter(cs, "IDA", DbType.Int32));
            //PL 20110921
            //parameters.Add(DataParameter.GetParameter(cs,DataParameter.ParameterEnum.HOSTNAME));    

            if ((pCookieGrpElement != Cst.SQLCookieGrpElement.DBNull))
                parameters.Add(new DataParameter(cs, "GRPELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));

            parameters["ELEMENT"].Value = pCookieElement;
            parameters["VALUE"].Value = (StrFunc.IsEmpty(pCookieValue) ? " " : pCookieValue);
            parameters["IDA"].Value = SessionTools.Collaborator_IDA;
            //PL 20110921
            //parameters["HOSTNAME"].Value  = SessionTools.HostName;
            if (parameters.Contains("GRPELEMENT"))
                parameters["GRPELEMENT"].Value = pCookieGrpElement.ToString();

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.COOKIE.ToString();
            sqlUpdate += SQLCst.SET + @"VALUE=@VALUE, DTSYS=@DTSYS";
            sqlUpdate += SQLCst.WHERE + @"ELEMENT=@ELEMENT" + SQLCst.AND + @"IDA=@IDA";
            //PL 20110921
            //sqlUpdate += SQLCst.AND + @"HOSTNAME=@HOSTNAME";

            if (parameters.Contains("GRPELEMENT"))
                sqlUpdate += SQLCst.AND + @"GRPELEMENT=@GRPELEMENT";
            else
                sqlUpdate += SQLCst.AND + @"GRPELEMENT" + SQLCst.IS_NULL;

            int changedRows = DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlUpdate.ToString(), parameters.GetArrayDbParameter());
            if (0 == changedRows)
            {
                StrBuilder sqlInsert = new StrBuilder();
                sqlInsert += SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.COOKIE.ToString();
                sqlInsert += @"(IDA, HOSTNAME, GRPELEMENT, ELEMENT, VALUE, DTSYS) values (@IDA, @HOSTNAME, @GRPELEMENT, @ELEMENT, @VALUE,@DTSYS)";
                parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.HOSTNAME));
                parameters["HOSTNAME"].Value = SessionTools.ServerAndUserHost;
                if (!parameters.Contains("GRPELEMENT"))
                {
                    parameters.Add(new DataParameter(cs, "GRPELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), DBNull.Value);
                    if ((pCookieGrpElement != Cst.SQLCookieGrpElement.DBNull))
                        parameters["GRPELEMENT"].Value = pCookieGrpElement.ToString();
                }

                _ = DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlInsert.ToString(), parameters.GetArrayDbParameter());
            }

        }

        /// <summary>
        /// Mettre à jour les Cookies pour:        
        /// <para>- les éléments temporaires (éléments commençant par {pTemporaryPrefix})</para>
        /// <para>- tous les autres utilisateurs, en dehors de l'utilisateur en cours, propriétaires ou pas de l'élément</para>
        /// </summary>
        /// <param name="pCookieElement"></param>
        /// <param name="pCookieValue"></param>
        /// <param name="pTemporaryPrefix"></param>
        public static void WriteSQLCookieTemporary(string pCookieElement, string pCookieValue, string pTemporaryPrefix)
        {
            string cs = SessionTools.CS;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(cs, "ELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN));
            parameters.Add(new DataParameter(cs, "VALUE", DbType.AnsiString, 128));
            parameters.Add(new DataParameter(cs, "IDA", DbType.Int32));

            // Dans le cas des Templates de consultation (LSTTEMPLATE):
            // - la valeur de la colonne COOKIE.ELEMENT est suffixée par l'Id de l'utilisateur
            // - il faudrait veiller à ce que la valeur de l'élément {pCookieElement} ne soit pas suffixée par l'Id de l'utilisateur
            parameters["ELEMENT"].Value = pCookieElement;
            // Dans le cas des Templates de consultation (LSTTEMPLATE), cette valeur est suffixée par:
            // l'IDA du propriétaire du Template
            parameters["VALUE"].Value = pCookieValue;
            parameters["IDA"].Value = SessionTools.Collaborator_IDA;

            StrBuilder sqlUpdate = new StrBuilder();
            sqlUpdate += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.COOKIE.ToString();
            sqlUpdate += SQLCst.SET + @"VALUE=@VALUE";
            sqlUpdate += SQLCst.WHERE + @"(VALUE = '" + pTemporaryPrefix + @"' || @VALUE)" + SQLCst.AND + @"(ELEMENT like @ELEMENT || '%')" + SQLCst.AND + @"(IDA!=@IDA)";
            _ = DataHelper.ExecuteNonQuery(cs, CommandType.Text, sqlUpdate.ToString(), parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// Ininitalisation d'un Cookie avec ses attribut par défaut
        /// </summary>
        /// <param name="pCookieName"></param>
        /// <returns></returns>
        /// EG 20210216 [25664] Sécurité Cookie
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour non partage en JS)
        public static HttpCookie InitHttpCookie(string pCookieName)
        {
            return InitHttpCookie(pCookieName, true);
        }
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour partage ou non en JS)
        public static HttpCookie InitHttpCookie(string pCookieName, bool pIsHttpOnly)
        {
            HttpCookie cookie = new HttpCookie(pCookieName)
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = pIsHttpOnly,
                //Secure = true, // Utilisation future sur HTTPS
                Path = "/",
            };
            return cookie;

        }
        /// <summary>
        /// Ininitalisation d'un Cookie avec ses attribut par défaut + Valeur du cookie
        /// </summary>
        /// <param name="pCookieName"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// EG 20210216 [25664] Sécurité Cookie
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour non partage en JS)
        public static HttpCookie InitHttpCookie(string pCookieName, string pValue)
        {
            return InitHttpCookie(pCookieName, true, pValue);
        }
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour partage ou non en JS)
        public static HttpCookie InitHttpCookie(string pCookieName, bool pIsHttpOnly, string pValue)
        {
            HttpCookie cookie = InitHttpCookie(pCookieName, pIsHttpOnly);
            cookie.Value = pValue;
            return cookie;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookieName"></param>
        /// <param name="pcookiedata"></param>
        /// <param name="pValueExpires"></param>
        /// <param name="pUniteValueExpires"></param>
        /// <param name="pDomain"></param>
        /// <param name="opCookie"></param>
        /// <returns></returns>
        /// EG 20210126 [25664] Gestion Samesite (Strict) sur cookie
        public static Cst.ErrLevel WriteCookie(string pCookieName, CookieData[] pcookiedata, int pValueExpires, string pUniteValueExpires, string pDomain, out HttpCookie opCookie)
        {
            HttpCookie cookie = InitHttpCookie(GetCookieName(pCookieName));
            Cst.ErrLevel ret;
            try
            {
                for (int i = 0; i < pcookiedata.Length; i++)
                    cookie.Values.Add(pcookiedata[i].EltName, pcookiedata[i].EltValue);
                ret = CookieSetParam(pValueExpires, pUniteValueExpires, pDomain, ref cookie);
            }
            catch
            {
                ret = Cst.ErrLevel.FAILURE;
            }
            opCookie = cookie;
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookieName"></param>
        /// <param name="pEltName"></param>
        /// <param name="pValue"></param>
        /// <param name="pValueExpires"></param>
        /// <param name="pUniteValueExpires"></param>
        /// <param name="pDomain"></param>
        /// <param name="opCookie"></param>
        /// <returns></returns>
        /// EG 20210126 [25556] Gestion Samesite (Strict) sur cookie
        /// EG 20210216 [25664] Sécurité Cookie
        public static Cst.ErrLevel WriteCookie(string pCookieName, string pEltName, string pValue, int pValueExpires, string pUniteValueExpires, string pDomain, out HttpCookie opCookie)
        {
            HttpCookie cookie = InitHttpCookie(GetCookieName(pCookieName));
            Cst.ErrLevel ret;
            try
            {
                cookie.Values.Add(pEltName, pValue);
                ret = CookieSetParam(pValueExpires, pUniteValueExpires, pDomain, ref cookie);
            }
            catch
            {
                ret = Cst.ErrLevel.FAILURE;
            }
            opCookie = cookie;
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValueExpires"></param>
        /// <param name="pUniteValueExpires"></param>
        /// <param name="pDomain"></param>
        /// <param name="opCookie"></param>
        /// <returns></returns>
        private static Cst.ErrLevel CookieSetParam(int pValueExpires, string pUniteValueExpires, string pDomain, ref HttpCookie opCookie)
        {
            Cst.ErrLevel ret;
            try
            {
                if (pDomain.Length > 0)
                    opCookie.Domain = pDomain;
                switch (pUniteValueExpires)
                {
                    case "MI":
                        opCookie.Expires = DateTime.Now.AddMinutes(pValueExpires);
                        break;
                    case "H":
                        opCookie.Expires = DateTime.Now.AddHours(pValueExpires);
                        break;
                    case "M":
                        opCookie.Expires = DateTime.Now.AddMonths(pValueExpires);
                        break;
                    case "Y":
                        opCookie.Expires = DateTime.Now.AddYears(pValueExpires);
                        break;
                    default:
                        //"D" --> Day
                        opCookie.Expires = DateTime.Now.AddDays(pValueExpires);
                        break;
                }
                ret = Cst.ErrLevel.SUCCESS;
            }
            catch
            {
                ret = Cst.ErrLevel.FAILURE;
            }
            //opCookie = cookie;
            return ret;
        }

        /// <summary>
        /// Ecriture le contenu d'un LOFile dans un fichier. 
        /// <para>Le nom du fichier généré est retouné par la fonction</para>
        /// </summary>
        /// <param name="pLOFile"></param>
        /// <param name="pPath"></param>
        /// <returns></returns>
        ///20102009 FI [download File] 
        public static string WriteLOFile(LOFile pLOFile, string pPath)
        {
            if (null == pLOFile)
                throw new ArgumentException("pLOFile is null");
            if (StrFunc.IsEmpty(pPath))
                throw new ArgumentException("pPath is empty");
            //
            if (false == pPath.EndsWith(@"\"))
                pPath += @"\";

            // FI 20200409 [XXXXX] Call FileTools.ReplaceFilenameInvalidChar
            string fileName = FileTools.ReplaceFilenameInvalidChar(pLOFile.FileName);
            string ret = pPath + fileName;
            //
            if (null != pLOFile.FileContent)
            {
                byte[] fileContent = pLOFile.FileContent;
                //
                if (Cst.TypeMIME.Text.Html == pLOFile.FileType)
                {
                    string html = LOFile.Encoding.GetString(fileContent);
                    html = XMLTools.ReplaceHtmlTagImage(SessionTools.CS, html, SessionTools.TemporaryDirectory.ImagesPathMapped, SessionTools.TemporaryDirectory.ImagesPath);
                    fileContent = LOFile.Encoding.GetBytes(html);
                }
                FileTools.WriteBytesToFile(fileContent, ret, FileTools.WriteFileOverrideMode.Override);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookie"></param>
        /// <param name="pEltName"></param>
        /// <param name="pValue"></param>
        public static void ReadCookie(HttpCookie pCookie, string pEltName, out string opValue)
        {
            //PL 20160223 Manage not exist pEltName
            opValue = string.Empty;
            if (pCookie != null)
            {
                object eltValue = pCookie.Values[pEltName];
                if (eltValue != null)
                    opValue = eltValue.ToString();
            }

        }
        public static void ReadCookie(HttpCookie pCookie, string pNewEltName, string pOldEltName, out string opValue)
        {
            opValue = string.Empty;
            if (pCookie != null)
            {
                object eltValue = pCookie.Values[pNewEltName];
                if (eltValue != null)
                {
                    opValue = eltValue.ToString();
                }
                else
                {
                    eltValue = pCookie.Values[pOldEltName];
                    if (eltValue != null)
                    {
                        opValue = eltValue.ToString();
                    }
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookie"></param>
        /// <param name="pEltName"></param>
        /// <param name="pValue"></param>
        /// <param name="pDefault"></param>
        public static void ReadCookie(HttpCookie pCookie, string pEltName, out string opValue, string pDefault)
        {
            ReadCookie(pCookie, pEltName, out opValue);
            if (StrFunc.IsEmpty(opValue))
                opValue = pDefault;
        }
        /// <summary>
        /// Lecture de la valeur du {pCookieElement} pour le couple {acteur, hostName} 
        /// </summary>
        /// <param name="pCookieElement"></param>
        /// <param name="pValue">Valeur retour du cookie</param>
        public static void ReadSQLCookie(string pCookieElement, out string opValue)
        {
            ReadSQLCookie(Cst.SQLCookieGrpElement.DBNull, pCookieElement, out opValue);
        }
        /// <summary>
        /// Lecture de la valeur du {pCookieElement} pour le couple {acteur, hostName} 
        /// </summary>
        /// <param name="pCookieGrpElement"></param>
        /// <param name="pCookieElement"></param>
        /// <param name="pValue">Valeur retour du cookie</param>
        // EG 20200818 [XXXXX] Réduction Lecture CacheOn
        public static void ReadSQLCookie(Cst.SQLCookieGrpElement pCookieGrpElement, string pCookieElement, out string opValue)
        {
            opValue = string.Empty;
            string connectionString = SessionTools.CS;

            //PL 20200803 Add test on CS (when the web app was launched, CS was not yet valued)
            if (!string.IsNullOrEmpty(connectionString))
            {
                //System.Diagnostics.Debug.WriteLine("PL 20200806 ReadSQLCookie - " + DateTime.Now.ToString() + " - " + pCookieElement + " - " + connectionString);

                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(connectionString, "ELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pCookieElement);
                if (pCookieGrpElement != Cst.SQLCookieGrpElement.DBNull)
                    parameters.Add(new DataParameter(connectionString, "GRPELEMENT", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), pCookieGrpElement.ToString());
                parameters.Add(DataParameter.GetParameter(connectionString, DataParameter.ParameterEnum.IDA), SessionTools.Collaborator_IDA);


                string SQLSelect = SQLCst.SELECT + @" VALUE" + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.COOKIE + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + @" ELEMENT=@ELEMENT";
                SQLSelect += SQLCst.AND + @"IDA=@IDA";
                //PL 20110921
                //SQLSelect += SQLCst.AND + @"HOSTNAME=@HOSTNAME";

                if (pCookieGrpElement != Cst.SQLCookieGrpElement.DBNull)
                    SQLSelect += SQLCst.AND + @"GRPELEMENT=@GRPELEMENT";
                else
                    SQLSelect += SQLCst.AND + @"GRPELEMENT" + SQLCst.IS_NULL;

                opValue = DataHelper.ExecuteScalar(connectionString, CommandType.Text, SQLSelect, parameters.GetArrayDbParameter()) as string;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEltName"></param>
        /// <param name="pValue"></param>
        /// <param name="pDefault"></param>
        public static void ReadSQLCookie(string pEltName, out string opValue, string pDefault)
        {
            ReadSQLCookie(pEltName, out opValue);
            if (StrFunc.IsEmpty(opValue))
                opValue = pDefault;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTbl"></param>
        /// <param name="opNbRowsDeleted"></param>
        // EG 20201125 [XXXXX] CAST Datetime2 to DateTime Column (SQLSERVER ONLY : Substract not possible between DateTime2 and Datetime)
        public static void ResetLog(Cst.OTCml_TBL pTbl, out int opNbRowsDeleted)
        {

            int nbDay = 90;
            opNbRowsDeleted = -1;

            string keyDelAttachedDoc = string.Empty;

            string SQLSelect = SQLCst.SELECT + "NBDAYKEEPPROCESSLOG" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LICENSEE.ToString();
            //20070717 FI utilisation de ExecuteScalar pour de cache pour cette Query
            object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, SQLSelect);
            if (null != obj)
                nbDay = Convert.ToInt32(obj);
            string columnName;
            //
            switch (pTbl)
            {
                case Cst.OTCml_TBL.TRACKER_L:
                    nbDay = Math.Min(5, nbDay);
                    columnName = "DTINS";
                    break;
                case Cst.OTCml_TBL.SYSTEM_L:
                    nbDay = Math.Min(60, nbDay);
                    columnName = "DTSYS";
                    break;
                case Cst.OTCml_TBL.PROCESS_L:
                    nbDay = Math.Min(90, nbDay);
                    columnName = "DTSTPROCESS";
                    keyDelAttachedDoc = Cst.OTCml_TBL.PROCESS_L.ToString();
                    break;
                case Cst.OTCml_TBL.RDBMS_L:
                    nbDay = 0;
                    columnName = "DTUPD";
                    break;
                default:
                    throw new NotImplementedException(pTbl + "Not Implemented");
            }
            //
            string sqlWhere = string.Empty;
            if (nbDay > 0)
            {
                /// FI 20200820 [25468] Dates systemes en UTC
                sqlWhere = SQLCst.WHERE + DataHelper.SQLGetDate(SessionTools.CS, true) + " - " + DataHelper.SQLDatetime2ToDateTime(SessionTools.CS, columnName) + " > " + nbDay.ToString();
            }
            //
            if (StrFunc.IsFilled(keyDelAttachedDoc))
            {
                StrBuilder querySelect = new StrBuilder(SQLCst.SELECT);
                querySelect += OTCmlHelper.GetColunmID(pTbl.ToString()) + Cst.CrLf;
                querySelect += SQLCst.FROM_DBO + pTbl.ToString();
                if (StrFunc.IsFilled(sqlWhere))
                    querySelect += sqlWhere;
                //
                StrBuilder deleteDoc = new StrBuilder();
                deleteDoc += SQLCst.DELETE_DBO + Cst.OTCml_TBL.ATTACHEDDOC.ToString() + Cst.CrLf;
                deleteDoc += SQLCst.WHERE + "ID  in (" + querySelect + ")" + Cst.CrLf;
                deleteDoc += SQLCst.AND + "TABLENAME=" + DataHelper.SQLString(keyDelAttachedDoc) + Cst.CrLf;
                //
                DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, deleteDoc.ToString());
            }
            //
            StrBuilder delete = new StrBuilder();
            delete += SQLCst.DELETE_DBO + pTbl.ToString() + Cst.CrLf;
            if (StrFunc.IsFilled(sqlWhere))
                delete += sqlWhere;
            //
            int deletedRows = DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, delete.ToString());
            if (deletedRows >= 0)
                opNbRowsDeleted = deletedRows;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRevision"></param>
        /// <returns></returns>
        static public bool IsDBRevisionEqualOrGreater(int pRevision)
        {
            try
            {
                string databaseVersion = (string)HttpContext.Current.Session["DBVERSION"];
                string delimStr = ".";
                char[] delimiter = delimStr.ToCharArray();
                string[] split = databaseVersion.Split(delimiter);
                int dbRevision = Convert.ToInt32(split[2]);
                return pRevision <= dbRevision;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBuild"></param>
        /// <returns></returns>
        static public bool IsDBBuildEqualOrGreater(int pBuild)
        {
            try
            {
                string databaseVersion = (string)HttpContext.Current.Session["DBVERSION"];
                string delimStr = ".";
                char[] delimiter = delimStr.ToCharArray();
                string[] split = databaseVersion.Split(delimiter);
                int dbBuild = Convert.ToInt32(split[3]);
                return pBuild <= dbBuild;
            }
            catch
            {
                return false;
            }
        }



        #region COL_GET, COL_POST, COL_SERVER
        // ----------------------------------------------------------------------------------------
        // Exemples d'utilisation
        // ----------------------------------------------------------------------------------------
        // Récupère le paramètre id dans l'url tel que http://toto.com/Default.aspx?id=5 
        // strGetId = CUtils.COL_GET["id"]; 
        // Récupère une variable post (typiquement envoyée d'un formumaire avec la méthode post) 
        // strPostNom = CUtils.COL_POST["nom"]; 
        // Récupère une variable serveur 
        // strServerAcceptLanguage = CUtils.COL_SERVER["HTTP_ACCEPT_LANGUAGE"]; 
        // ----------------------------------------------------------------------------------------
        /// <summary> 
        /// Récupère le context actuel 
        /// </summary> 
        /// <returns></returns> 
        public static HttpContext Context
        {
            get { return System.Web.HttpContext.Current; }
        }
        /// <summary> 
        /// Variable HTTP_GET du serveur 
        /// </summary> 
        public static System.Collections.Specialized.NameValueCollection COL_GET
        {
            get { return AspTools.Context.Request.QueryString; }
        }
        /// <summary> 
        /// Variable HTTP_POST du serveur 
        /// </summary> 
        public static System.Collections.Specialized.NameValueCollection COL_POST
        {
            get { return AspTools.Context.Request.Form; }
        }
        /// <summary> 
        /// Variable HTTP_SERVER du serveur 
        /// </summary> 
        public static System.Collections.Specialized.NameValueCollection COL_SERVER
        {
            get { return AspTools.Context.Request.ServerVariables; }
        }
        #endregion

        /// <summary>
        /// Ajoute l'argument IDMenu={pIdMenu} à l'URL et retourne le résultat
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        public static string AddIdMenuOnUrl(string pUrl, string pIdMenu)
        {
            string ret = pUrl;
            //
            string idMenu = "IDMenu=" + pIdMenu;
            if (ret.IndexOf(idMenu) <= 0)
            {
                if (ret.IndexOf("?") <= 0)
                    ret += "?" + idMenu;
                else
                    ret += "&" + idMenu;
            }
            //
            return ret;
        }

        /// <summary>
        /// Ecriture dans les logs de Spheres de l'exception rencontrée lors du traitement d'une requête HTTP 
        /// <para>Toute exception générée dans cette méthode n'est pas tracée</para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="exception"></param>
        /// FI 20210302 [XXXXX] Add Method
        public static void WriteLogException(Object source, Exception exception)
        {
            try
            {
                string url = Context.Request.Url.ToString();
                //Ecriture de l'exception dans le fichier de trace
                AppInstance.TraceManager.TraceError(source, $"Error on URL {url}:{Cst.CrLf}{ExceptionTools.GetMessageAndStackExtended(exception)}");

                //Ecriture de l'exception dans les différents supports (Windows EventLog, Email, XML log, etc)
                ErrorBlock errBlock = new ErrorBlock(exception, SessionTools.AppSession, url);
                WebErrorWriter.Write(errBlock);
            }
            catch
            {
            }
        }

        /// <summary>
        ///  Return 'DNS host name or IP address of the server' - 'IP address of client'
        /// <para>Utilisation de <see cref="HttpContext.Current.Request.Url"></see></para>
        /// </summary>
        /// <returns></returns>
        public static string GetServerAndUserHost()
        {
            //Il s'agit généralement du nom de l'hôte DNS ou de l'adresse IP du serveur
            string hostname = new Uri(HttpContext.Current.Request.Url.ToString()).Host;

            string hostaddress = HttpContext.Current.Request.UserHostAddress;
            if (hostaddress == "::1")
            {
                hostaddress = "127.0.0.1";
            }
            else
            {
                DnsHelper.TryGetIPv4Address(hostaddress, out hostaddress);
            }

            return $"{hostname} - {hostaddress}";
        }

        /// <summary>
        ///  return l'adresse IP du client distant
        /// <para>Utilisation de <see cref="HttpContext.Current.Request.Url"/></para>
        /// </summary>
        /// <returns></returns>
        public static string GetUserHostIP()
        {
            string hostaddress = HttpContext.Current.Request.UserHostAddress;
            if (hostaddress == "::1")
            {
                hostaddress = "127.0.0.1";
            }
            else
            {
                DnsHelper.TryGetIPv4Address(hostaddress, out hostaddress);
                if (hostaddress != HttpContext.Current.Request.UserHostAddress)
                    hostaddress = "IPv4: " + hostaddress + "  IPv6: " + HttpContext.Current.Request.UserHostAddress;
            }
            return hostaddress;
        }



        /// <summary>
        /// Retourne le nom de machine (ou à défaut le UserHostAddress) du client distant
        /// <para>Utilisation de <see cref="HttpContext.Current.Request.Url"/></para>
        /// </summary>
        public static string GetClientMachineName()
        {
            string ret = HttpContext.Current.Request.UserHostAddress;
            try
            {
                ret = DnsHelper.GetMachineName(ret);
            }
            catch { }// Si ca marche pas, on retourne UserHostAddress
            
            return ret;
        }

        /// <summary>
        /// Retoune les informations concernant le Browser
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserInfo()
        {
            string browserInfo = string.Empty;
            browserInfo += HttpContext.Current.Request.Browser.Browser;
            browserInfo += HttpContext.Current.Request.Browser.MajorVersion.ToString() + ".";
            browserInfo += HttpContext.Current.Request.Browser.MinorVersion.ToString() + " - ";
            //PL 20101124
            //browserInfo += "JS=" + HttpContext.Current.Request.Browser.JavaScript.ToString() + " - ";
            if (HttpContext.Current.Request.Browser.EcmaScriptVersion.Major < 1)
            {
                browserInfo += "JS=None" + " - ";
            }
            else
            {
                browserInfo += "JS=" + HttpContext.Current.Request.Browser.EcmaScriptVersion.Major.ToString();
                browserInfo += "." + HttpContext.Current.Request.Browser.EcmaScriptVersion.Minor.ToString();
                if (HttpContext.Current.Request.Browser.EcmaScriptVersion.Build >= 0)
                {
                    browserInfo += "." + HttpContext.Current.Request.Browser.EcmaScriptVersion.Build.ToString();
                    if (HttpContext.Current.Request.Browser.EcmaScriptVersion.Revision >= 0)
                        browserInfo += "." + HttpContext.Current.Request.Browser.EcmaScriptVersion.Revision.ToString();
                }
                browserInfo += " - ";
            }
            //
            browserInfo += "OS=" + HttpContext.Current.Request.Browser.Platform + " - ";
            //
            browserInfo += "CLR=" + HttpContext.Current.Request.Browser.ClrVersion.Major.ToString() + ".";
            browserInfo += HttpContext.Current.Request.Browser.ClrVersion.Minor.ToString() + ".";
            browserInfo += HttpContext.Current.Request.Browser.ClrVersion.Build.ToString();
            browserInfo += HttpContext.Current.Request.Browser.ClrVersion.Revision.ToString();
            return browserInfo;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class CookieData
    {
        public Cst.SQLCookieGrpElement GrpEltName;
        public string EltName;
        public string EltValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEltName"></param>
        /// <param name="pEltValue"></param>
        public CookieData(string pEltName, string pEltValue)
        {
            GrpEltName = Cst.SQLCookieGrpElement.DBNull;
            EltName = pEltName;
            EltValue = pEltValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGrpEltName"></param>
        /// <param name="pEltName"></param>
        /// <param name="pEltValue"></param>
        public CookieData(Cst.SQLCookieGrpElement pGrpEltName, string pEltName, string pEltValue)
        {
            GrpEltName = pGrpEltName;
            EltName = pEltName;
            EltValue = pEltValue;
        }
    }
}

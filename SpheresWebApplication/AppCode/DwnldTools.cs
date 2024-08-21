using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Microsoft.Win32;


using EFS;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.ApplicationBlocks.Data;
using EFS.Permission;

namespace EFS.Spheres.Download
{
    public class DwnldTools
    {
        #region AddBackSlash
        /// <summary>
        /// Retourne le "Path" complété par un "\", si celui-ci est renseigné et non terminé par un "\".
        /// </summary>
        /// <param name="pPath"></param>
        private static string AddBackSlash(string pPath)
        {
            AddBackSlash(ref pPath);
            return pPath;
        }
        /// <summary>
        /// Complète le "Path" par un "\", si celui-ci est renseigné et non terminé par un "\".
        /// </summary>
        /// <param name="pPath"></param>
        private static void AddBackSlash(ref string pPath)
        {
            if ((!String.IsNullOrEmpty(pPath)) && (!pPath.EndsWith(@"\")))
            {
                pPath += @"\";
            }
        }
        #endregion AddBackSlash

        /// <summary>
        /// Retourne la valeur de la colonne DIVERS.S_VALUE (fréquemment un Path) relative à la colonne DIVERS.S_NAME.
        /// </summary>
        /// <param name="asDirectory"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        private static string GetVALUEFromSqlDIVERS(string pS_Name)
        {
            string ret = null;
            IDataReader dr = null;
            try
            {
                string sqlSelect = "select S_VALUE" + Cst.CrLf;
                sqlSelect += "from DIVERS" + Cst.CrLf;
                sqlSelect += "where S_NAME=" + DataHelper.SQLString(pS_Name);
                dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlSelect);
                if (dr.Read())
                    ret = AddBackSlash(dr["S_VALUE"].ToString());
            }
            catch (Exception)
            {
                ret = null;
                throw;
            }
            finally
            {
                if (dr != null)
                    dr.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// Retourne le Path relatif à l'utilisateur connecté via les tables DIVERS et CLIENTS.
        /// </summary>
        /// <param name="pSuffix"></param>
        /// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        private static string GetPath_Customer(string pSubPath)
        {
            string client_Path = null;
            string repEurobbs = "*";    //"*" afin de générer une erreur si aucun folder n'est paramétré

            IDataReader dr = null;
            try
            {
                string root = GetVALUEFromSqlDIVERS("ROOT_CLIENT_PATH");
                string sqlSelect = "select c.REP_EURO_BBS" + Cst.CrLf;
                sqlSelect += "from CLIENTS c" + Cst.CrLf;
                sqlSelect += "inner join ACTOR a on a.EXTLLINK=c.CD_CLIENT and a.IDA=" + SessionTools.Collaborator_IDA.ToString();
                dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlSelect);
                if (dr.Read())
                    repEurobbs = dr["REP_EURO_BBS"].ToString();

                client_Path = root + repEurobbs;
                if (StrFunc.IsFilled(pSubPath))
                    client_Path += @"_" + pSubPath;
            }
            catch (Exception) 
            {
                client_Path = null;
                throw; 
            }
            finally
            {
                if (dr != null)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }
            return AddBackSlash(client_Path);
        }
        /// <summary>
        /// Retourne le Path "Partagé" pour les utilisateurs.
        /// </summary>
        /// <returns></returns>
        private static string GetPath_SharedCustomer()
        {
            string path = GetVALUEFromSqlDIVERS("ROOT_CLIENT_PATH");
            return AddBackSlash(path) + @"(SHARED)\";
        }
        /// <summary>
        /// Retourne le Path dit  "Commun".
        /// </summary>
        /// <returns></returns>
        private static string GetPath_Common()
        {
            return GetVALUEFromSqlDIVERS("ROOT_COMMUN_PATH");
        }
        /// <summary>
        /// Retourne le Path relatif aux collaborateurs EFS via la table DIVERS.
        /// </summary>
        /// <returns></returns>
        private static string GetPath_PrivateEFS()
        {
            return GetVALUEFromSqlDIVERS("ROOT_INTERNE_PATH");
        }
        /// <summary>
        /// Retourne le path "physique" relatif:
        /// <para>- Interprétation du caractère "~"</para>
        /// <para>- Transcription de KeyWord (i.e EFS Portal)</para>
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static string GetPath(Page pPage, string pPath)
        {
            return GetPath(pPage, pPath, string.Empty);
        }
        /// <summary>
        /// Retourne le path "physique" relatif:
        /// <para>- Interprétation du caractère "~"</para>
        /// <para>- Transcription de KeyWord (i.e EFS Portal)</para>
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pPath"></param>
        /// <param name="pSubPath"></param>
        /// <returns></returns>
        public static string GetPath(Page pPage, string pPath, string pSubPath)
        {
            string sFolder = string.Empty;

            #region IsSoftwarePortal()
            if (Software.IsSoftwarePortal())
            {
                try
                {
                    switch (pPath)
                    {
                        case "SUPPORT_PRODUCT"://20090316 PL
                            sFolder = DwnldTools.GetVALUEFromSqlDIVERS("ROOT_SUPPORT_PATH");
                            break;
                        case "PRIVATE":
                        case "PARAM"://20090316 PL  Obsolete, remplacé par PRIVATE
                        case "DWNLD"://20090313 PL  A priori obsolete
                            sFolder = DwnldTools.GetPath_Customer(null);
                            break;
                        case "PRIVATEEFS":
                        case "INTERNE"://20090316 PL  Obsolete, remplacé par PRIVATEEFS
                            sFolder = DwnldTools.GetPath_PrivateEFS();
                            break;
                        case "COMMUN"://20090313 PL  A priori obsolete
                            sFolder = DwnldTools.GetPath_Common();
                            break;
                        case "SHARED"://PL 20141008 New features for Shortcut (.lnk)
                            sFolder = DwnldTools.GetPath_SharedCustomer();
                            break;
                        default:
                            //20090313 PL  New Feature
                            if (StrFunc.IsEmpty(pPath))
                            {
                                //Aucun "path --> utilisation du path client par défaut + _PRIVATE 
                                sFolder = DwnldTools.GetPath_Customer("_PRIVATE");
                            }
                            else
                            {
                                if (pPath.ToLower().StartsWith(@"c:\feed\feed_data\efs\product\"))
                                {
                                    //Folder SUPPORT (cas traité en dur temporairement, à l'image de P=PRODUCT)
                                    sFolder = DwnldTools.GetVALUEFromSqlDIVERS("ROOT_SUPPORT_PATH");
                                    sFolder += pPath.Substring(@"c:\feed\feed_data\efs\product\".Length);
                                }
                                else
                                {
                                    if (pPath.IndexOf(@"~") > 0)
                                    {
                                        //ex.: P=SUPPORT_PRODUCT~Spheres\OTCml\Release\
                                        string keyword = pPath.Substring(0, pPath.IndexOf(@"~"));
                                        string detail = pPath.Substring(pPath.IndexOf(@"~") + 1);
                                        switch (keyword)
                                        {
                                            case "SUPPORT_PRODUCT":
                                                sFolder = DwnldTools.GetVALUEFromSqlDIVERS("ROOT_SUPPORT_PATH");
                                                break;
                                            case "PRIVATE":
                                                sFolder = DwnldTools.GetPath_Customer(null);
                                                break;
                                            case "PRIVATEEFS":
                                                sFolder = DwnldTools.GetPath_PrivateEFS();
                                                break;
                                            default:
                                                sFolder = DwnldTools.GetPath_Customer(keyword + @"_PRIVATE");
                                                break;
                                        }
                                        sFolder += detail;
                                    }
                                    else
                                    {
                                        //Le "path" n'est pas un réel path windows, mais une constante (ex. EUROSYS, OTCML, ...) 
                                        // ==> donc utilisation du path client par défaut + _{CONSTANT}_PRIVATE 
                                        sFolder = DwnldTools.GetPath_Customer(pPath + @"_PRIVATE");
                                    }
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("The RegisterPermission failed");
                    Console.WriteLine(ex.ToString());
                    Console.Out.WriteLine("Press the Enter key to exit.");
                }
            }
            #endregion IsSoftwarePortal()
            else
            #region !IsSoftwarePortal()
            {
                // FI 20211215 [25904] Seuls les dossiers enfants du root sont autorisés
                // La MapPath propriété contient potentiellement des informations sensibles sur l’environnement d’hébergement. La valeur de retour ne doit pas être affichée aux utilisateurs.
                sFolder = pPage.MapPath(pPath);
                string rootPath = pPage.MapPath("~"); //Server.MapPath("~") returns the physical path to the root of the application
                if ((!sFolder.ToUpper().StartsWith(rootPath.ToUpper())) || (sFolder.ToUpper() == rootPath.ToUpper()))
                    throw new HttpException($"unauthorized path: {pPath}");
            }
            #endregion !IsSoftwarePortal()

            sFolder = AddBackSlash(sFolder.ToUpper()) + pSubPath;

            return AddBackSlash(sFolder);
        }
    }
}

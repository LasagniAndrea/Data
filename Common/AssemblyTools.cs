using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using System.Threading;
using System.IO;

using EFS.ACommon;

namespace EFS.Common
{
    // EG 20221012 [XXXXX] Refactoring de l'affichage des composants depuis la fenêtre à propos.
    public enum ComponentTypeEnum
    {
        System,
        Oracle,
        EFS,
        External,
        Other,
        Misc,
        Temporary,
    }


    /// <summary>
    /// 
    /// </summary>
    /// FI 20190822 [24861] Add
    public sealed class AssemblyTools
    {
        /// <summary>
        ///  Retourne les DLL (.net) présentes dans le répertoire Root
        ///  <para>s'il sagit d'un service, la liste des assemblies contient en 1er lieu l'exe</para>
        /// </summary>
        /// <param name="pAppInstance"></param>
        /// FI 20190822 [24861] Add
        /// FI 20200709 [25473] Ajout de l'exe qd il s'agit d'un service 
        /// FI 20210108 [XXXXX] Gestion de l'application web
        private static Assembly[] GetAssemblyLibrary(AppInstance pAppInstance)
        {
            if (null == pAppInstance)
                throw new ArgumentNullException("Appinstance is null");

            // FI 20210108 [XXXXX] rootFolder bin sur appli web
            string rootFolder = pAppInstance.GetType().Equals(typeof(AppInstanceService)) ? pAppInstance.AppRootFolder : pAppInstance.AppRootFolder + @"\bin";

            Assembly[] ret = null;

            List<FileInfo> lstFileInfos = new List<FileInfo>();

            if (pAppInstance.GetType().Equals(typeof(AppInstanceService)))
            {
                // FI 20210223 [XXXXX] Tous les fichiers exe sont désormais ajoutés 
                string FilterFileExe = $"{Software.Name}*.exe";
                lstFileInfos.AddRange(Directory.GetFiles(rootFolder, FilterFileExe).Select(f => new FileInfo(f))
                    .OrderBy(x =>
                    {
                        // FI 20210223 [XXXXX] l'exe qui porte le nom de pAppInstance.AppName en priorité
                        string retOrder = x.Name;
                        if (x.Name.StartsWith(pAppInstance.AppName))
                            retOrder = "." + x.Name;
                        else
                            retOrder = x.Name.ToString();
                        return retOrder;
                    }));
            }

            string FilterFile = "*.dll";
            lstFileInfos.AddRange(Directory.GetFiles(rootFolder, FilterFile)
                    .Where(f => !f.EndsWith("SpheresServicesMessage.dll")).Select(f => new FileInfo(f))
                    .OrderBy(x =>
                    {
                        string retOrder;
                        if (AppInstance.IsSpheresWebApp(pAppInstance.AppNameVersion))
                        {
                            // FI 20210223 [XXXXX] sur l'application web => EFS.Spheres.dll en priorité
                            if (x.Name.EndsWith($"{Software.Name}.dll"))
                                retOrder = "." + x.Name;
                            else
                                retOrder = x.Name;
                        }
                        else
                        {
                            // FI 20210223 [XXXXX] sur les services => Spheres*.dll en priorité
                            if (x.Name.StartsWith($"{Software.Name}"))
                                retOrder = "." + x.Name;
                            else
                                retOrder = x.Name;
                        }
                        return retOrder;
                    }));

            List<Assembly> lstAssembly = new List<Assembly>(lstFileInfos.Count);
            foreach (FileInfo item in lstFileInfos)
            {
                try
                {
                    lstAssembly.Add(Assembly.ReflectionOnlyLoadFrom(item.FullName));
                }
                catch (BadImageFormatException)
                {
                    //assemblyFile n’est pas un assembly valide.
                }
                catch (FileLoadException)
                {
                    //assemblyFile a été trouvé, mais impossible de le charger.
                }
            }

            ret = lstAssembly.ToArray();

            return ret;
        }

        /// <summary>
        /// Donne une description des assemblies {pAssemblies} avec des regroupements selon ComponentTypeEnum
        /// </summary>
        /// <param name="pAssemblies"></param>
        /// <returns></returns>
        /// FI 20190822 [24861] Add
        /// FI 20210624 [XXXXX] Add FileVersion for each assembly
        /// EG 20221012 [XXXXX] Refactoring de l'affichage des composants depuis la fenêtre à propos.
        private static Dictionary<ComponentTypeEnum, string> GetAssemblies(Assembly[] pAssemblies)
        {
            Dictionary<ComponentTypeEnum, string> ret = new Dictionary<ComponentTypeEnum, string>();

            #region GetAssemblies()
            string system = string.Empty;   //mscorlib, System, vjs
            string oracle = string.Empty;   //Oracle
            string efs = string.Empty;      //EFS, EfsML, Spheres
            string addefs = string.Empty;   //FlyDocLibrary, FONet.net, Ionic.Zip, itextsharp, WebPageSecurity, XmlDiffPatch, NodaTime, CsvHelper, ClosedXML,DocumentFormat.OpenXml
            string temporary = string.Empty;//Temporary ASP.NET Files
            string other = string.Empty;

            foreach (System.Reflection.Assembly a in pAssemblies)
            {
                string fn = a.FullName;
                string assemblyDescriptionItem = $"Assembly Identity={fn}";
                
                // fileVersion from AssemblyFileVersionAttribute
                string fv = string.Empty;
                try
                {
                    if (a.ReflectionOnly)
                    {
                        //Cas où la l'assembly est chargée dans le contexte de réflexion uniquement 
                        CustomAttributeData attribData = GetAssemblyAttributeData<AssemblyFileVersionAttribute>(a);
                        if (null != attribData)
                            fv = (string)attribData.ConstructorArguments[0].Value;
                    }
                    else
                    {
                        // cas où l'assembly est chargée dans le contexte d'exécution
                        AssemblyFileVersionAttribute aFileversionAttrib = GetAssemblyAttribute<AssemblyFileVersionAttribute>(a);
                        if (null != aFileversionAttrib)
                            fv = aFileversionAttrib.Version;
                    }
                }
                catch { fv = string.Empty; }
                if (StrFunc.IsFilled(fv))
                    assemblyDescriptionItem += $", FileVersion={fv}"; 


                // InformationalVersionAttribute from AssemblyInformationalVersionAttribute
                string iv = string.Empty;
                try
                {
                    if (a.ReflectionOnly)
                    {
                        //Cas où la l'assembly est chargée dans le contexte de réflexion uniquement 
                        CustomAttributeData attribData = GetAssemblyAttributeData<AssemblyInformationalVersionAttribute>(a);
                        if (null != attribData)
                            iv = (string)attribData.ConstructorArguments[0].Value;
                    }
                    else
                    {
                        // cas où l'assembly est chargée dans le contexte d'exécution
                        AssemblyInformationalVersionAttribute aFileversionAttrib = GetAssemblyAttribute<AssemblyInformationalVersionAttribute>(a);
                        if (null != aFileversionAttrib)
                            iv = aFileversionAttrib.InformationalVersion;
                    }
                }
                catch { iv = string.Empty; }
                if (StrFunc.IsFilled(iv))
                    assemblyDescriptionItem += $", InformationalVersion={iv}";

                // codebase
                string cd = string.Empty;
                try
                {
                    cd = a.CodeBase;
                }
                catch { cd = string.Empty; }
                string codeBaseDescriptionItem = StrFunc.IsFilled(cd) ? string.Format("...Codebase={0}", cd) : string.Empty;

                if (fn.StartsWith("mscorlib") || fn.StartsWith("System") || fn.StartsWith("vjs"))
                {
                    system += new string(' ', 0 * 2) + assemblyDescriptionItem + "\r\n";
                    if (StrFunc.IsFilled(codeBaseDescriptionItem))
                        system += new string(' ', 1 * 2) + codeBaseDescriptionItem + "\r\n";
                }
                else if (fn.StartsWith("Oracle"))
                {
                    oracle += new string(' ', 0 * 2) + assemblyDescriptionItem + "\r\n";
                    if (StrFunc.IsFilled(codeBaseDescriptionItem))
                        oracle += new string(' ', 1 * 2) + codeBaseDescriptionItem + "\r\n";
                }
                else if (fn.StartsWith("EFS") || fn.StartsWith("EfsML") || fn.StartsWith("Spheres")) // FI 20210223 [XXXXX] Spheres est classé avec EFS
                {
                    efs += new string(' ', 0 * 2) + assemblyDescriptionItem + "\r\n";
                    if (StrFunc.IsFilled(codeBaseDescriptionItem))
                        efs += new string(' ', 1 * 2) + codeBaseDescriptionItem + "\r\n";
                }
                else if (fn.StartsWith("FlyDocLibrary") || fn.StartsWith("FONet")
                    || fn.StartsWith("Ionic") || fn.StartsWith("itextsharp")
                    || fn.StartsWith("WebPageSecurity") || fn.StartsWith("XmlDiffPatch")
                    || fn.StartsWith("NodaTime") || fn.StartsWith("CsvHelper")
                    || fn.StartsWith("ClosedXML") || fn.StartsWith("DocumentFormat.OpenXml")
                    || fn.StartsWith("Amqp.Net") || fn.StartsWith("org.apache") //FI 20210615 [XXXXX] add
                    )
                {
                    addefs += new string(' ', 0 * 2) + assemblyDescriptionItem + "\r\n";
                    if (StrFunc.IsFilled(codeBaseDescriptionItem))
                        addefs += new string(' ', 1 * 2) + codeBaseDescriptionItem + "\r\n";
                }
                else if (cd.IndexOf("Temporary ASP.NET Files") > 0)
                {
                    temporary += new string(' ', 0 * 2) + assemblyDescriptionItem + "\r\n";
                    if (StrFunc.IsFilled(codeBaseDescriptionItem))
                        temporary += new string(' ', 1 * 2) + codeBaseDescriptionItem + "\r\n";
                }
                else
                {
                    other += new string(' ', 0 * 2) + assemblyDescriptionItem + "\r\n";
                    if (StrFunc.IsFilled(codeBaseDescriptionItem))
                        other += new string(' ', 1 * 2) + codeBaseDescriptionItem + "\r\n";
                }
            }
            #endregion

            if (!String.IsNullOrEmpty(system))
            {
                ret.Add(ComponentTypeEnum.System, system);
            }
            if (!String.IsNullOrEmpty(oracle))
            {
                ret.Add(ComponentTypeEnum.Oracle, oracle);
            }
            if (!String.IsNullOrEmpty(efs))
            {
                ret.Add(ComponentTypeEnum.EFS, efs);
            }
            if (!String.IsNullOrEmpty(addefs))
            {
                ret.Add(ComponentTypeEnum.External, addefs);
            }
            if (!String.IsNullOrEmpty(other))
            {
                ret.Add(ComponentTypeEnum.Other, other);
            }
            if (!String.IsNullOrEmpty(temporary))
            {
                ret.Add(ComponentTypeEnum.Temporary, temporary);
            }

            return ret;
        }

        /// <summary>
        /// Retourne la liste des assemblies {pAssemblies} sous forme de string
        /// </summary>
        /// <param name="pAssemblies"></param>
        /// <returns></returns>
        /// FI 20190822 [24861] Add
        /// EG 20221012 [XXXXX] Refactoring de l'affichage des composants depuis la fenêtre à propos.
        private static string ConvertAssembliesToString(Dictionary<ComponentTypeEnum, string> pAssemblies)
        {
            string infos = string.Empty;

            if (pAssemblies.TryGetValue(ComponentTypeEnum.System, out string componentsList))
            {
                infos += "System:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (pAssemblies.TryGetValue(ComponentTypeEnum.Oracle, out componentsList))
            {
                infos += "Oracle:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (pAssemblies.TryGetValue(ComponentTypeEnum.EFS, out componentsList))
            {
                infos += "EFS:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (pAssemblies.TryGetValue(ComponentTypeEnum.External, out componentsList))
            {
                infos += "External:" + Cst.CrLf + componentsList + Cst.CrLf;
            }
            if (pAssemblies.ContainsKey(ComponentTypeEnum.Other) || pAssemblies.ContainsKey(ComponentTypeEnum.Misc))
            {
                if (pAssemblies.TryGetValue(ComponentTypeEnum.Other, out componentsList))
                {
                    infos += "Other:" + Cst.CrLf + componentsList + Cst.CrLf;
                }
                if (pAssemblies.TryGetValue(ComponentTypeEnum.Misc, out componentsList))
                {
                    infos += componentsList + Cst.CrLf;
                }
            }
            if (pAssemblies.TryGetValue(ComponentTypeEnum.Temporary, out componentsList))
            {
                infos += "Temporary:" + Cst.CrLf + componentsList + Cst.CrLf;
            }

            return infos;
        }

        /// <summary>
        /// Convertie la liste des assemblies {pAssemblies}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pAssemblies"></param>
        /// <returns></returns>
        private static T ConvertAssemblies<T>(Dictionary<ComponentTypeEnum, string> pAssemblies) where T : class
        {
            T ret;
            if (typeof(T).Equals(typeof(String)))
            {
                ret = (T)(ConvertAssembliesToString(pAssemblies) as object);
            }
            else if (typeof(T).Equals(typeof(Dictionary<ComponentTypeEnum, string>)))
            {
                ret = (T)(pAssemblies as object);
            }
            else
                throw new InvalidProgramException(StrFunc.AppendFormat("Type:{0} not supported", typeof(T).ToString()));

            return ret;
        }

        /// <summary>
        ///  Retourne la liste des assemblies chargées dans le contexte d'éxécution du Thread en cours
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// FI 20190822 [24861] Add
        public static T GetDomainAssemblies<T>() where T : class
        {
            Assembly[] myAssemblies = Thread.GetDomain().GetAssemblies();
            Dictionary<ComponentTypeEnum, string> ass = GetAssemblies(myAssemblies);
            return ConvertAssemblies<T>(ass);
        }

        /// <summary>
        ///  Retourne la liste des assemblies DLL (.net) présentes dans le répertoire Root.
        ///  <para>s'il sagit d'un service, la liste des assemblies contient en 1er lieu l'exe</para>
        /// </summary>
        /// <param name="pAppInstance"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// FI 20190822 [24861] Add
        public static T GetAppInstanceAssemblies<T>(AppInstance pAppInstance) where T : class
        {
            Assembly[] myAssemblies = GetAssemblyLibrary(pAppInstance);
            Dictionary<ComponentTypeEnum, string> ass = GetAssemblies(myAssemblies);
            return ConvertAssemblies<T>(ass);
        }

        /// <summary>
        /// Retourne l'attribut personnalisé de type T attribué à l'assembly {ass}
        /// <para>Cette méthode ne s'applique pas si l'assemblie est chargée dans le contexte de reflexion uniquement</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ass"></param>
        /// <returns></returns>
        /// FI 20210623 [XXXXX] Add Method
        public static T GetAssemblyAttribute<T>(Assembly ass) where T : Attribute
        {
            object[] attributes = ass.GetCustomAttributes(typeof(T), false);
            if (attributes == null || attributes.Length == 0)
                return null;
            return attributes.OfType<T>().SingleOrDefault();
        }

        /// <summary>
        /// Retourne des informations de l'attribut personnalisé de type T attribué à l'assembly {ass}, en tant qu'objets System.Reflection.CustomAttributeData.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ass"></param>
        /// <returns></returns>
        /// FI 20210623 [XXXXX] Add Method
        public static CustomAttributeData GetAssemblyAttributeData<T>(System.Reflection.Assembly ass) where T : Attribute
        {
            CustomAttributeData attributeData = ass.GetCustomAttributesData().Where(x => x.AttributeType.Equals(typeof(T))).FirstOrDefault();
            return attributeData;
        }
    }
}

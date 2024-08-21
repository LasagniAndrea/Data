//
using EFS.ACommon;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EFS.Common.Log
{

    /// <summary>
    /// Délégué pour interprétation du  "."  ou "~"  éventuellement présent dans chemin d'accès à un fichier ou à un répertoire
    /// </summary>
    /// <param name="pPathFile">Chemin d'accès à un fichier ou à un répertoire</param>
    /// PM 20160104 [POC MUREX] Add
    public delegate string GetFilepath(string pPathFile);

    /// <summary>
    ///  Classe de base pour la gestion des traces
    /// </summary>
    public abstract class TraceManagerBase
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly GetFilepath _getfilePath;

        /// <summary>
        ///
        /// </summary>
        protected string _fileNameSuffixe = null;

        /// <summary>
        /// Obtient l'éventuel suffixe appliqué au fichiers de trace 
        /// </summary>
        protected string FileNameSuffixe
        {
            set
            {
                string suffixe = value;
                if (StrFunc.IsFilled(suffixe))
                {
                    suffixe = suffixe.Replace(@"%", string.Empty);
                    suffixe = suffixe.Replace(@"\", string.Empty);
                    suffixe = suffixe.Replace(@"/", string.Empty);
                    suffixe = suffixe.Replace(@":", string.Empty);
                    suffixe = suffixe.Replace(@"*", string.Empty);
                    suffixe = suffixe.Replace(@"|", string.Empty);
                    suffixe = suffixe.Replace(@"?", string.Empty);
                    suffixe = suffixe.Replace(@"<", string.Empty);
                    suffixe = suffixe.Replace(@">", string.Empty);
                    suffixe = suffixe.Replace("\"", string.Empty);
                }
                _fileNameSuffixe = suffixe;
            }
        }
        #endregion Members

        #region constructor
        public TraceManagerBase(string pFileNameSuffixe)
            : this(pFileNameSuffixe, null)
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileNameSuffixe"></param>
        /// <param name="pGetFilePath"></param>
        public TraceManagerBase(string pFileNameSuffixe, GetFilepath pGetFilePath)
        {
            FileNameSuffixe = pFileNameSuffixe;
            _getfilePath = pGetFilePath;
            if (_getfilePath == default)
                _getfilePath = GetFilePathDefault;
        }
        #endregion

        #region Method
        /// <summary>
        /// Fonction qui permet d'interpréter le chemin lorsqu'il existe un "."  ou "~"
        /// <para>. est emplacé par  {AppDomain.CurrentDomain.BaseDirectory}</para>
        /// <para>~ est remplacé par {AppDomain.CurrentDomain.BaseDirectory}</para>
        /// <para>
        /// </para>
        /// </summary>
        /// <param name="pPathFile"></param>
        /// <returns></returns>
        /// PM [20200810] LOG: ajout GetFilePathDefault
        private static string GetFilePathDefault(string pPathFile)
        {
            string ret = pPathFile;

            if (false == File.Exists(pPathFile))
            {
                if (StrFunc.ContainsIn(pPathFile, @".\"))
                {
                    ret = ret.Replace(@".\", AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\");
                }
                if (StrFunc.ContainsIn(pPathFile, @"~\"))
                {
                    ret = ret.Replace(@"~\", AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\");
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le nom de la source
        /// </summary>
        /// <param name="pSource"></param>
        /// <returns></returns>
        protected static string GetObjectSourceName(Object pSource)
        {
            string ret = "no object source";
            if (null != pSource)
            {
                ret = pSource.GetType().Name;
                /// PM [20200810] LOG:
                // Dans le cas d'un objet string, on retourne directement le contenu de la string
                if (ret.Equals("String"))
                {
                    ret = (string)pSource;
                }
            }
            return ret;
        }

        /// <summary>
        /// Change the file name of the listeners by adding a suffixe
        /// </summary>
        /// <param name="pSource"></param>
        protected void ModifyListenersFile(TraceSource pSource)
        {

            ConfigurationSection diagnosticsSection = (ConfigurationSection)ConfigurationManager.GetSection("system.diagnostics");
            if (null != diagnosticsSection)
            {
                ConfigurationElementCollection sharedListeners = (ConfigurationElementCollection)diagnosticsSection.ElementInformation.Properties["sharedListeners"].Value;
                if (null != sharedListeners)
                    ChangeSourceListeners(pSource, sharedListeners);

                // Recherche de l'élément Source
                ConfigurationElementCollection sources = (ConfigurationElementCollection)diagnosticsSection.ElementInformation.Properties["sources"].Value;

                ConfigurationElement source = null;
                foreach (ConfigurationElement s in sources)
                {
                    if (null != s.ElementInformation.Properties["name"])
                    {
                        if (0 == pSource.Name.CompareTo(s.ElementInformation.Properties["name"].Value))
                        {
                            source = s;
                            break;
                        }
                    }
                }

                if (null != source)
                {
                    // Parcours des listeners propres à la source
                    ConfigurationElementCollection listeners = (ConfigurationElementCollection)source.ElementInformation.Properties["listeners"].Value;
                    if (null != listeners)
                        ChangeSourceListeners(pSource, listeners);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pListeners"></param>
        private void ChangeSourceListeners(TraceSource pSource, ConfigurationElementCollection pListeners)
        {
            if (null == pSource)
                throw new ArgumentNullException(nameof(pSource));
            if (null == pListeners)
                throw new ArgumentNullException(nameof(pListeners));

            // Parcours des listeners
            foreach (ConfigurationElement listener in pListeners)
            {
                string listenerName = (string)listener.ElementInformation.Properties["name"].Value;
                string listenerType = (string)listener.ElementInformation.Properties["type"].Value;
                string listenerInitializeData = (string)listener.ElementInformation.Properties["initializeData"].Value;
                if ((null != listenerType) && (null != listenerInitializeData))
                {
                    // Vérification qu'il s'agit d'un listener de la source
                    if (null != pSource.Listeners[listenerName])
                    {
                        #region traceOutputOptions
                        TraceOptions traceOptions = TraceOptions.None;
                        if (null != listener.ElementInformation.Properties["traceOutputOptions"])
                        {
                            traceOptions = (TraceOptions)listener.ElementInformation.Properties["traceOutputOptions"].Value;
                        }
                        #endregion traceOutputOptions
                        // Création du nouveau listener
                        TraceListener newListener = null;

                        if (0 == listenerType.CompareTo(typeof(System.Diagnostics.DelimitedListTraceListener).FullName))
                        {
                            string newFileName = GetFile(listenerInitializeData);
                            
                            // FI 20220805 [XXXXX] Génération du repertoire du fichier de trace s'il n'existe pas
                            string directoryName = Path.GetDirectoryName(newFileName);
                            if (false == Directory.Exists(directoryName))
                                Directory.CreateDirectory(directoryName);

                            DelimitedListTraceListener newDelimitedListener = new DelimitedListTraceListener(newFileName, listenerName)
                            {
                                TraceOutputOptions = traceOptions
                            };

                            #region delimiter
                            if (null != listener.ElementInformation.Properties["delimiter"])
                            {
                                string listenerDelimiter = (string)listener.ElementInformation.Properties["delimiter"].Value;
                                newDelimitedListener.Delimiter = listenerDelimiter;
                            }
                            #endregion delimiter
                            
                            newListener = newDelimitedListener;
                        }
                        else if (0 == listenerType.CompareTo(typeof(System.Diagnostics.TextWriterTraceListener).FullName))
                        {
                            string newFileName = GetFile(listenerInitializeData);

                            newListener = new TextWriterTraceListener(newFileName, listenerName)
                            {
                                TraceOutputOptions = traceOptions
                            };
                        }
                        else if (0 == listenerType.CompareTo(typeof(System.Diagnostics.XmlWriterTraceListener).FullName))
                        {
                            string newFileName = GetFile(listenerInitializeData);

                            newListener = new XmlWriterTraceListener(newFileName, listenerName)
                            {
                                TraceOutputOptions = traceOptions
                            };
                        }
                        if (null != newListener)
                        {
                            // Remplacement du listener
                            pSource.Listeners.Remove(listenerName);
                            pSource.Listeners.Add(newListener);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Les dernières trace sont renommées. Les anciennes traces de plus de 30 jours sont supprimées.
        /// </summary>
        protected void PrepareTrace()
        {
            ConfigurationSection diagnosticsSection = (ConfigurationSection)ConfigurationManager.GetSection("system.diagnostics");
            if (null != diagnosticsSection)
            {
                ConfigurationElementCollection sharedListeners = (ConfigurationElementCollection)diagnosticsSection.ElementInformation.Properties["sharedListeners"].Value;
                if ((null != sharedListeners) && (sharedListeners.Count > 0))
                {
                    // FI 20220721 [XXXXX] call
                    DeleteOlderFiles(sharedListeners);
                    RenameListenersFile(sharedListeners);
                }

                ConfigurationElementCollection sources = (ConfigurationElementCollection)diagnosticsSection.ElementInformation.Properties["sources"].Value;
                if ((null != sources) && (sources.Count > 0))
                {
                    foreach (ConfigurationElement source in sources)
                    {
                        ConfigurationElementCollection listeners = (ConfigurationElementCollection)source.ElementInformation.Properties["listeners"].Value;
                        // FI 20220721 [XXXXX] call
                        DeleteOlderFiles(listeners);
                        RenameListenersFile(listeners);
                    }
                }
            }
        }

        /// <summary>
        /// Renome les précédents fichiers de Traces (Ajout de la date et de l'heure)
        /// </summary>
        /// <param name="pListeners"></param>
        private void RenameListenersFile(ConfigurationElementCollection pListeners)
        {
            foreach (ConfigurationElement listener in pListeners)
            {
                string listenerType = (string)listener.ElementInformation.Properties["type"].Value;
                string listenerInitializeData = (string)listener.ElementInformation.Properties["initializeData"].Value;
                if ((null != listenerType) && (null != listenerInitializeData))
                {
                    if ((0 == listenerType.CompareTo(typeof(DelimitedListTraceListener).FullName))
                        || (0 == listenerType.CompareTo(typeof(TextWriterTraceListener).FullName))
                        || (0 == listenerType.CompareTo(typeof(System.Diagnostics.XmlWriterTraceListener).FullName))
                        )
                    {
                        string fileName = GetFile(listenerInitializeData);
                        //
                        FileInfo traceFile = new FileInfo(fileName);
                        if (traceFile.Exists)
                        {
                            string newFilename = traceFile.DirectoryName.Trim();
                            if (false == newFilename.EndsWith("\\"))
                            {
                                newFilename += "\\";
                            }
                            newFilename = newFilename + traceFile.LastWriteTime.ToString("yyyyMMddHHmmss") + "_" + traceFile.Name;
                            File.Delete(newFilename);
                            traceFile.MoveTo(newFilename);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete files older than 30 days
        /// </summary>
        /// <param name="pListeners"></param>
        /// FI 20220721 [XXXXX] Add
        private void DeleteOlderFiles(ConfigurationElementCollection pListeners)
        {
            foreach (ConfigurationElement listener in pListeners)
            {
                string listenerType = (string)listener.ElementInformation.Properties["type"].Value;
                string listenerInitializeData = (string)listener.ElementInformation.Properties["initializeData"].Value;
                if ((null != listenerType) && (null != listenerInitializeData))
                {
                    if ((0 == listenerType.CompareTo(typeof(System.Diagnostics.DelimitedListTraceListener).FullName))
                        || (0 == listenerType.CompareTo(typeof(System.Diagnostics.TextWriterTraceListener).FullName))
                        || (0 == listenerType.CompareTo(typeof(System.Diagnostics.XmlWriterTraceListener).FullName))
                        )
                    {
                        string file = GetFile(listenerInitializeData);

                        string filename = Path.GetFileName(file);
                        string directory = Path.GetDirectoryName(file);

                        if (StrFunc.IsFilled(directory) && Directory.Exists(directory))
                        {
                            Directory.GetFiles(directory)
                            .Select(f => new FileInfo(f))
                            .Where(f => (f.LastAccessTime < DateTime.Now.AddDays(-30)) &&
                                        (f.Name.EndsWith(filename)))
                            .ToList()
                            .ForEach(f =>
                            {
                                try { f.Delete(); }
                                catch (Exception) { }
                            });
                        }
                    }
                }
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFile"></param>
        /// <returns></returns>
        private string GetFile(string pFile)
        {
            string ret = pFile;

            if (null != _getfilePath)
            {
                ret = _getfilePath(ret);
            }

            ret = SystemIOTools.AddFileNameSuffixe(ret, _fileNameSuffixe);

            return ret;
        }


        #endregion Method
    }
}
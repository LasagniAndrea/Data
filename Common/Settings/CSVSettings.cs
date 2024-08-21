using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;

namespace EFS.Common
{
    /*------------------------------------------------------------------------------------*/
    /* Classes de configuration des paramètrages d'exportation CSV dans un fichier config */
    /*------------------------------------------------------------------------------------*/

    #region CsvPatternType
    /// <summary>
    /// Attribut pour les Custom Patterns (formats personnalisés)
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class CsvPatternType : Attribute
    {
        // Regroupement par type pour alimentation DDL
        public string @type;
        // Format de substitution à celui par défaut
        public string @format;
    }
    #endregion CsvPatternType
    #region CsvPattern
    /// <summary>
    /// Enumeration des formats de conversion pour l'exportation en CSV
    /// Cette liste n'est pas exhaustive
    /// </summary>
    // EG 20190411 [ExportFromCSV]
    // EG 20190419 [ExportFromCSV] Upd
    public enum CsvPattern
    {
        [CsvPatternType(@type = "datetime", @format = "d")]
        date,
        [CsvPatternType(@type = "datetime", @format = "yyyy-MM-dd")]
        isoDate,
        [CsvPatternType(@type = "datetime", @format = "G")]
        datetime,
        [CsvPatternType(@type = "datetimeoffset")]
        datetimeOffset,
        [CsvPatternType(@type = "datetime", @format = "o")]
        isoDatetime,
        [CsvPatternType(@type = "datetimeoffset", @format = DtFunc.FmtISOLongDateTime + "zzz")]
        isoDatetimeOffset,
        [CsvPatternType(@type = "datetimeoffset", @format = DtFunc.FmtTZISOLongDateTime)]
        utcDatetimeOffset,
        [CsvPatternType(@type = "decimal", @format = "#0.00;-#0.00")]
        twoFixedDecimal,
        [CsvPatternType(@type = "integer", @format = "#;-#")]
        integer,
        [CsvPatternType(@type = "datetime")]
        [CsvPatternType(@type = "datetimeoffset")]
        [CsvPatternType(@type = "decimal")]
        inherit,
    }
    #endregion CsvPattern

    #region CsvSection
    /// <summary>
    /// Section de configuration d'exportation au format CSV
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [ComVisible(false)]
    public class CsvSection : ConfigurationSection
    {
        #region Members
        /// <summary>
        /// Paramètres par défaut commun à tous les exports non spécifiés dans Settings
        /// </summary>
        [ConfigurationProperty("defaultSettings")]
        public CsvDefaultConfigElement DefaultSettings
        {
            get { return ((CsvDefaultConfigElement)(base["defaultSettings"])); }
            set { DefaultSettings = (CsvDefaultConfigElement)value; }
        }
        /// <summary>
        ///¨Paramètres propres à chaque élément d'une collection d'export 
        /// => la clé d'un élément est le nom de l'export (referential.objectName)
        /// pour un référentiel, une consultation, etc.
        /// </summary>
        [ConfigurationProperty("settings")]
        [ConfigurationCollection(typeof(CsvCollection), AddItemName = "csv")]
        public CsvCollection Settings
        {
            get { return ((CsvCollection)(base["settings"])); }
        }
        #endregion Members
        #region Methods
        /// <summary>
        /// Les paramètres par défaut sont spécifiés dans le fichier .config
        /// </summary>
        public bool DefaultSettingsSpecified
        {
            get { return this.DefaultSettings.ElementInformation.IsPresent; }
        }
        /// <summary>
        /// Retourne les paramètres pour un export donné, si non trouvé 
        /// retourne les paramètres par défaut
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        public CsvCommonConfigElement GetSettings(string pName)
        {
            CsvCommonConfigElement match = null;
            if (null != Settings)
                match = Settings[pName];
            if ((null == match) && DefaultSettingsSpecified)
                match = DefaultSettings;
            return match;
        }
        #endregion Methods
    }
    #endregion CsvSection
    #region CsvCommonConfigElement
    /// <summary>
    /// Paramètres
    /// </summary>
    // EG 20190411 [ExportFromCSV]
    // EG 20190415[ExportFromCSV] Gestion CrLf
    [ComVisible(false)]
    public abstract class CsvCommonConfigElement : ConfigurationElement
    {
        #region Members
        // Culture (fr-FR, eg-GB, etc.)
        [ConfigurationProperty("culture", IsRequired = false)]
        public string Culture
        {
            get { return (string)this["culture"]; }
            set { this["culture"] = value; }
        }
        // Entête
        [ConfigurationProperty("hasHeaderRecord", DefaultValue = true, IsRequired = false)]
        public bool HasHeaderRecord
        {
            get { return (bool)this["hasHeaderRecord"]; }
            set { this["hasHeaderRecord"] = value; }
        }
        // Si Entête insertion du Keyword HEADER sur la 1ère ligne
        [ConfigurationProperty("hasHeaderKeyword", DefaultValue = false, IsRequired = false)]
        public bool HasHeaderKeyword
        {
            get { return (bool)this["hasHeaderKeyword"]; }
            set { this["hasHeaderKeyword"] = value; }
        }
        // Pas de Crlf dans les entêtes
        // EG 20190415[ExportFromCSV] Gestion CrLf
        [ConfigurationProperty("noCrLf", DefaultValue = true, IsRequired = false)]
        public bool NoCrLf
        {
            get { return (bool)this["noCrLf"]; }
            set { this["noCrLf"] = value; }
        }
        // Delimiter de substitution
        [ConfigurationProperty("delimiter", IsRequired = false)]
        public string Delimiter
        {
            get { return (string)this["delimiter"]; }
            set { this["delimiter"] = value; }
        }
        // Précision du timestamp
        // EG 20190419 [ExportFromCSV] Upd
        [ConfigurationProperty("tmsPrecision", DefaultValue = 6, IsRequired = false)]
        public int TmsPrecision
        {
            get { return (int)this["tmsPrecision"]; }
            set { this["tmsPrecision"] = value; }
        }
        // Collection des formats 
        [ConfigurationProperty("patterns", IsDefaultCollection = false)]
        public CsvPatternsCollection Patterns
        {
            get { return (CsvPatternsCollection)this["patterns"]; }
            set { this["patterns"] = (CsvPatternsCollection)value; }
        }
        #endregion Members
    }
    #endregion CsvCommonConfigElement
    #region CsvDefaultConfigElement
    /// <summary>
    /// Paramètres par défaut
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [ComVisible(false)]
    public class CsvDefaultConfigElement : CsvCommonConfigElement
    {
    }
    #endregion CsvDefaultConfigElement
    #region CsvConfigElement
    /// <summary>
    /// Paramètres pour un export donné
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [ComVisible(false)]
    public class CsvConfigElement : CsvCommonConfigElement
    {
        #region Members
        // Nom de l'export
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        [StringValidator(InvalidCharacters = "  ~!@#$%^&*()[]{}/;’\"|\\")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
        #endregion Members
    }
    #endregion CsvConfigElement
    #region CsvPatternsCollection
    /// <summary>
    /// Collection des formats en fonction des types génériques pour la conversion des données
    /// (Non exhaustif)
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [ComVisible(false)]
    public class CsvPatternsCollection : ConfigurationElementCollection
    {
        #region Accessors
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
        // tag de l'élément dans la collection
        protected override string ElementName
        {
            get { return "pattern"; }
        }
        #endregion Accessors
        #region Indexers
        // Recherche d'un élément par son nom dans la collection
        public new CsvPatternConfigElement this[string pType]
        {
            get
            {
                if (IndexOf(pType) < 0) return null;
                return (CsvPatternConfigElement)base.BaseGet(pType);
            }
        }
        // Recherche d'un élément par sa position dans la collection
        public CsvPatternConfigElement this[int pIndex]
        {
            get { return (CsvPatternConfigElement)BaseGet(pIndex); }
        }

        // Retourne l'index d'un élément s'il existe
        public int IndexOf(string pType)
        {
            pType = pType.ToLower();
            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].Type.ToString().ToLower() == pType)
                    return idx;
            }
            return -1;
        }
        #endregion Indexers
        #region Methods
        /// <summary>
        /// Ajout d'un pattern
        /// </summary>
        /// <param name="pType">Type de donnée</param>
        /// <param name="pFormat">Pattern (format associé)</param>
        public void Add(string pType, CsvPattern pFormat)
        {
            Add(pType, pFormat.ToString());
        }
        /// <summary>
        /// Ajout d'un pattern
        /// </summary>
        /// <param name="pType">Type de donnée</param>
        /// <param name="pFormat">Pattern (format associé)</param>
        public void Add(string pType, string pFormat)
        {
            this.BaseAdd(this.Count, new CsvPatternConfigElement(pType, pFormat));
        }
        /// <summary>
        /// Création d'un nouvel élément
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CsvPatternConfigElement();
        }
        /// <summary>
        /// Obtention de la clé de l'élément
        /// </summary>
        protected override object GetElementKey(ConfigurationElement pElement)
        {
            return (pElement as CsvPatternConfigElement).Type;
        }
        #endregion Methods
    }
    #endregion CsvPatternsCollection
    #region CsvPatternConfigElement
    /// <summary>
    /// Format par type de donnée
    /// Ex : decimal => #0,00, dateTime => isoDate, dateTime => dd/MM/yyyy, etc.
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [ComVisible(false)]
    public class CsvPatternConfigElement : ConfigurationElement
    {
        #region Members
        // Type de donnée
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"].ToString(); }
            set { this["type"] = value; }
        }
        // Format associé
        [ConfigurationProperty("format", IsRequired = true)]
        public string Format
        {
            get { return this["format"].ToString(); }
            set { this["format"] = value; }
        }
        #endregion Members
        #region Accessors
        /// <summary>
        /// Conversion la chaine spécifié en Type ("decimal" => system.Decimal)
        /// </summary>
        public Type TypeOf
        {
            get
            {
                Type ret = System.Type.GetType(String.Format("System.{0}", Type), false, true);
                return ret;
            }
        }
        #endregion Accessors
        #region Constructors
        public CsvPatternConfigElement()
        {
        }
        public CsvPatternConfigElement(string pType, string pFormat)
        {
            Type = pType;
            Format = pFormat;
        }
        #endregion Constructors
    }
    #endregion CsvPatternConfigElement
    #region CsvCollection
    /// <summary>
    /// Collection de paramètres par nom d'exportation
    /// </summary>
    // EG 20190411 [ExportFromCSV] 
    [ComVisible(false)]
    public class CsvCollection : ConfigurationElementCollection
    {
        #region Accessors
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
        // tag de l'élément dans la collection
        protected override string ElementName
        {
            get { return "csv"; }
        }
        #endregion Accessors
        #region Indexers
        // Recherche d'un élément par son nom dans la collection
        public new CsvConfigElement this[string pName]
        {
            get
            {
                return (CsvConfigElement)base.BaseGet(pName);
            }
        }
        // Recherche d'un élément par sa position dans la collection
        public CsvConfigElement this[int pIndex]
        {
            get
            {
                return (CsvConfigElement)BaseGet(pIndex);
            }
            set
            {
                if (BaseGet(pIndex) != null)
                    BaseRemoveAt(pIndex);
                BaseAdd(pIndex, value);

            }
        }
        public int IndexOf(CsvConfigElement pDetails)
        {
            return BaseIndexOf(pDetails);
        }
        public void Add(CsvConfigElement pDetails)
        {
            BaseAdd(pDetails);
        }
        protected override void BaseAdd(ConfigurationElement pElement)
        {
            BaseAdd(pElement, false);
        }
        public void Remove(CsvConfigElement pDetails)
        {
            if (BaseIndexOf(pDetails) >= 0)
                BaseRemove(pDetails.Name);

        }
        public void RemoveAt(int pIndex)
        {
            BaseRemoveAt(pIndex);
        }
        public void Remove(string pName)
        {
            BaseRemove(pName);
        }
        public void Clear()
        {
            BaseClear();
        }
        #endregion Indexers
        #region Constructors
        public CsvCollection()
        {
            CsvConfigElement details = (CsvConfigElement)CreateNewElement();
            if (StrFunc.IsFilled(details.Name))
                Add(details);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Création d'un nouvel élément
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CsvConfigElement();
        }
        /// <summary>
        /// Obtention de la clé de l'élément
        /// </summary>
        protected override object GetElementKey(ConfigurationElement pElement)
        {
            return (pElement as CsvConfigElement).Name;
        }
        #endregion Methods
    }
    #endregion CsvCollection
}

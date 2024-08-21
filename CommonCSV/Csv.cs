using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;

using System.Configuration;
using System.Runtime.InteropServices;

using EFS.ApplicationBlocks;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.ACommon;
using EFS.Common;

using CsvHelper;
using CsvConfig = CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper.Excel;

using Tz = EFS.TimeZone;
using EFS.Referential;

namespace EFS.CommonCSV
{
    #region CsvTypeConverter
    /// <summary>
    /// Type de conversion customisé
    /// </summary>
    [Serializable]
    public sealed class CsvTypeConverter
    {
        #region Members
        /// <summary>
        /// Type de colonne (System.decimal, System.DateTimeOffset, System.DateTime etc.)
        /// </summary>
        public Type columnType;
        /// <summary>
        /// Format associé de type CsvPattern
        /// </summary>
        public Nullable<CsvPattern> pattern;
        /// <summary>
        /// Format final (Transformation du format si de type CsvPattern (pattern) ou 
        /// </summary>
        public string finalPattern;
        /// <summary>
        /// Option de conversion
        /// </summary>
        public TypeConverterOptions typeConverterOption;
        /// <summary>
        /// Colonnes avec traitement particulier
        /// </summary>
        public List<Pair<string, CsvPattern>> excludeColumns;
        #endregion Members
        #region Constructors
        public CsvTypeConverter(CsvPatternConfigElement pCsvPatternConfigElement)
        {
            // Type
            columnType = pCsvPatternConfigElement.TypeOf;
            // Format de type CsvPattern
            pattern = ReflectionTools.ConvertStringToEnumOrNullable<CsvPattern>(pCsvPatternConfigElement.Format);
            // ou Format générique
            if (false == pattern.HasValue)
                finalPattern = pCsvPatternConfigElement.Format;
        }
        public CsvTypeConverter(Type pColumnType, CsvPattern pPattern)
        {
            columnType = pColumnType;
            pattern = pPattern;
        }
        #endregion Constructors
        #region Methods
        #region AddTypeConverterOption
        /// <summary>
        /// Création du convertisseur 
        /// </summary>
        /// <param name="pCulture">Culture</param>
        /// <param name="pTmsSuffix">Pattern du timestamp</param>
        public void AddTypeConverterOption(CultureInfo pCulture, string pPatternTms)
        {
            if (pattern.HasValue && (pattern.Value != CsvPattern.inherit))
                finalPattern = CsvTools.GetFinalPatternFormat(pCulture, pattern.Value, pPatternTms);

            if (StrFunc.IsFilled(finalPattern))
            {
                typeConverterOption = new TypeConverterOptions()
                {
                    CultureInfo = pCulture,
                    Formats = new string[] { finalPattern },
                };
            }
        }
        #endregion AddTypeConverterOption
        #region AddExcludeColumns
        /// <summary>
        /// Exclusion des colonnes subissant un traitement particulier
        /// "DTORDERENTERED", "DTEXECUTION", "DTTIMESTAMP", "DTDLVYSTART", "DTDLVYEND"
        /// "DTSYS", "DTPROCESS", "DTINS", "DTUPD", "DTSYS_"
        /// </summary>
        /// <param name="pExcludePattern">Pattern</param>
        /// <param name="pExcludeColumns">Liste des nom de colonnes</param>
        public void AddExcludeColumns(CsvPattern pExcludePattern, params string[] pExcludeColumns)
        {
            if (null == excludeColumns)
                excludeColumns = new List<Pair<string, CsvPattern>>();

            pExcludeColumns.ToList().ForEach(excludeColumn =>
            {
                if (false == excludeColumns.Exists(item => item.First == excludeColumn))
                    excludeColumns.Add(new Pair<string, CsvPattern>(excludeColumn, pExcludePattern));
            });
        }
        #endregion AddExcludeColumns
        #endregion Methods
    }
    #endregion CsvTypeConverter

    #region CSVReferentialColumn
    /// <summary>
    /// Mapping d'un colonne Referentielle : Consultation, Referentiel, Process etc. (via List.aspx) 
    /// candidate à traitement par Export CSV
    /// </summary>
    public class CSVReferentialColumn
    {
        #region Members
        public string columnName;
        public string aliasColumnName;
        public bool aliasColumnNameSpecified;
        public string aliasTableName;
        public bool aliasTableNameSpecified;
        public string ressource;
        public bool ressourceSpecified;
        public bool isHide;
        public bool isConsultation;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Nom de la colonne (Mapping entre QUERY SQL et REFERENTIAL)
        /// </summary>
        public string Name
        {
            get { return ((aliasTableNameSpecified && isConsultation ? aliasTableName + "_" : string.Empty) + columnName).ToUpper(); }
        }
        /// <summary>
        /// Header de la colonne
        /// </summary>
        public string HeaderName
        {
            get { return ressourceSpecified ? ressource : columnName; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Mapping d'un colonne Referentielle : Consultation, Referentiel, Process etc. (via List.aspx) 
        /// pour Construction HeaderName dans CSV
        /// NB : les colonnes sélectionnées ont ISHIDEINDATAGRID = FALSE
        /// </summary>
        /// <param name="pReferentialColumn">Colonne source</param>
        /// <param name="pIsConsultation">Mode consultation</param>
        // EG 20190415[ExportFromCSV][24638] Gestion CrLf
        // EG 20190415[ExportFromCSV][24638] Gestion Colonne avec Relation
        public CSVReferentialColumn(ReferentialsReferentialColumn pReferentialColumn, bool pIsConsultation, bool pNoCrLf)
        {
            isConsultation = pIsConsultation;
            isHide = pReferentialColumn.IsHideInDataGrid;
            columnName = pReferentialColumn.ColumnName;
            aliasColumnNameSpecified = pReferentialColumn.AliasColumnNameSpecified;
            aliasColumnName = pReferentialColumn.AliasColumnName;
            aliasTableNameSpecified = pReferentialColumn.AliasTableNameSpecified;
            aliasTableName = pReferentialColumn.AliasTableName;
            ressourceSpecified = pReferentialColumn.RessourceSpecified;
            if (pIsConsultation)
                ressource = pReferentialColumn.Ressource;
            else
                Ressource.GetMulti(pReferentialColumn.Ressource, 1, out ressource);

            if (ArrFunc.IsFilled(pReferentialColumn.Relation))
                SetRelationalColumnSubstitution(pReferentialColumn.Relation[0], pIsConsultation);

            if (pNoCrLf)
                ressource = ressource.Replace(Cst.HTMLBreakLine, " ").Replace("<brnobold/>", " ");
            else
                ressource = ressource.Replace(Cst.HTMLBreakLine, Cst.Lf).Replace("<brnobold/>", Cst.Lf);
        }
        #endregion Constructors

        #region Members
        /// <summary>
        /// Substitution du nom de la colonne si existence d'une relation
        /// </summary>
        /// <param name="pRelation"></param>
        /// <param name="pIsConsultation"></param>
        // EG 20190415[ExportFromCSV][24638] New
        private void SetRelationalColumnSubstitution(ReferentialsReferentialColumnRelation pRelation, bool pIsConsultation)
        {
            if (ArrFunc.IsFilled(pRelation.ColumnSelect) && (null != pRelation.ColumnSelect[0]))
            {
                ReferentialsReferentialColumnRelationColumnSelect columnSelect = pRelation.ColumnSelect[0];
                columnName = pRelation.AliasTableName + "_" + columnSelect.ColumnName;
                ressourceSpecified = StrFunc.IsFilled(columnSelect.Ressource);
                if (ressourceSpecified)
                {
                    if (pIsConsultation)
                        ressource = columnSelect.Ressource;
                    else
                        Ressource.GetMulti(columnSelect.Ressource, 1, out ressource);
                }
            }
        }
        #endregion Members
    }
    #endregion CSVReferentialColumn

    #region CsvConfiguration
    /// <summary>
    /// Classe de configuration pour export CSV
    /// - Précision du timestamp
    /// - En-tête
    /// - Delimiteur
    /// - Ovveride de conversion en fonction des paramètres présent dans le fichier de configuration
    /// </summary>
    // EG 20190411 [ExportFromCSV]
    // EG 20190415 [ExportFromCSV] Gestion CrLf
    // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
    // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration et CsvConfig.Configuration devient CsvConfig.CsvConfiguration
    // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
    // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
    public class OverrideCsvConfiguration
    {
        #region Members
        public CsvConfig.CsvConfiguration configuration;
        /// <summary>
        /// Pattern de précision du timestamp
        /// </summary>
        public string patternTms;
        /// <summary>
        /// Mot clé HEADER avant en-tête
        /// </summary>
        public bool hasHeaderKeyword;
        /// <summary>
        /// Suppression des CrLF dans les en-têtes
        /// </summary>
        public bool noCrLf;
        /// <summary>
        /// Liste des types de conversion customisé
        /// </summary>
        public List<CsvTypeConverter> lstCsvTypeConverter;
        #endregion Members
        #region Constructors
        /// <summary>
        /// Constructeur sans paramètres de configuration
        /// La culture courante est utilisée
        /// </summary>
        public OverrideCsvConfiguration()
        {
        }
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
        public OverrideCsvConfiguration(string pCulture)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture(pCulture);
            configuration = new CsvConfig.CsvConfiguration(culture);
            patternTms = "$1";
        }
        /// <summary>
        /// Constructeur de configuration CSV sur la base des paramètres présents dans
        /// le fichier de configuration.
        /// </summary>
        /// <param name="pSettings">Paramètres de configuration (CsvCustomSettings.config)</param>
        // EG 20190415 [ExportFromCSV] Gestion CrLf
        // EG 20190415[ExportFromCSV][24638] Upd Delimiter si Culture renseigné dans Settings
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
        public OverrideCsvConfiguration(CsvCommonConfigElement pSettings)
        {
            // Avec En-tête ou pas
            hasHeaderKeyword = pSettings.HasHeaderKeyword;
            CultureInfo culture = StrFunc.IsFilled(pSettings.Culture) ? CultureInfo.CreateSpecificCulture(pSettings.Culture) : Thread.CurrentThread.CurrentCulture;

            configuration = new CsvConfig.CsvConfiguration(culture)
            {
                // Avec mot clé HEADER avant en-tête
                HasHeaderRecord = pSettings.HasHeaderRecord,
                // Avec délimiteur
                Delimiter = StrFunc.IsFilled(pSettings.Delimiter) ? pSettings.Delimiter : culture.TextInfo.ListSeparator,
                // FI 20211215 [25904] 
                InjectionCharacters = new[] { '=', '@', '+', '-', '\t', '\r' },
                InjectionEscapeCharacter = '\'',
                SanitizeForInjection = true,
            };

            // Avec ou sans CrLf
            noCrLf = pSettings.NoCrLf;
            // Précision du timestamp
            patternTms = "$1";
            if (0 < pSettings.TmsPrecision)
                patternTms += "." + new String('f', pSettings.TmsPrecision);

            // Création des convertisseurs de type customisés
            AddDefaultTypeConverter(pSettings);

            // Gestion spécifique des date avec timezone associé
            AddExcludeColumnTypeConverter(typeof(DateTimeOffset), CsvPattern.isoDatetimeOffset,
                "DTORDERENTERED", "DTEXECUTION", "DTTIMESTAMP", "DTDLVYSTART", "DTDLVYEND");
            // Gestion spécifique des date systèmes (timezone local)
            AddExcludeColumnTypeConverter(typeof(DateTime), CsvPattern.isoDatetimeOffset,
                "DTSYS", "DTPROCESS", "DTINS", "DTUPD", "DTSYS_");
            //AddConverterOptionsCache();
        }
        #endregion Constructors
        #region Methods
        #region AddDefaultTypeConverter
        /// <summary>
        /// Conversion customisée des données par type 
        /// en fonction des paramètres présent dans le fichier de configuration.
        /// - type de donnée, pattern associé (format) 
        /// Alimente une liste de conversion (de type CsvTypeConverter)
        /// </summary>
        /// <param name="pSettings">paramètres du fichier de configuration</param>
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
        private void AddDefaultTypeConverter(CsvCommonConfigElement pSettings)
        {
            lstCsvTypeConverter = new List<CsvTypeConverter>();

            pSettings.Patterns.Cast<CsvPatternConfigElement>().ToList().ForEach(pattern => lstCsvTypeConverter.Add(new CsvTypeConverter(pattern)));
            lstCsvTypeConverter.ForEach(converter => converter.AddTypeConverterOption(this.configuration.CultureInfo, patternTms));
        }
        #endregion AddDefaultTypeConverter
        #region AddExcludeColumnTypeConverter
        /// <summary>
        /// Colonnes exclues d'une conversion customisée car elles subissent un tratement particulier
        /// Ex : 
        /// Gestion spécifique des date avec timezone associé
        ///   => "DTORDERENTERED", "DTEXECUTION", "DTTIMESTAMP", "DTDLVYSTART", "DTDLVYEND" de type DateTimeOffset
        /// Gestion spécifique des date systèmes (timezone local)
        ///   => "DTSYS", "DTPROCESS", "DTINS", "DTUPD", "DTSYS_"
        /// </summary>
        /// <param name="pColumnType"></param>
        /// <param name="pExcludePattern"></param>
        /// <param name="pExcludeColumns"></param>
        private void AddExcludeColumnTypeConverter(Type pColumnType, CsvPattern pExcludePattern, params string[] pExcludeColumns)
        {
            CsvTypeConverter csvTypeConverter = lstCsvTypeConverter.Find(converter => converter.columnType == pColumnType);
            if (null != csvTypeConverter)
                csvTypeConverter.AddExcludeColumns(pExcludePattern, pExcludeColumns);
        }
        #endregion AddExcludeColumnTypeConverter
        #region AddConverterOptionsCache
        /// <summary>
        /// Ajoute des options de conversion au TypeConversion de CsvHelper
        /// en fonction de la liste lstCsvTypeConverter (voir AddDefaultTypeConverter)
        /// </summary>
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] TypeConverterOptionsCache est dans CsvContext
        public void AddConverterOptionsCache(CsvContext pCsvContext)
        {
            lstCsvTypeConverter.ForEach(converter =>
            {
                if (null != converter.typeConverterOption)
                    pCsvContext.TypeConverterOptionsCache.AddOptions(converter.columnType, converter.typeConverterOption);
            });
        }
        #endregion AddConverterOptionsCache
        #region IsDateTimeOffsetWithFacility
        /// <summary>
        /// La colonne est de type DateTimeOffset avec un TimezoneFacility associé
        /// Elle subit un traitement particulier (voir AddExcludeColumnTypeConverter)
        /// La colonne contient : "DTORDERENTERED", "DTEXECUTION", "DTTIMESTAMP", "DTDLVYSTART", "DTDLVYEND"
        /// </summary>
        /// <param name="pColumnName">Nom de la colonne</param>
        public bool IsDateTimeOffsetWithFacility(string pColumnName)
        {
            return IsExcludeColumn(typeof(DateTimeOffset), pColumnName);
        }
        #endregion IsDateTimeOffsetWithFacility
        #region IsDateTimeOffsetWithDeliveryFacility
        /// <summary>
        /// La colonne est de type DateTimeOffset avec un TimezoneFacility associé
        /// Elle subit un traitement particulier (voir AddExcludeColumnTypeConverter)
        /// La colonne contient : "DTORDERENTERED", "DTEXECUTION", "DTTIMESTAMP", "DTDLVYSTART", "DTDLVYEND"
        /// </summary>
        /// <param name="pColumnName">Nom de la colonne</param>
        // EG 20190417 [ExportFromCSV] New
        public bool IsDateTimeOffsetWithDeliveryFacility(string pColumnName)
        {
            return (null != lstCsvTypeConverter) && lstCsvTypeConverter.Exists(converter => converter.columnType.Equals(typeof(DateTimeOffset)) &&
                    (null != converter.excludeColumns) &&
                    converter.excludeColumns.Exists(excludeColumn => pColumnName.Contains(excludeColumn.First) && pColumnName.Contains("DLVY")));
        }
        #endregion IsDateTimeOffsetWithDeliveryFacility
        #region IsDateTimeOffsetLocal
        /// <summary>
        /// La colonne est de type DateTimeOffset de type Date Système 
        /// pour laquelle on associé un timezone local
        /// Elle subit un traitement particulier (voir AddExcludeColumnTypeConverter)
        /// La colonne contient : "DTSYS", "DTPROCESS", "DTINS", "DTUPD", "DTSYS_"
        /// </summary>
        /// <param name="pColumnName">Nom de la colonne</param>
        public bool IsDateTimeOffsetLocal(string pColumnName)
        {
            return IsExcludeColumn(typeof(DateTime), pColumnName);
        }
        #endregion IsDateTimeOffsetLocal
        #region IsExcludeColumn
        /// <summary>
        /// La colonne est exclue du traitement classique (voir AddExcludeColumnTypeConverter) 
        /// </summary>
        /// <param name="pType">Type de colonne</param>
        /// <param name="pColumnName">Nom de la colonne</param>
        public bool IsExcludeColumn(Type pType, string pColumnName)
        {
            return (null != lstCsvTypeConverter) && lstCsvTypeConverter.Exists(converter => converter.columnType.Equals(pType) &&
                    (null != converter.excludeColumns) &&
                    converter.excludeColumns.Exists(excludeColumn => pColumnName.Contains(excludeColumn.First)));
        }
        #endregion IsExcludeColumn
        #endregion Methods
    }
    #endregion CsvConfiguration

    // EG 20190411 [ExportFromCSV]
    public sealed class CsvTools
    {
        #region GetCsvPatternEnum
        /// <summary>
        /// Regroupement des patterns par type pour alimentation de dropdowns
        /// </summary>
        /// <typeparam name="T">CsvPatternEnum</typeparam>
        /// <param name="pType">Type</param>
        /// <returns></returns>
        public static IEnumerable<T> GetCsvPatternEnum<T>(string pType) where T : struct
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
                CsvPatternType[] enumAttrs = (CsvPatternType[])fieldInfo.GetCustomAttributes(typeof(CsvPatternType), true);

                if (ArrFunc.IsFilled(enumAttrs))
                {
                    if (enumAttrs.Where(x => x.type == pType).Count() > 0)
                        ret.Add(@value);
                }
            }
            return ret;
        }
        #endregion GetCsvPatternEnum

        #region GetFinalPatternFormat
        /// <summary>
        /// Retourne le format de conversion en focntion de l'enumerateur
        /// ex : isoDate => yyyy-MM-dd, twoFixedDecimal => #0.00;-#0.00, etc.
        /// </summary>
        /// <param name="pCulture">Culture</param>
        /// <param name="pCsvPattern">Pattern (Enumerateur)</param>
        /// <param name="pSecPatternSuffix">Pattern pour Timestamp (micro|millisecond)</param>
        /// <returns></returns>
        // EG 20190419 [ExportFromCSV] Upd
        public static string GetFinalPatternFormat(CultureInfo pCulture, CsvPattern pCsvPattern, string pPatternTms)
        {
            string convertedPattern = string.Empty;
            switch (pCsvPattern)
            {
                case CsvPattern.datetimeOffset:
                    DateTimeFormatInfo dfi = pCulture.DateTimeFormat as DateTimeFormatInfo;
                    convertedPattern = dfi.ShortDatePattern + " " + Regex.Replace(dfi.LongTimePattern, "(:ss|:s)", pPatternTms) + "zzz";
                    break;
                case CsvPattern.inherit:
                    break;
                default:
                    // Le format est un attribut de l'énumerateur
                    FieldInfo fieldInfo = typeof(CsvPattern).GetField(pCsvPattern.ToString());
                    CsvPatternType[] enumAttrs = (CsvPatternType[])fieldInfo.GetCustomAttributes(typeof(CsvPatternType), true);
                    if (ArrFunc.IsFilled(enumAttrs))
                        convertedPattern = enumAttrs.First().format;
                    break;
            }
            return convertedPattern;
        }
        #endregion GetFinalPatternFormat

        #region GetFullPathFileName
        /// <summary>
        /// Construction du nom complet de fichier
        /// </summary>
        /// <param name="pPathName">Dossier</param>
        /// <param name="pFileName">Nom de fichier</param>
        /// <param name="pCulture">Culture</param>
        /// <returns></returns>
        public static string GetFullPathFileName(string pPathName, string pFileName, string pCulture)
        {

            string fullPathFileName = pPathName.Trim();
            SystemIOTools.CreateDirectory(fullPathFileName);
            if (false == fullPathFileName.EndsWith(@"\"))
                fullPathFileName += @"\";
            fullPathFileName += pFileName.Trim() + "." + pCulture + ".csv";
            return fullPathFileName;
        }
        #endregion GetFullPathFileName

        #region ExportToCSV
        /// <summary>
        /// Exportation sous format CSV dans une fichier
        /// </summary>
        /// <typeparam name="T">Type de la source de données</typeparam>
        /// <param name="pSource">Données</param>
        /// <param name="pPathName">Dossier</param>
        /// <param name="pFileName">Nom de fichier</param>
        public static void ExportToCSV<T>(T pSource, string pPathName, string pFileName)
        {
            OverrideCsvConfiguration csvConfiguration = new OverrideCsvConfiguration(Thread.CurrentThread.CurrentUICulture.Name);
            ExportToCSV(pSource, pPathName, pFileName, csvConfiguration);
        }
        /// <summary>
        /// Exportation sous format CSV dans une fichier
        /// </summary>
        /// <typeparam name="T">Type de la source de données</typeparam>
        /// <param name="pSource">Données</param>
        /// <param name="pPathName">Dossier</param>
        /// <param name="pFileName">Nom de fichier</param>
        /// <param name="pCsvConfiguration">Fichier de configuation</param>
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
        public static void ExportToCSV<T>(T pSource, string pPathName, string pFileName, OverrideCsvConfiguration pCsvConfiguration)
        {
            // Fichier
            string fullPathFileName = GetFullPathFileName(pPathName, pFileName, pCsvConfiguration.configuration.CultureInfo.Name);
            if (File.Exists(fullPathFileName))
            {
                File.Copy(fullPathFileName, fullPathFileName.Replace("csv", "old.csv"), true);
                File.Delete(fullPathFileName);
            }

            using (var writer = new StreamWriter(fullPathFileName, false, Encoding.UTF8))
            {
                // Mapping
                List<MapDataReaderRow> mapData = MappingData(pSource);
                // Ecriture
                if (null != mapData)
                    WriteData(writer, mapData, pCsvConfiguration, null);
            }
        }
        /// <summary>
        /// Exportation sous format CSV dans un memory stream
        /// </summary>
        /// <typeparam name="T">Type de la source de données</typeparam>
        /// <param name="pSource">Données</param>
        /// <param name="pCsvConfiguration">Fichier de configuation</param>
        /// <returns>Tableau d'octets</returns>
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        public static byte[] ExportToCSV<T>(T pSource, OverrideCsvConfiguration pCsvConfiguration)
        {
            return ExportToCSV(pSource, pCsvConfiguration, null);
        }
        /// <summary>
        /// Exportation sous format CSV dans un memory stream
        /// </summary>
        /// <typeparam name="T">Type de la source de données</typeparam>
        /// <param name="pSource">Données</param>
        /// <param name="pCsvConfiguration">Fichier de configuation</param>
        /// <param name="pReferential">Referentiel associé</param>
        /// <returns>Tableau d'octets</returns>
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        public static byte[] ExportToCSV<T>(T pSource, OverrideCsvConfiguration pCsvConfiguration, ReferentialsReferential pReferential)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                // Mapping
                List<MapDataReaderRow> mapData = MappingData(pSource);
                // Ecriture
                if (null != mapData)
                    WriteData(streamWriter, mapData, pCsvConfiguration, pReferential);
                return memoryStream.ToArray();
            }
        }
        #endregion ExportToCSV

        #region MappingData
        /// <summary>
        /// Transformation de la source de données en List(MapDataReaderRow)
        /// </summary>
        /// <typeparam name="T">DataSet|DataReader|DataTable|List(MapDataReaderRow)</typeparam>
        /// <param name="pSource">Data</param>
        /// <returns></returns>
        private static List<MapDataReaderRow> MappingData<T>(T pSource)
        {
            List<MapDataReaderRow> mapData = null;
            if (pSource is List<MapDataReaderRow>)
                mapData = pSource as List<MapDataReaderRow>;
            else if (pSource is DataSet)
            {
                using (IDataReader dr = (pSource as DataSet).CreateDataReader())
                    mapData = DataReaderExtension.DataReaderMapToList(dr, DataReaderExtension.DataReaderMapColumnType(dr));
            }
            else if (pSource is DataTable)
            {
                using (IDataReader dr = (pSource as DataTable).CreateDataReader())
                    mapData = DataReaderExtension.DataReaderMapToList(dr, DataReaderExtension.DataReaderMapColumnType(dr));
            }
            else if (pSource is IDataReader)
            {
                mapData = DataReaderExtension.DataReaderMapToList(pSource as IDataReader, DataReaderExtension.DataReaderMapColumnType(pSource as IDataReader));
            }
            return mapData;
        }
        #endregion MappingData

        #region WriteData
        /// <summary>
        /// Ecriture fichier CSV
        /// </summary>
        /// <param name="pWriter">CSV writer (with configuration)</param>
        /// <param name="pRows">Data</param>
        /// <param name="pCsvConfiguration">Paramètres de configuration</param>
        /// <param name="pReferential">Référentiel source (si existe)</param>
        // EG 20190415 [ExportFromCSV] Gestion CrLf
        // EG 20190415[ExportFromCSV][24638] Application Ressource sur la base de la culture de l'export (hors Consultation car les ressources sont déjà appliquées)
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] CsvConfig.CsvConfiguration devient un record (ajout member configuration)
        private static void WriteData(StreamWriter pWriter, List<MapDataReaderRow> pRows, OverrideCsvConfiguration pCsvConfiguration, ReferentialsReferential pReferential)
        {
            List<CSVReferentialColumn> csvReferentialColumns = null;
            if (null != pReferential)
            {
                CultureInfo savCurrentCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = pCsvConfiguration.configuration.CultureInfo;
                csvReferentialColumns = (from column in pReferential.Column
                                         where (column.IsHideInDataGrid == false)
                                         select new CSVReferentialColumn(column, pReferential.IsConsultation, pCsvConfiguration.noCrLf)).ToList();
                Thread.CurrentThread.CurrentCulture = savCurrentCulture;
            }

            using (var csv = new CsvWriter(pWriter, pCsvConfiguration.configuration))
            {
                pCsvConfiguration.AddConverterOptionsCache(csv.Context);
                // Write Header Column
                if ((csv.Configuration.HasHeaderRecord) && (0 < pRows.Count))
                    WriteHeader(csv, pRows.First(), pCsvConfiguration, csvReferentialColumns);
                // Write Data (Columns values by row)
                WriteRows(csv, pRows, pCsvConfiguration, csvReferentialColumns);
            }
        }
        #endregion WriteData
        #region WriteHeader
        /// <summary>
        /// Ecriture de l'entête du fichier CSV
        /// 1. Nom des colonnes du DataReader mappé (mode générique)
        /// 2. Ressource des colonnes du référentiel (si existe) 
        /// </summary>
        /// <param name="pCsvWriter">CSV writer (with configuration)</param>
        /// <param name="pRow">Data</param>
        /// <param name="pCsvReferentialColumns">Liste des colonnes du référentiel source (si existe)</param>
        // EG 20190417 [ExportFromCSV] Upd
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        private static void WriteHeader(CsvWriter pCsvWriter, MapDataReaderRow pRow, OverrideCsvConfiguration pCsvConfiguration, IEnumerable<CSVReferentialColumn> pCsvReferentialColumns)
        {
            // Affichage dans l'en-tête d'un champ "Fuseau Horaire" si TZFACILITY existe mais est masquée 
            string tzdbId = Ressource.GetString("TIMEZONE");
            bool addTimeZoneHeader = (null != pRow["TZFACILITY"]) && (null != pCsvReferentialColumns) && (null == pCsvReferentialColumns.FirstOrDefault(refCol => refCol.columnName == "TZFACILITY"));

            // Mot clé HEADER
            if (pCsvConfiguration.hasHeaderKeyword)
            {
                pCsvWriter.WriteField("HEADER");
                pCsvWriter.NextRecord();
            }
            pRow.Column.ForEach(column =>
            {
                if (null != pCsvReferentialColumns)
                {
                    // En-tête sur la base d'un référentiel
                    CSVReferentialColumn refCol = pCsvReferentialColumns.FirstOrDefault(refcol => refcol.Name == column.Name);
                    if (null != refCol)
                    {
                        pCsvWriter.WriteField(refCol.HeaderName);
                        // Affichage dans l'en-tête d'un champ "Fuseau Horaire" 
                        // les dates Delivery sont exclues
                        if (pCsvConfiguration.IsDateTimeOffsetWithFacility(refCol.columnName) && addTimeZoneHeader &&
                            (false == pCsvConfiguration.IsDateTimeOffsetWithDeliveryFacility(refCol.columnName)))
                            pCsvWriter.WriteField(tzdbId);
                    }
                }
                else
                {
                    // En-tête sur le nom de la colonne
                    pCsvWriter.WriteField(column.Name);
                }
            });
            pCsvWriter.NextRecord();
        }
        #endregion WriteHeader
        #region WriteRows
        /// <summary>
        /// Ecriture des lignes de données
        /// 1. Toutes les colonnes présente dans le DataReader mappé (mode générique)
        /// 2. Les colonnes visibles du référentiel  (si existe avec ISHIDEINDATAGRID = false) 
        /// </summary>
        /// <param name="pCsvWriter">CSV writer (with configuration)</param>
        /// <param name="pRows">Liste des données</param>
        /// <param name="pCsvReferentialColumns">Liste des colonnes du référentiel</param>
        /// <param name="pCsvConfiguration">Paramètres de configuration</param>
        /// <param name="pCsvReferentialColumns">Liste des colonnes du référentiel source (si existe)</param>
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        // EG 20210309 [XXXXX] Mise à jour package CsvHelper 19.0.0 vers 20.0.0
        // EG 20210309 [XXXXX] TypeConverterOptionsCache est dans Context
        private static void WriteRows(CsvWriter pCsvWriter, List<MapDataReaderRow> pRows, OverrideCsvConfiguration pCsvConfiguration, IEnumerable<CSVReferentialColumn> pCsvReferentialColumns)
        {
            TypeConverterOptions tco = pCsvWriter.Context.TypeConverterOptionsCache.GetOptions<DateTimeOffset>();
            bool isUTC = (null != tco) && (null != tco.Formats) && tco.Formats.ToList().Exists(item => item == DtFunc.FmtTZISOLongDateTime);
            pRows.ForEach(row =>
            {
                row.Column.ForEach(column =>
                {
                    if ((null == pCsvReferentialColumns) || (null != pCsvReferentialColumns.FirstOrDefault(refcol => refcol.Name == column.Name)))
                        WriteColumn(pCsvWriter, row, column, pCsvConfiguration, isUTC);
                });
                pCsvWriter.NextRecord();
            });
        }
        #endregion WriteRows
        #region WriteColumn
        /// <summary>
        /// Ecriture des valeurs des colonnes d'une ligne de données
        /// Si Null   => Empty
        ///    => Ecriture
        /// Si Int64  => Conversion en Int64
        ///    => Ecriture avec format associé à Int64
        /// Si Double  => Conversion en Decimal
        ///    => Ecriture avec format associé à Decimal
        /// Si DateTimeOffset avec TimeZone (DTEXECUTION, DTORDERENTERED, DTTIMESTAMP) => Conversion de la date en DateTimeOffset 
        ///    => dans la TimeZone spécifiée
        ///    => Ecriture avec format associé à DateTimeOffset
        /// Si DateTimeOffset local (DTINS, DTUPD, DTSYS) => Conversion de la date en DateTimeOffset 
        ///    => dans la TimeZone locale (Local.Id)
        ///    => Ecriture avec format associé à DateTimeOffset
        /// Sinon 
        ///    => Ecriture avec format associé au type de donné de la colonne
        /// </summary>
        /// <param name="pCsvWriter">CSV writer (with configuration)</param>
        /// <param name="pRow">ligne de données</param>
        /// <param name="pColumn">Colonne source</param>
        /// <param name="pCsvConfiguration">Paramètres de configuration</param>
        /// <param name="pIsUTC">Si oui : Les dates avec TimeZone sont en entrée en UTC</param>
        // EG 20190417 [ExportFromCSV] Upd
        // EG 20190418 [ExportFromCSV] Upd
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        // RD 20221114 [26170] Ecriture des valeurs du type Double avec format associé au type Decimal
        private static void WriteColumn(CsvWriter pCsvWriter, MapDataReaderRow pRow, MapDataReaderColumn pColumn, OverrideCsvConfiguration pCsvConfiguration, bool pIsUTC)
        {
            if (Convert.IsDBNull(pColumn.Value))
            {
                pCsvWriter.WriteField(string.Empty);
            }
            else if (pColumn.OverrideType.Equals(typeof(Int64)))
            {
                pCsvWriter.WriteField<Int64>(Convert.ToInt64(pColumn.Value));
            }
            else if (pColumn.OverrideType.Equals(typeof(Double)))
            {
                pCsvWriter.WriteField<Decimal>(Convert.ToDecimal(pColumn.Value));
            }
            else if (pCsvConfiguration.IsDateTimeOffsetWithFacility(pColumn.Name))
            {
                string tzdbId = "Etc/UTC";
                if (false == pIsUTC)
                {
                    // Cas des Dates de type DTDLVYSTART, DTDLVYEND couplées avec un TIMEZONE CONTRACT_DELIVERYTIMEZONE|TZDLVY.
                    if (pCsvConfiguration.IsDateTimeOffsetWithDeliveryFacility(pColumn.Name))
                    {
                        if (null != pRow["CONTRACT_DELIVERYTIMEZONE"])
                            tzdbId = pRow["CONTRACT_DELIVERYTIMEZONE"].Value.ToString();
                        else if (null != pRow["TZDLVY"])
                            tzdbId = pRow["TZDLVY"].Value.ToString();
                    }
                    else
                    {
                        // Cas des Dates de type DTORDERENTERED, DTEXECUTION, DTTIMESTAMP couplées avec un TIMEZONE.
                        if (null != pRow["TZFACILITY"])
                            tzdbId = pRow["TZFACILITY"].Value.ToString();
                    }
                }
                if (StrFunc.IsEmpty(tzdbId))
                    tzdbId = "Etc/UTC";

                string dt = Tz.Tools.ToString(pColumn.Value);
                Nullable<DateTimeOffset> dtOffset = Tz.Tools.FromTimeZone(DtFunc.AddEndUTCMarker(dt), tzdbId);
                pCsvWriter.WriteField(dtOffset.Value);
                // Affichage du nom du fuseau horaire associé à la date (hors Delivery)
                if (false == pCsvConfiguration.IsDateTimeOffsetWithDeliveryFacility(pColumn.Name))
                    pCsvWriter.WriteField(tzdbId);
            }
            else if (pCsvConfiguration.IsDateTimeOffsetLocal(pColumn.Name))
            {
                // Cas des Dates de type DTINS, DTUPD, DTSYS couplées avec un TIMEZONE (=> Local.Id).
                string tzdbId = Tz.Tools.WindowsIdToTzdbId(TimeZoneInfo.Local.Id);
                string dt2 = Tz.Tools.ToString(pColumn.Value);
                Nullable<DateTimeOffset> dtOffset = Tz.Tools.FromTimeZone(dt2, tzdbId);
                pCsvWriter.WriteField(dtOffset.Value);
            }
            else
            {
                pCsvWriter.WriteField(pColumn.Value);
            }
        }
        #endregion WriteColumn
    }
}

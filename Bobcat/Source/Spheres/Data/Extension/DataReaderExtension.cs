using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EFS.ApplicationBlocks.Data.Extension
{
    /// <summary>
    /// Interface utilisée par la Class DataReaderExtension
    /// </summary>
    public interface IReaderRow
    {
        /// <summary>
        /// Data Reader permettant de lire les enregistrement
        /// </summary>
        IDataReader Reader { get; set; }
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        object GetRowData();
    }

    /// <summary>
    /// Mapping d'une colonne d'un DataReader
    /// </summary>
    // EG 20180326 [23769] New
    // EG 20190411 [ExportFromCSV] Refactoring (Add overrideType)
    [SerializableAttribute()]
    public class MapDataReaderColumn
    {
        // Nom de la colonne
        public string Name { set; get; }
        // Valeur de la colonne
        public object Value { set; get; }
        // Type overridé pour application d'un format (Pattern) CSV adapté (ex Int64 sur UT_ID : decimal car Numeric)
        public Type OverrideType { set; get; }

        public MapDataReaderColumn()
        {
        }
        public MapDataReaderColumn(string pName, object pValue, IEnumerable<MapDataReaderColumnType> pColumnType)
        {
            Name = pName;
            Value = pValue;
            OverrideType = Value.GetType();
            if (null != pColumnType)
            {
                MapDataReaderColumnType columnType = pColumnType.FirstOrDefault(item => item.columnName == Name);
                if (null != columnType)
                    OverrideType = columnType.overrideType;
            }
        }
    }

    /// <summary>
    /// Mapping d'une row d'un DataReader
    /// </summary>
    // EG 20180326 [23769] New
    [SerializableAttribute()]
    public class MapDataReaderRow
    {
        // Liste des colonnes d'une row
        public List<MapDataReaderColumn> Column { set; get; }

        public MapDataReaderRow(List<MapDataReaderColumn> pColumn)
        {
            Column = pColumn;
        }

        // Recherche d'une colonne par son index
        public MapDataReaderColumn this[int pIndex]
        {
            get
            {
                MapDataReaderColumn ret = null;
                if (null != Column)
                    ret = Column[pIndex];
                return ret;
            }
        }

        // Recherche d'une colonne par son nom
        public MapDataReaderColumn this[string pColumnName]
        {
            get
            {
                MapDataReaderColumn ret = null;
                if (null != Column)
                    ret = Column.Find(item => item.Name == pColumnName.ToUpper());
                return ret;
            }
        }
    }

    /// <summary>
    /// Stockage des colonnes d'un schéma (Datatable) en provenance d'un DataReader
    /// dans le but d'overrider un type décimal qui n'est en vérité qu'un entier
    /// Exemple : UT_ID est déclaré comme NUMERIC(10,0) donc decimal en .NET
    /// dans ce cas (cad quand NumericScale = 0) OverrideType est Int64.
    /// </summary>
    // EG 20190411 [ExportFromCSV] New
    [SerializableAttribute()]
    public class MapDataReaderColumnType
    {
        public string columnName;
        public Type overrideType;

        public MapDataReaderColumnType(string pColumnName, Type pOverrideType)
        {
            columnName = pColumnName;
            overrideType = pOverrideType;
        }
        public MapDataReaderColumnType(DataRow pSchemaRow)
        {
            columnName = pSchemaRow["ColumnName"].ToString();
            overrideType = pSchemaRow["DataType"] as Type;
            if (overrideType.Equals(typeof(Decimal)))
            {
                if ((false == Convert.IsDBNull(pSchemaRow["NumericScale"])) && 
                    (0 == Convert.ToInt32(pSchemaRow["NumericScale"])))
                    overrideType = typeof(Int64);
            }
        }
    }
    /// <summary>
    /// Permet la lecture d'un DataReader et de restituer son contenu sous forme de IEnumerable
    /// </summary>
    public static class DataReaderExtension
    {
        /// <summary>
        /// Lit le IDataReader et retourne les enregistrement sous la forme d'une énumération d'objets
        /// </summary>
        /// <typeparam name="T">Type des objets contenu par l'énumération retournée</typeparam>
        /// <typeparam name="TReader">Implémentation de IReaderRow</typeparam>
        /// <param name="source">Le IDataReader à étendre</param>
        /// <returns>Un énumération d'objets</returns>
        public static IEnumerable<T> DataReaderEnumerator<T, TReader>(this IDataReader source) where TReader : IReaderRow, new()
        {
            if (source == null)
                throw new ArgumentNullException("Source IDataReader is not instancied");

            IReaderRow rowReader = new TReader() { Reader = source };

            while (source.Read())
            {
                yield return (T)rowReader.GetRowData();
            }
        }

        /// <summary>
        /// Mapping d'un DataReader dans une liste de rows
        /// </summary>
        /// <param name="pDataReader">DataReader source</param>
        /// <returns></returns>
        // EG 20180326 [23769] New
        // EG 20190411 [ExportFromCSV] upd
        public static List<MapDataReaderRow> DataReaderMapToList(IDataReader pDataReader)
        {
            return DataReaderMapToList(pDataReader, null);
        }
        /// <summary>
        /// Mapping d'un DataReader dans une liste de rows
        /// </summary>
        /// <param name="pDataReader">DataReader source</param>
        /// <param name="pColumnType">Colonnes avec Type overridé</param>
        // EG 20190411 [ExportFromCSV] Upd
        public static List<MapDataReaderRow> DataReaderMapToList(IDataReader pDataReader, IEnumerable<MapDataReaderColumnType> pColumnType)
        {
            List<MapDataReaderRow> rows = new List<MapDataReaderRow>();
            bool isOk = true;
            while (isOk)
            {
                MapDataReaderRow row = DataReaderMapToSingle(pDataReader, pColumnType);
                isOk = (null != row);
                if (isOk)
                    rows.Add(row);
            }
            return rows;
        }

        /// <summary>
        /// Mapping d'une row d'un DataReader
        /// </summary>
        /// <param name="pDataReader">DataReader source</param>
        /// <returns></returns>
        // EG 20180326 [23769] New
        // EG 20190411 [ExportFromCSV] Upd
        public static MapDataReaderRow DataReaderMapToSingle(IDataReader pDataReader)
        {
            return DataReaderMapToSingle(pDataReader, null);
        }
        /// <summary>
        /// Mapping d'une row d'un DataReader
        /// </summary>
        /// <param name="pDataReader">DataReader source</param>
        /// <param name="pColumnType">Colonnes avec Type overridé</param>
        // EG 20190411 [ExportFromCSV] Upd
        public static MapDataReaderRow DataReaderMapToSingle(IDataReader pDataReader, IEnumerable<MapDataReaderColumnType> pColumnType)
        {
            MapDataReaderRow row = null;
            if (pDataReader.Read())
            {
                List<MapDataReaderColumn> col = Enumerable.Range(0, pDataReader.FieldCount).Select(i =>
                    new MapDataReaderColumn(pDataReader.GetName(i).ToUpper(), pDataReader.GetValue(i), pColumnType)).ToList();
                row = new MapDataReaderRow(col);
            }
            return row;
        }
        /// <summary>
        /// Construction d'un énumérateur de colonnes (MapDataReaderColumnType) du schéma d'un dataReader
        /// voir classe MapDataReaderColumnType
        /// </summary>
        /// <param name="pDataReader">DataReader source</param>
        /// <returns></returns>
        // EG 20190411 [ExportFromCSV] New
        public static IEnumerable<MapDataReaderColumnType> DataReaderMapColumnType(IDataReader pDataReader)
        {
            DataTable schema = pDataReader.GetSchemaTable();
            return (from dataRow in schema.Rows.Cast<DataRow>() select new MapDataReaderColumnType(dataRow));
        }
    }
}

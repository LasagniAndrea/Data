using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ThreadTasks = System.Threading.Tasks;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Enum;
using EFS.ACommon;
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.SpheresIO.MarketData
{
    /// <summary>
    /// Classe de base pour mise à jour de la base de données via l'uasage d'un DataTable
    /// </summary>
    internal class DataTableToUpdateBase
    {

        /// <summary>
        ///  Table des données en mémoire
        /// </summary>
        private DataTable _dataTable;

        /// <summary>
        /// 
        /// </summary>
        private string _queryForAdapter;

        /// <summary>
        /// 
        /// </summary>
        private string _cs;

        /// <summary>
        /// 
        /// </summary>
        private string _tableName;
             

        /// <summary>
        /// Nom de la table 
        /// </summary>
        protected DataTable DataTable
        {
            get { return _dataTable; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DataTableToUpdateBase()
        {
            _dataTable = new DataTable();
        }

        /// <summary>
        /// Chargement du Datatable 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="tableName"></param>
        /// <param name="queryParameters"></param>
        /// <param name="queryForAdapter"></param>
        protected void Load(string cs, string tableName, QueryParameters queryParameters, string queryForAdapter)
        {
            _cs = cs;
            _tableName = tableName;
            _queryForAdapter = queryForAdapter;
            _dataTable = DataHelper.ExecuteDataTable(cs, queryParameters.Query.ToString(), queryParameters.Parameters.GetArrayDbParameter());
            _dataTable.TableName = tableName;
        }

        /// <summary>
        /// Chargement du Datatable en mode asynchrone
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="tableName"></param>
        /// <param name="queryParameters"></param>
        /// <param name="queryForAdapter"></param>
        public async ThreadTasks.Task LoadAsync(string cs, string tableName, QueryParameters queryParameters, string queryForAdapter)
        {
            await ThreadTasks.Task.Run(() =>
            {
                Load(cs, tableName, queryParameters, queryForAdapter);
            });
        }


        /// <summary>
        /// Retourne le nombre de lignes modifiées depuis le dernier chargement (<see cref="Load(string, QueryParameters, string)"/> ou <see cref="LoadAsync(string, QueryParameters, string)"/>) ou depuis la dernière synchro avec la base (<see cref="UpdateDatabase"/> ou <see cref="UpdateDatabaseAsync"/>)
        /// </summary>
        /// <returns></returns>
        public int ChangesCount()
        {
            int ret = 0;
            DataTable dtChanges = _dataTable.GetChanges();
            if (null != dtChanges)
                ret = dtChanges.Rows.Count;
            return ret;
        }

        /// <summary>
        /// Liste des lignes. Cet objet peut être utilisé dans une expression LINQ ou une requête de méthode.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataRow> GetRows()
        {
            return _dataTable.AsEnumerable();
        }

        /// <summary>
        /// Retourne les lignes modifiées depuis le dernier chargement (<see cref="Load"/>) ou depuis la dernière synchro avec la base (<see cref="UpdateDatabase"/> ou <see cref="UpdateDatabaseAsync"/>)
        /// </summary>
        /// <returns></returns>
        public List<DataRow> GetRowsChanges()
        {
            List<DataRow> ret = new List<DataRow>();
            DataTable dtChanges = _dataTable.GetChanges();
            if (null != dtChanges)
                ret = dtChanges.AsEnumerable().ToList();
            return ret;
        }

        /// <summary>
        /// Applique les modifications à la base de données
        /// </summary>
        /// <returns>la liste des ID modifiés</returns>
        public List<Object> UpdateDatabase()
        {
            List<Object> ret = new List<Object>();

            DataTable dtChanges = AcceptChanges();

            if (null != dtChanges)
            {
                ret = (from item in dtChanges.AsEnumerable()
                       select item[$"{OTCmlHelper.GetColunmID(_tableName)}"]).ToList();

                DataHelper.ExecuteDataAdapter(_cs, _queryForAdapter, dtChanges);
            }

            return ret;
        }

        /// <summary>
        /// Applique les modifications à la base de données
        /// </summary>
        /// <returns>la liste des ID modifiés</returns>
        public async ThreadTasks.Task<List<Object>> UpdateDatabaseAsync()
        {
            List<Object> ret = new List<Object>();

            DataTable dtChanges = AcceptChanges();

            if (null != dtChanges)
            {
                ret = (from item in dtChanges.AsEnumerable()
                       select item[$"{OTCmlHelper.GetColunmID(_tableName)}"]).ToList();

                await ThreadTasks.Task.Run(() =>
                {
                    DataHelper.ExecuteDataAdapter(_cs, _queryForAdapter, dtChanges);
                });
            }

            return ret;
        }

        /// <summary>
        ///  Valide les lignes modifiées. 
        /// </summary>
        /// <returns>les lignes modifiées. Retourne null si aucune modification</returns>
        private DataTable AcceptChanges()
        {
            DataTable dtChanges = null;

            lock (_dataTable.Rows.SyncRoot)
            {
                dtChanges = _dataTable.GetChanges();
                if (null != dtChanges)
                    _dataTable.AcceptChanges();
            }

            return dtChanges;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFS.ApplicationBlocks.Data
{
    /// <summary>
    ///  Pilote la suppression des requêtes présente dans le cache  lorsqu'il y a modification d'une table
    /// </summary>
    public class QueryCacheTable
    {
        /// <summary>
        ///  Réprésente le nom de la table 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean tableReferenceSpecified;
        /// <summary>
        ///  
        /// </summary>
        [System.Xml.Serialization.XmlElement()]
        public QueryCacheTableReference[] tableReference;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean objSpecified;
        /// <summary>
        /// Liste des ojects SQL dépendant (exemple VW_BOOK_VIEWER lorsque id="ACTOR")
        /// </summary>
        [System.Xml.Serialization.XmlElement()]
        public QueryCacheObj[] obj;
    }

    /// <summary>
    /// 
    /// </summary>
    public class QueryCacheTableReference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }

    /// <summary>
    ///  Représente une objet SQL (table, vue) 
    /// </summary>
    public class QueryCacheObj
    {
        /// <summary>
        /// Nom de l'objet SQL
        /// </summary>
        [System.Xml.Serialization.XmlText()]
        public string name;
    }

    /// <summary>
    ///  Suppression des requêtes présente dans le cache
    /// </summary>
    public class QueryCacheDelCascading
    {
        /// <summary>
        ///  Liste des tables
        /// </summary>
        [System.Xml.Serialization.XmlElement()]
        public QueryCacheTable[] table;
    }
}

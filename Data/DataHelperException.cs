
using System;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Configuration;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Linq;
//
using EFS.ACommon;


namespace EFS.ApplicationBlocks.Data
{

    
    /// <summary>
    /// Type d'erreur
    /// </summary>
    public enum DataHelperErrorEnum
    {
        /// <summary>
        /// Erreur de connexion
        /// </summary>
        connection,
        /// <summary>
        /// Erreur lors du chgt du DAL
        /// </summary>
        loadDal,
        /// <summary>
        /// Erreur lors de l'exécution d'une requête SQL
        /// </summary>
        query
    }
    
    /// <summary>
    /// 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    // EG 20180425 Analyse du code Correction [CA2237]
    [ComVisible(false)]
    [Serializable]
    public class DataHelperException : DbException
    {
        /// <summary>
        /// 
        /// </summary>
        public override int ErrorCode
        {
            get
            {
                return base.ErrorCode;
            }
        }


        /// <summary>
        ///  Obtient la requête qui a généré l'exception
        /// </summary>
        public string Query
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le type d'erreur
        /// </summary>
        public Nullable<DataHelperErrorEnum> ErrorEnum
        {
            get;
            private set;
        }


        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        public DataHelperException(string pMessage)
            : base(pMessage)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorEnum"></param>
        /// <param name="pMessage"></param>
        public DataHelperException(DataHelperErrorEnum pErrorEnum, string pMessage)
            : base(pMessage)
        {
            ErrorEnum = pErrorEnum;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pErrorEnum"></param>
        /// <param name="pMessage"></param>
        /// <param name="pException"></param>
        public DataHelperException(DataHelperErrorEnum pErrorEnum, string pMessage, Exception pException)
            : base(pMessage, pException)
        {
            ErrorEnum = pErrorEnum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pQuery"></param>
        /// <param name="pException"></param>
        public DataHelperException(string pMessage, string pQuery, Exception pException)
            : base(pMessage, pException)
        {
            ErrorEnum = DataHelperErrorEnum.query;
            Query = pQuery;
        }

        #endregion

        // EG 20180425 Analyse du code Correction [CA2240]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("query", Query);
            info.AddValue("errorEnum", ErrorEnum);

            base.GetObjectData(info, context);
        }

    }
}
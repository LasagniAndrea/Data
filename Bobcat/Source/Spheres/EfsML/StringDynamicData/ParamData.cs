#region Using Directives
using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.IO;
using System.Globalization;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
using EFS.Tuning;


using EfsML.Business;
#endregion Using Directives

namespace EfsML.DynamicData
{

    /// <summary>
    /// Représente un donnée formattée en string qui sera utilisée en tant que paramètre dans une requête SQL
    /// </summary>
    [XmlRoot(ElementName = "Param", IsNullable = true)]
    public class ParamData : StringDynamicData
    {
        #region Members
        [System.Xml.Serialization.XmlAttribute()]
        public ParameterDirection direction;

        [System.Xml.Serialization.XmlAttribute()]
        public int size;
        #endregion Members

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="pValue"></param>
        public ParamData(string pName, string pValue)
            : base()
        {
            name = pName;
            value = pValue;
        }
        /// <summary>
        /// 
        /// </summary>
        public ParamData()
            : base()
        {
            direction = ParameterDirection.Input;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retourne un DataParameter qui contient la donnée
        /// <para>Le DataParameter sera utilisé dans une requête SQL paramétré</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pTransaction"></param>
        /// <param name="pCommandType"></param>
        /// <returns></returns>
        public DataParameter GetDataParameter(string pCs, IDbTransaction pTransaction, CommandType pCommandType)
        {
            return base.GetDataParameter(pCs, pTransaction, pCommandType, size, direction);
        }
        #endregion

    }

}

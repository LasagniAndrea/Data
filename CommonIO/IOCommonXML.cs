using EFS.ACommon;
using EFS.Common.IO.Interface;
using EFS.Common.Log;
using EfsML.DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Serialization;

namespace EFS.Common.IO
{
    #region class IOTask
    /// <summary>
    /// Classe d'une tâche IO
    /// </summary>
    /// PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTask (IOXML.cs)
    [XmlRoot(ElementName = "iotask", IsNullable = true)]
    public class IOTask
    {
        #region Members
        /// <remarks/>
        [XmlAttribute]
        public string id;

        [XmlAttribute()]
        public string name;

        [XmlAttribute()]
        public string displayname;

        [XmlAttribute()]
        public string loglevel;

        [XmlAttribute()]
        public string commitmode;

        [XmlAttribute()]
        public string taskguid;

        [XmlIgnore]
        public string inout;

        [XmlElementAttribute("parameters", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskParams parameters;

        [XmlElementAttribute("iotaskdet", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDet taskDet;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Indique s'il existe des paramètres
        /// </summary>
        public bool IsParametersSpecified { get { return (parameters != null && ArrFunc.IsFilled(parameters.param)); } }
        /// <summary>
        /// Task Id
        /// </summary>
        public string Id { get { return id; } }
        /// <summary>
        /// Task Name
        /// </summary>
        public string Name { get { return name; } }
        /// <summary>
        /// Task Guid
        /// </summary>
        public string TaskGuid { get { return taskguid; } }
        /// <summary>
        /// Parameters
        /// </summary>
        public IOTaskParams Parameters { get { return parameters; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public IOTask() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pGuid"></param>
        /// <param name="pSqlIOTask"></param>
        // PM 20180219 [23824] Nouveau constructor
        public IOTask(string pId, string pGuid, SQL_IOTask pSqlIOTask)
        {
            id = pId;
            taskguid = pGuid;
            if (default(SQL_IOTask) != pSqlIOTask)
            {
                name = pSqlIOTask.Identifier;
                displayname = pSqlIOTask.DisplayName;
                loglevel = pSqlIOTask.LogLevel;
                commitmode = pSqlIOTask.CommitMode;
                inout = pSqlIOTask.InOut;
            }
        }
        #endregion Constructors

        #region Methods
        #region GetTaskParamValue
        /// <summary>
        /// Lecture de la valeur d'un paramètre de la tâche
        /// </summary>
        /// <param name="pParamId"></param>
        /// <returns></returns>
        public string GetTaskParamValue(string pParamId)
        {
            return GetTaskParamValue(pParamId, out _);
        }
        /// <summary>
        /// Lecture de la valeur et du type d'un paramètre de la tâche
        /// </summary>
        /// <param name="pParamId"></param>
        /// <param name="pParamDataType"></param>
        /// <returns></returns>
        public string GetTaskParamValue(string pParamId, out string pParamDataType)
        {
            string ret = null;
            pParamDataType = string.Empty;
            //
            if (parameters != null && ArrFunc.IsFilled(parameters.param))
            {
                for (int i = 0; i < parameters.param.Length; i++)
                {
                    if (parameters.param[i].id.ToUpper() == pParamId.ToUpper())
                    {
                        ret = parameters.param[i].Value;
                        pParamDataType = parameters.param[i].datatype;
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion GetTaskParamValue
        #region ExistTaskParam
        public bool ExistTaskParam(string pParamId)
        {
            bool ret = false;
            //
            if (parameters != null && ArrFunc.IsFilled(parameters.param))
            {
                pParamId = pParamId.ToUpper();
                for (int i = 0; i < parameters.param.Length; i++)
                {
                    if (parameters.param[i].id != null)
                    {
                        if (parameters.param[i].id.ToUpper() == pParamId)
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        #endregion ExistTaskParam
        #endregion Methods
    }
    #endregion class IOTask
    #region class IOTaskParams
    /// <summary>
    /// Classe représentant les paramètres d'une tâche IO
    /// </summary>
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskParams (IOXML.cs)
    public class IOTaskParams
    {
        #region Members
        [XmlElementAttribute("parameter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskParamsParam[] param;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Parameter indexer - search parameters by key
        /// </summary>
        /// <param name="id">parameter id</param>
        /// <returns>parameter value</returns>
        /// <exception cref="">When the parameter id does not exist</exception>
        public string this[string id]
        {
            get
            {
                string value = null;

                bool found = false;

                foreach (IOTaskParamsParam parameter in param)
                    if (parameter.id == id)
                    {
                        value = parameter.Value;
                        found = true;
                        break;
                    }

                if (found)
                {
                    return value;
                }
                else
                {
                    throw new IndexOutOfRangeException(String.Format("{0} key does not exist", id));
                }
            }
        }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Search parameters by key
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public bool Contains(string id)
        {
            bool ret = false;

            foreach (IOTaskParamsParam parameter in param)
                if (parameter.id == id)
                {
                    ret = true;
                    break;
                }

            return ret;
        }
        #endregion Methods
    }
    #endregion class IOTaskParams
    #region class IOTaskParamsParam
    /// <summary>
    /// Classe représentant un paramètre d'une tâche IO
    /// </summary>
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskParamsParam (IOXML.cs)
    public class IOTaskParamsParam
    {
        #region Members
        [XmlAttribute]
        public string id;

        [XmlAttribute()]
        public string name;

        [XmlAttribute()]
        public string displayname;

        [XmlAttribute()]
        public string direction;

        [XmlAttribute()]
        public string datatype;

        [XmlAttribute()]
        public string returntype;

        [XmlText()]
        public string Value;
        #endregion Members
    }
    #endregion class IOTaskParamsParam
    #region class IOTaskDet
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDet (IOXML.cs)
    [XmlRoot(ElementName = "iotaskdet", IsNullable = true)]
    public class IOTaskDet
    {
        [XmlAttribute]
        public string id;

        [XmlAttribute()]
        public string loglevel;

        [XmlAttribute()]
        public string commitmode;

        [XmlElementAttribute("ioinput", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOut input;

        [XmlElementAttribute("iooutput", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOut output;
    }
    #endregion class MapTaskDet
    #region class IOTaskDetInOut
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOut (IOXML.cs)
    public class IOTaskDetInOut
    {
        [XmlAttribute]
        public string id;

        [XmlAttribute()]
        public string name;

        [XmlAttribute()]
        public string displayname;

        [XmlAttribute()]
        public string loglevel;

        [XmlAttribute()]
        public string commitmode;

        [XmlElementAttribute("file", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFile file;
    }
    #endregion class TaskDetInOut
    #region class IOTaskDetInOutFile
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOutFile (IOXML.cs)
    [XmlRoot(ElementName = "file", IsNullable = false)]
    public class IOTaskDetInOutFile
    {
        /// <remarks/>
        [XmlIgnoreAttribute()]
        public bool nameSpecified;
        [XmlAttribute]
        public string name;

        [XmlIgnoreAttribute()]
        public bool folderSpecified;
        [XmlAttribute()]
        public string folder;

        [XmlIgnoreAttribute()]
        public bool dateSpecified;
        [XmlAttribute()]
        public string date;

        [XmlIgnoreAttribute()]
        public bool sizeSpecified;
        [XmlAttribute()]
        public string size;

        [XmlIgnoreAttribute()]
        public bool statusSpecified;
        [XmlAttribute()]
        public string status;
        //
        [XmlElementAttribute("row", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFileRow[] row;
        //
        [XmlIgnore()]
        public IOTaskDetInOutFileRow[] rowsWithoutLevel0;
        //
        [XmlIgnore()]
        public IOTaskDetInOutFileRow[] rowsLevel0;
    }
    #endregion class MapTaskDetInOutFile
    #region class IOTaskDetInOutFileRow
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOutFileRow (IOXML.cs)
    public class IOTaskDetInOutFileRow
    {
        #region Members
        [XmlAttribute()]
        public string id;
        //
        [XmlAttribute()]
        public string src;
        //
        [XmlAttribute()]
        public string status;
        [XmlIgnore()]
        public bool statusSpecified;
        //
        //
        [XmlAttribute()]
        public int level;
        [XmlIgnore()]
        public bool levelSpecified;
        //
        [XmlElementAttribute("row", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFileRow[] childRows;
        //

        // RD 20100728 [17101] Input: Use parameters in Post Mapping XML flow
        // Input only => To use parameters common to all Columns of row 
        // - it must be referenced like "parameters.{ParameterName}", in column value or ValueXML
        // - it's less priority then Column parameters 
        /// <summary>
        /// Représente les paramètres associés au FileRow
        /// <para>Les paramètres représentent des données qui une fois valorisées peuvent être utiliser pour alimenter les colonnes </para>
        /// </summary>
        [XmlElementAttribute("parameters", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOMappedDataParameters parameters;

        //For Mode Output only => Ex Update Every Row SQL after export   
        [XmlElementAttribute("SQL", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataSQL sql;
        //Input/Output
        [XmlElementAttribute("data", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFileRowData[] data;
        //Input/Output
        [XmlElementAttribute("table", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFileRowTable[] table;
        //Input/Output
        [XmlElementAttribute("logInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IODetLogInfo logInfo;
        //Input only
        [XmlElementAttribute("tradeImport", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EFS.TradeInformation.Import.TradeImport[] tradeImport;
        /// <summary>
        /// Représente une importation dans POSREQUEST
        /// <para>201303 seuls EXE,ABN,NEX,NAS,ASS sont gérés pour l'instant</para>
        /// </summary>
        //FI 20130320 [18547]
        [XmlElementAttribute("posRequestImport", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EFS.PosRequestInformation.Import.PosRequestImport[] posRequestImport;
        #endregion Members

        #region Accessors
        /// <summary>
        ///  Obtient un descriptif destiné au Log
        /// </summary>
        /// FI 20131122 [19233] Add property
        [XmlIgnore]
        public string LogRowDesc
        {
            get
            {
                return IOCommonTools.RowDesc(this);
            }
        }

        /// <summary>
        ///  Obtient l'élément message présent dans logInfo entre crochet
        ///  <para>Obtient string.Empty si l'élément logInfo est null</para>
        /// </summary>
        /// FI 20131122 [19233] Add property
        [XmlIgnore]
        public string LogRowInfoMsg
        {
            get
            {
                return (null != logInfo ? "[" + logInfo.message + "]" : string.Empty);
            }
        }
        #endregion Accessors

        #region Methods
        #region ExistRowParam
        // RD 20100728 [17101] Input: Use parameters in Post Mapping XML flow
        /// <summary>
        /// Retourne true si le paramètre {pParamName} existe 
        /// </summary>
        /// <param name="pParamName"></param>
        /// <returns></returns>
        public bool ExistRowParam(string pParamName)
        {
            bool ret = false;
            //
            if (parameters != null && ArrFunc.IsFilled(parameters.parameter))
            {
                pParamName = pParamName.ToUpper();
                for (int i = 0; i < parameters.parameter.Length; i++)
                {
                    if (StrFunc.IsFilled(parameters.parameter[i].name))
                    {
                        if (parameters.parameter[i].name.ToUpper() == pParamName)
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            //
            return ret;
        }
        #endregion ExistRowParam

        #region GetRowParamValue
        /// <summary>
        /// Retourne la valeur de 
        /// </summary>
        /// <param name="pParamName"></param>
        /// <returns></returns>
        public string GetRowParamValue(string pParamName)
        {
            string ret = null;
            //
            if (parameters != null && ArrFunc.IsFilled(parameters.parameter))
            {
                for (int i = 0; i < parameters.parameter.Length; i++)
                {
                    if (parameters.parameter[i].name.ToUpper() == pParamName.ToUpper())
                    {
                        ret = parameters.parameter[i].value;
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion GetRowParamValue

        #region GetRowDataValue
        /// <summary>
        /// Retourne la valeur de la donnée {pDataName} 
        /// </summary>
        /// <param name="pDataName"></param>
        /// <returns></returns>
        /// FI 20131022 [17861] Add Method
        public string GetRowDataValue(string pCS, string pDataName)
        {
            string ret = null;
            //
            if (data != null && ArrFunc.IsFilled(this.data))
            {
                for (int i = 0; i < ArrFunc.Count(data); i++)
                {
                    if (data[i].name.ToUpper() == pDataName.ToUpper())
                    {
                        ret = data[i].GetDataValue(pCS, null);
                        break;
                    }
                }
            }
            //
            return ret;
        }
        #endregion GetRowDataValue
        #endregion Methods
    }
    #endregion class TaskDetInOutFileRow
    //
    #region class IOTaskDetInOutFileRowData
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOutFileRowData (IOXML.cs)
    public class IOTaskDetInOutFileRowData : IOMappedData
    {
        [XmlAttribute()]
        public string status;
        [XmlIgnore()]
        public bool statusSpecified;

        //[XmlAttribute()]
        //public bool ispath;
    }
    #endregion class TaskDetInOutFileRowData
    //
    #region class IOTaskDetInOutFileRowTable
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOutFileRowTable (IOXML.cs)
    public class IOTaskDetInOutFileRowTable
    {
        #region Members
        /// <remarks/>
        [XmlAttribute()]
        public string name;

        [XmlAttribute()]
        public string action;

        [XmlAttribute()]
        public string sequenceno;

        //For mode Input Only => Send Mqueue after table Import
        [XmlElementAttribute("mqueue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFileRowTableMQueue mQueue;

        //For mode Output Only => Send SpheresLib after table Import
        [XmlElementAttribute("SpheresLib", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataSpheresLib[] spheresLib;

        [XmlElementAttribute("column", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOTaskDetInOutFileRowTableColumn[] column;
        #endregion Members

        #region Methods
        /// <summary>
        /// Retourne la colonne Mqueue
        /// </summary>
        /// <returns></returns>
        /// FI 20130426 [18344] Add method
        public IOTaskDetInOutFileRowTableColumn GetMqueueColumn()
        {
            IOTaskDetInOutFileRowTableColumn ret = null;

            foreach (IOTaskDetInOutFileRowTableColumn item in column)
            {
                if (item.isColumnMQueue)
                {
                    ret = item;
                    break;
                }
            }
            return ret;
        }
        #endregion Methods
    }
    #endregion class IOTaskDetInOutFileRowTable
    #region class IOTaskDetInOutFileRowTableMQueue
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOutFileRowTableMQueue (IOXML.cs)
    public class IOTaskDetInOutFileRowTableMQueue : IOParamDataList
    {
        #region Members
        [XmlAttribute()]
        public string name;

        [XmlAttribute()]
        public string action;
        #endregion Members
    }
    #endregion class IOTaskDetInOutFileRowTableMQueue
    #region class IOTaskDetInOutFileRowTableColumn
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOTaskDetInOutFileRowTableColumn (IOXML.cs)
    public class IOTaskDetInOutFileRowTableColumn : IOMappedData
    {
        [XmlAttribute()]
        public string datakey;

        [XmlAttribute()]
        public string datakeyupd;
        //
        // RD 20100728 [17101] Input: Use parameters in Post Mapping XML flow
        // To use parameters common for column
        // - it must be referenced like "parameters.{ParameterName}", in column value or ValueXML
        // - it's more priority then Row parameters 
        /// <summary>
        /// Représente les paramètres associés à la colonne
        /// </summary>
        [XmlElementAttribute("parameters", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOMappedDataParameters parameters;

        [XmlIgnore()]
        public bool isColumnMQueueSpecified;


        /// <summary>
        /// Représente une colonne MQueue
        /// <para>Lorsqu'un message queue doit être envoyé et qu'il existe une colonne MQueue, Spheres® récupère le contenu de la colonne</para>
        /// <para>Il ne peut y avoir qu'une colonne MQueue par table</para>
        /// </summary>
        /// FI 20130426 [18344] add isColumnMQueue
        [XmlAttribute("isColumnMQueue")]
        public bool isColumnMQueue;

        /// <summary>
        /// Retourne true si la colonne contient le paramètre {pParamName}
        /// </summary>
        /// <param name="pParamName"></param>
        /// <returns></returns>
        public bool ExistColumnParam(string pParamName)
        {
            bool ret = false;
            //
            if (parameters != null && ArrFunc.IsFilled(parameters.parameter))
            {
                pParamName = pParamName.ToUpper();
                for (int i = 0; i < parameters.parameter.Length; i++)
                {
                    if (StrFunc.IsFilled(parameters.parameter[i].name))
                    {
                        if (parameters.parameter[i].name.ToUpper() == pParamName)
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            //
            return ret;
        }
        /// <summary>
        /// Retourne la valeur du paramètre {pParamName}
        /// </summary>
        /// <param name="pParamName"></param>
        /// <returns></returns>
        public string GetColumnParamValue(string pParamName)
        {
            string ret = null;
            //
            if (parameters != null && ArrFunc.IsFilled(parameters.parameter))
            {
                for (int i = 0; i < parameters.parameter.Length; i++)
                {
                    if (parameters.parameter[i].name.ToUpper() == pParamName.ToUpper())
                    {
                        ret = parameters.parameter[i].value;
                        break;
                    }
                }
            }
            //
            return ret;
        }
    }
    #endregion class TaskDetInOutFileRowTableColumn
    //
    #region class IOMappedData
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOMappedData (IOXML.cs)
    public class IOMappedData : IOData
    {
        #region Members
        [XmlElementAttribute("ValueXML", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public XmlCDataSection valueXML;

        [XmlElementAttribute("default", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOMappedDataDefault Default;

        [XmlElementAttribute("controls", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOMappedDataControls Controls;

        [XmlIgnore]
        public string ValueVerified;

        [XmlIgnore]
        public string DefaultVerified;
        #endregion Members
        //
        #region Methods
        public string GetParamDataValue(string pParamName, string pCs, IDbTransaction pTransaction)
        {
            string retValue = string.Empty;
            ParamData paramReturn = null;
            //                        
            if ((null != spheresLib) && (true == spheresLib.GetParam(pParamName, out paramReturn)))
                retValue = paramReturn.GetDataValue(pCs, pTransaction);
            else if ((null != sql) && (true == sql.GetParam(pParamName, ref paramReturn)))
                retValue = paramReturn.GetDataValue(pCs, pTransaction);
            //
            return retValue;
        }
        #endregion Methods
    }
    #endregion class IOMappedData
    #region class IOMappedDataDefault
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOMappedDataDefault (IOXML.cs)
    public class IOMappedDataDefault : IOData
    {
    }
    #endregion class IOMappedDataDefault
    #region class IOMappedDataControls
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOMappedDataControls (IOXML.cs)
    public class IOMappedDataControls
    {
        #region Members
        [XmlElementAttribute("control", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOMappedDataControl[] Control;
        #endregion Members
    }
    #endregion class IOMappedDataControls
    #region class IOMappedDataControl
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOMappedDataControl (IOXML.cs)
    public class IOMappedDataControl : IOData
    {
        #region Members
        [XmlAttribute()]
        public string action;

        [XmlAttribute("return", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string @return;

        [XmlAttribute()]
        public string logtype;

        [XmlElementAttribute("logInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IODetLogInfo logInfo;

        [XmlIgnore]
        public string valueVerified;
        #endregion Members
    }
    #endregion class IOMappedDataControl
    #region class IOMappedDataParameters
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOMappedDataParameters (IOXML.cs)
    public class IOMappedDataParameters
    {
        #region Members
        [XmlElementAttribute("parameter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IOMappedData[] parameter;
        #endregion Members

        #region Methods
        /// <summary>
        ///  Retourne le paramètre dont name vaut {pParamName}
        /// </summary>
        /// <param name="pParamName"></param>
        /// <returns></returns>
        /// FI 20130426 [18344] add Method
        public IOMappedData GetParameter(string pParamName)
        {
            IOMappedData ret = null;
            //
            if (ArrFunc.IsFilled(parameter))
            {
                for (int i = 0; i < parameter.Length; i++)
                {
                    if (parameter[i].name.ToUpper() == pParamName.ToUpper())
                    {
                        ret = parameter[i];
                        break;
                    }
                }
            }
            return ret;
        }
        #endregion Methods
    }
    #endregion class IOMappedDataParameters
    //
    #region class IOData
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOData (IOXML.cs)
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IOMappedDataControl))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IOMappedDataDefault))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IOMappedData))]
    public class IOData : StringDynamicData
    {
    }
    #endregion class IOData
    #region class IOParamDataList
    /// <summary>
    /// Il est utile d'utiliser une seule classe de base pour gérer les collections de paramètres.
    /// Donc cette classe est à utiliser pour toutes les classes qui ont un membre de type ParamData[] :
    /// - DataSpheresLib
    /// - DataSQL
    /// </summary>
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IOParamDataList (IOXML.cs)
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(IOTaskDetInOutFileRowTableMQueue))]
    public class IOParamDataList
    {
        #region Members
        [XmlElementAttribute("Param", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ParamData[] param;
        #endregion Members
        //
        #region Methods
        #region ValuateParamWithParameters
        /// <summary>
        /// Valoriser les éléments qui font référence à des "parameters"
        /// Valoriser les parametres dynamiques
        /// </summary>
        /// <param name="pXMLTask"></param>
        /// <param name="pRow"></param>
        /// <param name="pColumn"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pLogMsg"></param>
        // PM 20180219 [23824] Remplacement du type Task par l'interface IIOTaskLaunching
        //public void ValuateParamWithParameters(Task pXMLTask, IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTableColumn pColumn, IDbTransaction pDbTransaction, ArrayList pLogMsg)
        public void ValuateParamWithParameters(IIOTaskLaunching pXMLTask, IOTaskDetInOutFileRow pRow, IOTaskDetInOutFileRowTableColumn pColumn, IDbTransaction pDbTransaction, ArrayList pLogMsg)
        {
            XmlDocument xmlDocumentParam = new XmlDocument();

            for (int i = 0; i < ArrFunc.Count(param); i++)
            {
                ArrayList paramMesgLog = new ArrayList(pLogMsg)
                {
                    "Param (" + param[i].name + ")"
                };

                if ((null != pRow.parameters) && ArrFunc.IsFilled(pRow.parameters.parameter))
                {
                    EFS_SerializeInfoBase serializeInfo;
                    try
                    {
                        serializeInfo = new EFS_SerializeInfoBase(typeof(ParamData), param[i]);
                        string xmlParam = CacheSerializer.Serialize(serializeInfo).ToString();
                        xmlDocumentParam.LoadXml(xmlParam);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(@"<b>Error to Load param in XML Document</b>" + Cst.CrLf + ArrFunc.GetStringList(paramMesgLog, Cst.CrLf), ex);
                    }

                    if (StrFunc.IsFilled(xmlDocumentParam.OuterXml) && xmlDocumentParam.OuterXml.Trim().Contains(IOCommonTools.ParametersTools.Parameters))
                    {
                        // Valoriser les éléments qui font référence à des parameters
                        // PM 20180219 [23824] Déplacé de IOTools vers IOCommonTools
                        //IOTools.ParametersTools.ValuateDataWithParameters(pRow, pColumn, xmlDocumentParam);
                        IOCommonTools.ParametersTools.ValuateDataWithParameters(pRow, pColumn, xmlDocumentParam);
                        //                
                        // La valeur du Param a eventuellement changée, alors mettre à jour le Param avec le nouveau flux                
                        serializeInfo = new EFS_SerializeInfoBase(typeof(ParamData), xmlDocumentParam.OuterXml);
                        param[i] = (ParamData)CacheSerializer.Deserialize(serializeInfo);
                    }
                }
                // Valoriser le Parametre dynamique
                ProcessMapping.GetDataValue(pXMLTask, param[i], pDbTransaction, ref param[i].value, paramMesgLog);
            }
        }
        #endregion
        //
        #region public GetParameterValue
        /// <summary>
        /// Retourne True, si le paramètre exist dans la collection. En plus, la valeur du paramètre est retournée en sortie dans {pParamValue}
        /// Retourne False, si si le paramètre n'exist pas dans la collection.
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="pParamValue"></param>
        /// <returns></returns>
        public bool GetParameterValue(string pName, out string pParamValue)
        {
            pParamValue = string.Empty;
            ParamData retParam = null;
            string name = pName.ToLower();

            for (int i = 0; i < ArrFunc.Count(param); i++)
            {
                if (StrFunc.IsFilled(param[i].name) && param[i].name.ToLower() == name)
                {
                    retParam = param[i];
                    break;
                }
            }

            if (retParam == null)
            {
                name = "p" + name;
                for (int i = 0; i < ArrFunc.Count(param); i++)
                {
                    if (StrFunc.IsFilled(param[i].name) && param[i].name.ToLower() == name)
                        retParam = param[i];
                }
            }
            //
            bool isParamExist = retParam != null;
            //
            if (isParamExist)
            {
                pParamValue = retParam.value;
                //
                if (StrFunc.IsFilled(retParam.dataformat))
                {
                    TypeData.TypeDataEnum dataType = TypeData.TypeDataEnum.unknown;
                    //
                    if (StrFunc.IsFilled(retParam.datatype))
                        dataType = TypeData.GetTypeDataEnum(retParam.datatype);
                    //
                    switch (dataType)
                    {
                        case TypeData.TypeDataEnum.datetime:
                        case TypeData.TypeDataEnum.date:
                        case TypeData.TypeDataEnum.time:
                            pParamValue = new DtFunc().GetDateTimeString(pParamValue, retParam.dataformat);
                            break;
                    }
                }
            }
            //
            return isParamExist;
        }
        #endregion GetParameterValue
        #endregion Methods
    }
    #endregion class IOParamDataList
    //
    #region class DynamicPathSQL
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.DynamicPathSQL (IOXML.cs)
    [XmlRoot(ElementName = "SQL", IsNullable = true)]
    public class DynamicPathSQL : DataSQL
    {
        [XmlAttribute()]
        public string format;
    }
    #endregion class DynamicPathSQL
    #region class DynamicPathSpheresLib
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.DynamicPathSpheresLib (IOXML.cs)
    [XmlRoot(ElementName = "SpheresLib", IsNullable = true)]
    public class DynamicPathSpheresLib : DataSpheresLib
    {
        [XmlAttribute()]
        public string format;
    }
    #endregion class DynamicPathSpheresLib
    //
    #region class DynamicPathBase
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.DynamicPathBase (IOXML.cs)
    public class DynamicPathBase
    {
        [XmlAttribute()]
        public string attribut;

        [XmlAttribute()]
        public string format;
    }
    #endregion class DynamicPathBase
    #region class DynamicPathRow
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.DynamicPathRow (IOXML.cs)
    [XmlRoot(ElementName = "Row", IsNullable = true)]
    public class DynamicPathRow : DynamicPathBase
    {
    }
    #endregion class DynamicPathRow
    #region class DynamicPathData
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.DynamicPathData (IOXML.cs)
    [XmlRoot(ElementName = "RowData", IsNullable = true)]
    public class DynamicPathData : DynamicPathBase
    {
        [XmlAttribute()]
        public string name;
    }
    #endregion class DynamicPathData
    //
    #region public class IODetLogInfo
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.IODetLogInfo (IOXML.cs)
    public class IODetLogInfo : ProcessLogInfo
    {
        #region Members
        [XmlAttribute("isexception", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool isException;
        #endregion Members

       

        #region Methods
        /// <summary>
        /// Si {pLogInfo.status} commence par "REJECT" 
        /// <para>- alors le replacer par "WARNING"</para>
        /// <para>- mettre au début de {pLogInfo.message} l'ancien statut</para>
        /// </summary>
        /// <param name="pLogInfo"></param>
        public void TransformLogInfo()
        {
            if (this.status.ToUpper().StartsWith("REJECT"))
            {
                if (StrFunc.IsEmpty(this.Code) || this.Number == 0)
                    this.message = this.message.Replace(this.message.Trim(), this.status.ToUpper() + ": " + this.message.Trim());

                this.status = ProcessStateTools.StatusWarning;
            }
        }
        #endregion Methods
    }
    #endregion  IODetLogInfo
    //
    #region public class MemberAttributeOverride
    /// <summary>
    /// Représente un membre pour lequel un attribut de sérialization existant sera substitué par un autre, 
    /// dynamiquement à la sérialisation.
    /// </summary>
    // PM 20180219 [23824] Déplacée à partir de EFS.SpheresIO.MemberAttributeOverride (IOXML.cs)
    public class MemberAttributeOverride
    {
        #region members
        public Type m_MemberToOverrideParent;
        public string m_MemberToOverrideName;
        public string m_MemberOverrideName;
        public XmlNodeType m_MemberOverrideNodeType;
        #endregion members
        //
        #region constructor
        public MemberAttributeOverride(Type pMemberToOverrideParent, string pMemberToOverrideName, string pMemberOverrideName, XmlNodeType pMemberOverrideNodeType)
        {
            m_MemberToOverrideParent = pMemberToOverrideParent;
            m_MemberToOverrideName = pMemberToOverrideName;
            m_MemberOverrideName = pMemberOverrideName;
            m_MemberOverrideNodeType = pMemberOverrideNodeType;
        }
        #endregion constructor
    }
    #endregion  MemberAttributeOverride
    #region public class IOXmlOverrides
    /// <summary>
    /// Représente la liste des substitution destiné la sérialization
    /// </summary>
    public class IOXmlOverrides
    {
        #region members
        private readonly ArrayList m_MembersToOverride;
        private readonly IDictionary<string, string> m_DataTypesToOverride;
        private readonly IDictionary<string, string> m_StatusToOverride;
        private readonly XmlAttributeOverrides m_XmlAttributeOverrides;
        #endregion members

        #region accessors
        public XmlAttributeOverrides XmlAttributeOverrides
        {
            get { return m_XmlAttributeOverrides; }
        }
        #endregion accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsExtend"></param>
        /// FI 20130321 [] add posrequest alias
        public IOXmlOverrides(bool pIsExtend)
        {
            m_DataTypesToOverride = new Dictionary<string, string>();
            //
            #region DataType Serialization
            // Type Boolean
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.@bool.ToString().ToLower(), "b");
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.boolean.ToString().ToLower(), "b");
            // Type Date
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.date.ToString().ToLower(), "d");
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.datetime.ToString().ToLower(), "dt");
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.time.ToString().ToLower(), "tm");
            // Type Decimal
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.dec.ToString().ToLower(), "dc");
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.@decimal.ToString().ToLower(), "dc");
            // Type Image
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.image.ToString().ToLower(), "im");
            // Type Integer
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.@int.ToString().ToLower(), "i");
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.integer.ToString().ToLower(), "i");
            // Type String
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.@string.ToString().ToLower(), "s");
            // Type Text
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.text.ToString().ToLower(), "tx");
            // Type xml
            // Attention le "x" est réservé aux types qui commencent par xml ( exemple: xml, xmlfo, ...)
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.xml.ToString().ToLower(), "x");
            m_DataTypesToOverride.Add(TypeData.TypeDataEnum.xml.ToString().ToLower() + "fo", "xfo");
            #endregion
            //
            m_StatusToOverride = new Dictionary<string, string>();
            //
            #region Status Serialization
            m_StatusToOverride.Add(ProcessStateTools.StatusError.ToLower(), "err");
            m_StatusToOverride.Add(ProcessStateTools.StatusSuccess.ToLower(), "suc");
            #endregion
            //
            m_MembersToOverride = new ArrayList();
            //
            #region Common Member attribute Serialization
            //IOTaskDetInOutFile
            Add(typeof(IOTaskDetInOutFile), "row", "r", XmlNodeType.Element);
            //
            //IOTaskDetInOutFileRow
            Add(typeof(IOTaskDetInOutFileRow), "status", "st", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRow), "useCache", "uc", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRow), "level", "lv", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRow), "childRows", "r", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRow), "data", "d", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRow), "table", "tbl", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRow), "parameters", "pms", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRow), "logInfo", "li", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRow), "sql", "sql", XmlNodeType.Element);
            //
            //IOTaskDetInOutFileRowData
            Add(typeof(IOTaskDetInOutFileRowData), "status", "st", XmlNodeType.Attribute);
            //
            //IOTaskDetInOutFileRowTable
            Add(typeof(IOTaskDetInOutFileRowTable), "name", "n", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRowTable), "action", "a", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRowTable), "sequenceno", "sn", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRowTable), "mQueue", "mq", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRowTable), "spheresLib", "sl", XmlNodeType.Element);
            Add(typeof(IOTaskDetInOutFileRowTable), "column", "c", XmlNodeType.Element);
            //
            //IOTaskDetInOutFileRowTableColumn
            Add(typeof(IOTaskDetInOutFileRowTableColumn), "datakey", "dk", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRowTableColumn), "datakeyupd", "dku", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRowTableColumn), "parameters", "pms", XmlNodeType.Element);
            //
            //IOMappedDataParameters
            Add(typeof(IOMappedDataParameters), "parameter", "pm", XmlNodeType.Element);
            //
            //IOMappedData
            Add(typeof(IOMappedData), "valueXML", "vx", XmlNodeType.Element);
            Add(typeof(IOMappedData), "Default", "df", XmlNodeType.Element);
            Add(typeof(IOMappedData), "Controls", "ctls", XmlNodeType.Element);
            //
            //IOMappedDataControls
            Add(typeof(IOMappedDataControls), "Control", "ctl", XmlNodeType.Element);
            //
            //IOMappedDataControl
            Add(typeof(IOMappedDataControl), "action", "a", XmlNodeType.Attribute);
            Add(typeof(IOMappedDataControl), "return", "rt", XmlNodeType.Attribute);
            Add(typeof(IOMappedDataControl), "logtype", "lt", XmlNodeType.Attribute);
            Add(typeof(IOMappedDataControl), "logInfo", "li", XmlNodeType.Element);
            //
            //StringDynamicData
            Add(typeof(StringDynamicData), "value", "v", XmlNodeType.Attribute);
            Add(typeof(StringDynamicData), "name", "n", XmlNodeType.Attribute);
            Add(typeof(StringDynamicData), "datatype", "t", XmlNodeType.Attribute);
            Add(typeof(StringDynamicData), "dataformat", "f", XmlNodeType.Attribute);
            Add(typeof(StringDynamicData), "spheresLib", "sl", XmlNodeType.Element);
            Add(typeof(StringDynamicData), "sql", "sql", XmlNodeType.Element);
            //
            //DataSpheresLib
            Add(typeof(DataSpheresLib), "function", "fn", XmlNodeType.Attribute);
            Add(typeof(DataSpheresLib), "param", "p", XmlNodeType.Element);
            //
            //DataSQL
            Add(typeof(DataSQL), "command", "cd", XmlNodeType.Attribute);
            Add(typeof(DataSQL), "name", "n", XmlNodeType.Attribute);
            Add(typeof(DataSQL), "result", "rt", XmlNodeType.Attribute);
            Add(typeof(DataSQL), "param", "p", XmlNodeType.Element);
            //
            //ParamData
            Add(typeof(ParamData), "direction", "drc", XmlNodeType.Attribute);
            Add(typeof(ParamData), "size", "sz", XmlNodeType.Attribute);
            //
            //IODetLogInfo
            Add(typeof(IODetLogInfo), "isException", "ex", XmlNodeType.Attribute);
            //
            //IOTaskDetInOutFileRowTableMQueue
            Add(typeof(IOTaskDetInOutFileRowTableMQueue), "name", "n", XmlNodeType.Attribute);
            Add(typeof(IOTaskDetInOutFileRowTableMQueue), "action", "a", XmlNodeType.Attribute);

            //IOParamDataList
            Add(typeof(IOParamDataList), "param", "p", XmlNodeType.Element);
            //
            //ProcessLogInfo
            Add(typeof(ProcessLogInfo), "status", "st", XmlNodeType.Attribute);
            Add(typeof(ProcessLogInfo), "dtstatus", "dst", XmlNodeType.Attribute);
            Add(typeof(ProcessLogInfo), "idDataIdent", "iddi", XmlNodeType.Attribute);
            Add(typeof(ProcessLogInfo), "idData", "idd", XmlNodeType.Attribute);
            Add(typeof(ProcessLogInfo), "message", "msg", XmlNodeType.Attribute);
            Add(typeof(ProcessLogInfo), "data1", "d1", XmlNodeType.Element);
            Add(typeof(ProcessLogInfo), "data2", "d2", XmlNodeType.Element);
            Add(typeof(ProcessLogInfo), "data3", "d3", XmlNodeType.Element);
            Add(typeof(ProcessLogInfo), "data4", "d4", XmlNodeType.Element);
            Add(typeof(ProcessLogInfo), "data5", "d5", XmlNodeType.Element);
            Add(typeof(ProcessLogInfo), "loDataTxt", "lodt", XmlNodeType.Element);
            #endregion
            //
            // Spécifiques à l'import des Trades
            if (pIsExtend)
            {
                #region Trade Import/Export Member attribute Serialization
                //IOTaskDetInOutFileRow
                Add(typeof(IOTaskDetInOutFileRow), "tradeImport", "ti", XmlNodeType.Element);
                Add(typeof(IOTaskDetInOutFileRow), "posRequestImport", "pi", XmlNodeType.Element);
                //
                //TradeImport
                Add(typeof(EFS.TradeInformation.Import.TradeImport), "settings", "stgs", XmlNodeType.Element);
                Add(typeof(EFS.TradeInformation.Import.TradeImport), "tradeInput", "tin", XmlNodeType.Element);
                Add(typeof(EFS.PosRequestInformation.Import.PosRequestImport), "posRequestInput", "pin", XmlNodeType.Element);
                //
                //TradeImportSettings
                Add(typeof(EFS.Import.ImportSettings), "importMode", "im", XmlNodeType.Attribute);
                Add(typeof(EFS.Import.ImportSettings), "user", "us", XmlNodeType.Attribute);
                Add(typeof(EFS.Import.ImportSettings), "conditions", "cdts", XmlNodeType.Element);
                Add(typeof(EFS.Import.ImportSettings), "condition", "cdt", XmlNodeType.Element);
                Add(typeof(EFS.Import.ImportSettings), "parameters", "pms", XmlNodeType.Element);
                Add(typeof(EFS.Import.ImportSettings), "parameter", "pm", XmlNodeType.Element);
                //
                //TradeCommonInput
                Add(typeof(EFS.TradeInformation.TradeCommonInput), "customCaptureInfos", "ccis", XmlNodeType.Element);
                Add(typeof(EFS.TradeInformation.TradeCommonInput), "customCaptureInfo", "cci", XmlNodeType.Element);
                Add(typeof(EFS.TradeInformation.TradeCommonInput), "fullcustomCaptureInfos", "fccis", XmlNodeType.Element);
                Add(typeof(EFS.TradeInformation.TradeCommonInput), "fullcustomCaptureInfo", "fcci", XmlNodeType.Element);
                Add(typeof(EFS.TradeInformation.TradeCommonInput), "tradeStatus", "tst", XmlNodeType.Element);
                //
                //CustomCaptureInfo
                Add(typeof(EFS.GUI.CCI.CustomCaptureInfo), "clientId", "cid", XmlNodeType.Attribute);
                Add(typeof(EFS.GUI.CCI.CustomCaptureInfo), "accessKey", "ak", XmlNodeType.Attribute);
                Add(typeof(EFS.GUI.CCI.CustomCaptureInfo), "dataType", "t", XmlNodeType.Attribute);
                Add(typeof(EFS.GUI.CCI.CustomCaptureInfo), "regex", "rgx", XmlNodeType.Attribute);
                Add(typeof(EFS.GUI.CCI.CustomCaptureInfo), "mandatory", "mdt", XmlNodeType.Attribute);
                Add(typeof(EFS.GUI.CCI.CustomCaptureInfo), "value", "v", XmlNodeType.Attribute);
                #endregion
            }
            //
            m_XmlAttributeOverrides = GetXmlAttributeOverrides();
        }
        #endregion constructor

        #region private Add
        /// <summary>
        /// Ajouter un élément à la collection
        /// </summary>
        /// <param name="pMemberToOverrideParent"></param>
        /// <param name="pMemberToOverrideName"></param>
        /// <param name="pMemberOverrideName"></param>
        /// <param name="pMemberOverrideNodeType"></param>
        private void Add(Type pMemberToOverrideParent, string pMemberToOverrideName, string pMemberOverrideName, XmlNodeType pMemberOverrideNodeType)
        {
            MemberAttributeOverride newMemberToOverride = new MemberAttributeOverride(pMemberToOverrideParent, pMemberToOverrideName, pMemberOverrideName, pMemberOverrideNodeType);
            //
            m_MembersToOverride.Add(newMemberToOverride);
        }
        #endregion private Add
        #region private GetXmlAttributeOverrides
        /// <summary>
        /// Obtenir l'objet XmlAttributeOverrides qui représente les caractéristiques de substitution des différents attributs
        /// </summary>
        /// <returns></returns>
        public XmlAttributeOverrides GetXmlAttributeOverrides()
        {
            XmlAttributeOverrides elementsOverrides = new XmlAttributeOverrides();
            foreach (MemberAttributeOverride memberToOverride in m_MembersToOverride)
            {
                XmlAttributes newElements = new XmlAttributes();
                if (memberToOverride.m_MemberOverrideNodeType == XmlNodeType.Element)
                {
                    XmlElementAttribute element = new XmlElementAttribute(memberToOverride.m_MemberOverrideName);
                    newElements.XmlElements.Add(element);
                }
                else if (memberToOverride.m_MemberOverrideNodeType == XmlNodeType.Attribute)
                    newElements.XmlAttribute = new XmlAttributeAttribute(memberToOverride.m_MemberOverrideName);

                elementsOverrides.Add(memberToOverride.m_MemberToOverrideParent, memberToOverride.m_MemberToOverrideName, newElements);
            }
            return elementsOverrides;
        }
        #endregion GetXmlAttributeOverrides
        //
        #region public GetMemberNewAttribute
        /// <summary>
        /// Obtient le nouvel attribut d'un membre
        /// </summary>
        /// <returns></returns>
        public string GetMemberNewAttribute(string pMemberToOverrideName)
        {
            return GetMemberNewAttribute(null, pMemberToOverrideName);
        }
        public string GetMemberNewAttribute(Type pMemberToOverrideParent, string pMemberToOverrideName)
        {
            string ret = pMemberToOverrideName;
            //
            foreach (MemberAttributeOverride memberToOverride in m_MembersToOverride)
            {
                if (((pMemberToOverrideParent == null) || (memberToOverride.m_MemberToOverrideParent == pMemberToOverrideParent)) &&
                    (memberToOverride.m_MemberToOverrideName == pMemberToOverrideName))
                {
                    ret = memberToOverride.m_MemberOverrideName;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion
        //
        #region public GetDataTypeOverride
        /// <summary>
        /// Obtient la substitution d'un TypeData.TypeDataEnum pour la sérialisation
        /// </summary>
        /// <returns></returns>
        public string GetDataTypeOverride(TypeData.TypeDataEnum pDataType)
        {
            return GetDataTypeOverride(pDataType.ToString());
        }
        /// <summary>
        /// Obtient la substitution d'un DataType pour la sérialisation
        /// </summary>
        /// <returns></returns>
        public string GetDataTypeOverride(string pDataType)
        {
            string ret = pDataType.ToLower();
            //
            foreach (KeyValuePair<string, string> kvpDataType in m_DataTypesToOverride)
            {
                if (kvpDataType.Key == ret)
                {
                    ret = kvpDataType.Value;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion GetMemberNewAttribute
        #region public GetDataTypeFromOverride
        /// <summary>
        /// Obtient un DataType à partir de la substitution utilisée lors de la sérialisation
        /// </summary>
        /// <returns></returns>
        public TypeData.TypeDataEnum GetDataTypeFromOverride(string pOverride)
        {
            string dataType = pOverride.ToLower();
            //
            foreach (KeyValuePair<string, string> kvpDataType in m_DataTypesToOverride)
            {
                if (kvpDataType.Value == pOverride)
                {
                    dataType = kvpDataType.Key;
                    break;
                }
            }
            //
            TypeData.TypeDataEnum ret = TypeData.TypeDataEnum.unknown;
            //
            if (Enum.IsDefined(typeof(TypeData.TypeDataEnum), dataType))
                ret = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), dataType);
            //
            return ret;
        }
        #endregion GetMemberNewAttribute
        //
        #region public GetStatusOverride
        /// <summary>
        /// Obtient la substitution d'un ProcessStateTools.StatusEnum pour la sérialisation
        /// </summary>
        /// <returns></returns>
        public string GetStatusOverride(ProcessStateTools.StatusEnum pStatus)
        {
            return GetStatusOverride(pStatus.ToString());
        }
        /// <summary>
        /// Obtient la substitution d'un Status pour la sérialisation
        /// </summary>
        /// <returns></returns>
        public string GetStatusOverride(string pStatus)
        {
            string ret = pStatus.ToLower();
            //
            foreach (KeyValuePair<string, string> kvpStatus in m_StatusToOverride)
            {
                if (kvpStatus.Key == ret)
                {
                    ret = kvpStatus.Value;
                    break;
                }
            }
            //
            return ret;
        }
        #endregion GetStatusOverride
        #region public GetStatusFromOverride
        /// <summary>
        /// Obtient un Status à partir de la substitution utilisée lors de la sérialisation
        /// </summary>
        /// <returns></returns>
        public ProcessStateTools.StatusEnum GetStatusFromOverride(string pOverride)
        {
            string status = pOverride.ToLower();
            //
            foreach (KeyValuePair<string, string> kvpStatus in m_StatusToOverride)
            {
                if (kvpStatus.Value == pOverride)
                {
                    status = kvpStatus.Key;
                    break;
                }
            }

            ProcessStateTools.StatusEnum ret = ProcessStateTools.StatusUnknownEnum;
            if (Enum.IsDefined(typeof(ProcessStateTools.StatusEnum), status))
                ret = (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), status);

            return ret;
        }
        #endregion GetMemberNewAttribute
    }
    #endregion  IOXmlOverrides
}

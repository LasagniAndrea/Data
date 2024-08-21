using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EfsML.DynamicData;
using FpML.Interface;
using System;
using System.Data;
using System.Xml.Serialization;

//FI 20130325
//espace de nom qui contient les classes communes utilisées apr l'importation des trades, et l'importation de POSREQUEST
//Peut-être faudra-t-il créé un projet EfsImport ?? A voir
namespace EFS.Import
{
    /// <summary>
    /// Représente un paramètre d'une importation
    /// <para>L'évaluation du paramètre peut être riche (l'usage du SQL est possible)</para>
    /// </summary>
    public class ImportParameter : StringDynamicData, IScheme
    {
        #region Constructors
        public ImportParameter()
            : base()
        {
        }
        #endregion Constructors

        #region IScheme Members
        /// <summary>
        /// Identification du paramètre 
        /// </summary>
        string IScheme.Scheme
        {
            get { return this.name; }
            set { this.name = value; }
        }
        /// <summary>
        /// valeur du paramètre
        /// </summary>
        string IScheme.Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion IScheme Members
    }

    /// <summary>
    /// Représente une condition 
    /// <para>L'importation ne sera appliquée que si la condition est true</para>
    /// <para>L'évaluation de la condition peut être riche (l'usage du SQL est possible)</para>
    /// </summary>
    public class ImportCondition : StringDynamicData
    {
        /// <summary>
        /// Description lorsque la condition est non respectée
        /// </summary>
        [XmlElementAttribute("logInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ProcessLogInfo logInfo;

        #region Constructors
        public ImportCondition()
            : base()
        {
        }
        #endregion Constructors
    }

    /// <summary>
    ///  Représente la configuration de l'importation
    /// </summary>
    public class ImportSettings
    {
        #region Members
        
        

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool importModeSpecified;
        /// <summary>
        /// Represente le Mode opératoire (Ex New,Update,...) (Cst.Capture.ModeEnum)
        /// <para>L'évaluation du mode opératoire peut être riche (l'usage du SQL est possible)</para>
        /// <para>L'évaluation est fonction de recordType</para>
        /// </summary>
        /// FI 20130528 [18662] importMode est de type importMode
        [System.Xml.Serialization.XmlElementAttribute("importMode")]
        public ImportMode importMode;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool userSpecified;
        /// <summary>
        /// Représente l'utilisateur sur lequel s'appuie l'importation
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("user")]
        public string user;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool conditionSpecified;
        /// <summary>
        /// Represente les conditions nécessaires pour autoriser l'importation
        /// </summary>
        [System.Xml.Serialization.XmlArray("conditions")]
        [System.Xml.Serialization.XmlArrayItem("condition", typeof(ImportCondition))]
        public ImportCondition[] condition;

        /// <summary>
        /// </summary>
        /// <remarks></remarks>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool parametersSpecified;
        //
        /// <summary>
        /// Liste des paramètres de l'importation 
        /// </summary>
        /// <remarks>Par exemple : Si Mode Update, seul l'identifiant du trade est  obligatoire</remarks>
        [System.Xml.Serialization.XmlArray("parameters")]
        [System.Xml.Serialization.XmlArrayItem("parameter", typeof(ImportParameter))]
        public ImportParameter[] parameter;
        #endregion Members

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public ImportSettings()
        {
        }
        #endregion

        #region Method

        /// <summary>
        /// Vérification que les toutes les conditions de l'import sont vérifiées
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pLogHeader"></param>
        /// <param name="pMsgNotOkCondition">Message présent dans la condition, lorsqu'une condition est non respectée</param>
        /// <returns></returns>
        /// FI 20131122 [19233] add parameter pLogHeader
        public bool IsConditionOk(string pCs, IDbTransaction pDbTransaction, string pLogHeader, out ProcessLogInfo pMsgNotOkCondition)
        {
            bool ret = true;
            pMsgNotOkCondition = null;

            if (conditionSpecified)
            {
                for (int i = 0; i < ArrFunc.Count(condition); i++)
                {
                    ret = ret && BoolFunc.IsTrue(condition[i].GetDataValue(pCs, pDbTransaction));
                    if (false == ret)
                    {
                        //pMsgNotOkCondition = "Condition N° " + (i + 1).ToString() + ", Condition Name: " + condition[i].name;
                        pMsgNotOkCondition = condition[i].logInfo;
                        if (null == pMsgNotOkCondition)
                        {
                            pMsgNotOkCondition = new ProcessLogInfo
                            {
                                status = ProcessStateTools.StatusWarningEnum.ToString(),
                                levelOrder = 4
                            };
                            pMsgNotOkCondition.SetMessageAndData(new string[] { "LOG-06024", pLogHeader, (i + 1).ToString(), condition[i].name });
                        }
                        pMsgNotOkCondition.levelOrder = 4;
                        break;
                    }
                }
            }
            return ret;
        }

        
        #endregion Method
    }

    /// <summary>
    ///  Représente le mode opératoire dans Spheres® (Ex New,Update,...) (Cst.Capture.ModeEnum)
    /// </summary>
    /// FI [18662] Add class ImportMode
    public class ImportMode : StringDynamicData
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean actionTypeSpecified;

        /// <summary>
        /// Représente le type d'action à effectuer avec l'enregistrement (cette donnée est issue du flux intégré)
        /// <para>Type d'action est utilisé pour déterminer le mode opératoire de Spheres (Cst.Capture.ModeEnum)</para>
        /// <para>action Type est nécessairement renseigné dans les intégration des trades ETD si utilisation de xml de mapping LSD_TradeImport_Map.xsl</para>
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("actionType")]
        public string actionType;
    }

    /// <summary>
    /// Représente le type d'action
    /// <para>Type d'action permet de déterminer le mode opératoire de Spheres</para>
    /// </summary>
    /// FI 20130528 [18662] add Enum
    /// FI 20160907 [21831] Modify
    public enum ActionTypeEnum
    {
        /// <summary>
        /// New : Opération la plus courante, l'enregistrement est créé dans Spheres®
        /// </summary>
        N,
        /// <summary>
        /// Modify : Modification d'un enregistrement
        /// <para>Lorsque  le record n'existe pas Spheres I/O®, l'enregistrement est créé dans Spheres®</para>
        /// </summary>
        M,
        /// <summary>
        /// Update : Modification d'un enregistrement
        /// <para>Lorsque  le record n'existe pas Spheres I/O® génère un warning.</para>
        /// </summary>
        U,
        /// <summary>
        /// Suppress : Suppression d'un enregistrement déjà existant
        /// <para>Lorsque  le record n'existe pas Spheres I/O® génère un warning.</para>
        /// </summary>
        S,
        /// <summary>
        /// Give-up 
        /// <para>Spécifique à l'importation des trades depuis BCS</para>
        /// </summary>
        G,
        /// <summary>
        ///  Modification des frais d'un trade 
        /// <para>Spécifique à l'importation des trades depuis BCS</para>
        /// </summary>
        /// FI 20160907 [21831] Add
        F,
        /// <summary>
        /// Modification du trader d'un trade
        /// <para>Spécifique à l'importation des trades depuis BCS</para>
        /// </summary>
        /// FI 20170824 [23339] 
        T,
    }
}
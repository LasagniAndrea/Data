using EFS.ACommon;
using System.Xml.Schema;
using System.Xml.Serialization;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Classe qui pilote le chargement d'un enum dans un controle list
    /// </summary>
    [XmlRoot(ElementName = "LoadEnumArguments", IsNullable = true)]
    public class LoadEnumArguments
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        public string code;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isWithEmpty;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isFromSQLView;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isResource;
        //PL 20140128 Newness
        [System.Xml.Serialization.XmlTextAttribute()]
        public string resourcePrefix;

        /// <summary>
        /// Liste restrictive des valeurs de l'enum à charger ou Liste des valeurs à exclure
        /// </summary>
        [XmlElementAttribute("forcedEnum", Form = XmlSchemaForm.Unqualified)]
        public string[] forcedEnum;
        /// <summary>
        /// si True les valeurs de présentes dans forcedEnum sont exclues
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isExcludeForcedEnum;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isDisplayValue;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool isDisplayValueAndExtendValue;
        /// <summary>
        /// /
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string customValue;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string condition;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string filtre;
        #endregion

        #region constructor
        public LoadEnumArguments()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pListRetrievalData"></param>
        /// <param name="pWithEmpty"></param>
        /// <returns></returns>
        public static LoadEnumArguments GetArguments(string pListRetrievalData, bool pWithEmpty)
        {
            return GetArguments(pListRetrievalData, pWithEmpty, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pListRetrievalData"></param>
        /// <param name="pWithEmpty"></param>
        /// <param name="pIsResource"></param>
        /// <returns></returns>
        public static LoadEnumArguments GetArguments(string pListRetrievalData, bool pWithEmpty, bool pIsResource)
        {
            return GetArguments(pListRetrievalData, pWithEmpty, pIsResource, null);
        }
        //PL 20140128 Add pResourcePrefix
        public static LoadEnumArguments GetArguments(string pListRetrievalData, bool pWithEmpty, bool pIsResource, string pResourcePrefix)
        {
            // ------------------------
            // 20081113 RD : DDL avec Filtre dans le Referential
            string listRetrievalValue = pListRetrievalData;
            //
            string[] listRetrieval = pListRetrievalData.Split(".".ToCharArray());
            if (ArrFunc.IsFilled(listRetrieval) && listRetrieval.Length > 1)
            {
                listRetrievalValue = pListRetrievalData.Remove(0, ((string)(listRetrieval[0] + ".")).Length);
            }
            // ------------------------
            //
            //20100527 FI Remplace le try cath par StrFunc.IsXML pour raison de performance
            bool isComplexType = StrFunc.IsXML(listRetrievalValue);
            //
            LoadEnumArguments loadEnum = null;
            if (isComplexType)
            {
                loadEnum = LoadEnumArguments.GetArgumentsFromSerializedText(listRetrievalValue);
                loadEnum.isWithEmpty = pWithEmpty;
                loadEnum.isResource = pIsResource;
                loadEnum.resourcePrefix = pResourcePrefix;   
            }
            else if ((false == isComplexType) && listRetrievalValue.StartsWith("[") && listRetrievalValue.EndsWith("]"))
            {
                listRetrievalValue = listRetrievalValue.Remove(0, 1);
                listRetrievalValue = listRetrievalValue.Remove(listRetrievalValue.Length - 1, 1);
                loadEnum = new LoadEnumArguments
                {
                    isWithEmpty = pWithEmpty,
                    isResource = pIsResource,
                    resourcePrefix = pResourcePrefix
                };

                string[] list = listRetrievalValue.Split(";".ToCharArray());
                foreach (string s in list)
                {
                    string[] listValue = s.Split(":".ToCharArray());
                    switch (listValue[0])
                    {
                        case "code":
                            loadEnum.code = listValue[1];
                            break;
                        case "isfromsqlview":
                            loadEnum.isFromSQLView = BoolFunc.IsTrue(listValue[1]);
                            break;
                        case "isresource":
                            loadEnum.isResource = BoolFunc.IsTrue(listValue[1]);
                            break;
                        case "resourceprefix":
                            loadEnum.resourcePrefix = listValue[1];
                            break;
                        case "iswithoutextendvalue":
                            loadEnum.isDisplayValue = BoolFunc.IsTrue(listValue[1]);
                            break;
                        case "isdisplayvalue":
                            loadEnum.isDisplayValue = BoolFunc.IsTrue(listValue[1]);
                            break;
                        case "isdisplayvalueandextendvalue":
                            loadEnum.isDisplayValueAndExtendValue = BoolFunc.IsTrue(listValue[1]);
                            break;
                        case "customvalue":
                            loadEnum.customValue = listValue[1];
                            break;
                        case "condition":
                            loadEnum.condition = listValue[1];
                            break;
                        case "filtre":
                            loadEnum.filtre = listValue[1];
                            break;
                        case "forcedenum":
                            loadEnum.forcedEnum = listValue[1].Split("|".ToCharArray());
                            break;
                    }
                }
            }
            //
            if (null == loadEnum)
            {
                loadEnum = new LoadEnumArguments
                {
                    isWithEmpty = pWithEmpty,
                    code = listRetrievalValue,
                    isFromSQLView = false,
                    isResource = pIsResource,
                    resourcePrefix = pResourcePrefix,
                    forcedEnum = null,
                    isDisplayValue = false,
                    isDisplayValueAndExtendValue = false,
                    customValue = null,
                    condition = null,
                    filtre = null
                };
            }
            //
            return loadEnum;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pText"></param>
        /// <returns></returns>
        public static LoadEnumArguments GetArgumentsFromSerializedText(string pText)
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(LoadEnumArguments), pText);
            return (LoadEnumArguments)CacheSerializer.Deserialize(serializeInfo);
        }
        #endregion
    }
}

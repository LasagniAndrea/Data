#region Using Directives
using EFS.ACommon;
using EFS.Common;
using System.Text;
using System.Xml.Serialization;
#endregion Using Directives

namespace EfsML.DynamicData
{
    /// <summary>
    /// 
    /// </summary>
    [XmlRoot(ElementName = "Datas", IsNullable = true)]
    public class StringDynamicDatas
    {
        [XmlElementAttribute("Data", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public StringDynamicData[] data;

        #region Indexors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        public StringDynamicData this[string pName]
        {
            get
            {

                StringDynamicData ret = null;
                foreach (StringDynamicData sData in data)
                {
                    if (sData.name == pName)
                    {
                        ret = sData;
                        break;
                    }
                }
                return ret;

            }
        }
        #endregion

        #region  Method
        /// <summary>
        /// 
        /// </summary>
        public string Serialize()
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(StringDynamicDatas), this)
            {
                IsXMLTrade = false,
                IsWithoutNamespaces = true
            };
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pText"></param>
        /// <returns></returns>
        public static StringDynamicDatas LoadFromText(string pText)
        {
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(StringDynamicDatas), pText);
            return (StringDynamicDatas)CacheSerializer.Deserialize(serializeInfo);

        }

        /// <summary>
        ///  Remplace dans une string les mots clefs %%DA:{nom du dynamicArgument}.{method du dynamicArgument}%%  par leurs valeurs 
        ///  <para>Retourne le résultat obtenu</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public string ReplaceInString(string pCS, ref string pData)
        {
            string ret = pData;
            for (int i = 0; i < ArrFunc.Count(data); i++)
                ret = data[i].ReplaceInString(pCS, ret);
            return ret;
        }
        #endregion
    }
}

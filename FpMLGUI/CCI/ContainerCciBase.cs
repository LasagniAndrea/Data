#region Using Directives
using System;
using System.Collections.Generic;  
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using System.Linq;
#endregion Using Directives

using EFS.ACommon;
using EFS.Common.Web;


namespace EFS.GUI.CCI
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20190731 [XXXXX] Add
    /// Toutes les classes qui implémentent IContainerCci devrait hériter de ContainerCciBase
    public abstract class ContainerCciBase : IContainerCci
    {
        

        /// <summary>
        /// Obtiens le prefix 
        /// </summary>
        public string Prefix
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtiens la coollection ccis
        /// </summary>
        public CustomCaptureInfosBase CcisBase
        {
            get;
            private set;
        }


        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="prefixNumber"></param>
        /// <param name="pCcis"></param>
        public ContainerCciBase(string pPrefix, int prefixNumber, CustomCaptureInfosBase pCcis)
        {
            Prefix = string.Empty;
            
            if (StrFunc.IsFilled(pPrefix))
                Prefix = pPrefix + NumberPrefix(prefixNumber) + CustomObject.KEY_SEPARATOR;
            
            CcisBase = pCcis;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pCcis"></param>
        public ContainerCciBase(string pPrefix, CustomCaptureInfosBase pCcis)
            : this(pPrefix, -1, pCcis)
        {

        }
        
        #endregion


        /// <summary>
        /// 
        /// </summary>
        private static string NumberPrefix(int pPrefixNumber)
        {
            string ret = string.Empty;
            if (0 < pPrefixNumber)
                ret = pPrefixNumber.ToString();
            return ret;

        }

        #region IContainerCci implementation

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public string CciClientId<T>(T pEnumValue) where T : struct
        {
            return CciClientId(pEnumValue.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_Key"></param>
        /// <returns></returns>
        public string CciClientId(string pClientId_Key)
        {
            return Prefix + pClientId_Key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public CustomCaptureInfo Cci<T>(T pEnumValue) where T : struct
        {
            return CcisBase[CciClientId(pEnumValue.ToString())];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_Key"></param>
        /// <returns></returns>
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return CcisBase[CciClientId(pClientId_Key)];
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            //FI 20110418 [17405] ccis.Contains en commentaire => Tuning
            //isOk = ccis.Contains(pClientId_WithoutPrefix);
            //isOk = isOk && (pClientId_WithoutPrefix.StartsWith(m_Prefix));	
            bool isOk = (pClientId_WithoutPrefix.StartsWith(Prefix));
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(Prefix.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pEnumValue"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCci<T>(T pEnumValue, CustomCaptureInfo pCci) where T : struct
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }

        #endregion
    }
}

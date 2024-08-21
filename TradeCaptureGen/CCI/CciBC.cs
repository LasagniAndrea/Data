#region Using Directives
using EFS.ACommon;
using EFS.Common;
using FpML.Interface;
using System;
using System.Collections;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciBC
    /// <summary>
    ///  Dictionnaire qui contient les informations nécessaires à la détermination des BusinessCenters
    ///  <para>ChaqueItem contient le clientId (key) et le type de donnée (value)[Actor,Currency,Asset]</para>
    /// </summary>
    /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
    public class CciBC : DictionaryBase
    {
        #region enum
        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        public enum TypeReferentialInfo
        {
            Actor,
            Asset,
            Currency,
        }
        #endregion enum

        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly CciTradeBase _cciTrade;
        #endregion

        #region properties
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        #endregion

        #region constructor
        public CciBC(CciTradeBase ptrade)
        {
            _cciTrade = ptrade;
        }
        #endregion constructor

        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public TypeReferentialInfo this[String pClientId]
        {
            get
            {
                return ((TypeReferentialInfo)Dictionary[pClientId]);
            }
            set
            {
                Dictionary[pClientId] = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <param name="value"></param>
        public void Add(string pClientId, TypeReferentialInfo value)
        {
            string clientId = pClientId;
            // Si le clientId est une reference ex Fra_seller => Recherche du clientid (party ou broker) correspondant 
            // Cette classe travaille avec ce cci car elle s'appuie sur (sql_actor) cci.sql_table 
            if ((value == TypeReferentialInfo.Actor) && Ccis.Contains(pClientId) && (null == Ccis[pClientId].Sql_Table))
            {
                string clientIdActor = _cciTrade.ClientIdFromXmlId(Ccis[pClientId].NewValue);
                if (StrFunc.IsFilled(clientIdActor))  
                    clientId = clientIdActor;
            }

            if (!Dictionary.Contains(clientId))
                Dictionary.Add(clientId, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        public bool Contains(string pClientId)
        {
            return Dictionary.Contains(pClientId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        public void Remove(string pClientId)
        {
            Dictionary.Remove(pClientId);
        }
        
        /// <summary>
        /// Retourne un array de businessCenter 
        /// </summary>
        /// <returns></returns>
        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        // EG 20180205 [23769] Add dbTransaction  
        public IBusinessCenter[] GetBusinessCenters()
        {
            IBusinessCenter[] ret = null;
            //Attention les businessCenters des acteurs n'est plus considérés
            string[] actor = null;
            string[] cur = GetIdItem(TypeReferentialInfo.Currency);
            string[] assetMarket = GetIdItem(TypeReferentialInfo.Asset);

            IProduct product = _cciTrade.CurrentTrade.Product;
            IBusinessCenters bcs = ((IProductBase)product).LoadBusinessCenters(_cciTrade.CSCacheOn, null, actor, cur, assetMarket);

            if (null != bcs)
                ret = bcs.BusinessCenter;
            //
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        private bool IsItemOk(string pClientId)
        {
            TypeReferentialInfo typeInfo = this[pClientId];

            bool isOk = Ccis.Contains(pClientId);
            if (isOk)
                isOk = StrFunc.IsFilled(Ccis[pClientId].NewValue);
            if (isOk)
            {
                switch (typeInfo)
                {
                    case TypeReferentialInfo.Actor:
                    case TypeReferentialInfo.Asset:
                        isOk = (null != Ccis[pClientId].Sql_Table);
                        break;
                }
            }
            return isOk;
        }
        
        /// <summary>
        /// Retourne les identifiants présents dans le dictionnaires par type d'info
        /// <para>Par Exemple retourne les IdA des acteurs</para>
        /// <para>Par Exemple retourne les IdC des devises</para>
        /// </summary>
        /// <param name="pTypeInfo"></param>
        /// <returns></returns>
        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        private string[] GetIdItem(TypeReferentialInfo pTypeInfo)
        {
            string[] ret = null;
            ArrayList al = new ArrayList();

            foreach (string clientId in Dictionary.Keys)
            {
                if ((this[clientId] == pTypeInfo))
                {
                    if (IsItemOk(clientId))
                    {
                        string itemValue = string.Empty;
                        switch (pTypeInfo)
                        {
                            case TypeReferentialInfo.Actor:
                                itemValue = ((SQL_Actor)Ccis[clientId].Sql_Table).Id.ToString();
                                break;
                            case TypeReferentialInfo.Asset:
                                itemValue = ((SQL_AssetBase)Ccis[clientId].Sql_Table).IdM.ToString();
                                break;
                            case TypeReferentialInfo.Currency:
                                itemValue = (Ccis[clientId].NewValue);
                                break;
                        }
                        if (StrFunc.IsFilled(itemValue))
                            al.Add(itemValue);
                    }
                }
            }
            //
            if (ArrFunc.IsFilled(al))
                ret = (string[])al.ToArray(typeof(string));
            //
            return ret;
        }
        #endregion

    }
    #endregion
}

#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region public class CciNettingInformation
    /// <summary>
    /// Description résumée de CciSettlementInformations.
    /// </summary>
    public class CciNettingInformation : IContainerCciFactory, IContainerCci, IContainerCciSpecified
    {
        INettingInformationInput nettingInfoInput;
        readonly string prefix;

        private readonly CciTradeBase cciTrade;
        private readonly TradeCustomCaptureInfos _ccis;

        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("nettingMethod")]
            nettingMethod,
            [System.Xml.Serialization.XmlEnumAttribute("nettingDesignation")]
            nettingDesignation,
            unknown
        }
        #endregion Enum

        #region constructor
        public CciNettingInformation(CciTradeBase pCciTrade, string pPrefix, INettingInformationInput pNettingInfo)
        {
            cciTrade = pCciTrade;
            _ccis = pCciTrade.Ccis;
            nettingInfoInput = pNettingInfo;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion constructor

        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return (Cci(CciEnum.nettingMethod).IsFilled); } }
        #endregion

        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }

        public INettingInformationInput NettingInformationInput
        {
            set { nettingInfoInput = value; }
            get { return nettingInfoInput; }
        }
        #endregion accessors

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, nettingInfoInput);
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region Dump_ToDocument
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140331 [19793] Tuning => Boucle sur l'enum plutôt que les ccis
        public void Dump_ToDocument()
        {
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                { 
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        #region nettingMethod
                        case CciEnum.nettingMethod:
                            cciTrade.CurrentTrade.NettingInformationInputSpecified = StrFunc.IsFilled(data);
                            if (cciTrade.CurrentTrade.NettingInformationInputSpecified)
                            {
                                NettingMethodEnum nettingMethodEnum = (NettingMethodEnum)System.Enum.Parse(typeof(NettingMethodEnum), data, true);
                                nettingInfoInput.NettingMethod = nettingMethodEnum;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion
                        //
                        #region nettingDesignation
                        case CciEnum.nettingDesignation:
                            SQL_NetDesignation sql_netDesignation = null;
                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            //
                            if (StrFunc.IsFilled(data))
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";
                                    sql_netDesignation = new SQL_NetDesignation(cciTrade.CSCacheOn, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    isLoaded = sql_netDesignation.IsLoaded && (sql_netDesignation.RowsCount == 1);
                                    //
                                    if (isLoaded)
                                        break;
                                }
                                //
                                if (isLoaded)
                                {
                                    cci.NewValue = sql_netDesignation.Identifier;
                                    cci.Sql_Table = sql_netDesignation;
                                    cci.ErrorMsg = string.Empty;
                                    //
                                    nettingInfoInput.NettingDesignation.OTCmlId = sql_netDesignation.Id;
                                    nettingInfoInput.NettingDesignation.Value = sql_netDesignation.Identifier;
                                }
                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_NetDesignationNotFound") : string.Empty);
                            }
                            if (false == isLoaded)
                            {
                                cci.ErrorMsg = string.Empty;
                                if (cci.IsFilled || (cci.IsEmpty && cci.IsMandatory))
                                    cci.ErrorMsg = Ressource.GetString("NotFound");
                                //
                                cci.Sql_Table = null;
                                nettingInfoInput.NettingDesignation.OTCmlId = 0;
                                nettingInfoInput.NettingDesignation.Value = string.Empty;
                            }
                            //
                            nettingInfoInput.NettingDesignationSpecified = isLoaded;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            
            
            
            
            
        }
        #endregion  Dump_ToDocument
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// FI 20140331 [19793] Tuning => Boucle sur l'enum plutôt que les ccis
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = _ccis[prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        #region nettingMethod
                        case CciEnum.nettingMethod:
                            if (cciTrade.CurrentTrade.NettingInformationInputSpecified)
                                data = nettingInfoInput.NettingMethod.ToString();
                            break;
                        #endregion
                        #region nettingDesignation
                        case CciEnum.nettingDesignation:
                            if (nettingInfoInput.NettingDesignationSpecified)
                            {
                                SQL_NetDesignation sql_nettingDesignation = null;
                                string nettingdesignation = nettingInfoInput.NettingDesignation.Value;
                                int idNettingdesignation = nettingInfoInput.NettingDesignation.OTCmlId;
                                //
                                if (idNettingdesignation > 0)
                                    sql_nettingDesignation = new SQL_NetDesignation(cciTrade.CSCacheOn, idNettingdesignation);
                                else if (StrFunc.IsFilled(nettingdesignation))
                                    sql_nettingDesignation = new SQL_NetDesignation(cciTrade.CSCacheOn, nettingdesignation);
                                //
                                if (sql_nettingDesignation.IsLoaded)
                                {
                                    cci.Sql_Table = sql_nettingDesignation;
                                    data = sql_nettingDesignation.Identifier;
                                }
                            }
                            break;
                        #endregion css
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        #endregion
        #region ProcessInitialize
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (elt)
                {
                    #region nettingMethod
                    case CciEnum.nettingMethod:
                        bool bClearDesignation = false;
                        //
                        if (pCci.IsEmpty)
                            bClearDesignation = true;
                        else if ((pCci.IsFilled) && (nettingInfoInput.NettingMethod != NettingMethodEnum.Designation))
                            bClearDesignation = true;
                        //
                        if (bClearDesignation)
                            Ccis.SetNewValue(CciClientId(CciEnum.nettingDesignation), string.Empty);
                        //
                        Ccis.Set(CciClientId(CciEnum.nettingDesignation), "IsEnabled", (false == bClearDesignation));

                        break;
                    #endregion

                    #region nettingDesignation
                    case CciEnum.nettingDesignation:
                        if (pCci.IsFilled)
                        {
                            SQL_NetDesignation sqlNetDesignation = (SQL_NetDesignation)pCci.Sql_Table;
                            if (null != sqlNetDesignation)
                                Ccis.SetNewValue(CciClientId(CciEnum.nettingMethod), NettingMethodEnum.Designation.ToString());
                        }
                        else
                            Ccis.SetNewValue(CciClientId(CciEnum.nettingMethod), string.Empty);
                        break;
                    #endregion

                    #region Default
                    default:
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        break;
                    #endregion Default
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region IsClientId_XXXXX
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            return isOk;
        }
        #endregion IsClientId_XXXXX
        #region CleanUP
        public void CleanUp() { }
        #endregion CleanUP
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {

        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            Ccis.Set(CciClientId(CciEnum.nettingDesignation), "IsEnabled", nettingInfoInput.NettingDesignationSpecified);
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion

        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci


    }

    #endregion
}

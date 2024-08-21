#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using FpML.Interface;
using System.Collections;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de CciUnderlyingAsset. 
    /// </summary>
    public class CciUnderlyingAsset : ContainerCciBase, IContainerCciFactory
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("currency")]
            currency,
            exch,
            unknown
        }
        #endregion CciEnum
        #endregion Enums

        #region Members
        private readonly CciTradeBase cciTrade;
        public IUnderlyingAsset underlyingAsset;
        
        public CciSecurityIdSourceScheme[] cciSecurityIdSourceScheme;
        #endregion Members

        #region Accessors
        #region InstrumentIdLenght
        public int InstrumentIdLenght
        {
            get { return ArrFunc.IsFilled(cciSecurityIdSourceScheme) ? cciSecurityIdSourceScheme.Length : 0; }
        }
        #endregion InstrumentIdLenght
        #endregion

        #region ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        #endregion ccis

        #region Constructors
        public CciUnderlyingAsset(CciTradeBase pTrade, string pPrefix, IUnderlyingAsset pUnderlyingAsset) : base(pPrefix, pTrade.Ccis)
        {
            underlyingAsset = pUnderlyingAsset;
            cciTrade = pTrade;
        }
        #endregion Constructors

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public virtual void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, this.GetType().BaseType, underlyingAsset, "CciEnum");
            //
            InitializeInstrumentId_FromCci();
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        public virtual void AddCciSystem()
        {
            for (int i = 0; i < InstrumentIdLenght; i++)
                cciSecurityIdSourceScheme[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public virtual void Initialize_FromDocument()
        {

            string data;
            bool isSetting;
            SQL_Table sql_Table;
            string clientId_Key;
            //
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    data = string.Empty;
                    isSetting = true;
                    sql_Table = null;
                    #endregion
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        case CciEnum.currency:
                            #region currency
                            if (underlyingAsset.CurrencySpecified)
                                data = underlyingAsset.Currency.Value;
                            #endregion
                            break;
                        case CciEnum.exch:
                            #region Exchange
                            //ExchangeId
                            if (underlyingAsset.ExchangeIdSpecified)
                            {
                                data = underlyingAsset.ExchangeId.Value;
                                SQL_Market sqlMarket = new SQL_Market(cciTrade.CS, SQL_TableWithID.IDType.FIXML_SecurityExchange, underlyingAsset.ExchangeId.Value,
                                SQL_Table.ScanDataDtEnabledEnum.No);
                                if (sqlMarket.LoadTable())
                                    sql_Table = sqlMarket;
                            }

                            #endregion Exchange
                            break;
                        default:
                            isSetting = false;
                            break;

                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            for (int i = 0; i < InstrumentIdLenght; i++)
                cciSecurityIdSourceScheme[i].Initialize_FromDocument();

        }
        #endregion

        #region ProcessInitialize
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    #region default
                    default:

                        break;
                    #endregion default
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public virtual void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region IsClientId_PayerOrReceiver
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            return isOk;
        }
        #endregion
        #region CleanUp
        public virtual void CleanUp()
        {
            for (int i = 0; i < InstrumentIdLenght; i++)
                cciSecurityIdSourceScheme[i].CleanUp();
         
            for (int i = underlyingAsset.InstrumentId.Length - 1; -1 < i; i--)
            {
                bool isRemove = StrFunc.IsEmpty(underlyingAsset.InstrumentId[i].Scheme) || StrFunc.IsEmpty(underlyingAsset.InstrumentId[i].Value);
                if (isRemove)
                    ReflectionTools.RemoveItemInArray(underlyingAsset, "instrumentId", i);
            }

        }
        #endregion
        #region SetDisplay
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion
        #region SetEnabled
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, this.GetType().BaseType, "CciEnum", "IsEnabled", pIsEnabled);

            for (int i = 0; i < InstrumentIdLenght; i++)
                cciSecurityIdSourceScheme[i].SetEnabled(pIsEnabled);
        }
        #endregion SetEnabled
        #region RemoveLastItemInArray
        public virtual void RemoveLastItemInArray(string pPrefix)
        {
        }
        #endregion
        #region RefreshCciEnabled
        public virtual void RefreshCciEnabled()
        {
        }
        #endregion
        #region Initialize_Document
        public virtual void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion
        #region Dump_ToDocument
        public virtual void Dump_ToDocument()
        {

            bool isSetting;
            string data;
            string clientId, clientId_WithoutPrefix;
            string clientId_Element;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            //
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                //On ne traite que les contrôle dont le contenu à changé
                if (cci.HasChanged && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    clientId_WithoutPrefix = cci.ClientId_WithoutPrefix;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId = cci.ClientId;
                    data = cci.NewValue;
                    isSetting = true;
                    clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                    //
                    CciEnum elt = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //
                    switch (elt)
                    {
                        case CciEnum.currency:
                            underlyingAsset.Currency.Value = data;
                            underlyingAsset.CurrencySpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;

                        case CciEnum.exch:
                            underlyingAsset.ExchangeIdSpecified = StrFunc.IsFilled(data);
                            if (underlyingAsset.ExchangeIdSpecified)
                            {
                                SQL_Market sqlMarket = new SQL_Market(cciTrade.CS, SQL_TableWithID.IDType.FIXML_SecurityExchange, data, SQL_Table.ScanDataDtEnabledEnum.No);
                                if (sqlMarket.LoadTable())
                                {
                                    underlyingAsset.ExchangeId = underlyingAsset.CreateExchangeId(sqlMarket.FIXML_SecurityExchange);
                                    underlyingAsset.ExchangeId.OTCmlId = sqlMarket.Id;
                                    underlyingAsset.ExchangeId.Value = data;
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer ...
                            break;

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(clientId_WithoutPrefix, processQueue);
                }
            }
            //
            for (int i = 0; i < InstrumentIdLenght; i++)
                cciSecurityIdSourceScheme[i].Dump_ToDocument();

        }
        #endregion

        

        #region Method

        #region InitializeInstrumentId_FromCci
        private void InitializeInstrumentId_FromCci()
        {

            bool isOk = true;
            int index = -1;
            
            ArrayList lst = new ArrayList();
            lst.Clear();
            while (isOk)
            {
                index += 1;
                
                CciSecurityIdSourceScheme cciSchemeCurrent = new CciSecurityIdSourceScheme(cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_instrumentId, index + 1, null);
                
                isOk = CcisBase.Contains(cciSchemeCurrent.CciClientId(CciSecurityIdSourceScheme.CciEnum.scheme));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(underlyingAsset.InstrumentId) || (index == underlyingAsset.InstrumentId.Length))
                        ReflectionTools.AddItemInArray(underlyingAsset, "instrumentId", index);

                    cciSchemeCurrent.scheme = underlyingAsset.InstrumentId[index];

                    lst.Add(cciSchemeCurrent);
                }
            }
            cciSecurityIdSourceScheme = (CciSecurityIdSourceScheme[])lst.ToArray(typeof(CciSecurityIdSourceScheme));

            for (int i = 0; i < InstrumentIdLenght; i++)
            {
                //supprimer la valeur par défaut "http://www.euro-finance-systems.fr/otcml/instrumentId"; placé das le constructor de InstrumentId  
                if (cciSecurityIdSourceScheme[i].scheme.Scheme == "http://www.euro-finance-systems.fr/otcml/instrumentId")
                    cciSecurityIdSourceScheme[i].scheme.Scheme = string.Empty;
                //
                cciSecurityIdSourceScheme[i].Initialize_FromCci();
            }
        }
        #endregion InitializeInstrumentId_FromCci
        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, this.GetType().BaseType, "CciEnum", "NewValue", string.Empty);
            for (int i = 0; i < InstrumentIdLenght; i++)
                cciSecurityIdSourceScheme[i].Clear();
        }
        #endregion
        #endregion
    }
    
}

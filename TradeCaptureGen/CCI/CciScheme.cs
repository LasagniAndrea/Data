#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// CciExtends: Contient les informations nécessaires pour les instrument EXTEND.
    /// </summary>
    public abstract class CciSchemeBase : IContainerCciFactory, IContainerCci
    {
        public enum CciEnum
        {
            scheme,
            value
        }
        #region Members
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase cciTrade;
        public IScheme scheme;
        private readonly string prefix;
        private readonly int number;
        #endregion




        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        public virtual string Scheme
        {
            get { return null; }
        }
        #region numberPrefix
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < number)
                    ret = number.ToString();
                return ret;
            }
        }
        #endregion numberPrefix        
        #endregion
        #region constructor
        public CciSchemeBase(CciTradeBase ptrade, string pPrefix, int pSchemeNumber, IScheme pScheme)
        {
            cciTrade = ptrade;
            _ccis = cciTrade.Ccis;
            scheme = pScheme;
            number = pSchemeNumber;
            prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion constructor
        //
        #region AddCciSystem
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();
        }
        #endregion AddCciSystem
        #region private CreateInstance
        private void CreateInstance()
        {
        }
        #endregion
        //
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {

            string data;
            bool isSetting;
            SQL_Table sql_Table;
            //
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    //clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    data = string.Empty;
                    isSetting = true;
                    sql_Table = null;
                    #endregion
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.scheme:
                            #region scheme
                            if (StrFunc.IsFilled(scheme.Scheme))
                                data = GetSchemeData(scheme.Scheme);
                            #endregion scheme
                            break;
                        case CciEnum.value:
                            #region value
                            if (StrFunc.IsFilled(scheme.Value))
                                data = scheme.Value;
                            #endregion value
                            break;
                    }
                    //
                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);
                }
            }

        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

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
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            string clientId;
            string clientId_Key;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId = cci.ClientId;
                    data = cci.NewValue;
                    isSetting = true;
                    clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.scheme:
                            #region scheme
                            scheme.Scheme = string.Empty; 
                            if (StrFunc.IsFilled(data))
                                scheme.Scheme = GetDataScheme(data);
                            #endregion scheme
                            break;
                        case CciEnum.value:
                            #region value
                            scheme.Value = string.Empty; 
                            if (StrFunc.IsFilled(data))  
                                scheme.Value = data;
                            #endregion value
                            break;
                    }
                    //
                    if (isSetting)
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {

        }
        #endregion SetDisplay
        #region SetEnabled
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, this.GetType().BaseType, "CciEnum", "IsEnabled", pIsEnabled);
        }
        #endregion SetEnabled
        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, this.GetType().BaseType, "CciEnum", "NewValue", string.Empty);
        }
        #endregion
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        //
        #region Membres de IContainerCci
        #region CciClientId
        public string CciClientId(CciEnum pClientId_Key)
        {
            return CciClientId(pClientId_Key.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return _ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #endregion
        #region Membres de IContainerCci

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
        #endregion ITradeCci
        //
        #region virtual methods
        public virtual string GetDataScheme(string pData)
        {
            return Scheme + "?V=" + pData;
        }
        public virtual string GetSchemeData(string pScheme)
        {
            string temp = Scheme + "?V=";
            //
            return pScheme.TrimStart(temp.ToCharArray());
        }
        #endregion

    }
    
    /// <summary>
    /// 
    /// </summary>
    public class CciSecurityIdSourceScheme : CciSchemeBase
    {
        #region members 
        #endregion
        #region Accessors
        public override string Scheme
        {
            get { return Cst.OTCmL_SecurityIdSourceScheme; }
        }
        #endregion
        #region constructor
        public CciSecurityIdSourceScheme(CciTradeBase ptrade, string pPrefix, int pSchemeNumber, IScheme pScheme) : base(ptrade, pPrefix, pSchemeNumber, pScheme) { }        
        #endregion constructor
        #region virtual methods
        public override string GetDataScheme(string pData)
        {
            string ret = EnumTools.GenerateScheme(Ccis.CS, Scheme, "SecurityIDSourceEnum", pData, false);
            return ret;
        }

        public override string GetSchemeData(string pScheme)
        {
            string vPrefix = "V=";
            int posV = pScheme.IndexOf(vPrefix);
            string ret;
            if (posV > 0)
            {
                int pos = pScheme.IndexOf("&", posV);
                if (pos > 0)
                    ret = pScheme.Substring(posV + vPrefix.Length, pos - (posV + vPrefix.Length));
                else
                    ret = pScheme.Substring(posV + vPrefix.Length);
            }
            else
            {
                int posS = pScheme.IndexOf("/");
                int lastPos = posS;
                while (posS > 0)
                {
                    lastPos = posS;
                    posS = pScheme.IndexOf("/", posS + 1);
                }
                //
                ret = pScheme.Substring(lastPos + 1);
            }
            //
            return ret;
        }
        #endregion
        #region SetEnabled
        public new void SetEnabled(bool pIsEnabled)
        {
            base.SetEnabled(pIsEnabled);
            //
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
        }
        #endregion SetEnabled
        #region public Clear
        public new void Clear()
        {
            base.Clear();
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
        }
        #endregion
    }

    // EG 20221201 [25639] [WI484] New
    public class CciProductionDeviceScheme : CciSchemeBase
    {
        #region members 
        #endregion
        #region Accessors
        public override string Scheme
        {
            get { return "http://www.efsml.org/coding-scheme/environmental-production-device"; }
        }
        #endregion
        #region constructor
        public CciProductionDeviceScheme(CciTradeBase ptrade, string pPrefix, int pSchemeNumber, IScheme pScheme) : base(ptrade, pPrefix, pSchemeNumber, pScheme) { }
        #endregion constructor
        public override string GetDataScheme(string pData)
        {
            string ret = EnumTools.GenerateScheme(Ccis.CS, Scheme, "ProductionDevice", pData, false);
            return ret;
        }
    }
    
}

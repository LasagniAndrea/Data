#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciExtends
    /// <summary>
    /// CciExtends: Contient les informations nécessaires pour les instrument EXTEND.
    /// </summary>
    public class CciExtends : ContainerCciBase, IContainerCciFactory
    {
        #region Members
        private readonly CciTradeBase cciTrade;
        private readonly ITradeExtends extends;
        #endregion

        #region constructor
        public CciExtends(CciTradeBase ptrade, ITradeExtends pExtends, string pPrefix) : base(pPrefix, ptrade.Ccis)
        {
            cciTrade = ptrade;
            extends = pExtends;
        }
        #endregion constructor

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

        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            //
            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    string scheme = string.Empty;
                    string defaultValue = string.Empty;
                    //
                    string clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    int extendId = GetExtendIdFromClientId(clientId_Key);
                    //
                    SQL_DefineExtendDet sqlDefineExtendDet = new SQL_DefineExtendDet(cciTrade.CSCacheOn, Convert.ToInt32(extendId));
                    sqlDefineExtendDet.LoadTable(new string[] { "IDDEFINEEXTENDDET,SCHEME,DEFAULTVALUE" });
                    if (sqlDefineExtendDet.IsLoaded)
                    {
                        scheme = sqlDefineExtendDet.Scheme;
                        defaultValue = sqlDefineExtendDet.DefaultValue;
                    }
                    //
                    if (null != extends)
                    {
                        ITradeExtend spheresIdScheme = extends.GetSpheresIdFromScheme(extendId);
                        if (null == spheresIdScheme)
                        {
                            // 20090916 EG
                            int index = ArrFunc.Count(extends.TradeExtend);
                            if ((1 == index) && StrFunc.IsEmpty(extends.TradeExtend[0].OtcmlId))
                                index = 0;
                            else
                                ReflectionTools.AddItemInArray(extends, "tradeExtend", index);

                            extends.TradeExtend[index].OTCmlId = extendId;
                            //
                            if (StrFunc.IsFilled(defaultValue))
                                extends.TradeExtend[index].Value = defaultValue;
                            //
                            spheresIdScheme = extends.GetSpheresIdFromScheme(extendId);
                        }
                        //
                        if (StrFunc.IsFilled(scheme))
                            spheresIdScheme.Scheme = scheme;
                    }
                }
            }
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {

            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    string clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    string data = string.Empty;
                    Boolean isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion

                    int extendId = GetExtendIdFromClientId(clientId_Key);

                    if (null != extends)
                    {
                        ITradeExtend spheresIdScheme = extends.GetSpheresIdFromScheme(extendId);
                        if (null != spheresIdScheme)
                            data = spheresIdScheme.Value;
                    }

                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
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
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                CustomCaptureInfo cci = CcisBase[clientId];

                CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                string data = cci.NewValue;
                bool isSetting = true;
                string clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);

                int extendId = GetExtendIdFromClientId(clientId_Element);

                if (null != extends)
                {
                    ITradeExtend spheresIdScheme = extends.GetSpheresIdFromScheme(extendId);
                    if (null != spheresIdScheme)
                    {
                        //spheresIdScheme.OTCmlId = Convert.ToInt32(extendId);
                        spheresIdScheme.Value = data;
                    }
                }

                if (isSetting)
                    CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);

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
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document



        private static int GetExtendIdFromClientId(string pClientId)
        {
            string[] arrClientId = pClientId.Split(CustomObject.KEY_SEPARATOR.ToString().ToCharArray());
            return Convert.ToInt32(arrClientId[0]);
        }
    }
    #endregion
}

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
    #region CciSecurityLeg
    /// <summary>
    /// CciSecurityLeg
    /// </summary>
    public class CciSecurityLeg : ContainerCciBase, IContainerCciFactory,  IContainerCciSpecified, IContainerCciPayerReceiver, IContainerCciGetInfoButton , ICciPresentation
    {
        #region Enum
        public enum CciEnum
        {
            unknown
        }
        #endregion
        #region Members
        
        public ISecurityLeg securityLeg;
        
        
        public CciDebtSecurityTransaction cciDebtSecurityTransaction;
        #endregion
        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        #endregion
        #region constructor
        public CciSecurityLeg(CciTrade pTrade, string pPrefix, int pStreamNumber, ISecurityLeg pSecurityLeg) :
            base(pPrefix, pStreamNumber, pTrade.Ccis)
        {
            securityLeg = pSecurityLeg;
            cciDebtSecurityTransaction = new CciDebtSecurityTransaction(pTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_debtSecurityTransaction, pSecurityLeg.DebtSecurityTransaction);
        }
        #endregion constructor
        //
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();

            string clientId_WithoutPrefix = cciDebtSecurityTransaction.cciGrossAmount.CciClientId(CciPayment.CciEnum.date);
            CciTools.AddCciSystem(CcisBase, Cst.TXT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);

            cciDebtSecurityTransaction.AddCciSystem();

        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
                cciDebtSecurityTransaction.Initialize_FromCci();
            
        }
        
        
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (CciEnum cciEnum in Enum.GetValues(tCciEnum))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    SQL_Table sql_Table = null;
                    bool isSetting;
                    #endregion

                    switch (cciEnum)
                    {
                        default:
                            isSetting = false;
                            break;
                    }
                    //
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            cciDebtSecurityTransaction.Initialize_FromDocument();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            cciDebtSecurityTransaction.ProcessInitialize(pCci);

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            cciDebtSecurityTransaction.ProcessExecute(pCci);

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
                return cciDebtSecurityTransaction.IsClientId_PayerOrReceiver(pCci);
            
        }
        
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        /// FI 20121106 [18224] tuning Spheres ne balaye plus la collection cci mais la liste des enums de CciEnum
        public void Dump_ToDocument()
        {

            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    
                    switch (cciEnum)
                    {
                        default:
                            isSetting = false;
                            break;
                    }
                    //
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            cciDebtSecurityTransaction.Dump_ToDocument();

        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            cciDebtSecurityTransaction.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string pPrefix)
        {
            cciDebtSecurityTransaction.RemoveLastItemInArray(pPrefix);
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            cciDebtSecurityTransaction.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            cciDebtSecurityTransaction.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            cciDebtSecurityTransaction.SetEnabled(pIsEnabled);

        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            cciDebtSecurityTransaction.Initialize_Document();
        }
        #endregion
        
        //
        #region IContainerCciSpecified Membres
        /// <summary>
        /// SecurityLeg specified si le payer du grosdAmount renseigné
        /// </summary>
        public bool IsSpecified
        {
            get { return (cciDebtSecurityTransaction.cciGrossAmount.Cci(CciPayment.CciEnum.payer).IsFilled); }
        }
        #endregion
        //
        #region IContainerCciPayerReceiver Members
        public string CciClientIdPayer
        {
            get { return cciDebtSecurityTransaction.CciClientIdPayer; }
        }
        public string CciClientIdReceiver
        {
            get { return cciDebtSecurityTransaction.CciClientIdReceiver; }
        }
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            //
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
            //
            cciDebtSecurityTransaction.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion IContainerCciPayerReceiver Members
        
        #region Membres de IContainerCciGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            //            
            #region buttons of cciDebtSecurityTransaction
            if (!isOk)
                isOk = cciDebtSecurityTransaction.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion
            //
            return isOk;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {

            cciDebtSecurityTransaction.SetButtonReferential(pCci, pCo);

        }
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #endregion Membres de IContainerCciGetInfoButton

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        private void CreateInstance()
        {
            CciTools.CreateInstance(this, securityLeg, "CciEnum");
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
        }
        
        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20120625 Add DumpSpecific_ToGUI
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            cciDebtSecurityTransaction.DumpSpecific_ToGUI(pPage);
        }

        #endregion
    }
    #endregion
}

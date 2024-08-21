#region Using Directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceRebate. 
    /// </summary>
    public class CciInvoiceRebate : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("capConditionsSpecified")]
            capConditionsSpecified,
            [System.Xml.Serialization.XmlEnumAttribute("bracketConditionsSpecified")]
            bracketConditionsSpecified,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        //private CciTradeAdminBase m_CciTradeAdmin;
        private readonly CciInvoice m_CciInvoice;
        private IRebateConditions m_RebateConditions;
        private readonly string m_Prefix;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        private CciInvoiceRebateCap m_CciInvoiceRebateCap;
        private CciInvoiceRebateBracket m_CciInvoiceRebateBracket;
        #endregion Members
        #region Accessors
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region ExistInvoiceRebateCap
        public bool ExistInvoiceRebateCap
        {
            get { return (null != m_CciInvoiceRebateCap); }
        }
        #endregion ExistInvoiceRebateCap
        #region ExistInvoiceRebateBracket
        public bool ExistInvoiceRebateBracket
        {
            get { return (null != m_CciInvoiceRebateBracket); }
        }
        #endregion ExistInvoiceRebateBracket
        #region Invoice
        public IInvoice Invoice
        {
            get { return m_CciInvoice.Invoice; }
        }
        #endregion Invoice
        #region RebateConditions
        public IRebateConditions RebateConditions
        {
            get { return m_RebateConditions; }
            set { m_RebateConditions = value; }
        }
        #endregion RebateConditions
        #endregion Accessors
        #region Constructors
        public CciInvoiceRebate(CciInvoice pInvoice, string pPrefix, IRebateConditions pRebateConditions)
        {
            //m_CciTradeAdmin = pTradeAdmin;
            m_CciInvoice = pInvoice;
            m_Ccis = pInvoice.Ccis;
            m_Prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            m_RebateConditions = pRebateConditions;
        }
        #endregion Constructors
        #region Interface
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.CHK + CciClientId(CciEnum.capConditionsSpecified), false, TypeData.TypeDataEnum.@bool);
            CciTools.AddCciSystem(Ccis, Cst.CHK + CciClientId(CciEnum.bracketConditionsSpecified), false, TypeData.TypeDataEnum.@bool);

            if (null != m_CciInvoiceRebateCap)
                m_CciInvoiceRebateCap.AddCciSystem();

            if (null != m_CciInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
            if (ExistInvoiceRebateCap)
                m_CciInvoiceRebateCap.CleanUp();
            if (ExistInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.CleanUp();
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            if (ExistInvoiceRebateCap)
                m_CciInvoiceRebateCap.Dump_ToDocument();
            if (ExistInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_RebateConditions);
            InitializeInvoiceRebateCapConditions_FromCci();
            InitializeInvoiceRebateBracketConditions_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            if (this.ExistInvoiceRebateCap)
                m_CciInvoiceRebateCap.Initialize_FromDocument();
            if (this.ExistInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.Initialize_FromDocument();
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
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
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            /*
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (elt)
                {
                    default:
                        #region Default
                        #endregion Default
                        break;
                }
            }
            */
            if (this.ExistInvoiceRebateCap)
                m_CciInvoiceRebateCap.ProcessInitialize(pCci);
            if (this.ExistInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.ProcessInitialize(pCci);
            //
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            if (null != m_CciInvoiceRebateCap)
                m_CciInvoiceRebateCap.RefreshCciEnabled();
            if (null != m_CciInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string pPrefix)
        {
            if (ExistInvoiceRebateCap)
                m_CciInvoiceRebateCap.RemoveLastItemInArray(pPrefix);
            //
            if (ExistInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.RemoveLastItemInArray(pPrefix);
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (ExistInvoiceRebateCap)
                m_CciInvoiceRebateCap.SetDisplay(pCci);
            if (ExistInvoiceRebateBracket)
                m_CciInvoiceRebateBracket.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(m_Prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(m_Prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #endregion Interface

        #region Methods
        #region CalculRebateAmounts
        public void CalculRebateAmounts()
        {
            if (m_RebateConditions.CapConditionsSpecified)
                m_CciInvoiceRebateCap.CalculNetTurnOverExcessAmount();
            if (m_RebateConditions.BracketConditionsSpecified)
                m_CciInvoiceRebateBracket.CalculRebateBracketAmount();
        }
        #endregion CalculRebateAmounts
        #region SetRebateAmounts
        public void SetRebateAmounts()
        {
            EFS_Decimal totalRebateAmount = new EFS_Decimal(0);
            if (m_RebateConditions.CapConditionsSpecified &&
                m_RebateConditions.CapConditions.Result.NetTurnOverInExcessAmountSpecified)
                totalRebateAmount.DecValue += m_RebateConditions.CapConditions.Result.NetTurnOverInExcessAmount.Amount.DecValue;
            if (m_RebateConditions.BracketConditionsSpecified &&
                m_RebateConditions.BracketConditions.Result.TotalRebateBracketAmountSpecified)
                totalRebateAmount.DecValue += m_RebateConditions.BracketConditions.Result.TotalRebateBracketAmount.Amount.DecValue;
            Ccis.SetNewValue(m_CciInvoice.CciClientId(CciInvoice.CciEnum.totalRebateAmount_amount), totalRebateAmount.Value);
        }
        #endregion SetRebateAmounts

        #region InitializeInvoiceRebateBracketConditions_FromCci
        private void InitializeInvoiceRebateBracketConditions_FromCci()
        {
            m_CciInvoiceRebateBracket = new CciInvoiceRebateBracket(this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceRebateBracketConditions, null);
            bool isOk = Ccis.Contains(m_CciInvoiceRebateBracket.CciClientId(CciInvoiceRebateBracket.CciEnum.applicationPeriod_period));
            if (isOk)
            {
                if (null == m_RebateConditions.BracketConditions)
                    m_RebateConditions.CreateBracketConditions();

                m_CciInvoiceRebateBracket.RebateBracketConditions = m_RebateConditions.BracketConditions;
                m_CciInvoiceRebateBracket.Initialize_FromCci();
            }
        }
        #endregion InitializeInvoiceRebateBracketConditions_FromCci
        #region InitializeInvoiceRebateCapConditions_FromCci
        private void InitializeInvoiceRebateCapConditions_FromCci()
        {
            m_CciInvoiceRebateCap = new CciInvoiceRebateCap(this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceRebateCapConditions, null);
            bool isOk = Ccis.Contains(m_CciInvoiceRebateCap.CciClientId(CciInvoiceRebateCap.CciEnum.applicationPeriod_period));
            if (isOk)
            {
                if ((null != m_RebateConditions) && (null == m_RebateConditions.CapConditions))
                    m_RebateConditions.CreateCapConditions();
                m_CciInvoiceRebateCap.RebateCapConditions = m_RebateConditions.CapConditions;
                m_CciInvoiceRebateCap.Initialize_FromCci();
            }
        }
        #endregion InitializeInvoiceRebateCapConditions_FromCci
        #endregion Methods
    }
}
#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML;
using EfsML.Business;
// EG 20150624 [21151]
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Classe abstraite pour gestion des actions sur position (Transfert, Correction , dénouement sur Trade fongible)
    /// </summary>
    /// EG 210150716 [21103] Rename CciTradeETDActionBase en CciTradeActionBase
    public abstract class CciTradeActionBase : ContainerCciBase, IContainerCciFactory
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        // EG 20150716 [21103] Add isSafekeepingRestitution
        public enum CciEnum
        {
            tradeIdentification_identifier,
            tradeIdentification_displayName,
            tradeIdentification_description,
            date,
            quantity,
            initialQuantity,
            availableQuantity,
            isFeeRestitution,
            // EG 20150716 [21103]
            isSafekeepingRestitution,
            note,
            unknown,
        }
        #endregion Enums

        #region Members
        protected CciTrade _cciTrade;
        protected TradeActionBase _actionBase;
        #endregion Members

        #region accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        #endregion

        #region Constructors
        /// EG 210150716 [21103] Rename CciTradeETDActionBase en CciTradeActionBase
        public CciTradeActionBase(CciTrade pTrade, TradeActionBase pActionbase, string pPrefix) :
            base(pPrefix, pTrade.Ccis)
        {
            _cciTrade = pTrade;
            _actionBase = pActionbase;
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCciSystem()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CleanUp()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20150716 [21103] Add CciEnum.isSafekeepingRestitution
        public virtual void Dump_ToDocument()
        {

            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.date:
                            _actionBase.date.Value = data;
                            break;
                        case CciEnum.quantity:
                            cci.ErrorMsg = string.Empty;
                            _actionBase.quantity.Value = data;
                            if (_actionBase.MaxQuantity == TradeActionBase.MaxQuantityEnum.AvailableQuantity)
                            {
                                if (_actionBase.quantity.DecValue > _actionBase.availableQuantity.DecValue)
                                    cci.ErrorMsg = Ressource.GetString("Failure_NotEnoughAvailableQuantity");
                            }
                            else if (_actionBase.MaxQuantity == TradeActionBase.MaxQuantityEnum.InitialQuantity)
                            {
                                if (_actionBase.quantity.DecValue > _actionBase.initialQuantity.DecValue)
                                    cci.ErrorMsg = Ressource.GetString("Failure_NotEnoughInitialQuantity");
                            }
                            else
                            {
                                throw new Exception(StrFunc.AppendFormat("{0} is not implemented", _actionBase.MaxQuantity.ToString()));
                            }
                            break;
                        case CciEnum.isFeeRestitution:
                            _actionBase.isFeeRestitution.BoolValue = BoolFunc.IsTrue(data);
                            break;
                        case CciEnum.isSafekeepingRestitution:
                            _actionBase.isReversalSafekeeping = new EFS_Boolean
                            {
                                BoolValue = BoolFunc.IsTrue(data)
                            };
                            break;
                        case CciEnum.note:
                            _actionBase.note = data;
                            _actionBase.noteSpecified = StrFunc.IsFilled(data);
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize_Document()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _actionBase);
        }

        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// EG 20150716 [21103] Add CciEnum.isSafekeepingRestitution
        public virtual void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    //display = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.tradeIdentification_identifier:
                            data = _actionBase.tradeIdentification.Identifier;
                            break;
                        case CciEnum.tradeIdentification_displayName:
                            data = _actionBase.tradeIdentification.Displayname;
                            break;
                        case CciEnum.tradeIdentification_description:
                            data = _actionBase.tradeIdentification.Description;
                            break;
                        case CciEnum.date:
                            data = _actionBase.date.Value;
                            break;
                        case CciEnum.quantity:
                            data = _actionBase.quantity.Value;
                            break;
                        case CciEnum.availableQuantity:
                            data = _actionBase.availableQuantity.Value;
                            break;
                        case CciEnum.initialQuantity:
                            data = _actionBase.initialQuantity.Value;
                            break;
                        case CciEnum.isFeeRestitution:
                            data = _actionBase.isFeeRestitution.Value;
                            break;
                        case CciEnum.isSafekeepingRestitution:
                            if (null != _actionBase.isReversalSafekeeping)
                                data = _actionBase.isReversalSafekeeping.Value;
                            break;
                        case CciEnum.note:
                            data = _actionBase.note;
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessExecute(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void RefreshCciEnabled()
        {
            CustomCaptureInfo cci = Cci(CciEnum.availableQuantity);
            if (null != cci)
                cci.IsEnabled = false;

            cci = Cci(CciEnum.initialQuantity);
            if (null != cci)
                cci.IsEnabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public virtual void RemoveLastItemInArray(string pPrefix)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {
        }

        #endregion IContainerCciFactory Members
        #endregion Interfaces
    }

    /// <summary>
    /// Description résumée de CciTradeCorrectionOfQuantity.
    /// </summary>
    /// EG 210150716 [21103] Rename héritage CciTradeETDActionBase en CciTradeActionBase
    public class CciPositionCancelation : CciTradeActionBase, ICciPresentation
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        #endregion Members

        #region constructor
        public CciPositionCancelation(CciTrade pTrade, TradePositionCancelation pPositionCancelation, string pPrefix) :
            base(pTrade, pPositionCancelation, pPrefix)
        {
        }
        #endregion Members

        #region ICciPresentation Membres
        // EG 210150716 [21103] Add isSafeKeepingRestitution
        // EG 20220503 [XXXXX] Changement de la nature du control (new GUI)
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            // EG 210150716 [21103] 
            if (pPage.FindControl(Cst.BUT + CciClientId(CciTradeActionBase.CciEnum.isFeeRestitution)) is WCToolTipLinkButton image)
                image.Pty.TooltipContent = Ressource.GetString2("correctionOfQuantity_isFeeRestitution_ToolTip", DtFunc.DateTimeToString(_actionBase.date.DateValue, DtFunc.FmtShortDate));
            // EG 210150716 [21103] 
            image = pPage.FindControl(Cst.BUT + CciClientId(CciTradeActionBase.CciEnum.isSafekeepingRestitution)) as WCToolTipLinkButton;
            if (null != image)
                image.Pty.TooltipContent = Ressource.GetString2("correctionOfQuantity_isSafekeepingRestitution_ToolTip", DtFunc.DateTimeToString(_actionBase.date.DateValue, DtFunc.FmtShortDate));
        }
        #endregion
    }

    /// <summary>
    /// Description résumée de CciTradePositionTransfer.
    /// </summary>
    /// EG 210150716 Rename héritage CciTradeETDActionBase en CciTradeActionBase
    public class CciPositionTransfer : CciTradeActionBase, ICciPresentation
    {
        /// <summary>
        /// 
        /// </summary>
        public new enum CciEnum
        {
            tradeIdentification_identifier,
            tradeIdentification_displayName,
            tradeIdentification_description,
            date,
            initialQuantity,
            availableQuantity,
            quantity,
            note,
            unknown,
        }

        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly TradePositionTransfer _positionTransfer;
        #endregion Members

        #region Constructors
        public CciPositionTransfer(CciTrade pTrade, TradePositionTransfer pPositionTransfer, string pPrefix)
            : base(pTrade, pPositionTransfer, pPrefix)
        {
            _positionTransfer = pPositionTransfer;
        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.initialQuantity:
                            break;
                        case CciEnum.availableQuantity:
                            break;
                        case CciEnum.quantity:
                            // EG 20170127 Qty Long To Decimal
                            _positionTransfer.quantity = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            base.Dump_ToDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _positionTransfer);

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.initialQuantity:
                            data = _positionTransfer.initialQuantity.Value;
                            break;
                        case CciEnum.availableQuantity:
                            data = _positionTransfer.availableQuantity.Value;
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            base.Initialize_FromDocument();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20150624 [21151] Use CciProductExchangeTradedBase instead CciProductExchangeTradedDerivative (PositionTransfer)
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);

                switch (key)
                {
                    case CciEnum.quantity:
                        // EG 20150624 [21151] Use CciProductExchangeTradedBase
                        if (_cciTrade.cciProduct is CciProductExchangeTradedBase)
                        {
                            CciProductExchangeTradedBase _cciProduct = _cciTrade.cciProduct as CciProductExchangeTradedBase;
                            CciFixTradeCaptureReport cciFixTradeCaptureReport = _cciProduct.CciFixTradeCaptureReport;
                            CcisBase.SetNewValue(cciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.LastQty), _positionTransfer.quantity.Value);
                        }
                        else if (_cciTrade.cciProduct is CciProductDebtSecurityTransaction)
                        {
                            CciProductDebtSecurityTransaction _cciProduct = _cciTrade.cciProduct as CciProductDebtSecurityTransaction;
                            CciOrderQuantity cciQuantity = _cciProduct.CciQuantity;
                            IMoney nominal = null;
                            ISecurityAsset securityAsset = cciQuantity.DebtSecurityTransaction.GetSecurityAssetInDataDocument();
                            if (null != securityAsset)
                                nominal = new SecurityAssetContainer(securityAsset).GetNominal(_cciTrade.DataDocument.CurrentProduct.ProductBase);

                            CcisBase.SetNewValue(cciQuantity.CciClientId(CciOrderQuantity.CciEnum.numberOfUnits),
                                _positionTransfer.quantity.Value);
                            CcisBase.SetNewValue(cciQuantity.CciClientId(CciOrderQuantity.CciEnum.notional_amount),
                                StrFunc.FmtDecimalToInvariantCulture(_positionTransfer.quantity.DecValue * nominal.Amount.DecValue));

                            // EG 20150907 [21317]
                            decimal prorata = _positionTransfer.quantity.DecValue / _positionTransfer.initialQuantity.DecValue;
                            decimal grossAmount = _positionTransfer.InitialGrossAmount.Amount.DecValue * prorata;
                            decimal initialAccruedInterestAmount = _positionTransfer.InitialAccruedInterestAmount.Amount.DecValue * prorata;

                            CcisBase.SetNewValue(_cciProduct.CciGrossAmount.CciClientId(CciPayment.CciEnum.amount), StrFunc.FmtDecimalToInvariantCulture(grossAmount));
                            if (null != _positionTransfer.InitialAccruedInterestAmount)
                                CcisBase.SetNewValue(_cciProduct.CciPrice.CciClientId(CciOrderPrice.CciEnum.accruedInterestAmount_amount),
                                    StrFunc.FmtDecimalToInvariantCulture(initialAccruedInterestAmount));

                            if (cciQuantity.OrderQuantity.QuantityType == EfsML.Enum.OrderQuantityType3CodeEnum.CASH)
                                CcisBase.SetNewValue(cciQuantity.CciClientId(CciOrderQuantity.CciEnum.quantityAmount),
                                    CcisBase[cciQuantity.CciClientId(CciOrderQuantity.CciEnum.notional_amount)].NewValue);
                            else if (cciQuantity.OrderQuantity.QuantityType == EfsML.Enum.OrderQuantityType3CodeEnum.UNIT)
                                CcisBase.SetNewValue(cciQuantity.CciClientId(CciOrderQuantity.CciEnum.quantityAmount),
                                    CcisBase[cciQuantity.CciClientId(CciOrderQuantity.CciEnum.numberOfUnits)].NewValue);
                        }
                        else if (_cciTrade.cciProduct is CciProductReturnSwap)
                        {
                            CciProductReturnSwap _cciProduct = _cciTrade.cciProduct as CciProductReturnSwap;
                            if (0 < _cciProduct.ReturnLegLength)
                            {
                                CcisBase.SetNewValue(_cciProduct.CciReturnSwapReturnLeg[0].CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_openUnits), _positionTransfer.quantity.Value);
                            }
                        }
                        break;
                }
            }
            base.ProcessInitialize(pCci);
        }
        #endregion IContainerCciFactory Members

        #region ICciPresentation Membres
        // EG 210150716 [21103] Add isSafeKeepingRestitution
        // EG 20220503 [XXXXX] Changement de la nature du control (new GUI)
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            // EG 210150716 [21103] 
            if (pPage.FindControl(Cst.BUT + CciClientId(CciTradeActionBase.CciEnum.isFeeRestitution)) is WCToolTipLinkButton image)
                image.Pty.TooltipContent = Ressource.GetString2("positionTransfer_isFeeRestitution_ToolTip", DtFunc.DateTimeToString(_actionBase.date.DateValue, DtFunc.FmtShortDate));

            image = pPage.FindControl(Cst.BUT + CciClientId(CciTradeActionBase.CciEnum.isSafekeepingRestitution)) as WCToolTipLinkButton;
            if (null != image)
                image.Pty.TooltipContent = Ressource.GetString2("positionTransfer_isSafekeepingRestitution_ToolTip", DtFunc.DateTimeToString(_actionBase.date.DateValue, DtFunc.FmtShortDate));
        }
        #endregion
    }

    /// <summary>
    /// Description résumée de CciRemoveAllocation.
    /// </summary>
    /// EG 210150716 Rename héritage CciTradeETDActionBase en CciTradeActionBase
    public class CciRemoveAllocation : CciTradeActionBase, ICciPresentation
    {
        /// <summary>
        /// 
        /// </summary>
        public new enum CciEnum
        {
            tradeIdentification_identifier,
            tradeIdentification_displayName,
            tradeIdentification_description,
            date,
            initialQuantity,
            availableQuantity,
            unclearingmessage,
            note,
            unknown,
        }

        #region Members
        /// <summary>
        /// 
        /// </summary>
        private readonly TradeRemoveAllocation _removeAllocation;
        #endregion Members

        #region Constructors
        public CciRemoveAllocation(CciTrade pTrade, TradeRemoveAllocation pRemoveAllocation, string pPrefix)
            : base(pTrade, pRemoveAllocation, pPrefix)
        {
            _removeAllocation = pRemoveAllocation;
        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.initialQuantity:
                        case CciEnum.availableQuantity:
                        case CciEnum.unclearingmessage:
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            base.Dump_ToDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _removeAllocation);

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = CcisBase[Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.initialQuantity:
                            data = _removeAllocation.initialQuantity.Value;
                            break;
                        case CciEnum.availableQuantity:
                            data = _removeAllocation.availableQuantity.Value;
                            break;
                        case CciEnum.unclearingmessage:
                            if (_removeAllocation.availableQuantity.Value != _removeAllocation.initialQuantity.Value)
                                data = Ressource.GetString("removeAllocation_unclearing_ToolTip");
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            base.Initialize_FromDocument();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);
        }
        #endregion IContainerCciFactory Members


        #region ICciPresentation Membres
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
        }
        #endregion
    }

    /// <summary>
    /// Description résumée de CciActionOption.
    /// </summary>
    // EG 20151102 [21465] New
    public class CciDenOption : CciTradeActionBase, ICciPresentation
    {
        /// <summary>
        /// 
        /// </summary>

        #region Members
        private readonly TradeDenOption _tradeDenOption;
        private CciDenOptionAction[] _cciDenOptionAction;
        private CciPayment[] _cciOtherPartyPayment;
        #endregion Members

        #region Accessors
        public CciPayment[] CciOtherPartyPayment
        {
            get { return _cciOtherPartyPayment; }
        }

        public int OtherPartyPaymentLength
        {
            get { return ArrFunc.IsFilled(_cciOtherPartyPayment) ? _cciOtherPartyPayment.Length : 0; }
        }
        public int DenOptionActionLength
        {
            get { return ArrFunc.IsFilled(_cciDenOptionAction) ? _cciDenOptionAction.Length : 0; }
        }
        public int DenOptionActionRequestTypeLength
        {
            get
            {
                int length = 0;
                if (ArrFunc.IsFilled(_cciDenOptionAction))
                {
                    _tradeDenOption.action.ToList().ForEach(item =>
                    {
                        if (item.denPosRequestType == _tradeDenOption.posRequestType)
                            length++;
                    });
                }
                return length;
            }
        }
        /// <summary>
        /// Get the reference at the parent product
        /// </summary>
        public CciProductExchangeTradedDerivative CciProduct
        {
            get { return _cciTrade.cciProduct as CciProductExchangeTradedDerivative; }
        }

        /// <summary>
        /// Get the current consultation mode  
        /// </summary>
        public Cst.Capture.ModeEnum CaptureMode
        {
            get
            {
                return CcisBase.CaptureMode;
            }
        }
        #endregion Accessors

        #region Constructors
        public CciDenOption(CciTrade pTrade, TradeDenOption pTradeDenOption, string pPrefix)
            : base(pTrade, pTradeDenOption, pPrefix)
        {
            _tradeDenOption = pTradeDenOption;
        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {
            // Simple values update
            Dump_ToDocument(_tradeDenOption, typeof(TradeDenOption));
            Dump_ToDocument(_tradeDenOption.asset, typeof(TradeDenOptionAsset));
            Dump_ToDocument(_tradeDenOption.asset.underlyer, typeof(TradeDenOptionUnderlyer));

            if (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.remove)
            {
                for (int i = 0; i < ArrFunc.Count(CciOtherPartyPayment); i++)
                    CciOtherPartyPayment[i].Cci(CciPayment.CciEnum.payer).Reset();
            }
            for (int i = 0; i < OtherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].Dump_ToDocument();
            _tradeDenOption.otherPartyPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_tradeDenOption.otherPartyPaymentSpecified, _cciOtherPartyPayment);

            if (0 < DenOptionActionLength)
            {
                foreach (CciDenOptionAction cciAction in _cciDenOptionAction)
                    cciAction.Dump_ToDocument();
            }

            string quote = CciTools.GetFieldVariableName(new { _tradeDenOption.asset.underlyer.quoteValue }, typeof(TradeDenOptionUnderlyer));
            CustomCaptureInfo cciQuote = this.Cci(quote);
            if (null != cciQuote)
                cciQuote.ErrorMsg = string.Empty;

            string date = CciTools.GetFieldVariableName(new { _tradeDenOption.date }, typeof(TradeDenOption));
            CustomCaptureInfo cciDate = this.Cci(date);

            cciDate.ErrorMsg = string.Empty;
            if (false == _tradeDenOption.IsActionCanBePerformed)
                cciDate.ErrorMsg = Ressource.GetString("denOption_ERRActionCantBePerformedAtBusinessDate");

            string quantity = CciTools.GetFieldVariableName(new { _tradeDenOption.quantity }, typeof(TradeDenOption));
            CustomCaptureInfo cciQuantity = this.Cci(quantity);
            if (null != cciQuantity)
                cciQuantity.ErrorMsg = string.Empty;

            switch (_tradeDenOption.denOptionActionType)
            {
                case Cst.DenOptionActionType.remove:
                    string actionType = CciTools.GetFieldVariableName(new { _tradeDenOption.denOptionActionType }, typeof(TradeDenOption));
                    CustomCaptureInfo cciActionType = this.Cci(actionType);
                    if (null != cciActionType)
                    {
                        cciActionType.ErrorMsg = string.Empty;
                        if (0 == DenOptionActionRequestTypeLength)
                            cciActionType.ErrorMsg = Ressource.GetString("denOption_ERRRemoveActionCantBePerformed");
                        else if (false == _tradeDenOption.action.ToList().Exists(item => item.denIsRemove.BoolValue == true))
                            cciActionType.ErrorMsg = Ressource.GetString("denOption_ERRRemoveActionSelectLine");
                    }
                    break;
                case Cst.DenOptionActionType.@new:
                case Cst.DenOptionActionType.newRemaining:
                    #region Contrôle quantité saisie
                    if (null != cciQuantity)
                    {
                        // EG 20170127 Qty Long To Decimal
                        if ((_tradeDenOption.quantity.DecValue > _tradeDenOption.availableQuantity.DecValue) || (0 == _tradeDenOption.quantity.DecValue))
                            cciQuantity.ErrorMsg = Ressource.GetString("denOption_ERRNotEnoughQuantity");
                    }
                    #endregion Contrôle quantité saisie
                    break;
            }

            base.Dump_ToDocument();
        }

        private void Dump_ToDocument(Object pObject, Type pType)
        {
            string[] cciKeysWithoutPrefix = CciTools.GetCciKeys(pType);

            if (!ArrFunc.IsEmpty(cciKeysWithoutPrefix))
            {
                foreach (string cciKeyWithoutPrefix in cciKeysWithoutPrefix)
                {
                    CustomCaptureInfo cci = this.Cci(cciKeyWithoutPrefix);

                    if (cci != null && cci.HasChanged)
                    {
                        CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        bool set;
                        switch (cciKeyWithoutPrefix)
                        {
                            case "denOptionActionType":
                                _tradeDenOption.denOptionActionType = (Cst.DenOptionActionType)ReflectionTools.EnumParse(new Cst.DenOptionActionType(), cci.NewValue);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                set = true;
                                break;
                            case "denRequestMode":
                                _tradeDenOption.PosRequestMode = (SettlSessIDEnum)ReflectionTools.EnumParse(new SettlSessIDEnum(), cci.NewValue);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                set = true;
                                break;
                            default:
                                set = CciTools.SetStringValue(cciKeyWithoutPrefix, pObject, pType, cci.NewValue);
                                break;
                        }
                        if (set)
                            CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, this.GetType(), this, typeof(TradeDenOption).Name);

            InitializeDenAction_FromCci();
            InitializeOtherPartyPayment_FromCci();
        }

        public override void AddCciSystem()
        {
            for (int i = 0; i < DenOptionActionLength; i++)
                _cciDenOptionAction[i].AddCciSystem();

            for (int i = 0; i < OtherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            Initialize_FromDocument(_tradeDenOption, typeof(TradeDenOption));
            Initialize_FromDocument(_tradeDenOption.asset, typeof(TradeDenOptionAsset));
            Initialize_FromDocument(_tradeDenOption.asset.underlyer, typeof(TradeDenOptionUnderlyer));

            string identifier = CciTools.GetFieldVariableName(new { _tradeDenOption.asset.underlyer.identifier }, typeof(TradeDenOptionUnderlyer));
            if (null != Cci(identifier))
                Cci(identifier).Sql_Table = _tradeDenOption.asset.underlyer.sqlAsset;

            if ((_tradeDenOption.asset.underlyer.underlyingAsset == Cst.UnderlyingAsset.Future) ||
                (_tradeDenOption.asset.underlyer.underlyingAsset == Cst.UnderlyingAsset.ExchangeTradedContract))
            {
                string underlyerCategory = CciTools.GetFieldVariableName(new { _tradeDenOption.asset.underlyer.assetCategory }, typeof(TradeDenOptionUnderlyer));
                if (null != Cci(underlyerCategory))
                    Cci(underlyerCategory).Sql_Table = _tradeDenOption.asset.underlyer.sqlDerivativeContract;
            }

            for (int i = 0; i < OtherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].Initialize_FromDocument();

            for (int i = 0; i < DenOptionActionLength; i++)
                _cciDenOptionAction[i].Initialize_FromDocument();

            base.Initialize_FromDocument();
        }


        private void Initialize_FromDocument(Object pObject, Type pType)
        {
            // get all the Cci keys
            string[] cciKeysWithoutPrefix = CciTools.GetCciKeys(pType);

            if (!ArrFunc.IsEmpty(cciKeysWithoutPrefix))
            {
                foreach (string cciKeyWithoutPrefix in cciKeysWithoutPrefix)
                {
                    CustomCaptureInfo cci = this.Cci(cciKeyWithoutPrefix);
                    if (cci != null)
                    {
                        string data;
                        switch (cciKeyWithoutPrefix)
                        {
                            case "denOptionActionType":
                                data = _tradeDenOption.denOptionActionType.ToString();
                                break;
                            default:
                                data = CciTools.GetStringValue(cciKeyWithoutPrefix, pObject, pType);
                                break;
                        }
                        CcisBase.InitializeCci(cci, null, data);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                switch (cliendid_Key)
                {
                    case "denOptionActionType":
                        _tradeDenOption.InitQuantities(_cciTrade.CS, null, _tradeDenOption.idT, _tradeDenOption.denOptionActionType);
                        InitializeDenAction_FromCci();
                        break;
                }
            }

            //denOptionActionLength
            for (int i = 0; i < DenOptionActionLength; i++)
            {
                _cciDenOptionAction[i].ProcessInitialize(pCci);
            }

            //otherPartyPayment
            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                _cciOtherPartyPayment[i].ProcessInitialize(pCci);

                //Si cliend == OtherPartypayment => proposititon de receiver ( fonction des parties + brokers)
                if (_cciOtherPartyPayment[i].IsCci(CciPayment.CciEnumPayment.payer, pCci))
                {
                    if (_cciOtherPartyPayment[i].IsSpecified)
                    {
                        _cciTrade.SetClientIdDefaultReceiverToOtherPartyPayment(_cciOtherPartyPayment);
                        _cciOtherPartyPayment[i].PaymentInitialize();
                    }
                }
            }
        }

        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            //otherPartyPayment
            for (int i = 0; i < OtherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].ProcessExecute(pCci);
        }

        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < OtherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].ProcessExecuteAfterSynchronize(pCci);
        }

        /// <summary>
        /// the Cci does not have any contraints to be feed up
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            if (false == isOk)
            {
                for (int i = 0; i < OtherPartyPaymentLength; i++)
                {
                    isOk = _cciOtherPartyPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {

            string quantity = CciTools.GetFieldVariableName(new { _tradeDenOption.quantity }, typeof(TradeDenOption));
            string note = CciTools.GetFieldVariableName(new { _tradeDenOption.note }, typeof(TradeDenOption));
            string abandonRemaining = CciTools.GetFieldVariableName(new { _tradeDenOption.abandonRemaining }, typeof(TradeDenOption));

            CustomCaptureInfo cciQuantity = Cci(quantity);
            bool isAbandonRemainingEnabled = false;
            // EG 20170127 Qty Long To Decimal
            decimal actualQuantity = 0;
            bool isEnabled = _tradeDenOption.IsActionCanBePerformed && Decimal.TryParse(cciQuantity.NewValue, out actualQuantity);
            switch (_tradeDenOption.denOptionActionType)
            {
                case Cst.DenOptionActionType.@new:
                    // EG 20170127 Qty Long To Decimal
                    isEnabled &= (0 < actualQuantity) && (actualQuantity <= _tradeDenOption.availableQuantity.DecValue);
                    isAbandonRemainingEnabled = (0 < actualQuantity) && (actualQuantity < _tradeDenOption.availableQuantity.DecValue) &&
                        (_tradeDenOption.PosRequestMode == SettlSessIDEnum.Intraday);
                    break;
                case Cst.DenOptionActionType.newRemaining:
                    // EG 20170127 Qty Long To Decimal
                    isEnabled &= (0 < actualQuantity) && (actualQuantity == _tradeDenOption.positionQuantity.DecValue);
                    break;
                case Cst.DenOptionActionType.remove:
                    isEnabled = _tradeDenOption.IsActionCanBePerformed && (0 < DenOptionActionRequestTypeLength);
                    break;
            }

            string requestMode = CciTools.GetFieldVariableName(new { _tradeDenOption.requestMode }, typeof(TradeDenOption));
            CcisBase.Set(CciClientId(requestMode), "IsEnabled", _tradeDenOption.prevRequestMode != SettlSessIDEnum.Intraday);
            CcisBase.Set(CciClientId(abandonRemaining), "IsEnabled", isAbandonRemainingEnabled);

            if (false == cciQuantity.HasError)
                CcisBase.Set(CciClientId(note), "IsEnabled", isEnabled);
        }
        #endregion IContainerCciFactory Members


        #region ICciPresentation Membres

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {

            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                _cciOtherPartyPayment[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(_tradeDenOption.otherPartyPayment))
            {
                for (int i = _tradeDenOption.otherPartyPayment.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_tradeDenOption.otherPartyPayment[i].payerPartyReference))
                        ReflectionTools.RemoveItemInArray(_tradeDenOption, "otherPartyPayment", i);
                }
            }
            _tradeDenOption.otherPartyPaymentSpecified = ArrFunc.IsFilled(_tradeDenOption.otherPartyPayment);
        }

        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            SetDenOptionAction(pPage);
            SetMoneyPositionForColor(pPage);

            for (int i = 0; i < ArrFunc.Count(_cciOtherPartyPayment); i++)
            {
                if (CciOtherPartyPayment[i].IsSpecified)
                    CciOtherPartyPayment[i].DumpSpecific_ToGUI(pPage);
            }
            AddUnderlyerLink(pPage);

            // FI 20200820 [25468] Call DumpSpecific_ToGUI
            for (int i = 0; i < ArrFunc.Count(_cciDenOptionAction); i++)
                _cciDenOptionAction[i].DumpSpecific_ToGUI(pPage);
        }
        #endregion

        

        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < OtherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        private void AddUnderlyerLink(CciPageBase pPage)
        {

            //FI 20130307 add links on underlyer
            string underlyerCategory = CciTools.GetFieldVariableName(new { _tradeDenOption.asset.underlyer.assetCategory }, typeof(TradeDenOptionUnderlyer));
            string identifier = CciTools.GetFieldVariableName(new { _tradeDenOption.asset.underlyer.identifier }, typeof(TradeDenOptionUnderlyer));

            //Il restera à prévoir les autres type de ssjacent
            switch (_tradeDenOption.asset.underlyer.underlyingAsset)
            {
                case Cst.UnderlyingAsset.Future:
                case Cst.UnderlyingAsset.ExchangeTradedContract:
                    if (null != Cci(underlyerCategory) && (null != Cci(underlyerCategory).Sql_Table))
                        pPage.SetOpenFormReferential(Cci(underlyerCategory), Cst.OTCml_TBL.DERIVATIVECONTRACT);

                    if (null != Cci(identifier))
                        pPage.SetOpenFormReferential(Cci(identifier), Cst.OTCml_TBL.ASSET_ETD);
                    break;

                case Cst.UnderlyingAsset.Commodity:
                case Cst.UnderlyingAsset.Index:
                case Cst.UnderlyingAsset.RateIndex:
                case Cst.UnderlyingAsset.EquityAsset:
                case Cst.UnderlyingAsset.ExchangeTradedFund:
                    Cst.OTCml_TBL tbl = AssetTools.ConvertUnderlyingAssetToTBL(_tradeDenOption.asset.underlyer.underlyingAsset);
                    if (null != Cci(identifier))
                        pPage.SetOpenFormReferential(Cci(identifier), tbl);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        private void SetMoneyPositionForColor(CciPageBase pPage)
        {
            string moneyPosition = CciTools.GetFieldVariableName(new { _tradeDenOption.moneyPosition }, typeof(TradeDenOption));

            CustomCaptureInfo cciMoneyPosition = this.Cci(moneyPosition);
            Control controlMoneyPosition = (Control)pPage.FindControl(cciMoneyPosition.ClientId);
            if (controlMoneyPosition is TextBox txtBoxlMoneyPosition)
            {
                switch (_tradeDenOption.moneyPositionEnum)
                {
                    case MoneyPositionEnum.AtTheMoney:
                        //txtBoxlMoneyPosition.ForeColor = ... default color
                        break;

                    case MoneyPositionEnum.InTheMoney:
                        txtBoxlMoneyPosition.ForeColor = System.Drawing.Color.DarkGreen;
                        break;

                    case MoneyPositionEnum.OutOfTheMoney:
                        txtBoxlMoneyPosition.ForeColor = System.Drawing.Color.DarkRed;
                        break;
                }
            }
        }

        private void SetDenOptionAction(CciPageBase pPage)
        {
            // La quantité à dénouer est enable si (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.@new)
            bool isEnabled = (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.@new) && (DenOptionActionRequestTypeLength <= 1);

            string quantity = CciTools.GetFieldVariableName(new { _tradeDenOption.quantity }, typeof(TradeDenOption));
            CustomCaptureInfo cci = Cci(quantity);
            if (null != cci)
            {
                if (pPage.FindControl(cci.ClientId) is TextBox ctrl)
                {
                    ctrl.Enabled = isEnabled;
                    ctrl.CssClass = (isEnabled ? EFSCssClass.CssClassEnum.txtCapture.ToString() : EFSCssClass.CssClassEnum.txtCaptureConsult.ToString());
                    //ctrl.Text = _tradeDenOption.quantity.CultureValue;
                    cci.NewValue = _tradeDenOption.quantity.Value;
                    ctrl.Text = cci.NewValueFmtToCurrentCulture;
                    ctrl.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
                    ctrl.Visible = (_tradeDenOption.denOptionActionType != Cst.DenOptionActionType.remove);
                    if (pPage.FindControl(Cst.LBL + cci.ClientId_WithoutPrefix) is Label lbl)
                        lbl.Visible = (_tradeDenOption.denOptionActionType != Cst.DenOptionActionType.remove);
                }
            }

            if ((pPage.FindControl(Cst.IMG + "denOption_tblExeAssPartyPaymentBlock") is Control img) && (null != img.Parent) && (null != img.Parent.Parent))
            {
                img.Parent.Parent.Visible = (_tradeDenOption.denOptionActionType != Cst.DenOptionActionType.remove);
            }

            string actionTitle = CciTools.GetFieldVariableName(new { _tradeDenOption.actionTitle }, typeof(TradeDenOption));
            cci = Cci(actionTitle);
            if (null != cci)
            {
                if (pPage.FindControl(cci.ClientId) is TextBox ctrl)
                {
                    ctrl.Visible = ArrFunc.IsFilled(_tradeDenOption.action);
                    ctrl.Text = Ressource.GetString(cci.ClientId_WithoutPrefix);
                    switch (_tradeDenOption.moneyPositionEnum)
                    {
                        case MoneyPositionEnum.AtTheMoney:
                            //txtBoxlMoneyPosition.ForeColor = ... default color
                            break;

                        case MoneyPositionEnum.InTheMoney:
                            ctrl.ForeColor = System.Drawing.Color.DarkGreen;
                            break;

                        case MoneyPositionEnum.OutOfTheMoney:
                            ctrl.ForeColor = System.Drawing.Color.DarkRed;
                            break;
                    }

                }
            }

            // L'abandon de la quantité restante n'est possible qu'en mode IntraDay
            string abandonRemaining = CciTools.GetFieldVariableName(new { _tradeDenOption.abandonRemaining }, typeof(TradeDenOption));
            cci = Cci(abandonRemaining);
            if (null != cci)
            {
                if (pPage.FindControl(cci.ClientId) is HtmlInputCheckBox ctrl)
                {
                    //ctrl.Visible = true; // (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.@new);
                    ctrl.Visible = (_tradeDenOption.posRequestType != Cst.PosRequestTypeEnum.OptionAbandon) &&
                        (_tradeDenOption.posRequestType != Cst.PosRequestTypeEnum.OptionNotExercised) &&
                        (_tradeDenOption.posRequestType != Cst.PosRequestTypeEnum.OptionNotAssigned) &&
                        (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.@new);
                    if (ctrl.Checked && (_tradeDenOption.PosRequestMode != SettlSessIDEnum.Intraday))
                        ctrl.Checked = false;
                    if (pPage.FindControl(Cst.LBL + cci.ClientId_WithoutPrefix) is Label lbl)
                    {
                        lbl.Visible = (_tradeDenOption.posRequestType != Cst.PosRequestTypeEnum.OptionAbandon) &&
                            (_tradeDenOption.posRequestType != Cst.PosRequestTypeEnum.OptionNotExercised) &&
                            (_tradeDenOption.posRequestType != Cst.PosRequestTypeEnum.OptionNotAssigned) &&
                            (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.@new);
                    }
                }
            }

            // Les check de sélection des lignes de dénouement (s'il en existe) sont rendues visible (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.remove)
            bool isVisible = (_tradeDenOption.denOptionActionType == Cst.DenOptionActionType.remove);
            int index = 1;
            if (0 < DenOptionActionLength)
            {
                _cciDenOptionAction.ToList().ForEach(item =>
                    {
                        string denIsRemove = CciTools.GetFieldVariableName(new { item.tradeDenOptionAction.denIsRemove }, typeof(TradeDenOptionAction));
                        cci = item.Cci(denIsRemove);
                        if (null != cci)
                        {
                            if (pPage.FindControl(cci.ClientId) is HtmlInputCheckBox ctrl)
                            {
                                ctrl.Visible = isVisible && (item.tradeDenOptionAction.denPosRequestType == _tradeDenOption.posRequestType);
                                ctrl.Checked = item.tradeDenOptionAction.denIsRemove.BoolValue;
                            }
                        }
                        index++;
                    });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            for (int i = 0; i < OtherPartyPaymentLength; i++)
            {
                isOk = _cciOtherPartyPayment[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                if (isOk)
                {
                    pCo.Object = "otherPartyPayment";
                    pCo.Element = "settlementInformation";
                    pCo.OccurenceValue = i + 1;
                    pIsObjSpecified = _cciOtherPartyPayment[i].IsSettlementInfoSpecified;
                    pIsEnabled = _cciOtherPartyPayment[i].IsSettlementInstructionSpecified;
                    break;
                }
            }
            return isOk;

        }

        private void InitializeOtherPartyPayment_FromCci()
        {
            ArrayList lst = new ArrayList();
            bool isOk = true;
            int index = -1;
            while (isOk)
            {
                index += 1;
                string date =
                    CciTools.GetFieldVariableName(new { _tradeDenOption.date }, typeof(TradeDenOption));

                CciPayment cciPayment = new CciPayment((CciTrade)_cciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment,
                    TradeCustomCaptureInfos.CCst.Prefix_denOption + CustomObject.KEY_SEPARATOR + "otherPartyPayment", string.Empty, string.Empty, string.Empty, _cciTrade.CciClientIdMainCurrency, CciClientId(date));
                isOk = CcisBase.Contains(cciPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_tradeDenOption.otherPartyPayment) || (index == _tradeDenOption.otherPartyPayment.Length))
                        ReflectionTools.AddItemInArray(_tradeDenOption, "otherPartyPayment", index);

                    cciPayment.Payment = _tradeDenOption.otherPartyPayment[index];

                    lst.Add(cciPayment);
                }
            }
            //
            _cciOtherPartyPayment = new CciPayment[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                _cciOtherPartyPayment[i] = (CciPayment)lst[i];
                _cciOtherPartyPayment[i].Initialize_FromCci();
            }
        }

        private void InitializeDenAction_FromCci()
        {
            if (ArrFunc.IsFilled(_tradeDenOption.action))
            {
                _cciDenOptionAction = new CciDenOptionAction[_tradeDenOption.action.Count()];

                int index = 0;
                _tradeDenOption.action.ToList().ForEach(item =>
                {
                    CciDenOptionAction cci = new CciDenOptionAction(this, index + 1, null,
                        TradeCustomCaptureInfos.CCst.Prefix_denOption + CustomObject.KEY_SEPARATOR + "action")
                    {
                        tradeDenOptionAction = item
                    };
                    cci.Initialize_FromCci();
                    _cciDenOptionAction[index] = cci;
                    index++;
                });
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20200820 [25468] CciDenOptionAction herite de ContainerCciBase 
    public class CciDenOptionAction : ContainerCciBase, IContainerCciFactory
    {
        #region Enums
        public enum CciEnum
        {
            initialQuantity,
            positionQuantity,
            availableQuantity,
            denStatus,
            denRequestMode,
            denQuantity,
            inputQuantity,
            denDate,
            denUser,
            isDenRemove,
            unknown,
        }
        #endregion Enums

        #region Members
        public TradeDenOptionAction tradeDenOptionAction;
        #endregion

        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        #endregion Accessors

        #region Constructor
        public CciDenOptionAction(CciDenOption pCciDenOption, int pActionNumber, TradeDenOptionAction pTradeDenOptionAction, string pPrefix)
            : base(pPrefix, pActionNumber, pCciDenOption.CcisBase)

        {
            tradeDenOptionAction = pTradeDenOptionAction;

        }
        #endregion constructor

        #region AddCciSystem
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();
        }
        #endregion AddCciSystem
        #region CreateInstance
        private void CreateInstance()
        {
        }
        #endregion

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
            // get all the Cci keys
            string[] cciKeysWithoutPrefix = CciTools.GetCciKeys(typeof(TradeDenOptionAction));

            if (!ArrFunc.IsEmpty(cciKeysWithoutPrefix))
            {
                foreach (string cciKeyWithoutPrefix in cciKeysWithoutPrefix)
                {
                    CustomCaptureInfo cci = this.Cci(cciKeyWithoutPrefix);
                    if (cci != null)
                    {
                        string data = CciTools.GetStringValue(cciKeyWithoutPrefix, tradeDenOptionAction, typeof(TradeDenOptionAction));
                        CcisBase.InitializeCci(cci, null, data);
                    }
                }
            }
        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (pCci.ClientId_WithoutPrefix.EndsWith("denOptionActionType"))
            {
                Cst.DenOptionActionType denOptionActionType = (Cst.DenOptionActionType)ReflectionTools.EnumParse(new Cst.DenOptionActionType(), pCci.NewValue);

                if (null != tradeDenOptionAction)
                {
                    CcisBase.SetNewValue(CciClientId(CciEnum.denStatus), tradeDenOptionAction.denStatus.Value);
                    CcisBase.SetNewValue(CciClientId(CciEnum.denRequestMode), tradeDenOptionAction.denRequestMode.Value);
                    CcisBase.SetNewValue(CciClientId(CciEnum.denQuantity), tradeDenOptionAction.denQuantity.Value);
                    CcisBase.SetNewValue(CciClientId(CciEnum.denUser), tradeDenOptionAction.denUser.Value);
                    CcisBase.SetNewValue(CciClientId(CciEnum.denDate), tradeDenOptionAction.denDate.Value);
                    CcisBase.Set(CciClientId(CciEnum.isDenRemove), "IsEnabled", (denOptionActionType == Cst.DenOptionActionType.remove));
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
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            string[] cciKeysWithoutPrefix = CciTools.GetCciKeys(typeof(TradeDenOptionAction));

            if (!ArrFunc.IsEmpty(cciKeysWithoutPrefix))
            {
                foreach (string cciKeyWithoutPrefix in cciKeysWithoutPrefix)
                {
                    CustomCaptureInfo cci = this.Cci(cciKeyWithoutPrefix);

                    if (cci != null && cci.HasChanged)
                    {
                        bool set = CciTools.SetStringValue(cciKeyWithoutPrefix, tradeDenOptionAction, typeof(TradeDenOptionAction), cci.NewValue);
                        if (set)
                            CcisBase.Finalize(cci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);
                    }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200820 [25468] Add Method
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cciItem = Cci(CciEnum.denDate);
            if (pPage.FindControl(cciItem.ClientId) is WCTextBox2 ctrl)
            {
                ctrl.Text = DtFuncExtended.DisplayTimestampAudit(new DateTimeTz(tradeDenOptionAction.denDate.DateTimeValue, "Etc/UTC"), new AuditTimestampInfo()
                {
                    Collaborator = SessionTools.Collaborator,
                    TimestampZone = SessionTools.AuditTimestampZone,
                    Precision = Cst.AuditTimestampPrecision.Minute
                });
            }
        }
    }

}

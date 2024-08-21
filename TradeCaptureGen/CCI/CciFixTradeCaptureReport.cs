#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    /// FI 20190822 [XXXXX] Herite de ContainerCciBase
    /// FI 20200117 [25167] ICciPresentation implementation
    public class CciFixTradeCaptureReport : ContainerCciBase, IContainerCciFactory, IContainerCciGetInfoButton, ICciPresentation
    {
        #region Members
        private readonly IFixTradeCaptureReport _tradeCaptureReport;

        private readonly CciTrade _cciTrade;      //pointeur pour accéder aux éléments du trade
        private readonly CciProductExchangeTradedBase _cciExchangeTraded;  //pointeur pour accéder aux éléments du product

        private readonly CciFixInstrument _cciFixInstrument;
        private readonly CciRptSide _cciRptSide;
        #endregion Members

        #region Enums
        /// <summary>
        /// 
        /// </summary>
        /// FI 20180214 [23774] Modify
        public enum CciEnum
        {
            /// <summary>
            /// Electronic Identifier
            /// <para>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP1/tag17.html</para>
            /// </summary>
            ExecID,
            ExecType,
            LastPx,
            LastQty,
            TrdDt,
            BizDt,
            TxnTm,
            OrdCat,
            OrdStat,


            RptSide_Side,

            TrdSubTyp,
            TrdTyp,
            /// <summary>
            /// Secondary trade type
            /// </summary>
            /// <remarks>20120711 MF Ticket 18006</remarks>
            TrdTyp2,
            /// <summary>
            /// 
            /// <para>
            /// http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag1003.html
            /// </para>
            /// </summary>
            /// FI 20130114 ajout de la gestion de TrdID
            TrdID,

            /// <summary>
            /// N° leg si stratégie
            /// </summary>
            /// FI 20180214 [23774] Add
            TrdLeg_LegNo,

            /// <summary>
            /// Type de stratégie
            /// </summary>
            /// FI 20180214 [23774] Add
            TrdLeg_SecSubTyp,
        }
        #endregion Enums
        #region Constructors
        public CciFixTradeCaptureReport(CciTrade pCciTrade, CciProductExchangeTradedBase pCciExchangeTraded, string pPrefix, IFixTradeCaptureReport pTradeCaptureReport) :
            base(pPrefix, pCciTrade.Ccis)
        {
            _cciExchangeTraded = pCciExchangeTraded;
            _cciTrade = pCciTrade;

            _tradeCaptureReport = pTradeCaptureReport;

            if (null != pTradeCaptureReport)
            {
                _cciFixInstrument = new CciFixInstrument(pCciTrade, _cciExchangeTraded, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fixInstrument, _tradeCaptureReport.Instrument);
                _cciRptSide = new CciRptSide(_cciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_RptSide, _tradeCaptureReport.TrdCapRptSideGrp);
            }
        }
        #endregion Constructors

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public IFixTradeCaptureReport TradeCaptureReport
        {
            get { return _tradeCaptureReport; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CciFixInstrument CciFixInstrument
        {
            get { return _cciFixInstrument; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CciRptSide CciRptSide
        {
            get { return _cciRptSide; }
        }

        #endregion Accessors

        #region Interfaces
        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _tradeCaptureReport);
            CciFixInstrument.Initialize_FromCci();
            CciRptSide.Initialize_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            if (_cciExchangeTraded.ExchangeTradedContainer.IsOneSide)
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.RptSide_Side), true, TypeData.TypeDataEnum.@string);

            CciFixInstrument.AddCciSystem();
            CciRptSide.AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180214 [23774] Modify
        // EG 20180528 Upd Qty => LongValue
        public void Initialize_FromDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                //
                if (null != cci)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion

                    switch (cciEnum)
                    {
                        #region Trade ID
                        case CciEnum.TrdID:
                            if (true == TradeCaptureReport.TradeIdSpecified)
                                data = TradeCaptureReport.TradeId;
                            break;
                        #endregion Trade ID
                        #region Exec ID
                        case CciEnum.ExecID:
                            if (true == TradeCaptureReport.ExecIdSpecified)
                                data = TradeCaptureReport.ExecId;
                            break;
                        #endregion Exec ID
                        #region Exec Type
                        case CciEnum.ExecType:
                            if (true == TradeCaptureReport.ExecTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<ExecTypeEnum>(TradeCaptureReport.ExecType);
                            break;
                        #endregion Exec Type
                        #region Price
                        case CciEnum.LastPx:
                            if (true == TradeCaptureReport.LastPxSpecified)
                                data = TradeCaptureReport.LastPx.Value;
                            break;
                        #endregion Price
                        #region Quantity
                        case CciEnum.LastQty:
                            //20100705 FI [17081] LastQty est tjs integer, même si la modélisation FixML de de type Float   
                            //La modélisation FixML prévoit que cette donnée soit de type float dans un contexte qui n'est pas celui des Futures et options)
                            //Donc on s'attend à avoir un integer => Le descriptif XML de ce champ doit être aussi de type integer)
                            if (true == TradeCaptureReport.LastQtySpecified)
                            {
                                EFS_Decimal qty = TradeCaptureReport.LastQty;
                                // EG 20170127 Qty Long To Decimal
                                //if (cci.DataType == TypeData.TypeDataEnum.integer)
                                //    //Exemple 150.00 devient 150
                                //    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                //    data = IntFunc.IntValue64(qty.Value, System.Globalization.CultureInfo.InvariantCulture).ToString();
                                //else
                                //    data = qty.Value;

                                // RD 20171222 [23670]
                                //data = qty.Value;
                                if (cci.DataType == TypeData.TypeDataEnum.integer)
                                    //Exemple 150.00 devient 150
                                    //data = qty.Value.TrimEnd(new char[] { '0', '.', ',' }).Trim();
                                    data = qty.LongValue.ToString();
                                else
                                    data = qty.Value;
                            }
                            break;
                        #endregion Quantity
                        #region Clearing Business Date
                        case CciEnum.BizDt:
                            if (true == TradeCaptureReport.ClearingBusinessDateSpecified)
                                data = TradeCaptureReport.ClearingBusinessDate.Value;
                            break;
                        #endregion Clearing Business Date
                        #region Order Category
                        case CciEnum.OrdCat:
                            if (true == TradeCaptureReport.OrderCategorySpecified)
                                data = ReflectionTools.ConvertEnumToString<OrderCategoryEnum>(TradeCaptureReport.OrderCategory);
                            break;
                        #endregion Order Category
                        #region Order Status
                        case CciEnum.OrdStat:
                            if (true == TradeCaptureReport.OrdStatusSpecified)
                                data = ReflectionTools.ConvertEnumToString<OrdStatusEnum>(TradeCaptureReport.OrdStatus);
                            break;
                        #endregion Order Status

                        #region Side
                        case CciEnum.RptSide_Side:
                            if (TradeCaptureReport.TrdCapRptSideGrp[0].SideSpecified)
                                data = ReflectionTools.ConvertEnumToString<SideEnum>(TradeCaptureReport.TrdCapRptSideGrp[0].Side);
                            break;
                        #endregion Side

                        #region Trade Sub Type
                        case CciEnum.TrdSubTyp:
                            if (true == TradeCaptureReport.TrdSubTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<TrdSubTypeEnum>(TradeCaptureReport.TrdSubType);
                            break;
                        #endregion Trade Sub Type
                        #region Trade Type
                        case CciEnum.TrdTyp:
                            if (true == TradeCaptureReport.TrdTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<TrdTypeEnum>(TradeCaptureReport.TrdType);
                            break;
                        #endregion Trade Type
                        #region Secondary Trade Type
                        // 20120711 MF Ticket 18006
                        case CciEnum.TrdTyp2:
                            if (true == TradeCaptureReport.SecondaryTrdTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<SecondaryTrdTypeEnum>(TradeCaptureReport.SecondaryTrdType);
                            break;
                        #endregion Secondary Trade Type

                        case CciEnum.TrdLeg_SecSubTyp: // FI 20180214 [23774] 
                            data = TradeCaptureReport.SecSubTyp;
                            break;
                        case CciEnum.TrdLeg_LegNo: // FI 20180214 [23774] 
                            if (StrFunc.IsFilled(TradeCaptureReport.SecSubTyp))
                                data = TradeCaptureReport.LegNo.ToString();
                            break;

                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }

            CciFixInstrument.Initialize_FromDocument();
            CciRptSide.Initialize_FromDocument();
        }

        #region DumpBizDt_ToDocument
        /// <summary>
        /// Dump a clearedDate into DataDocument (FIXML => BizDt)
        /// </summary>
        /// <param name="pData"></param>
        // EG 20171031 [23509] New
        private void DumpBizDt_ToDocument(string pData)
        {
            TradeCaptureReport.ClearingBusinessDateSpecified = StrFunc.IsFilled(pData);
            TradeCaptureReport.ClearingBusinessDate = new EFS_Date();
            if (TradeCaptureReport.ClearingBusinessDateSpecified)
                TradeCaptureReport.ClearingBusinessDate = new EFS_Date(pData);
        }
        #endregion DumpBizDt_ToDocument

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            bool isOk = IsCciOfContainer(pCci.ClientId_WithoutPrefix);
            if (isOk)
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        case CciEnum.RptSide_Side:
                            if (_cciExchangeTraded.ExchangeTradedContainer.IsOneSide)
                            {
                                if (TradeCaptureReport.TrdCapRptSideGrp[0].SideSpecified)
                                {
                                    string clientId = string.Empty;
                                    if (TradeCaptureReport.TrdCapRptSideGrp[0].Side == SideEnum.Buy)
                                        clientId = _cciExchangeTraded.CciClientIdPayer;
                                    else if (TradeCaptureReport.TrdCapRptSideGrp[0].Side == SideEnum.Sell)
                                        clientId = _cciExchangeTraded.CciClientIdReceiver;
                                    //
                                    if (StrFunc.IsFilled(clientId))
                                        CcisBase.SetNewValue(clientId, _cciTrade.cciParty[0].GetPartyId(true));
                                }
                            }
                            break;

                        case CciEnum.LastPx:
                            break;

                        case CciEnum.TrdLeg_SecSubTyp: // FI 20180214 [23774] 
                            if (StrFunc.IsEmpty(TradeCaptureReport.SecSubTyp))
                                Cci(CciFixTradeCaptureReport.CciEnum.TrdLeg_LegNo).Reset();
                            break;
                    }
                }
            }
            //
            // RD 20110429 [17439]
            // Pour réinitialiser payementDate sur les frais en ClearingBusinessDate +1 JO sur le BC du marché du DC                                
            // FI 20190520 [XXXXX] Utilisation de IsCCiReferenceForInitPaymentDate
            if (this._cciExchangeTraded.IsCCiReferenceForInitPaymentDate(pCci) ||
                CciFixInstrument.IsCci(CciFixInstrument.CciEnum.ID, pCci))
            {
                for (int i = 0; i < _cciTrade.OtherPartyPaymentLength; i++)
                {
                    // RD 20110429
                    // Ici peut être il faudrait le faire uniquement pour les Frais non calculés !?
                    if (_cciTrade.cciOtherPartyPayment[i].IsSpecified)
                        _cciTrade.cciOtherPartyPayment[i].PaymentDateInitialize(true);
                }
            }

            CciFixInstrument.ProcessInitialize(pCci);

            CciRptSide.ProcessInitialize(pCci);

            if (CciFixInstrument.IsCci(CciFixInstrument.CciEnum.Sym, pCci))
                CheckContractDenominateur();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = CciFixInstrument.IsClientId_PayerOrReceiver(pCci);
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            CciFixInstrument.CleanUp();
            CciRptSide.CleanUp();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            CciFixInstrument.SetDisplay(pCci);
            CciRptSide.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            CciFixInstrument.SetEnabled(pIsEnabled);

        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            CciFixInstrument.Initialize_Document();
            CciRptSide.Initialize_Document();
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            CciFixInstrument.RefreshCciEnabled();
            CciRptSide.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            CciFixInstrument.ProcessExecute(pCci);
            CciRptSide.ProcessExecute(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            CciFixInstrument.ProcessExecuteAfterSynchronize(pCci);
            CciRptSide.ProcessExecuteAfterSynchronize(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        public void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region Trade ID
                        case CciEnum.TrdID:
                            TradeCaptureReport.TradeIdSpecified = cci.IsFilledValue;
                            if (TradeCaptureReport.TradeIdSpecified)
                                TradeCaptureReport.TradeId = data;
                            break;
                        #endregion Trade ID
                        #region Exec ID
                        case CciEnum.ExecID:
                            TradeCaptureReport.ExecIdSpecified = cci.IsFilledValue;
                            if (TradeCaptureReport.ExecIdSpecified)
                                TradeCaptureReport.ExecId = data;
                            break;
                        #endregion Exec ID
                        #region Exec Type
                        case CciEnum.ExecType:
                            TradeCaptureReport.ExecTypeSpecified = cci.IsFilledValue;
                            if (true == TradeCaptureReport.ExecTypeSpecified)
                            {
                                ExecTypeEnum execTypeEnum = (ExecTypeEnum)ReflectionTools.EnumParse(TradeCaptureReport.ExecType, data);
                                TradeCaptureReport.ExecType = execTypeEnum;
                            }
                            break;
                        #endregion Exec Type
                        #region Price
                        case CciEnum.LastPx:
                            cci.ErrorMsg = string.Empty;
                            //
                            TradeCaptureReport.LastPxSpecified = StrFunc.IsFilled(data);
                            //
                            TradeCaptureReport.LastPx = new EFS_Decimal("0");
                            if (TradeCaptureReport.LastPxSpecified)
                                TradeCaptureReport.LastPx = new EFS_Decimal(data);
                            //
                            // RD 20200429 [25302]
                            //if (TradeCaptureReport.LastPx.DecValue < decimal.Zero)
                            //    cci.ErrorMsg = Ressource.GetString("Msg_ExchangeTradedDerivativePriceNotPositive");
                            //
                            if (StrFunc.IsEmpty(cci.ErrorMsg))
                                CheckContractDenominateur();
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Price
                        #region Quantity
                        case CciEnum.LastQty:
                            TradeCaptureReport.LastQtySpecified = StrFunc.IsFilled(data);
                            //
                            TradeCaptureReport.LastQty = new EFS_Decimal("0");
                            if (TradeCaptureReport.LastQtySpecified)
                                TradeCaptureReport.LastQty = new EFS_Decimal(data);
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Quantity
                        #region Clearing Business Date
                        case CciEnum.BizDt:
                            // EG 20171031 [23509] Upd
                            DumpBizDt_ToDocument(data);
                            // RD 20110429 [17439]
                            // Pour réinitialiser payementDate sur les frais en ClearingBusinessDate +1 JO sur le BC du marché du DC
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Clearing Business Date
                        #region Order Category
                        case CciEnum.OrdCat:
                            TradeCaptureReport.OrderCategorySpecified = cci.IsFilledValue;
                            if (true == TradeCaptureReport.OrderCategorySpecified)
                            {
                                object orderCategory = ReflectionTools.EnumParse(TradeCaptureReport.OrderCategory, data);
                                if (null == orderCategory)
                                    throw new Exception(StrFunc.AppendFormat("data {0} is not valid for OrderCategoryEnum", data));
                                else
                                    TradeCaptureReport.OrderCategory = (OrderCategoryEnum)orderCategory;
                            }
                            break;
                        #endregion Order Category
                        #region Order Status
                        case CciEnum.OrdStat:
                            TradeCaptureReport.OrdStatusSpecified = cci.IsFilledValue;
                            if (true == TradeCaptureReport.OrdStatusSpecified)
                            {
                                object ordStatus = ReflectionTools.EnumParse(TradeCaptureReport.OrdStatus, data);
                                if (null == ordStatus)
                                    throw new Exception(StrFunc.AppendFormat("data {0} is not valid for OrdStatusEnum", data));
                                else
                                    TradeCaptureReport.OrdStatus = (FixML.Enum.OrdStatusEnum)ordStatus;
                            }
                            break;
                        #endregion Order Status

                        #region Side
                        case CciEnum.RptSide_Side:
                            if (_cciExchangeTraded.ExchangeTradedContainer.IsOneSide)
                            {
                                TradeCaptureReport.TrdCapRptSideGrp[0].SideSpecified = StrFunc.IsFilled(data);
                                if (TradeCaptureReport.TrdCapRptSideGrp[0].SideSpecified)
                                {
                                    SideEnum sideEnum = (SideEnum)ReflectionTools.EnumParse(TradeCaptureReport.TrdCapRptSideGrp[0].Side, data);
                                    TradeCaptureReport.TrdCapRptSideGrp[0].Side = sideEnum;
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Side

                        #region Trade Sub Type
                        case CciEnum.TrdSubTyp:
                            TradeCaptureReport.TrdSubTypeSpecified = cci.IsFilledValue;
                            if (true == TradeCaptureReport.TrdSubTypeSpecified)
                            {
                                TrdSubTypeEnum trdSubTypeEnum = (TrdSubTypeEnum)ReflectionTools.EnumParse(TradeCaptureReport.TrdSubType, data);
                                TradeCaptureReport.TrdSubType = trdSubTypeEnum;
                            }
                            break;
                        #endregion Trade Sub Type
                        #region Trade Type
                        case CciEnum.TrdTyp:
                            TradeCaptureReport.TrdTypeSpecified = cci.IsFilledValue;
                            if (true == TradeCaptureReport.TrdTypeSpecified)
                            {
                                TrdTypeEnum trdTypeEnum = (TrdTypeEnum)ReflectionTools.EnumParse(TradeCaptureReport.TrdType, data);
                                TradeCaptureReport.TrdType = trdTypeEnum;
                            }
                            break;
                        #endregion Trade Type
                        #region Secondary Trade Type
                        // 20120711 MF Ticket 18006
                        case CciEnum.TrdTyp2:
                            TradeCaptureReport.SecondaryTrdTypeSpecified = cci.IsFilledValue;
                            if (true == TradeCaptureReport.SecondaryTrdTypeSpecified)
                            {
                                SecondaryTrdTypeEnum secondaryTrdTypeEnum =
                                    (SecondaryTrdTypeEnum)ReflectionTools.EnumParse(TradeCaptureReport.SecondaryTrdType, data);
                                TradeCaptureReport.SecondaryTrdType = secondaryTrdTypeEnum;
                            }
                            break;
                        #endregion Secondary Trade Type
                        case CciEnum.TrdLeg_SecSubTyp: // FI 20180214 [23774] 
                            TradeCaptureReport.SecSubTyp = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.TrdLeg_LegNo: // FI 20180214 [23774] 
                            TradeCaptureReport.LegNo = IntFunc.IntValue(cci.NewValue);
                            break;

                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                }
            }
            //initilisation de ExecType si non saisi
            if (null != Cci(CciEnum.ExecType))
            {
                if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == TradeCaptureReport.ExecTypeSpecified))
                {
                    if (false == Cci(CciEnum.ExecType).IsInputByUser)
                    {
                        CcisBase.SetNewValue(CciClientId(CciEnum.ExecType), ReflectionTools.ConvertEnumToString<ExecTypeEnum>(ExecTypeEnum.New));
                    }
                }
            }
            //initilisation de TrdTyp si non saisi
            if (null != Cci(CciEnum.TrdTyp))
            {
                if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == TradeCaptureReport.TrdTypeSpecified))
                {
                    if (false == Cci(CciEnum.TrdTyp).IsInputByUser)
                    {
                        CcisBase.SetNewValue(CciClientId(CciEnum.TrdTyp), ReflectionTools.ConvertEnumToString<TrdTypeEnum>(TrdTypeEnum.RegularTrade));
                    }
                }
            }
            // 20120711 MF Ticket 18006 Activer l'initialisation après échange avec PL, 
            //                          la valeur secondaire devrait être vide pour les trades tous neufs
            // Initialisation TrdTyp2 (SecondaryTrdType), si non saisi
            //if (null != Cci(CciEnum.TrdTyp2))
            //{
            //    if (Cst.Capture.IsModeNew(ccis.CaptureMode) && (false == TradeCaptureReport.SecondaryTrdTypeSpecified))
            //    {
            //        if (false == Cci(CciEnum.TrdTyp2).IsInputByUser)
            //        {
            //            ccis.SetNewValue(CciClientId(CciEnum.TrdTyp2), ReflectionTools.EnumValueName(SecondaryTrdTypeEnum.RegularTrade));
            //        }
            //    }
            //}
            //
            CciFixInstrument.Dump_ToDocument();

            _cciRptSide.Dump_ToDocument();
        }

        #endregion IContainerCciFactory Members

        #region IContainerCciGetInfoButton Members
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
            CciFixInstrument.SetButtonReferential(pCci, pCo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }

        #endregion IContainerCciGetInfoButton Members

        #region ICciPresentation
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200117 [25167] ICciPresentation implementation
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            // FI 20200120 [25167] Call DisplayExecID
            DisplayExecID(pPage);

            // FI 20200122 [25175] Call _cciFixInstrument.DumpSpecific_ToGUI
            if (null != _cciFixInstrument)
                _cciFixInstrument.DumpSpecific_ToGUI(pPage);

            if (null != _cciRptSide)
                _cciRptSide.DumpSpecific_ToGUI(pPage);
        }
        #endregion
        #endregion Interfaces


        #region  Methods
        #region public Clear
        /// <summary>
        /// Affecte newValue des ccis gérés par ce CciContainer avec string.Empty
        /// </summary>
        public void Clear()
        {
            CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
            CciFixInstrument.Clear();
        }
        #endregion

        #region private CheckContractDenominateur
        /// <summary>
        /// vérifie que le prix est cohérent par rapport au dénominateur spécifié sur le contrat
        /// <para>Exemple il ne faut pas saisir 1.52 sur un contrat 1/32ème</para>
        /// </summary>
        private void CheckContractDenominateur()
        {
            bool isOk = true;
            //
            if (null == Cci(CciEnum.LastPx))
                throw new Exception("cci [CciEnum.LastPx] doesn't exist");
            //
            //FI 20100429 use LoadSqlDerivativeContractFromFixInstrument
            //FI 20121004 [18172] call ExchangeTradedDerivativeTools.LoadSqlDerivativeContract method
            SQL_DerivativeContract sqlTableDc =
                ExchangeTradedDerivativeTools.LoadSqlDerivativeContract(_cciTrade.CSCacheOn, CciFixInstrument.FixInstrument.SecurityExchange, CciFixInstrument.FixInstrument.Symbol, SQL_Table.ScanDataDtEnabledEnum.No);
            //
            if (null != sqlTableDc && Cci(CciEnum.LastPx).IsFilledValue)
            {
                if (sqlTableDc.InstrumentDen != 100) // inutile de faire le contrôle en base 100
                {
                    string[] decimalPrice = Cci(CciEnum.LastPx).NewValue.Split('.');
                    if (ArrFunc.Count(decimalPrice) > 1)
                    {
                        // RD 20130403 [18547] / Corriger l'algorithme de vérification pour:
                        // - Ne pas prendre en considération le nombre de décimales du prix (DERIVATIVECONTRACT.PRICEDECLOCATOR)
                        //   car c'est une donnée resérvée à l'importaion des DC
                        // - Considérer uniquement les trois chiffres après la virgule pour une base > 100 (128, ...)
                        // - Considérer uniquement les deux chiffres après la virgule pour une base > 10 et < 100 (16, 32, ...)
                        // - Considérer uniquement le premier chiffre après la virgule pour une base <= 10 (10, 8, ...)

                        // 1- Extraire la partie décimale                        
                        // Exemples: 
                        // a - pour un prix en base 32  : 128.265000000 donne: 265000000
                        // b - pour un prix en base 32  : 128.035       donne: 035
                        // c - pour un prix en base 32  : 128.3675      donne: 3675
                        // d - pour un prix en base 32  : 128.3075      donne: 3075
                        // e - pour un prix en base 8   : 128.6500      donne: 6500
                        // f - pour un prix en base 128 : 124.057       donne: 057
                        // g - pour un prix en base 128 : 124.6         donne: 6
                        string decimalPriceDec = decimalPrice[1];

                        // 2- Ramener la partie décimale à 3 chiffres 
                        // Exemples: 
                        // a - 265000000 (en base 32)  donne: 265
                        // b - 035       (en base 32)  donne: 035
                        // c - 3675      (en base 32)  donne: 367
                        // d - 3075      (en base 32)  donne: 307
                        // e - 6500      (en base 8)   donne: 650
                        // f - 057       (en base 128) donne: 057
                        // g - 6         (en base 128) donne: 600
                        decimalPriceDec = decimalPriceDec.PadRight(3, '0');
                        decimalPriceDec = decimalPriceDec.Substring(0, 3);

                        // 3- Convertir le résultat en décimal
                        // Exemples: 
                        // a - 265 (en base 32)  donne: 26.5
                        // b - 035 (en base 32)  donne: 3.5
                        // c - 367 (en base 32)  donne: 36.7
                        // d - 307 (en base 32)  donne: 30.7
                        // e - 650 (en base 8)   donne: 6.5
                        // f - 057 (en base 128) donne: 57
                        // g - 600 (en base 128) donne: 600
                        decimal dec = Convert.ToDecimal(decimalPriceDec);
                        if (sqlTableDc.InstrumentDen <= 10)
                            dec /= 100; // pour les cas des bases 10,8,....
                        else if (sqlTableDc.InstrumentDen < 100)
                            dec /= 10; // pour les cas des bases 16,32,....

                        // 4- Comparer le résultat à la base du prix
                        // Exemples: 
                        // a - 26.5  (Ok en base 32)
                        // b - 3.5   (OK en base 32)
                        // c - 36.7  (NOK en base 32)
                        // d - 30.7  (OK en base 32)
                        // e - 6.5   (Ok en base 8)
                        // f - 57    (Ok en base 128)
                        // g - 600   (NOk en base 128)
                        isOk = (dec < sqlTableDc.InstrumentDen);
                    }
                }
            }
            //
            Cci(CciEnum.LastPx).ErrorMsg = string.Empty;
            if (false == isOk)
                Cci(CciEnum.LastPx).ErrorMsg = Ressource.GetString("Msg_ExchangeTradedDerivativePriceNotCompatibleWithContract");
        }
        #endregion
        #endregion


        /// <summary>
        /// Affichage éventuel du ExecID dans le Header du WCTableH tblTrade
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200120 [25167] Add Method
        private void DisplayExecID(CciPageBase pPage)
        {
            string id = _cciTrade.Ccis.GetIdTableHProduct("tblTradeH");
            if (StrFunc.IsFilled(id))
            {
                if (pPage.FindControl(id) is WCTogglePanel pnl)
                {
                    string data = (this._tradeCaptureReport.ExecIdSpecified ? this._tradeCaptureReport.ExecId : string.Empty);
                    TradeCustomCaptureInfos.SetLinkInfoInTogglePanel(pnl, "ExecId", data, null);
                }
            }
        }
    }
}

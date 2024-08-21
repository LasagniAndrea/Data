using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FixML.Interface;
using FixML.v50SP1.Enum;
using System;
using System.Linq;


namespace EFS.TradeInformation
{
    /// <summary>
    ///
    /// </summary>
    /// FI 20190822 [XXXXX] Herite de ContainerCciBase
    /// FI 20200117 [25167] ICciPresentation implementation
    public class CciRptSide : ContainerCciBase, IContainerCciFactory, ICciPresentation
    {
        #region Membres
        private readonly CciTrade _cciTrade;
        #endregion

        #region Enum
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            InptDev,
            InptSrc,
            /// <summary>
            /// Order id
            /// </summary>
            OrdID,
            OrdInptDev,
            OrdTyp,
            PosEfct,
            /*Side,*/
            SesID,
            Txt,
            ReltdPosID,
            ExecRefID,
            unknown
        }
        #endregion

        #region accessor

        /// <summary>
        /// 
        /// </summary>
        public IFixTrdCapRptSideGrp[] RptSide
        {
            get;
            set;
        }

        #endregion

        #region constructor
        public CciRptSide(CciTrade pCCiTrade, string pPrefix, IFixTrdCapRptSideGrp[] pRptSide) :
            base(pPrefix, pCCiTrade.Ccis)
        {
            _cciTrade = pCCiTrade;
            RptSide = pRptSide;
        }
        #endregion

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
            //Noting TODO
        }
        /// <summary>
        /// Affectation les ccis par lecture du dataDocument
        /// </summary>
        public void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.InptDev:
                            if (RptSide[0].TradeInputDeviceSpecified)
                                data = RptSide[0].TradeInputDevice;
                            break;
                        case CciEnum.InptSrc:
                            if (RptSide[0].TradeInputSourceSpecified)
                                data = RptSide[0].TradeInputSource;
                            break;
                        case CciEnum.OrdID:
                            if (RptSide[0].OrderIdSpecified)
                                data = RptSide[0].OrderId;
                            break;
                        case CciEnum.OrdInptDev:
                            if (RptSide[0].OrderInputDeviceSpecified)
                                data = RptSide[0].OrderInputDevice;
                            break;

                        case CciEnum.OrdTyp:
                            if (RptSide[0].OrdTypeSpecified)
                                data = ReflectionTools.ConvertEnumToString<OrdTypeEnum>(RptSide[0].OrdType);
                            break;

                        case CciEnum.PosEfct:
                            if (RptSide[0].PositionEffectSpecified)
                                data = ReflectionTools.ConvertEnumToString<PositionEffectEnum>(RptSide[0].PositionEffect);
                            break;

                        //case CciEnum.RptSide_Side:
                        //    if (tradeCaptureReport.TrdCapRptSideGrp[0].SideSpecified)
                        //        data = ReflectionTools.EnumValueName(tradeCaptureReport.TrdCapRptSideGrp[0].Side);
                        //    break;

                        case CciEnum.SesID:
                            data = RptSide[0].TradingSessionId;
                            break;

                        case CciEnum.Txt:
                            data = RptSide[0].Text;
                            break;

                        case CciEnum.ReltdPosID: ///PL 20171214 Add because missing
                            IFixRelatedPositionGrp reltdPos = RptSideTools.GetRelatedPositionGrp(RptSide[0], RelatedPositionIDSourceEnum.PositionID, false);
                            if (reltdPos != null)
                                data = reltdPos.ID;
                            break;

                        case CciEnum.ExecRefID:
                            if (RptSide[0].ExecRefIdSpecified)
                                data = RptSide[0].ExecRefId;
                            break;

                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
        }

        /// <summary>
        /// Affectation (pré-proposition) d'un cci à partir du cci {pCCi}. {pCCi} vient d'être modifié.
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            //Nothing TODO
        }
        /// <summary>
        ///  Retourne true si le CCI représente un payer ou un receiver 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;

        }
        /// <summary>
        ///  Nettoyage du dataDocument 
        /// </summary>
        public void CleanUp()
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            //Nothing TODO
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
                        case CciEnum.InptDev:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].TradeInputDeviceSpecified = cci.IsFilledValue;
                                if (true == RptSide[i].TradeInputDeviceSpecified)
                                    RptSide[i].TradeInputDevice = data;
                            }
                            break;

                        case CciEnum.InptSrc:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].TradeInputSourceSpecified = cci.IsFilledValue;
                                if (true == RptSide[i].TradeInputSourceSpecified)
                                    RptSide[i].TradeInputSource = data;
                            }
                            break;

                        case CciEnum.OrdID:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].OrderIdSpecified = cci.IsFilledValue;
                                if (true == RptSide[i].OrderIdSpecified)
                                    RptSide[i].OrderId = data;
                            }
                            break;

                        case CciEnum.OrdInptDev:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].OrderInputDeviceSpecified = cci.IsFilledValue;
                                if (true == RptSide[i].OrderInputDeviceSpecified)
                                    RptSide[i].OrderInputDevice = data;
                            }
                            break;

                        case CciEnum.OrdTyp:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                // MF 20120515 Ticket 17778 new feature / ticket 17047 item 15
                                if (cci.IsFilledValue && !String.IsNullOrEmpty(data))
                                {
                                    RptSide[i].OrdTypeSpecified = true;

                                    // Check if the data value is valid, or a integer or defined as an enum type
                                    if (Int32.TryParse(data, out int value) || Enum.IsDefined(typeof(OrdTypeEnum), data))
                                    {
                                        RptSide[i].OrdType =
                                            (OrdTypeEnum)ReflectionTools.EnumParse(RptSide[i].OrdType, data);
                                    }
                                    else
                                    {
                                        RptSide[i].OrdTypeSpecified = false;
                                    }

                                }
                                else
                                {
                                    RptSide[i].OrdTypeSpecified = false;
                                }
                            }
                            break;

                        case CciEnum.PosEfct:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].PositionEffectSpecified = StrFunc.IsFilled(data);
                                if (RptSide[i].PositionEffectSpecified)
                                {
                                    object posEffect = ReflectionTools.EnumParse(RptSide[i].PositionEffect, data);
                                    if (posEffect == null)
                                        throw new Exception(StrFunc.AppendFormat("data {0} is not valid for PositionEffectEnum", data));
                                    else
                                        RptSide[i].PositionEffect = (PositionEffectEnum)posEffect;
                                }
                            }
                            break;

                        //case CciEnum.RptSide_Side:
                        //    if (_cciExchangeTraded.exchangeTradedContainer.IsOneSide)
                        //    {
                        //        _rptSide[0].SideSpecified = StrFunc.IsFilled(data);
                        //        if (_rptSide[0].SideSpecified)
                        //        {
                        //            SideEnum sideEnum = (SideEnum)ReflectionTools.EnumParse(_rptSide[0].Side, data);
                        //            _rptSide[0].Side = sideEnum;
                        //        }
                        //    }
                        //    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                        //    break;


                        case CciEnum.SesID:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].TradingSessionIdSpecified = cci.IsFilledValue;
                                if (true == RptSide[i].TradingSessionIdSpecified)
                                    RptSide[i].TradingSessionId = data;
                            }
                            break;

                        case CciEnum.Txt:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].TextSpecified = cci.IsFilledValue;
                                if (RptSide[i].TextSpecified)
                                    RptSide[i].Text = data;
                            }
                            break;

                        case CciEnum.ReltdPosID:
                            IFixRelatedPositionGrp reltdPos;
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                if (cci.IsFilledValue)
                                {
                                    reltdPos = RptSideTools.GetRelatedPositionGrp(RptSide[i], RelatedPositionIDSourceEnum.PositionID, true);
                                    reltdPos.ID = data.Trim();
                                }
                                else
                                {
                                    RptSideTools.RemoveRelatedPositionGrp(RptSide[i], RelatedPositionIDSourceEnum.PositionID);
                                }
                            }
                            break;

                        case CciEnum.ExecRefID:
                            for (int i = 0; i < ArrFunc.Count(RptSide); i++)
                            {
                                RptSide[i].ExecRefIdSpecified = cci.IsFilledValue;
                                if (RptSide[i].ExecRefIdSpecified)
                                    RptSide[i].ExecRefId = data;
                            }
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                }
            }
        }

        #endregion

        #region ICciPresentation
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200117 [25167] ICciPresentation implementation
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            // FI 20200120 [25167] call DisplayOrderID
            DisplayOrderID(pPage);
        }
        #endregion

        /// <summary>
        /// Affichage éventuel du OrderId dans le Header du WCTableH tblorderH
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200120 [25167] Add Method
        private void DisplayOrderID(CciPageBase pPage)
        {
            string id = _cciTrade.Ccis.GetIdTableHProduct("tblorderH");
            if (StrFunc.IsFilled(id))
            {
                if (pPage.FindControl(id) is WCTogglePanel pnl)
                {
                    string data = (this.RptSide[0].OrderIdSpecified ? this.RptSide[0].OrderId : string.Empty);
                    TradeCustomCaptureInfos.SetLinkInfoInTogglePanel(pnl, "OrderId", data, null);
                }
            }
        }
    }
}

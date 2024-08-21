#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{


    /// <summary>
    ///
    /// </summary>
    /// FI 20180221 [23803] add
    public class CciTradeFxOptionEarlyTermination : IContainerCci, IContainerCciFactory
    {
        #region Membres
        private readonly string _prefix = string.Empty;
        private readonly CciTradeCommonBase _cciTrade;
        private readonly TradeFxOptionEarlyTermination _fxOptionEarlyTermination;
        #endregion

        #region Enums
        public enum CciEnum
        {
            actionDate,
            actionTime,
            valueDate,
            payer,
            receiver,
            settlementDate,
            cashSettlementAmount_amount,
            cashSettlementAmount_currency,
            note,
            unknown
        }
        #endregion Enums

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCommonCustomCaptureInfos Ccis
        {
            get
            {
                return _cciTrade.Ccis;
            }
        }
        #endregion

        #region constructor
        public CciTradeFxOptionEarlyTermination(CciTradeCommonBase pCCiTrade, TradeFxOptionEarlyTermination pTradeFxOptionEarlyTermination, string pPrefix)
        {
            _fxOptionEarlyTermination = pTradeFxOptionEarlyTermination;
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;

        }
        #endregion

        #region Membres de IContainerCci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
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
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion Membres de IContainerCci

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fxOptionEarlyTermination);
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
            //throw new NotImplementedException();
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
                        case CciEnum.actionDate:
                            data = DtFunc.DateTimeToString(_fxOptionEarlyTermination.actionDate.DateTimeValue, DtFunc.FmtISODate);
                            break;
                        case CciEnum.actionTime:
                            data = DtFunc.DateTimeToString(_fxOptionEarlyTermination.actionDate.DateTimeValue, DtFunc.FmtISOTime);
                            break;
                        case CciEnum.valueDate:
                            data = _fxOptionEarlyTermination.valueDate.Value;
                            break;
                        case CciEnum.settlementDate:
                            data = _fxOptionEarlyTermination.settlementDate.Value;
                            break;
                        case CciEnum.cashSettlementAmount_amount:
                            data = _fxOptionEarlyTermination.cashSettlement.Amount.Value;
                            break;
                        case CciEnum.cashSettlementAmount_currency:
                            data = _fxOptionEarlyTermination.cashSettlement.Currency;
                            break;
                        case CciEnum.payer:
                            data = _fxOptionEarlyTermination.payerPartyReference.HRef;
                            break;
                        case CciEnum.receiver:
                            data = _fxOptionEarlyTermination.receiverPartyReference.HRef;
                            break;
                        case CciEnum.note:
                            if (_fxOptionEarlyTermination.noteSpecified)
                                data = _fxOptionEarlyTermination.note;
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        /// <summary>
        /// Affectation du dataDocument à partir des ccis 
        /// </summary>
        public void Dump_ToDocument()
        {


            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                DateTime dtResult = DateTime.MinValue;

                CustomCaptureInfo cci = Ccis[_prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    DateTime dtBefore;
                    switch (keyEnum)
                    {
                        case CciEnum.actionDate:
                            if (isFilled)
                            {
                                dtBefore = _fxOptionEarlyTermination.actionDate.DateTimeValue;
                                dtResult = new DtFunc().StringDateISOToDateTime(data);
                                dtResult = dtResult.Add(new TimeSpan(dtBefore.Hour, dtBefore.Minute, dtBefore.Second));
                            }
                            _fxOptionEarlyTermination.actionDate = new EFS.GUI.Interface.EFS_DateTime(DtFunc.DateTimeToString(dtResult, DtFunc.FmtISODateTime2));
                            break;
                        case CciEnum.actionTime:
                            dtBefore = new DtFunc().StringDateISOToDateTime(DtFunc.DateTimeToStringDateISO(_fxOptionEarlyTermination.actionDate.DateValue));
                            string[] hhmmss = new string[] { "00", "00", "00" };
                            if (isFilled)
                                hhmmss = data.Split(':');
                            dtResult = dtBefore.Add(new TimeSpan(Convert.ToInt32(hhmmss[0]),
                                    Convert.ToInt32(hhmmss[1]), Convert.ToInt32(hhmmss[2])));
                            _fxOptionEarlyTermination.actionDate = new EFS.GUI.Interface.EFS_DateTime(DtFunc.DateTimeToString(dtResult, DtFunc.FmtISODateTime2));
                            break;
                        case CciEnum.valueDate:
                            _fxOptionEarlyTermination.valueDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.settlementDate:
                            _fxOptionEarlyTermination.settlementDate.Value = data;
                            break;
                        case CciEnum.payer:
                            _fxOptionEarlyTermination.payerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.receiver:
                            _fxOptionEarlyTermination.receiverPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.cashSettlementAmount_amount:
                            _fxOptionEarlyTermination.cashSettlement.Amount.Value = data;
                            break;
                        case CciEnum.cashSettlementAmount_currency:
                            _fxOptionEarlyTermination.cashSettlement.Currency = data;
                            break;
                        case CciEnum.note:
                            _fxOptionEarlyTermination.noteSpecified = StrFunc.IsFilled(data);
                            if (_fxOptionEarlyTermination.noteSpecified)
                                _fxOptionEarlyTermination.note = data;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
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
                    case CciEnum.payer:
                        Ccis.Synchronize(Cci(CciEnum.receiver).ClientId_WithoutPrefix, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.receiver:
                        Ccis.Synchronize(Cci(CciEnum.payer).ClientId_WithoutPrefix, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.valueDate:
                        if (pCci.IsFilled)
                            this.Cci(CciEnum.settlementDate).NewValue = pCci.NewValue;
                        break;
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
            // throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            //   throw new NotImplementedException();
        }
        /// <summary>
        ///  Retourne true si le CCI représente un payer ou un receiver 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return IsCci(CciEnum.payer, pCci) || IsCci(CciEnum.receiver, pCci);
        }
        /// <summary>
        ///  Nettoyage du dataDocument 
        /// </summary>
        public void CleanUp()
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            //throw new NotImplementedException();
        }
        #endregion

    }
}

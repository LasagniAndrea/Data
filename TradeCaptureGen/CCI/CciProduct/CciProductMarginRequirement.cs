#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using EFS.Actor;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.GUI.CCI;

using EfsML;
using EfsML.Business;
using EfsML.Interface;

using FixML.Enum;

using FpML.Interface;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    
    /// <summary>
    /// Description résumée de CciProductMarginRequirement.
    /// </summary>
    public class CciProductMarginRequirement : CciProductBase, IContainerCciFactory , ICciPresentation 
    {
        // EG 20230808 [26454] New Gestion InitialMarginMethod
        public enum CciEnum
        {
            clearingOrganizationPartyReference,
            timing,
            // EG 20171031 [23509] New
            [CciGroup(name = "MarketTimeZone")]
            orderEntered,
            // EG 20171031 [23509] New
            clearedDate,
            initialMarginMethod,
            unknown,
        }
        
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private IMarginRequirement _marginRequirement;
        private IParty _partyEntity;

        /// <summary>
        /// 
        /// </summary>
        private readonly CciTradeRisk _cciTrade;
        /// <summary>
        /// 
        /// </summary>
        private CciSimplePayment[] _cciSimplePayment;
        /// <summary>
        /// 
        /// </summary>
        //private CciTradeParty _cciClearingOrganization; //UNDONE FI 20110523 pas utilisé pour le DUMP
        #endregion Members
        //
        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        #endregion
        //
        #region constructor
        public CciProductMarginRequirement(CciTradeRisk pCciTrade, IMarginRequirement pMarginRequirement, string pPrefix)
            : this(pCciTrade, pMarginRequirement, pPrefix, -1)
        { }
        public CciProductMarginRequirement(CciTradeRisk pCciTrade, IMarginRequirement pMarginRequirement, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pMarginRequirement, pPrefix, pNumber)
        {
            _cciTrade = pCciTrade;
        }
        #endregion constructor

        #region Membres de ITradeCci
        /// <summary>
        /// Obtient le sens rattaché au payer du flux (Sell)
        /// </summary>
        public override string RetSidePayer { get { return SideTools.RetSellSide(); } }

        /// <summary>
        /// Obtient le sens rattaché au receiver du flux (Buy)
        /// </summary>
        public override string RetSideReceiver { get { return SideTools.RetBuySide(); } }

        /// <summary>
        /// Obtient la devise principale du trade (devise du 1er payment)
        /// </summary>
        public override string GetMainCurrency
        {
            get
            {
                return _marginRequirement.Payment[0].PaymentAmount.Currency;
            }
        }

        /// <summary>
        /// Obtient le cci qui représente la devise principale du trade (devise du 1er payment)
        /// </summary>
        public override string CciClientIdMainCurrency
        {
            get
            {
                return _cciSimplePayment[0].CciClientId(CciSimplePayment.CciEnum.paymentAmount_currency);
            }
        }
        #endregion

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// Obtient le payer du 1er payment
        /// </summary>
        public override string CciClientIdPayer
        {
            get { return _cciSimplePayment[0].CciClientIdPayer; }
        }
        /// <summary>
        /// Obtient le receveur du 1er payment
        /// </summary>
        public override string CciClientIdReceiver
        {
            get { return _cciSimplePayment[0].CciClientIdReceiver; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            try
            {
                for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                    _cciSimplePayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);
            }
            catch (Exception) { throw; }
        }
        #endregion

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            InitializePayment_FromCci();
            InitializeClearingOrganization_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void AddCciSystem()
        {
            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                _cciSimplePayment[i].AddCciSystem();
         
        }

        // EG 20171109 [23509] Upd Add orderEntered, clearedDate
        public override void Initialize_FromDocument()
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
                        case CciEnum.clearingOrganizationPartyReference:

                            sql_Table = null;
                            IParty partyClearing = CciTradeCommon.DataDocument.GetParty(_marginRequirement.ClearingOrganizationPartyReference.HRef);
                            if (null != partyClearing)
                            {
                                data = partyClearing.PartyId;
                                SQL_Actor sqlcss = new SQL_Actor(CciTradeCommon.CSCacheOn, partyClearing.OTCmlId);
                                sqlcss.LoadTable();
                                sql_Table = sqlcss;
                            }
                            break;
                        case CciEnum.orderEntered:
                            if (null != _partyEntity)
                            {
                                IPartyTradeInformation partyTradeInformation = CciTradeCommon.DataDocument.GetPartyTradeInformation(_partyEntity.Id);
                                if ((null != partyTradeInformation) && (partyTradeInformation.TimestampsSpecified && partyTradeInformation.Timestamps.OrderEnteredSpecified))
                                {
                                    data = partyTradeInformation.Timestamps.OrderEntered;
                                    SynchronizeTimeZone();
                                }
                            }
                            break;
                        case CciEnum.clearedDate:
                            if (CciTradeCommon.DataDocument.TradeHeader.ClearedDateSpecified)
                                data = CciTradeCommon.DataDocument.TradeHeader.ClearedDate.Value;
                            break;
                        case CciEnum.timing:
                            data = ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(_marginRequirement.Timing);
                            break;

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                    //
                }
            }
            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                _cciSimplePayment[i].Initialize_FromDocument();

        }

        // EG 20171109 [23509] Upd Add orderEntered, clearedDate
        public override void Dump_ToDocument()
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
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region clearingOrganizationPartyReference
                        case CciEnum.clearingOrganizationPartyReference:

                            // RD 20200921 [25246] l'Id de l'acteur est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                            string lastParty_id = XMLTools.GetXmlId(cci.LastValue);
                            if (null != cci.LastSql_Table)
                                lastParty_id = ((SQL_Actor)cci.LastSql_Table).XmlId;

                            _cciTrade.DataDocument.RemoveParty(lastParty_id);
                            //
                            cci.ErrorMsg = string.Empty;
                            cci.Sql_Table = null;
                            _marginRequirement.ClearingOrganizationPartyReference = null;
                            if (StrFunc.IsFilled(data))
                            {
                                SQL_Actor sqlClearingHouse = new SQL_Actor(CciTradeCommon.CSCacheOn, data);
                                if (sqlClearingHouse.IsLoaded)
                                {
                                    cci.Sql_Table = sqlClearingHouse;
                                    _marginRequirement.ClearingOrganizationPartyReference =
                                        _cciTrade.DataDocument.CurrentProduct.ProductBase.CreatePartyReference(sqlClearingHouse.XmlId);

                                    _cciTrade.DataDocument.AddParty(sqlClearingHouse);
                                }
                                else
                                    cci.ErrorMsg = Ressource.GetString("Msg_ClearingOrganizationNotFound");
                            }
                            break;
                        #endregion clearingOrganizationPartyReference

                        #region orderEntered
                        case CciEnum.orderEntered:
                            DumpOrderEntered_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion orderEntered
                        #region clearedDate
                        case CciEnum.clearedDate:
                            DumpClearedDate_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion clearedDate
                        #region Timing
                        case CciEnum.timing:
                            SettlSessIDEnum timingEnum = (SettlSessIDEnum)ReflectionTools.EnumParse(_marginRequirement.Timing, data);
                            _marginRequirement.Timing = timingEnum;
                            break;
                        #endregion Timing
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion default
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
            {
                _cciSimplePayment[i].Dump_ToDocument();
                //
                if (StrFunc.IsEmpty(_cciSimplePayment[i].SimplePayment.PaymentAmount.Id))
                    _cciSimplePayment[i].SimplePayment.PaymentAmount.Id = _cciTrade.DataDocument.GenerateId(TradeCustomCaptureInfos.CCst.NOTIONAL_REFERENCE, false);
            }

        }


        #region DumpOrderEntered_ToDocument
        /// <summary>
        /// Dump a orderEntered into DataDocument
        /// </summary>
        /// <param name="pData"></param>
        // EG 20171109 [23509] New
        private void DumpOrderEntered_ToDocument(string pData)
        {
            if (null != _partyEntity)
            {
                IPartyTradeInformation partyTradeInformation = CciTradeCommon.DataDocument.GetPartyTradeInformation(_partyEntity.Id);
                if (null != partyTradeInformation)
                {
                    if (Tz.Tools.IsDateFilled(pData))
                    {
                        partyTradeInformation.Timestamps.OrderEntered = pData;
                        partyTradeInformation.Timestamps.OrderEnteredSpecified = true;
                    }
                    else
                    {
                        partyTradeInformation.Timestamps.OrderEntered = string.Empty;
                        partyTradeInformation.Timestamps.OrderEnteredSpecified = false;
                    }
                    partyTradeInformation.TimestampsSpecified = partyTradeInformation.Timestamps.OrderEnteredSpecified;
                }
            }
        }
        #endregion DumpOrderEntered_ToDocument
        #region DumpClearedDate_ToDocument
        /// <summary>
        /// Dump a clearedDate into DataDocument
        /// </summary>
        /// <param name="pData"></param>
        // EG 20171031 [23509] New
        private void DumpClearedDate_ToDocument(string pData)
        {
            CciTradeCommon.DataDocument.TradeHeader.TradeDate.Value = pData;
            CciTradeCommon.DataDocument.TradeHeader.ClearedDate.Value = pData;
            CciTradeCommon.DataDocument.TradeHeader.ClearedDateSpecified = StrFunc.IsFilled(pData);
        }
        #endregion DumpClearedDate_ToDocument


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// EG 20171004 [23452] TradeDateTime
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {


            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                _cciSimplePayment[i].ProcessInitialize(pCci);

            if ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                _cciTrade.InitializePartySide();
            //

            // RD 20170109 [22783] Alimenter les éléments _marginRequirement.marginRequirementOfficePartyReference et _marginRequirement.entityPartyReference
            for (int i = 0; i < _cciTrade.PartyLength; i++)
            {
                if (_cciTrade.cciParty[i].IsCci(CciTradeParty.CciEnum.actor, pCci))
                {
                    SQL_Actor sql_Actor = (SQL_Actor)pCci.Sql_Table;
                    if (sql_Actor != null)
                    {
                        if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, sql_Actor.Id, new RoleActor[] { RoleActor.MARGINREQOFFICE }, 0))
                            _marginRequirement.MarginRequirementOfficePartyReference.HRef = sql_Actor.XmlId;
                        if (ActorTools.IsActorWithRole(CciTradeCommon.CSCacheOn, sql_Actor.Id, new RoleActor[] { RoleActor.ENTITY }, 0))
                        {
                            _marginRequirement.EntityPartyReference.HRef = sql_Actor.XmlId;
                            _partyEntity = CciTradeCommon.DataDocument.GetParty(_marginRequirement.EntityPartyReference.HRef);
                        }
                    }
                }
            }

            // RD 20170109 [22783] Alimenter l'élément marginRequirement.payment[0].paymentDate.adjustableDate
            // EG 20171031 [23509] Upd
            //if (_cciTrade.cciTradeHeader.IsCci(CciTradeHeader.CciEnum.clearedDate, pCci))
            if (IsCci(CciEnum.clearedDate, pCci))
            {
                _marginRequirement.Payment[0].PaymentDate.AdjustableDateSpecified = true;
                Nullable<DateTimeOffset> dto = Tz.Tools.ToDateTimeOffset(pCci.NewValue);
                if (dto.HasValue)
                    _marginRequirement.Payment[0].PaymentDate.AdjustableDate = Product.ProductBase.CreateAdjustableDate(dto.Value.DateTime, FpML.Enum.BusinessDayConventionEnum.NotApplicable, null);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool ret = false;
            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
            {
                ret = _cciSimplePayment[i].IsClientId_PayerOrReceiver(pCci);
                if (ret)
                    break;
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                _cciSimplePayment[i].CleanUp();
        }

        // EG 20171109 [23509] Upd Set Enabled/Disabled Party
        // FI 20230131 [WI516] Les parties sont de nouveau enabled 
        // EG 20230808 [26454] New Gestion InitialMarginMethod
        public override void RefreshCciEnabled()
        {
            // RD 20130107 [18337] En mode modification, griser les zones clearingOrganizationPartyReference et Timing
            bool isEnabled = Cst.Capture.IsModeNew(_cciTrade.Ccis.CaptureMode);
            CcisBase.Set(CciClientId(CciEnum.clearingOrganizationPartyReference), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.timing), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.orderEntered), "IsEnabled", isEnabled);
            CcisBase.Set(CciClientId(CciEnum.initialMarginMethod), "IsEnabled", isEnabled);

            //for (int i = 0; i < ArrFunc.Count(CciTradeCommon.cciParty); i++)
            //    CciTools.SetCciContainer(CciTradeCommon.cciParty[i], "IsEnabled", isEnabled);


            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                _cciSimplePayment[i].RefreshCciEnabled();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20230808 [26454] New Gestion InitialMarginMethod
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                _cciSimplePayment[i].SetDisplay(pCci);

            if (IsCci(CciEnum.clearingOrganizationPartyReference, pCci))
            {
                if (null != pCci.Sql_Table)
                {
                    SQL_Actor sqlcss = (SQL_Actor)pCci.Sql_Table;
                    string display = sqlcss.DisplayName;
                    string tmp = Convert.ToString(sqlcss.GetFirstRowColumnValue("WEB"));
                    if (StrFunc.IsFilled(tmp))
                    {
                        string href = (tmp.StartsWith(@"http") ? string.Empty : @"http://") + tmp;
                        display += Cst.HTMLSpace2 + Cst.HTMLSpace2 + @"<a href=""" + href + @""" target=""_blank"" tabindex=""-1"" style=""color:gainsboro;font-size:xx-small"">" + tmp + @"</a>";
                    }
                    pCci.Display = display;
                }
            }
            if (IsCci(CciEnum.initialMarginMethod, pCci))
            {
                if (_marginRequirement.InitialMarginMethodSpecified)
                {
                    _marginRequirement.InitialMarginMethod.ToList().ForEach(item =>
                    pCci.Display += "[" + ReflectionTools.ConvertEnumToString<EfsML.Enum.InitialMarginMethodEnum>(item) + "]");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(_cciTrade.Ccis.CaptureMode) && (false == _cciTrade.Ccis.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(_marginRequirement.Payment[0].PayerPartyReference.HRef) && StrFunc.IsEmpty(_marginRequirement.Payment[0].ReceiverPartyReference.HRef))
                {
                    //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                    // HPC est broker ds les template et ne veut pas être 1 contrepartie
                    // id = GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    _marginRequirement.Payment[0].PayerPartyReference.HRef = id;
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    _cciTrade.AddPartyUnknown();
                // RD 20170109 [22783] Initialiser l'élément _marginRequirement.timing à EOD
                if (_marginRequirement.Timing == SettlSessIDEnum.None)
                    _marginRequirement.Timing = SettlSessIDEnum.EndOfDay;
            }

        }
        #endregion

        #region Membres de IContainerCciQuoteBasis
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {

            bool ret = false;
            //
            if (false == ret)
            {
                for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
                {
                    ret = _cciSimplePayment[i].IsClientId_QuoteBasis(pCci);
                    if (ret)
                        break;
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            return _cciSimplePayment[0].GetCurrency1(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            return _cciSimplePayment[0].GetCurrency2(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            return _cciSimplePayment[0].GetBaseCurrency(pCci);
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        // EG 20171109 [23509] Upd Set _partyEntity
        public override void SetProduct(IProduct pProduct)
        {
            _marginRequirement = (IMarginRequirement)pProduct;
            _partyEntity = CciTradeCommon.DataDocument.GetParty(_marginRequirement.EntityPartyReference.HRef);
            base.SetProduct(pProduct);

        }

        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {

            for (int i = 0; i < ArrFunc.Count(_cciSimplePayment); i++)
            {
                System.Web.UI.Control control = pPage.PlaceHolder.FindControl(Cst.TXT + _cciSimplePayment[i].CciClientId(CciSimplePayment.CciEnum.paymentAmountOrigin_amount));
                if (null != control)
                    control.Visible = (false == _cciSimplePayment[i].IsPaymentEqualOrigin);

                control = pPage.PlaceHolder.FindControl(Cst.LBL + _cciSimplePayment[i].CciClientId(CciSimplePayment.CciEnum.paymentAmountOrigin_amount));
                if (null != control)
                    control.Visible = (false == _cciSimplePayment[i].IsPaymentEqualOrigin);

                control = pPage.PlaceHolder.FindControl(Cst.DDL + _cciSimplePayment[i].CciClientId(CciSimplePayment.CciEnum.paymentAmountOrigin_currency));
                if (null != control)
                    control.Visible = (false == _cciSimplePayment[i].IsPaymentEqualOrigin);
            }
            //
            base.DumpSpecific_ToGUI(pPage);

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializePayment_FromCci()
        {

            bool isOk = true;
            int index = -1;
            //
            ArrayList lst = new ArrayList();
            lst.Clear();
            //
            while (isOk)
            {
                index += 1;
                //
                CciSimplePayment ccipayment = new CciSimplePayment(_cciTrade, index + 1, null, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_payment);
                //
                isOk = CcisBase.Contains(ccipayment.CciClientId(CciSimplePayment.CciEnum.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_marginRequirement.Payment) || (index == _marginRequirement.Payment.Length))
                        ReflectionTools.AddItemInArray(_marginRequirement, "payment", index);
                    ccipayment.SimplePayment = _marginRequirement.Payment[index];
                    //
                    TradeRiskInput tradeRiskInput = ((TradeRiskInput)this.Ccis.TradeCommonInput);
                    if (null != tradeRiskInput.MarginRequirementEvents && (ArrFunc.IsFilled(tradeRiskInput.MarginRequirementEvents.eventItem)))
                    {
                        for (int i = 0; i < ArrFunc.Count(tradeRiskInput.MarginRequirementEvents.eventItem); i++)
                        {
                            EventItem item = tradeRiskInput.MarginRequirementEvents.eventItem[i];
                            if (item.unitSpecified && (item.unit == ccipayment.SimplePayment.PaymentAmount.Currency))
                            {
                                if (item.valorisationSysSpecified)
                                {
                                    IMoney paymentOrigine = Product.ProductBase.CreateMoney();
                                    paymentOrigine.Amount.DecValue = item.valorisationSys.DecValue;
                                    paymentOrigine.Currency = item.unitSys;
                                    ccipayment.SimplePaymentOrigine = paymentOrigine;
                                }
                                break;
                            }
                        }
                    }
                    //
                    lst.Add(ccipayment);
                }
            }
            //
            _cciSimplePayment = (CciSimplePayment[])lst.ToArray(typeof(CciSimplePayment));
            for (int i = 0; i < lst.Count; i++)
                _cciSimplePayment[i].Initialize_FromCci();
        }


        /// <summary>
        /// 
        /// </summary>
        private void InitializeClearingOrganization_FromCci()
        {
            _ = new CciTradeParty(_cciTrade, 0, CciTradeParty.PartyType.party, Prefix + "clearingOrganization");
        }

        #region SynchronizeTimeZone
        // EG 20171109 [23509] New
        private void SynchronizeTimeZone()
        {
            string timeZone = (null != _partyEntity) && StrFunc.IsFilled(_partyEntity.Tzdbid) ? _partyEntity.Tzdbid : Tz.Tools.UniversalTimeZone;
            IEnumerable<CciEnum> cci = CciTools.GetCciEnum<CciEnum>("MarketTimeZone");
            foreach (CciEnum item in cci)
            {
                string clientIdZone = Cci(item).ClientId.Replace(Cst.TMS, Cst.TMZ);
                CustomCaptureInfo cciZone = CcisBase[clientIdZone, false];
                if (cciZone.NewValue != timeZone)
                    CcisBase.SetNewValue(cciZone.ClientId, false, timeZone, false);
            }
        }
        #endregion SynchronizeTimeZone
        #endregion

        #region Membres de IContainerCciGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        // EG 20230808 [26454] New Gestion InitialMarginMethod
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;

            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.initialMarginMethod, pCci);
                if (isOk)
                {
                    pCo.Element = "efs_initialMarginMethod";
                    pCo.Object = "product";
                    pCo.OccurenceValue = 1;
                    pIsSpecified = _marginRequirement.InitialMarginMethodSpecified;
                    pIsEnabled = true;
                }
            }
            return isOk;
        }
        #endregion Membres de IContainerCciGetInfoButton
    }
    
}
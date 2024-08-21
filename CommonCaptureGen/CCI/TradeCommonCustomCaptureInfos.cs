#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EfsML.Business;
using EfsML.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Ccis Trades de marché, Référentiel Titre, Trade Déposit et Facturation
    /// </summary>
    public class TradeCommonCustomCaptureInfos : CustomCaptureInfosBase
    {
        #region Members
        /// <summary>
        /// Représente le format d'affichage et de saisie préférentiel des échéances sur ASSET ETD
        /// </summary>
        private Cst.ETDMaturityInputFormatEnum _fmtETDMaturityInput;
        #endregion Members

        #region CCst
        public sealed class CCst
        {
            public CCst() { }
            #region Constants
            public const string Prefix_remove = "tradeRemove";
            public const string Prefix_removeAllocation = "removeAllocation";
            public const string Prefix_tradeAdminRemove = "tradeAdminRemove";
            public const string Prefix_trader = "trader";
            public const string Prefix_sales = "sales";
            public const string Prefix_party = "party";
            public const string Prefix_tradeAdminHeader = "tradeAdminHeader";
            public const string Prefix_invoiceSettlementHeader = "invoiceSettlementHeader";
            public const string Prefix_tradeHeader = "tradeHeader";
            public const string Prefix_broker = "broker";
            public const string TRADEDATE_REFERENCE = "tradeDate";
            public const string TRADETIMESTAMP_REFERENCE = "tradeTimestamp";
            #endregion Constants
        }
        #endregion CCst

        #region constante
        public const string PartyUnknown = "{unknown}";
        public const string PartyIssuer = "{Issuer}";
        #endregion

        #region Members

        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public CciTradeCommonBase CciTradeCommon
        {
            get { return (CciTradeCommonBase)CciContainer; }
        }
        
        /// <summary>
        /// Obtient ou définit le format de saisie et d'affichage préférentiel des échéances ETD
        /// </summary>
        public Cst.ETDMaturityInputFormatEnum FmtETDMaturityInput
        {
            get
            {
                return _fmtETDMaturityInput;
            }
            set
            {
                _fmtETDMaturityInput = value;
            }
        }


        /// <summary>
        /// Obtient true lorsque la saisie ne fait pas d'interprétation 
        /// <para>Exemple la saisie de la donnée TODAY sera conservée</para>
        /// <para>Notamment IsPreserveData = true lorsque on est sur un templare</para>
        /// </summary>
        public override bool IsPreserveData
        {
            get
            {
                bool ret = false;
                if (null != TradeCommonInput)
                    ret = TradeCommonInput.TradeStatus.IsStEnvironment_Template;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonInput TradeCommonInput
        {
            get { return (TradeCommonInput)Obj; }
        }

        #endregion Accessors

        #region Constructor
        public TradeCommonCustomCaptureInfos()
            : base()
        {
            _fmtETDMaturityInput = Cst.ETDMaturityInputFormatEnum.FIX;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciContainer"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIsSessionAdmin"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        public TradeCommonCustomCaptureInfos(string pCS, ICustomCaptureInfos pCciContainer, User pUser, string pSessionId, bool pIsGetDefaultOnInitializeCci)
            : base(pCS, pCciContainer, pUser, pSessionId, pIsGetDefaultOnInitializeCci)
        {
            _fmtETDMaturityInput = Cst.ETDMaturityInputFormatEnum.FIX;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pDataInput"></param>
        /// <returns></returns>
        protected override string InterceptInput(CustomCaptureInfo pCci, string pDataInput)
        {
            string ret = pDataInput;

            if (StrFunc.IsFilled(pDataInput))
            {
                if (TypeData.TypeDataEnum.date == pCci.DataType)
                {
                    Regex regex = new Regex(@"^\w*([/+/-]\d+\w*)?$");
                    if (regex.IsMatch(pDataInput))
                    {
                        string defaultSign = string.Empty;
                        pDataInput = pDataInput.ToUpper();
                        regex = new Regex(@"^\w*");
                        string regExValue = regex.Match(pDataInput).Value;
                        // EG 20110208 Add (pDataInput != DtFunc.TODAY)
                        if ((regExValue == pDataInput) && (pDataInput != DtFunc.TODAY)
                            && (pDataInput.EndsWith("Y") || pDataInput.EndsWith("M") || pDataInput.EndsWith("W") || pDataInput.EndsWith("D")))
                        {
                            //pDataInput ne possède ni + ni -, on considèrera donc + par défaut
                            defaultSign = "+";
                            //cela indique aussi qu'il n'existe pas de préfixe (ex.: 1Y qui sera donc sur un Swap identique à E+1Y et sur une FxOption à T+1Y)
                            regExValue = string.Empty;
                        }
                        //
                        string replaceValue = ((CciTradeCommonBase)CciContainer).GetData(regExValue, pCci);
                        replaceValue = new DtFunc().GetDateTimeString(replaceValue, "ddMMyy");
                        if (StrFunc.IsFilled(replaceValue))
                        {
                            //20080620 PL
                            if (StrFunc.IsEmpty(regExValue))
                                ret = replaceValue + defaultSign + ret;
                            else
                            {
                                ret = ret.Replace(regExValue, replaceValue);
                                if (ret == pDataInput)
                                    ret = ret.Replace(regExValue.ToLower(), replaceValue);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 1/ Déversement des CCI sur l'IHM
        /// 2/ Mise à Disabled de certains contrôles
        /// 3/ Reload de certaines DDL
        /// </summary>
        /// <param name="pPage"></param>
        /// EG 20100823 Reverse LBL if cci.HasError (for Actor & Book)
        /// FI 20140708 [20179] Modify => léger Refactoring 
        /// EG 20170822 [23342] Add Cst.TMS 
        /// EG 20171003 [23452] Upd Cst.TMS 
        public override void Dump_ToGUI(CciPageBase pPage)
        {
            PlaceHolder plh = pPage.PlaceHolder;

            //bool isModeConsult = MethodsGUI.IsModeConsult(pPage);
            //bool isModeConsult = Cst.Capture.IsModeConsult(this.CaptureMode);

            DisplayKeyInstrument(plh);

            if (null != CciContainer)
                CciContainer.RefreshCciEnabled();

            #region foreach cci
            foreach (CustomCaptureInfo cci in this)
            {
                //calcul du displayName
                cci.Display = string.Empty;
                CciContainer.SetDisplay(cci);

                // FI 20121126 [18224] appel à la méthode GetCciControl
                //Control control =  (Control)plh.FindControl(cci.ClientId);
                Control control = GetCciControl(cci.ClientId, pPage);
                if (null != control)
                {
                    bool isControlEnabled = cci.IsEnabled;
                    int indexParty = CciTradeCommon.GetIndexParty(cci.ClientId_WithoutPrefix);
                    if (-1 < indexParty)
                        isControlEnabled = IsControlPartyEnabled(CciTradeCommon.cciParty[indexParty], cci);

                    switch (cci.ClientId_Prefix)
                    {
                        case Cst.TXT:
                        case Cst.QKI:
                            SetTextBox(control, cci, isControlEnabled, pPage);
                            break;
                        case Cst.TMS:
                            string timeZone = Tz.Tools.UniversalTimeZone;

                            string clientIdZone = cci.ClientId.Replace(Cst.TMS, Cst.TMZ);
                            CustomCaptureInfo cciZone = this[clientIdZone, false];
                            if (StrFunc.IsFilled(cciZone.NewValue))
                                timeZone = cciZone.NewValue;

                            if (-1 < indexParty)
                            {
                                CustomCaptureInfo cciActor = CciTradeCommon.cciParty[indexParty].Cci(CciTradeParty.CciEnum.actor);
                                if (cciActor.IsFilledValue && (cciActor.Sql_Table is SQL_Actor sql_Actor) && StrFunc.IsFilled(sql_Actor.TimeZone))
                                    timeZone = StrFunc.IsFilled(sql_Actor.TimeZone) ? sql_Actor.TimeZone : Tz.Tools.UniversalTimeZone;
                            }
                            SetTimestamp(control, cci, timeZone, isControlEnabled, pPage);
                            break;
                        case Cst.HSL:
                            SetHtmlSelect(control, cci, isControlEnabled);
                            break;
                        case Cst.DDL:
                            SetDropDown(control, cci, isControlEnabled, pPage);
                            break;
                        case Cst.CHK:
                        case Cst.HCK:
                            SetCheckBox(control, cci, isControlEnabled);
                            break;
                    }

                    SetControlAttributs(control, cci, pPage);

                    SetCciButton(plh, cci, isControlEnabled, pPage);

                    //FI 20121126 [18224] SetCciDisplay a changé de signature 
                    SetCciDisplay(pPage, cci);

                }
            }
            #endregion

            SetImageOnPartiesAndBroker(plh);

            SetCssClassOnPartiesFromSide(plh);

            if ((false == Cst.Capture.IsModeMatch(CaptureMode)) && (pPage.FocusMode == CciPageBase.FocusModeEnum.Forced))
                SetFocus(pPage);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pSql_Table"></param>
        /// <param name="pData"></param>
        /// EG 20170822 [23342] Add IsTypeDateTimeOffset
        /// EG 20171003 [23452] Upd IsTypeDateTimeOffset
        // EG 20171031 [23509] Upd
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override void InitializeCci(CustomCaptureInfo pCci, SQL_Table pSql_Table, string pData)
        {

            string data = pData;
            string savData = data;
            CciTradeCommonBase cciTrade = (CciTradeCommonBase)CciContainer;
            if (false == IsPreserveData)
            {
                if (pCci.IsTypeTime)
                {
                    data = new DtFunc().GetDateTimeString(data, DtFunc.FmtISOTime);
                    if (StrFunc.IsFilled(data) && (data != savData))
                        pCci.LastValue = string.Empty; //Nécessaire pour positionner cci.HasChanged
                }
                else if (pCci.IsTypeDateTimeOffset)
                {
                    // EG 20170822 [22374]
                    data = Tz.Tools.GetDateTimeOffsetString(data, DtFunc.FmtTZISOLongDateTime);
                    if (StrFunc.IsFilled(data) && (data != savData))
                        pCci.LastValue = string.Empty; //Nécessaire pour positionner cci.HasChanged
                }
                else if (pCci.IsTypeDate)
                {
                    data = new DtFunc().GetDateTimeString(data, DtFunc.FmtISODate);
                    //
                    if (StrFunc.IsFilled(data) && (data != savData))
                        pCci.LastValue = string.Empty; //Nécessaire pour positionner cci.HasChanged
                    //
                    if (StrFunc.IsFilled(data) && (data != savData) && IsGetDefaultOnInitializeCci)
                    {
                        string cur = cciTrade.GetMainCurrency;
                        //
                        string idA = string.Empty;
                        if (TradeCommonInput.IsDefaultSpecified(CommonInput.DefaultEnum.party))
                            idA = ((EFS_DefaultParty)TradeCommonInput.GetDefault(CommonInput.DefaultEnum.party)).OTCmlId.ToString();
                        //
                        if (StrFunc.IsFilled(idA) || StrFunc.IsFilled(cur))
                        {
                            IBusinessCenters bcs = TradeCommonInput.CurrentTrade.Product.ProductBase.LoadBusinessCenters(CSTools.SetCacheOn(CS), 
                                null, StrFunc.IsFilled(idA) ? new string[] { idA } : null, StrFunc.IsFilled(cur) ? new string[] { cur } : null, null);
                            if ((null != bcs) && ArrFunc.IsFilled(bcs.BusinessCenter))
                            {
                                DateTime dt = new DtFunc().StringToDateTime(data, DtFunc.FmtISODate);
                                IInterval interval = TradeCommonInput.CurrentTrade.Product.ProductBase.CreateInterval(PeriodEnum.D, 0);
                                dt = Tools.ApplyAdjustedInterval(CSTools.SetCacheOn(CS), dt, interval, bcs, BusinessDayConventionEnum.FOLLOWING, TradeCommonInput.DataDocument);
                                data = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                            }
                        }
                    }
                }
                else if (pCci.IsTypeString)
                {
                    if ((Cst.FpML_EntityOfUserIdentifier == data))
                    {
                        EFS_DefaultParty defaultParty = (EFS_DefaultParty)TradeCommonInput.GetDefault(CommonInput.DefaultEnum.party);

                        if (null != defaultParty)
                        {
                            pCci.LastValue = data;
                            //
                            if (cciTrade.IsCci_Party(CciTradeParty.CciEnum.actor, pCci) ||
                                cciTrade.IsCci_Broker(CciTradeParty.CciEnum.actor, pCci))
                                data = defaultParty.partyId;
                            else
                                data = defaultParty.id;
                        }
                    }
                    else if (pCci.IsMandatory && pCci.IsListRetrieval("predef:BUSINESSCENTER"))
                    {
                        pCci.LastValue = data;
                        if (StrFunc.IsEmpty(data) && IsGetDefaultOnInitializeCci)
                            data = (string)TradeCommonInput.GetDefault(CommonInput.DefaultEnum.businessCenter);
                    }
                    else if (pCci.IsMandatory && pCci.IsListRetrieval("predef:CURRENCY"))
                    {
                        pCci.LastValue = data;
                        if (StrFunc.IsEmpty(data) && IsGetDefaultOnInitializeCci)
                            data = (string)TradeCommonInput.GetDefault(CommonInput.DefaultEnum.currency);
                    }
                }
            }
            pCci.Initialize(pSql_Table, data);

        }

        /// <summary>
        /// Retourne l'élément associé au ClientId, cet élément est nécessairement un array
        /// <para>Exemple sur les swap</para>
        /// <para>si clientid="trade1_swap_swapStream" alors la méthode retourne l'élément swapStream rattaché au swap, lui même étant le product du 1er trade existant dans le dataDocument</para>
        /// </summary>
        /// <param name="pClientid"></param>
        /// <param name="pTradeCommonInput"></param>
        /// <returns></returns>
        public override Array GetArrayElement(string pClientId)
        {
            return GetArrayElement(pClientId, out _);
        }

        /// <summary>
        /// Retourne l'élément associé au ClientId, cet élément est nécessairement un array
        /// <para>Exemple sur les swap</para>
        /// <para>si clientid="trade1_swap_swapStream" alors la méthode retourne l'élément swapStream rattaché au swap, lui même étant le product du 1er trade existant dans le dataDocument</para>
        /// </summary>
        /// <param name="pClientid"></param>
        /// <param name="pCcis"></param>
        /// <param name="pClientid"></param>
        /// <returns></returns>
        public override Array GetArrayElement(string pClientId, out object pParentElement)
        {

            //
            if (null == this.TradeCommonInput)
                throw new ArgumentException("TradeCommonInput is null");
            //
            //dataDocument est la source
            if (null == TradeCommonInput.DataDocument)
                throw new NullReferenceException("TradeCommonInput.DataDocument is null");
            //
            //dataDocument est la source
            if (null == TradeCommonInput.DataDocument.DataDocument)
                throw new NullReferenceException("TradeCommonInput.DataDocument.dataDocument is null");
            //
            //
            pParentElement = null;
            Array ret = null;
            //
            DataDocumentContainer dataDocumentContainer = TradeCommonInput.DataDocument;
            string clientIdNew = ShiftClientIdToDocumentElement(pClientId);
            //

            object currentObject;
            if (clientIdNew.StartsWith("trade1") || clientIdNew.StartsWith("party"))
            {
                //dataDocument est la source
                currentObject = dataDocumentContainer.DataDocument;
            }
            else
            {
                //tradeInput est la source (permet d'attaquer les classes de travail liées aux actions (
                currentObject = this.TradeCommonInput;
            }
            //
            Regex regEx = new Regex(CustomObject.KEY_SEPARATOR.ToString());
            string[] clientId = regEx.Split(clientIdNew);
            for (int i = 0; i < ArrFunc.Count(clientId) - 1; i++)
            {
                string elementName = StrFunc.PutOffSuffixNumeric(clientId[i]);
                //
                object obj = CaptureTools.GetElementByName(currentObject, elementName);
                //
                currentObject = null;
                if (null != obj)
                {
                    if (obj.GetType().IsArray)
                    {
                        int index = StrFunc.GetSuffixNumeric2(clientId[i]) - 1;
                        if (index < 0) index = 0; //garde fou
                        //
                        Array arrayObject = (Array)obj;
                        if (ArrFunc.Count(arrayObject) > index)
                            currentObject = arrayObject.GetValue(index);
                    }
                    else
                    {
                        currentObject = obj;
                    }
                }
                //
                if (null != currentObject)
                {
                    //Si currentObject est de type party  or broker 
                    //on recherche le PartyTradeInformation associé (Il contient les traders et les sales)
                    if (Tools.IsInterfaceOf(currentObject, InterfaceEnum.IParty))
                        currentObject = dataDocumentContainer.GetPartyTradeInformation(((IParty)currentObject).Id);
                    else if (Tools.IsInterfaceOf(currentObject, InterfaceEnum.IReference) && (elementName == "brokerPartyReference"))
                        currentObject = dataDocumentContainer.GetPartyTradeInformation(((IReference)currentObject).HRef);
                }
                //
                if (null == currentObject)
                    break;
            }
            //
            if (null != currentObject)
            {
                pParentElement = currentObject;
                //
                string arrayElementName = clientId[ArrFunc.Count(clientId) - 1];
                object arrayObject = CaptureTools.GetElementByName(currentObject, arrayElementName);
                //
                if (null != arrayObject)
                {
                    if (false == arrayObject.GetType().IsArray)
                        throw new Exception(StrFunc.AppendFormat("{0} is not an array", arrayObject.GetType().ToString()));
                    //
                    ret = (Array)arrayObject;
                }
            }
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDdl"></param>
        /// <param name="pKey"></param>
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        protected void LoadFpMLItemReference(DropDownList pDdl, string pKey,CciPageBase pPage)
        {
            FullConstructor fullctor = new FullConstructor(pPage.CSSMode);
            fullctor.LoadListFpMLReference(TradeCommonInput.FpMLDocReader);
            GUITools.LoadDDLFromListFpMLReference(pDdl, pKey, fullctor.ListFpMLReference);

        }

        /// <summary>
        /// Retourne true si la donnée pValue existe sur un des Emetteurs
        /// <para>Valable uniquement sur les opérations sur titre</para>
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public virtual bool IsValueUseInIssuer(string pValue)
        {
            //20090901 FI Add this function
            return false;
        }

        /// <summary>
        /// Calcul sur Monney => Calcul du montant arrondi par application des règles dec associées à la devise
        /// Calcul après saisie du montant lui même (pbFromAmount=true) ou après saisie de la devise  
        /// Fonction appelée sur les ProcessInitialize() devise et montant
        /// Try Catch => permet de ne pas planter (trop bête pour une histoire d'arrondi)
        /// </summary>
        public void ProcessInitialize_AroundAmount(string pCciClientidAmount, IMoney pMoney, bool pbFromAmount)
        {

            bool isContinue = true;
            decimal amount = pMoney.Amount.DecValue;
            //
            if (pbFromAmount && StrFunc.IsEmpty(pMoney.Currency))
                isContinue = false; // Pas la peine de arrondir s'il n'y a pas de devise
            //
            if (isContinue)
            {
                EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(CS), amount, pMoney.Currency);
                if (amount != cash.AmountRounded)
                {
                    this[pCciClientidAmount].NewValue = StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded);
                    if (pbFromAmount)
                        pMoney.Amount.Value = this[pCciClientidAmount].NewValue;
                    // après modif du montant il est recalculé => ds cas particulier mis à jour du datadocument 
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciClientidAmount"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pbFromAmount"></param>
        public void ProcessInitialize_AroundAmount(string pCciClientidAmount, EFS_Decimal pAmount, string pCurrency, bool pIsFromAmount)
        {
            if ((null != pAmount) && StrFunc.IsFilled(pCurrency) && this.Contains(pCciClientidAmount))
            {
                EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(CS), pAmount.DecValue, pCurrency);
                if (pAmount.DecValue != cash.AmountRounded)
                {
                    this[pCciClientidAmount].NewValue = StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded);
                    if (pIsFromAmount)
                        pAmount.Value = this[pCciClientidAmount].NewValue;
                    // après modif du montant il est recalculés => ds ce cas particulier mis à jour du directement de la classe  
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciClientIdDayType"></param>
        /// <param name="pOffset"></param>
        public void ProcessInitialize_DayType(string pCciClientIdDayType, IOffset pOffset)
        {
            // Initialise le DayType à empty => Daytype as a number of days only
            if ((null == pOffset) || (PeriodEnum.D != pOffset.Period))
                SetNewValue(pCciClientIdDayType, string.Empty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCcibDC"></param>
        /// <param name="pBusinessCenters"></param>
        public static void SetDisplayBusinessCenters(CustomCaptureInfo pCcibDC, IBusinessCenters pBusinessCenters)
        {

            string display = string.Empty;
            if ((null != pBusinessCenters) && ArrFunc.IsFilled(pBusinessCenters.BusinessCenter))
            {
                string listBCs = string.Empty;
                foreach (IBusinessCenter bc in pBusinessCenters.BusinessCenter)
                {
                    listBCs += (listBCs.Length == 0 ? string.Empty : ", ") + bc.Value;
                }
                display = listBCs;
            }
            if (StrFunc.IsFilled(display))
                pCcibDC.Display = display;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCcibDC"></param>
        /// <param name="pBda"></param>
        public void SetDisplayBusinessDayAdjustments(CustomCaptureInfo pCcibDC, IBusinessDayAdjustments pBda)
        {
            SetDisplayBusinessDayAdjustments(pCcibDC, pBda, true);
        }
        public void SetDisplayBusinessDayAdjustments(CustomCaptureInfo pCcibDC, IBusinessDayAdjustments pBda, bool pIsFirstCall)
        {

            //string display = string.Empty;
            if (pIsFirstCall)
                pCcibDC.Display = string.Empty;

            if (null != pBda)
            {
                if (pBda.BusinessCentersDefineSpecified)
                    SetDisplayBusinessCenters(pCcibDC, pBda.BusinessCentersDefine);
                else if (pBda.BusinessCentersReferenceSpecified)
                {
                    object obj = ReflectionTools.GetObjectById(TradeCommonInput.FpMLDataDocReader, pBda.BusinessCentersReference.HRef);
                    if (Tools.IsTypeOrInterfaceOf(obj, InterfaceEnum.IBusinessCenters))
                        SetDisplayBusinessCenters(pCcibDC, (IBusinessCenters)obj);
                }
            }

        }

        /// <summary>
        /// Retourne true si la donnée pValue existe sur une des contreparties ou un des brokers associés
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public bool IsValueUseInCciParty(string pValue)
        {

            bool ret = false;
            //
            if (StrFunc.IsFilled(pValue))
            {
                #region Party
                for (int i = 0; i < ArrFunc.Count(CciTradeCommon.cciParty); i++)
                {
                    if (pValue == CciTradeCommon.cciParty[i].Cci(CciTradeParty.CciEnum.actor).NewValue)
                        ret = true;
                    if (ret)
                        break;
                    //
                    if (false == ret)
                    {
                        for (int j = 0; j < CciTradeCommon.cciParty[i].BrokerLength; j++)
                        {
                            if (pValue == CciTradeCommon.cciParty[i].cciBroker[j].Cci(CciTradeParty.CciEnum.actor).NewValue)
                                ret = true;
                            if (ret)
                                break;
                        }
                    }
                }
                #endregion Party
            }
            //
            return ret;
        }

        /* FI 20180306 [23822] Cette méthode n'est plus appelée
        /// <summary>
        /// Retourne true si la donnée pCci.LastValue existe sur une des contreparties ou un des brokers associés
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsLastValueUseInParty(CustomCaptureInfo pCci)
        {
            bool ret = IsValueUseInCciParty(pCci.LastValue);

            // RD 20141022 [20409] C'est pour prendre en compte l'Entité rajoutée automatiquement en tant que Broker
            // (voir la méthode CciTradeParty.SetBrokerWithEntity)
            if ((false == ret) &&
                (null != pCci.LastSql_Table) &&
                CciTradeCommon.CurrentTrade.brokerPartyReferenceSpecified)
            {
                string lastHRef = ((SQL_Actor)pCci.LastSql_Table).XmlId;
                for (int j = 0; j < ArrFunc.Count(CciTradeCommon.CurrentTrade.brokerPartyReference); j++)
                {
                    if (lastHRef == CciTradeCommon.CurrentTrade.brokerPartyReference[j].hRef)
                        ret = true;
                    if (ret)
                        break;
                }
            }

            return ret;
        }
        
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        private CciTrader IsCciTraderIdentifier(CustomCaptureInfo pCci)
        {

            CciTrader ret = null;
            //Parties    
            if (null == ret)
            {
                for (int indexParty = 0; indexParty < CciTradeCommon.cciParty.Length; indexParty++)
                {
                    if (null == ret)
                    {
                        // Trader de la party 
                        if (ArrFunc.IsFilled(CciTradeCommon.cciParty[indexParty].cciTrader))
                        {
                            for (int i = 0; i < CciTradeCommon.cciParty[indexParty].cciTrader.Length; i++)
                            {
                                if (CciTradeCommon.cciParty[indexParty].cciTrader[i].IsCci(CciTrader.CciEnum.identifier, pCci))
                                {
                                    ret = CciTradeCommon.cciParty[indexParty].cciTrader[i];
                                    if (null != ret)
                                        break;
                                }
                            }
                        }
                    }
                    // Trader d'un des broker de la party
                    if (null == ret)
                    {
                        if (null != CciTradeCommon.cciParty[indexParty].cciBroker)
                        {
                            for (int i = 0; i < CciTradeCommon.cciParty[indexParty].cciBroker.Length; i++)
                            {
                                if (ArrFunc.IsFilled(CciTradeCommon.cciParty[indexParty].cciBroker[i].cciTrader))
                                {
                                    for (int j = 0; j < CciTradeCommon.cciParty[indexParty].cciBroker[i].cciTrader.Length; j++)
                                    {
                                        if (CciTradeCommon.cciParty[indexParty].cciBroker[i].cciTrader[j].IsCci(CciTrader.CciEnum.identifier, pCci))
                                        {
                                            ret = CciTradeCommon.cciParty[indexParty].cciBroker[i].cciTrader[j];
                                            if (null != ret)
                                                break;
                                        }
                                    }
                                }
                                if (null != ret)
                                    break;
                            }
                        }
                    }
                    //
                    if (null != ret)
                        break;
                }
            }
            //Brokers
            if (null == ret)
            {
                for (int indexBroker = 0; indexBroker < CciTradeCommon.BrokerLength; indexBroker++)
                {
                    if (null == ret)
                    {
                        // Trader de la party 
                        if (ArrFunc.IsFilled(CciTradeCommon.cciBroker[indexBroker].cciTrader))
                        {
                            for (int i = 0; i < CciTradeCommon.cciBroker[indexBroker].cciTrader.Length; i++)
                            {
                                if (CciTradeCommon.cciBroker[indexBroker].cciTrader[i].IsCci(CciTrader.CciEnum.identifier, pCci))
                                {
                                    ret = CciTradeCommon.cciBroker[indexBroker].cciTrader[i];
                                    if (null != ret)
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        private CciTrader IsCciSalesIdentifier(CustomCaptureInfo pCci)
        {

            CciTrader ret = null;
            //Parties    
            if (null == ret)
            {
                for (int indexParty = 0; indexParty < CciTradeCommon.cciParty.Length; indexParty++)
                {
                    if (null == ret)
                    {
                        // Trader de la party 
                        if (ArrFunc.IsFilled(CciTradeCommon.cciParty[indexParty].cciSales))
                        {
                            for (int i = 0; i < CciTradeCommon.cciParty[indexParty].cciSales.Length; i++)
                            {
                                if (CciTradeCommon.cciParty[indexParty].cciSales[i].IsCci(CciTrader.CciEnum.identifier, pCci))
                                {
                                    ret = CciTradeCommon.cciParty[indexParty].cciSales[i];
                                    if (null != ret)
                                        break;
                                }
                            }
                        }
                    }
                    // Trader d'un des broker de la party
                    if (null == ret)
                    {
                        if (null != CciTradeCommon.cciParty[indexParty].cciBroker)
                        {
                            for (int i = 0; i < CciTradeCommon.cciParty[indexParty].cciBroker.Length; i++)
                            {
                                if (ArrFunc.IsFilled(CciTradeCommon.cciParty[indexParty].cciBroker[i].cciSales))
                                {
                                    for (int j = 0; j < CciTradeCommon.cciParty[indexParty].cciBroker[i].cciSales.Length; j++)
                                    {
                                        if (CciTradeCommon.cciParty[indexParty].cciBroker[i].cciSales[j].IsCci(CciTrader.CciEnum.identifier, pCci))
                                        {
                                            ret = CciTradeCommon.cciParty[indexParty].cciBroker[i].cciSales[j];
                                            if (null != ret)
                                                break;
                                        }
                                    }
                                }
                                if (null != ret)
                                    break;
                            }
                        }
                    }
                    //
                    if (null != ret)
                        break;
                }
            }
            //Brokers
            if (null == ret)
            {
                for (int indexBroker = 0; indexBroker < CciTradeCommon.BrokerLength; indexBroker++)
                {
                    if (null == ret)
                    {
                        // Trader de la party 
                        if (ArrFunc.IsFilled(CciTradeCommon.cciBroker[indexBroker].cciSales))
                        {
                            for (int i = 0; i < CciTradeCommon.cciBroker[indexBroker].cciSales.Length; i++)
                            {
                                if (CciTradeCommon.cciBroker[indexBroker].cciSales[i].IsCci(CciTrader.CciEnum.identifier, pCci))
                                {
                                    ret = CciTradeCommon.cciBroker[indexBroker].cciSales[i];
                                    if (null != ret)
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return ret;

        }

        /// <summary>
        /// Ajoute dans la dropDown {pddl} un item où Text= pSqlActor.Identifier et value = pSqlActor.XmlId
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="pSqlActor"></param>
        private static void DDLAddActor(DropDownList pddl, SQL_Actor pSqlActor)
        {
            ListItem item = new ListItem(pSqlActor.Identifier, pSqlActor.XmlId);
            if (false == pddl.Items.Contains(item))
                pddl.Items.Add(item);
        }
        /// <summary>
        /// Gestion des panels des acteurs dans la facturation
        /// </summary>
        // EG 20230526 [WI640] Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
        protected static void SetChildSideColorParty(Control pControl, CustomCaptureInfo pCciSide, bool pIsModeConsult)
        {
            foreach (Control ctrl in pControl.Controls)
            {
                //System.Diagnostics.Debug.WriteLine(ctrl.GetType().FullName);
                if (ctrl is WebControl control)
                {
                    string cssClass = control.CssClass;
                    if (cssClass == EFSCssClass.CssClassEnum.txtCaptureConsult.ToString())
                    {
                        if (pCciSide.NewValue == "BUY")
                            cssClass = "txtCaptureBUY";
                        else
                            cssClass = "txtCaptureSELL";
                        control.CssClass = cssClass;
                    }
                    else if (cssClass == EFSCssClass.CssClassEnum.lblCapture.ToString())
                    {
                        control.Visible = (false == pIsModeConsult);
                    }
                }
                if (ctrl.HasControls())
                    SetChildSideColorParty(ctrl, pCciSide, pIsModeConsult);
            }
        }



        // 20120711 MF Ticket 18006 added opQualifTrdType in order to use the Replace command with the old way
        /// <summary>
        /// Obtient une signature et un style de qualification qui caractérise le trade. 
        /// Cette méthode ne gère pas plus que 2 qualifications (First and Second).  
        /// </summary>
        /// <param name="opForeColorFirst">text color of the first qualification label</param>
        /// <param name="opBackColorFirst">background color of the first qualification label</param>
        /// <param name="opQualifFirst">caption of the first qualification label</param>
        /// <param name="opForeColorSecond">text color of the second qualification label</param>
        /// <param name="opBackColorSecond">background color of the second qualification label</param>
        /// <param name="opQualifSecond">caption of the second qualification label</param>
        /// <returns>the drawing activation flag, when true then at least one qualification will be drawn</returns>
        protected virtual bool DisplayNameInstrumentStyle(
            out string opQualifFirst, out string opForeColorFirst, out string opBackColorFirst,
            out string opQualifSecond, out string opForeColorSecond, out string opBackColorSecond,
            out string opQualifStrategy, out string opForeColorStrategy, out string opBackColorStrategy)
        {
            bool ret = false;

            opQualifFirst = null;
            opQualifSecond = null;
            opQualifStrategy = null;

            opForeColorFirst = "transparent";
            opBackColorFirst = "transparent";
            opForeColorSecond = "transparent";
            opBackColorSecond = "transparent";
            opForeColorStrategy = "transparent";
            opBackColorStrategy = "transparent";

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plh"></param>
        // EG 20211220 [XXXXX] Les Labels affichant les libellés (TRDTYPE / SECONDARYTRDTYPE / STRATEGY) sont désormais toujours créés.
        // Les instruction CSS se chargent automatiquement de les afficher ou non.
        // Cette modification permet depuis les zones source (TRDTYPE, SECONDARYTRDTYPE et STRATEGY de mettre à jour ces zones
        // en invoquant sur le ONCHANGE la méthode "SetLinkIdToHeaderToggle" présente sur Trade.js.
        // en évitant un postback
        // EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        private void DisplayKeyInstrument(PlaceHolder plh)
        {
            Control control = (Control)plh.FindControl("DisplayKey_Instrument");
            if ((null != control))
            {
                string displayNameInstrument = TradeCommonInput.DisplayNameInstrument;
                //PL 20120524 New feature
                if (DisplayNameInstrumentStyle(
                    out string qualifTrdTyp, out string foreColorTrdTyp, out string backColorTrdTyp,
                    out string qualifSecondaryTrdTyp, out string foreColorSecondaryTrdTyp, out string backColorSecondaryTrdTyp,
                    out string qualifStrategy, out string foreColorStrategy, out string backColorStrategy))
                {
                    control.Controls.Clear();
                    ((Label)control).Text = Cst.HTMLSpace;

                    // 1. Add instrument label 

                    // TODO 20120712 MF - label already existing before Ticket 18006
                    Label lblDisplayNameInstrument = new Label()
                    {
                        ID = "lblDisplayNameInstrument",
                        Text = !String.IsNullOrEmpty(qualifTrdTyp) ? displayNameInstrument.Replace(qualifTrdTyp, string.Empty) : displayNameInstrument
                    };
                    control.Controls.Add(lblDisplayNameInstrument);

                    // 2. Add TrdTyp label
                    Label lblQualifTrdTyp = GetQualifLabel("lblQualifTrdTyp", foreColorTrdTyp, backColorTrdTyp, qualifTrdTyp);
                    control.Controls.Add(lblQualifTrdTyp);

                    // 3. Add SecondaryTrdTyp label
                    Label lblQualifSecondaryTrdTyp = GetQualifLabel("lblQualifSecondaryTrdTyp", foreColorSecondaryTrdTyp, backColorSecondaryTrdTyp, qualifSecondaryTrdTyp);
                    control.Controls.Add(lblQualifSecondaryTrdTyp);

                   // 4. Add Strategy label
                    Label lblStrategy = GetQualifLabel("lblStrategy", foreColorStrategy, backColorStrategy, qualifStrategy);
                    control.Controls.Add(lblStrategy);
                }
                else
                {
                    ((Label)control).Text = displayNameInstrument;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pForeColor"></param>
        /// <param name="pBackColor"></param>
        /// <param name="pQualif"></param>
        /// <returns></returns>
        /// EG 20211220 [XXXXX] Nouvelle gestion des libellés d'un panel (Header) en fonction d'un contrôle dans la saisie
        /// EG 20211020 [XXXXX] Ajout du paramètre pId pour le contrôle
        private Label GetQualifLabel(string pId, string pForeColor, string pBackColor, string pQualif)
        {
            Color foreColor = Color.FromName(pForeColor);
            Color backColor = Color.FromName(pBackColor);
                      
            string style = string.Empty;

            if ((backColor == Color.Transparent) && (foreColor == Color.Transparent))
            {
                backColor = Color.White;
                foreColor = Color.Black;
            }

            // back color
            if (backColor == Color.Transparent)
            {
                backColor = foreColor;
                foreColor = (foreColor == Color.White) ? Color.Black : Color.White;
            }
            style += "background:" + backColor.Name + "!important;";
            // fore color
            if (foreColor == Color.Transparent)
            {
                foreColor = (backColor == Color.Black) ? Color.White : Color.Black;
            }
            style += "color:" + foreColor.Name + "!important;";

            // 3. Default style: the label will be printed at screen rounded by an ellypse using the same color of the captions, 
            //    and using a font size reduced by a 20% of the parent font size
            style += "border: 1px solid " + foreColor.Name + "!important";

            Label lblQualif = new Label() {
                ID = pId,
                Text = pQualif
            };

            if (StrFunc.IsFilled(pQualif))
                lblQualif.Attributes.Add("style", style);
            return lblQualif;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plh"></param>
        private void SetImageOnPartiesAndBroker(PlaceHolder plh)
        {
            //FI 20110419 [17405] La gestion des images associées aux parties est sortie de la boucle principale => Tuning
            //Cas particulier des images associé aux contreparties
            if (CciTradeCommon.PartyLength == 2) // DebtSecurity=> il existe qu'1 party (l'émetteur), il n'y a pas d'icone associée   
            {
                //
                for (int i = 0; i < CciTradeCommon.PartyLength; i++)
                {
                    CustomCaptureInfo cci = CciTradeCommon.cciParty[i].Cci(CciTradeParty.CciEnum.actor);
                    if (null != cci)
                    {
                        if (plh.FindControl(Cst.PNL + cci.ClientId_WithoutPrefix + "_img") is Panel img)
                        {
                            ControlsTools.RemoveStyleDisplay(img);
                            ControlsTools.SetStyleList(img.Style, "display:none");
                            if (null != cci.Sql_Table)
                            {
                                //20090720 FI add SetCacheOn
                                ActorRoleCollection actorRoleCol = CciTradeCommon.DataDocument.GetActorRole(CSTools.SetCacheOn(CS));
                                if (actorRoleCol.IsActorRole(((SQL_Actor)cci.Sql_Table).Id, RoleActor.CLIENT))
                                {
                                    img.CssClass = CSS.SetCssClass(CSS.Main.customer);
                                    ControlsTools.RemoveStyleDisplay(img);
                                }
                                else if (actorRoleCol.IsActorRole(((SQL_Actor)cci.Sql_Table).Id, RoleActor.ENTITY))
                                {
                                    img.CssClass = CSS.SetCssClass(CSS.Main.entity);
                                    ControlsTools.RemoveStyleDisplay(img);
                                }
                                else if (actorRoleCol.IsActorRole(((SQL_Actor)cci.Sql_Table).Id, RoleActor.COUNTERPARTY))
                                {
                                    img.CssClass = CSS.SetCssClass(CSS.Main.external);
                                    ControlsTools.RemoveStyleDisplay(img);
                                }
                            }
                        }
                    }
                }
                //
                for (int i = 0; i < CciTradeCommon.BrokerLength; i++)
                {
                    CustomCaptureInfo cci = CciTradeCommon.cciBroker[i].Cci(CciTradeParty.CciEnum.actor);
                    // Spheres installé chez un broker, l'entité est le broker
                    if (null != cci)
                    {
                        if (plh.FindControl(Cst.PNL + cci.ClientId_WithoutPrefix + "_img") is Panel img)
                        {
                            ControlsTools.RemoveStyleDisplay(img);
                            ControlsTools.SetStyleList(img.Style, "display:none");
                            if (null != cci.Sql_Table)
                            {
                                ActorRoleCollection actorRoleCol = CciTradeCommon.DataDocument.GetActorRole(CSTools.SetCacheOn(CS));
                                if (actorRoleCol.IsActorRole(((SQL_Actor)cci.Sql_Table).Id, RoleActor.ENTITY))
                                {
                                    img.CssClass = CSS.SetCssClass(CSS.Main.entity);
                                    ControlsTools.RemoveStyleDisplay(img);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plh"></param>
        /// FI 20140204 [19564] Gestion de la couleur sur le bloc UTI
        /// FI 20170928 [23452] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200918 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et Compléments
        // EG 20230526 [WI640] Changement de déclaration (override sur Facturation)
        protected virtual void SetCssClassOnPartiesFromSide(PlaceHolder plh)
        {
            Boolean isModeCapture = Cst.Capture.IsModeConsult(this.CaptureMode);

            for (int i = 0; i < CciTradeCommon.PartyLength; i++)
            {
                CustomCaptureInfo cciSide = CciTradeCommon.cciParty[i].Cci(CciTradeParty.CciEnum.side);
                if (null != cciSide)
                {
                    // 20090911 PM : remplacement de ddlSide.SelectedValue par cciSide.NewValue pour savoir si le side est BUY or SELL
                    Table tblParty = (Table)plh.FindControl("tbl_party" + (i + 1).ToString());
                    if (null == tblParty)
                        tblParty = (Table)plh.FindControl("tradeHeader_tbl_party" + (i + 1).ToString());
                    if (null != tblParty)
                    {
                        if (tblParty.CssClass == "PnlRoundedBuyer" || tblParty.CssClass == "PnlRoundedSeller")
                            tblParty.CssClass = cciSide.NewValue == "BUY" ? "PnlRoundedBuyer" : "PnlRoundedSeller";
                    }

                    if (plh.FindControl("divtradeHeader_party" + (i + 1).ToString() + "_party") is WCTogglePanel pnlParty)
                    {
                        if (pnlParty.CssClass.EndsWith("blue") ||
                            pnlParty.CssClass.EndsWith("red") ||
                            pnlParty.CssClass.EndsWith("gray") ||
                            pnlParty.CssClass.EndsWith("green"))
                        {
                            if (cciSide.ListRetrieval == "predef:crdr")
                            {
                                if (cciSide.NewValue == "BUY")
                                {
                                    //Debit
                                    pnlParty.CssClass = pnlParty.CssClass.Replace("blue", "red").Replace("green", "red").Replace("gray", "red");
                                }
                                else if (cciSide.NewValue == "SELL")
                                {
                                    //Credit
                                    pnlParty.CssClass = pnlParty.CssClass.Replace("blue", "green").Replace("red", "green").Replace("gray", "green");
                                }
                            }
                            else
                            {
                                if (cciSide.NewValue == "BUY")
                                {
                                    pnlParty.CssClass = pnlParty.CssClass.Replace("red", "blue").Replace("gray", "blue");
                                }
                                else if (cciSide.NewValue == "SELL")
                                {
                                    pnlParty.CssClass = pnlParty.CssClass.Replace("blue", "red").Replace("gray", "red");
                                }
                            }

                            if (plh.FindControl("divparty" + (i + 1).ToString() + "_partytradeIdentifier_UTI") is WCTogglePanel pnlPartyUTI)
                                pnlPartyUTI.CssClass = pnlParty.CssClass;

                            // FI 20170928 [23452] Add
                            if (plh.FindControl("divparty" + (i + 1).ToString() + "_partyTradeInformation_MiFID") is WCTogglePanel pnlPartyMiFID)
                                pnlPartyMiFID.CssClass = pnlParty.CssClass;

                        }
                        Control pnlPartyHeader = plh.FindControl("tradeHeader_party" + (i + 1).ToString() + "_ptyHeader");
                        if (null != pnlPartyHeader)
                            SetChildSideColorParty(pnlPartyHeader, cciSide, isModeCapture);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cciTradeParty"></param>
        /// <param name="cci"></param>
        /// <returns></returns>
        private bool IsControlPartyEnabled(CciTradeParty pCciTradeParty, CustomCaptureInfo pCci)
        {
            bool isControlEnabled = pCci.IsEnabled;

            if (false == pCciTradeParty.IsCci(CciTradeParty.CciEnum.actor, pCci) &&
                false == pCciTradeParty.IsCci(CciTradeParty.CciEnum.book, pCci))
            {
                //
                isControlEnabled = isControlEnabled && pCciTradeParty.IsSpecified;
                //GlopREFACTORINGStatutory Voir avec FI, Disabled hedge si not hedging
                //20080903 FI Effectivement il faudra déplacer ce code ds cciParty RefreshCciEnabled
                //Le look Enabled/disabled doit être affecter dans les CCiContainer 
                if (pCciTradeParty.IsSpecified)
                {
                    string columName = null;
                    bool isDataFromBook = false;
                    bool isHedgeClass = false;
                    #region Data From Book (eg: LocalClassDerv, IASClassDerv, HedgeClassDerv, FxClass, ...)
                    if (false == isDataFromBook)
                    {
                        columName = "ISULOCALCLASSDERV";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.localClassDerv, pCci);
                    }
                    if (false == isDataFromBook)
                    {
                        columName = "ISUIASCLASSDERV";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.iasClassDerv, pCci);
                    }
                    if (false == isDataFromBook)
                    {
                        columName = "ISUHEDGECLASSDERV";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.hedgeClassDerv, pCci);
                        isHedgeClass = isDataFromBook;
                    }
                    if (false == isDataFromBook)
                    {
                        columName = "ISULOCALCLASSNDRV";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.localClassNDrv, pCci);
                    }
                    if (false == isDataFromBook)
                    {
                        columName = "ISUIASCLASSNDRV";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.iasClassNDrv, pCci);
                    }
                    if (false == isDataFromBook)
                    {
                        columName = "ISUHEDGECLASSNDRV";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.hedgeClassNDrv, pCci);
                        isHedgeClass = isDataFromBook;
                    }
                    if (!isDataFromBook)
                    {
                        columName = "ISUFXCLASS";
                        isDataFromBook = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.fxClass, pCci);
                    }
                    #endregion  Data From Book (eg: LocalClassDerv, IASClassDerv, HedgeClassDerv, FxClass, ...)
                    if (isDataFromBook)
                    {
                        string clientId = pCciTradeParty.CciClientId(CciTradeParty.CciEnum.book);
                        if (this.Contains(clientId))
                        {
                            SQL_Book sql_Book = (SQL_Book)this[clientId].Sql_Table;
                            if (null != sql_Book)
                                isControlEnabled = isControlEnabled && Convert.ToBoolean(sql_Book.FirstRow[columName]);
                            else
                                isControlEnabled = false;
                            //
                            if (isControlEnabled && isHedgeClass)
                            {
                                string iasClass = null;
                                if (columName.EndsWith("DERV"))
                                    iasClass = pCciTradeParty.Cci(CciTradeParty.CciEnum.iasClassDerv).NewValue;
                                else if (columName.EndsWith("NDRV"))
                                    iasClass = pCciTradeParty.Cci(CciTradeParty.CciEnum.iasClassNDrv).NewValue;
                                //
                                isControlEnabled = isControlEnabled && (iasClass.Trim() == "HEDGING");
                            }
                        }
                    }
                    else
                    {
                        isHedgeClass = CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.hedgeFolder, pCci) ||
                                       CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.hedgeFactor, pCci);
                        //
                        if (isHedgeClass)
                        {
                            CustomCaptureInfo iasClassDervCci = pCciTradeParty.Cci(CciTradeParty.CciEnum.iasClassDerv);
                            CustomCaptureInfo iasClassNDrvCci = pCciTradeParty.Cci(CciTradeParty.CciEnum.iasClassNDrv);
                            //
                            string iasClassDerv = (null != iasClassDervCci ? iasClassDervCci.NewValue : string.Empty);
                            string iasClassNDrv = (null != iasClassNDrvCci ? iasClassNDrvCci.NewValue : string.Empty);
                            //
                            isControlEnabled = isControlEnabled && (iasClassDerv.Trim() == "HEDGING") || (iasClassNDrv.Trim() == "HEDGING");
                        }
                        else
                        {
                            //GLOP FI comprends pas
                            //if (CciTradeCommon.IsCci_Party(CciTrader.CciEnum.factor, cci) && 
                            //    (false == cci.ClientId_WithoutPrefix.EndsWith(CciTrader.CciEnum.identifier.ToString())))
                            //{
                            //    indexTrader = CciTradeCommon.cciParty[indexParty].GetIndexTrader(cci.ClientId_WithoutPrefix);
                            //    CustomCaptureInfo traderTraderIdentifierCci = CciTradeCommon.cciParty[indexParty].cciTrader[indexTrader].Cci(CciTrader.CciEnum.identifier);
                            //    //
                            //    isControlEnabled = isControlEnabled && (null != traderTraderIdentifierCci && StrFunc.IsFilled(traderTraderIdentifierCci.NewValue));
                            //}
                        }
                    }
                }
            }
            return isControlEnabled;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cci"></param>
        /// <param name="isControlEnabled"></param>
        /// FI 20121127 [18224] Refactoring de la méthode
        /// FI 20121204 [18224] Nouveau Refactoring de la méthode
        /// FI 20140708 [20179] Modify: Gestion du mode Match
        /// FI 20140902 [XXXXX] Modify
        /// EG 20171004 [23452] Add test isLoadMapZone
        /// FI 20170214 [23629] Modify
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        protected override void SetDropDown(Control control, CustomCaptureInfo cci, bool isControlEnabled, CciPageBase pPage)
        {
            // RD 20110517 
            // Etant donner que c'est toujours WCDropDownList2 qui est utilisé pour la saisie
            // Voir CustomObjectDropDown.WriteDropDown()
            // alors autant utiliser directement ce control qui est un peu plus riche
            if (!(control is WCDropDownList2 ddl))
                throw new Exception(StrFunc.AppendFormat("control {0} is not a  DropDown", cci.ClientId));

            CustomObjectDropDown coDDL = pPage.GetCciCustomObject(cci.ClientId) as CustomObjectDropDown;

            if (coDDL.ListRetrieval.ToLower() == "predef:QUOTEBASIS".ToLower() ||
                coDDL.ListRetrieval.ToLower() == "predef:STRIKEQUOTEBASIS".ToLower())
            {
                CciTools.DDL_LoadQuoteBasis(coDDL, ddl, CciTradeCommon.GetCurrency1(cci), CciTradeCommon.GetCurrency2(cci));
            }
            else if (false == coDDL.IsDropDownLoaded)
            {
                if (coDDL.IsLoadWithEnum)
                {
                    DDLAddEnumValue(ddl, cci, coDDL);
                }
                else if (coDDL.IsLoadWithMapZone)
                {
                    Tz.Web.LoadMapZoneArguments args = Tz.Web.LoadMapZoneArguments.GetArguments(coDDL.ListRetrieval, false);
                    if (null != args)
                        Tz.Web.LoadMapZone(ddl, args);
                }
                else if (coDDL.IsDynamicListRetrieval)
                {
                    if ((ddl.HasViewer) && coDDL.ContainsReadOnlyMode && StrFunc.IsFilled(coDDL.GetReadOnlyModeValue("columnText")))
                    {
                        DDLReadOnlyModeLoadColumnText(ddl, cci, coDDL);
                    }
                    else
                    {
                        DDLLoadDynamic(ddl, cci, pPage);
                    }
                }
                else if (coDDL.ContainsRelativeTo)
                {
                    LoadFpMLItemReference(ddl, coDDL.RelativeTo, pPage);
                }
                else if (CciTradeCommon.IsClientId_PayerOrReceiver(cci))
                {
                    DDLLoadPayerReceiver(ddl, cci);
                }
                else
                {
                    CciTrader cciTrader = IsCciTraderIdentifier(cci);
                    CciTrader cciSales = IsCciSalesIdentifier(cci);
                    if ((false == ddl.HasViewer) && ((null != cciTrader) || (null != cciSales)))
                    {
                        if (null != cciTrader)
                        {
                            DDLLoadTraders(ddl, cci, cciTrader, ref isControlEnabled, pPage);
                        }
                        else if (null != cciSales)
                        {
                            DDLLoadSales(ddl, cci, cciSales, ref isControlEnabled, pPage);
                        }
                    }
                    else if ((ddl.HasViewer) && coDDL.ContainsReadOnlyMode && StrFunc.IsFilled(coDDL.GetReadOnlyModeValue("columnText")))
                    {
                        DDLReadOnlyModeLoadColumnText(ddl, cci, coDDL);
                    }
                }
            }
            else
            {
                //FI 20140902 
                if (coDDL.ListRetrieval.ToLower() == "predef:enum.[code:positioneffectenum;isressource:false;forcedenum:c|o]")
                {
                    if (this.TradeCommonInput.SQLInstrument.FungibilityMode == FungibilityModeEnum.CLOSE)
                    {
                        ListItem item = ddl.Items.FindByValue(ExchangeTradedDerivativeTools.GetPositionEffect_Open());
                        ddl.Items.Remove(item);
                    }
                }
            }


            if (ddl.HasViewer)// En consultation de trade Spheres® efface la ddl et y place la donnée présente dans le cci
            {
                ControlsTools.DDLLoadSingle_Value(ddl, cci.NewValue);
                // FI 20170214 [23629] coleur Rouge si information inconnue
                if (StrFunc.IsFilled(cci.ErrorMsg) && ddl.Items.Count == 1)
                    ddl.Items[0].Attributes.Add("style", "color:#AE0303");
            }

            if ((!cci.IsMandatory) || TradeCommonInput.TradeStatus.IsStEnvironment_Template)
                ControlsTools.DDLLoad_AddListItemEmptyEmpty(ddl);

            ddl.Enabled = isControlEnabled;
            //
            //bool isFound = ControlsTools.DDLSelectByValue(ddl, data);
            //


            string data = cci.NewValue;
            if ((false == ControlsTools.DDLSelectByValue(ddl, data)) && StrFunc.IsFilled(data))
            {
                bool isCci_Extends = CciTradeCommon.IsCci_Extends(cci);
                if (isCci_Extends && (!cci.IsMandatory))
                {
                    //20090624 PL La donnée n'existe plus ET elle n'est pas obligatoire --> on sélectionne "blanc"
                    data = string.Empty;
                    ControlsTools.DDLSelectByValue(ddl, data);
                }
                else
                {
                    ListItem liUnavailable = new ListItem(data + " " + Ressource.GetString("Msg_UnavailableOrRemoved", "[disabled or removed]"), data);
                    liUnavailable.Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
                    ddl.Items.Add(liUnavailable);
                    ControlsTools.DDLSelectByValue(ddl, data);
                }
            }

            // RD 20110517 
            // Etant donner que c'est toujours WCDropDownList2 qui est utilisé pour la saisie
            // Voir CustomObjectDropDown.WriteDropDown()
            // alors autant utiliser directement ce control qui est un peu plus riche

            // Ne devrait pas être effectué puisque present sur le render de WCDropDownList2
            // mais cela ne marche pas alors on réaffecte les titles des items
            if (ddl.IsSetTextOnTitle)
                ControlsTools.DDLItemsSetTextOnTitle(ddl);

            if (ddl.SelectedIndex >= 0)
                ddl.ToolTip = ddl.Items[ddl.SelectedIndex].Text;

            // FI 20140708 [20179]
            if (Cst.Capture.IsModeMatch(CaptureMode))
            {
                // FI 20200124 [XXXXX] on passe la ddl
                SetLookMatchControl(ddl, cci, pPage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plh"></param>
        /// <param name="cci"></param>
        /// <param name="pPage"></param>
        /// FI 20161129 [RATP] Modify
        // EG 20180514 [23812] Report
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Gestion onclick sur bouton Enabled
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre ouverture page(CustomObjectButtonInputMenu)
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        private void SetCciButton(PlaceHolder plh, CustomCaptureInfo cci, bool isControlEnabled, CciPageBase pPage)
        {
            //FI 20110419 [17405] Le FindControl n'est effectué que lorsque cela est nécessaire=> Tuning
            bool isOk = false;
            bool isSpecified = false;
            bool isEnabled = false;
             
            Boolean isModeConsult = Cst.Capture.IsModeConsult(this.CaptureMode); 

            if (false == isOk)
            {
                #region SetButtonZoomFpmlObject
                CustomObjectButtonFpmlObject co = new CustomObjectButtonFpmlObject();
                isOk = CciTradeCommon.SetButtonZoom(cci, co, ref isSpecified, ref isEnabled);
                if (isOk)
                {
                    if (plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) is WebControl but)
                    {
                        // 20090729 RD Pour gérer l'affichage du Zoom sur l'écran Full pour le mode UpdatePostEvts
                        // 20090831 EG à confirmer
                        //
                        // 20091005 RD Il faut laisser le code original. 
                        // C'est le seul moyen de savoir si le ZoomFull est Modifiable en mode UpdatePostEvts
                        // La methode SetButtonZoom ne pourra pas le savoir, parceque cette information n'est pas spécifiée sur le CO.
                        bool isButtonLockedModifyPostEvts = MethodsGUI.IsModeUpdatePostEvts(pPage) && BoolFunc.IsTrue(but.Attributes["isLockedModifyPostEvts"]);
                        bool isButtonLockedModifyFeesUninvoiced = MethodsGUI.IsModeUpdateFeesUninvoiced(pPage) && BoolFunc.IsTrue(but.Attributes["isLockedModifyFeesUninvoiced"]);
                        //bool isButtonLockedModifyPostEvts = MethodsGUI.IsModeUpdatePostEvts(pPage) && co.IsLockedModifyPostEvts;                        
                        string isModeReadOnly = (co.ContainsIsZoomOnModeReadOnly ? co.IsZoomOnModeReadOnly : Cst.FpML_Boolean_False);
                        co.IsZoomOnModeReadOnly = (BoolFunc.IsTrue(isModeReadOnly) || isButtonLockedModifyPostEvts || isButtonLockedModifyFeesUninvoiced ? Cst.FpML_Boolean_True : Cst.FpML_Boolean_False);
                        //
                        but.Enabled = isEnabled;
                        ControlsTools.SetAttributeOnClickOnControlZoomFpmlObject(co, but);
                        if (isModeConsult && (false == isSpecified))
                            but.Visible = false;
                        //
                        //if (but.GetType().Equals(typeof(ImageButton)) ||
                        //    but.GetType().BaseType.Equals(typeof(ImageButton)))
                        if (but is LinkButton but2)
                        {
                            but2.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                            if (isSpecified)
                                but2.Text = but2.Text.Replace("fa-file", "fa-file-contract");
                            else
                                but2.Text = but2.Text.Replace("fa-contract", "fa-file");
                        }
                    }
                }
                #endregion  SetButtonZoomFpmlObject
            }
            if (false == isOk)
            {
                #region SetButtonScreenBox
                CustomObjectButtonScreenBox cos = new CustomObjectButtonScreenBox();
                isOk = CciTradeCommon.SetButtonScreenBox(cci, cos, ref isSpecified, ref isEnabled);
                if (isOk)
                {
                    if (plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) is WebControl but)
                    {
                        ControlsTools.SetAttributeOnClickOnButtonScreenBox(cos, but);
                        but.Enabled = isEnabled;
                        if (but is Button but2)
                        {
                            but2.Text = but2.Text.Replace(Cst.Space + "...", string.Empty);
                            if (isSpecified)
                                but2.Text += Cst.Space + "...";
                        }
                    }
                }
                #endregion  SetButtonScreenBox
            }
            if (false == isOk)
            {
                #region IsButtonReferential
                CustomObjectButtonReferential cor = pPage.GetCustomObjectButtonReferential(cci.ClientId_WithoutPrefix);
                isOk = (null != cor);
                if (isOk)
                {
                    if (plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) is WebControl but)
                    {
                        //Modifications éventuelles des propriétés de cor en fonction du contexte
                        CciTradeCommon.SetButtonReferential(cci, cor);
                        but.Enabled = isControlEnabled;
                        ControlsTools.SetAttributeOnCLickButtonReferential(cor, but);
                    }
                }
                #endregion SetButtonReferential
            }
            if (false == isOk)
            {
                #region IsButtonInputMenu
                CustomObjectButtonInputMenu com = new CustomObjectButtonInputMenu();
                isOk = CciTradeCommon.IsButtonMenu(cci, ref com);
                if (isOk)
                {
                    if (plh.FindControl(Cst.BUT + cci.ClientId_WithoutPrefix) is WebControl but)
                    {
                        ControlsTools.SetAttributeOnClickOnButtonMenu(pPage, com, but);
                        but.Enabled = isControlEnabled;
                    }
                }
                #endregion IsButtonInputMenu
            }
        }
        /// <summary>
        /// Ajoute dans la DropDown les acteurs brokers 
        /// </summary>
        /// <param name="pddl"></param>
        /// FI 20121127 [18224] new method
        private void DDLAddBrokers(DropDownList pddl)
        {
            //Broker enfant de party
            for (int i = 0; i < ArrFunc.Count(CciTradeCommon.cciParty); i++)
            {
                for (int j = 0; j < ArrFunc.Count(CciTradeCommon.cciParty[i].cciBroker); j++)
                {
                    if (CciTradeCommon.cciParty[i].cciBroker[j].IsSpecified)
                    {
                        SQL_Actor sqlActor = (SQL_Actor)CciTradeCommon.cciParty[i].cciBroker[j].Cci(CciTradeParty.CciEnum.actor).Sql_Table;
                        if (null != sqlActor)
                            DDLAddActor(pddl, sqlActor);
                    }
                }
            }
            //Broker isolé
            for (int i = 0; i < CciTradeCommon.BrokerLength; i++)
            {
                if (CciTradeCommon.cciBroker[i].IsSpecified)
                {
                    SQL_Actor sqlActor = (SQL_Actor)CciTradeCommon.cciBroker[i].Cci(CciTradeParty.CciEnum.actor).Sql_Table;
                    if (null != sqlActor)
                        DDLAddActor(pddl, sqlActor);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20121127 [18224] new method
        private void DDLAddClearingHouse(DropDownList pddl)
        {
            //ClearingHouseOf market
            string exchange = TradeCommonInput.DataDocument.CurrentProduct.GetMarket();

            if (!String.IsNullOrEmpty(exchange))
            {
                //PL 20130208 ISO
                //SQL_Market sqlMarket = new SQL_Market(CciTradeCommon.CSCacheOn, market);
                SQL_Market sqlMarket = new SQL_Market(CciTradeCommon.CSCacheOn, SQL_TableWithID.IDType.FIXML_SecurityExchange, exchange, SQL_Table.ScanDataDtEnabledEnum.No);
                if (sqlMarket.LoadTable(new string[] { "IDA" }))
                {
                    int idClearingHouse = sqlMarket.IdA;
                    if (idClearingHouse > 0)
                    {
                        if (null != TradeCommonInput.DataDocument.GetParty(idClearingHouse.ToString(), PartyInfoEnum.OTCmlId))
                        {
                            SQL_Actor sqlClearingHouse = new SQL_Actor(CciTradeCommon.CSCacheOn, idClearingHouse);
                            DDLAddActor(pddl, sqlClearingHouse);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// FI 20121127 [18224] new method
        private void DDLAddParties(DropDownList pddl)
        {
            for (int i = 0; i < CciTradeCommon.PartyLength; i++)
            {
                if (CciTradeCommon.cciParty[i].IsSpecified)
                {
                    SQL_Actor sql_Actor = (SQL_Actor)CciTradeCommon.cciParty[i].Cci(CciTradeParty.CciEnum.actor).Sql_Table;
                    if (null != sql_Actor)
                        DDLAddActor(pddl, sql_Actor);
                }
                else if (TradeCommonInput.TradeStatus.IsStEnvironment_Template &&
                    (CciTradeCommon.cciParty[i].Cci(CciTradeParty.CciEnum.actor).NewValue == Cst.FpML_EntityOfUserIdentifier))
                {
                    pddl.Items.Add(new ListItem(Cst.FpML_EntityOfUserIdentifier, Cst.FpML_EntityOfUserIdentifier));
                }
                else
                    ControlsTools.DDLLoad_AddListItemEmptyEmpty(pddl);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="cci"></param>
        /// FI 20121127 [18224] new method
        private void DDLLoadDynamic(DropDownList pddl, CustomCaptureInfo cci, CciPageBase pPage)
        {
            //20090624 PLool  Add Try/Catch
            try
            {
                string listRetrieval = cci.ListRetrieval;
                listRetrieval = SessionTools.ReplaceDynamicConstantsWithValues(listRetrieval);
                listRetrieval = CciTradeCommon.ReplaceTradeDynamicConstantsWithValues(cci, listRetrieval);

                pPage.ControlSetState(cci.ClientId, listRetrieval, out bool isStateChange);
                if (isStateChange)
                {
                    ControlsTools.DDLLoad_FromListRetrieval(pddl, CSTools.SetCacheOn(CS), listRetrieval, !cci.IsMandatory, null);
                    pPage.ControlSynchroState(cci.ClientId);
                }
            }
            catch
            {
                ControlsTools.DDLLoad_ErrorOnLoad(pddl, !cci.IsMandatory);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="cciTrader"></param>
        /// <param name="pIsControlEnabled"></param>
        /// FI 20121127 [18224] new method
        private void DDLLoadTraders(DropDownList pddl, CustomCaptureInfo cci, CciTrader cciTrader, ref bool pIsControlEnabled, CciPageBase pPage)
        {
            int idA = cciTrader.CciParty.GetActorIda();
            string state = idA.ToString();

            pPage.ControlSetState(cci.ClientId, state, out bool isStateChange);
            if (isStateChange)
            {
                bool isOk = ControlsTools.DDL_LoadTrader(CSTools.SetCacheOn(CS), pddl, pIsIDinValue: false, idA, User, SessionId);
                pIsControlEnabled = pIsControlEnabled && isOk;
                pPage.ControlSynchroState(cci.ClientId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="cciSales"></param>
        /// <param name="pPage"></param>
        /// <param name="pIsControlEnabled"></param>
        /// FI 20121127 [18224] new method
        private void DDLLoadSales(DropDownList pddl, CustomCaptureInfo cci, CciTrader cciSales, ref bool pIsControlEnabled, CciPageBase pPage)
        {
            //Recherche de l'acteur parent
            int idA = cciSales.CciParty.GetActorIda();

            //Recherche de l'entité
            int idAEntity = 0;
            CustomCaptureInfo cciBook = cciSales.CciParty.Cci(CciTradeParty.CciEnum.book);
            if (null != cciBook)
            {
                SQL_Book sqlBook = (SQL_Book)cciBook.Sql_Table;
                if ((null != sqlBook) && sqlBook.IsLoaded)
                    idAEntity = sqlBook.IdA_Entity;
            }

            string state = idAEntity.ToString() + "{-}" + idA.ToString();
            pPage.ControlSetState(cci.ClientId, state, out bool isStateChange);
            if (isStateChange)
            {
                bool isOk = ControlsTools.DDL_LoadSales(CSTools.SetCacheOn(CS), pddl, pIsIDinValue: false, idA, idAEntity, User, SessionId);
                pIsControlEnabled = pIsControlEnabled && isOk;
                pPage.ControlSynchroState(cci.ClientId);
            }
        }

        /// <summary>
        ///  Alimente la Dropdown avec les contreparties.
        ///  <para>Lorsque cci appartient à un opp la Dropdown est chargée également avec les brokers et la chambre de compensation</para>
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="cci">cci alimenté par la DropDown</param>
        private void DDLLoadPayerReceiver(DropDownList pddl, CustomCaptureInfo cci)
        {
            //Add: Parties
            pddl.Items.Clear();

            DDLAddParties(pddl);

            if (CciTradeCommon.IsClientId_OtherPartyPaymentPayerReceiver(cci))
            {
                DDLAddBrokers(pddl);
                DDLAddClearingHouse(pddl);
                ControlsTools.DDLLoad_AddListItemEmptyEmpty(pddl);
            }

        }
        /// <summary>
        /// Alimente la combo {pddl} avec la valeur d'enum présente dans cci.NewValue
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="cci"></param>
        /// <param name="coDDL"></param>
        /// FI 20170928 [23452] Modify
        private void DDLAddEnumValue(DropDownList pddl, CustomCaptureInfo cci, CustomObjectDropDown coDDL)
        {
            if (StrFunc.IsFilled(cci.NewValue))
            {
                LoadEnumArguments loadEnumArg =
                        LoadEnumArguments.GetArguments(coDDL.ListRetrieval, cci.IsEmpty,
                                                                            coDDL.GetMiscValue("isresource", false),
                                                                            coDDL.GetMiscValue("resourceprefix", string.Empty));

                // RD 20130107 [18337] Eviter des plantages inutiles en cas "loadEnumArg.isDisplayValue = true"
                string text = cci.NewValue;
                string value = cci.NewValue;
                if (false == loadEnumArg.isDisplayValue)
                {
                    // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                    //ExtendEnum extendEnum = ExtendEnumsTools.ListEnumsSchemes[loadEnumArg.code];
                    ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(this.CS, loadEnumArg.code);
                    if (null != extendEnum)
                    {
                        ExtendEnumValue extendEnumValue = extendEnum.GetExtendEnumValueByValue(value);
                        if (null != extendEnumValue)
                        {
                            if (loadEnumArg.isDisplayValueAndExtendValue)
                                text = value + " - " + extendEnumValue.ExtValue;
                            else
                                text = extendEnumValue.ExtValue;
                        }
                    }
                }

                if (loadEnumArg.isResource)
                {
                    // FI 20170928 [23452] gestion de resourcePrefix
                    string resource = StrFunc.IsFilled(loadEnumArg.resourcePrefix) ? loadEnumArg.resourcePrefix + "_" + value : value;
                    string tmp = string.Empty;
                    if (Ressource.GetStringByRef(resource, ref tmp))
                        text = tmp;
                }

                pddl.Items.Add(new ListItem(text, value));
            }
        }

        /// <summary>
        /// Charge une DDL lorsque qu'elle est en mode ReadOnly et qu'il existe une l'attribut columnText
        /// </summary>
        /// <param name="pddl"></param>
        /// <param name="cci"></param>
        /// <param name="coDDL"></param>
        private static void DDLReadOnlyModeLoadColumnText(DropDownList pddl, CustomCaptureInfo cci, CustomObjectDropDown coDDL)
        {
            string columnText = coDDL.GetReadOnlyModeValue("columnText");
            if (cci.Sql_Table != null)
            {
                string value = cci.NewValue;
                string text = string.Empty;
                //PL 20130131 New feature: "||"
                if (columnText.IndexOf("|") > 0)
                {
                    string[] columnList = columnText.Split('|');
                    foreach (string column in columnList)
                    {
                        if (!String.IsNullOrEmpty(column))
                        {
                            if (column.Trim().StartsWith("'"))
                            {
                                //Lorsque la string commence par "'" elle se termine par "'"
                                //Spheres® enlève le 1er caractère et le dernier
                                text += column.Trim().Substring(1, column.Trim().Length - 2);
                            }
                            else
                            {
                                text += cci.Sql_Table.GetFirstRowColumnValue(column.Trim()).ToString();
                            }
                        }
                    }
                }
                else
                {
                    text = cci.Sql_Table.GetFirstRowColumnValue(columnText).ToString();
                }
                pddl.Items.Add(new ListItem(text, value));
            }
        }

        /// <summary>
        ///  Obtient la ressource associé au cci {pCci}
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        /// FI 20131125 [19233] Add Method
        /// FI 20131127 [19233] use StartsWith
        public override string GetRessource(CustomCaptureInfo pCci)
        {
            string res = base.GetRessource(pCci);
            if (this.TradeCommonInput.IsAllocation)
            {
                if (pCci.ClientId_WithoutPrefix.StartsWith("tradeHeader_party1"))
                    res = res + Cst.Space + "Dealer";
                else if (pCci.ClientId_WithoutPrefix.StartsWith("tradeHeader_party2"))
                    res = res + Cst.Space + "Clearer";
            }
            return res;
        }

        /// <summary>
        /// Alimentation des propriétés newValueMatch des ccis
        /// </summary>
        /// FI 20140708 [20179] add method
        protected override void Initialize_NewValueMatch()
        {
            base.Initialize_NewValueMatch();

            if (this.TradeCommonInput.IdT > 0)
            {
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDT), this.TradeCommonInput.IdT);

                string query = "select CLIENTID,MATCHSTATUS from dbo.TRADESTMATCHCCI where IDT=@IDT";

                QueryParameters qerParameters = new QueryParameters(CS, query, dp);

                IDataReader dr = null;
                try
                {
                    dr = DataHelper.ExecuteReader(CS, CommandType.Text, qerParameters.Query, qerParameters.Parameters.GetArrayDbParameter());
                    while (dr.Read())
                    {
                        string clientId = dr["CLIENTID"].ToString();
                        string status = dr["MATCHSTATUS"].ToString().ToLower();

                        Nullable<Cst.MatchEnum> matchEnum = null;
                        if (Enum.IsDefined(typeof(Cst.MatchEnum), status))
                            matchEnum = (Cst.MatchEnum)Enum.Parse(typeof(Cst.MatchEnum), status);

                        if (this.Contains(clientId))
                            this[clientId].NewValueMatch = matchEnum;
                    }
                }
                catch { throw; }
                finally
                {
                    if (null != dr)
                        dr.Close();
                }
            }
        }

        #endregion Methods
    }


    
}

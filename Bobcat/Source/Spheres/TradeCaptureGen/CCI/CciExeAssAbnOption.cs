using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using FpML.Interface;
using FixML.Enum;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.EFSTools;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML;
using EfsML.Enum;
using EfsML.Business;

namespace EFS.TradeInformation
{
    /// <summary>
    /// Class containing the Cci data to perform an exercise/assingnation/abandon related to an Option
    /// </summary>
    public class CciExeAssAbnOption : IContainerCciFactory, IContainerCci, IContainerCciPayerReceiver, IContainerCciGetInfoButton, ICciPresentation
    {

        private ExeAssAbnOption _exeAssAbnOption;
        private ExeAssAbnOption exeAssAbnOption
        {
            get { return _exeAssAbnOption; }
        }

        private CciPayment[] _cciOtherPartyPayment;
        public CciPayment[] cciOtherPartyPayment
        {
            get { return _cciOtherPartyPayment; }
        }

        public int otherPartyPaymentLength
        {
            get { return ArrFunc.IsFilled(_cciOtherPartyPayment) ? _cciOtherPartyPayment.Length : 0; }
        }

        private TradeCommonCustomCaptureInfos _ccis;
        /// <summary>
        /// Get the Ccis collection
        /// </summary>
        public TradeCommonCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }

        private CciTrade _cciTrade = null;
        /// <summary>
        /// Get the reference at the parent trade
        /// </summary>
        public CciTrade cciTrade
        {
            get { return _cciTrade; }
        }

        /// <summary>
        /// Get the reference at the parent product
        /// </summary>
        public CciProductExchangeTradedDerivative cciProduct
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
                return _ccis.CaptureMode;
            }
        }
        /// <summary>
        /// Get the prefix  
        /// </summary>
        static public string Prefix
        {
            get
            {
                return "exeAssAbnOption";
            }
        }


        public CciExeAssAbnOption(
            CciTrade pParent, TradeCommonCustomCaptureInfos pCcis, ExeAssAbnOption pBusinessClass)
        {
            _cciTrade = pParent;
            _ccis = pCcis;
            _exeAssAbnOption = pBusinessClass;
        }

        #region IContainerCciFactory Membres

        /// <summary>
        /// Initialize the _businessStruct from the Cci data
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(
                this, this.GetType(), this, typeof(ExeAssAbnOption).Name);
            //
            // UNDONE .. ne marche pas
            //
            InitializeOtherPartyPayment_FromCci();
        }

        public void AddCciSystem()
        {
            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].AddCciSystem();
        }

        /// <summary>
        /// Update the Cci values from the business class 
        /// </summary>
        public void Initialize_FromDocument()
        {
            // get all the Cci keys
            string[] cciKeysWithoutPrefix = CciTools.GetCciKeys(typeof(ExeAssAbnOption));

            if (!ArrFunc.IsEmpty(cciKeysWithoutPrefix))
            {
                foreach (string cciKeyWithoutPrefix in cciKeysWithoutPrefix)
                {
                    CustomCaptureInfo cci = this.Cci(cciKeyWithoutPrefix);

                    if (cci != null)
                    {
                        string data = CciTools.GetStringValue(cciKeyWithoutPrefix, _exeAssAbnOption, typeof(ExeAssAbnOption));

                        // Init the single Cci
                        Ccis.InitializeCci(cci, null, data);
                    }
                }
            }

            //FI 20130307 alimentation de sql_Table
            string identifierUnderlyer = CciTools.GetFieldVariableName(new { exeAssAbnOption.identifierUnderlyer }, typeof(ExeAssAbnOption));
            if (null != Cci(identifierUnderlyer))
                Cci(identifierUnderlyer).sql_Table = exeAssAbnOption.sqlUnderlyingAsset;

            //FI 20130307 alimentation de sql_Table
            // EG 20141103 Add Test on ExchangeTradedContract
            if ((exeAssAbnOption.underlyingAssetCategoryEnum == Cst.UnderlyingAsset.Future) ||
                (exeAssAbnOption.underlyingAssetCategoryEnum == Cst.UnderlyingAsset.ExchangeTradedContract))
            {
                string underlyerCategory = CciTools.GetFieldVariableName(new { exeAssAbnOption.underlyerCategory }, typeof(ExeAssAbnOption));
                if (null != Cci(underlyerCategory))
                    Cci(underlyerCategory).sql_Table = exeAssAbnOption.sqlUnderlyingDC;
            }


            //
            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].Initialize_FromDocument();
        }

        /// <summary>
        /// Update the business class from the Ccis values
        /// </summary>
        public void Dump_ToDocument()
        {
            // Simple values update

            string[] cciKeysWithoutPrefix = CciTools.GetCciKeys(typeof(ExeAssAbnOption));

            if (!ArrFunc.IsEmpty(cciKeysWithoutPrefix))
                foreach (string cciKeyWithoutPrefix in cciKeysWithoutPrefix)
                {
                    CustomCaptureInfo cci = this.Cci(cciKeyWithoutPrefix);

                    if (cci != null && cci.HasChanged)
                    {
                        bool set = CciTools.SetStringValue(cciKeyWithoutPrefix, _exeAssAbnOption, typeof(ExeAssAbnOption), cci.newValue);

                        if (set)
                            _ccis.Finalize(cci.ClientId_WithoutPrefix, CustomCaptureInfosBase.ProcessQueueEnum.None);
                    }
                }

            // Ccis update
            // FI 20120314 [] call cciOtherPartyPayment[i].Cci(CciPayment.CciEnum.payer).Reset();
            // Cela permet de purger les frais
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            bool isReset = exeAssAbnOption.prevQuantity > 0 && exeAssAbnOption.quantity.LongValue == 0;
            if (isReset)
            {
                for (int i = 0; i < ArrFunc.Count(cciOtherPartyPayment); i++)
                    cciOtherPartyPayment[i].Cci(CciPayment.CciEnum.payer).Reset();

                //// payments reset
                //_exeAssAbnOption.otherPartyPaymentSpecified = false;
                //_exeAssAbnOption.otherPartyPayment = null;
                //InitializeOtherPartyPayment_FromCci();
            }

            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].Dump_ToDocument();
            _exeAssAbnOption.otherPartyPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_exeAssAbnOption.otherPartyPaymentSpecified, _cciOtherPartyPayment);

            // Validation and error
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            bool isEnoughQuantity =
                exeAssAbnOption.quantity.LongValue <= exeAssAbnOption.availableQuantity.LongValue && exeAssAbnOption.quantity.LongValue > 0;

            string quantity = CciTools.GetFieldVariableName(new { exeAssAbnOption.quantity }, typeof(ExeAssAbnOption));
            CustomCaptureInfo cciQuantity = this.Cci(quantity);

            cciQuantity.errorMsg = "";

            if (!isEnoughQuantity && !isReset)
                cciQuantity.errorMsg = Ressource.GetString("exeAssAbnOption_ERRNotEnoughQuantity");

            string date = CciTools.GetFieldVariableName(new { exeAssAbnOption.date }, typeof(ExeAssAbnOption));
            CustomCaptureInfo cciDate = this.Cci(date);

            cciDate.errorMsg = "";
            if (!exeAssAbnOption.isActionCanBePerformed)
                cciDate.errorMsg = Ressource.GetString("exeAssAbnOption_ERRActionCantBePerformedAtBusinessDate");


            string lastQuote = CciTools.GetFieldVariableName(new { exeAssAbnOption.lastQuote }, typeof(ExeAssAbnOption));
            CustomCaptureInfo cciLastQuote = this.Cci(lastQuote);

            if (cciLastQuote != null)
            {
                cciLastQuote.errorMsg = "";
                //if (!exeAssAbnOption.IsQuoteLoaded)
                //    cciLastQuote.errorMsg = Ressource.GetString("exeAssAbnOption_ERRLastQuoteNull");
            }

        }

        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

            //otherPartyPayment
            for (int i = 0; i < otherPartyPaymentLength; i++)
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

        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            //otherPartyPayment
            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].ProcessExecute(pCci);
        }

        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].ProcessExecuteAfterSynchronize(pCci);
        }

        /// <summary>
        /// the Cci does not have any contraints to be feed up
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            if (false == isOk)
            {
                for (int i = 0; i < otherPartyPaymentLength; i++)
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
        public void CleanUp()
        {

            for (int i = 0; i < otherPartyPaymentLength; i++)
            {
                _cciOtherPartyPayment[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(_exeAssAbnOption.otherPartyPayment))
            {
                for (int i = _exeAssAbnOption.otherPartyPayment.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_exeAssAbnOption.otherPartyPayment[i].payerPartyReference))
                        ReflectionTools.RemoveItemInArray(_exeAssAbnOption, "otherPartyPayment", i);
                }
            }
            _exeAssAbnOption.otherPartyPaymentSpecified = ArrFunc.IsFilled(_exeAssAbnOption.otherPartyPayment);
        }

        public void RefreshCciEnabled()
        {

            string quantity = CciTools.GetFieldVariableName(new { exeAssAbnOption.quantity }, typeof(ExeAssAbnOption));
            string note = CciTools.GetFieldVariableName(new { exeAssAbnOption.note }, typeof(ExeAssAbnOption));
            string abandonRemaining = CciTools.GetFieldVariableName(new { exeAssAbnOption.abandonRemaining }, typeof(ExeAssAbnOption));

            CustomCaptureInfo cciQuantity = Cci(quantity);
            CustomCaptureInfo cciNote = Cci(note);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            long actualQuantity;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            bool isAValidQuantity = Int64.TryParse(cciQuantity.newValue, out actualQuantity);
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            bool isEnoughQuantity = actualQuantity <= exeAssAbnOption.availableQuantity.LongValue;
            bool isEnabled = exeAssAbnOption.isActionCanBePerformed && isEnoughQuantity;
            //
            bool isAbandonRemainingEnabled = isEnabled;
            //FI 20121214 [Recette de 3.0 RTM] Spheres® permet d'abandonner à n'importe quel moment
            //Même si dans la réalité économique cena n'a pas de sens 
            //On peut tout de même penser que compte veuille liquider ses positions options pour des raisons stratégiques
            //if  (exeAssAbnOption.maturity != null)
            //    isAbandonRemainingEnabled = (exeAssAbnOption.maturity.DateValue == exeAssAbnOption.date.DateValue);
            //
            if (!cciQuantity.HasError)
            {
                Ccis.Set(CciClientId(quantity), "isEnabled", isEnabled);
                Ccis.Set(CciClientId(note), "isEnabled", isEnabled);
                Ccis.Set(CciClientId(abandonRemaining), "isEnabled", isAbandonRemainingEnabled);
            }

            // FI 20130314[] les frais ne sont pas disponible en mode Reset
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            bool isReset = exeAssAbnOption.prevQuantity > 0 && exeAssAbnOption.quantity.LongValue == 0;
            foreach (CciPayment cciOpp in cciOtherPartyPayment)
                Ccis.Set(cciOpp.CciClientId(CciPayment.CciEnum.payer), "isEnabled", (false == isReset));     

            string physicalFactor = CciTools.GetFieldVariableName(new { exeAssAbnOption.physicalFactor }, typeof(ExeAssAbnOption));
            bool physicalFactorEnabled = exeAssAbnOption.settlementMethodEnum == SettlMethodEnum.PhysicalSettlement;
            Ccis.Set(CciClientId(physicalFactor), "isEnabled", physicalFactorEnabled);

            //otherPartyPayment
            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].RefreshCciEnabled();
            
            string requestMode = CciTools.GetFieldVariableName(new { exeAssAbnOption.requestMode }, typeof(ExeAssAbnOption));
            Ccis.Set(CciClientId(requestMode), "isEnabled", exeAssAbnOption.prevRequestMode != SettlSessIDEnum.Intraday);
        
        }

        public void SetDisplay(CustomCaptureInfo pCci)
        {


        }

        public void Initialize_Document()
        {

        }

        #endregion

        #region IContainerCci Membres

        /// <summary>
        /// Concatenate the current container name (identified by the current ModeEnum) to the input key, producing the 
        /// complete Cci key
        /// </summary>
        /// <param name="pClientId_Key">the input key to suffix at the container name</param>
        /// <returns>the total ClientId</returns>
        public string CciClientId(string pClientId_Key)
        {
            return String.Concat(CciExeAssAbnOption.Prefix, CustomObject.KEY_SEPARATOR, pClientId_Key);
        }

        /// <summary>
        /// Get the Cci relative to the input key pClientId_Key and the actual container
        /// </summary>
        /// <param name="pClientId_Key">the client id without the container prefix </param>
        /// <returns>the Cci relative to the input key if the key is part of the container; otherwise null</returns>
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            string completeKey = this.CciClientId(pClientId_Key);

            if (IsCciOfContainer(completeKey))
                return Ccis[completeKey];
            else
                return null;
        }

        /// <summary>
        /// Verify if the input key is part of this Cci
        /// </summary>
        /// <param name="pClientId_WithoutPrefix">the Cci key including the container prefix</param>
        /// <returns>true when the input key is part of the container </returns>
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            bool isOk = false;
            //FI 20110418 [17405] ccis.Contains en commentaire => Tuning
            //isOk = ccis.Contains(pClientId_WithoutPrefix);
            //isOk = isOk && (pClientId_WithoutPrefix.StartsWith(Prefix));	
            isOk = (pClientId_WithoutPrefix.StartsWith(Prefix));
            return isOk;
        }

        /// <summary>
        /// Get the container key of the input string, IFF the input key is part of the container
        /// </summary>
        /// <param name="pClientId_WithoutPrefix">A Cci key</param>
        /// <returns>the container prefix when the key is part of the container, null otherwise</returns>
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            if (IsCciOfContainer(pClientId_WithoutPrefix))
                return CciExeAssAbnOption.Prefix;
            else
                return null;
        }

        #endregion

        #region IContainerCciPayerReceiver Membres

        public string CciClientIdPayer
        {
            get { throw new NotImplementedException(); }
        }

        public string CciClientIdReceiver
        {
            get { throw new NotImplementedException(); }
        }

        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < otherPartyPaymentLength; i++)
                _cciOtherPartyPayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);

        }

        private void InitializeOtherPartyPayment_FromCci()
        {

            ArrayList lst = new ArrayList();
            bool isOk = true;
            int index = -1;
            while (isOk)
            {
                index += 1;
                //
                string date =
                    CciTools.GetFieldVariableName(new { exeAssAbnOption.date }, typeof(ExeAssAbnOption));

                CciPayment cciPayment = new CciPayment(_cciTrade, index + 1, null, CciPayment.PaymentTypeEnum.Payment, Prefix + CustomObject.KEY_SEPARATOR + "otherPartyPayment", string.Empty, string.Empty, string.Empty, _cciTrade.CciClientIdMainCurrency, CciClientId(date));
                //
                isOk = _ccis.Contains(cciPayment.CciClientId(CciPayment.CciEnumPayment.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_exeAssAbnOption.otherPartyPayment) || (index == _exeAssAbnOption.otherPartyPayment.Length))
                        ReflectionTools.AddItemInArray(_exeAssAbnOption, "otherPartyPayment", index);

                    cciPayment.Payment = _exeAssAbnOption.otherPartyPayment[index];
                    //
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
        #endregion

        #region IContainerCciGetInfoButton Membres

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public  bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            for (int i = 0; i < otherPartyPaymentLength; i++)
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {

            SetMoneyPositionForColor(pPage);

            for (int i = 0; i < _cciOtherPartyPayment.Length; i++)
            {
                if (cciOtherPartyPayment[i].IsSpecified)
                    cciOtherPartyPayment[i].DumpSpecific_ToGUI(pPage);
            }

            //FI 20130314 [] quelle idée de ne plus afficher les frais, ce n'est pas ds l'esprit de Spheres®
            //En plus la calculette reste visible 
            //string clientidControlOpp = String.Concat(Prefix, CustomObject.KEY_SEPARATOR, "tblExeAssPartyPaymentBlock");
            //Control ControlOpp = (Control)pPage.FindControl(clientidControlOpp);
            //if (ControlOpp != null)
            //{
            //    bool canReset = exeAssAbnOption.prevQuantity > 0 && exeAssAbnOption.quantity.IntValue == 0;
            //    ControlOpp.Visible = !canReset;
            //}

            AddUnderlyerLink(pPage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        private void AddUnderlyerLink(CciPageBase pPage)
        {

            //FI 20130307 add links on underlyer
            string underlyerCategory = CciTools.GetFieldVariableName(new { exeAssAbnOption.underlyerCategory }, typeof(ExeAssAbnOption));
            string identifierUnderlyer = CciTools.GetFieldVariableName(new { exeAssAbnOption.identifierUnderlyer }, typeof(ExeAssAbnOption));

            //Il restera à prévoir les autres type de ssjacent
            switch (exeAssAbnOption.underlyingAssetCategoryEnum)
            {
                case Cst.UnderlyingAsset.Future:
                case Cst.UnderlyingAsset.ExchangeTradedContract:
                    if (null != Cci(underlyerCategory) && (null != Cci(underlyerCategory).sql_Table))
                        pPage.SetOpenFormReferential(Cci(underlyerCategory), Cst.OTCml_TBL.DERIVATIVECONTRACT);

                    if (null != Cci(identifierUnderlyer))
                        pPage.SetOpenFormReferential(Cci(identifierUnderlyer), Cst.OTCml_TBL.ASSET_ETD);
                    break;

                case Cst.UnderlyingAsset.Commodity:
                case Cst.UnderlyingAsset.Index:
                case Cst.UnderlyingAsset.RateIndex:
                case Cst.UnderlyingAsset.EquityAsset:
                case Cst.UnderlyingAsset.ExchangeTradedFund:
                    Cst.OTCml_TBL tbl = AssetTools.ConvertUnderlyingAssetToTBL(exeAssAbnOption.underlyingAssetCategoryEnum);
                    if (null != Cci(identifierUnderlyer))
                        pPage.SetOpenFormReferential(Cci(identifierUnderlyer), tbl);
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

            string moneyPosition = CciTools.GetFieldVariableName(new { exeAssAbnOption.moneyPosition }, typeof(ExeAssAbnOption));

            CustomCaptureInfo cciMoneyPosition = this.Cci(moneyPosition);
            Control controlMoneyPosition = (Control)pPage.FindControl(cciMoneyPosition.clientId);
            if (controlMoneyPosition is TextBox)
            {
                TextBox txtBoxlMoneyPosition = (TextBox)controlMoneyPosition;

                switch (exeAssAbnOption.moneyPositionEnum)
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
        #endregion
    }
}

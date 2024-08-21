#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Book;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CheckValidationRuleBase
    {
        /// <summary>
        /// 
        /// </summary>
        public enum CheckModeEnum
        {
            /// <summary>
            /// Contrôle des erreurs critique uniquement
            /// </summary>
            ErrorCritical,
            /// <summary>
            /// Contrôle des erreurs uniquement (lié au paramétrage instrument)
            /// </summary>
            Error,
            /// <summary>
            /// Contrôle des warning uniquement (paramétrage instrument)
            /// </summary>
            Warning,
        }

        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected Hashtable m_CheckConformity;
        /// <summary>
        /// 
        /// </summary>
        protected CheckModeEnum m_CheckMode;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient true si contrôle des erreurs uniquements
        /// </summary>
        public bool IsCheckError
        {
            get
            {
                return (CheckModeEnum.Error == m_CheckMode) || (CheckModeEnum.ErrorCritical == m_CheckMode);
            }
        }
        /// <summary>
        /// Obtient true si contrôle des warning uniquement
        /// </summary>
        public bool IsCheckWarning
        {
            get { return CheckModeEnum.Warning == m_CheckMode; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public CheckValidationRuleBase()
        {
            m_CheckConformity = new Hashtable();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsExceptDec"></param>
        /// <param name="pAmount1"></param>
        /// <param name="pAmount2"></param>
        /// <returns></returns>
        protected static bool CompareAmount(bool pIsExceptDec, decimal pAmount1, decimal pAmount2)
        {
            bool ret;
            if (pIsExceptDec)
                ret = (Decimal.Truncate(pAmount1) == Decimal.Truncate(pAmount2));
            else
                ret = (pAmount1 == pAmount2);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="opExchangeAmountRounded"></param>
        /// <param name="pIsFxCtrvExceptDec"></param>
        /// <param name="pQuoteBasis"></param>
        /// <param name="pFxRate"></param>
        /// <param name="pAmount1"></param>
        /// <param name="pCur1"></param>
        /// <param name="pAmount2"></param>
        /// <param name="pCur2"></param>
        /// <returns></returns>
        protected bool CompareAmount(string pCS, out decimal opExchangeAmountRounded, bool pIsFxCtrvExceptDec, QuoteBasisEnum pQuoteBasis, decimal pFxRate, decimal pAmount1, string pCur1, decimal pAmount2, string pCur2)
        {
            opExchangeAmountRounded = 0;
            EFS_Cash cash;
            //Contrôle si contrevaleur du Amount1 est égale à Amount2
            cash = new EFS_Cash(CSTools.SetCacheOn(pCS), pCur1, pCur2, pAmount1, pFxRate, pQuoteBasis);
            //Note: En fonction du sens où elle est opérée une contrevaleur peut différer de 1 centimes, on testera alors en sens inverse.
            bool ret;
            if (pIsFxCtrvExceptDec)
                ret = (Decimal.Truncate(cash.ExchangeAmountRounded) == Decimal.Truncate(pAmount2));
            else
                ret = (cash.ExchangeAmountRounded == pAmount2);

            if (!ret)
            {
                //Contrôle si contrevaleur du Amount2 est égale à Amount1
                cash = new EFS_Cash(CSTools.SetCacheOn(pCS), pCur2, pCur1, pAmount2, pFxRate, pQuoteBasis == QuoteBasisEnum.Currency1PerCurrency2 ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2);
                if (pIsFxCtrvExceptDec)
                    ret = (Decimal.Truncate(cash.ExchangeAmountRounded) == Decimal.Truncate(pAmount1));
                else
                    ret = (cash.ExchangeAmountRounded == pAmount1);
            }

            if (!ret)
                opExchangeAmountRounded = cash.ExchangeAmountRounded;
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        protected virtual Cst.CheckModeEnum GetCheckMode(string pKey)
        {
            return Cst.CheckModeEnum.Error;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetConformityMsg()
        {

            StrBuilder ret = new StrBuilder();
            if (ArrFunc.IsFilled(m_CheckConformity))
            {
                Array targetArray = Array.CreateInstance(typeof(String), m_CheckConformity.Count);
                m_CheckConformity.Values.CopyTo(targetArray, 0);
                string[] arrMsg = (string[])targetArray;
                for (int i = 0; i < arrMsg.Length; i++)
                    ret += arrMsg[i] + Cst.CrLf + Cst.CrLf;
            }
            return ret.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        protected static string GetIdAdditionalInfo(string pId)
        {
            string ret = String.Empty;
            if (StrFunc.IsFilled(pId))
                ret = "(" + Cst.Space + "id:" + Cst.Space + pId + Cst.Space + ")";
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        protected bool IsToCheck(string pKey)
        {
            return (m_CheckMode.ToString().ToUpper() == GetCheckMode(pKey).ToString().ToUpper());
        }
        /// <summary>
        /// Ajoute un item dans la collection des messages
        /// </summary>
        /// <param name="pRes">ressoure</param>
        protected void SetValidationRuleError(string pRes)
        {
            SetValidationRuleError(pRes, null, null);
        }
        /// <summary>
        /// Ajoute un item dans la collection des messages 
        /// </summary>
        /// <param name="pRes">ressoure</param>
        /// <param name="pAdditionalInfo">Info supplementaire</param>
        protected void SetValidationRuleError(string pRes, string pAdditionalInfo)
        {
            SetValidationRuleError(pRes, pAdditionalInfo, null);
        }
        /// <summary>
        /// Ajoute un item dans la collection des messages 
        /// </summary>
        /// <param name="pRes">ressoure</param>
        /// <param name="pItems">liste des paramètres de la ressource</param>
        protected void SetValidationRuleError(string pRes, string[] pItems)
        {
            SetValidationRuleError(pRes, null, pItems);
        }
        /// <summary>
        /// Ajoute un item dans la collection des messages 
        /// </summary>
        /// <param name="pRes">ressoure</param>
        /// <param name="pAdditionalInfo">info supplémentaire</param>
        /// <param name="pItems">liste des paramètres de la ressource</param>
        protected void SetValidationRuleError(string pRes, string pAdditionalInfo, string[] pItems)
        {
            string res;
            if (ArrFunc.IsEmpty(pItems))
                res = Ressource.GetString(pRes, true);
            else
                res = Ressource.GetString2(pRes, true, pItems);

            res = res + Cst.Space + pAdditionalInfo;
            if (false == m_CheckConformity.Contains(res))
                m_CheckConformity.Add(res, "-" + Cst.Space + res);
        }

        /// <summary>
        /// Contrôles des règles de validité
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCheckMode"></param>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        public virtual bool ValidationRules(string pCS, IDbTransaction pDbTransaction, CheckModeEnum pCheckMode)
        {
            m_CheckMode = pCheckMode;
            m_CheckConformity = new Hashtable(); 

            return ArrFunc.IsEmpty(m_CheckConformity);
        }
        #endregion Methods
    }


    /// <summary>
    /// 
    /// </summary>
    public abstract class CheckTradeValidationRuleBase : CheckValidationRuleBase
    {

        #region Members

        /// <summary>
        /// Représente l'instrument qui pilote la validation
        /// </summary>
        private readonly SQL_Instrument _sqlInstrument;
        /// <summary>
        /// Représente le type de saisie
        /// </summary>
        private readonly Cst.Capture.ModeEnum _captureMode;

        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient l'instrument qui pilote la validation
        /// </summary>
        public SQL_Instrument SqlInstrument
        {
            get { return _sqlInstrument; }
        }
        /// <summary>
        /// Obtient le type de saisie
        /// </summary>
        public Cst.Capture.ModeEnum CaptureMode
        {
            get { return _captureMode; }
        }
        #endregion Accessors

        #region Constructors
        public CheckTradeValidationRuleBase(SQL_Instrument pSQLInstrument, Cst.Capture.ModeEnum pCaptureModeEnum)
            : base()
        {
            _captureMode = pCaptureModeEnum;
            _sqlInstrument = pSQLInstrument;
        }
        #endregion Constructors

        #region Method
        /// <summary>
        /// Retourne le type de contrôle définit par la colonne {pKey}
        /// </summary>
        /// <param name="pKey">Représente la colonne dans la table Instrument</param>
        /// <returns></returns>
        protected override Cst.CheckModeEnum GetCheckMode(string pKey)
        {
            return _sqlInstrument.GetFirstRowColumnCheckMode(pKey);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    // EG 20171115 Add _sessionInfo
    public abstract class CheckTradeInputValidationRuleBase : CheckTradeValidationRuleBase
    {
        #region Accessors
        public User User
        {
            private set;
            get;
        }
        public TradeCommonInput TradeInput
        {
            private set;
            get;
        }
        #endregion Accessors
        #region constructor
        public CheckTradeInputValidationRuleBase(TradeCommonInput pTradeInput, Cst.Capture.ModeEnum pCaptureMode, User pUser)
            : base(pTradeInput.SQLInstrument, pCaptureMode)
        {
            TradeInput = pTradeInput;
            User = pUser;
        }
        #endregion

                

        /// <summary>
        ///  Vérifie l'existence de au moins un book managé
        /// </summary>
        /// FI 20150730 [21156] Add Method 
        // EG 20180205 [23769] Add dbTransaction  
        protected void CheckValidationRule_BookManaged(string pCs, IDbTransaction pDbTransaction)
        {
            if (IsCheckError)
            {
                int count_ManagedBook = 0;

                foreach (IParty party in TradeInput.DataDocument.Party)
                {
                    if (TradeInput.DataDocument.IsPartyCounterParty(party))
                    {
                        IPartyTradeIdentifier partyTradeIdentifier = TradeInput.DataDocument.GetPartyTradeIdentifier(party.Id);
                        if (null != partyTradeIdentifier)
                        {
                            if (partyTradeIdentifier.BookIdSpecified && partyTradeIdentifier.BookId.OTCmlId > 0)
                            {
                                Boolean isBookManaged = BookTools.IsBookManaged(pCs, pDbTransaction, partyTradeIdentifier.BookId.OTCmlId);

                                count_ManagedBook += isBookManaged ? 1 : 0;
                            }
                        }
                    }
                }

                if (count_ManagedBook < 1)
                    SetValidationRuleError("Msg_ValidationRule_Book_IsAvailable");
            }
        }

        /// <summary>
        /// Contrôle des certains objets EfsML
        /// </summary>
        protected void CheckValidationRule_Element()
        {

            ArrayList list = null;
            DataDocumentContainer dataDocument = TradeInput.DataDocument;
            ProductContainer product = TradeInput.DataDocument.CurrentProduct;
            if (IsCheckError)
            {
                #region Payments
                // Les payment sont-ils corrects ???
                if (false == product.IsFx)
                    list = ReflectionTools.GetObjectsByType2(TradeInput.DataDocument.CurrentTrade, product.ProductBase.TypeofPayment, false);
                else if (null != dataDocument.OtherPartyPayment)
                    list = new ArrayList(dataDocument.OtherPartyPayment);

                if (null != list)
                {
                    IPayment[] payment = (IPayment[])list.ToArray(product.ProductBase.TypeofPayment);
                    for (int i = 0; i < payment.Length; i++)
                    {
                        if ((false == payment[i].AdjustedPaymentDateSpecified) && (false == payment[i].PaymentDateSpecified))
                            SetValidationRuleError("Msg_ValidationRule_PaymentdateInvalid");
                    }
                }
                #endregion Payments
            }
            //
            list = ReflectionTools.GetObjectsByType(TradeInput.DataDocument.CurrentTrade, product.ProductBase.TypeofSchedule, true);
            //
            if (ArrFunc.IsFilled(list))
            {
                ISchedule[] schedule = (ISchedule[])list.ToArray(product.ProductBase.TypeofSchedule);
                for (int i = 0; i < schedule.Length; i++)
                {
                    if (schedule[i].StepSpecified && ArrFunc.IsFilled(schedule[i].Step))
                    {
                        IStep[] step = schedule[i].Step;
                        if (IsCheckWarning)
                        {
                            DateTime dtLastItem = schedule[i].Step[0].StepDate.DateValue;
                            for (int j = 1; j < step.Length; j++)
                            {
                                if (false == (step[j].StepDate.DateValue.CompareTo(dtLastItem) > 0))
                                {
                                    SetValidationRuleError("Msg_ValidationRule_StepNotInOrder", GetIdAdditionalInfo(schedule[i].Id));
                                    break;
                                }
                                dtLastItem = step[j].StepDate.DateValue;
                            }
                        }
                        else if (IsCheckError)
                        {
                            DateTime[] dates = schedule[i].GetStepDatesValue;
                            for (int j = 1; j < step.Length; j++)
                            {
                                if (ArrFunc.CountNbOf(dates, step[j].StepDate.DateValue) > 1)
                                {
                                    SetValidationRuleError("Msg_ValidationRule_IdenticStepDate", GetIdAdditionalInfo(schedule[i].Id), new string[] { DtFunc.DateTimeToString(step[j].StepDate.DateValue, DtFunc.FmtShortDate) });
                                    break;
                                }
                            }
                        }

                    }
                }
            }
        }


        // EG 20150907 [21317] Refactoring = Messages complémentaires
        protected void CheckActorBuyerSeller()
        {
            if (IsCheckError)
            {
                Dictionary<string, BuyerSellerEnum> dicParty = null;
                foreach (IParty party in TradeInput.DataDocument.Party)
                {

                    if (TradeInput.DataDocument.IsPartyBuyer(party) || TradeInput.DataDocument.IsPartySeller(party))
                    {
                        if (null == dicParty)
                            dicParty = new Dictionary<string,BuyerSellerEnum>();

                        if (false == dicParty.ContainsKey(party.Id))
                            dicParty.Add(party.Id,
                                TradeInput.DataDocument.IsPartyBuyer(party)?BuyerSellerEnum.BUYER:BuyerSellerEnum.SELLER);
                    }
                }


                if (null != dicParty)
                {
                    if (false == dicParty.ContainsValue(BuyerSellerEnum.BUYER))
                        SetValidationRuleError("Msg_ValidationRule_BuyerIncorrectOrMissing");

                    if (false == dicParty.ContainsValue(BuyerSellerEnum.SELLER))
                        SetValidationRuleError("Msg_ValidationRule_SellerIncorrectOrMissing");

                    /*
                    BuyerSellerEnum[] targetArray = new BuyerSellerEnum[dicParty.Count];
                    dicParty.Values.CopyTo(targetArray, 0);

                    if (ArrFunc.CountNbOf(targetArray, BuyerSellerEnum.BUYER) !=
                        ArrFunc.CountNbOf(targetArray, BuyerSellerEnum.SELLER))
                        SetValidationRuleError("Msg_ValidationRule_TradeIdenticBuyerSeller");
                    */
                }

            }
        }

        /// <summary>
        ///  Contrôle qu'il n'existe aucune partie inconnue (party.OTCmlId &lt; 0)  
        /// </summary>
        /// FI 20140404 [19821] Add Method
        /// EG 202207010 [XXXXX] OTCmlId peut être négatif [Corrections Diverses (Demo OTC/BFF)]
        protected void CheckValidationRule_Party()
        {
            if (IsCheckError)
            {
                string partyUnknonw = string.Empty;
                foreach (IParty party in TradeInput.DataDocument.Party)
                {
                    if (party.OTCmlId == 0)
                    {
                        if (StrFunc.IsFilled(partyUnknonw))
                            partyUnknonw += ",";
                        partyUnknonw += StrFunc.IsFilled(party.PartyName) ? party.PartyName : party.PartyId;
                    }
                }
                if (StrFunc.IsFilled(partyUnknonw))
                    SetValidationRuleError("Msg_ValidationRule_party_PartyUnknown", null, new string[] { partyUnknonw });
            }
        }
    }
}

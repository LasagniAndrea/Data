using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
using System.Linq;
//PasGlop Faire un search dans la solution de "TODO FEEMATRIX"

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public class FeeResponse
    {
        #region Members
        private EventCodeEnum _eventCode;
        private IPayment[] _payment;
        private readonly FeeProcessing _parent;
        private LevelStatusTools.LevelEnum _levelEnum;
        private LevelStatusTools.StatusEnum _statusEnum;
        private string _auditMessage;
        private string _errorMessage;
        private string _infoMessage;
        private FeeMatrix _feeMatrix;
        private FeeMatrix _feeMatrixOriginalValues;
        private DataTable _dtBracket1;
        private bool _isBracket1Application_Cumulative;
        private bool _isBracket1Application_Periodic;
        // EG 20150713 Report PL
        private bool _isBracket1_NoBracket;
        private decimal _periodBasisValue = 0;
        private string _periodCharacteristics;
        private int _currentBraket = -1;
        #endregion Members
        //
        #region Accessors
        public EventCodeEnum EventCode
        {
            get { return _eventCode; }
        }
        public IPayment[] Payment
        {
            get { return _payment; }
        }
        public bool PaymentSpecified
        {
            get { return (_payment != null); }
        }
        public string AuditMessage
        {
            get { return _auditMessage; }
            set { _auditMessage = value; }
        }
        public bool AuditMessageSpecified
        {
            get { return StrFunc.IsFilled(_auditMessage); }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
        public bool ErrorMessageSpecified
        {
            get { return StrFunc.IsFilled(_errorMessage); }
        }
        public string InfoMessage
        {
            get { return _infoMessage; }
        }
        public bool InfoMessageSpecified
        {
            get { return StrFunc.IsFilled(_infoMessage); }
        }
        public LevelStatusTools.LevelEnum LevelEnum
        {
            get { return _levelEnum; }
        }
        public LevelStatusTools.StatusEnum StatusEnum
        {
            get { return _statusEnum; }
        }
        public FeeProcessing Parent
        {
            get { return _parent; }
        }
        #endregion Accessors
        //
        #region Constructors
        public FeeResponse(FeeProcessing pFeeProcessing)
        {
            _eventCode = EventCodeEnum.OPP;
            _parent = pFeeProcessing;
        }
        #endregion Constructors
        //
        #region Methods
        #region public Calc
        // EG 20130911 Add pDbTransaction = Gestion mode Transactionel (exemple : Appel via EOD TradeMerge)
        // EG 20171025 [23509] Add DTEXECUTION
        // EG 20180205 [23769] Upd DataHelper.ExecuteReader
        // EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        // EG 20180307 [23769] Gestion dbTransaction
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // EG 20220816 [XXXXX] [WI396] Split : Barème dégressif non appliqué
        public void Calc(string pCS, IDbTransaction pDbTransaction, FeeMatrix pFeeMatrix)
        {
            _feeMatrix = pFeeMatrix;
            string headerMessage = _feeMatrix.FEE_PAYMENTTYPE + @": " + _feeMatrix.FEE_IDENTIFIER + @" - " + _feeMatrix.FEESCHED_IDENTIFIER;
            SaveFeeMatrixOriginalValues();
            //
            #region Variable
            const string CALCAMOUNT = "[CalcAmount: {0}]";
            const string CALCAMOUNT_Begin = "[CalcAmount:";

            string stringFormulaBasisValue = string.Empty;
            string stringFormulaBracket1Value = BracketApplicationEnum.Unit.ToString();
            string[] stringFormulaValue1Value = new string[1];
            string[] stringFormulaValue2Value = new string[1];
            string stringDCFValue = string.Empty;
            string[] stringFormulaMinValue = new string[1];
            string[] stringFormulaMaxValue = new string[1];
            stringFormulaMinValue[0] = stringFormulaMaxValue[0] = Cst.NotAvailable;
            string stringInvoicingAValue = string.Empty;
            string stringInvoicingBValue = string.Empty;

            string errorMessage = string.Empty;
            string exchangeMessage = string.Empty;
            decimal fee1AssessmentBasisDecValue = 0;
            string fee1AssessmentBasisStrValue = string.Empty;
            decimal fee2AssessmentBasisDecValue = 0;
            bool fee2AssessmentBasisSpecified = false;
            string fee2AssessmentBasisStrValue = string.Empty;
            bool isOk = true;
            //bool isSpecificScheduleDiscard = false; //PL 20130508 

            decimal feeValue = 0;
            string feeCurrency;
            DateTime feeFirstDate = DateTime.MinValue;
            PeriodEnum period = PeriodEnum.T;
            IInterval interval = Parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateInterval(period, 1);
            RollConventionEnum rollConvention = RollConventionEnum.NONE;

            bool isBracket1 = false;
            _isBracket1Application_Cumulative = false;
            _isBracket1Application_Periodic = false;
            // EG 20150713 Report PL
            _isBracket1_NoBracket = false;
            _periodBasisValue = 0;

            //PL 20191210 [25099] 
            bool isFeeScope_OrderId = (_feeMatrix.FEESCHED_SCOPE == Cst.FeeScopeEnum.OrderId.ToString());
            bool isFeeScope_FolderId = (_feeMatrix.FEESCHED_SCOPE == Cst.FeeScopeEnum.FolderId.ToString());
            
            bool isFeePeriodic = false;
            bool isFeeInvoiced = _feeMatrix.ISINVOICINGSpecified && _feeMatrix.ISINVOICING;
            bool isFirstPeriodExcluded = false;
            bool isLastPeriodExcluded = false;
            int countPayment = 1;
            EFS_Period[] periods = new EFS_Period[1];
            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();

            _errorMessage = null;
            _infoMessage = null;
            _levelEnum = LevelStatusTools.LevelEnum.NA;
            _statusEnum = LevelStatusTools.StatusEnum.NA;

            bool isNewTrade = Cst.Capture.IsModeNewCapture(Parent.FeeRequest.Mode);
            #endregion Variable

            #region IDC Schedule
            if (!_feeMatrix.FEESCHED_IDCSpecified)
            {
                //Cas d'un barème exprimé en devise du trade.
                decimal tmp = 0;
                string scheduleCurrency = null;
                isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.Currency1Amount, ref tmp, ref scheduleCurrency, ref errorMessage);
                if (!isOk)
                {
                    errorMessage = "Undefined currency schedule: " + errorMessage;
                }
                else
                {
                    _feeMatrix.FEESCHED_IDCSpecified = true;
                    _feeMatrix.FEESCHED_IDC = scheduleCurrency;
                }
            }
            #endregion

            #region Payer/Receiver
            _feeMatrix.CommentForDebug = "Check Payer/Receiver";
            //PL 2013030 partyPayer et partyReceiver sont maintenant des membres de FeeMatrix.
            //           Dans le cas de barèmes spécifiques ils sont valorisés dans CheckFeeMatrixOnSpecificSchedule2(...)
            if (!_feeMatrix.FEESCHED_IDASpecified)
            {
                _feeMatrix.PartyPayer = Parent.GetPartyPayerReceiver(_feeMatrix.FEEPAYER, _feeMatrix.IsReverseParty, _feeMatrix.MatchedOtherParty1Index, _feeMatrix.MatchedOtherParty2Index);
                _feeMatrix.PartyReceiver = Parent.GetPartyPayerReceiver(_feeMatrix.FEERECEIVER, _feeMatrix.IsReverseParty, _feeMatrix.MatchedOtherParty1Index, _feeMatrix.MatchedOtherParty2Index);
            }
            //
            if ((_feeMatrix.PartyPayer == null) || (StrFunc.IsEmpty(_feeMatrix.PartyPayer.m_Party_Href)))
            {
                isOk = false;
                errorMessage = "Undefined payer: the payer specified does not exist on the trade.";
            }
            else if ((_feeMatrix.PartyReceiver == null) || (StrFunc.IsEmpty(_feeMatrix.PartyReceiver.m_Party_Href)))
            {
                isOk = false;
                errorMessage = "Undefined receiver: the receiver specified does not exist on the trade.";
            }
            //PL 20130508 *******************************************************************************************************
            //else if (_feeMatrix.FEESCHED_IDASpecified && (_feeMatrix.FEESCHED_IDA>0))
            //{
            //    if ((_feeMatrix.FEESCHED_IDA != _feeMatrix.PartyPayer.m_Party_Ida) && (_feeMatrix.FEESCHED_IDA != _feeMatrix.PartyReceiver.m_Party_Ida))
            //    {
            //        //Ni le Payer ni le Receiver n'est le propriétaire du barème dérogatoire
            //        isSpecificScheduleDiscard = true;
            //        isOk = false;
            //        errorMessage = "Payer and Receiver are not schedule owners!";
            //    }
            //}
            //PL 20130508 *******************************************************************************************************
            #endregion Payer/Receiver

            #region EventCode
            _eventCode = EventCodeEnum.OPP;
            if (isOk)
            {
                if (StrFunc.IsFilled(_feeMatrix.FEE_EVENTCODE))
                {
                    _eventCode = (EventCodeEnum)Enum.Parse(typeof(EventCodeEnum), _feeMatrix.FEE_EVENTCODE);
                }
                else
                {
                    //Déduction à partir des Payer\receiver
                    if (Parent.FeeRequest.PartyA != null && Parent.FeeRequest.PartyB != null)
                    {
                        if (Parent.IsProductWithADP())
                        {
                            if (
                                ((_feeMatrix.PartyPayer.m_Party_Href == Parent.FeeRequest.PartyA.m_Party_Href)
                                  && (_feeMatrix.PartyReceiver.m_Party_Href == Parent.FeeRequest.PartyB.m_Party_Href))
                                ||
                                ((_feeMatrix.PartyPayer.m_Party_Href == Parent.FeeRequest.PartyB.m_Party_Href)
                                  && (_feeMatrix.PartyReceiver.m_Party_Href == Parent.FeeRequest.PartyA.m_Party_Href))
                                )
                                _eventCode = EventCodeEnum.ADP;
                        }
                    }
                }
            }
            #endregion EventCode

            #region Trade Dates
            if (isOk)
            {
                //FI 20091223 [16471] Appel à DataDocument.GetStartAndEndDates
                Parent.FeeRequest.DataDocument.GetStartAndEndDates(pCS, false, out  dtStart, out dtEnd);
                isOk = DtFunc.IsDateTimeFilled(dtStart) && DtFunc.IsDateTimeFilled(dtEnd);
                if (!isOk)
                    errorMessage = "Undefined date: the effective or termination date is empty on the trade.";
            }
            #endregion Trade Dates

            #region Frequency/Date
            if (isOk)
            {
                #region Period
                if (_feeMatrix.PERIODMLTPSpecified && _feeMatrix.PERIODSpecified)
                {
                    isFeePeriodic = (_feeMatrix.PERIOD != FpML.Enum.PeriodEnum.T.ToString());
                    if (isFeePeriodic)
                    {
                        //Frais périodiques
                        isFeePeriodic = true;
                        period = (PeriodEnum)Enum.Parse(typeof(PeriodEnum), _feeMatrix.PERIOD);
                        interval = Parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateInterval(period, _feeMatrix.PERIODMLTP);
                        //
                        stringInvoicingAValue = "[Period: " + interval.PeriodMultiplier.ToString() + " " + interval.Period.ToString() + "]";
                    }
                }
                #endregion Period
                #region RollConvention
                if (_feeMatrix.ROLLCONVENTIONSpecified)
                {
                    rollConvention = EfsML.Enum.Tools.StringToEnum.RollConvention(_feeMatrix.ROLLCONVENTION);
                    stringInvoicingAValue = "[Roll: " + rollConvention.ToString() + "]";
                }
                #endregion RollConvention
                #region RelativeTo
                if (isOk)
                {
                    feeFirstDate = Parent.FeeRequest.DtReference;
                    //
                    // RD 20110429 / Correction du calcul de la date de référence (date de règlement)
                    // EG 20150708 [21103] Add IsSafekeepinAction
                    if (false == Parent.FeeRequest.IsSafekeeping)
                    {
                        if (_feeMatrix.RELATIVETOSpecified)
                        {
                            #region RELATIVETOSpecified
                            // La date relative est celle spécifiée sur la condition d’application du barème
                            stringInvoicingBValue = "[RelativeTo: " + _feeMatrix.RELATIVETO + "]";

                            if (Parent.FeeRequest.IsFeeOptionSettlementAction)
                            {
                                isOk = ((_feeMatrix.RELATIVETO == FeePayRelativeToEnum.BusinessDate.ToString()) ||
                                        (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.NextBusinessDate.ToString()) ||
                                        (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.DeliveryDate.ToString()));

                                if (isOk)
                                {
                                    if (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.NextBusinessDate.ToString())
                                    {
                                        // NextBusinessDate correspond à la date de l'action + 1JO sur le BC du Market du DC
                                        DateTime dtNextBusinessDate = Parent.FeeRequest.Product.GetNextBusinessDate(pCS, pDbTransaction, Parent.FeeRequest.DtReference);
                                        if (DtFunc.IsDateTimeFilled(dtNextBusinessDate))
                                            feeFirstDate = dtNextBusinessDate;
                                    }
                                    else if (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.DeliveryDate.ToString())
                                    {
                                        // 20140515 PL TRIM 19948
                                        // DeliveryDate correspond à la date du Jour de Règlement, Livraison, Liquidation ou Compensation
                                        DateTime dtDeliveryDate = GetDeliveryDate(pCS, pDbTransaction);
                                        if (DtFunc.IsDateTimeFilled(dtDeliveryDate))
                                            feeFirstDate = dtDeliveryDate;
                                    }
                                }
                                else
                                {
                                    errorMessage = "RelativeTo date invalid: the specified RelativeTo date is incorrect for action.";
                                }
                            }
                            else
                            {
                                if ((_feeMatrix.RELATIVETO == FeePayRelativeToEnum.ValueDate.ToString())
                                    || (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.EffectiveDate.ToString())
                                    || (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.BusinessDate.ToString()))
                                {
                                    feeFirstDate = dtStart;
                                }
                                else if (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.NextBusinessDate.ToString())
                                {
                                    // RD 20110429 [17440]
                                    // NextBusinessDate correspond à la date de compensation + 1JO sur le BC du Market du DC
                                    DateTime dtNextBusinessDate = Parent.FeeRequest.Product.GetNextBusinessDate(pCS, pDbTransaction);
                                    if (DtFunc.IsDateTimeFilled(dtNextBusinessDate))
                                        feeFirstDate = dtNextBusinessDate;
                                }
                                else if (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.DeliveryDate.ToString())
                                {
                                    // 20140515 PL TRIM 19948
                                    // DeliveryDate correspond à la date du Jour de Règlement, Livraison, Liquidation ou Compensation
                                    DateTime dtDeliveryDate = GetDeliveryDate(pCS, pDbTransaction);
                                    if (DtFunc.IsDateTimeFilled(dtDeliveryDate))
                                        feeFirstDate = dtDeliveryDate;
                                }
                                else if (_feeMatrix.RELATIVETO == FeePayRelativeToEnum.TerminationDate.ToString())
                                {
                                    feeFirstDate = dtEnd;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            // Par défaut la date de règlement est:
                            // - BusinessDate pour les trades ETD
                            // - TransactDate pour les trades NON ETD
                            if ((Parent.FeeRequest.Product.IsExchangeTradedDerivative) || (Parent.FeeRequest.Product.IsStrategy))
                                stringInvoicingBValue = "[RelativeTo: " + FeePayRelativeToEnum.BusinessDate.ToString() + "]";
                            else
                                stringInvoicingBValue = "[RelativeTo: " + FeePayRelativeToEnum.TransactDate.ToString() + "]";
                        }
                    }
                }
                #endregion RelativeTo
                #region Periods dates
                if (isOk)
                {
                    if (isFeePeriodic)
                    {
                        #region isFeePeriodic
                        if (_feeMatrix.PAYMENTRULESpecified)
                        {
                            stringInvoicingBValue += "[Rule: " + _feeMatrix.PAYMENTRULE + "]";
                            isFirstPeriodExcluded = (_feeMatrix.PAYMENTRULE == PaymentRuleEnum.FirstPeriodExcluded.ToString())
                                                        || (_feeMatrix.PAYMENTRULE == PaymentRuleEnum.FirstAndLastsPeriodsExcluded.ToString());
                            isLastPeriodExcluded = (_feeMatrix.PAYMENTRULE == PaymentRuleEnum.LastPeriodExcluded.ToString())
                                                        || (_feeMatrix.PAYMENTRULE == PaymentRuleEnum.FirstAndLastsPeriodsExcluded.ToString());
                        }
                        //
                        periods = Tools.ApplyInterval(feeFirstDate, dtEnd, interval, rollConvention);
                        if (periods == null)
                        {
                            #region Comment
                            //Durée du trade < Période de facturation
                            #endregion
                            interval = Parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateInterval(PeriodEnum.T, 1);
                            periods = Tools.ApplyInterval(feeFirstDate, dtEnd, interval, rollConvention);
                        }
                        //else if (DateTime.Compare(dtEnd, periods[periods.Length - 1].date2) > 0)
                        //{
                        //    #region Comment
                        //    //Date fin du trade > Date de dernière période --> On rajoute une période
                        //    //ex.: Trade du 24/05/2008 au 24/07/2008 Facturation 1 Month EOM
                        //    //     - Periode 1: 24/05/2008 au 31/05/2008
                        //    //     - Periode 2: 31/05/2008 au 30/06/2008
                        //    //     On calcul une date forcée (dtEndForced) au 31/04/2008
                        //    //     afin d'obtennir une nième période
                        //    //     - Periode 3: 30/06/2008 au 31/07/2008 
                        //    #endregion
                        //    DateTime dtEndForced = Tools.ApplyInterval(periods[periods.Length - 1].date2, DateTime.MaxValue, interval);
                        //    dtEndForced = Tools.ApplyRollConvention(dtEndForced, rollConvention);
                        //    periods = Tools.ApplyInterval(feeFirstDate, dtEndForced, interval, rollConvention);
                        //}
                        //PL 20091204
                        else
                        {
                            bool isDtForced = false;
                            DateTime dtStartForced = feeFirstDate;
                            DateTime dtEndForced = dtEnd;
                            if (interval.PeriodMultiplier.IntValue == 1 && interval.Period == PeriodEnum.M && rollConvention == RollConventionEnum.EOM)
                            {
                                isDtForced = true;
                                #region Comment
                                //Règlement mensuel fin de mois --> On décale la date de début d'un mois, afin d'avoir un 1ère période "courte"
                                //ex.: Trade du 24/05/2008 au 24/07/2008 Facturation 1 Month EOM
                                //     - Periode 1: 24/05/2008 au 31/05/2008
                                //     - Periode 2: 31/05/2008 au 30/06/2008
                                //     On calcul une date forcée (dtStartForced) au 24/04/2008
                                //     afin d'obtenir une 1ère période ("longue")
                                //     - Periode 1: 24/04/2008 au 30/05/2008 
                                //     période qui sera ramené plus bas à la date de début réelle
                                //     - Periode 1: 24/05/2008 au 30/05/2008 
                                #endregion
                                dtStartForced = feeFirstDate.AddMonths(-1);
                            }
                            if (DateTime.Compare(dtEnd, periods[periods.Length - 1].date2) > 0)
                            {
                                isDtForced = true;
                                #region Comment
                                //Date fin du trade > Date de dernière période --> On rajoute une période
                                //ex.: Trade du 24/05/2008 au 24/07/2008 Facturation 1 Month EOM
                                //     - Periode 1: 24/05/2008 au 31/05/2008
                                //     - Periode 2: 31/05/2008 au 30/06/2008
                                //     On calcul une date forcée (dtEndForced) au 31/07/2008
                                //     afin d'obtenir une nième période
                                //     - Periode 3: 30/06/2008 au 31/07/2008 
                                #endregion
                                dtEndForced = Tools.ApplyInterval(periods[periods.Length - 1].date2, DateTime.MaxValue, interval);
                                dtEndForced = Tools.ApplyRollConvention(dtEndForced, rollConvention);
                            }
                            if (isDtForced)
                            {
                                periods = Tools.ApplyInterval(dtStartForced, dtEndForced, interval, rollConvention);
                                if (dtStartForced != feeFirstDate)
                                {
                                    //Recadrage de la date de début réelle (cf ci-dessus)
                                    periods[0].date1 = feeFirstDate;
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        feeFirstDate = Tools.ApplyRollConvention(feeFirstDate, rollConvention);
                        //
                        periods[0] = new EFS_Period(feeFirstDate, feeFirstDate);
                    }
                    //
                    if (periods != null)
                    {
                        countPayment = Math.Max(1, periods.Length - (isFirstPeriodExcluded ? 1 : 0) - (isLastPeriodExcluded ? 1 : 0));
                    }
                }
                #endregion Periods dates
                //
                _payment = new IPayment[countPayment];
            }
            #endregion Frequency/Date

            #region Bracket 1 & 2
            //TODO FEEMATRIX
            isBracket1 = _feeMatrix.FEESCHED_BRACKETAPPLICATIONSpecified && _feeMatrix.FEESCHED_BRACKETBASISSpecified;
            if (isOk && isBracket1)
            {
                _feeMatrix.CommentForDebug = "Sliding fees schedules Check";
                decimal bracketBasisValue = 1;
                string bracketBasisCurrency = string.Empty;

                #region Select (Code source temporaire pour gérer les tranches de 1er niveau en mode Unitaire uniquement) //Ce commentaire semble obsolète PL 20130620
                _isBracket1Application_Cumulative = (_feeMatrix.FEESCHED_BRACKETAPPLICATION == BracketApplicationEnum.Cumulative.ToString());

                errorMessage = "Sliding fees schedules basis unavailable: the basis for the sliding fees schedules does not exist.";
                isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction ,  _feeMatrix.FEESCHED_BRACKETBASIS, ref bracketBasisValue, ref bracketBasisCurrency, ref errorMessage);

                if (isOk && _feeMatrix.FEESCHED_PERIODMLTPSpecified && _feeMatrix.FEESCHED_PERIODSpecified && (_feeMatrix.FEESCHED_PERIODMLTP > 0))
                {
                    #region Existence d'un paramétrage de Period
                    if (_feeMatrix.FEESCHED_BRACKETBASIS != Cst.AssessmentBasisEnum.Quantity.ToString())
                    {
                        errorMessage = String.Format("{0} basis on Period is unavailable! Only Quantity basis is available.", _feeMatrix.FEESCHED_BRACKETBASIS);
                        isOk = false;
                    }
                    else if (!Parent.FeeRequest.TradeInput.IsETDandAllocation)
                    {
                        errorMessage = "Period is unavailable! Only available on Allocation ETD.";
                        isOk = false;
                    }
                    else
                    {
                        _isBracket1Application_Periodic = true;
                        _isBracket1Application_Cumulative = true;

                        // Lecture de la Date relative (On considère par défaut la date de référence: TransactDate sur OTC et BusinessDate sur ETD)
                        DateTime dtRelativ = Parent.FeeRequest.DtReference;
                        if (_feeMatrix.FEESCHED_RELATIVETOSpecified)
                        {
                            if (_feeMatrix.FEESCHED_RELATIVETO == FeePayRelativeToEnum.TransactDate.ToString())
                            {
                                dtRelativ = dtStart;
                            }
                        }

                        // Calcul des dates de début et fin de la période sur la base de de la Date relative
                        if (!_feeMatrix.FEESCHED_ROLLCONVENTIONSpecified)
                        {
                            //On considère End Of Month
                            _feeMatrix.FEESCHED_ROLLCONVENTION = RollConventionEnum.EOM.ToString();
                        }

                        DateTime dtEndPreviousPeriod = DateTime.MinValue;
                        DateTime dtStartPeriod = DateTime.MinValue;
                        DateTime dtEndPeriod = DateTime.MaxValue;
                        if (!Tools.GetEndDateOfLastPeriod(_feeMatrix.FEESCHED_PERIODMLTP,
                                                        EfsML.Enum.Tools.StringToEnum.Period(_feeMatrix.FEESCHED_PERIOD),
                                                        EfsML.Enum.Tools.StringToEnum.RollConvention(_feeMatrix.FEESCHED_ROLLCONVENTION),
                                                        dtRelativ,
                                                        ref dtEndPreviousPeriod))
                        {
                            errorMessage = "Period uninterpretable! [" + _feeMatrix.FEESCHED_PERIODMLTP.ToString() + " " + _feeMatrix.FEESCHED_PERIOD
                                            + " " + _feeMatrix.FEESCHED_ROLLCONVENTION + " - " + dtRelativ.ToString() + "]";
                            isOk = false;
                        }
                        else
                        {
                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            // Calcul de la valeur de l'Assiette sur la base de:
                            // - la date de fin de la période précédente
                            // - la date du trade concerné
                            // - l'acteur concerné
                            // On somme dans un premier temps la Qté de tous les trades concernés (cf détail dans la query ci-dessous)
                            // à laquelle on vient soustraire dans un second temp les éventuelles Qté résultantes de Transfert (POT) ou Corrections (POC).
                            // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                            if (!_feeMatrix.FEESCHED_IDA_PERIODSpecified)
                            {
                                //Lorsqu'aucun acteur n'est précisé sur le barème, on considère le Payer
                                _feeMatrix.FEESCHED_IDA_PERIOD = _feeMatrix.PartyPayer.m_Party_Ida;
                            }

                            dtStartPeriod = dtEndPreviousPeriod.AddDays(1);
                            switch (EfsML.Enum.Tools.StringToEnum.Period(_feeMatrix.FEESCHED_PERIOD))
                            {
                                case PeriodEnum.D:
                                    dtEndPeriod = dtEndPreviousPeriod.AddDays(_feeMatrix.FEESCHED_PERIODMLTP);
                                    break;
                                case PeriodEnum.W:
                                    dtEndPeriod = dtEndPreviousPeriod.AddDays(7 * _feeMatrix.FEESCHED_PERIODMLTP);
                                    break;
                                case PeriodEnum.M:
                                    dtEndPeriod = dtEndPreviousPeriod.AddMonths(_feeMatrix.FEESCHED_PERIODMLTP);
                                    break;
                                case PeriodEnum.Y:
                                    dtEndPeriod = dtEndPreviousPeriod.AddYears(_feeMatrix.FEESCHED_PERIODMLTP);
                                    break;
                            }

                            #region sqlSelect
                            DataParameters parameters = new DataParameters();
                            parameters.Add(new DataParameter(pCS, "IDA", DbType.Int32), _feeMatrix.FEESCHED_IDA_PERIOD);
                            parameters.Add(new DataParameter(pCS, "DTSTARTPERIOD", DbType.Date), dtStartPeriod);    // FI 20201006 [XXXXX] DbType.Date
                                                                                                                    //PL 20220720 Simplification du critère (cf ci-dessous)
                                                                                                                    //parameters.Add(new DataParameter(pCS, "DTENDPERIOD", DbType.Date), dtEndPeriod);        // FI 20201006 [XXXXX] DbType.Date
                                                                                                                    //PL 20220720 Use DataParameter.GetParameter pour rester homogène avec le reste du code de cette méthode
                                                                                                                    //parameters.Add(new DataParameter(pCS, "DTEXECUTION", DbType.DateTime2), Parent.FeeRequest.TradeInput.CurrentTrade.tradeHeader.tradeDate.DateTimeValue);

                            //PL 20220930 Bug - Use partyTradeInformation.executionDateTime instead of tradeHeader.tradeDate.DateTime
                            //parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEXECUTION), Parent.FeeRequest.TradeInput.CurrentTrade.tradeHeader.tradeDate.DateTimeValue);
                            Nullable<DateTimeOffset> executionDateTime = Parent.FeeRequest.DataDocument.GetExecutionDateTimeOffset();
                            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEXECUTION), executionDateTime.HasValue ? executionDateTime.Value.DateTime : Parent.FeeRequest.TradeInput.CurrentTrade.TradeHeader.TradeDate.DateTimeValue);
                            // FI 20180502 [23926] Usage de IsModeNewCapture
                            bool isExistIdT = (Parent.FeeRequest.TradeInput.SQLTrade != null) && (Parent.FeeRequest.TradeInput.SQLTrade.IdT > 0);

                            //PL 20220729 [25848] Add filter on ETD Product (WARNING: actuellement le calcul dégressif n'est accessible qu'aux seuls ETD)
                            //PL 20220729 [25848] Add filter on TRDTYPE     (WARNING: les PositionOpening sont également exclues)
                            //PL 20230223 [XXXXX] Add filter (t.TRDTYPE is null) 
                            #region WARNING: Other filters from FeeSchedule (TODO)
                            /*
                             * Théoriquement il faudrait rajouter ici nombre des critères en vigueur sur le barème. 
                             * Notamment: Environnement instrumental, Marché/Contrat, Sous-jacent, Trading/Clearing 
                             * Actuellement cela n'est pas nécessaire pour AKROS/WEBANK. 
                             * Inutile donc de complexifier la query ni de mettre en place qq chose dont on n'est pas certain de son intérêt.
                             * Si cela devenait nécessaire, il faudrait partir des éléments de la méthode LoadFeeMatrix() de la classe FeeMatrixs.
                            */
                            #endregion WARNING: Other filters from FeeSchedule (TODO)
                            #region PositionOpening Information
                            /* 
                             * Si un client souhaitait considérer les PositionOpening dans le calcul des Qty négociées il lui faudrait procéder comme suit:
                             * - Importer ou saisir ses Positions Ouvertes avec TRDTYPE=1000 afin d'avoir sur les options un événement HPR pour la prime
                             * - Effectuer un update TRADE set SECONDARYTRDTYPE=TRDTYPE, TRDTYPE=0 where TRDTYPE=1000 and ....
                             * NB: plus tard si cela s'évérait nécessaire on pourrait faire évoluer le référentiel FEESCHEDULE pour rajouter un paramètre décrivant les TRDTYPE à ne pas considérer.
                            */
                            #endregion PositionOpening Information

                            //INFO: si la query se montre à l'avenir peu performante on pourra étudier de remplacer l'usage de TRADEACTOR par TRADE
                            string sqlSelect = @"select {0}
from dbo.TRADE t
inner join dbo.INSTRUMENT i on (i.IDI=t.IDI)
inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p. IDENTIFIER='exchangeTradedDerivative')
inner join dbo.TRADEACTOR ta on (ta.IDT = t.IDT) and (ta.FIXPARTYROLE in ('27','21','4','14'))
inner join dbo.BOOKACTOR_R ba on (ba.IDB = ta.IDB) and (ba.IDA_ACTOR = @IDA)
{1} 
where (t.IDSTENVIRONMENT = 'REGULAR') and (t.IDSTACTIVATION in ('REGULAR','LOCKED')) and (t.IDSTBUSINESS = 'ALLOC') 
  and (
        (t.TRDTYPE is null)        
        or
        (t.TRDTYPE not in ('45' /* OptionExercise */,'63' /* TechnicalTrade */,'1000' /* PositionOpening */,'1001' /* Cascading */,'1002' /* Shifting */,'1003' /* CorporateAction */))
      )
  and (t.DTEXECUTION <= @DTEXECUTION)" + Cst.CrLf;

                            if ((!isNewTrade) && isExistIdT)
                            {
                                // ----------------------------------------------------------------------------------------------------------------------------------------------
                                // Lorsqu'il s'agit d'un trade déjà enregistré, donc disposant d'un IDT, on ne retient que les Qté des trades strictement antérieures à celui-ci.
                                // Ex.: recalcul des frais opéré lors du EOD, modification totale d'un trade, ...
                                // ----------------------------------------------------------------------------------------------------------------------------------------------
                                sqlSelect += @" and (case when t.DTEXECUTION = @DTEXECUTION then t.IDT else 0 end < @IDT)";
                                //PL 20220720 Use DataParameter.GetParameter pour rester homogène avec le reste du code de cette méthode
                                //parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), Parent.FeeRequest.TradeInput.SQLTrade.IdT);
                                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), Parent.FeeRequest.TradeInput.SQLTrade.IdT);
                            }

                            if (_feeMatrix.FEESCHED_RELATIVETO == FeePayRelativeToEnum.TransactDate.ToString())
                            {
                                //PL 20220720 Simplification du critère (en espérant que cela ne modifie pas le plan d'exécution)
                                //sqlSelect += @" and ((t.DTTRADE between @DTSTARTPERIOD and @DTENDPERIOD) and (t.DTTRADE <= @DTTRADE))" + Cst.CrLf;
                                sqlSelect += @" and (t.DTTRADE between @DTSTARTPERIOD and @DTTRADE)" + Cst.CrLf;

                                //PL 20220720 Use DataParameter.GetParameter pour rester homogène avec le reste du code de cette méthode
                                //parameters.Add(new DataParameter(pCS, "DTTRADE", DbType.Date), Parent.FeeRequest.TradeInput.CurrentTrade.tradeHeader.tradeDate.DateValue); // FI 20201006 [XXXXX] DbType.Date
                                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTTRADE), Parent.FeeRequest.TradeInput.CurrentTrade.TradeHeader.TradeDate.DateValue); 
                            }
                            else
                            {
                                //PL 20220720 Simplification du critère (en espérant que cela ne modifie pas le plan d'exécution)
                                //sqlSelect += @" and ((t.DTBUSINESS  between @DTSTARTPERIOD and @DTENDPERIOD) and (t.DTBUSINESS <= @DTBUSINESS))" + Cst.CrLf;
                                sqlSelect += @" and ((t.DTBUSINESS  between @DTSTARTPERIOD and @DTBUSINESS)" + Cst.CrLf;

                                ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)Parent.FeeRequest.TradeInput.CurrentTrade.Product);
                                parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), etd.ClearingBusinessDate); // FI 20201006 [XXXXX] DbType.Date
                            }

                            sqlSelect += "{2}";
                            #endregion sqlSelect

                            #region DataReader & Setting
                            //Lecture des quantités (NB: avant la v10 la lecture était opérée depuis TRADETRADEINSTRUMENT)
                            //PL 20220720 [25848] Correction alias "t" instead of "tr" on sum(t.QTY) as QTY [Commentaire à supprimer une fois le report opéré en v10]
                            QueryParameters qry = new QueryParameters(pCS, String.Format(sqlSelect,
                                                                           "sum(t.QTY) as QTY, count(t.IDT) as NUMBEROFTRADES, min(t.IDT) as MINIDT, max(t.IDT) as MAXIDT",
                                                                           string.Empty, 
                                                                           string.Empty), parameters);

                            int numberOfTrades = 0;
                            string minIdT = Cst.NotAvailable;
                            string maxIdT = Cst.NotAvailable;

                            using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                            {
                                if (dr.Read())
                                {
                                    numberOfTrades = Convert.ToInt32(dr["NUMBEROFTRADES"]);

                                    if (numberOfTrades > 0)
                                    {
                                        // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                        // EG 20170127 Qty Long To Decimal
                                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                        // Step 1: Somme des quantités déjà négociées sur la période (hors trade courant)
                                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                        _periodBasisValue = Convert.ToDecimal(dr["QTY"]);
                                        // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                        minIdT = dr["MINIDT"].ToString();
                                        maxIdT = dr["MAXIDT"].ToString();
                                    }
                                }
                            }

                            // EG 20220816 [XXXXX] [WI396] Sortie du select du DataReader

                            if (numberOfTrades > 0)
                            {
                                //Lecture des quantités corrigées/transférées depuis EVENT (EventCode in POC, POT)
                                parameters.Add(new DataParameter(pCS, "MINIDT", DbType.Int32), Convert.ToInt32(minIdT));
                                parameters.Add(new DataParameter(pCS, "MAXIDT", DbType.Int32), Convert.ToInt32(maxIdT));

                                qry = new QueryParameters(pCS, String.Format(sqlSelect.ToString(),
                                                               "sum(e.VALORISATION) as QTY",
                                                               @"left outer join dbo.EVENT e on (e.IDT = t.IDT) and (e.IDSTACTIVATION = 'REGULAR') and (e.EVENTCODE in ('POC','POT'))
                                                               left outer join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.DTEVENT <= @DTEXECUTION)",
                                                               "and (t.IDT between @MINIDT and @MAXIDT)"), parameters);

                                Object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                                if (obj != null)
                                {
                                    //Retranchement de la somme des quantités corrigées/transférées sur la période (hors trade courant)
                                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                                    // EG 20170127 Qty Long To Decimal
                                    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                    // Step 2: Soustraction de la somme des quantités corrigées/transférées sur la période (hors trade courant)
                                    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                    _periodBasisValue = Math.Max(0, _periodBasisValue - Convert.ToDecimal(obj));
                                    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                }

                                bracketBasisValue += _periodBasisValue;
                            }

                            _periodCharacteristics = String.Format("[Start: {0} End: {1}][Value: {2}][Count: {3} First: id:{4} Last: id:{5}]",
                                    DtFunc.DateTimeToStringDateISO(dtStartPeriod), DtFunc.DateTimeToStringDateISO(dtEndPeriod),
                                    StrFunc.FmtDecimalToInvariantCulture(_periodBasisValue), numberOfTrades.ToString(), minIdT, maxIdT);

                            #endregion DataReader & Setting
                        }
                    }
                    #endregion
                }

                if (isOk)
                {
                    #region Exchange
                    if (StrFunc.IsFilled(bracketBasisCurrency) && (bracketBasisCurrency != _feeMatrix.FEESCHED_IDC))
                    {
                        isOk = _feeMatrix.FEESCHED_BRACKETBASIS_EXCHTSpecified;
                        if (!isOk)
                            errorMessage = "Sliding fees schedules basis exchange unavailable: the fixing rule does not exist.";
                        else
                        {
                            isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_BRACKETBASIS_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                        _feeMatrix.FEESCHED_IDC,
                                        ref bracketBasisValue, ref bracketBasisCurrency, ref exchangeMessage);
                            if (!isOk)
                                errorMessage = "Sliding fees schedules basis exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_BRACKETBASIS_EXCHT + ").";
                        }
                    }
                    #endregion Exchange
                    //
                    if (isOk)
                    {
                        errorMessage = "Sliding fees schedules range unavailable: the range of the sliding fees schedules is unavailable (Range: " + bracketBasisValue.ToString() + ").";
                        #region sqlSelect
                        DataParameters parameters = new DataParameters();

                        StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT);
                        sqlSelect += "IDENTIFIER, DISPLAYNAME, LOWVALUE, HIGHVALUE, " + Cst.CrLf;

                        sqlSelect += "BRACKETBASIS, BRACKETBASIS_EXCHT, BRACKETAPPLICATION, " + Cst.CrLf;

                        sqlSelect += "FEE1NUM, " + DataHelper.SQLIsNull(pCS, "FEE1DEN", "1", "FEE1DEN") + ", FEE1EXPRESSIONTYPE, IDC_FEE1, FEE1_EXCHT, " + Cst.CrLf;
                        sqlSelect += "FEE2NUM, " + DataHelper.SQLIsNull(pCS, "FEE2DEN", "1", "FEE2DEN") + ", FEE2EXPRESSIONTYPE, IDC_FEE2, FEE2_EXCHT, " + Cst.CrLf;

                        sqlSelect += "MINNUM, " + DataHelper.SQLIsNull(pCS, "MINDEN", "1", "MINDEN") + ", MAXNUM, " + DataHelper.SQLIsNull(pCS, "MAXDEN", "1", "MAXDEN") + ", " + Cst.CrLf;
                        sqlSelect += "MINMAXEXPRESSIONT, MINMAXBASIS, IDC_MINMAXBASIS, MINMAXBASIS_EXCHT" + Cst.CrLf;

                        sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.FEESCHEDBRACKET1.ToString() + Cst.CrLf;

                        sqlSelect += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(pCS, Cst.OTCml_TBL.FEESCHEDBRACKET1, Parent.FeeRequest.DtReference) + Cst.CrLf;
                        sqlSelect += SQLCst.AND + "(IDFEESCHEDULE=@IDFEESCHEDULE)" + Cst.CrLf;
                        //PL 20171205 Add (LOWVALUE=0 and @BRACKETBASISVALUE=0) for managed a basis value equal to zero
                        //            Remove (LOWVALUE is null) because LOWVALUE is a not nullable column
                        //sqlSelect += SQLCst.AND + "((LOWVALUE < @BRACKETBASISVALUE) or (LOWVALUE is null))" + Cst.CrLf;
                        sqlSelect += SQLCst.AND + "((LOWVALUE < @BRACKETBASISVALUE) or (LOWVALUE=0 and @BRACKETBASISVALUE=0))" + Cst.CrLf;
                        if (!_isBracket1Application_Cumulative)
                        {
                            sqlSelect += SQLCst.AND + "((HIGHVALUE >= @BRACKETBASISVALUE) or (HIGHVALUE is null))" + Cst.CrLf;
                        }

                        sqlSelect += SQLCst.ORDERBY + "LOWVALUE asc";
                        #endregion sqlSelect
                        //
                        #region Dataset & Setting
                        parameters.Add(new DataParameter(pCS, "IDFEESCHEDULE", DbType.Int32), _feeMatrix.IDFEESCHEDULE);
                        parameters.Add(new DataParameter(pCS, "BRACKETBASISVALUE", DbType.Decimal), bracketBasisValue);
                        //
                        DataSet dsResult = DataHelper.ExecuteDataset(pCS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
                        dsResult.DataSetName = "FEEBRACKET1";
                        _dtBracket1 = dsResult.Tables[0];
                        _dtBracket1.TableName = "FEEBRACKET1";
                        //
                        isOk = ((null != _dtBracket1.Rows) && (_dtBracket1.Rows.Count > 0));
                        if (isOk)
                        {
                            if ((!(_dtBracket1.Rows[0]["BRACKETBASIS"] is DBNull)) && StrFunc.IsFilled(_dtBracket1.Rows[0]["BRACKETBASIS"].ToString()))
                            {
                                //*******************************************************
                                //WARNING: TODO Bracket Level 2 
                                //*******************************************************
                                isOk = false;
                                errorMessage = "Sliding fees schedules Level 2 unavailable";
                            }
                            else if (!_isBracket1Application_Cumulative)
                            {
                                //-------------------------------------------------------
                                //Bracket Level 1 Unit
                                //-------------------------------------------------------
                                if (_dtBracket1.Rows.Count > 1)
                                {
                                    isOk = false;
                                    errorMessage = "Many sliding fees schedules ranges found: " + _dtBracket1.Rows.Count.ToString() + " ranges are founded (Range: " + bracketBasisValue.ToString() + ").";
                                }
                            }
                        }
                        // EG 20150713 Report PL
                        else if (!_isBracket1Application_Cumulative)
                        {
                            //PL 20150709 New features - Si "Tranche unitaire" et aucune tranche de paramétrée, on ne considère plus une erreur.
                            //                           Cette nouveauté est destinée à ne plus imposer la saisie d'une tranche avec un taux à zéro 
                            //                           et donc à ne plus générer de frais à zéro (ex.: PTM levy fee) 
                            _isBracket1_NoBracket = true;
                        }
                        if (isOk && _isBracket1Application_Cumulative && (_dtBracket1.Rows.Count == 1))
                        {
                            //PL 20150709 New features - Si "Tranche cumultative" et une seule tranche de concernée, on contrôle si la valeur est zéro.
                            //                           Cette nouveauté est destinée à ne plus générer de frais à zéro dasn un tel cas. 
                            #region isZero
                            bool isZero = false;
                            if ((_dtBracket1.Rows[0]["FEE1EXPRESSIONTYPE"] is DBNull))
                            {
                                if (_feeMatrixOriginalValues.FEESCHED_FEE1NUMSpecified)
                                    isZero = (_feeMatrixOriginalValues.FEESCHED_FEE1NUM == 0);
                            }
                            else
                            {
                                if (!(_dtBracket1.Rows[0]["FEE1NUM"] is DBNull))
                                    isZero = (Convert.ToDouble(_dtBracket1.Rows[0]["FEE1NUM"]) == 0);
                            }
                            #endregion
                            if (isZero)
                            {
                                isOk = false;
                                _isBracket1_NoBracket = true;
                            }
                        }

                        dsResult = null;
                        #endregion Dataset & Setting
                    }
                }
                //else
                //{
                //    errorMessage = "Sliding fees schedules type invalid: the type of sliding fees schedules is incorrect.";
                //}
                #endregion
            }
            #endregion Bracket 1 & 2

            #region Calcul du ou des montants des frais et sauvegarde (des --> Courtage périodique)
            FeeFormulaEnum formula = (FeeFormulaEnum)Enum.Parse(typeof(FeeFormulaEnum), _feeMatrix.FEESCHED_FORMULA);
            int currentPayment = -1;
            EFS_Period currentPeriod;
            while (isOk && (currentPayment < countPayment - 1))
            {
                #region Initialisation
                currentPayment++;
                currentPeriod = periods[Math.Min((countPayment - 1), currentPayment + (isFirstPeriodExcluded ? 1 : 0))];
                //
                decimal currentFeeValue;
                string currentFeeCurrency;
                int countBraket = -1;
                _currentBraket = -1;
                feeValue = 0;
                feeCurrency = null;
                //
                if (_isBracket1Application_Cumulative)
                {
                    countBraket = _dtBracket1.Rows.Count;
                    stringFormulaBracket1Value = BracketApplicationEnum.Cumulative.ToString();
                    stringFormulaValue1Value = new string[countBraket];
                    stringFormulaValue2Value = new string[countBraket];
                    //PL 20150313 -----------------------------------------------------
                    stringFormulaMinValue = new string[countBraket];
                    stringFormulaMaxValue = new string[countBraket];
                    for (int i = 0; i < countBraket; i++)
                    {
                        stringFormulaMinValue[i] = stringFormulaMaxValue[i] = Cst.NotAvailable;
                    }
                    //PL 20150313 -----------------------------------------------------
                }
                #endregion Initialisation
                //
                #region Calcul unitaire ou Boucle dans le cas de tranches "cumulatives" de 1er niveau (Bracket1)
                //20090525 PL Add isOk dans le While (Par mesure de sécurité...)
                while (isOk &&
                    (
                    (_isBracket1Application_Cumulative && (_currentBraket < countBraket - 1))
                    ||
                    (_currentBraket == -1)
                    )
                    )
                {
                    currentFeeValue = 0;
                    currentFeeCurrency = _feeMatrix.FEESCHED_IDC;
                    _currentBraket++;

                    if (isBracket1)
                    {
                        LoadValuesFromBracket1();
                    }

                    #region Formules
                    if (isOk)
                    {
                        #region F1, F2_Vx, F3MO, F3MOD, F3KO, F3KOD, F4x
                        if (
                            (formula == FeeFormulaEnum.F1)
                            ||
                            (formula == FeeFormulaEnum.F2_V1) || (formula == FeeFormulaEnum.F2_V2) || (formula == FeeFormulaEnum.F2_V3)
                            ||
                            (formula == FeeFormulaEnum.F3MO) || (formula == FeeFormulaEnum.F3MOD)
                            ||
                            (formula == FeeFormulaEnum.F3KO) || (formula == FeeFormulaEnum.F3KOD)
                            ||
                            (formula == FeeFormulaEnum.F4) || (formula == FeeFormulaEnum.F4QTY) || (formula == FeeFormulaEnum.F4PRM) || (formula == FeeFormulaEnum.F4STK)
                            ||
                            (formula == FeeFormulaEnum.F4CPS) || (formula == FeeFormulaEnum.F4BPS)
                            ||
                            (formula == FeeFormulaEnum.F5CPSBPS)
                            ||
                            (formula == FeeFormulaEnum.CSHARP)
                            )
                        {
                            _feeMatrix.CommentForDebug = "Formula " + formula;
                            decimal fee1AssessmentBasisValue, fee2AssessmentBasisValue;
                            string fee1AssessmentBasisCurrency, fee2AssessmentBasisCurrency;
                            fee1AssessmentBasisValue = fee2AssessmentBasisValue = 0;
                            fee1AssessmentBasisCurrency = fee2AssessmentBasisCurrency = null;

                            // RD 20100210
                            // Si l'assiette FeeRequest.AssessmentBasis est valorisée, alors utiliser sa valeur (FeeRequest.AssessmentBasis_DecValue et FeeRequest.AssessmentBasis_StrValue)
                            //  - En sachant que FeeMatrixs LoadFeeMatrix(), ne seront chargée que les barèmes avec comme assiette (FEE1FORMULABASIS) = FeeRequest.AssessmentBasis
                            // Sinon, chercher sur le Trade la valeur de l’assiette définie sur le barème                            
                            // RD 20170208 [22815]
                            //if (StrFunc.IsFilled(Parent.FeeRequest.AssessmentBasis))
                            if (ArrFunc.IsFilled(Parent.FeeRequest.AssessmentBasis))
                            {
                                // RD 20170308 [22921]
                                //fee1AssessmentBasisDecValue = Parent.FeeRequest.AssessmentBasis_DecValue;
                                //fee1AssessmentBasisStrValue = Parent.FeeRequest.AssessmentBasis_StrValue;
                                isOk = Parent.GetFeeRequestAssessmentBasisValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE1FORMULABASIS,
                                    ref fee1AssessmentBasisDecValue, ref fee1AssessmentBasisStrValue, ref errorMessage);
                            }
                            else
                            {
                                //PL 20141017
                                errorMessage = "First formula basis unavailable: the basis for the formula does not exist.";
                                isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE1FORMULABASIS, ref fee1AssessmentBasisDecValue, ref fee1AssessmentBasisStrValue, ref errorMessage);
                                if (isOk && _feeMatrix.FEESCHED_FEE2FORMULABASISSpecified)
                                {
                                    errorMessage = "Second formula basis unavailable: the basis for the formula does not exist.";
                                    isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE2FORMULABASIS, ref fee2AssessmentBasisDecValue, ref fee2AssessmentBasisStrValue, ref errorMessage);
                                    fee2AssessmentBasisSpecified = true;
                                }
                            }

                            if (isOk)
                            {
                                fee1AssessmentBasisValue = fee1AssessmentBasisDecValue;
                                fee1AssessmentBasisCurrency = fee1AssessmentBasisStrValue;
                                if (fee2AssessmentBasisSpecified)
                                {
                                    fee2AssessmentBasisValue = fee2AssessmentBasisDecValue;
                                    fee2AssessmentBasisCurrency = fee2AssessmentBasisStrValue;
                                }

                                //PL 20150707 WARNING: Déplacement de la #region Exchange AVANT la #region Bracket1 Cumulative
                                #region Exchange
                                if (StrFunc.IsFilled(fee1AssessmentBasisCurrency) && (fee1AssessmentBasisCurrency != _feeMatrix.FEESCHED_IDC))
                                {
                                    isOk = _feeMatrix.FEESCHED_FEE1FORMULABASIS_EXCHTSpecified;//PL 20141017
                                    if (!isOk)
                                    {
                                        errorMessage = "First formula basis exchange unavailable: the fixing rule does not exist.";
                                    }
                                    else
                                    {
                                        isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE1FORMULABASIS_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,//PL 20141017
                                                    _feeMatrix.FEESCHED_IDC,
                                                    ref fee1AssessmentBasisValue, ref fee1AssessmentBasisCurrency, ref exchangeMessage);
                                        if (!isOk)
                                            errorMessage = "First formula basis exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_FEE1FORMULABASIS_EXCHT + ").";
                                    }
                                }
                                if (fee2AssessmentBasisSpecified)
                                {
                                    if (StrFunc.IsFilled(fee2AssessmentBasisCurrency) && (fee2AssessmentBasisCurrency != _feeMatrix.FEESCHED_IDC))
                                    {
                                        isOk = _feeMatrix.FEESCHED_FEE2FORMULABASIS_EXCHTSpecified;
                                        if (!isOk)
                                        {
                                            errorMessage = "Second formula basis exchange unavailable: the fixing rule does not exist.";
                                        }
                                        else
                                        {
                                            isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE2FORMULABASIS_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                                        _feeMatrix.FEESCHED_IDC,
                                                        ref fee2AssessmentBasisValue, ref fee2AssessmentBasisCurrency, ref exchangeMessage);
                                            if (!isOk)
                                                errorMessage = "Second formula basis exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_FEE2FORMULABASIS_EXCHT + ").";
                                        }
                                    }
                                }
                                #endregion Exchange

                                #region Bracket1 Cumulative
                                if (_isBracket1Application_Cumulative) 
                                {
                                    if (_currentBraket == 0)
                                    {
                                        #region Première tranche: Assiette = High
                                        if (_dtBracket1.Rows[_currentBraket]["HIGHVALUE"] != Convert.DBNull)
                                        {
                                            if (_isBracket1Application_Periodic)
                                            {
                                                //Assiette = Assiette + Qté déjà réalisée sur la période
                                                fee1AssessmentBasisValue += _periodBasisValue;
                                                if (fee2AssessmentBasisSpecified)
                                                    fee2AssessmentBasisValue += _periodBasisValue;
                                            }

                                            //PL 20150707 bracket1_HighValue est exprimé en devise du barème 
                                            decimal bracket1_HighValue = Convert.ToDecimal(_dtBracket1.Rows[_currentBraket]["HIGHVALUE"]);
                                            fee1AssessmentBasisValue = Math.Min(fee1AssessmentBasisValue, bracket1_HighValue);
                                            if (fee2AssessmentBasisSpecified)
                                                fee2AssessmentBasisValue = Math.Min(fee2AssessmentBasisValue, bracket1_HighValue);

                                            if (_isBracket1Application_Periodic)
                                            {
                                                //Assiette = Assiette - Qté déjà réalisée  sur la période
                                                fee1AssessmentBasisValue = Math.Max(0, fee1AssessmentBasisValue - _periodBasisValue);
                                                if (fee2AssessmentBasisSpecified)
                                                    fee2AssessmentBasisValue = Math.Max(0, fee2AssessmentBasisValue - _periodBasisValue);
                                            }
                                        }
                                        #endregion
                                    }
                                    else if (_currentBraket == countBraket - 1)
                                    {
                                        #region Dernière tranche
                                        if (_isBracket1Application_Periodic)
                                        {
                                            //Assiette = Assiette + Qté déjà réalisée sur la période
                                            fee1AssessmentBasisValue += _periodBasisValue;
                                            if (fee2AssessmentBasisSpecified)
                                                fee2AssessmentBasisValue += _periodBasisValue;
                                        }

                                        //Dernière tranche: Assiette = Assiette - Low
                                        if (_dtBracket1.Rows[_currentBraket]["LOWVALUE"] != Convert.DBNull)
                                        {
                                            //PL 20150707 bracket1_HighValue est exprimé en devise du barème 
                                            decimal bracket1_LowValue = Convert.ToDecimal(_dtBracket1.Rows[_currentBraket]["LOWVALUE"]);
                                            fee1AssessmentBasisValue = Math.Max(0, fee1AssessmentBasisValue - bracket1_LowValue);
                                            fee1AssessmentBasisValue = Math.Min(fee1AssessmentBasisValue, fee1AssessmentBasisDecValue);
                                            if (fee2AssessmentBasisSpecified)
                                            {
                                                fee2AssessmentBasisValue = Math.Max(0, fee2AssessmentBasisValue - bracket1_LowValue);
                                                fee2AssessmentBasisValue = Math.Min(fee2AssessmentBasisValue, fee2AssessmentBasisDecValue);
                                            }
                                        }
                                        //Dernière tranche: Assiette = Assiette - High(PreviousBracket)
                                        else if (_dtBracket1.Rows[_currentBraket - 1]["HIGHVALUE"] != Convert.DBNull)
                                        {
                                            fee1AssessmentBasisValue = Math.Max(0, fee1AssessmentBasisValue - Convert.ToDecimal(_dtBracket1.Rows[_currentBraket - 1]["HIGHVALUE"]));
                                            fee1AssessmentBasisValue = Math.Min(fee1AssessmentBasisValue, fee1AssessmentBasisDecValue);
                                            if (fee2AssessmentBasisSpecified)
                                            {
                                                fee2AssessmentBasisValue = Math.Max(0, fee2AssessmentBasisValue - Convert.ToDecimal(_dtBracket1.Rows[_currentBraket - 1]["HIGHVALUE"]));
                                                fee2AssessmentBasisValue = Math.Min(fee2AssessmentBasisValue, fee2AssessmentBasisDecValue);
                                            }
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Tranche(s) intermédiaire(s): Assiette = High - Low
                                        if (_dtBracket1.Rows[_currentBraket]["LOWVALUE"] != Convert.DBNull
                                            && _dtBracket1.Rows[_currentBraket]["HIGHVALUE"] != Convert.DBNull)
                                        {
                                            //PL 20150707 bracket1_HighValue est exprimé en devise du barème 
                                            decimal bracket1_HighValue = Convert.ToDecimal(_dtBracket1.Rows[_currentBraket]["HIGHVALUE"]);
                                            decimal bracket1_LowValue = Convert.ToDecimal(_dtBracket1.Rows[_currentBraket]["LOWVALUE"]);

                                            fee1AssessmentBasisValue = Math.Max(0, bracket1_HighValue - bracket1_LowValue);
                                            if (fee2AssessmentBasisSpecified)
                                                fee2AssessmentBasisValue = fee1AssessmentBasisValue;

                                            if (_isBracket1Application_Periodic)
                                            {
                                                if (_periodBasisValue >= bracket1_HighValue)
                                                {
                                                    fee1AssessmentBasisValue = 0;
                                                    if (fee2AssessmentBasisSpecified)
                                                        fee2AssessmentBasisValue = fee1AssessmentBasisValue;
                                                }
                                                else if (_periodBasisValue > bracket1_LowValue)
                                                {
                                                    fee1AssessmentBasisValue = Math.Max(0, bracket1_HighValue - _periodBasisValue);
                                                    if (fee2AssessmentBasisSpecified)
                                                        fee2AssessmentBasisValue = fee1AssessmentBasisValue;
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                                #endregion Bracket1 Cumulative

                                if (isOk)
                                {
                                    errorMessage = "Formula Result";
                                    DayCountFractionEnum formula_DCF = DayCountFractionEnum.ACTACTISDA;
                                    if (_feeMatrix.FEESCHED_FORMULA_DCFSpecified)
                                        formula_DCF = EfsML.Enum.Tools.StringToEnum.DayCountFraction(_feeMatrix.FEESCHED_FORMULA_DCF);
                                    EFS_DayCountFraction efs_dcf = null;

                                    #region F1, F3MOD, F3KOD --> Durée et [Base]
                                    if (
                                        (formula == FeeFormulaEnum.F1)
                                        ||
                                        (formula == FeeFormulaEnum.F3MOD) || (formula == FeeFormulaEnum.F3KOD)
                                        ||
                                        (formula == FeeFormulaEnum.F2_V1) || (formula == FeeFormulaEnum.F2_V2) || (formula == FeeFormulaEnum.F2_V3)
                                        )
                                    {
                                        if (isFeePeriodic)
                                        {
                                            //On tient compte de la durée réelle de la période
                                            if (DateTime.Compare(currentPeriod.date2, dtEnd) > 0)
                                                //Cas de la dernière période évoquée plus haut... (cf. #region Comment)
                                                isOk = Parent.GetDCF(formula_DCF, currentPeriod.date1, dtEnd, out efs_dcf);
                                            else
                                                isOk = Parent.GetDCF(formula_DCF, currentPeriod.date1, currentPeriod.date2, out efs_dcf);
                                        }
                                        else
                                        {
                                            //On tient compte de la durée totale du trade
                                            // EG 20150708 (21103]
                                            if (Parent.FeeRequest.IsSafekeeping)
                                                isOk = Parent.GetDCF(formula_DCF, 
                                                    Parent.FeeRequest.SafekeepingAction.dtStartPeriod.DateValue,
                                                    Parent.FeeRequest.SafekeepingAction.dtEndPeriod.DateValue, out efs_dcf);
                                            else
                                                isOk = Parent.GetDCF(pCS, formula_DCF, out efs_dcf);
                                        }
                                        if (!isOk)
                                            errorMessage = "Formula DCF invalid: The Day Count Fraction does not exist or is incorrect on the repository.";
                                    }
                                    #endregion

                                    #region Calculate feeValue from FEE1 and FEE2
                                    if (isOk)
                                    {
                                        FeeDataForFreeFormula dataFees = new FeeDataForFreeFormula();
                                        isOk = _feeMatrix.FEESCHED_FEE1NUMSpecified && _feeMatrix.FEESCHED_FEE1DENSpecified && _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified;
                                        if (isOk)
                                        {
                                            #region FEE1
                                            decimal coeff = GetProductCoefficient(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE);
                                            decimal feeRate = (decimal)(_feeMatrix.FEESCHED_FEE1NUM / _feeMatrix.FEESCHED_FEE1DEN);

                                            #region Exchange
                                            if (_feeMatrix.FEESCHED_IDC_FEE1Specified && (_feeMatrix.FEESCHED_IDC_FEE1 != _feeMatrix.FEESCHED_IDC))
                                            {
                                                isOk = _feeMatrix.FEESCHED_FEE1_EXCHTSpecified;
                                                if (isOk)
                                                {
                                                    string tmpCurrency = _feeMatrix.FEESCHED_IDC_FEE1;
                                                    isOk = Parent.GetExchangeValue(pCS, pDbTransaction,  _feeMatrix.FEESCHED_FEE1_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                                                _feeMatrix.FEESCHED_IDC,
                                                                ref feeRate, ref tmpCurrency, ref exchangeMessage);
                                                    if (!isOk)
                                                        errorMessage = "Value 1 exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_FEE1_EXCHT + ").";
                                                }
                                                else
                                                {
                                                    isOk = true;
                                                    currentFeeCurrency = _feeMatrix.FEESCHED_IDC_FEE1;
                                                }
                                            }
                                            #endregion

                                            if (isOk)
                                            {
                                                #region F2_V1
                                                if (formula == FeeFormulaEnum.F2_V1)
                                                {
                                                    string tmp = null;
                                                    decimal tradeRate = 0;
                                                    int dcfYear = 0;

                                                    efs_dcf.IsCalculateAlwaysNumberOfCalendarYears = true;
                                                    efs_dcf.Calc(); //Rappel de Calc() pour prise en compte de IsCalculateAlwaysNumberOfCalendarYears
                                                    dcfYear = efs_dcf.NumberOfCalendarYears;

                                                    errorMessage = "F2_V1 formula trade rate unavailable: the fixed rate does not exist or is empty on the trade.";
                                                    isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.FixedRate, ref tradeRate, ref tmp, ref errorMessage);
                                                    if (isOk)
                                                    {
                                                        //isOk = (tradeRate > 0) && (dcfYear >= 1);
                                                        isOk = (tradeRate > 0);
                                                        if (isOk)
                                                        {
                                                            currentFeeValue = 0;

                                                            if (dcfYear >= 1)
                                                            {
                                                                currentFeeValue = (fee1AssessmentBasisValue * (feeRate * coeff));
                                                                currentFeeValue *= (1 - (decimal)Math.Pow((double)(1 + tradeRate), (double)-dcfYear));
                                                                currentFeeValue /= (tradeRate);

                                                                stringDCFValue = " " + dcfYear.ToString();
                                                            }

                                                            #region Avec Rompu (efs_dcf.Numerator > 0)
                                                            if (efs_dcf.Numerator > 0)
                                                            {
                                                                decimal stubDcf = (efs_dcf.Numerator / efs_dcf.Denominator);
                                                                decimal stubFeeValue = (fee1AssessmentBasisValue * (feeRate * coeff));
                                                                stubFeeValue *= stubDcf;
                                                                currentFeeValue = (currentFeeValue + stubFeeValue) * (decimal)Math.Pow((double)(1 + tradeRate), (double)-stubDcf);

                                                                FormatDCFValue(efs_dcf.Numerator, efs_dcf.Denominator, ref stringDCFValue);
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            errorMessage = "F2_V1 formula data incorrect: the fixed rate is missing on the trade.";
                                                        }
                                                    }
                                                }
                                                #endregion F2_V1
                                                #region F2_V2
                                                else if (formula == FeeFormulaEnum.F2_V2)
                                                {
                                                    string tmp = null;
                                                    decimal tradeRate = 0;
                                                    int dcfNum = efs_dcf.TotalNumberOfCalculatedDays;
                                                    decimal dcfDen = efs_dcf.Denominator;
                                                    //
                                                    errorMessage = "F2_V2 formula trade rate unavailable: the fixed rate does not exist or is empty on the trade.";
                                                    isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.FixedRate, ref tradeRate, ref tmp, ref errorMessage);
                                                    if (isOk)
                                                    {
                                                        isOk = (tradeRate > 0);
                                                        if (isOk)
                                                        {
                                                            currentFeeValue = (fee1AssessmentBasisValue * (feeRate * coeff));
                                                            currentFeeValue *= (1 - (decimal)Math.Pow((double)(1 + tradeRate), (double)(-dcfNum / dcfDen)));
                                                            currentFeeValue /= (tradeRate);

                                                            FormatDCFValue(dcfNum, dcfDen, ref stringDCFValue);
                                                        }
                                                        else
                                                        {
                                                            errorMessage = "F2_V2 formula data incorrect: the fixed rate is missing on the trade.";
                                                        }
                                                    }
                                                }
                                                #endregion F2_V2
                                                #region F2_V3
                                                else if (formula == FeeFormulaEnum.F2_V3)
                                                {
                                                    string tmp = null;
                                                    decimal tradeRate = 0;
                                                    decimal tradeNumberOfPeriod = 0;
                                                    int dcfNum = efs_dcf.TotalNumberOfCalculatedDays;
                                                    decimal dcfDen = efs_dcf.Denominator;
                                                    //
                                                    errorMessage = "F2_V3 formula trade rate unavailable: the fixed rate does not exist or is empty on the trade.";
                                                    isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.FixedRate, ref tradeRate, ref tmp, ref errorMessage)
                                                        && Parent.GetAssessmentBasisValue(pCS, pDbTransaction, Cst.AssessmentBasisEnum.NumberOfPeriodInYear, ref tradeNumberOfPeriod, ref tmp, ref errorMessage);
                                                    if (isOk)
                                                    {
                                                        isOk = (tradeRate > 0) && (tradeNumberOfPeriod > 0);
                                                        if (isOk)
                                                        {
                                                            #region Detail
                                                            //z(F9) --> Nb de Période de Paiement
                                                            int z = (int)tradeNumberOfPeriod;
                                                            //nbj(H15) --> Nb Jour total
                                                            int nbj = efs_dcf.TotalNumberOfCalendarDays;
                                                            //base360_365(H12) --> 365 si trade supérieur à une année calendaire sinon 360 (voir EPL)
                                                            int base360_365 = 360;
                                                            if (efs_dcf.NumberOfCalendarYears > 365)
                                                                base360_365 = 365;
                                                            //nba(G18) --> Nb Année Pleine
                                                            int nba = (int)Math.Floor((decimal)nbj / base360_365);
                                                            //k(F18) = H15 - G18 * H12 --> Nb Jour Restant
                                                            decimal k = nbj - (nba * base360_365);
                                                            //n(H18) = G18 * F9 --> 
                                                            int n = nba * (int)tradeNumberOfPeriod;
                                                            //x360 --> constante (ou pourrait être issus du barème...)
                                                            int x360 = 360;//efs_dcf.Denominator;
                                                            #endregion Detail
                                                            decimal tmpPow = (decimal)Math.Pow((double)(1 + tradeRate / z), (double)n);
                                                            feeRate /= 100;
                                                            currentFeeValue = (fee1AssessmentBasisValue * (feeRate * coeff));
                                                            currentFeeValue /= (1 + (tradeRate * (k / x360)));
                                                            currentFeeValue *= ((1 / tradeRate) * (1 - 1 / tmpPow) + (k / x360));

                                                            FormatDCFValue(dcfNum, dcfDen, ref stringDCFValue);
                                                        }
                                                        else
                                                        {
                                                            errorMessage = "F2_V3 formula data incorrect: the fixed rate is missing or the period is incorrect on the trade.";
                                                        }
                                                    }
                                                }
                                                #endregion F2_V3
                                                #region Others formulas
                                                else
                                                {
                                                    int dcfNum = 1;
                                                    decimal dcfDen = 1;
                                                    if ((formula == FeeFormulaEnum.F1) || (formula == FeeFormulaEnum.F3MOD) || (formula == FeeFormulaEnum.F3KOD))
                                                        dcfNum = efs_dcf.TotalNumberOfCalculatedDays;

                                                    if (formula == FeeFormulaEnum.F1)
                                                        dcfDen = efs_dcf.Denominator;

                                                    //PL 20141023
                                                    if (formula == FeeFormulaEnum.CSHARP)
                                                    {
                                                        dataFees.dataFee[0].DcfNum = dcfNum;
                                                        dataFees.dataFee[0].DcfDen = dcfDen;
                                                        dataFees.dataFee[0].Rate = feeRate * coeff;
                                                        dataFees.dataFee[0].AssessmentBasis = fee1AssessmentBasisValue;
                                                    }
                                                    else
                                                    {
                                                        currentFeeValue = fee1AssessmentBasisValue * (feeRate * coeff) * (dcfNum / dcfDen);

                                                        #region F3MO, F3MOD --> Changement d'unité (Million)
                                                        if ((formula == FeeFormulaEnum.F3MO) || (formula == FeeFormulaEnum.F3MOD))
                                                            currentFeeValue /= 1000000;
                                                        #endregion
                                                        #region F3KO, F3KOD --> Changement d'unité (Millier)
                                                        if ((formula == FeeFormulaEnum.F3KO) || (formula == FeeFormulaEnum.F3KOD))
                                                            currentFeeValue /= 1000;
                                                        #endregion
                                                    }

                                                    FormatDCFValue(dcfNum, dcfDen, ref stringDCFValue);
                                                }
                                                #endregion others formula

                                                if (formula != FeeFormulaEnum.CSHARP)
                                                {
                                                    FormatValueForLog(ref stringFormulaValue1Value,
                                                        _feeMatrix.FEESCHED_IDC, (decimal)_feeMatrix.FEESCHED_FEE1NUM, _feeMatrix.FEESCHED_FEE1DEN, _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE,
                                                        //PL 20141017 
                                                        _feeMatrix.FEESCHED_FEE1FORMULABASISSpecified ? (_feeMatrix.FEESCHED_FEE1FORMULABASIS + (_isBracket1Application_Cumulative ? " Value: " + StrFunc.FmtDecimalToInvariantCulture(fee1AssessmentBasisValue) : string.Empty)) : null,
                                                        exchangeMessage, currentFeeValue);
                                                }
                                            }
                                            #endregion FEE1

                                            #region FEE2
                                            if (isOk && _feeMatrix.FEESCHED_FEE2NUMSpecified && _feeMatrix.FEESCHED_FEE2DENSpecified && _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified)
                                            {
                                                coeff = GetProductCoefficient(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE);
                                                feeRate = (decimal)(_feeMatrix.FEESCHED_FEE2NUM / _feeMatrix.FEESCHED_FEE2DEN);

                                                #region Exchange
                                                if (_feeMatrix.FEESCHED_IDC_FEE2Specified && (_feeMatrix.FEESCHED_IDC_FEE2 != currentFeeCurrency))
                                                {
                                                    isOk = _feeMatrix.FEESCHED_FEE2_EXCHTSpecified;
                                                    if (isOk)
                                                    {
                                                        string tmpCurrency = _feeMatrix.FEESCHED_IDC_FEE2;
                                                        isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE2_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                                                    currentFeeCurrency,
                                                                    ref feeRate, ref tmpCurrency, ref exchangeMessage);
                                                        if (!isOk)
                                                            errorMessage = "Value 2 exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_IDC_FEE2 + ").";
                                                    }
                                                    else
                                                    {
                                                        errorMessage = "Value 2 exchange unavailable: the fixing rule does not exist.";
                                                    }
                                                }
                                                #endregion

                                                if (isOk)
                                                {
                                                    decimal fee2Value = 0;

                                                    #region F5CPSBPS
                                                    //PL 20141017
                                                    if (formula == FeeFormulaEnum.F5CPSBPS)
                                                    {
                                                        fee2Value = fee2AssessmentBasisValue * (feeRate * coeff);
                                                    }
                                                    #endregion
                                                    #region Others formula
                                                    else if (_feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE == CommissionDenominationEnum.FixedAmount.ToString())
                                                    {
                                                        fee2Value = (feeRate * coeff);
                                                    }
                                                    #endregion

                                                    //PL 20141023
                                                    if (formula == FeeFormulaEnum.CSHARP)
                                                    {
                                                        dataFees.dataFee[1].DcfNum = 0;
                                                        dataFees.dataFee[1].DcfDen = 1;
                                                        dataFees.dataFee[1].Rate = feeRate * coeff;
                                                        dataFees.dataFee[1].AssessmentBasis = fee2AssessmentBasisValue;
                                                    }
                                                    else
                                                    {
                                                        currentFeeValue += fee2Value;

                                                        //PL 20141017 
                                                        FormatValueForLog(ref stringFormulaValue2Value,
                                                                        _feeMatrix.FEESCHED_IDC, (decimal)_feeMatrix.FEESCHED_FEE2NUM, _feeMatrix.FEESCHED_FEE2DEN, _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE,
                                                                        _feeMatrix.FEESCHED_FEE2FORMULABASISSpecified ? (_feeMatrix.FEESCHED_FEE2FORMULABASIS + (_isBracket1Application_Cumulative ? " Value: " + StrFunc.FmtDecimalToInvariantCulture(fee2AssessmentBasisValue) : string.Empty)) : null,
                                                                        exchangeMessage, fee2Value);
                                                    }
                                                }
                                            }
                                            #endregion FEE2

                                            #region CSHARP
                                            //PL 20141023
                                            if (formula == FeeFormulaEnum.CSHARP)
                                            {
                                                //PL TEST IN PROGRESS...
                                                string codeCSharp = @"(@R1 * @A1) + (@R2 * @A2);";
                                                if (_feeMatrix.FEESCHED_FORMULAXMLSpecified)
                                                    codeCSharp = _feeMatrix.FEESCHED_FORMULAXML;
                                                else if (_feeMatrix.FEESCHED_LOFORMULASpecified)
                                                    codeCSharp = _feeMatrix.FEESCHED_LOFORMULA;

                                                currentFeeValue = CompileCSCAtRuntime.ExecuteFunctionP4(codeCSharp,
                                                    dataFees.dataFee[0].Rate, dataFees.dataFee[0].AssessmentBasis,
                                                    dataFees.dataFee[1].Rate, dataFees.dataFee[1].AssessmentBasis);

                                                FormatValueForLog(ref stringFormulaValue1Value,
                                                    _feeMatrix.FEESCHED_IDC, (decimal)_feeMatrix.FEESCHED_FEE1NUM, _feeMatrix.FEESCHED_FEE1DEN, _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE,
                                                    _feeMatrix.FEESCHED_FEE1FORMULABASISSpecified ? (_feeMatrix.FEESCHED_FEE1FORMULABASIS + (_isBracket1Application_Cumulative ? " Value: " + StrFunc.FmtDecimalToInvariantCulture(fee1AssessmentBasisValue) : string.Empty)) : null,
                                                    exchangeMessage, currentFeeValue);

                                                if (dataFees.dataFee[1].Specified)
                                                    FormatValueForLog(ref stringFormulaValue2Value,
                                                                    _feeMatrix.FEESCHED_IDC, (decimal)_feeMatrix.FEESCHED_FEE2NUM, _feeMatrix.FEESCHED_FEE2DEN, _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE,
                                                                    _feeMatrix.FEESCHED_FEE2FORMULABASISSpecified ? (_feeMatrix.FEESCHED_FEE2FORMULABASIS + (_isBracket1Application_Cumulative ? " Value: " + StrFunc.FmtDecimalToInvariantCulture(fee2AssessmentBasisValue) : string.Empty)) : null,
                                                                    exchangeMessage, currentFeeValue);
                                            }
                                            #endregion CSHARP
                                        }
                                        else
                                        {
                                            errorMessage = "Formula result incorrect: the amount or rate of value 1 does not exist or is incorrect on the repository.";
                                        }
                                    }
                                    #endregion Calculate feeValue from FEE1 and FEE2
                                }
                            }
                        }
                        #endregion F1, F2_Vx, F3MO, F3MOD, F3KO, F3KOD, F4x
                        #region CONSTANT
                        if (formula == FeeFormulaEnum.CONST)
                        {
                            _feeMatrix.CommentForDebug = "Formula " + formula;

                            isOk = _feeMatrix.FEESCHED_FEE1NUMSpecified && _feeMatrix.FEESCHED_FEE1DENSpecified && _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified;
                            if (isOk)
                            {
                                #region FEE1
                                decimal coeff = GetProductCoefficient(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE);
                                decimal feeRate = (decimal)(_feeMatrix.FEESCHED_FEE1NUM / _feeMatrix.FEESCHED_FEE1DEN);
                                currentFeeValue = (feeRate * coeff);

                                FormatValueForLog(ref stringFormulaValue1Value,
                                                _feeMatrix.FEESCHED_IDC, (decimal)_feeMatrix.FEESCHED_FEE1NUM, _feeMatrix.FEESCHED_FEE1DEN, _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE,
                                                    null,
                                                    null, currentFeeValue);
                                #endregion FEE1

                                #region FEE2
                                if (_feeMatrix.FEESCHED_FEE2NUMSpecified && _feeMatrix.FEESCHED_FEE2DENSpecified && _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified)
                                {
                                    if (_feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE == CommissionDenominationEnum.FixedAmount.ToString())
                                    {
                                        coeff = GetProductCoefficient(pCS, pDbTransaction, _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE);
                                        feeRate = (decimal)(_feeMatrix.FEESCHED_FEE2NUM / _feeMatrix.FEESCHED_FEE2DEN);
                                        decimal fee2Value = (feeRate * coeff);
                                        currentFeeValue += fee2Value;

                                        FormatValueForLog(ref stringFormulaValue2Value,
                                                    _feeMatrix.FEESCHED_IDC, (decimal)_feeMatrix.FEESCHED_FEE2NUM, _feeMatrix.FEESCHED_FEE2DEN, _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE,
                                                    null,
                                                    null, fee2Value);
                                    }
                                }
                                #endregion FEE2
                            }
                            else
                            {
                                errorMessage = "Formula result incorrect: the amount or rate of value 1 does not exist or is incorrect on the repository.";
                            }
                        }
                        #endregion CONSTANT
                    }
                    #endregion Formules

                    if (StrFunc.IsEmpty(feeCurrency))
                    {
                        feeCurrency = currentFeeCurrency;
                    }
                    else if (feeCurrency != currentFeeCurrency)
                    {
                        isOk = false;
                        errorMessage = "Formula result incorrect: Multi-Currencies on amount or rate of value.";
                    }
                    if (isOk)
                        feeValue += currentFeeValue;
                }
                #endregion

                #region Min/Max
                if (isOk)
                {
                    if (_feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified)
                    {
                        errorMessage = "Min/Max";

                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        //PL 20191210 [25099] 
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                        int order_NumberOfExecution = 0;
                        decimal order_TotalQty = 0;
                        decimal order_TotalValorisation = 0;
                        decimal order_TotalTheoriticalValorisation = 0;
                        string order_FormulaMinOnFirstExecution = null;
                        string order_FormulaMaxOnFirstExecution = null;

                        if (isFeeScope_FolderId)
                        {
                            //TODO
                        }
                        else if (isFeeScope_OrderId) 
                        {
                            string orderId = Parent.FeeRequest.DataDocument.GetOrderId();
                            //------------------------------------------------------------------------------------------------------------------------
                            //NB: si aucun n° d'ordre sur l'exécution, on considère celle-ci comme seule et unique exécution de l'ordre 
                            //------------------------------------------------------------------------------------------------------------------------
                            if (!string.IsNullOrWhiteSpace(orderId))                        
                            {
                                QueryParameters qry = null; 
                                order_NumberOfExecution = PaymentTools.CountNumberOfTrades(pCS, pDbTransaction, Cst.FeeScopeEnum.OrderId, orderId,
                                                                                           Parent.FeeRequest.TradeInput.CurrentTrade.TradeHeader.TradeDate.DateValue,
                                                                                           ref qry,
                                                                                           _feeMatrix.FEE_EVENTTYPE, feeCurrency,
                                                                                           _feeMatrix.IDFEE, _feeMatrix.IDFEEMATRIX, _feeMatrix.IDFEESCHEDULE,
                                                                                           _feeMatrix.PartyPayer.m_Party_Ida, _feeMatrix.PartyReceiver.m_Party_Ida);
                                if ((order_NumberOfExecution > 1) 
                                    && (Cst.Capture.IsModeNewOrDuplicateOrReflect(Parent.FeeRequest.Mode) 
                                        || 
                                        Cst.Capture.IsModeUpdate(Parent.FeeRequest.Mode))
                                    )
                                {
                                    //Le trade concerné n'est pas la seule exécution relative à l'ordre --> Audit des frais déjà appliqués sur les autres exécutions
                                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                                    {
                                        string[] separators = {"[", "]"};
                                        order_NumberOfExecution = 0;
                                        while (dr.Read())
                                        {
                                            if (Cst.Capture.IsModeUpdate(Parent.FeeRequest.Mode) && Parent.FeeRequest.TradeInput.IdT == Convert.ToInt32(dr["IDT"]))
                                            {
                                                //WARNING: Dans le cas d'un "Recalcul" (ex. appel dans le cas d'une Annulation d'un trade), IdT contient l'Id du Trade en cours de recalcul et "Mode" contient "Update". 
                                                //         On ne considère dasn ce cas que les trades antérieurs à celui en cours de recalcul. 
                                                //         Pour rappel, dans le cas d'un "Recalcul" on procède au recalcul, un à un, de tous les Trades relatif à l'ordre ... 
                                                break;
                                            }
                                            
                                            order_NumberOfExecution++;
                                            order_TotalQty += Convert.ToInt32(dr["QTY"]);
                                            order_TotalValorisation += Convert.ToInt32(dr["VALORISATION"]);
                                            //Ex. de valeur: [EUR 5.00][Unit: FixedAmount][Basis: Quantity][Amount: 40.00]

                                            string[] split = Convert.ToString(dr["FORMULAVALUE1"]).Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                            string amount = split.Where(item => item.StartsWith("Amount: ")).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(amount))
                                                order_TotalTheoriticalValorisation += DecFunc.DecValueFromInvariantCulture(amount.Substring(8));

                                            if (string.IsNullOrEmpty(order_FormulaMinOnFirstExecution))
                                                order_FormulaMinOnFirstExecution = Convert.ToString(dr["FORMULAMIN"]);
                                            if (string.IsNullOrEmpty(order_FormulaMaxOnFirstExecution))
                                                order_FormulaMaxOnFirstExecution = Convert.ToString(dr["FORMULAMAX"]);
                                        }
                                    }
                                }
                            }
                        }
                        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                        decimal minMaxBasisValue = 1;
                        string minMaxBasisCurrency = string.Empty;

                        if (_feeMatrix.FEESCHED_MINMAXBASISSpecified)
                        {
                            errorMessage = "Min/Max basis unavailable: the basis for the Min/Max does not exist.";
                            isOk = Parent.GetAssessmentBasisValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_MINMAXBASIS, ref minMaxBasisValue, ref minMaxBasisCurrency, ref errorMessage);
                            if (isOk)
                            {
                                #region Exchange
                                if (StrFunc.IsFilled(minMaxBasisCurrency) && (minMaxBasisCurrency != _feeMatrix.FEESCHED_IDC))
                                {
                                    isOk = (_feeMatrix.FEESCHED_MINMAXBASIS_EXCHTSpecified);
                                    if (!isOk)
                                        errorMessage = "Min/Max basis exchange unavailable: the fixing rule does not exist.";
                                    else
                                    {
                                        isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                                _feeMatrix.FEESCHED_IDC,
                                                ref minMaxBasisValue, ref minMaxBasisCurrency, ref exchangeMessage);
                                        if (!isOk)
                                            errorMessage = "Min/Max basis exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT + ").";
                                    }
                                }
                                #endregion Exchange
                            }
                        }

                        if (isOk)
                        {
                            //PL 20141017 New: Init from _feeMatrix.FEESCHED_IDC
                            //string idc_MinMaxBasisCurrency = string.Empty;
                            string idc_MinMaxBasisCurrency = _feeMatrix.FEESCHED_IDC;
                            if (_feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified)
                                idc_MinMaxBasisCurrency = _feeMatrix.FEESCHED_IDC_MINMAXBASIS;

                            decimal coeff = GetProductCoefficient(pCS, pDbTransaction, _feeMatrix.FEESCHED_MINMAXEXPRESSIONT);
                            bool isTouched = false;
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            #region MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            if (isOk && (!isTouched) && _feeMatrix.FEESCHED_MINNUMSpecified && _feeMatrix.FEESCHED_MINDENSpecified)
                            {
                                decimal minValue = (decimal)(_feeMatrix.FEESCHED_MINNUM / _feeMatrix.FEESCHED_MINDEN);
                                minValue = minMaxBasisValue * (minValue * coeff);

                                #region Exchange
                                if (StrFunc.IsFilled(idc_MinMaxBasisCurrency) && (idc_MinMaxBasisCurrency != _feeMatrix.FEESCHED_IDC))
                                {
                                    isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                                _feeMatrix.FEESCHED_IDC,
                                                ref minValue, ref idc_MinMaxBasisCurrency, ref exchangeMessage);
                                    if (!isOk)
                                        errorMessage = "Min/Max basis exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT + ").";

                                    //Warning: Reinit de idc_MinMaxBasisCurrency pour la suite (maxValue)
                                    idc_MinMaxBasisCurrency = _feeMatrix.FEESCHED_IDC_MINMAXBASIS;
                                }
                                #endregion Exchange

                                FormatValueForLog(ref stringFormulaMinValue, idc_MinMaxBasisCurrency, (decimal)_feeMatrix.FEESCHED_MINNUM, _feeMatrix.FEESCHED_MINDEN, _feeMatrix.FEESCHED_MINMAXEXPRESSIONT,
                                    _feeMatrix.FEESCHED_MINMAXBASISSpecified ? _feeMatrix.FEESCHED_MINMAXBASIS : null,
                                    exchangeMessage, null);

                                //------------------------------------------------------------------------------------------------------------------------------
                                bool isBreakMin = false;
                                if (order_NumberOfExecution > 0)
                                {
                                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                    //PL 20191210 [25099] 
                                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                    //Si order_NumberOfExecution=0, alors le trade concerné est actuellement l'unique exécution relative à l'ordre --> Application classique du barème
                                    //Sinon:
                                    if (order_TotalValorisation < minValue)
                                    {
                                        //Total sur les autres exécutions inférieur au MIN --> /WARNING: cas théoriquement impossible !
                                        //GLOP25099 TODO Gérer ERROR 
                                    }
                                    else if (order_TotalValorisation > minValue)
                                    {
                                        //Total sur les autres exécutions supérieur au MIN --> Pas d'application du MIN sur le trade concerné
                                        minValue = feeValue;
                                    }
                                    else //order_TotalValorisation = minValue
                                    {
                                        if ((order_NumberOfExecution == 1) && (order_FormulaMinOnFirstExecution.IndexOf(Cst.TOUCHED_NO) > 0))
                                        {
                                            //Il existe une seule autre exécution et son montant est identique au MIN --> Pas d'application du MIN sur le trade concerné
                                            minValue = feeValue;
                                        }
                                        else
                                        {
                                            isBreakMin = true;

                                            decimal totalValorisation = order_TotalTheoriticalValorisation + feeValue;
                                            if (totalValorisation > minValue)
                                            {
                                                //Le montant MIN est déjà appliqué sur une autre exécution et le total, incluant cette exécution, n'atteint pas le MIN --> Frais égales à la DIFF sur le trade concerné
                                                stringFormulaMinValue[_currentBraket] += String.Format(CALCAMOUNT, StrFunc.FmtMoneyToCurrentCulture(feeValue, feeCurrency));
                                                feeValue = (totalValorisation - minValue);
                                                stringFormulaMinValue[_currentBraket] += Cst.TOUCHED_YESPARTIALLY;
                                            }
                                            else
                                            {
                                                //Le montant MIN est déjà appliqué sur une autre excéution et le total, incluant cette exécution, n'atteint pas le MIN --> Frais à ZERO sur le trade concerné
                                                stringFormulaMinValue[_currentBraket] += String.Format(CALCAMOUNT, StrFunc.FmtMoneyToCurrentCulture(feeValue, feeCurrency));
                                                feeValue = 0;
                                                stringFormulaMinValue[_currentBraket] += Cst.TOUCHED_YESALREADY;
                                            }
                                        }
                                    }
                                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                                }
                                if (!isBreakMin)
                                {
                                    decimal finalValue = Math.Max(minValue, feeValue);
                                    isTouched = (finalValue != feeValue);
                                    if (isTouched)
                                    {
                                        stringFormulaMinValue[_currentBraket] += String.Format(CALCAMOUNT, StrFunc.FmtMoneyToCurrentCulture(feeValue, feeCurrency));
                                        feeValue = finalValue;
                                    }
                                    stringFormulaMinValue[_currentBraket] += (isTouched ? Cst.TOUCHED_YES : Cst.TOUCHED_NO);
                                }
                                //------------------------------------------------------------------------------------------------------------------------------
                            }
                            #endregion MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN --- MIN

                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            #region MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX
                            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                            if (isOk && (!isTouched) && _feeMatrix.FEESCHED_MAXNUMSpecified && _feeMatrix.FEESCHED_MAXDENSpecified)
                            {
                                decimal maxValue = (decimal)(_feeMatrix.FEESCHED_MAXNUM / _feeMatrix.FEESCHED_MAXDEN);
                                maxValue = minMaxBasisValue * (maxValue * coeff);

                                #region Exchange
                                if (StrFunc.IsFilled(idc_MinMaxBasisCurrency) && (idc_MinMaxBasisCurrency != _feeMatrix.FEESCHED_IDC))
                                {
                                    isOk = Parent.GetExchangeValue(pCS, pDbTransaction, _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT, _feeMatrix.PartyPayer, _feeMatrix.PartyReceiver, feeFirstDate,
                                                _feeMatrix.FEESCHED_IDC,
                                                ref maxValue, ref idc_MinMaxBasisCurrency, ref exchangeMessage);
                                    if (!isOk)
                                        errorMessage = "Min/Max basis exchange unavailable: the fixing rate is unavailable (fixing " + exchangeMessage + " rule: " + _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT + ").";
                                }
                                #endregion Exchange

                                FormatValueForLog(ref stringFormulaMaxValue, idc_MinMaxBasisCurrency, (decimal)_feeMatrix.FEESCHED_MAXNUM, _feeMatrix.FEESCHED_MAXDEN, _feeMatrix.FEESCHED_MINMAXEXPRESSIONT,
                                    _feeMatrix.FEESCHED_MINMAXBASISSpecified ? _feeMatrix.FEESCHED_MINMAXBASIS : null,
                                    exchangeMessage, null);

                                decimal finalValue = Math.Min(maxValue, feeValue);
                                isTouched = (finalValue != feeValue);
                                if (isTouched)
                                {
                                    stringFormulaMaxValue[_currentBraket] += String.Format(CALCAMOUNT, StrFunc.FmtMoneyToCurrentCulture(feeValue, feeCurrency));
                                    feeValue = finalValue;
                                }
                                stringFormulaMaxValue[_currentBraket] += (isTouched ? Cst.TOUCHED_YES : Cst.TOUCHED_NO);
                            }
                            #endregion MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX --- MAX
                        }
                    }
                }
                #endregion Min/Max

                #region Set Payment data
                if (isOk)
                {
                    _payment[currentPayment] = Parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreatePayment();
                    IPayment payment = _payment[currentPayment];
                    //Payer\Receiver
                    payment.PayerPartyReference.HRef = _feeMatrix.PartyPayer.m_Party_Href;
                    payment.ReceiverPartyReference.HRef = _feeMatrix.PartyReceiver.m_Party_Href;
                    //paymentType
                    payment.PaymentTypeSpecified = true;
                    payment.PaymentType.Value = _feeMatrix.FEE_PAYMENTTYPE;
                    //paymentAmount
                    EFS_Cash cash = null;
                    if (_feeMatrix.FEESCHED_ROUNDPRECSpecified && _feeMatrix.FEESCHED_ROUNDDIRSpecified)
                    {
                        cash = new EFS_Cash(pCS, pDbTransaction, feeValue, new CurrencyCashInfo(1, _feeMatrix.FEESCHED_ROUNDPREC, _feeMatrix.FEESCHED_ROUNDDIR));
                        //PL 20231218 [WI786] Ne plus appliquer la règle de la devise, afin de conserver, si besoin, un nombre de décimal supérieur, dans le cas de frais facturés au sein d'une facture mensuelle (ex. TRADITION).
                        //cash = new EFS_Cash(pCS, pDbTransaction, cash.AmountRounded, feeCurrency);
                    }
                    else
                    {
                        cash = new EFS_Cash(pCS, pDbTransaction, feeValue, feeCurrency);
                    }
                    payment.PaymentAmount.Amount.DecValue = cash.AmountRounded;
                    payment.PaymentAmount.Currency = feeCurrency;
                    //paymentQuote GLOP
                    payment.PaymentQuoteSpecified = false;
                    //payment.paymentQuote.percentageRateFractionSpecified = false;
                    //payment.paymentQuote.paymentRelativeTo
                    //paymentDate
                    payment.PaymentDateSpecified = true;
                    payment.PaymentDate.UnadjustedDate.DateValue = currentPeriod.date2;
                    payment.PaymentDate.DateAdjustments.BusinessCentersNoneSpecified = true;
                    payment.PaymentDate.DateAdjustments.BusinessCentersReferenceSpecified = false;
                    payment.PaymentDate.DateAdjustments.BusinessCentersDefineSpecified = false;
                    payment.PaymentDate.DateAdjustments.BusinessDayConvention = BusinessDayConventionEnum.NONE;
                    //settlementInformation
                    payment.SettlementInformationSpecified = false;
                    //paymentSource
                    payment.PaymentSourceSpecified = true;
                    //
                    payment.PaymentSource.StatusSpecified = true;
                    payment.PaymentSource.Status = EfsML.Enum.SpheresSourceStatusEnum.Default;

                    #region spheresId
                    int nb_spheresId = 14; //PL 20191210 [25099]
                    foreach (string tmpString in stringFormulaValue2Value)
                    {
                        if (StrFunc.IsFilled(tmpString))
                            nb_spheresId++;
                    }
                    if (StrFunc.IsFilled(stringDCFValue))
                        nb_spheresId++;

                    if (_feeMatrix.TAXAPPLICATIONSpecified)
                        nb_spheresId++;

                    if (fee2AssessmentBasisSpecified)
                        nb_spheresId++;

                    if (isBracket1)
                    {
                        nb_spheresId++;
                        if (_isBracket1Application_Cumulative)
                        {
                            nb_spheresId += (countBraket - 1);

                            if (_isBracket1Application_Periodic)
                                nb_spheresId++;
                        }
                    }
                    payment.PaymentSource.SpheresId = Parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateSpheresId(nb_spheresId);
                    ISpheresSource source = payment.PaymentSource;
                    nb_spheresId = -1;
                    //Infos sur la ligne de la "matrice" à l'origine de ce paiement
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeMatrixScheme;
                    source.SpheresId[nb_spheresId].OTCmlId = _feeMatrix.IDFEEMATRIX;
                    source.SpheresId[nb_spheresId].Value = _feeMatrix.IDENTIFIER;
                    //Infos sur le "type de frais" à l'origine de ce paiement
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeScheme;
                    source.SpheresId[nb_spheresId].OTCmlId = _feeMatrix.IDFEE;
                    source.SpheresId[nb_spheresId].Value = _feeMatrix.FEE_IDENTIFIER;
                    // EG 20100505 Tax
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeEventTypeScheme;
                    source.SpheresId[nb_spheresId].Value = _feeMatrix.FEE_EVENTTYPE;
                    //Infos sur le "barème" à l'origine de ce paiement
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeScheduleScheme;
                    source.SpheresId[nb_spheresId].OTCmlId = _feeMatrix.IDFEESCHEDULE;
                    source.SpheresId[nb_spheresId].Value = _feeMatrix.FEESCHED_IDENTIFIER;
                    //Infos sur le "scope" relatif à ce paiement //PL 20191210 [25099]
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedScopeScheme;
                    source.SpheresId[nb_spheresId].Value = _feeMatrix.FEESCHED_SCOPE;
                    //Infos sur le "tarif" à l'origine de ce paiement
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedFormulaScheme;
                    source.SpheresId[nb_spheresId].Value = formula.ToString();
                    //
                    if (StrFunc.IsFilled(stringDCFValue))
                    {
                        nb_spheresId++;
                        source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedFormulaDCFScheme;
                        source.SpheresId[nb_spheresId].Value = stringDCFValue.Trim();
                    }
                    //Infos sur les "éventuelles tranches" à l'origine de ce paiement
                    if (isBracket1)
                    {
                        nb_spheresId++;
                        source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedBracket1Scheme;
                        source.SpheresId[nb_spheresId].Value = stringFormulaBracket1Value.Trim();
                    }
                    //
                    string scheme_suffix;
                    for (int i = 0; i < stringFormulaValue1Value.Length; i++)
                    {
                        //Warning: Suffixe "_Bracket" exploité dans la méthode CalcFeeRestitution(...) 
                        scheme_suffix = (_isBracket1Application_Cumulative ? "_Bracket" + (i + 1).ToString() : string.Empty);
                        //
                        nb_spheresId++;
                        source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedFormulaValue1Scheme + scheme_suffix;
                        source.SpheresId[nb_spheresId].Value = stringFormulaValue1Value[i].Trim();
                        if (StrFunc.IsFilled(stringFormulaValue2Value[i]))
                        {
                            nb_spheresId++;
                            source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedFormulaValue2Scheme + scheme_suffix;
                            source.SpheresId[nb_spheresId].Value = stringFormulaValue2Value[i].Trim();
                        }
                    }
                    //
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedFormulaMinScheme;
                    //PL 20150313 use Math.Max()
                    //source.spheresId[nb_spheresId].Value = stringFormulaMinValue[0].Trim();
                    source.SpheresId[nb_spheresId].Value = stringFormulaMinValue[Math.Max(0, _currentBraket)].Trim();
                    //
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedFormulaMaxScheme;
                    //PL 20150313 use Math.Max()
                    //source.spheresId[nb_spheresId].Value = stringFormulaMaxValue[0].Trim();
                    source.SpheresId[nb_spheresId].Value = stringFormulaMaxValue[Math.Max(0, _currentBraket)].Trim();
                    //
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeInvoicingScheme;
                    source.SpheresId[nb_spheresId].Value = isFeeInvoiced ? "true" : "false";
                    //
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeePaymentFrequencyScheme;
                    source.SpheresId[nb_spheresId].Value = (stringInvoicingAValue.Trim() + " " + stringInvoicingBValue.Trim()).Trim();
                    //
                    nb_spheresId++;
                    source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue1Scheme;
                    source.SpheresId[nb_spheresId].Value = StrFunc.FmtDecimalToInvariantCulture(fee1AssessmentBasisDecValue);
                    if (fee2AssessmentBasisSpecified)
                    {
                        nb_spheresId++;
                        source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedAssessmentBasisValue2Scheme;
                        source.SpheresId[nb_spheresId].Value = StrFunc.FmtDecimalToInvariantCulture(fee2AssessmentBasisDecValue);
                    }
                    if (_isBracket1Application_Periodic)
                    {
                        nb_spheresId++;
                        //source.spheresId[nb_spheresId].scheme = Cst.OTCml_RepositoryFeeSchedPeriodBasisValueScheme;
                        //source.spheresId[nb_spheresId].Value = StrFunc.FmtDecimalToInvariantCulture(_periodBasisValue + fee1AssessmentBasisDecValue);
                        source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeSchedPeriodCharacteristicsScheme;
                        source.SpheresId[nb_spheresId].Value = _periodCharacteristics;
                    }
                    if (_feeMatrix.TAXAPPLICATIONSpecified)
                    {
                        nb_spheresId++;
                        source.SpheresId[nb_spheresId].Scheme = Cst.OTCml_RepositoryFeeTaxApplicationScheme;
                        source.SpheresId[nb_spheresId].Value = _feeMatrix.TAXAPPLICATION;
                    }
                    #endregion spheresId

                    if (currentPayment == 0)
                    {
                        string msgMinMaxInfo = Cst.Space;
                        string msgAdditionalInfo, msgMinMaxAddInfo;
                        msgAdditionalInfo = msgMinMaxAddInfo = string.Empty;

                        string MinMaxInfo(string x) => " " + Ressource.GetString(x) + " ";
                        string MinMaxInfoDet(string x) => " " + Ressource.GetString2("Msg_FeeInformation_MinMaxDet",
                            x.Substring(x.IndexOf(CALCAMOUNT_Begin) + CALCAMOUNT_Begin.Length,
                            x.IndexOf("]", x.IndexOf(CALCAMOUNT_Begin)) - (x.IndexOf(CALCAMOUNT_Begin) + CALCAMOUNT_Begin.Length)));

                        #region MIN Touched
                        if (stringFormulaMinValue[0].IndexOf(Cst.TOUCHED_YES) > 0) 
                        {
                            msgMinMaxInfo = MinMaxInfo("Msg_FeeInformation_Min");
                            msgAdditionalInfo = MinMaxInfoDet(stringFormulaMinValue[0]);
                        }
                        else if (stringFormulaMinValue[0].IndexOf(Cst.TOUCHED_YESALREADY) > 0) //PL 20191210 [25099] 
                        {
                            msgMinMaxAddInfo = MinMaxInfo("Msg_FeeInformation_MinAlreadyApply").TrimStart(' ');
                            msgAdditionalInfo = MinMaxInfoDet(stringFormulaMinValue[0]);
                        }
                        else if (stringFormulaMinValue[0].IndexOf(Cst.TOUCHED_YESPARTIALLY) > 0) 
                        {
                            msgMinMaxAddInfo = MinMaxInfo("Msg_FeeInformation_MinPartiallyApply").TrimStart(' ');
                            msgAdditionalInfo = MinMaxInfoDet(stringFormulaMinValue[0]);
                        }
                        #endregion MIN Touched
                        #region MAX Touched
                        else if (stringFormulaMaxValue[0].IndexOf(Cst.TOUCHED_YES) > 0)
                        {
                            msgMinMaxInfo = MinMaxInfo("Msg_FeeInformation_Max");
                            msgAdditionalInfo = MinMaxInfoDet(stringFormulaMaxValue[0]);
                        }
                        else if (stringFormulaMaxValue[0].IndexOf(Cst.TOUCHED_YESALREADY) > 0)
                        { 
                            //TODO
                            msgMinMaxAddInfo = MinMaxInfo("Msg_FeeInformation_MaxAlreadyApply");
                            msgAdditionalInfo = MinMaxInfoDet(stringFormulaMaxValue[0]);
                        }
                        else if (stringFormulaMaxValue[0].IndexOf(Cst.TOUCHED_YESPARTIALLY) > 0)
                        {
                            //TODO
                            msgMinMaxAddInfo = MinMaxInfo("Msg_FeeInformation_MaxPartiallyApply");
                            msgAdditionalInfo = MinMaxInfoDet(stringFormulaMaxValue[0]);
                        }
                        #endregion MAX Touched
                        #region Periodic Fee
                        if (isFeePeriodic)
                        {
                            string res1 = Ressource.GetString("PERIODMLTP");
                            string res2 = Ressource.GetString(_feeMatrix.PERIOD);
                            msgAdditionalInfo += Cst.CrLf + StrFunc.AppendFormat("    [{0}: {1} {2}" + " Roll: {3}]", res1, _feeMatrix.PERIODMLTP, res2, rollConvention.ToString());
                        }
                        #endregion Periodic Fee
                        #region Invoiced Fee
                        if (isFeeInvoiced)
                        {
                            string res1 = Ressource.GetString("ISINVOICING").Split(';')[0];
                            string res2 = Ressource.GetString("Yes");
                            msgAdditionalInfo += Cst.CrLf + StrFunc.AppendFormat("    [{0}: {1}]", res1, res2);
                        }
                        #endregion Invoiced Fee
                        
                        _infoMessage = _eventCode.ToString() + @" \ " + headerMessage + Cst.CrLf + "    ";
                        _infoMessage += Ressource.GetString2("Msg_FeeInformation", DtFunc.DateTimeToString(currentPeriod.date2, DtFunc.FmtShortDate),
                            _feeMatrix.PartyPayer.m_Party_PartyId, _feeMatrix.PartyReceiver.m_Party_PartyId,
                            msgMinMaxInfo,
                            StrFunc.FmtMoneyToCurrentCulture(payment.PaymentAmount.Amount.DecValue, payment.PaymentAmount.Currency),
                            msgMinMaxAddInfo,
                            msgAdditionalInfo);

                        System.Diagnostics.Debug.WriteLine(_infoMessage);
                    }
                }
                #endregion Set Payment data
            }
            #endregion Calcul du ou des montant des frais et sauvegarde

            #region Set Status and Log message
            if (isOk)
            {
                _levelEnum = (feeValue == 0 ? LevelStatusTools.LevelEnum.WARNING : LevelStatusTools.LevelEnum.INFO);
                _statusEnum = LevelStatusTools.StatusEnum.SUCCESS;
            }
            //PL 20130508 *******************************************************************************************************
            //else if (isSpecificScheduleDiscard)
            //{
            //    _levelEnum = LevelStatusTools.LevelEnum.WARNING;
            //    _statusEnum = LevelStatusTools.StatusEnum.SUCCESS;
            //    _feeMatrix.CommentForDebug = "Specific schedule rulet out." + Cst.CrLf + "[" + errorMessage + "]";
            //    _discardMessage = headerMessage + Cst.CrLf + "    " + _feeMatrix.CommentForDebug;

            //    _payment = null;
            //}
            //PL 20130508 *******************************************************************************************************
            // EG 20150713 Report PL
            else if (_isBracket1_NoBracket) //PL 20150709
            {
                _levelEnum = (LevelStatusTools.LevelEnum.INFO);
                _statusEnum = LevelStatusTools.StatusEnum.SUCCESS;

                _payment = null;
            }
            else
            {
                _feeMatrix.CommentForDebug = "Error on formula " + formula + Cst.CrLf + "[" + errorMessage + "]";
                _errorMessage = headerMessage + Cst.CrLf + "    " + _feeMatrix.CommentForDebug;
                _levelEnum = LevelStatusTools.LevelEnum.ALERT;
                _statusEnum = LevelStatusTools.StatusEnum.ERROR;

                _payment = null;
            }
            #endregion Set Status and Log message
        }
        #endregion

        #region GetDeliveryDate
        /// <summary>
        /// Retourne la "DeliveryDate" en vigueur pour l'Asset ETD présent sur le Trade.
        /// <para>Retourne la date de règlement sur DebtSecurityTransaction et EquirySecurityTansaction </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <returns></returns>
        /// EG 20150618 [21124] 
        /// EG 20180205 [23769] Upd DataHelper.ExecuteScalar
        /// EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        /// EG 20180307 [23769] Gestion dbTransaction
        /// FI 20190520 [XXXXX] Getion de isDebtSecurityTransaction
        private DateTime GetDeliveryDate(string pCS, IDbTransaction pDbTransaction)
        {
            DateTime ret = DateTime.MinValue;

            if ((_parent.FeeRequest.DataDocument != null) && (_parent.FeeRequest.DataDocument.CurrentProduct != null))
            {
                // EG 20150402 [POC] Add CS parameter to read FXRateAsset default for FX
                if (Parent.FeeRequest.Product.IsExchangeTradedDerivative)
                {
                    #region EquitySecurityTransaction
                    Nullable<int> idAsset = _parent.FeeRequest.DataDocument.CurrentProduct.GetUnderlyingAssetId(pCS);
                    if (idAsset.HasValue)
                    {
                        DataParameters parameters = new DataParameters();
                        parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), idAsset.Value);

                        string sqlSelect = @"select ma.DELIVERYDATE
                        from dbo.ASSET_ETD ass
                        inner join dbo.DERIVATIVEATTRIB da on da.IDDERIVATIVEATTRIB=ass.IDDERIVATIVEATTRIB
                        inner join dbo.MATURITY ma on ma.IDMATURITY=da.IDMATURITY
                        where ass.IDASSET=@IDASSET";

                        QueryParameters qry = new QueryParameters(pCS, sqlSelect, parameters);
                        Object obj = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                        if (obj != null)
                            ret = Convert.ToDateTime(obj);
                    }
                    #endregion
                }
                else if (Parent.FeeRequest.Product.IsEquitySecurityTransaction)
                {
                    #region EquitySecurityTransaction
                    IEquitySecurityTransaction eqs = (IEquitySecurityTransaction)Parent.FeeRequest.Product.Product;

                    //PL 20150708 Utilisation prioritairement de la date du Gross Amount (GAM). See also [TRIM 21162].
                    EquitySecurityTransactionContainer equitySecurityTransaction = new EquitySecurityTransactionContainer(pCS, eqs);
                    EFS_Payment payment = new EFS_Payment(pCS, pDbTransaction, equitySecurityTransaction.GrossAmount, _parent.DataDocument);
                    ret = payment.AdjustedPaymentDate.DateValue;

                    if (DtFunc.IsDateTimeEmpty(ret))
                    {
                        ret = _parent.DataDocument.CurrentProduct.GetBusinessDate();

                        // EG 20150618 [21124] New Application SettlementOffset (asset Equity) sur la date reference (ClearingBusinessDate)
                        Nullable<int> idAsset = _parent.FeeRequest.DataDocument.CurrentProduct.GetUnderlyingAssetId(pCS);
                        if (idAsset.HasValue)
                        {
                            SQL_AssetEquity sql_Asset = new SQL_AssetEquity(pCS, idAsset.Value);
                            if (null != pDbTransaction)
                                sql_Asset.DbTransaction = pDbTransaction;
                            if (sql_Asset.IsLoaded)
                            {
                                if (sql_Asset.StlOffsetMultiplier.HasValue && StrFunc.IsFilled(sql_Asset.StlOffsetPeriod) && StrFunc.IsFilled(sql_Asset.StlOffsetDaytype))
                                {
                                    PeriodEnum period = (FpML.Enum.PeriodEnum)Enum.Parse(typeof(FpML.Enum.PeriodEnum), sql_Asset.StlOffsetPeriod, true);
                                    DayTypeEnum dayType = (FpML.Enum.DayTypeEnum)Enum.Parse(typeof(FpML.Enum.DayTypeEnum), sql_Asset.StlOffsetDaytype, true);
                                    IOffset offset = _parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateOffset(period, sql_Asset.StlOffsetMultiplier.Value, dayType);
                                    IBusinessCenters businessCenters = null;
                                    if (StrFunc.IsFilled(sql_Asset.Market_IDBC))
                                        businessCenters = _parent.FeeRequest.DataDocument.CurrentProduct.ProductBase.CreateBusinessCenters(sql_Asset.Market_IDBC);
                                    // Application offset sur ClearingBusinessDate
                                    if ((null != offset) && (null != businessCenters))
                                    {
                                        ret = Tools.ApplyOffset(pCS, ret, offset, businessCenters);
                                    }
                                }
                            }
                        }
                    }
                    #endregion EquitySecurityTransaction
                }
                else if (Parent.FeeRequest.Product.IsDebtSecurityTransaction)
                {
                    IDebtSecurityTransaction dst = (IDebtSecurityTransaction)Parent.FeeRequest.Product.Product;

                    DebtSecurityTransactionContainer dstContainer = new DebtSecurityTransactionContainer(pCS, pDbTransaction, dst, _parent.DataDocument);
                    EFS_Payment payment = new EFS_Payment(pCS, pDbTransaction, dstContainer.DebtSecurityTransaction.GrossAmount, _parent.DataDocument);
                    ret = payment.AdjustedPaymentDate.DateValue;

                    if (DtFunc.IsDateTimeEmpty(ret))
                        ret = _parent.DataDocument.CurrentProduct.GetBusinessDate2();
                }
            }
            return ret;
        }
        #endregion
        #region GetProductCoefficient
        // EG 20180307 [23769] Gestion dbTransaction
        private decimal GetProductCoefficient(string pCS, IDbTransaction pDbTransaction, string pFeeExpressionType)
        {
            //PL 20100316 TBD isStrategy
            string CS = CSTools.SetCacheOn(pCS);
            decimal ret = GetCoefficient(pFeeExpressionType);

            //PL/FL 20100323 ReAdd Test CentsPerShare
            // RD 20110429
            // Utilisation de la nouvelle méthode GetContractMultiplier() 
            // pour la Multiplication du coef par la quotité du contrat
            if (pFeeExpressionType == CommissionDenominationEnum.CentsPerShare.ToString())
                ret *= Parent.FeeRequest.Product.GetContractMultiplier(CS, pDbTransaction);
            return ret;
        }
        #endregion
        #region FormatDCFValue
        /// <summary>
        /// Formatage du DCF pour un log.
        /// </summary>
        /// <param name="pDcfNum"></param>
        /// <param name="pDcfDen"></param>
        /// <param name="pDCFValue"></param>
        private void FormatDCFValue(int pDcfNum, decimal pDcfDen, ref string pDCFValue)
        {
            pDCFValue += (String.IsNullOrEmpty(pDCFValue) ? @" " : @"+") + pDcfNum.ToString() + @"/";
            if (decimal.Truncate(pDcfDen) == pDcfDen)
                pDCFValue += decimal.Truncate(pDcfDen).ToString();
            else
                pDCFValue += StrFunc.FmtDecimalToInvariantCulture(pDcfDen);
        }
        #endregion
        #region private GetCoefficient
        /// <summary>
        /// Return the coefficient 
        /// </summary>
        /// <param name="pFeeExpressionType"></param>
        /// <returns></returns>
        private static decimal GetCoefficient(string pFeeExpressionType)
        {
            decimal coeff = 1;
            if (pFeeExpressionType == CommissionDenominationEnum.BPS.ToString())
                coeff = 1 / (Convert.ToDecimal(10000));//glop base 32, base 64, ...
            else if (pFeeExpressionType == CommissionDenominationEnum.Percentage.ToString())
                coeff = 1 / (Convert.ToDecimal(100));
            else if (pFeeExpressionType == CommissionDenominationEnum.CentsPerShare.ToString())
                coeff = 1 / (Convert.ToDecimal(100));
            else if (pFeeExpressionType == CommissionDenominationEnum.FixedAmount.ToString())
                coeff = 1;
            return coeff;
        }
        #endregion GetCoefficient
        #region private FormatValueForLog
        private void FormatValueForLog(ref string[] pString, string pIdc, decimal pNum, int pDen, string pExpressionType,
            string pBasis, string pExchangeMessage, Nullable<decimal> pAmount)
        {
            string ret = "[" + pIdc;
            if (pDen == 1)
            {
                ret += " " + StrFunc.FmtDecimalToInvariantCulture(pNum);
            }
            else
            {
                if (decimal.Truncate(pNum) == pNum)
                    ret += " " + decimal.Truncate(pNum).ToString();
                else
                    ret += " " + StrFunc.FmtDecimalToInvariantCulture(pNum);

                ret += @"/" + pDen.ToString();
            }
            ret += "][Unit: " + pExpressionType + "]";

            if (StrFunc.IsFilled(pBasis))
                ret += "[Basis: " + pBasis + "]";
            if (StrFunc.IsFilled(pExchangeMessage))
                ret += "[Exchange: " + pExchangeMessage + "]";
            if (pAmount != null)
                ret += "[Amount: " + StrFunc.FmtDecimalToInvariantCulture(Convert.ToDecimal(pAmount)) + "]";

            //PL 20141017 Add Test on Cst.NotAvailable
            if (pString[_currentBraket] == Cst.NotAvailable)
                pString[_currentBraket] = ret;
            else
                pString[_currentBraket] += ret;
        }
        #endregion FormatValueForLog
        #region private SaveFeeMatrixOriginalValues
        /// <summary>
        /// Sauvegarde des valeurs d'origine de FEESCHEDULE pour restitution ultérieure si nécessaire, dans LoadValuesFromBracket1()
        /// </summary>
        private void SaveFeeMatrixOriginalValues()
        {
            _feeMatrixOriginalValues = new FeeMatrix();
            //
            #region FEE1
            _feeMatrixOriginalValues.FEESCHED_FEE1NUMSpecified = _feeMatrix.FEESCHED_FEE1NUMSpecified;
            if (_feeMatrix.FEESCHED_FEE1NUMSpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE1NUM = _feeMatrix.FEESCHED_FEE1NUM;
            _feeMatrixOriginalValues.FEESCHED_FEE1DENSpecified = _feeMatrix.FEESCHED_FEE1DENSpecified;
            if (_feeMatrix.FEESCHED_FEE1DENSpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE1DEN = _feeMatrix.FEESCHED_FEE1DEN;
            _feeMatrixOriginalValues.FEESCHED_FEE1EXPRESSIONTYPESpecified = _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified;
            if (_feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE1EXPRESSIONTYPE = _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE;
            _feeMatrixOriginalValues.FEESCHED_IDC_FEE1Specified = _feeMatrix.FEESCHED_IDC_FEE1Specified;
            if (_feeMatrix.FEESCHED_IDC_FEE1Specified)
                _feeMatrixOriginalValues.FEESCHED_IDC_FEE1 = _feeMatrix.FEESCHED_IDC_FEE1;
            _feeMatrixOriginalValues.FEESCHED_FEE1_EXCHTSpecified = _feeMatrix.FEESCHED_FEE1_EXCHTSpecified;
            if (_feeMatrix.FEESCHED_FEE1_EXCHTSpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE1_EXCHT = _feeMatrix.FEESCHED_FEE1_EXCHT;
            #endregion FEE1
            #region FEE2
            _feeMatrixOriginalValues.FEESCHED_FEE2NUMSpecified = _feeMatrix.FEESCHED_FEE2NUMSpecified;
            if (_feeMatrix.FEESCHED_FEE2NUMSpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE2NUM = _feeMatrix.FEESCHED_FEE2NUM;
            _feeMatrixOriginalValues.FEESCHED_FEE2DENSpecified = _feeMatrix.FEESCHED_FEE2DENSpecified;
            if (_feeMatrix.FEESCHED_FEE2DENSpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE2DEN = _feeMatrix.FEESCHED_FEE2DEN;
            _feeMatrixOriginalValues.FEESCHED_FEE2EXPRESSIONTYPESpecified = _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified;
            if (_feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE2EXPRESSIONTYPE = _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE;
            _feeMatrixOriginalValues.FEESCHED_IDC_FEE2Specified = _feeMatrix.FEESCHED_IDC_FEE2Specified;
            if (_feeMatrix.FEESCHED_IDC_FEE2Specified)
                _feeMatrixOriginalValues.FEESCHED_IDC_FEE2 = _feeMatrix.FEESCHED_IDC_FEE2;
            _feeMatrixOriginalValues.FEESCHED_FEE2_EXCHTSpecified = _feeMatrix.FEESCHED_FEE2_EXCHTSpecified;
            if (_feeMatrix.FEESCHED_FEE2_EXCHTSpecified)
                _feeMatrixOriginalValues.FEESCHED_FEE2_EXCHT = _feeMatrix.FEESCHED_FEE2_EXCHT;
            #endregion FEE2
            #region MINNUM / MAXNUM
            _feeMatrixOriginalValues.FEESCHED_MINNUMSpecified = _feeMatrix.FEESCHED_MINNUMSpecified;
            if (_feeMatrix.FEESCHED_MINNUMSpecified)
                _feeMatrixOriginalValues.FEESCHED_MINNUM = _feeMatrix.FEESCHED_MINNUM;
            _feeMatrixOriginalValues.FEESCHED_MINDENSpecified = _feeMatrix.FEESCHED_MINDENSpecified;
            if (_feeMatrix.FEESCHED_MINDENSpecified)
                _feeMatrixOriginalValues.FEESCHED_MINDEN = _feeMatrix.FEESCHED_MINDEN;
            //
            _feeMatrixOriginalValues.FEESCHED_MAXNUMSpecified = _feeMatrix.FEESCHED_MAXNUMSpecified;
            if (_feeMatrix.FEESCHED_MAXNUMSpecified)
                _feeMatrixOriginalValues.FEESCHED_MAXNUM = _feeMatrix.FEESCHED_MAXNUM;
            _feeMatrixOriginalValues.FEESCHED_MAXDENSpecified = _feeMatrix.FEESCHED_MAXDENSpecified;
            if (_feeMatrix.FEESCHED_MAXDENSpecified)
                _feeMatrixOriginalValues.FEESCHED_MAXDEN = _feeMatrix.FEESCHED_MAXDEN;
            //
            _feeMatrixOriginalValues.FEESCHED_MINMAXEXPRESSIONTSpecified = _feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified;
            if (_feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified)
                _feeMatrixOriginalValues.FEESCHED_MINMAXEXPRESSIONT = _feeMatrix.FEESCHED_MINMAXEXPRESSIONT;
            _feeMatrixOriginalValues.FEESCHED_MINMAXBASISSpecified = _feeMatrix.FEESCHED_MINMAXBASISSpecified;
            if (_feeMatrix.FEESCHED_MINMAXBASISSpecified)
                _feeMatrixOriginalValues.FEESCHED_MINMAXBASIS = _feeMatrix.FEESCHED_MINMAXBASIS;
            _feeMatrixOriginalValues.FEESCHED_IDC_MINMAXBASISSpecified = _feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified;
            if (_feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified)
                _feeMatrixOriginalValues.FEESCHED_IDC_MINMAXBASIS = _feeMatrix.FEESCHED_IDC_MINMAXBASIS;
            _feeMatrixOriginalValues.FEESCHED_MINMAXEXPRESSIONTSpecified = _feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified;
            if (_feeMatrix.FEESCHED_MINMAXBASIS_EXCHTSpecified)
                _feeMatrixOriginalValues.FEESCHED_MINMAXBASIS_EXCHT = _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT;
            #endregion MINNUM / MAXNUM
        }
        #endregion
        #region private LoadValuesFromBracket1
        /// <summary>
        ///Substitution des valeurs issues de FEESCHEDULE par celle issues de FEESCHEDBRACKET1
        ///Note: S'il n'existe pas de valeurs dans FEESCHEDBRACKET1 on utilise celle de FEESCHEDULE précedemment sauvegardées via SaveFeeMatrixOriginalValues()
        /// </summary>
        /// <param name="pRowNumber"></param>
        private void LoadValuesFromBracket1()
        {
            int pRowNumber = _currentBraket;
            //
            #region FEE1
            if ((_dtBracket1.Rows[pRowNumber]["FEE1EXPRESSIONTYPE"] is DBNull))
            {
                _feeMatrix.FEESCHED_FEE1NUMSpecified = _feeMatrixOriginalValues.FEESCHED_FEE1NUMSpecified;
                if (_feeMatrix.FEESCHED_FEE1NUMSpecified)
                    _feeMatrix.FEESCHED_FEE1NUM = _feeMatrixOriginalValues.FEESCHED_FEE1NUM;
                _feeMatrix.FEESCHED_FEE1DENSpecified = _feeMatrixOriginalValues.FEESCHED_FEE1DENSpecified;
                if (_feeMatrix.FEESCHED_FEE1DENSpecified)
                    _feeMatrix.FEESCHED_FEE1DEN = _feeMatrixOriginalValues.FEESCHED_FEE1DEN;
                _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified = _feeMatrixOriginalValues.FEESCHED_FEE1EXPRESSIONTYPESpecified;
                if (_feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified)
                    _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE = _feeMatrixOriginalValues.FEESCHED_FEE1EXPRESSIONTYPE;
                _feeMatrix.FEESCHED_IDC_FEE1Specified = _feeMatrixOriginalValues.FEESCHED_IDC_FEE1Specified;
                if (_feeMatrix.FEESCHED_IDC_FEE1Specified)
                    _feeMatrix.FEESCHED_IDC_FEE1 = _feeMatrixOriginalValues.FEESCHED_IDC_FEE1;
                _feeMatrix.FEESCHED_FEE1_EXCHTSpecified = _feeMatrixOriginalValues.FEESCHED_FEE1_EXCHTSpecified;
                if (_feeMatrix.FEESCHED_FEE1_EXCHTSpecified)
                    _feeMatrix.FEESCHED_FEE1_EXCHT = _feeMatrixOriginalValues.FEESCHED_FEE1_EXCHT;
            }
            else
            {
                _feeMatrix.FEESCHED_FEE1NUMSpecified = !(_dtBracket1.Rows[pRowNumber]["FEE1NUM"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE1NUMSpecified)
                    _feeMatrix.FEESCHED_FEE1NUM = (float)Convert.ToDouble(_dtBracket1.Rows[pRowNumber]["FEE1NUM"]);
                _feeMatrix.FEESCHED_FEE1DENSpecified = !(_dtBracket1.Rows[pRowNumber]["FEE1DEN"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE1DENSpecified)
                    _feeMatrix.FEESCHED_FEE1DEN = Convert.ToInt32(_dtBracket1.Rows[pRowNumber]["FEE1DEN"]);
                _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified = !(_dtBracket1.Rows[pRowNumber]["FEE1EXPRESSIONTYPE"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE1EXPRESSIONTYPESpecified)
                    _feeMatrix.FEESCHED_FEE1EXPRESSIONTYPE = Convert.ToString(_dtBracket1.Rows[pRowNumber]["FEE1EXPRESSIONTYPE"]);
                _feeMatrix.FEESCHED_IDC_FEE1Specified = !(_dtBracket1.Rows[pRowNumber]["IDC_FEE1"] is DBNull);
                if (_feeMatrix.FEESCHED_IDC_FEE1Specified)
                    _feeMatrix.FEESCHED_IDC_FEE1 = Convert.ToString(_dtBracket1.Rows[pRowNumber]["IDC_FEE1"]);
                _feeMatrix.FEESCHED_FEE1_EXCHTSpecified = !(_dtBracket1.Rows[pRowNumber]["FEE1_EXCHT"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE1_EXCHTSpecified)
                    _feeMatrix.FEESCHED_FEE1_EXCHT = Convert.ToString(_dtBracket1.Rows[pRowNumber]["FEE1_EXCHT"]);
            }
            #endregion FEE1
            #region FEE2
            if ((_dtBracket1.Rows[pRowNumber]["FEE2EXPRESSIONTYPE"] is DBNull))
            {
                _feeMatrix.FEESCHED_FEE2NUMSpecified = _feeMatrixOriginalValues.FEESCHED_FEE2NUMSpecified;
                if (_feeMatrix.FEESCHED_FEE2NUMSpecified)
                    _feeMatrix.FEESCHED_FEE2NUM = _feeMatrixOriginalValues.FEESCHED_FEE2NUM;
                _feeMatrix.FEESCHED_FEE2DENSpecified = _feeMatrixOriginalValues.FEESCHED_FEE2DENSpecified;
                if (_feeMatrix.FEESCHED_FEE2DENSpecified)
                    _feeMatrix.FEESCHED_FEE2DEN = _feeMatrixOriginalValues.FEESCHED_FEE2DEN;
                _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified = _feeMatrixOriginalValues.FEESCHED_FEE2EXPRESSIONTYPESpecified;
                if (_feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified)
                    _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE = _feeMatrixOriginalValues.FEESCHED_FEE2EXPRESSIONTYPE;
                _feeMatrix.FEESCHED_IDC_FEE2Specified = _feeMatrixOriginalValues.FEESCHED_IDC_FEE2Specified;
                if (_feeMatrix.FEESCHED_IDC_FEE2Specified)
                    _feeMatrix.FEESCHED_IDC_FEE2 = _feeMatrixOriginalValues.FEESCHED_IDC_FEE2;
                _feeMatrix.FEESCHED_FEE2_EXCHTSpecified = _feeMatrixOriginalValues.FEESCHED_FEE2_EXCHTSpecified;
                if (_feeMatrix.FEESCHED_FEE2_EXCHTSpecified)
                    _feeMatrix.FEESCHED_FEE2_EXCHT = _feeMatrixOriginalValues.FEESCHED_FEE2_EXCHT;
            }
            else
            {
                _feeMatrix.FEESCHED_FEE2NUMSpecified = !(_dtBracket1.Rows[pRowNumber]["FEE2NUM"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE2NUMSpecified)
                    _feeMatrix.FEESCHED_FEE2NUM = (float)Convert.ToDouble(_dtBracket1.Rows[pRowNumber]["FEE2NUM"]);
                _feeMatrix.FEESCHED_FEE2DENSpecified = !(_dtBracket1.Rows[pRowNumber]["FEE2DEN"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE2DENSpecified)
                    _feeMatrix.FEESCHED_FEE2DEN = Convert.ToInt32(_dtBracket1.Rows[pRowNumber]["FEE2DEN"]);
                _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified = !(_dtBracket1.Rows[pRowNumber]["FEE2EXPRESSIONTYPE"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE2EXPRESSIONTYPESpecified)
                    _feeMatrix.FEESCHED_FEE2EXPRESSIONTYPE = Convert.ToString(_dtBracket1.Rows[pRowNumber]["FEE2EXPRESSIONTYPE"]);
                _feeMatrix.FEESCHED_IDC_FEE2Specified = !(_dtBracket1.Rows[pRowNumber]["IDC_FEE2"] is DBNull);
                if (_feeMatrix.FEESCHED_IDC_FEE2Specified)
                    _feeMatrix.FEESCHED_IDC_FEE2 = Convert.ToString(_dtBracket1.Rows[pRowNumber]["IDC_FEE2"]);
                _feeMatrix.FEESCHED_FEE2_EXCHTSpecified = !(_dtBracket1.Rows[pRowNumber]["FEE2_EXCHT"] is DBNull);
                if (_feeMatrix.FEESCHED_FEE2_EXCHTSpecified)
                    _feeMatrix.FEESCHED_FEE2_EXCHT = Convert.ToString(_dtBracket1.Rows[pRowNumber]["FEE2_EXCHT"]);
            }
            #endregion FEE2
            #region MINNUM / MAXNUM
            if ((_dtBracket1.Rows[pRowNumber]["MINNUM"] is DBNull) || (_dtBracket1.Rows[pRowNumber]["MAXNUM"] is DBNull))
            {
                _feeMatrix.FEESCHED_MINNUMSpecified = _feeMatrixOriginalValues.FEESCHED_MINNUMSpecified;
                if (_feeMatrix.FEESCHED_MINNUMSpecified)
                    _feeMatrix.FEESCHED_MINNUM = _feeMatrixOriginalValues.FEESCHED_MINNUM;
                _feeMatrix.FEESCHED_MINDENSpecified = _feeMatrixOriginalValues.FEESCHED_MINDENSpecified;
                if (_feeMatrix.FEESCHED_MINDENSpecified)
                    _feeMatrix.FEESCHED_MINDEN = _feeMatrixOriginalValues.FEESCHED_MINDEN;
                //
                _feeMatrix.FEESCHED_MAXNUMSpecified = _feeMatrixOriginalValues.FEESCHED_MAXNUMSpecified;
                if (_feeMatrix.FEESCHED_MAXNUMSpecified)
                    _feeMatrix.FEESCHED_MAXNUM = _feeMatrixOriginalValues.FEESCHED_MAXNUM;
                _feeMatrix.FEESCHED_MAXDENSpecified = _feeMatrixOriginalValues.FEESCHED_MAXDENSpecified;
                if (_feeMatrix.FEESCHED_MAXDENSpecified)
                    _feeMatrix.FEESCHED_MAXDEN = _feeMatrixOriginalValues.FEESCHED_MAXDEN;
                //
                _feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified = _feeMatrixOriginalValues.FEESCHED_MINMAXEXPRESSIONTSpecified;
                if (_feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified)
                    _feeMatrix.FEESCHED_MINMAXEXPRESSIONT = _feeMatrixOriginalValues.FEESCHED_MINMAXEXPRESSIONT;
                _feeMatrix.FEESCHED_MINMAXBASISSpecified = _feeMatrixOriginalValues.FEESCHED_MINMAXBASISSpecified;
                if (_feeMatrix.FEESCHED_MINMAXBASISSpecified)
                    _feeMatrix.FEESCHED_MINMAXBASIS = _feeMatrixOriginalValues.FEESCHED_MINMAXBASIS;
                _feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified = _feeMatrixOriginalValues.FEESCHED_IDC_MINMAXBASISSpecified;
                if (_feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified)
                    _feeMatrix.FEESCHED_IDC_MINMAXBASIS = _feeMatrixOriginalValues.FEESCHED_IDC_MINMAXBASIS;
                _feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified = _feeMatrixOriginalValues.FEESCHED_MINMAXEXPRESSIONTSpecified;
                if (_feeMatrix.FEESCHED_MINMAXBASIS_EXCHTSpecified)
                    _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT = _feeMatrixOriginalValues.FEESCHED_MINMAXBASIS_EXCHT;
            }
            else
            {
                _feeMatrix.FEESCHED_MINNUMSpecified = !(_dtBracket1.Rows[pRowNumber]["MINNUM"] is DBNull);
                if (_feeMatrix.FEESCHED_MINNUMSpecified)
                    _feeMatrix.FEESCHED_MINNUM = (float)Convert.ToDouble(_dtBracket1.Rows[pRowNumber]["MINNUM"]);
                _feeMatrix.FEESCHED_MINDENSpecified = !(_dtBracket1.Rows[pRowNumber]["MINDEN"] is DBNull);
                if (_feeMatrix.FEESCHED_MINDENSpecified)
                    _feeMatrix.FEESCHED_MINDEN = Convert.ToInt32(_dtBracket1.Rows[pRowNumber]["MINDEN"]);
                //
                _feeMatrix.FEESCHED_MAXNUMSpecified = !(_dtBracket1.Rows[pRowNumber]["MAXNUM"] is DBNull);
                if (_feeMatrix.FEESCHED_MAXNUMSpecified)
                    _feeMatrix.FEESCHED_MAXNUM = (float)Convert.ToDouble(_dtBracket1.Rows[pRowNumber]["MAXNUM"]);
                _feeMatrix.FEESCHED_MAXDENSpecified = !(_dtBracket1.Rows[pRowNumber]["MAXDEN"] is DBNull);
                if (_feeMatrix.FEESCHED_MAXDENSpecified)
                    _feeMatrix.FEESCHED_MAXDEN = Convert.ToInt32(_dtBracket1.Rows[pRowNumber]["MAXDEN"]);
                //
                _feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified = !(_dtBracket1.Rows[pRowNumber]["MINMAXEXPRESSIONT"] is DBNull);
                if (_feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified)
                    _feeMatrix.FEESCHED_MINMAXEXPRESSIONT = Convert.ToString(_dtBracket1.Rows[pRowNumber]["MINMAXEXPRESSIONT"]);
                _feeMatrix.FEESCHED_MINMAXBASISSpecified = !(_dtBracket1.Rows[pRowNumber]["MINMAXBASIS"] is DBNull);
                if (_feeMatrix.FEESCHED_MINMAXBASISSpecified)
                    _feeMatrix.FEESCHED_MINMAXBASIS = Convert.ToString(_dtBracket1.Rows[pRowNumber]["MINMAXBASIS"]);
                _feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified = !(_dtBracket1.Rows[pRowNumber]["IDC_MINMAXBASIS"] is DBNull);
                if (_feeMatrix.FEESCHED_IDC_MINMAXBASISSpecified)
                    _feeMatrix.FEESCHED_IDC_MINMAXBASIS = Convert.ToString(_dtBracket1.Rows[pRowNumber]["IDC_MINMAXBASIS"]);
                _feeMatrix.FEESCHED_MINMAXEXPRESSIONTSpecified = !(_dtBracket1.Rows[pRowNumber]["MINMAXBASIS_EXCHT"] is DBNull);
                if (_feeMatrix.FEESCHED_MINMAXBASIS_EXCHTSpecified)
                    _feeMatrix.FEESCHED_MINMAXBASIS_EXCHT = Convert.ToString(_dtBracket1.Rows[pRowNumber]["MINMAXBASIS_EXCHT"]);
            }
            #endregion MINNUM / MAXNUM
        }
        #endregion
        #endregion Methods
    }

    public class FeeDataForFreeFormula
    {
        public FeeDataForFreeFormulaDet[] dataFee;

        public FeeDataForFreeFormula()
        {
            dataFee = new FeeDataForFreeFormulaDet[2] { new FeeDataForFreeFormulaDet(), new FeeDataForFreeFormulaDet() };
        }
    }
    public class FeeDataForFreeFormulaDet
    {
        private bool specified;
        private decimal rate;

        public int DcfNum;
        public decimal DcfDen;
        public decimal AssessmentBasis;
        public decimal Rate
        {
            get { return rate; }
            set
            {
                rate = value;
                specified = true;
            }
        }
        public bool Specified
        {
            get { return specified; }
        }

        public FeeDataForFreeFormulaDet()
        {
            specified = false;
        }

        public void Reset()
        {
            DcfNum = 0;
            DcfDen = 0;
            AssessmentBasis = 0;
            rate = 0;
            specified = false;
        }
    }
}
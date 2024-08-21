using System;
using System.Xml.Serialization;

namespace EFS.SpheresIO
{
    /// <summary>
    /// RegulatoryReport
    /// </summary>
    public partial class RegulatoryReport
    {
        public RegulatoryReport()
        {
            trade = new RegulatoryReportTrade();
        }
    }
    #region Trade
    /// <summary>
    ///RegulatoryReport.trade
    /// </summary>
    public partial class RegulatoryReportTrade
    {
        public RegulatoryReportTrade()
        {
            header = new RegulatoryReportTradeHeader();
            dataDocument = new RegulatoryReportTradeDataDocument();
        }
    }
    /// <summary>
    /// header
    /// </summary>
    public partial class RegulatoryReportTradeHeader
    {
        public RegulatoryReportTradeHeader()
        {
        }
    }
    /// <summary>
    /// dataDocument
    /// </summary>
    public partial class RegulatoryReportTradeDataDocument
    {
        public RegulatoryReportTradeDataDocument()
        {
            repository = new RegulatoryReportTradeDataDocumentRepository();
            business = new RegulatoryReportTradeDataDocumentBusiness();
        }
    }
    /// <summary>
    /// repository
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentRepository
    {
        public RegulatoryReportTradeDataDocumentRepository()
        {
            mainActor = new RegulatoryReportTradeDataDocumentRepositoryMainActor();
            counterparty = new RegulatoryReportTradeDataDocumentRepositoryCounterparty();
        }
        public void InstantiateMasterAgreement()
        {
            masterAgreement = new RegulatoryReportTradeDataDocumentRepositoryMasterAgreement();
        }
    }
    /// <summary>
    /// mainActor
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentRepositoryMainActor
    {
        public RegulatoryReportTradeDataDocumentRepositoryMainActor()
        {
        }
    }
    /// <summary>
    /// counterparty
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentRepositoryCounterparty
    {
        public RegulatoryReportTradeDataDocumentRepositoryCounterparty()
        {
        }
    }
    /// <summary>
    /// masterAgreement
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentRepositoryMasterAgreement
    {
        public RegulatoryReportTradeDataDocumentRepositoryMasterAgreement()
        {
        }
    }

    /// <summary>
    /// business
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusiness
    {
        public RegulatoryReportTradeDataDocumentBusiness()
        {
        }
        public void InstantiateSwap()
        {
            swap = new RegulatoryReportTradeDataDocumentBusinessSwap();
        }
        public void InstantiateCapFloor()
        {
            capFloor = new RegulatoryReportTradeDataDocumentBusinessCapFloor();
        }
        public void InstantiateBondOption()
        {
            bondOption = new RegulatoryReportTradeDataDocumentBusinessBondOption();
        }
        public void InstantiateFxSimpleOption()
        {
            fxSimpleOption = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOption();
        }
        public void InstantiateFxBarrierOption()
        {
            fxBarrierOption = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOption();
        }
        public void InstantiateFxDigitalOption()
        {
            fxDigitalOption = new RegulatoryReportTradeDataDocumentBusinessFxDigitalOption();
        }
        public void InstantiateExchangedCashFlows()
        {
            exchangedCashFlows = new RegulatoryReportTradeDataDocumentBusinessExchangedCashFlows();
        }
        public void InstantiateForwardCashFlows()
        {
            forwardCashFlows = new RegulatoryReportTradeDataDocumentBusinessForwardCashFlows();
        }
        public void InstantiateClosingEvents()
        {
            closingEvents = new RegulatoryReportTradeDataDocumentBusinessClosingEvents();
        }
    }

    /// <summary>
    /// Swap
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessSwap
    {
        public RegulatoryReportTradeDataDocumentBusinessSwap()
        {
            receiverStream = new RegulatoryReportTradeDataDocumentBusinessSwapReceiverStream();
            payerStream = new RegulatoryReportTradeDataDocumentBusinessSwapPayerStream();
            nominal = new RegulatoryReportTradeDataDocumentBusinessSwapNominal();
        }
        public void InstantiateUpFront()
        {
            upfront = new RegulatoryReportTradeDataDocumentBusinessSwapUpfront();
        }
    }

    public partial class RegulatoryReportTradeDataDocumentBusinessSwapReceiverStream
    {

        [XmlIgnore]
        public bool periodRateTypeSpecified;

        [XmlIgnore]
        public bool fixedRateSpecified;

        [XmlIgnore]
        public bool isInterpolateFloatRateSpecified;

        [XmlIgnore]
        public bool floatRateAssetIdentSpecified;

        [XmlIgnore]
        public bool spreadEndDateSpecified;

        [XmlIgnore]
        public bool spreadSpecified;

        [XmlIgnore]
        public bool elapsedDaysSpecified;

        [XmlIgnore]
        public bool idcSpecified;

        [XmlIgnore]
        public bool accruedInterestSpecified;
    }

    public partial class RegulatoryReportTradeDataDocumentBusinessSwapPayerStream
    {
        [XmlIgnore]
        public bool periodRateTypeSpecified;

        [XmlIgnore]
        public bool fixedRateSpecified;

        [XmlIgnore]
        public bool isInterpolateFloatRateSpecified;

        [XmlIgnore]
        public bool floatRateAssetIdentSpecified;

        [XmlIgnore]
        public bool spreadEndDateSpecified;

        [XmlIgnore]
        public bool spreadSpecified;

        [XmlIgnore]
        public bool elapsedDaysSpecified;

        [XmlIgnore]
        public bool idcSpecified;

        [XmlIgnore]
        public bool accruedInterestSpecified;
    }

    /// <summary>
    /// CapFloor
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessCapFloor
    {
        public RegulatoryReportTradeDataDocumentBusinessCapFloor()
        {
            capFloorStream = new RegulatoryReportTradeDataDocumentBusinessCapFloorCapFloorStream();
            nominal = new RegulatoryReportTradeDataDocumentBusinessCapFloorNominal();
        }
    }

    /// <summary>
    /// BondOption
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessBondOption
    {
        public RegulatoryReportTradeDataDocumentBusinessBondOption()
        {
            notionalAmount = new RegulatoryReportTradeDataDocumentBusinessBondOptionNotionalAmount();
            premium = new RegulatoryReportTradeDataDocumentBusinessBondOptionPremium();
            exercise = new RegulatoryReportTradeDataDocumentBusinessBondOptionExercise();
            bond = new RegulatoryReportTradeDataDocumentBusinessBondOptionBond();
            //securityAsset = new RegulatoryReportTradeDataDocumentBusinessBondOptionSecurityAsset();
        }
    }


    /// <summary>
    /// Notional amount : instatiate countervalue class
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessBondOptionNotionalAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessBondOptionNotionalAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessBondOptionNotionalAmountCounterValueAmount();
        }
    }

    /// <summary>
    /// Notional amount : instatiate countervalue class
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessBondOptionPremium
    {
        public RegulatoryReportTradeDataDocumentBusinessBondOptionPremium()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessBondOptionPremiumCounterValueAmount();
        }
    }

    /// <summary>
    /// fxSimpleOption
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxSimpleOption
    {
        public RegulatoryReportTradeDataDocumentBusinessFxSimpleOption()
        {
            fxStrikePrice = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionFxStrikePrice();
            putCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPutCurrencyAmount();
            callCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionCallCurrencyAmount();
            payCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPayCurrencyAmount();
            recCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionRecCurrencyAmount();
        }
    }
    /// <summary>
    /// PutCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPutCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPutCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPutCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// CallCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionCallCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionCallCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionCallCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// PayCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPayCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPayCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionPayCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// RecCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionRecCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionRecCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxSimpleOptionRecCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// fxBarrierOption
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxBarrierOption
    {
        public RegulatoryReportTradeDataDocumentBusinessFxBarrierOption()
        {
            fxStrikePrice = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionFxStrikePrice();
            putCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPutCurrencyAmount();
            callCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionCallCurrencyAmount();
            payCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPayCurrencyAmount();
            recCurrencyAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionRecCurrencyAmount();
        }
    }
    /// <summary>
    /// PutCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPutCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPutCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPutCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// CallCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionCallCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionCallCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionCallCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// PayCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPayCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPayCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionPayCurrencyAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// RecCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionRecCurrencyAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionRecCurrencyAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionRecCurrencyAmountCounterValueAmount();
        }
    }

    /// <summary>
    /// RecCurrencyAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionFxBarrier
    {
        public RegulatoryReportTradeDataDocumentBusinessFxBarrierOptionFxBarrier()
        {
        }
    }

    /// <summary>
    /// fxDigitalOption
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxDigitalOption
    {
        public RegulatoryReportTradeDataDocumentBusinessFxDigitalOption()
        {
            fxStrikePrice = new RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionFxStrikePrice();
            payoutAmount = new RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionPayoutAmount();
            incomeAmount = new RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionIncomeAmount();
        }
    }
    /// <summary>
    /// PayoutAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionPayoutAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionPayoutAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionPayoutAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// IncomeAmount
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionIncomeAmount
    {
        public RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionIncomeAmount()
        {
        }
        public void InstantiateCounterValueAmount()
        {
            counterValueAmount = new RegulatoryReportTradeDataDocumentBusinessFxDigitalOptionIncomeAmountCounterValueAmount();
        }
    }
    /// <summary>
    /// ClosingEvents
    /// </summary>
    public partial class RegulatoryReportTradeDataDocumentBusinessClosingEvents
    {
        public RegulatoryReportTradeDataDocumentBusinessClosingEvents()
        {
        }
        public void InstantiateMarkToMarket()
        {
            markToMarket = new RegulatoryReportTradeDataDocumentBusinessClosingEventsMarkToMarket();
        }
        public void InstantiateLinearDepreciation()
        {
            linearDepreciation = new RegulatoryReportTradeDataDocumentBusinessClosingEventsLinearDepreciation();
        }
        public void InstantiateAccruedInterest()
        {
            accruedInterest = new RegulatoryReportTradeDataDocumentBusinessClosingEventsAccruedInterest();
        }
        public void InstantiateIntrinsicValues()
        {
            intrinsicValues = new RegulatoryReportTradeDataDocumentBusinessClosingEventsIntrinsicValues();
        }
        public void InstantiateGreeks()
        {
            greeks = new RegulatoryReportTradeDataDocumentBusinessClosingEventsGreeks();
        }

        public void InstantiateBondUnderlyingQuotation()
        {
            bondUnderlyingQuotation = new RegulatoryReportTradeDataDocumentBusinessClosingEventsBondUnderlyingQuotation();
        }

        public void InstantiateCapLetsFloorLets()
        {
            capLetsFloorLets = new RegulatoryReportTradeDataDocumentBusinessClosingEventsCapLetsFloorLets();
        }
    }
    public partial class RegulatoryReportTradeDataDocumentBusinessClosingEventsMarkToMarket
    {
        [XmlIgnore]
        public bool toReceiveDateSpecified;
        [XmlIgnore]
        public bool toReceiveIdcSpecified;
        [XmlIgnore]
        public bool toReceiveAmountSpecified;
        [XmlIgnore]
        public bool toPayDateSpecified;
        [XmlIgnore]
        public bool toPayIdcSpecified;
        [XmlIgnore]
        public bool toPayAmountSpecified;
    }
    #endregion
    #region Report
    /// <summary>
    /// report
    /// </summary>
    public partial class RegulatoryReportReport
    {
        public RegulatoryReportReport()
        {
        }
    }
    /// <summary>
    /// formaTecnica
    /// </summary>
    public partial class RegulatoryReportReportFormaTecnica
    {
        public RegulatoryReportReportFormaTecnica()
        {
        }
    }
    /// <summary>
    /// Data
    /// </summary>
    public partial class RegulatoryReportReportFormaTecnicaData
    {
        /// <summary>
        /// empty method 
        /// no parameters
        /// </summary>
        public RegulatoryReportReportFormaTecnicaData()
        {
        }
        /// <summary>
        /// Only for static value
        /// </summary>
        /// <param name="pDataIndex"></param>
        /// <param name="pId"></param>
        /// <param name="pSource"></param>
        /// <param name="pValue"></param>
        public RegulatoryReportReportFormaTecnicaData(int pDataIndex, string pId, string pSource, string pValue)
        {
            sqc = "d0" + pDataIndex;
            field = pId;
            src = pSource;
            value = pValue;
        }
        /// <summary>
        /// Simple method
        /// feed BKI data
        /// Create no formatted data(datalayout and parsing parameters not used)
        /// </summary>
        /// <param name="pFormaTecnica"></param>
        /// <param name="pDataIndex"></param>
        /// <param name="pDataId"></param>
        /// <param name="pDataDisplayName"></param>
        /// <param name="pDataSourceProcess"></param>
        /// <param name="pValue"></param>
        /// <param name="errMessage"></param>
        public RegulatoryReportReportFormaTecnicaData(string pFormaTecnica, int pDataIndex, string pDataId, string pDataDisplayName, string pDataSourceProcess, string pValue, string errMessage)
        {
            sqc = "d0" + pDataIndex;
            field = pDataId;
            displayName = pDataDisplayName;
            src = pDataSourceProcess;
            value = pValue;
            errMsg = errMessage;
        }
        /// <summary>
        /// Complex method 
        /// Feed subSystem data 
        /// Create formatted data(datalayout and parsing parameters used)
        /// </summary>
        /// <param name="pFormaTecnica"></param>
        /// <param name="pDataIndex"></param>
        /// <param name="pDataId"></param>
        /// <param name="pDataDisplayName">Exploited on BankItalia and Elsag subSystem and is null for Oasi subSystem</param>
        /// <param name="pOriginalDataId">Exploited on Oasi subSystem</param>
        /// <param name="pDataSourceProcess"></param>
        /// <param name="pValue"></param>
        /// <param name="errMessage"></param>
        public RegulatoryReportReportFormaTecnicaData(string pFormaTecnica, int pDataIndex, string pDataId, string pDataDisplayName, string pOriginalDataId, string pDataSourceProcess, string pValue, string errMessage)
        {
            sqc = "d0" + pDataIndex;
            field = pDataId;
            // add displayName attribute if pDataDisplayName param is not null (BankItalia, Elsag )
            if (null != pDataDisplayName)
                displayName = pDataDisplayName;
            // add originalFieldName attribute if pOriginalDataId param is not null (Oasi SubSystem)
            if (null != pOriginalDataId)
                originalName = pOriginalDataId;
            //
            src = pDataSourceProcess;
            // GS 20110124: control value for each type 
            // String = empty
            // datetime = MinValue (01-01-0001)
            // decimal = MinValue? Zero? now the control is handled for '0.00'
            //if (String.Empty != pValue & pValue != "0.00" & pValue != "!" & pValue != Convert.ToString(DateTime.MinValue))
            if (String.Empty != pValue & pValue != "0.00" & pValue != Convert.ToString(Decimal.MinusOne) & pValue != Convert.ToString(DateTime.MinValue))
            {
                // out of the scope value (in use for intrinsic values)
                if (pValue == "*")
                    value = RRDataLayout.GetFormattedOutOfTheScopeData(pFormaTecnica, pDataId, pValue);
                else
                    value = RRDataLayout.GetFormattedProcessData(pFormaTecnica, pDataId, pValue);
            }
            else
            {
                RRDataLayout.DataLayoutInformation info = new RRDataLayout.DataLayoutInformation();
                RRDataLayout.GetDataLayoutInformation(pFormaTecnica, pDataId, ref info);
                alertLevel = info.missingAlertLevel;
                value = RRDataLayout.GetFormattedMissingData(pFormaTecnica, pDataId, info.staticValue);
                errMsg = errMessage;
            }
        }
    }
    #endregion
    #region Logs
    /// <summary>
    /// logs
    /// </summary>
    public partial class RegulatoryReportLog
    {
        public RegulatoryReportLog()
        {

        }
    }
    /// <summary>
    /// log
    /// </summary>
    public partial class RegulatoryReportLogData
    {
        public RegulatoryReportLogData()
        {

        }
    }

    #endregion
}

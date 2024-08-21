#region Using Directives
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Enum;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Process
{
    #region TradeActionGenAbandonExercise
    public class TradeActionGenProcessAbandonExercise : TradeActionGenProcessBase
	{
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessAbandonExercise(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
		{
			//CodeReturn = Valorize();
		}
		#endregion Constructors
		#region Methods
		#region AddEventClassCurrencyAmount
		/// <revision>
		///     <version>1.1.8</version><date>20070823</date><author>EG</author>
		///     <comment>
		///     Apply Rules to determine the date of Pre-Settlement events
		///     </comment>
		/// </revision>
        protected DataRow AddEventClassAmount(int pIdE, DataRow pRowSource, ActionMsgBase pEventLine)
		{
            return AddEventClassAmount(pIdE, pRowSource, pEventLine, false, null);
		}
        protected DataRow AddEventClassAmount(int pIdE, DataRow pRowSource, ActionMsgBase pEventLine, bool pIsSettlement)
        {
            return AddEventClassAmount(pIdE, pRowSource, pEventLine, pIsSettlement, null);
        }
        protected DataRow AddEventClassAmount(int pIdE, DataRow pRowSource, ActionMsgBase pEventLine, bool pIsSettlement, EFS_PreSettlement pPreSettlement)
		{
            AbandonExerciseMsgBase abandonExerciseMsg = (AbandonExerciseMsgBase)pEventLine;
            object[] clone = (object[])pRowSource.ItemArray.Clone();
			DataRow rowCurrencyAmount   = DtEventClass.NewRow();
            rowCurrencyAmount.ItemArray = clone;
			rowCurrencyAmount.BeginEdit();
			rowCurrencyAmount["IDE"] = pIdE;
			rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.Recognition;
            rowCurrencyAmount["DTEVENT"] = abandonExerciseMsg.actionDate.Date;
            rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, abandonExerciseMsg.actionDate.Date);
			rowCurrencyAmount["ISPAYMENT"]= false; 
			rowCurrencyAmount.EndEdit();
			DtEventClass.Rows.Add(rowCurrencyAmount);
			if (pIsSettlement)
			{
                if (null != pPreSettlement)
                {
                    rowCurrencyAmount = DtEventClass.NewRow();
                    rowCurrencyAmount.ItemArray = clone;
                    rowCurrencyAmount.BeginEdit();
                    rowCurrencyAmount["IDE"] = pIdE;
                    rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.PreSettlement;
                    rowCurrencyAmount["DTEVENT"] = pPreSettlement.AdjustedPreSettlementDate.DateValue;
                    rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, pPreSettlement.AdjustedPreSettlementDate.DateValue);
                    rowCurrencyAmount["ISPAYMENT"] = false;
                    rowCurrencyAmount.EndEdit();
                    DtEventClass.Rows.Add(rowCurrencyAmount);
                }

				rowCurrencyAmount = DtEventClass.NewRow();
                rowCurrencyAmount.ItemArray = clone;
				rowCurrencyAmount.BeginEdit();
				rowCurrencyAmount["IDE"] = pIdE;
				rowCurrencyAmount["EVENTCLASS"] = EventClassFunc.Settlement;
                rowCurrencyAmount["DTEVENT"] = abandonExerciseMsg.valueDate;
                rowCurrencyAmount["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, abandonExerciseMsg.valueDate);
				rowCurrencyAmount["ISPAYMENT"] = true; 
				rowCurrencyAmount.EndEdit();
				DtEventClass.Rows.Add(rowCurrencyAmount);
			}
			return rowCurrencyAmount;
		}		
		#endregion AddEventClassCurrencyAmount
        #region AddEventAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pIdEParent"></param>
        /// <param name="pRowSource"></param>
        /// <param name="pEventLine"></param>
        /// <param name="pAmountType"></param>
        /// <returns></returns>
        /// FI 20180220 [XXXXX] Modify
        protected DataRow AddEventAmount(int pIdE, int pIdEParent, DataRow pRowSource, ActionMsgBase pEventLine, string pAmountType)
        {
            BO_AbandonExerciseMsgBase abandonExerciseMsg = pEventLine as BO_AbandonExerciseMsgBase;
            PayerReceiverInfoDet payer = null;
            PayerReceiverInfoDet receiver = null;
            DataRow rowAmount = DtEvent.NewRow();
            rowAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
            rowAmount.BeginEdit();
            rowAmount["IDE"] = pIdE;
            rowAmount["IDE_EVENT"] = pIdEParent;
            rowAmount["EVENTCODE"] = GetEventCodeExercise(pRowSource, abandonExerciseMsg, pAmountType); 
            rowAmount["EVENTTYPE"] = pAmountType;

            BO_ExerciseMsg exerciseMsg;
            if (EventTypeFunc.IsSettlementCurrency(pAmountType))
            {
                // Settlement
                exerciseMsg = pEventLine as BO_ExerciseMsg;
                payer = exerciseMsg.settlement.payer;
                receiver = exerciseMsg.settlement.receiver;

                rowAmount["VALORISATION"] = exerciseMsg.settlement.Amount;
                rowAmount["UNIT"] = exerciseMsg.settlement.Currency;
                rowAmount["UNITTYPE"] = UnitTypeEnum.Currency;
                rowAmount["VALORISATIONSYS"] = exerciseMsg.settlement.Amount;
                rowAmount["UNITSYS"] = exerciseMsg.settlement.Currency;
                rowAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;
            }
            else if (EventTypeFunc.IsBondPayment(pAmountType))
            {
                // BondPayment
                exerciseMsg = pEventLine as BO_ExerciseMsg;
                payer = exerciseMsg.bondPayment.payer;
                receiver = exerciseMsg.bondPayment.receiver;

                rowAmount["VALORISATION"] = exerciseMsg.bondPayment.Amount;
                rowAmount["UNIT"] = exerciseMsg.bondPayment.Currency;
                rowAmount["UNITTYPE"] = UnitTypeEnum.Currency;
                rowAmount["VALORISATIONSYS"] = exerciseMsg.bondPayment.Amount;
                rowAmount["UNITSYS"] = exerciseMsg.bondPayment.Currency;
                rowAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;
            }
            else if (EventTypeFunc.IsNominal(pAmountType))
            {
                // Notional
                payer = abandonExerciseMsg.notional.payer;
                receiver = abandonExerciseMsg.notional.receiver;

                rowAmount["VALORISATION"] = abandonExerciseMsg.notional.Amount;
                rowAmount["UNIT"] = abandonExerciseMsg.notional.Currency;
                rowAmount["UNITTYPE"] = UnitTypeEnum.Currency;
                rowAmount["VALORISATIONSYS"] = abandonExerciseMsg.notional.Amount;
                rowAmount["UNITSYS"] = abandonExerciseMsg.notional.Currency;
                rowAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;

            }
            else if (EventTypeFunc.IsQuantity(pAmountType))
            {
                // NbOptions
                payer = abandonExerciseMsg.nbOptions.payer;
                receiver = abandonExerciseMsg.nbOptions.receiver;

                rowAmount["VALORISATION"] = abandonExerciseMsg.nbOptions.Quantity;
                rowAmount["UNIT"] = Convert.DBNull;
                rowAmount["UNITTYPE"] = UnitTypeEnum.Qty;
                rowAmount["VALORISATIONSYS"] = abandonExerciseMsg.nbOptions.Quantity;
                rowAmount["UNITSYS"] = Convert.DBNull;
                rowAmount["UNITTYPESYS"] = UnitTypeEnum.Qty;
            }
            else if (EventTypeFunc.IsUnderlyer(pAmountType))
            {
                // Entitlement
                payer = abandonExerciseMsg.entitlement.payer;
                receiver = abandonExerciseMsg.entitlement.receiver;

                rowAmount["VALORISATION"] = abandonExerciseMsg.entitlement.Quantity;
                rowAmount["UNIT"] = Convert.DBNull;
                rowAmount["UNITTYPE"] = UnitTypeEnum.Qty;
                rowAmount["VALORISATIONSYS"] = abandonExerciseMsg.entitlement.Quantity;
                rowAmount["UNITSYS"] = Convert.DBNull;
                rowAmount["UNITTYPESYS"] = UnitTypeEnum.Qty;
            }

            // FI 20180220 [XXXXX] Add différents tests sur First.HasValue
            rowAmount["IDA_PAY"] = payer.actor.First ?? Convert.DBNull;
            rowAmount["IDB_PAY"] = payer.book.First ?? Convert.DBNull;
            rowAmount["IDA_REC"] = receiver.actor.First ?? Convert.DBNull;
            rowAmount["IDB_REC"] = receiver.book.First ?? Convert.DBNull;


            rowAmount.EndEdit();
            DtEvent.Rows.Add(rowAmount);
            SetRowStatus(rowAmount, TuningOutputTypeEnum.OES);
            return rowAmount;
        }
        #endregion AddEventAmount
        #region AddEventCurrencyAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pIdEParent"></param>
        /// <param name="pRowSource"></param>
        /// <param name="pEventLine"></param>
        /// <param name="pCurrencyType"></param>
        /// <returns></returns>
        /// FI 20180220 [XXXXX] Modify
        protected DataRow AddEventCurrencyAmount(int pIdE, int pIdEParent, DataRow pRowSource, ActionMsgBase pEventLine, string pCurrencyType)
		{
            FX_AbandonExerciseMsgBase abandonExerciseMsg = (FX_AbandonExerciseMsgBase)pEventLine;
            string eventCode = GetEventCodeExercise(pRowSource, abandonExerciseMsg, pCurrencyType);

            DataRow rowCurrencyAmount = DtEvent.NewRow();
			rowCurrencyAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
			rowCurrencyAmount.BeginEdit();
			rowCurrencyAmount["IDE"] = pIdE;
			rowCurrencyAmount["IDE_EVENT"] = pIdEParent;
			rowCurrencyAmount["EVENTCODE"] = eventCode;
			rowCurrencyAmount["EVENTTYPE"] = pCurrencyType;

            AmountPayerReceiverInfo amount;
            if (pCurrencyType == EventTypeFunc.CallCurrency)
                amount = (AmountPayerReceiverInfo)abandonExerciseMsg.callAmount;
            else
                amount = (AmountPayerReceiverInfo)abandonExerciseMsg.putAmount;

            // FI 20180220 [XXXXX] add différents Tests sur First.HasValue
            rowCurrencyAmount["VALORISATION"] = amount.Amount;
            rowCurrencyAmount["UNIT"] = amount.Currency;
			rowCurrencyAmount["UNITTYPE"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["VALORISATIONSYS"] = amount.Amount;
            rowCurrencyAmount["UNITSYS"] = amount.Currency;
			rowCurrencyAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["IDA_PAY"] = amount.payer.actor.First ?? Convert.DBNull;
            rowCurrencyAmount["IDB_PAY"] = amount.payer.book.First ?? Convert.DBNull;
            rowCurrencyAmount["IDA_REC"] = amount.receiver.actor.First ?? Convert.DBNull;
            rowCurrencyAmount["IDB_REC"] = amount.receiver.book.First ?? Convert.DBNull;
            rowCurrencyAmount.EndEdit();
			DtEvent.Rows.Add(rowCurrencyAmount);
			SetRowStatus(rowCurrencyAmount, TuningOutputTypeEnum.OES); 
			return rowCurrencyAmount;
		}
		#endregion AddEventCurrencyAmount
		#region AddEventDetailExercise
        protected void AddEventDetailExercise(int pIdE, AbandonExerciseMsgBase pExerciseMsg, string pAmountType)
		{
            DataRow rowExercise = DtEventDet.NewRow();
			rowExercise.BeginEdit();
			rowExercise["IDE"] = pIdE;

            if (pExerciseMsg is FX_ExerciseMsg)
            {
                // SettlementAmount

                FX_ExerciseMsg exerciseMsg = pExerciseMsg as FX_ExerciseMsg;
                rowExercise["FXTYPE"] = exerciseMsg.fxOptionType;
                rowExercise["NOTIONALAMOUNT"] = (exerciseMsg.settlementCurrency == exerciseMsg.callAmount.Currency) ? exerciseMsg.callAmount.Amount : exerciseMsg.putAmount.Amount;
                rowExercise["SETTLEMENTRATE"] = exerciseMsg.settlementRate;
                rowExercise["STRIKEPRICE"] = exerciseMsg.strikePrice;

                if (exerciseMsg.fixingDateSpecified)
                {
                    rowExercise["DTFIXING"] = exerciseMsg.fixingDate;

                    DataRow rowAssetFixing = DtEventAsset.NewRow();
                    rowAssetFixing.BeginEdit();
                    rowAssetFixing["IDE"] = pIdE;
                    rowAssetFixing["ASSETTYPE"] = QuoteEnum.FXRATE;
                    rowAssetFixing["ASSETCATEGORY"] = Cst.UnderlyingAsset.FxRateAsset;
                    rowAssetFixing["IDASSET"] = exerciseMsg.primaryIdAsset;
                    rowAssetFixing["QUOTESIDE"] = StrFunc.IsFilled(exerciseMsg.quoteSide)?exerciseMsg.quoteSide:Convert.DBNull;
                    rowAssetFixing["QUOTETIMING"] = StrFunc.IsFilled(exerciseMsg.quoteTiming) ? exerciseMsg.quoteTiming : Convert.DBNull;
                    rowAssetFixing["TIME"] = exerciseMsg.fixingDate;

                    if (null != exerciseMsg.primaryKeyAsset)
                    {
                        KeyAssetFxRate asset = exerciseMsg.primaryKeyAsset;
                        rowExercise["IDC1"] = asset.IdC1;
                        rowExercise["IDC2"] = asset.IdC2;
                        rowExercise["IDBC"] = asset.IdBCRateSrc;
                        rowExercise["BASIS"] = asset.QuoteBasis.ToString();
                        rowAssetFixing["IDBC"] = asset.IdBCRateSrc;
                        rowAssetFixing["PRIMARYRATESRC"] = StrFunc.IsFilled(asset.PrimaryRateSrc) ? asset.PrimaryRateSrc : Convert.DBNull;
                        rowAssetFixing["PRIMARYRATESRCPAGE"] = StrFunc.IsFilled(asset.PrimaryRateSrcPage) ? asset.PrimaryRateSrcPage : Convert.DBNull;
                        rowAssetFixing["PRIMARYRATESRCHEAD"] = StrFunc.IsFilled(asset.PrimaryRateSrcHead) ? asset.PrimaryRateSrcHead : Convert.DBNull;
                    }
                    rowAssetFixing.EndEdit();
                    DtEventAsset.Rows.Add(rowAssetFixing);
                }

            }
            else if (pExerciseMsg is BO_ExerciseMsg)
            {
                DebtSecurityOptionContainer debtSecurityOptionContainer = new DebtSecurityOptionContainer((IDebtSecurityOption) Product, DataDocument);
                ISecurityAsset securityAsset = debtSecurityOptionContainer.GetSecurityAssetInDataDocument();
                ISecurity security = securityAsset.DebtSecurity.Security;
                BO_ExerciseMsg exerciseMsg = pExerciseMsg as BO_ExerciseMsg;
                rowExercise["NOTIONALAMOUNT"] = exerciseMsg.notional.Amount;
                rowExercise["DAILYQUANTITY"] = exerciseMsg.nbOptions.Quantity;
                rowExercise["STRIKEPRICE"] = exerciseMsg.strikePrice;
                rowExercise["SETTLEMENTRATE"] = exerciseMsg.settlementRate;
                if (EventTypeFunc.IsBondPayment(pAmountType))
                    rowExercise["INTEREST"] = exerciseMsg.accruedInterest.Amount;

                DataRow rowAsset = DtEventAsset.NewRow();
                rowAsset.BeginEdit();
                rowAsset["IDE"] = pIdE;
                rowAsset["ASSETTYPE"] = QuoteEnum.DEBTSECURITY;
                rowAsset["ASSETCATEGORY"] = Cst.UnderlyingAsset.Bond;
                rowAsset["IDASSET"] = exerciseMsg.idAsset;
                rowAsset["IDM"] = security.ExchangeIdSpecified?security.ExchangeId.OTCmlId:Convert.DBNull;
                rowAsset["IDC"] = security.CurrencySpecified ? security.Currency.Value : Convert.DBNull;
                rowAsset["QUOTESIDE"] = StrFunc.IsFilled(exerciseMsg.quoteSide) ? exerciseMsg.quoteSide : Convert.DBNull;
                rowAsset["QUOTETIMING"] = StrFunc.IsFilled(exerciseMsg.quoteTiming) ? exerciseMsg.quoteTiming : Convert.DBNull;
                rowAsset["TIME"] = exerciseMsg.valueDate;
                rowAsset.EndEdit();
                DtEventAsset.Rows.Add(rowAsset);
            }

            rowExercise["NOTE"] = pExerciseMsg.note;
            rowExercise["DTACTION"] = pExerciseMsg.actionDate;
            rowExercise.EndEdit();
            DtEventDet.Rows.Add(rowExercise);
		}
		#endregion AddEventDetailExercise		
		#region AddEventObservationDates
        protected DataRow AddEventObservationDates(int pIdE, int pIdEParent, DataRow pRowSource, FX_ExerciseMsg pExerciseMsg, RateObservationDateMsg pRateObservationDateMsg)
		{
            #region Event EventObservationDates
            DataRow rowFixing   = DtEvent.NewRow();
			rowFixing.ItemArray = (object[])pRowSource.ItemArray.Clone();
			rowFixing.BeginEdit();
			rowFixing["IDE"] = pIdE;
			rowFixing["IDE_EVENT"]  = pIdEParent;
			rowFixing["EVENTCODE"] = EventCodeFunc.Reset;
			rowFixing["EVENTTYPE"] = EventTypeFunc.FxRate;
            rowFixing["VALORISATION"] = pRateObservationDateMsg.observedRate;
            rowFixing["VALORISATIONSYS"] = pRateObservationDateMsg.observedRate;
			rowFixing["UNITTYPE"] = UnitTypeEnum.Rate;
			rowFixing["UNITTYPESYS"] = UnitTypeEnum.Rate;
			rowFixing.EndEdit();
			DtEvent.Rows.Add(rowFixing);
			SetRowStatus(rowFixing, TuningOutputTypeEnum.OES); 

			#region Event ObservationDates
			DataRow rowEventClassFixing = DtEventClass.NewRow();
			rowEventClassFixing.BeginEdit();
			rowEventClassFixing["IDE"] = pIdE;
			rowEventClassFixing["EVENTCLASS"] = EventClassFunc.Fixing;
			rowEventClassFixing["DTEVENT"] = pRateObservationDateMsg.observedDate;
			rowEventClassFixing["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS,pRateObservationDateMsg.observedDate);
            rowEventClassFixing["ISPAYMENT"] = false; 
            rowEventClassFixing.EndEdit();
			DtEventClass.Rows.Add(rowEventClassFixing);

            #endregion Event ObservationDates

            KeyAssetFxRate keyAsset;
            int idAsset;
            if (pRateObservationDateMsg.isSecondaryRateSource)
            {
                keyAsset = pExerciseMsg.secondaryKeyAsset;
                idAsset = pExerciseMsg.secondaryIdAsset;
            }
            else
            {
                keyAsset = pExerciseMsg.primaryKeyAsset;
                idAsset = pExerciseMsg.primaryIdAsset;
            }

            #region Event ObservationDates
            DataRow rowDetFixing = DtEventDet.NewRow();
			rowDetFixing.BeginEdit();
			rowDetFixing["IDE"] = pIdE;
			rowDetFixing["RATE"] = pRateObservationDateMsg.observedRate;
			rowDetFixing["DTFIXING"] = pRateObservationDateMsg.observedDate;
			rowDetFixing["IDC1"] = keyAsset.IdC1;
			rowDetFixing["IDC2"] = keyAsset.IdC2;
			rowDetFixing["IDBC"] = keyAsset.IdBCRateSrc;
			rowDetFixing["BASIS"] = keyAsset.QuoteBasis.ToString();
			rowDetFixing.EndEdit();
            DtEventDet.Rows.Add(rowDetFixing);
			#endregion EventDet ObservationDates

			#region EventAsset ObservationDates
			DataRow rowAssetFixing = DtEventAsset.NewRow();
			rowAssetFixing.BeginEdit();
			rowAssetFixing["IDE"] = pIdE;
			rowAssetFixing["IDASSET"] = idAsset;
			rowAssetFixing["ASSETTYPE"] = QuoteEnum.FXRATE;
            rowAssetFixing["ASSETCATEGORY"] = Cst.UnderlyingAsset.FxRateAsset;
			rowAssetFixing["TIME"] = pRateObservationDateMsg.observedDate;
			rowAssetFixing["IDBC"] = keyAsset.IdBCRateSrc;
			rowAssetFixing["QUOTESIDE"] = QuotationSideEnum.Mid.ToString();
			rowAssetFixing["QUOTETIMING"] = QuoteTimingEnum.Close.ToString();
			rowAssetFixing["PRIMARYRATESRC"] = keyAsset.PrimaryRateSrc;
			rowAssetFixing["PRIMARYRATESRCPAGE"] = keyAsset.PrimaryRateSrcPage;
			rowAssetFixing["PRIMARYRATESRCHEAD"] = keyAsset.PrimaryRateSrcHead;
			rowAssetFixing.EndEdit();
			DtEventAsset.Rows.Add(rowAssetFixing);
			#endregion EventDet ObservationDates

			return rowFixing;
			#endregion Event EventObservationDates
		}
		#endregion AddEventObservationDates		
		#region AddEventSettlementCurrencyAmount
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdE"></param>
        /// <param name="pIdEParent"></param>
        /// <param name="pRowSource"></param>
        /// <param name="pExerciseMsg"></param>
        /// <returns></returns>
        /// FI 20180220 [XXXXX] Modify
        protected DataRow AddEventSettlementCurrencyAmount(int pIdE, int pIdEParent, DataRow pRowSource, FX_ExerciseMsg pExerciseMsg)
		{
            PayerReceiverInfoDet payer = pExerciseMsg.callAmount.payer;
            PayerReceiverInfoDet receiver = pExerciseMsg.callAmount.receiver;

            string eventCode = GetEventCodeExercise(pRowSource, (FX_AbandonExerciseMsgBase)pExerciseMsg, EventTypeFunc.CallCurrency);
            DataRow rowCurrencyAmount = DtEvent.NewRow();
			rowCurrencyAmount.ItemArray = (object[])pRowSource.ItemArray.Clone();
			rowCurrencyAmount.BeginEdit();
			rowCurrencyAmount["IDE"] = pIdE;
            rowCurrencyAmount["IDE_EVENT"] = pIdEParent;
			rowCurrencyAmount["EVENTCODE"] = eventCode;
			rowCurrencyAmount["EVENTTYPE"] = EventTypeFunc.SettlementCurrency;
            rowCurrencyAmount["VALORISATION"] = pExerciseMsg.settlementAmount;
            rowCurrencyAmount["UNIT"] = pExerciseMsg.settlementCurrency;
            rowCurrencyAmount["UNITTYPE"] = UnitTypeEnum.Currency;
            rowCurrencyAmount["VALORISATIONSYS"] = pExerciseMsg.settlementAmount;
            rowCurrencyAmount["UNITSYS"] = pExerciseMsg.settlementCurrency;
            rowCurrencyAmount["UNITTYPESYS"] = UnitTypeEnum.Currency;

            // FI 20180220 [XXXXX] Add différents tests sur First.HasValue
            rowCurrencyAmount["IDA_PAY"] = payer.actor.First.HasValue ? payer.actor.First : Convert.DBNull;
            rowCurrencyAmount["IDB_PAY"] = payer.book.First.HasValue ? payer.book.First : Convert.DBNull;
            rowCurrencyAmount["IDA_REC"] = receiver.actor.First.HasValue ? receiver.actor.First : Convert.DBNull;
            rowCurrencyAmount["IDB_REC"] = receiver.book.First.HasValue ? receiver.book.First : Convert.DBNull;

			rowCurrencyAmount.EndEdit();
			DtEvent.Rows.Add(rowCurrencyAmount);
			SetRowStatus(rowCurrencyAmount, TuningOutputTypeEnum.OES); 
			return rowCurrencyAmount;
		}
		#endregion AddEventSettlementCurrencyAmount		
        #region GetAmountDispo
        /// <summary>
        /// Retourne le montant|Quantité dispo
        /// </summary>
        /// <param name="pRowSource"></param>
        /// <param name="pAmountType"></param>
        /// <returns></returns>
        private decimal GetAmountDispo(DataRow pRowSource, string pAmountType)
		{
			decimal initialAmount = 0;
			decimal exerciseAmount = 0;
			int idEStream = Convert.ToInt32(pRowSource["IDE_EVENT"]);
			DataRow rowParent = RowEvent(idEStream);
			if (null != rowParent)
			{
				DataRow[] rowChilds = rowParent.GetChildRows(DsEvents.ChildEvent);
				if (null != rowChilds)
				{
					foreach (DataRow rowChild in rowChilds)
					{
                        if ((EventCodeFunc.Start == rowChild["EVENTCODE"].ToString()) && (pAmountType == rowChild["EVENTTYPE"].ToString()))
						{
							initialAmount = Convert.ToDecimal(rowChild["VALORISATION"]);
						}
                        else if ((EventCodeFunc.InitialValuation == rowChild["EVENTCODE"].ToString()) && 
                            EventTypeFunc.IsBond(rowChild["EVENTTYPE"].ToString()))
                        {
                            DataRow[] rowExerciseChilds = rowChild.GetChildRows(DsEvents.ChildEvent);
                            if (null != rowExerciseChilds)
                            {
                                foreach (DataRow rowExerciseChild in rowExerciseChilds)
                                {
                                    if ((EventCodeFunc.Start == rowExerciseChild["EVENTCODE"].ToString()) && (pAmountType == rowExerciseChild["EVENTTYPE"].ToString()))
                                    {
                                        initialAmount = Convert.ToDecimal(rowExerciseChild["VALORISATION"]);
                                    }
                                }
                            }
                        }
						else if (EventCodeFunc.Exercise == rowChild["EVENTCODE"].ToString())
						{
							DataRow[] rowExerciseChilds = rowChild.GetChildRows(DsEvents.ChildEvent);
							if (null != rowExerciseChilds)
							{
								foreach (DataRow rowExerciseChild in rowExerciseChilds)
								{
                                    if ((pAmountType == rowExerciseChild["EVENTTYPE"].ToString()) &&
										(false == EventCodeFunc.IsNominalStep(rowExerciseChild["EVENTCODE"].ToString())) &&
										(DataRowState.Unchanged == rowExerciseChild.RowState))
										exerciseAmount += Convert.ToDecimal(rowExerciseChild["VALORISATION"]);
								}
							}
						}
					}
				}
			}
			return initialAmount - exerciseAmount;
		}
		#endregion GetAmountDispo
		#region GetEventCodeExercise
        /// <summary>
        /// Détermine l'EVENTCODE de l'exercise (INT|TER)
        /// </summary>
        /// <param name="pRowSource"></param>
        /// <param name="pAbandonExerciseMsgBase"></param>
        /// <param name="pAmountType"></param>
        /// <returns></returns>
        private string GetEventCodeExercise(DataRow pRowSource, AbandonExerciseMsgBase pAbandonExerciseMsgBase, string pAmountType)
		{
			string eventCode = EventCodeFunc.Termination;
            decimal exerciseAmount = 0;
            decimal amountDispo = 0;
            if (EventTypeFunc.IsMultiple(pAbandonExerciseMsgBase.abandonExerciseType))
			{
                if (IsFX_AbandonExerciseMsg(pAbandonExerciseMsgBase))
                {
                    FX_AbandonExerciseMsgBase exercise = pAbandonExerciseMsgBase as FX_AbandonExerciseMsgBase;
                    exerciseAmount = EventTypeFunc.IsCallCurrency(pAmountType) ? exercise.callAmount.Amount : exercise.putAmount.Amount;
                    amountDispo = GetAmountDispo(pRowSource, pAmountType);
                }
                else if (IsBO_AbandonExerciseMsg(pAbandonExerciseMsgBase))
                {
                    BO_AbandonExerciseMsgBase exercise = pAbandonExerciseMsgBase as BO_AbandonExerciseMsgBase;
                    exerciseAmount = exercise.nbOptions.Quantity;
                    amountDispo = GetAmountDispo(pRowSource, EventTypeFunc.Quantity);
                }
                if (0 < (amountDispo - exerciseAmount))
                    eventCode = EventCodeFunc.Intermediary;
            }
			return eventCode;
		}
		#endregion GetEventCodeExercise

		#region Valorize
		/// <revision>
		///     <version>1.1.8</version><date>20070823</date><author>EG</author>
		///     <comment>
		///     Apply Rules to determine the date of Pre-Settlement events
		///     </comment>
		/// </revision>
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
            {
                if (IsAbandonMsg(actionMsg) && IsAllBarrierDeclared)
                {
                    if (IsFX_AbandonMsg(actionMsg))
                        ret = FX_AbandonGen(actionMsg);
                    else if (IsBO_AbandonMsg(actionMsg))
                        ret = BO_AbandonExerciseGen(actionMsg);
                }
                else if (IsExerciseMsg(actionMsg))
                {
                    if (IsFX_ExerciseMsg(actionMsg))
                        ret = FX_ExerciseGen(actionMsg);
                    else if (IsBO_ExerciseMsg(actionMsg))
                        ret = BO_AbandonExerciseGen(actionMsg);
                }
            }
            Update();
            return ret;
        }	
		#endregion Valorize
        #region FX_AbandonGen
        /// <summary>
        /// Abandon FX options
        /// </summary>
        /// <param name="pActionMsg"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel FX_AbandonGen(ActionMsgBase pActionMsg)
        {
            int newIdEParent;
            FX_AbandonMsg abandonMsg = (FX_AbandonMsg)pActionMsg;
            
            // PM 20210121 [XXXXX] Passage du message au niveau de log None
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7210), 0,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(abandonMsg.abandonExerciseType),
                new LogParam(LogTools.AmountAndCurrency(abandonMsg.callAmount.Amount, abandonMsg.callAmount.Currency)),
                new LogParam(LogTools.AmountAndCurrency(abandonMsg.putAmount.Amount, abandonMsg.putAmount.Currency))));

            DataRow rowAbandonSource = RowEvent(m_TradeAction.idE);
            Cst.ErrLevel ret = SQLUP.GetId(out int newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 3);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                #region Abandon event
                DataRow rowAbandon = AddEventAbandonExerciseOut(newIdE, rowAbandonSource, (ActionMsgBase)abandonMsg, EventCodeFunc.Abandon, abandonMsg.abandonExerciseType);
                DataRow rowEventClassAbandon = AddEventClassAbandonExerciseOut(newIdE, abandonMsg.settlementDate);
                AddEventDetailAbandonExerciseOut(newIdE, (ActionMsgBase)abandonMsg);
                #endregion Abandon event
                #region CallCurrency amount
                newIdEParent = newIdE;
                newIdE++;
                DataRow rowCcyAmount = AddEventCurrencyAmount(newIdE, newIdEParent, rowAbandon, pActionMsg, EventTypeFunc.CallCurrency);
                DataRow rowEventClassCcyAmount = AddEventClassAmount(newIdE, rowEventClassAbandon, pActionMsg);
                #endregion CallCurrency amount
                #region PutCurrency amount
                newIdE++;
                AddEventCurrencyAmount(newIdE, newIdEParent, rowCcyAmount, pActionMsg, EventTypeFunc.PutCurrency);
                AddEventClassAmount(newIdE, rowEventClassCcyAmount, pActionMsg);
                #endregion PutCurrency amount
            }
            return ret;
        }
        #endregion FX_AbandonGen
        #region FX_ExerciseGen
        /// <summary>
        /// Exercise FX options
        /// </summary>
        /// <param name="pActionMsg"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel FX_ExerciseGen(ActionMsgBase pActionMsg)
        {
            int newIdEParent;
            FX_ExerciseMsg exerciseMsg = (FX_ExerciseMsg)pActionMsg;

            // PM 20210121 [XXXXX] Passage du message au niveau de log None
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7220), 0,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(exerciseMsg.abandonExerciseType),
                new LogParam(LogTools.AmountAndCurrency(exerciseMsg.callAmount.Amount, exerciseMsg.callAmount.Currency)),
                new LogParam(LogTools.AmountAndCurrency(exerciseMsg.putAmount.Amount, exerciseMsg.putAmount.Currency)),
                new LogParam(LogTools.AmountAndCurrency(exerciseMsg.settlementAmount, exerciseMsg.settlementCurrency))));

            DataRow rowExerciseSource = RowEvent(m_TradeAction.idE);
            int nbIdE = (exerciseMsg.isCashSettlement ? 4 : 3);
            Cst.ErrLevel ret = SQLUP.GetId(out int newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                #region Exercise event
                DataRow rowExercise = AddEventAbandonExerciseOut(newIdE, rowExerciseSource, (ActionMsgBase)exerciseMsg, EventCodeFunc.Exercise, exerciseMsg.abandonExerciseType);
                DataRow rowEventClassExercise = AddEventClassAbandonExerciseOut(newIdE, exerciseMsg.settlementDate);
                if ((exerciseMsg.isCashSettlement && (false == exerciseMsg.isInTheMoney)) ||
                    false == exerciseMsg.isCashSettlement)
                    AddEventDetailExercise(newIdE, exerciseMsg, EventTypeFunc.SettlementCurrency);
                else
                    AddEventDetailAbandonExerciseOut(newIdE, exerciseMsg);
                newIdEParent = newIdE;

                #endregion Exercise event
                #region Pre-Settlement
                #region PreSettlementInfo
                IOffset offset = m_tradeLibrary.Product.ProductBase.CreateOffset(PeriodEnum.D, -2, DayTypeEnum.Business);
                // EG 20150706 [21021] Remove Payer|Receiver parameters (exerciseMsg.callAmount.payer.actor.First|exerciseMsg.callAmount.receiver.actor.First)
                EFS_SettlementInfoEntity preSettlementInfo = new EFS_SettlementInfoEntity(m_CS,
                                            exerciseMsg.callAmount.Currency, exerciseMsg.callAmount.payer.book.First, exerciseMsg.callAmount.receiver.book.First, offset);
                preSettlementInfo.SetOffset(exerciseMsg.callAmount.Currency, exerciseMsg.putAmount.Currency);
                #endregion PreSettlementInfo
                #region PreSettlementDate
                EFS_PreSettlement preSettlement = new EFS_PreSettlement(m_CS, null, exerciseMsg.settlementDate,
                                    exerciseMsg.callAmount.Currency, exerciseMsg.putAmount.Currency,
                                    preSettlementInfo.OffsetPreSettlement, preSettlementInfo.PreSettlementMethod, m_tradeLibrary.DataDocument);
                #endregion PreSettlementDate
                #endregion Pre-Settlement

                #region CallCurrency amount
                newIdE++;
                _ = AddEventCurrencyAmount(newIdE, newIdEParent, rowExercise, pActionMsg, EventTypeFunc.CallCurrency);
                _ = AddEventClassAmount(newIdE, rowEventClassExercise, pActionMsg, (false == exerciseMsg.isCashSettlement), preSettlement);
                #endregion CallCurrency amount

                #region PutCurrency amount
                newIdE++;
                AddEventCurrencyAmount(newIdE, newIdEParent, rowExercise, pActionMsg, EventTypeFunc.PutCurrency);
                AddEventClassAmount(newIdE, rowEventClassExercise, pActionMsg, (false == exerciseMsg.isCashSettlement), preSettlement);
                #endregion PutCurrency amount

                #region SettlementCurrency amount
                if (exerciseMsg.isCashSettlement && exerciseMsg.isInTheMoney)
                {
                    newIdE++;
                    AddEventSettlementCurrencyAmount(newIdE, newIdEParent, rowExercise, exerciseMsg);
                    AddEventClassAmount(newIdE, rowEventClassExercise, pActionMsg, true, preSettlement);
                    AddEventDetailExercise(newIdE, exerciseMsg,EventTypeFunc.SettlementCurrency);
                    if (exerciseMsg.rateObservationDatesSpecified)
                    {
                        newIdEParent = newIdE;
                        nbIdE = exerciseMsg.rateObservationDates.Length;
                        ret = SQLUP.GetId(out newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, nbIdE);
                        foreach (RateObservationDateMsg item in exerciseMsg.rateObservationDates)
                        {
                            AddEventObservationDates(newIdE, newIdEParent, rowExercise, exerciseMsg, item);
                            newIdE++;
                        }
                    }
                }
                #endregion SettlementCurrency amount
            }
            return ret;
        }
        #endregion FX_ExerciseGen

        #region BO_AbandonExerciseGen
        /// <summary>
        /// Abandon|Exercise Bond options
        /// </summary>
        /// <param name="pActionMsg"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel BO_AbandonExerciseGen(ActionMsgBase pActionMsg)
        {
            int newIdEParent;

            BO_AbandonExerciseMsgBase abandonExerciseMsg = (BO_AbandonExerciseMsgBase)pActionMsg;

            // PM 20210121 [XXXXX] Passage du message au niveau de log None
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 7320), 0,
                new LogParam(LogTools.IdentifierAndId(m_TradeActionGenProcess.MQueue.Identifier, m_TradeActionGenProcess.MQueue.id)),
                new LogParam(abandonExerciseMsg.abandonExerciseType),
                new LogParam(StrFunc.FmtDecimalToInvariantCulture(abandonExerciseMsg.nbOptions.Quantity)),
                new LogParam(StrFunc.FmtDecimalToInvariantCulture(abandonExerciseMsg.entitlement.Quantity)),
                new LogParam(LogTools.AmountAndCurrency(abandonExerciseMsg.notional.Amount, abandonExerciseMsg.notional.Currency))));

            DataRow rowSource = RowEvent(m_TradeAction.idE);
            Cst.ErrLevel ret = SQLUP.GetId(out int newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, IsBO_ExerciseMsg(pActionMsg) ? 5 : 4);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                // Exercise|Abandon event
                DataRow rowEvent = AddEventAbandonExerciseOut(newIdE, rowSource, (ActionMsgBase)abandonExerciseMsg,
                    IsBO_ExerciseMsg(pActionMsg) ? EventCodeFunc.Exercise : EventCodeFunc.Abandon, abandonExerciseMsg.abandonExerciseType);
                DataRow rowEventClass = AddEventClassAbandonExerciseOut(newIdE, abandonExerciseMsg.valueDate);
                AddEventDetailExercise(newIdE, abandonExerciseMsg, abandonExerciseMsg.abandonExerciseType);

                newIdEParent = newIdE;

                // Notional amount (INT|TER)
                newIdE++;
                _ = AddEventAmount(newIdE, newIdEParent, rowEvent, pActionMsg, EventTypeFunc.Nominal);
                _ = AddEventClassAmount(newIdE, rowEventClass, pActionMsg, abandonExerciseMsg.isCashSettlement);
                // NbOptions (INT|TER)
                newIdE++;
                AddEventAmount(newIdE, newIdEParent, rowEvent, pActionMsg, EventTypeFunc.Quantity);
                AddEventClassAmount(newIdE, rowEventClass, pActionMsg);
                // Entitlement (INT|TER)
                newIdE++;
                AddEventAmount(newIdE, newIdEParent, rowEvent, pActionMsg, EventTypeFunc.Underlyer);
                AddEventClassAmount(newIdE, rowEventClass, pActionMsg);

                if (IsBO_ExerciseMsg(pActionMsg))
                {
                    // Settlement|BondPayment (INT|TER)
                    newIdE++;
                    string eventTypeSettlement = abandonExerciseMsg.isCashSettlement ? EventTypeFunc.SettlementCurrency : EventTypeFunc.BondPayment;
                    AddEventAmount(newIdE, newIdEParent, rowEvent, pActionMsg, eventTypeSettlement);
                    AddEventClassAmount(newIdE, rowEventClass, pActionMsg, true);
                    AddEventDetailExercise(newIdE, abandonExerciseMsg, eventTypeSettlement);
                }
            }
            return ret;
        }
        #endregion BO_AbandonExerciseGen
        #endregion Methods
    }
	#endregion TradeActionGenAbandonExercise
}

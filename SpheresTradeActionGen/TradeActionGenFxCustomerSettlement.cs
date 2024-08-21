#region Using Directives
using System;
using System.Data;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
#endregion Using Directives

namespace EFS.Process
{
	#region TradeActionGenProcessFxCustomerSettlement
    public class TradeActionGenProcessFxCustomerSettlement : TradeActionGenProcessBase
	{
		#region Members
        private CustomerSettlementRateMsg m_CustomerSettlementRateMsg;
		private decimal m_ForwardRate;
		private decimal m_SettlementRate;
		#endregion Members
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public TradeActionGenProcessFxCustomerSettlement(TradeActionGenProcess pTradeActionGenProcess, DataSetTrade pDsTrade, 
            EFS_TradeLibrary pTradeLibrary, TradeActionMQueue pTradeAction)
            : base(pTradeActionGenProcess, pDsTrade, pTradeLibrary, pTradeAction)
		{
			//CodeReturn = Valorize();
		}
		#endregion Constructors
		#region Methods
		#region ISDADifferential
		private decimal ISDADifferential(DataRow pRowSettlement,DataRow pRowSettlementDetail)
		{
            decimal notionalAmount               = 0;
			decimal conversionRate               = 1;
			string  currency                     = pRowSettlement["UNIT"].ToString();

            if (false == Convert.IsDBNull(pRowSettlementDetail["NOTIONALAMOUNT"]))
			    notionalAmount = Convert.ToDecimal(pRowSettlementDetail["NOTIONALAMOUNT"]);
			if (false == Convert.IsDBNull(pRowSettlementDetail["RATE"]))
				m_ForwardRate  = Convert.ToDecimal(pRowSettlementDetail["RATE"]);
			if (false == Convert.IsDBNull(pRowSettlementDetail["CONVERSIONRATE"]))
				conversionRate = Convert.ToDecimal(pRowSettlementDetail["CONVERSIONRATE"]);
			if (false == Convert.IsDBNull(pRowSettlementDetail["SETTLEMENTRATE"]))
				m_SettlementRate = Convert.ToDecimal(pRowSettlementDetail["SETTLEMENTRATE"]);

            decimal differentialAmount;
            if (0 != m_SettlementRate)
                differentialAmount = notionalAmount * (1 - (m_ForwardRate / m_SettlementRate));
            else
                differentialAmount = notionalAmount;

            decimal settlementDifferentialAmount = Math.Abs(differentialAmount * conversionRate);
            EFS_Cash cash                = new EFS_Cash(m_CS,settlementDifferentialAmount,currency);
			settlementDifferentialAmount = cash.AmountRounded;
			return settlementDifferentialAmount;
		}
		#endregion ISDADifferential
		#region RowReset
		protected  DataRow RowReset(string pEventType)
		{
			DataRow rowReset    = null;
			DataRow rowParent   = RowEvent(m_CustomerSettlementRateMsg.idE);
			DataRow[] rowResets = rowParent.GetChildRows(DsEvents.ChildEvent);
			if (0 != rowResets.Length)
			{
				foreach (DataRow row in rowResets)
				{
					if (EventCodeFunc.IsReset(row["EVENTCODE"].ToString()) && 
						(pEventType == row["EVENTTYPE"].ToString()))
					{
						rowReset = row;
						break;
					}
				}
			}
			return rowReset;
		}
		#endregion RowReset
		#region Valorize
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public override Cst.ErrLevel Valorize()
        {
            foreach (ActionMsgBase actionMsg in m_TradeAction.ActionMsgs)
            {
                if (IsFxCustomerSettlementMsg(actionMsg))
                {
                    #region CustomerSettlementRate Process
                    m_CustomerSettlementRateMsg = actionMsg as CustomerSettlementRateMsg;
                    #region Original TER-SCU + EventClass + Reset
                    DataRow rowSettlementCurrency = RowEvent(m_CustomerSettlementRateMsg.idE);
                    DataRow rowSettlementCurrencyDetail = RowDetail(m_CustomerSettlementRateMsg.idE);
                    DataRow rowEventClassSettlementCurrency = RowSettlement(m_CustomerSettlementRateMsg.idE);
                    DataRow rowEventClassPreSettlementCurrency = RowPreSettlement(m_CustomerSettlementRateMsg.idE);
                    DataRow rowResetSettlementCurrency = RowReset(EventTypeFunc.FxRate);
                    if (null == rowResetSettlementCurrency)
                        rowResetSettlementCurrency = RowReset(EventTypeFunc.FxRate1);
                    #endregion Original TER-SCU + EventClass + Reset

                    #region Reset Customer + EventClass + Detail
                    DataRow rowResetCustomer = RowReset(EventTypeFunc.FxCalculationAgent);
                    DataRow rowResetCustomerDetail;
                    DataRow rowEventClassResetCustomer;
                    int newIdE;
                    if (null != rowResetCustomer)
                    {
                        newIdE = Convert.ToInt32(rowResetCustomer["IDE"]);
                        rowEventClassResetCustomer = RowFixing(newIdE);
                        rowResetCustomerDetail = RowDetail(newIdE);
                    }
                    else
                    {
                        rowResetCustomer = DtEvent.NewRow();
                        rowEventClassResetCustomer = DtEventClass.NewRow();
                        rowResetCustomerDetail = DtEventDet.NewRow();

                        DtEvent.Rows.Add(rowResetCustomer);
                        DtEventClass.Rows.Add(rowEventClassResetCustomer);
                        DtEventDet.Rows.Add(rowResetCustomerDetail);
                        _ = SQLUP.GetId(out newIdE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                        rowResetCustomer.ItemArray = (object[])rowResetSettlementCurrency.ItemArray.Clone();
                    }
                    #endregion Reset Customer + EventClass + Detail
                    if (null != rowResetCustomer)
                    {
                        rowResetCustomer.BeginEdit();
                        rowResetCustomer["IDE"] = newIdE;
                        rowResetCustomer["EVENTTYPE"] = EventTypeFunc.FxCalculationAgent;
                        rowResetCustomer["VALORISATION"] = m_CustomerSettlementRateMsg.fixingRate;
                        rowResetCustomer["SOURCE"] = m_TradeActionGenProcess.AppInstance.ServiceName;
                        rowResetCustomer.EndEdit();
                        SetRowStatus(rowResetCustomer, Tuning.TuningOutputTypeEnum.OES);

                        rowResetCustomerDetail.BeginEdit();
                        rowResetCustomerDetail["IDE"] = newIdE;
                        rowResetCustomerDetail["DTFIXING"] = m_CustomerSettlementRateMsg.valueDate;
                        rowResetCustomerDetail["RATE"] = m_CustomerSettlementRateMsg.fixingRate;
                        rowResetCustomerDetail["NOTE"] = m_CustomerSettlementRateMsg.note;
                        rowResetCustomerDetail["DTACTION"] = m_CustomerSettlementRateMsg.actionDate;
                        rowResetCustomerDetail.EndEdit();

                        rowEventClassResetCustomer.BeginEdit();
                        rowEventClassResetCustomer["IDE"] = newIdE;
                        rowEventClassResetCustomer["EVENTCLASS"] = EventClassFunc.Fixing;
                        rowEventClassResetCustomer["DTEVENT"] = m_CustomerSettlementRateMsg.valueDate;
                        //PL 20100629 
                        //rowEventClassResetCustomer["DTEVENTFORCED"] = m_CustomerSettlementRateEvent.valueDate;
                        rowEventClassResetCustomer["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_CustomerSettlementRateMsg.valueDate);
                        rowEventClassResetCustomer["ISPAYMENT"] = false;
                        rowEventClassResetCustomer.EndEdit();

                        rowSettlementCurrencyDetail.BeginEdit();
                        rowSettlementCurrencyDetail["SETTLEMENTRATE"] = m_CustomerSettlementRateMsg.fixingRate;
                        rowSettlementCurrencyDetail.EndEdit();
                        // EG 20160404 Migration vs2013
                        // #warning ATTENTION Code dupliqué avec la Valo FX (restructuration indispensable EG 11/12/2006)
                        #region Settlement Amount Valuation
                        decimal settlementAmount = ISDADifferential(rowSettlementCurrency, rowSettlementCurrencyDetail);
                        rowSettlementCurrency.BeginEdit();
                        rowSettlementCurrency["VALORISATION"] = settlementAmount;

                        //Recherche de la ligne STA en ReferenceCurrency
                        DataRow rowParent = rowSettlementCurrency.GetParentRow(DsEvents.ChildEvent);
                        string currencyReference = rowSettlementCurrencyDetail["IDC_REF"].ToString();
                        foreach (DataRow rowChild in rowParent.GetChildRows(DsEvents.ChildEvent))
                        {
                            if (EventCodeFunc.IsStart(rowChild["EVENTCODE"].ToString()) &&
                                (currencyReference == rowChild["UNIT"].ToString()))
                            {
                                //Synchronisation des acteurs/books avec la ligne STA en ReferenceCurrency
                                rowSettlementCurrency["IDA_PAY"] = rowChild["IDA_PAY"];
                                rowSettlementCurrency["IDB_PAY"] = rowChild["IDB_PAY"];
                                rowSettlementCurrency["IDA_REC"] = rowChild["IDA_REC"];
                                rowSettlementCurrency["IDB_REC"] = rowChild["IDB_REC"];
                                break;
                            }
                        }
                        //
                        if (m_SettlementRate > m_ForwardRate)
                            CommonValFunc.SwapPayerAndReceiver(rowSettlementCurrency);
                        //
                        rowSettlementCurrency.EndEdit();

                        rowEventClassSettlementCurrency.BeginEdit();
                        rowEventClassSettlementCurrency["DTEVENT"] = m_CustomerSettlementRateMsg.valueDate;
                        rowEventClassSettlementCurrency["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, m_CustomerSettlementRateMsg.valueDate);
                        rowEventClassSettlementCurrency.EndEdit();

                        rowEventClassPreSettlementCurrency.BeginEdit();

                        EFS_FxPreSettlement preSettlement = new EFS_FxPreSettlement(m_CS, m_tradeLibrary.Product,
                            m_CustomerSettlementRateMsg.valueDate, rowSettlementCurrency["UNIT"].ToString(), currencyReference, m_tradeLibrary.DataDocument);
                        rowEventClassPreSettlementCurrency["DTEVENT"] = preSettlement.AdjustedPreSettlementDate.DateValue;
                        rowEventClassPreSettlementCurrency["DTEVENTFORCED"] = OTCmlHelper.GetAnticipatedDate(m_CS, preSettlement.AdjustedPreSettlementDate.DateValue);
                        rowEventClassPreSettlementCurrency.EndEdit();


                        #endregion Settlement Amount Valuation
                    }
                    #endregion CustomerSettlementRate Process
                }
            }
            Update();
            return Cst.ErrLevel.SUCCESS;
        }	
		#endregion Valorize
		#endregion Methods
	}
	#endregion TradeActionGenFxCustomerSettlement
}

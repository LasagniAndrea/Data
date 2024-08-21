#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
//
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
#endregion Using Directives

namespace EFS.Process
{
    /// <summary>
    /// Description résumée de EventsValProcessFX.
    /// </summary>
    #region EventsValProcessFX
    public class EventsValProcessFX : EventsValProcessBase
    {
        #region Members
        private readonly CommonValParameters m_Parameters;
        #endregion Members
        #region Accessors
        #region Parameters
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
        #endregion Accessors
        #region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public EventsValProcessFX(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_Parameters = new CommonValParametersFX();
            // EG 20150617 [20665]
            //InitializeDataSetEvent();
        }
        #endregion Constructors
        #region Methods
        #region IsRowMustBeCalculate
        public override bool IsRowMustBeCalculate(DataRow pRow)
        {
            string eventCode = pRow["EVENTCODE"].ToString();
            string eventType = pRow["EVENTTYPE"].ToString();
            if (null != m_EventsValMQueue.quote)
            {
                if (IsQuote_FxRate)
                {
                    if (EventCodeAndEventTypeFunc.IsFxRateReset(eventCode, eventType))
                        return (IsRowHasFixingEvent(pRow));
                    else
                    {
                        DataRow[] drChild = pRow.GetChildRows(this.DsEvents.ChildEvent);
                        if (ArrFunc.IsFilled(drChild))
                        {
                            foreach (DataRow row in drChild)
                            {
                                eventCode = row["EVENTCODE"].ToString();
                                eventType = row["EVENTTYPE"].ToString();
                                if (EventCodeAndEventTypeFunc.IsFxRateReset(eventCode, eventType) && IsRowHasFixingEvent(row))
                                    return true;
                            }
                        }
                    }
                }
            }
            else
                return true;
            return false;
        }
        #endregion IsRowMustBeCalculate
        #region GetRowSettlement
        public DataRow[] GetRowSettlement()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE = '{2}'",
            m_ParamIdT.Value, EventCodeFunc.Termination, EventTypeFunc.SettlementCurrency), "IDE");
        }
        #endregion GetRowSettlement
        #region GetRowExchangeCurrencyFixing
        public DataRow[] GetRowExchangeCurrencyFixing()
        {
            return DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTTYPE in ('{1}','{2}')",
            m_ParamIdT.Value, EventTypeFunc.Currency1, EventTypeFunc.Currency2), "IDE");
        }
        #endregion GetRowExchangeCurrencyFixing
        #region Valorize
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ArrayList alSpheresException = new ArrayList();

            #region ExchangeCurrency with fixing Process
            DataRow[] rowsExchangeCurrencyFixing = GetRowExchangeCurrencyFixing();

            if (ArrFunc.IsFilled(rowsExchangeCurrencyFixing))
            {
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                    new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                    new LogParam("STA-TER / CU1-CU2")));
            }

            bool isError;
            foreach (DataRow rowExchangeCurrencyFixing in rowsExchangeCurrencyFixing)
            {
                isError = false;
                m_ParamInstrumentNo.Value = Convert.ToInt32(rowExchangeCurrencyFixing["INSTRUMENTNO"]);
                m_ParamStreamNo.Value = Convert.ToInt32(rowExchangeCurrencyFixing["STREAMNO"]);
                //
                int idE = Convert.ToInt32(rowExchangeCurrencyFixing["IDE"]);
                bool isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowExchangeCurrencyFixing);
                //
                if (isRowMustBeCalculate)
                {
                    EFS_ExchangeCurrencyFixingEvent exchangeCurrencyFixingEvent = null;
                    try
                    {
                        Parameters.Add(m_CS, m_tradeLibrary, rowExchangeCurrencyFixing);
                        exchangeCurrencyFixingEvent = new EFS_ExchangeCurrencyFixingEvent(this, rowExchangeCurrencyFixing);
                    }
                    catch (SpheresException2 ex)
                    {
                        isError = true;
                        if (ex.IsStatusError)
                            alSpheresException.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        isError = true;
                        alSpheresException.Add(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                    }
                    finally
                    {
                        Update(idE, isError);
                    }
                }
            }
            #endregion ExchangeCurrency with fixing Process
            //
            #region Payment Process
            DataRow[] rowsSettlement = GetRowSettlement();

            if (ArrFunc.IsFilled(rowsSettlement))
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                    new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                    new LogParam("TER/STL")));
            }

            foreach (DataRow rowSettlement in rowsSettlement)
            {
                isError = false;
                m_ParamInstrumentNo.Value = Convert.ToInt32(rowSettlement["INSTRUMENTNO"]);
                m_ParamStreamNo.Value = Convert.ToInt32(rowSettlement["STREAMNO"]);
                int idE = Convert.ToInt32(rowSettlement["IDE"]);
                //
                if (IsRowMustBeCalculate(rowSettlement))
                {
                    EFS_SettlementEvent settlementEvent = null;
                    try
                    {
                        Parameters.Add(m_CS, m_tradeLibrary, rowSettlement);
                        settlementEvent = new EFS_SettlementEvent(this, rowSettlement);
                    }
                    catch (SpheresException2 ex)
                    {
                        isError = true;
                        if (ex.IsStatusError)
                            alSpheresException.Add(ex);
                    }
                    catch (Exception ex)
                    {
                        isError = true;
                        alSpheresException.Add(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                    }
                    finally
                    {
                        Update(idE, isError);
                    }
                }
            }
            #endregion Payment Process

            if (ArrFunc.IsFilled(alSpheresException))
            {
                ret = Cst.ErrLevel.ABORTED;
                foreach (SpheresException2 ex in alSpheresException)
                {
                    Logger.Log(new LoggerData(ex));
                }
            }
            return ret;

        }
        #endregion Valorize
        #endregion Methods
    }
    #endregion EventsValProcessFX
}

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
    #region EventsValProcessIRD
    public class EventsValProcessIRD : EventsValProcessBase
	{
		#region Members
		private readonly CommonValParameters m_Parameters;
		#endregion Members
		#region Accessors
		#region Parameters
        public override CommonValParameters ParametersIRD
        {
            get { return m_Parameters; }
        }
		public override CommonValParameters Parameters
		{
			get {return m_Parameters;}
		}
		#endregion Parameters
		#endregion Accessors
		#region Constructors
        // EG 20180502 Analyse du code Correction [CA2214]
        public EventsValProcessIRD(EventsValProcess pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
			:base(pProcess, pDsTrade, pTradeLibrary,pProduct)
		{
			m_Parameters = new CommonValParametersIRD();
            // EG 20150617 [20665]
            //InitializeDataSetEvent();
		}
		#endregion Constructors
		#region Methods
		#region IsRowHasNominalChanged
		private  bool IsRowHasNominalChanged(DataRow pRow)
		{
			DataRow rowNominal = GetCurrentNominal(Convert.ToDateTime(pRow["DTSTARTADJ"]));
            DataRow rowVariationNominal = GetRowVariationNominal(Convert.ToDateTime(rowNominal["DTSTARTADJ"]));
            if (null != rowVariationNominal)
            {
                DataRow[] rowAssets = GetRowAsset(Convert.ToInt32(rowVariationNominal["IDE"]));
                if ((null != rowAssets) && (0 < rowAssets.Length))
                {
                    DataRow rowAsset = rowAssets[0];
                    if (IsQuote_FxRate && (m_Quote.time <= Convert.ToDateTime(rowNominal["DTSTARTADJ"])) && (m_Quote.idAsset == Convert.ToInt32(rowAsset["IDASSET"])))
                        return true;
                }
            }
			return false;
		}
		#endregion IsRowHasNominalChanged
		#region IsRowMustBeCalculate
		public override bool IsRowMustBeCalculate(DataRow pRow)
		{
            string eventCode   = pRow["EVENTCODE"].ToString();
			string eventType   = pRow["EVENTTYPE"].ToString();

            if (null != m_EventsValMQueue.quote)
			{
				if (IsQuote_RateIndex)
				{
					if (EventCodeFunc.IsReset(eventCode) || EventCodeFunc.IsSelfReset(eventCode))
					{
						if (IsRowHasFixingEvent(pRow))
							return true;
						else
							return IsRowHasChildrens(pRow);
					}
					else
						return IsRowHasChildrens(pRow);

				}
				else if (IsQuote_FxRate)
				{
					if (EventCodeAndEventTypeFunc.IsNominalPeriodVariation(eventCode,eventType))
					{
						if (IsRowHasFixingEvent(pRow))
							return true;
						else
							return IsRowHasNominalChanged(pRow);
					}
					else
					{
						if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode))
							return IsRowHasNominalChanged(pRow);
						else
							return true;
					}
				}
			}
			else
				return true;
			return false;
		}
		#endregion IsRowMustBeCalculate        
		
        #region Valorize
        /// <revision>
		///     <build>23</build><date>20050808</date><author>PL</author>
		///     <comment>
		///     Add CancelEdit()
		///     </comment>
		/// </revision>
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Valorize()
		{
			Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ArrayList alSpheresException = new ArrayList();

            #region Nominal Process (FxLinkedNotional Case)
            bool isRowMustBeCalculate = false;
            DataRow[] rowVariations = GetRowAllVariationNominal();
            bool isError;
            if (ArrFunc.IsFilled(rowVariations))
            {
                if (ArrFunc.IsFilled(rowVariations))
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                        new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                        new LogParam("STA-INT-TER / NOM")));
                }

                foreach (DataRow rowVariation in rowVariations)
                {
                    isError = false;
                    bool isRowHasFixingEvent = false;
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowVariation["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowVariation["STREAMNO"]);
                    int idE = Convert.ToInt32(rowVariation["IDE"]);

                    EFS_VariationNominalEvent variationNominalEvent;
                    try
                    {
                        Parameters.Add(m_CS, m_tradeLibrary, rowVariation);
                        // ScanCompatibility_Event
                        isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                        // isRowMustBeCalculate
                        isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowVariation);
                        //
                        if (isRowMustBeCalculate)
                        {
                            isRowHasFixingEvent = IsRowHasFixingEvent(rowVariation);
                            if (isRowHasFixingEvent)
                            {
                                rowVariation.BeginEdit();
                                CommonValParameterIRD parameter = (CommonValParameterIRD)Parameters[ParamInstrumentNo, ParamStreamNo];
                                if (Cst.ErrLevel.SUCCESS == parameter.GetStreamNotionalReference(out int instrumentNoReference, out int streamNoReference))
                                {
                                    m_ParamInstrumentNoReference.Value = instrumentNoReference;
                                    m_ParamStreamNoReference.Value = streamNoReference;
                                    variationNominalEvent = new EFS_VariationNominalEvent(CommonValDate, this, rowVariation);
                                }
                            }
                        }
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
                        bool cancelEdit = true;
                        if (isRowMustBeCalculate)
                        {
                            if (isRowHasFixingEvent)
                            {
                                cancelEdit = false;
                                rowVariation.EndEdit();
                                Update(idE, isError);
                            }
                        }
                        if (cancelEdit)
                            rowVariation.CancelEdit();
                    }
                }
            }
            #endregion Nominal Process (FxLinkedNotional Case)

            #region Payment Process
            DataRow[] rowsInterest = GetRowInterest();
            if (ArrFunc.IsFilled(rowsInterest))
            {
                if (ArrFunc.IsFilled(rowsInterest))
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 601), 3,
                        new LogParam(LogTools.IdentifierAndId(m_EventsValProcess.MQueue.Identifier, m_EventsValProcess.MQueue.id)),
                        new LogParam("STA-INT-TER / INT")));
                }

                foreach (DataRow rowInterest in rowsInterest)
                {
                    isError = false;
                    m_ParamInstrumentNo.Value = Convert.ToInt32(rowInterest["INSTRUMENTNO"]);
                    m_ParamStreamNo.Value = Convert.ToInt32(rowInterest["STREAMNO"]);
                    int idE = Convert.ToInt32(rowInterest["IDE"]);
                    // ScanCompatibility_Event
                    isRowMustBeCalculate = (Cst.ErrLevel.SUCCESS == m_EventsValProcess.ScanCompatibility_Event(idE));
                    // isRowMustBeCalculate
                    isRowMustBeCalculate = isRowMustBeCalculate && IsRowMustBeCalculate(rowInterest);
                    //
                    if (isRowMustBeCalculate)
                    {
                        try
                        {
                            rowInterest.BeginEdit();
                            Parameters.Add(m_CS, m_tradeLibrary, rowInterest);
                            new EFS_PaymentEvent(CommonValDate, this, rowInterest);
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
                            rowInterest.EndEdit();
                            Update(idE, isError);
                        }
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
	#endregion EventsValIRD
}

#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

using EFS.Common;
using EFS.Tuning;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Interface;
#endregion Using Directives


namespace EFS.Process.Accruals
{
	#region public class AccrualsGenProcessFX
	public class AccrualsGenProcessFX : AccrualsGenProcessBase
	{
		#region Constructors
		public AccrualsGenProcessFX(AccrualsGenProcess pProcess,DataSetTrade pDsTrade,EFS_TradeLibrary pTradeLibrary,IProduct pProduct)
			: base(pProcess,pDsTrade,pTradeLibrary,pProduct)
		{
			m_Parameters = new CommonValParametersFX();
		}
		#endregion Constructors
		#region Methods
		#region IsRowMustBeCalculate
		public override bool IsRowMustBeCalculate(DataRow pRow)
		{
			bool ret = true;
			DateTime startDate = Convert.ToDateTime(pRow["DTSTARTUNADJ"]);
			DateTime endDate   = Convert.ToDateTime(pRow["DTENDUNADJ"]);
			string eventCode   = pRow["EVENTCODE"].ToString();
			string eventType   = pRow["EVENTTYPE"].ToString();
			//
			if (DtFunc.IsDateTimeFilled(CommonValDate))
			{
				
				if (EventTypeFunc.IsInterest(eventType) || EventCodeFunc.IsCalculationPeriod(eventCode))
				{
					if ((CommonValDate <= startDate) || (CommonValDate > endDate))
						ret =  false;
				}
				else if (EventCodeFunc.IsDailyClosing(eventCode))
					ret = false;
			}
			else
				ret = false;
			return ret;
		}
		#endregion IsRowMustBeCalculate
		#region IsRowsEventCalculated
		public  override bool IsRowsEventCalculated(DataRow[] pRows)
		{
			foreach (DataRow row in pRows)
			{
				DateTime startDate = Convert.ToDateTime(row["DTSTARTUNADJ"]);
				DateTime endDate   = Convert.ToDateTime(row["DTENDUNADJ"]);

				if (StatusCalculFunc.IsToCalculate(row["IDSTCALCUL"].ToString()))
				{
					if (DtFunc.IsDateTimeFilled(CommonValDate))
					{
						if ((CommonValDate > startDate) && (CommonValDate <= endDate))
						{
							DataRow[] rowChilds = row.GetChildRows(DsEvents.ChildEvent);
							if (0 != rowChilds.Length)
								return IsRowsEventCalculated(rowChilds);
							return false;
						}
					}
					return false;
				}
			}
			return true;
		}
		#endregion IsRowsEventCalculated
		#region Valorize
		/// <revision>
		///     <build>23</build><date>20050808</date><author>PL</author>
		///     <comment>
		///     Add CancelEdit()
		///     </comment>
		/// </revision>
		public override Cst.ErrLevel Valorize()
		{
            return Cst.ErrLevel.SUCCESS;
		}	
		#endregion Valorize
		#endregion Methods
	}
	#endregion AccrualsGenProcessFX
}

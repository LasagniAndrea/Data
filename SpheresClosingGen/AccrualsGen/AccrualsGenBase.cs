#region Using Directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Globalization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;


using EFS.Status;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.Accruals
{
	#region classAccrualsGenProcessBase
	public abstract class AccrualsGenProcessBase : CommonValProcessBase
	{
		#region Members
        protected CommonValParameters m_Parameters;
		protected AccrualsGenProcess m_AccrualsGenProcess;
		protected AccrualsGenMQueue  m_AccrualsGenMQueue;
		#endregion Members
		#region Accessors
		#region CommonValDate
		public override DateTime CommonValDate
		{
            get
            {
                DateTime ret = DateTime.MinValue;
                if (m_Process.MQueue.IsMasterDateSpecified)
                    ret = m_Process.MQueue.GetMasterDate();
                return ret;
            }
		}
		#endregion CommonValDate
		#region CommonValDateIncluded
        public override DateTime CommonValDateIncluded
        {
            get
            {
                DateTime ret = DateTime.MinValue;
                if (m_Process.MQueue.IsMasterDateSpecified)
                {
                    //20070522 PL Pour Bancaperta on n'inclue pas le dernier jour sur un réescompte
                    bool isEndDayIncluded = true;
                    try
                    {
                        string sqlQuery = SQLCst.SELECT + "ISAINWITHENDDAY" + Cst.CrLf;
                        sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
                        sqlQuery += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);
                        object obj = DataHelper.ExecuteScalar(m_CS, CommandType.Text, sqlQuery);
                        if (null != obj)
                            isEndDayIncluded = Convert.ToBoolean(obj);
                    }
                    catch { isEndDayIncluded = true; }
                    ret = m_Process.MQueue.GetMasterDate().AddDays(isEndDayIncluded ? 1 : 0);
                }
                return ret;
            }
        }
		#endregion CommonValDateIncluded
        #region Parameters
        public override CommonValParameters Parameters
        {
            get { return m_Parameters; }
        }
        #endregion Parameters
		#endregion Accessors
		#region Constructor
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        // EG 20180502 Analyse du code Correction [CA2214]
        public AccrualsGenProcessBase(AccrualsGenProcess pAccrualsGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pAccrualsGenProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_AccrualsGenProcess = pAccrualsGenProcess;
            m_AccrualsGenMQueue = (AccrualsGenMQueue)m_AccrualsGenProcess.MQueue;
            // EG 20150612 [20665]
            //InitializeDataSetEvent();
        }
        #endregion Constructor
		#region Methods
        #region GetRowAccrual
        public DataRow[] GetRowAccrual()
        {
            DataRow[] ret = null;
            if (null != DsEvents)
            {
                ret = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"EVENTTYPE='{0}' and IDE_EVENT={1} and DTENDUNADJ='{2}'", 
                    EventTypeFunc.AccrualInterests, m_ParamIdE.Value, CommonValDate.ToString(DtFunc.FmtISODate)), "IDE");
            }
            return ret;
        }
        #endregion GetRowAccrual
		#region Valorize
		public override Cst.ErrLevel Valorize()
		{
			return Cst.ErrLevel.UNDEFINED;
		}
		#endregion Valorize
		#endregion Methods
    }
    #endregion AccrualsGenProcessBase
}

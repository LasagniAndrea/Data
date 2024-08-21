#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML;
using EfsML.Business;
using EfsML.Enum.Tools;
using FpML.Interface;
using System;
using System.Data;
#endregion Using Directives

namespace EFS.Process.LinearDepreciation
{
    #region LinearDepGenTreatment
    public class LinearDepGenTreatment : CommonValProcessBase
	{
		#region Members
		protected LinearDepGenProcess m_LinearDepGenProcess;
		#endregion Members
		#region Accessors
		#region CommonValDate
        /// <summary>
        /// Obtient la date de traitement
        /// </summary>
		public override DateTime CommonValDate
		{
			get 
			{
                if (m_Process.MQueue.IsMasterDateSpecified)
                    return m_Process.MQueue.GetMasterDate();
				return DateTime.MinValue;
			}
		}
		#endregion CommonValDate
		#region CommonValDateIncluded
        /// <summary>
        /// Obtient la date Fin du traitement, cette date est exclue
        /// <para>Pour Bancaperta Spheres® n'inclue pas le dernier jour</para>
        /// </summary>
		public override DateTime CommonValDateIncluded
		{
            get
            {
                DateTime ret = DateTime.MinValue;
                if (m_Process.MQueue.IsMasterDateSpecified)
                {
                    bool isEndDayIncluded = true;
                    try
                    {
                        string sqlQuery = SQLCst.SELECT + "ISAINWITHENDDAY" + Cst.CrLf;
                        sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
                        sqlQuery += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);
                        //
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
        #region public RowLinearDepreciation
        public DataRow RowLinearDepreciation
        {
            get
            {
                DataRow row = null;
                if (null != DsEvents)
                {
                    DataRow[] rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDE_EVENT = {0} and EVENTCODE = '{1}' and DTENDUNADJ='{2}'",
                            m_ParamIdE.Value, EventCodeFunc.DailyClosing, CommonValDate.ToString(DtFunc.FmtISODate)) , "IDE");
                    if ((null != rows) && (0 < rows.Length))
                        row = rows[0];
                }
                return row;
            }
        }
		#endregion RowLinearDepreciation
        #endregion Accessors
		#region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLinearDepGenProcess">Représente le Process Appelant</param>
        /// <param name="pDsTrade">Réprésente le Dataset associé au Trade</param>
        /// <param name="pTradeLibrary">Réprésente le Trade</param>
        /// <param name="pProduct">Réprésente un produit du Trade</param>
        // EG 20150612 [20665] Refactoring : Chargement DataSetEventTrade
        public LinearDepGenTreatment(LinearDepGenProcess pLinearDepGenProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary, IProduct pProduct)
            : base(pLinearDepGenProcess, pDsTrade, pTradeLibrary, pProduct)
        {
            m_LinearDepGenProcess = pLinearDepGenProcess;
            // EG 20150617 [20665] InitializeDataSetEvent moved to Valorize() method
            // EG 20150612 [20665]
            //InitializeDataSetEvent();
        }
        #endregion Constructor
		#region Methods
		#region AddNewRowEvent
        protected void AddNewRowEvent(DataRow pRow, int pIdE)
        {
            DataRow rowLinearDepreciation = DsEvents.DtEvent.NewRow();
            rowLinearDepreciation.ItemArray = (object[])pRow.ItemArray.Clone();
            rowLinearDepreciation.BeginEdit();
            rowLinearDepreciation["IDE"] = pIdE;
            rowLinearDepreciation["IDE_EVENT"] = pRow["IDE"];
            rowLinearDepreciation["EVENTCODE"] = EventCodeFunc.DailyClosing;
            rowLinearDepreciation["SOURCE"] = m_LinearDepGenProcess.AppInstance.ServiceName;
            rowLinearDepreciation["IDSTTRIGGER"] = Cst.StatusTrigger.StatusTriggerEnum.NA.ToString();
            rowLinearDepreciation["DTENDADJ"] = CommonValDate;
            rowLinearDepreciation["DTENDUNADJ"] = CommonValDate;
            rowLinearDepreciation.EndEdit();
            DsEvents.DtEvent.Rows.Add(rowLinearDepreciation);
        }
		#endregion AddNewRowEvent
		#region AddRowLinearDepreciation
        private Cst.ErrLevel AddRowLinearDepreciation(DataRow pRow, decimal pLinearDepreciationAmount, string pEventClass)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            m_ParamIdE.Value = Convert.ToInt32(pRow["IDE"]);
            DataRow rowLinearDepreciation = RowLinearDepreciation;
            int idE;

            if (null == rowLinearDepreciation)
            {
                #region NewRow LinearDepreciation
                ret = SQLUP.GetId(out idE, m_CS, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, 1);
                if (Cst.ErrLevel.SUCCESS == ret)
                    AddNewRowEvent(pRow, idE);
                #endregion NewRow LinearDepreciation
            }

            if (Cst.ErrLevel.SUCCESS == ret)
            {
                #region LinearDepreciation Row valuation
                rowLinearDepreciation = RowLinearDepreciation;
                idE = Convert.ToInt32(rowLinearDepreciation["IDE"]);
                m_ParamIdE.Value = idE;
                //
                rowLinearDepreciation.BeginEdit();
                rowLinearDepreciation["IDE"] = idE;
                rowLinearDepreciation["VALORISATION"] = pLinearDepreciationAmount;
                rowLinearDepreciation["VALORISATIONSYS"] = pLinearDepreciationAmount;
                SetRowStatus(rowLinearDepreciation, Tuning.TuningOutputTypeEnum.OES);
                rowLinearDepreciation.EndEdit();
                #endregion LinearDepreciation Row valuation

                #region LinearDepreciation Row EventClass
                DeleteAllRowEventClass(idE);
                if (null == GetRowEventClass(idE))
                    AddNewRowEventClass(idE, pEventClass, CommonValDate, false);
                //
                DataRow rowLinearDepreciationClass = GetRowEventClass(idE)[0];
                rowLinearDepreciationClass.BeginEdit();
                rowLinearDepreciationClass["EVENTCLASS"] = pEventClass;
                rowLinearDepreciationClass.EndEdit();
                #endregion LinearDepreciation Row EventClass

                #region	LinearDepreciation Accounting EventClass (CLA , ...)
                AddRowAccountingEventClass(idE);
                #endregion	LinearDepreciation Accounting EventClass (CLA , ...)

                #region Update/Insert Event
                Update(idE, false);
                #endregion Update/Insert Event
            }
            return ret;
        }
		#endregion AddRowLinearDepreciation

        #region GetRowAdditionalPayments
        /// <summary>
        /// Obtient les évènements [ADP,xxx] 
        /// </summary>
        /// <returns></returns>
		protected DataRow[] GetRowAdditionalPayments()
		{
            DataRow[] rows = null;
			if (null != DsEvents)
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}'", m_ParamIdT.Value, EventCodeFunc.AdditionalPayment), "IDE");
			return rows;
		} 
        #endregion RowAdditionalPayments
        #region GetRowForwardPoints
        /// <summary>
        /// Obtient les évènement [DEA,FWP] du trade
        /// <para>Obtient null si aucune correspondance</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetRowForwardPoints()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE='{2}'",
                    m_ParamIdT.Value, EventCodeFunc.DepreciableAmount, EventTypeFunc.ForwardPoints), "IDE");
            return rows;
        }
        #endregion GetRowForwardPoints
        #region GetRowPremiums
        /// <summary>
        /// Obtient les évènement [LPP,RPM] du trade
        /// <para>Obtient null si aucune correspondance</para>
        /// </summary>
        /// <returns></returns>
        protected DataRow[] GetRowPremiums()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}' and EVENTTYPE='{2}'",
                    m_ParamIdT.Value, EventCodeFunc.LinkedProductPayment, EventTypeFunc.Premium), "IDE");
            return rows;
        }
        #region GetRowOtherPartyPayments
        /// <summary>
        /// Obtient les évènement [OPP,xxx] du trade
        /// <para>Obtient null si aucune correspondance</para>
        /// </summary>
        protected DataRow[] GetRowOtherPartyPayments()
        {
            DataRow[] rows = null;
            if (null != DsEvents)
                rows = DsEvents.DtEvent.Select(StrFunc.AppendFormat(@"IDT = {0} and EVENTCODE = '{1}'",
                    m_ParamIdT.Value, EventCodeFunc.OtherPartyPayment), "IDE");
            return rows;
        }
        #endregion GetRowOtherPartyPayments
        #endregion GetRowPremiums

		#region LinearDepreciation
        /// <summary>
        /// Calcul de l'amortissement linéaire pour les évènements candidats 
        /// </summary>
        /// <param name="pRowCandidates">Représente les évènements condidats</param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel LinearDepreciation(DataRow[] pRowCandidates)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            #region Linear depreciation amount
            foreach (DataRow rowCandidate in pRowCandidates)
            {
                m_ParamInstrumentNo.Value = Convert.ToInt32(rowCandidate["INSTRUMENTNO"]);
                m_ParamStreamNo.Value = Convert.ToInt32(rowCandidate["STREAMNO"]);
                DateTime dtStart = Convert.ToDateTime(rowCandidate["DTSTARTUNADJ"]);
                DateTime dtEnd = Convert.ToDateTime(rowCandidate["DTENDUNADJ"]);
                string eventCode = rowCandidate["EVENTCODE"].ToString();
                string eventType = rowCandidate["EVENTTYPE"].ToString();

                bool isRowMustBeCalculate = (0 <= CommonValDate.CompareTo(dtStart)) && (0 < dtEnd.CompareTo(CommonValDate));
                isRowMustBeCalculate = isRowMustBeCalculate && 
                    (Cst.ErrLevel.SUCCESS == m_LinearDepGenProcess.ScanCompatibility_Event(Convert.ToInt32(rowCandidate["IDE"])));
                //
                if (isRowMustBeCalculate)
                {
                    if (EventCodeFunc.IsOtherPartyPayment(rowCandidate["EVENTCODE"].ToString()))
                        isRowMustBeCalculate = IsOppDepreciable(rowCandidate);
                }
                //
                if (isRowMustBeCalculate)
                {
                    #region Linear amount calculation
                    bool isPeriodRemaining = m_LinearDepGenProcess.CheckLinearDepPeriod(rowCandidate["IDB_PAY"].ToString(), rowCandidate["IDB_REC"].ToString());
                    int accrualDays = 0;
                    decimal linearDepreciationAmount = 0;
                    string eventClass = string.Empty;
                    //
                    decimal initialAmount = Convert.ToDecimal(rowCandidate["VALORISATION"]);
                    int totalDays = ((TimeSpan)(dtEnd - dtStart)).Days;
                    //    
                    if (isPeriodRemaining)
                    {
                        accrualDays = ((TimeSpan)(dtEnd - CommonValDateIncluded)).Days;
                        eventClass = EventClassFunc.LinearDepRemaining;
                    }
                    else
                    {
                        accrualDays = ((TimeSpan)(CommonValDateIncluded - dtStart)).Days;
                        eventClass = EventClassFunc.LinearDepreciation;
                    }
                    //
                    linearDepreciationAmount = initialAmount * accrualDays / totalDays;
                    #endregion Linear amount calculation

                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 721), 1,
                        new LogParam(LogTools.IdentifierAndId(Process.MQueue.Identifier, Process.MQueue.id)),
                        new LogParam(eventCode + " - " + eventType),
                        new LogParam(LogTools.AmountAndCurrency(initialAmount, rowCandidate["UNIT"].ToString())),
                        new LogParam(eventClass),
                        new LogParam(LogTools.AmountAndCurrency(linearDepreciationAmount, rowCandidate["UNIT"].ToString()))));

                    #region AddRowLinearDepreciation
                    ret = AddRowLinearDepreciation(rowCandidate, linearDepreciationAmount, eventClass);
                    #endregion AddRowLinearDepreciation
                }
            }
            #endregion Linear depreciation amount
            return ret;
        }
		#endregion LinearDepreciation
		#region Valorize
        // EG 20180502 Analyse du code Correction [CA2214]
        public override Cst.ErrLevel Valorize()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            // EG 20150612 [20665]
            //InitializeDataSetEvent();

            //ForwardPoint
            if (Cst.ErrLevel.SUCCESS == ret)
                ret = LinearDepreciation(GetRowForwardPoints());
            //Premium
            if (Cst.ErrLevel.SUCCESS == ret)
                ret = LinearDepreciation(GetRowPremiums());
            //AdditionalPayment
            if (Cst.ErrLevel.SUCCESS == ret)
                ret = LinearDepreciation(GetRowAdditionalPayments());
            //OtherPartyPayments
            if (Cst.ErrLevel.SUCCESS == ret)
                ret = LinearDepreciation(GetRowOtherPartyPayments());
            return ret;
        }
		#endregion Valorize

        #region Private
        /// <summary>
        /// Retourne true si l'évènement OPP en paramètre doit être amorti
        /// </summary>
        /// <param name="pRow">Représente un évènement OPP</param>
        /// <returns></returns>
        private bool IsOppDepreciable(DataRow pRow)
        {
            bool ret = true;
            StrBuilder sql = new StrBuilder(SQLCst.SELECT) + Cst.CrLf;
            sql += "ISDEPRECIABLE" + Cst.CrLf;
            sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.FEE + " f" + Cst.CrLf;
            sql += SQLCst.WHERE + "f.EVENTTYPE=@EVENTTYPE";
            //
            DataParameters param = new DataParameters();
            param.Add(DataParameter.GetParameter(m_LinearDepGenProcess.Cs, DataParameter.ParameterEnum.EVENTTYPE), pRow["EVENTTYPE"].ToString());
            QueryParameters qry = new QueryParameters(m_LinearDepGenProcess.Cs, sql.ToString(), param);
            //
            //FI et PL Dès qu'il existe un FEE telque FEE.EVENTTYPE = EVENT.EVENTTYPE où ISDEPRECIABLE = true
            //impossible ajourd'hui de remonter de l'EVENTYPE vers le FEE (plusieurs FEE peuvent avoir le même EVENTTYPE)
            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteReader(m_LinearDepGenProcess.Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
                if (dr.Read())
                {
                    if (BoolFunc.IsTrue(dr["ISDEPRECIABLE"]))
                        ret = true;
                }
            }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
            return ret;
        }
        #endregion
		#endregion Methods

        

    }
	#endregion LinearDepGenProcessBase
}

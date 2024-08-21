#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.Common;
using EFS.Tuning;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using EfsML.Interface;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    #region EventsGenProductCalculationETD
    public class EventsGenProductCalculationETD : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationETD(ProcessBase pProcess, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary)
            : base(pProcess, pDsTrade, pTradeLibrary) { }
        #endregion Constructors
        #region Methods
        #region Calculation
        public override Cst.ErrLevel Calculation()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            try
            {
                
                return ret;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("EventsGenProductCalculationETD.Calculation", ex); }
        }
        #endregion Calculation
        #endregion Methods
    }
    #endregion EventsGenProductCalculationETD
}

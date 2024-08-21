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
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.OTCmlStatus;
using EFS.Tuning;
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;

using EfsML.Interface;

using FpML.Interface;
#endregion Using Directives

namespace EFS.Process
{
    public abstract class CommonEventsGen
    {
        #region Members
        
        /// <summary>
        /// 
        /// </summary>
        protected string m_ConnectionString;
        /// <summary>
        /// 
        /// </summary>
        protected EFS_TradeLibrary m_tradeLibrary;
        /// <summary>
        /// 
        /// </summary>
        protected DataSetTrade m_DsTrade;
        
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public string CS
        {
            get { return m_ConnectionString; }
        }

        /// <summary>
        /// Obtient le trade 
        /// </summary>
        public ITrade CurrentTrade
        {
            get { return m_tradeLibrary.currentTrade; }
        }

        /// <summary>
        /// Obtient le dataSet du trade
        /// </summary>
        public DataSetTrade DsTrade
        {
            get { return m_DsTrade; }
        }

        /// <summary>
        /// Obtient le IdT du trade
        /// </summary>
        protected int idT
        {
            get { return m_DsTrade.IdT; }
        }

        #endregion Accessors

        #region Constructor
        public CommonEventsGen(string pCs, DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary)
        {
            m_ConnectionString = pCs;
            m_DsTrade = pDsTrade;
            m_tradeLibrary = pTradeLibrary;
        }
        #endregion Constructor

    }
}

#region Using Directives
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using System.IO;
using System.Xml;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Actor;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Book;


using EFS.SpheresService;
using EFS.Tuning;


using EfsML;
using EfsML.Business;
using EfsML.Notification;
using EfsML.Enum;
using EfsML.Enum.Tools;
using FpML.Interface;

#endregion Using Directives

namespace EFS.Process.Notification
{

    /// <summary>
    /// Représente un enregistrement MCO et les trades qui rentrent dans la construction de cet enregistrement
    /// </summary>
    internal class TradeMcoInput : McoInput
    {
        #region Members
        /// <summary>
        /// Liste des trades inclus dans un enregistrement MCO
        /// </summary>
        internal List<TradeInfo> trade;
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        internal TradeMcoInput()
            : base()
        {
            trade = new List<TradeInfo>();
        }
        #endregion
    }


}

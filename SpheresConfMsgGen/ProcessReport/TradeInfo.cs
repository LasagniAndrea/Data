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
    ///  Représente les informations issues d'un trade nécessaires à la messagerie
    /// </summary>
    /// FI 20140808 [20275] Modify
    /// FI 20170913 [23417] Modify
    internal struct TradeInfo
    {
        #region Members
        /// <summary>
        /// IdT
        /// </summary>
        internal int idT;

        /// <summary>
        /// identifier du trade
        /// </summary>
        internal string identifier;

        /// <summary>
        /// Instrument
        /// </summary>
        internal int idI;

        /// <summary>
        ///  Marché
        ///  <para>0 si trade de type cash Balance</para>
        /// </summary>
        /// FI 20140808 [20275] add
        internal int idM;

        /* FI 20170913 [23417]  idDC devient contractId
        /// <summary>
        ///  Derivative Contrat
        ///  <para>0 si trade autre que sur famille de produit LSD</para>
        /// </summary>
        internal int idDC;
        */
        /// <summary>
        ///  contractId 
        ///  <para>null si trade sans contract (ni LSD, ni commodity)</para>
        /// </summary>
        internal Pair<Cst.ContractCategory,int>  contractId;



        /// <summary>
        ///  Devise du trade
        /// </summary>
        internal string idC;

        /// <summary>
        /// Statut business
        /// </summary>
        internal Cst.StatusBusiness statusBusiness;

        /// PL 20140710 [20179]
        /// <summary>
        /// Statut User - Match
        /// </summary>
        internal string statusMatch;
        
        /// <summary>
        /// Statut User - Check
        /// </summary>
        internal string statusCheck;

        /// <summary>
        /// 
        /// </summary>
        internal int idIUnderlyer;

        /// <summary>
        /// Représente les évènements déclencheurs
        /// </summary>
        internal List<Int32> idE;
        #endregion
    }
}



#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Common.Web;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.Interface;



using EFS.Tuning;
using EFS.Permission;
using EFS.Status;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;

using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Représente l'environnement de sauvegarde (l'utilisateur, l'appInstane, etc..)
    /// </summary>
    public class CaptureSessionInfo
    {
        
        /// <summary>
        /// Représente le user 
        /// </summary>
        public User user;
        /// <summary>
        /// Représente la session
        /// </summary>
        public AppSession session;
        /// <summary>
        /// Représente la licence
        /// </summary>
        public License licence;
        /// <summary>
        /// Renseigné si traitement (Enregistrement d'un trade sous IO par exemple)
        /// </summary>
        public Nullable<int> idTracker_L;
        /// <summary>
        /// Renseigné si traitement (Enregistrement d'un trade sous IO par exemple)/// 
        /// </summary>
        public Nullable<int> idProcess_L;

    }
}

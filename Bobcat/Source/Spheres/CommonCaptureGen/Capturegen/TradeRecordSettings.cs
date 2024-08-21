#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
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




using EFS.Status;
using EFS.Tuning;
using EFS.Permission;

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
    ///  Représente les directives lors d'une sauvegarde de trade
    /// </summary>
    /// FI 20091016 [Rebuild identification] nouveaux commentaires
    /// FI 20160907 [21831] Modify
    // EG 20190613 [24683] New typeForClosingReopeningPosition
    // EG 20200519 [XXXXX] Add recordMode (0, 1 ou 2) for CheckAndRecord performance
    public class TradeRecordSettings
    {
        #region Members
        /// <summary>
        /// Définit le displayName du trade
        /// </summary>
        public string displayName;
        /// <summary>
        /// Définit la description du trade
        /// </summary>
        public string description;
        /// <summary>
        /// Définit l'external id du trade
        /// </summary>
        public string extLink;
        /// <summary>
        /// Définit le screen 
        /// </summary>
        public string idScreen;

        /// <summary>
        /// voir PL pour cette fonctionnalité
        /// </summary>
        //public bool isUpdateOnly_TradeStream;
        /// <summary>
        /// Modification d'une trade où seuls les évènements de frais sont supprimés
        /// <para>Mode de fonctionnement particulier disponible depuis IO (Importation de trade) qui permet l'injection de frais manuels en fin de journée sur les allocations</para>
        /// <para>Hormis les frais, il est considéré qu'aucune donnée n'a évolué</para>
        /// </summary>
        /// FI 20160907 [21831] Add
        public bool isUpdateFeesOnly;
        /// <summary>
        /// Existence de frais pour lesquels le Scope est l'ORDRE et pour lesquels le barème comporte un MIN/MAX
        /// </summary>
        ///PL 20191210 [25099]
        //public bool isExistsFeeScope_OrderId;
        /// <summary>
        /// Numéro d'ORDRE
        /// </summary>
        ///PL 20191210 [25099]
        //public string OrderId;
        /// <summary>
        /// Si true l'enregistrement d'un trade applique les contrôles associés aux validation rules
        /// </summary>
        /// <returns></returns>
        public bool isCheckValidationRules;
        /// <summary>
        /// Si true l'enregistrement d'un trade applique la validation xsd
        /// </summary>
        /// <returns></returns>
        public bool isCheckValidationXSD;
        /// <summary>
        /// Si true l'enregistrement d'un trade génère l'identifier via la procedure UP_GETID (true si transaction, false si titre)
        /// </summary>
        /// <returns></returns>
        public bool isGetNewIdForIdentifier;
        /// <summary>
        /// Si true l'enregistrement d'un trade vérifie la license
        /// </summary>
        /// <returns></returns>
        public bool isCheckLicense;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isCheckActionTuning;
        /// <summary>
        /// Si true Injecte dans NOTEPAD les notes rattachées au trade source {pIdT_Source} dans le trade cible {pIdT} 
        /// </summary>
        /// <returns></returns>
        // EG 20131216 [19342] Used by CB
        public bool isCopyNotePad;
        /// <summary>
        /// Si true Injecte dans ATTACHEDDOC les documents rattachés au trade source {pIdT_Source} dans le trade Cible {pIdT} 
        /// </summary>
        /// <returns></returns>
        // EG 20131216 [19342] Used by CB
        public bool isCopyAttachedDoc;
        /// <summary>
        /// Si true copy les informations des trades liés entre trades source et trade créé/modifié
        /// </summary>
        /// <returns></returns>
        // EG 20131216 [19342] Used by CB
        public bool isUpdateTradeXMLWithTradeLink;
        /// <summary>
        ///  Si true l'asset sous jacent est inséré dans une transaction indépendante de celle attaché au processus de sauvegarde du trade
        /// </summary>
        /// EG 20140103 New Used by CA
        public bool isSaveUnderlyingInParticularTransaction;
        /// <summary>
        /// 
        /// </summary>
        // EG 20140103 [19344] New Used by CA
        public DateTime dtRefForDtEnabled;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DateTime dtCorpoAction;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Nullable<FixML.v50SP1.Enum.PositionEffectEnum> typeForClosingReopeningPosition;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140328 [19793] add
        public Boolean isCheckValidationLock;
        // EG 20200519 [XXXXX] Add

        /// <summary>
        /// Type de transaction générée dans CheckAndRecord 
        ///   <para>
        ///   0 (all in transaction),
        ///   1 (TRADE in short-transaction and others with connectionString),
        ///   2 (TRADE in short-transaction and others with connectionString and multi-threading)
        ///   </para> 
        /// </summary>
        public int recordMode;
        #endregion Members
        //
        #region constructor
        public TradeRecordSettings()
        {
            // EG 20131216 [19342] Used by CB
            isCopyNotePad = true;
            isCopyAttachedDoc = true;
            isUpdateTradeXMLWithTradeLink = true;
            // EG 20140103 New Used by CA
            isSaveUnderlyingInParticularTransaction = true;
            // EG 20140103 [19344] New Used by CA
            dtRefForDtEnabled = DateTime.MinValue;
            isCheckValidationLock = true;
            // EG 20200519 [XXXXX] Add
            recordMode = 0;
        }
        #endregion
    }
}
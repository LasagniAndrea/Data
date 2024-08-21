#region Using Directives
using System;
using System.Collections;
using System.Reflection;
using System.Data;
using System.Text;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;

using EFS.Restriction; 
using EFS.Tuning; 

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Settlement.Message;

using FpML.Enum;
#endregion Using Directives

namespace EFS.Process.SettlementMessage
{

    /// <summary>
    ///  Classe chargé de résupérer les flux pouvant rentrer dans un ESR
    ///  <para> Prise en considération des statuts de l'évènement (processTuning)</para>
    ///  <para> Un  flux (évènement) déjà impliqué dans un messaga de reglement est nécessairement exclu</para>
    /// </summary>
    public class ESRProcessEventRestriction : IRestrictionElement
    {
        #region Members
        readonly ESRGenProcessBase _process;
        #endregion Members
        #region Constructor
        public ESRProcessEventRestriction(ESRGenProcessBase pProcess)
        {
            _process = pProcess;
        }
        #endregion Constructor
        #region Member IRestrictionElement
        #region Class
        public string Class
        {
            get { return Cst.OTCml_TBL.EVENT.ToString(); }
        }
        #endregion Class
        #region GetQueryRestrictionElement
        public QueryParameters GetQueryRestrictionElement()
        {
            return _process.GetQueryEvent(true);
        }
        #endregion GetQueryRestrictionElement

        /// <summary>
        /// Retourne true si l'évènement peut rentrer dans le calcul d'un netting 
        /// </summary>
        /// <param name="pIdEvent"></param>
        /// <returns></returns>
        public bool IsItemEnabled(int pIdEvent)
        {
            bool ret = (Cst.ErrLevel.SUCCESS == _process.ScanCompatibility_Event(pIdEvent));
            if (ret)
                ret = (false == SettlementMessageTools.IsEventUseByMSO(_process.Cs, pIdEvent));
            return ret;
        }

        #endregion Member IRestrictionElement
    }

}

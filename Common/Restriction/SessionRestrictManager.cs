using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using EFS.ApplicationBlocks.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.Actor;

namespace EFS.Restriction
{
    /// <summary>
    /// Classe chargée de piloter l'alimentation de la table SESSIONRESTRICT pour une session 
    /// </summary>
    /// FI 20170124 [XXXXX] Add class 
    public class SessionRestrictManager
    {
        /// <summary>
        /// Représente l'application
        /// </summary>
        private readonly AppSession session;
        /// <summary>
        /// connectionstion string
        /// </summary>
        private readonly string cs;
        /// <summary>
        /// Représente l'utilisateur coonecté
        /// </summary>
        private readonly User user;


        /// <summary>
        /// 
        /// </summary>
        public SessionRestrictManager(string pCS, AppSession pSession, User pUser)
        {
            cs = pCS;
            session = pSession;
            user = pUser;
        }

        /// <summary>
        /// Alimentation de SESSIONRESTRICT
        /// </summary>
        public void SetRestriction(int pIdSessionRestrictId)
        {
            List<Cst.OTCml_TBL> tstTable = GetTableRestrict();

            List<Task> lstTask = new List<Task>();
            foreach (Cst.OTCml_TBL item in tstTable)
            {
                //Task task = SetRestrictionTableAsync(cs, item, user, appInstance, idSessionId);
                Task task = Task.Run(() =>  SetRestrictionTable(cs, item, user, session, pIdSessionRestrictId));
                lstTask.Add(task);
            }
            try
            {
                Task.WaitAll(lstTask.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }          
        }


        /// <summary>
        /// Alimentation de SESSIONRESTRICT
        /// </summary>
        /// <returns></returns>
        public async Task SetRestrictionAsync(int pIdSessionRestrictId)
        {
            List<Cst.OTCml_TBL> tstTable = GetTableRestrict();

            List<Task> lstTask = new List<Task>();
            foreach (Cst.OTCml_TBL item in tstTable)
            {
                Task task = SetRestrictionTableAsync(cs, item, user, session, pIdSessionRestrictId);
                lstTask.Add(task);
            }

            try
            {
                await Task.WhenAll(lstTask);
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static List<Cst.OTCml_TBL> GetTableRestrict()
        {

            List<Cst.OTCml_TBL> tstTable = new List<Cst.OTCml_TBL>();
            tstTable.AddRange(new Cst.OTCml_TBL[] { Cst.OTCml_TBL.ACTOR, Cst.OTCml_TBL.MARKET, Cst.OTCml_TBL.IOTASK });
            if (Software.IsSoftwareOTCmlOrFnOml())
                tstTable.Add(Cst.OTCml_TBL.INSTRUMENT);

            return tstTable;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClass"></param>
        public void UnSetRestriction()
        {
            UnSetRestriction(String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClass"></param>
        public void UnSetRestriction(string pClass)
        {
            SqlSessionRestrict restrict = new SqlSessionRestrict(cs, session);
            restrict.UnSetRestriction(pClass);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pUser"></param>
        /// <param name="pAppInstance"></param>
        /// <returns></returns>
        private static async Task SetRestrictionTableAsync(string pCS, Cst.OTCml_TBL pTable, User pUser, AppSession pSession, int pIdSessionId)
        {
            await Task.Run(() => SetRestrictionTable(pCS, pTable, pUser, pSession, pIdSessionId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pUser"></param>
        /// <param name="pAppInstance"></param>
        private static void SetRestrictionTable(string pCS, Cst.OTCml_TBL pTable, User pUser, AppSession pSession, int pIdSessionId)
        {
            RestrictionUserBase restrict;
            switch (pTable)
            {
                case Cst.OTCml_TBL.INSTRUMENT:
                    restrict = new RestrictionInstrument(pUser);
                    break;
                case Cst.OTCml_TBL.ACTOR:
                    restrict = new RestrictionActor(pUser);
                    break;
                case Cst.OTCml_TBL.MARKET:
                    restrict = new RestrictionMarket(pUser);
                    break;
                case Cst.OTCml_TBL.IOTASK:
                    restrict = new RestrictionIOTask(pUser);
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Table: {0} is not implemented", pTable.ToString()));
            }
            restrict.Initialize(pCS);

            SqlSessionRestrict sqlRestrict = new SqlSessionRestrict(pCS, pSession, pIdSessionId);
            sqlRestrict.UnSetRestriction(restrict.Class);
            sqlRestrict.SetRestrictUseSelectUnion(restrict);
            
        }
    }
}

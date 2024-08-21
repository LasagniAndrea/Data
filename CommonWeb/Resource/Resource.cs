using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.Common;
using EFS.ApplicationBlocks.Data;
//using NodaTime.TimeZones;

namespace EFS.Common.Web
{
    public class RessourceExtended
    {

        /// <summary>
        /// Formatage de la l'horodatage de création en fonction du profil utilisateur (fuseau horaire d'affichage des horodatages d'audit (Log, Tracker,...))
        /// <para>Précision de l'horodatage en seconde</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pCollaborator"></param>
        /// <param name="pDisplayTime">si true affichage de date/heure sinon affichge de la date uniquement</param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        public static string GetString_CreatedBy(DateTimeTz pDate, string pCollaborator, bool pDisplayTime)
        {
            return GetString_By("Msg_CreatedBy", pDate, pCollaborator, pDisplayTime);
        }
        /// <summary>
        /// Formatage de la l'horodatage de la dernière modification en fonction du profil utilisateur (fuseau horaire d'affichage des horodatages d'audit (Log, Tracker,...))
        /// <para>Précision de l'horodatage en seconde</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pCollaborator"></param>
        /// <param name="pDisplayTime">si true affichage de date/heure sinon affichge de la date uniquement</param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        public static string GetString_LastModifyBy(DateTimeTz pDate, string pCollaborator, bool pDisplayTime)
        {
            return GetString_By("Msg_LastModifyBy", pDate, pCollaborator, pDisplayTime);
        }

        /// <summary>
        /// Formatage de la l'horodatage de Checking en fonction du profil utilisateur (fuseau horaire d'affichage des horodatages d'audit (Log, Tracker,...))
        /// <para>Précision de l'horodatage en seconde</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pCollaborator"></param>
        /// <param name="pDisplayTime">si true affichage de date/heure sinon affichge de la date uniquement</param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        public static string GetString_CheckingBy(DateTimeTz pDate, string pCollaborator, bool pDisplayTime)
        {
            return GetString_By("Msg_CheckingBy", pDate, pCollaborator, pDisplayTime);
        }

        /// <summary>
        /// Formatage dd'un horodatage en fonction du profil utilisateur (fuseau horaire d'affichage des horodatages d'audit (Log, Tracker,...))
        /// <para>Précision de l'horodatage en seconde</para>
        /// </summary>
        /// <param name="pRessource"></param>
        /// <param name="pDate"></param>
        /// <param name="pCollaborator"></param>
        /// <param name="pDisplayTime">si true affichage de date/heure sinon affichge de la date uniquement</param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        private static string GetString_By(string pRessource, DateTimeTz pDate, string pCollaborator, bool pDisplayTime)
        {
            string fmtDate = DtFuncExtended.DisplayTimestampAudit(pDate, new AuditTimestampInfo()
            {
                Collaborator = SessionTools.Collaborator,
                TimestampZone = SessionTools.AuditTimestampZone,
                Precision = Cst.AuditTimestampPrecision.Second,
                Type = pDisplayTime ? "datetime" : "date"
            });

            string addMsg = string.Empty;
            if (pDate.TzdbIdSpecified && pDate.TzdbId == "Etc/UTC")
            {
                DateTime dtSysUTC = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                //PL 20111013
                bool isToday = (dtSysUTC.Date == pDate.Date.Date);
                bool isYesterday = (dtSysUTC.AddDays(-1).Date == pDate.Date.Date);

                if (isToday)
                    addMsg = Cst.HTMLBold + Ressource.GetString("Msg_Today") + Cst.HTMLEndBold + " ";
                else if (isYesterday)
                    addMsg = Cst.HTMLBold + Ressource.GetString("Msg_Yesterday") + Cst.HTMLEndBold + " ";
            }
            return Ressource.GetString2(pRessource, addMsg + fmtDate, pCollaborator);
        }
    }
}

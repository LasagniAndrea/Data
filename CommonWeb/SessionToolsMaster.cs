using System;
using System.Web;
using System.Web.SessionState;

//
using EFS.Actor;
using EFS.ACommon;

namespace EFS.Common.Web
{

    /// <summary>
    /// Donne accès à l'état de session
    /// <para>L'etat de session est accessible depuis HttpContext.Current (contexte requête Http)</para>
    /// <para>L'etat de session est accessible depuis HttpApplication (Global.asax)</para>
    /// </summary>
    public sealed class HttpSessionStateTools
    {
        /// <summary>
        /// Obtient true si le session est accessible
        /// </summary>
        /// <param name="pSessionState"></param>
        /// <returns></returns>
        public static bool IsSessionAvailable(HttpSessionState pSessionState)
        {
            return (null != pSessionState);
        }
        /// <summary>
        /// Stocke l'object {pValue} dans Session
        /// </summary>
        /// <param name="pSessionState"></param>
        /// <param name="pkey">clef d'accès à l'objet</param>
        /// <param name="pValue">Représente l'objet</param>
        public static void Set(HttpSessionState pSessionState, string pkey, object pValue)
        {
            if (IsSessionAvailable(pSessionState))
                pSessionState[pkey] = pValue;
        }
        /// <summary>
        /// Retourne un object stocké dans Session
        /// <para>Retourne null si la session n'est plus disponible</para>
        /// </summary>
        /// <param name="pSessionState"></param>
        /// <param name="pkey"></param>
        /// <returns></returns>
        public static object Get(HttpSessionState pSessionState, string pkey)
        {
            object ret = null;
            if (IsSessionAvailable(pSessionState))
                ret = pSessionState[pkey];
            return ret;
        }
        /// <summary>
        /// Retourne les 10 premiers caractères du SESSIONID
        /// <para>retourne null si la session n'est pas disponible</para>
        /// </summary>
        /// <param name="pSessionState"></param>
        /// <returns></returns>
        public static string ShortSessionId(HttpSessionState pSessionState)
        {
            string ret = null;
            if (IsSessionAvailable(pSessionState))
                ret = pSessionState.SessionID.Substring(0, 10);
            return ret;
        }
        /// <summary>
        /// L'authentification est de type Shibboleth et la session est active
        /// </summary>
        /// <param name="pSessionState"></param>
        /// <returns></returns>
        /// EG 20220623 [XXXXX] New
        public static bool IsShibbolethAuthenticationType(HttpSessionState pSessionState)
        {
            return IsSessionAvailable(pSessionState) && (HttpContext.Current.User.Identity.AuthenticationType == "Shibboleth");
        }
    }
    
}

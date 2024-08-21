using System;
using System.Collections;
using System.Collections.Generic;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{
    /// <summary>
    ///  Collection de données en cache dans la session web courante
    ///  <para>Les données sont stockées dans une variable SESSION généralement pour réduire l'accès à la base de donné</para>
    ///  <para>Une purge automatique est effectuée lorsque les données ne sont plus utilisées (voir constante timeout)</para>
    /// </summary>
    /// FI 20200225 [XXXXX] Add Class
    public class DataCache
    {
        public const string Key = "DATACACHE";

        /// <summary>
        /// timeout. 
        /// <para>
        /// Si la donnée n'est pas utilisé au delà de 2 heures. elle est supprimée pour libérer la mémoire.
        /// </para>
        /// </summary>
        public const int timeout = 2 * 60 * 60; //2 heures en secondes

        /// <summary>
        /// Collection qui contient les données en cache dans le session web courante
        /// <para>Chaque valeur est constituée d'une pair</para>
        /// <para>First  : Date de dernière utilisation de la donnée mise en cache</para>
        /// <para>Second : La donnée elle même</para>
        /// </summary>
        private static Dictionary<string, Pair<DateTime, Object>> Data
        {
            get
            {
                Dictionary<string, Pair<DateTime, Object>> ret;
                ret = HttpSessionStateTools.Get(SessionTools.SessionState, Key) as Dictionary<string, Pair<DateTime, Object>>;
                return ret;
            }
            set
            {
                HttpSessionStateTools.Set(SessionTools.SessionState, Key, value);
            }
        }

        /// <summary>
        /// Retourne true s'il existe la donnée en cache pour la clé {pKey}
        /// </summary>
        /// <returns></returns>
        public static Boolean ContainsKey(string pKey)
        {
            Boolean ret = false;
            if ((null != Data) && Data.ContainsKey(pKey))
                ret = true;
            return ret;
        }

        /// <summary>
        /// Retourne la donnée en cache. 
        /// <para>Si la donnée n'est pas en cache, retourne la valeur par défaut du type générique T</para>
        /// </summary>
        /// <param name="pKey">Clé d'accès à la donnée</param>
        /// <returns></returns>
        public static T GetData<T>(String pKey)
        {
            Pair<DateTime, Object> ret = new Pair<DateTime, Object>()
            {
                Second = default(T)
            };

            if (ContainsKey(pKey))
            {
                ret = Data[pKey] as Pair<DateTime, Object>;
                ret.First = DateTime.Now; //date heure de la dernière lecture
            }
            return (T)(ret.Second);
        }

        /// <summary>
        /// Ajoute la donnée dans le cache
        /// </summary>
        /// <param name="pKey">Représente la clé d'accès</param>
        /// <param name="pData">Représente la donnée</param>
        public static void SetData<T>(String pKey, T pData)
        {
            if (null == Data)
                Data = new Dictionary<string, Pair<DateTime, Object>>();

            if (false == Data.ContainsKey(pKey))
                NewData(pKey, pData);
            else
                (Data[pKey] as Pair<DateTime, Object>).Second = pData;
        }

        /// <summary>
        /// Ajout d'une nouvelle entrée dans le cache
        /// </summary>
        /// <param name="pKey">Représente la clé d'accès</param>
        /// <param name="pData">Repésente le donnée</param>
        /// <returns></returns>
        private static void NewData<T>(String pKey, T pData)
        {
            lock (((ICollection)Data).SyncRoot)
            {
                RemoveOutOfDate();
                Data[pKey] = new Pair<DateTime, Object>(DateTime.Now, pData);
            }
        }

        /// <summary>
        /// Suppression des données non utilisées depuis un cetains temps (prise en compte du timeout)
        /// <para>par Exemple suite à fermeture de page web</para>
        /// </summary>
        public static void RemoveOutOfDate()
        {
            if (null != Data && Data.Count > 0)
            {
                // Get the keys collection, and copy to an array.
                String[] keys = new String[Data.Count];

                // Copy.
                Data.Keys.CopyTo(keys, 0);

                for (int index = keys.Length - 1; index >= 0; --index)
                {
                    Pair<DateTime, Object> val = Data[keys[index]];
                    if ((null != val) && ((DateTime.Now - val.First).TotalSeconds) > (timeout))
                    {
                        // Remove the item.
                        Data.Remove(keys[index]);
                    }
                }
            }
        }
        /// <summary>
        /// Supprime toutes les clés et les valeurs présentes dans le cache
        /// </summary>
        public static int Clear()
        {
            int ret = 0;
            if (null != Data && Data.Count > 0)
            {
                ret = Data.Count;
                Data.Clear();
            }
            return ret;
        }
    }
}

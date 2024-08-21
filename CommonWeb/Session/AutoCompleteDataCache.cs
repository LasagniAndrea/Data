using System;
using System.Collections;
using System.Collections.Generic;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{
    /// <summary>
    ///  Collection de jeux de données autocomplete en cache dans la session web courante
    ///  <para>Les jeux de données sont stockés au fil de l'eau en mémoire dans une variable SESSION pour réduire l'accès à la base de donnée</para>
    ///  <para>Une purge automatique est effectuée lorsque les données ne sont plus utilisées (voir constante timeout)</para>
    /// </summary>
    /// FI 20191227 [XXXXX] Add
    public class AutoCompleteDataCache
    {
        public const string Key = "AUTOCOMPLETEDATACACHE";

        /// <summary>
        /// timeout. 
        /// <para>
        /// Si le jeu de données autocomplete n'est pas utilisé au delà de 3 minutes. Il est supprimé pour libérer la mémoire.
        /// </para>
        /// </summary>
        private const int timeout = 180;

        /// <summary>
        /// Collection qui contient les jeux de données autocomplete 
        /// <para>Chaque valeur est constituée d'une pair</para>
        /// <para>First  : Date de dernière utilisation du jeu de données autocomplete</para>
        /// <para>Second : jeux de données autocomplete</para>
        /// </summary>
        private static Dictionary<AutoCompleteKey, Pair<DateTime, IEnumerable>> Data
        {
            get
            {
                Dictionary<AutoCompleteKey, Pair<DateTime, IEnumerable>> ret;
                ret = HttpSessionStateTools.Get(SessionTools.SessionState, Key) as Dictionary<AutoCompleteKey, Pair<DateTime, IEnumerable>>;
                return ret;
            }
            set
            {
                HttpSessionStateTools.Set(SessionTools.SessionState, Key, value);
            }
        }

        /// <summary>
        /// Retourne true s'il existe déjà un jeu de données autocomplete pour la clé {pKey}
        /// </summary>
        /// <param name="pKey">Clé d'accès au jeux de  données autocomplete</param>
        /// <returns></returns>
        public static Boolean ContainsKey(AutoCompleteKey pKey)
        {
            Boolean ret = false;
            if ((null != Data) && Data.ContainsKey(pKey))
                ret = true;
            return ret;
        }

        /// <summary>
        /// Retourne le jeux de données autocomplete
        /// </summary>
        /// <returns></returns>
        public static T GetData<T>(AutoCompleteKey pKey) where T : IEnumerable
        {
            Pair<DateTime, IEnumerable> ret = new Pair<DateTime, IEnumerable>();

            if (ContainsKey(pKey))
            {
                ret = Data[pKey] as Pair<DateTime, IEnumerable>;
                ret.First = DateTime.Now; //date heure de la dernière lecture
            }
            return (T)(ret.Second);
        }

        /// <summary>
        /// Ajoute un jeux de données autocomplete
        /// </summary>
        /// <param name="pKey">Représente la clé d'accès</param>
        /// <param name="pEnumerable">Jeu de données autocomplete</param>
        public static void SetData<T>(AutoCompleteKey pKey, T pEnumerable) where T : IEnumerable
        {

            if (null == Data)
                Data = new Dictionary<AutoCompleteKey, Pair<DateTime, IEnumerable>>(new AutoCompleteKeyComparer());

            if (false == Data.ContainsKey(pKey))
                NewData(pKey, pEnumerable);
            else
                (Data[pKey] as Pair<DateTime, IEnumerable>).Second = pEnumerable;
        }

        /// <summary>
        /// Ajout d'une nouvelle entrée dans le cache
        /// </summary>
        /// <param name="pKey">Représente la clé d'accès</param>
        /// <param name="pEnumerable">Jeu de données autocomplete</param>
        /// <returns></returns>
        private static void NewData<T>(AutoCompleteKey pKey, T pEnumerable) where T : IEnumerable
        {
            lock (((ICollection)Data).SyncRoot)
            {
                RemoveOutOfDate();
                Data[pKey] = new Pair<DateTime, IEnumerable>(DateTime.Now, pEnumerable);
            }
        }
        /// <summary>
        /// Supprime une entrée dans le cache 
        /// </summary>
        /// <param name="pKey"></param>
        public static void RemoveData(AutoCompleteKey pKey)
        {
            if (null != Data && Data.Count > 0)
            {
                lock (((ICollection)Data).SyncRoot)
                {
                    Data.Remove(pKey);
                }
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
                AutoCompleteKey[] keys = new AutoCompleteKey[Data.Count];

                // Copy.
                Data.Keys.CopyTo(keys, 0);

                for (int index = keys.Length - 1; index >= 0; --index)
                {
                    Pair<DateTime, IEnumerable> val = Data[keys[index]];
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

    /// <summary>
    ///  Clé d'accès aux jeu de données autocomplete
    /// </summary>
    /// FI 20191226 [XXXXX] Add
    public class AutoCompleteKey
    {
        /// <summary>
        /// __GUID unique d'une page Web (A renseigné si les jeux de données sont spécifique à chaque page)
        /// </summary>
        public string pageGuId = string.Empty;
        /// <summary>
        ///  Contrôle Web Id du contôle sur lequel il existe des données autocomplete
        /// </summary>
        public string controlId = string.Empty;
        /// <summary>
        ///  Autre élément pouvant rentrer dans la clé
        /// </summary>
        public string additionnalKey = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public AutoCompleteKey()
        {
            pageGuId = string.Empty;
            controlId = string.Empty;
            additionnalKey = string.Empty;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class AutoCompleteKeyComparer : IEqualityComparer<AutoCompleteKey>
    {
        /// <summary>
        /// Check the equality of two keys
        /// </summary>
        /// <param name="x">first key to be compared</param>
        /// <param name="y">second key  to be compared</param>
        /// <returns>true when the provided log keys are equal</returns>
        public bool Equals(AutoCompleteKey x, AutoCompleteKey y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return
                x.pageGuId.Equals(y.pageGuId) &&
                x.controlId.Equals(y.controlId) &&
                x.additionnalKey.Equals(y.additionnalKey);
        }

        /// <summary>
        /// Get the hashing code of the input  key
        /// </summary>
        /// <param name="obj">input log key we want ot compute the hashing code</param>
        /// <returns></returns>
        public int GetHashCode(AutoCompleteKey obj)
        {
            if (obj is null) return 0;

            int hashA = obj.pageGuId.GetHashCode();
            int hashB = obj.controlId.GetHashCode();
            int hashC = obj.additionnalKey.GetHashCode();

            return hashA ^ hashB ^ hashC;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;

// EG 20231130 [WI756] Spheres Core : Refactoring Code Analysis
namespace EFS.ApplicationBlocks.Data.Extension
{
    /// <summary>
    /// Fournit un IEqualityComparer Delegate
    /// </summary>
    /// <typeparam name="T">Type de l'objet à comparer</typeparam>
    public class DelegateComparer<T> : IEqualityComparer<T>
    {
        #region members
        private readonly Func<T, T, bool> m_Equals;
        private readonly Func<T, int> m_HashCode;
        #endregion
        #region methods
        /// <summary>
        /// Implémentation du Comparer Delegate
        /// </summary>
        /// <param name="equals">Méthode testant l'égalité</param>
        /// <param name="hashCode">Méthode fournissant le HashCode</param>
        public DelegateComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            m_Equals = equals;
            m_HashCode = hashCode;
        }
        /// <summary>
        /// Méthode testant l'égalité entre un objet x et un objet y de même type
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(T x, T y)
        {
            return m_Equals(x, y);
        }
        /// <summary>
        /// Méthode fournissant le HashCode d'un objet
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(T obj)
        {
            if (m_HashCode != null)
            {
                return m_HashCode(obj);
            }
            else
            {
                return obj.GetHashCode();
            }
        }
        #endregion
    }

    /// <summary>
    /// Extension de la méthode Distinct de l'interface IEnumerable
    /// </summary>
    public static class ComparerExtension
    {
        /// <summary>
        /// Extension de la méthode Distinct de l'interface IEnumerable
        /// </summary>
        /// <typeparam name="T">Type des objets</typeparam>
        /// <param name="items">Collection d'objets</param>
        /// <param name="equals">Delegate de test d'égalité</param>
        /// <param name="hashCode">Delegate d'obtention du HashCode</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items,
            Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            return items.Distinct(new DelegateComparer<T>(equals, hashCode));
        }
        /// <summary>
        /// Extension de la méthode Distinct de l'interface IEnumerable
        /// </summary>
        /// <typeparam name="T">Type des objets</typeparam>
        /// <param name="items">Collection d'objets</param>
        /// <param name="equals">Delegate de test d'égalité</param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> items,
            Func<T, T, bool> equals)
        {
            return items.Distinct(new DelegateComparer<T>(equals, null));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace EFS.Common
{
    // EG 20180205 [23769] New
    public static class ListExtensionsTools
    {
        /// <summary>
        /// Tronçonage d'une liste source en liste de sous-listes
        /// </summary>
        /// <typeparam name="T">type de la source</typeparam>
        /// <param name="source">source</param>
        /// <param name="chunkSize">taille du nombre d'éléments maximal pour constituer une sous liste</param>
        /// <returns></returns>
        // EG 20180205 [23769] New
        public static List<List<T>> ChunkBy<T>(this List<T> pSource, int pHeapSize)
        {
            return pSource
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / pHeapSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
    /// <summary>
    /// Check if object is an array of a certain type
    /// </summary>
    // EG 20190205 New
    public static class TypeExtensions
    {
        /// <summary>
        /// Retourne true si les élements d'un Array T[] sont du type pType
        /// </summary>
        /// <param name="pType">Type de comparaison</param>
        /// <returns></returns>
        // EG 20190114 New 
        public static bool IsArrayOf<T>(this Type pType)
        {
            return pType == typeof(T[]);
        }
        /// <summary>
        /// Transforme une énumération d'Enum en flags d'enum (Enum1|Enum2|...)
        /// </summary>
        // EG 20190114 New 
        public static T ConvertToFlag<T>(this IEnumerable<T> flags) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new NotSupportedException("ConvertToFlag");

            return (T)(object)flags.Cast<int>().Aggregate(0, (c, n) => c |= n);
        }
    } 
}

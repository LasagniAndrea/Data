using System;
using System.Collections.Generic;

namespace EFS.Common
{

    /// <summary>
    /// Class represeting a couple 
    /// </summary>
    /// <typeparam name="T1">type of the first element of the couple</typeparam>
    /// <typeparam name="T2">type of the second element of the couple</typeparam>
    ///<remarks>The equality comparer is implemented comparing references (shallow comparison) of the elements composing the couple, 
    ///do not compare any Pair object that does not use atomic type</remarks>
    [System.SerializableAttribute()]
    public class Pair<T1, T2>
    {

        T1 m_First;

        /// <summary>
        /// Get set the first element of the couple
        /// </summary>
        public T1 First
        {
            get { return m_First; }
            set { m_First = value; }
        }

        T2 m_Second;

        /// <summary>
        /// Get set the second element of the couple
        /// </summary>
        public T2 Second
        {
            get { return m_Second; }
            set { m_Second = value; }
        }

        /// <summary>
        /// Return an empty paire
        /// </summary>
        public Pair()
        { }

        /// <summary>
        /// Return a well built pair
        /// </summary>
        /// <param name="pFirst">firs pair element</param>
        /// <param name="pSecond">second pair element</param>
        public Pair(T1 pFirst, T2 pSecond)
        {
            m_First = pFirst;

            m_Second = pSecond;
        }

    }

    /// <summary>
    /// IEqualityComparer implementation for the PosKeepingKey class
    /// </summary>
    [System.SerializableAttribute()]
    public class PairComparer<T1, T2> : IEqualityComparer<Pair<T1, T2>>
    {

        #region IEqualityComparer<Pair<T1,T2>> Membres

        /// <summary>
        /// Check the equality of two pair objects
        /// </summary>
        /// <param name="x">first pair to to be compared</param>
        /// <param name="y">second pair to be compared</param>
        /// <returns>true when the provided pairs are equals</returns>
        public bool Equals(Pair<T1, T2> x, Pair<T1, T2> y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null)
                return false;

            return
                x.First.Equals(y.First) &&
                x.Second.Equals(y.Second);
        }

        /// <summary>
        /// Get the hashing code of the input key
        /// </summary>
        /// <param name="obj">input pair object we want to compute the hashing code</param>
        /// <returns>the hashing code of the provided pair</returns>
        public int GetHashCode(Pair<T1, T2> obj)
        {
            if (obj is null) return 0;

            int hashFirst = obj.First.GetHashCode();
            int hashSecond = obj.Second.GetHashCode();

            return hashFirst ^ hashSecond;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;

namespace EFS.Common
{
    /// <summary>
    /// Generic comparer class allowing use of lambda expression in order to compare two collections
    /// </summary>
    /// <remarks>
    /// Do not use with dictionary, use for Distinct and Except LINQ methods
    /// </remarks>
    /// <typeparam name="T">the type we want to compare</typeparam>
    public class SpheresLambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> m_lambdaComparer;

        private readonly Func<T, int> m_lambdaHash;

        /// <summary>
        /// Build a new lambda comparer
        /// </summary>
        /// <remarks>hash expression is always false, anything => 0</remarks>
        /// <param name="pLambdaExpression">the lambda expression used to compare two instances of the type T
        /// <example>
        /// new SpheresLambdaComparer<MyObject>((x, y) => x.Id == y.Id))
        /// </example>
        /// </param>
        public SpheresLambdaComparer(Func<T, T, bool> pLambdaExpression) :
            this(pLambdaExpression, anything => 0)
        {
        }

        private SpheresLambdaComparer(Func<T, T, bool> pLambdaExpression, Func<T, int> pLambdaHash)
        {
            m_lambdaComparer = pLambdaExpression ?? throw new ArgumentNullException("lambdaComparer expression is null");
            m_lambdaHash = pLambdaHash ?? throw new ArgumentNullException("lambdaHash expression is null");
        }

        /// <summary>
        /// Check the equality of the input parameters, using the instance lambda expression
        /// </summary>
        /// <param name="x">first parameter ro compare</param>
        /// <param name="y">second parameter ro compare</param>
        /// <returns></returns>
        public bool Equals(T x, T y)
        {
            return m_lambdaComparer(x, y);
        }

        /// <summary>
        /// Get the hash code, returns always 0
        /// </summary>
        /// <param name="obj">input object you want to calculate the hash code</param>
        /// <returns></returns>
        public int GetHashCode(T obj)
        {
            return m_lambdaHash(obj);
        }
    }
}

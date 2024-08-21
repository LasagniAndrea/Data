using System;

namespace Fonet.Fo.Expr
{
    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    internal class PropertyException : Exception
    {
        public PropertyException(string detail) : base(detail)
        {
        }
    }
}
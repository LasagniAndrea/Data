namespace Fonet.Pdf.Filter
{
    using System;

    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    public class UnsupportedFilterException : Exception
    {
        public UnsupportedFilterException(string filterName)
            : base(String.Format("The {0} filter is not supported.", filterName))
        {
        }
    }
}
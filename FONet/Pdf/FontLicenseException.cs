using System;

namespace Fonet.Pdf
{
    /// <summary>
    ///     Thrown during creation of PDF font object if the font's license
    ///     is violated, e.g. attempting to subset a font that does not permit 
    ///     subsetting.
    /// </summary>
    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    public class FontLicenseException : Exception
    {
        public FontLicenseException(string message)
            : base(message)
        {
        }
    }
}
namespace Fonet.Image
{
    using System;

    // EG 20180425 Analyse du code Correction [CA2237]
    [Serializable]
    internal class FonetImageException : Exception
    {
        public FonetImageException()
        {
        }

        public FonetImageException(string message) : base(message)
        {
        }
    }
}